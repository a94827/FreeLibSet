// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using FreeLibSet.Collections;
using FreeLibSet.Core;

namespace FreeLibSet.Forms
{
  #region Перечисление EFPAppStdCommandItems

  /// <summary>
  /// Стандартные команды меню.
  /// Также перечисляются подменю верхнего уровня для главного меню программы (элементы перечисления MenuXXX).
  /// Объекты <see cref="EFPCommandItem"/> можно создавать с помощью <see cref="EFPAppCommandItems.CreateStdCommand(EFPAppStdCommandItems)"/>.
  /// Для главного меню программы рекомендуется создать объект <see cref="EFPAppCommandItemHelpers"/>.
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
    /// Меню "Буфер обмена". Используется в локальном меню табличного просмотра для уменьшения длины меню
    /// </summary>
    MenuClipboard,

    /// <summary>
    /// Меню "Поиск". Используется в локальном меню табличного просмотра для уменьшения длины меню
    /// </summary>
    MenuSearch,

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
    /// Управляющие элементы используют действующие файловые ассоциации, заданные в операционной системе.
    /// </summary>
    SendToInternetBrowser,

    /// <summary>
    /// Не следует использовать
    /// Управляющие элементы используют действующие файловые ассоциации, заданные в операционной системе.
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
    /// Правка-Копировать гиперссылку
    /// </summary>
    CopyHyperlink,

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
  /// панелей инструментов.
  /// Реализация свойства <see cref="EFPApp.CommandItems"/>.
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
      return "EFPAppCommandItems. Count=" + Count.ToString();
    }

    #endregion

    #region Стандартные команды

    /// <summary>
    /// Создать стандартную команду для локального меню.
    /// Если команда в главном меню была добавлена, то создается <see cref="EFPCommandItem"/>,
    /// соединенная с мастер-командой в главном меню (с установленным свойством <see cref="EFPCommandItem.Master"/>). 
    /// Иначе создается независимая команда.
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
    /// <para>
    /// Команда создается, но ни к чему не присоединяется.
    /// Устанавливаются свойства <see cref="EFPCommandItem.Category"/>, <see cref="EFPCommandItem.Name"/>, 
    /// <see cref="EFPCommandItem.MenuText"/>, <see cref="EFPCommandItem.ToolTipText"/>, 
    /// <see cref="EFPCommandItem.ShortCut"/> и <see cref="EFPCommandItem.ImageKey"/>
    /// Обработчик <see cref="EFPCommandItem.Click"/> не устанавливается. Остальные свойства сохраняют значения по умолчанию. Сепараторы не добавляются.
    /// </para>
    /// <para>
    /// Обычно для создания главного меню программы следует использовать объект <see cref="EFPAppCommandItemHelpers"/>,
    /// который реализует необходимые обработчики.
    /// Стандартные команды для локального меню обычно создавать не требуется, так как они уже реализованы. 
    /// В редких случаях можно использовать метод <see cref="CreateContext(EFPAppStdCommandItems)"/>,
    /// который устанавливает связь с командой главного меню (свойство <see cref="EFPCommandItem.Master"/>).
    /// </para>
    /// </summary>
    /// <param name="stdItem">Перечислимое значение для стандартной команды</param>
    /// <returns>Созданная команда без обработчика</returns>
    public static EFPCommandItem CreateStdCommand(EFPAppStdCommandItems stdItem)
    {
      string category, name;
      GetStdCommandCategoryAndName(stdItem, out category, out name);
      EFPCommandItem ci = new EFPCommandItem(category, name);

      switch (stdItem)
      {
        #region Файл

        case EFPAppStdCommandItems.MenuFile:
          ci.MenuText = Res.Cmd_Menu_File;
          break;

        case EFPAppStdCommandItems.New:
          ci.MenuText = Res.Cmd_Menu_File_New;
          ci.ShortCut = Keys.Control | Keys.N;
          ci.ImageKey = "New";
          break;

        case EFPAppStdCommandItems.Open:
          ci.MenuText = Res.Cmd_Menu_File_Open;
          ci.ShortCut = Keys.Control | Keys.O;
          ci.ImageKey = "Open";
          break;

        case EFPAppStdCommandItems.Save:
          ci.MenuText = Res.Cmd_Menu_File_Save;
          ci.ShortCut = Keys.Control | Keys.S;
          ci.ImageKey = "Save";
          break;

        case EFPAppStdCommandItems.SaveAs:
          ci.MenuText = Res.Cmd_Menu_File_SaveAs;
          ci.ShortCut = Keys.Control | Keys.Shift | Keys.S; // 03.10.2025
          break;

        case EFPAppStdCommandItems.PageSetup:
          ci.MenuText = Res.Cmd_Menu_File_PageSetup;
          ci.ImageKey = "PageSetup";
          break;

        case EFPAppStdCommandItems.PrintPreview:
          ci.MenuText = Res.Cmd_Menu_File_PrintPreview;
          ci.ImageKey = "Preview";
          break;

        case EFPAppStdCommandItems.Print:
          ci.MenuText = Res.Cmd_Menu_File_Print;
          ci.ToolTipText = "Печать";
          ci.ShortCut = Keys.Control | Keys.P;
          ci.ImageKey = "Print";
          break;

        case EFPAppStdCommandItems.PrintDefault:
          //      ci.MenuText = "Печать";
          ci.ToolTipText = Res.Cmd_ToolTip_File_PrintDefault;
          ci.ImageKey = "Print";
          break;

        case EFPAppStdCommandItems.Exit:
          ci.MenuText = Res.Cmd_Menu_File_Exit;
          ci.ImageKey = "Exit";
          break;

        #endregion

        #region Файл - Отправить

        case EFPAppStdCommandItems.MenuSendTo:
          ci.MenuText = Res.Cmd_Menu_SendTo;
          break;

        case EFPAppStdCommandItems.SendToInternetBrowser:
          ci.MenuText = Res.Cmd_Menu_SendTo_InternetBrowser;
          ci.ImageKey = "InternetExplorer";
          ci.ToolTipText = Res.Cmd_Menu_ToolTip_InternetBrowser;
          break;
        case EFPAppStdCommandItems.SendToMicrosoftWord:
          ci.MenuText = "Microsoft Word";
          ci.ImageKey = "MicrosoftWord";
          ci.ToolTipText = String.Format(Res.Cmd_ToolTip_SendTo_App, ci.MenuText);
          break;
        case EFPAppStdCommandItems.SendToMicrosoftExcel:
          ci.MenuText = "Microsoft Excel";
          ci.ImageKey = "MicrosoftExcel";
          ci.ToolTipText = String.Format(Res.Cmd_ToolTip_SendTo_App, ci.MenuText);
          break;
        case EFPAppStdCommandItems.SendToOpenOfficeWriter:
          ci.MenuText = EFPApp.OpenOfficeKindName + " Writer";
          ci.ImageKey = "OpenOfficeWriter";
          ci.ToolTipText = String.Format(Res.Cmd_ToolTip_SendTo_App, ci.MenuText);
          break;
        case EFPAppStdCommandItems.SendToOpenOfficeCalc:
          ci.MenuText = EFPApp.OpenOfficeKindName + " Calc";
          ci.ImageKey = "OpenOfficeCalc";
          ci.ToolTipText = String.Format(Res.Cmd_ToolTip_SendTo_App, ci.MenuText);
          break;
        case EFPAppStdCommandItems.SendToNotepad:
          ci.MenuText = Res.Cmd_Menu_SendTo_Notepad;
          ci.ImageKey = "Notepad";
          ci.ToolTipText = Res.Cmd_ToolTip_SendTo_NotePad;
          break;

        #endregion

        #region Правка

        case EFPAppStdCommandItems.MenuEdit:
          ci.MenuText = Res.Cmd_Menu_Edit;
          break;

        case EFPAppStdCommandItems.MenuClipboard:
          ci.MenuText = Res.Cmd_Menu_Clipboard;
          break;

        case EFPAppStdCommandItems.MenuSearch:
          ci.MenuText = Res.Cmd_Menu_Search;
          break;

        case EFPAppStdCommandItems.Undo:
          ci.MenuText = Res.Cmd_Menu_Edit_Undo;
          ci.ImageKey = "Undo";
          ci.ShortCuts.Add(Keys.Control | Keys.Z);
          ci.ShortCuts.Add(Keys.Alt | Keys.Back);
          break;

        case EFPAppStdCommandItems.Redo:
          ci.MenuText = Res.Cmd_Menu_Edit_Redo;
          ci.ImageKey = "Redo";
          ci.ShortCuts.Add(Keys.Control | Keys.Y);
          ci.ShortCuts.Add(Keys.Alt | Keys.Shift | Keys.Back);
          break;

        case EFPAppStdCommandItems.Cut:
          ci.MenuText = Res.Cmd_Menu_Edit_Cut;
          ci.ShortCuts.Add(Keys.Control | Keys.X);
          ci.ShortCuts.Add(Keys.Shift | Keys.Delete);
          //ci.MenuRightText = "Ctrl+X";
          ci.ImageKey = "Cut";
          break;

        case EFPAppStdCommandItems.Copy:
          ci.MenuText = Res.Cmd_Menu_Edit_Copy;
          ci.ShortCuts.Add(Keys.Control | Keys.C);
          ci.ShortCuts.Add(Keys.Control | Keys.Insert);
          //ci.MenuRightText = "Ctrl+С";
          ci.ImageKey = "Copy";
          break;

        case EFPAppStdCommandItems.CopyHyperlink:
          ci.MenuText = Res.Cmd_Menu_Edit_CopyHyperlink;
          ci.ShortCut = Keys.Control | Keys.Shift | Keys.H;
          ci.ImageKey = "CopyHyperlink";
          break;

        case EFPAppStdCommandItems.Paste:
          ci.MenuText = Res.Cmd_Menu_Edit_Paste;
          ci.ShortCuts.Add(Keys.Control | Keys.V);
          ci.ShortCuts.Add(Keys.Shift | Keys.Insert);
          //ci.MenuRightText = "Ctrl+V";
          ci.ImageKey = "Paste";
          break;

        case EFPAppStdCommandItems.PasteSpecial:
          ci.MenuText = Res.Cmd_Menu_Edit_PasteSpecial;
          ci.ShortCut = Keys.Control | Keys.Shift | Keys.V;
          break;

        case EFPAppStdCommandItems.Find:
          ci.MenuText = Res.Cmd_Menu_Edit_Find;
          ci.ShortCut = Keys.Control | Keys.F;
          ci.ImageKey = "Find";
          break;

        case EFPAppStdCommandItems.IncSearch:
          ci.MenuText = Res.Cmd_Menu_Edit_IncSearch;
          ci.ShortCut = Keys.Control | Keys.K;
          ci.ImageKey = "IncSearch";
          break;

        case EFPAppStdCommandItems.FindNext:
          ci.MenuText = Res.Cmd_Menu_Edit_FindNext;
          ci.ShortCut = Keys.F3;
          ci.ImageKey = "FindNext";
          break;

        case EFPAppStdCommandItems.Replace:
          ci.MenuText = Res.Cmd_Menu_Edit_Replace;
          ci.ShortCut = Keys.Control | Keys.H;
          ci.ImageKey = "Replace";
          break;

        case EFPAppStdCommandItems.Goto:
          ci.MenuText = Res.Cmd_Menu_Edit_Goto;
          ci.ShortCut = Keys.Control | Keys.G;
          ci.ImageKey = "Goto";
          break;

        case EFPAppStdCommandItems.SelectAll:
          ci.MenuText = Res.Cmd_Menu_Edit_SelectAll;
          ci.ShortCut = Keys.Control | Keys.A;
          break;

        #endregion

        #region Вид

        case EFPAppStdCommandItems.MenuView:
          ci.MenuText = Res.Cmd_Menu_View;
          break;

        case EFPAppStdCommandItems.Refresh:
          ci.MenuText = Res.Cmd_Menu_View_Refresh;
          ci.ShortCut = Keys.F5;
          ci.ImageKey = "Refresh";
          break;

        case EFPAppStdCommandItems.FullScreen:
          ci.MenuText = Res.Cmd_Menu_View_FullScreen;
          ci.ImageKey = "FullScreen";

          // плохо работает
          // срабатывает дважды при выходе из полноэкранного режима
          // ci.ShortCut = Keys.Alt | Keys.Enter;
          break;

        #endregion

        #region Окно

        case EFPAppStdCommandItems.MenuWindow:
          ci.MenuText = Res.Cmd_Menu_Window;
          break;

        case EFPAppStdCommandItems.TileHorizontal:
          ci.MenuText = Res.Cmd_Menu_Window_TileHorizotal;
          ci.ImageKey = "TileHorizontal";
          break;

        case EFPAppStdCommandItems.TileVertical:
          ci.MenuText = Res.Cmd_Menu_Window_TileVertical;
          ci.ImageKey = "TileVertical";
          break;

        case EFPAppStdCommandItems.Cascade:
          ci.MenuText = Res.Cmd_Menu_Window_Cascade;
          ci.ImageKey = "Cascade";
          break;

        case EFPAppStdCommandItems.ArrangeIcons:
          ci.MenuText = Res.Cmd_Menu_Window_ArrangeIcons;
          ci.ImageKey = "ArrangeIcons";
          break;

        case EFPAppStdCommandItems.CloseAll:
          ci.MenuText = Res.Cmd_Menu_Window_CloseAll;
          ci.ImageKey = "CloseAll";
          break;

        case EFPAppStdCommandItems.CloseAllButThis:
          ci.MenuText = Res.Cmd_Menu_Window_CloseAllButThis;
          ci.ImageKey = "CloseAllButThis";
          break;

        case EFPAppStdCommandItems.OtherWindows:
          ci.MenuText = Res.Cmd_Menu_Window_OtherWindows;
          ci.ImageKey = "WindowList";
          break;

        case EFPAppStdCommandItems.NewMainWindow:
          ci.MenuText = Res.Cmd_Menu_Window_NewMainWindow;
          ci.ImageKey = "NewMainWindow";
          break;

        case EFPAppStdCommandItems.SavedCompositions:
          ci.MenuText = Res.Cmd_Menu_Window_SavedCompositions;
          ci.ImageKey = "SavedCompositions";
          break;

        #endregion

        #region Справка

        case EFPAppStdCommandItems.MenuHelp:
          ci.MenuText = Res.Cmd_Menu_Help;
          break;

        case EFPAppStdCommandItems.HelpContents:
          ci.MenuText = Res.Cmd_Menu_Help_Contents;
          ci.ShortCut = Keys.Control | Keys.F1;
          ci.ImageKey = "HelpContents";
          ci.ToolTipText = Res.Cmd_ToolTip_Help_Contents;
          break;

        case EFPAppStdCommandItems.HelpIndex:
          ci.MenuText = Res.Cmd_Menu_Help_Index;
          ci.ShortCut = Keys.Shift | Keys.F1;
          ci.ImageKey = "HelpIndex";
          ci.ToolTipText = Res.Cmd_ToolTip_Help_Index;
          break;

        case EFPAppStdCommandItems.ContextHelp:
          ci.MenuText = Res.Cmd_Menu_Help_ContextHelp;
          ci.ShortCut = Keys.F1;
          ci.ImageKey = "Help";
          break;

        case EFPAppStdCommandItems.About:
          ci.MenuText = Res.Cmd_Menu_Help_About;
          ci.ImageKey = "About";
          break;

        #endregion

        default:
          throw ExceptionFactory.ArgUnknownValue("stdItem", stdItem, null);
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
        EFPApp.ShowException(e);
      }
    }

    #endregion

    #endregion

    #region Коды для стандартных команд

    /// <summary>
    /// Возвращает для стандартной команды категорию и имя.
    /// Этот метод не зависит от наличия реальных команд в меню. Объекты <see cref="EFPCommandItem"/> не создаются.
    /// Для выполнения обратного преобразования используйте метод <see cref="FindStdCommand(string, string, out EFPAppStdCommandItems)"/>.
    /// </summary>
    /// <param name="stdItem">Стандартная команда</param>
    /// <param name="category">Сюда записывается категория команды <see cref="EFPCommandItem.Category"/></param>
    /// <param name="name">Сюда записывается имя команды <see cref="EFPCommandItem.Name"/></param>
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
        throw ExceptionFactory.ArgUnknownValue("stdItem", stdItem, null);
    }

    /// <summary>
    /// Найти значение перечисления <see cref="EFPAppStdCommandItems"/> для стандартной команды по категории и имени.
    /// Метод является обратным по отношению к <see cref="GetStdCommandCategoryAndName(EFPAppStdCommandItems, out string, out string)"/>.
    /// Метод не зависит от наличия команды в главном меню и от существования объекта <see cref="EFPCommandItem"/>.
    /// Если имеется объект <see cref="EFPCommandItem"/>, удобнее использовать метод <see cref="IsStdCommandItem(EFPCommandItem, out EFPAppStdCommandItems)"/>.
    /// </summary>
    /// <param name="category">Категория команды <see cref="EFPCommandItem.Category"/></param>
    /// <param name="name">Имя команды <see cref="EFPCommandItem.Name"/></param>
    /// <param name="stdItem">Сюда записывается перечислимое значение, если команда найдена</param>
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
    /// Найти значение перечисления <see cref="EFPAppStdCommandItems"/> для стандартной команды по категории и имени.
    /// Если переданные аргументы не соответствуют стандартной команде, то генерируется исключение.
    /// Метод не зависит от наличия команды в главном меню и от существования объекта <see cref="EFPCommandItem"/>.
    /// Если выброс исключения является нежелательным, используйте метод <see cref="FindStdCommand(string, string, out EFPAppStdCommandItems)"/>.
    /// </summary>
    /// <param name="category">Категория команды</param>
    /// <param name="name">Имя команды</param>
    /// <returns>Найденное перечислимое значение</returns>
    public static EFPAppStdCommandItems GetStdCommand(string category, string name)
    {
      EFPAppStdCommandItems res;
      if (FindStdCommand(category, name, out res))
        return res;
      else
        throw new ArgumentException(String.Format(Res.EFPApp_Arg_UnknownStdCommandCategoryAndName, category, name));
    }

    private static BidirectionalDictionary<EFPAppStdCommandItems, string> _StdCommandDict = InitCommandDict();

    private static BidirectionalDictionary<EFPAppStdCommandItems, string> InitCommandDict()
    {
      string[] a = new string[]{
        "|File",
        "|SendTo",
        "|Edit",
        "|Clipboard",
        "|Search",
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
        "Edit|CopyHyperlink",
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

      MinMax<int> range = DataTools.GetEnumRange(typeof(EFPAppStdCommandItems));

#if DEBUG
      if (range.MaxValue != (a.Length - 1))
        throw new BugException("Bad list");
#endif

      BidirectionalDictionary<EFPAppStdCommandItems, string> dict = new BidirectionalDictionary<EFPAppStdCommandItems, string>(a.Length);
      for (int i = 0; i < a.Length; i++)
      {
        dict.Add((EFPAppStdCommandItems)i, a[i]);
      }

      return dict;
    }

    /// <summary>
    /// Возвращает true, если данная команда является стандартной.
    /// Метод не зависит от наличия команды в главном меню.
    /// Если нет объекта <see cref="EFPCommandItem"/>, используйте <see cref="FindStdCommand(string, string, out EFPAppStdCommandItems)"/>.
    /// </summary>
    /// <param name="commandItem">Проверяемая команда. Может быть null.</param>
    /// <param name="stdItem">Сюда записывается код стандартной команды <see cref="EFPAppStdCommandItems"/></param>
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
    /// Метод не зависит от наличия команды в главном меню.
    /// Вряд ли эта перегрузка метода является полезной.
    /// </summary>
    /// <param name="commandItem">Проверяемая команда</param>
    /// <returns>true, если команда является стандартной</returns>
    public static bool IsStdCommandItem(EFPCommandItem commandItem)
    {
      EFPAppStdCommandItems stdItem;
      return IsStdCommandItem(commandItem, out stdItem);
    }

    #endregion

    #region Глобальные сочетания клавиш

    /// <summary>
    /// Сюда можно добавить команды, содержащие сочетания клавиш, которые будут
    /// обработаны в любом окне программы, включая модальные.
    /// Например, можно добавить сочетание клавиш для вызова окна калькулятора.
    /// </summary>
    public List<EFPCommandItem> GlobalShortCuts { get { return _GlobalShortCuts; } }
    private List<EFPCommandItem> _GlobalShortCuts;

    #endregion
  }

  /// <summary>
  /// Задает список кнопок для одной панели инструментов в главном окне программы.
  /// Команды <see cref="EFPCommandItem"/> должны быть добавлены в главное меню программы <see cref="EFPAppCommandItems"/>.
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
        throw ExceptionFactory.ArgStringIsNullOrEmpty("name");
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
    private readonly string _Name;

    /// <summary>
    /// Отображаемое имя панели для меню "Вид" - "Панели инструментов".
    /// Если свойство не установлено в явном виде, используется значение свойства <see cref="Name"/>.
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
    /// По умолчанию - true.
    /// </summary>
    public bool DefaultVisible { get { return _DefaultVisible; } set { _DefaultVisible = value; } }
    private bool _DefaultVisible;

    /// <summary>
    /// С какой стороны (по умолчанию) располагается панель.
    /// По умолчанию - <see cref="DockStyle.Top"/>.
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
            throw ExceptionFactory.ArgUnknownValue("value", value);
        }
      }
    }
    private DockStyle _DefaultDock;

    /// <summary>
    /// Возвращает <see cref="DisplayName"/>
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
  /// Список объектов для свойства <see cref="EFPApp.ToolBars"/>.
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
