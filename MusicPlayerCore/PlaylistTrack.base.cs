//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by StorageClassGenerator
//
//     Do not change code in this file, it will get lost when the file gets 
//     auto generated again. Write your code into PlaylistTrack.cs.
// </auto-generated>
//------------------------------------------------------------------------------
#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using StorageLib;


namespace MusicPlayer  {


  public partial class PlaylistTrack: IStorageItemGeneric<PlaylistTrack> {

    #region Properties
    //      ----------

    /// <summary>
    /// Unique identifier for PlaylistTrack. Gets set once PlaylistTrack gets added to DC.Data.
    /// </summary>
    public int Key { get; private set; }
    internal static void SetKey(IStorageItem playlistTrack, int key, bool _) {
      ((PlaylistTrack)playlistTrack).Key = key;
    }


    public Playlist Playlist { get; private set; }


    public Track Track { get; private set; }


    public int TrackNo { get; private set; }


    /// <summary>
    /// Headers written to first line in CSV file
    /// </summary>
    internal static readonly string[] Headers = {"Key", "Playlist", "Track", "TrackNo"};


    /// <summary>
    /// None existing PlaylistTrack, used as a temporary place holder when reading a CSV file
    /// which was not compacted. It might create first a later deleted item linking to a 
    /// deleted parent. In this case, the parent property gets set to NoPlaylistTrack. Once the CSV
    /// file is completely read, that child will actually be deleted (released) and Verify()
    /// ensures that there are no stored children with links to NoPlaylistTrack.
    /// </summary>
    internal static PlaylistTrack NoPlaylistTrack = new PlaylistTrack(Playlist.NoPlaylist, Track.NoTrack, int.MinValue, isStoring: false);
    #endregion


    #region Events
    //      ------

    /// <summary>
    /// Content of PlaylistTrack has changed. Gets only raised for changes occurring after loading DC.Data with previously stored data.
    /// </summary>
    public event Action</*old*/PlaylistTrack, /*new*/PlaylistTrack>? HasChanged;
    #endregion


    #region Constructors
    //      ------------

    /// <summary>
    /// PlaylistTrack Constructor. If isStoring is true, adds PlaylistTrack to DC.Data.PlaylistTracks.
    /// </summary>
    public PlaylistTrack(Playlist playlist, Track track, int trackNo, bool isStoring = true) {
      Key = StorageExtensions.NoKey;
      Playlist = playlist;
      Track = track;
      TrackNo = trackNo;
      Playlist.AddToTracks(this);
      Track.AddToPlaylists(this);
      onConstruct();
      if (DC.Data.IsTransaction) {
        DC.Data.AddTransaction(new TransactionItem(3,TransactionActivityEnum.New, Key, this));
      }

      if (isStoring) {
        Store();
      }
    }
    partial void onConstruct();


    /// <summary>
    /// Cloning constructor. It will copy all data from original except any collection (children).
    /// </summary>
    #pragma warning disable CS8618 // Children collections are uninitialized.
    public PlaylistTrack(PlaylistTrack original) {
    #pragma warning restore CS8618 //
      Key = StorageExtensions.NoKey;
      Playlist = original.Playlist;
      Track = original.Track;
      TrackNo = original.TrackNo;
      onCloned(this);
    }
    partial void onCloned(PlaylistTrack clone);


    /// <summary>
    /// Constructor for PlaylistTrack read from CSV file
    /// </summary>
    private PlaylistTrack(int key, CsvReader csvReader){
      Key = key;
      var playlistKey = csvReader.ReadInt();
      Playlist = DC.Data._Playlists.GetItem(playlistKey)?? Playlist.NoPlaylist;
      var trackKey = csvReader.ReadInt();
      Track = DC.Data._Tracks.GetItem(trackKey)?? Track.NoTrack;
      TrackNo = csvReader.ReadInt();
      if (Playlist!=Playlist.NoPlaylist) {
        Playlist.AddToTracks(this);
      }
      if (Track!=Track.NoTrack) {
        Track.AddToPlaylists(this);
      }
      onCsvConstruct();
    }
    partial void onCsvConstruct();


    /// <summary>
    /// New PlaylistTrack read from CSV file
    /// </summary>
    internal static PlaylistTrack Create(int key, CsvReader csvReader) {
      return new PlaylistTrack(key, csvReader);
    }


    /// <summary>
    /// Verify that playlistTrack.Playlist exists.
    /// Verify that playlistTrack.Track exists.
    /// </summary>
    internal static bool Verify(PlaylistTrack playlistTrack) {
      if (playlistTrack.Playlist==Playlist.NoPlaylist) return false;
      if (playlistTrack.Track==Track.NoTrack) return false;
      return true;
    }
    #endregion


    #region Methods
    //      -------

    /// <summary>
    /// Adds PlaylistTrack to DC.Data.PlaylistTracks.<br/>
    /// Throws an Exception when PlaylistTrack is already stored.
    /// </summary>
    public void Store() {
      if (Key>=0) {
        throw new Exception($"PlaylistTrack cannot be stored again in DC.Data, key {Key} is greater equal 0." + Environment.NewLine + ToString());
      }

      var isCancelled = false;
      onStoring(ref isCancelled);
      if (isCancelled) return;

      if (Playlist.Key<0) {
        throw new Exception($"Cannot store child PlaylistTrack '{this}'.Playlist to Playlist '{Playlist}' because parent is not stored yet.");
      }
      if (Track.Key<0) {
        throw new Exception($"Cannot store child PlaylistTrack '{this}'.Track to Track '{Track}' because parent is not stored yet.");
      }
      DC.Data._PlaylistTracks.Add(this);
      onStored();
    }
    partial void onStoring(ref bool isCancelled);
    partial void onStored();


    /// <summary>
    /// Estimated number of UTF8 characters needed to write PlaylistTrack to CSV file
    /// </summary>
    public const int EstimatedLineLength = 27;


    /// <summary>
    /// Write PlaylistTrack to CSV file
    /// </summary>
    internal static void Write(PlaylistTrack playlistTrack, CsvWriter csvWriter) {
      playlistTrack.onCsvWrite();
      if (playlistTrack.Playlist.Key<0) throw new Exception($"Cannot write playlistTrack '{playlistTrack}' to CSV File, because Playlist is not stored in DC.Data.Playlists.");

      csvWriter.Write(playlistTrack.Playlist.Key.ToString());
      if (playlistTrack.Track.Key<0) throw new Exception($"Cannot write playlistTrack '{playlistTrack}' to CSV File, because Track is not stored in DC.Data.Tracks.");

      csvWriter.Write(playlistTrack.Track.Key.ToString());
      csvWriter.Write(playlistTrack.TrackNo);
    }
    partial void onCsvWrite();


    /// <summary>
    /// Updates PlaylistTrack with the provided values
    /// </summary>
    public void Update(Playlist playlist, Track track, int trackNo) {
      if (Key>=0){
        if (playlist.Key<0) {
          throw new Exception($"PlaylistTrack.Update(): It is illegal to add stored PlaylistTrack '{this}'" + Environment.NewLine + 
            $"to Playlist '{playlist}', which is not stored.");
        }
        if (track.Key<0) {
          throw new Exception($"PlaylistTrack.Update(): It is illegal to add stored PlaylistTrack '{this}'" + Environment.NewLine + 
            $"to Track '{track}', which is not stored.");
        }
      }
      var clone = new PlaylistTrack(this);
      var isCancelled = false;
      onUpdating(playlist, track, trackNo, ref isCancelled);
      if (isCancelled) return;


      //remove not yet updated item from parents which will be removed by update
      var hasPlaylistChanged = Playlist!=playlist;
      if (hasPlaylistChanged) {
        Playlist.RemoveFromTracks(this);
      }
      var hasTrackChanged = Track!=track;
      if (hasTrackChanged) {
        Track.RemoveFromPlaylists(this);
      }

      //update properties and detect if any value has changed
      var isChangeDetected = false;
      if (Playlist!=playlist) {
        Playlist = playlist;
        isChangeDetected = true;
      }
      if (Track!=track) {
        Track = track;
        isChangeDetected = true;
      }
      if (TrackNo!=trackNo) {
        TrackNo = trackNo;
        isChangeDetected = true;
      }

      //add updated item to parents which have been newly added during update
      if (hasPlaylistChanged) {
        Playlist.AddToTracks(this);
      }
      if (hasTrackChanged) {
        Track.AddToPlaylists(this);
      }
      if (isChangeDetected) {
        onUpdated(clone);
        if (Key>=0) {
          DC.Data._PlaylistTracks.ItemHasChanged(clone, this);
        } else if (DC.Data.IsTransaction) {
          DC.Data.AddTransaction(new TransactionItem(3, TransactionActivityEnum.Update, Key, this, oldItem: clone));
        }
        HasChanged?.Invoke(clone, this);
      }
    }
    partial void onUpdating(Playlist playlist, Track track, int trackNo, ref bool isCancelled);
    partial void onUpdated(PlaylistTrack old);


    /// <summary>
    /// Updates this PlaylistTrack with values from CSV file
    /// </summary>
    internal static void Update(PlaylistTrack playlistTrack, CsvReader csvReader){
        var playlist = DC.Data._Playlists.GetItem(csvReader.ReadInt())??
          Playlist.NoPlaylist;
      if (playlistTrack.Playlist!=playlist) {
        if (playlistTrack.Playlist!=Playlist.NoPlaylist) {
          playlistTrack.Playlist.RemoveFromTracks(playlistTrack);
        }
        playlistTrack.Playlist = playlist;
        playlistTrack.Playlist.AddToTracks(playlistTrack);
      }
        var track = DC.Data._Tracks.GetItem(csvReader.ReadInt())??
          Track.NoTrack;
      if (playlistTrack.Track!=track) {
        if (playlistTrack.Track!=Track.NoTrack) {
          playlistTrack.Track.RemoveFromPlaylists(playlistTrack);
        }
        playlistTrack.Track = track;
        playlistTrack.Track.AddToPlaylists(playlistTrack);
      }
      playlistTrack.TrackNo = csvReader.ReadInt();
      playlistTrack.onCsvUpdate();
    }
    partial void onCsvUpdate();


    /// <summary>
    /// Removes PlaylistTrack from DC.Data.PlaylistTracks.
    /// </summary>
    public void Release() {
      if (Key<0) {
        throw new Exception($"PlaylistTrack.Release(): PlaylistTrack '{this}' is not stored in DC.Data, key is {Key}.");
      }
      onReleased();
      DC.Data._PlaylistTracks.Remove(Key);
    }
    partial void onReleased();


    /// <summary>
    /// Removes PlaylistTrack from parents as part of a transaction rollback of the new() statement.
    /// </summary>
    internal static void RollbackItemNew(IStorageItem item) {
      var playlistTrack = (PlaylistTrack) item;
      if (playlistTrack.Playlist!=Playlist.NoPlaylist) {
        playlistTrack.Playlist.RemoveFromTracks(playlistTrack);
      }
      if (playlistTrack.Track!=Track.NoTrack) {
        playlistTrack.Track.RemoveFromPlaylists(playlistTrack);
      }
      playlistTrack.onRollbackItemNew();
    }
    partial void onRollbackItemNew();


    /// <summary>
    /// Releases PlaylistTrack from DC.Data.PlaylistTracks as part of a transaction rollback of Store().
    /// </summary>
    internal static void RollbackItemStore(IStorageItem item) {
      var playlistTrack = (PlaylistTrack) item;
      playlistTrack.onRollbackItemStored();
    }
    partial void onRollbackItemStored();


    /// <summary>
    /// Restores the PlaylistTrack item data as it was before the last update as part of a transaction rollback.
    /// </summary>
    internal static void RollbackItemUpdate(IStorageItem oldStorageItem, IStorageItem newStorageItem) {
      var oldItem = (PlaylistTrack) oldStorageItem;//an item clone with the values before item was updated
      var item = (PlaylistTrack) newStorageItem;//is the instance whose values should be restored

      // remove updated item from parents
      var hasPlaylistChanged = oldItem.Playlist!=item.Playlist;
      if (hasPlaylistChanged) {
        item.Playlist.RemoveFromTracks(item);
      }
      var hasTrackChanged = oldItem.Track!=item.Track;
      if (hasTrackChanged) {
        item.Track.RemoveFromPlaylists(item);
      }

      // updated item: restore old values
      item.Playlist = oldItem.Playlist;
      item.Track = oldItem.Track;
      item.TrackNo = oldItem.TrackNo;

      // add item with previous values to parents
      if (hasPlaylistChanged) {
        item.Playlist.AddToTracks(item);
      }
      if (hasTrackChanged) {
        item.Track.AddToPlaylists(item);
      }
      item.onRollbackItemUpdated(oldItem);
    }
    partial void onRollbackItemUpdated(PlaylistTrack oldPlaylistTrack);


    /// <summary>
    /// Adds PlaylistTrack to DC.Data.PlaylistTracks as part of a transaction rollback of Release().
    /// </summary>
    internal static void RollbackItemRelease(IStorageItem item) {
      var playlistTrack = (PlaylistTrack) item;
      playlistTrack.onRollbackItemRelease();
    }
    partial void onRollbackItemRelease();


    /// <summary>
    /// Returns property values for tracing. Parents are shown with their key instead their content.
    /// </summary>
    public string ToTraceString() {
      var returnString =
        $"{this.GetKeyOrHash()}|" +
        $" Playlist {Playlist.GetKeyOrHash()}|" +
        $" Track {Track.GetKeyOrHash()}|" +
        $" {TrackNo}";
      onToTraceString(ref returnString);
      return returnString;
    }
    partial void onToTraceString(ref string returnString);


    /// <summary>
    /// Returns property values
    /// </summary>
    public string ToShortString() {
      var returnString =
        $"{Key.ToKeyString()}," +
        $" {Playlist.ToShortString()}," +
        $" {Track.ToShortString()}," +
        $" {TrackNo}";
      onToShortString(ref returnString);
      return returnString;
    }
    partial void onToShortString(ref string returnString);


    /// <summary>
    /// Returns all property names and values
    /// </summary>
    public override string ToString() {
      var returnString =
        $"Key: {Key.ToKeyString()}," +
        $" Playlist: {Playlist.ToShortString()}," +
        $" Track: {Track.ToShortString()}," +
        $" TrackNo: {TrackNo};";
      onToString(ref returnString);
      return returnString;
    }
    partial void onToString(ref string returnString);
    #endregion
  }
}
