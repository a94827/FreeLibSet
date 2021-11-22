﻿// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Windows.Forms;
using System.Drawing;
using FreeLibSet.Core;
using FreeLibSet.Collections;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Коллекция панелек, размещаемых в статусной строке.
  /// </summary>
  public class EFPStatusBarPanels : EFPUIObjs
  {
    #region Конструктор и Dispose

    /// <summary>
    /// Эта версия конструктора предназначена для создания панелей для управляющего элемента
    /// </summary>
    /// <param name="statusBarControl">Интефрейс управляющего элемента статусной строки. Не может быть null</param>
    /// <param name="ownerControl">Если не null, то к этому элементу будет присоединен
    /// обработчик Disposed, чтобы выполнить Dispose() для коллекции</param>
    public EFPStatusBarPanels(IEFPStatusBarControl statusBarControl, Control ownerControl)
    {
      if (statusBarControl == null)
        throw new ArgumentNullException("statusBarControl");
      _StatusBarControl = statusBarControl;

      _Items = new List<EFPUIStatusPanelObj>();
      if (ownerControl != null)
        ownerControl.Disposed += new EventHandler(OwnerControl_Disposed);
    }

#pragma warning disable 0618 // обход [Obsolete]
    /// <summary>
    /// Эта версия конструктора предназначена для создания панелей на уровне приложения
    /// </summary>
    [Obsolete("Создавать объекты EFPStatusBarPanels из прилоежния не следует. Используйте EFPApp.Interface", false)]
    public EFPStatusBarPanels()
      : this(EFPApp.StatusBar, null)
    {
    }
#pragma warning restore 0618 

    /// <summary>
    /// Вызывает метод Detach()
    /// </summary>
    /// <param name="disposing">true, если вызван метод Dispose()</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        /*
        foreach (EFPUIStatusPanelObj obj in FItems)
          obj.Dispose();
        FItems.Clear();
         * */
        Detach();
      }
      base.Dispose(disposing);
    }

    void OwnerControl_Disposed(object sender, EventArgs args)
    {
      Dispose();
    }

    #endregion

    #region Добавление элементов

    /// <summary>
    /// Создает панель статусной строки для команды и добавляет ее в список панелей
    /// </summary>
    /// <param name="item">Команда</param>
    public void Add(EFPCommandItem item)
    {
      EFPUIStatusPanelObj obj = new EFPUIStatusPanelObj(this, item);
      _Items.Add(obj);
    }

    /// <summary>
    /// Создает панели статусной строки для команд и добавляет их в список панелей.
    /// Используются только команды с установленным свойством EFPCommandItem.StatusBarUsage
    /// </summary>
    /// <param name="items">Список команд</param>
    public void Add(EFPCommandItems items)
    {
      foreach (EFPCommandItem Item in items)
      {
        if (Item.StatusBarUsage)
          Add(Item);
      }
    }

    #endregion

    #region Активация

    /// <summary>
    /// Запоминаем между вызовами Attach() и Detach(), на случай неправильного поведения StatusBarControl.
    /// </summary>
    private EFPStatusBarHandler _UsedStatusBarHandler;

    /// <summary>
    /// Присоединяет панели в коллеции к StatusBarControl.StatusBarHandler
    /// </summary>
    public void Attach()
    {
      _UsedStatusBarHandler = StatusBarControl.StatusBarHandler;
      if (_UsedStatusBarHandler != null)
      {
        //if (this.Count!=4)
        _UsedStatusBarHandler.Add(this);
      }
    }

    /// <summary>
    /// Отсоединяет панели в коллеции к StatusBarControl.StatusBarHandler
    /// </summary>
    public void Detach()
    {
      if (_UsedStatusBarHandler != null)
        _UsedStatusBarHandler.Remove(this);
      _UsedStatusBarHandler = null;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Интерфейс элемента управления статусной строки.
    /// Используется в методах Attach() и Detach().
    /// Свойство задается в конструкторе и не может быть null. Однако, интерфейс может возвращать 
    /// null, если статусной строки нет.
    /// </summary>
    public IEFPStatusBarControl StatusBarControl { get { return _StatusBarControl; } }
    private IEFPStatusBarControl _StatusBarControl;

    internal List<EFPUIStatusPanelObj> Items { get { return _Items; } }
    private List<EFPUIStatusPanelObj> _Items;

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      string s;
      if (_Items.Count == 0)
        s = "Пустой список";
      else
      {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < _Items.Count; i++)
        {
          if (i > 0)
            sb.Append(", ");
          sb.Append(_Items[i].ToString());
        }
        s = sb.ToString();
      }
      return s;
    }

    #endregion
  }

  /// <summary>
  /// Класс для реализации одной панели
  /// </summary>
  internal class EFPUIStatusPanelObj : EFPUIObjBase
  {
    #region Конструктор

    public EFPUIStatusPanelObj(EFPStatusBarPanels owner, EFPCommandItem item)
      : base(owner, item)
    {
      _StatusLabel = new ToolStripStatusLabel();
      _StatusLabel.Width = 80;
      _StatusLabel.BorderSides = ToolStripStatusLabelBorderSides.All;
      base.SetAll();
      if (!item.HasChildren)
      {
        _StatusLabel.DoubleClickEnabled = true;
        _StatusLabel.DoubleClick += new EventHandler(ClickEvent);
      }
    }

    #endregion

    #region Свойства

    public ToolStripStatusLabel StatusLabel { get { return _StatusLabel; } }
    private ToolStripStatusLabel _StatusLabel;

    #endregion

    #region Переопределенные методы

    public override void SetStatusBarText()
    {
      if (Item.StatusBarText == EFPCommandItem.EmptyStatusBarText)
        _StatusLabel.Text = String.Empty;
      else
        _StatusLabel.Text = Item.StatusBarText;
    }

    public override void SetVisible()
    {
      _StatusLabel.Visible = Item.Visible;
    }

    public override void SetEnabled()
    {
      _StatusLabel.Enabled = Item.Enabled;
    }

    public override void SetImage()
    {
      if (Item.Image == null)
      {
        int idx = EFPApp.MainImages.Images.IndexOfKey(Item.ImageKey);
        if (idx < 0)
          _StatusLabel.Image = null;
        else
          _StatusLabel.Image = EFPApp.MainImages.Images[idx];
      }
      else
        _StatusLabel.Image = Item.Image;
    }

    public override void SetToolTipText()
    {
      _StatusLabel.ToolTipText = Item.ToolTipText;
    }

    #endregion
  }

  /// <summary>
  /// Интерфейс для доступа к управляющему элементу статусной строки.
  /// Реализуется EFPAppStatusBar (статусная строка главного окна программы) и 
  /// EFPFormProvider (собственная статусная строка формы)
  /// </summary>
  public interface IEFPStatusBarControl
  {
    /// <summary>
    /// Возвращает обработчик статусной строки или null,
    /// если статусная строка не поддерживается
    /// </summary>
    EFPStatusBarHandler StatusBarHandler { get;}
  }

  /// <summary>
  /// Обработчик статусной строки в главном окне программы или статусной строки окна.
  /// Хранит коллекцию объектов EFPStatusBarPanels, элементы которой меняются при смене фокуса ввода (или активного окна).
  /// Обработчик выполняет отложенное обновление панелек статусной строки по таймеру, так как из некоторых событий формы
  /// нельзя выполнять обновление. К тому же, изменение фокуса ввода может выполняться несколько раз подряд.
  /// Объект реализует Disposable-интерфейс. Уничтожение объекта очищает список панелек, но не удаляет управляющий элемент
  /// </summary>
  public sealed class EFPStatusBarHandler : DisposableObject, ICollection<EFPStatusBarPanels>, IEFPAppIdleHandler
  {
    // Нельзя использовать SimpleDisposableObject в качестве базового класса

    #region Конструктор и Dispose

    /// <summary>
    /// Создает новый обработчик для управляющего элемента статусной строки
    /// </summary>
    /// <param name="statusStripControl">Управляющий элемент</param>
    public EFPStatusBarHandler(StatusStrip statusStripControl)
    {
      if (statusStripControl == null)
        throw new ArgumentNullException("statusStripControl");
      if (statusStripControl.IsDisposed)
        throw new ObjectDisposedException("statusStripControl");
      _StatusStripControl = statusStripControl;

      _Panels = new SingleScopeList<EFPStatusBarPanels>();
      _ShownPanels = new EFPStatusBarPanels[0];

      // 14.03.2018 Отображаем всплывающую подсказку самостоятельно
      // Если использовать "штатную" подсказку, то она выводится под статусной строкой, и, если окно развернуто,
      // то она не видна.
      statusStripControl.ShowItemToolTips = false;
      if (SystemInformation.MousePresent)
        _TheToolTip = new ToolTip();
      // Хорошо бы обрабатывать события мыши, но:
      // ToolStrip.MouseMove не вызывается, если мышь находится над панелькой, а не над рамкой
      // ToolStripItem.MouseEnter,MouseLeave и MouseMove не вызываются, если ToolStripItem.Enabled=false
      // Поэтому всю обработку делаем в IdleHandler.
      // Собственно, так даже проще

      EFPApp.IdleHandlers.Add(this);
    }

    /// <summary>
    /// Отсоединяет текущий объект от EFPApp.IdleHandlers.
    /// Выполняет дополнительные действия по очистке.
    /// </summary>
    /// <param name="disposing">true, если вызван метод Dispose()</param>
    protected override void Dispose(bool disposing)
    {
      try
      {
        EFPApp.IdleHandlers.Remove(this);
      }
      catch { }

      if (disposing)
      {
        if (_TheToolTip != null)
        {
          _TheToolTip.Dispose();
          _TheToolTip = null;
        }
      }

      if (disposing && (!_StatusStripControl.IsDisposed))
        _StatusStripControl.Items.Clear();
      _Panels.Clear(); // чтобы не удерживать ресурсы
      _ShownPanels = new EFPStatusBarPanels[0];
      _CurrentToolTipItem = null;

      base.Dispose(disposing);
    }

    #endregion

    #region Свойство StatusStripControl

    /// <summary>
    /// Управляемая статусная строка
    /// </summary>
    public StatusStrip StatusStripControl { get { return _StatusStripControl; } }
    private StatusStrip _StatusStripControl;

    #endregion

    #region ICollection<EFPStatusBarPanels> Members

    /// <summary>
    /// Список присоединенных панелей.
    /// Этот список меняется при переключении управляющих элементов или форм, но может операжать реально 
    /// существующий набор панелек, который обновляется по таймеру.
    /// Элементы в списке хранятся в порядке, обратном отображаемому, так как самые новые объекты добавляются
    /// в конец списка, но добавляются в начало статусной строки
    /// </summary>
    private SingleScopeList<EFPStatusBarPanels> _Panels;

    /// <summary>
    /// Устанавливается в true при изменении списка _Panels, и сбрасывается в false при синхронизации по таймеру
    /// </summary>
    private bool _Modified;

    /// <summary>
    /// Реализация ICollection
    /// </summary>
    /// <param name="item">Добавляемая коллекция панелек</param>
    public void Add(EFPStatusBarPanels item)
    {
      _Panels.Add(item);
      _Modified = true;
    }

    void ICollection<EFPStatusBarPanels>.Clear()
    {
      _Panels.Clear();
      _Modified = true;
    }

    /// <summary>
    /// Реализация ICollection
    /// </summary>
    /// <param name="item">Объект коллекции для поиска</param>
    /// <returns>Результат поиска</returns>
    public bool Contains(EFPStatusBarPanels item)
    {
      return _Panels.Contains(item);
    }

    /// <summary>
    /// Реализация ICollection
    /// </summary>
    /// <param name="array"></param>
    /// <param name="arrayIndex"></param>
    public void CopyTo(EFPStatusBarPanels[] array, int arrayIndex)
    {
      _Panels.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Возвращает количество объектов EFPStatusBarPanels в коллекции, а не количество панелек статусной строки.
    /// Это количество может не совпадать с актульным количеством, если отложенное обновление еще не выполнено.
    /// </summary>
    public int Count { get { return _Panels.Count; } }

    bool ICollection<EFPStatusBarPanels>.IsReadOnly { get { return false; } }

    /// <summary>
    /// Реализация ICollection
    /// </summary>
    /// <param name="item">Удаляемая коллекция панелек</param>
    /// <returns>Результат удаления</returns>
    public bool Remove(EFPStatusBarPanels item)
    {
      if (_Panels.Remove(item))
      {
        _Modified = true;
        return true;
      }
      else
        return false;
    }

    #endregion

    #region IEnumerable<EFPStatusBarPanels> Members

    /// <summary>
    /// Возвращаеи перечислитель по коллекциям панелек EFPStatusBarPanels, а не по панелькам.
    /// </summary>
    /// <returns>Перечислитель</returns>
    public IEnumerator<EFPStatusBarPanels> GetEnumerator()
    {
      return _Panels.GetEnumerator();
    }

    #endregion

    #region IEnumerable Members

    IEnumerator IEnumerable.GetEnumerator()
    {
      return _Panels.GetEnumerator();
    }

    #endregion

    #region IEFPAppIdleHandler Members

    void IEFPAppIdleHandler.HandleIdle()
    {
      UpdateIfModified();

      //StatusStripControl_MouseMove(null, null);
      try
      {
        if (_TheToolTip != null)
        {
          if (!StatusStripControl.IsDisposed)
          {
            Point Pos1 = Cursor.Position;
            Point Pos2 = StatusStripControl.PointToClient(Pos1);
            InitToolTip(StatusStripControl.GetItemAt(Pos2));
          }
        }
      }
      catch { }
    }

    #endregion

    #region Обновление статусной строки

    private EFPStatusBarPanels[] _ShownPanels;

    /// <summary>
    /// Выполняет обновление статусной строки, если в списке произощли изменения с момента последнего вызова
    /// </summary>
    public void UpdateIfModified()
    {
      if (!_Modified)
        return;
      _Modified = false;
      if (StatusStripControl.IsDisposed)
        return;

      EFPStatusBarPanels[] WantedPanels = _Panels.ToArray();

      #region 1. Удаление ненужных более элементов

      for (int i = 0; i < _ShownPanels.Length; i++)
      {
        if (Array.IndexOf<EFPStatusBarPanels>(WantedPanels, _ShownPanels[i]) < 0)
        {
          foreach (EFPUIStatusPanelObj obj in _ShownPanels[i].Items)
          {
            try
            {
              StatusStripControl.Items.Remove(obj.StatusLabel);
            }
            catch (Exception e)
            {
              EFPApp.ShowException(e, "Ошибка отсоединения панели статусной строки " + obj.ToString());
            }
          }
        }
      }

      #endregion

      #region 2. Добавление недостающих панелей

      for (int i = 0; i < WantedPanels.Length; i++)
      {
        if (Array.IndexOf<EFPStatusBarPanels>(_ShownPanels, WantedPanels[i]) < 0)
        {
          int pos = 0;
          // Все элементы добавляются перед уже существующими, но в порядке,
          // в котором они добавлены в этот набор
          foreach (EFPUIStatusPanelObj obj in WantedPanels[i].Items)
          {
            StatusStripControl.Items.Insert(pos, obj.StatusLabel);
            //if (ClientMainForm.MainForm.TheStatusBar.RenderMode == ToolStripRenderMode.System)
            obj.StatusLabel.BackColor = SystemColors.Control;
            //        ClientMainForm.MainForm.TheStatusBar.Items.Add(obj.StatusLabel);

            pos++;
          }
        }
      }

      #endregion

      _ShownPanels = WantedPanels;
    }

    #endregion

    #region Всплывающая подсказка

    /// <summary>
    /// Объект для вывода подсказок
    /// </summary>
    private ToolTip _TheToolTip;

    /// <summary>
    /// Текущая панелька, для которой выведена подсказка
    /// </summary>
    private ToolStripItem _CurrentToolTipItem;


    private void InitToolTip(ToolStripItem item)
    {
      if (_TheToolTip==null)
        return;

      if (item == null)
        _TheToolTip.Hide(StatusStripControl);
      else if (!Object.ReferenceEquals(item, _CurrentToolTipItem))
      {
        if (String.IsNullOrEmpty(item.ToolTipText))
          _TheToolTip.Hide(StatusStripControl);
        else
        {
          // Количество строк в подсказке влияет на размещение
          int LineCount = 1;
          foreach (char ch in item.ToolTipText)
          {
            if (ch == '\n')
              LineCount++;
          }

          // Выводим подсказку над статусной строкой
          Point Pos = new Point(item.Bounds.Left, -StatusStripControl.Height - (LineCount - 1) * StatusStripControl.Font.Height);
          _TheToolTip.Show(item.ToolTipText, StatusStripControl, Pos, _TheToolTip.AutoPopDelay);
        }
      }
      _CurrentToolTipItem = item;
    }


    #endregion
  }

  /// <summary>
  /// Класс для управления статусной строкой приложения в целом.
  /// Реализует свойство EFPApp.StatusBar
  /// </summary>
  public sealed class EFPAppStatusBar : IEFPStatusBarControl
  {
    #region Защищенный конструктор

    internal EFPAppStatusBar()
    {
      _Visible = true;
    }

    #endregion

    #region Свойство StatusStripControl

    /// <summary>
    /// Управляющий элемент статусной строки
    /// Свойство может быть установлено для отображения статусной строки не в главном окне программы
    /// </summary>
    public StatusStrip StatusStripControl
    {
      get { return _StatusStripControl; }
      set
      {
        if (value == _StatusStripControl)
          return;
        _StatusStripControl = value;
        if (_StatusStripControl != null)
          _StatusStripControl.Visible = Visible;

        if (_StatusBarHandler != null)
        {
          _StatusBarHandler.Dispose();
          _StatusBarHandler = null;
        }
        if (_StatusStripControl != null)
        {
          _StatusStripControl.Visible = Visible;
          _StatusBarHandler = new EFPStatusBarHandler(_StatusStripControl);
          _StatusBarHandler.UpdateIfModified();
        }
      }
    }
    private StatusStrip _StatusStripControl;

    #endregion

    #region Свойство Visible

    /// <summary>
    /// Управляет видимостью статусной строки, например, для реализации команды главного меню "Вид - Статусная строка"
    /// По умолчанию строка видна
    /// </summary>
    public bool Visible
    {
      get { return _Visible; }
      set
      {
        if (value == _Visible)
          return;
        _Visible = value;
        if (StatusStripControl != null)
          StatusStripControl.Visible = value;
        if (VisibleChanged != null)
          VisibleChanged(this, EventArgs.Empty);
        if (!EFPApp.InsideLoadComposition)
          EFPApp.SetInterfaceChanged(); // 11.12.2018
      }
    }
    private bool _Visible;

    /// <summary>
    /// Вызывается при изменении видимости статусной строки с помощью свойства Visible.
    /// Установка свойства Control не вызывает появление события
    /// </summary>
    public event EventHandler VisibleChanged;

    #endregion

    #region VisibleCommandItem

    /// <summary>
    /// Создает команду для меню "Вид"-"Панель инструментов", которая позволяет включать или отключать
    /// отображение статусной строки в главном окне программы
    /// </summary>
    /// <param name="parentItem">Родительское меню (обычно, "Вид")</param>
    /// <returns>Созданная команда</returns>
    public EFPCommandItem CreateVisibleCommandItem(EFPCommandItem parentItem)
    {
      VisibleCommandItem ci = new VisibleCommandItem();
      ci.Parent = parentItem;
      return ci;
    }

#pragma warning disable 0618 // обход [Obsolete]
    private class VisibleCommandItem : EFPCommandItem
    {
      #region Конструкторы

      public VisibleCommandItem()
        : base("", "StatusBarVisible")
      {
        MenuText = "Статусная строка";
        Checked = EFPApp.StatusBar.Visible;
        Click += new EventHandler(VisibleClick);
        EFPApp.StatusBar.VisibleChanged += new EventHandler(StatusBarVisibleChanged);
      }

      public override EFPCommandItem Clone()
      {
        return new VisibleCommandItem();
      }

      #endregion

      #region Обработчики событий

      void VisibleClick(object sender, EventArgs args)
      {
        EFPApp.StatusBar.Visible = !EFPApp.StatusBar.Visible;
      }

      void StatusBarVisibleChanged(object sender, EventArgs args)
      {
        Checked = EFPApp.StatusBar.Visible;
      }

      #endregion
    }
#pragma warning restore 0618 

    #endregion

    #region IEFPStatusBarControl Members

    /// <summary>
    /// Объект статусной строки
    /// </summary>
    public EFPStatusBarHandler StatusBarHandler { get { return _StatusBarHandler; } }
    private EFPStatusBarHandler _StatusBarHandler;

    #endregion
  }
}
