using FreeSql;
using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;

namespace dbcontext_01
{

    public class SongContext : DbContext
    {

        public DbSet<Song> Songs { get; set; }
        public DbSet<Tag> Tags { get; set; }

        //protected override void OnConfiguring(DbContextOptionsBuilder builder) {
        //	builder.UseFreeSql(dbcontext_01.Startup.Fsql);
        //}
    }


    public class Song
    {
        [Column(IsIdentity = true)]
        public int Id { get; set; }
        public DateTime? Create_time { get; set; }
        public bool? Is_deleted { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }

        public virtual ICollection<Tag> Tags { get; set; }

        [Column(IsVersion = true)]
        public long versionRow { get; set; }
    }
    public class Song_tag
    {
        public int Song_id { get; set; }
        public virtual Song Song { get; set; }

        public int Tag_id { get; set; }
        public virtual Tag Tag { get; set; }
    }

    public class Tag
    {
        [Column(IsIdentity = true)]
        public int Id { get; set; }
        public int? Parent_id { get; set; }
        public virtual Tag Parent { get; set; }

        public decimal? Ddd { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Song> Songs { get; set; }
        public virtual ICollection<Tag> Tags { get; set; }
    }
}
