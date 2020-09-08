using System;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using FreeSql.DataAnnotations;
using FreeSql.Internal;

namespace FreeSql
{
    partial class FreeSqlBuilder
    {
        
        /// <summary>
        /// 转小写同步结构
        /// </summary>
        /// <param name="value">true:转小写, false:不转</param>
        /// <returns></returns>
        [Obsolete("请使用 UseNameConvert(NameConvertType.ToLower)，或者 fsql.CodeFirst.IsSyncStructureToLower = value")]
        public FreeSqlBuilder UseSyncStructureToLower(bool value)
        {
            _isSyncStructureToLower = value;
            return this;
        }
        /// <summary>
        /// 转大写同步结构
        /// </summary>
        /// <param name="value">true:转大写, false:不转</param>
        /// <returns></returns>
        [Obsolete("请使用 UseNameConvert(NameConvertType.ToUpper)，或者 fsql.CodeFirst.IsSyncStructureToUpper = value")]
        public FreeSqlBuilder UseSyncStructureToUpper(bool value)
        {
            _isSyncStructureToUpper = value;
            return this;
        }

        /// <summary>
        /// 自动转换实体属性名称 Entity Property -> Db Filed
        /// <para></para>
        /// *不会覆盖 [Column] 特性设置的Name
        /// </summary>
        /// <param name="convertType"></param>
        /// <returns></returns>
        [Obsolete("请使用 UseNameConvert 功能")]
        public FreeSqlBuilder UseEntityPropertyNameConvert(StringConvertType convertType)
        {
            _entityPropertyConvertType = convertType;
            return this;
        }

        void EntityPropertyNameConvert(IFreeSql fsql)
        {
            //添加实体属性名全局AOP转换处理
            if (_entityPropertyConvertType != StringConvertType.None)
            {
                string PascalCaseToUnderScore(string str) => string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString()));

                switch (_entityPropertyConvertType)
                {
                    case StringConvertType.Lower:
                        fsql.Aop.ConfigEntityProperty += (_, e) => e.ModifyResult.Name = e.Property.Name.ToLower();
                        break;
                    case StringConvertType.Upper:
                        fsql.Aop.ConfigEntityProperty += (_, e) => e.ModifyResult.Name = e.Property.Name.ToUpper();
                        break;
                    case StringConvertType.PascalCaseToUnderscore:
                        fsql.Aop.ConfigEntityProperty += (_, e) => e.ModifyResult.Name = PascalCaseToUnderScore(e.Property.Name);
                        break;
                    case StringConvertType.PascalCaseToUnderscoreWithLower:
                        fsql.Aop.ConfigEntityProperty += (_, e) => e.ModifyResult.Name = PascalCaseToUnderScore(e.Property.Name).ToLower();
                        break;
                    case StringConvertType.PascalCaseToUnderscoreWithUpper:
                        fsql.Aop.ConfigEntityProperty += (_, e) => e.ModifyResult.Name = PascalCaseToUnderScore(e.Property.Name).ToUpper();
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
