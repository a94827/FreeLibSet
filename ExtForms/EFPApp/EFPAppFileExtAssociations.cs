using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Shell;
using FreeLibSet.IO;
using System.Drawing;
using FreeLibSet.Logging;

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
  /// Реализация свойства EFPApp.FileExtAssociations.
  /// Поддерживает буферизацию списков ассоциаций по расширению файлов.
  /// Также реализует буферизацию значков приложений.
  /// Используется EFPFileAssociationsCommandItemsHandler.
  /// </summary>
  public sealed class EFPAppFileExtAssociations : IDisposable
  {
    #region Конструктор и Dispose()

    internal EFPAppFileExtAssociations()
    {
      _FADict = new Dictionary<string, FileAssociations>();
      _IconImageDict = new Dictionary<string, Image>();
    }

    /// <summary>
    /// Упрощенная реализация без деструктора
    /// </summary>
    void IDisposable.Dispose()
    {
      Reset();
    }

    #endregion

    #region Доступ к ассоциациям файлов

    /// <summary>
    /// Возвращает true, если извлечение ассоциаций реализовано для операционной системы
    /// </summary>
    public bool IsSupported { get { return FileAssociations.IsSupported; } }

    // Неохота использовать систему Cache, т.к. обычно будет немного расширений файлов:
    // только ".txt", ".html" и ".xml"

    private Dictionary<string, FileAssociations> _FADict;

    /// <summary>
    /// Возвращает буферизованные данные файловых ассоциаций.
    /// Не может возвращать null
    /// </summary>
    /// <param name="fileExt">Расширение файла, например, ".txt"</param>
    /// <returns>Файловые ассоциации</returns>
    public FileAssociations this[string fileExt]
    {
      get
      {
        FileAssociations FAItems;
        if (!_FADict.TryGetValue(fileExt, out FAItems))
        {
          FAItems = FileAssociations.FromFileExtension(fileExt);
          _FADict.Add(fileExt, FAItems);
        }
        return FAItems;
      }
    }

    /// <summary>
    /// Возвращает буферизованный объект файловых ассоциация для просмотра каталога.
    /// Для Windows возвращает единственную ассоциацию для запуска Windows Explorer.
    /// </summary>
    public FileAssociations ShowDirectory
    {
      get
      {
        FileAssociations FA = _ShowDirectory;
        if (FA == null)
        {
          FA = FileAssociations.FromDirectory();
          _ShowDirectory = FA;
        }
        return FA;
      }
    }
    private FileAssociations _ShowDirectory;

    #endregion

    #region Доступ к значкам

    //private struct FileIconInfo
    //{
    //  #region Конструктор

    //  public FileIconInfo(AbsPath IconPath, int IconIndex)
    //  {
    //    FIconPath = IconPath;
    //    FIconIndex = IconIndex;
    //  }

    //  #endregion
    //}

    /// <summary>
    /// Ключ - путь к файлу плюс индекс значка плюс true/false
    /// Значение - значок
    /// </summary>
    private Dictionary<string, Image> _IconImageDict;

    /// <summary>
    /// Получить значок из ресурсов файла требуемого размера.
    /// Буферизация вызовов WinFormsTools.ExtractIcon()
    /// Если для значка нет требуемого размера, возвращается значок другого размера.
    /// Если файл не найден или в файле нет значка с заданным индексом, возвращается null.
    /// Для платформ, отличных от Windows, всегда возвращает null.
    /// </summary>
    /// <param name="filePath">Путь к файлу</param>
    /// <param name="iconIndex">Индекс значка в файле. 
    /// См. описание функции Windows ExtractIcon или ExtractIconEx()</param>
    /// <param name="smallIcon">true - извлечь маленький значок (16x16), false - больщой (32x32)</param>
    /// <returns>Значок или null</returns>
    public Image GetIconImage(AbsPath filePath, int iconIndex, bool smallIcon)
    {
      string Key = filePath.Path + ";" + iconIndex.ToString() + ";" + smallIcon.ToString();
      Image Image;
      if (!_IconImageDict.TryGetValue(Key, out Image))
      {
        try
        {
          Image = WinFormsTools.ExtractIconImage(filePath, iconIndex, smallIcon);
        }
        catch (Exception e)
        {
          e.Data["Path"] = filePath;
          e.Data["IconIndex"] = iconIndex;
          e.Data["SmallIcon"] = smallIcon;
          LogoutTools.LogoutException(e, "Ошибка извлечения значка из файла");
          Image = null;
        }
        _IconImageDict.Add(Key, Image);
      }
      return Image;
    }

    #endregion

    #region Сброс буферизации

    /// <summary>
    /// Сброс буферизации
    /// </summary>
    public void Reset()
    {
      _FADict.Clear();
      _ShowDirectory = null;

      foreach (Image Image in _IconImageDict.Values)
      {
        try
        {
          Image.Dispose();
        }
        catch { }
      }
      _IconImageDict.Clear();
    }

    /// <summary>
    /// Пока не знаю, как отслеживать изменения (с помощью SHChangeNotifyRegister ?)
    /// Класс SystemEvents тут не помогает.
    /// Пока тупо сбрасываем ассоциации по таймеру. Буферизация будет работать, только если в одном
    /// окне есть несколько управляющих элементов, использующих EFPFileAssociationsCommandItemsHandler.
    /// </summary>
    internal void ResetFA()
    {
      _FADict.Clear();
    }

    #endregion
  }
}
