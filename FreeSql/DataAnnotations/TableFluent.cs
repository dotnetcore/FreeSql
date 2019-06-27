using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace FreeSql.DataAnnotations
{
    public class TableFluent
    {

        public TableFluent(Type entityType, TableAttribute table)
        {
            _entityType = entityType;
            _properties = _entityType.GetProperties().ToDictionary(a => a.Name, a => a, StringComparer.CurrentCultureIgnoreCase);
            _table = table;
        }

        Type _entityType;
        Dictionary<string, PropertyInfo> _properties;
        TableAttribute _table;
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
        /// 查询过滤SQL，实现类似 a.IsDeleted = 1 功能
        /// </summary>
        public TableFluent SelectFilter(string value)
        {
            _table.SelectFilter = value;
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
    }

    public class TableFluent<T>
    {

        public TableFluent(TableAttribute table)
        {
            _table = table;
        }

        TableAttribute _table;
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
        /// 查询过滤SQL，实现类似 a.IsDeleted = 1 功能
        /// </summary>
        public TableFluent<T> SelectFilter(string value)
        {
            _table.SelectFilter = value;
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
            var col = _table._columns.GetOrAdd(proto.Name, name => new ColumnAttribute { Name = proto.Name });
            return new ColumnFluent(col);
        }
    }
}
