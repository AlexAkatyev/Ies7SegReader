﻿using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Drawing;
using static Ies7SegReader.I7SReader;
using Tesseract;

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
        Text = "",
        TextColor = "",
        Data = 0.0,
        ErrorCode = 0
      };

      if (bm == null)
      {
        result.ErrorCode = 1;
        return result;
      }

      // получаем фильтрованную бинарную картинку для анализа
      Mat m = filterAndBinarization(bm);

      // проводим анализ
      result.Text = getTextFromMat(m, true);

      if (result.Text.Length == 0) // Если ничего не прочитали, а текст есть, надо сделать шире символы
      {
        byte[] kernelValues = [ 0, 1, 0,
                                1, 1, 1,
                                0, 1, 0 ];
        Mat kernel1 = new Mat(3, 3, MatType.CV_8UC1, kernelValues);
        int countIter = m.Height / 10;
        for (int iter = 0; iter < countIter && result.Text.Length == 0; iter++)
        {
          Cv2.Erode(m, m, kernel1); // делается эрозия фона, т.к. фон = 255, а цифра = 0
          result.Text = getTextFromMat(m, true);
        }
      }


      // работа с цветной картинкой
      Mat c = loadBitmap(bm);
      result.TextColor = getTextFromMat(c, false);

      // коррректировка полученной строки
      result.Text = correctResult(result.Text);
      result.TextColor = correctResult(result.TextColor);

      return result;
    }


    private string correctResult(string input)
    {
      string result = "";
      for (int i = 0; i < input.Length; i++)
      {
        if (input[i] >= 48
            && input[i] <= 57)
          result += input[i];
        else if (input[i] == '-')
        {
          if (i == 0)
            result += input[i];
          else
            result += '.';
        }
        else if (input[i] == ','
            || input[i] == '.')
          result += '.';
        else if (input[i] == 'B')
          result += '8';
        else if (input[i] == 'g'
            || input[i] == 'Y')
          result += '9';
        else if (input[i] == 'Z'
            || input[i] == 'C')
          result += '2';
        else if (input[i] == 'Q'
            || input[i] == 'O'
            || input[i] == 'o'
            || input[i] == 'D'
            || input[i] == 'U')
          result += '0';
        else if (input[i] == '|'
            || input[i] == 'I'
            || input[i] == 'l')
          result += '1';
      }

      if (!result.Contains('.'))
      {
        if (result.Length > 1
            && result[0] == '0'
            && result[1] == '0')
          result = result.Insert(1, ".");
        else if (result.Length > 2
            && result[0] == '-'
            && result[1] == '0'
            && result[2] == '0')
          result = result.Insert(2, ".");
      }

      return result;
    }

    private string getTextFromMat(Mat a, bool isGray)
    {
      Mat m = new Mat();
      if (isGray)
        Cv2.CvtColor(a, m, ColorConversionCodes.GRAY2RGBA);
      else
        m = a;
      string tempFile = "temp.png";
      m.SaveImage(tempFile);

      Pix img = Pix.LoadFromFile(tempFile);

      string lang = "7seg";
      TesseractEngine tesseractEng = new TesseractEngine(@".\tessdata", lang, EngineMode.Default);
      Page page = tesseractEng.Process(img, PageSegMode.SingleLine); // SingleLine -psm 7 SingleWord -psm 8 +
      return page.GetText();
    }


    private Mat loadBitmap(Bitmap bm)
    {
      // Загрузка картинки
      Mat inputMaterial = BitmapConverter.ToMat(bm);
      return inputMaterial;
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

      // раздвигеам символы друг от друга для удобной работы tesseract
      //inputMaterial = moveApart(inputMaterial);

      return inputMaterial;
    }


    private Mat moveApart(Mat m)
    {
      // раздвигание символов
      int w = m.Width;
      int h = m.Height;
      int dx = h / 3;
      m.GetArray<byte>(out byte[] b);
      List<byte> vw = new List<byte>(b);

      const byte ONE = 255;
      const byte ZERO = 0;
      var getPosLine = (int width, int height, int i, int j, int k) =>
      {
        return k * width + i + (j - i) * (height - k) / height;
      };

      for (int i = w - dx - 1; i > -1; i--)
        for (int j = i + dx; j >= i; j--)
        {
          bool rastrOpen = true;
          for (int k = 0; k < h; k++)
            rastrOpen = rastrOpen && vw[getPosLine(w, h, i, j, k)] == ONE;
          if (rastrOpen)
          {
            for (int k = h - 1; k > -1; k--)
              vw.Insert(getPosLine(w, h, i, j, k), ONE);
            w += 1;
          }
        }

      byte[] aw = vw.ToArray();

      return new Mat(h, w, MatType.CV_8UC1, aw);
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
