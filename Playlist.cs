using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer.music
{
    internal class Playlist
    {
        private string playlistName;
        private List<Song> songPlaylist;

        public Playlist(string playlistName)
        {
            this.playlistName = playlistName;
            this.songPlaylist = new List<Song>();
        }

        public string PlaylistName 
        { 
            get {  return playlistName; } 
            set { playlistName = value; }
        }

        public List<Song> SongPlaylist 
        { 
            get { return songPlaylist; } 
            set { songPlaylist = value; }
        }

        public override string ToString()
        {
            return playlistName;
        }

        public void addSong(Song song)
        {
            songPlaylist.Add(song);
        }

    }
}
