// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace FreeLibSet.Data.Docs
{
  /// <summary>
  /// Реализация свойства DBxMultiDocs.SubDocs
  /// Содержит коллекцию видов поддокументов с доступом по имени поддокумента
  /// </summary>
  public class DBxMultiDocsSubDocs
  {
    #region Защищенный конструктор

    internal DBxMultiDocsSubDocs(DBxMultiDocs owner)
    {
      _Owner = owner;
    }

    internal void InitTables()
    {
      if (_Items == null)
        return;
      foreach (KeyValuePair<string, DBxMultiSubDocs> Pair in _Items)
        Pair.Value.InitTables();
    }

    #endregion

    #region Поддокументы

    /// <summary>
    /// Доступ по индексу, соответствующему порядку поддокумента в DBxDocType.SubDocs
    /// </summary>
    /// <param name="index">Индекс вида поддокументов в диапазоне от 0 до (DBxDocType.SubDocs.Count-1)</param>
    /// <returns></returns>
    public DBxMultiSubDocs this[int index]
    {
      get
      {
        string SubDocTypeName = _Owner.DocType.SubDocs[index].Name;
        return this[SubDocTypeName];
      }
    }

    /// <summary>
    /// Возвращает количество видов поддокументов, определенных в DBxDocType.SubDocs
    /// </summary>
    public int Count { get { return _Owner.DocType.SubDocs.Count; } }

    /// <summary>
    /// Возвращает объект доступа к поддокументам одного вида.
    /// Если запрошен неизвестный вид поддокумента, генерируется исключение
    /// </summary>
    /// <param name="subDocTypeName">Вид поддокумента</param>
    /// <returns>Объект доступа к поддокументам</returns>
    public DBxMultiSubDocs this[string subDocTypeName]
    {
      get
      {
        if (_Items == null)
          _Items = new Dictionary<string, DBxMultiSubDocs>();
        DBxMultiSubDocs Res;
        if (!_Items.TryGetValue(subDocTypeName, out Res))
        {
          DBxSubDocType sdt = _Owner.DocType.SubDocs[subDocTypeName];
          if (sdt == null)
          {
            if (String.IsNullOrEmpty(subDocTypeName))
              throw new ArgumentNullException("subDocTypeName");
            else
              throw new ArgumentException("Неизвестный вид поддокумента \"" + subDocTypeName + "\"", "subDocTypeName");
          }
          DBxMultiSubDocs Res2 = new DBxMultiSubDocs(_Owner, sdt);

          if (!_Items.TryGetValue(subDocTypeName, out Res)) // 22.01.2019
          {
            Res = Res2;
            _Items.Add(subDocTypeName, Res);
          }
        }
        return Res;
      }
    }

    private Dictionary<string, DBxMultiSubDocs> _Items;

    internal void ClearList()
    {
      if (_Items == null)
        return;
      foreach (KeyValuePair<string, DBxMultiSubDocs> Pair in _Items)
        Pair.Value.ClearList();
    }

    internal void ResetTables()
    {
      if (_Items == null)
        return;
      foreach (KeyValuePair<string, DBxMultiSubDocs> Pair in _Items)
        Pair.Value.ResetTable();
    }

    /// <summary>
    /// Возвращает true, если есть хотя бы один поддокумент заданного вида в любом состоянии
    /// Метод ничего не экономит, т.к. вынужден выполнять загрузку поддокументов, если она еще не выполнена
    /// </summary>
    /// <param name="subDocTypeName">Имя вида поддокумента</param>
    /// <returns>True, если есть такой вид поддокумента</returns>
    public bool ContainsSubDocs(string subDocTypeName)
    {
      return this[subDocTypeName].SubDocCount > 0;
    }

    #endregion

    #region Прочие свойства и методы

    /// <summary>
    /// Документы одного вида
    /// </summary>
    public DBxMultiDocs Owner { get { return _Owner; } }
    private DBxMultiDocs _Owner;

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>текстовое представление</returns>
    public override string ToString()
    {
      return "Поддокументы для " + _Owner.DocType.PluralTitle;
    }

    /// <summary>
    /// Загружает все поддокументы.
    /// Обычно, нет необходимости вызывать этот метод явно, так как поддокументы загружаются по мере обращения к ним.
    /// Вызов метода нужен, если требуется доступ к набору данных DataSet
    /// </summary>
    public void LoadAll()
    {
      int dummy=0;
      for (int i = 0; i < Count; i++)
        dummy += this[i].SubDocCount;
    }

    #endregion

    #region Состояние поддокументов


    /// <summary>
    /// Возвращает true, если для указанного вида поддокументов есть изменнные, созданные или удаленные строки
    /// Вызов этого метода предпочтительнее, чем DBxMultiSubDocs.ContainsChanged
    /// </summary>
    /// <param name="subDocTypeName">Имя поддокумента</param>
    /// <returns></returns>
    public bool ContainsModified(string subDocTypeName)
    {
      if (_Items == null)
        return false;
      DBxMultiSubDocs Res;
      if (!_Items.TryGetValue(subDocTypeName, out Res))
        return false;
      return Res.ContainsModified;
    }


    /// <summary>
    /// Возвращает число поддокументов в заданном состоянии
    /// Вызов этого метода предпочтительнее, чем DBxMultiSubDocs.ContainsChanged
    /// </summary>
    /// <param name="subDocTypeName">Имя поддокумента</param>
    /// <param name="state">Проверяемое состояние</param>
    /// <returns>Количество найденных поддокументов</returns>
    public int GetSubDocCount(string subDocTypeName, DBxDocState state)
    {
      if (_Items == null)
        return 0;
      DBxMultiSubDocs Res;
      if (!_Items.TryGetValue(subDocTypeName, out Res))
        return 0;
      return Res.GetSubDocCount(state);
    }


    #endregion
  }
}
