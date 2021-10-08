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
      NewInput.OwnerInfo = new DepOwnerInfo(this, "[" + _Inputs.Count.ToString() + "]");
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
      _Arg1 = new DepInput<T>();
      _Arg1.OwnerInfo = new DepOwnerInfo(this, "Arg1");
      _Arg1.ValueChanged += new EventHandler(ArgValueChanged);

      _Arg2 = new DepInput<T>();
      _Arg2.OwnerInfo = new DepOwnerInfo(this, "Arg2");
      _Arg2.ValueChanged += new EventHandler(ArgValueChanged);

      // Сразу устанавливаем значение
      ArgValueChanged(null, null);
    }

    /// <summary>
    /// Создает объект, выполняющий сравнение двух управляемых объектов.
    /// </summary>
    /// <param name="arg1">Первый аргумент</param>
    /// <param name="arg2">Второй аргумент</param>
    public DepEqual(DepValue<T> arg1, DepValue<T> arg2)
      : this()
    {
      this.Arg1 = arg1;
      this.Arg2 = arg2;
    }

    /// <summary>
    /// Создает объект, выполняющий сравнение управляемого значения с константой
    /// </summary>
    /// <param name="arg1">Первый аргумент</param>
    /// <param name="arg2">Второй аргумент (константа)</param>
    public DepEqual(DepValue<T> arg1, T arg2)
      : this(arg1, new DepConst<T>(arg2))
    {
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
      OwnerSetValue(Object.Equals(Arg1.Value, Arg2.Value));
    }

    #endregion
  }


  #region Перечисление DepCompareKind

  /// <summary>
  /// Режимы сравнения поля с константой для фильтра ValueFilter
  /// </summary>
  [Serializable]
  public enum DepCompareKind
  {
    /// <summary>
    /// Сравнение на равенство
    /// </summary>
    Equal,

    /// <summary>
    /// "Меньше"
    /// </summary>
    LessThan,

    /// <summary>
    /// "Меньше или равно"
    /// </summary>
    LessOrEqualThan,

    /// <summary>
    /// "Больше"
    /// </summary>
    GreaterThan,

    /// <summary>
    /// Больше или равно"
    /// </summary>
    GreaterOrEqualThan,

    /// <summary>
    /// "Не равно"
    /// </summary>
    NotEqual,
  }

  #endregion


  /// <summary>
  /// Сравнение двух значений, с поддержкой шести операций сравнения.
  /// Содержит результат сравнения аргументов Arg1 и Arg2.
  /// В отличие от шаблонного класса DepEqual, для DepCompare требуется, чтобы тип <typeparamref name="T"/> поддерживал сравнение на "больше/меньше".
  /// Для этого, либо класс должен реализовывать интерфейс IComparable of T, либо должно быть задано свойство Comparer.
  /// </summary>
  /// <typeparam name="T">Тип сравниваемых значений</typeparam>
  [Serializable]
  public sealed class DepComparer<T> : DepValueObject<Boolean>
  {
    #region Конструкторы

    /// <summary>
    /// Создает пустой объект с неприсоединенными аргументами.
    /// Эта версия подразумевает, что будут установлены свойства Arg1, Arg2 и Kind.
    /// Свойство Kind имеет значение Equal
    /// </summary>
    public DepComparer()
    {
      _Arg1 = new DepInput<T>();
      _Arg1.OwnerInfo = new DepOwnerInfo(this, "Arg1");
      _Arg1.ValueChanged += new EventHandler(ArgValueChanged);

      _Arg2 = new DepInput<T>();
      _Arg2.OwnerInfo = new DepOwnerInfo(this, "Arg2");
      _Arg2.ValueChanged += new EventHandler(ArgValueChanged);

      _Kind = DepCompareKind.Equal;

      // Сразу устанавливаем значение
      OwnerSetValue(true);
    }

    /// <summary>
    /// Создает объект, выполняющий сравнение двух управляемых объектов.
    /// </summary>
    /// <param name="arg1">Первый аргумент</param>
    /// <param name="arg2">Второй аргумент</param>
    /// <param name="kind">Операция сравнения</param>
    public DepComparer(DepValue<T> arg1, DepValue<T> arg2, DepCompareKind kind)
      : this()
    {
      this.Arg1 = arg1;
      this.Arg2 = arg2;
      this.Kind = kind;
    }

    /// <summary>
    /// Создает объект, выполняющий сравнение управляемого значения с константой
    /// </summary>
    /// <param name="arg1">Первый аргумент</param>
    /// <param name="arg2">Второй аргумент (константа)</param>
    /// <param name="kind">Операция сравнения</param>
    public DepComparer(DepValue<T> arg1, T arg2, DepCompareKind kind)
      : this(arg1, new DepConst<T>(arg2), kind)
    {
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

    /// <summary>
    /// Операция сравнения
    /// </summary>
    public DepCompareKind Kind
    {
      get { return _Kind; }
      set
      {
        if (value == _Kind)
          return;
        _Kind = value;
        ArgValueChanged(null, null);
      }
    }
    private DepCompareKind _Kind;

    /// <summary>
    /// Сравниватель для значений.
    /// Если свойство не установлено в явном виде, возвращается Comparer of T.Default
    /// </summary>
    public IComparer<T> Comparer
    {
      get
      {
        if (_Comparer == null)
          return Comparer<T>.Default;
        else
          return _Comparer;
      }
      set
      {
        if (Object.ReferenceEquals(value, _Comparer))
          return;
        _Comparer = value;
        ArgValueChanged(null, null);
      }
    }
    private IComparer<T> _Comparer;

    #endregion

    #region ToString()

    /// <summary>
    /// Возвращает "OPERATOR xxx", если свойство OwnerInfo не установлено
    /// </summary>
    /// <returns>Текстовое представление для отладки</returns>
    public override string ToString()
    {
      if (OwnerInfo.Owner == null)
        return "OPERATOR " + Kind.ToString();
      else
        return base.ToString();
    }

    #endregion

    #region Внутренняя реализация

    private void ArgValueChanged(object sender, EventArgs args)
    {
      OwnerSetValue(Compare(Arg1.Value, Arg2.Value, Kind, Comparer));
    }

    internal static bool Compare(T value1, T value2, DepCompareKind kind, IComparer<T> comparer)
    {
      switch (kind)
      {
        case DepCompareKind.Equal: return comparer.Compare(value1, value2) == 0;
        case DepCompareKind.LessThan: return comparer.Compare(value1, value2) < 0;
        case DepCompareKind.LessOrEqualThan: return comparer.Compare(value1, value2) <= 0;
        case DepCompareKind.GreaterThan: return comparer.Compare(value1, value2) > 0;
        case DepCompareKind.GreaterOrEqualThan: return comparer.Compare(value1, value2) >= 0;
        case DepCompareKind.NotEqual: return comparer.Compare(value1, value2) != 0;
        default:
          throw new ArgumentException("Неизвестная операция " + kind.ToString(), "kind");
      }
    }

    #endregion
  }

  /// <summary>
  /// Проверка попадания значения в диапазон.
  /// Тип <typeparamref name="T"/> должен реализовывать интерфейс IComparable of T, либо должно быть задано свойство Comparer.
  /// Полуоткрытые интервалы не поддерживаются. Используйте класс DepComparer.
  /// </summary>
  /// <typeparam name="T">Тип сравниваемых значений</typeparam>
  [Serializable]
  public sealed class DepInRange<T> : DepValueObject<Boolean>
  {
    #region Конструкторы

    /// <summary>
    /// Создает пустой объект с неприсоединенными аргументами.
    /// Эта версия подразумевает, что будут установлены свойства Arg1, Arg2 и Kind.
    /// Свойство Kind имеет значение Equal
    /// </summary>
    public DepInRange()
    {
      _TestedValue = new DepInput<T>();
      _TestedValue.OwnerInfo = new DepOwnerInfo(this, "Value");
      _TestedValue.ValueChanged += new EventHandler(ArgValueChanged);

      _Minimum = new DepInput<T>();
      _Minimum.OwnerInfo = new DepOwnerInfo(this, "Minimum");
      _Minimum.ValueChanged += new EventHandler(ArgValueChanged);

      _Maximum = new DepInput<T>();
      _Maximum.OwnerInfo = new DepOwnerInfo(this, "Maximum");
      _Maximum.ValueChanged += new EventHandler(ArgValueChanged);

      // Сразу устанавливаем значение
      OwnerSetValue(true);
    }

    /// <summary>
    /// Создает объект, выполняющий сравнение с управляемыми объектами.
    /// </summary>
    /// <param name="testedValue">Проверяемое значение</param>
    /// <param name="minimum">Минимальное значение</param>
    /// <param name="maximum">Максимальное значение</param>
    public DepInRange(DepValue<T> testedValue, DepValue<T> minimum, DepValue<T> maximum)
      : this()
    {
      this.TestedValue = testedValue;
      this.Minimum = minimum;
      this.Maximum = maximum;
    }

    /// <summary>
    /// Создает объект, выполняющий сравнение с константным диапазоном
    /// </summary>
    /// <param name="testedValue">Проверяемое значение</param>
    /// <param name="minimum">Минимальное значение</param>
    /// <param name="maximum">Максимальное значение</param>
    public DepInRange(DepValue<T> testedValue, T minimum, T maximum)
      : this(testedValue, new DepConst<T>(minimum), new DepConst<T>(maximum))
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Проверяемое значение
    /// </summary>
    public DepValue<T> TestedValue
    {
      get { return _TestedValue; }
      set { _TestedValue.Source = value; }
    }
    private DepInput<T> _TestedValue;

    /// <summary>
    /// Минимальное значение
    /// </summary>
    public DepValue<T> Minimum
    {
      get { return _Minimum; }
      set { _Minimum.Source = value; }
    }
    private DepInput<T> _Minimum;

    /// <summary>
    /// Максимальное значение
    /// </summary>
    public DepValue<T> Maximum
    {
      get { return _Maximum; }
      set { _Maximum.Source = value; }
    }
    private DepInput<T> _Maximum;

    /// <summary>
    /// Сравниватель для значений.
    /// Если свойство не установлено в явном виде, возвращается Comparer of T.Default
    /// </summary>
    public IComparer<T> Comparer
    {
      get
      {
        if (_Comparer == null)
          return Comparer<T>.Default;
        else
          return _Comparer;
      }
      set
      {
        if (Object.ReferenceEquals(value, _Comparer))
          return;
        _Comparer = value;
        ArgValueChanged(null, null);
      }
    }
    private IComparer<T> _Comparer;

    #endregion

    #region ToString()

    /// <summary>
    /// Возвращает "INRANGE", если свойство OwnerInfo не установлено
    /// </summary>
    /// <returns>Текстовое представление для отладки</returns>
    public override string ToString()
    {
      if (OwnerInfo.Owner == null)
        return "INRANGE";
      else
        return base.ToString();
    }

    #endregion

    #region Внутренняя реализация

    private void ArgValueChanged(object sender, EventArgs args)
    {
      OwnerSetValue(Comparer.Compare(TestedValue.Value, Minimum.Value)>=0 && Comparer.Compare(TestedValue.Value, Maximum.Value)<=0);
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


  /// <summary>
  /// Реализация условного оператора (функция IIF, тернарный оператор в CSharp), который возвращает одно из двух значений, в зависимости от третьего значения.
  /// </summary>
  /// <typeparam name="T">Тип значений</typeparam>
  [Serializable]
  public sealed class DepIf<T> : DepValueObject<T>
  {
    #region Конструкторы

    /// <summary>
    /// Создает пустой объект с неприсоединенными аргументами.
    /// Эта версия подразумевает, что будут установлены свойства ConditionArg, TrueArg и FalseArg
    /// </summary>
    public DepIf()
    {
      _ConditionArg = new DepInput<bool>();
      _ConditionArg.OwnerInfo = new DepOwnerInfo(this, "ConditionArg");
      _ConditionArg.ValueChanged += new EventHandler(ArgValueChanged);

      _TrueArg = new DepInput<T>();
      _TrueArg.OwnerInfo = new DepOwnerInfo(this, "TrueArg");
      _TrueArg.ValueChanged += new EventHandler(ArgValueChanged);

      _FalseArg = new DepInput<T>();
      _FalseArg.OwnerInfo = new DepOwnerInfo(this, "FalseArg");
      _FalseArg.ValueChanged += new EventHandler(ArgValueChanged);

      // Сразу устанавливаем значение
      ArgValueChanged(null, null);
    }

    /// <summary>
    /// Создает объект, выполняющий выбор из двух управляемых объектов.
    /// </summary>
    /// <param name="conditionArg">Управляющий аргумент</param>
    /// <param name="trueArg">Аргумент, значение которого используется, если <paramref name="conditionArg"/> возвращает true</param>
    /// <param name="falseArg">Аргумент, значение которого используется, если <paramref name="conditionArg"/> возвращает false</param>
    public DepIf(DepValue<bool> conditionArg, DepValue<T> trueArg, DepValue<T> falseArg)
      : this()
    {
      this.ConditionArg = conditionArg;
      this.TrueArg = trueArg;
      this.FalseArg = falseArg;
    }

    /// <summary>
    /// Создает объект, выполняющий выбор из двух фиксированных значений.
    /// </summary>
    /// <param name="conditionArg">Управляющий аргумент</param>
    /// <param name="trueArg">Аргумент, значение которого используется, если <paramref name="conditionArg"/> возвращает true</param>
    /// <param name="falseArg">Аргумент, значение которого используется, если <paramref name="conditionArg"/> возвращает false</param>
    public DepIf(DepValue<bool> conditionArg, T trueArg, T falseArg)
      : this(conditionArg, new DepConst<T>(trueArg), new DepConst<T>(falseArg))
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Аргумент - условие
    /// </summary>
    public DepValue<bool> ConditionArg
    {
      get { return _ConditionArg; }
      set { _ConditionArg.Source = value; }
    }
    private DepInput<bool> _ConditionArg;

    /// <summary>
    /// Первый аргумент
    /// </summary>
    public DepValue<T> TrueArg
    {
      get { return _TrueArg; }
      set { _TrueArg.Source = value; }
    }
    private DepInput<T> _TrueArg;

    /// <summary>
    /// Второй аргумент
    /// </summary>
    public DepValue<T> FalseArg
    {
      get { return _FalseArg; }
      set { _FalseArg.Source = value; }
    }
    private DepInput<T> _FalseArg;


    #endregion

    #region ToString()

    /// <summary>
    /// Возвращает "EQUAL", если свойство OwnerInfo не установлено
    /// </summary>
    /// <returns>Текстовое представление для отладки</returns>
    public override string ToString()
    {
      if (OwnerInfo.Owner == null)
        return "IF";
      else
        return base.ToString();
    }

    #endregion

    #region Внутренняя реализация

    private void ArgValueChanged(object sender, EventArgs args)
    {
      if (ConditionArg.Value)
        OwnerSetValue(TrueArg.Value);
      else
        OwnerSetValue(FalseArg.Value);
    }

    #endregion
  }
}
