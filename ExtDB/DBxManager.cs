// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using FreeLibSet.Collections;

namespace FreeLibSet.Data
{
  /// <summary>
  /// ��������� �������� DBx ��� ����� �����������.
  /// ��� ������� ���� ���� ������ ��������� ���� (��������) �����, ����������� �� DBxManager.
  /// ����������� ������ DBxManager.Managers �������� ������ ����������, �� ������ �� ������ ���
  /// ���� ������.
  /// � �������� ���� ������������ ��� ���������� ���� ������, �������� � Net Framework. 
  /// ��������, ��� Microsoft SQL Server ������������ ��� "System.Data.SqlClient".
  /// ��. �������� System.Configuration.ConnectionStringSettings.ProviderName
  /// ���� ����� �������� ����������������.
  /// </summary>
  public abstract class DBxManager : ObjectWithCode
  {
    #region ���������� �����������

    /// <summary>
    /// ������� ���������
    /// </summary>
    /// <param name="providerName">��� ���������� Net Framework</param>
    protected DBxManager(string providerName)
      : base(providerName)
    {
    }

    #endregion

    #region ��������

    /// <summary>
    /// ��� ���������� Net Framework.
    /// �������� ������ � ����������� ������ Managers.
    /// </summary>
    public string ProviderName { get { return base.Code; } }

    /// <summary>
    /// ��������� ��������, ������������� ��� ���� ������.
    /// ���� ���� ��������� ������ ���� ������ DBx, ����������� �������� DBx.ProviderFactory
    /// </summary>
    public abstract DbProviderFactory ProviderFactory { get; }

    #endregion

    #region ����������� ������

    /// <summary>
    /// ������� ������ ���� ������, ��������� ������ �����������
    /// </summary>
    /// <param name="connectionString">������ �����������</param>
    /// <returns>��������� ������ DBx</returns>
    public abstract DBx CreateDBObject(string connectionString);

    /// <summary>
    /// �������� ������ ����������� ��� ������ ���� ������ "�� �������".
    /// ���������� ������ �������� "�����������������" ������ ����������� (����������� �� DBConnectionString)
    /// � ��������� ������ �������, ��������, DatabaseName.
    /// ���� � ������ ����������� ������ ����� ������ (��� ���������), ��� ����� ������ ���� ����������
    /// ��������.
    /// </summary>
    /// <param name="connectionString">������������ ������ ����������� � ���� ������ <paramref name="oldDBName"/></param>
    /// <param name="oldDBName">��� ���� ������ � c����������� ������ �����������</param>
    /// <param name="newDBName">��� ����� ���� ������</param>
    /// <returns></returns>
    /// <remarks>
    /// ����� ������ ��������� ����������� ������� �������� � �������� � ����� ���� ������.
    /// ��������, ���� � �������� ������ ����������� ������ ��� ���� ������ "myprog_db1", � ���������
    /// �������� ��� ���� ������ "db1" �� "db2", �� ����� ������ ������ � ������ ����������� ��� 
    /// ���� ������ "myprog_db2".
    /// ��� ���������� ������ ������� ������������ ������ ReplaceDatabaseItem() � ReplaceFileItem(),
    /// ������� ������������ ������� ���������
    /// </remarks>
    public abstract string ReplaceDBName(string connectionString, string oldDBName, string newDBName);

    /// <summary>
    /// ������� ������ DbConnectionStringBuilder ��� �������� ������ �����������.
    /// ������������ ������ ������������ ������
    /// </summary>
    /// <param name="connectionString">������ �����������</param>
    /// <returns>������ ��� ����������� ������� �����������</returns>
    public abstract DbConnectionStringBuilder CreateConnectionStringBuilder(string connectionString);

    #endregion

    #region ������ ��� ���������� ReplaceDBName

    /// <summary>
    /// ������ ����� ���� ������ <paramref name="oldName"/> � �������� ������ ����������� <paramref name="oldItem"/>
    /// �� <paramref name="newName"/>
    /// </summary>
    /// <param name="oldItem">"������� ������ �����������</param>
    /// <param name="oldName">��� ���� ������, ������� ��� ������ � <paramref name="oldItem"/>. 
    /// ���� �������� �� �����, �� ��� ���� ������ ������� ������ <paramref name="oldItem"/>.
    /// � ���� ������ ������ ������������ �������/������� ��� ���� ������</param>
    /// <param name="newName">��� ����� ���� ������, ������� ������ ���� ������</param>
    /// <returns>���������� �����</returns>
    protected static string ReplaceDBItem(string oldItem, string oldName, string newName)
    {
      if (String.IsNullOrEmpty(oldItem))
        return oldItem;
      if (String.IsNullOrEmpty(oldName))
        oldName = oldItem;
      if (String.IsNullOrEmpty(newName))
        throw new ArgumentNullException("newName");

      int p = oldItem.LastIndexOf(oldName, StringComparison.OrdinalIgnoreCase);
      if (p < 0)
        throw new ArgumentException("��� ���� ������ \"" + oldItem + "\" �� �������� � ���� ��������� \"" + oldName + "\", ������� ��������� ��������");
      return oldItem.Substring(0, p) + newName + oldItem.Substring(p + oldName.Length);
    }

    /// <summary>
    /// ������ ����� ���� ������ � ����� �����.
    /// ���� ����� �������� ������ ��� �����, �� �� �������, � ������� ���� ����������
    /// </summary>
    /// <param name="oldItem">������������ ��� ����� � ������ �����������</param>
    /// <param name="oldName">��� ������������ ���� ������. ���� �������� �� �����, ������������
    /// ��� ����� ��� �����������, ����������� �� <paramref name="oldItem"/>.</param>
    /// <param name="newName">��� ����� ���� ������</param>
    /// <returns></returns>
    protected static string ReplaceFileItem(string oldItem, string oldName, string newName)
    {
      if (String.IsNullOrEmpty(oldItem))
        return oldItem;

      // �� ����� ������������ AbsPath ��� ��������������, �.�. ��� ����� ����� ���� ������
      // �����-������ ������ �������

      int p = oldItem.LastIndexOf(System.IO.Path.DirectorySeparatorChar);
      string Dir = oldItem.Substring(0, p + 1); // ������� ������ ��������
      string FileName = oldItem.Substring(p + 1); // ��� ����
      string Ext = System.IO.Path.GetExtension(FileName);
      FileName = System.IO.Path.GetFileNameWithoutExtension(FileName);

      if (String.IsNullOrEmpty(oldName))
        oldName = FileName;
      if (String.IsNullOrEmpty(newName))
        throw new ArgumentNullException("newName");

      p = FileName.LastIndexOf(oldName, StringComparison.OrdinalIgnoreCase);
      if (p < 0)
        throw new ArgumentException("��� ����� ���� ������ \"" + oldItem + "\" �� �������� � ���� ��������� \"" + oldName + "\", ������� ��������� ��������");
      FileName = FileName.Substring(0, p) + newName + FileName.Substring(p + oldName.Length);

      return Dir + FileName + Ext;
    }


    #endregion

    #region ����������� ������

    // ������ ���������������� ������ ��������������� ��� � ����������� ������������ DBxManager,
    // �.�. ����� ��������: � ������ ���� �������� ������� �������, ����������� �� DBxManager.
    // �� ��������� ������������� ������ ���������� ����������� ������
    //public static SyncNamedCollection<DBxManager> Managers { get { return FManagers; } }
    //private static SyncNamedCollection<DBxManager> FManagers =  new SyncNamedCollection<DBxManager>();

    /// <summary>
    /// ������ ������������������ ����������.
    /// �� ��������� ������ �������� ��������� ��� ���� �����������, ������������� � ExtDB.dll.
    /// ��. ��������� � DBxProviderNames
    /// ����� �������� ���������, ������������� � ����������
    /// </summary>
    public static SyncNamedCollection<DBxManager> Managers 
    { 
      get 
      {
        lock (SyncRoot)
        {
          if (_Managers == null)
          {
            _Managers = new SyncNamedCollection<DBxManager>();
            _Managers.Add(FreeLibSet.Data.SqlClient.SqlDBxManager.TheManager);
            _Managers.Add(FreeLibSet.Data.Npgsql.NpgsqlDBxManager.TheManager);
            _Managers.Add(FreeLibSet.Data.SQLite.SQLiteDBxManager.TheManager);
          }
          return _Managers;
        }
      }
    }
    private static SyncNamedCollection<DBxManager> _Managers = null;

    private static object SyncRoot = new object();

    #endregion
  }

  /// <summary>
  /// ������, �������� ����� �����������, ������������� � ExtDB.dll
  /// ���� ��� ����������� � �� ������������ ���� ������������ ����������,
  /// �� ��������� ����� ���������� ����� ��� �������� ConnectionStringSettings.ProviderName
  /// </summary>
  public static class DBxProviderNames
  {
    #region ���������

    /// <summary>
    /// ��� ���������� ��� MS SQL Server
    /// </summary>
    public const string Sql = "System.Data.SqlClient";

    /// <summary>
    /// ��� ���������� ��� PostgreSQL
    /// </summary>
    public const string Npgsql = "Npgsql";

    /// <summary>
    /// ��� ���������� ��� SQLite
    /// </summary>
    public const string SQLite = "SQLite";

    #endregion
  }
}
