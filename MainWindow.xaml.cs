using MusicPlayer.music;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MusicPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isPlaying = true;
        private System.Timers.Timer songTimer;
        private List<Playlist> playlists;
        private Queue<Song> songQueue;
        private Stack<Song> songReplayStack;
        private MediaPlayer mediaPlayer;
        private Thread musicPlayingThread;

        public MainWindow()
        {
            InitializeComponent();
            songTimer = new System.Timers.Timer(2000);
            songTimer.Elapsed += UpdateScrollBarPosition;
            SongList.SelectionChanged += SongSelection_Click;
            PreviewKeyDown += MainWindow_PreviewKeyDown;
            VolumeSlider.ValueChanged += VolumeSlider_ValueChanged;
            songQueue = new Queue<Song>();
            songReplayStack = new Stack<Song>();
            mediaPlayer = new MediaPlayer();
            mediaPlayer.IsMuted = false;
            VolumeSlider.Value = 0.5;
            musicPlayingThread = new Thread(PlayMusic);
            playlists = new List<Playlist>();
            playlists.Add(new Playlist("All Songs"));
            PlaylistList.Items.Add(playlists[0].PlaylistName);
            DisplayPlaylist(0);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            AddPlaylist addPlaylist = new AddPlaylist();

            // Subscribe to the PlaylistCreated event
            addPlaylist.PlaylistCreated += AddPlaylistWindow_PlaylistCreated;
            addPlaylist.ShowDialog();
        }

        private void AddPlaylistWindow_PlaylistCreated(object sender, string playlistName)
        {
            // Handle the PlaylistCreated event and use the playlist name
            playlists.Add(new Playlist(playlistName));
            PlaylistList.Items.Add(playlistName);
        }

        private void AddSongButton_Click(object sender, RoutedEventArgs e)
        {
            AddSong addSong = new AddSong();

            //Subscribe to the AddSongEvent event
            addSong.AddSongEvent += AddSongToWindow;

            addSong.ShowDialog();
        }

        private void DeleteSong_Click(object sender, RoutedEventArgs e)
        {
            DeleteSongLable.Visibility = Visibility.Visible;
            SongList.SelectionChanged -= SongSelection_Click;
            SongList.SelectionChanged += RemoveSongSelection_Click;
        }

        private void AddSongToWindow(string songName, string artistName, string songFilePath)
        {
            Song newSong = new Song(songName, artistName, songFilePath);

            if (PlaylistList.SelectedIndex == -1 || PlaylistList.SelectedIndex == 0)
            {
                playlists[0].addSong(newSong);
                Console.WriteLine("New Song Added");
                SortAndDisplaySongList(0);
            }
            else
            {
                playlists[0].addSong(newSong);
                playlists[PlaylistList.SelectedIndex].addSong(newSong);
                SortAndDisplaySongList(PlaylistList.SelectedIndex);
                SortAndDisplaySongList(0);
                DisplayPlaylist(PlaylistList.SelectedIndex);
            }
        }

        private void PlaylistSelected_Click(object sender, RoutedEventArgs e)
        {
            DisplayPlaylist(PlaylistList.SelectedIndex);
        }

        private void SongSelection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string songInformation = playlists[PlaylistList.SelectedIndex].SongPlaylist[SongList.SelectedIndex].ToString();

                MusicControls.Visibility = Visibility.Visible;
                SongInformationDisplay.Text = songInformation;
                songQueue.Clear();

                for(int i  = SongList.SelectedIndex; i <  SongList.Items.Count; i++)
                {
                    songQueue.Enqueue((Song)playlists[PlaylistList.SelectedIndex].SongPlaylist[i]);
                }

                Console.WriteLine("Thread: " + musicPlayingThread.ThreadState);
                // Check if music is already playing
                if (musicPlayingThread.ThreadState != ThreadState.Stopped)
                {
                    // If no song is currently playing, start a new thread
                    musicPlayingThread.Start();
                }
                else
                {
                    PlayMusic();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("catch in Main");
                // Handle any exceptions
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private void RemoveSongSelection_Click(object sender, RoutedEventArgs e)
        {
            int index = SongList.SelectedIndex;
            String songName = SongList.SelectedValue.ToString();
            Console.WriteLine("index: " + index + " songName: " + songName);

            if (PlaylistList.SelectedIndex == -1 || PlaylistList.SelectedIndex == 0)
            {
                playlists[0].SongPlaylist.RemoveAt(index);
                SongList.SelectionChanged -= RemoveSongSelection_Click;
                SongList.SelectionChanged += SongSelection_Click;
                DeleteSongLable.Visibility = Visibility.Hidden;
                SortAndDisplaySongList(0);
            }
            else
            {
                for(int i = 0; i < playlists[0].SongPlaylist.Count; i++)
                {
                    if(songName == playlists[0].SongPlaylist[i].ToString())
                    {
                        playlists[0].SongPlaylist.RemoveAt(i);
                        break;
                    }
                }
                playlists[PlaylistList.SelectedIndex].SongPlaylist.RemoveAt(index);
                SongList.SelectionChanged -= RemoveSongSelection_Click;
                SongList.SelectionChanged += SongSelection_Click;
                DeleteSongLable.Visibility = Visibility.Hidden;
                SortAndDisplaySongList(PlaylistList.SelectedIndex);
                SortAndDisplaySongList(0);
                DisplayPlaylist(PlaylistList.SelectedIndex);
            }
        }

        public void ClearSongList()
        {
            SongList.SelectionChanged -= SongSelection_Click;
            SongList.Items.Clear();
            SongList.SelectionChanged += SongSelection_Click;
        }

        public void DisplayPlaylist(int playlistIndex)
        {
            ClearSongList();
            foreach (Song s in playlists[playlistIndex].SongPlaylist)
            {
                SongList.Items.Add(s);
            }
        }

        public void UpdatePlaylistList()
        {
            //PlaylistList.Items.Clear();
            foreach(Playlist p in playlists)
            {
                PlaylistList.Items.Add((string)p.PlaylistName);
            }
        }

        public void SortAndDisplaySongList(int playlistIndex)
        {
            Console.WriteLine(playlists[playlistIndex].SongPlaylist.Count);
            if (playlistIndex >= 0 && playlistIndex < playlists.Count)
            {
                Playlist playlist = playlists[playlistIndex];

                // Bubble sort songs by song title
                for (int i = 0; i < playlist.SongPlaylist.Count - 1; i++)
                {
                    for (int j = 0; j < playlist.SongPlaylist.Count - 1 - i; j++)
                    {
                        // Compare song names and swap if necessary
                        if (string.Compare(playlist.SongPlaylist[j].Title, playlist.SongPlaylist[j + 1].Title) > 0)
                        {
                            // Swap songs
                            Song temp = playlist.SongPlaylist[j];
                            playlist.SongPlaylist[j] = playlist.SongPlaylist[j + 1];
                            playlist.SongPlaylist[j + 1] = temp;
                        }
                    }
                }

                // Clear the song list
                ClearSongList();

                // Add sorted songs to the song list
                foreach (Song song in playlists[playlistIndex].SongPlaylist)
                {
                    Console.WriteLine();
                    SongList.Items.Add(song);
                }
                   
            }
        }

        private void PlayMusic()
        {
            Dispatcher.Invoke(() =>
            {
                string filePath = songQueue.First().FilePath;
                string verbatimString = filePath;
                mediaPlayer.Open(new Uri(verbatimString));
                ExtractAlbumArt(verbatimString);
                PlayPauseButton.Source = new BitmapImage(new Uri("/images/pause.png", UriKind.RelativeOrAbsolute));
                mediaPlayer.Play();
                songTimer.Enabled = true;
            });
        }

        private void ExtractAlbumArt(string filePath)
        {
            SongsImage.Source = null;
            try
            {
                using (var file = TagLib.File.Create(filePath))
                {
                    var pictures = file.Tag.Pictures;
                    if (pictures.Length >= 1)
                    {
                        var picture = pictures[0];
                        using (MemoryStream ms = new MemoryStream(picture.Data.Data))
                        {
                            BitmapImage image = new BitmapImage();
                            image.BeginInit();
                            image.CacheOption = BitmapCacheOption.OnLoad;
                            image.StreamSource = ms;
                            image.EndInit();

                            SongsImage.Source = image;
                        }
                    }
                    else
                    {
                        Console.WriteLine("No album art found");
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        // This method updates the position of the scroll bar based on the elapsed time of the song
        private void UpdateScrollBarPosition(object source, ElapsedEventArgs e)
        {
            try
            { 
                Dispatcher.Invoke(() =>
                {
                    // Get the current position and duration of the song
                    double currentPosition = mediaPlayer.Position.TotalSeconds;
                    double totalDuration = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                    Console.WriteLine(currentPosition + " " + totalDuration);

                    // Calculate the percentage of the song that has been played
                    double percentagePlayed = (currentPosition / totalDuration) * 10;

                    // Update the position of the scroll bar
                    TimeBar.Value = percentagePlayed;
                });
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Update the volume of the MediaPlayer based on the slider value
            if (mediaPlayer != null)
            {
                mediaPlayer.Volume = VolumeSlider.Value;
            }
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Check if the pressed key is the desired key
            if (e.Key == Key.Play || e.Key == Key.MediaPlayPause)
            {
                Play_Click(null, null);                
            }
            else if (e.Key == Key.MediaNextTrack)
            {
                Next_Click(null, null);
            }
            else if(e.Key == Key.MediaPreviousTrack)
            {
                Previous_Click(null, null);
            }
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Dequeue the current song
                Song removed = songQueue.Dequeue();
                songReplayStack.Push(removed);

                // Check if there are more songs in the queue
                if (songQueue.Count > 0)
                {
                    // If there are more songs, play the next one
                    SongInformationDisplay.Text = songQueue.First().Title + " - " + songQueue.First().Artist;
                    PlayMusic();
                }
                else
                {
                    // If the queue is empty, stop playback
                    mediaPlayer.Stop();
                    MessageBox.Show("End of playlist/queue!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while playing next song: " + ex.Message);
                // Handle any exceptions
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                // Check if there are more songs in the queue
                if (songReplayStack.Count > 0)
                {
                    // Pop the last song off the stack and then add it to the front of the queue
                    Queue<Song> tempQueue = new Queue<Song>();
                    tempQueue.Enqueue(songReplayStack.Pop());
                    foreach (Song s in songQueue)
                    {
                        tempQueue.Enqueue(s);
                    }
                    songQueue = tempQueue;

                    // If there are more songs, play the next one
                    SongInformationDisplay.Text = songQueue.First().Title + " - " + songQueue.First().Artist;
                    PlayMusic();
                }
                else
                {
                    // If the queue is empty, stop playback
                    MessageBox.Show("No previous song!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while playing next song: " + ex.Message);
                // Handle any exceptions
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            isPlaying = !isPlaying;
            try
            {
                if(isPlaying)
                {
                    PlayPauseButton.Source = new BitmapImage(new Uri("/images/pause.png", UriKind.RelativeOrAbsolute));
                    mediaPlayer.Play();
                }
                else
                {
                    PlayPauseButton.Source = new BitmapImage(new Uri("/images/play.png", UriKind.RelativeOrAbsolute));
                    mediaPlayer.Pause();
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }
    }
}
