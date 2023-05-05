using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeSql.DataAnnotations;
using FreeSql.Extensions.DynamicEntity;
using Xunit;

namespace FreeSql.Tests.DynamicEntity
{
    public class DynamicEntityTest
    {
        private static IFreeSql fsql = new FreeSqlBuilder().UseConnectionString(DataType.PostgreSQL,
                "Host=192.168.0.36;Port=5432;Username=postgres;Password=123; Database=test;ArrayNullabilityMode=Always;Pooling=true;Minimum Pool Size=1")
            .UseMonitorCommand(d => Console.WriteLine(d.CommandText)).Build();


        [Fact]
        public void NormalTest()
        {
            Type type = DynamicCompileHelper.DynamicBuilder()
                .Class("NormalUsers")
                .Property("Id", typeof(string))
                .Property("Name", typeof(string))
                .Property("Address", typeof(string))
                .Build();
            var dict = new Dictionary<string, object>
            {
                ["Name"] = "张三",
                ["Id"] = Guid.NewGuid().ToString(),
                ["Address"] = "北京市"
            };
            var instance = DynamicCompileHelper.CreateObjectByType(type, dict);
            //根据Type生成表
            fsql.CodeFirst.SyncStructure(type);
            fsql.Insert<object>().AsType(type).AppendData(instance).ExecuteAffrows();
        }

        [Fact]
        public void AttributeTest()
        {
            Type type = DynamicCompileHelper.DynamicBuilder()
                .Class("AttributeUsers", new TableAttribute() { Name = "T_Attribute_User" },
                    new IndexAttribute("Name_Index", "Name", false))
                .Property("Id", typeof(int),
                    new ColumnAttribute() { IsPrimary = true, IsIdentity = true, Position = 1 })
                .Property("Name", typeof(string),
                    new ColumnAttribute() { StringLength = 20, Position = 2 })
                .Property("Address", typeof(string),
                    new ColumnAttribute() { StringLength = 150, Position = 3 })
                .Build();
            var dict = new Dictionary<string, object>
            {
                ["Name"] = "张三",
                ["Address"] = "北京市"
            };
            var instance = DynamicCompileHelper.CreateObjectByType(type, dict);
            //根据Type生成表
            fsql.CodeFirst.SyncStructure(type);
            var insertId = fsql.Insert<object>().AsType(type).AppendData(instance).ExecuteIdentity();
            var select = fsql.Select<object>().AsType(type).ToList();
        }

        [Fact]
        public void SuperClassTest()
        {
            Type type = DynamicCompileHelper.DynamicBuilder()
                .Class("Roles", new TableAttribute() { Name = "T_Role" },
                    new IndexAttribute("Name_Index", "Name", false))
                .SuperClass(typeof(BaseModel))
                .Property("Id", typeof(int),
                    new ColumnAttribute() { IsPrimary = true, IsIdentity = true, Position = 1 })
                .Property("Name", typeof(string),
                    new ColumnAttribute() { StringLength = 20, Position = 2 })
                .Build();
            var dict = new Dictionary<string, object>
            {
                ["Name"] = "系统管理员",
                ["UpdateTime"] = DateTime.Now,
                ["UpdatePerson"] = "admin"
            };
            var instance = DynamicCompileHelper.CreateObjectByType(type, dict);
            //根据Type生成表
            fsql.CodeFirst.SyncStructure(type);
            fsql.Insert<object>().AsType(type).AppendData(instance).ExecuteAffrows();
        }
    }
    public class BaseModel
    {
        [Column(Position = 99)]
        public DateTime UpdateTime
        {
            get; set;
        }

        [Column(Position = 100, StringLength = 20)]
        public string UpdatePerson
        {
            get; set;
        }
    }
}