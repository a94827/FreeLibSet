// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using FreeLibSet.Remoting;
using FreeLibSet.Collections;
using FreeLibSet.Core;

namespace FreeLibSet.Parsing
{
  #region Константы

  /// <summary>
  /// Круглые скобки "()" для задания порядка вычислений
  /// </summary>
  [Serializable]
  public sealed class ParenthesExpression : IExpression
  {
    #region Конструктор

    /// <summary>
    /// Создает выражение "круглые скобки"
    /// </summary>
    /// <param name="expression">Выражение в скобках</param>
    public ParenthesExpression(IExpression expression)
    {
      if (expression == null)
        throw new ArgumentNullException("expression");

      _Expression = expression;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Выражение в скобках
    /// </summary>
    public IExpression Expression { get { return _Expression; } }
    private IExpression _Expression;

    /// <summary>
    /// Если свойство установить в true, то при получении текстового представления будет выведено
    /// только текст для Expression, а скобки выводится не будут. То есть данный объект будет бездействующим
    /// </summary>
    public bool IgnoreParenthes { get { return _IgnoreParenthes; } set { _IgnoreParenthes = value; } }
    private bool _IgnoreParenthes;

    #endregion

    #region IExpression Members

    void IExpression.Init(ParsingData data) { }

    /// <summary>
    /// Вычисляет выражение в скобках
    /// </summary>
    /// <returns>Результат вычисления выражения</returns>
    public object Calc()
    {
      return _Expression.Calc();
    }

    /// <summary>
    /// Возвращает true, если выражение является константой
    /// </summary>
    public bool IsConst
    {
      get
      {
        return _Expression.IsConst;
      }
    }

    /// <summary>
    /// Добавляет в список выражение в скобках
    /// </summary>
    /// <param name="expressions">Список для заполнения</param>
    public void GetChildExpressions(IList<IExpression> expressions)
    {
      expressions.Add(_Expression);
    }

    /// <summary>
    /// Синтезирует выражение
    /// </summary>
    /// <param name="data">Заполняемый объект</param>
    public void Synthesize(SynthesisData data)
    {
      if (!IgnoreParenthes)
        data.Tokens.Add(new SynthesisToken(data, this, "("));
      _Expression.Synthesize(data);
      if (!IgnoreParenthes)
        data.Tokens.Add(new SynthesisToken(data, this, ")"));
    }

    #endregion

    #region Текстовое представление

    /// <summary>
    /// Возвращает "()"
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return "()";
      //return "(" + FExpression.ToString() + ")";
    }

    #endregion
  }

  /// <summary>
  /// Парсер для разбора числовых констант
  /// Научная нотация (1e3) не поддерживается.
  /// Создает лексему "Const"
  /// </summary>
  public class NumConstParser : IParser
  {
    #region Конструктор

    /// <summary>
    /// Создает парсер
    /// </summary>
    public NumConstParser()
    {
      _NumberFormat = CultureInfo.CurrentCulture.NumberFormat;
      _ReplaceChars = new Dictionary<string, string>();
      _ValidChars = null;

      AllowInt = true;
      AllowSingle = true;
      AllowDouble = true;
      AllowDecimal = true;
    }

    #endregion

    #region Свойства, управляющие парсингом

    /// <summary>
    /// Форматировщик.
    /// По умолчанию используется CultureInfo.CurrentCulture.NumberFormat
    /// </summary>
    public NumberFormatInfo NumberFormat
    {
      get { return _NumberFormat; }
      set
      {
        if (value == null)
          throw new ArgumentNullException();
        _NumberFormat = value;
      }
    }
    private NumberFormatInfo _NumberFormat;

    /// <summary>
    /// Символы замены. Например, если требуется задать распознание в качестве десятичной точки символов
    /// "," и ".", можно задать FormatProvider=StdConvert.NumberFormat и добавить пару (",":".") в ReplaceChars
    /// По умолчанию, список замен пустой
    /// </summary>
    public Dictionary<string, string> ReplaceChars { get { return _ReplaceChars; } }
    private Dictionary<string, string> _ReplaceChars;

    /// <summary>
    /// Если true (по умолчанию), разрешается получение констант типа Int32
    /// </summary>
    public bool AllowInt { get { return _AllowInt; } set { _AllowInt = value; } }
    private bool _AllowInt;

    /// <summary>
    /// Если true (по умолчанию), разрешается получение констант типа Single
    /// </summary>
    public bool AllowSingle { get { return _AllowSingle; } set { _AllowSingle = value; } }
    private bool _AllowSingle;

    /// <summary>
    /// Если true (по умолчанию), разрешается получение констант типа Double
    /// </summary>
    public bool AllowDouble { get { return _AllowDouble; } set { _AllowDouble = value; } }
    private bool _AllowDouble;

    /// <summary>
    /// Если true (по умолчанию), разрешается получение констант типа Decimal
    /// </summary>
    public bool AllowDecimal { get { return _AllowDecimal; } set { _AllowDecimal = value; } }
    private bool _AllowDecimal;

    #endregion

    #region Список допустимых символов

    private volatile string _ValidChars;

    private string GetValidChars()
    {
      // Парсинг может выполняться асинхронно, поэтому необходима блокировка
      if (_ValidChars != null)
        return _ValidChars; // быстрый возврат результата

      lock (_ReplaceChars)
      {
        if (_ValidChars == null)
        {
          SingleScopeList<char> chars = new SingleScopeList<char>();
          for (char ch = '0'; ch <= '9'; ch++)
            chars.Add(ch);

          foreach (KeyValuePair<string, string> pair in _ReplaceChars)
          {
            if (pair.Value.Length != pair.Key.Length)
              throw new InvalidOperationException("Задана недопустимая подстановка \"" + pair.Key + "\" -> \"" + pair.Value +
                "\". Исходный текст и замена должны иметь одинаковую длину");

            for (int j = 0; j < pair.Key.Length; j++)
              chars.Add(pair.Key[j]);
          }

          for (int j = 0; j < _NumberFormat.NegativeSign.Length; j++)
            chars.Add(_NumberFormat.NegativeSign[j]);

          for (int j = 0; j < _NumberFormat.PositiveSign.Length; j++)
            chars.Add(_NumberFormat.PositiveSign[j]); // 07.03.2023

          for (int j = 0; j < _NumberFormat.NumberDecimalSeparator.Length; j++)
            chars.Add(_NumberFormat.NumberDecimalSeparator[j]);

          _ValidChars = new string(chars.ToArray());
        }
      }
      return _ValidChars;
    }

    #endregion

    #region Parse

    /// <summary>
    /// Выполняет парсинг
    /// </summary>
    /// <param name="data">Заполняемый объект</param>
    public void Parse(ParsingData data)
    {
      #region Сначала выполняем быстрый поиск по подходяшим символам

      string validChars = GetValidChars();
      int len = 0;
      for (int p = data.CurrPos; p < data.Text.Text.Length; p++)
      {
        if (validChars.IndexOf(data.GetChar(p)) >= 0)
          len++;
        else
          break;
      }

      if (len == 0)
        return;

      string s = data.Text.Text.Substring(data.CurrPos, len);

      #endregion

      #region Подстановки символов

      foreach (KeyValuePair<string, string> pair in _ReplaceChars)
        s = s.Replace(pair.Key, pair.Value);

      #endregion

      #region Пытаемся выполнить преобразование

      NumberStyles ns = NumberStyles.AllowLeadingSign /*| Нельзя ! NumberStyles.AllowTrailingSign */;
      if (AllowDecimal || AllowDouble || AllowSingle) // 07.11.2022
        ns |= NumberStyles.AllowDecimalPoint;
      Token newToken;

      for (int len2 = len; len2 > 0; len2--)
      {
        string s2 = s.Substring(0, len2);
        if (AllowInt)
        {
          int v;
          if (Int32.TryParse(s2, ns, NumberFormat, out v))
          {
            newToken = new Token(data, this, "Const", data.CurrPos, len2, v);
            data.Tokens.Add(newToken);
            return;
          }
        }
        if (AllowSingle)
        {
          float v;
          if (Single.TryParse(s2, ns, NumberFormat, out v))
          {
            newToken = new Token(data, this, "Const", data.CurrPos, len2, v);
            data.Tokens.Add(newToken);
            return;
          }
        }
        if (AllowDouble)
        {
          double v;
          if (Double.TryParse(s2, ns, NumberFormat, out v))
          {
            newToken = new Token(data, this, "Const", data.CurrPos, len2, v);
            data.Tokens.Add(newToken);
            return;
          }
        }
        if (AllowDecimal)
        {
          decimal v;
          if (Decimal.TryParse(s2, ns, NumberFormat, out v))
          {
            newToken = new Token(data, this, "Const", data.CurrPos, len2, v);
            data.Tokens.Add(newToken);
            return;
          }
        }
      }

      #endregion
    }

    #endregion

    #region CreateExpression

    /// <summary>
    /// Возвращает выражение ConstExpression
    /// </summary>
    /// <param name="data">Данные парисинга</param>
    /// <param name="leftExpression">Должно быть null</param>
    /// <returns>Выражение</returns>
    public IExpression CreateExpression(ParsingData data, IExpression leftExpression)
    {
      Token currToken = data.CurrToken;
      data.SkipToken();
      if (leftExpression != null)
      {
        currToken.SetError("Константа не должна идти непосредственно после другого выражения. Ожидалась операция");
        // ? можно продолжить разбор
      }

      ConstExpression expr = ParsingShareTools.ShareConstExpression(currToken.AuxData);
      data.TokenMap.Add(currToken, expr);
      return expr;
    }

    #endregion
  }

  /// <summary>
  /// Парсер для разбора строковых констант, заключенных в кавычки или апострофы.
  /// Чтобы символ-ограничитель входил в строку, он должен быть задвоен.
  /// Создает лексему "Const"
  /// </summary>
  public class StrConstParser : IParser
  {
    #region Конструктор

    /// <summary>
    /// Создает сепаратор для строк в двойных кавычках
    /// </summary>
    public StrConstParser()
      : this('\"')
    {
    }

    /// <summary>
    /// Создает сепаратор для строк, ограниченных указанным символом.
    /// </summary>
    /// <param name="separator">Маркер начала/окончания строки (кавычки или апостроф)</param>
    public StrConstParser(char separator)
    {
      _Separator = separator;
    }

    #endregion

    #region Свойства, управляющие парсингом

    /// <summary>
    /// Ограничитель строки.
    /// Задается в конструкторе
    /// </summary>
    public char Separator { get { return _Separator; } }
    private char _Separator;

    #endregion

    #region Parse

    /// <summary>
    /// Выполняет парсинг
    /// </summary>
    /// <param name="data">Объект парсинга</param>
    public void Parse(ParsingData data)
    {
      if (data.GetChar(data.CurrPos) != Separator)
        return;

      StringBuilder sb = new StringBuilder();
      int len = 1; // сама кавычка
      for (int p = data.CurrPos + 1; p < data.Text.Text.Length; p++)
      {
        len++;
        if (data.GetChar(p) == Separator)
        {
          if (data.GetChar(p + 1) == Separator)
          {
            // Две кавычки подряд - это часть строки
            p++;
            len++;
            sb.Append(Separator);
          }
          else
          {
            // Конец строки
            data.Tokens.Add(new Token(data, this, "Const", data.CurrPos, len, sb.ToString()));
            return;
          }
        }
        else
          // Обычный символ внутри строки
          sb.Append(data.GetChar(p));
      }

      // Строка не закончена
      data.Tokens.Add(new Token(data, this, "Const", data.CurrPos, len, sb.ToString(), new ErrorMessageItem(ErrorMessageKind.Error, "Не найден символ завершения строки (" + Separator + ")")));
    }

    #endregion

    #region CreateExpression

    /// <summary>
    /// Возвращает StringExpression
    /// </summary>
    /// <param name="data">Объект парсинга</param>
    /// <param name="leftExpression">Должно быть null</param>
    /// <returns>Выражение</returns>
    public IExpression CreateExpression(ParsingData data, IExpression leftExpression)
    {
      Token currToken = data.CurrToken;
      data.SkipToken();
      if (leftExpression != null)
      {
        currToken.SetError("Константа не должна идти непосредственно после другого выражения. Ожидалась операция");
        // ? можно продолжить разбор
      }

      ConstExpression expr = ParsingShareTools.ShareConstExpression((string)(currToken.AuxData));
      data.TokenMap.Add(currToken, expr);
      return expr;
    }

    #endregion
  }


  /// <summary>
  /// Константное выражение
  /// </summary>
  [Serializable]
  public sealed class ConstExpression : IExpression
  {
    #region Конструктор

    /// <summary>
    /// Создает выражение.
    /// Обычно нет необходимости создавать объекты в прикладном коде.
    /// </summary>
    /// <param name="value">Значение константы</param>
    public ConstExpression(object value)
    {
      _Value = value;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Константа
    /// </summary>
    public object Value { get { return _Value; } }
    private readonly object _Value;

    #endregion

    #region IExpression Members

    void IExpression.Init(ParsingData data) { }

    /// <summary>
    /// Возвращает Value
    /// </summary>
    /// <returns></returns>
    public object Calc()
    {
      return _Value;
    }

    /// <summary>
    /// Возвращает true - признак константного выражения.
    /// </summary>
    public bool IsConst { get { return true; } }

    /// <summary>
    /// Ничего не делает
    /// </summary>
    /// <param name="expressions">Список для заполнения</param>
    public void GetChildExpressions(IList<IExpression> expressions)
    {
      // Нет дочерних выражений
    }

    /// <summary>
    /// Синтезирует выражение "Const"
    /// </summary>
    /// <param name="data">Заполняемый объект</param>
    public void Synthesize(SynthesisData data)
    {
      data.Tokens.Add(new SynthesisToken(data, this, "Const", data.CreateValueText(_Value), _Value));
    }

    #endregion

    #region Текстовое представление

    /// <summary>
    /// Возвращает Value
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      if (_Value == null)
        return "null";
      else
        return _Value.ToString();
    }

    #endregion
  }

  #endregion

  #region Функции

  /// <summary>
  /// Делегат для вычисления функции
  /// </summary>
  /// <param name="expression">Созданное вычисляемое выражение функции, для которого вызван метод FunctionExcpression.Calc().
  /// Может использоваться для извлечения имени функции (<see cref="FunctionExpression.Function"/>) или дополнительных данных <see cref="FunctionExpression.FixedData"/>.</param>
  /// <param name="args">Аргументы функции (вычислены раньше)</param>
  /// <returns>Результат вычисления функции</returns>
  public delegate object FunctionDelegate(FunctionExpression expression, object[] args);

  #region FunctionArgExpressionsCreatedEventHandler

  /// <summary>
  /// Аргументы события FunctionDef.ArgExpressionsCreated
  /// </summary>
  public class FunctionExpressionInitEventArgs : EventArgs
  {
    #region Конструктор

    internal FunctionExpressionInitEventArgs(FunctionExpression expression, ParsingData data)
    {
      _Expression = expression;
      _Data = data;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Вычисляемый объект функции
    /// </summary>
    public FunctionExpression Expression { get { return _Expression; } }
    private FunctionExpression _Expression;

    /// <summary>
    /// Данные парсинга
    /// </summary>
    public ParsingData Data { get { return _Data; } }
    private ParsingData _Data;

    #endregion

    #region Методы

    /// <summary>
    /// Копирование данных из <see cref="ParsingData.UserData"/> в <see cref="FunctionExpression.FixedData"/>.
    /// </summary>
    /// <param name="names">Имена элементов через запятую, которые нужно скопировать</param>
    public void CopyFixedData(string names)
    {
      Data.UserData.CopyTo(Expression.FixedData, names);
    }

    #endregion
  }

  /// <summary>
  /// Делегат события FunctionDef.ExpressionInit
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="args"></param>
  public delegate void FunctionExpressionInitEventHandler(object sender, FunctionExpressionInitEventArgs args);

  #endregion

  /// <summary>
  /// Описание одной функции
  /// </summary>
  [Serializable]
  public class FunctionDef : ObjectWithCode
  {
    #region Конструктор

    /// <summary>
    /// Создает описание функции
    /// </summary>
    /// <param name="name">Имя функции</param>
    /// <param name="calcMethod">Вычисляющий метод</param>
    /// <param name="argCount">Количество аргументов. Если функция может содержать переменное число аргументов,
    /// задайте здесь максимальное количество, а затем установите MinArgCount</param>
    public FunctionDef(string name, FunctionDelegate calcMethod, int argCount)
      : base(name)
    {
      _CalcMethod = calcMethod;
      _MinArgCount = argCount;
      _MaxArgCount = argCount;
      _LocalName = String.Empty;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя функции, например, "SIN".
    /// НЕ локализуется. Если требуется локализация, то используется свойство LocalName
    /// </summary>
    public string Name { get { return base.Code; } }

    /// <summary>
    /// Локализованное имя.
    /// Свойство не является обязательным.
    /// По умолчанию содержит пустую строку
    /// </summary>
    public string LocalName
    {
      get { return _LocalName; }
      set
      {
        if (value == null)
          value = String.Empty;
        _LocalName = value;
      }
    }
    private string _LocalName;

    /// <summary>
    /// Метод, используемый для вычисления функции
    /// </summary>
    public FunctionDelegate CalcMethod { get { return _CalcMethod; } }
    private FunctionDelegate _CalcMethod;

    /// <summary>
    /// Минимально допустимое число аргументов
    /// </summary>
    public int MinArgCount
    {
      get { return _MinArgCount; }
      set { _MinArgCount = value; }
    }
    private int _MinArgCount;

    /// <summary>
    /// Максимально допустимое число аргументов
    /// </summary>
    public int MaxArgCount
    {
      get { return _MaxArgCount; }
      set { _MaxArgCount = value; }
    }
    private int _MaxArgCount;

    /// <summary>
    /// Является ли функция недетерминированной.
    /// Если false (по умолчанию), то результат функции однозначно зависит от ее аргументов. Справедливо для
    /// большинства функций, например, "SIN".
    /// Если функция может возвращать различные значения при одних и тех же аргументов, например "TODAY", то
    /// свойство должно быть установлено в true
    /// </summary>
    public bool IsVolatile
    {
      get { return _IsVolatile; }
      set { _IsVolatile = value; }
    }
    private bool _IsVolatile;

    /// <summary>
    /// Возвращает LocalName
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      if (String.IsNullOrEmpty(LocalName))
        return Name;
      else
        return LocalName; // 27.12.2020
    }

    #endregion

    #region События

    /// <summary>
    /// Событие вызывается на второй стадии парсинга, когда построены выражения для всех аргументов функции,
    /// но еще не построено само выражение FunctionParser.FunctionExpression
    /// </summary>
    public event FunctionExpressionInitEventHandler ExpressionInit;

    internal void OnExpressionInit(FunctionExpression expression, ParsingData data)
    {
      if (ExpressionInit != null)
      {
        FunctionExpressionInitEventArgs args = new FunctionExpressionInitEventArgs(expression, data);
        ExpressionInit(this, args);
      }
    }

    #endregion
  }

  /// <summary>
  /// Выражение для вычисления функции.
  /// Если функция имеет аргументы, сначала вычисляются по очереди все аргументы слева направо.
  /// Затем вычисляется функция.
  /// </summary>
  [Serializable]
  public sealed class FunctionExpression : IExpression
  {
    #region Конструктор

    /// <summary>
    /// Создает объект выражения
    /// </summary>
    /// <param name="function">Описание функции</param>
    /// <param name="args">Выражения для аргументов функции</param>
    public FunctionExpression(FunctionDef function, IExpression[] args)
    {
      if (function == null)
        throw new ArgumentNullException("function");
      if (args == null)
        throw new ArgumentNullException("args");
      for (int i = 0; i < args.Length; i++)
      {
        if (args[i] == null)
          throw new ArgumentNullException("args[" + i.ToString() + "]");
      }

      _Function = function;
      _Args = args;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Определение функции
    /// </summary>
    public FunctionDef Function { get { return _Function; } }
    private readonly FunctionDef _Function;


    /// <summary>
    /// Вычисляемые аргументы
    /// </summary>
    public IExpression[] Args { get { return _Args; } }
    private readonly IExpression[] _Args;

    /// <summary>
    /// Дополнительные данные, вычисляемые в момент инициализации выражения функции в методе <see cref="Init(ParsingData)"/>.
    /// По умолчанию коллекция пустая. Обработчик события <see cref="FunctionDef.ExpressionInit"/> может добавить данные в коллекцию.
    /// После завершения инициализации <see cref="FunctionExpression"/> коллекция переводится в режим "только чтение".
    /// Нельзя менять данные при выполнении расчета значения функции.
    /// </summary>
    public NamedValues FixedData
    {
      get
      {
        if (_FixedData == null)
        {
          if (_Inside_Init)
            _FixedData = new NamedValues();
          else
            return NamedValues.Empty;
        }
        return _FixedData;
      }
    }
    private NamedValues _FixedData;

    #endregion

    #region IExpression Members

    [ThreadStatic]
    private static bool _Inside_Init;

    /// <summary>
    /// Вызывает событие FunctionDef.ExpressionInit
    /// </summary>
    /// <param name="data">Данные парсинга</param>
    public void Init(ParsingData data)
    {
      _Inside_Init = true;
      try
      {
        _Function.OnExpressionInit(this, data);
      }
      finally
      {
        _Inside_Init = false;
      }
      if (_FixedData != null)
      {
        if (_FixedData.IsEmpty)
          _FixedData = null;
        else
          _FixedData.SetReadOnly();
      }
    }

    /// <summary>
    /// Вычисляет функцию.
    /// </summary>
    /// <returns>Результат вычислений</returns>
    public object Calc()
    {
      object[] argVals = new object[Args.Length];
      for (int i = 0; i < Args.Length; i++)
        argVals[i] = Args[i].Calc();

      return Function.CalcMethod(this, argVals);
    }

    /// <summary>
    /// Возвращает false если FunctionDef.IsVolatile=true.
    /// Иначе возвращает true, если функция не имеет аргументов или если все аргументы являются константами.
    /// </summary>
    public bool IsConst
    {
      get
      {
        if (Function.IsVolatile)
          return false;

        for (int i = 0; i < Args.Length; i++)
        {
          if (!Args[i].IsConst)
            return false;
        }

        return true;
      }
    }

    /// <summary>
    /// Добавляет в список все выражения для вычисления аргументов
    /// </summary>
    /// <param name="expressions">Список для заполнения</param>
    public void GetChildExpressions(IList<IExpression> expressions)
    {
      for (int i = 0; i < Args.Length; i++)
        expressions.Add(Args[i]);
    }

    /// <summary>
    /// Синтез выражения
    /// </summary>
    /// <param name="data"></param>
    public void Synthesize(SynthesisData data)
    {
      if (data.UseFormulas)
      {
        data.Tokens.Add(new SynthesisToken(data, this, FunctionParser.TokenName, Function.Name, Function.Name));
        data.Tokens.Add(new SynthesisToken(data, this, FunctionParser.TokenOpen, "("));
        for (int i = 0; i < Args.Length; i++)
        {
          if (i > 0)
          {
            data.Tokens.Add(new SynthesisToken(data, this, FunctionParser.TokenArgSep, data.ListSeparator));
            if (data.UseSpaces)
              data.Tokens.Add(new SynthesisToken(data, this, "Space", " "));
          }
          Args[i].Synthesize(data);
        }
        data.Tokens.Add(new SynthesisToken(data, this, FunctionParser.TokenClose, ")"));
      }
      else
      {
        // Добавляем константу
        data.Tokens.Add(new SynthesisToken(data, this, "Const", data.CreateValueText(Calc())));
      }
    }

    #endregion

    #region ToString()

    /// <summary>
    /// Возвращает имя функции и скобки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return _Function.Name + "()";
    }

    #endregion
  }

  /// <summary>
  /// Парсер для разбора функций.
  /// Распознает имя функции (задается список), круглые скобки и разделители аргументов функции 
  /// (список разделителей можно задать)
  /// </summary>
  public class FunctionParser : IParser
  {
    #region Константы

    /// <summary>
    /// Лексема "Имя функции"
    /// </summary>
    public const string TokenName = "FunctionName";

    /// <summary>
    /// Лексема "("
    /// </summary>
    public const string TokenOpen = "FunctionOpen";

    /// <summary>
    /// Лексема ")"
    /// </summary>
    public const string TokenClose = "FunctionClose";

    /// <summary>
    /// Лексема ","
    /// </summary>
    public const string TokenArgSep = "FunctionArgSep";

    #endregion

    #region Конструктор

    /// <summary>
    /// Создать парсер
    /// </summary>
    public FunctionParser()
    {
      _ArgSeparators = new List<string>();
      _ArgSeparators.Add(CultureInfo.CurrentCulture.TextInfo.ListSeparator);
      _Functions = new NamedList<FunctionDef>();
      _UseBothNames = true;

      _ValidNameChars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
      _InvalidFirstChars = "0123456789";
      _CaseSensitive = false;
    }

    #endregion

    #region Список функций

    /// <summary>
    /// Список функций
    /// Определения могут добавляться вручную. Для добавления функций Excel можно использовать
    /// метод ExcelFunctions.AddFunctions()
    /// </summary>
    public NamedList<FunctionDef> Functions { get { return _Functions; } }
    private NamedList<FunctionDef> _Functions;

    /// <summary>
    /// Добавить локальные имена функций.
    /// Просматривает заполненный список функций Functions и ищет соответствующий ключ в коллекции
    /// <paramref name="localNames"/>. Если ключ найден, то для функции устанавливается свойство LocalName, равным значению коллекции.
    /// Коллекция <paramref name="localNames"/> может содержать ключи, для которых нет функций в списке Functions
    /// </summary>
    /// <param name="localNames">Локализованные имена. Ключом является глобальное имя, например, "TODAY", а значением - локализованное, например "СЕГОДНЯ"</param>
    public void SetLocalNames(IDictionary<string, string> localNames)
    {
      for (int i = 0; i < Functions.Count; i++)
      {
        string localName;
        if (localNames.TryGetValue(Functions[i].Name, out localName))
          Functions[i].LocalName = localName;
      }
    }

    /// <summary>
    /// Добавить локальные имена функций.
    /// Просматривает список заданных глобальных имен <paramref name="names"/> и находит соответствующее определение функции
    /// в списке Functions. Если найдено, устанавливает свойство FunctionDef.LocalName равным элементу массива <paramref name="localNames"/>
    /// Список<paramref name="names"/> может содержать имена функций, которых нет в списке Functions
    /// </summary>
    /// <param name="names">Глобальные имена</param>
    /// <param name="localNames">Локализованные имена</param>
    public void SetLocalNames(string[] names, string[] localNames)
    {
      if (localNames.Length != names.Length)
        throw new ArgumentException("Длина массивов не совпадает", "localNames");

      for (int i = 0; i < names.Length; i++)
      {
        FunctionDef fd = Functions[names[i]];
        if (fd != null)
          fd.LocalName = localNames[i];
      }
    }

    #endregion

    #region Свойства, управляющие парсингом

    /// <summary>
    /// Символы, которые могут содержаться в имени функции
    /// По умолчанию содержит заглавные латинские буквы и цифры
    /// Если используется локализация, то требуется добавить символы в строку.
    /// Также можно добавить управляющие символы, например, знак "_"
    /// Если свойство CaseSensitive установлено в true, то требуется также добавить символы неижнего регистра
    /// </summary>
    public string ValidNameChars
    {
      get { return _ValidNameChars; }
      set { _ValidNameChars = value; }
    }
    private string _ValidNameChars;

    /// <summary>
    /// Символы из ValidNameChars, которые не могут быть первыми в имени функции/
    /// По умолчанию содержит цифры от 0 до 9
    /// </summary>
    public string InvalidFirstChars
    {
      get { return _InvalidFirstChars; }
      set { _InvalidFirstChars = value; }
    }
    private string _InvalidFirstChars;

    /// <summary>
    /// Если свойство установлено в true, то имена функций чувстсительны к регистру.
    /// По умолчанию - false
    /// </summary>
    public bool CaseSensitive
    {
      get { return _CaseSensitive; }
      set { _CaseSensitive = value; }
    }
    private bool _CaseSensitive;


    /// <summary>
    /// Разделители списка аргументов. По умолчанию содержит один разделитель, заданный в
    /// CultureInfo.CurrentCulture.TextInfo.ListSeparator.
    /// При необходимости, список может быть очищен и заполнен нужным значением
    /// </summary>
    public List<string> ArgSeparators { get { return _ArgSeparators; } }
    private List<string> _ArgSeparators;

    /// <summary>
    /// Если свойство установлено в true (по умолчанию), то в разбираемых выражениях разрешено использовать
    /// как локальные имена (LocalName), так и глобавльные (Name).
    /// Если свойства сброшено в false, то можно использовать только локальные имена (LocalName), кроме функций, у
    /// которых LocalName="". Для последних используется Name
    /// </summary>
    public bool UseBothNames
    {
      get { return _UseBothNames; }
      set { _UseBothNames = value; }
    }
    private bool _UseBothNames;

    #endregion

    #region Словарь имен

    private Dictionary<string, FunctionDef> _NameDict;

    private Dictionary<string, FunctionDef> GetNameDict()
    {
      lock (_Functions)
      {
        if (_NameDict == null)
        {
          _NameDict = new Dictionary<string, FunctionDef>();
          foreach (FunctionDef fd in _Functions)
          {
            if (String.IsNullOrEmpty(fd.LocalName))
              _NameDict.Add(CaseSensitive ? fd.Name : fd.Name.ToUpperInvariant(), fd);
            else
            {
              _NameDict.Add(CaseSensitive ? fd.LocalName : fd.LocalName.ToUpperInvariant(), fd);
              if (UseBothNames)
                _NameDict.Add(CaseSensitive ? fd.Name : fd.Name.ToUpperInvariant(), fd);
            }
          }
        }
        return _NameDict;
      }
    }

    #endregion

    #region Parse

    /// <summary>
    /// Определяет лексемы:
    /// "Function" - содержит имя функции. В качестве AuxData содержит имя функции, как оно задано в тексте.
    /// Лексема определяется, даже если не обнаружена открывающая скобка
    /// "FunctionOpen" и "FunctionClose" задают скобки "(" и ")". 
    /// Открывающая круглая скобка не распознается, если имя функции не было задано.
    /// "FunctionSep" - разделитель списка аргументов. Реальный разделитель (разделители), задаются в списке ArgSeparators
    /// </summary>
    /// <param name="data"></param>
    public void Parse(ParsingData data)
    {
      Token newToken;

      #region Скобки

      char ch = data.GetChar(data.CurrPos);

      // Скобки могут быть частью функции, а могут быть частью математического выражения 

      if (ch == '(')
      {
        // Проверяем, что до этого была наша функция
        for (int i = data.Tokens.Count - 1; i >= 0; i--)
        {
          switch (data.Tokens[i].TokenType)
          {
            case "Space":
            case "Comment":
              continue;
            case TokenName:
              newToken = new Token(data, this, TokenOpen, data.CurrPos, 1);
              data.Tokens.Add(newToken);
              return;
            default:
              return; // 09.02.2017 - не наша скобка
          }
        }
        return;
      }

      if (ch == ')')
      {
        // Проверяем, подсчитывая скобки
        int counter = 1; // наша скобка
        for (int j = data.Tokens.Count - 1; j >= 0; j--)
        {
          switch (data.Tokens[j].TokenType)
          {
            case TokenOpen:
              counter--;
              if (counter == 0)
              {
                if (data.Tokens[j].Parser == this)
                {
                  // Открывающая скобка наша
                  newToken = new Token(data, this, TokenClose, data.CurrPos, 1);
                  data.Tokens.Add(newToken);
                }
                return;
              }
              break;
            case "(": // 07.11.2022
              counter--;
              break;
            case ")": // 07.11.2022
            case TokenClose:
              counter++;
              break;
          }
        }
        return;
      }

      #endregion

      #region Разделитель списка аргументов

      for (int i = 0; i < ArgSeparators.Count; i++)
      {
        if (data.StartsWith(ArgSeparators[i], false))
        {
          // 17.12.2018. Проверяем, подсчитывая скобки
          int counter = 1; // наша скобка
          for (int j = data.Tokens.Count - 1; j >= 0; j--)
          {
            switch (data.Tokens[j].TokenType)
            {
              case TokenClose:
                counter++;
                break;
              case TokenOpen:
                counter--;
                if (counter == 0)
                {
                  if (data.Tokens[j].Parser == this)
                  {
                    // Открывающая скобка наша
                    newToken = new Token(data, this, TokenArgSep, data.CurrPos, ArgSeparators[i].Length);
                    data.Tokens.Add(newToken);
                  }
                  return;
                }
                break;
            }
          }
        }
      }

      #endregion

      #region Имя функции

      StringComparison sc = CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

      string s1 = new string(ch, 1);

      if (ValidNameChars.IndexOf(s1, sc) < 0)
        return;
      if (InvalidFirstChars.IndexOf(s1, sc) >= 0)
        return;

      // Начало имени функции найдено
      int len = 1;
      for (int p = data.CurrPos + 1; p < data.Text.Text.Length; p++)
      {
        ch = data.GetChar(p);
        s1 = new string(ch, 1);
        if (ValidNameChars.IndexOf(s1, sc) < 0)
          break;
        else
          len++;
      }

      string funcName = data.Text.Text.Substring(data.CurrPos, len);
      newToken = new Token(data, this, TokenName, data.CurrPos, len, funcName);
      data.Tokens.Add(newToken);

      #endregion
    }

    #endregion

    #region CreateExpression

    /// <summary>
    /// Создает выражение FunctionExpression
    /// </summary>
    /// <param name="data">Данные парсинга</param>
    /// <param name="leftExpression">Выражение слева. Должно быть null</param>
    /// <returns>Выражение</returns>
    public IExpression CreateExpression(ParsingData data, IExpression leftExpression)
    {
      switch (data.CurrTokenType)
      {
        case TokenName:
          Token currToken = data.CurrToken;
          data.SkipToken();

          if (leftExpression != null)
          {
            currToken.SetError("Имя функции не может быть продолжением другого выражения. Ожидалась операция");
            // ? можно продолжить
          }

          string functionName = currToken.AuxData.ToString();
          if (String.IsNullOrEmpty(functionName))
            throw new BugException("Не определено имя функции для лексемы");
          FunctionDef fd = GetFunction(functionName);

          // Ищем лексему открывающей функции
          Token openToken = null;
          while ((data.CurrTokenIndex < data.Tokens.Count) && (openToken == null))
          {
            switch (data.CurrTokenType)
            {
              case TokenOpen:
                openToken = data.CurrToken;
                data.SkipToken();
                break;

              case "Space":
              case "Comment":
                data.SkipToken();
                break;

              default:
                data.CurrToken.SetError("Ожидалась открывающая скобка после имени функции");
                return null;
            }
          }
          if (openToken == null)
          {
            currToken.SetError("Не найдена открывающая скобка после имени функции");
            return null;
          }

          // Перебираем аргуметы функции
          List<IExpression> argExprs = new List<IExpression>();
          Token closeToken = null;
          List<Token> argSepTokens = new List<Token>();
          bool lastIsArgSep = false;
          while (true)
          {
            IExpression argExpr = data.Parsers.CreateSubExpression(data, new string[] { TokenArgSep, TokenClose });
            if (argExpr == null)
            {
              if (data.CurrToken == null)
              {
                currToken.SetError("Не найдена закрывающая скобка для функции " + functionName);
                return null;
              }

              if (data.CurrTokenType == TokenClose)
              {
                closeToken = data.CurrToken;
                data.SkipToken();
                if (lastIsArgSep)
                {
                  closeToken.SetError("Ожидался еще аргумент");
                  return null; // 08.11.2022
                }
                break;
              }

              Token errorToken;
              if (argSepTokens.Count > 0)
                errorToken = argSepTokens[argSepTokens.Count - 1];
              else
                errorToken = openToken;

              errorToken.SetError("Ожидался аргумент");
              return null; // 08.11.2022
            }

            argExprs.Add(argExpr);
            lastIsArgSep = false;
            if (data.CurrToken == null)
            {
              currToken.SetError("Не найдена закрывающая скобка для функции " + functionName);
              return null;
            }
            if (data.CurrTokenType == TokenClose)
            {
              closeToken = data.CurrToken; // 03.12.2015
              data.SkipToken();
              if (lastIsArgSep)
              {
                closeToken.SetError("Ожидался еще аргумент");
                return null; // 08.11.2022
              }
              break;
            }
            if (data.CurrTokenType != TokenArgSep)
            {
              string errorText = "Ожидалась закрывающая скобка вызова функции";
              if (argSepTokens.Count > 0)
              {
                errorText += " или разделитель списка аргументов \"" + argSepTokens[0] + "\"";
                for (int i = 1; i < argSepTokens.Count; i++)
                  errorText += " или \"" + argSepTokens[i] + "\"";
              }
              data.CurrToken.SetError(errorText);
              return null;
            }

            argSepTokens.Add(data.CurrToken);
            lastIsArgSep = true;
            data.SkipToken();
          }

          // Список аргументов загружен. Скобка получена
          if (fd == null)
          {
            currToken.SetError("Неизвестное имя функции \"" + currToken.AuxData.ToString() + "\"");
            return null;
          }

          IExpression[] argExprs2 = argExprs.ToArray();

          if (argExprs2.Length < fd.MinArgCount || argExprs2.Length > fd.MaxArgCount)
          {
            string errorText = "Неправильное количество аргументов функции \"" + fd.ToString() + "\" (" + argExprs2.Length.ToString() + ")";
            if (fd.MaxArgCount == fd.MinArgCount)
              errorText += ". Ожидалось аргументов: " + fd.MaxArgCount.ToString();
            else
              errorText += ". Ожидалось аргументов: от " + fd.MinArgCount.ToString() + " до " + fd.MaxArgCount.ToString();
            currToken.SetError(errorText);
            return null;
          }


          FunctionExpression funcExpr = new FunctionExpression(fd, argExprs2 /*, data.UserData убрано 03.01.2022 */);
          data.TokenMap.Add(currToken, funcExpr);
          data.TokenMap.Add(openToken, funcExpr);
          foreach (Token tk in argSepTokens)
            data.TokenMap.Add(tk, funcExpr);
          data.TokenMap.Add(closeToken, funcExpr);
          return funcExpr;

        default:
          data.CurrToken.SetError("Неожиданное вхождение \"" + data.CurrToken.Text + "\" вне вызова функции");
          data.SkipToken();
          return null;
      }
    }

    /// <summary>
    /// Возвращает описание функции по заданному имени (как оно задано в выражении)
    /// Базовая реализация возвращает описание из массива Functions
    /// Переопределенный метод может возвращать динамически создаваемое описание
    /// Метод возвращает null, если имя функции неизвестно
    /// </summary>
    /// <param name="name">Локализованное или нелокализованное имя функции</param>
    /// <returns>Описание функции</returns>
    protected virtual FunctionDef GetFunction(string name)
    {
      if (String.IsNullOrEmpty(name))
        throw new ArgumentNullException("name");
      if (!CaseSensitive)
        name = name.ToUpperInvariant();

      Dictionary<string, FunctionDef> dict = GetNameDict();

      FunctionDef fd;
      if (dict.TryGetValue(name, out fd))
        return fd;
      else
        return null;
    }

    #endregion
  }

  #endregion

  #region Операции

  /// <summary>
  /// Метод, выполняющий бинарную операцию
  /// </summary>
  /// <param name="op">Знак операции</param>
  /// <param name="arg1">Левый аргумент</param>
  /// <param name="arg2">Правый аргумент</param>
  /// <returns></returns>
  public delegate object BinaryOpDelegate(string op, object arg1, object arg2);

  /// <summary>
  /// Описание бинарной операции
  /// Порядок добавления объектов MathOpDef в списки MathOpParser должен соответствовать
  /// понижению приоритета операций
  /// </summary>
  public class BinaryOpDef : ObjectWithCode
  {
    #region Константы приоритетов

    /// <summary>
    /// Не используется
    /// </summary>
    public const int PriorityPower = 500;

    /// <summary>
    /// Операции умножения и деления имеют высший приоритет
    /// </summary>
    public const int PriorityMulDiv = 400;

    /// <summary>
    /// Операции сложения и вычитания имеют меньший приоритет
    /// </summary>
    public const int PriorityAddSub = 300;

    /// <summary>
    /// Операции сравнения имеют низкий приритет
    /// </summary>
    public const int PriorityCompare = 200;

    /// <summary>
    /// Не используется
    /// </summary>
    public const int PriorityLogical = 100;

    #endregion

    #region Конструктор

    /// <summary>
    /// Создает описание операции
    /// </summary>
    /// <param name="op">Знак операции</param>
    /// <param name="calcMethod">Метод вычисления</param>
    /// <param name="priority">Приоритет операции. Чем больше значение, тем выше приоритет</param>
    public BinaryOpDef(string op, BinaryOpDelegate calcMethod, int priority)
      : base(op)
    {
      if (calcMethod == null)
        throw new ArgumentNullException("calcMethod");
      _CalcMethod = calcMethod;
      _Priority = priority;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Знак операции.
    /// Задается в конструкторе.
    /// </summary>
    public string Op { get { return base.Code; } }

    /// <summary>
    /// Метод вычисления операции.
    /// Задается в конструкторе
    /// </summary>
    public BinaryOpDelegate CalcMethod { get { return _CalcMethod; } }
    private readonly BinaryOpDelegate _CalcMethod;

    /// <summary>
    /// Приоритеты операций.
    /// Используются, когда несколько операций идут подряд без скобок
    /// Чем больше значение, тем выше приоритет операции.
    /// Например, приритет операции умножения больше, чем операции сложения.
    /// Для стандартных операций используются константы PriorityXXX
    /// </summary>
    public int Priority { get { return _Priority; } }
    private readonly int _Priority;

    #endregion
  }

  /// <summary>
  /// Метод, выполняющий унарную операцию
  /// </summary>
  /// <param name="op">Знак операции</param>
  /// <param name="arg">Аргумент (справа от операции)</param>
  /// <returns></returns>
  public delegate object UnaryOpDelegate(string op, object arg);

  /// <summary>
  /// Описание унарной операции
  /// Порядок добавления объектов MathOpDef в списки MathOpParser должен соответствовать
  /// понижению приоритета операций
  /// </summary>
  public class UnaryOpDef : ObjectWithCode
  {
    #region Конструктор

    /// <summary>
    /// Создает описание операции
    /// </summary>
    /// <param name="op">Знак операции</param>
    /// <param name="calcMethod">Метод для вычисления операции</param>
    public UnaryOpDef(string op, UnaryOpDelegate calcMethod)
      : base(op)
    {
      if (calcMethod == null)
        throw new ArgumentNullException("calcMethod");
      _CalcMethod = calcMethod;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Знак операции.
    /// Задается в конструкторе.
    /// </summary>
    public string Op { get { return base.Code; } }

    /// <summary>
    /// Метод вычисления операции.
    /// Задается в конструкторе.
    /// </summary>
    public UnaryOpDelegate CalcMethod { get { return _CalcMethod; } }
    private readonly UnaryOpDelegate _CalcMethod;

    #endregion
  }

  /// <summary>
  /// Парсер для разбора арифметических операций.
  /// Создает лексемы, совпадающие с операциями: "+", "-", "*", "/", знаки сравнения, круглые скобки "()".
  /// </summary>
  public class MathOpParser : IParser
  {
    #region Конструкторы

    /// <summary>
    /// Создает парсер, распознающий стандартные арифметические и логические операции
    /// </summary>
    public MathOpParser()
      : this(true)
    {
    }

    /// <summary>
    /// Создает парсер, с возможностью добавления операций.
    /// </summary>
    /// <param name="useDefaultOps">Надо ли добавлять описания стандартных арифметических и логических операций.
    /// Если false, то описания всех операций должны быть добавлены вручную</param>
    public MathOpParser(bool useDefaultOps)
    {
      _BinaryOps = new NamedList<BinaryOpDef>();
      _UnaryOps = new NamedList<UnaryOpDef>();

      if (useDefaultOps)
      {
        BinaryOpDelegate binaryD = new BinaryOpDelegate(BinaryCalc);
        UnaryOpDelegate unaryD = new UnaryOpDelegate(UnaryCalc);

        // 10.10.2017
        // Сначала должны идти операции из двух символов, а затем - из одного,
        // иначе, например, операция ">=" будет распознана как ">" и "="

        string[] binarySigns = new string[] { 
          "<>", ">=", "<=",
          "*", "/", "+", "-", "=", ">", "<" };
        int[] binaryPriorities = new int[] { 
          BinaryOpDef.PriorityCompare, // "<>"
          BinaryOpDef.PriorityCompare, // ">="
          BinaryOpDef.PriorityCompare, // "<="
          BinaryOpDef.PriorityMulDiv,  // "*"
          BinaryOpDef.PriorityMulDiv,  // "/"
          BinaryOpDef.PriorityAddSub,  // "+"
          BinaryOpDef.PriorityAddSub,  // "-"
          BinaryOpDef.PriorityCompare, // "="
          BinaryOpDef.PriorityCompare, // ">"
          BinaryOpDef.PriorityCompare  // "<"
        };

        for (int i = 0; i < binarySigns.Length; i++)
          BinaryOps.Add(new BinaryOpDef(binarySigns[i], binaryD, binaryPriorities[i])); // один делегат на все

        string[] unarySigns = new string[] { "+", "-" };

        for (int i = 0; i < unarySigns.Length; i++)
          UnaryOps.Add(new UnaryOpDef(unarySigns[i], unaryD)); // один делегат на все
      }
    }

    #endregion

    #region Списки операций

    /// <summary>
    /// Список допустимых бинарных операций
    /// </summary>
    public NamedList<BinaryOpDef> BinaryOps { get { return _BinaryOps; } }
    private NamedList<BinaryOpDef> _BinaryOps;

    /// <summary>
    /// Список допустимых унарных операций
    /// </summary>
    public NamedList<UnaryOpDef> UnaryOps { get { return _UnaryOps; } }
    private NamedList<UnaryOpDef> _UnaryOps;

    #endregion

    #region Вычисление

    #region Бинарные операции

    /// <summary>
    /// Стандартное вычисление арифметических и логических бинарных операций
    /// </summary>
    /// <param name="op">Знак операции</param>
    /// <param name="arg1">Первый аргумент</param>
    /// <param name="arg2">Второй аргумент</param>
    /// <returns>Результат операции</returns>
    public static object BinaryCalc(string op, object arg1, object arg2)
    {
      if (arg1 == null)
      {
        if (arg2 == null)
          return CalcNull(op);
        else
          arg1 = DataTools.GetEmptyValue(arg2.GetType());
      }
      else if (arg2 == null)
        arg2 = DataTools.GetEmptyValue(arg1.GetType());

      if (arg1 is DateTime || arg2 is DateTime)
      {
        // Тут все сложнее. Даты могут складываться с числами
        if (arg1 is DateTime)
        {
          if (arg2 is DateTime)
            return CalcDateTime(op, (DateTime)arg1, (DateTime)arg2);
          else if (arg2 is TimeSpan)
            return CalcDateTimeAndTimeSpan(op, (DateTime)arg1, (TimeSpan)arg2);
          else
            return CalcDateTimeAndDouble(op, (DateTime)arg1, DataTools.GetDouble(arg2));
        }
        else
        {
          if (arg1 is TimeSpan)
            return CalcTimeSpanAndDateTime(op, (TimeSpan)arg1, (DateTime)arg2);
          else
            return CalcDoubleAndDateTime(op, DataTools.GetDouble(arg1), (DateTime)arg2);
        }
      }

      if (arg1 is TimeSpan || arg2 is TimeSpan)
      {
        if (arg1 is TimeSpan)
        {
          if (arg2 is TimeSpan)
            return CalcTimeSpan(op, (TimeSpan)arg1, (TimeSpan)arg2);
          else
            return CalcTimeSpanAndDouble(op, (TimeSpan)arg1, DataTools.GetDouble(arg2));
        }
        else
          return CalcDouble(op, DataTools.GetDouble(arg1), DataTools.GetDouble(arg2));
      }

      if (arg1 is decimal || arg2 is decimal)
        return CalcDecimal(op, DataTools.GetDecimal(arg1), DataTools.GetDecimal(arg2));

      if (arg1 is double || arg2 is double)
        return CalcDouble(op, DataTools.GetDouble(arg1), DataTools.GetDouble(arg2));

      if (arg1 is float || arg2 is float)
        return CalcSingle(op, DataTools.GetSingle(arg1), DataTools.GetSingle(arg2));

      if (arg1 is int || arg2 is int)
        return CalcInt(op, DataTools.GetInt(arg1), DataTools.GetInt(arg2));

      if (arg1 is string || arg2 is string)
        //throw new NotSupportedException("Математические операции над строками не поддерживаются. Используйте функции");
        return CalcString(op, DataTools.GetString(arg1), DataTools.GetString(arg2));
      if (arg1 is bool && arg2 is bool)
        return CalcBool(op, DataTools.GetBool(arg1), DataTools.GetBool(arg2));

      throw new NotSupportedException("Остальное не реализовано. Тип данных аргумента 1:" + arg1.GetType().ToString() + ", аргумента 2: " + arg2.GetType().ToString());
    }

    /// <summary>
    /// "Вычисляет" унарную операцию для двух аргументов null.
    /// Для операций сравнение, содержажих знак равенства, возвращает true.
    /// Для операций "больше", "меньше" и "не равно", возвращает false.
    /// Для арифметических операций возвращает null.
    /// </summary>
    /// <param name="op"></param>
    /// <returns></returns>
    public static object CalcNull(string op)
    {
      switch (op)
      {
        case "*":
        case "/":
        case "+":
        case "-":
          return null;
        case "=":
        case ">=":
        case "<=":
          return true;
        case "!=":
        case ">":
        case "<":
          return false;
        default:
          throw new InvalidOperationException("Операция \"" + op + "\" не реализована для аргументов null");
      }
    }

    /// <summary>
    /// Стандартное вычисление арифметических и логических бинарных операций для типа Decimal
    /// </summary>
    /// <param name="op">Знак операции</param>
    /// <param name="arg1">Первый аргумент</param>
    /// <param name="arg2">Второй аргумент</param>
    /// <returns>Результат операции</returns>
    public static object CalcDecimal(string op, decimal arg1, decimal arg2)
    {
      switch (op)
      {
        case "+": return arg1 + arg2;
        case "-": return arg1 - arg2;
        case "*": return arg1 * arg2;
        case "/": return arg1 / arg2;

        case "=": return arg1 == arg2;
        case "<>": return arg1 != arg2;
        case ">": return arg1 > arg2;
        case "<": return arg1 < arg2;
        case ">=": return arg1 >= arg2;
        case "<=": return arg1 <= arg2;
        default:
          throw new InvalidOperationException("Операция \"" + op + "\" не реализована");
      }
    }

    /// <summary>
    /// Стандартное вычисление арифметических и логических бинарных операций для типа Double
    /// </summary>
    /// <param name="op">Знак операции</param>
    /// <param name="arg1">Первый аргумент</param>
    /// <param name="arg2">Второй аргумент</param>
    /// <returns>Результат операции</returns>
    public static object CalcDouble(string op, double arg1, double arg2)
    {
      switch (op)
      {
        case "+": return arg1 + arg2;
        case "-": return arg1 - arg2;
        case "*": return arg1 * arg2;
        case "/": return arg1 / arg2;

        case "=": return arg1 == arg2;
        case "<>": return arg1 != arg2;
        case ">": return arg1 > arg2;
        case "<": return arg1 < arg2;
        case ">=": return arg1 >= arg2;
        case "<=": return arg1 <= arg2;
        default:
          throw new InvalidOperationException("Операция \"" + op + "\" не реализована");
      }
    }

    /// <summary>
    /// Стандартное вычисление арифметических и логических бинарных операций для типа Single
    /// </summary>
    /// <param name="op">Знак операции</param>
    /// <param name="arg1">Первый аргумент</param>
    /// <param name="arg2">Второй аргумент</param>
    /// <returns>Результат операции</returns>
    public static object CalcSingle(string op, float arg1, float arg2)
    {
      switch (op)
      {
        case "+": return arg1 + arg2;
        case "-": return arg1 - arg2;
        case "*": return arg1 * arg2;
        case "/": return arg1 / arg2;

        case "=": return arg1 == arg2;
        case "<>": return arg1 != arg2;
        case ">": return arg1 > arg2;
        case "<": return arg1 < arg2;
        case ">=": return arg1 >= arg2;
        case "<=": return arg1 <= arg2;
        default:
          throw new InvalidOperationException("Операция \"" + op + "\" не реализована");
      }
    }

    /// <summary>
    /// Стандартное вычисление арифметических и логических бинарных операций для типа Int32
    /// </summary>
    /// <param name="op">Знак операции</param>
    /// <param name="arg1">Первый аргумент</param>
    /// <param name="arg2">Второй аргумент</param>
    /// <returns>Результат операции</returns>
    public static object CalcInt(string op, int arg1, int arg2)
    {
      // 01.03.2017
      // Сначала пытаемся вычислить для целочисленного аргумента
      int res1;
      try
      {
        checked // обязательно с контролем переполнения
        {
          switch (op)
          {
            case "+": return arg1 + arg2;
            case "-": return arg1 - arg2;
            case "*": return arg1 * arg2;
            case "/":
              res1 = arg1 / arg2; // тут может быть существенная потеря точности
              break;

            case "=": return arg1 == arg2;
            case "<>": return arg1 != arg2;
            case ">": return arg1 > arg2;
            case "<": return arg1 < arg2;
            case ">=": return arg1 >= arg2;
            case "<=": return arg1 <= arg2;
            default:
              throw new InvalidOperationException("Операция \"" + op + "\" не реализована");
          }
        }
      }
      catch
      {
        // В случае переполнения вычисляем тоже самое для типа Double
        return CalcDouble(op, (double)arg1, (double)arg2);
      }

      // Для операции деления вычисляем тоже самое для типа Double
      double res2 = (double)CalcDouble(op, (double)arg1, (double)arg2);

      if (Math.Abs(res2) > (double)(Int32.MaxValue))
        return res2; // переполнение целого числа

      if (res2 == (double)res1)
        return res1; // нет потери точности
      else
        return res2; // есть потеря точности
    }

    /// <summary>
    /// Стандартное вычисление арифметических и логических бинарных операций для типа String
    /// </summary>
    /// <param name="op">Знак операции</param>
    /// <param name="arg1">Первый аргумент</param>
    /// <param name="arg2">Второй аргумент</param>
    /// <returns>Результат операции</returns>
    public static object CalcString(string op, string arg1, string arg2)
    {
      switch (op)
      {
        case "+": return arg1 + arg2;
        case "=": return arg1 == arg2;
        case "<>": return arg1 != arg2;
        default:
          throw new InvalidOperationException("Для строк применимы только операции \"+\", \"=\" и \"<>\". Операция \"" + op + "\" не реализована");
      }
    }

    private static object CalcBool(string op, bool arg1, bool arg2)
    {
      switch (op)
      {
        case "=": return arg1 == arg2;
        case "<>": return arg1 != arg2;
        default:
          throw new InvalidOperationException("Для логических значений применимы только операции \"=\" и \"<>\". Операция \"" + op + "\" не применима");
      }
    }

    #region DateTime и TimeSpan

    /// <summary>
    /// Стандартное вычисление арифметических и логических бинарных операций для типа DateTime
    /// </summary>
    /// <param name="op">Знак операции</param>
    /// <param name="arg1">Первый аргумент</param>
    /// <param name="arg2">Второй аргумент</param>
    /// <returns>Результат операции</returns>
    public static object CalcDateTime(string op, DateTime arg1, DateTime arg2)
    {
      arg1 = DateTime.SpecifyKind(arg1, DateTimeKind.Unspecified);
      arg2 = DateTime.SpecifyKind(arg2, DateTimeKind.Unspecified);

      switch (op)
      {
        case "-": return arg1 - arg2;

        case "=": return arg1 == arg2;
        case "<>": return arg1 != arg2;
        case ">": return arg1 > arg2;
        case "<": return arg1 < arg2;
        case ">=": return arg1 >= arg2;
        case "<=": return arg1 <= arg2;
        default:
          throw new InvalidOperationException("Операция \"" + op + "\" не поддерживается для двух аргументов DateTime");
      }
    }

    private static object CalcDateTimeAndDouble(string op, DateTime arg1, double arg2)
    {
      arg1 = DateTime.SpecifyKind(arg1, DateTimeKind.Unspecified);
      switch (op)
      {
        case "+": return arg1 + TimeSpan.FromDays(arg2);
        case "-": return arg1 + TimeSpan.FromDays(arg2);
      }

      DateTime dt2 = DateTime.FromOADate(arg2);
      return CalcDateTime(op, arg1, dt2);
    }

    private static object CalcDoubleAndDateTime(string op, double arg1, DateTime arg2)
    {
      DateTime dt1 = DateTime.FromOADate(arg1);
      dt1 = DateTime.SpecifyKind(dt1, DateTimeKind.Unspecified);
      arg2 = DateTime.SpecifyKind(arg2, DateTimeKind.Unspecified);

      switch (op)
      {
        case "=": return dt1 == arg2;
        case "<>": return dt1 != arg2;
        case ">": return dt1 > arg2;
        case "<": return dt1 < arg2;
        case ">=": return dt1 >= arg2;
        case "<=": return dt1 <= arg2;
        default:
          throw new InvalidOperationException("Операция \"" + op + "\" не поддерживается для двух аргументов DateTime");
      }
    }


    private static object CalcTimeSpan(string op, TimeSpan arg1, TimeSpan arg2)
    {
      switch (op)
      {
        case "+": return arg1 + arg2;
        case "-": return arg1 - arg2;
        case "=": return arg1 == arg2;
        case "<>": return arg1 != arg2;
        case ">": return arg1 > arg2;
        case "<": return arg1 < arg2;
        case ">=": return arg1 >= arg2;
        case "<=": return arg1 <= arg2;
        default:
          throw new InvalidOperationException("Операция \"" + op + "\" не поддерживается для двух аргументов TimeSpan");
      }
    }

    private static object CalcTimeSpanAndDateTime(string op, TimeSpan arg1, DateTime arg2)
    {
      arg2 = DateTime.SpecifyKind(arg2, DateTimeKind.Unspecified);
      switch (op)
      {
        case "+": return arg2 + arg1; // 09.11.2022
        default:
          throw new InvalidOperationException("Операция \"" + op + "\" не поддерживается для аргументов TimeSpan и DateTime");
      }
    }

    private static object CalcDateTimeAndTimeSpan(string op, DateTime arg1, TimeSpan arg2)
    {
      arg1 = DateTime.SpecifyKind(arg1, DateTimeKind.Unspecified);
      switch (op)
      {
        case "+": return arg1 + arg2;
        case "-": return arg1 - arg2;
        default:
          throw new InvalidOperationException("Операция \"" + op + "\" не поддерживается для двух аргументов TimeSpan");
      }
    }

    private static object CalcTimeSpanAndDouble(string op, TimeSpan arg1, double arg2)
    {
      switch (op)
      {
        case "+": return arg1 + TimeSpan.FromDays(arg2);
        case "-": return arg1 - TimeSpan.FromDays(arg2);
        default:
          throw new InvalidOperationException("Операция \"" + op + "\" не поддерживается для аргументов TimeSpan и Double");
      }
    }


    #endregion

    #endregion

    #region Унарные операции

    /// <summary>
    /// Стандартное вычисление унарной операции
    /// </summary>
    /// <param name="op">Знак операции</param>
    /// <param name="arg">Аргумент</param>
    /// <returns>Результат операции</returns>
    public static object UnaryCalc(string op, object arg)
    {
      if (arg == null)
        return null;

      if (arg is decimal)
        return CalcDecimal(op, (decimal)arg);

      if (arg is double)
        return CalcDouble(op, (double)arg);

      if (arg is float)
        return CalcSingle(op, (float)arg);

      if (arg is int)
        return CalcInt(op, (int)arg);

      if (arg is TimeSpan)
        return CalcTimeSpan(op, (TimeSpan)arg);

      throw new NotSupportedException("Унарные операции не реализованы для типа " + arg.GetType().ToString());
    }

    private static object CalcDecimal(string op, decimal arg)
    {
      switch (op)
      {
        case "+": return arg;
        case "-": return -arg;
        default:
          throw new InvalidOperationException("Унарная операция \"" + op + "\" не реализована для Decimal");
      }
    }

    private static object CalcDouble(string op, double arg)
    {
      switch (op)
      {
        case "+": return arg;
        case "-": return -arg;
        default:
          throw new InvalidOperationException("Унарная операция \"" + op + "\" не реализована для Double");
      }
    }

    private static object CalcSingle(string op, float arg)
    {
      switch (op)
      {
        case "+": return arg;
        case "-": return -arg;
        default:
          throw new InvalidOperationException("Унарная операция \"" + op + "\" не реализована для Single");
      }
    }

    private static object CalcInt(string op, int arg)
    {
      switch (op)
      {
        case "+": return arg;
        case "-": return -arg;
        default:
          throw new InvalidOperationException("Унарная операция \"" + op + "\" не реализована для Int32");
      }
    }

    private static object CalcTimeSpan(string op, TimeSpan arg)
    {
      switch (op)
      {
        case "+": return arg;
        case "-": return -arg;
        default:
          throw new InvalidOperationException("Унарная операция \"" + op + "\" не реализована для TimeSpan");
      }
    }

    #endregion

    #endregion

    #region IParser Members

    #region Parse

    /// <summary>
    /// Распознание лексемы
    /// </summary>
    /// <param name="data">Данные парсинга</param>
    public void Parse(ParsingData data)
    {
      if (!_BinaryOps.IsReadOnly)
      {
        InitEndTokens();
        _BinaryOps.SetReadOnly();
        _UnaryOps.SetReadOnly();
      }

      if (data.GetChar(data.CurrPos) == '(')
      {
        data.Tokens.Add(new Token(data, this, "(", data.CurrPos, 1));
        return;
      }
      if (data.GetChar(data.CurrPos) == ')')
      {
        data.Tokens.Add(new Token(data, this, ")", data.CurrPos, 1));
        return;
      }

      // 07.03.2023
      // Для выражений вида "1+-2" "+" будет бинарной операцией, а "-" - знаком, относящимся к константе.
      // Соответственно, MathOpParser должен оставить знак "-" без попытки парсинга
      if ((data.GetChar(data.CurrPos) == '+' || data.GetChar(data.CurrPos) == '-') &&
        data.GetChar(data.CurrPos + 1) >= '0' &&
        data.GetChar(data.CurrPos + 1) <= '9' &&
        LastTokenIsMine(data))
        return;

      // На первом этапе разбора не важно, будет операция бинарной или унарной

      // Бинарные операции 
      foreach (BinaryOpDef opDef in BinaryOps)
      {
        if (data.StartsWith(opDef.Op, false))
        {
          data.Tokens.Add(new Token(data, this, opDef.Op, data.CurrPos, opDef.Op.Length));
          return;
        }
      }

      // Унарные операции 
      foreach (UnaryOpDef opDef in UnaryOps)
      {
        if (data.StartsWith(opDef.Op, false))
        {
          data.Tokens.Add(new Token(data, this, opDef.Op, data.CurrPos, opDef.Op.Length));
          return;
        }
      }
    }

    private bool LastTokenIsMine(ParsingData data)
    {
      for (int i = data.Tokens.Count - 1; i >= 0; i--)
      {
        if (data.Tokens[i].TokenType == "Space" || data.Tokens[i].TokenType == "Comment")
          continue;
        return Object.ReferenceEquals(data.Tokens[i].Parser, this);
      }
      return false;
    }

    #endregion

    #region CreateExpression

    /// <summary>
    /// Получение вычислимого выражения
    /// </summary>
    /// <param name="data">Данные парсинга</param>
    /// <param name="leftExpression">Предшествуюшее выражение</param>
    /// <returns>Вычислимое выражение или null в случае ошибки</returns>
    public IExpression CreateExpression(ParsingData data, IExpression leftExpression)
    {
      Token opToken = data.CurrToken; // после поиска правого выражения ссылка изменится
      data.SkipToken(); // Пропускаем знак операции

      if (opToken.TokenType == "(")
        return CreateParenthesExpression(data, leftExpression);
      if (opToken.TokenType == ")")
      {
        opToken.SetError("Непарная закрывающая скобка");
        return null;
      }

      // 08.11.2022
      if (leftExpression == null &&
        UnaryOps.Contains(opToken.TokenType) &&
        data.CurrTokenType == "Const")
      {
        Token constToken = data.CurrToken;
        IExpression constExpr = constToken.Parser.CreateExpression(data, null);
        UnaryExpression opExpr = new UnaryExpression(opToken.TokenType, constExpr, UnaryOps[opToken.TokenType].CalcMethod);
        data.TokenMap.Add(opToken, opExpr);
        return opExpr;
      }


      // 07.09.2015 Лексемы, которые могут завершать правую часть выражение.
      // Например, для выражения "a+b*c" правым выражением будет "b*c",
      // а для "a+b-c" будет "b", а "-с" вычисляется отдельно

      string[] endTokens = data.EndTokens;

      if (leftExpression != null)
      {
        int thisPriority = BinaryOps[opToken.TokenType].Priority;
        if (endTokens == null)
          endTokens = _EndTokensDict[thisPriority];
        else
          endTokens = DataTools.MergeArrays<string>(_EndTokensDict[thisPriority], endTokens);
      }

      IExpression rightExpession = data.Parsers.CreateSubExpression(data, endTokens); // получение правого выражения
      if (rightExpession == null)
      {
        if (data.FirstErrorToken == null)
          opToken.SetError("Не найден правый операнд для операции \"" + opToken.TokenType + "\"");
        return null;
      }

      if (leftExpression == null)
      {
        // Если левого операнда нет, то может быть только унарная операция
        if (!UnaryOps.Contains(opToken.TokenType))
        {
          //data.CurrToken. Исправлено 10.01.2022
          opToken.SetError("Не найден левый операнд для операции \"" + opToken.TokenType + "\". Операция не может быть унарной");
          return null;
        }

        UnaryExpression opExpr = new UnaryExpression(opToken.TokenType, rightExpession, UnaryOps[opToken.TokenType].CalcMethod);
        data.TokenMap.Add(opToken, opExpr);
        return opExpr;
      }

      // Формальность
      if (!BinaryOps.Contains(opToken.TokenType))
      {
        //data.CurrToken. Исправлено 10.01.2022
        opToken.SetError("Операция \"" + opToken.TokenType + "\" не может быть бинарной");
        return null;
      }


      BinaryExpression leftExpression2 = leftExpression as BinaryExpression;
      if (leftExpression2 != null)
      {
        int leftPriority = GetPriority(leftExpression2.Op);
        int currPriority = GetPriority(opToken.TokenType);
        if (currPriority > leftPriority)
        {
          // Текущая операция ("*") имеет больший приоритет, чем предыдущая ("+")
          // Выполняем замену

          // Текущая операция
          BinaryExpression expr2 = new BinaryExpression(opToken.TokenType, leftExpression2.RightExpression, rightExpession, BinaryOps[opToken.TokenType].CalcMethod);

          BinaryExpression opExpr3 = new BinaryExpression(leftExpression2.Op, leftExpression2.LeftExpression, expr2, leftExpression2.CalcMethod);
          data.TokenMap.Add(opToken, opExpr3);
          return opExpr3;
        }
      }

      // Обычный порядок операции
      BinaryExpression opExpr4 = new BinaryExpression(opToken.TokenType, leftExpression, rightExpession, BinaryOps[opToken.TokenType].CalcMethod);
      data.TokenMap.Add(opToken, opExpr4);
      return opExpr4;
    }

    #region EndTokens()

    /// <summary>
    /// Завершающие лексемы.
    /// Ключ - приоритет текущей операции
    /// Значение - массив лексем с таким же или меньшим приоритетом
    /// </summary>
    private Dictionary<int, string[]> _EndTokensDict;

    private void InitEndTokens()
    {
      _EndTokensDict = new Dictionary<int, string[]>();

      for (int i = 0; i < _BinaryOps.Count; i++)
      {
        int priority = _BinaryOps[i].Priority;
        if (!_EndTokensDict.ContainsKey(priority))
        {
          List<string> ops = new List<string>();
          for (int j = 0; j < _BinaryOps.Count; j++)
          {
            if (_BinaryOps[j].Priority <= priority)
              ops.Add(_BinaryOps[j].Op);
          }
          _EndTokensDict.Add(priority, ops.ToArray());
        }
      }
    }

    #endregion

    /// <summary>
    /// Получить приоритет для операции
    /// </summary>
    /// <param name="op"></param>
    /// <returns></returns>
    private int GetPriority(string op)
    {
      int p = BinaryOps.IndexOf(op);
      if (p < 0)
        throw new ArgumentException("Операции \"" + op + "\" нет в списке бинарных операций", "op");

      return BinaryOps[p].Priority;
    }


    private static IExpression CreateParenthesExpression(ParsingData data, IExpression leftExpression)
    {
      Token openToken = data.Tokens[data.CurrTokenIndex - 1];
      //Data.SkipToken(); было пропущено в вызывающем методе

      if (leftExpression != null)
      {
        openToken.SetError("Перед открывающей скобкой должна идти операция");
        // ? можно продолжить обзор
      }

      IExpression expr = data.Parsers.CreateSubExpression(data, new string[] { ")" });
      if (expr == null)
      {
        if (data.FirstErrorToken == null)
          openToken.SetError("Выражение в скобках не задано");
        return null;
      }

      if (data.CurrTokenType == ")")
      {
        Token closeToken = data.CurrToken;
        data.SkipToken();
        ParenthesExpression parExpr = new ParenthesExpression(expr);
        data.TokenMap.Add(openToken, parExpr);
        data.TokenMap.Add(closeToken, parExpr);
        return parExpr;
      }

      openToken.SetError("Не найдена парная закрывающая скобка");
      return null;
    }

    #endregion

    #endregion
  }

  /// <summary>
  /// Бинарная операция "+", "-", "*", "/", операции сравнения
  /// </summary>
  [Serializable]
  public sealed class BinaryExpression : IExpression
  {
    #region Конструктор

    /// <summary>
    /// Создает выражение для бинарной операции
    /// </summary>
    /// <param name="op">Лексема</param>
    /// <param name="leftExpression">Выражение слева от операции</param>
    /// <param name="rightExpression">Выражение справа от операции</param>
    /// <param name="calcMethod">Вычисляющий метод</param>
    public BinaryExpression(string op, IExpression leftExpression, IExpression rightExpression, BinaryOpDelegate calcMethod)
    {
      if (String.IsNullOrEmpty(op))
        throw new ArgumentNullException("op");
      if (leftExpression == null)
        throw new ArgumentNullException("leftExpression");
      if (rightExpression == null)
        throw new ArgumentNullException("rightExpression");
      if (calcMethod == null)
        throw new ArgumentNullException("calcMethod");

      _Op = op;
      _LeftExpression = leftExpression;
      _RightExpression = rightExpression;
      _CalcMethod = calcMethod;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Выражение слева от операции
    /// </summary>
    public IExpression LeftExpression { get { return _LeftExpression; } }
    private readonly IExpression _LeftExpression;

    /// <summary>
    /// Выражение справа от операции
    /// </summary>
    public IExpression RightExpression { get { return _RightExpression; } }
    private readonly IExpression _RightExpression;

    /// <summary>
    /// Вычисляющий метод
    /// </summary>
    public BinaryOpDelegate CalcMethod { get { return _CalcMethod; } }
    private BinaryOpDelegate _CalcMethod;

    /// <summary>
    /// Знак операции "+", "-", "*" или "/"
    /// </summary>
    public string Op { get { return _Op; } }
    private readonly string _Op;

    #endregion

    #region IExpression Members

    void IExpression.Init(ParsingData data) { }

    /// <summary>
    /// Выполнить вычисление выражения.
    /// Вычисляюся левое и правое выражение, затем вычисляется CalcMethod для операции
    /// </summary>
    /// <returns>Результат вычислей</returns>
    public object Calc()
    {
      object v1 = _LeftExpression.Calc();
      object v2 = _RightExpression.Calc();
      return _CalcMethod(Op, v1, v2);
    }

    /// <summary>
    /// Возвращает true, если и левое и правое выражение являются константами
    /// </summary>
    public bool IsConst
    {
      get
      {
        return _LeftExpression.IsConst && _RightExpression.IsConst;
      }
    }

    /// <summary>
    /// Добавляет в список левое и правое выражение
    /// </summary>
    /// <param name="expressions">Список для заполнения</param>
    public void GetChildExpressions(IList<IExpression> expressions)
    {
      expressions.Add(_LeftExpression);
      expressions.Add(_RightExpression);
    }


    /// <summary>
    /// Синтезирует выражение
    /// </summary>
    /// <param name="data">Заполняемый объект</param>
    public void Synthesize(SynthesisData data)
    {
      #region Необходимость определения скобок

      bool leftExprWithP = false;
      if (_LeftExpression is UnaryExpression)
        leftExprWithP = true;
      else if (_LeftExpression is BinaryExpression)
      {
        BinaryExpression be = (BinaryExpression)_LeftExpression;
        switch (this.Op)
        {
          case "+":
          case "-": break;
          case "*":
          case "/":
            switch (be.Op)
            {
              case "+":
              case "-":
                leftExprWithP = true;
                break;
            }
            break;
        }
      }

      bool rightExprWithP = false;
      if (_RightExpression is UnaryExpression)
        rightExprWithP = true;
      else if (_RightExpression is BinaryExpression)
      {
        BinaryExpression be = (BinaryExpression)_RightExpression;
        switch (this.Op)
        {
          case "+": break;
          case "-":
            switch (be.Op)
            {
              case "+":
              case "-":
                rightExprWithP = true;
                break;
            }
            break;

          case "*":
          case "/":
            rightExprWithP = true;
            break;
        }
      }

      #endregion

      if (leftExprWithP)
        data.Tokens.Add(new SynthesisToken(data, this, "("));
      _LeftExpression.Synthesize(data);
      if (leftExprWithP)
        data.Tokens.Add(new SynthesisToken(data, this, ")"));

      if (data.UseSpaces)
        data.Tokens.Add(new SynthesisToken(data, this, "Space", " "));
      data.Tokens.Add(new SynthesisToken(data, this, Op));
      if (data.UseSpaces)
        data.Tokens.Add(new SynthesisToken(data, this, "Space", " "));

      if (rightExprWithP)
        data.Tokens.Add(new SynthesisToken(data, this, "("));
      _RightExpression.Synthesize(data);
      if (rightExprWithP)
        data.Tokens.Add(new SynthesisToken(data, this, ")"));
    }

    #endregion

    #region Текстовое представление

    /// <summary>
    /// Возвращает свойство Op
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return Op;
#if XXX
        StringBuilder sb = new StringBuilder();
        sb.Append('(');
        sb.Append(FLeftExpression.ToString());
        sb.Append(')');
        sb.Append(Op);
        sb.Append('(');
        sb.Append(FRightExpression.ToString());
        sb.Append(')');
        return sb.ToString();
#endif
    }

    #endregion
  }

  /// <summary>
  /// Унарная операция "+" (ничего не делает) и "-"
  /// </summary>
  [Serializable]
  public sealed class UnaryExpression : IExpression
  {
    #region Конструктор

    /// <summary>
    /// Создает выражение
    /// </summary>
    /// <param name="op">Знак операции</param>
    /// <param name="rightExpression">Выражение справа от операции. Не может быть null</param>
    /// <param name="calcMethod">Вычисляющий метод</param>
    public UnaryExpression(string op, IExpression rightExpression, UnaryOpDelegate calcMethod)
    {
      if (String.IsNullOrEmpty(op))
        throw new ArgumentNullException("op");
      if (rightExpression == null)
        throw new ArgumentNullException("rightExpression");
      if (calcMethod == null)
        throw new ArgumentNullException("calcMethod");

      _Op = op;
      _RightExpression = rightExpression;
      _CalcMethod = calcMethod;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Выражение справа от знака операции
    /// </summary>
    public IExpression RightExpression { get { return _RightExpression; } }
    private readonly IExpression _RightExpression;

    /// <summary>
    /// Вычисляющий метод
    /// </summary>
    public UnaryOpDelegate CalcMethod { get { return _CalcMethod; } }
    private readonly UnaryOpDelegate _CalcMethod;

    /// <summary>
    /// Знак операции "+", "-"
    /// </summary>
    public string Op { get { return _Op; } }
    private readonly string _Op;

    #endregion

    #region IExpression Members

    void IExpression.Init(ParsingData data) { }

    /// <summary>
    /// Вычисляет выражение справа, затем - унарную операцию
    /// </summary>
    /// <returns>Результат вычисления</returns>
    public object Calc()
    {
      object v2 = _RightExpression.Calc();
      return _CalcMethod(Op, v2);
    }

    /// <summary>
    /// Возвращает true, если выражение справа является константой
    /// </summary>
    public bool IsConst
    {
      get
      {
        return _RightExpression.IsConst;
      }
    }

    /// <summary>
    /// Добавляет выражение справа в список
    /// </summary>
    /// <param name="expressions">Список для заполнения</param>
    public void GetChildExpressions(IList<IExpression> expressions)
    {
      expressions.Add(_RightExpression);
    }


    /// <summary>
    /// Синтезирует выражение
    /// </summary>
    /// <param name="data">Объект для заполнения</param>
    public void Synthesize(SynthesisData data)
    {
      data.Tokens.Add(new SynthesisToken(data, this, Op));
      data.Tokens.Add(new SynthesisToken(data, this, "("));
      _RightExpression.Synthesize(data);
      data.Tokens.Add(new SynthesisToken(data, this, ")"));
    }
    #endregion

    #region Текстовое представление

    /// <summary>
    /// Возвращает знак операции
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return Op;
#if XXX
        StringBuilder sb = new StringBuilder();
        sb.Append(Op);
        sb.Append('(');
        sb.Append(FRightExpression.ToString());
        sb.Append(')');
        return sb.ToString();
#endif
    }

    #endregion
  }

  #endregion
}
