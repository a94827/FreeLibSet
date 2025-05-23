﻿// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

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
  /// Расширяет обычный просмотр документов узлом "Все документы".
  /// Дерево может отображаться в левой части формы <see cref="DocTableViewForm"/>. 
  /// </summary>
  /// <seealso cref="EFPGroupDocComboBox"/>
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
      AlwaysUseDefaultConfig = true; // 25.03.2024
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
          throw new ArgumentException(Res.EFPGroupDocTreeView_Arg_MultiSelect);
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

    #region Переопределенные методы и свойства

    /// <summary>
    /// Обновление данных или первоначальная загрузка
    /// </summary>
    /// <param name="args">Не используется</param>
    protected override void OnRefreshData(EventArgs args)
    {
      if (args == null)
        EFPApp.BeginWait(Res.Common_Phase_DataLoad, DocTypeUI.TableImageKey);
      else
        EFPApp.BeginWait(Res.Common_Phase_Refresh, "Refresh");
      try
      {
        DBxDocTreeModel model = new DBxDocTreeModel(DocTypeUI.UI.DocProvider, DocTypeUI.DocType, UsedColumnNames);
        AddRootNode(model);
        Control.Model = model;

        base.CallRefreshDataEventHandler(args);
      }
      finally
      {
        EFPApp.EndWait();
      }
    }

    /// <summary>
    /// Возвращает имя изображения для документа, к которому относится узел.
    /// Изображение находится в списке <seealso cref="EFPApp.MainImages"/>.
    /// </summary>
    /// <param name="node">Узел</param>
    /// <returns>Имя изображения</returns>
    public override string GetNodeImageKey(TreeNodeAdv node)
    {
      DataRow row = node.Tag as DataRow;
      if (row == null)
        return base.GetNodeImageKey(node);

      Int32 docId = DataTools.GetInt(row, "Id");
      if (docId == RootNodeDocId)
        return RootNodeImageKey;
      else if (node.IsLeaf)
        return node.IsSelected ? "TreeViewOpenFolder" : "TreeViewClosedFolder";
      else
        //return base.GetNodeImageKey(Node);
        return String.Empty; // раскрывающийся конвертик
    }

    /// <summary>
    /// Повторная инициализация текста корневого узла
    /// </summary>
    protected override void OnAttached()
    {
      base.OnAttached();

      // 04.02.2022
      // Модель может быть инициализирована до показа дерева на экране, когда столбцы просмотра еще созданы.
      // Обновляем текст корневого узла

      if (_RootNodeDataRow != null)
      {
        Int32 oldId = base.CurrentId;

        if (GetFirstNodeControl<BaseTextControl>() != null)
          _RootNodeDataRow[GetFirstNodeControl<BaseTextControl>().DataPropertyName] = this.RootNodeTextValue;
        DBxDocTreeModel model = (DBxDocTreeModel)(Control.Model);
        model.RefreshNode(model.TreePathFromDataRow(_RootNodeDataRow));

        base.CurrentId = oldId; // 20.06.2022
        // Хорошо бы, конечно, иметь возможность избежать отправки события StructureChanged для всего дерева.
      }
    }

    /// <summary>
    /// Вызывает <seealso cref="FreeLibSet.Forms.Docs.DocTypeUI.PerformEditing(int, bool)"/> для выбранного узла
    /// </summary>
    /// <param name="args">Не используется</param>
    /// <returns>Игнорируется</returns>
    protected override bool OnEditData(EventArgs args)
    {
      return DocTypeUI.PerformEditing(this.SelectedIds, State, Control.FindForm().Modal, ViewHandler);
    }

    /// <summary>
    /// Возвращает false, так как не нужна команда настройки просмотра
    /// </summary>
    public override bool HasConfigureViewHandler
    {
      get { return false; } // 25.03.2024
    }

    #endregion

    #region Свойство IncludeNested

    /// <summary>
    /// Признак "Включая вложенные папки".
    /// Это свойство влияет на отображение корневого узла,
    /// если свойства <seealso cref="RootNodeTextValue"/> и <seealso cref="RootNodeImageKey"/> не установлены в явном виде.
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

        if (_RootNodeDataRow != null)
        {
          if (GetFirstNodeControl<BaseTextControl>() != null)
            _RootNodeDataRow[GetFirstNodeControl<BaseTextControl>().DataPropertyName] = this.RootNodeTextValue;
          DBxDocTreeModel model = (DBxDocTreeModel)(Control.Model);
          model.RefreshNode(model.TreePathFromDataRow(_RootNodeDataRow));
        }

        Control.Invalidate();
      }
    }
    private bool _IncludeNested;

    /// <summary>
    /// Управляемое свойство для <seealso cref="IncludeNested"/>.
    /// Обычно его делают равным свойству <seealso cref="EFPCheckBox.CheckedEx"/> для флажка "Включая вложенные группы" или для аналогичной команды локального меню.
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
        _IncludeNestedEx = new DepInput<bool>(IncludeNested, IncludeNestedEx_ValueChanged);
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
      model.BeginUpdate();
      try
      {
        // 22.02.2022
        // Сначала нужно добавить корневой узел, а потом уже делать ссылки на него
        _RootNodeDataRow = model.Table.NewRow();
        _RootNodeDataRow["Id"] = RootNodeDocId;
        BaseTextControl tc = base.GetFirstNodeControl<BaseTextControl>();
        if (tc != null)
          _RootNodeDataRow[tc.DataPropertyName] = RootNodeTextValue;
        model.Table.Rows.Add(_RootNodeDataRow);


        foreach (DataRow row in model.Table.Rows)
        {
          if (!Object.ReferenceEquals(row, _RootNodeDataRow))
          {
            if (DataTools.GetInt(row, model.ParentColumnName) == 0)
              row[model.ParentColumnName] = RootNodeDocId;
          }
        }
      }
      finally
      {
        model.EndUpdate();
      }
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
        if (_RootNodeDataRow != null && GetFirstNodeControl<BaseTextControl>() != null)
          _RootNodeDataRow[GetFirstNodeControl<BaseTextControl>().DataPropertyName] = this.RootNodeTextValue;
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
          DBxDocTreeModel model = (DBxDocTreeModel)(Control.Model);
          model.RefreshNode(model.TreePathFromDataRow(_RootNodeDataRow));
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
      /// Заполнение строки <seealso cref="DataRow"/> <paramref name="resRow"/>, используемой моделью иерархического просмотра.
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
            throw new BugException("Attempt to refresh for fictive row");
          }
          catch (Exception e)
          {
            LogoutTools.LogoutException(e);
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
    /// Создает объект <seealso cref="DocumentViewHandler"/> для просмотра.
    /// </summary>
    /// <returns>Объект класса <seealso cref="IntDocumentViewHandler"/></returns>
    protected override EFPDocTreeView.IntDocumentViewHandler CreateDocumentViewHandler()
    {
      return new IntDocumentViewHandler(this); // // 10.06.2019 Переопределенный класс
    }

    #endregion
  }
}
