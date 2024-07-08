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
using FreeLibSet.Config;

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
  /// Базовый класс для <see cref="HistFolderBrowserDialog"/> и <see cref="HistFileBrowserDialog"/>
  /// </summary>
  public abstract class HistFileSystemBrowserDialogBase
  {
    #region Конструктор

    /// <summary>
    /// Инициализация значениями по умолчанию
    /// </summary>
    protected HistFileSystemBrowserDialogBase()
    {
      _MaxHistLength = HistoryList.DefaultMaxHistLength;
      _Mode = FileDialogMode.Read;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Заголовок блока диалога
    /// </summary>
    public string Title { get { return _Title ?? String.Empty; } set { _Title = value; } }
    private string _Title;

    /// <summary>
    /// Описание в нижней части диалога
    /// </summary>
    public string Description { get { return _Description ?? String.Empty; } set { _Description = value; } }
    private string _Description;

    /// <summary>
    /// Значок формы (изображение в <see cref="EFPApp.MainImages"/>)
    /// </summary>
    public string ImageKey { get { return _ImageKey; } set { _ImageKey = value; } }
    private string _ImageKey;

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
    /// Упрощенный доступ к выбранному файлу/каталогу.
    /// Установка свойства добавляет путь к списку.
    /// </summary>
    public abstract string SelectedPath { get; set; }

    /// <summary>
    /// Путь по умолчанию (по умолчанию - значение не задано).
    /// </summary>
    public abstract string DefaultPath { get; set; }


    /// <summary>
    /// <para>Имя секции конфигурации.</para> 
    /// <para>Если свойство установлено, то список <see cref="HistList"/> будет сохраняться в секции конфигурации
    /// категории <see cref="EFPConfigCategories.UserFiles"/> и восстанавливаться при следующем запуске.
    /// Для хранения настроек используется <see cref="EFPApp.ConfigManager"/>.
    /// По умолчанию - пустая строка - настройки не сохраняются.</para>
    /// </summary>
    public string ConfigSectionName
    {
      get { return _ConfigSectionName ?? String.Empty; }
      set { _ConfigSectionName = value; }
    }
    private string _ConfigSectionName;

    /// <summary>
    /// Предполагаемый режим открытия файла - на чтение или на запись.
    /// По умолчанию - <see cref="FileDialogMode.Read"/>.
    /// </summary>
    public FileDialogMode Mode { get { return _Mode; } set { _Mode = value; } }
    private FileDialogMode _Mode;

    /// <summary>
    /// Режим проверки введенного пути.
    /// </summary>
    public TestPathMode PathValidateMode
    {
      get
      {
        if (_PathValidateMode.HasValue)
          return _PathValidateMode.Value;
        else
          return DefaultPathValidateMode;
      }
      set { _PathValidateMode = value; }
    }
    private TestPathMode? _PathValidateMode;

    /// <summary>
    /// Значение свойства <see cref="PathValidateMode"/> по умолчанию
    /// </summary>
    protected abstract TestPathMode DefaultPathValidateMode { get; }

    /// <summary>
    /// Сбрасывает свойство <see cref="PathValidateMode"/> в значение по умолчанию.
    /// </summary>
    public void ResetPathValidateMode()
    {
      _PathValidateMode = null;
    }

    #endregion

    #region Показ диалога

    /// <summary>
    /// Показывает блок диалога.
    /// </summary>
    /// <returns>Результат работы</returns>
    public abstract DialogResult ShowDialog();

    #endregion
  }

  /// <summary>
  /// Диалог выбора каталога. Диалог содержит поле ввода пути и кнопку "Обзор".
  /// Имеет выпадающий список с ранее выбранными каталогами (история).
  /// Также может быть выведена кнопка "Включая подкаталоги".
  /// </summary>
  public sealed class HistFolderBrowserDialog : HistFileSystemBrowserDialogBase
  {
    #region Конструктор

    /// <summary>
    /// Инициализация диалога параметрами по умолчанию
    /// </summary>
    public HistFolderBrowserDialog()
    {
      Title = "Выбор папки";
      ImageKey = "Open";
    }

    #endregion

    #region Свойства

    /// <summary>
    /// <para>Имя секции конфигурации.</para> 
    /// <para>Если свойство установлено, то список <see cref="HistFileSystemBrowserDialogBase.HistList"/> будет сохраняться в секции конфигурации
    /// категории <see cref="EFPConfigCategories.UserFiles"/> и восстанавливаться при следующем запуске.
    /// Для хранения настроек используется <see cref="EFPApp.ConfigManager"/>.
    /// По умолчанию - пустая строка - настройки не сохраняются.</para>
    /// <para>Используется строкое значение "Directory".
    /// Также может сохраняться флажок <see cref="SubFolders"/> в поле "SubDirs" при <see cref="ShowSubFoldersButton"/>=true.</para>
    /// </summary>
    public new string ConfigSectionName
    {
      get { return base.ConfigSectionName; }
      set { base.ConfigSectionName = value; }
    }

    /// <summary>
    /// Упрощенный доступ к каталогу.
    /// Установка свойства добавляет каталог к списку.
    /// Текущий каталог (если не пустая строка) всегда заканчивается символом "\".
    /// </summary>
    public override string SelectedPath
    {
      get { return HistList.Top; }
      set
      {
        //AbsPath path = new AbsPath(value);
        RelPath path = new RelPath(value); // 26.06.2024
        HistList = HistList.Add(path.SlashedPath, MaxHistLength);
      }
    }

    /// <summary>
    /// Каталог по умолчанию (по умолчанию - не задано).
    /// Если свойство установлено, а список истории пустой, то при открытии диалога
    /// выбирается каталог по умолчанию. Каталог по умолчанию присутствует в 
    /// выпадающем списке, но не сохраняется в истории, если он был выбран в явном виде.
    /// </summary>
    public override string DefaultPath
    {
      get { return _DefaultPath; }
      set
      {
        //AbsPath path = new AbsPath(value);
        RelPath path = new RelPath(value); // 26.06.2024
        _DefaultPath = path.SlashedPath;
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
    /// свойство <see cref="ShowSubFoldersButton"/>=true.
    /// </summary>
    public bool SubFolders { get { return _SubFolders; } set { _SubFolders = value; } }
    private bool _SubFolders;


    /// <summary>
    /// Режим проверки введенного пути.
    /// Значение по умолчанию зависит от свойства <see cref="HistFileSystemBrowserDialogBase.Mode"/>. При <see cref="HistFileSystemBrowserDialogBase.Mode"/>=<see cref="FileDialogMode.Write"/> возвращает <see cref="TestPathMode.RootExists"/>,
    /// а при false - <see cref="TestPathMode.DirectoryExists"/>.
    /// </summary>
    public new TestPathMode PathValidateMode
    {
      get { return base.PathValidateMode; }
      set { base.PathValidateMode = value; }
    }

    /// <summary>
    /// Значение по умолчанию
    /// </summary>
    protected override TestPathMode DefaultPathValidateMode
    {
      get { return (Mode == FileDialogMode.Write) ? TestPathMode.RootExists : TestPathMode.DirectoryExists; }
    }

    #endregion

    #region Показ диалога

    private FileBrowseForm _TheForm;

    /// <summary>
    /// Показывает блок диалога.
    /// </summary>
    /// <returns>Результат работы</returns>
    public override DialogResult ShowDialog()
    {
      if (!String.IsNullOrEmpty(ConfigSectionName))
      {
        EFPConfigSectionInfo sectInfo = new EFPConfigSectionInfo(ConfigSectionName, EFPConfigCategories.UserFiles);
        CfgPart cfg;
        using (EFPApp.ConfigManager.GetConfig(sectInfo, EFPConfigMode.Read, out cfg))
        {
          HistoryList histList2 = cfg.GetHist("Directory");
          if (histList2.Count > 0)
            HistList = histList2;
          if (ShowSubFoldersButton)
            SubFolders = cfg.GetBoolDef("SubDirs", SubFolders);
        }
      }

      DialogResult res;
      _TheForm = new FileBrowseForm();
      try
      {
        _TheForm.Text = Title;
        _TheForm.Icon = EFPApp.MainImages.Icons[ImageKey];
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
          "Чтобы выбрать каталог с помощью стандартного диалога, нажмите кнопку \"Обзор\"";

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

          if (!String.IsNullOrEmpty(ConfigSectionName))
          {
            EFPConfigSectionInfo sectInfo = new EFPConfigSectionInfo(ConfigSectionName, EFPConfigCategories.UserFiles);
            CfgPart cfg;
            using (EFPApp.ConfigManager.GetConfig(sectInfo, EFPConfigMode.Write, out cfg))
            {
              cfg.SetHist("Directory", HistList);
              if (ShowSubFoldersButton)
                cfg.SetBool("SubDirs", SubFolders);
            }
          }
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
      AbsPath path = new AbsPath(_TheForm.efpMainCB.Text);
      _TheForm.efpMainCB.Text = path.SlashedPath;


      if (Mode == FileDialogMode.Write)
      {
        try
        {
          FileTools.ForceDirs(path);
        }
        catch (Exception e)
        {
          EFPApp.ShowTempMessage("Не удалось создать каталог. " + e.Message);
          _TheForm.DialogResult = DialogResult.Cancel;
          args.Cancel = true;
        }
      }
      // Убрано 25.06.2024
      //else
      //{
      //  if (!Directory.Exists(path.Path))
      //  {
      //    EFPApp.ShowTempMessage("Каталог не существует");
      //    _TheForm.DialogResult = DialogResult.Cancel;
      //    args.Cancel = true;
      //  }
      //}
    }

    #endregion
  }

  /// <summary>
  /// Диалог выбора файла. Диалог содержит поле ввода пути и кнопку "Обзор".
  /// Имеет выпадающий список с ранее выбранными файлами (история).
  /// </summary>
  public class HistFileBrowserDialog : HistFileSystemBrowserDialogBase
  {
    #region Конструктор

    /// <summary>
    /// Инициализация диалога параметрами по умолчанию
    /// </summary>
    public HistFileBrowserDialog()
    {
      Title = "Выбор файла";
      ImageKey = "Open";
    }

    #endregion

    #region Свойства

    /// <summary>
    /// <para>Имя секции конфигурации.</para> 
    /// <para>Если свойство установлено, то список <see cref="HistFileSystemBrowserDialogBase.HistList"/> будет сохраняться в секции конфигурации
    /// категории <see cref="EFPConfigCategories.UserFiles"/> и восстанавливаться при следующем запуске.
    /// Для хранения настроек используется <see cref="EFPApp.ConfigManager"/>.
    /// По умолчанию - пустая строка - настройки не сохраняются.</para>
    /// <para> Используется строковое значение "File"</para> 
    /// </summary>
    public new string ConfigSectionName
    {
      get { return base.ConfigSectionName; }
      set { base.ConfigSectionName = value; }
    }

    /// <summary>
    /// Фильтр для файлов в формате "Описание|Маска|...".
    /// См описание фильтра стандартных блоков диалога <see cref="System.Windows.Forms.FileDialog.Filter"/>.
    /// </summary>
    public string Filter { get { return _Filter; } set { _Filter = value; } }
    private string _Filter;

    /// <summary>
    /// Упрощенный доступ к имени выбранного файла (включая путь).
    /// Установка свойства добавляет список к истории.
    /// </summary>
    public override string SelectedPath
    {
      get { return HistList.Top; }
      set
      {
        //AbsPath path = new AbsPath(value);
        RelPath path = new RelPath(value); // 26.06.2024
        HistList = HistList.Add(path.Path, MaxHistLength);
      }
    }

    /// <summary>
    /// Дублирует свойство SelectedPath
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public string FileName { get { return SelectedPath; } set { SelectedPath = value; } }

    /// <summary>
    /// Путь к файлу по умолчанию.
    /// Если свойство установлено, и <see cref="HistFileSystemBrowserDialogBase.HistList"/> не содержит ни одной строки, то
    /// при открытии диалога в поле будет введен этот путь. Также он будет присутствовать в списке
    /// истории.
    /// </summary>
    public override string DefaultPath
    {
      get { return _DefaultPath; }
      set
      {
        RelPath path = new RelPath(value); 
        _DefaultPath = path.Path;
      }
    }
    private string _DefaultPath;

    /// <summary>
    /// Дублирует свойство DefaultPath
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public string DefaultFileName { get { return DefaultPath; } set { DefaultPath = value; } }

    /// <summary>
    /// Режим проверки введенного пути.
    /// Значение по умолчанию зависит от свойства <see cref="HistFileSystemBrowserDialogBase.Mode"/>. При <see cref="HistFileSystemBrowserDialogBase.Mode"/>=<see cref="FileDialogMode.Write"/> возвращает <see cref="TestPathMode.RootExists"/>,
    /// а при <see cref="FileDialogMode.Read"/> - <see cref="TestPathMode.FileExists"/>.
    /// </summary>
    public new TestPathMode PathValidateMode
    {
      get { return base.PathValidateMode; }
      set { base.PathValidateMode = value; }
    }

    /// <summary>
    /// Значение по умолчанию
    /// </summary>
    protected override TestPathMode DefaultPathValidateMode
    {
      get { return (Mode == FileDialogMode.Write) ? TestPathMode.RootExists : TestPathMode.FileExists; }
    }

    #endregion

    #region Показ диалога

    private FileBrowseForm _TheForm;

    /// <summary>
    /// Показывает блок диалога
    /// </summary>
    /// <returns>Результат выполнения</returns>
    public override DialogResult ShowDialog()
    {
      if (!String.IsNullOrEmpty(ConfigSectionName))
      {
        EFPConfigSectionInfo sectInfo = new EFPConfigSectionInfo(ConfigSectionName, EFPConfigCategories.UserFiles);
        CfgPart cfg;
        using (EFPApp.ConfigManager.GetConfig(sectInfo, EFPConfigMode.Read, out cfg))
        {
          HistoryList histList2 = cfg.GetHist("File");
          if (histList2.Count > 0)
            HistList = histList2;
        }
      }

      DialogResult res;
      _TheForm = new FileBrowseForm();
      try
      {
        _TheForm.Text = Title;
        _TheForm.Icon = EFPApp.MainImages.Icons[ImageKey];
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

          if (!String.IsNullOrEmpty(ConfigSectionName))
          {
            EFPConfigSectionInfo sectInfo = new EFPConfigSectionInfo(ConfigSectionName, EFPConfigCategories.UserFiles);
            CfgPart cfg;
            using (EFPApp.ConfigManager.GetConfig(sectInfo, EFPConfigMode.Write, out cfg))
            {
              cfg.SetHist("File", HistList);
            }
          }
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

      // Убрано 25.06.2024
      //if (Mode == FileDialogMode.Read)
      //{
      //  if (!File.Exists(_TheForm.efpMainCB.Text))
      //  {
      //    EFPApp.ShowTempMessage("Файл не найден");
      //    _TheForm.DialogResult = DialogResult.Cancel;
      //    args.Cancel = true;
      //  }
      //}
    }

    #endregion
  }
}
