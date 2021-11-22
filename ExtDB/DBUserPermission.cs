// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Config;

namespace FreeLibSet.Data
{

  /// <summary>
  /// ������� ����� ��� WholeDBPermission � TablePermission 
  /// </summary>
  public abstract class DBUserPermission : UserPermission
  {
    #region �����������

    /// <summary>
    /// ������� ���������� � �������� �����.
    /// �������� Mode �������� �������� Full (������ ������)
    /// </summary>
    /// <param name="classCode">����� ����������</param>
    protected DBUserPermission(string classCode)
      : base(classCode)
    {
    }

    /// <summary>
    /// ������� ���������� � �������� �����
    /// </summary>
    /// <param name="classCode">����� ����������</param>
    /// <param name="mode">����� �������</param>
    protected DBUserPermission(string classCode, DBxAccessMode mode)
      : base(classCode)
    {
      _Mode = mode;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ����� ������� � ������� ���� ������
    /// </summary>
    public DBxAccessMode Mode
    {
      get { return _Mode; }
      set
      {
        CheckNotReadOnly();
        _Mode = value;
      }
    }

    private DBxAccessMode _Mode;

    /// <summary>
    /// ��������� ������������� ��� ����������
    /// </summary>
    public override string ValueText
    {
      get { return DBxPermissions.GetAccessModeText(Mode); }
    }

    #endregion

    #region XML

    /// <summary>
    /// ���������� � ������ ������������ �������� "Mode"
    /// </summary>
    /// <param name="cfg">������ ������������</param>
    public override void Write(CfgPart cfg)
    {
      cfg.SetString("Mode", Mode.ToString());
    }

    /// <summary>
    /// ������ �� ������ ������������ �������� "Mode"
    /// </summary>
    /// <param name="cfg">������ ������������</param>
    public override void Read(CfgPart cfg)
    {
      string s = cfg.GetString("Mode");
      switch (s)
      {
        case "":
        case "Full": Mode = DBxAccessMode.Full; break;
        case "ReadOnly": Mode = DBxAccessMode.ReadOnly; break;
        case "None": Mode = DBxAccessMode.None; break;
        default:
          throw new InvalidOperationException("����������� �������� Mode=\"" + s + "\"");
      }
    }

#if XXX
    public static DBxAccessMode ReadMode(CfgPart Part)
    {
      string s = Part.GetString("Mode");
      switch (s)
      {
        case "":
        case "Full": return DBxAccessMode.Full;
        case "ReadOnly": return DBxAccessMode.ReadOnly;
        case "None": return DBxAccessMode.None;
        default:
          throw new InvalidOperationException("����������� �������� Mode=\"" + s + "\"");
      }
    }
#endif

    #endregion

    #region ����������� ������ ��� DBxAccessMode

    /// <summary>
    /// ����, ��������������� ������������ DBxAccessMode
    /// </summary>
    public static readonly string[] ValueCodes = new string[] { "FULL", "READONLY", "NONE" };

    /// <summary>
    /// ������������ ��������, ��������������� ������������ DBxAccessMode
    /// </summary>
    public static readonly string[] ValueNames = new string[] { "������", "������", "������" };


    /// <summary>
    /// �������� ��������� �������������, ��������������� ����
    /// </summary>
    /// <param name="code">��� �� ������ ValueCodes</param>
    /// <returns>��������� �������������</returns>
    public static string GetValueName(string code)
    {
      int p = Array.IndexOf<string>(ValueCodes, code);
      if (p >= 0)
        return ValueNames[p];
      else
        return "?? \"" + code + "\"";
    }

    /// <summary>
    /// �������� ��������� ������������� ��� ������������ DBxAccessMode.
    /// </summary>
    /// <param name="mode">����� DBxAccessMode</param>
    /// <returns>��������� �������������</returns>
    public static string GetValueName(DBxAccessMode mode)
    {
      if ((int)mode >= 0 && (int)mode <= ValueNames.Length)
        return ValueNames[(int)mode];
      else
        return "?? " + mode.ToString();
    }

    /// <summary>
    /// �������� ��� �� ������ ValueCodes ��� ������������ DBxAccessMode.
    /// </summary>
    /// <param name="mode">����� DBxAccessMode</param>
    /// <returns>"���</returns>
    public static string GetValueCode(DBxAccessMode mode)
    {
      if ((int)mode >= 0 && (int)mode <= ValueCodes.Length)
        return ValueCodes[(int)mode];
      else
        throw new ArgumentException("����������� ����� ������� " + mode.ToString());
    }

    /// <summary>
    /// �������� ������� ������������ DBxAccessMode, ��������������� ���� �� ������ ValueCodes.
    /// ���� ����� ������������ ���, ������������� ����������
    /// </summary>
    /// <param name="code">���</param>
    /// <returns>������������ DBxAccessMode</returns>
    public static DBxAccessMode GetAccessMode(string code)
    {
      int p = Array.IndexOf<string>(ValueCodes, code);
      if (p >= 0)
        return (DBxAccessMode)p;
      else
        throw new ArgumentException("����������� ��� ������ �������: \"" + code + "\"", "code");
    }

    /// <summary>
    /// ���������� true, ���� ��������� ��� ���� � ������ ValueCodes
    /// </summary>
    /// <param name="code">���</param>
    /// <returns>������� � ������</returns>
    public static bool IsValidAccessMode(string code)
    {
      int p = Array.IndexOf<string>(ValueCodes, code);
      return p >= 0;
    }

    #endregion
  }

  /// <summary>
  /// ���������� �� ������ � ���� ������ �-�����.
  /// ��� ������ "DB"
  /// </summary>
  public class WholeDBPermission : DBUserPermission
  {
    #region Creator

    /// <summary>
    /// ���������� ���������� IUserPermissionCreator
    /// </summary>
    public sealed class Creator : IUserPermissionCreator
    {
      #region IUserPermissionCreator Members

      /// <summary>
      /// ���������� "DB"
      /// </summary>
      public string Code { get { return "DB"; } }

      /// <summary>
      /// ������� WholeDBPermission
      /// </summary>
      /// <returns></returns>
      public UserPermission CreateUserPermission()
      {
        return new WholeDBPermission();
      }

      #endregion
    }

    #endregion

    #region ������������

    /// <summary>
    /// ������� ���������� � ������ "Full".
    /// </summary>
    public WholeDBPermission()
      : base("DB")
    {
    }

    /// <summary>
    /// ������� ����������
    /// </summary>
    /// <param name="mode">�����</param>
    public WholeDBPermission(DBxAccessMode mode)
      : this()
    {
      this.Mode = mode;
    }

    #endregion

    #region ���������������� ������

    /// <summary>
    /// ������������� �������� DBxPermissions.DBMode.
    /// </summary>
    /// <param name="dbPermissions">����������� ������ ���������� ���� ������</param>
    public override void ApplyDbPermissions(DBxPermissions dbPermissions)
    {
      dbPermissions.DBMode = Mode;
    }

    /// <summary>
    /// ���������� "������ � ���� ������"
    /// </summary>
    public override string ObjectText { get { return "������ � ���� ������"; } }

    #endregion
  }

  /// <summary>
  /// ���������� �� ������ � ����� ��� ���������� ��������
  /// ��� ������ "Table"
  /// </summary>
  public class TablePermission : DBUserPermission
  {
    #region Creator

    /// <summary>
    /// ���������� ���������� IUserPermissionCreator
    /// </summary>
    public sealed class Creator : IUserPermissionCreator
    {
      #region IUserPermissionCreator Members

      /// <summary>
      /// ���������� "Table"
      /// </summary>
      public string Code { get { return "Table"; } }

      /// <summary>
      /// ������� TablePermission
      /// </summary>
      /// <returns></returns>
      public UserPermission CreateUserPermission()
      {
        return new TablePermission();
      }

      #endregion
    }

    #endregion

    #region ������������

    /// <summary>
    /// ������� ����������.
    /// �������� TableNames � Mode ������ ���� ����������� ����.
    /// �������� Mode �������� �������� Full.
    /// </summary>
    public TablePermission()
      :base("Table")
    { 
    }

    /// <summary>
    /// ������� ���������� ��� �������� ������
    /// </summary>
    /// <param name="tableNames">����� ������ � ���� ������</param>
    /// <param name="mode">����� �������</param>
    public TablePermission(string[] tableNames, DBxAccessMode mode)
      :this()
    {
      this.TableNames = tableNames;
      this.Mode = mode;
    }

    /// <summary>
    /// ������� ���������� ��� ����� �������
    /// </summary>
    /// <param name="tableName">��� ������� � ���� ������</param>
    /// <param name="mode">����� �������</param>
    public TablePermission(string tableName, DBxAccessMode mode)
      : this()
    {
      this.TableNames = new string[] { tableName };
      this.Mode = mode;
    }

    #endregion

    #region ��������

    /// <summary>
    /// ����� ������ � ���� ������.
    /// </summary>
    public string[] TableNames
    {
      get { return _TableNames; }
      set
      {
        CheckNotReadOnly();
        _TableNames = value;
      }
    }
    private string[] _TableNames;

    #endregion

    #region ���������������� ������

    /// <summary>
    /// �������� ���������� � ������ DBxPermissions 
    /// </summary>
    /// <param name="dbPermissions">���������� �� ������ � �������� ���� ������</param>
    public override void ApplyDbPermissions(DBxPermissions dbPermissions)
    {
      for (int i = 0; i < TableNames.Length; i++)
        dbPermissions.TableModes[TableNames[i]] = Mode;
    }

    /// <summary>
    /// ��������� ������������� ��� ������ ������
    /// </summary>
    public override string ObjectText
    {
      get
      {
        if (TableNames == null)
          return "������� �� ������";

        if (TableNames.Length == 1)
          return "������� \"" + TableNames[0] + "\"";
        else
          return "������� " + String.Join(", ", TableNames);
      }
    }

    /// <summary>
    /// ������ ���������� � ������ ������������
    /// </summary>
    /// <param name="cfg">������ ������������ ��� ����������</param>
    public override void Write(CfgPart cfg)
    {
      base.Write(cfg);
      string s = String.Join(",", this.TableNames);
      cfg.SetString("Tables", s);
    }

    /// <summary>
    /// ������ ���������� �� ������ ������������
    /// </summary>
    /// <param name="cfg">������ ������������ ��� ����������</param>
    public override void Read(CfgPart cfg)
    {
      base.Read(cfg);

      string s = cfg.GetString("Tables");
      this.TableNames = s.Split(',');
    }

    #endregion

    #region ����� ����������

    /// <summary>
    /// ���������� ���������� �� ������� ����������.
    /// � ������ ���������������� ���������� <paramref name="permissions"/> ����������� ����� ����������� ���������� TablePermission ��� WholeDBPermission.
    /// ����� ����������� �� ����� � ������ ������.
    /// ���� ���������� �� �������, ������������ DBxAccessMode.Full
    /// </summary>
    /// <param name="permissions">���������������� ����������</param>
    /// <param name="tableName">��� �������</param>
    /// <returns>����������</returns>
    public static DBxAccessMode GetAccessMode(UserPermissions permissions, string tableName)
    {
      if (permissions == null)
        throw new ArgumentNullException("permissions");
      if (String.IsNullOrEmpty(tableName))
        throw new ArgumentNullException("tableName");
      for (int i = permissions.Count - 1; i >= 0; i--)
      {
        TablePermission tp = permissions[i] as TablePermission;
        if (tp != null)
        {
          if (Array.IndexOf<string>(tp.TableNames, tableName) >= 0)
            return tp.Mode;

          continue;
        }

        WholeDBPermission dbp = permissions[i] as WholeDBPermission;
        if (dbp != null)
          return dbp.Mode;
      }

      return DBxAccessMode.Full;
    }

    #endregion
  }
}
