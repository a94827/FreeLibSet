// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Core;
using System.Collections;

namespace FreeLibSet.Collections
{
  /// <summary>
  /// Структура для хранения списка истории (массива строк).
  /// Структура является "одноразовой", то есть любое измемение
  /// приводит к созданию нового объекта, а не к изменению существующего.
  /// </summary>
  /// <remarks>
  /// Объект реализует интерфейсы <see cref="IList"/> и <see cref="ICollection"/> в режиме "только чтение", чтобы можно было использовать его в качестве источника для DataGridView
  /// Интерфейс <see cref="ICloneable"/> не реализуется, т.к. всегда можно использовать один и тот же объект.
  /// </remarks>
  [Serializable]
  public struct HistoryList : IList<string>, IList
  {
    #region Константа

    /// <summary>
    /// Количество строк истории в списке по умолчанию.
    /// </summary>
    public const int DefaultMaxHistLength = 10;

    #endregion

    #region Конструкторы

    /// <summary>
    /// Создает новый объект с заданным списком строк.
    /// </summary>
    /// <param name="items">Исходный массив строк</param>
    public HistoryList(string[] items)
    {
      if (items == null)
        _Items = null;
      else
      {
        _Items = new string[items.Length];
        items.CopyTo(_Items, 0);
      }
    }

    /// <summary>
    /// Этот конструктор не выполняет копирование массива
    /// </summary>
    /// <param name="items"></param>
    /// <param name="dummy"></param>
    private HistoryList(string[] items, bool dummy)
    {
      _Items = items;
    }

    /// <summary>
    /// Создает новый объект с заданным списком строк.
    /// </summary>
    /// <param name="items">Исходный массив строк</param>
    public HistoryList(ICollection<string> items)
    {
      if (items == null)
        _Items = null;
      else
      {
        _Items = new string[items.Count];
        items.CopyTo(_Items, 0);
      }
    }

    /// <summary>
    /// Создает новый объект с одной строкой.
    /// Если задана пустая строка, создается пустой объект
    /// </summary>
    /// <param name="item">Строка</param>
    public HistoryList(string item)
    {
      if (String.IsNullOrEmpty(item))
        _Items = null;
      else
        _Items = new string[1] { item };
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Единственное поле данных.
    /// Если null, то список пустой
    /// </summary>
    private readonly string[] _Items;

    /// <summary>
    /// Возвращает строку по индексу
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public string this[int index] { get { return _Items[index]; } }

    /// <summary>
    /// Возвращает количество строк в списке
    /// </summary>
    public int Count
    {
      get
      {
        if (_Items == null)
          return 0;
        else
          return _Items.Length;
      }
    }

    /// <summary>
    /// Возвращает true, если список пустой
    /// </summary>
    public bool IsEmpty { get { return Count == 0; } }

    /// <summary>
    /// Возвращает верхнее значение или пустую строку, если список пустой
    /// </summary>
    public string Top
    {
      get
      {
        if (_Items == null)
          return String.Empty;
        if (_Items.Length == 0)
          return String.Empty;
        return _Items[0];
      }
    }

    /// <summary>
    /// текстовое представление в виде "Count=XXX" или "Empty"
    /// </summary>
    /// <returns>текстовое представление для отладки</returns>
    public override string ToString()
    {
      if (Count == 0)
        return "Empty";
      else
        return "Count=" + Count.ToString() + ", Top=" + Top;
    }

    #endregion

    #region Методы

    /// <summary>
    /// Создает новый список, в котором первая строка будет равна <paramref name="item"/>.
    /// Если строка уже есть в списке, она перемещается в первую позицию списка.
    /// Длина списка ограничивается <see cref="DefaultMaxHistLength"/> элементами.
    /// Если <paramref name="item"/> - пустая строка, возвращается текущий список без изменений.
    /// </summary>
    /// <param name="item">Добавляемая строка</param>
    /// <returns>Новый список</returns>
    public HistoryList Add(string item)
    {
      return Add(item, DefaultMaxHistLength);
    }

    /// <summary>
    /// Создает новый список, в котором первая строка будет равна <paramref name="item"/>.
    /// Если строка уже есть в списке, она перемещается в первую позицию списка.
    /// Длина списка ограничивается <paramref name="maxHistLength"/> элементами.
    /// Если <paramref name="item"/> - пустая строка, возвращается текущий список без изменений.
    /// </summary>
    /// <param name="item">Добавляемая строка</param>
    /// <param name="maxHistLength">Максимальное число строк в списке</param>
    /// <returns>Новый список</returns>
    public HistoryList Add(string item, int maxHistLength)
    {
      CheckMaxLength(maxHistLength);

      if (string.IsNullOrEmpty(item))
        return this;

      if (IsEmpty)
        return new HistoryList(item);

      int p = Array.IndexOf<string>(_Items, item);
      if (p < 0)
      {
        int n = Math.Min(_Items.Length + 1, maxHistLength);
        string[] a = new string[n];
        a[0] = item;
        for (int i = 1; i < n; i++)
          a[i] = _Items[i - 1];
        return new HistoryList(a, true);
      }
      else
      {
        int n = Math.Min(_Items.Length, maxHistLength);
        string[] a = new string[n];
        a[0] = item;
        int j = 0;
        for (int i = 1; i < n; i++)
        {
          if (j == p)
            j++;
          a[i] = _Items[j];
          j++;
        }
        return new HistoryList(a, true);
      }
    }

    private static void CheckMaxLength(int maxHistLength)
    {
      if (maxHistLength < 1)
        throw new ArgumentOutOfRangeException("maxHistLength", maxHistLength, "Длина списка истории не может быть меньше 1");
    }

    /// <summary>
    /// Ограничивает длину списка <paramref name="maxHistLength"/> элементами.
    /// Если текущая длина массива не превышает лимит, возвращается текущий список без изменений
    /// </summary>
    /// <param name="maxHistLength">Максимальное число строк в списке</param>
    /// <returns>Новый список</returns>
    public HistoryList SetLimit(int maxHistLength)
    {
      CheckMaxLength(maxHistLength);

      if (Count <= maxHistLength)
        return this;

      string[] a = new string[maxHistLength];
      Array.Copy(_Items, a, maxHistLength);
      return new HistoryList(a, true);
    }

    /// <summary>
    /// Возвращает true, если строка есть в списке
    /// </summary>
    /// <param name="item">Строка для поиска</param>
    /// <returns>true, если строка есть в списке</returns>
    public bool Contains(string item)
    {
      return IndexOf(item) >= 0;
    }

    /// <summary>
    /// Возвращает позицию строки в списке.
    /// Если строка не найдена, возвращается (-1)
    /// </summary>
    /// <param name="item">Строка для поиска</param>
    /// <returns>Индекс строки</returns>
    public int IndexOf(string item)
    {
      if (_Items == null)
        return -1;
      else
        return Array.IndexOf<string>(_Items, item);
    }


    /// <summary>
    /// Копирует элементы в массив строк
    /// </summary>
    /// <param name="array">Заполняемый массив строк</param>
    public void CopyTo(string[] array)
    {
      if (_Items != null)
        _Items.CopyTo(array, 0);
    }

    /// <summary>
    /// Копирует элементы в массив строк
    /// </summary>
    /// <param name="array">Заполняемый массив строк</param>
    /// <param name="arrayIndex">Индекс в массиве <paramref name="array"/>, начиная с которой записываются значения</param>
    public void CopyTo(string[] array, int arrayIndex)
    {
      if (_Items != null)
        _Items.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Копирует элементы в массив строк
    /// </summary>
    /// <param name="index">Индекс первого элемента в текущем списке, с которого начинать копирование</param>
    /// <param name="array">Заполняемый массив</param>
    /// <param name="arrayIndex">Начальный индекс в массиве для заполнения</param>
    /// <param name="count">Количество элементов, которые нужно скопировать</param>
    public void CopyTo(int index, string[] array, int arrayIndex, int count)
    {
      if (_Items != null)
      {
        for (int i = 0; i < count; i++)
          array[arrayIndex + i] = _Items[index + i];
      }
    }

    /// <summary>
    /// Удаляет элемент в заданной позиции
    /// </summary>
    /// <param name="index">Индекс удаляемой строки</param>
    /// <returns>Новый список</returns>
    public HistoryList RemoveAt(int index)
    {
      if (index < 0 || index >= Count)
        throw new ArgumentOutOfRangeException();

      if (_Items.Length == 1)
        return HistoryList.Empty;

      string[] a = new string[_Items.Length - 1];
      for (int i = 0; i < index; i++)
        a[i] = _Items[i];
      for (int i = index + 1; i < _Items.Length; i++)
        a[i - 1] = _Items[i];
      return new HistoryList(a, true);
    }

    /// <summary>
    /// Удаляет строку из списка.
    /// Если строки нет в списке, возвращается текущий объект без изменений
    /// </summary>
    /// <param name="item">Удаляемая строка</param>
    /// <returns>Новый список</returns>
    public HistoryList Remove(string item)
    {
      int p = IndexOf(item);
      if (p < 0)
        return this;
      else
        return RemoveAt(p);
    }

    /// <summary>
    /// Возвращает копию массива строк в виде списка
    /// </summary>
    /// <returns>Массив строк</returns>
    public string[] ToArray()
    {
      if (_Items == null)
        return DataTools.EmptyStrings;
      else
      {
        string[] a = new string[_Items.Length];
        _Items.CopyTo(a, 0);
        return a;
      }
    }

    #endregion

    #region IList<string> Members

    void IList<string>.Insert(int index, string item)
    {
      throw new NotImplementedException();
    }

    void IList<string>.RemoveAt(int index)
    {
      throw new NotImplementedException();
    }

    string IList<string>.this[int index]
    {
      get { return this[index]; }
      set
      {
        throw new NotImplementedException();
      }
    }

    #endregion

    #region ICollection<string> Members

    void ICollection<string>.Add(string item)
    {
      throw new NotImplementedException();
    }

    void ICollection<string>.Clear()
    {
      throw new NotImplementedException();
    }
    bool ICollection<string>.IsReadOnly
    {
      get { return true; }
    }

    bool ICollection<string>.Remove(string item)
    {
      throw new NotImplementedException();
    }

    #endregion

    #region IEnumerable<string> Members

    /// <summary>
    /// Возвращает перечислитель по строкам в списке.
    /// 
    /// Тип возвращаемого значения (<see cref="ArrayEnumerable{String}"/>) может измениться в будущем, 
    /// гарантируется только реализация интерфейса перечислителя.
    /// Поэтому в прикладном коде метод должен использоваться исключительно для использования в операторе foreach.
    /// </summary>
    /// <returns>Перечислитель</returns>
    public ArrayEnumerable<string>.Enumerator GetEnumerator()
    {
      if (_Items == null)
        return new ArrayEnumerable<string>.Enumerator(DataTools.EmptyStrings);
      else
        return new ArrayEnumerable<string>.Enumerator(_Items);
    }

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
    {
      return GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion

    #region IList Members

    int IList.Add(object value)
    {
      throw new NotImplementedException();
    }

    void IList.Clear()
    {
      throw new NotImplementedException();
    }

    bool IList.Contains(object value)
    {
      return Contains(value as string);
    }

    int IList.IndexOf(object value)
    {
      return IndexOf(value as string);
    }

    void IList.Insert(int index, object value)
    {
      throw new NotImplementedException();
    }

    bool IList.IsFixedSize
    {
      get { return true; }
    }

    bool IList.IsReadOnly
    {
      get { return true; }
    }

    void IList.Remove(object value)
    {
      throw new NotImplementedException();
    }

    void IList.RemoveAt(int index)
    {
      throw new NotImplementedException();
    }

    object IList.this[int index]
    {
      get
      {
        return this[index];
      }
      set
      {
        throw new NotImplementedException();
      }
    }

    #endregion

    #region ICollection Members

    void ICollection.CopyTo(Array array, int index)
    {
      if (!IsEmpty)
        ((ICollection)_Items).CopyTo(array, index);
    }

    bool ICollection.IsSynchronized
    {
      get { return false; }
    }

    object ICollection.SyncRoot
    {
      get { return this; }
    }

    #endregion

    #region Статические методы и свойства

    /// <summary>
    /// Объединение двух или более списков истории в один.
    /// Используйте метод, если в диалоге есть несколько одинаковых полей истории
    /// и история является общей. В этом случае, после нажатия кнопки ОК вызовите
    /// метод и сохраняйте общий список и свойство Text для каждого элемента. 
    /// При чтении присоединяйте список к каждому комбоблоку, в свойство Text 
    /// устанавливайте отдельно.
    /// Полученный общий список истории может быть длиннее, чем предолагалось,
    /// но при присоединении длинного списка к EFPHistComboBox будут использованы
    /// только первые MaxHistLength элементов
    /// </summary>
    /// <param name="histLists">Два или более списков истории</param>
    /// <returns></returns>
    public static HistoryList Merge(params HistoryList[] histLists)
    {
      List<string> lst = new List<string>();
      bool flag = false;
      int index = 0;
      do
      {
        flag = false;

        for (int i = 0; i < histLists.Length; i++)
        {
          if (histLists[i].IsEmpty)
            continue;
          if (index < histLists[i].Count)
          {
            flag = true;
            string s = histLists[i][index];
            if (!lst.Contains(s))
              lst.Add(s);
          }
        }

        index++;
      } while (flag);

      return new HistoryList(lst);
    }

    /// <summary>
    /// Пустой список
    /// </summary>
    public static readonly HistoryList Empty = new HistoryList();

    #endregion
  }
}
