// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace FreeLibSet.Core
{
  /// <summary>
  /// Реализует интерфейс IEnumerable для коллекции, не содержащей элементов
  /// </summary>
  /// <typeparam name="T"></typeparam>
  [Serializable]
  public sealed class DummyEnumerable<T> : IEnumerable<T>
  {
    /// <summary>
    /// Пустой перечислитель.
    /// Реализует интерфейс IEnumerator для коллекции, не содержащей элементов
    /// </summary>
    [Serializable]
    public struct Enumerator : IEnumerator<T>
    {
      #region IEnumerator<T> Members

      /// <summary>
      /// Возвращает пустое значение
      /// </summary>
      public T Current
      {
        get { return default(T); }
      }

      /// <summary>
      /// Ничего не делает
      /// </summary>
      public void Dispose()
      {
      }

      object IEnumerator.Current
      {
        get { return null; }
      }

      /// <summary>
      /// Всегда возвращает false
      /// </summary>
      /// <returns></returns>
      public bool MoveNext()
      {
        return false;
      }

      void IEnumerator.Reset()
      {
      }

      #endregion
    }

    #region IEnumerable<T> Members

    /// <summary>
    /// Возвращает фиктивный перечислитель
    /// </summary>
    /// <returns></returns>
    public Enumerator GetEnumerator()
    {
      return new Enumerator();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return new Enumerator();
    }


    IEnumerator IEnumerable.GetEnumerator()
    {
      return new Enumerator();
    }

    #endregion
  }


  /// <summary>
  /// "Перечислимый объект" для одного элемента.
  /// Не учитывает равенство объекта null, то есть, если объект равен null, то он все равно будет перечислен.
  /// Если требуется пустой перечислитель, используйте DummyEnumerable.
  /// </summary>
  /// <typeparam name="T">Тип перечислимого объекта</typeparam>
  [Serializable]
  public sealed class SingleObjectEnumerable<T> : IEnumerable<T>
  {
    #region Конструктор

    /// <summary>
    /// Создает псевдоперечислитель для одного объекта
    /// </summary>
    /// <param name="singleObject">Единственный объект</param>
    public SingleObjectEnumerable(T singleObject)
    {
      _Object = singleObject;
    }

    private T _Object;

    #endregion

    /// <summary>
    /// "Перечислитель" для одного элемента.
    /// </summary>
    [Serializable]
    public struct Enumerator : IEnumerator<T>
    {
      #region Конструктор

      /// <summary>
      /// Создает псевдоперечислитель для одного объекта
      /// </summary>
      /// <param name="singleObject">Единственный объект</param>
      public Enumerator(T singleObject)
      {
        _Object = singleObject;
        _Flag = false;
      }

      #endregion

      #region IEnumerator<T> Members

      private T _Object;
      private bool _Flag;

      /// <summary>
      /// Возвращает текущее значение
      /// </summary>
      public T Current { get { return _Object; } }

      /// <summary>
      /// Ничего не делает
      /// </summary>
      public void Dispose()
      {
      }

      object IEnumerator.Current
      {
        get { return _Object; }
      }

      /// <summary>
      /// Переход к следующему значению.
      /// При первом вызове возвращает true, при втором - false.
      /// </summary>
      /// <returns>Наличие значения</returns>
      public bool MoveNext()
      {
        if (_Flag)
          return false;
        else
        {
          _Flag = true;
          return true;
        }
      }

      void IEnumerator.Reset()
      {
        _Flag = false;
      }

      #endregion
    }

    #region IEnumerable<T> Members

    /// <summary>
    /// Создает новый перечислитель по объекту
    /// </summary>
    /// <returns></returns>
    public Enumerator GetEnumerator()
    {
      return new Enumerator(_Object);
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return new Enumerator(_Object);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return new Enumerator(_Object);
    }

    #endregion
  }

  /// <summary>
  /// Простейшая обертка над объектом, реализующим интерфейс IEnumerator.
  /// Реализует интерфейс IEnumerable, чтобы можно было применять оператор foreach.
  /// Структура является "одноразовой", так как повторный оператор foreach получил бы тот же экземпляр перечислителя.
  /// </summary>
  /// <typeparam name="T">Тип перечислителя</typeparam>
  [Serializable]
  public struct EnumerableWrapper<T> : IEnumerable<T>
  {
    #region Конструктор

    /// <summary>
    /// Инициализирует объект
    /// </summary>
    /// <param name="enumerator">Перечислитель. Не может быть null</param>
    public EnumerableWrapper(IEnumerator<T> enumerator)
    {
      if (enumerator == null)
        throw new ArgumentNullException("enumerator");
      _Enumerator = enumerator;
    }

    #endregion

    #region GetEnumerator()

    private IEnumerator<T> _Enumerator;

    /// <summary>
    /// Возвращает перечислитель, переданный конструктору.
    /// Повторный вызов вызывает исключение.
    /// </summary>
    /// <returns>Перечислитель</returns>
    public IEnumerator<T> GetEnumerator()
    {
      if (_Enumerator == null)
        throw new InvalidOperationException("Повторный вызов перечислителя не допускается");
      IEnumerator<T> res = _Enumerator;
      _Enumerator = null;
      return res;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion
  }

  /// <summary>
  /// Рекурсивный перечислитель, который вызывает другие перечислители
  /// Базовый класс.
  /// </summary>
  /// <typeparam name="T">Тип перечислимых объектов</typeparam>
  [Serializable]
  public abstract class GroupEnumerator<T> : IEnumerator<T>
  {
    #region Конструктор

    /// <summary>
    /// Конструктор базового класса
    /// </summary>
    public GroupEnumerator()
    {
      _GroupIndex = -1;
    }

    #endregion

    #region Поля

    private int _GroupIndex;

    private IEnumerator<T> _CurrentGroup;

    #endregion

    #region IEnumerator<T> Members

    /// <summary>
    /// Возвращает текущий объект
    /// </summary>
    public T Current
    {
      get
      {
        if (_CurrentGroup == null)
          return default(T);
        else
          return _CurrentGroup.Current;
      }
    }

    /// <summary>
    /// Вызывает Reset()
    /// </summary>
    public virtual void Dispose()
    {
      Reset();
    }

    object IEnumerator.Current
    {
      get { return Current; }
    }

    /// <summary>
    /// Переход к следующему значению
    /// </summary>
    /// <returns>Наличие значения</returns>
    public bool MoveNext()
    {
      while (true)
      {
        if (_CurrentGroup != null)
        {
          if (_CurrentGroup.MoveNext())
            return true;

          _CurrentGroup.Dispose();
          _CurrentGroup = null;
        }

        _GroupIndex++;
        _CurrentGroup = GetNextGroup(_GroupIndex);
        if (_CurrentGroup == null)
          return false;
      }
    }

    /// <summary>
    /// Этот метод должен быть переопределен в производном классе.
    /// Он должен вернуть очередной перечислитель или null, когда больше нет перечислителей.
    /// </summary>
    /// <param name="groupIndex">Индекс очередного перечислителя.
    /// Метод последовательно вызывается с индексом 0,1,2,...</param>
    /// <returns>Очередной перечислитель или null</returns>
    protected abstract IEnumerator<T> GetNextGroup(int groupIndex);

    /// <summary>
    /// Повторная инициализация перечислителя
    /// </summary>
    public void Reset()
    {
      if (_CurrentGroup != null)
      {
        _CurrentGroup.Dispose();
        _CurrentGroup = null;
        _GroupIndex = -1;
      }
    }

    #endregion
  }

  /// <summary>
  /// Рекурсивный перечислитель, который по очереди вызывает другие перечислители, заданные в массиве.
  /// Этот класс является менее полезным, чем GroupEnumerator, т.к. требует, чтобы дочерние перечислители
  /// были созданы заранее.
  /// </summary>
  /// <typeparam name="T">Тип перечислимых объектов</typeparam>
  [Serializable]
  public struct GroupArrayEnumerable<T> : IEnumerable<T>
  {
    #region Конструктор

    /// <summary>
    /// Создает перечислитель
    /// </summary>
    /// <param name="groups">Массив исходных перечислителей</param>
    public GroupArrayEnumerable(IEnumerable<T>[] groups)
    {
      if (groups == null)
        throw new ArgumentNullException("groups");
      _Groups = groups;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Массив групп
    /// </summary>
    public IEnumerable<T>[] Groups { get { return _Groups; } }
    private IEnumerable<T>[] _Groups;

    #endregion

    /// <summary>
    /// Рекурсивный перечислитель, который по очереди вызывает другие перечислители, заданные в массиве.
    /// Этот класс является менее полезным, чем GroupEnumerator, т.к. требует, чтобы дочерние перечислители
    /// были созданы заранее.
    /// </summary>
    [Serializable]
    public sealed class Enumerator : GroupEnumerator<T>
    {
      #region Конструктор

      /// <summary>
      /// Создает перечислитель с заданным списком объектов.
      /// </summary>
      /// <param name="groups">Фиксированный список перечислителей.
      /// Массив не может содержать значения null, т.к. в этом случае перечисление завершится досрочно</param>
      public Enumerator(IEnumerator<T>[] groups)
      {
        if (groups == null)
          throw new ArgumentNullException("groups");
        _Groups = groups;
      }

      #endregion

      #region Список групп

      private IEnumerator<T>[] _Groups;

      /// <summary>
      /// Возвращает следующую группу из списка
      /// </summary>
      /// <param name="groupIndex">Индекс следующей группы</param>
      /// <returns>Очередной перечислитель или null, если список групп пустой</returns>
      protected override IEnumerator<T> GetNextGroup(int groupIndex)
      {
        if (groupIndex >= _Groups.Length)
          return null;
        else
          return _Groups[groupIndex];
      }

      /// <summary>
      /// Вызывает Dispose() для всех перечислителей
      /// </summary>
      public override void Dispose()
      {
        base.Dispose();
        for (int i = 0; i < _Groups.Length; i++)
          _Groups[i].Dispose(); // может быть повторный вызов
      }

      #endregion
    }

    #region GetEnumerator()

    /// <summary>
    /// Создает перечислитель
    /// </summary>
    /// <returns>Перечислитель</returns>
    public Enumerator GetEnumerator()
    {
      IEnumerator<T>[] a = new IEnumerator<T>[_Groups.Length];
      for (int i = 0; i < _Groups.Length; i++)
        a[i] = _Groups[i].GetEnumerator();

      return new Enumerator(a);
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion
  }

  /// <summary>
  /// Получение типизированного перечислителя для массива
  /// </summary>
  /// <typeparam name="T"></typeparam>
  [Serializable]
  public struct ArrayEnumerable<T> : IEnumerable<T>
  {
    #region Конструктор

    /// <summary>
    /// Создает объект для массива
    /// </summary>
    /// <param name="a">Массив</param>
    public ArrayEnumerable(T[] a)
    {
      if (a == null)
        throw new ArgumentNullException("a");
      _Array = a;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Массив, по которому будет выполняться перечисление
    /// </summary>
    public T[] Array { get { return _Array; } }
    private T[] _Array;

    #endregion

    /// <summary>
    /// Типизированный перечислитель по массиву.
    /// Метод Array.GetEnumerator() возвращает нетипизированный перечислитель
    /// </summary>
    [Serializable]
    public struct Enumerator : IEnumerator<T>
    {
      #region Конструктор

      /// <summary>
      /// Создает перечислитель для одномерного массива
      /// </summary>
      /// <param name="a">Массив</param>
      public Enumerator(T[] a)
      {
        if (a == null)
          throw new ArgumentNullException("a");
        _Array = a;
        _Index = -1;
      }

      private T[] _Array;
      private int _Index;

      #endregion

      #region IEnumerator<T> Members

      /// <summary>
      /// Возвращает текущий элемент массива
      /// </summary>
      public T Current { get { return _Array[_Index]; } }

      /// <summary>
      /// Ничего не делает
      /// </summary>
      public void Dispose()
      {
      }

      object IEnumerator.Current { get { return _Array[_Index]; } }

      /// <summary>
      /// Переходит к следующему элементу массива
      /// </summary>
      /// <returns></returns>
      public bool MoveNext()
      {
        _Index++;
        return _Index < _Array.Length;
      }

      /// <summary>
      /// Сброс перечислителя
      /// </summary>
      void IEnumerator.Reset()
      {
        _Index = -1;
      }

      #endregion
    }

    #region IEnumerable<T> Members

    /// <summary>
    /// Создает новый перечислитель для массива
    /// </summary>
    /// <returns>Перечислитель</returns>
    public Enumerator GetEnumerator()
    {
      return new Enumerator(_Array);
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return new Enumerator(_Array);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return new Enumerator(_Array);
    }

    #endregion
  }

  /// <summary>
  /// Объект, для которого можно вызвать оператор foreach.
  /// Возвращает перечислитель для перебора сегментов массива, каждый из которых имеет размер, не превышающий заданный.
  /// Элементами перечисления являются структуры ArraySegment.
  /// При переборе все сегменты, кроме последнего, имеют заданный размер.
  /// Если задан массив нулевой длины, перечислитель не будет вызван ни разу.
  /// В теле цикла перебора можно менять элементы перебираемого массива.
  /// Если при переборе требуется создавать подмассив для каждого сегмента, используйте ArrayBlockEnumerable.
  /// </summary>
  /// <typeparam name="T">Тип данных, хранящихся в массиве</typeparam>
  [Serializable]
  public struct ArraySegmentEnumerable<T> : IEnumerable<ArraySegment<T>>
  {
    #region Конструктор

    /// <summary>
    /// Создает перечислитель для массива
    /// </summary>
    /// <param name="array">Перебираемый массив</param>
    /// <param name="segmentSize">Размер сегмента. Не может быть меньше 1</param>
    public ArraySegmentEnumerable(T[] array, int segmentSize)
    {
      if (array == null)
        throw new ArgumentNullException("array");
      if (segmentSize < 1)
        throw new ArgumentOutOfRangeException("segmentSize");

      _Array = array;
      _SegmentSize = segmentSize;
    }

    #endregion

    #region Поля

    private T[] _Array;
    private int _SegmentSize;

    #endregion

    #region IEnumerable<ArraySegment<T>> Members

    /// <summary>
    /// Перечислитель для перебора сегментов массива, каждый из которых имеет размер, не превышающий заданный.
    /// Элементами перечисления являются структуры ArraySegment.
    /// При переборе все сегменты, кроме последнего, имеют заданный размер.
    /// Если задан массив нулевой длины, перечислитель не будет вызван ни разу.
    /// В теле цикла перебора можно менять элементы перебираемого массива.
    /// Если при переборе требуется создавать подмассив для каждого сегмента, используйте ArrayBlockEnumerator.
    /// </summary>
    [Serializable]
    public struct Enumerator : IEnumerator<ArraySegment<T>>
    {
      #region Конструктор

      /// <summary>
      /// Создает перечислитель для массива
      /// </summary>
      /// <param name="array">Перебираемый массив</param>
      /// <param name="segmentSize">Размер сегмента. Не может быть меньше 1</param>
      internal Enumerator(T[] array, int segmentSize)
      {

        _Array = array;
        _SegmentSize = segmentSize;
        _CurrentOffset = -1;
      }

      #endregion

      #region Поля

      private T[] _Array;
      private int _SegmentSize;
      private int _CurrentOffset;

      #endregion

      #region IEnumerator<ArraySegment<T>> Members

      /// <summary>
      /// Возвращает текущий сегмент
      /// </summary>
      public ArraySegment<T> Current
      {
        get
        {
          int count = Math.Min(_Array.Length - _CurrentOffset, _SegmentSize);
          return new ArraySegment<T>(_Array, _CurrentOffset, count);
        }
      }

      /// <summary>
      /// Ничего не делает
      /// </summary>
      public void Dispose()
      {
      }

      object IEnumerator.Current { get { return Current; } }

      /// <summary>
      /// Переход к очередному сегменту
      /// </summary>
      /// <returns>Есть сегмент Current</returns>
      public bool MoveNext()
      {
        if (_CurrentOffset < 0)
          _CurrentOffset = 0;
        else
          _CurrentOffset += _SegmentSize;

        return _CurrentOffset < _Array.Length;
      }

      /// <summary>
      /// Инициализация повторного перечисления
      /// </summary>
      void System.Collections.IEnumerator.Reset()
      {
        _CurrentOffset = -1;
      }

      #endregion
    }

    /// <summary>
    /// Создает перечислитель ArraySegmentEnumerator
    /// </summary>
    /// <returns></returns>
    public Enumerator GetEnumerator()
    {
      return new Enumerator(_Array, _SegmentSize);
    }

    IEnumerator<ArraySegment<T>> IEnumerable<ArraySegment<T>>.GetEnumerator()
    {
      return new Enumerator(_Array, _SegmentSize);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return new Enumerator(_Array, _SegmentSize);
    }

    #endregion
  }

  /// <summary>
  /// Объект, для которого можно вызвать оператор foreach.
  /// Возвращает перечислитель для перебора частей массива, каждая из которых имеет размер, не превышающий заданный.
  /// Элементами перечисления являются одномерные массивы меньшей длины.
  /// При переборе все сегменты, кроме последнего, имеют заданный размер.
  /// Если задан массив нулевой длины, перечислитель не будет вызван ни разу.
  /// В теле цикла перебора нельзя менять элементы перебираемого подмассива.
  /// Если при переборе не нужны помассивы как самостоятельные объекты, может быть выгоднее использовать ArraySegmentEnumerator.
  /// </summary>
  /// <typeparam name="T">Тип данных, хранящихся в массиве</typeparam>
  [Serializable]
  public struct ArrayBlockEnumerable<T> : IEnumerable<T[]>
  {
    #region Конструктор

    /// <summary>
    /// Создает перечислитель для массива
    /// </summary>
    /// <param name="array">Перебираемый массив</param>
    /// <param name="segmentSize">Размер сегмента. Не может быть меньше 1</param>
    public ArrayBlockEnumerable(T[] array, int segmentSize)
    {
      if (array == null)
        throw new ArgumentNullException("array");
      if (segmentSize < 1)
        throw new ArgumentOutOfRangeException("segmentSize");

      _Array = array;
      _SegmentSize = segmentSize;
    }

    #endregion

    #region Поля

    private T[] _Array;
    private int _SegmentSize;

    #endregion

    #region IEnumerable<ArraySegment<T>> Members

    /// <summary>
    /// Перечислитель для перебора частей массива, каждая из которых имеет размер, не превышающий заданный.
    /// Элементами перечисления являются одномерные массивы меньшей длины.
    /// При переборе все сегменты, кроме последнего, имеют заданный размер.
    /// Если задан массив нулевой длины, перечислитель не будет вызван ни разу.
    /// В теле цикла перебора нельзя менять элементы перебираемого подмассива.
    /// Если при переборе не нужны помассивы как самостоятельные объекты, может быть выгоднее использовать ArraySegmentEnumerator.
    /// </summary>
    [Serializable]
    public struct Enumerator : IEnumerator<T[]>
    {
      #region Конструктор

      /// <summary>
      /// Создает перечислитель для массива
      /// </summary>
      /// <param name="array">Перебираемый массив</param>
      /// <param name="segmentSize">Размер сегмента. Не может быть меньше 1</param>
      internal Enumerator(T[] array, int segmentSize)
      {

        _Array = array;
        _SegmentSize = segmentSize;
        _CurrentOffset = -1;
      }

      #endregion

      #region Поля

      private T[] _Array;
      private int _SegmentSize;
      private int _CurrentOffset;

      #endregion

      #region IEnumerator<ArraySegment<T>> Members

      /// <summary>
      /// Возвращает текущий сегмент
      /// </summary>
      public T[] Current
      {
        get
        {
          if (_CurrentOffset == 0 && _SegmentSize >= _Array.Length)
            return _Array;
          else
          {
            int count = Math.Min(_Array.Length - _CurrentOffset, _SegmentSize);
            T[] a = new T[count];
            Array.Copy(_Array, _CurrentOffset, a, 0, count);
            return a;
          }
        }
      }

      /// <summary>
      /// Ничего не делает
      /// </summary>
      public void Dispose()
      {
      }

      object IEnumerator.Current { get { return Current; } }

      /// <summary>
      /// Переход к очередному сегменту
      /// </summary>
      /// <returns>Есть сегмент Current</returns>
      public bool MoveNext()
      {
        if (_CurrentOffset < 0)
          _CurrentOffset = 0;
        else
          _CurrentOffset += _SegmentSize;

        return _CurrentOffset < _Array.Length;
      }

      /// <summary>
      /// Инициализация повторного перечисления
      /// </summary>
      void System.Collections.IEnumerator.Reset()
      {
        _CurrentOffset = -1;
      }

      #endregion
    }

    /// <summary>
    /// Создает перечислитель по блокам
    /// </summary>
    /// <returns></returns>
    public Enumerator GetEnumerator()
    {
      return new Enumerator(_Array, _SegmentSize);
    }

    IEnumerator<T[]> IEnumerable<T[]>.GetEnumerator()
    {
      return new Enumerator(_Array, _SegmentSize);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return new Enumerator(_Array, _SegmentSize);
    }

    #endregion
  }

  /// <summary>
  /// Перечислитель, выполняющий преобразование типа данных.
  /// Получает на входе нетипизированный перечислитель и использует его для перебора элементов.
  /// При запросе очередного элемента выполняется его преобразование в тип <typeparamref name="T"/>
  /// с помощью оператора "as". Если преобразование невозможно, элемент перечисления пропускается
  /// </summary>
  /// <typeparam name="T">Тип данных, которые перечисляются. Должно быть классом, а не структурой</typeparam>
  [Serializable]
  public struct ConvertEnumerable<T> : IEnumerable<T>
    where T : class
  {
    #region Конструктор

    /// <summary>
    /// Создает перечислитель
    /// </summary>
    /// <param name="enumerable">Исходный перечислитель</param>
    public ConvertEnumerable(System.Collections.IEnumerable enumerable)
    {
      if (enumerable == null)
        throw new ArgumentNullException("enumerable");
      _Enumerable = enumerable;
    }

    #endregion

    #region Поля

    private System.Collections.IEnumerable _Enumerable;

    #endregion

    /// <summary>
    /// Перечислитель, выполняющий преобразование типа данных.
    /// Получает на входе нетипизированный перечислитель и использует его для перебора элементов.
    /// При запросе очередного элемента выполняется его преобразование в тип <typeparamref name="T"/>
    /// с помощью оператора "as". Если преобразование невозможно, элемент перечисления пропускается
    /// </summary>
    [Serializable]
    public struct Enumerator : IEnumerator<T>
    {
      #region Конструктор

      /// <summary>
      /// Создает перечислитель
      /// </summary>
      /// <param name="enumerator">Нетипизированный перечислитель</param>
      public Enumerator(System.Collections.IEnumerator enumerator)
      {
        if (enumerator == null)
          throw new ArgumentNullException("enumerator");
        _Enumerator = enumerator;
        _Current = null;
      }

      #endregion

      #region Поля

      /// <summary>
      /// Оригинальный перечислитель
      /// </summary>
      System.Collections.IEnumerator _Enumerator;

      /// <summary>
      /// Сохраняем текущий элемент, чтобы избежать повторного преобразования
      /// </summary>
      T _Current;

      #endregion

      #region IEnumerator<T> Members

      /// <summary>
      /// Возвращает текущий элемент перечисления
      /// </summary>
      public T Current { get { return _Current; } }

      /// <summary>
      /// Уничтожает перечислитель
      /// </summary>
      public void Dispose()
      {
        IDisposable Disp = _Enumerator as IDisposable;
        if (Disp != null)
          Disp.Dispose();
        _Enumerator = null;
        _Current = null;
      }

      #endregion

      #region IEnumerator Members

      object IEnumerator.Current { get { return _Current; } }

      /// <summary>
      /// Переход к очередному элементу перечисления.
      /// </summary>
      /// <returns>true, если есть следующий элемент</returns>
      public bool MoveNext()
      {
        while (_Enumerator.MoveNext())
        {
          _Current = _Enumerator.Current as T;
          if (_Current != null)
            return true;
        }
        return false;
      }

      /// <summary>
      /// Перезапуск перечислителя
      /// </summary>
      void IEnumerator.Reset()
      {
        _Enumerator.Reset();
      }

      #endregion
    }

    #region IEnumerable<T> members

    /// <summary>
    /// Создает новый перечислитель
    /// </summary>
    /// <returns>Перечислитель</returns>
    public Enumerator GetEnumerator()
    {
      return new Enumerator(_Enumerable.GetEnumerator());
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}
