using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MusicPlayer {


  //public class TrackGridRowShared {
  //  public Action? UpdateStats { get; }

  //  int nextNo;

  //  public TrackGridRowShared(Action? updateStats) {
  //    UpdateStats = updateStats;
  //  }

  //  public int GetNextNo() {
  //    return nextNo++;
  //  }
  //}


  public class TrackGridRow: INotifyPropertyChanged {
    public int No { get; }
    public Track Track { get; }
    public string? Playlists { get; set; }
 
    /// <summary>
    /// Track is marked for deletion
    /// </summary>
    public bool IsDeletion {
      get {
        return isDeletion;
      }
      set {
        if (isDeletion!=value) {
          isDeletion = value;
          updatePlaylistCheckBox();
          dataChanged?.Invoke();
        }
      }
    }
    bool isDeletion;


    /// <summary>
    /// User selected this track for adding to playlist OR CheckBox is disabled and displays that the track is already in the playlist
    /// </summary>
    public bool IsAddPlaylist {
      get {
        return isAddPlaylist;
      }
      set {
        if (isAddPlaylist!=value) {
          isAddPlaylist = value;
          dataChanged?.Invoke();
          PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsAddPlaylist)));
        }
      }
    }
    bool isAddPlaylist;


    /// <summary>
    /// Playlist CheckBox is visible if a playlist name is entered and track is not marked for deletion
    /// </summary>
    public Visibility PlaylistCheckBoxVisibility {
      get {
        return playlistCheckBoxVisibility;
      }
      set {
        if (playlistCheckBoxVisibility!=value) {
          playlistCheckBoxVisibility = value;
          PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlaylistCheckBoxVisibility)));
        }
      }
    }
    Visibility playlistCheckBoxVisibility;


    /// <summary>
    /// Playlist CheckBox is disabled when the track is already in the playlist
    /// </summary>
    public bool PlaylistCheckBoxIsEnabled {
      get {
        return playlistCheckBoxIsEnabled;
      }
      set {
        if (playlistCheckBoxIsEnabled!=value) {
          playlistCheckBoxIsEnabled = value;
          PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlaylistCheckBoxIsEnabled)));
        }
      }
    }
    bool playlistCheckBoxIsEnabled;


    //INotifyPropertyChanged interface
    public event PropertyChangedEventHandler? PropertyChanged;


    internal void RaisePropertyChanged(string propertyName) {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }


    //#pragma warning disable CA2211 // Non-constant fields should not be visible
    //    public static Action? HasIsDeletionChanged;
    //    public static Action? HasIsAddPlaylistChanged;
    //#pragma warning restore CA2211

    /// <summary>
    /// Raised when marked 'for deletion' or 'adding to playlist' is changed
    /// </summary>
    readonly Action? dataChanged;


    public TrackGridRow(ref int trackNo, Track track, Action? dataChanged) {
      this.dataChanged = dataChanged;
      No = trackNo++;
      Track = track;
      UpdatePlaylists();
    }


    public void UpdatePlaylists() {
      Playlists = null;
      var isFirst = true;
      foreach (var playlistTrack in Track.Playlists) {
        if (isFirst) {
          isFirst = false;
        } else {
          Playlists += '|';
        }
        Playlists += playlistTrack.Playlist.Name;
      }
    }


    bool hasPlaylistName;//user selected an existing or entered a new playlist name


    public void UpdatePlaylistCheckBox(Playlist? playlist, bool hasPlaylistName) {
      this.hasPlaylistName = hasPlaylistName;
      if (hasPlaylistName) {
        PlaylistCheckBoxIsEnabled = !Track.Playlists.Where(plt => plt.Playlist==playlist).Any();
      }
      updatePlaylistCheckBox();
    }


    private void updatePlaylistCheckBox() {
      if (hasPlaylistName) {
        if (PlaylistCheckBoxIsEnabled) {
          if (IsDeletion) {
            PlaylistCheckBoxVisibility = Visibility.Hidden;
          } else {
            PlaylistCheckBoxVisibility = Visibility.Visible;
            IsAddPlaylist = false;
          }
        } else {
          //track is already in playlist, show it as disabled and selected
          PlaylistCheckBoxVisibility = Visibility.Visible;
          IsAddPlaylist = true;
        }
      } else {
        PlaylistCheckBoxVisibility = Visibility.Hidden;
      }
    }
  }
}
