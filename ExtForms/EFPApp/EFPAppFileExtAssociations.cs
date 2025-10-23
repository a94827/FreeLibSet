// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Shell;
using FreeLibSet.IO;
using System.Drawing;
using FreeLibSet.Logging;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Реализация свойства <see cref="EFPApp.FileExtAssociations"/>.
  /// Поддерживает буферизацию списков ассоциаций по расширению файлов.
  /// Также реализует буферизацию значков приложений.
  /// Используется в <see cref="EFPFileAssociationsCommandItemsHandler"/>.
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

    private readonly Dictionary<string, FileAssociations> _FADict;

    /// <summary>
    /// Возвращает буферизованные данные файловых ассоциаций.
    /// Не может возвращать null. Если для расширения нет зарегистрированных приложений, возвращается пустой список <see cref="FileAssociations"/>.
    /// </summary>
    /// <param name="fileExt">Расширение файла с ведущей начальной точкой, например, ".txt"</param>
    /// <returns>Файловые ассоциации</returns>
    public FileAssociations this[string fileExt]
    {
      get
      {
        FileAssociations faItems;
        if (!_FADict.TryGetValue(fileExt, out faItems))
        {
          faItems = FileAssociations.FromFileExtension(fileExt);
          _FADict.Add(fileExt, faItems);
        }
        return faItems;
      }
    }

    /// <summary>
    /// Возвращает буферизованный объект файловых ассоциаций <see cref="FileAssociations"/> для просмотра каталога.
    /// Для Windows возвращает единственную ассоциацию для запуска Windows Explorer.
    /// </summary>
    public FileAssociations ShowDirectory
    {
      get
      {
        FileAssociations faItems = _ShowDirectory;
        if (faItems == null)
        {
          faItems = FileAssociations.FromDirectory();
          _ShowDirectory = faItems;
        }
        return faItems;
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
    private readonly Dictionary<string, Image> _IconImageDict;

    /// <summary>
    /// Получить значок из ресурсов файла требуемого размера.
    /// Буферизация вызовов <see cref="WinFormsTools.ExtractIcon(AbsPath, int, bool)"/>.
    /// Если для значка нет требуемого размера, возвращается значок другого размера.
    /// Если файл не найден или в файле нет значка с заданным индексом, возвращается null.
    /// Для платформ, отличных от Windows, всегда возвращает null.
    /// </summary>
    /// <param name="filePath">Путь к файлу</param>
    /// <param name="iconIndex">Индекс значка в файле. 
    /// См. описание функции Windows ExtractIcon или ExtractIconEx()</param>
    /// <param name="smallIcon">true - извлечь маленький значок (16x16), false - большой (32x32)</param>
    /// <returns>Значок или null</returns>
    public Image GetIconImage(AbsPath filePath, int iconIndex, bool smallIcon)
    {
      string key = filePath.Path + ";" + iconIndex.ToString() + ";" + smallIcon.ToString();
      Image image;
      if (!_IconImageDict.TryGetValue(key, out image))
      {
        try
        {
          image = WinFormsTools.ExtractIconImage(filePath, iconIndex, smallIcon);
        }
        catch (Exception e)
        {
          e.Data["Path"] = filePath;
          e.Data["IconIndex"] = iconIndex;
          e.Data["SmallIcon"] = smallIcon;
          LogoutTools.LogoutException(e, Res.EFPApp_ErrTitle_ExtractIconImage);
          image = null;
        }
        _IconImageDict.Add(key, image);
      }
      return image;
    }

    /// <summary>
    /// Получить значок из ресурсов файла требуемого размера.
    /// Буферизация вызовов <see cref="WinFormsTools.ExtractIcon(AbsPath, int, bool)"/>.
    /// Если для значка нет требуемого размера, возвращается значок другого размера.
    /// Если файл не найден или в файле нет значка с заданным индексом, возвращается null.
    /// Для платформ, отличных от Windows, всегда возвращает null.
    /// </summary>
    /// <param name="fa">Файловая ассоциация. Используются свойства <see cref="FileAssociationItem.IconPath"/> и <see cref="FileAssociationItem.IconIndex"/></param>
    /// <param name="smallIcon">true - извлечь маленький значок (16x16), false - большой (32x32)</param>
    /// <returns>Значок или null</returns>
    public Image GetIconImage(FileAssociationItem fa, bool smallIcon)
    {
      if (fa == null)
        return null;
      else if (fa.IconPath.IsEmpty)
        return null;
      else
        return GetIconImage(fa.IconPath, fa.IconIndex, smallIcon);
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

      foreach (Image image in _IconImageDict.Values)
      {
        try
        {
          image.Dispose();
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
