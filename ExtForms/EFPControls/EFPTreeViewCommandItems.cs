// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace FreeLibSet.Forms
{

  /// <summary>
  /// Команды локального меню для TreeView и TreeViewAdv.
  /// Базовый класс для EFPTreeViewCommandItems и EFPTreeViewAdvCommandItemsBase
  /// </summary>
  public abstract class EFPTreeViewCommandItemsBase : EFPControlCommandItems, IEFPClipboardCommandItems, IEFPDataViewClipboardCommandItems
  {
    #region Конструктор

    /// <summary>
    /// Инициализация
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    protected EFPTreeViewCommandItemsBase(IEFPTreeView controlProvider)
      : base((EFPControlBase)controlProvider)
    {
      if (controlProvider == null)
        throw new ArgumentNullException("controlProvider");
      _Owner = controlProvider;

      Idle += CommandItems_Idle;
    }

    /// <summary>
    /// Добавление команд в список
    /// </summary>
    protected void AddCommands()
    {
      #region Буфер обмена

      MenuClipboard = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.MenuClipboard);
      Add(MenuClipboard);

      ciCut = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Cut);
      ciCut.Parent = MenuClipboard;
      ciCut.Click += new EventHandler(DoCut);
      Add(ciCut);

      ciCopy = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Copy);
      ciCopy.Parent = MenuClipboard;
      ciCopy.Enabled = true;
      ciCopy.Click += new EventHandler(DoCopy);
      Add(ciCopy);

      ciCopySettings = EFPDataViewCopyFormatsForm.AddCommandItem(this);
      ciCopySettings.Parent = MenuClipboard;

      /*
      if (EFPApp.ShowToolTips)
      {
        ciCopyToolTip = new EFPCommandItem("Правка", "КопироватьПодсказку");
        ciCopyToolTip.MenuText = "Копировать всплывающую подсказку";
        ciCopyToolTip.Click += new EventHandler(DoCopyToolTip);
        Add(ciCopyToolTip);
      } */
      AddSeparator();

      _PasteHandler = new EFPPasteHandler(this, MenuClipboard);
      _PasteHandler.UseToolBar = false; // по умолчанию - кнопки не нужны

      #endregion

      #region Поиск

      MenuSearch = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.MenuSearch);
      Add(MenuSearch);

      ciFind = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Find);
      ciFind.Parent = MenuSearch;
      ciFind.Click += new EventHandler(Find);
      Add(ciFind);

      ciIncSearch = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.IncSearch);
      ciIncSearch.Parent = MenuSearch;
      ciIncSearch.Click += new EventHandler(IncSearch);
      ciIncSearch.StatusBarText = "??????????????????????";
      Add(ciIncSearch);

      ciFindNext = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.FindNext);
      ciFindNext.Parent = MenuSearch;
      ciFindNext.Click += new EventHandler(FindNext);
      Add(ciFindNext);

      AddSeparator();

      #endregion

      #region Установка отметок

      MenuCheck = new EFPCommandItem("Edit", "MenuCheckMarks");
      MenuCheck.MenuText = Res.Cmd_Menu_CheckMarks;
      MenuCheck.ImageKey = "CheckListChecked";
      MenuCheck.Usage = EFPCommandItemUsage.Menu;
      Add(MenuCheck);


      ciCheckAll = new EFPCommandItem("Edit", "SetAllCheckMarks");
      ciCheckAll.Parent = MenuCheck;
      ciCheckAll.MenuText = Res.Cmd_Menu_CheckMarks_SetAll;
      ciCheckAll.ImageKey = "CheckListAll";
      ciCheckAll.ShortCut = Keys.Control | Keys.A;
      ciCheckAll.Click += new EventHandler(ciCheckAll_Click);
      Add(ciCheckAll);

      ciUncheckAll = new EFPCommandItem("Edit", "DeleteAllCheckMarks");
      ciUncheckAll.Parent = MenuCheck;
      ciUncheckAll.GroupEnd = true;
      ciUncheckAll.MenuText = Res.Cmd_Menu_CheckMarks_DelAll;
      ciUncheckAll.ImageKey = "CheckListNone";
      ciUncheckAll.ShortCut = Keys.Control | Keys.Shift | Keys.A;
      ciUncheckAll.Click += new EventHandler(ciUncheckAll_Click);
      Add(ciUncheckAll);

      AddSeparator();

      #endregion
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер управляющего элемента
    /// </summary>
    public IEFPTreeView Owner { get { return _Owner; } }
    private IEFPTreeView _Owner;

    #endregion

    #region OnPrepare()

    /// <summary>
    /// Инициализация свойств EFPCommandItem.Usage
    /// </summary>
    protected override void OnPrepare()
    {
      // Добавляем форматы вставки текста после пользовательских форматов
      // (если уже не были добавлены явно)
      //AddTextPasteFormats();

      base.OnPrepare();

      #region буфер обмена

      EFPCommandItemUsage clipboardUsage1 = EFPCommandItemUsage.Menu | EFPCommandItemUsage.ShortCut;
      if (ClipboardInToolBar)
        clipboardUsage1 |= EFPCommandItemUsage.ToolBar;
      EFPCommandItemUsage clipboardUsage2 = EFPCommandItemUsage.Menu | EFPCommandItemUsage.ShortCut;
      if (ClipboardInToolBar)
        clipboardUsage2 |= EFPCommandItemUsage.ToolBarAux;

      if (Cut == null)
      {
        ciCut.Enabled = false;
        ciCut.Usage = EFPCommandItemUsage.None;
      }
      else
        ciCut.Usage = clipboardUsage1;

      ciCopy.Usage = clipboardUsage1;

      ciCopySettings.Usage = clipboardUsage2;
      //ciCopyToolTip.Usage = clipboardUsage2;

      #endregion

      ciIncSearch.Usage = EFPCommandItemUsage.None; // TODO: Команды поиска по первым буквам
      if (Owner.TextSearchContext == null)
      {
        ciFind.Usage = EFPCommandItemUsage.None;
        ciFindNext.Usage = EFPCommandItemUsage.None;
      }
    }

    #endregion

    #region Буфер обмена


    /// <summary>
    /// Нужно ли показывать кнопки "Вырезать", "Копировать" и "Вставить" в панели
    /// инструментов (если она есть).
    /// По умолчанию - false (только в меню и горячие клавиши)
    /// </summary>
    public bool ClipboardInToolBar
    {
      get { return _PasteHandler.UseToolBar; }
      set { _PasteHandler.UseToolBar = value; }
    }

    private EFPCommandItem MenuClipboard;

    #region Вырезать

    private EFPCommandItem ciCut;

    /// <summary>
    /// Если обработчик установлен, то в локальное меню добавляется команда "Вырезать"
    /// Если обработчик не установлен, то поддерживается вырезка текста ячеек.
    /// При необходимости обработчик Cut может вызывать метод PerformCutText() или
    /// TryPerformCutText()
    /// </summary>
    public event EventHandler Cut;

    private void DoCut(object sender, EventArgs args)
    {
      if (Cut != null)
        Cut(this, EventArgs.Empty);
    }

    #endregion

    #region Копировать

    private EFPCommandItem ciCopy;

    internal EFPCommandItem ciCopySettings;

    private void DoCopy(object sender, EventArgs args)
    {
      //Owner.CurrentIncSearchColumn = null;
      PerformCopy();
    }


    /// <summary>
    /// Стандартные форматы копирования в буфер обмена
    /// По умолчанию: Text, CSV и HTML для TreeViewAdv и Text для TreeView.
    /// Можно отключить стандартные форматы копирования, если необходимо копировать данные в нестандартном формате.
    /// Тогда эти форматы можно добавить в обработчике AddCopyFormats
    /// </summary>
    public EFPDataViewCopyFormats CopyFormats
    {
      get { return _CopyFormats; }
      set
      {
        CheckNotReadOnly();
        _CopyFormats = value;
      }
    }
    private EFPDataViewCopyFormats _CopyFormats;

    /// <summary>
    /// Стандартные форматы с учетом выбранных пользователем
    /// </summary>
    public EFPDataViewCopyFormats SelectedCopyFormats { get { return CopyFormats & EFPDataViewCopyFormatsForm.UserSelectedFormats; } }


    /// <summary>
    /// Добавляет в буфер обмена текстовый формат для выбранных узлов
    /// </summary>
    /// <param name="args">Ссылка на DataObject</param>
    protected abstract void OnAddDefaultCopyFormats(DataObjectEventArgs args);

    /// <summary>
    /// Добавление форматов Text и CSV.
    /// Используется в реализации методов OnAddDefaultCopyFormats()
    /// </summary>
    /// <param name="dobj"></param>
    /// <param name="a"></param>
    /// <param name="copyFormats"></param>
    protected static void AddDefaultCopyFormats(IDataObject dobj, string[,] a, EFPDataViewCopyFormats copyFormats)
    {
      if ((copyFormats & EFPDataViewCopyFormats.Text) == EFPDataViewCopyFormats.Text)
        WinFormsTools.SetTextMatrixText(dobj, a);

      if ((copyFormats & EFPDataViewCopyFormats.CSV) == EFPDataViewCopyFormats.CSV)
        WinFormsTools.SetTextMatrixCsv(dobj, a);
    }


    /// <summary>
    /// Обработчик может добавить при копировании в буфер обмена дополнительные форматы
    /// </summary>
    public event DataObjectEventHandler AddCopyFormats;

    /// <summary>
    /// Вызывает событие AddCopyFormats.
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected virtual void OnAddCopyFormats(DataObjectEventArgs args)
    {
      if (AddCopyFormats != null)
        AddCopyFormats(this, args);
    }

    /// <summary>
    /// Выполнить копирование выделенных ячеек табличного просмотра в буфер обмена.
    /// В случае ошибки выдает сообщение на экран и возвращает false
    /// Метод может использоваться внутри реализации обработчика Cut
    /// </summary>
    /// <returns>true, если копирование успешно выполнено. false - в случае ошибки</returns>
    public bool PerformCopy()
    {
      try
      {
        EFPApp.BeginWait(Res.Clipboard_Phase_SetData, "Copy");
        try
        {
          DataObject dobj2 = new DataObject();
          DataObjectEventArgs args = new DataObjectEventArgs(dobj2);

          OnAddDefaultCopyFormats(args);
          OnAddCopyFormats(args);

          new EFPClipboard().SetDataObject(dobj2, true);
        }
        finally
        {
          EFPApp.EndWait();
        }
        return true;
      }
      catch (Exception e)
      {
        EFPApp.ErrorMessageBox(e.Message);
        return false;
      }
    }

    #endregion

    #region Вставить

    /// <summary>
    /// Обработчик для команд "Вставка" и "Специальная вставка"
    /// </summary>
    public EFPPasteHandler PasteHandler { get { return _PasteHandler; } }
    private EFPPasteHandler _PasteHandler;

    #endregion

    #endregion

    #region Команды поиска

    EFPCommandItem MenuSearch, ciIncSearch, ciFind, ciFindNext;

    private void IncSearch(object sender, EventArgs args)
    {
      /*
      // Начать / закончить поиск по первым буквам
      if (Owner.CurrentIncSearchColumn == null)
      {
        if (Owner.CurrentColumn == null)
        {
          EFPApp.ShowTempMessage("Столбец не выбран");
          return;
        }
        if (!Owner.CurrentColumn.CanIncSearch)
        {
          EFPApp.ShowTempMessage("Текущий столбец не поддерживает поиск по первым буквам");
          return;
        }
        Owner.CurrentIncSearchColumn = Owner.CurrentColumn;
      }
      else
      {
        Owner.CurrentIncSearchColumn = null;
      }
       * */
    }

    private void Find(object sender, EventArgs args)
    {
      Owner.TextSearchContext.StartSearch();
      RefreshSearchItems();
    }

    private void FindNext(object sender, EventArgs args)
    {

      //if (Owner.CurrentIncSearchColumn == null)
      //{
      if (Owner.TextSearchContext != null)
        Owner.TextSearchContext.ContinueSearch();
      //}
      //else
      //  if (!Owner.CurrentIncSearchColumn.PerformIncSearch(Owner.CurrentIncSearchChars.ToUpper(), true))
      //    EFPApp.ShowTempMessage("Нет больше строк, в которых значение поля начинается с \"" +
      //      Owner.CurrentIncSearchChars + "\"");
    }


    internal void RefreshSearchItems()
    {
      if (Owner.TextSearchContext != null)
      {
        ciFind.Enabled = Owner.HasNodes; // 17.06.2024
        ciFindNext.Enabled = Owner.HasNodes && Owner.TextSearchContext.ContinueEnabled;
      }
      /*
      if (ciIncSearch == null)
        return;

      bool Enabled;
      string MenuText;
      string StatusBarText;
      bool Checked = false;

      if (Owner.CurrentIncSearchColumn != null)
      {
        // Поиск по буквам выполняется
        Enabled = true;
        MenuText = "Закончить поиск по буквам";
        string s = Owner.CurrentIncSearchChars;
        s = s.Replace(' ', (char)(0x00B7));
        s = s.PadRight(20);
        StatusBarText = s.ToUpper();
        Checked = true;
      }
      else
      {
        // Поиск по буквам не выполняется
        if (Owner.CanIncSearch)
        {
          MenuText = "Начать поиск по буквам";
          if (Owner.CurrentColumn == null)
          {
            Enabled = false;
            StatusBarText = "<Столбец не выбран>";
          }
          else
          {
            Enabled = Owner.CurrentColumn.CanIncSearch;
            if (Enabled)
              StatusBarText = "<Поиск не начат>";
            else
              StatusBarText = "<Неподходящий столбец>";
          }
        }
        else
        {
          MenuText = "Поиск по буквам";
          Enabled = false;
          StatusBarText = "<Поиск невозможен>";
        }
      }

      ciIncSearch.MenuText = MenuText;
      ciIncSearch.Enabled = Enabled;
      ciIncSearch.StatusBarText = StatusBarText;
      ciIncSearch.Checked = Checked;
      ciIncSearch.ToolTipText = MenuText;

      if (Owner.CurrentIncSearchColumn == null)
        ciFindNext.Enabled = Owner.TextSearchContext.ContinueEnabled;
      else
        ciFindNext.Enabled = true;
      * */
    }

    #endregion

    #region Команды установки отметок

    private EFPCommandItem MenuCheck, ciCheckAll, ciUncheckAll;

    private void ciCheckAll_Click(object sender, EventArgs args)
    {
      _Owner.CheckAll(true);
    }

    private void ciUncheckAll_Click(object sender, EventArgs args)
    {
      _Owner.CheckAll(false);
    }

    private void RefreshCheckItems()
    {
      ciCheckAll.Visible = ciUncheckAll.Visible = _Owner.CheckBoxes;
      ciCheckAll.Enabled = ciUncheckAll.Enabled = _Owner.HasNodes; // 17.06.2024
    }

    #endregion

    #region Обновление состояния команд

    private void CommandItems_Idle(object sender, EventArgs args)
    {
      OnRefreshItems();
    }

    /// <summary>
    /// Обновление видимости и доступности команд
    /// </summary>
    protected virtual void OnRefreshItems()
    {
      RefreshSearchItems();
      RefreshCheckItems();
    }

    #endregion
  }

  /// <summary>
  /// Команды для просмотра EFPTreeView
  /// </summary>
  public class EFPTreeViewCommandItems : EFPTreeViewCommandItemsBase
  {
    #region Конструктор

    /// <summary>
    /// Инициализация
    /// </summary>
    /// <param name="owner">Провайдер управляющего элемента</param>
    public EFPTreeViewCommandItems(EFPTreeView owner)
      : base(owner)
    {
      base.CopyFormats = EFPDataViewCopyFormats.Text;
      AddCommands();
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайлер управляющего элемента
    /// </summary>
    public new EFPTreeView Owner { get { return (EFPTreeView)(base.Owner); } }

    #endregion

    #region Буфер обмена

    /// <summary>
    /// Добавляет в буфер обмена текстое представление для текущего узла.
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected override void OnAddDefaultCopyFormats(DataObjectEventArgs args)
    {
      if (Owner.Control.SelectedNode != null)
      {
        string[,] a = new string[1, 1];
        a[0, 0] = Owner.Control.SelectedNode.Text;
        AddDefaultCopyFormats(args.DataObject, a, SelectedCopyFormats);
      }
    }

    #endregion

    #region Обновление команд

    /// <summary>
    /// Установка доступности команд
    /// </summary>
    protected override void OnRefreshItems()
    {
      base.OnRefreshItems();
      this[EFPAppStdCommandItems.Copy].Enabled = Owner.Control.SelectedNode != null; // 17.06.2024
    }

    #endregion
  }
}
