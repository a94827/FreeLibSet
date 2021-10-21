using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using FreeLibSet.Data.Docs;
using FreeLibSet.Data;
using FreeLibSet.Core;

/*
 * The BSD License
 * 
 * Copyright (c) 2015, Ageyev A.V.
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

namespace FreeLibSet.Forms.Docs
{
  #region DocTypeGetImageEventHandler

  /// <summary>
  /// Причина вызова события Doc/SubDocTypeUI.GetImage
  /// </summary>
  public enum DBxImageValueNeededReason
  {
    /// <summary>
    /// Получить изображение для документа или поддокумента
    /// </summary>
    Image,

    /// <summary>
    /// Получить всплывающую подсказку для ячейки со значком
    /// </summary>
    ToolTipText,

    /// <summary>
    /// Определить цвет строки
    /// </summary>
    RowColor,
  }

  /// <summary>
  /// Аргументы для события получения изображения документа или поддокумента
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
    /// Причина вызова события (изображение или всплывающая подсказка)
    /// </summary>
    public DBxImageValueNeededReason Reason { get { return _Reason; } }
    private DBxImageValueNeededReason _Reason;

    /// <summary>
    /// Сюда должно быть помещено имя изображения из EFPApp.MainImages
    /// в режиме Reason=Image
    /// </summary>
    public string ImageKey
    {
      get { return _ImageKey; }
      set { _ImageKey = value; }
    }
    private string _ImageKey;

    /// <summary>
    /// Сюда должен быть помещен текст всплывающей подсказкм для ячейки изображения
    /// в режиме Reason=ToolTipText
    /// Если свойство не будет установлено обработчиком, используется стандартный
    /// текст подсказки, содержащий описание документа / поддокумента, получаемый
    /// GetTextValue()
    /// </summary>
    public string ToolTipText
    {
      get { return _ToolTipText; }
      set { _ToolTipText = value; }
    }
    private string _ToolTipText;

    /// <summary>
    /// Сюда должен может быть помещен цвет строки в режиме Reason=RowColor
    /// </summary>
    public EFPDataGridViewColorType ColorType
    {
      get { return _ColorType; }
      set { _ColorType = value; }
    }
    private EFPDataGridViewColorType _ColorType;

    /// <summary>
    /// Может быть установлено значение true, если требуется пометить строку сервм цветом
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
  /// Делегат для обработчика DBxDocImageHandlers.TableHandler.ImageValueNeeded
  /// </summary>
  /// <param name="sender">Не используется</param>
  /// <param name="args">Аргументы события</param>
  public delegate void DBxImageValueNeededEventHandler(object sender, DBxImageValueNeededEventArgs args);

  #endregion

  /// <summary>
  /// Система получения значков и подсказок документов и поддокументов.
  /// Также позволяет получить раскраску строк табличного просмотра и комбоблоков выбора.
  /// Объект DBxDocImageHandlers может использоваться только на стороне клиента.
  /// В части извлечения значений объекты являются потокобезопасными, если используемый DBxCache 
  /// является потокобезопасным. В процессе установки обработчиков и заполнения полей, 
  /// объект не является безопасным.
  /// </summary>
  public class DBxDocImageHandlers : IReadOnlyObject
  {
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
    /// Задается в конструкторе
    /// </summary>
    public DBxDocTypes DocTypes { get { return _DocTypes; } }
    private DBxDocTypes _DocTypes;

    /// <summary>
    /// Источник для получения значений полей.
    /// Задается в конструторе.
    /// Свойство может быть изменено в процессе работы
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
          foreach (KeyValuePair<string, TableHandler> Pair in _TableItems)
          {
            Pair.Value.AccessDeniedFlag = false;
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
    /// Задать фиксированное изображение для документа или поддокумента
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="imageKey">Имя изображения в списке EFPApp.MainImages</param>
    public void Add(string tableName, string imageKey)
    {
      Add(tableName, imageKey, (DBxColumns)null, null);
    }

    /// <summary>
    /// Задать обработчник получения изображения для документа или поддокумента
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="imageKey">Имя основного изображения в списке EFPApp.MainImages</param>
    /// <param name="columnNames">Список столбцов (через запятую), которые использует обработчик</param>
    /// <param name="imageValueNeeded">Обработчик, который позволяет получить изображение, раскраску и всплывающую подсказку 
    /// для конкретного документа и поддокумента.
    /// Обработчик должен выполняться быстро, так как вызывается при прорисовке кажой строки табличного просмотра</param>
    public void Add(string tableName, string imageKey, string columnNames, DBxImageValueNeededEventHandler imageValueNeeded)
    {
      DBxColumns ColumnNames2 = null;
      if (!String.IsNullOrEmpty(columnNames))
        ColumnNames2 = new DBxColumns(columnNames);
      Add(tableName, imageKey, ColumnNames2, imageValueNeeded);
    }

    /// <summary>
    /// Задать обработчник получения изображения для документа или поддокумента
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="ImageKey">Имя основного изображения в списке EFPApp.MainImages</param>
    /// <param name="columnNames">Список столбцов, которые использует обработчик</param>
    /// <param name="imageValueNeeded">Обработчик, который позволяет получить изображение, раскраску и всплывающую подсказку 
    /// для конкретного документа и поддокумента.
    /// Обработчик должен выполняться быстро, так как вызывается при прорисовке кажой строки табличного просмотра</param>
    public void Add(string tableName, string ImageKey, DBxColumns columnNames, DBxImageValueNeededEventHandler imageValueNeeded)
    {
      if (String.IsNullOrEmpty(tableName))
        throw new ArgumentNullException("tableName");

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

        TableHandler Handler = new TableHandler(this, tableName, ImageKey, columnNames, imageValueNeeded);
        _TableItems.Add(tableName, Handler);
      }
    }

    #endregion

    #region Получение изображения

    /// <summary>
    /// Используем единственный экземпляр объекта, т.к. при запросе выполняется блокировка
    /// </summary>
    private DBxImageValueNeededEventArgs _Args;

    /// <summary>
    /// Получить имя изображения для документа или поддокумента в списке EFPApp.MainImages.
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
    /// Получить имя изображения для документа или поддокумента в списке EFPApp.MainImages.
    /// Если поле "Id" равно 0, то возвращается "EmptyImage".
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="row">Строка в табличном просмотре. При поиске значений полей она обладает приоритетом,
    /// по сравнению с DBCache</param>
    /// <returns>Имя изображения</returns>
    public string GetImageKey(string tableName, DataRow row)
    {
      try
      {
        if (row.RowState == DataRowState.Deleted)
          return "Cancel";
        Int32 Id = DataTools.GetInt(row, "Id");
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

        TableHandler Handler;
        if (!_TableItems.TryGetValue(tableName, out Handler))
        {
          // добавляем пустышку
          if (String.IsNullOrEmpty(tableName))
            throw new ArgumentNullException("tableName");

          return "Item";
        }


        if (Handler.DocType == null)
          // Не найденная таблица. Не нужно ничего запрашивать, даже если есть поля и обработчик
          return "Error";
        if (Handler.AccessDeniedFlag)
          return "UnknownState"; // 10.07.2018

        bool FromRow = false;
        if (row != null)
          FromRow = DBxColumns.TableContains(row.Table, Handler.QueriedColumnNames);
        object[] Values;
        try
        {
          if (FromRow)
            Values = Handler.QueriedColumnNames.GetRowValues(row);
          else
          {
            if (id < 0)
              return "Error"; // 16.06.2021
            Values = DBCache[tableName].GetValues(id, Handler.QueriedColumnNames, primaryDS); // включая Id,DocId и Delete
          }
        }
        catch (DBxAccessException)
        {
          Handler.AccessDeniedFlag = true; // 10.07.2018
          throw;
        }
        _Args.InitData(tableName, id, Handler.QueriedColumnNames, Values, DBxImageValueNeededReason.Image);

        // Добавляем информацию об удаленном документе
        if (DocTypes.UseDeleted)
        {
          if (Handler.SubDocType == null)
          {
            if (_Args.GetBool("Deleted"))
              return "Cancel";
          }
          else
          {
            if (_Args.GetBool("Deleted"))
              return "Cancel";
            if (GetDocIdDeleted(Handler))
              return "Cancel";
          }
        }

        _Args.ImageKey = Handler.ImageKey;
        if (Handler.ImageValueNeeded != null)
        {
          try
          {
            Handler.ImageValueNeeded(this, _Args);
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
      Int32 docId = _Args.GetInt("DocId");
      if (docId <= 0)
        return false;

      return _DBCache[handler.DocType.Name].GetBool(docId, "Deleted");
    }

    #endregion

    #region Получение значка для объекта документа / поддокумента

    /// <summary>
    /// Получить имя изображения для документа в списке EFPApp.MainImages.
    /// Поля документа используются для определения изображения. 
    /// Весь набор данных, к которому относится <paramref name="doc"/>, имеет приоритет над DBCache.
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
    /// Получить имя изображения для поддокумента в списке EFPApp.MainImages.
    /// Поля поддокумента используются для определения изображения. 
    /// Весь набор данных, к которому относится <paramref name="subDoc"/>, имеет приоритет над DBCache.
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
    /// Получить имя изображения для документа или поддокумента в списке EFPApp.MainImages.
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
    /// Получить имя изображения из списка EFPApp.MainImages для таблицы документов или поддокументов в целом,
    /// а не для конкретной записи в таблице.
    /// Возвращает свойство DBxDocImageHandlers.TableHandler.ImageKey, если оно установлено.
    /// Если изображение не задано, то возвращается пустая строка
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <returns>Имя изображения</returns>
    public string GetImageKey(string tableName)
    {
      return DoGetImageKey(tableName, ImageKind.AsIs);
    }

    /// <summary>
    /// Получить имя изображения из списка EFPApp.MainImages для таблицы документов или поддокументов в целом,
    /// а не для конкретной записи в таблице.
    /// Возвращает свойство DBxDocImageHandlers.TableHandler.ImageKey, если оно установлено.
    /// Если изображение не задано, то возвращается "Item".
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <returns>Имя изображения</returns>
    public string GetSingleDocImageKey(string tableName)
    {
      return DoGetImageKey(tableName, ImageKind.SingleDoc);
    }

    /// <summary>
    /// Получить имя изображения из списка EFPApp.MainImages для таблицы документов или поддокументов в целом,
    /// а не для конкретной записи в таблице.
    /// Возвращает свойство DBxDocImageHandlers.TableHandler.ImageKey, если оно установлено.
    /// Если изображение не задано, то возвращается "Table".
    /// Этот метод используется для получения значка для окна справочника а также для команд меню,
    /// которые открывают справочник
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
          TableHandler Handler;
          if (!_TableItems.TryGetValue(tableName, out Handler))
          {
            // добавляем пустышку
            if (String.IsNullOrEmpty(tableName))
              throw new ArgumentNullException("tableName");

            return GetDummyImageKey(kind);
          }

          if (String.IsNullOrEmpty(Handler.ImageKey))
            return GetDummyImageKey(kind);
          else
            return Handler.ImageKey;
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
    /// Получить раскраску строки для документа или поддокумента
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="id">Идентификатор</param>
    /// <param name="colorType">Сюда записывается цветовое оформление строки</param>
    /// <param name="grayed">Сюда записывается true, если строка должна быть выделена серым цветом</param>
    public void GetRowColor(string tableName, Int32 id, out EFPDataGridViewColorType colorType, out bool grayed)
    {
      try
      {
        DoGetRowColor(tableName, id, null, null, out colorType, out grayed);
      }
      catch
      {
        colorType = EFPDataGridViewColorType.Error;
        grayed = true;
      }
    }

    private void DoGetRowColor(string tableName, Int32 id, DataSet primaryDS, DataRow row, out EFPDataGridViewColorType colorType, out bool grayed)
    {
      colorType = EFPDataGridViewColorType.Normal;
      grayed = false;

      if (id == 0)
        return;

      lock (_TableItems)
      {
        //if (DocProvider == null)
        //  return; // источник данных не присоединен

        TableHandler Handler;
        if (!_TableItems.TryGetValue(tableName, out Handler))
        {
          // добавляем пустышку
          if (String.IsNullOrEmpty(tableName))
            throw new ArgumentNullException("tableName");

          return;
        }


        if (Handler.DocType == null)
          // Не найденная таблица. Не нужно ничего запрашивать, даже если есть поля и обработчик
          return;

        bool FromRow = false;
        if (row != null)
          FromRow = DBxColumns.TableContains(row.Table, Handler.QueriedColumnNames);
        object[] Values;
        if (FromRow)
          Values = Handler.QueriedColumnNames.GetRowValues(row);
        else
        {
          if (id < 0)
            return; // 16.06.2021
          Values = DBCache[tableName].GetValues(id, Handler.QueriedColumnNames, primaryDS); // включая Id,DocId и Delete
        }

        _Args.InitData(tableName, id, Handler.QueriedColumnNames, Values, DBxImageValueNeededReason.RowColor);

        // Добавляем информацию об удаленном документе
        if (DocTypes.UseDeleted)
        {
          // 24.11.2017
          // Вызываем пользовательский обработчик и для удаленного документа
          if (Handler.SubDocType == null)
          {
            if (_Args.GetBool("Deleted"))
            {
              grayed = true;
              // return;
            }
          }
          else
          {
            if (_Args.GetBool("Deleted") || GetDocIdDeleted(Handler))
            {
              grayed = true;
              //return;
            }
          }
        }

        _Args.ColorType = colorType;
        _Args.Grayed = grayed;
        if (Handler.ImageValueNeeded != null)
        {
          try
          {
            Handler.ImageValueNeeded(this, _Args);
          }
          catch
          {
            _Args.ColorType = EFPDataGridViewColorType.Error;
          }
        }

        colorType = _Args.ColorType;
        grayed = _Args.Grayed;
      }
    }

    /// <summary>
    /// Получить раскраску строки для документа или поддокумента
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="row">Строка в табличном просмотре. При поиске значений полей она обладает приоритетом,
    /// по сравнению с DBCache</param>
    /// <param name="colorType">Сюда записывается цветовое оформление строки</param>
    /// <param name="grayed">Сюда записывается true, если строка должна быть выделена серым цветом</param>
    public void GetRowColor(string tableName, DataRow row, out EFPDataGridViewColorType colorType, out bool grayed)
    {
      try
      {
        if (row.RowState == DataRowState.Deleted)
        {
          colorType = EFPDataGridViewColorType.Normal;
          grayed = true;
          return;
        }

        Int32 Id = DataTools.GetInt(row, "Id");
        DoGetRowColor(tableName, Id, row.Table.DataSet, row, out colorType, out grayed);
      }
      catch
      {
        colorType = EFPDataGridViewColorType.Error;
        grayed = true;
      }
    }

    /// <summary>
    /// Получить раскраску строки для документа.
    /// Поля документа используются для определения раскраски. 
    /// Весь набор данных, к которому относится <paramref name="doc"/>, имеет приоритет над DBCache.
    /// </summary>
    /// <param name="doc">Загруженный документ</param>
    /// <param name="colorType">Сюда записывается цветовое оформление строки</param>
    /// <param name="grayed">Сюда записывается true, если строка должна быть выделена серым цветом</param>
    public void GetRowColor(DBxSingleDoc doc, out EFPDataGridViewColorType colorType, out bool grayed)
    {
      try
      {
        DoGetRowColor(doc.DocType.Name, doc.DocId, doc.DocSet.DataSet, null, out colorType, out grayed);
      }
      catch
      {
        colorType = EFPDataGridViewColorType.Error;
        grayed = false;
      }
    }

    /// <summary>
    /// Получить раскраску строки для поддокумента.
    /// Поля поддокумента используются для определения раскраски. 
    /// Весь набор данных, к которому относится <paramref name="subDoc"/>, имеет приоритет над DBCache.
    /// </summary>
    /// <param name="subDoc">Загруженный поддокумент</param>
    /// <param name="colorType">Сюда записывается цветовое оформление строки</param>
    /// <param name="grayed">Сюда записывается true, если строка должна быть выделена серым цветом</param>
    public void GetRowColor(DBxSubDoc subDoc, out EFPDataGridViewColorType colorType, out bool grayed)
    {
      try
      {
        DoGetRowColor(subDoc.SubDocType.Name, subDoc.SubDocId, subDoc.Doc.DocSet.DataSet, null, out colorType, out grayed);
      }
      catch
      {
        colorType = EFPDataGridViewColorType.Error;
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
        return "Ошибка при получении всплывающей подсказки. " + e.Message;
      }
    }

    /// <summary>
    /// Получить всплывающую подсказку для документа или поддокумента.
    /// </summary>
    /// <param name="tableName">Имя таблицы документа или поддокумента</param>
    /// <param name="row">Строка в табличном просмотре. При поиске значений полей она обладает приоритетом,
    /// по сравнению с DBCache</param>
    /// <returns>Текст всплывающей подсказки</returns>
    public string GetToolTipText(string tableName, DataRow row)
    {
      try
      {
        if (row.RowState == DataRowState.Deleted)
          return "Строка удалена";
        Int32 Id = DataTools.GetInt(row, "Id");
        return DoGetToolTipText(tableName, Id, row.Table.DataSet, row);
      }
      catch (Exception e)
      {
        return "Ошибка при получении всплывающей подсказки. " + e.Message;
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

        TableHandler Handler;
        if (!_TableItems.TryGetValue(tableName, out Handler))
        {
          // добавляем пустышку
          if (String.IsNullOrEmpty(tableName))
            throw new ArgumentNullException("tableName");

          return String.Empty;
        }

        if (Handler.DocType == null)
          // Не найденная таблица. Не нужно ничего запрашивать, даже если есть поля и обработчик
          return "Таблица не найдена";
        if (Handler.AccessDeniedFlag)
          return "Доступ к таблице запрещен"; // 10.07.2018

        bool FromRow = false;
        if (row != null)
          FromRow = DBxColumns.TableContains(row.Table, Handler.QueriedColumnNames);
        object[] Values;
        try
        {
          if (FromRow)
            Values = Handler.QueriedColumnNames.GetRowValues(row);
          else
          {
            if (id < 0)
              return "Фиктивный идентификатор"; // 16.06.2021
            Values = DBCache[tableName].GetValues(id, Handler.QueriedColumnNames, primaryDS); // включая Id,DocId и Delete
          }
        }
        catch (DBxAccessException)
        {
          Handler.AccessDeniedFlag = true; // 10.07.2018
          throw;
        }

        _Args.InitData(tableName, id, Handler.QueriedColumnNames, Values, DBxImageValueNeededReason.ToolTipText);

        // Добавляем информацию об удаленном документе
        if (DocTypes.UseDeleted)
        {
          if (Handler.SubDocType == null)
          {
            if (_Args.GetBool("Deleted"))
              return "Документ удален";
          }
          else
          {
            if (_Args.GetBool("Deleted"))
              return "Поддокумент удален";
            if (GetDocIdDeleted(Handler))
              return "Удален основной документ";
          }
        }

        _Args.ToolTipText = String.Empty;
        if (Handler.ImageValueNeeded != null)
        {
          try
          {
            Handler.ImageValueNeeded(this, _Args);
          }
          catch (Exception e)
          {
            _Args.ToolTipText = "Ошибка при получении всплывающей подсказки. " + e.Message;
          }
        }

        return _Args.ToolTipText;
      }
    }


    /// <summary>
    /// Получить всплывающую подсказку для документа.
    /// Поля документа используются для определения подсказки. 
    /// Весь набор данных, к которому относится <paramref name="doc"/>, имеет приоритет над DBCache.
    /// </summary>
    /// <param name="doc">Загруженный документ</param>
    /// <returns>Имя изображения</returns>
    public string GetToolTipText(DBxSingleDoc doc)
    {
      try
      {
        return DoGetToolTipText(doc.DocType.Name, doc.DocId, doc.DocSet.DataSet, null);
      }
      catch (Exception e)
      {
        return "Ошибка при получении всплывающей подсказки. " + e.Message;
      }
    }

    /// <summary>
    /// Получить всплывающую подсказку для поддокумента.
    /// Поля поддокумента используются для определения подсказки. 
    /// Весь набор данных, к которому относится <paramref name="subDoc"/>, имеет приоритет над DBCache.
    /// </summary>
    /// <param name="subDoc">Загруженный поддокумент</param>
    /// <returns>Имя изображения</returns>
    public string GetToolTipText(DBxSubDoc subDoc)
    {
      try
      {
        return DoGetToolTipText(subDoc.SubDocType.Name, subDoc.SubDocId, subDoc.Doc.DocSet.DataSet, null);
      }
      catch (Exception e)
      {
        return "Ошибка при получении всплывающей подсказки. " + e.Message;
      }
    }

    /// <summary>
    /// Получить всплывающую подсказку для документа.
    /// </summary>
    /// <param name="tableId">Идентификатор таблицы документа</param>
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
        return "Ошибка при получении всплывающей подсказки. " + e.Message;
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

        owner.DocTypes.FindByTableName(tableName, out FDocType, out FSubDocType);

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
      private string _TableName;

      public string ImageKey { get { return _ImageKey; } }
      private string _ImageKey;

      /// <summary>
      /// Список полей, заданных пользователем
      /// </summary>
      public DBxColumns ColumnNames { get { return _ColumnNames; } }
      private DBxColumns _ColumnNames;

      public DBxImageValueNeededEventHandler ImageValueNeeded { get { return _ImageValueNeeded; } }
      private DBxImageValueNeededEventHandler _ImageValueNeeded;

      public override string ToString()
      {
        return TableName;
      }

      /// <summary>
      /// Вид документа, к которому относится таблица (если найдено)
      /// </summary>
      public DBxDocType DocType { get { return FDocType; } }
      private DBxDocType FDocType;

      /// <summary>
      /// Вид поддокумента, к которому относится таблица, или null, если таблица относится к документу
      /// </summary>
      public DBxSubDocType SubDocType { get { return FSubDocType; } }
      private DBxSubDocType FSubDocType;

      /// <summary>
      /// Список полей, используемых для запросов.
      /// В начале идут поля из ColumnNames, затем - Id, DocId и Deleted
      /// Если null, значит объект TableHandler еще не был инициализирован для DocProvider
      /// </summary>
      public DBxColumns QueriedColumnNames { get { return _QueriedColumnNames; } }
      private DBxColumns _QueriedColumnNames;

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
    /// Если false - то только те поля, которые были заданы пользователем в методе Add</param>
    /// <returns>Список полей</returns>
    public DBxColumns GetColumnNames(string tableName, bool forQuery)
    {
      lock (_TableItems)
      {
        TableHandler Handler;
        if (_TableItems.TryGetValue(tableName, out Handler))
        {
          if (forQuery)
            return Handler.QueriedColumnNames;
          else
            return Handler.ColumnNames;
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
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// Генерирует исключение при IsReadOnly=true.
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
