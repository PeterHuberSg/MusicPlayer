using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MusicPlayer
{
  public class ArrowButton: Button{


    public int ButtonType
    {
      get { return (int)GetValue(ButtonTypeProperty); }
      set { SetValue(ButtonTypeProperty, value); }
    }

    // Using a DependencyProperty as the backing store for ButtonType.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ButtonTypeProperty =
        DependencyProperty.Register("ButtonType", typeof(int), typeof(ArrowButton), new PropertyMetadata(0, onButtonTypeChanged));


    Path arrowButtonPath;
    static readonly LinearGradientBrush linearGradientBrushDark = new LinearGradientBrush(
        Color.FromRgb(0x30, 0x30, 0x30),
        Color.FromRgb(0xDC, 0xDC, 0xDC),
        new Point(0.5, 0),
        new Point(0.5, 1));
    static readonly LinearGradientBrush linearGradientBrushLight = new LinearGradientBrush(
        Color.FromRgb(0xA0, 0xA0, 0xA0),
        Color.FromRgb(0xF0, 0xF0, 0xF0),
        new Point(0.5, 0),
        new Point(0.5, 1));


    private static void onButtonTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e){
      var arrowButton = (ArrowButton)d;
      arrowButton.arrowButtonPath = new Path();
      arrowButton.arrowButtonPath.Stroke = Brushes.Black;
      arrowButton.arrowButtonPath.StrokeThickness = 1;
      arrowButton.arrowButtonPath.Margin = new Thickness(2);
      arrowButton.arrowButtonPath.Fill = linearGradientBrushDark;


      GeometryGroup arrowsGeometryGroup = new GeometryGroup();
      arrowsGeometryGroup.FillRule = FillRule.Nonzero; 
      if (arrowButton.ButtonType==1 || arrowButton.ButtonType==2 || arrowButton.ButtonType==3){
        arrowsGeometryGroup.Children.Add(createArrowUp(0));
      }
      if (arrowButton.ButtonType==2){
        arrowsGeometryGroup.Children.Add(createArrowUp(40));
      }
      if (arrowButton.ButtonType==1)
      {
        arrowsGeometryGroup.Children.Add(createLine(0));
      }
      if (arrowButton.ButtonType==4 ||arrowButton.ButtonType==5 || arrowButton.ButtonType==6){
        arrowsGeometryGroup.Children.Add(createArrowDown(0));
      }
      if (arrowButton.ButtonType==5){
        arrowsGeometryGroup.Children.Add(createArrowDown(40));
      }
      if (arrowButton.ButtonType==6)
      {
        arrowsGeometryGroup.Children.Add(createLine(42));
      }
      arrowButton.arrowButtonPath.Data = arrowsGeometryGroup;
      //arrowButton.Content = arrowButtonPath;

      var arrowViewBox = new Viewbox();
      arrowViewBox.Child = arrowButton.arrowButtonPath;
      arrowViewBox.MaxWidth = 40;
      arrowViewBox.MaxHeight = 40;
      arrowButton.Content = arrowViewBox;
    }


    const double scale = 1.0 / 1.0;


    private static StreamGeometry createArrowUp(double xOffset)
    {
      StreamGeometry arrowGeometry = new StreamGeometry();
      arrowGeometry.FillRule = FillRule.EvenOdd;
      using (StreamGeometryContext ctx = arrowGeometry.Open())
      {
        double x0 = (xOffset + 0) * scale;
        double x1 = (xOffset + 10) * scale;
        double x2 = (xOffset + 15) * scale;
        double x3 = (xOffset + 20) * scale;
        double x4 = (xOffset + 30) * scale;

        const double y0 = 43 * scale;
        const double y1 = 13 * scale;
        const double y2 = 3 * scale;
        ctx.BeginFigure(new Point(x1, y0), true /* is filled */, true /* is closed */);
        ctx.LineTo(new Point(x1, y1), true /* is stroked */, false /* is smooth join */);
        ctx.LineTo(new Point(x0, y1), true /* is stroked */, false /* is smooth join */);
        ctx.LineTo(new Point(x2, y2), true /* is stroked */, false /* is smooth join */);
        ctx.LineTo(new Point(x4, y1), true /* is stroked */, false /* is smooth join */);
        ctx.LineTo(new Point(x3, y1), true /* is stroked */, false /* is smooth join */);
        ctx.LineTo(new Point(x3, y0), true /* is stroked */, false /* is smooth join */);
      }
      arrowGeometry.Freeze();
      return arrowGeometry;
    }


    private static StreamGeometry createArrowDown(double xOffset){
      StreamGeometry arrowGeometry = new StreamGeometry();
      arrowGeometry.FillRule = FillRule.EvenOdd;
      using (StreamGeometryContext ctx = arrowGeometry.Open()){
        double x0 = (xOffset + 0) * scale;
        double x1 = (xOffset + 10) * scale;
        double x2 = (xOffset + 15) * scale;
        double x3 = (xOffset + 20) * scale;
        double x4 = (xOffset + 30) * scale;

        const double y0 = 0 * scale;
        const double y1 = 30 * scale;
        const double y2 = 40 * scale;
        ctx.BeginFigure(new Point(x1, y0), true /* is filled */, true /* is closed */);
        ctx.LineTo(new Point(x1, y1), true /* is stroked */, false /* is smooth join */);
        ctx.LineTo(new Point(x0, y1), true /* is stroked */, false /* is smooth join */);
        ctx.LineTo(new Point(x2, y2), true /* is stroked */, false /* is smooth join */);
        ctx.LineTo(new Point(x4, y1), true /* is stroked */, false /* is smooth join */);
        ctx.LineTo(new Point(x3, y1), true /* is stroked */, false /* is smooth join */);
        ctx.LineTo(new Point(x3, y0), true /* is stroked */, false /* is smooth join */);
      }
      arrowGeometry.Freeze();
      return arrowGeometry;
    }


    private static StreamGeometry createLine(double yOffset)
    {
      StreamGeometry lineGeometry = new StreamGeometry();
      lineGeometry.FillRule = FillRule.EvenOdd;
      using (StreamGeometryContext ctx = lineGeometry.Open())
      {
        double x0 = (0) * scale;
        double x1 = (30) * scale;

        double y0 = yOffset * scale;
        double y1 = (yOffset+1) * scale;
        ctx.BeginFigure(new Point(x0, y0), true /* is filled */, true /* is closed */);
        ctx.LineTo(new Point(x0, y1), true /* is stroked */, false /* is smooth join */);
        ctx.LineTo(new Point(x1, y1), true /* is stroked */, false /* is smooth join */);
        ctx.LineTo(new Point(x1, y0), true /* is stroked */, false /* is smooth join */);
      }
      lineGeometry.Freeze();
      return lineGeometry;
    }


    #pragma warning disable CS8618 // Non-nullable field arrowButtonPath must contain a non-null value when exiting constructor.
    public ArrowButton() {
    #pragma warning restore CS8618 
      IsEnabledChanged += ArrowButton_IsEnabledChanged;
    }


    private void ArrowButton_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e) {
      if (IsEnabled) {
        arrowButtonPath.Stroke = Brushes.Black;
        arrowButtonPath.Fill = linearGradientBrushDark;
      } else {
        arrowButtonPath.Stroke = Brushes.Gray;
        arrowButtonPath.Fill = linearGradientBrushLight;
      }
    }
  }
}
