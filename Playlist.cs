using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer.music
{
    internal class Playlist
    {
        //Instance variables
        private string playlistName;
        private List<Song> songPlaylist;

        //Playlist constructor
        public Playlist(string playlistName)
        {
            this.playlistName = playlistName;
            this.songPlaylist = new List<Song>();
        }

        //Playlist name getter and setter
        public string PlaylistName 
        { 
            get {  return playlistName; } 
            set { playlistName = value; }
        }

        //Song List getter and setter
        public List<Song> SongPlaylist 
        { 
            get { return songPlaylist; } 
            set { songPlaylist = value; }
        }

        //Overriden ToString method
        public override string ToString()
        {
            return playlistName;
        }

        //Method to add a song to the Song List
        public void addSong(Song song)
        {
            songPlaylist.Add(song);
        }

    }
}
