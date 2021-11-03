using FreeLibSet.DependedValues;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

/*
 * The BSD License
 * 
 * Copyright (c) 2006-2015, Ageyev A.V.
 * 
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, 
 * this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice, 
 * this list of conditions and the following disclaimer in the documentation 
 * and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE 
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Провайдер для TabControl.
  /// - Поддержка скрытых вкладок
  /// - Отдельный BaseProvider для каждой вкладки
  /// - Исправление для изображений на вкладках
  /// </summary>
  public class EFPTabControl : EFPControl<TabControl>
  {
    #region Конструкторы и Dispose

    /// <summary>
    /// Создает провайдер
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

      if (control.ImageList == null)
      {
        control.ImageList = EFPApp.MainImages;
        for (int i = 0; i < control.TabCount; i++)
          UpdatePageImageKey(control.TabPages[i]);
      }
    }

    /// <summary>
    /// Создает и размещает объект с закладками в родительском элементе
    /// </summary>
    /// <param name="parent">Родительский управляющий элемент, например EFPGroupBox или другая EFPTabPage</param>
    public EFPTabControl(EFPControlBase parent)
      : this(parent.BaseProvider, CreateTabControl(parent.Control))
    {
    }

    /// <summary>
    /// Создает и размещает объект с закладками на форме
    /// </summary>
    /// <param name="formProvider">Провайдер формы</param>
    public EFPTabControl(EFPFormProvider formProvider)
      : this(formProvider, CreateTabControl(formProvider.Form))
    {
    }

    private static TabControl CreateTabControl(Control parent)
    {
      TabControl TabControl = new TabControl();
      TabControl.Dock = DockStyle.Fill;
      parent.Controls.Add(TabControl);
      return TabControl;
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
        EFPApp.ShowException(e, "Ошибка обработчика TabControl.Disposed");
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
    /// Коллекция страниц для свойства EFPTabContro.TabPages.
    /// </summary>
    public class TabPageCollection : IEnumerable<EFPTabPage>
    {
      #region Конструктор

      internal TabPageCollection(EFPTabControl owner)
      {
        _Owner = owner;
      }

      private EFPTabControl _Owner;

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
      /// <param name="index">Индекс страницы от 0 до (Count-1)</param>
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
      /// Доступ к странице по объекту System.Windows.Forms.TabPage.
      /// Если страница не входит в просмотр, возвращает null
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
      /// Поиск страницы по провайдеру EFPTabPage.
      /// </summary>
      /// <param name="pageControlProvider">Искомый провайдер</param>
      /// <returns>Индекс страницы или (-1), если страница не найдена</returns>
      public int IndexOf(EFPTabPage pageControlProvider)
      {
        _Owner.ValidateItemList();
        return _Owner._Items.IndexOf(pageControlProvider);
      }

      /// <summary>
      /// Поиск страницы по объекту System.Windows.Forms.TabPage.
      /// </summary>
      /// <param name="pageControl">Страница Windows Forms</param>
      /// <returns>Индекс страницы или (-1), если страница не найдена</returns>
      public int IndexOf(TabPage pageControl)
      {
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
      /// Добавляет страницу в просмотр
      /// </summary>
      /// <param name="text">Заголовок закладки</param>
      /// <returns>Провайдер для новой страницы</returns>
      public EFPTabPage Add(string text)
      {
        TabPage Page = new TabPage(text);
        _Owner.Control.TabPages.Add(Page);
        return this[Page];
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
    private TabPageCollection _TabPages;

    /// <summary>
    /// Реальный список страниц
    /// </summary>
    internal List<EFPTabPage> _Items;

    /// <summary>
    /// Флажок корректности списка страниц. 
    /// В TabControl страницы могут добавляться или удаляться и может быть необходимо
    /// обновить список EFPTabPage
    /// </summary>
    internal bool ItemListValid;

    void Control_ControlAdded(object sender, ControlEventArgs args)
    {
      try
      {
        if (!_InsideVisibleChanged)
          ItemListValid = false;

        TabPage Page = args.Control as TabPage;
        if (Page != null)
          UpdatePageImageKey(Page);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка обработчика Control.ControlAdded");
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
        EFPApp.ShowException(e, "Ошибка обработчика Control.ControlRemoved");
      }
    }

    /// <summary>
    /// Исправление для изображений на вкладке.
    /// Когда свойство TabPage.ImageKey устанавливается до присоединения вкладки к
    /// TabControl, значок на корешке вкладки не рисуется
    /// </summary>
    /// <param name="pageControl"></param>
    private static void UpdatePageImageKey(TabPage pageControl)
    {
      if (!String.IsNullOrEmpty(pageControl.ImageKey))
      {
        string ImageKey = pageControl.ImageKey;
        pageControl.ImageKey = String.Empty;
        pageControl.ImageKey = ImageKey;
      }
    }

    internal void ValidateItemList()
    {
      if (ItemListValid)
        return; // Обновление списка не требуется

      // Первый проход - удаление лишних объектов
      // Заодно создаем коллекцию для поиска
      Dictionary<TabPage, EFPTabPage> Pages2 = new Dictionary<TabPage, EFPTabPage>();
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
          Pages2.Add(_Items[i].Control, _Items[i]);
      }

      // Второй проход - добавление недостающих страниц
      int LastItemIndex = 0;
      for (int i = 0; i < Control.TabCount; i++)
      {
        if (!Pages2.ContainsKey(Control.TabPages[i]))
        {
          // Необходимо добавить провайдер для страницы
          EFPTabPage NewPage = new EFPTabPage(this, Control.TabPages[i]);
          for (int j = LastItemIndex + 1; j < _Items.Count; j++)
          {
            if (_Items[j].Visible)
              break;
            else
              LastItemIndex++;
          }

          if (LastItemIndex >= _Items.Count)
            _Items.Add(NewPage);
          else
            _Items.Insert(LastItemIndex + 1, NewPage);
          LastItemIndex++;
        }
        else
        {
          EFPTabPage LastPage = Pages2[Control.TabPages[i]];
          LastItemIndex = _Items.IndexOf(LastPage);
        }
      }

      ItemListValid = true;
    }

    internal bool _InsideVisibleChanged;

    #endregion

    #region SelectedTab

    /// <summary>
    /// Текущая выбранная закладка
    /// Если просмотр не содержит ни одной видимой закладки, возвращается null
    /// </summary>
    public EFPTabPage SelectedTab
    {
      get
      {
        TabPage Page = Control.SelectedTab;
        if (Page == null)
          return null;
        else
          return TabPages[Page];
      }
      set
      {
        if (value == null)
          return;
        if (!value.Visible)
          throw new InvalidOperationException("Нельзя активировать скрытую закладку");

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
    /// Метод вызывается при изменении выбранной вкладки в управляющем элементе.
    /// При переопределении обязательно должен вызываться базовый метод
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
    }

    /// <summary>
    /// Свойство SelectedTabEx
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
        EFPApp.ShowException(e, "Ошибка обработчика TabControl.VisibleChanged");
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
        EFPApp.ShowException(e, "Ошибка обработчика TabControl.Selected");
      }
    }

    #endregion

    #region SelectedIndex

    /// <summary>
    /// Индекс текущей выбранной страницы
    /// В отличие от оригинального свойства TabControl.SelectedIndex,
    /// когда нет ни одной видимой страницы, свойство возвращает -1, а не 0
    /// В список входят также скрытые страницы
    /// </summary>
    public int SelectedIndex
    {
      get
      {
        EFPTabPage Page = SelectedTab;
        if (Page == null)
          return -1;
        else
          return TabPages.IndexOf(Page);
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
    /// Управляемое свойство для SelectedIndex
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
  }

  /// <summary>
  /// Свойства для управления TabPage
  /// </summary>
  public interface IEFPTabPageControl
  {
    #region Свойства

    /// <summary>
    /// Текст закладки
    /// </summary>
    string Text { get; set; }

    /// <summary>
    /// Значок в EFPApp.MainImages
    /// </summary>
    string ImageKey { get; set; }

    /// <summary>
    /// Текст всплывающей подсказки
    /// </summary>
    string ToolTipText { get; set; }

    #endregion
  }

  #region Перечисление EFPTabPageControlOptions

  /// <summary>
  /// Перечисление управляет, какими частями закладки следует управлять.
  /// Используется EFPErrorDataGridView.TabPageControlOptions
  /// </summary>
  [Flags]
  public enum EFPTabPageControlOptions
  {
    /// <summary>
    /// Управлять только текстом закладки
    /// </summary>
    Text = 0x1,

    /// <summary>
    /// Управлять только значком
    /// </summary>
    ImageKey = 0x2,

    /// <summary>
    /// Управлять только всплывающей подсказкой
    /// </summary>
    ToolTipText = 0x4,

    /// <summary>
    /// Ничем не управлять
    /// </summary>
    None = 0,

    /// <summary>
    /// Управлять текстом закладки, значком и всплывающей подсказкой
    /// </summary>
    All = Text | ImageKey | ToolTipText
  }

  #endregion

  /// <summary>
  /// Объект страницы создается EFPTabControl автоматически
  /// </summary>
  public class EFPTabPage : EFPTextViewControl<TabPage>, IEFPTabPageControl
  {
    #region Конструктор

    /// <summary>
    /// Создается закладка с заголовком <paramref name="text"/>.
    /// В отличие от оригинального объекта TabPage, созданная закладка сразу же
    /// добавляется в объект EFPTabControl
    /// </summary>
    /// <param name="parent">Провайдер просмотра с вкладками</param>
    /// <param name="text">Заголовок закладки</param>
    public EFPTabPage(EFPTabControl parent, string text)
      : this(parent, new TabPage(text))
    {
      parent.Control.TabPages.Add(Control);
      parent._Items.Add(this);

      //InitControl();
    }

    /// <summary>
    /// Создается закладка без заголовка.
    /// В отличие от оригинального объекта TabPage, созданная закладка сразу же
    /// добавляется в объект EFPTabControl
    /// </summary>
    /// <param name="parent">Провайдер просмотра с вкладками</param>
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
      _PageVisible = true;
      ((EFPTabPageBaseProvider)BaseProvider).DisplayName = "Для TabPage \"" + control.Text + "\"";
      ((EFPTabPageBaseProvider)BaseProvider).ControlProvider = this;
      BaseProvider.Parent = parent.BaseProvider;

      InitControl();
    }

    private void InitControl()
    {
      if (EFPApp.EasyInterface)
        Control.BackColor = SystemColors.Control;
      else
        Control.BackColor = Color.Transparent;
      Control.UseVisualStyleBackColor = !EFPApp.EasyInterface; // 10.04.2015
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
    /// Провайдер просмотра, к которому относится вкладка.
    /// Задается в конструкторе. Не может быть null.
    /// </summary>
    public EFPTabControl Parent { get { return _Parent; } }
    private EFPTabControl _Parent;

    #endregion

    #region Свойство Visible

    /// <summary>
    /// Видимость страниц
    /// Стандартный TabPage не поддерживает временное сокрытие страниц.
    /// Для эмуляции действия страница временно удаляется из TabControl, а при
    /// установке Visible=true - возвращается обратно
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

        Parent.ValidateItemList(); // иначе закладка может совсем исчезнуть

        Parent._InsideVisibleChanged = true;
        try
        {
          if (value)
          {
            // Ищем индекс предыдущей страницы
            int PrevPageIndex = -1;
            for (int i = Parent.TabPages.IndexOf(this) - 1; i >= 0; i--)
            {
              if (Parent.TabPages[i].Visible)
              {
                TabPage PrevPage = Parent.TabPages[i].Control;
                PrevPageIndex = Parent.Control.TabPages.IndexOf(PrevPage);
                break;
              }
            }

            // Добавляем страницу в TabControl
            int ThisPageIndex = PrevPageIndex + 1;
            if (ThisPageIndex < (Parent.TabPages.Count - 1))
              Parent.Control.TabPages.Insert(ThisPageIndex, Control);
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
        _PageVisible = value;
        ControlVisibleChanged(Control, EventArgs.Empty);
      }
    }
    private bool _PageVisible;

    /// <summary>
    /// Возвращает true.
    /// Свойство Enabled не актуально для закладки.
    /// </summary>
    public override bool EnabledState
    {
      get { return Enabled; }
    }

    #endregion

    #region Свойство Selected

    /// <summary>
    /// Свойство возвращает true, если текущая страница является выбранной в TabControl
    /// </summary>
    public bool Selected
    {
      get { return Parent.SelectedTab == this; }
    }

    /// <summary>
    /// Управляемое свойство для Selected
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
    /// Вызывается при первом переключении на закладку
    /// </summary>
    public event EventHandler FirstSelected;

    /// <summary>
    /// Вызывает обработчик события FirstSelected, если он присоединен
    /// </summary>
    protected virtual void OnFirstSelected()
    {
      if (FirstSelected != null)
        FirstSelected(this, EventArgs.Empty);
    }

    private bool _FirstSelectedCalled;

    /// <summary>
    /// Вызывается при каждом переключении на закладку
    /// </summary>
    public event EventHandler PageSelected;

    /// <summary>
    /// Вызывает обработчик события PageSelected, если он присоединен
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
    /// Имя значка вкладки.
    /// Значок должен находиться в списке EFPApp.MainImages.
    /// </summary>
    public string ImageKey
    {
      get { return Control.ImageKey; }
      set { Control.ImageKey = value; }
    }

    /// <summary>
    /// Всплывающая подсказка для ярлычка закладки
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
  }
}
