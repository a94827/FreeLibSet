using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using FreeLibSet.Core;
using FreeLibSet.Collections;

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

namespace FreeLibSet.Data
{
  #region Перечисление DBxDataWriterMode

  /// <summary>
  /// Режим работы DBxDataWriter
  /// </summary>
  public enum DBxDataWriterMode
  {
    /// <summary>
    /// Добавление строк
    /// </summary>
    Insert,

    /// <summary>
    /// Модификация существующих строк
    /// </summary>
    Update,

    /// <summary>
    /// Добавление недостающих строк и обновление существующих
    /// </summary>
    InsertOrUpdate
  }

  #endregion

  /// <summary>
  /// Набор параметров для работы с DBxDataWriter.
  /// Объект создается и заполняется в пользовательском коде. Затем вызывается метод DBxConBase.CreateWriter()
  /// </summary>
  [Serializable]
  public sealed class DBxDataWriterInfo : IReadOnlyObject, ICloneable
  {
    #region Конструктор

    /// <summary>
    /// Создает неинициализированный набор параметров
    /// </summary>
    public DBxDataWriterInfo()
    {
      _Mode = DBxDataWriterMode.Insert;
    }

    #endregion

    #region Основные свойства

    /// <summary>
    /// Имя таблицы в базе данных.
    /// Свойство обязательно должно быть установлено.
    /// </summary>
    public string TableName
    {
      get { return _TableName; }
      set
      {
        CheckNotReadOnly();
        _TableName = value;
      }
    }
    private string _TableName;

    /// <summary>
    /// Список столбцов, которые будут обрабатываться в запросе.
    /// Свойство обязательно должно быть установлено.
    /// Сюда должны входить все столбцы первичного ключа, если режим отличается от Insert.
    /// В режимах Update и InsertOrUpdate() должен быть задан хотя бы один столбец, не входящий в первичный ключ или в список SearchColumns, если он задан.
    /// </summary>
    public DBxColumns Columns
    {
      get { return _Columns; }
      set
      {
        CheckNotReadOnly();
        _Columns = value;
      }
    }
    private DBxColumns _Columns;


    /// <summary>
    /// Список столбцов, по которым будет выполняться поиск.
    /// Если свойство не установлено в явном виде, будут использованы столбцы из свойства DBxTableStruct.PrimaryKey.
    /// </summary>
    public DBxColumns SearchColumns
    {
      get { return _SearchColumns; }
      set
      {
        CheckNotReadOnly();
        _SearchColumns = value;
      }
    }
    private DBxColumns _SearchColumns;

    /// <summary>
    /// Режим работы.
    /// По умолчанию - Insert
    /// </summary>
    public DBxDataWriterMode Mode
    {
      get { return _Mode; }
      set
      {
        CheckNotReadOnly();
        _Mode = value;
      }
    }
    private DBxDataWriterMode _Mode;

    #endregion

    #region Дополнительные свойства

    /// <summary>
    /// Ожидаемое количество строк, которые предстоит обработать.
    /// По умолчанию - 0 - неизвестно.
    /// Свойство носит рекомендательный характер. Реальное количество строк, которые будут обработаны, может отличаться
    /// 
    /// Установка свойства может привести к изменению стратегии работы.
    /// Например, если ожидается добавление очень большого количества строк, может быть выгодно создать временную базу данных.
    /// В большинстве случаев этот параметр игнорируется.
    /// </summary>
    public long ExpectedRowCount
    {
      get { return _ExpectedRowCount; }
      set
      {
        CheckNotReadOnly();
        if (value < 0L)
          throw new ArgumentOutOfRangeException();
        _ExpectedRowCount = value;
      }
    }
    private long _ExpectedRowCount;

    /// <summary>
    /// Если задано положительное значение, то через указанное количество операций будет выполняться COMMINT TRANSACION,
    /// а затем начинаться новая транзакция.
    /// По умолчанию - 0 - промежуточное подтверждение транзакций не выполняется.
    /// Свойство работает, только если конструктор DBxDataWriter начинает транзакцию. Если на момент вызова DBxConBase.CreateWrite().
    /// 
    /// Для SQLite есть предельный размер данных, который можно обработать. Если предел превышен, то возникает ошибка,
    /// приводящая к перезагрузке Windows. Это происходит только при очень большом количестве записей.
    /// Рекомендуется задать достаточно большое значение, например, 100000, если проблема возникает.
    /// Установка маленького значения (меньше 1000) снижает скорость работы.
    /// </summary>
    public int TransactionPulseRowCount
    {
      get { return _TransactionPulseRowCount; }
      set
      {
        CheckNotReadOnly();
        if (value < 0)
          throw new ArgumentOutOfRangeException();
        _TransactionPulseRowCount = value;
      }
    }
    private int _TransactionPulseRowCount;

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Возвращает true, если набор параметров переведен в режим "Только чтение"
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// Проверка IsReadOnly=false
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (_IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    /// <summary>
    /// Переводит набор параметров в режим "Только чтение"
    /// </summary>
    public void SetReadOnly()
    {
      _IsReadOnly = true;
    }

    #endregion

    #region ICloneable Members

    /// <summary>
    /// Создает копию набора параметров, которую можно редактировать
    /// </summary>
    public DBxDataWriterInfo Clone()
    {
      DBxDataWriterInfo res = new DBxDataWriterInfo();
      res.TableName = TableName;
      res.Columns = Columns;
      res.Mode = Mode;
      res.ExpectedRowCount = ExpectedRowCount;
      return res;
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    #endregion
  }

  #region Перечисление DBxDataWriterState

  /// <summary>
  /// Текущее состояние объекта DBxDataWriter
  /// </summary>
  [Serializable]
  public enum DBxDataWriterState
  {
    /// <summary>
    /// Объект создан и готов к добавлению строк.
    /// Методы Write() и LoadFrom() еще не вызывались
    /// </summary>
    Created,

    /// <summary>
    /// Были вызовы Write() или LoadFrom(), метод Finish() еще не вызывался
    /// </summary>
    Writing,

    /// <summary>
    /// Был вызов метода Finish()
    /// </summary>
    Finished,

    /// <summary>
    /// При вызове Write() или Finish() произошла ошибка.
    /// Вызов других методов, кроме Dispose(), запрещен
    /// </summary>
    Error,

    /// <summary>
    /// Был вызов метода Dispose()
    /// </summary>
    Disposed
  }

  #endregion

  /// <summary>
  /// Объект для добавления и/или изменения строк в таблице.
  /// Создается вызовом DBxConBase.CreateWriter().
  /// Вызывающий код должен заполнять буфер строки и вызывать метод Write() для каждой строки.
  /// Также можно использовать метод LoadFrom() для группового добавления.
  /// По окончании записи должны быть вызваны методы Finish() и Dispose().
  /// Если вызовы методов Write() и Finish() не вызвали исключения, значит запись выполнена успешно.
  /// </summary>
  public abstract class DBxDataWriter : MarshalByRefDisposableObject
  {
    #region Конструктор и Dispose

    /// <summary>
    /// Инициализация объекта в состоянии Created.
    /// Если нет текущей транзации (DBxConBase.CurrentTransaction=null), то начинает транзацию вызовом DBxConBase.TransactionBegin().
    /// Если у соединения уже начата транзакция, то DBxDataWriter не будет обрабатывать транзакции.
    /// </summary>
    /// <param name="con"></param>
    /// <param name="writerInfo"></param>
    public DBxDataWriter(DBxConBase con, DBxDataWriterInfo writerInfo)
    {
      // Проверка аргументов находится в DBxConBase.CreateWriter()

      _Con = con;
      _WriterInfo = writerInfo;
      _State = DBxDataWriterState.Created;
      _Values = new object[writerInfo.Columns.Count];
      _ColumnNameIndexer = new StringArrayIndexer(writerInfo.Columns.AsArray, false);

      _TableStruct = con.DB.Struct.Tables[writerInfo.TableName];
#if DEBUG
      if (_TableStruct == null)
        throw new ArgumentException("Не найдена структура таблицы \"" + writerInfo.TableName + "\"");
#endif

      _ColumnDefs = new DBxColumnStruct[writerInfo.Columns.Count];
      for (int i = 0; i < writerInfo.Columns.Count; i++)
      {
        _ColumnDefs[i] = _TableStruct.Columns[writerInfo.Columns[i]];
        if (_ColumnDefs[i] == null)
          throw new ArgumentException("Не найден столбец \"" + writerInfo.Columns[i] + "\" в таблице \"" + writerInfo.TableName + "\"");
      }


      if (writerInfo.SearchColumns == null)
      {
        if (writerInfo.Columns.ContainsAny(_TableStruct.PrimaryKey))
          _SearchColumns = _TableStruct.PrimaryKey;
        else
          _SearchColumns = DBxColumns.Empty;
      }
      else
        _SearchColumns = writerInfo.SearchColumns; // 02.08.2020

      if (_SearchColumns.Count > 0)
      {
        if (writerInfo.Columns.ContainsAny(_SearchColumns) &&
          (!writerInfo.Columns.Contains(_SearchColumns)))
          throw new ArgumentException("Список всех столбцов не может содержать только часть столбцов составного ключа для поиска (" + _SearchColumns.ToString() + ") в таблице \"" + _TableStruct.TableName + "\"", "writerInfo");
      }
      else // SearchColumns.IsEmpty
      {
        switch (writerInfo.Mode)
        {
          case DBxDataWriterMode.Update:
          case DBxDataWriterMode.InsertOrUpdate:
            throw new ArgumentException("Таблица \"" + writerInfo.TableName + "\" не содержит первичного ключа. Первичный ключ требуется для режима " + writerInfo.Mode.ToString(), "writerInfo");
        }
      }


      _SearchColumnPositions = new int[_SearchColumns.Count];
      for (int i = 0; i < _SearchColumns.Count; i++)
        _SearchColumnPositions[i] = _ColumnNameIndexer.IndexOf(SearchColumns[i]);

      _OtherColumns = writerInfo.Columns - _SearchColumns;

      _OtherColumnPositions = new int[_OtherColumns.Count];
      for (int i = 0; i < _OtherColumns.Count; i++)
        _OtherColumnPositions[i] = _ColumnNameIndexer.IndexOf(OtherColumns[i]);

      switch (writerInfo.Mode)
      {
        case DBxDataWriterMode.Update:
        case DBxDataWriterMode.InsertOrUpdate:
          if (_SearchColumns.Count == 0)
            throw new ArgumentException("В режиме " + writerInfo.Mode.ToString() + " должен быть задан список столбцов для поиска", "writerInfo");
          if (_OtherColumns.Count == 0)
            throw new ArgumentException("В режиме " + writerInfo.Mode.ToString() + " список столбцов должен включать в себя хотя бы один столбец, не являющийся столбцом для поиска в таблице \"" + _TableStruct.TableName + "\"", "writerInfo");
          break;
      }

      if (con.CurrentTransaction == null)
      {
        con.TransactionBegin();
        _TransactionStarted = true;
      }
      _PulseCounter = writerInfo.TransactionPulseRowCount;
    }

    /// <summary>
    /// Переводит объект в состояние State=Disposed.
    /// При нормальной работе, перед вызовом метода Dispose() должен вызываться метод Finish().
    /// Если метод Finish() не вызывался, или ему не удалось завершить транзакцию, вызывается DBxConBase.TransactionRollback(),
    /// если транзакция была начата в конструкторе объекта
    /// </summary>
    /// <param name="disposing">true, если был вызван метод Dispose(), а не деструктор объекта</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (_TransactionStarted)
        {
          _Con.TransactionRollback();
          _TransactionStarted = false;
        }
      }

      _State = DBxDataWriterState.Disposed;
      _Values = null;
      base.Dispose(disposing);

    }

    #endregion

    #region Основные свойства

    /// <summary>
    /// Соединение с базой данной, для которого был вызван метод CreateCon().
    /// Свойство объявлено защищенным, чтобы при вызове по сети не было незаконного доступа к объекту.
    /// </summary>
    protected DBxConBase Con { get { return _Con; } }
    private readonly DBxConBase _Con;

    /// <summary>
    /// Управляющие параметры.
    /// Свойства этого объекта нельзя менять.
    /// </summary>
    public DBxDataWriterInfo WriterInfo { get { return _WriterInfo; } }
    private readonly DBxDataWriterInfo _WriterInfo;

    /// <summary>
    /// Текущее состояние объекта
    /// </summary>
    public DBxDataWriterState State { get { return _State; } }
    private DBxDataWriterState _State;

    bool _TransactionStarted;

    #endregion

    #region Буфер заполняемой строки

    /// <summary>
    /// Буфер строки.
    /// Длина массива соответствует полям в DBxDataWriterInfo.Columns.
    /// Обычно используется индексированное свойство объекта DBxDataWriter для записи значений по одному
    /// </summary>
    public object[] Values
    {
      get { return _Values; }
      set
      {
        CheckStateWriteable();
        if (value == null)
          throw new ArgumentNullException();
        if (value.Length != _Values.Length)
          throw new ArgumentException("Неправильная длина массива");
        _Values = value;
      }
    }
    private object[] _Values;

    /// <summary>
    /// Доступ к полю буфера строки по индексу поля в списке DBxDataWriterInfo.Columns.
    /// </summary>
    /// <param name="columnIndex">Индекс столбца в списке</param>
    /// <returns>Значение поля</returns>
    public object this[int columnIndex]
    {
      get { return _Values[columnIndex]; }
      set { _Values[columnIndex] = value; }
    }

    /// <summary>
    /// Доступ к буферу строки по имени поля в списке DBxDataWriterInfo.Columns.
    /// </summary>
    /// <param name="columnName">Имя поля</param>
    /// <returns>Значение поля</returns>
    public object this[string columnName]
    {
      get { return _Values[GetColumnIndex(columnName)]; }
      set { _Values[GetColumnIndex(columnName)] = value; }
    }

    private readonly StringArrayIndexer _ColumnNameIndexer;

    private int GetColumnIndex(string columnName)
    {
      int p = _ColumnNameIndexer.IndexOf(columnName);
      if (p >= 0)
        return p;

      if (String.IsNullOrEmpty(columnName))
        throw new ArgumentNullException("columnName");
      else
        throw new ArgumentException("Неизвестное имя столбца \"" + columnName + "\"", "columnName");
    }

    #endregion

    #region Форматированный доступ

    /// <summary>
    /// Получить ранее записанное значение из буфера заполняемой строки.
    /// </summary>
    /// <param name="columnName">Имя поля из списка DBxDataWriterInfo.Columns</param>
    /// <returns>Значение в буфере</returns>
    public string GetString(string columnName) { return DataTools.GetString(this[columnName]); }

    /// <summary>
    /// Получить ранее записанное значение из буфера заполняемой строки.
    /// </summary>
    /// <param name="columnIndex">Индекс поля из списка DBxDataWriterInfo.Columns</param>
    /// <returns>Значение в буфере</returns>
    public string GetString(int columnIndex) { return DataTools.GetString(this[columnIndex]); }

    /// <summary>
    /// Записать значение в буфер строки.
    /// </summary>
    /// <param name="columnName">Имя поля из списка DBxDataWriterInfo.Columns</param>
    /// <param name="value">Устанавливаемое значение</param>
    public void SetString(string columnName, string value)
    {
      SetString(GetColumnIndex(columnName), value);
    }

    /// <summary>
    /// Записать значение в буфер строки.
    /// </summary>
    /// <param name="columnIndex">Индекс поля из списка DBxDataWriterInfo.Columns</param>
    /// <param name="value">Устанавливаемое значение</param>
    public void SetString(int columnIndex, string value)
    {
      if (value == null)
        value = String.Empty;

      if (value.Length == 0 && _ColumnDefs[columnIndex].Nullable)
        _Values[columnIndex] = null;
      else
      {
        switch (_ColumnDefs[columnIndex].ColumnType)
        {
          case DBxColumnType.Int:
            _Values[columnIndex] = StdConvert.ToInt64(value);
            break;
          case DBxColumnType.Float:
            _Values[columnIndex] = StdConvert.ToDouble(value);
            break;
          case DBxColumnType.Money:
            _Values[columnIndex] = StdConvert.ToDecimal(value);
            break;
          case DBxColumnType.Boolean:
            _Values[columnIndex] = StdConvert.ToInt32(value) != 0;
            break;
          case DBxColumnType.String:
            _Values[columnIndex] = value;
            break;
          default:
            throw CreateColumnTypeException(columnIndex, typeof(Int32));
        }
      }
    }


    /// <summary>
    /// Получить ранее записанное значение из буфера заполняемой строки.
    /// </summary>
    /// <param name="columnName">Имя поля из списка DBxDataWriterInfo.Columns</param>
    /// <returns>Значение в буфере</returns>
    public int GetInt(string columnName) { return DataTools.GetInt(this[columnName]); }

    /// <summary>
    /// Получить ранее записанное значение из буфера заполняемой строки.
    /// </summary>
    /// <param name="columnIndex">Индекс поля из списка DBxDataWriterInfo.Columns</param>
    /// <returns>Значение в буфере</returns>
    public int GetInt(int columnIndex) { return DataTools.GetInt(this[columnIndex]); }

    /// <summary>
    /// Записать значение в буфер строки.
    /// </summary>
    /// <param name="columnName">Имя поля из списка DBxDataWriterInfo.Columns</param>
    /// <param name="value">Устанавливаемое значение</param>
    public void SetInt(string columnName, int value)
    {
      SetInt(GetColumnIndex(columnName), value);
    }

    /// <summary>
    /// Записать значение в буфер строки.
    /// </summary>
    /// <param name="columnIndex">Индекс поля из списка DBxDataWriterInfo.Columns</param>
    /// <param name="value">Устанавливаемое значение</param>
    public void SetInt(int columnIndex, int value)
    {
      if (value == 0 && _ColumnDefs[columnIndex].Nullable)
        _Values[columnIndex] = null;
      else
      {
        switch (_ColumnDefs[columnIndex].ColumnType)
        {
          case DBxColumnType.Int:
          case DBxColumnType.Float:
          case DBxColumnType.Money:
            _Values[columnIndex] = value;
            break;
          case DBxColumnType.String:
            _Values[columnIndex] = StdConvert.ToString(value);
            break;
          default:
            throw CreateColumnTypeException(columnIndex, typeof(Int32));
        }
      }
    }


    /// <summary>
    /// Получить ранее записанное значение из буфера заполняемой строки.
    /// </summary>
    /// <param name="columnName">Имя поля из списка DBxDataWriterInfo.Columns</param>
    /// <returns>Значение в буфере</returns>
    public long GetInt64(string columnName) { return DataTools.GetInt64(this[columnName]); }

    /// <summary>
    /// Получить ранее записанное значение из буфера заполняемой строки.
    /// </summary>
    /// <param name="columnIndex">Индекс поля из списка DBxDataWriterInfo.Columns</param>
    /// <returns>Значение в буфере</returns>
    public long GetInt64(int columnIndex) { return DataTools.GetInt64(this[columnIndex]); }

    /// <summary>
    /// Записать значение в буфер строки.
    /// </summary>
    /// <param name="columnName">Имя поля из списка DBxDataWriterInfo.Columns</param>
    /// <param name="value">Устанавливаемое значение</param>
    public void SetInt64(string columnName, long value)
    {
      SetInt64(GetColumnIndex(columnName), value);
    }

    /// <summary>
    /// Записать значение в буфер строки.
    /// </summary>
    /// <param name="columnIndex">Индекс поля из списка DBxDataWriterInfo.Columns</param>
    /// <param name="value">Устанавливаемое значение</param>
    public void SetInt64(int columnIndex, long value)
    {
      if (value == 0L && _ColumnDefs[columnIndex].Nullable)
        _Values[columnIndex] = null;
      else
      {
        switch (_ColumnDefs[columnIndex].ColumnType)
        {
          case DBxColumnType.Int:
          case DBxColumnType.Float:
          case DBxColumnType.Money:
            _Values[columnIndex] = value;
            break;
          case DBxColumnType.String:
            _Values[columnIndex] = StdConvert.ToString(value);
            break;
          default:
            throw CreateColumnTypeException(columnIndex, typeof(Int64));
        }
      }
    }


    /// <summary>
    /// Получить ранее записанное значение из буфера заполняемой строки.
    /// </summary>
    /// <param name="columnName">Имя поля из списка DBxDataWriterInfo.Columns</param>
    /// <returns>Значение в буфере</returns>
    public float GetSingle(string columnName) { return DataTools.GetSingle(this[columnName]); }

    /// <summary>
    /// Получить ранее записанное значение из буфера заполняемой строки.
    /// </summary>
    /// <param name="columnIndex">Индекс поля из списка DBxDataWriterInfo.Columns</param>
    /// <returns>Значение в буфере</returns>
    public float GetSingle(int columnIndex) { return DataTools.GetSingle(this[columnIndex]); }

    /// <summary>
    /// Записать значение в буфер строки.
    /// </summary>
    /// <param name="columnName">Имя поля из списка DBxDataWriterInfo.Columns</param>
    /// <param name="value">Устанавливаемое значение</param>
    public void SetSingle(string columnName, float value)
    {
      SetSingle(GetColumnIndex(columnName), value);
    }

    /// <summary>
    /// Записать значение в буфер строки.
    /// </summary>
    /// <param name="columnIndex">Индекс поля из списка DBxDataWriterInfo.Columns</param>
    /// <param name="value">Устанавливаемое значение</param>
    public void SetSingle(int columnIndex, float value)
    {
      if (value == 0f && _ColumnDefs[columnIndex].Nullable)
        _Values[columnIndex] = null;
      else
      {
        switch (_ColumnDefs[columnIndex].ColumnType)
        {
          case DBxColumnType.Int:
          case DBxColumnType.Float:
          case DBxColumnType.Money:
            _Values[columnIndex] = value;
            break;
          case DBxColumnType.String:
            _Values[columnIndex] = StdConvert.ToString(value);
            break;
          default:
            throw CreateColumnTypeException(columnIndex, typeof(Single));
        }
      }
    }


    /// <summary>
    /// Получить ранее записанное значение из буфера заполняемой строки.
    /// </summary>
    /// <param name="columnName">Имя поля из списка DBxDataWriterInfo.Columns</param>
    /// <returns>Значение в буфере</returns>
    public double GetDouble(string columnName) { return DataTools.GetDouble(this[columnName]); }

    /// <summary>
    /// Получить ранее записанное значение из буфера заполняемой строки.
    /// </summary>
    /// <param name="columnIndex">Индекс поля из списка DBxDataWriterInfo.Columns</param>
    /// <returns>Значение в буфере</returns>
    public double GetDouble(int columnIndex) { return DataTools.GetDouble(this[columnIndex]); }

    /// <summary>
    /// Записать значение в буфер строки.
    /// </summary>
    /// <param name="columnName">Имя поля из списка DBxDataWriterInfo.Columns</param>
    /// <param name="value">Устанавливаемое значение</param>
    public void SetDouble(string columnName, double value)
    {
      SetDouble(GetColumnIndex(columnName), value);
    }

    /// <summary>
    /// Записать значение в буфер строки.
    /// </summary>
    /// <param name="columnIndex">Индекс поля из списка DBxDataWriterInfo.Columns</param>
    /// <param name="value">Устанавливаемое значение</param>
    public void SetDouble(int columnIndex, double value)
    {
      if (value == 0.0 && _ColumnDefs[columnIndex].Nullable)
        _Values[columnIndex] = null;
      else
      {
        switch (_ColumnDefs[columnIndex].ColumnType)
        {
          case DBxColumnType.Int:
          case DBxColumnType.Float:
          case DBxColumnType.Money:
            _Values[columnIndex] = value;
            break;
          case DBxColumnType.String:
            _Values[columnIndex] = StdConvert.ToString(value);
            break;
          default:
            throw CreateColumnTypeException(columnIndex, typeof(Double));
        }
      }
    }


    /// <summary>
    /// Получить ранее записанное значение из буфера заполняемой строки.
    /// </summary>
    /// <param name="columnName">Имя поля из списка DBxDataWriterInfo.Columns</param>
    /// <returns>Значение в буфере</returns>
    public decimal GetDecimal(string columnName) { return DataTools.GetDecimal(this[columnName]); }

    /// <summary>
    /// Получить ранее записанное значение из буфера заполняемой строки.
    /// </summary>
    /// <param name="columnIndex">Индекс поля из списка DBxDataWriterInfo.Columns</param>
    /// <returns>Значение в буфере</returns>
    public decimal GetDecimal(int columnIndex) { return DataTools.GetDecimal(this[columnIndex]); }

    /// <summary>
    /// Записать значение в буфер строки.
    /// </summary>
    /// <param name="columnName">Имя поля из списка DBxDataWriterInfo.Columns</param>
    /// <param name="value">Устанавливаемое значение</param>
    public void SetDecimal(string columnName, decimal value)
    {
      SetDecimal(GetColumnIndex(columnName), value);
    }

    /// <summary>
    /// Записать значение в буфер строки.
    /// </summary>
    /// <param name="columnIndex">Индекс поля из списка DBxDataWriterInfo.Columns</param>
    /// <param name="value">Устанавливаемое значение</param>
    public void SetDecimal(int columnIndex, decimal value)
    {
      if (value == 0 && _ColumnDefs[columnIndex].Nullable)
        _Values[columnIndex] = null;
      else
      {
        switch (_ColumnDefs[columnIndex].ColumnType)
        {
          case DBxColumnType.Int:
          case DBxColumnType.Float:
          case DBxColumnType.Money:
            _Values[columnIndex] = value;
            break;
          case DBxColumnType.String:
            _Values[columnIndex] = StdConvert.ToString(value);
            break;
          default:
            throw CreateColumnTypeException(columnIndex, typeof(Decimal));
        }
      }
    }

    /// <summary>
    /// Получить ранее записанное значение из буфера заполняемой строки.
    /// </summary>
    /// <param name="columnName">Имя поля из списка DBxDataWriterInfo.Columns</param>
    /// <returns>Значение в буфере</returns>
    public bool GetBool(string columnName) { return DataTools.GetBool(this[columnName]); }

    /// <summary>
    /// Получить ранее записанное значение из буфера заполняемой строки.
    /// </summary>
    /// <param name="columnIndex">Индекс поля из списка DBxDataWriterInfo.Columns</param>
    /// <returns>Значение в буфере</returns>
    public bool GetBool(int columnIndex) { return DataTools.GetBool(this[columnIndex]); }

    /// <summary>
    /// Записать значение в буфер строки.
    /// </summary>
    /// <param name="columnName">Имя поля из списка DBxDataWriterInfo.Columns</param>
    /// <param name="value">Устанавливаемое значение</param>
    public void SetBool(string columnName, bool value)
    {
      SetBool(GetColumnIndex(columnName), value);
    }

    /// <summary>
    /// Записать значение в буфер строки.
    /// </summary>
    /// <param name="columnIndex">Индекс поля из списка DBxDataWriterInfo.Columns</param>
    /// <param name="value">Устанавливаемое значение</param>
    public void SetBool(int columnIndex, bool value)
    {
      if (value == false && _ColumnDefs[columnIndex].Nullable)
        _Values[columnIndex] = null;
      else
      {
        switch (_ColumnDefs[columnIndex].ColumnType)
        {
          case DBxColumnType.Boolean:
            _Values[columnIndex] = value;
            break;
          default:
            throw CreateColumnTypeException(columnIndex, typeof(Decimal));
        }
      }
    }

    /// <summary>
    /// Получить ранее записанное значение из буфера заполняемой строки.
    /// </summary>
    /// <param name="columnName">Имя поля из списка DBxDataWriterInfo.Columns</param>
    /// <returns>Значение в буфере</returns>
    public DateTime GetDateTime(string columnName) { return DataTools.GetDateTime(this[columnName]); }

    /// <summary>
    /// Получить ранее записанное значение из буфера заполняемой строки.
    /// </summary>
    /// <param name="columnIndex">Индекс поля из списка DBxDataWriterInfo.Columns</param>
    /// <returns>Значение в буфере</returns>
    public DateTime GetDateTime(int columnIndex) { return DataTools.GetDateTime(this[columnIndex]); }

    /// <summary>
    /// Записать значение в буфер строки.
    /// </summary>
    /// <param name="columnName">Имя поля из списка DBxDataWriterInfo.Columns</param>
    /// <param name="value">Устанавливаемое значение</param>
    public void SetDateTime(string columnName, DateTime value)
    {
      SetDateTime(GetColumnIndex(columnName), value);
    }

    /// <summary>
    /// Записать значение в буфер строки.
    /// </summary>
    /// <param name="columnIndex">Индекс поля из списка DBxDataWriterInfo.Columns</param>
    /// <param name="value">Устанавливаемое значение</param>
    public void SetDateTime(int columnIndex, DateTime value)
    {
      switch (_ColumnDefs[columnIndex].ColumnType)
      {
        case DBxColumnType.Date:
        case DBxColumnType.DateTime:
          _Values[columnIndex] = value;
          break;
        default:
          throw CreateColumnTypeException(columnIndex, typeof(Decimal));
      }
    }

    /// <summary>
    /// Получить ранее записанное значение из буфера заполняемой строки.
    /// </summary>
    /// <param name="columnName">Имя поля из списка DBxDataWriterInfo.Columns</param>
    /// <returns>Значение в буфере</returns>
    public DateTime? GetNullableDateTime(string columnName) { return DataTools.GetNullableDateTime(this[columnName]); }

    /// <summary>
    /// Получить ранее записанное значение из буфера заполняемой строки.
    /// </summary>
    /// <param name="columnIndex">Индекс поля из списка DBxDataWriterInfo.Columns</param>
    /// <returns>Значение в буфере</returns>
    public DateTime? GetNullableDateTime(int columnIndex) { return DataTools.GetNullableDateTime(this[columnIndex]); }

    /// <summary>
    /// Записать значение в буфер строки.
    /// </summary>
    /// <param name="columnName">Имя поля из списка DBxDataWriterInfo.Columns</param>
    /// <param name="value">Устанавливаемое значение</param>
    public void SetNullableDateTime(string columnName, DateTime? value)
    {
      SetNullableDateTime(GetColumnIndex(columnName), value);
    }

    /// <summary>
    /// Записать значение в буфер строки.
    /// </summary>
    /// <param name="columnIndex">Индекс поля из списка DBxDataWriterInfo.Columns</param>
    /// <param name="value">Устанавливаемое значение</param>
    public void SetNullableDateTime(int columnIndex, DateTime? value)
    {
      if ((!value.HasValue) && (!_ColumnDefs[columnIndex].Nullable))
        throw new InvalidOperationException("Столбец \"" + _ColumnDefs[columnIndex] + "\" таблицы \"" + _TableStruct.TableName + "\" не разрешает значения null");
      // Если даже для столбца определено значение DEFAULT, то его нельзя указывать в списке VALUES()

      switch (_ColumnDefs[columnIndex].ColumnType)
      {
        case DBxColumnType.Date:
        case DBxColumnType.DateTime:
          _Values[columnIndex] = value;
          break;
        default:
          throw CreateColumnTypeException(columnIndex, typeof(Decimal));
      }
    }

    /// <summary>
    /// Получить ранее записанное значение из буфера заполняемой строки.
    /// </summary>
    /// <param name="columnName">Имя поля из списка DBxDataWriterInfo.Columns</param>
    /// <returns>Значение в буфере</returns>
    public Guid GetGuid(string columnName) { return DataTools.GetGuid(this[columnName]); }

    /// <summary>
    /// Получить ранее записанное значение из буфера заполняемой строки.
    /// </summary>
    /// <param name="columnIndex">Индекс поля из списка DBxDataWriterInfo.Columns</param>
    /// <returns>Значение в буфере</returns>
    public Guid GetGuid(int columnIndex) { return DataTools.GetGuid(this[columnIndex]); }

    /// <summary>
    /// Записать значение в буфер строки.
    /// </summary>
    /// <param name="columnName">Имя поля из списка DBxDataWriterInfo.Columns</param>
    /// <param name="value">Устанавливаемое значение</param>
    public void SetGuid(string columnName, Guid value)
    {
      SetGuid(GetColumnIndex(columnName), value);
    }

    /// <summary>
    /// Записать значение в буфер строки.
    /// </summary>
    /// <param name="columnIndex">Индекс поля из списка DBxDataWriterInfo.Columns</param>
    /// <param name="value">Устанавливаемое значение</param>
    public void SetGuid(int columnIndex, Guid value)
    {
      if (value == Guid.Empty && _ColumnDefs[columnIndex].Nullable)
        _Values[columnIndex] = null;
      else
      {
        switch (_ColumnDefs[columnIndex].ColumnType)
        {
          case DBxColumnType.Guid:
            _Values[columnIndex] = value;
            break;
          case DBxColumnType.String:
            _Values[columnIndex] = value.ToString();
            break;
          case DBxColumnType.Binary:
            _Values[columnIndex] = value.ToByteArray();
            break;
          default:
            throw CreateColumnTypeException(columnIndex, typeof(Decimal));
        }
      }
    }

    private Exception CreateColumnTypeException(int columnIndex, Type type)
    {
      return new Exception("Столбец \"" + _ColumnDefs[columnIndex] + "\" таблицы \"" + _TableStruct.TableName + "\" имеет тип " +
        _ColumnDefs[columnIndex].ColumnType.ToString() + " и не может принимать значения типа " + type.ToString());
    }

    #endregion

    #region Позиции столбцов для поиска и прочих столбцов

    /// <summary>
    /// Описание структуры таблицы
    /// </summary>
    public DBxTableStruct TableStruct { get { return _TableStruct; } }
    private readonly DBxTableStruct _TableStruct;


    /// <summary>
    /// Описания столбцов таблицы, соответствующие DBxDataWriterInfo.Columns
    /// </summary>
    public DBxColumnStruct[] ColumnDefs { get { return _ColumnDefs; } }
    private readonly DBxColumnStruct[] _ColumnDefs;

    /// <summary>
    /// Имена столбцов первичного ключа или заданных в свойстве DBxDataWriterInfo.SearchColumns
    /// </summary>
    public DBxColumns SearchColumns { get { return _SearchColumns; } }
    private readonly DBxColumns _SearchColumns;

    /// <summary>
    /// Позиции столбцов первичного ключа в массиве Values
    /// </summary>
    public int[] SearchColumnPositions { get { return _SearchColumnPositions; } }
    private readonly int[] _SearchColumnPositions;

    /// <summary>
    /// Имена столбцов, не входящих в первичный ключ
    /// </summary>
    public DBxColumns OtherColumns { get { return _OtherColumns; } }
    private readonly DBxColumns _OtherColumns;

    /// <summary>
    /// Позиции прочих столбцов в массиве Values
    /// </summary>
    public int[] OtherColumnPositions { get { return _OtherColumnPositions; } }
    private readonly int[] _OtherColumnPositions;

    #endregion

    #region Основные методы, вызываемые из прикладного кода

    /// <summary>
    /// Счетчик для вызова PulseTransaction()
    /// </summary>
    private int _PulseCounter;

    /// <summary>
    /// Обработка одной строки.
    /// На момент вызова должен быть заполнен буфер строки
    /// </summary>
    public void Write()
    {
      CheckStateWriteable();
      try
      {
        OnWrite();
      }
      catch
      {
        _State = DBxDataWriterState.Error;
        throw;
      }

      for (int i = 0; i < _Values.Length; i++)
        _Values[i] = null;

      if (_WriterInfo.TransactionPulseRowCount > 0)
      {
        _PulseCounter--;
        if (_PulseCounter < 0)
        {
          PulseTransaction();
        }
      }
    }

    /// <summary>
    /// Завершение работы.
    /// Если используется какая-либо отложенная запись, она будет выполнена.
    /// Затем будет завершена транзакция
    /// В случае успеха, метод переводит объект в состояние Finished.
    /// </summary>
    public void Finish()
    {
      CheckStateWriteable();
      try
      {
        OnFinish();

        if (_TransactionStarted)
        {
          _Con.TransactionCommit();
          _TransactionStarted = false;
        }


        _State = DBxDataWriterState.Finished;
      }
      catch
      {
        _State = DBxDataWriterState.Error;
        throw;
      }
    }

    #endregion

    #region Защишенные методы

    private void CheckStateWriteable()
    {
      switch (_State)
      {
        case DBxDataWriterState.Created:
        case DBxDataWriterState.Writing:
          break;
        default:
          throw new InvalidOperationException("DBxDataWriter находится в состоянии " + _State.ToString());
      }
    }

    /// <summary>
    /// Должен выполнять добавление строки из буфера
    /// </summary>
    protected abstract void OnWrite();

    /// <summary>
    /// Должен записать отложенные буферизованные данные, если такие есть.
    /// Этот метод также вызывается из PulseTransaction()
    /// </summary>
    protected virtual void OnFinish()
    {
    }

    /// <summary>
    /// Создает пустую таблицу данных, содержащую столбцы ColumnDefs
    /// </summary>
    /// <returns>Новая таблица DataTable</returns>
    protected DataTable CreateDataTable()
    {
      DataTable table = new DataTable(WriterInfo.TableName);
      for (int i = 0; i < ColumnDefs.Length; i++)
        table.Columns.Add(ColumnDefs[i].CreateDataColumn());
      return table;
    }


    #endregion

    #region Дополнительные методы

    /// <summary>
    /// Загрузка данных из таблицы
    /// </summary>
    /// <param name="table">Таблица, откуда будут взяты строки</param>
    public virtual void LoadFrom(DataTable table)
    {
      if (table == null)
        throw new ArgumentNullException("table");

      if (table.Rows.Count == 0)
        return;

      // Построение индексов полей в таблице
      int[] columnIndices = new int[_Values.Length];
      for (int i = 0; i < columnIndices.Length; i++)
      {
        columnIndices[i] = table.Columns.IndexOf(_WriterInfo.Columns[i]);
        if (columnIndices[i] < 0)
          throw new ArgumentException("Таблица \"" + table.TableName + "\" не содержит столбца \"" + _WriterInfo.Columns[i] + "\"", "table");
      }

      foreach (DataRow row in table.Rows)
      {
        for (int i = 0; i < _Values.Length; i++)
          _Values[i] = row[columnIndices[i]];
        Write();
      }
    }

    /// <summary>
    /// Загрузка данных из DbDataReader
    /// </summary>
    /// <param name="reader">Источник, откуда будут взяты строки</param>
    public virtual void LoadFrom(DbDataReader reader)
    {
      if (reader == null)
        throw new ArgumentNullException("reader");

      // Построение индексов полей в таблице
      int[] columnIndices = new int[_Values.Length];
      for (int i = 0; i < columnIndices.Length; i++)
      {
        columnIndices[i] = reader.GetOrdinal(_WriterInfo.Columns[i]);
        if (columnIndices[i] < 0)
          throw new ArgumentException("Источник данных не содержит столбца \"" + _WriterInfo.Columns[i] + "\"", "reader");
      }

      while (reader.Read())
      {
        for (int i = 0; i < _Values.Length; i++)
          _Values[i] = reader[columnIndices[i]];
        Write();
      }
    }

    /// <summary>
    /// Завершает текущую транзакцию, вызывая COMMIT_TRANSACTION, и начинает новую транзакцию.
    /// Этот метод может вызываться автоматически из Write(), если было установлено свойство
    /// DBxDataWriterInfo.TransactionPulseRowCount
    /// </summary>
    /// <returns></returns>
    public bool PulseTransaction()
    {
      // обнуляем счетчик здесь, на случай, если метод вызван из прикладного кода, а свойство TransactionPulseRowCount
      // также установлено
      _PulseCounter = _WriterInfo.TransactionPulseRowCount;

      if (!_TransactionStarted)
        return false;

      OnFinish();

      OnPulseTransaction();

      return true;
    }

    /// <summary>
    /// Вызывается из PulseTransaction().
    /// Выполняет DBxCon.TransactionCommit() и DBxCon.TransactionBegin()
    /// </summary>
    protected virtual void OnPulseTransaction()
    {
      _Con.TransactionCommit();
      _TransactionStarted = false; // на случай ошибки начала новой транзакции

      _Con.TransactionBegin();
      _TransactionStarted = true;
    }


    #endregion
  }

#if XXX
  /// <summary>
  /// Класс-заглушка, который просто теряет все строки
  /// </summary>
  internal class DBxDummyDataWriter : DBxDataWriter
  {
  #region Конструктор

    /// <summary>
    /// Инициализация объекта в состоянии Created
    /// </summary>
    /// <param name="con"></param>
    /// <param name="writerInfo"></param>
    public DBxDummyDataWriter(DBxConBase con, DBxDataWriterInfo writerInfo)
      : base(con, writerInfo)
    {
    }

  #endregion

  #region Заглушки

    /// <summary>
    /// Ничего не делает
    /// </summary>
    protected override void OnWrite()
    {
    }

  #endregion
  }
#endif

  /// <summary>
  /// Реализация DBxDataWriter по умолчанию.
  /// Он вызывает методы класса DBxConBase для обработки каждой строки.
  /// Вероятно, этот класс никогда не будет использоваться
  /// </summary>
  public class DBxDefaultDataWriter : DBxDataWriter
  {
    #region Конструктор

    /// <summary>
    /// Инициализация объекта в состоянии Created
    /// </summary>
    /// <param name="con"></param>
    /// <param name="writerInfo"></param>
    public DBxDefaultDataWriter(DBxConBase con, DBxDataWriterInfo writerInfo)
      : base(con, writerInfo)
    {
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Обработка строки
    /// </summary>
    protected override void OnWrite()
    {
      switch (WriterInfo.Mode)
      {
        case DBxDataWriterMode.Insert:
          Con.AddRecord(WriterInfo.TableName, WriterInfo.Columns, Values);
          break;
        case DBxDataWriterMode.Update:
        case DBxDataWriterMode.InsertOrUpdate:
          DBxFilter[] Filters = new DBxFilter[SearchColumns.Count];
          for (int i = 0; i < SearchColumns.Count; i++)
            Filters[i] = new ValueFilter(SearchColumns[i], this[SearchColumnPositions[i]]);
          DBxFilter f = AndFilter.FromArray(Filters);

          if (WriterInfo.Mode == DBxDataWriterMode.InsertOrUpdate)
          {
            if (Con.GetRecordCount(WriterInfo.TableName, f) == 0)
            {
              // INSERT
              Con.AddRecord(WriterInfo.TableName, WriterInfo.Columns, Values);
              return;
            }
          }

          // Обновление
          object[] v2 = new object[OtherColumns.Count];
          for (int i = 0; i < v2.Length; i++)
            v2[i] = this[OtherColumnPositions[i]];

          Con.SetValues(WriterInfo.TableName, AndFilter.FromArray(Filters), OtherColumns, v2);
          break;
        default:
          throw new BugException("Mode=" + WriterInfo.Mode.ToString());
      }
    }

    #endregion
  }

#if XXX
  /// <summary>
  /// Объект для записи, использующий команду DBxCommand с параметрами "P1", "P2", ...
  /// Этот класс не работает в режиме Insert-or-update
  /// </summary>
  internal class DBxParametriсDataWriter : DBxDataWriter
  {
  #region Конструктор

    public DBxParametriсDataWriter(DBxConBase con, DBxDataWriterInfo writerInfo)
      : base(con, writerInfo)
    {
    }

  #endregion

  #region Подготовленная команда

    /// <summary>
    /// Команда, выполняющая запрос для каждой строки
    /// </summary>
    protected DbCommand Command { get { return _Command; } }
    private DbCommand _Command;

    protected void PrepareCommand()
    {
      DBxSqlBuffer Buffer = new DBxSqlBuffer(Con.DB.Formatter);
      switch (WriterInfo.Mode)
      {
        case DBxDataWriterMode.Insert:
          FormatInsertSQL(Buffer);
          break;

        case DBxDataWriterMode.Update:
          FormatUpdateSql(Buffer);
          break;

        case DBxDataWriterMode.InsertOrUpdate:
          throw new NotImplementedException("Не реализовано для режима InsertOrUpdate");

        default:
          throw new BugException("Неизвестный Mode=" + WriterInfo.Mode.ToString());
      }

      _Command = Con.DB.ProviderFactory.CreateCommand();
      _Command.CommandText = Buffer.SB.ToString();
      for (int i = 0; i < Values.Length; i++)
      {
        DbParameter p = Con.DB.ProviderFactory.CreateParameter();
        p.ParameterName="P" + (i + 1).ToString();
        _Command.Parameters.Add(p);
      }
      _Command.Connection = Con.сonnection;
      _Command.Prepare(); // подготовка команды. Для SQLite ничего не делает, а для других провайдеров - кто знает
    }

    private void FormatInsertSQL(DBxSqlBuffer Buffer)
    {
      Buffer.SB.Append("INSERT INTO ");
      Buffer.FormatTableName(WriterInfo.TableName);
      Buffer.SB.Append(" (");
      Buffer.FormatCSColumnNames(WriterInfo.Columns);
      Buffer.SB.Append(") VALUES (");

      for (int i = 0; i < Values.Length; i++)
      {
        if (i > 0)
          Buffer.SB.Append(',');
        Buffer.FormatParamPlaceholder(i);
      }
      Buffer.SB.Append(")");
    }

    private void FormatUpdateSql(DBxSqlBuffer Buffer)
    {
      Buffer.SB.Append("UPDATE ");
      Buffer.FormatTableName(WriterInfo.TableName);
      Buffer.SB.Append(" SET ");

      for (int i = 0; i < OtherColumnNames.Count; i++)
      {
        if (i > 0)
          Buffer.SB.Append(", ");
        Buffer.FormatColumnName(OtherColumnNames[i]);
        Buffer.SB.Append("=");
        Buffer.FormatParamPlaceholder(OtherColumnPositions[i]);
      }

      Buffer.SB.Append(" WHERE ");
      for (int i = 0; i < PKColumnNames.Count; i++)
      {
        if (i > 0)
          Buffer.SB.Append(" AND ");
        Buffer.FormatColumnName(PKColumnNames[i]);
        Buffer.SB.Append("=");
        Buffer.FormatParamPlaceholder(PKColumnPositions[i]);
      }
    }

  #endregion

  #region OnWrite

    protected override void OnWrite()
    {
      if (_Command == null)
        PrepareCommand();

      for (int i = 0; i < Values.Length; i++)
      {
        object v = Con.DB.Formatter.PrepareParamValue(Values[i], ColumnDefs[i].ColumnType);
        _Command.Parameters[i].Value = v;
      }
      _Command.ExecuteNonQuery();
    }

  #endregion
  }
#endif
}
