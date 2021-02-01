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
      LocationsComboBox.SelectionChanged += filterComboBox_SelectionChanged;
      LocationsComboBox.ItemsSource = DC.Data.LocationStrings;
      PlayListsComboBox.SelectionChanged += filterComboBox_SelectionChanged;
      PlayListsComboBox.ItemsSource = DC.Data.PlaylistStrings;
      AddToPlayListComboBox.ItemsSource = DC.Data.PlaylistStrings;
      ArtistComboBox.SelectionChanged += filterComboBox_SelectionChanged;
      ArtistComboBox.ItemsSource = DC.Data.Artists;
      AlbumComboBox.SelectionChanged += filterComboBox_SelectionChanged;
      AlbumComboBox.ItemsSource = DC.Data.Albums;
      AlbumComboBox.DisplayMemberPath = "AlbumArtist";
      GenreComboBox.SelectionChanged += filterComboBox_SelectionChanged;
      GenreComboBox.ItemsSource = DC.Data.Genres;
      YearComboBox.SelectionChanged += yearComboBox_SelectionChanged;
      YearComboBox.ItemsSource = DC.Data.Years;
      DeleteCheckBox.Click += checkBox_Click;
      PlayListCheckBox.Click += checkBox_Click;
      ClearButton.Click += clearButton_Click;
      SelectAllButton.Click += selectAllButton_Click;
      UnselectAllButton.Click += unselectAllButton_Click;
      DeleteButton.Click += deleteButton_Click;
      AddToPlayListButton.Click += addToPlayListButton_Click;
      RenameTrackButton.Click += renameTrackButton_Click;

      //datagrid
      trackRows = new();
      foreach (var track in DC.Data.Tracks.Values.OrderBy(t=>t.Title)) {
        trackRows.Add(new TrackRow(track));
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

      TrackRow.HasIsDeletionChanged = trackRow_HasSelectedChanged;
      trackRow_HasSelectedChanged();

      TrackPlayer.TrackChanged += trackPlayer_TrackChanged;
      TrackPlayer.Init(getCurrentTrack, getNextTrack);
      Closed += memberWindow_Closed;

      MainWindow.Register(this, "Tracks");
    }
    #endregion


    #region TrackRow Data
    //      -------------

    static int trackNo = 0;


    public class TrackRow: INotifyPropertyChanged {
      public int No { get; }
      public Track Track { get; }
      public string? Playlists { get; set; }
      public bool IsDeletion {
        get {
          return isDeletion;
        }
        set {
          if (isDeletion!=value) {
            isDeletion = value;
            HasIsDeletionChanged?.Invoke();
          }
        }
      }
      bool isDeletion;

      //User selected this track for adding to playlist OR CheckBox is disabled and displays that the track is already in the playlist
      public bool IsAddPlaylist {
        get {
          return isAddPlaylist;
        }
        set {
          if (isAddPlaylist!=value) {
            isAddPlaylist = value;
            HasIsAddPlaylistChanged?.Invoke();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsAddPlaylist)));
          }
        }
      }
      bool isAddPlaylist;

      //Playlist CheckBox is visible if a playlist name is entered
      public Visibility PlaylistCheckBoxVisibility {
        get {
          return playlistCheckBoxVisibility;
        }
        set {
          if (playlistCheckBoxVisibility!=value) {
            playlistCheckBoxVisibility = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlaylistCheckBoxVisibility)));
          }
        }
      }
      Visibility playlistCheckBoxVisibility;

      //Playlist CheckBox is disabled when the track is already in the playlist
      public bool PlaylistCheckBoxIsEnabled {
        get {
          return playlistCheckBoxIsEnabled;
        }
        set {
          if (playlistCheckBoxIsEnabled!=value) {
            playlistCheckBoxIsEnabled = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlaylistCheckBoxIsEnabled)));
          }
        }
      }
      bool playlistCheckBoxIsEnabled;

      public Visibility ImportCheckBoxVisibility { get; }//Import Check is only visible if track is not imported yet


      public event PropertyChangedEventHandler? PropertyChanged;

#pragma warning disable CA2211 // Non-constant fields should not be visible
      public static Action? HasIsDeletionChanged;
      public static Action? HasIsAddPlaylistChanged;
#pragma warning restore CA2211


      public TrackRow(Track track) {
        No = trackNo++;
        Track = track;
        UpdatePlayLists();

        isDeletion = false;
      }

      public void UpdatePlayLists() {
        Playlists = null;
        var isFirst = true;
        foreach (var playlistTrack in Track.Playlists) {
          if (isFirst) {
            isFirst = false;
          } else {
            Playlists += '|';
          }
          Playlists += playlistTrack.Playlist.Name;
        }
      }

      public void UpdatePlayListCheckBoxes(Playlist? playlist, bool hasPlayListName) {
        if (!hasPlayListName) {
          PlaylistCheckBoxVisibility = Visibility.Hidden;
        } else {
          PlaylistCheckBoxVisibility = Visibility.Visible;
          PlaylistCheckBoxIsEnabled =!Track.Playlists.Where(plt => plt.Playlist==playlist).Any();
          if (!PlaylistCheckBoxIsEnabled) {
            //track is already in playlist, show it as disabled and selected
            IsAddPlaylist = true;
          }
        }
      }
    }
    #endregion


    #region Events
    //      ------

    private void tracksWindow_Loaded(object sender, RoutedEventArgs e) {
      AddToPlayListComboBox.SelectionChanged += addToPlayListComboBox_SelectionChanged;
      AddToPlayListComboBox.AddHandler(TextBoxBase.TextChangedEvent, new RoutedEventHandler(addToPlayListComboBox_TextChanged));
      AddToPlayListComboBox.LostFocus += addToPlayListComboBox_LostFocus;
      updatePlayListCheckBoxes();
    }

    const int minFilterStringLength = 0;
    //const int minFilterStringLength = 2;
    string filterText = "";


    private void filterTextBox_TextChanged(object sender, TextChangedEventArgs e) {
      if (isClearing) return;

      var textLength = FilterTextBox.Text.Length;
      if (textLength==0 || textLength>minFilterStringLength) {
        filterText = FilterTextBox.Text.ToLowerInvariant();
        tracksViewSource.View.Refresh();
      };
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
      LocationsComboBox.SelectedIndex = 0;
      PlayListsComboBox.SelectedIndex = 0;
      ArtistComboBox.SelectedIndex = 0;
      AlbumComboBox.SelectedIndex = 0;
      GenreComboBox.SelectedIndex = 0;
      YearComboBox.SelectedIndex = 0;

      isClearing = false;
      tracksViewSource.View.SortDescriptions.Clear();
      tracksViewSource.View.Refresh();
    }


    private void selectAllButton_Click(object sender, RoutedEventArgs e) {
      foreach (var item in TracksDataGrid.Items) {
        var trackRow = (TrackRow)item;
        trackRow.IsDeletion = true;
        if (trackRow.PlaylistCheckBoxIsEnabled) {
          trackRow.IsAddPlaylist = true;
        }
      }
      tracksViewSource.View.Refresh();
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


    private void addToPlayListComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      if (e.AddedItems.Count>0) {
        var addedItem = e.AddedItems[0];
        if (addedItem is not null) {
          updatePlayList(addedItem.ToString());
          return;
        }
      }
      updatePlayList(AddToPlayListComboBox.Text);
    }


    private void addToPlayListComboBox_LostFocus(object sender, RoutedEventArgs e) {
      updatePlayList(AddToPlayListComboBox.Text);
    }

    private void addToPlayListComboBox_TextChanged(object sender, RoutedEventArgs e) {
      updatePlayList(AddToPlayListComboBox.Text);
    }

    bool hasPlayListName;
    Playlist? playlist;


    private void updatePlayList(string? playlistName) {
      var hasPlayListNameNew = !string.IsNullOrEmpty(playlistName);
      Playlist? playlistNew;
      if (string.IsNullOrWhiteSpace(playlistName)) {
        playlistNew = null;
      } else {
        DC.Data.PlaylistsByNameLower.TryGetValue(playlistName.ToLowerInvariant(), out playlistNew);
      }

      if (hasPlayListName!=hasPlayListNameNew || playlist!=playlistNew) {
        hasPlayListName = hasPlayListNameNew;
        playlist = playlistNew;
        updatePlayListCheckBoxes();
      }
    }


    private void updatePlayListCheckBoxes() {
      foreach (var item in TracksDataGrid.Items) {
        var trackRow = (TrackRow)item;
        trackRow.UpdatePlayListCheckBoxes(playlist, hasPlayListName);
      }
      updateDeletionCountTextBox();
    }


    private void tracksViewSource_Filter(object sender, FilterEventArgs e) {
      var trackRow = (TrackRow)e.Item;
      var isAccepted = false;
      var isRefused = false;
      if (filterText.Length>minFilterStringLength) {
        if (trackRow.Track.Album?.ToLowerInvariant().Contains(filterText)??false) {
          isAccepted = true;
        } else if (trackRow.Track.Artists?.ToLowerInvariant().Contains(filterText)??false) {
          isAccepted = true;
        } else if (trackRow.Track.Composers?.ToLowerInvariant().Contains(filterText)??false) {
          isAccepted = true;
        } else if (trackRow.Track.Publisher?.ToLowerInvariant().Contains(filterText)??false) {
          isAccepted = true;
        } else if (trackRow.Track.Title?.ToLowerInvariant().Contains(filterText)??false) {
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

      if (PlayListsComboBox.SelectedIndex>0) {
        if (trackRow.Playlists?.Contains((string)PlayListsComboBox.SelectedItem)??false) {
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

      //if no filter is active (=!isRefused) OR any filter does accept the item, then accept, i.e. OR conidion.
      isAccepted = !isRefused || isAccepted;

      //only accept item which match IsDeletion
      if (DeleteCheckBox.IsChecked is not null) {
        if (trackRow.IsDeletion!=DeleteCheckBox.IsChecked) {
          isAccepted = false;
        }
      }

      //only accept item which match IsDeletion
      if (PlayListCheckBox.IsChecked is not null) {
        if (trackRow.IsAddPlaylist!=PlayListCheckBox.IsChecked) {
          isAccepted = false;
        }
      }

      e.Accepted = isAccepted;
    }


    private void trackRow_HasSelectedChanged() {
      updateDeletionCountTextBox();
    }


    int deletionCount;


    private void updateDeletionCountTextBox() {
      deletionCount = 0;
      var existPlaylistCount = 0;
      var addPlaylistCount = 0;
      foreach (var trackRow in trackRows) {
        if (trackRow.IsDeletion) {
          deletionCount++;
        }
        if (hasPlayListName) {
          if (trackRow.PlaylistCheckBoxIsEnabled) {
            if (trackRow.IsAddPlaylist) {
              addPlaylistCount++;
            }
          } else {
            existPlaylistCount++;
          }
        }
      }
      DeletionCountTextBox.Text = $"Del: {deletionCount}, PL exist: {existPlaylistCount}, new: {addPlaylistCount}";
    }


    private void deleteButton_Click(object sender, RoutedEventArgs e) {
      var result = MessageBox.Show($"Do you want to delete {deletionCount} track(s) ?", "Deletion", MessageBoxButton.YesNo,
        MessageBoxImage.Question, MessageBoxResult.No);
      if (result==MessageBoxResult.Yes) {
        var newTrackRows = new List<TrackRow>(trackRows.Count);
        foreach (var trackRow in trackRows) {
          if (trackRow.IsDeletion) {
            foreach (var playlistsTrack in trackRow.Track.Playlists) {
              playlistsTrack.Release();
            }
            trackRow.Track.Release();
          } else {
            newTrackRows.Add(trackRow);
          }
        }
        tracksViewSource.Source = trackRows = newTrackRows;
      }
    }


    private void tracksDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
      var selectedIndex = TracksDataGrid.SelectedIndex;
      if (selectedIndex<0) return;

      var dataGridCell = ((DependencyObject)e.OriginalSource).FindVisualParentOfType<DataGridCell>();
      if (dataGridCell is null) return;

      if (dataGridCell.Column.DisplayIndex==11) return;

      this.TrackPlayer.Play(((TrackRow)TracksDataGrid.Items[selectedIndex]).Track);
    }


    private void tracksDataGrid_KeyDown(object sender, KeyEventArgs e) {
      if (e.SystemKey==Key.R) {
        renameSeletctedTrack();
        e.Handled = true;
      };
    }


    private void renameSeletctedTrack() {
      Track track = ((TrackRow)TracksDataGrid.SelectedItem).Track;
      TrackPlayer.CloseIfSelected(track);
      TrackRenameWindow.Show(this, track, updateSelectedItem);
    }


    private void renameTrackButton_Click(object sender, RoutedEventArgs e) {
      renameSeletctedTrack();
    }


    private void renameMenuItem_Click(object sender, RoutedEventArgs e) {
      Track track = ((TrackRow)TracksDataGrid.SelectedItem).Track;
      TrackPlayer.CloseIfSelected(track);
      TrackRenameWindow.Show(this, track, updateSelectedItem);
    }


    private void updateSelectedItem(Track track) {
      var selectedIndex = TracksDataGrid.SelectedIndex;
      tracksViewSource.View.Refresh();
      TracksDataGrid.SelectedIndex = selectedIndex;
    }


    private void trackPlayer_TrackChanged(Track track) {
      for (int itemIndex = 0; itemIndex < TracksDataGrid.Items.Count; itemIndex++) {
        var trackRow = (TrackRow)TracksDataGrid.Items[itemIndex];
        if (trackRow.Track==track) {
          TracksDataGrid.SelectedIndex = itemIndex;
          TracksDataGrid.ScrollIntoView(trackRow);
          return;
        }
      }
    }


    private void addToPlayListButton_Click(object sender, RoutedEventArgs e) {
      var playListName = AddToPlayListComboBox.Text;
      if (string.IsNullOrEmpty(playListName)) {
        MessageWindow.Show(this, "Provide name for playlist");
        return;
      }
      if (!DC.Data.PlaylistsByNameLower.TryGetValue(playListName.ToLower(), out var playlist)) {
        playlist = new Playlist(playListName);
      }
      var playlistTracks = new List<Track>();
      var trackNo = playlist.Tracks.Count;
      foreach (var item in TracksDataGrid.Items) {
        var trackRow = (TrackRow)item;
        if (trackRow.PlaylistCheckBoxIsEnabled &&  trackRow.IsAddPlaylist) {
          _ = new PlaylistTrack(playlist, trackRow.Track, trackNo++);
          playlistTracks.Add(trackRow.Track);
        }
      }
      PlaylistWindow.Show(this, playlist, playlistTracks, refreshTrackDataGrid);
    }


    private void refreshTrackDataGrid(Playlist playlist) {
      foreach (var item in TracksDataGrid.Items) {
        var trackRow = (TrackRow)item;
        trackRow.IsDeletion = false;
        trackRow.IsAddPlaylist = false;
        trackRow.UpdatePlayLists();
        trackRow.UpdatePlayListCheckBoxes(playlist, hasPlayListName);
      }
      tracksViewSource.View.Refresh();
    }


    private void memberWindow_Closed(object? sender, EventArgs e) {
      TrackPlayer.TrackChanged -= trackPlayer_TrackChanged;

      Owner?.Activate();
    }
    #endregion



    #region Methods
    //      -------

    private Track getCurrentTrack() {
      return ((TrackRow)TracksDataGrid.SelectedItem).Track;
    }


    private Track getNextTrack() {
      var trackIndex = TracksDataGrid.SelectedIndex + 1;
      if (trackIndex>=TracksDataGrid.Items.Count) {
        trackIndex = 0;
      }
      return ((TrackRow)TracksDataGrid.Items[trackIndex]).Track;
    }
    #endregion
  }
}
