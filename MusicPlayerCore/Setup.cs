using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace MusicPlayer {


  public class Setup {
    public string? CsvFilePath { get; private set; }
    public string? BackupFilePath { get; set; }
    public string? CsvTestFilePath { get; set; }


    public bool IsFirstTimeRunning { get; }
    public string SetupFilePath { get; }


    DirectoryInfo musicplayerSetupDirectory;
    FileInfo? musicplayerSetupFileInfo;

    const string setupDirectoryName = "MusicPlayer";
    const string setupFileName = "MusicPlayer.setup";


    public Setup() {
      var applicationDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
      var applicationDataDirectory = new DirectoryInfo(applicationDataPath);
      SetupFilePath = applicationDataDirectory.FullName + '\\' + setupDirectoryName + '\\' + setupFileName;
      if (!applicationDataDirectory.Exists) {
        throw new DirectoryNotFoundException($"Could not find '{applicationDataPath}', where Musicplayer wants to create a directory to store setup data.");
      }
      var directories = applicationDataDirectory.GetDirectories(setupDirectoryName);
      if (directories.Length==1) {
        musicplayerSetupDirectory = directories[0];
      } else {
        try {
          musicplayerSetupDirectory = applicationDataDirectory.CreateSubdirectory(setupDirectoryName);
          IsFirstTimeRunning = true;
        } catch (Exception ex) {
          throw new ApplicationException($"Could not create directory '{setupDirectoryName}' in '{applicationDataPath}' for storing setup data.", ex);
        }
        return;
      }

      var files = musicplayerSetupDirectory.GetFiles(setupFileName);
      if (files.Length==1) {
        musicplayerSetupFileInfo = files[0];
        using (var musicplayerSetupStreamReader = new StreamReader(musicplayerSetupFileInfo.FullName)) {
          while (!musicplayerSetupStreamReader.EndOfStream) {
            var line = musicplayerSetupStreamReader.ReadLine();
            var doublePointPos = line!.IndexOf(':');
            if (doublePointPos>0) {
              var parameterName = line[0..doublePointPos];
              var value = line[(doublePointPos+2)..] ;
              switch (parameterName) {
              case "CsvFilePath": CsvFilePath = parseDirectory(value); break;
              case "BackupFilePath": BackupFilePath = parseDirectory(value); break;
              case "CsvTestFilePath": CsvTestFilePath = parseDirectory(value); break;
              default:
                break;
              }
            }
          }
        }
      }
    }


    private string? parseDirectory(string path) {
      if (string.IsNullOrEmpty(path)) return null;

      try {
        var directoryInfo = new DirectoryInfo(path);
        if (directoryInfo.Exists) {
          return path;
        } else {
          return null;
        }
      } catch {
        return null;
      }
    }


    public void Update(string? csvFilePath, string? backupFilePath, string? csvTestFilePath) {
      CsvFilePath = csvFilePath;
      BackupFilePath = backupFilePath;
      CsvTestFilePath = csvTestFilePath;
      try {
        musicplayerSetupFileInfo?.Delete();
        using (var musicplayerSetupStreamWriter = new StreamWriter(musicplayerSetupDirectory.FullName + '\\' + setupFileName)) {
          musicplayerSetupStreamWriter.WriteLine($"CsvFilePath: {CsvFilePath}");
          musicplayerSetupStreamWriter.WriteLine($"BackupFilePath: {BackupFilePath}");
          musicplayerSetupStreamWriter.WriteLine($"CsvTestFilePath: {CsvTestFilePath}");
        }
      } catch (Exception ex) {
        throw new ApplicationException($"Could not write setup data to file '{musicplayerSetupDirectory.FullName + '\\' + setupFileName}'.", ex);
      }
    }
  }
}
