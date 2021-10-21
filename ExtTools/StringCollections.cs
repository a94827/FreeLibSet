using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Runtime.Serialization;
using FreeLibSet.Core;

/*
 * The BSD License
 * 
 * Copyright (c) 2012-2015, Ageyev A.V.
 * 
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, 
 * this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice, 
 * this list of conditions and the following disclaimer in the documentation 
 * and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE 
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

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
  public class SingleScopeStringList : IList<string>, IReadOnlyObject
  {
    #region Конструкторы

    /// <summary>
    /// Создает пустой список
    /// </summary>
    /// <param name="ignoreCase">Нужно ли игнорировать регистр строк</param>
    public SingleScopeStringList(bool ignoreCase)
    {
      _List = new List<string>();
      _Dict = new Dictionary<string, string>();
      _IgnoreCase = ignoreCase;
    }

    /// <summary>
    /// Создает пустой список заданной емкости.
    /// Используйте этот конструктор, если конечное число элементов в коллекции известно с большой долей вероятности.
    /// </summary>
    /// <param name="capacity">Начальная емкость коллекции</param>
    /// <param name="ignoreCase">Нужно ли игнорировать регистр строк</param>
    public SingleScopeStringList(int capacity, bool ignoreCase)
    {
      _List = new List<string>(capacity);
      _Dict = new Dictionary<string, string>(capacity);
      _IgnoreCase = ignoreCase;
    }

    /// <summary>
    /// Создает список и заполняет его заданными значениями.
    /// </summary>
    /// <param name="src">Коллекция, откуда следует взять строки</param>
    /// <param name="ignoreCase">Нужно ли игнорировать регистр строк</param>
    public SingleScopeStringList(ICollection<string> src, bool ignoreCase)
      : this(src.Count, ignoreCase)
    {
      foreach (string Item in src)
        Add(Item);
    }

    /// <summary>
    /// Создает список и заполняет его заданными значениями.
    /// </summary>
    /// <param name="src">Коллекция, откуда следует взять строки</param>
    /// <param name="ignoreCase">Нужно ли игнорировать регистр строк</param>
    public SingleScopeStringList(IEnumerable<string> src, bool ignoreCase)
      : this(ignoreCase)
    {
      foreach (string Item in src)
        Add(Item);
    }

    #endregion

    #region Доступ к элементам

    /// <summary>
    /// Основной список.
    /// Строки хранятся в том регистре, в котором были переданы
    /// </summary>
    private List<string> _List;

    /// <summary>
    /// Ключ - строка. Если IgnoreCase=true, то строка переводится в верхний регистр.
    /// Значением является та же строка, но с учетом регистра
    /// </summary>
    [NonSerialized]
    private Dictionary<string, string> _Dict;

    /// <summary>
    /// Доступ по индексу
    /// </summary>
    /// <param name="index">Индекс элемента в списке</param>
    /// <returns>Значение</returns>
    public string this[int index]
    {
      get { return _List[index]; }
      set
      {
        CheckNotReadOnly();

        string value2 = PrepareValue(value);

        if (_Dict.ContainsKey(value2))
          throw new InvalidOperationException("Значение " + value.ToString() + " уже есть в списке");

        string OldItem = _List[index];
        _Dict.Remove(PrepareValue(OldItem));
        try
        {
          _Dict.Add(value2, value);
        }
        catch
        {
          _Dict.Add(PrepareValue(OldItem), OldItem);
          throw;
        }
        _List[index] = value;
      }
    }

    /// <summary>
    /// Возвращает количество строк в списке.
    /// </summary>
    public int Count { get { return _List.Count; } }

    /// <summary>
    /// Текстовое представление "Count=XXX"
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      string s = "Count=" + Count.ToString();
      if (IsReadOnly)
        s += " (ReadOnly)";
      return s;
    }

    #endregion

    #region IgnoreCase

    /// <summary>
    /// Если true, то регистр строк не учитывается.
    /// Свойство задается в конструкторе
    /// </summary>
    public bool IgnoreCase { get { return _IgnoreCase; } }
    private bool _IgnoreCase;

    private string PrepareValue(string value)
    {
      if (value == null)
        //return null;
        throw new ArgumentNullException("value"); // 27.12.2020

      if (_IgnoreCase)
        return value.ToUpperInvariant();
      else
        return value;
    }

    #endregion

    #region Доступ Только для чтения

    /// <summary>
    /// Возвращает true, есои список был переведен в режим "только чтение"
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// Защищенный метод для перевода списка в режим "только чтение"
    /// </summary>
    protected void SetReadOnly()
    {
      _IsReadOnly = true;
    }

    /// <summary>
    /// Генерирует исключение, если IsReadOnly=true
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (_IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    #endregion

    #region IEnumerable<string> Members

    /// <summary>
    /// Возвращает перечислитель по строкам списка.
    /// 
    /// Тип возвращаемого значения может измениться в будущем, 
    /// гарантируется только реализация интерфейса перечислителя.
    /// Поэтому в прикладном коде метод должен использоваться исключительно для использования в операторе foreach.
    /// </summary>
    /// <returns>Перечислитель</returns>
    public List<string>.Enumerator GetEnumerator()
    {
      return _List.GetEnumerator();
    }

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
    {
      return _List.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return _List.GetEnumerator();
    }

    #endregion

    #region IList<string> Members

    /// <summary>
    /// Возвращает индекс заданной строки в списке.
    /// При поиске строки учитывается свойство IgnoreCase.
    /// Поиск является медленным. Если требуется только определить факт надичия строки, используйте Contains().
    /// </summary>
    /// <param name="item">Искомая строка</param>
    /// <returns>Индекс найденной строки или (-1), если строка не найдена</returns>
    public int IndexOf(string item)
    {
      StringComparison Flags = IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

      for (int i = 0; i < _List.Count; i++)
      {
        if (String.Equals(item, _List[i], Flags))
          return i;
      }
      return -1;
    }

    /// <summary>
    /// Добавляет строку в заданную позицию.
    /// Если такая строка (с учетом IgnoreCase) уже есть в списке, никаких действий не выполняется.
    /// </summary>
    /// <param name="index">Индекс в списке, куда должна быть добавлена строка</param>
    /// <param name="item">Добавляемая строка</param>
    public void Insert(int index, string item)
    {
      CheckNotReadOnly();

      string value2 = PrepareValue(item);

      if (_Dict.ContainsKey(value2))
        return;

      _Dict.Add(value2, item);
      try
      {
        _List.Insert(index, item);
      }
      catch
      {
        _Dict.Remove(value2);
        throw;
      }
    }

    /// <summary>
    /// Удалить строку из списка в заданной позиции.
    /// </summary>
    /// <param name="index">Индекс строки для удаления</param>
    public void RemoveAt(int index)
    {
      CheckNotReadOnly();

      string item = _List[index];
      _List.RemoveAt(index);
      _Dict.Remove(PrepareValue(item));
    }

    #endregion

    #region ICollection<T> Members

    /// <summary>
    /// Добавляет строку в список.
    /// Если такая строка уже есть в списке (с учетом IgnoreCase), никаких действий не выполняется.
    /// </summary>
    /// <param name="item">Добавляемая строка</param>
    public void Add(string item)
    {
      CheckNotReadOnly();

      string value2 = PrepareValue(item);

      if (_Dict.ContainsKey(value2))
        return;

      _Dict.Add(value2, item);
      try
      {
        _List.Add(item);
      }
      catch
      {
        _Dict.Remove(value2);
      }
    }

    /// <summary>
    /// Очищает список
    /// </summary>
    public void Clear()
    {
      CheckNotReadOnly();

      _List.Clear();
      _Dict.Clear();
    }

    /// <summary>
    /// Возвращает true, если в списке есть такая строка (с учетом IgnoreCase).
    /// В отличие от IndexOf(), этот метод выполняется быстро.
    /// </summary>
    /// <param name="item">Искомая строка</param>
    /// <returns>Наличие строки в списке</returns>
    public bool Contains(string item)
    {
      return _Dict.ContainsKey(PrepareValue(item));
    }

    /// <summary>
    /// Копирует весь список в массив
    /// </summary>
    /// <param name="array">Заполняемый массив</param>
    public void CopyTo(string[] array)
    {
      _List.CopyTo(array);
    }

    /// <summary>
    /// Копирует весь список в массив
    /// </summary>
    /// <param name="array">Заполняемый массив</param>
    /// <param name="arrayIndex">Индекс в массиве, начиная с которого он заполняется</param>
    public void CopyTo(string[] array, int arrayIndex)
    {
      _List.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Копирует часть списка в массив.
    /// </summary>
    /// <param name="index">Индекс первого элемента в текущем списке, с которого начинать копирование</param>
    /// <param name="array">Заполняемый массив</param>
    /// <param name="arrayIndex">Начальный индекс в массиве для заполнения</param>
    /// <param name="count">Количество элементов, которые нужно скопировать</param>
    public void CopyTo(int index, string[] array, int arrayIndex, int count)
    {
      _List.CopyTo(index, array, arrayIndex, count);
    }

    /// <summary>
    /// Удаляет заданную строку из списка (с учетом IgnoreCase).
    /// Если строки нет в списке, никаких действий не выполняется.
    /// </summary>
    /// <param name="item">Удаляемая строка</param>
    /// <returns>true, если строка была найдена и удалена. 
    /// false, если строка не найдена в списке.</returns>
    public bool Remove(string item)
    {
      CheckNotReadOnly();

      int p = IndexOf(item);
      if (p >= 0)
      {
        _List.RemoveAt(p);
        string value2 = PrepareValue(item);
        _Dict.Remove(value2);
        return true;
      }
      else
        return false;
    }

    #endregion

    #region Дополнительные методы

    /// <summary>
    /// Создает массив строк из списка
    /// </summary>
    /// <returns>Новый массив</returns>
    public string[] ToArray()
    {
      return _List.ToArray();
    }

    /// <summary>
    /// Групповое добавление элементов списка
    /// В исходной коллекции могут быть одинаковые элементы, которые пропускаются
    /// </summary>
    /// <param name="collection"></param>
    public void AddRange(IEnumerable<string> collection)
    {
      CheckNotReadOnly();
#if DEBUG
      if (collection == null)
        throw new ArgumentException("collection");
#endif
      if (Object.ReferenceEquals(collection, this))
        throw new ArgumentException("Нельзя добавить элементы из самого себя", "collection");

      foreach (string Item in collection)
        Add(Item);
    }

    /// <summary>
    /// Сортировка списка строк.
    /// При сортировке регистр символов учитывается или игнорируется, в зависимости от свойства IgnoreCase
    /// </summary>
    public void Sort()
    {
      CheckNotReadOnly();

      if (_IgnoreCase)
        _List.Sort(StringComparer.OrdinalIgnoreCase);
      else
        _List.Sort(StringComparer.Ordinal);
    }

    /// <summary>
    /// Заменяет порядок элементов на обратный
    /// </summary>
    public void Reverse()
    {
      CheckNotReadOnly();

      _List.Reverse();
    }

    #endregion

    #region Сериализация

    [OnDeserialized]
    private void OnDeserializedMethod(StreamingContext context)
    {
      _Dict = new Dictionary<string, string>(_List.Count);
      for (int i = 0; i < _List.Count; i++)
      {
        string value2 = PrepareValue(_List[i]);
        _Dict.Add(value2, _List[i]);
      }
    }

    #endregion
  }

  /// <summary>
  /// Реализация типизированной коллекции, в которой ключом являются строки, а значения имеют заданный тип.
  /// В отличие от обычной коллекции Dictionary, может быть не чувствительна к регистру ключа
  /// </summary>
  /// <typeparam name="TValue">Тип хранящихся значений</typeparam>
  [Serializable]
  public class TypedStringDictionary<TValue> : IDictionary<string, TValue>, IReadOnlyObject, INamedValuesAccess
  {
    #region Конструкторы

    /// <summary>
    /// Создает пустую коллекцию.
    /// </summary>
    /// <param name="ignoreCase">Нужно ли игнорировать регистр строк</param>
    public TypedStringDictionary(bool ignoreCase)
    {
      _IgnoreCase = ignoreCase;
      _MainDict = new Dictionary<string, TValue>();
    }

    /// <summary>
    /// Создает пустую коллекцию заданной емкости
    /// </summary>
    /// <param name="capacity">Начальная емкость коллекции</param>
    /// <param name="ignoreCase">Нужно ли игнорировать регистр строк</param>
    public TypedStringDictionary(int capacity, bool ignoreCase)
    {
      _IgnoreCase = ignoreCase;
      _MainDict = new Dictionary<string, TValue>(capacity);
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

    #endregion

    #region Доступ к элементам

    // Используется две коллекции Dictionary

    /// <summary>
    /// Основная коллекция.
    /// Содержит ключи в исходном регистре, независимо от IgnoreCase
    /// </summary>
    private Dictionary<string, TValue> _MainDict;

    /// <summary>
    /// Дополнительная коллекция.
    /// Существует, когда IgnoreCase=true. 
    /// Создается при необходимости. В частности, коллекция не сериализуется
    /// Ключ: - ключи, приведенные к верхнему регистру
    /// Значение: ключи в исходном регистре.
    /// </summary>
    [NonSerialized]
    private Dictionary<string, string> _AuxDict;

    /// <summary>
    /// При IgnoreCase переводит ключ в тот регистр, который был задан при первом обращении
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private string PrepareKey(string key)
    {
      if (!IgnoreCase)
        return key;

      // Используем метод двойной проверки на случай асинхронного доступа
      if (_AuxDict == null)
        PrepareAuxDict();

      string Key2 = key.ToUpperInvariant();
      string Key3;
      if (_AuxDict.TryGetValue(Key2, out Key3))
        return Key3;
      else
        return key;
    }

    private void PrepareAuxDict()
    {
      // Конструктор Dictionary по умолчанию устанавливает capacity=0.
      // Если в коллекции уже есть элементы, то, вероятно, этот метод вызван после десериализации.
      // Используем MainDict.Count в качестве начальной емкости
      Dictionary<string, string> AuxDict2 = new Dictionary<string, string>(_MainDict.Count);
      foreach (KeyValuePair<string, TValue> Pair in _MainDict)
      {
        AuxDict2.Add(Pair.Key.ToUpperInvariant(), Pair.Key);
      }

      // Если все хорошо, устанавливаем основное поле
      _AuxDict = AuxDict2;
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

    #region IDictionary<string,TValue> Members

    /// <summary>
    /// Добавляет пару "Ключ-Значение" в коллекцию.
    /// В режиме IgnoreCase=true возникнет исключение, если в коллекции уже есть похожий ключ, отличающийся только значением
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <param name="value">Значение</param>
    public void Add(string key, TValue value)
    {
      CheckNotReadOnly();
      key = PrepareKey(key);
      _MainDict.Add(key, value);
      if (_AuxDict != null)
        _AuxDict.Add(key.ToUpperInvariant(), key);
    }

    /// <summary>
    /// Возвращает true, если коллекция содержит указанный ключ
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool ContainsKey(string key)
    {
      return _MainDict.ContainsKey(PrepareKey(key));
    }

    /// <summary>
    /// Доступ к ключам коллекции.
    /// Возвращаемая коллекция предназначена только для просмотра.
    /// </summary>
    public ICollection<string> Keys { get { return _MainDict.Keys; } }

    /// <summary>
    /// Удалить ключ из коллекции
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <returns>true, если элемент был удален из коллекции</returns>
    public bool Remove(string key)
    {
      CheckNotReadOnly();

      key = PrepareKey(key);
      if (_MainDict.Remove(key))
      {
        if (_AuxDict != null)
          _AuxDict.Remove(key.ToUpperInvariant());
        return true;
      }
      else
        return false;
    }

    /// <summary>
    /// Попытка извлечь элемент с заданным ключом из коллекции
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <param name="value">Сюда записывается значение</param>
    /// <returns>true, если элемент с ключом есть в коллекции</returns>
    public bool TryGetValue(string key, out TValue value)
    {
      key = PrepareKey(key);
      return _MainDict.TryGetValue(key, out value);
    }

    /// <summary>
    /// Коллекция значений.
    /// Возвращаемая коллекция предназначена толькот для просмотра.
    /// </summary>
    public ICollection<TValue> Values { get { return _MainDict.Values; } }

    /// <summary>
    /// Извлечение или запись значения с ключом.
    /// Если при чтении запрошен несуществующий ключ, гененируется исключение.
    /// При установке свойства, либо добавляется новая запись, либо заменяется существующая с таким ключом.
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <returns>Значение</returns>
    public TValue this[string key]
    {
      get
      {
        key = PrepareKey(key);
        return _MainDict[key];
      }
      set
      {
        key = PrepareKey(key);
        _MainDict[key] = value;
        // Коллекция RegDict не меняется
      }
    }

    #endregion

    #region ICollection<KeyValuePair<string,TValue>> Members

    void ICollection<KeyValuePair<string, TValue>>.Add(KeyValuePair<string, TValue> item)
    {
      Add(item.Key, item.Value);
    }

    /// <summary>
    /// Очистить коллекцию
    /// </summary>
    public void Clear()
    {
      CheckNotReadOnly();

      _MainDict.Clear();
      _AuxDict = null; // пересоздадим, если понадобиться
    }

    bool ICollection<KeyValuePair<string, TValue>>.Contains(KeyValuePair<string, TValue> item)
    {
      if (_IgnoreCase)
        item = new KeyValuePair<string, TValue>(PrepareKey(item.Key), item.Value);
      return ((ICollection<KeyValuePair<string, TValue>>)_MainDict).Contains(item);
    }

    void ICollection<KeyValuePair<string, TValue>>.CopyTo(KeyValuePair<string, TValue>[] array, int arrayIndex)
    {
      ((ICollection<KeyValuePair<string, TValue>>)_MainDict).CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Возвращает количество элементов в коллекции
    /// </summary>
    public int Count { get { return _MainDict.Count; } }

    bool ICollection<KeyValuePair<string, TValue>>.Remove(KeyValuePair<string, TValue> item)
    {
      return Remove(item.Key);
    }

    #endregion

    #region IEnumerable<KeyValuePair<string,TValue>> Members

    /// <summary>
    /// Возвращает перечислитель коллекции.
    /// Элементами перечисления являются структуры KeyValuePair.
    /// Ключ в паре имеет тот регистр, который использовался при добавлении элемента в коллекцию.
    /// 
    /// Тип возвращаемого значения может измениться в будущем, 
    /// гарантируется только реализация интерфейса перечислителя.
    /// Поэтому в прикладном коде метод должен использоваться исключительно для использования в операторе foreach.
    /// </summary>
    /// <returns>Перечислитель</returns>
    public Dictionary<string, TValue>.Enumerator GetEnumerator()
    {
      return _MainDict.GetEnumerator();
    }

    IEnumerator<KeyValuePair<string, TValue>> IEnumerable<KeyValuePair<string, TValue>>.GetEnumerator()
    {
      return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion

    #region Доступ Только для чтения

    /// <summary>
    /// Возвращает true, если коллекция была переведена в режим "только чтения"
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// Защищенный метод для перевода коллекции в режим "только чтение"
    /// </summary>
    protected void SetReadOnly()
    {
      _IsReadOnly = true;
    }

    /// <summary>
    /// Генерирует исключение, если IsReadOnly=true
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (_IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    #endregion

    #region Десериализация

    // Достаточно стандартной десериализации

    #endregion

    #region Прочее

    /// <summary>
    /// Возвращает строку вида "Count=XXX"
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return "Count=" + Count.ToString();
    }

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
  public class BidirectionalTypedStringDictionary<TValue> : IDictionary<string, TValue>, IReadOnlyObject, INamedValuesAccess
  {
    #region Конструкторы

    /// <summary>
    /// Создает пустую коллекцию.
    /// </summary>
    /// <param name="ignoreCase">Нужно ли игнорировать регистр строк</param>
    public BidirectionalTypedStringDictionary(bool ignoreCase)
    {
      _IgnoreCase = ignoreCase;
      _MainDict = new Dictionary<string, TValue>();
    }

    /// <summary>
    /// Создает пустую коллекцию заданной емкости
    /// </summary>
    /// <param name="capacity">Начальная емкость коллекции</param>
    /// <param name="ignoreCase">Нужно ли игнорировать регистр строк</param>
    public BidirectionalTypedStringDictionary(int capacity, bool ignoreCase)
    {
      _IgnoreCase = ignoreCase;
      _MainDict = new Dictionary<string, TValue>(capacity);
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

    #endregion

    #region Доступ к элементам

    // Используется две коллекции Dictionary

    /// <summary>
    /// Основная коллекция.
    /// Содержит ключи в исходном регистре, независимо от IgnoreCase
    /// </summary>
    private Dictionary<string, TValue> _MainDict;

    /// <summary>
    /// Дополнительная коллекция.
    /// Существует, когда IgnoreCase=true. 
    /// Создается при необходимости. В частности, коллекция не сериализуется
    /// Ключ: - ключи, приведенные к верхнему регистру
    /// Значение: ключи в исходном регистре.
    /// </summary>
    [NonSerialized]
    private Dictionary<string, string> _AuxDict;

    /// <summary>
    /// При IgnoreCase переводит ключ в тот регистр, который был задан при первом обращении
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private string PrepareKey(string key)
    {
      if (!IgnoreCase)
        return key;

      // Используем метод двойной проверки на случай асинхронного доступа
      if (_AuxDict == null)
        PrepareAuxDict();

      string Key2 = key.ToUpperInvariant();
      string Key3;
      if (_AuxDict.TryGetValue(Key2, out Key3))
        return Key3;
      else
        return key;
    }

    private void PrepareAuxDict()
    {
      // Конструктор Dictionary по умолчанию устанавливает capacity=0.
      // Если в коллекции уже есть элементы, то, вероятно, этот метод вызван после десериализации.
      // Используем MainDict.Count в качестве начальной емкости
      Dictionary<string, string> AuxDict2 = new Dictionary<string, string>(_MainDict.Count);
      foreach (KeyValuePair<string, TValue> Pair in _MainDict)
      {
        AuxDict2.Add(Pair.Key.ToUpperInvariant(), Pair.Key);
      }

      // Если все хорошо, устанавливаем основное поле
      _AuxDict = AuxDict2;
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

    #region IDictionary<string,TValue> Members

    /// <summary>
    /// Добавляет пару "Ключ-Значение" в коллекцию.
    /// В режиме IgnoreCase=true возникнет исключение, если в коллекции уже есть похожий ключ, отличающийся только значением
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <param name="value">Значение</param>
    public void Add(string key, TValue value)
    {
      CheckNotReadOnly();
      key = PrepareKey(key);
      _MainDict.Add(key, value);
      try
      {
        if (_AuxDict != null)
          _AuxDict.Add(key.ToUpperInvariant(), key);
        if (_ReversedDict != null)
          _ReversedDict.Add(value, key);
      }
      catch
      {
        _MainDict.Remove(key);
        _AuxDict = null;
        _ReversedDict = null;
        throw;
      }
    }

    /// <summary>
    /// Возвращает true, если коллекция содержит указанный ключ
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool ContainsKey(string key)
    {
      return _MainDict.ContainsKey(PrepareKey(key));
    }

    /// <summary>
    /// Доступ к ключам коллекции.
    /// Возвращаемая коллекция предназначена только для просмотра.
    /// </summary>
    public ICollection<string> Keys { get { return _MainDict.Keys; } }

    /// <summary>
    /// Удалить ключ из коллекции
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <returns>true, если элемент был удален из коллекции</returns>
    public bool Remove(string key)
    {
      CheckNotReadOnly();

      key = PrepareKey(key);

      TValue Value;
      if (_MainDict.TryGetValue(key, out Value))
      {
        _MainDict.Remove(key);
        if (_AuxDict != null)
          _AuxDict.Remove(key.ToUpperInvariant());
        if (_ReversedDict != null)
          _ReversedDict.Remove(Value);
        return true;
      }
      else
        return false;
    }

    /// <summary>
    /// Попытка извлечь элемент с заданным ключом из коллекции
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <param name="value">Сюда записывается значение</param>
    /// <returns>true, если элемент с ключом есть в коллекции</returns>
    public bool TryGetValue(string key, out TValue value)
    {
      key = PrepareKey(key);
      return _MainDict.TryGetValue(key, out value);
    }

    /// <summary>
    /// Коллекция значений.
    /// Возвращаемая коллекция предназначена толькот для просмотра.
    /// </summary>
    public ICollection<TValue> Values { get { return _MainDict.Values; } }

    /// <summary>
    /// Извлечение или запись значения с ключом.
    /// Если при чтении запрошен несуществующий ключ, гененируется исключение.
    /// При установке свойства, либо добавляется новая запись, либо заменяется существующая с таким ключом.
    /// </summary>
    /// <param name="key">Ключ</param>
    /// <returns>Значение</returns>
    public TValue this[string key]
    {
      get
      {
        key = PrepareKey(key);
        return _MainDict[key];
      }
      set
      {
        if (ContainsKey(key))
        {
          TValue OldValue = this[key];
          Remove(key);
          try
          {
            Add(key, value);
          }
          catch
          {
            Add(key, OldValue);
            throw;
          }
        }
        else
        {
          // Просто добавляем пару
          Add(key, value);
        }
      }
    }

    #endregion

    #region ICollection<KeyValuePair<string,TValue>> Members

    void ICollection<KeyValuePair<string, TValue>>.Add(KeyValuePair<string, TValue> item)
    {
      Add(item.Key, item.Value);
    }

    /// <summary>
    /// Очистить коллекцию
    /// </summary>
    public void Clear()
    {
      CheckNotReadOnly();

      _MainDict.Clear();
      _AuxDict = null; // пересоздадим, если понадобится
      _ReversedDict = null;
    }

    bool ICollection<KeyValuePair<string, TValue>>.Contains(KeyValuePair<string, TValue> item)
    {
      if (_IgnoreCase)
        item = new KeyValuePair<string, TValue>(PrepareKey(item.Key), item.Value);
      return ((ICollection<KeyValuePair<string, TValue>>)_MainDict).Contains(item);
    }

    void ICollection<KeyValuePair<string, TValue>>.CopyTo(KeyValuePair<string, TValue>[] array, int arrayIndex)
    {
      ((ICollection<KeyValuePair<string, TValue>>)_MainDict).CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Возвращает количество элементов в коллекции
    /// </summary>
    public int Count { get { return _MainDict.Count; } }

    bool ICollection<KeyValuePair<string, TValue>>.Remove(KeyValuePair<string, TValue> item)
    {
      return Remove(item.Key);
    }

    #endregion

    #region IEnumerable<KeyValuePair<string,TValue>> Members

    /// <summary>
    /// Возвращает перечислитель коллекции.
    /// Элементами перечисления являются структуры KeyValuePair.
    /// Ключ в паре имеет тот регистр, который использовался при добавлении элемента в коллекцию.
    /// 
    /// Тип возвращаемого значения может измениться в будущем, 
    /// гарантируется только реализация интерфейса перечислителя.
    /// Поэтому в прикладном коде метод должен использоваться исключительно для использования в операторе foreach.
    /// </summary>
    /// <returns>Перечислитель</returns>
    public Dictionary<string, TValue>.Enumerator GetEnumerator()
    {
      return _MainDict.GetEnumerator();
    }

    IEnumerator<KeyValuePair<string, TValue>> IEnumerable<KeyValuePair<string, TValue>>.GetEnumerator()
    {
      return _MainDict.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return _MainDict.GetEnumerator();
    }

    #endregion

    #region Обратная коллекция

    /// <summary>
    /// Обратная коллекция.
    /// Создается только по необходимости.
    /// Значением явдяется ключ исходной коллекции в оригинальном регистре
    /// </summary>
    [NonSerialized]
    private Dictionary<TValue, string> _ReversedDict;

    private void PrepareReversed()
    {
      if (_ReversedDict != null)
        return;

      Dictionary<TValue, string> r2 = new Dictionary<TValue, string>(_MainDict.Count);
      foreach (KeyValuePair<string, TValue> Pair in _MainDict)
        r2.Add(Pair.Value, Pair.Key);
      _ReversedDict = r2;
    }

    /// <summary>
    /// Возвращает true, если в коллекции содержится указанное значение
    /// </summary>
    /// <param name="value">значение для поиска в обратной коллекции</param>
    /// <returns>true, если значение существует</returns>
    public bool ContainsValue(TValue value)
    {
      PrepareReversed();
      return _ReversedDict.ContainsKey(value);
    }

    /// <summary>
    /// Попытка получить ключ по значению.
    /// Если значение <paramref name="value"/> существует, возвращает true и по ссылке <paramref name="key"/>
    /// записывается полученное значение.
    /// Если значения <paramref name="value"/> не существует, возвращается false, а а апо ссылке записывается
    /// пустое значение
    /// </summary>
    /// <param name="value">значение для поиска в обратной коллекции</param>
    /// <param name="key">ключ, соответствующий значению</param>
    /// <returns>true, если значение существует</returns>
    public bool TryGetKey(TValue value, out string key)
    {
      PrepareReversed();
      return _ReversedDict.TryGetValue(value, out key);
    }

    /// <summary>
    /// Удалить значение из коллекции
    /// </summary>
    /// <param name="value">значение для поиска и удаления</param>
    /// <returns>true, если значение было найдено в обратной коллекции</returns>
    public bool RemoveValue(TValue value)
    {
      CheckNotReadOnly();
      PrepareReversed();
      string key;
      if (_ReversedDict.TryGetValue(value, out key))
      {
        bool Res = Remove(key);
        if (!Res)
          throw new BugException("Ошибка синхронизации основной и обратной коллекции");
        return true;
      }
      else
        return false;
    }

    #endregion

    #region Доступ Только для чтения

    /// <summary>
    /// Возвращает true, если коллекция была переведена в режим "только чтения"
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// Защищенный метод для перевода коллекции в режим "только чтение"
    /// </summary>
    protected void SetReadOnly()
    {
      _IsReadOnly = true;
    }

    /// <summary>
    /// Генерирует исключение, если IsReadOnly=true
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (_IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    #endregion

    #region Десериализация

    // Достаточно стандартной десериализации

    #endregion

    #region Прочее

    /// <summary>
    /// Возвращает строку вида "Count=XXX"
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return "Count=" + Count.ToString();
    }

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
  public sealed class StringArrayIndexer : IComparer<string>
  {
    #region Конструкторы
    /// <summary>
    /// Создает индексатор для массива.
    /// Эта версия конструкторов учитывает регистр символов.
    /// </summary>
    /// <param name="source">Индексируемый массив</param>
    public StringArrayIndexer(string[] source)
      :this(source, false)
    { 
    }

    /// <summary>
    /// Создает индексатор для массива.
    /// </summary>
    /// <param name="source">Индексируемый массив</param>
    /// <param name="ignoreCase">Нужно ли игнорировать регистр строк</param>
    public StringArrayIndexer(string[] source, bool ignoreCase)
    {
      if (source == null)
        throw new ArgumentNullException("source");

      _Dict = new Dictionary<string, int>(source.Length);
      for (int i = 0; i < source.Length; i++)
      {
        if (ignoreCase)
          _Dict.Add(source[i].ToUpperInvariant(), i);
        else
          _Dict.Add(source[i], i);
      }

      _IgnoreCase = ignoreCase;
    }

    /// <summary>
    /// Создает индексатор для коллекции строк.
    /// Эта версия конструкторов учитывает регистр символов.
    /// </summary>
    /// <param name="source">Индексируемая коллекция</param>
    public StringArrayIndexer(ICollection<string> source)
      :this(source, false)
    { 
    }

    /// <summary>
    /// Создает индексатор для коллекции строк.
    /// </summary>
    /// <param name="source">Индексируемая коллекция</param>
    /// <param name="ignoreCase">Нужно ли игнорировать регистр строк</param>
    public StringArrayIndexer(ICollection<string> source, bool ignoreCase)
    {
      if (source == null)
        throw new ArgumentNullException("source");

      _Dict = new Dictionary<string, int>(source.Count);
      int cnt = 0;
      foreach (string Item in source)
      {
        if (ignoreCase)
          _Dict.Add(Item.ToUpperInvariant(), cnt);
        else
          _Dict.Add(Item, cnt);
        cnt++;
      }
    }

    private StringArrayIndexer()
      : this(DataTools.EmptyStrings, false)
    {
      _IsReadOnly = true;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Ключом являются строки, приведенные к верхнему региструк, если IgnoreCase=true
    /// </summary>
    private Dictionary<string, int> _Dict;

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
    /// Строковое представление "Count=XXX"
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return "Count=" + _Dict.Count.ToString();
    }

    /// <summary>
    /// Возвращает индекс элемента в массиве.
    /// В отличие от Array.IndexOf(), выполняется быстро
    /// </summary>
    /// <param name="item">Искомая строка</param>
    /// <returns>Позиция в списке</returns>
    public int IndexOf(string item)
    {
      if (_IgnoreCase)
        item = item.ToUpperInvariant();

      int p;
      if (_Dict.TryGetValue(item, out p))
        return p;
      else
        return -1;
    }

    /// <summary>
    /// Возвращает true, если в индексированной коллекции/массиве есть указанная строка (с учетом IgnoreCase)
    /// </summary>
    /// <param name="item">Искомая строка</param>
    /// <returns>Признак наличия строки</returns>
    public bool Contains(string item)
    {
      if (_IgnoreCase)
        item = item.ToUpperInvariant();

      return _Dict.ContainsKey(item);
    }


    /// <summary>
    /// Возвращает true, если в списке содержатся все элементы, то есть если Contains() возвращает true для каждого элемента.
    /// Если проверяемый список пустой, возвращает true.
    /// </summary>
    /// <param name="items">Проверяемый список элементов</param>
    /// <returns>Наличие элементов</returns>
    public bool ContainsAll(IEnumerable<string> items)
    {
      foreach (string item in items)
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
    public bool ContainsAny(IEnumerable<string> items)
    {
      foreach (string item in items)
      {
        if (Contains(item))
          return true;
      }
      return false;
    }

    #endregion

    #region IComparer<T> members

    /// <summary>
    /// Внутренний флаг, установленный для списка Empty
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
    /// Сравнение положения двух элементов.
    /// Метод может быть использован для сортировки произвольных списков и массивов, чтобы
    /// отсортировать их в соответствии с порядком элементов в текущем объекте StringArrayIndexer.
    /// Сравнивается положение элементов в текущем объекте, а не строки.
    /// Если какое-либо значение отсутствует в текущем объекте, то оно будет расположено в
    /// начале или в конце списка, в зависимости от свойства UnknownItemPosition.
    /// 
    /// Метод возвращает отрицательное значение, если <paramref name="x"/> располагается ближе
    /// к началу списка, чем <paramref name="y"/>. Положительное значение возвращается, если
    /// <paramref name="x"/> располагается ближе к концу списка, чем <paramref name="y"/>. 
    /// Если обоих значений нет в текущем списке, то возвращается результат сравнения строк.
    /// /// </summary>
    /// <param name="x">Первое сравниваемое значение</param>
    /// <param name="y">Второе сравниваемое значение</param>
    /// <returns>Результат сравнение позиций</returns>
    public int Compare(string x, string y)
    {
      int px = IndexOf(x);
      int py = IndexOf(y);

      if (px < 0 && py < 0)
      {
        // Если обоих элементов нет в списке, сравниваем элементы
        return String.Compare(x, y, IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
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
    public static readonly StringArrayIndexer Empty = new StringArrayIndexer();

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
    #region Конструкторы

    /// <summary>
    /// Создает индексатор для массива.
    /// В отличие от ArrayIndexer of Char, допускается наличие в <paramref name="source"/> повторяющихся символов, которые отбрасываются.
    /// Сравнение будет чувствительным к регистру символов.
    /// </summary>
    /// <param name="source">Массив символов</param>
    public CharArrayIndexer(char[] source)
      :this(source, false)
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
      :this(source, false)
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
