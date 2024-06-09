// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using FreeLibSet.Data.Docs;
using FreeLibSet.Data;
using FreeLibSet.Remoting;
using FreeLibSet.Core;

namespace FreeLibSet.Forms.Docs
{
  /// <summary>
  /// Обработчик просмотра документов. Базовый класс.
  /// Производные классы реализуются в <see cref="EFPDocGridView"/> и других объектах пользовательского интефейса.
  /// Реализует:
  /// <list type="bullet">
  /// <item><description></description>Обновление просмотра после сохранения документов в базе данных. 
  /// Коллекция действующих объектов, требующих обновления, хранится в массиве <see cref="DocTypeUI.Browsers"/>.</item>
  /// <item><description>Инициализацию начальных значений в редакторе при создании нового документа</description></item>
  /// </list>
  /// </summary>
  public abstract class DocumentViewHandler : SimpleDisposableObject
  {
    // 03.01.2021
    // Можно использовать базовый класс без деструктора

    #region Информация о просмотре

    /// <summary>
    /// Свойство возвращает описатель вида документа, к которому относится данный браузер
    /// </summary>
    public abstract DocTypeUI DocTypeUI { get; }

    /// <summary>
    /// Идентификикатор табличного просмотра.
    /// На основании идентификатора определяется аргумент isCaller в методе <see cref="ApplyChanges(DataSet, bool)"/>.
    /// </summary>
    public virtual Guid BrowserGuid { get { return Guid.Empty; } }

    /// <summary>
    /// Возвращает имя текущего столбца табличного просмотра или пустую строку, если выбрано несколько столбцов,
    /// или информация недоступна.
    /// </summary>
    public virtual string CurrentColumnName { get { return String.Empty; } }

    #endregion

    #region Обновление после записи документов

    /// <summary>
    /// Обновить просмотр после записи документов.
    /// Метод вызывается <see cref="DBxDocProvider.ApplyChanges(DataSet, bool)"/>, после того, как сервер выполнил запись документов.
    /// </summary>
    /// <param name="dataSet">Набор таблиц документов, полученный от сервера, с учетом изменений, внесенных сервером</param>
    /// <param name="isCaller">true, если идентификатор <see cref="BrowserGuid"/> совпадает с объектом, переданным в <see cref="DocumentEditor"/>,
    /// то есть если редактор был открыт из этого просмотра. В этом случае в просмотр должны быть добавлены новые строки, 
    /// даже если они не проходят условия фильтра</param>
    public virtual void ApplyChanges(DataSet dataSet, bool isCaller)
    {
    }

    /// <summary>
    /// Обновить документы с заданными идентификаторами.
    /// Перед вызовом этого метода из пользовательского кода, должен быть очищен кэш таблицы документа
    /// в <see cref="DocTypeUIBase.TableCache"/>.Clear(<paramref name="docIds"/>).
    /// </summary>
    /// <param name="docIds">Массив идентификаторов документов</param>
    public virtual void UpdateRowsForIds(Int32[] docIds)
    {
    }

    /// <summary>
    /// Свойство временно устанавливается в DocumentEditor на время вызова ApplyChanges
    /// </summary>
    internal static DocumentViewHandler CurrentHandler = null;

    #endregion

    #region Инициализация полей редактора

    /// <summary>
    /// Инициализировать значения нового документа.
    /// На момент вызова документ находится в режиме Insert
    /// Реализация класса может устанавливать значения <paramref name="newDoc"/>.Values или 
    /// добавлять поддокументы.
    /// </summary>
    /// <param name="newDoc">Создаваемый документ</param>
    public virtual void InitNewDocValues(DBxSingleDoc newDoc)
    {
    }

    /// <summary>
    /// Проверить документ перед записью на соответствие внешним требованием
    /// (например, условиям фильтра).
    /// Реализация класса не должна изменять значения полей в <paramref name="savingDoc"/>.
    /// </summary>
    /// <param name="savingDoc">Записываемый документ</param>
    /// <param name="errorMessages">Сюда можно добавить предупреждения о
    /// несоответствии фильтра с помощью <see cref="ErrorMessageList.AddWarning(string)"/>. Может быть
    /// добавлено несколько предупреждений</param>
    public virtual void ValidateDocValues(DBxSingleDoc savingDoc, ErrorMessageList errorMessages)
    {
    }

    /// <summary>
    /// Вспомогательный метод, возвращающий значение поля для нового документа
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение</returns>
    public object GetNewDocValue(string columnName)
    {
      // Создаем псевдодокумент
      DBxDocSet docSet = new DBxDocSet(DocTypeUI.UI.DocProvider);
      DBxSingleDoc doc = docSet[DocTypeUI.DocType.Name].Insert();
      InitNewDocValues(doc);
      return doc.Values[columnName].Value;
    }


    #endregion

    #region Прочие методы

    /// <summary>
    /// Обновляет табличный просмотр, вызывая <see cref="System.Windows.Forms.Control.Invalidate()"/>
    /// </summary>
    public abstract void InvalidateControl();

    #endregion
  }

  /// <summary>
  /// Список объектов <see cref="DocumentViewHandler"/>, реализуемый свойством <see cref="DocTypeUI.Browsers"/>.
  /// </summary>
  public sealed class DocumentViewHandlerList : List<DocumentViewHandler>
  {
    #region Защищенный конструктор

    internal DocumentViewHandlerList(DocTypeUI docTypeUI)
    {
      _DocTypeUI = docTypeUI;
    }

    private DocTypeUI _DocTypeUI;

    #endregion

    #region Обновление после записи документов

    /// <summary>
    /// Обновить документы с заданными идентификаторами.
    /// Перед вызовом этого метода из пользовательского кода, должен быть очищен кэш таблицы документа
    /// в <see cref="DocTypeUIBase.TableCache"/>.Clear(<paramref name="docIds"/>)
    /// </summary>
    /// <param name="docIds">Массив идентификаторов документов</param>
    public void UpdateRowsForIds(Int32[] docIds)
    {
      if (Count == 0)
        return;
      foreach (DocumentViewHandler item in this)
        item.UpdateRowsForIds(docIds);
    }

    #endregion

    #region Прочие методы

    /// <summary>
    /// Вызывает Control.Invalidate() для всех табличных просмотров
    /// </summary>
    public void InvalidateControls()
    {
      if (Count == 0)
        return;
      foreach (DocumentViewHandler item in this)
        item.InvalidateControl();
    }

    /// <summary>
    /// Обновление страниц кэша таблиц и строк табличных просмотров для набора документов.
    /// Предполагается, что набор документов был загружен из базы данных.
    /// Удаленные строки и строки с фиктивными идентификаторами (новые) пропускаются.
    /// Если набор содержит данные для просмотра истории документов, никаких действий не выполняется.
    /// </summary>
    /// <param name="documents">Набор данных</param>
    public void UpdateDBCacheAndRows(DBxDocSet documents)
    {
      // Идентичная реализация есть в DBUI.UpdateDBCacheAndRows()

#if DEBUG
      if (documents == null)
        throw new ArgumentNullException("documents");
#endif

      if (documents.VersionView)
        return;

      documents.DocProvider.UpdateDBCache(documents.DataSet);
      Int32[] docIds = documents[_DocTypeUI.DocType.Name].DocIds;
      if (docIds.Length > 0)
        UpdateRowsForIds(docIds);
      _DocTypeUI.RefreshBufferedData(); // 03.02.2022
    }

    #endregion
  }

  /// <summary>
  /// Стандартная реализация <see cref="DocumentViewHandler"/> для <see cref="EFPDocGridView"/>.
  /// Объект также может быть соединен с пользовательским просмотром, реализующим <see cref="IEFPDocView"/>.
  /// </summary>
  public class StdDocumentViewHandler : DocumentViewHandler
  {
    #region Конструктор и Dispose

    /// <summary>
    /// Создает объект
    /// </summary>
    /// <param name="owner">Провайдер табличного просмотра - владелец</param>
    public StdDocumentViewHandler(IEFPDocView owner)
      : this(owner, owner.DocTypeUI)
    {
    }

    /// <summary>
    /// Создает объект
    /// </summary>
    /// <param name="owner">Провайдер табличного просмотра - владелец</param>
    /// <param name="docTypeName">Имя таблицы вида документа</param>
    public StdDocumentViewHandler(IEFPDBxView owner, string docTypeName)
      : this(owner, owner.UI.DocTypes[docTypeName])
    {
    }

    /// <summary>
    /// Создает объект
    /// </summary>
    /// <param name="owner">Провайдер табличного просмотра - владелец</param>
    /// <param name="docTypeUI">Интерфейс вида документа</param>
    public StdDocumentViewHandler(IEFPDBxView owner, DocTypeUI docTypeUI)
    {
#if DEBUG
      if (owner == null)
        throw new ArgumentNullException("owner");
      if (docTypeUI == null)
        throw new ArgumentNullException("docTypeUI");
#endif
      if (!Object.ReferenceEquals(owner.UI, docTypeUI.UI))
        throw new ArgumentException("Разные ссылки на DBUI");

      _Owner = owner;
      _DocTypeUI = docTypeUI;

      IEFPDocView owner2 = owner as IEFPDocView;
      if (owner2 != null)
      {
        _ExternalEditorCaller = owner2.ExternalEditorCaller;
        _BrowserGuid = owner2.BrowserGuid;
      }
      else
        _BrowserGuid = Guid.NewGuid();
    }

    /// <summary>
    /// Уничтожает объект при закрытии формы
    /// </summary>
    /// <param name="disposing">True, если вызван метод Dispose()</param>
    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      _Owner = null; // разрыв ссылки, чтобы текущий просмотр мог быть удален
      _ExternalEditorCaller = null;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер табличного просмотра - владелец
    /// </summary>
    public IEFPDBxView Owner { get { return _Owner; } }
    private /*readonly*/ IEFPDBxView _Owner;

    private IEFPDocView Owner2 { get { return _Owner as IEFPDocView; } }

    /// <summary>
    /// Интерфейс вида документов
    /// </summary>
    public override DocTypeUI DocTypeUI { get { return _DocTypeUI; } }
    private readonly DocTypeUI _DocTypeUI;


    /// <summary>
    /// Используется при инициализации полей нового документа
    /// </summary>
    public DocumentViewHandler ExternalEditorCaller { get { return _ExternalEditorCaller; } }
    private /*readonly*/ DocumentViewHandler _ExternalEditorCaller;

    /// <summary>
    /// Возвращает EFPDocGridView.BrowserGuid
    /// </summary>
    public override Guid BrowserGuid { get { return _BrowserGuid; } }
    private readonly Guid _BrowserGuid;


    #endregion

    #region Переопределенные свойства

    /// <summary>
    /// Возвращает <see cref="IEFPDataView.CurrentColumnName"/>
    /// </summary>
    public override string CurrentColumnName
    {
      get
      {
        if (Owner == null)
          return String.Empty;
        else
          return Owner.CurrentColumnName;
      }
    }

    #endregion

    #region ApplyChanges()

    /// <summary>
    /// Обновление табличного просмотра
    /// </summary>
    /// <param name="dataSet">Набор данных после отправки на сервер</param>
    /// <param name="isCaller">True, если инициатором записи документов был редактор, открытый из текущего просмотра.</param>
    public override void ApplyChanges(DataSet dataSet, bool isCaller)
    {
      if (Owner == null)
        return;
      if (Owner.SourceAsDataTable == null)
        return; // 04.06.2018

      string docTypeName = DocTypeUI.DocType.Name;

      if (!dataSet.Tables.Contains(docTypeName))
        return; // Нет таблицы

      DataTable srcTable = dataSet.Tables[docTypeName];
      List<DataRow> newSelRows = new List<DataRow>();

      DBxColumns filterColumns = null;
      if (Owner2 != null)
        filterColumns = Owner2.Filters.GetColumnNames();

      // Есть в таблице первичный ключ по полю "Id"?
      bool hasPrimaryKey = String.Compare(DataTools.GetPrimaryKey(_Owner.SourceAsDataTable), "Id", StringComparison.OrdinalIgnoreCase) == 0;

      // 08.07.2016
      // Таблица может не содержать первичного ключа
      DataView dvFind = null;
      if (!hasPrimaryKey)
      {
        dvFind = new DataView(Owner.SourceAsDataTable);
        dvFind.Sort = "Id";
      }
      try
      {
        foreach (DataRow srcRow in srcTable.Rows)
        {
          // 15.07.2020. Алгоритм изменен

          #region Определяем необходимость наличия строки в просмотре

          bool isVisible;
          switch (srcRow.RowState)
          {
            case DataRowState.Added: // нельзя базироваться только на RowState
              if (isCaller)
                isVisible = true; // Не уверен, что надо всегда показывать
              else if (Owner2 == null)
                isVisible = false;
              else
                isVisible = TestFilters(srcRow, filterColumns);
              break;
            case DataRowState.Modified:
              if (Owner2 == null)
                isVisible = true;
              else
                isVisible = TestFilters(srcRow, filterColumns);
              break;
            case DataRowState.Deleted:
              if (Owner2 == null)
                isVisible = true;
              else
              {
                if (Owner2.ShowDeleted)
                  isVisible = TestFilters(srcRow, filterColumns);
                else
                  isVisible = false;
              }
              break;
            default: // Unchanged
              continue;

          }

          #endregion

          #region Определяем существование строки в просмотре

          Int32 docId;
          if (srcRow.RowState == DataRowState.Deleted)
            docId = (Int32)(srcRow["Id", DataRowVersion.Original]);
          else
            docId = (Int32)(srcRow["Id"]);
          DataRow resRow = FindDocRow(docId, dvFind);

          #endregion

          #region Добавление / обновление / удаление строки в просмотре

          if (isVisible)
          {
            if (resRow == null)
            {
              resRow = Owner.SourceAsDataTable.NewRow();
              DataTools.CopyRowValues(srcRow, resRow, true);
              UpdateRefValues(srcRow, resRow);
              Owner.SourceAsDataTable.Rows.Add(resRow);
            }
            else
            {
              DataTools.CopyRowValues(srcRow, resRow, true);
              UpdateRefValues(srcRow, resRow);
              Owner.InvalidateDataRow(resRow); // не Update
            }

            // Перенесено сюда 26.05.2021
            // Может быть несколько просмотров, привязанных к одной кэшированной таблице данных.
            // В этом случае предыдущее условие "ResRow==null" выполняется для первого просмотра,
            // а второй просмотр не будет добавлять дублирующую строку в таблицу.
            // Но позиционировать на новую строку надо в любом случае, если она видна в просмотре.
            if (srcRow.RowState == DataRowState.Added)
              newSelRows.Add(resRow);
          }
          else
          {
            if (resRow != null)
              resRow.Delete();
          }

          #endregion
        }
      }
      finally
      {
        if (dvFind != null)
          dvFind.Dispose();
      }

      if (newSelRows.Count > 0)
      {
        try
        {
          Owner.SelectedDataRows = newSelRows.ToArray();
        }
        catch
        {
          EFPApp.ErrorMessageBox("Не удалось активировать добавленные строки в табличном просмотре");
        }
      }
    }

    private DataRow FindDocRow(Int32 docId, DataView dvFind)
    {
      if (docId == 0)
        return null;

      if (dvFind == null)
        return Owner.SourceAsDataTable.Rows.Find(docId);
      else
      {
        int p = dvFind.Find(docId);
        if (p >= 0)
          return dvFind[p].Row;
        else
          return null;
      }
    }

    private void UpdateRefValues(DataRow srcRow, DataRow resRow)
    {
      // 13.11.2020
      // Строка srcRow может быть помечена на удаление
      if (srcRow.RowState == DataRowState.Deleted)
        return;

      for (int i = 0; i < resRow.Table.Columns.Count; i++)
      {
        string colName = resRow.Table.Columns[i].ColumnName;
        int p = colName.IndexOf('.');
        if (p >= 0)
        {
          string mainColName = colName.Substring(0, p);
          int pCol = srcRow.Table.Columns.IndexOf(mainColName);
          if (pCol >= 0)
          {
            Int32 refId = DataTools.GetInt(srcRow, mainColName); // в ResRow может не быть базового поля
            object refValue = Owner.UI.TextHandlers.DBCache[DocTypeUI.DocType.Name].GetRefValue(colName, refId);
            if (refValue == null)
              resRow[i] = DBNull.Value; // 26.10.2016
            else
              resRow[i] = refValue;
          }
        }
      }
    }

    /// <summary>
    /// Проверяет, проходит ли строка документа из набора DBxDataSet условия фильтра, заданные для просмотра
    /// </summary>
    /// <param name="srcRow">Строка документа для DBxSingleDoc</param>
    /// <param name="filterColumns">Список столбов, которые требуются для проверки установленных фильтров табличного просмотра.
    /// В списке могут быть ссылочные поля</param>
    /// <returns>True, если документ проходит фильтрацию</returns>
    private bool TestFilters(DataRow srcRow, DBxColumns filterColumns)
    {
      if (filterColumns == null)
        return true; // Нечего тестировать

      // 13.11.2020
      // Проверяемая строка может быть помечена на удаление
      DataRowVersion rowVer = srcRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current;

      NamedValues pairs = new NamedValues();
      for (int i = 0; i < filterColumns.Count; i++)
      {
        string colName = filterColumns[i];
        object value;
        int pDot = colName.IndexOf('.');
        if (pDot >= 0)
        {
          string mainColName = colName.Substring(0, pDot);
          int pCol = srcRow.Table.Columns.IndexOf(mainColName);
          if (pCol >= 0)
          {
            Int32 refId = DataTools.GetInt(srcRow[mainColName, rowVer]);
            value = Owner.UI.TextHandlers.DBCache[DocTypeUI.DocType.Name].GetRefValue(colName, refId);
          }
          else
            throw new BugException("Не найдено поле \"" + mainColName + "\"");
        }
        else
          value = srcRow[colName, rowVer];

        if (value is DBNull)
          value = null;
        pairs.Add(colName, value);
      }

      return Owner2.Filters.TestValues(pairs);
    }

    #endregion

    #region Прочие переопределенные методы

    /// <summary>
    /// Возвращает <see cref="Owner"/>.ToString(), если объект присоединен к просмотру
    /// </summary>
    /// <returns>Текстовое представление для табличного просмотра</returns>
    public override string ToString()
    {
      if (Owner == null)
        return "[ Отключен от просмотра ]";
      else
        return Owner.ToString();
    }


    /// <summary>
    /// Вызывает <see cref="IEFPDBxView.UpdateRowsForIds(int[])"/>
    /// </summary>
    /// <param name="docIds">Идентификаторы обновляемых документов</param>
    public override void UpdateRowsForIds(Int32[] docIds)
    {
      if (Owner != null)
        Owner.UpdateRowsForIds(docIds);
    }

    /// <summary>
    /// Инициализация полей нового документа
    /// </summary>
    /// <param name="newDoc">Новый документ</param>
    public override void InitNewDocValues(DBxSingleDoc newDoc)
    {
      if (ExternalEditorCaller == null)
      {
        if (Owner2 != null)
        {
          if (Owner2.Filters != null)
            Owner2.Filters.InitNewDocValues(newDoc);


          // 10.06.2019
          DocTypeUI.InitNewDocGroupIdValue(newDoc, Owner2.AuxFilterGroupIds);
        }
      }
      else
        ExternalEditorCaller.InitNewDocValues(newDoc);
    }

    /// <summary>
    /// Проверка полей документа на соответствие установленным фильтрам.
    /// </summary>
    /// <param name="savingDoc">Проверяемый документ</param>
    /// <param name="errorMessages">Заполняемый список сообщений</param>
    public override void ValidateDocValues(DBxSingleDoc savingDoc, ErrorMessageList errorMessages)
    {
      if (ExternalEditorCaller == null)
      {
        if (Owner2 != null)
        {
          if (Owner2.Filters != null)
            Owner2.Filters.ValidateDocValues(savingDoc, errorMessages);
        }
      }
      else
        ExternalEditorCaller.ValidateDocValues(savingDoc, errorMessages);
    }

    /// <summary>
    /// Вызывает <see cref="System.Windows.Forms.Control.Invalidate()"/>.
    /// </summary>
    public override void InvalidateControl()
    {
      if (Owner != null)
        Owner.Control.Invalidate();
    }

    #endregion
  }
}
