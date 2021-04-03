using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MusicPlayer {

  public abstract record PlayinglistItem ();
  public record PlayinglistItemTrack(Track Track): PlayinglistItem;
  public record PlayinglistItemPlaylistTrack(PlaylistTrack PlaylistTrack): PlayinglistItem;

  //public abstract class PlayinglistItem { }
  //public class PlayinglistItemTrack: PlayinglistItem {
  //  public Track Track { get; }
  //  public PlayinglistItemTrack(Track track) {
  //    Track = track;
  //  }
  //}
  //public class PlayinglistItemPlaylistTrack(PlaylistTrack PlaylistTrack): PlayinglistItem;



  public class Playinglist {

    #region Properties
    //      ----------

    public Playlist? Playlist { get; }


    public IReadOnlyList<PlayinglistItem> AllTracks => allTracks;
    readonly List<PlayinglistItem> allTracks;

    public IReadOnlyList<PlayinglistItem> ToPlayTracks => toPlayTracks;
    readonly List<PlayinglistItem> toPlayTracks;
    #endregion


    #region Constructors
    //      ------------

    private Playinglist(Playlist? playlist=null) {
      Playlist=playlist;
      allTracks = new();
      toPlayTracks = new();
    }


    public Playinglist(IEnumerable<Track> tracks):this() {
      foreach (var track in tracks) {
        var playinglistItem = new PlayinglistItemTrack(track);
        allTracks.Add(playinglistItem);
        toPlayTracks.Add(playinglistItem);
      }
      if (AllTracks.Count==0) throw new Exception("Playinglist cannot be constructed with 0 tracks.");
    }


    public Playinglist(IEnumerable<PlaylistTrack> playlistTracks) : this(playlistTracks.First().Playlist) {
      fill(playlistTracks);
    }


    private void fill(IEnumerable<PlaylistTrack> playlistTracks) {
      foreach (var playlistTrack in playlistTracks) {
        var playinglistItem = new PlayinglistItemPlaylistTrack(playlistTrack);
        toPlayTracks.Add(playinglistItem);
        _ = new PlayinglistTrack(playlistTrack.Key);
      }
    }


    /// <summary>
    /// Used when Playinglist gets generated based on value in CSV file
    /// </summary>
    public Playinglist(IOrderedEnumerable<PlayinglistTrack> playinglistTracks) : 
      this(playinglistTracks.First().PlaylistTrack!.Playlist) 
    {
      foreach (var playinglistTrack in playinglistTracks) {
        var playinglistItem = new PlayinglistItemPlaylistTrack(playinglistTrack.PlaylistTrack!);
        allTracks.Add(playinglistItem);
        toPlayTracks.Add(playinglistItem);
      }
    }
    #endregion


    #region Methods
    //      -------

    internal void Refill(Playlist playlist) {
      foreach (var playinglistItem in ToPlayTracks) {
        if (playinglistItem is PlayinglistItemPlaylistTrack playinglistItemPlaylistTrack) {
          var playinglistTrack = DC.Data.PlayinglistTracksByPlaylistTrackKey[playinglistItemPlaylistTrack.PlaylistTrack.Key];
          playinglistTrack.Release();
        } else {
          throw new NotSupportedException();
        }
      }
      allTracks.Clear();
      toPlayTracks.Clear();
      fill(playlist.Tracks.OrderBy(pt => pt.TrackNo));
    }


    public Track GetNext(Random? random) {
      Track? next = null;
      if (toPlayTracks.Count>0) {
        var trackIndex = random?.Next(toPlayTracks.Count) ?? 0;
        var playinglistItem = toPlayTracks[trackIndex];
        toPlayTracks.RemoveAt(trackIndex);
        if (playinglistItem is PlayinglistItemTrack playinglistItemTrack) {
          next = playinglistItemTrack.Track;
        } else if (playinglistItem is PlayinglistItemPlaylistTrack playinglistItemPlaylistTrack) {
          var playinglistTrack = DC.Data.PlayinglistTracksByPlaylistTrackKey[playinglistItemPlaylistTrack.PlaylistTrack.Key];
          playinglistTrack.Release();
          next = playinglistItemPlaylistTrack.PlaylistTrack.Track;
        } else {
          throw new NotSupportedException();
        }
      }

      if (toPlayTracks.Count==0) {
        if (Playlist is null) {
          foreach (var copyTrack in allTracks) {
            toPlayTracks.Add(copyTrack);
            if (copyTrack is PlayinglistItemPlaylistTrack playinglistItemPlaylistTrack) {
              _ = new PlayinglistTrack(playinglistItemPlaylistTrack.PlaylistTrack.Key);
            }
          }
        } else {
          fill(Playlist.Tracks.OrderBy(pt => pt.TrackNo));
        }
      }
      if (next is null) {
        //toPlayTracks was empty when GetNext() was called. Now toPlayTracks is supposed to be full
        System.Diagnostics.Debugger.Break();
        return GetNext(random);
      }
      return next;
    }
    #endregion
  }
}
