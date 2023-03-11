using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Forms;
using FreeLibSet.FIAS;
using FreeLibSet.Forms.Diagnostics;

namespace FIASDemo
{
  public partial class TestFormatForm : Form
  {
    #region Конструктор формы

    public TestFormatForm()
    {
      InitializeComponent();

      Icon = EFPApp.MainImages.Icons["Font"];
      EFPFormProvider efpForm = new EFPFormProvider(this);

      cbFormat.Items.AddRange(FiasFormatStringParser.ComponentTypes);
      efpFormat = new EFPTextComboBox(efpForm, cbFormat);
      efpFormat .TextEx.ValueChanged+=new EventHandler(efpFormat_TextChanged);

      efpRes = new EFPRichTextBox(efpForm, edRes);

      btnDebugParsing.Image = EFPApp.MainImages.Images["Debug"];
      btnDebugParsing.ImageAlign = ContentAlignment.MiddleCenter;
      EFPButton efpDebugParsing = new EFPButton(efpForm, btnDebugParsing);
      efpDebugParsing.DisplayName = "Парсинг строки";
      efpDebugParsing.ToolTipText = "Показывает результаты парсинга строки при вызове FiasFormatStringParser.TryParse()";
      efpDebugParsing.Click += new EventHandler(efpDebugParsing_Click);
    }

    #endregion

    #region Поля

    EFPTextComboBox efpFormat;

    EFPRichTextBox efpRes;

    FiasHandler Handler;

    FiasAddress Address;

    #endregion

    #region Обработчик

    private FreeLibSet.Parsing.ParsingData pd;

    private void efpFormat_TextChanged(object sender, EventArgs args)
    {
      edRes.Clear();
      try
      {
        FiasParsedFormatString fs;
        int errorStart, errorLen;
        string errorText;
        if (FiasFormatStringParser.TryParse(cbFormat.Text, out fs, out errorText, out errorStart, out errorLen, out pd))
        {
          edRes.Text = Handler.Format(Address, fs);
        }
        else
        {
          StringBuilder sb = new StringBuilder();
          sb.Append("Неправильная строка форматирования. ");
          sb.Append(Environment.NewLine);
          sb.Append(errorText);
          sb.Append(Environment.NewLine);
          edRes.Text = sb.ToString();
          //int pText = sb.Length;
          int pText = edRes.Text.Length; // RichTextBox заменил CRLF на CR - длина поменялась
          edRes.Text += cbFormat.Text;
          edRes.Select(pText + errorStart, errorLen);
          edRes.SelectionBackColor = Color.Red;
        }
      }
      catch (Exception e)
      {
        edRes.Text = "Ошибка. " + e.Message;
        edRes.SelectAll();
        edRes.SelectionBackColor = Color.Red;
      }
    }

    void efpDebugParsing_Click(object sender, EventArgs args)
    {
      DebugTools.DebugParsingData(pd, "Парсинг строки форматирования");
    }

    #endregion

    #region Статический метод запуска

    private static string _LastFormat = "TEXT";

    public static void PerformTest(FiasHandler handler, FiasAddress address)
    {
      TestFormatForm form = new TestFormatForm();
      form.Handler = handler;
      form.Address = address;
      form.efpFormat.Text = _LastFormat;
      EFPApp.ShowDialog(form, true);
      _LastFormat = form.efpFormat.Text;
    }

    #endregion
  }
}