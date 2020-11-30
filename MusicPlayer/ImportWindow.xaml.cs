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


namespace MusicPlayer {
  /// <summary>
  /// Interaction logic for ImportWindow.xaml
  /// </summary>
  public partial class ImportWindow: Window {


    #region Constructor
    //      -----------

    public static ImportWindow Show(Window ownerWindow) {
      var window = new ImportWindow { Owner = ownerWindow };
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
      AlbumComboBox.SelectionChanged += filterComboBox_SelectionChanged;
      ArtistComboBox.SelectionChanged += filterComboBox_SelectionChanged;
      ComposerComboBox.SelectionChanged += filterComboBox_SelectionChanged;
      GenreComboBox.SelectionChanged += filterComboBox_SelectionChanged;
      PublisherComboBox.SelectionChanged += filterComboBox_SelectionChanged;
      YearComboBox.SelectionChanged += yearComboBox_SelectionChanged;
      ImportCheckBox.Click += checkBox_Click;
      ExistCheckBox.Click += checkBox_Click;
      DuplicateCheckBox.Click += checkBox_Click;
      ClearButton.Click += clearButton_Click;
      AddAllButton.Click += addAllButton_Click;
      RemoveAllButton.Click += removeAllButton_Click;
      RenameTrackButton.Click += renameTrackButton_Click;

      //datagrid
      tracksViewSource = ((System.Windows.Data.CollectionViewSource)this.FindResource("TracksViewSource"));
      tracksViewSource.Filter += tracksViewSource_Filter;
      TracksDataGrid.MouseDoubleClick += tracksDataGrid_MouseDoubleClick;
      TracksDataGrid.KeyDown += tracksDataGrid_KeyDown;
      var contectMenu = new ContextMenu();
      var renameMenuItem = new MenuItem {Header = "Rename" };
      renameMenuItem.Click += renameMenuItem_Click;
      contectMenu.Items.Add(renameMenuItem);
      TracksDataGrid.ContextMenu = contectMenu;

      SaveButton.Click += saveButton_Click;
      SaveButton.IsEnabled = false;

      TrackPlayer.TrackChanged += trackPlayer_TrackChanged;
      Closed += memberWindow_Closed;

      MainWindow.Register(this, "Import");

      Player.Current!.Traced += importWindow_Traced;
    }


    private void importWindow_Traced(string trace) {
      TraceTextBox.Text += Environment.NewLine + trace;
      TraceTextBox.ScrollToEnd();
    }
    #endregion


    #region TrackRow Data
    //      -------------

    static int trackNo = 0;


    public class TrackRow {
      public int No { get; }
      public Track Track { get; }
      public bool IsSelected {
        get {
          return isSelected;
        }
        set {
          if (isSelected!=value) {
            isSelected = value;
            HasSelectedChanged?.Invoke();
          }
        }
      }
      bool isSelected;


      public bool IsExisting { get; set; }
      public bool IsDouble { get; set; }


      #pragma warning disable CA2211 // Non-constant fields should not be visible
      public static Action? HasSelectedChanged;
      #pragma warning restore CA2211 


      public TrackRow(FileInfo file) {
        No = trackNo++;
        Track = new Track(file, isStoring: false);
        IsExisting = DC.Data.TracksByTitleArtists.ContainsKey(Track.TitleArtists);
        IsSelected = !IsExisting;
      }

      public TrackRow(TrackRow oldTrackRow, Track track) {
        No = oldTrackRow.No;
        Track = track;
        IsSelected = oldTrackRow.IsSelected;
        IsExisting = oldTrackRow.IsExisting;
      }
    }
    #endregion


    #region Events
    //      ------

    private async void importWindow_Loaded(object sender, RoutedEventArgs e) {
      await selectDirectoy();
    }


    private async void changeDirectoryButton_Click(object sender, RoutedEventArgs e) {
      await selectDirectoy();
    }


    List<TrackRow> trackRows;


    private async Task selectDirectoy() {
      var openFolderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
      if (openFolderDialog.ShowDialog()==false) {
        return;

      } else {
        Task task;
        DirectoryTextBox.Text = openFolderDialog.SelectedPath;
        var tracks = new List<Track>();
        trackRows = new List<TrackRow>();
        trackNo = 0;
        var tracksDirectory = new DirectoryInfo(DirectoryTextBox.Text);
        var files = tracksDirectory.GetFiles("*.mp3");
        Array.Sort<FileInfo>(files, (x, y) => { return x.Name.CompareTo(y.Name); });
        TrackRow.HasSelectedChanged = null;
        task = Task.Run(() => readTracks(files, trackRows, tracks));
        
        await task;
        ImportTextBox.Text = $"{files.Length} files read.";
        markDoubles(trackRows);
        tracksViewSource.Source = trackRows;
        TrackRow.HasSelectedChanged = trackRow_HasSelectedChanged;
        trackRow_HasSelectedChanged();
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


        TrackPlayer.Init(getCurrentTrack, getNextTrack);

        SaveButton.IsEnabled = true;
        return;
      }
    }


    private static void markDoubles(List<TrackRow> trackRows) {
      HashSet<string> titleArtistsHashSet = new();
      HashSet<string> doubleTitleArtistsHashSet = new();
      foreach (var trackRow in trackRows) {
        if (!titleArtistsHashSet.Add(trackRow.Track.TitleArtists)) {
          doubleTitleArtistsHashSet.Add(trackRow.Track.TitleArtists);
        }
      }

      foreach (var trackRow in trackRows) {
        trackRow.IsDouble = doubleTitleArtistsHashSet.Contains(trackRow.Track.TitleArtists);
      }
    }


    private void trackRow_HasSelectedChanged() {
      var selectedCount = 0;
      foreach (var trackRow in trackRows) {
        if (trackRow.IsSelected && !trackRow.IsExisting) {
          selectedCount++;
        }
      }
      SelectedCountTextBox.Text = selectedCount.ToString();
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
        var trackRow = new TrackRow(file);
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
      AlbumComboBox.SelectedIndex = 0;
      ArtistComboBox.SelectedIndex = 0;
      ComposerComboBox.SelectedIndex = 0;
      GenreComboBox.SelectedIndex = 0;
      PublisherComboBox.SelectedIndex = 0;
      YearComboBox.SelectedIndex = 0;
      ImportCheckBox.IsChecked = null;
      ExistCheckBox.IsChecked = null;
      DuplicateCheckBox.IsChecked = null;

      isClearing = false;
      tracksViewSource.View.SortDescriptions.Clear();
      tracksViewSource.View.Refresh();
    }


    private void addAllButton_Click(object sender, RoutedEventArgs e) {
      foreach (var item in TracksDataGrid.Items) {
        var trackRow = (TrackRow)item;
        trackRow.IsSelected = true;
      }
      tracksViewSource.View.Refresh();
    }


    private void removeAllButton_Click(object sender, RoutedEventArgs e) {
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

      if (AlbumComboBox.SelectedIndex>0) {
        if (trackRow.Track.Album==(string)AlbumComboBox.SelectedItem) {
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

      if (ComposerComboBox.SelectedIndex>0) {
        if (trackRow.Track.Composers?.Contains((string)ComposerComboBox.SelectedItem)??false) {
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

      //if no filter is active (=!isRefused) OR any filter does accept the item, then accept, i.e. OR conidion.
      isAccepted = !isRefused || isAccepted;

      //only accept item which match IsSelected AND IsDuplicated, i.e. AND condition
      if (ImportCheckBox.IsChecked is not null) {
        if (trackRow.IsSelected!=ImportCheckBox.IsChecked) {
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


    private void tracksDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
      var selectedIndex = TracksDataGrid.SelectedIndex;
      if (selectedIndex<0) return;

      var dataGridCell = ((DependencyObject)e.OriginalSource).FindVisualParentOfType<DataGridCell>();
      if (dataGridCell is null) return;

      if (dataGridCell.Column.DisplayIndex==10) return;

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


    private void tracksDataGrid_KeyDown(object sender, KeyEventArgs e) {
      System.Diagnostics.Debug.WriteLine($"Key: {e.Key}; Sys: {e.SystemKey}");
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
      for (int trackRowsIndex = 0; trackRowsIndex < trackRows.Count; trackRowsIndex++) {
        var trackRow = trackRows[trackRowsIndex];
        if (trackRow.Track.FileName==track.FileName) {
          trackRows[trackRowsIndex] = new TrackRow(trackRow, track);
          var selectedIndex = TracksDataGrid.SelectedIndex;
          tracksViewSource.View.Refresh();
          //var sortDescriptions = new List<SortDescription>();
          //foreach (var sortDescription in tracksViewSource.View.SortDescriptions) {
          //  sortDescriptions.Add(sortDescription);
          //}
          //tracksViewSource.Source = null;
          //tracksViewSource.Source = trackRows;
          //tracksViewSource.View.SortDescriptions.Clear();
          //foreach (var sortDescription in sortDescriptions) {
          //  tracksViewSource.View.SortDescriptions.Add(sortDescription);
          //}
          TracksDataGrid.SelectedIndex = selectedIndex;
          return;

        }
      }
      System.Diagnostics.Debugger.Break();
    }


    private void saveButton_Click(object sender, RoutedEventArgs e) {
      var storedCount = 0;
      try {
        foreach (var trackRow in trackRows) {
          if (trackRow.IsSelected && !trackRow.IsExisting) {
            trackRow.Track.Store();
            trackRow.IsSelected = false;
            storedCount++;
          }
        };
        Close();
      } catch (Exception ex) {

        MessageWindow.Show(this, "Exception during Save" + Environment.NewLine +
          $"{storedCount} tracks saved." + Environment.NewLine +ex.ToDetailString(), null).Title = "Exception";
      }
    }


    private void memberWindow_Closed(object? sender, EventArgs e) {
      TrackPlayer.TrackChanged -= trackPlayer_TrackChanged;
      Player.Current!.Traced -= importWindow_Traced;

      Owner?.Activate();
    }
    #endregion



    #region Methods
    //      -------

    #endregion
  }
}
