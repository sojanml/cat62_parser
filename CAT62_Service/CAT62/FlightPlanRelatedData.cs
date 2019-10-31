using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAT62_Service.CAT62 {
  public class FlightPlanRelatedData {

    private byte[] _DataBlock;
    private Dictionary<int, Boolean> FSPEC { get; set; } = new Dictionary<int, bool>();

    public int FieldLength { get; private set; } = 0;
    public FlightPlanRelatedData() {

    }

    public FlightPlanRelatedData(byte[] ReceivedDataBlock) {
      _DataBlock = ReceivedDataBlock;
      SetFSPEC();
    }


    public void SetFSPEC() {
      int DataIndex = 0;
      int FSpecIndex = 0;
      while (true) {
        var bits = new BitArray(new byte[] { _DataBlock[DataIndex] });
        for (var i = 7; i >= 0; i--) {
          if (bits[i])
            FSPEC.Add(FSpecIndex, bits[i]);
          FSpecIndex++;
        }
        //Remove the extenstion from field index
        FSPEC.Remove(FSpecIndex - 1);

        if (!bits[0])
          break;
        DataIndex++;
      }



      String Binary = String.Empty;
      StringBuilder sbAvailableFields = new StringBuilder();
      int LengthIndex = 0;
      foreach (int i in FSPEC.Keys) {
        Binary = Binary + (FSPEC[i] ? "1 " : "0 ");
        if (FSPEC[i]) {
          LengthIndex = LengthIndex + FieldMaping.FlightPlanRelatedData[i].FieldLength;
          sbAvailableFields.AppendLine(
            FieldMaping.FlightPlanRelatedData[i].Code + " - " + FieldMaping.FlightPlanRelatedData[i].Name +
            ", Length: " + FieldMaping.FlightPlanRelatedData[i].FieldLength +
            (FieldMaping.FlightPlanRelatedData[i].DynamicLength ? " (Dynamic)" : "")
          );
        }
      }

      FieldLength = 1 + DataIndex + LengthIndex;

      //Console.WriteLine(Binary);
      //Console.WriteLine(sbAvailableFields.ToString());

    }
  }
}
