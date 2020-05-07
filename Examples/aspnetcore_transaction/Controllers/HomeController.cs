using FreeSql;
using FreeSql.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Threading.Tasks;

namespace aspnetcore_transaction.Controllers
{
    [ApiController]
    [Route("")]
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpGet("1")]
        //[Transactional]
        virtual public object Get([FromServices] BaseRepository<Song> repoSong, [FromServices] BaseRepository<Detail> repoDetail, [FromServices] SongRepository repoSong2,
            [FromServices] SongService serviceSong)
        {
            serviceSong.Test1();
            return "111";
        }

        [HttpGet("2")]
        //[Transactional]
        async virtual public Task<object> GetAsync([FromServices] BaseRepository<Song> repoSong, [FromServices] BaseRepository<Detail> repoDetail, [FromServices] SongRepository repoSong2,
           [FromServices] SongService serviceSong)
        {
            await serviceSong.Test2();
            await serviceSong.Test3();
            return "111";
        }
    }

    public class SongService
    {
        BaseRepository<Song> _repoSong;
        BaseRepository<Detail> _repoDetail;
        SongRepository _repoSong2;

        public SongService(BaseRepository<Song> repoSong, BaseRepository<Detail> repoDetail, SongRepository repoSong2)
        {
            var tb = repoSong.Orm.CodeFirst.GetTableByEntity(typeof(Song));
            _repoSong = repoSong;
            _repoDetail = repoDetail;
            _repoSong2 = repoSong2;
        }

        [Transactional(Propagation = Propagation.Nested)] //sqlite 不能嵌套事务，会锁库的
        public virtual void Test1()
        {
            _repoSong.Insert(new Song());
            _repoDetail.Insert(new Detail());
            _repoSong2.Insert(new Song());
        }

        [Transactional(Propagation = Propagation.Nested)] //sqlite 不能嵌套事务，会锁库的
        async public virtual Task Test2()
        {
            await _repoSong.InsertAsync(new Song());
            await _repoDetail.InsertAsync(new Detail());
            await _repoSong2.InsertAsync(new Song());
        }

        [Transactional(Propagation = Propagation.Nested)] //sqlite 不能嵌套事务，会锁库的
        async public virtual Task<object> Test3()
        {
            await _repoSong.InsertAsync(new Song());
            await _repoDetail.InsertAsync(new Detail());
            await _repoSong2.InsertAsync(new Song());
            return "123";
        }
    }

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
