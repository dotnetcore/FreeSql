using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Numerics;
using Xunit;

namespace FreeSql.Tests.DataAnnotations
{
    public class OneToManyTest
    {
        #region 约定一对多
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
                    mto_userid = uid,
                    title = "测试标标题" + (a * 3) + Guid.NewGuid().ToString("N"),
                    createtime = DateTime.Now
                };
                topics[a * 3 + 1] = new mto_topic
                {
                    id = Guid.NewGuid(),
                    mto_userid = uid,
                    title = "测试标标题" + (a * 3 + 1) + Guid.NewGuid().ToString("N"),
                    createtime = DateTime.Now
                };
                topics[a * 3 + 2] = new mto_topic
                {
                    id = Guid.NewGuid(),
                    mto_userid = uid,
                    title = "测试标标题" + (a * 3 + 2) + Guid.NewGuid().ToString("N"),
                    createtime = DateTime.Now
                };
            }
            g.sqlite.Insert(users).ExecuteAffrows();
            g.sqlite.Insert(topics).ExecuteAffrows();

            var select1 = g.sqlite.Select<mto_user>().Limit(10).OrderByDescending(a => a.createtime).ToList(true);

            var select2 = g.sqlite.Select<mto_user>().IncludeMany(a => a.mto_topics).Limit(10).OrderByDescending(a => a.createtime).ToList(true);
        }

        public class mto_user
        {
            public Guid id { get; set; }
            public string username { get; set; }
            public DateTime createtime { get; set; }

            public virtual List<mto_topic> mto_topics { get; set; }
        }
        public class mto_topic
        {
            public Guid id { get; set; }
            public Guid mto_userid { get; set; }

            public string title { get; set; }
            public DateTime createtime { get; set; }
        }
        #endregion

        #region 自定义一对多
        [Fact]
        public void Navigate()
        {
            var users = new otm_user_nav[10];
            var topics = new otm_topic_nav[30];
            for (var a = 0; a < users.Length; a++)
            {
                var uid = Guid.NewGuid();
                users[a] = new otm_user_nav
                {
                    id = uid,
                    username = Guid.NewGuid().ToString("N"),
                    createtime = DateTime.Now
                };
                topics[a * 3] = new otm_topic_nav
                {
                    id = Guid.NewGuid(),
                    user_nav_pkid = uid,
                    title = "测试标标题" + (a * 3) + Guid.NewGuid().ToString("N"),
                    createtime = DateTime.Now
                };
                topics[a * 3 + 1] = new otm_topic_nav
                {
                    id = Guid.NewGuid(),
                    user_nav_pkid = uid,
                    title = "测试标标题" + (a * 3 + 1) + Guid.NewGuid().ToString("N"),
                    createtime = DateTime.Now
                };
                topics[a * 3 + 2] = new otm_topic_nav
                {
                    id = Guid.NewGuid(),
                    user_nav_pkid = uid,
                    title = "测试标标题" + (a * 3 + 2) + Guid.NewGuid().ToString("N"),
                    createtime = DateTime.Now
                };
            }
            g.sqlite.Insert(users).ExecuteAffrows();
            g.sqlite.Insert(topics).ExecuteAffrows();

            var select1 = g.sqlite.Select<otm_user_nav>().Limit(10).OrderByDescending(a => a.createtime).ToList(true);

            var select2 = g.sqlite.Select<otm_user_nav>().IncludeMany(a => a.topics).Limit(10).OrderByDescending(a => a.createtime).ToList(true);
        }

        public class otm_user_nav
        {
            public Guid id { get; set; }
            public string username { get; set; }
            public DateTime createtime { get; set; }

            [Navigate("user_nav_pkid")]
            public virtual List<otm_topic_nav> topics { get; set; }
        }
        public class otm_topic_nav
        {
            public Guid id { get; set; }
            public Guid user_nav_pkid { get; set; }

            public string title { get; set; }
            public DateTime createtime { get; set; }
        }
        #endregion
    }
}
