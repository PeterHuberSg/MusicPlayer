using Storage;
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




    ////readonly MediaPlayer mediaPlayer;
    TrackList trackList;
    int trackListIndex;
    readonly System.Windows.Data.CollectionViewSource playListViewSource;
    List<PlayListTrack> playListTracks;
    bool isManualPlay;


    public MainWindow() {
      if (Current!=null) throw new Exception();

      Current = this;
      _ = new Player();

      InitializeComponent();

      addMenus();

      ////PauseToggleButton.Checked += pauseToggleButton_Checked;
      ////PauseToggleButton.Unchecked += pauseToggleButton_Unchecked;
      ////NextButton.Click += nextButton_Click;
      ////MuteToggleButton.Checked += muteToggleButton_Checked;
      ////MuteToggleButton.Unchecked += muteToggleButton_Unchecked;
      PlayListDataGrid.MouseDoubleClick += playListDataGrid_MouseDoubleClick;

      DcModeComboBox.SelectedIndex = 0;
      DcModeComboBox.SelectionChanged += DcModeComboBox_SelectionChanged;

      Loaded += mainWindow_Loaded;
      //VolumeScrollBar.ValueChanged += volumeScrollBar_ValueChanged;

      StatusBarBackgroundNormal = StatusDockPanel.Background;
      StatusBarBackgroundTest = Brushes.LightCoral;

      ////mediaPlayer = new MediaPlayer();
      ////mediaPlayer.BufferingStarted += mediaPlayer_BufferingStarted;
      ////mediaPlayer.BufferingEnded += mediaPlayer_BufferingEnded;
      ////mediaPlayer.MediaOpened += mediaPlayer_MediaOpened;
      ////mediaPlayer.MediaFailed += mediaPlayer_MediaFailed;
      ////mediaPlayer.MediaEnded += mediaPlayer_MediaEnded;
      ////mediaPlayer.ScriptCommand += mediaPlayer_ScriptCommand;

      ////var dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
      ////dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
      ////dispatcherTimer.Interval = TimeSpan.FromMilliseconds(100);
      ////dispatcherTimer.Start();

      //trackList = new TrackList(@"E:\Musig\Oldies");
      //playListTracks = new List<PlayListTrack>();
      //var trackNo = 0;
      //foreach (var track in trackList.PlayList) {
      //  playListTracks.Add(new PlayListTrack(trackNo++, track));
      //}
      //playListViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("PlayListViewSource")));
      //playListViewSource.Source = playListTracks;
      //trackListIndex = -1;
      //playNext();
    }


    private void playListDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
      isManualPlay = true;
      playListViewSource.View.MoveCurrentToPrevious();
      playNext();
    }


    public class PlayListTrack {
      public Track Track { get; }
      public int No { get; }
      public string? Title { get; }
      public string? Duration { get; }
      public string? Album { get; }
      public string? Artists { get; }
      public string? Composers { get; }
      public string? Genres { get; }
      public string? Publisher { get; }
      public string? Year { get; }

      public PlayListTrack(int no, Track track){
        Track = track;
        No = no;
        Title = track.Title is null ? track.FileName : track.Title;
        Duration = track.Duration is null ? null : $"{(int)track.Duration.Value.TotalMinutes}:{track.Duration.Value.Seconds:00}";
        Album = track.Album;
        Artists = track.Artists;
        Composers = track.Composers;
        Genres = track.Genres;
        Publisher = track.Publisher;
        Year = track.Year?.ToString("0000");
      }
    }


    ////RepeatButton repeatButtonLeft;
    ////RepeatButton repeatButtonRight;


    private void mainWindow_Loaded(object sender, RoutedEventArgs e) {

      dlInitAsync();
    }


    #region Menu
    //      ----

    MenuItem importMenu;
    MenuItem playlistMenu;
    MenuItem? windowsMenu;


    private void addMenus() {
      importMenu = addSubMenu(MainMenu, "_Import", () => ImportWindow.Show(this), isEnabled: true, isOneTime: true);
      //playlistMenu = addSubMenu(MainMenu, "_Import", () => ImportWindow.Show(this), isEnabled: false, isOneTime: true);
      //addSubMenu(playlistMenu, "Import Stock _Quote", () => SgxQuotesImportWindow.Show(this), isOneTime: true);

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


    private void DcModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      if (isSkipNextEvent) {
        isSkipNextEvent = false;
        return;
      }
      bool isProductionDb = RealComboBoxItem.IsSelected;
      var questionString = isProductionDb
          ? "Do you want to switch from Debug to Production database ?"
          : "Do you want to switch from Production to Debug database ?";
      if (MessageBox.Show("Switch database ?", questionString, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No)==MessageBoxResult.Yes) {
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
      StatusDockPanel.Background =IsProductionDC ? StatusBarBackgroundNormal : StatusBarBackgroundTest;

      Progress<string> progress = new Progress<string>(s => EventsTextBox.Text += Environment.NewLine + s);
      await Task.Run(() => dlInit(IsProductionDC, progress));
      MainMenu.IsEnabled = true;
    }


    public void SetBackground(Panel panel) {
      panel.Background =IsProductionDC ? StatusBarBackgroundNormal : StatusBarBackgroundTest;
    }


    private void dlInit(bool isProduction, IProgress<string> progress) {
      if (DC.Data!=null) {
        progress.Report("Data dispose");
        DC.Data.Dispose();
      }

      progress.Report("Data initialising");
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
      progress.Report("Data initialised");
      //if (DC.Data!.Parties.Count<=0) {
      //  progress.Report("Early parties adding");
      //  new ProcessEmailAdresses();
      //  progress.Report("Early parties added");

      //}
    }
    #endregion


    private void nextButton_Click(object sender, RoutedEventArgs e) {
      isManualPlay = false;
      playNext();
    }


    //TimeSpan pausePosition;


    ////private void pauseToggleButton_Checked(object sender, RoutedEventArgs e) {
    ////  PauseToggleButton.Content = "_Play";
    ////  pausePosition = mediaPlayer.Position;
    ////  mediaPlayer.Stop();
    ////}


    ////private void pauseToggleButton_Unchecked(object sender, RoutedEventArgs e) {
    ////  PauseToggleButton.Content = "_Pause";
    ////  if (pausePosition.Ticks>0) {
    ////    mediaPlayer.Position = pausePosition;
    ////    pausePosition = default;
    ////  }
    ////  mediaPlayer.Play();
    ////}


    ////double volumeBeforeMute;


    ////private void muteToggleButton_Checked(object sender, RoutedEventArgs e) {
    ////  volumeBeforeMute = mediaPlayer.Volume;
    ////  mediaPlayer.Volume = 0;
    ////  MuteToggleButton.Content = "Un_Mute";
    ////}

    ////private void muteToggleButton_Unchecked(object sender, RoutedEventArgs e) {
    ////  mediaPlayer.Volume = volumeBeforeMute;
    ////  MuteToggleButton.Content = "_Mute";
    ////}

    private void playNext() {
      Track track;
      if (isManualPlay) {
        playListViewSource.View.MoveCurrentToNext();
        if (playListViewSource.View.IsCurrentAfterLast) {
          playListViewSource.View.MoveCurrentToFirst();
        }
        var playListTrack = (PlayListTrack)playListViewSource.View.CurrentItem;
        track = playListTrack.Track;
      } else {
        //playlist
        trackListIndex = trackListIndex++<trackList.PlayList.Count ? trackListIndex : 0;
        track = trackList.PlayList[trackListIndex];
        var playListTrack = playListTracks[trackListIndex];
        playListViewSource.View.MoveCurrentTo(playListTrack);
        PlayListDataGrid.ScrollIntoView(playListTrack);
      }
      //mediaPlayer.Open(new Uri(track.FullFileName));
      //mediaPlayer.Play();
      //PositionScrollBar.Value = 0;
    }

    ////private void mediaPlayer_BufferingStarted(object? sender, EventArgs e) {
    ////  //EventsTextBox.Text += $"BufferingStarted: {e}" + Environment.NewLine;
    ////}

    ////private void mediaPlayer_BufferingEnded(object? sender, EventArgs e) {
    ////  //EventsTextBox.Text += $"BufferingEnded: {e}" + Environment.NewLine;
    ////}

    ////private void mediaPlayer_MediaOpened(object? sender, EventArgs e) {
    ////  var duration = mediaPlayer.NaturalDuration.TimeSpan;
    ////  DurationTextBox.Text = $"{(int)duration.TotalMinutes}:{duration.Seconds:00}";
    ////  PositionScrollBar.Maximum = duration.TotalMilliseconds;
    ////}

    ////private void mediaPlayer_MediaFailed(object? sender, ExceptionEventArgs e) {
    ////  //EventsTextBox.Text += $"MediaFailed: {e}" + Environment.NewLine;
    ////}

    ////private void mediaPlayer_MediaEnded(object? sender, EventArgs e) {
    ////  playNext();
    ////}

    ////private void mediaPlayer_ScriptCommand(object? sender, MediaScriptCommandEventArgs e) {
    ////  //EventsTextBox.Text += $"ScriptCommand: {e}" + Environment.NewLine;
    ////}

    ////private void dispatcherTimer_Tick(object? sender, EventArgs e) {
    ////  var position = mediaPlayer.Position;
    ////  PositionTextBox.Text = $"{(int)position.TotalMinutes}:{position.Seconds:00}";

    ////  if (!hasPositionScrollBarDragStarted && mediaPlayer.NaturalDuration.HasTimeSpan) {
    ////    PositionScrollBar.Value = position.TotalMilliseconds;
    ////  }
    ////}

    ////bool hasPositionScrollBarDragStarted;


    ////private void thumb_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e) {
    ////  hasPositionScrollBarDragStarted = true;
    ////}


    ////private void thumb_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e) {
    ////  hasPositionScrollBarDragStarted = false;
    ////  mediaPlayer.Position = TimeSpan.FromMilliseconds(PositionScrollBar.Value);
    ////}


    ////private void repeatButtonLeft_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
    ////  var mousePosition = e.GetPosition(repeatButtonLeft);
    ////  if (mousePosition.X>=0 && mousePosition.X<=repeatButtonLeft.ActualWidth &&
    ////    mousePosition.Y>=0 && mousePosition.Y<=repeatButtonLeft.ActualHeight) 
    ////  {
    ////    PositionScrollBar.Value = PositionScrollBar.Value * mousePosition.X / repeatButtonLeft.ActualWidth;
    ////    mediaPlayer.Position = TimeSpan.FromMilliseconds(PositionScrollBar.Value);
    ////  }
    ////}


    ////private void repeatButtonRight_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
    ////  var mousePosition = e.GetPosition(repeatButtonRight);
    ////  if (mousePosition.X>=0 && mousePosition.X<=repeatButtonRight.ActualWidth &&
    ////    mousePosition.Y>=0 && mousePosition.Y<=repeatButtonRight.ActualHeight) 
    ////  {
    ////    PositionScrollBar.Value += (PositionScrollBar.Maximum - PositionScrollBar.Value) * mousePosition.X / repeatButtonRight.ActualWidth;
    ////    mediaPlayer.Position = TimeSpan.FromMilliseconds(PositionScrollBar.Value);
    ////  }
    ////}


    ////private void volumeScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
    ////  mediaPlayer.Volume = VolumeScrollBar.Value;
    ////}
  }
}
