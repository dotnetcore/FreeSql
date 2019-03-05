using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace domain_01.Entitys
{
	/// <summary>
	/// 专辑
	/// </summary>
	public class Album
    {
		public Guid Id { get; set; }

		public string Name { get; set; }

		public virtual ICollection<Song> Songs { get; set; }

		public DateTime PublishTime { get; set; }
	}
}
