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
  /// Interaction logic for PlaylistWindow.xaml
  /// </summary>
  public partial class PlaylistWindow: Window {


    #region Constructor
    //      -----------

    public static void Show(
      Window ownerWindow, 
      Playlist playlist, 
      IEnumerable<Track>? additionalTracks = null, 
      Action<Playlist>? refreshOwner = null) 
    {
      var window = new PlaylistWindow(playlist, additionalTracks, refreshOwner) { Owner = ownerWindow };
      window.Show();
    }

    readonly Playlist? playlist;
    readonly Action<Playlist>? refreshOwner;

    readonly System.Windows.Data.CollectionViewSource tracksViewSource;
    readonly List<Track> tracks;
    readonly List<TrackRow> trackRows;
    readonly TimeSpan totalDuration;
    readonly List<string> locations;
    readonly List<DC.AlbumArtistAlbum> albums;
    readonly List<string> artists;
    readonly List<string> genres;
    readonly List<string> years;


    public PlaylistWindow(Playlist? playlist = null, IEnumerable<Track>? additionalTracks = null, Action<Playlist>? refreshOwner = null) {
      this.playlist = playlist;
      this.refreshOwner = refreshOwner;

      InitializeComponent();

      Loaded += playlistWindow_Loaded;

      //tracks
      tracks = new();
      trackRows = new();
      totalDuration = TimeSpan.Zero;
      locations = new();
      albums = new();
      artists = new();
      genres = new();
      years = new();

      if (playlist is not null) {
        PlaylistNameTextBox.Text = playlist.Name;
        foreach (var playListTrack in playlist.Tracks.OrderBy(plt => plt.TrackNo)) {
          tracks.Add(playListTrack.Track);
          trackRows.Add(new TrackRow(playListTrack.Track, isInPlaylist: true, isSelected: false, hasSelectedChanged));
        }
        if (additionalTracks is not null) {
          foreach (var track in additionalTracks) {
            var isExistsAlready = false;
            foreach (var playListTrack in track.Playlists) {
              if (playListTrack.Playlist!=playlist) {
                isExistsAlready = true;
                break;
              }
            }
            if (!isExistsAlready) {
              tracks.Add(track);
              trackRows.Add(new TrackRow(track, isInPlaylist: false, isSelected: true, hasSelectedChanged));
            }
          }
        }
        DC.GetTracksStats(ref totalDuration, locations, albums, artists, genres, years, tracks);
      }
      TracksCountTextBox.Text = tracks.Count.ToString();

      //filter
      FilterTextBox.TextChanged += filterTextBox_TextChanged;
      GenreComboBox.SelectionChanged += filterComboBox_SelectionChanged;
      GenreComboBox.ItemsSource = genres;
      LocationsComboBox.SelectionChanged += filterComboBox_SelectionChanged;
      LocationsComboBox.ItemsSource = locations;
      ArtistComboBox.SelectionChanged += filterComboBox_SelectionChanged;
      ArtistComboBox.ItemsSource = artists;
      AlbumComboBox.SelectionChanged += filterComboBox_SelectionChanged;
      AlbumComboBox.ItemsSource = albums;
      AlbumComboBox.DisplayMemberPath = "AlbumArtist";
      GenreComboBox.SelectionChanged += filterComboBox_SelectionChanged;
      GenreComboBox.ItemsSource = genres;
      YearComboBox.SelectionChanged += yearComboBox_SelectionChanged;
      YearComboBox.ItemsSource = years;
      SelectedCheckBox.Click += checkBox_Click;
      ClearButton.Click += clearButton_Click;
      SelectAllButton.Click += selectAllButton_Click;
      UnselectAllButton.Click += unselectAllButton_Click;


      //datagrid
      tracksViewSource = ((System.Windows.Data.CollectionViewSource)this.FindResource("TracksViewSource"));
      tracksViewSource.Source = trackRows;
      tracksViewSource.Filter += tracksViewSource_Filter;
      TracksDataGrid.MouseDoubleClick += tracksDataGrid_MouseDoubleClick;
      //TracksDataGrid.KeyDown += tracksDataGrid_KeyDown;
      //var contectMenu = new ContextMenu();
      //var renameMenuItem = new MenuItem { Header = "Rename" };
      //renameMenuItem.Click += renameMenuItem_Click;
      //contectMenu.Items.Add(renameMenuItem);
      //TracksDataGrid.ContextMenu = contectMenu;

      SaveButton.Click += saveButton_Click;

      TrackPlayer.TrackChanged += trackPlayer_TrackChanged;
      TrackPlayer.Init(getCurrentTrack, getNextTrack);
      Closed += memberWindow_Closed;

      MainWindow.Register(this, "Playlist " + playlist?.Name);
    }
    #endregion


    #region TrackRow Data
    //      -------------

    static int trackNo = 0;


    public class TrackRow {
      public int No { get; }
      public Track Track { get; }
      public string LocationLower { get; }
      public string AlbumLower { get; }
      public string ArtistsLower { get; }
      public string ComposersLower { get; }
      public string PublisherLower { get; }
      public string TitleLower { get; }
      public bool IsInPlaylist { get; }
      public bool IsSelected {
        get {
          return isSelected;
        }
        set {
          if (isSelected!=value) {
            isSelected = value;
            hasSelectedChanged?.Invoke();
          }
        }
      }
      bool isSelected;


      private Action? hasSelectedChanged;


      public TrackRow(Track track, bool isInPlaylist = false, bool isSelected = false, Action? hasSelectedChanged = null) {
        No = trackNo++;
        Track = track;
        LocationLower = track.Location.Name.ToLowerInvariant();
        ArtistsLower = track.Artists?.ToLowerInvariant()??"";
        AlbumLower = track.Album?.ToLowerInvariant()??"";
        ComposersLower = track.Composers?.ToLowerInvariant()??"";
        PublisherLower = track.Publisher?.ToLowerInvariant()??"";
        TitleLower = track.Title?.ToLowerInvariant()??"";
        IsInPlaylist = isInPlaylist;
        this.isSelected = isSelected;
        this.hasSelectedChanged = hasSelectedChanged;
      }


      //public TrackRow(TrackRow oldTrackRow, Track track) {
      //  No = oldTrackRow.No;
      //  Track = track;
      //  IsSelected = oldTrackRow.IsSelected;
      //}
    }
    #endregion


    #region Events
    //      ------

    private void playlistWindow_Loaded(object sender, RoutedEventArgs e) {
      PlaylistNameTextBox.Focus();
    }


    string filterText = "";


    private void filterTextBox_TextChanged(object sender, TextChangedEventArgs e) {
      if (isClearing) return;

      var textLength = FilterTextBox.Text.Length;
      filterText = FilterTextBox.Text.ToLowerInvariant();
      tracksViewSource.View.Refresh();
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
        trackRow.IsSelected = true;
      }
      tracksViewSource.View.Refresh();
    }


    private void unselectAllButton_Click(object sender, RoutedEventArgs e) {
      foreach (var item in TracksDataGrid.Items) {
        var trackRow = (TrackRow)item;
        trackRow.IsSelected = false;
      }
      tracksViewSource.View.Refresh();
    }


    private void tracksViewSource_Filter(object sender, FilterEventArgs e) {
      var trackRow = (TrackRow)e.Item;
      var isAccepted = false;
      var isRefused = false;
      if (filterText.Length>0) {
        if (trackRow.LocationLower.Contains(filterText)) {
          isAccepted = true;
        } else if (trackRow.ArtistsLower.Contains(filterText)) {
          isAccepted = true;
        } else if (trackRow.AlbumLower.Contains(filterText)) {
          isAccepted = true;
        } else if (trackRow.ComposersLower.Contains(filterText)) {
          isAccepted = true;
        } else if (trackRow.PublisherLower.Contains(filterText)) {
          isAccepted = true;
        } else if (trackRow.TitleLower.Contains(filterText)) {
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
        if (trackRow.IsSelected!=SelectedCheckBox.IsChecked) {
          isAccepted = false;
        }
      }

      e.Accepted = isAccepted;
    }


    private void hasSelectedChanged() {
      var selectedCount = 0;
      foreach (var trackRow in trackRows) {
        if (trackRow.IsSelected) {
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


    private void saveButton_Click(object sender, RoutedEventArgs e) {
      if (playlist is not null) {
        if (playlist.Name!=PlaylistNameTextBox.Text) {
          playlist.Update(PlaylistNameTextBox.Text);
        }
      }

      Close();
    }


    private void memberWindow_Closed(object? sender, EventArgs e) {
      TrackPlayer.TrackChanged -= trackPlayer_TrackChanged;
      refreshOwner?.Invoke(playlist!);
      Owner?.Activate();
    }
    #endregion
  }
}
