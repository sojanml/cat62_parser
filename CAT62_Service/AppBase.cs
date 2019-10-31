using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CAT62_Service {

  public class ADSIClientEventArgs : EventArgs {
    public String Message { get; set; }
    public Exception exception { get; set; }
  }

  // State object for reading client data asynchronously  
  public class StateObject {
    // Client  socket.  
    public System.Net.Sockets.Socket workSocket = null;
    // Size of receive buffer.  
    public const int BufferSize = 1024;
    // Receive buffer.  
    public byte[] buffer = new byte[BufferSize];
    // Received data string.  
    public StringBuilder sb = new StringBuilder();
  }

  public class ExtSocket: System.Net.Sockets.Socket {
    public ExtSocket(SocketInformation socketInformation) : base(socketInformation) {}
    public ExtSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType):base(addressFamily, socketType, protocolType) {}

    public bool IsDisposed { get; set; }
    protected override void Dispose(bool disposing) {
      IsDisposed = true;
      base.Dispose(disposing);
    }
  }
}
