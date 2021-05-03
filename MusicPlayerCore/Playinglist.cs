using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MusicPlayer {

  /********************************************************************************************************************
  Playlist: holds the Tracks to be played together
  Playinglist: holds the Tracks of a Playlist or Tracks selected by the user in a Window which have not been played yet.
    
  PlayingList can get the tracks to play from 2 different sources:

  A) PlayList: When the user starts playing a Playlist, all the PlaylistTracks get copied to Playinglist.ToPlayTracks.

  B) Selected tracks in Window: When user selects some tracks in a Window and starts to play them, all selected Tracks
  get copied to Playinglist.ToPlayTracks and Playinglist.AllTracks

  Playinglist.ToPlayTracks can hold 2 kinds of C# records:

  A) PlayinglistItemPlaylistTrack, which holds a reference to PlaylistTrack

  B) PlayinglistItemTrack, which holds a link to the track selected by the user

  When the Player plays a track from the playlist, the track gets removed from Playinglist.ToPlayTracks.

  Once Playinglist.ToPlayTracks is empty, it gets refilled from

  A) Playlist.PlaylistTracks

  B) Playinglist.AllTracks

  PlayingLists gets only deleted when its Playlist gets deleted.

  Playinglists don't get stored permanently. When the application starts and DC.Data finds a PlayinglistTrack, it 
  creates a Playinglist as needed. 

  If a PlaylistTrack gets deleted, its PlayinglistTrack gets also deleted, if it does exist, and removed from 
  Playinglist.ToPlayTracks

  The DC.Data.Playinglists Dictionary holds only Playinglist of type A). The Dictionary Key is Playlist. 

  ********************************************************************************************************************/

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

    private Playinglist() {
      allTracks = new();
      toPlayTracks = new();
    }


    /// <summary>
    /// Creates Playinglist for some tracks
    /// </summary>
    public Playinglist(IEnumerable<Track> tracks):this() {
      foreach (var track in tracks) {
        var playinglistItem = new PlayinglistItemTrack(track);
        allTracks.Add(playinglistItem);
        toPlayTracks.Add(playinglistItem);
      }
      if (allTracks.Count==0) throw new Exception("Playinglist cannot be constructed with 0 tracks.");
    }


    /// <summary>
    /// Creates Playinglist for playlist
    /// </summary>
    public Playinglist(Playlist playlist): this() {
      Playlist = playlist;
      DC.Data.Playinglists.Add(playlist, this);
      fill(playlist.PlaylistTracks);
    }


    private void fill(IEnumerable<PlaylistTrack> playlistTracks) {
      foreach (var playlistTrack in playlistTracks.OrderBy(pt => pt.TrackNo)) {
        var playinglistItem = new PlayinglistItemPlaylistTrack(playlistTrack);
        toPlayTracks.Add(playinglistItem);
        _ = new PlayinglistTrack(playlistTrack.Key);
      }
    }


    /// <summary>
    /// Used when Playinglist gets generated based on value in CSV file
    /// </summary>
    public Playinglist(IOrderedEnumerable<PlayinglistTrack> playinglistTracks) : 
      this() 
    {
      Playlist = playinglistTracks.First().PlaylistTrack!.Playlist;
      DC.Data.Playinglists.Add(Playlist, this);
      foreach (var playinglistTrack in playinglistTracks) {
        var playinglistItem = new PlayinglistItemPlaylistTrack(playinglistTrack.PlaylistTrack!);
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
      fill(playlist.PlaylistTracks);
    }


    public Track? GetNext(Random? random) {
      Track? next = null;
      if (toPlayTracks.Count>0) {
        var trackIndex = random?.Next(toPlayTracks.Count) ?? 0;
        var playinglistItem = toPlayTracks[trackIndex];
        toPlayTracks.RemoveAt(trackIndex);
        if (playinglistItem is PlayinglistItemTrack playinglistItemTrack) {
          next = playinglistItemTrack.Track;
        } else if (playinglistItem is PlayinglistItemPlaylistTrack playinglistItemPlaylistTrack) {
          if (DC.Data.PlayinglistTracksByPlaylistTrackKey.TryGetValue(playinglistItemPlaylistTrack.PlaylistTrack.Key, out var playinglistTrack)) {
            playinglistTrack.Release();
            next = playinglistItemPlaylistTrack.PlaylistTrack.Track;
          } else {
            //PlayinglistTrack not found, Track might just have been removed from Playlist
            //search for next existing playinglistItem
            while (toPlayTracks.Count>0) {
              if (trackIndex>=toPlayTracks.Count) {
                trackIndex = 0;
              }
              playinglistItem = toPlayTracks[trackIndex];
              toPlayTracks.RemoveAt(trackIndex);
              if (DC.Data.PlayinglistTracksByPlaylistTrackKey.TryGetValue(playinglistItemPlaylistTrack.PlaylistTrack.Key, out playinglistTrack)) {
                playinglistTrack.Release();
                return playinglistItemPlaylistTrack.PlaylistTrack.Track;
              }
            }
            System.Diagnostics.Debugger.Break();
            return null;
          }
        } else {
          throw new NotSupportedException();
        }
      }

      if (toPlayTracks.Count==0) {
        if (Playlist is null) {
          if (allTracks.Count==0) {
            System.Diagnostics.Debugger.Break();
            return null;
          }
          foreach (var copyTrack in allTracks) {
            toPlayTracks.Add(copyTrack);
            if (copyTrack is PlayinglistItemPlaylistTrack playinglistItemPlaylistTrack) {
              System.Diagnostics.Debugger.Break();//We should never come here, because PlayinglistItemPlaylistTrack are not used when Playlist is null
                //todo: change allTracks to List<PlayinglistItemTrack>
              _ = new PlayinglistTrack(playinglistItemPlaylistTrack.PlaylistTrack.Key);
            }
          }
        } else {
          fill(Playlist.PlaylistTracks);
          if (toPlayTracks.Count==0) {
            //it seems user has deleted all tracks from playlist
            return null;
          }
        }
      }
      if (next is null) {
        //toPlayTracks was empty when GetNext() was called. Now toPlayTracks is supposed to be full, unless user deleted
        //all tracks which were in the playlist in the meantime
        System.Diagnostics.Debugger.Break();
        return null;
      }
      return next;
    }


    /// <summary>
    /// Searches playinglistTrack in toPlayTracks and removes it.
    /// </summary>
    public void Remove(PlayinglistTrack playinglistTrack) {
      var playlistTrack = playinglistTrack.PlaylistTrack;
      for (int toPlayTracksIndex = 0; toPlayTracksIndex < toPlayTracks.Count; toPlayTracksIndex++) {
        var playinglistItem = toPlayTracks[toPlayTracksIndex];
        if (playinglistItem is PlayinglistItemPlaylistTrack playinglistItemPlaylistTrack) {
          if (playinglistItemPlaylistTrack.PlaylistTrack==playlistTrack) {
            toPlayTracks.RemoveAt(toPlayTracksIndex);
            return;
          }
        }
      }
    }


    //public void Relase() {
    //  if (allTracks is not null) return; //nothing to remove from DC.Data

    //  while (ToPlayTracks.Count>0) {
    //    var playinglistItemPlaylistTrack = (PlayinglistItemPlaylistTrack)ToPlayTracks[^1];
    //    playinglistItemPlaylistTrack.
    //  }
    //}
    #endregion
  }
}
