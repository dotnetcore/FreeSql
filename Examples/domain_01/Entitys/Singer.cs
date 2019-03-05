using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace domain_01.Entitys
{
	/// <summary>
	/// 歌手
	/// </summary>
    public class Singer
    {
		public Guid Id { get; set; }

		public string Nickname { get; set; }

		public DateTime RegTime { get; set; } = DateTime.Now;
	}
}
