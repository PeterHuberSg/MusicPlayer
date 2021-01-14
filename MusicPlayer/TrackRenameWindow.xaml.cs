using BaseLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
  /// Interaction logic for TrackRenameWindow.xaml
  /// </summary>
  public partial class TrackRenameWindow: CheckedWindow {


    #region Constructor
    //      -----------

    public static void Show(Window ownerWindow, Track track, Action<Track>? refreshOwner) {
      var window = new TrackRenameWindow(track, refreshOwner) { Owner = ownerWindow };
      window.Show();
    }


    readonly Track? track;
    readonly Action<Track>? refreshOwner;


    public TrackRenameWindow(Track? track = null, Action<Track>? refreshOwner = null) {
      this.track = track;
      this.refreshOwner = refreshOwner;

      InitializeComponent();

      Loaded += trackRenameWindow_Loaded;
      GoogleButton.Click += googleButton_Click;
      SaveButton.Click += saveButton_Click;
      Closed += trackRenameWindow_Closed;

      if (track is not null) {
        FileNameTextBox.Text = track.FileName;
        TitleTextBox.Text = track.Title;
        TitleTextBoxNew.Initialise(track.Title);
        AlbumTextBox.Text = track.Album;
        AlbumTextBoxNew.Initialise(track.Album);
        AlbumTrackTextBox.Text = track.AlbumTrack.ToString();
        AlbumTrackTextBoxNew.Initialise(track.AlbumTrack);
        ArtistsTextBox.Text = track.Artists;
        ArtistsTextBoxNew.Initialise(track.Artists);
        ComposersTextBox.Text = track.Composers;
        ComposersTextBoxNew.Initialise(track.Composers);
        GenresTextBox.Text = track.Genres;

        GenreEditComboBox.ItemsSource = DC.Data.Genres;
        int? genreIndex = 0;
        var isFound = false;
        foreach (var genre in DC.Data.Genres) {
          if (genre==track.Genres) {
            isFound = true;
            break;
          }
          genreIndex++;
        }
        string? text;
        if (isFound) {
          text = null;
        } else {
          genreIndex = null;
          text = track.Genres;
        }
        GenreEditComboBox.Initialise(text, genreIndex);
        GenreEditComboBox.Text = track.Genres;
        PublisherTextBox.Text = track.Publisher;
        PublisherTextBoxNew.Initialise(track.Publisher);
        YearTextBox.Text = track.Year.ToString();
        YearTextBoxNew.Initialise(track.Year);

        MainWindow.Register(this, $"Rename {track.FileName}");
        updateSaveButtonIsEnabled();
      } else {
        MainWindow.Register(this, "Rename Track");
      }
    }
    #endregion


    #region Events
    //      ------

    private void trackRenameWindow_Loaded(object sender, RoutedEventArgs e) {
      TitleTextBoxNew.Focus();
    }


    private void googleButton_Click(object sender, RoutedEventArgs e) {
      var searchParameter = WebUtility.UrlEncode(ArtistsTextBoxNew.Text + " " + TitleTextBoxNew.Text);
      System.Diagnostics.Process.Start(@"C:\Program Files\Google\Chrome\Application\chrome.exe",
        $"http://google.com/search?q=" + searchParameter);
      GenresTextBox.Focus();
    }


    protected override void OnICheckChanged() {
      updateSaveButtonIsEnabled();
    }


    private void updateSaveButtonIsEnabled() {
      SaveButton.IsEnabled = HasICheckChanged && IsAvailable;
    }


    protected override void OnIsAvailableChanged() {
      updateSaveButtonIsEnabled();
    }


    private void saveButton_Click(object sender, RoutedEventArgs e) {
      FileInfo fileInfo = new FileInfo(track!.FullFileName);
      if (fileInfo.IsReadOnly) {
        fileInfo.IsReadOnly = false;
      }

      var backupDirectoryInfo = new DirectoryInfo(fileInfo.Directory!.FullName + '\\' + "Backup");
      if (!backupDirectoryInfo.Exists) {
        backupDirectoryInfo.Create();
      }

      var newFileName = backupDirectoryInfo.FullName + '\\' + track.FileName + fileInfo.Extension;
      try {
        File.Copy(fileInfo.FullName, newFileName, overwrite: true);
      } catch (Exception ex) {
        MessageWindow.Show(this, $"Exception during backup of file {fileInfo.FullName}:" + Environment.NewLine + 
          ex.ToDetailString(), null).Title = "Exception";
        return;
      }

      var fileProperties = TagLib.File.Create(fileInfo.FullName);
      fileProperties.Tag.Title = TitleTextBoxNew.Text.Length==0 ? null : TitleTextBoxNew.Text;
      fileProperties.Tag.Album = AlbumTextBoxNew.Text.Length==0 ? null : AlbumTextBoxNew.Text;
      fileProperties.Tag.Track = (uint)(AlbumTrackTextBoxNew.IntValue??0);
      fileProperties.Tag.Performers = ArtistsTextBoxNew.Text.Split(';', StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries);
      fileProperties.Tag.Composers = ComposersTextBoxNew.Text.Split(';', StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries);
      fileProperties.Tag.Genres = GenreEditComboBox.Text?.Split(';', StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries);
      fileProperties.Tag.Publisher = PublisherTextBoxNew.Text.Length == 0 ? null : PublisherTextBoxNew.Text;
      fileProperties.Tag.Year = (uint)(YearTextBoxNew.IntValue??0);
      fileProperties.Save();

      if (refreshOwner is not null) {
        track.Update(
          stringOrNull(TitleTextBoxNew.Text),
          stringOrNull(AlbumTextBoxNew.Text),
          AlbumTrackTextBoxNew.IntValue,
          stringOrNull(ArtistsTextBoxNew.Text),
          stringOrNull(ComposersTextBoxNew.Text),
          stringOrNull(PublisherTextBoxNew.Text),
          YearTextBoxNew.IntValue,
          stringOrNull(GenreEditComboBox.Text),
          track.Weight,
          track.Volume,
          track.SkipStart,
          track.SkipEnd,
          TitleTextBoxNew.Text.ToLowerInvariant().Trim() + "|" + ArtistsTextBoxNew.Text.ToLowerInvariant().Trim());
        refreshOwner(track);
      }

      ResetHasChanged();
      Close();
    }


    private static string? stringOrNull(string? text) {
      return (text?.Length??0)==0 ? null : text;
    }


    private void trackRenameWindow_Closed(object? sender, EventArgs e) {
      Owner?.Activate();
    }
    #endregion
  }
}
