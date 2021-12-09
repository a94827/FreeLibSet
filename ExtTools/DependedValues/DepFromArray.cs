using System;
using System.Collections.Generic;
using System.Text;

namespace FreeLibSet.DependedValues
{
  /// <summary>
  /// Выбор одного из значений по индексу.
  /// Массив значений для выбора является фиксированным.
  /// Если требуется выбор из вычисляемых значений, используйте класс DepByIndex
  /// </summary>
  /// <typeparam name="T">Тип элементов в массиве и вычисляемого значения</typeparam>
  [Serializable]
  public sealed class DepFromArray<T> : DepExpr1<T, int>
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект для заданного списка фиксированных значений
    /// </summary>
    /// <param name="indexArg">Источник данных типа Int32, определяющий выбор значения.
    /// Не может быть null/</param>
    /// <param name="values">Массив значений, из которого выполняется выбор.
    /// Не может быть null, но может, теоретически, быть пустым массивом.
    /// Не имеет смысла создавать объекты с массивами меньше, чем из двух элементов</param>
    /// <param name="defaultValue">Значение, возвращаемое, когда в источнике данных <paramref name="indexArg"/> содержится значение вне диапазона.</param>
    public DepFromArray(DepValue<int> indexArg, T[] values, T defaultValue)
      : base(indexArg, null)
    {
      if (values == null)
        throw new ArgumentNullException("values");
      _Values = values;
      _DefaultValue = defaultValue;

      BaseSetValue(Calculate(), false);
    }

    /// <summary>
    /// Создает объект для заданного списка фиксированных значений
    /// Если значение, возвращаемое <paramref name="indexArg"/>, находится вне диапазона,
    /// возвращается пустое значение default(T).
    /// </summary>
    /// <param name="indexArg">Источник данных типа Int32, определяющий выбор значения.
    /// Не может быть null/</param>
    /// <param name="values">Массив значений, из которого выполняется выбор.
    /// Не может быть null.</param>
    public DepFromArray(DepValue<int> indexArg, params T[] values)
      : this(indexArg, values, default(T))
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Массив значений для выбора
    /// </summary>
    public T[] Values { get { return _Values; } }
    private readonly T[] _Values;

    /// <summary>
    /// Значение, возвращаемое, если индекс находится вне диапазона
    /// </summary>
    public T DefaultValue { get { return _DefaultValue; } }
    private readonly T _DefaultValue;

    #endregion

    #region Расчет

    /// <summary>
    /// Выполняет расчет
    /// </summary>
    /// <returns></returns>
    protected override T Calculate()
    {
      if (Arg.Value < 0 || Arg.Value >= Values.Length)
        return DefaultValue;
      else
        return Values[Arg.Value];
    }

    #endregion
  }

  /// <summary>
  /// Выбор одного из вычисляемых значений по индексу.
  /// Если требуется выбор элемента из массива константных значений, используйте класс DepFromArray
  /// </summary>
  /// <typeparam name="T">Тип исходных данных, кроме индекса, и вычисляемого значения</typeparam>
  [Serializable]
  public sealed class DepByIndex<T> : DepExprTA<T, T>
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект для заданного списка входов
    /// </summary>
    /// <param name="indexArg">Источник данных типа Int32, определяющий выбор значения.
    /// Не может быть null/</param>
    /// <param name="args">Источники данных, из которых выполняется выбор.
    /// Не может быть null или содержать элементы null.</param>
    /// <param name="defaultArg">Источник данных, используемый, когда в источнике данных <paramref name="indexArg"/> содержится значение вне диапазона.
    /// Если источник не задан, используется константа, возвращающая значение по умолчанию для типа <typeparamref name="T"/>.</param>
    public DepByIndex(DepValue<int> indexArg, DepValue<T>[] args, DepValue<T> defaultArg)
      : base(args, null)
    {
#if DEBUG
      if (indexArg == null)
        throw new ArgumentNullException("indexArg");
#endif

      _IndexArg = indexArg;
      if (!_IndexArg.IsConst)
        _IndexArg.ValueChanged += new EventHandler(SourceValueChanged);

      if (defaultArg == null)
        _DefaultArg = new DepConst<T>(default(T));
      else
      {
        _DefaultArg = defaultArg;
        if (!_DefaultArg.IsConst)
          _DefaultArg.ValueChanged += new EventHandler(SourceValueChanged);
      }

      SourceValueChanged(null, null); // вычисляем исходное значение
    }

    /// <summary>
    /// Создает объект для заданного списка входов
    /// </summary>
    /// <param name="indexArg">Источник данных типа Int32, определяющий выбор значения.
    /// Не может быть null/</param>
    /// <param name="args">Источники данных, из которых выполняется выбор.
    /// Не может быть null или содержать элементы null.</param>
    /// <param name="defaultValue">Значение, возвращаемое, когда в источнике данных <paramref name="indexArg"/> содержится значение вне диапазона.</param>
    public DepByIndex(DepValue<int> indexArg, DepValue<T>[] args, T defaultValue)
      : this(indexArg, args, new DepConst<T>(defaultValue))
    {
    }

    /// <summary>
    /// Создает объект для заданного списка входов.
    /// Если значение, возвращаемое <paramref name="indexArg"/>, находится вне диапазона,
    /// возвращается пустое значение default(T).
    /// </summary>
    /// <param name="indexArg">Источник данных типа Int32, определяющий выбор значения.
    /// Не может быть null/</param>
    /// <param name="args">Источники данных, из которых выполняется выбор.
    /// Не может быть null или содержать элементы null.</param>
    public DepByIndex(DepValue<int> indexArg, params DepValue<T>[] args)
      : this(indexArg, args, new DepConst<T>(default(T)))
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Аргумент - индекс
    /// </summary>
    public DepValue<int> IndexArg { get { return _IndexArg; } }
    private readonly DepValue<int> _IndexArg;

    /// <summary>
    /// Значение, возвращаемое, если индекс находится вне диапазона
    /// </summary>
    public DepValue<T> DefaultArg { get { return _DefaultArg; } }
    private readonly DepValue<T> _DefaultArg;

    /// <summary>
    /// Возвращает true, если все аргументы - константы.
    /// Фактически, всегда должно возвращать false, иначе это бесполезное использование класса.
    /// </summary>
    public override bool IsConst
    {
      get
      {
        if (!_IndexArg.IsConst)
          return false; // обычно на этом и заканчивается, иначе, зачем было создавать объект
        if (!base.IsConst)
          return false;
        return _DefaultArg.IsConst;
      }
    }

    #endregion

    #region Расчет

    private void SourceValueChanged(object sender, EventArgs args)
    {
      BaseSetValue(Calculate(), false);
    }

    /// <summary>
    /// Выполняет расчет
    /// </summary>
    /// <returns></returns>
    protected override T Calculate()
    {
      if (IndexArg.Value < 0 || IndexArg.Value >= Args.Length)
        return DefaultArg.Value;
      else
        return Args[IndexArg.Value].Value;
    }

    #endregion
  }
  /// <summary>
  /// Выбор одного из значений по индексу.
  /// Массив значений для выбора является фиксированным.
  /// Если требуется выбор из вычисляемых значений, используйте класс DepByIndex
  /// </summary>
  /// <typeparam name="TKey">Тип ключа. Управляющий аргумент возвращает значение этого типа</typeparam>
  /// <typeparam name="TValue">Тип возвращаемого значения</typeparam>
  [Serializable]
  public sealed class DepFromDictionary<TKey, TValue> : DepExpr1<TValue, TKey>
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект для заданного списка фиксированных значений
    /// </summary>
    /// <param name="arg">Источник данных (ключ) типа <typeparamref name="TKey"/>, определяющий выбор значения.
    /// Не может быть null/</param>
    /// <param name="dictionary">Словарь значений, из которого выполняется выбор.
    /// Не может быть null, но может, теоретически, быть пустым словарем.</param>
    /// <param name="defaultValue">Значение, возвращаемое, когда в источнике данных <paramref name="arg"/> содержится ключ, которого нет в словаре <paramref name="dictionary"/>.</param>
    public DepFromDictionary(DepValue<TKey> arg, IDictionary<TKey, TValue> dictionary, TValue defaultValue)
      : base(arg, null)
    {
      if (dictionary == null)
        throw new ArgumentNullException("dictionary");
      _Dictionary = dictionary;
      _DefaultValue = defaultValue;

      BaseSetValue(Calculate(), false);
    }

    /// <summary>
    /// Создает объект для заданного списка фиксированных значений.
    /// При вычислении, если ключ не найден в коллекции, возвращается default(TValue)
    /// </summary>
    /// <param name="arg">Источник данных (ключ) типа <typeparamref name="TKey"/>, определяющий выбор значения.
    /// Не может быть null/</param>
    /// <param name="dictionary">Словарь значений, из которого выполняется выбор.
    /// Не может быть null, но может, теоретически, быть пустым словарем.</param>
    public DepFromDictionary(DepValue<TKey> arg, IDictionary<TKey, TValue> dictionary)
      : this(arg, dictionary, default(TValue))
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Массив значений для выбора
    /// </summary>
    public IDictionary<TKey, TValue> Dictionary { get { return _Dictionary; } }
    private readonly IDictionary<TKey, TValue> _Dictionary;

    /// <summary>
    /// Значение, возвращаемое, если индекс находится вне диапазона
    /// </summary>
    public TValue DefaultValue { get { return _DefaultValue; } }
    private readonly TValue _DefaultValue;

    #endregion

    #region Расчет

    /// <summary>
    /// Выполняет расчет
    /// </summary>
    /// <returns></returns>
    protected override TValue Calculate()
    {
      TValue res;
      if (_Dictionary.TryGetValue(Arg.Value, out res))
        return res;
      else
        return _DefaultValue;
    }

    #endregion
  }
}
