// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.IO;
using FreeLibSet.Shell;
using System.Drawing;
using System.ComponentModel;
using FreeLibSet.Logging;
using FreeLibSet.Core;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Реализация команд "Открыть" и "Открыть с помощью" для локального меню управляющего элемента,
  /// который может сохранять файлы определенного вида.
  /// На момент создания управляющего элемента файл не обязан (но может) существовать.
  /// Вместо этого, файл может создаваться по требованию, если пользователь выбирает одну из команд.
  /// В общем случае, файл на диске может иметь другое расширение, чем переданное в конструкторе
  /// расширение. Например, можно в конструкторе задать расширение ".txt", а затем передать файл "myfile.log"
  /// </summary>
  public sealed class EFPFileAssociationsCommandItemsHandler
  {
    #region Конструктор

    /// <summary>
    /// Создает команды, относящиеся к файлу с заданным расширением.
    /// Если для операционной системы не реализованы файловые ассоциации, команды не добавляются.
    /// </summary>
    /// <param name="commandItems">Список для добавления команд</param>
    /// <param name="fileExt">Расширение файла, включая точку.
    /// Например, ".txt", ".xml". Должно быть задано обязательно.</param>
    public EFPFileAssociationsCommandItemsHandler(EFPCommandItems commandItems, string fileExt)
    {
      if (commandItems == null)
        throw new ArgumentNullException("commandItems");
      commandItems.CheckNotReadOnly();
      if (String.IsNullOrEmpty(fileExt))
        throw new ArgumentNullException("fileExt");
      if (fileExt[0] != '.')
        throw new ArgumentException(Res.EFPFileAssociationsCommandItemsHandler_Arg_MustStartWithDot, "fileExt");
      _FileExt = fileExt;

      #region Добавление команд

      _AllCommands = new List<EFPCommandItem>();

      try
      {
        FileAssociations faItems;
        if (EFPApp.FileExtAssociations.IsSupported)
          faItems = EFPApp.FileExtAssociations[fileExt];
        else
          faItems = FileAssociations.Empty;

        if (faItems.FirstItem != null)
        {
          EFPCommandItem ci = CreateCommandItem(faItems.FirstItem);
          ci.MenuText = Res.Cmd_Menu_File_FAOpen;
          ci.Usage = EFPCommandItemUsage.Menu | EFPCommandItemUsage.ToolBar;
          if (!ci.HasImage)
            ci.ImageKey = "UnknownState";
          commandItems.Add(ci);
          //CmdItems.DefaultCommandItem = ci;
          _AllCommands.Add(ci);
        }
        else
        {
          EFPCommandItem ci = new EFPCommandItem("File", "FAOpenNone");
          ci.MenuText = Res.Cmd_Menu_File_FAOpen;
          if (faItems.Exception == null)
          {
            ci.ToolTipText = String.Format(Res.Cmd_ToolTip_File_FAOpen_None, fileExt);
            ci.ImageKey = "UnknownState";
          }
          else
          {
            ci.ToolTipText = String.Format(Res.Cmd_ToolTip_File_FAOpen_Error, faItems.Exception.Message);
            ci.ImageKey = "Error";
          }
          ci.Usage = EFPCommandItemUsage.Menu | EFPCommandItemUsage.ToolBar;
          ci.Enabled = false;
          commandItems.Add(ci);
          _AllCommands.Add(ci);
        }

        EFPCommandItem smOpenWith = new EFPCommandItem("File", "FAOpenWith");
        smOpenWith.MenuText = Res.Cmd_Menu_File_FAOpenWith;
        smOpenWith.Usage = EFPCommandItemUsage.Menu | EFPCommandItemUsage.ToolBarDropDown;
        commandItems.Add(smOpenWith);
        _AllCommands.Add(smOpenWith);

        if (faItems.Count > 1)
        {
          for (int i = 1; i < faItems.Count; i++)
          {
            EFPCommandItem ci = CreateCommandItem(faItems[i]);
            ci.Parent = smOpenWith;
            ci.Usage = EFPCommandItemUsage.Menu; // в панели инструментов не надо
            commandItems.Add(ci);
            _AllCommands.Add(ci);
          }
        }
        else if (faItems.Exception == null)
        {
          EFPCommandItem ci = new EFPCommandItem("File", "FAOpenWithNone");
          ci.Parent = smOpenWith;
          ci.MenuText = Res.Cmd_Menu_File_FAOpenWith_None;
          ci.ImageKey = "UnknownState";
          ci.Usage = EFPCommandItemUsage.Menu; // в панели инструментов не надо
          ci.Enabled = false;
          commandItems.Add(ci);
          _AllCommands.Add(ci);
        }
        else
        {
          EFPCommandItem ci = new EFPCommandItem("File", "OpenWithError");
          ci.Parent = smOpenWith;
          ci.MenuText = Res.Cmd_Menu_File_FAOpenWith_Error;
          ci.ImageKey = "Error";
          ci.Usage = EFPCommandItemUsage.Menu; // в панели инструментов не надо
          ci.Enabled = true;
          ci.Tag = faItems.Exception;
          ci.Click += new EventHandler(ciOpenWithError_Click);
          commandItems.Add(ci);
          _AllCommands.Add(ci);
        }
      }
      catch (Exception e)
      {
        e.Data["FileExt"] = fileExt;
        e.Data["CommandItems"] = commandItems.ToString();

        // Исключение показываем один раз, дальше только выводим в log-файл

        if (_ExceptionShown)
          LogoutTools.LogoutException(e, Res.EFPFileAssociationsCommandItemsHandler_ErrTitle_Init);
        else
        {
          _ExceptionShown = false;
          EFPApp.ShowException(e, Res.EFPFileAssociationsCommandItemsHandler_ErrTitle_Init);
        }
      }

      #endregion
    }

    private static bool _ExceptionShown = false;

    #endregion

    #region Имя файла и расширение

    /// <summary>
    /// Расширение файла, включая точку.
    /// Задается в конструкторе.
    /// </summary>
    public string FileExt { get { return _FileExt; } }
    private readonly string _FileExt;

    /// <summary>
    /// Путь к существующему файлу на диске.
    /// Свойство может быть установлено либо до вывода элемента на экрав, если файл существует заранее,
    /// либо в обработчике события <see cref="FileNeeded"/>.
    /// </summary>
    public AbsPath FilePath
    {
      get { return _FilePath; }
      set { _FilePath = value; }
    }
    private AbsPath _FilePath;

    /// <summary>
    /// Событие вызывается перед выполнением любой из команд.
    /// Если свойство <see cref="FilePath"/> устанавливается до вывода управляющего элемента на экран, то обработчик не нужен.
    /// Обычно обработчик должен проверить <see cref="FilePath"/>.IsEmpty. Если свойство не было установлено, обработчик
    /// записывает (во временный каталог) файл и устанавливает свойство <see cref="FilePath"/>.
    /// Если после вызова обработчика свойство <see cref="FilePath"/> не установлено или файла нет на диске, выбрасывается
    /// исключение при выполнении команды.
    /// Обработчик может установить свойство <see cref="CancelEventArgs.Cancel"/>=true, чтобы предотвратить выполнение команды без выдачи исключения.
    /// В этом случае обязанность выдачи сообщения об ошибке лежит на обработчике события.
    /// </summary>
    public event CancelEventHandler FileNeeded;

    /// <summary>
    /// Подготовка файла.
    /// Вызывается перед выполнением любой команды.
    /// <para>1. Вызывает событие <see cref="FileNeeded"/>, если есть обработчик.</para>
    /// <para>2. Проверяет, что свойство <see cref="FilePath"/> установлено.</para>
    /// <para>3. Проверяет наличие файла на диске.</para>
    /// </summary>
    /// <returns>Возвращает false, если обработчик <see cref="FileNeeded"/> установил свойство <see cref="CancelEventArgs.Cancel"/></returns>
    public bool PrepareFile()
    {
      if (FileNeeded != null)
      {
        CancelEventArgs args = new CancelEventArgs();
        FileNeeded(this, args);
        if (args.Cancel)
          return false;
      }
      if (FilePath.IsEmpty)
        throw ExceptionFactory.ObjectPropertyNotSet(this, "FilePath");
      if (!System.IO.File.Exists(FilePath.Path))
        throw ExceptionFactory.FileNotFound(FilePath);

      return true;
    }

    #endregion

    #region Команды меню

    /// <summary>
    /// Все созданные команды меню
    /// </summary>
    private readonly List<EFPCommandItem> _AllCommands;

    private EFPCommandItem CreateCommandItem(FileAssociationItem fa)
    {
      EFPCommandItem ci = new EFPCommandItem("File", "FAOpen" + Guid.NewGuid().ToString());
      ci.MenuText = fa.DisplayName;
      ci.Tag = fa;
      ci.Click += OpenFile_Click;

      ci.Image = EFPApp.FileExtAssociations.GetIconImage(fa.IconPath, fa.IconIndex, true);
      if (ci.Image == null)
        ci.ImageKey = "EmptyImage"; // иначе не будет кнопки на панели инструментов

      ci.ToolTipText = String.Format(Res.Cmd_ToolTip_File_FAOpen, fa.DisplayName, fa.ProgramPath.FileName);

      return ci;
    }

    private void OpenFile_Click(object sender, EventArgs args)
    {
      EFPCommandItem ci = (EFPCommandItem)sender;
      FileAssociationItem fa = (FileAssociationItem)(ci.Tag);
      if (!PrepareFile())
        return;
      try
      {
        fa.Execute(FilePath);
      }
      catch (Exception e)
      {
        e.Data["FileAssociationItem"] = fa;
        e.Data["FilePath"] = FilePath;
        throw;
      }
    }

    void ciOpenWithError_Click(object sender, EventArgs args)
    {
      EFPCommandItem ci = (EFPCommandItem)sender;
      Exception ex = (Exception)(ci.Tag);
      EFPApp.ExceptionMessageBox(String.Format(Res.EFPFileAssociationsCommandItemsHandler_Err_ForFileExt, FileExt), 
        ex,
        EFPCommandItem.RemoveMnemonic(Res.Cmd_Menu_File_FAOpenWith));
    }

    /// <summary>
    /// Возвращает ссылку на основную команду меню "Открыть"
    /// </summary>
    public EFPCommandItem OpenCommandItem { get { return _AllCommands[0]; } }

    /// <summary>
    /// Возвращает ссылку на подменю "Открыть с помощью"
    /// </summary>
    public EFPCommandItem OpenWithSubMenu { get { return _AllCommands[1]; } }

    #endregion

    #region Свойство Visible

    /// <summary>
    /// Видимость команд отправки.
    /// По умолчанию равно true.
    /// </summary>
    public bool Visible
    {
      get
      {
        if (_AllCommands.Count == 0)
          return false; // Не может быть никогда
        else
          return _AllCommands[0].Visible;
      }
      set
      {
        for (int i = 0; i < _AllCommands.Count; i++)
          _AllCommands[i].Visible = value;
      }
    }

    #endregion

    #region Свойство Tag

    /// <summary>
    /// Произвольные пользовательские данные
    /// </summary>
    public object Tag { get { return _Tag; } set { _Tag = value; } }
    private object _Tag;

    #endregion
  }

  /// <summary>
  /// Комбоблок для выбора файловой ассоциации
  /// </summary>
  public class EFPFileAssociationsComboBox : EFPListComboBox
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер.
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPFileAssociationsComboBox(EFPBaseProvider baseProvider, System.Windows.Forms.ComboBox control)
      : base(baseProvider, control)
    {
      FileExt = String.Empty;
      ListControlImagePainter painter = new ListControlImagePainter(Control, ItemPainter);
      painter.IgnoreDisabledState = true;
    }

    #endregion

    #region Рисование элемента

    /// <summary>
    /// Элемент, добавляемый в комбоблок
    /// </summary>
    private class CBItem
    {
      #region Поля

      public FileAssociationItem FA;

      public Image Image;

      public bool IsDefault;

      public override string ToString()
      {
        if (FA == null)
          return String.Empty;
        return FA.DisplayName;
      }

      #endregion
    }

    private void ItemPainter(object sender, ListControlImageEventArgs args)
    {
      CBItem cbItem = args.Item as CBItem;
      if (cbItem == null)
        return;
      if (cbItem.IsDefault)
        args.Text += " (по умолчанию)";
      args.Image = cbItem.Image;
    }

    #endregion

    #region Свойство FileExt

    /// <summary>
    /// Расширение файла.
    /// В расширении должна быть задана ведущая точка.
    /// Установка свойства заполняет выпадающий список.
    /// </summary>
    public string FileExt
    {
      get { return _FileExt; }
      set
      {
        _FileExt = value;

        FreeLibSet.Shell.FileAssociations faItems;
        if (EFPApp.FileExtAssociations.IsSupported && (!String.IsNullOrEmpty(_FileExt)))
          faItems = EFPApp.FileExtAssociations[_FileExt];
        else
          faItems = FreeLibSet.Shell.FileAssociations.Empty;

        List<string> lstCodes = new List<string>();
        List<CBItem> lstItems = new List<CBItem>();

        int selIndex = -1;

        Control.BeginUpdate();
        try
        {
          for (int i = 0; i < faItems.Count; i++)
          {
            lstItems.Add(CreateCBItem(faItems[i], false));
            lstCodes.Add(faItems[i].DisplayName);
          }

          _FileAssociations = null;
          Control.Items.Clear();
          Control.Items.AddRange(lstItems.ToArray());
          base.Codes = lstCodes.ToArray();

          if (selIndex >= 0)
            Control.SelectedIndex = selIndex;
        }
        finally
        {
          Control.EndUpdate();
        }
      }
    }

    #region Свойство FileAssociations

    /// <summary>
    /// Список элементов для выбора в виде массива
    /// </summary>
    public FileAssociationItem[] FileAssociations
    {
      get { return _FileAssociations; }
      set
      {
        if (value == null)
          value = new FileAssociationItem[0];
        _FileAssociations = value;
        Control.BeginUpdate();
        try
        {
          CBItem[] cbItems = new CBItem[_FileAssociations.Length];
          string[] aCodes = new string[_FileAssociations.Length];

          for (int i = 0; i < _FileAssociations.Length; i++)
          {
            cbItems[i] = CreateCBItem(_FileAssociations[i], false);
            aCodes[i] = _FileAssociations[i].DisplayName;
          }

          Control.Items.Clear();
          Control.Items.AddRange(cbItems);
          base.Codes = aCodes;
        }
        finally
        {
          Control.EndUpdate();
        }
      }
    }
    private FileAssociationItem[] _FileAssociations;

    #endregion

    private static CBItem CreateCBItem(FileAssociationItem fa, bool isDefault)
    {
      CBItem cbItem = new CBItem();
      cbItem.FA = fa;
      cbItem.Image = EFPApp.FileExtAssociations.GetIconImage(fa.IconPath, fa.IconIndex, true);
      if (cbItem.Image == null)
        cbItem.Image = EFPApp.MainImages.Images["EmptyImage"];
      cbItem.IsDefault = isDefault;
      return cbItem;
    }

    private string _FileExt;

    #endregion
  }
}
