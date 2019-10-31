using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CAT62_Service.CAT62 {
  public class DataReceiver {
    public event EventHandler<List<CAT62.DataBlock>> OnBlockReceive;

    private List<byte> _ReceivedBuffer { get; set; } = new List<byte>{};
    //private List<DataBlock> DataBlocks = new List<DataBlock> { };
    
    private Boolean _IsProcessStarted = false;
    private object _lockBuffer = new object();
    private object _lockDataBlock = new object();

    public void AddBlock(byte[] Buffer) {
      //Add data to Block

      //Minimul lenth required is CAT (1 bit) + Length of Data (2 bit)
      if (Buffer.Count() <= 3)
        return;

      //Start with data that received CAT62;
      int CatSpecification = Buffer.First();
      if (CatSpecification != 62) {
        return;
      }

      byte[] DataBlockBytes = Buffer.Skip(3).ToArray();
      //AppLog.Add($"Received {DataBlockBytes.Count()}...");
      MultiDataBlock multiBlock = new MultiDataBlock();
      List<DataBlock> blocks = multiBlock.Parse(DataBlockBytes);
      //Raise the event once block is received
      OnBlockReceive?.Invoke(this, blocks);
    }

    public void Add(byte[] Buffer) {
      lock (_lockBuffer) {
        _ReceivedBuffer.AddRange(Buffer);

        if (_ReceivedBuffer.Count > 24632) {
          AppLog.Add($"Something is wrong with length {_ReceivedBuffer.Count}...");
          _RemoveUntilNextData();
        }
      }

      if (!_IsProcessStarted)
        Start();
    }


    public void _RemoveUntilNextData() {
      while (true) {
        if (_ReceivedBuffer.Count == 0)
          return;
        _ReceivedBuffer.RemoveAt(0);
        int CatSpecification = _ReceivedBuffer.First();
        if (CatSpecification == 62)
          return;
      }
    }


    public void Start() {
      _IsProcessStarted = true;
      Thread t = new Thread(RunThread);
      t.IsBackground = true;
      t.Priority = ThreadPriority.BelowNormal;
      t.Start();
    }

    public void RunThread() {
      while (true) {
        try {
          DataBlockThread();
        } catch (Exception ex) {
          AppLog.Add(ex.Message);
          AppLog.Add(ex.StackTrace);
          lock (_lockBuffer) {
            _ReceivedBuffer.Clear();
          }
        }
      }
    }

    private void DataBlockThread() {
      byte[] DataBlockBytes;

      //Minimul lenth required is CAT (1 bit) + Length of Data (2 bit)
      if (_ReceivedBuffer.Count() <= 3)
        return;

      //Start with data that received CAT62;
      int CatSpecification = _ReceivedBuffer.First();
      if (CatSpecification != 62) {
        lock (_lockBuffer)
          _ReceivedBuffer.RemoveAt(0);
        return;
      }
        

      //Get the length of Data
      byte[] _DataLengthBytes = new byte[] { _ReceivedBuffer.ElementAt(2), _ReceivedBuffer.ElementAt(1) };
      int DataLength = BitConverter.ToInt16(_DataLengthBytes, 0);
      if(DataLength < 16) {
        lock (_lockBuffer)
          _ReceivedBuffer.RemoveAt(0);
        return;
      }

      //If the data is not fully received, wait for next add
      if (_ReceivedBuffer.Count() < DataLength)
        return;


      //if the total length is received, make it as a block
      lock (_lockBuffer) { 
        DataBlockBytes = _ReceivedBuffer.Skip(3).Take(DataLength - 3).ToArray();
        _ReceivedBuffer.RemoveRange(0, DataLength);
      }

      //Add data to Block
      MultiDataBlock multiBlock = new MultiDataBlock();
      List<DataBlock> blocks = multiBlock.Parse(DataBlockBytes);
      //Raise the event once block is received
      OnBlockReceive?.Invoke(this, blocks);
    }

  }
}
