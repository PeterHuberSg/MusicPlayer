using System;
using System.Collections.Generic;
using StorageLib;


namespace MusicPlayer  {


  public partial class Playlist: IStorageItemGeneric<Playlist> {


    #region Properties
    //      ----------

    public int TracksCount { get; private set; }
    public TimeSpan TracksDuration { get; private set; }
    #endregion


    #region Events
    //      ------

    #endregion


    #region Constructors
    //      ------------

    /// <summary>
    /// Called once the constructor has filled all the properties
    /// </summary>
    partial void onConstruct() {
      if (Name!="NoName") {
        DC.Data.UpdatePlayListStrings();
      }
    }


    /// <summary>
    /// Called once the cloning constructor has filled all the properties. Clones have no children data.
    /// </summary>
    //partial void onCloned(Playlist clone) {
    //}


    /// <summary>
    /// Called once the CSV-constructor who reads the data from a CSV file has filled all the properties
    /// </summary>
    //partial void onCsvConstruct() {
    //}


    #endregion


    #region Methods
    //      -------

    /// <summary>
    /// Called before {ClassName}.Store() gets executed
    /// </summary>
    //partial void onStoring(ref bool isCancelled) {
    //}


    /// <summary>
    /// Called after Playlist.Store() is executed
    /// </summary>
    //partial void onStored() {
    //}


    /// <summary>
    /// Called before Playlist gets written to a CSV file
    /// </summary>
    //partial void onCsvWrite() {
    //}


    /// <summary>
    /// Called after all properties of Playlist are updated, but before the HasChanged event gets raised
    /// </summary>
    //partial void onUpdating(string name, ref bool isCancelled){
    //}


    /// <summary>
    /// Called after all properties of Playlist are updated, but before the HasChanged event gets raised
    /// </summary>
    partial void onUpdated(Playlist old) {
      DC.Data.UpdatePlayListStrings();
    }


    /// <summary>
    /// Called after an update for Playlist is read from a CSV file
    /// </summary>
    //partial void onCsvUpdate() {
    //}


    /// <summary>
    /// Called after Playlist.Release() got executed
    /// </summary>
    partial void onReleased() {
      DC.Data.UpdatePlayListStrings();
    }


    /// <summary>
    /// Called after 'new Playlist()' transaction is rolled back
    /// </summary>
    partial void onRollbackItemNew() {
      DC.Data.UpdatePlayListStrings();
    }


    /// <summary>
    /// Called after Playlist.Store() transaction is rolled back
    /// </summary>
    //partial void onRollbackItemStored() {
    //}


    /// <summary>
    /// Called after Playlist.Update() transaction is rolled back
    /// </summary>
    partial void onRollbackItemUpdated(Playlist oldPlaylist) {
      DC.Data.UpdatePlayListStrings();
    }


    /// <summary>
    /// Called after Playlist.Release() transaction is rolled back
    /// </summary>
    partial void onRollbackItemRelease() {
      DC.Data.UpdatePlayListStrings();
    }


    /// <summary>
    /// Called after a playlistTrack gets added to Tracks.
    /// </summary>
    partial void onAddedToTracks(PlaylistTrack playlistTrack) {
      TracksCount++;
      TracksDuration += playlistTrack.Track.Duration??TimeSpan.Zero;
    }


    /// <summary>
    /// Called after a playlistTrack gets removed from Tracks.
    /// </summary>
    partial void onRemovedFromTracks(PlaylistTrack playlistTrack) {
      TracksCount--;
      TracksDuration -= playlistTrack.Track.Duration??TimeSpan.Zero;
    }


    /// <summary>
    /// Updates returnString with additional info for a short description.
    /// </summary>
    //partial void onToShortString(ref string returnString) {
    //}


    /// <summary>
    /// Updates returnString with additional info for a short description.
    /// </summary>
    //partial void onToString(ref string returnString) {
    //}
    #endregion
  }
}
