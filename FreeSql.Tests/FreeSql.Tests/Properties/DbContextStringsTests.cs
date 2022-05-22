using Xunit;
using Xunit.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace FreeSql.Tests.Properties
{
    public class DbContextStringsTests
    {
        private readonly ITestOutputHelper output;
        public DbContextStringsTests(ITestOutputHelper output)
        {
            this.output = output;
            //Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("zh-Hans");
            //Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("zh-Hans");

            //Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            //Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");

            //DbContextStrings.Culture= new System.Globalization.CultureInfo("zh-Hans");
            DbContextStrings.Culture = new System.Globalization.CultureInfo("en-US");
        }

        [Fact]
        public void AddFreeDbContextError_CheckConstructionTest()
        {
            string text = DbContextStrings.AddFreeDbContextError_CheckConstruction("1");
            output.WriteLine(text);
        }

        [Fact]
        public void CannotAdd_AlreadyExistsInStateManagementTest()
        {
            string text = DbContextStrings.CannotAdd_AlreadyExistsInStateManagement("1");
            output.WriteLine(text);
        }

        [Fact]
        public void CannotAdd_EntityHasNo_PrimaryKeyTest()
        {
            string text = DbContextStrings.CannotAdd_EntityHasNo_PrimaryKey("1");
            output.WriteLine(text);
        }

        [Fact]
        public void CannotAdd_PrimaryKey_NotSetTest()
        {
            string text = DbContextStrings.CannotAdd_PrimaryKey_NotSet("1");
            output.WriteLine(text);
        }

        [Fact]
        public void CannotAdd_SelfIncreasingHasValueTest()
        {
            string text = DbContextStrings.CannotAdd_SelfIncreasingHasValue("1");
            output.WriteLine(text);
        }

        [Fact]
        public void CannotAttach_EntityHasNo_PrimaryKeyTest()
        {
            string text = DbContextStrings.CannotAttach_EntityHasNo_PrimaryKey("1");
            output.WriteLine(text);
        }

        [Fact]
        public void CannotAttach_PrimaryKey_NotSetTest()
        {
            string text = DbContextStrings.CannotAttach_PrimaryKey_NotSet("1");
            output.WriteLine(text);
        }

        [Fact]
        public void CannotDelete_DataNotTracked_ShouldQueryTest()
        {
            string text = DbContextStrings.CannotDelete_DataNotTracked_ShouldQuery("1");
            output.WriteLine(text);
        }

        [Fact]
        public void CannotDelete_EntityHasNo_PrimaryKeyTest()
        {
            string text = DbContextStrings.CannotDelete_EntityHasNo_PrimaryKey("1");
            output.WriteLine(text);
        }

        [Fact]
        public void CannotDelete_PrimaryKey_NotSetTest()
        {
            string text = DbContextStrings.CannotDelete_PrimaryKey_NotSet("1");
            output.WriteLine(text);
        }

        [Fact]
        public void CannotEdit_EntityHasNo_PrimaryKeyTest()
        {
            string text = DbContextStrings.CannotEdit_EntityHasNo_PrimaryKey("1");
            output.WriteLine(text);
        }

        [Fact]
        public void CannotUpdate_DataShouldQueryOrAttachTest()
        {
            string text = DbContextStrings.CannotUpdate_DataShouldQueryOrAttach("1");
            output.WriteLine(text);
        }

        [Fact]
        public void CannotUpdate_EntityHasNo_PrimaryKeyTest()
        {
            string text = DbContextStrings.CannotUpdate_EntityHasNo_PrimaryKey("1");
            output.WriteLine(text);
        }

        [Fact]
        public void CannotUpdate_PrimaryKey_NotSetTest()
        {
            string text = DbContextStrings.CannotUpdate_PrimaryKey_NotSet("1");
            output.WriteLine(text);
        }

        [Fact]
        public void CannotUpdate_RecordDoesNotExistTest()
        {
            string text = DbContextStrings.CannotUpdate_RecordDoesNotExist("1");
            output.WriteLine(text);
        }

        [Fact]
        public void EntityType_CannotConvertTest()
        {
            string text = DbContextStrings.EntityType_CannotConvert("1", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void EntityType_PrimaryKeyErrorTest()
        {
            string text = DbContextStrings.EntityType_PrimaryKeyError("1", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void EntityType_PrimaryKeyIsNotOneTest()
        {
            string text = DbContextStrings.EntityType_PrimaryKeyIsNotOne("1");
            output.WriteLine(text);
        }

        [Fact]
        public void Incomparable_EntityHasNo_PrimaryKeyTest()
        {
            string text = DbContextStrings.Incomparable_EntityHasNo_PrimaryKey("1");
            output.WriteLine(text);
        }

        [Fact]
        public void Incomparable_PrimaryKey_NotSetTest()
        {
            string text = DbContextStrings.Incomparable_PrimaryKey_NotSet("1");
            output.WriteLine(text);
        }

        [Fact]
        public void InsertError_FilterTest()
        {
            string text = DbContextStrings.InsertError_Filter("1", "2", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void NotFound_PropertyTest()
        {
            string text = DbContextStrings.NotFound_Property("1", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void ParameterDataTypeErrorTest()
        {
            string text = DbContextStrings.ParameterDataTypeError("1");
            output.WriteLine(text);
        }

        [Fact]
        public void ParameterErrorTest()
        {
            string text = DbContextStrings.ParameterError("1");
            output.WriteLine(text);
        }

        [Fact]
        public void ParameterError_CannotBeNullTest()
        {
            string text = DbContextStrings.ParameterError_CannotBeNull("1");
            output.WriteLine(text);
        }

        [Fact]
        public void ParameterError_IsNot_CollectionPropertiesTest()
        {
            string text = DbContextStrings.ParameterError_IsNot_CollectionProperties("1");
            output.WriteLine(text);
        }

        [Fact]
        public void ParameterError_NotFound_CollectionPropertiesTest()
        {
            string text = DbContextStrings.ParameterError_NotFound_CollectionProperties("1");
            output.WriteLine(text);
        }

        [Fact]
        public void ParameterError_NotFound_PropertyTest()
        {
            string text = DbContextStrings.ParameterError_NotFound_Property("1");
            output.WriteLine(text);
        }

        [Fact]
        public void PropertyOfType_IsNot_OneToManyOrManyToManyTest()
        {
            string text = DbContextStrings.PropertyOfType_IsNot_OneToManyOrManyToMany("1", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void SpecialError_BatchAdditionFailedTest()
        {
            string text = DbContextStrings.SpecialError_BatchAdditionFailed("1");
            output.WriteLine(text);
        }

        [Fact]
        public void SpecialError_UpdateFailedDataNotTrackedTest()
        {
            string text = DbContextStrings.SpecialError_UpdateFailedDataNotTracked("1");
            output.WriteLine(text);
        }

        [Fact]
        public void TypeHasSetProperty_IgnoreAttributeTest()
        {
            string text = DbContextStrings.TypeHasSetProperty_IgnoreAttribute("1", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void UnitOfWorkManager_Construction_CannotBeNullTest()
        {
            string text = DbContextStrings.UnitOfWorkManager_Construction_CannotBeNull("1", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void UpdateError_FilterTest()
        {
            string text = DbContextStrings.UpdateError_Filter("1", "2", "3");
            output.WriteLine(text);
        }
    }
}