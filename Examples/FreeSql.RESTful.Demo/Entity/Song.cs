using FreeSql.DataAnnotations;

namespace FreeSql.RESTful.Demo.Entity {
	public class Song {

		[Column(IsIdentity = true)]
		public int Id { get; set; }
		public string Title { get; set; }
	}
}
