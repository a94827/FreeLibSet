// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Config;
using FreeLibSet.Collections;
using FreeLibSet.Core;

namespace FreeLibSet.Data.Docs
{
  /// <summary>
  /// Доступ к конфигурационным данным как к строке данных.
  /// Дает доступ к полям для одного узла CfgPart. 
  /// Для доступа к дочерним узлам требуется отдельный экземпляр XmlCfgDocValues.
  /// "Серые" значения не поддерживаются.
  /// </summary>
  /// <remarks>
  /// В отличие от доступа к полям документов и DataRowDocValues, этот объект не имеет доступа к
  /// списку полей до того, как выполнено обращение к DocValues
  /// </remarks>
  public class XmlCfgDocValues : IDBxDocValues
  {
    #region Конструктор

    /// <summary>
    /// Создает объект доступа для секции
    /// </summary>
    /// <param name="part">Секция конфигурации</param>
    public XmlCfgDocValues(CfgPart part)
      : this(part, false)
    {
    }

    /// <summary>
    /// Создает объект доступа для секции
    /// </summary>
    /// <param name="part">Секция конфигурации</param>
    /// <param name="isReadOnly">Если true, то будет разрешено только чтение значений, а не запись</param>
    public XmlCfgDocValues(CfgPart part, bool isReadOnly)
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
    private CfgPart _Part;

    #endregion

    #region Доступ к IDBxDocValues

    /*
     * DBxDocValue является структурой, которую не обязательно хранить, а создавать при каждом обращении.
     * DBxDocValue хранит индекс поля, поэтому словарь должен быстро определять имя по индексу
     * (можно обычный List).
     * При обращении к this нужно, наоборот, быстро находить индекс по имени, чтобы не добавлять новые записи.
     * Используем BidirectionalDictionary
     */

    private BidirectionalDictionary<string, int> _Items;

    /// <summary>
    /// Возвращает структуру для доступа к значению
    /// </summary>
    /// <param name="name">Имя параметра</param>
    /// <returns>Доступ к значению</returns>
    public DBxDocValue this[string name]
    {
      get
      {
        int p;
        if (!_Items.TryGetValue(name, out p))
        {
          if (String.IsNullOrEmpty(name))
            throw new ArgumentNullException("name");
          p = _Items.Count;
          _Items.Add(name, p);
        }
        return new DBxDocValue(this, p);
      }
    }

    /// <summary>
    /// Получить имя параметра по индексу
    /// </summary>
    /// <param name="index">Условный индекс параметра</param>
    /// <returns>Имя параметра</returns>
    private string GetName(int index)
    {
      string Name;
      _Items.TryGetKey(index, out Name);
      return Name;
    }


    /// <summary>
    /// Получить имя параметра по индексу
    /// </summary>
    /// <param name="index">Условный индекс параметра</param>
    /// <returns>Имя параметра</returns>
    string IDBxDocValues.GetName(int index)
    {
      return GetName(index);
    }

    /// <summary>
    /// Возвращает GetName(), т.к. в CfgPart нет отображаемых названий параметров
    /// </summary>
    /// <param name="index">Условный индекс параметра</param>
    /// <returns>Имя параметра</returns>
    string IDBxDocValues.GetDisplayName(int index)
    {
      return GetName(index);
    }

    /// <summary>
    /// Поиск параметра
    /// </summary>
    /// <param name="name">Имя параметра</param>
    /// <returns>Индекс или (-1)</returns>
    int IDBxDocValues.IndexOf(string name)
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
    DBxDocValue IDBxDocValues.this[int index]
    {
      get { return new DBxDocValue(this, index); }
    }

    /// <summary>
    /// Возвращает количество поименованных параметров
    /// </summary>
    int IDBxDocValues.Count { get { return _Items.Count; } }

    int IDBxDocValues.RowCount { get { return 1; } }

    object IDBxDocValues.GetValue(int Index, DBxDocValuePreferredType preferredType)
    {
      switch (preferredType)
      {
        case DBxDocValuePreferredType.Int32: return Part.GetNullableInt(GetName(Index));
        case DBxDocValuePreferredType.Single: return Part.GetNullableSingle(GetName(Index));
        case DBxDocValuePreferredType.Double: return Part.GetNullableDouble(GetName(Index));
        case DBxDocValuePreferredType.Decimal: return Part.GetNullableDecimal(GetName(Index));
        case DBxDocValuePreferredType.Boolean: return Part.GetNullableBool(GetName(Index));
        case DBxDocValuePreferredType.DateTime: return Part.GetNullableDateTime(GetName(Index));
        case DBxDocValuePreferredType.TimeSpan: return Part.GetNullableTimeSpan(GetName(Index));
        default: return Part.GetString(GetName(Index));
      }
    }

    void IDBxDocValues.SetValue(int index, object value)
    {
      if (value == null || value is DBNull)
        Part.Remove(GetName(index));
      else if (value is Int32)
        Part.SetInt(GetName(index), DataTools.GetInt(value));
      else if (value is Single)
        Part.SetSingle(GetName(index), DataTools.GetSingle(value));
      else if (value is Double)
        Part.SetDouble(GetName(index), DataTools.GetDouble(value));
      else if (value is Decimal)
        Part.SetDecimal(GetName(index), DataTools.GetDecimal(value));
      else if (value is Boolean)
        Part.SetBool(GetName(index), DataTools.GetBool(value));
      else if (value is DateTime)
        Part.SetNullableDateTime(GetName(index), DataTools.GetNullableDateTime(value));
      else if (value is TimeSpan)
        Part.SetTimeSpan(GetName(index), (TimeSpan)value);
      else
        Part.SetString(GetName(index), DataTools.GetString(value));
    }

    bool IDBxDocValues.IsNull(int index)
    {
      return Part.GetString(GetName(index)).Length == 0;
    }

    bool IDBxDocValues.AllowDBNull(int index)
    {
      return true;
    }

    int IDBxDocValues.MaxLength(int index)
    {
      return -1;
    }

    bool IDBxDocValues.GetValueReadOnly(int index)
    {
      return false;
    }

    bool IDBxDocValues.GetGrayed(int index)
    {
      return false;
    }

    object IDBxDocValues.GetComplexValue(int index)
    {
      return ((IDBxDocValues)this).GetValue(index, DBxDocValuePreferredType.Unknown);
    }

    void IDBxDocValues.SetComplexValue(int index, object value)
    {
      ((IDBxDocValues)this).SetValue(index, value);
    }

    #endregion

    #region IEnumerable<DBxDocValue> Members

    IEnumerator<DBxDocValue> IEnumerable<DBxDocValue>.GetEnumerator()
    {
      return new DBxDocValueEnumerator(this);
    }

    #endregion

    #region IEnumerable Members

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new DBxDocValueEnumerator(this);
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Возвращает true, если разрешен только просмотр значений, но не изменение
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// Генерирует исключение, если IsReadOnly=true
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    #endregion

    #region Дочерние объекты

    /// <summary>
    /// Возвращает объект для доступа к дочерней секции с заданным именем
    /// </summary>
    /// <param name="name">Имя дочерней секции конфигурации</param>
    /// <returns>Объект доступа к значениям</returns>
    public XmlCfgDocValues GetChild(string name)
    {
      if (_Children == null)
        _Children = new Dictionary<string, XmlCfgDocValues>();

      XmlCfgDocValues Child;
      if (!_Children.TryGetValue(name, out Child))
      {
        CfgPart Part2 = Part.GetChild(name, true);
        Child = new XmlCfgDocValues(Part2, IsReadOnly);
        _Children.Add(name, Child);
      }
      return Child;
    }

    private Dictionary<string, XmlCfgDocValues> _Children;

    #endregion
  }
}
