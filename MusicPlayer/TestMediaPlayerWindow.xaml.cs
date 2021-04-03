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
  /// Interaction logic for TestMediaPlayerWindow.xaml
  /// </summary>
  public partial class TestMediaPlayerWindow: Window {

    readonly DirectoryInfo projectDirectory;
    readonly MediaPlayer mediaPlayer;
    readonly DispatcherTimer dispatcherTimer;


    public TestMediaPlayerWindow() {
      InitializeComponent();

      projectDirectory = new DirectoryInfo(Environment.CurrentDirectory).Parent!.Parent!.Parent!.Parent!;
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

      dispatcherTimer = new DispatcherTimer();
      dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
      dispatcherTimer.Interval = TimeSpan.FromSeconds(10);

      mediaPlayer = new MediaPlayer();
      mediaPlayer.BufferingStarted += mediaPlayer_BufferingStarted;
      mediaPlayer.BufferingEnded += mediaPlayer_BufferingEnded;
      mediaPlayer.MediaOpened += mediaPlayer_MediaOpened;
      mediaPlayer.MediaFailed += mediaPlayer_MediaFailed;
      mediaPlayer.MediaEnded += mediaPlayer_MediaEnded;
      mediaPlayer.ScriptCommand += mediaPlayer_ScriptCommand;

      Open1Button.Click += Open1Button_Click;
      Open2Button.Click += Open2Button_Click;
      Open3Button.Click += Open3Button_Click;
      Open4Button.Click += Open4Button_Click;
      PlayButton.Click += PlayButton_Click;
      Open1PlayButton.Click += Open1PlayButton_Click;
      PauseButton.Click += PauseButton_Click;
      SkipNearEndButton.Click += SkipNearEndButton_Click;
      StopButton.Click += StopButton_Click;
      CloseButton.Click += CloseButton_Click;
      StateButton.Click += StateButton_Click;
    }


    private void Open1Button_Click(object sender, RoutedEventArgs e) {
      var track = projectDirectory.FullName + @"\track1.mp3";
      trace("Open " + track);
      mediaPlayer.Open(new Uri(track));
      traceState();
    }


    private void Open2Button_Click(object sender, RoutedEventArgs e) {
      var track = projectDirectory.FullName + @"\track2.mp3";
      trace("Open " + track);
      mediaPlayer.Open(new Uri(track));
      traceState();
    }


    private void Open3Button_Click(object sender, RoutedEventArgs e) {
      var track = projectDirectory.FullName + @"\track3.mp3";
      trace("Open " + track);
      mediaPlayer.Open(new Uri(track));
      traceState();
    }


    private void Open4Button_Click(object sender, RoutedEventArgs e) {
      var track = projectDirectory.FullName + @"\track4.mp3";
      trace("Open " + track);
      mediaPlayer.Open(new Uri(track));
      traceState();
    }


    private void PlayButton_Click(object sender, RoutedEventArgs e) {
      trace("Play");
      dispatcherTimer.Start();
      mediaPlayer.Play();
    }


    private void Open1PlayButton_Click(object sender, RoutedEventArgs e) {
      var track = projectDirectory.FullName + @"\track1.mp3";
      trace("Open & play " + track);
      mediaPlayer.Open(new Uri(track));
      mediaPlayer.Play();
      traceState();
      dispatcherTimer.Start();
    }


    private void PauseButton_Click(object sender, RoutedEventArgs e) {
      trace("Pause");
      dispatcherTimer.Stop();
      mediaPlayer.Pause();
      traceState();
    }


    private void SkipNearEndButton_Click(object sender, RoutedEventArgs e) {
      if (mediaPlayer.NaturalDuration.HasTimeSpan) {
        var newPosition = mediaPlayer.NaturalDuration.TimeSpan.Subtract(TimeSpan.FromSeconds(5));
        trace("Skip near end");
        mediaPlayer.Position = newPosition;
        traceState();
      } else {
        trace("Skip near end, NaturalDuration.HasTimeSpan: false");
      }
    }
    
    
    private void StopButton_Click(object sender, RoutedEventArgs e) {
      trace("Stop");
      dispatcherTimer.Stop();
      mediaPlayer.Stop();
      traceState();
    }


    private void CloseButton_Click(object sender, RoutedEventArgs e) {
      trace("Close");
      dispatcherTimer.Stop();
      mediaPlayer.Close();
      traceState();
    }


    private void StateButton_Click(object sender, RoutedEventArgs e) {
      traceState();
    }


    private void mediaPlayer_BufferingStarted(object? sender, EventArgs e) {
      trace("Player_BufferingStarted");
    }


    private void mediaPlayer_BufferingEnded(object? sender, EventArgs e) {
      trace("Player_BufferingStarted");
    }


    private void mediaPlayer_MediaOpened(object? sender, EventArgs e) {
      trace("Player_MediaOpened");
      traceState();
    }


    private void mediaPlayer_MediaFailed(object? sender, ExceptionEventArgs e) {
      trace("Player_MediaFailed");
    }


    private void mediaPlayer_MediaEnded(object? sender, EventArgs e) {
      trace("Player_MediaEnded");
      dispatcherTimer.Stop();
      traceState();
    }


    private void mediaPlayer_ScriptCommand(object? sender, MediaScriptCommandEventArgs e) {
      trace("Player_ScriptCommand");
    }


    private void dispatcherTimer_Tick(object? sender, EventArgs e) {
      trace($"Tick {mediaPlayer.Position}, CanPause: {mediaPlayer.CanPause}, HasAudio: {mediaPlayer.HasAudio}");
    }


    private void traceState() {
      trace($"state CanPause: {mediaPlayer.CanPause}, HasAudio: {mediaPlayer.HasAudio}" + 
        $", NaturalDuration: {mediaPlayer.NaturalDuration}, Position: {mediaPlayer.Position}" +
        $", SpeedRatio: {mediaPlayer.SpeedRatio}, Source: {mediaPlayer.Source}");
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
