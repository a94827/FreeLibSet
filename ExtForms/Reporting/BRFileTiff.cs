using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using BitMiracle.LibTiff.Classic;
using FreeLibSet.Core;
using FreeLibSet.IO;
using FreeLibSet.Reporting;

namespace FreeLibSet.Drawing.Reporting
{
  /// <summary>
  /// Создание файлов TIFF
  /// </summary>
  public class BRFileTiff : BRFileCreator
  {
    #region Управляющие свойства

    /// <summary>
    /// Разрешение и другие параметры графики для TIFF-файла
    /// </summary>
    public BRBitmapSettingsDataItem BitmapSettings
    {
      get
      {
        if (_BitmapSettings == null)
          _BitmapSettings = new BRBitmapSettingsDataItem();
        return _BitmapSettings;
      }
      set { _BitmapSettings = value; }
    }

    private BRBitmapSettingsDataItem _BitmapSettings;

    #endregion

    #region Создание файла

    /// <summary>
    /// Создает файл
    /// </summary>
    /// <param name="report"></param>
    /// <param name="filePath"></param>
    protected override void DoCreateFile(BRReport report, AbsPath filePath)
    {
      bool useLibTiff = EnvironmentTools.IsMono && (!EnvironmentTools.IsWine);
      //useLibTiff = true;

      using (BRReportPainter painter = new BRReportPainter())
      {
        if (EnvironmentTools.IsMono)
          painter.FontHeightScale = 2.5f; //72f / 25.4f; // 08.12.2023
        
        BRPaginator paginator = new BRPaginator(painter);
        BRPaginatorPageInfo[] pages = paginator.CreatePages(report);

        if (useLibTiff)
        {
          // Создаем с помощью LibTiff
          LibTiffTools.DoCreateFile(report, filePath, painter, pages, this.Splash, this.BitmapSettings);
        }
        else
        {
          Bitmap firstBmp = null;
          System.Drawing.Imaging.Encoder encSaveFlag = System.Drawing.Imaging.Encoder.SaveFlag;
          EncoderParameters pars = new EncoderParameters(1);
          ImageCodecInfo ici = ImagingTools.GetImageCodecInfo(ImageFormat.Tiff);
          if (ici == null)
            throw new InvalidOperationException("В GDI+ не установлен кодировщик для записи файлов TIFF");

          Splash.PercentMax = pages.Length * 2 + 1;
          Splash.AllowCancel = true;

          for (int i = 0; i < pages.Length; i++)
          {
            Bitmap bmp = painter.CreateBitmap(pages[i], BitmapSettings);
            Splash.IncPercent();

            try
            {
              SaveEXIFMetadata(bmp, report);
            }
            catch { }

            if (i == 0)
            {
              // Первая страница
              firstBmp = bmp;
              EncoderParameter encPar = new EncoderParameter(encSaveFlag, (long)EncoderValue.MultiFrame);
              pars.Param[0] = encPar;
              firstBmp.Save(filePath.Path, ici, pars);
            }
            else
            {
              EncoderParameter encPar = new EncoderParameter(encSaveFlag, (long)EncoderValue.FrameDimensionPage);
              pars.Param[0] = encPar;
              firstBmp.SaveAdd(bmp, pars);
              bmp.Dispose();
            }

            Splash.IncPercent();
          }
          Splash.AllowCancel = false;

          if (firstBmp != null)
          {
            // Закрываем многостраничный TIFF
            EncoderParameter encPar = new EncoderParameter(encSaveFlag, (long)EncoderValue.Flush);
            pars.Param[0] = encPar;
            firstBmp.SaveAdd(pars);

            firstBmp.Dispose();
          }
          Splash.IncPercent();
        }
        Splash.PercentMax = 0;
      }
    }


    private static void SaveEXIFMetadata(Image img, BRReport report)
    {
      #region Эти константы приведены в справке для свойства PropertyItem.Id

      //const int PropertyTagDocumentName = 0x010D;
      const int PropertyTagImageDescription = 0x010E;
      //const int PropertyTagImageTitle = 0x0320;
      const int PropertyTagArtist = 0x013B;
      //const int PropertyTagExifUserComment = 0x9286;

      if (!String.IsNullOrEmpty(report.DocumentProperties.Title))
        SaveEXIFMetadataProperty(img, PropertyTagImageDescription, report.DocumentProperties.Title, Encoding.UTF8); // дублируется и в название, и в тему
      //if (!String.IsNullOrEmpty(TheDoc.Subject))
      //  SaveEXIFMetadataProperty(img, PropertyTagExifUserComment, TheDoc.Subject); не работает
      if (!String.IsNullOrEmpty(report.DocumentProperties.Author))
        SaveEXIFMetadataProperty(img, PropertyTagArtist, report.DocumentProperties.Author, Encoding.UTF8);

      #endregion

      #region Эти константы не документированы

      // Выяснено опытным путем
      // Свойства записываются в формате Unicode, а не UTF-8

      const int PropertyUnknownTitle = 40091;
      const int PropertyUnknownAuthor = 40093;
      const int PropertyUnknownSubject = 40095;

      if (!String.IsNullOrEmpty(report.DocumentProperties.Title))
        SaveEXIFMetadataProperty(img, PropertyUnknownTitle, report.DocumentProperties.Title, Encoding.Unicode);
      if (!String.IsNullOrEmpty(report.DocumentProperties.Author))
        SaveEXIFMetadataProperty(img, PropertyUnknownAuthor, report.DocumentProperties.Author, Encoding.Unicode);
      if (!String.IsNullOrEmpty(report.DocumentProperties.Subject))
        SaveEXIFMetadataProperty(img, PropertyUnknownSubject, report.DocumentProperties.Subject, Encoding.Unicode);

      #endregion
    }

    private static void SaveEXIFMetadataProperty(Image img, int propertyId, string value, Encoding encoding)
    {
      PropertyItem Item = CreatePropertyItem();

      Item.Id = (int)propertyId;
      // Type=1 means Array of Bytes. 
      Item.Type = 2; // строка ASCII
      byte[] b = encoding.GetBytes(value + "\0");
      Item.Len = b.Length;
      Item.Value = b;
      img.SetPropertyItem(Item);
      // img.Save(filepath);
    }


    private static PropertyItem CreatePropertyItem()
    {
      System.Reflection.ConstructorInfo ci = typeof(PropertyItem).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public,
        null, new Type[] { }, null);

      return (PropertyItem)ci.Invoke(null);
    }

    #endregion

    #region Возможность использования

    /// <summary>
    /// Возвращает false в Mono без Wine, так как не поддерживается многостраничный TIFF
    /// </summary>
    public static bool IsSupported
    {
      get
      {
        if (EnvironmentTools.IsMono && (!EnvironmentTools.IsWine))
          //return false;
          return GetLibTiffAvailable();

        return true;
      }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool GetLibTiffAvailable()
    {
      return LibTiffTools.LibTiffAvailable;
    }

    #endregion

    #region Библиотека LibTiff.Net

    private static class LibTiffTools
    {
      #region Наличие библиотеки LibTiff.Net

      private static readonly object SyncRoot = new object();

      /// <summary>
      /// Возвращает true, если библиотека LibTiff.Net загружена
      /// </summary>
      internal static bool LibTiffAvailable
      {
        [DebuggerStepThrough] // подавление остановки в отладчике при возникновении исключения
        get
        {
          lock (SyncRoot)
          {
            if (!_LibTiffAvailable.HasValue)
            {
              try
              {
                TryTestLibTiffFile();
                _LibTiffAvailable = true;
              }
              catch
              {
                _LibTiffAvailable = false;
              }
            }
          }

          return _LibTiffAvailable.Value;
        }
      }
      private static bool? _LibTiffAvailable = null;

      /// <summary>
      /// Это должно быть в отдельном методе, т.к. оно может не запускаться
      /// </summary>
      [DebuggerStepThrough]
      private static void TryTestLibTiffFile()
      {
        Type dummy = typeof(BitMiracle.LibTiff.Classic.Tiff);
      }

      ///// <summary>
      ///// Выбрасывает исключение, если <see cref="LibTiffAvailable"/>=false.
      ///// </summary>
      //public static void CheckLibTiffAvailable()
      //{
      //  if (!LibTiffAvailable)
      //    throw new DllNotFoundException("Не удалось загрузить библиотеку BitMiracle.LibTiff.dll. Без нее невозможно создание многостраничного tiff-файла");
      //}

      #endregion

      #region Создание файла

      internal static void DoCreateFile(BRReport report, AbsPath filePath, BRReportPainter painter, BRPaginatorPageInfo[] pages, ISimpleSplash splash, BRBitmapSettingsDataItem bitmapSettings)
      {
        using (Tiff output = Tiff.Open(filePath.Path, "w"))
        {
          SaveEXIFMetadata(output, report);

          splash.PercentMax = pages.Length * 3;
          splash.AllowCancel = true;

          for (int i = 0; i < pages.Length; i++)
          {
            Bitmap bmp = painter.CreateBitmap(pages[i], bitmapSettings);
            splash.IncPercent();

            byte[] raster = ImagingTools.GetImageRasterBytes(bmp);
            int stride = raster.Length / bmp.Height;

            output.SetField(TiffTag.IMAGEWIDTH, bmp.Width);
            output.SetField(TiffTag.IMAGELENGTH, bmp.Height);
            switch (bmp.PixelFormat)
            {
              //case PixelFormat.Format32bppRgb:
              //  output.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB);
              //  output.SetField(TiffTag.SAMPLESPERPIXEL, 4);
              //  output.SetField(TiffTag.BITSPERSAMPLE, 8);
              //  break;
              case PixelFormat.Format8bppIndexed:
                output.SetField(TiffTag.PHOTOMETRIC, Photometric.PALETTE);
                output.SetField(TiffTag.SAMPLESPERPIXEL, 1);
                output.SetField(TiffTag.BITSPERSAMPLE, 8);
                output.SetField(TiffTag.ROWSPERSTRIP, output.DefaultStripSize(0));

                InitColorMap(output, bmp);
                break;
              case PixelFormat.Format4bppIndexed:
                output.SetField(TiffTag.PHOTOMETRIC, Photometric.PALETTE);
                //output.SetField(TiffTag.SAMPLESPERPIXEL, 2);
                output.SetField(TiffTag.BITSPERSAMPLE, 4);
                output.SetField(TiffTag.ROWSPERSTRIP, output.DefaultStripSize(0));

                InitColorMap(output, bmp);
                break;
              case PixelFormat.Format1bppIndexed:
                output.SetField(TiffTag.PHOTOMETRIC, Photometric.PALETTE);
                //output.SetField(TiffTag.SAMPLESPERPIXEL, 2);
                output.SetField(TiffTag.BITSPERSAMPLE, 1);
                output.SetField(TiffTag.ROWSPERSTRIP, output.DefaultStripSize(0));

                InitColorMap(output, bmp);
                break;
              case PixelFormat.Format24bppRgb:
                output.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB);
                output.SetField(TiffTag.SAMPLESPERPIXEL, 3);
                output.SetField(TiffTag.BITSPERSAMPLE, 8);
                CorrectRaster24bpp(raster, bmp.Width, bmp.Height);
                break;
              default:
                throw new NotImplementedException();
            }
            output.SetField(TiffTag.ORIENTATION, BitMiracle.LibTiff.Classic.Orientation.TOPLEFT);
            output.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
            output.SetField(TiffTag.XRESOLUTION, bmp.HorizontalResolution);
            output.SetField(TiffTag.YRESOLUTION, bmp.VerticalResolution);
            output.SetField(TiffTag.RESOLUTIONUNIT, ResUnit.INCH);
            output.SetField(TiffTag.SUBFILETYPE, FileType.PAGE);
            output.SetField(TiffTag.PAGENUMBER, i + 1, 10);
            output.SetField(TiffTag.COMPRESSION, Compression.LZW);

            int offset = 0;
            for (int j = 0; j < bmp.Height; j++)
            {
              output.WriteScanline(raster, offset, j, 0);
              offset += stride;
            }

            bmp.Dispose();

            splash.IncPercent();

            output.WriteDirectory();
            splash.IncPercent();
          }
          splash.AllowCancel = false;
          splash.PercentMax = 0;
        }
      }

      private static void SaveEXIFMetadata(Tiff output, BRReport report)
      {
        #region Эти константы приведены в справке для свойства PropertyItem.Id

        if (!String.IsNullOrEmpty(report.DocumentProperties.Title))
          SaveEXIFMetadataProperty(output, TiffTag.IMAGEDESCRIPTION, report.DocumentProperties.Title, Encoding.UTF8);
        if (!String.IsNullOrEmpty(report.DocumentProperties.Author))
          SaveEXIFMetadataProperty(output, TiffTag.ARTIST, report.DocumentProperties.Author, Encoding.UTF8);

        #endregion

        #region Эти константы не документированы

        // Выяснено опытным путем
        // Свойства записываются в формате Unicode, а не UTF-8

        const TiffTag PropertyUnknownTitle = (TiffTag)40091;
        const TiffTag PropertyUnknownAuthor = (TiffTag)40093;
        const TiffTag PropertyUnknownSubject = (TiffTag)40095;

        if (!String.IsNullOrEmpty(report.DocumentProperties.Title))
          SaveEXIFMetadataProperty(output, PropertyUnknownTitle, report.DocumentProperties.Title, Encoding.Unicode);
        if (!String.IsNullOrEmpty(report.DocumentProperties.Author))
          SaveEXIFMetadataProperty(output, PropertyUnknownAuthor, report.DocumentProperties.Author, Encoding.Unicode);
        if (!String.IsNullOrEmpty(report.DocumentProperties.Subject))
          SaveEXIFMetadataProperty(output, PropertyUnknownSubject, report.DocumentProperties.Subject, Encoding.Unicode);

        #endregion
      }

      private static void SaveEXIFMetadataProperty(Tiff output, TiffTag tag, string value, Encoding encoding)
      {
        //output.SetField(tag, value);

        //Item.Id = (int)tag;
        //// Type=1 means Array of Bytes. 
        //Item.Type = 2; // строка ASCII
        byte[] b = encoding.GetBytes(value + "\0");
        //Item.Len = b.Length;
        //Item.Value = b;
        //img.SetPropertyItem(Item);
        //// img.Save(filepath);
        output.SetField(tag, b);
      }


      /// <summary>
      /// Инициализация атрибута <see cref="TiffTag.COLORMAP"/> из палитры <see cref="Image.Palette"/>.
      /// </summary>
      /// <param name="output"></param>
      /// <param name="bmp"></param>
      private static void InitColorMap(Tiff output, Bitmap bmp)
      {
        int n = bmp.Palette.Entries.Length;
        ushort[] r = new ushort[n];
        ushort[] g = new ushort[n];
        ushort[] b = new ushort[n];
        for (int i = 0; i < n; i++)
        {
          Color clr = bmp.Palette.Entries[i];
          r[i] = clr.R;
          g[i] = clr.G;
          b[i] = clr.B;
        }

        output.SetField(TiffTag.COLORMAP, r, g, b);
      }

      /// <summary>
      /// Корректирует порядок байт RGB-BGR для формата <see cref="PixelFormat.Format24bppRgb"/>
      /// </summary>
      /// <param name="raster">Данные растра</param>
      /// <param name="width">Ширина изображения</param>
      /// <param name="height">Высота изображения</param>
      private static void CorrectRaster24bpp(byte[] raster, int width, int height)
      {
        const int samplesPerPixel = 3;
        int stride = raster.Length / height;

        for (int i = 0; i < height; i++)
        {
          int offset = i * stride;
          int strideEnd = offset + width * samplesPerPixel;
          for (int j = offset; j < strideEnd; j += samplesPerPixel)
          {
            byte tmp = raster[j];
            raster[j] = raster[j + 2];
            raster[j + 2] = tmp;
          }
        }
      }

      #endregion
    }
  }

  #endregion
}
