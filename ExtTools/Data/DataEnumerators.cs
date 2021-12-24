// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using FreeLibSet.Core;

namespace FreeLibSet.Data
{
  /// <summary>
  /// Поиск строк в таблице с совпадающими значениями полей, входящих в DataView.Sort.
  /// Реализует перечислитель по массивам строк с одинаковыми значениями полей.
  /// </summary>
  public sealed class DataViewRowPairEnumarable : IEnumerable<DataRow[]>
  {
    #region Конструктор

    /// <summary>
    /// Создает объект, для которого можно вызвать оператор foreach.
    /// Будут возвращаться только строки с совпадающими значениями полей.
    /// При сравнении учитываются все столбцы, заданные в DataView.Sort
    /// </summary>
    /// <param name="dv">Набор данных с установленным свойством Sort</param>
    public DataViewRowPairEnumarable(DataView dv)
      : this(dv, 0, false)
    {
    }

    /// <summary>
    /// Создает объект, для которого можно вызвать оператор foreach.
    /// </summary>
    /// <param name="dv">Набор данных с установленным свойством Sort</param>
    /// <param name="compareColumnCount">Количество столбцов, участвующих в сравнении.
    /// В сравнении участвуют первые столбцы, заданные в DataView.Sort, но могут использоваться не все столбцы.
    /// Нулевое значение означает использование всех столбцов.</param>
    public DataViewRowPairEnumarable(DataView dv, int compareColumnCount)
      : this(dv, compareColumnCount, false)
    {
    }

    /// <summary>
    /// Создает объект, для которого можно вызвать оператор foreach.
    /// </summary>
    /// <param name="dv">Набор данных с установленным свойством Sort</param>
    /// <param name="compareColumnCount">Количество столбцов, участвующих в сравнении.
    /// В сравнении участвуют первые столбцы, заданные в DataView.Sort, но могут использоваться не все столбцы.
    /// Нулевое значение означает использование всех столбцов.</param>
    /// <param name="enumSingleRows">Если true, то будут перебраны все строки в DataView, включая одиночные.
    /// Если false, то будут возвращаться только строки с совпадающими значениями полей.</param>
    public DataViewRowPairEnumarable(DataView dv, int compareColumnCount, bool enumSingleRows)
    {
      if (dv == null)
        throw new ArgumentNullException("dv");
      if (String.IsNullOrEmpty(dv.Sort))
        throw new InvalidOperationException("Не установлено свойство DataView.Sort");

      _DV = dv;
      string[] aColNames = DataTools.GetDataViewSortColumnNames(dv.Sort);

      if (compareColumnCount == 0)
        compareColumnCount = aColNames.Length;

      if (compareColumnCount < 1 || compareColumnCount > aColNames.Length) // испр. 24.12.2021
        throw new ArgumentOutOfRangeException("compareColumnCount", compareColumnCount, "Количество столбцов для сравнения должно быть в диапазоне от 1 до " + (aColNames.Length - 1).ToString());

      _ColPoss = new int[compareColumnCount];
      for (int i = 0; i < compareColumnCount; i++)
      {
        _ColPoss[i] = dv.Table.Columns.IndexOf(aColNames[i]);
        if (_ColPoss[i] < 0)
          throw new BugException("Не найден столбец \"" + aColNames[i] + "\"");
      }
      _EnumSingleRows = enumSingleRows;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Набор данных, по которому выполняется перечисление
    /// </summary>
    public DataView DV { get { return _DV; } }
    private DataView _DV;

    /// <summary>
    /// Массив текущих позиций столбцов, входящих в DataView.Sort.
    /// </summary>
    private int[] _ColPoss;

    /// <summary>
    /// Надо ли перебирать одиночные строки (true) или только с совпадющими значениями (false).
    /// </summary>
    public bool EnumSingleRows { get { return _EnumSingleRows; } }
    private bool _EnumSingleRows;

    #endregion

    #region Внутренние методы

    internal bool AreRowsEqual(DataRow row1, DataRow row2)
    {
      for (int i = 0; i < _ColPoss.Length; i++)
      {
        if (!AreValuesEqual(row1, row2, _ColPoss[i], !_DV.Table.CaseSensitive))
          //if (!DataTools.AreValuesEqual(row1, row2, _ColPoss[i]))
          return false;
      }
      return true;
    }
    /// <summary>
    /// Сравнение значений одного поля для двух строк.
    /// Возвращает значение true, если значения одинаковы. Если есть пустые
    /// значения DBNull, то строки считаются одинаковыми, если обе строки содержат
    /// DBNull
    /// </summary>
    /// <param name="row1">Первая сравниваемая строка</param>
    /// <param name="row2">Вторая сравниваемая строка</param>
    /// <param name="columnPos">Позиция столбца</param>
    /// <param name="ignoreCase">Если столбец имеет строковый тип, то сравнение выполняется без учета регистра, как это обычно делается при поиске в DataView</param>
    /// <returns>true, если значения одинаковы</returns>
    private static bool AreValuesEqual(DataRow row1, DataRow row2, int columnPos, bool ignoreCase)
    {
      object x1 = row1[columnPos];
      object x2 = row2[columnPos];

      if (ignoreCase)
      {
        string s1 = x1 as String;
        string s2 = x2 as String;
        if (!(Object.ReferenceEquals(s1, null) || Object.ReferenceEquals(s2, null))) // испр. 24.12.2021
          return String.Equals(s1, s2, StringComparison.OrdinalIgnoreCase);
      }

      return x1.Equals(x2);
    }

    #endregion

    #region Перечислитель

    /// <summary>
    /// Реализация перечислителя.
    /// </summary>
    public struct Enumerator : IEnumerator<DataRow[]>
    {
      #region Конструктор

      internal Enumerator(DataViewRowPairEnumarable owner)
      {
        _Owner = owner;
        _CurrIndex = -1;
        _CurrCount = 1;
      }

      #endregion

      #region Поля

      /// <summary>
      /// Просматриваемый набор данных
      /// </summary>
      private DataViewRowPairEnumarable _Owner;

      /// <summary>
      /// Индекс первой строки в блоке повторяющихся строк
      /// </summary>
      private int _CurrIndex;

      /// <summary>
      /// Количество повторяющихся строк. Минимум 2, если найдено
      /// </summary>
      private int _CurrCount;

      #endregion

      #region IEnumerator<DataRow[]> Members

      /// <summary>
      /// Основной метод.
      /// Выполняет поиск последовательности одинаковых строк
      /// </summary>
      /// <returns>true, если найдены очередные одинаковые строки</returns>
      public bool MoveNext()
      {
        while ((_CurrIndex + _CurrCount) < _Owner._DV.Count)
        {
          _CurrIndex += _CurrCount;

          DataRow row0 = _Owner._DV[_CurrIndex].Row;
          _CurrCount = 1;

          while ((_CurrIndex + _CurrCount) < _Owner._DV.Count)
          {
            DataRow row2 = _Owner._DV[_CurrIndex + _CurrCount].Row;
            if (_Owner.AreRowsEqual(row0, row2))
              _CurrCount++;
            else
              break;
          }

          if (_CurrCount >= 2 || _Owner._EnumSingleRows)
            return true;
        }

        _CurrCount = 0;
        return false;
      }

      /// <summary>
      /// Возвращает текущий блок из двух или более одинаковых строк, если EnumSingleRows=false.
      /// При EnumSingleRows=false могут возвращаться массивы из одного элемента.
      /// </summary>
      public DataRow[] Current
      {
        get
        {
          DataRow[] a = new DataRow[_CurrCount];
          for (int i = 0; i < a.Length; i++)
            a[i] = _Owner._DV[_CurrIndex + i].Row;
          return a;
        }
      }

      /// <summary>
      /// Ничего не делает
      /// </summary>
      public void Dispose()
      {
      }

      object System.Collections.IEnumerator.Current { get { return Current; } }

      void System.Collections.IEnumerator.Reset()
      {
        _CurrIndex = -1;
        _CurrCount = 1;
      }

      #endregion
    }

    /// <summary>
    /// Возвращает перечислитель
    /// </summary>
    /// <returns></returns>
    public Enumerator GetEnumerator()
    {
      return new Enumerator(this);
    }

    IEnumerator<DataRow[]> IEnumerable<DataRow[]>.GetEnumerator()
    {
      return new Enumerator(this);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new Enumerator(this);
    }

    #endregion
  }
}
