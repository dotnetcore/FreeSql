using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Xunit;

namespace FreeSql.Tests.DataAnnotations
{
    public class OneToOneTest
    {
        [Fact]
        public void Select()
        {
            var users = new oto_user[10];
            for (var a = 0; a < users.Length; a++)
            {
                var uid = Guid.NewGuid();
                users[a] = new oto_user
                {
                    id = uid,
                    username = Guid.NewGuid().ToString("N"),
                    createtime = DateTime.Now,
                    ext = new oto_user_field
                    {
                        userid = uid,
                        age = a,
                        createtime = DateTime.Now
                    }
                };
            }
            g.sqlite.Insert(users).ExecuteAffrows();
            g.sqlite.Insert(users.Select(a => a.ext).ToArray()).ExecuteAffrows();

            var select1 = g.sqlite.Select<oto_user>().Include(a => a.ext).Limit(10).OrderByDescending(a => a.createtime).ToList(true);

            var select2 = g.sqlite.Select<oto_user_field>().Limit(10).OrderByDescending(a => a.createtime).ToList(true);

            var select3 = g.sqlite.Select<oto_user_field>().Include(a => a.user).Limit(10).OrderByDescending(a => a.createtime).ToList(true);
        }

        public class oto_user
        {
            public Guid id { get; set; }
            public string username { get; set; }
            public DateTime createtime { get; set; }
            public oto_user_field ext { get; set; }
        }
        public class oto_user_field
        {
            [Column(IsPrimary = true)]
            public Guid userid { get; set; }
            public virtual oto_user user { get; set; }

            public int age { get; set; }
            public DateTime createtime { get; set; }
        }
    }

}
