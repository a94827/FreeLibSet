// Part of FreeLibSet.
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
    private readonly FileCopierForm _Form;

    public ProgressBar PB { get { return _PB; } }
    private readonly ProgressBar _PB;

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
        if (EFPApp.MessageBox(Res.FileCopier_Msg_Cancel, Res.FileCopier_Title_Cancel,
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
    private readonly List<string> _Templates;

    /// <summary>
    /// Добавить шаблон имен файлов для копирования
    /// </summary>
    /// <param name="template">Шаблон, например, "*.txt"</param>
    /// <param name="recurse">Если true, то будет выполнено рекурсивное копирование вложенных каталогов</param>
    public void AddTemplate(string template, bool recurse)
    {
      if (String.IsNullOrEmpty(template))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("template");
      if (template[0] == Path.DirectorySeparatorChar)
        throw ExceptionFactory.ArgInvalidChar("template", template, 0);
      if (template[template.Length - 1] == Path.DirectorySeparatorChar)
        throw ExceptionFactory.ArgInvalidChar("template", template, template.Length - 1);
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
        throw ExceptionFactory.ObjectPropertyNotSet(this, "SrcDir");
      if (DstDir.IsEmpty)
        throw ExceptionFactory.ObjectPropertyNotSet(this, "DstDir");
      if (_Templates.Count == 0)
        throw ExceptionFactory.ObjectPropertyNotSet(this, "Templates");

      //      SrcDir = FileTools.GetFullDirName(SrcDir);
      //      DstDir = FileTools.GetFullDirName(DstDir);

      BeginForm();
      _Form.Text = Res.FileCopier_Phase_Copy;
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
      _Form.grpTotal.Text = Res.FileCopier_Phase_Prepare;
      _Form.grpTotal.Visible = true;
      _FileNames = new List<string>();
      _TotalSize = 0L;

      if (!EFPApp.DirectoryExists(SrcDir))
        throw ExceptionFactory.DirectoryNotFound(SrcDir);

      _Form.txtTotalFiles.Text = "0";
      _Form.txtTotalBytes.Text = "0";
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
          throw new BugException("File lost: \"" + a[i] + "\"");
        if (_FileNames.Contains(fileName))
          continue; // файл может входить больше чем в один шаблон
        _TotalSize += fi.Length;
        _FileNames.Add(fileName);
      }

      _Form.txtTotalFiles.Text = _FileNames.Count.ToString();
      _Form.txtTotalBytes.Text = _TotalSize.ToString();
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
      _Form.txtCopiedFiles.Text = "0";
      _Form.txtCopiedBytes.Text = "0";
      _Form.txtTotalFiles.Text = _FileNames.Count.ToString();
      _Form.txtTotalBytes.Text = _TotalSize.ToString();
      _Form.ExtPBTotal.MaxValue = _TotalSize;
      _Form.grpTotal.Text = Res.FileCopier_Phase_Copy;
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
          throw ExceptionFactory.FileNotFound(thisPath);

        if (EnvironmentTools.IsWindowsPlatform)
        {
          if (DstDir.Path.StartsWith("A:\\", StringComparison.OrdinalIgnoreCase) || DstDir.Path.StartsWith("B:\\", StringComparison.OrdinalIgnoreCase))
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
              DialogResult res = EFPApp.MessageBox(String.Format(Res.FileCopier_Msg_ChangeFloppy,
                DstDir.Path.Substring(0, 2), _FileNames[i], fi.Length.ToString(), freeSpace.ToString()),
                Res.FileCopier_Title_ChangeFloppy, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information);
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
        }

        _Form.txtFileName.Text = Path.GetFileName(_FileNames[i]);
        _Form.txtFileSize.Text = Res.FileCopier_Msg_SizeAsDir;
        Application.DoEvents();
        // Создаем каталог
        AbsPath dstFile = DstDir + _FileNames[i];
        FileTools.ForceDirs(dstFile.ParentDir);
        // Копирование одного файла
        _Form.txtFileSize.Text = fi.Length.ToString();
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
              _Form.txtCopiedBytes.Text = _Form.ExtPBTotal.Value.ToString();
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
        _Form.txtFileSize.Text = Res.FileCopier_Msg_SizeAsFileAttr;
        FileInfo fiDst = new FileInfo(dstFile.Path); // исправлено 18.03.2016
        if (fi.CreationTime.Year < 1980)
          fiDst.CreationTime = fi.LastWriteTime;
        else
          fiDst.CreationTime = fi.CreationTime;
        fiDst.LastWriteTime = fi.LastWriteTime;

        _CopiedFiles.Add(_FileNames[i]);
        _CopiedTotalSize += fi.Length;
        _Form.txtCopiedFiles.Text = (i + 1).ToString();
      }

      CheckPrevFiles();
    }

    private void CheckPrevFiles()
    {
      if (!FileTools.IsFloppyDriveDir(DstDir))
        return;
      if (EFPApp.MessageBox(String.Format(Res.FileCopier_Msg_CheckDisk,
        _CopiedFiles.Count, DstDir.Path.Substring(0, 2)),
        Res.FileCopier_Title_CheckDisk, MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk) == DialogResult.OK)
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
            throw new IOException(String.Format(Res.FileCopier_Err_CheckDiskFail, _CopiedFiles[i], e.Message), e);
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
      _Form.txtFileName.Text = Path.GetFileName(fileName);
      FileInfo fiSrc = new FileInfo(srcFile.Path);
      if (!fiSrc.Exists)
        throw ExceptionFactory.FileNotFound(srcFile);
      _Form.txtFileSize.Text = fiSrc.Length.ToString();

      FileTools.ForceDirs(dstFile.ParentDir);

      FileStream fsSrc = new FileStream(srcFile.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
      try
      {
        FileStream fsDst = new FileStream(DstDir.Path + fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        try
        {
          if (fsDst.Length != fsSrc.Length)
            throw new IOException(String.Format(Res.FileCopier_Err_CheckLen, fsSrc.Length, fsDst.Length));
          for (int i = 0; i < fsSrc.Length; i++)
          {
            if (fsDst.ReadByte() != fsSrc.ReadByte())
              throw new IOException(Res.FileCopier_Err_CheckBytes);
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
        throw new ArgumentException("Invalid path", "path");
      try
      {

        DriveInfo info = new DriveInfo(path.Substring(0, 1));
        return info.TotalFreeSpace;
      }
      catch (Exception e)
      {
        EFPApp.MessageBox(e.Message,
          String.Format(Res.FileCopier_Title_FreeSpaceDetectionError, path.Substring(0, 2)),
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
        EFPApp.ErrorMessageBox(e.Message);
      }
    }

    private static void DoShowClearFloppyMenu(AbsPath rootDir)
    {
      string[] files = Directory.GetFiles(rootDir.Path);
      string[] subDirs = Directory.GetDirectories(rootDir.Path);

      RadioSelectDialog dlg = new RadioSelectDialog();
      dlg.Title = String.Format(Res.FileCopier_Title_ClearDialog, rootDir.Path.Substring(0, 2));
      dlg.GroupTitle = Res.FileCopier_Title_ClearDialogGroupTitle;
      dlg.Items = new string[]{
        Res.FileCopier_Msg_ClearDialogExplorer,
        String.Format(Res.FileCopier_Msg_ClearDialogDelete, files.Length,subDirs.Length)};
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
        if (EFPApp.MessageBox(String.Format(Res.FileCopier_Msg_ConfirmDelele, rootDir.Path.Substring(0, 2)),
          Res.FileCopier_Title_ConfirmDelele, MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) != DialogResult.OK)
          return;
        Splash spl = new Splash(String.Format(Res.FileCopier_Phase_ClearDisk, rootDir.Path.Substring(0, 2)));
        try
        {
          spl.AllowCancel = true;
          spl.PercentMax = files.Length + subDirs.Length;
          for (int i = 0; i < files.Length; i++)
          {
            spl.PhaseText = String.Format(Res.FileCopier_Phase_DeleteFile, files[i]);
            spl.CheckCancelled();
            File.Delete(files[i]);
            spl.IncPercent();
          }
          for (int i = 0; i < subDirs.Length; i++)
          {
            spl.PhaseText = String.Format(Res.FileCopier_Phase_DeleteDir, subDirs[i]);
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
