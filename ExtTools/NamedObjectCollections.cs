using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel;

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


// Типизированные коллекции, использующие интерфейс IObjectWithCode
// 25.07.2019.
// Коллекции требуют, чтобы тип T был классом, а не структурой.
// Технически ограничение не является обязательным и его можно было бы не вводить, 
// но использование структур приводит к постоянному boxing'у структуры для вызова метода интерфейса, что является неэффективным.

namespace AgeyevAV
{
  #region Интерфейс IObjectWithCode

  /// <summary>
  /// Объект, содержащий код
  /// </summary>
  public interface IObjectWithCode
  {
    /// <summary>
    /// Возвращает код объекта.
    /// Коллекции могут учитывать или игнорировать рнгистр символов кода
    /// </summary>
    string Code { get; }
  }

  #endregion

  /// <summary>
  /// Простая реализация для интерфейса IObjectWithCode
  /// </summary>
  [Serializable]
  public class ObjectWithCode : IObjectWithCode
  {
    #region Конструктор

    /// <summary>
    /// Создает новый объект с заданным кодом
    /// </summary>
    /// <param name="code">Код. Должен быть задан</param>
    public ObjectWithCode(string code)
    {
      if (String.IsNullOrEmpty(code))
        throw new ArgumentNullException("code");
      _Code = code;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Код, заданный в конструкторе
    /// </summary>
    protected string Code { get { return _Code; } }
    private readonly string _Code;

    /// <summary>
    /// Текстовое представление, возвращающее код.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return _Code;
    }

    #endregion

    #region Вспомогательные методы

    string IObjectWithCode.Code
    {
      get { return _Code; }
    }

    /// <summary>
    /// Хэш-значение для коллекции. Возвращает Code.GetHashCode()
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
      return _Code.GetHashCode();
    }

    #endregion
  }

  /// <summary>
  /// Объект для сортировки объектов, реализующих интерфейс IObjectWithCode.
  /// Используйте статические экземпляры объектов Ordinal или OrdinalIgnoreCase.
  /// </summary>
  public sealed class ObjectWithCodeComparer<T> : IComparer<T>
    where T:IObjectWithCode
  {
    #region Защищенный конструктор

    private ObjectWithCodeComparer(StringComparison comparisonType)
    {
      _ComparisonType = comparisonType;
    }

    #endregion

    #region Поля

    private readonly StringComparison _ComparisonType;

    #endregion

    #region IComparer<IObjectWithCode> Members

    /// <summary>
    /// Сравнение двух объектов.
    /// Использует String.Compare(x.Code, y.Code, ComparisonType)
    /// </summary>
    /// <param name="x">Первый сравниваемый объект</param>
    /// <param name="y">Второй сравниваемый объект</param>
    /// <returns>Результат сравнения кодов объектов</returns>
    public int Compare(T x, T y)
    {
      return String.Compare(x.Code, y.Code, _ComparisonType);
    }

    #endregion

    #region Статические экземпляры

    /// <summary>
    /// Сортировщик, учитывающий регистр символов
    /// </summary>
    public static readonly ObjectWithCodeComparer<T> Ordinal = new ObjectWithCodeComparer<T>(StringComparison.Ordinal);

    /// <summary>
    /// Сортировщик, игнорирующий регистр символов
    /// </summary>
    public static readonly ObjectWithCodeComparer<T> OrdinalIgnoreCase = new ObjectWithCodeComparer<T>(StringComparison.OrdinalIgnoreCase);

    #endregion
  }

#if OLD  
  /// <summary>
  /// Список объектов произвольного типа, доступ к которым может осуществляться как по
  /// индексу (как в обычном списке List), так и по коду, как в Dictionary с ключом String.
  /// Список не может содержать значения null.
  /// Коллекция может быть чувствительной или нечувствительной к регистру кода (задается в конструкторе).
  /// Этот класс является потокобезопасным после установки свойства ReadOnly.
  /// Пока список заполняется, объект не является потокобезопасным.
  /// </summary>
  /// <typeparam name="T">Тип объектов, хранящихся в списке, поддерживающих интерфейс IObjectWithCode</typeparam>
  /// <remarks>
  /// Если значение не реализует интерфейс IObjectWithCode, используйте OrderSortedList.
  /// Если не требуется доступ к объектам по индексу, используйте более "легкий" класс NamedCollection.
  /// </remarks>
  [Serializable]
  public class NamedList<T> : IEnumerable<T>, IList<T>, IReadOnlyObject, INamedValuesAccess
    where T : IObjectWithCode
  {
  #region Конструкторы

    /// <summary>
    /// Создает пустой список.
    /// Регистр кода учитывается
    /// </summary>
    public NamedList()
      : this(false)
    {
    }


    /// <summary>
    /// Создает пустой список
    /// </summary>
    /// <param name="IgnoreCase">Надо ли игнорировать регистр кода</param>
    public NamedList(bool IgnoreCase)
    {
      FList = new List<T>();
      FDict = new Dictionary<string, T>();
      FIgnoreCase = IgnoreCase;
    }

    /// <summary>
    /// Создает пустой список заданной емкости.
    /// Регистр кода учитывается.
    /// </summary>
    /// <param name="Capacity">Начальная емкость списка</param>
    public NamedList(int Capacity)
      : this(Capacity, false)
    {
    }

    /// <summary>
    /// Создает пустой список заданной емкости
    /// </summary>
    /// <param name="Capacity">Начальная емкость списка</param>
    /// <param name="IgnoreCase">Надо ли игнорировать регистр кода</param>
    public NamedList(int Capacity, bool IgnoreCase)
    {
      FList = new List<T>(Capacity);
      FDict = new Dictionary<string, T>(Capacity);
      FIgnoreCase = IgnoreCase;
    }


    /// <summary>
    /// Создает список, заполняя его значениями из коллекции.
    /// Регистр кода учитывается.
    /// </summary>
    /// <param name="Src">Словарь ключей и значений</param>
    public NamedList(IDictionary<string, T> Src)
    {
      FList = new List<T>(Src.Values);
      FDict = new Dictionary<string, T>(Src);
    }

    /// <summary>
    /// Создает список, заполняя его значениями из коллекции.
    /// Регистр кода учитывается.
    /// Если список <paramref name="Src"/> содержит объекты с повторяющимися кодами, повторы отбрасываются
    /// </summary>
    /// <param name="Src">Исходный список объектов</param>
    public NamedList(ICollection<T> Src)
      : this(Src, false)
    {
    }

    /// <summary>
    /// Создает список, заполняя его значениями из коллекции.
    /// Если список <paramref name="Src"/> содержит объекты с повторяющимися кодами (с учетом <paramref name="IgnoreCase"/>), повторы отбрасываются
    /// </summary>
    /// <param name="Src">Исходный список объектов</param>
    /// <param name="IgnoreCase">Надо ли игнорировать регистр кода</param>
    public NamedList(ICollection<T> Src, bool IgnoreCase)
      : this(Src.Count, IgnoreCase)
    {
      foreach (T Item in Src)
        Add(Item);
    }

    /// <summary>
    /// Создает список, заполняя его значениями из коллекции.
    /// Регистр кода учитывается.
    /// Если список <paramref name="Src"/> содержит объекты с повторяющимися кодами, повторы отбрасываются
    /// </summary>
    /// <param name="Src">Исходный список объектов</param>
    public NamedList(IEnumerable<T> Src)
      : this(Src, false)
    {
    }

    /// <summary>
    /// Создает список, заполняя его значениями из коллекции.
    /// Если список <paramref name="Src"/> содержит объекты с повторяющимися кодами (с учетом <paramref name="IgnoreCase"/>), повторы отбрасываются
    /// </summary>
    /// <param name="Src">Исходный список объектов</param>
    /// <param name="IgnoreCase">Надо ли игнорировать регистр кода</param>
    public NamedList(IEnumerable<T> Src, bool IgnoreCase)
      : this(IgnoreCase)
    {
      foreach (T Item in Src)
        Add(Item);
    }

  #endregion

  #region Доступ к элементам

    /// <summary>
    /// Линейный список, определяющий порядок элементов
    /// </summary>
    private List<T> FList;

    /// <summary>
    /// Коллекция по кодам.
    /// Если IgnoreCase=true, то ключ преобразован к верхнему регистру
    /// </summary>
    [NonSerialized]
    private Dictionary<string, T> FDict;

    /// <summary>
    /// Доступ по индексу
    /// </summary>
    /// <param name="Index">Индекс элемента в массиве. Должен быть в диапазоне от 0 до Count-1</param>
    /// <returns></returns>
    public T this[int Index]
    {
      get { return FList[Index]; }
      set
      {
        CheckNotReadOnly();

        T OldItem = FList[Index];
        FDict.Remove(OldItem.Code);
        try
        {
          string NewCode = value.Code;
          if (FIgnoreCase)
            NewCode = NewCode.ToUpperInvariant();
          FDict.Add(NewCode, value);
        }
        catch
        {
          string OldItemCode = OldItem.Code;
          if (FIgnoreCase)
            OldItemCode = OldItemCode.ToUpperInvariant();
          FDict.Add(OldItemCode, OldItem);
          throw;
        }
        FList[Index] = value;
      }
    }

    /// <summary>
    /// Доступ по коду.
    /// Если запрошен несуществуюший код, возвращается пустой элемент
    /// </summary>
    /// <param name="Code">Код объекта</param>
    /// <returns>Объект или пустое значение, если в списке нет объекта с таким кодом</returns>
    public T this[string Code]
    {
      get
      {
        T res;
        if (string.IsNullOrEmpty(Code))
          return default(T);

        if (IgnoreCase)
          Code = Code.ToUpperInvariant();

        if (FDict.TryGetValue(Code, out res))
          return res;
        else
          return default(T);
      }
    }

    /// <summary>
    /// Возвращает элемент с заданным кодом.
    /// В отличие от индексированного доступа по коду, если не найден объект с заданным кодом, генерируется исключение
    /// </summary>
    /// <param name="Code">Код объекта</param>
    /// <returns>Объект</returns>
    public T GetRequired(string Code)
    {
      T res;
      if (string.IsNullOrEmpty(Code))
        throw new ArgumentNullException("Code");

      if (IgnoreCase)
        Code = Code.ToUpperInvariant();

      if (FDict.TryGetValue(Code, out res))
        return res;
      else
        throw new ArgumentException("В списке нет элемента с кодом \"" + Code + "\"");
    }

    /// <summary>
    /// Возвращает количество элементов в списке
    /// </summary>
    public int Count { get { return FList.Count; } }

    /// <summary>
    /// Текстовое представление "Count=XXX"
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return "Count=" + Count.ToString();
    }

  #endregion

  #region IgnoreCase

    /// <summary>
    /// Если установлено в true, то при поиске элементов будет игнорироваться регистр.
    /// Если свойство установлено в false (по умолчанию), то регистр символов отличается
    /// Свойство устанавливается в конструкторе
    /// </summary>
    public bool IgnoreCase { get { return FIgnoreCase; } }
    private bool FIgnoreCase;

  #endregion

  #region Доступ Только для чтения

    /// <summary>
    /// Возвращает true, если список был переведен в режим "только чтение"
    /// </summary>
    public bool IsReadOnly { get { return FIsReadOnly; } }
    private bool FIsReadOnly;

    /// <summary>
    /// Защищенный метод для перевода списка в режим "только чтение"
    /// </summary>
    internal protected void SetReadOnly()
    {
      FIsReadOnly = true;
    }

    /// <summary>
    /// Генерирует исключение, если IsReadOnly=true
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (FIsReadOnly)
        throw new ObjectReadOnlyException();
    }

  #endregion

  #region IEnumerable<T> Members

    /// <summary>
    /// Возвращает перечислитель объектов в списке
    /// </summary>
    /// <returns>Перечислитель</returns>
    public IEnumerator<T> GetEnumerator()
    {
      return FList.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return FList.GetEnumerator();
    }

  #endregion

  #region IList<T> Members

    /// <summary>
    /// Возвращает индекс объекта в списке или (-1), если объект не найден.
    /// Метод является медленным.
    /// Если требуется только проверить наличие элемента с таким кодом, рекомендуется использовать
    /// метод Contains(), принимающий строковый код
    /// </summary>
    /// <param name="item">Объект для поиска</param>
    /// <returns>Индекс объекта.</returns>
    public int IndexOf(T item)
    {
      return FList.IndexOf(item);
    }

    /// <summary>
    /// Добавляет элемент в заданную позицию списка.
    /// Если в списке уже есть элемент с таким кодом (с учетом IgnoreCase), генерируется исключение
    /// </summary>
    /// <param name="index">Позиция для добавления</param>
    /// <param name="item">Добавляемый объект</param>
    public void Insert(int index, T item)
    {
      CheckNotReadOnly();

      string ItemCode = item.Code;
      if (IgnoreCase)
        ItemCode = ItemCode.ToUpperInvariant();

      FDict.Add(ItemCode, item);
      try
      {
        FList.Insert(index, item);
      }
      catch
      {
        FDict.Remove(ItemCode);
        throw;
      }
    }

    /// <summary>
    /// Удаляет элемент из указанной позиции списка.
    /// </summary>
    /// <param name="index">Индекс элемента</param>
    public void RemoveAt(int index)
    {
      CheckNotReadOnly();

      T item = FList[index];
      FList.RemoveAt(index);

      string ItemCode = item.Code;
      if (IgnoreCase)
        ItemCode = ItemCode.ToUpperInvariant();

      FDict.Remove(ItemCode);
    }

  #endregion

  #region ICollection<T> Members

    /// <summary>
    /// Добавляет элемент в конец списка.
    /// Если в списке уже есть элемент с таким кодом (с учетом IgnoreCase), генерируется исключение
    /// </summary>
    /// <param name="item">Добавляемый элемент</param>
    public void Add(T item)
    {
      CheckNotReadOnly();

      string ItemCode = item.Code;
      if (IgnoreCase)
        ItemCode = ItemCode.ToUpperInvariant();

      FDict.Add(ItemCode, item);
      try
      {
        FList.Add(item);
      }
      catch
      {
        FDict.Remove(ItemCode);
        throw;
      }
    }

    /// <summary>
    /// Очищает список
    /// </summary>
    public void Clear()
    {
      CheckNotReadOnly();

      FList.Clear();
      FDict.Clear();
    }

    /// <summary>
    /// Выполняет быстрый поиск в списке элемента с таким кодом.
    /// Рекомендуется использовать перегрузку, принимающую строковый код.
    /// </summary>
    /// <param name="item">Искомый элемент</param>
    /// <returns>true, если элемент найден</returns>
    public bool Contains(T item)
    {
      string ItemCode = item.Code;
      if (IgnoreCase)
        ItemCode = ItemCode.ToUpperInvariant();
      // return FDict.ContainsKey(ItemCode);

      // 21.02.2018
      // Надо еще и на равенство проверить
      T ResItem;
      if (FDict.TryGetValue(ItemCode, out ResItem))
        return item.Equals(ResItem);
      else
        return false;
    }

    /// <summary>
    /// Копирует список в массив
    /// </summary>
    /// <param name="array">Заполняемый массив</param>
    /// <param name="arrayIndex">Начальный индекс в массиве для заполнения</param>
    public void CopyTo(T[] array, int arrayIndex)
    {
      FList.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Удаляет заданный элемент из списка
    /// </summary>
    /// <param name="item">удаляемый элемент</param>
    /// <returns>true, если элемент был найден и удален</returns>
    public bool Remove(T item)
    {
      CheckNotReadOnly();

      if (FList.Remove(item))
      {
        string ItemCode = item.Code;
        if (IgnoreCase)
          ItemCode = ItemCode.ToUpperInvariant();

        FDict.Remove(ItemCode);
        return true;
      }
      else
        return false;
    }

  #endregion

  #region Дополнительные методы

    /// <summary>
    /// Медленный поиск по коду (с учетом IgnoreCase).
    /// Возвращает индекс найденного элемента или (-1)
    /// Если требуется только определить существование элемента с заданным кодом, используйте
    /// Contains(), принимающий строковый аргумент.
    /// </summary>
    /// <param name="Code">Искомый код</param>
    /// <returns>Индекс объекта</returns>
    public int IndexOf(string Code)
    {
      if (IgnoreCase)
      {
        for (int i = 0; i < FList.Count; i++)
        {
          if (String.Equals(FList[i].Code, Code, StringComparison.OrdinalIgnoreCase))
            return i;
        }
      }
      else
      {
        for (int i = 0; i < FList.Count; i++)
        {
          if (FList[i].Code == Code)
            return i;
        }
      }
      return -1;
    }

    /// <summary>
    /// Быстрый поиск кода (с учетом IgnoreCase).
    /// Возвращает true, если в списке есть элемент с заданным кодом
    /// </summary>
    /// <param name="Code">Искомый код</param>
    /// <returns>Наличие элемента в списке</returns>
    public bool Contains(string Code)
    {
      if (String.IsNullOrEmpty(Code))
        return false;

      if (IgnoreCase)
        Code = Code.ToUpperInvariant();

      return FDict.ContainsKey(Code);
    }

    /// <summary>
    /// Быстрый поиск кода (с учетом IgnoreCase).
    /// Возвращает true, если в списке есть элемент с заданным кодом.
    /// При этом сразу возвращается и элемент.
    /// Если в списке нет элемента с таким кодом, возвращается false, 
    /// а Value получает пустое значение
    /// </summary>
    /// <param name="Code">Искомый код</param>
    /// <param name="Value">Сюда помещается найденное значение</param>
    /// <returns>Наличие элемента в списке</returns>
    public bool TryGetValue(string Code, out T Value)
    {
      if (String.IsNullOrEmpty(Code))
      {
        Value = default(T);
        return false;
      }

      if (IgnoreCase)
        Code = Code.ToUpperInvariant();

      return FDict.TryGetValue(Code, out Value);
    }

    /// <summary>
    /// Копирует список в массив
    /// </summary>
    /// <returns>Новый массив</returns>
    public T[] ToArray()
    {
      return FList.ToArray();
    }

    /// <summary>
    /// Добавляет несколько элементов в список.
    /// Эквивалентно последовательному вызову метода Add()
    /// </summary>
    /// <param name="Collection">Список добавляемых элементов</param>
    public void AddRange(IEnumerable<T> Collection)
    {
      CheckNotReadOnly();
      foreach (T Item in Collection)
        Add(Item);
    }

    /// <summary>
    /// Удаляет элемент с заданным кодом (с учетом IgnoreCase)
    /// </summary>
    /// <param name="Code">Код удаляемого элемента</param>
    /// <returns>true, если объект был найден и удален</returns>
    public bool Remove(string Code)
    {
      CheckNotReadOnly();

      if (String.IsNullOrEmpty(Code))
        return false;
      if (IgnoreCase)
        Code = Code.ToUpperInvariant();

      int p = IndexOf(Code);
      if (p >= 0)
      {
        FList.RemoveAt(p);
        FDict.Remove(Code);
        return true;
      }
      else
        return false;
    }

    /// <summary>
    /// Возвращает строковый массив с кодами элементов.
    /// Порядок соответствует расположению элементов в списке.
    /// Регистр символов не меняется, даже если IgnoreCase=true.
    /// </summary>
    /// <returns></returns>
    public string[] GetCodes()
    {
      string[] a = new string[FList.Count];
      for (int i = 0; i < FList.Count; i++)
        a[i] = FList[i].Code;

      return a;
    }

  #endregion

  #region Десериализация

    [OnDeserialized]
    private void OnDeserializedMethod(StreamingContext context)
    {
      FDict = new Dictionary<string, T>(FList.Count);
      for (int i = 0; i < FList.Count; i++)
      {
        string ItemCode = FList[i].Code;
        if (IgnoreCase)
          ItemCode = ItemCode.ToUpperInvariant();

        FDict.Add(ItemCode, FList[i]);
      }
    }

  #endregion

  #region INamedValuesAccess Members

    object INamedValuesAccess.GetValue(string Name)
    {
      return this[Name];
    }

    string[] INamedValuesAccess.GetNames()
    {
      return GetCodes();
    }

  #endregion
  }
#else
  /// <summary>
  /// Список объектов произвольного типа, доступ к которым может осуществляться как по
  /// индексу (как в обычном списке List), так и по коду, как в Dictionary с ключом String.
  /// Список не может содержать значения null.
  /// Коллекция может быть чувствительной или нечувствительной к регистру кода (задается в конструкторе).
  /// Этот класс является потокобезопасным после установки свойства ReadOnly.
  /// Пока список заполняется, объект не является потокобезопасным.
  /// </summary>
  /// <typeparam name="T">Тип объектов, хранящихся в списке, поддерживающих интерфейс IObjectWithCode</typeparam>
  /// <remarks>
  /// Если значение не реализует интерфейс IObjectWithCode, используйте OrderSortedList.
  /// Если не требуется доступ к объектам по индексу, используйте более "легкий" класс NamedCollection.
  /// Элементы в списке хранятся в порядке добавления. При необходимости, используйте метод Sort().
  /// </remarks>
  [Serializable]
  public class NamedList<T> : IEnumerable<T>, IList<T>, IReadOnlyObject, INamedValuesAccess
    where T : class, IObjectWithCode
  {
    #region Конструкторы

    /// <summary>
    /// Создает пустой список.
    /// Регистр кода учитывается
    /// </summary>
    public NamedList()
      : this(false)
    {
    }


    /// <summary>
    /// Создает пустой список
    /// </summary>
    /// <param name="ignoreCase">Надо ли игнорировать регистр кода</param>
    public NamedList(bool ignoreCase)
    {
      _List = new List<T>();
      _Dict = new Dictionary<string, int>();
      _DictIsValid = true;
      _IgnoreCase = ignoreCase;
    }

    /// <summary>
    /// Создает пустой список заданной емкости.
    /// Регистр кода учитывается.
    /// </summary>
    /// <param name="capacity">Начальная емкость списка</param>
    public NamedList(int capacity)
      : this(capacity, false)
    {
    }

    /// <summary>
    /// Создает пустой список заданной емкости
    /// </summary>
    /// <param name="capacity">Начальная емкость списка</param>
    /// <param name="ignoreCase">Надо ли игнорировать регистр кода</param>
    public NamedList(int capacity, bool ignoreCase)
    {
      _List = new List<T>(capacity);
      _Dict = new Dictionary<string, int>(capacity);
      _DictIsValid = true;
      _IgnoreCase = ignoreCase;
    }


    /// <summary>
    /// Создает список, заполняя его значениями из коллекции.
    /// Регистр кода учитывается.
    /// </summary>
    /// <param name="srcDictionary">Словарь ключей и значений</param>
    public NamedList(IDictionary<string, T> srcDictionary)
      :this(srcDictionary.Count)
    {
      AddRange(srcDictionary.Values);
    }

    /// <summary>
    /// Создает список, заполняя его значениями из коллекции.
    /// Регистр кода учитывается.
    /// Если список <paramref name="srcCollection"/> содержит объекты с повторяющимися кодами, повторы отбрасываются
    /// </summary>
    /// <param name="srcCollection">Исходный список объектов</param>
    public NamedList(ICollection<T> srcCollection)
      : this(srcCollection, false)
    {
    }

    /// <summary>
    /// Создает список, заполняя его значениями из коллекции.
    /// Если список <paramref name="srcCollection"/> содержит объекты с повторяющимися кодами (с учетом <paramref name="ignoreCase"/>), повторы отбрасываются
    /// </summary>
    /// <param name="srcCollection">Исходный список объектов</param>
    /// <param name="ignoreCase">Надо ли игнорировать регистр кода</param>
    public NamedList(ICollection<T> srcCollection, bool ignoreCase)
      : this(srcCollection.Count, ignoreCase)
    {
      foreach (T Item in srcCollection)
        Add(Item);
    }

    /// <summary>
    /// Создает список, заполняя его значениями из коллекции.
    /// Регистр кода учитывается.
    /// Если список <paramref name="srcCollection"/> содержит объекты с повторяющимися кодами, повторы отбрасываются
    /// </summary>
    /// <param name="srcCollection">Исходный список объектов</param>
    public NamedList(IEnumerable<T> srcCollection)
      : this(srcCollection, false)
    {
    }

    /// <summary>
    /// Создает список, заполняя его значениями из коллекции.
    /// Если список <paramref name="srcCollection"/> содержит объекты с повторяющимися кодами (с учетом <paramref name="ignoreCase"/>), повторы отбрасываются
    /// </summary>
    /// <param name="srcCollection">Исходный список объектов</param>
    /// <param name="ignoreCase">Надо ли игнорировать регистр кода</param>
    public NamedList(IEnumerable<T> srcCollection, bool ignoreCase)
      : this(ignoreCase)
    {
      foreach (T Item in srcCollection)
        Add(Item);
    }

    #endregion

    #region Доступ к элементам

    /// <summary>
    /// Линейный список, определяющий порядок элементов
    /// </summary>
    private readonly List<T> _List;

    /// <summary>
    /// Коллекция по кодам.
    /// Если IgnoreCase=true, то ключ преобразован к верхнему регистру
    /// Значением является индекс элемента в списке FList.
    /// Когда FDictIsValid=false, значения в FDict недействительны. Ключи действительны всегда
    /// </summary>
    [NonSerialized]
    private Dictionary<string, int> _Dict;

    /// <summary>
    /// Если true, то словарь _Dict содержит корректные значения
    /// </summary>
    [NonSerialized] // после десериализации всегда восстанавливаем словарь
    private bool _DictIsValid;

    /// <summary>
    /// Переводит словарь _Dict в корректное состояние.
    /// Пересчитывает значения, предполагая, что ключи - верные.
    /// </summary>
    /// <returns></returns>
    private void ValidateDict()
    {
#if DEBUG
      CheckDictCount();
#endif

      if (_DictIsValid)
        return;

      for (int i = 0; i < _List.Count; i++)
      {
        string Code = _List[i].Code;
        if (_IgnoreCase)
          Code = Code.ToUpperInvariant();
        _Dict[Code] = i;
      }

#if DEBUG
      CheckDictCount(); // вдруг в словаре были перепутаны ключи. Тогда теперь стало FDict.Count > FList.Count
#endif

      _DictIsValid = true;
    }

#if DEBUG

    private void CheckDictCount()
    {
      if (_Dict.Count != _List.Count)
        throw new BugException("Длина списка (" + _List.Count.ToString() + ") не совпадает с длиной словаря индексов (" + _Dict.Count.ToString() + ")");
    }

#endif

    /// <summary>
    /// Доступ по индексу
    /// </summary>
    /// <param name="index">Индекс элемента в массиве. Должен быть в диапазоне от 0 до Count-1</param>
    /// <returns>Элемент</returns>
    public T this[int index]
    {
      get { return _List[index]; }
      set
      {
        CheckNotReadOnly();
#if DEBUG
        if (Object.Equals(value, default(T)))
          throw new ArgumentNullException();
#endif
        string NewCode = value.Code;
        if (String.IsNullOrEmpty(NewCode))
          throw new ArgumentException("value.Code пустой");
        if (_IgnoreCase)
          NewCode = NewCode.ToUpperInvariant();


        T OldItem = _List[index];
        _Dict.Remove(OldItem.Code);
        try
        {
          _Dict.Add(NewCode, index);
        }
        catch
        {
          string OldItemCode = OldItem.Code;
          if (_IgnoreCase)
            OldItemCode = OldItemCode.ToUpperInvariant();
          _Dict.Add(OldItemCode, index);
          _DictIsValid = false;
          throw;
        }
        _List[index] = value;
      }
    }

    /// <summary>
    /// Доступ по коду.
    /// Если запрошен несуществуюший код, возвращается пустой элемент
    /// </summary>
    /// <param name="code">Код объекта</param>
    /// <returns>Объект или пустое значение, если в списке нет объекта с таким кодом</returns>
    public T this[string code]
    {
      get
      {
        int Index = IndexOf(code); // восстанавливает словарь
        if (Index >= 0)
          return _List[Index];
        else
          return default(T);
      }
    }

    /// <summary>
    /// Возвращает элемент с заданным кодом.
    /// В отличие от индексированного доступа по коду, если не найден объект с заданным кодом, генерируется исключение
    /// </summary>
    /// <param name="code">Код объекта</param>
    /// <returns>Объект</returns>
    public T GetRequired(string code)
    {
      int Index = IndexOf(code); // восстанавливает словарь
      if (Index >= 0)
        return _List[Index];
      else if (string.IsNullOrEmpty(code))
        throw new ArgumentNullException("code");
      else
        throw new ArgumentException("В списке нет элемента с кодом \"" + code + "\"");
    }

    /// <summary>
    /// Возвращает количество элементов в списке
    /// </summary>
    public int Count { get { return _List.Count; } }

    /// <summary>
    /// Текстовое представление "Count=XXX"
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return "Count=" + Count.ToString();
    }

    #endregion

    #region IgnoreCase

    /// <summary>
    /// Если установлено в true, то при поиске элементов будет игнорироваться регистр.
    /// Если свойство установлено в false (по умолчанию), то регистр символов отличается
    /// Свойство устанавливается в конструкторе
    /// </summary>
    public bool IgnoreCase { get { return _IgnoreCase; } }
    private readonly bool _IgnoreCase;

    #endregion

    #region Доступ Только для чтения

    /// <summary>
    /// Возвращает true, если список был переведен в режим "только чтение"
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// Защищенный метод для перевода списка в режим "только чтение"
    /// </summary>
    internal protected void SetReadOnly()
    {
      if (_IsReadOnly)
        return; // Вложенный вызов игнорируется

      ValidateDict();
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

    #region IEnumerable<T> Members

    /// <summary>
    /// Возвращает перечислитель объектов в списке.
    /// 
    /// Тип возвращаемого значения может измениться в будущем, 
    /// гарантируется только реализация интерфейса перечислителя.
    /// Поэтому в прикладном коде метод должен использоваться исключительно для использования в операторе foreach.
    /// </summary>
    /// <returns>Перечислитель</returns>
    public List<T>.Enumerator GetEnumerator()
    {
      return _List.GetEnumerator();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return _List.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return _List.GetEnumerator();
    }

    #endregion

    #region IList<T> Members

    /// <summary>
    /// Возвращает индекс объекта в списке или (-1), если объект не найден.
    /// Если требуется только проверить наличие элемента с таким кодом, рекомендуется использовать
    /// метод Contains(), принимающий строковый код.
    /// Также рекомендуется использовать перегрузку метода IndexOf(), принимающую код, чтобы избежать лишнего сравнения объектов, которое обычно не нужно.
    /// </summary>
    /// <param name="item">Объект для поиска</param>
    /// <returns>Индекс объекта.</returns>
    public int IndexOf(T item)
    {
      if (Object.Equals(item, default(T)))
        return -1;

      int p = IndexOf(item.Code);
      if (p < 0)
        return -1;

      if (_List[p].Equals(item))
        return p;
      else
        return -1; // коды совпадают, а объекты - разные
    }

    /// <summary>
    /// Добавляет элемент в заданную позицию списка.
    /// Если в списке уже есть элемент с таким кодом (с учетом IgnoreCase), генерируется исключение
    /// </summary>
    /// <param name="index">Позиция для добавления</param>
    /// <param name="item">Добавляемый объект</param>
    public void Insert(int index, T item)
    {
      CheckNotReadOnly();
#if DEBUG
      if (Object.Equals(item, default(T)))
        throw new ArgumentNullException("item");
#endif
      if (index > _List.Count)
        throw new ArgumentOutOfRangeException("index", index, "Индекс должен быть в диапазоне от 0 до " + _List.Count.ToString());

      string ItemCode = item.Code;
      if (String.IsNullOrEmpty(ItemCode))
        throw new ArgumentException("Пустой Item.Code", "item");
      if (IgnoreCase)
        ItemCode = ItemCode.ToUpperInvariant();

      if (index < _List.Count)
        _DictIsValid = false; // иначе это обычный метод Add()

      _Dict.Add(ItemCode, index); // здесь возникнет исключение, если уже есть элемент с таким кодом
      try
      {
        _List.Insert(index, item);
      }
      catch
      {
        _DictIsValid = false;
        _Dict.Remove(ItemCode);
        throw;
      }
    }

    /// <summary>
    /// Удаляет элемент из указанной позиции списка.
    /// </summary>
    /// <param name="index">Индекс элемента</param>
    public void RemoveAt(int index)
    {
      CheckNotReadOnly();

      if (index < (_List.Count - 1))
        _DictIsValid = false; // иначе убирается последний элемент и словарь остается валидным

      T item = _List[index];
      _List.RemoveAt(index);

      string ItemCode = item.Code;
      if (IgnoreCase)
        ItemCode = ItemCode.ToUpperInvariant();

      _Dict.Remove(ItemCode);
    }

    #endregion

    #region ICollection<T> Members

    /// <summary>
    /// Добавляет элемент в конец списка.
    /// Если в списке уже есть элемент с таким кодом (с учетом IgnoreCase), генерируется исключение
    /// </summary>
    /// <param name="item">Добавляемый элемент</param>
    public void Add(T item)
    {
      CheckNotReadOnly();
#if DEBUG
      if (Object.Equals(item, default(T)))
        throw new ArgumentNullException("item");
#endif

      string ItemCode = item.Code;
      if (String.IsNullOrEmpty(ItemCode))
        throw new ArgumentException("Пустой Item.Code", "item");
      if (IgnoreCase)
        ItemCode = ItemCode.ToUpperInvariant();

      _Dict.Add(ItemCode, _List.Count); // здесь возникнет исключение, если уже есть элемент с таким кодом
      try
      {
        _List.Add(item);
      }
      catch
      {
        _DictIsValid = false;
        _Dict.Remove(ItemCode);
        throw;
      }

      // Словарь остается валидным, если он был валидным до этого
    }

    /// <summary>
    /// Очищает список
    /// </summary>
    public void Clear()
    {
      CheckNotReadOnly();

      _List.Clear();
      _Dict.Clear();
      _DictIsValid = true; // теперь словарь точно валидный
    }

    /// <summary>
    /// Выполняет быстрый поиск в списке элемента с таким кодом.
    /// Рекомендуется использовать перегрузку, принимающую строковый код.
    /// </summary>
    /// <param name="item">Искомый элемент</param>
    /// <returns>true, если элемент найден</returns>
    public bool Contains(T item)
    {
      if (Object.Equals(item, default(T)))
        return false;

      string ItemCode = item.Code;
      if (IgnoreCase)
        ItemCode = ItemCode.ToUpperInvariant();
      if (!_Dict.ContainsKey(ItemCode))
        return false;

      // 21.02.2018
      // Надо еще и на равенство проверить
      return item.Equals(this[ItemCode]);
    }

    /// <summary>
    /// Копирует весь список в массив.
    /// </summary>
    /// <param name="array">Заполняемый массив</param>
    public void CopyTo(T[] array)
    {
      _List.CopyTo(array);
    }

    /// <summary>
    /// Копирует весь список в массив.
    /// </summary>
    /// <param name="array">Заполняемый массив</param>
    /// <param name="arrayIndex">Начальный индекс в массиве для заполнения</param>
    public void CopyTo(T[] array, int arrayIndex)
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
    public void CopyTo(int index, T[] array, int arrayIndex, int count)
    {
      _List.CopyTo(index, array, arrayIndex, count);
    }

    /// <summary>
    /// Удаляет заданный элемент из списка
    /// </summary>
    /// <param name="item">удаляемый элемент</param>
    /// <returns>true, если элемент был найден и удален</returns>
    public bool Remove(T item)
    {
      CheckNotReadOnly();

      int p = IndexOf(item); // в том числе и сравнение элементов, а не только кода
      if (p >= 0)
      {
        RemoveAt(p);
        return true;
      }
      else
        return false;
    }

    #endregion

    #region Дополнительные методы

    /// <summary>
    /// Поиск по коду (с учетом IgnoreCase).
    /// Возвращает индекс найденного элемента или (-1).
    /// Если требуется только определить существование элемента с заданным кодом, используйте
    /// Contains(), принимающий строковый аргумент.
    /// </summary>
    /// <param name="code">Искомый код</param>
    /// <returns>Индекс объекта</returns>
    public int IndexOf(string code)
    {
      if (String.IsNullOrEmpty(code))
        return -1;

      if (IgnoreCase)
        code = code.ToUpperInvariant();

      ValidateDict();
      int p;
      if (_Dict.TryGetValue(code, out p))
        return p;
      else
        return -1;
    }

    /// <summary>
    /// Медленный поиск по коду (с учетом IgnoreCase).
    /// Возвращает индекс найденного элемента или (-1)
    /// Если требуется только определить существование элемента с заданным кодом, используйте
    /// Contains(), принимающий строковый аргумент.
    /// </summary>
    /// <param name="code">Искомый код</param>
    /// <returns>Индекс объекта</returns>
    private int SlowIndexOf(string code)
    {
      if (IgnoreCase)
      {
        for (int i = 0; i < _List.Count; i++)
        {
          if (String.Equals(_List[i].Code, code, StringComparison.OrdinalIgnoreCase))
            return i;
        }
      }
      else
      {
        for (int i = 0; i < _List.Count; i++)
        {
          if (_List[i].Code == code)
            return i;
        }
      }
      return -1;
    }

    /// <summary>
    /// Быстрый поиск кода (с учетом IgnoreCase).
    /// Возвращает true, если в списке есть элемент с заданным кодом
    /// </summary>
    /// <param name="code">Искомый код</param>
    /// <returns>Наличие элемента в списке</returns>
    public bool Contains(string code)
    {
      if (String.IsNullOrEmpty(code))
        return false;

      if (IgnoreCase)
        code = code.ToUpperInvariant();

      // Неважно, целый словарь или нет

      return _Dict.ContainsKey(code);
    }

    /// <summary>
    /// Быстрый поиск кода (с учетом IgnoreCase).
    /// Возвращает true, если в списке есть элемент с заданным кодом.
    /// При этом сразу возвращается и элемент.
    /// Если в списке нет элемента с таким кодом, возвращается false, 
    /// а Value получает пустое значение
    /// </summary>
    /// <param name="code">Искомый код</param>
    /// <param name="value">Сюда помещается найденное значение</param>
    /// <returns>Наличие элемента в списке</returns>
    public bool TryGetValue(string code, out T value)
    {
      if (String.IsNullOrEmpty(code))
      {
        value = default(T);
        return false;
      }

      if (IgnoreCase)
        code = code.ToUpperInvariant();

      ValidateDict();
      int p;
      if (_Dict.TryGetValue(code, out p))
      {
        value = _List[p];
        return true;
      }
      else
      {
        value = default(T);
        return false;
      }
    }

    /// <summary>
    /// Копирует список в массив
    /// </summary>
    /// <returns>Новый массив</returns>
    public T[] ToArray()
    {
      return _List.ToArray();
    }

    /// <summary>
    /// Добавляет несколько элементов в список.
    /// Эквивалентно последовательному вызову метода Add()
    /// </summary>
    /// <param name="collection">Список добавляемых элементов</param>
    public void AddRange(IEnumerable<T> collection)
    {
      CheckNotReadOnly();
#if DEBUG
      if (collection==null)
        throw new ArgumentException("collection");
#endif
      if (Object.ReferenceEquals(collection, this))
        throw new ArgumentException("Нельзя добавить элементы из самого себя", "collection");

      foreach (T Item in collection)
        Add(Item);
    }

    /// <summary>
    /// Удаляет элемент с заданным кодом (с учетом IgnoreCase)
    /// </summary>
    /// <param name="code">Код удаляемого элемента</param>
    /// <returns>true, если объект был найден и удален</returns>
    public bool Remove(string code)
    {
      CheckNotReadOnly();

      int p;
      if (_DictIsValid)
        p = IndexOf(code);
      else // не пытаемся чинить словарь каждый раз, чтобы не тормозить.
        p = SlowIndexOf(code);
      if (p >= 0)
      {
        RemoveAt(p);
        return true;
      }
      else
        return false;
    }

    /// <summary>
    /// Возвращает строковый массив с кодами элементов.
    /// Порядок соответствует расположению элементов в списке.
    /// Регистр символов не меняется, даже если IgnoreCase=true.
    /// </summary>
    /// <returns></returns>
    public string[] GetCodes()
    {
      string[] a = new string[_List.Count];
      for (int i = 0; i < _List.Count; i++)
        a[i] = _List[i].Code;

      return a;
    }


    /// <summary>
    /// Сортировка списка строк.
    /// При сортировке регистр символов учитывается или игнорируется, в зависимости от свойства IgnoreCase
    /// </summary>
    public void Sort()
    {
      CheckNotReadOnly();

      _DictIsValid = false;

      if (_IgnoreCase)
        _List.Sort(ObjectWithCodeComparer<T>.OrdinalIgnoreCase);
      else
        _List.Sort(ObjectWithCodeComparer<T>.Ordinal);
    }

    /// <summary>
    /// Заменяет порядок элементов на обратный
    /// </summary>
    public void Reverse()
    {
      CheckNotReadOnly();

      _DictIsValid = false;

      _List.Reverse();
    }

    #endregion

    #region Десериализация

    [OnDeserialized]
    private void OnDeserializedMethod(StreamingContext context)
    {
      _Dict = new Dictionary<string, int>(_List.Count);
      for (int i = 0; i < _List.Count; i++)
      {
        string ItemCode = _List[i].Code;
        if (IgnoreCase)
          ItemCode = ItemCode.ToUpperInvariant();

        _Dict.Add(ItemCode, i);
      }
      _DictIsValid = true;
    }

    #endregion

    #region INamedValuesAccess Members

    object INamedValuesAccess.GetValue(string name)
    {
      return this[name];
    }

    string[] INamedValuesAccess.GetNames()
    {
      return GetCodes();
    }

    #endregion
  }
#endif

  /// <summary>
  /// Аргументы событий NamedListWithNotifications.BeforeAdd, AfterAdd, BeforeRemove и AfterRemove
  /// </summary>
  /// <typeparam name="T">Тип элементов, хранящихся в списке NamedListWithNotifications</typeparam>
  public sealed class NamedListItemEventArgs<T> : EventArgs
    where T : IObjectWithCode
  {
    #region Конструктор

    /// <summary>
    /// Создает экземпляр объекта
    /// </summary>
    /// <param name="item">Добавляемый или удаляемый элемент коллекции</param>
    public NamedListItemEventArgs(T item)
    {
      _Item = item;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Добавляемый или удаляемый элемент коллекции
    /// </summary>
    public T Item { get { return _Item; } }
    private readonly T _Item;

    #endregion
  }

  /// <summary>
  /// Делегат событий NamedListWithNotifications.ItemAdded и ItemRemoved
  /// </summary>
  /// <typeparam name="T">Тип элементов, хранящихся в списке NamedListWithNotifications</typeparam>
  /// <param name="sender">Объект списка</param>
  /// <param name="args">Аргументы события</param>
  public delegate void NamedListItemEventHandler<T>(object sender, NamedListItemEventArgs<T> args)
    where T : IObjectWithCode;

#if OLD
  /// <summary>
  /// Список объектов произвольного типа, доступ к которым может осуществляться как по
  /// индексу (как в обычном списке List), так и по коду, как в Dictionary с ключом String.
  /// Список не может содержать значения null.
  /// В отличие от NamedList, при добавлении и удалении элементов списка вызываются виртуальные методы и события,
  /// поэтому этот список работает медленнее.
  /// В отличие от NamedList, этот класс не является потокобезопасным.
  /// </summary>
  /// <typeparam name="T">Тип объектов, хранящихся в списке, поддерживающих интерфейс IObjectWithCode</typeparam>
  [Serializable]
  public class NamedListWithNotifications<T> : IEnumerable<T>, IList<T>, IReadOnlyObject, INamedValuesAccess
    where T : IObjectWithCode
  {
    #region Конструкторы

    // В отличие от NamedList, у этого класса нет конструкторов, принимающих источники данных.
    // Предполагается наличие какой-либо обработки в производном классе или присоединении обработчиков событий.

    /// <summary>
    /// Создает пустой список.
    /// Регистр кода учитывается
    /// </summary>
    public NamedListWithNotifications()
      : this(false)
    {
    }

    /// <summary>
    /// Создает пустой список
    /// </summary>
    /// <param name="IgnoreCase">Надо ли игнорировать регистр кода</param>
    public NamedListWithNotifications(bool IgnoreCase)
    {
      FList = new List<T>();
      FDict = new Dictionary<string, T>();
      FIgnoreCase = IgnoreCase;
    }

    /// <summary>
    /// Создает пустой список заданной емкости.
    /// Регистр кода учитывается.
    /// </summary>
    /// <param name="Capacity">Начальная емкость списка</param>
    public NamedListWithNotifications(int Capacity)
      : this(Capacity, false)
    {
    }

    /// <summary>
    /// Создает пустой список заданной емкости
    /// </summary>
    /// <param name="Capacity">Начальная емкость списка</param>
    /// <param name="IgnoreCase">Надо ли игнорировать регистр кода</param>
    public NamedListWithNotifications(int Capacity, bool IgnoreCase)
    {
      FList = new List<T>(Capacity);
      FDict = new Dictionary<string, T>(Capacity);
      FIgnoreCase = IgnoreCase;
    }

    #endregion

    #region Доступ к элементам

    /// <summary>
    /// Линейный список, определяющий порядок элементов
    /// </summary>
    private List<T> FList;

    /// <summary>
    /// Коллекция по кодам.
    /// Если IgnoreCase=true, то ключ преобразован к верхнему регистру
    /// </summary>
    [NonSerialized]
    private Dictionary<string, T> FDict;

    /// <summary>
    /// Доступ по индексу
    /// </summary>
    /// <param name="Index">Индекс элемента в массиве. Должен быть в диапазоне от 0 до Count-1</param>
    /// <returns></returns>
    public T this[int Index]
    {
      get { return FList[Index]; }
      set
      {
        CheckNotReadOnly();

        T OldItem = FList[Index];

        OnBeforeAdd(value);
        OnBeforeRemove(OldItem);

        FDict.Remove(OldItem.Code);
        try
        {
          string NewCode = value.Code;
          if (FIgnoreCase)
            NewCode = NewCode.ToUpperInvariant();
          FDict.Add(NewCode, value);
        }
        catch
        {
          string OldItemCode = OldItem.Code;
          if (FIgnoreCase)
            OldItemCode = OldItemCode.ToUpperInvariant();
          FDict.Add(OldItemCode, OldItem);
          throw;
        }
        FList[Index] = value;

        OnAfterAdd(value);
        OnAfterRemove(OldItem);

        CallListChanged(ListChangedType.ItemChanged, Index);
      }
    }

    /// <summary>
    /// Доступ по коду.
    /// Если запрошен несуществуюший код, возвращается пустой элемент
    /// </summary>
    /// <param name="Code">Код объекта</param>
    /// <returns>Объект или пустое значение, если в списке нет объекта с таким кодом</returns>
    public T this[string Code]
    {
      get
      {
        T res;
        if (string.IsNullOrEmpty(Code))
          return default(T);

        if (IgnoreCase)
          Code = Code.ToUpperInvariant();

        if (FDict.TryGetValue(Code, out res))
          return res;
        else
          return default(T);
      }
    }

    /// <summary>
    /// Возвращает элемент с заданным кодом.
    /// В отличие от индексированного доступа по коду, если не найден объект с заданным кодом, генерируется исключение
    /// </summary>
    /// <param name="Code">Код объекта</param>
    /// <returns>Объект</returns>
    public T GetRequired(string Code)
    {
      T res;
      if (string.IsNullOrEmpty(Code))
        throw new ArgumentNullException("Code");

      if (IgnoreCase)
        Code = Code.ToUpperInvariant();

      if (FDict.TryGetValue(Code, out res))
        return res;
      else
        throw new ArgumentException("В списке нет элемента с кодом \"" + Code + "\"");
    }

    /// <summary>
    /// Возвращает количество элементов в списке
    /// </summary>
    public int Count { get { return FList.Count; } }

    /// <summary>
    /// Текстовое представление "Count=XXX"
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return "Count=" + Count.ToString();
    }

    #endregion

    #region IgnoreCase

    /// <summary>
    /// Если установлено в true, то при поиске элементов будет игнорироваться регистр.
    /// Если свойство установлено в false (по умолчанию), то регистр символов отличается
    /// Свойство устанавливается в конструкторе
    /// </summary>
    public bool IgnoreCase { get { return FIgnoreCase; } }
    private bool FIgnoreCase;

    #endregion

    #region Доступ Только для чтения

    /// <summary>
    /// Возвращает true, если список был переведен в режим "только чтение"
    /// </summary>
    public bool IsReadOnly { get { return FIsReadOnly; } }
    private bool FIsReadOnly;

    /// <summary>
    /// Защищенный метод для перевода списка в режим "только чтение"
    /// </summary>
    internal protected void SetReadOnly()
    {
      FIsReadOnly = true;
    }

    /// <summary>
    /// Генерирует исключение, если IsReadOnly=true
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (FIsReadOnly)
        throw new ObjectReadOnlyException();
    }

    #endregion

    #region IEnumerable<T> Members

    /// <summary>
    /// Возвращает перечислитель объектов в списке
    /// </summary>
    /// <returns>Перечислитель</returns>
    public IEnumerator<T> GetEnumerator()
    {
      return FList.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return FList.GetEnumerator();
    }

    #endregion

    #region IList<T> Members

    /// <summary>
    /// Возвращает индекс объекта в списке или (-1), если объект не найден.
    /// Метод является медленным.
    /// Если требуется только проверить наличие элемента с таким кодом, рекомендуется использовать
    /// метод Contains(), принимающий строковый код
    /// </summary>
    /// <param name="item">Объект для поиска</param>
    /// <returns>Индекс объекта.</returns>
    public int IndexOf(T item)
    {
      return FList.IndexOf(item);
    }

    /// <summary>
    /// Добавляет элемент в заданную позицию списка.
    /// Если в списке уже есть элемент с таким кодом (с учетом IgnoreCase), генерируется исключение
    /// </summary>
    /// <param name="index">Позиция для добавления</param>
    /// <param name="item">Добавляемый объект</param>
    public void Insert(int index, T item)
    {
      CheckNotReadOnly();

      OnBeforeAdd(item);

      string ItemCode = item.Code;
      if (IgnoreCase)
        ItemCode = ItemCode.ToUpperInvariant();

      FDict.Add(ItemCode, item);
      try
      {
        FList.Insert(index, item);
      }
      catch
      {
        FDict.Remove(ItemCode);
        throw;
      }

      OnAfterAdd(item);
      CallListChanged(ListChangedType.ItemAdded, index);
    }

    /// <summary>
    /// Удаляет элемент из указанной позиции списка.
    /// </summary>
    /// <param name="index">Индекс элемента</param>
    public void RemoveAt(int index)
    {
      CheckNotReadOnly();

      T item = FList[index];

      OnBeforeRemove(item);

      FList.RemoveAt(index);

      string ItemCode = item.Code;
      if (IgnoreCase)
        ItemCode = ItemCode.ToUpperInvariant();

      FDict.Remove(ItemCode);

      OnAfterRemove(item);
      ListChangedEventArgs Args = new ListChangedEventArgs(ListChangedType.ItemDeleted, index);
    }

    #endregion

    #region ICollection<T> Members

    /// <summary>
    /// Добавляет элемент в конец списка.
    /// Если в списке уже есть элемент с таким кодом (с учетом IgnoreCase), генерируется исключение
    /// </summary>
    /// <param name="item">Добавляемый элемент</param>
    public void Add(T item)
    {
      CheckNotReadOnly();

      string ItemCode = item.Code;
      if (IgnoreCase)
        ItemCode = ItemCode.ToUpperInvariant();

      OnBeforeAdd(item);

      FDict.Add(ItemCode, item);
      try
      {
        FList.Add(item);
      }
      catch
      {
        FDict.Remove(ItemCode);
        throw;
      }

      OnAfterAdd(item);
      CallListChanged(ListChangedType.ItemAdded, FList.Count - 1);
    }

    /// <summary>
    /// Очищает список
    /// </summary>
    public void Clear()
    {
      CheckNotReadOnly();

      if (Count == 0)
        return;

      T[] Items = ToArray();
      for (int i = 0; i < Items.Length; i++)
        OnBeforeRemove(Items[i]);

      FList.Clear();
      FDict.Clear();

      for (int i = 0; i < Items.Length; i++)
        OnAfterRemove(Items[i]);

      CallListChanged(ListChangedType.Reset, -1);
    }

    /// <summary>
    /// Выполняет быстрый поиск в списке элемента с таким кодом.
    /// Рекомендуется использовать перегрузку, принимающую строковый код.
    /// </summary>
    /// <param name="item">Искомый элемент</param>
    /// <returns>true, если элемент найден</returns>
    public bool Contains(T item)
    {
      string ItemCode = item.Code;
      if (IgnoreCase)
        ItemCode = ItemCode.ToUpperInvariant();
      // return FDict.ContainsKey(ItemCode);

      // 21.02.2018
      // Надо еще и на равенство проверить
      T ResItem;
      if (FDict.TryGetValue(ItemCode, out ResItem))
        return item.Equals(ResItem);
      else
        return false;
    }

    /// <summary>
    /// Копирует список в массив
    /// </summary>
    /// <param name="array">Заполняемый массив</param>
    /// <param name="arrayIndex">Начальный индекс в массиве для заполнения</param>
    public void CopyTo(T[] array, int arrayIndex)
    {
      FList.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Удаляет заданный элемент из списка
    /// </summary>
    /// <param name="item">удаляемый элемент</param>
    /// <returns>true, если элемент был найден и удален</returns>
    public bool Remove(T item)
    {
      CheckNotReadOnly();

      if (!Contains(item)) // быстрая проверка
        return false;

      int p = IndexOf(item);
      RemoveAt(p);
      return true;
    }

    #endregion

    #region Дополнительные методы

    /// <summary>
    /// Медленный поиск по коду (с учетом IgnoreCase).
    /// Возвращает индекс найденного элемента или (-1)
    /// Если требуется только определить существование элемента с заданным кодом, используйте
    /// Contains(), принимающий строковый аргумент.
    /// </summary>
    /// <param name="Code">Искомый код</param>
    /// <returns>Индекс объекта</returns>
    public int IndexOf(string Code)
    {
      if (IgnoreCase)
      {
        for (int i = 0; i < FList.Count; i++)
        {
          if (String.Equals(FList[i].Code, Code, StringComparison.OrdinalIgnoreCase))
            return i;
        }
      }
      else
      {
        for (int i = 0; i < FList.Count; i++)
        {
          if (FList[i].Code == Code)
            return i;
        }
      }
      return -1;
    }

    /// <summary>
    /// Быстрый поиск кода (с учетом IgnoreCase).
    /// Возвращает true, если в списке есть элемент с заданным кодом
    /// </summary>
    /// <param name="Code">Искомый код</param>
    /// <returns>Наличие элемента в списке</returns>
    public bool Contains(string Code)
    {
      if (String.IsNullOrEmpty(Code))
        return false;

      if (IgnoreCase)
        Code = Code.ToUpperInvariant();

      return FDict.ContainsKey(Code);
    }

    /// <summary>
    /// Быстрый поиск кода (с учетом IgnoreCase).
    /// Возвращает true, если в списке есть элемент с заданным кодом.
    /// При этом сразу возвращается и элемент.
    /// Если в списке нет элемента с таким кодом, возвращается false, 
    /// а Value получает пустое значение
    /// </summary>
    /// <param name="Code">Искомый код</param>
    /// <param name="Value">Сюда помещается найденное значение</param>
    /// <returns>Наличие элемента в списке</returns>
    public bool TryGetValue(string Code, out T Value)
    {
      if (String.IsNullOrEmpty(Code))
      {
        Value = default(T);
        return false;
      }

      if (IgnoreCase)
        Code = Code.ToUpperInvariant();

      return FDict.TryGetValue(Code, out Value);
    }

    /// <summary>
    /// Копирует список в массив
    /// </summary>
    /// <returns>Новый массив</returns>
    public T[] ToArray()
    {
      return FList.ToArray();
    }

    /// <summary>
    /// Добавляет несколько элементов в список.
    /// Эквивалентно последовательному вызову метода Add()
    /// </summary>
    /// <param name="Collection">Список добавляемых элементов</param>
    public void AddRange(IEnumerable<T> Collection)
    {
      CheckNotReadOnly();
#if DEBUG
      if (collection==null)
        throw new ArgumentException("collection");
#endif
      if (Object.ReferenceEquals(collection, this))
        throw new ArgumentException("Нельзя добавить элементы из самого себя", "collection");

      BeginUpdate();
      try
      {
        foreach (T Item in Collection)
          Add(Item);
      }
      finally
      {
        EndUpdate();
      }
    }

    /// <summary>
    /// Удаляет элемент с заданным кодом (с учетом IgnoreCase)
    /// </summary>
    /// <param name="Code">Код удаляемого элемента</param>
    /// <returns>true, если объект был найден и удален</returns>
    public bool Remove(string Code)
    {
      CheckNotReadOnly();

      if (!Contains(Code)) // быстрая проверка
        return false;

      int p = IndexOf(Code);
      RemoveAt(p);
      return true;
    }

    /// <summary>
    /// Возвращает строковый массив с кодами элементов.
    /// Порядок соответствует расположению элементов в списке.
    /// Регистр символов не меняется, даже если IgnoreCase=true.
    /// </summary>
    /// <returns></returns>
    public string[] GetCodes()
    {
      string[] a = new string[FList.Count];
      for (int i = 0; i < FList.Count; i++)
        a[i] = FList[i].Code;

      return a;
    }

    #endregion

  #region Извещения при изменениях в списке

    /// <summary>
    /// Событие вызывается перед добавлением элемента.
    /// Если обработчик события выбросит исключение, список останется в неизменном и согласованном состоянии.
    /// Событие не вызывается, если есть непарный вызов BeginUpdate()
    /// </summary>
    public event NamedListItemEventHandler<T> BeforeAdd;

    /// <summary>
    /// Вызывает событие BeforeAdd, если нет непарного BeginUpdate().
    /// </summary>
    /// <param name="Item">Добавляемый элемент</param>
    protected virtual void OnBeforeAdd(T Item)
    {
      if (FUpdateCount == 0 && BeforeAdd != null)
      {
        NamedListItemEventArgs<T> Args = new NamedListItemEventArgs<T>(Item);
        BeforeAdd(this, Args);
      }
    }

    /// <summary>
    /// Событие вызывается после добавления элемента.
    /// Если обработчик события выбросит исключение, список окажется в несогласованном состоянии и не может использоваться дальше.
    /// Событие не вызывается, если есть непарный вызов BeginUpdate()
    /// </summary>
    public event NamedListItemEventHandler<T> AfterAdd;

    /// <summary>
    /// Вызывает событие AfterAdd, если нет непарного BeginUpdate().
    /// </summary>
    /// <param name="Item">Добавленный элемент</param>
    protected virtual void OnAfterAdd(T Item)
    {
      if (FUpdateCount == 0 && AfterAdd != null)
      {
        NamedListItemEventArgs<T> Args = new NamedListItemEventArgs<T>(Item);
        AfterAdd(this, Args);
      }
    }

    /// <summary>
    /// Событие вызывается перед удалением элемента.
    /// Если обработчик события выбросит исключение, список останется в неизменном и согласованном состоянии.
    /// Событие не вызывается, если есть непарный вызов BeginUpdate()
    /// </summary>
    public event NamedListItemEventHandler<T> BeforeRemove;

    /// <summary>
    /// Вызывает событие BeforeRemove, если нет непарного BeginUpdate().
    /// </summary>
    /// <param name="Item">Удаляемый элемент</param>
    protected virtual void OnBeforeRemove(T Item)
    {
      if (FUpdateCount == 0 && BeforeRemove != null)
      {
        NamedListItemEventArgs<T> Args = new NamedListItemEventArgs<T>(Item);
        BeforeRemove(this, Args);
      }
    }

    /// <summary>
    /// Событие вызывается после удаления элемента.
    /// Если обработчик события выбросит исключение, список окажется в несогласованном состоянии и не может использоваться дальше.
    /// Событие не вызывается, если есть непарный вызов BeginUpdate()
    /// </summary>
    public event NamedListItemEventHandler<T> AfterRemove;

    /// <summary>
    /// Вызывает событие AfterRemove, если нет непарного BeginUpdate().
    /// </summary>
    /// <param name="Item">Добавленный элемент</param>
    protected virtual void OnAfterRemove(T Item)
    {
      if (FUpdateCount == 0 && AfterRemove != null)
      {
        NamedListItemEventArgs<T> Args = new NamedListItemEventArgs<T>(Item);
        AfterRemove(this, Args);
      }
    }

    /// <summary>
    /// Событие вызывается при изменениях в списке.
    /// </summary>
    public event ListChangedEventHandler ListChanged;

    /// <summary>
    /// Вызывает событие ListChanged, если нет непарного вызова BeginUpdate()
    /// </summary>
    /// <param name="Args">Аргументы события</param>
    protected void OnListChanged(ListChangedEventArgs Args)
    {
#if DEBUG
      if (Args == null)
        throw new ArgumentNullException("Args");
#endif

      if (InsideOnListChanged)
        throw new ReenterException("Вложенный вызов ListChanged");

      if (FUpdateCount > 0)
        DelayedListChanged = true; // запомнили на будущее
      else if (ListChanged != null)
      {
        InsideOnListChanged = true;
        try
        {
          ListChanged(this, Args);
        }
        finally
        {
          InsideOnListChanged = false;
        }
      }
    }

    private bool InsideOnListChanged;

    private void CallListChanged(ListChangedType ListChangedType, int NewIndex)
    {
      ListChangedEventArgs Args = new ListChangedEventArgs(ListChangedType, NewIndex);
      OnListChanged(Args);
    }

    /// <summary>
    /// Вызывает событие ListChanged с ListChangedType=ItemChanged
    /// </summary>
    /// <param name="Index">Индекс элемента. Должен быть в диапазоне от 0 до (Count-1)</param>
    public void NotifyItemChanged(int Index)
    {
      if (Index < 0 || Index >= Count)
        throw new ArgumentOutOfRangeException("Index", Index, "Индекс элемента вне диапазона");

      CallListChanged(ListChangedType.ItemChanged, Index);
    }

    #endregion

  #region Приостановка отправки извещений

    /// <summary>
    /// После вызова метода перестают посылаться извещения BeforeAdd, AfterAdd, BeforeRemove и AfterRemove.
    /// Должен обязательно завершаться парным вызовом EndUpdate().
    /// </summary>
    public virtual void BeginUpdate()
    {
      if (FUpdateCount == 0)
        DelayedListChanged = false; // по идее, никогда не должно срабатывать
      FUpdateCount++;
    }

    /// <summary>
    /// Окончание обновления списка.
    /// Вызов должен быть парным, по отношению к BeginUpdate()
    /// </summary>
    public virtual void EndUpdate()
    {
      if (FUpdateCount <= 0)
        throw new InvalidOperationException("Непарный вызов EndUpdate()");

      FUpdateCount--;
      if (FUpdateCount == 0 && DelayedListChanged)
      {
        CallListChanged(ListChangedType.Reset, -1);
        DelayedListChanged = false;
      }
    }

    /// <summary>
    /// Возвращает true, если был непарный вызов метода BeginUpdate
    /// </summary>
    public bool IsUpdating { get { return FUpdateCount > 0; } }
    private int FUpdateCount;

    /// <summary>
    /// Устанавливается в true при любых изменениях в списке, если был непарный вызов BeginUpdate().
    /// В этом случае, метод EndUpdate() отправляет сигнал о полном обновлении списка
    /// </summary>
    private bool DelayedListChanged;

    #endregion

  #region Десериализация

    [OnDeserialized]
    private void OnDeserializedMethod(StreamingContext context)
    {
      FDict = new Dictionary<string, T>(FList.Count);
      for (int i = 0; i < FList.Count; i++)
      {
        string ItemCode = FList[i].Code;
        if (IgnoreCase)
          ItemCode = ItemCode.ToUpperInvariant();

        FDict.Add(ItemCode, FList[i]);
      }
    }

    #endregion

  #region INamedValuesAccess Members

    object INamedValuesAccess.GetValue(string Name)
    {
      return this[Name];
    }

    string[] INamedValuesAccess.GetNames()
    {
      return GetCodes();
    }

    #endregion
  }
#else

  /// <summary>
  /// Список объектов произвольного типа, доступ к которым может осуществляться как по
  /// индексу (как в обычном списке List), так и по коду, как в Dictionary с ключом String.
  /// Список не может содержать значения null.
  /// В отличие от NamedList, при добавлении и удалении элементов списка вызываются виртуальные методы и события,
  /// поэтому этот список работает медленнее.
  /// В отличие от NamedList, этот класс не является потокобезопасным.
  /// </summary>
  /// <typeparam name="T">Тип объектов, хранящихся в списке, поддерживающих интерфейс IObjectWithCode</typeparam>
  [Serializable]
  public class NamedListWithNotifications<T> : IEnumerable<T>, IList<T>, IReadOnlyObject, INamedValuesAccess
    where T : class, IObjectWithCode
  {
    #region Конструкторы

    // В отличие от NamedList, у этого класса нет конструкторов, принимающих источники данных.
    // Предполагается наличие какой-либо обработки в производном классе или присоединении обработчиков событий.

    /// <summary>
    /// Создает пустой список.
    /// Регистр кода учитывается
    /// </summary>
    public NamedListWithNotifications()
      : this(false)
    {
    }

    /// <summary>
    /// Создает пустой список
    /// </summary>
    /// <param name="ignoreCase">Надо ли игнорировать регистр кода</param>
    public NamedListWithNotifications(bool ignoreCase)
    {
      _List = new List<T>();
      _Dict = new Dictionary<string, int>();
      _DictIsValid = true;
      _IgnoreCase = ignoreCase;
    }

    /// <summary>
    /// Создает пустой список заданной емкости.
    /// Регистр кода учитывается.
    /// </summary>
    /// <param name="capacity">Начальная емкость списка</param>
    public NamedListWithNotifications(int capacity)
      : this(capacity, false)
    {
    }

    /// <summary>
    /// Создает пустой список заданной емкости
    /// </summary>
    /// <param name="capacity">Начальная емкость списка</param>
    /// <param name="ignoreCase">Надо ли игнорировать регистр кода</param>
    public NamedListWithNotifications(int capacity, bool ignoreCase)
    {
      _List = new List<T>(capacity);
      _Dict = new Dictionary<string, int>(capacity);
      _DictIsValid = true;
      _IgnoreCase = ignoreCase;
    }

    #endregion

    #region Доступ к элементам

    /// <summary>
    /// Линейный список, определяющий порядок элементов
    /// </summary>
    private readonly List<T> _List;

    /// <summary>
    /// Коллекция по кодам.
    /// Если IgnoreCase=true, то ключ преобразован к верхнему регистру
    /// Значением является индекс элемента в списке FList.
    /// Когда FDictIsValid=false, значения в FDict недействительны. Ключи действительны всегда
    /// </summary>
    [NonSerialized]
    private Dictionary<string, int> _Dict;

    /// <summary>
    /// Если true, то словарь FDict содержит корректные значения
    /// </summary>
    [NonSerialized] // после десериализации всегда восстанавливаем словарь
    private bool _DictIsValid;

    /// <summary>
    /// Переводит словарь FDict в корректное состояние.
    /// Пересчитывает значения, предполагая, что ключи - верные.
    /// </summary>
    /// <returns></returns>
    private void ValidateDict()
    {
#if DEBUG
      CheckDictCount();
#endif

      if (_DictIsValid)
        return;

      for (int i = 0; i < _List.Count; i++)
      {
        string Code = _List[i].Code;
        if (_IgnoreCase)
          Code = Code.ToUpperInvariant();
        _Dict[Code] = i;
      }

#if DEBUG
      CheckDictCount(); // вдруг в словаре были перепутаны ключи. Тогда теперь стало FDict.Count > FList.Count
#endif

      _DictIsValid = true;
    }

#if DEBUG

    private void CheckDictCount()
    {
      if (_Dict.Count != _List.Count)
        throw new BugException("Длина списка (" + _List.Count.ToString() + ") не совпадает с длиной словаря индексов (" + _Dict.Count.ToString() + ")");
    }

#endif


    /// <summary>
    /// Доступ по индексу
    /// </summary>
    /// <param name="index">Индекс элемента в массиве. Должен быть в диапазоне от 0 до Count-1</param>
    /// <returns></returns>
    public T this[int index]
    {
      get { return _List[index]; }
      set
      {
        CheckNotReadOnly();
#if DEBUG
        if (Object.Equals(value, default(T)))
          throw new ArgumentNullException();
#endif
        string NewCode = value.Code;
        if (String.IsNullOrEmpty(NewCode))
          throw new ArgumentException("value.Code пустой");
        if (_IgnoreCase)
          NewCode = NewCode.ToUpperInvariant();

        T OldItem = _List[index];

        OnBeforeAdd(value);
        OnBeforeRemove(OldItem);

        _Dict.Remove(OldItem.Code);
        try
        {
          _Dict.Add(NewCode, index);
        }
        catch
        {
          string OldItemCode = OldItem.Code;
          if (_IgnoreCase)
            OldItemCode = OldItemCode.ToUpperInvariant();
          _Dict.Add(OldItemCode, index);
          _DictIsValid = false;
          throw;
        }
        _List[index] = value;

        OnAfterAdd(value);
        OnAfterRemove(OldItem);

        CallListChanged(ListChangedType.ItemChanged, index);
      }
    }

    /// <summary>
    /// Доступ по коду.
    /// Если запрошен несуществуюший код, возвращается пустой элемент
    /// </summary>
    /// <param name="code">Код объекта</param>
    /// <returns>Объект или пустое значение, если в списке нет объекта с таким кодом</returns>
    public T this[string code]
    {
      get
      {
        int Index = IndexOf(code); // восстанавливает словарь
        if (Index >= 0)
          return _List[Index];
        else
          return default(T);
      }
    }

    /// <summary>
    /// Возвращает элемент с заданным кодом.
    /// В отличие от индексированного доступа по коду, если не найден объект с заданным кодом, генерируется исключение
    /// </summary>
    /// <param name="code">Код объекта</param>
    /// <returns>Объект</returns>
    public T GetRequired(string code)
    {
      int Index = IndexOf(code); // восстанавливает словарь
      if (Index >= 0)
        return _List[Index];
      else if (string.IsNullOrEmpty(code))
        throw new ArgumentNullException("code");
      else
        throw new ArgumentException("В списке нет элемента с кодом \"" + code + "\"", "code");
    }

    /// <summary>
    /// Возвращает количество элементов в списке
    /// </summary>
    public int Count { get { return _List.Count; } }

    /// <summary>
    /// Текстовое представление "Count=XXX"
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return "Count=" + Count.ToString();
    }

    #endregion

    #region IgnoreCase

    /// <summary>
    /// Если установлено в true, то при поиске элементов будет игнорироваться регистр.
    /// Если свойство установлено в false (по умолчанию), то регистр символов отличается
    /// Свойство устанавливается в конструкторе
    /// </summary>
    public bool IgnoreCase { get { return _IgnoreCase; } }
    private readonly bool _IgnoreCase;

    #endregion

    #region Доступ Только для чтения

    /// <summary>
    /// Возвращает true, если список был переведен в режим "только чтение"
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// Защищенный метод для перевода списка в режим "только чтение"
    /// </summary>
    internal protected void SetReadOnly()
    {
      if (_IsReadOnly)
        return; // Вложенный вызов игнорируется

      ValidateDict();
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

    #region IEnumerable<T> Members

    /// <summary>
    /// Возвращает перечислитель объектов в списке
    /// 
    /// Тип возвращаемого значения может измениться в будущем, 
    /// гарантируется только реализация интерфейса перечислителя.
    /// Поэтому в прикладном коде метод должен использоваться исключительно для использования в операторе foreach.
    /// </summary>
    /// <returns>Перечислитель</returns>
    public List<T>.Enumerator GetEnumerator()
    {
      return _List.GetEnumerator();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return _List.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return _List.GetEnumerator();
    }

    #endregion

    #region IList<T> Members

    /// <summary>
    /// Возвращает индекс объекта в списке или (-1), если объект не найден.
    /// Если требуется только проверить наличие элемента с таким кодом, рекомендуется использовать
    /// метод Contains(), принимающий строковый код.
    /// Также рекомендуется использовать перегрузку метода IndexOf(), принимающую код, чтобы избежать лишнего сравнения объектов, которое обычно не нужно.
    /// </summary>
    /// <param name="item">Объект для поиска</param>
    /// <returns>Индекс объекта.</returns>
    public int IndexOf(T item)
    {
      if (Object.Equals(item, default(T)))
        return -1;

      int p = IndexOf(item.Code);
      if (p < 0)
        return -1;

      if (_List[p].Equals(item))
        return p;
      else
        return -1; // коды совпадают, а объекты - разные
    }

    /// <summary>
    /// Добавляет элемент в заданную позицию списка.
    /// Если в списке уже есть элемент с таким кодом (с учетом IgnoreCase), генерируется исключение
    /// </summary>
    /// <param name="index">Позиция для добавления</param>
    /// <param name="item">Добавляемый объект</param>
    public void Insert(int index, T item)
    {
      CheckNotReadOnly();
#if DEBUG
      if (Object.Equals(item, default(T)))
        throw new ArgumentNullException("item");
#endif
      if (index > _List.Count)
        throw new ArgumentOutOfRangeException("index", index, "Индекс должен быть в диапазоне от 0 до " + _List.Count.ToString());

      string ItemCode = item.Code;
      if (String.IsNullOrEmpty(ItemCode))
        throw new ArgumentException("Пустой Item.Code", "item");
      if (IgnoreCase)
        ItemCode = ItemCode.ToUpperInvariant();

      OnBeforeAdd(item);

      if (index < _List.Count)
        _DictIsValid = false; // иначе это обычный метод Add()

      _Dict.Add(ItemCode, index);
      try
      {
        _List.Insert(index, item);// здесь возникнет исключение, если уже есть элемент с таким кодом
      }
      catch
      {
        _DictIsValid = false;
        _Dict.Remove(ItemCode);
        throw;
      }

      OnAfterAdd(item);
      CallListChanged(ListChangedType.ItemAdded, index);
    }

    /// <summary>
    /// Удаляет элемент из указанной позиции списка.
    /// </summary>
    /// <param name="index">Индекс элемента</param>
    public void RemoveAt(int index)
    {
      CheckNotReadOnly();

      if (index < (_List.Count - 1))
        _DictIsValid = false; // иначе убирается последний элемент и словарь остается валидным

      T item = _List[index];

      OnBeforeRemove(item);

      _List.RemoveAt(index);

      string ItemCode = item.Code;
      if (IgnoreCase)
        ItemCode = ItemCode.ToUpperInvariant();

      _Dict.Remove(ItemCode);

      OnAfterRemove(item);
      CallListChanged(ListChangedType.ItemDeleted, index);
    }

    #endregion

    #region ICollection<T> Members

    /// <summary>
    /// Добавляет элемент в конец списка.
    /// Если в списке уже есть элемент с таким кодом (с учетом IgnoreCase), генерируется исключение
    /// </summary>
    /// <param name="item">Добавляемый элемент</param>
    public void Add(T item)
    {
      CheckNotReadOnly();
#if DEBUG
      if (Object.Equals(item, default(T)))
        throw new ArgumentNullException("item");
#endif

      string ItemCode = item.Code;
      if (String.IsNullOrEmpty(ItemCode))
        throw new ArgumentException("Пустой Item.Code", "item");
      if (IgnoreCase)
        ItemCode = ItemCode.ToUpperInvariant();

      OnBeforeAdd(item);

      _Dict.Add(ItemCode, _List.Count); // здесь возникнет исключение, если уже есть элемент с таким кодом
      try
      {
        _List.Add(item);
      }
      catch
      {
        _DictIsValid = false;
        _Dict.Remove(ItemCode);
        throw;
      }

      // Словарь остается валидным, если он был валидным до этого

      OnAfterAdd(item);
      CallListChanged(ListChangedType.ItemAdded, _List.Count - 1);
    }

    /// <summary>
    /// Очищает список
    /// </summary>
    public void Clear()
    {
      CheckNotReadOnly();

      if (Count == 0)
        return;

      T[] Items = ToArray();
      for (int i = 0; i < Items.Length; i++)
        OnBeforeRemove(Items[i]);

      _List.Clear();
      _Dict.Clear();
      _DictIsValid = true; // теперь словарь точно валидный

      for (int i = 0; i < Items.Length; i++)
        OnAfterRemove(Items[i]);

      CallListChanged(ListChangedType.Reset, -1);
    }

    /// <summary>
    /// Выполняет быстрый поиск в списке элемента с таким кодом.
    /// Рекомендуется использовать перегрузку, принимающую строковый код.
    /// </summary>
    /// <param name="item">Искомый элемент</param>
    /// <returns>true, если элемент найден</returns>
    public bool Contains(T item)
    {
      if (Object.Equals(item, default(T)))
        return false;

      string ItemCode = item.Code;
      if (IgnoreCase)
        ItemCode = ItemCode.ToUpperInvariant();
      if (!_Dict.ContainsKey(ItemCode))
        return false;

      // Надо еще и на равенство проверить
      return item.Equals(this[ItemCode]);
    }

    /// <summary>
    /// Копирует весь список в массив.
    /// </summary>
    /// <param name="array">Заполняемый массив</param>
    public void CopyTo(T[] array)
    {
      _List.CopyTo(array);
    }

    /// <summary>
    /// Копирует весь список в массив.
    /// </summary>
    /// <param name="array">Заполняемый массив</param>
    /// <param name="arrayIndex">Начальный индекс в массиве для заполнения</param>
    public void CopyTo(T[] array, int arrayIndex)
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
    public void CopyTo(int index, T[] array, int arrayIndex, int count)
    {
      _List.CopyTo(index, array, arrayIndex, count);
    }

    /// <summary>
    /// Удаляет заданный элемент из списка
    /// </summary>
    /// <param name="item">удаляемый элемент</param>
    /// <returns>true, если элемент был найден и удален</returns>
    public bool Remove(T item)
    {
      CheckNotReadOnly();

      int p = IndexOf(item); // в том числе и сравнение элементов, а не только кода
      if (p >= 0)
      {
        RemoveAt(p);
        return true;
      }
      else
        return false;
    }

    #endregion

    #region Дополнительные методы

    /// <summary>
    /// Медленный поиск по коду (с учетом IgnoreCase).
    /// Возвращает индекс найденного элемента или (-1).
    /// Если требуется только определить существование элемента с заданным кодом, используйте
    /// Contains(), принимающий строковый аргумент.
    /// </summary>
    /// <param name="code">Искомый код</param>
    /// <returns>Индекс объекта</returns>
    public int IndexOf(string code)
    {
      if (String.IsNullOrEmpty(code))
        return -1;

      if (IgnoreCase)
        code = code.ToUpperInvariant();

      ValidateDict();
      int p;
      if (_Dict.TryGetValue(code, out p))
        return p;
      else
        return -1;
    }

    /// <summary>
    /// Медленный поиск по коду (с учетом IgnoreCase).
    /// Возвращает индекс найденного элемента или (-1)
    /// Если требуется только определить существование элемента с заданным кодом, используйте
    /// Contains(), принимающий строковый аргумент.
    /// </summary>
    /// <param name="code">Искомый код</param>
    /// <returns>Индекс объекта</returns>
    private int SlowIndexOf(string code)
    {
      if (IgnoreCase)
      {
        for (int i = 0; i < _List.Count; i++)
        {
          if (String.Equals(_List[i].Code, code, StringComparison.OrdinalIgnoreCase))
            return i;
        }
      }
      else
      {
        for (int i = 0; i < _List.Count; i++)
        {
          if (_List[i].Code == code)
            return i;
        }
      }
      return -1;
    }

    /// <summary>
    /// Быстрый поиск кода (с учетом IgnoreCase).
    /// Возвращает true, если в списке есть элемент с заданным кодом
    /// </summary>
    /// <param name="code">Искомый код</param>
    /// <returns>Наличие элемента в списке</returns>
    public bool Contains(string code)
    {
      if (String.IsNullOrEmpty(code))
        return false;

      if (IgnoreCase)
        code = code.ToUpperInvariant();

      // Неважно, целый словарь или нет

      return _Dict.ContainsKey(code);
    }

    /// <summary>
    /// Быстрый поиск кода (с учетом IgnoreCase).
    /// Возвращает true, если в списке есть элемент с заданным кодом.
    /// При этом сразу возвращается и элемент.
    /// Если в списке нет элемента с таким кодом, возвращается false, 
    /// а Value получает пустое значение
    /// </summary>
    /// <param name="code">Искомый код</param>
    /// <param name="value">Сюда помещается найденное значение</param>
    /// <returns>Наличие элемента в списке</returns>
    public bool TryGetValue(string code, out T value)
    {
      if (String.IsNullOrEmpty(code))
      {
        value = default(T);
        return false;
      }

      if (IgnoreCase)
        code = code.ToUpperInvariant();

      ValidateDict();
      int p;
      if (_Dict.TryGetValue(code, out p))
      {
        value = _List[p];
        return true;
      }
      else
      {
        value = default(T);
        return false;
      }
    }

    /// <summary>
    /// Копирует список в массив
    /// </summary>
    /// <returns>Новый массив</returns>
    public T[] ToArray()
    {
      return _List.ToArray();
    }

    /// <summary>
    /// Добавляет несколько элементов в список.
    /// Эквивалентно последовательному вызову метода Add()
    /// </summary>
    /// <param name="collection">Список добавляемых элементов</param>
    public void AddRange(IEnumerable<T> collection)
    {
      CheckNotReadOnly();
#if DEBUG
      if (collection == null)
        throw new ArgumentException("collection");
#endif
      if (Object.ReferenceEquals(collection, this))
        throw new ArgumentException("Нельзя добавить элементы из самого себя", "collection");

      BeginUpdate();
      try
      {
        foreach (T Item in collection)
          Add(Item);
      }
      finally
      {
        EndUpdate();
      }
    }

    /// <summary>
    /// Удаляет элемент с заданным кодом (с учетом IgnoreCase)
    /// </summary>
    /// <param name="code">Код удаляемого элемента</param>
    /// <returns>true, если объект был найден и удален</returns>
    public bool Remove(string code)
    {
      CheckNotReadOnly();

      int p;
      if (_DictIsValid)
        p = IndexOf(code);
      else // не пытаемся чинить словарь каждый раз, чтобы не тормозить.
        p = SlowIndexOf(code);
      if (p >= 0)
      {
        RemoveAt(p);
        return true;
      }
      else
        return false;
    }

    /// <summary>
    /// Возвращает строковый массив с кодами элементов.
    /// Порядок соответствует расположению элементов в списке.
    /// Регистр символов не меняется, даже если IgnoreCase=true.
    /// </summary>
    /// <returns></returns>
    public string[] GetCodes()
    {
      string[] a = new string[_List.Count];
      for (int i = 0; i < _List.Count; i++)
        a[i] = _List[i].Code;

      return a;
    }

    /// <summary>
    /// Сортировка списка строк.
    /// При сортировке регистр символов учитывается или игнорируется, в зависимости от свойства IgnoreCase.
    /// После сортировки вызывается событие ListChanged в режиме Reset.
    /// </summary>
    public void Sort()
    {
      CheckNotReadOnly();

      _DictIsValid = false;

      if (_IgnoreCase)
        _List.Sort(ObjectWithCodeComparer<T>.OrdinalIgnoreCase);
      else
        _List.Sort(ObjectWithCodeComparer<T>.Ordinal);

      CallListChanged(ListChangedType.Reset, -1);
    }

    /// <summary>
    /// Заменяет порядок элементов на обратный.
    /// После сортировки вызывается событие ListChanged в режиме Reset.
    /// </summary>
    public void Reverse()
    {
      CheckNotReadOnly();

      _DictIsValid = false;

      _List.Reverse();

      CallListChanged(ListChangedType.Reset, -1);
    }

    #endregion

    #region Извещения при изменениях в списке

    /// <summary>
    /// Событие вызывается перед добавлением элемента.
    /// Если обработчик события выбросит исключение, список останется в неизменном и согласованном состоянии.
    /// Событие не вызывается, если есть непарный вызов BeginUpdate()
    /// </summary>
    public event NamedListItemEventHandler<T> BeforeAdd;

    /// <summary>
    /// Вызывает событие BeforeAdd, если нет непарного BeginUpdate().
    /// </summary>
    /// <param name="item">Добавляемый элемент</param>
    protected virtual void OnBeforeAdd(T item)
    {
      if (_UpdateCount == 0 && BeforeAdd != null)
      {
        NamedListItemEventArgs<T> Args = new NamedListItemEventArgs<T>(item);
        BeforeAdd(this, Args);
      }
    }

    /// <summary>
    /// Событие вызывается после добавления элемента.
    /// Если обработчик события выбросит исключение, список окажется в несогласованном состоянии и не может использоваться дальше.
    /// Событие не вызывается, если есть непарный вызов BeginUpdate()
    /// </summary>
    public event NamedListItemEventHandler<T> AfterAdd;

    /// <summary>
    /// Вызывает событие AfterAdd, если нет непарного BeginUpdate().
    /// </summary>
    /// <param name="item">Добавленный элемент</param>
    protected virtual void OnAfterAdd(T item)
    {
      if (_UpdateCount == 0 && AfterAdd != null)
      {
        NamedListItemEventArgs<T> Args = new NamedListItemEventArgs<T>(item);
        AfterAdd(this, Args);
      }
    }

    /// <summary>
    /// Событие вызывается перед удалением элемента.
    /// Если обработчик события выбросит исключение, список останется в неизменном и согласованном состоянии.
    /// Событие не вызывается, если есть непарный вызов BeginUpdate()
    /// </summary>
    public event NamedListItemEventHandler<T> BeforeRemove;

    /// <summary>
    /// Вызывает событие BeforeRemove, если нет непарного BeginUpdate().
    /// </summary>
    /// <param name="item">Удаляемый элемент</param>
    protected virtual void OnBeforeRemove(T item)
    {
      if (_UpdateCount == 0 && BeforeRemove != null)
      {
        NamedListItemEventArgs<T> Args = new NamedListItemEventArgs<T>(item);
        BeforeRemove(this, Args);
      }
    }

    /// <summary>
    /// Событие вызывается после удаления элемента.
    /// Если обработчик события выбросит исключение, список окажется в несогласованном состоянии и не может использоваться дальше.
    /// Событие не вызывается, если есть непарный вызов BeginUpdate()
    /// </summary>
    public event NamedListItemEventHandler<T> AfterRemove;

    /// <summary>
    /// Вызывает событие AfterRemove, если нет непарного BeginUpdate().
    /// </summary>
    /// <param name="item">Добавленный элемент</param>
    protected virtual void OnAfterRemove(T item)
    {
      if (_UpdateCount == 0 && AfterRemove != null)
      {
        NamedListItemEventArgs<T> Args = new NamedListItemEventArgs<T>(item);
        AfterRemove(this, Args);
      }
    }

    /// <summary>
    /// Событие вызывается при изменениях в списке.
    /// </summary>
    public event ListChangedEventHandler ListChanged;

    /// <summary>
    /// Вызывает событие ListChanged, если нет непарного вызова BeginUpdate()
    /// </summary>
    /// <param name="args">Аргументы события</param>
    protected void OnListChanged(ListChangedEventArgs args)
    {
#if DEBUG
      if (args == null)
        throw new ArgumentNullException("args");
#endif

      if (InsideOnListChanged)
        throw new ReenteranceException("Вложенный вызов ListChanged");

      if (_UpdateCount > 0)
        DelayedListChanged = true; // запомнили на будущее
      else if (ListChanged != null)
      {
        InsideOnListChanged = true;
        try
        {
          ListChanged(this, args);
        }
        finally
        {
          InsideOnListChanged = false;
        }
      }
    }

    private bool InsideOnListChanged;

    private void CallListChanged(ListChangedType listChangedType, int newIndex)
    {
      ListChangedEventArgs Args = new ListChangedEventArgs(listChangedType, newIndex);
      OnListChanged(Args);
    }

    /// <summary>
    /// Вызывает событие ListChanged с ListChangedType=ItemChanged
    /// </summary>
    /// <param name="index">Индекс элемента. Должен быть в диапазоне от 0 до (Count-1)</param>
    public void NotifyItemChanged(int index)
    {
      if (index < 0 || index >= Count)
        throw new ArgumentOutOfRangeException("index", index, "Индекс элемента вне диапазона");

      CallListChanged(ListChangedType.ItemChanged, index);
    }

    #endregion

    #region Приостановка отправки извещений

    /// <summary>
    /// После вызова метода перестают посылаться извещения BeforeAdd, AfterAdd, BeforeRemove и AfterRemove.
    /// Должен обязательно завершаться парным вызовом EndUpdate().
    /// </summary>
    public virtual void BeginUpdate()
    {
      if (_UpdateCount == 0)
        DelayedListChanged = false; // по идее, никогда не должно срабатывать
      _UpdateCount++;
    }

    /// <summary>
    /// Окончание обновления списка.
    /// Вызов должен быть парным, по отношению к BeginUpdate()
    /// </summary>
    public virtual void EndUpdate()
    {
      if (_UpdateCount <= 0)
        throw new InvalidOperationException("Непарный вызов EndUpdate()");

      _UpdateCount--;
      if (_UpdateCount == 0 && DelayedListChanged)
      {
        CallListChanged(ListChangedType.Reset, -1);
        DelayedListChanged = false;
      }
    }

    /// <summary>
    /// Возвращает true, если был непарный вызов метода BeginUpdate
    /// </summary>
    public bool IsUpdating { get { return _UpdateCount > 0; } }
    private int _UpdateCount;

    /// <summary>
    /// Устанавливается в true при любых изменениях в списке, если был непарный вызов BeginUpdate().
    /// В этом случае, метод EndUpdate() отправляет сигнал о полном обновлении списка
    /// </summary>
    private bool DelayedListChanged;

    #endregion

    #region Десериализация

    [OnDeserialized]
    private void OnDeserializedMethod(StreamingContext context)
    {
      _Dict = new Dictionary<string, int>(_List.Count);
      for (int i = 0; i < _List.Count; i++)
      {
        string ItemCode = _List[i].Code;
        if (IgnoreCase)
          ItemCode = ItemCode.ToUpperInvariant();

        _Dict.Add(ItemCode, i);
      }
      _DictIsValid = true;
    }

    #endregion

    #region INamedValuesAccess Members

    object INamedValuesAccess.GetValue(string name)
    {
      return this[name];
    }

    string[] INamedValuesAccess.GetNames()
    {
      return GetCodes();
    }

    #endregion
  }

#endif

  /// <summary>
  /// Коллекция объектов, поддерживающих интерфейс IObjectWithCode.
  /// В отличие от NamedList, порядок элементов является неопределенным.
  /// Существует доступ только по коду, но не по индексу.
  /// В отличие от стандартной коллекции Dictionary, перебор выполняется не по KeyValuePair,
  /// а непосредственно по объектам.
  /// </summary>
  /// <typeparam name="T">Тип объектов, хранящихся в списке, поддерживающих интерфейс IObjectWithCode</typeparam>
  [Serializable]
  public class NamedCollection<T> : IEnumerable<T>, ICollection<T>, IReadOnlyObject, INamedValuesAccess
    where T : class, IObjectWithCode
  {
    #region Конструкторы

    /// <summary>
    /// Создает пустую коллекцию.
    /// Регистр ключа учитывается
    /// </summary>
    public NamedCollection()
      : this(false)
    {
    }

    /// <summary>
    /// Создает пустую коллекцию
    /// </summary>
    /// <param name="ignoreCase">Надо ли игнорировать регистр кода</param>
    public NamedCollection(bool ignoreCase)
    {
      _Dict = new Dictionary<string, T>();
      _IgnoreCase = ignoreCase;
    }

    /// <summary>
    /// Создает пустую коллекцию.
    /// Регистр ключа учитывается
    /// </summary>
    /// <param name="capacity">Начальная емкость коллекции</param>
    public NamedCollection(int capacity)
      : this(capacity, false)
    {
    }

    /// <summary>
    /// Создает пустую коллекцию
    /// </summary>
    /// <param name="capacity">Начальная емкость коллекции</param>
    /// <param name="ignoreCase">Надо ли игнорировать регистр кода</param>
    public NamedCollection(int capacity, bool ignoreCase)
    {
      _Dict = new Dictionary<string, T>(capacity);
      _IgnoreCase = ignoreCase;
    }


    /// <summary>
    /// Создает коллекцию и заполняет ее значениями из другой коллекции.
    /// Регистр ключа учитывается.
    /// </summary>
    /// <param name="srcDictionary">Исходная коллекция, откуда берутся значения. Коллекция должна иметь ключ по коду</param>
    public NamedCollection(IDictionary<string, T> srcDictionary)
    {
      _Dict = new Dictionary<string, T>(srcDictionary);
    }

    /// <summary>
    /// Создает коллекцию и заполняет ее значениями из списка.
    /// Регистр ключа учитывается
    /// </summary>
    /// <param name="srcCollection">Исходная коллекция</param>
    public NamedCollection(ICollection<T> srcCollection)
      : this(srcCollection, false)
    {
    }

    /// <summary>
    /// Создает коллекцию и заполняет ее значениями из списка
    /// </summary>
    /// <param name="srcCollection">Исходная коллекция</param>
    /// <param name="ignoreCase">Надо ли игнорировать регистр кода</param>
    public NamedCollection(ICollection<T> srcCollection, bool ignoreCase)
      : this(srcCollection.Count, ignoreCase)
    {
      foreach (T Item in srcCollection)
        Add(Item);
    }


    /// <summary>
    /// Создает коллекцию и заполняет ее значениями из списка.
    /// Регистр ключа учитывается
    /// </summary>
    /// <param name="srcCollection">Исходная коллекция</param>
    public NamedCollection(IEnumerable<T> srcCollection)
      : this(srcCollection, false)
    {
    }

    /// <summary>
    /// Создает коллекцию и заполняет ее значениями из списка
    /// </summary>
    /// <param name="srcCollection">Исходная коллекция</param>
    /// <param name="ignoreCase">Надо ли игнорировать регистр кода</param>
    public NamedCollection(IEnumerable<T> srcCollection, bool ignoreCase)
      : this(ignoreCase)
    {
      foreach (T Item in srcCollection)
        Add(Item);
    }

    #endregion

    #region Доступ к элементам

    /// <summary>
    /// Коллекция по кодам.
    /// Если IgnoreCase=true, то ключ преобразован к верхнему регистру
    /// </summary>
    private readonly Dictionary<string, T> _Dict;

    /// <summary>
    /// Доступ по коду.
    /// Если запрошен несуществуюший код, возвращается пустой элемент
    /// </summary>
    /// <param name="code">Код элемента</param>
    /// <returns>Найденный элемент или пустое значение</returns>
    public T this[string code]
    {
      get
      {
        T res;
        if (string.IsNullOrEmpty(code))
          return default(T);

        if (IgnoreCase)
          code = code.ToUpperInvariant();

        if (_Dict.TryGetValue(code, out res))
          return res;
        else
          return default(T);
      }
    }

    /// <summary>
    /// Доступ по коду.
    /// В отличие от доступа по индексированному свойству, если запрошен несуществуюший код, генерируется исключение
    /// </summary>
    /// <param name="code">Код элемента</param>
    /// <returns>Найденный элемент или пустое значение</returns>
    public T GetRequired(string code)
    {
      T res;
      if (string.IsNullOrEmpty(code))
        throw new ArgumentNullException("code");

      if (IgnoreCase)
        code = code.ToUpperInvariant();

      if (_Dict.TryGetValue(code, out res))
        return res;
      else
        throw new ArgumentException("В коллекции нет элемента с кодом \"" + code + "\"", "code");
    }

    /// <summary>
    /// Возвращает количество элементов в коллекции
    /// </summary>
    public int Count { get { return _Dict.Count; } }

    /// <summary>
    /// Текстовое представление "Count=XXX"
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return "Count=" + Count.ToString();
    }

    #endregion

    #region IgnoreCase

    /// <summary>
    /// Если установлено в true, то при поиске элементов будет игнорироваться регистр.
    /// Если свойство установлено в false (по умолчанию), то регистр символов отличается.
    /// Свойство устанавливается в конструкторе.
    /// </summary>
    public bool IgnoreCase { get { return _IgnoreCase; } }
    private readonly bool _IgnoreCase;

    #endregion

    #region Доступ Только для чтения

    /// <summary>
    /// Возвращает true, если изменение коллекции запрещено
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// Заблокировать коллекцию от изменений
    /// </summary>
    internal protected void SetReadOnly()
    {
      _IsReadOnly = true;
    }

    /// <summary>
    /// Генерирует исключение ObjectReadOnlyException, если коллекция не может быть модифицирована
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (_IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    #endregion

    #region IEnumerable<T> Members

    /// <summary>
    /// Возвращает перечислитель по объектам коллекции.
    /// 
    /// Тип возвращаемого значения может измениться в будущем, 
    /// гарантируется только реализация интерфейса перечислителя.
    /// Поэтому в прикладном коде метод должен использоваться исключительно для использования в операторе foreach.
    /// </summary>
    /// <returns>Перечислитель</returns>
    public Dictionary<string, T>.ValueCollection.Enumerator GetEnumerator()
    {
      return _Dict.Values.GetEnumerator();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return _Dict.Values.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return _Dict.Values.GetEnumerator();
    }

    #endregion

    #region ICollection<T> Members

    /// <summary>
    /// Добавляет элемент в коллекцию.
    /// Если в коллекции уже есть элемент с таким кодом (с учетом IgnoreCase), генерируется исключение.
    /// </summary>
    /// <param name="item">Добавляемый элемент</param>
    public void Add(T item)
    {
      CheckNotReadOnly();

      string ItemCode = item.Code;
      if (IgnoreCase)
        ItemCode = ItemCode.ToUpperInvariant();

      _Dict.Add(ItemCode, item);
    }

    /// <summary>
    /// Очищает коллекцию
    /// </summary>
    public void Clear()
    {
      CheckNotReadOnly();

      _Dict.Clear();
    }

    /// <summary>
    /// Возвращает true, если элемент есть в коллекции.
    /// Рекомендуется использовать метод, принимающий строковый код
    /// </summary>
    /// <param name="item">Элемент, наличие которого проверяется</param>
    /// <returns></returns>
    public bool Contains(T item)
    {
      string ItemCode = item.Code;
      if (IgnoreCase)
        ItemCode = ItemCode.ToUpperInvariant();
      //return FDict.ContainsKey(ItemCode);

      // 22.02.2018. Дополнительно проверяем на равенство
      T ResItem;
      if (_Dict.TryGetValue(ItemCode, out ResItem))
        return item.Equals(ResItem);
      else
        return false;
    }

    /// <summary>
    /// Копирует коллекцию в массив
    /// </summary>
    /// <param name="array">Заполняемый массив</param>
    public void CopyTo(T[] array)
    {
      _Dict.Values.CopyTo(array, 0);
    }

    /// <summary>
    /// Копирует коллекцию в массив
    /// </summary>
    /// <param name="array">Заполняемый массив</param>
    /// <param name="arrayIndex">Начальный индекс в заполняемом массиве</param>
    public void CopyTo(T[] array, int arrayIndex)
    {
      _Dict.Values.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Удаляет элемент из коллекции
    /// </summary>
    /// <param name="item">Удаляемый элемент</param>
    /// <returns>true, если элемент был найден и удален</returns>
    public bool Remove(T item)
    {
      CheckNotReadOnly();

      string ItemCode = item.Code;
      if (IgnoreCase)
        ItemCode = ItemCode.ToUpperInvariant();

      return _Dict.Remove(ItemCode);
    }

    #endregion

    #region Дополнительные методы

    /// <summary>
    /// Быстрый поиск.
    /// Возвращает true, если элемент с заданным кодом (с учетом IgnoreCase) есть в коллекции.
    /// Это - рекомендуемый метод поиска.
    /// </summary>
    /// <param name="code">Проверяемый код</param>
    /// <returns>Наличие элемента с кодом</returns>
    public bool Contains(string code)
    {
      if (String.IsNullOrEmpty(code))
        return false;

      if (IgnoreCase)
        code = code.ToUpperInvariant();

      return _Dict.ContainsKey(code);
    }

    /// <summary>
    /// Быстрый поиск.
    /// Возвращает true, если элемент с заданным кодом (с учетом IgnoreCase) есть в коллекции.
    /// Идентичен обращению к свойству this, но позволяет отличать ситуацию наличия или отсутствия элемента.
    /// </summary>
    /// <param name="code">Проверяемый код</param>
    /// <param name="value">Сюда помещается найденное значение</param>
    /// <returns>Наличие элемента с кодом</returns>
    public bool TryGetValue(string code, out T value)
    {
      if (String.IsNullOrEmpty(code))
      {
        value = default(T);
        return false;
      }

      if (IgnoreCase)
        code = code.ToUpperInvariant();

      return _Dict.TryGetValue(code, out value);
    }

    /// <summary>
    /// Создает массив с элементами из коллекции
    /// </summary>
    /// <returns>Новый массив</returns>
    public T[] ToArray()
    {
      T[] a = new T[_Dict.Count];
      _Dict.Values.CopyTo(a, 0);
      return a;
    }

    /// <summary>
    /// Добавляет элементы из списка.
    /// Эквивалентно поштучному вызову метода Add()
    /// </summary>
    /// <param name="collection">Исходный список элементов</param>
    public void AddRange(IEnumerable<T> collection)
    {
      CheckNotReadOnly();
#if DEBUG
      if (collection == null)
        throw new ArgumentException("collection");
#endif
      if (Object.ReferenceEquals(collection, this))
        throw new ArgumentException("Нельзя добавить элементы из самого себя", "collection");

      foreach (T Item in collection)
        Add(Item);
    }

    /// <summary>
    /// Удаляет элемент с заданным кодом
    /// </summary>
    /// <param name="code">Код удаляемого элемента</param>
    /// <returns>true, если элемент был найден и удален</returns>
    public bool Remove(string code)
    {
      CheckNotReadOnly();

      if (String.IsNullOrEmpty(code))
        return false;
      if (IgnoreCase)
        code = code.ToUpperInvariant();

      return _Dict.Remove(code);
    }

    /// <summary>
    /// Возвращает строковый массив с кодами элементов
    /// Регистр символов не меняется, даже если IgnoreCase=true
    /// </summary>
    /// <returns>Массив кодов</returns>
    public string[] GetCodes()
    {
      string[] a = new string[_Dict.Count];
      int cnt = 0;
      foreach (T Item in _Dict.Values)
      {
        a[cnt] = Item.Code;
        cnt++;
      }

      return a;
    }

    #endregion

    #region INamedValuesAccess Members

    object INamedValuesAccess.GetValue(string name)
    {
      return this[name];
    }

    string[] INamedValuesAccess.GetNames()
    {
      return GetCodes();
    }

    #endregion
  }

  /// <summary>
  /// Потокобезопасная реализация NamedList, включая методы записи. 
  /// Если NamedList уже переведен в режим ReadOnly, можно использовать оригинальный класс, т.к. в этом случае он
  /// сам является потокобезопасным.
  /// </summary>
  /// <typeparam name="T">Тип объектов, хранящихся в списке, поддерживающих интерфейс IObjectWithCode</typeparam>
  [Obsolete("Так как доступ по индексу все равно невозможен, лучше использовать класс SynNamedCollection", false)]
  public class SyncNamedList<T> : SyncCollection<T>, IReadOnlyObject, INamedValuesAccess
    where T : class, IObjectWithCode
  {
    #region Конструкторы

    /// <summary>
    /// Создание синхронизированной оболочки над существующим списком
    /// </summary>
    /// <param name="source">Обертываемый список</param>
    public SyncNamedList(NamedList<T> source)
      : base(source)
    {
    }


    /// <summary>
    /// Создает пустой NamedList и обертку для него.
    /// Регистр символов ключа учитывается
    /// </summary>
    public SyncNamedList()
      : this(new NamedList<T>())
    {
    }

    /// <summary>
    /// Создает пустой NamedList и обертку для него.
    /// </summary>
    /// <param name="ignoreCase">Надо ли игнорировать регистр кода</param>
    public SyncNamedList(bool ignoreCase)
      : this(new NamedList<T>(ignoreCase))
    {
    }


    /// <summary>
    /// Создает пустой NamedList и обертку для него.
    /// Регистр символов ключа учитывается
    /// </summary>
    /// <param name="capacity">Начальная емкость списка NamedList</param>
    public SyncNamedList(int capacity)
      : this(new NamedList<T>(capacity))
    {
    }

    /// <summary>
    /// Создает пустой NamedList и обертку для него.
    /// </summary>
    /// <param name="capacity">Начальная емкость списка NamedList</param>
    /// <param name="ignoreCase">Надо ли игнорировать регистр кода</param>
    public SyncNamedList(int capacity, bool ignoreCase)
      : this(new NamedList<T>(capacity, ignoreCase))
    {
    }

    /// <summary>
    /// Создает NamedList и заполняет его элементами из коллекции.
    /// Регистр символов ключа учитывается
    /// </summary>
    /// <param name="srcDictionary">Исходная коллекция элементов. Должна иметь ключ по коду</param>
    public SyncNamedList(IDictionary<string, T> srcDictionary)
      : this(new NamedList<T>(srcDictionary))
    {
    }


    /// <summary>
    /// Создает NamedList и заполняет его элементами из списка.
    /// Регистр символов ключа учитывается
    /// </summary>
    /// <param name="srcCollection">Исходная коллекция элементов. Должна иметь ключ по коду</param>
    public SyncNamedList(ICollection<T> srcCollection)
      : this(new NamedList<T>(srcCollection))
    {
    }

    /// <summary>
    /// Создает NamedList и заполняет его элементами из списка.
    /// </summary>
    /// <param name="srcCollection">Исходная коллекция элементов. Должна иметь ключ по коду</param>
    /// <param name="ignoreCase">Надо ли игнорировать регистр кода</param>
    public SyncNamedList(ICollection<T> srcCollection, bool ignoreCase)
      : this(new NamedList<T>(srcCollection, ignoreCase))
    {
    }


    /// <summary>
    /// Создает NamedList и заполняет его элементами из списка.
    /// Регистр символов ключа учитывается
    /// </summary>
    /// <param name="srcCollection">Исходная коллекция элементов. Должна иметь ключ по коду</param>
    public SyncNamedList(IEnumerable<T> srcCollection)
      : this(new NamedList<T>(srcCollection))
    {
    }

    /// <summary>
    /// Создает NamedList и заполняет его элементами из списка.
    /// </summary>
    /// <param name="srcCollection">Исходная коллекция элементов. Должна иметь ключ по коду</param>
    /// <param name="ignoreCase">Надо ли игнорировать регистр кода</param>
    public SyncNamedList(IEnumerable<T> srcCollection, bool ignoreCase)
      : this(new NamedList<T>(srcCollection, ignoreCase))
    {
    }

    #endregion

    #region Доступ к элементам

    /// <summary>
    /// Основной объект данных
    /// </summary>
    protected new NamedList<T> Source { get { return (NamedList<T>)(base.Source); } }

    /// <summary>
    /// Доступ по коду.
    /// Если запрошен несуществуюший код, возвращается пустой элемент
    /// </summary>
    /// <param name="code">Код элемента</param>
    /// <returns>Найденный элемент или пустое значение</returns>
    public T this[string code]
    {
      get
      {
        lock (SyncRoot)
        {
          return Source[code];
        }
      }
    }

    /// <summary>
    /// Доступ по коду.
    /// В отличие от доступа по индексированному свойству, если запрошен несуществуюший код, генерируется исключение
    /// </summary>
    /// <param name="code">Код элемента</param>
    /// <returns>Найденный элемент</returns>
    public T GetRequired(string code)
    {
      lock (SyncRoot)
      {
        return Source.GetRequired(code);
      }
    }

    #endregion

    #region IgnoreCase

    /// <summary>
    /// Возвращает true, если регистр кода не учитывается при поиске элементов
    /// </summary>
    public bool IgnoreCase { get { return Source.IgnoreCase; } }

    #endregion

    #region Доступ Только для чтения

    /// <summary>
    /// Возвращает true, если список находится в состоянии "только чтение"
    /// </summary>
    public new bool IsReadOnly
    {
      get
      {
        lock (SyncRoot)
        {
          return Source.IsReadOnly;
        }
      }
    }

    /// <summary>
    /// Защищенный метод для установки состояния "только чтение"
    /// </summary>
    protected void SetReadOnly()
    {
      lock (SyncRoot)
      {
        Source.SetReadOnly();
      }
    }

    /// <summary>
    /// Генерирует исключение, если список находится в состоянии "только чтение"
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    #endregion

    #region Дополнительные методы

    /// <summary>
    /// Быстрый поиск.
    /// Возвращает true, если элемент с таким кодом есть в списке
    /// </summary>
    /// <param name="code">Искомый код</param>
    /// <returns>Наличие элемента в списке</returns>
    public bool Contains(string code)
    {
      lock (SyncRoot)
      {
        return Source.Contains(code);
      }
    }

    /// <summary>
    /// Быстрый поиск.
    /// Возвращает true, если элемент с таким кодом есть в списке.
    /// </summary>
    /// <param name="code">Искомый код</param>
    /// <param name="value">Сюда помещается найденное значение</param>
    /// <returns>Наличие элемента в списке</returns>
    public bool TryGetValue(string code, out T value)
    {
      lock (SyncRoot)
      {
        return Source.TryGetValue(code, out value);
      }
    }

    /// <summary>
    /// Добавляет элементы из другого списка
    /// </summary>
    /// <param name="collection">Исходные элементы</param>
    public new void AddRange(IEnumerable<T> collection)
    {
#if DEBUG
      if (collection == null)
        throw new ArgumentException("collection");
#endif
      if (Object.ReferenceEquals(collection, this))
        throw new ArgumentException("Нельзя добавить элементы из самого себя", "collection");
      if (Object.ReferenceEquals(collection, Source))
        throw new ArgumentException("Нельзя добавить элементы из базового списка", "collection");

      lock (SyncRoot)
      {
        ResetCopyArray();
        Source.AddRange(collection);
      }
    }

    /// <summary>
    /// Возвращает массив всех кодов, которые есть в списке.
    /// </summary>
    /// <returns>Массив кодов</returns>
    public string[] GetCodes()
    {
      lock (SyncRoot)
      {
        return Source.GetCodes();
      }
    }

    /// <summary>
    /// Сортировка списка строк.
    /// При сортировке регистр символов учитывается или игнорируется, в зависимости от свойства IgnoreCase
    /// </summary>
    public void Sort()
    {
      lock (SyncRoot)
      {
        ResetCopyArray();
        Source.Sort();
      }
    }

    /// <summary>
    /// Заменяет порядок элементов на обратный
    /// </summary>
    public void Reverse()
    {
      lock (SyncRoot)
      {
        ResetCopyArray();
        Source.Reverse();
      }
    }


    #endregion

    #region INamedValuesAccess Members

    object INamedValuesAccess.GetValue(string name)
    {
      return this[name];
    }

    string[] INamedValuesAccess.GetNames()
    {
      return GetCodes();
    }

    #endregion
  }

  /// <summary>
  /// Потокобезопасная реализация NamedCollection, включая методы записи. 
  /// Если объект NamedCollection уже переведен в режим ReadOnly, можно использовать оригинальный класс, т.к. в этом случае он
  /// сам является потокобезопасным.
  /// Список объектов произвольного типа, доступ к которым может осуществляться по имени, как в Dictionary с 
  /// ключом String
  /// </summary>
  /// <typeparam name="T">Тип объектов, хранящихся в списке, поддерживающих интерфейс IObjectWithCode</typeparam>
  public class SyncNamedCollection<T> : SyncCollection<T>, IReadOnlyObject, INamedValuesAccess
    where T : class, IObjectWithCode
  {
    #region Конструкторы

    /// <summary>
    /// Создание синхронизированной оболочки над существующим списком.
    /// </summary>
    /// <param name="source">Базовый список</param>
    public SyncNamedCollection(NamedCollection<T> source)
      : base(source)
    {
    }

    /// <summary>
    /// Создает пустую коллекцию.
    /// Регистр символов кода учитывается.
    /// </summary>
    public SyncNamedCollection()
      : this(new NamedCollection<T>())
    {
    }

    /// <summary>
    /// Создает пустую коллекцию.
    /// </summary>
    /// <param name="ignoreCase">Надо ли игнорировать регистр кода</param>
    public SyncNamedCollection(bool ignoreCase)
      : this(new NamedCollection<T>(ignoreCase))
    {
    }


    /// <summary>
    /// Создает пустую коллекцию.
    /// Регистр символов кода учитывается.
    /// </summary>
    /// <param name="capacity">Начальная емкость коллекции</param>
    public SyncNamedCollection(int capacity)
      : this(new NamedCollection<T>(capacity))
    {
    }

    /// <summary>
    /// Создает пустую коллекцию.
    /// </summary>
    /// <param name="capacity">Начальная емкость коллекции</param>
    /// <param name="ignoreCase">Надо ли игнорировать регистр кода</param>
    public SyncNamedCollection(int capacity, bool ignoreCase)
      : this(new NamedCollection<T>(capacity, ignoreCase))
    {
    }

    /// <summary>
    /// Создает коллекцию и заполняет ее элементами из другой коллекции.
    /// Регистр символов кода учитывается.
    /// Нет парного конструктора с IgnoreCase
    /// </summary>
    /// <param name="srcDictionary">Исходная коллекция. Должна иметь ключ по коду элементов</param>
    public SyncNamedCollection(IDictionary<string, T> srcDictionary)
      : this(new NamedCollection<T>(srcDictionary))
    {
    }


    /// <summary>
    /// Создает коллекцию и заполняет ее элементами из другого списка.
    /// Регистр символов кода учитывается.
    /// </summary>
    /// <param name="srcCollection">Исходная коллекция. Должна иметь ключ по коду элементов</param>
    public SyncNamedCollection(ICollection<T> srcCollection)
      : this(new NamedCollection<T>(srcCollection))
    {
    }

    /// <summary>
    /// Создает коллекцию и заполняет ее элементами из другого списка.
    /// </summary>
    /// <param name="srcCollection">Исходная коллекция. Должна иметь ключ по коду элементов</param>
    /// <param name="ignoreCase">Надо ли игнорировать регистр кода</param>
    public SyncNamedCollection(ICollection<T> srcCollection, bool ignoreCase)
      : this(new NamedCollection<T>(srcCollection, ignoreCase))
    {
    }


    /// <summary>
    /// Создает коллекцию и заполняет ее элементами из другого списка.
    /// Регистр символов кода учитывается.
    /// </summary>
    /// <param name="srcCollection">Исходная коллекция. Должна иметь ключ по коду элементов</param>
    public SyncNamedCollection(IEnumerable<T> srcCollection)
      : this(new NamedCollection<T>(srcCollection))
    {
    }

    /// <summary>
    /// Создает коллекцию и заполняет ее элементами из другого списка.
    /// </summary>
    /// <param name="srcCollection">Исходная коллекция. Должна иметь ключ по коду элементов</param>
    /// <param name="ignoreCase">Надо ли игнорировать регистр кода</param>
    public SyncNamedCollection(IEnumerable<T> srcCollection, bool ignoreCase)
      : this(new NamedCollection<T>(srcCollection, ignoreCase))
    {
    }

    #endregion

    #region Доступ к элементам

    /// <summary>
    /// Основной объект данных
    /// </summary>
    protected new NamedCollection<T> Source { get { return (NamedCollection<T>)(base.Source); } }

    /// <summary>
    /// Доступ по коду.
    /// Если запрошен несуществуюший код, возвращается пустой элемент
    /// </summary>
    /// <param name="code">Код элемента</param>
    /// <returns>Найденный элемент или пустое значение</returns>
    public T this[string code]
    {
      get
      {
        lock (SyncRoot)
        {
          return Source[code];
        }
      }
    }

    /// <summary>
    /// Доступ по коду.
    /// В отличие от доступа по индексированному свойству, если запрошен несуществуюший код, генерируется исключение
    /// </summary>
    /// <param name="code">Код элемента</param>
    /// <returns>Найденный элемент</returns>
    public T GetRequired(string code)
    {
      lock (SyncRoot)
      {
        return Source.GetRequired(code);
      }
    }


    #endregion

    #region IgnoreCase

    /// <summary>
    /// Возвращает true, если регистр символов игнорируется при поиске по имени.
    /// False, если учитывается. Задается в конструкторе
    /// </summary>
    public bool IgnoreCase { get { return Source.IgnoreCase; } }

    #endregion

    #region Доступ Только для чтения

    /// <summary>
    /// Возвращает true, если коллекция наъодится в режиме "только чтение"
    /// </summary>
    public new bool IsReadOnly
    {
      get
      {
        lock (SyncRoot)
        {
          return Source.IsReadOnly;
        }
      }
    }

    /// <summary>
    /// Защищенный метод для перевода коллекции в режим "только чтение"
    /// </summary>
    protected void SetReadOnly()
    {
      lock (SyncRoot)
      {
        Source.SetReadOnly();
      }
    }

    /// <summary>
    /// Генерирует исключение, если коллекция находится в режиме "только чтение"
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    #endregion

    #region Дополнительные методы

    /// <summary>
    /// Быстрый поиск.
    /// Возвращает true, если коллекция содержит элемент с таким кодом (с учетом IgnoreCase).
    /// Рекомендуемый метод поиска.
    /// </summary>
    /// <param name="code">Проверяемый код</param>
    /// <returns>Наличие элемента</returns>
    public bool Contains(string code)
    {
      lock (SyncRoot)
      {
        return Source.Contains(code);
      }
    }

    /// <summary>
    /// Быстрый поиск.
    /// Возвращает true, если коллекция содержит элемент с таким кодом (с учетом IgnoreCase).
    /// Рекомендуемый метод поиска.
    /// </summary>
    /// <param name="code">Проверяемый код</param>
    /// <param name="value">Сюда помещается найденное значение</param>
    /// <returns>Наличие элемента</returns>
    public bool TryGetValue(string code, out T value)
    {
      lock (SyncRoot)
      {
        return Source.TryGetValue(code, out value);
      }
    }

    /// <summary>
    /// Добавляет элементы из другого списка.
    /// </summary>
    /// <param name="collection">Список элементов для добавления</param>
    public new void AddRange(IEnumerable<T> collection)
    {
#if DEBUG
      if (collection == null)
        throw new ArgumentException("collection");
#endif
      if (Object.ReferenceEquals(collection, this))
        throw new ArgumentException("Нельзя добавить элементы из самого себя", "collection");
      if (Object.ReferenceEquals(collection, Source))
        throw new ArgumentException("Нельзя добавить элементы из базового списка", "collection");

      lock (SyncRoot)
      {
        ResetCopyArray();
        Source.AddRange(collection);
      }
    }

    /// <summary>
    /// Возвращает массив всех кодов в коллекции.
    /// </summary>
    /// <returns>Массив кодов</returns>
    public string[] GetCodes()
    {
      lock (SyncRoot)
      {
        return Source.GetCodes();
      }
    }

    #endregion

    #region INamedValuesAccess Members

    object INamedValuesAccess.GetValue(string name)
    {
      return this[name];
    }

    string[] INamedValuesAccess.GetNames()
    {
      return GetCodes();
    }

    #endregion
  }
}
