// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;

namespace FreeLibSet.FIAS
{
  /// <summary>
  /// ��������� ������ �������� ��������
  /// </summary>
  [Serializable]
  public sealed class FiasAddressSearchParams
  {
    #region �����������

    /// <summary>
    /// ������� ����� ����������
    /// </summary>
    public FiasAddressSearchParams()
    {
      _ActualOnly = true;
      _StartAddress = new FiasAddress();
    }

    #endregion

    #region ��������

    /// <summary>
    /// ����� � ������������ ��������� �������
    /// </summary>
    public string Text { get { return _Text; } set { _Text = value; } }
    private string _Text;

    /// <summary>
    /// ������, ����� ������� ����������� �����.
    /// ���� null, �� ����� ���� �������
    /// </summary>
    public FiasLevel[] Levels { get { return _Levels; } set { _Levels = value; } }
    private FiasLevel[] _Levels;

    /// <summary>
    /// ���� true (�� ���������), �� ��������� ����� ������ ����� ���������� �������.
    /// �������� false �� ����� ��������������, ���� FiasDBConfig.UseHistory=false.
    /// </summary>
    public bool ActualOnly { get { return _ActualOnly; } set { _ActualOnly = value; } }
    private bool _ActualOnly;

    /// <summary>
    /// ��������� ����� (��������, �����), � ������� ����� ��������� ����� (����)
    /// </summary>
    public FiasAddress StartAddress { get { return _StartAddress; } set { _StartAddress = value; } }
    private FiasAddress _StartAddress;

    /// <summary>
    /// ���������� true, ���� ��������� �� ������
    /// </summary>
    public bool IsEmpty { get { return String.IsNullOrEmpty(Text); } }

    #endregion

    #region ��������� �������������

    /// <summary>
    /// ��������� ������������� ���������� ������
    /// </summary>
    /// <returns>��������� �������������</returns>
    public override string ToString()
    {
      if (IsEmpty)
        return "��������� ������ �� ������";

      StringBuilder sb = new StringBuilder();
      sb.Append("������ \"");
      sb.Append(_Text);
      sb.Append("\"");
      sb.Append(", ������: ");
      if (_Levels == null)
        sb.Append(" ��� �������� �������");
      else
      {
        for (int i = 0; i < _Levels.Length; i++)
        {
          if (i > 0)
            sb.Append(", ");
          sb.Append(FiasEnumNames.ToString(_Levels[i], false));
        }
      }
      if (_ActualOnly)
        sb.Append(", ������ ����������");
      else
        sb.Append(", ������� ������������");
      sb.Append(", �: ");
      if (_StartAddress.IsEmpty)
        sb.Append("���� ����������");
      else
        sb.Append(_StartAddress.ToString());

      return sb.ToString();
    }

    #endregion
  }
}
