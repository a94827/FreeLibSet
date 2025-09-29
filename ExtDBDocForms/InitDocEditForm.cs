// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Drawing;
using System.Windows.Forms;
using FreeLibSet.Forms;
using System.Collections.Generic;
using FreeLibSet.DependedValues;
using FreeLibSet.Data.Docs;
using FreeLibSet.Core;
using FreeLibSet.Controls;
using FreeLibSet.Data;
using FreeLibSet.UICore;
using FreeLibSet.Forms.Data;

namespace FreeLibSet.Forms.Docs
{
  #region Абстрактный базовый класс инициализации формы

  /// <summary>
  /// Базовый класс для <see cref="InitDocEditFormEventArgs"/> и <see cref="InitSubDocEditFormEventArgs"/>
  /// </summary>
  public abstract class InitDocEditFormEventArgsBase : DBxExtValuesDialogInitEventArgs, IReadOnlyObject
  {
    #region Конструктор

    internal InitDocEditFormEventArgsBase(ExtEditDialog dialog, IDBxExtValues values)
      :base(dialog, values)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Значок, который по умолчанию задается для первой добавляемой вкладки (<see cref="ExtEditPage.ImageKey"/>).
    /// Возвращает значение <see cref="DocTypeUIBase.SingleDocImageKey"/>.
    /// </summary>
    public abstract string MainImageKey { get; }

    #endregion

    #region AddPage()

    /// <summary>
    /// Добавить вкладку в редактор документа или поддокумента.
    /// После вызова метода можно создавать провайдеры управляющих элементов, используя свойство
    /// <see cref="ExtEditPage.BaseProvider"/>.
    /// </summary>
    /// <param name="title">Заголовок вкладки</param>
    /// <param name="mainControl">Управляющий элемент (обычно, панель), на котором располагаются другие элементы</param>
    /// <returns>Объект для управления вкладкой</returns>
    public ExtEditPage AddPage(string title, Control mainControl)
    {
      if (mainControl == null)
        throw new ArgumentNullException("mainControl");

      AddFormToDispose(mainControl.FindForm());

      if (String.IsNullOrEmpty(title))
        title = (Dialog.Pages.Count + 1).ToString();

      ExtEditPage page=Dialog.Pages.Add( mainControl);
      page.Title = title;

      // 07.01.2022. Инициализируем значок по умолчанию для страницы
      if (Dialog.Pages.Count == 1)
        page.ImageKey = MainImageKey;
      else
        page.ImageKey = "Properties";

      //Size sz = new Size();
      //sz.Width = MainControl.DisplayRectangle.Width;
      //sz.Height = MainControl.DisplayRectangle.Height;
      Size sz = mainControl.Size; // 21.10.2019. Почему-то раньше было DisplayRectangle

      //TabPage.Control.BackColor = Color.Transparent;
      //TabPage.Control.UseVisualStyleBackColor = true;
      if (mainControl is Panel)
        mainControl.BackColor = Color.Transparent;

      mainControl.Visible = false; // 31.07.2012 прячем до вызова FirstShow


      mainControl.Dock = DockStyle.None; // управляется из Resize

      return page;
    }

    /// <summary>
    /// Добавить в редактор вкладку, содержащую единственный управляющий элемент и панель инструментов.
    /// Метод создает вкладку, управляющий элемент и панель инструментов.
    /// </summary>
    /// <typeparam name="T">Класс управляющего элемента, производного от <see cref="Control"/>. У элемента должен
    /// быть конструктор по умолчанию.</typeparam>
    /// <param name="title">Заголовок вкладки</param>
    /// <param name="controlWithToolBar">Сюда помещается объект с элементом управления и панелью инструментов</param>
    /// <returns>Объект для управления вкладкой</returns>
    public ExtEditPage AddSimplePage<T>(string title, out EFPControlWithToolBar<T> controlWithToolBar)
      where T : Control, new()
    {
      Panel thePanel = new Panel();
      ExtEditPage editorPage = AddPage(title, thePanel);
      controlWithToolBar = new EFPControlWithToolBar<T>(editorPage.BaseProvider, editorPage.MainControl);
      return editorPage;
    }

    #endregion

    #region Методы привязки полей

    #region Ссылочные поля

    /// <summary>
    /// Добавляет связку для редактирования ссылочного поля на документ с помощью комбоблока.
    /// Значение поля связывается со свойством <see cref="EFPDocComboBox.DocId"/>.
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов или поддокументов.
    /// В этом случае на форму может быть добавлен <see cref="CheckBox"/> (обычно взамен метки поля) для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс <see cref="IUIExtEditItem"/></returns>
    public ExtValueDocComboBox AddRef(EFPDocComboBox controlProvider, string columnName, bool canMultiEdit)
    {
      ExtValueDocComboBox dvc = new ExtValueDocComboBox(GetExtValue(columnName), controlProvider, canMultiEdit);
      AddEditItem(dvc);
      return dvc;
    }

    /// <summary>
    /// Добавляет связку для редактирования ссылочного поля на поддокумент с помощью комбоблока.
    /// Значение поля связывается со свойством <see cref="EFPSubDocComboBox.SubDocId"/>.
    /// Поддокумент не должен относиться к документу, открытому в текущем редакторе. См. перегрузку <see cref="AddRef(EFPInsideSubDocComboBox, string, bool)"/> .
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов или поддокументов.
    /// В этом случае на форму может быть добавлен <see cref="CheckBox"/> (обычно взамен метки поля) для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс <see cref="IUIExtEditItem"/></returns>
    public ExtValueSubDocComboBox AddRef(EFPSubDocComboBox controlProvider, string columnName, bool canMultiEdit)
    {
      ExtValueSubDocComboBox dvc = new ExtValueSubDocComboBox(GetExtValue(columnName), controlProvider, canMultiEdit);
      AddEditItem(dvc);
      return dvc;
    }

    /// <summary>
    /// Добавляет связку для редактирования ссылочного поля на поддокументы с помощью комбоблока.
    /// Значение поля связывается со свойством <see cref="EFPInsideSubDocComboBox.SubDocId"/>.
    /// Предполагается, что поддокумент относится к тому документу, который в настоящее время
    /// открыт в редакторе документа.
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов или поддокументов.
    /// В этом случае на форму может быть добавлен <see cref="CheckBox"/> (обычно взамен метки поля) для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс <see cref="IUIExtEditItem"/></returns>
    public ExtValueInsideSubDocComboBox AddRef(EFPInsideSubDocComboBox controlProvider, string columnName, bool canMultiEdit)
    {
      ExtValueInsideSubDocComboBox dvc = new ExtValueInsideSubDocComboBox(GetExtValue(columnName), controlProvider, canMultiEdit);
      AddEditItem(dvc);
      return dvc;
    }

    #endregion

    #region Ссылки на таблицу DocType

    /// <summary>
    /// Добавляет связку для редактирования ссылочного поля на таблицу документов.
    /// Числовое поле содержит идентификатор таблицы документов <see cref="DBxDocType.TableId"/>.
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов или поддокументов.
    /// В этом случае на форму может быть добавлен <see cref="CheckBox"/> (обычно взамен метки поля) для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс <see cref="IUIExtEditItem"/></returns>
    public ExtValueDocTypeComboBoxByTableId AddTableId(EFPDocTypeComboBox controlProvider, string columnName, bool canMultiEdit)
    {
      ExtValueDocTypeComboBoxByTableId dvc = new ExtValueDocTypeComboBoxByTableId(GetExtValue(columnName), controlProvider, canMultiEdit);
      AddEditItem(dvc);
      return dvc;
    }
#if XXX
    public DocValueCheckBoxWithDocTypeComboBoxByTableId AddTableId(EFPCheckBox ControlProvider1, EFPDocTypeComboBox ControlProvider2, string ColumnName, bool CanMultiEdit)
    {
      DocValueCheckBoxWithDocTypeComboBoxByTableId dvc = new DocValueCheckBoxWithDocTypeComboBoxByTableId(GetExtValue(ColumnName), ControlProvider1, ControlProvider2, CanMultiEdit);
      AddDocEditItem(dvc);
      return dvc;
    }
    public DocValueDocComboBoxByTableId AddTableId(EFPDocComboBox ControlProvider, string ColumnName, bool CanMultiEdit)
    {
      DocValueDocComboBoxByTableId dvc = new DocValueDocComboBoxByTableId(GetExtValue(ColumnName), ControlProvider, CanMultiEdit);
      AddDocEditItem(dvc);
      return dvc;
    }
#endif

    /// <summary>
    /// Добавляет связку для редактирования ссылочного поля на таблицу документов.
    /// Строкое поле содержит имя таблицы документов <see cref="DBxDocTypeBase.Name"/>.
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов или поддокументов.
    /// В этом случае на форму может быть добавлен <see cref="CheckBox"/> (обычно взамен метки поля) для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс <see cref="IUIExtEditItem"/></returns>
    public ExtValueDocTypeComboBoxByName AddTableName(EFPDocTypeComboBox controlProvider, string columnName, bool canMultiEdit)
    {
      ExtValueDocTypeComboBoxByName dvc = new ExtValueDocTypeComboBoxByName(GetExtValue(columnName), controlProvider, canMultiEdit);
      AddEditItem(dvc);
      return dvc;
    }

    #endregion

    #endregion

    #region Внутренняя реализация

    /// <summary>
    /// Добавить форму для последующего разрушения.
    /// Метод должен вызываться до того, как управляющий элемент переприсоединяется
    /// к форме редактора.
    /// </summary>
    /// <param name="form"></param>
    public void AddFormToDispose(Form form)
    {
      if (form == null)
        return;
      if (Dialog.DisposeFormList.IndexOf(form) < 0)
        Dialog.DisposeFormList.Add(form);
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Свойство возвращает true, если редактор документа или поддокумента
    /// находится в режиме просмотра данных.
    /// </summary>
    public abstract bool IsReadOnly { get; }

    /// <summary>
    /// Генерирует <see cref="ObjectReadOnlyException"/>, если <see cref="IsReadOnly"/>=true.
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw new ObjectReadOnlyException(Res.Editor_Err_IsReadOnly);
    }

    #endregion
  }

  #endregion

  #region Инициализация основной формы документа

  /// <summary>
  /// Аргументы события <see cref="FreeLibSet.Forms.Docs.DocTypeUI.InitEditForm"/>
  /// </summary>
  public class InitDocEditFormEventArgs : InitDocEditFormEventArgsBase
  {
    #region Конструктор

    /// <summary>
    /// Создается в <see cref="DocTypeUI"/>
    /// </summary>
    /// <param name="editor">Открываемый редактор документов</param>
    /// <param name="multiDocs">Редактируемые документы</param>
    public InitDocEditFormEventArgs(DocumentEditor editor, DBxMultiDocs multiDocs)
      : base(editor.Dialog, multiDocs.Values)
    {
      _Editor = editor;
      _MultiDocs = multiDocs;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Открываемый редактор документов
    /// </summary>
    public DocumentEditor Editor { get { return _Editor; } }
    private readonly DocumentEditor _Editor;

    /// <summary>                              
    /// Редактируемые документы, для которых открыт редактор.
    /// Для доступа к документов других видов используйте объект <see cref="DBxDocSet"/>, который доступен через свойство <see cref="Editor"/>.Documents.
    /// </summary>
    public DBxMultiDocs MultiDocs { get { return _MultiDocs; } }
    private readonly DBxMultiDocs _MultiDocs;

    private DocTypeUI DocTypeUI { get { return _Editor.UI.DocTypes[_MultiDocs.DocType.Name]; } }

    /// <summary>
    /// Коллекция для добавления объектов синхронизации
    /// </summary>
    public DepSyncCollection Syncs
    {
      get
      {
        return Dialog.Syncs;
      }
    }

    /// <summary>
    /// Возвращает свойство <see cref="DocumentEditor.IsReadOnly"/>
    /// </summary>
    public override bool IsReadOnly { get { return Editor.IsReadOnly; } }

    /// <summary>
    /// Возвращает свойство <see cref="DocTypeUIBase.SingleDocImageKey"/>
    /// </summary>
    public override string MainImageKey
    {
      get { return DocTypeUI.SingleDocImageKey; }
    }

    #endregion

    #region Методы добавления страницы

    #region С поддокументами

    /// <summary>
    /// Добавляет вкладку с таблицей поддокументов.
    /// Эта перегрузка метода не дает доступа к созданному провайдеру просмотра <see cref="EFPSubDocGridView"/>,
    /// поэтому может использоваться только в простейших случаях.
    /// </summary>
    /// <param name="subDocTableName">Имя таблицы поддокументов</param>
    /// <returns>Объект для управления вкладкой</returns>
    public ExtEditPage AddSubDocsPage(string subDocTableName)
    {
      // TODO: Переделать с использованием отложенной инициализации
      EFPSubDocGridView ControlProvider;
      return AddSubDocsPage(subDocTableName, out ControlProvider);
    }

    /// <summary>
    /// Добавляет вкладку с таблицей поддокументов.
    /// </summary>
    /// <param name="subDocTableName">Имя таблицы поддокументов</param>
    /// <param name="controlProvider">Сюда записывается ссылка на созданный провайдер просмотра <see cref="EFPSubDocGridView"/></param>
    /// <returns>Объект для управления вкладкой</returns>
    public ExtEditPage AddSubDocsPage(string subDocTableName, out EFPSubDocGridView controlProvider)
    {
      Panel panel = new Panel();

      SubDocTypeUI sdtUI = DocTypeUI.SubDocTypes[subDocTableName];
      ExtEditPage page = AddPage(sdtUI.SubDocType.PluralTitle, panel);
      page.ImageKey = sdtUI.TableImageKey;
      EFPControlWithToolBar<DataGridView> cwt = new EFPControlWithToolBar<DataGridView>(page.BaseProvider, panel);

      controlProvider = new EFPSubDocGridView(cwt, _Editor, _MultiDocs.SubDocs[subDocTableName]);
      return page;
    }

    /// <summary>
    /// Добавляет вкладку с иерархическим просмотром поддокументов.
    /// Эта перегрузка метода не дает доступа к созданному провайдеру просмотра <see cref="EFPSubDocTreeView"/>,
    /// поэтому может использоваться только в простейших случаях.
    /// </summary>
    /// <param name="subDocTableName">Имя таблицы поддокументов</param>
    /// <returns>Объект для управления вкладкой</returns>
    public ExtEditPage AddSubDocsTreePage(string subDocTableName)
    {
      // TODO: Переделать с использованием отложенной инициализации
      EFPSubDocTreeView controlProvider;
      return AddSubDocsTreePage(subDocTableName, out controlProvider);
    }

    /// <summary>
    /// Добавляет вкладку с иерархическим просмотром поддокументов.
    /// </summary>
    /// <param name="subDocTableName">Имя таблицы поддокументов</param>
    /// <param name="controlProvider">Сюда записывается ссылка на созданный провайдер просмотра <see cref="EFPSubDocTreeView"/></param>
    /// <returns>Объект для управления вкладкой</returns>
    public ExtEditPage AddSubDocsTreePage(string subDocTableName, out EFPSubDocTreeView controlProvider)
    {
      Panel panel = new Panel();

      SubDocTypeUI sdtUI = DocTypeUI.SubDocTypes[subDocTableName];
      ExtEditPage page = AddPage(sdtUI.SubDocType.PluralTitle, panel);
      EFPControlWithToolBar<TreeViewAdv> cwt = new EFPControlWithToolBar<TreeViewAdv>(page.BaseProvider, panel);

      controlProvider = new EFPSubDocTreeView(cwt, _Editor, _MultiDocs.SubDocs[subDocTableName]);
      return page;
    }


#if XXXX
    #region Вложенный класс SubDocsPageInfo

    private class SubDocsPageInfo
    {
      public Panel MainPanel;
      public ClientSubDocType SubDocType;
      public SubDocsGrid Grid; // создается в FirstShow
      public string ManualOrderColumn;
      public bool ValidateBeforeEdit;

      public void FirstShow(object Sender, DocEditPageEventArgs Args)
      {
        if (AccDepClientExec.Permissions.Tables[SubDocType.Name] == AccDepAccessMode.None)
        {
          Args.Page.SetPanelMessage("У Вас нет права доступа к поддокументам \"" + SubDocType.PluralTitle + "\"", MainPanel);
          return;
        }

        if (Args.Editor.MultiDocMode && (!String.IsNullOrEmpty(ManualOrderColumn)))
        {
          Args.Page.SetPanelMessage("Таблица недоступна если одновременно редактируется несколько документов", MainPanel);
          return;
        }

        Grid = new SubDocsGrid(Args.Page.BaseProvider, Args.Editor, Args.MultiDocs, SubDocType.Name);
        Grid.ManualOrderColumn = ManualOrderColumn;
        Grid.ValidateBeforeEdit = ValidateBeforeEdit;
        Grid.CreateControl(MainPanel);
      }
    }

    #endregion

#endif

#if XXX
    /// <summary>
    /// Добавить страницу с таблицей поддокументов
    /// Таблица будет создана при первом переключении на закладку. Объект SubDocsGrid недоступен
    /// </summary>
    /// <param name="Title">Заголовок вкладки</param>
    /// <param name="SubDocTableName">Вид поддокументов</param>
    /// <param name="ImageKey">Значок закладки</param>
    /// <param name="ToolTipText">Всплывающая подсказка</param>
    /// <returns>Объект страницы</returns>
    public DocEditPage AddSubDocsPage(string Title, string SubDocTableName, string ImageKey, string ToolTipText)
    {
      return AddSubDocsPage(Title, SubDocTableName, ImageKey, ToolTipText, String.Empty, false);
    }

    /// <summary>
    /// Добавить страницу с таблицей поддокументов
    /// Таблица будет создана при первом переключении на закладку. Объект SubDocsGrid недоступен
    /// </summary>
    /// <param name="Title">Заголовок вкладки</param>
    /// <param name="SubDocTableName">Вид поддокументов</param>
    /// <param name="ImageKey">Значок закладки</param>
    /// <param name="ToolTipText">Всплывающая подсказка</param>
    /// <param name="ManualOrderColumn">Имя столбца ручной сортировки. null - нет</param>
    /// <returns>Объект страницы</returns>
    public DocEditPage AddSubDocsPage(string Title, string SubDocTableName, string ImageKey, string ToolTipText, string ManualOrderColumn)
    {
      return AddSubDocsPage(Title, SubDocTableName, ImageKey, ToolTipText, ManualOrderColumn, false);
    }

    /// <summary>
    /// Добавить страницу с таблицей поддокументов
    /// Таблица будет создана при первом переключении на закладку. Объект SubDocsGrid недоступен
    /// </summary>
    /// <param name="Title">Заголовок вкладки</param>
    /// <param name="SubDocTableName">Вид поддокументов</param>
    /// <param name="ImageKey">Значок закладки</param>
    /// <param name="ToolTipText">Всплывающая подсказка</param>
    /// <param name="ManualOrderColumn">Имя столбца ручной сортировки. null - нет</param>
    /// <param name="ValidateBeforeEdit">Если установлено в true, то перед добавлением или редактированием записи,
    /// будет вызываться метод DocumentEditor.ValidateData(), чтобы поля основного документа содержали
    /// актуальные значения. Необходимо, если редактор поддокумента обращается к
    /// полям основного документа</param>
    /// <returns>Объект страницы</returns>
    public DocEditPage AddSubDocsPage(string Title, string SubDocTableName, string ImageKey, string ToolTipText, string ManualOrderColumn, bool ValidateBeforeEdit)
    {
      if (SubDocTableName == null)
        throw new ArgumentNullException("SubDocTableName");


      SubDocsPageInfo sdpi = new SubDocsPageInfo();
      sdpi.SubDocType = (ClientSubDocType)(MultiDocs.DocType.SubDocs[SubDocTableName]);
      if (sdpi.SubDocType == null)
        throw new BugException("Поддокумент типа \"" + SubDocTableName + "\" не объявлен");

      sdpi.MainPanel = new Panel();
      sdpi.MainPanel.Size = new Size(300, 200);
      sdpi.ManualOrderColumn = ManualOrderColumn;
      sdpi.ValidateBeforeEdit = ValidateBeforeEdit;

      DocEditPage dep = AddPage(Title, sdpi.MainPanel, ImageKey, ToolTipText);
      dep.FirstShow += new DocEditPageEventHandler(sdpi.FirstShow);
      return dep;

    }

    /// <summary>
    /// Добавить страницу с таблицей поддокументов
    /// Таблица будет создана при первом переключении на закладку. Объект SubDocsGrid недоступен
    /// Заголовок вкладки и значок задаются автоматически
    /// </summary>
    /// <param name="SubDocTableName">Имя поддокументов</param>
    /// <returns>Объект страницы</returns>
    public DocEditPage AddSubDocsPage(string SubDocTableName)
    {
      ClientSubDocType SubDocType = (ClientSubDocType)(MultiDocs.DocType.SubDocs[SubDocTableName]);

      return AddSubDocsPage(SubDocType.PluralTitle, SubDocTableName, SubDocType.ImageKey, null, null);
    }
#endif

    #endregion
#if XXX
    #region С произвольным табличным просмотром

    /// <summary>
    /// Добавление в редактор закладки, содержащей табличный просмотр
    /// По ссылке возвращаются обработчик табличного просмотра и панель для кнопок.
    /// После добавления команд локального меню следует вызвать DocGridHandler.SetCommandItems(PanSpb)
    /// </summary>
    /// <param name="Title">Заголовок закладки</param>
    /// <param name="DocGridHandler">Сюда помещается обработчик созданного табличного просмотра</param>
    /// <param name="PanSpb">Сюда помещается ссылка на панель кнопок</param>
    /// <param name="ImageKey">Изображение для закладки</param>
    /// <param name="ToolTipText">Подсказка</param>
    /// <returns>Страница редактора</returns>
    public DocEditPage AddGridPage(string Title, out EFPAccDepGrid ControlProvider, out Panel PanSpb, string ImageKey, string ToolTipText)
    {
      Panel MainPanel = new Panel();
      MainPanel.Dock = DockStyle.Fill;
      DataGridView Grid = new DataGridView();
      Grid.Dock = DockStyle.Fill;
      MainPanel.Controls.Add(Grid);
      PanSpb = new Panel();
      PanSpb.Dock = DockStyle.Top;
      MainPanel.Controls.Add(PanSpb);
      ControlProvider = new EFPAccDepGrid(BaseProvider, Grid);
      return AddPage(Title, MainPanel, ImageKey, ToolTipText);
    }

    public DocEditPage AddGridPage(string Title, out EFPAccDepGrid ControlProvider, out Panel PanSpb, string ImageKey)
    {
      return AddGridPage(Title, out ControlProvider, out PanSpb, ImageKey, null);
    }

    #endregion

    #region С отчетом

    /// <summary>
    /// Добавить страницу со встроенным отчетом.
    /// Отчет перестраивается при каждом переходе на страницу. Перед этим вызывается ValidateData()
    /// Заголовок страницы берется из параметров отчета, а значок - из MainImageKey
    /// </summary>
    /// <param name="Report">Отчет</param>
    /// <param name="ToolTipText">Текст всплывающей подсказки для страницы</param>
    /// <returns>Страница редактора документа</returns>
    public DocEditPage AddPageWithGridReport(GridReport Report, string ToolTipText)
    {
      // !!! Надо бы обойтись без доп. панели и встривать отчет непосредственно в EFPTabPage

#if DEBUG
      if (Report == null)
        throw new ArgumentNullException("Report");
#endif

      GridReportHandler Handler = new GridReportHandler();
      Handler.Report = Report;
      Panel MainPanel = new Panel();
      Handler.Page = AddPage(Report.ReportParams.Title, MainPanel, Report.MainImageKey, ToolTipText);
      Handler.PanelHandler = new EFPPanel(Handler.Page.BaseProvider, MainPanel);
      Handler.Page.FirstShow += new DocEditPageEventHandler(Handler.Page_FirstShow);
      Handler.Page.PageShow += new DocEditPageEventHandler(Handler.Page_PageShow);

      return Handler.Page;
    }

    private class EFPPanel : EFPControl<Panel>
    {
    #region Конструктор

      public EFPPanel(EFPBaseProvider BaseProvider, Panel Control)
        : base(BaseProvider, Control, false)
      { 
      }

    #endregion
    }

    private class GridReportHandler
    {
    #region Поля

      public GridReport Report;
      public DocEditPage Page;
      public EFPPanel PanelHandler;

      private bool FirstFlag;

    #endregion

    #region Обработчики

      public void Page_FirstShow(object Sender, DocEditPageEventArgs Args)
      {
        FirstFlag = true;
      }

      public void Page_PageShow(object Sender, DocEditPageEventArgs Args)
      {
        if (!Page.Editor.ValidateData())
          return;


        if (FirstFlag)
        {
          Report.RunBuiltIn(PanelHandler, Page.BaseProvider);
          FirstFlag = false;
        }
        else
          Report.RefreshReport();
      }

    #endregion
    }

    #endregion

#endif

    #endregion

    #region Методы привязки полей

#if !XXX
    private class DocComboBoxParentIdValidator
    {
      #region Поля

      /// <summary>
      /// Используется для извлечения идентификаторов документов
      /// </summary>
      public DBxMultiDocs Docs;

      public EFPDocComboBox ControlProvider;

      public string ParentColumnName;

      #endregion

      #region Метод проверки

      public void Validate(object sender, FreeLibSet.UICore.UIValidatingEventArgs args)
      {
        if (args.ValidateState == FreeLibSet.UICore.UIValidateState.Error)
          return;

        IIdSet<Int32> chain = ControlProvider.DocTypeUI.TableCache.GetTreeChainIds(ControlProvider.DocId, ParentColumnName);
        if (chain.Count == 0) // либо DocId=0, либо дерево уже зациклено.
          return;

        IdCollection<Int32> chain2 = new IdCollection<Int32>(chain); // чтобы быстрее работало

        IIdSet<Int32> currIds = Docs.DocIds;
        foreach (Int32 currId in currIds)
        {
          if (currId <= 0)
            continue;

          if (chain2.Contains(currId))
          {
            args.SetError(String.Format(Res.Editor_Err_DocTreeLoop, ControlProvider.DocTypeUI.GetTextValue(currId)));
            return;
          }
        }
      }

      #endregion
    }

    /// <summary>
    /// Добавляет связку для редактирования ссылочного поля, которое используется для построения дерева, с помощью комбоблока.
    /// Вызывает основной метод <see cref="InitDocEditFormEventArgsBase.AddRef(EFPDocComboBox, string, bool)"/> 
    /// и добавляет к <paramref name="controlProvider"/> валидатор, который не позволяет
    /// задавать ссылку, которая приведет к зацикливанию дерева.
    /// Если одновременно редактируется несколько документов, то проверяются все они.
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="columnName">Имя поля</param>
    /// <param name="canMultiEdit">Если true, то допускается редактирование для нескольких документов или поддокументов.
    /// В этом случае на форму может быть добавлен <see cref="CheckBox"/> (обычно взамен метки поля) для изменения "серых" значений.</param>
    /// <returns>Переходник, реализующий интерфейс <see cref="IUIExtEditItem"/></returns>
    public ExtValueDocComboBox AddRefToParent(EFPDocComboBox controlProvider, string columnName, bool canMultiEdit)
    {
      //DBxColumnStruct cs = this.DocTypeUI.DocType.Struct.Columns[columnName];
      //if (cs.MasterTableName!=controlProvider.)
      ExtValueDocComboBox dv = AddRef(controlProvider, columnName, canMultiEdit);

      DocComboBoxParentIdValidator validator = new DocComboBoxParentIdValidator();
      validator.Docs = this.Editor.Documents[controlProvider.DocTypeName];
      validator.ControlProvider = controlProvider;
      validator.ParentColumnName = columnName;
      controlProvider.Validating += validator.Validate;

      return dv;
    }
#endif

    #endregion
  }

  /// <summary>
  /// Делегат события <see cref="DocTypeUI.InitEditForm"/>
  /// </summary>
  /// <param name="sender">Объект <see cref="DocTypeUI"/></param>
  /// <param name="args">Аргументы события</param>
  public delegate void InitDocEditFormEventHandler(object sender, InitDocEditFormEventArgs args);

  #endregion

  #region Инициализация формы поддокумента

  /// <summary>
  /// Аргументы события <see cref="SubDocTypeUI.InitEditForm"/>
  /// </summary>
  public class InitSubDocEditFormEventArgs : InitDocEditFormEventArgsBase
  {
    #region Конструктор

    /// <summary>
    /// Создается в <see cref="SubDocTypeUI"/>
    /// </summary>
    /// <param name="editor">Редактор поддокумента</param>
    public InitSubDocEditFormEventArgs(SubDocumentEditor editor)
      : base(editor.Dialog, editor.SubDocs.Values)
    {
      _Editor = editor;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Редактор поддокумента, инициализация которого выполняется
    /// </summary>
    public SubDocumentEditor Editor { get { return _Editor; } }
    private readonly SubDocumentEditor _Editor;

    /// <summary>
    /// Редактор основного документа, для которого открывается редактор поддокумента
    /// </summary>
    public DocumentEditor MainEditor { get { return Editor.MainEditor; } }

    /// <summary>
    /// Коллекция для добавления объектов синхронизации
    /// </summary>
    public DepSyncCollection Syncs { get { return MainEditor.Dialog.Syncs; } }

    /// <summary>
    /// Возвращает значение свойства <see cref="SubDocumentEditor.IsReadOnly"/>
    /// </summary>
    public override bool IsReadOnly { get { return Editor.IsReadOnly; } }

    /// <summary>
    /// Возвращает свойство <see cref="DocTypeUIBase.SingleDocImageKey"/>
    /// </summary>
    public override string MainImageKey
    {
      get { return Editor.SubDocTypeUI.SingleDocImageKey; }
    }

    #endregion
  }

  /// <summary>
  /// Делегат события <see cref="SubDocTypeUI.InitEditForm"/>
  /// </summary>
  /// <param name="sender">Объект <see cref="SubDocTypeUI"/></param>
  /// <param name="args">Аргументы события</param>
  public delegate void InitSubDocEditFormEventHandler(object sender, InitSubDocEditFormEventArgs args);

  #endregion
}

