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
  /// Interaction logic for TestWindow.xaml
  /// </summary>
  public partial class TestWindow: Window {
    public TestWindow() {

      InitializeComponent();
      var grid = new Grid {HorizontalAlignment=HorizontalAlignment.Stretch, Background=Brushes.WhiteSmoke};
      MainGrid.Children.Add(grid);
      Grid.SetRow(grid, 0);
      Grid.SetColumn(grid, 0);
      Grid.SetColumnSpan(grid, 3);
      for (int i = 0; i < 23; i++) {
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width=GridLength.Auto});
      }
      addColor(grid, "Black", Colors.Black);
      addColor(grid, "Gray", Colors.Gray);
      addColor(grid, "White", Colors.White);
      addColor(grid, "Yellow", Colors.Yellow);
      addColor(grid, "Green", Colors.Green);
      addColor(grid, "Blue", Colors.Blue);
      addColor(grid, "Violet", Colors.Violet);
      addColor(grid, "Red", Colors.Red);
      addColor(grid, "Orange", Colors.Orange);
      addColor(grid, "OrangeRed", Colors.OrangeRed);
      addColor(grid, "DarkOrange", Colors.DarkOrange);
      grid.RowDefinitions.Add(new RowDefinition());
    }


    static int gridRowIndex;


    private static void addColor(Grid grid, string name, Color color) {
      grid.RowDefinitions.Add(new RowDefinition {Height=GridLength.Auto });
      var textBlock = new TextBlock { Text = name, Margin=new Thickness(5,0,5,0), VerticalAlignment=VerticalAlignment.Center, Background=Brushes.White };
      grid.Children.Add(textBlock);
      Grid.SetRow(textBlock, gridRowIndex);
      Grid.SetColumn(textBlock, 0);
      for (int i = -10; i < 11; i++) {
        var rectangle = new Rectangle {
          Height = 20,
          Width=20,
        Fill=new SolidColorBrush(MyResources.ChangeBrightness(color, i/10.0)) };
        grid.Children.Add(rectangle);
        Grid.SetRow(rectangle, gridRowIndex);
        Grid.SetColumn(rectangle, i+11);
      }
      gridRowIndex++;
    }
  }
}
