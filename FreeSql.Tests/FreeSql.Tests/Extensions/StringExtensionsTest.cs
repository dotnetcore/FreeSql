using Xunit;

namespace FreeSql.Tests.Extensions
{
    public class StringExtensionsTest
    {
        [Fact]
        public void FormatMySql()
        {

            Assert.Empty(((string)null).FormatMySql("11"));
            Assert.Equal("a=1", "a={0}".FormatMySql(1));
            Assert.Equal("a =1", "a ={0}".FormatMySql(1));
            Assert.Equal("a = 1", "a = {0}".FormatMySql(1));
            Assert.Equal("a='a'", "a={0}".FormatMySql('a'));
            Assert.Equal("a ='a'", "a ={0}".FormatMySql('a'));
            Assert.Equal("a = 'a'", "a = {0}".FormatMySql('a'));

            Assert.Equal("a=1 and b IS NULL", "a={0} and b={1}".FormatMySql(1, null));
            Assert.Equal("a =1 and b IS NULL", "a ={0} and b ={1}".FormatMySql(1, null));
            Assert.Equal("a = 1 and b IS NULL", "a = {0} and b = {1}".FormatMySql(1, null));

            Assert.Equal("a=1 and b IS NULL and c in (1,2,3,4)", "a={0} and b={1} and c in {2}".FormatMySql(1, null, new[] { 1, 2, 3, 4 }));
            Assert.Equal("a=1 and b IS NULL and c IS NULL", "a={0} and b={1} and c in {2}".FormatMySql(1, null, null));
            Assert.Equal("a=1 and b IS NULL and c not IS NULL", "a={0} and b={1} and c not in {2}".FormatMySql(1, null, null));
        }

        [Fact]
        public void FormatSqlServer()
        {

            Assert.Empty(((string)null).FormatSqlServer("11"));
            Assert.Equal("a=1", "a={0}".FormatSqlServer(1));
            Assert.Equal("a =1", "a ={0}".FormatSqlServer(1));
            Assert.Equal("a = 1", "a = {0}".FormatSqlServer(1));
            Assert.Equal("a='a'", "a={0}".FormatSqlServer('a'));
            Assert.Equal("a ='a'", "a ={0}".FormatSqlServer('a'));
            Assert.Equal("a = 'a'", "a = {0}".FormatSqlServer('a'));

            Assert.Equal("a=1 and b IS NULL", "a={0} and b={1}".FormatSqlServer(1, null));
            Assert.Equal("a =1 and b IS NULL", "a ={0} and b ={1}".FormatSqlServer(1, null));
            Assert.Equal("a = 1 and b IS NULL", "a = {0} and b = {1}".FormatSqlServer(1, null));

            Assert.Equal("a=1 and b IS NULL and c in (1,2,3,4)", "a={0} and b={1} and c in {2}".FormatSqlServer(1, null, new[] { 1, 2, 3, 4 }));
            Assert.Equal("a=1 and b IS NULL and c IS NULL", "a={0} and b={1} and c in {2}".FormatSqlServer(1, null, null));
            Assert.Equal("a=1 and b IS NULL and c not IS NULL", "a={0} and b={1} and c not in {2}".FormatSqlServer(1, null, null));
        }

        [Fact]
        public void FormatPostgreSQL()
        {

            Assert.Empty(((string)null).FormatPostgreSQL("11"));
            Assert.Equal("a=1", "a={0}".FormatPostgreSQL(1));
            Assert.Equal("a =1", "a ={0}".FormatPostgreSQL(1));
            Assert.Equal("a = 1", "a = {0}".FormatPostgreSQL(1));
            Assert.Equal("a='a'", "a={0}".FormatPostgreSQL('a'));
            Assert.Equal("a ='a'", "a ={0}".FormatPostgreSQL('a'));
            Assert.Equal("a = 'a'", "a = {0}".FormatPostgreSQL('a'));

            Assert.Equal("a=1 and b IS NULL", "a={0} and b={1}".FormatPostgreSQL(1, null));
            Assert.Equal("a =1 and b IS NULL", "a ={0} and b ={1}".FormatPostgreSQL(1, null));
            Assert.Equal("a = 1 and b IS NULL", "a = {0} and b = {1}".FormatPostgreSQL(1, null));

            Assert.Equal("a=1 and b IS NULL and c in (1,2,3,4)", "a={0} and b={1} and c in {2}".FormatPostgreSQL(1, null, new[] { 1, 2, 3, 4 }));
            Assert.Equal("a=1 and b IS NULL and c IS NULL", "a={0} and b={1} and c in {2}".FormatSqlServer(1, null, null));
            Assert.Equal("a=1 and b IS NULL and c not IS NULL", "a={0} and b={1} and c not in {2}".FormatSqlServer(1, null, null));
        }
    }
}
