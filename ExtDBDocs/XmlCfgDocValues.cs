using System;
using System.Collections.Generic;
using System.Text;
using AgeyevAV.Config;

namespace AgeyevAV.ExtDB.Docs
{
  /// <summary>
  /// ������ � ���������������� ������ ��� � ������ ������.
  /// ���� ������ � ����� ��� ������ ���� CfgPart. 
  /// ��� ������� � �������� ����� ��������� ��������� ��������� XmlCfgDocValues.
  /// "�����" �������� �� ��������������.
  /// </summary>
  /// <remarks>
  /// � ������� �� ������� � ����� ���������� � DataRowDocValues, ���� ������ �� ����� ������� �
  /// ������ ����� �� ����, ��� ��������� ��������� � DocValues
  /// </remarks>
  public class XmlCfgDocValues : IDBxDocValues
  {
    #region �����������

    /// <summary>
    /// ������� ������ ������� ��� ������
    /// </summary>
    /// <param name="part">������ ������������</param>
    public XmlCfgDocValues(CfgPart part)
      : this(part, false)
    {
    }

    /// <summary>
    /// ������� ������ ������� ��� ������
    /// </summary>
    /// <param name="part">������ ������������</param>
    /// <param name="isReadOnly">���� true, �� ����� ��������� ������ ������ ��������, � �� ������</param>
    public XmlCfgDocValues(CfgPart part, bool isReadOnly)
    {
      if (part == null)
        throw new ArgumentNullException("part");
      _Part = part;
      _IsReadOnly = isReadOnly;

      _Items = new BidirectionalDictionary<string, int>();
    }

    #endregion

    #region ������

    /// <summary>
    /// ������ ������������, ������ � ��������� ������� ����������� ��������.
    /// </summary>
    public CfgPart Part { get { return _Part; } }
    private CfgPart _Part;

    #endregion

    #region ������ � IDBxDocValues

    /*
     * DBxDocValue �������� ����������, ������� �� ����������� �������, � ��������� ��� ������ ���������.
     * DBxDocValue ������ ������ ����, ������� ������� ������ ������ ���������� ��� �� �������
     * (����� ������� List).
     * ��� ��������� � this �����, ��������, ������ �������� ������ �� �����, ����� �� ��������� ����� ������.
     * ���������� BidirectionalDictionary
     */

    private BidirectionalDictionary<string, int> _Items;

    /// <summary>
    /// ���������� ��������� ��� ������� � ��������
    /// </summary>
    /// <param name="name">��� ���������</param>
    /// <returns>������ � ��������</returns>
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
    /// �������� ��� ��������� �� �������
    /// </summary>
    /// <param name="index">�������� ������ ���������</param>
    /// <returns>��� ���������</returns>
    private string GetName(int index)
    {
      string Name;
      _Items.TryGetKey(index, out Name);
      return Name;
    }


    /// <summary>
    /// �������� ��� ��������� �� �������
    /// </summary>
    /// <param name="index">�������� ������ ���������</param>
    /// <returns>��� ���������</returns>
    string IDBxDocValues.GetName(int index)
    {
      return GetName(index);
    }

    /// <summary>
    /// ���������� GetName(), �.�. � CfgPart ��� ������������ �������� ����������
    /// </summary>
    /// <param name="index">�������� ������ ���������</param>
    /// <returns>��� ���������</returns>
    string IDBxDocValues.GetDisplayName(int index)
    {
      return GetName(index);
    }

    /// <summary>
    /// ����� ���������
    /// </summary>
    /// <param name="name">��� ���������</param>
    /// <returns>������ ��� (-1)</returns>
    int IDBxDocValues.IndexOf(string name)
    {
      int p;
      if (_Items.TryGetValue(name, out p))
        return p;
      else
        return -1;
    }

    /// <summary>
    /// ������ � �������� �� �������
    /// </summary>
    /// <param name="index">�������� ������ ���������</param>
    /// <returns>������ ������� � ��������</returns>
    DBxDocValue IDBxDocValues.this[int index]
    {
      get { return new DBxDocValue(this, index); }
    }

    /// <summary>
    /// ���������� ���������� ������������� ����������
    /// </summary>
    int IDBxDocValues.Count { get { return _Items.Count; } }

    int IDBxDocValues.DocCount { get { return 1; } }

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
    /// ���������� true, ���� �������� ������ �������� ��������, �� �� ���������
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// ���������� ����������, ���� IsReadOnly=true
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    #endregion

    #region �������� �������

    /// <summary>
    /// ���������� ������ ��� ������� � �������� ������ � �������� ������
    /// </summary>
    /// <param name="name">��� �������� ������ ������������</param>
    /// <returns>������ ������� � ���������</returns>
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
