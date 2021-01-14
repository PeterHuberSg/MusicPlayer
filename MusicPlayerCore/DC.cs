using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicPlayer {


  public partial class DC {
    #region Properties
    //      -----------

    //real
    public const string CsvFilePath = @"C:\Users\Peter\OneDrive\OneDriveData\MusicPlayer";
    public const string BackupFilePath = @"E:\MusicPlayerBackup";

    //test
    public const string CsvTestFilePath = @"E:\MusicPlayerCsvTest";
    public const string? BackupTestFilePath = null;


    public IList<string> PlaylistStrings => playlistStrings;
    private readonly List<string> playlistStrings = new();

    public IList<string> LocationStrings => locationStrings;
    private readonly List<string> locationStrings = new();

    public record AlbumArtistAlbum (string AlbumArtist, string Album);

    public IList<AlbumArtistAlbum> Albums => albums;
    private readonly List<AlbumArtistAlbum> albums = new();

    public IList<string> Artists => artists;
    private readonly List<string> artists = new();

    public IList<string> Genres => genres;
    private readonly List<string> genres = new();

    public IList<string> Years => years;
    private readonly List<string> years = new();

    public TimeSpan TotalDuration => totalDuration;
    private TimeSpan totalDuration;
    #endregion


    #region Methods
    //      -------

    partial void onConstructed() {
      UpdatePlayListStrings();
      UpdateTracksStats();
    }


    public void UpdatePlayListStrings() {
      playlistStrings.Clear();
      playlistStrings.Add("");
      foreach (var playlist in Playlists.Values.Where(pl => pl.Key>=0).OrderBy(pl => pl.Name)) {
        playlistStrings.Add(playlist.Name);
      }
    }


    public void UpdateTracksStats() {
      locationStrings.Clear();
      locationStrings.Add("");
      foreach (var location in Locations.Values.OrderBy(l => l.Name)) {
        locationStrings.Add(location.Name);
      }

      GetTracksStats(ref totalDuration, null, albums, artists, genres, years, Tracks);
    }


    public static void GetTracksStats(
      ref TimeSpan totalDuration,
      List<string>? locations,
      List<AlbumArtistAlbum> albums, 
      List<string> artists,
      List<string> genres,
      List<string> years,
      IEnumerable<Track> trackEnumerable) 
    {
      var locationsSortedSet = new SortedSet<string> { "" };
      var albumsDictionary = new SortedDictionary<string, (int count, SortedDictionary<string, string> artists)>();
      var artistsDictionary = new SortedDictionary<string, int>();
      var genresSortedSet = new SortedSet<string>{""};
      var yearsDictionary = new SortedDictionary<int, int>();
      totalDuration = TimeSpan.Zero;
      foreach (var track in trackEnumerable) {
        if (track.Duration is not null) {
          totalDuration += track.Duration.Value;
        }

        if (locations is not null) {
          if (track.Location is not null) {
            locationsSortedSet.Add(track.Location.Name);
          }
        }

        //collect album and the album's artists
        if (track.Album is not null) {
          //there is an album name
          if (albumsDictionary.TryGetValue(track.Album, out var albumCountArtists)) {
            //the album was already found for another track
            albumsDictionary[track.Album] = (albumCountArtists.count+1, albumCountArtists.artists);
            if (track.ArtistsStrings is not null) {
              //track has some artists for this album
              var isFirstTrack = false;
              if (albumCountArtists.artists is null) {
                //album was found before for a different track, but has no artists so far
                isFirstTrack = true;
                albumCountArtists.artists = new();
              }
              foreach (var trackArtistsString in track.ArtistsStrings) {
                if (!isFirstTrack && albumCountArtists.artists.ContainsKey(trackArtistsString.Key)) continue;

                albumCountArtists.artists[trackArtistsString.Key] = trackArtistsString.Value;
              }
            }

          } else {
            //there is no imformation stored yet for that album
            var albumArtistsStrings = new SortedDictionary<string, string>();
            if (track.ArtistsStrings is not null) {
              foreach (var trackArtistsString in track.ArtistsStrings) {
                albumArtistsStrings[trackArtistsString.Key] = trackArtistsString.Value;
              }
            }
            albumsDictionary[track.Album] = (1, albumArtistsStrings);
          }
        }

        if (track.Artists is not null) {
          var singleArtists = track.Artists.Split(';', StringSplitOptions.RemoveEmptyEntries);
          foreach (var singleArtist in singleArtists) {
            var singleArtistTrimmed = singleArtist.Trim();
            artistsDictionary[singleArtistTrimmed] =artistsDictionary.TryGetValue(singleArtistTrimmed, out var artistCount) ? ++artistCount : 1;
          }
        }

        if (track.Genres is not null) {
          genresSortedSet.Add(track.Genres);
        }

        if (track.Year is not null) {
          yearsDictionary[track.Year.Value] =yearsDictionary.TryGetValue(track.Year.Value, out var yearCount) ? ++yearCount : 1;
        }
      }
      totalDuration = TimeSpan.FromMinutes((int)totalDuration.TotalMinutes);

      if (locations is not null) {
        locations.Clear();
        // already done: locations.Add("");
        foreach (var location in locationsSortedSet) {
          locations.Add(location);
        }
      }

      albums.Clear();
      albums.Add(new AlbumArtistAlbum("", ""));
      foreach (var albumKeyValue in albumsDictionary) {
        (var count, var artistStrings) = albumKeyValue.Value;
        if (count>6) {
          //album has enough tracks to be shown
          if (artistStrings.Count==0) {
            albums.Add(new AlbumArtistAlbum(albumKeyValue.Key, albumKeyValue.Key));
          } else {
            if (artistStrings.Count>4) {
              albums.Add(new AlbumArtistAlbum(albumKeyValue.Key + " | various artists", albumKeyValue.Key));
            } else {
              var albumString = albumKeyValue.Key;
              foreach (var artist in artistStrings) {
                if (!albumString.Contains(artist.Value)) {
                  albumString += " | " + artist.Value;
                }
              }
              albums.Add(new AlbumArtistAlbum(albumString, albumKeyValue.Key));
            }
          }
        }
      }

      artists.Clear();
      artists.Add("");
      foreach (var artistKeyValue in artistsDictionary) {
        if (artistKeyValue.Value>6) {
          artists.Add(artistKeyValue.Key);
        }
      }

      genres.Clear();
      // already done: genres.Add("");
      foreach (var genre in genresSortedSet) {
        genres.Add(genre);
      }
 
      years.Clear();
      years.Add("");
      foreach (var yearKeyValue in yearsDictionary) {
        if (yearKeyValue.Value>3) {
          years.Add(yearKeyValue.Key.ToString());
        }
      }
   }
    #endregion
  }
}
