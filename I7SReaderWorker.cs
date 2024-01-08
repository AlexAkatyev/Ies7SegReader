using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Drawing;
using static Ies7SegReader.I7SReader;

namespace Ies7SegReader
{
  internal class I7SReaderWorker
  {
    public string Errors(int code)
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


    public Result7SReader Read(Bitmap? bm)
    {
      Result7SReader result = new Result7SReader
      {
        S = "",
        D = 0.0,
        ErrorCode = 0
      };

      if (bm == null)
      {
        result.ErrorCode = 1;
        return result;
      }

      // получамем фильтрованную бинарную картинку для анализа
      Mat m = filterAndBinarization(bm);

      // проводим анализ

      return result;
    }


    private Mat filterAndBinarization(Bitmap bm)
    {
      // Загрузка картинки
      Mat inputMaterial = BitmapConverter.ToMat(bm);

      // усиление цифр на общем фоне (осветление картинки)
      correctGamma(inputMaterial, inputMaterial, 1.2);

      // переход в оттенки серого
      Cv2.CvtColor(inputMaterial, inputMaterial, ColorConversionCodes.RGB2GRAY);

      // подготовка фильтра нижних частот для очистки фона
      float[] data = [ 1, 1, 1,
                       1, 1, 1,
                       1, 1, 1 ];
      Mat kernel = new Mat(rows: 3, cols: 3, type: MatType.CV_8UC1, data: data);
      Cv2.Normalize(src: kernel, dst: kernel, alpha: 1, beta: 0, normType: NormTypes.L2);

      // снова убираем шумы
      Cv2.Filter2D(inputMaterial, inputMaterial, inputMaterial.Type(), kernel, new OpenCvSharp.Point(0, 0));

      // Затемнение картинки и перевод в бинарную картинку
      correctGamma(inputMaterial, inputMaterial, 0.5);
      Cv2.Threshold(inputMaterial, inputMaterial, 200, 255, ThresholdTypes.Binary);

      return inputMaterial;
    }


    private void correctGamma(Mat src, Mat dst, double gamma)
    {
      byte[] lut = new byte[256];
      for (int i = 0; i < lut.Length; i++)
        lut[i] = (byte)(Math.Pow(i / 255.0, 1.0 / gamma) * 255.0);

      Cv2.LUT(src, lut, dst);
    }
  }
}
