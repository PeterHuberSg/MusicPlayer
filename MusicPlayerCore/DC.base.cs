//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by StorageClassGenerator
//
//     Do not change code in this file, it will get lost when the file gets 
//     auto generated again. Write your code into DC.cs.
// </auto-generated>
//------------------------------------------------------------------------------
#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using StorageLib;


namespace MusicPlayer  {

  /// <summary>
  /// A part of DC is static, which gives easy access to all stored data (=context) through DC.Data. But most functionality is in the
  /// instantiatable part of DC. Since it is instantiatable, is possible to use different contexts over the lifetime of a program. This 
  /// is helpful for unit testing. Use DC.Init() to create a new context and dispose it with DisposeData() before creating a new one.
  /// </summary>
  public partial class DC: DataContextBase {

    #region static Part
    //      -----------

    /// <summary>
    /// Provides static root access to the data context
    /// </summary>
    public static DC Data {
      get { return data!; }
    }
    private static DC? data; //data is needed for Interlocked.Exchange(ref data, null) in DisposeData()


    /// <summary>
    /// Flushes all data to permanent storage location if permanent data storage is active. Compacts data storage
    /// by applying all updates and removing all instances marked as deleted.
    /// </summary>
    public static void DisposeData() {
      var dataLocal = Interlocked.Exchange(ref data, null);
      dataLocal?.Dispose();
    }
    #endregion


    #region Properties
    //      ----------

    /// <summary>
    /// Configuration parameters if data gets stored in .csv files
    /// </summary>
    public CsvConfig? CsvConfig { get; }

    /// <summary>
    /// Is all data initialised
    /// </summary>
    public bool IsInitialised { get; private set; }

    /// <summary>
    /// Directory of all Locations
    /// </summary>
    public IReadonlyDataStore<Location> Locations => _Locations;
    internal DataStore<Location> _Locations { get; private set; }

    /// <summary>
    /// Directory of all Locations by PathLower
    /// </summary>
    public IReadOnlyDictionary<string, Location> LocationsByPathLower => _LocationsByPathLower;
    internal Dictionary<string, Location> _LocationsByPathLower { get; private set; }

    /// <summary>
    /// Directory of all PlayinglistTracks
    /// </summary>
    public IReadonlyDataStore<PlayinglistTrack> PlayinglistTracks => _PlayinglistTracks;
    internal DataStore<PlayinglistTrack> _PlayinglistTracks { get; private set; }

    /// <summary>
    /// Directory of all PlayinglistTracks by PlaylistTrackKey
    /// </summary>
    public IReadOnlyDictionary<int, PlayinglistTrack> PlayinglistTracksByPlaylistTrackKey => _PlayinglistTracksByPlaylistTrackKey;
    internal Dictionary<int, PlayinglistTrack> _PlayinglistTracksByPlaylistTrackKey { get; private set; }

    /// <summary>
    /// Directory of all Playlists
    /// </summary>
    public IReadonlyDataStore<Playlist> Playlists => _Playlists;
    internal DataStore<Playlist> _Playlists { get; private set; }

    /// <summary>
    /// Directory of all Playlists by NameLower
    /// </summary>
    public IReadOnlyDictionary<string, Playlist> PlaylistsByNameLower => _PlaylistsByNameLower;
    internal Dictionary<string, Playlist> _PlaylistsByNameLower { get; private set; }

    /// <summary>
    /// Directory of all PlaylistTracks
    /// </summary>
    public IReadonlyDataStore<PlaylistTrack> PlaylistTracks => _PlaylistTracks;
    internal DataStore<PlaylistTrack> _PlaylistTracks { get; private set; }

    /// <summary>
    /// Directory of all Tracks
    /// </summary>
    public IReadonlyDataStore<Track> Tracks => _Tracks;
    internal DataStore<Track> _Tracks { get; private set; }

    /// <summary>
    /// Directory of all Tracks by TitleArtists
    /// </summary>
    public IReadOnlyDictionary<string, Track> TracksByTitleArtists => _TracksByTitleArtists;
    internal Dictionary<string, Track> _TracksByTitleArtists { get; private set; }
    #endregion


    #region Events
    //      ------

    #endregion


    #region Constructors
    //      ------------

    /// <summary>
    /// Creates a new DataContext. If csvConfig is null, the data is only stored in RAM and gets lost once the 
    /// program terminates. With csvConfig defined, existing data gets read at startup, changes get immediately
    /// written and Dispose() ensures by flushing that all data is permanently stored.
    /// </summary>
    public DC(CsvConfig? csvConfig): base(DataStoresCount: 5) {
      data = this;
      IsInitialised = false;

      string? backupResult = null;
      if (csvConfig!=null) {
        backupResult = Csv.Backup(csvConfig, DateTime.Now);
      }

      CsvConfig = csvConfig;
      onConstructing(backupResult);

      _LocationsByPathLower = new Dictionary<string, Location>();
      _PlayinglistTracksByPlaylistTrackKey = new Dictionary<int, PlayinglistTrack>();
      _PlaylistsByNameLower = new Dictionary<string, Playlist>();
      _TracksByTitleArtists = new Dictionary<string, Track>();
      if (csvConfig==null) {
        _Locations = new DataStore<Location>(
          this,
          0,
          Location.SetKey,
          Location.RollbackItemNew,
          Location.RollbackItemStore,
          Location.RollbackItemUpdate,
          Location.RollbackItemRelease,
          areInstancesUpdatable: true,
          areInstancesReleasable: true);
        DataStores[0] = _Locations;
        onLocationsFilled();

        _Tracks = new DataStore<Track>(
          this,
          1,
          Track.SetKey,
          Track.RollbackItemNew,
          Track.RollbackItemStore,
          Track.RollbackItemUpdate,
          Track.RollbackItemRelease,
          areInstancesUpdatable: true,
          areInstancesReleasable: true);
        DataStores[1] = _Tracks;
        onTracksFilled();

        _Playlists = new DataStore<Playlist>(
          this,
          2,
          Playlist.SetKey,
          Playlist.RollbackItemNew,
          Playlist.RollbackItemStore,
          Playlist.RollbackItemUpdate,
          Playlist.RollbackItemRelease,
          areInstancesUpdatable: true,
          areInstancesReleasable: true);
        DataStores[2] = _Playlists;
        onPlaylistsFilled();

        _PlaylistTracks = new DataStore<PlaylistTrack>(
          this,
          3,
          PlaylistTrack.SetKey,
          PlaylistTrack.RollbackItemNew,
          PlaylistTrack.RollbackItemStore,
          PlaylistTrack.RollbackItemUpdate,
          PlaylistTrack.RollbackItemRelease,
          areInstancesUpdatable: true,
          areInstancesReleasable: true);
        DataStores[3] = _PlaylistTracks;
        onPlaylistTracksFilled();

        _PlayinglistTracks = new DataStore<PlayinglistTrack>(
          this,
          4,
          PlayinglistTrack.SetKey,
          PlayinglistTrack.RollbackItemNew,
          PlayinglistTrack.RollbackItemStore,
          PlayinglistTrack.RollbackItemUpdate,
          PlayinglistTrack.RollbackItemRelease,
          areInstancesUpdatable: true,
          areInstancesReleasable: true);
        DataStores[4] = _PlayinglistTracks;
        onPlayinglistTracksFilled();

      } else {
        IsPartiallyNew = false;
        _Locations = new DataStoreCSV<Location>(
          this,
          0,
          csvConfig!,
          Location.EstimatedLineLength,
          Location.Headers,
          Location.SetKey,
          Location.Create,
          null,
          Location.Update,
          Location.Write,
          Location.RollbackItemNew,
          Location.RollbackItemStore,
          Location.RollbackItemUpdate,
          Location.RollbackItemRelease,
          areInstancesUpdatable: true,
          areInstancesReleasable: true);
        IsPartiallyNew |= _Locations.IsNew;
        IsNew &= _Locations.IsNew;
        DataStores[0] = _Locations;
        onLocationsFilled();

        _Tracks = new DataStoreCSV<Track>(
          this,
          1,
          csvConfig!,
          Track.EstimatedLineLength,
          Track.Headers,
          Track.SetKey,
          Track.Create,
          Track.Verify,
          Track.Update,
          Track.Write,
          Track.RollbackItemNew,
          Track.RollbackItemStore,
          Track.RollbackItemUpdate,
          Track.RollbackItemRelease,
          areInstancesUpdatable: true,
          areInstancesReleasable: true);
        IsPartiallyNew |= _Tracks.IsNew;
        IsNew &= _Tracks.IsNew;
        DataStores[1] = _Tracks;
        onTracksFilled();

        _Playlists = new DataStoreCSV<Playlist>(
          this,
          2,
          csvConfig!,
          Playlist.EstimatedLineLength,
          Playlist.Headers,
          Playlist.SetKey,
          Playlist.Create,
          null,
          Playlist.Update,
          Playlist.Write,
          Playlist.RollbackItemNew,
          Playlist.RollbackItemStore,
          Playlist.RollbackItemUpdate,
          Playlist.RollbackItemRelease,
          areInstancesUpdatable: true,
          areInstancesReleasable: true);
        IsPartiallyNew |= _Playlists.IsNew;
        IsNew &= _Playlists.IsNew;
        DataStores[2] = _Playlists;
        onPlaylistsFilled();

        _PlaylistTracks = new DataStoreCSV<PlaylistTrack>(
          this,
          3,
          csvConfig!,
          PlaylistTrack.EstimatedLineLength,
          PlaylistTrack.Headers,
          PlaylistTrack.SetKey,
          PlaylistTrack.Create,
          PlaylistTrack.Verify,
          PlaylistTrack.Update,
          PlaylistTrack.Write,
          PlaylistTrack.RollbackItemNew,
          PlaylistTrack.RollbackItemStore,
          PlaylistTrack.RollbackItemUpdate,
          PlaylistTrack.RollbackItemRelease,
          areInstancesUpdatable: true,
          areInstancesReleasable: true);
        IsPartiallyNew |= _PlaylistTracks.IsNew;
        IsNew &= _PlaylistTracks.IsNew;
        DataStores[3] = _PlaylistTracks;
        onPlaylistTracksFilled();

        _PlayinglistTracks = new DataStoreCSV<PlayinglistTrack>(
          this,
          4,
          csvConfig!,
          PlayinglistTrack.EstimatedLineLength,
          PlayinglistTrack.Headers,
          PlayinglistTrack.SetKey,
          PlayinglistTrack.Create,
          null,
          PlayinglistTrack.Update,
          PlayinglistTrack.Write,
          PlayinglistTrack.RollbackItemNew,
          PlayinglistTrack.RollbackItemStore,
          PlayinglistTrack.RollbackItemUpdate,
          PlayinglistTrack.RollbackItemRelease,
          areInstancesUpdatable: true,
          areInstancesReleasable: true);
        IsPartiallyNew |= _PlayinglistTracks.IsNew;
        IsNew &= _PlayinglistTracks.IsNew;
        DataStores[4] = _PlayinglistTracks;
        onPlayinglistTracksFilled();

      }
      onConstructed();
      IsInitialised = true;
    }

    /// <summary>}
    /// Called at beginning of constructor
    /// </summary>}
    partial void onConstructing(string? backupResult);

    /// <summary>}
    /// Called at end of constructor
    /// </summary>}
    partial void onConstructed();

    /// <summary>}
    /// Called once the data for Locations is read.
    /// </summary>}
    partial void onLocationsFilled();

    /// <summary>}
    /// Called once the data for Tracks is read.
    /// </summary>}
    partial void onTracksFilled();

    /// <summary>}
    /// Called once the data for Playlists is read.
    /// </summary>}
    partial void onPlaylistsFilled();

    /// <summary>}
    /// Called once the data for PlaylistTracks is read.
    /// </summary>}
    partial void onPlaylistTracksFilled();

    /// <summary>}
    /// Called once the data for PlayinglistTracks is read.
    /// </summary>}
    partial void onPlayinglistTracksFilled();
    #endregion


    #region Overrides
    //      ---------

    internal new void AddTransaction(TransactionItem transactionItem) {
      base.AddTransaction(transactionItem);
    }


    protected override void Dispose(bool disposing) {
      if (disposing) {
        onDispose();
        _PlayinglistTracks?.Dispose();
        _PlayinglistTracks = null!;
        _PlayinglistTracksByPlaylistTrackKey = null!;
        _PlaylistTracks?.Dispose();
        _PlaylistTracks = null!;
        _Playlists?.Dispose();
        _Playlists = null!;
        _PlaylistsByNameLower = null!;
        _Tracks?.Dispose();
        _Tracks = null!;
        _TracksByTitleArtists = null!;
        _Locations?.Dispose();
        _Locations = null!;
        _LocationsByPathLower = null!;
        data = null;
      }
      base.Dispose(disposing);
    }

    /// <summary>}
    /// Called before storageDirectories get disposed.
    /// </summary>}
    partial void onDispose();
    #endregion


    #region Methods
    //      -------

    #endregion

  }
}

