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
using Storage;


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
    /// Directory of all Tracks
    /// </summary>
    public DataStore<Track> Tracks { get; private set; }

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
    public DC(CsvConfig? csvConfig): base(DataStoresCount: 1) {
      data = this;
      IsInitialised = false;

      string? backupResult = null;
      if (csvConfig!=null) {
        backupResult = Csv.Backup(csvConfig, DateTime.Now);
      }

      CsvConfig = csvConfig;
      onConstructing(backupResult);

      _TracksByTitleArtists = new Dictionary<string, Track>();
      if (csvConfig==null) {
        Tracks = new DataStore<Track>(
          this,
          0,
          Track.SetKey,
          Track.RollbackItemNew,
          Track.RollbackItemStore,
          Track.RollbackItemUpdate,
          Track.RollbackItemRelease,
          areInstancesUpdatable: true,
          areInstancesDeletable: true);
        DataStores[0] = Tracks;
        onTracksFilled();

      } else {
        Tracks = new DataStoreCSV<Track>(
          this,
          0,
          csvConfig!,
          Track.EstimatedLineLength,
          Track.Headers,
          Track.SetKey,
          Track.Create,
          null,
          Track.Update,
          Track.Write,
          Track.RollbackItemNew,
          Track.RollbackItemStore,
          Track.RollbackItemUpdate,
          Track.RollbackItemRelease,
          areInstancesUpdatable: true,
          areInstancesDeletable: true);
        DataStores[0] = Tracks;
        onTracksFilled();

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
    /// Called once the data for Tracks is read.
    /// </summary>}
    partial void onTracksFilled();
    #endregion


    #region Overrides
    //      ---------

    internal new void AddTransaction(TransactionItem transactionItem) {
      base.AddTransaction(transactionItem);
    }


    protected override void Dispose(bool disposing) {
      if (disposing) {
        onDispose();
        Tracks?.Dispose();
        Tracks = null!;
        _TracksByTitleArtists = null!;
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
