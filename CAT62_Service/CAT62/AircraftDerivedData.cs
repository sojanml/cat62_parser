using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAT62_Service.CAT62 {
  public class AircraftDerivedData {
    private byte[] _DataBlock;
    private Dictionary<int, Boolean> FSPEC { get; set; } = new Dictionary<int, bool>();
    private int FSpecIndex { get;  set; } = 0;
    private int DataIndex { get;  set; } = 0;

    public int FieldLength { get; private set; } = 0;
    
    public String TargetIdentification { get; set; } = String.Empty;
    public String TargetAddress { get; set; } = String.Empty;

    public AircraftDerivedData() {

    }

    public AircraftDerivedData(byte[] ReceivedDataBlock) {
      _DataBlock = ReceivedDataBlock;
      SetFSPEC();
      Parse();
    }

    private void Parse() {
      //Console.WriteLine(BitConverter.ToString(_DataBlock).ToArray());

      int DataLength = DataIndex;
      //ID (bit-31) -  Subfield #2: Target Identification
      foreach (int FieldIndex in FSPEC.Keys) {
        DataField FieldMap = FieldMaping.AircraftDerivedData[FieldIndex];
        int FieldLength = FieldMaping.AircraftDerivedData[FieldIndex].FieldLength;
        byte[] BytesToParse = { };
        if (!FieldMap.DynamicLength)
          BytesToParse = _DataBlock.Skip(DataLength + 1).Take(FieldMap.FieldLength).ToArray();
        switch (FieldMap.Code) {
          case "ADR":
            TargetAddress = BitConverter.ToString(BytesToParse).Replace("-", string.Empty);
            break;
          case "ID":
            TargetIdentification = GetTargetIdentification(BytesToParse);
            break;
        }
        DataLength = DataLength + FieldLength;
      }
    }

    private String GetTargetIdentification(byte[] BytesToParse) {
      //Console.WriteLine(BitConverter.ToString(BytesToParse).ToArray());
      BitArray Bits = new BitArray(48);
      int BitsPosition = 0;
      for(var i = 0; i < BytesToParse.Length; i++ ) {
        BitArray thisBit = new BitArray(new byte[] { BytesToParse[i] });
        for(var x = 7; x >= 0; x--) {
          Bits.Set(BitsPosition, thisBit.Get(x));
          BitsPosition++;
        }
      }


      StringBuilder SB = new StringBuilder();      

      for(var Index = 0; Index < 42; Index+=6) {
        byte b = 0;
        if (Bits.Get(Index + 5))
          b++;
        if (Bits.Get(Index + 4))
          b += 2;
        if (Bits.Get(Index + 3))
          b += 4;
        if (Bits.Get(Index + 2))
          b += 8;
        if (Bits.Get(Index + 1))
          b += 16;
        if (Bits.Get(Index + 0))
          b += 32;
        //if (Bits.Get(1))
        //  b += 64;
        //if (Bits.Get(0))
        //  b += 128;

        if(b == 32) {
          //White Space - Ignore
        } else if(Encode6Bit.Char.ContainsKey(b)) {
          SB.Append(Encode6Bit.Char[b]);
        } else {
          SB.Append("-");
        }
      }
      return SB.ToString();
    }



    private void SetFSPEC() {
      int LengthIndex = 0;

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

      foreach (int i in FSPEC.Keys) {
        if (FSPEC[i]) {
          LengthIndex = LengthIndex + FieldMaping.AircraftDerivedData[i].FieldLength;
        }
      }
      FieldLength = 1 + DataIndex + LengthIndex;

      /*
      String Binary = String.Empty;
      StringBuilder sbAvailableFields = new StringBuilder();
      foreach (int i in FSPEC.Keys) {
        Binary = Binary + (FSPEC[i] ? "1 " : "0 ");
        if (FSPEC[i]) {
          sbAvailableFields.AppendLine(
            FieldMaping.AircraftDerivedData[i].Code + " - " + FieldMaping.AircraftDerivedData[i].Name +
            ", Length: " + FieldMaping.AircraftDerivedData[i].FieldLength +
            (FieldMaping.AircraftDerivedData[i].DynamicLength ? " (Dynamic)" : "")
          );
        }
      }
      */
      
      //Console.WriteLine(Binary);
     //Console.WriteLine(sbAvailableFields.ToString());

    }

  }
}
