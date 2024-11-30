using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace FreeSql
{
    public static class CoreErrorStrings
    {
        public static string Language = "en";
        /// <summary>
        /// [Table(AsTable = "{asTable}")] 特性值格式错误
        /// </summary>
        public static string AsTable_PropertyName_FormatError(object asTable) => Language == "cn" ?
            $@"[Table(AsTable = ""{asTable}"")] 特性值格式错误" :
            $@"FreeSql: [Table(AsTable=""{asTable}"")] Property value formatted incorrectly";
        /// <summary>
        /// [Table(AsTable = xx)] 设置的属性名 {atmGroupsValue} 不是 DateTime 类型
        /// </summary>
        public static string AsTable_PropertyName_NotDateTime(object atmGroupsValue) => Language == "cn" ?
            $@"[Table(AsTable = xx)] 设置的属性名 {atmGroupsValue} 不是 DateTime 类型" :
            $@"FreeSql: The property name {atmGroupsValue} set by [Table (AsTable = xx)] is not of type DateTime";
        /// <summary>
        /// {name}: Failed to get resource {statistics}
        /// </summary>
        public static string Available_Failed_Get_Resource(object name, object statistics) => Language == "cn" ?
            $@"{name}: Failed to get resource {statistics}" :
            $@"FreeSql: {name}: Failed to get resource {statistics}";
        /// <summary>
        /// {name}: An exception needs to be thrown
        /// </summary>
        public static string Available_Thrown_Exception(object name) => Language == "cn" ?
            $@"{name}: An exception needs to be thrown" :
            $@"FreeSql: {name}: An exception needs to be thrown";
        /// <summary>
        /// 错误的表达式格式 {column}
        /// </summary>
        public static string Bad_Expression_Format(object column) => Language == "cn" ?
            $@"错误的表达式格式 {column}" :
            $@"FreeSql: Wrong expression format {column}";
        /// <summary>
        /// Chunk 功能之前不可使用 Select
        /// </summary>
        public static string Before_Chunk_Cannot_Use_Select => Language == "cn" ?
            @"Chunk 功能之前不可使用 Select" :
            @"FreeSql: Select is not available until the Chunk function";
        /// <summary>
        /// 安全起见，请务必在事务开启之后，再使用 ForUpdate
        /// </summary>
        public static string Begin_Transaction_Then_ForUpdate => Language == "cn" ?
            @"安全起见，请务必在事务开启之后，再使用 ForUpdate" :
            @"FreeSql: For security reasons, be sure to use ForUpdate after the transaction is open";
        /// <summary>
        /// 不能为 null
        /// </summary>
        public static string Cannot_Be_NULL => Language == "cn" ?
            @"不能为 null" :
            @"FreeSql: Cannot be null";
        /// <summary>
        /// {name} 不能为 null
        /// </summary>
        public static string Cannot_Be_NULL_Name(object name) => Language == "cn" ?
            $@"{name} 不能为 null" :
            $@"FreeSql: {name} cannot be null";
        /// <summary>
        /// 无法匹配 {property}
        /// </summary>
        public static string Cannot_Match_Property(object property) => Language == "cn" ?
            $@"无法匹配 {property}" :
            $@"FreeSql: Unable to match {property}";
        /// <summary>
        /// {property} 无法解析为表达式树
        /// </summary>
        public static string Cannot_Resolve_ExpressionTree(object property) => Language == "cn" ?
            $@"{property} 无法解析为表达式树" :
            $@"FreeSql: {property} cannot be resolved to an expression tree";
        /// <summary>
        /// 参数 masterConnectionString 不可为空，请检查 UseConnectionString
        /// </summary>
        public static string Check_UseConnectionString => Language == "cn" ?
            @"参数 masterConnectionString 不可为空，请检查 UseConnectionString" :
            @"FreeSql: The parameter master ConnectionString cannot be empty, check UseConnectionString";
        /// <summary>
        /// 提交
        /// </summary>
        public static string Commit => Language == "cn" ?
            @"提交" :
            @"FreeSql: Commit";
        /// <summary>
        /// 连接失败，准备切换其他可用服务器
        /// </summary>
        public static string Connection_Failed_Switch_Servers => Language == "cn" ?
            @"连接失败，准备切换其他可用服务器" :
            @"FreeSql: Connection failed, ready to switch other available servers";
        /// <summary>
        /// 自定义表达式解析错误：类型 {exp3MethodDeclaringType} 需要定义 static ThreadLocal&amp;lt;ExpressionCallContext&amp;gt; 字段、字段、字段（重要三次提醒）
        /// </summary>
        public static string Custom_Expression_ParsingError(object exp3MethodDeclaringType) => Language == "cn" ?
            $@"自定义表达式解析错误：类型 {exp3MethodDeclaringType} 需要定义 static ThreadLocal<ExpressionCallContext> 字段、字段、字段（重要三次提醒）" :
            $@"FreeSql: Custom expression parsing error: type {exp3MethodDeclaringType} needs to define static ThreadLocal<ExpressionCallContext>field, field, field (important three reminders)";
        /// <summary>
        /// Custom { 反射信息 }不能为空，格式：{ 静态方法名 }{ 空格 }{ 反射信息 }
        /// </summary>
        public static string Custom_Reflection_IsNotNull => Language == "cn" ?
            $@"Custom {{ 反射信息 }}不能为空，格式：{{ 静态方法名 }}{{ 空格 }}{{ 反射信息 }}" :
            $@"FreeSql: Custom {{Reflection Information}} cannot be empty, format: {{static method name}}{{space}}{{reflection information}}";
        /// <summary>
        /// Custom { 静态方法名 }不能为空，格式：{ 静态方法名 }{ 空格 }{ 反射信息 }
        /// </summary>
        public static string Custom_StaticMethodName_IsNotNull => Language == "cn" ?
            $@"Custom {{ 静态方法名 }}不能为空，格式：{{ 静态方法名 }}{{ 空格 }}{{ 反射信息 }}" :
            $@"FreeSql: Custom {{static method name}} cannot be empty, format: {{static method name}}{{space}}{{reflection information}}";
        /// <summary>
        /// Custom 对应的{{ 静态方法名 }}：{fiValueCustomArray} 未设置 [DynamicFilterCustomAttribute] 特性
        /// </summary>
        public static string Custom_StaticMethodName_NotSet_DynamicFilterCustom(object fiValueCustomArray) => Language == "cn" ?
            $@"Custom 对应的{{{{ 静态方法名 }}}}：{fiValueCustomArray} 未设置 [DynamicFilterCustomAttribute] 特性" :
            $@"FreeSql: Custom corresponding {{{{static method name}}}}:{fiValueCustomArray} The [DynamicFilterCustomAttribute] attribute is not set";
        /// <summary>
        /// Custom 要求 Field 应该空格分割，并且长度为 2，格式：{ 静态方法名 }{ 空格 }{ 反射信息 }
        /// </summary>
        public static string CustomFieldSeparatedBySpaces => Language == "cn" ?
            $@"Custom 要求 Field 应该空格分割，并且长度为 2，格式：{{ 静态方法名 }}{{ 空格 }}{{ 反射信息 }}" :
            $@"FreeSql: Custom requires that Fields be space-split and 2-length in the format: {{static method name}}{{space}}{{reflection information}}";
        /// <summary>
        /// 操作的数据类型({dataDisplayCsharp}) 与 AsType({tableTypeDisplayCsharp}) 不一致，请检查。
        /// </summary>
        public static string DataType_AsType_Inconsistent(object dataDisplayCsharp, object tableTypeDisplayCsharp) => Language == "cn" ?
            $@"操作的数据类型({dataDisplayCsharp}) 与 AsType({tableTypeDisplayCsharp}) 不一致，请检查。" :
            $@"FreeSql: The data type of the operation ({dataDisplayCsharp}) is inconsistent with AsType ({tableTypeDisplayCsharp}). Please check.";
        /// <summary>
        /// DateRange 要求 Value 应该逗号分割，并且长度为 2
        /// </summary>
        public static string DateRange_Comma_Separateda_By2Char => Language == "cn" ?
            @"DateRange 要求 Value 应该逗号分割，并且长度为 2" :
            @"FreeSql: DateRange requires that Value be comma-separated and 2-length";
        /// <summary>
        /// DateRange 要求 Value[1] 格式必须为：yyyy、yyyy-MM、yyyy-MM-dd、yyyy-MM-dd HH、yyyy、yyyy-MM-dd HH:mm
        /// </summary>
        public static string DateRange_DateFormat_yyyy => Language == "cn" ?
            @"DateRange 要求 Value[1] 格式必须为：yyyy、yyyy-MM、yyyy-MM-dd、yyyy-MM-dd HH、yyyy、yyyy-MM-dd HH:mm" :
            @"FreeSql: DateRange requires that the Value [1] format must be: yyyy, yyyy-MM, yyyy-MM-dd, yyyyy-MM-dd HH, yyyy, yyyy-MM-dd HH:mm";
        /// <summary>
        /// 记录可能不存在，或者【行级乐观锁】版本过旧，更新数量{sourceCount}，影响的行数{affrows}。
        /// </summary>
        public static string DbUpdateVersionException_RowLevelOptimisticLock(object sourceCount, object affrows) => Language == "cn" ?
            $@"记录可能不存在，或者【行级乐观锁】版本过旧，更新数量{sourceCount}，影响的行数{affrows}。" :
            $@"FreeSql: The record may not exist, or the row level optimistic lock version is out of date, the number of updates {sourceCount}, the number of rows affected {affrows}.";
        /// <summary>
        /// SlaveConnectionString 数量与 SlaveWeights 不相同
        /// </summary>
        public static string Different_Number_SlaveConnectionString_SlaveWeights => Language == "cn" ?
            @"SlaveConnectionString 数量与 SlaveWeights 不相同" :
            @"FreeSql: The number of SlaveConnectionStrings is not the same as SlaveWeights";
        /// <summary>
        /// ColumnAttribute.Name {colattrName} 重复存在，请检查（注意：不区分大小写）
        /// </summary>
        public static string Duplicate_ColumnAttribute(object colattrName) => Language == "cn" ?
            $@"ColumnAttribute.Name {colattrName} 重复存在，请检查（注意：不区分大小写）" :
            $@"FreeSql: ColumnAttribute. Name {colattrName} exists repeatedly, please check (note: case insensitive)";
        /// <summary>
        /// 属性名 {pName} 重复存在，请检查（注意：不区分大小写）
        /// </summary>
        public static string Duplicate_PropertyName(object pName) => Language == "cn" ?
            $@"属性名 {pName} 重复存在，请检查（注意：不区分大小写）" :
            $@"FreeSql: Property name {pName} exists repeatedly, please check (note: case insensitive)";
        /// <summary>
        /// {function} 功能要求实体类 {tableCsName} 必须有主键
        /// </summary>
        public static string Entity_Must_Primary_Key(object function, object tableCsName) => Language == "cn" ?
            $@"{function} 功能要求实体类 {tableCsName} 必须有主键" :
            $@"FreeSql: The {function} feature requires that the entity class {tableCsName} must have a primary key";
        /// <summary>
        /// {tbTypeFullName} 是父子关系，但是 MySql 8.0 以下版本中不支持组合多主键
        /// </summary>
        public static string Entity_MySQL_VersionsBelow8_NotSupport_Multiple_PrimaryKeys(object tbTypeFullName) => Language == "cn" ?
            $@"{tbTypeFullName} 是父子关系，但是 MySql 8.0 以下版本中不支持组合多主键" :
            $@"FreeSql: {tbTypeFullName} is a parent-child relationship, but combinations of multiple primary keys are not supported in versions below MySql 8.0";
        /// <summary>
        /// {tbTypeFullName} 不是父子关系，无法使用该功能
        /// </summary>
        public static string Entity_NotParentChild_Relationship(object tbTypeFullName) => Language == "cn" ?
            $@"{tbTypeFullName} 不是父子关系，无法使用该功能" :
            $@"FreeSql: {tbTypeFullName} is not a parent-child relationship and cannot be used";
        /// <summary>
        /// 这个特别的子查询不能解析
        /// </summary>
        public static string EspeciallySubquery_Cannot_Parsing => Language == "cn" ?
            @"这个特别的子查询不能解析" :
            @"FreeSql: This particular subquery cannot be resolved";
        /// <summary>
        /// 表达式错误，它的顶级对象不是 ParameterExpression：{exp}
        /// </summary>
        public static string Expression_Error_Use_ParameterExpression(object exp) => Language == "cn" ?
            $@"表达式错误，它的顶级对象不是 ParameterExpression：{exp}" :
            $@"FreeSql: Expression error, its top object is not ParameterExpression:{exp}";
        /// <summary>
        /// 表达式错误，它不是连续的 MemberAccess 类型：{exp}
        /// </summary>
        public static string Expression_Error_Use_Successive_MemberAccess_Type(object exp) => Language == "cn" ?
            $@"表达式错误，它不是连续的 MemberAccess 类型：{exp}" :
            $@"FreeSql: Expression error, it is not a continuous MemberAccess type: {exp}";
        /// <summary>
        /// ExpressionTree 转换类型错误，值({value})，类型({valueTypeFullName})，目标类型({typeFullName})，{exMessage}
        /// </summary>
        public static string ExpressionTree_Convert_Type_Error(object value, object valueTypeFullName, object typeFullName, object exMessage) => Language == "cn" ?
            $@"ExpressionTree 转换类型错误，值({value})，类型({valueTypeFullName})，目标类型({typeFullName})，{exMessage}" :
            $@"FreeSql: ExpressionTree conversion type error, value ({value}), type ({valueTypeFullName}), target type ({typeFullName}), Error:{exMessage}";
        /// <summary>
        /// 未能解析分表字段值 {sqlWhere}
        /// </summary>
        public static string Failed_SubTable_FieldValue(object sqlWhere) => Language == "cn" ?
            $@"未能解析分表字段值 {sqlWhere}" :
            $@"FreeSql: Failed to parse table field value {sqlWhere}";
        /// <summary>
        /// AsTable 未实现的功能 {asTable}
        /// </summary>
        public static string Functions_AsTable_NotImplemented(object asTable) => Language == "cn" ?
            $@"AsTable 未实现的功能 {asTable}" :
            $@"FreeSql: Function {asTable} not implemented by AsTable";
        /// <summary>
        /// GBase 暂时不支持逗号以外的分割符
        /// </summary>
        public static string GBase_NotSupport_OtherThanCommas => Language == "cn" ?
            @"GBase 暂时不支持逗号以外的分割符" :
            @"FreeSql: GBase does not support separators other than commas at this time";
        /// <summary>
        /// tableName：{tableName} 生成了相同的分表名
        /// </summary>
        public static string Generated_Same_SubTable(object tableName) => Language == "cn" ?
            $@"tableName：{tableName} 生成了相同的分表名" :
            $@"FreeSql: TableName:{tableName} generated the same table name";
        /// <summary>
        /// GetPrimarys 传递的参数 "{primary}" 不正确，它不属于字典数据的键名
        /// </summary>
        public static string GetPrimarys_ParameterError_IsNotDictKey(object primary) => Language == "cn" ?
            $@"GetPrimarys 传递的参数 ""{primary}"" 不正确，它不属于字典数据的键名" :
            $@"FreeSql: The parameter'{primary}'passed by GetPrimarys is incorrect and does not belong to the key name of the dictionary data";
        /// <summary>
        /// 已经指定了 {first}，不能再指定 {second}
        /// </summary>
        public static string Has_Specified_Cannot_Specified_Second(object first, object second) => Language == "cn" ?
            $@"已经指定了 {first}，不能再指定 {second}" :
            $@"FreeSql: {first} has already been specified and {second} can no longer be specified";
        /// <summary>
        /// {tb2DbName}.{mp2MemberName} 被忽略，请检查 IsIgnore 设置，确认 get/set 为 public
        /// </summary>
        public static string Ignored_Check_Confirm_PublicGetSet(object tb2DbName, object mp2MemberName) => Language == "cn" ?
            $@"{tb2DbName}.{mp2MemberName} 被忽略，请检查 IsIgnore 设置，确认 get/set 为 public" :
            $@"FreeSql: {tb2DbName}. {mp2MemberName} is ignored. Check the IsIgnore setting to make sure get/set is public";
        /// <summary>
        /// Include 参数类型错误
        /// </summary>
        public static string Include_ParameterType_Error => Language == "cn" ?
            @"Include 参数类型错误" :
            @"FreeSql: Include parameter type error";
        /// <summary>
        /// Include 参数类型错误，集合属性请使用 IncludeMany
        /// </summary>
        public static string Include_ParameterType_Error_Use_IncludeMany => Language == "cn" ?
            @"Include 参数类型错误，集合属性请使用 IncludeMany" :
            @"FreeSql: Include parameter type is wrong, use IncludeMany for collection properties";
        /// <summary>
        /// Include 参数类型错误，表达式类型应该为 MemberAccess
        /// </summary>
        public static string Include_ParameterType_Error_Use_MemberAccess => Language == "cn" ?
            @"Include 参数类型错误，表达式类型应该为 MemberAccess" :
            @"FreeSql: Include parameter type is wrong, expression type should be MemberAccess";
        /// <summary>
        /// IncludeMany 类型 {tbTypeDisplayCsharp} 的属性 {collMemMemberName} 不是有效的导航属性，提示：IsIgnore = true 不会成为导航属性
        /// </summary>
        public static string IncludeMany_NotValid_Navigation(object collMemMemberName, object tbTypeDisplayCsharp) => Language == "cn" ?
            $@"IncludeMany 类型 {tbTypeDisplayCsharp} 的属性 {collMemMemberName} 不是有效的导航属性，提示：IsIgnore = true 不会成为导航属性" :
            $@"FreeSql: The property {collMemMemberName} of IncludeMany type {tbTypeDisplayCsharp} is not a valid navigation property, hint: IsIgnore = true will not be a navigation property";
        /// <summary>
        /// IncludeMany {navigateSelector} 参数错误，Select 只可以使用一个参数的方法，正确格式：.Select(t =&amp;gt;new TNavigate {{}})
        /// </summary>
        public static string IncludeMany_ParameterError_OnlyUseOneParameter(object navigateSelector) => Language == "cn" ?
            $@"IncludeMany {navigateSelector} 参数错误，Select 只可以使用一个参数的方法，正确格式：.Select(t =>new TNavigate {{{{}}}})" :
            $@"FreeSql: IncludeMany {navigateSelector} parameter is wrong, Select can only use one parameter's method, the correct format:.Select(t =>new TNavigate{{{{}}}})";
        /// <summary>
        /// IncludeMany {navigateSelector} 参数错误，Select lambda参数返回值必须和 {collMemElementType} 类型一致
        /// </summary>
        public static string IncludeMany_ParameterError_Select_ReturnConsistentType(object navigateSelector, object collMemElementType) => Language == "cn" ?
            $@"IncludeMany {navigateSelector} 参数错误，Select lambda参数返回值必须和 {collMemElementType} 类型一致" :
            $@"FreeSql: IncludeMany {navigateSelector} parameter error, Select lambda parameter return value must match {collMemElementType} type";
        /// <summary>
        /// IncludeMany 参数1 类型错误，表达式类型应该为 MemberAccess
        /// </summary>
        public static string IncludeMany_ParameterType_Error_Use_MemberAccess => Language == "cn" ?
            @"IncludeMany 参数1 类型错误，表达式类型应该为 MemberAccess" :
            @"FreeSql: IncludeMany parameter 1 has wrong type, expression type should be MemberAccess";
        /// <summary>
        /// IncludeMany {navigateSelector} 参数类型错误，正确格式： a.collections.Take(1).Where(c =&amp;gt;c.aid == a.id).Select(a=&amp;gt; new TNavigate{{}})
        /// </summary>
        public static string IncludeMany_ParameterTypeError(object navigateSelector) => Language == "cn" ?
            $@"IncludeMany {navigateSelector} 参数类型错误，正确格式： a.collections.Take(1).Where(c =>c.aid == a.id).Select(a=> new TNavigate{{{{}}}})" :
            $@"FreeSql: IncludeMany {navigateSelector} parameter type is wrong, correct format: a.collections.Take(1).Where(c => C.A ID == a.id).Select (a => new TNavigate{{{{}}}})";
        /// <summary>
        /// ISelect.InsertInto() 未选择属性: {displayCsharp}
        /// </summary>
        public static string InsertInto_No_Property_Selected(object displayCsharp) => Language == "cn" ?
            $@"ISelect.InsertInto() 未选择属性: {displayCsharp}" :
            $@"FreeSql: ISelect. InsertInto() did not select an attribute: {displayCsharp}";
        /// <summary>
        /// ISelect.InsertInto() 类型错误: {displayCsharp}
        /// </summary>
        public static string InsertInto_TypeError(object displayCsharp) => Language == "cn" ?
            $@"ISelect.InsertInto() 类型错误: {displayCsharp}" :
            $@"FreeSql: ISelect. InsertInto() type error: {displayCsharp}";
        /// <summary>
        /// InsertOrUpdate 功能执行 merge into 要求实体类 {CsName} 必须有主键
        /// </summary>
        public static string InsertOrUpdate_Must_Primary_Key(object CsName) => Language == "cn" ?
            $@"InsertOrUpdate 功能执行 merge into 要求实体类 {CsName} 必须有主键" :
            $@"FreeSql: The InsertOrUpdate function performs merge into requiring the entity class {CsName} to have a primary key";
        /// <summary>
        /// InsertOrUpdate&amp;lt;&amp;gt;的泛型参数 不支持 {typeofT1},请传递您的实体类
        /// </summary>
        public static string InsertOrUpdate_NotSuport_Generic_UseEntity(object typeofT1) => Language == "cn" ?
            $@"InsertOrUpdate<>的泛型参数 不支持 {typeofT1},请传递您的实体类" :
            $@"FreeSql: The generic parameter for InsertOrUpdate<>does not support {typeofT1}. Pass in your entity class";
        /// <summary>
        /// 【延时加载】功能需要安装 FreeSql.Extensions.LazyLoading.dll，可前往 nuget 下载
        /// </summary>
        public static string Install_FreeSql_Extensions_LazyLoading => Language == "cn" ?
            @"【延时加载】功能需要安装 FreeSql.Extensions.LazyLoading.dll，可前往 nuget 下载" :
            @"FreeSql: FreeSql needs to be installed for Delayed Loading. Extensions. LazyLoading. Dll, downloadable to nuget";
        /// <summary>
        /// 【延时加载】{trytbTypeName} 编译错误：{exMessage}
        /// </summary>
        public static string LazyLoading_CompilationError(object trytbTypeName, object exMessage, object cscode) => Language == "cn" ?
            $@"【延时加载】{trytbTypeName} 编译错误：{exMessage}

{cscode}" :
            $@"FreeSql: {trytbTypeName} Compilation error: {exMessage}

{cscode}";
        /// <summary>
        /// 【延时加载】实体类型 {trytbTypeName} 必须声明为 public
        /// </summary>
        public static string LazyLoading_EntityMustDeclarePublic(object trytbTypeName) => Language == "cn" ?
            $@"【延时加载】实体类型 {trytbTypeName} 必须声明为 public" :
            $@"FreeSql: Entity type {trytbTypeName} must be declared public";
        /// <summary>
        /// ManyToMany 导航属性 .AsSelect() 暂时不可用于 Sum/Avg/Max/Min/First/ToOne/ToList 方法
        /// </summary>
        public static string ManyToMany_AsSelect_NotSupport_Sum_Avg_etc => Language == "cn" ?
            @"ManyToMany 导航属性 .AsSelect() 暂时不可用于 Sum/Avg/Max/Min/First/ToOne/ToList 方法" :
            @"FreeSql: ManyToMany navigation properties. AsSelect() is temporarily unavailable for the Sum/Avg/Max/Min/First/ToOne/ToList method";
        /// <summary>
        /// 【ManyToMany】导航属性 {trytbTypeName}.{pnvName} 在 {tbmidCsName} 中没有找到对应的字段，如：{midTypePropsTrytbName}{findtrytbPkCsName}、{midTypePropsTrytbName}_{findtrytbPkCsName}
        /// </summary>
        public static string ManyToMany_NotFound_CorrespondingField(object trytbTypeName, object pnvName, object tbmidCsName, object midTypePropsTrytbName, object findtrytbPkCsName) => Language == "cn" ?
            $@"【ManyToMany】导航属性 {trytbTypeName}.{pnvName} 在 {tbmidCsName} 中没有找到对应的字段，如：{midTypePropsTrytbName}{findtrytbPkCsName}、{midTypePropsTrytbName}_{findtrytbPkCsName}" :
            $@"FreeSql: [ManyToMany] Navigation property {trytbTypeName}. {pnvName} did not find a corresponding field in {tbmidCsName}, such as: {midTypePropsTrytbName}{findtrytbPkCsName}, {midTypePropsTrytbName}_ {findtrytbPkCsName}";
        /// <summary>
        /// 【ManyToMany】导航属性 {trytbTypeName}.{pnvName} 解析错误，实体类型 {tbrefTypeName} 缺少主键标识，[Column(IsPrimary = true)]
        /// </summary>
        public static string ManyToMany_ParsingError_EntityMissing_PrimaryKey(object trytbTypeName, object pnvName, object tbrefTypeName) => Language == "cn" ?
            $@"【ManyToMany】导航属性 {trytbTypeName}.{pnvName} 解析错误，实体类型 {tbrefTypeName} 缺少主键标识，[Column(IsPrimary = true)]" :
            $@"FreeSql: [ManyToMany] Navigation property {trytbTypeName}. {pnvName} parsing error, entity type {tbrefTypeName} missing primary key identity, [Column (IsPrimary = true)]";
        /// <summary>
        /// 【ManyToMany】导航属性 {trytbTypeName}.{pnvName} 解析错误，实体类型 {tbrefTypeName} 必须存在对应的 [Navigate(ManyToMany = x)] 集合属性
        /// </summary>
        public static string ManyToMany_ParsingError_EntityMustHas_NavigateCollection(object trytbTypeName, object pnvName, object tbrefTypeName) => Language == "cn" ?
            $@"【ManyToMany】导航属性 {trytbTypeName}.{pnvName} 解析错误，实体类型 {tbrefTypeName} 必须存在对应的 [Navigate(ManyToMany = x)] 集合属性" :
            $@"FreeSql: [ManyToMany] Navigation property {trytbTypeName}. {pnvName} parsing error, entity type {tbrefTypeName} must have a corresponding [Navigate (ManyToMany = x)] collection property";
        /// <summary>
        /// 【ManyToMany】导航属性 {trytbTypeName}.{pnvName} 解析错误，{tbmidCsName}.{trycolCsName} 和 {trytbCsName}.{trytbPrimarysCsName} 类型不一致
        /// </summary>
        public static string ManyToMany_ParsingError_InconsistentType(object trytbTypeName, object pnvName, object tbmidCsName, object trycolCsName, object trytbCsName, object trytbPrimarysCsName) => Language == "cn" ?
            $@"【ManyToMany】导航属性 {trytbTypeName}.{pnvName} 解析错误，{tbmidCsName}.{trycolCsName} 和 {trytbCsName}.{trytbPrimarysCsName} 类型不一致" :
            $@"FreeSql: [ManyToMany] Navigation property {trytbTypeName}. {pnvName} parsing error, {tbmidCsName}. {trycolCsName} and {trytbCsName}. {trytbPrimarysCsName} type inconsistent";
        /// <summary>
        /// 【ManyToMany】导航属性 {trytbTypeName}.{pnvName} 解析错误，中间类 {tbmidCsName}.{midTypePropsTrytbName} 错误：{exMessage}
        /// </summary>
        public static string ManyToMany_ParsingError_IntermediateClass_ErrorMessage(object trytbTypeName, object pnvName, object tbmidCsName, object midTypePropsTrytbName, object exMessage) => Language == "cn" ?
            $@"【ManyToMany】导航属性 {trytbTypeName}.{pnvName} 解析错误，中间类 {tbmidCsName}.{midTypePropsTrytbName} 错误：{exMessage}" :
            $@"FreeSql: [ManyToMany] Navigation property {trytbTypeName}. {pnvName} parsing error, intermediate class {tbmidCsName}.{midTypePropsTrytbName} Error: {exMessage}";
        /// <summary>
        /// 【ManyToMany】导航属性 {trytbTypeName}.{pnvName} 解析错误，中间类 {tbmidCsName}.{midTypePropsTrytbName} 导航属性不是【ManyToOne】或【OneToOne】
        /// </summary>
        public static string ManyToMany_ParsingError_IntermediateClass_NotManyToOne_OneToOne(object trytbTypeName, object pnvName, object tbmidCsName, object midTypePropsTrytbName) => Language == "cn" ?
            $@"【ManyToMany】导航属性 {trytbTypeName}.{pnvName} 解析错误，中间类 {tbmidCsName}.{midTypePropsTrytbName} 导航属性不是【ManyToOne】或【OneToOne】" :
            $@"FreeSql: [ManyToMany] Navigation property {trytbTypeName}. {pnvName} parsing error, intermediate class {tbmidCsName}. The {midTypePropsTrytbName} navigation property is not ManyToOne or OneToOne";
        /// <summary>
        /// 映射异常：{name} 没有一个属性名相同
        /// </summary>
        public static string Mapping_Exception_HasNo_SamePropertyName(object name) => Language == "cn" ?
            $@"映射异常：{name} 没有一个属性名相同" :
            $@"FreeSql: Mapping exception: {name} None of the property names are the same";
        /// <summary>
        /// Ado.MasterPool 值为 null，该操作无法自启用事务，请显式传递【事务对象】解决
        /// </summary>
        public static string MasterPool_IsNull_UseTransaction => Language == "cn" ?
            @"Ado.MasterPool 值为 null，该操作无法自启用事务，请显式传递【事务对象】解决" :
            @"FreeSql: Ado. MasterPool value is null, this operation cannot self-enable transactions, please explicitly pass [transaction object] resolution";
        /// <summary>
        /// 缺少 FreeSql 数据库实现包：FreeSql.Provider.{Provider}.dll，可前往 nuget 下载
        /// </summary>
        public static string Missing_FreeSqlProvider_Package(object Provider) => Language == "cn" ?
            $@"缺少 FreeSql 数据库实现包：FreeSql.Provider.{Provider}.dll，可前往 nuget 下载" :
            $@"FreeSql: Missing FreeSql database implementation package: FreeSql.Provider.{Provider}.Dll, downloadable to nuget";
        /// <summary>
        /// 缺少 FreeSql 数据库实现包：{dll}，可前往 nuget 下载；如果存在 {dll} 依然报错（原因是环境问题导致反射不到类型），请在 UseConnectionString/UseConnectionFactory 第三个参数手工传入 typeof({providerType})
        /// </summary>
        public static string Missing_FreeSqlProvider_Package_Reason(object dll, object providerType) => Language == "cn" ?
            $@"缺少 FreeSql 数据库实现包：{dll}，可前往 nuget 下载；如果存在 {dll} 依然报错（原因是环境问题导致反射不到类型），请在 UseConnectionString/UseConnectionFactory 第三个参数手工传入 typeof({{2}})" :
            $@"FreeSql: The FreeSql database implementation package is missing: {dll} can be downloaded to nuget; If there is {dll} and an error still occurs (due to environmental issues that cause the type to be unreflected), manually pass in typeof ({{2}}) in the third parameter of UseConnectionString/UseConnectionFactory";
        /// <summary>
        /// 导航属性 {trytbTypeName}.{pnvName} 特性 [Navigate] Bind 数目({bindColumnsCount}) 与 外部主键数目({tbrefPrimarysLength}) 不相同
        /// </summary>
        public static string Navigation_Bind_Number_Different(object trytbTypeName, object pnvName, object bindColumnsCount, object tbrefPrimarysLength) => Language == "cn" ?
            $@"导航属性 {trytbTypeName}.{pnvName} 特性 [Navigate] Bind 数目({bindColumnsCount}) 与 外部主键数目({tbrefPrimarysLength}) 不相同" :
            $@"FreeSql: Navigation property {trytbTypeName}. The number of {pnvName} attributes [Navigate] Binds ({bindColumnsCount}) is different from the number of external primary keys ({tbrefPrimarysLength})";
        /// <summary>
        /// {tb2DbName}.{mp2MemberName} 导航属性集合忘了 .AsSelect() 吗？如果在 ToList(a =&amp;gt; a.{mp2MemberName}) 中使用，请移步参考 IncludeMany 文档。
        /// </summary>
        public static string Navigation_Missing_AsSelect(object tb2DbName, object mp2MemberName) => Language == "cn" ?
            $@"{tb2DbName}.{mp2MemberName} 导航属性集合忘了 .AsSelect() 吗？如果在 ToList(a => a.{mp2MemberName}) 中使用，请移步参考 IncludeMany 文档。" :
            $@"FreeSql: {tb2DbName}. {mp2MemberName} Navigation Property Collection forgotten. AsSelect()? If used in ToList (a => a. {mp2MemberName}), step by step to refer to the IncludeMany document.";
        /// <summary>
        /// 【导航属性】{trytbTypeDisplayCsharp}.{pName} 缺少 set 属性
        /// </summary>
        public static string Navigation_Missing_SetProperty(object trytbTypeDisplayCsharp, object pName) => Language == "cn" ?
            $@"【导航属性】{trytbTypeDisplayCsharp}.{pName} 缺少 set 属性" :
            $@"FreeSql: Navigation Properties {trytbTypeDisplayCsharp}. Missing set attribute for {pName}";
        /// <summary>
        /// 导航属性 {trytbTypeName}.{pnvName} 没有找到对应的字段，如：{pnvName}{findtbrefPkCsName}、{pnvName}_{findtbrefPkCsName}。或者使用 [Navigate] 特性指定关系映射。
        /// </summary>
        public static string Navigation_NotFound_CorrespondingField(object trytbTypeName, object pnvName, object findtbrefPkCsName) => Language == "cn" ?
            $@"导航属性 {trytbTypeName}.{pnvName} 没有找到对应的字段，如：{pnvName}{{3}}、{pnvName}_{{3}}。或者使用 [Navigate] 特性指定关系映射。" :
            $@"FreeSql: Navigation property {trytbTypeName}. {pnvName} No corresponding fields were found, such as: {pnvName}{{3}}, {pnvName}_ {{3}}. Or use the [Navigate] attribute to specify the relationship mapping.";
        /// <summary>
        /// 导航属性 {trytbTypeName}.{pnvName} 解析错误，实体类型 {trytcTypeName} 缺少主键标识，[Column(IsPrimary = true)]
        /// </summary>
        public static string Navigation_ParsingError_EntityMissingPrimaryKey(object trytbTypeName, object pnvName, object trytcTypeName) => Language == "cn" ?
            $@"导航属性 {trytbTypeName}.{pnvName} 解析错误，实体类型 {trytcTypeName} 缺少主键标识，[Column(IsPrimary = true)]" :
            $@"FreeSql: Navigation property {trytbTypeName}. {pnvName} parsing error, entity type {trytcTypeName} missing primary key identity, [Column (IsPrimary = true)]";
        /// <summary>
        /// 导航属性 {trytbTypeName}.{pnvName} 解析错误，{trytbCsName}.{trycolCsName} 和 {tbrefCsName}.{tbrefPrimarysCsName} 类型不一致
        /// </summary>
        public static string Navigation_ParsingError_InconsistentType(object trytbTypeName, object pnvName, object trytbCsName, object trycolCsName, object tbrefCsName, object tbrefPrimarysCsName) => Language == "cn" ?
            $@"导航属性 {trytbTypeName}.{pnvName} 解析错误，{trytbCsName}.{trycolCsName} 和 {tbrefCsName}.{tbrefPrimarysCsName} 类型不一致" :
            $@"FreeSql: Navigation property {trytbTypeName}. {pnvName} parsing error, {trytbCsName}. {trycolCsName} and {tbrefCsName}. {tbrefPrimarysCsName} type inconsistent";
        /// <summary>
        /// 导航属性 {trytbTypeName}.{pnvName} 特性 [Navigate] 解析错误，在 {tbrefTypeName} 未找到属性：{bi}
        /// </summary>
        public static string Navigation_ParsingError_NotFound_Property(object trytbTypeName, object pnvName, object tbrefTypeName, object bi) => Language == "cn" ?
            $@"导航属性 {trytbTypeName}.{pnvName} 特性 [Navigate] 解析错误，在 {tbrefTypeName} 未找到属性：{bi}" :
            $@"FreeSql: Navigation property {trytbTypeName}. {pnvName} attribute [Navigate] parsing error, property not found at {tbrefTypeName}: {bi}";
        /// <summary>
        /// {tableTypeDisplayCsharp} 没有定义主键，无法使用 SetSource，请尝试 SetDto 或者 SetSource 指定临时主键
        /// </summary>
        public static string NoPrimaryKey_UseSetDto(object tableTypeDisplayCsharp) => Language == "cn" ?
            $@"{tableTypeDisplayCsharp} 没有定义主键，无法使用 SetSource，请尝试 SetDto 或者 SetSource 指定临时主键" :
            $@"FreeSql: {tableTypeDisplayCsharp} has no primary key defined and cannot use SetSource. Try SetDto";
        /// <summary>
        ///  没有定义属性 
        /// </summary>
        public static string NoProperty_Defined => Language == "cn" ?
            @" 没有定义属性 " :
            @"FreeSql: No properties defined";
        /// <summary>
        /// 未实现
        /// </summary>
        public static string Not_Implemented => Language == "cn" ?
            @"未实现" :
            @"FreeSql: Not implemented";
        /// <summary>
        /// 未实现函数表达式 {exp} 解析
        /// </summary>
        public static string Not_Implemented_Expression(object exp) => Language == "cn" ?
            $@"未实现函数表达式 {exp} 解析" :
            $@"FreeSql: Function expression {exp} parsing not implemented";
        /// <summary>
        /// 未实现函数表达式 {exp} 解析，参数 {expArguments} 必须为常量
        /// </summary>
        public static string Not_Implemented_Expression_ParameterUseConstant(object exp, object expArguments) => Language == "cn" ?
            $@"未实现函数表达式 {exp} 解析，参数 {expArguments} 必须为常量" :
            $@"FreeSql: Function expression {exp} parsing not implemented, parameter {expArguments} must be constant";
        /// <summary>
        /// 未实现函数表达式 {exp} 解析，如果正在操作导航属性集合，请使用 .AsSelect().{exp3MethodName}({exp3ArgumentsCount})
        /// </summary>
        public static string Not_Implemented_Expression_UseAsSelect(object exp, object exp3MethodName, object exp3ArgumentsCount) => Language == "cn" ?
            $@"未实现函数表达式 {exp} 解析，如果正在操作导航属性集合，请使用 .AsSelect().{exp3MethodName}({exp3ArgumentsCount})" :
            $@"FreeSql: Function expression {exp} parsing is not implemented. Use if you are working on a navigation property collection. AsSelect (). {exp3MethodName} ({exp3ArgumentsCount})";
        /// <summary>
        /// 未实现 MemberAccess 下的 Constant
        /// </summary>
        public static string Not_Implemented_MemberAcess_Constant => Language == "cn" ?
            @"未实现 MemberAccess 下的 Constant" :
            @"FreeSql: Constant under MemberAccess is not implemented";
        /// <summary>
        /// 未实现 {name}
        /// </summary>
        public static string Not_Implemented_Name(object name) => Language == "cn" ?
            $@"未实现 {name}" :
            $@"FreeSql: {name} is not implemented";
        /// <summary>
        /// 不支持
        /// </summary>
        public static string Not_Support => Language == "cn" ?
            @"不支持" :
            @"FreeSql: I won't support it";
        /// <summary>
        /// {dataType} 不支持 OrderByRandom 随机排序
        /// </summary>
        public static string Not_Support_OrderByRandom(object dataType) => Language == "cn" ?
            $@"{dataType} 不支持 OrderByRandom 随机排序" :
            $@"FreeSql: {dataType} does not support OrderByRandom sorting";
        /// <summary>
        /// {property} 不是有效的导航属性
        /// </summary>
        public static string Not_Valid_Navigation_Property(object property) => Language == "cn" ?
            $@"{property} 不是有效的导航属性" :
            $@"FreeSql: {property} is not a valid navigation property";
        /// <summary>
        /// {dbName} 找不到列 {memberName}
        /// </summary>
        public static string NotFound_Column(object dbName, object memberName) => Language == "cn" ?
            $@"{dbName} 找不到列 {memberName}" :
            $@"FreeSql: {dbName} Column {memberName} not found";
        /// <summary>
        /// 找不到 {CsName} 对应的列
        /// </summary>
        public static string NotFound_CsName_Column(object CsName) => Language == "cn" ?
            $@"找不到 {CsName} 对应的列" :
            $@"FreeSql: Cannot find the column corresponding to {CsName}";
        /// <summary>
        /// 找不到属性：{memberName}
        /// </summary>
        public static string NotFound_Property(object memberName) => Language == "cn" ?
            $@"找不到属性：{memberName}" :
            $@"FreeSql: Attribute not found: {memberName}";
        /// <summary>
        /// 找不到属性名 {proto}
        /// </summary>
        public static string NotFound_PropertyName(object proto) => Language == "cn" ?
            $@"找不到属性名 {proto}" :
            $@"FreeSql: Property name {proto} not found";
        /// <summary>
        /// Custom 找不到对应的{{ 反射信息 }}：{fiValueCustomArray}
        /// </summary>
        public static string NotFound_Reflection(object fiValueCustomArray) => Language == "cn" ?
            $@"Custom 找不到对应的{{{{ 反射信息 }}}}：{fiValueCustomArray}" :
            $@"FreeSql: Custom could not find the corresponding {{{{reflection information}}}}:{fiValueCustomArray}";
        /// <summary>
        /// Custom 找不到对应的{{ 静态方法名 }}：{fiValueCustomArray}
        /// </summary>
        public static string NotFound_Static_MethodName(object fiValueCustomArray) => Language == "cn" ?
            $@"Custom 找不到对应的{{{{ 静态方法名 }}}}：{fiValueCustomArray}" :
            $@"FreeSql: Custom could not find the corresponding {{{{static method name}}}}:{fiValueCustomArray}";
        /// <summary>
        /// [Table(AsTable = xx)] 设置的属性名 {atmGroupsValue} 不存在
        /// </summary>
        public static string NotFound_Table_Property_AsTable(object atmGroupsValue) => Language == "cn" ?
            $@"[Table(AsTable = xx)] 设置的属性名 {atmGroupsValue} 不存在" :
            $@"FreeSql: The property name {atmGroupsValue} set by [Table(AsTable = xx)] does not exist";
        /// <summary>
        /// 未指定 UseConnectionString 或者 UseConnectionFactory
        /// </summary>
        public static string NotSpecified_UseConnectionString_UseConnectionFactory => Language == "cn" ?
            @"未指定 UseConnectionString 或者 UseConnectionFactory" :
            @"FreeSql: No UseConnectionString or UseConnectionFactory specified";
        /// <summary>
        /// 【{policyName}】ObjectPool.{GetName}() timeout {totalSeconds} seconds, see: https://github.com/dotnetcore/FreeSql/discussions/1081
        /// </summary>
        public static string ObjectPool_Get_Timeout(object policyName, object GetName, object totalSeconds) => Language == "cn" ?
            $@"【{policyName}】ObjectPool.{GetName}() timeout {totalSeconds} seconds, see: https://github.com/dotnetcore/FreeSql/discussions/1081" :
            $@"FreeSql: [{policyName}] ObjectPool. {GetName}() timeout {totalSeconds} seconds, see: https://github.com/dotnetcore/FreeSql/discussions/1081";
        /// <summary>
        /// 【{policyName}】ObjectPool.GetAsync() The queue is too long. Policy.AsyncGetCapacity = {asyncGetCapacity}
        /// </summary>
        public static string ObjectPool_GetAsync_Queue_Long(object policyName, object asyncGetCapacity) => Language == "cn" ?
            $@"【{policyName}】ObjectPool.GetAsync() The queue is too long. Policy.AsyncGetCapacity = {asyncGetCapacity}" :
            $@"FreeSql: [{policyName}] ObjectPool. GetAsync() The queue is too long. Policy. AsyncGetCapacity = {asyncGetCapacity}";
        /// <summary>
        /// 【OneToMany】导航属性 {trytbTypeName}.{pnvName} 在 {tbrefCsName} 中没有找到对应的字段，如：{findtrytb}{findtrytbPkCsName}、{findtrytb}_{findtrytbPkCsName}
        /// </summary>
        public static string OneToMany_NotFound_CorrespondingField(object trytbTypeName, object pnvName, object tbrefCsName, object findtrytb, object findtrytbPkCsName) => Language == "cn" ?
            $@"【OneToMany】导航属性 {trytbTypeName}.{pnvName} 在 {tbrefCsName} 中没有找到对应的字段，如：{findtrytb}{findtrytbPkCsName}、{findtrytb}_{findtrytbPkCsName}" :
            $@"FreeSql: [OneToMany] Navigation property {trytbTypeName}.{pnvName} did not find a corresponding field in {tbrefCsName}, such as: {findtrytb}{findtrytbPkCsName}, {findtrytb}_{findtrytbPkCsName}";
        /// <summary>
        /// 【OneToMany】导航属性 {trytbTypeName}.{pnvName} 解析错误，{trytbCsName}.{trytbPrimarysCsName} 和 {tbrefCsName}.{trycolCsName} 类型不一致
        /// </summary>
        public static string OneToMany_ParsingError_InconsistentType(object trytbTypeName, object pnvName, object trytbCsName, object trytbPrimarysCsName, object tbrefCsName, object trycolCsName) => Language == "cn" ?
            $@"【OneToMany】导航属性 {trytbTypeName}.{pnvName} 解析错误，{trytbCsName}.{trytbPrimarysCsName} 和 {tbrefCsName}.{trycolCsName} 类型不一致" :
            $@"FreeSql: [OneToMany] Navigation property {trytbTypeName}.{pnvName} parsing error, {trytbCsName}.{trytbPrimarysCsName} and {tbrefCsName}.{trycolCsName} is of inconsistent type";
        /// <summary>
        /// 、{refpropName}{findtrytbPkCsName}、{refpropName}_{findtrytbPkCsName}。或者使用 [Navigate] 特性指定关系映射。
        /// </summary>
        public static string OneToMany_UseNavigate(object refpropName, object findtrytbPkCsName) => Language == "cn" ?
            $@"、{refpropName}{findtrytbPkCsName}、{refpropName}_{findtrytbPkCsName}。或者使用 [Navigate] 特性指定关系映射。" :
            $@", {refpropName}{findtrytbPkCsName}, {refpropName}_{findtrytbPkCsName}. Or use the [Navigate] attribute to specify the relationship mapping.";
        /// <summary>
        /// 参数 field 未指定
        /// </summary>
        public static string Parameter_Field_NotSpecified => Language == "cn" ?
            @"参数 field 未指定" :
            @"FreeSql: Parameter field not specified";
        /// <summary>
        /// {property} 参数错误，它不是集合属性，必须为 IList&amp;lt;T&amp;gt; 或者 ICollection&amp;lt;T&amp;gt;
        /// </summary>
        public static string ParameterError_NotValid_Collection(object property) => Language == "cn" ?
            $@"{property} 参数错误，它不是集合属性，必须为 IList<T> 或者 ICollection<T>" :
            $@"FreeSql: The {property} parameter is incorrect, it is not a collection property and must be IList<T>or ICollection<T>";
        /// <summary>
        /// {property} 参数错误，它不是有效的导航属性
        /// </summary>
        public static string ParameterError_NotValid_Navigation(object property) => Language == "cn" ?
            $@"{property} 参数错误，它不是有效的导航属性" :
            $@"FreeSql: The {property} parameter is incorrect, it is not a valid navigation property";
        /// <summary>
        /// {where} 参数错误，{keyval} 不是有效的属性名，在实体类 {reftbTypeDisplayCsharp} 无法找到
        /// </summary>
        public static string ParameterError_NotValid_PropertyName(object where, object keyval, object reftbTypeDisplayCsharp) => Language == "cn" ?
            $@"{where} 参数错误，{keyval} 不是有效的属性名，在实体类 {reftbTypeDisplayCsharp} 无法找到" :
            $@"FreeSql: {where} parameter error, {keyval} is not a valid property name and cannot be found in entity class {reftbTypeDisplayCsharp}";
        /// <summary>
        /// {property} 参数错误，格式 "TopicId=Id，多组使用逗号连接" 
        /// </summary>
        public static string ParameterError_NotValid_UseCommas(object property) => Language == "cn" ?
            $@"{property} 参数错误，格式 ""TopicId=Id，多组使用逗号连接"" " :
            $@"FreeSql: {property} parameter error, format ""TopicId=Id, multiple groups using comma connection""";
        /// <summary>
        /// 解析失败 {callExpMethodName} {message}
        /// </summary>
        public static string Parsing_Failed(object callExpMethodName, object message) => Language == "cn" ?
            $@"解析失败 {callExpMethodName} {message}" :
            $@"FreeSql: Parsing failed {callExpMethodName} {message}";
        /// <summary>
        /// 【{policyName}】The ObjectPool has been disposed, see: https://github.com/dotnetcore/FreeSql/discussions/1079
        /// </summary>
        public static string Policy_ObjectPool_Dispose(object policyName) => Language == "cn" ?
            $@"【{policyName}】The ObjectPool has been disposed, see: https://github.com/dotnetcore/FreeSql/discussions/1079" :
            $@"FreeSql: [{policyName}] The ObjectPool has been disposed, see: https://github.com/dotnetcore/FreeSql/discussions/1079";
        /// <summary>
        /// 【{policyName}】状态不可用，等待后台检查程序恢复方可使用。{UnavailableExceptionMessage}
        /// </summary>
        public static string Policy_Status_NotAvailable(object policyName, object UnavailableExceptionMessage) => Language == "cn" ?
            $@"【{policyName}】状态不可用，等待后台检查程序恢复方可使用。{UnavailableExceptionMessage}" :
            $@"FreeSql: The {policyName} status is unavailable and cannot be used until the background checker is restored. {UnavailableExceptionMessage}";
        /// <summary>
        /// 属性{trytbVersionColumnCsName} 被标注为行锁（乐观锁）(IsVersion)，但其必须为数字类型 或者 byte[] 或者 string，并且不可为 Nullable
        /// </summary>
        public static string Properties_AsRowLock_Must_Numeric_Byte(object trytbVersionColumnCsName) => Language == "cn" ?
            $@"属性{trytbVersionColumnCsName} 被标注为行锁（乐观锁）(IsVersion)，但其必须为数字类型 或者 byte[] 或者 string，并且不可为 Nullable" :
            $@"FreeSql: The property {trytbVersionColumnCsName} is labeled as a row lock (optimistic lock) (IsVersion), but it must be a numeric type or byte[] or string, and it cannot be Nullable";
        /// <summary>
        /// properties 参数不能为空
        /// </summary>
        public static string Properties_Cannot_Null => Language == "cn" ?
            @"properties 参数不能为空" :
            @"FreeSql: Properrties parameter cannot be empty";
        /// <summary>
        /// {property} 属性名无法找到
        /// </summary>
        public static string Property_Cannot_Find(object property) => Language == "cn" ?
            $@"{property} 属性名无法找到" :
            $@"FreeSql: {property} property name not found";
        /// <summary>
        /// Range 要求 Value 应该逗号分割，并且长度为 2
        /// </summary>
        public static string Range_Comma_Separateda_By2Char => Language == "cn" ?
            @"Range 要求 Value 应该逗号分割，并且长度为 2" :
            @"FreeSql: Range requires that Value be comma-separated and 2-length";
        /// <summary>
        /// 回滚
        /// </summary>
        public static string RollBack => Language == "cn" ?
            @"回滚" :
            @"FreeSql: RollBack";
        /// <summary>
        /// 运行时错误，反射获取 IncludeMany 方法失败
        /// </summary>
        public static string RunTimeError_Reflection_IncludeMany => Language == "cn" ?
            @"运行时错误，反射获取 IncludeMany 方法失败" :
            @"FreeSql: Runtime error, reflection failed to get IncludeMany method";
        /// <summary>
        /// {qoteSql} is NULL，除非设置特性 [Column(IsNullable = false)]
        /// </summary>
        public static string Set_Column_IsNullable_False(object qoteSql) => Language == "cn" ?
            $@"{qoteSql} is NULL，除非设置特性 [Column(IsNullable = false)]" :
            $@"FreeSql: {qoteSql} is NULL unless the attribute [Column (IsNullable = false)]";
        /// <summary>
        /// 分表字段值 "{dt}" 不能小于 "{beginTime} "
        /// </summary>
        public static string SubTableFieldValue_CannotLessThen(object dt, object beginTime) => Language == "cn" ?
            $@"分表字段值 ""{dt}"" 不能小于 ""{beginTime} """ :
            $@"FreeSql: Subtable field value'{dt}'cannot be less than'{beginTime}'";
        /// <summary>
        /// 分表字段值不能为 null
        /// </summary>
        public static string SubTableFieldValue_IsNotNull => Language == "cn" ?
            @"分表字段值不能为 null" :
            @"FreeSql: Subtable field value cannot be null";
        /// <summary>
        /// 分表字段值 "{columnValue}" 不能转化成 DateTime
        /// </summary>
        public static string SubTableFieldValue_NotConvertDateTime(object columnValue) => Language == "cn" ?
            $@"分表字段值 ""{columnValue}"" 不能转化成 DateTime" :
            $@"FreeSql: The tabular field value'{columnValue}'cannot be converted to DateTime";
        /// <summary>
        /// 分表字段值 "{dt}" 未匹配到分表名
        /// </summary>
        public static string SubTableFieldValue_NotMatchTable(object dt) => Language == "cn" ?
            $@"分表字段值 ""{dt}"" 未匹配到分表名" :
            $@"FreeSql: Table field value'{dt}'does not match table name";
        /// <summary>
        /// T2 类型错误
        /// </summary>
        public static string T2_Type_Error => Language == "cn" ?
            @"T2 类型错误" :
            @"FreeSql: Type T2 Error";
        /// <summary>
        /// tableName 格式错误，示例：“log_{yyyyMMdd}”
        /// </summary>
        public static string TableName_Format_Error(object yyyyMMdd) => Language == "cn" ?
            $@"tableName 格式错误，示例：“log_{yyyyMMdd}”" :
            $@"FreeSql: TableName format error, example: ""log_{yyyyMMdd}""";
        /// <summary>
        /// {Type}.AsType 参数错误，请传入正确的实体类型
        /// </summary>
        public static string Type_AsType_Parameter_Error(object Type) => Language == "cn" ?
            $@"{Type}.AsType 参数错误，请传入正确的实体类型" :
            $@"FreeSql: {Type}.AsType parameter error, please pass in the correct entity type";
        /// <summary>
        /// {thatFullName} 类型无法访问构造函数
        /// </summary>
        public static string Type_Cannot_Access_Constructor(object thatFullName) => Language == "cn" ?
            $@"{thatFullName} 类型无法访问构造函数" :
            $@"FreeSql: The {thatFullName} type cannot access the constructor";
        /// <summary>
        /// {name} 类型错误
        /// </summary>
        public static string Type_Error_Name(object name) => Language == "cn" ?
            $@"{name} 类型错误" :
            $@"FreeSql: {name} type error";
        /// <summary>
        /// {Type}.AsType 参数不支持指定为 object
        /// </summary>
        public static string TypeAsType_NotSupport_Object(object Type) => Language == "cn" ?
            $@"{Type}.AsType 参数不支持指定为 object" :
            $@"FreeSql: {Type}.AsType parameter does not support specifying as object";
        /// <summary>
        /// 类型 {typeofFullName} 错误，不能使用 IncludeMany
        /// </summary>
        public static string TypeError_CannotUse_IncludeMany(object typeofFullName) => Language == "cn" ?
            $@"类型 {typeofFullName} 错误，不能使用 IncludeMany" :
            $@"FreeSql: Type {typeofFullName} error, IncludeMany cannot be used";
        /// <summary>
        /// 无法解析表达式：{exp}
        /// </summary>
        public static string Unable_Parse_Expression(object exp) => Language == "cn" ?
            $@"无法解析表达式：{exp}" :
            $@"FreeSql: Unable to parse expression: {exp}";
        /// <summary>
        /// 无法解析表达式方法 {exp3tmpCallMethodName}
        /// </summary>
        public static string Unable_Parse_ExpressionMethod(object exp3tmpCallMethodName) => Language == "cn" ?
            $@"无法解析表达式方法 {exp3tmpCallMethodName}" :
            $@"FreeSql: Unable to parse expression method {exp3tmpCallMethodName}";
        /// <summary>
        /// 请使用 fsql.InsertDict(dict) 方法插入字典数据
        /// </summary>
        public static string Use_InsertDict_Method => Language == "cn" ?
            @"请使用 fsql.InsertDict(dict) 方法插入字典数据" :
            @"FreeSql: Please use fsql. InsertDict (dict) method inserts dictionary data";
        /// <summary>
        /// 找不到 {name}
        /// </summary>
        public static string S_NotFound_Name(object name) => Language == "cn" ?
            $@"找不到 {name}" :
            $@"FreeSql: {name} not found";
        /// <summary>
        /// 从库
        /// </summary>
        public static string S_SlaveDatabase => Language == "cn" ?
            @"从库" :
            @"FreeSql: Slave Database";
        /// <summary>
        /// 主库
        /// </summary>
        public static string S_MasterDatabase => Language == "cn" ?
            @"主库" :
            @"FreeSql: Master Database";
        /// <summary>
        /// 蛋疼的 Access 插入只能一条一条执行，不支持 values(..),(..) 也不支持 select .. UNION ALL select ..
        /// </summary>
        public static string S_Access_InsertOnlyOneAtTime => Language == "cn" ?
            @"蛋疼的 Access 插入只能一条一条执行，不支持 values(..),(..) 也不支持 select .. UNION ALL select .." :
            @"FreeSql: Egg pain Accs insertion can only be performed one at a time, values (..) are not supported. (..) Select is also not supported.. UNION ALL select..";
        /// <summary>
        /// BaseEntity.Initialization 初始化错误，获取到 IFreeSql 是 null
        /// </summary>
        public static string S_BaseEntity_Initialization_Error => Language == "cn" ?
            @"BaseEntity.Initialization 初始化错误，获取到 IFreeSql 是 null" :
            @"FreeSql: BaseEntity. Initialization initialization error, get IFreeSql is null";
        /// <summary>
        /// 【{thisName}】Block access and wait for recovery: {exMessage}
        /// </summary>
        public static string S_BlockAccess_WaitForRecovery(object thisName, object exMessage) => Language == "cn" ?
            $@"【{thisName}】Block access and wait for recovery: {exMessage}" :
            $@"FreeSql: [{thisName}] Block access and wait for recovery: {exMessage}";
        /// <summary>
        /// 无法将 IQueryable&amp;lt;{typeofName}&amp;gt; 转换为 ISelect&amp;lt;{typeofName}&amp;gt;，因为他的实现不是 FreeSql.Extensions.Linq.QueryableProvider
        /// </summary>
        public static string S_CannotBeConverted_To_ISelect(object typeofName) => Language == "cn" ?
            $@"无法将 IQueryable<{typeofName}> 转换为 ISelect<{typeofName}>，因为他的实现不是 FreeSql.Extensions.Linq.QueryableProvider" :
            $@"FreeSql: IQueryable<{typeofName}> cannot be converted to ISelect<{typeofName}> because its implementation is not FreeSql.Extensions.Linq.QueryableProvider";
        /// <summary>
        /// 连接字符串错误
        /// </summary>
        public static string S_ConnectionStringError => Language == "cn" ?
            @"连接字符串错误" :
            @"FreeSql: Connection string error";
        /// <summary>
        /// 【{thisName}】连接字符串错误，请检查。
        /// </summary>
        public static string S_ConnectionStringError_Check(object thisName) => Language == "cn" ?
            $@"【{thisName}】连接字符串错误，请检查。" :
            $@"FreeSql: [{thisName}] Connection string error, please check.";
        /// <summary>
        /// 连接字符串错误，或者检查项目属性 &amp;gt; 生成 &amp;gt; 目标平台：x86 | x64，或者改用 FreeSql.Provider.SqliteCore 访问 arm 平台
        /// </summary>
        public static string S_ConnectionStringError_CheckProject => Language == "cn" ?
            @"连接字符串错误，或者检查项目属性 > 生成 > 目标平台：x86 | x64，或者改用 FreeSql.Provider.SqliteCore 访问 arm 平台" :
            @"FreeSql: Connection string error, or check project properties > Build > Target Platform: x86 | x64, Or use FreeSql.Provider.SqliteCore accessing arm platform";
        /// <summary>
        /// 【{thisName}】连接字符串错误，请检查。或者检查项目属性 &amp;gt; 生成 &amp;gt; 目标平台：x86 | x64，或者改用 FreeSql.Provider.SqliteCore 访问 arm 平台
        /// </summary>
        public static string S_ConnectionStringError_CheckProjectConnection(object thisName) => Language == "cn" ?
            $@"【{thisName}】连接字符串错误，请检查。或者检查项目属性 > 生成 > 目标平台：x86 | x64，或者改用 FreeSql.Provider.SqliteCore 访问 arm 平台" :
            $@"FreeSql: [{thisName}] Connection string error, please check. Or check Project Properties > Build > Target Platform: x86 | x64, Or use FreeSql.Provider.SqliteCore accessing arm platform";
        /// <summary>
        /// FreeSql.Provider.CustomAdapter 无法使用 CreateCommand
        /// </summary>
        public static string S_CustomAdapter_Cannot_Use_CreateCommand => Language == "cn" ?
            @"FreeSql.Provider.CustomAdapter 无法使用 CreateCommand" :
            @"FreeSql: FreeSql.Provider.CustomAdapter cannot use CreateCommand";
        /// <summary>
        /// FreeSql.Provider.CustomAdapter 仅支持 UseConnectionFactory 方式构建 IFreeSql
        /// </summary>
        public static string S_CustomAdapter_OnlySuppport_UseConnectionFactory => Language == "cn" ?
            @"FreeSql.Provider.CustomAdapter 仅支持 UseConnectionFactory 方式构建 IFreeSql" :
            @"FreeSql: FreeSql.Provider.CustomAdapter only supports building IFreeSql in the UseConnectionFactory way";
        /// <summary>
        /// 达梦 CodeFirst 不支持代码创建 tablespace 与 schemas {tbname}
        /// </summary>
        public static string S_Dameng_NotSupport_TablespaceSchemas(object tbname) => Language == "cn" ?
            $@"达梦 CodeFirst 不支持代码创建 tablespace 与 schemas {tbname}" :
            $@"FreeSql: Dream CodeFirst does not support code creation tablespace and schemas {tbname}";
        /// <summary>
        /// -DB 参数错误，未提供 ConnectionString
        /// </summary>
        public static string S_DB_Parameter_Error_NoConnectionString => Language == "cn" ?
            @"-DB 参数错误，未提供 ConnectionString" :
            @"FreeSql: -DB parameter error, no ConnectionString provided";
        /// <summary>
        /// -DB 参数错误，格式为：MySql,ConnectionString
        /// </summary>
        public static string S_DB_ParameterError => Language == "cn" ?
            @"-DB 参数错误，格式为：MySql,ConnectionString" :
            @"FreeSql: -DB parameter error, format: MySql, ConnectionString";
        /// <summary>
        /// -DB 参数错误，不支持的类型："{dbargs}"
        /// </summary>
        public static string S_DB_ParameterError_UnsupportedType(object dbargs) => Language == "cn" ?
            $@"-DB 参数错误，不支持的类型：""{dbargs}""" :
            $@"FreeSql: -DB parameter error, unsupported type: ""{dbargs}""";
        /// <summary>
        /// {method} 是 FreeSql.Provider.{provider} 特有的功能
        /// </summary>
        public static string S_Features_Unique(object method, object provider) => Language == "cn" ?
            $@"{method} 是 FreeSql.Provider.{provider} 特有的功能" :
            $@"FreeSql: {method} is FreeSql.Provider.{provider} specific features";
        /// <summary>
        /// fsql.InsertOrUpdate Sqlite 无法完成 UpdateColumns 操作
        /// </summary>
        public static string S_InsertOrUpdate_Unable_UpdateColumns => Language == "cn" ?
            @"fsql.InsertOrUpdate Sqlite 无法完成 UpdateColumns 操作" :
            @"FreeSql: InsertOrUpdate Sqlite was unable to complete the UpdateColumns operation";
        /// <summary>
        /// MygisGeometry.Parse 未实现 "{wkt}"
        /// </summary>
        public static string S_MygisGeometry_NotImplement(object wkt) => Language == "cn" ?
            $@"MygisGeometry.Parse 未实现 ""{wkt}""" :
            $@"FreeSql: MygisGeometry. Parse does not implement ""{wkt}""";
        /// <summary>
        /// -NameOptions 参数错误，格式为：0,0,0,0
        /// </summary>
        public static string S_NameOptions_Incorrect => Language == "cn" ?
            @"-NameOptions 参数错误，格式为：0,0,0,0" :
            @"FreeSql: -NameOptions parameter incorrect, format: 0,0,0,0";
        /// <summary>
        ///  未实现该功能
        /// </summary>
        public static string S_Not_Implemented_Feature => Language == "cn" ?
            @" 未实现该功能" :
            @"FreeSql: This function is not implemented";
        /// <summary>
        /// 未实现错误，请反馈给作者
        /// </summary>
        public static string S_Not_Implemented_FeedBack => Language == "cn" ?
            @"未实现错误，请反馈给作者" :
            @"FreeSql: Unrealized error, please feedback to author";
        /// <summary>
        /// FreeSql.Provider.{providerName} 未实现 Skip/Offset 功能，如果需要分页请使用判断上一次 id
        /// </summary>
        public static string S_NotImplementSkipOffset(object providerName) => Language == "cn" ?
            $@"FreeSql.Provider.{providerName} 未实现 Skip/Offset 功能，如果需要分页请使用判断上一次 id" :
            $@"FreeSql: FreeSql.Provider.{providerName} does not implement Skip/Offset functionality, use to determine last ID if paging is required";
        /// <summary>
        /// 旧表(OldName)：{tboldname} 存在，数据库已存在 {tbname} 表，无法改名
        /// </summary>
        public static string S_OldTableExists(object tboldname, object tbname) => Language == "cn" ?
            $@"旧表(OldName)：{tboldname} 存在，数据库已存在 {tbname} 表，无法改名" :
            $@"FreeSql: Old table (OldName): {tboldname} exists, database already exists {tbname} table, cannot rename";
        /// <summary>
        /// OnConflictDoUpdate 功能要求实体类必须设置 IsPrimary 属性
        /// </summary>
        public static string S_OnConflictDoUpdate_MustIsPrimary => Language == "cn" ?
            @"OnConflictDoUpdate 功能要求实体类必须设置 IsPrimary 属性" :
            @"FreeSql: The OnConflictDoUpdate feature requires that entity classes must set the IsPrimary property";
        /// <summary>
        /// Oracle CodeFirst 不支持代码创建 tablespace 与 schemas {tbname}
        /// </summary>
        public static string S_Oracle_NotSupport_TablespaceSchemas(object tbname) => Language == "cn" ?
            $@"Oracle CodeFirst 不支持代码创建 tablespace 与 schemas {tbname}" :
            $@"FreeSql: Oracle CodeFirst does not support code creation of tablespace and schemas {tbname}";
        /// <summary>
        /// 解析失败 {callExpMethodName} {message}，提示：可以使用扩展方法 IQueryable.RestoreToSelect() 还原为 ISelect 再查询
        /// </summary>
        public static string S_ParsingFailed_UseRestoreToSelect(object callExpMethodName, object message) => Language == "cn" ?
            $@"解析失败 {callExpMethodName} {message}，提示：可以使用扩展方法 IQueryable.RestoreToSelect() 还原为 ISelect 再查询" :
            $@"FreeSql: Parsing failed {callExpMethodName} {message}, hint: Extension method IQueryable can be used. RestoreToSelect() reverted to ISelect re-query";
        /// <summary>
        /// fsql.InsertOrUpdate + IfExistsDoNothing + {providerName}要求实体类 {tableCsName} 必须有主键
        /// </summary>
        public static string S_RequiresEntityPrimaryKey(object providerName, object tableCsName) => Language == "cn" ?
            $@"fsql.InsertOrUpdate + IfExistsDoNothing + {providerName}要求实体类 {tableCsName} 必须有主键" :
            $@"FreeSql: InsertOrUpdate + IfExistsDoNothing + {providerName} requires the entity class {tableCsName} to have a primary key";
        /// <summary>
        /// SelectMany 错误的类型：{typeFullName}
        /// </summary>
        public static string S_SelectManayErrorType(object typeFullName) => Language == "cn" ?
            $@"SelectMany 错误的类型：{typeFullName}" :
            $@"FreeSql: SelectMany error type: {typeFullName}";
        /// <summary>
        /// 类型 {objentityTypeFullName} 不可迁移
        /// </summary>
        public static string S_Type_IsNot_Migrable(object objentityTypeFullName) => Language == "cn" ?
            $@"类型 {objentityTypeFullName} 不可迁移" :
            $@"FreeSql: Type {objentityTypeFullName} is not migrable";
        /// <summary>
        /// 类型 {objentityTypeFullName} 不可迁移，可迁移属性0个
        /// </summary>
        public static string S_Type_IsNot_Migrable_0Attributes(object objentityTypeFullName) => Language == "cn" ?
            $@"类型 {objentityTypeFullName} 不可迁移，可迁移属性0个" :
            $@"FreeSql: Type {objentityTypeFullName} is not migrable, migratable property 0";
        /// <summary>
        /// 未实现 {columnDbTypeTextFull} 类型映射
        /// </summary>
        public static string S_TypeMappingNotImplemented(object columnDbTypeTextFull) => Language == "cn" ?
            $@"未实现 {columnDbTypeTextFull} 类型映射" :
            $@"FreeSql: {columnDbTypeTextFull} type mapping not implemented";
        /// <summary>
        /// 错误的参数设置：{args}
        /// </summary>
        public static string S_WrongParameter(object args) => Language == "cn" ?
            $@"错误的参数设置：{args}" :
            $@"FreeSql: Wrong parameter setting: {args}";
        /// <summary>
        /// 对象池
        /// </summary>
        public static string S_ObjectPool => Language == "cn" ?
            @"对象池" :
            @"FreeSql: Object pool";
    }
}

/*
var xml1 = `<xml id="xml1">
  <data name="AsTable_PropertyName_FormatError" xml:space="preserve">
    <value>FreeSql: [Table(AsTable="{asTable}")] Property value formatted incorrectly</value>
  </data>
  <data name="AsTable_PropertyName_NotDateTime" xml:space="preserve">
    <value>FreeSql: The property name {atmGroupsValue} set by [Table (AsTable = xx)] is not of type DateTime</value>
  </data>
  <data name="Available_Failed_Get_Resource" xml:space="preserve">
    <value>FreeSql: {name}: Failed to get resource {statistics}</value>
  </data>
  <data name="Available_Thrown_Exception" xml:space="preserve">
    <value>FreeSql: {name}: An exception needs to be thrown</value>
  </data>
  <data name="Bad_Expression_Format" xml:space="preserve">
    <value>FreeSql: Wrong expression format {column}</value>
  </data>
  <data name="Before_Chunk_Cannot_Use_Select" xml:space="preserve">
    <value>FreeSql: Select is not available until the Chunk function</value>
  </data>
  <data name="Begin_Transaction_Then_ForUpdate" xml:space="preserve">
    <value>FreeSql: For security reasons, be sure to use ForUpdate after the transaction is open</value>
  </data>
  <data name="Cannot_Be_NULL" xml:space="preserve">
    <value>FreeSql: Cannot be null</value>
  </data>
  <data name="Cannot_Be_NULL_Name" xml:space="preserve">
    <value>FreeSql: {name} cannot be null</value>
  </data>
  <data name="Cannot_Match_Property" xml:space="preserve">
    <value>FreeSql: Unable to match {property}</value>
  </data>
  <data name="Cannot_Resolve_ExpressionTree" xml:space="preserve">
    <value>FreeSql: {property} cannot be resolved to an expression tree</value>
  </data>
  <data name="Check_UseConnectionString" xml:space="preserve">
    <value>FreeSql: The parameter master ConnectionString cannot be empty, check UseConnectionString</value>
  </data>
  <data name="Commit" xml:space="preserve">
    <value>FreeSql: Commit</value>
  </data>
  <data name="Connection_Failed_Switch_Servers" xml:space="preserve">
    <value>FreeSql: Connection failed, ready to switch other available servers</value>
  </data>
  <data name="Custom_Expression_ParsingError" xml:space="preserve">
    <value>FreeSql: Custom expression parsing error: type {exp3MethodDeclaringType} needs to define static ThreadLocal&lt;ExpressionCallContext&gt;field, field, field (important three reminders)</value>
  </data>
  <data name="Custom_Reflection_IsNotNull" xml:space="preserve">
    <value>FreeSql: Custom {Reflection Information} cannot be empty, format: {static method name}{space}{reflection information}</value>
  </data>
  <data name="Custom_StaticMethodName_IsNotNull" xml:space="preserve">
    <value>FreeSql: Custom {static method name} cannot be empty, format: {static method name}{space}{reflection information}</value>
  </data>
  <data name="Custom_StaticMethodName_NotSet_DynamicFilterCustom" xml:space="preserve">
    <value>FreeSql: Custom corresponding {{static method name}}:{fiValueCustomArray} The [DynamicFilterCustomAttribute] attribute is not set</value>
  </data>
  <data name="CustomFieldSeparatedBySpaces" xml:space="preserve">
    <value>FreeSql: Custom requires that Fields be space-split and 2-length in the format: {static method name}{space}{reflection information}</value>
  </data>
  <data name="DataType_AsType_Inconsistent" xml:space="preserve">
    <value>FreeSql: The data type of the operation ({dataDisplayCsharp}) is inconsistent with AsType ({tableTypeDisplayCsharp}). Please check.</value>
  </data>
  <data name="DateRange_Comma_Separateda_By2Char" xml:space="preserve">
    <value>FreeSql: DateRange requires that Value be comma-separated and 2-length</value>
  </data>
  <data name="DateRange_DateFormat_yyyy" xml:space="preserve">
    <value>FreeSql: DateRange requires that the Value [1] format must be: yyyy, yyyy-MM, yyyy-MM-dd, yyyyy-MM-dd HH, yyyy, yyyy-MM-dd HH:mm</value>
  </data>
  <data name="DbUpdateVersionException_RowLevelOptimisticLock" xml:space="preserve">
    <value>FreeSql: The record may not exist, or the row level optimistic lock version is out of date, the number of updates {sourceCount}, the number of rows affected {affrows}.</value>
  </data>
  <data name="Different_Number_SlaveConnectionString_SlaveWeights" xml:space="preserve">
    <value>FreeSql: The number of SlaveConnectionStrings is not the same as SlaveWeights</value>
  </data>
  <data name="Duplicate_ColumnAttribute" xml:space="preserve">
    <value>FreeSql: ColumnAttribute. Name {colattrName} exists repeatedly, please check (note: case insensitive)</value>
  </data>
  <data name="Duplicate_PropertyName" xml:space="preserve">
    <value>FreeSql: Property name {pName} exists repeatedly, please check (note: case insensitive)</value>
  </data>
  <data name="Entity_Must_Primary_Key" xml:space="preserve">
    <value>FreeSql: The {function} feature requires that the entity class {tableCsName} must have a primary key</value>
  </data>
  <data name="Entity_MySQL_VersionsBelow8_NotSupport_Multiple_PrimaryKeys" xml:space="preserve">
    <value>FreeSql: {tbTypeFullName} is a parent-child relationship, but combinations of multiple primary keys are not supported in versions below MySql 8.0</value>
  </data>
  <data name="Entity_NotParentChild_Relationship" xml:space="preserve">
    <value>FreeSql: {tbTypeFullName} is not a parent-child relationship and cannot be used</value>
  </data>
  <data name="EspeciallySubquery_Cannot_Parsing" xml:space="preserve">
    <value>FreeSql: This particular subquery cannot be resolved</value>
  </data>
  <data name="Expression_Error_Use_ParameterExpression" xml:space="preserve">
    <value>FreeSql: Expression error, its top object is not ParameterExpression:{exp}</value>
  </data>
  <data name="Expression_Error_Use_Successive_MemberAccess_Type" xml:space="preserve">
    <value>FreeSql: Expression error, it is not a continuous MemberAccess type: {exp}</value>
  </data>
  <data name="ExpressionTree_Convert_Type_Error" xml:space="preserve">
    <value>FreeSql: ExpressionTree conversion type error, value ({value}), type ({valueTypeFullName}), target type ({typeFullName}), Error:{exMessage}</value>
  </data>
  <data name="Failed_SubTable_FieldValue" xml:space="preserve">
    <value>FreeSql: Failed to parse table field value {sqlWhere}</value>
  </data>
  <data name="Functions_AsTable_NotImplemented" xml:space="preserve">
    <value>FreeSql: Function {asTable} not implemented by AsTable</value>
  </data>
  <data name="GBase_NotSupport_OtherThanCommas" xml:space="preserve">
    <value>FreeSql: GBase does not support separators other than commas at this time</value>
  </data>
  <data name="Generated_Same_SubTable" xml:space="preserve">
    <value>FreeSql: TableName:{tableName} generated the same table name</value>
  </data>
  <data name="GetPrimarys_ParameterError_IsNotDictKey " xml:space="preserve">
    <value>FreeSql: The parameter'{primary}'passed by GetPrimarys is incorrect and does not belong to the key name of the dictionary data</value>
  </data>
  <data name="Has_Specified_Cannot_Specified_Second" xml:space="preserve">
    <value>FreeSql: {first} has already been specified and {second} can no longer be specified</value>
  </data>
  <data name="Ignored_Check_Confirm_PublicGetSet" xml:space="preserve">
    <value>FreeSql: {tb2DbName}. {mp2MemberName} is ignored. Check the IsIgnore setting to make sure get/set is public</value>
  </data>
  <data name="Include_ParameterType_Error" xml:space="preserve">
    <value>FreeSql: Include parameter type error</value>
  </data>
  <data name="Include_ParameterType_Error_Use_IncludeMany" xml:space="preserve">
    <value>FreeSql: Include parameter type is wrong, use IncludeMany for collection properties</value>
  </data>
  <data name="Include_ParameterType_Error_Use_MemberAccess" xml:space="preserve">
    <value>FreeSql: Include parameter type is wrong, expression type should be MemberAccess</value>
  </data>
  <data name="IncludeMany_NotValid_Navigation" xml:space="preserve">
    <value>FreeSql: The property {collMemMemberName} of IncludeMany type {tbTypeDisplayCsharp} is not a valid navigation property, hint: IsIgnore = true will not be a navigation property</value>
  </data>
  <data name="IncludeMany_ParameterError_OnlyUseOneParameter" xml:space="preserve">
    <value>FreeSql: IncludeMany {navigateSelector} parameter is wrong, Select can only use one parameter's method, the correct format:.Select(t =&gt;new TNavigate{{}})</value>
  </data>
  <data name="IncludeMany_ParameterError_Select_ReturnConsistentType" xml:space="preserve">
    <value>FreeSql: IncludeMany {navigateSelector} parameter error, Select lambda parameter return value must match {collMemElementType} type</value>
  </data>
  <data name="IncludeMany_ParameterType_Error_Use_MemberAccess" xml:space="preserve">
    <value>FreeSql: IncludeMany parameter 1 has wrong type, expression type should be MemberAccess</value>
  </data>
  <data name="IncludeMany_ParameterTypeError" xml:space="preserve">
    <value>FreeSql: IncludeMany {navigateSelector} parameter type is wrong, correct format: a.collections.Take(1).Where(c =&gt; C.A ID == a.id).Select (a =&gt; new TNavigate{{}})</value>
  </data>
  <data name="InsertInto_No_Property_Selected" xml:space="preserve">
    <value>FreeSql: ISelect. InsertInto() did not select an attribute: {displayCsharp}</value>
  </data>
  <data name="InsertInto_TypeError" xml:space="preserve">
    <value>FreeSql: ISelect. InsertInto() type error: {displayCsharp}</value>
  </data>
  <data name="InsertOrUpdate_Must_Primary_Key" xml:space="preserve">
    <value>FreeSql: The InsertOrUpdate function performs merge into requiring the entity class {CsName} to have a primary key</value>
  </data>
  <data name="InsertOrUpdate_NotSuport_Generic_UseEntity" xml:space="preserve">
    <value>FreeSql: The generic parameter for InsertOrUpdate&lt;&gt;does not support {typeofT1}. Pass in your entity class</value>
  </data>
  <data name="Install_FreeSql_Extensions_LazyLoading" xml:space="preserve">
    <value>FreeSql: FreeSql needs to be installed for Delayed Loading. Extensions. LazyLoading. Dll, downloadable to nuget</value>
  </data>
  <data name="LazyLoading_CompilationError" xml:space="preserve">
    <value>FreeSql: {trytbTypeName} Compilation error: {exMessage}\r\n\r\n{cscode}</value>
  </data>
  <data name="LazyLoading_EntityMustDeclarePublic" xml:space="preserve">
    <value>FreeSql: Entity type {trytbTypeName} must be declared public</value>
  </data>
  <data name="ManyToMany_AsSelect_NotSupport_Sum_Avg_etc" xml:space="preserve">
    <value>FreeSql: ManyToMany navigation properties. AsSelect() is temporarily unavailable for the Sum/Avg/Max/Min/First/ToOne/ToList method</value>
  </data>
  <data name="ManyToMany_NotFound_CorrespondingField" xml:space="preserve">
    <value>FreeSql: [ManyToMany] Navigation property {trytbTypeName}. {pnvName} did not find a corresponding field in {tbmidCsName}, such as: {midTypePropsTrytbName}{findtrytbPkCsName}, {midTypePropsTrytbName}_ {findtrytbPkCsName}</value>
  </data>
  <data name="ManyToMany_ParsingError_EntityMissing_PrimaryKey" xml:space="preserve">
    <value>FreeSql: [ManyToMany] Navigation property {trytbTypeName}. {pnvName} parsing error, entity type {tbrefTypeName} missing primary key identity, [Column (IsPrimary = true)]</value>
  </data>
  <data name="ManyToMany_ParsingError_EntityMustHas_NavigateCollection" xml:space="preserve">
    <value>FreeSql: [ManyToMany] Navigation property {trytbTypeName}. {pnvName} parsing error, entity type {tbrefTypeName} must have a corresponding [Navigate (ManyToMany = x)] collection property</value>
  </data>
  <data name="ManyToMany_ParsingError_InconsistentType" xml:space="preserve">
    <value>FreeSql: [ManyToMany] Navigation property {trytbTypeName}. {pnvName} parsing error, {tbmidCsName}. {trycolCsName} and {trytbCsName}. {trytbPrimarysCsName} type inconsistent</value>
  </data>
  <data name="ManyToMany_ParsingError_IntermediateClass_ErrorMessage" xml:space="preserve">
    <value>FreeSql: [ManyToMany] Navigation property {trytbTypeName}. {pnvName} parsing error, intermediate class {tbmidCsName}.{midTypePropsTrytbName} Error: {exMessage}</value>
  </data>
  <data name="ManyToMany_ParsingError_IntermediateClass_NotManyToOne_OneToOne" xml:space="preserve">
    <value>FreeSql: [ManyToMany] Navigation property {trytbTypeName}. {pnvName} parsing error, intermediate class {tbmidCsName}. The {midTypePropsTrytbName} navigation property is not ManyToOne or OneToOne</value>
  </data>
  <data name="Mapping_Exception_HasNo_SamePropertyName" xml:space="preserve">
    <value>FreeSql: Mapping exception: {name} None of the property names are the same</value>
  </data>
  <data name="MasterPool_IsNull_UseTransaction" xml:space="preserve">
    <value>FreeSql: Ado. MasterPool value is null, this operation cannot self-enable transactions, please explicitly pass [transaction object] resolution</value>
  </data>
  <data name="Missing_FreeSqlProvider_Package" xml:space="preserve">
    <value>FreeSql: Missing FreeSql database implementation package: FreeSql.Provider.{Provider}.Dll, downloadable to nuget</value>
  </data>
  <data name="Missing_FreeSqlProvider_Package_Reason" xml:space="preserve">
    <value>FreeSql: The FreeSql database implementation package is missing: {dll} can be downloaded to nuget; If there is {dll} and an error still occurs (due to environmental issues that cause the type to be unreflected), manually pass in typeof ({providerType}) in the third parameter of UseConnectionString/UseConnectionFactory</value>
  </data>
  <data name="Navigation_Bind_Number_Different" xml:space="preserve">
    <value>FreeSql: Navigation property {trytbTypeName}. The number of {pnvName} attributes [Navigate] Binds ({bindColumnsCount}) is different from the number of external primary keys ({tbrefPrimarysLength})</value>
  </data>
  <data name="Navigation_Missing_AsSelect" xml:space="preserve">
    <value>FreeSql: {tb2DbName}. {mp2MemberName} Navigation Property Collection forgotten. AsSelect()? If used in ToList (a =&gt; a. {mp2MemberName}), step by step to refer to the IncludeMany document.</value>
  </data>
  <data name="Navigation_Missing_SetProperty" xml:space="preserve">
    <value>FreeSql: Navigation Properties {trytbTypeDisplayCsharp}. Missing set attribute for {pName}</value>
  </data>
  <data name="Navigation_NotFound_CorrespondingField" xml:space="preserve">
    <value>FreeSql: Navigation property {trytbTypeName}. {pnvName} No corresponding fields were found, such as: {pnvName}{findtbrefPkCsName}, {pnvName}_ {findtbrefPkCsName}. Or use the [Navigate] attribute to specify the relationship mapping.</value>
  </data>
  <data name="Navigation_ParsingError_EntityMissingPrimaryKey" xml:space="preserve">
    <value>FreeSql: Navigation property {trytbTypeName}. {pnvName} parsing error, entity type {trytcTypeName} missing primary key identity, [Column (IsPrimary = true)]</value>
  </data>
  <data name="Navigation_ParsingError_InconsistentType" xml:space="preserve">
    <value>FreeSql: Navigation property {trytbTypeName}. {pnvName} parsing error, {trytbCsName}. {trycolCsName} and {tbrefCsName}. {tbrefPrimarysCsName} type inconsistent</value>
  </data>
  <data name="Navigation_ParsingError_NotFound_Property" xml:space="preserve">
    <value>FreeSql: Navigation property {trytbTypeName}. {pnvName} attribute [Navigate] parsing error, property not found at {tbrefTypeName}: {bi}</value>
  </data>
  <data name="NoPrimaryKey_UseSetDto" xml:space="preserve">
    <value>FreeSql: {tableTypeDisplayCsharp} has no primary key defined and cannot use SetSource. Try SetDto</value>
  </data>
  <data name="NoProperty_Defined" xml:space="preserve">
    <value>FreeSql: No properties defined</value>
  </data>
  <data name="Not_Implemented" xml:space="preserve">
    <value>FreeSql: Not implemented</value>
  </data>
  <data name="Not_Implemented_Expression" xml:space="preserve">
    <value>FreeSql: Function expression {exp} parsing not implemented</value>
  </data>
  <data name="Not_Implemented_Expression_ParameterUseConstant" xml:space="preserve">
    <value>FreeSql: Function expression {exp} parsing not implemented, parameter {expArguments} must be constant</value>
  </data>
  <data name="Not_Implemented_Expression_UseAsSelect" xml:space="preserve">
    <value>FreeSql: Function expression {exp} parsing is not implemented. Use if you are working on a navigation property collection. AsSelect (). {exp3MethodName} ({exp3ArgumentsCount})</value>
  </data>
  <data name="Not_Implemented_MemberAcess_Constant" xml:space="preserve">
    <value>FreeSql: Constant under MemberAccess is not implemented</value>
  </data>
  <data name="Not_Implemented_Name" xml:space="preserve">
    <value>FreeSql: {name} is not implemented</value>
  </data>
  <data name="Not_Support" xml:space="preserve">
    <value>FreeSql: I won't support it</value>
  </data>
  <data name="Not_Support_OrderByRandom" xml:space="preserve">
    <value>FreeSql: {dataType} does not support OrderByRandom sorting</value>
  </data>
  <data name="Not_Valid_Navigation_Property" xml:space="preserve">
    <value>FreeSql: {property} is not a valid navigation property</value>
  </data>
  <data name="NotFound_Column" xml:space="preserve">
    <value>FreeSql: {dbName} Column {memberName} not found</value>
  </data>
  <data name="NotFound_CsName_Column" xml:space="preserve">
    <value>FreeSql: Cannot find the column corresponding to {CsName}</value>
  </data>
  <data name="NotFound_Property" xml:space="preserve">
    <value>FreeSql: Attribute not found: {memberName}</value>
  </data>
  <data name="NotFound_PropertyName" xml:space="preserve">
    <value>FreeSql: Property name {proto} not found</value>
  </data>
  <data name="NotFound_Reflection" xml:space="preserve">
    <value>FreeSql: Custom could not find the corresponding {{reflection information}}:{fiValueCustomArray}</value>
  </data>
  <data name="NotFound_Static_MethodName" xml:space="preserve">
    <value>FreeSql: Custom could not find the corresponding {{static method name}}:{fiValueCustomArray}</value>
  </data>
  <data name="NotFound_Table_Property_AsTable" xml:space="preserve">
    <value>FreeSql: The property name {atmGroupsValue} set by [Table(AsTable = xx)] does not exist</value>
  </data>
  <data name="NotSpecified_UseConnectionString_UseConnectionFactory" xml:space="preserve">
    <value>FreeSql: No UseConnectionString or UseConnectionFactory specified</value>
  </data>
  <data name="ObjectPool_Get_Timeout" xml:space="preserve">
    <value>FreeSql: [{policyName}] ObjectPool. {GetName}() timeout {totalSeconds} seconds, see: https://github.com/dotnetcore/FreeSql/discussions/1081</value>
  </data>
  <data name="ObjectPool_GetAsync_Queue_Long" xml:space="preserve">
    <value>FreeSql: [{policyName}] ObjectPool. GetAsync() The queue is too long. Policy. AsyncGetCapacity = {asyncGetCapacity}</value>
  </data>
  <data name="OneToMany_NotFound_CorrespondingField" xml:space="preserve">
    <value>FreeSql: [OneToMany] Navigation property {trytbTypeName}.{pnvName} did not find a corresponding field in {tbrefCsName}, such as: {findtrytb}{findtrytbPkCsName}, {findtrytb}_{findtrytbPkCsName}</value>
  </data>
  <data name="OneToMany_ParsingError_InconsistentType" xml:space="preserve">
    <value>FreeSql: [OneToMany] Navigation property {trytbTypeName}.{pnvName} parsing error, {trytbCsName}.{trytbPrimarysCsName} and {tbrefCsName}.{trycolCsName} is of inconsistent type</value>
  </data>
  <data name="OneToMany_UseNavigate" xml:space="preserve">
    <value>, {refpropName}{findtrytbPkCsName}, {refpropName}_{findtrytbPkCsName}. Or use the [Navigate] attribute to specify the relationship mapping.</value>
  </data>
  <data name="Parameter_Field_NotSpecified" xml:space="preserve">
    <value>FreeSql: Parameter field not specified</value>
  </data>
  <data name="ParameterError_NotValid_Collection" xml:space="preserve">
    <value>FreeSql: The {property} parameter is incorrect, it is not a collection property and must be IList&lt;T&gt;or ICollection&lt;T&gt;</value>
  </data>
  <data name="ParameterError_NotValid_Navigation" xml:space="preserve">
    <value>FreeSql: The {property} parameter is incorrect, it is not a valid navigation property</value>
  </data>
  <data name="ParameterError_NotValid_PropertyName" xml:space="preserve">
    <value>FreeSql: {where} parameter error, {keyval} is not a valid property name and cannot be found in entity class {reftbTypeDisplayCsharp}</value>
  </data>
  <data name="ParameterError_NotValid_UseCommas" xml:space="preserve">
    <value>FreeSql: {property} parameter error, format "TopicId=Id, multiple groups using comma connection"</value>
  </data>
  <data name="Parsing_Failed" xml:space="preserve">
    <value>FreeSql: Parsing failed {callExpMethodName} {message}</value>
  </data>
  <data name="Policy_ObjectPool_Dispose" xml:space="preserve">
    <value>FreeSql: [{policyName}] The ObjectPool has been disposed, see: https://github.com/dotnetcore/FreeSql/discussions/1079</value>
  </data>
  <data name="Policy_Status_NotAvailable" xml:space="preserve">
    <value>FreeSql: The {policyName} status is unavailable and cannot be used until the background checker is restored. {UnavailableExceptionMessage}</value>
  </data>
  <data name="Properties_AsRowLock_Must_Numeric_Byte" xml:space="preserve">
    <value>FreeSql: The property {trytbVersionColumnCsName} is labeled as a row lock (optimistic lock) (IsVersion), but it must be a numeric type or byte[] or string, and it cannot be Nullable</value>
  </data>
  <data name="Properties_Cannot_Null" xml:space="preserve">
    <value>FreeSql: Properrties parameter cannot be empty</value>
  </data>
  <data name="Property_Cannot_Find" xml:space="preserve">
    <value>FreeSql: {property} property name not found</value>
  </data>
  <data name="Range_Comma_Separateda_By2Char" xml:space="preserve">
    <value>FreeSql: Range requires that Value be comma-separated and 2-length</value>
  </data>
  <data name="RollBack" xml:space="preserve">
    <value>FreeSql: RollBack</value>
  </data>
  <data name="RunTimeError_Reflection_IncludeMany" xml:space="preserve">
    <value>FreeSql: Runtime error, reflection failed to get IncludeMany method</value>
  </data>
  <data name="Set_Column_IsNullable_False" xml:space="preserve">
    <value>FreeSql: {qoteSql} is NULL unless the attribute [Column (IsNullable = false)]</value>
  </data>
  <data name="SubTableFieldValue_CannotLessThen" xml:space="preserve">
    <value>FreeSql: Subtable field value'{dt}'cannot be less than'{beginTime}'</value>
  </data>
  <data name="SubTableFieldValue_IsNotNull" xml:space="preserve">
    <value>FreeSql: Subtable field value cannot be null</value>
  </data>
  <data name="SubTableFieldValue_NotConvertDateTime" xml:space="preserve">
    <value>FreeSql: The tabular field value'{columnValue}'cannot be converted to DateTime</value>
  </data>
  <data name="SubTableFieldValue_NotMatchTable" xml:space="preserve">
    <value>FreeSql: Table field value'{dt}'does not match table name</value>
  </data>
  <data name="T2_Type_Error" xml:space="preserve">
    <value>FreeSql: Type T2 Error</value>
  </data>
  <data name="TableName_Format_Error" xml:space="preserve">
    <value>FreeSql: TableName format error, example: "log_{yyyyMMdd}"</value>
  </data>
  <data name="Type_AsType_Parameter_Error" xml:space="preserve">
    <value>FreeSql: {Type}.AsType parameter error, please pass in the correct entity type</value>
  </data>
  <data name="Type_Cannot_Access_Constructor" xml:space="preserve">
    <value>FreeSql: The {thatFullName} type cannot access the constructor</value>
  </data>
  <data name="Type_Error_Name" xml:space="preserve">
    <value>FreeSql: {name} type error</value>
  </data>
  <data name="TypeAsType_NotSupport_Object" xml:space="preserve">
    <value>FreeSql: {Type}.AsType parameter does not support specifying as object</value>
  </data>
  <data name="TypeError_CannotUse_IncludeMany" xml:space="preserve">
    <value>FreeSql: Type {typeofFullName} error, IncludeMany cannot be used</value>
  </data>
  <data name="Unable_Parse_Expression" xml:space="preserve">
    <value>FreeSql: Unable to parse expression: {exp}</value>
  </data>
  <data name="Unable_Parse_ExpressionMethod" xml:space="preserve">
    <value>FreeSql: Unable to parse expression method {exp3tmpCallMethodName}</value>
  </data>
  <data name="Use_InsertDict_Method" xml:space="preserve">
    <value>FreeSql: Please use fsql. InsertDict (dict) method inserts dictionary data</value>
  </data>
  <data name="S_NotFound_Name" xml:space="preserve">
    <value>FreeSql: {name} not found</value>
  </data>
  <data name="S_SlaveDatabase" xml:space="preserve">
    <value>FreeSql: Slave Database</value>
  </data>
  <data name="S_MasterDatabase" xml:space="preserve">
    <value>FreeSql: Master Database</value>
  </data>
  <data name="S_Access_InsertOnlyOneAtTime" xml:space="preserve">
    <value>FreeSql: Egg pain Accs insertion can only be performed one at a time, values (..) are not supported. (..) Select is also not supported.. UNION ALL select..</value>
  </data>
  <data name="S_BaseEntity_Initialization_Error" xml:space="preserve">
    <value>FreeSql: BaseEntity. Initialization initialization error, get IFreeSql is null</value>
  </data>
  <data name="S_BlockAccess_WaitForRecovery" xml:space="preserve">
    <value>FreeSql: [{thisName}] Block access and wait for recovery: {exMessage}</value>
  </data>
  <data name="S_CannotBeConverted_To_ISelect" xml:space="preserve">
    <value>FreeSql: IQueryable&lt;{typeofName}&gt; cannot be converted to ISelect&lt;{typeofName}&gt; because its implementation is not FreeSql.Extensions.Linq.QueryableProvider</value>
  </data>
  <data name="S_ConnectionStringError" xml:space="preserve">
    <value>FreeSql: Connection string error</value>
  </data>
  <data name="S_ConnectionStringError_Check" xml:space="preserve">
    <value>FreeSql: [{thisName}] Connection string error, please check.</value>
  </data>
  <data name="S_ConnectionStringError_CheckProject" xml:space="preserve">
    <value>FreeSql: Connection string error, or check project properties &gt; Build &gt; Target Platform: x86 | x64, Or use FreeSql.Provider.SqliteCore accessing arm platform</value>
  </data>
  <data name="S_ConnectionStringError_CheckProjectConnection" xml:space="preserve">
    <value>FreeSql: [{thisName}] Connection string error, please check. Or check Project Properties &gt; Build &gt; Target Platform: x86 | x64, Or use FreeSql.Provider.SqliteCore accessing arm platform</value>
  </data>
  <data name="S_CustomAdapter_Cannot_Use_CreateCommand" xml:space="preserve">
    <value>FreeSql: FreeSql.Provider.CustomAdapter cannot use CreateCommand</value>
  </data>
  <data name="S_CustomAdapter_OnlySuppport_UseConnectionFactory " xml:space="preserve">
    <value>FreeSql: FreeSql.Provider.CustomAdapter only supports building IFreeSql in the UseConnectionFactory way</value>
  </data>
  <data name="S_Dameng_NotSupport_TablespaceSchemas  " xml:space="preserve">
    <value>FreeSql: Dream CodeFirst does not support code creation tablespace and schemas {tbname}</value>
  </data>
  <data name="S_DB_Parameter_Error_NoConnectionString" xml:space="preserve">
    <value>FreeSql: -DB parameter error, no ConnectionString provided</value>
  </data>
  <data name="S_DB_ParameterError" xml:space="preserve">
    <value>FreeSql: -DB parameter error, format: MySql, ConnectionString</value>
  </data>
  <data name="S_DB_ParameterError_UnsupportedType" xml:space="preserve">
    <value>FreeSql: -DB parameter error, unsupported type: "{dbargs}"</value>
  </data>
  <data name="S_Features_Unique" xml:space="preserve">
    <value>FreeSql: {method} is FreeSql.Provider.{provider} specific features</value>
  </data>
  <data name="S_InsertOrUpdate_Unable_UpdateColumns" xml:space="preserve">
    <value>FreeSql: InsertOrUpdate Sqlite was unable to complete the UpdateColumns operation</value>
  </data>
  <data name="S_MygisGeometry_NotImplement" xml:space="preserve">
    <value>FreeSql: MygisGeometry. Parse does not implement "{wkt}"</value>
  </data>
  <data name="S_NameOptions_Incorrect" xml:space="preserve">
    <value>FreeSql: -NameOptions parameter incorrect, format: 0,0,0,0</value>
  </data>
  <data name="S_Not_Implemented_Feature" xml:space="preserve">
    <value>FreeSql: This function is not implemented</value>
  </data>
  <data name="S_Not_Implemented_FeedBack" xml:space="preserve">
    <value>FreeSql: Unrealized error, please feedback to author</value>
  </data>
  <data name="S_NotImplementSkipOffset" xml:space="preserve">
    <value>FreeSql: FreeSql.Provider.{providerName} does not implement Skip/Offset functionality, use to determine last ID if paging is required</value>
  </data>
  <data name="S_OldTableExists" xml:space="preserve">
    <value>FreeSql: Old table (OldName): {tboldname} exists, database already exists {tbname} table, cannot rename</value>
  </data>
  <data name="S_OnConflictDoUpdate_MustIsPrimary" xml:space="preserve">
    <value>FreeSql: The OnConflictDoUpdate feature requires that entity classes must set the IsPrimary property</value>
  </data>
  <data name="S_Oracle_NotSupport_TablespaceSchemas" xml:space="preserve">
    <value>FreeSql: Oracle CodeFirst does not support code creation of tablespace and schemas {tbname}</value>
  </data>
  <data name="S_ParsingFailed_UseRestoreToSelect" xml:space="preserve">
    <value>FreeSql: Parsing failed {callExpMethodName} {message}, hint: Extension method IQueryable can be used. RestoreToSelect() reverted to ISelect re-query</value>
  </data>
  <data name="S_RequiresEntityPrimaryKey" xml:space="preserve">
    <value>FreeSql: InsertOrUpdate + IfExistsDoNothing + {providerName} requires the entity class {tableCsName} to have a primary key</value>
  </data>
  <data name="S_SelectManayErrorType" xml:space="preserve">
    <value>FreeSql: SelectMany error type: {typeFullName}</value>
  </data>
  <data name="S_Type_IsNot_Migrable" xml:space="preserve">
    <value>FreeSql: Type {objentityTypeFullName} is not migrable</value>
  </data>
  <data name="S_Type_IsNot_Migrable_0Attributes" xml:space="preserve">
    <value>FreeSql: Type {objentityTypeFullName} is not migrable, migratable property 0</value>
  </data>
  <data name="S_TypeMappingNotImplemented" xml:space="preserve">
    <value>FreeSql: {columnDbTypeTextFull} type mapping not implemented</value>
  </data>
  <data name="S_WrongParameter" xml:space="preserve">
    <value>FreeSql: Wrong parameter setting: {args}</value>
  </data>
  <data name="S_ObjectPool" xml:space="preserve">
    <value>FreeSql: Object pool</value>
  </data></xml>`;

var xml2= `<xml id="xml2"><data name="AsTable_PropertyName_FormatError" xml:space="preserve">
    <value>[Table(AsTable = "{asTable}")] 特性值格式错误</value>
  </data>
  <data name="AsTable_PropertyName_NotDateTime" xml:space="preserve">
    <value>[Table(AsTable = xx)] 设置的属性名 {atmGroupsValue} 不是 DateTime 类型</value>
  </data>
  <data name="Available_Failed_Get_Resource" xml:space="preserve">
    <value>{name}: Failed to get resource {statistics}</value>
  </data>
  <data name="Available_Thrown_Exception" xml:space="preserve">
    <value>{name}: An exception needs to be thrown</value>
  </data>
  <data name="Bad_Expression_Format" xml:space="preserve">
    <value>错误的表达式格式 {column}</value>
  </data>
  <data name="Before_Chunk_Cannot_Use_Select" xml:space="preserve">
    <value>Chunk 功能之前不可使用 Select</value>
  </data>
  <data name="Begin_Transaction_Then_ForUpdate" xml:space="preserve">
    <value>安全起见，请务必在事务开启之后，再使用 ForUpdate</value>
  </data>
  <data name="Cannot_Be_NULL" xml:space="preserve">
    <value>不能为 null</value>
  </data>
  <data name="Cannot_Be_NULL_Name" xml:space="preserve">
    <value>{name} 不能为 null</value>
  </data>
  <data name="Cannot_Match_Property" xml:space="preserve">
    <value>无法匹配 {property}</value>
  </data>
  <data name="Cannot_Resolve_ExpressionTree" xml:space="preserve">
    <value>{property} 无法解析为表达式树</value>
  </data>
  <data name="Check_UseConnectionString" xml:space="preserve">
    <value>参数 masterConnectionString 不可为空，请检查 UseConnectionString</value>
  </data>
  <data name="Commit" xml:space="preserve">
    <value>提交</value>
  </data>
  <data name="Connection_Failed_Switch_Servers" xml:space="preserve">
    <value>连接失败，准备切换其他可用服务器</value>
  </data>
  <data name="Custom_Expression_ParsingError" xml:space="preserve">
    <value>自定义表达式解析错误：类型 {exp3MethodDeclaringType} 需要定义 static ThreadLocal&lt;ExpressionCallContext&gt; 字段、字段、字段（重要三次提醒）</value>
  </data>
  <data name="Custom_Reflection_IsNotNull" xml:space="preserve">
    <value>Custom { 反射信息 }不能为空，格式：{ 静态方法名 }{ 空格 }{ 反射信息 }</value>
  </data>
  <data name="Custom_StaticMethodName_IsNotNull" xml:space="preserve">
    <value>Custom { 静态方法名 }不能为空，格式：{ 静态方法名 }{ 空格 }{ 反射信息 }</value>
  </data>
  <data name="Custom_StaticMethodName_NotSet_DynamicFilterCustom" xml:space="preserve">
    <value>Custom 对应的{{ 静态方法名 }}：{fiValueCustomArray} 未设置 [DynamicFilterCustomAttribute] 特性</value>
  </data>
  <data name="CustomFieldSeparatedBySpaces" xml:space="preserve">
    <value>Custom 要求 Field 应该空格分割，并且长度为 2，格式：{ 静态方法名 }{ 空格 }{ 反射信息 }</value>
  </data>
  <data name="DataType_AsType_Inconsistent" xml:space="preserve">
    <value>操作的数据类型({dataDisplayCsharp}) 与 AsType({tableTypeDisplayCsharp}) 不一致，请检查。</value>
  </data>
  <data name="DateRange_Comma_Separateda_By2Char" xml:space="preserve">
    <value>DateRange 要求 Value 应该逗号分割，并且长度为 2</value>
  </data>
  <data name="DateRange_DateFormat_yyyy" xml:space="preserve">
    <value>DateRange 要求 Value[1] 格式必须为：yyyy、yyyy-MM、yyyy-MM-dd、yyyy-MM-dd HH、yyyy、yyyy-MM-dd HH:mm</value>
  </data>
  <data name="DbUpdateVersionException_RowLevelOptimisticLock" xml:space="preserve">
    <value>记录可能不存在，或者【行级乐观锁】版本过旧，更新数量{sourceCount}，影响的行数{affrows}。</value>
  </data>
  <data name="Different_Number_SlaveConnectionString_SlaveWeights" xml:space="preserve">
    <value>SlaveConnectionString 数量与 SlaveWeights 不相同</value>
  </data>
  <data name="Duplicate_ColumnAttribute" xml:space="preserve">
    <value>ColumnAttribute.Name {colattrName} 重复存在，请检查（注意：不区分大小写）</value>
  </data>
  <data name="Duplicate_PropertyName" xml:space="preserve">
    <value>属性名 {pName} 重复存在，请检查（注意：不区分大小写）</value>
  </data>
  <data name="Entity_Must_Primary_Key" xml:space="preserve">
    <value>{function} 功能要求实体类 {tableCsName} 必须有主键</value>
  </data>
  <data name="Entity_MySQL_VersionsBelow8_NotSupport_Multiple_PrimaryKeys" xml:space="preserve">
    <value>{tbTypeFullName} 是父子关系，但是 MySql 8.0 以下版本中不支持组合多主键</value>
  </data>
  <data name="Entity_NotParentChild_Relationship" xml:space="preserve">
    <value>{tbTypeFullName} 不是父子关系，无法使用该功能</value>
  </data>
  <data name="EspeciallySubquery_Cannot_Parsing" xml:space="preserve">
    <value>这个特别的子查询不能解析</value>
  </data>
  <data name="Expression_Error_Use_ParameterExpression" xml:space="preserve">
    <value>表达式错误，它的顶级对象不是 ParameterExpression：{exp}</value>
  </data>
  <data name="Expression_Error_Use_Successive_MemberAccess_Type" xml:space="preserve">
    <value>表达式错误，它不是连续的 MemberAccess 类型：{exp}</value>
  </data>
  <data name="ExpressionTree_Convert_Type_Error" xml:space="preserve">
    <value>ExpressionTree 转换类型错误，值({value})，类型({valueTypeFullName})，目标类型({typeFullName})，{exMessage}</value>
  </data>
  <data name="Failed_SubTable_FieldValue" xml:space="preserve">
    <value>未能解析分表字段值 {sqlWhere}</value>
  </data>
  <data name="Functions_AsTable_NotImplemented" xml:space="preserve">
    <value>AsTable 未实现的功能 {asTable}</value>
  </data>
  <data name="GBase_NotSupport_OtherThanCommas" xml:space="preserve">
    <value>GBase 暂时不支持逗号以外的分割符</value>
  </data>
  <data name="Generated_Same_SubTable" xml:space="preserve">
    <value>tableName：{tableName} 生成了相同的分表名</value>
  </data>
  <data name="GetPrimarys_ParameterError_IsNotDictKey " xml:space="preserve">
    <value>GetPrimarys 传递的参数 "{primary}" 不正确，它不属于字典数据的键名</value>
  </data>
  <data name="Has_Specified_Cannot_Specified_Second" xml:space="preserve">
    <value>已经指定了 {first}，不能再指定 {second}</value>
  </data>
  <data name="Ignored_Check_Confirm_PublicGetSet" xml:space="preserve">
    <value>{tb2DbName}.{mp2MemberName} 被忽略，请检查 IsIgnore 设置，确认 get/set 为 public</value>
  </data>
  <data name="Include_ParameterType_Error" xml:space="preserve">
    <value>Include 参数类型错误</value>
  </data>
  <data name="Include_ParameterType_Error_Use_IncludeMany" xml:space="preserve">
    <value>Include 参数类型错误，集合属性请使用 IncludeMany</value>
  </data>
  <data name="Include_ParameterType_Error_Use_MemberAccess" xml:space="preserve">
    <value>Include 参数类型错误，表达式类型应该为 MemberAccess</value>
  </data>
  <data name="IncludeMany_NotValid_Navigation" xml:space="preserve">
    <value>IncludeMany 类型 {tbTypeDisplayCsharp} 的属性 {collMemMemberName} 不是有效的导航属性，提示：IsIgnore = true 不会成为导航属性</value>
  </data>
  <data name="IncludeMany_ParameterError_OnlyUseOneParameter" xml:space="preserve">
    <value>IncludeMany {navigateSelector} 参数错误，Select 只可以使用一个参数的方法，正确格式：.Select(t =&gt;new TNavigate {{}})</value>
  </data>
  <data name="IncludeMany_ParameterError_Select_ReturnConsistentType" xml:space="preserve">
    <value>IncludeMany {navigateSelector} 参数错误，Select lambda参数返回值必须和 {collMemElementType} 类型一致</value>
  </data>
  <data name="IncludeMany_ParameterType_Error_Use_MemberAccess" xml:space="preserve">
    <value>IncludeMany 参数1 类型错误，表达式类型应该为 MemberAccess</value>
  </data>
  <data name="IncludeMany_ParameterTypeError" xml:space="preserve">
    <value>IncludeMany {navigateSelector} 参数类型错误，正确格式： a.collections.Take(1).Where(c =&gt;c.aid == a.id).Select(a=&gt; new TNavigate{{}})</value>
  </data>
  <data name="InsertInto_No_Property_Selected" xml:space="preserve">
    <value>ISelect.InsertInto() 未选择属性: {displayCsharp}</value>
  </data>
  <data name="InsertInto_TypeError" xml:space="preserve">
    <value>ISelect.InsertInto() 类型错误: {displayCsharp}</value>
  </data>
  <data name="InsertOrUpdate_Must_Primary_Key" xml:space="preserve">
    <value>InsertOrUpdate 功能执行 merge into 要求实体类 {CsName} 必须有主键</value>
  </data>
  <data name="InsertOrUpdate_NotSuport_Generic_UseEntity" xml:space="preserve">
    <value>InsertOrUpdate&lt;&gt;的泛型参数 不支持 {typeofT1},请传递您的实体类</value>
  </data>
  <data name="Install_FreeSql_Extensions_LazyLoading" xml:space="preserve">
    <value>【延时加载】功能需要安装 FreeSql.Extensions.LazyLoading.dll，可前往 nuget 下载</value>
  </data>
  <data name="LazyLoading_CompilationError" xml:space="preserve">
    <value>【延时加载】{trytbTypeName} 编译错误：{exMessage}\r\n\r\n{cscode}</value>
  </data>
  <data name="LazyLoading_EntityMustDeclarePublic" xml:space="preserve">
    <value>【延时加载】实体类型 {trytbTypeName} 必须声明为 public</value>
  </data>
  <data name="ManyToMany_AsSelect_NotSupport_Sum_Avg_etc" xml:space="preserve">
    <value>ManyToMany 导航属性 .AsSelect() 暂时不可用于 Sum/Avg/Max/Min/First/ToOne/ToList 方法</value>
  </data>
  <data name="ManyToMany_NotFound_CorrespondingField" xml:space="preserve">
    <value>【ManyToMany】导航属性 {trytbTypeName}.{pnvName} 在 {tbmidCsName} 中没有找到对应的字段，如：{midTypePropsTrytbName}{findtrytbPkCsName}、{midTypePropsTrytbName}_{findtrytbPkCsName}</value>
  </data>
  <data name="ManyToMany_ParsingError_EntityMissing_PrimaryKey" xml:space="preserve">
    <value>【ManyToMany】导航属性 {trytbTypeName}.{pnvName} 解析错误，实体类型 {tbrefTypeName} 缺少主键标识，[Column(IsPrimary = true)]</value>
  </data>
  <data name="ManyToMany_ParsingError_EntityMustHas_NavigateCollection" xml:space="preserve">
    <value>【ManyToMany】导航属性 {trytbTypeName}.{pnvName} 解析错误，实体类型 {tbrefTypeName} 必须存在对应的 [Navigate(ManyToMany = x)] 集合属性</value>
  </data>
  <data name="ManyToMany_ParsingError_InconsistentType" xml:space="preserve">
    <value>【ManyToMany】导航属性 {trytbTypeName}.{pnvName} 解析错误，{tbmidCsName}.{trycolCsName} 和 {trytbCsName}.{trytbPrimarysCsName} 类型不一致</value>
  </data>
  <data name="ManyToMany_ParsingError_IntermediateClass_ErrorMessage" xml:space="preserve">
    <value>【ManyToMany】导航属性 {trytbTypeName}.{pnvName} 解析错误，中间类 {tbmidCsName}.{midTypePropsTrytbName} 错误：{exMessage}</value>
  </data>
  <data name="ManyToMany_ParsingError_IntermediateClass_NotManyToOne_OneToOne" xml:space="preserve">
    <value>【ManyToMany】导航属性 {trytbTypeName}.{pnvName} 解析错误，中间类 {tbmidCsName}.{midTypePropsTrytbName} 导航属性不是【ManyToOne】或【OneToOne】</value>
  </data>
  <data name="Mapping_Exception_HasNo_SamePropertyName" xml:space="preserve">
    <value>映射异常：{name} 没有一个属性名相同</value>
  </data>
  <data name="MasterPool_IsNull_UseTransaction" xml:space="preserve">
    <value>Ado.MasterPool 值为 null，该操作无法自启用事务，请显式传递【事务对象】解决</value>
  </data>
  <data name="Missing_FreeSqlProvider_Package" xml:space="preserve">
    <value>缺少 FreeSql 数据库实现包：FreeSql.Provider.{Provider}.dll，可前往 nuget 下载</value>
  </data>
  <data name="Missing_FreeSqlProvider_Package_Reason" xml:space="preserve">
    <value>缺少 FreeSql 数据库实现包：{dll}，可前往 nuget 下载；如果存在 {dll} 依然报错（原因是环境问题导致反射不到类型），请在 UseConnectionString/UseConnectionFactory 第三个参数手工传入 typeof({providerType})</value>
  </data>
  <data name="Navigation_Bind_Number_Different" xml:space="preserve">
    <value>导航属性 {trytbTypeName}.{pnvName} 特性 [Navigate] Bind 数目({bindColumnsCount}) 与 外部主键数目({tbrefPrimarysLength}) 不相同</value>
  </data>
  <data name="Navigation_Missing_AsSelect" xml:space="preserve">
    <value>{tb2DbName}.{mp2MemberName} 导航属性集合忘了 .AsSelect() 吗？如果在 ToList(a =&gt; a.{mp2MemberName}) 中使用，请移步参考 IncludeMany 文档。</value>
  </data>
  <data name="Navigation_Missing_SetProperty" xml:space="preserve">
    <value>【导航属性】{trytbTypeDisplayCsharp}.{pName} 缺少 set 属性</value>
  </data>
  <data name="Navigation_NotFound_CorrespondingField" xml:space="preserve">
    <value>导航属性 {trytbTypeName}.{pnvName} 没有找到对应的字段，如：{pnvName}{findtbrefPkCsName}、{pnvName}_{findtbrefPkCsName}。或者使用 [Navigate] 特性指定关系映射。</value>
  </data>
  <data name="Navigation_ParsingError_EntityMissingPrimaryKey" xml:space="preserve">
    <value>导航属性 {trytbTypeName}.{pnvName} 解析错误，实体类型 {trytcTypeName} 缺少主键标识，[Column(IsPrimary = true)]</value>
  </data>
  <data name="Navigation_ParsingError_InconsistentType" xml:space="preserve">
    <value>导航属性 {trytbTypeName}.{pnvName} 解析错误，{trytbCsName}.{trycolCsName} 和 {tbrefCsName}.{tbrefPrimarysCsName} 类型不一致</value>
  </data>
  <data name="Navigation_ParsingError_NotFound_Property" xml:space="preserve">
    <value>导航属性 {trytbTypeName}.{pnvName} 特性 [Navigate] 解析错误，在 {tbrefTypeName} 未找到属性：{bi}</value>
  </data>
  <data name="NoPrimaryKey_UseSetDto" xml:space="preserve">
    <value>{tableTypeDisplayCsharp} 没有定义主键，无法使用 SetSource，请尝试 SetDto 或者 SetSource 指定临时主键</value>
  </data>
  <data name="NoProperty_Defined" xml:space="preserve">
    <value> 没有定义属性 </value>
  </data>
  <data name="Not_Implemented" xml:space="preserve">
    <value>未实现</value>
  </data>
  <data name="Not_Implemented_Expression" xml:space="preserve">
    <value>未实现函数表达式 {exp} 解析</value>
  </data>
  <data name="Not_Implemented_Expression_ParameterUseConstant" xml:space="preserve">
    <value>未实现函数表达式 {exp} 解析，参数 {expArguments} 必须为常量</value>
  </data>
  <data name="Not_Implemented_Expression_UseAsSelect" xml:space="preserve">
    <value>未实现函数表达式 {exp} 解析，如果正在操作导航属性集合，请使用 .AsSelect().{exp3MethodName}({exp3ArgumentsCount})</value>
  </data>
  <data name="Not_Implemented_MemberAcess_Constant" xml:space="preserve">
    <value>未实现 MemberAccess 下的 Constant</value>
  </data>
  <data name="Not_Implemented_Name" xml:space="preserve">
    <value>未实现 {name}</value>
  </data>
  <data name="Not_Support" xml:space="preserve">
    <value>不支持</value>
  </data>
  <data name="Not_Support_OrderByRandom" xml:space="preserve">
    <value>{dataType} 不支持 OrderByRandom 随机排序</value>
  </data>
  <data name="Not_Valid_Navigation_Property" xml:space="preserve">
    <value>{property} 不是有效的导航属性</value>
  </data>
  <data name="NotFound_Column" xml:space="preserve">
    <value>{dbName} 找不到列 {memberName}</value>
  </data>
  <data name="NotFound_CsName_Column" xml:space="preserve">
    <value>找不到 {CsName} 对应的列</value>
  </data>
  <data name="NotFound_Property" xml:space="preserve">
    <value>找不到属性：{memberName}</value>
  </data>
  <data name="NotFound_PropertyName" xml:space="preserve">
    <value>找不到属性名 {proto}</value>
  </data>
  <data name="NotFound_Reflection" xml:space="preserve">
    <value>Custom 找不到对应的{{ 反射信息 }}：{fiValueCustomArray}</value>
  </data>
  <data name="NotFound_Static_MethodName" xml:space="preserve">
    <value>Custom 找不到对应的{{ 静态方法名 }}：{fiValueCustomArray}</value>
  </data>
  <data name="NotFound_Table_Property_AsTable" xml:space="preserve">
    <value>[Table(AsTable = xx)] 设置的属性名 {atmGroupsValue} 不存在</value>
  </data>
  <data name="NotSpecified_UseConnectionString_UseConnectionFactory" xml:space="preserve">
    <value>未指定 UseConnectionString 或者 UseConnectionFactory</value>
  </data>
  <data name="ObjectPool_Get_Timeout" xml:space="preserve">
    <value>【{policyName}】ObjectPool.{GetName}() timeout {totalSeconds} seconds, see: https://github.com/dotnetcore/FreeSql/discussions/1081</value>
  </data>
  <data name="ObjectPool_GetAsync_Queue_Long" xml:space="preserve">
    <value>【{policyName}】ObjectPool.GetAsync() The queue is too long. Policy.AsyncGetCapacity = {asyncGetCapacity}</value>
  </data>
  <data name="OneToMany_NotFound_CorrespondingField" xml:space="preserve">
    <value>【OneToMany】导航属性 {trytbTypeName}.{pnvName} 在 {tbrefCsName} 中没有找到对应的字段，如：{findtrytb}{findtrytbPkCsName}、{findtrytb}_{findtrytbPkCsName}</value>
  </data>
  <data name="OneToMany_ParsingError_InconsistentType" xml:space="preserve">
    <value>【OneToMany】导航属性 {trytbTypeName}.{pnvName} 解析错误，{trytbCsName}.{trytbPrimarysCsName} 和 {tbrefCsName}.{trycolCsName} 类型不一致</value>
  </data>
  <data name="OneToMany_UseNavigate" xml:space="preserve">
    <value>、{refpropName}{findtrytbPkCsName}、{refpropName}_{findtrytbPkCsName}。或者使用 [Navigate] 特性指定关系映射。</value>
  </data>
  <data name="Parameter_Field_NotSpecified" xml:space="preserve">
    <value>参数 field 未指定</value>
  </data>
  <data name="ParameterError_NotValid_Collection" xml:space="preserve">
    <value>{property} 参数错误，它不是集合属性，必须为 IList&lt;T&gt; 或者 ICollection&lt;T&gt;</value>
  </data>
  <data name="ParameterError_NotValid_Navigation" xml:space="preserve">
    <value>{property} 参数错误，它不是有效的导航属性</value>
  </data>
  <data name="ParameterError_NotValid_PropertyName" xml:space="preserve">
    <value>{where} 参数错误，{keyval} 不是有效的属性名，在实体类 {reftbTypeDisplayCsharp} 无法找到</value>
  </data>
  <data name="ParameterError_NotValid_UseCommas" xml:space="preserve">
    <value>{property} 参数错误，格式 "TopicId=Id，多组使用逗号连接" </value>
  </data>
  <data name="Parsing_Failed" xml:space="preserve">
    <value>解析失败 {callExpMethodName} {message}</value>
  </data>
  <data name="Policy_ObjectPool_Dispose" xml:space="preserve">
    <value>【{policyName}】The ObjectPool has been disposed, see: https://github.com/dotnetcore/FreeSql/discussions/1079</value>
  </data>
  <data name="Policy_Status_NotAvailable" xml:space="preserve">
    <value>【{policyName}】状态不可用，等待后台检查程序恢复方可使用。{UnavailableExceptionMessage}</value>
  </data>
  <data name="Properties_AsRowLock_Must_Numeric_Byte" xml:space="preserve">
    <value>属性{trytbVersionColumnCsName} 被标注为行锁（乐观锁）(IsVersion)，但其必须为数字类型 或者 byte[] 或者 string，并且不可为 Nullable</value>
  </data>
  <data name="Properties_Cannot_Null" xml:space="preserve">
    <value>properties 参数不能为空</value>
  </data>
  <data name="Property_Cannot_Find" xml:space="preserve">
    <value>{property} 属性名无法找到</value>
  </data>
  <data name="Range_Comma_Separateda_By2Char" xml:space="preserve">
    <value>Range 要求 Value 应该逗号分割，并且长度为 2</value>
  </data>
  <data name="RollBack" xml:space="preserve">
    <value>回滚</value>
  </data>
  <data name="RunTimeError_Reflection_IncludeMany" xml:space="preserve">
    <value>运行时错误，反射获取 IncludeMany 方法失败</value>
  </data>
  <data name="S_Access_InsertOnlyOneAtTime" xml:space="preserve">
    <value>蛋疼的 Access 插入只能一条一条执行，不支持 values(..),(..) 也不支持 select .. UNION ALL select ..</value>
    <comment>Providers</comment>
  </data>
  <data name="S_BaseEntity_Initialization_Error" xml:space="preserve">
    <value>BaseEntity.Initialization 初始化错误，获取到 IFreeSql 是 null</value>
    <comment>Extensions</comment>
  </data>
  <data name="S_BlockAccess_WaitForRecovery" xml:space="preserve">
    <value>【{thisName}】Block access and wait for recovery: {exMessage}</value>
    <comment>Providers</comment>
  </data>
  <data name="S_CannotBeConverted_To_ISelect" xml:space="preserve">
    <value>无法将 IQueryable&lt;{typeofName}&gt; 转换为 ISelect&lt;{typeofName}&gt;，因为他的实现不是 FreeSql.Extensions.Linq.QueryableProvider</value>
    <comment>Extensions</comment>
  </data>
  <data name="S_ConnectionStringError" xml:space="preserve">
    <value>连接字符串错误</value>
    <comment>Providers</comment>
  </data>
  <data name="S_ConnectionStringError_Check" xml:space="preserve">
    <value>【{thisName}】连接字符串错误，请检查。</value>
    <comment>Providers</comment>
  </data>
  <data name="S_ConnectionStringError_CheckProject" xml:space="preserve">
    <value>连接字符串错误，或者检查项目属性 &gt; 生成 &gt; 目标平台：x86 | x64，或者改用 FreeSql.Provider.SqliteCore 访问 arm 平台</value>
    <comment>Providers</comment>
  </data>
  <data name="S_ConnectionStringError_CheckProjectConnection" xml:space="preserve">
    <value>【{thisName}】连接字符串错误，请检查。或者检查项目属性 &gt; 生成 &gt; 目标平台：x86 | x64，或者改用 FreeSql.Provider.SqliteCore 访问 arm 平台</value>
    <comment>Providers</comment>
  </data>
  <data name="S_CustomAdapter_Cannot_Use_CreateCommand" xml:space="preserve">
    <value>FreeSql.Provider.CustomAdapter 无法使用 CreateCommand</value>
    <comment>Providers</comment>
  </data>
  <data name="S_CustomAdapter_OnlySuppport_UseConnectionFactory " xml:space="preserve">
    <value>FreeSql.Provider.CustomAdapter 仅支持 UseConnectionFactory 方式构建 IFreeSql</value>
    <comment>Providers</comment>
  </data>
  <data name="S_Dameng_NotSupport_TablespaceSchemas  " xml:space="preserve">
    <value>达梦 CodeFirst 不支持代码创建 tablespace 与 schemas {tbname}</value>
    <comment>Providers</comment>
  </data>
  <data name="S_DB_Parameter_Error_NoConnectionString" xml:space="preserve">
    <value>-DB 参数错误，未提供 ConnectionString</value>
    <comment>Extensions</comment>
  </data>
  <data name="S_DB_ParameterError" xml:space="preserve">
    <value>-DB 参数错误，格式为：MySql,ConnectionString</value>
    <comment>Extensions</comment>
  </data>
  <data name="S_DB_ParameterError_UnsupportedType" xml:space="preserve">
    <value>-DB 参数错误，不支持的类型："{dbargs}"</value>
    <comment>Extensions</comment>
  </data>
  <data name="S_Features_Unique" xml:space="preserve">
    <value>{method} 是 FreeSql.Provider.{provider} 特有的功能</value>
    <comment>Providers</comment>
  </data>
  <data name="S_InsertOrUpdate_Unable_UpdateColumns" xml:space="preserve">
    <value>fsql.InsertOrUpdate Sqlite 无法完成 UpdateColumns 操作</value>
    <comment>Providers</comment>
  </data>
  <data name="S_MasterDatabase" xml:space="preserve">
    <value>主库</value>
    <comment>Providers</comment>
  </data>
  <data name="S_MygisGeometry_NotImplement" xml:space="preserve">
    <value>MygisGeometry.Parse 未实现 "{wkt}"</value>
    <comment>Providers</comment>
  </data>
  <data name="S_NameOptions_Incorrect" xml:space="preserve">
    <value>-NameOptions 参数错误，格式为：0,0,0,0</value>
    <comment>Extensions</comment>
  </data>
  <data name="S_Not_Implemented_Feature" xml:space="preserve">
    <value> 未实现该功能</value>
    <comment>Providers</comment>
  </data>
  <data name="S_Not_Implemented_FeedBack" xml:space="preserve">
    <value>未实现错误，请反馈给作者</value>
    <comment>Providers</comment>
  </data>
  <data name="S_NotFound_Name" xml:space="preserve">
    <value>找不到 {name}</value>
    <comment>Providers</comment>
  </data>
  <data name="S_NotImplementSkipOffset" xml:space="preserve">
    <value>FreeSql.Provider.{providerName} 未实现 Skip/Offset 功能，如果需要分页请使用判断上一次 id</value>
    <comment>Providers</comment>
  </data>
  <data name="S_ObjectPool" xml:space="preserve">
    <value>对象池</value>
    <comment>Providers</comment>
  </data>
  <data name="S_OldTableExists" xml:space="preserve">
    <value>旧表(OldName)：{tboldname} 存在，数据库已存在 {tbname} 表，无法改名</value>
    <comment>Providers</comment>
  </data>
  <data name="S_OnConflictDoUpdate_MustIsPrimary" xml:space="preserve">
    <value>OnConflictDoUpdate 功能要求实体类必须设置 IsPrimary 属性</value>
    <comment>Providers</comment>
  </data>
  <data name="S_Oracle_NotSupport_TablespaceSchemas" xml:space="preserve">
    <value>Oracle CodeFirst 不支持代码创建 tablespace 与 schemas {tbname}</value>
    <comment>Providers</comment>
  </data>
  <data name="S_ParsingFailed_UseRestoreToSelect" xml:space="preserve">
    <value>解析失败 {callExpMethodName} {message}，提示：可以使用扩展方法 IQueryable.RestoreToSelect() 还原为 ISelect 再查询</value>
    <comment>Extensions</comment>
  </data>
  <data name="S_RequiresEntityPrimaryKey" xml:space="preserve">
    <value>fsql.InsertOrUpdate + IfExistsDoNothing + {providerName}要求实体类 {tableCsName} 必须有主键</value>
    <comment>Providers</comment>
  </data>
  <data name="S_SelectManayErrorType" xml:space="preserve">
    <value>SelectMany 错误的类型：{typeFullName}</value>
    <comment>Extensions</comment>
  </data>
  <data name="S_SlaveDatabase" xml:space="preserve">
    <value>从库</value>
    <comment>Providers</comment>
  </data>
  <data name="S_Type_IsNot_Migrable" xml:space="preserve">
    <value>类型 {objentityTypeFullName} 不可迁移</value>
    <comment>Providers</comment>
  </data>
  <data name="S_Type_IsNot_Migrable_0Attributes" xml:space="preserve">
    <value>类型 {objentityTypeFullName} 不可迁移，可迁移属性0个</value>
    <comment>Providers</comment>
  </data>
  <data name="S_TypeMappingNotImplemented" xml:space="preserve">
    <value>未实现 {columnDbTypeTextFull} 类型映射</value>
    <comment>Providers</comment>
  </data>
  <data name="S_WrongParameter" xml:space="preserve">
    <value>错误的参数设置：{args}</value>
    <comment>Extensions</comment>
  </data>
  <data name="Set_Column_IsNullable_False" xml:space="preserve">
    <value>{qoteSql} is NULL，除非设置特性 [Column(IsNullable = false)]</value>
  </data>
  <data name="SubTableFieldValue_CannotLessThen" xml:space="preserve">
    <value>分表字段值 "{dt}" 不能小于 "{beginTime} "</value>
  </data>
  <data name="SubTableFieldValue_IsNotNull" xml:space="preserve">
    <value>分表字段值不能为 null</value>
  </data>
  <data name="SubTableFieldValue_NotConvertDateTime" xml:space="preserve">
    <value>分表字段值 "{columnValue}" 不能转化成 DateTime</value>
  </data>
  <data name="SubTableFieldValue_NotMatchTable" xml:space="preserve">
    <value>分表字段值 "{dt}" 未匹配到分表名</value>
  </data>
  <data name="T2_Type_Error" xml:space="preserve">
    <value>T2 类型错误</value>
  </data>
  <data name="TableName_Format_Error" xml:space="preserve">
    <value>tableName 格式错误，示例：“log_{yyyyMMdd}”</value>
  </data>
  <data name="Type_AsType_Parameter_Error" xml:space="preserve">
    <value>{Type}.AsType 参数错误，请传入正确的实体类型</value>
  </data>
  <data name="Type_Cannot_Access_Constructor" xml:space="preserve">
    <value>{thatFullName} 类型无法访问构造函数</value>
  </data>
  <data name="Type_Error_Name" xml:space="preserve">
    <value>{name} 类型错误</value>
  </data>
  <data name="TypeAsType_NotSupport_Object" xml:space="preserve">
    <value>{Type}.AsType 参数不支持指定为 object</value>
  </data>
  <data name="TypeError_CannotUse_IncludeMany" xml:space="preserve">
    <value>类型 {typeofFullName} 错误，不能使用 IncludeMany</value>
  </data>
  <data name="Unable_Parse_Expression" xml:space="preserve">
    <value>无法解析表达式：{exp}</value>
  </data>
  <data name="Unable_Parse_ExpressionMethod" xml:space="preserve">
    <value>无法解析表达式方法 {exp3tmpCallMethodName}</value>
  </data>
  <data name="Use_InsertDict_Method" xml:space="preserve">
    <value>请使用 fsql.InsertDict(dict) 方法插入字典数据</value>
  </data></xml>`;

$(document.body).append(xml1);
$(document.body).append(xml2);

var sb = 'public static class ErrorStrings {\r\npublic static string Language = "en";';
var datas = $('#xml1 data');
for (var a= 0; a < datas.length; a++) {
   var name = $(datas[a]).attr('name');
   var en = $(datas[a]).find('value').text();
   var cn = $('#xml2 data[name="' + name + '"]').find('value').text();
   console.log(name + ':' + en + '|' + cn);
   sb += `
        /// <summary>
        /// ` + cn.replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/&/g, '&amp;').split('\n')[0].replace(/\n/, '').replace(/\r\n/g, '') + `
        /// </summary>`;
   var args = en.match(/\{(\w+)\}/g);
   if (args == null || args.length == 0) {
      sb += '\r\n        public static string ' + name + ' => Language == "cn" ? \r\n            @"' + cn.replace(/"/g, '""') + '" : \r\n            @"' + en.replace(/"/g, '""') + '";';
   }
   else   if (args.length > 0) {
   sb += '\r\n        public static string ' + name + '(';
   var csargs = [];
   var csargsdict = {};
   for (var b = 0; b < args.length; b++) {
      var argname = args[b].substr(1, args[b].length - 2);
      if (csargsdict[argname] == null) {
         if (b > 0) sb += ', ';
         sb += 'object ' + argname;
         csargsdict[argname] = b;
         csargs.push(argname);
      }
      en = en.replace(args[b], '{' + csargsdict[argname] + '}');
      cn = cn.replace(args[b], '{' + csargsdict[argname] + '}');
   }
   var cn = cn.replace(/"/g, '""').replace(/\{/g, '{{').replace(/\}/g, '}}');
   var en = en.replace(/"/g, '""').replace(/\{/g, '{{').replace(/\}/g, '}}');
   for (var b = 0; b < csargs.length; b++) {
      cn = cn.replace(new RegExp('\\{\\{' + b + '\\}\\}', 'g'), '{' + csargs[b] + '}');
      en = en.replace(new RegExp('\\{\\{' + b + '\\}\\}', 'g'), '{' + csargs[b] + '}');
   }
   sb += ') => Language == "cn" ? \r\n            $@"' + cn + '" : \r\n            $@"' + en + '";';
   }
}
sb += '\r\n}\r\n';
console.log(sb);
        */