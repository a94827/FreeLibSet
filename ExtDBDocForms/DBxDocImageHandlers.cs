// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using FreeLibSet.Data.Docs;
using FreeLibSet.Data;
using FreeLibSet.Core;
using FreeLibSet.UICore;

namespace FreeLibSet.Forms.Docs
{
  #region DocTypeGetImageEventHandler

  /// <summary>
  /// Причина вызова события получения изображения для документа или поддокумента (свойство <see cref="DBxImageValueNeededEventArgs.Reason"/>).
  /// </summary>
  public enum DBxImageValueNeededReason
  {
    /// <summary>
    /// Получить изображение для документа или поддокумента (вызван <see cref="DocTypeUIBase.GetImageKey(int)"/>)
    /// </summary>
    Image,

    /// <summary>
    /// Получить всплывающую подсказку для ячейки со значком (вызван <see cref="DocTypeUIBase.GetToolTipText(int)"/>)
    /// </summary>
    ToolTipText,

    /// <summary>
    /// Определить цвет строки (вызван <see cref="DocTypeUIBase.GetRowColor(int, EFPDataGridViewRowInfoEventArgs)"/>)
    /// </summary>
    RowColor,
  }

  /// <summary>
  /// Аргументы для события <see cref="DBxDocImageHandlers.TableHandler.ImageValueNeeded"/> получения изображения документа или поддокумента
  /// </summary>
  public class DBxImageValueNeededEventArgs : DBxValueNeededEventArgsBase
  {
    #region Конструктор

    internal DBxImageValueNeededEventArgs()
    {
    }

    #endregion

    #region Методы

    /// <summary>
    /// Этот метод вызывается перед вызовом события для установки значений
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="id"></param>
    /// <param name="columnNames"></param>
    /// <param name="values"></param>
    /// <param name="reason"></param>
    internal void InitData(string tableName, Int32 id, DBxColumns columnNames, object[] values, DBxImageValueNeededReason reason)
    {
      base.InitData(tableName, id, columnNames, values);
      _Reason = reason;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Причина вызова события (изображение, всплывающая подсказка или цветовое оформление)
    /// </summary>
    public DBxImageValueNeededReason Reason { get { return _Reason; } }
    private DBxImageValueNeededReason _Reason;

    /// <summary>
    /// Сюда должно быть помещено имя изображения из <see cref="EFPApp.MainImages"/>
    /// в режиме <see cref="Reason"/>=<see cref="DBxImageValueNeededReason.Image"/>
    /// </summary>
    public string ImageKey
    {
      get { return _ImageKey; }
      set { _ImageKey = value; }
    }
    private string _ImageKey;

    /// <summary>
    /// Сюда должен быть помещен текст всплывающей подсказкм для ячейки изображения
    /// в режиме <see cref="Reason"/>=<see cref="DBxImageValueNeededReason.ToolTipText"/>.
    /// Если свойство не будет установлено обработчиком, используется стандартный
    /// текст подсказки, содержащий описание документа / поддокумента, получаемый
    /// <see cref="DocTypeUIBase.GetTextValue(int)"/>.
    /// </summary>
    public string ToolTipText
    {
      get { return _ToolTipText; }
      set { _ToolTipText = value; }
    }
    private string _ToolTipText;

    /// <summary>
    /// Сюда должен может быть помещен цвет строки в режиме 
    /// <see cref="Reason"/>=<see cref="DBxImageValueNeededReason.RowColor"/>.
    /// </summary>
    public UIDataViewColorType ColorType
    {
      get { return _ColorType; }
      set { _ColorType = value; }
    }
    private UIDataViewColorType _ColorType;

    /// <summary>
    /// Может быть установлено значение true, если требуется пометить строку серым цветом.
    /// Используется в режиме <see cref="Reason"/>=<see cref="DBxImageValueNeededReason.RowColor"/>.
    /// </summary>
    public bool Grayed
    {
      get { return _Grayed; }
      set { _Grayed = value; }
    }
    private bool _Grayed;

    #endregion
  }

  /// <summary>
  /// Делегат для обработчика <see cref="DBxDocImageHandlers.TableHandler.ImageValueNeeded"/> 
  /// </summary>
  /// <param name="sender">Не используется</param>
  /// <param name="args">Аргументы события</param>
  public delegate void DBxImageValueNeededEventHandler(object sender, DBxImageValueNeededEventArgs args);

  #endregion

  /// <summary>
  /// Система получения значков и подсказок документов и поддокументов.
  /// Также позволяет получить раскраску строк табличного просмотра и комбоблоков выбора.
  /// Объект <see cref="DBxDocImageHandlers"/> может использоваться только на стороне клиента.
  /// В части извлечения значений объекты являются потокобезопасными, если используемый <see cref="DBxCache"/>
  /// является потокобезопасным. В процессе установки обработчиков и заполнения полей, 
  /// объект не является безопасным.
  /// </summary>
  public class DBxDocImageHandlers : IReadOnlyObject
  {
    // TODO: 31.05.2022. Выделить из DBxDocTextHandlers базовый класс DBxDocValueHandlersBase, из TableHandler - BaseTableHandler и вынести общий метод DBxDocValueHandlersBase.GetValue(). Но там сложно получается...

    #region Конструктор

    /// <summary>
    /// Объект создается в конструкторе класса DBUI
    /// </summary>
    /// <param name="docTypes"></param>
    /// <param name="dbCache"></param>
    internal DBxDocImageHandlers(DBxDocTypes docTypes, DBxCache dbCache/*, DBxPermissions DBPermissions*/)
    {
      if (docTypes == null)
        throw new ArgumentNullException("docTypes");
      if (dbCache == null)
        throw new ArgumentNullException("dbCache");
      //if (DBPermissions == null)
      //  throw new ArgumentNullException("DBPermissions");

      _DocTypes = docTypes;
      _DBCache = dbCache;
      //FDBPermissions = DBPermissions;

      _TableItems = new Dictionary<string, TableHandler>();
      _Args = new DBxImageValueNeededEventArgs();

      _ColumnsId = new DBxColumns("Id");
      if (docTypes.UseDeleted)
        _ColumnsDoc = new DBxColumns("Id,Deleted");
      else
        _ColumnsDoc = _ColumnsId;

      if (docTypes.UseDeleted)
        _ColumnsSubDoc = new DBxColumns("Id,DocId,Deleted");
      else
        _ColumnsSubDoc = new DBxColumns("Id,DocId");
    }

    #endregion

    #region Списки полей для запроса

    internal DBxColumns ColumnsId { get { return _ColumnsId; } }
    private DBxColumns _ColumnsId;

    internal DBxColumns ColumnsDoc { get { return _ColumnsDoc; } }
    private DBxColumns _ColumnsDoc;

    internal DBxColumns ColumnsSubDoc { get { return _ColumnsSubDoc; } }
    private DBxColumns _ColumnsSubDoc;

    #endregion

    #region Источник данных

    /// <summary>
    /// Описание видов документов.
    /// Задается в конструкторе.
    /// </summary>
    public DBxDocTypes DocTypes { get { return _DocTypes; } }
    private readonly DBxDocTypes _DocTypes;

    /// <summary>
    /// Источник для получения значений полей.
    /// Задается в конструкторе.
    /// Свойство может быть изменено в процессе работы.
    /// </summary>
    public DBxCache DBCache
    {
      get { return _DBCache; }
      set
      {
        if (value == null)
          throw new ArgumentNullException();
        if (object.ReferenceEquals(value, _DBCache))
          return;

        lock (_TableItems)
        {
          _DBCache = value;
          foreach (KeyValuePair<string, TableHandler> pair in _TableItems)
          {
            pair.Value.AccessDeniedFlag = false;
          }
        }
      }
    }
    private DBxCache _DBCache;

    ///// <summary>
    ///// Разрешения на доступ таблицам.
    ///// Если у пользователя нет разрешения на просмотр таблицы или отдельных полей, то
    ///// нельзя использовать обработчик для получения значка
    ///// </summary>
    //public DBxPermissions DBPermissions { get { return FDBPermissions; } }
    //private DBxPermissions FDBPermissions;

    #endregion

    #region Инициализация обработчиков

    /// <summary>
    /// Задать фиксированное изображение для документа или поддокумента без использования события <see cref="TableHandler.ImageValueNeeded"/>.
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="imageKey">Имя изображения в списке <see cref="EFPApp.MainImages"/></param>
    public void Add(string tableName, string imageKey)
    {
      Add(tableName, imageKey, (DBxColumns)null, null);
    }

    /// <summary>
    /// Задать обработчник получения изображения для документа или поддокумента с возможностью задать обработчик события <see cref="TableHandler.ImageValueNeeded"/>.
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="imageKey">Имя основного изображения в списке <see cref="EFPApp.MainImages"/>.
    /// Свойство следует задавать, даже если обработчик <paramref name="imageValueNeeded"/> переопределяет изображение для всех документов.
    /// Основное изображение может использоваться в командах меню и в окне просмотра ссылок для обозначения вида документа в-целом.</param>
    /// <param name="columnNames">Список столбцов (через запятую), которые использует обработчик</param>
    /// <param name="imageValueNeeded">Обработчик, который позволяет получить изображение, раскраску и всплывающую подсказку 
    /// для конкретного документа и поддокумента.
    /// Обработчик должен выполняться быстро, так как вызывается при прорисовке кажой строки табличного просмотра</param>
    public void Add(string tableName, string imageKey, string columnNames, DBxImageValueNeededEventHandler imageValueNeeded)
    {
      DBxColumns columnNames2 = null;
      if (!String.IsNullOrEmpty(columnNames))
        columnNames2 = new DBxColumns(columnNames);
      Add(tableName, imageKey, columnNames2, imageValueNeeded);
    }

    /// <summary>
    /// Задать обработчник получения изображения для документа или поддокумента с возможностью задать обработчик события <see cref="TableHandler.ImageValueNeeded"/>.
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="imageKey">Имя основного изображения в списке <see cref="EFPApp.MainImages"/>.
    /// Свойство следует задавать, даже если обработчик <paramref name="imageValueNeeded"/> переопределяет изображение для всех документов.
    /// Основное изображение может использоваться в командах меню и в окне просмотра ссылок для обозначения вида документа в-целом.</param>
    /// <param name="columnNames">Список столбцов, которые использует обработчик</param>
    /// <param name="imageValueNeeded">Обработчик, который позволяет получить изображение, раскраску и всплывающую подсказку 
    /// для конкретного документа и поддокумента.
    /// Обработчик должен выполняться быстро, так как вызывается при прорисовке кажой строки табличного просмотра</param>
    public void Add(string tableName, string imageKey, DBxColumns columnNames, DBxImageValueNeededEventHandler imageValueNeeded)
    {
      if (String.IsNullOrEmpty(tableName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("tableName");

      //string TableName2 = TableName;

      lock (_TableItems)
      {
        CheckNotReadOnly();

        // Удаляем в случае повторного вызова метода Add
        _TableItems.Remove(tableName);

        //// 10.07.2018
        //// Проверяем права доступа
        //if (ColumnNames != null)
        //{
        //  if (FDBPermissions.TableModes[TableName] == DBxAccessMode.None)
        //  {
        //    TableName2 = "?" + TableName; // пусть выдает значок ошибки
        //    ColumnNames = null;
        //    ImageValueNeeded = null;
        //  }
        //  else
        //  {
        //    // проверяем, что есть доступ ко всем поляем
        //    for (int i = 0; i < ColumnNames.Count; i++)
        //    {
        //      // TODO: Надо проверять имена с точками
        //      if (ColumnNames[i].IndexOf('.') < 0)
        //      {
        //        if (FDBPermissions.ColumnModes[TableName, ColumnNames[i]] == DBxAccessMode.None)
        //        {
        //          ColumnNames = null;
        //          ImageValueNeeded = null;
        //          break;
        //        }
        //      }
        //    }
        //  }
        //}

        TableHandler handler = new TableHandler(this, tableName, imageKey, columnNames, imageValueNeeded);
        _TableItems.Add(tableName, handler);
      }
    }

    #endregion

    #region Получение изображения

    /// <summary>
    /// Используем единственный экземпляр объекта, т.к. при запросе выполняется блокировка
    /// </summary>
    private DBxImageValueNeededEventArgs _Args;

    /// <summary>
    /// Получить имя изображения для документа или поддокумента в списке <see cref="EFPApp.MainImages"/>.
    /// Если <paramref name="id"/>=0, то возвращается "EmptyImage".
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="id">Идентификатор</param>
    /// <returns>Имя изображения</returns>
    public string GetImageKey(string tableName, Int32 id)
    {
      try
      {
        return DoGetImageKey(tableName, id, null, null);
      }
      catch
      {
        return "Error";
      }
    }

    /// <summary>
    /// Получить имя изображения для документа или поддокумента в списке <see cref="EFPApp.MainImages"/>.
    /// Если поле "Id" равно 0, то возвращается "EmptyImage".
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="row">Строка в табличном просмотре. При поиске значений полей она обладает приоритетом,
    /// по сравнению с данными в <see cref="DBxCache"/>.</param>
    /// <returns>Имя изображения</returns>
    public string GetImageKey(string tableName, DataRow row)
    {
      try
      {
        if (row.RowState == DataRowState.Deleted)
          return "Cancel";
        Int32 Id = DataTools.GetInt32(row, "Id");
        return DoGetImageKey(tableName, Id, row.Table.DataSet, row);
      }
      catch
      {
        return "Error";
      }
    }

    private string DoGetImageKey(string tableName, Int32 id, DataSet primaryDS, DataRow row)
    {
      if (id == 0)
        return "EmptyImage";

      lock (_TableItems)
      {
        //if (DocProvider == null)
        //  return "Error"; // источник данных не присоединен

        TableHandler handler;
        if (!_TableItems.TryGetValue(tableName, out handler))
        {
          // добавляем пустышку
          if (String.IsNullOrEmpty(tableName))
            throw ExceptionFactory.ArgStringIsNullOrEmpty("tableName");

          return "Item";
        }


        if (handler.DocType == null)
          // Не найденная таблица. Не нужно ничего запрашивать, даже если есть поля и обработчик
          return "Error";
        if (handler.AccessDeniedFlag)
          return "UnknownState"; // 10.07.2018

        bool fromRow = false;
        if (row != null)
          fromRow = DBxColumns.TableContains(row.Table, handler.QueriedColumnNames);
        object[] values;
        try
        {
          if (fromRow)
            values = handler.QueriedColumnNames.GetRowValues(row);
          else
          {
            if (id < 0)
            {
              values = InternalGetValues(tableName, id, handler.QueriedColumnNames, primaryDS); // 31.05.2022
              if (values == null)
                return "Error"; // 16.06.2021
            }
            else
              values = DBCache[tableName].GetValues(id, handler.QueriedColumnNames, primaryDS); // включая Id,DocId и Delete
          }
        }
        catch (DBxAccessException)
        {
          handler.AccessDeniedFlag = true; // 10.07.2018
          throw;
        }
        _Args.InitData(tableName, id, handler.QueriedColumnNames, values, DBxImageValueNeededReason.Image);

        // Добавляем информацию об удаленном документе
        if (DocTypes.UseDeleted)
        {
          if (handler.SubDocType == null)
          {
            if (_Args.GetBoolean("Deleted"))
              return "Cancel";
          }
          else
          {
            if (_Args.GetBoolean("Deleted"))
              return "Cancel";
            if (GetDocIdDeleted(handler))
              return "Cancel";
          }
        }

        _Args.ImageKey = handler.ImageKey;
        if (handler.ImageValueNeeded != null)
        {
          try
          {
            handler.ImageValueNeeded(this, _Args);
          }
          catch
          {
            _Args.ImageKey = "Error";
          }
        }

        return _Args.ImageKey;
      }
    }

    private bool GetDocIdDeleted(TableHandler handler)
    {
      Int32 docId = _Args.GetInt32("DocId");
      if (docId <= 0)
        return false;

      return _DBCache[handler.DocType.Name].GetBoolean(docId, "Deleted");
    }

    #endregion

    #region InternalGetValues()

    /// <summary>
    /// Извлечение значений из набора <paramref name="primaryDS"/> для строки,
    /// когда нельзя обращаться к <see cref="DBxCache"/> (фиктивный Id)
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="id"></param>
    /// <param name="columnNames"></param>
    /// <param name="primaryDS"></param>
    /// <returns></returns>
    private object[] InternalGetValues(string tableName, Int32 id, DBxColumns columnNames, DataSet primaryDS)
    {
      DataRow row = InternalGetRow(tableName, id, primaryDS);
      if (row == null)
        return null;

      object[] a = new object[columnNames.Count];
      for (int i = 0; i < columnNames.Count; i++)
        a[i] = InternalGetValue(row, tableName, columnNames[i], primaryDS);
      return a;
    }

    private DataRow InternalGetRow(string tableName, Int32 id, DataSet primaryDS)
    {
      if (primaryDS == null)
        return null;

      DataTable table = primaryDS.Tables[tableName];
      if (table == null)
        return null;

      return table.Rows.Find(id);
    }

    private object InternalGetValue(DataRow row, string tableName, string columnName, DataSet primaryDS)
    {
#if DEBUG
      if (String.IsNullOrEmpty(columnName))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("columnName");
#endif
      int p = columnName.IndexOf('.');
      if (p < 0)
      {
        // Простое поле таблицы
        if (row.Table.Columns.Contains(columnName))
          return row[columnName];
        else
          return null;
      }
      else
      {
        string refColumnName = columnName.Substring(0, p);
        Int32 refId = DataTools.GetInt32(InternalGetValue(row, tableName, refColumnName, primaryDS));
        if (refId == 0)
          return null; // пустая ссылка

        DBxTableStruct tableStruct = DBCache[tableName].TableStruct;

        string extTableName = tableStruct.Columns[refColumnName].MasterTableName;
        if (String.IsNullOrEmpty(extTableName))
          throw new ArgumentException(String.Format(Res.Common_Arg_NoRefColumn, extTableName, tableName), "columnName");
        string extColumnName = columnName.Substring(p + 1);

        DataRow row2 = InternalGetRow(extTableName, refId, primaryDS);
        if (row2 == null)
        {
          if (refId > 0)
            return DBCache[extTableName].GetValue(refId, extColumnName);
          else
            return null;
        }
        return InternalGetValue(row2, extTableName, extColumnName, primaryDS);
      }
    }

    #endregion

    #region Получение значка для объекта документа / поддокумента

    /// <summary>
    /// Получить имя изображения для документа в списке <see cref="EFPApp.MainImages"/>.
    /// Поля документа используются для определения изображения. 
    /// Весь набор данных, к которому относится <paramref name="doc"/>, имеет приоритет над данными из <see cref="DBxCache"/>.
    /// </summary>
    /// <param name="doc">Загруженный документ</param>
    /// <returns>Имя изображения</returns>
    public string GetImageKey(DBxSingleDoc doc)
    {
      try
      {
        return DoGetImageKey(doc.DocType.Name, doc.DocId, doc.DocSet.DataSet, null);
      }
      catch
      {
        return "Error";
      }
    }

    /// <summary>
    /// Получить имя изображения для поддокумента в списке <see cref="EFPApp.MainImages"/>.
    /// Поля поддокумента используются для определения изображения. 
    /// Весь набор данных, к которому относится <paramref name="subDoc"/>, имеет приоритет над данными из <see cref="DBxCache"/>.
    /// </summary>
    /// <param name="subDoc">Загруженный поддокумент</param>
    /// <returns>Имя изображения</returns>
    public string GetImageKey(DBxSubDoc subDoc)
    {
      try
      {
        return DoGetImageKey(subDoc.SubDocType.Name, subDoc.SubDocId, subDoc.Doc.DocSet.DataSet, null);
      }
      catch
      {
        return "Error";
      }
    }

    #endregion

    #region Получение значка по идентификатору таблицы документа

    /// <summary>
    /// Получить имя изображения для документа или поддокумента в списке <see cref="EFPApp.MainImages"/>.
    /// Если <paramref name="tableId"/>=0 или <paramref name="docId"/>=0, то возвращается "EmptyImage".
    /// </summary>
    /// <param name="tableId">Идентификатор таблицы документа</param>
    /// <param name="docId">Идентификатор документа</param>
    /// <returns>Имя изображения</returns>
    public string GetImageKey(Int32 tableId, Int32 docId)
    {
      try
      {
        if (tableId == 0 || docId == 0)
          return "EmptyImage";

        string tableName = DocTypes.GetTableNameById(tableId);
        return DoGetImageKey(tableName, docId, null, null);
      }
      catch
      {
        return "Error";
      }
    }

    #endregion

    #region Получения значка без обращения к обработчику

    private enum ImageKind { AsIs, SingleDoc, Table }

    /// <summary>
    /// Получить имя изображения из списка <see cref="EFPApp.MainImages"/> для таблицы документов или поддокументов в целом,
    /// а не для конкретной записи в таблице.
    /// Возвращает свойство <see cref="DBxDocImageHandlers.TableHandler.ImageKey"/>, если оно установлено.
    /// Если изображение не задано, то возвращается пустая строка. В этом случае вызывающий код должен показать какой-нибудь стандартный значок.
    /// Обычно используются методы <see cref="GetSingleDocImageKey(string)"/> или <see cref="GetTableImageKey(string)"/>.
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <returns>Имя изображения</returns>
    public string GetImageKey(string tableName)
    {
      return DoGetImageKey(tableName, ImageKind.AsIs);
    }

    /// <summary>
    /// Получить имя изображения из списка <see cref="EFPApp.MainImages"/> для таблицы документов или поддокументов в целом,
    /// а не для конкретной записи в таблице. 
    /// Возвращает свойство <see cref="DBxDocImageHandlers.TableHandler.ImageKey"/>, если оно установлено.
    /// Событие <see cref="DBxDocImageHandlers.TableHandler.ImageValueNeeded"/> не вызывается.
    /// Если изображение не задано, то возвращается "Item".
    /// Этот метод используется при прорисовке таблицы документов/поддокументов или выбранного элемента в комбоблоке.
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <returns>Имя изображения</returns>
    public string GetSingleDocImageKey(string tableName)
    {
      return DoGetImageKey(tableName, ImageKind.SingleDoc);
    }

    /// <summary>
    /// Получить имя изображения из списка <see cref="EFPApp.MainImages"/> для таблицы документов или поддокументов в целом,
    /// а не для конкретной записи в таблице.
    /// Возвращает свойство <see cref="DBxDocImageHandlers.TableHandler.ImageKey"/>, если оно установлено.
    /// Событие <see cref="DBxDocImageHandlers.TableHandler.ImageValueNeeded"/> не вызывается.
    /// Если изображение не задано, то возвращается "Table".
    /// Этот метод используется для получения значка для окна справочника а также для команд меню,
    /// которые открывают справочник.
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <returns>Имя изображения</returns>
    public string GetTableImageKey(string tableName)
    {
      return DoGetImageKey(tableName, ImageKind.Table);
    }

    private string DoGetImageKey(string tableName, ImageKind kind)
    {
      try
      {
        lock (_TableItems)
        {
          TableHandler handler;
          if (!_TableItems.TryGetValue(tableName, out handler))
          {
            // добавляем пустышку
            if (String.IsNullOrEmpty(tableName))
              throw ExceptionFactory.ArgStringIsNullOrEmpty("tableName");

            return GetDummyImageKey(kind);
          }

          if (String.IsNullOrEmpty(handler.ImageKey))
            return GetDummyImageKey(kind);
          else
            return handler.ImageKey;
        }
      }
      catch
      {
        return "Error";
      }
    }
    private static string GetDummyImageKey(ImageKind kind)
    {
      switch (kind)
      {
        case ImageKind.Table: return "Table";
        case ImageKind.SingleDoc: return "Item";
        case ImageKind.AsIs: return String.Empty;
        default: throw new ArgumentException();
      }
    }

    #endregion

    #region Получение цвета строки документа / поддокумента

    /// <summary>
    /// Получить раскраску строки для документа или поддокумента.
    /// Для удаленных документов или поддокументов устанавливается признак <paramref name="grayed"/>=true.
    /// После этого вызывается обработчик <see cref="TableHandler.ImageValueNeeded"/>, если он установлен.
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="id">Идентификатор</param>
    /// <param name="colorType">Сюда записывается цветовое оформление строки</param>
    /// <param name="grayed">Сюда записывается true, если строка должна быть выделена серым цветом</param>
    public void GetRowColor(string tableName, Int32 id, out UIDataViewColorType colorType, out bool grayed)
    {
      try
      {
        DoGetRowColor(tableName, id, null, null, out colorType, out grayed);
      }
      catch
      {
        colorType = UIDataViewColorType.Error;
        grayed = true;
      }
    }

    private void DoGetRowColor(string tableName, Int32 id, DataSet primaryDS, DataRow row, out UIDataViewColorType colorType, out bool grayed)
    {
      colorType = UIDataViewColorType.Normal;
      grayed = false;

      if (id == 0)
        return;

      lock (_TableItems)
      {
        //if (DocProvider == null)
        //  return; // источник данных не присоединен

        TableHandler handler;
        if (!_TableItems.TryGetValue(tableName, out handler))
        {
          // добавляем пустышку
          if (String.IsNullOrEmpty(tableName))
            throw ExceptionFactory.ArgStringIsNullOrEmpty("tableName");

          return;
        }


        if (handler.DocType == null)
          // Не найденная таблица. Не нужно ничего запрашивать, даже если есть поля и обработчик
          return;

        bool fromRow = false;
        if (row != null)
          fromRow = DBxColumns.TableContains(row.Table, handler.QueriedColumnNames);
        object[] values;
        if (fromRow)
          values = handler.QueriedColumnNames.GetRowValues(row);
        else
        {
          if (id < 0)
          {
            values = InternalGetValues(tableName, id, handler.QueriedColumnNames, primaryDS); // 31.05.2022
            if (values == null)
              return; // 16.06.2021
          }
          else
            values = DBCache[tableName].GetValues(id, handler.QueriedColumnNames, primaryDS); // включая Id,DocId и Delete
        }

        _Args.InitData(tableName, id, handler.QueriedColumnNames, values, DBxImageValueNeededReason.RowColor);

        // Добавляем информацию об удаленном документе
        if (DocTypes.UseDeleted)
        {
          // 24.11.2017
          // Вызываем пользовательский обработчик и для удаленного документа
          if (handler.SubDocType == null)
          {
            if (_Args.GetBoolean("Deleted"))
            {
              grayed = true;
              // return;
            }
          }
          else
          {
            if (_Args.GetBoolean("Deleted") || GetDocIdDeleted(handler))
            {
              grayed = true;
              //return;
            }
          }
        }

        _Args.ColorType = colorType;
        _Args.Grayed = grayed;
        if (handler.ImageValueNeeded != null)
        {
          try
          {
            handler.ImageValueNeeded(this, _Args);
          }
          catch
          {
            _Args.ColorType = UIDataViewColorType.Error;
          }
        }

        colorType = _Args.ColorType;
        grayed = _Args.Grayed;
      }
    }

    /// <summary>
    /// Получить раскраску строки для документа или поддокумента.
    /// Для удаленных документов или поддокументов устанавливается признак <paramref name="grayed"/>=true.
    /// После этого вызывается обработчик <see cref="TableHandler.ImageValueNeeded"/>, если он установлен.
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="row">Строка в табличном просмотре. При поиске значений полей она обладает приоритетом,
    /// по сравнению с DBCache</param>
    /// <param name="colorType">Сюда записывается цветовое оформление строки</param>
    /// <param name="grayed">Сюда записывается true, если строка должна быть выделена серым цветом</param>
    public void GetRowColor(string tableName, DataRow row, out UIDataViewColorType colorType, out bool grayed)
    {
      try
      {
        if (row.RowState == DataRowState.Deleted)
        {
          colorType = UIDataViewColorType.Normal;
          grayed = true;
          return;
        }

        Int32 id = DataTools.GetInt32(row, "Id");
        DoGetRowColor(tableName, id, row.Table.DataSet, row, out colorType, out grayed);
      }
      catch
      {
        colorType = UIDataViewColorType.Error;
        grayed = true;
      }
    }

    /// <summary>
    /// Получить раскраску строки для документа.
    /// Поля документа используются для определения раскраски. 
    /// Весь набор данных <see cref="DBxDocSet"/>, к которому относится <paramref name="doc"/>, имеет приоритет над данными из <see cref="DBxCache"/>.
    /// Для удаленных документов устанавливается признак <paramref name="grayed"/>=true.
    /// После этого вызывается обработчик <see cref="TableHandler.ImageValueNeeded"/>, если он установлен.
    /// </summary>
    /// <param name="doc">Загруженный документ</param>
    /// <param name="colorType">Сюда записывается цветовое оформление строки</param>
    /// <param name="grayed">Сюда записывается true, если строка должна быть выделена серым цветом</param>
    public void GetRowColor(DBxSingleDoc doc, out UIDataViewColorType colorType, out bool grayed)
    {
      try
      {
        DoGetRowColor(doc.DocType.Name, doc.DocId, doc.DocSet.DataSet, null, out colorType, out grayed);
      }
      catch
      {
        colorType = UIDataViewColorType.Error;
        grayed = false;
      }
    }

    /// <summary>
    /// Получить раскраску строки для поддокумента.
    /// Поля поддокумента используются для определения раскраски. 
    /// Весь набор данных <see cref="DBxDocSet"/>, к которому относится <paramref name="subDoc"/>, имеет приоритет над данными из <see cref="DBxCache"/>.
    /// Для удаленных поддокументов устанавливается признак <paramref name="grayed"/>=true.
    /// После этого вызывается обработчик <see cref="TableHandler.ImageValueNeeded"/>, если он установлен.
    /// </summary>
    /// <param name="subDoc">Загруженный поддокумент</param>
    /// <param name="colorType">Сюда записывается цветовое оформление строки</param>
    /// <param name="grayed">Сюда записывается true, если строка должна быть выделена серым цветом</param>
    public void GetRowColor(DBxSubDoc subDoc, out UIDataViewColorType colorType, out bool grayed)
    {
      try
      {
        DoGetRowColor(subDoc.SubDocType.Name, subDoc.SubDocId, subDoc.Doc.DocSet.DataSet, null, out colorType, out grayed);
      }
      catch
      {
        colorType = UIDataViewColorType.Error;
        grayed = false;
      }
    }

    #endregion

    #region Получение всплывающей подсказки

    /// <summary>
    /// Получить всплывающую подсказку для документа или поддокумента.
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="id">Идентификатор</param>
    /// <returns>Текст всплывающей подсказки</returns>
    public string GetToolTipText(string tableName, Int32 id)
    {
      try
      {
        return DoGetToolTipText(tableName, id, null, null);
      }
      catch (Exception e)
      {
        return String.Format(Res.DBxDocImageHandler_Err_GetToolTipText, e.Message);
      }
    }

    /// <summary>
    /// Получить всплывающую подсказку для документа или поддокумента.
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="row">Строка в табличном просмотре. При поиске значений полей она обладает приоритетом над данными из <see cref="DBxCache"/>.</param>
    /// <returns>Текст всплывающей подсказки</returns>
    public string GetToolTipText(string tableName, DataRow row)
    {
      try
      {
        if (row.RowState == DataRowState.Deleted)
          return Res.DBxDocImageHandler_Msg_RowDeleted;
        Int32 id = DataTools.GetInt32(row, "Id");
        return DoGetToolTipText(tableName, id, row.Table.DataSet, row);
      }
      catch (Exception e)
      {
        return String.Format(Res.DBxDocImageHandler_Err_GetToolTipText, e.Message);
      }
    }

    private string DoGetToolTipText(string tableName, Int32 id, DataSet primaryDS, DataRow row)
    {
      if (id == 0)
        return String.Empty;

      lock (_TableItems)
      {
        //if (DocProvider == null)
        //  return "Источник данных не присоединен";

        TableHandler handler;
        if (!_TableItems.TryGetValue(tableName, out handler))
        {
          // добавляем пустышку
          if (String.IsNullOrEmpty(tableName))
            throw ExceptionFactory.ArgStringIsNullOrEmpty("tableName");

          return String.Empty;
        }

        if (handler.DocType == null)
          // Не найденная таблица. Не нужно ничего запрашивать, даже если есть поля и обработчик
          return Res.DBxDocImageHandler_Msg_TableNotFound;
        if (handler.AccessDeniedFlag)
          return Res.DBxDocImageHandler_Msg_AccessDenied; // 10.07.2018

        bool fromRow = false;
        if (row != null)
          fromRow = DBxColumns.TableContains(row.Table, handler.QueriedColumnNames);
        object[] values;
        try
        {
          if (fromRow)
            values = handler.QueriedColumnNames.GetRowValues(row);
          else
          {
            if (id < 0)
            {
              values = InternalGetValues(tableName, id, handler.QueriedColumnNames, primaryDS); // 31.05.2022
              if (values == null)
                return Res.DBxDocImageHandler_Msg_FictiveId; // 16.06.2021
            }
            else
              values = DBCache[tableName].GetValues(id, handler.QueriedColumnNames, primaryDS); // включая Id,DocId и Delete
          }
        }
        catch (DBxAccessException)
        {
          handler.AccessDeniedFlag = true; // 10.07.2018
          throw;
        }

        _Args.InitData(tableName, id, handler.QueriedColumnNames, values, DBxImageValueNeededReason.ToolTipText);

        // Добавляем информацию об удаленном документе
        if (DocTypes.UseDeleted)
        {
          if (handler.SubDocType == null)
          {
            if (_Args.GetBoolean("Deleted"))
              return Res.DBxDocImageHandler_Msg_DocDeleted;
          }
          else
          {
            if (_Args.GetBoolean("Deleted"))
              return Res.DBxDocImageHandler_Msg_SubDocDeleted;
            if (GetDocIdDeleted(handler))
              return Res.DBxDocImageHandler_Msg_MainDocDeleted;
          }
        }

        _Args.ToolTipText = String.Empty;
        if (handler.ImageValueNeeded != null)
        {
          try
          {
            handler.ImageValueNeeded(this, _Args);
          }
          catch (Exception e)
          {
            _Args.ToolTipText = String.Format(Res.DBxDocImageHandler_Err_GetToolTipText, e.Message);
          }
        }

        return _Args.ToolTipText;
      }
    }


    /// <summary>
    /// Получить всплывающую подсказку для документа.
    /// Поля документа используются для определения подсказки. 
    /// Весь набор данных <see cref="DBxDocSet"/>, к которому относится <paramref name="doc"/>, имеет приоритет над над данными из <see cref="DBxCache"/>.
    /// </summary>
    /// <param name="doc">Загруженный документ</param>
    /// <returns>Текст всплывающей подсказки</returns>
    public string GetToolTipText(DBxSingleDoc doc)
    {
      try
      {
        return DoGetToolTipText(doc.DocType.Name, doc.DocId, doc.DocSet.DataSet, null);
      }
      catch (Exception e)
      {
        return String.Format(Res.DBxDocImageHandler_Err_GetToolTipText, e.Message);
      }
    }

    /// <summary>
    /// Получить всплывающую подсказку для поддокумента.
    /// Поля поддокумента используются для определения подсказки. 
    /// Весь набор данных <see cref="DBxDocSet"/> , к которому относится <paramref name="subDoc"/>, имеет приоритет над данными из <see cref="DBxCache"/>.
    /// </summary>
    /// <param name="subDoc">Загруженный поддокумент</param>
    /// <returns>Текст всплывающей подсказки</returns>
    public string GetToolTipText(DBxSubDoc subDoc)
    {
      try
      {
        return DoGetToolTipText(subDoc.SubDocType.Name, subDoc.SubDocId, subDoc.Doc.DocSet.DataSet, null);
      }
      catch (Exception e)
      {
        return String.Format(Res.DBxDocImageHandler_Err_GetToolTipText, e.Message);
      }
    }

    /// <summary>
    /// Получить всплывающую подсказку для документа.
    /// </summary>
    /// <param name="tableId">Идентификатор таблицы документа <see cref="DBxDocType.TableId"/></param>
    /// <param name="docId">Идентификатор документа</param>
    /// <returns>Текст всплывающей подсказки</returns>
    public string GetToolTipText(Int32 tableId, Int32 docId)
    {
      try
      {
        if (tableId == 0 || docId == 0)
          return String.Empty;
        string tableName = DocTypes.GetTableNameById(tableId);
        return DoGetToolTipText(tableName, docId, null, null);
      }
      catch (Exception e)
      {
        return String.Format(Res.DBxDocImageHandler_Err_GetToolTipText, e.Message);
      }
    }

    #endregion

    #region Внутренний доступ к таблицам

    private class TableHandler
    {
      #region Конструктор

      public TableHandler(DBxDocImageHandlers owner, string tableName, string imageKey, DBxColumns columnNames, DBxImageValueNeededEventHandler imageValueNeeded)
      {
        _TableName = tableName;
        _ImageKey = imageKey;
        _ColumnNames = columnNames;
        _ImageValueNeeded = imageValueNeeded;

        owner.DocTypes.FindByTableName(tableName, out _DocType, out _SubDocType);

        if (DocType != null)
        {
          if (SubDocType == null)
            _QueriedColumnNames = columnNames + owner.ColumnsDoc;
          else
            _QueriedColumnNames = columnNames + owner.ColumnsSubDoc;
        }
        else
        {
          _QueriedColumnNames = owner.ColumnsId;
        }
      }

      #endregion

      #region Свойства, задаваемые в конструкторе

      public string TableName { get { return _TableName; } }
      private readonly string _TableName;

      public string ImageKey { get { return _ImageKey; } }
      private readonly string _ImageKey;

      /// <summary>
      /// Список полей, заданных пользователем
      /// </summary>
      public DBxColumns ColumnNames { get { return _ColumnNames; } }
      private readonly DBxColumns _ColumnNames;

      public DBxImageValueNeededEventHandler ImageValueNeeded { get { return _ImageValueNeeded; } }
      private readonly DBxImageValueNeededEventHandler _ImageValueNeeded;

      public override string ToString()
      {
        return TableName;
      }

      /// <summary>
      /// Вид документа, к которому относится таблица (если найдено)
      /// </summary>
      public DBxDocType DocType { get { return _DocType; } }
      private readonly DBxDocType _DocType;

      /// <summary>
      /// Вид поддокумента, к которому относится таблица, или null, если таблица относится к документу
      /// </summary>
      public DBxSubDocType SubDocType { get { return _SubDocType; } }
      private readonly DBxSubDocType _SubDocType;

      /// <summary>
      /// Список полей, используемых для запросов.
      /// В начале идут поля из <see cref="ColumnNames"/>, затем - "Id", "DocId" и "Deleted"
      /// Если null, значит объект TableHandler еще не был инициализирован для DocProvider
      /// </summary>
      public DBxColumns QueriedColumnNames { get { return _QueriedColumnNames; } }
      private readonly DBxColumns _QueriedColumnNames;

      #endregion

      #region Флажок доступа

      /// <summary>
      /// true, если при попытке доступа к полям возникло исключение DBxAccessException.
      /// Предотврашает повторное обращение к серверу за кэшем страницы, чтобы избежать
      /// перегрузку канала связи
      /// </summary>
      public bool AccessDeniedFlag;

      #endregion
    }

    private Dictionary<string, TableHandler> _TableItems;

    #endregion

    #region Получение дополнительных сведений

    /// <summary>
    /// Получить список полей, из которых собирается значение.
    /// Если список полей не был задан в явном виде, то при <paramref name="forQuery"/>=false возвращается null.
    /// При <paramref name="forQuery"/>=true возвращается минимально необходимый список полей.
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="forQuery">Если true, то возвращается расширенный список, который используется в запросах.
    /// Если false - то только те поля, которые были заданы пользователем в методе <see cref="Add(string, string, DBxColumns, DBxImageValueNeededEventHandler)"/></param>
    /// <returns>Список полей</returns>
    public DBxColumns GetColumnNames(string tableName, bool forQuery)
    {
      lock (_TableItems)
      {
        TableHandler handler;
        if (_TableItems.TryGetValue(tableName, out handler))
        {
          if (forQuery)
            return handler.QueriedColumnNames;
          else
            return handler.ColumnNames;
        }
        else
        {
          if (forQuery)
          {
            DBxDocType dt;
            DBxSubDocType sdt;
            DocTypes.FindByTableName(tableName, out dt, out sdt);

            if (dt != null)
            {
              if (sdt == null)
                return ColumnsDoc;
              else
                return ColumnsSubDoc;
            }
            else
              return ColumnsId;
          }
          else
            return null;
        }
      }
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Возвращает true, если список обработчиков документов был переведен в режим "только чтение".
    /// Не имеет отношения к правам пользователя на доступ к базе данных.
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// Генерирует исключение при <see cref="IsReadOnly"/>=true.
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (_IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    /// <summary>
    /// Переводит список обработчиков в режим "только чтения".
    /// Вызывается из метода DBUI.EndInit(), а не из пользовательского кода
    /// </summary>
    internal void SetReadOnly()
    {
      _IsReadOnly = true;
    }

    #endregion
  }
}
