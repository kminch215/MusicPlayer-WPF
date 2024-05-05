using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MusicPlayer
{
    /// <summary>
    /// Interaction logic for AddPlaylist.xaml
    /// </summary>
    public partial class AddPlaylist : Window
    {
        public event EventHandler<string> PlaylistCreated;

        public AddPlaylist()
        {
            InitializeComponent();
        }

        //Logic for Creating Playlist pop up window
        private void CreatePlaylist_Click(object sender, RoutedEventArgs e)
        {
            //If the user does not enter a playlist name
            if(string.IsNullOrEmpty(NewPlaylistName.Text))
            {
                MessageBox.Show("No playlist name");
            }

            string playlistName = NewPlaylistName.Text;
            // Raise the PlaylistCreated event and pass the playlist name
            PlaylistCreated?.Invoke(this, playlistName);

            // Close the AddPlaylist window after passing the playlist name
            this.Close();
        }
    }
}
