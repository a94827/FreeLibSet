using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Microsoft.Win32;
using System.Data;
using System.Runtime.InteropServices;

namespace AgeyevAV
{
  /// <summary>
  /// Пустой перечислитель.
  /// Реализует интерфейс IEnumerator для коллекции, не содержащей элементов
  /// </summary>
  /// <typeparam name="T">Тип "перечислимых" объектов</typeparam>
  [Serializable]
  public sealed class DummyEnumerator<T> : IEnumerator<T>
  {
    #region IEnumerator<T> Members

    T IEnumerator<T>.Current
    {
      get { return default(T); }
    }

    void IDisposable.Dispose()
    {
    }

    object IEnumerator.Current
    {
      get { return null; }
    }

    bool IEnumerator.MoveNext()
    {
      return false;
    }

    void IEnumerator.Reset()
    {
    }

    #endregion
  }

  /// <summary>
  /// Реализует интерфейс IEnumerable для коллекции, не содержащей элементов
  /// </summary>
  /// <typeparam name="T"></typeparam>
  [Serializable]
  public sealed class DummyEnumerable<T> : IEnumerable<T>
  {
    #region IEnumerable<T> Members

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return new DummyEnumerator<T>();
    }


    IEnumerator IEnumerable.GetEnumerator()
    {
      return new DummyEnumerator<T>();
    }

    #endregion
  }

  /// <summary>
  /// "Перечислитель" для одного элемента.
  /// Не учитывает равенство объекта null, то есть, если объект равен null, то он все равно будет перечислен.
  /// Если требуется пустой перечислитель, используйте DummyEnumerator.
  /// </summary>
  /// <typeparam name="T">Тип перечислимого объекта</typeparam>
  [Serializable]
  public sealed class SingleObjectEnumerator<T> : IEnumerator<T>
  {
    #region Конструктор

    /// <summary>
    /// Создает псевдоперечислитель для одного объекта
    /// </summary>
    /// <param name="singleObject">Единственный объект</param>
    public SingleObjectEnumerator(T singleObject)
    {
      _Object = singleObject;
      Flag = false;
    }

    #endregion

    #region IEnumerator<T> Members

    private T _Object;
    private bool Flag;

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
    /// Переход к следующему значению
    /// </summary>
    /// <returns>Наличие значения</returns>
    public bool MoveNext()
    {
      if (Flag)
        return false;
      else
      {
        Flag = true;
        return true;
      }
    }

    void IEnumerator.Reset()
    {
      Flag = false;
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

    #region IEnumerable<T> Members

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      return new SingleObjectEnumerator<T>(_Object);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return new SingleObjectEnumerator<T>(_Object);
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
    public void Dispose()
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
  /// Этот класс является менее полезным, чем GroupEnumerator, т.к. требует, чтобы дочерение перечислители
  /// были созданы заранее.
  /// </summary>
  /// <typeparam name="T">Тип перечислимых объектов</typeparam>
  [Serializable]
  public sealed class GroupArrayEnumerator<T> : GroupEnumerator<T>
  {
    #region Конструктор

    /// <summary>
    /// Создает перечислитель с заданным списком объектов.
    /// </summary>
    /// <param name="groups">Фиксированный список перечислителей.
    /// Массив не может содержать значения null, т.к. в этом случае перечисление завершится досрочно</param>
    public GroupArrayEnumerator(IEnumerator<T>[] groups)
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

    #endregion
  }

  /// <summary>
  /// Типизированный перечислитель по массиву.
  /// Метод Array.GetEnumerator() возвращает нетипизированный перечислитель
  /// </summary>
  /// <typeparam name="T">Тип элементов массива</typeparam>
  [Serializable]
  [StructLayout(LayoutKind.Auto)]
  public struct ArrayEnumerator<T> : IEnumerator<T>
  {
    #region Конструктор

    /// <summary>
    /// Создает перечислитель для одномерного массива
    /// </summary>
    /// <param name="a">Массив</param>
    public ArrayEnumerator(T[] a)
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
  public class ArraySegmentEnumerable<T> : IEnumerable<ArraySegment<T>>
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
  public class ArrayBlockEnumerable<T> : IEnumerable<T[]>
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
  public sealed class ConvertEnumerator<T> : IEnumerator<T>
    where T : class
  {
    #region Конструктор

    /// <summary>
    /// Создает перечислитель
    /// </summary>
    /// <param name="enumerator">Нетипизированный перечислитель</param>
    public ConvertEnumerator(System.Collections.IEnumerator enumerator)
    {
      if (enumerator == null)
        throw new ArgumentNullException("enumerator");
      _Enumerator = enumerator;
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
    public void Reset()
    {
      _Enumerator.Reset();
    }

    #endregion
  }

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

      if (compareColumnCount < 1 || compareColumnCount >= aColNames.Length)
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
        if (!(Object.ReferenceEquals(s1, null)) || Object.ReferenceEquals(s2, null))
          return String.Compare(s1, s2, StringComparison.OrdinalIgnoreCase) == 0;
      }

      return x1.Equals(x2);
    }

    #endregion

    #region Перечислитель

    /// <summary>
    /// Реализация перечислителя.
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
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
