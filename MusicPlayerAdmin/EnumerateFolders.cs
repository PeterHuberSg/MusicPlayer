using System;
using System.Collections.Generic;
using System.Linq;

namespace MusicPlayerAdmin {


  internal class EnumerateFolders {


    public EnumerateFolders() {
      var folders = new List<(string name, string path)>();
      foreach (var folder in Enum.GetValues<Environment.SpecialFolder>()) {
        folders.Add(new(folder.ToString(), Environment.GetFolderPath(folder)));
      }
      foreach (var folder in folders.OrderBy(f => f.path)) {
        Console.Write($"{folder.path}: ");
        var fgc = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"{folder.name}");
        Console.ForegroundColor = fgc;
        Console.WriteLine();
      }
    }
  }
}