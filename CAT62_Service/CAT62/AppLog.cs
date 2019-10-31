using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAT62_Service.CAT62 {
  public static class AppLog {
    public static object _lockWrite = new object();
    public static void Add(String Message) {
      String BaseDir = AppDomain.CurrentDomain.BaseDirectory;
      BaseDir = BaseDir.Replace("\\Debug\\", "\\");
      BaseDir = BaseDir.Replace("\\bin\\", "\\");
      String BasePath = System.IO.Path.Combine(BaseDir, "ApplicationLog");
      if (!System.IO.Directory.Exists(BasePath))
        System.IO.Directory.CreateDirectory(BasePath);
      String LogFile = System.IO.Path.Combine(BasePath, DateTime.UtcNow.ToString("yyyy-MM-dd") + ".txt");
      lock(_lockWrite) { 
        System.IO.File.AppendAllText(LogFile, Message);
      }
    }
  }
}
