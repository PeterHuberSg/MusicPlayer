using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MusicPlayer {
  public static class MyResources {

    public static Color ChangeBrightness(Color color, double factor) {
      if (factor<-1) throw new Exception($"Factor {factor} must be greater equal -1.");
      if (factor>1) throw new Exception($"Factor {factor} must be smaller equal 1.");

      if (factor==0) return color;

      if (factor<0) {
        //make color darker
        factor += 1;
        return Color.FromArgb(color.A, (byte)(color.R*factor), (byte)(color.G*factor), (byte)(color.B*factor));
      } else {
        //make color lighter
        return Color.FromArgb(
          color.A,
          (byte)(color.R + (255-color.R)*factor), 
          (byte)(color.G + (255-color.G)*factor),
          (byte)(color.B + (255-color.B)*factor));
      }
    }
     
  }
}
