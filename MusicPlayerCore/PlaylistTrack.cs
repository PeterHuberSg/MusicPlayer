using System;
using System.Collections.Generic;
using StorageLib;


namespace MusicPlayer  {


  public partial class PlaylistTrack: IStorageItem<PlaylistTrack> {


    #region Properties
    //      ----------

    #endregion


    #region Events
    //      ------

    #endregion


    #region Constructors
    //      ------------

    /// <summary>
    /// Called once the constructor has filled all the properties
    /// </summary>
    //partial void onConstruct() {
    //}


    /// <summary>
    /// Called once the cloning constructor has filled all the properties. Clones have no children data.
    /// </summary>
    //partial void onCloned(PlaylistTrack clone) {
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
    /// Called after PlaylistTrack.Store() is executed
    /// </summary>
    //partial void onStored() {
    //}


    /// <summary>
    /// Called before PlaylistTrack gets written to a CSV file
    /// </summary>
    //partial void onCsvWrite() {
    //}


    /// <summary>
    /// Called after all properties of PlaylistTrack are updated, but before the HasChanged event gets raised
    /// </summary>
    //partial void onUpdating(Playlist playlist, Track track, ref bool isCancelled){
    //}


    /// <summary>
    /// Called after all properties of PlaylistTrack are updated, but before the HasChanged event gets raised
    /// </summary>
    //partial void onUpdated(PlaylistTrack old) {
    //}


    /// <summary>
    /// Called after an update for PlaylistTrack is read from a CSV file
    /// </summary>
    //partial void onCsvUpdate() {
    //}


    /// <summary>
    /// Called before PlaylistTrack.Release() gets executed
    /// </summary>
    partial void onReleasing() {
      if (DC.Data.PlayinglistTracksByPlaylistTrackKey.TryGetValue(Key, out var playinglistTrack)) {
        playinglistTrack.Release();
        //playinglistTrack.Release() does not remove playinglistTrack from Playinglist.ToPlayTracks
        DC.Data.Playinglists[Playlist].Remove(playinglistTrack);
      }
      Playlist.RemoveFromPlaylistTracks(this);
      Playlist = null!;
      Track.RemoveFromPlaylistTracks(this);
      Track = null!;
    }


    /// <summary>
    /// Called after 'new PlaylistTrack()' transaction is rolled back
    /// </summary>
    //partial void onRollbackItemNew() {
    //}


    /// <summary>
    /// Called after PlaylistTrack.Store() transaction is rolled back
    /// </summary>
    //partial void onRollbackItemStored() {
    //}


    /// <summary>
    /// Called after PlaylistTrack.Update() transaction is rolled back
    /// </summary>
    //partial void onRollbackItemUpdated(PlaylistTrack oldPlaylistTrack) {
    //}


    /// <summary>
    /// Called after PlaylistTrack.Release() transaction is rolled back
    /// </summary>
    //partial void onRollbackItemRelease() {
    //}


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
