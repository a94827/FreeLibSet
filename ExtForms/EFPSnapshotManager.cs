using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using FreeLibSet.Config;

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

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Интефрейс сохранения изображений предварительного просмотра композиций интерфейса.
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
      Bitmap Bmp = null;
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
              Bmp = Image.FromStream(ms) as Bitmap;
            }
          }
          catch //(Exception e)
          {
          }
        }
      }
      return Bmp;
    }

    #endregion
  }
}
