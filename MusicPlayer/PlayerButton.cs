using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MusicPlayer {


  public class PlayerButton: Shape {

    #region Properties
    //      ----------

    public enum TypeEnum { Play, Next, Repeat, Random, Mute}


    /// <summary>
    /// Type property configuration
    /// </summary>
    public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(
        "Data",
        typeof(TypeEnum),
        typeof(Shape),
        new FrameworkPropertyMetadata(
            TypeEnum.Play,
            FrameworkPropertyMetadataOptions.AffectsRender, 
            typeChanged),
        null);


    private static void typeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
      var playerButton = (PlayerButton)d;
      if (!playerButton.IsLoaded) return;

      playerButton.setToolTip();
    }


    private void setToolTip() {
      ToolTip = Type switch {
        TypeEnum.Play => new ToolTip { Content="Starts and stops a track." },
        TypeEnum.Next => new ToolTip { Content="Plays the next song in the list." },
        TypeEnum.Repeat => new ToolTip { Content="Keeps playing all tracks in the list." },
        TypeEnum.Random => new ToolTip { Content="Shufles the tracks in a list and plays them in a random order." },
        TypeEnum.Mute => new ToolTip { Content="Switches the loudspeaker off and on." },
        _ => throw new NotSupportedException(),
      };
    }


    /// <summary>
    /// Type property
    /// </summary>
    public TypeEnum Type {
      get {
        return (TypeEnum)GetValue(TypeProperty);
      }
      set {
        SetValue(TypeProperty, value);
      }
    }


    /// <summary>
    /// True if button displays Symbol corresponding to a pressed button, false if button displays Symbol corresponding to a 
    /// released button. Clicking on the button toggels between the 2 states.
    /// </summary>
    private bool isPressed;

    public bool IsPressed {
      get { return isPressed; }
      set {
        if (isPressed!=value) {
          isPressed = value;
          InvalidateVisual();//unfortunately, there is no InvalidateRender()
        }
      }
    }

    #endregion`


    #region Events
    //      ------

    /// <summary>
    /// Event corresponds to left mouse button click
    /// </summary>
    public static readonly RoutedEvent ClickEvent = 
      EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PlayerButton));

    /// <summary>
    /// Add / Remove ClickEvent handler
    /// </summary>
    [System.ComponentModel.Category("Behavior")]
    public event RoutedEventHandler Click { add { AddHandler(ClickEvent, value); } remove { RemoveHandler(ClickEvent, value); } }
    #endregion`


    #region Static Constructor
    //      ------------------

    static readonly GeometryGroup outlineUpGeometryGroup;
    static readonly GeometryGroup outlineDownGeometryGroup;
    static readonly StreamGeometry symbolPlayStreamGeometry;
    static readonly StreamGeometry symbolStopStreamGeometry;
    static readonly StreamGeometry symbolNextStreamGeometry;
    static readonly GeometryGroup symbolRepeatUpGeometryGroup;
    static readonly GeometryGroup symbolRepeatDownGeometryGroup;
    static readonly GeometryGroup symbolRandomUpGeometryGroup;
    static readonly GeometryGroup symbolRandomDownGeometryGroup;
    static readonly StreamGeometry symbolMuteUpStreamGeometry;
    static readonly StreamGeometry symbolMuteDownStreamGeometry;
    static readonly StreamGeometry symbolMuteSlashUpStreamGeometry;
    static readonly StreamGeometry symbolMuteSlashDownStreamGeometry;

    //coordinates of area in button where symbols can be painted
    const double scale = 1;
    const double pixel = 1 * scale;
    const double shiftY = scale * 3; //shift y when button is pressed
    const double originX = 5 * scale;
    const double originY = 7 * scale;
    const double width = 30 * scale;
    const double widthHalf = 30 * scale;
    const double height = 20 * scale;
    const double heightHalf = height/2;
    const double strokeWidth = 2 * scale; //stroke used to draw symbols which are not filled


    static PlayerButton() {
      //Button shape geometry
      outlineUpGeometryGroup = createOutlineGeometryGroup(isPressed: false);
      outlineDownGeometryGroup = createOutlineGeometryGroup(isPressed: true);

      //Play symbol
      symbolPlayStreamGeometry = new StreamGeometry { FillRule = FillRule.EvenOdd };
      using (StreamGeometryContext ctx = symbolPlayStreamGeometry.Open()) {
        const double x0 = originX + width / 3;
        const double x1 = originX + width;
        const double y0 = originY;
        const double y1 = originY + height/2;
        const double y2 = originY + height;
        ctx.BeginFigure(new Point(x0, y0), isFilled: true, isClosed: true);
        ctx.LineTo(new Point(x1, y1), isStroked: true, isSmoothJoin: false);
        ctx.LineTo(new Point(x0, y2), isStroked: true, isSmoothJoin: false);
      }
      symbolPlayStreamGeometry.Freeze();

      //Stop symbol
      symbolStopStreamGeometry = new StreamGeometry { FillRule = FillRule.EvenOdd };
      using (StreamGeometryContext ctx = symbolStopStreamGeometry.Open()) {
        const double offset = height / 6;
        const double length = height - 2*offset;
        const double x0 = originX + (width-length) / 2;
        const double x1 = x0 + length;
        const double y0 = originY + offset + shiftY;
        const double y1 = y0 + length;
        ctx.BeginFigure(new Point(x0, y0), isFilled: true, isClosed: true);
        ctx.LineTo(new Point(x0, y1), isStroked: true, isSmoothJoin: false);
        ctx.LineTo(new Point(x1, y1), isStroked: true, isSmoothJoin: false);
        ctx.LineTo(new Point(x1, y0), isStroked: true, isSmoothJoin: false);
      }
      symbolStopStreamGeometry.Freeze();

      //Next symbol
      symbolNextStreamGeometry = new StreamGeometry { FillRule = FillRule.EvenOdd };
      using (StreamGeometryContext ctx = symbolNextStreamGeometry.Open()) {
        const double x0 = originX + width / 4;
        const double x1 = x0 + heightHalf;
        const double x2 = x1 + heightHalf;
        const double y0 = originY;
        const double y1 = y0 + heightHalf;
        const double y2 = y1 + heightHalf;
        ctx.BeginFigure(new Point(x0, y0), isFilled: true, isClosed: true);
        ctx.LineTo(new Point(x0, y2), isStroked: true, isSmoothJoin: false);
        ctx.LineTo(new Point(x1, y1), isStroked: true, isSmoothJoin: false);
        ctx.LineTo(new Point(x1, y2), isStroked: true, isSmoothJoin: false);
        ctx.LineTo(new Point(x2, y1), isStroked: true, isSmoothJoin: false);
        ctx.LineTo(new Point(x1, y0), isStroked: true, isSmoothJoin: false);
        ctx.LineTo(new Point(x1, y1), isStroked: true, isSmoothJoin: false);
      }
      symbolNextStreamGeometry.Freeze();

      //Repeat symbol
      symbolRepeatUpGeometryGroup = createSymbolRepeatGeometryGroup(isPressed: false);
      symbolRepeatDownGeometryGroup = createSymbolRepeatGeometryGroup(isPressed: true);

      //Random symbol
      symbolRandomUpGeometryGroup = createSymbolRandomGeometryGroup(isPressed: false);
      symbolRandomDownGeometryGroup = createSymbolRandomGeometryGroup(isPressed: true);

      //Mute symbol
      //symbolMuteUpStreamGeometry
      (symbolMuteUpStreamGeometry, symbolMuteSlashUpStreamGeometry) = createSymbolMuteStreamGeometry(isPressed: false);
      (symbolMuteDownStreamGeometry, symbolMuteSlashDownStreamGeometry) = createSymbolMuteStreamGeometry(isPressed: true);
    }


    private static GeometryGroup createOutlineGeometryGroup(bool isPressed) {
      var geometryGroup = new GeometryGroup { FillRule = FillRule.Nonzero };
      var streamGeometry = new StreamGeometry { FillRule = FillRule.EvenOdd };
      const double x0 = 0;
      const double x1 = 40 * scale;
      double y0 = isPressed ? 20 * scale : 20 * scale - shiftY;
      const double y1 = 24 * scale;
      using (StreamGeometryContext ctx = streamGeometry.Open()) {
        ctx.BeginFigure(new Point(x0, y0), isFilled: true, isClosed: true);
        ctx.ArcTo(new Point(x1, y0), new Size(10, 7), 0, true, SweepDirection.Clockwise, true, false);
        ctx.LineTo(new Point(x1, y1), isStroked: true, isSmoothJoin: false);
        ctx.ArcTo(new Point(x0, y1), new Size(10, 7), 0, true, SweepDirection.Clockwise, true, false);
      }
      streamGeometry.Freeze();
      geometryGroup.Children.Add(streamGeometry);

      streamGeometry = new StreamGeometry { FillRule = FillRule.EvenOdd };
      using (StreamGeometryContext ctx = streamGeometry.Open()) {
        ctx.BeginFigure(new Point(x0, y0), isFilled: true, isClosed: true);
        ctx.ArcTo(new Point(x1, y0), new Size(10, 7), 0, true, SweepDirection.Clockwise, true, false);
        ctx.ArcTo(new Point(x0, y0), new Size(10, 7), 0, true, SweepDirection.Clockwise, true, false);
      }
      geometryGroup.Children.Add(streamGeometry);
      geometryGroup.Freeze();
      return geometryGroup;
    }


    private static GeometryGroup createSymbolRepeatGeometryGroup(bool isPressed) {
      const double marginX = 3*pixel; //arrow heads need a bit more margin left and right
      const double newWidth = width - 2 * marginX;
      const double x0 = originX + marginX - 1*pixel;
      const double controlX = x0 + newWidth/2;//Bezier control point
      const double x1 = x0 + newWidth;

      const double marginY = 0*pixel; //arrows need less margin because they are curved
      const double newHeight = 2 * marginY + height; //arrows need less margin because they are curved
      const double startY = originY - marginY;
      const double control0Y = startY - newHeight/6; //Bezier control point top
      const double y0 = startY + newHeight/3;
      const double y1 = y0 + newHeight/3;
      const double control1Y = startY + newHeight + newHeight/6; //Bezier control point bottom
      const double d = newHeight/4;
      const double strokeHalf = strokeWidth/2;
      var pressedY = isPressed ? shiftY : 0;
      var symbolRepeatStreamGeometry = new StreamGeometry { FillRule = FillRule.EvenOdd };
      using (StreamGeometryContext ctx = symbolRepeatStreamGeometry.Open()) {
        ctx.BeginFigure(new Point(x0, y0), isFilled: false, isClosed: false);
        ctx.QuadraticBezierTo(new Point(controlX, control0Y+pressedY), new Point(x1, y0+pressedY), isStroked: true, isSmoothJoin: false);
        ctx.LineTo(new Point(x1-d + strokeHalf, y0+pressedY + strokeHalf), isStroked: false, isSmoothJoin: true);
        ctx.LineTo(new Point(x1 + strokeHalf, y0+pressedY + strokeHalf), isStroked: true, isSmoothJoin: true);
        ctx.LineTo(new Point(x1 + strokeHalf, y0-d+pressedY + strokeHalf), isStroked: false, isSmoothJoin: true);
        ctx.LineTo(new Point(x1 + strokeHalf, y0+pressedY + strokeHalf), isStroked: true, isSmoothJoin: false);
      }
      var symbolRepeatGeometryGroup = new GeometryGroup();
      symbolRepeatGeometryGroup.Children.Add(symbolRepeatStreamGeometry);
      symbolRepeatStreamGeometry = new StreamGeometry { FillRule = FillRule.EvenOdd };
      using (StreamGeometryContext ctx = symbolRepeatStreamGeometry.Open()) {
        ctx.BeginFigure(new Point(x1, y1+pressedY), isFilled: false, isClosed: false);
        ctx.QuadraticBezierTo(new Point(controlX, control1Y+pressedY), new Point(x0, y1+pressedY), isStroked: true, isSmoothJoin: false);
        ctx.LineTo(new Point(x0+d-strokeHalf, y1-strokeHalf+pressedY), isStroked: false, isSmoothJoin: true);
        ctx.LineTo(new Point(x0-strokeHalf, y1-strokeHalf+pressedY), isStroked: true, isSmoothJoin: true);
        ctx.LineTo(new Point(x0-strokeHalf, y1+d-strokeHalf+pressedY), isStroked: false, isSmoothJoin: true);
        ctx.LineTo(new Point(x0-strokeHalf, y1-strokeHalf+pressedY), isStroked: true, isSmoothJoin: false);
      }
      symbolRepeatGeometryGroup.Children.Add(symbolRepeatStreamGeometry);
      symbolRepeatGeometryGroup.Freeze();
      return symbolRepeatGeometryGroup;
    }


    private static GeometryGroup createSymbolRandomGeometryGroup(bool isPressed) {
      const double widthUnit = width / 55;
      const double x0 = originX;
      const double x1 = x0 + 20*widthUnit;
      const double x2 = x0 + 22.5*widthUnit;
      const double x3 = x0 + 25*widthUnit;
      const double x4 = x0 + 30*widthUnit;
      const double x5 = x0 + 32.5*widthUnit;
      const double x6 = x0 + 35*widthUnit;
      const double x7 = originX + width;

      const double marginY = 5*pixel; //arrow heads need some space
      const double newHeight = height - 2 * marginY;
      const double heightUnit = newHeight / 4;
      const double y0 = originY + marginY;
      const double y1 = y0 + heightUnit;
      const double y2 = y0 + 3*heightUnit;
      const double y3 = y0 + newHeight;
      const double d = newHeight/4;

      var pressedY = isPressed ? shiftY : 0;
      var symbolRepeatStreamGeometry = new StreamGeometry { FillRule = FillRule.EvenOdd };
      using (StreamGeometryContext ctx = symbolRepeatStreamGeometry.Open()) {
        ctx.BeginFigure(new Point(x0, y0+pressedY), isFilled: false, isClosed: false);
        ctx.LineTo(new Point(x1, y0+pressedY), isStroked: true, isSmoothJoin: true);
        ctx.QuadraticBezierTo(new Point(x2, y0+pressedY), new Point(x3, y1+pressedY), isStroked: true, isSmoothJoin: true);
        ctx.LineTo(new Point(x4, y2+pressedY), isStroked: true, isSmoothJoin: true);
        ctx.QuadraticBezierTo(new Point(x5, y3+pressedY), new Point(x6, y3+pressedY), isStroked: true, isSmoothJoin: true);
        ctx.LineTo(new Point(x7, y3+pressedY), isStroked: true, isSmoothJoin: true);
        ctx.LineTo(new Point(x7-d, y3-d+pressedY), isStroked: true, isSmoothJoin: true);
        ctx.LineTo(new Point(x7, y3+pressedY), isStroked: false, isSmoothJoin: true);
        ctx.LineTo(new Point(x7-d, y3+d+pressedY), isStroked: true, isSmoothJoin: true);
      }
      var symbolRepeatGeometryGroup = new GeometryGroup();
      symbolRepeatGeometryGroup.Children.Add(symbolRepeatStreamGeometry);
      symbolRepeatStreamGeometry = new StreamGeometry { FillRule = FillRule.EvenOdd };
      using (StreamGeometryContext ctx = symbolRepeatStreamGeometry.Open()) {
        ctx.BeginFigure(new Point(x0, y3+pressedY), isFilled: false, isClosed: false);
        ctx.LineTo(new Point(x1, y3+pressedY), isStroked: true, isSmoothJoin: true);
        ctx.QuadraticBezierTo(new Point(x2, y3+pressedY), new Point(x3, y2+pressedY), isStroked: true, isSmoothJoin: true);
        ctx.LineTo(new Point(x4, y1+pressedY), isStroked: true, isSmoothJoin: true);
        ctx.QuadraticBezierTo(new Point(x5, y0+pressedY), new Point(x6, y0+pressedY), isStroked: true, isSmoothJoin: true);
        ctx.LineTo(new Point(x7, y0+pressedY), isStroked: true, isSmoothJoin: true);
        ctx.LineTo(new Point(x7-d, y0-d+pressedY), isStroked: true, isSmoothJoin: true);
        ctx.LineTo(new Point(x7, y0+pressedY), isStroked: false, isSmoothJoin: true);
        ctx.LineTo(new Point(x7-d, y0+d+pressedY), isStroked: true, isSmoothJoin: true);
      }
      symbolRepeatGeometryGroup.Children.Add(symbolRepeatStreamGeometry);
      symbolRepeatGeometryGroup.Freeze();
      return symbolRepeatGeometryGroup;
    }


    private static (StreamGeometry loudspeaker, StreamGeometry slash) createSymbolMuteStreamGeometry(bool isPressed) {
      var pressedY = isPressed ? shiftY : 0;
      var loudspeakerStreamGeometry = new StreamGeometry { FillRule = FillRule.EvenOdd };
      using (StreamGeometryContext ctx = loudspeakerStreamGeometry.Open()) {
        const double startX = originX + 7*pixel;
        const double starty = originY;
        const double heightHalf = height/2;
        const double heightQuart = height/4;
        const double height3Quart = height*3/4;
        const double x0 = 0;
        const double x1 = heightHalf;
        const double y0 = 0;
        const double y1 = height;

        ctx.BeginFigure(new Point(startX+x0, starty+heightQuart+pressedY), isFilled: true, isClosed: true);
        ctx.LineTo(new Point(startX+heightQuart, starty+heightQuart+pressedY), isStroked: true, isSmoothJoin: false);
        ctx.LineTo(new Point(startX+x1, starty+y0+pressedY), isStroked: true, isSmoothJoin: false);
        ctx.LineTo(new Point(startX+x1, starty+y1+pressedY), isStroked: true, isSmoothJoin: false);
        ctx.LineTo(new Point(startX+heightQuart, starty+height3Quart+pressedY), isStroked: true, isSmoothJoin: false);
        ctx.LineTo(new Point(startX+x0, starty+height3Quart+pressedY), isStroked: true, isSmoothJoin: false);
      }
      loudspeakerStreamGeometry.Freeze();

      var slashStreamGeometry = new StreamGeometry { FillRule = FillRule.EvenOdd };
      using (StreamGeometryContext ctx = slashStreamGeometry.Open()) {
        const double marginX = width / 10; 
        const double newWidth = width - 2 * marginX;
        const double x0 = originX + marginX ;
        const double x1 = x0 + newWidth;
        const double marginY = height / 10; 
        const double newHeight = height - 2 * marginY;
        const double y0 = originY + marginY;
        const double y1 = y0 + newHeight;
        ctx.BeginFigure(new Point(x0, y0+pressedY), isFilled: false, isClosed: false);
        ctx.LineTo(new Point(x1, y1+pressedY), isStroked: true, isSmoothJoin: false);
      }
      slashStreamGeometry.Freeze();

      return (loudspeakerStreamGeometry, slashStreamGeometry);
    }
    #endregion


    #region Constructor
    //      -----------

    public PlayerButton() {
      Fill = linearGradientBrushButton;
      Stroke = buttonStrokeBrush;
      Loaded += playerButton_Loaded;
    }


    private void playerButton_Loaded(object sender, RoutedEventArgs e) {
      setToolTip();
    }
    #endregion`


    #region Overrides
    //      ---------

    protected override Geometry DefiningGeometry {
      get {
        return isPressed ? outlineDownGeometryGroup : outlineUpGeometryGroup;
      }
    }


    protected override void OnMouseEnter(MouseEventArgs e) {
      base.OnMouseEnter(e);
    }


    protected override void OnMouseLeave(MouseEventArgs e) {
      isLeftButtonDown = false;
      base.OnMouseLeave(e);
    }


    bool isLeftButtonDown;
    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
      isLeftButtonDown = true;
      base.OnMouseLeftButtonDown(e);
    }


    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
      if (isLeftButtonDown) {
        isLeftButtonDown = false;
        if (Type!=TypeEnum.Next) {
          IsPressed = !isPressed;
        }
        RaiseEvent(new RoutedEventArgs(ClickEvent, this));
      }
    }


    protected override void OnRender(DrawingContext drawingContext) {
      base.OnRender(drawingContext);

      switch (Type) {
      case TypeEnum.Play:
        if (isPressed) {
          drawingContext.DrawGeometry(ButtonSymbolFillBrush, buttonSymbolStrokePen, symbolStopStreamGeometry);
        } else {
          drawingContext.DrawGeometry(ButtonSymbolFillBrush, buttonSymbolStrokePen, symbolPlayStreamGeometry);
        }
        break;

      case TypeEnum.Next:
        drawingContext.DrawGeometry(ButtonSymbolFillBrush, buttonSymbolStrokePen, symbolNextStreamGeometry);
        break;


      case TypeEnum.Repeat:
        if (isPressed) {
          drawingContext.DrawGeometry(null, buttonSymbolEnableDrawPen, symbolRepeatDownGeometryGroup);
        } else {
          drawingContext.DrawGeometry(null, buttonSymbolDisableDrawPen, symbolRepeatUpGeometryGroup);
        }
        break;

      case TypeEnum.Random:
        if (isPressed) {
          drawingContext.DrawGeometry(null, buttonSymbolEnableDrawPen, symbolRandomDownGeometryGroup);
        } else {
          drawingContext.DrawGeometry(null, buttonSymbolDisableDrawPen, symbolRandomUpGeometryGroup);
        }
        break;

      case TypeEnum.Mute:
        if (isPressed) {
          drawingContext.DrawGeometry(null, buttonSymbolEnableDrawPen, symbolMuteDownStreamGeometry);
          drawingContext.DrawGeometry(null, buttonSymbolBlackPen, symbolMuteSlashDownStreamGeometry);
        } else {
          drawingContext.DrawGeometry(null, buttonSymbolDisableDrawPen, symbolMuteUpStreamGeometry);
          drawingContext.DrawGeometry(null, buttonSymbolDisableDrawPen, symbolMuteSlashUpStreamGeometry);
        }
        break;
      default:
        throw new NotSupportedException();
      }
    }
    #endregion`


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
    static readonly Brush buttonFillBrush = createFrozenSolidColorBrush(grayE);
    static readonly Brush buttonStrokeBrush = createFrozenSolidColorBrush(grayB);
    public static readonly Brush ButtonSymbolFillBrush = createFrozenSolidColorBrush(violet8);
    public static readonly Brush ButtonSymbolStrokeBrush = createFrozenSolidColorBrush(violet4);
    static readonly Pen buttonSymbolStrokePen = createFrozenPen(ButtonSymbolStrokeBrush, 1);
    static readonly Pen buttonSymbolEnableDrawPen = createFrozenPen(ButtonSymbolFillBrush, strokeWidth);
    static readonly Pen buttonSymbolDisableDrawPen = createFrozenPen(Brushes.DarkCyan, strokeWidth);
    static readonly Pen buttonSymbolBlackPen = createFrozenPen(Brushes.Black, strokeWidth);


    private static Brush createFrozenSolidColorBrush(Color color) {
      var brush = new SolidColorBrush(color);
      brush.Freeze();
      return brush;
    }


    private static Pen createFrozenPen(Brush brush, double width) {
      var pen = new Pen(brush, width);
      pen.Freeze();
      return pen;
    }

    #endregion
  }
}
