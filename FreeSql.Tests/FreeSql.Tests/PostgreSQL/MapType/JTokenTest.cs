using FreeSql.DataAnnotations;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using Xunit;

namespace FreeSql.Tests.PostgreSQLMapType
{
    public class JTokenTest
    {
        class JTokenTestMap
        {
            public Guid id { get; set; }

            [Column(MapType = typeof(JToken))]
            public string string_to_jtoken { get; set; }
            [Column(MapType = typeof(JArray))]
            public string string_to_jarray { get; set; }
            [Column(MapType = typeof(JObject))]
            public string string_to_jobject { get; set; }

            [Column(MapType = typeof(string))]
            public JToken jtoken_to_string { get; set; }
            [Column(MapType = typeof(string))]
            public JArray jarray_to_string { get; set; }
            [Column(MapType = typeof(string))]
            public JObject jobject_to_string { get; set; }
        }
        [Fact]
        public void JTokenWithObjectToString()
        {
            //insert
            var orm = g.pgsql;
            var item = new JTokenTestMap { };
            Assert.Equal(1, orm.Insert<JTokenTestMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<JTokenTestMap>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Null(find.jtoken_to_string);

            var json = JToken.FromObject(new { test1 = 1, test2 = "222" });
            item = new JTokenTestMap { jtoken_to_string = json };
            Assert.Equal(1, orm.Insert<JTokenTestMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<JTokenTestMap>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(json, find.jtoken_to_string);

            //update
            json = JToken.FromObject(new { test2 = 33, test3 = "333" });
            item.jtoken_to_string = json;
            Assert.Equal(1, orm.Update<JTokenTestMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<JTokenTestMap>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(json, find.jtoken_to_string);

            item.jtoken_to_string = null;
            Assert.Equal(1, orm.Update<JTokenTestMap>().SetSource(item).ExecuteAffrows());
            Assert.Null(orm.Select<JTokenTestMap>().Where(a => a.id == item.id && a.jtoken_to_string == json).First());
            find = orm.Select<JTokenTestMap>().Where(a => a.id == item.id && a.jtoken_to_string == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Null(find.jtoken_to_string);

            //update set
            json = JToken.FromObject(new { testa = 455, test31 = "666" });
            Assert.Equal(1, orm.Update<JTokenTestMap>().Where(a => a.id == item.id).Set(a => a.jtoken_to_string, json).ExecuteAffrows());
            find = orm.Select<JTokenTestMap>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(json, find.jtoken_to_string);

            Assert.Equal(1, orm.Update<JTokenTestMap>().Where(a => a.id == item.id).Set(a => a.jtoken_to_string, null).ExecuteAffrows());
            Assert.Null(orm.Select<JTokenTestMap>().Where(a => a.id == item.id && a.jtoken_to_string == json).First());
            find = orm.Select<JTokenTestMap>().Where(a => a.id == item.id && a.jtoken_to_string == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Null(find.jtoken_to_string);

            //delete
            Assert.Equal(0, orm.Delete<JTokenTestMap>().Where(a => a.id == item.id && a.jtoken_to_string == json).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<JTokenTestMap>().Where(a => a.id == item.id && a.jtoken_to_string == null).ExecuteAffrows());
            Assert.Null(orm.Select<JTokenTestMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void JTokenWithArrayToString()
        {
            //insert
            var orm = g.pgsql;
            var item = new JTokenTestMap { };
            Assert.Equal(1, orm.Insert<JTokenTestMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<JTokenTestMap>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Null(find.jtoken_to_string);

            var json = JArray.FromObject(new[] { "a", "b" });
            item = new JTokenTestMap { jtoken_to_string = json };
            Assert.Equal(1, orm.Insert<JTokenTestMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<JTokenTestMap>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(json, find.jtoken_to_string);

            //update
            json = JArray.FromObject(new[] { "333", "zzz" });
            item.jtoken_to_string = json;
            Assert.Equal(1, orm.Update<JTokenTestMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<JTokenTestMap>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(json, find.jtoken_to_string);

            item.jtoken_to_string = null;
            Assert.Equal(1, orm.Update<JTokenTestMap>().SetSource(item).ExecuteAffrows());
            Assert.Null(orm.Select<JTokenTestMap>().Where(a => a.id == item.id && a.jtoken_to_string == json).First());
            find = orm.Select<JTokenTestMap>().Where(a => a.id == item.id && a.jtoken_to_string == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Null(find.jtoken_to_string);

            //update set
            json = JArray.FromObject(new[] { "zxzz", "sdfsdf" });
            Assert.Equal(1, orm.Update<JTokenTestMap>().Where(a => a.id == item.id).Set(a => a.jtoken_to_string, json).ExecuteAffrows());
            find = orm.Select<JTokenTestMap>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(json, find.jtoken_to_string);

            Assert.Equal(1, orm.Update<JTokenTestMap>().Where(a => a.id == item.id).Set(a => a.jtoken_to_string, null).ExecuteAffrows());
            Assert.Null(orm.Select<JTokenTestMap>().Where(a => a.id == item.id && a.jtoken_to_string == json).First());
            find = orm.Select<JTokenTestMap>().Where(a => a.id == item.id && a.jtoken_to_string == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Null(find.jtoken_to_string);

            //delete
            Assert.Equal(0, orm.Delete<JTokenTestMap>().Where(a => a.id == item.id && a.jtoken_to_string == json).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<JTokenTestMap>().Where(a => a.id == item.id && a.jtoken_to_string == null).ExecuteAffrows());
            Assert.Null(orm.Select<JTokenTestMap>().Where(a => a.id == item.id).First());
        }

        [Fact]
        public void JArrayToString()
        {
            //insert
            var orm = g.pgsql;
            var item = new JTokenTestMap { };
            Assert.Equal(1, orm.Insert<JTokenTestMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<JTokenTestMap>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Null(find.jarray_to_string);

            var json = JArray.FromObject(new[] { "a", "b" });
            item = new JTokenTestMap { jarray_to_string = json };
            Assert.Equal(1, orm.Insert<JTokenTestMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<JTokenTestMap>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(json, find.jarray_to_string);

            //update
            json = JArray.FromObject(new[] { "333", "zzz" });
            item.jarray_to_string = json;
            Assert.Equal(1, orm.Update<JTokenTestMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<JTokenTestMap>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(json, find.jarray_to_string);

            item.jarray_to_string = null;
            Assert.Equal(1, orm.Update<JTokenTestMap>().SetSource(item).ExecuteAffrows());
            Assert.Null(orm.Select<JTokenTestMap>().Where(a => a.id == item.id && a.jarray_to_string == json).First());
            find = orm.Select<JTokenTestMap>().Where(a => a.id == item.id && a.jarray_to_string == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Null(find.jarray_to_string);

            //update set
            json = JArray.FromObject(new[] { "zxzz", "sdfsdf" });
            Assert.Equal(1, orm.Update<JTokenTestMap>().Where(a => a.id == item.id).Set(a => a.jarray_to_string, json).ExecuteAffrows());
            find = orm.Select<JTokenTestMap>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(json, find.jarray_to_string);

            Assert.Equal(1, orm.Update<JTokenTestMap>().Where(a => a.id == item.id).Set(a => a.jarray_to_string, null).ExecuteAffrows());
            Assert.Null(orm.Select<JTokenTestMap>().Where(a => a.id == item.id && a.jarray_to_string == json).First());
            find = orm.Select<JTokenTestMap>().Where(a => a.id == item.id && a.jarray_to_string == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Null(find.jarray_to_string);

            //delete
            Assert.Equal(0, orm.Delete<JTokenTestMap>().Where(a => a.id == item.id && a.jarray_to_string == json).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<JTokenTestMap>().Where(a => a.id == item.id && a.jarray_to_string == null).ExecuteAffrows());
            Assert.Null(orm.Select<JTokenTestMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void JObjectToString()
        {
            //insert
            var orm = g.pgsql;
            var item = new JTokenTestMap { };
            Assert.Equal(1, orm.Insert<JTokenTestMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<JTokenTestMap>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Null(find.jobject_to_string);

            var json = JObject.FromObject(new { test1 = 1, test2 = "222" });
            item = new JTokenTestMap { jobject_to_string = json };
            Assert.Equal(1, orm.Insert<JTokenTestMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<JTokenTestMap>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(json, find.jobject_to_string);

            //update
            json = JObject.FromObject(new { test2 = 33, test3 = "333" });
            item.jobject_to_string = json;
            Assert.Equal(1, orm.Update<JTokenTestMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<JTokenTestMap>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(json, find.jobject_to_string);

            item.jobject_to_string = null;
            Assert.Equal(1, orm.Update<JTokenTestMap>().SetSource(item).ExecuteAffrows());
            Assert.Null(orm.Select<JTokenTestMap>().Where(a => a.id == item.id && a.jobject_to_string == json).First());
            find = orm.Select<JTokenTestMap>().Where(a => a.id == item.id && a.jobject_to_string == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Null(find.jobject_to_string);

            //update set
            json = JObject.FromObject(new { testa = 455, test31 = "666" });
            Assert.Equal(1, orm.Update<JTokenTestMap>().Where(a => a.id == item.id).Set(a => a.jobject_to_string, json).ExecuteAffrows());
            find = orm.Select<JTokenTestMap>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(json, find.jobject_to_string);

            Assert.Equal(1, orm.Update<JTokenTestMap>().Where(a => a.id == item.id).Set(a => a.jobject_to_string, null).ExecuteAffrows());
            Assert.Null(orm.Select<JTokenTestMap>().Where(a => a.id == item.id && a.jobject_to_string == json).First());
            find = orm.Select<JTokenTestMap>().Where(a => a.id == item.id && a.jobject_to_string == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Null(find.jobject_to_string);

            //delete
            Assert.Equal(0, orm.Delete<JTokenTestMap>().Where(a => a.id == item.id && a.jobject_to_string == json).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<JTokenTestMap>().Where(a => a.id == item.id && a.jobject_to_string == null).ExecuteAffrows());
            Assert.Null(orm.Select<JTokenTestMap>().Where(a => a.id == item.id).First());
        }

        [Fact]
        public void StringToJToken()
        {
            //insert
            var orm = g.pgsql;
            var item = new JTokenTestMap { };
            Assert.Equal(1, orm.Insert<JTokenTestMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<JTokenTestMap>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Null(find.string_to_jtoken);

            var json = JToken.FromObject(new { test1 = 1, test2 = "222" });
            var whereJson = JToken.FromObject(new { test2 = "222" });
            item = new JTokenTestMap { string_to_jtoken = json.ToString() };
            Assert.Equal(1, orm.Insert<JTokenTestMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<JTokenTestMap>().Where(a => a.id == item.id && JToken.Parse(a.string_to_jtoken).Contains(whereJson)).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(json["test2"], JToken.Parse(find.string_to_jtoken)["test2"]);

            //update
            json = JToken.FromObject(new { test2 = 33, test3 = "333" });
            whereJson = JToken.FromObject(new { test3 = "333" });
            item.string_to_jtoken = json.ToString();
            Assert.Equal(1, orm.Update<JTokenTestMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<JTokenTestMap>().Where(a => a.id == item.id && JToken.Parse(a.string_to_jtoken).Contains(whereJson)).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(json["test3"], JToken.Parse(find.string_to_jtoken)["test3"]);

            item.string_to_jtoken = null;
            Assert.Equal(1, orm.Update<JTokenTestMap>().SetSource(item).ExecuteAffrows());
            Assert.Null(orm.Select<JTokenTestMap>().Where(a => a.id == item.id && JToken.Parse(a.string_to_jtoken).Contains(whereJson)).First());
            find = orm.Select<JTokenTestMap>().Where(a => a.id == item.id && a.string_to_jtoken == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Null(find.string_to_jtoken);

            //update set
            json = JToken.FromObject(new { testa = 455, test31 = "666" });
            whereJson = JToken.FromObject(new { test31 = "666" });
            Assert.Equal(1, orm.Update<JTokenTestMap>().Where(a => a.id == item.id).Set(a => a.string_to_jtoken, json.ToString()).ExecuteAffrows());
            find = orm.Select<JTokenTestMap>().Where(a => a.id == item.id && JToken.Parse(a.string_to_jtoken).Contains(whereJson)).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(json["test31"], JToken.Parse(find.string_to_jtoken)["test31"]);

            Assert.Equal(1, orm.Update<JTokenTestMap>().Where(a => a.id == item.id).Set(a => a.string_to_jtoken, null).ExecuteAffrows());
            Assert.Null(orm.Select<JTokenTestMap>().Where(a => a.id == item.id && JToken.Parse(a.string_to_jtoken).Contains(whereJson)).First());
            find = orm.Select<JTokenTestMap>().Where(a => a.id == item.id && a.string_to_jtoken == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Null(find.string_to_jtoken);

            //delete
            Assert.Equal(0, orm.Delete<JTokenTestMap>().Where(a => a.id == item.id && JToken.Parse(a.string_to_jtoken).Contains(whereJson)).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<JTokenTestMap>().Where(a => a.id == item.id && a.string_to_jtoken == null).ExecuteAffrows());
            Assert.Null(orm.Select<JTokenTestMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void StringToJObject()
        {
            //insert
            var orm = g.pgsql;
            var item = new JTokenTestMap { };
            Assert.Equal(1, orm.Insert<JTokenTestMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<JTokenTestMap>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Null(find.string_to_jobject);

            var json = JObject.FromObject(new { test1 = 1, test2 = "222" });
            item = new JTokenTestMap { string_to_jobject = json.ToString() };
            Assert.Equal(1, orm.Insert<JTokenTestMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<JTokenTestMap>().Where(a => a.id == item.id && JObject.Parse(a.string_to_jobject).ContainsKey("test1")).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(json["test2"], JObject.Parse(find.string_to_jobject)["test2"]);

            //update
            json = JObject.FromObject(new { test2 = 33, test3 = "333" });
            item.string_to_jobject = json.ToString();
            Assert.Equal(1, orm.Update<JTokenTestMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<JTokenTestMap>().Where(a => a.id == item.id && JObject.Parse(a.string_to_jobject).ContainsKey("test3")).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(json["test3"], JObject.Parse(find.string_to_jobject)["test3"]);

            item.string_to_jobject = null;
            Assert.Equal(1, orm.Update<JTokenTestMap>().SetSource(item).ExecuteAffrows());
            Assert.Null(orm.Select<JTokenTestMap>().Where(a => a.id == item.id && JObject.Parse(a.string_to_jobject).ContainsKey("test3")).First());
            find = orm.Select<JTokenTestMap>().Where(a => a.id == item.id && a.string_to_jobject == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Null(find.string_to_jobject);

            //update set
            json = JObject.FromObject(new { testa = 455, test31 = "666" });
            Assert.Equal(1, orm.Update<JTokenTestMap>().Where(a => a.id == item.id).Set(a => a.string_to_jobject, json.ToString()).ExecuteAffrows());
            find = orm.Select<JTokenTestMap>().Where(a => a.id == item.id && JObject.Parse(a.string_to_jobject).ContainsKey("test31")).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(json["test31"], JObject.Parse(find.string_to_jobject)["test31"]);

            Assert.Equal(1, orm.Update<JTokenTestMap>().Where(a => a.id == item.id).Set(a => a.string_to_jobject, null).ExecuteAffrows());
            Assert.Null(orm.Select<JTokenTestMap>().Where(a => a.id == item.id && JObject.Parse(a.string_to_jobject).ContainsKey("test31")).First());
            find = orm.Select<JTokenTestMap>().Where(a => a.id == item.id && a.string_to_jobject == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Null(find.string_to_jobject);

            //delete
            Assert.Equal(0, orm.Delete<JTokenTestMap>().Where(a => a.id == item.id && JObject.Parse(a.string_to_jobject).ContainsKey("test31")).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<JTokenTestMap>().Where(a => a.id == item.id && a.string_to_jobject == null).ExecuteAffrows());
            Assert.Null(orm.Select<JTokenTestMap>().Where(a => a.id == item.id).First());
        }
        [Fact]
        public void StringToJArray()
        {
            //insert
            var orm = g.pgsql;
            var item = new JTokenTestMap { };
            Assert.Equal(1, orm.Insert<JTokenTestMap>().AppendData(item).ExecuteAffrows());
            var find = orm.Select<JTokenTestMap>().Where(a => a.id == item.id).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Null(find.string_to_jarray);

            var json = JArray.FromObject(new[] { "aa", "bb" });
            item = new JTokenTestMap { string_to_jarray = json.ToString() };
            Assert.Equal(1, orm.Insert<JTokenTestMap>().AppendData(item).ExecuteAffrows());
            find = orm.Select<JTokenTestMap>().Where(a => a.id == item.id && JArray.Parse(a.string_to_jarray).Contains("bb")).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(json[1], JArray.Parse(find.string_to_jarray)[1]);

            //update
            json = JArray.FromObject(new[] { "aa", "dddd" });
            item.string_to_jarray = json.ToString();
            Assert.Equal(1, orm.Update<JTokenTestMap>().SetSource(item).ExecuteAffrows());
            find = orm.Select<JTokenTestMap>().Where(a => a.id == item.id && JArray.Parse(a.string_to_jarray).Contains("dddd")).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(json[1], JArray.Parse(find.string_to_jarray)[1]);

            item.string_to_jarray = null;
            Assert.Equal(1, orm.Update<JTokenTestMap>().SetSource(item).ExecuteAffrows());
            Assert.Null(orm.Select<JTokenTestMap>().Where(a => a.id == item.id && JArray.Parse(a.string_to_jarray).Contains("dddd")).First());
            find = orm.Select<JTokenTestMap>().Where(a => a.id == item.id && a.string_to_jarray == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Null(find.string_to_jarray);

            //update set
            json = JArray.FromObject(new[] { "aa", "bdfdfb" });
            Assert.Equal(1, orm.Update<JTokenTestMap>().Where(a => a.id == item.id).Set(a => a.string_to_jarray, json.ToString()).ExecuteAffrows());
            find = orm.Select<JTokenTestMap>().Where(a => a.id == item.id && JArray.Parse(a.string_to_jarray).Contains("bdfdfb")).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Equal(json[1], JArray.Parse(find.string_to_jarray)[1]);

            Assert.Equal(1, orm.Update<JTokenTestMap>().Where(a => a.id == item.id).Set(a => a.string_to_jarray, null).ExecuteAffrows());
            Assert.Null(orm.Select<JTokenTestMap>().Where(a => a.id == item.id && JArray.Parse(a.string_to_jarray).Contains("bdfdfb")).First());
            find = orm.Select<JTokenTestMap>().Where(a => a.id == item.id && a.string_to_jarray == null).First();
            Assert.NotNull(find);
            Assert.Equal(item.id, find.id);
            Assert.Null(find.string_to_jarray);

            //delete
            Assert.Equal(0, orm.Delete<JTokenTestMap>().Where(a => a.id == item.id && JArray.Parse(a.string_to_jarray).Contains("bdfdfb")).ExecuteAffrows());
            Assert.Equal(1, orm.Delete<JTokenTestMap>().Where(a => a.id == item.id && a.string_to_jarray == null).ExecuteAffrows());
            Assert.Null(orm.Select<JTokenTestMap>().Where(a => a.id == item.id).First());
        }
    }
}
