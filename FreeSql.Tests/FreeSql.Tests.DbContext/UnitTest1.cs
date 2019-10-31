using FreeSql.DataAnnotations;
using FreeSql;
using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace FreeSql.Tests
{
    public class UnitTest1
    {

        class testenumWhere
        {
            public Guid id { get; set; }
            public testenumWhereType type { get; set; }
        }
        public enum testenumWhereType { Menu, Class, Blaaa }

        [Fact]
        public void Include_ManyToMany()
        {

            g.sqlite.CodeFirst.SyncStructure<Song_tag>();
            g.sqlite.CodeFirst.SyncStructure<Tag>();
            g.sqlite.CodeFirst.SyncStructure<Song>();

            using (var ctx = g.sqlite.CreateDbContext())
            {

                var songs = ctx.Set<Song>().Select
                    .IncludeMany(a => a.Tags)
                    .ToList();

                var tag1 = new Tag
                {
                    Ddd = DateTime.Now.Second,
                    Name = "test_manytoMany_01_中国"
                };
                var tag2 = new Tag
                {
                    Ddd = DateTime.Now.Second,
                    Name = "test_manytoMany_02_美国"
                };
                var tag3 = new Tag
                {
                    Ddd = DateTime.Now.Second,
                    Name = "test_manytoMany_03_日本"
                };
                ctx.AddRange(new[] { tag1, tag2, tag3 });

                var song1 = new Song
                {
                    Create_time = DateTime.Now,
                    Title = "test_manytoMany_01_我是中国人.mp3",
                    Url = "http://ww.baidu.com/"
                };
                var song2 = new Song
                {
                    Create_time = DateTime.Now,
                    Title = "test_manytoMany_02_爱你一万年.mp3",
                    Url = "http://ww.163.com/"
                };
                var song3 = new Song
                {
                    Create_time = DateTime.Now,
                    Title = "test_manytoMany_03_千年等一回.mp3",
                    Url = "http://ww.sina.com/"
                };
                ctx.AddRange(new[] { song1, song2, song3 });

                ctx.AddRange(
                    new[] {
                        new Song_tag { Song_id = song1.Id, Tag_id = tag1.Id },
                        new Song_tag { Song_id = song2.Id, Tag_id = tag1.Id },
                        new Song_tag { Song_id = song3.Id, Tag_id = tag1.Id },
                        new Song_tag { Song_id = song1.Id, Tag_id = tag2.Id },
                        new Song_tag { Song_id = song3.Id, Tag_id = tag2.Id },
                        new Song_tag { Song_id = song3.Id, Tag_id = tag3.Id },
                    }
                );
                ctx.SaveChanges();
            }
        }

        [Fact]
        public void Add()
        {

            g.sqlite.SetDbContextOptions(opt =>
            {
                //opt.EnableAddOrUpdateNavigateList = false;
            });

            g.mysql.Insert<testenumWhere>().AppendData(new testenumWhere { type = testenumWhereType.Blaaa }).ExecuteAffrows();

            var sql = g.mysql.Select<testenumWhere>().Where(a => a.type == testenumWhereType.Blaaa).ToSql();
            var tolist = g.mysql.Select<testenumWhere>().Where(a => a.type == testenumWhereType.Blaaa).ToList();

            //支持 1对多 联级保存

            using (var ctx = new FreeContext(g.sqlite))
            {

                var tags = ctx.Set<Tag>().Select.IncludeMany(a => a.Tags).ToList();

                var tag = new Tag
                {
                    Name = "testaddsublist",
                    Tags = new[] {
                        new Tag { Name = "sub1" },
                        new Tag { Name = "sub2" },
                        new Tag {
                            Name = "sub3",
                            Tags = new[] {
                                new Tag { Name = "sub3_01" }
                            }
                        }
                    }
                };
                ctx.Add(tag);
                ctx.SaveChanges();
            }
        }

        [Fact]
        public void Update()
        {
            //查询 1对多，再联级保存

            using (var ctx = new FreeContext(g.sqlite))
            {

                var tag = ctx.Set<Tag>().Select.First();
                tag.Tags.Add(new Tag { Name = "sub3" });
                ctx.Update(tag);
                ctx.SaveChanges();
            }
        }

        public class Song
        {
            [Column(IsIdentity = true)]
            public int Id { get; set; }
            public DateTime? Create_time { get; set; }
            public bool? Is_deleted { get; set; }
            public string Title { get; set; }
            public string Url { get; set; }

            public virtual ICollection<Tag> Tags { get; set; }

            [Column(IsVersion = true)]
            public long versionRow { get; set; }
        }
        public class Song_tag
        {
            public int Song_id { get; set; }
            public virtual Song Song { get; set; }

            public int Tag_id { get; set; }
            public virtual Tag Tag { get; set; }
        }

        public class Tag
        {
            [Column(IsIdentity = true)]
            public int Id { get; set; }
            public int? Parent_id { get; set; }
            public virtual Tag Parent { get; set; }

            public decimal? Ddd { get; set; }
            public string Name { get; set; }

            public virtual ICollection<Song> Songs { get; set; }
            public virtual ICollection<Tag> Tags { get; set; }
        }
    }
}

