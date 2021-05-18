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
      private set {
        if (isPlayerOwner!=value) {
          isPlayerOwner = value;
          BackgroundPath.Fill = value ? darkBackgroundBrush : lightBackgroundBrush;
          if (isPlayerOwner) {
            //just became owner of Player, this only happens with Play, Resume or Skip
            playerState = PlayerStateEnum.Playing;
            updatePauseButton();
            updateNextButton(Player.Current!);
          } else {
            if (playerState==PlayerStateEnum.Playing) {
              playerState = PlayerStateEnum.Paused;
            }
            updatePauseButton();
            //if (PauseButton.IsEnabled) {
            //  PauseButton.IsEnabled = false;
            //  PauseButton.InvalidateVisual();
            //}
          }
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
        Player.Current.TrackChanged += player_TrackChanged;
        Player.Current.PositionChanged += player_PositionChanged;
        Player.Current.VolumeChanged += player_VolumeChanged;
        Player.Current.IsMutedChanged += player_IsMutedChanged;
        //player_CanSkipTrackChanged(Player.Current);

        BackgroundPath.Fill = lightBackgroundBrush;
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
        var newPlayinglist = getPlayingList();
        if (playinglist!=newPlayinglist) {
          playinglist = newPlayinglist;
          Player.Current!.Play(this, playinglist, null, TimeSpan.FromMilliseconds(PositionScrollBar.Value), ShuffleButton.IsPressed, VolumeSlider.Value);
        } else if (!isPlayerOwner) {
          Player.Current!.Play(this, playinglist, track, TimeSpan.FromMilliseconds(PositionScrollBar.Value), ShuffleButton.IsPressed, VolumeSlider.Value);
        }
      }
      //if (isPlayerOwner) {
      //  // This PlayerControl is already controlling the player. Start playing a new Playinglist
      //  playNewPlaylist();
      //} else {
      //  //This PlayerControl is not yet controlling the player. 
      //  if (track is null) {
      //    //This PlayerControl has not played recently.  Start playing a new Playinglist
      //    playNewPlaylist();
      //  } else {
      //    //continue playing the current track
      //    Player.Current!.Play(this, playinglist, track, TimeSpan.FromMilliseconds(PositionScrollBar.Value), ShuffleButton.IsPressed, VolumeSlider.Value);
      //  }
      //}
    }

    private void executePlay() {
      throw new NotImplementedException();
    }

    Playinglist? playinglist;


    //private void playNewPlaylist() {
    //  if (getPlayingList is not null) {
    //    playinglist = getPlayingList();
    //    if (playinglist is not null) {
    //      Player.Current!.Play(this, playinglist, null, TimeSpan.FromMilliseconds(PositionScrollBar.Value), ShuffleButton.IsPressed, VolumeSlider.Value);
    //    }
    //  }
    //}


    private void pauseButton_Click(object sender, RoutedEventArgs e) {
      if (PauseButton.IsPressed) {
        Player.Current?.Pause();
      } else {
        Player.Current?.Resume(this, playinglist, track!, TimeSpan.FromMilliseconds(PositionScrollBar.Value), ShuffleButton.IsPressed, VolumeSlider.Value);
      }
    }


    private void nextButton_Click(object sender, RoutedEventArgs e) {
      if (!Player.Current!.CanSkipTrack) {
        //button should be disabled and not possible that code comes here
        System.Diagnostics.Debugger.Break();
        return;
      }

      Player.Current.PlayNextTrack(this, playinglist!, ShuffleButton.IsPressed, VolumeSlider.Value);
    }


    private void ShuffleButton_Click(object sender, RoutedEventArgs e) {
      if (isPlayerOwner) {
        Player.Current!.IsShuffle = ShuffleButton.IsPressed;
      }
    }


    //private void RepeatButton_Click(object sender, RoutedEventArgs e) {
    //}


    private void muteButton_Click(object sender, RoutedEventArgs e) {
      if (!isMutedFeedback) {
        Player.Current?.SetMute(MuteButton.IsPressed);
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
          if (isPlayerOwner && playerState==PlayerStateEnum.Playing) {
            Player.Current?.SetPosition(TimeSpan.FromMilliseconds(PositionScrollBar.Value));
          }
        }
      }
    }


    bool isThumbTragging;


    private void thumb_DragStarted(object sender, DragStartedEventArgs e) {
      isThumbTragging = true;
    }


    private void thumb_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e) {
      isThumbTragging = false;
      if (isPlayerOwner && playerState==PlayerStateEnum.Playing) {
        Player.Current?.SetPosition(TimeSpan.FromMilliseconds(PositionScrollBar.Value));
      }
    }


    bool isScrolling;


    private void scrollRepeatButtonLeft_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
      var mousePosition = e.GetPosition(scrollRepeatButtonLeft);
      if (mousePosition.X>=0 && mousePosition.X<=scrollRepeatButtonLeft.ActualWidth &&
        mousePosition.Y>=0 && mousePosition.Y<=scrollRepeatButtonLeft.ActualHeight) {
        isScrolling = true;
        PositionScrollBar.Value = PositionScrollBar.Value * mousePosition.X / scrollRepeatButtonLeft.ActualWidth;
        isScrolling = false;
        if (isPlayerOwner && playerState==PlayerStateEnum.Playing) {
          Player.Current?.SetPosition(TimeSpan.FromMilliseconds(PositionScrollBar.Value));
        }
      }
    }


    private void scrollRepeatButtonRight_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
      var mousePosition = e.GetPosition(scrollRepeatButtonRight);
      if (mousePosition.X>=0 && mousePosition.X<=scrollRepeatButtonRight.ActualWidth &&
        mousePosition.Y>=0 && mousePosition.Y<=scrollRepeatButtonRight.ActualHeight) {
        isScrolling = true;
        PositionScrollBar.Value += (PositionScrollBar.Maximum - PositionScrollBar.Value) * mousePosition.X / scrollRepeatButtonRight.ActualWidth;
        isScrolling = false;
        if (isPlayerOwner && playerState==PlayerStateEnum.Playing) {
          Player.Current?.SetPosition(TimeSpan.FromMilliseconds(PositionScrollBar.Value));
        }
      }
    }


    private void volumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
      if (!isVolumeFeedback) {
        if (isPlayerOwner) {
          Player.Current?.SetVolume(VolumeSlider.Value);
        }
      }
    }
    #endregion


    #region Player events
    //      -------------

    private void player_OwnerChanged(PlayerControl? playerControl) {
      IsPlayerOwner = playerControl==this;
    }


    PlayerStateEnum playerState;


    private void player_StateChanged(Player player) {
      if (!isPlayerOwner) return;

      playerState = player.State;
      updateInfoTextBox(player);
      //normally, the PlayButton is in the correct state, but once the player becomes idle (or error), it should switch from
      // displaying the stop symbol to displaying the play symbol

      updatePauseButton();
    }


    private void updatePauseButton() {
      bool isEnabled, isPressed;
      switch (playerState) {
      case PlayerStateEnum.Idle: isEnabled = false; isPressed = false; break;
      case PlayerStateEnum.Playing: isEnabled = true; isPressed = false; break;
      case PlayerStateEnum.Paused: isEnabled = true; isPressed = true; break;
      case PlayerStateEnum.Error: isEnabled = true; isPressed = false; break;
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
      if (!isPlayerOwner) return;

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


    Track? track;


    private void player_TrackChanged(Player player) {
      if (isPlayerOwner) {
        track = player.Track;
      }
    }


    string actualPositionText;
    bool isPositionChangedByPlayer;


    private void player_PositionChanged(Player player) {
      if (!isPlayerOwner) return;

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
      if (!isPlayerOwner) return;

      var volume = player.Volume;
      isVolumeFeedback = true;
      VolumeSlider.Value = volume;
      isVolumeFeedback = false;
    }


    bool isMutedFeedback;


    private void player_IsMutedChanged(Player player) {
      isMutedFeedback = true;
      MuteButton.IsPressed = player.IsMuted;
      isMutedFeedback = false;
    }



    private void player_CanSkipTrackChanged(Player player) {
      if (!isPlayerOwner) return;

      updateNextButton(player);
    }


    private void updateNextButton(Player player) {
      if (NextButton.IsEnabled!=player.CanSkipTrack) {
        NextButton.IsEnabled = player.CanSkipTrack;
        NextButton.InvalidateVisual();
      }
    }


    private void playerControl_Unloaded(object sender, RoutedEventArgs e) {
      if (Player.Current?.OwnerPlayerControl==this) {
        Player.Current!.Release(this);
      }
      Player.Current!.OwnerChanged -= player_OwnerChanged;
      Player.Current.CanSkipTrackChanged -= player_CanSkipTrackChanged;
      Player.Current.StateChanged -= player_StateChanged;
      Player.Current.ErrorMessageChanged -= player_ErrorMessageChanged;
      Player.Current.TrackChanged -= player_TrackChanged;
      Player.Current.PositionChanged -= player_PositionChanged;
      Player.Current.VolumeChanged -= player_VolumeChanged;
      Player.Current.IsMutedChanged -= player_IsMutedChanged;
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
      if (NextButton.IsEnabled) {
        NextButton.IsEnabled = false;
        NextButton.InvalidateVisual();
      }
      playinglist = null;
      Player.Current?.Play(this, null, track, TimeSpan.Zero, ShuffleButton.IsPressed, VolumeSlider.Value);
    }


    public void Play(Playinglist allTracksPlayinglist) {
      if (!NextButton.IsEnabled) {
        NextButton.IsEnabled = true;
        NextButton.InvalidateVisual();
      }
      playinglist = allTracksPlayinglist;
      Player.Current?.Play(this, playinglist, null, TimeSpan.Zero, ShuffleButton.IsPressed, VolumeSlider.Value);
    }



    public void PlayNextTrack() {
      Player.Current!.PlayNextTrack(this, playinglist!, ShuffleButton.IsPressed, VolumeSlider.Value);
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


    static Brush lightBackgroundBrush1 = Brushes.Orange;
    static Brush darkBackgroundBrush1 = Brushes.Yellow;

    static LinearGradientBrush darkBackgroundBrush = new LinearGradientBrush(
      new GradientStopCollection {
        new GradientStop(Color.FromRgb(0xDD, 0x00, 0x00), 0),
        new GradientStop(Color.FromRgb(0xDD, 0x88, 0x00), 0.2),
        new GradientStop(Color.FromRgb(0xDD, 0xAA, 0x00), 0.5),
        new GradientStop(Color.FromRgb(0xDD, 0x88, 0x00), 0.8),
        new GradientStop(Color.FromRgb(0xDD, 0x00, 0x00), 1) },
      new Point(0, 0), new Point(0, 1));

    static LinearGradientBrush lightBackgroundBrush = new LinearGradientBrush(
      new GradientStopCollection {
        //new GradientStop(Color.FromRgb(0xEE, 0x80, 0x80), 0),
        //new GradientStop(Color.FromRgb(0xEE, 0xA7, 0x80), 0.2),
        //new GradientStop(Color.FromRgb(0xEE, 0xD5, 0x80), 0.5),
        //new GradientStop(Color.FromRgb(0xEE, 0xA7, 0x80), 0.8),
        //new GradientStop(Color.FromRgb(0xEE, 0x80, 0x80), 1) },
        //new GradientStop(Color.FromRgb(0xE6, 0x78, 0x80), 0),
        //new GradientStop(Color.FromRgb(0xE6, 0xBF, 0x80), 0.2),
        //new GradientStop(Color.FromRgb(0xE6, 0xCD, 0x80), 0.5),
        //new GradientStop(Color.FromRgb(0xE6, 0xBF, 0x80), 0.8),
        //new GradientStop(Color.FromRgb(0xE6, 0x78, 0x80), 1) },
        new GradientStop(Color.FromRgb(0xBE, 0x50, 0x50), 0),
        new GradientStop(Color.FromRgb(0xBE, 0x77, 0x50), 0.2),
        new GradientStop(Color.FromRgb(0xBE, 0xA5, 0x50), 0.5),
        new GradientStop(Color.FromRgb(0xBE, 0x77, 0x50), 0.8),
        new GradientStop(Color.FromRgb(0xBE, 0x50, 0x50), 1) },
    new Point(0, 0), new Point(0, 1));
    #endregion
  }
}