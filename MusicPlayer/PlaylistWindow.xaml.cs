﻿using System;
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
  /// Interaction logic for PlaylistWindow.xaml
  /// </summary>
  public partial class PlaylistWindow: Window {


    #region Constructor
    //      -----------

    public static void Show(
      Window ownerWindow, 
      Playlist playlist, 
      IEnumerable<PlaylistTrack>? additionalTracks = null,
      Action? refreshOwner = null) 
    {
      var window = new PlaylistWindow(playlist, additionalTracks, refreshOwner) { Owner = ownerWindow };
      window.Show();
    }


    readonly Playlist? playlist;
    readonly Action? refreshOwner;
    readonly System.Windows.Data.CollectionViewSource tracksViewSource;
    readonly List<Track> tracks;
    List<TrackRow> trackRows;
    readonly TrackRow? firstAddedTrackRow;
    readonly TimeSpan totalDuration;
    readonly List<string> locations;
    readonly List<DC.AlbumArtistAlbum> albums;
    readonly List<string> artists;
    readonly List<string> genres;
    readonly List<string> years;


    public PlaylistWindow(
      Playlist? playlist = null, 
      IEnumerable<PlaylistTrack>? additionalPlaylistTracks = null, 
      Action? refreshOwner = null) 
    {
      this.playlist = playlist;
      this.refreshOwner = refreshOwner;

      InitializeComponent();

      Width = SystemParameters.PrimaryScreenWidth * .8;
      Height = SystemParameters.PrimaryScreenHeight * .8;

      Loaded += playlistWindow_Loaded;
      Closing += PlaylistWindow_Closing;
      Closed += playlistWindow_Closed;

      //tracks
      tracks = new();
      trackRows = new();
      var trackNo = 0;
      totalDuration = TimeSpan.Zero;
      locations = new();
      albums = new();
      artists = new();
      genres = new();
      years = new();

      if (playlist is not null) {
        //there is usually a playlist, except when VS tries to display PlaylistWindow
        PlaylistNameTextBox.Text = playlist.Name;
        foreach (var playlistTrack in playlist.PlaylistTracks.GetStoredItems().OrderBy(plt => plt.TrackNo)) {
          tracks.Add(playlistTrack.Track);
          var isAdditionalTrack = additionalPlaylistTracks?.Contains(playlistTrack)??false;
          var trackRow = new TrackRow(ref trackNo, playlistTrack, updateSelectedCountTextBox, isAdditionalTrack);
          trackRows.Add(trackRow);
          if (isAdditionalTrack && firstAddedTrackRow is null) {
            firstAddedTrackRow = trackRow;
          }
        }
         DurationTextBox.Text = playlist.TracksDurationHhMm;
        DC.GetTracksStats(ref totalDuration, locations, albums, artists, genres, years, tracks);
      }

      PlaylistNameTextBox.TextChanged += playlistNameTextBox_TextChanged;
      TracksCountTextBox.Text = tracks.Count.ToString();

      //filter
      FilterTextBox.TextChanged += filterTextBox_TextChanged;
      ArtistComboBox.SelectionChanged += filterComboBox_SelectionChanged;
      ArtistComboBox.ItemsSource = artists;
      AlbumComboBox.SelectionChanged += filterComboBox_SelectionChanged;
      AlbumComboBox.ItemsSource = albums;
      AlbumComboBox.DisplayMemberPath = "AlbumArtist";
      GenreComboBox.SelectionChanged += filterComboBox_SelectionChanged;
      GenreComboBox.ItemsSource = genres;
      YearComboBox.SelectionChanged += yearComboBox_SelectionChanged;
      YearComboBox.ItemsSource = years;
      LocationsComboBox.SelectionChanged += filterComboBox_SelectionChanged;
      LocationsComboBox.ItemsSource = locations;
      RemoveCheckBox.Click += checkBox_Click;
      PlaylistCheckBox.Click += checkBox_Click;
      ClearButton.Click += clearButton_Click;
      RemoveAllButton.Click += removeAllButton_Click;
      PLAllButton.Click += plAllButton_Click;
      UnselectAllButton.Click += unselectAllButton_Click;
      ExecuteRemoveButton.Click += executeRemoveButton_Click;
      AddToOtherPlaylistComboBox.ItemsSource = DC.Data.PlaylistStrings.Where(pls => pls!=PlaylistNameTextBox.Text);
      AddToOtherPlaylistButton.Click += addToOtherPlaylistButton_Click;

      //datagrid
      tracksViewSource = ((System.Windows.Data.CollectionViewSource)this.FindResource("TracksViewSource"));
      tracksViewSource.Source = trackRows;
      tracksViewSource.IsLiveSortingRequested = true;
      tracksViewSource.Filter += tracksViewSource_Filter;
      //strangely, it seems both following lines are needed to make sorting work properly
      TracksDataGrid.Columns[0].SortDirection = ListSortDirection.Ascending;
      tracksViewSource.View.SortDescriptions.Add(new SortDescription("PlaylistTrackNo", ListSortDirection.Ascending));
      TracksDataGrid.Sorting += tracksDataGrid_Sorting;
      TracksDataGrid.SelectionChanged += tracksDataGrid_SelectionChanged;
      TracksDataGrid.LayoutUpdated += TracksDataGrid_LayoutUpdated;
      BeginningButton.Click += beginningButton_Click;
      UpPageButton.Click += upPageButton_Click;
      UpRowButton.Click += upRowButton_Click;
      DownRowButton.Click += downRowButton_Click;
      DownPageButton.Click += downPageButton_Click;
      EndButton.Click += endButton_Click;
      SaveButton.Click += saveButton_Click;
      SaveButton.IsEnabled = false;

      //Replaced: TracksDataGrid.MouseDoubleClick += tracksDataGrid_MouseDoubleClick;
      //Style rowStyle = new Style(typeof(DataGridRow));
      //rowStyle.Setters.Add(new EventSetter(DataGridRow.MouseDoubleClickEvent,
      //                         new MouseButtonEventHandler(tracksDataGrid_MouseDoubleClick)));
      //TracksDataGrid.RowStyle = rowStyle;
      TracksDataGrid.RowStyle.Setters.Add(new EventSetter(DataGridRow.MouseDoubleClickEvent,
                               new MouseButtonEventHandler(tracksDataGrid_MouseDoubleClick)));

      TrackPlayer.TrackChanged += trackPlayer_TrackChanged;
      TrackPlayer.Init(getPlayinglist);

      MainWindow.Register(this, "Playlist " + playlist?.Name);
    }
    #endregion


    #region TrackRow Data
    //      -------------

    public class TrackRow: TrackGridRow {
      public PlaylistTrack PlaylistTrack { get; }
      public bool IsAdditionalTrack { get; }
      public int PlaylistTrackNo {
        get {
          return playlistTrackNo;
        }
        set {
          if (playlistTrackNo!=value) {
            playlistTrackNo = value;
            RaisePropertyChanged(nameof(PlaylistTrackNo));
          }
        }
      }
      int playlistTrackNo;
      public int PlaylistTrackNoOld { get; }


      static readonly Brush highlightedBackgroundBrush = new SolidColorBrush(Color.FromRgb(0xDF, 0xF1, 0xFA));


      public TrackRow(ref int trackNo, PlaylistTrack playlistTrack, Action? dataChanged, bool isAdditionalTrack) :
        base(ref trackNo, playlistTrack.Track, dataChanged) {
        PlaylistTrack = playlistTrack;
        IsAdditionalTrack = isAdditionalTrack;
        RowBackground = isAdditionalTrack ? highlightedBackgroundBrush : Brushes.White;
        PlaylistTrackNoOld = PlaylistTrackNo = playlistTrack.TrackNo;
      }

    }
      //    public class TrackRow: INotifyPropertyChanged {
      //      public PlaylistTrack PlaylistTrack { get;}
      //      public Track Track { get; }
      //      public string LocationLower { get; }
      //      public string AlbumLower { get; }
      //      public string ArtistsLower { get; }
      //      public string ComposersLower { get; }
      //      public string PublisherLower { get; }
      //      public string TitleLower { get; }
      //      public bool IsAdditionalTrack { get; }
      //      public Brush RowBackground { get; }
      //      public int PlaylistTrackNo {
      //        get {
      //          return playlistTrackNo;
      //        }
      //        set {
      //          if (playlistTrackNo!=value) {
      //            playlistTrackNo = value;
      //            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlaylistTrackNo)));
      //          }
      //        }
      //      }
      //      int playlistTrackNo;
      //      public int PlaylistTrackNoOld { get; }


      //      /// <summary>
      //      /// Track is marked for removal from playlist
      //      /// </summary>
      //      public bool IsRemoval {
      //        get {
      //          return isRemoval;
      //        }
      //        set {
      //          if (isRemoval!=value) {
      //            isRemoval = value;
      //            //**--updatePlaylistCheckBox();
      //            HasIsRemovalChanged?.Invoke();
      //          }
      //        }
      //      }
      //      bool isRemoval;


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
      //      public event PropertyChangedEventHandler? PropertyChanged;


      //#pragma warning disable CA2211 // Non-constant fields should not be visible
      //      public static Action? HasIsRemovalChanged;
      //      public static Action? HasIsAddPlaylistChanged;
      //#pragma warning restore CA2211

      //      static readonly Brush highlightedBackgroundBrush = new SolidColorBrush(Color.FromRgb(0xDF, 0xF1, 0xFA));


      //      public TrackRow(PlaylistTrack playlistTrack, bool isAdditionalTrack = false) {
      //        PlaylistTrack = playlistTrack;
      //        Track = playlistTrack.Track;
      //        LocationLower = Track.Location.Name.ToLowerInvariant();
      //        ArtistsLower = Track.Artists?.ToLowerInvariant()??"";
      //        AlbumLower = Track.Album?.ToLowerInvariant()??"";
      //        ComposersLower = Track.Composers?.ToLowerInvariant()??"";
      //        PublisherLower = Track.Publisher?.ToLowerInvariant()??"";
      //        TitleLower = Track.Title?.ToLowerInvariant()??"";
      //        IsAdditionalTrack = isAdditionalTrack;
      //        RowBackground = isAdditionalTrack ? highlightedBackgroundBrush : Brushes.White;
      //        PlaylistTrackNoOld = PlaylistTrackNo = playlistTrack.TrackNo;
      //        isRemoval = false;
      //        isAddPlaylist = false;
      //      }
      //    }
      #endregion


      #region Events
      //      ------

    private void playlistWindow_Loaded(object sender, RoutedEventArgs e) {
      PlaylistNameTextBox.Focus();
      if (firstAddedTrackRow is not null) {
        TracksDataGrid.ScrollIntoView(firstAddedTrackRow);
      }

      AddToOtherPlaylistComboBox.SelectionChanged += addToOtherPlaylistComboBox_SelectionChanged;
      AddToOtherPlaylistComboBox.AddHandler(TextBoxBase.TextChangedEvent, new RoutedEventHandler(addToOtherPlaylistComboBox_TextChanged));
      AddToOtherPlaylistComboBox.LostFocus += addToOtherPlaylistComboBox_LostFocus;
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
      isClearing = false;

      tracksViewSource.View.SortDescriptions.Clear();
      tracksViewSource.View.Refresh();
    }


    private void tracksViewSource_Filter(object sender, FilterEventArgs e) {
      var trackRow = (TrackRow)e.Item;
      var isAccepted = false;
      var isRefused = false;
      if (filterText.Length>2) {
        if (trackRow.Track.TitleLowerCase?.Contains(filterText)??false) {
          isAccepted = true;
        } else if (trackRow.Track.ArtistsLowerCase?.Contains(filterText)??false) {
          isAccepted = true;
        } else if (trackRow.Track.AlbumLowerCase?.Contains(filterText)??false) {
          isAccepted = true;
        } else if (trackRow.Track.ComposersLowerCase?.Contains(filterText)??false) {
          isAccepted = true;
        } else if (trackRow.Track.PublisherLowerCase?.Contains(filterText)??false) {
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

      //if no filter is active (=!isRefused) OR any filter does accept the item, then accept, i.e. OR conidion.
      isAccepted = !isRefused || isAccepted;

      //only accept item which match IsRemoval
      if (RemoveCheckBox.IsChecked is not null) {
        if (trackRow.IsDeletion!=RemoveCheckBox.IsChecked) {
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

    private void removeAllButton_Click(object sender, RoutedEventArgs e) {
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


    private void addToOtherPlaylistComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      if (e.AddedItems.Count>0) {
        var addedItem = e.AddedItems[0];
        if (addedItem is not null) {
          updatePlaylist(addedItem.ToString());
          return;
        }
      }
      updatePlaylist(AddToOtherPlaylistComboBox.Text);
    }


    private void addToOtherPlaylistComboBox_LostFocus(object sender, RoutedEventArgs e) {
      updatePlaylist(AddToOtherPlaylistComboBox.Text);
    }


    private void addToOtherPlaylistComboBox_TextChanged(object sender, RoutedEventArgs e) {
      updatePlaylist(AddToOtherPlaylistComboBox.Text);
    }


    bool hasOtherPlaylistName;
    Playlist? otherPlaylist;


    private void updatePlaylist(string? playlistName) {
      var hasPlaylistNameNew = !string.IsNullOrEmpty(playlistName);
      AddToOtherPlaylistButton.IsEnabled = PLAllButton.IsEnabled = hasPlaylistNameNew;
      Playlist? playlistNew;
      if (!hasPlaylistNameNew) {
        playlistNew = null;
      } else {
        DC.Data.PlaylistsByNameLower.TryGetValue(playlistName!.ToLowerInvariant(), out playlistNew);
      }

      if (hasOtherPlaylistName!=hasPlaylistNameNew || otherPlaylist!=playlistNew) {
        hasOtherPlaylistName = hasPlaylistNameNew;
        otherPlaylist = playlistNew;
        updatePlaylistCheckBoxes();
      }
    }


    private void updatePlaylistCheckBoxes() {
      foreach (var item in TracksDataGrid.Items) {
        var trackRow = (TrackRow)item;
        trackRow.UpdatePlaylistCheckBox(otherPlaylist, hasOtherPlaylistName);
      }
      updateSelectedCountTextBox();
    }


    int removalCount;


    //**--
    private void updateSelectedCountTextBox() {
      removalCount = 0;
      var existPlaylistCount = 0;
      var addPlaylistCount = 0;
      foreach (var trackRow in trackRows) {
        if (trackRow.IsDeletion) {
          removalCount++;
        } else {
          if (hasOtherPlaylistName) {
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
      SelectedCountTextBox.Text = $"Remove: {removalCount}, PL exist: {existPlaylistCount}, new: {addPlaylistCount}";
      updateExecuteRemoveButton();
    }


    private void executeRemoveButton_Click(object sender, RoutedEventArgs e) {
      var result = MessageBox.Show($"Do you want to remove {removalCount} track(s) from playlist {playlist!.Name}?", "Track Removal", MessageBoxButton.YesNo,
        MessageBoxImage.Question, MessageBoxResult.No);
      if (result==MessageBoxResult.Yes) {
        Track? playingTrack = null;
        if (Player.Current!.Playinglist?.Playlist==playlist) {
          playingTrack = Player.Current.Track;
        }
        var isSkipTrackNeeded = false;
        var remainingTrackRows = new List<TrackRow>(trackRows.Count);
        foreach (var trackRow in trackRows) {
          if (trackRow.IsDeletion) {

            if (playingTrack is not null && playingTrack==trackRow.Track) {
              isSkipTrackNeeded = true;
            }
            PlaylistTrack? removePlaylistTrack = null;
            foreach (var playlistsTrack in trackRow.Track.PlaylistTracks) {
              if (playlistsTrack.Playlist==playlist) {
                removePlaylistTrack = playlistsTrack;//cannot release playlistsTrack within foreach
                break; //a track can be included only once in a playlist
              }
            }
            removePlaylistTrack?.Release();
          } else {
            remainingTrackRows.Add(trackRow);
          }
        }
        if (isSkipTrackNeeded) {
          TrackPlayer.PlayNextTrack();
        }
        remainingTrackRows.Sort((tr1, tr2) => { return tr1.PlaylistTrack.TrackNo.CompareTo(tr2.PlaylistTrack.TrackNo); });
        var trackNo = 0;
        foreach (var trackRow in remainingTrackRows) {
          var playlistTrack = trackRow.PlaylistTrack;
          playlistTrack.Update(playlistTrack.Playlist, playlistTrack.Track, trackNo++);
        }
        tracksViewSource.Source = trackRows = remainingTrackRows;
        DurationTextBox.Text = playlist.TracksDurationHhMm;
        TracksCountTextBox.Text = tracks.Count.ToString();
        MainWindow.Current!.UpdatePlaylistsDataGrid();
        MainWindow.Current!.UpdatePlaylistDataGrid(playlist);
      }
    }


    private void addToOtherPlaylistButton_Click(object sender, RoutedEventArgs e) {
      var playlistName = AddToOtherPlaylistComboBox.Text;
      if (string.IsNullOrEmpty(playlistName)) {
        MessageWindow.Show(this, "Provide name for playlist");
        return;
      }
      if (!DC.Data.PlaylistsByNameLower.TryGetValue(playlistName.ToLower(), out var playlist)) {
        playlist = new Playlist(playlistName);
      }
      var playlistTracks = new List<PlaylistTrack>();
      var trackNo = playlist.PlaylistTracks.Count;
      foreach (var item in TracksDataGrid.Items) {
        var trackRow = (TrackRow)item;
        if (trackRow.PlaylistCheckBoxIsEnabled &&  trackRow.IsAddPlaylist) {
          playlistTracks.Add(new PlaylistTrack(playlist, trackRow.Track, trackNo++));
        }
      }
      MainWindow.Current!.UpdatePlaylistsDataGrid();
      MainWindow.Current.UpdatePlaylistDataGrid(playlist);
      PlaylistWindow.Show(this, playlist, playlistTracks);
    }
    #endregion


    #region DataGrid events
    //      ---------------

    private void tracksDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
      var playinglist = getPlayinglist();
      if (playinglist is not null) {
        TrackPlayer.Play(playinglist);
      }
    }


    Track? playingTrack;


    private void trackPlayer_TrackChanged(Track? track) {
      playingTrack = track;
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
        trackRow.UpdatePlaylistCheckBox(playlist, hasOtherPlaylistName);
      }
      tracksViewSource.View.Refresh();
    }


    bool isFirstColSorting = true;


    private void tracksDataGrid_Sorting(object sender, DataGridSortingEventArgs e) {
      isFirstColSorting = (string)e.Column.Header=="#";
      updateButtonsEnabled();
    }


    bool areContiousTracksSelected = true;


    private void tracksDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      var selectedItemsCount = TracksDataGrid.SelectedItems.Count;
      if (selectedItemsCount<=1) {
        areContiousTracksSelected = selectedItemsCount==1;
      } else {
        var minPLTrackNo = int.MaxValue;
        var maxPLTrackNo = int.MinValue;
        foreach (var item in TracksDataGrid.SelectedItems) {
          var trackRow = (TrackRow)item;
          if (minPLTrackNo>trackRow.PlaylistTrackNo) {
            minPLTrackNo = trackRow.PlaylistTrackNo;
          }
          if (maxPLTrackNo<trackRow.PlaylistTrackNo) {
            maxPLTrackNo = trackRow.PlaylistTrackNo;
          }
        }
        areContiousTracksSelected = maxPLTrackNo - minPLTrackNo == selectedItemsCount - 1;
      }
      updateButtonsEnabled();
    }


    private void updateButtonsEnabled() {
      var isMovingTracksPossible = isFirstColSorting && areContiousTracksSelected;
      BeginningButton.IsEnabled = isMovingTracksPossible;
      UpPageButton.IsEnabled = isMovingTracksPossible;
      UpRowButton.IsEnabled = isMovingTracksPossible;
      DownRowButton.IsEnabled = isMovingTracksPossible;
      DownPageButton.IsEnabled = isMovingTracksPossible;
      EndButton.IsEnabled = isMovingTracksPossible;
    }


    enum scrollEnum { All, Page, Row};
    const int rowsPerPage = 20;


    private void beginningButton_Click(object sender, RoutedEventArgs e) {
      moveSelectedTracksUp(scrollEnum.All);
    }


    private void upPageButton_Click(object sender, RoutedEventArgs e) {
      moveSelectedTracksUp(scrollEnum.Page);
    }


    private void upRowButton_Click(object sender, RoutedEventArgs e) {
      moveSelectedTracksUp(scrollEnum.Row);
    }


    private void downRowButton_Click(object sender, RoutedEventArgs e) {
      moveSelectedTracksDown(scrollEnum.Row);
    }


    private void downPageButton_Click(object sender, RoutedEventArgs e) {
      moveSelectedTracksDown(scrollEnum.Page);
    }


    private void endButton_Click(object sender, RoutedEventArgs e) {
      moveSelectedTracksDown(scrollEnum.All);
    }


    private void moveSelectedTracksUp(scrollEnum scrollAmount) {
      (int firstSelectedTrack, _, int selectedTracksCount) = getSelectedFirstLast();
      if (firstSelectedTrack<=0) return;//cannot move up any further

      int firstMoveTrack;
      int moveTracksCount;
      switch (scrollAmount) {
      case scrollEnum.All:
        firstMoveTrack = 0;
        moveTracksCount = firstSelectedTrack;
        break;
      case scrollEnum.Page:
        firstMoveTrack = Math.Max(0, firstSelectedTrack - rowsPerPage);
        moveTracksCount = Math.Min(rowsPerPage, firstSelectedTrack - firstMoveTrack);
        break;
      case scrollEnum.Row:
        firstMoveTrack = firstSelectedTrack-1;
        moveTracksCount = 1;
        break;
      default: throw new NotSupportedException();
      }
      isScrollUpNeeded = true;
      moveTracksDown(firstMoveTrack, moveTracksCount, selectedTracksCount);
      moveTracksUp(firstSelectedTrack, selectedTracksCount, moveTracksCount);
      updateHasRankChanged();
      //TracksDataGrid.UpdateLayout();
      //TracksDataGrid.ScrollIntoView(TracksDataGrid.SelectedItem); //doesn't work :-(

      ////tracksViewSource.View.Refresh();
      ////selectAndBringInView(firstSelectedTrack - moveTracksCount, selectedTracksCount);
    }


    private void moveSelectedTracksDown(scrollEnum scrollAmount) {
      (int firstSelectedTrack, int lastSelectedTrack, int selectedTracksCount) = getSelectedFirstLast();
      if (lastSelectedTrack + 1 >= TracksDataGrid.Items.Count) return;//cannot move down any further

      int lastMoveTrack;
      int moveTracksCount;
      switch (scrollAmount) {
      case scrollEnum.All:
        lastMoveTrack = TracksDataGrid.Items.Count-1;
        moveTracksCount = lastMoveTrack - lastSelectedTrack;
        break;
      case scrollEnum.Page:
        lastMoveTrack = Math.Min(TracksDataGrid.Items.Count-1, lastSelectedTrack + rowsPerPage);
        moveTracksCount = Math.Min(rowsPerPage, lastMoveTrack - lastSelectedTrack);
        break;
      case scrollEnum.Row:
        lastMoveTrack = lastSelectedTrack + 1;
        moveTracksCount = 1;
        break;
      default: throw new NotSupportedException();
      }
      isScrollDownNeeded = true;
      moveTracksUp(lastMoveTrack - moveTracksCount + 1, moveTracksCount, selectedTracksCount);
      moveTracksDown(firstSelectedTrack, selectedTracksCount, moveTracksCount);
      updateHasRankChanged();
      //TracksDataGrid.ScrollIntoView(TracksDataGrid.SelectedItem); //doesn't work :-(

      ////tracksViewSource.View.Refresh();
      ////selectAndBringInView(firstSelectedTrack + moveTracksCount, selectedTracksCount);
    }


    private (int firstSelectedTrack, int lastSelectedTrack, int selectedTracksCount) getSelectedFirstLast() {
      var minPLTrackNo = int.MaxValue;
      var maxPLTrackNo = int.MinValue;
      //var x = DataGridRow.GetRowContainingElement(cell);
      foreach (var item in TracksDataGrid.SelectedItems) {
        var trackRow = (TrackRow)item;
        if (minPLTrackNo>trackRow.PlaylistTrackNo) {
          minPLTrackNo = trackRow.PlaylistTrackNo;
        }
        if (maxPLTrackNo<trackRow.PlaylistTrackNo) {
          maxPLTrackNo = trackRow.PlaylistTrackNo;
        }
      }
      var count = maxPLTrackNo - minPLTrackNo + 1;
      #pragma warning disable IDE0046 // Convert to conditional expression
      if (count != TracksDataGrid.SelectedItems.Count) throw new Exception("Selected tracks are not continuous, they cannot be moved.");
      #pragma warning restore IDE0046

      return (minPLTrackNo, maxPLTrackNo, count);
    }


    private void moveTracksDown(int firstTrack, int tracksCount, int offset) {
      for (int itemIndex = firstTrack; itemIndex<firstTrack+tracksCount; itemIndex++) {
        TrackRow trackRow = (TrackRow)TracksDataGrid.Items[itemIndex]!;
        trackRow.PlaylistTrackNo += offset;
      }
    }


    private void moveTracksUp(int firstTrack, int tracksCount, int offset) {
      for (int itemIndex = firstTrack; itemIndex<firstTrack+tracksCount; itemIndex++) {
        TrackRow trackRow = (TrackRow)TracksDataGrid.Items[itemIndex]!;
        trackRow.PlaylistTrackNo -= offset;
      }
    }


    bool hasRankChanged;


    private void updateHasRankChanged() {
      var newHasRankChanged = false;
      foreach (var trackRow in trackRows) {
        if (trackRow.PlaylistTrackNo!=trackRow.PlaylistTrackNoOld) {
          newHasRankChanged = true;
          break;
        }
      };
      if (hasRankChanged!=newHasRankChanged) {
        hasRankChanged = newHasRankChanged;
        RemoveAllButton.IsEnabled = !hasRankChanged;
        updateExecuteRemoveButton();
        updateSaveButton();
      }
    }


    private void updateExecuteRemoveButton() {
      ExecuteRemoveButton.IsEnabled = !hasRankChanged && removalCount>0;
    }


    ////////https://social.technet.microsoft.com/wiki/contents/articles/21202.wpf-programmatically-selecting-and-focusing-a-row-or-cell-in-a-datagrid.aspx
    //////private void selectAndBringInView(int firstTrack, int tracksCount) {
    //////  TracksDataGrid.SelectedItems.Clear();
    //////  for (int itemIndex = firstTrack; itemIndex<firstTrack+tracksCount; itemIndex++) {
    //////    object item = TracksDataGrid.Items[itemIndex];
    //////    TracksDataGrid.SelectedItems.Add(item);
    //////    DataGridRow? row = TracksDataGrid.ItemContainerGenerator.ContainerFromIndex(itemIndex) as DataGridRow;
    //////    if (row is null) {
    //////      TracksDataGrid.ScrollIntoView(item);
    //////      row = TracksDataGrid.ItemContainerGenerator.ContainerFromIndex(itemIndex) as DataGridRow;
    //////    }
    //////    if (row is not null) {
    //////      DataGridCell? cell = GetCell(TracksDataGrid, row, 0);
    //////      if (cell is not null)
    //////        cell.Focus();
    //////    }
    //////  }
    //////}


    //////public static DataGridCell? GetCell(DataGrid dataGrid, DataGridRow rowContainer, int column) {
    //////  if (rowContainer is not null) {
    //////    DataGridCellsPresenter? presenter = FindVisualChild<DataGridCellsPresenter>(rowContainer);
    //////    if (presenter is null) {
    //////      /* if the row has been virtualized away, call its ApplyTemplate() method
    //////       * to build its visual tree in order for the DataGridCellsPresenter
    //////       * and the DataGridCells to be created */
    //////      rowContainer.ApplyTemplate();
    //////      presenter = FindVisualChild<DataGridCellsPresenter>(rowContainer);
    //////    }
    //////    if (presenter is not null) {
    //////      if (presenter.ItemContainerGenerator.ContainerFromIndex(column) is not DataGridCell cell) {
    //////        /* bring the column into view
    //////         * in case it has been virtualized away */
    //////        dataGrid.ScrollIntoView(rowContainer, dataGrid.Columns[column]);
    //////        return presenter.ItemContainerGenerator.ContainerFromIndex(column) as DataGridCell;
    //////      }
    //////      return cell;
    //////    }
    //////  }
    //////  return null;
    //////}


    //////public static T? FindVisualChild<T>(DependencyObject obj) where T : DependencyObject {
    //////  for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++) {
    //////    DependencyObject? child = VisualTreeHelper.GetChild(obj, i);
    //////    if (child is not null) {
    //////      if (child is T t)
    //////        return t;
    //////      else {
    //////        T? childOfChild = FindVisualChild<T>(child);
    //////        if (childOfChild is not null)
    //////          return childOfChild;
    //////      }
    //////    }
    //////  }
    //////  return null;
    //////}


    bool isScrollDownNeeded;
    bool isScrollUpNeeded;


    private void TracksDataGrid_LayoutUpdated(object? sender, EventArgs e) {
      if (isScrollUpNeeded) {
        isScrollUpNeeded = false;
        (int firstSelectedTrack, _, _) = getSelectedFirstLast();
        firstSelectedTrack = Math.Max(0, firstSelectedTrack-3);
        TracksDataGrid.ScrollIntoView(TracksDataGrid.Items[firstSelectedTrack]);
      }
      if (isScrollDownNeeded) {
        isScrollDownNeeded = false;
        (_, int lastSelectedTrack, _) = getSelectedFirstLast();
        lastSelectedTrack = Math.Min(TracksDataGrid.Items.Count-1, lastSelectedTrack+3);
        TracksDataGrid.ScrollIntoView(TracksDataGrid.Items[lastSelectedTrack]);
      }
    }
    #endregion


    bool hasPlaylistNameChanged;

    private void playlistNameTextBox_TextChanged(object sender, TextChangedEventArgs e) {
      var newHasPlaylistNameChanged = (playlist?.Name??"")!=PlaylistNameTextBox.Text;
      if (hasPlaylistNameChanged!=newHasPlaylistNameChanged) {
        hasPlaylistNameChanged = newHasPlaylistNameChanged;
        updateSaveButton();
      }
    }


    private void updateSaveButton() {
      SaveButton.IsEnabled = hasRankChanged || hasPlaylistNameChanged;
    }


    private void saveButton_Click(object sender, RoutedEventArgs e) {
      save();
      Close();
    }


    private void save() {
      if (playlist is not null) {
        if (playlist.Name!=PlaylistNameTextBox.Text) {
          playlist.Update(PlaylistNameTextBox.Text);
        }
        if (hasRankChanged) {
          foreach (var trackRow in trackRows) {
            if (trackRow.PlaylistTrackNo!=trackRow.PlaylistTrackNoOld) {
              trackRow.PlaylistTrack.Update(playlist, trackRow.Track, trackRow.PlaylistTrackNo);
            }
          }
        }
      }
      hasPlaylistNameChanged = false;
      hasRankChanged = false;
    }


    private void PlaylistWindow_Closing(object sender, CancelEventArgs e) {
      string message;
      if (hasRankChanged) {
        message =hasPlaylistNameChanged ? "Playlist name and sequence of tracks have changed." : "Sequence of tracks have changed.";
      } else {
        if (hasPlaylistNameChanged) {
          message = "Playlist name has changed.";
        } else {
          return;
        }
      }

      var answer = MessageBox.Show(message + Environment.NewLine + 
        "Press Yes to save changes and close window." + Environment.NewLine +
        "Press No to discard changes and close window." + Environment.NewLine +
        "Press Cancel to keep window open.", 
        "Closing window. Should changes be saved ?", 
        MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);
      switch (answer) {
      case MessageBoxResult.None:
      case MessageBoxResult.Cancel:
        e.Cancel = true;
        break;
      case MessageBoxResult.Yes:
        save();
        break;
      case MessageBoxResult.No:
        break;
      default:
        throw new NotSupportedException();
      }
    }


    private void playlistWindow_Closed(object? sender, EventArgs e) {
      TrackPlayer.TrackChanged -= trackPlayer_TrackChanged;
      Owner?.Activate();
      refreshOwner?.Invoke();
    }
    #endregion


    #region Methods
    //      -------

    Playinglist? playinglist;


    private Playinglist? getPlayinglist() {
      if (TracksDataGrid.SelectedItems.Count==0) {
        System.Diagnostics.Debugger.Break();
        return null;

      } else if (TracksDataGrid.SelectedItems.Count==1) {
        if (playingTrack==((TrackRow)TracksDataGrid.SelectedItem).Track) {
          //in the grid selected track is already playing, just return the previously created playinglist
          return playinglist;
        }

        var tracks = new List<Track>();
        for (int rowIndex = TracksDataGrid.SelectedIndex; rowIndex<TracksDataGrid.Items.Count; rowIndex++) {
          tracks.Add(((TrackRow)TracksDataGrid.Items[rowIndex]).Track);
        }
        for (int rowIndex = 0; rowIndex<TracksDataGrid.SelectedIndex; rowIndex++) {
          tracks.Add(((TrackRow)TracksDataGrid.Items[rowIndex]).Track);
        }
        playinglist = new Playinglist(tracks);
        foreach (var item in TracksDataGrid.Items) {
          var trackRow = (TrackRow)item;
          trackRow.RowBackground = Brushes.White;
        }
        return playinglist;

      } else {
        foreach (var item in TracksDataGrid.Items) {
          var trackRow = (TrackRow)item;
          trackRow.RowBackground = Brushes.White;
        }
        foreach (var item in TracksDataGrid.SelectedItems) {
          var trackRow = (TrackRow)item;
          trackRow.RowBackground = Brushes.LightBlue;
        }
        var trackQuery =
          from object gridItem in TracksDataGrid.SelectedItems
          select ((TrackRow)gridItem).Track;
        playinglist = new Playinglist(trackQuery);
        return playinglist;
      }
    }
    #endregion
  }
}
