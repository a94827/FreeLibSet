using System;
using System.Collections.Generic;
using System.Text;

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

namespace AgeyevAV
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

      bool Changed = false;
      for (int i = 0; i < _Flags.Length; i++)
      {
        if (value[i] != _Flags[i])
        {
          _Flags[i] = value[i];
          Changed = true;
        }
      }
      if (Changed)
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
      bool Changed = false;
      for (int i = 0; i < _Flags.Length; i++)
      {
        if (!_Flags[i])
        {
          _Flags[i] = true;
          Changed = true;
        }
      }
      if (Changed)
        OnChanged();
    }

    /// <summary>
    /// Устанавливает все флажки в значение false.
    /// Событие Changed вызывается один раз в конце, только если значение какого-либо флажка изменилось
    /// </summary>
    public void UnselectAll()
    {
      bool Changed = false;
      for (int i = 0; i < _Flags.Length; i++)
      {
        if (_Flags[i])
        {
          _Flags[i] = false;
          Changed = true;
        }
      }
      if (Changed)
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
