using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using Xunit;

namespace FreeSql.Tests.DataAnnotations
{
    public class ManyToManyTest
    {
        Random rnd = new Random();

        #region 约定多对多
        [Fact]
        public void Select()
        {
            var users = new mtm_user[10];
            var roles = new mtm_role[10];
            var urs = new List<mtm_user_mtm_role>();
            for (var a = 0; a < users.Length; a++)
            {
                var uid = Guid.NewGuid();
                users[a] = new mtm_user
                {
                    id = uid,
                    username = "用户" + a + "_" + Guid.NewGuid().ToString("N"),
                    createtime = DateTime.Now
                };
            }
            g.sqlite.Insert(users).ExecuteAffrows();
            for (var a = 0; a < roles.Length; a++)
            {
                var uid = Guid.NewGuid();
                roles[a] = new mtm_role
                {
                    id = uid,
                    name = "角色" + a + "_" + Guid.NewGuid().ToString("N"),
                    createtime = DateTime.Now
                };
            }
            g.sqlite.Insert(roles).ExecuteAffrows();

            for (var a = 0; a < users.Length; a++)
            {
                for (var b = roles.Length; b >= 0; b--)
                {
                    var ur = new mtm_user_mtm_role
                    {
                        mtm_user_id = users[a].id,
                        mtm_role_id = roles[rnd.Next(roles.Length)].id
                    };
                    if (urs.Where(c => c.mtm_role_id == ur.mtm_role_id && c.mtm_user_id == ur.mtm_user_id).Any() == false)
                        urs.Add(ur);
                }
            }
            g.sqlite.Insert(urs.ToArray()).ExecuteAffrows();

            var select1 = g.sqlite.Select<mtm_user>().Limit(10).OrderByDescending(a => a.createtime).ToList(true);

            var select2 = g.sqlite.Select<mtm_user>().IncludeMany(a => a.mtm_roles).Limit(10).OrderByDescending(a => a.createtime).ToList(true);
        }

        public class mtm_user
        {
            public Guid id { get; set; }
            public string username { get; set; }
            public DateTime createtime { get; set; }

            public virtual List<mtm_role> mtm_roles { get; set; }
        }
        public class mtm_user_mtm_role
        {
            public Guid mtm_user_id { get; set; }
            public Guid mtm_role_id { get; set; }

            public mtm_user mtm_user { get; set; }
            public mtm_role mtm_role { get; set; }
        }
        public class mtm_role
        {
            public Guid id { get; set; }

            public string name { get; set; }
            public DateTime createtime { get; set; }

            public virtual List<mtm_user> mtm_users { get; set; }
        }
        #endregion

        #region 自定义多对多，中间类为约定
        [Fact]
        public void Navigate1()
        {
            var users = new mtm_user_nav1[10];
            var roles = new mtm_role_nav1[10];
            var urs = new List<user_role_nav1>();
            for (var a = 0; a < users.Length; a++)
            {
                var uid = Guid.NewGuid();
                users[a] = new mtm_user_nav1
                {
                    id = uid,
                    username = "用户" + a + "_" + Guid.NewGuid().ToString("N"),
                    createtime = DateTime.Now
                };
            }
            g.sqlite.Insert(users).ExecuteAffrows();
            for (var a = 0; a < roles.Length; a++)
            {
                var uid = Guid.NewGuid();
                roles[a] = new mtm_role_nav1
                {
                    id = uid,
                    name = "角色" + a + "_" + Guid.NewGuid().ToString("N"),
                    createtime = DateTime.Now
                };
            }
            g.sqlite.Insert(roles).ExecuteAffrows();

            for (var a = 0; a < users.Length; a++)
            {
                for (var b = roles.Length; b >= 0; b--)
                {
                    var ur = new user_role_nav1
                    {
                        user_id = users[a].id,
                        role_id = roles[rnd.Next(roles.Length)].id
                    };
                    if (urs.Where(c => c.role_id == ur.role_id && c.user_id == ur.user_id).Any() == false)
                        urs.Add(ur);
                }
            }
            g.sqlite.Insert(urs.ToArray()).ExecuteAffrows();

            var select1 = g.sqlite.Select<mtm_user_nav1>().Limit(10).OrderByDescending(a => a.createtime).ToList(true);

            var select2 = g.sqlite.Select<mtm_user_nav1>().IncludeMany(a => a.roles).Limit(10).OrderByDescending(a => a.createtime).ToList(true);
        }

        public class mtm_user_nav1
        {
            public Guid id { get; set; }
            public string username { get; set; }
            public DateTime createtime { get; set; }

            [Navigate(ManyToMany = typeof(user_role_nav1))]
            public virtual List<mtm_role_nav1> roles { get; set; }
        }
        public class user_role_nav1
        {
            public Guid user_id { get; set; }
            public Guid role_id { get; set; }

            public mtm_user_nav1 user { get; set; }
            public mtm_role_nav1 role { get; set; }
        }
        public class mtm_role_nav1
        {
            public Guid id { get; set; }

            public string name { get; set; }
            public DateTime createtime { get; set; }

            [Navigate(ManyToMany = typeof(user_role_nav1))]
            public virtual List<mtm_user_nav1> users { get; set; }
        }
        #endregion

        #region 自定义多对多，中间表为自定义
        [Fact]
        public void Navigate()
        {
            var users = new mtm_user_nav[10];
            var roles = new mtm_role_nav[10];
            var urs = new List<user_role_nav>();
            for (var a = 0; a < users.Length; a++)
            {
                var uid = Guid.NewGuid();
                users[a] = new mtm_user_nav
                {
                    id = uid,
                    username = "用户" + a + "_" + Guid.NewGuid().ToString("N"),
                    createtime = DateTime.Now
                };
            }
            g.sqlite.Insert(users).ExecuteAffrows();
            for (var a = 0; a < roles.Length; a++)
            {
                var uid = Guid.NewGuid();
                roles[a] = new mtm_role_nav
                {
                    id = uid,
                    name = "角色" + a + "_" + Guid.NewGuid().ToString("N"),
                    createtime = DateTime.Now
                };
            }
            g.sqlite.Insert(roles).ExecuteAffrows();

            for (var a = 0; a < users.Length; a++)
            {
                for (var b = roles.Length; b >= 0; b--)
                {
                    var ur = new user_role_nav
                    {
                        user_pkid = users[a].id,
                        role_pkid = roles[rnd.Next(roles.Length)].id
                    };
                    if (urs.Where(c => c.role_pkid == ur.role_pkid && c.user_pkid == ur.user_pkid).Any() == false)
                        urs.Add(ur);
                }
            }
            g.sqlite.Insert(urs.ToArray()).ExecuteAffrows();

            var select1 = g.sqlite.Select<mtm_user_nav>().Limit(10).OrderByDescending(a => a.createtime).ToList(true);

            var select2 = g.sqlite.Select<mtm_user_nav>().IncludeMany(a => a.roles).Limit(10).OrderByDescending(a => a.createtime).ToList(true);
        }

        public class mtm_user_nav
        {
            public Guid id { get; set; }
            public string username { get; set; }
            public DateTime createtime { get; set; }

            [Navigate(ManyToMany = typeof(user_role_nav))]
            public virtual List<mtm_role_nav> roles { get; set; }
        }
        public class user_role_nav
        {
            public Guid user_pkid { get; set; }
            public Guid role_pkid { get; set; }

            [Navigate("user_pkid")]
            public mtm_user_nav user { get; set; }
            [Navigate("role_pkid")]
            public mtm_role_nav role { get; set; }
        }
        public class mtm_role_nav
        {
            public Guid id { get; set; }

            public string name { get; set; }
            public DateTime createtime { get; set; }

            [Navigate(ManyToMany = typeof(user_role_nav))]
            public virtual List<mtm_user_nav> users { get; set; }
        }
        #endregion
    }
}
