using domain_01.Entitys;
using FreeSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace domain_01.Domains
{
    public class MusicDomain
    {
		GuidRepository<Singer> _singerRepostiry => g.orm.GetGuidRepository<Singer>();
		GuidRepository<Album> _albumRepostiry => g.orm.GetGuidRepository<Album>();
		GuidRepository<Song> _songRepostiry => g.orm.GetGuidRepository<Song>();
		GuidRepository<AlbumSong> _albumSongRepostiry => g.orm.GetGuidRepository<AlbumSong>();

		public void SaveSong() {

		}
	}
}
