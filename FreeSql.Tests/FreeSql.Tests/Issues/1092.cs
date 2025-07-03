using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Xunit;

namespace FreeSql.Tests.Issues
{
    public class _1092
    {
        [Fact]
        public void JsonbTest()
        {
            var fsql = g.pgsql;
            fsql.Delete<Test>().Where("1=1").ExecuteAffrows();

            var model = new Test() { Name = "hahahah", Ids = new List<long> { 22 } };

            var json1 = JsonConvert.SerializeObject(new List<long> { 22 });
            var jarray1 = JArray.FromObject(new List<long> { 22 });

            //异常：
            /*            
             * Unable to cast object of type 'Newtonsoft.Json.Linq.JArray' to type 'System.String'.”
             */
            var insertAndOut = fsql.Insert(model)
                .ExecuteInserted();

            //异常，和ExecuteInserted一样的错误
            var updateResult = fsql.Update<Test>()
                .SetSource(model)
                .ExecuteUpdated();
        }

        [Table(Name = "public.test_issues_1092")]
        public class Test
        {
            [Column(Name = "id", IsPrimary = true, IsIdentity = true)]
            public int Id { get; set; }
            /// <summary> 
            /// 会员ID
            /// </summary>
            [Column(Name = "name")]
            public string Name { get; set; }
            /// <summary> 
            /// 会员ID
            /// </summary>
            [Column(Name = "ids", MapType = typeof(JArray))]
            public List<long> Ids { get; set; }
        }
    }
}
