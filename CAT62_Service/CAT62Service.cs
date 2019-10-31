using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace CAT62_Service {
  public partial class CAT62Service : ServiceBase {


    private AsynchronousSocketListener cat62 = new AsynchronousSocketListener();
    private Dictionary<String, CAT62.LatLng> SavePosition = new Dictionary<string, CAT62.LatLng>();
    private CAT62.DataReceiver dataReceiver = new CAT62.DataReceiver();
    private String PortType = "TCP";
    private SqlConnection cn;


    public CAT62Service() {
      InitializeComponent();
    }

    protected override void OnStart(string[] args) {
      Log("Starting Service...");
      InitSettings();

      cat62.OnMessage += OnMessage;
      cat62.OnDataReceive += OnDataReceive;
      if (cat62.ClearRecordsOnConnect)
        cat62.OnClientConnect += OnClientConnect;
      OnMessage(new object(), new ADSIClientEventArgs { Message = "Initilizing the connection..." });
      cat62.StartServer();

      dataReceiver.OnBlockReceive += OnBlockReceive;
    }

    protected override void OnStop() {
      Log("Stopping Service...");
      try {
        cat62.StopServer();
      } catch(Exception ex) {
        Log(ex.Message);
      }
      Log("Service Stopped...");
    }


    private void Log(String Message) {
      CAT62.AppLog.Add($"{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")} {Message}\r\n");
    }

    private void OnMessage(object sender, ADSIClientEventArgs e) {
      CAT62.AppLog.Add($"{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")} { e.Message}\r\n");
      if(e.exception != null) {
        CAT62.AppLog.Add(e.exception.StackTrace + "\r\n");
      }        
      //Environment.Exit(0);
    }

    private void OnDataReceive(object sender, byte[] buffer) {
      //dataReceiver.Add(buffer);
      dataReceiver.AddBlock(buffer);
      
      //lblData.Invoke(new Action(() => lblData.Text = $"Updated On: {DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff")}"));
    }

    private void OnClientConnect(object sender, System.Net.Sockets.Socket socket) {
      OnMessage(this, new ADSIClientEventArgs { Message = "Deleting existing records from server..." });
      var cmd = cn.CreateCommand();
      cmd.CommandText = "DELETE FROM ADSBLive";
      cmd.ExecuteNonQuery();
    }

    private void OnBlockReceive(object sender, List<CAT62.DataBlock> blocks) {
      //OnMessage(this, new ADSIClientEventArgs { Message = $"Found  {blocks.Count} blocks to process" });

      foreach (CAT62.DataBlock block in blocks) {
        //If No Position, do not insert
        if (block.Location.Lat == 0 && block.Location.Lng == 0) {
          continue;
        }


        CAT62.ADSB adsb = new CAT62.ADSB {
          ADSBDate = block.TimeOfTrack,
          Altitude = block.Altitude,
          Heading = block.CalcuatedVelocity.Heading,
          Speed = block.CalcuatedVelocity.Speed,
          Latitude = block.Location.Lat,
          Longtitude = block.Location.Lng,
          HexCode = String.IsNullOrWhiteSpace(block.AircraftData.TargetAddress) ? block.TrackNumber : block.AircraftData.TargetAddress,
          Registration = String.IsNullOrWhiteSpace(block.AircraftData.TargetIdentification) ? block.TrackNumber : block.AircraftData.TargetIdentification
        };
        try {
          adsb.Update(cn);
        } catch (Exception ex) {
          String ADSBJson = Newtonsoft.Json.JsonConvert.SerializeObject(adsb);
          OnMessage(this, new ADSIClientEventArgs {
            Message = ex.Message,
            exception = ex
          });
          CAT62.AppLog.Add(ADSBJson + "\r\n\r\n");
        }
      }



      //lblData.Invoke(new Action(() => lblData.Text = $"Updated On: {DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff")}"));
      //lblData.Invoke(new Action(() => lblData.Text = "Done"));
      //lblData.Text = "Done";
    }


    private bool InitSettings() {
      try {
        OnMessage(this, new ADSIClientEventArgs { Message = "Connecting to database server..." });
        String DSN = ConfigurationManager.AppSettings["DSN"];
        cn = new SqlConnection(DSN);
        cn.Open();
        OnMessage(this, new ADSIClientEventArgs { Message = "... Connected" });

        String sPort = ConfigurationManager.AppSettings["Port"];
        int iPort = cat62.Port;
        int.TryParse(sPort, out iPort);

        String sPortType = ConfigurationManager.AppSettings["PortType"];
        if (sPortType.Equals("TCP", StringComparison.InvariantCultureIgnoreCase)) {
          PortType = "TCP";
        } else if (sPortType.Equals("UDP", StringComparison.InvariantCultureIgnoreCase)) {
          PortType = "UDP";
        }

        if (iPort > 0)
          cat62.Port = iPort;
        cat62.PortType = PortType;

        OnMessage(this, new ADSIClientEventArgs { Message = $"Listening to port {cat62.Port} of type {cat62.PortType}..." });

        String sClearRecordsOnConnect = ConfigurationManager.AppSettings["ClearRecordsOnConnect"];
        cat62.ClearRecordsOnConnect = sClearRecordsOnConnect.Equals("True", StringComparison.InvariantCultureIgnoreCase);
        if (cat62.ClearRecordsOnConnect)
          OnMessage(this, new ADSIClientEventArgs { Message = $"Clear all records when connection is made..." });

      } catch (Exception ex) {
        OnMessage(this, new ADSIClientEventArgs {
          Message = ex.Message,
          exception = ex
        });
        return false;
      }

      return true;
    }




  }
}
