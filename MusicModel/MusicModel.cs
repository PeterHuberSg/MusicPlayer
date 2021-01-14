#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
using StorageLib;
using System.Collections.Generic;

namespace MusicPlayer {


  public class Track {
    public readonly string FileName;
    public readonly string FullFileName;
    public readonly Location Location;
    public string? Title;
    public readonly Time? Duration;
    public string? Album;
    public int? AlbumTrack;
    public string? Artists;
    public string? Composers;
    public string? Publisher;
    public int? Year;
    public string? Genres;
    public int? Weight;
    public int? Volume;
    public int? SkipStart;
    public int? SkipEnd;

    [StorageProperty(needsDictionary: true)]
    public string TitleArtists;

    public List<PlaylistTrack> Playlists;
  }


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

    public List<PlaylistTrack> Tracks;
  }


  public class PlaylistTrack {
    public Playlist Playlist;
    public Track Track;
    public int TrackNo;
  }

}
