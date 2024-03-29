﻿#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
using StorageLib;
using System.Collections.Generic;


namespace MusicPlayer {


  public class Track {
    public readonly string FileName;
    public readonly string FullFileName;
    public readonly Location Location;
    public string? Title;
    [StorageProperty(toLower: "Title")]
    public string? TitleLowerCase;
    public readonly Time? Duration;
    public string? Album;
    [StorageProperty(toLower: "Album")]
    public string? AlbumLowerCase;
    public int? AlbumTrack;
    public string? Artists;
    [StorageProperty(toLower: "Artists")]
    public string? ArtistsLowerCase;
    public string? Composers;
    [StorageProperty(toLower: "Composers")]
    public string? ComposersLowerCase;
    public string? Publisher;
    [StorageProperty(toLower: "Publisher")]
    public string? PublisherLowerCase;
    public int? Year;
    public string? Genres;
    public int? Weight;
    public int? Volume;
    public int? SkipStart;
    public int? SkipEnd;

    [StorageProperty(needsDictionary: true)]
    public string TitleArtists;

    public List<PlaylistTrack> PlaylistTracks;
  }

  [StorageClass]
  public class Location {
    public string Path;
    /// <summary>
    /// Lower case version of Path
    /// </summary>
    [StorageProperty(toLower: "Path", needsDictionary: true)]
    public string PathLower;
    public string Name;

    public List<Track> Tracks;
  }


  public class Playlist {
    public string Name;
    [StorageProperty(toLower: "Name", needsDictionary: true)]
    public string NameLower;

    public List<PlaylistTrack> PlaylistTracks;
  }


  /// <summary>
  /// User generated list of tracks which should be played together
  /// </summary>
  public class PlaylistTrack {
    public Playlist Playlist;
    public Track Track;
    public int TrackNo;
  }


  /// <summary>
  /// List tracking per Playlist which tracks have not been played yet
  /// </summary>
  public class PlayinglistTrack {
    [StorageProperty(needsDictionary: true)]
    public int PlaylistTrackKey;
  }
}
