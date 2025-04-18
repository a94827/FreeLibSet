﻿// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.DependedValues;
using FreeLibSet.Data.Docs;
using FreeLibSet.Models.Tree;
using FreeLibSet.Data;
using System.Data;
using FreeLibSet.Core;

namespace FreeLibSet.Forms.Docs
{
  /// <summary>
  /// Провайдер комбоблока выбора группы документов.
  /// Группы являются отдельным видом документов, которые на стороне клиента представлены описываются <see cref="GroupDocTypeUI"/>.
  /// Документы группы образуют иерархическую структуру.
  /// Комбоблок может отображаться в верхней части формы <see cref="DocTableViewForm"/>. Первоначально позволяет выбрать только группы верхнего уровня.
  /// После того, как выбрана группа верхнего уровня, в выпадающем списке отображаются группы второго уровня, относящиеся к этой группе, и т.д.
  /// В дереве отображается корневой узел "Все документы" / "Документы без групп". Дополнительно имеется свойство <see cref="EFPGroupDocComboBox.IncludeNested"/>, которое не влияет непосредственно на работу комбоблока,
  /// но нужно для правильного отображения корневого узла.
  /// Для прорисовки выпадающего списка используется <see cref="ListControlImagePainter"/>.
  /// </summary>
  /// <seealso cref="EFPGroupDocTreeView"/>
  public class EFPGroupDocComboBox : EFPControl<ComboBox>
  {
    // Класс нельзя выводить ни из EFPDocComboBox, ни из EFPListComboBox

    #region Конструктор

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    /// <param name="docTypeUI">Вид документа групп</param>
    public EFPGroupDocComboBox(EFPBaseProvider baseProvider, ComboBox control, GroupDocTypeUI docTypeUI)
      : base(baseProvider, control, true)
    {
      if (docTypeUI == null)
        throw new ArgumentNullException("docTypeUI");
      _DocTypeUI = docTypeUI;

      _DocId = 0;
      _IncludeNested = true;

      new ListControlImagePainter(control, new ListControlImageEventHandler(ControlPainter));
      control.DropDownStyle = ComboBoxStyle.DropDownList; // 20.12.2022
      control.DropDown += new EventHandler(Control_DropDown);
      //Control.DropDownClosed += new EventHandler(Control_DropDownClosed);
      control.SelectedValueChanged += new EventHandler(Control_SelectedValueChanged);
      base.UseIdle = true;

      _BrowserGuid = Guid.NewGuid();
    }

    #endregion

    #region Прочие свойства

    /// <summary>
    /// Вид документа групп
    /// </summary>
    public GroupDocTypeUI DocTypeUI { get { return _DocTypeUI; } }
    private readonly GroupDocTypeUI _DocTypeUI;

    #endregion

    #region Свойство DocId

    /// <summary>
    /// Идентификатор выбранной группы.
    /// Значение DocTypeUi.RootNodeDocId означает "Все документы"
    /// </summary>
    public Int32 DocId
    {
      get { return _DocId; }
      set
      {
        if (value == _DocId)
          return;
        _DocId = value;

        if (!Control.DroppedDown)
        {
          Control.SelectedIndex = -1;
          Control.Invalidate();
        }
        if (_DocIdEx != null)
          _DocIdEx.Value = value;
        Validate();

        if (CommandItemsAssigned)
        {
          if (CommandItems is EFPGroupDocComboBoxCommandItems)
            ((EFPGroupDocComboBoxCommandItems)CommandItems).InitEnabled();
        }
      }
    }
    private Int32 _DocId;

    /// <summary>
    /// Управляемое свойство для DocId
    /// </summary>
    public DepValue<Int32> DocIdEx
    {
      get
      {
        InitDocIdEx();
        return _DocIdEx;
      }
      set
      {
        InitDocIdEx();
        _DocIdEx.Source = value;
      }
    }
    private DepInput<Int32> _DocIdEx;

    private void InitDocIdEx()
    {
      if (_DocIdEx == null)
      {
        _DocIdEx = new DepInput<Int32>(DocId,DocIdEx_ValueChanged);
        _DocIdEx.OwnerInfo = new DepOwnerInfo(this, "DocIdEx");
      }
    }

    private void DocIdEx_ValueChanged(object sender, EventArgs args)
    {
      DocId = _DocIdEx.Value;
    }

    #endregion

    #region Свойство IncludeNested

    /// <summary>
    /// Признак "Включая вложенные папки".
    /// Это свойство влияет только на отображение элемента при <see cref="DocId"/>=0.
    /// Если true (по умолчанию), то отображается строка "Все документы",
    /// иначе отображается "Документы без иерархии".
    /// </summary>
    public bool IncludeNested
    {
      get { return _IncludeNested; }
      set
      {
        if (value == _IncludeNested)
          return;
        _IncludeNested = value;
        if (_IncludeNestedEx != null)
          _IncludeNestedEx.Value = value;
        Control.Invalidate();
      }
    }
    private bool _IncludeNested;

    /// <summary>
    /// Управляемое свойство для <see cref="IncludeNested"/>.
    /// Обычно его делают равным свойству <see cref="EFPCheckBox.CheckedEx"/> для флажка "Включая вложенные группы" или для аналогичной команды локального меню.
    /// </summary>
    public DepValue<bool> IncludeNestedEx
    {
      get
      {
        InitIncludeNestedEx();
        return _IncludeNestedEx;
      }
      set
      {
        InitIncludeNestedEx();
        _IncludeNestedEx.Source = value;
      }
    }
    private DepInput<bool> _IncludeNestedEx;

    private void InitIncludeNestedEx()
    {
      if (_IncludeNestedEx == null)
      {
        _IncludeNestedEx = new DepInput<bool>(IncludeNested,IncludeNestedEx_ValueChanged);
        _IncludeNestedEx.OwnerInfo = new DepOwnerInfo(this, "IncludeNestedEx");
      }
    }

    private void IncludeNestedEx_ValueChanged(object sender, EventArgs args)
    {
      IncludeNested = _IncludeNestedEx.Value;
    }

    #endregion

    #region Модель дерева

    /// <summary>
    /// Модель дерева.
    /// При первом обращении к свойству создает <see cref="DBxDocTreeModel"/>.
    /// </summary>
    public DBxDocTreeModel Model
    {
      get
      {
        if (_Model == null)
          _Model = new DBxDocTreeModel(DocTypeUI.UI.DocProvider,
            DocTypeUI.DocType,
            new DBxColumns(new string[] { "Id", DocTypeUI.DocType.TreeParentColumnName, DocTypeUI.NameColumnName }));
        return _Model;
      }
    }
    private DBxDocTreeModel _Model;

    /// <summary>
    /// Обновление модели дерева
    /// </summary>
    public void PerformRefresh()
    {
      _Model = null;
      Control.SelectedIndex = -1;
      Control.Invalidate();
      Validate();
    }

    #endregion

    #region Свойство AuxFilterGroupIds

    /// <summary>
    /// Возвращает массив идентификаторов отфильтрованных групп документов.
    /// Если выбраны "Все документы", возвращает null.
    /// Если выбраны "Документы без групп", возвращает массив нулевой длины.
    /// Если есть выбранная группа <see cref="DocId"/>, возвращает массив из одного или нескольких элементов,
    /// в зависимости от <see cref="IncludeNested"/>.
    /// </summary>
    public Int32[] AuxFilterGroupIds
    {
      get
      {
        if (DocId == 0)
        {
          if (IncludeNested)
            return null;
          else
            return DataTools.EmptyIds;
        }
        else
        {
          if (IncludeNested)
            return Model.GetIdWithChildren(DocId);
          else
            return new Int32[1] { DocId };
        }
      }
    }

    #endregion

    #region Элементы в выпадающем списке

    /// <summary>
    /// Элементы, добавляемые в комбоблок.
    /// Не имеет смысла делать структурой, т.к. при добавлении в комбоблок элементы преобразуются в Object
    /// </summary>
    private class ItemObject
    {
      #region Конструктор

      /// <summary>
      /// Создание корневого узла
      /// </summary>
      public ItemObject()
        : this(0, 0 , true)
      {
      }

      public ItemObject(Int32 docId, int indentLevel, bool isOpen)
      {
        _DocId = docId;
        _IndentLevel = indentLevel;
        _IsOpen = isOpen;
      }

      #endregion

      #region Свойства

      /// <summary>
      /// Идентификатор группы
      /// </summary>
      public Int32 DocId { get { return _DocId; } }
      private readonly Int32 _DocId;

      /// <summary>
      /// Уровень вложения
      /// </summary>
      public int IndentLevel { get { return _IndentLevel; } }
      private readonly int _IndentLevel;

      public bool IsOpen { get { return _IsOpen; } }
      private readonly bool _IsOpen;

      #endregion

      #region Текстовое представление

      public override string ToString()
      {
        return _DocId.ToString();
      }

      #endregion
    }

    /// <summary>
    /// Флажок устанавливается на время заполнения выпадающего списка
    /// </summary>
    private bool _InsideInitList;

    private List<ItemObject> InitItemList(out int selIndex)
    {
      List<ItemObject> items = new List<ItemObject>();

      // 1. Корневой узел
      items.Add(new ItemObject());
      selIndex = 0;

      int indentLevel = 1; // Отступ для текущего узла


      // 2. Иерархия до текущего узла, не включая его (испр. 17.11.2017)
      if (DocId != 0)
      {
        object[] rows = Model.TreePathFromId(DocId).FullPath;
        for (int i = 0; i < rows.Length - 1; i++)
        {
          object[] partRows = new object[i + 1];
          Array.Copy(rows, partRows, i + 1);
          TreePath partPath = new TreePath(partRows);

          Int32 thisDocId = Model.TreePathToId(partPath);
          items.Add(new ItemObject(thisDocId, i + 1, true));
        }

        indentLevel = rows.Length;
      }

      // 3. Список узлов, находящихся на одном уровне с текущим, включая текущий
      TreePath parentPath = Model.TreePathFromId(DocId).Parent;
      foreach (object row in Model.GetChildren(parentPath))
      {
        TreePath thisPath = new TreePath(parentPath, row);
        Int32 thisDocId = Model.TreePathToId(thisPath);
        items.Add(new ItemObject(thisDocId, indentLevel, thisDocId == DocId));
        if (thisDocId == DocId)
        {
          selIndex = items.Count - 1;

          // 4. Список дочерних узлов
          foreach (object row2 in Model.GetChildren(thisPath))
          {
            TreePath thisPath2 = new TreePath(thisPath, row2);
            Int32 thisDocId2 = Model.TreePathToId(thisPath2);
            items.Add(new ItemObject(thisDocId2, indentLevel + 1, false));
          }
        }
      }

      return items;
    }

    #endregion

    #region Обработчики комбоблока

    void Control_DropDown(object sender, EventArgs args)
    {
      try
      {
        _InsideInitList = true;
        try
        {
          int selIndex;
          List<ItemObject> items = InitItemList(out selIndex);
          Control.Items.Clear();
          Control.Items.AddRange(items.ToArray());
          Control.SelectedIndex = selIndex;
        }
        finally
        {
          _InsideInitList = false;
        }
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e);
      }
    }

    // Следует учитывать разный порядок вызова событий SelectedValueChanged и DropDownClosed
    // при работе с мыщью и с клавиатурой

    //void Control_DropDownClosed(object Sender, EventArgs Args)
    //{
    //  try
    //  {
    //    InsideInitList = true;
    //    try
    //    {
    //      Control.SelectedItem = null;
    //      Control.Items.Clear(); // чтобы нельзя было переключать непонятно что нажатием стрелочек 
    //                             // "вверх/вниз" при закрытом списке
    //    }
    //    finally
    //    {
    //      InsideInitList = false;
    //    }
    //  }
    //  catch (Exception e)
    //  {
    //    EFPApp.ShowException(e, "EFPGroupDocComboBox. Ошибка обработки DropDownClosed");
    //  }
    //}

    void Control_SelectedValueChanged(object sender, EventArgs args)
    {
      try
      {
        if (!_InsideInitList)
        {
          ItemObject item = (ItemObject)(Control.SelectedItem);
          if (item != null)
            DocId = item.DocId;
        }
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e);
      }
    }

    /// <summary>
    /// Метод обработки события Idle.
    /// Вызывается периодически, когда приложение ничем не занято.
    /// </summary>
    public override void HandleIdle()
    {
      // 18.11.2017
      // Нельзя выполнять очистку списка в DropDownClosed, т.к. при работе с мышью сначала закрывается список,
      // а затем вызывается SelectedValueChanged
      if ((!Control.DroppedDown) && Control.SelectedItem != null)
      {
        try
        {
          _InsideInitList = true;
          try
          {
            Control.SelectedItem = null;
            Control.Items.Clear(); // чтобы нельзя было переключать непонятно что нажатием стрелочек 
            // "вверх/вниз" при закрытом списке
          }
          finally
          {
            _InsideInitList = false;
          }
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e);
        }
      }
    }

    #endregion

    #region Рисование списка

    private void ControlPainter(object sender, ListControlImageEventArgs args)
    {
      ItemObject item;
      if (args.ItemIndex < 0)
        item = new ItemObject(DocId, 0, true);
      else
        item = (ItemObject)(args.Item);

      PaintItem(args, item);
    }

    private void PaintItem(ListControlImageEventArgs args, ItemObject item)
    {
#if DEBUG
      if (item == null)
        throw new ArgumentNullException("item");
#endif
      args.Text = item.ToString();
      if (item.DocId == 0)
      {
        if (IncludeNested)
        {
          args.Text = "[ " + DocTypeUI.AllGroupsDisplayName + " ]";
          args.ImageKey = "Table";
        }
        else
        {
          args.Text = "[ " + DocTypeUI.NoGroupDisplayName + " ]";
          args.ImageKey = "No";
        }
      }
      else
      {
        args.Text = DocTypeUI.GetTextValue(item.DocId);
        if (this._DocTypeUI.UI.DebugShowIds)
          args.Text += " (Id=" + item.DocId + ")";
        if (item.IsOpen)
          args.ImageKey = "TreeViewOpenFolder";
        else
          args.ImageKey = "TreeViewClosedFolder";
      }
      args.LeftMargin = 16 * item.IndentLevel;
    }

    #endregion

    #region Реализация DocumentViewHandler

    private class IntDocumentViewHandler : DocumentViewHandler
    {
      #region Свойства

      public EFPGroupDocComboBox Owner;

#pragma warning disable 0649 // TODO: Убрать лишние переменные

      public DocumentViewHandler ExternalEditorCaller;

#pragma warning restore 0649


      #endregion

      #region Переопределенные методы и свойства

      public override DocTypeUI DocTypeUI
      {
        get
        {
          if (Owner == null)
            return null;
          else
            return Owner.DocTypeUI;
        }
      }

      public override Guid BrowserGuid
      {
        get
        {
          if (Owner == null)
            return Guid.Empty;
          else
            return Owner.BrowserGuid;
        }
      }

      public override string CurrentColumnName
      {
        get
        {
          if (Owner == null)
            return String.Empty;
          else
            return Owner.DocTypeUI.NameColumnName;
        }
      }

      /// <summary>
      /// Обновление табличного просмотра
      /// </summary>
      /// <param name="dataSet"></param>
      /// <param name="isCaller"></param>
      public override void ApplyChanges(DataSet dataSet, bool isCaller)
      {
        if (Owner == null)
          return;


        if (!dataSet.Tables.Contains(Owner.DocTypeUI.DocType.Name))
          return; // Нет таблицы

        // Просто обновляем модель
        Owner._Model = null;
        Owner.Control.Invalidate();
      }

      //public override void UpdateRowsForIds32(int[] DocIds)
      //{
      //  if (Owner != null)
      //  {
      //    // Просто обновляем модель
      //    Owner.FModel = null;
      //    Owner.Control.Invalidate();
      //  }
      //}

      public override void InitNewDocValues(DBxSingleDoc newDoc)
      {
        if (ExternalEditorCaller == null)
        {
          if (Owner != null)
          {
            if (Owner.DocId != 0)
              newDoc.Values[Owner.DocTypeUI.DocType.GroupRefColumnName].SetInteger(Owner.DocId);
          }
        }
        else
          ExternalEditorCaller.InitNewDocValues(newDoc);
      }

      //public override void ValidateDocValues(DBxSingleDoc SavingDoc, ErrorMessageList ErrorMessages)
      //{
      //}

      public override void InvalidateControl()
      {
        if (Owner != null)
          Owner.Control.Invalidate();
      }

      /// <summary>
      /// Возвращает Owner.ToString(), если объект присоединен к просмотру
      /// </summary>
      /// <returns>Текстовое представление для табличного просмотра</returns>
      public override string ToString()
      {
        if (Owner == null)
          return "[ Disconnected from data view ]";
        else
          return Owner.ToString();
      }

      #endregion
    }

    /// <summary>
    /// Обработчик просмотра документов, связанный с текущим просмотром.
    /// Свойство имеет значение не null, когда просмотр выведен на экран.
    /// </summary>
    public DocumentViewHandler ViewHandler { get { return _ViewHandler; } }
    private IntDocumentViewHandler _ViewHandler;

    /// <summary>
    /// Уникальный идентификатор табличного просмотра.
    /// Используется <see cref="DocumentViewHandler"/>.
    /// </summary>
    public Guid BrowserGuid
    {
      get { return _BrowserGuid; }
      set { _BrowserGuid = value; }
    }
    private Guid _BrowserGuid;

    /// <summary>
    /// Метод вызывается при первом появлении элемента на экране.
    /// Инициализирует свойство <see cref="ViewHandler"/>.
    /// </summary>
    protected override void OnCreated()
    {
      base.OnCreated();

      _ViewHandler = new IntDocumentViewHandler();
      _ViewHandler.Owner = this;
      DocTypeUI.Browsers.Add(_ViewHandler);
    }


    /// <summary>
    /// Вызывается, когда форма с управляющим элементом закрывается.
    /// Удаляет свойство <see cref="ViewHandler"/>.
    /// </summary>
    protected override void OnDetached()
    {
      if (_ViewHandler != null)
      {
        _ViewHandler.Owner = null; // разрыв ссылки, чтобы текущий просмотр мог быть удален
        DocTypeUI.Browsers.Remove(_ViewHandler);
        _ViewHandler = null;
      }
      base.OnDetached();
    }

    #endregion

    #region Локальное меню

    /// <summary>
    /// Команды локального меню.
    /// </summary>
    public new EFPGroupDocComboBoxCommandItems CommandItems { get { return (EFPGroupDocComboBoxCommandItems)(base.CommandItems); } }

    /// <summary>
    /// Создает команды локального меню <see cref="EFPGroupDocComboBoxCommandItems"/>.
    /// </summary>
    /// <returns>Созданный объект</returns>
    protected override EFPControlCommandItems CreateCommandItems()
    {
      EFPGroupDocComboBoxCommandItems items = new EFPGroupDocComboBoxCommandItems(this);
      items.InitEnabled();
      return items;
    }

    #endregion
  }

  /// <summary>
  /// Команды локального меню для комбоблока <see cref="EFPGroupDocComboBox"/>.
  /// Поддерживает команды буфера обмена для <see cref="DBxDocSelection"/> и команду "Информация о документе" (группе).
  /// </summary>
  public class EFPGroupDocComboBoxCommandItems : EFPControlCommandItems
  {
    #region Конструктор

    /// <summary>
    /// Создает список команд
    /// </summary>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    public EFPGroupDocComboBoxCommandItems(EFPGroupDocComboBox controlProvider)
      : base(controlProvider)
    {
      ciCut = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Cut);
      ciCut.GroupBegin = true;
      ciCut.Click += new EventHandler(ciCut_Click);
      Add(ciCut);

      ciCopy = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Copy);
      ciCopy.Click += new EventHandler(ciCopy_Click);
      Add(ciCopy);

      ciPaste = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Paste);
      ciPaste.GroupEnd = true;
      ciPaste.Click += new EventHandler(ciPaste_Click);
      Add(ciPaste);

      ciShowDocInfo = new EFPCommandItem("View", "DocInfo");
      ciShowDocInfo.MenuText = Res.Cmd_Menu_DocInfo;
      ciShowDocInfo.ShortCut = Keys.F12;
      ciShowDocInfo.ImageKey = "Information";
      ciShowDocInfo.Click += new EventHandler(ciShowDocInfo_Click);
      ciShowDocInfo.GroupBegin = true;
      ciShowDocInfo.GroupEnd = true;
      Add(ciShowDocInfo);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер управляющего элемента
    /// </summary>
    public new EFPGroupDocComboBox ControlProvider { get { return (EFPGroupDocComboBox)(base.ControlProvider); } }

    #endregion

    #region Инициализация доступности команд

    /// <summary>
    /// Инициализация доступности команд.
    /// </summary>
    public void InitEnabled()
    {
      ciCut.Enabled = ControlProvider.DocId != 0;
      ciCopy.Enabled = ControlProvider.DocId != 0;
      ciPaste.Enabled = true;
      ciShowDocInfo.Enabled = ControlProvider.DocId != 0;
    }

    #endregion

    #region Команды буфера обмена

    private EFPCommandItem ciCut, ciCopy, ciPaste;

    void ciCut_Click(object sender, EventArgs args)
    {
      ciCopy_Click(null, null);
      ControlProvider.DocId = 0;
    }

    void ciCopy_Click(object sender, EventArgs args)
    {
      if (ControlProvider.DocId == 0)
      {
        EFPApp.ShowTempMessage(Res.Common_Err_NoSelectedDoc);
        return;
      }
      DBxDocSelection docSel = new DBxDocSelection(ControlProvider.DocTypeUI.UI.DocProvider.DBIdentity);
      docSel.Add(ControlProvider.DocTypeUI.DocType.Name, ControlProvider.DocId);
      DataObject dObj = new DataObject();
      dObj.SetData(docSel);
      ControlProvider.DocTypeUI.UI.OnAddCopyFormats(dObj, docSel);
      dObj.SetText(ControlProvider.DocTypeUI.GetTextValue(ControlProvider.DocId));
      new EFPClipboard().SetDataObject(dObj, true);
    }

    void ciPaste_Click(object sender, EventArgs args)
    {
      DBxDocSelection docSel = ControlProvider.DocTypeUI.UI.PasteDocSel();
      if (docSel == null)
      {
        EFPApp.ShowTempMessage(Res.Clipboard_Err_NoDocSel);
        return;
      }

      Int32[] ids = docSel[ControlProvider.DocTypeUI.DocType.Name];
      if (ids.Length == 0)
      {
        EFPApp.ShowTempMessage(String.Format(Res.Clipboard_Err_NoDocType, ControlProvider.DocTypeUI.DocType.PluralTitle));
        return;
      }

      ControlProvider.DocId = ids[0];
    }

    #endregion

    #region Информация о документе

    EFPCommandItem ciShowDocInfo;

    void ciShowDocInfo_Click(object sender, EventArgs args)
    {
      if (ControlProvider.DocId == 0)
      {
        EFPApp.ShowTempMessage(Res.Common_Err_NoSelectedDoc);
        return;
      }

      ControlProvider.DocTypeUI.ShowDocInfo(ControlProvider.DocId);
    }

    #endregion
  }
}
