using System;
using System.Collections.Generic;
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
  /// Provides a Window with the user interface to control a Player, i.e. playing music tracks like .mp3 files.
  /// </summary>
  public partial class PlayerControl: UserControl {


    #region Properties
    //      ----------

    public bool IsPlayerOwner {
      get {
        return isPlayerOwner;
      }
      set {
        if (isPlayerOwner!=value) {
          isPlayerOwner = value;
          BackgroundPath.Fill = value ? darkBackgroundBrush : lightBackgroundBrush;
        }
      }
    }
    bool isPlayerOwner;


    /// <summary>
    /// Is only raised if this PlayerControl is presently the Player.Owner.
    /// </summary>
    public event Action<Track?>? TrackChanged;
    #endregion


    #region Constructors
    //      ------------

    public PlayerControl() {
      InitializeComponent();

      PlayButton.Click += playButton_Click;
      PauseButton.Click += pauseButton_Click;
      NextButton.Click += nextButton_Click;
      //RepeatButton.Click += RepeatButton_Click;
      ShuffleButton.Click += ShuffleButton_Click;
      MuteButton.Click += muteButton_Click;
      VolumeSlider.ValueChanged += volumeSlider_ValueChanged;
      Loaded += playerControl_Loaded;
      Unloaded += playerControl_Unloaded;

      if (!DesignerProperties.GetIsInDesignMode(this)) {
        //in VS designer, Player.Current is null. Prevent the designer form executing the following lines.
        if (Player.Current is null) {
          //not already set in MainWindow or other Windoe
          _ = new Player();
        }
        Player.Current!.OwnerChanged += player_OwnerChanged;
        Player.Current!.CanSkipTrackChanged += player_CanSkipTrackChanged;
        Player.Current.StateChanged += player_StateChanged;
        Player.Current.ErrorMessageChanged += player_ErrorMessageChanged;
        Player.Current.PositionChanged += player_PositionChanged;
        Player.Current.VolumeChanged += player_VolumeChanged;
        player_CanSkipTrackChanged(Player.Current);
      }
    }
    #endregion


    #region EventHandlers
    //      -------------

    #region GUI events
    //      ----------

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
      if (getPlayingList is not null) {
        var playinglist = getPlayingList();
        if (playinglist is not null) {
          Player.Current!.Play(this, playinglist);
        }
      }
    }


    private void pauseButton_Click(object sender, RoutedEventArgs e) {
      if (PauseButton.IsPressed) {
        Player.Current?.Pause();
      } else {
        Player.Current?.Resume();
      }
    }


    private void nextButton_Click(object sender, RoutedEventArgs e) {
      if (!Player.Current!.CanSkipTrack) {
        //button should be disabled and not possible that code comes here
        System.Diagnostics.Debugger.Break();
        return;
      }

      Player.Current.PlayNextTrack();
    }


    private void ShuffleButton_Click(object sender, RoutedEventArgs e) {
      Player.Current!.IsShuffle = ShuffleButton.IsPressed;
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
    #endregion


    #region Player events
    //      -------------

    private void player_OwnerChanged(PlayerControl? playerControl) {
      IsPlayerOwner = playerControl==this;
    }


    private void player_StateChanged(Player player) {
      updateInfoTextBox(player);
      //normally, the PlayButton is in the correct state, but once the player becomes idle (or error), it should switch from
      // displaying the stop symbol to displaying the play symbol

      bool isEnabled, isPressed;
      switch (player.State) {
      case PlayerStateEnum.Idle:    isEnabled = false; isPressed = false; break;
      case PlayerStateEnum.Playing: isEnabled = true;  isPressed = false; break;
      case PlayerStateEnum.Paused:  isEnabled = true;  isPressed = true; break;
      case PlayerStateEnum.Error:   isEnabled = true;  isPressed = false; break;
      default:
        throw new NotSupportedException();
      }
      if (PauseButton.IsEnabled!=isEnabled || PauseButton.IsPressed!=isPressed) {
        PauseButton.IsEnabled = isEnabled;
        PauseButton.IsPressed = isPressed;
        PauseButton.InvalidateVisual();
      }
    }


    private void player_ErrorMessageChanged(Player player) {
      if (player.ErrorMessage.Length==0) {
        InfoTextBox.ToolTip = null;
      } else {
        var infoTextBoxToolTip = new ToolTip { Content = player.ErrorMessage };
        InfoTextBox.ToolTip = infoTextBoxToolTip;
        infoTextBoxToolTip.Placement = PlacementMode.Relative;
        infoTextBoxToolTip.PlacementTarget = InfoTextBox;
        infoTextBoxToolTip.IsOpen = true;
      }
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


    private void player_VolumeChanged(Player player) {
      var volume = player.Volume;
      MuteButton.IsPressed = volume==0;
      isVolumeFeedback = true;
      VolumeSlider.Value = volume;
      isVolumeFeedback = false;
    }


    private void player_CanSkipTrackChanged(Player player) {
      NextButton.IsEnabled = player.CanSkipTrack;
      NextButton.InvalidateVisual();
    }


    private void playerControl_Unloaded(object sender, RoutedEventArgs e) {
      if (Player.Current?.OwnerPlayerControl==this) {
        Player.Current!.Release(this);
      }
      Player.Current!.StateChanged -= player_StateChanged;
      Player.Current.PositionChanged -= player_PositionChanged;
      Player.Current.VolumeChanged -= player_VolumeChanged;
      Player.Current.CanSkipTrackChanged -= player_CanSkipTrackChanged;
    }
    #endregion
    #endregion


    #region Methods
    //      -------

    Func<Playinglist?>? getPlayingList;


    //internal void Init(Func<Track> getCurrentTrack, Func<Track> getNextTrack) {
    internal void Init(Func<Playinglist?>? getPlayingList) {
      this.getPlayingList = getPlayingList;
      //PlayButton.IsEnabled = getPlayingList is not null;
      //this.getNextTrack = getNextTrack;
      //PlayButton.IsEnabled = true;
      //NextButton.IsEnabled = true;
      //MuteButton.IsEnabled = true;
    }


    public void Play(Track track) {
      NextButton.IsEnabled = false;
      Player.Current?.Play(this, track);
    }


    public void Play(Playinglist allTracksPlayinglist) {
      NextButton.IsEnabled = true;
      Player.Current?.Play(this, allTracksPlayinglist);
    }


    //public bool GetNextTrack([NotNullWhen(true)] out Track? nextTrack) {
    //  if (getNextTrack is null) {
    //    System.Diagnostics.Debugger.Break();
    //    nextTrack = null;
    //    return false;
    //  }
    //  nextTrack = getNextTrack();
    //  return true;
    //}


    public void SetSelectedTrack(Track? track) {
      TrackChanged?.Invoke(track);
    }


    public void StopTrackIfPlaying(Track track) {
      Player.Current?.StopTrackIfPlaying(track);
    }


    static Brush lightBackgroundBrush = Brushes.Orange;
    static Brush darkBackgroundBrush = Brushes.Red;
    #endregion
  }
}