using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using FreeLibSet.Data.Docs;
using FreeLibSet.Controls.TreeViewAdvNodeControls;
using FreeLibSet.Logging;
using FreeLibSet.DependedValues;
using FreeLibSet.Controls;
using FreeLibSet.Core;

namespace FreeLibSet.Forms.Docs
{

  /// <summary>
  /// Иерархический просмотр дерева групп.
  /// Расширяет обычный просмотр документов узлом "Все документы"
  /// </summary>
  public class EFPGroupDocTreeView : EFPDocTreeView, IEFPDBxView
  {
    #region Конструкторы


    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент - дерево</param>
    /// <param name="docTypeUI">Интерфейс доступа к виду документов группы</param>
    public EFPGroupDocTreeView(EFPBaseProvider baseProvider, TreeViewAdv control, GroupDocTypeUI docTypeUI)
      : base(baseProvider, control, docTypeUI)
    {
      Init();
    }

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="controlWithToolBar">Управляющий элемент и панель инструментов</param>
    /// <param name="docTypeUI">Интерфейс доступа к виду документов группы</param>
    public EFPGroupDocTreeView(IEFPControlWithToolBar<TreeViewAdv> controlWithToolBar, GroupDocTypeUI docTypeUI)
      : base(controlWithToolBar, docTypeUI)
    {
      Init();
    }

    private void Init()
    {
      Control.SelectionMode = TreeViewAdvSelectionMode.Single;
      _IncludeNested = true;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Интерфейс доступа к документам группы
    /// </summary>
    public new GroupDocTypeUI DocTypeUI { get { return (GroupDocTypeUI)(base.DocTypeUI); } }

    /// <summary>
    /// Идентификатор выбранной группы.
    /// Для узла "Все документы" возвращается 0.
    /// Дерево групп не поддерживает множественный выбор документов, поэтому свойство возвращает
    /// массив, состояший из одного идентификатора или пустой массив.
    /// Попытка установки свойства как массива с несколькими элементами, приводит к выбросу исключения.
    /// </summary>
    public override Int32[] SelectedIds
    {
      get
      {
        if (CurrentId == 0)
          return DataTools.EmptyIds;
        else
          return new Int32[1] { CurrentId };
      }
      set
      {
        if (value == null)
          value = DataTools.EmptyIds;
        if (value.Length == 0)
          CurrentId = 0;
        else if (value.Length > 1)
          throw new ArgumentException("Множественный выбор узлов в дереве групп документов не допускается");
        else
          CurrentId = value[0];
      }
    }

    /// <summary>
    /// Идентификатор выбранной группы.
    /// Для узла "Все документы" возвращается 0.
    /// </summary>
    public override Int32 CurrentId
    {
      get
      {
        Int32 v = base.CurrentId;
        if (v == RootNodeDocId)
          return 0;
        else
          return v;
      }
      set
      {
        if (value == 0)
          base.CurrentId = RootNodeDocId;
        else
          base.CurrentId = value;
      }
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Обновление данных или первоначальная загрузка
    /// </summary>
    /// <param name="args">Не используется</param>
    protected override void OnRefreshData(EventArgs args)
    {
      if (args == null)
        EFPApp.BeginWait("Загрузка данных", DocTypeUI.TableImageKey);
      else
        EFPApp.BeginWait("Обновление данных", "Refresh");
      try
      {
        DBxDocTreeModel Model = new DBxDocTreeModel(DocTypeUI.UI.DocProvider, DocTypeUI.DocType, UsedColumnNames);
        AddRootNode(Model);
        Control.Model = Model;

        base.CallRefreshDataEventHandler(args);
      }
      finally
      {
        EFPApp.EndWait();
      }
    }

    /// <summary>
    /// Возвращает имя изображения для документа, к которому относится узел.
    /// Изображение находится в списке EFPApp.MainImages
    /// </summary>
    /// <param name="node">Узел</param>
    /// <returns>Имя изображения</returns>
    public override string GetNodeImageKey(TreeNodeAdv node)
    {
      DataRow Row = node.Tag as DataRow;
      if (Row == null)
        return base.GetNodeImageKey(node);

      Int32 DocId = DataTools.GetInt(Row, "Id");
      if (DocId == RootNodeDocId)
        return RootNodeImageKey;
      else if (node.IsLeaf)
        return node.IsSelected ? "TreeViewOpenFolder" : "TreeViewClosedFolder";
      else
        //return base.GetNodeImageKey(Node);
        return String.Empty; // раскрывающийся конвертик
    }

    /// <summary>
    /// Вызывает DocTypeUI.PerformEditing() для выбранных узлов
    /// </summary>
    /// <param name="args">Не используется</param>
    /// <returns>Игнорируется</returns>
    protected override bool OnEditData(EventArgs args)
    {
      return DocTypeUI.PerformEditing(this.SelectedIds, State, Control.FindForm().Modal, ViewHandler);
    }

    #endregion

    #region Свойство IncludeNested

    /// <summary>
    /// Признак "Включая вложенные папки".
    /// Это свойство влияет на отображение корневого узла,
    /// если свойства RootNodeTextValue и RootNodeImageKey не установлены в явном виде.
    /// Если true (по умолчанию), то отображается строка "Все документы",
    /// иначе отображается "Документы без иерархии".
    /// Также свойство влияет на значение, возвращаемое AuxFilterGroupIds.
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

        if (_RootNodeDataRow != null)
        {
          if (FirstTextControl != null)
            _RootNodeDataRow[FirstTextControl.DataPropertyName] = this.RootNodeTextValue;
          DBxDocTreeModel Model = (DBxDocTreeModel)(Control.Model);
          Model.RefreshNode(Model.TreePathFromDataRow(_RootNodeDataRow));
        }

        Control.Invalidate();
      }
    }
    private bool _IncludeNested;

    /// <summary>
    /// Управляемое свойство IncludeNested.
    /// Обычно его делают равным свойству EFPCheckBox.CheckedEx для флажка "Включая вложенные группы"
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

    #region Корневой узел

    /// <summary>
    /// Фиктивный идентификатор документа для корневого узла в деревьях
    /// </summary>
    private const Int32 RootNodeDocId = unchecked((int)0x80000000);

    /// <summary>
    /// Возвращает строку "данных", соответствуюзую корневому узлу
    /// </summary>
    //public DataRow RootNodeDataRow { get { return FRootNodeDataRow; } }
    private DataRow _RootNodeDataRow;

    private void AddRootNode(DBxDocTreeModel model)
    {
      foreach (DataRow Row in model.Table.Rows)
      {
        if (DataTools.GetInt(Row, model.ParentColumnName) == 0)
          Row[model.ParentColumnName] = RootNodeDocId;
      }

      _RootNodeDataRow = model.Table.NewRow();
      _RootNodeDataRow["Id"] = RootNodeDocId;

      BaseTextControl tc = base.FirstTextControl;
      if (tc != null)
      {
        _RootNodeDataRow[tc.DataPropertyName] = RootNodeTextValue;
      }
      model.Table.Rows.Add(_RootNodeDataRow);
    }

    /// <summary>
    /// Текст для узла с нулевым идентификатором.
    /// По умолчанию возвращает "Все документы"
    /// </summary>
    public string RootNodeTextValue
    {
      get
      {
        if (String.IsNullOrEmpty(_RootNodeTextValue))
          return "[ " + (_IncludeNested ? DocTypeUI.AllGroupsDisplayName : DocTypeUI.NoGroupDisplayName) + " ]";
        else
          return _RootNodeTextValue;
      }
      set
      {
        _RootNodeTextValue = value;
        if (_RootNodeDataRow != null && FirstTextControl != null)
          _RootNodeDataRow[FirstTextControl.DataPropertyName] = this.RootNodeTextValue;
      }
    }
    private string _RootNodeTextValue;

    /// <summary>
    /// Значок для корневого узла
    /// </summary>
    public string RootNodeImageKey
    {
      get
      {
        if (String.IsNullOrEmpty(_RootNodeImageKey))
          return _IncludeNested ? "Table" : "No";
        else
          return _RootNodeImageKey;
      }
      set
      {
        _RootNodeImageKey = value;
        if (_RootNodeDataRow != null)
        {
          DBxDocTreeModel Model = (DBxDocTreeModel)(Control.Model);
          Model.RefreshNode(Model.TreePathFromDataRow(_RootNodeDataRow));
        }
      }
    }
    private string _RootNodeImageKey;

    #endregion

    #region Реализация DocumentViewHandler

    /// <summary>
    /// Требуется учесть наличие корневого узла с фиктивным идентификатором
    /// </summary>
    protected new class IntDocumentViewHandler : EFPDocTreeView.IntDocumentViewHandler
    {
      #region Конструктор

      /// <summary>
      /// Конструктор объекта
      /// </summary>
      /// <param name="owner">Провайдер иерархического просмотра - владелец</param>
      public IntDocumentViewHandler(EFPGroupDocTreeView owner)
        : base(owner)
      {
      }

      #endregion

      /// <summary>
      /// Заполнение строки DataRow <paramref name="resRow"/>, используемой моделью ирархического просмотра.
      /// Для групп верхнего уровня заменяет значение поля "ParentId" фиктивным значением.
      /// </summary>
      /// <param name="srcRow">Строка в обновленном наборе данных</param>
      /// <param name="resRow">Заполняемая строка в модели иерархического просмотра</param>
      protected override void CopyRowValues(DataRow srcRow, DataRow resRow)
      {
        base.CopyRowValues(srcRow, resRow);

        if ((Int32)(resRow["Id"]) == RootNodeDocId)
        {
#if DEBUG
          try
          {
            throw new BugException("Попытка обновления для фиктивной строки");
          }
          catch (Exception e)
          {
            LogoutTools.LogoutException(e, "EFPGroupTreeView.InitDocumentViewHandler.CopyRowValues()");
          }
#endif
          return;
        }

        int p = resRow.Table.Columns.IndexOf("ParentId");
        if (p < 0)
          return;

        if (resRow.IsNull(p))
        {
          // Заменяем на фиктивный идентификатор
          resRow[p] = RootNodeDocId;
        }
      }
    }

    /// <summary>
    /// Создает объект DocumentViewHandler для просмотра.
    /// </summary>
    /// <returns>Объект класса, производного от DocumentViewHandler</returns>
    protected override EFPDocTreeView.IntDocumentViewHandler CreateDocumentViewHandler()
    {
      return new IntDocumentViewHandler(this); // // 10.06.2019 Переопределенный класс
    }

    #endregion
  }
}
