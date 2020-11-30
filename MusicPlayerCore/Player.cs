//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Windows.Media;
//using System.Windows.Threading;

//namespace MusicPlayer {


//  public enum PlayerStateEnum {
//    Idle,
//    Starting,
//    Playing,
//    Paused
//  }
  

//  public class Player {


//    #region Properties
//    //      ----------

//    #pragma warning disable CA2211 // Non-constant fields should not be visible
//    public static Player? Current;
//#pragma warning restore CA2211 // Non-constant fields should not be visible


//    public Duration NaturalDuration => mediaPlayer.NaturalDuration;


//    public TimeSpan Position => mediaPlayer.Position;


//    public bool IsMuted { get; private set; }


//    public double Volume => mediaPlayer.Volume;


//    public PlayerControl? PlayerControl { get; private set; }


//    public PlayerStateEnum PlayerState { get; private set; }


//    bool hasStateChanged;


//    private void setState(PlayerStateEnum newState) {
//      if (PlayerState!=newState) {
//        PlayerState = newState;
//        trace($"State: {newState}");
//        hasStateChanged = true;
//      }
//    }


//    private void reportStateChange() {
//      if (hasStateChanged) {
//        hasStateChanged = false;
//        StateChanged?.Invoke(this);
//      }
//    }


//    private void trace(string traceString) {
//      var trace = DateTime.Now.ToString("mm:ss.fffff ") + traceString;
//      System.Diagnostics.Debug.WriteLine(trace);
//      Traced?.Invoke(trace);
//    }


//    public Track? Track { get; private set; }


//    public event Action<string>? Traced;
//    public event Action<Player>? StateChanged;
//    public event Action<Player>? PositionChanged;
//    public event Action<Player>? VolumeChanged;
//    #endregion


//    #region Constructor
//    //      -----------

//    readonly MediaPlayer mediaPlayer;
//    readonly DispatcherTimer dispatcherTimer;


//    public Player() {
//      if (Current is not null)  throw new Exception();

//      Current = this;
//      PlayerState = PlayerStateEnum.Idle;
//      mediaPlayer = new MediaPlayer();
//      mediaPlayer.BufferingStarted += mediaPlayer_BufferingStarted;
//      mediaPlayer.BufferingEnded += mediaPlayer_BufferingEnded;
//      mediaPlayer.MediaOpened += mediaPlayer_MediaOpened;
//      mediaPlayer.MediaFailed += mediaPlayer_MediaFailed;
//      mediaPlayer.MediaEnded += mediaPlayer_MediaEnded;
//      mediaPlayer.ScriptCommand += mediaPlayer_ScriptCommand;

//      dispatcherTimer = new DispatcherTimer();
//      dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
//      dispatcherTimer.Interval = TimeSpan.FromMilliseconds(100);
//    }
//    #endregion


//    #region Eventhandlers
//    //      -------------

//    private void mediaPlayer_BufferingStarted(object? sender, EventArgs e) {
//      System.Diagnostics.Debugger.Break();
//    }

//    private void mediaPlayer_BufferingEnded(object? sender, EventArgs e) {
//      System.Diagnostics.Debugger.Break();
//    }


//    private void mediaPlayer_MediaOpened(object? sender, EventArgs e) {
//      trace("MediaOpened");
//      PlayerControl?.SetSelectedTrack(Track!);

//      switch (PlayerState) {
//      case PlayerStateEnum.Idle:
//        System.Diagnostics.Debugger.Break(); return;
//      case PlayerStateEnum.Starting:
//        if (isPlayWaiting) {
//          startDelayedPlay();
//          return;
//        }

//        if (isStopWaiting) {
//          isStopWaiting = false;
//          setState(PlayerStateEnum.Idle);
//          trace("mediaPlayer.Stop");
//          mediaPlayer.Stop();
//          reportStateChange();
//          return;
//        }

//        dispatcherTimer.Start();
//        setState(PlayerStateEnum.Playing);
//        reportStateChange();
//        return;
//      case PlayerStateEnum.Playing:
//        System.Diagnostics.Debugger.Break(); return;
//      case PlayerStateEnum.Paused:
//        System.Diagnostics.Debugger.Break(); return;
//      default:
//        throw new NotSupportedException();
//      }
//      //should never come here
//      throw new NotSupportedException();
//    }


//    private void startDelayedPlay() {
//      isPlayWaiting = false;
//      isStopWaiting = false;
//      Track = playWaitingTrack;
//      setState(PlayerStateEnum.Starting);
//      trace("mediaPlayer.Open & Play");

//      mediaPlayer.Open(new Uri(playWaitingTrack!.FullFileName));
//      playWaitingTrack = null;
//      mediaPlayer.Play();
//      reportStateChange();
//    }


//    private void mediaPlayer_MediaFailed(object? sender, ExceptionEventArgs e) {
//      dispatcherTimer.Stop();
//      System.Diagnostics.Debugger.Break();
//    }

//    private void mediaPlayer_MediaEnded(object? sender, EventArgs e) {
//      trace("MediaEnded");
//      dispatcherTimer.Stop();
//      PositionChanged?.Invoke(this);

//      switch (PlayerState) {
//      case PlayerStateEnum.Idle:
//        System.Diagnostics.Debugger.Break(); return;
//      case PlayerStateEnum.Starting:
//        System.Diagnostics.Debugger.Break(); return;
//      case PlayerStateEnum.Playing:
//        Track? nextTrack = null;
//        if (PlayerControl?.GetNextTrack(out nextTrack)??false) {
//          trace($"GetNextTrack {nextTrack!.Title}");
//          Track = nextTrack;
//          setState(PlayerStateEnum.Starting);
//          trace("mediaPlayer.Open & Play");
//          mediaPlayer.Open(new Uri(nextTrack!.FullFileName));
//          mediaPlayer.Play();
//          reportStateChange();
//        } else {
//          Track = null;
//          setState(PlayerStateEnum.Idle);
//          reportStateChange();
//        }
//        return;
//      case PlayerStateEnum.Paused:
//        System.Diagnostics.Debugger.Break(); return;
//      default:
//        throw new NotSupportedException();
//      }
//      //should never come here
//      throw new NotSupportedException();
//    }


//    private void mediaPlayer_ScriptCommand(object? sender, MediaScriptCommandEventArgs e) {
//      System.Diagnostics.Debugger.Break();
//    }


//    private void dispatcherTimer_Tick(object? sender, EventArgs e) {
//      PositionChanged?.Invoke(this);
//    }
//    #endregion


//    #region Play/Stop Methods
//    //      -----------------

//    Track? playWaitingTrack;
//    bool isPlayWaiting;


//    public void Play(PlayerControl playerControl, Track track) {

//      trace($"Play {track.Title}");
//      PlayerControl = playerControl;

//      switch (PlayerState) {
//      case PlayerStateEnum.Idle:
//        break;
//      case PlayerStateEnum.Starting:
//        if (Track==track) return; //the track is already starting, so just continue
//        isPlayWaiting = true;
//        playWaitingTrack = track;
//        return;
//      case PlayerStateEnum.Playing:
//        if (Track==track) return; //the track is already playing, so just continue
//        break;
//      case PlayerStateEnum.Paused:
//        break;
//      default:
//        throw new NotSupportedException();
//      }

//      if (Track==track) {
//        //it would not raise MediaOpened, so just go to Playing
//        setState(PlayerStateEnum.Playing);
//        trace("mediaPlayer.Play");
//        mediaPlayer.Play();
//        reportStateChange();
//        return;
//      }

//      Track = track;
//      setState(PlayerStateEnum.Starting);
//      trace("mediaPlayer.Open & Play");
//      mediaPlayer.Open(new Uri(track.FullFileName));
//      mediaPlayer.Play();
//      reportStateChange();
//    }


//    public void Pause() {
//      trace("Pause");
//      if (PlayerState!=PlayerStateEnum.Playing) return;

//      setState(PlayerStateEnum.Paused);
//      trace("mediaPlayer.Pause");
//      mediaPlayer.Pause();
//      reportStateChange();
//    }


//    public void Resume() {
//      trace("Resume");
//      if (PlayerState!=PlayerStateEnum.Paused) return;

//      setState(PlayerStateEnum.Playing);
//      trace("mediaPlayer.Play");
//      mediaPlayer.Play();
//      reportStateChange();
//    }


//    public void SetPosition(TimeSpan positionTimeSpan) {
//      if (mediaPlayer.Position!=positionTimeSpan) {
//        mediaPlayer.Position = positionTimeSpan;
//        PositionChanged?.Invoke(this);
//      }
//    }


//    bool isStopWaiting;


//    public void Stop() {
//      trace("Stop");
//      playWaitingTrack = null;
//      switch (PlayerState) {
//      case PlayerStateEnum.Idle:
//        return;
//      case PlayerStateEnum.Starting:
//        isStopWaiting = true;
//        return;
//      case PlayerStateEnum.Playing:
//        break;
//      case PlayerStateEnum.Paused:
//        break;
//      default:
//        throw new NotSupportedException();
//      }

//      setState(PlayerStateEnum.Idle);
//      trace("mediaPlayer.Stop");
//      mediaPlayer.Stop();
//      reportStateChange();
//    }
//    #endregion


//    #region Volume Methods
//    //      --------------

//    public void SetVolume(double volume) {
//      var hasChanged = false;
//      if (IsMuted) {
//        IsMuted = false;
//        hasChanged = true;
//      }
//      if (mediaPlayer.Volume!=volume) {
//        mediaPlayer.Volume = volume;
//        hasChanged = true;
//      }
//      if (hasChanged) {
//        VolumeChanged?.Invoke(this);
//      }
//    }


//    double volumeBeforeMute;


//    public void Mute() {
//      if (!IsMuted) {
//        IsMuted = true;
//        volumeBeforeMute = mediaPlayer.Volume;
//        mediaPlayer.Volume = 0;
//        VolumeChanged?.Invoke(this);
//      }
//    }


//    public void UnMute() {
//      if (IsMuted) {
//        IsMuted = false;
//        mediaPlayer.Volume = volumeBeforeMute;
//        VolumeChanged?.Invoke(this);
//      }
//    }
//    #endregion
//  }
//}
