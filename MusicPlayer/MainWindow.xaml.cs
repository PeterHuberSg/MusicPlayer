using StorageLib;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfWindowsLib;

namespace MusicPlayer {

  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow: Window {

    #region Properties
    //      ----------

    public static MainWindow? Current { get; private set; }


    public bool IsProductionDC { get; private set; }


    public Brush StatusBarBackgroundNormal { get; }


    public Brush StatusBarBackgroundTest { get; }
    #endregion


    #region Constructor
    //      -----------

    readonly System.Windows.Data.CollectionViewSource playListsViewSource;
    readonly System.Windows.Data.CollectionViewSource playListViewSource;


    public MainWindow() {
      Current = this;
      _ = new Player();

      InitializeComponent();

      addMenus();

      DcModeComboBox.SelectedIndex = 0;
      DcModeComboBox.SelectionChanged += dcModeComboBox_SelectionChanged;

      Loaded += mainWindow_Loaded;
      Closed += mainWindow_Closed;

      //playlists datagrid
      playListsViewSource = ((System.Windows.Data.CollectionViewSource)this.FindResource("PlayListsViewSource"));
      //playListViewSource.Filter += tracksViewSource_Filter;
      PlayListsDataGrid.MouseDoubleClick += playListsDataGrid_MouseDoubleClick;
      var contextMenu = new ContextMenu();
      var deletePlaylistMenuItem = new MenuItem { Header = "_Delete Playlist" };
      deletePlaylistMenuItem.Click += deletePlaylistMenuItem_Click;
      contextMenu.Items.Add(deletePlaylistMenuItem);
      PlayListsDataGrid.ContextMenu = contextMenu;

      //playlist datagrid
      playListViewSource = ((System.Windows.Data.CollectionViewSource)this.FindResource("PlayListViewSource"));

      TrackPlayer.Init(getPlayinglist);
      TrackPlayer.TrackChanged += TrackPlayer_TrackChanged;

      StatusBarBackgroundNormal = MainStatusBar.Background;
      StatusBarBackgroundTest = Brushes.LightCoral;
    }


    Playinglist? playinglist;


    private Playinglist? getPlayinglist() {
      Playlist playlist = (Playlist)PlayListsDataGrid.SelectedItem;
      if (!DC.Data.Playinglists.TryGetValue(playlist, out playinglist)) {
        playinglist = new Playinglist(playlist.Tracks);
      }
      return playinglist;
    }


    private void TrackPlayer_TrackChanged(Track? track) {
      playListViewSource.Source =
        playinglist!.ToPlayTracks.Cast<PlayinglistItemPlaylistTrack>().
        OrderBy(plt => plt.PlaylistTrack.TrackNo).
        Select(plt => plt.PlaylistTrack).ToList();
    }


    private void deletePlaylistMenuItem_Click(object sender, RoutedEventArgs e) {
      Playlist playlist = (Playlist)PlayListsDataGrid.SelectedItem;
      if (MessageBox.Show($"Delete {playlist.Name} ?", "Delete Playlist", MessageBoxButton.YesNo, MessageBoxImage.Question)
        ==MessageBoxResult.Yes) 
      {
        foreach (var playlistTrack in playlist.Tracks) {
          playlistTrack.Release();
        }
        playlist.Release();
        refreshPlaylistDataGrid();
      }
    }
    #endregion


    //public class PlayListTrack {
    //  public Track Track { get; }
    //  public int No { get; }
    //  public string? Title { get; }
    //  public string? Duration { get; }
    //  public string? Album { get; }
    //  public string? Artists { get; }
    //  public string? Composers { get; }
    //  public string? Genres { get; }
    //  public string? Publisher { get; }
    //  public string? Year { get; }

    //  public PlayListTrack(int no, Track track){
    //    Track = track;
    //    No = no;
    //    Title = track.Title is null ? track.FileName : track.Title;
    //    Duration = track.Duration is null ? null : $"{(int)track.Duration.Value.TotalMinutes}:{track.Duration.Value.Seconds:00}";
    //    Album = track.Album;
    //    Artists = track.Artists;
    //    Composers = track.Composers;
    //    Genres = track.Genres;
    //    Publisher = track.Publisher;
    //    Year = track.Year?.ToString("0000");
    //  }
    //}


    #region Eventhandlers
    //      -------------

    private void mainWindow_Loaded(object sender, RoutedEventArgs e) {

      dlInitAsync();
    }


    private void mainWindow_Closed(object? sender, EventArgs e) {
      DC.DisposeData();
    }


    private void playListsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
      var selectedIndex = PlayListsDataGrid.SelectedIndex;
      if (selectedIndex<0) return;

      var dataGridCell = ((DependencyObject)e.OriginalSource).FindVisualParentOfType<DataGridCell>();
      if (dataGridCell is null) return;

      //if (dataGridCell.Column.DisplayIndex==10) return;

      //this.TrackPlayer.Play(((TrackRow)TracksDataGrid.Items[selectedIndex]).Track);
      PlaylistWindow.Show(this, ((Playlist)PlayListsDataGrid.Items[selectedIndex]), null, RefreshPlaylistDataGrid);
    }


    public void RefreshPlaylistDataGrid(Playlist playlist) {
      refreshPlaylistDataGrid();
    }


    private void refreshPlaylistDataGrid() {
      updateStatistics();
      playListsViewSource.Source = DC.Data.Playlists.Values.OrderBy(pl => pl.Name).ToList();
    }
    #endregion


    #region Menu
    //      ----

    MenuItem? windowsMenu;


    private void addMenus() {
      _ = addSubMenu(MainMenu, "_Tracks", () => TracksWindow.Show(this), isEnabled: true, isOneTime: false);
      _ = addSubMenu(MainMenu, "_Import", () => ImportWindow.Show(this, refreshPlaylistDataGrid), isEnabled: true, isOneTime: true);

      windowsMenu = addSubMenu(MainMenu, "_Windows");
    }


    MenuItem addSubMenu(
      ItemsControl parentMenu,
      string header,
      Func<Window?>? menuCommand = null,
      bool isEnabled = true,
      bool isOneTime = false) 
    {
      MenuItem newMenuItem = new MenuItem{
        Header = header,
        IsEnabled = isEnabled,
        Tag = (menuCommand, isOneTime)
      };
      if (menuCommand!=null) {
        newMenuItem.Click += menuItem_Click;
      }
      parentMenu.Items.Add(newMenuItem);
      return newMenuItem;
    }


    readonly Dictionary<Window, MenuItem> oneTimeMenuItems = new Dictionary<Window, MenuItem>();


    void menuItem_Click(object sender, RoutedEventArgs e) {
      //GC.Collect();
      //GC.WaitForPendingFinalizers();
      //GC.WaitForFullGCComplete();
      //GC.Collect();


      MenuItem menuItem = (MenuItem)sender;
      var (menuCommand, isOneTime) = ((Func<Window?>?, bool))menuItem.Tag;
      var window = menuCommand!();
      if (window!=null && isOneTime) {
        menuItem.IsEnabled = false;
        oneTimeMenuItems.Add(window, menuItem);
      }
    }
    #endregion


    #region Windows registration
    //      --------------------

    readonly Dictionary<Window, (MenuItem MenuItem, int Number)> windowsSubMenus = new Dictionary<Window, (MenuItem, int)>();
    readonly Dictionary<Type, BigBitSetAuto> windowsNummern = new Dictionary<Type, BigBitSetAuto>();


    public static void Register(Window window, string name) {
      Current!.register(window, name);
    }


    private void register(Window childWindow, string name) {
      var windowType = childWindow.GetType();
      if (!windowsNummern.TryGetValue(windowType, out var windowNummern)) {
        windowNummern = new BigBitSetAuto();
        windowsNummern.Add(windowType, windowNummern);
      }
      var nextNummer = 0;
      while (windowNummern[nextNummer]) {
        nextNummer++;
      }
      windowNummern[nextNummer] = true;
      if (nextNummer!=0) {
        name = name + " " + nextNummer;
      }
      childWindow.Title = name;

      var windowsSubMenu = addSubMenu(windowsMenu!, name, () => { childWindow.Activate(); childWindow.WindowState = WindowState.Normal; childWindow.BringIntoView(); return null; });

      windowsSubMenus.Add(childWindow, (windowsSubMenu, nextNummer));
      childWindow.Closed += childWindow_Closed;
    }


    private void childWindow_Closed(object? sender, EventArgs e) {
      var senderWindow = (Window)sender!;
      var menuItemNumber = windowsSubMenus[senderWindow];
      windowsMenu!.Items.Remove(menuItemNumber.MenuItem);
      DcModeComboBox.IsEnabled = windowsMenu.Items.Count==0;
      var menuNummer = menuItemNumber.Number;
      windowsSubMenus.Remove((Window)sender!);
      var windowType = senderWindow.GetType();
      windowsNummern[windowType][menuNummer] = false;
      if (oneTimeMenuItems.TryGetValue(senderWindow, out var menuItem)) {
        menuItem.IsEnabled = true;
        oneTimeMenuItems.Remove(senderWindow);
      }
    }
    #endregion



    #region Real or TestData
    //      ----------------

    bool isSkipNextEvent = false;


    private void dcModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      if (isSkipNextEvent) {
        isSkipNextEvent = false;
        return;
      }
      bool isProductionDb = RealComboBoxItem.IsSelected;
      var questionString = isProductionDb
          ? "Do you want to switch from Debug to Production database ?"
          : "Do you want to switch from Production to Debug database ?";
      if (MessageBox.Show("Switch database ?", questionString, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No)
        ==MessageBoxResult.Yes) 
      {
        //stocksViewSource.Source = null;
        dlInitAsync();
      } else {
        isSkipNextEvent = true;
        if (RealComboBoxItem.IsSelected) {
          TestComboBoxItem.IsSelected = true;
        } else {
          RealComboBoxItem.IsSelected = true;
        }
      }
    }


    private async void dlInitAsync() {
      MainMenu.IsEnabled = false;
      IsProductionDC = RealComboBoxItem.IsSelected;
      MainStatusBar.Background = IsProductionDC ? StatusBarBackgroundNormal : StatusBarBackgroundTest;
      PathTextBox.Text = IsProductionDC ? DC.CsvFilePath + "; " + DC.BackupFilePath : DC.CsvTestFilePath;

      //Progress<string> progress = new Progress<string>(s => EventsTextBox.Text += Environment.NewLine + s);
      await Task.Run(() => dlInit(IsProductionDC, null));
      playListsViewSource.Source = DC.Data.Playlists.Values.OrderBy(pl => pl.Name).ToList();
      MainMenu.IsEnabled = true;
      updateStatistics();
    }



    private void updateStatistics() {
      StatusTextBox.Text = $"{DC.Data.Tracks.Count} tracks; {(int)DC.Data.TotalDuration.TotalHours} hours";
    }


    public void SetBackground(Panel panel) {
      panel.Background =IsProductionDC ? StatusBarBackgroundNormal : StatusBarBackgroundTest;
    }


    private static void dlInit(bool isProduction, IProgress<string>? progress) {
      if (DC.Data!=null) {
        progress?.Report("Data dispose");
        DC.Data.Dispose();
      }

      progress?.Report("Data initialising");
      CsvConfig csvConfig;
      if (isProduction) {
        csvConfig = new CsvConfig(DC.CsvFilePath, DC.BackupFilePath, backupPeriodicity: 1, backupCopies: 8);
      } else {
        foreach (var fileInfo in new DirectoryInfo(DC.CsvTestFilePath).GetFiles()) {
          fileInfo.Delete();
        }
        foreach (var fileInfo in new DirectoryInfo(DC.CsvFilePath).GetFiles()) {
          fileInfo.CopyTo(DC.CsvTestFilePath + "\\" + fileInfo.Name);
        }
        csvConfig = new CsvConfig(DC.CsvTestFilePath);
      }
      _ = new DC(csvConfig);
      progress?.Report("Data initialised");
    }
    #endregion
  }
}
