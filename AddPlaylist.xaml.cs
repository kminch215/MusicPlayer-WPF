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

        private void CreatePlaylist_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(NewPlaylistName.Text))
            {
                Console.WriteLine("No playlist name");
            }

            string playlistName = NewPlaylistName.Text;
            // Raise the PlaylistCreated event and pass the playlist name
            PlaylistCreated?.Invoke(this, playlistName);

            // Close the AddPlaylist window after passing the playlist name
            this.Close();
        }
    }
}
