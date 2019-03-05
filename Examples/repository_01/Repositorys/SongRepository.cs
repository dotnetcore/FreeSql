using FreeSql;
using restful.Entitys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace repository_01.Repositorys {
	public class SongRepository : BaseRepository<Song, int> {
		public SongRepository(IFreeSql fsql) : base(fsql, null) {
		}
	}
}
