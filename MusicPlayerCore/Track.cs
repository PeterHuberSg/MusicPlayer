using System;
using System.Collections.Generic;
using System.IO;
using Storage;


namespace MusicPlayer  {


  public partial class Track: IStorageItemGeneric<Track> {


    #region Properties
    //      ----------

    #endregion


    #region Events
    //      ------

    #endregion


    #region Constructors
    //      ------------

    public Track(FileInfo fileInfo, bool isStoring = true) {
      Key = StorageExtensions.NoKey;
      FileName = fileInfo.Name[..^".mp3".Length];
      FullFileName = fileInfo.FullName;
      var fileProperties = TagLib.File.Create(FullFileName);
      var tag = fileProperties.Tag;
      Title = string.IsNullOrEmpty(tag.Title) ? null : tag.Title;
      Duration = fileProperties.Properties.Duration.Ticks==0 ? null : fileProperties.Properties.Duration;
      Album = string.IsNullOrEmpty(tag.Album) ? null : tag.Album;
      AlbumTrack = tag.Track>0 ? (int)tag.Track : null;
      //
      Artists = string.IsNullOrEmpty(tag.JoinedPerformers) ? null : tag.JoinedPerformers;
      Composers = string.IsNullOrEmpty(tag.JoinedComposers) ? null : tag.JoinedComposers;
      Genres = string.IsNullOrEmpty(tag.JoinedGenres) ? null : tag.JoinedGenres;
      Publisher = string.IsNullOrEmpty(tag.Publisher) ? null : tag.Publisher;
      Year = tag.Year==0 ? null : (int)tag.Year;
      Weight = null;
      Volume = null;
      SkipStart = null;
      SkipEnd = null;
      TitleArtists = Title?.ToLowerInvariant().Trim() + "|" + Artists?.ToLowerInvariant().Trim();

      onConstruct();
      if (DC.Data.IsTransaction) {
        DC.Data.AddTransaction(new TransactionItem(0, TransactionActivityEnum.New, Key, this));
      }

      if (isStoring) {
        Store();
      }
    }


    /// <summary>
    /// Called once the constructor has filled all the properties
    /// </summary>
    //partial void onConstruct() {
    //}


    /// <summary>
    /// Called once the cloning constructor has filled all the properties. Clones have no children data.
    /// </summary>
    //partial void onCloned(Track clone) {
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
    /// Called after Track.Store() is executed
    /// </summary>
    //partial void onStored() {
    //}


    /// <summary>
    /// Called before Track gets written to a CSV file
    /// </summary>
    //partial void onCsvWrite() {
    //}


    /// <summary>
    /// Called after all properties of Track are updated, but before the HasChanged event gets raised
    /// </summary>
    //partial void onUpdating(
    //string? title, 
    //string? album, 
    //string? artists, 
    //string? composers, 
    //string? genres, 
    //string? publisher, 
    //int? year, 
    //int? weight, 
    //int? volume, 
    //int? skipStart, 
    //int? skipEnd, 
    //string titleArtists, 
    //ref bool isCancelled)
    //{
    //}


    /// <summary>
    /// Called after all properties of Track are updated, but before the HasChanged event gets raised
    /// </summary>
    //partial void onUpdated(Track old) {
    //}


    /// <summary>
    /// Called after an update for Track is read from a CSV file
    /// </summary>
    //partial void onCsvUpdate() {
    //}


    /// <summary>
    /// Called after Track.Release() got executed
    /// </summary>
    //partial void onReleased() {
    //}


    /// <summary>
    /// Called after 'new Track()' transaction is rolled back
    /// </summary>
    //partial void onRollbackItemNew() {
    //}


    /// <summary>
    /// Called after Track.Store() transaction is rolled back
    /// </summary>
    //partial void onRollbackItemStored() {
    //}


    /// <summary>
    /// Called after Track.Update() transaction is rolled back
    /// </summary>
    //partial void onRollbackItemUpdated(Track oldTrack) {
    //}


    /// <summary>
    /// Called after Track.Release() transaction is rolled back
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
