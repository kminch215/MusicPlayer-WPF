using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer.music
{
    internal class Song
    {
        private string songName;
        private string artist;
        private string filePath;

        public Song(string songName, string artist, string filePath)
        {
            this.songName = songName;
            this.artist = artist;
            this.filePath = filePath;
        }

        public string Title
        {
            get { return songName; }
            set { songName = value; }
        }

        public string Artist
        {
            get { return artist; }
            set { artist = value; }
        }

        public string FilePath
        {
            get { return filePath; }
            set { filePath = value; }   
        }

        public override string ToString()
        {
            return songName + " - " + artist;
        }
    }
}
