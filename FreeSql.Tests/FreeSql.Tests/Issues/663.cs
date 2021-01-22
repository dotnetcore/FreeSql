using FreeSql.DataAnnotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Xunit;

namespace FreeSql.Tests.Issues
{
	public class _663
	{
		[Fact]
		public void MySqlInsertOrUpdate()
		{
            var fsql = g.mysql;
			var rst = fsql.Insert(new[] { new Song_Tag { SongId = 1, TagId = 1 } })
				.NoneParameter()
				.MySqlIgnoreInto()
				.ToSql();
			Assert.Equal(@"INSERT IGNORE INTO `Song_Tag663`(`SongId`, `TagId`) VALUES(1, 1)", rst);

			rst = fsql.InsertOrUpdate<Song_Tag>()
				.SetSource(new[] { new Song_Tag { SongId = 1, TagId = 1 } })
				.IfExistsDoNothing()
				.ToSql();
			Assert.Equal(@"INSERT INTO `Song_Tag663`(`SongId`, `TagId`) SELECT 1, 1 
 FROM dual WHERE NOT EXISTS(SELECT 1 
    FROM `Song_Tag663` a 
    WHERE (a.`SongId` = 1 AND a.`TagId` = 1) 
    limit 0,1)", rst);
		}

		[Table(Name = "Song663")]
		class Song
		{
			[Column(IsIdentity = true)]
			public int Id { set; get; }
			public string Text { set; get; }
			public List<Tag> Tags { set; get; }
		}
		[Table(Name = "Song_Tag663")]
		class Song_Tag
		{
			public int SongId { set; get; }
			public int TagId { set; get; }
			public Song Song { set; get; }
			public Tag Tag { set; get; }
		}
		[Table(Name = "Tag663")]
		class Tag
		{
			[Column(IsIdentity = true)]
			public int Id { set; get; }
			public string Text { set; get; }
			public List<Song> Songs { set; get; }
		}
	}
}
