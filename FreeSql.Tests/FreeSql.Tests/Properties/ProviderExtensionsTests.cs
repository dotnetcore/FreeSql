using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace FreeSql.Tests.Properties
{
    public class ProviderExtensionsTests
    {
        private readonly ITestOutputHelper output;
        public ProviderExtensionsTests(ITestOutputHelper output)
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
            var x = CoreStrings.S_Access_InsertOnlyOneAtTime;
            output.WriteLine(x);
            x = CoreStrings.S_BaseEntity_Initialization_Error;
            output.WriteLine(x);
            x = CoreStrings.S_BlockAccess_WaitForRecovery(1, 2);
            output.WriteLine(x);
            x = CoreStrings.S_CannotBeConverted_To_ISelect(1);
            output.WriteLine(x);
            x = CoreStrings.S_ConnectionStringError;
            output.WriteLine(x);
            x = CoreStrings.S_ConnectionStringError_Check(1);
            output.WriteLine(x);
            x = CoreStrings.S_ConnectionStringError_CheckProject;
            output.WriteLine(x);
            x = CoreStrings.S_ConnectionStringError_CheckProjectConnection(1);
            output.WriteLine(x);
            x = CoreStrings.S_CustomAdapter_Cannot_Use_CreateCommand;
            output.WriteLine(x);
            x = CoreStrings.S_CustomAdapter_OnlySuppport_UseConnectionFactory;
            output.WriteLine(x);
            x = CoreStrings.S_Dameng_NotSupport_TablespaceSchemas(1);
            output.WriteLine(x);
            x = CoreStrings.S_DB_ParameterError;
            output.WriteLine(x);
            x = CoreStrings.S_DB_ParameterError_UnsupportedType(1);
            output.WriteLine(x);
            x = CoreStrings.S_DB_Parameter_Error_NoConnectionString;
            output.WriteLine(x);
            x = CoreStrings.S_Features_Unique(1, 2);
            output.WriteLine(x);
            x = CoreStrings.S_InsertOrUpdate_Unable_UpdateColumns;
            output.WriteLine(x);
            x = CoreStrings.S_MasterDatabase;
            output.WriteLine(x);
            x = CoreStrings.S_MygisGeometry_NotImplement(1);
            output.WriteLine(x);
            x = CoreStrings.S_NameOptions_Incorrect;
            output.WriteLine(x);
            x = CoreStrings.S_NotFound_Name("x");
            output.WriteLine(x);
            x = CoreStrings.S_NotImplementSkipOffset("oRACLE");
            output.WriteLine(x);
            x = CoreStrings.S_Not_Implemented_Feature;
            output.WriteLine(x);
            x = CoreStrings.S_Not_Implemented_FeedBack;
            output.WriteLine(x);
            x = CoreStrings.S_ObjectPool;
            output.WriteLine(x);
            x = CoreStrings.S_OldTableExists("old", "new");
            output.WriteLine(x);
            x = CoreStrings.S_OnConflictDoUpdate_MustIsPrimary;
            output.WriteLine(x);
            x = CoreStrings.S_Oracle_NotSupport_TablespaceSchemas(1);
            output.WriteLine(x);
            x = CoreStrings.S_ParsingFailed_UseRestoreToSelect(1, 2);
            output.WriteLine(x);
            x = CoreStrings.S_RequiresEntityPrimaryKey(1, 2);
            output.WriteLine(x);
            x = CoreStrings.S_SelectManayErrorType(1);
            output.WriteLine(x);
            x = CoreStrings.S_SlaveDatabase;
            output.WriteLine(x);
            x = CoreStrings.S_TypeMappingNotImplemented(1);
            output.WriteLine(x);
            x = CoreStrings.S_Type_IsNot_Migrable(1);
            output.WriteLine(x);
            x = CoreStrings.S_Type_IsNot_Migrable_0Attributes(1);
            output.WriteLine(x);
            x = CoreStrings.S_WrongParameter(1);
            output.WriteLine(x);
        }

    }
}