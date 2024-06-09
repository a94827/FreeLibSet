using System;
using System.Windows.Forms;
using System.Collections;
using System.Drawing;
using System.Windows.Forms.VisualStyles;
using System.ComponentModel;
using System.Collections.Generic;
using FreeLibSet.Core;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Базовый класс для главного меню программы и контекстных меню
  /// </summary>
  public abstract class EFPMenuBase : EFPUIObjs
  {
    #region Конструктор и Dispose

    /// <summary>
    /// Создает меню
    /// </summary>
    /// <param name="menu">Меню Windows Forms</param>
    /// <param name="delayedAdd">Если true, то список команд будет инициализирован только при открытии меню</param>
    protected EFPMenuBase(ToolStrip menu, bool delayedAdd)
    {
      _Menu = menu;
      // почему-то не работает
      //FMenu.ImageList = ClientImages.Main.ImageList; 

      _ItemDict = new Dictionary<string, ToolStripMenuItem>();

      this._DelayedAdd = delayedAdd;
      if (menu is ToolStripDropDown)
        ((ToolStripDropDown)menu).Opening += new System.ComponentModel.CancelEventHandler(Menu_Opening1);
      else if (menu is MenuStrip)
        ((MenuStrip)menu).MenuActivate += new EventHandler(Menu_MenuActivate2);
      else
        throw new ArgumentException("Неизвестный тип меню", "menu");

#if DEBUG
      DisposableObject.RegisterDisposableObject(_Menu);
      _Menu.Disposed += new EventHandler(Menu_Disposed);
#endif
    }

    /// <summary>
    /// Вызывает ToolStrip.Dispose()
    /// </summary>
    /// <param name="disposing">true, если был вызван метод Dispose(), а е не деструктор</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        _Menu.Dispose();
        _Menu = null;
      }
      base.Dispose(disposing);
    }

#if DEBUG
    void Menu_Disposed(object sender, EventArgs args)
    {
      DisposableObject.UnregisterDisposableObject(sender);
    }
#endif

    #endregion

    #region Свойства

    /// <summary>
    /// Имя для меню. Дублирует свойство ToolsTrip.Name
    /// </summary>
    public string Name
    {
      get
      {
        if (_Menu == null)
          return "Disposed";
        else
          return _Menu.Name;
      }
      set { _Menu.Name = value; }
    }

    /// <summary>
    /// Если true, то реально команды в объект меню будут добавлены при выборе
    /// меню, а не при вызове команды Add
    /// </summary>
    private bool _DelayedAdd;

    private List<EFPCommandItems> _DelayedItems;

    #endregion

    #region Добавление элементов

    /// <summary>
    /// Добавляет команду в меню.
    /// Создает ToolStripMenuItem.
    /// Повторный вызов метода для той же самой команды игнорируется.
    /// Эта версия используется при создании выпадающего меню для кнопки на панели инструментов.
    /// </summary>
    /// <param name="item">Добавляемая команда меню</param>
    /// <param name="parent">Родительская команда меню</param>
    public void Add(EFPCommandItem item, EFPCommandItem parent)
    {
      if (item == null)
        return;

      if (ItemDict.ContainsKey(item.CategoryAndName))
        return; // повторное добавление темы

      ToolStripItemCollection parentItems; // куда будем добавлять
      if (parent == null)
        parentItems = _Menu.Items;
      else
      {
        if (!ItemDict.ContainsKey(parent.CategoryAndName))
          throw new InvalidOperationException("Родительская команда " + parent.ToString() + " не была добавлен в меню");
        parentItems = ItemDict[parent.CategoryAndName].DropDownItems;
      }

      if (item.GroupBegin)
        AddSeparator(parentItems);
      ToolStripMenuItem thisMenuItem = new ToolStripMenuItem();
      ItemDict.Add(item.CategoryAndName, thisMenuItem);
      EFPUIMenuItemObj uiObj = new EFPUIMenuItemObj(this, item, thisMenuItem);
      uiObj.SetAll();
      parentItems.Add(thisMenuItem);
      if (item.GroupEnd)
        AddSeparator(parentItems);
    }

    /// <summary>
    /// Добавляет команду в меню.
    /// Создает ToolStripMenuItem.
    /// Если родительская команда <paramref name="item"/>.Parent еще не была добавлена,
    /// то она добавляется в первую очередь.
    /// Повторный вызов метода для той же самой команды игнорируется.
    /// </summary>
    /// <param name="item">Добавляемая команда меню</param>
    public void Add(EFPCommandItem item)
    {
      EFPCommandItem parent = item.Parent;
      if (parent != null)
      {
        if (!ItemDict.ContainsKey(parent.CategoryAndName))
          Add(parent);
      }
      Add(item, parent);
    }

    /// <summary>
    /// Добавляет из списка все команды, которые имеют установленное свойство EFPCommandItem.MenuUsage=true.
    /// Если в конструкторе был задан параметр DelayedAdd=true, то команды добавляются во внутренний список,
    /// а создание объектов ToolStripMenuItem откладывается до реального открытия меню пользователем.
    /// </summary>
    /// <param name="items">Заполненный список команд</param>
    public void Add(EFPCommandItems items)
    {
      if (_DelayedAdd)
      {
        if (_DelayedItems == null)
          _DelayedItems = new List<EFPCommandItems>();
        _DelayedItems.Add(items);
      }
      else
        DoAdd(items);
    }

    private void DoAdd(EFPCommandItems items)
    {
      // Команды меню могут входить в массив Items в обратном порядке, то есть:
      // 1. Создается подменю VisinleClientItem, но в массив Items не добавляется.
      // 2. Создается команда, ее свойство Parent устанавливается, команда добавляется
      // 3. Команда подменю добавляется в Items.
      //
      // Если использовать один проход, то команда меню вставит досрочно команду
      // подменю, что может нарушить логический порядок следования команд.
      //
      // Будем добавлять команды за несколько проходов, пока они все не добавятся

      AddSeparator(_Menu.Items); // 05.06.2017

      while (true)
      {
        bool hasDelayed = false;
        foreach (EFPCommandItem item in items)
        {
          if (item.MenuUsage)
          {
            if (ItemDict.ContainsKey(item.CategoryAndName))
              continue; // уже добавили на предыдущем проходе

            if (item.Parent != null)
            {
              if ((!item.Parent.MenuUsage))
              {
                if (item.IsInToolBarDropDown)
                  continue;
                throw new InvalidOperationException("Нельзя присоединить команду \"" + item.ToString() +
                  "\" к меню, потому что ее родительская команда \"" + item.Parent.ToString() +
                  "\" не относится к меню");
              }
              if (!items.Contains(item.Parent))
                throw new InvalidOperationException("Нельзя присоединить команду \"" + item.ToString() +
                  "\" к меню, потому что ее родительская команда \"" + item.Parent.ToString() +
                  "\" не входит в список команд");
              if (!ItemDict.ContainsKey(item.Parent.CategoryAndName))
              {
                hasDelayed = true;
                continue;
              }
            }
            Add(item);
          }
        }
        if (!hasDelayed)
          break;
      }
    }

    /// <summary>
    /// Вставляет команду меню перед указанной.
    /// </summary>
    /// <param name="item">Добавляемый элемент</param>
    /// <param name="before">Элемент, перед котормы выполняется добавление.
    /// Если null, то выполняется добавление в конец меню</param>
    public void Insert(EFPCommandItem item, EFPCommandItem before)
    {
      if (item == null)
        return;

      if (before == null)
      {
        Add(item);
        return;
      }

      EFPCommandItem parent = item.Parent;
      if (parent != null)
      {
        if (!ItemDict.ContainsKey(parent.CategoryAndName))
          Add(parent);
      }

      if (ItemDict.ContainsKey(item.CategoryAndName))
        return; // повторное добавление темы

      ToolStripItemCollection parentItems; // куда будем добавлять
      if (parent == null)
        parentItems = _Menu.Items;
      else
      {
        //if (ht[Parent] == null)
        //  throw new InvalidOperationException("Родительская команда " + Parent.ToString() + " не была добавлен в меню");
        parentItems = ItemDict[parent.CategoryAndName].DropDownItems;
      }

      ToolStripMenuItem beforeMenuItem;
      if (!ItemDict.TryGetValue(before.CategoryAndName, out beforeMenuItem))
        throw new ArgumentException("Команда " + before.ToString() + " не была добавлена в меню", "before");
      int index = parentItems.IndexOf(beforeMenuItem);
      if (index < 0)
        throw new ArgumentException("Команда " + before.ToString() + " располагается в другом меню", "before");

      if (item.GroupBegin)
        InsertSeparator(parentItems, ref index);
      ToolStripMenuItem thisMenuItem = new ToolStripMenuItem();
      ItemDict.Add(item.CategoryAndName, thisMenuItem);
      EFPUIMenuItemObj uiObj = new EFPUIMenuItemObj(this, item, thisMenuItem);
      uiObj.SetAll();
      parentItems.Insert(index, thisMenuItem);
      index++;
      if (item.GroupEnd /*|| Before.GroupEnd*/)
        InsertSeparator(parentItems, ref index);
    }

    #endregion

    #region Внутренняя реализация

    /// <summary>
    /// Объект ToolStrip.
    /// Задается в конструкторе
    /// </summary>
    protected ToolStrip Menu { get { return _Menu; } }
    private ToolStrip _Menu;

    /// <summary>
    /// Словарь команд меню:
    /// Ключ - объект EFPCommandItem.CaregoryAndName, значение - управляющий элемент команды меню
    /// </summary>
    protected IDictionary<string, ToolStripMenuItem> ItemDict { get { return _ItemDict; } }
    private Dictionary<string, ToolStripMenuItem> _ItemDict;


    /// <summary>
    /// Этот сепаратор всегда должен быть видимым.
    /// Он отделяет список окон MDI от остальных команд меню "Окно"
    /// </summary>
    protected ToolStripSeparator WindowMenuSeparator;

    private void AddSeparator(ToolStripItemCollection parentItems)
    {
      if (parentItems.Count < 1)
        return;
      if (parentItems[parentItems.Count - 1] is ToolStripSeparator)
        return;
      ToolStripSeparator mi = new ToolStripSeparator();
      parentItems.Add(mi);
    }

    private void InsertSeparator(ToolStripItemCollection parentItems, ref int index)
    {
      if (parentItems.Count < 1)
        return;
      if (index > 0)
      {
        if (parentItems[index - 1] is ToolStripSeparator)
          return;
      }
      if (parentItems[index] is ToolStripSeparator)
        return;
      ToolStripSeparator mi = new ToolStripSeparator();
      parentItems.Insert(index, mi);
      index++;
    }

    /// <summary>
    /// Реализация отложенного заполнения команд меню
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void Menu_Opening1(object sender, CancelEventArgs args)
    {
      args.Cancel = false; // по умолчанию флаг установлен, неизвестно почему
      DoMenuOpening();
    }

    void Menu_MenuActivate2(object sender, EventArgs args)
    {
      DoMenuOpening();
    }

    private void DoMenuOpening()
    {
      #region Перенос DelayedItems

      if (_DelayedItems != null)
      {
        // Не повторный вызов
        try
        {
          for (int i = 0; i < _DelayedItems.Count; i++)
            DoAdd(_DelayedItems[i]);
          InitSeparatorVisiblity();
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e, "Ошибка отложенной инициализации меню");
        }
        _DelayedItems = null;

        if (DefaultCommandItem != null)
        {
          ToolStripMenuItem item = ToolStripMenuItems[DefaultCommandItem];
          if (item != null)
            item.Font = new Font(item.Font, FontStyle.Bold);
        }
      }

      #endregion

      #region Событие в EFPCommandItem

      foreach (EFPUIObjBase obj in this)
        obj.Item.HandleMenuOpening();

      #endregion
    }


    //public override string ToString()
    //{
    //  return "ItemCount=" + ht.Count.ToString();
    //}

    /// <summary>
    /// Установка видимости разделителей в меню и всех подменю
    /// Этот метод должен быть вызван после обновления команд меню, если оно выполняется нестандартым
    /// способом, а прямым вызовом методов Add() или Insert()
    /// </summary>
    public void InitSeparatorVisiblity()
    {
      InitSeparatorVisiblityRecurse(_Menu.Items, WindowMenuSeparator);
    }

    internal static void InitSeparatorVisiblityRecurse(ToolStripItemCollection parentItems, ToolStripSeparator windowMenuSeparator)
    {
      InitSeparatorVisiblity(parentItems, windowMenuSeparator);

      foreach (ToolStripItem mi in parentItems)
      {
        if (mi is ToolStripMenuItem)
          InitSeparatorVisiblityRecurse(((ToolStripMenuItem)mi).DropDownItems, windowMenuSeparator);
      }
    }

    /// <summary>
    /// Установки видимости сепараторов в пределах меню (без подменю)
    /// </summary>
    internal static void InitSeparatorVisiblity(ToolStripItemCollection parentItems, ToolStripSeparator windowMenuSeparator)
    {
      //if (ParentItems.Count == 0)
      //  return;

      //bool HasSep = false;
      //bool HasVisible = false;
      //foreach (ToolStripItem mi in ParentItems)
      //{
      //  if (mi is ToolStripSeparator)
      //    HasSep = true;
      //  else
      //  {
      //    ClientItem Item = (ClientItem)(mi.Tag);
      //    if (Item.Visible)
      //      HasVisible = true;
      //  }
      //}
      //if (!(HasVisible && HasSep))
      //  return;


      int lastIndex = -1;
      for (int i = parentItems.Count - 1; i >= 0; i--)
      {
        ToolStripItem mi = parentItems[i];
        if (mi is ToolStripSeparator)
        {
          if (mi == windowMenuSeparator)
            mi.Visible = true; //EFPApp.Forms.Length > 0;
          else
            mi.Visible = false;
        }
        else
        {
          EFPCommandItem item = mi.Tag as EFPCommandItem;
          if (item != null) // 09.06.2015
          {
            if (item.Visible)
            {
              lastIndex = i - 1;
              break;
            }
          }
        }
      }

      bool wantsSep = false;
      for (int i = 0; i <= lastIndex; i++)
      {
        ToolStripItem mi = parentItems[i];
        if (mi is ToolStripSeparator)
        {
          if (mi == windowMenuSeparator)
            mi.Visible = true;//EFPApp.Forms.Length > 0;
          else
            mi.Visible = wantsSep;
          wantsSep = false;
        }
        else
        {
          EFPCommandItem item = mi.Tag as EFPCommandItem;
          if (item != null) // 09.06.2015
          {
            if (item.Visible)
              wantsSep = true;
          }
        }
      }
    }

    #endregion

    /// <summary>
    /// Переходник между объектом меню в программе и позицией меню Windows Forms
    /// </summary>
    private class EFPUIMenuItemObj : EFPUIObjBase
    {
      #region Конструктор

      public EFPUIMenuItemObj(EFPMenuBase owner, EFPCommandItem item, ToolStripMenuItem menuItem)
        : base(owner, item)
      {
        _MenuItem = menuItem;
        _MenuItem.Tag = item;
        if (!item.HasChildren)
          _MenuItem.Click += new EventHandler(ClickEvent);
      }

      #endregion

      #region Свойства

      public new EFPMenuBase Owner
      {
        get { return (EFPMenuBase)(base.Owner); }
      }

      /// <summary>
      /// Возвращает true для темы в строке главного меню
      /// </summary>
      private bool IsMainMenuBarItem
      {
        get
        {
          return Item.Parent == null && Owner is EFPMainMenu;
        }
      }

      public ToolStripMenuItem MenuItem { get { return _MenuItem; } }
      private ToolStripMenuItem _MenuItem;

      #endregion

      #region Переопределенные методы

      /*
      public override void SetMenuText   () 
      {
        string VersionStr=Item.MenuText;
        if (Item.MenuRightText != null)
          VersionStr+='\t'+Item.MenuRightText;
        else
          if (Item.ShortCut != Keys.None)
            VersionStr+='\t'+Item.ShortCutText;
        FMenuItem.Text=VersionStr;
      }
      
       */

      /// <summary>
      /// Символ "галочка"
      /// </summary>
      const char MarkerChar = '\u221a';

      public override void SetMenuText()
      {
        if (ToolStripManager.RenderMode == ToolStripManagerRenderMode.System && (!IsMainMenuBarItem))
        {
          if (Item.Checked)
            _MenuItem.Text = MarkerChar + " " + Item.MenuText;
          else
            _MenuItem.Text = new string(DataTools.NonBreakSpaceChar, 3) + Item.MenuText;
        }
        else
          _MenuItem.Text = Item.MenuText;
        if (Item.MenuRightText != null)
          _MenuItem.ShortcutKeyDisplayString = Item.MenuRightText;
        else
          if (Item.ShortCut != Keys.None)
          _MenuItem.ShortcutKeyDisplayString = Item.ShortCutText;
        //FMenuItem.ShortcutKeys = Item.ShortCut;
      }


      public override void SetVisible()
      {
        _MenuItem.Visible = Item.Visible;
        if (_MenuItem.Owner == null)
          EFPMenuBase.InitSeparatorVisiblity(Owner._Menu.Items, Owner.WindowMenuSeparator);
        else
          EFPMenuBase.InitSeparatorVisiblity(_MenuItem.Owner.Items, Owner.WindowMenuSeparator);
      }

      public override void SetEnabled()
      {
        _MenuItem.Enabled = Item.Enabled && (!Item.InsideClick);
      }

      public override void SetChecked()
      {
        _MenuItem.Checked = Item.Checked;
        if (ToolStripManager.RenderMode == ToolStripManagerRenderMode.System)
        {
          SetMenuText();
          _MenuItem.Checked = false; // Не отображаем стандартную галочку. Она некрасивозалезет сверху на значок.
        }
        else
          _MenuItem.Checked = Item.Checked;
      }

      public override void SetImage()
      {
        //  // почему-то не работает
        //  // FMenuItem.ImageIndex = ClientImages.Main[Item.ImageName];

        if (Item.Image == null)
        {
          if (String.IsNullOrEmpty(Item.ImageKey))
            _MenuItem.Image = null;
          else
            _MenuItem.Image = EFPApp.MainImages.Images[Item.ImageKey];
        }
        else
          _MenuItem.Image = Item.Image;
      }

      #endregion
    }

    #region Доступ к командам меню

    /// <summary>
    /// Структура для реализации свойства EFPMenuBase.ToolStripMenuItems 
    /// </summary>
    public struct ToolStripMenuItemList
    {
      #region Конструктор

      internal ToolStripMenuItemList(EFPMenuBase owner)
      {
        _Owner = owner;
      }

      #endregion

      #region Свойства

      private EFPMenuBase _Owner;

      /// <summary>
      /// Возвращает ToolStripMenuItem для заданной команды.
      /// Если для такой команды нет позиции меню, возвращает null
      /// </summary>
      /// <param name="commandItem">Команда</param>
      /// <returns>Позиция меню или null</returns>
      public ToolStripMenuItem this[EFPCommandItem commandItem]
      {
        get
        {
          EFPUIObjBase uiObj = _Owner[commandItem];
          if (uiObj == null)
            return null;
          else
            return ((EFPUIMenuItemObj)uiObj).MenuItem;
        }
      }

      /// <summary>
      /// Возвращает ToolStripMenuItem для заданной команды.
      /// Если для такой команды нет позиции меню, возвращает null
      /// </summary>
      /// <param name="category">Категория</param>
      /// <param name="name">Код команды</param>
      /// <returns>Позиция меню или null</returns>
      public ToolStripMenuItem this[string category, string name]
      {
        get
        {
          EFPUIObjBase uiObj = _Owner[category, name];
          if (uiObj == null)
            return null;
          else
            return ((EFPUIMenuItemObj)uiObj).MenuItem;
        }
      }

      #endregion
    }

    /// <summary>
    /// Доступ к командам меню
    /// </summary>
    public ToolStripMenuItemList ToolStripMenuItems { get { return new ToolStripMenuItemList(this); } }

    #endregion

    #region Расширенные действия с командами

    /// <summary>
    /// Команда выделенная жирным шрифтом
    /// </summary>
    public EFPCommandItem DefaultCommandItem
    {
      get { return _DefaultCommandItem; }
      set
      {
        if (Object.ReferenceEquals(value, _DefaultCommandItem))
          return;
        if (_DefaultCommandItem != null)
        {
          ToolStripMenuItem item = ToolStripMenuItems[_DefaultCommandItem];
          if (item != null)
            item.ResetFont();
        }
        _DefaultCommandItem = value;
        if (_DefaultCommandItem != null)
        {
          ToolStripMenuItem item = ToolStripMenuItems[_DefaultCommandItem];
          if (item != null)
            item.Font = new Font(item.Font, FontStyle.Bold);
        }
      }
    }
    private EFPCommandItem _DefaultCommandItem;

    #endregion
  }

  /// <summary>
  /// Главное меню программы
  /// </summary>
  public sealed class EFPMainMenu : EFPMenuBase
  {
    #region Конструктор

    /// <summary>
    /// Создает меню с объектом MenuStrip.
    /// </summary>
    public EFPMainMenu()
      : base(new MenuStrip(), false)
    {
      base.Menu.AllowMerge = true;
      base.Menu.RendererChanged += new EventHandler(Menu_RendererChanged);
      //FMenu_RendererChanged(null, null);
      base.Menu.CanOverflow = true; // 10.06.2021
    }

    #endregion

    #region Методы

    /// <summary>
    /// Присоединение команд обработки списка окон. Должна вызываться
    /// после того, как добавлены все команды меню с помощью Add(), но
    /// перед вызовом Attach()
    /// </summary>
    /// <param name="windowCommandItem">Команда "Окно"</param>
    public void InitWindowMenu(EFPCommandItem windowCommandItem)
    {
      if (windowCommandItem == null)
        throw new ArgumentNullException("windowCommandItem");

      ToolStripMenuItem windowMenuItem;
      if (!ItemDict.TryGetValue(windowCommandItem.CategoryAndName, out windowMenuItem))
        throw new ArgumentException("Команда " + windowCommandItem.ToString() + " не была добавлена в меню", "windowCommandItem");

      if (windowMenuItem.DropDownItems.Count > 0)
      {
        // Надо добавить сепаратор
        WindowMenuSeparator = new ToolStripSeparator();
        windowMenuItem.DropDownItems.Add(WindowMenuSeparator);
      }

      ((MenuStrip)Menu).MdiWindowListItem = (ToolStripMenuItem)windowMenuItem;
    }

    /// <summary>
    /// Этот метод является устаревшим.
    /// В новых программах следует использовать свойство EFPApp.Interface, 
    /// где меню, панели инструментов и статусная строка присоединяется автоматически
    /// </summary>
    public void Attach()
    {
      Attach(EFPApp.MainWindow);
    }

    internal void Attach(Form mainWindow)
    {
      if (mainWindow == null)
        throw new ArgumentNullException("mainWindow");

      if (mainWindow.MainMenuStrip != null)
        throw new InvalidOperationException("Повторное присоединение главного меню к окну " + mainWindow.ToString());

      // 10.06.2021
      foreach (ToolStripItem topItem in base.Menu.Items)
        topItem.Overflow = ToolStripItemOverflow.AsNeeded;

      mainWindow.Controls.Add(Menu);
      mainWindow.MainMenuStrip = (MenuStrip)Menu;
      InitSeparatorVisiblity();

      _Info = new FormToolStripInfo(mainWindow);
      Menu_RendererChanged(null, null);
    }

    private FormToolStripInfo _Info;

    #endregion

    #region Внутренняя реализация

    private void Menu_RendererChanged(object sender, EventArgs args)
    {
      // Исправляем дефект в MenuStrip.
      // Под Windows-98/Me/2000 не определен цвет SystemColors.MenuBar (он черный),
      // поэтому полоса меню рисуется черным цветом, что в сочетании с черными
      // символами делает его неработающим.
      //
      // Для исправления дефекта надо вручную устанавливать MenuStrip.BackColor.
      // Однако, когда используется рендерер Professional, фиксированная установка
      // цвета фона продолжает действовать, и меню не будет выглядеть рельефным.
      // Поэтому, при обратном переключении надо отключать цвет фона.
      //
      // Второй дефект: Меню имеет свойство AutoSize=true, но высота всегда равна
      // 24 (что нормально для меню в стиле Professional), а не заданной в настройках
      // экрана. Как его исправить пока не знаю. Если установить высоту меню, то
      // названия команд окажутся прижатыми к нижней части меню, а не центрированы.

      if (_Info == null)
        return; // не было вызова Attach()

      if (ToolStripManager.RenderMode == ToolStripManagerRenderMode.System)
      {
        if (VisualStyleInformation.IsSupportedByOS) // Windows XP или больше
          base.Menu.BackColor = SystemColors.MenuBar;
        else
          base.Menu.BackColor = SystemColors.Menu; // Windows-98/Me/2000

        if (_Info.StatusBar != null) // 27.12.2020
          _Info.StatusBar.RenderMode = ToolStripRenderMode.System;

        //base.FMenu.AutoSize = false;
        //base.FMenu.Height = SystemInformation.MenuHeight;
      }
      else
      {
        base.Menu.BackColor = Color.Empty;
        if (_Info.StatusBar != null)
        {
          //MainForm.TheStatusBar.RenderMode = ToolStripRenderMode.Professional;
          _Info.StatusBar.RenderMode = ToolStripRenderMode.ManagerRenderMode; // 22.08.2017
        }
      }
      //MessageBox.Show(base.FMenu.Renderer.ToString(), "RendererChanged");

      // 10.06.2021
      // Многострочное меню не используется, поэтому автоподбор высоты реально не нужен.
      // Когда меню не входит по ширине, справа появляется кнопка с уголочком. При этом высота меню увеличивается
      // на пару пикселей. Это раздражает, особенно для интерфейса MDI, когда дочерние окна перестают помещаться и появляется полоса прокрутки
      base.Menu.AutoSize = false;
    }

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>текстовое представление</returns>
    public override string ToString()
    {
      return "Главное меню. " + base.ToString();
    }

    #endregion
  }

  /// <summary>
  /// Контекстное меню, привязанное к управляющему элементу
  /// </summary>
  public sealed class EFPContextMenu : EFPMenuBase
  {
    #region Конструктор

    /// <summary>
    /// Создает меню, используя элемент ContextMenuStrip.
    /// </summary>
    public EFPContextMenu()
      : base(new ContextMenuStrip(), true)
    {
    }

    #endregion

    #region Методы

    /// <summary>
    /// Присоединяет меню к управляющему элементу
    /// </summary>
    /// <param name="control">Управляющий элемент</param>
    public void Attach(Control control)
    {
#if DEBUG
      if (control == null)
        throw new ArgumentNullException("control");
      if (_Control != null)
        throw new InvalidOperationException("Повторное присоединение контекстного меню к управляющему элементу");
      _Control = control;
#endif

      InitSeparatorVisiblity();

      control.Disposed += new EventHandler(ControlDisposed); // обработчик Dispose только для верхнего элемента
      DoAttach(control);
    }

    /// <summary>
    /// Рекурсивное присоединение меню
    /// </summary>
    /// <param name="currControl"></param>
    private void DoAttach(Control currControl)
    {
#if !NET
      currControl.ContextMenu = null; // 11.07.2022. В Mono для TextBox назначается свое меню, которое "перевешивает" наше меню в ContextMenuStrip
#endif
      currControl.ContextMenuStrip = Menu;

      if (currControl is UserControl)
      {
        foreach (Control childControl in currControl.Controls)
          DoAttach(childControl); // рекурсия
      }
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Объект меню Windows Forms
    /// </summary>
    public new ContextMenuStrip Menu
    {
      get
      {
        return (ContextMenuStrip)(base.Menu);
      }
    }

#if DEBUG

    /// <summary>
    /// Отладочное свойство.
    /// Не использовать
    /// </summary>
    public Control Control { get { return _Control; } }
    private Control _Control;

#endif

    #endregion

    #region Внутренняя реализация

    private void ControlDisposed(object sender, EventArgs args)
    {
      Dispose();
    }

#if DEBUG

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return "ItemCount=" + Count.ToString() + ". Control=" + (Control == null ? "null" : Control.ToString());
    }

#endif

    #endregion
  }

  /// <summary>
  /// Выпадающее меню для кнопки панели инструментов
  /// </summary>
  public sealed class EFPDropDownMenu : EFPMenuBase
  {
    #region Конструктор

    /// <summary>
    /// Создает объект и ToolStripDropDownMenu
    /// </summary>
    public EFPDropDownMenu()
      : base(new ToolStripDropDownMenu(), true)
    {
    }

    #endregion

    #region Методы

    /// <summary>
    /// Присоединяет меню к кнопке
    /// </summary>
    /// <param name="owner">Кнопка-владелец меню</param>
    public void Attach(ToolStripDropDownItem owner)
    {
      owner.DropDown = (ToolStripDropDownMenu)Menu;
      InitSeparatorVisiblity();
    }

    #endregion
  }

}
