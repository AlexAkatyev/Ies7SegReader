using System.Drawing;

namespace Ies7SegReader
{
  public class I7SReader
  {
    public struct Result7SReader
    {
      public string Text;
      public double Data;
      public int ErrorCode;
    }

    private Bitmap? _bitmap = null;
    private I7SReaderWorker _worker = new I7SReaderWorker();


    public bool SetBitMapForRead(Bitmap bitmap)
    {
      _bitmap = bitmap;
      return LoadedBitmab();
    }


    public bool LoadedBitmab()
    {
      return _bitmap != null;
    }


    public string CodeText(int code)
    {
      return _worker.Errors(code);
    }


    public Result7SReader Read()
    {
      return _worker.Read(_bitmap);
    }
  }
}
