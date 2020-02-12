using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace FreeSql.DataAnnotations
{
    public class TableFluent
    {

        public TableFluent(ICodeFirst codeFirst, Type entityType, TableAttribute table)
        {
            _codeFirst = codeFirst;
            _entityType = entityType;
            _properties = _entityType.GetPropertiesDictIgnoreCase();
            _table = table;
        }

        ICodeFirst _codeFirst;
        Type _entityType;
        Dictionary<string, PropertyInfo> _properties;
        TableAttribute _table;

        public void ConfigEntity<T2>(Action<TableFluent<T2>> fluent2) => _codeFirst.ConfigEntity<T2>(fluent2);

        /// <summary>
        /// 数据库表名
        /// </summary>
        public TableFluent Name(string value)
        {
            _table.Name = value;
            return this;
        }
        /// <summary>
        /// 指定数据库旧的表名，修改实体命名时，同时设置此参数为修改之前的值，CodeFirst才可以正确修改数据库表；否则将视为【创建新表】
        /// </summary>
        public TableFluent OldName(string value)
        {
            _table.OldName = value;
            return this;
        }

        /// <summary>
        /// 禁用 CodeFirst 同步结构迁移
        /// </summary>
        public TableFluent DisableSyncStructure(bool value)
        {
            _table.DisableSyncStructure = value;
            return this;
        }

        public ColumnFluent Property(string proto)
        {
            if (_properties.ContainsKey(proto) == false) throw new KeyNotFoundException($"找不到属性名 {proto}");
            var col = _table._columns.GetOrAdd(proto, name => new ColumnAttribute { Name = proto });
            return new ColumnFluent(col);
        }

        /// <summary>
        /// 导航关系Fluent，与 NavigateAttribute 对应
        /// </summary>
        /// <param name="proto"></param>
        /// <param name="bind"></param>
        /// <param name="manyToMany">多对多关系的中间实体类型</param>
        /// <returns></returns>
        public TableFluent Navigate(string proto, string bind, Type manyToMany = null)
        {
            if (_properties.TryGetValue(proto, out var tryProto) == false) throw new KeyNotFoundException($"找不到属性名 {proto}");
            var nav = new NavigateAttribute { Bind = bind, ManyToMany = manyToMany };
            _table._navigates.AddOrUpdate(tryProto.Name, nav, (name, old) => nav);
            return this;
        }

        /// <summary>
        /// 设置实体的索引
        /// </summary>
        /// <param name="name">索引名</param>
        /// <param name="fields">索引字段，为属性名以逗号分隔，如：Create_time ASC, Title ASC</param>
        /// <param name="isUnique">是否唯一</param>
        /// <returns></returns>
        public TableFluent Index(string name, string fields, bool isUnique = false)
        {
            var idx = new IndexAttribute(name, fields, isUnique);
            _table._indexs.AddOrUpdate(name, idx, (_, __) => idx);
            return this;
        }
    }

    public class TableFluent<T>
    {

        public TableFluent(ICodeFirst codeFirst, TableAttribute table)
        {
            _codeFirst = codeFirst;
            _properties = typeof(T).GetPropertiesDictIgnoreCase();
            _table = table;
        }

        ICodeFirst _codeFirst;
        Dictionary<string, PropertyInfo> _properties;
        TableAttribute _table;

        public void ConfigEntity<T2>(Action<TableFluent<T2>> fluent2) => _codeFirst.ConfigEntity<T2>(fluent2);

        /// <summary>
        /// 数据库表名
        /// </summary>
        public TableFluent<T> Name(string value)
        {
            _table.Name = value;
            return this;
        }
        /// <summary>
        /// 指定数据库旧的表名，修改实体命名时，同时设置此参数为修改之前的值，CodeFirst才可以正确修改数据库表；否则将视为【创建新表】
        /// </summary>
        public TableFluent<T> OldName(string value)
        {
            _table.OldName = value;
            return this;
        }

        /// <summary>
        /// 禁用 CodeFirst 同步结构迁移
        /// </summary>
        public TableFluent<T> DisableSyncStructure(bool value)
        {
            _table.DisableSyncStructure = value;
            return this;
        }

        public ColumnFluent Property<TProto>(Expression<Func<T, TProto>> column)
        {
            var proto = (column.Body as MemberExpression)?.Member;
            if (proto == null) throw new FormatException($"错误的表达式格式 {column}");
            return Property(proto.Name);
        }
        public ColumnFluent Property(string proto)
        {
            if (_properties.TryGetValue(proto, out var tryProto) == false) throw new KeyNotFoundException($"找不到属性名 {proto}");
            var col = _table._columns.GetOrAdd(tryProto.Name, name => new ColumnAttribute { Name = proto });
            return new ColumnFluent(col);
        }

        /// <summary>
        /// 导航关系Fluent，与 NavigateAttribute 对应
        /// </summary>
        /// <typeparam name="TProto"></typeparam>
        /// <param name="proto"></param>
        /// <param name="bind"></param>
        /// <param name="manyToMany">多对多关系的中间实体类型</param>
        /// <returns></returns>
        public TableFluent<T> Navigate<TProto>(Expression<Func<T, TProto>> proto, string bind, Type manyToMany = null)
        {
            var member = (proto.Body as MemberExpression)?.Member;
            if (member == null) throw new FormatException($"错误的表达式格式 {proto}");
            return Navigate(member.Name, bind, manyToMany);
        }
        public TableFluent<T> Navigate(string proto, string bind, Type manyToMany = null)
        {
            if (_properties.TryGetValue(proto, out var tryProto) == false) throw new KeyNotFoundException($"找不到属性名 {proto}");
            var nav = new NavigateAttribute { Bind = bind, ManyToMany = manyToMany };
            _table._navigates.AddOrUpdate(tryProto.Name, nav, (name, old) => nav);
            return this;
        }

        /// <summary>
        /// 设置实体的索引
        /// </summary>
        /// <param name="name">索引名</param>
        /// <param name="fields">索引字段，为属性名以逗号分隔，如：Create_time ASC, Title ASC</param>
        /// <param name="isUnique">是否唯一</param>
        /// <returns></returns>
        public TableFluent<T> Index(string name, string fields, bool isUnique = false)
        {
            var idx = new IndexAttribute(name, fields, isUnique);
            _table._indexs.AddOrUpdate(name, idx, (_, __) => idx);
            return this;
        }
        public TableFluent<T> IndexRemove(string name)
        {
            _table._indexs.TryRemove(name, out var oldidx);
            return this;
        }
    }
}
