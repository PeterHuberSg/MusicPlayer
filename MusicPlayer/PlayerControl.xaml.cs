using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
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
      //RepeatButton.Click += RepeatButton_Click;
      RandomButton.Click += RandomButton_Click;
      MuteButton.Click += muteButton_Click;
      VolumeSlider.ValueChanged += volumeSlider_ValueChanged;
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
    RepeatButton scrollRepeatButtonLeft;
    RepeatButton scrollRepeatButtonRight;


    private void playerControl_Loaded(object sender, RoutedEventArgs e) {
      PositionScrollBar.ValueChanged += positionScrollBar_ValueChanged;
      PositionScrollBar.Track.Thumb.DragStarted += thumb_DragStarted;
      PositionScrollBar.Track.Thumb.DragCompleted += thumb_DragCompleted;
      PositionScrollBar.Track.Thumb.Background=PlayerButton.ButtonSymbolFillBrush;

      scrollRepeatButtonLeft = (RepeatButton)VisualTreeHelper.GetChild(PositionScrollBar.Track, 0);
      scrollRepeatButtonRight = (RepeatButton)VisualTreeHelper.GetChild(PositionScrollBar.Track, 1);
      scrollRepeatButtonLeft.PreviewMouseUp += scrollRepeatButtonLeft_PreviewMouseUp;
      scrollRepeatButtonRight.PreviewMouseUp += scrollRepeatButtonRight_PreviewMouseUp;
      var thumbRectangle = (Rectangle)VisualTreeHelper.GetChild(PositionScrollBar.Track.Thumb, 0);
      thumbRectangle.Fill = PlayerButton.ButtonSymbolFillBrush;
    }


    private void playButton_Click(object sender, RoutedEventArgs e) {
      if (getCurrentTrack is null) {
        System.Diagnostics.Debugger.Break();
        return;
      }
      if (PlayButton.IsPressed) {
        Player.Current?.Play(this, getCurrentTrack());
      } else {
        Player.Current?.Pause();
      }
    }
    //private void pauseToggleButton_Checked(object sender, RoutedEventArgs e) {
    //  if (isNoPauseButtonEvent) return;

    //  Player.Current?.Pause();
    //}


    //private void pauseToggleButton_Unchecked(object sender, RoutedEventArgs e) {
    //  if (isNoPauseButtonEvent) return;

    //  Player.Current?.Resume();
    //}


    //private void stopButton_Click(object sender, RoutedEventArgs e) {
    //  Player.Current?.Stop();
    //}
//////////////////////////////////////////



    private void nextButton_Click(object sender, RoutedEventArgs e) {
      if (getNextTrack is null) return;

      SelectedTrack = getNextTrack();
      Player.Current?.Play(this, SelectedTrack);
    }


    bool isNoPauseButtonEvent;



    private void RandomButton_Click(object sender, RoutedEventArgs e) {
    }


    //private void RepeatButton_Click(object sender, RoutedEventArgs e) {
    //}


    private void muteButton_Click(object sender, RoutedEventArgs e) {
      if (MuteButton.IsPressed) {
        Player.Current?.Mute();
      } else {
        Player.Current?.UnMute();
      }
    }


    private void positionScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
      //System.Diagnostics.Debug.WriteLine($"ScrollBarValueChanged: {PositionScrollBar.Value} {PositionScrollBar.Maximum} {TimeSpan.FromMilliseconds(PositionScrollBar.Value)} {Player.Current?.Position}");
      //if (isNewTrackStartDetected) {
      //  //System.Diagnostics.Debugger.Break();
      //  System.Diagnostics.Debug.WriteLine("ScrollBarValueChanged & isNewTrackStartDetected");
      //}

      if (isThumbTragging) {
        var position = TimeSpan.FromMilliseconds(PositionScrollBar.Value);
        actualPositionText = $" {(int)position.TotalMinutes}:{position.Seconds:00} of";
        updateInfoTextBox(Player.Current!);
      } else if (!isPositionChangedByPlayer && !isScrolling && !isNewTrackStartDetected) {
        //scrollLineButton of Scrollbar was pressed.
        if (PositionScrollBar.Value<PositionScrollBar.Maximum*.98) {
          Player.Current?.SetPosition(TimeSpan.FromMilliseconds(PositionScrollBar.Value));
        }
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


    bool isScrolling;


    private void scrollRepeatButtonLeft_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
      var mousePosition = e.GetPosition(scrollRepeatButtonLeft);
      if (mousePosition.X>=0 && mousePosition.X<=scrollRepeatButtonLeft.ActualWidth &&
        mousePosition.Y>=0 && mousePosition.Y<=scrollRepeatButtonLeft.ActualHeight) {
        isScrolling = true;
        PositionScrollBar.Value = PositionScrollBar.Value * mousePosition.X / scrollRepeatButtonLeft.ActualWidth;
        isScrolling = false;
        Player.Current?.SetPosition(TimeSpan.FromMilliseconds(PositionScrollBar.Value));
      }
    }


    private void scrollRepeatButtonRight_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
      var mousePosition = e.GetPosition(scrollRepeatButtonRight);
      if (mousePosition.X>=0 && mousePosition.X<=scrollRepeatButtonRight.ActualWidth &&
        mousePosition.Y>=0 && mousePosition.Y<=scrollRepeatButtonRight.ActualHeight) {
        isScrolling = true;
        PositionScrollBar.Value += (PositionScrollBar.Maximum - PositionScrollBar.Value) * mousePosition.X / scrollRepeatButtonRight.ActualWidth;
        isScrolling = false;
        Player.Current?.SetPosition(TimeSpan.FromMilliseconds(PositionScrollBar.Value));
      }
    }


    private void volumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
      if (!isVolumeFeedback) {
        Player.Current?.SetVolume(VolumeSlider.Value);
      }
    }


    private void player_StateChanged(Player player) {
      updateInfoTextBox(player);
      SelectedTrack = player.Track;

      if (PlayButton.IsPressed != (player.PlayerState==PlayerStateEnum.Playing || player.PlayerState==PlayerStateEnum.Starting)) {
        isNoPauseButtonEvent = true;
        PlayButton.IsPressed = !PlayButton.IsPressed;
        isNoPauseButtonEvent = false;
      }
      //isNoPauseButtonEvent = true;
      //if (player.PlayerState==PlayerStateEnum.Paused) {
        
      //  PauseToggleButton.Content = "_Resume";
      //  PauseToggleButton.IsChecked = true;
      //} else {
      //  PauseToggleButton.Content = "_Pause";
      //  PauseToggleButton.IsChecked = false;
      //}
      //isNoPauseButtonEvent = false;
      //PauseToggleButton.IsEnabled =
      //  player.PlayerState==PlayerStateEnum.Playing || player.PlayerState==PlayerStateEnum.Paused;

      //StopButton.IsEnabled = player.PlayerState!=PlayerStateEnum.Idle;

    }


    TimeSpan oldDuration = TimeSpan.MaxValue;
    string durationText;
    bool isNewTrackStartDetected;


    private void updateInfoTextBox(Player player) {
      if (player.Track is null) {
        InfoTextBox.Text = "";
      } else {
        var duration = player.NaturalDuration.HasTimeSpan ? player.NaturalDuration.TimeSpan : TimeSpan.FromTicks(0);
        if (oldDuration!=duration) {
          oldDuration = duration;
          durationText =duration.Ticks==0 ? "" : $" {(int)duration.TotalMinutes}:{duration.Seconds:00}";
          isNewTrackStartDetected = true;
          PositionScrollBar.Value = 0; //needed, otherwise changing PositionScrollBar.Maximum will PositionScrollBar have shortly wrong Value
          PositionScrollBar.Maximum = duration.TotalMilliseconds;
          //PositionScrollBar.LargeChange = duration.TotalMilliseconds/3;
          //PositionScrollBar.SmallChange = duration.TotalMilliseconds/20;
          isNewTrackStartDetected = false;
        }

        InfoTextBox.Text = $"{player.Track.Title}{actualPositionText}{durationText}";
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


    string actualPositionText;
    bool isPositionChangedByPlayer;


    private void player_PositionChanged(Player player) {
      if (!isThumbTragging) {
        var position = player.Position;
        actualPositionText = position.Ticks>0 ? $" {(int)position.TotalMinutes}:{position.Seconds:00} of" : "";
        updateInfoTextBox(player);
        isPositionChangedByPlayer = true;
        PositionScrollBar.Value = position.TotalMilliseconds;
        isPositionChangedByPlayer = false;
      }
    }


    bool isVolumeFeedback;


    private void player_VolumeChanged(Player obj) {
      var volume = Player.Current!.Volume;
      MuteButton.IsPressed = volume==0;
      //if (volume==0) {
      //  MuteToggleButton.Content = "Un_Mute";
      //  MuteToggleButton.IsChecked = true;
      //} else {
      //  MuteToggleButton.Content = "_Mute";
      //  MuteToggleButton.IsChecked = false;
      //}
      isVolumeFeedback = true;
      VolumeSlider.Value = volume;
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
      //PlayButton.IsEnabled = true;
      NextButton.IsEnabled = true;
      //MuteButton.IsEnabled = true;
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