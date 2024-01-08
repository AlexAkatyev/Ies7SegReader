using System.Drawing;

namespace Ies7SegReader
{
  public class I7SReader
  {
    public struct Result7SReader
    {
      public string S;
      public double D;
      public int ErrorCode;
    }

    private Bitmap? _bitmap = null;


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
      string result = "";
      switch (code)
      {
        case 0:
          result = "Нет ошибки";
          break;
        case 1:
          result = "Чтение не производилось. Картинка не загружена";
          break;
        default:
          result = "Неизвестная ошибка";
          break;
      }
      return result;
    }


    public Result7SReader Read()
    {
      Result7SReader result = new Result7SReader
      {
        S = "",
        D = 0.0,
        ErrorCode = 0
      };

      if (!LoadedBitmab())
      {
        result.ErrorCode = 1;
        return result;
      }

      return result;
    }
  }
}
