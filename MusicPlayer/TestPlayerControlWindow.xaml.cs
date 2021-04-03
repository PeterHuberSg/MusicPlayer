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
  /// Interaction logic for TestPlayerControlWindow.xaml
  /// </summary>
  public partial class TestPlayerControlWindow: Window {

    readonly DirectoryInfo projectDirectory;

    //dummy variables needed for player API
    readonly Location location;

    readonly Track track1;
    readonly Track track2;
    readonly Track track3;
    readonly Track track4;
    readonly Track trackError;
    readonly List<Track> allTracks;
    readonly Playinglist allTracksPlayinglist;


    public TestPlayerControlWindow() {
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

      Player.Current!.Traced += Player_Traced;
      TrackPlayer.Init(getPlayinglist);


      Play1Button.Click += Play1Button_Click;
      Play2Button.Click += Play2Button_Click;
      PlayAllButton.Click += PlayAllButton_Click;
      PlayErrorButton.Click += PlayErrorButton_Click;
      IsTracePositionButton.Click += IsTracePositionButton_Click;
    }


    private Playinglist getPlayinglist() {
      return new Playinglist(allTracks.OrderByDescending(t=>t.Title));
    }


    //private void Player_StateChanged(Player player) {
    //  trace($"<<<State: {player.State}");
    //}


    private void Player_Traced(string message) {
      trace($"PlayerTrace: {message}");
    }


    private void Play1Button_Click(object sender, RoutedEventArgs e) {
      trace($">>>Play {track1.Title}");
      TrackPlayer.Play(track1);
    }


    private void Play2Button_Click(object sender, RoutedEventArgs e) {
      trace($">>>Play {track2.Title}");
      TrackPlayer.Play(track2);
    }


    private void PlayAllButton_Click(object sender, RoutedEventArgs e) {
      trace($">>>Play all");
      TrackPlayer.Play(allTracksPlayinglist);
    }


    private void PlayErrorButton_Click(object sender, RoutedEventArgs e) {
      trace($">>>Play {trackError.Title}");
      TrackPlayer.Play(trackError);
    }


    private void IsTracePositionButton_Click(object sender, RoutedEventArgs e) {
      Player.Current!.IsTracePosition = IsTracePositionButton.IsChecked!.Value;
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
