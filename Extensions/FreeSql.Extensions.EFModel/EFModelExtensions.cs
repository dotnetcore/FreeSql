using FreeSql;
using FreeSql.Internal;
using FreeSql.Internal.CommonProvider;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using DbContext = Microsoft.EntityFrameworkCore.DbContext;

public static partial class EFModelExtensions
{
    /// <summary>
    /// EFCore ModelBuilder 与 FreeSql 打通实体特性配置（实现室）
    /// </summary>
    /// <param name="codeFirst"></param>
    /// <param name="dbContextTypes"></param>
    public static ICodeFirst ApplyConfigurationFromEFCore(this ICodeFirst codeFirst, params Type[] dbContextTypes)
    {
        var util = (codeFirst as CodeFirstProvider)._commonUtils;
        var globalFilters = typeof(GlobalFilter).GetField("_filters", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(util._orm.GlobalFilter) as ConcurrentDictionary<string, GlobalFilter.Item>;
        var globalFiltersIndex = 55100;
        string QuoteSqlName(string name) => util.QuoteSqlName(name.Replace(".", "_-_dot_-_")).Replace("_-_dot_-_", ".");
        foreach (var type in dbContextTypes)
        {
            if (type == null) throw new ArgumentNullException(nameof(dbContextTypes));
            if (!typeof(DbContext).IsAssignableFrom(type)) throw new ArgumentException($"类型 {type.FullName} 不是 DbContext");
            var dbContext = Activator.CreateInstance(type);
            if (dbContext == null) throw new InvalidOperationException($"无法创建 DbContext 实例: {type.FullName}");
            using (dbContext as IDisposable)
            {
                var dbContextModel = ((DbContext)dbContext).Model;
                var defaultSchema = dbContextModel.GetDefaultSchema();
                foreach (var entityEf in dbContextModel.GetEntityTypes())
                {
                    if (entityEf.IsOwned()) continue;
                    var queryFilter = entityEf.GetQueryFilter();
                    if (queryFilter != null)
                    {
                        var globalFilterName = $"efcore_{++globalFiltersIndex}";
                        var globalFilterItem = new GlobalFilter.Item();
                        typeof(GlobalFilter.Item).GetProperty("Id").SetValue(globalFilterItem, globalFiltersIndex);
                        typeof(GlobalFilter.Item).GetProperty("Name").SetValue(globalFilterItem, globalFilterName);
                        typeof(GlobalFilter.Item).GetProperty("Where").SetValue(globalFilterItem, queryFilter);
                        typeof(GlobalFilter.Item).GetProperty("Only").SetValue(globalFilterItem, true);
                        globalFilters.TryAdd(globalFilterName, globalFilterItem);
                    }
                    codeFirst.Entity(entityEf.ClrType, entity =>
                    {
                        var schema = entityEf.GetSchema();
                        if (string.IsNullOrWhiteSpace(schema)) schema = defaultSchema;
                        var tbname = entityEf.GetTableName() ?? entityEf.GetViewName();
                        entity.ToTable($"{(string.IsNullOrWhiteSpace(schema) ? $"{QuoteSqlName(schema)}." : "")}{QuoteSqlName(tbname)}");
                        var pk = entityEf.FindPrimaryKey();
                        if (pk != null) entity.HasKey(string.Join(",", pk.Properties.Select(a => a.PropertyInfo.Name)));
                        var props = new List<string>();
                        foreach (var propEf in entityEf.GetProperties())
                        {
                            if (propEf.PropertyInfo == null) continue;
                            props.Add(propEf.PropertyInfo.Name);
                            var prop = entity.Property(propEf.PropertyInfo.Name);
                            prop.HasColumnName(propEf.GetColumnName());
                            prop.HasColumnType(propEf.GetColumnType());
                            var isIdentity = propEf.ValueGenerated == ValueGenerated.OnAdd &&
                                propEf.IsKey() &&
                                (propEf.ClrType == typeof(int) || propEf.ClrType == typeof(long));
                            if (isIdentity)
                            {
                                foreach (var anno in propEf.GetAnnotations())
                                {
                                    if (anno.Name.EndsWith("ValueGenerationStrategy") && anno.Value != null && anno.Value.Equals(2))
                                    {
                                        isIdentity = true;
                                        break;
                                    }
                                }
                            }
                            prop.Help().IsIdentity(isIdentity);
                            if (!propEf.IsColumnNullable()) prop.IsRequired();
                            prop.HasDefaultValueSql(propEf.GetDefaultValueSql());
                            var maxLen = propEf.GetMaxLength();
                            if (maxLen != null) prop.HasMaxLength(maxLen.Value);
                            var precision = propEf.GetPrecision();
                            var scale = propEf.GetScale();
                            if (precision != null && scale != null) prop.HasPrecision(precision.Value, scale.Value);
                            else if (precision != null) prop.HasPrecision(precision.Value);
                            else if (scale != null) prop.HasPrecision(20, scale.Value);
                            if (propEf.IsConcurrencyToken) prop.IsRowVersion();
                            //var position = propEf.GetColumnOrder();
                            //if (position != null) prop.Position((short)position.Value);
                        }
                        foreach (var prop in entityEf.ClrType.GetProperties())
                        {
                            if (props.Contains(prop.Name)) continue;
                            var isIgnore = false;
                            var setMethod = prop.GetSetMethod(true); //trytb.Type.GetMethod($"set_{p.Name}");
                            var tp = codeFirst.GetDbInfo(prop.PropertyType);
                            if (setMethod == null || (tp == null && prop.PropertyType.IsValueType)) // 属性没有 set自动忽略
                                isIgnore = true;
                            if (tp == null && isIgnore == false) continue; //导航属性
                            entity.Property(prop.Name).Help().IsIgnore(true);
                        }
                        var navsEf = entityEf.GetNavigations();
                        foreach (var navEf in navsEf)
                        {
                            if (navEf.ForeignKey.DeclaringEntityType.IsOwned()) continue;
                            if (navEf.IsCollection)
                            {
                                var navFluent = entity.HasMany(navEf.Name);
                                if (navEf.Inverse != null)
                                {
                                    if (navEf.Inverse.IsCollection)
                                        navFluent.WithMany(navEf.Inverse.Name, typeof(int));
                                    else
                                        navFluent.WithOne(navEf.Inverse.Name).HasForeignKey(string.Join(",", navEf.Inverse.ForeignKey.Properties.Select(a => a.Name)));
                                }
                            }
                            else
                            {
                                var navFluent = entity.HasOne(navEf.Name);
                                if (navEf.Inverse != null)
                                {
                                    if (navEf.Inverse.IsCollection)
                                        navFluent.WithMany(navEf.Inverse.Name).HasForeignKey(string.Join(",", navEf.Inverse.ForeignKey.Properties.Select(a => a.Name)));
                                    else
                                        navFluent.WithOne(navEf.Inverse.Name, string.Join(",", navEf.Inverse.ForeignKey.Properties.Select(a => a.Name))).HasForeignKey(string.Join(",", navEf.ForeignKey.Properties.Select(a => a.Name)));
                                }
                            }
                        }
                    });
                }
            }
        }
        return codeFirst;
    }
}
