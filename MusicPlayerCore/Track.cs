using System;
using System.Collections.Generic;
using System.IO;
using StorageLib;


namespace MusicPlayer  {


  public partial class Track: IStorageItem<Track> {


    #region Properties
    //      ----------

    public IReadOnlyDictionary<string, string> ArtistsStrings => artistsStrings;
    readonly SortedDictionary<string, string> artistsStrings = new();

    private void updateArtists() {
      artistsStrings.Clear();
      if (Artists is not null) {
        foreach (var artistString in Artists.Split(';', StringSplitOptions.TrimEntries|StringSplitOptions.RemoveEmptyEntries)) {
          artistsStrings.Add(artistString.ToLowerInvariant(), artistString);
        }
      }
    }
    #endregion


    #region Events
    //      ------

    #endregion


    #region Constructors
    //      ------------

    public Track(FileInfo fileInfo, Location location, bool isStoring = true) {
      Key = StorageExtensions.NoKey;
      FileName = fileInfo.Name[..^".mp3".Length];
      FullFileName = fileInfo.FullName;
      Location = location;
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
      playlistTracks = new StorageList<PlaylistTrack>();
      Location.AddToTracks(this);

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
    partial void onConstruct() {
      updateArtists();
    }


    /// <summary>
    /// Called once the cloning constructor has filled all the properties. Clones have no children data.
    /// </summary>
    partial void onCloned(Track _) {
      updateArtists();
    }


    /// <summary>
    /// Called once the CSV-constructor who reads the data from a CSV file has filled all the properties
    /// </summary>
    partial void onCsvConstruct() {
      updateArtists();
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
    /// Called before any property of Track us updated and before the HasChanged event gets raised
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
    partial void onUpdated(Track _) {
      updateArtists();
    }


    /// <summary>
    /// Called after an update for Track is read from a CSV file
    /// </summary>
    partial void onCsvUpdate() {
      updateArtists();
    }


    ///// <summary>
    ///// Called after playlistTrack is removed from Playlists
    ///// </summary>
    //partial void onRemovedFromPlaylists(PlaylistTrack playlistTrack) {
    //}


    /// <summary>
    /// Called before Track.Release() gets executed
    /// </summary>
    partial void onReleasing() {
      while (PlaylistTracks.Count>0) {
        PlaylistTracks[^1].Release(); //this will release playlistsTrack here and also from Playinglist, should it be used there
      }
    }


    /// <summary>
    /// Called after Track.Release() got executed
    /// </summary>
    //partial void onReleased() {
    //}


    //public void ReleaseFully() {
    //  while (Playlists.Count>0) {
    //    Playlists[^1].Release(); //this will release playlistsTrack here and also from Playinglist, should it be used there
    //  }
    //  Release();
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
    partial void onRollbackItemUpdated(Track _) {
      updateArtists();
    }


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
