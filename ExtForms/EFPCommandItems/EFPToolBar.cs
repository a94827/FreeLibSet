// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Collections;
using FreeLibSet.Core;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Базовый класс для <see cref="EFPAppToolBar"/> и <see cref="EFPPanelToolBar"/>
  /// </summary>
  public abstract class EFPToolBarBase : EFPUIObjs
  {
    #region Конструктор и Dispose()

    /// <summary>
    /// Создает внутренний объект <see cref="ToolStrip"/>.
    /// </summary>
    protected EFPToolBarBase()
      : this(new ToolStrip())
    {
      // 20.05.2024 ImageList больше не используется
      // _Bar.ImageList = EFPApp.MainImages.ImageList;
#if DEBUG
      _Bar.Disposed += new EventHandler(Bar_Disposed);
      DisposableObject.RegisterDisposableObject(_Bar);
#endif
    }

    /// <summary>
    /// Дополнительный конструктор, использующий готовую панель
    /// </summary>
    protected EFPToolBarBase(ToolStrip bar)
    {
#if DEBUG
      if (bar == null)
        throw new ArgumentNullException("bar");
#endif
      _Bar = bar;
    }

#if DEBUG
    void Bar_Disposed(object sender, EventArgs args)
    {
      DisposableObject.UnregisterDisposableObject(sender);
    }
#endif

    /// <summary>
    /// Удаляет внутренний объект <see cref="ToolStrip"/>
    /// </summary>
    /// <param name="disposing">true, если был вызван метод <see cref="IDisposable.Dispose"/>Dispose() (всегда), а не деструктор</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        _Bar.Dispose();
        _Bar = null;
      }
      base.Dispose(disposing);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Свойство <see cref="Control.Name"/> для панели <see cref="ToolStrip"/> 
    /// </summary>
    public string Name
    {
      get
      {
        if (_Bar == null)
          return "Disposed";
        else
          return _Bar.Name;
      }
      set { _Bar.Name = value; }
    }

    #endregion

    #region Добавление элементов

    /// <summary>
    /// Добавляет кнопку для команды. 
    /// Свойство <see cref="EFPCommandItem.Usage"/> проверяется на наличие флага <see cref="EFPCommandItemUsage.ToolBarDropDown"/>, но не наличие флага <see cref="ToolBar"/>.
    /// Таким образом, кнопка создается в любом случае.
    /// </summary>
    /// <param name="item">Команда</param>
    public void Add(EFPCommandItem item)
    {
      if (item.GroupBegin)
        AddSeparator();
      if ((item.Usage & EFPCommandItemUsage.ToolBarAux) == EFPCommandItemUsage.ToolBarAux)
      {
        ToolStripDropDownButton tdd = null;
        EFPDropDownMenu cddm = null;
        if (Bar.Items.Count > 0)
          tdd = Bar.Items[Bar.Items.Count - 1] as ToolStripDropDownButton;
        if (tdd == null)
        {
          tdd = new ToolStripDropDownButton();
          cddm = new EFPDropDownMenu();
          cddm.Name = "DropDownAux_" + item.Name;
          Bar.Items.Add(tdd);
          //cddm.Attach(tdd);
        }
        else
          cddm = tdd.DropDown.Tag as EFPDropDownMenu;

        EFPUIObjBase uiObj = cddm.Add(item, true); // 03.10.2025
        //if (cddm.Count == 0)
        //  uiObj.DropDownMenu = cddm; // чтобы было разрушено
        uiObj.SetAll();
        if (item.HasChildren)
        {
          for (int i = 0; i < item.Children.Count; i++)
          {
            if (item.Children[i].MenuUsage)
              (uiObj.Owner as EFPMenuBase).Add(item.Children[i], null);
          }
        }
        cddm.Attach(tdd);
      }
      else if (item.HasChildren || (item.Usage & EFPCommandItemUsage.ToolBarDropDown) == EFPCommandItemUsage.ToolBarDropDown)
      {
        ToolStripDropDownButton tdd = new ToolStripDropDownButton();
        tdd.Tag = item;

        EFPDropDownMenu cddm = new EFPDropDownMenu();
        cddm.Name = "DropDownForPanel_" + item.Name;
        for (int i = 0; i < item.Children.Count; i++)
        {
          if (item.Children[i].MenuUsage)
            cddm.Add(item.Children[i], null);
        }
        cddm.Attach(tdd);
        Bar.Items.Add(tdd);
        EFPUIToolBarButtonObj uiObj = new EFPUIToolBarButtonObj(this, item, tdd);
        uiObj.DropDownMenu = cddm; // чтобы было разрушено
        uiObj.SetAll();
      }
      else
      {
        ToolStripButton tbb = new ToolStripButton();
        //tbb.Tag=Item;
        Bar.Items.Add(tbb);
        EFPUIToolBarButtonObj uiObj = new EFPUIToolBarButtonObj(this, item, tbb);
        uiObj.SetAll();
      }
      if (item.GroupEnd)
        AddSeparator();
    }

    /// <summary>
    /// Добавляет все команды из списка, для которых в <see cref="EFPCommandItem.Usage"/> задан флаг <see cref="EFPCommandItemUsage.ToolBar"/> или
    /// <see cref="EFPCommandItemUsage.ToolBarDropDown"/>. См. свойство <see cref="EFPCommandItem.ToolBarUsage"/>.
    /// </summary>
    /// <param name="items">Список команд</param>
    public void AddRange(EFPCommandItems items)
    {
      foreach (EFPCommandItem item in items)
      {
        if (item.Parent == null)
          AddOne(item);
      }
    }

    private bool AddOne(EFPCommandItem item)
    {
      if (item.HasChildren)
      {
        // Если есть дети, то обычно они отображаются в панели инструментов.
        // Но если ни один из них не хочет отображаться, то отображается 
        // родительская команда
        bool childrenInToolBar = false;
        for (int i = 0; i < item.Children.Count; i++)
        {
          if (AddOne(item.Children[i]))
            childrenInToolBar = true;
        }
        if (childrenInToolBar)
          return true;
      }
      if (!item.ToolBarUsage)
        return false;
      Add(item);
      return true;
    }

    /// <summary>
    /// Добавляет все команды из списка, для которых в в <see cref="EFPCommandItem.Usage"/> задан флаг <see cref="EFPCommandItemUsage.ToolBar"/> или
    /// <see cref="EFPCommandItemUsage.ToolBarDropDown"/>. См. свойство <see cref="EFPCommandItem.ToolBarUsage"/>.
    /// </summary>
    /// <param name="items">Список команд</param>
    public void AddRange(List<EFPCommandItem> items)
    {
      for (int i = 0; i < items.Count; i++)
      {
        if (items[i].ToolBarUsage)
          Add(items[i]);
      }
    }

    #endregion

    #region Внутренняя реализация

    /// <summary>
    /// Обслуживаемая панель с кнопками.
    /// Создается в конструкторе.
    /// </summary>
    public ToolStrip Bar { get { return _Bar; } }
    private ToolStrip _Bar;

#if DEBUG

    /// <summary>
    /// Для отладки
    /// </summary>
    internal string[] DebugCommandCodes
    {
      get
      {
        if (_Bar == null)
          return EmptyArray<string>.Empty;

        string[] a = new string[_Bar.Items.Count];
        for (int i = 0; i < _Bar.Items.Count; i++)
        {
          EFPCommandItem ci = _Bar.Items[i].Tag as EFPCommandItem;
          if (ci != null)
            a[i] = ((IObjectWithCode)ci).Code;
        }
        return a;
      }
    }

#endif

    private void AddSeparator()
    {
      if (Bar.Items.Count < 1)
        return;
      if (Bar.Items[Bar.Items.Count - 1] is ToolStripSeparator)
        return;
      ToolStripSeparator sep = new ToolStripSeparator();
      Bar.Items.Add(sep);
    }

    /// <summary>
    /// Возвращает отладочную информацию о панели
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return (String.IsNullOrEmpty(Name) ? "Noname" : Name) + ". ItemCount=" + Count.ToString();
    }

    /// <summary>
    /// Установка видимости разделителей между кнопками
    /// </summary>
    protected void InitSeparatorVisibility()
    {
      EFPMenuBase.InitSeparatorVisiblityRecurse(Bar.Items, null);
    }

    #endregion

    private class EFPUIToolBarButtonObj : EFPUIObjBase
    {
      #region Конструктор

      public EFPUIToolBarButtonObj(EFPToolBarBase owner, EFPCommandItem item, ToolStripItem button)
        : base(owner, item)
      {
        _Button = button;
        _Button.Tag = item;
        if (_Button is ToolStripButton)
          _Button.Click += new EventHandler(ClickEvent);
      }

      protected override void Dispose(bool disposing)
      {
        if (disposing)
        {
          if (DropDownMenu != null)
          {
            DropDownMenu.Dispose();
            DropDownMenu = null;
          }
        }
        base.Dispose(disposing);
      }

      #endregion

      #region Свойства

      public new EFPToolBarBase Owner
      {
        get { return (EFPToolBarBase)(base.Owner); }
      }

      private readonly ToolStripItem _Button;

      /// <summary>
      /// Присоединенное выпадающее меню. Его может потребоваться разрушить
      /// </summary>
      internal EFPDropDownMenu DropDownMenu;

      #endregion

      #region Переопределенные методы

      public override void SetVisible()
      {
        _Button.Visible = Item.Visible;

        if (_Button.Owner == null)
          EFPMenuBase.InitSeparatorVisiblity(Owner.Bar.Items, null);
        else
          EFPMenuBase.InitSeparatorVisiblity(_Button.Owner.Items, null);
      }

      public override void SetEnabled()
      {
        _Button.Enabled = Item.Enabled && (!Item.InsideClick);
      }

      public override void SetChecked()
      {
        if (_Button is ToolStripButton)
          ((ToolStripButton)_Button).Checked = Item.Checked;
      }

      public override void SetImage()
      {
        if ((Item.Usage & EFPCommandItemUsage.ToolBarDropDown) == EFPCommandItemUsage.ToolBarDropDown)
          return;

        if (Item.Image == null)
        {
          // _Button.ImageKey = Item.ImageKey;

          // 20.05.2024.
          // В Net6 свойство ImageKey не работает, если панель расположена в модальном окне
          if (String.IsNullOrEmpty(Item.ImageKey))
            _Button.Image = null;
          else
            _Button.Image = EFPApp.MainImages.Images[Item.ImageKey];
        }
        else
          _Button.Image = Item.Image;
      }

      public override void SetToolTipText()
      {
        string s = null;
        if (String.IsNullOrEmpty(Item.ToolTipText))
        {
          if (!(String.IsNullOrEmpty(Item.MenuText)))
            s = Item.MenuTextWithoutMnemonic;
        }
        else
          s = _Button.ToolTipText = Item.ToolTipText;

        if ((!String.IsNullOrEmpty(s)) && ((Item.Usage & EFPCommandItemUsage.DisableRightTextInToolTip) == EFPCommandItemUsage.None))
        {
          if (!String.IsNullOrEmpty(Item.MenuRightText))
            s += " (" + Item.MenuRightText + ")";
          else
          {
            if (Item.ShortCut != Keys.None)
              s += " (" + Item.ShortCutText + ")";
          }
        }
        _Button.ToolTipText = s;
      }

      public override void SetMenuText()
      {
        SetToolTipText();
      }

      #endregion
    }
  }

  /// <summary>
  /// Панель инструментов, относящаяся к главному окну приложения.
  /// </summary>
  public class EFPAppToolBar : EFPToolBarBase, IObjectWithCode
  {
    #region Конструктор

    /// <summary>
    /// Создает панель инструментов
    /// </summary>
    /// <param name="name">Имя для сохранения параметров в секции конфигурации</param>
    public EFPAppToolBar(string name)
    {
      if (String.IsNullOrEmpty(name))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("name");

      base.Name = name;
      _DisplayName = null;
      _Dock = DockStyle.Top;
      _UseLocation = false;
      _RowIndex = 0;
      _Location = new Point(0, 0);
    }

    /// <summary>
    /// Создает панель инструментов
    /// </summary>
    /// <param name="name">Имя для сохранения параметров в секции конфигурации</param>
    /// <param name="bar">Готовая панель инструментов</param>
    internal EFPAppToolBar(string name, ToolStrip bar)
      : base(bar)
    {
      if (String.IsNullOrEmpty(name))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("name");

      base.Name = name;
      _DisplayName = null;
      _Dock = DockStyle.Top;
      _UseLocation = false;
      _RowIndex = 0;
      _Location = new Point(0, 0);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя панели инструментов, предъявляемое пользователю
    /// </summary>
    public string DisplayName
    {
      get
      {
        if (String.IsNullOrEmpty(_DisplayName))
          return Name;
        else
          return _DisplayName;
      }
      set
      {
        _DisplayName = value;
      }
    }
    private string _DisplayName;

    /// <summary>
    /// Видимость панели инструментов
    /// </summary>
    public bool Visible
    {
      get { return Bar.Visible; }
      set
      {
        if (value == Bar.Visible)
          return;
        Bar.Visible = value;
        if (VisibleChanged != null)
          VisibleChanged(this, EventArgs.Empty);
        if (!EFPApp.InsideLoadComposition)
          EFPApp.SetInterfaceChanged(); // 11.12.2018
      }
    }

    /// <summary>
    /// Куда должна быть присоединена панель
    /// Изменение свойства приводит к переключению панели
    /// </summary>
    public DockStyle Dock
    {
      get
      {
        if (Bar.Parent == null)
          return _Dock;
        else
          return Bar.Parent.Dock;
      }
      set
      {
        switch (value)
        {
          case DockStyle.Top:
          case DockStyle.Bottom:
          case DockStyle.Left:
          case DockStyle.Right:
            break;
          default:
            throw new ArgumentException();
        }

        _Dock = value;
        if (Bar.Parent != null)
        {
          if (Bar.Parent.Dock != value)
            // Переприсоединяем панель
            DoJoin();
        }
      }
    }
    private DockStyle _Dock; // используется до присоединения

    /// <summary>
    /// Индекс строки, в которой будет размещена панель
    /// </summary>
    public int RowIndex
    {
      get
      {
        if (Bar.Parent != null)
        {
          ToolStripPanel panel = (ToolStripPanel)(Bar.Parent);
          for (int i = 0; i < panel.Rows.Length; i++)
          {
            foreach (Control ctrl in panel.Rows[i].Controls)
            {
              if (ctrl == Bar)
                return i;
            }
          }
          return 0;
        }
        else
          return _RowIndex;
      }
      set
      {
        _UseLocation = false;
        _RowIndex = value;
        if (Bar.Parent != null && value != RowIndex)
          DoJoin();
      }
    }
    private int _RowIndex; // используется до присоединения

    /// <summary>
    /// Позиция панели в координатах главного окна.
    /// </summary>
    public Point Location
    {
      get
      {
        if (Bar.Parent != null)
          return Bar.Location;
        else
          return _Location;
      }
      set
      {
        _UseLocation = true;
        _Location = value;
        if (Bar.Parent != null && value != _Location)
          DoJoin();
      }
    }
    internal Point _Location; // используется до присоединения

    /// <summary>
    /// true, если последним устанавливалось свойство Location, false-RowIndex
    /// </summary>
    internal bool UseLocation { get { return _UseLocation; } }
    private bool _UseLocation;

    #endregion

    #region События

    /// <summary>
    /// Вызывается при изменении видимости панели инструментов.
    /// Используется для обновления команды меню видимости.
    /// </summary>
    public event EventHandler VisibleChanged;

    #endregion

    #region Методы

    /// <summary>
    /// Установка значений параметров просмотра по умолчанию.
    /// Устанавливается Visible=true, панель помещается в верхней части окна.
    /// </summary>
    public void Reset()
    {
      _RowIndex = 0;
      _Location = new Point(0, 0);
      _UseLocation = false;
      _Dock = DockStyle.Top;
      Visible = true;
    }

    internal FormToolStripInfo Info
    {
      get
      {
        //if (FInfo == null)
        //{
        //  if (EFPApp.MainWindow is EFPMainMDIForm)
        //    FInfo = new FormToolStripInfo(EFPApp.MainWindow);
        //}
        return _Info;
      }
      set { _Info = value; }
    }
    FormToolStripInfo _Info;

    internal void DoJoin()
    {
      ToolStripPanel panel;
      switch (_Dock)
      {
        case DockStyle.Top: panel = Info.StripPanelTop; break;
        case DockStyle.Left: panel = Info.StripPanelLeft; break;
        case DockStyle.Right: panel = Info.StripPanelRight; break;
        case DockStyle.Bottom: panel = Info.StripPanelBottom; break;
        default:
          throw new BugException("Unknown toolBar placement. Dock=" + _Dock.ToString());
      }

      if (_UseLocation)
        panel.Join(Bar, _Location);
      else
        panel.Join(Bar, _RowIndex);

      if (!EFPApp.InsideLoadComposition)
        EFPApp.SetInterfaceChanged(); // 11.12.2018
    }

    /// <summary>
    /// Создание команды, выполняющей переключение видимости панели инструментов
    /// </summary>
    /// <param name="parentItem"></param>
    /// <returns></returns>
    public EFPCommandItem CreateVisibleCommandItem(EFPCommandItem parentItem)
    {
      EFPCommandItem ci = new VisibleCommandItem(this);
      ci.Parent = parentItem;
      return ci;
    }

    private class VisibleCommandItem : EFPCommandItem
    {
      public VisibleCommandItem(EFPAppToolBar ToolBar)
        : base("MainToolBarVisible", ToolBar.Name)
      {
        _ToolBar = ToolBar;
        MenuText = _ToolBar.DisplayName;
        Checked = Visible;
        Click += new EventHandler(VisibleClick);
        _ToolBar.VisibleChanged += new EventHandler(ToolBarVisibleChanged);
      }

      public override EFPCommandItem Clone()
      {
        return new VisibleCommandItem(_ToolBar);
      }

      void ToolBarVisibleChanged(object sender, EventArgs args)
      {
        Checked = _ToolBar.Visible;
      }

      private readonly EFPAppToolBar _ToolBar;

      private void VisibleClick(object sender, EventArgs args)
      {
        _ToolBar.Visible = !_ToolBar.Visible;
      }
    }

    /// <summary>
    /// Возвращает отладочную информацию
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append(base.ToString());
      sb.Append(", Dock=");
      sb.Append(Dock.ToString());
      if (_UseLocation)
      {
        sb.Append(", Location=");
        sb.Append(Location.ToString());
      }
      else
      {
        sb.Append(", RowIndex=");
        sb.Append(RowIndex.ToString());
      }
      return sb.ToString();
    }

    /// <summary>
    /// Завершение инициализации
    /// Удаляет лишние сепаратора
    /// </summary>
    public void Attach()
    {
      base.InitSeparatorVisibility();
    }

    #endregion

    #region IObjectWithCode Members

    string IObjectWithCode.Code { get { return Name; } }

    #endregion
  }

  /// <summary>
  /// Свойство <see cref="EFPAppMainWindowLayout.ToolBars"/>.
  /// Также устаревшее свойство <see cref="EFPApp.AppToolBars"/>.
  /// </summary>
  public class EFPAppToolBars : NamedList<EFPAppToolBar>
  {
    #region Конструктор

    /// <summary>
    /// Создает пустой список панелей
    /// </summary>
    public EFPAppToolBars()
    {
      _ContextMenu = new EFPContextMenu();
    }

    #endregion

    #region Методы

    /// <summary>
    /// Добавляет панель в список
    /// </summary>
    /// <param name="toolBar">Добавляемая панель</param>
    public new void Add(EFPAppToolBar toolBar)
    {
      base.Add(toolBar);
      EFPCommandItem ci = toolBar.CreateVisibleCommandItem(null);
      ContextMenu.Add(ci);
    }

    /// <summary>
    /// Присоединение всех панелей к главному окну программы.
    /// 
    /// Этот метод является устаревшим. Следует использовать свойство <see cref="EFPApp.Interface"/>, где главное меню,
    /// панели инструментов и статусная строка присоединяется автоматически.
    /// </summary>
    public void Attach()
    {
      // Отцепляем все объектв
      foreach (EFPAppToolBar toolBar in this)
        toolBar.Bar.Parent = null;

      // Сортируем список панелей
      EFPAppToolBar[] a = base.ToArray();
      if (a.Length == 0)
        return;
      Array.Sort<EFPAppToolBar>(a, new Comparison<EFPAppToolBar>(MyCmp));

      // Присоединяем панели

      // На время присоединения выключаем "хваталку", чтобы кнопки меньше прыгали
      for (int i = 0; i < a.Length; i++)
      {
        GetReadyInfo(a[i]);

        a[i].Bar.GripStyle = ToolStripGripStyle.Hidden;
        a[i].DoJoin();
      }

      // Если кнопки упрыгали - загоняем их на место в обратном порядке
      for (int i = a.Length - 1; i >= 0; i--)
      {
        if (a[i].UseLocation)
          a[i].Bar.Location = a[i]._Location;
        a[i].Bar.GripStyle = ToolStripGripStyle.Visible;
      }

      // При первом вызове - инициализируем контекстные меню
      if (a[0].Info.StripPanelTop.ContextMenuStrip == null)
        AttachContextMenu(a[0].Info);
    }

    /// <summary>
    /// Обеспечение совместимости со старыми приложениями
    /// </summary>
    /// <param name="toolBar"></param>
    private static void GetReadyInfo(EFPAppToolBar toolBar)
    {
      if (toolBar.Info != null)
        return;

      EFPAppInterfaceMDI oldMDI = EFPApp.Interface as EFPAppInterfaceMDI;
      if (oldMDI != null)
      {
        if (oldMDI.ObsoleteMode)
        {
          toolBar.Info = new FormToolStripInfo(oldMDI.CurrentMainWindowLayout.MainWindow);
          return;
        }
      }

      throw new NullReferenceException("Main menu not set");
    }

    private int MyCmp(EFPAppToolBar b1, EFPAppToolBar b2)
    {
      // Порядок сортировки:
      // 1-по номеру строки
      // 2-по координатам
      // 3-по порядку добавления панелей в список

      // 1
      int rowIndex1 = b1.RowIndex;
      int rowIndex2 = b2.RowIndex;
      if (rowIndex1 < rowIndex2)
        return -1;
      if (rowIndex1 > rowIndex2)
        return 1;

      // 2
      int coord1, coord2;
      if (b1.Dock == DockStyle.Left || b1.Dock == DockStyle.Right)
        coord1 = b1.Location.Y;
      else
        coord1 = b1.Location.X;
      if (b2.Dock == DockStyle.Left || b2.Dock == DockStyle.Right)
        coord2 = b2.Location.Y;
      else
        coord2 = b2.Location.X;

      if (coord1 < coord2)
        return 1;
      if (coord1 > coord2)
        return -1;

      // 3
      int barIndex1 = base.IndexOf(b1);
      int barIndex2 = base.IndexOf(b2);
      if (barIndex1 > barIndex2)
        return -1;
      else
        return 1;
    }

    #endregion

    #region Локальное меню для панелей

    /// <summary>
    /// Локальное меню для панелей
    /// </summary>
    public EFPContextMenu ContextMenu { get { return _ContextMenu; } }
    private readonly EFPContextMenu _ContextMenu;

    private void AttachContextMenu(FormToolStripInfo info)
    {
      ContextMenu.Attach(info.StripPanelTop);
      info.StripPanelLeft.ContextMenuStrip = info.StripPanelTop.ContextMenuStrip;
      info.StripPanelRight.ContextMenuStrip = info.StripPanelTop.ContextMenuStrip;
      info.StripPanelBottom.ContextMenuStrip = info.StripPanelTop.ContextMenuStrip;
    }

    #endregion
  }

#if XXXXX
  /// <summary>
  /// Панель инструментов вверху формы
  /// </summary>
  public class ClientFormToolBar : ClientToolBarBase
  {
    public ClientFormToolBar()
      : base()
    {
    }

    public void Attach(Form Form)
    {
      Size sz = Form.Size;
      Form.Size = new Size(sz.Width, sz.Height + Bar.Size.Height);
      Form.Controls.Add(Bar);
      Form.Disposed += new EventHandler(FormDisposed);
    }

    private void FormDisposed(object sender, EventArgs args)
    {
      Dispose();
    }
  }
#endif


  /// <summary>
  /// Панель инструментов по месту. Предполагается, что ей выделена панель Panel
  /// </summary>
  public class EFPPanelToolBar : EFPToolBarBase
  {
    #region Конструктор

    /// <summary>
    /// Создает локальную панель инструментов.
    /// Созданный объект <see cref="ToolStrip"/> пока никуда не присоединен.
    /// </summary>
    public EFPPanelToolBar()
    {
      Bar.GripStyle = ToolStripGripStyle.Hidden;

      UseLocalMenu = true;
      UseVisibleCommand = false;
    }

    #endregion

    #region Присоединение к панели

    /// <summary>
    /// Присоединяет <see cref="ToolStrip"/> к заданной панели
    /// </summary>
    /// <param name="userPanel">Панель для добавления <see cref="ToolStrip"/></param>
    public void Attach(Panel userPanel)
    {
      //Bar.AutoSize = true;
      //FPanel.Dock = DockStyle.Fill;
      userPanel.Size = new Size(Bar.Size.Width, Bar.Size.Height);
      userPanel.Controls.Add(/*FPanel*/Bar);
      userPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
      userPanel.AutoSize = true;
      userPanel.Disposed += new EventHandler(PanelDisposed);
      userPanel.DockChanged += new EventHandler(UserPanel_DockChanged);
      UserPanel_DockChanged(null, null);

      base.InitSeparatorVisibility();

      // Команды управления видимостью и положением
      if (UseLocalMenu && ToolBarControllable)
      {
        _PanelCommandItems = new DockCommandItems(this);

        EFPContextMenu ccm = new EFPContextMenu();
        ccm.AddRange(PanelCommandItems);
        ccm.Attach(userPanel);
      }
    }

    void UserPanel_DockChanged(object sender, EventArgs args)
    {
      Bar.Dock = Bar.Parent.Dock;
    }

    private void PanelDisposed(object sender, EventArgs args)
    {
      Dispose();
    }

    #endregion

    #region Свойства положения и видимости панели

    /// <summary>
    /// Если true (по умолчанию), то разрешается наличие 
    /// локального меню с командами перемещения
    /// </summary>
    public bool UseLocalMenu { get { return _UseLocalMenu; } set { _UseLocalMenu = value; } }
    private bool _UseLocalMenu;

    /// <summary>
    /// Если true, то в локальном меню есть команда "Панель включена".
    /// По умолчанию - false
    /// </summary>
    public bool UseVisibleCommand { get { return _UseVisibleCommand; } set { _UseVisibleCommand = value; } }
    private bool _UseVisibleCommand;

    /// <summary>
    /// Возвращает true, если панель и основной управляющий элемент располагаются в родительском
    /// элементе так, что можно безопасно управлять расположением панели инструментов (сверху,
    /// сбоку, ...) относительно основного элемента.
    /// </summary>
    public bool ToolBarControllable
    {
      get
      {
        Control userPanel = Bar.Parent;
        if (userPanel == null)
          return false;

        //        if (UserPanel.Parent.Controls.IndexOf(UserPanel) != 0)
        //          return false;
        if (userPanel.Parent.Controls.Count != 2)
          return false;
        int panelIndex = userPanel.Parent.Controls.IndexOf(userPanel);
        int mainControlIndex = 1 - panelIndex;
        if (userPanel.Parent.Controls[mainControlIndex].Dock != DockStyle.Fill)
          return false;
        return true;
      }
    }

    /// <summary>
    /// Управляет видимостью панели
    /// </summary>
    public bool ToolBarVisible
    {
      get
      {
        if (Bar.Parent == null)
          return false;
        else
          return Bar.Parent.Visible;
      }
      set
      {
        Bar.Parent.Visible = value;
      }
    }

    /// <summary>
    /// Управляет расположением панели относительно основного элемента.
    /// Это свойство можно устанавливать, только если <see cref="ToolBarControllable"/> возвращает true.
    /// </summary>
    public TabAlignment ToolBarAlignment
    {
      get
      {
        if (Bar.Parent == null)
          return TabAlignment.Top;
        else
        {
          switch (Bar.Parent.Dock)
          {
            case DockStyle.Left: return TabAlignment.Left;
            case DockStyle.Right: return TabAlignment.Right;
            case DockStyle.Bottom: return TabAlignment.Bottom;
            default: return TabAlignment.Top;
          }
        }
      }
      set
      {
        switch (value)
        {
          case TabAlignment.Top: Bar.Parent.Dock = DockStyle.Top; break;
          case TabAlignment.Left: Bar.Parent.Dock = DockStyle.Left; break;
          case TabAlignment.Right: Bar.Parent.Dock = DockStyle.Right; break;
          case TabAlignment.Bottom: Bar.Parent.Dock = DockStyle.Bottom; break;
        }
      }
    }

    #endregion

    #region Локальное меню управления размещением панели

    /// <summary>
    /// Локальное меню управления размещением локальной панели инструментов.
    /// </summary>
    public class DockCommandItems : EFPCommandItems
    {
      #region Конструктор

      /// <summary>
      /// Создает команды меню
      /// </summary>
      /// <param name="owner">Управляемая панель инструментов</param>
      public DockCommandItems(EFPPanelToolBar owner)
      {
        _Owner = owner;

        if (owner.UseVisibleCommand)
        {
          ciVisible = new EFPCommandItem("View", "ToolBarVisible");
          ciVisible.MenuText = Res.Cmd_Menu_View_ToolBarVisible;
          ciVisible.GroupEnd = true;
          ciVisible.Click += new EventHandler(ciVisible_Click);
          Add(ciVisible);
        }

        ciTop = new EFPCommandItem("View", "ToolBarOnTop");
        ciTop.MenuText = Res.Cmd_Menu_View_ToolBarOnTop;
        ciTop.Click += new EventHandler(ciTop_Click);
        Add(ciTop);

        ciBottom = new EFPCommandItem("View", "ToolBarOnBottom");
        ciBottom.MenuText = Res.Cmd_Menu_View_ToolBarOnBottom;
        ciBottom.Click += new EventHandler(ciBottom_Click);
        Add(ciBottom);

        ciLeft = new EFPCommandItem("View", "ToolBarOnLeft");
        ciLeft.MenuText = Res.Cmd_Menu_View_ToolBarOnLeft;
        ciLeft.Click += new EventHandler(ciLeft_Click);
        Add(ciLeft);

        ciRight = new EFPCommandItem("View", "ToolBarOnRight");
        ciRight.MenuText = Res.Cmd_Menu_View_ToolBarOnRight;
        ciRight.Click += new EventHandler(ciRight_Click);
        Add(ciRight);

        if (owner.UseVisibleCommand)
          UserPanel.VisibleChanged += new EventHandler(UpdateItemState);
        UserPanel.DockChanged += new EventHandler(UpdateItemState);

        UpdateItemState(null, null);
      }

      #endregion

      #region Свойства

      /// <summary>
      /// Управляемая панель инструментов
      /// </summary>
      public EFPPanelToolBar Owner { get { return _Owner; } }
      private readonly EFPPanelToolBar _Owner;

      private Control UserPanel
      {
        get
        {
          return _Owner.Bar.Parent;
        }
      }

      #endregion

      #region Команды управления панелью

      private readonly EFPCommandItem ciVisible, ciTop, ciBottom, ciLeft, ciRight;

      void ciVisible_Click(object sender, EventArgs args)
      {
        UserPanel.Visible = !UserPanel.Visible;
      }

      void ciTop_Click(object sender, EventArgs args)
      {
        UserPanel.Dock = DockStyle.Top;
      }

      void ciBottom_Click(object sender, EventArgs args)
      {
        UserPanel.Dock = DockStyle.Bottom;
      }

      void ciLeft_Click(object sender, EventArgs args)
      {
        UserPanel.Dock = DockStyle.Left;
      }

      void ciRight_Click(object sender, EventArgs args)
      {
        UserPanel.Dock = DockStyle.Right;
      }

      void UpdateItemState(object sender, EventArgs args)
      {
        if (ciVisible != null)
          ciVisible.Checked = UserPanel.Visible;
        DockStyle Dock = UserPanel.Dock;
        ciTop.Checked = (Dock == DockStyle.Top);
        ciBottom.Checked = (Dock == DockStyle.Bottom);
        ciLeft.Checked = (Dock == DockStyle.Left);
        ciRight.Checked = (Dock == DockStyle.Right);
      }

      #endregion
    }

    /// <summary>
    /// Устанавливается методом <see cref="Attach(Panel)"/>, если управление
    /// панелью инструментов допустимо.
    /// </summary>
    public DockCommandItems PanelCommandItems { get { return _PanelCommandItems; } }
    private DockCommandItems _PanelCommandItems;

    #endregion
  }
}
