using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer.music
{
    internal class Song
    {
        //Instance variables
        private string songName;
        private string artist;
        private string filePath;

        //Song Constructor
        public Song(string songName, string artist, string filePath)
        {
            this.songName = songName;
            this.artist = artist;
            this.filePath = filePath;
        }

        //title variable getter and setter
        public string Title
        {
            get { return songName; }
            set { songName = value; }
        }

        //artist variable getter and setter
        public string Artist
        {
            get { return artist; }
            set { artist = value; }
        }

        //filePath variable getter and setter
        public string FilePath
        {
            get { return filePath; }
            set { filePath = value; }   
        }

        //Overriden ToString() method
        public override string ToString()
        {
            return songName + " - " + artist;
        }
    }
}
