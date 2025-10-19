using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Core;
using FreeLibSet.Forms;
using FreeLibSet.Forms.Diagnostics;

namespace WinFormsDemo.EFPClipboardDemo
{
  public partial class EFPClipboardForm : Form
  {
    #region Конструктор формы

    public EFPClipboardForm()
    {
      InitializeComponent();

      _TestObj = new EFPClipboard();

      EFPFormProvider efpForm = new EFPFormProvider(this);

      #region Кнопки методов

      EFPButton efpSetDataObject = new EFPButton(efpForm, btnSetDataObject);
      efpSetDataObject.Click += EfpSetDataObject_Click;

      EFPButton efpGetDataObject = new EFPButton(efpForm, btnGetDataObject);
      efpGetDataObject.Click += EfpGetDataObject_Click;


      EFPButton efpSetData = new EFPButton(efpForm, btnSetData);
      efpSetData.Click += EfpSetData_Click;

      EFPButton efpGetData = new EFPButton(efpForm, btnGetData);
      efpGetData.Click += EfpGetData_Click;


      EFPButton efpSetText = new EFPButton(efpForm, btnSetText);
      efpSetText.Click += EfpSetText_Click;

      EFPButton efpGetText = new EFPButton(efpForm, btnGetText);
      efpGetText.Click += EfpGetText_Click;


      EFPButton efpSetTextMatrix = new EFPButton(efpForm, btnSetTextMatrix);
      efpSetTextMatrix.Click += EfpSetTextMatrix_Click;

      EFPButton efpGetTextMatrix = new EFPButton(efpForm, btnGetTextMatrix);
      efpGetTextMatrix.Click += EfpGetTextMatrix_Click;


      EFPButton efpSetImage = new EFPButton(efpForm, btnSetImage);
      efpSetImage.Click += EfpSetImage_Click;

      EFPButton efpGetImage = new EFPButton(efpForm, btnGetImage);
      efpGetImage.Click += EfpGetImage_Click;

      #endregion

      #region Управляющие свойства

      efpRepeatCount = new EFPInt32EditBox(efpForm, edRepeatCount);
      efpRepeatCount.Value = _TestObj.RepeatCount;
      efpRepeatCount.ValueEx.ValueChanged += efpRepeatCount_ValueChanged;

      efpRepeatDelay = new EFPInt32EditBox(efpForm, edRepeatDelay);
      efpRepeatDelay.Value = _TestObj.RepeatDelay;
      efpRepeatDelay.ValueEx.ValueChanged += efpRepeatDelay_ValueChanged;

      cbErrorHandling.Items.AddRange(Enum.GetNames(typeof(EFPClipboardErrorHandling)));
      efpErrorHandling = new EFPListComboBox(efpForm, cbErrorHandling);
      efpErrorHandling.CanBeEmpty = false;
      efpErrorHandling.SelectedItemString = _TestObj.ErrorHandling.ToString();
      efpErrorHandling.SelectedItemStringEx.ValueChanged += efpErrorHandling_ValueChanged;

      efpErrorIfEmpty = new EFPCheckBox(efpForm, cbErrorIfEmpty);
      efpErrorIfEmpty.Checked = _TestObj.ErrorIfEmpty;
      efpErrorIfEmpty.CheckedEx.ValueChanged += efpErrorIfEmpty_ValueChanged;

      #endregion

      #region Ошибки и Clipboard

      EFPTextBox efpExceptionClass = new EFPTextBox(efpForm, edExceptionClass);
      EFPTextBox efpExceptionMessage = new EFPTextBox(efpForm, edExceptionMessage);

      btnExceptionView.Image = EFPApp.MainImages.Images["View"];
      btnExceptionView.ImageAlign = ContentAlignment.MiddleCenter;
      efpExceptionView = new EFPButton(efpForm, btnExceptionView);
      efpExceptionView.Click += EfpExceptionView_Click;

      efpForm.UpdateByTimeHandlers.Add(new EFPUpdateByTimeHandler(UpdateByTime_Tick));

      #endregion
    }

    private EFPClipboard _TestObj;

    #endregion

    #region Кнопки методов

    private void EfpSetDataObject_Click(object sender, EventArgs args)
    {
      DataObject dobj = new DataObject();
      dobj.SetText("Hello, world");
      dobj.SetImage(EFPApp.MainImages.Images["Ok"]);
      _TestObj.SetDataObject(dobj, true);
    }

    private void EfpGetDataObject_Click(object sender, EventArgs args)
    {
      DebugTools.DebugObject(_TestObj.GetDataObject(), "GetDataObject()");
    }


    private void EfpSetData_Click(object sender, EventArgs args)
    {
      throw new NotImplementedException();
    }

    private void EfpGetData_Click(object sender, EventArgs args)
    {
      TextInputDialog dlg = new TextInputDialog();
      dlg.Title = "GetData()";
      dlg.Prompt = "Format";
      dlg.CanBeEmpty = false;
      if (dlg.ShowDialog() != DialogResult.OK)
        return;

      DebugTools.DebugObject(_TestObj.GetData(dlg.Text), "GetData(" + dlg.Text + ")");
    }


    private void EfpSetText_Click(object sender, EventArgs args)
    {
      _TestObj.SetText("Test " + DateTime.Now.ToString("G"));
    }

    private void EfpGetText_Click(object sender, EventArgs args)
    {
      DebugTools.DebugObject(_TestObj.GetText(), "GetText()");
    }

    private void EfpSetTextMatrix_Click(object sender, EventArgs args)
    {
      string[,] a = new string[2, 3] { 
        {"One", "Two", "Tree" }, 
        {"Four", "Five", "Six"} };
      _TestObj.SetTextMatrix(a);
    }

    private void EfpGetTextMatrix_Click(object sender, EventArgs args)
    {
      string[,] a = _TestObj.GetTextMatrix();
      //DebugTools.DebugObject(a, "GetTextMatrix()");
      if (a == null)
      {
        EFPApp.MessageBox("null", "GetTextMatrix()");
        return;
      }

      int nR = a.GetLength(0);
      int nC = a.GetLength(1);
      SimpleGridForm form2 = new SimpleGridForm();
      form2.Text = "GetTextMatrix() (" + nR.ToString() + "x" + nC.ToString() + ")";
      EFPDataGridView efpGr = new EFPDataGridView(form2.ControlWithToolBar);

      efpGr.Control.ColumnCount = nC;
      efpGr.Control.RowCount = nR;

      for (int i = 0; i < nR; i ++)
      {
        for (int j = 0; j < nC; j++)
          efpGr.Control[j, i].Value = a[i, j];
      }
      efpGr.DisableOrdering();
      efpGr.Control.ReadOnly = true;
      efpGr.ReadOnly = true;
      efpGr.CanView = false;

      EFPApp.ShowDialog(form2, true);
    }

    private void EfpSetImage_Click(object sender, EventArgs args)
    {
      _TestObj.SetImage(EFPApp.MainImages.Images["User"]);
    }

    private void EfpGetImage_Click(object sender, EventArgs args)
    {
      DebugTools.DebugObject(_TestObj.GetImage(), "GetImage()");
    }

    #endregion

    #region Управляющие свойства

    EFPInt32EditBox efpRepeatCount, efpRepeatDelay;

    EFPListComboBox efpErrorHandling;

    EFPCheckBox efpErrorIfEmpty;

    private void efpRepeatCount_ValueChanged(object sender, EventArgs args)
    {
      _TestObj.RepeatCount = efpRepeatCount.Value;
    }

    private void efpRepeatDelay_ValueChanged(object sender, EventArgs args)
    {
      _TestObj.RepeatDelay = efpRepeatDelay.Value;
    }

    private void efpErrorHandling_ValueChanged(object sender, EventArgs args)
    {
      _TestObj.ErrorHandling = (EFPClipboardErrorHandling)(efpErrorHandling.SelectedIndex);
    }

    private void efpErrorIfEmpty_ValueChanged(object sender, EventArgs args)
    {
      _TestObj.ErrorIfEmpty = efpErrorIfEmpty.Checked;
    }

    #endregion

    #region Результаты

    EFPButton efpExceptionView;

    private void EfpExceptionView_Click(object sender, EventArgs args)
    {
      Exception e = efpExceptionView.Tag as Exception;
      EFPApp.ShowException(e, "Exception");
    }

    #endregion

    #region  Обновление по таймеру

    private void UpdateByTime_Tick(object sender, EventArgs args)
    {
      try
      {
        Exception e1 = _TestObj.Exception;
        efpExceptionView.Tag = e1;
        if (e1 == null)
        {
          edExceptionClass.Text = String.Empty;
          edExceptionMessage.Text = String.Empty;
          efpExceptionView.Enabled = false;
        }
        else
        {
          edExceptionClass.Text = e1.GetType().ToString();
          edExceptionMessage.Text = e1.Message;
          efpExceptionView.Enabled = true;
        }

        string[] a = Clipboard.GetDataObject().GetFormats();
        int p = Array.IndexOf<string>(a, "Text");
        if (p >= 0)
        {
          string s = DataTools.GetString(Clipboard.GetText());
          if (s.Length > 30)
            s = s.Substring(0, 30) + " ...";
          a[p] += " (" + s + ")";
        }
        edClipboardContents.Lines = a;
      }
      catch (Exception e2)
      {
        edClipboardContents.Text = "Error. " + e2.Message;
      }
    }

    #endregion
  }
}
