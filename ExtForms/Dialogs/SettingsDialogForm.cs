using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Config;
using FreeLibSet.Collections;
using FreeLibSet.UICore;

#pragma warning disable 1591
#pragma warning disable 414
#pragma warning disable 0169

namespace FreeLibSet.Forms
{
  internal partial class SettingsDialogForm : Form, IEFPConfigParamSetHandler, IEFPConfigParamSetAuxTextHandler
  {
    #region Конструктор формы

    public SettingsDialogForm(SettingsDialog owner)
    {
      InitializeComponent();
      _Owner = owner;
      Text = owner.Title;
      if (owner.Image == null)
        Icon = EFPApp.MainImages.Icons[owner.ImageKey];
      else
        WinFormsTools.InitIcon(this, owner.Image);
      efpForm = new EFPFormProvider(this);
      efpForm.AddFormCheck(FormValidating);

      #region Просмотр с вкладками

      efpTabControl = new EFPTabControl(efpForm, TheTabControl);
      for (int i = 0; i < owner.Pages.Count; i++)
      {
        EFPTabPage efpTab = efpTabControl.TabPages.Add("??");
        efpTab.Control.Controls.Add(owner.Pages[i].Control);
        owner.Pages[i].BaseProvider.Parent = efpTab.BaseProvider;
        owner.Pages[i].TabPage = efpTab;
        owner.Pages[i].CallDataToControls();
      }
      efpTabControl.SelectedIndex = 0;
      efpTabControl.SelectedIndexEx.ValueChanged += SelectedIndexEx_ValueChanged;
      SelectedIndexEx_ValueChanged(null, null); // Первый вызов сразу

      #endregion

      #region Готовые наборы

      _UseHistory = ((!String.IsNullOrEmpty(owner.ConfigSectionName)) &&
         EFPConfigTools.IsPersist(owner.ConfigManager.Persistence));

      if (_UseHistory)
      {
        efpParamSet = new EFPConfigParamSetComboBox(efpForm, SetComboBox, this);
        efpParamSet.ConfigSectionName = owner.ConfigSectionName;
        efpParamSet.ParamsCategory = owner.UserCategory; // ??
        efpParamSet.HistoryCategory = owner.HistoryCategory;

        //!!!!!!!!!!!!!!!!!!
        //EFPConfigParamSetComboBox.DefaultSet DefSet = new EFPConfigParamSetComboBox.DefaultSet(cfgDefault);
        //efpParamSet.DefaultSets.Add(DefSet);

        efpParamSet.AuxTextHandler = this;
      }
      else
        grpSets.Visible = false;

      #endregion
    }

    EFPFormProvider efpForm;

    int _PrevActivePageIndex = -1;

    /// <summary>
    /// Обрабатываем событие изменения индекса, а не PageSelected, т.к. сначала нужно выгрузить данные для предыдущей активной страницы
    /// </summary>
    private void SelectedIndexEx_ValueChanged(object sender, EventArgs args)
    {
      if (_PrevActivePageIndex >= 0)
      {
        _Owner.Pages[_PrevActivePageIndex].CallDataFromControls();
      }
      _PrevActivePageIndex = efpTabControl.SelectedIndex;
      _Owner.Pages[_PrevActivePageIndex].CallPageShow();
    }

    #endregion

    #region Свойства

    SettingsDialog _Owner;

    private EFPTabControl efpTabControl;

    /// <summary>
    /// Используются ли готовые наборы
    /// </summary>
    private bool _UseHistory;

    /// <summary>
    /// Готовые наборы
    /// </summary>
    private EFPConfigParamSetComboBox efpParamSet;

    #endregion

    private void FormValidating(object sender, UIValidatingEventArgs args)
    {
      if (efpForm.ValidateReason != EFPFormValidateReason.Shown)
      {
        for (int i = 0; i < _Owner.Pages.Count; i++)
          _Owner.Pages[i].CallDataFromControls();
      }
    }

    #region IEFPConfigParamSetHandler Members

    public void ConfigToControls(CfgPart cfg)
    {
      _Owner.Data.ReadConfig(cfg, SettingsPart.User | SettingsPart.Machine);

      for (int i = 0; i < _Owner.Pages.Count; i++)
        _Owner.Pages[i].CallDataToControls();
    }

    public void ConfigFromControls(CfgPart cfg)
    {
      for (int i = 0; i < _Owner.Pages.Count; i++)
        _Owner.Pages[i].CallDataFromControls();

      _Owner.Data.WriteConfig(cfg, SettingsPart.User | SettingsPart.Machine);
    }

    #endregion

    #region IEFPConfigParamSetAuxTextHandler Members

    private TempCfg _AuxTextTempCfg;

    public void BeginGetAuxText()
    {
      //_AuxTextTempCfg = new TempCfg();
      //Filters.WriteConfig(_AuxTextTempCfg);
    }

    public string GetAuxText(CfgPart cfg)
    {
      //Filters.ClearAllFilters();
      //Filters.ReadConfig(cfg);
      //if (Filters.IsEmpty)
      //  return "Фильтры не установлены";

      StringBuilder sb = new StringBuilder();
      //for (int i = 0; i < Filters.Count; i++)
      //{
      //  if (!Filters[i].IsEmpty)
      //  {
      //    if (sb.Length > 0)
      //      sb.Append(", ");
      //    sb.Append(Filters[i].DisplayName);
      //    sb.Append('=');
      //    sb.Append(Filters[i].FilterText);
      //  }
      //}
      return sb.ToString();
    }

    public void EndGetAuxText()
    {
      //Filters.ClearAllFilters();
      //Filters.ReadConfig(_AuxTextTempCfg);
      //_AuxTextTempCfg = null;
    }

    #endregion
  }

  public class SettingsDialog
  {
    #region Конструктор

    public SettingsDialog()
    {
      _Pages = new PageCollection(this);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Заголовок формы
    /// </summary>
    public string Title
    {
      get { return _Title ?? String.Empty; }
      set { _Title = value; }
    }
    private string _Title;

    /// <summary>
    /// Изображение для значка формы, извлекаемое из коллекции EFPApp.MainImages.
    /// По умолчанию - нет значка
    /// </summary>
    public string ImageKey
    {
      get { return _ImageKey ?? String.Empty; }
      set
      {
        _Image = null;
        _ImageKey = value;
      }
    }
    private string _ImageKey;

    public Image Image
    {
      get { return _Image; }
      set
      {
        _ImageKey = null;
        _Image = value;
      }
    }
    private Image _Image;
    public IEFPConfigManager ConfigManager
    {
      get { return _ConfigManager ?? EFPApp.ConfigManager; }
      set { _ConfigManager = value; }
    }
    private IEFPConfigManager _ConfigManager;

    public string ConfigSectionName { get { return _ConfigSectionName ?? String.Empty; } set { _ConfigSectionName = value; } }
    private string _ConfigSectionName;

    public string UserCategory
    {
      get { return _UserCategory ?? EFPConfigCategories.UserParams; }
      set { _UserCategory = value; }
    }
    private string _UserCategory;

    public string MachineCategory
    {
      get { return _MachineCategory ?? UserCategory; }
      set { _MachineCategory = value; }
    }
    private string _MachineCategory;

    public string NoHistoryCategory
    {
      get { return _NoHistoryCategory ?? UserCategory; }
      set { _NoHistoryCategory = value; }
    }
    private string _NoHistoryCategory;

    public string HistoryCategory
    {
      get { return _HistoryCategory ?? EFPConfigCategories.UserHistory; }
      set { _HistoryCategory = value; }
    }
    private string _HistoryCategory;

    /// <summary>
    /// Контекст справки, вызываемой по F1
    /// </summary>
    public string HelpContext { get { return _HelpContext ?? String.Empty; } set { _HelpContext = value; } }
    private string _HelpContext;

    /// <summary>
    /// Позиция блока диалога на экране.
    /// По умолчанию блок диалога центрируется относительно EFPApp.DefaultScreen.
    /// </summary>
    public EFPDialogPosition DialogPosition
    {
      get { return _DialogPosition; }
      set
      {
        if (value == null)
          _DialogPosition = new EFPDialogPosition();
        else
          _DialogPosition = value;
      }
    }
    private EFPDialogPosition _DialogPosition;

    #endregion

    #region Набор данных

    /// <summary>
    /// Основное свойство - редактируемые данные.
    /// </summary>
    public SettingsDataList Data
    {
      get
      {
        if (_Data == null)
          _Data = new SettingsDataList();
        return _Data;
      }
      set
      {
        _Data = value;
      }
    }
    private SettingsDataList _Data;

    public void ReadValues()
    {
      if (String.IsNullOrEmpty(ConfigSectionName))
        return;
      SettingsPart usedParts = Data.UsedParts;

      CfgPart cfg;
      if ((usedParts & SettingsPart.User) != 0)
      {
        using (ConfigManager.GetConfig(new EFPConfigSectionInfo(ConfigSectionName, UserCategory), EFPConfigMode.Read, out cfg))
        {
          Data.ReadConfig(cfg, SettingsPart.User);
        }
      }
      if ((usedParts & SettingsPart.Machine) != 0)
      {
        using (ConfigManager.GetConfig(new EFPConfigSectionInfo(ConfigSectionName, MachineCategory), EFPConfigMode.Read, out cfg))
        {
          Data.ReadConfig(cfg, SettingsPart.Machine);
        }
      }
      if ((usedParts & SettingsPart.NoHistory) != 0)
      {
        using (ConfigManager.GetConfig(new EFPConfigSectionInfo(ConfigSectionName, NoHistoryCategory), EFPConfigMode.Read, out cfg))
        {
          Data.ReadConfig(cfg, SettingsPart.NoHistory);
        }
      }
    }

    public void WriteValues()
    {
      if (String.IsNullOrEmpty(ConfigSectionName))
        return;

      SettingsPart usedParts = Data.UsedParts;

      CfgPart cfg;
      if ((usedParts & SettingsPart.User) != 0)
      {
        using (ConfigManager.GetConfig(new EFPConfigSectionInfo(ConfigSectionName, UserCategory), EFPConfigMode.Write, out cfg))
        {
          Data.WriteConfig(cfg, SettingsPart.User);
        }
      }
      if ((usedParts & SettingsPart.Machine) != 0)
      {
        using (ConfigManager.GetConfig(new EFPConfigSectionInfo(ConfigSectionName, MachineCategory), EFPConfigMode.Write, out cfg))
        {
          Data.WriteConfig(cfg, SettingsPart.Machine);
        }
      }
      if ((usedParts & SettingsPart.NoHistory) != 0)
      {
        using (ConfigManager.GetConfig(new EFPConfigSectionInfo(ConfigSectionName, NoHistoryCategory), EFPConfigMode.Write, out cfg))
        {
          Data.WriteConfig(cfg, SettingsPart.NoHistory);
        }
      }
    }

    #endregion

    #region Список страниц

    public sealed class PageCollection : List<SettingsDialogPage>
    {
      internal PageCollection(SettingsDialog owner)
      {
        _Owner = owner;
      }

      private SettingsDialog _Owner;

      public SettingsDialogPage Add(Control control)
      {
        SettingsDialogPage page = new SettingsDialogPage(_Owner, control);
        base.Add(page);
        return page;
      }
    }

    public PageCollection Pages { get { return _Pages; } }
    private readonly PageCollection _Pages;

    #endregion

    #region Показ диалога

    public DialogResult ShowDialog()
    {
      if (_Pages.Count == 0)
      {
        EFPApp.ErrorMessageBox("Нет ни одной страницы", Title);
        return DialogResult.Cancel;
      }

      // Чтение данных
      ReadValues();

      DialogResult res;
      using (SettingsDialogForm form = new SettingsDialogForm(this))
      {
        res = EFPApp.ShowDialog(form, false, DialogPosition);
        if (res == DialogResult.OK)
          WriteValues();
      }
      return res;
    }

    #endregion
  }

  public sealed class SettingsDialogPage : IEFPTabPageControl
  {
    #region Конструктор

    /// <summary>
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="control">Обычно панель с управляющими элементами (<see cref="Panel"/>), но может быть и другим контейнером, например, <see cref="GroupBox"/>.
    /// Не может быть объектом <see cref="TabPage"/></param>
    public SettingsDialogPage(SettingsDialog owner, Control control)
    {
      if (owner == null)
        throw new ArgumentNullException("owner");
      if (control == null)
        throw new ArgumentNullException("control");
      if (control.IsDisposed)
        throw new ObjectDisposedException(control.ToString(), "Панель шага мастера уже разрушена");
      if (control is Form)
        throw new ArgumentException("Управляющий элемент не может быть Form. Если используется форма-шаблон, то добавьте панель с элементами и передавайте Panel в конструктор SettingsDialogPage");
      if (control is TabPage)
        throw new ArgumentException("Управляющий элемент не может быть TabPage. Если используется форма-шаблон, то добавьте на TabPage объект Panel с элементами и передавайте Panel в конструктор SettingsDialogPage");

      _Owner = owner;
      control.Dock = DockStyle.Fill;
      _Control = control;
      _BaseProvider = new EFPBaseProvider();
    }

    #endregion

    #region Свойства

    public SettingsDialog Owner { get { return _Owner; } }
    private readonly SettingsDialog _Owner;

    public EFPBaseProvider BaseProvider { get { return _BaseProvider; } }
    private readonly EFPBaseProvider _BaseProvider;

    public Control Control { get { return _Control; } }
    private readonly Control _Control;

    #endregion

    #region События

    public event EventHandler DataToControls;

    public event EventHandler DataFromControls;

    public event EventHandler PageShow;

    internal void CallDataToControls()
    {
      if (DataToControls != null)
        DataToControls(this, EventArgs.Empty);
    }
    internal void CallDataFromControls()
    {
      if (DataFromControls != null)
        DataFromControls(this, EventArgs.Empty);
    }
    internal void CallPageShow()
    {
      if (PageShow != null)
        PageShow(this, EventArgs.Empty);
    }

    #endregion

    #region IEFPTabPageControl

    public string Text
    {
      get { return _Text; }
      set
      {
        _Text = value;
        InitTabPage();
      }
    }
    private string _Text;

    public string ImageKey
    {
      get { return _ImageKey; }
      set
      {
        _ImageKey = value;
        InitTabPage();
      }
    }
    private string _ImageKey;

    public string ToolTipText
    {
      get { return _ToolTipText; }

      set
      {
        _ToolTipText = value;
        InitTabPage();
      }
    }
    private string _ToolTipText;

    internal IEFPTabPageControl TabPage
    {
      get { return _TabPage; }
      set
      {
        _TabPage = value;
        InitTabPage();
      }
    }
    private IEFPTabPageControl _TabPage;

    private void InitTabPage()
    {
      if (_TabPage != null)
      {
        _TabPage.Text = _Text;
        _TabPage.ImageKey = _ImageKey;
        _TabPage.ToolTipText = _ToolTipText;
      }
    }

    #endregion
  }
}
