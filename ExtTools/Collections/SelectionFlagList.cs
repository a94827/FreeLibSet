// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;

namespace FreeLibSet.Collections
{
  /// <summary>
  /// Массив флажков для выбора.
  /// Содержит виртуальный метод OnChanged() и событие Changed.
  /// Этот класс не является потокобезопасным
  /// </summary>
  [Serializable]
  public class SelectionFlagList
  {
    #region Конструкторы

    /// <summary>
    /// Создает массив с заданным числом элементов.
    /// В исходном состоянии флажки не установлены.
    /// </summary>
    /// <param name="count">Количество флажков</param>
    public SelectionFlagList(int count)
    {
      _Flags = new bool[count];
    }

    /// <summary>
    /// Создает массив флажков, копируя флажки из коллекции.
    /// После выполнения конструктора связь с исходной коллекцией не сохряняется.
    /// </summary>
    /// <param name="source">Образцовая коллекция флажков</param>
    public SelectionFlagList(ICollection<bool> source)
    {
#if DEBUG
      if (source == null)
        throw new ArgumentNullException("source");
#endif
      _Flags = new bool[source.Count];
      source.CopyTo(_Flags, 0);
    }

    #endregion

    #region Флажки

    private bool[] _Flags;

    /// <summary>
    /// Получение / установка значения одного флажка по индексу.
    /// </summary>
    /// <param name="index">Индекс флажка</param>
    /// <returns>Значение флажка</returns>
    public bool this[int index]
    {
      get { return _Flags[index]; }
      set
      {
        if (value == _Flags[index])
          return;
        _Flags[index] = value;
        OnChanged();
      }
    }

    /// <summary>
    /// Количество флажков в массиве. Определяется в конструкторе и не может быть изменено.
    /// </summary>
    public int Count { get { return _Flags.Length; } }

    /// <summary>
    /// Возвращает количество выбранных элементов
    /// </summary>
    public int SelectedCount
    {
      get
      {
        int cnt = 0;
        for (int i = 0; i < _Flags.Length; i++)
        {
          if (_Flags[i])
            cnt++;
        }
        return cnt;
      }
    }

    /// <summary>
    /// Текстовое представление для отладки.
    /// Возвращает строку длиной Count символов "0" и "1". Для установленных флажков содержит "1", для не установленных - "0"
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder(_Flags.Length);
      for (int i = 0; i < _Flags.Length; i++)
        sb.Append(_Flags[i] ? '1' : '0');

      return sb.ToString();
    }

    #endregion

    #region Событие Changed

    /// <summary>
    /// Событие вызывается при изменении любого флажка
    /// </summary>
    public event EventHandler Changed;

    /// <summary>
    /// Вызывается при изменении люого флажка.
    /// Вызывает событие Changed.
    /// </summary>
    protected virtual void OnChanged()
    {
      if (Changed != null)
        Changed(this, EventArgs.Empty);
    }

    #endregion

    #region Преобразование в массив

    /// <summary>
    /// Копирует флажки в массив.
    /// </summary>
    /// <returns>Новый массив флажков</returns>
    public bool[] ToArray()
    {
      return (bool[])(_Flags.Clone());
    }

    /// <summary>
    /// Групповая установлка всех флажков.
    /// Длина массива <paramref name="value"/> должна совпадать с числом флажков Count.
    /// Событие Changed вызывается один раз в конце, только если значение какого-либо флажка изменилось.
    /// </summary>
    /// <param name="value">Массив новых значений</param>
    public void FromArray(bool[] value)
    {
      if (value == null)
        throw new ArgumentNullException();
      if (value.Length != _Flags.Length)
        throw new ArgumentException("Неправильная длина массива");

      bool changed = false;
      for (int i = 0; i < _Flags.Length; i++)
      {
        if (value[i] != _Flags[i])
        {
          _Flags[i] = value[i];
          changed = true;
        }
      }
      if (changed)
        OnChanged();
    }

    #endregion

    #region Выбор всех строк и отмена выбора

    /// <summary>
    /// Устанавливает все флажки в значение true.
    /// Событие Changed вызывается один раз в конце, только если значение какого-либо флажка изменилось
    /// </summary>
    public void SelectAll()
    {
      bool changed = false;
      for (int i = 0; i < _Flags.Length; i++)
      {
        if (!_Flags[i])
        {
          _Flags[i] = true;
          changed = true;
        }
      }
      if (changed)
        OnChanged();
    }

    /// <summary>
    /// Устанавливает все флажки в значение false.
    /// Событие Changed вызывается один раз в конце, только если значение какого-либо флажка изменилось
    /// </summary>
    public void UnselectAll()
    {
      bool changed = false;
      for (int i = 0; i < _Flags.Length; i++)
      {
        if (_Flags[i])
        {
          _Flags[i] = false;
          changed = true;
        }
      }
      if (changed)
        OnChanged();
    }

    /// <summary>
    /// Устанавливает все флажки в противоположное значение.
    /// Событие Changed вызывается один раз в конце, если число флажков больше 0.
    /// </summary>
    public void InvertAll()
    {
      for (int i = 0; i < _Flags.Length; i++)
        _Flags[i] = !_Flags[i];
      if (_Flags.Length > 0)
        OnChanged();
    }

    /// <summary>
    /// Возвращает true, если все флажки имеют значение true.
    /// Если Count=0, возвращается true.
    /// </summary>
    public bool AreAllSelected
    {
      get
      {
        for (int i = 0; i < _Flags.Length; i++)
        {
          if (!_Flags[i])
            return false;
        }
        return true;
      }
    }

    /// <summary>
    /// Возвращает true, если все флажки имеют значение false.
    /// Если Count=0, возвращается true.
    /// </summary>
    public bool AreAllUnselected
    {
      get
      {
        for (int i = 0; i < _Flags.Length; i++)
        {
          if (_Flags[i])
            return false;
        }
        return true;
      }
    }

    #endregion

    #region SelectedIndices

    /// <summary>
    /// Выбранные элементы в виде массива индексов элементов.
    /// Чтение и запись свойства являются относительно медленными, поэтому следует избегать обращения
    /// к свойству в цикле.
    /// При записи в массиве могут быть значения (-1), которые игнорируются
    /// </summary>
    public int[] SelectedIndices
    {
      get
      {
        List<int> lst = new List<int>();
        for (int i = 0; i < _Flags.Length; i++)
        {
          if (_Flags[i])
            lst.Add(i);
        }
        return lst.ToArray();
      }
      set
      {
        if (value != null)
        {
          bool[] a = new bool[_Flags.Length];
          for (int i = 0; i < value.Length; i++)
          {
            if (value[i] >= 0)
              a[value[i]] = true;
          }
          FromArray(a);
        }
        else
          UnselectAll();
      }
    }

    #endregion
  }
}
