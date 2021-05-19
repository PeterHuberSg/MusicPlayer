# MusicPlayer
Here is a player of of music files which can deal with big file collections 
quickly and guarantees to play each track only once, even in shuffle mode, before 
any track gets repeated. A lot of work was put into playlists, so that they can be 
easily generated and managed. It can play any file format Windows knows to play.

You can use it out of the box, or, if you are like me, you can add features as you
please. It is completely written in C# using WPF.

## Start Screen

![Start screen](MusicPlayer.png)

The main screen shows the playlists on the left and the items of the 
selected Oldies playlist on the right. At the bottom are the buttons to play, 
pause, skip to next track and shuffle mode. By the way, if you don't like the looks 
of the user interface, you can easily change it, it is all WPF.

There are different windows for importing tracks, managing all tracks or just 
managing a single playlist. They all display the tracks in a DataGrid, which gives 
great flexibility to search, sort and filter them.

## Track Filter

![Filter](Filter.png)

The user can search for any text in track title, album name, composer, 
publisher, genre, year and hard disk folder. All this information is 
stored in the track files themselves, which is kind of cumbersome to edit 
in Windows File Explorere. The MusicPlayer supports changing them directly 
in the application.

## Data Storage

Information about the tracks, playlists, etc. are stored in CSV files on 
the local hard disk using my library 
[StorageLib](http://github.com/PeterHuberSg/StorageLib). It is a 
replacement for a database with the advantages:
+ Just define your classes and their properties, *StorageLib* will create all the code needed for storing the data permanently
+ Support for hierarchical data structures, guaranteeing the data integrity (no child without a parent).
+ Transaction support
+ Automatic backup
+ all data stored in RAM, resulting in extremly fast data access using LINQ.


## Configuration

I am working to improve how you can configure the lcoations where the .CSV files 
get stored. In the meantime, you have to enter this information in the file 
MusicPlayerCore\DC.cs, lines 16 - 21:

    public const string CsvFilePath = @"C:\Users\Peter\OneDrive\OneDriveData\MusicPlayer";
    public const string BackupFilePath = @"E:\MusicPlayerBackup";

    //test
    public const string CsvTestFilePath = @"E:\MusicPlayerCsvTest";
    public const string? BackupTestFilePath = null;

**CsvFilePath:** where the actual data gets stored. I keep it in a OneDrive directory to 
get an immediate online backup.

**BackupFilePath:** When MusicPlayer stops, it copies all .CSV files to this path.

For testing:

**CsvTestFilePath:** When switching to test mode, all .Csv files get copied to this path 
and all Musicplayer operations use these files.

**BackupTestFilePath:** Since the test data gets copied over from the real data each 
time testing starts, usually no testing backup directory is needed.

## VS Solution MusicPlayer Project Structure

### Project MusicPlayer
Contains the WPF application, which starts with MainWindow. MainWindow.xaml.cs starts 
the data Layer MusicPlayerCore.

### Project MusicPlayerCore
MusicPlayerCore contains all the auto generated classes with names like xxx.base.cs. 
They contain classes for Tracks, Playlists, PlaylistTracks, etc. The developer can 
add functionality to the auto created classes by adding a xxx.cs file.

DC.base.cs is the data context and gives static access to all data like this:
DC.Data.Xxxs

### Project MusicModel

MusicModel contains the data model. In there I have defined classes like Track and all 
its properties. *StorageLib* then created the classes like Track.base.cs. Each time 
an instance of Track gets created, updated or deleted, a copy gets maintained in the
Track.CSV file. On startup of MusicPlayer, these .CSV files get read and all the 
data made available in Ram, like DC.Data.Tracks.

For more details see [StorageLib](http://github.com/PeterHuberSg/StorageLib)

