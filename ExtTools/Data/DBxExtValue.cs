// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Data;
using FreeLibSet.IO;
using System.Runtime.InteropServices;
using FreeLibSet.Core;
using FreeLibSet.Collections;

namespace FreeLibSet.Data
{
  #region Перечисление DBxExtValuePreferredType

  /// <summary>
  /// Предпочитаемый тип для вызова <see cref="IDBxExtValues.GetValue(int, DBxExtValuePreferredType)"/>
  /// </summary>
  [Serializable]
  public enum DBxExtValuePreferredType
  {
    /// <summary>
    /// Тип не определен
    /// </summary>
    Unknown,

    /// <summary>
    /// Строка
    /// </summary>
    String,

    /// <summary>
    /// Целое число
    /// </summary>
    Int32,

    /// <summary>
    /// Целое число
    /// </summary>
    Int64,

    /// <summary>
    /// Число с плавающей точкой
    /// </summary>
    Single,

    /// <summary>
    /// Число с плавающей точкой
    /// </summary>
    Double,

    /// <summary>
    /// Число с плавающей точкой
    /// </summary>
    Decimal,

    /// <summary>
    /// Логический
    /// </summary>
    Boolean,

    /// <summary>
    /// Дата и время
    /// </summary>
    DateTime,

    /// <summary>
    /// Время
    /// </summary>
    TimeSpan,

    /// <summary>
    /// Идентификатор
    /// </summary>
    Guid,
  }

  #endregion

  /// <summary>
  /// Интерфейс коллекции значений полей <see cref="DBxExtValue"/>.
  /// Поддерживает групповое редактирование.
  /// Реализует интерфейс <see cref="IReadOnlyObject"/>, указывающий, находится ли текущий набор данных в-целом в режиме просмотра.
  /// Если <see cref="IReadOnlyObject.IsReadOnly"/>=false, то набор предназначен для редактирования, но отдельные поля могут быть доступны только для просмотра (при <see cref="DBxExtValue.IsReadOnly"/>=true).
  /// </summary>
  public interface IDBxExtValues : IEnumerable<DBxExtValue>, IReadOnlyObject
  {
    #region Свойства получения DBxExtValue

    /// <summary>
    /// Получить объект для доступа к значению по индексу поля
    /// </summary>
    /// <param name="index">Индекс поля от 0 до (Count-1)</param>
    /// <returns>Объект для доступа к значению</returns>
    DBxExtValue this[int index] { get; }

    /// <summary>
    /// Получить доступ к одному значению
    /// </summary>
    /// <param name="name">Имя поля</param>
    /// <returns>Объект для доступа к значению</returns>
    DBxExtValue this[string name] { get;}

    /// <summary>
    /// Получить имя поля по индексу.
    /// </summary>
    /// <param name="index">Индекс поля от 0 до (Count-1)</param>
    /// <returns>Имя поля</returns>
    string GetName(int index);

    /// <summary>
    /// Получить отображаемое имя поля по индексу.
    /// Как правило, возвращается имя поля, равное GetName()
    /// </summary>
    /// <param name="index">Индекс поля от 0 до (Count-1)</param>
    /// <returns>Отображаемое имя</returns>
    string GetDisplayName(int index);

    /// <summary>
    /// Получить индекс поля по заданному имени или (-1), если нет такого имени
    /// </summary>
    /// <param name="name">Имя поля</param>
    /// <returns>Индекс поля</returns>
    int IndexOf(string name);

    /// <summary>
    /// Получить общее число полей в коллекции.
    /// </summary>
    int Count { get;}

    #endregion

    #region Свойства набора данных в-целом

    /// <summary>
    /// Число одновременно редактируемых строк для обработки "серых" значений
    /// </summary>
    int RowCount { get;}

    // Объявлено в IReadOnlyObject
    ///// <summary>
    ///// true, если набор данных в-целом находится в режиме "Только чтение"
    ///// </summary>
    //bool IsReadOnly { get;}

    #endregion

    #region Доступ к значениям

    /// <summary>
    /// Получить значение
    /// </summary>
    /// <param name="valueIndex">Индекс поля в списке</param>
    /// <param name="preferredType">Предпочитаемый тип значения</param>
    /// <returns>Значение поля</returns>
    object GetValue(int valueIndex, DBxExtValuePreferredType preferredType);

    /// <summary>
    /// Записать значение.
    /// Требуется, чтобы был доступ на запись значения, то есть должно быть IsReadOnly=false и GetValueReadOnly(<paramref name="valueIndex"/>)=false.
    /// </summary>
    /// <param name="valueIndex">Индекс поля в списке</param>
    /// <param name="value">Новое значение</param>
    void SetValue(int valueIndex, object value);

    /// <summary>
    /// Проверить, является ли значение пустым
    /// </summary>
    /// <param name="valueIndex">Индекс поля в списке</param>
    /// <returns>True, если значение NULL</returns>
    bool IsNull(int valueIndex);

    /// <summary>
    /// Может ли поле принимать пустое значение?
    /// </summary>
    /// <param name="valueIndex">Индекс поля в списке</param>
    /// <returns>True, если допускаются значения NULL</returns>
    bool AllowDBNull(int valueIndex);

    /// <summary>
    /// Узнать максимальную длину для строкового поля
    /// (-1), если длина поля не ограничена (как для DataColumn)
    /// </summary>
    /// <param name="valueIndex">Индекс поля в списке</param>
    /// <returns>Длина</returns>
    int MaxLength(int valueIndex);

    /// <summary>
    /// Узнать, предназначено ли конкретное поле только для чтения.
    /// Должно возвращать true, если весь набор имеет свойство IsReadOnly=true
    /// </summary>
    /// <param name="valueIndex">Индекс поля в списке</param>
    /// <returns>Доступ только для чтения?</returns>
    bool GetValueReadOnly(int valueIndex);

    /// <summary>
    /// Проверить, является ли значение "серым", то есть редактируется одновременно
    /// несколько строк и значения поля не совпадают.
    /// Если набор данных не поддерживает "серые" значения, возвращается false.
    /// </summary>
    /// <param name="valueIndex">Индекс поля в списке</param>
    /// <returns>Серое значение?</returns>
    bool GetGrayed(int valueIndex);

    /// <summary>
    /// Получить массив всех значений.
    /// Длина массива равна RowCount.
    /// </summary>
    /// <param name="valueIndex">Индекс поля в списке</param>
    /// <returns>Данные в произвольной форме</returns>
    object[] GetValueArray(int valueIndex);

    /// <summary>
    /// Установка всех значений
    /// После выполнения метода значение может стать "серым".
    /// </summary>
    /// <param name="valueIndex">Индекс поля в списке</param>
    /// <param name="values">Массив значений. Длина массива должна быть равна RowCount</param>
    void SetValueArray(int valueIndex, object[] values);

    /// <summary>
    /// Получить значение для одной из строк. Значение не может быть "серым".
    /// Так как свойство RowCount может возвращать 0, этот метод может оказаться неприменимым для конкретного набора данных.
    /// </summary>
    /// <param name="valueIndex">Индекс поля в списке</param>
    /// <param name="rowIndex">Индекс строки в диапазоне от 0 до RowCount</param>
    /// <returns></returns>
    object GetRowValue(int valueIndex, int rowIndex);

    /// <summary>
    /// Установить значение для одной из строк.
    /// Так как свойство RowCount может возвращать 0, этот метод может оказаться неприменимым для конкретного набора данных.
    /// </summary>
    /// <param name="valueIndex">Индекс поля в списке</param>
    /// <param name="rowIndex">Индекс строки в диапазоне от 0 до RowCount</param>
    /// <param name="value">Значение ячейки</param>
    void SetRowValue(int valueIndex, int rowIndex, object value);

    #endregion
  }

  /// <summary>
  /// Интерфейс получения доступа к двоичным файлам и файлам по ссылкам.
  /// Используется <see cref="DBxExtValue"/>.
  /// Если источник данных поддерживает доступ к данным по ссылке, он должен реализовать, кроме <see cref="IDBxExtValues"/>,
  /// интерфейс <see cref="IDBxBinDataExtValues"/>.
  /// </summary>
  public interface IDBxBinDataExtValues
  {
    #region Свойства

    ///// <summary>
    ///// Свойство возвращает true, если поддерживаются ссылки на двоичные данные
    ///// </summary>
    //bool UseBinDataRefs { get; }

    ///// <summary>
    ///// Свойство возвращает true, если поддерживаются ссылки на файлы
    ///// </summary>
    //bool UseFileRefs { get;}

    /// <summary>
    /// Возвращает двоичные данные для поля
    /// </summary>
    /// <param name="index">Индекс поля в диапазоне от 0 до (<see cref="IDBxExtValues.Count"/>-1)</param>
    /// <returns>Двоичные данные или null</returns>
    byte[] GetBinData(int index);

    /// <summary>
    /// Устанавливает двоичные данные для поля.
    /// Установка значения null выполняет очистку данных
    /// </summary>
    /// <param name="index">Индекс поля в диапазоне от 0 до (<see cref="IDBxExtValues.Count"/>-1)</param>
    /// <param name="data">Двоичные данные или null</param>
    void SetBinData(int index, byte[] data);

    /// <summary>
    /// Возвращает двоичные данные в виде файла (включая имя файла и некоторые атрибуты)
    /// </summary>
    /// <param name="index">Индекс поля в диапазоне от 0 до (<see cref="IDBxExtValues.Count"/>-1)</param>
    /// <returns>Контейнер с содержимым файла</returns>
    FileContainer GetDBFile(int index);

    /// <summary>
    /// Возвращает имя и атрибуты файла
    /// </summary>
    /// <param name="index">Индекс поля в диапазоне от 0 до (<see cref="IDBxExtValues.Count"/>-1)</param>
    /// <returns>Информация о файле</returns>
    StoredFileInfo GetDBFileInfo(int index);

    /// <summary>
    /// Записывает файловые данные в поля
    /// </summary>
    /// <param name="index">Индекс поля в диапазоне от 0 до (<see cref="IDBxExtValues.Count"/>-1)</param>
    /// <param name="file">Контейнер с файлом</param>
    void SetDBFile(int index, FileContainer file);

    #endregion
  }


  /// <summary>
  /// Доступ к одному полю таблицы (объекта, реализующего интерфейс <see cref="IDBxExtValues"/> и, необязательно, <see cref="IDBxBinDataExtValues"/>).
  /// Поддерживает "серые" значения.
  /// </summary>
  public struct DBxExtValue : IReadOnlyObject, IObjectWithCode
  {
    #region Конструктор

    /// <summary>
    /// Инициализация структуры.
    /// Как правило, доступ к структурам выполняется из другого объекта,
    /// реализующего интерфейс <see cref="IDBxExtValues"/>.
    /// </summary>
    /// <param name="source">Объект-владелец</param>
    /// <param name="index">Индекс поля</param>
    public DBxExtValue(IDBxExtValues source, int index)
    {
#if DEBUG
      if (source == null)
        throw new ArgumentNullException("source");
#endif
      if (index < 0 || index >= source.Count)
        throw ExceptionFactory.ArgOutOfRange("index", index, 0, source.Count - 1);

      _Source = source;
      _Index = index;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Основная коллекция - владелец
    /// </summary>
    public IDBxExtValues Source { get { return _Source; } }
    private readonly IDBxExtValues _Source;

    /// <summary>
    /// Индекс значения в коллекции
    /// </summary>
    public int Index { get { return _Index; } }
    private readonly int _Index;

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Признак "Только чтение" для поля.
    /// Возвращает true, если <see cref="Source"/>.IsReadOnly (коллекция в-целом) возвращает true 
    /// или <see cref="IDBxExtValues.GetValueReadOnly(int)"/> (поле) возвращает true.
    /// </summary>
    public bool IsReadOnly
    {
      get { return _Source.IsReadOnly || _Source.GetValueReadOnly(_Index); }
    }

    /// <summary>
    /// Генерирует исключение, если <see cref="IsReadOnly"/>=true.
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw ExceptionFactory.ObjectReadOnly(this);
    }

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>текстовое представление</returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append("Name=");
      sb.Append(Name);
      sb.Append(", Value=");
      if (Value == null)
        sb.Append("null");
      else
        sb.Append(Value.ToString());
      if (Grayed)
        sb.Append(" (Grayed)");
      if (IsReadOnly)
        sb.Append(" (ReadOnly)");
      return sb.ToString();
    }

    #endregion

    #region IObjectWithCode Members

    string IObjectWithCode.Code
    {
      get { return _Source.GetName(_Index); }
    }

    #endregion

    #region Свойства - переходники в DocValues

    /// <summary>
    /// Возвращает имя поля
    /// </summary>
    public string Name { get { return Source.GetName(Index); } }

    /// <summary>
    /// Неформатированное чтение и запись значения
    /// </summary>
    public object Value
    {
      get { return Source.GetValue(Index, DBxExtValuePreferredType.Unknown); }
      set { Source.SetValue(Index, value); }
    }

    /// <summary>
    /// Находится ли в поле несколько разных значений для нескольких строк
    /// </summary>
    public bool Grayed { get { return Source.GetGrayed(Index); } }

    ///// <summary>
    ///// Имелось ли в поле в исходном состоянии несколько значений
    ///// </summary>
    //public bool OriginalGrayed { get { return DocValues.GetOriginalGrayed(Index); } }

    /// <summary>
    /// Количество документов в объекте-владельце
    /// </summary>
    public int RowCount { get { return Source.RowCount; } }

    /// <summary>
    /// Является ли текущее значение пустым?
    /// </summary>
    public bool IsNull { get { return Source.IsNull(Index); } }

    ///// <summary>
    ///// Вернуть значение поля к исходному состоянию
    ///// </summary>
    //public void RestoreValue()
    //{
    //  DocValues.RestoreValue(Index);
    //}

    /// <summary>
    /// Максимальная длина для строковых полей или (-1), если длина
    /// поля не ограничена
    /// </summary>
    public int MaxLength { get { return Source.MaxLength(Index); } }

    /// <summary>
    /// Может ли поле содержать значение null
    /// </summary>
    public bool AllowDBNull { get { return Source.AllowDBNull(Index); } }

    /// <summary>
    /// Читаемый заголовок для поля
    /// </summary>
    public string DisplayName { get { return Source.GetDisplayName(Index); } }

    /// <summary>
    /// Массив значений для всех строк.
    /// Длина массива равна количеству строк или 1, если <see cref="Source"/> предоставляет доступ к одной строке таблицы. 
    /// Для некоторых объектов, реализаующих <see cref="IDBxExtValues"/>, может возвращаться массив нулевой длины.
    /// </summary>
    public object[] ValueArray
    {
      get { return _Source.GetValueArray(Index); }
      set { _Source.SetValueArray(Index, value); }
    }

    #endregion

    #region Форматный доступ к значению поля

    /// <summary>
    /// Доступ к полю как к строке. Если установлено свойство <see cref="MaxLength"/>,
    /// то при записи значения строка будет обрезана.
    /// Начальные и конечные пробелы удаляются. Для доступа к необрезанной строке
    /// используйте свойство <see cref="Value"/>
    /// </summary>
    public string AsString
    {
      get { return DataTools.GetString(_Source.GetValue(Index, DBxExtValuePreferredType.String)); }
      set
      {
        SetString(value);
      }
    }

    /// <summary>
    /// Доступ к значению как к числу.
    /// </summary>
    public int AsInt32
    {
      get { return DataTools.GetInt32(_Source.GetValue(Index, DBxExtValuePreferredType.Int32)); }
      set
      {
        if (value == 0 && AllowDBNull)
          SetNull();
        else
          _Source.SetValue(Index, value);
      }
    }

    /// <summary>
    /// Доступ к значению как к числу.
    /// </summary>
    public long AsInt64
    {
      get { return DataTools.GetInt64(_Source.GetValue(Index, DBxExtValuePreferredType.Int64)); }
      set
      {
        if (value == 0L && AllowDBNull)
          SetNull();
        else
          _Source.SetValue(Index, value);
      }
    }

    /// <summary>
    /// Доступ к значению как к числу.
    /// </summary>
    public float AsSingle
    {
      get { return DataTools.GetSingle(_Source.GetValue(Index, DBxExtValuePreferredType.Single)); }
      set
      {
        if (value == 0f && AllowDBNull)
          SetNull();
        else
          _Source.SetValue(Index, value);
      }
    }

    /// <summary>
    /// Доступ к значению как к числу.
    /// </summary>
    public double AsDouble
    {
      get { return DataTools.GetDouble(_Source.GetValue(Index, DBxExtValuePreferredType.Double)); }
      set
      {
        if (value == 0.0 && AllowDBNull)
          SetNull();
        else
          _Source.SetValue(Index, value);
      }
    }

    /// <summary>
    /// Доступ к значению как к числу.
    /// </summary>
    public decimal AsDecimal
    {
      get { return DataTools.GetDecimal(_Source.GetValue(Index, DBxExtValuePreferredType.Decimal)); }
      set
      {
        if (value == 0m && AllowDBNull)
          SetNull();
        else
          _Source.SetValue(Index, value);
      }
    }

    /// <summary>
    /// Доступ к значению как к логическому значению.
    /// </summary>
    public bool AsBoolean
    {
      get { return DataTools.GetBoolean(_Source.GetValue(Index, DBxExtValuePreferredType.Boolean)); }
      set
      {
        if ((!value) && AllowDBNull)
          SetNull();
        else
          _Source.SetValue(Index, value);
      }
    }

    /// <summary>
    /// Чтение / запись значения типа <see cref="DateTime"/> с возможностью обработки значения Null.
    /// </summary>
    public DateTime? AsNullableDateTime
    {
      get
      {
        return DataTools.GetNullableDateTime(_Source.GetValue(Index, DBxExtValuePreferredType.DateTime));
      }
      set
      {
        if (value.HasValue)
          _Source.SetValue(Index, value.Value);
        else
          SetNull();
      }
    }

    /// <summary>
    /// Чтение / запись значения типа <see cref="DateTime"/> без возможности обработки значения Null.
    /// </summary>
    public DateTime AsDateTime
    {
      get
      {
        return DataTools.GetDateTime(_Source.GetValue(Index, DBxExtValuePreferredType.DateTime));
      }
      set
      {
        _Source.SetValue(Index, value);
      }
    }

    /// <summary>
    /// Чтение / запись значения типа <see cref="TimeSpan"/>
    /// </summary>
    public TimeSpan AsTimeSpan
    {
      get
      {
        return DataTools.GetTimeSpan(_Source.GetValue(Index, DBxExtValuePreferredType.TimeSpan));
      }
      set
      {
        if (value == TimeSpan.Zero && AllowDBNull)
          _Source.IsNull(Index);
        else
          _Source.SetValue(Index, value);
      }
    }

    /// <summary>
    /// Чтение / запись значения типа <see cref="System.Guid"/>
    /// </summary>
    public Guid AsGuid
    {
      get
      {
        return DataTools.GetGuid(_Source.GetValue(Index, DBxExtValuePreferredType.Guid));
      }
      set
      {
        if (value == Guid.Empty && AllowDBNull)
          _Source.IsNull(Index);
        else
          _Source.SetValue(Index, value);
      }
    }

    /// <summary>
    /// Чтение / запись текстового поля как документа XML. Если строка содержит пустое значение, то возвращается значение null
    /// </summary>
    public XmlDocument AsTextXml
    {
      get
      {
        return XmlTools.XmlDocumentFromString(AsString);
      }
      set
      {
        SetTextXml(value);
      }
    }

    /// <summary>
    /// Установить значение null (точнее, <see cref="DBNull"/>)
    /// </summary>
    public void SetNull()
    {
      _Source.SetValue(Index, DBNull.Value);
    }

    #endregion

    #region Альтернативные записывающие методы

    // Так как DBxExtValue является структурой, может возникать ошибка компиляции Compiler Error CS1612, 
    // если свойство устанавливается при обращени по индексу. Например:
    // IDBxDocValue Values=...
    // Values[ИмяПоля].AsString="Hello"; // возникнет ошибка

    /// <summary>
    /// Установить значение поля
    /// </summary>
    /// <param name="value">Значение</param>
    public void SetValue(object value)
    {
      Source.SetValue(Index, value);
    }

    /// <summary>
    /// Установить значение поля.
    /// Если <see cref="AllowDBNull"/>=true, то вместо пустой строки устанавливается пустое значение вызовом <see cref="SetNull()"/>.
    /// </summary>
    /// <param name="value">Значение</param>
    public void SetString(string value)
    {
      if (String.IsNullOrEmpty(value))
      {
        if (AllowDBNull)
          SetNull();
        else
          SetValue(String.Empty);
      }
      else
      {
        string s = value.Trim();
        if (MaxLength > 0 && s.Length > MaxLength)
          s = value.Substring(0, MaxLength);
        SetValue(s);
      }
    }

    /// <summary>
    /// Установить значение поля.
    /// Если <see cref="AllowDBNull"/>=true, то вместо 0 устанавливается пустое значение вызовом <see cref="SetNull()"/>.
    /// </summary>
    /// <param name="value">Значение</param>
    public void SetInt32(int value)
    {
      if (value == 0 && AllowDBNull)
        SetNull();
      else
        SetValue(value);
    }

    /// <summary>
    /// Установить значение поля.
    /// Если <see cref="AllowDBNull"/>=true, то вместо 0 устанавливается пустое значение вызовом <see cref="SetNull()"/>.
    /// </summary>
    /// <param name="value">Значение</param>
    public void SetInt64(long value)
    {
      if (value == 0L && AllowDBNull)
        SetNull();
      else
        SetValue(value);
    }

    /// <summary>
    /// Установить значение поля.
    /// Если <see cref="AllowDBNull"/>=true, то вместо 0 устанавливается пустое значение вызовом <see cref="SetNull()"/>.
    /// </summary>
    /// <param name="value">Значение</param>
    public void SetSingle(float value)
    {
      if (value == 0f && AllowDBNull)
        SetNull();
      else
        SetValue(value);
    }

    /// <summary>
    /// Установить значение поля.
    /// Если <see cref="AllowDBNull"/>=true, то вместо 0 устанавливается пустое значение вызовом <see cref="SetNull()"/>.
    /// </summary>
    /// <param name="value">Значение</param>
    public void SetDouble(double value)
    {
      if (value == 0.0 && AllowDBNull)
        SetNull();
      else
        SetValue(value);
    }

    /// <summary>
    /// Установить значение поля.
    /// Если <see cref="AllowDBNull"/>=true, то вместо 0 устанавливается пустое значение вызовом <see cref="SetNull()"/>.
    /// </summary>
    /// <param name="value">Значение</param>
    public void SetDecimal(decimal value)
    {
      if (value == 0m && AllowDBNull)
        SetNull();
      else
        SetValue(value);
    }

    /// <summary>
    /// Установить значение поля.
    /// Если <see cref="AllowDBNull"/>=true, то вместо false устанавливается пустое значение вызовом <see cref="SetNull()"/>.
    /// </summary>
    /// <param name="value">Значение</param>
    public void SetBoolean(bool value)
    {
      if ((!value) && AllowDBNull)
        SetNull();
      else
        SetValue(value);
    }

    /// <summary>
    /// Установить значение поля.
    /// </summary>
    /// <param name="value">Значение</param>
    public void SetNullableDateTime(DateTime? value)
    {
      if (value.HasValue)
        SetValue(value.Value);
      else
        SetNull();
    }

    /// <summary>
    /// Установить значение поля.
    /// Если <see cref="AllowDBNull"/>=true, то вместо <see cref="TimeSpan.Zero"/> устанавливается пустое значение вызовом <see cref="SetNull()"/>.
    /// </summary>
    /// <param name="value">Значение</param>
    public void SetTimeSpan(TimeSpan value)
    {
      if (value == TimeSpan.Zero && AllowDBNull)
        SetNull();
      else
        SetValue(value);
    }

    /// <summary>
    /// Установить значение текстового поля исходя из XML-документа.
    /// Если <paramref name="value"/>=null, то записывается пустая строка или null, в зависимости от свойства <see cref="AllowDBNull"/>.
    /// </summary>
    /// <param name="value">Значение</param>
    public void SetTextXml(XmlDocument value)
    {
      // Нельзя использовать,
      // т.к. в поле не должна попасть кодировка, иначе сервер базы данных (SQL Server) может выбросить исключение,
      // если поле имеет тип xml, а не nvarchar()
      // SetString(DataTools.XmlDocumentToString(value));

      if (value == null)
        SetString(String.Empty);
      else
      {
        /*
        StringWriter wr1 = new StringWriter();
        XmlTextWriter wr2 = new XmlTextWriter(wr1);
        value.WriteTo(wr2);
        string s = wr1.ToString();
         * */

        StringBuilder sb = new StringBuilder();

        XmlWriterSettings wrtSettings = new XmlWriterSettings();
        //Settings.NewLineChars = Environment.NewLine;
        wrtSettings.OmitXmlDeclaration = true; // Убираем заголовок с кодировкой
        //Settings.Indent = true;
        //Settings.IndentChars = "  ";
        using (XmlWriter wrt = XmlWriter.Create(sb, wrtSettings))
        {
          value.WriteTo(wrt);
          wrt.Flush();
        }

        SetString(sb.ToString());
      }
    }

    //public void SetValueArray(object[] values)
    //{
    //  _DocValues.SetValueArray(Index, values);
    //}

    #endregion

    #region Доступ к двоичным данным и файлам

    /// <summary>
    /// Получить двоичные данные, храняшиеся в базе данных.
    /// Если данных нет, возвращается null.
    /// Если <see cref="Source"/> не реализует интерфейс <see cref="IDBxBinDataExtValues"/>, возвращается null.
    /// </summary>
    /// <returns>Двоичные данные или null</returns>
    public byte[] GetBinData()
    {
      IDBxBinDataExtValues bddv = Source as IDBxBinDataExtValues;
      if (bddv == null)
        return null;
      else
        return bddv.GetBinData(Index);
    }

    /// <summary>
    /// Записать двоичные данные в ссылочное поле.
    /// Присвоение значения null очищает ссылочное поле.
    /// Если <see cref="Source"/> не реализует интерфейс <see cref="IDBxBinDataExtValues"/>, генерируется исключение.
    /// </summary>
    /// <param name="data">Двоичные данные или null</param>
    public void SetBinData(byte[] data)
    {
      GetBinDataExtValuesRequired().SetBinData(Index, data);
    }

    private IDBxBinDataExtValues GetBinDataExtValuesRequired()
    {
      IDBxBinDataExtValues bddv = Source as IDBxBinDataExtValues;
      if (bddv == null)
        throw new NotSupportedException(String.Format(Res.DBxExtValue_Err_BinDataNotSupported, Source.ToString()));
      return bddv;
    }

    /// <summary>
    /// Получить файл, хранящийся в базе данных.
    /// Если ссылка не задана, возвращается null.
    /// Если <see cref="Source"/> не реализует интерфейс <see cref="IDBxBinDataExtValues"/>, возвращается null.
    /// </summary>
    /// <remarks>
    /// Если требуется только получить имя файла, размер или дату/создания изменения, но не двоичные данные,
    /// используйте метод <see cref="GetDBFileInfo()"/>.
    /// </remarks>
    /// <returns>Контейнер с файлом или null</returns>
    public FileContainer GetDBFile()
    {
      IDBxBinDataExtValues bddv = Source as IDBxBinDataExtValues;
      if (bddv == null)
        return null;
      else
        return bddv.GetDBFile(Index);
    }

    /// <summary>
    /// Получить сведения о файле, хранящемся в базе данных.
    /// Двоичные данные не загружаются.
    /// Если ссылка не задана, возвращается <see cref="StoredFileInfo.Empty"/>.
    /// Если <see cref="Source"/> не реализует интерфейс <see cref="IDBxBinDataExtValues"/>, возвращается <see cref="StoredFileInfo.Empty"/>.
    /// </summary>
    /// <returns>Информация о файле</returns>
    public StoredFileInfo GetDBFileInfo()
    {
      IDBxBinDataExtValues bddv = Source as IDBxBinDataExtValues;
      if (bddv == null)
        return StoredFileInfo.Empty;
      else
        return bddv.GetDBFileInfo(Index);
    }

    /// <summary>
    /// Записать файл в ссылочное поле.
    /// Присвоение значения null очищает ссылочное поле.
    /// Если <see cref="Source"/> не реализует интерфейс <see cref="IDBxBinDataExtValues"/>, генерируется исключение.
    /// </summary>
    /// <param name="file">Контейнер с файлом или null</param>
    public void SetDBFile(FileContainer file)
    {
      GetBinDataExtValuesRequired().SetDBFile(Index, file);
    }

    /// <summary>
    /// Получение файла для ссылочного поля и преобразование в XML-документ.
    /// Если файла нет, возвращается null.
    /// Используется вызов <see cref="GetDBFile()"/>.
    /// Если <see cref="Source"/> не реализует интерфейс <see cref="IDBxBinDataExtValues"/>, возвращается null.
    /// Если в базе данных XML-документ хранится не как файл, а как безымянные двоичные данные,
    /// используйте <see cref="GetBinDataXml()"/>
    /// </summary>
    /// <returns>XML-документ или null</returns>
    public XmlDocument GetDBFileXml()
    {
      FileContainer file = GetDBFile();
      if (file == null)
        return null;
      else
        return XmlTools.XmlDocumentFromByteArray(file.Content);
    }


    /// <summary>
    /// Запись XML-документа в ссылочное поля.
    /// Присвоение значения null очищает ссылочное поле.
    /// Используется вызов <see cref="SetDBFile(FileContainer)"/>.
    /// Если <see cref="Source"/> не реализует интерфейс <see cref="IDBxBinDataExtValues"/>, генерируется исключение.
    /// </summary>
    /// <param name="xmlDoc">XML-документ или null</param>
    /// <param name="fileName">Имя файла</param>
    public void SetDBFileXml(XmlDocument xmlDoc, string fileName)
    {
      if (xmlDoc == null)
        SetDBFile(null);
      else
      {
        byte[] contents = XmlTools.XmlDocumentToByteArray(xmlDoc);
        FileContainer file = new FileContainer(fileName, contents);
        SetDBFile(file);
      }
    }

    /// <summary>
    /// Получение двоичных данных и преобразование их в XML-документ.
    /// Если данных нет, возвращается null.
    /// Используется вызов <see cref="GetBinData()"/>.
    /// Если <see cref="Source"/> не реализует интерфейс <see cref="IDBxBinDataExtValues"/>, возвращается null.
    /// Если в базе данных XML-документ хранится как файл, а не как безымянные двоичные данные,
    /// используйте <see cref="GetDBFileXml()"/>.
    /// </summary>
    /// <returns>XML-документ или null</returns>
    public XmlDocument GetBinDataXml()
    {
      byte[] data = GetBinData();
      if (data == null)
        return null;
      else
        return XmlTools.XmlDocumentFromByteArray(data);
    }

    /// <summary>
    /// Запись XML-документа как двоичных данных.
    /// Присвоение значения null очищает ссылочное поле.
    /// Используется вызов <see cref="SetBinData(byte[])"/>.
    /// Если <see cref="Source"/> не реализует интерфейс <see cref="IDBxBinDataExtValues"/>, генерируется исключение.
    /// </summary>
    /// <param name="xmlDoc">XML-документ или null</param>
    public void SetBinDataXml(XmlDocument xmlDoc)
    {
      if (xmlDoc == null)
        SetBinData(null);
      else
      {
        byte[] data = XmlTools.XmlDocumentToByteArray(xmlDoc);
        SetBinData(data);
      }
    }

    #endregion

    #region Статические методы

    /// <summary>
    /// Копирование значений из исходного набора <paramref name="src"/> в набор <paramref name="res"/>.
    /// Копирование выполняется по именам полей, а не по индексам. Непарные поля пропускаются.
    /// Значения с установленным признаком <see cref="IsReadOnly"/> в конечном наборе пропускаются.
    /// Копирование выполняется без обработки "серых" значений. Если в исходном наборе значение "серое",
    /// то в конечном наборе будет пустое "не-серое" значение. Построчное копирование не используется.
    /// </summary>
    /// <param name="src">Исходный набор</param>
    /// <param name="res">Записываемый набор</param>
    public static void CopyValues(IDBxExtValues src, IDBxExtValues res)
    {
      if (src == null)
        throw new ArgumentNullException("src");
#if DEBUG
      if (res == null)
        throw new ArgumentNullException("res");
#endif
      res.CheckNotReadOnly(); // 01.03.2022

      for (int i = 0; i < res.Count; i++)
      {
        string name = res.GetName(i);
        if (res.GetValueReadOnly(i))
          continue;

        int p = src.IndexOf(name);
        if (p < 0)
          continue;

        if (src.IsNull(p))
          res.SetValue(i, null);
        else
        {
          object v = src.GetValue(p, DBxExtValuePreferredType.Unknown);
          res.SetValue(i, v);
        }
      }
    }

    /// <summary>
    /// Копирование значений из строки данных <see cref="DataRow"/> в набор <paramref name="res"/>
    /// Значения с установленным признаком <see cref="IsReadOnly"/> в конечном наборе пропускаются
    /// </summary>
    /// <param name="src">Исходная строка данных</param>
    /// <param name="res">Записываемый набор</param>
    public static void CopyValues(DataRow src, IDBxExtValues res)
    {
      if (src == null)
        throw new ArgumentNullException("src");
#if DEBUG
      if (res == null)
        throw new ArgumentNullException("res");
#endif

      res.CheckNotReadOnly(); // 01.03.2022

      for (int i = 0; i < res.Count; i++)
      {
        if (res.GetValueReadOnly(i))
          continue;

        int p = src.Table.Columns.IndexOf(res.GetName(i));
        if (p < 0)
          continue;

        if (src.IsNull(p))
          res.SetValue(i, null);
        else
          res.SetValue(i, src[p]);
      }
    }

    #endregion
  }

  /// <summary>
  /// Перечислитель для реализации <see cref="IDBxExtValues"/>
  /// </summary>
  public struct DBxExtValueEnumerator : IEnumerator<DBxExtValue>
  {
    #region Конструктор

    /// <summary>
    /// Создает перечислитель
    /// </summary>
    /// <param name="extValues">Интерфейс для доступа к коллекции значений</param>
    public DBxExtValueEnumerator(IDBxExtValues extValues)
    {
      if (extValues == null)
        throw new ArgumentNullException("extValues");

      _ExtValues = extValues;
      _Index = -1;
    }

    #endregion

    #region Поля

    private readonly IDBxExtValues _ExtValues;

    private int _Index;

    #endregion

    #region IEnumerator<DBxExtValue> Members

    /// <summary>
    /// Доступ к текущему полю
    /// </summary>
    public DBxExtValue Current
    {
      get { return _ExtValues[_Index]; }
    }

    /// <summary>
    /// Ничего не делает
    /// </summary>
    public void Dispose()
    {
    }

    object System.Collections.IEnumerator.Current
    {
      get { return _ExtValues[_Index]; }
    }

    /// <summary>
    /// Переход к следующему полю в коллекции значений
    /// </summary>
    /// <returns>True, если есть очередное значение. False, если больше нет полей</returns>
    public bool MoveNext()
    {
      _Index++;
      return _Index < _ExtValues.Count;
    }

    /// <summary>
    /// Сброс перепчислителя
    /// </summary>
    void System.Collections.IEnumerator.Reset()
    {
      _Index = -1;
    }

    #endregion
  }


  /// <summary>
  /// Реализация <see cref="IDBxExtValues"/>, позволяющая работать с массивом значений в памяти
  /// Имена полей являются чувствительными к регистру.
  /// "Серые" значения не поддерживаются. Тип хранимых значений не проверяется.
  /// </summary>
  public class DBxArrayExtValues : IDBxExtValues, IReadOnlyObject
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект доступа к коллекции.
    /// Первоначально предолагается, что все значения коллекции равны null.
    /// </summary>
    /// <param name="names">Список имен. Массив может быть пустым, но не может содержать пустые или повторяющиеся строки</param>
    public DBxArrayExtValues(string[] names)
    {
#if DEBUG
      if (names == null)
        throw new ArgumentNullException("names");
#endif
      for (int i = 0; i < names.Length; i++)
      {
        if (String.IsNullOrEmpty(names[i]))
          ExceptionFactory.ArgInvalidListItem("names", names, i);
      }

      _Names = names;
      _NameIndexer = new StringArrayIndexer(names, false);
      _Values = new object[names.Length];
    }

    /// <summary>
    /// Конструктор копирования.
    /// Значения берутся из исходного объекта.
    /// После создания объекта, старая и новая коллекция никак не связаны.
    /// Хранимые значения являются копией.
    /// </summary>
    /// <param name="orgValues">Коллекция, откуда берутся имена полей и значения</param>
    public DBxArrayExtValues(IDBxExtValues orgValues)
    {
      if (orgValues == null)
        throw new ArgumentNullException("orgValues");
      _Names = new string[orgValues.Count];
      _Values = new object[orgValues.Count];
      for (int i = 0; i < orgValues.Count; i++)
      {
        _Names[i] = orgValues.GetName(i);
        _Values[i] = orgValues.GetValue(i, DBxExtValuePreferredType.Unknown);
      }
      _NameIndexer = new StringArrayIndexer(_Names, false);
    }

    /// <summary>
    /// Создает коллекцию с заданным списком полей.
    /// Первоначально все значения равны null.
    /// </summary>
    /// <param name="columnNames">Список имен полей</param>
    public DBxArrayExtValues(DBxColumns columnNames)
      : this(columnNames.AsArray)
    {
    }

    /// <summary>
    /// Создает коллекцию с заданным списком полей.
    /// После создания коллекции, хранимые значения не связаны с массивом <paramref name="columnValues"/>
    /// </summary>
    /// <param name="columnNames">Список имен полей</param>
    /// <param name="columnValues">Начальные значения полей. Длина массива должна совпадать
    /// с <paramref name="columnNames"/>.</param>
    public DBxArrayExtValues(DBxColumns columnNames, object[] columnValues)
      : this(columnNames.AsArray)
    {
      if (columnValues == null)
        throw new ArgumentNullException("columnValues");
      if (columnValues.Length != columnNames.Count)
        throw ExceptionFactory.ArgWrongCollectionCount("columnValues", columnValues, columnNames.Count);

      for (int i = 0; i < _Names.Length; i++)
        _Values[i] = columnValues[i];
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Массив имен полей
    /// </summary>
    public string[] Names { get { return _Names; } }
    private readonly string[] _Names;

    /// <summary>
    /// Индексатор для списка имен
    /// </summary>
    private readonly StringArrayIndexer _NameIndexer;

    /// <summary>
    /// Массив значений. Длина размера должна совпадать с Names.
    /// </summary>
    public object[] Values
    {
      get { return _Values; }
      set
      {
#if DEBUG
        if (value == null)
          throw new ArgumentNullException("value");
#endif
        if (value.Length != Names.Length)
          ExceptionFactory.ArgWrongCollectionCount("value", value, Names.Length);

        _Values = value;
      }
    }
    private object[] _Values;

    #endregion

    #region IDBxExtValues Members

    /// <summary>
    /// Доступ к значению по имени.
    /// Если в конструкторе не было задано такое имя, генерируется исключение.
    /// </summary>
    /// <param name="name">Имя поля</param>
    /// <returns>Объект для доступа к значению</returns>
    public DBxExtValue this[string name]
    {
      get
      {
        int p = IndexOf(name);
        if (p < 0)
          throw ExceptionFactory.ArgUnknownValue("name", name);
        return new DBxExtValue(this, p);
      }
    }

    /// <summary>
    /// Возвращает имя поля по индексу.
    /// Список допустимых имен полей задается в конструкторе
    /// </summary>
    /// <param name="index">Индекс поля в диапазоне от 0 до (<see cref="Count"/>-1)</param>
    /// <returns>Имя поля</returns>
    public string GetName(int index)
    {
      return _Names[index];
    }

    string IDBxExtValues.GetDisplayName(int index)
    {
      return _Names[index];
    }

    /// <summary>
    /// Возвращает индекс поля в списке Names.
    /// Возвращает (-1), если нет такого поля
    /// </summary>
    /// <param name="name">Имя поля</param>
    /// <returns>Индекс</returns>
    public int IndexOf(string name)
    {
      //return Array.IndexOf<string>(_Names, name);
      return _NameIndexer.IndexOf(name); // 31.12.2019
    }

    /// <summary>
    /// Доступ к значению по индексу поля
    /// </summary>
    /// <param name="index">Индекс поля в диапазоне от 0 до (<see cref="Count"/>-1)</param>
    /// <returns>Объект доступа к значению</returns>
    public DBxExtValue this[int index]
    {
      get
      {
        return new DBxExtValue(this, index);
      }
    }

    /// <summary>
    /// Возвращает количество полей, заданных в конструкторе
    /// </summary>
    public int Count { get { return _Names.Length; } }

    int IDBxExtValues.RowCount { get { return 1; } }

    /// <summary>
    /// Получить значение поля
    /// </summary>
    /// <param name="index">Индекс поля в диапазоне от 0 до (<see cref="Count"/>-1)</param>
    /// <param name="preferredType">Не используется</param>
    /// <returns>Значение</returns>
    public object GetValue(int index, DBxExtValuePreferredType preferredType)
    {
      return _Values[index];
    }

    /// <summary>
    /// Установить значение поля
    /// </summary>
    /// <param name="index">Индекс поля в диапазоне от 0 до (<see cref="Count"/>-1)</param>
    /// <param name="value">Новое значение</param>
    public void SetValue(int index, object value)
    {
      CheckNotReadOnly(); // 03.03.2022
      _Values[index] = value;
    }

    /// <summary>
    /// Возвращает true, если значение поля равно null или <see cref="DBNull"/>
    /// </summary>
    /// <param name="index">Индекс поля в диапазоне от 0 до (<see cref="Count"/>-1)</param>
    /// <returns>Признак null</returns>
    public bool IsNull(int index)
    {
      if (_Values[index] == null)
        return true;
      if (_Values[index] is DBNull)
        return true;
      return false;
    }

    bool IDBxExtValues.AllowDBNull(int index)
    {
      return true;
    }

    int IDBxExtValues.MaxLength(int index)
    {
      //return 0;
      return -1; // 03.03.2022
    }

    bool IDBxExtValues.GetValueReadOnly(int index)
    {
      return false;
    }

    bool IDBxExtValues.GetGrayed(int index)
    {
      return false;
    }

    object[] IDBxExtValues.GetValueArray(int index)
    {
      return new object[1] { _Values[index] };
    }

    void IDBxExtValues.SetValueArray(int index, object[] values)
    {
      if (values.Length != 1)
        throw new ArgumentException("values.Length must be 1", "values");

      CheckNotReadOnly(); // 03.03.2022

      _Values[index] = values[0];
    }

    object IDBxExtValues.GetRowValue(int valueIndex, int rowIndex)
    {
      if (rowIndex != 0)
        throw ExceptionFactory.ArgOutOfRange("rowIndex", rowIndex, 0, 0);
      return GetValue(valueIndex, DBxExtValuePreferredType.Unknown);
    }

    void IDBxExtValues.SetRowValue(int valueIndex, int rowIndex, object value)
    {
      if (rowIndex != 0)
        throw ExceptionFactory.ArgOutOfRange("rowIndex", rowIndex, 0, 0);
      CheckNotReadOnly(); // 03.03.2022
      SetValue(valueIndex, value);
    }

    #endregion

    #region IEnumerable<DBxExtValue> Members

    /// <summary>
    /// Создает перечислитель по всем полям, заданным в конструкторе
    /// </summary>
    /// <returns>Перечислитель</returns>
    public DBxExtValueEnumerator GetEnumerator()
    {
      return new DBxExtValueEnumerator(this);
    }

    IEnumerator<DBxExtValue> IEnumerable<DBxExtValue>.GetEnumerator()
    {
      return new DBxExtValueEnumerator(this);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new DBxExtValueEnumerator(this);
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Возвращает true, если коллекция была переведена в режим "только чтение" вызовом SetReadOnly()
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// Генерирует исключение, если <see cref="IsReadOnly"/>=true.
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (_IsReadOnly)
        throw ExceptionFactory.ObjectReadOnly(this);
    }

    /// <summary>
    /// Переводит коллекцию в режим "Только чтение".
    /// Обратный переход невозможен.
    /// Повторные вызовы метода игнорируются.
    /// </summary>
    public void SetReadOnly()
    {
      _IsReadOnly = true;
    }

    #endregion
  }
}
