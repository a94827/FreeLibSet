using System;
using System.Collections.Generic;
using System.Text;
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

namespace FreeLibSet.Collections
{
  /// <summary>
  /// Структура для хранения списка истории (массива строк).
  /// Структура является "одноразовой", как DateTime, то есть любое измемение
  /// приводит к созданию нового объекта, а не к изменению существующего.
  /// </summary>
  /// <remarks>
  /// Объект не реализует интерфейс ICollection, т.к. изменение списка приводит к созданию нового объекта.
  /// Интерфейс ICloneable также не реализуется, т.к. всегда можно использовать один и тот же объект
  /// </remarks>
  [Serializable]
  public struct HistoryList : IEnumerable<string>
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
    /// Длина списка ограничивается DefaultMaxHistLength элементами.
    /// Если Item - пустая строка, возвращается текущий список без изменений.
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
    /// Если Item - пустая строка, возвращается текущий список без изменений.
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
    /// <returns>Масси строк</returns>
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

    #region IEnumerable<string> Members

    /// <summary>
    /// Возвращает перечислитель по строкам в списке.
    /// 
    /// Тип возвращаемого значения (ArrayEnumerator) может измениться в будущем, 
    /// гарантируется только реализация интерфейса перечислителя.
    /// Поэтому в прикладном коде метод должен использоваться исключительно для использования в операторе foreach.
    /// </summary>
    /// <returns>Перечислитель</returns>
    public FreeLibSet.Core.ArrayEnumerator<string> GetEnumerator()
    {
      if (_Items == null)
        return new ArrayEnumerator<string>(DataTools.EmptyStrings);
      else
        return new ArrayEnumerator<string>(_Items);
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

    #region Статические методы и свойства

    /// <summary>
    /// Объединение двух или более списков истории в один.
    /// Используйте метод, если в диалоге есть несколько одинаковых полей истории
    /// и история является общей. В этом случае, после нажатия кнопки ОК вызовите
    /// метод и сохраняйте общий список и свойство Text для каждого элемента. 
    /// При чтении присоединяйте спискок к каждому комбоблоку, в свойство Text 
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
      bool Flag = false;
      int Index = 0;
      do
      {
        Flag = false;

        for (int i = 0; i < histLists.Length; i++)
        {
          if (histLists[i].IsEmpty)
            continue;
          if (Index < histLists[i].Count)
          {
            Flag = true;
            string s = histLists[i][Index];
            if (!lst.Contains(s))
              lst.Add(s);
          }
        }

        Index++;
      } while (Flag);

      return new HistoryList(lst);
    }

    /// <summary>
    /// Пустой список
    /// </summary>
    public static readonly HistoryList Empty = new HistoryList();

    #endregion
  }
}
