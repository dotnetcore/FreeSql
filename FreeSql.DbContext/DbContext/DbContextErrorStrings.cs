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
        public static string AddFreeDbContextError_CheckConstruction(object dbContextTypeName) => Language == "cn" ?
            $@"AddFreeDbContext 发生错误，请检查 {dbContextTypeName} 的构造参数都已正确注入" :
            $@"FreeSql: An error occurred in AddFreeDbContext, check that the construction parameters of {dbContextTypeName} have been injected correctly";
        /// <summary>
        /// 不可添加，已存在于状态管理：{entityString}
        /// </summary>
        public static string CannotAdd_AlreadyExistsInStateManagement(object entityString) => Language == "cn" ?
            $@"不可添加，已存在于状态管理：{entityString}" :
            $@"FreeSql: Not addable, already exists in state management: {entityString}";
        /// <summary>
        /// 不可添加，实体没有主键：{entityString}
        /// </summary>
        public static string CannotAdd_EntityHasNo_PrimaryKey(object entityString) => Language == "cn" ?
            $@"不可添加，实体没有主键：{entityString}" :
            $@"FreeSql: Not addable, entity has no primary key: {entityString}";
        /// <summary>
        /// 不可添加，未设置主键的值：{entityString}
        /// </summary>
        public static string CannotAdd_PrimaryKey_NotSet(object entityString) => Language == "cn" ?
            $@"不可添加，未设置主键的值：{entityString}" :
            $@"FreeSql: Not addable, no value for primary key set: {entityString}";
        /// <summary>
        /// 不可添加，自增属性有值：{entityString}
        /// </summary>
        public static string CannotAdd_SelfIncreasingHasValue(object entityString) => Language == "cn" ?
            $@"不可添加，自增属性有值：{entityString}" :
            $@"FreeSql: Not addable, self-increasing attribute has value: {entityString}";
        /// <summary>
        /// 不可附加，实体没有主键：{entityString}
        /// </summary>
        public static string CannotAttach_EntityHasNo_PrimaryKey(object entityString) => Language == "cn" ?
            $@"不可附加，实体没有主键：{entityString}" :
            $@"FreeSql: Not attachable, entity has no primary key: {entityString}";
        /// <summary>
        /// 不可附加，未设置主键的值：{entityString}
        /// </summary>
        public static string CannotAttach_PrimaryKey_NotSet(object entityString) => Language == "cn" ?
            $@"不可附加，未设置主键的值：{entityString}" :
            $@"FreeSql: Not attachable, no value for primary key set: {entityString}";
        /// <summary>
        /// 不可删除，数据未被跟踪，应该先查询：{entityString}
        /// </summary>
        public static string CannotDelete_DataNotTracked_ShouldQuery(object entityString) => Language == "cn" ?
            $@"不可删除，数据未被跟踪，应该先查询：{entityString}" :
            $@"FreeSql: Not deletable, data not tracked, should query first: {entityString}";
        /// <summary>
        /// 不可删除，实体没有主键：{entityString}
        /// </summary>
        public static string CannotDelete_EntityHasNo_PrimaryKey(object entityString) => Language == "cn" ?
            $@"不可删除，实体没有主键：{entityString}" :
            $@"FreeSql: Not deletable, entity has no primary key: {entityString}";
        /// <summary>
        /// 不可删除，未设置主键的值：{entityString}
        /// </summary>
        public static string CannotDelete_PrimaryKey_NotSet(object entityString) => Language == "cn" ?
            $@"不可删除，未设置主键的值：{entityString}" :
            $@"FreeSql: Not deletable, no value for primary key set: {entityString}";
        /// <summary>
        /// 不可进行编辑，实体没有主键：{entityString}
        /// </summary>
        public static string CannotEdit_EntityHasNo_PrimaryKey(object entityString) => Language == "cn" ?
            $@"不可进行编辑，实体没有主键：{entityString}" :
            $@"FreeSql: Not editable, entity has no primary key: {entityString}";
        /// <summary>
        /// 不可更新，数据未被跟踪，应该先查询 或者 Attach：{entityString}
        /// </summary>
        public static string CannotUpdate_DataShouldQueryOrAttach(object entityString) => Language == "cn" ?
            $@"不可更新，数据未被跟踪，应该先查询 或者 Attach：{entityString}" :
            $@"FreeSql: Not updatable, data not tracked, should be queried first or Attach:{entityString}";
        /// <summary>
        /// 不可更新，实体没有主键：{entityString}
        /// </summary>
        public static string CannotUpdate_EntityHasNo_PrimaryKey(object entityString) => Language == "cn" ?
            $@"不可更新，实体没有主键：{entityString}" :
            $@"FreeSql: Not updatable, entity has no primary key: {entityString}";
        /// <summary>
        /// 不可更新，未设置主键的值：{entityString}
        /// </summary>
        public static string CannotUpdate_PrimaryKey_NotSet(object entityString) => Language == "cn" ?
            $@"不可更新，未设置主键的值：{entityString}" :
            $@"FreeSql: Not updatable, no value for primary key set: {entityString}";
        /// <summary>
        /// 不可更新，数据库不存在该记录：{entityString}
        /// </summary>
        public static string CannotUpdate_RecordDoesNotExist(object entityString) => Language == "cn" ?
            $@"不可更新，数据库不存在该记录：{entityString}" :
            $@"FreeSql: Not updatable, the record does not exist in the database: {entityString}";
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
        public static string EntityType_CannotConvert(object EntityTypeName, object name) => Language == "cn" ?
            $@"实体类型 {EntityTypeName} 无法转换为 {name}，无法使用该方法" :
            $@"FreeSql: Entity type {EntityTypeName} cannot be converted to {name} and cannot use this method";
        /// <summary>
        /// 实体类型 {EntityTypeName} 主键类型不为 {fullName}，无法使用该方法
        /// </summary>
        public static string EntityType_PrimaryKeyError(object EntityTypeName, object fullName) => Language == "cn" ?
            $@"实体类型 {EntityTypeName} 主键类型不为 {fullName}，无法使用该方法" :
            $@"FreeSql: Entity type {EntityTypeName} Primary key type is not {fullName} and cannot be used with this method";
        /// <summary>
        /// 实体类型 {EntityTypeName} 主键数量不为 1，无法使用该方法
        /// </summary>
        public static string EntityType_PrimaryKeyIsNotOne(object EntityTypeName) => Language == "cn" ?
            $@"实体类型 {EntityTypeName} 主键数量不为 1，无法使用该方法" :
            $@"FreeSql: Entity type {EntityTypeName} Primary key number is not 1 and cannot be used with this method";
        /// <summary>
        /// FreeSql.Repository 设置过滤器失败，原因是对象不属于 IRepository
        /// </summary>
        public static string FailedSetFilter_NotBelongIRpository => Language == "cn" ?
            @"FreeSql.Repository 设置过滤器失败，原因是对象不属于 IRepository" :
            @"FreeSql: FreeSql. Repository failed to set filter because object does not belong to IRepository";
        /// <summary>
        /// 不可比较，实体没有主键：{entityString}
        /// </summary>
        public static string Incomparable_EntityHasNo_PrimaryKey(object entityString) => Language == "cn" ?
            $@"不可比较，实体没有主键：{entityString}" :
            $@"FreeSql: Not comparable, entity has no primary key: {entityString}";
        /// <summary>
        /// 不可比较，未设置主键的值：{entityString}
        /// </summary>
        public static string Incomparable_PrimaryKey_NotSet(object entityString) => Language == "cn" ?
            $@"不可比较，未设置主键的值：{entityString}" :
            $@"FreeSql: Non-comparable, no value for primary key set: {entityString}";
        /// <summary>
        /// FreeSql.Repository Insert 失败，因为设置了过滤器 {filterKey}: {filterValueExpression}，插入的数据不符合 {entityString}
        /// </summary>
        public static string InsertError_Filter(object filterKey, object filterValueExpression, object entityString) => Language == "cn" ?
            $@"FreeSql.Repository Insert 失败，因为设置了过滤器 {filterKey}: {filterValueExpression}，插入的数据不符合 {entityString}" :
            $@"FreeSql: FreeSql.Repository Insert failed because the filter {filterKey}: {filterValueExpression} was set and the inserted data does not conform to {entityString}";
        /// <summary>
        /// ISelect.AsType 参数不支持指定为 object
        /// </summary>
        public static string ISelectAsType_ParameterError => Language == "cn" ?
            @"ISelect.AsType 参数不支持指定为 object" :
            @"FreeSql: ISelect. AsType parameter does not support specifying as object";
        /// <summary>
        /// {tableTypeFullName} 不存在属性 {propertyName}
        /// </summary>
        public static string NotFound_Property(object propertyName, object tableTypeFullName) => Language == "cn" ?
            $@"{tableTypeFullName} 不存在属性 {propertyName}" :
            $@"FreeSql: Property {propertyName} does not exist for {tableTypeFullName}";
        /// <summary>
        /// 找不到方法 DbSet&amp;lt;&amp;gt;.StatesRemoveByObjects
        /// </summary>
        public static string NotFoundMethod_StatesRemoveByObjects => Language == "cn" ?
            @"找不到方法 DbSet<>.StatesRemoveByObjects" :
            @"FreeSql: Method DbSet<> not found. StatesRemoveByObjects";
        /// <summary>
        /// 参数 data 类型错误 {entityTypeFullName} 
        /// </summary>
        public static string ParameterDataTypeError(object entityTypeFullName) => Language == "cn" ?
            $@"参数 data 类型错误 {entityTypeFullName} " :
            $@"FreeSql: Parameter data type error {entityTypeFullName}";
        /// <summary>
        /// 参数错误 {param}
        /// </summary>
        public static string ParameterError(object param) => Language == "cn" ?
            $@"参数错误 {param}" :
            $@"FreeSql: Parameter error {param}";
        /// <summary>
        /// 参数错误 {param} 不能为 null
        /// </summary>
        public static string ParameterError_CannotBeNull(object param) => Language == "cn" ?
            $@"参数错误 {param} 不能为 null" :
            $@"FreeSql: Parameter error {param} cannot be null";
        /// <summary>
        /// 参数错误 {many} 不是集合属性
        /// </summary>
        public static string ParameterError_IsNot_CollectionProperties(object many) => Language == "cn" ?
            $@"参数错误 {many} 不是集合属性" :
            $@"FreeSql: Parameter error {many} is not a collection property";
        /// <summary>
        /// 参数错误 {many} 集合属性不存在
        /// </summary>
        public static string ParameterError_NotFound_CollectionProperties(object many) => Language == "cn" ?
            $@"参数错误 {many} 集合属性不存在" :
            $@"FreeSql: Parameter error {many} Collection property does not exist";
        /// <summary>
        /// 参数错误 {one} 属性不存在
        /// </summary>
        public static string ParameterError_NotFound_Property(object one) => Language == "cn" ?
            $@"参数错误 {one} 属性不存在" :
            $@"FreeSql: Parameter error {one} attribute does not exist";
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
        public static string PropertyOfType_IsNot_OneToManyOrManyToMany(object propertyName, object tableTypeFullName) => Language == "cn" ?
            $@"{tableTypeFullName} 类型的属性 {propertyName} 不是 OneToMany 或 ManyToMany 特性" :
            $@"FreeSql: Property {propertyName} of type {tableTypeFullName} is not OneToMany or ManyToMany attribute";
        /// <summary>
        /// 特别错误：批量添加失败，{dataType} 的返回数据，与添加的数目不匹配
        /// </summary>
        public static string SpecialError_BatchAdditionFailed(object dataType) => Language == "cn" ?
            $@"特别错误：批量添加失败，{dataType} 的返回数据，与添加的数目不匹配" :
            $@"FreeSql: Special error: Bulk add failed, {dataType} returned data, does not match the number added";
        /// <summary>
        /// 特别错误：更新失败，数据未被跟踪：{entityString}
        /// </summary>
        public static string SpecialError_UpdateFailedDataNotTracked(object entityString) => Language == "cn" ?
            $@"特别错误：更新失败，数据未被跟踪：{entityString}" :
            $@"FreeSql: Special error: Update failed, data not tracked: {entityString}";
        /// <summary>
        /// 已开启事务，不能禁用工作单元
        /// </summary>
        public static string TransactionHasBeenStarted => Language == "cn" ?
            @"已开启事务，不能禁用工作单元" :
            @"FreeSql: Transaction opened, unit of work cannot be disabled";
        /// <summary>
        /// {tableTypeFullName} 类型已设置属性 {propertyName} 忽略特性
        /// </summary>
        public static string TypeHasSetProperty_IgnoreAttribute(object tableTypeFullName, object propertyName) => Language == "cn" ?
            $@"{tableTypeFullName} 类型已设置属性 {propertyName} 忽略特性" :
            $@"FreeSql: The {tableTypeFullName} type has set the property {propertyName} Ignore the attribute";
        /// <summary>
        /// {unitOfWorkManager} 构造参数 {fsql} 不能为 null
        /// </summary>
        public static string UnitOfWorkManager_Construction_CannotBeNull(object unitOfWorkManager, object fsql) => Language == "cn" ?
            $@"{unitOfWorkManager} 构造参数 {fsql} 不能为 null" :
            $@"FreeSql: The {unitOfWorkManager} constructor parameter {fsql} cannot be null";
        /// <summary>
        /// FreeSql.Repository Update 失败，因为设置了过滤器 {filterKey}: {filterValueExpression}，更新的数据不符合{entityString}
        /// </summary>
        public static string UpdateError_Filter(object filterKey, object filterValueExpression, object entityString) => Language == "cn" ?
            $@"FreeSql.Repository Update 失败，因为设置了过滤器 {filterKey}: {filterValueExpression}，更新的数据不符合{entityString}" :
            $@"FreeSql: FreeSql.Repository Update failed because the filter {filterKey}: {filterValueExpression} is set and the updated data does not conform to {entityString}";
    }
}
