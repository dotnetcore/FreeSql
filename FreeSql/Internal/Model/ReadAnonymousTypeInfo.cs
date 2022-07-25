using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace FreeSql.Internal.Model
{
    public class ReadAnonymousTypeInfo
    {
        public PropertyInfo Property { get; set; }
        public FieldInfo ReflectionField { get; set; }
        public string CsName { get; set; }
        public Type CsType { get; set; }
        public Type MapType { get; set; }
        public string DbField { get; set; }
        public string DbNestedField { get; set; }
        public ConstructorInfo Consturctor { get; set; }
        public List<ReadAnonymousTypeInfo> Childs = new List<ReadAnonymousTypeInfo>();
        public TableInfo Table { get; set; }
        public bool IsEntity { get; set; }
        public bool IsDefaultCtor { get; set; }
        public string IncludeManyKey { get; set; } //ToList(a => new { a.Childs }) 集合属性指定加载
        public Expression SubSelectMany { get; set; } //ToList(a => new { sublist = fsql.Select<T>().ToList() }) 子集合查询

        public void CopyTo(ReadAnonymousTypeInfo target)
        {
            target.Property = Property;
            target.ReflectionField = ReflectionField;
            target.CsName = CsName;
            target.CsType = CsType;
            target.MapType = MapType;
            target.DbField = DbField;
            target.DbNestedField = DbNestedField;
            target.Consturctor = Consturctor;
            LocalEachCopyChilds(Childs, target.Childs);
            target.Table = Table;
            target.IsEntity = IsEntity;
            target.IsDefaultCtor = IsDefaultCtor;
            target.IncludeManyKey = IncludeManyKey;

            void LocalEachCopyChilds(List<ReadAnonymousTypeInfo> from, List<ReadAnonymousTypeInfo> to)
            {
                foreach(var fromChild in from)
                {
                    var toChild = new ReadAnonymousTypeInfo();
                    fromChild.CopyTo(toChild);
                    to.Add(toChild);
                }
            }
        }

        public List<ReadAnonymousTypeInfo> GetAllChilds(int maxDepth = 10)
        {
            if (maxDepth <= 0) return new List<ReadAnonymousTypeInfo>();
            var allchilds = new List<ReadAnonymousTypeInfo>();
            foreach (var child in Childs)
            {
                if (child.Childs.Any())
                    allchilds.AddRange(child.GetAllChilds(maxDepth - 1));
                else
                    allchilds.Add(child);
            }
            return allchilds;
        }
    }
    public class ReadAnonymousTypeAfInfo
    {
        public ReadAnonymousTypeInfo map { get; }
        public string field { get; }
        public List<NativeTuple<string, IList, int>> fillIncludeMany { get; set; } //回填集合属性的数据
        public List<NativeTuple<Expression, IList, int>> fillSubSelectMany { get; set; } //回填集合属性的数据
        public ReadAnonymousTypeAfInfo(ReadAnonymousTypeInfo map, string field)
        {
            this.map = map;
            this.field = field;
        }
    }
    public class ReadAnonymousTypeOtherInfo {
        public string field { get; }
        public ReadAnonymousTypeInfo read { get; }
        public List<object> retlist { get; }
        public ReadAnonymousTypeOtherInfo(string field, ReadAnonymousTypeInfo read, List<object> retlist)
        {
            this.field = field;
            this.read = read;
            this.retlist = retlist;
        }
    }
}
