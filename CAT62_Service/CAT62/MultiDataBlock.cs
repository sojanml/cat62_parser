using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAT62_Service.CAT62 {
  public class MultiDataBlock {
    internal List<DataBlock> Parse(byte[] dataBlockBytes) {
      List<DataBlock> Blocks = new List<DataBlock>();
      int DataBlockCount = 0;

      try {
        while (true) {
          DataBlockCount = DataBlockCount + 1;
          DataBlock block = new DataBlock(dataBlockBytes);
          Blocks.Add(block);
          int LengthOfRecord = block.DataLength;
          if (dataBlockBytes.Length == LengthOfRecord + 1)
            break;
          dataBlockBytes = dataBlockBytes.Skip(LengthOfRecord + 1).ToArray();
        }

      } catch(Exception ex) {
        //If there is any error, ignore it
      }
      //Console.WriteLine($"Total {Blocks.Count} Processed...");

      return Blocks;
    }
  }
}
