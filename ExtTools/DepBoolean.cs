using AgeyevAV.DependedValues;
using System;
using System.Collections.Generic;
using System.Text;

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
  /// Базовый класс для DepAnd и DepOr
  /// </summary>
  [Serializable]
  public abstract class DepAndOrBase : DepValueObject<Boolean>
  {
    #region Конструктор

    /// <summary>
    /// Защищенный конструктор
    /// </summary>
    protected DepAndOrBase()
    {
      _Inputs = new List<DepInput<Boolean>>();
    }

    #endregion

    #region Методы

    /// <summary>
    /// Добавляет зависимое значение
    /// </summary>
    /// <param name="depenedValue">Добавляемое значение. 
    /// Если null, то никаких действий не выполняется</param>
    public void Add(DepValue<Boolean> depenedValue)
    {
      if (depenedValue == null)
        return;

      DepInput<Boolean> NewInput = new DepInput<Boolean>();
      NewInput.OwnerInfo=new DepOwnerInfo(this, "["+_Inputs.Count.ToString()+"]");
      NewInput.Source = depenedValue;
      NewInput.ValueChanged += new EventHandler(SourceValueChanged);

      _Inputs.Add(NewInput);
      SourceValueChanged(null, null);
    }

    /// <summary>
    /// Удаляет зависимое значение.
    /// Этот метод вряд ли понадобится.
    /// </summary>
    /// <param name="depenedValue">Удялемое значение.
    /// Если null, то никаких действий не выполняется</param>
    public void Remove(DepValue<Boolean> depenedValue)
    {
      if (depenedValue == null)
        return;

      for (int i = 0; i < _Inputs.Count; i++)
      {
        if (_Inputs[i].Source == depenedValue)
        {
          _Inputs.RemoveAt(i);
          break;
        }
      }
      SourceValueChanged(null, null);
    }

    /// <summary>
    /// Список входных значений
    /// </summary>
    protected List<DepInput<Boolean>> Inputs { get { return _Inputs; } }
    private List<DepInput<Boolean>> _Inputs;

    private void SourceValueChanged(object sender, EventArgs args)
    {
      DoCalc();
    }

    /// <summary>
    /// Абстрактный метод, выполняющий расчет значения
    /// </summary>
    protected abstract void DoCalc();

    #endregion
  }

  /// <summary>
  /// Логическое "И"
  /// </summary>
  [Serializable]
  public sealed class DepAnd : DepAndOrBase
  {
    #region Конструкторы

    /// <summary>
    /// Создает пустой объект, для которого надо будет вызвать метод Add()
    /// </summary>
    public DepAnd()
    {
    }

    /// <summary>
    /// Создает объект, реализующий функцию AND для двух аргументов
    /// </summary>
    /// <param name="a">Первое значение</param>
    /// <param name="b">Второе значение</param>
    public DepAnd(DepValue<Boolean> a, DepValue<Boolean> b)
    {
      Add(a);
      Add(b);
    }

    /// <summary>
    /// Создает объект, реализующий функцию AND для произвольного числав вргументов
    /// </summary>
    /// <param name="a">Массив аргументов</param>
    public DepAnd(params DepValue<Boolean>[] a)
    {
      for (int i = 0; i < a.Length; i++)
        Add(a[i]);
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Вычисляет функцию AND
    /// </summary>
    protected override void DoCalc()
    {
      if (Inputs.Count == 0)
        OwnerSetValue(true);
      else
      {
        bool x = Inputs[0].Value;
        for (int i = 1; i < Inputs.Count; i++)
          x = x & Inputs[i].Value;
        OwnerSetValue(x);
      }
    }

    #endregion

    #region ToString()

    /// <summary>
    /// Возвращает "AND", если свойство OwnerInfo не установлено
    /// </summary>
    /// <returns>Текстовое представление для отладки</returns>
    public override string ToString()
    {
      if (OwnerInfo.Owner == null)
        return "AND";
      else
        return base.ToString();
    }

    #endregion

    #region Статические методы

    /// <summary>
    /// Присоединение ко входу с логикой "И" к существующим источникам.
    /// Если <paramref name="resValue"/> не имеет источника, то <paramref name="srcValue"/> присоединяется в качестве источника
    /// непосредственно. Если источник есть, проверяется, не является ли он DepAnd. Если
    /// является, то проверяется, имеет ли он только один выход на ResValue, или есть другие
    /// объекты, которые от него зависят. Если <paramref name="resValue"/> - единственный выход DepAnd, то <paramref name="srcValue"/>
    /// добавляется к нему. Иначе создается новый объект DepAnd, существующий источник
    /// переключается на него, а затем добавляется <paramref name="srcValue"/>.
    /// </summary>
    /// <param name="resValue">Зависимое значение</param>
    /// <param name="srcValue">Исходное значение</param>
    public static void AttachInput(DepValue<Boolean> resValue, DepValue<Boolean> srcValue)
    {
      if (resValue == null)
        throw new ArgumentNullException("resValue");
      if (srcValue == null)
        throw new ArgumentNullException("srcValue");

      if (resValue.Source == null)
      {
        resValue.Source = srcValue;
        return;
      }

      if (resValue.Source is DepAnd)
      {
        if (resValue.Source.OutputCount == 1)
        {
          ((DepAnd)(resValue.Source)).Add(srcValue);
          return;
        }
      }

      // Добавляем новый AND
      DepAnd NewAnd = new DepAnd();
      NewAnd.Add(resValue.Source);
      NewAnd.Add(srcValue);
      resValue.Source = NewAnd;
    }

    #endregion
  }

  /// <summary>
  /// Логическое "ИЛИ"
  /// </summary>
  [Serializable]
  public sealed class DepOr : DepAndOrBase
  {
    #region Конструкторы

    /// <summary>
    /// Создает пустой объект, для которого нужно будет вызвать метод Add()
    /// </summary>
    public DepOr()
    {
    }

    /// <summary>
    /// Создает объект, вычисляющую функцию OR для двух аргументов
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    public DepOr(DepValue<Boolean> a, DepValue<Boolean> b)
    {
      Add(a);
      Add(b);
    }
  
    /// <summary>
    /// Создает объект, реализующий функцию OR для произвольного числав вргументов
    /// </summary>
    /// <param name="a">Массив аргументов</param>
    public DepOr(params DepValue<Boolean>[] a)
    {
      for (int i = 0; i < a.Length; i++)
        Add(a[i]);
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Вычисляет функцию OR
    /// </summary>
    protected override void DoCalc()
    {
      if (Inputs.Count == 0)
        OwnerSetValue(false);
      else
      {
        bool x = Inputs[0].Value;
        for (int i = 1; i < Inputs.Count; i++)
          x = x | Inputs[i].Value;
        OwnerSetValue(x);
      }
    }

    #endregion

    #region ToString()

    /// <summary>
    /// Возвращает "OR", если свойство OwnerInfo не установлено
    /// </summary>
    /// <returns>Текстовое представление для отладки</returns>
    public override string ToString()
    {
      if (OwnerInfo.Owner == null)
        return "OR";
      else
        return base.ToString();
    }

    #endregion

    #region Статические методы

    /// <summary>
    /// Присоединение ко входу с логикой "ИЛИ" к существующим источникам.
    /// Если <paramref name="resValue"/> не имеет источника, то <paramref name="srcValue"/> присоединяется в качестве источника
    /// непосредственно. Если источник есть, проверяется, не является ли он DepOr. Если
    /// является, то проверяется, имеет ли он только один выход на ResValue, или есть другие
    /// объекты, которые от него зависят. Если ResValue-единственный выход DepOr, то <paramref name="srcValue"/>
    /// добавляется к нему. Иначе создается новый объект DepOr, существующий источник
    /// переключается на него, а затем добавляется <paramref name="srcValue"/>.
    /// </summary>
    /// <param name="resValue">Зависимое значение</param>
    /// <param name="srcValue">Исходное значение</param>
    public static void AttachInput(DepValue<Boolean> resValue, DepValue<Boolean> srcValue)
    {
      if (resValue == null)
        throw new ArgumentNullException("resValue");
      if (srcValue == null)
        throw new ArgumentNullException("srcValue");

      if (resValue.Source == null)
      {
        resValue.Source = srcValue;
        return;
      }

      if (resValue.Source is DepOr)
      {
        if (resValue.Source.OutputCount == 1)
        {
          ((DepOr)(resValue.Source)).Add(srcValue);
          return;
        }
      }

      // Добавляем новый OR
      DepOr NewOr = new DepOr();
      NewOr.Add(resValue.Source);
      NewOr.Add(srcValue);
      resValue.Source = NewOr;
    }

    #endregion
  }

  /// <summary>
  /// Логическое "НЕ"
  /// </summary>
  [Serializable]
  public sealed class DepNot : DepValueObject<Boolean>
  {
    #region Конструкторы

    /// <summary>
    /// Создает объект с неприсоединенным исходным значением.
    /// </summary>
    public DepNot()
    {
      _Source = new DepInput<Boolean>();
      _Source.OwnerInfo = new DepOwnerInfo(this, "Source");
      _Source.ValueChanged += new EventHandler(SourceValueChanged);
      SourceValueChanged(null, null);
    }

    /// <summary>
    /// Создает объект, вычисляющий функцию NOT для заданного аргумента
    /// </summary>
    /// <param name="arg">Аргумент</param>
    public DepNot(DepValue<Boolean> arg)
    {
      _Source = new DepInput<Boolean>();
      _Source.OwnerInfo = new DepOwnerInfo(this, "Source");
      _Source.ValueChanged += new EventHandler(SourceValueChanged);
      _Source.Source = arg;
      SourceValueChanged(null, null);
    }

    #endregion

    #region Свойство Source

    /// <summary>
    /// Исходное значение (аргумент) для вычисления функции NOT
    /// </summary>
    public override DepValue<Boolean> Source
    {
      get
      {
        return _Source;
      }
      set
      {
        _Source.Source = value;
      }
    }
    private DepInput<Boolean> _Source;

    private void SourceValueChanged(object sender, EventArgs args)
    {
      OwnerSetValue(!Source.Value);
    }

    #endregion

    #region ToString()

    /// <summary>
    /// Возвращает "NOT", если свойство OwnerInfo не установлено
    /// </summary>
    /// <returns>Текстовое представление для отладки</returns>
    public override string ToString()
    {
      if (OwnerInfo.Owner == null)
        return "NOT";
      else
        return base.ToString();
    }

    #endregion

    #region Статические методы

    /// <summary>
    /// Добавление логического "НЕ" к выходу <paramref name="srcValue"/>.
    /// Среди выходов <paramref name="srcValue"/> выполняется поиск существующего объекта DepNot. Если
    /// найден, то он возвращается. Иначе создается новый объект DepNot
    /// </summary>
    /// <param name="srcValue"></param>
    /// <returns>Инвертированный выход</returns>
    public static DepNot NotOutput(DepValue<Boolean> srcValue)
    {
      if (srcValue == null)
        throw new ArgumentNullException("srcValue");

      DepInput<Boolean>[] Inputs = srcValue.GetOutputs();
      for (int i = 0; i < Inputs.Length; i++)
      {
        if (Inputs[i].OwnerInfo.Owner != null)
        {
          if (Inputs[i].OwnerInfo.Owner is DepNot)
            return (DepNot)(Inputs[i].OwnerInfo.Owner);
        }
      }
      // Нет подходящего объекта
      return new DepNot(srcValue);
    }

    #endregion
  }
  /// <summary>
  /// Сравнение двух значений
  /// Содержит значение true, когда аргументы Arg1 и Arg2 равны друг другу
  /// </summary>
  /// <typeparam name="T">Тип сравниваемых значений.</typeparam>
  [Serializable]
  public sealed class DepEqual<T> : DepValueObject<Boolean>
  {
    #region Конструкторы

    /// <summary>
    /// Создает пустой объект с неприсоединенными аргументами.
    /// Эта версия подразумевает, что будут установлены свойства Arg1 и Arg2
    /// </summary>
    public DepEqual()
    {
      DoInit();
    }

    /// <summary>
    /// Создает объект, выполняющий сравнение двух управляемых объектов.
    /// </summary>
    /// <param name="arg1">Первый аргумент</param>
    /// <param name="arg2">Второй аргумент</param>
    public DepEqual(DepValue<T> arg1, DepValue<T> arg2)
    {
      DoInit();
      this.Arg1 = arg1;
      this.Arg2 = arg2;
    }

    /// <summary>
    /// Создает объект, выполняющий сравнение управляемого значения с константой
    /// </summary>
    /// <param name="arg1">Первый аргумент</param>
    /// <param name="arg2">Второй аргумент (константа)</param>
    public DepEqual(DepValue<T> arg1, T arg2)
    {
      DoInit();
      this.Arg1 = arg1;
      DepValueObject<T> Arg2Obj = new DepValueObject<T>();
      Arg2Obj.OwnerInfo = new DepOwnerInfo(this, "Arg2 (const)");
      Arg2Obj.OwnerSetValue(arg2);
      this.Arg2 = Arg2Obj;
    }

    private void DoInit()
    {
      _Arg1 = new DepInput<T>();
      _Arg1.OwnerInfo = new DepOwnerInfo(this, "Arg1");
      _Arg1.ValueChanged += new EventHandler(ArgValueChanged);

      _Arg2 = new DepInput<T>();
      _Arg2.OwnerInfo = new DepOwnerInfo(this, "Arg2");
      _Arg2.ValueChanged += new EventHandler(ArgValueChanged);

      // Сразу устанавливаем значение
      ArgValueChanged(null, null);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Первый аргумент
    /// </summary>
    public DepValue<T> Arg1
    {
      get { return _Arg1; }
      set { _Arg1.Source = value; }
    }
    private DepInput<T> _Arg1;

    /// <summary>
    /// Второй аргумент
    /// </summary>
    public DepValue<T> Arg2
    {
      get { return _Arg2; }
      set { _Arg2.Source = value; }
    }
    private DepInput<T> _Arg2;

    #endregion

    #region ToString()

    /// <summary>
    /// Возвращает "EQUAL", если свойство OwnerInfo не установлено
    /// </summary>
    /// <returns>Текстовое представление для отладки</returns>
    public override string ToString()
    {
      if (OwnerInfo.Owner == null)
        return "EQUAL";
      else
        return base.ToString();
    }

    #endregion

    #region Внутренняя реализация

    private void ArgValueChanged(object sender, EventArgs args)
    {
      OwnerSetValue(DepValue<T>.IsEqualValues(Arg1.Value, Arg2.Value));
    }

    #endregion
  }

  /// <summary>
  /// Определяет наличие элемента в массиве.
  /// Возвращает true, если значение присутствует в массиве.
  /// Массив является фиксированным.
  /// Применяется, например, если требуется делать элемент управления доступным, когда выбрана одна из нескольких радиокнопок (Вместо комбинации DepOr с DepEqual).
  /// </summary>
  /// <typeparam name="T">Тип значения</typeparam>
  [Serializable]
  public sealed class DepInArray<T> : DepExpr1<bool, T>
  {
    #region Конструктор

    /// <summary>
    /// Создает объект.
    /// </summary>
    /// <param name="arg">Проверяемое значение</param>
    /// <param name="items">Массив элементов для поиска. Не может быть null</param>
    public DepInArray(DepValue<T> arg, T[] items)
    {
      if (items == null)
        throw new ArgumentNullException("items");
      _Items = items;
      base.Init(arg);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Массив, в котором выполняется поиск.
    /// Задается в конструкторе. Не может быть null (хотя элементы могут быть null)
    /// </summary>
    public T[] Items { get { return _Items; } }
    private readonly T[] _Items;

    #endregion

    #region Переопределенный метод

    /// <summary>
    /// Вычисляет выражение, используя Array.IndexOf()
    /// </summary>
    /// <returns>true, если значение находится в массиве</returns>
    protected override bool Calculate()
    {
      if (_Items == null)
        return false;
      return Array.IndexOf<T>(_Items, Source.Value) >= 0;
    }

    #endregion

    #region ToString()

    /// <summary>
    /// Возвращает "INARRAY", если свойство OwnerInfo не установлено
    /// </summary>
    /// <returns>Текстовое представление для отладки</returns>
    public override string ToString()
    {
      if (OwnerInfo.Owner == null)
        return "INARRAY";
      else
        return base.ToString();
    }

    #endregion
  }
}
