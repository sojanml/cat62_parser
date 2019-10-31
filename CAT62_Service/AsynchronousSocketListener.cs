using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CAT62_Service {
  public class AsynchronousSocketListener {
    // Thread signal.  
    public event EventHandler<ADSIClientEventArgs> OnMessage;
    public event EventHandler<byte[]> OnDataReceive;
    public event EventHandler<Socket> OnClientConnect;

    private object _lockFile = new object();
    private ExtSocket listener = null;
    public Boolean IsConnected { get; private set; } = false;

    public int Port { get; set; } = 55555;
    public string PortType { get; internal set; } = "TCP";
    public bool ClearRecordsOnConnect { get; internal set; }
    private Boolean IsUDPReading = true;

    public void StopServer() {
      if (this.PortType.Equals("TCP", StringComparison.InvariantCultureIgnoreCase)) {
        FnOnMessage("Closing the socket connection...");
        if (listener != null && listener.Connected)
          listener.Shutdown(SocketShutdown.Both);
        listener.Close();
      } else {
        IsUDPReading = false;
      }
      //listener = null;        
    }

    public void StartServer() {
      if (this.PortType.Equals("TCP", StringComparison.InvariantCultureIgnoreCase)) {
        StartTCPServer();
      } else {
        StartUDPServer();
      }
    }



    public void StartUDPServer() {
      Thread UDPServerThread = new Thread(StartUDPReceiver);
      UDPServerThread.IsBackground = true;
      UDPServerThread.Priority = ThreadPriority.BelowNormal;
      UDPServerThread.Start();
    }

    public async void StartUDPReceiver() {
      while (IsUDPReading) {
        IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, Port);
        UdpClient Client = new UdpClient(localEndPoint);

        FnOnMessage("Waiting for UDP connection...");
        try {

          FnOnMessage("Reading from UDP Connection...");
          while (IsUDPReading) {
            UdpReceiveResult result = await Client.ReceiveAsync();
            int bytesRead = result.Buffer.Length;
            if (bytesRead > 0) {
              //Add data to received buffer
              OnDataReceive?.Invoke(this, result.Buffer);
              //FnOnMessage($"Reading data of {bytesRead} bytes from Server...");
              WriteTo(result.Buffer);
            }
            await Task.Delay(10);
          }

        } catch (Exception ex) {
          FnOnMessage(ex.Message);
        }
        Client.Close();
        FnOnMessage("... Disconnected.");
        await Task.Delay(50000);
      }
    }

    public void StartTCPServer() {
      // Establish the local endpoint for the socket.  
      // The DNS name of the computer  
      // running the listener is "host.contoso.com".  
      IPAddress ipAddress = IPAddress.Any;
      IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, Port);


      // Create a TCP/IP socket.  
      listener = new ExtSocket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

      // Bind the socket to the local endpoint and listen for incoming connections.  
      try {
        listener.Bind(localEndPoint);
        listener.Listen(100);

        // Start an asynchronous socket to listen for connections.  
        FnOnMessage($"Waiting for a connection on port {Port}...");
        listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
        IsConnected = true;

      } catch (Exception e) {
        FnOnMessage(e.Message, e);
      }

    }

    public void AcceptCallback(IAsyncResult ar) {
      //if (listener.Connected)
      FnOnMessage("Client is Connected...");
      //else
      //  return;
      // Get the socket that handles the client request.  
      //Socket listener = (Socket)ar.AsyncState;
      try {
        if (listener.IsDisposed)
          return;
        Socket handler = listener.EndAccept(ar);

        // Create the state object.  
        StateObject state = new StateObject();
        state.workSocket = handler;
        OnClientConnect?.Invoke(this, handler);

        FnOnMessage("Starting to Read...");
        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);


        FnOnMessage("Initilizing Next Client for connection...");
        listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
      } catch (Exception ex) {
        FnOnMessage(ex.Message, ex);
      }
    }

    public void ReadCallback(IAsyncResult ar) {
      String content = String.Empty;

      // Retrieve the state object and the handler socket  
      // from the asynchronous state object.  
      StateObject state = (StateObject)ar.AsyncState;
      Socket handler = state.workSocket;
      try {
        // Read data from the client socket.   
        int bytesRead = handler.EndReceive(ar);

        if (bytesRead > 0) {
          //Add data to received buffer
          byte[] buffer = state.buffer.Take(bytesRead).ToArray();
          OnDataReceive?.Invoke(this, buffer);

          //FnOnMessage($"Reading data of {bytesRead} bytes from Server...");
          WriteTo(buffer);

          // Not all data received. Get more.  
          handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
        } else {
          FnOnMessage($"Closing the client connection...");
          handler.Shutdown(SocketShutdown.Receive);
          handler.BeginDisconnect(true, new AsyncCallback(DisconnectCallback), state);
        }
      } catch (Exception ex) {
        FnOnMessage(ex.Message, ex);
      }
    }

    private void WriteTo(byte[] buffer) {
      try {
        //byte[] buffer = state.buffer.Take(bytesRead).ToArray();

        String LogPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CAT62Log");
        if (!System.IO.Directory.Exists(LogPath))
          System.IO.Directory.CreateDirectory(LogPath);
        String LogFile = Path.Combine(LogPath, DateTime.UtcNow.ToString("yyyy-MM-dd") + ".bin");
        using (var fileStream = new FileStream(LogFile, FileMode.Append, FileAccess.Write, FileShare.None)) {
          using (var bw = new BinaryWriter(fileStream)) {
            lock (_lockFile) {
              bw.Write(buffer);
            }
          }
        }
      } catch (Exception ex) {
        FnOnMessage(ex.Message, ex);
      }
    }

    public void DisconnectCallback(IAsyncResult ar) {
      try {
        // Retrieve the state object and the handler socket  
        // from the asynchronous state object.  
        StateObject state = (StateObject)ar.AsyncState;
        Socket handler = state.workSocket;
        if (handler != null) {
          handler.EndDisconnect(ar);
          handler.Close();
        }
        FnOnMessage($"... Client connection is closed.");
      } catch (Exception ex) {
        FnOnMessage(ex.Message, ex);
      }
    }

    private void FnOnMessage(String Message, Exception ex = null) {
      ADSIClientEventArgs e = new ADSIClientEventArgs {
        Message = Message,
        exception = ex
      };
      if (OnMessage != null)
        OnMessage(this, e);
    }//private void FnOnMessage
  }
}
