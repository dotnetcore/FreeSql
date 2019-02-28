using Microsoft.AspNetCore.Mvc;
using repository_01.Repositorys;
using restful.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace restful.Controllers {


	[Route("restapi/[controller]")]
	public class SongsController : Controller {

		SongRepository _songRepository;

		public SongsController(IFreeSql fsql) {
			_songRepository = new SongRepository(fsql);

			//test code
			var curd1 = fsql.GetRepository<Song, int>();
			var curd2 = fsql.GetRepository<Song, string>();
			var curd3 = fsql.GetRepository<Song, Guid>();
			var curd4 = fsql.GetGuidRepository<Song>();
		}

		[HttpGet]
		public Task<List<Song>> GetItems([FromQuery] string key, [FromQuery] int page = 1, [FromQuery] int limit = 20) {
			return _songRepository.Select.WhereIf(!string.IsNullOrEmpty(key), a => a.Title.Contains(key)).Page(page, limit).ToListAsync();
		}

		[HttpGet("{id}")]
		public Task<Song> GetItem([FromRoute] int id) {
			return _songRepository.FindAsync(id);
		}

		public class ModelSong {
			public string title { get; set; }
		}

		[HttpPost, ProducesResponseType(201)]
		public Task<Song> Create([FromBody] ModelSong model) {
			return _songRepository.InsertAsync(new Song { Title = model.title });
		}

		[HttpPut("{id}")]
		public Task Update([FromRoute] int id, [FromBody] ModelSong model) {
			return _songRepository.UpdateAsync(new Song { Id = id, Title = model.title });
		}

		[HttpPatch("{id}")]
		async public Task<Song> UpdateDiy([FromRoute] int id, [FromForm] string title) {
			var up = _songRepository.UpdateDiy.Where(a => a.Id == id);
			if (!string.IsNullOrEmpty(title)) up.Set(a => a.Title, title);
			var ret = await up.ExecuteUpdatedAsync();
			return ret.FirstOrDefault();
		}

		[HttpDelete("{id}"), ProducesResponseType(204)]
		public Task Delete([FromRoute] int id) {
			return _songRepository.DeleteAsync(a => a.Id == id);
		}
	}
}
