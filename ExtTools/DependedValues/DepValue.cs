using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.InteropServices;
using FreeLibSet.Core;

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

namespace FreeLibSet.DependedValues
{

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
    object Value { get;}

    /// <summary>
    /// Возвращает true, если текущий объект является константой
    /// </summary>
    bool IsConst { get;}

    /// <summary>
    /// true, если в настоящее время выполняется установка значения
    /// </summary>
    bool InsideSetValue { get; }

    /// <summary>
    /// Извещение посылается при изменении значения свойства Value
    /// </summary>
    event EventHandler ValueChanged;

    #endregion
  }

  /// <summary>
  /// Абстрактный базовый класс для типизированного доступа к значению.
  /// Свойства объектов объявляются с этим типом.
  /// Неабстрактные реализации - классы DepInput (или производных), DepValueObject и DepConst.
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
    /// Текущее значение.
    /// Переопределяется в DepInputDelayed
    /// </summary>
    public T Value { get { return GetValue(); } }
    private T _Value;

    /// <summary>
    /// Текущее значение.
    /// Переопределяется в DepInputDelayed
    /// </summary>
    protected virtual T GetValue()
    {
      return _Value;
    }

    object IDepValue.Value { get { return this.Value; } }

    /// <summary>
    /// Защищенный метод установки значения.
    /// Устанавливает поле и вызывает событие ValueChanged. После этого вызывается DepInput.SetValueChanged() для всех входов, подключенных к этому элементу
    /// На время вызова события и SetValueChanged() устанавливается свойство InseideSetValue.
    /// Реентрантные вызовы ничего не делают.
    /// Если <paramref name="value"/> совпадает с текущим значением, никаих действий не выполняется.
    /// </summary>
    /// <param name="value">Устанавливаемое значение</param>
    /// <param name="forced">Если true, то не выполняется проверка на равенство с текущим значением</param>
    protected void BaseSetValue(T value, bool forced)
    {
      // Можно было бы переименовать в SetValue() и сделать virtual, но так понятнее, когда выполняется отладка.

      if (forced)
      {
        if (_InsideSetValue)
          throw new ReenteranceException();
      }
      else
      {
        // Предотвращаем зациклинивание, когда установка значения приводит через
        // цепочку значений к повторной установке этого же значения
        if (_InsideSetValue)
          return;

        if (Object.Equals(value, _Value))
          return;
      }

      _InsideSetValue = true;
      try
      {
        _Value = value;
        OnValueChanged();
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

    #endregion

    #region Событие

    /// <summary>
    /// Извещение посылается при изменении значения свойства ValueEx
    /// </summary>
    public event EventHandler ValueChanged;

    /// <summary>
    /// Вызов события ValueChanged.
    /// Затем извещаются объекты DepInput, подключенные к текущему объекту
    /// </summary>
    public void OnValueChanged()
    {
      if (ValueChanged != null)
        ValueChanged(this, EventArgs.Empty);

      DepInput<T> CurrOutput = FirstOutput;
      while (CurrOutput != null)
      {
        CurrOutput.SetValueChanged();
        CurrOutput = CurrOutput.NextOutput;
      }
    }

    #endregion

    #region Вспомогательные свойства

    /// <summary>
    /// Возвращает true для DepInput, если он подключен к внешнему источнику данных.
    /// Непереопределенный метод возвращает false.
    /// </summary>
    public virtual bool HasSource { get { return false; } }

    /// <summary>
    /// Возвращает true, если текущий объект соединен с другими,
    /// то есть имеет имеет источник (HasSource=true) или есть объекты, подключенные к текущему (HasOutputs=true)
    /// </summary>
    public bool IsConnected { get { return HasOutputs || HasSource; } }

    /// <summary>
    /// Является ли текущее значение константой.
    /// Непереопределенный метод возвращает false.
    /// </summary>
    public virtual bool IsConst { get { return false; } }

    #endregion

    #region Вспомогательные методы
#if XXX
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

#endif
    #endregion

    #region Список зависимых значений

    // Зависимые значения строятся с помощью связанного списка
    // Используются свойства DepValue.FirstOutput и DepInput.NextOutput

    internal DepInput<T> FirstOutput;

#if XXX
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
#endif

    /// <summary>
    /// Свойство возвращает true, если к данному DepValue подключены один или несколько выходов.
    /// Свойство предназначено для отладочных целей.
    /// </summary>
    public bool HasOutputs { get { return FirstOutput != null; } }

    /// <summary>
    /// Возвращает массив зависимых объектов, присоединенных к данному DepValue.
    /// Свойство предназначено для отладочных целей.
    /// </summary>
    /// <returns>Массив выходов</returns>
    public DepInput<T>[] Outputs
    {
      get
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
  /// Неабстрактная реализация DepValue.
  /// Используется для "выходных" свойств, предназначенных только для чтения, но не для изменения внешним кодом
  /// </summary>
  /// <typeparam name="T">Тип хранимого значения</typeparam>
  [Serializable]
  public class DepValueObject<T> : DepValue<T>
  {
    #region Установка значения

    /// <summary>
    /// Этот метод вызывается объектом-владельцем для установки значения
    /// </summary>
    /// <param name="value"></param>
    public void OwnerSetValue(T value)
    {
      BaseSetValue(value, false);
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
      BaseSetValue(value, false);
    }

    #endregion

    #region Переопределяемые методы

    /// <summary>
    /// Текстовое представление "Константа XXX"
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return "Константа " + Value.ToString();
    }

    /// <summary>
    /// Возвращает true
    /// </summary>
    public override bool IsConst { get { return true; } }

    #endregion

    #region Статические методы

    /// <summary>
    /// Создает массив констант из массива значений.
    /// </summary>
    /// <param name="values">Массив значений</param>
    /// <returns>Массив констант</returns>
    public static DepConst<T>[] CreateArray(T[] values)
    {
      if (values == null)
        return null;
      DepConst<T>[] a = new DepConst<T>[values.Length];
      for (int i = 0; i < values.Length; i++)
        a[i] = new DepConst<T>(values[i]);
      return a;
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
    #region Установка значения

    /// <summary>
    /// Чтение и установка значения
    /// </summary>
    public new T Value
    {
      get { return GetValue(); }
      set { SetValue(value); }
    }

    /// <summary>
    /// Вызов BaseSetValue().
    /// Переопределяется в DepInputDelayed и DepInputWithCheck
    /// </summary>
    /// <param name="value"></param>
    protected virtual void SetValue(T value)
    {
      BaseSetValue(value, false);
    }

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
      //BaseSetValue(NewVal);
      SetValue(NewVal); // 21.10.2021. Иначе не будет работать DepInputWithCheck()
    }

    #endregion

    #region Источник данных

    /// <summary>
    /// Источник данных.
    /// Установка свойства приводит к "отцеплению" объекта от текущего источника, если он был, 
    /// и присоединению к новому источнику, если задано значение, отличное от null
    /// </summary>
    public DepValue<T> Source
    {
      get { return _Source; }
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
    /// Используется для связанного списка
    /// </summary>
    internal DepInput<T> NextOutput;

    /// <summary>
    /// Возвращает true, если текущий объект подключен к источнику данных (Source!=null)
    /// </summary>
    public override bool HasSource { get { return _Source != null; } }

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
  /// Реализация DepInput, обеспечивающая отложенное получение значения, когда внешний источник Source не установлен.
  /// Этот класс не является сериализуемым
  /// </summary>
  /// <typeparam name="T">Тип хранимого значения</typeparam>
  public class DepInputDelayed<T> : DepInput<T>
  {
    #region Установка и получение отложенного значения

    /// <summary>
    /// Получение текущего значения.
    /// Вызывает GetDelayedValue(), если был вызов SetDelayed().
    /// </summary>
    /// <returns></returns>
    protected override T GetValue()
    {
      if (_Delayed)
      {
        BaseSetValue(GetDelayedValue(), false);
        _Delayed = false;
      }
      return base.GetValue();
    }

    /// <summary>
    /// Установка текущего значения.
    /// Отменяет предыдущий вызов SetDelayed().
    /// </summary>
    /// <param name="value"></param>
    protected override void SetValue(T value)
    {
      _Delayed = false;
      base.SetValue(value);
    }

    /// <summary>
    /// Признак того, что значение будет получено позже, если понадобится
    /// </summary>
    private bool _Delayed;

    /// <summary>
    /// Уставливает признак отложенной установки в true.
    /// Посылает событие ValueChanged текущему объекту и зависимым объектам.
    /// Если зависимым объектам потребуется текущее значение, они запросят свойство Value,
    /// при этом будет вызван метод ValueNeeded.
    /// Если нет подключенных зависимых объектов, получение текущего значения откладывается.
    /// </summary>
    public void SetDelayed()
    {
#if DEBUG
      if (ValueNeeded == null)
        throw new InvalidOperationException("Обработчик ValueNeeded не установлен");
#endif


      if (InsideSetValue)
        return;

      if (Source == null)
      {
        _Delayed = true;

        OnValueChanged();
      }
    }

    /// <summary>
    /// Вызывается, когда требуется получить отложенное значение
    /// </summary>
    public event DepValueNeededEventHandler<T> ValueNeeded;

    /// <summary>
    /// Чтобы не создавать каждый раз объект аргументов
    /// </summary>
    private DepValueNeededEventArgs<T> _ValueNeededArgs;

    /// <summary>
    /// Вызывает событие ValueNeeded
    /// </summary>
    /// <returns></returns>
    private T GetDelayedValue()
    {
#if DEBUG
      if (ValueNeeded == null)
        throw new InvalidOperationException("Обработчик ValueNeeded не установлен");
#endif

      if (_ValueNeededArgs == null)
        _ValueNeededArgs = new DepValueNeededEventArgs<T>();
      _ValueNeededArgs.Value = default(T);
      ValueNeeded(this, _ValueNeededArgs);
      return _ValueNeededArgs.Value;
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

    /// <summary>
    /// Если установить в true, то будет выполнена принудительная установка значения без предварительной проверки
    /// на равенство. Используется в реализации свойства ValueEx, когда есть связанное свойство NValue, новое значение равно 0 при существующем NValue=null.
    /// </summary>
    public bool Forced { get { return _Forced; } set { _Forced = value; } }
    private bool _Forced;

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
  /// Расширение класса DepInput добавлением проверки устанавливаемого значения с помощью события CheckValue.
  /// Этот класс не является сериализуемым.
  /// Используется, например, в EFPDateTimeBox, для обрезки компонента времени в DateTime.
  /// </summary>
  /// <typeparam name="T">Тип хранимого значения</typeparam>
  public class DepInputWithCheck<T> : DepInput<T>
  {
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
    /// Установка текущего значения.
    /// Вызывает обработчик события CheckValue, если он установлен.
    /// Обработчик события может изменить устанавливаемое значение или совсем отменить установку.
    /// </summary>
    /// <param name="value"></param>
    protected override void SetValue(T value)
    {
      if (InsideSetValue) // чтобы не выполнять лишней проверки
        return;

      bool forced=false;
      if (CheckValue != null)
      {
        if (_CheckValueArgs == null)
          _CheckValueArgs = new DepInputCheckEventArgs<T>(this);

        _CheckValueArgs.NewValue = value;
        _CheckValueArgs.Cancel = false;
        _CheckValueArgs.Forced = false;
        CheckValue(this, _CheckValueArgs);
        if (_CheckValueArgs.Cancel)
          return;
        value = _CheckValueArgs.NewValue;
        forced = _CheckValueArgs.Forced;
      }

      base.BaseSetValue(value, forced);
    }

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Установка значения в обход основного входа
    /// </summary>
    /// <param name="value"></param>
    public void OwnerSetValue(T value)
    {
      base.BaseSetValue(value, false);
    }

    #endregion
  }
}
