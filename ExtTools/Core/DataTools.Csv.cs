using System;
using System.Collections.Generic;
using System.Text;

namespace FreeLibSet.Core
{
	partial class DataTools
	{
    #region CommaString

    #region Одна строка

    /// <summary>
    /// Преобразование строки в массив идентификаторов. Строка должна содержать
    /// список чисел, разделенных запятыми. Пробелы отбрасываются
    /// Возвращается null, если строка пустая или содержит только пробелы
    /// </summary>
    /// <param name="s">Строка</param>
    /// <returns>Массив чисел или null</returns>
    public static Int32[] CommaStringToIds(string s)
    {
      if (String.IsNullOrEmpty(s))
        return null;
      s = s.Trim();
      if (s.Length == 0)
        return null;
      string[] a = s.Split(',');
      Int32[] Res = new Int32[a.Length];
      for (int i = 0; i < a.Length; i++)
        Res[i] = Int32.Parse(a[i].Trim());
      return Res;
    }

    /// <summary>
    /// Получение строки чисел, разделенных запятыми из массива идентификаторов.
    /// Если массив <paramref name="ids"/> пустой или ссылка равна null, возвращается пустая строка.
    /// </summary>
    /// <param name="ids">Массив идентификаторов (может быть null)</param>
    /// <param name="addSpace">Добавить пробелы после запятых</param>
    /// <returns>Строка</returns>
    public static string CommaStringFromIds(Int32[] ids, bool addSpace)
    {
      if (ids == null)
        return String.Empty;
      if (ids.Length == 0)
        return String.Empty;
      if (ids.Length == 1)
        return ids[0].ToString(); // чтобы не создавать объект
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < ids.Length; i++)
      {
        if (i > 0)
        {
          sb.Append(',');
          if (addSpace)
            sb.Append(' ');
        }
        sb.Append(ids[i]);
      }
      return sb.ToString();
    }

    /// <summary>
    /// Преобразование строки в массив идентификаторов GUID. Строка должна содержать
    /// список идентификаторов GUID, разделенных запятыми. Пробелы отбрасываются
    /// Возвращается null, если строка пустая или содержит только пробелы
    /// </summary>
    /// <param name="s">Строка</param>
    /// <returns>Массив идентификаторов или null</returns>
    public static Guid[] CommaStringToGuids(string s)
    {
      if (String.IsNullOrEmpty(s))
        return null;
      s = s.Trim();
      if (s.Length == 0)
        return null;
      string[] a = s.Split(',');
      Guid[] Res = new Guid[a.Length];
      for (int i = 0; i < a.Length; i++)
        Res[i] = new Guid(a[i].Trim());
      return Res;
    }

    /// <summary>
    /// Получение строки с GUID'ами, разделенных запятыми из массива идентификаторов
    /// </summary>
    /// <param name="guids">Массив идентификаторов (может быть null)</param>
    /// <param name="addSpace">Добавить пробелы после запятых</param>
    /// <returns>Строка</returns>
    public static string CommaStringFromGuids(Guid[] guids, bool addSpace)
    {
      if (guids == null)
        return String.Empty;
      if (guids.Length == 0)
        return String.Empty;
      if (guids.Length == 1)
        return guids[0].ToString(); // чтобы не создавать объект
      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < guids.Length; i++)
      {
        if (i > 0)
        {
          sb.Append(',');
          if (addSpace)
            sb.Append(' ');
        }
        sb.Append(guids[i].ToString());
      }
      return sb.ToString();
    }

    #endregion

    #endregion
  }
}
