using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using AgeyevAV.Config;
using System.IO;

namespace AgeyevAV.ExtForms
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
      Icon = EFPApp.MainImageIcon("SavedCompositions");

      efpForm = new EFPFormProvider(this);

      efpPreview = new EFPPictureBox(efpForm, pbPreview);
      efpPreview.Control.SizeMode = PictureBoxSizeMode.Zoom;

      efpSelCB = new EFPTextComboBox(efpForm, cbParamSet.TheCB);
      efpSelCB.DisplayName = "Готовые наборы";

      efpSaveButton = new EFPButton(efpForm, cbParamSet.SaveButton);
      efpSaveButton.DisplayName = "Сохранить набор";
      efpSaveButton.ToolTipText = "Сохранить установленные значения как новый пользовательский набор" + Environment.NewLine +
        "Перед нажатием кнопки в поле слева должно быть введено имя набора";
      //efpSaveButton.Click += new EventHandler(efpSaveButton_Click);
      cbParamSet.SaveClick += new ParamSetComboBoxSaveEventHandler(cbParamSet_SaveClick);

      efpDelButton = new EFPButton(efpForm, cbParamSet.DeleteButton);
      efpDelButton.DisplayName = "Удалить набор";
      efpDelButton.ToolTipText = "Удалить пользовательский набор значений, имя которого задано в списке слева";
      //efpDelButton.Click += new EventHandler(efpDelButton_Click);
      cbParamSet.DeleteClick += new ParamSetComboBoxItemEventHandler(cbParamSet_DeleteClick);

      cbParamSet.ShowImages = EFPApp.ShowListImages;

      cbParamSet.ItemSelected += new ParamSetComboBoxItemEventHandler(cbParamSet_ItemSelected);
      efpSelCB.TextEx.ValueChanged += new EventHandler(efpSelCB_TextChanged);

      btnXml.Image = EFPApp.MainImages.Images["XML"];
      btnXml.ImageAlign = ContentAlignment.MiddleCenter;
      EFPButton efpXml = new EFPButton(efpForm, btnXml);
      efpXml.DisplayName = "Просмотр XML";
      efpXml.ToolTipText = "Просмотр данных выбранной композиции";
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
        EFPApp.ShowException(e, "Ошибка OnLoad");
      }

      cbParamSet.Select();
    }

    private void LoadItems()
    {
      cbParamSet.Items.Clear();

      #region Текущая композиция

      ParamSetComboBoxItem CurrItem = new ParamSetComboBoxItem("Current", "[ Текущая композиция ]",
        "ArrowRight", /*DateTime.Now*/null, 1, _CurrPart.MD5Sum());
      cbParamSet.Items.Add(CurrItem);

      #endregion

      #region Именные секции

      EFPAppCompositionHistoryItem[] Items;
      Items = EFPAppCompositionHistoryHandler.GetUserItems();
      for (int i = 0; i < Items.Length; i++)
      {
        ParamSetComboBoxItem cbItem = new ParamSetComboBoxItem(Items[i].UserSetName,
          Items[i].DisplayName,
          "User", /*Items[i].Time*/null, 2, Items[i].MD5);
        cbItem.Tag = Items[i];
        cbParamSet.Items.Add(cbItem);
      }

      #endregion

      #region История

      Items = EFPAppCompositionHistoryHandler.GetHistoryItems();
      for (int i = 0; i < Items.Length; i++)
      {
        ParamSetComboBoxItem cbItem = new ParamSetComboBoxItem(Items[i].UserSetName,
          Items[i].DisplayName,
          "Time", /*Items[i].Time*/null, 2, Items[i].MD5);
        cbItem.Tag = Items[i];
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

    private EFPPictureBox efpPreview;

    void SetImage(Image image)
    {
      if (image == null)
      {
        SetWarning("Нет изображения");
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
        SetError("Не удалось вывести изображение. " + e.Message);
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

    private EFPTextComboBox efpSelCB;

    private EFPButton efpSaveButton;

    private EFPButton efpDelButton;

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
      else if (String.IsNullOrEmpty(efpSelCB.Text) || efpSelCB.Text.StartsWith("["))
        efpSaveButton.Enabled = false;
      else
        efpSaveButton.Enabled = true;
      efpDelButton.Enabled = SelectedItem != null;
    }

    void cbParamSet_SaveClick(object sender, ParamSetComboBoxSaveEventArgs args)
    {
      string Name = efpSelCB.Text;
      if (Name.StartsWith("["))
        Name = String.Empty;

      string UserSetName;
      if (SelectedItem == null)
      {
        if (Name.Length == 0)
        {
          if (EFPApp.MessageBox("Сохранить текущую композицию в списке истории, а не как \"именную\" композицию?",
            "Сохранение текущей композиции", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
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
          EFPApp.ShowTempMessage("Должно быть задано название сохраняемой композиции");
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
        EFPApp.ShowTempMessage("Нельзя удалить текущую композицию");
        return;
      }
      if (EFPApp.MessageBox("Удалить композицию \"" + SelectedItem.DisplayName + "\"?",
        "Подтверждение удаления", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
        return;

      EFPAppCompositionHistoryHandler.Delete(SelectedItem);
      LoadItems();
    }

    #endregion

    #region Кнопка XML

    void efpXml_Click(object sender, EventArgs args)
    {
      if (SelectedItem == null)
        EFPApp.ShowTextView(DataTools.XmlDocumentToString(_CurrPart.Document), "Текущая композиция");
      else
      {
        CfgPart cfg;
        using (EFPApp.ConfigManager.GetConfig(EFPAppCompositionHistoryHandler.GetConfigInfo(SelectedItem),
          EFPConfigMode.Read, out cfg))
        {
          TempCfg Cfg2 = new TempCfg();
          cfg.CopyTo(Cfg2);
          EFPApp.ShowTextView(DataTools.XmlDocumentToString(Cfg2.Document), SelectedItem.DisplayName);
        }
      }
    }

    #endregion
  }

  /// <summary>
  /// Выводит диалог загрузки сохраненной композиции рабочего стола.
  /// Если пользователь нажал [OK], автоматически вызывается EFPApp.LoadComposition().
  /// Свойство EFPApp.CompositionHistoryCount должно быть установлено в значение, отличное от 0
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
    /// <returns>true, если пользователь выполнил загрузку композиции рабочего стола</returns>
    public DialogResult ShowDialog()
    {
      SelectCompositionForm Form = new SelectCompositionForm();
      Form.efpForm.HelpContext = HelpContext;
      if (EFPApp.ShowDialog(Form, true) == DialogResult.OK)
      {
        if (Form.SelectedItem != null)
        {
          CfgPart cfg;
          using (EFPApp.ConfigManager.GetConfig(EFPAppCompositionHistoryHandler.GetConfigInfo(Form.SelectedItem), EFPConfigMode.Read, out cfg))
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