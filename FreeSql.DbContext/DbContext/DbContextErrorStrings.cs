using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeSql
{
    public static class DbContextErrorStrings
    {
        public static string Language = "en";
        /// <summary>
        /// AddFreeDbContext 发生错误，请检查 {dbContextTypeName} 的构造参数都已正确注入
        /// </summary>
        public static string AddFreeDbContextError_CheckConstruction(object dbContextTypeName) => string.Format(Language == "cn" ?
            @"AddFreeDbContext 发生错误，请检查 {0} 的构造参数都已正确注入" :
            @"FreeSql: An error occurred in AddFreeDbContext, check that the construction parameters of {0} have been injected correctly", dbContextTypeName);
        /// <summary>
        /// 不可添加，已存在于状态管理：{entityString}
        /// </summary>
        public static string CannotAdd_AlreadyExistsInStateManagement(object entityString) => string.Format(Language == "cn" ?
            @"不可添加，已存在于状态管理：{0}" :
            @"FreeSql: Not addable, already exists in state management: {0}", entityString);
        /// <summary>
        /// 不可添加，实体没有主键：{entityString}
        /// </summary>
        public static string CannotAdd_EntityHasNo_PrimaryKey(object entityString) => string.Format(Language == "cn" ?
            @"不可添加，实体没有主键：{0}" :
            @"FreeSql: Not addable, entity has no primary key: {0}", entityString);
        /// <summary>
        /// 不可添加，未设置主键的值：{entityString}
        /// </summary>
        public static string CannotAdd_PrimaryKey_NotSet(object entityString) => string.Format(Language == "cn" ?
            @"不可添加，未设置主键的值：{0}" :
            @"FreeSql: Not addable, no value for primary key set: {0}", entityString);
        /// <summary>
        /// 不可添加，自增属性有值：{entityString}
        /// </summary>
        public static string CannotAdd_SelfIncreasingHasValue(object entityString) => string.Format(Language == "cn" ?
            @"不可添加，自增属性有值：{0}" :
            @"FreeSql: Not addable, self-increasing attribute has value: {0}", entityString);
        /// <summary>
        /// 不可附加，实体没有主键：{entityString}
        /// </summary>
        public static string CannotAttach_EntityHasNo_PrimaryKey(object entityString) => string.Format(Language == "cn" ?
            @"不可附加，实体没有主键：{0}" :
            @"FreeSql: Not attachable, entity has no primary key: {0}", entityString);
        /// <summary>
        /// 不可附加，未设置主键的值：{entityString}
        /// </summary>
        public static string CannotAttach_PrimaryKey_NotSet(object entityString) => string.Format(Language == "cn" ?
            @"不可附加，未设置主键的值：{0}" :
            @"FreeSql: Not attachable, no value for primary key set: {0}", entityString);
        /// <summary>
        /// 不可删除，数据未被跟踪，应该先查询：{entityString}
        /// </summary>
        public static string CannotDelete_DataNotTracked_ShouldQuery(object entityString) => string.Format(Language == "cn" ?
            @"不可删除，数据未被跟踪，应该先查询：{0}" :
            @"FreeSql: Not deletable, data not tracked, should query first: {0}", entityString);
        /// <summary>
        /// 不可删除，实体没有主键：{entityString}
        /// </summary>
        public static string CannotDelete_EntityHasNo_PrimaryKey(object entityString) => string.Format(Language == "cn" ?
            @"不可删除，实体没有主键：{0}" :
            @"FreeSql: Not deletable, entity has no primary key: {0}", entityString);
        /// <summary>
        /// 不可删除，未设置主键的值：{entityString}
        /// </summary>
        public static string CannotDelete_PrimaryKey_NotSet(object entityString) => string.Format(Language == "cn" ?
            @"不可删除，未设置主键的值：{0}" :
            @"FreeSql: Not deletable, no value for primary key set: {0}", entityString);
        /// <summary>
        /// 不可进行编辑，实体没有主键：{entityString}
        /// </summary>
        public static string CannotEdit_EntityHasNo_PrimaryKey(object entityString) => string.Format(Language == "cn" ?
            @"不可进行编辑，实体没有主键：{0}" :
            @"FreeSql: Not editable, entity has no primary key: {0}", entityString);
        /// <summary>
        /// 不可更新，数据未被跟踪，应该先查询 或者 Attach：{entityString}
        /// </summary>
        public static string CannotUpdate_DataShouldQueryOrAttach(object entityString) => string.Format(Language == "cn" ?
            @"不可更新，数据未被跟踪，应该先查询 或者 Attach：{0}" :
            @"FreeSql: Not updatable, data not tracked, should be queried first or Attach:{0}", entityString);
        /// <summary>
        /// 不可更新，实体没有主键：{entityString}
        /// </summary>
        public static string CannotUpdate_EntityHasNo_PrimaryKey(object entityString) => string.Format(Language == "cn" ?
            @"不可更新，实体没有主键：{0}" :
            @"FreeSql: Not updatable, entity has no primary key: {0}", entityString);
        /// <summary>
        /// 不可更新，未设置主键的值：{entityString}
        /// </summary>
        public static string CannotUpdate_PrimaryKey_NotSet(object entityString) => string.Format(Language == "cn" ?
            @"不可更新，未设置主键的值：{0}" :
            @"FreeSql: Not updatable, no value for primary key set: {0}", entityString);
        /// <summary>
        /// 不可更新，数据库不存在该记录：{entityString}
        /// </summary>
        public static string CannotUpdate_RecordDoesNotExist(object entityString) => string.Format(Language == "cn" ?
            @"不可更新，数据库不存在该记录：{0}" :
            @"FreeSql: Not updatable, the record does not exist in the database: {0}", entityString);
        /// <summary>
        /// 请在 OnConfiguring 或 AddFreeDbContext 中配置 UseFreeSql
        /// </summary>
        public static string ConfigureUseFreeSql => Language == "cn" ?
            @"请在 OnConfiguring 或 AddFreeDbContext 中配置 UseFreeSql" :
            @"FreeSql: Please configure UseFreeSql in OnConfiguring or AddFreeDbContext";
        /// <summary>
        /// DbSet.AsType 参数错误，请传入正确的实体类型
        /// </summary>
        public static string DbSetAsType_NotSupport_Object => Language == "cn" ?
            @"DbSet.AsType 参数错误，请传入正确的实体类型" :
            @"FreeSql: DbSet. AsType parameter error, please pass in the correct entity type";
        /// <summary>
        /// 实体类型 {EntityTypeName} 无法转换为 {name}，无法使用该方法
        /// </summary>
        public static string EntityType_CannotConvert(object EntityTypeName, object name) => string.Format(Language == "cn" ?
            @"实体类型 {0} 无法转换为 {1}，无法使用该方法" :
            @"FreeSql: Entity type {0} cannot be converted to {1} and cannot use this method", EntityTypeName, name);
        /// <summary>
        /// 实体类型 {EntityTypeName} 主键类型不为 {fullName}，无法使用该方法
        /// </summary>
        public static string EntityType_PrimaryKeyError(object EntityTypeName, object fullName) => string.Format(Language == "cn" ?
            @"实体类型 {0} 主键类型不为 {1}，无法使用该方法" :
            @"FreeSql: Entity type {0} Primary key type is not {1} and cannot be used with this method", EntityTypeName, fullName);
        /// <summary>
        /// 实体类型 {EntityTypeName} 主键数量不为 1，无法使用该方法
        /// </summary>
        public static string EntityType_PrimaryKeyIsNotOne(object EntityTypeName) => string.Format(Language == "cn" ?
            @"实体类型 {0} 主键数量不为 1，无法使用该方法" :
            @"FreeSql: Entity type {0} Primary key number is not 1 and cannot be used with this method", EntityTypeName);
        /// <summary>
        /// FreeSql.Repository 设置过滤器失败，原因是对象不属于 IRepository
        /// </summary>
        public static string FailedSetFilter_NotBelongIRpository => Language == "cn" ?
            @"FreeSql.Repository 设置过滤器失败，原因是对象不属于 IRepository" :
            @"FreeSql: FreeSql. Repository failed to set filter because object does not belong to IRepository";
        /// <summary>
        /// 不可比较，实体没有主键：{entityString}
        /// </summary>
        public static string Incomparable_EntityHasNo_PrimaryKey(object entityString) => string.Format(Language == "cn" ?
            @"不可比较，实体没有主键：{0}" :
            @"FreeSql: Not comparable, entity has no primary key: {0}", entityString);
        /// <summary>
        /// 不可比较，未设置主键的值：{entityString}
        /// </summary>
        public static string Incomparable_PrimaryKey_NotSet(object entityString) => string.Format(Language == "cn" ?
            @"不可比较，未设置主键的值：{0}" :
            @"FreeSql: Non-comparable, no value for primary key set: {0}", entityString);
        /// <summary>
        /// FreeSql.Repository Insert 失败，因为设置了过滤器 {filterKey}: {filterValueExpression}，插入的数据不符合 {entityString}
        /// </summary>
        public static string InsertError_Filter(object filterKey, object filterValueExpression, object entityString) => string.Format(Language == "cn" ?
            @"FreeSql.Repository Insert 失败，因为设置了过滤器 {0}: {1}，插入的数据不符合 {2}" :
            @"FreeSql: FreeSql.Repository Insert failed because the filter {0}: {1} was set and the inserted data does not conform to {2}", filterKey, filterValueExpression, entityString);
        /// <summary>
        /// ISelect.AsType 参数不支持指定为 object
        /// </summary>
        public static string ISelectAsType_ParameterError => Language == "cn" ?
            @"ISelect.AsType 参数不支持指定为 object" :
            @"FreeSql: ISelect. AsType parameter does not support specifying as object";
        /// <summary>
        /// {tableTypeFullName} 不存在属性 {propertyName}
        /// </summary>
        public static string NotFound_Property(object propertyName, object tableTypeFullName) => string.Format(Language == "cn" ?
            @"{1} 不存在属性 {0}" :
            @"FreeSql: Property {0} does not exist for {1}", propertyName, tableTypeFullName);
        /// <summary>
        /// 找不到方法 DbSet&amp;lt;&amp;gt;.StatesRemoveByObjects
        /// </summary>
        public static string NotFoundMethod_StatesRemoveByObjects => Language == "cn" ?
            @"找不到方法 DbSet<>.StatesRemoveByObjects" :
            @"FreeSql: Method DbSet<> not found. StatesRemoveByObjects";
        /// <summary>
        /// 参数 data 类型错误 {entityTypeFullName} 
        /// </summary>
        public static string ParameterDataTypeError(object entityTypeFullName) => string.Format(Language == "cn" ?
            @"参数 data 类型错误 {0} " :
            @"FreeSql: Parameter data type error {0}", entityTypeFullName);
        /// <summary>
        /// 参数错误 {param}
        /// </summary>
        public static string ParameterError(object param) => string.Format(Language == "cn" ?
            @"参数错误 {0}" :
            @"FreeSql: Parameter error {0}", param);
        /// <summary>
        /// 参数错误 {param} 不能为 null
        /// </summary>
        public static string ParameterError_CannotBeNull(object param) => string.Format(Language == "cn" ?
            @"参数错误 {0} 不能为 null" :
            @"FreeSql: Parameter error {0} cannot be null", param);
        /// <summary>
        /// 参数错误 {many} 不是集合属性
        /// </summary>
        public static string ParameterError_IsNot_CollectionProperties(object many) => string.Format(Language == "cn" ?
            @"参数错误 {0} 不是集合属性" :
            @"FreeSql: Parameter error {0} is not a collection property", many);
        /// <summary>
        /// 参数错误 {many} 集合属性不存在
        /// </summary>
        public static string ParameterError_NotFound_CollectionProperties(object many) => string.Format(Language == "cn" ?
            @"参数错误 {0} 集合属性不存在" :
            @"FreeSql: Parameter error {0} Collection property does not exist", many);
        /// <summary>
        /// 参数错误 {one} 属性不存在
        /// </summary>
        public static string ParameterError_NotFound_Property(object one) => string.Format(Language == "cn" ?
            @"参数错误 {0} 属性不存在" :
            @"FreeSql: Parameter error {0} attribute does not exist", one);
        /// <summary>
        /// Propagation_Mandatory: 使用当前事务，如果没有当前事务，就抛出异常
        /// </summary>
        public static string Propagation_Mandatory => Language == "cn" ?
            @"Propagation_Mandatory: 使用当前事务，如果没有当前事务，就抛出异常" :
            @"FreeSql: Propagation_ Mandatory: With the current transaction, throw an exception if there is no current transaction";
        /// <summary>
        /// Propagation_Never: 以非事务方式执行操作，如果当前事务存在则抛出异常
        /// </summary>
        public static string Propagation_Never => Language == "cn" ?
            @"Propagation_Never: 以非事务方式执行操作，如果当前事务存在则抛出异常" :
            @"FreeSql: Propagation_ Never: Perform the operation non-transactionally and throw an exception if the current transaction exists";
        /// <summary>
        /// {tableTypeFullName} 类型的属性 {propertyName} 不是 OneToMany 或 ManyToMany 特性
        /// </summary>
        public static string PropertyOfType_IsNot_OneToManyOrManyToMany(object propertyName, object tableTypeFullName) => string.Format(Language == "cn" ?
            @"{1} 类型的属性 {0} 不是 OneToMany 或 ManyToMany 特性" :
            @"FreeSql: Property {0} of type {1} is not OneToMany or ManyToMany attribute", propertyName, tableTypeFullName);
        /// <summary>
        /// 特别错误：批量添加失败，{dataType} 的返回数据，与添加的数目不匹配
        /// </summary>
        public static string SpecialError_BatchAdditionFailed(object dataType) => string.Format(Language == "cn" ?
            @"特别错误：批量添加失败，{0} 的返回数据，与添加的数目不匹配" :
            @"FreeSql: Special error: Bulk add failed, {0} returned data, does not match the number added", dataType);
        /// <summary>
        /// 特别错误：更新失败，数据未被跟踪：{entityString}
        /// </summary>
        public static string SpecialError_UpdateFailedDataNotTracked(object entityString) => string.Format(Language == "cn" ?
            @"特别错误：更新失败，数据未被跟踪：{0}" :
            @"FreeSql: Special error: Update failed, data not tracked: {0}", entityString);
        /// <summary>
        /// 已开启事务，不能禁用工作单元
        /// </summary>
        public static string TransactionHasBeenStarted => Language == "cn" ?
            @"已开启事务，不能禁用工作单元" :
            @"FreeSql: Transaction opened, unit of work cannot be disabled";
        /// <summary>
        /// {tableTypeFullName} 类型已设置属性 {propertyName} 忽略特性
        /// </summary>
        public static string TypeHasSetProperty_IgnoreAttribute(object tableTypeFullName, object propertyName) => string.Format(Language == "cn" ?
            @"{0} 类型已设置属性 {1} 忽略特性" :
            @"FreeSql: The {0} type has set the property {1} Ignore the attribute", tableTypeFullName, propertyName);
        /// <summary>
        /// {unitOfWorkManager} 构造参数 {fsql} 不能为 null
        /// </summary>
        public static string UnitOfWorkManager_Construction_CannotBeNull(object unitOfWorkManager, object fsql) => string.Format(Language == "cn" ?
            @"{0} 构造参数 {1} 不能为 null" :
            @"FreeSql: The {0} constructor parameter {1} cannot be null", unitOfWorkManager, fsql);
        /// <summary>
        /// FreeSql.Repository Update 失败，因为设置了过滤器 {filterKey}: {filterValueExpression}，更新的数据不符合{entityString}
        /// </summary>
        public static string UpdateError_Filter(object filterKey, object filterValueExpression, object entityString) => string.Format(Language == "cn" ?
            @"FreeSql.Repository Update 失败，因为设置了过滤器 {0}: {1}，更新的数据不符合{2}" :
            @"FreeSql: FreeSql.Repository Update failed because the filter {0}: {1} is set and the updated data does not conform to {2}", filterKey, filterValueExpression, entityString);
    }

}
