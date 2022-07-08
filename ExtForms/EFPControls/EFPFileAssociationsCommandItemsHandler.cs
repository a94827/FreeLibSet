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
    /// Если для операционной системы не реализованы файловые ассоциации, команды не добавляются
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
        throw new ArgumentException("Расширение должно начинаться с точки", "fileExt");
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

        if (faItems.OpenItem != null)
        {
          EFPCommandItem ci = CreateCommandItem(faItems.OpenItem);
          ci.MenuText = "Открыть";
          ci.Usage = EFPCommandItemUsage.Menu | EFPCommandItemUsage.ToolBar;
          if (ci.HasImage)
            ci.ImageKey = "UnknownState";
          commandItems.Add(ci);
          //CmdItems.DefaultCommandItem = ci;
          _AllCommands.Add(ci);
        }
        else
        {
          EFPCommandItem ci = new EFPCommandItem("File", "OpenNowhere");
          ci.MenuText = "Открыть";
          if (faItems.Exception == null)
          {
            ci.ToolTipText = "Нет приложения, которое может открывать файлы с расширением \"" + fileExt + "\"";
            ci.ImageKey = "UnknownState";
          }
          else
          {
            ci.ToolTipText = "Возикла ошибка при получении файловых ассоциаций. " + faItems.Exception.Message;
            ci.ImageKey = "Error";
          }
          ci.Usage = EFPCommandItemUsage.Menu | EFPCommandItemUsage.ToolBar;
          ci.Enabled = false;
          commandItems.Add(ci);
          _AllCommands.Add(ci);
        }

        EFPCommandItem smOpenWith = new EFPCommandItem("File", "OpenWith");
        smOpenWith.MenuText = "Открыть с помощью";
        smOpenWith.Usage = EFPCommandItemUsage.Menu | EFPCommandItemUsage.ToolBarDropDown;
        commandItems.Add(smOpenWith);
        _AllCommands.Add(smOpenWith);

        if (faItems.OpenWithItems.Count > 0)
        {
          for (int i = 0; i < faItems.OpenWithItems.Count; i++)
          {
            EFPCommandItem ci = CreateCommandItem(faItems.OpenWithItems[i]);
            ci.Parent = smOpenWith;
            ci.Usage = EFPCommandItemUsage.Menu; // в панели инструментов не надо
            commandItems.Add(ci);
            _AllCommands.Add(ci);
          }
        }
        else if (faItems.Exception == null)
        {
          EFPCommandItem ci = new EFPCommandItem("File", "OpenWithNone");
          ci.Parent = smOpenWith;
          ci.MenuText = "[ Нет приложения ]";
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
          ci.MenuText = "[ Ошибка ]";
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

        string title = "Ошибка инициализации EFPFileAssociationsCommandItemsHandler";
        if (_ExceptionShown)
          LogoutTools.LogoutException(e, title);
        else
        {
          _ExceptionShown = false;
          EFPApp.ShowException(e, title);
        }
      }

      #endregion
    }

    private static bool _ExceptionShown = false;

    #endregion

    #region Имя файла и расширение

    /// <summary>
    /// Расширение файла, включая точку.
    /// Задается в конструкторе
    /// </summary>
    public string FileExt { get { return _FileExt; } }
    private string _FileExt;

    /// <summary>
    /// Путь к существующему файлу на диске.
    /// Свойство может быть установлено либо до вывода элемента на экрав, если файл существует заранее,
    /// либо в обработчике события FileNeeded
    /// </summary>
    public AbsPath FilePath
    {
      get { return _FilePath; }
      set { _FilePath = value; }
    }
    private AbsPath _FilePath;

    /// <summary>
    /// Событие вызывается перед выполнением любой из команд.
    /// Если свойство FilePath устанавливается до вывода управляющего элемента на экран, то обработчик не нужен.
    /// Обычно обработчик должен проверить FilePath.IsEmpty. Если свойство не было установлено, обработчик
    /// записывает (во временный каталог) файл и устанавливает свойство FilePath.
    /// Если после вызова обработчика свойство FilePath не установлено или файла нет на диске, выбрасывается
    /// исключение при выполнении команды.
    /// Обработчик может установить свойство Cancel=true, чтобы предотвратить выполнение команды без выдачи исключения.
    /// В этом случае обязанность выдачи сообщения об ошибке лежит на обработчике события.
    /// </summary>
    public event CancelEventHandler FileNeeded;

    /// <summary>
    /// Подготовка файла.
    /// Вызывается перед выполнением любой команды.
    /// 1. Вызывает событие FileNeeded, если есть обработчик.
    /// 2. Проверяет, что свойство FilePath установлено.
    /// 3. Проверяет наличие файла на диске
    /// </summary>
    /// <returns>Возвращает false, если обработчик FileNeeded установил свойство Cancel</returns>
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
        throw new NullReferenceException("Свойство FilePath не установлено");
      if (!System.IO.File.Exists(FilePath.Path))
        throw new System.IO.FileNotFoundException("Файл не существует", FilePath.Path);

      return true;
    }

    #endregion

    #region Команды меню

    /// <summary>
    /// Все созданные команды меню
    /// </summary>
    private List<EFPCommandItem> _AllCommands;

    private EFPCommandItem CreateCommandItem(FileAssociationItem fa)
    {
      EFPCommandItem ci = new EFPCommandItem("File", "Open" + Guid.NewGuid().ToString());
      ci.MenuText = fa.DisplayName;
      ci.Tag = fa;
      ci.Click += OpenFile_Click;

      ci.Image = EFPApp.FileExtAssociations.GetIconImage(fa.IconPath, fa.IconIndex, true);
      if (ci.Image == null)
        ci.ImageKey = "EmptyImage"; // иначе не будет кнопки на панели инструментов

      ci.ToolTipText = "Открыть файл с помощью приложения " + fa.DisplayName + " (" + fa.ProgramPath.FileName + ")";

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
      EFPApp.ExceptionMessageBox("Не удалось получить файловые ассоциации для расширения \"" + this.FileExt + "\"", ex,
        "Открыть с помощью");
    }

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
          return false;
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
}
