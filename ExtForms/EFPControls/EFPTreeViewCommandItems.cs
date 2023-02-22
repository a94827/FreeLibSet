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
  public abstract class EFPTreeViewCommandItemsBase : EFPControlCommandItems, IEFPClipboardCommandItems
  {
    #region Конструктор

    /// <summary>
    /// Инициализация
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    protected EFPTreeViewCommandItemsBase(IEFPTreeView controlProvider)
      :base((EFPControlBase)controlProvider)
    {
      if (controlProvider == null)
        throw new ArgumentNullException("controlProvider");
      _Owner = controlProvider;
    }

    /// <summary>
    /// Добавление команд в список
    /// </summary>
    protected void AddCommands()
    {
      #region Буфер обмена

      AddSeparator();

      ciCut = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Cut);
      ciCut.Click += new EventHandler(DoCut);
      Add(ciCut);

      ciCopy = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Copy);
      ciCopy.MenuText = "Копировать ячейки";
      ciCopy.Enabled = true;
      ciCopy.Click += new EventHandler(DoCopy);
      Add(ciCopy);

      /*
      if (EFPApp.ShowToolTips)
      {
        ciCopyToolTip = new EFPCommandItem("Правка", "КопироватьПодсказку");
        ciCopyToolTip.MenuText = "Копировать всплывающую подсказку";
        ciCopyToolTip.Click += new EventHandler(DoCopyToolTip);
        Add(ciCopyToolTip);
      } */
      AddSeparator();

      _PasteHandler = new EFPPasteHandler(this);

      #endregion

      #region Поиск

      AddSeparator();


      ciFind = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Find);
      ciFind.Click += new EventHandler(Find);
      Add(ciFind);

      ciIncSearch = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.IncSearch);
      ciIncSearch.Click += new EventHandler(IncSearch);
      ciIncSearch.StatusBarText = "??????????????????????";
      Add(ciIncSearch);

      ciFindNext = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.FindNext);
      ciFindNext.Click += new EventHandler(FindNext);
      Add(ciFindNext);

      AddSeparator();

      #endregion

      #region Установка отметок

      ciCheckAll = new EFPCommandItem("Edit", "SetAllCheckMarks");
      //ciCheckAll.Parent = MenuCheck;
      ciCheckAll.MenuText = "Установить отметки для всех строк";
      ciCheckAll.ImageKey = "CheckListAll";
      ciCheckAll.ShortCut = Keys.Control | Keys.A;
      ciCheckAll.Click += new EventHandler(ciCheckAll_Click);
      Add(ciCheckAll);

      ciUncheckAll = new EFPCommandItem("Edit", "DeleteAllCheckmarks");
      //ciUncheckAll.Parent = MenuCheck;
      ciUncheckAll.MenuText = "Снять отметки для всех строк";
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

    #region Переопределенные методы

    /// <summary>
    /// Инициализация свойств EFPCommandItem.Usage
    /// </summary>
    protected override void OnPrepare()
    {
      base.OnPrepare();

      EFPCommandItemUsage clipboardUsage = EFPCommandItemUsage.Menu | EFPCommandItemUsage.ShortCut;
      if (ClipboardInToolBar)
        clipboardUsage |= EFPCommandItemUsage.ToolBar;

      if (Cut == null)
      {
        ciCut.Enabled = false;
        ciCut.Usage = EFPCommandItemUsage.None;
      }
      else
        ciCut.Usage = clipboardUsage;

      ciCopy.Usage = clipboardUsage;


      // Добавляем форматы вставки текста после пользовательских форматов
      // (если уже не были добавлены явно)
      //AddTextPasteFormats();

      _PasteHandler.InitCommandUsage(ClipboardInToolBar);
      _PasteHandler.PasteApplied += new EventHandler(FPasteHandler_PasteApplied);


      ciIncSearch.Usage = EFPCommandItemUsage.None; // TODO: Команды поиска по первым буквам
      if (Owner.TextSearchContext == null)
      {
        ciFind.Usage = EFPCommandItemUsage.None;
        ciFindNext.Usage = EFPCommandItemUsage.None;
      }

      PerformRefreshItems();
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
      get { return _ClipboardInToolBar; } 
      set 
      {
        CheckNotReadOnly();
        _ClipboardInToolBar = value; 
      } 
    }
    private bool _ClipboardInToolBar;

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
      {
        Cut(this, EventArgs.Empty);

        PerformRefreshItems();
      }
    }

    #endregion

    #region Копировать

    private EFPCommandItem ciCopy;

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
      {
        string txt = new FreeLibSet.Text.TabTextConvert().ToString(a);
        dobj.SetData(DataFormats.Text, true, txt);
      }

      if ((copyFormats & EFPDataViewCopyFormats.CSV) == EFPDataViewCopyFormats.CSV)
      {
        string txt = new FreeLibSet.Text.CsvTextConvert().ToString(a);
        dobj.SetData(DataFormats.CommaSeparatedValue, txt);
      }
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
        EFPApp.BeginWait("Копирование ячеек в буфер обмена", "Copy");
        try
        {
          DataObject dobj2 = new DataObject();
          DataObjectEventArgs args = new DataObjectEventArgs(dobj2);

          OnAddDefaultCopyFormats(args);
          OnAddCopyFormats(args);

          EFPApp.Clipboard.SetDataObject(dobj2, true);
        }
        finally
        {
          EFPApp.EndWait();
        }
        return true;
      }
      catch (Exception e)
      {
        EFPApp.MessageBox(e.Message, "Ошибка при копировании в буфер обмена",
          MessageBoxButtons.OK, MessageBoxIcon.Error);
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

    void FPasteHandler_PasteApplied(object sender, EventArgs args)
    {
      //Owner.CurrentIncSearchColumn = null;
      PerformRefreshItems();
    }

    #endregion

    #endregion

    #region Команды поиска

    EFPCommandItem ciIncSearch, ciFind, ciFindNext;

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
      if (Owner.TextSearchContext!=null)
        ciFindNext.Enabled = Owner.TextSearchContext.ContinueEnabled;

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

    private EFPCommandItem ciCheckAll, ciUncheckAll;

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
      ciCheckAll.Visible = _Owner.CheckBoxes;
      ciUncheckAll.Visible = _Owner.CheckBoxes;
    }

    #endregion

    #region Обновление состояния команд

    /// <summary>
    /// Вызывается при изменении текущей позиции в управляющем элементе или
    /// при вызове PerformRefreshItems()
    /// </summary>
    public event EventHandler RefreshItems;

    /// <summary>
    /// Обновление доступности команд локального меню после внешнего изменения
    /// выбранных ячеек просмотра
    /// </summary>
    public void PerformRefreshItems()
    {
      if (Owner == null)
        return;

      // Вызываем виртуальный метод
      DoRefreshItems();
      // Посылаем извещения
      if (RefreshItems != null)
        RefreshItems(this, EventArgs.Empty);
    }

    /// <summary>
    /// Обновление видимости и доступности команд
    /// </summary>
    protected virtual void DoRefreshItems()
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
        AddDefaultCopyFormats(args.DataObject, a, CopyFormats);
      }
    }

    #endregion
  }
}
