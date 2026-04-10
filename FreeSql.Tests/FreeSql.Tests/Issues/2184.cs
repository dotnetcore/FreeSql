using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace FreeSql.Tests.Issues
{
    public class _2184
    {
        public class Tenant
        {
            [Column(IsIdentity = true, IsPrimary = true)]
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        public class User
        {
            [Column(IsIdentity = true, IsPrimary = true)]
            public int Id { get; set; }
            public float WorkDuration { get; set; } = 7.5f;
        }


        public class SignInRecord
        {
            [Column(IsIdentity = true, IsPrimary = true)]
            public int Id { get; set; }

            public int UserId { get; set; }

            public int TenantId { get; set; }

            public DateTime? SignInTime { get; set; }

            [Navigate("UserId")]
            public virtual User? User { get; set; }

        }

        [Fact]
        public void TestCultureES()
        {
            var cultureInfo = new CultureInfo("es-ES");
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            var _fsql = g.sqlite;

            var tenant = new Tenant
            {
                Name = "Default Tenant"
            };

            _fsql.Insert<Tenant>().AppendData(tenant).ExecuteIdentity();

            var user = new User
            {
                WorkDuration = 8.0f,
            };

            var id = (int)_fsql.Insert<User>().AppendData(user).ExecuteIdentity();

            var signInRecord = new SignInRecord
            {
                UserId = id,
                TenantId = 1,
                SignInTime = DateTime.Now
            };
            _fsql.Insert<SignInRecord>().AppendData(signInRecord).ExecuteIdentity();

            var user2 = new User
            {
                WorkDuration = 7.5f,
            };

            var id2 = (int)_fsql.Insert<User>().AppendData(user2).ExecuteIdentity();

            var signInRecord2 = new SignInRecord
            {
                UserId = id2,
                TenantId = 1,
                SignInTime = DateTime.Now
            };
            _fsql.Insert<SignInRecord>().AppendData(signInRecord2).ExecuteIdentity();


            var users = _fsql.Select<User>().ToList();

            foreach (var u in users)
            {
                var res = $"ID: {u.Id}, WorkDuration: {u.WorkDuration}";
                if (u.Id == 2)
                {
                    Xunit.Assert.Equal("ID: 2, WorkDuration: 7,5", res);
                }
            }

            var records = _fsql!.Select<SignInRecord, User, Tenant>()
                .LeftJoin((r, u, t) => r.UserId == u.Id)
                .LeftJoin((r, u, t) => r.TenantId == t.Id)
                .ToList((r, u, t) => new
                {
                    r.UserId,
                    u.WorkDuration
                });

            foreach (var record in records)
            {
                var res = $"UserID: {record.UserId},  WorkDuration: {record.WorkDuration}";
                if (record.UserId == 2)
                {
                    Xunit.Assert.Equal("UserID: 2,  WorkDuration: 7,5", res);
                }
            }
        }
    }

}
