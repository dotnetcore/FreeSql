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

					id = await ctx.Songs.Insert(new Song { }).ExecuteIdentityAsync();

					var item = await ctx.Songs.Select.Where(a => a.Id == id).FirstAsync();

					throw new Exception("回滚");

				}
			} catch {
				var item = await _orm.Select<Song>().Where(a => a.Id == id).FirstAsync();

				throw;
			}
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
