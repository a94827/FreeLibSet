using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;

/*
 * The BSD License
 * 
 * Copyright (c) 2006-2015, Ageyev A.V.
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

namespace AgeyevAV.DependedValues
{
  /// <summary>
  /// Нетипизированный интерфейс, реализуемый шаблонным классом DepValue. Нет других классов, реализующих этот интерфейс.
  /// </summary>
  public interface IDepValue
  {
    #region Свойства и событие

    /// <summary>
    /// Информация об объекте-владельце и реализуемом свойстве. Может быть не инициализировано.
    /// </summary>
    DepOwnerInfo OwnerInfo { get; set; }

    /// <summary>
    /// Текущее значение
    /// </summary>
    object Value { get; set;}

    /// <summary>
    /// true, если в настоящее время выполняется установка значения
    /// </summary>
    bool InsideSetValue { get; }

    /// <summary>
    /// Извещение посылается при изменении значения свойства ValueEx
    /// </summary>
    event EventHandler ValueChanged;

    #endregion
  }

  /// <summary>
  /// Абстрактный базовый класс для доступа к значению производного типа.
  /// Свойства объектов объявляются с этим типом.
  /// Реализация свойств выполняется с помощью классов DepInput и DepValueObject
  /// </summary>
  /// <typeparam name="T">Тип хранимого значения</typeparam>
  [Serializable]
  public abstract class DepValue<T> : IDepValue
  {
    #region Конструктор

    /// <summary>
    /// Создает новый объект
    /// </summary>
    public DepValue()
    {
      _InsideSetValue = false;
      _Delayed = false;
    }

    #endregion

    #region Объект-владелец

    /// <summary>
    /// Информация об объекте-владельце и реализуемом свойстве. Может быть не инициализировано
    /// Используется в отладочных целях и некоторых методах поиска, например,
    /// DepNot.NotOutput().
    /// Также используется в ToString()
    /// Это свойство не передается при сериализации.
    /// </summary>
    public DepOwnerInfo OwnerInfo { get { return _OwnerInfo; } set { _OwnerInfo = value; } }
    [NonSerialized]
    private DepOwnerInfo _OwnerInfo;

    #endregion

    #region Текущее значение

    /// <summary>
    /// Текущее значение
    /// </summary>
    public T Value
    {
      get
      {
        if (_Delayed)
        {
          _Value = GetDelayedValue();
          _Delayed = false;
        }
        return _Value;
      }
      set
      {
        DoSetValue(value);
      }
    }
    private T _Value;

    object IDepValue.Value { get { return this.Value; } set { this.Value = (T)value; } }

    /// <summary>
    /// Этот метод обычно вызывает BaseSetValue(), но может выполнять дополнительные действия по установке значения
    /// </summary>
    /// <param name="value">Устанавливаемоен значение</param>
    protected abstract void DoSetValue(T value);

    /// <summary>
    /// Защищенный метод установки значения без вызова DoSetValue()
    /// </summary>
    /// <param name="value">Устанавливаемоен значение</param>
    protected void BaseSetValue(T value)
    {
      // Предотвращаем зациклинивание, когда установка значения приводит через
      // цепочку значений к повторной установке этого же значения
      if (_InsideSetValue)
        return;
      _InsideSetValue = true;
      try
      {
        if (!Object.Equals(value, _Value))
        {
          _Value = value;

          OnValueChanged();
          DepInput<T> CurrOutput = FirstOutput;
          while (CurrOutput != null)
          {
            CurrOutput.SetValueChanged();
            CurrOutput = CurrOutput.NextOutput;
          }
        }
      }
      finally
      {
        _InsideSetValue = false;
      }
    }

    /// <summary>
    /// true, если в настоящее время выполняется установка значения
    /// </summary>
    public bool InsideSetValue { get { return _InsideSetValue; } }
    private bool _InsideSetValue;

    /// <summary>
    /// Признак того, что значение будет получено позже, если понадобится
    /// </summary>
    private bool _Delayed;

    /// <summary>
    /// Уставливает признак отложенной установки в true.
    /// Посылает событие ValueChanged текущему объекту и зависимым объектам
    /// </summary>
    protected void SetDelayed()
    {
      if (_InsideSetValue)
        return;
      _InsideSetValue = true;
      try
      {
        _Delayed = true;

        if (ValueChanged != null)
          ValueChanged(this, EventArgs.Empty);

        DepInput<T> CurrOutput = FirstOutput;
        while (CurrOutput != null)
        {
          CurrOutput.SetValueChanged();
          CurrOutput = CurrOutput.NextOutput;
        }
      }
      finally
      {
        _InsideSetValue = false;
      }
    }

    /// <summary>
    /// Получить отложенное значение.
    /// Непереопределенный метод генерирует исключение.
    /// </summary>
    /// <returns>Значение</returns>
    protected virtual T GetDelayedValue()
    {
      throw new NotImplementedException("Получение отложенных значений не реализовано");
    }


    /// <summary>
    /// Обмен местами двух значений.
    /// Нестатическая версия метода.
    /// </summary>
    /// <param name="otherValue">Другой объект, хранящий значение</param>
    public void SwapValues(DepValue<T> otherValue)
    {
      T x = otherValue.Value;
      otherValue.Value = Value;
      Value = x;
    }

    /// <summary>
    /// Обмен местами двух значений
    /// Статическая версия метода.
    /// </summary>
    /// <param name="v1">Первый объект со значением</param>
    /// <param name="v2">Второй объект со значением</param>
    public static void SwapValues(DepValue<T> v1, DepValue<T> v2)
    {
      T x = v1.Value;
      v1.Value = v2.Value;
      v2.Value = x;
    }

    #endregion

    #region Абстрактное свойство

    /// <summary>
    /// Источник для значения.
    /// Непереопределенное свойство возвращает null и не поддерживает присвоение значения
    /// </summary>
    public virtual DepValue<T> Source
    {
      get
      {
        return null;
      }
      set
      {
        if (value != null)
          throw new InvalidOperationException("Этот объект не поддерживает установку свойства Source");
      }
    }

    #endregion

    #region Событие

    /// <summary>
    /// Извещение посылается при изменении значения свойства ValueEx
    /// </summary>
    public event EventHandler ValueChanged;

    /// <summary>
    /// Вызов события ValueChanged
    /// </summary>
    public void OnValueChanged()
    {
      if (ValueChanged != null)
        ValueChanged(this, EventArgs.Empty);
    }

    #endregion

    #region Список зависимых значений

    // Зависимые значения строятся с помощью связанного списка
    // Используются свойства DepValue.FirstOutput и DepInput.NextOutput

    internal DepInput<T> FirstOutput;

    /// <summary>
    /// Количество зависимых объектов, присоединенных к данному
    /// </summary>
    public int OutputCount
    {
      get
      {
        int n = 0;
        DepInput<T> CurrOutput = FirstOutput;
        while (CurrOutput != null)
        {
          n++;
          CurrOutput = CurrOutput.NextOutput;
        }
        return n;
      }
    }

    /// <summary>
    /// Массив зависимых объектов, присоединенных к данному
    /// </summary>
    /// <returns>Массив выходов</returns>
    public DepInput<T>[] GetOutputs()
    {
      #region Подсчет

      int n = 0;
      DepInput<T> CurrInput = FirstOutput;
      while (CurrInput != null)
      {
        n++;
        CurrInput = CurrInput.NextOutput;
      }

      #endregion

      #region Создание списка

      DepInput<T>[] Outputs = new DepInput<T>[n];
      n = 0;
      CurrInput = FirstOutput;
      while (CurrInput != null)
      {
        Outputs[n] = CurrInput;
        n++;
        CurrInput = CurrInput.NextOutput;
      }
      return Outputs;

      #endregion
    }

    internal void AddOutput(DepInput<T> theInput)
    {
      if (theInput == null)
        return;

#if DEBUG
      if (theInput.NextOutput != null)
        throw new ArgumentException("Повторное присоеднинение входа " + theInput.ToString());
#endif
      theInput.NextOutput = FirstOutput;
      FirstOutput = theInput;
    }

    internal void RemoveOutput(DepInput<T> theInput)
    {
      if (theInput == null)
        return;

      if (FirstOutput == theInput)
        FirstOutput = theInput.NextOutput;
      else
      {
        DepInput<T> CurrOutput = FirstOutput;
        while (CurrOutput != null)
        {
          if (CurrOutput.NextOutput == theInput)
          {
            CurrOutput.NextOutput = theInput.NextOutput;
            break;
          }
          CurrOutput = CurrOutput.NextOutput;
        }
      }
      theInput.NextOutput = null;
    }

    #endregion

    #region Виртуальный метод для доступа к источнику данных

    /// <summary>
    /// Переопределенный метод в DepInput возвращает источник данных.
    /// Базовый метод возвращает null.
    /// Этот метод не предназначен для использования в пользовательском коде
    /// </summary>
    /// <returns></returns>
    public virtual DepValue<T> GetSource()
    {
      return null;
    }

    #endregion

    #region Отладочные средства

    /// <summary>
    /// Текстовое представление содержит OwnerInfo и текущее значение Value
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return _OwnerInfo.ToString() + ", Value=" + Value.ToString();
    }

    #endregion
  }

  /// <summary>
  /// Информация об объекте-владельце и реализуемом свойстве.
  /// Хранится в DepValuе и используется для отладочных целей.
  /// Структура не является сериализуемой
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  public struct DepOwnerInfo
  {
    #region Конструктор

    /// <summary>
    /// Создает новый объект
    /// </summary>
    /// <param name="owner">Объект-владелец</param>
    /// <param name="property">Имя свойства</param>
    public DepOwnerInfo(object owner, string property)
    {
      _Owner = owner;
      _Property = property;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Объект-владелец
    /// </summary>
    public object Owner { get { return _Owner; } }
    private object _Owner;

    /// <summary>
    /// Реализуемое свойство
    /// </summary>
    public string Property { get { return _Property; } }
    private string _Property;

    /// <summary>
    /// Выводит Owner.ToString() и Property
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      if (_Owner == null)
        return "Unknown";
      else
      {
        string s = _Owner.ToString();
        if (!String.IsNullOrEmpty(_Property))
          s += ", Property=\"" + _Property + "\"";
        return s;
      }
    }

    #endregion
  }


  /// <summary>
  /// Неабстрактная реализация DepValue
  /// </summary>
  /// <typeparam name="T">Тип хранимого значения</typeparam>
  [Serializable]
  public class DepValueObject<T> : DepValue<T>
  {
    #region Установка значения

    /// <summary>
    /// Этот метод генерирует исключение.
    /// Следует использовать OwnerSetValue() для установки значения
    /// </summary>
    /// <param name="value"></param>
    protected override void DoSetValue(T value)
    {
      throw new InvalidOperationException("Установка этого значения не допускается");
    }

    /// <summary>
    /// Этот метод вызывается объектом-владельцем для установки значения
    /// </summary>
    /// <param name="value"></param>
    public void OwnerSetValue(T value)
    {
      BaseSetValue(value);
    }

    #endregion
  }

  /// <summary>
  /// Константа
  /// </summary>
  /// <typeparam name="T">Тип константы</typeparam>
  [Serializable]
  public class DepConst<T> : DepValue<T>
  {
    #region Конструктор

    /// <summary>
    /// Создает константу
    /// </summary>
    /// <param name="value">Значение</param>
    public DepConst(T value)
    {
      BaseSetValue(value);
    }

    #endregion

    #region Переопределяемые методы

    /// <summary>
    /// Генерирует исключение
    /// </summary>
    /// <param name="value"></param>
    protected override void DoSetValue(T value)
    {
      throw new InvalidOperationException("Изменение значения константы не допускается");
    }

    /// <summary>
    /// Текстовое представление "Константа XXX"
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return "Константа " + Value.ToString();
    }

    #endregion
  }


  /// <summary>
  /// Реализация DepValue, позволяющая устанавливать значения "снаружи" вручную,
  /// или с помощью источника данных Source
  /// </summary>
  /// <typeparam name="T">Тип хранимого значения</typeparam>
  [Serializable]
  public class DepInput<T> : DepValue<T>
  {
    #region Извещение об изменении значения внешним источником

    internal void SetValueChanged()
    {
      T NewVal;
      if (Source == null)
        NewVal = default(T);
      else
        NewVal = Source.Value;

      // 09.11.2009
      // Проверки не нужны, т.к.:
      // - при установке Value проверка будет выполнена повторно;
      // - в DepInputWithCheck может потребоваться обработка для значения, которое
      // - совпадает с базовым
      //if (NewVal == null && base.Value == null)
      //  return;
      //if (IsEqualValues(NewVal, base.Value))
      //  return;

      // Значение на входе изменилось
      base.Value = NewVal;
    }

    #endregion

    #region Переопределяемый метод

    /// <summary>
    /// Вызывает BaseSetValue()
    /// </summary>
    /// <param name="value"></param>
    protected override void DoSetValue(T value)
    {
      BaseSetValue(value);
    }

    #endregion

    #region Источник данных

    /// <summary>
    /// Источник данных.
    /// Установка свойства приводит к "отцеплению" объекта от текущего источника, если он был, 
    /// и присоединению к новому источнику, если задано значение, отличное от null
    /// </summary>
    public override DepValue<T> Source
    {
      get
      {
        return _Source;
      }
      set
      {
        if (value == _Source)
          return;
        if (_Source != null)
          _Source.RemoveOutput(this);
        _Source = value;
        if (_Source != null)
          _Source.AddOutput(this);
        SetValueChanged();
      }
    }
    private DepValue<T> _Source;

    /// <summary>
    /// Возвращает свойство Source
    /// </summary>
    /// <returns></returns>
    public override DepValue<T> GetSource()
    {
      return _Source;
    }

    /// <summary>
    /// Используется для связанного списка
    /// </summary>
    internal DepInput<T> NextOutput;

    #endregion
  }

  #region Делегат для события DepInputDelayed.ValueNeeded

  /// <summary>
  /// Аргумент события DepInputDelayed.ValueNeeded
  /// </summary>
  /// <typeparam name="T">Тип значения</typeparam>
  public class DepValueNeededEventArgs<T> : EventArgs
  {
    /// <summary>
    /// Сюда надо поместить значение
    /// </summary>
    public T Value { get { return _Value; } set { _Value = value; } }
    private T _Value;
  }

  /// <summary>
  /// Делегат события DepInputDelayed.ValueNeeded
  /// </summary>
  /// <typeparam name="T">Тип значения</typeparam>
  /// <param name="sender">Источник события</param>
  /// <param name="args">Аргументы события</param>
  public delegate void DepValueNeededEventHandler<T>(object sender, DepValueNeededEventArgs<T> args);

  #endregion

  /// <summary>
  /// Реализация DepInput, обеспечивающая отложенное получение значения, когда
  /// Source не установлен.
  /// Этот класс не является сериализуемым
  /// </summary>
  /// <typeparam name="T">Тип хранимого значения</typeparam>
  public class DepInputDelayed<T> : DepInput<T>
  {
    #region Установка и получение отложенного значения

    /// <summary>
    /// Установка признака наличия отложенного значения
    /// </summary>
    public new void SetDelayed()
    {
      if (Source == null)
        base.SetDelayed();
    }

    /// <summary>
    /// Вызывается, когда требуется получить отложенное значение
    /// </summary>
    public event DepValueNeededEventHandler<T> ValueNeeded;

    /// <summary>
    /// Чтобы не создавать каждый раз объект аргументов
    /// </summary>
    private DepValueNeededEventArgs<T> ValueNeededArgs = new DepValueNeededEventArgs<T>();

    /// <summary>
    /// Вызывает событие ValueNeeded
    /// </summary>
    /// <returns></returns>
    protected override T GetDelayedValue()
    {
      if (ValueNeeded == null)
        throw new InvalidOperationException("Обработчик ValueNeeded не установлен");
      ValueNeededArgs.Value = default(T);
      ValueNeeded(this, ValueNeededArgs);
      return ValueNeededArgs.Value;
    }

    #endregion
  }

  #region DepInputCheckEventHandler

  /// <summary>
  /// Аргументы события DepInputWithCheck.CheckValue
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class DepInputCheckEventArgs<T> : CancelEventArgs
  {
    #region Конструктор

    /// <summary>
    /// Создает новый аргумент.
    /// Вызывается из DepInputWithCheck
    /// </summary>
    /// <param name="owner">Владелец</param>
    public DepInputCheckEventArgs(DepValue<T> owner)
    {
      _Owner = owner;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Значение которое будет установлено. Обработчик может изменить это значение.
    /// Если установить его равным CurrValue, то значения устанавливаться не будет
    /// </summary>
    public T NewValue { get { return _NewValue; } set { _NewValue = value; } }
    private T _NewValue;

    /// <summary>
    /// Получить текущее значение, действующее до установки
    /// </summary>
    public T CurrValue { get { return _Owner.Value; } }

    private readonly DepValue<T> _Owner;

    #endregion
  }

  /// <summary>
  /// Делегат события DepInputWithCheck.CheckValue
  /// </summary>
  /// <typeparam name="T">Тип значения</typeparam>
  /// <param name="sender">Источник события</param>
  /// <param name="args">Аргументы события</param>
  public delegate void DepInputCheckEventHandler<T>(object sender,
    DepInputCheckEventArgs<T> args);

  #endregion

  /// <summary>
  /// Расширение класса DepInput добавлением проверки устанавливаемого значения
  /// с помощью события CheckValue.
  /// Этот класс не является сериализуемым
  /// </summary>
  /// <typeparam name="T">Тип хранимого значения</typeparam>
  public class DepInputWithCheck<T> : DepInput<T>
  {
    #region Конструктор

    /// <summary>
    /// Создает новый объект
    /// </summary>
    public DepInputWithCheck()
    {
      _CheckValueArgs = new DepInputCheckEventArgs<T>(this);
    }

    #endregion

    #region Событие CheckValue

    /// <summary>
    /// Событие вызывается при установке значения.
    /// Пользовательский обработчик может изменить значение NewValue
    /// </summary>
    public event DepInputCheckEventHandler<T> CheckValue;

    private DepInputCheckEventArgs<T> _CheckValueArgs; // чтобы каждый раз не создавать

    #endregion

    #region Переопределенный метод

    /// <summary>
    /// Вызывает событие CheckValue, если есть обработчик события
    /// </summary>
    /// <param name="value"></param>
    protected override void DoSetValue(T value)
    {
      if (InsideSetValue) // на всякий случай
        return;

      if (CheckValue != null)
      {
        _CheckValueArgs.NewValue = value;
        _CheckValueArgs.Cancel = false;
        CheckValue(this, _CheckValueArgs);
        if (_CheckValueArgs.Cancel)
          return;
        value = _CheckValueArgs.NewValue;
      }

      base.DoSetValue(value);
    }

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Установка значения в обход основного входа
    /// </summary>
    /// <param name="value"></param>
    public void OwnerSetValue(T value)
    {
      base.DoSetValue(value);
    }

    #endregion
  }
}
