using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading;
using Xunit;

namespace FreeSql.Tests.Issues
{
	public class _592
	{
		[Fact]
		public void AdoNet()
		{
            var fsql = g.mysql;
            fsql.Select<park_sys_users, park_sys_userrole>();

            using (var conn = fsql.Ado.MasterPool.Get())
            {
                var list = conn.Value.Select<park_sys_users, park_sys_userrole>()
                    .LeftJoin((rs, le) => rs.user_id == le.ur_userid)
                    .Where((rs, le) => rs.user_id > 0)
                    .Count();
            }
		}

        [Table(Name = "park_sys_users")]
        public partial class park_sys_users
        {
            public park_sys_users()
            {
            }

            [Column(IsIdentity = true)]
            public int user_id { get; set; }
            public string user_loginname { get; set; }
            public string user_name { get; set; }
            public string user_pwd { get; set; }
            public int user_type { get; set; }
            public string user_spotid { get; set; }
            public string user_phone { get; set; }
            public string user_email { get; set; }
            public string user_remark { get; set; }
            public int user_specialrole { get; set; }
            public string user_createuser { get; set; }
            public DateTime user_createtime { get; set; }
            public int user_state { get; set; }
            public int user_managetype { get; set; }
        }

        public partial class park_sys_userrole
        {
            public park_sys_userrole()
            {
            }

            public int ur_id { get; set; }
            public int ur_userid { get; set; }
            public int ur_roleid { get; set; }
        }
    }
}
