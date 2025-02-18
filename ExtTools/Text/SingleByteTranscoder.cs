using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Core;

namespace FreeLibSet.Text
{

  /// <summary>
  /// Прямой перекодировщик byte[]-byte[] без промежуточного преобразования в строку.
  /// Может быть применен только к однобайтным кодовым страницам
  /// </summary>
  public class SingleByteTranscoder
  {
    #region Конструктор

    /// <summary>
    /// Создает перекодировщик
    /// </summary>
    /// <param name="srcEncoding">Исходная кодировка</param>
    /// <param name="resEncoding">Конечная кодировка</param>
    public SingleByteTranscoder(Encoding srcEncoding, Encoding resEncoding)
    {
      if (srcEncoding == null)
        throw new ArgumentNullException("srcEncoding");
      if (resEncoding == null)
        throw new ArgumentNullException("resEncoding");

      if (!CanCreate(srcEncoding, resEncoding))
        throw new ArgumentException(String.Format(Res.SingleByteTranscoder_Arg_Unsupported, srcEncoding, resEncoding));

      _SrcEncoding = srcEncoding;
      _ResEncoding = resEncoding;
      _IsDirect = (resEncoding.CodePage == srcEncoding.CodePage);

      if (!_IsDirect)
      {
        TranscodeTable = new byte[256];
        for (int i = 0; i < 256; i++)
          TranscodeTable[i] = (byte)i;

        // Преобразуется только вторая половина таблицы
        string s = srcEncoding.GetString(TranscodeTable, 128, 128);
        resEncoding.GetBytes(s, 0, 128, TranscodeTable, 128);
      }
    }

    /// <summary>
    /// Возвращает true, если можно создать перекодировщик для заданных кодировок
    /// </summary>
    /// <param name="srcEncoding">Исходная кодировка</param>
    /// <param name="resEncoding">Конечная кодировка</param>
    /// <returns>true, если обе кодировки являются однобайтными</returns>
    public static bool CanCreate(Encoding srcEncoding, Encoding resEncoding)
    {
      return srcEncoding.IsSingleByte && resEncoding.IsSingleByte;
    }

    #endregion

    #region Основные свойства

    /// <summary>
    /// Исходная кодировка
    /// </summary>
    public Encoding SrcEncoding { get { return _SrcEncoding; } }
    private readonly Encoding _SrcEncoding;

    /// <summary>
    /// Конечная кодировка
    /// </summary>
    public Encoding ResEncoding { get { return _ResEncoding; } }
    private readonly Encoding _ResEncoding;

    /// <summary>
    /// Возвращает true, если исходная и конечная кодировки совпадают и перекодирование не нужно.
    /// Вместо него можно использовать прямое копирование
    /// </summary>
    public bool IsDirect { get { return _IsDirect; } }
    private bool _IsDirect;

    /// <summary>
    /// Текстовое представление для отладки
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return SrcEncoding.CodePage.ToString() + "->" + ResEncoding.CodePage.ToString();
    }

    #endregion

    #region Перекодирование

    /// <summary>
    /// Таблица перекодирования размером 256 байт, если IsDirect=false
    /// </summary>
    private byte[] TranscodeTable;

    /// <summary>
    /// Выполнить преобразование byte[]-byte[] для части массива.
    /// Если <see cref="IsDirect"/>=true, вызывается <see cref="System.Array"/>.Copy()
    /// </summary>
    /// <param name="srcArray">Исходный массив байт, задающий символы в кодировке <see cref="SrcEncoding"/></param>
    /// <param name="srcIndex">Начальная позиция в массиве <paramref name="srcArray"/></param>
    /// <param name="resArray">Заполняемый массив байт, задающий символы в кодировке <see cref="ResEncoding"/></param>
    /// <param name="resIndex">Начальная позиция в массиве <paramref name="resArray"/></param>
    /// <param name="length">Количество байт для перекодировки</param>
    public void Transcode(byte[] srcArray, int srcIndex, byte[] resArray, int resIndex, int length)
    {
#if DEBUG
      if (srcArray == null)
        throw new ArgumentNullException("srcArray");
      if (resArray == null)
        throw new ArgumentNullException("resArray");

      //if (srcArray.Rank != 1)
      //  throw new ArgumentException("Исходный массив должен быть одномерным", "srcArray");
      //if (resArray.Rank != 1)
      //  throw new ArgumentException("Заполняемый массив должен быть одномерным", "resArray");
#endif

      if (_IsDirect)
        Array.Copy(srcArray, srcIndex, resArray, resIndex, length);
      else
      {
        for (int i = 0; i < length; i++)
        {
          int x = srcArray[srcIndex + i];
          resArray[resIndex + i] = TranscodeTable[x];
        }
      }
    }

    /// <summary>
    /// Выполнить преобразование byte[]-byte[] для целого массива.
    /// Если <see cref="IsDirect"/>=true, вызывается <see cref="System.Array"/>.Copy()
    /// </summary>
    /// <param name="srcArray">Исходный массив байт, задающий символы в кодировке <see cref="SrcEncoding"/></param>
    /// <param name="resArray">Заполняемый массив байт, задающий символы в кодировке <see cref="ResEncoding"/></param>
    public void Transcode(byte[] srcArray, byte[] resArray)
    {
#if DEBUG
      if (srcArray == null)
        throw new ArgumentNullException("srcArray");
      if (resArray == null)
        throw new ArgumentNullException("resArray");
#endif
      if (resArray.Length != srcArray.Length)
        throw ExceptionFactory.ArgWrongCollectionCount("resArray", resArray, srcArray.Length);
      Transcode(srcArray, 0, resArray, 0, srcArray.Length);
    }

    #endregion
  }
}
