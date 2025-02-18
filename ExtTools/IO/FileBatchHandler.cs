using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FreeLibSet.Collections;
using FreeLibSet.Core;

namespace FreeLibSet.IO
{
  /// <summary>
  /// Элемент для добавления в список <see cref="FileBatchHandler"/>.
  /// Задает один каталог, файл или маску файлов для копирования/перемещения.
  /// </summary>
  public struct FileBatchItem
  {
    #region Конструктор

    /// <summary>
    /// Инициализация структуры
    /// </summary>
    /// <param name="srcPath">Путь к существующему файлу или каталогу</param>
    /// <param name="destPath">Путь к конечному файлу или каталогу</param>
    /// <param name="template">Маска файлов. Может использоваться, если <paramref name="srcPath"/> задает каталог</param>
    /// <param name="recurse">Если true, то будет выполняться рекурсивный поиск по маске <paramref name="template"/></param>
    public FileBatchItem(AbsPath srcPath, AbsPath destPath, string template, bool recurse)
    {
      if (srcPath.IsEmpty)
        throw ExceptionFactory.ArgIsEmpty("srcPath");
      if (destPath.IsEmpty)
        throw ExceptionFactory.ArgIsEmpty("destPath");
      if (destPath == srcPath)
        throw ExceptionFactory.ArgAreSame("srcPath", "destPath");

      _SrcPath = srcPath;
      _DestPath = destPath;
      _Template = template;
      _Recurse = recurse;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Путь к существующему файлу или каталогу
    /// </summary>
    public AbsPath SrcPath { get { return _SrcPath; } }
    private readonly AbsPath _SrcPath;

    /// <summary>
    /// Путь к конечному файлу или каталогу
    /// </summary>
    public AbsPath DestPath { get { return _DestPath; } }
    private readonly AbsPath _DestPath;

    /// <summary>
    /// Маска файлов
    /// </summary>
    public string Template { get { return _Template; } }
    private readonly string _Template;

    /// <summary>
    /// Если true, то будет выполняться рекурсивный поиск по маске <see cref="Template"/>.
    /// </summary>
    public bool Recurse { get { return _Recurse; } }
    private readonly bool _Recurse;

    #endregion
  }

  /// <summary>
  /// Режим работы <see cref="FileBatchHandler"/> - копирование или перемещение
  /// </summary>
  public enum FileBatchMode
  {
    /// <summary>
    /// Копирование файлов / каталогов
    /// </summary>
    Copy,

    /// <summary>
    /// Переименование или перемещение файлов / каталогов
    /// </summary>
    Move
  }

  /// <summary>
  /// Групповое копирование или перемещение файлов и каталогов
  /// </summary>
  public class FileBatchHandler : ListWithReadOnly<FileBatchItem>
  {
    #region Конструктор

    /// <summary>
    /// Инициализация объектов
    /// </summary>
    public FileBatchHandler()
    {
      _Mode = FileBatchMode.Copy;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Режим работы - копирование или перемещение
    /// </summary>
    public FileBatchMode Mode
    {
      get { return _Mode; }
      set
      {
        CheckNotReadOnly();
        _Mode = value;
      }
    }
    private FileBatchMode _Mode;

    /// <summary>
    /// Подключаемая экранная заставка.
    /// Если свойство установлено, то в заставке будет отображаться процентный индикатор и текущее выполняемое действие.
    /// Также можно прервать выполнение. Для этого следует установить свойство <see cref="ISimpleSplash.AllowCancel"/>=true.
    /// </summary>
    public ISimpleSplash Splash { get { return _Splash; } set { _Splash = value; } }
    private ISimpleSplash _Splash;

    /// <summary>
    /// Отладочное свойство.
    /// Если установлено в true, то операции перемещения не будут использоваться даже в пределах одного диска.
    /// Вместо этого будет выполняться копирование с последующим удалением.
    /// Используется при <see cref="Mode"/>=<see cref="FileBatchMode.Move"/>.
    /// </summary>
    public bool MoveAsCopyAndDelete
    {
      get { return _MoveAsCopyAndDelete; }
      set
      {
        CheckNotReadOnly();
        _MoveAsCopyAndDelete = value;
      }
    }
    private bool _MoveAsCopyAndDelete;

    #endregion

    #region Дополнительные методы добавления

    /// <summary>
    /// Добавляет в список один файл или каталог для копирования/переноса.
    /// При копировании каталога выполняется рекурсивное копирование всех файлов и вложенных подкаталогов.
    /// </summary>
    /// <param name="srcPath">Путь к исходному файлу или каталогу</param>
    /// <param name="destPath">Путь к конечному файлу или каталогу</param>
    public void Add(AbsPath srcPath, AbsPath destPath)
    {
      base.Add(new FileBatchItem(srcPath, destPath, String.Empty, true));
    }

    /// <summary>
    /// Добавляет в список группу файлов по заданной маске
    /// </summary>
    /// <param name="srcPath">Путь к исходному каталогу</param>
    /// <param name="destPath">Путь к конечному каталогу</param>
    /// <param name="template">Маска отбора файлов. Пустая строка и "*" означает "все файлы".
    /// Отличие между "" и "*" проявляется при переносе. Если пустая строка, то при <paramref name="recurse"/>=true,
    /// исходный каталог удаляется.</param>
    /// <param name="recurse">Если true, то будет выполнен рекурсивный поиск</param>
    public void Add(AbsPath srcPath, AbsPath destPath, string template, bool recurse)
    {
      base.Add(new FileBatchItem(srcPath, destPath, template, recurse));
    }

    #endregion

    #region Журнал действий

    private enum FileAction
    {
      CopyFile,

      MoveFile,

      MoveDir,

      CreateDir,

      RemoveDir
    }

    private struct LogInfo
    {
      #region Конструктор

      public LogInfo(FileAction action, AbsPath srcPath, AbsPath destPath)
      {
        _Action = action;
        _SrcPath = srcPath;
        _DestPath = destPath;
      }

      #endregion

      #region Свойства

      public FileAction Action { get { return _Action; } }
      private readonly FileAction _Action;

      public AbsPath SrcPath { get { return _SrcPath; } }
      private readonly AbsPath _SrcPath;

      public AbsPath DestPath { get { return _DestPath; } }
      private readonly AbsPath _DestPath;

      public override string ToString()
      {
        StringBuilder sb = new StringBuilder();
        sb.Append(_Action.ToString());
        sb.Append(" ");
        if (!_SrcPath.IsEmpty)
        {
          sb.Append(_SrcPath.Path);
          sb.Append(" -> ");
        }
        sb.Append(_DestPath.Path);

        return sb.ToString();
      }

      #endregion
    }

    #endregion

    #region Основной метод

    /// <summary>
    /// Выполнение копирования / перемещения.
    /// Для программ с пользовательским интерфейсом рекомендуется использовать рабочий, а не основной поток.
    /// </summary>
    public void Process()
    {
      SetReadOnly();

      ISimpleSplash spl = Splash ?? new DummySimpleSplash();

      List<LogInfo> logList = new List<LogInfo>();

      if (Mode == FileBatchMode.Move)
        _NonMovableDirs = new SingleScopeList<string>();

      foreach (FileBatchItem item in this)
      {
        switch (Mode)
        {
          case FileBatchMode.Copy:
            ProcessCopyItem(item, logList, spl);
            break;
          case FileBatchMode.Move:
            ProcessMoveItem(item, logList, spl);
            break;
#if DEBUG
          default:
            throw new BugException("Unknown mode");
#endif
        }
      }

      _NonMovableDirs = null;
    }

    private void ProcessCopyItem(FileBatchItem item, List<LogInfo> logList, ISimpleSplash spl)
    {
      if (String.IsNullOrEmpty(item.Template))
      {
        if (File.Exists(item.SrcPath.Path))
        {
          ForceDirs(item.DestPath.ParentDir, logList, spl);
          spl.PhaseText = String.Format(Res.FileBatchHandler_Phase_CopyFile, item.DestPath);
          File.Copy(item.SrcPath.Path, item.DestPath.Path, true);
          logList.Add(new LogInfo(FileAction.CopyFile, item.SrcPath, item.DestPath));
        }
        else if (Directory.Exists(item.SrcPath.Path))
        {
          DoCopyDirectory(item.SrcPath, item.DestPath, logList, spl);
        }
        else
          throw ExceptionFactory.FileNotFound(item.SrcPath);
      }
      else
      {
        if (!Directory.Exists(item.SrcPath.Path))
          throw ExceptionFactory.DirectoryNotFound(item.SrcPath);
        DoCopyTemplateFiles(item.SrcPath, item.DestPath, item.Template, item.Recurse, logList, spl);
      }
    }

    private void ForceDirs(AbsPath dir, List<LogInfo> logList, ISimpleSplash spl)
    {
      AbsPath[] a;
      FileTools.ForceDirs(dir, spl, out a);
      if (logList != null)
      {
        for (int i = 0; i < a.Length; i++)
          logList.Add(new LogInfo(FileAction.CreateDir, AbsPath.Empty, a[i]));
      }
    }

    private void DoCopyDirectory(AbsPath srcPath, AbsPath destPath, List<LogInfo> logList, ISimpleSplash spl)
    {
      ForceDirs(destPath, logList, spl);
      string[] aFiles = Directory.GetFiles(srcPath.Path);
      foreach (string file in aFiles)
      {
        AbsPath srcFile = new AbsPath(file);
        AbsPath destFile = new AbsPath(destPath, srcFile.FileName);
        spl.PhaseText = String.Format(Res.FileBatchHandler_Phase_CopyFile, destFile.Path);
        File.Copy(srcFile.Path, destFile.Path, true);
        if (logList!=null)
          logList.Add(new LogInfo(FileAction.CopyFile, srcFile, destFile));
      }

      string[] aDirs = Directory.GetDirectories(srcPath.Path);
      foreach (string dir in aDirs)
      {
        AbsPath srcDir = new AbsPath(dir);
        AbsPath destDir = new AbsPath(destPath, srcDir.FileName);
        DoCopyDirectory(srcDir, destDir, logList, spl); // рекурсия
      }
    }

    private void DoCopyTemplateFiles(AbsPath srcPath, AbsPath destPath, string template, bool recurse, List<LogInfo> logList, ISimpleSplash spl)
    {
      string[] aFiles = Directory.GetFiles(srcPath.Path, template);
      if (aFiles.Length > 0)
      {
        ForceDirs(destPath, logList, spl); // создаем каталог только при наличии файлов, в отличие от копирования каталога без шаблона файлов
        foreach (string file in aFiles)
        {
          AbsPath srcFile = new AbsPath(file);
          AbsPath destFile = new AbsPath(destPath, srcFile.FileName);
          spl.PhaseText = String.Format(Res.FileBatchHandler_Phase_CopyFile, destFile.Path);
          File.Copy(srcFile.Path, destFile.Path, true);
          logList.Add(new LogInfo(FileAction.CopyFile, srcFile, destFile));
        }
      }

      if (recurse)
      {
        string[] aDirs = Directory.GetDirectories(srcPath.Path);
        foreach (string dir in aDirs)
        {
          AbsPath srcDir = new AbsPath(dir);
          AbsPath destDir = new AbsPath(destPath, srcDir.FileName);
          DoCopyTemplateFiles(srcDir, destDir, template, recurse, logList, spl); // рекурсия
        }
      }
    }

    private void ProcessMoveItem(FileBatchItem item, List<LogInfo> logList, ISimpleSplash spl)
    {
      if (String.IsNullOrEmpty(item.Template))
      {
        if (File.Exists(item.SrcPath.Path))
        {
          ForceDirs(item.DestPath.ParentDir, logList, spl);
          DoMoveFile(item.SrcPath, item.DestPath, logList, spl);
        }
        else if (Directory.Exists(item.SrcPath.Path))
        {
          DoMoveDirectory(item.SrcPath, item.DestPath, logList, spl);
        }
        else
          throw ExceptionFactory.FileNotFound(item.SrcPath);
      }
      else
      {
        if (!Directory.Exists(item.SrcPath.Path))
          throw ExceptionFactory.DirectoryNotFound(item.SrcPath);
        DoMoveTemplateFiles(item.SrcPath, item.DestPath, item.Template, item.Recurse, logList, spl);
      }
    }

    private SingleScopeList<string> _NonMovableDirs;

    private void DoMoveFile(AbsPath srcPath, AbsPath destPath, List<LogInfo> logList, ISimpleSplash spl)
    {
      // сначала пытаемся переместить
      string dirKey = srcPath.ParentDir.Path + "|" + destPath.ParentDir.Path;
      if (!_NonMovableDirs.Contains(dirKey))
      {
        try
        {
          // Может быть, стоит проверять наличие конечного файла и сначала удалять его.
          // Но тогда надо быть уверенным, что исходный и конечный файл - это не одно и тоже.
          // Так как могут быть всякие ссылки и прочее, то таких гарантий нет.
          // Надо временно переименовывать конечный файл, но тогда может не хватить места на диске...

          ThrowIfNoMove();
          spl.PhaseText = String.Format(Res.FileBatchHandler_Phase_MoveFile, destPath.Path);
          File.Move(srcPath.Path, destPath.Path);
          logList.Add(new LogInfo(FileAction.MoveFile, srcPath, destPath));
          return;
        }
        catch (IOException)
        {
          _NonMovableDirs.Add(dirKey);
        }
      }

      // теперь пытаемся скопировать + удалить
      spl.PhaseText = String.Format(Res.FileBatchHandler_Phase_CopyFile, destPath.Path);
      File.Copy(srcPath.Path, destPath.Path, true);
      spl.PhaseText = String.Format(Res.FileBatchHandler_Phase_DeleteFile, srcPath.Path);
      File.Delete(srcPath.Path);
      logList.Add(new LogInfo(FileAction.MoveFile, srcPath, destPath));
    }

    private void ThrowIfNoMove()
    {
      if (MoveAsCopyAndDelete)
        throw new IOException();
    }

    private void DoMoveDirectory(AbsPath srcPath, AbsPath destPath, List<LogInfo> logList, ISimpleSplash spl)
    {
      ForceDirs(destPath.ParentDir, logList, spl);

      // сначала пытаемся переместить
      string dirKey = srcPath.ParentDir.Path + "|" + destPath.ParentDir.Path;
      if (!_NonMovableDirs.Contains(dirKey))
      {
        try
        {
          ThrowIfNoMove();
          spl.PhaseText = String.Format(Res.FileBatchHandler_Phase_MoveDir, destPath.Path);
          Directory.Move(srcPath.Path, destPath.Path);
          logList.Add(new LogInfo(FileAction.MoveDir, srcPath, destPath));
          return;
        }
        catch
        {
          _NonMovableDirs.Add(dirKey);
        }
      }

      // теперь пытаемся скопировать + удалить
      DoCopyDirectory(srcPath, destPath, null, spl);
      spl.PhaseText = String.Format(Res.FileBatchHandler_Phase_DeleteDir, srcPath.Path);
      Directory.Delete(srcPath.Path, true);
      logList.Add(new LogInfo(FileAction.MoveDir, srcPath, destPath));
    }

    private void DoMoveTemplateFiles(AbsPath srcPath, AbsPath destPath, string template, bool recurse, List<LogInfo> logList, ISimpleSplash spl)
    {
      string[] aFiles = Directory.GetFiles(srcPath.Path, template);
      if (aFiles.Length > 0)
      {
        ForceDirs(destPath, logList, spl);
        foreach (string file in aFiles)
        {
          AbsPath srcFile = new AbsPath(file);
          AbsPath destFile = new AbsPath(destPath, srcFile.FileName);
          DoMoveFile(srcFile, destFile, logList, spl);
        }
      }

      if (recurse)
      {
        string[] aDirs = Directory.GetDirectories(srcPath.Path);
        foreach (string dir in aDirs)
        {
          AbsPath srcDir = new AbsPath(dir);
          AbsPath destDir = new AbsPath(destPath, srcDir.FileName);
          DoMoveTemplateFiles(srcDir, destDir, template, recurse, logList, spl); // рекурсия
        }
      }
    }

    #endregion
  }
}
