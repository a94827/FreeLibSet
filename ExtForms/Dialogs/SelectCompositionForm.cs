// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Config;
using System.IO;
using FreeLibSet.Controls;
using FreeLibSet.Core;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Форма выбора сохраненной композиции рабочего стола
  /// </summary>
  internal partial class SelectCompositionForm : Form
  {
    #region Конструктор формы

    public SelectCompositionForm()
    {
      InitializeComponent();
      Icon = EFPApp.MainImages.Icons["SavedCompositions"];

      efpForm = new EFPFormProvider(this);

      efpPreview = new EFPPictureBox(efpForm, pbPreview);
      efpPreview.Control.SizeMode = PictureBoxSizeMode.Zoom;

      efpSelCB = new EFPTextComboBox(efpForm, cbParamSet.TheCB);
      efpSelCB.CanBeEmpty = true;
      efpSelCB.DisplayName = EFPCommandItem.RemoveMnemonic(grpPresets.Text);

      efpSaveButton = new EFPButton(efpForm, cbParamSet.SaveButton);
      efpSaveButton.DisplayName = Res.ParamSetComboBox_Name_SaveButton;
      efpSaveButton.ToolTipText = Res.ParamSetComboBox_ToolTip_SaveButton;
      //efpSaveButton.Click += new EventHandler(efpSaveButton_Click);
      cbParamSet.SaveClick += new ParamSetComboBoxSaveEventHandler(cbParamSet_SaveClick);

      efpDelButton = new EFPButton(efpForm, cbParamSet.DeleteButton);
      efpDelButton.DisplayName = Res.ParamSetComboBox_Name_DelButton;
      efpDelButton.ToolTipText = Res.ParamSetComboBox_ToolTip_DelButton;
      //efpDelButton.Click += new EventHandler(efpDelButton_Click);
      cbParamSet.DeleteClick += new ParamSetComboBoxItemEventHandler(cbParamSet_DeleteClick);

      cbParamSet.ShowImages = EFPApp.ShowListImages;

      cbParamSet.ItemSelected += new ParamSetComboBoxItemEventHandler(cbParamSet_ItemSelected);
      efpSelCB.TextEx.ValueChanged += new EventHandler(efpSelCB_TextChanged);

      btnXml.Image = EFPApp.MainImages.Images["XML"];
      btnXml.ImageAlign = ContentAlignment.MiddleCenter;
      EFPButton efpXml = new EFPButton(efpForm, btnXml);
      efpXml.DisplayName = Res.SelectCompositionForm_Name_XmlButton;
      efpXml.ToolTipText = Res.SelectCompositionForm_ToolTip_XmlButton;
      efpXml.Click += new EventHandler(efpXml_Click);
    }

    protected override void OnLoad(EventArgs args)
    {
      base.OnLoad(args);

      try
      {
        #region Текущая композиция

        _CurrPart = new TempCfg();
        EFPApp.Interface.SaveComposition(_CurrPart);
        _CurrSnapshot = EFPApp.CreateSnapshot(true);

        LoadItems();

        cbParamSet.SelectedCode = "Current";
        cbParamSet_ItemSelected(null, null);

        #endregion
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e);
      }

      cbParamSet.Select();
    }

    private void LoadItems()
    {
      cbParamSet.Items.Clear();

      #region Текущая композиция

      ParamSetComboBoxItem currItem = new ParamSetComboBoxItem("Current", Res.SelectCompositionForm_Msg_CurrentComposition,
        "ArrowRight", /*DateTime.Now*/null, 1, _CurrPart.MD5Sum());
      cbParamSet.Items.Add(currItem);

      #endregion

      #region Именные секции

      EFPAppCompositionHistoryItem[] items;
      items = EFPAppCompositionHistoryHandler.GetUserItems();
      for (int i = 0; i < items.Length; i++)
      {
        ParamSetComboBoxItem cbItem = new ParamSetComboBoxItem(items[i].UserSetName,
          items[i].DisplayName,
          "User", /*Items[i].Time*/null, 2, items[i].MD5);
        cbItem.Tag = items[i];
        cbParamSet.Items.Add(cbItem);
      }

      #endregion

      #region История

      items = EFPAppCompositionHistoryHandler.GetHistoryItems();
      for (int i = 0; i < items.Length; i++)
      {
        ParamSetComboBoxItem cbItem = new ParamSetComboBoxItem(items[i].UserSetName,
          items[i].DisplayName,
          "Time", /*Items[i].Time*/null, 2, items[i].MD5);
        cbItem.Tag = items[i];
        cbParamSet.Items.Add(cbItem);
      }

      #endregion
    }

    internal EFPFormProvider efpForm;

    #endregion

    #region Текущая композиция

    private TempCfg _CurrPart;

    private Bitmap _CurrSnapshot;

    #endregion

    #region Изображение и сообщение об ошибке

    private readonly EFPPictureBox efpPreview;

    void SetImage(Image image)
    {
      if (image == null)
      {
        SetWarning(Res.SelectCompositionForm_Err_NoImage);
        return;
      }
      try
      {
        pbPreview.Image = image;
        lblInfo.Visible = false;
        pbPreview.Visible = true;
      }
      catch (Exception e)
      {
        SetError(String.Format(Res.SelectCompositionForm_ErrTitle_SetImage, e.Message));
      }
    }

    private void SetError(string text)
    {
      lblInfo.Text = text;
      lblInfo.Icon = MessageBoxIcon.Error;
      pbPreview.Visible = false;
      lblInfo.Visible = true;
    }

    private void SetWarning(string text)
    {
      lblInfo.Text = text;
      lblInfo.Icon = MessageBoxIcon.Warning;
      pbPreview.Visible = false;
      lblInfo.Visible = true;
    }

    #endregion

    #region Комбоблок истории

    private readonly EFPTextComboBox efpSelCB;

    private readonly EFPButton efpSaveButton;

    private readonly EFPButton efpDelButton;

    /// <summary>
    /// Текущая выбранная композиция.
    /// null, если выбраная текущая композиция
    /// </summary>
    public EFPAppCompositionHistoryItem SelectedItem;

    void cbParamSet_ItemSelected(object sender, ParamSetComboBoxItemEventArgs args)
    {
      SelectedItem = null;
      if (cbParamSet.SelectedItem != null)
        SelectedItem = cbParamSet.SelectedItem.Tag as EFPAppCompositionHistoryItem;

      if (SelectedItem == null)
        SetImage(_CurrSnapshot);
      else
      {
        EFPConfigSectionInfo ConfigInfo = EFPAppCompositionHistoryHandler.GetSnapshotConfigInfo(SelectedItem);
        SetImage(EFPApp.SnapshotManager.LoadSnapshot(ConfigInfo));
      }

      InitButtonsEnabled();
    }

    void efpSelCB_TextChanged(object sender, EventArgs args)
    {
      InitButtonsEnabled();
    }

    private void InitButtonsEnabled()
    {
      if (SelectedItem == null)
        efpSaveButton.Enabled = true;
      else if (String.IsNullOrEmpty(efpSelCB.Text) || efpSelCB.Text.StartsWith("[", StringComparison.Ordinal))
        efpSaveButton.Enabled = false;
      else
        efpSaveButton.Enabled = true;
      efpDelButton.Enabled = SelectedItem != null;
    }

    void cbParamSet_SaveClick(object sender, ParamSetComboBoxSaveEventArgs args)
    {
      string Name = efpSelCB.Text;
      if (Name.StartsWith("[", StringComparison.Ordinal))
        Name = String.Empty;

      string UserSetName;
      if (SelectedItem == null)
      {
        if (Name.Length == 0)
        {
          if (EFPApp.MessageBox(Res.SelectCompositionForm_Msg_SaveInHistory,
            efpSaveButton.DisplayName, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
            return;
          UserSetName = EFPAppCompositionHistoryHandler.SaveHistory(_CurrPart, _CurrSnapshot).UserSetName;
        }
        else
          UserSetName = EFPAppCompositionHistoryHandler.SaveUser(Name, _CurrPart, _CurrSnapshot).UserSetName;
      }
      else
      {
        if (Name.Length == 0)
        {
          EFPApp.ShowTempMessage(Res.SelectCompositionForm_Err_NaneIsEmpty);
          return;
        }
        UserSetName = EFPAppCompositionHistoryHandler.SaveUser(Name, _CurrPart, _CurrSnapshot).UserSetName;
      }
      LoadItems();
      cbParamSet.SelectedCode = UserSetName;
    }

    void cbParamSet_DeleteClick(object sender, ParamSetComboBoxItemEventArgs args)
    {
      if (SelectedItem == null)
      {
        EFPApp.ShowTempMessage(Res.SelectCompositionForm_Err_CannotDeleteCurrent);
        return;
      }
      if (EFPApp.MessageBox(String.Format(Res.SelectCompositionForm_Msg_ConfirmDel, SelectedItem.DisplayName),
        efpDelButton.DisplayName, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
        return;

      EFPAppCompositionHistoryHandler.Delete(SelectedItem);
      LoadItems();
    }

    #endregion

    #region Кнопка XML

    void efpXml_Click(object sender, EventArgs args)
    {
      if (SelectedItem == null)
        EFPApp.ShowTextView(XmlTools.XmlDocumentToString(_CurrPart.Document), "Текущая композиция");
      else
      {
        CfgPart cfg;
        using (EFPApp.ConfigManager.GetConfig(EFPAppCompositionHistoryHandler.GetConfigInfo(SelectedItem),
          EFPConfigMode.Read, out cfg))
        {
          TempCfg cfg2 = new TempCfg();
          cfg.CopyTo(cfg2);
          EFPApp.ShowTextView(XmlTools.XmlDocumentToString(cfg2.Document), SelectedItem.DisplayName);
        }
      }
    }

    #endregion
  }

  /// <summary>
  /// Выводит диалог загрузки сохраненной композиции рабочего стола.
  /// Если пользователь нажал [OK], автоматически вызывается <see cref="EFPApp.LoadComposition()"/>.
  /// Свойство <see cref="EFPApp.CompositionHistoryCount"/> должно быть установлено в значение, отличное от 0.
  /// </summary>
  public sealed class SelectCompositionDialog
  {
    #region Конструктор

    /// <summary>
    /// Конструктор задает параметры диалога по умолчанию
    /// </summary>
    public SelectCompositionDialog()
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Контекст справки, вызываемой по F1
    /// </summary>
    public string HelpContext { get { return _HelpContext; } set { _HelpContext = value; } }
    private string _HelpContext;

    #endregion

    #region Вывод диалога

    /// <summary>
    /// Основной метод - вывод формы диалога
    /// </summary>
    /// <returns><see cref="DialogResult.OK"/>, если пользователь выполнил загрузку композиции рабочего стола</returns>
    public DialogResult ShowDialog()
    {
      SelectCompositionForm frm = new SelectCompositionForm();
      frm.efpForm.HelpContext = HelpContext;
      if (EFPApp.ShowDialog(frm, true) == DialogResult.OK)
      {
        if (frm.SelectedItem != null)
        {
          CfgPart cfg;
          using (EFPApp.ConfigManager.GetConfig(EFPAppCompositionHistoryHandler.GetConfigInfo(frm.SelectedItem), EFPConfigMode.Read, out cfg))
          {
            EFPApp.LoadComposition(cfg);
          }
          return DialogResult.OK;
        }
        else
          return DialogResult.Cancel;
      }
      else
        return DialogResult.Cancel;
    }

    #endregion
  }
}
