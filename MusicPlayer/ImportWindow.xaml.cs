using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
using BaseLib;
using System.Collections;
using System.Windows.Controls.Primitives;


namespace MusicPlayer {


  /// <summary>
  /// Interaction logic for ImportWindow.xaml
  /// </summary>
  public partial class ImportWindow: Window {


    #region Constructor
    //      -----------

    public static ImportWindow Show(Window ownerWindow) {
      var window = new ImportWindow() { Owner = ownerWindow };
      window.Show();
      return window;
    }


    readonly System.Windows.Data.CollectionViewSource tracksViewSource;


   public ImportWindow() {
      InitializeComponent();

      Loaded += importWindow_Loaded;

      ChangeDirectoryButton.Click += changeDirectoryButton_Click;

      //filter
      FilterTextBox.TextChanged += filterTextBox_TextChanged;
      ArtistComboBox.SelectionChanged += filterComboBox_SelectionChanged;
      AlbumComboBox.SelectionChanged += filterComboBox_SelectionChanged;
      ComposerComboBox.SelectionChanged += filterComboBox_SelectionChanged;
      PublisherComboBox.SelectionChanged += filterComboBox_SelectionChanged;
      YearComboBox.SelectionChanged += yearComboBox_SelectionChanged;
      GenreComboBox.SelectionChanged += filterComboBox_SelectionChanged;
      ImportCheckBox.Click += filterCheckBox_Click;
      PlaylistCheckBox.Click += filterCheckBox_Click;
      ExistCheckBox.Click += filterCheckBox_Click;
      DuplicateCheckBox.Click += filterCheckBox_Click;
      ClearButton.Click += clearButton_Click;
      ImportAllButton.Click += importAllButton_Click;
      PLAllButton.Click += plAllButton_Click;
      UnselectAllButton.Click += unselectAllButton_Click;
      RenameTrackButton.Click += renameTrackButton_Click;

      //datagrid
      tracksViewSource = ((System.Windows.Data.CollectionViewSource)this.FindResource("TracksViewSource"));
      tracksViewSource.Filter += tracksViewSource_Filter;
      TracksDataGrid.MouseDoubleClick += tracksDataGrid_MouseDoubleClick;
      TracksDataGrid.KeyDown += tracksDataGrid_KeyDown;
      var contextMenu = new ContextMenu();
      var renameMenuItem = new MenuItem {Header = "Rename" };
      renameMenuItem.Click += renameMenuItem_Click;
      contextMenu.Items.Add(renameMenuItem);
      TracksDataGrid.ContextMenu = contextMenu;

      ImportButton.Click += importButton_Click;
      ImportButton.IsEnabled = false;

      TrackPlayer.TrackChanged += trackPlayer_TrackChanged;
      Closed += memberWindow_Closed;

      MainWindow.Register(this, "Import");
    }
    #endregion


    #region TrackRow Data
    //      -------------

    static int trackNo = 0;


    public class TrackRow: INotifyPropertyChanged{
      public int No { get; }
      public Track Track { get; }

      /// <summary>
      /// User selected this track for import
      /// </summary>
      public bool IsImport {
        get {
          return isImport;
        }
        set {
          //if (value && IsExisting) {
          //  throw new Exception("It is not possible to select (import) a track which exists already.");
          //}
          if (IsExisting) return; //user clicked into empty cell where hidden Import CheckBox sits

          if (isImport!=value) {
            isImport = value;
            if (PlaylistCheckBoxIsEnabled) {
              IsAddPlaylist = value;
            }
            PlaylistCheckBoxVisibility = hasPlaylistName && (value || IsExisting) ? Visibility.Visible : Visibility.Hidden;
            HasSelectedChanged?.Invoke();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsImport)));
          }
        }
      }
      bool isImport;


      public Visibility ImportCheckBoxVisibility { get; }//Import Check is only visible if track is not imported yet
      public bool ImportCheckBoxIsEnabled { get; }


      /// <summary>
      /// User selected this track for adding to playlist OR CheckBox is disabled and displays that the track is already in 
      /// the playlist
      /// </summary>
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


      /// <summary>
      /// Playlist CheckBox is visible if a playlist name is entered
      /// </summary>
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


      /// <summary>
      /// Playlist CheckBox is disabled when the track is already in the playlist
      /// </summary>
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


      public bool IsExisting { get;} //track is already imported
      public bool IsDouble { get; set; } //2 tracks in the import list have the same name and artist

#pragma warning disable CA2211 // Non-constant fields should not be visible
      public static Action? HasSelectedChanged;
      public static Action? HasIsAddPlaylistChanged;
#pragma warning restore CA2211

      public event PropertyChangedEventHandler? PropertyChanged;


      public TrackRow(FileInfo file, Location location) {
        No = trackNo++;
        Track = new Track(file, location, isStoring: false);
        if (DC.Data.TracksByTitleArtists.TryGetValue(Track.TitleArtists, out var existingTrack)) {
          Track = existingTrack;
          IsExisting = true;
          ImportCheckBoxVisibility = Visibility.Hidden;
          ImportCheckBoxIsEnabled = false;
        } else {
          IsExisting = false;
          ImportCheckBoxVisibility = Visibility.Visible;
          ImportCheckBoxIsEnabled = true;
        }
        IsImport = !IsExisting;
      }


      Playlist? playlist;
      bool hasPlaylistName;


      public void UpdatePlaylistCheckBoxes(Playlist? playlist, bool hasPlaylistName) {
        this.playlist = playlist;
        this.hasPlaylistName = hasPlaylistName;
        if (!hasPlaylistName) {
          PlaylistCheckBoxVisibility = Visibility.Hidden;
        } else {
          PlaylistCheckBoxVisibility = IsImport || IsExisting ? Visibility.Visible : Visibility.Hidden;
          PlaylistCheckBoxIsEnabled =!Track.PlaylistTracks.Where(plt => plt.Playlist==playlist).Any();
          if (PlaylistCheckBoxIsEnabled) {
            //track is not yet in playlist. User has just selected a playlist. 
            IsAddPlaylist = IsImport;
          } else {
            //track is already in playlist, show it as disabled and selected
            IsAddPlaylist = true;
          }
        }
      }
    }
    #endregion


    #region Events
    //      ------

    private async void importWindow_Loaded(object sender, RoutedEventArgs e) {
      await selectDirectoy();
      PlaylistComboBox.SelectionChanged += PlaylistComboBox_SelectionChanged;
      PlaylistComboBox.AddHandler(TextBoxBase.TextChangedEvent, new RoutedEventHandler(PlaylistComboBox_TextChanged));
      PlaylistComboBox.LostFocus += PlaylistComboBox_LostFocus;
    }

    #region Read Tracks

    private async void changeDirectoryButton_Click(object sender, RoutedEventArgs e) {
      await selectDirectoy();
    }


    List<TrackRow> trackRows;
    Location location;


    private async Task selectDirectoy() {
      var openFolderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
      if (openFolderDialog.ShowDialog()==false) {
        return;

      } else {
        Task task;
        var path = openFolderDialog.SelectedPath;
        DirectoryTextBox.Text = path;
        var pathLowerCase = path.ToLowerInvariant();
        #pragma warning disable CS8601 // Possible null reference assignment.
        if (!DC.Data.LocationsByPathLower.TryGetValue(pathLowerCase, out location)) {
        #pragma warning restore CS8601
          location = new Location(path, path[(path.LastIndexOf('\\')+1)..], isStoring: false);
        }
        LocationTextBox.Text = location.Name;
        var tracks = new List<Track>();
        trackRows = new List<TrackRow>();
        trackNo = 0;
        var tracksDirectory = new DirectoryInfo(DirectoryTextBox.Text);
        var files = tracksDirectory.GetFiles("*.mp3");
        Array.Sort<FileInfo>(files, (x, y) => { return x.Name.CompareTo(y.Name); });
        TrackRow.HasSelectedChanged = null;
        TrackRow.HasIsAddPlaylistChanged = null;
        task = Task.Run(() => readTracks(files, trackRows, tracks));
        
        await task;
        var (doublesCount, existingsCount) = markDoubles(trackRows);
        ImportTextBox.Text = $"{files.Length} files read, {existingsCount} existing, {doublesCount} doubles";
        tracksViewSource.Source = trackRows;
        AlbumComboBox.ItemsSource = albums;
        ArtistComboBox.ItemsSource = artists;
        ComposerComboBox.ItemsSource = composers;
        GenreComboBox.ItemsSource = genres;
        PublisherComboBox.ItemsSource = publishers;
        var yearList = new List<string> { "" };
        foreach (var year in years) {
          yearList.Add(year.ToString());
        }
        YearComboBox.ItemsSource = yearList;

        var playlists = DC.Data.Playlists.Values.OrderBy(p => p.Name).Select(p=>p.Name).ToList();
        playlists.Insert(0, "");
        PlaylistComboBox.ItemsSource = playlists;

        TrackPlayer.Init(getPlayinglist);

        ImportButton.IsEnabled = true;
        updatePlaylistCheckBoxes();
        PlaylistComboBox.Focus();
        TrackRow.HasSelectedChanged = trackRow_HasSelectedChanged;
        TrackRow.HasIsAddPlaylistChanged = trackRow_HasSelectedChanged;
        trackRow_HasSelectedChanged();
        return;
      }
    }


    private static (int, int) markDoubles(List<TrackRow> trackRows) {
      HashSet<string> titleArtistsHashSet = new();
      HashSet<string> doubleTitleArtistsHashSet = new();
      foreach (var trackRow in trackRows) {
        if (!titleArtistsHashSet.Add(trackRow.Track.TitleArtists)) {
          doubleTitleArtistsHashSet.Add(trackRow.Track.TitleArtists);
        }
      }

      var doublesCount = 0;
      var existingsCount = 0;
      foreach (var trackRow in trackRows) {
        trackRow.IsDouble = doubleTitleArtistsHashSet.Contains(trackRow.Track.TitleArtists);
        if (trackRow.IsDouble) {
          doublesCount++;
        }
        if (trackRow.IsExisting) {
          existingsCount++;
        }
      }
      return (doublesCount, existingsCount);
    }


    string? selectedCountString;
    string? playlistCountString;


    private void trackRow_HasSelectedChanged() {
      var selectedCount = 0;
      var playlistCount = 0;
      foreach (var trackRow in trackRows) {
        if (trackRow.IsImport && !trackRow.IsExisting) {
          selectedCount++;
        }
        if (trackRow.IsAddPlaylist) {
          playlistCount++;
        }
      }
      selectedCountString = $"Selected: { selectedCount}";
      playlistCountString = $"Playlist: {playlistCount}";
      updateSelectedCountTextBox();
    }

    private void updateSelectedCountTextBox() {
      SelectedCountTextBox.Text =hasPlaylistName ? $"{selectedCountString}, {playlistCountString}" : selectedCountString;
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


    SortedSet<string> albums;
    SortedSet<string> artists;
    SortedSet<string> composers;
    SortedSet<string> genres;
    SortedSet<string> publishers;
    SortedSet<int> years;


    private void readTracks(FileInfo[] files, List<TrackRow> trackRows, List<Track> tracks) {
      albums = new() {""};
      artists = new() { "" };
      composers = new() { "" };
      genres = new() { "" };
      publishers = new() { "" };
      years = new();
      var nextUpdate = DateTime.MinValue;
      var fileCount = 0;
      foreach (var file in files) {
        var now = DateTime.Now;
        if (nextUpdate<now) {
          nextUpdate = now.AddMilliseconds(100);
          ImportTextBox.Dispatcher.Invoke(() =>
          {
            ImportTextBox.Text = $"{fileCount} of {files.Length} files read.";
          });
        }
        var trackRow = new TrackRow(file, location);
        trackRows.Add(trackRow);
        tracks.Add(trackRow.Track);
        fileCount++;

        if (trackRow.Track.Album is not null) albums.Add(trackRow.Track.Album);
        add(trackRow.Track.Artists, artists);
        add(trackRow.Track.Composers, composers);
        add(trackRow.Track.Genres, genres);
        if (trackRow.Track.Publisher is not null) publishers.Add(trackRow.Track.Publisher);
        if (trackRow.Track.Year is not null) years.Add(trackRow.Track.Year.Value);
      }
    }


    private static void add(string? text, SortedSet<string> texts) {
      if (text is not null) {
        var textParts = text.Split(';', StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries);
        foreach (var textPart in textParts) {
          texts.Add(textPart);
        }
      }
    }
    #endregion


    #region Filter

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


    private void filterCheckBox_Click(object sender, RoutedEventArgs e) {
      if (isClearing) return;

      tracksViewSource.View.Refresh();
    }


    bool isClearing;


    void clearButton_Click(object sender, RoutedEventArgs e) {
      isClearing = true;
      FilterTextBox.Text = "";
      filterText = "";
      AlbumComboBox.SelectedIndex = 0;
      ArtistComboBox.SelectedIndex = 0;
      ComposerComboBox.SelectedIndex = 0;
      GenreComboBox.SelectedIndex = 0;
      PublisherComboBox.SelectedIndex = 0;
      YearComboBox.SelectedIndex = 0;
      ImportCheckBox.IsChecked = null;
      PlaylistCheckBox.IsChecked = null;
      ExistCheckBox.IsChecked = null;
      DuplicateCheckBox.IsChecked = null;

      isClearing = false;
      tracksViewSource.View.SortDescriptions.Clear();
      tracksViewSource.View.Refresh();
    }


    private void importAllButton_Click(object sender, RoutedEventArgs e) {
      foreach (var item in TracksDataGrid.Items) {
        var trackRow = (TrackRow)item;
        trackRow.IsImport = !trackRow.IsExisting;
      }
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
        trackRow.IsImport = false;
        if (trackRow.PlaylistCheckBoxIsEnabled) {
          trackRow.IsAddPlaylist = false;
        }
      }
      //tracksViewSource.View.Refresh();
    }


    private void PlaylistComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      if (e.AddedItems.Count>0) {
        var addedItem = e.AddedItems[0];
        if (addedItem is not null) {
          updatePlaylist(addedItem.ToString());
          return;
        }
      }
      updatePlaylist(PlaylistComboBox.Text);
    }


    private void PlaylistComboBox_LostFocus(object sender, RoutedEventArgs e) {
      updatePlaylist(PlaylistComboBox.Text);
    }

    private void PlaylistComboBox_TextChanged(object sender, RoutedEventArgs e) {
      updatePlaylist(PlaylistComboBox.Text);
    }

    bool hasPlaylistName;
    Playlist? playlist;


    private void updatePlaylist(string? playlistName) {
      var hasPlaylistNameNew = !string.IsNullOrEmpty(playlistName);
      Playlist? playlistNew;
      if (string.IsNullOrWhiteSpace(playlistName)) {
        playlistNew = null;
      } else {
        DC.Data.PlaylistsByNameLower.TryGetValue(playlistName.ToLowerInvariant(), out playlistNew);
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
        trackRow.UpdatePlaylistCheckBoxes(playlist, hasPlaylistName);
      }
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

      if (ArtistComboBox.SelectedIndex>0) {
        if (trackRow.Track.Artists?.Contains((string)ArtistComboBox.SelectedItem)??false) {
          isAccepted = true;
        } else {
          isRefused = true;
        }
      }

      if (AlbumComboBox.SelectedIndex>0) {
        if (trackRow.Track.Album==(string)AlbumComboBox.SelectedItem) {
          isAccepted = true;
        } else {
          isRefused = true;
        }
      }

      if (ComposerComboBox.SelectedIndex>0) {
        if (trackRow.Track.Composers?.Contains((string)ComposerComboBox.SelectedItem)??false) {
          isAccepted = true;
        } else {
          isRefused = true;
        }
      }

      if (PublisherComboBox.SelectedIndex>0) {
        if (trackRow.Track.Publisher==(string)PublisherComboBox.SelectedItem) {
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

      if (GenreComboBox.SelectedIndex>0) {
        if (trackRow.Track.Genres?.Contains((string)GenreComboBox.SelectedItem)??false) {
          isAccepted = true;
        } else {
          isRefused = true;
        }
      }

      //if no filter is active (=!isRefused) OR any filter does accept the item, then accept, i.e. OR conidion.
      isAccepted = !isRefused || isAccepted;

      //only accept item which match IsSelected AND IsDuplicated, i.e. AND condition
      if (ImportCheckBox.IsChecked is not null) {
        if (trackRow.IsImport!=ImportCheckBox.IsChecked) {
          isAccepted = false;
        }
      }

      if (PlaylistCheckBox.IsChecked is not null) {
        if (trackRow.IsAddPlaylist!=PlaylistCheckBox.IsChecked) {
          isAccepted = false;
        }
      }

      if (ExistCheckBox.IsChecked is not null) {
        if (trackRow.IsExisting!=ExistCheckBox.IsChecked) {
          isAccepted = false;
        }
      }

      if (DuplicateCheckBox.IsChecked is not null) {
        if (trackRow.IsDouble!=DuplicateCheckBox.IsChecked) {
          isAccepted = false;
        }
      }

      e.Accepted = isAccepted;
    }
    #endregion


    #region Tracks Datagrid

    private void tracksDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
      var selectedIndex = TracksDataGrid.SelectedIndex;
      if (selectedIndex<0) return;

      var dataGridCell = ((DependencyObject)e.OriginalSource).FindVisualParentOfType<DataGridCell>();
      if (dataGridCell is null) return;

      if (dataGridCell.Column.DisplayIndex>=10) return; //checkboxes

      //this.TrackPlayer.Play(((TrackRow)TracksDataGrid.Items[selectedIndex]).Track);
      var playinglist = getPlayinglist();
      if (playinglist is not null) {
        TrackPlayer.Play(playinglist);
      }
    }


    private void tracksDataGrid_KeyDown(object sender, KeyEventArgs e) {
      if (e.SystemKey==Key.R) {
        renameSelectedTrack();
        e.Handled = true;
      };
    }


    private void renameSelectedTrack() {
      Track track = ((TrackRow)TracksDataGrid.SelectedItem).Track;
      TrackPlayer.StopTrackIfPlaying(track);
      TrackRenameWindow.Show(this, track, updateSelectedItem);
    }


    private void renameTrackButton_Click(object sender, RoutedEventArgs e) {
      renameSelectedTrack();
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
    #endregion


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


    private void importButton_Click(object sender, RoutedEventArgs e) {
      if (playlist is null) {
        var playlistName = PlaylistComboBox.Text;
        if (!string.IsNullOrWhiteSpace(playlistName)) {
          playlist = new Playlist(playlistName);
        }
      }

      foreach (var trackRow in trackRows) {
        if (trackRow.IsDouble && trackRow.IsImport) {
          MessageWindow.Show(this, "It is not possible to import tracks with the same artist name and title:" + Environment.NewLine +
            trackRow.Track.TitleArtists, null);
          return;
        }
      }

      if (location.Name!=LocationTextBox.Text) {
        location.Update(DirectoryTextBox.Text, LocationTextBox.Text);
      }
      if (location.Key<0) {
        location.Store();
      }

      var storedCount = 0;
      var playlistNewCount = playlist?.TracksCount??0;
      var playlistTracks = new List<PlaylistTrack>();
      try {
        foreach (var trackRow in trackRows) {
          if (trackRow.IsExisting) {
            if (trackRow.IsAddPlaylist && playlist is not null && trackRow.PlaylistCheckBoxIsEnabled) {
              playlistTracks.Add(new PlaylistTrack(playlist, trackRow.Track, playlistNewCount));
              playlistNewCount++;
            }

          } else {
            if (trackRow.IsImport) {
              trackRow.Track.Store();
              storedCount++;
              if (trackRow.IsAddPlaylist && playlist is not null) {
                playlistTracks.Add(new PlaylistTrack(playlist, trackRow.Track, playlistNewCount));
                playlistNewCount++;
              }
              trackRow.IsImport = false;//this sets also IsAddPlaylist to false
            }
          }
        };

        DC.Data.UpdateTracksStats();
        MainWindow.Current!.UpdatePlaylistsDataGrid();
        MainWindow.Current!.UpdateTracksStatistics();

        Close();
        if (playlistTracks.Count>0) {
          PlaylistWindow.Show(Owner, playlist!, playlistTracks);
        }
      } catch (Exception ex) {

        MessageWindow.Show(this, "Exception during Save" + Environment.NewLine +
          $"{storedCount} tracks saved, {playlistNewCount} added to playlist." + Environment.NewLine +ex.ToDetailString(), null).Title = "Exception";
      }
    }


    private void memberWindow_Closed(object? sender, EventArgs e) {
      TrackPlayer.TrackChanged -= trackPlayer_TrackChanged;

      Owner?.Activate();
    }
    #endregion
  }
}
