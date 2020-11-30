//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Text;

//namespace MusicPlayer {
  
  
//  public partial class Track {


//    public Track(FileInfo fileInfo) {
//      FileName = fileInfo.Name[..^".mp3".Length];
//      FullFileName = fileInfo.FullName;
//      var fileProperties = TagLib.File.Create(FullFileName);
//      var tag = fileProperties.Tag;
//      Title = string.IsNullOrEmpty(tag.Title) ? null : tag.Title;
//      Duration = fileProperties.Properties.Duration.Ticks==0 ? null : fileProperties.Properties.Duration;
//      Album = string.IsNullOrEmpty(tag.Album) ? null : tag.Album;
//      Artists = string.IsNullOrEmpty(tag.JoinedPerformers) ? null : tag.JoinedPerformers;
//      Composers = string.IsNullOrEmpty(tag.JoinedComposers) ? null : tag.JoinedComposers;
//      Genres = string.IsNullOrEmpty(tag.JoinedGenres) ? null : tag.JoinedGenres;
//      Publisher = string.IsNullOrEmpty(tag.Publisher) ? null : tag.Publisher;
//      Year = tag.Year==0 ? null : (int)tag.Year;
//    }


//    public string ToStringShort() {
//      return
//        $"FileName: {FileName};" +
//        (Title is null ? null : $"Title: {Title};") +
//        (Duration?.Ticks is null ? null : $"Duration: {(int)Duration.Value.TotalMinutes}:{Duration.Value.Seconds};") +
//        (Album is null ? null : $"Album: {Album};") +
//        (Artists is null ? null : $"Artists: {Artists};") +
//        (Composers is null ? null : $"Composers: {Composers};") +
//        (Genres is null ? null : $"Genres: {Genres};") +
//        (Publisher is null ? null : $"Publisher: {Publisher};") +
//        (Year>0 ? null : $"Year: {Year};");
//    }
//  }
//}
