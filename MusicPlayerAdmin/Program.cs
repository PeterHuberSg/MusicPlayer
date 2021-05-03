using MusicPlayer;
using StorageLib;
using System;
using System.IO;
using System.Linq;

namespace MusicPlayerAdmin {
  class Program {
    static void Main(string[] args) {
      Console.WriteLine("Music Player Adming");
      Console.WriteLine("===================");

      var oldGenre = "Hard Rock";
      var newGenre = "Rock";
      var csvConfig = new CsvConfig(DC.CsvFilePath, DC.BackupFilePath, backupPeriodicity: 1, backupCopies: 8);
      using (var dc = new DC(csvConfig)) {
        var tracksQuery = 
          from track in dc.Tracks.Values
          where track.Genres==oldGenre
          select track;

        foreach (var track in tracksQuery) {
          Console.WriteLine($"{track.Title} | {track.Artists} | {track.Genres} | {track.Year} ");
        }

        Console.WriteLine();
        Console.WriteLine($"Press 'y' to rename \"{oldGenre}\" to \"{newGenre}\".");
        if (Console.ReadKey(true).Key==ConsoleKey.Y) {
          var count = 0;
          foreach (var track in tracksQuery) {
            FileInfo fileInfo = new FileInfo(track!.FullFileName);
            if (fileInfo.IsReadOnly) {
              fileInfo.IsReadOnly = false;
            }
            var fileProperties = TagLib.File.Create(fileInfo.FullName);
            fileProperties.Tag.Genres = new string[]{newGenre};
            fileProperties.Save();

            track.Update(
              track.Title,
              track.Album,
              track.AlbumTrack,
              track.Artists,
              track.Composers,
              track.Publisher,
              track.Year,
              newGenre,
              track.Weight,
              track.Volume,
              track.SkipStart,
              track.SkipEnd,
              track.TitleArtists);
            count++;
          }

          Console.WriteLine($"{count} renames completed");
        }
      }

    }


  }
}
