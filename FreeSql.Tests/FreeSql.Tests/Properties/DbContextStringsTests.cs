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
    public class DbContextErrorStringsTests
    {
        private readonly ITestOutputHelper output;
        public DbContextErrorStringsTests(ITestOutputHelper output)
        {
            this.output = output;
            //Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("zh-Hans");
            //Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("zh-Hans");

            //Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            //Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");

            //DbContextErrorStrings.Culture= new System.Globalization.CultureInfo("zh-Hans");
            //DbContextErrorStrings.Culture = new System.Globalization.CultureInfo("en-US");
        }

        [Fact]
        public void AddFreeDbContextError_CheckConstructionTest()
        {
            string text = DbContextErrorStrings.AddFreeDbContextError_CheckConstruction("1");
            output.WriteLine(text);
        }

        [Fact]
        public void CannotAdd_AlreadyExistsInStateManagementTest()
        {
            string text = DbContextErrorStrings.CannotAdd_AlreadyExistsInStateManagement("1");
            output.WriteLine(text);
        }

        [Fact]
        public void CannotAdd_EntityHasNo_PrimaryKeyTest()
        {
            string text = DbContextErrorStrings.CannotAdd_EntityHasNo_PrimaryKey("1");
            output.WriteLine(text);
        }

        [Fact]
        public void CannotAdd_PrimaryKey_NotSetTest()
        {
            string text = DbContextErrorStrings.CannotAdd_PrimaryKey_NotSet("1");
            output.WriteLine(text);
        }

        [Fact]
        public void CannotAdd_SelfIncreasingHasValueTest()
        {
            string text = DbContextErrorStrings.CannotAdd_SelfIncreasingHasValue("1");
            output.WriteLine(text);
        }

        [Fact]
        public void CannotAttach_EntityHasNo_PrimaryKeyTest()
        {
            string text = DbContextErrorStrings.CannotAttach_EntityHasNo_PrimaryKey("1");
            output.WriteLine(text);
        }

        [Fact]
        public void CannotAttach_PrimaryKey_NotSetTest()
        {
            string text = DbContextErrorStrings.CannotAttach_PrimaryKey_NotSet("1");
            output.WriteLine(text);
        }

        [Fact]
        public void CannotDelete_DataNotTracked_ShouldQueryTest()
        {
            string text = DbContextErrorStrings.CannotDelete_DataNotTracked_ShouldQuery("1");
            output.WriteLine(text);
        }

        [Fact]
        public void CannotDelete_EntityHasNo_PrimaryKeyTest()
        {
            string text = DbContextErrorStrings.CannotDelete_EntityHasNo_PrimaryKey("1");
            output.WriteLine(text);
        }

        [Fact]
        public void CannotDelete_PrimaryKey_NotSetTest()
        {
            string text = DbContextErrorStrings.CannotDelete_PrimaryKey_NotSet("1");
            output.WriteLine(text);
        }

        [Fact]
        public void CannotEdit_EntityHasNo_PrimaryKeyTest()
        {
            string text = DbContextErrorStrings.CannotEdit_EntityHasNo_PrimaryKey("1");
            output.WriteLine(text);
        }

        [Fact]
        public void CannotUpdate_DataShouldQueryOrAttachTest()
        {
            string text = DbContextErrorStrings.CannotUpdate_DataShouldQueryOrAttach("1");
            output.WriteLine(text);
        }

        [Fact]
        public void CannotUpdate_EntityHasNo_PrimaryKeyTest()
        {
            string text = DbContextErrorStrings.CannotUpdate_EntityHasNo_PrimaryKey("1");
            output.WriteLine(text);
        }

        [Fact]
        public void CannotUpdate_PrimaryKey_NotSetTest()
        {
            string text = DbContextErrorStrings.CannotUpdate_PrimaryKey_NotSet("1");
            output.WriteLine(text);
        }

        [Fact]
        public void CannotUpdate_RecordDoesNotExistTest()
        {
            string text = DbContextErrorStrings.CannotUpdate_RecordDoesNotExist("1");
            output.WriteLine(text);
        }

        [Fact]
        public void EntityType_CannotConvertTest()
        {
            string text = DbContextErrorStrings.EntityType_CannotConvert("1", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void EntityType_PrimaryKeyErrorTest()
        {
            string text = DbContextErrorStrings.EntityType_PrimaryKeyError("1", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void EntityType_PrimaryKeyIsNotOneTest()
        {
            string text = DbContextErrorStrings.EntityType_PrimaryKeyIsNotOne("1");
            output.WriteLine(text);
        }

        [Fact]
        public void Incomparable_EntityHasNo_PrimaryKeyTest()
        {
            string text = DbContextErrorStrings.Incomparable_EntityHasNo_PrimaryKey("1");
            output.WriteLine(text);
        }

        [Fact]
        public void Incomparable_PrimaryKey_NotSetTest()
        {
            string text = DbContextErrorStrings.Incomparable_PrimaryKey_NotSet("1");
            output.WriteLine(text);
        }

        [Fact]
        public void InsertError_FilterTest()
        {
            string text = DbContextErrorStrings.InsertError_Filter("1", "2", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void NotFound_PropertyTest()
        {
            string text = DbContextErrorStrings.NotFound_Property("1", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void ParameterDataTypeErrorTest()
        {
            string text = DbContextErrorStrings.ParameterDataTypeError("1");
            output.WriteLine(text);
        }

        [Fact]
        public void ParameterErrorTest()
        {
            string text = DbContextErrorStrings.ParameterError("1");
            output.WriteLine(text);
        }

        [Fact]
        public void ParameterError_CannotBeNullTest()
        {
            string text = DbContextErrorStrings.ParameterError_CannotBeNull("1");
            output.WriteLine(text);
        }

        [Fact]
        public void ParameterError_IsNot_CollectionPropertiesTest()
        {
            string text = DbContextErrorStrings.ParameterError_IsNot_CollectionProperties("1");
            output.WriteLine(text);
        }

        [Fact]
        public void ParameterError_NotFound_CollectionPropertiesTest()
        {
            string text = DbContextErrorStrings.ParameterError_NotFound_CollectionProperties("1");
            output.WriteLine(text);
        }

        [Fact]
        public void ParameterError_NotFound_PropertyTest()
        {
            string text = DbContextErrorStrings.ParameterError_NotFound_Property("1");
            output.WriteLine(text);
        }

        [Fact]
        public void PropertyOfType_IsNot_OneToManyOrManyToManyTest()
        {
            string text = DbContextErrorStrings.PropertyOfType_IsNot_OneToManyOrManyToMany("1", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void SpecialError_BatchAdditionFailedTest()
        {
            string text = DbContextErrorStrings.SpecialError_BatchAdditionFailed("1");
            output.WriteLine(text);
        }

        [Fact]
        public void SpecialError_UpdateFailedDataNotTrackedTest()
        {
            string text = DbContextErrorStrings.SpecialError_UpdateFailedDataNotTracked("1");
            output.WriteLine(text);
        }

        [Fact]
        public void TypeHasSetProperty_IgnoreAttributeTest()
        {
            string text = DbContextErrorStrings.TypeHasSetProperty_IgnoreAttribute("1", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void UnitOfWorkManager_Construction_CannotBeNullTest()
        {
            string text = DbContextErrorStrings.UnitOfWorkManager_Construction_CannotBeNull("1", "2");
            output.WriteLine(text);
        }

        [Fact]
        public void UpdateError_FilterTest()
        {
            string text = DbContextErrorStrings.UpdateError_Filter("1", "2", "3");
            output.WriteLine(text);
        }
    }
}