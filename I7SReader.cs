// Акатьев Алексей
// Резюме:
// Для приемлемой достоверности распознавания цифровых символов требуется участие оператора при настройке фильтров,
// использующихся перед запуском алгоритма распознавания. Это связано с освещенностью и загрязнением исходной картинки.
// Можно подобрать параметры, приведенные в коде, но они будут только подгонять к известному результату для
// заданных картинок
//
// Папку tessdata следует разместить в рабочем директории исполняемой программы


using System.Drawing;

namespace Ies7SegReader
{
  public class I7SReader
  {
    public struct Result7SReader
    {
      public string Text;
      public string TextColor;
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
