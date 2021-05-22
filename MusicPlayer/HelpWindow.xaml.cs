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
  /// Interaction logic for HelpWindow.xaml
  /// </summary>
  public partial class HelpWindow: Window {


    public static HelpWindow Show(Window ownerWindow) {
      var window = new HelpWindow(ownerWindow) { Owner = ownerWindow, ShowInTaskbar = false };
      window.Show();
      return window;
    }


    public HelpWindow(Window ownerWindow) {
      InitializeComponent();
      MainWindow.Register(this, "Help");
      Width = ownerWindow.Width * 0.4;
      Height = ownerWindow.Height * 0.8;
      Left = ownerWindow.Left + ownerWindow.Width * 0.5;
      Top = ownerWindow.Top + ownerWindow.Height * 0.1;

      Closed += HelpWindow_Closed;
    }


    private void HelpWindow_Closed(object? sender, EventArgs e) {
      Owner?.Activate();
    }
  }
}
