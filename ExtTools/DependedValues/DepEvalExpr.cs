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
  /// ���������� ���������, ������������ � ������� ������ ���������
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public sealed class DepEvalExpr<T> : DepExprOA<T>
  {
    #region �����������

    /// <summary>
    /// ������� ������
    /// </summary>
    /// <param name="expression">��������� ������������� ������������ ���������</param>
    /// <param name="args">���������</param>
    public DepEvalExpr(string expression, IDepValue[] args)
      : base(args, null)
    {
      if (string.IsNullOrEmpty(expression))
        throw new ArgumentNullException("expression");

      _Expression = expression;

      InitParsedExpression(); // ����� ��������� ����������, ���� ��������� ��������

      base.BaseSetValue(Calculate(), false); // ����� ��������� ��������
    }

    #endregion

    #region ����������� ���������

    /// <summary>
    /// ��������� ������������� ������������ ���������.
    /// �������� � ������������
    /// </summary>
    public string Expression { get { return _Expression; } }
    private readonly string _Expression;

    /// <summary>
    /// ���������, ����������� ����������.
    /// ��������� � ������������ � ����� ���������� ��������������
    /// </summary>
    [NonSerialized]
    IExpression _ParsedExpression;


    /// <summary>
    /// ������������� ������������ ��������� (���� _ParsedExpression) �� ��������� ���������� ������������� (���� _Expression).
    /// ����� ����������� ���������� � ������ ������ � ���������.
    /// </summary>
    private void InitParsedExpression()
    {
      ParsingData pd = new ParsingData(_Expression);
      pd.UserData["Args"] = base.Args;
      _ParserList.Parse(pd);

      //if (pd.FirstErrorToken != null)
      //  throw new FreeLibSet.Core.ParsingException("������ ��������: " + pd.FirstErrorToken.ErrorMessage.Value.Text);

      _ParsedExpression = _ParserList.CreateExpression(pd, true);

#if DEBUG
      if (_ParsedExpression == null)
        throw new NullReferenceException("ParserList.CreateExpression() �� ������ ����������� ���������");
#endif
    }

    #endregion

    #region ���������� �������

    /// <summary>
    /// ���������� ���������
    /// </summary>
    /// <returns>���������</returns>
    protected override T Calculate()
    {
      if (_ParsedExpression == null) // ����� ��������������
        InitParsedExpression();

      object res = _ParsedExpression.Calc();
      return (T)Convert.ChangeType(res, typeof(T));
    }

    #endregion

    #region ������

    /// <summary>
    /// ������, ������������ ������� "@1", "@2".
    /// ������� ������� "Argument". � �������� AuxData ������������ ����� ���������
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
          err = new ErrorMessageItem(ErrorMessageKind.Error, "����� \"@\" ������ ���� ����� ���������");
        else if (!int.TryParse(argName, out argNum))
          err = new ErrorMessageItem(ErrorMessageKind.Error, "������ \"" + argName + "\" ������ ������������� � �����");
        else if (argNum < 1 || argNum > args.Length)
          err = new ErrorMessageItem(ErrorMessageKind.Error, "����� ��������� ������ ���� � ��������� �� 1 �� " + args.Length.ToString());
        Token tk = new Token(data, this, "Argument", data.CurrPos, ndig + 1, argNum, err);
        data.Tokens.Add(tk);
      }

      public IExpression CreateExpression(ParsingData data, IExpression leftExpression)
      {
        Token currToken = data.CurrToken;
        data.SkipToken();
        if (leftExpression != null)
          currToken.SetError("�������� �� ������ ���� ��������������� ����� ������� ���������. ��������� ��������");

        IDepValue[] args = (IDepValue[])(data.UserData["Args"]);
        int argNum = (int)(currToken.AuxData);

        return new ArgExpression(args[argNum - 1], currToken);
      }

      #endregion
    }

    private class ArgExpression : IExpression
    {
      #region �����������

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
    /// ������
    /// </summary>
    private static readonly ParserList _ParserList = CreateParserList();

    private static ParserList CreateParserList()
    {
      ParserList pl = new ParserList();

      FunctionParser fp = new FunctionParser(); // ����� MathOpParser
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