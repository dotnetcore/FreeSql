using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;

namespace domain_01.Entitys {

	/// <summary>
	/// 歌曲
	/// </summary>
	public class Song {

		public Guid Id { get; set; }

		public Guid SingerId { get; set; }
		public virtual Guid Singer { get; set; }

		public string Name { get; set; }

		public string Url { get; set; }

		public virtual ICollection<Album> Albums { get; set; }

		public DateTime RegTime { get; set; } = DateTime.Now;
	}
}
