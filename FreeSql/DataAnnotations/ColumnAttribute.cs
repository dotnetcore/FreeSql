using System;
using System.Linq;

namespace FreeSql.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {

        /// <summary>
        /// 数据库列名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 指定数据库旧的列名，修改实体属性命名时，同时设置此参数为修改之前的值，CodeFirst才可以正确修改数据库字段；否则将视为【新增字段】
        /// </summary>
        public string OldName { get; set; }
        /// <summary>
        /// 数据库类型，如： varchar(255) <para></para>
        /// 字符串长度，可使用特性 [MaxLength(255)]
        /// </summary>
        public string DbType { get; set; }

        internal bool? _IsPrimary, _IsIdentity, _IsNullable, _IsIgnore, _IsVersion;
        /// <summary>
        /// 主键
        /// </summary>
        public bool IsPrimary { get => _IsPrimary ?? false; set => _IsPrimary = value; }
        /// <summary>
        /// 自增标识
        /// </summary>
        public bool IsIdentity { get => _IsIdentity ?? false; set => _IsIdentity = value; }
        /// <summary>
        /// 是否可DBNull
        /// </summary>
        public bool IsNullable { get => _IsNullable ?? false; set => _IsNullable = value; }
        /// <summary>
        /// 忽略此列，不迁移、不插入
        /// </summary>
        public bool IsIgnore { get => _IsIgnore ?? false; set => _IsIgnore = value; }
        /// <summary>
        /// 设置行锁（乐观锁）版本号，每次更新累加版本号，若更新整个实体时会附带当前的版本号判断（修改失败时抛出异常）
        /// </summary>
        public bool IsVersion { get => _IsVersion ?? false; set => _IsVersion = value; }

        /// <summary>
        /// 类型映射，除了可做基本的类型映射外，特别介绍的功能：<para></para>
        /// 1、将 enum 属性映射成 typeof(string)<para></para>
        /// 2、将 对象 属性映射成 typeof(string)，请安装扩展包 FreeSql.Extensions.JsonMap
        /// </summary>
        public Type MapType { get; set; }

        internal short? _Position;
        /// <summary>
        /// 创建表时字段的位置（场景：实体继承后设置字段顺序），规则如下：
        /// <para></para>
        /// &gt;0时排前面，1,2,3...
        /// <para></para>
        /// =0时排中间(默认)
        /// <para></para>
        /// &lt;0时排后面，...-3,-2,-1
        /// </summary>
        public short Position { get => _Position ?? 0; set => _Position = value; }

        internal bool? _CanInsert, _CanUpdate;
        /// <summary>
        /// 该字段是否可以插入，默认值true，指定为false插入时该字段会被忽略
        /// </summary>
        public bool CanInsert { get => _CanInsert ?? true; set => _CanInsert = value; }
        /// <summary>
        /// 该字段是否可以更新，默认值true，指定为false更新时该字段会被忽略
        /// </summary>
        public bool CanUpdate { get => _CanUpdate ?? true; set => _CanUpdate = value; }

        /// <summary>
        /// 标记属性为数据库服务器时间(utc/local)，在插入的时候使用类似 getdate() 执行
        /// </summary>
        public DateTimeKind ServerTime { get; set; }

        internal int? _StringLength;
        /// <summary>
        /// 设置长度，针对 string/byte[] 类型避免 DbType 的繁琐设置<para></para>
        /// 提示：也可以使用 [MaxLength(100)]<para></para>
        /// ---<para></para>
        /// StringLength = 100 时，对应 DbType：<para></para>
        /// MySql -> varchar(100)<para></para>
        /// SqlServer -> nvarchar(100)<para></para>
        /// PostgreSQL -> varchar(100)<para></para>
        /// Oracle -> nvarchar2(100)<para></para>
        /// Sqlite -> nvarchar(100)<para></para>
        /// ---<para></para>
        /// StringLength &lt; 0 时，对应 DbType：<para></para>
        /// MySql -> text (StringLength = -2 时，对应 longtext)<para></para>
        /// SqlServer -> nvarchar(max)<para></para>
        /// PostgreSQL -> text<para></para>
        /// Oracle -> nclob<para></para>
        /// Sqlite -> text<para></para>
        /// v1.6.0+ byte[] 支持设置 StringLength
        /// </summary>
        public int StringLength { get => _StringLength ?? 0; set => _StringLength = value; }

        /// <summary>
        /// 执行 Insert 方法时使用此值<para></para>
        /// 注意：如果是 getdate() 这种请可考虑使用 ServerTime，因为它对数据库间作了适配
        /// </summary>
        public string InsertValueSql { get; set; }

        internal int? _Precision;
        /// <summary>
        /// decimal/numeric 类型的长度
        /// </summary>
        public int Precision { get => _Precision ?? 0; set => _Precision = value; }
        internal int? _Scale;
        /// <summary>
        /// decimal/numeric 类型的小数位长度
        /// </summary>
        public int Scale { get => _Scale ?? 0; set => _Scale = value; }

        /// <summary>
        /// 重写功能<para></para>
        /// 比如：[Column(RewriteSql = &quot;geography::STGeomFromText({0},4236)&quot;)]<para></para>
        /// 插入：INSERT INTO [table]([geo]) VALUES(geography::STGeomFromText('...',4236))<para></para>
        /// 提示：更新也生效
        /// </summary>
        public string RewriteSql { get; set; }
        /// <summary>
        /// 重读功能<para></para>
        /// 比如：[Column(RereadSql = &quot;{0}.STAsText()&quot;)]<para></para>
        /// 查询：SELECT a.[id], a.[geo].STAsText() FROM [table] a
        /// </summary>
        public string RereadSql { get; set; }
    }
}
