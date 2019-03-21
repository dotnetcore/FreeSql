using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace dbcontext_01.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {

		IFreeSql _orm;
		public ValuesController(SongContext songContext, IFreeSql orm) {

			_orm = orm;
			
		}

		// GET api/values
		[HttpGet]
        async public Task<string> Get()
        {

			long id = 0;

			try {
				using (var ctx = new SongContext()) {

					ctx.Songs.Select.Where(a => a.Id > 10).ToList();

					var song = new Song { };
					ctx.Songs.Add(song);
					id = song.Id;

					var adds = Enumerable.Range(0, 100)
						.Select(a => new Song { Create_time = DateTime.Now, Is_deleted = false, Title = "xxxx" + a, Url = "url222" })
						.ToList();
					ctx.Songs.AddRange(adds);

					for (var a = 0; a < adds.Count; a++)
						adds[a].Title = "dkdkdkdk" + a;

					ctx.Songs.UpdateRange(adds);

					ctx.Songs.RemoveRange(adds.Skip(10).Take(20).ToList());

					//ctx.Songs.Update(adds.First());

					adds.Last().Url = "skldfjlksdjglkjjcccc";
					ctx.Songs.Update(adds.Last());

					//throw new Exception("回滚");

					ctx.SaveChanges();
				}

				using (var ctx = new SongContext()) {

					var song = new Song { };
					await ctx.Songs.AddAsync(song);
					id = song.Id;

					var adds = Enumerable.Range(0, 100)
						.Select(a => new Song { Create_time = DateTime.Now, Is_deleted = false, Title = "xxxx" + a, Url = "url222" })
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
			} catch {
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
