using Microsoft.VisualStudio.TestTools.UnitTesting;
using MusicPlayer;
using StorageLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace MusicPlayerCoreTest {


  [TestClass]
  public class PlayingListTest {


    [TestMethod]
    public void TestPlayingList() {
      try {
        var directoryInfo = new DirectoryInfo("TestCsv");
        if (directoryInfo.Exists) {
          directoryInfo.Delete(recursive: true);
          directoryInfo.Refresh();
        }

        directoryInfo.Create();

        var csvConfig = new CsvConfig(directoryInfo.FullName, reportException: reportException);
        _ = new DC(csvConfig);
        var random = new Random();

        //test Playinglist with Playlist
        var playlist = new Playlist("Name");
        var location = new Location("Path", "Name");
        var expectedTrackKeys = new HashSet<int>();
        var expectedAllTrackKeys = new HashSet<int>();
        var trackNo = 0;
        createTrack(playlist, location, trackNo++, expectedAllTrackKeys);
        createTrack(playlist, location, trackNo++, expectedAllTrackKeys);
        createTrack(playlist, location, trackNo++, expectedAllTrackKeys);
        refill(expectedAllTrackKeys, expectedTrackKeys);
        var playinglist1 = DC.Data.AddPlayinglist(playlist);
        assert(expectedTrackKeys);
        DC.DisposeData();

        var iterations = expectedTrackKeys.Count*2;
        Playinglist playinglist;
        for (int i = 0; i < iterations; i++) {
          if (expectedTrackKeys.Count==0) {
            refill(expectedAllTrackKeys, expectedTrackKeys);
          }
          _ = new DC(csvConfig);
          assert(expectedTrackKeys);
          playlist = DC.Data.Playlists[playlist.Key];
          playinglist = DC.Data.Playinglists[playlist];
          var trackKey = playinglist.GetNext(random)!.PlaylistTracks[0].Track.Key;
          expectedTrackKeys.Remove(trackKey);
          DC.DisposeData();
        }

        //test Playinglist with some tracks
        _ = new DC(csvConfig);
        refill(expectedAllTrackKeys, expectedTrackKeys);
        playinglist = DC.Data.AddPlayinglist(DC.Data.Tracks);

        iterations = expectedTrackKeys.Count*2;
        for (int i = 0; i < iterations; i++) {
          if (expectedTrackKeys.Count==0) {
            refill(expectedAllTrackKeys, expectedTrackKeys);
          }
          assert(expectedTrackKeys, playinglist);
          var trackKey = playinglist.GetNext(random)!.PlaylistTracks[0].Track.Key;
          expectedTrackKeys.Remove(trackKey);
          DC.DisposeData();
        }

        Assert.IsTrue(true);//Just for setting a breakpoint

      } finally {
        DC.DisposeData();
      }
    }


    private void refill(HashSet<int> expectedAllPlaylistTrackKeys, HashSet<int> expectedPlaylistTrackKeys) {
      foreach (var playlistTrackKey in expectedAllPlaylistTrackKeys) {
        expectedPlaylistTrackKeys.Add(playlistTrackKey);
      }
    }


    private static void createTrack(Playlist playlist, Location location, int trackNo, HashSet<int> expectedPlaylistTrackKeys) {
      var track = new Track("Filename" + trackNo, "fullFileName" + trackNo, location, null, null, null, null, null, null, null, null, null, null, null, null, null, "titleArtists" + trackNo);
      var playlistTrack = new PlaylistTrack(playlist, track, trackNo);
      expectedPlaylistTrackKeys.Add(track.Key);
    }


    private static void assert(HashSet<int> expectedPlaylistTrackKeys) {
      Assert.AreEqual(expectedPlaylistTrackKeys.Count, DC.Data.PlayinglistTracks.Count);
      foreach (var playinglistTrack in DC.Data.PlayinglistTracks) {
        Assert.IsTrue(expectedPlaylistTrackKeys.Contains(playinglistTrack.PlaylistTrack!.Track.Key));
      }
    }


    private static void assert(HashSet<int> expectedPlaylistTrackKeys, Playinglist playinglist) {
      Assert.AreEqual(expectedPlaylistTrackKeys.Count, playinglist.ToPlayTracks.Count);
      foreach (var playinglistItem in playinglist.ToPlayTracks) {
        if (playinglistItem is PlayinglistItemTrack playinglistItemTrack) {
          Assert.IsTrue(expectedPlaylistTrackKeys.Contains(playinglistItemTrack.Track.Key));
        }
      }
    }


    private void reportException(Exception ex) {
      Console.WriteLine(ex.ToString());
      System.Diagnostics.Debugger.Break();
      Assert.Fail();
    }
  }
}
