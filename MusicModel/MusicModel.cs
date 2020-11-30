#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
using Storage;


namespace MusicPlayer {


  //[StorageClass(areInstancesUpdatable: true, areInstancesDeletable: false)]
  //public class Genre {
  //  [StorageProperty(needsDictionary: true)]
  //  public string Name;
  //}


  [StorageClass(areInstancesUpdatable: true, areInstancesDeletable: true)]
  public class Track {
    public readonly string FileName;
    public readonly string FullFileName;
    public string? Title;
    public readonly Time? Duration;
    public string? Album;
    public int? AlbumTrack;
    public string? Artists;
    public string? Composers;
    public string? Genres;
    public string? Publisher;
    public int? Year;
    public int? Weight;
    public int? Volume;
    public int? SkipStart;
    public int? SkipEnd;

    [StorageProperty(needsDictionary: true)]
    public string TitleArtists;
  }
}
