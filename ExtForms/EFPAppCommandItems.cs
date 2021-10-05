using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;

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

namespace AgeyevAV.ExtForms
{
  #region Перечисление EFPAppStdCommandItems

  /// <summary>
  /// Команды главного меню, которые самостоятельно умеет создавать
  /// </summary>
  public enum EFPAppStdCommandItems
  {
    // При добавлении новых команд не забыть пополнить списки в 
    // EFPAppCommandItems.InitCommandDict() и CreateCommand()

    /// <summary>
    /// Меню "Файл"
    /// </summary>
    MenuFile,

    /// <summary>
    /// Меню "Файл" - "Отправить"
    /// </summary>
    MenuSendTo,

    /// <summary>
    /// Меню "Правка"
    /// </summary>
    MenuEdit,

    /// <summary>
    /// Меню "Вид"
    /// </summary>
    MenuView,

    /// <summary>
    /// Меню "Окно"
    /// </summary>
    MenuWindow,

    /// <summary>
    /// Меню "Справка"
    /// </summary>
    MenuHelp,

    /// <summary>
    /// Файл-Создать
    /// </summary>
    New,

    /// <summary>
    /// Файл-Открыть
    /// </summary>
    Open,

    /// <summary>
    /// Файл-Сохранить
    /// </summary>
    Save,

    /// <summary>
    /// Файл-Сохранить как
    /// </summary>
    SaveAs,

    /// <summary>
    /// Файл-Параметры страницы
    /// </summary>
    PageSetup,

    /// <summary>
    /// Файл-Предварительный просмотр
    /// </summary>
    PrintPreview,

    /// <summary>
    /// Файл-Печать
    /// </summary>
    Print,

    /// <summary>
    /// Кнопка печати на принтере по умолчанию без вывода диалога параметров
    /// </summary>
    PrintDefault,

    /// <summary>
    /// Файл-Выход
    /// </summary>
    Exit,

    /// <summary>
    /// Не следует использовать.
    /// Управляющие элементы используют дейтсвующие файловые ассоциации, заданные в операционной системе.
    /// </summary>
    SendToInternetExplorer,

    /// <summary>
    /// Не следует использовать
    /// Управляющие элементы используют дейтсвующие файловые ассоциации, заданные в операционной системе.
    /// </summary>
    SendToNotepad,

    /// <summary>
    /// Файл-Отправить-Microsoft Word
    /// </summary>
    SendToMicrosoftWord,

    /// <summary>
    /// Файл-Отправить-Microsoft Excel
    /// </summary>
    SendToMicrosoftExcel,

    /// <summary>
    /// Файл-Отправить-OpenOffice/LibreOffice Writer
    /// </summary>
    SendToOpenOfficeWriter,

    /// <summary>
    /// Файл-Отправить-OpenOffice/LibreOffice Calc
    /// </summary>
    SendToOpenOfficeCalc,

    /// <summary>
    /// Правка-Отменить
    /// </summary>
    Undo,

    /// <summary>
    /// Правка-Повторить
    /// </summary>
    Redo,

    /// <summary>
    /// Правка-Вырезать
    /// </summary>
    Cut,

    /// <summary>
    /// Правка-Копировать
    /// </summary>
    Copy,

    /// <summary>
    /// Правка-Вставить
    /// </summary>
    Paste,

    /// <summary>
    /// Правка-Специальная вставка
    /// </summary>
    PasteSpecial,

    /// <summary>
    /// Правка-Найти
    /// </summary>
    Find,

    /// <summary>
    /// Правка-Инкрементный поиск (по первым буквам)
    /// </summary>
    IncSearch,

    /// <summary>
    /// Правка-Найти дальше
    /// </summary>
    FindNext,

    /// <summary>
    /// Правка-Заменить
    /// </summary>
    Replace,

    /// <summary>
    /// Правка-Перейти
    /// </summary>
    Goto,

    /// <summary>
    /// Правка-Выделить все
    /// </summary>
    SelectAll,

    /// <summary>
    /// Вид-Обновить (в том числе, перестроить отчет)
    /// </summary>
    Refresh,

    /// <summary>
    /// Вид-На весь экран
    /// </summary>
    FullScreen,

    /// <summary>
    /// Окно-Сверху вниз
    /// </summary>
    TileHorizontal,

    /// <summary>
    /// Окно-Слева направо
    /// </summary>
    TileVertical,

    /// <summary>
    /// Окно-Каскадом
    /// </summary>
    Cascade,

    /// <summary>
    /// Окно-Упорядочить значки
    /// </summary>
    ArrangeIcons,

    /// <summary>
    /// Окно-Закрыть все
    /// </summary>
    CloseAll,

    /// <summary>
    /// Окно-Закрыть все, кроме текущего
    /// </summary>
    CloseAllButThis, // 21.08.2018

    /// <summary>
    /// Окно-Другие окна (список открытых окон)
    /// </summary>
    OtherWindows, // 22.08.2018  

    /// <summary>
    /// Окно-Новое главное окно
    /// </summary>
    NewMainWindow, // 20.08.2018

    /// <summary>
    /// Окно-Композиции рабочего стола
    /// </summary>
    SavedCompositions, // 20.08.2018

    /// <summary>
    /// Справка-Содержание
    /// </summary>
    HelpContents,

    /// <summary>
    /// Справка-Указатель
    /// </summary>
    HelpIndex,

    /// <summary>
    /// Справка-Для текущего элемента (используется только в дочерней форме, если задан контекст справки)
    /// </summary>
    ContextHelp,

    /// <summary>
    /// Справка-О программе
    /// </summary>
    About
  }

  #endregion

  /// <summary>
  /// Глобальный список элементов для главного меню и 
  /// панелей инструментов
  /// </summary>
  public sealed class EFPAppCommandItems : EFPCommandItems
  {
    #region Защищенный конструктор

    internal EFPAppCommandItems()
    {
      _GlobalShortCuts = new List<EFPCommandItem>();
      AddFocus();
    }

    /// <summary>
    /// Текстовое представление для отладки
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return "Общий список команд. Count=" + Count.ToString();
    }

    #endregion

    #region Стандартные команды

    /// <summary>
    /// Создать стандартную команду для локального меню.
    /// Если команда в главном меню была добавлена, то создается CommandItem,
    /// соединенная с мастер-командой в главном меню. Иначе создается независимая команда
    /// </summary>
    /// <param name="stdItem">Идентификатор стандартной команды</param>
    /// <returns>Созданная команда меню</returns>
    public EFPCommandItem CreateContext(EFPAppStdCommandItems stdItem)
    {
      if (this[stdItem] == null)
        return EFPAppCommandItems.CreateStdCommand(stdItem);
      else
        return new EFPCommandItem(this[stdItem]);
    }

    /// <summary>
    /// Создание стандартной команды.
    /// Команда создается, но ни к чему не присоединяется.
    /// Устанавливаются свойства Category, Name, MenuText, ToolTipText, ShortCut и ImageKey
    /// Обработчик Click не устанавливается. Сепараторы и свойство Enabled не задаются
    /// </summary>
    /// <param name="stdItem">Перечислимое значение для стандартной команды</param>
    /// <returns>Созданная команда без обработчика</returns>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Используйте статический метод CreateStdCommand() или перегрузку EFPCommandItems.Add()", false)]
    public EFPCommandItem CreateCommand(EFPAppStdCommandItems stdItem)
    {
      return CreateStdCommand(stdItem);
    }

    /// <summary>
    /// Создание стандартной команды.
    /// Команда создается, но ни к чему не присоединяется.
    /// Устанавливаются свойства Category, Name, MenuText, ToolTipText, ShortCut и ImageKey
    /// Обработчик Click не устанавливается. Сепараторы и свойство Enabled не задаются
    /// </summary>
    /// <param name="stdItem">Перечислимое значение для стандартной команды</param>
    /// <returns>Созданная команда без обработчика</returns>
    public static EFPCommandItem CreateStdCommand(EFPAppStdCommandItems stdItem)
    {
      string Category, Name;
      GetStdCommandCategoryAndName(stdItem, out Category, out Name);
      EFPCommandItem ci = new EFPCommandItem(Category, Name);

      switch (stdItem)
      {
        #region Файл

        case EFPAppStdCommandItems.MenuFile:
          ci.MenuText = "&Файл";
          break;

        case EFPAppStdCommandItems.New:
          ci.MenuText = "Создать";
          ci.ToolTipText = "Создать";
          ci.ShortCut = Keys.Control | Keys.N;
          ci.ImageKey = "New";
          break;

        case EFPAppStdCommandItems.Open:
          ci.MenuText = "Открыть";
          ci.ToolTipText = "Открыть";
          ci.ShortCut = Keys.Control | Keys.O;
          ci.ImageKey = "Open";
          break;

        case EFPAppStdCommandItems.Save:
          ci.MenuText = "Сохранить";
          ci.ToolTipText = "Сохранить";
          ci.ShortCut = Keys.Control | Keys.S;
          ci.ImageKey = "Save";
          break;

        case EFPAppStdCommandItems.SaveAs:
          ci.MenuText = "Сохранить как";
          break;

        case EFPAppStdCommandItems.PageSetup:
          ci.MenuText = "Параметры страницы ...";
          ci.ToolTipText = "Параметры страницы";
          ci.ImageKey = "PageSetup";
          break;

        case EFPAppStdCommandItems.PrintPreview:
          ci.MenuText = "Предварительный просмотр";
          ci.ToolTipText = "Предварительный просмотр";
          ci.ImageKey = "Preview";
          break;

        case EFPAppStdCommandItems.Print:
          ci.MenuText = "Печать ...";
          ci.ToolTipText = "Печать";
          ci.ShortCut = Keys.Control | Keys.P;
          ci.ImageKey = "Print";
          break;

        case EFPAppStdCommandItems.PrintDefault:
          //      ci.MenuText = "Печать";
          ci.ToolTipText = "Печать на принтере по умолчанию";
          ci.ImageKey = "Print";
          break;

        case EFPAppStdCommandItems.Exit:
          ci.MenuText = "В&ыход";
          ci.ImageKey = "Exit";
          break;

        #endregion

        #region Файл - Отправить

        case EFPAppStdCommandItems.MenuSendTo:
          ci.MenuText = "Отправит&ь";
          break;

        case EFPAppStdCommandItems.SendToInternetExplorer:
          ci.MenuText = "Internet Explorer";
          ci.ImageKey = "InternetExplorer";
          ci.ToolTipText = "Открыть в Internet Explorer";
          break;
        case EFPAppStdCommandItems.SendToMicrosoftWord:
          ci.MenuText = "Microsoft Word";
          ci.ImageKey = "MicrosoftWord";
          ci.ToolTipText = "Открыть в Microsoft Word";
          break;
        case EFPAppStdCommandItems.SendToMicrosoftExcel:
          ci.MenuText = "Microsoft Excel";
          ci.ImageKey = "MicrosoftExcel";
          ci.ToolTipText = "Открыть в Microsoft Excel";
          break;
        case EFPAppStdCommandItems.SendToOpenOfficeWriter:
          ci.MenuText = EFPApp.OpenOfficeKindName + " Writer";
          ci.ImageKey = "OpenOfficeWriter";
          ci.ToolTipText = "Открыть в " + EFPApp.OpenOfficeKindName + " Writer";
          break;
        case EFPAppStdCommandItems.SendToOpenOfficeCalc:
          ci.MenuText = EFPApp.OpenOfficeKindName + " Calc";
          ci.ImageKey = "OpenOfficeCalc";
          ci.ToolTipText = "Открыть в " + EFPApp.OpenOfficeKindName + " Calc";
          break;
        case EFPAppStdCommandItems.SendToNotepad:
          ci.MenuText = "Блокнот";
          ci.ImageKey = "Notepad";
          ci.ToolTipText = "Открыть в Блокноте";
          break;

        #endregion

        #region Правка

        case EFPAppStdCommandItems.MenuEdit:
          ci.MenuText = "&Правка";
          break;

        case EFPAppStdCommandItems.Undo:
          ci.MenuText = "&Отменить";
          ci.ToolTipText = "Отменить";
          ci.ImageKey = "Undo";
          ci.ShortCut = Keys.Control | Keys.Z;
          break;

        case EFPAppStdCommandItems.Redo:
          ci.MenuText = "Пов&торить";
          ci.ToolTipText = "Повторить";
          ci.ImageKey = "Redo";
          ci.ShortCut = Keys.Control | Keys.Y;
          break;

        case EFPAppStdCommandItems.Cut:
          ci.MenuText = "&Вырезать";
          ci.ShortCut = Keys.Control | Keys.X;
          //ci.MenuRightText = "Ctrl+X";
          ci.ImageKey = "Cut";
          break;

        case EFPAppStdCommandItems.Copy:
          ci.MenuText = "&Копировать";
          ci.ShortCut = Keys.Control | Keys.C;
          //ci.MenuRightText = "Ctrl+С";
          ci.ImageKey = "Copy";
          break;

        case EFPAppStdCommandItems.Paste:
          ci.MenuText = "Вст&авить";
          ci.ShortCut = Keys.Control | Keys.V;
          //ci.MenuRightText = "Ctrl+V";
          ci.ImageKey = "Paste";
          break;

        case EFPAppStdCommandItems.PasteSpecial:
          ci.MenuText = "Сп&ециальная вставка ...";
          break;

        case EFPAppStdCommandItems.Find:
          ci.MenuText = "&Найти";
          ci.ShortCut = Keys.Control | Keys.F;
          ci.ImageKey = "Find";
          break;

        case EFPAppStdCommandItems.IncSearch:
          ci.MenuText = "Найти по первым буквам";
          ci.ShortCut = Keys.Control | Keys.K;
          ci.ImageKey = "IncSearch";
          break;

        case EFPAppStdCommandItems.FindNext:
          ci.MenuText = "Найти &далее";
          ci.ShortCut = Keys.F3;
          ci.ImageKey = "FindNext";
          break;

        case EFPAppStdCommandItems.Replace:
          ci.MenuText = "Заменить";
          ci.ShortCut = Keys.Control | Keys.H;
          ci.ImageKey = "Replace";
          break;

        case EFPAppStdCommandItems.Goto:
          ci.MenuText = "Перейти";
          ci.ShortCut = Keys.Control | Keys.G;
          ci.ImageKey = "Goto";
          break;

        case EFPAppStdCommandItems.SelectAll:
          ci.MenuText = "В&ыделить все";
          ci.ShortCut = Keys.Control | Keys.A;
          break;

        #endregion

        #region Вид

        case EFPAppStdCommandItems.MenuView:
          ci.MenuText = "&Вид";
          break;

        case EFPAppStdCommandItems.Refresh:
          ci.MenuText = "Обновить";
          ci.ShortCut = Keys.F5;
          ci.ImageKey = "Refresh";
          break;

        case EFPAppStdCommandItems.FullScreen:
          ci.MenuText = "Во весь экран";
          ci.ImageKey = "FullScreen";

          // плохо работает
          // срабатывает дважды при выходе из полноэкранного режима
          // ci.ShortCut = Keys.Alt | Keys.Enter;
          break;

        #endregion

        #region Окно

        case EFPAppStdCommandItems.MenuWindow:
          ci.MenuText = "&Окно";
          break;

        case EFPAppStdCommandItems.TileHorizontal:
          ci.MenuText = "Сверху вниз";
          ci.ImageKey = "TileHorizontal";
          break;

        case EFPAppStdCommandItems.TileVertical:
          ci.MenuText = "Слева направо";
          ci.ImageKey = "TileVertical";
          break;

        case EFPAppStdCommandItems.Cascade:
          ci.MenuText = "Каскадом";
          ci.ImageKey = "Cascade";
          break;

        case EFPAppStdCommandItems.ArrangeIcons:
          ci.MenuText = "Упорядочить значки";
          ci.ImageKey = "ArrangeIcons";
          break;

        case EFPAppStdCommandItems.CloseAll:
          ci.MenuText = "Закрыть все";
          ci.ImageKey = "CloseAll";
          break;

        case EFPAppStdCommandItems.CloseAllButThis:
          ci.MenuText = "Закрыть все, кроме текущего окна";
          ci.ImageKey = "CloseAllButThis";
          break;

        case EFPAppStdCommandItems.OtherWindows:
          ci.MenuText = "Другие окна ...";
          ci.ImageKey = "WindowList";
          break;

        case EFPAppStdCommandItems.NewMainWindow:
          ci.MenuText = "Новое главное окно";
          ci.ImageKey = "NewMainWindow";
          break;

        case EFPAppStdCommandItems.SavedCompositions:
          ci.MenuText = "Композиции рабочего стола";
          ci.ImageKey = "SavedCompositions";
          break;

        #endregion

        #region Справка

        case EFPAppStdCommandItems.MenuHelp:
          ci.MenuText = "&Справка";
          break;

        case EFPAppStdCommandItems.HelpContents:
          ci.MenuText = "&Содержание";
          ci.ShortCut = Keys.Control | Keys.F1;
          ci.ImageKey = "HelpContents";
          ci.ToolTipText = "Содержание справки";
          break;

        case EFPAppStdCommandItems.HelpIndex:
          ci.MenuText = "&Указатель";
          ci.ShortCut = Keys.Shift | Keys.F1;
          ci.ImageKey = "HelpIndex";
          break;

        case EFPAppStdCommandItems.ContextHelp:
          ci.MenuText = "Контекстная помощь";
          ci.ShortCut = Keys.F1;
          ci.ImageKey = "Help";
          break;

        case EFPAppStdCommandItems.About:
          ci.MenuText = "О программе";
          ci.ImageKey = "About";
          break;

        #endregion

        default:
          throw new ArgumentException("Неизвестная команда меню " + stdItem.ToString());
      }
      return ci;
    }

    #region События

    /// <summary>
    /// Вызывается при изменении числа открытых дочерних MDI-окон
    /// </summary>
    public event EventHandler MdiCildrenChanged;

    internal void OnMdiCildrenChanged(object sender, EventArgs args)
    {
      try
      {
        if (MdiCildrenChanged != null)
          MdiCildrenChanged(null, EventArgs.Empty);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка обработки события MdiCildrenChanged");
      }
    }

    #endregion

    #region Обработчики для стандартных команд

    #region Файл

    /// <summary>
    /// Вызывает EFPApp.Exit()
    /// </summary>
    /// <param name="sender">Не используется</param>
    /// <param name="args">Не используется</param>
    public void Exit_Click(object sender, EventArgs args)
    {
      EFPApp.Exit();
    }

    #endregion

    #region Окно

#pragma warning disable 0618 // обход [Obsolete]

    /// <summary>
    /// Обработка команды "Сверху вниз"
    /// </summary>
    /// <param name="sender">Не используется</param>
    /// <param name="args">Не используется</param>
    [Obsolete("Используйте класс EFPAppCommandItemHelpers", false)]
    public void TileHorizontal_Click(object sender, EventArgs args)
    {
      EFPApp.LayoutMdiChildren(MdiLayout.TileHorizontal);
    }

    /// <summary>
    /// Обработка команды "Слева направо"
    /// </summary>
    /// <param name="sender">Не используется</param>
    /// <param name="args">Не используется</param>
    [Obsolete("Используйте класс EFPAppCommandItemHelpers", false)]
    public void TileVertical_Click(object sender, EventArgs args)
    {
      EFPApp.LayoutMdiChildren(MdiLayout.TileVertical);
    }

    /// <summary>
    /// Обработка команды "Каскадом"
    /// </summary>
    /// <param name="sender">Не используется</param>
    /// <param name="args">Не используется</param>
    [Obsolete("Используйте класс EFPAppCommandItemHelpers", false)]
    public void Cascade_Click(object sender, EventArgs args)
    {
      EFPApp.LayoutMdiChildren(MdiLayout.Cascade);
    }

    /// <summary>
    /// Обработка команды "Упорядочить значки"
    /// </summary>
    /// <param name="sender">Не используется</param>
    /// <param name="args">Не используется</param>
    [Obsolete("Используйте класс EFPAppCommandItemHelpers", false)]
    public void ArrangeIcons_Click(object sender, EventArgs args)
    {
      EFPApp.LayoutMdiChildren(MdiLayout.ArrangeIcons);
    }

    /// <summary>
    /// Обработка команды "Закрыть все"
    /// </summary>
    /// <param name="sender">Не используется</param>
    /// <param name="args">Не используется</param>
    [Obsolete("Используйте класс EFPAppCommandItemHelpers", false)]
    public void CloseAll_Click(object sender, EventArgs args)
    {
      EFPApp.CloseAllMdiChildren();
    }

    /// <summary>
    /// Добавляет команды меню "Окно" интерфейса MDI.
    /// Создаются команды упорядочивания и закрыть все. Устанавливаются
    /// обработчики команд. Отслеживается открытие / закрытие окон для 
    /// определения видимости команд. 
    /// После команды "Закрыть все" обычно должен быть присоединен список окон с помощью EFPMainMenu.InitWindowMenu
    /// список окон
    /// </summary>
    /// <param name="menuWindow"></param>
    [Obsolete("Используйте класс EFPAppCommandItemHelpers", false)]
    public void AddMdiWindowCommands(EFPCommandItem menuWindow)
    {
      Add(EFPAppStdCommandItems.TileHorizontal, menuWindow);
      this[EFPAppStdCommandItems.TileHorizontal].GroupBegin = true;
      this[EFPAppStdCommandItems.TileHorizontal].Click += new EventHandler(TileHorizontal_Click);

      Add(EFPAppStdCommandItems.TileVertical, menuWindow);
      this[EFPAppStdCommandItems.TileVertical].Click += new EventHandler(TileVertical_Click);

      Add(EFPAppStdCommandItems.Cascade, menuWindow);
      this[EFPAppStdCommandItems.Cascade].Click += new EventHandler(Cascade_Click);

      Add(EFPAppStdCommandItems.ArrangeIcons, menuWindow);
      this[EFPAppStdCommandItems.ArrangeIcons].GroupEnd = true;
      this[EFPAppStdCommandItems.ArrangeIcons].Click += new EventHandler(ArrangeIcons_Click);

      Add(EFPAppStdCommandItems.CloseAll, menuWindow);
      this[EFPAppStdCommandItems.CloseAll].GroupBegin = true;
      this[EFPAppStdCommandItems.CloseAll].GroupEnd = true; // 21.01.2012 - не работает
      this[EFPAppStdCommandItems.CloseAll].Click += new EventHandler(CloseAll_Click);

      MdiCildrenChanged += new EventHandler(This_MdiCildrenChanged);
      This_MdiCildrenChanged(null, null);

    }

    private void This_MdiCildrenChanged(object sender, EventArgs args)
    {
      bool HasWindows = EFPApp.MdiChildren.Length > 0;

      this[EFPAppStdCommandItems.TileHorizontal].Enabled = HasWindows;
      this[EFPAppStdCommandItems.TileVertical].Enabled = HasWindows;
      this[EFPAppStdCommandItems.Cascade].Enabled = HasWindows;
      this[EFPAppStdCommandItems.ArrangeIcons].Enabled = HasWindows;
      this[EFPAppStdCommandItems.CloseAll].Enabled = HasWindows;
    }

#pragma warning restore 0618

    #endregion

    #endregion

    #endregion

    #region Коды для стандартных команд

    /// <summary>
    /// Возвращает для стандартной команды категорию и имя.
    /// Этот метод не зависит от наличия реальных команд в меню
    /// </summary>
    /// <param name="stdItem">Стандартная команда</param>
    /// <param name="category">Сюда записывается категория команды</param>
    /// <param name="name">Сюда записывается имя команды</param>
    public static void GetStdCommandCategoryAndName(EFPAppStdCommandItems stdItem, out string category, out string name)
    {
      string s;
      if (_StdCommandDict.TryGetValue(stdItem, out s))
      {
        int p = s.IndexOf('|');
        category = s.Substring(0, p);
        name = s.Substring(p + 1);
      }
      else
        throw new ArgumentException("Неизвестная команда " + stdItem.ToString(), "stdItem");
    }

    /// <summary>
    /// Найти значение перечисления для стандартной команды по категории и имени
    /// </summary>
    /// <param name="category">Категория команды</param>
    /// <param name="name">Имя команды</param>
    /// <param name="stdItem">Сюда записываектся перечислимое значение, если команда найдена</param>
    /// <returns>true, если команда найдена</returns>
    public static bool FindStdCommand(string category, string name, out EFPAppStdCommandItems stdItem)
    {
      if (_StdCommandDict.TryGetKey(category + "|" + name, out stdItem))
        return true;
      else
      {
        stdItem = (EFPAppStdCommandItems)(-1);
        return false;
      }
    }

    /// <summary>
    /// Найти значение перечисления для стандартной команды по категории и имени.
    /// Если переданные аргументы не соответствуют стандартной команде, то генерируется исключение
    /// </summary>
    /// <param name="category">Категория команды</param>
    /// <param name="name">Имя команды</param>
    /// <returns>Найденное перечислимое значение</returns>
    public static EFPAppStdCommandItems GetStdCommand(string category, string name)
    {
      EFPAppStdCommandItems Res;
      if (FindStdCommand(category, name, out Res))
        return Res;
      else
        throw new ArgumentException("Неизвестная стандартная команда с категорией \"" + category + "\" и именем \"" + name + "\"");
    }

    private static BidirectionalDictionary<EFPAppStdCommandItems, string> _StdCommandDict = InitCommandDict();

    private static BidirectionalDictionary<EFPAppStdCommandItems, string> InitCommandDict()
    {
      string[] a = new string[]{
        "|File",
        "|SendTo",
        "|Edit",
        "|View",
        "|Window",
        "|Help",

        "File|New",
        "File|Open",
        "File|Save",
        "File|SaveAs",
        "File|PageSetup",
        "File|PrintPreview",
        "File|Print",
        "File|PrintDefault",
        "File|Exit",

        "SendTo|InternetExplorer",
        "SendTo|Notepad",
        "SendTo|MicrosoftWord",
        "SendTo|MicrosoftExcel",
        "SendTo|OpenOfficeWriter",
        "SendTo|OpenOfficeCalc",

        "Edit|Undo",
        "Edit|Redo",
        "Edit|Cut",
        "Edit|Copy",
        "Edit|Paste",
        "Edit|PasteSpecial",
        "Edit|Find",
        "Edit|IncSearch",
        "Edit|FindNext",
        "Edit|Replace",
        "Edit|Goto",
        "Edit|SelectAll",

        "View|Refresh",
        "View|FullScreen",

        "Window|TileHorizontal",
        "Window|TileVertical",
        "Window|Cascade",
        "Window|ArrangeIcons",
        "Window|CloseAll",
        "Window|CloseAllButThis",
        "Window|NewMainWindow",
        "Window|OtherWindows",
        "Window|SavedCompositions",

        "Help|Contents",
        "Help|Index",
        "Help|Context",
        "Help|About"
      };

      MinMaxInt Range = DataTools.GetEnumRange(typeof(EFPAppStdCommandItems));

#if DEBUG
      if (Range.MaxValue != (a.Length - 1))
        throw new BugException("Неправильный список");
#endif

      BidirectionalDictionary<EFPAppStdCommandItems, string> Dict = new BidirectionalDictionary<EFPAppStdCommandItems, string>(a.Length);
      for (int i = 0; i < a.Length; i++)
      {
        Dict.Add((EFPAppStdCommandItems)i, a[i]);
      }

      return Dict;
    }

    /// <summary>
    /// Возвращает true, если данная команда является стандартной
    /// </summary>
    /// <param name="commandItem">Проверяемая команда</param>
    /// <param name="stdItem">Сюда записывается код стандартной команды</param>
    /// <returns>true, если команда является стандартной</returns>
    public static bool IsStdCommandItem(EFPCommandItem commandItem, out EFPAppStdCommandItems stdItem)
    {
      if (commandItem == null)
      {
        stdItem = (EFPAppStdCommandItems)(-1);
        return false;
      }
      return FindStdCommand(commandItem.Category, commandItem.Name, out stdItem);
    }

    /// <summary>
    /// Возвращает true, если данная команда является стандартной.
    /// Вряд ли эта перегрузка метода является полезной.
    /// </summary>
    /// <param name="commandItem">Проверяемая команда</param>
    /// <returns>true, если команда является стандартной</returns>
    public static bool IsStdCommandItem(EFPCommandItem commandItem)
    {
      EFPAppStdCommandItems StdItem;
      return IsStdCommandItem(commandItem, out StdItem);
    }

    #endregion

    #region Глобальные сочетания клавиш

    /// <summary>
    /// Сюда можно добавить команды, содержащие сочетания клавиш, которые будут
    /// обработаны в любом окне программы, включая модальные
    /// Например, для вызова окна калькулятора
    /// </summary>
    public List<EFPCommandItem> GlobalShortCuts { get { return _GlobalShortCuts; } }
    private List<EFPCommandItem> _GlobalShortCuts;

    #endregion
  }

  /// <summary>
  /// Задает список кнопок для одной панели инструментов
  /// </summary>
  public sealed class EFPAppToolBarCommandItems : List<EFPCommandItem>, IObjectWithCode
  {
    #region Конструктор

    /// <summary>
    /// Создает пустую панель
    /// </summary>
    /// <param name="name">Имя панели</param>
    public EFPAppToolBarCommandItems(string name)
    {
      if (String.IsNullOrEmpty(name))
        throw new ArgumentNullException("name");
      _Name = name;

      _DefaultVisible = true;
      _DefaultDock = DockStyle.Top;
    }

    #endregion

    #region Свойства панели

    /// <summary>
    /// Имя панели инструментов, используемое при сохранении настроек главного окна.
    /// Задается в конструкторе
    /// </summary>
    public string Name { get { return _Name; } }
    private string _Name;

    /// <summary>
    /// Отображаемое имя панели для меню "Вид" - "Панели инструментов".
    /// Если свойство не установлено в явном виде, используется значение свойства Name.
    /// </summary>
    public string DisplayName
    {
      get
      {
        if (String.IsNullOrEmpty(_DisplayName))
          return _Name;
        else
          return _DisplayName;
      }
      set { _DisplayName = value; }
    }
    private string _DisplayName;

    /// <summary>
    /// Должна ли быть по умолчанию панель видима в главном окне.
    /// По умолчанию - true
    /// </summary>
    public bool DefaultVisible { get { return _DefaultVisible; } set { _DefaultVisible = value; } }
    private bool _DefaultVisible;

    /// <summary>
    /// С какой стороны (по умолчанию) располагается панель.
    /// По умолчанию - DockStyle.Top
    /// </summary>
    public DockStyle DefaultDock
    {
      get { return _DefaultDock; }
      set
      {
        switch (value)
        {
          case DockStyle.Top:
          case DockStyle.Bottom:
          case DockStyle.Left:
          case DockStyle.Right:
            _DefaultDock = value;
            break;
          default:
            throw new ArgumentException();
        }
      }
    }
    private DockStyle _DefaultDock;

    /// <summary>
    /// Возвращает DisplayName
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return DisplayName;
    }

    #endregion

    #region IObjectWithCode Members

    string IObjectWithCode.Code { get { return _Name; } }

    #endregion
  }

  /// <summary>
  /// Список объектов для свойства EFPApp.ToolBars.
  /// </summary>
  public sealed class EFPAppToolBarList : NamedList<EFPAppToolBarCommandItems>
  {
    #region Альтернативные методы добавления панелей

    /// <summary>
    /// Создает и добавляет новую панель инструментов.
    /// </summary>
    /// <param name="name">Имя панели для сохранения настроек</param>
    /// <param name="displayName">Заголовок панели при показе пользователю</param>
    /// <returns>Созданный список команд для заполнения</returns>
    public EFPAppToolBarCommandItems Add(string name, string displayName)
    {
      EFPAppToolBarCommandItems tb = new EFPAppToolBarCommandItems(name);
      tb.DisplayName = displayName;
      base.Add(tb);
      return tb;
    }

    /// <summary>
    /// Создает и добавляет новую панель инструментов.
    /// Используется заголовок панели по умолчанию.
    /// </summary>
    /// <param name="name">Имя панели для сохранения настроек</param>
    /// <returns>Созданный список команд для заполнения</returns>
    public EFPAppToolBarCommandItems Add(string name)
    {
      EFPAppToolBarCommandItems tb = new EFPAppToolBarCommandItems(name);
      base.Add(tb);
      return tb;
    }

    #endregion
  }
}
