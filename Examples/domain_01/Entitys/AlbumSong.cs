using FreeSql.DataAnnotations;
using System;

namespace domain_01.Entitys {
	public class AlbumSong {

		public Guid AlbumId { get; set; }

		public Guid SongId { get; set; }
	}
}
