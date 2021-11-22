// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Runtime.Serialization;
using FreeLibSet.Core;

// Коллекции, использующие строки (в качестве ключа).
// Поддерживают режим IgnoreCase

namespace FreeLibSet.Collections
{
  /// <summary>
  /// Список строк с однократным вхождением.
  /// Поддерживаются варианты с учетом и без учета регистра.
  /// В варианте без учета регистра, исходный вариант регистра сохраняется.
  /// Значения null не допускаются.
  /// После установки свойства ReadOnly=true, список становится потокобезопасным.
  /// Строки хранятся в списке в порядке добавления. Используйте метод Sort() для сортировки списка
  /// </summary>
  [Serializable]
  public class SingleScopeStringList : SingleScopeList<string>
  {
    #region Конструкторы

    /// <summary>
    /// Создает пустой список
    /// </summary>
    /// <param name="ignoreCase">Нужно ли игнорировать регистр строк</param>
    public SingleScopeStringList(bool ignoreCase)
      : base(ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal)
    {
      _IgnoreCase = ignoreCase;
    }

    /// <summary>
    /// Создает пустой список
    /// </summary>
    /// <param name="comparer">Объект для сравнения строк</param>
    public SingleScopeStringList(StringComparer comparer)
      : base(comparer)
    {
      _IgnoreCase = DataTools.GetIgnoreCase(comparer);
    }

    /// <summary>
    /// Создает пустой список заданной емкости.
    /// Используйте этот конструктор, если конечное число элементов в коллекции известно с большой долей вероятности.
    /// </summary>
    /// <param name="capacity">Начальная емкость коллекции</param>
    /// <param name="ignoreCase">Нужно ли игнорировать регистр строк</param>
    public SingleScopeStringList(int capacity, bool ignoreCase)
      : base(capacity, ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal)
    {
      _IgnoreCase = ignoreCase;
    }

    /// <summary>
    /// Создает пустой список заданной емкости.
    /// Используйте этот конструктор, если конечное число элементов в коллекции известно с большой долей вероятности.
    /// </summary>
    /// <param name="capacity">Начальная емкость коллекции</param>
    /// <param name="comparer">Объект для сравнения строк</param>
    public SingleScopeStringList(int capacity, StringComparer comparer)
      : base(capacity, comparer)
    {
      _IgnoreCase = DataTools.GetIgnoreCase(comparer);
    }

    /// <summary>
    /// Создает список и заполняет его заданными значениями.
    /// </summary>
    /// <param name="src">Коллекция, откуда следует взять строки</param>
    /// <param name="ignoreCase">Нужно ли игнорировать регистр строк</param>
    public SingleScopeStringList(ICollection<string> src, bool ignoreCase)
      : base(src, ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal)
    {
      _IgnoreCase = ignoreCase;
    }

    /// <summary>
    /// Создает список и заполняет его заданными значениями.
    /// </summary>
    /// <param name="src">Коллекция, откуда следует взять строки</param>
    /// <param name="comparer">Объект для сравнения строк</param>
    public SingleScopeStringList(ICollection<string> src, StringComparer comparer)
      : base(src, comparer)
    {
      _IgnoreCase = DataTools.GetIgnoreCase(comparer);
    }

    /// <summary>
    /// Создает список и заполняет его заданными значениями.
    /// </summary>
    /// <param name="src">Коллекция, откуда следует взять строки</param>
    /// <param name="ignoreCase">Нужно ли игнорировать регистр строк</param>
    public SingleScopeStringList(IEnumerable<string> src, bool ignoreCase)
      : base(src, ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal)
    {
      _IgnoreCase = ignoreCase;
    }

    /// <summary>
    /// Создает список и заполняет его заданными значениями.
    /// </summary>
    /// <param name="src">Коллекция, откуда следует взять строки</param>
    /// <param name="comparer">Объект для сравнения строк</param>
    public SingleScopeStringList(IEnumerable<string> src, StringComparer comparer)
      : base(src, comparer)
    {
      _IgnoreCase = DataTools.GetIgnoreCase(comparer);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Возвращает true, если список не является чувствительным к регистру
    /// </summary>
    public bool IgnoreCase { get { return _IgnoreCase; } }
    private bool _IgnoreCase;

    #endregion
  }

  /// <summary>
  /// Реализация типизированной коллекции, в которой ключом являются строки, а значения имеют заданный тип.
  /// В отличие от обычной коллекции Dictionary, может быть не чувствительна к регистру ключа
  /// </summary>
  /// <typeparam name="TValue">Тип хранящихся значений</typeparam>
  [Serializable]
  public class TypedStringDictionary<TValue> : DictionaryWithReadOnly<string, TValue>, INamedValuesAccess
  {
    #region Конструкторы

    /// <summary>
    /// Создает пустую коллекцию.
    /// </summary>
    /// <param name="ignoreCase">Нужно ли игнорировать регистр строк</param>
    public TypedStringDictionary(bool ignoreCase)
      : base(ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal)
    {
      _IgnoreCase = ignoreCase;
    }

    /// <summary>
    /// Создает пустую коллекцию.
    /// </summary>
    /// <param name="comparer">Объект для сравнения строк</param>
    public TypedStringDictionary(StringComparer comparer)
      : base(comparer)
    {
      _IgnoreCase = DataTools.GetIgnoreCase(comparer);
    }

    /// <summary>
    /// Создает пустую коллекцию заданной емкости
    /// </summary>
    /// <param name="capacity">Начальная емкость коллекции</param>
    /// <param name="ignoreCase">Нужно ли игнорировать регистр строк</param>
    public TypedStringDictionary(int capacity, bool ignoreCase)
      : base(capacity, ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal)
    {
      _IgnoreCase = ignoreCase;
    }

    /// <summary>
    /// Создает пустую коллекцию заданной емкости
    /// </summary>
    /// <param name="capacity">Начальная емкость коллекции</param>
    /// <param name="comparer">Объект для сравнения строк</param>
    public TypedStringDictionary(int capacity, StringComparer comparer)
      : base(capacity, comparer)
    {
      _IgnoreCase = DataTools.GetIgnoreCase(comparer);
    }

    /// <summary>
    /// Создает коллецию и заполняет ее значениями
    /// </summary>
    /// <param name="dictionary">Исходная коллекция, откуда берутся значения для заполнения</param>
    /// <param name="ignoreCase">Нужно ли игнорировать регистр строк</param>
    public TypedStringDictionary(IDictionary<string, TValue> dictionary, bool ignoreCase)
      : this(dictionary.Count, ignoreCase)
    {
      foreach (KeyValuePair<string, TValue> Pair in dictionary)
        Add(Pair.Key, Pair.Value);
    }

    /// <summary>
    /// Создает коллецию и заполняет ее значениями
    /// </summary>
    /// <param name="dictionary">Исходная коллекция, откуда берутся значения для заполнения</param>
    /// <param name="comparer">Объект для сравнения строк</param>
    public TypedStringDictionary(IDictionary<string, TValue> dictionary, StringComparer comparer)
      : this(dictionary.Count, comparer)
    {
      _IgnoreCase = DataTools.GetIgnoreCase(comparer);

      foreach (KeyValuePair<string, TValue> Pair in dictionary)
        Add(Pair.Key, Pair.Value);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Возвращает true, если коллекция не является чувствительной к регистру
    /// </summary>
    public bool IgnoreCase { get { return _IgnoreCase; } }
    private bool _IgnoreCase;

    #endregion

    #region INamedValuesAccess Members

    object INamedValuesAccess.GetValue(string name)
    {
      return this[name];
    }

    bool INamedValuesAccess.Contains(string name)
    {
      return ContainsKey(name);
    }

    string[] INamedValuesAccess.GetNames()
    {
      string[] a = new string[Count];
      Keys.CopyTo(a, 0);
      return a;
    }

    #endregion
  }

  /// <summary>
  /// Реализация типизированной коллекции, в которой ключом являются строки, а значения имеют заданный тип.
  /// Двусторонняя коллекция, в которой можно получить не только значение для ключа, но и ключ для значения.
  /// В отличие от обычной коллекции Dictionary, ключ может быть не чувствителен к регистру ключа.
  /// Если значением <typeparamref name="TValue"/> является строка, то она всегда является чувствительной к регистру
  /// </summary>
  /// <typeparam name="TValue">Тип хранящихся значений</typeparam>
  [Serializable]
  public class BidirectionalTypedStringDictionary<TValue> : BidirectionalDictionary<string, TValue>, INamedValuesAccess
  {
    #region Конструкторы

    /// <summary>
    /// Создает пустую коллекцию.
    /// </summary>
    /// <param name="ignoreCase">Нужно ли игнорировать регистр строк</param>
    public BidirectionalTypedStringDictionary(bool ignoreCase)
      :base(ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal, null)
    {
      _IgnoreCase = ignoreCase;
    }

    /// <summary>
    /// Создает пустую коллекцию.
    /// </summary>
    /// <param name="comparer">Объект для сравнения строк</param>
    public BidirectionalTypedStringDictionary(StringComparer comparer)
      : base(comparer, null)
    {
      _IgnoreCase = DataTools.GetIgnoreCase(comparer);
    }
        
    /// <summary>
    /// Создает пустую коллекцию заданной емкости
    /// </summary>
    /// <param name="capacity">Начальная емкость коллекции</param>
    /// <param name="ignoreCase">Нужно ли игнорировать регистр строк</param>
    public BidirectionalTypedStringDictionary(int capacity, bool ignoreCase)
      :base(capacity, ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal, null)
    {
      _IgnoreCase = ignoreCase;
    }

    /// <summary>
    /// Создает пустую коллекцию заданной емкости
    /// </summary>
    /// <param name="capacity">Начальная емкость коллекции</param>
    /// <param name="comparer">Объект для сравнения строк</param>
    public BidirectionalTypedStringDictionary(int capacity, StringComparer comparer)
      : base(capacity, comparer, null)
    {
      _IgnoreCase = DataTools.GetIgnoreCase(comparer);
    }

    /// <summary>
    /// Создает коллецию и заполняет ее значениями
    /// </summary>
    /// <param name="dictionary">Исходная коллекция, откуда берутся значения для заполнения</param>
    /// <param name="ignoreCase">Нужно ли игнорировать регистр строк</param>
    public BidirectionalTypedStringDictionary(IDictionary<string, TValue> dictionary, bool ignoreCase)
      : this(dictionary.Count, ignoreCase)
    {
      foreach (KeyValuePair<string, TValue> Pair in dictionary)
        Add(Pair.Key, Pair.Value);
    }

    /// <summary>
    /// Создает коллецию и заполняет ее значениями
    /// </summary>
    /// <param name="dictionary">Исходная коллекция, откуда берутся значения для заполнения</param>
    /// <param name="comparer">Объект для сравнения строк</param>
    public BidirectionalTypedStringDictionary(IDictionary<string, TValue> dictionary, StringComparer comparer)
      : this(dictionary.Count, comparer)
    {
      foreach (KeyValuePair<string, TValue> Pair in dictionary)
        Add(Pair.Key, Pair.Value);
    }

    #endregion

    #region IgnoreCase

    /// <summary>
    /// Если true, то регистр строк не учитывается.
    /// Свойство задается в конструкторе
    /// </summary>
    public bool IgnoreCase { get { return _IgnoreCase; } }
    private bool _IgnoreCase;

    #endregion

    #region INamedValuesAccess Members

    object INamedValuesAccess.GetValue(string name)
    {
      return this[name];
    }

    bool INamedValuesAccess.Contains(string name)
    {
      return ContainsKey(name);
    }

    string[] INamedValuesAccess.GetNames()
    {
      string[] a = new string[Count];
      Keys.CopyTo(a, 0);
      return a;
    }

    #endregion
  }


  /// <summary>
  /// Простой класс, реализующий быстрый поиск элементов в массиве строк.
  /// Содержит методы Contains и IndexOf.
  /// Поддерживает игнорирование регистра
  /// Исходный массив должен подходить в качестве ключа коллекции: элементы должны быть уникальными,
  /// значения null недопустимы.
  /// Не содержит исходного массива.
  /// Этот класс не является сериализуемым, т.к. легко может быть воссоздан.
  /// Интерфейс реализует интерфейс IComparer для сортировки других массивов и списков (метод Compare()).
  /// Класс является потокобезопасным.
  /// </summary>
  public sealed class StringArrayIndexer : ArrayIndexer<string>
  {
    #region Конструкторы

    /// <summary>
    /// Создает индексатор для массива.
    /// Эта версия конструкторов учитывает регистр символов.
    /// </summary>
    /// <param name="source">Индексируемый массив</param>
    public StringArrayIndexer(string[] source)
      : this(source, false)
    {
    }

    /// <summary>
    /// Создает индексатор для массива.
    /// </summary>
    /// <param name="source">Индексируемый массив</param>
    /// <param name="ignoreCase">Нужно ли игнорировать регистр строк</param>
    public StringArrayIndexer(string[] source, bool ignoreCase)
      :base(source, ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal)
    {
      _IgnoreCase = ignoreCase;
    }

    /// <summary>
    /// Создает индексатор для массива.
    /// </summary>
    /// <param name="source">Индексируемый массив</param>
    /// <param name="comparer">Объект для сравнения строк</param>
    public StringArrayIndexer(string[] source, StringComparer comparer)
      : base(source, comparer)
    {
      _IgnoreCase = DataTools.GetIgnoreCase(comparer);
    }

    /// <summary>
    /// Создает индексатор для коллекции строк.
    /// Эта версия конструкторов учитывает регистр символов.
    /// </summary>
    /// <param name="source">Индексируемая коллекция</param>
    public StringArrayIndexer(ICollection<string> source)
      : this(source, false)
    {
    }

    /// <summary>
    /// Создает индексатор для коллекции строк.
    /// </summary>
    /// <param name="source">Индексируемая коллекция</param>
    /// <param name="ignoreCase">Нужно ли игнорировать регистр строк</param>
    public StringArrayIndexer(ICollection<string> source, bool ignoreCase)
      :base(source, ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal)
    {
      _IgnoreCase = ignoreCase;
    }

    /// <summary>
    /// Создает индексатор для коллекции строк.
    /// </summary>
    /// <param name="source">Индексируемая коллекция</param>
    /// <param name="comparer">Объект для сравнения строк</param>
    public StringArrayIndexer(ICollection<string> source, StringComparer comparer)
      : base(source, comparer)
    {
      _IgnoreCase = DataTools.GetIgnoreCase(comparer);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Если true, то регистр строк не учитывается.
    /// Свойство задается в конструкторе
    /// </summary>
    public bool IgnoreCase { get { return _IgnoreCase; } }
    private bool _IgnoreCase;

    #endregion

    #region Статический список

    /// <summary>
    /// Пустой список - индексатор
    /// </summary>
    public static readonly StringArrayIndexer Empty = new StringArrayIndexer(DataTools.EmptyStrings, false);

    #endregion
  }

  /// <summary>
  /// Простой класс, реализующий быстрый поиск символов.
  /// Содержит методы Contains() и IndexOf().
  /// Исходный массив должен подходить в качестве ключа коллекции: элементы должны быть уникальными,
  /// значения null недопустимы.
  /// Не содержит исходного массива.
  /// Этот класс не является сериализуемым, т.к. легко может быть воссоздан.
  /// Идентичен ArrayIndexer of Char за исключением конструкторов из строки.
  /// Также поддерживает поиск с игнорированием регистра
  /// Интерфейс реализует интерфейс IComparer для сортировки других массивов и списков (метод Compare()).
  /// Класс является потокобезопасным.
  /// </summary>
  public sealed class CharArrayIndexer : IComparer<char>
  {
    // Требуется собственная реализация, а не через ArrayIndexer, т.к. StringComparer не реализует IEqualityComparer для Char

    #region Конструкторы

    /// <summary>
    /// Создает индексатор для массива.
    /// В отличие от ArrayIndexer of Char, допускается наличие в <paramref name="source"/> повторяющихся символов, которые отбрасываются.
    /// Сравнение будет чувствительным к регистру символов.
    /// </summary>
    /// <param name="source">Массив символов</param>
    public CharArrayIndexer(char[] source)
      : this(source, false)
    {
    }

    /// <summary>
    /// Создает индексатор для массива.
    /// В отличие от ArrayIndexer of Char, допускается наличие в <paramref name="source"/> повторяющихся символов, которые отбрасываются
    /// </summary>
    /// <param name="source">Массив символов</param>
    /// <param name="ignoreCase">Если true, то будет игнорироваться регистр символов</param>
    public CharArrayIndexer(char[] source, bool ignoreCase)
    {
      if (source == null)
        throw new ArgumentNullException("source");

      _IgnoreCase = ignoreCase;

      _Dict = new Dictionary<char, int>(source.Length);
      for (int i = 0; i < source.Length; i++)
      {
        char ch = source[i];
        if (ignoreCase)
          ch = Char.ToUpperInvariant(ch);
        _Dict[ch] = i;
      }
    }


    /// <summary>
    /// Создает индексатор для строки символов
    /// Допускается наличие в <paramref name="source"/> повторяющихся символов, которые отбрасываются
    /// Сравнение будет чувствительным к регистру символов.
    /// </summary>
    /// <param name="source">Строка символов</param>
    public CharArrayIndexer(string source)
      : this(source, false)
    {
    }

    /// <summary>
    /// Создает индексатор для строки символов
    /// Допускается наличие в <paramref name="source"/> повторяющихся символов, которые отбрасываются
    /// </summary>
    /// <param name="source">Строка символов</param>
    /// <param name="ignoreCase">Если true, то будет игнорироваться регистр символов</param>
    public CharArrayIndexer(string source, bool ignoreCase)
    {
      if (source == null)
        source = String.Empty;

      _IgnoreCase = ignoreCase;

      if (ignoreCase)
        source = source.ToUpperInvariant();

      _Dict = new Dictionary<char, int>(source.Length);
      for (int i = 0; i < source.Length; i++)
        _Dict[source[i]] = i;
    }

    private CharArrayIndexer()
      : this(String.Empty, false)
    {
      _IsReadOnly = true;
    }

    #endregion

    #region Свойства

    private Dictionary<char, int> _Dict;

    /// <summary>
    /// Количество элементов в массиве
    /// </summary>
    public int Count { get { return _Dict.Count; } }

    /// <summary>
    /// Если true, то регистр строк не учитывается.
    /// Свойство задается в конструкторе
    /// </summary>
    public bool IgnoreCase { get { return _IgnoreCase; } }
    private bool _IgnoreCase;

    #endregion

    #region Методы

    /// <summary>
    /// Текстовое представление для отладки
    /// </summary>
    /// <returns>Строка вида "Count=XXX"</returns>
    public override string ToString()
    {
      return "Count=" + _Dict.Count.ToString();
    }

    /// <summary>
    /// Возвращает индекс элемента в массиве.
    /// В отличие от Array.IndexOf(), выполняется быстро
    /// </summary>
    /// <param name="item">Символ для поиска</param>
    /// <returns>Индекс элемента</returns>
    public int IndexOf(char item)
    {
      if (_IgnoreCase)
        item = Char.ToUpperInvariant(item);

      int p;
      if (_Dict.TryGetValue(item, out p))
        return p;
      else
        return -1;
    }

    /// <summary>
    /// Возвращает индекс любого символа из строки <paramref name="s"/>, если он есть в текущем массиве.
    /// Если в текущем массиве нет ни одного символа из строки <paramref name="s"/>, возвращается (-1)
    /// </summary>
    /// <param name="s">Символы для поиска</param>
    /// <returns>Индекс первого найденного символа</returns>
    public int IndexOfAny(string s)
    {
      if (String.IsNullOrEmpty(s))
        return -1;

      if (_IgnoreCase)
        s = s.ToUpperInvariant();

      int p;
      for (int i = 0; i < s.Length; i++)
      {
        if (_Dict.TryGetValue(s[i], out p))
          return p;
      }
      return -1;
    }

    /// <summary>
    /// Возвращает true, если элемент есть в исходном массиве
    /// </summary>
    /// <param name="item">Символ для поиска</param>
    /// <returns>true, если символ есть в списке</returns>
    public bool Contains(char item)
    {
      if (_IgnoreCase)
        item = Char.ToUpperInvariant(item);

      return _Dict.ContainsKey(item);
    }

    /// <summary>
    /// Возвращает true, если в текущем индексаторе есть хотя бы один символ из строки <paramref name="s"/>
    /// </summary>
    /// <param name="s">Проверяемая строка символов</param>
    /// <returns>результат поиска</returns>
    public bool ContainsAny(string s)
    {
      if (String.IsNullOrEmpty(s))
        return false;

      if (_IgnoreCase)
        s = s.ToUpperInvariant();

      for (int i = 0; i < s.Length; i++)
      {
        if (_Dict.ContainsKey(s[i]))
          return true;
      }
      return false;
    }

    /// <summary>
    /// Возвращает true, если в текущем индексаторе есть все символы из строки <paramref name="s"/>
    /// </summary>
    /// <param name="s">Проверяемая строка символов</param>
    /// <returns>результат поиска</returns>
    public bool ContainsAll(string s)
    {
      if (String.IsNullOrEmpty(s))
        return true;

      if (_IgnoreCase)
        s = s.ToUpperInvariant();

      for (int i = 0; i < s.Length; i++)
      {
        if (!_Dict.ContainsKey(s[i]))
          return false;
      }
      return true;
    }

    /// <summary>
    /// Возвращает true, если в списке содержатся все элементы, то есть если Contains() возвращает true для каждого элемента.
    /// Если проверяемый список пустой, возвращает true.
    /// </summary>
    /// <param name="items">Проверяемый список элементов</param>
    /// <returns>Наличие элементов</returns>
    public bool ContainsAll(IEnumerable<char> items)
    {
      foreach (char item in items)
      {
        if (!Contains(item))
          return false;
      }
      return true;
    }

    /// <summary>
    /// Возвращает true, если в списке есть хотя бы один элемент, то есть если Contains() возвращает true для какого-либо элемента
    /// Если проверяемый список пустой, возвращает false.
    /// </summary>
    /// <param name="items">Проверяемый список элементов</param>
    /// <returns>Наличие элементов</returns>
    public bool ContainsAny(IEnumerable<char> items)
    {
      foreach (char item in items)
      {
        if (Contains(item))
          return true;
      }
      return false;
    }

    #endregion

    #region IComparer<T> members

    /// <summary>
    /// Устанавливается для списка Empty
    /// </summary>
    private bool _IsReadOnly;

    /// <summary>
    /// Положение ненайденных элементов при сортировке с помощью метода Compare().
    /// По умолчанию - First - ненайденные элементы располагаются в начале списка.
    /// </summary>
    public UnknownItemPosition UnknownItemPosition
    {
      get { return _UnknownItemPosition; }
      set
      {
        if (_IsReadOnly)
          throw new ObjectReadOnlyException();

        switch (value)
        {
          case FreeLibSet.Collections.UnknownItemPosition.First:
          case FreeLibSet.Collections.UnknownItemPosition.Last:
            _UnknownItemPosition = value;
            break;
          default:
            throw new ArgumentException();
        }
      }
    }
    private UnknownItemPosition _UnknownItemPosition;

    /// <summary>
    /// Сравнение положения двух символов.
    /// Метод может быть использован для сортировки произвольных списков и массивов, чтобы
    /// отсортировать их в соответствии с порядком элементов в текущем объекте CharArrayIndexer.
    /// Сравнивается положение элементов в текущем объекте, а не коды символов.
    /// Если какое-либо значение отсутствует в текущем объекте, то оно будет расположено в
    /// начале или в конце списка, в зависимости от свойства UnknownItemPosition.
    /// Если обоих значений нет в текущем списке, то возвращается результат сравнения значений.
    /// 
    /// Метод возвращает отрицательное значение, если <paramref name="x"/> располагается ближе
    /// к началу списка, чем <paramref name="y"/>. Положительное значение возвращается, если
    /// <paramref name="x"/> располагается ближе к концу списка, чем <paramref name="y"/>. 
    /// </summary>
    /// <param name="x">Первое сравниваемое значение</param>
    /// <param name="y">Второе сравниваемое значение</param>
    /// <returns>Результат сравнение позиций</returns>
    public int Compare(char x, char y)
    {
      int px = IndexOf(x);
      int py = IndexOf(y);

      if (px < 0 && py < 0)
      {
        // Если обоих элементов нет в списке, сравниваем элементы
        return x.CompareTo(y);
      }

      if (UnknownItemPosition == FreeLibSet.Collections.UnknownItemPosition.Last)
      {
        if (px < 0)
          px = int.MaxValue;
        if (py < 0)
          py = int.MaxValue;
      }

      return px.CompareTo(py);
    }

    #endregion

    #region Статический список

    /// <summary>
    /// Пустой список - индексатор
    /// </summary>
    public static readonly CharArrayIndexer Empty = new CharArrayIndexer();

    #endregion
  }

  #region Числовые коллекции со строковым ключом

  /// <summary>
  /// Словарь числовых значений со строковым ключом.
  /// Словарь может быть не чувствителен к регистру ключа (определяется в конструкторе).
  /// </summary>
  [Serializable]
  public class IntNamedDictionary : TypedStringDictionary<int>
  {
    #region Конструкторы

    /// <summary>
    /// Основная версия конструктора.
    /// Создает словарь с ключом, чувствительным к регистру
    /// </summary>
    public IntNamedDictionary()
      : this(false)
    {
    }

    /// <summary>
    /// Создает словарь с указанием чувствительности к регистру
    /// </summary>
    /// <param name="ignoreCase">Нужно ли игнорировать регистр ключа</param>
    public IntNamedDictionary(bool ignoreCase)
      : base(ignoreCase)
    {
    }

    /// <summary>
    /// Создает словарь с ключом, чувствительным к регистру
    /// Эту версию следует использовать, если заранее известно, сколько будет элементов в словаре.
    /// </summary>
    /// <param name="capacity">Начальная емкость коллекции</param>
    public IntNamedDictionary(int capacity)
      : this(capacity, false)
    {
    }

    /// <summary>
    /// Создает словарь с указанием чувствительности к регистру.
    /// Эту версию следует использовать, если заранее известно, сколько будет элементов в словаре.
    /// </summary>
    /// <param name="capacity">Начальная емкость коллекции</param>
    /// <param name="ignoreCase">Нужно ли игнорировать регистр ключа</param>
    public IntNamedDictionary(int capacity, bool ignoreCase)
      : base(capacity, ignoreCase)
    {
    }

    /// <summary>
    /// Создает словарь с ключом, чувствительным к регистру и заполняет его значениями.
    /// </summary>
    /// <param name="dictionary">Источник, откуда берутся значения</param>
    public IntNamedDictionary(IDictionary<string, int> dictionary)
      : this(dictionary, false)
    {
    }

    /// <summary>
    /// Создает словарь с указанием чувствительности к регистру, и заполняет его значениями.
    /// </summary>
    /// <param name="dictionary">Источник, откуда берутся значения</param>
    /// <param name="ignoreCase">Нужно ли игнорировать регистр ключа</param>
    public IntNamedDictionary(IDictionary<string, int> dictionary, bool ignoreCase)
      : base(dictionary, ignoreCase)
    {
    }

    #endregion

    #region Доступ к значениям

    /// <summary>
    /// Получение или установка значения.
    /// Если в словаре нет значения с указанным ключом, возвращается 0
    /// (без выброса исключения, как это принято в стандартных коллекциях)
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <returns>Значение</returns>
    public new int this[string key]
    {
      get
      {
        int v;
        if (base.TryGetValue(key, out v))
          return v;
        else
          return 0;
      }
      set
      {
        base[key] = value;
      }
    }

    #endregion

    #region Сложение и вычитание

    /// <summary>
    /// Добавить к текущей коллекции значения из другой коллекции.
    /// Текущая коллекция должна быть доступна для записи (IsReadOnly=false).
    /// </summary>
    /// <param name="source">Словарь "Код-Значения", откуда берутся добавляемые значения.
    /// Если null, то никаких действий не выполняется</param>
    public void Add(IDictionary<string, int> source)
    {
      if (source == null)
        return;

      foreach (KeyValuePair<string, int> Pair in source)
        checked { this[Pair.Key] += Pair.Value; }
    }

    /// <summary>
    /// Вычесть из текущей коллекции значения из другой коллекции.
    /// Текущая коллекция должна быть доступна для записи (IsReadOnly=false).
    /// </summary>
    /// <param name="source">Словарь "Код-Значения", откуда берутся вычитаемые значения.
    /// Если null, то никаких действий не выполняется</param>
    public void Substract(IDictionary<string, int> source)
    {
      if (source == null)
        return;

      foreach (KeyValuePair<string, int> Pair in source)
        checked { this[Pair.Key] -= Pair.Value; }
    }

    /// <summary>
    /// Сложение двух коллекций.
    /// При выполнении оператора всегда создается новая коллекция, поэтому обычно экономичнее использовать
    /// нестатический метод Add(), который мофицирует существующую коллекцию.
    /// Свойство IgnoreCase новой коллекции устанавливается в соответствии со значением свойства первой коллекции.
    /// </summary>
    /// <param name="a">Первая исходная коллекция</param>
    /// <param name="b">Вторая исходная коллекция</param>
    /// <returns>Новая коллекция</returns>
    public static IntNamedDictionary operator +(IntNamedDictionary a, IDictionary<string, int> b)
    {
      IntNamedDictionary Res = new IntNamedDictionary(a, a.IgnoreCase);
      Res.Add(b);
      return Res;
    }

    /// <summary>
    /// Вычитание одной коллекции из другой.
    /// При выполнении оператора всегда создается новая коллекция, поэтому обычно экономичнее использовать
    /// нестатический метод Substract(), который мофицирует существующую коллекцию.
    /// Свойство IgnoreCase новой коллекции устанавливается в соответствии со значением свойства первой коллекции.
    /// </summary>
    /// <param name="a">Первая исходная коллекция</param>
    /// <param name="b">Вторая исходная коллекция</param>
    /// <returns>Новая коллекция</returns>
    public static IntNamedDictionary operator -(IntNamedDictionary a, IDictionary<string, int> b)
    {
      IntNamedDictionary Res = new IntNamedDictionary(a, a.IgnoreCase);
      Res.Substract(b);
      return Res;
    }

    #endregion

    #region Умножение и деление

    /// <summary>
    /// Умножение всех значений коллекции на заданное число.
    /// Текущая коллекция должна быть доступна для записи (IsReadOnly=false).
    /// </summary>
    /// <param name="m">Множитель</param>
    public void Multiply(int m)
    {
      string[] Codes = new string[base.Count];
      base.Keys.CopyTo(Codes, 0);
      for (int i = 0; i < Codes.Length; i++)
        checked { this[Codes[i]] *= m; }
    }

    /// <summary>
    /// Деление всех значений коллекции на заданное число.
    /// Деление выполняется с выполнением округления по правилам для целых чисел.
    /// Текущая коллекция должна быть доступна для записи (IsReadOnly=false).
    /// </summary>
    /// <param name="d">Делитель. Не может быть равен 0</param>
    public void Divide(int d)
    {
      string[] Codes = new string[base.Count];
      base.Keys.CopyTo(Codes, 0);
      for (int i = 0; i < Codes.Length; i++)
        checked { this[Codes[i]] /= d; }
    }

    #endregion
  }

  /// <summary>
  /// Словарь числовых значений со строковым ключом.
  /// Словарь может быть не чувствителен к регистру ключа (определяется в конструкторе).
  /// </summary>
  [Serializable]
  public class Int64NamedDictionary : TypedStringDictionary<long>
  {
    #region Конструкторы

    /// <summary>
    /// Основная версия конструктора.
    /// Создает словарь с ключом, чувствительным к регистру
    /// </summary>
    public Int64NamedDictionary()
      : this(false)
    {
    }

    /// <summary>
    /// Создает словарь с указанием чувствительности к регистру
    /// </summary>
    /// <param name="ignoreCase">Нужно ли игнорировать регистр ключа</param>
    public Int64NamedDictionary(bool ignoreCase)
      : base(ignoreCase)
    {
    }

    /// <summary>
    /// Создает словарь с ключом, чувствительным к регистру
    /// Эту версию следует использовать, если заранее известно, сколько будет элементов в словаре.
    /// </summary>
    /// <param name="capacity">Начальная емкость коллекции</param>
    public Int64NamedDictionary(int capacity)
      : this(capacity, false)
    {
    }

    /// <summary>
    /// Создает словарь с указанием чувствительности к регистру.
    /// Эту версию следует использовать, если заранее известно, сколько будет элементов в словаре.
    /// </summary>
    /// <param name="capacity">Начальная емкость коллекции</param>
    /// <param name="ignoreCase">Нужно ли игнорировать регистр ключа</param>
    public Int64NamedDictionary(int capacity, bool ignoreCase)
      : base(capacity, ignoreCase)
    {
    }

    /// <summary>
    /// Создает словарь с ключом, чувствительным к регистру и заполняет его значениями.
    /// </summary>
    /// <param name="dictionary">Источник, откуда берутся значения</param>
    public Int64NamedDictionary(IDictionary<string, long> dictionary)
      : this(dictionary, false)
    {
    }

    /// <summary>
    /// Создает словарь с указанием чувствительности к регистру, и заполняет его значениями.
    /// </summary>
    /// <param name="dictionary">Источник, откуда берутся значения</param>
    /// <param name="ignoreCase">Нужно ли игнорировать регистр ключа</param>
    public Int64NamedDictionary(IDictionary<string, long> dictionary, bool ignoreCase)
      : base(dictionary, ignoreCase)
    {
    }

    #endregion

    #region Доступ к значениям

    /// <summary>
    /// Получение или установка значения.
    /// Если в словаре нет значения с указанным ключом, возвращается 0
    /// (без выброса исключения, как это принято в стандартных коллекциях)
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <returns>Значение</returns>
    public new long this[string key]
    {
      get
      {
        long v;
        if (base.TryGetValue(key, out v))
          return v;
        else
          return 0L;
      }
      set
      {
        base[key] = value;
      }
    }

    #endregion

    #region Сложение и вычитание

    /// <summary>
    /// Добавить к текущей коллекции значения из другой коллекции.
    /// Текущая коллекция должна быть доступна для записи (IsReadOnly=false).
    /// </summary>
    /// <param name="source">Словарь "Код-Значения", откуда берутся добавляемые значения.
    /// Если null, то никаких действий не выполняется</param>
    public void Add(IDictionary<string, long> source)
    {
      if (source == null)
        return;

      foreach (KeyValuePair<string, long> Pair in source)
        checked { this[Pair.Key] += Pair.Value; }
    }

    /// <summary>
    /// Вычесть из текущей коллекции значения из другой коллекции.
    /// Текущая коллекция должна быть доступна для записи (IsReadOnly=false).
    /// </summary>
    /// <param name="source">Словарь "Код-Значения", откуда берутся вычитаемые значения.
    /// Если null, то никаких действий не выполняется</param>
    public void Substract(IDictionary<string, long> source)
    {
      if (source == null)
        return;

      foreach (KeyValuePair<string, long> Pair in source)
        checked { this[Pair.Key] -= Pair.Value; }
    }

    /// <summary>
    /// Сложение двух коллекций.
    /// При выполнении оператора всегда создается новая коллекция, поэтому обычно экономичнее использовать
    /// нестатический метод Add(), который мофицирует существующую коллекцию.
    /// Свойство IgnoreCase новой коллекции устанавливается в соответствии со значением свойства первой коллекции.
    /// </summary>
    /// <param name="a">Первая исходная коллекция</param>
    /// <param name="b">Вторая исходная коллекция</param>
    /// <returns>Новая коллекция</returns>
    public static Int64NamedDictionary operator +(Int64NamedDictionary a, IDictionary<string, long> b)
    {
      Int64NamedDictionary Res = new Int64NamedDictionary(a, a.IgnoreCase);
      Res.Add(b);
      return Res;
    }

    /// <summary>
    /// Вычитание одной коллекции из другой.
    /// При выполнении оператора всегда создается новая коллекция, поэтому обычно экономичнее использовать
    /// нестатический метод Substract(), который мофицирует существующую коллекцию.
    /// Свойство IgnoreCase новой коллекции устанавливается в соответствии со значением свойства первой коллекции.
    /// </summary>
    /// <param name="a">Первая исходная коллекция</param>
    /// <param name="b">Вторая исходная коллекция</param>
    /// <returns>Новая коллекция</returns>
    public static Int64NamedDictionary operator -(Int64NamedDictionary a, IDictionary<string, long> b)
    {
      Int64NamedDictionary Res = new Int64NamedDictionary(a, a.IgnoreCase);
      Res.Substract(b);
      return Res;
    }

    #endregion

    #region Умножение и деление

    /// <summary>
    /// Умножение всех значений коллекции на заданное число.
    /// Текущая коллекция должна быть доступна для записи (IsReadOnly=false).
    /// </summary>
    /// <param name="m">Множитель</param>
    public void Multiply(long m)
    {
      string[] Codes = new string[base.Count];
      base.Keys.CopyTo(Codes, 0);
      for (int i = 0; i < Codes.Length; i++)
        checked { this[Codes[i]] *= m; }
    }

    /// <summary>
    /// Деление всех значений коллекции на заданное число.
    /// Деление выполняется с выполнением округления по правилам для целых чисел.
    /// Текущая коллекция должна быть доступна для записи (IsReadOnly=false).
    /// </summary>
    /// <param name="d">Делитель. Не может быть равен 0</param>
    public void Divide(long d)
    {
      string[] Codes = new string[base.Count];
      base.Keys.CopyTo(Codes, 0);
      for (int i = 0; i < Codes.Length; i++)
        checked { this[Codes[i]] /= d; }
    }

    #endregion
  }

  /// <summary>
  /// Словарь числовых значений со строковым ключом.
  /// Словарь может быть не чувствителен к регистру ключа (определяется в конструкторе).
  /// </summary>
  [Serializable]
  public class SingleNamedDictionary : TypedStringDictionary<float>
  {
    #region Конструкторы

    /// <summary>
    /// Основная версия конструктора.
    /// Создает словарь с ключом, чувствительным к регистру
    /// </summary>
    public SingleNamedDictionary()
      : this(false)
    {
    }

    /// <summary>
    /// Создает словарь с указанием чувствительности к регистру
    /// </summary>
    /// <param name="ignoreCase">Нужно ли игнорировать регистр ключа</param>
    public SingleNamedDictionary(bool ignoreCase)
      : base(ignoreCase)
    {
    }

    /// <summary>
    /// Создает словарь с ключом, чувствительным к регистру
    /// Эту версию следует использовать, если заранее известно, сколько будет элементов в словаре.
    /// </summary>
    /// <param name="capacity">Начальная емкость коллекции</param>
    public SingleNamedDictionary(int capacity)
      : this(capacity, false)
    {
    }

    /// <summary>
    /// Создает словарь с указанием чувствительности к регистру.
    /// Эту версию следует использовать, если заранее известно, сколько будет элементов в словаре.
    /// </summary>
    /// <param name="capacity">Начальная емкость коллекции</param>
    /// <param name="ignoreCase">Нужно ли игнорировать регистр ключа</param>
    public SingleNamedDictionary(int capacity, bool ignoreCase)
      : base(capacity, ignoreCase)
    {
    }

    /// <summary>
    /// Создает словарь с ключом, чувствительным к регистру и заполняет его значениями.
    /// </summary>
    /// <param name="dictionary">Источник, откуда берутся значения</param>
    public SingleNamedDictionary(IDictionary<string, float> dictionary)
      : this(dictionary, false)
    {
    }

    /// <summary>
    /// Создает словарь с указанием чувствительности к регистру, и заполняет его значениями.
    /// </summary>
    /// <param name="dictionary">Источник, откуда берутся значения</param>
    /// <param name="ignoreCase">Нужно ли игнорировать регистр ключа</param>
    public SingleNamedDictionary(IDictionary<string, float> dictionary, bool ignoreCase)
      : base(dictionary, ignoreCase)
    {
    }

    #endregion

    #region Доступ к значениям

    /// <summary>
    /// Получение или установка значения.
    /// Если в словаре нет значения с указанным ключом, возвращается 0
    /// (без выброса исключения, как это принято в стандартных коллекциях)
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <returns>Значение</returns>
    public new float this[string key]
    {
      get
      {
        float v;
        if (base.TryGetValue(key, out v))
          return v;
        else
          return 0f;
      }
      set
      {
        base[key] = value;
      }
    }

    #endregion

    #region Сложение и вычитание

    /// <summary>
    /// Добавить к текущей коллекции значения из другой коллекции.
    /// Текущая коллекция должна быть доступна для записи (IsReadOnly=false).
    /// </summary>
    /// <param name="source">Словарь "Код-Значения", откуда берутся добавляемые значения.
    /// Если null, то никаких действий не выполняется</param>
    public void Add(IDictionary<string, float> source)
    {
      if (source == null)
        return;

      foreach (KeyValuePair<string, float> Pair in source)
        this[Pair.Key] += Pair.Value;
    }

    /// <summary>
    /// Вычесть из текущей коллекции значения из другой коллекции.
    /// Текущая коллекция должна быть доступна для записи (IsReadOnly=false).
    /// </summary>
    /// <param name="source">Словарь "Код-Значения", откуда берутся вычитаемые значения.
    /// Если null, то никаких действий не выполняется</param>
    public void Substract(IDictionary<string, float> source)
    {
      if (source == null)
        return;

      foreach (KeyValuePair<string, float> Pair in source)
        this[Pair.Key] -= Pair.Value;
    }

    /// <summary>
    /// Сложение двух коллекций.
    /// При выполнении оператора всегда создается новая коллекция, поэтому обычно экономичнее использовать
    /// нестатический метод Add(), который мофицирует существующую коллекцию.
    /// Свойство IgnoreCase новой коллекции устанавливается в соответствии со значением свойства первой коллекции.
    /// </summary>
    /// <param name="a">Первая исходная коллекция</param>
    /// <param name="b">Вторая исходная коллекция</param>
    /// <returns>Новая коллекция</returns>
    public static SingleNamedDictionary operator +(SingleNamedDictionary a, IDictionary<string, float> b)
    {
      SingleNamedDictionary Res = new SingleNamedDictionary(a, a.IgnoreCase);
      Res.Add(b);
      return Res;
    }

    /// <summary>
    /// Вычитание одной коллекции из другой.
    /// При выполнении оператора всегда создается новая коллекция, поэтому обычно экономичнее использовать
    /// нестатический метод Substract(), который мофицирует существующую коллекцию.
    /// Свойство IgnoreCase новой коллекции устанавливается в соответствии со значением свойства первой коллекции.
    /// </summary>
    /// <param name="a">Первая исходная коллекция</param>
    /// <param name="b">Вторая исходная коллекция</param>
    /// <returns>Новая коллекция</returns>
    public static SingleNamedDictionary operator -(SingleNamedDictionary a, IDictionary<string, float> b)
    {
      SingleNamedDictionary Res = new SingleNamedDictionary(a, a.IgnoreCase);
      Res.Substract(b);
      return Res;
    }

    #endregion

    #region Умножение и деление

    /// <summary>
    /// Умножение всех значений коллекции на заданное число.
    /// Текущая коллекция должна быть доступна для записи (IsReadOnly=false).
    /// </summary>
    /// <param name="m">Множитель</param>
    public void Multiply(float m)
    {
      string[] Codes = new string[base.Count];
      base.Keys.CopyTo(Codes, 0);
      for (int i = 0; i < Codes.Length; i++)
        this[Codes[i]] *= m;
    }

    /// <summary>
    /// Деление всех значений коллекции на заданное число.
    /// Деление выполняется без округления.
    /// Используйте метод Round() после выполнения деления
    /// Текущая коллекция должна быть доступна для записи (IsReadOnly=false).
    /// </summary>
    /// <param name="d">Делитель. Не может быть равен 0</param>
    public void Divide(float d)
    {
      string[] Codes = new string[base.Count];
      base.Keys.CopyTo(Codes, 0);
      for (int i = 0; i < Codes.Length; i++)
        this[Codes[i]] /= d;
    }

    #endregion

    #region Округление

    /// <summary>
    /// Выполняет округление всех элементов коллекции до заданного числа знаков после запятой.
    /// Используются правила математического округления.
    /// </summary>
    /// <param name="decimals">Число знаков после запятой</param>
    public void Round(int decimals)
    {
      string[] Codes = new string[base.Count];
      base.Keys.CopyTo(Codes, 0);
      for (int i = 0; i < Codes.Length; i++)
        this[Codes[i]] = (float)(Math.Round((double)(this[Codes[i]]), decimals, MidpointRounding.AwayFromZero));
    }

    /// <summary>
    /// Выполняет округление всех элементов коллекции до целых значений.
    /// Используются правила математического округления.
    /// </summary>
    public void Round()
    {
      Round(0);
    }

    #endregion
  }

  /// <summary>
  /// Словарь числовых значений со строковым ключом.
  /// Словарь может быть не чувствителен к регистру ключа (определяется в конструкторе).
  /// </summary>
  [Serializable]
  public class DoubleNamedDictionary : TypedStringDictionary<double>
  {
    #region Конструкторы

    /// <summary>
    /// Основная версия конструктора.
    /// Создает словарь с ключом, чувствительным к регистру
    /// </summary>
    public DoubleNamedDictionary()
      : this(false)
    {
    }

    /// <summary>
    /// Создает словарь с указанием чувствительности к регистру
    /// </summary>
    /// <param name="ignoreCase">Нужно ли игнорировать регистр ключа</param>
    public DoubleNamedDictionary(bool ignoreCase)
      : base(ignoreCase)
    {
    }

    /// <summary>
    /// Создает словарь с ключом, чувствительным к регистру
    /// Эту версию следует использовать, если заранее известно, сколько будет элементов в словаре.
    /// </summary>
    /// <param name="capacity">Начальная емкость коллекции</param>
    public DoubleNamedDictionary(int capacity)
      : this(capacity, false)
    {
    }

    /// <summary>
    /// Создает словарь с указанием чувствительности к регистру.
    /// Эту версию следует использовать, если заранее известно, сколько будет элементов в словаре.
    /// </summary>
    /// <param name="capacity">Начальная емкость коллекции</param>
    /// <param name="ignoreCase">Нужно ли игнорировать регистр ключа</param>
    public DoubleNamedDictionary(int capacity, bool ignoreCase)
      : base(capacity, ignoreCase)
    {
    }

    /// <summary>
    /// Создает словарь с ключом, чувствительным к регистру и заполняет его значениями.
    /// </summary>
    /// <param name="dictionary">Источник, откуда берутся значения</param>
    public DoubleNamedDictionary(IDictionary<string, double> dictionary)
      : this(dictionary, false)
    {
    }

    /// <summary>
    /// Создает словарь с указанием чувствительности к регистру, и заполняет его значениями.
    /// </summary>
    /// <param name="dictionary">Источник, откуда берутся значения</param>
    /// <param name="ignoreCase">Нужно ли игнорировать регистр ключа</param>
    public DoubleNamedDictionary(IDictionary<string, double> dictionary, bool ignoreCase)
      : base(dictionary, ignoreCase)
    {
    }

    #endregion

    #region Доступ к значениям

    /// <summary>
    /// Получение или установка значения.
    /// Если в словаре нет значения с указанным ключом, возвращается 0
    /// (без выброса исключения, как это принято в стандартных коллекциях)
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <returns>Значение</returns>
    public new double this[string key]
    {
      get
      {
        double v;
        if (base.TryGetValue(key, out v))
          return v;
        else
          return 0.0;
      }
      set
      {
        base[key] = value;
      }
    }

    #endregion

    #region Сложение и вычитание

    /// <summary>
    /// Добавить к текущей коллекции значения из другой коллекции.
    /// Текущая коллекция должна быть доступна для записи (IsReadOnly=false).
    /// </summary>
    /// <param name="source">Словарь "Код-Значения", откуда берутся добавляемые значения.
    /// Если null, то никаких действий не выполняется</param>
    public void Add(IDictionary<string, double> source)
    {
      if (source == null)
        return;

      foreach (KeyValuePair<string, double> Pair in source)
        this[Pair.Key] += Pair.Value;
    }

    /// <summary>
    /// Вычесть из текущей коллекции значения из другой коллекции.
    /// Текущая коллекция должна быть доступна для записи (IsReadOnly=false).
    /// </summary>
    /// <param name="source">Словарь "Код-Значения", откуда берутся вычитаемые значения.
    /// Если null, то никаких действий не выполняется</param>
    public void Substract(IDictionary<string, double> source)
    {
      if (source == null)
        return;

      foreach (KeyValuePair<string, double> Pair in source)
        this[Pair.Key] -= Pair.Value;
    }

    /// <summary>
    /// Сложение двух коллекций.
    /// При выполнении оператора всегда создается новая коллекция, поэтому обычно экономичнее использовать
    /// нестатический метод Add(), который мофицирует существующую коллекцию.
    /// Свойство IgnoreCase новой коллекции устанавливается в соответствии со значением свойства первой коллекции.
    /// </summary>
    /// <param name="a">Первая исходная коллекция</param>
    /// <param name="b">Вторая исходная коллекция</param>
    /// <returns>Новая коллекция</returns>
    public static DoubleNamedDictionary operator +(DoubleNamedDictionary a, IDictionary<string, double> b)
    {
      DoubleNamedDictionary Res = new DoubleNamedDictionary(a, a.IgnoreCase);
      Res.Add(b);
      return Res;
    }

    /// <summary>
    /// Вычитание одной коллекции из другой.
    /// При выполнении оператора всегда создается новая коллекция, поэтому обычно экономичнее использовать
    /// нестатический метод Substract(), который мофицирует существующую коллекцию.
    /// Свойство IgnoreCase новой коллекции устанавливается в соответствии со значением свойства первой коллекции.
    /// </summary>
    /// <param name="a">Первая исходная коллекция</param>
    /// <param name="b">Вторая исходная коллекция</param>
    /// <returns>Новая коллекция</returns>
    public static DoubleNamedDictionary operator -(DoubleNamedDictionary a, IDictionary<string, double> b)
    {
      DoubleNamedDictionary Res = new DoubleNamedDictionary(a, a.IgnoreCase);
      Res.Substract(b);
      return Res;
    }

    #endregion

    #region Умножение и деление

    /// <summary>
    /// Умножение всех значений коллекции на заданное число.
    /// Текущая коллекция должна быть доступна для записи (IsReadOnly=false).
    /// </summary>
    /// <param name="m">Множитель</param>
    public void Multiply(double m)
    {
      string[] Codes = new string[base.Count];
      base.Keys.CopyTo(Codes, 0);
      for (int i = 0; i < Codes.Length; i++)
        this[Codes[i]] *= m;
    }

    /// <summary>
    /// Деление всех значений коллекции на заданное число.
    /// Деление выполняется без округления.
    /// Используйте метод Round() после выполнения деления
    /// Текущая коллекция должна быть доступна для записи (IsReadOnly=false).
    /// </summary>
    /// <param name="d">Делитель. Не может быть равен 0</param>
    public void Divide(double d)
    {
      string[] Codes = new string[base.Count];
      base.Keys.CopyTo(Codes, 0);
      for (int i = 0; i < Codes.Length; i++)
        this[Codes[i]] /= d;
    }

    #endregion

    #region Округление

    /// <summary>
    /// Выполняет округление всех элементов коллекции до заданного числа знаков после запятой.
    /// Используются правила математического округления.
    /// </summary>
    /// <param name="decimals">Число знаков после запятой</param>
    public void Round(int decimals)
    {
      string[] Codes = new string[base.Count];
      base.Keys.CopyTo(Codes, 0);
      for (int i = 0; i < Codes.Length; i++)
        this[Codes[i]] = Math.Round(this[Codes[i]], decimals, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Выполняет округление всех элементов коллекции до целых значений.
    /// Используются правила математического округления.
    /// </summary>
    public void Round()
    {
      Round(0);
    }

    #endregion
  }

  /// <summary>
  /// Словарь числовых значений со строковым ключом.
  /// Словарь может быть не чувствителен к регистру ключа (определяется в конструкторе).
  /// </summary>
  [Serializable]
  public class DecimalNamedDictionary : TypedStringDictionary<decimal>
  {
    #region Конструкторы

    /// <summary>
    /// Основная версия конструктора.
    /// Создает словарь с ключом, чувствительным к регистру
    /// </summary>
    public DecimalNamedDictionary()
      : this(false)
    {
    }

    /// <summary>
    /// Создает словарь с указанием чувствительности к регистру
    /// </summary>
    /// <param name="ignoreCase">Нужно ли игнорировать регистр ключа</param>
    public DecimalNamedDictionary(bool ignoreCase)
      : base(ignoreCase)
    {
    }

    /// <summary>
    /// Создает словарь с ключом, чувствительным к регистру
    /// Эту версию следует использовать, если заранее известно, сколько будет элементов в словаре.
    /// </summary>
    /// <param name="capacity">Начальная емкость коллекции</param>
    public DecimalNamedDictionary(int capacity)
      : this(capacity, false)
    {
    }

    /// <summary>
    /// Создает словарь с указанием чувствительности к регистру.
    /// Эту версию следует использовать, если заранее известно, сколько будет элементов в словаре.
    /// </summary>
    /// <param name="capacity">Начальная емкость коллекции</param>
    /// <param name="ignoreCase">Нужно ли игнорировать регистр ключа</param>
    public DecimalNamedDictionary(int capacity, bool ignoreCase)
      : base(capacity, ignoreCase)
    {
    }

    /// <summary>
    /// Создает словарь с ключом, чувствительным к регистру и заполняет его значениями.
    /// </summary>
    /// <param name="dictionary">Источник, откуда берутся значения</param>
    public DecimalNamedDictionary(IDictionary<string, decimal> dictionary)
      : this(dictionary, false)
    {
    }

    /// <summary>
    /// Создает словарь с указанием чувствительности к регистру, и заполняет его значениями.
    /// </summary>
    /// <param name="dictionary">Источник, откуда берутся значения</param>
    /// <param name="ignoreCase">Нужно ли игнорировать регистр ключа</param>
    public DecimalNamedDictionary(IDictionary<string, decimal> dictionary, bool ignoreCase)
      : base(dictionary, ignoreCase)
    {
    }

    #endregion

    #region Доступ к значениям

    /// <summary>
    /// Получение или установка значения.
    /// Если в словаре нет значения с указанным ключом, возвращается 0
    /// (без выброса исключения, как это принято в стандартных коллекциях)
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <returns>Значение</returns>
    public new decimal this[string key]
    {
      get
      {
        decimal v;
        if (base.TryGetValue(key, out v))
          return v;
        else
          return 0m;
      }
      set
      {
        base[key] = value;
      }
    }

    #endregion

    #region Сложение и вычитание

    /// <summary>
    /// Добавить к текущей коллекции значения из другой коллекции.
    /// Текущая коллекция должна быть доступна для записи (IsReadOnly=false).
    /// </summary>
    /// <param name="source">Словарь "Код-Значения", откуда берутся добавляемые значения.
    /// Если null, то никаких действий не выполняется</param>
    public void Add(IDictionary<string, decimal> source)
    {
      if (source == null)
        return;

      foreach (KeyValuePair<string, decimal> Pair in source)
        this[Pair.Key] += Pair.Value;
    }

    /// <summary>
    /// Вычесть из текущей коллекции значения из другой коллекции.
    /// Текущая коллекция должна быть доступна для записи (IsReadOnly=false).
    /// </summary>
    /// <param name="source">Словарь "Код-Значения", откуда берутся вычитаемые значения.
    /// Если null, то никаких действий не выполняется</param>
    public void Substract(IDictionary<string, decimal> source)
    {
      if (source == null)
        return;

      foreach (KeyValuePair<string, decimal> Pair in source)
        this[Pair.Key] -= Pair.Value;
    }

    /// <summary>
    /// Сложение двух коллекций.
    /// При выполнении оператора всегда создается новая коллекция, поэтому обычно экономичнее использовать
    /// нестатический метод Add(), который мофицирует существующую коллекцию.
    /// Свойство IgnoreCase новой коллекции устанавливается в соответствии со значением свойства первой коллекции.
    /// </summary>
    /// <param name="a">Первая исходная коллекция</param>
    /// <param name="b">Вторая исходная коллекция</param>
    /// <returns>Новая коллекция</returns>
    public static DecimalNamedDictionary operator +(DecimalNamedDictionary a, IDictionary<string, decimal> b)
    {
      DecimalNamedDictionary Res = new DecimalNamedDictionary(a, a.IgnoreCase);
      Res.Add(b);
      return Res;
    }

    /// <summary>
    /// Вычитание одной коллекции из другой.
    /// При выполнении оператора всегда создается новая коллекция, поэтому обычно экономичнее использовать
    /// нестатический метод Substract(), который мофицирует существующую коллекцию.
    /// Свойство IgnoreCase новой коллекции устанавливается в соответствии со значением свойства первой коллекции.
    /// </summary>
    /// <param name="a">Первая исходная коллекция</param>
    /// <param name="b">Вторая исходная коллекция</param>
    /// <returns>Новая коллекция</returns>
    public static DecimalNamedDictionary operator -(DecimalNamedDictionary a, IDictionary<string, decimal> b)
    {
      DecimalNamedDictionary Res = new DecimalNamedDictionary(a, a.IgnoreCase);
      Res.Substract(b);
      return Res;
    }

    #endregion

    #region Умножение и деление

    /// <summary>
    /// Умножение всех значений коллекции на заданное число.
    /// Текущая коллекция должна быть доступна для записи (IsReadOnly=false).
    /// </summary>
    /// <param name="m">Множитель</param>
    public void Multiply(decimal m)
    {
      string[] Codes = new string[base.Count];
      base.Keys.CopyTo(Codes, 0);
      for (int i = 0; i < Codes.Length; i++)
        this[Codes[i]] *= m;
    }

    /// <summary>
    /// Деление всех значений коллекции на заданное число.
    /// Деление выполняется без округления.
    /// Используйте метод Round() после выполнения деления
    /// Текущая коллекция должна быть доступна для записи (IsReadOnly=false).
    /// </summary>
    /// <param name="d">Делитель. Не может быть равен 0</param>
    public void Divide(decimal d)
    {
      string[] Codes = new string[base.Count];
      base.Keys.CopyTo(Codes, 0);
      for (int i = 0; i < Codes.Length; i++)
        this[Codes[i]] /= d;
    }

    #endregion

    #region Округление

    /// <summary>
    /// Выполняет округление всех элементов коллекции до заданного числа знаков после запятой.
    /// Используются правила математического округления.
    /// </summary>
    /// <param name="decimals">Число знаков после запятой</param>
    public void Round(int decimals)
    {
      string[] Codes = new string[base.Count];
      base.Keys.CopyTo(Codes, 0);
      for (int i = 0; i < Codes.Length; i++)
        this[Codes[i]] = Math.Round(this[Codes[i]], decimals, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Выполняет округление всех элементов коллекции до целых значений.
    /// Используются правила математического округления.
    /// </summary>
    public void Round()
    {
      Round(0);
    }

    #endregion
  }

  #endregion
}
