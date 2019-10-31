using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAT62_Service.CAT62 {


  public class DataBlock {
    public int DataLength { get; private set; } = 0;
    private byte[] _DataBlock { get; set; }
    private Dictionary<int, Boolean> FSPEC { get; set; } = new Dictionary<int, bool>();



    public DateTime TimeOfTrack { get; private set; } = DateTime.MinValue;
    public LatLng Location { get; private set; } = new LatLng();
    public int Altitude { get; private set; } = 0;
    public String TrackNumber { get; private set; } = String.Empty;
    public Velocity CalcuatedVelocity { get; set; } = new Velocity();
    public DataBlock(byte[] Block) {
      if (Block.Length < 16)
        return;
      _DataBlock = Block;
      SetFSPEC();
      StartParsingOfData();
    }

    public AircraftDerivedData AircraftData { get; set; } = new AircraftDerivedData();
    public FlightPlanRelatedData FlightPlanData { get; set; } = new FlightPlanRelatedData();

    private void StartParsingOfData() {
      StringBuilder sbAvailableFields = new StringBuilder();

      foreach (int FieldIndex in FSPEC.Keys) {
        DataField FieldMap = FieldMaping.DataModel[FieldIndex];
        int FieldLength = FieldMaping.DataModel[FieldIndex].FieldLength;
        byte[] BytesToParse = { };
        if (!FieldMap.DynamicLength)
          BytesToParse = _DataBlock.Skip(DataLength + 1).Take(FieldMap.FieldLength).ToArray();

        switch (FieldMap.Code) {
          case "I062/070":
            TimeOfTrack = GetParsedTime(BytesToParse);
            break;
          case "I062/105":
            Location = GetParsedLocation(BytesToParse);
            break;
          case "I062/135":
            Altitude = GetAltitude(BytesToParse);
            break;
          case "I062/040":
            TrackNumber = BitConverter.ToString(BytesToParse).Replace("-", "");
            break;
          case "I062/185":
            CalcuatedVelocity = GetVelocity(BytesToParse);
            break;
          case "I062/380":
            //Dynamic Column - need to add the field length
            byte[] _DataToParse1 = _DataBlock.Skip(DataLength + 1).ToArray();
            AircraftData = new AircraftDerivedData(_DataToParse1);
            FieldLength = AircraftData.FieldLength;
            //BytesToParse = _DataBlock.Skip(DataLength + 1).Take(FieldLength).ToArray();
            break;
          case "I062/080":
            int TrackFieldLength = GetFieldLengthAt(DataLength + 1);
            FieldLength = TrackFieldLength;
            //BytesToParse = _DataBlock.Skip(DataLength + 1).Take(FieldLength).ToArray();
            break;
          case "I062/390":
            //Dynamic Column - need to add the field length
            byte[] _DataToParse3 = _DataBlock.Skip(DataLength + 1).ToArray();
            FlightPlanData = new FlightPlanRelatedData(_DataToParse3);
            FieldLength = FlightPlanData.FieldLength;
            //BytesToParse = _DataBlock.Skip(DataLength + 1).Take(FieldLength).ToArray();
            break;
          default:
            //Console.WriteLine("Not Processed: " + FieldMaping.DataModel[FieldIndex].Code);
            break;
        }

        //Move to next data Index
        DataLength = DataLength + FieldLength;
        /*
        sbAvailableFields.AppendLine(
          FieldMaping.DataModel[FieldIndex].Code + " - " + FieldMaping.DataModel[FieldIndex].Name +
          ", Length: " + FieldLength + " of " + DataLength +
          (FieldMaping.DataModel[FieldIndex].DynamicLength ? " (Dynamic)" : "") +
          "  [" +   BitConverter.ToString(BytesToParse) + "]"
        );
        */
      }
      //Console.WriteLine(sbAvailableFields.ToString());
      //Console.WriteLine(BitConverter.ToString(_DataBlock.Take(DataLength+1).ToArray()));

    }


    private DateTime GetParsedTime(byte[] TimeInBytes) {
      byte[] bTime = new byte[] { TimeInBytes[2], TimeInBytes[1], TimeInBytes[0], 0 };
      int timeInDec = BitConverter.ToInt32(bTime, 0);
      long timeInSec = timeInDec / 128;
      TimeSpan TimeOfDay = TimeSpan.FromSeconds(timeInSec);
      DateTime Now = DateTime.UtcNow;

      return new DateTime(Now.Year, Now.Month, Now.Day, TimeOfDay.Hours, TimeOfDay.Minutes, TimeOfDay.Seconds);
    }

    private int GetAltitude(byte[] AltitudeInBytes) {
      byte b = AltitudeInBytes[0];
      b &= byte.MaxValue ^ (1 << 7);

      byte[] bAlt = new byte[] { AltitudeInBytes[1], b };
      int Alt = BitConverter.ToUInt16(bAlt, 0);
      //Shift to Left 1 bit
      Alt = Alt * 25;
      return (int)Alt;
    }

    private LatLng GetParsedLocation(byte[] LocationInBytes) {
      byte[] bLat = new byte[] { LocationInBytes[3], LocationInBytes[2], LocationInBytes[1], LocationInBytes[0] };
      byte[] bLng = new byte[] { LocationInBytes[7], LocationInBytes[6], LocationInBytes[5], LocationInBytes[4] };
      Double Lat = BitConverter.ToInt32(bLat, 0) * FieldMaping.WGS84;
      Double Lng = BitConverter.ToInt32(bLng, 0) * FieldMaping.WGS84;
      return new LatLng { Lat = Lat, Lng = Lng };
    }

    private Velocity GetVelocity(byte[] VelocityInBytes) {
      byte[] bVx = new byte[] { VelocityInBytes[1], VelocityInBytes[0] };
      byte[] bVy = new byte[] { VelocityInBytes[3], VelocityInBytes[2], };
      Double Vx = BitConverter.ToInt16(bVx, 0) * 0.25D;
      Double Vy = BitConverter.ToInt16(bVy, 0) * 0.25D;

      Velocity CV = new Velocity();
      CV.SetV(Vx, Vy);
      return CV;
    }

    public int Length {
      get {
        return _DataBlock.Length;
      }
    }

    public void SetFSPEC() {

      int FSpecIndex = 0;
      while (true) {
        var bits = new BitArray(new byte[] { _DataBlock[DataLength] });
        for (var i = 7; i >= 0; i--) {
          if (bits[i])
            FSPEC.Add(FSpecIndex, bits[i]);
          FSpecIndex++;
        }
        //Remove the extenstion from field index
        FSPEC.Remove(FSpecIndex - 1);

        if (!bits[0])
          break;
        DataLength++;
      }


      /*
      String Binary = String.Empty;
      StringBuilder sbAvailableFields = new StringBuilder();
      int LengthIndex = 0;
      foreach (int i in FSPEC.Keys) {
        Binary = Binary + (FSPEC[i] ? "1 " : "0 ");
        if (FSPEC[i]) {
          LengthIndex = LengthIndex + FieldMaping.DataModel[i].FieldLength;
          sbAvailableFields.AppendLine(
            FieldMaping.DataModel[i].Code + " - " + FieldMaping.DataModel[i].Name +
            ", Length: " + FieldMaping.DataModel[i].FieldLength +
            (FieldMaping.DataModel[i].DynamicLength ? " (Dynamic)" : "")
          );
        }
      }

      Console.WriteLine(Binary);
      Console.WriteLine(sbAvailableFields.ToString());
      */
    }


    private int GetFieldLengthAt(int FieldIndex) {
      int FieldLength = 0;
      while (true) {
        var bits = new BitArray(new byte[] { _DataBlock[FieldIndex + FieldLength] });
        if (!bits[0])
          break;
        FieldLength++;
      };
      return FieldLength + 1;
    }


  }
}
