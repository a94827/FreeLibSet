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
using FreeLibSet.IO;
using FreeLibSet.Collections;
using FreeLibSet.UICore;

namespace FreeLibSet.Forms
{
  internal partial class FileBrowseForm : OKCancelForm
  {
    #region Конструктор

    public FileBrowseForm()
    {
      InitializeComponent();
    }

    #endregion

    #region Поля

    public EFPHistComboBox efpMainCB;

    public EFPCheckBox efpSubFolders;

    #endregion
  }

  #region Перечисление FileDialogMode

  /// <summary>
  /// Режим диалога выбора файла: для чтения или для записи
  /// </summary>
  public enum FileDialogMode
  {
    /// <summary>
    /// Файл предполагается открывать для чтения
    /// </summary>
    Read,

    /// <summary>
    /// Файл предполагается открывать на запись
    /// </summary>
    Write
  }

  #endregion

  /// <summary>
  /// Диалог выбора каталога. Диалог содержит поле ввода пути и кнопку "Обзор".
  /// Имеет выпадающий список с ранее выбранными каталогами (история).
  /// Также может быть выведена кнопка "Включая подкаталоги".
  /// </summary>
  public class HistFolderBrowserDialog
  {
    #region Конструктор

    /// <summary>
    /// Инициализация диалога параметрами по умолчанию
    /// </summary>
    public HistFolderBrowserDialog()
    {
      _Title = "Выбор папки";
      _ImageKey = "Open";
      _Description = String.Empty;
      _MaxHistLength = HistoryList.DefaultMaxHistLength;
      _Mode = FileDialogMode.Read;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Заголовок блока диалога
    /// </summary>
    public string Title { get { return _Title; } set { _Title = value; } }
    private string _Title;

    /// <summary>
    /// Описание в нижней части диалога
    /// </summary>
    public string Description { get { return _Description; } set { _Description = value; } }
    private string _Description;

    /// <summary>
    /// Значок формы (изображение в EFPApp.MainImages)
    /// </summary>
    public string ImageKey { get { return _ImageKey; } set { _ImageKey = value; } }
    private string _ImageKey;

    /// <summary>
    /// Режим: открытие или запись.
    /// По умолчанию - Read
    /// </summary>
    public FileDialogMode Mode { get { return _Mode; } set { _Mode = value; } }
    private FileDialogMode _Mode;

    /// <summary>
    /// Максимальная длина списка истории
    /// По умолчанию - 10 значений
    /// </summary>
    public int MaxHistLength { get { return _MaxHistLength; } set { _MaxHistLength = value; } }
    private int _MaxHistLength;

    /// <summary>
    /// Основное свойство - список истории. Строка с нулевым индексом соответствует
    /// выбранному значению
    /// </summary>
    public HistoryList HistList { get { return _HistList; } set { _HistList = value; } }
    private HistoryList _HistList;

    /// <summary>
    /// Упрощенный доступ к каталогу.
    /// Установка свойства добавляет каталог к списку.
    /// Текущий каталог (если не пустая строка) всегда заканчивается символом "\"
    /// </summary>
    public string SelectedPath
    {
      get { return HistList.Top; }
      set
      {
        AbsPath Path = new AbsPath(value);
        HistList = HistList.Add(Path.SlashedPath, MaxHistLength);
      }
    }

    /// <summary>
    /// Каталог по умолчанию (по умолчанию - не задано).
    /// Если свойство установлено, а список истории пустой, то при открытии диалога
    /// выбирается каталог по умолчанию. Каталог по умолчанию присутсвует в 
    /// выпадающем списке, но не сохраняется в истории, если он был выбран в явном виде
    /// </summary>
    public string DefaultPath
    {
      get { return _DefaultPath; }
      set
      {
        AbsPath Path = new AbsPath(value);
        _DefaultPath = Path.SlashedPath;
      }
    }
    private string _DefaultPath;

    /// <summary>
    /// Если это свойство установить в true, то будет показана кнопка
    /// "Включая подкаталоги"
    /// </summary>
    public bool ShowSubFoldersButton { get { return _ShowSubFoldersButton; } set { _ShowSubFoldersButton = value; } }
    private bool _ShowSubFoldersButton;

    /// <summary>
    /// Значение переключателя "Включая подкаталоги".
    /// Чтобы переключатель был выведен на экран, должно быть установлено
    /// свойство ShowSubFoldersButton
    /// </summary>
    public bool SubFolders { get { return _SubFolders; } set { _SubFolders = value; } }
    private bool _SubFolders;

    #endregion

    #region Проверка введенного пути

    /// <summary>
    /// Режим проверки введенного пути.
    /// Значение по умолчанию зависит от свойства Mode. При Mode=Write возвращает RootExists,
    /// а при false - DirectoryExists
    /// </summary>
    public TestPathMode PathValidateMode
    {
      get
      {
        if (_PathValidateMode.HasValue)
          return _PathValidateMode.Value;
        else
          return (Mode == FileDialogMode.Write) ? TestPathMode.RootExists : TestPathMode.DirectoryExists;
      }
      set { _PathValidateMode = value; }
    }
    private TestPathMode? _PathValidateMode;

    /// <summary>
    /// Сбрасывает свойство PathValidateMode в значение по умолчанию
    /// </summary>
    public void ResetPathValidateMode()
    {
      _PathValidateMode = null;
    }

    #endregion

    #region Показ диалога

    private FileBrowseForm _TheForm;

    /// <summary>
    /// Показывает блок диалога.
    /// </summary>
    /// <returns>Результат работы</returns>
    public DialogResult ShowDialog()
    {
      DialogResult res;
      _TheForm = new FileBrowseForm();
      try
      {
        _TheForm.Text = Title;
        _TheForm.Icon = EFPApp.MainImageIcon(ImageKey);
        _TheForm.grpMain.Text = "Каталог";
        _TheForm.TextLabel.Text = "Путь";
        _TheForm.lblDescription.Text = Description;

        _TheForm.efpMainCB = new EFPHistComboBox(_TheForm.FormProvider, _TheForm.MainCB);
        _TheForm.efpMainCB.CanBeEmpty = false;
        _TheForm.efpMainCB.MaxHistLength = MaxHistLength;
        _TheForm.efpMainCB.HistList = HistList;
        if (!String.IsNullOrEmpty(DefaultPath))
          _TheForm.efpMainCB.DefaultItems = new string[1] { DefaultPath };
        _TheForm.efpMainCB.ToolTipText = "Поле ввода каталога." + Environment.NewLine +
          "Чтобы выбрать ранее использованный путь, используйте выпадающий список." + Environment.NewLine +
          "Чтобы выбрать каталог с помощью стандартного диалога Windows, нажмите кнопку \"Обзор\"";

        EFPFolderBrowserButton efpBrowse = new EFPFolderBrowserButton(_TheForm.efpMainCB, _TheForm.btnBrowse);
        efpBrowse.ShowNewFolderButton = (Mode == FileDialogMode.Write);
        efpBrowse.Description = Description; // 31.12.2020
        efpBrowse.PathValidateMode = PathValidateMode;

        if (ShowSubFoldersButton)
        {
          _TheForm.PanelSubFolders.Visible = true;
          _TheForm.efpSubFolders = new EFPCheckBox(_TheForm.FormProvider, _TheForm.cbSubFolders);
          _TheForm.efpSubFolders.ToolTipText = "Если флажок установлен, то будут просмотрены также все вложенные каталоги";
          _TheForm.efpSubFolders.Checked = SubFolders;
        }

        EFPWindowsExplorerButton efpExplorer = new EFPWindowsExplorerButton(_TheForm.efpMainCB, _TheForm.btnExplorer);
        efpExplorer.IsFileName = false;

        _TheForm.FormProvider.FormClosing += new FormClosingEventHandler(Form_Closing);

        res = EFPApp.ShowDialog(_TheForm, true);
        if (res == DialogResult.OK)
        {
          HistList = _TheForm.efpMainCB.HistList;
          if (ShowSubFoldersButton)
            SubFolders = _TheForm.efpSubFolders.Checked;

          EFPApp.CurrentDirectory = new AbsPath(_TheForm.efpMainCB.Text);
        }
      }
      finally
      {
        _TheForm = null;
      }
      return res;
    }

    private void Form_Closing(object sender, FormClosingEventArgs args)
    {
      if (_TheForm.DialogResult != DialogResult.OK)
        return;
      AbsPath Path = new AbsPath(_TheForm.efpMainCB.Text);
      _TheForm.efpMainCB.Text = Path.SlashedPath;


      if (Mode == FileDialogMode.Write)
      {
        try
        {
          FileTools.ForceDirs(Path);
        }
        catch (Exception e)
        {
          EFPApp.ShowTempMessage("Не удалось создать каталог. " + e.Message);
          _TheForm.DialogResult = DialogResult.Cancel;
          args.Cancel = true;
        }
      }
      else
      {
        if (!Directory.Exists(Path.Path))
        {
          EFPApp.ShowTempMessage("Каталог не существует");
          _TheForm.DialogResult = DialogResult.Cancel;
          args.Cancel = true;
        }
      }
    }

    #endregion
  }

  /// <summary>
  /// Диалог выбора файла. Диалог содержит поле ввода пути и кнопку "Обзор".
  /// Имеет выпадающий список с ранее выбранными файлами (история).
  /// </summary>
  public class HistFileBrowserDialog
  {
    #region Конструктор

    /// <summary>
    /// Инициализация диалога параметрами по умолчанию
    /// </summary>
    public HistFileBrowserDialog()
    {
      _Title = "Выбор файла";
      _ImageKey = "Open";
      _Description = String.Empty;
      _MaxHistLength = 10;
      _Mode = FileDialogMode.Read;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Заголовок блока диалога.
    /// По умолчанию - "Выбор файла"
    /// </summary>
    public string Title { get { return _Title; } set { _Title = value; } }
    private string _Title;

    /// <summary>
    /// Описание в нижней части диалога.
    /// По умолчанию - пусто
    /// </summary>
    public string Description { get { return _Description; } set { _Description = value; } }
    private string _Description;

    /// <summary>
    /// Значок формы (изображение в EFPApp.MainImages)
    /// </summary>
    public string ImageKey { get { return _ImageKey; } set { _ImageKey = value; } }
    private string _ImageKey;

    /// <summary>
    /// Предполагаемый режим открытия файла - на чтение или на запись.
    /// По умолчанию - Read
    /// </summary>
    public FileDialogMode Mode { get { return _Mode; } set { _Mode = value; } }
    private FileDialogMode _Mode;

    /// <summary>
    /// Фильтр для файлов в формате "Описание|Маска|...".
    /// См описание стандартных блоков диалога System.Windows.Forms.OpenFileDialog
    /// </summary>
    public string Filter { get { return _Filter; } set { _Filter = value; } }
    private string _Filter;

    /// <summary>
    /// Максимальное количество элементов в списке истории.
    /// По умолчанию: 10.
    /// </summary>
    public int MaxHistLength { get { return _MaxHistLength; } set { _MaxHistLength = value; } }
    private int _MaxHistLength;

    /// <summary>
    /// Основное свойство - список истории. 
    /// </summary>
    public HistoryList HistList { get { return _HistList; } set { _HistList = value; } }
    private HistoryList _HistList;

    /// <summary>
    /// Упрощенный доступ к имени выбранного файла (включая путь)
    /// Установка свойства добавляет список к истории
    /// </summary>
    public string FileName
    {
      get { return HistList.Top; }
      set
      {
        HistList = HistList.Add(value);
      }
    }

    /// <summary>
    /// Путь к файлу по умолчанию.
    /// Если свойство установлено, и HistList не содержит ни одной строки, то
    /// при открытии диалога в поле будет введен этот путь. Также он будет присутствовать в списке
    /// истории
    /// </summary>
    public string DefaultFileName { get { return _DefaultFileName; } set { _DefaultFileName = value; } }
    private string _DefaultFileName;

    #endregion

    #region Проверка введенного пути

    /// <summary>
    /// Режим проверки введенного пути.
    /// Значение по умолчанию зависит от свойства Mode. При Mode=Write возвращает RootExists,
    /// а при false - DirectoryExists
    /// </summary>
    public TestPathMode PathValidateMode
    {
      get
      {
        if (_PathValidateMode.HasValue)
          return _PathValidateMode.Value;
        else
          return (Mode == FileDialogMode.Write) ? TestPathMode.RootExists : TestPathMode.FileExists;
      }
      set { _PathValidateMode = value; }
    }
    private TestPathMode? _PathValidateMode;

    /// <summary>
    /// Сбрасывает свойство PathValidateMode в значение по умолчанию
    /// </summary>
    public void ResetPathValidateMode()
    {
      _PathValidateMode = null;
    }

    #endregion

    #region Показ диалога

    private FileBrowseForm _TheForm;

    /// <summary>
    /// Показывает блок диалога
    /// </summary>
    /// <returns>Результат выполнения</returns>
    public DialogResult ShowDialog()
    {
      DialogResult res;
      _TheForm = new FileBrowseForm();
      try
      {
        _TheForm.Text = Title;
        _TheForm.Icon = EFPApp.MainImageIcon(ImageKey);
        _TheForm.grpMain.Text = "Файл";
        _TheForm.TextLabel.Text = "Путь к файлу";
        _TheForm.lblDescription.Text = Description;

        _TheForm.efpMainCB = new EFPHistComboBox(_TheForm.FormProvider, _TheForm.MainCB);
        _TheForm.efpMainCB.CanBeEmpty = false;
        _TheForm.efpMainCB.MaxHistLength = MaxHistLength;
        _TheForm.efpMainCB.HistList = HistList;
        if (!String.IsNullOrEmpty(DefaultFileName))
          _TheForm.efpMainCB.DefaultItems = new string[1] { DefaultFileName };
        _TheForm.efpMainCB.ToolTipText = "Поле ввода пути к файлу." + Environment.NewLine +
          "Чтобы выбрать ранее использованный путь, используйте выпадающий список." + Environment.NewLine +
          "Чтобы выбрать файл с помощью стандартного диалога Windows, нажмите кнопку \"Обзор\"";

        EFPFileDialogButton efpBrowse = new EFPFileDialogButton(_TheForm.efpMainCB, _TheForm.btnBrowse);
        efpBrowse.Title = "Выбор файла";
        efpBrowse.Filter = Filter;
        efpBrowse.Mode = Mode;
        efpBrowse.PathValidateMode = PathValidateMode;

        EFPWindowsExplorerButton efpExplorer = new EFPWindowsExplorerButton(_TheForm.efpMainCB, _TheForm.btnExplorer);
        efpExplorer.IsFileName = true;

        _TheForm.FormProvider.FormClosing += new FormClosingEventHandler(Form_Closing);

        res = EFPApp.ShowDialog(_TheForm, true);
        if (res == DialogResult.OK)
        {
          if (Mode == FileDialogMode.Write && File.Exists(_TheForm.efpMainCB.Text))
          {
            if (EFPApp.MessageBox("Файл \"" + _TheForm.efpMainCB.Text + "\" уже существует. Перезаписать его?",
              "Подтверждение", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
              return DialogResult.Cancel;
            FileTools.ForceDirs(new AbsPath(_TheForm.efpMainCB.Text).ParentDir);

            if (!String.IsNullOrEmpty(_TheForm.efpMainCB.Text))
              EFPApp.CurrentDirectory = new AbsPath(_TheForm.efpMainCB.Text).ParentDir;
          }
          HistList = _TheForm.efpMainCB.HistList;
        }
      }
      finally
      {
        _TheForm = null;
      }
      return res;
    }

    private void Form_Closing(object sender, FormClosingEventArgs args)
    {
      if (_TheForm.DialogResult != DialogResult.OK)
        return;
      _TheForm.efpMainCB.Validate();
      if (_TheForm.efpMainCB.ValidateState == UIValidateState.Error)
        return;
      if (Mode == FileDialogMode.Read)
      {
        if (!File.Exists(_TheForm.efpMainCB.Text))
        {
          EFPApp.ShowTempMessage("Файл не найден");
          _TheForm.DialogResult = DialogResult.Cancel;
          args.Cancel = true;
        }
      }
    }

    #endregion
  }
}