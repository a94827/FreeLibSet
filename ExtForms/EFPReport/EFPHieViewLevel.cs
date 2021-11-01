using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using FreeLibSet.Remoting;

/*
 * The BSD License
 * 
 * Copyright (c) 2006-2015, Ageyev A.V.
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

/*
 * ������������� ������ �� ���� EFPDataGridView
 */


#if XXX

namespace FreeLibSet.Forms
{
#region ������������

  /// <summary>
  /// �������� �������� HieViewLevel.Position. ���������� ����������� ������������
  /// ������� �������� � ��������� �����������
  /// </summary>
  public enum EFPHieViewLevelPosition
  {
    /// <summary>
    /// �������� �� ���������. 
    /// ���� ������� ����� �������������� � �������, �������� �������� Position=Normal
    /// </summary>
    Normal,

    /// <summary>
    /// ���� ������� �� ����� ���� �����������. ������ � Position=FixedTop 
    /// ������������� �� "�������" ����������� (�� ���� ����� ������� ������).
    /// ������� ����� ������� ���������� � ������������� ��������� �������
    /// </summary>
    FixedTop,

    /// <summary>
    /// ���� ������� �� ����� ���� �����������. ������ � Position=FixedBottom 
    /// ������������� "������" �����������.
    /// ������� ����� ������� ���������� � ������������� ��������� �������
    /// </summary>
    FixedBottom
  }

#endregion

#region ��������

  /// <summary>
  /// �������� ��� ������� HieViewLevel.UserGetText
  /// </summary>
  public class EFPHieViewLevelTextNeededEventArgs : EventArgs
  {
#region �����������

    internal EFPHieViewLevelTextNeededEventArgs()
    {
    }

#endregion

#region ��������

    /// <summary>
    /// ������, �� ������� ����� ������� ������� ����
    /// </summary>
    public DataRow Row { get { return FRow; } internal set { FRow = value; } }
    private DataRow FRow;

    /// <summary>
    /// ���� ���� �������� ��������� - ��������� ��������
    /// </summary>
    public string Text
    {
      get { return FText; }
      set
      {
        if (value == null)
          FText = String.Empty;
        else
          FText = value;
      }
    }
    private string FText;

    /// <summary>
    /// ���� ����� ������� ������ ����� ��� �������� ������. ���� �������� �� �����
    /// �����������, �� ������������ ����� ���� "�����: "+Text
    /// </summary>
    public string TotalText
    {
      get
      {
        if (String.IsNullOrEmpty(FTotalText))
          return "�����: " + Text;
        else
          return FTotalText;
      }
      set
      {
        FTotalText = value;
      }
    }
    private string FTotalText;

#endregion
  }

  public delegate void EFPHieViewLevelTextNeededEventHandler(object Sender,
    EFPHieViewLevelTextNeededEventArgs Args);

  public class EFPHieViewLevelSortKeyNeededEventArgs : EventArgs
  {
#region �����������

    internal EFPHieViewLevelSortKeyNeededEventArgs()
    {
    }

#endregion

#region ��������

    /// <summary>
    /// ������, �� ������� ����� ������� �������� ����
    /// </summary>
    public DataRow Row { get { return FRow; } internal set { FRow = value; } }
    private DataRow FRow;

    /// <summary>
    /// ���� ���� �������� ��������� - ��������� ��������, ������� ����� ��������������
    /// ��� ����������
    /// </summary>
    public string Key { get { return FKey; } set { FKey = value; } }
    private string FKey;

#endregion
  }

  public delegate void EFPHieViewLevelSortKeyNeededEventHandler(object Sender,
    EFPHieViewLevelSortKeyNeededEventArgs Args);

  public class HieViewLevelEditRowEventArgs : EventArgs
  {
#region �����������

    public HieViewLevelEditRowEventArgs(DataRow Row, bool ReadOnly)
    {
      FRow = Row;
      FReadOnly = ReadOnly;
    }

#endregion

#region ��������

    /// <summary>
    /// ������ � ��������� ���������, ��� ������� ����������� ��������������
    /// </summary>
    public DataRow Row { get { return FRow; } }
    private DataRow FRow;

    /// <summary>
    /// ���������� true, ���� �������������� ��������, � �� ��������������
    /// </summary>
    public bool ReadOnly { get { return FReadOnly; } }
    private bool FReadOnly;

    /// <summary>
    /// �������� ������ ���� ����������� � true, ���� �������������� ���� ���������
    /// � ������, ��������, ����������
    /// </summary>
    public bool Modified
    {
      get { return FModified; }
      set { FModified = value; }
    }
    private bool FModified;

#endregion
  }

  public delegate void EFPHieViewLevelEditRowEventHandler(object Sender,
    HieViewLevelEditRowEventArgs Args);

#endregion

  /// <summary>
  /// ���� ������� �������� ��� ������.
  /// ������ �������� �������������� � �������� EFPHieView.Levels.
  /// ������ EFPHieViewLevel, � ������� �� EFPHieView, �������� "������������".
  /// </summary>
  public class EFPHieViewLevel : IReadOnlyObject
  {
#region �����������

    public EFPHieViewLevel(string Name)
    {
#if DEBUG
      if (String.IsNullOrEmpty(Name))
        throw new ArgumentNullException("Name", "�� ������ ��� ������ ��������");
      if (Name.IndexOf(',') >= 0)
        throw new ArgumentException("��� ������ �������� \"" + Name + "\" �������� �������", "Name");
#endif
      FName = Name;
      FColumnNameArray = new string[] { Name };
      FVisible = true;
    }

#if XXX
    /// <summary>
    /// �������� ����� ��� ������������� � ������ ������
    /// </summary>
    /// <param name="OrgLevel"></param>
    public EFPHieViewLevel(EFPHieViewLevel OrgLevel)
      : this(OrgLevel.Name)
    {
      Mode = OrgLevel.Mode;
      FieldNameArray = OrgLevel.FieldNameArray;
      TextPrefix = OrgLevel.TextPrefix;
      RefTable = OrgLevel.RefTable;
      RefFieldName = OrgLevel.RefFieldName;
      EmptyText = OrgLevel.EmptyText;
      NotFoundText = OrgLevel.NotFoundText;
      if (OrgLevel.UserGetText != null)
        UserGetText = (EFPHieViewLevelGetTextEventHandler)(OrgLevel.UserGetText.Clone());

      FVisible = true;
      if (OrgLevel.FExtValues != null)
        FExtValues = (NamedValues)(OrgLevel.ExtValues.Clone());

      FParamEditor = OrgLevel.ParamEditor;
    }
#endif

#endregion

#region �������� ��������

    /// <summary>
    /// �������� ������ ��������.
    /// </summary>
    public string Name { get { return FName; } }
    private string FName;

    /// <summary>
    /// ���� ���������� � True, �� ������ ��������� ��� ������� ������ ����� ���������
    /// ����� ���������� ��������, � ������ ��������� ���������� �� �����
    /// ������������ ��� ����� �������� ������
    /// </summary>
    public bool SubTotalRowFirst
    {
      get { return FSubTotalRowFirst; }
      set
      {
        CheckNotReadOnly();
        FSubTotalRowFirst = value;
      }
    }
    private bool FSubTotalRowFirst;

#if XXX
    /// <summary>
    /// ������������ ���������������� ������, ��������, ��� ����������� UserGetText
    /// </summary>
    public NamedValues ExtValues
    {
      get
      {
        if (FExtValues == null)
          FExtValues = new NamedValues();
        return FExtValues;
      }
    }
    private NamedValues FExtValues;

    internal bool HasExtValues { get { return FExtValues != null; } }
#endif

    /// <summary>
    /// ��� ����, �� �������� ������� ��������.
    /// ����� ���� ������ ��������� ���� �����, ����������� ��������
    /// ��� ������� � ����� ��� � �������, ����������� FieldNameArray
    /// </summary>
    public string ColumnName
    {
      get { return String.Join(",", FColumnNameArray); }
      set
      {
#if DEBUG
        if (String.IsNullOrEmpty(value))
          throw new ArgumentNullException("FieldName");
#endif
        ColumnNameArray = value.Split(',');
      }
    }

    /// <summary>
    /// ����� ����� ��� ������ � ���� �������.
    /// ������ �������� ���� �� ���� ����
    /// </summary>
    public string[] ColumnNameArray
    {
      get { return FColumnNameArray; }
      set
      {
        CheckNotReadOnly();
        if (value == null || value.Length == 0)
          throw new ArgumentNullException("ColumnName");
        for (int i = 0; i < value.Length; i++)
        {
          if (value[i].IndexOf(',') >= 0)
            throw new ArgumentException("����� ����� � ������� �� ����� ��������� �������", "FieldNameArray");
        }
        FColumnNameArray = value;
      }
    }
    private string[] FColumnNameArray;

#endregion

#region ��������� ������ ������

    /// <summary>
    /// ��������� ������ � ������ UserDefined
    /// </summary>
    public event EFPHieViewLevelTextNeededEventHandler TextNeeded;

    private EFPHieViewLevelTextNeededEventArgs TextNeededArgs;

    /// <summary>
    /// ������� �����. ��������� ���������� �������� ��� ������.
    /// ������ ������� ������. ����������� ������� �� ������
    /// </summary>
    /// <param name="Row">������ ������</param>
    /// <returns></returns>
    public string GetRowText(DataRow Row)
    {
      return GetRowText(Row, false);
    }

    public virtual string GetRowText(DataRow Row, bool TotalRow)
    {
      if (Row == null)
        throw new ArgumentNullException("Row");

      if (TextNeeded == null)
      {
        // ���������� �������� ����
        object v = Row[ColumnName];
        if (v is DateTime)
          return ((DateTime)v).ToString("d");
        string s = v.ToString();
        return s;
      }
      else
      {
        // ���������� ���������������� ����������

        if (TextNeededArgs == null)
          TextNeededArgs = new EFPHieViewLevelTextNeededEventArgs();
        TextNeededArgs.Row = Row;
        TextNeededArgs.Text = "???";
        TextNeeded(this, TextNeededArgs);
        if (TotalRow)
          return TextNeededArgs.TotalText;
        else
          return TextNeededArgs.Text;
      }
    }

#endregion

#region ���� ��� ���������� �������

    /// <summary>
    /// ������� ���������� ��� ���������� ������ ��� ����������� ������� ����������
    /// ����� ������ ������. ���� ���������� �� �����, �� ������ ����������� ��
    /// ���������� �������� ���� ��������� ������������� ���� �����
    /// ���������� ����� ����������� �������������� ����������
    /// �� ������ ����� �������� �������, ������� �������� ������-����� ��� ����������
    /// </summary>
    public event EFPHieViewLevelSortKeyNeededEventHandler SortKeyNeeded;

    private EFPHieViewLevelSortKeyNeededEventArgs SortKeyNeededArgs;

    /// <summary>
    /// �������� ����� ��� ���������� ����� ������ ������
    /// </summary>
    /// <param name="Row"></param>
    /// <returns></returns>
    public virtual string GetRowSortKey(DataRow Row)
    {
      if (Row == null)
        throw new ArgumentNullException("Row");

      if (SortKeyNeeded == null)
      {
        if (ColumnNameArray.Length == 1)
          return GetColumnKeyValue(Row, ColumnNameArray[0]);
        else
        {
          string[] a = new string[ColumnNameArray.Length];
          for (int i = 0; i < ColumnNameArray.Length; i++)
            a[i] = GetColumnKeyValue(Row, ColumnNameArray[i]);
          return String.Join("|", a);
        }
      }
      else
      {
        if (SortKeyNeededArgs == null)
          SortKeyNeededArgs = new EFPHieViewLevelSortKeyNeededEventArgs();

        SortKeyNeededArgs.Row = Row;
        SortKeyNeededArgs.Key = String.Empty;
        SortKeyNeeded(this, SortKeyNeededArgs);
        return SortKeyNeededArgs.Key;
      }
    }

    private static string GetColumnKeyValue(DataRow Row, string ColumnName)
    {
      object v = Row[ColumnName];
      if (v is DBNull)
        return String.Empty;

      Type t = Row.Table.Columns[ColumnName].DataType;
      if (t == typeof(string))
        return ((string)v).ToUpperInvariant();
      if (t == typeof(DateTime))
        return ((DateTime)v).ToString("s");
      if (t == typeof(Int32))
        return BitConverter.ToString(BitConverter.GetBytes((Int32)v));
      // !!! ��������� ���� ������
      throw new NotSupportedException("���������� ��� ���� ���� " + t.ToString() + " �� �����������");
    }

#endregion

#region ��������, ������������ ���������� �����������

    /// <summary>
    /// ��� ������ ��� ����������� � ��������� ��������. ���� �������� ��
    /// ����������� ����, ������������ �������� �������� NamePart
    /// </summary>
    public string DisplayName
    {
      get
      {
        if (String.IsNullOrEmpty(FDisplayName))
          return Name;
        else
          return FDisplayName;
      }
      set
      {
        CheckNotReadOnly();
        FDisplayName = value;
      }
    }
    private string FDisplayName;

    /// <summary>
    /// ����������� ��� ������, ������������ ���������� �����������
    /// </summary>
    public string ImageKey
    {
      get
      {
        if (String.IsNullOrEmpty(FImageKey))
          return "Item";
        else
          return FImageKey;
      }
      set
      {
        CheckNotReadOnly();
        FImageKey = value;
      }
    }
    private string FImageKey;

    /// <summary>
    /// ������������ ���������� �����������. ���� �������� ����������� � false,
    /// �� ���� ������� �������� �� ������������ � ������� � �� ����� ���� ������.
    /// �������� �� ��������� - true. �������� ����� ������������, ����� �������
    /// ������� ������� �� ������ ���������� ������. 
    /// ����� ��������� �������� ������� ��������� ����������� ������ ���� ���������
    /// </summary>
    public bool Visible
    {
      get { return FVisible; }
      set
      {
        FVisible = value;
        if (!value)
          Requred = false;
      }
    }
    private bool FVisible;

    /// <summary>
    /// ������������ ���������� �����������. ���� �������� ����������� � true,
    /// �� ������� �������� ������������ (������ ������� � �� ����� ���� ����)
    /// ����� ��������� �������� ������� ��������� ����������� ������ ���� ���������
    /// </summary>
    public bool Requred
    {
      get { return FRequred; }
      set
      {
        CheckNotReadOnly();
        FRequred = value;
        if (value)
          Visible = true;
      }
    }
    private bool FRequred;

    /// <summary>
    /// ����������� ������������ ������� ���������� � ��������� �����������
    /// ����� ��������� �������� ������� ��������� ����������� ������ ���� ���������
    /// </summary>
    public EFPHieViewLevelPosition Position { get { return FPosition; } set { FPosition = value; } }
    private EFPHieViewLevelPosition FPosition;

    /// <summary>
    /// ������� ����������� �� ��������� ��� ������� ������
    /// �������� �� ��������� - false (������� �� �������)
    /// �������� ������������, ec�� Required=true ��� Visible=false
    /// </summary>
    public bool DefaultSelected { get { return FDefaultSelected; } set { FDefaultSelected = value; } }
    private bool FDefaultSelected;

    /// <summary>
    /// �������� ������������� ��������� (����������) ��������� ������
    /// ���� �������� �����������, �� � ������� ��������� �������� ������ "�������������",
    /// � ����� � ��������� ������ DisplayName ��������� �������������� �����
    /// </summary>
    public EFPHieViewLevelParamEditor ParamEditor { get { return FParamEditor; } set { FParamEditor = value; } }
    private EFPHieViewLevelParamEditor FParamEditor;

#endregion

#region �������� � ������, ������������ ��� �������������� � ������

    /// <summary>
    /// ������� ���������� ������� HieViewHandler.EditReportRow() ��� ��������� ��������������
    /// ������ ������, ��� ������� ������ � ���������� ������
    /// </summary>
    public event EFPHieViewLevelEditRowEventHandler EditRow;

    public bool OnEditRow(DataRow Row, bool ReadOnly)
    {
      if (EditRow == null)
      {
        EFPApp.ShowTempMessage("�������������� ��� ������ \"" + DisplayName + "\" �� �������������");
        return false;
      }

      HieViewLevelEditRowEventArgs Args = new HieViewLevelEditRowEventArgs(Row, ReadOnly);
      EditRow(this, Args);
      return Args.Modified;
    }

#endregion

#region ������ ������

    public override string ToString()
    {
      return Name;
    }

#endregion

#region IReadOnlyObject Members

    public bool IsReadOnly { get { return FIsReadOnly; } }
    private bool FIsReadOnly;

    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    public void SetReadOnly()
    {
      FIsReadOnly = true;
    }

#endregion
  }


}
#endif
