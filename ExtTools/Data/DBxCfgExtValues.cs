// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Config;
using FreeLibSet.Collections;
using FreeLibSet.Core;

namespace FreeLibSet.Data
{
  /// <summary>
  /// Доступ к конфигурационным данным как к строке данных.
  /// Дает доступ к полям для одного узла <see cref="CfgPart"/>. 
  /// Для доступа к дочерним узлам требуется отдельный экземпляр <see cref="DBxCfgExtValues"/>.
  /// "Серые" значения не поддерживаются.
  /// В отличие от доступа к полям документов и <see cref="DBxDataRowExtValues"/>, этот объект не имеет доступа к
  /// списку полей до того, как к ним выполнено обращение по имени.
  /// </summary>
  public class DBxCfgExtValues : IDBxExtValues
  {
    #region Конструктор

    /// <summary>
    /// Создает объект доступа для секции
    /// </summary>
    /// <param name="part">Секция конфигурации</param>
    public DBxCfgExtValues(CfgPart part)
      : this(part, false)
    {
    }

    /// <summary>
    /// Создает объект доступа для секции
    /// </summary>
    /// <param name="part">Секция конфигурации</param>
    /// <param name="isReadOnly">Если true, то будет разрешено только чтение значений, а не запись</param>
    public DBxCfgExtValues(CfgPart part, bool isReadOnly)
    {
      if (part == null)
        throw new ArgumentNullException("part");
      _Part = part;
      _IsReadOnly = isReadOnly;

      _Items = new BidirectionalDictionary<string, int>();
    }

    #endregion

    #region Секции

    /// <summary>
    /// Секция конфигурации, доступ к значениям которой выполняется объектом.
    /// </summary>
    public CfgPart Part { get { return _Part; } }
    private readonly CfgPart _Part;

    #endregion

    #region Доступ к IDBxExtValues

    /*
     * DBxExtValue является структурой, которую не обязательно хранить, а создавать при каждом обращении.
     * DBxExtValue хранит индекс поля, поэтому словарь должен быстро определять имя по индексу
     * (можно обычный List).
     * При обращении к this нужно, наоборот, быстро находить индекс по имени, чтобы не добавлять новые записи.
     * Используем BidirectionalDictionary
     */

    private readonly BidirectionalDictionary<string, int> _Items;

    /// <summary>
    /// Возвращает структуру для доступа к значению
    /// </summary>
    /// <param name="name">Имя параметра</param>
    /// <returns>Доступ к значению</returns>
    public DBxExtValue this[string name]
    {
      get
      {
        int p;
        if (!_Items.TryGetValue(name, out p))
        {
          if (String.IsNullOrEmpty(name))
            throw ExceptionFactory.ArgStringIsNullOrEmpty("name");
          p = _Items.Count;
          _Items.Add(name, p);
        }
        return new DBxExtValue(this, p);
      }
    }

    /// <summary>
    /// Получить имя параметра по индексу
    /// </summary>
    /// <param name="index">Условный индекс параметра</param>
    /// <returns>Имя параметра</returns>
    private string GetName(int index)
    {
      string name;
      _Items.TryGetKey(index, out name);
      return name;
    }


    /// <summary>
    /// Получить имя параметра по индексу
    /// </summary>
    /// <param name="index">Условный индекс параметра</param>
    /// <returns>Имя параметра</returns>
    string IDBxExtValues.GetName(int index)
    {
      return GetName(index);
    }

    /// <summary>
    /// Возвращает <see cref="GetName(int)"/>, т.к. в <see cref="CfgPart"/> нет отображаемых названий параметров
    /// </summary>
    /// <param name="index">Условный индекс параметра</param>
    /// <returns>Имя параметра</returns>
    string IDBxExtValues.GetDisplayName(int index)
    {
      return GetName(index);
    }

    /// <summary>
    /// Поиск параметра
    /// </summary>
    /// <param name="name">Имя параметра</param>
    /// <returns>Индекс или (-1)</returns>
    int IDBxExtValues.IndexOf(string name)
    {
      int p;
      if (_Items.TryGetValue(name, out p))
        return p;
      else
        return -1;
    }

    /// <summary>
    /// Доступ к значению по индексу
    /// </summary>
    /// <param name="index">Условный индекс параметра</param>
    /// <returns>Объект доступа к значению</returns>
    DBxExtValue IDBxExtValues.this[int index]
    {
      get { return new DBxExtValue(this, index); }
    }

    /// <summary>
    /// Возвращает количество поименованных параметров
    /// </summary>
    int IDBxExtValues.Count { get { return _Items.Count; } }

    int IDBxExtValues.RowCount { get { return 1; } }

    object IDBxExtValues.GetValue(int Index, DBxExtValuePreferredType preferredType)
    {
      switch (preferredType)
      {
        case DBxExtValuePreferredType.Int32: return Part.GetNullableInt32(GetName(Index));
        case DBxExtValuePreferredType.Int64: return Part.GetNullableInt64(GetName(Index));
        case DBxExtValuePreferredType.Single: return Part.GetNullableSingle(GetName(Index));
        case DBxExtValuePreferredType.Double: return Part.GetNullableDouble(GetName(Index));
        case DBxExtValuePreferredType.Decimal: return Part.GetNullableDecimal(GetName(Index));
        case DBxExtValuePreferredType.Boolean: return Part.GetNullableBoolean(GetName(Index));
        case DBxExtValuePreferredType.DateTime: return Part.GetNullableDateTime(GetName(Index));
        case DBxExtValuePreferredType.TimeSpan: return Part.GetNullableTimeSpan(GetName(Index));
        case DBxExtValuePreferredType.Guid: return Part.GetNullableGuid(GetName(Index));
        default: return Part.GetString(GetName(Index));
      }
    }

    void IDBxExtValues.SetValue(int index, object value)
    {
      if (value == null || value is DBNull)
        Part.Remove(GetName(index));
      else if (value is Int32)
        Part.SetInt32(GetName(index), DataTools.GetInt32(value));
      else if (value is Single)
        Part.SetSingle(GetName(index), DataTools.GetSingle(value));
      else if (value is Double)
        Part.SetDouble(GetName(index), DataTools.GetDouble(value));
      else if (value is Decimal)
        Part.SetDecimal(GetName(index), DataTools.GetDecimal(value));
      else if (value is Boolean)
        Part.SetBoolean(GetName(index), DataTools.GetBoolean(value));
      else if (value is DateTime)
        Part.SetNullableDateTime(GetName(index), DataTools.GetNullableDateTime(value));
      else if (value is TimeSpan)
        Part.SetTimeSpan(GetName(index), (TimeSpan)value);
      else
        Part.SetString(GetName(index), DataTools.GetString(value));
    }

    bool IDBxExtValues.IsNull(int index)
    {
      return Part.GetString(GetName(index)).Length == 0;
    }

    bool IDBxExtValues.AllowDBNull(int index)
    {
      return true;
    }

    int IDBxExtValues.MaxLength(int index)
    {
      return -1;
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
      return new object[1] { ((IDBxExtValues)this).GetValue(index, DBxExtValuePreferredType.Unknown) };
    }

    void IDBxExtValues.SetValueArray(int index, object[] values)
    {
      if (values.Length != 1)
        throw new ArgumentException("values.Length must be 1", "values");

      ((IDBxExtValues)this).SetValue(index, values[0]);
    }

    object IDBxExtValues.GetRowValue(int valueIndex, int rowIndex)
    {
      if (rowIndex != 0)
        throw new ArgumentOutOfRangeException("rowIndex", "Row index must be 0");
      return ((IDBxExtValues)this).GetValue(valueIndex, DBxExtValuePreferredType.Unknown);
    }

    void IDBxExtValues.SetRowValue(int valueIndex, int rowIndex, object value)
    {
      if (rowIndex != 0)
        throw new ArgumentOutOfRangeException("rowIndex", "Row index must be 0");
      ((IDBxExtValues)this).SetValue(valueIndex, value);
    }

    #endregion

    #region IEnumerable<DBxExtValue> Members

    IEnumerator<DBxExtValue> IEnumerable<DBxExtValue>.GetEnumerator()
    {
      return new DBxExtValueEnumerator(this);
    }

    #endregion

    #region IEnumerable Members

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new DBxExtValueEnumerator(this);
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Возвращает true, если разрешен только просмотр значений, но не изменение
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private readonly bool _IsReadOnly;

    /// <summary>
    /// Генерирует исключение, если <see cref="IsReadOnly"/>=true
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw ExceptionFactory.ObjectReadOnly(this);
    }

    #endregion

    #region Дочерние объекты

    /// <summary>
    /// Возвращает объект для доступа к дочерней секции с заданным именем
    /// </summary>
    /// <param name="name">Имя дочерней секции конфигурации</param>
    /// <returns>Объект доступа к значениям</returns>
    public DBxCfgExtValues GetChild(string name)
    {
      if (_Children == null)
        _Children = new Dictionary<string, DBxCfgExtValues>();

      DBxCfgExtValues child;
      if (!_Children.TryGetValue(name, out child))
      {
        CfgPart part2 = Part.GetChild(name, true);
        child = new DBxCfgExtValues(part2, IsReadOnly);
        _Children.Add(name, child);
      }
      return child;
    }

    private Dictionary<string, DBxCfgExtValues> _Children;

    #endregion
  }
}
