// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using FreeLibSet.DependedValues;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using FreeLibSet.Core;

namespace FreeLibSet.Forms
{
  // Термины:
  // TabControl - контейнер со страницами
  // TabPage    - страница
  // Tab        - ярлык страницы

  /// <summary>
  /// Провайдер для <see cref="TabControl"/>.
  /// - Поддержка скрытых страниц.
  /// - Отдельный <see cref="EFPBaseProvider"/> для каждой страницы.
  /// - Отдельное локальное меню для каждой страницы плюс общее локальное меню с автоматическим присоединением.
  /// - Исправление для изображений на ярлыках страниц.
  /// </summary>
  public class EFPTabControl : EFPControl<TabControl>
  {
    #region Конструкторы и Dispose

    /// <summary>
    /// Создает провайдер для существующего контейнера <see cref="System.Windows.Forms.TabControl"/>.
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control"></param>
    public EFPTabControl(EFPBaseProvider baseProvider, TabControl control)
      : base(new EFPTabControlBaseProvider(), control, true)
    {
      _Items = new List<EFPTabPage>();
      _TabPages = new TabPageCollection(this);

      this.BaseProvider.Parent = baseProvider;
      ((EFPTabControlBaseProvider)(this.BaseProvider)).ControlProvider = this;

      ItemListValid = false;
      control.ControlAdded += new ControlEventHandler(Control_ControlAdded);
      control.ControlRemoved += new ControlEventHandler(Control_ControlRemoved);

      _PrevSelectedTabControl = null;
      control.VisibleChanged += new EventHandler(Control_VisibleChanged);
      control.Selected += new TabControlEventHandler(Control_Selected);

      control.Disposed += new EventHandler(Control_Disposed);

      control.MouseDown += new MouseEventHandler(Control_MouseDown);
      control.MouseUp += new MouseEventHandler(Control_MouseUp);

      if (control.ImageList == null)
      {
        control.ImageList = EFPApp.MainImages.ImageList;
        for (int i = 0; i < control.TabCount; i++)
          UpdatePageImageKey(control.TabPages[i]);
      }
    }

    /// <summary>
    /// Создает новый контейнер <see cref="System.Windows.Forms.TabControl"/> и размещает его в родительском элементе.
    /// </summary>
    /// <param name="parent">Провайдер родительского управляющего элемента, например <see cref="EFPGroupBox"/> или другая страница <see cref="EFPTabPage"/></param>
    public EFPTabControl(EFPControlBase parent)
      : this(parent.BaseProvider, CreateTabControl(parent.Control))
    {
    }

    /// <summary>
    /// Создает новый контейнер <see cref="System.Windows.Forms.TabControl"/> и размещает его на форме
    /// </summary>
    /// <param name="formProvider">Провайдер формы</param>
    public EFPTabControl(EFPFormProvider formProvider)
      : this(formProvider, CreateTabControl(formProvider.Form))
    {
    }

    private static TabControl CreateTabControl(Control parent)
    {
      TabControl tabControl = new TabControl();
      tabControl.Dock = DockStyle.Fill;
      parent.Controls.Add(tabControl);
      return tabControl;
    }

    void Control_Disposed(object sender, EventArgs args)
    {
      try
      {
        for (int i = 0; i < _Items.Count; i++)
        {
          if (!_Items[i].Visible)
            _Items[i].Control.Dispose();

          _Items[i].BaseProvider.Parent = null;
        }
        // 31.10.2014
        // FItems.Clear();
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e);
      }
    }

    #endregion

    #region Наследник EFPBaseProvider

    private class EFPTabControlBaseProvider : EFPBaseProvider
    {
      #region Доступ к меню

      public EFPTabControl ControlProvider;

      public override void InitCommandItemList(List<EFPCommandItems> list)
      {
        if (ControlProvider != null)
        {
          if (ControlProvider.CommandItems.Count > 0)
            list.Add(ControlProvider.CommandItems);
        }
        base.InitCommandItemList(list);
      }

      #endregion
    }

    #endregion

    #region Список страниц

    /// <summary>
    /// Коллекция страниц для свойства <see cref="EFPTabControl.TabPages"/>.
    /// </summary>
    public sealed class TabPageCollection : IEnumerable<EFPTabPage>
    {
      #region Конструктор

      internal TabPageCollection(EFPTabControl owner)
      {
        _Owner = owner;
      }

      private readonly EFPTabControl _Owner;

      #endregion

      #region Доступ у страницам

      /// <summary>
      /// Общее количество страниц, включая скрытые
      /// </summary>
      public int Count
      {
        get
        {
          _Owner.ValidateItemList();
          return _Owner._Items.Count;
        }
      }

      /// <summary>
      /// Доступ к странице по индексу
      /// </summary>
      /// <param name="index">Индекс страницы от 0 до (<see cref="Count"/>-1)</param>
      /// <returns>Провайдер для доступа к странице</returns>
      public EFPTabPage this[int index]
      {
        get
        {
          _Owner.ValidateItemList();
          return _Owner._Items[index];
        }
      }

      /// <summary>
      /// Доступ к странице по объекту <see cref="System.Windows.Forms.TabPage"/>.
      /// Если страница не входит в просмотр, возвращает null.
      /// </summary>
      /// <param name="pageControl">Страница Windows Forms</param>
      /// <returns>Провайдер для доступа к странице</returns>
      public EFPTabPage this[TabPage pageControl]
      {
        get
        {
          int p = IndexOf(pageControl);
          if (p >= 0)
            return _Owner._Items[p];
          else
            return null;
        }
      }

      /// <summary>
      /// Поиск страницы по провайдеру <see cref="EFPTabPage"/>.
      /// </summary>
      /// <param name="pageControlProvider">Искомый провайдер</param>
      /// <returns>Индекс страницы или (-1), если страница не найдена</returns>
      public int IndexOf(EFPTabPage pageControlProvider)
      {
        if (pageControlProvider == null)
          return -1;

        _Owner.ValidateItemList();
        return _Owner._Items.IndexOf(pageControlProvider);
      }

      /// <summary>
      /// Поиск страницы по объекту <see cref="System.Windows.Forms.TabPage"/>.
      /// </summary>
      /// <param name="pageControl">Страница Windows Forms</param>
      /// <returns>Индекс страницы или (-1), если страница не найдена</returns>
      public int IndexOf(TabPage pageControl)
      {
        if (pageControl == null)
          return -1;

        _Owner.ValidateItemList();
        for (int i = 0; i < _Owner._Items.Count; i++)
        {
          if (_Owner._Items[i].Control == pageControl)
            return i;
        }
        return -1;
      }

      #endregion

      #region Добавление страниц

      /// <summary>
      /// Добавляет страницу в просмотр.
      /// Эту версию можно использовать только при создании формы.
      /// Добавление страницы не делает ее активной. Используйте свойство <see cref="EFPTabControl.SelectedTab"/>.
      /// Для динамического создания страницы в процессе работы используйте конструктор <see cref="EFPTabPage"/> без ссылки на <see cref="EFPTabControl"/>,
      /// заполните страницу управляющими элементами, и вызовите перегрузку метода<see cref="Add(EFPTabPage)"/>
      /// </summary>
      /// <param name="text">Заголовок ярлыка страницы</param>
      /// <returns>Провайдер для новой страницы</returns>
      public EFPTabPage Add(string text)
      {
        TabPage page = new TabPage(text);
        _Owner.Control.TabPages.Add(page);
        return this[page]; // здесь создается новый объект EFPTabPage
      }

      /// <summary>
      /// Присоединяет заполненную страницу к просмотру.
      /// Можно использовать и для динамического добавления страниц.
      /// Добавление страницы не делает ее активной. Используйте свойство <see cref="EFPTabControl.SelectedTab"/>.
      /// </summary>
      /// <param name="tabPage">Созданный но не присоединенный провайдер страницы</param>
      public void Add(EFPTabPage tabPage)
      {
#if DEBUG
        if (tabPage == null)
          throw new ArgumentNullException("tabPage");
#endif
        if (tabPage.Parent != null)
          throw ExceptionFactory.ObjectPropertyAlreadySet(tabPage, "Parent");
        tabPage.BaseProvider.Parent = _Owner.BaseProvider;
        tabPage.Parent = _Owner;
        _Owner._Items.Add(tabPage);
        if (tabPage.Visible)
          _Owner.Control.TabPages.Add(tabPage.Control);
      }

      #endregion

      #region IEnumerable<EFPTabPage> Members

      /// <summary>
      /// Возвращает перечислитель по провайдерам страниц.
      /// Перечисляются в том числе и скрытые страницы.
      /// </summary>
      /// <returns>Перечислитель</returns>
      public IEnumerator<EFPTabPage> GetEnumerator()
      {
        _Owner.ValidateItemList();
        return _Owner._Items.GetEnumerator();
      }

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
        _Owner.ValidateItemList();
        return _Owner._Items.GetEnumerator();
      }

      #endregion
    }

    /// <summary>
    /// Список страниц. Сюда входят также скрытые страницы
    /// </summary>
    public TabPageCollection TabPages { get { return _TabPages; } }
    private readonly TabPageCollection _TabPages;

    /// <summary>
    /// Реальный список страниц
    /// </summary>
    internal readonly List<EFPTabPage> _Items;

    /// <summary>
    /// Флажок корректности списка страниц. 
    /// В <see cref="System.Windows.Forms.TabControl"/> страницы могут добавляться или удаляться и может быть необходимо
    /// обновить список <see cref="EFPTabPage"/>.
    /// </summary>
    internal bool ItemListValid;

    void Control_ControlAdded(object sender, ControlEventArgs args)
    {
      try
      {
        if (!_InsideVisibleChanged)
          ItemListValid = false;

        TabPage tabPage = args.Control as TabPage;
        if (tabPage != null)
          UpdatePageImageKey(tabPage);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e);
      }
    }

    void Control_ControlRemoved(object sender, ControlEventArgs args)
    {
      try
      {
        if (!_InsideVisibleChanged)
          ItemListValid = false;
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e);
      }
    }

    /// <summary>
    /// Исправление для изображений на ярлыке страницы.
    /// Когда свойство TabPage.ImageKey устанавливается до присоединения страницы к
    /// TabControl, значок на ярлыке не рисуется.
    /// </summary>
    /// <param name="pageControl"></param>
    private static void UpdatePageImageKey(TabPage pageControl)
    {
      if (!String.IsNullOrEmpty(pageControl.ImageKey))
      {
        string imageKey = pageControl.ImageKey;
        pageControl.ImageKey = String.Empty;
        pageControl.ImageKey = imageKey;
      }
    }

    internal void ValidateItemList()
    {
      if (ItemListValid)
        return; // Обновление списка не требуется

      // Первый проход - удаление лишних объектов
      // Заодно создаем коллекцию для поиска
      Dictionary<TabPage, EFPTabPage> pages2 = new Dictionary<TabPage, EFPTabPage>();
      for (int i = _Items.Count - 1; i >= 0; i--)
      {
        if (!_Items[i].Visible)
          continue; // временно скрытая страница

        if (_Items[i].Control.IsDisposed || _Items[i].Control.Parent != Control)
        {
          _Items[i].BaseProvider.Parent = null;
          _Items.RemoveAt(i);
        }
        else
          pages2.Add(_Items[i].Control, _Items[i]);
      }

      // Второй проход - добавление недостающих страниц
      int lastItemIndex = 0;
      for (int i = 0; i < Control.TabCount; i++)
      {
        if (!pages2.ContainsKey(Control.TabPages[i]))
        {
          // Необходимо добавить провайдер для страницы
          EFPTabPage newPage = new EFPTabPage(this, Control.TabPages[i]);
          for (int j = lastItemIndex + 1; j < _Items.Count; j++)
          {
            if (_Items[j].Visible)
              break;
            else
              lastItemIndex++;
          }

          if (lastItemIndex >= _Items.Count)
            _Items.Add(newPage);
          else
            _Items.Insert(lastItemIndex + 1, newPage);
          lastItemIndex++;
        }
        else
        {
          EFPTabPage lastPage = pages2[Control.TabPages[i]];
          lastItemIndex = _Items.IndexOf(lastPage);
        }
      }

      ItemListValid = true;
    }

    internal bool _InsideVisibleChanged;

    #endregion

    #region SelectedTab

    /// <summary>
    /// Текущая выбранная страница.
    /// Если просмотр не содержит ни одной видимой страницы, возвращается null.
    /// Попытка установить значение свойства на скрытую страницу вызывает исключение.
    /// </summary>
    public EFPTabPage SelectedTab
    {
      get
      {
        TabPage page = Control.SelectedTab;
        if (page == null)
          return null;
        else
          return TabPages[page];
      }
      set
      {
        if (value == null)
          return;
        if (!value.Visible)
          throw new InvalidOperationException(Res.EFPTabPage_Err_SelectHiddenPage);

        Control.SelectedTab = value.Control;
      }
    }

    private TabPage _PrevSelectedTabControl;

    private void CheckSelectedTab()
    {
      if (Control.SelectedTab != _PrevSelectedTabControl)
      {
        OnSelectedTabChanged();
      }
    }

    /// <summary>
    /// Метод вызывается при изменении выбранной страницы в контейнере.
    /// При переопределении обязательно должен вызываться базовый метод.
    /// </summary>
    protected virtual void OnSelectedTabChanged()
    {
      if (SelectedTab != null)
        SelectedTab.PerformPageSelected();
      _PrevSelectedTabControl = Control.SelectedTab;

      if (_SelectedTabEx != null)
        _SelectedTabEx.Value = SelectedTab;
      if (_SelectedIndexEx != null)
        _SelectedIndexEx.Value = SelectedIndex;

      for (int i = 0; i < TabPages.Count; i++)
        TabPages[i].UpdateSelected();

      Validate();

      InitCurrentContextMenu();
    }

    /// <summary>
    /// Управляемое свойство <see cref="SelectedTab"/>
    /// </summary>
    public DepValue<EFPTabPage> SelectedTabEx
    {
      get
      {
        InitSelectedTabEx();
        return _SelectedTabEx;
      }
      set
      {
        InitSelectedTabEx();
        _SelectedTabEx.Source = value;
      }
    }

    private void InitSelectedTabEx()
    {
      if (_SelectedTabEx == null)
      {
        _SelectedTabEx = new DepInput<EFPTabPage>(SelectedTab, SelectedTabEx_ValueChanged);
        _SelectedTabEx.OwnerInfo = new DepOwnerInfo(this, "SelectedTabEx");
      }
    }

    private DepInput<EFPTabPage> _SelectedTabEx;

    void SelectedTabEx_ValueChanged(object sender, EventArgs args)
    {
      SelectedTab = _SelectedTabEx.Value;
    }

    void Control_VisibleChanged(object sender, EventArgs args)
    {
      try
      {
        CheckSelectedTab();
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e);
      }
    }

    void Control_Selected(object sender, TabControlEventArgs args)
    {
      try
      {
        CheckSelectedTab();
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e);
      }
    }

    #endregion

    #region SelectedIndex

    /// <summary>
    /// Индекс текущей выбранной страницы
    /// В отличие от оригинального свойства <see cref="System.Windows.Forms.TabControl.SelectedIndex"/>,
    /// когда нет ни одной видимой страницы, свойство возвращает -1, а не 0.
    /// В список входят также скрытые страницы.
    /// Попытка установить значение свойства на скрытую страницу вызывает исключение.
    /// </summary>
    public int SelectedIndex
    {
      get
      {
        EFPTabPage page = SelectedTab;
        if (page == null)
          return -1;
        else
          return TabPages.IndexOf(page);
      }
      set
      {
        if (value < 0)
          return;

        SelectedTab = TabPages[value];
      }
    }

    /// <summary>
    /// Индекс текущей выбранной страницы.
    /// Управляемое свойство для <see cref="SelectedIndex"/>.
    /// </summary>
    public DepValue<int> SelectedIndexEx
    {
      get
      {
        InitSelectedIndexEx();
        return _SelectedIndexEx;
      }
      set
      {
        InitSelectedIndexEx();
        _SelectedIndexEx.Source = value;
      }
    }
    private DepInput<int> _SelectedIndexEx;

    private void InitSelectedIndexEx()
    {
      if (_SelectedIndexEx == null)
      {
        _SelectedIndexEx = new DepInput<int>(SelectedIndex, SelectedIndexEx_ValueChanged);
        _SelectedIndexEx.OwnerInfo = new DepOwnerInfo(this, "SelectedIndexEx");
      }
    }

    void SelectedIndexEx_ValueChanged(object sender, EventArgs args)
    {
      SelectedIndex = _SelectedIndexEx.Value;
    }

    #endregion

    #region Контекстное меню

    /// <summary>
    /// Контекстное меню, используемое, когда нет выбранной страницы или по щелчку мыши по пустому месту на полосе ярлыков.
    /// </summary>
    private ContextMenuStrip _MainContextMenuStrip;

    /// <summary>
    /// Инициализирует контекстное меню
    /// </summary>
    protected override void OnCreated()
    {
      base.OnCreated();
      PrepareContextMenu();
      _MainContextMenuStrip = Control.ContextMenuStrip;
    }

    /// <summary>
    /// Переключает контекстное меню на активную страницу
    /// </summary>
    protected override void OnAttached()
    {
      base.OnAttached();
      InitCurrentContextMenu();
    }

    private void InitCurrentContextMenu()
    {
      if (SelectedTab == null)
        Control.ContextMenuStrip = _MainContextMenuStrip;
      else
        Control.ContextMenuStrip = SelectedTab.GetContextMenuStrip();
    }

    /// <summary>
    /// Инициализирует контекстное меню в соответствии со страницей, на которой выполнен щелчок правой кнопки мыши по ярлыку
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void Control_MouseDown(object sender, MouseEventArgs args)
    {
      if (args.Button == MouseButtons.Right)
      {
        for (int i = 0; i < Control.TabPages.Count; i++)
        {
          Rectangle rc = Control.GetTabRect(i);
          if (rc.Contains(args.Location))
          { 
            EFPTabPage tab = TabPages[Control.TabPages[i]];
            Control.ContextMenuStrip = tab.GetContextMenuStrip();
            return;
          }
        }

        // Щелчок вне ярлыка страницы
        Control.ContextMenuStrip = _MainContextMenuStrip;
      }
    }

    /// <summary>
    /// Восстанавливает контекстное меню после отпускания правой кнопки мыши
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void Control_MouseUp(object sender, MouseEventArgs args)
    {
      if (args.Button == MouseButtons.Right)
        InitCurrentContextMenu();
    }

    #endregion
  }

  /// <summary>
  /// Свойства для управления <see cref="System.Windows.Forms.TabPage"/>: текст, значок и всплывающая подсказка.
  /// </summary>
  public interface IEFPTabPageControl
  {
    #region Свойства

    /// <summary>
    /// Текст ярлыка страницы
    /// </summary>
    string Text { get; set; }

    /// <summary>
    /// Значок ярлыка страницы в <see cref="EFPApp.MainImages"/>
    /// </summary>
    string ImageKey { get; set; }

    /// <summary>
    /// Текст всплывающей подсказки ярлыка страницы
    /// </summary>
    string ToolTipText { get; set; }

    #endregion
  }

  #region Перечисление EFPTabPageControlOptions

  /// <summary>
  /// Набор флагов определяет, какими свойствами ярлыка страницы <see cref="System.Windows.Forms.TabPage"/> следует управлять.
  /// Используется <see cref="EFPErrorDataGridView.TabPageControlOptions"/>.
  /// </summary>
  [Flags]
  public enum EFPTabPageControlOptions
  {
    /// <summary>
    /// Управлять только текстом ярлыка страницы
    /// </summary>
    Text = 0x1,

    /// <summary>
    /// Управлять только значком ярлыка страницы
    /// </summary>
    ImageKey = 0x2,

    /// <summary>
    /// Управлять только всплывающей подсказкой ярлыка страницы
    /// </summary>
    ToolTipText = 0x4,

    /// <summary>
    /// Ничем не управлять
    /// </summary>
    None = 0,

    /// <summary>
    /// Управлять текстом ярлыка страницы, значком и всплывающей подсказкой
    /// </summary>
    All = Text | ImageKey | ToolTipText
  }

  #endregion

  /// <summary>
  /// Объект страницы, обычно создается <see cref="EFPTabControl"/> автоматически.
  /// </summary>
  public class EFPTabPage : EFPTextViewControl<TabPage>, IEFPTabPageControl
  {
    #region Конструктор

    /// <summary>
    /// Создается страница с заголовком ярлыка <paramref name="text"/>.
    /// Cозданная страница сразу же добавляется в объект <see cref="EFPTabControl"/>.
    /// Этот конструктор можно использовать только до вывода формы на экран.
    /// Для динамического создания страницы используйте версию конструктора без аргумента <see cref="EFPTabControl"/>.
    /// </summary>
    /// <param name="parent">Провайдер контейнера</param>
    /// <param name="text">Заголовок ярлыка страницы</param>
    public EFPTabPage(EFPTabControl parent, string text)
      : this(parent, new TabPage(text))
    {
      parent.Control.TabPages.Add(Control);
      parent._Items.Add(this);

      //InitControl();
    }

    /// <summary>
    /// Создается страница без заголовка.
    /// Этот конструктор можно использовать только до вывода формы на экран.
    /// Для динамического создания страницы используйте версию конструктора без аргумента <see cref="EFPTabControl"/>.
    /// </summary>
    /// <param name="parent">Провайдер контейнера</param>
    public EFPTabPage(EFPTabControl parent)
      : this(parent, String.Empty)
    {
    }

    internal EFPTabPage(EFPTabControl parent, TabPage control)
      : base(new EFPTabPageBaseProvider(), control, false)
    {
#if DEBUG
      if (parent == null)
        throw new ArgumentNullException("parent");
#endif

      _Parent = parent;
      BaseProvider.Parent = parent.BaseProvider;

      Init();
    }


    /// <summary>
    /// Эта версия конструктора создает новую страницу <see cref="TabPage"/>, не привязаную к <see cref="TabControl"/>.
    /// После инициализации управляющих элементов, добавьте созданный <see cref="EFPTabPage"/> к коллекции страниц вызовом метода <see cref="EFPTabControl.TabPageCollection.Add(EFPTabPage)"/>.
    /// Эту версию можно использовать для динамического добавления страниц.
    /// Создается страница без заголовка.
    /// </summary>
    public EFPTabPage()
      : this(String.Empty)
    {
      Init();
    }

    /// <summary>
    /// Эта версия конструктора создает новую страницу <see cref="TabPage"/>, не привязаную к <see cref="TabControl"/>.
    /// После инициализации управляющих элементов, добавьте созданный <see cref="EFPTabPage"/> к коллекции страниц вызовом метода <see cref="EFPTabControl.TabPageCollection.Add(EFPTabPage)"/>.
    /// Эту версию можно использовать для динамического добавления страниц.
    /// </summary>
    /// <param name="text">Текст ярлыка страницы</param>
    public EFPTabPage(string text)
      : base(new EFPTabPageBaseProvider(), new TabPage(text), false)
    {
      Init();
    }

    private void Init()
    {
      if (EFPApp.EasyInterface)
        Control.BackColor = SystemColors.Control;
      else
        Control.BackColor = Color.Transparent;
      Control.UseVisualStyleBackColor = !EFPApp.EasyInterface; // 10.04.2015

      _PageVisible = true;
      ((EFPTabPageBaseProvider)BaseProvider).DisplayName = String.Format(Res.EFPTabPage_Name_Default, Control.Text);
      ((EFPTabPageBaseProvider)BaseProvider).ControlProvider = this;
    }

    #endregion

    #region Наследник EFPBaseProvider

    private class EFPTabPageBaseProvider : EFPBaseProvider
    {
      #region Доступ к меню

      public EFPTabPage ControlProvider;

      public override void InitCommandItemList(List<EFPCommandItems> list)
      {
        if (ControlProvider != null)
        {
          if (ControlProvider.CommandItems.Count > 0)
            list.Add(ControlProvider.CommandItems);
        }
        base.InitCommandItemList(list);
      }

      #endregion
    }

    #endregion

    #region Общие свойства

    /// <summary>
    /// Провайдер просмотра, к которому относится страница.
    /// Задается в конструкторе. Не может быть null.
    /// </summary>
    public EFPTabControl Parent { 
      get { return _Parent; }
      internal set { _Parent = value; }
    }
    private EFPTabControl _Parent;

    #endregion

    #region Свойство Visible

    /// <summary>
    /// Видимость страниц
    /// Стандартный <see cref="System.Windows.Forms.TabPage"/> не поддерживает временное сокрытие страниц.
    /// Для эмуляции действия страница временно удаляется из <see cref="System.Windows.Forms.TabControl"/>, а при
    /// установке <see cref="ControlVisible"/>=true - возвращается обратно.
    /// </summary>
    protected override bool ControlVisible
    {
      get
      {
        return _PageVisible;
      }
      set
      {
        if (value == _PageVisible)
          return;

        _PageVisible = value; // перенсено наверх 16.07.2023

        if (Parent != null)
        {
          Parent.ValidateItemList(); // иначе страница может совсем исчезнуть

          Parent._InsideVisibleChanged = true;
          try
          {
            if (value)
            {
              // Ищем индекс предыдущей страницы
              int prevPageIndex = -1;
              for (int i = Parent.TabPages.IndexOf(this) - 1; i >= 0; i--)
              {
                if (Parent.TabPages[i].Visible)
                {
                  TabPage prevPage = Parent.TabPages[i].Control;
                  prevPageIndex = Parent.Control.TabPages.IndexOf(prevPage);
                  break;
                }
              }

              // Добавляем страницу в TabControl
              int thisPageIndex = prevPageIndex + 1;
              if (thisPageIndex < (Parent.TabPages.Count - 1))
                Parent.Control.TabPages.Insert(thisPageIndex, Control);
              else
                Parent.Control.TabPages.Add(Control);
            }
            else
            {
              // Удаляем страницу из TabContol
              Parent.Control.TabPages.Remove(Control);
            }
          }
          finally
          {
            Parent._InsideVisibleChanged = false;
          }
        }
        ControlVisibleChanged(Control, EventArgs.Empty);
      }
    }
    private bool _PageVisible;

    /// <summary>
    /// Возвращает true.
    /// Свойство <see cref="EFPControlBase.Enabled"/> не актуально для страницы <see cref="System.Windows.Forms.TabPage"/>.
    /// </summary>
    public override bool EnabledState
    {
      get { return true; }
    }

    #endregion

    #region Свойство Selected

    /// <summary>
    /// Свойство возвращает true, если текущая страница является выбранной в <see cref="System.Windows.Forms.TabControl"/>.
    /// Скрытая страница не может быть выбранной.
    /// Для выбора страницы используйте свойства <see cref="EFPTabControl.SelectedIndex"/> или <see cref="EFPTabControl.SelectedTab"/>.
    /// </summary>
    public bool Selected
    {
      get 
      {
        if (Parent == null)
          return false;
        else
          return Parent.SelectedTab == this; 
      }
    }

    /// <summary>
    /// Управляемое свойство для <see cref="Selected"/>
    /// </summary>
    public DepValue<bool> SelectedEx
    {
      get
      {
        if (_SelectedEx == null)
        {
          _SelectedEx = new DepOutput<bool>(Selected);
          _SelectedEx.OwnerInfo = new DepOwnerInfo(this, "SelectedEx");
        }
        return _SelectedEx;
      }
    }
    private DepOutput<bool> _SelectedEx;

    internal void UpdateSelected()
    {
      if (_SelectedEx != null)
        _SelectedEx.OwnerSetValue(Selected);
    }

    #endregion

    #region События активации

    /// <summary>
    /// Вызывается при первом переключении на страницу
    /// </summary>
    public event EventHandler FirstSelected;

    /// <summary>
    /// Вызывает обработчик события <see cref="FirstSelected"/>, если он присоединен
    /// </summary>
    protected virtual void OnFirstSelected()
    {
      if (FirstSelected != null)
        FirstSelected(this, EventArgs.Empty);
    }

    private bool _FirstSelectedCalled;

    /// <summary>
    /// Вызывается при каждом переключении на страницу
    /// </summary>
    public event EventHandler PageSelected;

    /// <summary>
    /// Вызывает обработчик события <see cref="PageSelected"/>, если он присоединен
    /// </summary>
    protected virtual void OnPageSelected()
    {
      if (PageSelected != null)
        PageSelected(this, EventArgs.Empty);
    }

    internal void PerformPageSelected()
    {
      if (!_FirstSelectedCalled)
      {
        _FirstSelectedCalled = true;
        OnFirstSelected();
      }
      OnPageSelected();
    }

    #endregion

    #region IEFPTabPageControl Members

    /// <summary>
    /// Имя значка ярлыка страницы.
    /// Значок должен находиться в списке <see cref="EFPApp.MainImages"/>.
    /// </summary>
    public string ImageKey
    {
      get { return Control.ImageKey; }
      set { Control.ImageKey = value; }
    }

    /// <summary>
    /// Всплывающая подсказка для ярлыка страницы
    /// </summary>
    public override string ToolTipText
    {
      get { return Control.ToolTipText; }
      set
      {
        Control.ToolTipText = value;
        base.InitToolTips();
      }
    }

    #endregion

    #region Контекстное меню

    internal ContextMenuStrip GetContextMenuStrip()
    {
      PrepareContextMenu();
      return Control.ContextMenuStrip;
    }

    #endregion
  }
}
