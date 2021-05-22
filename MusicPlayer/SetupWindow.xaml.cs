using System;
using System.Collections.Generic;
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


namespace MusicPlayer {


  /// <summary>
  /// Interaction logic for SetupWindow.xaml
  /// </summary>
  public partial class SetupWindow: CheckedWindow {

    public static SetupWindow Show(Window ownerWindow, Setup setup, Action? refreshOwner = null){
      var window = new SetupWindow(setup, refreshOwner) { Owner = ownerWindow, ShowInTaskbar = false };
      window.Show();
      return window;
    }


    public static void ShowDialog(Window ownerWindow, Setup setup) {
      var window = new SetupWindow(setup, null) { Owner = ownerWindow, ShowInTaskbar = false };
      window.ShowDialog();
    }


    readonly Setup setup;
    readonly Action? refreshOwner;


    public SetupWindow(Setup setup, Action? refreshOwner = null) {
      this.setup = setup;
      this.refreshOwner = refreshOwner;

      InitializeComponent();

      IntroFlowDocument.Blocks.Add(new Paragraph(
        new Run($"MusicPlayer always stores the storage locations in {setup.SetupFilePath}.")));

      CsvFilePathTextBox.Initialise(setup.CsvFilePath, isRequired: true);
      BackupFilePathTextBox.Initialise(setup.BackupFilePath);
      CsvTestFilePathTextBox.Initialise(setup.CsvTestFilePath);
      updateSaveButtonIsEnabled();

      Loaded += SetupWindow_Loaded;
      BrowseDataDirButton.Click += BrowseDataDirButton_Click;
      BrowseBackupDirButton.Click += BrowseBackupDirButton_Click;
      BrowseTestDirButton.Click += BrowseTestDirButton_Click;
      SaveButton.Click += saveButton_Click;
      Closing += SetupWindow_Closing;
      Closed += SetupWindow_Closed;
      MainWindow.Register(this, "Setup");
    }


    DateTime loadedTime;


    private void SetupWindow_Loaded(object sender, RoutedEventArgs e) {
      CsvFilePathTextBox.Focus();
      CsvFilePathTextBox.PreviewLostKeyboardFocus += CsvFilePathTextBox_PreviewLostKeyboardFocus;
      BackupFilePathTextBox.PreviewLostKeyboardFocus += BackupFilePathTextBox_PreviewLostKeyboardFocus;
      CsvTestFilePathTextBox.PreviewLostKeyboardFocus += CsvTestFilePathTextBox_PreviewLostKeyboardFocus;
      loadedTime = DateTime.Now;
    }


    static readonly TimeSpan lostFocusTestingDelay = TimeSpan.FromMilliseconds(200);


    private void CsvFilePathTextBox_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
      var d = DateTime.Now - loadedTime;
      if (isClosing || DateTime.Now - loadedTime<lostFocusTestingDelay) {
        //lostfocus can fire before window is displayed and after closing. Do not verify in those situations.
        return;
      }
      if (e.NewFocus==BrowseDataDirButton) {
        //allow to use Browse Data Directory button when data directory path is not defined.
        return;
      }

      e.Handled = !verifyDataDirectory();
    }


    private void BrowseDataDirButton_Click(object sender, RoutedEventArgs e) {
      var openFolderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
      if (openFolderDialog.ShowDialog()==false) return;

      CsvFilePathTextBox.Text = openFolderDialog.SelectedPath;
    }


    private void BackupFilePathTextBox_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
      if (isClosing || DateTime.Now - loadedTime<lostFocusTestingDelay) {
        //lostfocus can fire before window is displayed and after closing. Do not verify in those situations.
        return;
      }

      e.Handled = !verifyBackupDirectory();
    }


    private void BrowseBackupDirButton_Click(object sender, RoutedEventArgs e) {
      var openFolderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
      if (openFolderDialog.ShowDialog()==false) return;

      BackupFilePathTextBox.Text = openFolderDialog.SelectedPath;
    }


    private void CsvTestFilePathTextBox_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) {
      if (isClosing || DateTime.Now - loadedTime<lostFocusTestingDelay) {
        //lostfocus can fire before window is displayed and after closing. Do not verify in those situations.
        return;
      }

      e.Handled = !verifyTestDirectory();
    }


    private void BrowseTestDirButton_Click(object sender, RoutedEventArgs e) {
      var openFolderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
      if (openFolderDialog.ShowDialog()==false) return;

      CsvTestFilePathTextBox.Text = openFolderDialog.SelectedPath;
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
      if (!verifyDirectories()) return;

      try {
        setup.Update(
          stringOrNull(CsvFilePathTextBox.Text),
          stringOrNull(BackupFilePathTextBox.Text),
          stringOrNull(CsvTestFilePathTextBox.Text));
        ResetHasChanged();
        Close();
        refreshOwner?.Invoke();

      } catch (Exception ex) {

        MessageBox.Show("Could not store setup data.", "Setup Data Error" + Environment.NewLine + ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }


    private bool verifyDirectories() {
      if (isClosing || DateTime.Now - loadedTime<lostFocusTestingDelay) {
        //lostfocus can fire before window is displayed and after closing. Do not verify in those situations.
        return true; 
      }

      if (!verifyDataDirectory()) return false;

      if (!verifyBackupDirectory()) return false;

      if (!verifyTestDirectory()) return false;

      return true;
    }


    private bool verifyDataDirectory() {
      if (!Directory.Exists(CsvFilePathTextBox.Text)) {
        MessageBox.Show($"Cannot find Data Directory at '{CsvFilePathTextBox.Text}'.", "Directory Missing Error", MessageBoxButton.OK, MessageBoxImage.Error);
        return false;
      }

      return true;
    }


    private bool verifyBackupDirectory() {
      if (BackupFilePathTextBox.Text.Length>0 && !Directory.Exists(BackupFilePathTextBox.Text)) {
        MessageBox.Show($"Cannot find Backup Directory at '{BackupFilePathTextBox.Text}'.", "Directory Missing Error", MessageBoxButton.OK, MessageBoxImage.Error);
        return false;
      }

      return true;
    }


    private bool verifyTestDirectory() {
      if (CsvTestFilePathTextBox.Text.Length>0 && !Directory.Exists(CsvTestFilePathTextBox.Text)) {
        MessageBox.Show($"Cannot find Test Data Directory at '{CsvTestFilePathTextBox.Text}'.", "Directory Missing Error", MessageBoxButton.OK, MessageBoxImage.Error);
        return false;
      }

      return true;
    }


    private static string? stringOrNull(string? text) {
      return (text?.Length??0)==0 ? null : text;
    }


    bool isClosing;


    private void SetupWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
      isClosing = true;
    }


    private void SetupWindow_Closed(object? sender, EventArgs e) {
      Owner?.Activate();
    }
  }
}
