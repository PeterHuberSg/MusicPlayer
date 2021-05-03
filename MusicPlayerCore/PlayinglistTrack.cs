using System;
using System.Collections.Generic;
using StorageLib;


namespace MusicPlayer  {


  public partial class PlayinglistTrack: IStorageItem<PlayinglistTrack> {


    #region Properties
    //      ----------

    public PlaylistTrack? PlaylistTrack { get; private set; }
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
      //PlaylistTrack = PlaylistTrackKey==int.MinValue ? PlaylistTrack.NoPlaylistTrack : DC.Data.PlaylistTracks[PlaylistTrackKey];
      //if (!DC.Data.Playinglists.TryGetValue(PlaylistTrack.Playlist, out Playinglist? playingList)) {
      //  playingList = new Playinglist(PlaylistTrack.Playlist);
      //}
      //playingList.Add(this);
      if (PlaylistTrackKey!=int.MinValue) {
        updatePlaylistTrack();
      }
    }


    private void updatePlaylistTrack() {
      var playlistTrack = DC.Data.PlaylistTracks.GetItem(PlaylistTrackKey);
      if (playlistTrack is null) {
        System.Diagnostics.Debugger.Break();
        Release();
        //todo: add some error tracing
      } else {
        PlaylistTrack = playlistTrack;
      }
    }


    /// <summary>
    /// Called once the cloning constructor has filled all the properties. Clones have no children data.
    /// </summary>
    partial void onCloned(PlayinglistTrack clone) {
      clone.PlaylistTrack = PlaylistTrack;
    }


    /// <summary>
    /// Called once the CSV-constructor who reads the data from a CSV file has filled all the properties
    /// </summary>
    partial void onCsvConstruct() {
      updatePlaylistTrack();
    }
    #endregion


    #region Methods
    //      -------

    /// <summary>
    /// Called before {ClassName}.Store() gets executed
    /// </summary>
    //partial void onStoring(ref bool isCancelled) {
    //}


    /// <summary>
    /// Called after PlayingListTrack.Store() is executed
    /// </summary>
    //partial void onStored() {
    //}


    /// <summary>
    /// Called before PlayingListTrack gets written to a CSV file
    /// </summary>
    //partial void onCsvWrite() {
    //}


    /// <summary>
    /// Called before any property of PlayingListTrack is updated and before the HasChanged event gets raised
    /// </summary>
    //partial void onUpdating(int playlistTrackNo, ref bool isCancelled){
    //}


    /// <summary>
    /// Called after all properties of PlayingListTrack are updated, but before the HasChanged event gets raised
    /// </summary>
    //partial void onUpdated(PlayingListTrack old) {
    //}


    /// <summary>
    /// Called after an update for PlayingListTrack is read from a CSV file
    /// </summary>
    //partial void onCsvUpdate() {
    //}


    /// <summary>
    /// Called after PlayingListTrack.Release() got executed
    /// </summary>
    //partial void onReleased() {
    //}


    /// <summary>
    /// Called after 'new PlayingListTrack()' transaction is rolled back
    /// </summary>
    //partial void onRollbackItemNew() {
    //}


    /// <summary>
    /// Called after PlayingListTrack.Store() transaction is rolled back
    /// </summary>
    //partial void onRollbackItemStored() {
    //}


    /// <summary>
    /// Called after PlayingListTrack.Update() transaction is rolled back
    /// </summary>
    //partial void onRollbackItemUpdated(PlayingListTrack oldPlayingListTrack) {
    //}


    /// <summary>
    /// Called after PlayingListTrack.Release() transaction is rolled back
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
