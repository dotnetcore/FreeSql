using System;
using System.ComponentModel;
using System.Threading.Tasks;
using aspnetcore_transaction.Domain;
using FreeSql;
using FreeSql.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace aspnetcore_transaction.Services
{
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
        public void Test1()
        {
            _repoSong.Insert(new Song());
            _repoDetail.Insert(new Detail());
            _repoSong2.Insert(new Song());
        }
        [Transactional(Propagation = Propagation.Nested)] //sqlite 不能嵌套事务，会锁库的
        public Task Test11()
        {
            return Task.Delay(TimeSpan.FromSeconds(1)).ContinueWith(t => 
                _repoSong.InsertAsync(new Song()));
        }

        [Transactional(Propagation = Propagation.Nested)] //sqlite 不能嵌套事务，会锁库的
        public async Task Test2()
        {
            await _repoSong.InsertAsync(new Song());
            await _repoDetail.InsertAsync(new Detail());
            await _repoSong2.InsertAsync(new Song());
        }

        [Transactional(Propagation = Propagation.Nested)] //sqlite 不能嵌套事务，会锁库的
        public async Task<object> Test3()
        {
            await _repoSong.InsertAsync(new Song());
            await _repoDetail.InsertAsync(new Detail());
            await _repoSong2.InsertAsync(new Song());
            return "123";
        }
    }
}
