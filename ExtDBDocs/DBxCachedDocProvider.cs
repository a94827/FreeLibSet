using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

// ���� �� ����, ���� ����
#if XXXX
namespace FreeLibSet.Data.Docs
{
  /// <summary>
  /// ��������� ������� � ����������, ������������ ����������� ������
  /// </summary>
  public class DBxCachedDocProvider:DBxChainDocProvider
  {
#region �����������

    public DBxCachedDocProvider(DBxDocProvider Source, bool CurrentThreadOnly)
      :base(Source, CurrentThreadOnly)
    { 
    }

#endregion

#region ���������������� ������ ��� ������������� �����������

    public override DataTable LoadDocData(string DocTypeName, Int32[] DocIds)
    {
      return DBCache[DocTypeName].CreateTable(DocIds, null);
    }

#endregion
  }
}

#endif
