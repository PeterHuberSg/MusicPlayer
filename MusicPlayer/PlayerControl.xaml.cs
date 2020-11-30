using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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


namespace MusicPlayer {


  /// <summary>
  /// Interaction logic for PlayerControl.xaml
  /// </summary>
  public partial class PlayerControl: UserControl {

    #region Properties
    //      ----------

    public Track? SelectedTrack { get; private set; }
    //public int? SelectedTrackId { get; private set; }
    //public IList<Track>? Tracks { get; private set; }


    public event Action<Track>? TrackChanged;
    #endregion


    #region Constructors
    //      ------------

    public PlayerControl() {
      InitializeComponent();

      PlayButton.Click += playButton_Click;
      NextButton.Click += nextButton_Click;
      PauseToggleButton.Checked += pauseToggleButton_Checked;
      PauseToggleButton.Unchecked += pauseToggleButton_Unchecked;
      StopButton.Click += stopButton_Click;
      MuteToggleButton.Checked += muteToggleButton_Checked;
      MuteToggleButton.Unchecked += muteToggleButton_Unchecked;
      VolumeScrollBar.ValueChanged += volumeScrollBar_ValueChanged;
      Loaded += playerControl_Loaded;
      Unloaded += playerControl_Unloaded;

      if (!DesignerProperties.GetIsInDesignMode(this)) {
        //in VS designer, Player.Current is null. Prevent the designer form executing the following lines.
        Player.Current!.StateChanged += player_StateChanged;
        Player.Current.PositionChanged += player_PositionChanged;
        Player.Current.VolumeChanged += player_VolumeChanged;
      }
    }
    #endregion


    #region EventHandlers
    //      -------------
    RepeatButton repeatButtonLeft;
    RepeatButton repeatButtonRight;


    private void playerControl_Loaded(object sender, RoutedEventArgs e) {
      PositionScrollBar.ValueChanged += positionScrollBar_ValueChanged;
      PositionScrollBar.Track.Thumb.DragStarted += thumb_DragStarted;
      PositionScrollBar.Track.Thumb.DragCompleted += thumb_DragCompleted;
      PositionScrollBar.Track.Thumb.Background=Brushes.Black;
      repeatButtonLeft = (RepeatButton)VisualTreeHelper.GetChild(PositionScrollBar.Track, 0);
      repeatButtonRight = (RepeatButton)VisualTreeHelper.GetChild(PositionScrollBar.Track, 1);
      repeatButtonLeft.PreviewMouseUp += repeatButtonLeft_PreviewMouseUp;
      repeatButtonRight.PreviewMouseUp += repeatButtonRight_PreviewMouseUp;
      var thumbRectangle = (Rectangle)VisualTreeHelper.GetChild(PositionScrollBar.Track.Thumb, 0);
      thumbRectangle.Fill = Brushes.DarkViolet;
    }


    private void playButton_Click(object sender, RoutedEventArgs e) {
      if (getCurrentTrack is null) {
        System.Diagnostics.Debugger.Break();
        return;
      }
      Player.Current?.Play(this, getCurrentTrack());
    }


    private void nextButton_Click(object sender, RoutedEventArgs e) {
      if (getNextTrack is null) return;

      SelectedTrack = getNextTrack();
      Player.Current?.Play(this, SelectedTrack);
    }


    bool isNoPauseButtonEvent;


    private void pauseToggleButton_Checked(object sender, RoutedEventArgs e) {
      if (isNoPauseButtonEvent) return;

      Player.Current?.Pause();
    }


    private void pauseToggleButton_Unchecked(object sender, RoutedEventArgs e) {
      if (isNoPauseButtonEvent) return;

      Player.Current?.Resume();
    }


    private void stopButton_Click(object sender, RoutedEventArgs e) {
      Player.Current?.Stop();
    }


    private void muteToggleButton_Checked(object sender, RoutedEventArgs e) {
      Player.Current?.Mute();
    }


    private void muteToggleButton_Unchecked(object sender, RoutedEventArgs e) {
      Player.Current?.UnMute();
    }


    private void positionScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
      if (isThumbTragging) {
        var position = TimeSpan.FromMilliseconds(PositionScrollBar.Value);
        PositionTextBox.Text = $"{(int)position.TotalMinutes}:{position.Seconds:00}";
      }
    }


    bool isThumbTragging;


    private void thumb_DragStarted(object sender, DragStartedEventArgs e) {
      isThumbTragging = true;
    }


    private void thumb_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e) {
      isThumbTragging = false;
      Player.Current?.SetPosition(TimeSpan.FromMilliseconds(PositionScrollBar.Value));
    }


    private void repeatButtonLeft_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
      var mousePosition = e.GetPosition(repeatButtonLeft);
      if (mousePosition.X>=0 && mousePosition.X<=repeatButtonLeft.ActualWidth &&
        mousePosition.Y>=0 && mousePosition.Y<=repeatButtonLeft.ActualHeight) 
      {
        PositionScrollBar.Value = PositionScrollBar.Value * mousePosition.X / repeatButtonLeft.ActualWidth;
        Player.Current?.SetPosition(TimeSpan.FromMilliseconds(PositionScrollBar.Value));
      }
    }


    private void repeatButtonRight_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
      var mousePosition = e.GetPosition(repeatButtonRight);
      if (mousePosition.X>=0 && mousePosition.X<=repeatButtonRight.ActualWidth &&
        mousePosition.Y>=0 && mousePosition.Y<=repeatButtonRight.ActualHeight) 
      {
        PositionScrollBar.Value += (PositionScrollBar.Maximum - PositionScrollBar.Value) * mousePosition.X / repeatButtonRight.ActualWidth;
        Player.Current?.SetPosition(TimeSpan.FromMilliseconds(PositionScrollBar.Value));
      }
    }


    private void volumeScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
      if (!isVolumeFeedback) {
        Player.Current?.SetVolume(VolumeScrollBar.Value);
      }
    }


    TimeSpan oldDuration = TimeSpan.MaxValue;


    private void player_StateChanged(Player obj) {
      Player player = Player.Current!;

      TrackTitleTextBox.Text = player.Track?.Title;
      SelectedTrack = player.Track;

      isNoPauseButtonEvent = true;
      if (player.PlayerState==PlayerStateEnum.Paused) {
        PauseToggleButton.Content = "_Resume";
        PauseToggleButton.IsChecked = true;
      } else {
        PauseToggleButton.Content = "_Pause";
        PauseToggleButton.IsChecked = false;
      }
      isNoPauseButtonEvent = false;
      PauseToggleButton.IsEnabled = 
        player.PlayerState==PlayerStateEnum.Playing || player.PlayerState==PlayerStateEnum.Paused;

      StopButton.IsEnabled = player.PlayerState!=PlayerStateEnum.Idle;

      var duration = player.NaturalDuration.HasTimeSpan ? player.NaturalDuration.TimeSpan : TimeSpan.FromTicks(0);
      if (oldDuration!=duration) {
        oldDuration = duration;
        if (duration.Ticks==0) {
          DurationTextBox.Text = "";
        } else {
          DurationTextBox.Text = $"{(int)duration.TotalMinutes}:{duration.Seconds:00}";
        }
        PositionScrollBar.Maximum = duration.TotalMilliseconds;
      }
    }


    //private void setSelectedIndex(Track? track) {
    //  if (Tracks is null) return;

    //  if (track is null) {
    //    //SelectedTrackId = null;
    //    //SelectedTrack = null;
    //    return;
    //  }

    //  for (int tracksIndex = 0; tracksIndex < Tracks.Count; tracksIndex++) {
    //    var indexedTrack = Tracks[tracksIndex];
    //    if (indexedTrack==track) {
    //      SelectedTrackId = tracksIndex;
    //      SelectedTrack = track;
    //    }
    //  }
    //}


    private void player_PositionChanged(Player obj) {
      if (!isThumbTragging) {
        var position = Player.Current!.Position;
        if (position.Ticks>0) {
          PositionTextBox.Text = $"{(int)position.TotalMinutes}:{position.Seconds:00}";
        } else {
          PositionTextBox.Text = "";
        }
        PositionScrollBar.Value = position.TotalMilliseconds;
      }
    }


    bool isVolumeFeedback;


    private void player_VolumeChanged(Player obj) {
      var volume = Player.Current!.Volume;
      if (volume==0) {
        MuteToggleButton.Content = "Un_Mute";
        MuteToggleButton.IsChecked = true;
      } else {
        MuteToggleButton.Content = "_Mute";
        MuteToggleButton.IsChecked = false;
      }
      isVolumeFeedback = true;
      VolumeScrollBar.Value = volume;
      isVolumeFeedback = false;
    }


    private void playerControl_Unloaded(object sender, RoutedEventArgs e) {
      if (Player.Current?.PlayerControl==this) {
        Player.Current.Stop();
      }
      Player.Current!.StateChanged -= player_StateChanged;
      Player.Current.PositionChanged -= player_PositionChanged;
      Player.Current.VolumeChanged -= player_VolumeChanged;
    }
    #endregion


    #region Methods
    //      -------

    Func<Track>? getCurrentTrack;
    Func<Track>? getNextTrack;


    internal void Init(Func<Track> getCurrentTrack, Func<Track> getNextTrack) {
      this.getCurrentTrack = getCurrentTrack;
      this.getNextTrack = getNextTrack;
      //Tracks = tracks;
      //SelectedTrackId = selectedTrackId;
      //SelectedTrack = tracks[selectedTrackId];
      PlayButton.IsEnabled = true;
      NextButton.IsEnabled = true;
      MuteToggleButton.IsEnabled = true;
    }


    public void Play(Track track) {
      SelectedTrack = track;
      Player.Current?.Play(this, track);
    }


    public bool GetNextTrack([NotNullWhen(true)] out Track? nextTrack) {
      if (getNextTrack is null) {
        System.Diagnostics.Debugger.Break();
        nextTrack = null;
        return false;
      }
      nextTrack = getNextTrack();
      return true;
    }


    public void SetSelectedTrack(Track track) {
      TrackChanged?.Invoke(track);
    }


    internal void CloseIfSelected(Track track) {
      Player.Current?.CloseIfSelected(track);
    }
    #endregion
  }
}
