using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using FreeLibSet.Forms;
using FreeLibSet.Parsing;
using FreeLibSet.Core;
using FreeLibSet.Forms.Diagnostics;

#pragma warning disable 0618 // "Obsolete" warning for VsaEngine

namespace TestCalculator
{
  public partial class MainForm : Form
  {
    #region Конструктор формы

    public MainForm()
    {
      InitializeComponent();

      InitParser();

      EFPFormProvider efpForm = new EFPFormProvider(this);
      edExpr.TextChanged += new EventHandler(edExpr_TextChanged);

      btnDebug.Image = EFPApp.MainImages.Images["Debug"];
      btnDebug.ImageAlign = ContentAlignment.BottomLeft;
      EFPButton efpDebug = new EFPButton(efpForm, btnDebug);
      efpDebug.Click += new EventHandler(efpDebug_Click);

      edExpr_TextChanged(null, null);
    }

    #endregion

    #region Объект для вычисления

    private ParserList _TheParser;

    private ParsingData _TheParsingData;

    private IExpression _TheExpression;

    private void InitParser()
    {
      _TheParser = new ParserList();

      FunctionParser fp = new FunctionParser();
      ExcelFunctions.AddFunctions(fp);
      _TheParser.Add(fp);

      _TheParser.Add(new MathOpParser());
      _TheParser.Add(new SpaceParser());
      NumConstParser ncp = new NumConstParser();
      ncp.AllowSingle = false;
      ncp.AllowDecimal = false;
      _TheParser.Add(ncp);
      _TheParser.Add(new StrConstParser());

      _TestEng = Microsoft.JScript.Vsa.VsaEngine.CreateEngine();
    }

    #endregion

    #region Вычисление

    void edExpr_TextChanged(object sender, EventArgs args)
    {
      try
      {
        string expr = edExpr.Text;

        edResType.Text = String.Empty;
        lblCheckRes.Text = String.Empty;

        _TheExpression = null; // очищаем
        _TheParsingData = new ParsingData(expr);
        _TheParser.Parse(_TheParsingData);
        if (_TheParsingData.FirstErrorToken != null)
        {
          edRes.Text = _TheParsingData.FirstErrorToken.ErrorMessage.Value.Text;
          return;
        }

        try
        {
          _TheExpression = _TheParser.CreateExpression(_TheParsingData);
        }
        catch (Exception e2)
        {
          edRes.Text = "Ошибка парсинга. " + e2.Message;
          return;
        }

        if (_TheParsingData.FirstErrorToken != null)
        {
          edRes.Text = _TheParsingData.FirstErrorToken.ErrorMessage.Value.Text;
          return;
        }

        if (_TheExpression == null)
        {
          edRes.Text = "Нельзя вычислить";
          return;
        }

        object res;
        try
        {
          res = _TheExpression.Calc();
        }
        catch (Exception e2)
        {
          edRes.Text = "Ошибка вычисления. " + e2.Message;
          return;
        }

        if (_TheParsingData.FirstErrorToken != null)
        {
          edRes.Text = _TheParsingData.FirstErrorToken.ErrorMessage.Value.Text;
          return;
        }

        if (res == null)
          edRes.Text = "[ null ]";
        else
        {
          edRes.Text = res.ToString();
          edResType.Text = res.GetType().ToString();
        }

        #region Проверка

        CheckResult(expr, res);

        #endregion
      }
      catch (Exception e)
      {
        DebugTools.ShowException(e, "Ошибка вычисления");
      }
    }

    void efpDebug_Click(object sender, EventArgs args)
    {
      DebugTools.DebugParsingData(_TheParsingData, "Парсинг", _TheExpression);
    }

    #endregion

    #region Для проверки

    Microsoft.JScript.Vsa.VsaEngine _TestEng;

    [DebuggerStepThrough]
    private void CheckResult(string expr, object res)
    {
      if (res == null)
        return;
      try
      {
        if (res is Boolean)
          res = (bool)res ? 1 : 0;
        if (res is Int32 || MathTools.IsFloatType(res.GetType()))
        {
          string expr2 = expr.Replace(',', '.'); // вычислитель использует точку, а не запятую
          expr2 = expr2.Replace(" ", ""); // пробелы недопустимы
          expr2 = expr2.Replace(';', ','); // разделитель аргументов
          object res2 = Microsoft.JScript.Eval.JScriptEvaluate(expr2, _TestEng);
          if (DataTools.GetDouble(res2) != DataTools.GetDouble(res)) // double более "широкий" тип, чем decimal
          {
            lblCheckRes.Text = "Должно быть " + DataTools.GetString(res2);
            lblCheckRes.ForeColor = Color.Red;
          }
          else
          {
            lblCheckRes.Text = "OK";
            lblCheckRes.ForeColor = Color.Green;
          }
        }
      }
      catch (Exception e)
      {
        lblCheckRes.Text = "Нельзя проверить в JScript.Eval: " + e.Message;
        lblCheckRes.ForeColor = Color.Magenta;
      }
    }

    #endregion
  }
}
#pragma warning restore 0618
