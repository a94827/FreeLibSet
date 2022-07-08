// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Parsing;
using FreeLibSet.Core;

namespace FreeLibSet.DependedValues
{
  /// <summary>
  /// Вычислимое выражение, определяемое с помощью строки выражения
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public sealed class DepEvalExpr<T> : DepExprOA<T>
  {
    #region Конструктор

    /// <summary>
    /// Создает объект
    /// </summary>
    /// <param name="expression">Текстовое представление вычисляемого выражения</param>
    /// <param name="args">Аргументы</param>
    public DepEvalExpr(string expression, IDepValue[] args)
      : base(args, null)
    {
      if (string.IsNullOrEmpty(expression))
        throw new ArgumentNullException("expression");

      _Expression = expression;

      InitParsedExpression(); // чтобы выбросить исключение, если выражение неверное

      base.BaseSetValue(Calculate(), false); // Сразу вычисляем значение
    }

    #endregion

    #region Вычисляемое выражение

    /// <summary>
    /// Текстовое представление вычисляемого выражения.
    /// Задается в конструкторе
    /// </summary>
    public string Expression { get { return _Expression; } }
    private readonly string _Expression;

    /// <summary>
    /// Интерфейс, выполняющий вычисления.
    /// Создается в конструкторе и после выполнения десериализации
    /// </summary>
    [NonSerialized]
    IExpression _ParsedExpression;


    /// <summary>
    /// Инициализация вычисляемого выражения (поля _ParsedExpression) на основании текстового представления (поле _Expression).
    /// Метод выбрасывает исключение в случае ошибок в выражении.
    /// </summary>
    private void InitParsedExpression()
    {
      ParsingData pd = new ParsingData(_Expression);
      pd.UserData["Args"] = base.Args;
      _ParserList.Parse(pd);

      //if (pd.FirstErrorToken != null)
      //  throw new FreeLibSet.Core.ParsingException("Ошибка парсинга: " + pd.FirstErrorToken.ErrorMessage.Value.Text);

      _ParsedExpression = _ParserList.CreateExpression(pd, true);

#if DEBUG
      if (_ParsedExpression == null)
        throw new NullReferenceException("ParserList.CreateExpression() не вернул вычисляемое выражение");
#endif
    }

    #endregion

    #region Выполнение расчета

    /// <summary>
    /// Вычисление выражения
    /// </summary>
    /// <returns>Результат</returns>
    protected override T Calculate()
    {
      if (_ParsedExpression == null) // после десериализации
        InitParsedExpression();

      object res = _ParsedExpression.Calc();
      return (T)Convert.ChangeType(res, typeof(T));
    }

    #endregion

    #region Парсер

    /// <summary>
    /// Парсер, распознающий лексемы "@1", "@2".
    /// Создает лексему "Argument". В качестве AuxData используется номер аргумента
    /// </summary>
    private class ArgParser : IParser
    {
      #region IParser Members

      public void Parse(ParsingData data)
      {
        if (data.CurrChar != '@')
          return;
        int ndig = 0;
        while (data.GetChar(data.CurrPos + ndig + 1) >= '0' && data.GetChar(data.CurrPos + ndig + 1) <= '9')
          ndig++;

        IDepValue[] args = (IDepValue[])(data.UserData["Args"]);

        ErrorMessageItem? err = null;
        string argName = data.Text.Text.Substring(data.CurrPos + 1, ndig);
        int argNum = 0;
        if (ndig == 0)
          err = new ErrorMessageItem(ErrorMessageKind.Error, "После \"@\" должен идти номер аргумента");
        else if (!int.TryParse(argName, out argNum))
          err = new ErrorMessageItem(ErrorMessageKind.Error, "Строку \"" + argName + "\" нельзя преобразовать в число");
        else if (argNum < 1 || argNum > args.Length)
          err = new ErrorMessageItem(ErrorMessageKind.Error, "Номер аргумента должен быть в диапазоне от 1 до " + args.Length.ToString());
        Token tk = new Token(data, this, "Argument", data.CurrPos, ndig + 1, argNum, err);
        data.Tokens.Add(tk);
      }

      public IExpression CreateExpression(ParsingData data, IExpression leftExpression)
      {
        Token currToken = data.CurrToken;
        data.SkipToken();
        if (leftExpression != null)
          currToken.SetError("Аргумент не должен идти непосредственно после другого выражения. Ожидалась операция");

        IDepValue[] args = (IDepValue[])(data.UserData["Args"]);
        int argNum = (int)(currToken.AuxData);

        return new ArgExpression(args[argNum - 1], currToken);
      }

      #endregion
    }

    private class ArgExpression : IExpression
    {
      #region Конструктор

      public ArgExpression(IDepValue arg, Token token)
      {
        _Arg = arg;
        _Token = token;
      }

      private IDepValue _Arg;

      private Token _Token;

      #endregion

      #region IExpression Members

      public object Calc()
      {
        return _Arg.Value;
      }

      public bool IsConst
      {
        get { return _Arg.IsConst; }
      }

      public void GetTokens(IList<Token> tokens)
      {
        tokens.Add(_Token);
      }

      public void GetChildExpressions(IList<IExpression> expressions)
      {
      }

      public void Synthesize(SynthesisData data)
      {
        data.Tokens.Add(new SynthesisToken(data, this, _Token.Text));
      }

      #endregion
    }

    /// <summary>
    /// Парсер
    /// </summary>
    private static readonly ParserList _ParserList = CreateParserList();

    private static ParserList CreateParserList()
    {
      ParserList pl = new ParserList();

      FunctionParser fp = new FunctionParser(); // перед MathOpParser
      ExcelFunctions.AddFunctions(fp);
      pl.Add(fp);

      pl.Add(new ArgParser());
      pl.Add(new MathOpParser());

      NumConstParser ncp = new NumConstParser();
      ncp.NumberFormat = StdConvert.NumberFormat;
      pl.Add(ncp);

      pl.Add(new StrConstParser());

      pl.Add(new SpaceParser());

      return pl;
    }


    #endregion
  }
}
