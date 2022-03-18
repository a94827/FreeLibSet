// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace FreeLibSet.Data.Docs
{
  /// <summary>
  /// Реализация свойства DBxSingleDoc.SubDocs
  /// </summary>
  public struct DBxSingleDocSubDocs
  {
    #region Конструктор

    internal DBxSingleDocSubDocs(DBxSingleDoc owner)
    {
      _Owner = owner;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Документ, к которому относятся поддокументы
    /// </summary>
    public DBxSingleDoc Owner { get { return _Owner; } }
    private DBxSingleDoc _Owner;

    /// <summary>
    /// Возвращает количество видов поддокументов, определенных в DBxDocType.SubDocs
    /// </summary>
    public int Count { get { return _Owner.MultiDocs.SubDocs.Count; } }

    /// <summary>
    /// Возвращает поддокументы требуемого вида.
    /// Если для документов не определены поддокументы вида <paramref name="subDocTypeName"/>,
    /// генерируется исключение
    /// </summary>
    /// <param name="subDocTypeName">Имя таблицы поддокументов</param>
    /// <returns>Объект для доступа к однотипным документам</returns>
    public DBxSingleSubDocs this[string subDocTypeName]
    {
      get
      {
        DBxMultiSubDocs subDocs = _Owner.MultiDocs.SubDocs[subDocTypeName];
        if (subDocs == null)
        {
          if (String.IsNullOrEmpty(subDocTypeName))
            throw new ArgumentNullException("subDocTypeName");
          else
            throw new ArgumentException("Неизвестный вид поддокументов \"" + subDocTypeName + "\"", "subDocTypeName");
        }
        return new DBxSingleSubDocs(_Owner, subDocs);
      }
    }

    /// <summary>
    /// Возвращает поддокументы требуемого вида.
    /// </summary>
    /// <param name="index">Индекс вида поддокументов в диапазоне от 0 до (Count-1)</param>
    /// <returns>Объект для доступа к однотипным документам</returns>
    public DBxSingleSubDocs this[int index]
    {
      get
      {
        DBxMultiSubDocs subDocs = _Owner.MultiDocs.SubDocs[index];
        return new DBxSingleSubDocs(_Owner, subDocs);
      }
    }

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return "Поддокументы для " + Owner.ToString();
    }

    #endregion
  }
}
