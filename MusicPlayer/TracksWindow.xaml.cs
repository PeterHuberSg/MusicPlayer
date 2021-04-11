using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfWindowsLib;

namespace MusicPlayer {


  /// <summary>
  /// Interaction logic for TracksWindow.xaml
  /// </summary>
  public partial class TracksWindow: Window {

    #region Constructor
    //      -----------

    public static TracksWindow Show(Window ownerWindow) {
      var window = new TracksWindow { Owner = ownerWindow };
      window.Show();
      return window;
    }


    readonly System.Windows.Data.CollectionViewSource tracksViewSource;
    List<TrackRow> trackRows;


    public TracksWindow() {
      InitializeComponent();

      Loaded += tracksWindow_Loaded;

      //filter
      FilterTextBox.TextChanged += filterTextBox_TextChanged;
      ArtistComboBox.SelectionChanged += filterComboBox_SelectionChanged;
      ArtistComboBox.ItemsSource = DC.Data.Artists;
      AlbumComboBox.SelectionChanged += filterComboBox_SelectionChanged;
      AlbumComboBox.ItemsSource = DC.Data.Albums;
      AlbumComboBox.DisplayMemberPath = "AlbumArtist";
      GenreComboBox.SelectionChanged += filterComboBox_SelectionChanged;
      GenreComboBox.ItemsSource = DC.Data.Genres;
      YearComboBox.SelectionChanged += yearComboBox_SelectionChanged;
      YearComboBox.ItemsSource = DC.Data.Years;
      LocationsComboBox.SelectionChanged += filterComboBox_SelectionChanged;
      LocationsComboBox.ItemsSource = DC.Data.LocationStrings;
      PlaylistsComboBox.SelectionChanged += filterComboBox_SelectionChanged;
      PlaylistsComboBox.ItemsSource = DC.Data.PlaylistStrings;
      DeleteCheckBox.Click += checkBox_Click;
      PlaylistCheckBox.Click += checkBox_Click;
      ClearButton.Click += clearButton_Click;
      DeleteAllButton.Click += deleteAllButton_Click;
      PLAllButton.Click += plAllButton_Click;
      UnselectAllButton.Click += unselectAllButton_Click;
      ExecuteDeleteButton.Click += executeDeleteButton_Click;
      AddToPlaylistComboBox.ItemsSource = DC.Data.PlaylistStrings;
      AddToPlaylistButton.Click += addToPlaylistButton_Click;
      RenameTrackButton.Click += renameTrackButton_Click;

      //datagrid
      trackRows = new();
      var trackNo = 0;
      foreach (var track in DC.Data.Tracks.Values.OrderBy(t=>t.Title)) {
        trackRows.Add(new TrackRow(ref trackNo, track, updateSelectedCountTextBox));
      }

      tracksViewSource = ((System.Windows.Data.CollectionViewSource)this.FindResource("TracksViewSource"));
      tracksViewSource.Source = trackRows;
      tracksViewSource.Filter += tracksViewSource_Filter;
      TracksDataGrid.MouseDoubleClick += tracksDataGrid_MouseDoubleClick;
      TracksDataGrid.KeyDown += tracksDataGrid_KeyDown;
      var contextMenu = new ContextMenu();
      var renameMenuItem = new MenuItem { Header = "Rename" };
      renameMenuItem.Click += renameMenuItem_Click;
      contextMenu.Items.Add(renameMenuItem);
      TracksDataGrid.ContextMenu = contextMenu;

      //TrackRow.HasIsDeletionChanged = updateSelectedCountTextBox;
      //TrackRow.HasIsAddPlaylistChanged = updateSelectedCountTextBox;
      updateSelectedCountTextBox();

      TrackPlayer.TrackChanged += trackPlayer_TrackChanged;
      TrackPlayer.Init(getPlayinglist);
      Closed += memberWindow_Closed;

      MainWindow.Register(this, "Tracks");
    }
    #endregion


    #region TrackRow Data
    //      -------------

    public class TrackRow: TrackGridRow{

      public TrackRow(ref int trackNo, Track track, Action? dataChanged) :base(ref trackNo, track, dataChanged) {

      }
    }
//    static int trackNo = 0;


    //    public class TrackRow: INotifyPropertyChanged {
    //      public int No { get; }
    //      public Track Track { get; }
    //      public string? Playlists { get; set; }
    //      /// <summary>
    //      /// Track is marked for deletion
    //      /// </summary>
    //      public bool IsDeletion {
    //        get {
    //          return isDeletion;
    //        }
    //        set {
    //          if (isDeletion!=value) {
    //            isDeletion = value;
    //            updatePlaylistCheckBox();
    //            HasIsDeletionChanged?.Invoke();
    //          }
    //        }
    //      }
    //      bool isDeletion;

    //      /// <summary>
    //      /// User selected this track for adding to playlist OR CheckBox is disabled and displays that the track is already in the playlist
    //      /// </summary>
    //      public bool IsAddPlaylist {
    //        get {
    //          return isAddPlaylist;
    //        }
    //        set {
    //          if (isAddPlaylist!=value) {
    //            isAddPlaylist = value;
    //            HasIsAddPlaylistChanged?.Invoke();
    //            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsAddPlaylist)));
    //          }
    //        }
    //      }
    //      bool isAddPlaylist;


    //      /// <summary>
    //      /// Playlist CheckBox is visible if a playlist name is entered and track is not marked for deletion
    //      /// </summary>
    //      public Visibility PlaylistCheckBoxVisibility {
    //        get {
    //          return playlistCheckBoxVisibility;
    //        }
    //        set {
    //          if (playlistCheckBoxVisibility!=value) {
    //            playlistCheckBoxVisibility = value;
    //            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlaylistCheckBoxVisibility)));
    //          }
    //        }
    //      }
    //      Visibility playlistCheckBoxVisibility;


    //      /// <summary>
    //      /// Playlist CheckBox is disabled when the track is already in the playlist
    //      /// </summary>
    //      public bool PlaylistCheckBoxIsEnabled {
    //        get {
    //          return playlistCheckBoxIsEnabled;
    //        }
    //        set {
    //          if (playlistCheckBoxIsEnabled!=value) {
    //            playlistCheckBoxIsEnabled = value;
    //            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlaylistCheckBoxIsEnabled)));
    //          }
    //        }
    //      }
    //      bool playlistCheckBoxIsEnabled;

    //      //todo: remove unuser (?) PropertyChanged event
    //      public event PropertyChangedEventHandler? PropertyChanged;//**--


    //#pragma warning disable CA2211 // Non-constant fields should not be visible
    //      public static Action? HasIsDeletionChanged;
    //      public static Action? HasIsAddPlaylistChanged;
    //#pragma warning restore CA2211


    //      public TrackRow(Track track) {
    //        No = trackNo++;
    //        Track = track;
    //        UpdatePlaylists();

    //        isDeletion = false;
    //      }


    //      public void UpdatePlaylists() {
    //        Playlists = null;
    //        var isFirst = true;
    //        foreach (var playlistTrack in Track.Playlists) {
    //          if (isFirst) {
    //            isFirst = false;
    //          } else {
    //            Playlists += '|';
    //          }
    //          Playlists += playlistTrack.Playlist.Name;
    //        }
    //      }


    //      bool hasPlaylistName;//user selected an existing or entered a new playlist name


    //      public void UpdatePlaylistCheckBox(Playlist? playlist, bool hasPlaylistName) {
    //        this.hasPlaylistName = hasPlaylistName;
    //        if (hasPlaylistName) {
    //          PlaylistCheckBoxIsEnabled = !Track.Playlists.Where(plt => plt.Playlist==playlist).Any();
    //        }
    //        updatePlaylistCheckBox();
    //      }


    //      private void updatePlaylistCheckBox() {
    //        if (hasPlaylistName) {
    //          if (PlaylistCheckBoxIsEnabled) {
    //            PlaylistCheckBoxVisibility = IsDeletion ? Visibility.Hidden : Visibility.Visible;
    //          } else {
    //            //track is already in playlist, show it as disabled and selected
    //            PlaylistCheckBoxVisibility = Visibility.Visible;
    //            IsAddPlaylist = true;
    //          }
    //        } else {
    //          PlaylistCheckBoxVisibility = Visibility.Hidden;
    //        }
    //      }
    //    }
    #endregion


    #region Events
    //      ------

    private void tracksWindow_Loaded(object sender, RoutedEventArgs e) {
      AddToPlaylistComboBox.SelectionChanged += addToPlaylistComboBox_SelectionChanged;
      AddToPlaylistComboBox.AddHandler(TextBoxBase.TextChangedEvent, new RoutedEventHandler(addToPlaylistComboBox_TextChanged));
      AddToPlaylistComboBox.LostFocus += addToPlaylistComboBox_LostFocus;
      updatePlaylistCheckBoxes();
    }


    #region Filter events
    //      -------------

    const int minFilterStringLength = 2;
    string filterText = "";


    private void filterTextBox_TextChanged(object sender, TextChangedEventArgs e) {
      if (isClearing) return;

      var textLength = FilterTextBox.Text.Length;
      if (textLength==0) {
        filterText = "";
        tracksViewSource.View.Refresh();
      } else if (textLength>minFilterStringLength) {
        filterText = FilterTextBox.Text.ToLowerInvariant();
        tracksViewSource.View.Refresh();
      }
    }


    private void filterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      if (isClearing) return;

      tracksViewSource.View.Refresh();
    }


    int filteredYear;


    private void yearComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      if (isClearing) return;

      var yearString = (string)YearComboBox.SelectedItem;
      filteredYear = yearString=="" ? 0 : int.Parse(yearString);
      tracksViewSource.View.Refresh();
    }


    private void checkBox_Click(object sender, RoutedEventArgs e) {
      if (isClearing) return;

      tracksViewSource.View.Refresh();
    }


    bool isClearing;


    void clearButton_Click(object sender, RoutedEventArgs e) {
      isClearing = true;
      FilterTextBox.Text = "";
      filterText = "";
      ArtistComboBox.SelectedIndex = 0;
      AlbumComboBox.SelectedIndex = 0;
      GenreComboBox.SelectedIndex = 0;
      YearComboBox.SelectedIndex = 0;
      LocationsComboBox.SelectedIndex = 0;
      PlaylistsComboBox.SelectedIndex = 0;
      isClearing = false;

      tracksViewSource.View.SortDescriptions.Clear();
      tracksViewSource.View.Refresh();
    }


    private void tracksViewSource_Filter(object sender, FilterEventArgs e) {
      var trackRow = (TrackRow)e.Item;
      var isAccepted = false;
      var isRefused = false;
      if (filterText.Length>minFilterStringLength) {
        if (trackRow.Track.Title?.ToLowerInvariant().Contains(filterText)??false) {
          isAccepted = true;
        } else if (trackRow.Track.Artists?.ToLowerInvariant().Contains(filterText)??false) {
          isAccepted = true;
        } else if (trackRow.Track.Album?.ToLowerInvariant().Contains(filterText)??false) {
          isAccepted = true;
        } else if (trackRow.Track.Composers?.ToLowerInvariant().Contains(filterText)??false) {
          isAccepted = true;
        } else if (trackRow.Track.Publisher?.ToLowerInvariant().Contains(filterText)??false) {
          isAccepted = true;
        } else {
          isRefused = true;
        }
      }

      if (ArtistComboBox.SelectedIndex>0) {
        if (trackRow.Track.Artists?.Contains((string)ArtistComboBox.SelectedItem)??false) {
          isAccepted = true;
        } else {
          isRefused = true;
        }
      }

      if (AlbumComboBox.SelectedIndex>0) {
        if (trackRow.Track.Album is not null && ((DC.AlbumArtistAlbum)AlbumComboBox.SelectedItem).Album==trackRow.Track.Album) {
          isAccepted = true;
        } else {
          isRefused = true;
        }
      }

      if (GenreComboBox.SelectedIndex>0) {
        if (trackRow.Track.Genres?.Contains((string)GenreComboBox.SelectedItem)??false) {
          isAccepted = true;
        } else {
          isRefused = true;
        }
      }

      if (YearComboBox.SelectedIndex>0) {
        if (trackRow.Track.Year==filteredYear) {
          isAccepted = true;
        } else {
          isRefused = true;
        }
      }

      if (LocationsComboBox.SelectedIndex>0) {
        if (trackRow.Track.Location.Name==(string)LocationsComboBox.SelectedItem) {
          isAccepted = true;
        } else {
          isRefused = true;
        }
      }

      if (PlaylistsComboBox.SelectedIndex>0) {
        if (trackRow.Playlists?.Contains((string)PlaylistsComboBox.SelectedItem)??false) {
          isAccepted = true;
        } else {
          isRefused = true;
        }
      }

      //if no filter is active (=!isRefused) OR any filter does accept the item, then accept, i.e. OR condition.
      isAccepted = !isRefused || isAccepted;

      //only accept item which match IsDeletion
      if (DeleteCheckBox.IsChecked is not null) {
        if (trackRow.IsDeletion!=DeleteCheckBox.IsChecked) {
          isAccepted = false;
        }
      }

      //only accept item which match IsAddPlaylist
      if (PlaylistCheckBox.IsChecked is not null) {
        if (trackRow.IsAddPlaylist!=PlaylistCheckBox.IsChecked) {
          isAccepted = false;
        }
      }

      e.Accepted = isAccepted;
    }
    #endregion


    #region Deletion and PlayList Checkboxes
    //      --------------------------------

    private void deleteAllButton_Click(object sender, RoutedEventArgs e) {
      foreach (var item in TracksDataGrid.Items) {
        var trackRow = (TrackRow)item;
        trackRow.IsDeletion = true;
      }
      tracksViewSource.View.Refresh();
    }


    private void plAllButton_Click(object sender, RoutedEventArgs e) {
      foreach (var item in TracksDataGrid.Items) {
        var trackRow = (TrackRow)item;
        if (trackRow.PlaylistCheckBoxIsEnabled) {
          trackRow.IsAddPlaylist = true;
        }
      }
    }


    private void unselectAllButton_Click(object sender, RoutedEventArgs e) {
      foreach (var item in TracksDataGrid.Items) {
        var trackRow = (TrackRow)item;
        trackRow.IsDeletion = false;
        if (trackRow.PlaylistCheckBoxIsEnabled) {
          trackRow.IsAddPlaylist = false;
        }
      }
      tracksViewSource.View.Refresh();
    }


    private void addToPlaylistComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      if (e.AddedItems.Count>0) {
        var addedItem = e.AddedItems[0];
        if (addedItem is not null) {
          updatePlaylist(addedItem.ToString());
          return;
        }
      }
      updatePlaylist(AddToPlaylistComboBox.Text);
    }


    private void addToPlaylistComboBox_LostFocus(object sender, RoutedEventArgs e) {
      updatePlaylist(AddToPlaylistComboBox.Text);
    }

    private void addToPlaylistComboBox_TextChanged(object sender, RoutedEventArgs e) {
      updatePlaylist(AddToPlaylistComboBox.Text);
    }


    bool hasPlaylistName;
    Playlist? playlist;


    private void updatePlaylist(string? playlistName) {
      var hasPlaylistNameNew = !string.IsNullOrEmpty(playlistName);
      AddToPlaylistButton.IsEnabled = PLAllButton.IsEnabled = hasPlaylistNameNew;
      Playlist? playlistNew;
      if (hasPlaylistNameNew) {
        DC.Data.PlaylistsByNameLower.TryGetValue(playlistName!.ToLowerInvariant(), out playlistNew);
      } else {
        playlistNew = null;
      }

      if (hasPlaylistName!=hasPlaylistNameNew || playlist!=playlistNew) {
        hasPlaylistName = hasPlaylistNameNew;
        playlist = playlistNew;
        updatePlaylistCheckBoxes();
      }
    }


    private void updatePlaylistCheckBoxes() {
      foreach (var item in TracksDataGrid.Items) {
        var trackRow = (TrackRow)item;
        trackRow.UpdatePlaylistCheckBox(playlist, hasPlaylistName);
      }
      updateSelectedCountTextBox();
    }


    int deletionCount;


    private void updateSelectedCountTextBox() {
      deletionCount = 0;
      var existPlaylistCount = 0;
      var addPlaylistCount = 0;
      foreach (var trackRow in trackRows) {
        if (trackRow.IsDeletion) {
          deletionCount++;
        } else {
          if (hasPlaylistName) {
            if (trackRow.PlaylistCheckBoxIsEnabled) {
              if (trackRow.IsAddPlaylist) {
                addPlaylistCount++;
              }
            } else {
              existPlaylistCount++;
            }
          }
        }
      }
      SelectedCountTextBox.Text = $"Del: {deletionCount}, PL exist: {existPlaylistCount}, new: {addPlaylistCount}";
    }


    private void executeDeleteButton_Click(object sender, RoutedEventArgs e) {
      var result = MessageBox.Show($"Do you want to delete {deletionCount} track(s) ?", "Deletion", MessageBoxButton.YesNo,
        MessageBoxImage.Question, MessageBoxResult.No);
      if (result==MessageBoxResult.Yes) {
        var remainingTrackRows = new List<TrackRow>(trackRows.Count);
        foreach (var trackRow in trackRows) {
          if (trackRow.IsDeletion) {
            foreach (var playlistsTrack in trackRow.Track.Playlists) {
              playlistsTrack.Release();
            }
            trackRow.Track.Release();
          } else {
            remainingTrackRows.Add(trackRow);
          }
        }
        tracksViewSource.Source = trackRows = remainingTrackRows;
      }
    }


    private void addToPlaylistButton_Click(object sender, RoutedEventArgs e) {
      var playlistName = AddToPlaylistComboBox.Text;
      if (string.IsNullOrEmpty(playlistName)) {
        MessageWindow.Show(this, "Provide name for playlist");
        return;
      }
      if (!DC.Data.PlaylistsByNameLower.TryGetValue(playlistName.ToLower(), out var playlist)) {
        playlist = new Playlist(playlistName);
      }
      var playlistTracks = new List<PlaylistTrack>();
      var trackNo = playlist.Tracks.Count;
      foreach (var item in TracksDataGrid.Items) {
        var trackRow = (TrackRow)item;
        if (trackRow.PlaylistCheckBoxIsEnabled &&  trackRow.IsAddPlaylist) {
          playlistTracks.Add(new PlaylistTrack(playlist, trackRow.Track, trackNo++));
        }
      }
      PlaylistWindow.Show(this, playlist, playlistTracks, refreshTrackDataGrid);
    }
    #endregion


    #region DataGrid events
    //      ---------------

    private void tracksDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
      //var selectedIndex = TracksDataGrid.SelectedIndex;
      //if (selectedIndex<0) return;

      //var dataGridCell = ((DependencyObject)e.OriginalSource).FindVisualParentOfType<DataGridCell>();
      //if (dataGridCell is null) return;

      //if (dataGridCell.Column.DisplayIndex==11) return;

      //TrackPlayer.Play(((TrackRow)TracksDataGrid.Items[selectedIndex]).Track);
      var playinglist = getPlayinglist();
      if (playinglist is not null) {
        TrackPlayer.Play(playinglist);
      }
    }


    private void tracksDataGrid_KeyDown(object sender, KeyEventArgs e) {
      if (e.SystemKey==Key.R) {
        renameSeletctedTrack();
        e.Handled = true;
      };
    }


    private void renameSeletctedTrack() {
      Track track = ((TrackRow)TracksDataGrid.SelectedItem).Track;
      TrackPlayer.StopTrackIfPlaying(track);
      TrackRenameWindow.Show(this, track, updateSelectedItem);
    }


    private void renameTrackButton_Click(object sender, RoutedEventArgs e) {
      renameSeletctedTrack();
    }


    private void renameMenuItem_Click(object sender, RoutedEventArgs e) {
      Track track = ((TrackRow)TracksDataGrid.SelectedItem).Track;
      TrackPlayer.StopTrackIfPlaying(track);
      TrackRenameWindow.Show(this, track, updateSelectedItem);
    }


    private void updateSelectedItem(Track track) {
      var selectedIndex = TracksDataGrid.SelectedIndex;
      tracksViewSource.View.Refresh();
      TracksDataGrid.SelectedIndex = selectedIndex;
    }


    private void trackPlayer_TrackChanged(Track? track) {
      if (track is null) return;

      for (int itemIndex = 0; itemIndex < TracksDataGrid.Items.Count; itemIndex++) {
        var trackRow = (TrackRow)TracksDataGrid.Items[itemIndex];
        if (trackRow.Track==track) {
          TracksDataGrid.SelectedIndex = itemIndex;
          TracksDataGrid.ScrollIntoView(trackRow);
          return;
        }
      }
    }


    private void refreshTrackDataGrid(Playlist playlist) {
      foreach (var item in TracksDataGrid.Items) {
        var trackRow = (TrackRow)item;
        trackRow.IsDeletion = false;
        trackRow.IsAddPlaylist = false;
        trackRow.UpdatePlaylists();
        trackRow.UpdatePlaylistCheckBox(playlist, hasPlaylistName);
      }
      tracksViewSource.View.Refresh();
    }


    private void memberWindow_Closed(object? sender, EventArgs e) {
      TrackPlayer.TrackChanged -= trackPlayer_TrackChanged;

      Owner?.Activate();
    }
    #endregion
    #endregion


    #region Methods
    //      -------

    private Playinglist? getPlayinglist() {
      if (TracksDataGrid.SelectedItems.Count==0) {
        System.Diagnostics.Debugger.Break();
        return null;
      } else if (TracksDataGrid.SelectedItems.Count==1) {
        var tracks = new List<Track>();
        for (int rowIndex = TracksDataGrid.SelectedIndex; rowIndex<TracksDataGrid.Items.Count; rowIndex++) {
          tracks.Add(((TrackRow)TracksDataGrid.Items[rowIndex]).Track);
        }
        for (int rowIndex = 0; rowIndex<TracksDataGrid.SelectedIndex; rowIndex++) {
          tracks.Add(((TrackRow)TracksDataGrid.Items[rowIndex]).Track);
        }
        return new Playinglist(tracks);
      } else {
        var trackQuery =
          from object gridItem in TracksDataGrid.SelectedItems
          select ((TrackRow)gridItem).Track;
        return new Playinglist(trackQuery);
      }
    }


    //private Track getNextTrack() {
    //  var trackIndex = TracksDataGrid.SelectedIndex + 1;
    //  if (trackIndex>=TracksDataGrid.Items.Count) {
    //    trackIndex = 0;
    //  }
    //  return ((TrackRow)TracksDataGrid.Items[trackIndex]).Track;
    //}
    #endregion
  }
}
