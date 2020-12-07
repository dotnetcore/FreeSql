using FreeSql;
using FreeSql.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
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
            //repoSong.Insert(new Song());
            //repoDetail.Insert(new Detail());
            //repoSong2.Insert(new Song());

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

    public static class IdleBusExtesions
    {
        static AsyncLocal<string> AsyncLocalTenantId = new AsyncLocal<string>();
        public static IdleBus<IFreeSql> ChangeTenant(this IdleBus<IFreeSql> ib, string tenantId)
        {
            AsyncLocalTenantId.Value = tenantId;
            return ib;
        }
        public static IFreeSql Get(this IdleBus<IFreeSql> ib) => ib.Get(AsyncLocalTenantId.Value ?? "default");
        public static IBaseRepository<T> GetRepository<T>(this IdleBus<IFreeSql> ib) where T : class => ib.Get().GetRepository<T>();

        static void test()
        {
            IdleBus<IFreeSql> ib = null; //单例注入

            var fsql = ib.Get(); //获取当前租户对应的 IFreeSql

            var fsql00102 = ib.ChangeTenant("00102").Get(); //切换租户，后面的操作都是针对 00102

            var songRepository = ib.GetRepository<Song>();
            var detailRepository = ib.GetRepository<Detail>();
        }

        public static IServiceCollection AddRepository(this IServiceCollection services, params Assembly[] assemblies)
        {
            services.AddScoped(typeof(IBaseRepository<>), typeof(YourDefaultRepository<>));
            services.AddScoped(typeof(BaseRepository<>), typeof(YourDefaultRepository<>));

            services.AddScoped(typeof(IBaseRepository<,>), typeof(YourDefaultRepository<,>));
            services.AddScoped(typeof(BaseRepository<,>), typeof(YourDefaultRepository<,>));

            if (assemblies?.Any() == true)
                foreach (var asse in assemblies)
                    foreach (var repo in asse.GetTypes().Where(a => a.IsAbstract == false && typeof(IBaseRepository).IsAssignableFrom(a)))
                        services.AddScoped(repo);

            return services;
        }
    }

    class YourDefaultRepository<T> : BaseRepository<T> where T : class
    {
        public YourDefaultRepository(IdleBus<IFreeSql> ib) : base(ib.Get(), null, null) { }
    }
    class YourDefaultRepository<T, TKey> : BaseRepository<T, TKey> where T : class
    {
        public YourDefaultRepository(IdleBus<IFreeSql> ib) : base(ib.Get(), null, null) { }
    }
}
