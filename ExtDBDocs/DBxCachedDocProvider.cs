using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

// Пока не знаю, чего хочу
#if XXXX
namespace FreeLibSet.Data.Docs
{
  /// <summary>
  /// Провайдер доступа к документам, использующий буферизацию таблиц
  /// </summary>
  public class DBxCachedDocProvider:DBxChainDocProvider
  {
#region Конструктор

    public DBxCachedDocProvider(DBxDocProvider Source, bool CurrentThreadOnly)
      :base(Source, CurrentThreadOnly)
    { 
    }

#endregion

#region Переопределенные методы для использования буферизации

    public override DataTable LoadDocData(string DocTypeName, Int32[] DocIds)
    {
      return DBCache[DocTypeName].CreateTable(DocIds, null);
    }

#endregion
  }
}

#endif
