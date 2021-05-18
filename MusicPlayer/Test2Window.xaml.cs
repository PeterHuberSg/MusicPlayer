using System;
using System.Collections.Generic;
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

namespace MusicPlayer {


  /// <summary>
  /// Interaction logic for Test2Window.xaml
  /// </summary>
  public partial class Test2Window: Window {


    public static Test2Window Show(Window ownerWindow) {
      var window = new Test2Window() { Owner = ownerWindow };
      window.Show();
      return window;
    }


    class TrackRow: TrackGridRow {
      public int TrackNo { get; }
      //public Brush RowBackground { get; set; }

      public TrackRow(ref int trackNo, Track track): base(ref trackNo, track, null) {
        TrackNo=trackNo;
        RowBackground = trackNo%2==0 ? Brushes.Pink : Brushes.Violet;
      }
    }


    readonly System.Windows.Data.CollectionViewSource tracksViewSource;
    List<TrackRow> trackRows;


    public Test2Window() {
      InitializeComponent();

      trackRows = new();
      var trackNo = 0;
      foreach (var track in DC.Data.Tracks.Values.OrderBy(t => t.Title)) {
        //trackRows.Add(new TrackRow(ref trackNo, track, updateSelectedCountTextBox));
        trackRows.Add(new TrackRow(ref trackNo, track));
      }
      tracksViewSource = ((System.Windows.Data.CollectionViewSource)this.FindResource("TracksViewSource"));
      tracksViewSource.Source = trackRows;
      TracksDataGrid.KeyDown += tracksDataGrid_KeyDown;
      var contextMenu = new ContextMenu();
      var renameMenuItem = new MenuItem { Header = "Rename" };
      renameMenuItem.Click += renameMenuItem_Click;
      contextMenu.Items.Add(renameMenuItem);
      TracksDataGrid.ContextMenu = contextMenu;
    }


    private void renameMenuItem_Click(object sender, RoutedEventArgs e) {
      Track track = ((TrackRow)TracksDataGrid.SelectedItem).Track;
      TrackRenameWindow.Show(this, track, updateSelectedItem);
    }


    private void updateSelectedItem(Track track) {
      var selectedIndex = TracksDataGrid.SelectedIndex;
      tracksViewSource.View.Refresh();
      TracksDataGrid.SelectedIndex = selectedIndex;
    }


    private void tracksDataGrid_KeyDown(object sender, KeyEventArgs e) {
      if (e.SystemKey==Key.R) {
        e.Handled = true;
      };
    }
  }
}
