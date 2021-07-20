using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using AgeyevAV.ExtForms;
using AgeyevAV.Parsing;
using AgeyevAV;

namespace TestCalculator
{
  public partial class MainForm : Form
  {
    #region ����������� �����

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

    #region ������ ��� ����������

    ParserList TheParser;

    ParsingData TheParsingData;

    IExpression TheExpression;

    private void InitParser()
    {
      TheParser = new ParserList();

      FunctionParser fp = new FunctionParser();
      ExcelFunctions.AddFunctions(fp);
      TheParser.Add(fp);

      TheParser.Add(new MathOpParser());
      TheParser.Add(new SpaceParser());
      NumConstParser ncp = new NumConstParser();
      ncp.AllowSingle = false;
      ncp.AllowDecimal = false;
      TheParser.Add(ncp);
      TheParser.Add(new StrConstParser());

      TestEng = Microsoft.JScript.Vsa.VsaEngine.CreateEngine();
    }

    #endregion

    #region ����������

    void edExpr_TextChanged(object Sender, EventArgs Args)
    {
      try
      {
        string Expr = edExpr.Text;

        edResType.Text = String.Empty;
        lblCheckRes.Text = String.Empty;

        TheExpression = null; // �������
        TheParsingData = new ParsingData(Expr);
        TheParser.Parse(TheParsingData);
        if (TheParsingData.FirstErrorToken != null)
        {
          edRes.Text = TheParsingData.FirstErrorToken.ErrorMessage.Value.Text;
          return;
        }

        try
        {
          TheExpression = TheParser.CreateExpression(TheParsingData);
        }
        catch (Exception e2)
        {
          edRes.Text = "������ ��������. " + e2.Message;
          return;
        }

        if (TheParsingData.FirstErrorToken != null)
        {
          edRes.Text = TheParsingData.FirstErrorToken.ErrorMessage.Value.Text;
          return;
        }

        if (TheExpression == null)
        {
          edRes.Text = "������ ���������";
          return;
        }

        object res;
        try
        {
          res = TheExpression.Calc();
        }
        catch (Exception e2)
        {
          edRes.Text = "������ ����������. "+e2.Message;
          return;
        }

        if (TheParsingData.FirstErrorToken != null)
        {
          edRes.Text = TheParsingData.FirstErrorToken.ErrorMessage.Value.Text;
          return;
        }

        if (res == null)
          edRes.Text = "[ null ]";
        else
        {
          edRes.Text = res.ToString();
          edResType.Text = res.GetType().ToString();
        }

        #region ��������

        CheckResult(Expr, res);

        #endregion
      }
      catch (Exception e)
      {
        DebugTools.ShowException(e, "������ ����������");
      }
    }

    void efpDebug_Click(object Sender, EventArgs Args)
    {
      DebugTools.DebugParsingData(TheParsingData, "�������", TheExpression);
    }

    #endregion

    #region ��� ��������

    Microsoft.JScript.Vsa.VsaEngine TestEng;

    [DebuggerStepThrough]
    private void CheckResult(string Expr, object res)
    {
      if (res == null)
        return;
      try
      {
        if (res is Int32 || DataTools.IsFloatType(res.GetType()))
        {
          string Expr2 = Expr.Replace(',', '.').Replace(" ", "."); // ����������� ���������� �����, � �� �������
          Expr2 = Expr2.Replace(';', ','); // ����������� ����������
          object res2 = Microsoft.JScript.Eval.JScriptEvaluate(Expr2, TestEng);
          if (DataTools.GetDouble(res2) != DataTools.GetDouble(res)) // double ����� "�������" ���, ��� decimal
          {
            lblCheckRes.Text = "������ ���� " + DataTools.GetString(res2);
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
        lblCheckRes.Text = "������ ��������� � JScript.Eval: " + e.Message;
        lblCheckRes.ForeColor = Color.Magenta;
      }
    }

    #endregion
  }
}