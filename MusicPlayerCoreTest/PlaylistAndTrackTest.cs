using Microsoft.VisualStudio.TestTools.UnitTesting;
using MusicPlayer;
using StorageLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MusicPlayerCoreTest {


  [TestClass]
  public class PlaylistAndTrackTest {


    #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
    CsvConfig csvConfig;
    Location location;
    int generation = 0;
    string generationString = "0";
    char playListNo = 'A';
    char trackNo = 'A';


#pragma warning restore CS8618


    private void increaseGeneration() {
      generation++;
      generationString = generation.ToString();
      playListNo = 'A';
      trackNo = 'A';
    }


    [TestMethod]
    public void TestPlayingList() {
      try {
        var directoryInfo = new DirectoryInfo("TestCsv");
        if (directoryInfo.Exists) {
          directoryInfo.Delete(recursive: true);
          directoryInfo.Refresh();
        }

        directoryInfo.Create();

        csvConfig = new CsvConfig(directoryInfo.FullName, reportException: reportException);
        _ = new DC(csvConfig);
        var location = new Location("SomePath", "SomeLocation");

        //create and delete playlist
        var playlistRecordA = createPlayList();
        release(playlistRecordA);
        increaseGeneration();

        //create and delete track
        var trackRecordA = createTrack();
        release(trackRecordA);
        increaseGeneration();

        //assign new track to new playlist, delete playlistTrack, track, playlist
        playlistRecordA = createPlayList();
        trackRecordA = createTrack();
        var playlistTrackRecordA = createPlaylistTrack(playlistRecordA, trackRecordA);
        release(playlistTrackRecordA);
        release(trackRecordA);
        release(playlistRecordA);
        increaseGeneration();

        //assign new track to new playlist, delete playlistTrack track, playlist
        playlistRecordA = createPlayList();
        trackRecordA = createTrack();
        playlistTrackRecordA = createPlaylistTrack(playlistRecordA, trackRecordA);
        release(trackRecordA);
        release(playlistRecordA);
        increaseGeneration();

        //assign new track to new playlist, delete playlistTrack playlist, track
        playlistRecordA = createPlayList();
        trackRecordA = createTrack();
        playlistTrackRecordA = createPlaylistTrack(playlistRecordA, trackRecordA);
        release(playlistRecordA);
        release(trackRecordA);
        increaseGeneration();

        //create new playlist, track, playlistTrack, playingList
        //delete playlist, track
        playlistRecordA = createPlayList();
        trackRecordA = createTrack();
        playlistTrackRecordA = createPlaylistTrack(playlistRecordA, trackRecordA);
        var playingListRecordA = createPlayingList(playlistRecordA);
        release(playlistRecordA);
        release(trackRecordA);
        increaseGeneration();

        //create new playlist, track, playlistTrack, playingList
        //delete track, playlist
        playlistRecordA = createPlayList();
        trackRecordA = createTrack();
        playlistTrackRecordA = createPlaylistTrack(playlistRecordA, trackRecordA);
        playingListRecordA = createPlayingList(playlistRecordA);
        release(trackRecordA);
        release(playlistRecordA);
        increaseGeneration();

        //create new playlist, track, playlistTrack, playingList
        //delete playlistTrack, track, playlist
        playlistRecordA = createPlayList();
        trackRecordA = createTrack();
        playlistTrackRecordA = createPlaylistTrack(playlistRecordA, trackRecordA);
        playingListRecordA = createPlayingList(playlistRecordA);
        release(playlistTrackRecordA);
        release(trackRecordA);
        release(playlistRecordA);
        increaseGeneration();

        //create new playlist with 3 tracks, 3 PlaylistTracks, playingList
        //play 1 track
        //delete first playlistTrack
        //delete second playlistTrack
        playlistRecordA = createPlayList();
        trackRecordA = createTrack();
        playlistTrackRecordA = createPlaylistTrack(playlistRecordA, trackRecordA);
        var trackRecordB = createTrack();
        var playlistTrackRecordB = createPlaylistTrack(playlistRecordA, trackRecordB);
        var trackRecordC = createTrack();
        var playlistTrackRecordC = createPlaylistTrack(playlistRecordA, trackRecordC);
        playingListRecordA = createPlayingList(playlistRecordA);
        playNextTrack(playingListRecordA, trackRecordA);
        release(playlistTrackRecordA);
        release(playlistTrackRecordB);

        //playlist contains only 1 record. Keep playing it
        playNextTrack(playingListRecordA, trackRecordC);
        playNextTrack(playingListRecordA, trackRecordC);

        //create second playinglist with same 3 tracks
        //delete 3. track
        var playlistRecordB = createPlayList();
        _ = createPlaylistTrack(playlistRecordB, trackRecordA);
        _ = createPlaylistTrack(playlistRecordB, trackRecordB);
        _ = createPlaylistTrack(playlistRecordB, trackRecordC);

        //Testing of playNextTrack from empty PlayingList cannot be tested, because disposeCreateDC() makes empty PlayingList
        //disapear
        //release(trackRecordC);
        //playNextTrack(playingListRecordA, null);


        //increaseGeneration();


        Assert.IsTrue(true);//Just for setting a breakpoint

      } finally {
        DC.DisposeData();
      }
    }


    private void disposeCreateDC() {
      assertData();
      DC.DisposeData();
      //removeReleasedExpectedRecords();
      var dc = new DC(csvConfig);
      location = dc.Locations[0];
      expectedPlayinglists.RemoveAll(pl => pl.PlayinglistTrackRecords.Count==0);
      assertData();
    }


    //private void removeReleasedExpectedRecords() {
    //  while (true) {
    //    var playlistRecord = expectedPlaylists.Where(pl => pl.Key<0).FirstOrDefault();
    //    if (playlistRecord is null) break;

    //    expectedPlaylists.Remove(playlistRecord);
    //  }
    //}


    #region PlaylistRecord

    record PlaylistRecord(string Name) {
      public int Key { get; set; }
      readonly public List<PlaylistTrackRecord> PlaylistTrackRecords = new();
      readonly public List<PlayinglistRecord> PlayinglistRecords = new();  //using List instead direct reference to acoid ToString() stack overflow
      public Playlist? Playlist => Key>=0 ? DC.Data.Playlists[Key] : null;
    }


    readonly List<PlaylistRecord> expectedPlaylists = new();


    private PlaylistRecord createPlayList() {
      var playlist = new Playlist($"Playlist{generationString}{playListNo++}");

      var playListRecord = new PlaylistRecord(playlist.Name) { Key = playlist.Key };
      expectedPlaylists.Add(playListRecord);
      disposeCreateDC();
      return playListRecord;
    }


    private void release(PlaylistRecord playlistRecord) {
      var playlist = DC.Data.Playlists[playlistRecord.Key];
      playlist.Release();

      playlistRecord.Key = -1;
      while (playlistRecord.PlaylistTrackRecords.Count>0) {
        remove(playlistRecord.PlaylistTrackRecords[^1]);
      }

      if (playlistRecord.PlayinglistRecords.Count==1) {
        remove(playlistRecord.PlayinglistRecords[0]);
        playlistRecord.PlayinglistRecords.Clear();
      } else if (playlistRecord.PlayinglistRecords.Count!=0) {
        Assert.Fail();
      }

      expectedPlaylists.Remove(playlistRecord);
      disposeCreateDC();
    }
    #endregion


    #region TrackRecord

    record TrackRecord(string Title) {
      public int Key { get; set; }
      readonly public List<PlaylistTrackRecord> PlaylistTrackRecords = new();
      public Track? Track => Key>=0 ? DC.Data.Tracks[Key] : null;
    }


    readonly List<TrackRecord> expectedTracks = new();


    private TrackRecord createTrack() {
      var id = generationString + trackNo++;
      var track = new Track("Filename" + id, "fullFileName" + id, location, "title" + id, null, null, null, null, null, 
        null, null, null, null, null, null, null, "titleArtists" + id);

      var trackRecord = new TrackRecord(track.Title!) { Key = track.Key };
      expectedTracks.Add(trackRecord);
      disposeCreateDC();
      return trackRecord;
    }


    private void release(TrackRecord trackRecord) {
      var track = DC.Data.Tracks[trackRecord.Key];
      track.Release();

      trackRecord.Key = -1;
      while (trackRecord.PlaylistTrackRecords.Count>0) {
        remove(trackRecord.PlaylistTrackRecords[^1]);
      }
      expectedTracks.Remove(trackRecord);
      disposeCreateDC();
    }
    #endregion


    #region PlaylistTrackRecord

    record PlaylistTrackRecord(PlaylistRecord PlaylistRecord, TrackRecord TrackRecord) {
      public int Key { get; set; }
      readonly public List<PlayinglistTrackRecord> PlayinglistTrackRecords = new(); //using List instead direct reference to acoid ToString() stack overflow
      public PlaylistTrack? PlaylistTrack => Key>=0 ? DC.Data.PlaylistTracks[Key] : null;
    }


    readonly List<PlaylistTrackRecord> expectedPlaylistTracks = new();


    private PlaylistTrackRecord createPlaylistTrack(PlaylistRecord playlistRecord, TrackRecord trackRecord) {
      var track = DC.Data.Tracks[trackRecord.Key];
      var playlist = DC.Data.Playlists[playlistRecord.Key];
      var playlistTrack = new PlaylistTrack(playlist, track, trackNo: playlist.PlaylistTracks.Count);
      
      var playlistTrackRecord = new PlaylistTrackRecord(playlistRecord, trackRecord) { Key = playlistTrack.Key };
      playlistRecord.PlaylistTrackRecords.Add(playlistTrackRecord!);
      trackRecord.PlaylistTrackRecords.Add(playlistTrackRecord);
      expectedPlaylistTracks.Add(playlistTrackRecord);
      disposeCreateDC();
      return playlistTrackRecord;
    }


    private void release(PlaylistTrackRecord playlistTrackRecord) {
      var playlistTrack = DC.Data.PlaylistTracks[playlistTrackRecord.Key];
      playlistTrack.Release();

      remove(playlistTrackRecord);
      disposeCreateDC();
    }


    private void remove(PlaylistTrackRecord playlistTrackRecord) {
      playlistTrackRecord.Key = -1;
      playlistTrackRecord.TrackRecord.PlaylistTrackRecords.Remove(playlistTrackRecord);
      playlistTrackRecord.PlaylistRecord.PlaylistTrackRecords.Remove(playlistTrackRecord);

      if (playlistTrackRecord.PlayinglistTrackRecords.Count==1) {
        var playinglistTrackRecord = playlistTrackRecord.PlayinglistTrackRecords[0];
        expectedPlayinglistTracks.Remove(playinglistTrackRecord);
        playlistTrackRecord.PlayinglistTrackRecords.Clear();
        var playinglistRecord = playinglistTrackRecord.PlayinglistRecord;
        playinglistRecord.PlayinglistTrackRecords.Remove(playinglistTrackRecord);
      } else if (playlistTrackRecord.PlayinglistTrackRecords.Count!=0) {
        Assert.Fail();
      }
      expectedPlaylistTracks.Remove(playlistTrackRecord);
    }
    #endregion


    #region PlayinglistRecord

    record PlayinglistRecord(PlaylistRecord PlaylistRecord) {
      readonly public List<PlayinglistTrackRecord> PlayinglistTrackRecords = new();
      public Playinglist? Playinglist => PlaylistRecord.Key>=0 ? DC.Data.Playinglists[DC.Data.Playlists[PlaylistRecord.Key]] : null;

      //public PlayinglistRecord(PlaylistRecord playlist) {
      //  Playlist = playlist;
      //  playlist.PlayinglistRecords.Add(this);
      //  foreach (var playlisttrackRecord in playlist.PlaylistTrackRecords) {
      //    PlayinglistTrackRecords.Add(playlisttrackRecord);
      //  }
      //}
    }


    readonly List<PlayinglistRecord> expectedPlayinglists = new();


    private PlayinglistRecord createPlayingList(PlaylistRecord playlistRecord) {
      var playlist = DC.Data.Playlists[playlistRecord.Key];
      var playinglist = new Playinglist(playlist);

      var playinglistRecord = new PlayinglistRecord(playlistRecord!);
      if (playlistRecord.PlayinglistRecords.Count!=0) Assert.Fail();

      playlistRecord.PlayinglistRecords.Add(playinglistRecord);
      fill(playinglistRecord);
      expectedPlayinglists.Add(playinglistRecord);
      disposeCreateDC();
      return playinglistRecord;
    }


    private void fill(PlayinglistRecord playinglistRecord) {
      var playinglist = playinglistRecord.Playinglist;
      foreach (var playinglistItem in playinglist!.ToPlayTracks) {
        var playlistTrack = ((PlayinglistItemPlaylistTrack)playinglistItem).PlaylistTrack;
        var playlistTrackRecord = expectedPlaylistTracks.Find(
          plt => plt.TrackRecord.Title==playlistTrack.Track.Title && plt.PlaylistRecord.Name==playlistTrack.Playlist.Name);
        var playinglistTrackRecord = new PlayinglistTrackRecord(playlistTrackRecord!, playinglistRecord) { 
          Key = DC.Data.PlayinglistTracksByPlaylistTrackKey[playlistTrack.Key].Key };
        expectedPlayinglistTracks.Add(playinglistTrackRecord);
        if (playlistTrackRecord!.PlayinglistTrackRecords.Count!=0) Assert.Fail();

        playlistTrackRecord.PlayinglistTrackRecords.Add(playinglistTrackRecord);
        playinglistRecord.PlayinglistTrackRecords.Add(playinglistTrackRecord);
      }
    }


    //private void release(PlayinglistRecord playinglistRecord) {
    //  var playlist = DC.Data.Playlists.Values.Select(pl=>pl).Where(pl=>pl.Name==playinglistRecord.PlaylistRecord.Name).First();
    //  var playinglist = DC.Data.Playinglists[playlist];
    //  playinglist.Release();

    //  remove();

    //  trackRecord.Key = -1;
    //  while (playinglistRecord.PlayinglistTrackRecords.Count>0) {
    //    remove(trackRecord.PlaylistTrackRecords[^1]);
    //  }
    //  expectedPlayinglists.Remove(playinglistRecord);
    //  disposeCreateDC();
    //}


    private void remove(PlayinglistRecord playinglistRecord) {
      foreach (var playinglistTrackRecord in playinglistRecord.PlayinglistTrackRecords) {
        expectedPlayinglistTracks.Remove(playinglistTrackRecord);
      }
      expectedPlayinglists.Remove(playinglistRecord);
    }
    #endregion


    #region PlayinglistTrackRecord

    record PlayinglistTrackRecord(PlaylistTrackRecord PlaylistTrackRecord, PlayinglistRecord PlayinglistRecord) {
      public int Key { get; set; }
      public PlayinglistTrack? PlayinglistTrack => Key>=0 ? DC.Data.PlayinglistTracks[Key] : null;
    }


    readonly List<PlayinglistTrackRecord> expectedPlayinglistTracks = new();
    #endregion


    private void playNextTrack(PlayinglistRecord playinglistRecord, TrackRecord? expectedTrackRecord) {
      var playlist = DC.Data.Playlists[playinglistRecord.PlaylistRecord.Key];
      var playinglist = DC.Data.Playinglists[playlist];
      var track = playinglist.GetNext(null);
      if (expectedTrackRecord is null) {
        Assert.IsNull(track);
      } else {
        Assert.AreEqual(expectedTrackRecord.Key, track!.Key);
        var playinglistTrackRecord = expectedPlayinglistTracks.Where(
          ptr => ptr.PlaylistTrackRecord.TrackRecord.Key==expectedTrackRecord.Key && 
          ptr.PlayinglistRecord==playinglistRecord).First();
        playinglistRecord.PlayinglistTrackRecords.Remove(playinglistTrackRecord);
        expectedPlayinglistTracks.Remove(playinglistTrackRecord);
        Assert.IsTrue(playinglistTrackRecord.PlaylistTrackRecord.PlayinglistTrackRecords.Remove(playinglistTrackRecord));
        Assert.AreEqual(0, playinglistTrackRecord.PlaylistTrackRecord.PlayinglistTrackRecords.Count);
        if (playinglistRecord.PlayinglistTrackRecords.Count==0) {
          fill(playinglistRecord);
        }
      }
      assertData();
    }


    void assertData() {
      Assert.AreEqual(expectedPlaylists.Count, DC.Data.Playlists.Count);
      foreach (var playlist in DC.Data.Playlists) {
        var expectedPlaylist = expectedPlaylists.Find(plr => plr.Name==playlist.Name);
        Assert.IsNotNull(expectedPlaylist);
        Assert.AreEqual(expectedPlaylist!.Key, playlist.Key);
        Assert.AreEqual(expectedPlaylist.PlaylistTrackRecords.Count, playlist.PlaylistTracks.Count);
        foreach (var playlistTrack in playlist.PlaylistTracks) {
          var expectedTrack = expectedPlaylist.PlaylistTrackRecords.Find(plt => plt.TrackRecord.Title==playlistTrack.Track.Title);
          Assert.IsNotNull(expectedTrack);
          Assert.AreEqual(expectedTrack!.Key, expectedTrack.Key);
        }
      }

      Assert.AreEqual(expectedTracks.Count, DC.Data.Tracks.Count);
      foreach (var track in DC.Data.Tracks) {
        var expectedTrack = expectedTracks.Find(t => t.Title==track.Title);
        Assert.IsNotNull(expectedTrack);
        Assert.AreEqual(expectedTrack!.Key, track.Key);
        Assert.AreEqual(expectedTrack.PlaylistTrackRecords.Count, track.PlaylistTracks.Count);
        foreach (var playlistTrack in track.PlaylistTracks) {
          var expectedPlaylist = expectedTrack.PlaylistTrackRecords.Find(plt => plt.PlaylistRecord.Name==playlistTrack.Playlist.Name);
          Assert.IsNotNull(expectedPlaylist);
          Assert.AreEqual(expectedPlaylist!.Key, expectedPlaylist.Key);
        }
      }

      Assert.AreEqual(expectedPlaylistTracks.Count, DC.Data.PlaylistTracks.Count);
      foreach (var playlistTrack in DC.Data.PlaylistTracks) {
        var expectedPlaylistTrack = expectedPlaylistTracks.Find(
          plt => plt.PlaylistRecord.Name==playlistTrack.Playlist.Name && plt.TrackRecord.Title==playlistTrack.Track.Title);
        Assert.IsNotNull(expectedPlaylistTrack);
        Assert.AreEqual(expectedPlaylistTrack!.Key, playlistTrack.Key);
      }

      Assert.AreEqual(expectedPlayinglists.Count, DC.Data.Playinglists.Count);
      foreach (var playinglist in DC.Data.Playinglists.Values) {
        var expectedPlayinglist = expectedPlayinglists.Find(pl => pl.PlaylistRecord.Name==playinglist.Playlist!.Name);
        Assert.IsNotNull(expectedPlayinglist);
        Assert.AreEqual(expectedPlayinglist!.PlayinglistTrackRecords.Count, playinglist.ToPlayTracks.Count);
        foreach (var playinglistItem in playinglist.ToPlayTracks) {
          var playinglistTrack = (PlayinglistItemPlaylistTrack)playinglistItem;
          var expectedPlayinglistTrack =
            expectedPlayinglist!.PlayinglistTrackRecords.Find(
              pt => pt.PlaylistTrackRecord.TrackRecord.Title==playinglistTrack.PlaylistTrack.Track.Title &&
              pt.PlaylistTrackRecord.PlaylistRecord.Name==playinglistTrack.PlaylistTrack.Playlist.Name);
          Assert.IsNotNull(expectedPlayinglistTrack);
        }
      }

      Assert.AreEqual(expectedPlayinglistTracks.Count, DC.Data.PlayinglistTracks.Count);
      foreach (var playinglistTrack in DC.Data.PlayinglistTracks) {
        var expectedPlayinglistTrack =expectedPlayinglistTracks.Find(pt => pt.Key==playinglistTrack.Key);
        Assert.IsNotNull(expectedPlayinglistTrack);
        Assert.AreEqual(expectedPlayinglistTrack!.Key, playinglistTrack.Key);
      }
    }


    private void reportException(Exception ex) {
      Console.WriteLine(ex.ToString());
      System.Diagnostics.Debugger.Break();
      Assert.Fail();
    }
  }
}
