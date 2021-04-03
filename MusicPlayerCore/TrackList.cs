//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Text;


//namespace MusicPlayer {
  
  
//  public class TrackList {

//    public readonly string Directory;

//    public IReadOnlyList<Track> Tracks => tracks;
//    readonly List<Track> tracks;


//    public IReadOnlyList<Track> PlayList => playList;
//    readonly List<Track> playList;


//    public TrackList(string directory) {
//      tracks = new List<Track>();
//      playList = new List<Track>();
//      Directory = directory;
//      var tracksDirectory = new DirectoryInfo(directory);
//      foreach (var file in tracksDirectory.GetFiles("*.mp3")) {
//        //tracks.Add(new Track(file));
//      }

//      var random = new Random();
//      var tracksCopy = new List<Track>(tracks);
//      while (tracksCopy.Count>0) {
//        var tracksCopyIndex = random.Next(tracksCopy.Count);
//        playList.Add(tracksCopy[tracksCopyIndex]);
//        tracksCopy.RemoveAt(tracksCopyIndex);
//      }
//    }
//  }
//}
