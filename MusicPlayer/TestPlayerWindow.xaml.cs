using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Threading;


namespace MusicPlayer {


  /// <summary>
  /// Interaction logic for TestPlayerWindow.xaml
  /// </summary>
  public partial class TestPlayerWindow: Window {

    readonly DirectoryInfo projectDirectory;
    readonly Player player;

    //dummy variables needed for player API
    readonly PlayerControl playerControl;
    readonly Location location;

    readonly Track track1;
    readonly Track track2;
    readonly Track track3;
    readonly Track track4;
    readonly Track trackError;
    readonly List<Track> allTracks;
    readonly Playinglist allTracksPlayinglist;


    public TestPlayerWindow() {
      InitializeComponent();

      TraceTextBox.Text =
        "Copyrights: " + Environment.NewLine +
        "Music promoted by https://www.free-stock-music.com" + Environment.NewLine +
@"Track1: Stream Countdown(10s) by Alexander Nakarada | https://www.serpentsoundstudios.com
Attribution 4.0 International(CC BY 4.0)
https://creativecommons.org/licenses/by/4.0/" + Environment.NewLine + Environment.NewLine +

@"Track2: Humorous And Comic Intro by Free Music | https://soundcloud.com/fm_freemusic
Creative Commons Attribution 3.0 Unported License
https://creativecommons.org/licenses/by/3.0/deed.en_US" + Environment.NewLine + Environment.NewLine +

@"Track3: Music Logo For Storytelling by Free Music | https://soundcloud.com/fm_freemusic
Creative Commons Attribution 3.0 Unported License
https://creativecommons.org/licenses/by/3.0/deed.en_US" + Environment.NewLine + Environment.NewLine +

@"Track4: Happy Media Music Opener by Free Music | https://soundcloud.com/fm_freemusic
Creative Commons Attribution 3.0 Unported License
https://creativecommons.org/licenses/by/3.0/deed.en_US"+ Environment.NewLine + Environment.NewLine +

      "MediaPlayer test trace" + Environment.NewLine +
        "======================" + Environment.NewLine;
      _ = new DC(null);
      player = new Player();
      VolumeTextBox.Text = player.Volume.ToString();
      playerControl = new PlayerControl();
      projectDirectory = new DirectoryInfo(Environment.CurrentDirectory).Parent!.Parent!.Parent!.Parent!;
      location = new Location("locationPath", projectDirectory.FullName, isStoring: false);

      track1 = new Track(new FileInfo(projectDirectory.FullName + @"\track1.mp3"), location, isStoring: false);
      track2 = new Track(new FileInfo(projectDirectory.FullName + @"\track2.mp3"), location, isStoring: false);
      track3 = new Track(new FileInfo(projectDirectory.FullName + @"\track3.mp3"), location, isStoring: false);
      track4 = new Track(new FileInfo(projectDirectory.FullName + @"\track4.mp3"), location, isStoring: false);
      var errorFileName = projectDirectory.FullName + @"\trackError.mp3";
      File.Copy(track4.FullFileName, errorFileName, overwrite: true);
      trackError = new Track(new FileInfo(errorFileName), location, isStoring: false);
      trackError.Update(
        "TrackCausingError",
        trackError.Album,
        trackError.AlbumTrack,
        trackError.Artists,
        trackError.Composers,
        trackError.Publisher,
        trackError.Year,
        trackError.Genres,
        trackError.Weight,
        trackError.Volume,
        trackError.SkipStart,
        trackError.SkipEnd,
        trackError.TitleArtists);
      File.Delete(errorFileName);
      allTracks = new List<Track>{track1, track2, track3, track4 };
      allTracksPlayinglist = new Playinglist(allTracks);

      player.CanSkipTrackChanged += Player_CanSkipTrackChanged;
      player.TrackChanged += Player_TrackChanged;
      player.IsShuffleChanged += Player_IsShuffleChanged;
      player.PositionChanged += Player_PositionChanged;
      player.IsMutedChanged += Player_IsMutedChanged;
      player.VolumeChanged += Player_VolumeChanged;
      player.StateChanged += Player_StateChanged;
      player.ErrorMessageChanged += Player_ErrorMessageChanged;
      player.Traced += Player_Traced;

      Play1Button.Click += Play1Button_Click;
      Play2Button.Click += Play2Button_Click;
      PlayAllButton.Click += PlayAllButton_Click;
      PlayErrorButton.Click += PlayErrorButton_Click;
      ShuffleButton.Click += ShuffleButton_Click;
      PauseButton.Click += PauseButton_Click;
      ResumeButton.Click += ResumeButton_Click;
      SkipNearEndButton.Click += SkipNearEndButton_Click;
      SkipNextTrackButton.Click += SkipNextTrackButton_Click;
      VolumeTextBox.TextChanged += VolumeTextBox_TextChanged;
      IsMuteButton.Click += IsMuteButton_Click;
    }


    private void Player_CanSkipTrackChanged(Player player) {
      SkipNextTrackButton.IsEnabled = player.CanSkipTrack;
    }


    private void Player_TrackChanged(Player player) {
      trace($"<<<Track changed {player.Track}");
      TrackTextBox.Text = player.Track?.Title??"";
    }


    private void Player_IsShuffleChanged(Player player) {
      trace($"<<<IsShuffle: {player.IsShuffle}");
    }


    private void Player_PositionChanged(Player player) {
      if (!PositionToggleButton.IsChecked!.Value) {
        trace($"<<<Position: {player.Position}");
      }
      PositionTextBox.Text = player.Position.ToString();
    }


    private void Player_IsMutedChanged(Player player) {
      trace($"<<<IsMuted: {player.IsMuted}");
    }


    bool isVolumeChangedEvent;


    private void Player_VolumeChanged(Player player) {
      trace($"<<<Volume: {player.Volume}");
      isVolumeChangedEvent = true;
      VolumeTextBox.Text = player.Volume.ToString();
      isVolumeChangedEvent = false;
    }


    private void Player_StateChanged(Player player) {
      trace($"<<<State: {player.State}");
    }


    private void Player_ErrorMessageChanged(Player player) {
      trace($"<<<ErrorMessage: {player.ErrorMessage}");
      MessageTextBox.Text = player.ErrorMessage;
    }


    private void Player_Traced(string message) {
      trace($"PlayerTrace: {message}");
    }


    private void Play1Button_Click(object sender, RoutedEventArgs e) {
      trace($">>>Play {track1.Title}");
      player.Play(playerControl, track1);
    }


    private void Play2Button_Click(object sender, RoutedEventArgs e) {
      trace($">>>Play {track2.Title}");
      player.Play(playerControl, track2);
    }


    private void PlayAllButton_Click(object sender, RoutedEventArgs e) {
      trace($">>>Play all");
      player.Play(playerControl, allTracksPlayinglist);
    }


    private void PlayErrorButton_Click(object sender, RoutedEventArgs e) {
      trace($">>>Play {trackError.Title}");
      player.Play(playerControl, trackError);
    }


    private void ShuffleButton_Click(object sender, RoutedEventArgs e) {
      trace($">>>toggle shuffle.");
      player.IsShuffle = !player.IsShuffle;
    }


    private void PauseButton_Click(object sender, RoutedEventArgs e) {
      trace($">>>Pause");
      player.Pause();
    }


    private void ResumeButton_Click(object sender, RoutedEventArgs e) {
      trace($">>>Resume");
      player.Resume();
    }


    private void SkipNearEndButton_Click(object sender, RoutedEventArgs e) {
      if (player.NaturalDuration.HasTimeSpan) {
        trace($">>>Skip near end");
        var newPosition = player.NaturalDuration.TimeSpan.Subtract(TimeSpan.FromSeconds(5));
        player.SetPosition(newPosition);
      }
    }


    private void SkipNextTrackButton_Click(object sender, RoutedEventArgs e) {
      player.PlayNextTrack();
    }


    private void VolumeTextBox_TextChanged(object sender, TextChangedEventArgs e) {
      if (!isVolumeChangedEvent) {
        if (double.TryParse(VolumeTextBox.Text, out double volume)) {
          trace($">>>Volume {volume}");
          player.SetVolume(volume);
        }
      }
    }


    private void IsMuteButton_Click(object sender, RoutedEventArgs e) {
      if (IsMuteButton.IsChecked!.Value) {
        trace($">>>Mute");
        player.Mute();
        IsMuteButton.Content = "Unmute";
      } else {
        trace($">>>UnMute");
        player.UnMute();
        IsMuteButton.Content = "Mute";
      }
    }


    bool isTracing;


    private void trace(string text) {
      text = DateTime.Now.ToString("mm:ss ffff ") + text;
      if (isTracing) {
        TraceTextBox.Text += Environment.NewLine + text;
        TraceTextBox.ScrollToEnd();
      } else {
        isTracing = true;
        TraceTextBox.Text = text;
      }
    }
  }
}
