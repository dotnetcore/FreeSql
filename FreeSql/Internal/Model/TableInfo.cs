using FreeSql.DataAnnotations;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FreeSql.Internal.Model
{
    public class TableInfo
    {
        public Type Type { get; set; }
        public Type TypeLazy { get; set; }
        public MethodInfo TypeLazySetOrm { get; set; }
        public Dictionary<string, PropertyInfo> Properties { get; set; } = new Dictionary<string, PropertyInfo>(StringComparer.CurrentCultureIgnoreCase);
        public Dictionary<string, ColumnInfo> Columns { get; set; } = new Dictionary<string, ColumnInfo>(StringComparer.CurrentCultureIgnoreCase);
        public Dictionary<string, ColumnInfo> ColumnsByCs { get; set; } = new Dictionary<string, ColumnInfo>(StringComparer.CurrentCultureIgnoreCase);
        public Dictionary<string, ColumnInfo> ColumnsByCsIgnore { get; set; } = new Dictionary<string, ColumnInfo>(StringComparer.CurrentCultureIgnoreCase);
        public ColumnInfo[] ColumnsByPosition { get; set; }
        public ColumnInfo[] Primarys { get; set; }
        public IndexInfo[] Indexes { get; set; }
        public string CsName { get; set; }
        public string DbName { get; set; }
        public string DbOldName { get; set; }
        public bool DisableSyncStructure { get; set; }
        public string Comment { get; internal set; }
        public bool IsRereadSql { get; internal set; }

        public ColumnInfo VersionColumn { get; set; }

        ConcurrentDictionary<string, TableRef> _refs { get; } = new ConcurrentDictionary<string, TableRef>(StringComparer.CurrentCultureIgnoreCase);

        internal void AddOrUpdateTableRef(string propertyName, TableRef tbref)
        {
            _refs.AddOrUpdate(propertyName, tbref, (ok, ov) => tbref);
        }
        public TableRef GetTableRef(string propertyName, bool isThrowException)
        {
            if (_refs.TryGetValue(propertyName, out var tryref) == false) return null;
            if (tryref.Exception != null)
            {
                if (isThrowException) throw tryref.Exception;
                return null;
            }
            return tryref;
        }

        //public void CopyTo(TableInfo target)
        //{
        //    target.Type = this.Type;
        //    target.TypeLazy = this.TypeLazy;
        //    target.TypeLazySetOrm = this.TypeLazySetOrm;
        //    foreach (var prop in this.Properties) target.Properties.Add(prop.Key, prop.Value);
        //    foreach (var col in this.Columns) target.Columns.Add(col.Key, cloneColumn(col.Value));
        //    foreach (var col in this.ColumnsByCs) target.ColumnsByCs.Add(col.Key, getOrCloneColumn(col.Value));
        //    foreach (var col in this.ColumnsByCsIgnore) target.ColumnsByCsIgnore.Add(col.Key, getOrCloneColumn(col.Value));
        //    target.ColumnsByPosition = this.ColumnsByPosition.Select(col => getOrCloneColumn(col)).ToArray();
        //    target.Primarys = this.Primarys.Select(col => getOrCloneColumn(col)).ToArray();
        //    target.Indexes = this.Indexes.Select(idx => new IndexInfo
        //    {
        //        IsUnique = idx.IsUnique,
        //        Name = idx.Name,
        //        Columns = idx.Columns.Select(col => new IndexColumnInfo
        //        {
        //            Column = getOrCloneColumn(col.Column),
        //            IsDesc = col.IsDesc
        //        }).ToArray()
        //    }).ToArray();
        //    target.CsName = this.CsName;
        //    target.DbName = this.DbName;
        //    target.DbOldName = this.DbOldName;
        //    target.DisableSyncStructure = this.DisableSyncStructure;
        //    target.Comment = this.Comment;
        //    target.IsRereadSql = this.IsRereadSql;
        //    target.VersionColumn = getOrCloneColumn(this.VersionColumn);
        //    foreach (var rf in this._refs) target._refs.TryAdd(rf.Key, new TableRef
        //    {
                 
        //    });


        //    ColumnInfo getOrCloneColumn(ColumnInfo col)
        //    {
        //        if (target.Columns.TryGetValue(col.Attribute.Name, out var trycol)) return trycol;
        //        return cloneColumn(col);
        //    }
        //    ColumnInfo cloneColumn(ColumnInfo col)
        //    {
        //        return new ColumnInfo
        //        {
        //            Table = target,
        //            CsName = col.CsName,
        //            CsType = col.CsType,
        //            Attribute = new ColumnAttribute
        //            {
        //                Name = col.Attribute.Name,
        //                OldName = col.Attribute.OldName,
        //                DbType = col.Attribute.DbType,
        //                _IsPrimary = col.Attribute._IsPrimary,
        //                _IsIdentity = col.Attribute._IsIdentity,
        //                _IsNullable = col.Attribute._IsNullable,
        //                _IsIgnore = col.Attribute._IsIgnore,
        //                _IsVersion = col.Attribute._IsVersion,
        //                MapType = col.Attribute.MapType,
        //                _Position = col.Attribute._Position,
        //                _CanInsert = col.Attribute._CanInsert,
        //                _CanUpdate = col.Attribute._CanUpdate,
        //                ServerTime = col.Attribute.ServerTime,
        //                _StringLength = col.Attribute._StringLength,
        //                InsertValueSql = col.Attribute.InsertValueSql,
        //                _Precision = col.Attribute._Precision,
        //                _Scale = col.Attribute._Scale,
        //                RewriteSql = col.Attribute.RewriteSql,
        //                RereadSql = col.Attribute.RereadSql,
        //            },
        //            Comment = col.Comment,
        //            DbTypeText = col.DbTypeText,
        //            DbDefaultValue = col.DbDefaultValue,
        //            DbInsertValue = col.DbInsertValue,
        //            DbUpdateValue = col.DbUpdateValue,
        //            DbSize = col.DbSize,
        //            DbPrecision = col.DbPrecision,
        //            DbScale = col.DbScale
        //        };
        //    }
        //}
    }

    public class TableRef
    {
        public PropertyInfo Property { get; set; }

        public TableRefType RefType { get; set; }

        public Type RefEntityType { get; set; }
        /// <summary>
        /// 中间表，多对多
        /// </summary>
        public Type RefMiddleEntityType { get; set; }

        public List<ColumnInfo> Columns { get; set; } = new List<ColumnInfo>();
        public List<ColumnInfo> MiddleColumns { get; set; } = new List<ColumnInfo>();
        public List<ColumnInfo> RefColumns { get; set; } = new List<ColumnInfo>();

        public Exception Exception { get; set; }
    }
    public enum TableRefType
    {
        OneToOne, ManyToOne, OneToMany, ManyToMany
    }
}