using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
    readonly List<TrackRow> trackRows;


    public TracksWindow() {
      InitializeComponent();

      //filter
      FilterTextBox.TextChanged += filterTextBox_TextChanged;
      LocationsComboBox.SelectionChanged += filterComboBox_SelectionChanged;
      LocationsComboBox.ItemsSource = DC.Data.LocationStrings;
      PlayListsComboBox.SelectionChanged += filterComboBox_SelectionChanged;
      PlayListsComboBox.ItemsSource = DC.Data.PlaylistStrings;
      AddToPlayListsComboBox.ItemsSource = DC.Data.PlaylistStrings;
      ArtistComboBox.SelectionChanged += filterComboBox_SelectionChanged;
      ArtistComboBox.ItemsSource = DC.Data.Artists;
      AlbumComboBox.SelectionChanged += filterComboBox_SelectionChanged;
      AlbumComboBox.ItemsSource = DC.Data.Albums;
      AlbumComboBox.DisplayMemberPath = "AlbumArtist";
      GenreComboBox.SelectionChanged += filterComboBox_SelectionChanged;
      GenreComboBox.ItemsSource = DC.Data.Genres;
      YearComboBox.SelectionChanged += yearComboBox_SelectionChanged;
      YearComboBox.ItemsSource = DC.Data.Years;
      SelectedCheckBox.Click += checkBox_Click;
      ClearButton.Click += clearButton_Click;
      SelectAllButton.Click += selectAllButton_Click;
      UnselectAllButton.Click += unselectAllButton_Click;
      AddToPlayListButton.Click += addToPlayListButton_Click;
      RenameTrackButton.Click += renameTrackButton_Click;

      //read tracks
      trackRows = new();
      foreach (var track in DC.Data.Tracks.Values.OrderBy(t=>t.Title)) {
        trackRows.Add(new TrackRow(track));
      }

      //datagrid
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

      TrackRow.HasSelectedChanged = trackRow_HasSelectedChanged;
      trackRow_HasSelectedChanged();

      TrackPlayer.TrackChanged += trackPlayer_TrackChanged;
      TrackPlayer.Init(getCurrentTrack, getNextTrack);
      Closed += memberWindow_Closed;

      MainWindow.Register(this, "Tracks");
    }


    private static void addStringParts(string? text, SortedSet<string> texts) {
      if (text is not null) {
        var textParts = text.Split(';', StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries);
        foreach (var textPart in textParts) {
          texts.Add(textPart);
        }
      }
    }
    #endregion


    #region TrackRow Data
    //      -------------

    static int trackNo = 0;


    public class TrackRow {
      public int No { get; }
      public Track Track { get; }
      public string? Playlists { get; }
      public bool IsTrackSelected {
        get {
          return isTrackSelected;
        }
        set {
          if (isTrackSelected!=value) {
            isTrackSelected = value;
            HasSelectedChanged?.Invoke();
          }
        }
      }
      bool isTrackSelected;

#pragma warning disable CA2211 // Non-constant fields should not be visible
      public static Action? HasSelectedChanged;
#pragma warning restore CA2211


      public TrackRow(Track track) {
        No = trackNo++;
        Track = track;

        Playlists = null;
        var isFirst = true;
        foreach (var playlistTrack in track.Playlists) {
          if (isFirst) {
            isFirst = false;
          } else {
            Playlists += '|';
          }
          Playlists += playlistTrack.Playlist.Name;
        }

        isTrackSelected = false;
      }
    }
    #endregion


    #region Events
    //      ------

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
        trackRow.IsTrackSelected = true;
      }
      tracksViewSource.View.Refresh();
    }


    private void unselectAllButton_Click(object sender, RoutedEventArgs e) {
      foreach (var item in TracksDataGrid.Items) {
        var trackRow = (TrackRow)item;
        trackRow.IsTrackSelected = false;
      }
      tracksViewSource.View.Refresh();
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

      //only accept item which match IsSelected
      if (SelectedCheckBox.IsChecked is not null) {
        if (trackRow.IsTrackSelected!=SelectedCheckBox.IsChecked) {
          isAccepted = false;
        }
      }

      e.Accepted = isAccepted;
    }


    private void trackRow_HasSelectedChanged() {
      var selectedCount = 0;
      foreach (var trackRow in trackRows) {
        if (trackRow.IsTrackSelected) {
          selectedCount++;
        }
      }
      SelectedCountTextBox.Text = selectedCount.ToString();
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
      var playListName = AddToPlayListsComboBox.Text;
      if (string.IsNullOrEmpty(playListName)) {
        MessageWindow.Show(this, "Provide name for playlist");
        return;
      }
      if (!DC.Data.PlaylistsByNameLower.TryGetValue(playListName.ToLower(), out var playlist)) {
        playlist = new Playlist(playListName);
      }
      var playlistTracks = new List<Track>();
      foreach (var item in TracksDataGrid.Items) {
        var trackRow = (TrackRow)item;
        if (trackRow.IsTrackSelected) {
          playlistTracks.Add(trackRow.Track);
        }
      }
      PlaylistWindow.Show(this, playlist, playlistTracks, refreshTrackDataGrid);
    }


    private void refreshTrackDataGrid(Playlist playlist) {
      foreach (var item in TracksDataGrid.Items) {
        ((TrackRow)item).IsTrackSelected = false;
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
