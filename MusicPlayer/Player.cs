using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace MusicPlayer {


  public enum PlayerStateEnum {
    Idle,
    Playing,
    Paused,
    Error
  }
  

  public class Player {


    #region Properties
    //      ----------

    #pragma warning disable CA2211 // Non-constant fields should not be visible
    public static Player? Current { get; private set;}
    #pragma warning restore CA2211


    public bool CanSkipTrack {
      get { return _canSkipTrack; }
      set {
        if (_canSkipTrack!=value) {
          _canSkipTrack = value;
          trace($"CanSkipTrack: {value}");
          CanSkipTrackChanged?.Invoke(this);
        }
      }
    }
    bool _canSkipTrack;
    public event Action<Player>? CanSkipTrackChanged;


    public Track? Track {
      get { return _track; }
      private set {
        if (_track!=value) {
          _track = value;
          trace($"Track: {value}");
          TrackChanged?.Invoke(this);
        }
        ownerPlayerControl?.SetSelectedTrack(Track);//MainWindow needs to know even if the same track gets played again
                                                    //so it can refresh the tracks displayed from the playlist. Most likely
                                                    //so many tracks got deleted and only 1 is left.
      }
    }
    Track? _track;
    public event Action<Player>? TrackChanged;


    public Duration NaturalDuration => mediaPlayer.NaturalDuration;


    public bool IsShuffle {
      get { return _isShuffle; }
      set {
        if (_isShuffle!=value) {
          _isShuffle = value;
          trace($"IsShuffle: {value}");
          IsShuffleChanged?.Invoke(this);
        }
      }
    }
    bool _isShuffle;
    public event Action<Player>? IsShuffleChanged;


    public TimeSpan Position => mediaPlayer.Position;
    public event Action<Player>? PositionChanged;


    public bool IsMuted {
      get { return _isMuted; }
      private set {
        if (_isMuted!=value) {
          _isMuted = value;
          trace($"IsMuted: {value}");
          IsMutedChanged?.Invoke(this);
        }
      }
    }
    bool _isMuted;
    public event Action<Player>? IsMutedChanged;


    public double Volume => mediaPlayer.Volume;
    public event Action<Player>? VolumeChanged;


    public PlayerControl? OwnerPlayerControl {
      get {
        return ownerPlayerControl;
      }
      private set {
        if (ownerPlayerControl!=value) {
          ownerPlayerControl = value;
          OwnerChanged?.Invoke(value);
        }
      }
    }
    PlayerControl? ownerPlayerControl;
    public event Action<PlayerControl?> OwnerChanged;



    public Playinglist? Playinglist { get; private set; }



    public PlayerStateEnum State {
      get { return _state; }
      private set {
        if (_state!=value) {
          _state = value;
          trace($"State: {value}");
          if (value!=PlayerStateEnum.Error) {
            ErrorMessage = "";
          }
          StateChanged?.Invoke(this);
        }
      }
    }
    PlayerStateEnum _state;
    public event Action<Player>? StateChanged;


    public string ErrorMessage {
      get { return _errorMessage; }
      private set {
        if (_errorMessage!=value) {
          _errorMessage = value;
          trace($"ErrorMessage: {value}");
          ErrorMessageChanged?.Invoke(this);
        }
      }
    }
    string _errorMessage;
    public event Action<Player>? ErrorMessageChanged;


    public bool IsTracePosition { set; private get; }


    private void trace(string traceString) {
      var trace = DateTime.Now.ToString("mm:ss.fffff ") + traceString;
      System.Diagnostics.Debug.WriteLine(trace);
      Traced?.Invoke(trace);
    }


    public event Action<string>? Traced;
    #endregion


    #region Constructor
    //      -----------

    readonly Random random;
    readonly MediaPlayer mediaPlayer;
    readonly DispatcherTimer dispatcherTimer;


    public Player() {
      if (Current is not null)  throw new Exception();

      Current = this;
      random = new Random();
      _errorMessage = "";
      mediaPlayer = new MediaPlayer();
      mediaPlayer.MediaOpened += mediaPlayer_MediaOpened;
      mediaPlayer.MediaFailed += mediaPlayer_MediaFailed;
      mediaPlayer.MediaEnded += mediaPlayer_MediaEnded;

      dispatcherTimer = new DispatcherTimer();
      dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
      dispatcherTimer.Interval = TimeSpan.FromMilliseconds(100);
    }
    #endregion


    #region Eventhandlers
    //      -------------

    private void mediaPlayer_MediaOpened(object? sender, EventArgs e) {
      trace("MediaOpened");
      State = PlayerStateEnum.Playing;
    }


    private void mediaPlayer_MediaFailed(object? sender, ExceptionEventArgs e) {
      dispatcherTimer.Stop();
      trace($"MediaFailed: {e.ErrorException.Message}");
      ErrorMessage = e.ErrorException.Message;
      State = PlayerStateEnum.Error;
    }


    private void mediaPlayer_MediaEnded(object? sender, EventArgs e) {
      trace("MediaEnded");
      dispatcherTimer.Stop();
      PositionChanged?.Invoke(this);

      playNextTrack();
    }


    private void playNextTrack() {
      Track = Playinglist?.GetNext(IsShuffle ? random : null)??null;
      if (Track is null) {
        trace($"playNextTrack() cannot find a track.");
        mediaPlayer.Pause();//prevents from immediate playing when user changes position
        State = PlayerStateEnum.Idle;
      } else {
        trace($"Play next {(IsShuffle ? "random " : "")}{Track.Title}");
        playtrack();
      }
    }


    private void playtrack() {
      mediaPlayer.Open(new Uri(Track!.FullFileName));
      mediaPlayer.Play();
      State = PlayerStateEnum.Playing;
      dispatcherTimer.Start();//
    }


    int lastPositionSeconds;


    private void dispatcherTimer_Tick(object? sender, EventArgs e) {
      if (lastPositionSeconds!=mediaPlayer.Position.Seconds) {
        lastPositionSeconds = mediaPlayer.Position.Seconds;
        if (IsTracePosition) {
          trace($"Tick {mediaPlayer.Position}, {mediaPlayer.Source}");
        }
        PositionChanged?.Invoke(this);
      }
    }
    #endregion


    #region Play/Pause Methods
    //      ------------------

    public void Play(PlayerControl playerControl, Track track) {
      OwnerPlayerControl = playerControl;
      Playinglist = null;
      CanSkipTrack = false;
      Track = track;
      trace($"Play single track {Track.Title}");
      playtrack();
    }


    public void Play(PlayerControl playerControl, Playinglist playinglist) {
      OwnerPlayerControl = playerControl;
      this.Playinglist = playinglist;
      CanSkipTrack = true;
      trace($"Play playlist {playinglist.Playlist?.Name}");
      Track = playinglist.GetNext(null);
      if (Track is null) {
        trace($"cannot find a track.");
        mediaPlayer.Pause();//prevents from immediate playing when user changes position
        State = PlayerStateEnum.Idle;
      } else {
        playtrack();
      }
    }


    public void PlayNextTrack() {
      playNextTrack();
    }


    public void Pause() {
      if (State!=PlayerStateEnum.Playing) return;

      trace("mediaPlayer.Pause");
      mediaPlayer.Pause();
      State = PlayerStateEnum.Paused;
    }


    public void Resume() {
      if (State!=PlayerStateEnum.Paused) return;

      trace("mediaPlayer.Resume");
      mediaPlayer.Play();
      State = PlayerStateEnum.Playing;
    }


    //static TimeSpan mediaOpenedTimeDelay = new TimeSpan(0, 0, 0, 0, 500);
    public void SetPosition(TimeSpan positionTimeSpan) {
      //if (DateTime.Now-mediaOpenedTime<mediaOpenedTimeDelay) {
      //  System.Diagnostics.Debugger.Break();
      //}
      if (mediaPlayer.Position!=positionTimeSpan) {
        mediaPlayer.Position = positionTimeSpan;
        PositionChanged?.Invoke(this);
      }
      //trace($"SetP {mediaPlayer.Position}, {mediaPlayer.Source}");
    }


    public void StopTrackIfPlaying(Track track) {
      trace($"Stop {track.FileName} if playing");
      if (Track is null) return;

      if (Track.FileName==track.FileName) {
        Track = null;
        OwnerPlayerControl = null;
        trace("mediaPlayer.Close()");
        mediaPlayer.Close();
        State = PlayerStateEnum.Idle;
      }
    }


    /// <summary>
    /// If playerControl is the current owner of the player, it gets removed as owner, any playing track gets stopped
    /// and player becomes idle.
    /// </summary>
    public void Release(PlayerControl playerControl) {
      if (ownerPlayerControl!=playerControl) return;

      OwnerPlayerControl = null;
      Playinglist = null;
      CanSkipTrack = false;
      trace("mediaPlayer.Release(playerControl)");
      mediaPlayer.Close();
      State = PlayerStateEnum.Idle;
    }
    #endregion


    #region Volume Methods
    //      --------------

    public void SetVolume(double volume) {
      var hasChanged = false;
      if (IsMuted) {
        IsMuted = false;
        hasChanged = true;
      }
      if (mediaPlayer.Volume!=volume) {
        mediaPlayer.Volume = volume;
        hasChanged = true;
      }
      if (hasChanged) {
        VolumeChanged?.Invoke(this);
      }
    }


    double volumeBeforeMute;


    public void Mute() {
      if (!IsMuted) {
        volumeBeforeMute = mediaPlayer.Volume;
        mediaPlayer.Volume = 0;
        IsMuted = true;
        VolumeChanged?.Invoke(this);
      }
    }


    public void UnMute() {
      if (IsMuted) {
        mediaPlayer.Volume = volumeBeforeMute;
        IsMuted = false;
        VolumeChanged?.Invoke(this);
      }
    }
    #endregion
  }
}
