using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;

namespace efcore_to_freesql.Entitys
{
    public class SongType
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<Song> Songs { get; set; }
    }

    public class Song
    {
        [Column(IsIdentity = true)]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public DateTime CreateTime { get; set; }

        public int TypeId { get; set; }
        public SongType Type { get; set; }
        public List<Tag> Tags { get; set; }

        public int Field1 { get; set; }
        public long RowVersion { get; set; }
    }
    public class Song_tag
    {
        public int Song_id { get; set; }
        public Song Song { get; set; }

        public int Tag_id { get; set; }
        public Tag Tag { get; set; }
    }

    public class Tag
    {
        [Column(IsIdentity = true)]
        public int Id { get; set; }

        public string Name { get; set; }

        public List<Song> Songs { get; set; }
    }
}
