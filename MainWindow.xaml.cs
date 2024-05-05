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
        //Vairables need to be used in the Music Player
        private bool isPlaying = true;
        private System.Timers.Timer songTimer;
        private List<Playlist> playlists;
        private Queue<Song> songQueue;
        private Stack<Song> songReplayStack;
        private MediaPlayer mediaPlayer;
        private Thread musicPlayingThread;

        //Main constructor that initializes the Music Player environment
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

        /*
         * This method handles a click event when the PlaylistList listbox is right clicked on
         * This is used to add a new playlist to the PlaylistList listbox.
         */
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            //Creates a new instance of the AddPlaylist window
            AddPlaylist addPlaylist = new AddPlaylist();

            // Subscribe to the PlaylistCreated event
            addPlaylist.PlaylistCreated += AddPlaylistWindow_PlaylistCreated;
            //Shows the window to enter the new playlist name
            addPlaylist.ShowDialog();
        }

        /*
         * Helper method for the right click add playlist event that handle the logic to add a new playlist.
         * It will create a new Playlist object with the name of the playlist the user entered, which in the object creation
         * will create a List of Song objects that the user can then add their songs to.
         * 
         * It then add the playlist name to the listbox PlaylistList
         */
        private void AddPlaylistWindow_PlaylistCreated(object sender, string playlistName)
        {
            //Handle the PlaylistCreated event and use the playlist name
            playlists.Add(new Playlist(playlistName));

            //Add the playlist to the PlaylistList listbox
            PlaylistList.Items.Add(playlistName);
        }

        /*
         * This method handles the event where the user clicks the Add Song button.
         */
        private void AddSongButton_Click(object sender, RoutedEventArgs e)
        {
            //Creates an instance of the AddSong object
            AddSong addSong = new AddSong();

            //Subscribe to the AddSongEvent event
            addSong.AddSongEvent += AddSongToWindow;

            //Will open the window attached to the AddSong object 
            addSong.ShowDialog();
        }

        /*
         * Method that handles the click event for the user to delete a song from a playlist
         */
        private void DeleteSong_Click(object sender, RoutedEventArgs e)
        {
            //Sets the visibility of a lable on the window to visible so the user knows that any song they click will be deleted
            DeleteSongLable.Visibility = Visibility.Visible;
            //Removes the SongSelection_Click method for being an event for when a song is selected in the SongList
            SongList.SelectionChanged -= SongSelection_Click;
            //Adds the RemoveSongSelection_Click method to the SelectionChanged event to allow the user to delete the song when they click a song in the SongList
            SongList.SelectionChanged += RemoveSongSelection_Click;
        }


        /*
         * This method is passed three strings: a songs name, an artist name, and a song file paths
         * 
         * Based on this information and also the information of which playlist in the PlaylistList listbox is selected,
         * the Music Player will add the song with these attributes to the selected playlist. If the playlist that is selected is not the
         * All Music playlist, it will also add the song to this playlist in addition to the selected playlist.
         */
        private void AddSongToWindow(string songName, string artistName, string songFilePath)
        {
            //Creating the song object for the new attributes
            Song newSong = new Song(songName, artistName, songFilePath);

            //For when the song is being added to the All Music playlist
            if (PlaylistList.SelectedIndex == -1 || PlaylistList.SelectedIndex == 0)
            {
                playlists[0].addSong(newSong);
                //Sort of the Song Lists
                SortAndDisplaySongList(0);
            }
            //For when the song is being added to a selected playlist
            else
            {
                //Adds the song to the All Music playlist
                playlists[0].addSong(newSong);
                //Adds the song to the selected playlist
                playlists[PlaylistList.SelectedIndex].addSong(newSong);
                //Sorts both of the Song Lists
                SortAndDisplaySongList(PlaylistList.SelectedIndex);
                SortAndDisplaySongList(0);
                //Displays the sorted song playlist
                DisplayPlaylist(PlaylistList.SelectedIndex);
            }
        }

        /*
         * Whenever a SelectionChange event happens in the PlaylistList listbox, it will change the SongList listbox
         * to display the Songs that are in that selected playlist
         */
        private void PlaylistSelected_Click(object sender, RoutedEventArgs e)
        {
            //Calls the helper method
            DisplayPlaylist(PlaylistList.SelectedIndex);
        }

        /*
         * This is passed an index number of the selected playlist. This method will clear all the songs in the SongList listbox
         * and then will iterate through the Song List stored in that playlist index and add them into the SongList listbox
         */
        public void DisplayPlaylist(int playlistIndex)
        {
            //Call helper method to clear the SongList listbox
            ClearSongList();
            //Add each of the Songs in the playlists Song List
            foreach (Song s in playlists[playlistIndex].SongPlaylist)
            {
                SongList.Items.Add(s);
            }
        }


        //public void UpdatePlaylistList()
        //{
        //    foreach (Playlist p in playlists)
        //    {
        //        PlaylistList.Items.Add((string)p.PlaylistName);
        //    }
        //}

        /*
         * This method will sort the songs in a certain playlist so that they are in alphabetical order.
         */
        public void SortAndDisplaySongList(int playlistIndex)
        {
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
                    SongList.Items.Add(song);
                }

            }
        }

        /*
         * This will handle clearing the SongList listbox
         */
        public void ClearSongList()
        {
            //Removes the method for the SelectionChange event so that the listbox can be cleared and not throw a selection change event
            SongList.SelectionChanged -= SongSelection_Click;
            //Clear the SongList listbox
            SongList.Items.Clear();
            //Adds the method back for the SelectionChange event
            SongList.SelectionChanged += SongSelection_Click;
        }

        /*
         * Handles the song selection click event and will start the song selected
         */
        private void SongSelection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Gets the song that was selected
                string songInformation = playlists[PlaylistList.SelectedIndex].SongPlaylist[SongList.SelectedIndex].ToString();

                //Sets the music controls to visible
                MusicControls.Visibility = Visibility.Visible;
                SongInformationDisplay.Text = songInformation;
                songQueue.Clear();

                //Adds the song that was selected and all the songs following it into a queue so that the next tracks can be implemented
                for(int i  = SongList.SelectedIndex; i <  SongList.Items.Count; i++)
                {
                    songQueue.Enqueue((Song)playlists[PlaylistList.SelectedIndex].SongPlaylist[i]);
                }

                // Check if music is already playing
                if (musicPlayingThread.ThreadState != ThreadState.Stopped)
                {
                    // If no song is currently playing, start a new thread
                    musicPlayingThread.Start();
                }
                else
                {
                    //Starts the music
                    PlayMusic();
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        /*
         * This method is the event method that is called when the user wantes to remove a song from a playlist
         */
        private void RemoveSongSelection_Click(object sender, RoutedEventArgs e)
        {
            //It will retrieve the song that selected to be deleted
            int index = SongList.SelectedIndex;
            String songName = SongList.SelectedValue.ToString();

            //Will remove the song from just the All Music playlist if this playlist is selected
            if (PlaylistList.SelectedIndex == -1 || PlaylistList.SelectedIndex == 0)
            {
                playlists[0].SongPlaylist.RemoveAt(index);
                //Removes SelectionChange event for deleting the song and adds the SongSelection_Click method back to the event so songs can be played again
                SongList.SelectionChanged -= RemoveSongSelection_Click;
                SongList.SelectionChanged += SongSelection_Click;
                //Hide the delete song lable
                DeleteSongLable.Visibility = Visibility.Hidden;
                //Sorts the songs again and redisplays them in the SongList listbox
                SortAndDisplaySongList(0);
            }
            //Will remove the song from just the All Music playlist and the selected playlist
            else
            {
                //It must be searched for in the All Music playlist because it will be a different index than the index it is in the selected playlist
                for(int i = 0; i < playlists[0].SongPlaylist.Count; i++)
                {
                    if(songName == playlists[0].SongPlaylist[i].ToString())
                    {
                        //Removes it when it finds the match
                        playlists[0].SongPlaylist.RemoveAt(i);
                        break;
                    }
                }
                playlists[PlaylistList.SelectedIndex].SongPlaylist.RemoveAt(index);
                //Removes SelectionChange event for deleting the song and adds the SongSelection_Click method back to the event so songs can be played again
                SongList.SelectionChanged -= RemoveSongSelection_Click;
                SongList.SelectionChanged += SongSelection_Click;
                //Hide the delete song lable
                DeleteSongLable.Visibility = Visibility.Hidden;
                //Sorts the songs again (in both playlists) and redisplays them in the SongList listbox
                SortAndDisplaySongList(PlaylistList.SelectedIndex);
                SortAndDisplaySongList(0);
                DisplayPlaylist(PlaylistList.SelectedIndex);
            }
        }

        /*
         * Method that will play the song that is next in the queue to the Music Playing thread 
         */
        private void PlayMusic()
        {
            Dispatcher.Invoke(() =>
            {
                string filePath = songQueue.First().FilePath;
                string verbatimString = filePath;
                mediaPlayer.Open(new Uri(verbatimString));
                //Display the mp3's albumn art
                ExtractAlbumArt(verbatimString);
                PlayPauseButton.Source = new BitmapImage(new Uri("/images/pause.png", UriKind.RelativeOrAbsolute));
                //Uses the mediaPlayer to play the song that was enqueued
                mediaPlayer.Play();
                //Starts the song duration bar
                songTimer.Enabled = true;
            });
        }

        /*
         * This method is used to diaply the albumn art for the mp3 that selected
         */
        private void ExtractAlbumArt(string filePath)
        {
            //Originally sets the image to null so that it clears the previous album art
            SongsImage.Source = null;
            try
            {
                //Using the TagLib to get the infromation out of the .mp3 file regarding the album art
                using (var file = TagLib.File.Create(filePath))
                {
                    var pictures = file.Tag.Pictures;
                    //Checking if there is an image stored in the mp3
                    if (pictures.Length >= 1)
                    {
                        var picture = pictures[0];
                        using (MemoryStream ms = new MemoryStream(picture.Data.Data))
                        {
                            //Retrieving the image
                            BitmapImage image = new BitmapImage();
                            image.BeginInit();
                            image.CacheOption = BitmapCacheOption.OnLoad;
                            image.StreamSource = ms;
                            image.EndInit();

                            //Disaplying the image
                            SongsImage.Source = image;
                        }
                    }
                    //If there is no image, display message letting the user know
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

        /*
         * This method updates the position of the scroll bar based on the elapsed time of the song
         */
        private void UpdateScrollBarPosition(object source, ElapsedEventArgs e)
        {
            try
            { 
                Dispatcher.Invoke(() =>
                {
                    // Get the current position and duration of the song
                    double currentPosition = mediaPlayer.Position.TotalSeconds;
                    double totalDuration = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;

                    // Calculate the percentage of the song that has been played
                    double percentagePlayed = (currentPosition / totalDuration) * 10;

                    // Update the position of the scroll bar
                    TimeBar.Value = percentagePlayed;
                });
            }
            catch(Exception ex)
            {
                // Handle any exceptions
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        /*
         * Event handle method that will update the volumn whenever the volume slider is moved
         */
        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Update the volume of the MediaPlayer based on the slider value
            if (mediaPlayer != null)
            {
                mediaPlayer.Volume = VolumeSlider.Value;
            }
        }

        /*
         * This method handles any event where the media keys are pressed. According to which button is pressed it will perform that 
         * action using the exisiting playback methods
         */
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

        /*
         * This event happens when the next button is clicked. This method will allow the next song in the queue to be played along with
         * adding the song that was playing before the event to a stack that is used for the previous song button.
         */
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
                // Handle any exceptions
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        /*
         * This event happens when the previous button is clicked. This method will allow the previous song in the stack to be added back to the queue.
         */
        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Check if there are more songs in the queue
                if (songReplayStack.Count > 0)
                {
                    //Pop the last song off the stack and then add it to the front of the queue
                    Queue<Song> tempQueue = new Queue<Song>();
                    tempQueue.Enqueue(songReplayStack.Pop());
                    foreach (Song s in songQueue)
                    {
                        tempQueue.Enqueue(s);
                    }
                    songQueue = tempQueue;

                    //If there are more songs, play the next one
                    SongInformationDisplay.Text = songQueue.First().Title + " - " + songQueue.First().Artist;
                    PlayMusic();
                }
                else
                {
                    //If the queue is empty, stop playback
                    MessageBox.Show("No previous song!");
                }
            }
            catch (Exception ex)
            {
                //Handle any exceptions
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        /*
         * This event happens when the Pause/Play button is clicked. It will know which action to perform depending on the boolean value that is 
         * stored as a global variable.
         */
        private void Play_Click(object sender, RoutedEventArgs e)
        {
            //Flips if the song is either playing or paused
            isPlaying = !isPlaying;
            try
            {
                if(isPlaying)
                {
                    //If the song was paused, play the song
                    PlayPauseButton.Source = new BitmapImage(new Uri("/images/pause.png", UriKind.RelativeOrAbsolute));
                    mediaPlayer.Play();
                }
                else
                {
                    //If the song was playing, pause the song
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
