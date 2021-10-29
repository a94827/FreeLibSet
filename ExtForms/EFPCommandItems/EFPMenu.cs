using System;
using System.Windows.Forms;
using System.Collections;
using System.Drawing;
using System.Windows.Forms.VisualStyles;
using System.ComponentModel;
using System.Collections.Generic;
using FreeLibSet.Core;

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

      ToolStripItemCollection ParentItems; // куда будем добавлять
      if (parent == null)
        ParentItems = _Menu.Items;
      else
      {
        if (!ItemDict.ContainsKey(parent.CategoryAndName))
          throw new InvalidOperationException("Родительская команда " + parent.ToString() + " не была добавлен в меню");
        ParentItems = ItemDict[parent.CategoryAndName].DropDownItems;
      }

      if (item.GroupBegin)
        AddSeparator(ParentItems);
      ToolStripMenuItem ThisMenuItem = new ToolStripMenuItem();
      ItemDict.Add(item.CategoryAndName, ThisMenuItem);
      EFPUIMenuItemObj UIObj = new EFPUIMenuItemObj(this, item, ThisMenuItem);
      UIObj.SetAll();
      ParentItems.Add(ThisMenuItem);
      if (item.GroupEnd)
        AddSeparator(ParentItems);
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
      EFPCommandItem Parent = item.Parent;
      if (Parent != null)
      {
        if (!ItemDict.ContainsKey(Parent.CategoryAndName))
          Add(Parent);
      }
      Add(item, Parent);
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
        bool HasDelayed = false;
        foreach (EFPCommandItem Item in items)
        {
          if (Item.MenuUsage)
          {
            if (ItemDict.ContainsKey(Item.CategoryAndName))
              continue; // уже добавили на предыдущем проходе
            if (Item.Parent != null)
            {
              if (!Item.Parent.MenuUsage)
                throw new InvalidOperationException("Нельзя присоединить команду \"" + Item.ToString() +
                  "\" к меню, потому что ее родительская команда \"" + Item.Parent.ToString() +
                  "\" не относится к меню");
              if (!items.Contains(Item.Parent))
                throw new InvalidOperationException("Нельзя присоединить команду \"" + Item.ToString() +
                  "\" к меню, потому что ее родительская команда \"" + Item.Parent.ToString() +
                  "\" не входит в список команд");
              if (!ItemDict.ContainsKey(Item.Parent.CategoryAndName))
              {
                HasDelayed = true;
                continue;
              }
            }
            Add(Item);
          }
        }
        if (!HasDelayed)
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

      EFPCommandItem Parent = item.Parent;
      if (Parent != null)
      {
        if (!ItemDict.ContainsKey(Parent.CategoryAndName))
          Add(Parent);
      }

      if (ItemDict.ContainsKey(item.CategoryAndName))
        return; // повторное добавление темы

      ToolStripItemCollection ParentItems; // куда будем добавлять
      if (Parent == null)
        ParentItems = _Menu.Items;
      else
      {
        //if (ht[Parent] == null)
        //  throw new InvalidOperationException("Родительская команда " + Parent.ToString() + " не была добавлен в меню");
        ParentItems = ItemDict[Parent.CategoryAndName].DropDownItems;
      }

      ToolStripMenuItem BeforeMenuItem;
      if (!ItemDict.TryGetValue(before.CategoryAndName, out BeforeMenuItem))
        throw new ArgumentException("Команда " + before.ToString() + " не была добавлена в меню", "before");
      int Index = ParentItems.IndexOf(BeforeMenuItem);
      if (Index < 0)
        throw new ArgumentException("Команда " + before.ToString() + " располагается в другом меню", "before");

      if (item.GroupBegin)
        InsertSeparator(ParentItems, ref Index);
      ToolStripMenuItem ThisMenuItem = new ToolStripMenuItem();
      ItemDict.Add(item.CategoryAndName, ThisMenuItem);
      EFPUIMenuItemObj UIObj = new EFPUIMenuItemObj(this, item, ThisMenuItem);
      UIObj.SetAll();
      ParentItems.Insert(Index, ThisMenuItem);
      Index++;
      if (item.GroupEnd /*|| Before.GroupEnd*/)
        InsertSeparator(ParentItems, ref Index);
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
          ToolStripMenuItem Item = ToolStripMenuItems[DefaultCommandItem];
          if (Item != null)
            Item.Font = new Font(Item.Font, FontStyle.Bold);
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


      int LastIndex = -1;
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
          EFPCommandItem Item = mi.Tag as EFPCommandItem;
          if (Item != null) // 09.06.2015
          {
            if (Item.Visible)
            {
              LastIndex = i - 1;
              break;
            }
          }
        }
      }

      bool WantsSep = false;
      for (int i = 0; i <= LastIndex; i++)
      {
        ToolStripItem mi = parentItems[i];
        if (mi is ToolStripSeparator)
        {
          if (mi == windowMenuSeparator)
            mi.Visible = true;//EFPApp.Forms.Length > 0;
          else
            mi.Visible = WantsSep;
          WantsSep = false;
        }
        else
        {
          EFPCommandItem Item = mi.Tag as EFPCommandItem;
          if (Item != null) // 09.06.2015
          {
            if (Item.Visible)
              WantsSep = true;
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
          int idx = EFPApp.MainImages.Images.IndexOfKey(Item.ImageKey);
          if (idx < 0)
            _MenuItem.Image = null;
          else
            _MenuItem.Image = EFPApp.MainImages.Images[idx];
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
          EFPUIObjBase Obj = _Owner[commandItem];
          if (Obj == null)
            return null;
          else
            return ((EFPUIMenuItemObj)Obj).MenuItem;
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
          EFPUIObjBase Obj = _Owner[category, name];
          if (Obj == null)
            return null;
          else
            return ((EFPUIMenuItemObj)Obj).MenuItem;
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
          ToolStripMenuItem Item = ToolStripMenuItems[_DefaultCommandItem];
          if (Item != null)
            Item.ResetFont();
        }
        _DefaultCommandItem = value;
        if (_DefaultCommandItem != null)
        {
          ToolStripMenuItem Item = ToolStripMenuItems[_DefaultCommandItem];
          if (Item != null)
            Item.Font = new Font(Item.Font, FontStyle.Bold);
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

      ToolStripMenuItem WindowMenuItem;
      if (!ItemDict.TryGetValue(windowCommandItem.CategoryAndName, out WindowMenuItem))
        throw new ArgumentException("Команда " + windowCommandItem.ToString() + " не была добавлена в меню", "windowCommandItem");

      if (WindowMenuItem.DropDownItems.Count > 0)
      {
        // Надо добавить сепаратор
        WindowMenuSeparator = new ToolStripSeparator();
        WindowMenuItem.DropDownItems.Add(WindowMenuSeparator);
      }

      ((MenuStrip)Menu).MdiWindowListItem = (ToolStripMenuItem)WindowMenuItem;
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
    /// Присвоединяет меню к управляющему элементу
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

      control.ContextMenuStrip = Menu;
      control.Disposed += new EventHandler(ControlDisposed);
      InitSeparatorVisiblity();
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
