using System;
using System.Collections;
using System.Collections.Generic;
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
        public ConstructorInfo Consturctor { get; set; }
        public List<ReadAnonymousTypeInfo> Childs = new List<ReadAnonymousTypeInfo>();
        public TableInfo Table { get; set; }
        public bool IsEntity { get; set; }
        public bool IsDefaultCtor { get; set; }
        public string IncludeManyKey { get; set; } //ToList(a => new { a.Childs }) 集合属性指定加载

        public void CopyTo(ReadAnonymousTypeInfo target)
        {
            target.Property = Property;
            target.ReflectionField = ReflectionField;
            target.CsName = CsName;
            target.CsType = CsType;
            target.MapType = MapType;
            target.DbField = DbField;
            target.Consturctor = Consturctor;
            target.Childs = Childs;
            target.Table = Table;
            target.IsEntity = IsEntity;
            target.IsDefaultCtor = IsDefaultCtor;
            target.IncludeManyKey = IncludeManyKey;
        }
    }
    public class ReadAnonymousTypeAfInfo
    {
        public ReadAnonymousTypeInfo map { get; }
        public string field { get; }
        public List<NativeTuple<string, IList, int>> fillIncludeMany { get; set; } //回填集合属性的数据
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
