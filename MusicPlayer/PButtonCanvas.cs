using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MusicPlayer {
  public class PButtonCanvas:Canvas {

    public PButtonCanvas() {
      Width=50;
      Path path1 = new Path();
      var geometryGroup = new GeometryGroup {FillRule = FillRule.Nonzero};
      var streamGeometry = new StreamGeometry { FillRule = FillRule.EvenOdd };
      const double x0 = 0;
      const double x1 = 40;
      const double y0 = 17;
      const double y1 = 24;//pressed button: 21
      using (StreamGeometryContext ctx = streamGeometry.Open()) {
        ctx.BeginFigure(new Point(x0, y0), isFilled: true, isClosed: true);
        ctx.ArcTo(new Point(x1, y0), new Size(10, 7), 0, true, SweepDirection.Clockwise, true, false);
        ctx.LineTo(new Point(x1, y1), isStroked: true, isSmoothJoin: false);
        ctx.ArcTo(new Point(x0, y1), new Size(10, 7), 0, true, SweepDirection.Clockwise, true, false);
      }
      streamGeometry.Freeze();
      geometryGroup.Children.Add(streamGeometry);

      streamGeometry = new StreamGeometry {FillRule = FillRule.EvenOdd};
      using (StreamGeometryContext ctx = streamGeometry.Open()) {
        ctx.BeginFigure(new Point(x0, y0), isFilled: true, isClosed: true);
        ctx.ArcTo(new Point(x1, y0), new Size(10, 7), 0, true, SweepDirection.Clockwise, true, false);
        ctx.ArcTo(new Point(x0, y0), new Size(10, 7), 0, true, SweepDirection.Clockwise, true, false);
      }
      streamGeometry.Freeze();
      geometryGroup.Children.Add(streamGeometry);
      path1.Data = geometryGroup;
      path1.Fill = linearGradientBrushButton;
      path1.Stroke = buttonStrokeBrush;
      Children.Add(path1);


    }
    #region Brushes
    //      -------

    static readonly Color gray0 = Color.FromRgb(0x00, 0x00, 0x00);
    static readonly Color gray1 = Color.FromRgb(0x10, 0x10, 0x10);
    static readonly Color gray2 = Color.FromRgb(0x20, 0x20, 0x20);
    static readonly Color gray3 = Color.FromRgb(0x30, 0x30, 0x30);
    static readonly Color gray4 = Color.FromRgb(0x40, 0x40, 0x40);
    static readonly Color gray5 = Color.FromRgb(0x50, 0x50, 0x50);
    static readonly Color gray6 = Color.FromRgb(0x60, 0x60, 0x60);
    static readonly Color gray7 = Color.FromRgb(0x70, 0x70, 0x70);
    static readonly Color gray8 = Color.FromRgb(0x80, 0x80, 0x80);
    static readonly Color gray9 = Color.FromRgb(0x90, 0x90, 0x90);
    static readonly Color grayA = Color.FromRgb(0xA0, 0xA0, 0xA0);
    static readonly Color grayB = Color.FromRgb(0xB0, 0xB0, 0xB0);
    static readonly Color grayC = Color.FromRgb(0xC0, 0xC0, 0xC0);
    static readonly Color grayD = Color.FromRgb(0xD0, 0xD0, 0xD0);
    static readonly Color grayE = Color.FromRgb(0xE0, 0xE0, 0xE0);
    static readonly Color grayF = Color.FromRgb(0xF0, 0xF0, 0xF0);

    const int violetr = 0xC3;
    const int violetb = 0xFF;
    static readonly Color violetF = Color.FromRgb(violetr, 0x00, violetb);
    static readonly Color violetE = Color.FromRgb(violetr * 14 / 16, 0x00, violetb * 14 / 16);
    static readonly Color violetD = Color.FromRgb(violetr * 13 / 16, 0x00, violetb * 13 / 16);
    static readonly Color violetC = Color.FromRgb(violetr * 12 / 16, 0x00, violetb * 12 / 16);
    static readonly Color violetB = Color.FromRgb(violetr * 11 / 16, 0x00, violetb * 11 / 16);
    static readonly Color violetA = Color.FromRgb(violetr * 10 / 16, 0x00, violetb * 10 / 16);
    static readonly Color violet9 = Color.FromRgb(violetr *  9 / 16, 0x00, violetb *  9 / 16);
    static readonly Color violet8 = Color.FromRgb(violetr *  8 / 16, 0x00, violetb *  8 / 16);
    static readonly Color violet7 = Color.FromRgb(violetr *  7 / 16, 0x00, violetb *  7 / 16);
    static readonly Color violet6 = Color.FromRgb(violetr *  6 / 16, 0x00, violetb *  6 / 16);
    static readonly Color violet5 = Color.FromRgb(violetr *  5 / 16, 0x00, violetb *  5 / 16);
    static readonly Color violet4 = Color.FromRgb(violetr *  4 / 16, 0x00, violetb *  4 / 16);
    static readonly Color violet3 = Color.FromRgb(violetr *  3 / 16, 0x00, violetb *  3 / 16);
    static readonly Color violet2 = Color.FromRgb(violetr *  2 / 16, 0x00, violetb *  2 / 16);
    static readonly Color violet1 = Color.FromRgb(violetr *  1 / 16, 0x00, violetb *  1 / 16);
    static readonly Color violet0 = Color.FromRgb(0x00, 0x00, 0x00);
    //static readonly LinearGradientBrush linearGradientBrushButton = new LinearGradientBrush(
    //    gray5,
    //    grayF,
    //    new Point(0.5, 0),
    //    new Point(0.5, 1));
    static readonly LinearGradientBrush linearGradientBrushButton = new LinearGradientBrush(
      new GradientStopCollection {
        //new GradientStop(gray8, 0),
        //new GradientStop(Colors.White, 0.5),
        //new GradientStop(gray8, 1)
         new GradientStop(violet8, 0),
        new GradientStop(Colors.White, 0.5),
        new GradientStop(violet8, 1)
     });
    static readonly Brush buttonFillBrush = new SolidColorBrush(grayE);
    static readonly Brush buttonStrokeBrush = new SolidColorBrush(grayB);
    static readonly Brush buttonSymbolFillBrush = new SolidColorBrush(violet8);
    static readonly Brush buttonSymbolStrokeBrush = new SolidColorBrush(violet4);

    #endregion
  }
}
