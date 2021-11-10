using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using FreeLibSet.Core;

/*
 * The BSD License
 * 
 * Copyright (c) 2006-2015, Ageyev A.V.
 * 
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, 
 * this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice, 
 * this list of conditions and the following disclaimer in the documentation 
 * and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE 
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

namespace FreeLibSet.Drawing
{
  /// <summary>
  /// Функции для работы с графикой
  /// </summary>
  public static class ImagingTools
  {
    #region Поиск ближайшего цвета

    /// <summary>
    /// Найти индекс ближайшего цвета в массиве цветов (палитре)
    /// </summary>
    /// <param name="array">Массив цветов, в котором выполлняется поиск (обычно свойство ColorPalette.Entries)</param>
    /// <param name="color">Цвет, который нужно найти</param>
    /// <returns></returns>
    public static int GetNearestColorIndex(Color[] array, Color color)
    {
      int ResIndex = -1;
      int MinDiff = 1000;

      for (int i = 0; i < array.Length; i++)
      {
        Color ThisColor = array[i];

        int ThisDiff = 
          Math.Abs(ThisColor.R - color.R) + 
          Math.Abs(ThisColor.G - color.G) + 
          Math.Abs(ThisColor.B - color.B);

        if (ThisDiff < MinDiff)
        {
          if (ThisDiff == 0)
            return i; // точное совпадение

          ResIndex = i;
          MinDiff = ThisDiff;
        }
      }

      return ResIndex;
    }

    #endregion

    #region Преобразование формата Bitmap

    /*
     */

    /// <summary>
    /// Преобразование цветного 24-битного изображения в монохромное, 4-цветное
    /// или 256-цветное изображение, использущее палитру
    /// Метод Bitmap.Clone() может принимать аргумент PixelFormat и менять формат 
    /// хранения изображения. К сожалению, это не работает (всегда?) под Windows-XP
    /// </summary>
    /// <param name="source">Исходное изображение с PixelFormat=Format24bppRgb</param>
    /// <param name="resFormat">Требуемый формат (Format1bppIndexed, Format4bppIndexed или Format8bppIndexed)</param>
    /// <returns>Преобразованное изображение</returns>
    public static Bitmap ConvertToIndexed(Bitmap source, PixelFormat resFormat)
    {
      if (source == null)
        throw new ArgumentNullException("source");
      if (source.PixelFormat != PixelFormat.Format24bppRgb)
        throw new ArgumentException("Неверный формат исходного изображения: " + source.PixelFormat.ToString() + ". Ожидалось: " + PixelFormat.Format24bppRgb.ToString(), "source");

      switch (resFormat)
      {
        case PixelFormat.Format1bppIndexed:
        case PixelFormat.Format4bppIndexed:
        case PixelFormat.Format8bppIndexed:
          break;
        default:
          throw new ArgumentException("Неподдерживаемый выходной формат: " + resFormat.ToString(), "resFormat");
      }

      Dictionary<Color, int> colorIndices = new Dictionary<Color, int>();

      Bitmap Res;

      Rectangle rc = new Rectangle(0, 0, source.Width, source.Height);
      BitmapData SrcData = source.LockBits(rc, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
      try
      {
        Res = new Bitmap(source.Width, source.Height, resFormat);
        try
        {
          ColorPalette Palette = Res.Palette;

          BitmapData ResData = Res.LockBits(rc, ImageLockMode.ReadWrite, resFormat);
          try
          {
            byte[] SrcA = new byte[SrcData.Height * SrcData.Stride];
            byte[] ResA = new byte[ResData.Height * ResData.Stride];
            DataTools.FillArray<byte>(ResA, 0);
            Marshal.Copy(SrcData.Scan0, SrcA, 0, SrcA.Length);

            for (int i = 0; i < source.Height; i++)
            {
              int SrcStartIndex = SrcData.Stride * i;
              int ResStartIndex = ResData.Stride * i;

              for (int j = 0; j < SrcData.Width; j++)
              {
                int SrcIdx = SrcStartIndex + 3 * j;
                Color SrcColor = Color.FromArgb(SrcA[SrcIdx + 2], SrcA[SrcIdx + 1], SrcA[SrcIdx + 0]);

                int ResColorIndex;
                if (!colorIndices.TryGetValue(SrcColor, out ResColorIndex))
                {
                  // Находим ближайший цвет
                  ResColorIndex = GetNearestColorIndex(Palette.Entries, SrcColor);
                  // Запоминаем для будущего использования
                  colorIndices.Add(SrcColor, ResColorIndex);
                }

                switch (resFormat)
                {
                  case PixelFormat.Format1bppIndexed:
                    if (ResColorIndex == 1)
                    {
                      int ResIdx = ResStartIndex + (j / 8);
                      int Pix = j % 8;
                      int b = ResA[ResIdx];
                      int v = 0x80 >> Pix;
                      b = b | v;
                      ResA[ResIdx] = (byte)b;
                    }
                    break;

                  case PixelFormat.Format4bppIndexed:
                    if (ResColorIndex != 0)
                    {
                      int ResIdx = ResStartIndex + (j / 2);
                      int b = ResA[ResIdx];
                      int v = ResColorIndex;
                      if ((j % 2) == 0)
                        v = v << 4;
                      b = b | v;
                      ResA[ResIdx] = (byte)b;
                    }
                    break;

                  case PixelFormat.Format8bppIndexed:
                    ResA[ResStartIndex + j] = (byte)ResColorIndex;
                    break;
                }
              }
            }

            Marshal.Copy(ResA, 0, ResData.Scan0, ResA.Length);
          }
          finally
          {
            Res.UnlockBits(ResData);
          }
        }
        catch
        {
          Res.Dispose();
          throw;
        }
      }
      finally
      {
        source.UnlockBits(SrcData);
      }

      return Res;
    }

    #endregion

    #region Другие Преобразования Bitmap

    /*
    [StructLayout(LayoutKind.Sequential)]
    private struct PixelData
    {
      public byte B;
      public byte G;
      public byte R;
      public byte A;
    }
     * */

    internal static void SetAlphaChanelValue(Bitmap image, byte value)
    {
      if (image == null)
        throw new ArgumentNullException("image");
      if (image.PixelFormat != PixelFormat.Format32bppArgb)
        throw new ArgumentException("Wrong PixelFormat");

      BitmapData bitmapData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                   ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

      byte[] a = new byte[bitmapData.Height * bitmapData.Stride];
      Marshal.Copy(bitmapData.Scan0, a, 0, a.Length);
      for (int i = 0; i < bitmapData.Height; i++)
      {
        int off = bitmapData.Stride*i;
        for (int j = 0; j < bitmapData.Width; j++)
        {
          a[off + 3] = value;
          off += 4;
        }
      }
      Marshal.Copy(a, 0, bitmapData.Scan0, a.Length);



#if XXXX
			unsafe
			{
				PixelData* pPixel = (PixelData*)bitmapData.Scan0;
				for (int i = 0; i < bitmapData.Height; i++)
				{
					for (int j = 0; j < bitmapData.Width; j++)
					{
						pPixel->A = value;
						pPixel++;
					}
					pPixel += bitmapData.Stride - (bitmapData.Width * 4);
				}
			}
#endif
      image.UnlockBits(bitmapData);
    }

    #endregion

    #region Объекты ImageCodecInfo

    private static readonly Dictionary<Guid, ImageCodecInfo> _FormatDict =
      CreateFormatDict();

    private static Dictionary<Guid, ImageCodecInfo> CreateFormatDict()
    {
      Dictionary<Guid, ImageCodecInfo> Dict = new Dictionary<Guid, ImageCodecInfo>();
      ImageCodecInfo[] icis = ImageCodecInfo.GetImageDecoders();
      for (int i = 0; i < icis.Length; i++)
      {
        if (!Dict.ContainsKey(icis[i].FormatID))
          Dict.Add(icis[i].FormatID, icis[i]);
      }
      return Dict;
    }

    /// <summary>
    /// Получить кодек, отвечающий за выбранный формат
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    public static ImageCodecInfo GetImageCodecInfo(ImageFormat format)
    {
      ImageCodecInfo ici;
      _FormatDict.TryGetValue(format.Guid, out ici);
      return ici;
    }

    #endregion

    #region Загрузка значков из файла

#if XXXX // Пока не нужно
    // Взято из:
    // http://www.codeproject.com/Articles/29923/Assigning-an-application-s-icon-to-all-forms-in-th
    // Copyright: (C) 2008, Sergey Stoyan

    /// <summary>
    /// Used as a parameter type for ExtractIconFromFile
    /// </summary>
    public enum IconSize : uint
    {
      /// <summary>
      /// 32x32
      /// </summary>
      Large = 0x0,
      /// <summary>
      /// 16x16
      /// </summary>
      Small = 0x1
    }

    /// <summary>
    /// Extracts the specified icon from the file.
    /// </summary>
    /// <param name="lpszFile">path of the icon file</param>
    /// <param name="nIconIndex">index of the icon with the file</param>
    /// <param name="phIconLarge">32x32 icon</param>
    /// <param name="phIconSmall">16x16 icon</param>
    /// <param name="nIcons">number of icons to extract</param>
    /// <returns>number of icons within the file</returns>
    [DllImport("Shell32", CharSet = CharSet.Auto)]
    extern static int ExtractIconEx(
        [MarshalAs(UnmanagedType.LPTStr)] 
            string lpszFile,
        int nIconIndex,
        IntPtr[] phIconLarge,
        IntPtr[] phIconSmall,
        int nIcons
        );

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    extern static bool DestroyIcon(IntPtr handle);

    /// <summary>
    /// Extracts icon from the binary file like .exe, .dll, etc. (because of .NET ExtractAssociatedIcon does not get 16x16 icons)
    /// </summary>
    /// <param name="file">fiel path from where icon is to be extracted</param>
    /// <param name="size">size of the icon</param>
    /// <returns>extracted icon</returns>
    public static Icon ExtractIconFromLibrary(string file, IconSize size)
    {
      if (ExtractIconEx(file, -1, null, null, 0) < 1)
        return null;

      IntPtr[] icon_ptr = new IntPtr[1];
      if (size == IconSize.Small)
        ExtractIconEx(file, 0, null, icon_ptr, 1);
      else
        ExtractIconEx(file, 0, icon_ptr, null, 1);

      Icon unmanaged_icon = (Icon)Icon.FromHandle(icon_ptr[0]);
      Icon icon = (Icon)unmanaged_icon.Clone();
      DestroyIcon(unmanaged_icon.Handle);

      return icon;
    }
#endif

    #endregion

    #region Маленькие изображения

    /// <summary>
    /// Определяет необходимость уменьшения изображения.
    /// </summary>
    /// <param name="image">Исходное изображение</param>
    /// <param name="maxSize">Ограничение на размер изображения</param>
    /// <param name="newSize">Сюда помещается новый размер изображения, меньший или равный исходному.
    /// Пропорции сохраняются</param>
    /// <returns>true, если изображение должно быть уменьшено</returns>
    public static bool IsImageShrinkNeeded(Image image, Size maxSize, out Size newSize)
    {
      return IsImageShrinkNeeded(image.Size, maxSize, out newSize);
    }

    /// <summary>
    /// Определяет необходимость уменьшить размер изображения, чтобы вписать его в заданную область.
    /// При уменьшении сохраняются существующие пропорции.
    /// Если изображение меньше, чем <paramref name="maxSize"/>, то уменьшение не выполняется.
    /// </summary>
    /// <param name="srcImageSize">Существующий размер изображение</param>
    /// <param name="maxSize">Размер, в который нужно вписать</param>
    /// <param name="newSize">Сюда записываются уменьшенные размеры.
    /// Если уменьшение не требуется, возвращается <paramref name="srcImageSize"/></param>
    /// <returns>true, если требуется уменьшение размеров</returns>
    public static bool IsImageShrinkNeeded(Size srcImageSize, Size maxSize, out Size newSize)
    {
#if DEBUG
      if (srcImageSize.Width < 0 || srcImageSize.Height < 0)
        throw new ArgumentException("Неправильный исходный размер: " + srcImageSize.ToString(), "maxSize");
      if (maxSize.Width < 1 || maxSize.Height < 1)
        throw new ArgumentException("Неправильный максимальный размер: " + maxSize.ToString(), "maxSize");
#endif

      if (srcImageSize.Width > maxSize.Width || srcImageSize.Height > maxSize.Height)
      {
        double s1 = 1.0;
        double s2 = 1.0;
        if (srcImageSize.Width > 0) // 27.12.2020
          s1 = (double)(maxSize.Width) / (double)(srcImageSize.Width);
        if (srcImageSize.Height > 0) // 27.12.2020
          s2 = (double)(maxSize.Height) / (double)(srcImageSize.Height);
        double s = Math.Min(s1, s2);

#if DEBUG
        if (s <= 0.0 || s > 1.0)
          throw new BugException("Неправильный коэффициент масштабирования: " + s.ToString());
#endif

        newSize = new Size((int)(Math.Round(srcImageSize.Width * s)),
        (int)(Math.Round(srcImageSize.Height * s)));
        return true;
      }
      else
      {
        newSize = srcImageSize;
        return false;
      }
    }

    #endregion
  }
}
