using System.Threading;

using Xunit;
using Xunit.Abstractions;

namespace FreeSql.Tests.Properties
{
    public class CoreStringsTests
    {
        private readonly ITestOutputHelper output;
        public CoreStringsTests(ITestOutputHelper output)
        {
            this.output = output;
            //Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("zh-Hans");
            //Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("zh-Hans");

            //Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            //Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");

            //CoreStrings.Culture= new System.Globalization.CultureInfo("zh-CN");
            CoreStrings.Culture = new System.Globalization.CultureInfo("en-US");
        }
        [Fact]
        public void AsTable_PropertyName_NotDateTimeTest()
        {
            var x = CoreStrings.CustomFieldSeparatedBySpaces;
            output.WriteLine(x);
            x = CoreStrings.Custom_StaticMethodName_IsNotNull;
            output.WriteLine(x);
            x = CoreStrings.Custom_Reflection_IsNotNull;
            output.WriteLine(x);
            string text = CoreStrings.AsTable_PropertyName_NotDateTime("1");
            output.WriteLine(text);
        }

        [Fact]
        public void AsTable_PropertyName_FormatErrorTest()
        {
            string text = CoreStrings.AsTable_PropertyName_FormatError("1");
            output.WriteLine(text);
        }


        [Fact]
        public void Available_Failed_Get_ResourceTest()
        {
            string text = CoreStrings.Available_Failed_Get_Resource("1", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void Available_Thrown_ExceptionTest()
        {
            string text = CoreStrings.Available_Thrown_Exception("1");
            output.WriteLine(text);
        }

        [Fact]
        public void Bad_Expression_FormatTest()
        {
            string text = CoreStrings.Bad_Expression_Format("1");
            output.WriteLine(text);
        }

        [Fact]
        public void Cannot_Be_NULL_NameTest()
        {
            string text = CoreStrings.Cannot_Be_NULL_Name("1");
            output.WriteLine(text);
        }

        [Fact]
        public void Cannot_Match_PropertyTest()
        {
            string text = CoreStrings.Cannot_Match_Property("1");
            output.WriteLine(text);
        }

        [Fact]
        public void Cannot_Resolve_ExpressionTreeTest()
        {
            string text = CoreStrings.Cannot_Resolve_ExpressionTree("1");
            output.WriteLine(text);
        }

        [Fact]
        public void Custom_Expression_ParsingErrorTest()
        {
            string text = CoreStrings.Custom_Expression_ParsingError("1");
            output.WriteLine(text);
        }

        [Fact]
        public void Custom_StaticMethodName_NotSet_DynamicFilterCustomTest()
        {
            string text = CoreStrings.Custom_StaticMethodName_NotSet_DynamicFilterCustom("1");
            output.WriteLine(text);
        }

        [Fact]
        public void DataType_AsType_InconsistentTest()
        {
            string text = CoreStrings.DataType_AsType_Inconsistent("1", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void DbUpdateVersionException_RowLevelOptimisticLockTest()
        {
            string text = CoreStrings.DbUpdateVersionException_RowLevelOptimisticLock("1", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void Duplicate_ColumnAttributeTest()
        {
            string text = CoreStrings.Duplicate_ColumnAttribute("1");
            output.WriteLine(text);
        }

        [Fact]
        public void Duplicate_PropertyNameTest()
        {
            string text = CoreStrings.Duplicate_PropertyName("1");
            output.WriteLine(text);
        }

        [Fact]
        public void Entity_Must_Primary_KeyTest()
        {
            string text = CoreStrings.Entity_Must_Primary_Key("1", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void Entity_MySQL_VersionsBelow8_NotSupport_Multiple_PrimaryKeysTest()
        {
            string text = CoreStrings.Entity_MySQL_VersionsBelow8_NotSupport_Multiple_PrimaryKeys("1");
            output.WriteLine(text);
        }

        [Fact]
        public void Entity_NotParentChild_RelationshipTest()
        {
            string text = CoreStrings.Entity_NotParentChild_Relationship("1");
            output.WriteLine(text);
        }

        [Fact]
        public void Expression_Error_Use_ParameterExpressionTest()
        {
            string text = CoreStrings.Expression_Error_Use_ParameterExpression("1");
            output.WriteLine(text);
        }

        [Fact]
        public void Expression_Error_Use_Successive_MemberAccess_TypeTest()
        {
            string text = CoreStrings.Expression_Error_Use_Successive_MemberAccess_Type("1");
            output.WriteLine(text);
        }

        [Fact]
        public void ExpressionTree_Convert_Type_ErrorTest()
        {
            string text = CoreStrings.ExpressionTree_Convert_Type_Error("1", "2", "3", "4 ");
            output.WriteLine(text);
        }

        [Fact]
        public void Failed_SubTable_FieldValueTest()
        {
            string text = CoreStrings.Failed_SubTable_FieldValue("1");
            output.WriteLine(text);
        }

        [Fact]
        public void Functions_AsTable_NotImplementedTest()
        {
            string text = CoreStrings.Functions_AsTable_NotImplemented("1");
            output.WriteLine(text);
        }

        [Fact]
        public void Generated_Same_SubTableTest()
        {
            string text = CoreStrings.Generated_Same_SubTable("1");
            output.WriteLine(text);
        }

        [Fact]
        public void GetPrimarys_ParameterError_IsNotDictKeyTest()
        {
            string text = CoreStrings.GetPrimarys_ParameterError_IsNotDictKey("1");
            output.WriteLine(text);
        }

        [Fact]
        public void Has_Specified_Cannot_Specified_SecondTest()
        {
            string text = CoreStrings.Has_Specified_Cannot_Specified_Second("1", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void Ignored_Check_Confirm_PublicGetSetTest()
        {
            string text = CoreStrings.Ignored_Check_Confirm_PublicGetSet("1", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void IncludeMany_NotValid_NavigationTest()
        {
            string text = CoreStrings.IncludeMany_NotValid_Navigation("1", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void IncludeMany_ParameterError_OnlyUseOneParameterTest()
        {
            string text = CoreStrings.IncludeMany_ParameterError_OnlyUseOneParameter("1");
            output.WriteLine(text);
        }

        [Fact]
        public void IncludeMany_ParameterError_Select_ReturnConsistentTypeTest()
        {
            string text = CoreStrings.IncludeMany_ParameterError_Select_ReturnConsistentType("1", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void IncludeMany_ParameterTypeErrorTest()
        {
            string text = CoreStrings.IncludeMany_ParameterTypeError("1");
            output.WriteLine(text);
        }

        [Fact]
        public void InsertInto_No_Property_SelectedTest()
        {
            string text = CoreStrings.InsertInto_No_Property_Selected("1");
            output.WriteLine(text);
        }

        [Fact]
        public void InsertInto_TypeErrorTest()
        {
            string text = CoreStrings.InsertInto_TypeError("1");
            output.WriteLine(text);
        }

        [Fact]
        public void InsertOrUpdate_Must_Primary_KeyTest()
        {
            string text = CoreStrings.InsertOrUpdate_Must_Primary_Key("1");
            output.WriteLine(text);
        }

        [Fact]
        public void InsertOrUpdate_NotSuport_Generic_UseEntityTest()
        {
            string text = CoreStrings.InsertOrUpdate_NotSuport_Generic_UseEntity("1");
            output.WriteLine(text);
        }

        [Fact]
        public void LazyLoading_CompilationErrorTest()
        {
            string text = CoreStrings.LazyLoading_CompilationError("1", "2", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void LazyLoading_EntityMustDeclarePublicTest()
        {
            string text = CoreStrings.LazyLoading_EntityMustDeclarePublic("1");
            output.WriteLine(text);
        }

        [Fact]
        public void ManyToMany_NotFound_CorrespondingFieldTest()
        {
            string text = CoreStrings.ManyToMany_NotFound_CorrespondingField("1", "2", "2", "2", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void ManyToMany_ParsingError_EntityMissing_PrimaryKeyTest()
        {
            string text = CoreStrings.ManyToMany_ParsingError_EntityMissing_PrimaryKey("1", "2", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void ManyToMany_ParsingError_EntityMustHas_NavigateCollectionTest()
        {
            string text = CoreStrings.ManyToMany_ParsingError_EntityMustHas_NavigateCollection("1", "2", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void ManyToMany_ParsingError_InconsistentTypeTest()
        {
            string text = CoreStrings.ManyToMany_ParsingError_InconsistentType("1", "2", "2", "2", "2", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void ManyToMany_ParsingError_IntermediateClass_ErrorMessageTest()
        {
            string text = CoreStrings.ManyToMany_ParsingError_IntermediateClass_ErrorMessage("1", "2", "2", "2", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void ManyToMany_ParsingError_IntermediateClass_NotManyToOne_OneToOneTest()
        {
            string text = CoreStrings.ManyToMany_ParsingError_IntermediateClass_NotManyToOne_OneToOne("1", "2", "2", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void Mapping_Exception_HasNo_SamePropertyNameTest()
        {
            string text = CoreStrings.Mapping_Exception_HasNo_SamePropertyName("1");
            output.WriteLine(text);
        }

        [Fact]
        public void Missing_FreeSqlProvider_PackageTest()
        {
            string text = CoreStrings.Missing_FreeSqlProvider_Package("1");
            output.WriteLine(text);
        }

        [Fact]
        public void Missing_FreeSqlProvider_Package_ReasonTest()
        {
            string text = CoreStrings.Missing_FreeSqlProvider_Package_Reason("1", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void Navigation_Bind_Number_DifferentTest()
        {
            string text = CoreStrings.Navigation_Bind_Number_Different("1", "2", "2", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void Navigation_Missing_AsSelectTest()
        {
            string text = CoreStrings.Navigation_Missing_AsSelect("1", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void Navigation_Missing_SetPropertyTest()
        {
            string text = CoreStrings.Navigation_Missing_SetProperty("1", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void Navigation_NotFound_CorrespondingFieldTest()
        {
            string text = CoreStrings.Navigation_NotFound_CorrespondingField("1", "2", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void Navigation_ParsingError_EntityMissingPrimaryKeyTest()
        {
            string text = CoreStrings.Navigation_ParsingError_EntityMissingPrimaryKey("1", "2", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void Navigation_ParsingError_InconsistentTypeTest()
        {
            string text = CoreStrings.Navigation_ParsingError_InconsistentType("1", "2", "2", "2", "2", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void Navigation_ParsingError_NotFound_PropertyTest()
        {
            string text = CoreStrings.Navigation_ParsingError_NotFound_Property("1", "2", "2", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void NoPrimaryKey_UseSetDtoTest()
        {
            string text = CoreStrings.NoPrimaryKey_UseSetDto("1");
            output.WriteLine(text);
        }

        [Fact]
        public void Not_Implemented_ExpressionTest()
        {
            string text = CoreStrings.Not_Implemented_Expression("1");
            output.WriteLine(text);
        }

        [Fact]
        public void Not_Implemented_Expression_ParameterUseConstantTest()
        {
            string text = CoreStrings.Not_Implemented_Expression_ParameterUseConstant("1", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void Not_Implemented_Expression_UseAsSelectTest()
        {
            string text = CoreStrings.Not_Implemented_Expression_UseAsSelect("1", "2", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void Not_Implemented_NameTest()
        {
            string text = CoreStrings.Not_Implemented_Name("1");
            output.WriteLine(text);
        }

        [Fact]
        public void Not_Support_OrderByRandomTest()
        {
            string text = CoreStrings.Not_Support_OrderByRandom("1");
            output.WriteLine(text);
        }

        [Fact]
        public void Not_Valid_Navigation_PropertyTest()
        {
            string text = CoreStrings.Not_Valid_Navigation_Property("1");
            output.WriteLine(text);
        }

        [Fact]
        public void NotFound_ColumnTest()
        {
            string text = CoreStrings.NotFound_Column("1", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void NotFound_CsName_ColumnTest()
        {
            string text = CoreStrings.NotFound_Column("1", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void NotFound_PropertyTest()
        {
            string text = CoreStrings.NotFound_Property("1");
            output.WriteLine(text);
        }

        [Fact]
        public void NotFound_PropertyNameTest()
        {
            string text = CoreStrings.NotFound_PropertyName("1");
            output.WriteLine(text);
        }

        [Fact]
        public void NotFound_ReflectionTest()
        {
            string text = CoreStrings.NotFound_Reflection("1");
            output.WriteLine(text);
        }

        [Fact]
        public void NotFound_Static_MethodNameTest()
        {
            string text = CoreStrings.NotFound_Static_MethodName("1");
            output.WriteLine(text);
        }

        [Fact]
        public void NotFound_Table_Property_AsTableTest()
        {
            string text = CoreStrings.NotFound_Table_Property_AsTable("1");
            output.WriteLine(text);
        }

        [Fact]
        public void ObjectPool_Get_TimeoutTest()
        {
            string text = CoreStrings.ObjectPool_Get_Timeout("1", "2", "3");
            output.WriteLine(text);
        }

        [Fact]
        public void ObjectPool_GetAsync_Queue_LongTest()
        {
            string text = CoreStrings.ObjectPool_GetAsync_Queue_Long("1", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void OneToMany_NotFound_CorrespondingFieldTest()
        {
            string text = CoreStrings.OneToMany_NotFound_CorrespondingField("1", "2", "2", "2", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void OneToMany_ParsingError_InconsistentTypeTest()
        {
            string text = CoreStrings.OneToMany_ParsingError_InconsistentType("1", "2", "2", "2", "2", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void OneToMany_UseNavigateTest()
        {
            string text = CoreStrings.OneToMany_UseNavigate("1", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void ParameterError_NotValid_CollectionTest()
        {
            string text = CoreStrings.ParameterError_NotValid_Collection("1");
            output.WriteLine(text);
        }

        [Fact]
        public void ParameterError_NotValid_NavigationTest()
        {
            string text = CoreStrings.ParameterError_NotValid_Navigation("1");
            output.WriteLine(text);
        }

        [Fact]
        public void ParameterError_NotValid_PropertyNameTest()
        {
            string text = CoreStrings.ParameterError_NotValid_PropertyName("1", "2", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void ParameterError_NotValid_UseCommasTest()
        {
            string text = CoreStrings.ParameterError_NotValid_UseCommas("1");
            output.WriteLine(text);
        }

        [Fact]
        public void Parsing_FailedTest()
        {
            string text = CoreStrings.Parsing_Failed("1", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void Policy_ObjectPool_DisposeTest()
        {
            string text = CoreStrings.Policy_ObjectPool_Dispose("1");
            output.WriteLine(text);
        }

        [Fact]
        public void Policy_Status_NotAvailableTest()
        {
            string text = CoreStrings.Policy_Status_NotAvailable("1", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void Properties_AsRowLock_Must_Numeric_ByteTest()
        {
            string text = CoreStrings.Properties_AsRowLock_Must_Numeric_Byte("1");
            output.WriteLine(text);
        }

        [Fact]
        public void Property_Cannot_FindTest()
        {
            string text = CoreStrings.Property_Cannot_Find("1");
            output.WriteLine(text);
        }

        [Fact]
        public void Set_Column_IsNullable_FalseTest()
        {
            string text = CoreStrings.Set_Column_IsNullable_False("1");
            output.WriteLine(text);
        }

        [Fact]
        public void SubTableFieldValue_CannotLessThenTest()
        {
            string text = CoreStrings.SubTableFieldValue_CannotLessThen("1", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void SubTableFieldValue_NotConvertDateTimeTest()
        {
            string text = CoreStrings.SubTableFieldValue_NotConvertDateTime("1");
            output.WriteLine(text);
        }

        [Fact]
        public void SubTableFieldValue_NotMatchTableTest()
        {
            string text = CoreStrings.SubTableFieldValue_NotConvertDateTime("1");
            output.WriteLine(text);
        }


        [Fact]
        public void Type_AsType_Parameter_ErrorTest()
        {
            string text = CoreStrings.Type_AsType_Parameter_Error("1");
            output.WriteLine(text);
        }

        [Fact]
        public void Type_Cannot_Access_ConstructorTest()
        {
            string text = CoreStrings.Type_Cannot_Access_Constructor("1");
            output.WriteLine(text);
        }

        [Fact]
        public void Type_Error_NameTest()
        {
            string text = CoreStrings.Type_Error_Name("1");
            output.WriteLine(text);
        }

        [Fact]
        public void TypeAsType_NotSupport_ObjectTest()
        {
            string text = CoreStrings.TypeAsType_NotSupport_Object("1");
            output.WriteLine(text);
        }

        [Fact]
        public void TypeError_CannotUse_IncludeManyTest()
        {
            string text = CoreStrings.TypeError_CannotUse_IncludeMany("1");
            output.WriteLine(text);
        }

        [Fact]
        public void Unable_Parse_ExpressionTest()
        {
            string text = CoreStrings.Unable_Parse_Expression("1");
            output.WriteLine(text);
        }

        [Fact]
        public void Unable_Parse_ExpressionMethodTest()
        {
            string text = CoreStrings.Unable_Parse_ExpressionMethod("1");
            output.WriteLine(text);
        }

    }
}