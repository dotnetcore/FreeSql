using FreeSql.DataAnnotations;
using System;
using System.Numerics;
using Xunit;

namespace FreeSql.Tests.DataAnnotations
{
    public class ManyToOneTest
    {
        #region 约定多对一
        [Fact]
        public void Select()
        {
            var users = new mto_user[10];
            var topics = new mto_topic[30];
            for (var a = 0; a < users.Length; a++)
            {
                var uid = Guid.NewGuid();
                users[a] = new mto_user
                {
                    id = uid,
                    username = Guid.NewGuid().ToString("N"),
                    createtime = DateTime.Now
                };
                topics[a * 3] = new mto_topic
                {
                    id = Guid.NewGuid(),
                    userid = uid,
                    title = "测试标标题" + (a * 3) + Guid.NewGuid().ToString("N"),
                    createtime = DateTime.Now
                };
                topics[a * 3 + 1] = new mto_topic
                {
                    id = Guid.NewGuid(),
                    userid = uid,
                    title = "测试标标题" + (a * 3 + 1) + Guid.NewGuid().ToString("N"),
                    createtime = DateTime.Now
                };
                topics[a * 3 + 2] = new mto_topic
                {
                    id = Guid.NewGuid(),
                    userid = uid,
                    title = "测试标标题" + (a * 3 + 2) + Guid.NewGuid().ToString("N"),
                    createtime = DateTime.Now
                };
            }
            g.sqlite.Insert(users).ExecuteAffrows();
            g.sqlite.Insert(topics).ExecuteAffrows();

            var select1 = g.sqlite.Select<mto_topic>().Limit(30).OrderByDescending(a => a.createtime).ToList(true);

            var select2 = g.sqlite.Select<mto_topic>().Include(a => a.user).Limit(30).OrderByDescending(a => a.createtime).ToList(true);

            var firstct = users[0].createtime.AddSeconds(-1);
            var select3 = g.sqlite.Select<mto_topic>().Where(a => a.user.createtime > firstct).Limit(30).OrderByDescending(a => a.createtime).ToList(true);
        }

        public class mto_user
        {
            public Guid id { get; set; }
            public string username { get; set; }
            public DateTime createtime { get; set; }
        }
        public class mto_topic
        {
            public Guid id { get; set; }
            public Guid userid { get; set; }
            public virtual mto_user user { get; set; }

            public string title { get; set; }
            public DateTime createtime { get; set; }
        }
        #endregion

        #region 自定义多对一
        [Fact]
        public void Navigate()
        {
            var users = new mto_user_nav[10];
            var topics = new mto_topic_nav[30];
            for (var a = 0; a < users.Length; a++)
            {
                var uid = Guid.NewGuid();
                users[a] = new mto_user_nav
                {
                    id = uid,
                    username = Guid.NewGuid().ToString("N"),
                    createtime = DateTime.Now
                };
                topics[a * 3] = new mto_topic_nav
                {
                    id = Guid.NewGuid(),
                    user_nav_pkid = uid,
                    title = "测试标标题" + (a * 3) + Guid.NewGuid().ToString("N"),
                    createtime = DateTime.Now
                };
                topics[a * 3 + 1] = new mto_topic_nav
                {
                    id = Guid.NewGuid(),
                    user_nav_pkid = uid,
                    title = "测试标标题" + (a * 3 + 1) + Guid.NewGuid().ToString("N"),
                    createtime = DateTime.Now
                };
                topics[a * 3 + 2] = new mto_topic_nav
                {
                    id = Guid.NewGuid(),
                    user_nav_pkid = uid,
                    title = "测试标标题" + (a * 3 + 2) + Guid.NewGuid().ToString("N"),
                    createtime = DateTime.Now
                };
            }
            g.sqlite.Insert(users).ExecuteAffrows();
            g.sqlite.Insert(topics).ExecuteAffrows();

            var select1 = g.sqlite.Select<mto_topic_nav>().Limit(30).OrderByDescending(a => a.createtime).ToList(true);

            var select2 = g.sqlite.Select<mto_topic_nav>().Include(a => a.user).Limit(30).OrderByDescending(a => a.createtime).ToList(true);

            var firstct = users[0].createtime.AddSeconds(-1);
            var select3 = g.sqlite.Select<mto_topic_nav>().Where(a => a.user.createtime > firstct).Limit(30).OrderByDescending(a => a.createtime).ToList(true);
        }

        public class mto_user_nav
        {
            public Guid id { get; set; }
            public string username { get; set; }
            public DateTime createtime { get; set; }
        }
        public class mto_topic_nav
        {
            public Guid id { get; set; }
            public Guid user_nav_pkid { get; set; }
            [Navigate("user_nav_pkid")]
            public virtual mto_user_nav user { get; set; }

            public string title { get; set; }
            public DateTime createtime { get; set; }
        }
        #endregion
    }
}
