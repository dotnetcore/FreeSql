using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FreeSql;
using Microsoft.AspNetCore.Mvc;

namespace dbcontext_01.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {

        IFreeSql _orm;
        SongContext _songContext;
        public ValuesController(SongContext songContext,
            IFreeSql orm1, IFreeSql orm2,
            IFreeSql<long> orm3
            )
        {
            _songContext = songContext;
            _orm = orm1;

        }

        // GET api/values
        [HttpGet]
        async public Task<string> Get()
        {

            long id = 0;

            try
            {

                var repos2Song = _orm.GetRepository<Song, int>();
                repos2Song.Where(a => a.Id > 10).ToList();
                //查询结果，进入 states

                var song = new Song { };
                repos2Song.Insert(song);
                id = song.Id;

                var adds = Enumerable.Range(0, 100)
                    .Select(a => new Song { Create_time = DateTime.Now, Is_deleted = false, Title = "xxxx" + a, Url = "url222" })
                    .ToList();
                //创建一堆无主键值

                repos2Song.Insert(adds);

                for (var a = 0; a < 10; a++)
                    adds[a].Title = "dkdkdkdk" + a;

                repos2Song.Update(adds);
                //批量修改

                repos2Song.Delete(adds.Skip(10).Take(20).ToList());
                //批量删除，10-20 元素的主键值会被清除

                adds.Last().Url = "skldfjlksdjglkjjcccc";
                repos2Song.Update(adds.Last());

                adds.First().Url = "skldfjlksdjglkjjcccc";
                repos2Song.Update(adds.First());


                var ctx = _songContext;
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
                ctx.Tags.Add(tag);


                ctx.UnitOfWork.GetOrBeginTransaction();

                var tagAsync = new Tag
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
                await ctx.Tags.AddAsync(tagAsync);


                ctx.Songs.Select.Where(a => a.Id > 10).ToList();
                //查询结果，进入 states

                song = new Song { };
                //可插入的 song

                ctx.Songs.Add(song);
                id = song.Id;
                //因有自增类型，立即开启事务执行SQL，返回自增值

                adds = Enumerable.Range(0, 100)
                    .Select(a => new Song { Create_time = DateTime.Now, Is_deleted = false, Title = "xxxx" + a, Url = "url222" })
                    .ToList();
                //创建一堆无主键值

                ctx.Songs.AddRange(adds);
                //立即执行，将自增值赋给 adds 所有元素，因为有自增类型，如果其他类型，指定传入主键值，不会立即执行

                for (var a = 0; a < 10; a++)
                    adds[a].Title = "dkdkdkdk" + a;

                ctx.Songs.UpdateRange(adds);
                //批量修改，进入队列

                ctx.Songs.RemoveRange(adds.Skip(10).Take(20).ToList());
                //批量删除，进入队列，完成时 10-20 元素的主键值会被清除

                //ctx.Songs.Update(adds.First());

                adds.Last().Url = "skldfjlksdjglkjjcccc";
                ctx.Songs.Update(adds.Last());

                adds.First().Url = "skldfjlksdjglkjjcccc";
                ctx.Songs.Update(adds.First());

                //单条修改 urls 的值，进入队列

                //throw new Exception("回滚");

                //ctx.Songs.Select.First();
                //这里做一个查询，会立即打包【执行队列】，避免没有提交的数据，影响查询结果

                ctx.SaveChanges();
                //打包【执行队列】，提交事务


                using (var uow = _orm.CreateUnitOfWork())
                {

                    var reposSong = uow.GetRepository<Song, int>();
                    reposSong.Where(a => a.Id > 10).ToList();
                    //查询结果，进入 states

                    song = new Song { };
                    reposSong.Insert(song);
                    id = song.Id;

                    adds = Enumerable.Range(0, 100)
                        .Select(a => new Song { Create_time = DateTime.Now, Is_deleted = false, Title = "xxxx" + a, Url = "url222" })
                        .ToList();
                    //创建一堆无主键值

                    reposSong.Insert(adds);

                    for (var a = 0; a < 10; a++)
                        adds[a].Title = "dkdkdkdk" + a;

                    reposSong.Update(adds);
                    //批量修改

                    reposSong.Delete(adds.Skip(10).Take(20).ToList());
                    //批量删除，10-20 元素的主键值会被清除

                    adds.Last().Url = "skldfjlksdjglkjjcccc";
                    reposSong.Update(adds.Last());

                    adds.First().Url = "skldfjlksdjglkjjcccc";
                    reposSong.Update(adds.First());

                    uow.Commit();
                }



                //using (var ctx = new SongContext()) {

                //	var song = new Song { };
                //	await ctx.Songs.AddAsync(song);
                //	id = song.Id;

                //	var adds = Enumerable.Range(0, 100)
                //		.Select(a => new Song { Create_time = DateTime.Now, Is_deleted = false, Title = "xxxx" + a, Url = "url222" })
                //		.ToList();
                //	await ctx.Songs.AddRangeAsync(adds);

                //	for (var a = 0; a < adds.Count; a++)
                //		adds[a].Title = "dkdkdkdk" + a;

                //	ctx.Songs.UpdateRange(adds);

                //	ctx.Songs.RemoveRange(adds.Skip(10).Take(20).ToList());

                //	//ctx.Songs.Update(adds.First());

                //	adds.Last().Url = "skldfjlksdjglkjjcccc";
                //	ctx.Songs.Update(adds.Last());

                //	//throw new Exception("回滚");

                //	await ctx.SaveChangesAsync();
                //}
            }
            catch
            {
                var item = await _orm.Select<Song>().Where(a => a.Id == id).FirstAsync();

                throw;
            }

            var item22 = await _orm.Select<Song>().Where(a => a.Id == id).FirstAsync();
            var item33 = await _orm.Select<Song>().Where(a => a.Id > id).ToListAsync();

            return item22.Id.ToString();
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
