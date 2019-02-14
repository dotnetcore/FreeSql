using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FreeSql.RESTful.Demo.Entity {
	public class Song {

		public int Id { get; set; }
		public string Title { get; set; }
	}
}
