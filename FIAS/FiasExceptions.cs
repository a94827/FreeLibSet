// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace FreeLibSet.FIAS
{
  /// <summary>
  /// ���������� �������������, ���� ���������, �������� � ������ FiasDBSettings, �� ��������� ���������
  /// ��������. ��������, ��������� �������� ���������� �� ������� �����, � DBxSettings.UseHouse=false
  /// </summary>
  [Serializable]
  public class FiasDBSettingsException : InvalidOperationException
  {
    #region �����������

    /// <summary>
    /// ������� ������ ���������� � �������� ����������
    /// </summary>
    /// <param name="message">����� ���������</param>
    public FiasDBSettingsException(string message)
      : this(message, null)
    {
    }


    /// <summary>
    /// ������� ������ ���������� � �������� ���������� � ��������� �����������
    /// </summary>
    /// <param name="message">����� ���������</param>
    /// <param name="innerException">��������� ����������</param>
    public FiasDBSettingsException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    /// <summary>
    /// ��� ������ ������������ ����� ��� ���������� ��������������
    /// </summary>
    protected FiasDBSettingsException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    #endregion
  }

  /// <summary>
  /// ���������� �������������, ���� ���������, ���������� ������������ FiasDB, �� ��������� � ������������� � ���� ������
  /// </summary>
  [Serializable]
  public class FiasDBSettingsDifferentException : InvalidOperationException
  {
    #region �����������

    /// <summary>
    /// ������� ������ ���������� � �������� ����������
    /// </summary>
    /// <param name="message">����� ���������</param>
    public FiasDBSettingsDifferentException(string message)
      : this(message, null)
    {
    }


    /// <summary>
    /// ������� ������ ���������� � �������� ���������� � ��������� �����������
    /// </summary>
    /// <param name="message">����� ���������</param>
    /// <param name="innerException">��������� ����������</param>
    public FiasDBSettingsDifferentException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    /// <summary>
    /// ��� ������ ������������ ����� ��� ���������� ��������������
    /// </summary>
    protected FiasDBSettingsDifferentException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    #endregion
  }
}
