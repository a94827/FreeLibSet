// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;

namespace FreeLibSet.Data.Docs
{
  #region ������������ DBxDocTypeRefType

  /// <summary>
  /// ��� ������ ����� ����������� (�������� DBxDocTypeRefInfo.RefType)
  /// </summary>
  public enum DBxDocTypeRefType
  {
    /// <summary>
    /// ������� ��������� ����
    /// </summary>
    Column,

    /// <summary>
    /// ���������� ������ VTReference
    /// </summary>
    VTRefernce
  }

  #endregion

  /// <summary>
  /// �������� ��������� ����������� ����� ����� ������ ����������
  /// </summary>
  public sealed class DBxDocTypeRefInfo
  {
    #region ��������

    /// <summary>
    /// ��������, � ������� ���� ��������� ���� ��� ���� ����������� �� ��������� �����
    /// </summary>
    public DBxDocType FromDocType { get { return _FromDocType; } internal set { _FromDocType = value; } }
    private DBxDocType _FromDocType;

    /// <summary>
    /// �����������, � ������� ���� ��������� ����. ���� ��������� ���� ��������� � �������� ���������, ��
    /// �������� �������� null
    /// </summary>
    public DBxSubDocType FromSubDocType { get { return _FromSubDocType; } internal set { _FromSubDocType = value; } }
    private DBxSubDocType _FromSubDocType;

    /// <summary>
    /// ��������, �� ������� ���� ������ ��� �������� �����������, �� ������� ���� ������
    /// </summary>
    public DBxDocType ToDocType { get { return _ToDocType; } internal set { _ToDocType = value; } }
    private DBxDocType _ToDocType;

    /// <summary>
    /// �����������, �� ������� ���� ������.
    /// ���� ������ ���� �� �������� ��������, �� �������� �������� null
    /// </summary>
    public DBxSubDocType ToSubDocType { get { return _ToSubDocType; } internal set { _ToSubDocType = value; } }
    private DBxSubDocType _ToSubDocType;

    /// <summary>
    /// ��� ������ (��������� ���� ��� ���������� ������)
    /// </summary>
    public DBxDocTypeRefType RefType
    {
      get
      {
        if (_FromVTReference == null)
          return DBxDocTypeRefType.Column;
        else
          return DBxDocTypeRefType.VTRefernce;
      }
    }

    /// <summary>
    /// ��������� ���� � FromDocType ��� FromSubDocType.
    /// �������� ������������ ���� RefType=Column
    /// </summary>
    public DBxColumnStruct FromColumn { get { return _FromColumn; } internal set { _FromColumn = value; } }
    private DBxColumnStruct _FromColumn;

    /// <summary>
    /// ���������� ������ � FromDocType ��� FromSubDocType.
    /// �������� ������������ ���� RefType=VTReference
    /// </summary>
    public DBxVTReference FromVTReference { get { return _FromVTReference; } internal set { _FromVTReference = value; } }
    private DBxVTReference _FromVTReference;

    #endregion

    #region ��������������� ��������

    /// <summary>
    /// ���������� �������� ToDocType ��� ToSubDocType
    /// </summary>
    public DBxDocTypeBase ToDocTypeBase { get { return ToSubDocType == null ? (DBxDocTypeBase)ToDocType : (DBxDocTypeBase)ToSubDocType; } }

    /// <summary>
    /// ���������� �������� FromDocType ��� FromSubDocType
    /// </summary>
    public DBxDocTypeBase FromDocTypeBase { get { return FromSubDocType == null ? (DBxDocTypeBase)FromDocType : (DBxDocTypeBase)FromSubDocType; } }

    #endregion

    #region ToString()

    /// <summary>
    /// ��������� ������������� (��� �������)
    /// </summary>
    /// <returns>��������� �������������</returns>
    public override string ToString()
    {
      if (_FromDocType == null)
        return "Empty";
      string s1;
      if (_FromSubDocType == null)
        s1 = _FromDocType.SingularTitle;
      else
        s1 = _FromSubDocType.SingularTitle;
      string s2;
      switch (RefType)
      {
        case DBxDocTypeRefType.Column:
          s2 = _FromColumn.ColumnName;
          break;
        case DBxDocTypeRefType.VTRefernce:
          s2 = "VT:" + _FromVTReference.Name;
          break;
        default:
          s2 = "???";
          break;
      }
      return s1 + "." + s2;
    }

    #endregion

    #region ����������� ������ ��� DBxDocTypes

    internal static DBxDocTypeRefInfo[] GetToDocTypeRefs(DBxDocTypes docTypes, string docTypeName)
    {
      if (String.IsNullOrEmpty(docTypeName))
        throw new ArgumentNullException("docTypeName");
      DBxDocType ToDocType = docTypes[docTypeName];
      if (ToDocType == null)
        throw new ArgumentException("����������� ��� ���������: \"" + docTypeName + "\"", "docTypeName");


      List<DBxDocTypeRefInfo> lst = new List<DBxDocTypeRefInfo>();
      for (int i = 0; i < docTypes.Count; i++)
      {
        DBxDocType FromDocType = docTypes[i];
        // ���� ������ �� ��������� ���������
        GetToDocTypeRefs2(lst, ToDocType, FromDocType, FromDocType);
        // ���� ������ �� �������������
        for (int j = 0; j < FromDocType.SubDocs.Count; j++)
          GetToDocTypeRefs2(lst, ToDocType, FromDocType, FromDocType.SubDocs[j]);
      }

      return lst.ToArray();
    }

    private static void GetToDocTypeRefs2(List<DBxDocTypeRefInfo> lst, DBxDocType toDocType, DBxDocType fromDocType, DBxDocTypeBase fromDTB)
    {
      // ������ �� �������� ��������
      GetToDocTypeRefs3(lst, toDocType, toDocType, fromDocType, fromDTB);
      // ������ �� ������������
      for (int i = 0; i < toDocType.SubDocs.Count; i++)
        GetToDocTypeRefs3(lst, toDocType, toDocType.SubDocs[i], fromDocType, fromDTB);

      // VTReference
      for (int i = 0; i < fromDTB.VTRefs.Count; i++)
      {
        DBxVTReference vtr = fromDTB.VTRefs[i];
        if (vtr.MasterTableNames.Contains(toDocType.Name))
        {
          DBxDocTypeRefInfo Info = new DBxDocTypeRefInfo();
          Info.FromDocType = fromDocType;
          if (fromDTB.IsSubDoc)
            Info.FromSubDocType = (DBxSubDocType)fromDTB;

          Info.ToDocType = toDocType;
          Info.FromVTReference = vtr;
          lst.Add(Info);
        }
        // VTReference �� ������������ �� ������
      }
    }

    private static void GetToDocTypeRefs3(List<DBxDocTypeRefInfo> lst, DBxDocType toDocType, DBxDocTypeBase toDTB, DBxDocType fromDocType, DBxDocTypeBase fromDTB)
    {
      for (int i = 0; i < fromDTB.Struct.Columns.Count; i++)
      {
        DBxColumnStruct ColDef = fromDTB.Struct.Columns[i];
        if (ColDef.MasterTableName == toDTB.Name)
        {
          DBxDocTypeRefInfo Info = new DBxDocTypeRefInfo();
          Info.FromDocType = fromDocType;
          if (fromDTB.IsSubDoc)
            Info.FromSubDocType = (DBxSubDocType)fromDTB;

          Info.ToDocType = toDocType;
          if (toDTB.IsSubDoc)
            Info.ToSubDocType = (DBxSubDocType)toDTB;
          Info.FromColumn = ColDef;
          lst.Add(Info);
        }
      }
    }

    #endregion
  }
}
