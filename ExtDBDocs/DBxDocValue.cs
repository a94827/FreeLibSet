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

namespace FreeLibSet.Data.Docs
{
  #region Перечисление DBxDocValuePreferredType

  /// <summary>
  /// Предпочитаемый тип для вызова IDBxDocValue.GetValue()
  /// </summary>
  [Serializable]
  public enum DBxDocValuePreferredType
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
  }

  #endregion

  /// <summary>
  /// Интерфейс коллекции значений полей документа.
  /// Поддерживает групповое редактирование документов.
  /// </summary>
  public interface IDBxDocValues : IEnumerable<DBxDocValue>, IReadOnlyObject
  {
    #region Свойства получения DocValue

    /// <summary>
    /// Получить объект для доступа к значению по индексу поля
    /// </summary>
    /// <param name="index">Индекс поля от 0 до (Count-1)</param>
    /// <returns>Объект для доступа к значению</returns>
    DBxDocValue this[int index] { get; }

    /// <summary>
    /// Получить доступ к одному значению
    /// </summary>
    /// <param name="name">Имя поля</param>
    /// <returns>Объект для доступа к значению</returns>
    DBxDocValue this[string name] { get;}

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
    object GetValue(int valueIndex, DBxDocValuePreferredType preferredType);

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
  /// Используется DBxDocValue.
  /// Если источник данных поддерживает доступ к данным по ссылке, он должен реализовать, кроме IDBxDocValues,
  /// интерфейс IDBxBinDataDocValues
  /// </summary>
  public interface IDBxBinDataDocValues
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
    /// <param name="index">Индекс поля в диапазоне от 0 до (IDBxDocValues.Count-1)</param>
    /// <returns>Двоичные данные или null</returns>
    byte[] GetBinData(int index);

    /// <summary>
    /// Устанавливает двоичные данные для поля.
    /// Установка значения null выполняет очистку данных
    /// </summary>
    /// <param name="index">Индекс поля в диапазоне от 0 до (IDBxDocValues.Count-1)</param>
    /// <param name="data">Двоичные данные или null</param>
    void SetBinData(int index, byte[] data);

    /// <summary>
    /// Возвращает двоичные данные в виде файла (включая имя файла и некоторые атрибуты)
    /// </summary>
    /// <param name="index">Индекс поля в диапазоне от 0 до (IDBxDocValues.Count-1)</param>
    /// <returns>Контейнер с содержимым файла</returns>
    FileContainer GetDBFile(int index);

    /// <summary>
    /// Возвращает имя и атрибуты файла
    /// </summary>
    /// <param name="index">Индекс поля в диапазоне от 0 до (IDBxDocValues.Count-1)</param>
    /// <returns>Информация о файле</returns>
    StoredFileInfo GetDBFileInfo(int index);

    /// <summary>
    /// Записывает файловые данные в поля
    /// </summary>
    /// <param name="index">Индекс поля в диапазоне от 0 до (IDBxDocValues.Count-1)</param>
    /// <param name="file">Контейнер с файлом</param>
    void SetDBFile(int index, FileContainer file);

    #endregion
  }

  /// <summary>
  /// Доступ к одному полю документа.
  /// Поддерживает "серые" значения.
  /// </summary>
  public struct DBxDocValue : IReadOnlyObject, IObjectWithCode
  {
    #region Конструктор

    /// <summary>
    /// Инициализация структуры.
    /// Как правило, доступ к структурам выполняется из другого объекта,
    /// реализующего интерфейс IDBxDocValues.
    /// </summary>
    /// <param name="docValues">Объект-владелец</param>
    /// <param name="index">Индекс поля</param>
    public DBxDocValue(IDBxDocValues docValues, int index)
    {
#if DEBUG
      if (docValues == null)
        throw new ArgumentNullException("docValues");
      if (index < 0 || index >= docValues.Count)
        throw new ArgumentOutOfRangeException("index", index, "Индекс значения должен быть в диапазоне от 0 до " + (docValues.Count - 1).ToString());
#endif
      _DocValues = docValues;
      _Index = index;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Основная коллекция - владелец
    /// </summary>
    public IDBxDocValues DocValues { get { return _DocValues; } }
    private IDBxDocValues _DocValues;

    /// <summary>
    /// Индекс значения в коллекции
    /// </summary>
    public int Index { get { return _Index; } }
    private int _Index;

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Признак "Только чтение" для поля.
    /// Возвращает true, если IDBxDocValues.IsReadOnly (коллекция в-целом) возвращает true 
    /// или IDBxDocValues.GetValueReadOnly() (поле) возвращает true.
    /// </summary>
    public bool IsReadOnly
    {
      get { return _DocValues.IsReadOnly || _DocValues.GetValueReadOnly(_Index); }
    }

    /// <summary>
    /// Генерирует исключение, если IsReadOnly=true.
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw new ObjectReadOnlyException();
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
      get { return _DocValues.GetName(_Index); }
    }

    #endregion

    #region Свойства - переходники в DocValues

    /// <summary>
    /// Возвращает имя поля
    /// </summary>
    public string Name { get { return DocValues.GetName(Index); } }

    /// <summary>
    /// Неформатированное чтение и запись значения
    /// </summary>
    public object Value
    {
      get { return DocValues.GetValue(Index, DBxDocValuePreferredType.Unknown); }
      set { DocValues.SetValue(Index, value); }
    }

    /// <summary>
    /// Находится ли в поле несколько разных значений для нескольких строк
    /// </summary>
    public bool Grayed { get { return DocValues.GetGrayed(Index); } }

    ///// <summary>
    ///// Имелось ли в поле в исходном состоянии несколько значений
    ///// </summary>
    //public bool OriginalGrayed { get { return DocValues.GetOriginalGrayed(Index); } }

    /// <summary>
    /// Количество документов в объекте-владельце
    /// </summary>
    public int RowCount { get { return DocValues.RowCount; } }

    /// <summary>
    /// Является ли текущее значение пустым ?
    /// </summary>
    public bool IsNull { get { return DocValues.IsNull(Index); } }

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
    public int MaxLength { get { return DocValues.MaxLength(Index); } }

    /// <summary>
    /// Может ли поле содержать значение null
    /// </summary>
    public bool AllowDBNull { get { return DocValues.AllowDBNull(Index); } }

    /// <summary>
    /// Читаемый заголовок для поля
    /// </summary>
    public string DisplayName { get { return DocValues.GetDisplayName(Index); } }

    /// <summary>
    /// Массив значений для всех строк.
    /// Длина массива равна RowCount. Для некоторых объектов, реализацующих IDBxDocValues, может возвращаться массив нулевой длины
    /// </summary>
    public object[] ValueArray
    {
      get { return _DocValues.GetValueArray(Index); }
      set { _DocValues.SetValueArray(Index, value); }
    }

    #endregion

    #region Форматный доступ к значению поля

    /// <summary>
    /// Доступ к полю как к строке. Если установлено свойство MaxLength,
    /// то при записи значения строка будет обрезана
    /// Начальные и конечные пробелы удаляются. Для доступа к необрезанной строке
    /// используйте свойство Value
    /// </summary>
    public string AsString
    {
      get { return DataTools.GetString(_DocValues.GetValue(Index, DBxDocValuePreferredType.String)); }
      set
      {
        SetString(value);
      }
    }

    /// <summary>
    /// Доступ к значению как к числу.
    /// </summary>
    public int AsInteger
    {
      get { return DataTools.GetInt(_DocValues.GetValue(Index, DBxDocValuePreferredType.Int32)); }
      set
      {
        if (value == 0 && AllowDBNull)
          SetNull();
        else
          _DocValues.SetValue(Index, value);
      }
    }

    /// <summary>
    /// Доступ к значению как к числу.
    /// </summary>
    public float AsSingle
    {
      get { return DataTools.GetSingle(_DocValues.GetValue(Index, DBxDocValuePreferredType.Single)); }
      set
      {
        if (value == 0f && AllowDBNull)
          SetNull();
        else
          _DocValues.SetValue(Index, value);
      }
    }

    /// <summary>
    /// Доступ к значению как к числу.
    /// </summary>
    public double AsDouble
    {
      get { return DataTools.GetDouble(_DocValues.GetValue(Index, DBxDocValuePreferredType.Double)); }
      set
      {
        if (value == 0.0 && AllowDBNull)
          SetNull();
        else
          _DocValues.SetValue(Index, value);
      }
    }

    /// <summary>
    /// Доступ к значению как к числу.
    /// </summary>
    public decimal AsDecimal
    {
      get { return DataTools.GetDecimal(_DocValues.GetValue(Index, DBxDocValuePreferredType.Decimal)); }
      set
      {
        if (value == 0m && AllowDBNull)
          SetNull();
        else
          _DocValues.SetValue(Index, value);
      }
    }

    /// <summary>
    /// Доступ к значению как к логическому значению.
    /// </summary>
    public bool AsBoolean
    {
      get { return DataTools.GetBool(_DocValues.GetValue(Index, DBxDocValuePreferredType.Boolean)); }
      set
      {
        if ((!value) && AllowDBNull)
          SetNull();
        else
          _DocValues.SetValue(Index, value);
      }
    }

    /// <summary>
    /// Чтение / запись значения типа DateTime с возможностью обработки значения Null
    /// (используя шаблонную структуру Nullable)
    /// </summary>
    public DateTime? AsNullableDateTime
    {
      get
      {
        return DataTools.GetNullableDateTime(_DocValues.GetValue(Index, DBxDocValuePreferredType.DateTime));
      }
      set
      {
        if (value.HasValue)
          _DocValues.SetValue(Index, value.Value);
        else
          SetNull();
      }
    }

    /// <summary>
    /// Чтение / запись значения типа DateTime без возможности обработки значения Null
    /// </summary>
    public DateTime AsDateTime
    {
      get
      {
        return DataTools.GetDateTime(_DocValues.GetValue(Index, DBxDocValuePreferredType.DateTime));
      }
      set
      {
        _DocValues.SetValue(Index, value);
      }
    }

    /// <summary>
    /// Чтение / запись значения типа TimeSpan
    /// </summary>
    public TimeSpan AsTimeSpan
    {
      get
      {
        return DataTools.GetTimeSpan(_DocValues.GetValue(Index, DBxDocValuePreferredType.TimeSpan));
      }
      set
      {
        if (value == TimeSpan.Zero && AllowDBNull)
          _DocValues.IsNull(Index);
        else
          _DocValues.SetValue(Index, value);
      }
    }

    /// <summary>
    /// Чтение / запись текстового поля как документа XML. Если строка содержит пустое значение, то
    /// возвращается значение null
    /// </summary>
    public XmlDocument AsTextXml
    {
      get
      {
        return DataTools.XmlDocumentFromString(AsString);
      }
      set
      {
        SetTextXml(value);
      }
    }

    /// <summary>
    /// Установить значение null (точнее, DBNull)
    /// </summary>
    public void SetNull()
    {
      _DocValues.SetValue(Index, DBNull.Value);
    }

    #endregion

    #region Альтернативные записывающие методы

    // Так как DBxDocValue является структурой, может возникать ошибка компиляции Compiler Error CS1612, 
    // если свойство устанавливается при обращени по индексу. Например:
    // IDBxDocValue Values=...
    // Values[ИмяПоля].AsString="Hello"; // возникнет ошибка

    /// <summary>
    /// Установить значение поля
    /// </summary>
    /// <param name="value">Значение</param>
    public void SetValue(object value)
    {
      DocValues.SetValue(Index, value);
    }

    /// <summary>
    /// Установить значение поля.
    /// Если AllowDBNull=true, то вместо пустой строки устанавливается пустое значение вызовом SetNull().
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
    /// Если AllowDBNull=true, то вместо 0 устанавливается пустое значение вызовом SetNull().
    /// </summary>
    /// <param name="value">Значение</param>
    public void SetInteger(int value)
    {
      if (value == 0 && AllowDBNull)
        SetNull();
      else
        SetValue(value);
    }

    /// <summary>
    /// Установить значение поля.
    /// Если AllowDBNull=true, то вместо 0 устанавливается пустое значение вызовом SetNull().
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
    /// Если AllowDBNull=true, то вместо 0 устанавливается пустое значение вызовом SetNull().
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
    /// Если AllowDBNull=true, то вместо 0 устанавливается пустое значение вызовом SetNull().
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
    /// Если AllowDBNull=true, то вместо false устанавливается пустое значение вызовом SetNull().
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
    /// Если AllowDBNull=true, то вместо TimeSpan.Zero устанавливается пустое значение вызовом SetNull().
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
    /// Если <paramref name="value"/> то записывается пустая строка или null, в зависимости от свойства AllowDBNull.
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
    /// Получить двоичные данные, храняшиеся в базе данных (ссылочное поле, объявленное в ВinDataRefs)
    /// Если ссылка не задана, возвращается null.
    /// Если DocValues не реализует интерфейс IDBxBinDataDocValues, возвращается null.
    /// </summary>
    /// <returns>Двоичные данные или null</returns>
    public byte[] GetBinData()
    {
      IDBxBinDataDocValues bddv = DocValues as IDBxBinDataDocValues;
      if (bddv == null)
        return null;
      else
        return bddv.GetBinData(Index);
    }

    /// <summary>
    /// Записать двоичные данные в ссылочное поле, объявленное в ВinDataRefs.
    /// Данные будут помещены в базу данных при вызове ApplyChanges().
    /// Присвоение значения null очищает ссылочное поле.
    /// Если DocValues не реализует интерфейс IDBxBinDataDocValues, генерируется исключение.
    /// </summary>
    /// <param name="data">Двоичные данные или null</param>
    public void SetBinData(byte[] data)
    {
      IDBxBinDataDocValues bddv = DocValues as IDBxBinDataDocValues;
      if (bddv == null)
        throw new NullReferenceException("Двоичные данные по ссылке не поддерживаются");
      bddv.SetBinData(Index, data);
    }

    /// <summary>
    /// Получить файл, хранящийся в базе данных (ссылочное поле, объявленное в FileRefs).
    /// Если ссылка не задана, возвращается null.
    /// Если DocValues не реализует интерфейс IDBxBinDataDocValues, возвращается null.
    /// </summary>
    /// <remarks>
    /// Если требуется только получить имя файла, размер или дату/создания изменения, но не двоичные данные,
    /// используйте метод GetDBFileInfo().
    /// </remarks>
    /// <returns>Контейнер с файлом или null</returns>
    public FileContainer GetDBFile()
    {
      IDBxBinDataDocValues bddv = DocValues as IDBxBinDataDocValues;
      if (bddv == null)
        return null;
      else
        return bddv.GetDBFile(Index);
    }

    /// <summary>
    /// Получить сведения о файле, хранящемся в базе данных (ссылочное поле, объявленное в FileRefs).
    /// Двоичные данные не загружаются.
    /// Если ссылка не задана, возвращается StoredFileInfo.Empty.
    /// Если DocValues не реализует интерфейс IDBxBinDataDocValues, генерируется исключение.
    /// </summary>
    /// <returns>Информация о файле</returns>
    public StoredFileInfo GetDBFileInfo()
    {
      IDBxBinDataDocValues bddv = DocValues as IDBxBinDataDocValues;
      if (bddv == null)
        throw new NullReferenceException("Файлы по ссылке не поддерживаются");
      else
        return bddv.GetDBFileInfo(Index);
    }

    /// <summary>
    /// Записать файл в ссылочное поле, объявленное в FileRefs.
    /// Данные будут помещены в базу данных при вызове ApplyChanges().
    /// Присвоение значения null очищает ссылочное поле.
    /// Если DocValues не реализует интерфейс IDBxBinDataDocValues, генерируется исключение.
    /// </summary>
    /// <param name="file">Контейнер с файлом или null</param>
    public void SetDBFile(FileContainer file)
    {
      IDBxBinDataDocValues bddv = DocValues as IDBxBinDataDocValues;
      if (bddv == null)
        throw new NullReferenceException("Файлы по ссылке не поддерживаются");
      bddv.SetDBFile(Index, file);
    }

    /// <summary>
    /// Получение файла для ссылочного поля, объявленного в FileRefs, и преобразование в XML-документ.
    /// Если файла нет, возвращается null.
    /// Используется вызов GetDBFile().
    /// Если DocValues не реализует интерфейс IDBxBinDataDocValues, генерируется исключение.
    /// </summary>
    /// <remarks>
    /// Если в базе данных XML-документ хранится не как файл, а как безымянные двомчные данные,
    /// используйте GetBinDataXml()
    /// </remarks>
    /// <returns>XML-документ или null</returns>
    public XmlDocument GetDBFileXml()
    {
      FileContainer File = GetDBFile();
      if (File == null)
        return null;
      else
        return File.GetXml();
    }


    /// <summary>
    /// Запись XML-документа в ссылочное поля, объявленное в FileRefs.
    /// Присвоение значения null очищает ссылочное поле.
    /// Используется вызов SetDBFile().
    /// Если DocValues не реализует интерфейс IDBxBinDataDocValues, генерируется исключение.
    /// </summary>
    /// <param name="xmlDoc">XML-документ или null</param>
    /// <param name="fileName">Имя файла</param>
    public void SetDBFileXml(XmlDocument xmlDoc, string fileName)
    {
      if (xmlDoc == null)
        SetDBFile(null);
      else
      {
        byte[] contents = DataTools.XmlDocumentToByteArray(xmlDoc);
        FileContainer file = new FileContainer(fileName, contents);
        SetDBFile(file);
      }
    }

    /// <summary>
    /// Получение двоичных данных и преобразование их в XML-документ.
    /// Если данных нет, возвращается null.
    /// Используется вызов GetBinData().
    /// Если DocValues не реализует интерфейс IDBxBinDataDocValues, генерируется исключение.
    /// </summary>
    /// <remarks>
    /// Если в базе данных XML-документ хранится как файл, а не как безымянные двомчные данные,
    /// используйте GetDBFileXml()
    /// </remarks>
    /// <returns>XML-документ или null</returns>
    public XmlDocument GetBinDataXml()
    {
      byte[] data = GetBinData();
      if (data == null)
        return null;
      else
        return DataTools.XmlDocumentFromByteArray(data);
    }

    /// <summary>
    /// Запись XML-документа как двоичных данных.
    /// Присвоение значения null очищает ссылочное поле.
    /// Используется вызов SetBinData().
    /// Если DocValues не реализует интерфейс IDBxBinDataDocValues, генерируется исключение.
    /// </summary>
    /// <param name="xmlDoc">XML-документ или null</param>
    public void SetBinDataXml(XmlDocument xmlDoc)
    {
      if (xmlDoc == null)
        SetBinData(null);
      else
      {
        byte[] data = DataTools.XmlDocumentToByteArray(xmlDoc);
        SetBinData(data);
      }
    }

    #endregion

    #region Статические методы

    /// <summary>
    /// Копирование значений из исходного набора <paramref name="src"/> в набор <paramref name="res"/>.
    /// Копирование выполняется по именам полей, а не по индексам. Непарные поля пропускаются.
    /// Значения с установленным признаком IsReadOnly в конечном наборе пропускаются.
    /// Копирование выполняется без обработки "серых" значений. Если в исходном наборе значение "серое",
    /// то в конечном наборе будет пустое "не-серое" значение. Построчное копирование не используется.
    /// </summary>
    /// <param name="src">Исходный набор</param>
    /// <param name="res">Записываемый набор</param>
    public static void CopyValues(IDBxDocValues src, IDBxDocValues res)
    {
      if (src == null)
        throw new ArgumentNullException("src");
      if (res == null)
        throw new ArgumentNullException("res");

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
          object v = src.GetValue(p, DBxDocValuePreferredType.Unknown);
          res.SetValue(i, v);
        }
      }
    }

    /// <summary>
    /// Копирование значений из строки данных DataRow в набор <paramref name="res"/>
    /// Значения с установленным признаком IsReadOnly в конечном наборе пропускаются
    /// </summary>
    /// <param name="src">Исходная строка данных</param>
    /// <param name="res">Записываемый набор</param>
    public static void CopyValues(DataRow src, IDBxDocValues res)
    {
      if (src == null)
        throw new ArgumentNullException("src");
      if (res == null)
        throw new ArgumentNullException("res");

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
  /// Перечислитель для реализации IDBxDocValues
  /// </summary>
  public struct DBxDocValueEnumerator : IEnumerator<DBxDocValue>
  {
    #region Конструктор

    /// <summary>
    /// Создает перечислитель
    /// </summary>
    /// <param name="docValues">Интерфейс для доступа к коллекции значений</param>
    public DBxDocValueEnumerator(IDBxDocValues docValues)
    {
      if (docValues == null)
        throw new ArgumentNullException("docValues");

      _DocValues = docValues;
      _Index = -1;
    }

    #endregion

    #region Поля

    private IDBxDocValues _DocValues;

    private int _Index;

    #endregion

    #region IEnumerator<DBxDocValue> Members

    /// <summary>
    /// Доступ к текущему полю
    /// </summary>
    public DBxDocValue Current
    {
      get { return _DocValues[_Index]; }
    }

    /// <summary>
    /// Ничего не делает
    /// </summary>
    public void Dispose()
    {
    }

    object System.Collections.IEnumerator.Current
    {
      get { return _DocValues[_Index]; }
    }

    /// <summary>
    /// Переход к следующему полю в коллекции значений
    /// </summary>
    /// <returns>True, если есть очередное значение. False, если больше нет полей</returns>
    public bool MoveNext()
    {
      _Index++;
      return _Index < _DocValues.Count;
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
  /// Реализация IDBxDocValues, позволяющая работать с массивом значений в памяти
  /// Имена полей являются чувствительными к регистру.
  /// "Серые" значения не поддерживаются. Тип хранимых значений не проверяется.
  /// </summary>
  public class DBxMemoryDocValues : IDBxDocValues, IReadOnlyObject
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект доступа к коллекции.
    /// Первоначально предолагается, что все значения коллекции равны null.
    /// </summary>
    /// <param name="names">Список имен. Массив может быть пустым, но не может содержать пустые строки</param>
    public DBxMemoryDocValues(string[] names)
    {
      if (_Names == null)
        throw new ArgumentNullException("names");
      for (int i = 0; i < names.Length; i++)
      {
        if (String.IsNullOrEmpty(names[i]))
          throw new ArgumentNullException("names[" + i.ToString() + "]");
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
    public DBxMemoryDocValues(IDBxDocValues orgValues)
    {
      if (orgValues == null)
        throw new ArgumentNullException("orgValues");
      _Names = new string[orgValues.Count];
      _Values = new object[orgValues.Count];
      for (int i = 0; i < orgValues.Count; i++)
      {
        _Names[i] = orgValues.GetName(i);
        _Values[i] = orgValues.GetValue(i, DBxDocValuePreferredType.Unknown);
      }
      _NameIndexer = new StringArrayIndexer(_Names, false);
    }

    /// <summary>
    /// Создает коллекцию с заданным списком полей.
    /// Первоначально все значения равны null.
    /// </summary>
    /// <param name="columnNames">Список имен полей</param>
    public DBxMemoryDocValues(DBxColumns columnNames)
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
    public DBxMemoryDocValues(DBxColumns columnNames, object[] columnValues)
      : this(columnNames.AsArray)
    {
      if (columnValues == null)
        throw new ArgumentNullException("columnValues");
      if (columnValues.Length != columnNames.Count)
        throw new ArgumentException("Длина массива значений не совпадает с количеством полей", "columnValues");

      for (int i = 0; i < _Names.Length; i++)
        _Values[i] = columnValues[i];
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Массив имен полей
    /// </summary>
    public string[] Names { get { return _Names; } }
    private string[] _Names;

    /// <summary>
    /// Индексатор для списка имен
    /// </summary>
    private StringArrayIndexer _NameIndexer;

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
        if (value.Length != Names.Length)
          throw new ArgumentException("Неправильная длина массива", "value");
#endif
        _Values = value;
      }
    }
    private object[] _Values;

    #endregion

    #region IDBxDocValues Members

    /// <summary>
    /// Доступ к значению по имени.
    /// Если в конструкторе не было задано такое имя, генерируется исключение.
    /// </summary>
    /// <param name="name">Имя поля</param>
    /// <returns>Объект для доступа к значению</returns>
    public DBxDocValue this[string name]
    {
      get
      {
        int p = IndexOf(name);
        if (p < 0)
          throw new ArgumentException("Набор значений не содержит имени \"" + name + "\"");
        return new DBxDocValue(this, p);
      }
    }

    /// <summary>
    /// Возвращает имя поля по индексу.
    /// Список допустимых имен полей задается в конструкторе
    /// </summary>
    /// <param name="index">Индекс поля в диапазоне от 0 до (Count-1)</param>
    /// <returns>Имя поля</returns>
    public string GetName(int index)
    {
      return _Names[index];
    }

    string IDBxDocValues.GetDisplayName(int index)
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
    /// <param name="index">Индекс поля в диапазоне от 0 до (Count-1)</param>
    /// <returns>Объект доступа к значению</returns>
    public DBxDocValue this[int index]
    {
      get
      {
        return new DBxDocValue(this, index);
      }
    }

    /// <summary>
    /// Возвращает количество полей, заданных в конструкторе
    /// </summary>
    public int Count { get { return _Names.Length; } }

    int IDBxDocValues.RowCount { get { return 1; } }

    /// <summary>
    /// Получить значение поля
    /// </summary>
    /// <param name="index">Индекс поля в диапазоне от 0 до (Count-1)</param>
    /// <param name="preferredType">Не используется</param>
    /// <returns>Значение</returns>
    public object GetValue(int index, DBxDocValuePreferredType preferredType)
    {
      return _Values[index];
    }

    /// <summary>
    /// Установить значение поля
    /// </summary>
    /// <param name="index">Индекс поля в диапазоне от 0 до (Count-1)</param>
    /// <param name="value">Новое значение</param>
    public void SetValue(int index, object value)
    {
      _Values[index] = value;
    }

    /// <summary>
    /// Возвращает true, если значение поля равно null или DBNull
    /// </summary>
    /// <param name="index">Индекс поля в диапазоне от 0 до (Count-1)</param>
    /// <returns>Признак null</returns>
    public bool IsNull(int index)
    {
      if (_Values[index] == null)
        return true;
      if (_Values[index] is DBNull)
        return true;
      return false;
    }

    bool IDBxDocValues.AllowDBNull(int index)
    {
      return true;
    }

    int IDBxDocValues.MaxLength(int index)
    {
      return 0;
    }

    /// <summary>
    /// Возвращает IsReadOnly
    /// </summary>
    /// <param name="index">Игнорируется</param>
    /// <returns>Значение свойства IsReadOnly</returns>
    public bool GetValueReadOnly(int index)
    {
      return _IsReadOnly;
    }

    bool IDBxDocValues.GetGrayed(int index)
    {
      return false;
    }

    object[] IDBxDocValues.GetValueArray(int index)
    {
      return new object[1] { _Values[index] };
    }

    void IDBxDocValues.SetValueArray(int index, object[] values)
    {
      if (values.Length != 1)
        throw new ArgumentException("values.Length must be 1", "values");

      _Values[index] = values[0];
    }

    object IDBxDocValues.GetRowValue(int valueIndex, int rowIndex)
    {
      if (rowIndex != 0)
        throw new ArgumentOutOfRangeException("rowIndex", "Row index must be 0");
      return GetValue(valueIndex, DBxDocValuePreferredType.Unknown);
    }

    void IDBxDocValues.SetRowValue(int valueIndex, int rowIndex, object value)
    {
      if (rowIndex != 0)
        throw new ArgumentOutOfRangeException("rowIndex", "Row index must be 0");
      SetValue(valueIndex, value);
    }

    #endregion

    #region IEnumerable<DBxDocValue> Members

    /// <summary>
    /// Создает перечислитель по всем полям, заданным в конструкторе
    /// </summary>
    /// <returns>Перечислитель</returns>
    public DBxDocValueEnumerator GetEnumerator()
    {
      return new DBxDocValueEnumerator(this);
    }

    IEnumerator<DBxDocValue> IEnumerable<DBxDocValue>.GetEnumerator()
    {
      return new DBxDocValueEnumerator(this);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new DBxDocValueEnumerator(this);
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Возвращает true, если коллекция была переведена в режим "только чтение" вызовом SetReadOnly()
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// Генерирует исключение, если IsReadOnly=true.
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (_IsReadOnly)
        throw new ObjectReadOnlyException();
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
