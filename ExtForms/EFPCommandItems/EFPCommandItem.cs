// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using FreeLibSet.DependedValues;
using FreeLibSet.Core;
using FreeLibSet.Collections;
using FreeLibSet.Controls;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Базовый класс для реализации посредников между списком тем и
  /// управляющими элементами Windows (MenuItem, SpeedButton)
  /// </summary>
  public abstract class EFPUIObjBase : SimpleDisposableObject, IObjectWithCode
  {
    // 03.01.2021
    // Можно использовать базовый класс без деструктора

    #region Конструктор и реализация IDispose

    /// <summary>
    /// Конструктор добавляет текущий объект в списки Owner и в Item.UIObjs
    /// </summary>
    /// <param name="owner">Список-владелец</param>
    /// <param name="item">Команда</param>
    protected EFPUIObjBase(EFPUIObjs owner, EFPCommandItem item)
    {
#if DEBUG
      if (owner == null)
        throw new ArgumentNullException("owner");
      if (item == null)
        throw new ArgumentNullException("item");
#endif

      _Code = item.Category + "|" + item.Name; // ключ для NamedList

      _Item = item;
      _Item.AddUIObj(this);

      _Owner = owner;
      owner._UIObjs.Add(this);
    }

    /// <summary>
    ///  Удаляет текущий объект из EFPCommandItem
    /// </summary>
    /// <param name="disposing">true, если был вызван метод Dispose(), а не деструктор</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (_Item != null)
        {
          _Item.RemoveUIObj(this);
          _Item = null;
        }
      }
      base.Dispose(disposing);
    }

    #endregion

    #region Общие свойства

    /// <summary>
    /// Элемент EFPCommandItem, к которому привязан данный объект
    /// </summary>
    public EFPCommandItem Item { get { return _Item; } }
    private EFPCommandItem _Item;

    /// <summary>
    /// Коллекция - владелец
    /// </summary>
    public EFPUIObjs Owner { get { return _Owner; } }
    private EFPUIObjs _Owner;

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      if (_Item == null)
        return "Не присоединена";
      else
        return _Item.ToString();
    }

    #endregion

    #region Переопределяемые виртуальные методы

    /// <summary>
    /// Установить текст меню
    /// </summary>
    public virtual void SetMenuText() { }

    /// <summary>
    /// Установить видимость элемента
    /// </summary>
    public virtual void SetVisible() { }

    /// <summary>
    /// Установить доступность элемента
    /// </summary>
    public virtual void SetEnabled() { }

    /// <summary>
    /// Установить отметку
    /// </summary>
    public virtual void SetChecked() { }

    /// <summary>
    /// Установить изображение по свойствам ImageKey или Image
    /// </summary>
    public virtual void SetImage() { }

    /// <summary>
    /// Установить всплывающую подсказку
    /// </summary>
    public virtual void SetToolTipText() { }

    /// <summary>
    /// Установить текст в панели статусной строки
    /// </summary>
    public virtual void SetStatusBarText() { }

    /// <summary>
    /// Установить все свойства
    /// </summary>
    public virtual void SetAll()
    {
      SetMenuText();
      SetVisible();
      SetEnabled();
      SetChecked();
      SetImage();
      SetToolTipText();
      SetStatusBarText();
    }

    #endregion

    #region Внутренняя реализация

    /// <summary>
    /// Используется для реализации связанного списка команд
    /// </summary>
    internal EFPUIObjBase NextUIObj;

    /// <summary>
    /// Реализация обработчиков Click из темы меню
    /// </summary>
    protected void ClickEvent(object sender, EventArgs args)
    {
      if (Item == null)
        return;
      if (Item.InsideClick)
      {
        EFPApp.ShowTempMessage("Предыдущее выполнение этой команды еще не закончено");
        return;
      }

      if (Item.HasChildren)
        return; // 19.07.2018 - дочерние элементы могли быть созданы позднее

      Item.PerformClick();
    }

    #endregion

    #region IObjectWithCode Members

    string IObjectWithCode.Code { get { return _Code; } }
    /// <summary>
    /// Свойство Item очищается при вызове Dispose(), поэтому надо запоминать код независимо.
    /// </summary>
    private string _Code;

    #endregion
  }

  #region Перечисление EFPCommandItemUsage

  /// <summary>
  /// Где применить команду (свойство EFPCommandItem.Usage)
  /// </summary>
  [FlagsAttribute]
  public enum EFPCommandItemUsage
  {
    /// <summary>
    /// Добавить команду меню, если свойство MenuText установлен
    /// </summary>
    Menu = 0x0001,

    /// <summary>
    /// Добавить кнопку в панель инструментов, если свойство ImageKey установлено
    /// </summary>
    ToolBar = 0x0002,

    /// <summary>
    /// Обрабатывать комбинацию клавиш, если она задана
    /// </summary>
    ShortCut = 0x0004,

    /// <summary>
    /// Добавить окно в статусной панели, если свойство StatusText установлено
    /// </summary>
    StatusBar = 0x0008,

    /// <summary>
    /// Не применять команду. Установите это значение, чтобы отменить действие уже
    /// добавленной команды. Для команд, имеющих свойство Master, присоединение к
    /// родительской команде не выполняется
    /// </summary>
    None = 0x0000,

    /// <summary>
    /// Значение по умолчанию - применять везде, где возможно
    /// </summary>
    Everywhere = 0x000F,

    /// <summary>
    /// Запретить вывод подсказки о горячей клавише или MenuRightText во всплывающей
    /// подсказке к кнопке или панели статусной строки
    /// </summary>
    DisableRightTextInToolTip = 0x0010,

    /// <summary>
    /// Если команда является подменю, то она добавляется как "уголочек" в панель инструментов.
    /// Это не зависит от наличия или отсутствия изображения (свойства Image и ImageKey)
    /// </summary>
    ToolBarDropDown = 0x0020,
  }

  #endregion

  #region Делегаты

  /// <summary>
  /// Аргументы события EFPCommandItems.BeforeClick
  /// </summary>
  public class EFPCommandItemBeforeClickEventArgs : CancelEventArgs
  {
    #region Конструктор

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="item">Команда меню</param>
    public EFPCommandItemBeforeClickEventArgs(EFPCommandItem item)
    {
      _Item = item;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Выполняющаяся команда меню
    /// </summary>
    public EFPCommandItem Item { get { return _Item; } }
    private EFPCommandItem _Item;

    #endregion
  }

  /// <summary>
  /// Делегат события EFPCommandItems.BeforeClick
  /// </summary>
  /// <param name="sender">Список, к которому принадлежит команда</param>
  /// <param name="args">Аргументы события</param>
  public delegate void EFPCommandItemBeforeClickEventHandler(object sender, EFPCommandItemBeforeClickEventArgs args);

  /// <summary>
  /// Аргументы события EFPCommandItems.AfterClick
  /// </summary>
  public class EFPCommandItemAfterClickEventArgs : EventArgs
  {
    #region Конструктор

    /// <summary>
    /// Конструктор
    /// </summary>
    /// <param name="item">Команда меню</param>
    public EFPCommandItemAfterClickEventArgs(EFPCommandItem item)
    {
      _Item = item;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Выполняющаяся команда меню
    /// </summary>
    public EFPCommandItem Item { get { return _Item; } }
    private EFPCommandItem _Item;

    #endregion
  }

  /// <summary>
  /// Делегат события EFPCommandItems.AfterClick
  /// </summary>
  /// <param name="sender">Список, к которому принадлежит команда</param>
  /// <param name="args">Аргументы события</param>
  public delegate void EFPCommandItemAfterClickEventHandler(object sender, EFPCommandItemAfterClickEventArgs args);

  #endregion

  /*
   * 13.07.2018
   * Иерархия команд.
   * EFPCommandItem задает одну команду меню
   * EFPCommandItems является коллекцией всех элементов, независимо от их вложенности
   * 
   * EFPCommandItems.Add() добавляет команду на нужный уровень, ориентируясь на свойство EFPCommandItem.Parent.
   * В момент добавления команды устанавливается свойство EFPCommandItem.Owner типа EFPCommandItems.
   * Повторное добавление команды генерирует ошибку.
   * 
   * Список команд одного уровня доступен через свойства EFPCommandItem.Children и EFPCommandItems.TopLevelItems
   * соответственно. Этот список имеет тип EFPCommandOneLevelItems.
   * 
   * Иерархия не связана со свойствами EFPCommandItem.Master и Servant.
   */

  /// <summary>
  /// Описание позиции меню, кнопки панели инструментов или ячейки статусной строки.
  /// Если создается производный класс, то он должен переопределять метод Clone() и защищенный конструктор копирования
  /// </summary>
  public class EFPCommandItem : IObjectWithCode, ICloneable
  {
    #region Конструкторы

    /// <summary>
    /// Создание обычной команды меню или подменю, кнопки и/или панели стаусной строки
    /// </summary>
    /// <param name="category">Категория</param>
    /// <param name="name">Условное имя команды</param>
    public EFPCommandItem(string category, string name)
    {
#if DEBUG
      //if (String.IsNullOrEmpty(Category))
      //  throw new ArgumentNullException("Category");
      if (String.IsNullOrEmpty(name))
        throw new ArgumentNullException("name");
#endif

      _CategoryAndName = category + "|" + name;
      _Category = category;
      _Name = name;
      _Usage = EFPCommandItemUsage.Everywhere;
      _MasterActive = false;

      _MenuText = null;
      _Visible = true;
      _Enabled = true;
      _Checked = false;
      _ImageKey = null;
      _ToolTipText = null;
      _GroupBegin = false;
      _GroupEnd = false;

      // Уникальный номер команды для отладки
      _ItemIdCounter++;
      _ItemId = _ItemIdCounter;
    }

    /// <summary>
    /// Создание команды для локального меню, связанной с командой в главном меню (Master-Slave)
    /// </summary>
    /// <param name="master">Команда главного меню</param>
    public EFPCommandItem(EFPCommandItem master)
    {
#if DEBUG
      if (master == null)
        throw new ArgumentNullException("master");
#endif
      _Category = master.Category;
      _Name = master.Name;
      _CategoryAndName = _Category + "|" + _Name;
      _Usage = EFPCommandItemUsage.Everywhere;
      _Master = master;

      _MasterActive = false;

      _MenuText = master._MenuText;
      _ShortCut = master._ShortCut;
      _MenuRightText = master._MenuRightText;
      _Visible = true;
      _Enabled = true;
      _Checked = false;
      _Image = master._Image;
      _ImageKey = master._ImageKey;
      _ToolTipText = master._ToolTipText;
      _MenuRightText = master._MenuRightText;

      // 12.09.2012 - не дублируем
      // FGroupBegin = Master.FGroupBegin;
      // FGroupEnd = Master.FGroupEnd;

      // Уникальный номер команды для отладки
      _ItemIdCounter++;
      _ItemId = _ItemIdCounter;
    }

    #endregion

    #region Клонирование

    object ICloneable.Clone()
    {
      return Clone();
    }

    /// <summary>
    /// Создает копию команды, не присоединенную к какому-либо набору EFPCommandItems.
    /// Дочерние команды не копируются. Ссылка на родительскую команду не копируется.
    /// Не устанавливается свойство Parent. Свойство Master устанавливается.
    /// </summary>
    /// <returns>Новая команда, идентичная текущей</returns>
    public virtual EFPCommandItem Clone()
    {
      return new EFPCommandItem(this, true);
    }

    /// <summary>
    /// Конструктор копирования
    /// </summary>
    /// <param name="source">Копируемая команда. Не может быть null</param>
    /// <param name="dummy">Не используется. Нужно, чтобы отличать от конструктора Master-Slave</param>
    protected EFPCommandItem(EFPCommandItem source, bool dummy)
      : this(source.Category, source.Name)
    {
      this._Usage = source.Usage;
      this._MenuText = source.MenuText;
      this._MenuRightText = source.MenuRightText;
      this._Image = source.Image;
      this._ImageKey = source.ImageKey;
      this._ShortCut = source.ShortCut;
      this._StatusBarText = source.StatusBarText;
      this._Master = source.Master;
      this._Visible = source.Visible;
      this._Enabled = source.Enabled;
      this._Checked = source.Checked;
      this._GroupBegin = source.GroupBegin;
      this._GroupEnd = source.GroupEnd;
      if (source.Click != null)
        this.Click += (EventHandler)(source.Click.Clone());
      if (source.Idle != null)
        this.Idle += (EventHandler)(source.Idle.Clone());
      this._Tag = source.Tag;
    }

    #endregion

    #region Общие свойства

    /// <summary>
    /// Категория. Может быть пустой строкой для элементов строки главного меню.
    /// Используется для поиска и для хранения настроек пользователя (планируется).
    /// Не используется для отображения, в частности, как текст команды меню.
    /// </summary>
    public string Category { get { return _Category; } }
    private readonly string _Category;

    /// <summary>
    /// Условное имя команды. Не может быть пустой строкой
    /// Используется для поиска и для хранения настроек пользователя (планируется).
    /// Не используется для отображения, в частности, как текст команды меню.
    /// Задается в конструкторе. Пара свойств Category и Name должны быть уникальными в пределах EFPCommandItems
    /// </summary>
    public string Name { get { return _Name; } }
    private readonly string _Name;

    /// <summary>
    /// Пользовательские данные.
    /// Их можно использовать внутри события Click
    /// </summary>
    public object Tag { get { return _Tag; } set { _Tag = value; } }
    private object _Tag;

    /// <summary>
    /// Возвращает true, если тема была присоединена к основному набору команд
    /// </summary>
    public bool IsGlobalItem
    {
      get
      {
        return EFPApp.CommandItems.Contains(this);
      }
    }

    #endregion

    #region Применимость команды в пользовательском интерфейсе

    /// <summary>
    /// Набор флагов, определяющих, где может применяться данная команда (в меню, панели инструментов, как горячая клавиша и/или в статусной строке).
    /// По умолчанию - Everywhere (везде).
    /// Заданное разрешение не означат автоматическое наличие команды в соответствующем месте. Для наличия
    /// команды меню требуется непустая строка MenuText, кнопки панели инструментов - ImageKey, 
    /// горячей клавиши - свойство ShortCut, панели статусной строки - StatusBarText.
    /// Для проверки применимости команды используйте свойства MenuUsage, ToolBarUsage, StatusBarUsage и ShortCutUsage
    /// </summary>
    public EFPCommandItemUsage Usage
    {
      get { return _Usage; }
      set
      {
        if (_FirstUIObj != null)
          throw new InvalidOperationException("Свойство Usage нельзя устанавливать при наличии элементов пользовательского интерфейса");
        _Usage = value;
      }
    }
    private EFPCommandItemUsage _Usage;

    /// <summary>
    /// Возвращает true, если данная команда может быть добавлена в меню (установлен флаг Usage.Menu и задана непустая
    /// строка в свойстве MenuText)
    /// </summary>
    public bool MenuUsage
    {
      get
      {
        return ((Usage & EFPCommandItemUsage.Menu) == EFPCommandItemUsage.Menu) &&
          (!String.IsNullOrEmpty(MenuText));
      }
    }

    /// <summary>
    /// Возвращает true, если данная команда может быть добавлена в панель инструментов (установлен флаг Usage.ToolBar и
    /// задана непустая строка в свойстве ImageKey)
    /// </summary>
    public bool ToolBarUsage
    {
      get
      {
        if ((Usage & EFPCommandItemUsage.ToolBarDropDown) == EFPCommandItemUsage.ToolBarDropDown)
          return true;
        return ((Usage & EFPCommandItemUsage.ToolBar) == EFPCommandItemUsage.ToolBar) &&
          ((!String.IsNullOrEmpty(ImageKey)) || Image != null);
      }
    }

    /// <summary>
    /// Возвращает true, если данная команда может быть добавлена в статусную строку (установлен флаг Usage.StatusBar и
    /// задана непустая строка в свойстве StatusBarText)
    /// </summary>
    public bool StatusBarUsage
    {
      get
      {
        return ((Usage & EFPCommandItemUsage.StatusBar) == EFPCommandItemUsage.StatusBar) &&
          (!String.IsNullOrEmpty(StatusBarText));
      }
    }

    /// <summary>
    /// Возвращает true, если данная команда имеет назначенную комбинацию клавиш (установлен флаг Usage.ShortCut и
    /// свойство ShortCut отлично от None)
    /// </summary>
    public bool ShortCutUsage
    {
      get
      {
        return ((Usage & EFPCommandItemUsage.ShortCut) == EFPCommandItemUsage.ShortCut) &&
          (ShortCut != Keys.None);
      }
    }

    #endregion

    #region Объект - владелец

    /// <summary>
    /// Список, содержащий все команды меню, независимо от иерархии.
    /// Свойство инициализируется при вызове EFPCommandItems.Add()
    /// </summary>
    public EFPCommandItems Owner
    {
      get { return _Owner; }
      internal set { _Owner = value; }
    }
    private EFPCommandItems _Owner;

    /// <summary>
    /// Возвращает список команд одного уровня, к которому добавлена команда.
    /// Возвращает null, если оба свойства Parent и Owner равны null
    /// </summary>
    public EFPOneLevelCommandItems OneLevelItems
    {
      get
      {
        if (Parent != null)
          return Parent.Children;
        else if (Owner != null)
          return Owner.TopLevelItems;
        else
          return null;
      }
    }

    #endregion

    #region Родительская и дочерние команды

    /// <summary>
    /// Родительский элемент для получения иерархии меню. Задает подменю, в котором расположена команда,
    /// если она не является командой верхнего уровня
    /// </summary>
    public EFPCommandItem Parent
    {
      get { return _Parent; }
      set
      {
        if (value == _Parent)
          return;

        if (_Parent != null)
          _Parent.Children.Items.Remove(this);
        //else if (FOwner != null)
        //  FOwner.TopLevelItems.Items.Remove(this);

        _Parent = value;

        if (_Parent != null)
          _Parent.Children.Items.Add(this);
        //else if (FOwner != null)
        //  FOwner.TopLevelItems.Items.Add(this);
      }
    }
    private EFPCommandItem _Parent;

    /// <summary>
    /// Возвращает корневой элемент, путем рекурсивного опроса свойства Parent.
    /// Если команда является элементом верхнего уровня (Parent=null), возвращает null
    /// </summary>
    public EFPCommandItem Root
    {
      get
      {
        if (Parent == null)
          return null;

        EFPCommandItem ci = Parent;
        while (true)
        {
          if (ci.Parent == null)
            break;
          ci = ci.Parent;
        }
        return ci;
      }
    }

    /// <summary>
    /// Команды, дочерние по отношению к данной (то есть команды подменю).
    /// Для определения, является ли данный объект подменю, используйте свойство HasChildren,
    /// чтобы избежать создания лишнего внутреннего списка
    /// </summary>
    public EFPOneLevelCommandItems Children
    {
      get
      {
        if (_Children == null)
          _Children = new EFPOneLevelCommandItems();
        return _Children;
      }
    }
    private EFPOneLevelCommandItems _Children;


    /// <summary>
    /// Возвращает true, если есть дочерние команды меню (даже со сброшенным свойством
    /// MenuUsage). Совпадает с (Children.Count!=0), но не требует создания дополнительного объекта
    /// </summary>
    public bool HasChildren
    {
      get
      {
        if (_Children == null)
          return false;
        else
          return _Children.Count != 0;
      }
    }

    /// <summary>
    /// Возвращает свойство Children.Count без создания дополнительного объекта
    /// </summary>
    public int ChildCount
    {
      get
      {
        if (_Children == null)
          return 0;
        else
          return _Children.Count;
      }
    }

    /// <summary>
    /// Возвращает количество действующих тем подменю. Учитывает свойство MenuUsage
    /// для дочерних команд
    /// Свойство может быть использовано для установки свойства Usage этой команды,
    /// если подменю не содержит ни одной команды
    /// </summary>
    public int MenuChildrenCount
    {
      get
      {
        if (_Children == null)
          return 0;

        int res = 0;
        for (int i = 0; i < _Children.Count; i++)
        {
          if (_Children[i].MenuUsage)
            res++;
        }
        return res;
      }
    }

    #endregion

    #region Следующий и предыдущий элементы

    ///// <summary>
    ///// Возвращает следующий элемент на том же уровне иерархии.
    ///// Возвращает null, если элемента нет
    ///// </summary>
    //public EFPCommandItem Next
    //{
    //  get
    //  {
    //    if (OneLevelItems == null)
    //      return null;
    //    else
    //    {
    //      int p = OneLevelItems.Items.IndexOf(this);
    //      if (p < 0)
    //        throw new BugException("Не нашли себя в списке");
    //      if (p >= OneLevelItems.Items.Count)
    //        return null;
    //      else
    //        return OneLevelItems.Items[p + 1];
    //    }
    //  }
    //}

    ///// <summary>
    ///// Возвращает предыдущий элемент на том же уровне иерархии.
    ///// Возвращает null, если элемента нет
    ///// </summary>
    //public EFPCommandItem Previous
    //{
    //  get
    //  {
    //    if (OneLevelItems == null)
    //      return null;
    //    else
    //    {
    //      int p = OneLevelItems.Items.IndexOf(this);
    //      if (p < 0)
    //        throw new BugException("Не нашли себя в списке");
    //      if (p == 0)
    //        return null;
    //      else
    //        return OneLevelItems.Items[p - 1];
    //    }
    //  }
    //}

    #endregion

    #region Свойства команды

    #region Тексты

    /// <summary>
    /// Текст команды меню.
    /// Свойство должно быть установлено, чтобы команда появилась в меню (вместе с флагом Usage.Menu).
    /// Может содержать символ "амперсанд" для подчеркивания буквы
    /// </summary>
    public string MenuText
    {
      get
      {
        if (Servant == null || Servant.MenuText == null)
          return _MenuText;
        return Servant.MenuText;
      }
      set
      {
        if (value == _MenuText)
          return;
        _MenuText = value;
        SetMenuText();
        if (MasterActive)
          Master.SetMenuText();
      }
    }
    private string _MenuText;

    /// <summary>
    /// Возвращает текст команды меню (свойство MenuText) без символа "амперсанд" для 
    /// подчеркивания буквы
    /// </summary>
    public string MenuTextWithoutMnemonic
    {
      get
      {
        return WinFormsTools.RemoveMnemonic(MenuText);
      }
    }

    /// <summary>
    /// Текст всплывающей подсказки, используемый при наведении мыши на кнопку панели инструментов или
    /// панель статусной строки
    /// </summary>
    public string ToolTipText
    {
      get
      {
        if (Servant == null || Servant.ToolTipText == null)
          return _ToolTipText;
        return Servant.ToolTipText;
      }
      set
      {
        if (value == _ToolTipText)
          return;
        _ToolTipText = value;
        SetToolTipText();
        if (MasterActive)
          Master.SetToolTipText();
      }
    }
    private string _ToolTipText;

    /// <summary>
    /// Специальное значение для StatusBarText, задающее панель без текста
    /// </summary>
    public const string EmptyStatusBarText = " ";

    /// <summary>
    /// Текст для панели в статусной строке
    /// Используйте специальное значение EmptyStatusBarText, чтобы создать панель,
    /// содержащую только значок
    /// </summary>
    public string StatusBarText
    {
      get
      {
        if (Servant == null || Servant.StatusBarText == null)
          return _StatusBarText;
        return Servant.StatusBarText;
      }
      set
      {
        if (value == _StatusBarText)
          return;
        _StatusBarText = value;
        SetStatusBarText();
        if (MasterActive)
          Master.SetStatusBarText();
      }
    }
    private string _StatusBarText;

    /// <summary>
    /// Возвращает читаемое представление команды
    /// </summary>
    public string DisplayName
    {
      get
      {
        StringBuilder sb = new StringBuilder();
        if (MenuUsage)
        {
          EFPCommandItem currItem = this;
          while (currItem != null)
          {
            if (sb.Length > 0)
              sb.Insert(0, "-");
            if (!String.IsNullOrEmpty(currItem.MenuText))
              sb.Insert(0, currItem.MenuTextWithoutMnemonic);
            currItem = currItem.Parent;
          }
        }
        if (ShortCutUsage)
        {
          if (sb.Length > 0)
          {
            sb.Append(" (");
            sb.Append(ShortCutText);
            sb.Append(")");
          }
          else
            sb.Append(ShortCutText);
        }
        if (sb.Length == 0)
        {
          if (ToolBarUsage)
            sb.Append(ToolTipText);
          else
          {
            if (StatusBarUsage)
              sb.Append("[окно статусной строки]");
          }
        }
        if (sb.Length == 0)
          return ToString();
        else
          return sb.ToString();
      }
    }

    #endregion

    #region Флажки Visible, Enabled, Checked

    #region Visible

    /// <summary>
    /// Видимость команды (по умолчанию true).
    /// Если сброшено в false, то скрываются соответствующие элементы интерфейса и перестает действовать
    /// комбинация клавиш, заданная свойством ShortCut
    /// </summary>
    public bool Visible
    {
      get
      {
        if (Servant == null)
          return _Visible;
        return Servant.Visible;
      }
      set
      {
        if (value == _Visible)
          return;
        _Visible = value;
        SetVisible();
        if (Parent != null)
        {
          bool HasVisible = false;
          for (int i = 0; i < Parent.Children.Count; i++)
          {
            if (Parent.Children[i].Visible)
            {
              HasVisible = true;
              break;
            }
          }
          Parent.Visible = HasVisible;
        }
        if (MasterActive)
        {
          Master.SetVisible();
          //if (Master.Parent != null)
          //{
          //  bool HasVisible = false;
          //  for (int i = 0; i < MasterParent.Children.Count; i++)
          //  {
          //    if (Parent.Children[i].Visible)
          //    {
          //      HasVisible = true;
          //      break;
          //    }
          //  }
          //  Parent.Visible = HasVisible;
          //}

          if (_VisibleEx != null)
            _VisibleEx.Value = value;
        }
      }
    }
    private bool _Visible;

    /// <summary>
    /// Управляемое свойство для Visible.
    /// </summary>
    public DepValue<bool> VisibleEx
    {
      get
      {
        InitVisibleEx();
        return _VisibleEx;
      }
      set
      {
        InitVisibleEx();
        _VisibleEx.Source = value;
      }
    }
    private DepInput<bool> _VisibleEx;

    private void InitVisibleEx()
    {
      if (_VisibleEx == null)
      {
        _VisibleEx = new DepInput<bool>(Visible,VisibleEx_ValueChanged);
        _VisibleEx.OwnerInfo = new DepOwnerInfo(this, "VisibleEx");
      }
    }

    void VisibleEx_ValueChanged(object sender, EventArgs args)
    {
      Visible = _VisibleEx.Value;
    }

    #endregion

    #region Enabled

    /// <summary>
    /// Доступность команды (по умолчанию - true).
    /// Если сброшено в false, то команда и кнопка панели инструментов делается неактивной.
    /// Комбинация клавиш, заданная свойством ShortCut перестает действовать. Перестает действовать
    /// двойной щелчок на панели статусной строки
    /// </summary>
    public bool Enabled
    {
      get
      {
        if (Servant == null)
          return _Enabled;
        return Servant.Enabled;
      }
      set
      {
        if (value == _Enabled)
          return;
        _Enabled = value;
        SetEnabled();
        if (MasterActive)
          Master.SetEnabled();
        if (_EnabledEx != null)
          _EnabledEx.Value = value;
      }
    }
    private bool _Enabled;


    /// <summary>
    /// Управляемое свойство для Enabled.
    /// </summary>
    public DepValue<bool> EnabledEx
    {
      get
      {
        InitEnabledEx();
        return _EnabledEx;
      }
      set
      {
        InitEnabledEx();
        _EnabledEx.Source = value;
      }
    }
    private DepInput<bool> _EnabledEx;

    private void InitEnabledEx()
    {
      if (_EnabledEx == null)
      {
        _EnabledEx = new DepInput<bool>(Enabled,EnabledEx_ValueChanged);
        _EnabledEx.OwnerInfo = new DepOwnerInfo(this, "EnabledEx");
      }
    }

    void EnabledEx_ValueChanged(object sender, EventArgs args)
    {
      Enabled = _EnabledEx.Value;
    }

    #endregion

    #region Checked

    /// <summary>
    /// Отметка команды. Команда меню отмечается "галочкой", кнопка панели инструментов становится "нажатой"
    /// </summary>
    public bool Checked
    {
      get
      {
        if (Servant == null)
          return _Checked;
        return Servant.Checked;
      }
      set
      {
        if (value == _Checked)
          return;
        _Checked = value;
        SetChecked();
        if (MasterActive)
          Master.SetChecked();
        if (_CheckedEx != null)
          _CheckedEx.Value = value;
      }
    }
    private bool _Checked;

    /// <summary>
    /// Управляемое свойство для Checked.
    /// </summary>
    public DepValue<bool> CheckedEx
    {
      get
      {
        InitCheckedEx();
        return _CheckedEx;
      }
      set
      {
        InitCheckedEx();
        _CheckedEx.Source = value;
      }
    }
    private DepInput<bool> _CheckedEx;

    private void InitCheckedEx()
    {
      if (_CheckedEx == null)
      {
        _CheckedEx = new DepInput<bool>(Checked,CheckedEx_ValueChanged);
        _CheckedEx.OwnerInfo = new DepOwnerInfo(this, "CheckedEx");
      }
    }

    void CheckedEx_ValueChanged(object sender, EventArgs args)
    {
      Checked = _CheckedEx.Value;
    }

    #endregion

    #endregion

    #region Image и ImageKey

    /// <summary>
    /// Значок кнопки панели инструментов, около команды меню и в панели статусной строки.
    /// Изображение выбирается из списка EFPApp.MainImages. Для локальтного меню, установка непустого значения 
    /// свойства приводит к появлению кнопки в панели инструментов. Чтобы недопустить появления кнопки, а использовать
    /// изображение только для меню и/или панели статусной строки, следует использовать свойство Usage
    /// </summary>
    public string ImageKey
    {
      get
      {
        if (Servant == null || Servant.ImageKey == null)
          return _ImageKey;
        return Servant.ImageKey;
      }
      set
      {
        if (value == _ImageKey)
          return;
        _ImageKey = value;
        SetImage();
        if (MasterActive)
          Master.SetImage();
      }
    }
    private string _ImageKey;

    /// <summary>
    /// Значок кнопки панели инструментов, около команды меню и в панели статусной строки.
    /// Это позволяет задавать произвольное изображение, не обязательно из списка EFPApp.MainImages. 
    /// Для локальтного меню, установка непустого значения свойства приводит к появлению кнопки в 
    /// панели инструментов. Чтобы недопустить появления кнопки, а использовать
    /// изображение только для меню и/или панели статусной строки, следует использовать свойство Usage.
    /// Обычно рекомендуется использовать свойство ImageKey
    /// </summary>
    public Image Image
    {
      get
      {
        if (Servant == null || Servant.ImageKey == null)
          return _Image;
        return Servant.Image;
      }
      set
      {
        if (value == _Image)
          return;
        _Image = value;
        SetImage();
        if (MasterActive)
          Master.SetImage();
      }
    }
    private Image _Image;

    /// <summary>
    /// Возвращает true, если установлено свойство ImageKey или Image.
    /// </summary>
    public bool HasImage
    {
      get
      {
        return (!String.IsNullOrEmpty(ImageKey)) ||
          Image != null;
      }
    }

    #endregion

    #region ShortCut

    /// <summary>
    /// Сочетание клавиш, нажатие которых приводит к выполнению команды.
    /// По умолчанию содержит значение None
    /// </summary>
    public Keys ShortCut
    {
      get
      {
        // ShortCut'ы берем из главного меню
        //     if (Master != null)
        //       return Master.ShortCut;
        return _ShortCut;
      }
      set
      {
        CheckNoRun();
        _ShortCut = value;
      }
    }
    private Keys _ShortCut;

    /// <summary>
    /// Проверить комбинацию клавиш.
    /// Проверяются свойства ShortCut и Usage. Свойства Visible и Enabled не проверяются в этом методе
    /// Возвращает true для альтернативных команд. Если ShortCut задает Ctrl-Z, а проверяемое 
    /// значение value=Alt-Backspace, то возвращается true. Аналогично проверяются ShortCut=Ctrl-Y, Ctrl-C, Ctrl-V и Ctrl-X
    /// </summary>
    /// <param name="value">Проверяемая комбинация</param>
    /// <returns>true, если комбинация совпадает</returns>
    public bool IsShortCut(Keys value)
    {
      if ((Usage & EFPCommandItemUsage.ShortCut) == EFPCommandItemUsage.None)
        return false; // заблокирована

      Keys myKey = ShortCut;
      if (myKey == Keys.None)
        return false;

      // 10.11.2012 Альтернативные комбинации для меню "Правка"
      switch (value)
      {
        case Keys.Alt | Keys.Back:
          if (myKey == (Keys.Control | Keys.Z))
            return true;
          break;
        case Keys.Alt | Keys.Shift | Keys.Back:
          if (myKey == (Keys.Control | Keys.Y))
            return true;
          break;
        case Keys.Control | Keys.Insert:
          if (myKey == (Keys.Control | Keys.C))
            return true;
          break;
        case Keys.Shift | Keys.Insert:
          if (myKey == (Keys.Control | Keys.V))
            return true;
          break;
        case Keys.Shift | Keys.Delete:
          if (myKey == (Keys.Control | Keys.X))
            return true;
          break;
      }

      // Сравниваем Ctrl, Alt и Shift
#if XXX
      bool hasAlt1 = (value & Keys.Alt) != Keys.None;
      bool hasAlt2 = (myKey & Keys.Alt) != Keys.None;

      bool hasCtrl1 = (value & Keys.Control) != Keys.None;
      bool hasCtrl2 = (myKey & Keys.Control) != Keys.None;

      bool hasShift1 = (value & Keys.Shift) != Keys.None;
      bool hasShift2 = (myKey & Keys.Shift) != Keys.None;

      if (!(hasAlt1 == hasAlt2 && hasCtrl1 == hasCtrl2 && hasShift1 == hasShift2))
        return false;
#endif
      // 17.03.2022. Оптимизация
      Keys modifiers1 = value & (Keys.Alt | Keys.Control | Keys.Shift);
      Keys modifiers2 = myKey & (Keys.Alt | Keys.Control | Keys.Shift);
      if (modifiers1 != modifiers2)
        return false;


      // Сравниваем код клавиши
      Keys code1 = value & Keys.KeyCode;
      Keys code2 = myKey & Keys.KeyCode;
      return code1 == code2;
    }

    /// <summary>
    /// Текстовое представление для свойства ShortCut.
    /// Свойство MenuRightText сюда не включается
    /// </summary>
    public string ShortCutText
    {
      get
      {
        return GetShortCutText(ShortCut);
      }
    }

    /// <summary>
    /// Статический метод получения текста для ShortCut'а
    /// </summary>
    /// <param name="value">Комбинация клавиш</param>
    /// <returns>Текстовое представление</returns>
    public static string GetShortCutText(Keys value)
    {
      if (value == Keys.None)
        return String.Empty;

      string s = String.Empty;
      if ((value & Keys.Control) != Keys.None)
        s += "Ctrl+";
      if ((value & Keys.Alt) != Keys.None)
        s += "Alt+";
      if ((value & Keys.Shift) != Keys.None)
        s += "Shift+";

      // Некоторые клавиши называются неудачно
      switch (value & Keys.KeyCode)
      {
        case Keys.Space:
          return s + "Пробел";
        case Keys.Return:
          return s + "Enter";
        case Keys.PageDown:
          return s + "PageDown";
        case Keys.PageUp:
          return s + "PageUp";
        case Keys.Up:
          return s + "Стрелка вверх";
        case Keys.Down:
          return s + "Стрелка вниз";
        case Keys.Left:
          return s + "Стрелка влево";
        case Keys.Right:
          return s + "Стрелка вправо";
        case Keys.D0:
          return s + "0";
        case Keys.D1:
          return s + "1";
        case Keys.D2:
          return s + "2";
        case Keys.D3:
          return s + "3";
        case Keys.D4:
          return s + "4";
        case Keys.D5:
          return s + "5";
        case Keys.D6:
          return s + "6";
        case Keys.D7:
          return s + "7";
        case Keys.D8:
          return s + "8";
        case Keys.D9:
          return s + "9";
        case Keys.Add:
          return s + "[+]";
        case Keys.Subtract:
          return s + "[-]";
        case Keys.Multiply:
          return s + "[*]";
        case Keys.Divide:
          return s + "[/]";
        case Keys.OemOpenBrackets:
          return s + "[";
        case Keys.OemCloseBrackets:
          return s + "]";
        case Keys.Oemplus:
          return s + "Верхний [=]";
        case Keys.OemMinus:
          return s + "Верхний [-]";
        case Keys.OemPipe:
          return s + "[\\]";
        case Keys.OemQuestion:
          return s + "[?]";
        case Keys.OemPeriod:
          return s + "[>]";
        case Keys.Oemcomma:
          return s + "[<]";
      }
      return s + (value & Keys.KeyCode).ToString();
    }

    /// <summary>
    /// Текст в правой части строки меню, где обычно находится описание ShortCut.
    /// Можно задать альтернативный текст.
    /// Если свойство не установлено явно, а ShortCut установлен, то свойство возвращает пустую строку,
    /// то есть подстановка не выполняется
    /// </summary>
    public string MenuRightText
    {
      get
      {
        if (Servant == null || Servant.MenuRightText == null)
          return _MenuRightText;
        return Servant.MenuRightText;
      }
      set
      {
        if (value == _MenuRightText)
          return;
        _MenuRightText = value;
        SetMenuText(); // общая команда
        if (MasterActive)
          Master.SetMenuText();
      }
    }
    private string _MenuRightText;

    /// <summary>
    /// Заменяет комбинацию клавиш на эквивалентное ему текстовое представление.
    /// Свойство ShortCut очищается, а MenuRightText - устанавливается
    /// </summary>
    public void ShortCutToRightText()
    {
      if (ShortCut == Keys.None)
        MenuRightText = String.Empty;
      else
      {
        MenuRightText = GetShortCutText(ShortCut);
        ShortCut = Keys.None;
      }
    }

    #endregion

    #endregion

    #region Событие Click

    /// <summary>
    /// Событие вызывается при нажатии кнопки или выборе команды меню.
    /// Возникшее исключение перехватывается и выдается отладочное окно
    /// </summary>
    public event EventHandler Click;

    /// <summary>
    /// Генерация события Click
    /// </summary>
    public void PerformClick()
    {
      try
      {
        DoPerformClick();
      }
      catch (Exception e)
      {
        e.Data["EFPCommandItem"] = this.ToString();
        e.Data["EFPCommandItem.DisplayName"] = DisplayName;
        EFPApp.ShowException(e, "Неперехваченная ошибка при обработке команды. Обратитесь к разработчику программы");
      }
    }

    /// <summary>
    /// Генерация события Click.
    /// </summary>
    internal void PerformClick(object sender, EventArgs args)
    {
      PerformClick();
    }

    private void DoPerformClick()
    {
      if (Servant == null)
      {
        if (!Visible)
        {
          EFPApp.ShowTempMessage("Команда невидима: " + DisplayName);
          return;
        }
        if (!Enabled)
        {
          EFPApp.ShowTempMessage("Команда заблокирована: " + DisplayName);
          return;
        }

        if (EFPApp.OnBeforeCommandItemClick(this)) // 15.01.2017
        {
          bool beforeClickResult = true;
          if (Owner != null) // 19.03.2021
            beforeClickResult = Owner.DoBeforeClick(this);
          if (beforeClickResult)
          {
            if (Click != null)
            {
              EFPApp.ShowTempMessage(null); // очистка статусной строки
              try
              {
                if (InsideClick)
                  throw new InvalidOperationException("Попытка повторного выполнения команды \"" +
                    DisplayName + "\" до завершения предыдущего запуска");
                _InsideClick = true;
                SetEnabled(); // блокирует команды на время выполнения
                try
                {
                  Click(this, null);
                }
                finally
                {
                  _InsideClick = false;
                  SetEnabled();
                }
              }
              catch (Exception e)
              {
                EFPApp.ShowException(e, "Ошибка при обработке команды \"" + DisplayName + "\"");
              }
            }
            else
              EFPApp.ShowTempMessage("Нет обработчика выполнения команды " + DisplayName);

            if (Owner != null)
              Owner.DoAfterClick(this);
            EFPApp.OnAfterCommandItemClick(this);
          }
        }
      }
      else
      {
        // Servant != null
        _InsideClick = true;
        try
        {
          SetEnabled(); // блокирует команды на время выполнения
          Servant.PerformClick();
        }
        finally
        {
          _InsideClick = false;
          SetEnabled();
        }
      }
    }

    /// <summary>
    /// Возвращает true, если в данный момент выполняется PerformClick для этой команды
    /// </summary>
    public bool InsideClick { get { return _InsideClick; } }
    private bool _InsideClick = false;

    /// <summary>
    /// Есть ли добавленные обработчики события Click
    /// </summary>
    public bool HasClick
    {
      get
      {
        return (Click != null);
      }
    }

    /// <summary>
    /// Метод не выполняет никаких действий
    /// </summary>
    /// <param name="sender">Игнорируется</param>
    /// <param name="args">Игнорируется</param>
    public static void NoActionClick(object sender, EventArgs args)
    {
    }

    #endregion

    #region Сепаратор

    /// <summary>
    /// Наличие сепаратора перед этим элементом
    /// </summary>
    public bool GroupBegin
    {
      get
      {
        return _GroupBegin;
      }
      set
      {
        CheckNoRun();
        _GroupBegin = value;
      }
    }
    private bool _GroupBegin;

    /// <summary>
    /// Наличие сепаратора после этого элемента
    /// </summary>
    public bool GroupEnd
    {
      get
      {
        return _GroupEnd;
      }
      set
      {
        CheckNoRun();
        _GroupEnd = value;
      }
    }
    private bool _GroupEnd;

    #endregion

    #region Присоединенные элементы интерфейса

    /// <summary>
    /// Получить количество присоединенных интерфейсных элементов.
    /// Чтение этого свойства выполняется медленно, поэтому не следует использовать
    /// в цикле
    /// </summary>
    public int UIObjectCount
    {
      get
      {
        int n = 0;
        EFPUIObjBase uiObj = _FirstUIObj;
        while (uiObj != null)
        {
          n++;
          uiObj = uiObj.NextUIObj;
        }
        return n;
      }
    }

    /// <summary>
    /// Возвращает список элементов интерфейса (команд меню, кнопок панелей инструментов,
    /// панелей статусной строки), связанных с данной командой.
    /// При каждом обращении к свойству создается новый массив.
    /// </summary>
    /// <returns>Массив</returns>
    public EFPUIObjBase[] GetUIObjects()
    {
      EFPUIObjBase[] res = new EFPUIObjBase[UIObjectCount];
      int index = 0;
      EFPUIObjBase uiObj = _FirstUIObj;
      while (uiObj != null)
      {
        res[index] = uiObj;
        index++;
        uiObj = uiObj.NextUIObj;
      }
      return res;
    }

    #endregion

    #region Отношение хозяин - дочерняя команда

    /// <summary>
    /// Родительская команда при организации цепочек управления
    /// Свойство устанавливается только в конструкторе
    /// Let'VersionStr play Master and Servant ... (DM)
    /// </summary>
    public EFPCommandItem Master { get { return _Master; } }
    private EFPCommandItem _Master;

    /// <summary>
    /// Активная подчиненная команда.
    /// </summary>
    public EFPCommandItem Servant { get { return _Servant; } }
    private EFPCommandItem _Servant;

    private void SetServant(EFPCommandItem value)
    {
      if (value == _Servant)
        return;
      _Servant = value;
      SetAll();
    }


    /// <summary>
    /// Подключение данной команды к Master'у и ее отключение.
    /// Команды с Usage=None не подключаются
    /// </summary>
    public bool MasterActive
    {
      get
      {
        return _MasterActive;
      }
      set
      {
        if (Master == null)
          return;
        if (Usage == EFPCommandItemUsage.None)
          return;
        if (value == _MasterActive)
          return;
        _MasterActive = value;
        if (value)
        {
          // Вставляем себя в начало цепочки
          _NextServant = Master.Servant;
          Master.SetServant(this);
        }
        else
        {
          // Удаляем себя из цепочки. Мы можем быть не первыми в цепочке
          if (Master.Servant == this)
            Master.SetServant(_NextServant);
          else
          {
            EFPCommandItem ci = Master.Servant;
            while (true)
            {
              if (ci == null)
                throw new BugException("Потеряли ссылку на себя при отключении MasterActive");
              if (ci._NextServant == this)
              {
                ci._NextServant = _NextServant;
                break;
              }
              ci = ci._NextServant;
            }
          }
          _NextServant = null; // иначе будут утечки памяти
        }
      }
    }
    private bool _MasterActive;

    /// <summary>
    /// Поле для организации цепочек из нескольких подчиненных команд
    /// </summary>
    private EFPCommandItem _NextServant;

    #endregion

    #region Блокировка подменю

    /// <summary>
    /// Инициализация видимости подменю на основании видимости вложенных команд меню.
    /// Предполагается, что видимость всех "конечных" команд меню определена, а видимость подменю требуется определить
    /// Если текущая команда не является подменю, никаких действий не выполняется.
    /// Если команда - подменю, то выполняется рекурсивный вызов функции для определения видимости всех дочерних
    /// команд. После этого устанавливается свойство Visible этого объекта
    /// </summary>
    public void InitMenuVisible()
    {
      if (!HasChildren)
        return;

      Children.InitMenuVisible(); // рекурсивный вызов

      bool hasVisible = false;
      for (int i = 0; i < Children.Count; i++)
      {
        if (Children[i].Usage == EFPCommandItemUsage.None)
          continue; // 09.07.2019

        if (Children[i].Visible)
        {
          hasVisible = true;
          break;
        }
      }

      this.Visible = hasVisible;
    }

    #endregion

    #region Событие Idle

    /// <summary>
    /// Событие вызывается, когда система свободна, с неопределенной периодичностью.
    /// См. описание события System.Windows.Forms.Application.Idle.
    /// Обработчик может инициализировать доступность команды меню
    /// </summary>
    public event EventHandler Idle;

    /// <summary>
    /// Вызывает событие Idle
    /// </summary>
    public void HandleIdle()
    {
      if (Idle != null)
        Idle(this, EventArgs.Empty);
    }

    /// <summary>
    /// Возвращает true, если есть обработчик события Idle
    /// </summary>
    public bool HasIdle { get { return Idle != null; } }

    #endregion

    #region Событие MenuOpening

    /// <summary>
    /// Событие вызывается при открытии команды меню (главного или выпадающего).
    /// Обработчик события можно использовать для инициализации команды, которая присоединяется только к команде меню,
    /// но не к панели инструментов или статусной строке (Usage=Menu).
    /// В остальных случаях можно использовать событие Idle
    /// </summary>
    public event EventHandler MenuOpening;

    /// <summary>
    /// Вызывает обработчик события MenuOpening.
    /// </summary>
    public void HandleMenuOpening()
    {
      if (MenuOpening != null)
        MenuOpening(this, EventArgs.Empty);
    }

    #endregion

    #region Внутренняя реализация

    /// <summary>
    /// К одной теме может быть присоединено несколько интерфейсных элементов.
    /// Чтобы не добавлять к каждой команде по массиву, используем связанный список
    /// с помощью свойств VisinleClientItem.FirtUIObj и ClientUIObjBase.NextUIObj
    /// </summary>
    private EFPUIObjBase _FirstUIObj;

    internal void AddUIObj(EFPUIObjBase uiObj)
    {
#if DEBUG
      if (uiObj.NextUIObj != null)
        throw new ArgumentException("Поле UIObj.NextUIObj уже установлено", "uiObj");
#endif

      if (_FirstUIObj == null)
        _FirstUIObj = uiObj;
      else
      {
        EFPUIObjBase lastObj = _FirstUIObj;
        while (lastObj.NextUIObj != null)
          lastObj = lastObj.NextUIObj;
        lastObj.NextUIObj = uiObj;
      }
    }

    internal void RemoveUIObj(EFPUIObjBase uiObj)
    {
      if (_FirstUIObj == uiObj)
        _FirstUIObj = uiObj.NextUIObj;
      else
      {
        EFPUIObjBase prevObj = _FirstUIObj;
        while (prevObj.NextUIObj != uiObj)
        {
          prevObj = prevObj.NextUIObj;
#if DEBUG
          if (prevObj == null)
            throw new BugException("Потерялся объект UI");
#endif
        }
        prevObj.NextUIObj = uiObj.NextUIObj;
      }
      uiObj.NextUIObj = null;
    }

    private void SetMenuText()
    {
      EFPUIObjBase uiObj = _FirstUIObj;
      while (uiObj != null)
      {
        uiObj.SetMenuText();
        uiObj = uiObj.NextUIObj;
      }
    }

    private void SetVisible()
    {
      EFPUIObjBase uiObj = _FirstUIObj;
      while (uiObj != null)
      {
        uiObj.SetVisible();
        uiObj = uiObj.NextUIObj;
      }
    }

    private void SetEnabled()
    {
      EFPUIObjBase uiObj = _FirstUIObj;
      while (uiObj != null)
      {
        uiObj.SetEnabled();
        uiObj = uiObj.NextUIObj;
      }
    }

    private void SetChecked()
    {
      EFPUIObjBase uiObj = _FirstUIObj;
      while (uiObj != null)
      {
        uiObj.SetChecked();
        uiObj = uiObj.NextUIObj;
      }
    }

    private void SetImage()
    {
      EFPUIObjBase uiObj = _FirstUIObj;
      while (uiObj != null)
      {
        uiObj.SetImage();
        uiObj = uiObj.NextUIObj;
      }
    }

    private void SetToolTipText()
    {
      EFPUIObjBase uiObj = _FirstUIObj;
      while (uiObj != null)
      {
        uiObj.SetToolTipText();
        uiObj = uiObj.NextUIObj;
      }
    }

    private void SetStatusBarText()
    {
      EFPUIObjBase uiObj = _FirstUIObj;
      while (uiObj != null)
      {
        uiObj.SetStatusBarText();
        uiObj = uiObj.NextUIObj;
      }
    }

    private void SetAll()
    {
      EFPUIObjBase uiObj = _FirstUIObj;
      while (uiObj != null)
      {
        uiObj.SetAll();
        uiObj = uiObj.NextUIObj;
      }
    }

    private void CheckNoRun()
    {
      if (_FirstUIObj != null)
        throw new BugException("Элемент уже имеет связанные объекты пользовательского интерфейса.");
    }

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      string s1, s2;
      if (Category == null)
        s1 = String.Empty;
      else
        s1 = Category;
      s2 = Name;
      return ItemId.ToString() + ": " + s1 + "." + s2;
    }

    /// <summary>
    /// Для реализации коллекций
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
      return _Name.GetHashCode();
    }

    #endregion

    #region Отладочные средства

    /// <summary>
    /// Уникальный номер команды, чтобы можно было отличать команды в цепочках
    /// Master-Servant
    /// </summary>
    public int ItemId { get { return _ItemId; } }
    private readonly int _ItemId;

    private static int _ItemIdCounter = 0;

    #endregion

    #region IObjectWithCode Members

    /// <summary>
    /// Строка в виде "Category|Name".
    /// Используется при добавлении в NamedList.
    /// </summary>
    public string CategoryAndName { get { return _CategoryAndName; } }
    private string _CategoryAndName; // 28.01.2021. Пусть будет лишнее поле, чтобы каждый раз не создавать 

    string IObjectWithCode.Code { get { return _CategoryAndName; } }

    #endregion
  }

  /// <summary>
  /// Список команд одного уровня иерархии
  /// </summary>
  public sealed class EFPOneLevelCommandItems : IEnumerable<EFPCommandItem>
  {
    #region Конструктор

    internal EFPOneLevelCommandItems()
    {
      _Items = new List<EFPCommandItem>();
    }

    #endregion

    #region Список команд

    internal List<EFPCommandItem> Items { get { return _Items; } }
    private List<EFPCommandItem> _Items;

    /// <summary>
    /// Возвращает количество элементов на этом уровне иерархии.
    /// Для подменю следует использовать свойство EFPCommandItem.ChildCount вместо EFPCommandItem.Children.Count,
    /// чтобы избежать создания лишнего объекта
    /// </summary>
    public int Count { get { return _Items.Count; } }

    /// <summary>
    /// Возвращает команду по индексу
    /// </summary>
    /// <param name="index">Индекс команды в пределах списка одного уровня от 0 до Count-1</param>
    /// <returns></returns>
    public EFPCommandItem this[int index] { get { return _Items[index]; } }

    /// <summary>
    /// Добавление сепаратора.
    /// Если в списке уже есть команды, то для последней из них устанавливается
    /// свойство GroupEnd=true
    /// </summary>
    public void AddSeparator()
    {
      if (Count > 0)
        this[Count - 1].GroupEnd = true;
    }

    /// <summary>
    /// Инициализация видимости всех команд данного уровная на основании видимости вложенных команд меню.
    /// Предполагается, что видимость всех "конечных" команд меню определена, а видимость подменю требуется определить
    /// </summary>
    public void InitMenuVisible()
    {
      for (int i = 0; i < Count; i++)
        this[i].InitMenuVisible();
    }

    #endregion

    #region IEnumerable<EFPCommandItem> Members

    /// <summary>
    /// Рекурсивный перечислитель для элементов меню
    /// </summary>
    private class RecurseItemEnumerator : GroupEnumerator<EFPCommandItem>
    {
      #region Конструктор

      public RecurseItemEnumerator(EFPOneLevelCommandItems items)
      {
        _Items = items;
      }

      #endregion

      #region Перебор элементов

      private EFPOneLevelCommandItems _Items;

      private static readonly DummyEnumerable<EFPCommandItem>.Enumerator _Dummy = new DummyEnumerable<EFPCommandItem>.Enumerator();

      /// <summary>
      /// На каждый элемент меню возвращаем два перечислителя.
      /// Первый (<paramref name="groupIndex"/>=0,2,4...) - однообъектный перечислитель,
      /// возвращающий сам элемент меню, второй (<paramref name="groupIndex"/>=1,3,5...) - 
      /// либо рекурсивный ItemEnumerator, либо DummyEnumerator
      /// </summary>
      /// <param name="groupIndex"></param>
      /// <returns></returns>
      protected override IEnumerator<EFPCommandItem> GetNextGroup(int groupIndex)
      {
        int itemIndex = groupIndex / 2;
        bool isSub = (groupIndex % 2) == 1;
        if (itemIndex >= _Items.Count)
          return null;
        if (isSub)
        {
          if (_Items[itemIndex].HasChildren)
            return new RecurseItemEnumerator(_Items[itemIndex].Children);
          else
            return _Dummy;
        }
        else
          return new SingleObjectEnumerable<EFPCommandItem>.Enumerator(_Items[itemIndex]);
      }

      #endregion
    }

    /// <summary>
    /// Возвращает перечислитель только по элементам этого уровня иерархии, или по всем уровням
    /// </summary>
    /// <returns>Перечислитель</returns>
    /// <param name="recurse">true - вернуть рекурсивный перечислитель.
    /// false - вернуть обычный перечислитель для одного уровня</param>
    public IEnumerator<EFPCommandItem> GetEnumerator(bool recurse)
    {
      if (recurse)
        return new RecurseItemEnumerator(this);
      else
        return _Items.GetEnumerator();
    }

    /// <summary>
    /// Возвращает перечислитель только по элементам этого уровня иерархии
    /// </summary>
    /// <returns>Перечислитель</returns>
    public IEnumerator<EFPCommandItem> GetEnumerator()
    {
      return _Items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return _Items.GetEnumerator();
    }

    #endregion

    #region Прочее

    /// <summary>
    /// Возвращает "Count=XXX" для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return "Count=" + Count.ToString();
    }

    #endregion
  }

  /// <summary>
  /// Список элементов для главного или локального меню в-целом.
  /// Реализуемый интерфейс IEnumerable позволяет перебирать все элементы в порядке их расположения в иерархии.
  /// Для доступа к к элементам верхнего меню используйте свойство TopLevelItems.
  /// </summary>
  public class EFPCommandItems : SimpleDisposableObject, IEnumerable<EFPCommandItem>, IReadOnlyObject
  {
    #region Конструктор и Dispose()

    /// <summary>
    /// Создает пустой список команд
    /// </summary>
    public EFPCommandItems()
    {
      _AllItems = new NamedList<EFPCommandItem>();
      _TopLevelItems = new EFPOneLevelCommandItems();
    }

    /// <summary>
    /// Очищает список команд при вызове Dispose()
    /// </summary>
    /// <param name="disposing">True, если был вызван метод Dispose(). False, если вызван деструктор</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        _AllItems.Clear();
        _TopLevelItems.Items.Clear();
      }
      base.Dispose(disposing);
    }

    #endregion

    #region Полный список команд

    /// <summary>
    /// Полный список команд.
    /// В этой коллекции элементы располагаются в порядке добавления элементов, без учета иерархии
    /// </summary>
    private NamedList<EFPCommandItem> _AllItems;

    /// <summary>
    /// Возвращает команду с заданной категорией и именем.
    /// Если такая команда отсутствует в списке, возвращает null.
    /// </summary>
    /// <param name="category">Категория</param>
    /// <param name="name">Имя</param>
    /// <returns>Команда или null</returns>
    public EFPCommandItem this[string category, string name]
    {
      get
      {
        string code = category + "|" + name;
        return _AllItems[code];
      }
    }

    /// <summary>
    /// Возвращает true, если команда есть в списке (независимо от уровня иерархии).
    /// Этот метод выполняется быстро.
    /// </summary>
    /// <param name="value">Искомая команда</param>
    /// <returns>Наличие команды</returns>
    public bool Contains(EFPCommandItem value)
    {
      if (value == null)
        return false;
      EFPCommandItem ci = this[value.Category, value.Name];
      return ci != null;
    }


    /// <summary>
    /// Возвращает true, если команда есть в списке (независимо от уровня иерархии).
    /// Этот метод выполняется быстро.
    /// </summary>
    /// <param name="category">Категория</param>
    /// <param name="name">Имя</param>
    /// <returns>Наличие команды</returns>
    public bool Contains(string category, string name)
    {
      return _AllItems.Contains(category + "|" + name);
    }

    /// <summary>
    /// Возвращает общее количество команд в списке, независимо от уровня иерархии
    /// </summary>
    public int Count { get { return _AllItems.Count; } }

    /// <summary>
    /// Возвращает элемент в заданной позиции.
    /// Имеются в виду все команды, а не только команды верхнего уроввня
    /// </summary>
    /// <param name="index">Индекс команды в общем списке</param>
    /// <returns></returns>
    public EFPCommandItem this[int index]
    {
      get
      {
        return _AllItems[index];
      }
    }

    /// <summary>
    /// Возвращает позицию команды в текущем наборе.
    /// </summary>
    /// <param name="value">Команда</param>
    /// <returns>Индекс</returns>
    public int IndexOf(EFPCommandItem value)
    {
      if (value == null)
        return -1;
      if (value.Owner != this)
        return -1;

      //return _AllItems.IndexOf(value);
      // Лучше искать по коду
      return _AllItems.IndexOf(((IObjectWithCode)value).Code);
    }

    /// <summary>
    /// Возвращает иедекс команды в текущем наборы
    /// </summary>
    /// <param name="Category">Категория</param>
    /// <param name="Name">Имя</param>
    /// <returns>Индекс команды</returns>
    public int IndexOf(string Category, string Name)
    {
      return _AllItems.IndexOf(Category + "|" + Name);
    }

    /// <summary>
    /// Добавление команды в список
    /// </summary>
    /// <param name="item"></param>
    public void Add(EFPCommandItem item)
    {
#if DEBUG
      CheckNotDisposed();
      CheckNotReadOnly();

      if (item == null)
        throw new ArgumentNullException("item");

      string code = ((IObjectWithCode)item).Code;
      if (_AllItems.Contains(code))
        throw new InvalidOperationException("Команда с кодом \"" + code + "\" уже есть");
#endif

      if (item.Owner != null)
        throw new InvalidOperationException("Повторное добавление команды \"" + item.DisplayName + "\" в список");

      if (item.Parent == null)
        _TopLevelItems.Items.Add(item);
      else if (item.Parent.Owner != this)
        throw new InvalidOperationException("Родительская команда " + item.Parent.ToString() + " не была присоединена к коллекции ранее");

      _AllItems.Add(item);

      item.Owner = this;

      if (item.HasIdle)
      {
        if (_ItemsWithIdle == null)
          _ItemsWithIdle = new List<EFPCommandItem>();
        _ItemsWithIdle.Add(item);
      }
    }

    /// <summary>
    /// Добавляет в текущий набор команд все команды из другого набора.
    /// Каждая команда клонируется.
    /// </summary>
    /// <param name="source">Исходный набор команд</param>
    public void Add(EFPCommandItems source)
    {
      Add(source, null);
    }

    /// <summary>
    /// Добавляет в текущий набор команд все команды из другого набора.
    /// Каждая команда клонируется.
    /// </summary>
    /// <param name="source">Исходный набор команд</param>
    /// <param name="parent">Команда, относящаяся к текущему набору, которая будет родительской</param>
    public void Add(EFPCommandItems source, EFPCommandItem parent)
    {
      if (source == null)
        throw new ArgumentNullException("source");

      if (parent != null)
      {
        if (parent.Owner != this)
          throw new ArgumentException("Родительская команда не относится к текущему набору", "parent");
      }

      foreach (EFPCommandItem ci in source)
      {
        EFPCommandItem ci2 = ci.Clone();
        if (ci.Parent != null)
          ci2.Parent = this[ci.Parent.Category, ci.Parent.Name];
        else
          ci2.Parent = parent;
        this.Add(ci2);
      }
    }

    #endregion

    #region Список команд верхнего уровня

    /// <summary>
    /// Содержит команды верхнего уровня, то есть имеющие свойство Parent=null
    /// </summary>
    public EFPOneLevelCommandItems TopLevelItems { get { return _TopLevelItems; } }
    private EFPOneLevelCommandItems _TopLevelItems;

    /// <summary>
    /// Добавление сепаратора.
    /// Если в списке уже есть команды, то для последней из них устанавливается
    /// свойство GroupEnd=true.
    /// Эквивалентно вызову TopLevelItems.AddSeparator()
    /// </summary>
    public void AddSeparator()
    {
      _TopLevelItems.AddSeparator();
    }

    #endregion

    #region Взаимное расположение команд

    private EFPOneLevelCommandItems GetOneLevelItems(EFPCommandItem ci)
    {
      if (ci.Parent == null)
        return TopLevelItems;
      else
        return ci.Parent.Children;
    }

    /// <summary>
    /// Поместить команду <paramref name="movedItem"/> перед командой <paramref name="baseItem"/>.
    /// Обе команды должны быть добавлены в текущий список команд.
    /// </summary>
    /// <param name="movedItem">Команда, которую требуется переместить</param>
    /// <param name="baseItem">Команда, перед которой требуется вставить команду <paramref name="movedItem"/>.
    /// Если null, то команда будет перемещена в начало списка</param>
    public void SetBefore(EFPCommandItem movedItem, EFPCommandItem baseItem)
    {
#if DEBUG
      if (movedItem == null)
        throw new ArgumentNullException("movedItem");
#endif

      if (baseItem != null)
        movedItem.Parent = baseItem.Parent; // Перенесли в нужный список

      EFPOneLevelCommandItems list = GetOneLevelItems(movedItem);
      list.Items.Remove(movedItem);
      if (baseItem == null)
        list.Items.Insert(0, movedItem);
      else
      {
        int p = list.Items.IndexOf(baseItem);
        list.Items.Insert(p, movedItem);
      }

      //if (Owner != null)
      //{
      //  Owner.AllItems.Remove(this);
      //  if (BaseItem == null)
      //    Owner.AllItems.Insert(0, this);
      //  else
      //  {
      //    int p = Owner.IndexOf(BaseItem);
      //    Owner.AllItems.Insert(p, this);
      //  }
      //}
    }


    /// <summary>
    /// Поместить команду <paramref name="movedItem"/> после команды <paramref name="baseItem"/>.
    /// Обе команды должны быть добавлены в текущий список команд.
    /// </summary>
    /// <param name="movedItem">Команда, которую требуется переместить</param>
    /// <param name="baseItem">Команда, после которой требуется вставить команду <paramref name="movedItem"/>.
    /// Если null, то команда будет перемещена в конец списка</param>
    public void SetAfter(EFPCommandItem movedItem, EFPCommandItem baseItem)
    {
#if DEBUG
      if (movedItem == null)
        throw new ArgumentNullException("movedItem");
#endif

      if (baseItem != null)
        movedItem.Parent = baseItem.Parent; // Перенесли в нужный список

      EFPOneLevelCommandItems list = GetOneLevelItems(movedItem);

      list.Items.Remove(movedItem);
      if (baseItem == null)
        list.Items.Add(movedItem);
      else
      {
        int p = list.Items.IndexOf(baseItem);
        list.Items.Insert(p + 1, movedItem);
      }

      //if (Owner != null)
      //{
      //  Owner.AllItems.Remove(this);
      //  if (BaseItem == null)
      //    Owner.AllItems.Add(this);
      //  else
      //  {
      //    int p = Owner.IndexOf(BaseItem);
      //    Owner.AllItems.Insert(p + 1, this);
      //  }
      //}

    }

    #endregion

    #region IEnumerable Members

    /// <summary>
    /// Возвращает перечислитель, который рекурсивно обходит все элементы, с учетом иерархии
    /// </summary>
    /// <returns></returns>
    public IEnumerator<EFPCommandItem> GetEnumerator()
    {
#if DEBUG
      CheckNotDisposed();
#endif
      return TopLevelItems.GetEnumerator(true);
    }


    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion

    #region Свойство IsReadOnly

    /// <summary>
    /// Если свойство возвращает true, то добавлять команды запрещено
    /// (свойства самих команд устанавливать можно)
    /// </summary>
    public bool IsReadOnly
    {
      get { return _IsReadOnly; }
      internal protected set
      {
        _IsReadOnly = value;
      }
    }
    private bool _IsReadOnly;

    /// <summary>
    /// Генерирует исключение, если IsReadOnly=true
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw new ObjectReadOnlyException("Нельзя изменять список команд, т.к. установлен признак \"Только для чтения\"");
    }

    #endregion

    #region Блокировка подменю

    /// <summary>
    /// Инициализация видимости подменю на основании видимости вложенных команд меню.
    /// Предполагается, что видимость всех "конечных" команд меню определена, а видимость подменю требуется определить
    /// </summary>
    public void InitMenuVisible()
    {
      TopLevelItems.InitMenuVisible();
    }

    #endregion

    #region События при выполнении команды

    /// <summary>
    /// Событие вызывается перед выполнением команды меню.
    /// Обработчик может прервать выполнение команды, установив свойство Cancel=true
    /// </summary>
    public event EFPCommandItemBeforeClickEventHandler BeforeClick;

    /// <summary>
    /// Вызывает событие BeforeClick
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected virtual void OnBeforeClick(EFPCommandItemBeforeClickEventArgs args)
    {
#if DEBUG
      if (args == null)
        throw new ArgumentNullException("args");
#endif

      if (BeforeClick != null)
        BeforeClick(this, args);
    }

    internal bool DoBeforeClick(EFPCommandItem item)
    {
      EFPCommandItemBeforeClickEventArgs args = new EFPCommandItemBeforeClickEventArgs(item);
      OnBeforeClick(args);
      return !args.Cancel;
    }

    /// <summary>
    /// Событие вызывается после выполнения команды меню.
    /// </summary>
    public event EFPCommandItemAfterClickEventHandler AfterClick;

    /// <summary>
    /// Вызывает событие AfterClick
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected virtual void OnAfterClick(EFPCommandItemAfterClickEventArgs args)
    {
#if DEBUG
      if (args == null)
        throw new ArgumentNullException("args");
#endif

      if (AfterClick != null)
        AfterClick(this, args);
    }

    internal void DoAfterClick(EFPCommandItem item)
    {
      EFPCommandItemAfterClickEventArgs args = new EFPCommandItemAfterClickEventArgs(item);
      OnAfterClick(args);
    }

    #endregion

    #region Событие Idle

    /// <summary>
    /// Событие вызывается, когда система свободна, с неопределенной периодичностью.
    /// См. описание события System.Windows.Forms.Application.Idle.
    /// Обработчик может инициализировать доступность некоторых команд меню
    /// </summary>
    public event EventHandler Idle;

    /// <summary>
    /// Список команд, у которых есть обработчик события Idle.
    /// Если ни одна команда не содержит обработчика, то равно null.
    /// </summary>
    private List<EFPCommandItem> _ItemsWithIdle;

    /// <summary>
    /// Вызывает событие Idle для EFPCommandItems и команд, в которых задан свой обработчик
    /// </summary>
    public void HandleIdle()
    {
      if (Idle != null)
        Idle(this, EventArgs.Empty);

      if (_ItemsWithIdle != null)
      {
        for (int i = 0; i < _ItemsWithIdle.Count; i++)
          _ItemsWithIdle[i].HandleIdle();
      }
    }

    /// <summary>
    /// Возвращает true, если есть обработчик события Idle у EFPCommandItems, или была добавлена хотя бы одна команда с обработчиком
    /// </summary>
    public bool HasIdle
    {
      get
      {
        return Idle != null || _ItemsWithIdle != null;
      }
    }

    #endregion

    #region Прочие методы

    /// <summary>
    /// Выполнить команду, связанную с комбинацией клавиш.
    /// Если команда найдена, но скрыта или заблокирована, выдается сообщение в статусной строке
    /// </summary>
    /// <param name="shortCut">Комбинация клавиш</param>
    /// <returns>true, если команда найдена</returns>
    public bool PerformShortCut(Keys shortCut)
    {
      EFPCommandItem ci = FindShortCut(shortCut);
      if (ci == null)
        return false;
      if (ci.InsideClick)
      {
        EFPApp.ShowTempMessage("Команда " + ci.DisplayName + " в настоящий момент выполняется");
        return true;
      }
      ci.PerformClick();
      return true;
    }

    /// <summary>
    /// Выполнить команду, связанную с комбинацией клавиш.
    /// Если команда не найдена, возвращается null
    /// </summary>
    /// <param name="shortCut">Комбинация клавиш</param>
    /// <returns>Найденная команда</returns>
    public EFPCommandItem FindShortCut(Keys shortCut)
    {
      foreach (EFPCommandItem item in this)
      {
        if (item.Visible && item.Enabled &&
          item.Servant == null) // 14.09.2015. Иначе Ctrl-V обнаруживается на уровне главного меню, даже если в локальном меню ShortCut убран
        {
          if (item.IsShortCut(shortCut))
            return item;
        }
      }

      return null;
    }

    /// <summary>
    /// Отменить заданный режим использования для всех команд в списке.
    /// Может использоваться, например, если в панели инструментов табличного просмотра должны быть
    /// только выбранные кнопки, а все остальные стандартные кнопки доступны только через меню.
    /// </summary>
    /// <param name="itemUsage">Флаги использования, которые должны быть сброшены</param>
    public void ClearUsage(EFPCommandItemUsage itemUsage)
    {
      for (int i = 0; i < _AllItems.Count; i++)
        _AllItems[i].Usage &= (~itemUsage);
    }

    /// <summary>
    /// Возвращает отладочную информацию
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      if (Count == 0)
        sb.Append("Empty");
      else
      {
        sb.Append("ItemCount=");
        sb.Append(Count);
      }
      if (IsReadOnly)
        sb.Append(" (ReadOnly)");

      return sb.ToString();
    }

    /// <summary>
    /// Добавляет одинаковый обработчик для всех команд меню
    /// </summary>
    /// <param name="handler"></param>
    internal void AddClickHandler(EventHandler handler)
    {
      for (int i = 0; i < _AllItems.Count; i++)
        _AllItems[i].Click += handler;
    }

    #endregion

    #region Статические методы для обработки ShortCut'ов

    /// <summary>
    /// Свойство не должно использоваться в пользовательском коде.
    /// Действует только в отладочном режиме.
    /// В режиме Release возвращает null.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static IList<EFPCommandItems> DebugFocusedObjects
    {
      get
      {
        // Используется в CS1
#if DEBUG
        return _FocusedObjects;
#else
        return null;
#endif
      }
    }

    private static List<EFPCommandItems> _FocusedObjects = new List<EFPCommandItems>();

    /// <summary>
    /// Используется свойством EFPControlCommandItems.Active.
    /// </summary>
    protected void AddFocus()
    {
      _FocusedObjects.Add(this);
    }

    /// <summary>
    /// Используется свойством EFPControlCommandItems.Active.
    /// </summary>
    protected void RemoveFocus()
    {
      _FocusedObjects.Remove(this);
    }

    /// <summary>
    /// Получить массив всех наборов команд, которые сейчас действуют (находятся в фокусе)
    /// </summary>
    /// <returns></returns>
    public static EFPCommandItems[] GetFocusedObjects()
    {
      return _FocusedObjects.ToArray();
    }

    /// <summary>
    /// Реализация обработчика нажатия клавиши, которая проверяет ShortCut'ы
    /// для всех наборов элементов, входящих в фокус.
    /// </summary>
    /// <param name="sender">Источник соыбтия. Игнорируется</param>
    /// <param name="args">Аргументы события. Если нажатие клавиши обработано, устанавливается свойство Handled</param>
    public static void PerformKeyDown(object sender, KeyEventArgs args)
    {
      // Этот метод не может возвращать значение

      if (args.Handled)
        return;
      if (args.KeyCode == Keys.ControlKey || args.KeyCode == Keys.ShiftKey || args.KeyCode == Keys.Menu)
        return;

      if (PerformKeyDown(args.KeyData))
        args.Handled = true;
    }

    /// <summary>
    /// Обработка события Control.KeyDown.
    /// Метод не должен использоваться в пользовательском коде
    /// </summary>
    /// <param name="keyData">Аргументы события</param>
    /// <returns>true, если нажатие клавищи обработано</returns>
    public static bool PerformKeyDown(Keys keyData)
    {
      if (!TestControlShortcut())
        return false;

      foreach (EFPCommandItems items in _FocusedObjects)
      {
        if (items == EFPApp.CommandItems)
        {
          // Темы главного меню не работают, если есть диалоговое окно
          if (EFPApp.ActiveDialog != null)
            continue;
        }
        if (items.PerformShortCut(keyData))
          return true;
      }

      // 29.08.2012
      // Глобальные сочетания клавиш
      for (int i = 0; i < EFPApp.CommandItems.GlobalShortCuts.Count; i++)
      {
        EFPCommandItem item = EFPApp.CommandItems.GlobalShortCuts[i];
        if (item.IsShortCut(keyData))
        {
          if (item.Visible && item.Enabled)
          {
            if (item.InsideClick)
              EFPApp.ShowTempMessage("Команда " + item.DisplayName + " в настоящий момент выполняется");
            else
              item.PerformClick();
          }
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Проверяем, можно ли обрабатывать ShortCut в текущем окне
    /// </summary>
    /// <returns></returns>
    private static bool TestControlShortcut()
    {
      Form frm = Form.ActiveForm;
      if (frm == null)
      {
        frm = EFPApp.MainWindow;
        if (frm == null)
          return true; // 30.10.2015
      }
      if (frm == EFPApp.MainWindow)
      {
        frm = EFPApp.MainWindow.ActiveMdiChild;
        if (frm == null)
          return true;
      }

      if (frm is EFPToolWindow)
        return false; // 08.09.2012


      Control ctrl = frm.ActiveControl;
      if (ctrl == null)
        return true;
      while (ctrl.Parent != null)
      {
        if (ctrl.Parent is DataGridView ||
          ctrl.Parent is TreeViewAdv) // 02.11.2015
          return false;
        ctrl = ctrl.Parent;
      }
      return true;
    }

    /// <summary>
    /// Альтернативный метод, который может вызваться, например из
    /// ProcessDialogKey()
    /// </summary>
    /// <param name="keyData">Код комбинации клавиш</param>
    /// <returns>true, если комбинация найдена</returns>
    public static bool PerformShortCutKey(Keys keyData)
    {
      foreach (EFPCommandItems items in _FocusedObjects)
      {
        if (items == EFPApp.CommandItems)
        {
          // Темы главного меню не работают, если есть диалоговое окно
          if (EFPApp.ActiveDialog != null)
            continue;
        }
        if (items.PerformShortCut(keyData))
        {
          return true;
        }
      }
      return false;
    }

    #endregion

    #region Отладочное окно

#if DEBUG

    /// <summary>
    /// Свойство не должно использоваться в пользовательском коде
    /// </summary>
    public EFPCommandItem[] DebugItems { get { return _AllItems.ToArray(); } }

#endif

    private class DebugForm : Form, IEFPAppTimeHandler
    {
      #region Конструктор

      public DebugForm()
      {
        base.Text = "Активные Command Items";
        base.FormBorderStyle = FormBorderStyle.SizableToolWindow;
        base.TopMost = true;
        base.FormClosed += new FormClosedEventHandler(DebugForm_FormClosed);

        _TheListBox = new ListBox();
        _TheListBox.Dock = DockStyle.Fill;
        _TheListBox.DoubleClick += new EventHandler(TheListBox_DoubleClick);
        Controls.Add(_TheListBox);

        EFPApp.Timers.Add(this);
      }

      void DebugForm_FormClosed(object sender, FormClosedEventArgs args)
      {
        EFPApp.Timers.Remove(this);
        EFPCommandItems._TheDebugWindow = null;
      }

      #endregion

      #region Поля

      private ListBox _TheListBox;

      #endregion

      #region Обновление списка

      public void TimerTick()
      {
        _TheListBox.BeginUpdate();
        try
        {
          _TheListBox.Items.Clear();
          _TheListBox.Items.AddRange(_FocusedObjects.ToArray());
        }
        finally
        {
          _TheListBox.EndUpdate();
        }
      }

      #endregion

      #region Просмотр одного набора

      private void TheListBox_DoubleClick(object sender, EventArgs args)
      {
        if (_TheListBox.SelectedItem == null)
          return;

        FreeLibSet.Forms.Diagnostics.DebugTools.DebugObject(_TheListBox.SelectedItem, "EFPCommandItem №" + (_TheListBox.SelectedIndex + 1).ToString());
      }

      #endregion
    }

    /// <summary>
    /// Управляет видимостью окна со списком действующих команд
    /// </summary>
    public static bool DebugWindowVisible
    {
      get { return _TheDebugWindow != null; }
      set
      {
        if (value == (_TheDebugWindow != null))
          return;
        if (value)
        {
          _TheDebugWindow = new DebugForm();
          _TheDebugWindow.Visible = true;
        }
        else
        {
          _TheDebugWindow.Dispose();
          _TheDebugWindow = null;
        }
      }
    }
    private static DebugForm _TheDebugWindow;

    #endregion

    #region Стандартные команды

    /// <summary>
    /// Стандартные команды, типа "Правка" - "Копировать"
    /// Если они заданы, то локальные меню управляющих элементов, например строки
    /// текста, смогут присоединяться к командам главного меню
    /// В главном меню программы может быть задана только часть команд
    /// </summary>
    /// <param name="stdItem">Идентификатор стандартной команды</param>
    /// <returns>Стандартная команда</returns>
    public EFPCommandItem this[EFPAppStdCommandItems stdItem]
    {
      get
      {
        string category, name;
        EFPAppCommandItems.GetStdCommandCategoryAndName(stdItem, out category, out name);
        return this[category, name];
      }
    }

    /// <summary>
    /// Создание и добавление стандартной команды.
    /// Обычно используется в главном меню
    /// </summary>
    /// <param name="stdItem">Идентификатор стандартной команды</param>
    /// <param name="parent">Родительская команда</param>
    public EFPCommandItem Add(EFPAppStdCommandItems stdItem, EFPCommandItem parent)
    {
#if DEBUG
      if (this[stdItem] != null)
        throw new BugException("Повторное создание стандартной команды " + stdItem.ToString());
#endif

      EFPCommandItem ci = EFPAppCommandItems.CreateStdCommand(stdItem);
      ci.Parent = parent;
      ci.Usage &= ~(EFPCommandItemUsage.StatusBar); // 21.08.2018
      Add(ci);
      return ci;
    }

    /// <summary>
    /// Создание и добавление стандартной команды.
    /// Обычно используется в главном меню
    /// </summary>
    /// <param name="stdItem">Идентификатор стандартной команды</param>
    public EFPCommandItem Add(EFPAppStdCommandItems stdItem)
    {
      return Add(stdItem, null);
    }

    #endregion
  }

  /// <summary>
  /// Базовый класс для построения элементов. 
  /// </summary>
  public abstract class EFPUIObjs : SimpleDisposableObject, IEnumerable<EFPUIObjBase>
  {
    // 03.01.2020
    // Несмотря на то, что классы-наследники содержат ссылки на управляющие элементы,
    // нет нужды использовать базовый класс с деструктором.
    // У нас нет ссылок на неуправляемые ресурсы

    #region Конструктор и Dispose()

    /// <summary>
    /// Защищенный конструктор
    /// </summary>
    protected EFPUIObjs()
    {
      _UIObjs = new NamedList<EFPUIObjBase>();
    }

    /// <summary>
    /// Вызывает Dispose() для всех элементов в списке
    /// </summary>
    /// <param name="disposing">true, если вызван метод Dispose(), а не деструктор</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        foreach (EFPUIObjBase uiObj in _UIObjs)
        {
          uiObj.Dispose();
        }
        _UIObjs = null;
      }

      base.Dispose(disposing);
    }

    #endregion

    #region Внутренняя реализация

    internal NamedList<EFPUIObjBase> _UIObjs;

    /// <summary>
    /// Возвращает отладочную информацию
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      string s = GetType().Name + " (Item Count=" + _UIObjs.Count.ToString() + ")";
      return s;
    }

    /// <summary>
    /// Возвращает количество добавленных объектов
    /// </summary>
    public int Count
    {
      get
      {
        if (_UIObjs == null)
          return 0;
        else
          return _UIObjs.Count;
      }
    }

    /// <summary>
    /// Возвращает объект интерфейса по индексу
    /// </summary>
    /// <param name="index">Индекс в диапазоне от 0 до Count-1</param>
    /// <returns>Объект интерефйса</returns>
    public EFPUIObjBase this[int index]
    {
      get { return _UIObjs[index]; }
    }

    /// <summary>
    /// Возвращает элемент для заданной команды.
    /// Если нет такой команды, возвращает null
    /// </summary>
    /// <param name="category">Категория</param>
    /// <param name="name">Имя</param>
    /// <returns>Элемент интерфейса или null</returns>
    public EFPUIObjBase this[string category, string name]
    {
      get { return _UIObjs[category + "|" + name]; }
    }

    /// <summary>
    /// Возвращает элемент для заданной команды.
    /// Если нет такой команды, возвращает null
    /// </summary>
    /// <param name="commandItem">Команда</param>
    /// <returns>Элемент интерфейса или null</returns>
    public EFPUIObjBase this[EFPCommandItem commandItem]
    {
      get
      {
        if (commandItem == null)
          return null;
        EFPUIObjBase res = this[commandItem.Category, commandItem.Name];
        if (res == null)
          return null;
        if (Object.ReferenceEquals(commandItem, res.Item))
          return res;
        else
          return null;
      }
    }

#if DEBUG

    /// <summary>
    /// Отладочное свойство.
    /// Не использовать в прикладном коде.
    /// </summary>
    public EFPUIObjBase[] DebugItems
    {
      get { return _UIObjs.ToArray(); }
      set { /* заглушка для отображения в окне свойств объекта */}
    }

#endif

    #endregion

    #region IEnumerable<EFPUIObjBase> Members

    /// <summary>
    /// Возвращает перечислитель по объектам EFPUIObjBase
    /// </summary>
    /// <returns>Перечислитель</returns>
    public IEnumerator<EFPUIObjBase> GetEnumerator()
    {
      return _UIObjs.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return _UIObjs.GetEnumerator();
    }

    #endregion
  }
}
