// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using FreeLibSet.Config;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Интерфейс сохранения изображений предварительного просмотра композиций интерфейса.
  /// Используется свойством EFPApp.SnapshotManager
  /// </summary>
  public interface IEFPSnapshotManager
  {
    /// <summary>
    /// Сохраняет изображение
    /// </summary>
    /// <param name="configInfo">Идентификация изображения</param>
    /// <param name="snapshot">Сохраняемое изображение. Может быть null</param>
    void SaveSnapshot(EFPConfigSectionInfo configInfo, Bitmap snapshot);

    /// <summary>
    /// Загружает изображение.
    /// Если изображение не было сохранено, метод должен вернуть null
    /// </summary>
    /// <param name="configInfo">Идентификация изображения</param>
    /// <returns>Изображение или null</returns>
    Bitmap LoadSnapshot(EFPConfigSectionInfo configInfo);
  }

  /// <summary>
  /// Реализация интерфейса сохранения изображений, используемая по умолчанию.
  /// Хранит изображение в секции конфигурации в виде текстовой строки в кодировке Base64
  /// </summary>
  public class EFPSnapshotConfigManager : IEFPSnapshotManager
  {
    #region Конструктор

    /// <summary>
    /// Создает менеджер
    /// </summary>
    public EFPSnapshotConfigManager()
    {
      _ImageFormat = System.Drawing.Imaging.ImageFormat.Gif;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Формат сжатия изображения перед преобразованием в Base64.
    /// По умолчанию - GIF
    /// </summary>
    public System.Drawing.Imaging.ImageFormat ImageFormat
    {
      get { return _ImageFormat; }
      set { _ImageFormat = value; }
    }
    private System.Drawing.Imaging.ImageFormat _ImageFormat;

    #endregion

    #region IEFPSnapshotManager Members

    /// <summary>
    /// Сохраняет изображение.
    /// Непереопределенный метод получает секцию конфигурации вызовом EFPApp.ConfigManager.GetConfig().
    /// Изображение преобразуется в строку в кодировке Base64.
    /// Строка записывается в секцию конфигурации как строковое значение "Snapshot".
    /// Если <paramref name="snapshot"/>=null, то значение "Snapshot" удаляется из секции конфигурации.
    /// </summary>
    /// <param name="configInfo">Имя и категория секции конфигурации</param>
    /// <param name="snapshot">Изображение. Может быть null</param>
    public virtual void SaveSnapshot(EFPConfigSectionInfo configInfo, Bitmap snapshot)
    {
      CfgPart cfg;
      using (EFPApp.ConfigManager.GetConfig(configInfo, EFPConfigMode.Write, out cfg))
      {
        if (snapshot == null)
          cfg.Remove("Snapshot");
        else
        {
          using (System.IO.MemoryStream strm = new System.IO.MemoryStream())
          {
            snapshot.Save(strm, System.Drawing.Imaging.ImageFormat.Gif);
            //string s = Convert.ToBase64String(strm.GetBuffer());
            string s = Convert.ToBase64String(strm.ToArray()); // 20.11.2020
            cfg.SetString("Snapshot", s);
          }
        }
      }
    }

    /// <summary>
    /// Загружает изображение.
    /// Непереопределенный метод получает секцию конфигурации вызовом EFPApp.ConfigManager.GetConfig().
    /// Из секции читается строковое значение "Snapshot".
    /// Если значения нет или оно является пустой строкой, возвращается null.
    /// Иначе строка преобразуется в изображение, предполагая, что оно хранится в кодировке Base64.
    /// В случае ошибки преобразования возвращается null без выброса исключения.
    /// </summary>
    /// <param name="configInfo">Имя и категория секции конфигурации</param>
    /// <returns>Загруженное изображение или null</returns>
    public virtual Bitmap LoadSnapshot(EFPConfigSectionInfo configInfo)
    {
      Bitmap bmp = null;
      CfgPart cfg;
      using (EFPApp.ConfigManager.GetConfig(configInfo, EFPConfigMode.Read, out cfg))
      {
        string sSnapshot = cfg.GetString("Snapshot");
        if (!String.IsNullOrEmpty(sSnapshot))
        {
          try
          {
            byte[] b = Convert.FromBase64String(sSnapshot);
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(b))
            {
              bmp = Image.FromStream(ms) as Bitmap;
            }
          }
          catch //(Exception e)
          {
          }
        }
      }
      return bmp;
    }

    #endregion
  }
}
