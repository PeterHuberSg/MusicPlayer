//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by StorageClassGenerator
//
//     Do not change code in this file, it will get lost when the file gets 
//     auto generated again. Write your code into Location.cs.
// </auto-generated>
//------------------------------------------------------------------------------
#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using StorageLib;


namespace MusicPlayer  {


  public partial class Location: IStorageItem<Location> {

    #region Properties
    //      ----------

    /// <summary>
    /// Unique identifier for Location. Gets set once Location gets added to DC.Data.
    /// </summary>
    public int Key { get; private set; }
    internal static void SetKey(IStorageItem location, int key, bool _) {
      ((Location)location).Key = key;
    }


    public string Path { get; private set; }


    /// <summary>
    /// Lower case version of Path
    /// </summary>
    public string PathLower { get; private set; }


    public string Name { get; private set; }


    public IStorageReadOnlyList<Track> Tracks => tracks;
    readonly StorageList<Track> tracks;


    /// <summary>
    /// Headers written to first line in CSV file
    /// </summary>
    internal static readonly string[] Headers = {"Key", "Path", "Name"};


    /// <summary>
    /// None existing Location, used as a temporary place holder when reading a CSV file
    /// which was not compacted. It might create first a later deleted item linking to a 
    /// deleted parent. In this case, the parent property gets set to NoLocation. Once the CSV
    /// file is completely read, that child will actually be deleted (released) and Verify()
    /// ensures that there are no stored children with links to NoLocation.
    /// </summary>
    internal static Location NoLocation = new Location("NoPath", "NoName", isStoring: false);
    #endregion


    #region Events
    //      ------

    /// <summary>
    /// Content of Location has changed. Gets only raised for changes occurring after loading DC.Data with previously stored data.
    /// </summary>
    public event Action</*old*/Location, /*new*/Location>? HasChanged;
    #endregion


    #region Constructors
    //      ------------

    /// <summary>
    /// Location Constructor. If isStoring is true, adds Location to DC.Data.Locations.
    /// </summary>
    public Location(string path, string name, bool isStoring = true) {
      Key = StorageExtensions.NoKey;
      Path = path;
      PathLower = Path.ToLowerInvariant();
      Name = name;
      tracks = new StorageList<Track>();
      onConstruct();
      if (DC.Data.IsTransaction) {
        DC.Data.AddTransaction(new TransactionItem(0,TransactionActivityEnum.New, Key, this));
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
    public Location(Location original) {
    #pragma warning restore CS8618 //
      Key = StorageExtensions.NoKey;
      Path = original.Path;
      PathLower = original.PathLower;
      Name = original.Name;
      onCloned(this);
    }
    partial void onCloned(Location clone);


    /// <summary>
    /// Constructor for Location read from CSV file
    /// </summary>
    private Location(int key, CsvReader csvReader){
      Key = key;
      Path = csvReader.ReadString();
      PathLower = Path.ToLowerInvariant();
      DC.Data._LocationsByPathLower.Add(PathLower, this);
      Name = csvReader.ReadString();
      tracks = new StorageList<Track>();
      onCsvConstruct();
    }
    partial void onCsvConstruct();


    /// <summary>
    /// New Location read from CSV file
    /// </summary>
    internal static Location Create(int key, CsvReader csvReader) {
      return new Location(key, csvReader);
    }
    #endregion


    #region Methods
    //      -------

    /// <summary>
    /// Adds Location to DC.Data.Locations.<br/>
    /// Throws an Exception when Location is already stored.
    /// </summary>
    public void Store() {
      if (Key>=0) {
        throw new Exception($"Location cannot be stored again in DC.Data, key {Key} is greater equal 0." + Environment.NewLine + ToString());
      }

      var isCancelled = false;
      onStoring(ref isCancelled);
      if (isCancelled) return;

      DC.Data._LocationsByPathLower.Add(PathLower, this);
      DC.Data._Locations.Add(this);
      onStored();
    }
    partial void onStoring(ref bool isCancelled);
    partial void onStored();


    /// <summary>
    /// Estimated number of UTF8 characters needed to write Location to CSV file
    /// </summary>
    public const int EstimatedLineLength = 300;


    /// <summary>
    /// Write Location to CSV file
    /// </summary>
    internal static void Write(Location location, CsvWriter csvWriter) {
      location.onCsvWrite();
      csvWriter.Write(location.Path);
      csvWriter.Write(location.Name);
    }
    partial void onCsvWrite();


    /// <summary>
    /// Updates Location with the provided values
    /// </summary>
    public void Update(string path, string name) {
      var clone = new Location(this);
      var isCancelled = false;
      onUpdating(path, name, ref isCancelled);
      if (isCancelled) return;


      //update properties and detect if any value has changed
      var isChangeDetected = false;
      if (Path!=path) {
        if (Key>=0) {
            DC.Data._LocationsByPathLower.Remove(PathLower);
        }
        Path = path;
        PathLower = Path.ToLowerInvariant();
        if (Key>=0) {
            DC.Data._LocationsByPathLower.Add(PathLower, this);
        }
        isChangeDetected = true;
      }
      if (Name!=name) {
        Name = name;
        isChangeDetected = true;
      }
      if (isChangeDetected) {
        onUpdated(clone);
        if (Key>=0) {
          DC.Data._Locations.ItemHasChanged(clone, this);
        } else if (DC.Data.IsTransaction) {
          DC.Data.AddTransaction(new TransactionItem(0, TransactionActivityEnum.Update, Key, this, oldItem: clone));
        }
        HasChanged?.Invoke(clone, this);
      }
    }
    partial void onUpdating(string path, string name, ref bool isCancelled);
    partial void onUpdated(Location old);


    /// <summary>
    /// Updates this Location with values from CSV file
    /// </summary>
    internal static void Update(Location location, CsvReader csvReader){
      DC.Data._LocationsByPathLower.Remove(location.PathLower);
      location.Path = csvReader.ReadString();
      location.PathLower = location.Path.ToLowerInvariant();
      DC.Data._LocationsByPathLower.Add(location.PathLower, location);
      location.Name = csvReader.ReadString();
      location.onCsvUpdate();
    }
    partial void onCsvUpdate();


    /// <summary>
    /// Add track to Tracks.
    /// </summary>
    internal void AddToTracks(Track track) {
#if DEBUG
      if (track==Track.NoTrack) throw new Exception();
      if ((track.Key>=0)&&(Key<0)) throw new Exception();
      if (tracks.Contains(track)) throw new Exception();
#endif
      tracks.Add(track);
      onAddedToTracks(track);
    }
    partial void onAddedToTracks(Track track);


    /// <summary>
    /// Removes track from Location.
    /// </summary>
    internal void RemoveFromTracks(Track track) {
#if DEBUG
      if (!tracks.Remove(track)) throw new Exception();
#else
        tracks.Remove(track);
#endif
      onRemovedFromTracks(track);
    }
    partial void onRemovedFromTracks(Track track);


    /// <summary>
    /// Removes Location from DC.Data.Locations.
    /// </summary>
    public void Release() {
      if (Key<0) {
        throw new Exception($"Location.Release(): Location '{this}' is not stored in DC.Data, key is {Key}.");
      }
      foreach (var track in Tracks) {
        if (track?.Key>=0) {
          throw new Exception($"Cannot release Location '{this}' " + Environment.NewLine + 
            $"because '{track}' in Location.Tracks is still stored.");
        }
      }
      DC.Data._LocationsByPathLower.Remove(PathLower);
      DC.Data._Locations.Remove(Key);
      onReleased();
    }
    partial void onReleased();


    /// <summary>
    /// Undoes the new() statement as part of a transaction rollback.
    /// </summary>
    internal static void RollbackItemNew(IStorageItem item) {
      var location = (Location) item;
      location.onRollbackItemNew();
    }
    partial void onRollbackItemNew();


    /// <summary>
    /// Releases Location from DC.Data.Locations as part of a transaction rollback of Store().
    /// </summary>
    internal static void RollbackItemStore(IStorageItem item) {
      var location = (Location) item;
      DC.Data._LocationsByPathLower.Remove(location.PathLower);
      DC.Data._LocationsByPathLower.Remove(location.PathLower);
      location.onRollbackItemStored();
    }
    partial void onRollbackItemStored();


    /// <summary>
    /// Restores the Location item data as it was before the last update as part of a transaction rollback.
    /// </summary>
    internal static void RollbackItemUpdate(IStorageItem oldStorageItem, IStorageItem newStorageItem) {
      var oldItem = (Location) oldStorageItem;//an item clone with the values before item was updated
      var item = (Location) newStorageItem;//is the instance whose values should be restored

      // remove updated item from dictionaries
      DC.Data._LocationsByPathLower.Remove(item.PathLower);

      // updated item: restore old values
      item.Path = oldItem.Path;
      item.PathLower = item.Path.ToLowerInvariant();
      item.Name = oldItem.Name;

      // add item with previous values to dictionaries
      DC.Data._LocationsByPathLower.Add(item.PathLower, item);
      item.onRollbackItemUpdated(oldItem);
    }
    partial void onRollbackItemUpdated(Location oldLocation);


    /// <summary>
    /// Adds Location to DC.Data.Locations as part of a transaction rollback of Release().
    /// </summary>
    internal static void RollbackItemRelease(IStorageItem item) {
      var location = (Location) item;
      DC.Data._LocationsByPathLower.Add(location.PathLower, location);
      location.onRollbackItemRelease();
    }
    partial void onRollbackItemRelease();


    /// <summary>
    /// Returns property values for tracing. Parents are shown with their key instead their content.
    /// </summary>
    public string ToTraceString() {
      var returnString =
        $"{this.GetKeyOrHash()}|" +
        $" {Path}|" +
        $" {PathLower}|" +
        $" {Name}";
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
        $" {Path}," +
        $" {PathLower}," +
        $" {Name}";
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
        $" Path: {Path}," +
        $" PathLower: {PathLower}," +
        $" Name: {Name}," +
        $" Tracks: {Tracks.Count}," +
        $" TracksStored: {Tracks.CountStoredItems};";
      onToString(ref returnString);
      return returnString;
    }
    partial void onToString(ref string returnString);
    #endregion
  }
}
