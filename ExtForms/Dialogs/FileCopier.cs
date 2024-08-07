﻿// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using FreeLibSet.IO;
using FreeLibSet.Core;

namespace FreeLibSet.Forms
{
  internal partial class FileCopierForm : Form
  {
    #region Конструктор формы

    public FileCopierForm()
    {
      InitializeComponent();
      ExtPBFile = new FileCopierFormPB(this, pbFile);
      ExtPBTotal = new FileCopierFormPB(this, pbTotal);
      ExtPBCheck = new FileCopierFormPB(this, pbCheck);
      //EFPApp.InitFormImages(this);
      btnCancel.Image = EFPApp.MainImages.Images["Cancel"];
      btnCancel.ImageAlign = ContentAlignment.BottomLeft;
    }

    #endregion

    #region Обработчики формы

    private void FileCopierForm_FormClosing(object sender, FormClosingEventArgs args)
    {
      args.Cancel = !AllowClose;
      if (!AllowClose)
        CancelClicked = true;
    }

    private void btnCancel_Click(object sender, EventArgs args)
    {
      if (!AllowClose)
        CancelClicked = true;
    }

    #endregion

    #region Поля

    public bool AllowClose;

    public bool CancelClicked;

    public FileCopierFormPB ExtPBFile, ExtPBTotal, ExtPBCheck;

    #endregion
  }

  /// <summary>
  /// Управление процентными индикаторами в форме
  /// </summary>
  internal class FileCopierFormPB
  {
    #region Конструктор

    public FileCopierFormPB(FileCopierForm form, ProgressBar pb)
    {
      _Form = form;
      _PB = pb;
    }

    #endregion

    #region Свойства

    public FileCopierForm Form { get { return _Form; } }
    private FileCopierForm _Form;

    public ProgressBar PB { get { return _PB; } }
    private ProgressBar _PB;

    public long MaxValue
    {
      get { return _MaxValue; }
      set
      {
        _MaxValue = value;
        PB.Visible = value > 0L;
        PB.Value = 0;
        _Value = 0L;
        Application.DoEvents();
        CheckCancel();
      }
    }
    private long _MaxValue;

    public long Value
    {
      get { return _Value; }
      set
      {
        _Value = value;
        if (_MaxValue > 0)
        {
          if (value < 0L)
            value = 0L;
          if (value > _MaxValue)
            value = _MaxValue;
          PB.Value = (int)(value * 100L / _MaxValue);
          Application.DoEvents();
          CheckCancel();
        }
      }
    }
    private long _Value;

    public void Inc()
    {
      Inc(1L);
    }
    public void Inc(long delta)
    {
      Value += delta;
    }

    private void CheckCancel()
    {
      if (_Form.CancelClicked)
      {
        _Form.CancelClicked = false;
        if (EFPApp.MessageBox("Прервать операцию?", "Подтверждение",
          MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)

          throw new UserCancelException();
      }
    }

    #endregion
  }

  /// <summary>
  /// Копировщик файлов с выводом процентного индикатора.
  /// Может отображаться как отдельное окно или как шаг мастера (<see cref="WizardStep"/>).
  /// </summary>
  public class FileCopier
  {
    #region Конструкторы

    /// <summary>
    /// Создать копировщик, использующий отдельное окно
    /// </summary>
    public FileCopier()
      : this(null)
    {
    }

    /// <summary>
    /// Эта версия конструктора предназначена для копирования файла в окне мастера.
    /// При вызове метода <see cref="Copy()"/>, вместо открытия отдельного окна, выводится временная
    /// страница в окне мастера.
    /// </summary>
    /// <param name="wizard">Мастер, в который встраивается окно</param>
    public FileCopier(Wizard wizard)
    {
      _Wizard = wizard;
      _Templates = new List<string>();
    }

    #endregion

    #region Исходные данные

    /// <summary>
    /// Исходный каталог
    /// </summary>
    public AbsPath SrcDir { get { return _SrcDir; } set { _SrcDir = value; } }
    private AbsPath _SrcDir;

    /// <summary>
    /// Результирующий каталог
    /// </summary>
    public AbsPath DstDir { get { return _DstDir; } set { _DstDir = value; } }
    private AbsPath _DstDir;

    /// <summary>
    /// Список шаблонов имен файлов
    /// </summary>
    private List<string> _Templates;

    /// <summary>
    /// Добавить шаблон имен файлов для копирования
    /// </summary>
    /// <param name="template">Шаблон, например, "*.txt"</param>
    /// <param name="recurse">Если true, то будет выполнено рекурсивное копирование вложенных каталогов</param>
    public void AddTemplate(string template, bool recurse)
    {
      if (String.IsNullOrEmpty(template))
        throw new ArgumentNullException("template");
      if (template[0] == Path.DirectorySeparatorChar || template[template.Length - 1] == Path.DirectorySeparatorChar)
        throw new ArgumentException("Шаблон не может начинаться или заканчиваться символом \"" + Path.DirectorySeparatorChar + "\"");
      _Templates.Add((recurse ? ">" : "") + template);
    }

    /// <summary>
    /// Добавить шаблон имен файлов для копирования.
    /// Рекурсивное копирование вложенных каталогов не выполняется.
    /// </summary>
    /// <param name="template">Шаблон, например, "*.txt"</param>
    public void AddTemplate(string template)
    {
      AddTemplate(template, false);
    }

    /// <summary>
    /// Мастер, в котором отображается процентый индикатор.
    /// Если мастер не используется, то свойство равно null.
    /// Для задания свойства используется специальная версия конструктора.
    /// </summary>
    public Wizard Wizard { get { return _Wizard; } }
    private readonly Wizard _Wizard;

    #endregion

    #region Методы

    /// <summary>
    /// Основной метод.
    /// Выполняет копирование из каталога <see cref="SrcDir"/> в <see cref="DstDir"/> в соответствии с добавлеными шаблонами.
    /// </summary>
    public void Copy()
    {
      if (SrcDir.IsEmpty)
        throw new NullReferenceException("Не задан исходный каталог");
      if (DstDir.IsEmpty)
        throw new NullReferenceException("Не задан конечный каталог");
      if (_Templates.Count == 0)
        throw new InvalidOperationException("Не задано ни одного шаблона файлов");

      //      SrcDir = FileTools.GetFullDirName(SrcDir);
      //      DstDir = FileTools.GetFullDirName(DstDir);

      BeginForm();
      _Form.Text = "Копирование файлов";
      try
      {
        CreateFileList();
        DoCopy();
      }
      finally
      {
        EndForm();
      }
    }

    #endregion

    #region Список файлов

    /// <summary>
    /// Имена файлов (без шаблонов) и подкаталогов
    /// </summary>
    private List<string> _FileNames;

    /// <summary>
    /// Общий размер всех файлов
    /// </summary>
    private long _TotalSize;

    private void CreateFileList()
    {
      _Form.grpTotal.Text = "Подготовка к копированию";
      _Form.grpTotal.Visible = true;
      _FileNames = new List<string>();
      _TotalSize = 0L;

      if (!EFPApp.DirectoryExists(SrcDir))
        throw new DirectoryNotFoundException("Исходный каталог не существует: \"" + SrcDir + "\"");

      _Form.lblTotalFiles.Text = "0";
      _Form.lblTotalBytes.Text = "0";
      _Form.ExtPBTotal.MaxValue = 2 * _Templates.Count;
      for (int i = 0; i < _Templates.Count; i++)
      {
        bool recurse = false;
        string template = _Templates[i];
        if (template.StartsWith(">", StringComparison.Ordinal))
        {
          recurse = true;
          template = template.Substring(1);
        }
        string subDir = String.Empty;
        int p = template.LastIndexOf(Path.DirectorySeparatorChar);
        if (p >= 0)
        {
          subDir = template.Substring(0, p + 1);
          template = template.Substring(p + 1);
        }
        DoAddFiles(SrcDir + subDir, template, recurse);
      }
    }

    private void DoAddFiles(AbsPath dir, string template, bool recurse)
    {
      string[] a = Directory.GetFiles(dir.Path, template, recurse ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
      Array.Sort<string>(a);
      _Form.ExtPBTotal.Inc();

      for (int i = 0; i < a.Length; i++)
      {
        string fileName = a[i].Substring(dir.SlashedPath.Length);
        FileInfo fi = new FileInfo(dir.SlashedPath + fileName);
        if (!fi.Exists)
          throw new BugException("Потеряли файл \"" + a[i] + "\"");
        if (_FileNames.Contains(fileName))
          continue; // файл может входить больше чем в один шаблон
        _TotalSize += fi.Length;
        _FileNames.Add(fileName);
      }

      _Form.lblTotalFiles.Text = _FileNames.Count.ToString();
      _Form.lblTotalBytes.Text = _TotalSize.ToString();
      _Form.ExtPBTotal.Inc();
    }

    #endregion

    #region Копирование файлов

    /// <summary>
    /// Список уже скопированных, но еще не проверенных файлов
    /// </summary>
    private List<string> _CopiedFiles;

    /// <summary>
    /// Размер уже скопированных файлов
    /// </summary>
    private long _CopiedTotalSize;

    private void DoCopy()
    {
      _Form.lblCopiedFiles.Text = "0";
      _Form.lblCopiedBytes.Text = "0";
      _Form.lblTotalFiles.Text = _FileNames.Count.ToString();
      _Form.lblTotalBytes.Text = _TotalSize.ToString();
      _Form.ExtPBTotal.MaxValue = _TotalSize;
      _Form.grpTotal.Text = "Копирование";
      _Form.grpTotal.Visible = true;
      _Form.grpCurrentFile.Visible = true;

      _CopiedFiles = new List<string>();
      _CopiedTotalSize = 0L;
      byte[] buffer = new byte[32768]; // Размер буфера копирования
      for (int i = 0; i < _FileNames.Count; i++)
      {
        AbsPath thisPath = new AbsPath(SrcDir.SlashedPath + _FileNames[i]);
        FileInfo fi = new FileInfo(thisPath.Path);
        if (!fi.Exists)
          throw new FileNotFoundException("Не найден исходный файл", thisPath.Path);

        if (DstDir.Path.StartsWith("A:\\", StringComparison.OrdinalIgnoreCase))
        {
          long freeSpace = GetFreeSpace(DstDir.Path);
          if (freeSpace < fi.Length)
          {
            // Перед сменой дискеты надо выполнить проверку
            CheckPrevFiles();
          }
          // Требуем вставить дискету
          while (freeSpace < fi.Length)
          {
            DialogResult res = EFPApp.MessageBox("Вставьте следующую дискету в устройство " +
              DstDir.Path.Substring(0, 2) + " для копирования файла \"" + _FileNames[i] +
              "\". Размер файла: " + fi.Length.ToString() +
              ", на текущей дискете свободного места: " + freeSpace.ToString() +
              ". Нажмите <Да>, после замены дискеты или очистки существующей. Нажмите <Нет> для очистки текущей дискеты с помощью Проводника Windows или автоматически",
              "Смена дискеты", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);
            switch (res)
            {
              case DialogResult.Yes:
                break;
              case DialogResult.No:
                ShowClearFloppyMenu(DstDir);
                break;
              default:
                throw new UserCancelException();
            }
            // Перечитываем размер
            freeSpace = GetFreeSpace(DstDir.Path);
          }
        }

        _Form.lblFileName.Text = Path.GetFileName(_FileNames[i]);
        _Form.lblFileSize.Text = "Каталог";
        Application.DoEvents();
        // Создаем каталог
        AbsPath dstFile = DstDir + _FileNames[i];
        FileTools.ForceDirs(dstFile.ParentDir);
        // Копирование одного файла
        _Form.lblFileSize.Text = fi.Length.ToString();
        _Form.ExtPBFile.MaxValue = fi.Length;
        FileStream fsSrc = new FileStream(SrcDir.SlashedPath + _FileNames[i], FileMode.Open, FileAccess.Read, FileShare.Read);
        try
        {
          FileStream fsDst = new FileStream(DstDir.SlashedPath + _FileNames[i], FileMode.Create, FileAccess.Write, FileShare.None);
          try
          {
            while (true)
            {
              int count = fsSrc.Read(buffer, 0, buffer.Length);
              if (count == 0)
                break;
              fsDst.Write(buffer, 0, count);
              _Form.ExtPBFile.Inc(count);
              _Form.ExtPBTotal.Inc(count);
              _Form.lblCopiedBytes.Text = _Form.ExtPBTotal.Value.ToString();
            }

            fsDst.Close();
            fsSrc.Close();
          }
          finally
          {
            fsDst.Dispose();
          }
        }
        finally
        {
          fsSrc.Dispose();
        }
        _Form.lblFileSize.Text = "Атрибуты";
        FileInfo fiDst = new FileInfo(dstFile.Path); // исправлено 18.03.2016
        if (fi.CreationTime.Year < 1980)
          fiDst.CreationTime = fi.LastWriteTime;
        else
          fiDst.CreationTime = fi.CreationTime;
        fiDst.LastWriteTime = fi.LastWriteTime;

        _CopiedFiles.Add(_FileNames[i]);
        _CopiedTotalSize += fi.Length;
        _Form.lblCopiedFiles.Text = (i + 1).ToString();
      }

      CheckPrevFiles();
    }

    private void CheckPrevFiles()
    {
      if (!FileTools.IsFloppyDriveDir(DstDir))
        return;
      if (EFPApp.MessageBox("Для проверки " + (_CopiedFiles.Count == 1 ? "записанного файла" : "записанных файлов") +
        " извлеките дискету " +
        DstDir.Path.Substring(0, 2) + " и вставьте ее обратно",
        "Проверка записи", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk) == DialogResult.OK)
      {
        _Form.ExtPBCheck.MaxValue = _CopiedTotalSize;
        _Form.grpCheck.Visible = true;
        for (int i = 0; i < _CopiedFiles.Count; i++)
        {
          try
          {
            CheckOneFile(_CopiedFiles[i]);
          }
          catch (Exception e)
          {
            throw new IOException("Ошибка при проверке файла \"" + _CopiedFiles[i] + "\". " + e.Message, e);
          }
        }
      }

      _Form.grpCheck.Visible = false;
      _CopiedFiles.Clear();
      _CopiedTotalSize = 0L;
    }

    private void CheckOneFile(string fileName)
    {
      AbsPath srcFile = SrcDir + fileName;
      AbsPath dstFile = DstDir + fileName;
      _Form.lblFileName.Text = Path.GetFileName(fileName);
      FileInfo fiSrc = new FileInfo(srcFile.Path);
      if (!fiSrc.Exists)
        throw new FileNotFoundException("Не найден исходный файл", srcFile.Path);
      _Form.lblFileSize.Text = fiSrc.Length.ToString();

      FileTools.ForceDirs(dstFile.ParentDir);

      FileStream fsSrc = new FileStream(srcFile.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
      try
      {
        FileStream fsDst = new FileStream(DstDir.Path + fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        try
        {
          if (fsDst.Length != fsSrc.Length)
            throw new IOException("Исходный файл имеет длину " + fsSrc.Length.ToString() +
              ", а записанный - " + fsDst.Length.ToString());
          for (int i = 0; i < fsSrc.Length; i++)
          {
            if (fsDst.ReadByte() != fsSrc.ReadByte())
              throw new IOException("Ошибка при копировании файла. Файл записался неправильно");
          }
          _Form.ExtPBCheck.Inc(fsDst.Length);
        }
        finally
        {
          fsDst.Close();
        }
      }
      finally
      {
        fsSrc.Close();
      }
    }

    static long GetFreeSpace(string path)
    {
      if (path.Length < 3 || path.Substring(1, 2) != ":\\")
        throw new ArgumentException("Неправильный путь", "path");
      try
      {

        DriveInfo info = new DriveInfo(path.Substring(0, 1));
        return info.TotalFreeSpace;
      }
      catch (Exception e)
      {
        EFPApp.MessageBox(e.Message,
          "Ошибка при определении свободного места на диске " + path.Substring(0, 2),
          MessageBoxButtons.OK, MessageBoxIcon.Error);
        return 0;
      }
    }

    #endregion

    #region Окно заставки

    private void BeginForm()
    {
      _Form = new FileCopierForm();
      if (_Wizard == null)
      {
        _Form.Icon = EFPApp.MainImages.Icons["Computer"];
        EFPApp.ShowFormInternal(_Form);
      }
      else
      {
        WizardTempPage tempPage = new WizardTempPage(_Form.MainPanel);
        tempPage.CancelClick += new EventHandler(TempPage_CancelClick);
        tempPage.CancelEnabled = true;
        _Wizard.BeginTempPage(tempPage);
      }
      _Form.CancelClicked = false;
    }

    private void EndForm()
    {
      if (_TempPage != null)
        _Wizard.EndTempPage();
      else
      {
        _Form.AllowClose = true;
        _Form.Close();
        _Form.Dispose();
      }
      _TempPage = null;
      _Form = null;
    }

    private void TempPage_CancelClick(object sender, EventArgs args)
    {
      _Form.CancelClicked = true;
    }

    private FileCopierForm _Form;
    private WizardTempPage _TempPage;

    #endregion

    #region Меню очистки диска

    /// <summary>
    /// Выводит диалог, в котором можно выполнить очистку файлов из заданного каталога.
    /// </summary>
    /// <param name="rootDir">Каталог, обычно, корень диска A</param>
    public static void ShowClearFloppyMenu(AbsPath rootDir)
    {
      try
      {
        DoShowClearFloppyMenu(rootDir);
      }
      catch (Exception e)
      {
        EFPApp.MessageBox(e.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private static void DoShowClearFloppyMenu(AbsPath rootDir)
    {
      string[] files = Directory.GetFiles(rootDir.Path);
      string[] subDirs = Directory.GetDirectories(rootDir.Path);

      RadioSelectDialog dlg = new RadioSelectDialog();
      dlg.Title = "Очистка диска " + rootDir.Path.Substring(0, 2);
      dlg.GroupTitle = "Что сделать";
      dlg.Items = new string[]{
        "Запустить проводник Windows",
        "Удалить все файлы ("+files.Length.ToString()+") и каталоги ("+subDirs.Length.ToString()+")"};
      dlg.SelectedIndex = 0;
#if DEBUG
      if (dlg.EnabledItemFlags == null)
        throw new BugException("dlg.EnabledItemFlags==null");
#endif
      dlg.EnabledItemFlags[0] = EFPApp.IsWindowsExplorerSupported;
      if (dlg.ShowDialog() != DialogResult.OK)
        return;
      if (dlg.SelectedIndex == 0)
        EFPApp.ShowWindowsExplorer(rootDir);
      else
      {
        if (EFPApp.MessageBox("Сейчас все сушествующие файлы и каталоги на диске " +
          rootDir.Path.Substring(0, 2) + " будут необратимо УНИЧТОЖЕНЫ. Подтвердите Ваше намерение очистить диск",
          "Подтверждение", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) != DialogResult.OK)
          return;
        Splash spl = new Splash("Очистка диска " + rootDir.Path.Substring(0, 2));
        try
        {
          spl.AllowCancel = true;
          spl.PercentMax = files.Length + subDirs.Length;
          for (int i = 0; i < files.Length; i++)
          {
            spl.PhaseText = "Удаление файла \"" + files[i] + "\"";
            spl.CheckCancelled();
            File.Delete(files[i]);
            spl.IncPercent();
          }
          for (int i = 0; i < subDirs.Length; i++)
          {
            spl.PhaseText = "Удаление каталога \"" + subDirs[i] + "\"";
            spl.CheckCancelled();
            Directory.Delete(subDirs[i], true);
            spl.IncPercent();
          }
        }
        finally
        {
          spl.Close();
        }
      }
    }

    #endregion
  }
}
