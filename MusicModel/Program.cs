using Storage;

_ = new StorageClassGenerator(
  sourceDirectoryString: @"C:\Users\peter\Source\Repos\MusicPlayer\MusicModel", //directory from where the .cs files get read.
  targetDirectoryString: @"C:\Users\peter\Source\Repos\MusicPlayer\MusicPlayerCore", //directory where the new .cs files get written.
  context: "DC"); //>Name of Context class, which gives static access to all data stored.
