using Microsoft.Win32;
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
    /// Interaction logic for AddSong.xaml
    /// </summary>
    public partial class AddSong : Window
    {
        public delegate void SongEventHandler(string arg1, string arg2, string arg3);
        public event SongEventHandler AddSongEvent;          // Declare an event of the delegate type

        public AddSong()
        {
            InitializeComponent();
        }

        private void AddSong_CLick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "MP3 files (*.mp3)|*.mp3|All files (*.*)|*.*";

            //openFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            try
            {
                if (openFileDialog.ShowDialog() == true)
                {
                    string selectedFileName = openFileDialog.FileName;
                    // Check if the selected file is an MP3 file

                    if (!string.IsNullOrEmpty(System.IO.Path.GetExtension(selectedFileName)) && System.IO.Path.GetExtension(selectedFileName).Equals(".mp3", StringComparison.OrdinalIgnoreCase))
                    {
                        // File is valid
                        Console.WriteLine(selectedFileName);
                        OnAddSongEvent(SongName.Text, ArtistName.Text, selectedFileName);
                    }
                    else
                    {
                        // File is not an MP3 file
                        MessageBox.Show("Please select a valid MP3 file.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("catch in AddSong");

                // Exception occurred while opening the file dialog
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }

        // Method to raise the event
        protected virtual void OnAddSongEvent(string arg1, string arg2, string arg3)
        {
            // Check if there are any subscribers to the event
            if (AddSongEvent != null)
            {
                // Raise the event by invoking all subscribed methods
                AddSongEvent.Invoke(arg1, arg2, arg3);
                this.Close();

            }
        }

        private void CancelAddingSong(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
