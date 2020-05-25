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
        CurdAfterLog _curdLog;
        public ValuesController(SongContext songContext, IFreeSql orm1, CurdAfterLog curdLog)
        {
            _songContext = songContext;
            _orm = orm1;
            _curdLog = curdLog;

        }

        // GET api/values
        [HttpGet]
        async public Task<string> Get()
        {
            _orm.SetDbContextOptions(opt => {
                opt.OnEntityChange = changeReport => {
                    Console.WriteLine(changeReport);
                };
            });

            long id = 0;

            try
            {

                var repos2Song = _orm.GetRepository<Song, int>();
                repos2Song.Where(a => a.Id > 10).ToList();
                //查询结果，进入 states

                var song = new Song { Title = "empty" };
                repos2Song.Insert(song);
                id = song.Id;

                var adds = Enumerable.Range(0, 100)
                    .Select(a => new Song { CreateTime = DateTime.Now, Title = "xxxx" + a, Url = "url222" })
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
                    Name = "testaddsublist"
                };
                ctx.Tags.Add(tag);


                ctx.UnitOfWork.GetOrBeginTransaction();

                var tagAsync = new Tag
                {
                    Name = "testaddsublist"
                };
                await ctx.Tags.AddAsync(tagAsync);


                ctx.Songs.Select.Where(a => a.Id > 10).ToList();
                //查询结果，进入 states

                song = new Song { Title = "empty" };
                //可插入的 song

                ctx.Songs.Add(song);
                id = song.Id;
                //因有自增类型，立即开启事务执行SQL，返回自增值

                adds = Enumerable.Range(0, 100)
                    .Select(a => new Song { CreateTime = DateTime.Now, Title = "xxxx" + a, Url = "url222" })
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

                    song = new Song { Title = "empty" };
                    reposSong.Insert(song);
                    id = song.Id;

                    adds = Enumerable.Range(0, 100)
                        .Select(a => new Song { CreateTime = DateTime.Now, Title = "xxxx" + a, Url = "url222" })
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



                using (ctx = new SongContext())
                {

                    song = new Song { Title = "empty" };
                    await ctx.Songs.AddAsync(song);
                    id = song.Id;

                    adds = Enumerable.Range(0, 100)
                        .Select(a => new Song { CreateTime = DateTime.Now, Title = "xxxx" + a, Url = "url222" })
                        .ToList();
                    await ctx.Songs.AddRangeAsync(adds);

                    for (var a = 0; a < adds.Count; a++)
                        adds[a].Title = "dkdkdkdk" + a;

                    ctx.Songs.UpdateRange(adds);

                    ctx.Songs.RemoveRange(adds.Skip(10).Take(20).ToList());

                    //ctx.Songs.Update(adds.First());

                    adds.Last().Url = "skldfjlksdjglkjjcccc";
                    ctx.Songs.Update(adds.Last());

                    //throw new Exception("回滚");

                    await ctx.SaveChangesAsync();
                }
            }
            catch
            {
                var item = await _orm.Select<Song>().Where(a => a.Id == id).FirstAsync();

                throw;
            }

            var item22 = await _orm.Select<Song>().Where(a => a.Id == id).FirstAsync();
            var item33 = await _orm.Select<Song>().Where(a => a.Id > id).ToListAsync();

            return item22.Id.ToString() + "\r\n\r\n" + _curdLog.Sb.ToString();
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<object> Get(int id)
        {
            return _orm.Select<Song>().Where(a => a.Id == id).First();
        }

        [HttpGet("get{id}")]
        public ActionResult<string> Get2(int id)
        {
            var item1 = _orm.Select<Song>().Where(a => a.Id == id).First();
            var item2 = _orm.Select<Song>().Where(a => a.Id == id).First();
            return _curdLog.Sb.ToString();
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
