using Microsoft.AspNetCore.Mvc;
using restful.Entitys;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace restful.Controllers
{


    [Route("restapi/[controller]")]
    public class SongsController : Controller
    {

        IFreeSql _fsql;

        public SongsController(IFreeSql fsql)
        {
            _fsql = fsql;
        }

        [HttpGet]
        public Task<List<Song>> GetItems([FromQuery] string key, [FromQuery] int page = 1, [FromQuery] int limit = 20)
        {
            return _fsql.Select<Song>().WhereIf(!string.IsNullOrEmpty(key), a => a.Title.Contains(key)).Page(page, limit).ToListAsync();
        }

        /// <summary>
        /// curl -X GET "http://localhost:5000/restapi/Songs/GetPagingItems?key=FreeSql&PageNumber=2&PageSize=10" -H  "accept: text/plain"
        /// </summary>
        /// <param name="key"></param>
        /// <param name="pagingInfo"></param>
        /// <returns></returns>
        [HttpGet("GetPagingItems")]
        public Task<List<Song>> GetPagingItems([FromQuery] string key, [FromQuery] PagingInfo pagingInfo)
        {
            return _fsql.Select<Song>().WhereIf(!string.IsNullOrEmpty(key), a => a.Title.Contains(key)).Page(pagingInfo).ToListAsync();
        }

        [HttpGet("{id}")]
        public Task<Song> GetItem([FromRoute] int id)
        {
            return _fsql.Select<Song>().Where(a => a.Id == id).ToOneAsync();
        }

        public class ModelSong
        {
            public string title { get; set; }
        }

        [HttpPost, ProducesResponseType(201)]
        async public Task<Song> Create([FromBody] ModelSong model)
        {
            var ret = await _fsql.Insert<Song>().AppendData(new Song { Title = model.title }).ExecuteInsertedAsync();
            return ret.FirstOrDefault();
        }

        [HttpPut("{id}")]
        async public Task<Song> Update([FromRoute] int id, [FromBody] ModelSong model)
        {
            var ret = await _fsql.Update<Song>().SetSource(new Song { Id = id, Title = model.title }).ExecuteUpdatedAsync();
            return ret.FirstOrDefault();
        }

        [HttpPatch("{id}")]
        async public Task<Song> UpdateDiy([FromRoute] int id, [FromForm] string title)
        {
            var up = _fsql.Update<Song>().Where(a => a.Id == id);
            if (!string.IsNullOrEmpty(title)) up.Set(a => a.Title, title);
            var ret = await up.ExecuteUpdatedAsync();
            return ret.FirstOrDefault();
        }

        [HttpDelete("{id}"), ProducesResponseType(204)]
        async public Task<Song> Delete([FromRoute] int id)
        {
            var ret = await _fsql.Delete<Song>().Where(a => a.Id == id).ExecuteDeletedAsync();
            return ret.FirstOrDefault();
        }
    }
}
