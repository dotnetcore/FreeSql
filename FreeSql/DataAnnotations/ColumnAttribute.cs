using System;
using System.Linq;

namespace FreeSql.DataAnnotations
{
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
        /// 数据库类型，如： varchar(255)
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

        internal string[] _Uniques;
        /// <summary>
        /// 唯一键，在多个属性指定相同的标识，代表联合键；可使用逗号分割多个 UniqueKey 名。
        /// </summary>
        public string Unique
        {
            get => _Uniques == null ? null : string.Join(", ", _Uniques);
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _Uniques = null;
                    return;
                }
                var val = value?.Trim(' ', '\t', ',');
                if (string.IsNullOrEmpty(val))
                {
                    _Uniques = null;
                    return;
                }
                var arr = val.Split(',').Select(a => a.Trim(' ', '\t').Trim()).Where(a => !string.IsNullOrEmpty(a)).ToArray();
                if (arr.Any() == false)
                {
                    _Uniques = null;
                    return;
                }
                _Uniques = arr;
            }
        }
        /// <summary>
        /// 数据库默认值
        /// </summary>
        public object DbDefautValue { get; internal set; }

        /// <summary>
        /// 类型映射，比如：可将 enum 属性映射成 typeof(string)
        /// </summary>
        public Type MapType { get; set; }
    }
}
