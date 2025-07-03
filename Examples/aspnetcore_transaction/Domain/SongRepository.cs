using System;
using System.ComponentModel;
using System.Threading.Tasks;
using FreeSql;
using FreeSql.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace aspnetcore_transaction.Domain
{
    public class SongRepository : DefaultRepository<Song, int>
    {
        public SongRepository(UnitOfWorkManager uowm) : base(uowm?.Orm, uowm) { }
    }

    [Description("123")]
    public class Song
    {
        /// <summary>
        /// 自增
        /// </summary>
        [Column(IsIdentity = true)]
        [Description("自增id")]
        public int Id { get; set; }
        public string Title { get; set; }
    }
    public class Detail
    {
        [Column(IsIdentity = true)]
        public int Id { get; set; }

        public int SongId { get; set; }
        public string Title { get; set; }
    }
}
