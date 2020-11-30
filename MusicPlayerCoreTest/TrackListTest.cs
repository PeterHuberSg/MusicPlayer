using Microsoft.VisualStudio.TestTools.UnitTesting;
using MusicPlayer;
using System;
using System.Collections.Generic;
using System.Text;

namespace MusicPlayerCoreTest {


  [TestClass]
  public class TrackListTest {


    [TestMethod]
    public void TestTrackList() {
      var trackList = new TrackList(@"E:\Musig\Oldies");
    }
  }
}
