using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAT62_Service.CAT62 {
  public class Velocity {
    public Double Vx { get; set; } = 0;
    public Double Vy { get; set; } = 0;
    public Double Speed { get; private set; } = 0;
    public int Heading { get; private set; } = 0;

    public void SetV(Double Vx, Double Vy) {
      Speed = Math.Sqrt(Vx * Vx + Vy * Vy);
      Heading = (int)(Math.Atan2(Vx , Vy) * 180 / Math.PI );
    }

  }
}
