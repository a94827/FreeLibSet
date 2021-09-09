using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using AgeyevAV.ExtForms;
using AgeyevAV.FIAS;
using AgeyevAV.IO;
using AgeyevAV.DBF;
using AgeyevAV.ExtForms.FIAS;
using AgeyevAV;
using AgeyevAV.Config;
using AgeyevAV.ExtDB;
using System.Data.Common;

namespace FIASDemo
{
  public partial class MainForm : Form
  {
    #region Конструктор формы

    public MainForm(FiasDB fiasDB, CfgPart cfg)
    {
      InitializeComponent();
      EFPFormProvider efpForm = new EFPFormProvider(this);

      this.fiasDB = fiasDB;
      this.cfg = cfg;
      this.UI = new FiasUI(fiasDB.Source);

      #region Вкладка "Адрес"

      #region Редактор адреса

      cbLevel.Items.AddRange(FiasEnumNames.FiasEditorLevelNames);
      string[] AvailableLevelCodes = new string[FiasEnumNames.FiasEditorLevelNames.Length];
      for (int i = 0; i < AvailableLevelCodes.Length; i++)
        AvailableLevelCodes[i] = ((FiasEditorLevel)i).ToString();
      efpLevel = new EFPListComboBox(efpForm, cbLevel);
      efpLevel.Codes = AvailableLevelCodes;
      efpLevel.SelectedCode = cfg.GetEnumDef<FiasLevel>("AddressLevel", FiasLevel.Flat).ToString();

      efpManualPostalCode = new EFPCheckBox(efpForm, cbManualPostalCode);
      efpManualPostalCode.Checked = cfg.GetBool("ManualPostalCode");

      efpReadOnly = new EFPCheckBox(efpForm, cbReadOnly);
      efpReadOnly.Checked = cfg.GetBool("AddressReadOnly");

      efpEmptyMode = new EFPListComboBox(efpForm, cbEmpyMode);
      efpEmptyMode.SelectedIndex = cfg.GetInt("EmptyMode");

      efpPartialMode = new EFPListComboBox(efpForm, cbPartialMode);
      efpPartialMode.SelectedIndex = cfg.GetInt("PartialMode");

      btnEdit.Image = EFPApp.MainImages.Images["Edit"];
      btnEdit.ImageAlign = ContentAlignment.MiddleLeft;
      EFPButton efpEdit = new EFPButton(efpForm, btnEdit);
      efpEdit.Click += new EventHandler(efpEdit_Click);

      #endregion

      #region Текущий адрес

      btnFormat.Image = EFPApp.MainImages.Images["Font"];
      btnFormat.ImageAlign = ContentAlignment.MiddleCenter;
      EFPButton efpFormat = new EFPButton(efpForm, btnFormat);
      efpFormat.DisplayName = "Тестирование Format()";
      efpFormat.ToolTipText = "Тестирование метода FiasHandler.Format()";
      efpFormat.Click += new EventHandler(efpFormat_Click);

      efpMessages = new EFPErrorDataGridView(efpForm, grMessages);

      #endregion

      #region Сохранение адреса как строки

      efpVFormat = new EFPListComboBox(efpForm, cbVFormat);
      efpVText = new EFPHistComboBox(efpForm, cbVText);
      try
      {
        FiasAddressConvert convert = new FiasAddressConvert(fiasDB.Source);
        FiasAddressConvertGuidMode[] guidModes = convert.AvailableGuidModes;
        for (int i = 0; i < guidModes.Length; i++)
          cbVFormat.Items.Add(guidModes[i].ToString());

        cbVFormat.SelectedItem = cfg.GetString("GuidMode");
        if (cbVFormat.SelectedIndex < 0)
          cbVFormat.SelectedItem = convert.GuidMode.ToString();

        efpVText.HistList = cfg.GetHist("CurrAddress");
      }
      catch
      {
      }

      #endregion

      try
      {
        FiasAddressConvert convert = new FiasAddressConvert(fiasDB.Source);
        CurrAddress = convert.Parse(efpVText.Text);
      }
      catch
      {
        CurrAddress = new FiasAddress();
      }
      efpVFormat.SelectedIndexEx.ValueChanged += new EventHandler(efpVFormat_ValueChanged);

      btnFromVText.Image = EFPApp.MainImages.Images["ArrowRightThenUp"];
      btnFromVText.ImageAlign = ContentAlignment.MiddleCenter;
      EFPButton efpFromVText = new EFPButton(efpForm, btnFromVText);
      btnFromVText.Click += new EventHandler(efpFromVText_Click);

      #region Поиск по Guid

      efpGuid = new EFPHistComboBox(efpForm, cbGuid);
      efpGuid.HistList = cfg.GetHist("AddressGuid");

      btnFromGuid.Image = EFPApp.MainImages.Images["ArrowRightThenUp"];
      btnFromGuid.ImageAlign = ContentAlignment.MiddleCenter;
      EFPButton efpFromGuid = new EFPButton(efpForm, btnFromGuid);
      efpFromGuid.Click += new EventHandler(efpFromGuid_Click);

      #endregion

      btnAbout.Image = EFPApp.MainImages.Images["About"];
      btnAbout.ImageAlign = ContentAlignment.MiddleLeft;
      EFPButton efpAbout = new EFPButton(efpForm, btnAbout);
      efpAbout.Click += new EventHandler(efpAbout_Click);

      #endregion

      #region Вкладка "Классификатор"

      efpActualDate = new EFPDateBox(efpForm, edActualDate);
      efpActualDate.ReadOnly = true;

      efpAddrObCount = new EFPLabel(efpForm, lblAddrObCount);
      efpAddrObCount.Label = lblAddrObjCount2;
      efpHouseCount = new EFPLabel(efpForm, lblHouseCount);
      efpHouseCount.Label = lblHouseCount2;
      efpRoomCount = new EFPLabel(efpForm, lblRoomCount);
      efpRoomCount.Label = lblRoomCount2;

      efpLoadWeb = new EFPButton(efpForm, btnLoadWeb);
      efpLoadWeb.Click += new EventHandler(efpLoadWeb_Click);

      EFPButton efpLoadFiles = new EFPButton(efpForm, btnLoadFiles);
      efpLoadFiles.Click += new EventHandler(efpLoadFiles_Click);

      EFPButton efpUpdateHistory = new EFPButton(efpForm, btnUpdateHistory);
      efpUpdateHistory.Click += new EventHandler(efpUpdateHistory_Click);

      EFPButton efpToDisk = new EFPButton(efpForm, btnToDisk);
      efpToDisk.Click += new EventHandler(efpToDisk_Click);

      EFPButton efpClearCache = new EFPButton(efpForm, btnClearCache);
      efpClearCache.Click += new EventHandler(efpClearCache_Click);

      EFPButton efpConvertGuid = new EFPButton(efpForm, btnConvertGuid);
      efpConvertGuid.Enabled = (fiasDB.Source.InternalSettings.ProviderName == DBxProviderNames.SQLite);
      efpConvertGuid.Click += new EventHandler(efpConvertGuid_Click);

      InitDBStat();


      #endregion

      #region Вкладка "Поиск"

      ParseSettings = new OldFiasParseSettings(fiasDB.Source);
      try { ParseSettings.ReadConfig(cfg.GetChild("ParseSettings", true)); }
      catch { }
      UpdateParseSettings();

      btnBaseAddress.Image = EFPApp.MainImages.Images["Settings"];
      btnBaseAddress.ImageAlign = ContentAlignment.MiddleLeft;
      EFPButton efpBaseAddressButton = new EFPButton(efpForm, btnBaseAddress);
      efpBaseAddressButton.Click += new EventHandler(efpBaseAddressButton_Click);

      efpSearchLevel = new EFPListComboBox(efpForm, cbSearchLevel);
      efpSearchLevel.ItemStrings = efpLevel.ItemStrings; // такие же уровни, как в редакторе
      efpSearchLevel.Codes = AvailableLevelCodes;
      efpSearchLevel.SelectedCode = cfg.GetEnumDef<FiasLevel>("AddressLevel", FiasLevel.Flat).ToString();

      try
      { efpSearchLevel.SelectedCode = ParseSettings.EditorLevel.ToString(); }
      catch { }
      efpSearchLevel.SelectedIndexEx.ValueChanged += new EventHandler(efpSearchLevel_ValueChanged);


      efpSearchText = new EFPTextBox(efpForm, edSearchText);

      btnSearch.Image = EFPApp.MainImages.Images["Find"];
      btnSearch.ImageAlign = ContentAlignment.MiddleLeft;
      EFPButton efpSearch = new EFPButton(efpForm, btnSearch);
      efpSearch.Click += new EventHandler(efpSearch_Click);

      #endregion
    }

    #endregion

    /// <summary>
    /// Объект для сохранения настроек
    /// </summary>
    private CfgPart cfg;

    #region Адрес

    void efpVFormat_ValueChanged(object sender, EventArgs args)
    {
      CurrAddress = CurrAddress;
      if (cbVFormat.SelectedIndex >= 0)
        cfg.SetString("GuidMode", cbVFormat.SelectedItem.ToString());
    }

    /// <summary>
    /// Текущий адрес
    /// </summary>
    public FiasAddress CurrAddress
    {
      get { return _CurrAddress; }
      set
      {
        if (value == null)
          value = new FiasAddress();
        _CurrAddress = value;
        if (_CurrAddress.IsEmpty)
          edAddress.Text = "[ пусто ]";
        else
        {
          FiasHandler handler = new FiasHandler(fiasDB.Source);
          edAddress.Text = handler.GetText(_CurrAddress);
          efpMessages.ErrorMessages = _CurrAddress.Messages;
        }

        string s;
        try
        {
          FiasAddressConvert convert = new FiasAddressConvert(fiasDB.Source);
          if (efpVFormat.SelectedIndex >= 0)
            convert.GuidMode = (FiasAddressConvertGuidMode)(Enum.Parse(typeof(FiasAddressConvertGuidMode), cbVFormat.SelectedItem.ToString()));
          s = convert.ToString(_CurrAddress);
          efpVText.HistList = efpVText.HistList.Add(s);
          cfg.SetHist("CurrAddress", efpVText.HistList);
        }
        catch (Exception e)
        {
          s = e.Message;
        }
      }
    }
    private FiasAddress _CurrAddress;

    private EFPErrorDataGridView efpMessages;

    EFPListComboBox efpVFormat;

    EFPHistComboBox efpVText;

    EFPHistComboBox efpGuid;

    private void efpFromGuid_Click(object sender, EventArgs args)
    {
      Guid g;
      if (String.IsNullOrEmpty(efpGuid.Text))
        g = Guid.Empty;
      else
      {
        try
        {
          g = new Guid(efpGuid.Text);
        }
        catch
        {
          EFPApp.ErrorMessageBox("Неправильный формат GUIDа");
          return;
        }

        efpGuid.HistList = efpGuid.HistList.Add(efpGuid.Text);
        cfg.SetHist("AddressGuid", efpGuid.HistList);
      }

      FiasAddress addr = new FiasAddress();
      addr.UnknownGuid = g;

      FiasHandler handler = new FiasHandler(UI.Source);
      handler.FillAddress(addr);
      CurrAddress = addr;
    }

    private void efpFromVText_Click(object sender, EventArgs args)
    {
      string s = efpVText.Text;

      try
      {
        FiasAddressConvert convert = new FiasAddressConvert(fiasDB.Source);
        //if (efpVFormat.SelectedIndex > 0)
        //  convert.GuidMode = (FiasAddressConvertGuidMode)(Enum.Parse(typeof(FiasAddressConvertGuidMode), cbVFormat.SelectedItem.ToString()));
        CurrAddress = convert.Parse(s);
      }
      catch (Exception e)
      {
        EFPApp.ErrorMessageBox(e.Message);
      }
    }

    EFPListComboBox efpLevel;

    EFPCheckBox efpManualPostalCode, efpReadOnly;

    EFPListComboBox efpEmptyMode, efpPartialMode;


    private void efpEdit_Click(object sender, EventArgs args)
    {
      if (efpLevel.SelectedCode.Length == 0)
      {
        efpLevel.SetFocus("Не выбран уровень");
        return;
      }
      FiasEditorLevel editorLevel = (FiasEditorLevel)(Enum.Parse(typeof(FiasEditorLevel), efpLevel.SelectedCode));

      cfg.SetEnum<FiasEditorLevel>("AddressLevel", editorLevel);
      cfg.SetBool("ManualPostalCode", efpManualPostalCode.Checked);
      cfg.SetBool("AddressReadOnly", efpReadOnly.Checked);
      cfg.SetInt("EmptyMode", efpEmptyMode.SelectedIndex);
      cfg.SetInt("PartialMode", efpPartialMode.SelectedIndex);

      FiasAddressDialog dlg = new FiasAddressDialog(UI);
      dlg.EditorLevel = editorLevel;
      dlg.PostalCodeEditable = efpManualPostalCode.Checked;
      switch (efpEmptyMode.SelectedIndex)
      {
        case 0: dlg.CanBeEmpty = true; break;
        case 2: dlg.CanBeEmpty = true; dlg.WarningIfEmpty = true; break;
      }
      switch (efpPartialMode.SelectedIndex)
      {
        case 0: dlg.CanBePartial = true; break;
        case 2: dlg.CanBePartial = true; dlg.WarningIfPartial = true; break;
      }
      dlg.ReadOnly = efpReadOnly.Checked;
      dlg.Address = CurrAddress;
      if (dlg.ShowDialog() == DialogResult.OK)
        CurrAddress = dlg.Address;
    }

    private void efpAbout_Click(object sender, EventArgs args)
    {
      EFPApp.ShowAboutDialog();
    }

    void efpFormat_Click(object sender, EventArgs args)
    {
      TestFormatForm.PerformTest(new FiasHandler(fiasDB.Source), _CurrAddress);
    }

    #endregion

    #region Классификатор

    #region Текущая база данных

    /// <summary>
    /// Объект классификатора
    /// </summary>
    private FiasDB fiasDB;

    /// <summary>
    /// Пользовательский интерфейс ФИАС
    /// </summary>
    private FiasUI UI;

    EFPDateBox efpActualDate;

    EFPLabel efpAddrObCount, efpHouseCount, efpRoomCount;

    private void InitDBStat()
    {
      efpLoadWeb.Enabled = !fiasDB.IsEmpty; // 26.02.2021 Пока нельзя загрузить полную версию с сайта

      efpActualDate.Value = fiasDB.ActualDate;

      efpAddrObCount.Text = fiasDB.DBStat.AddrObCount.ToString("#,##0");

      efpHouseCount.Visible = fiasDB.DBSettings.UseHouse;
      efpHouseCount.Text = fiasDB.DBStat.HouseCount.ToString("#,##0");

      efpRoomCount.Visible = fiasDB.DBSettings.UseRoom;
      efpRoomCount.Text = fiasDB.DBStat.RoomCount.ToString("#,##0");
    }

    #endregion

    #region Обновление

    EFPButton efpLoadWeb;

    void efpLoadWeb_Click(object sender, EventArgs args)
    {
      try
      {
        using (Splash spl = new Splash("Обновление ФИАС"))
        {
          FiasDBWebUpdater updater = new FiasDBWebUpdater(fiasDB);
          updater.Splash = spl;
          updater.PerformUpdate();
        }
      }
      finally
      {
        InitDBStat();
      }
    }


    void efpLoadFiles_Click(object sender, EventArgs args)
    {
      FolderBrowserDialog dlg = new FolderBrowserDialog();
      if (EFPApp.ShowDialog(dlg) != DialogResult.OK)
        return;

      DateInputDialog dlg2 = new DateInputDialog();
      dlg2.Title = "Загрузка файлов";
      dlg2.Prompt = "Дата актуальности обновления";
      dlg2.CanBeEmpty = false;
      if (dlg2.ShowDialog() != DialogResult.OK)
        return;

      try
      {
        int cntFiles;
        DateTime startTime = DateTime.Now;
        using (Splash spl = new Splash("Загрузка файлов ФИАС"))

        using (FiasDBUpdater up = new FiasDBUpdater(fiasDB))
        {
          up.Splash = spl;
          cntFiles = up.LoadDir(new AbsPath(dlg.SelectedPath), dlg2.Value.Value);
        }

        TimeSpan time = DateTime.Now - startTime;
        EFPApp.MessageBox("Загружено файлов: " + cntFiles.ToString() + ". Время загрузки: " + time.ToString());
      }
      finally
      {
        InitDBStat();
      }
    }


    void efpToDisk_Click(object sender, EventArgs args)
    {
      DownloadFilesForm.PerformDownload();
    }

    void efpUpdateHistory_Click(object sender, EventArgs args)
    {
      UI.ShowClassifUpdateTable();
    }


    #endregion

    #region Дополнительно

    private void efpClearCache_Click(object sender, EventArgs args)
    {
      ((FiasCachedSource)(fiasDB.Source)).ClearCache();
    }

    void efpConvertGuid_Click(object sender, EventArgs args)
    {
      EFPApp.ShowDialog(new ConvertGuidForm(), true);
    }

    #endregion

    private void btnStat_Click(object sender, EventArgs args)
    {

      FolderBrowserDialog dlg = new FolderBrowserDialog();
      if (EFPApp.ShowDialog(dlg) != DialogResult.OK)
        return;

      int MaxOffNameLen = 0;
      string MaxOffNameStr = String.Empty;
      int MaxHouseLen = 0;
      string MaxHouseStr = String.Empty;

      using (Splash spl = new Splash("Просмотр файлов"))
      {
        string[] aFiles = System.IO.Directory.GetFiles(dlg.SelectedPath, "*.dbf", System.IO.SearchOption.TopDirectoryOnly);
        spl.PercentMax = aFiles.Length;
        spl.AllowCancel = true;
        for (int i = 0; i < aFiles.Length; i++)
        {
          AbsPath fp = new AbsPath(aFiles[i]);
          spl.PhaseText = fp.FileName;
          if (fp.FileNameWithoutExtension.StartsWith("addrob", StringComparison.OrdinalIgnoreCase))
          {
            using (DbfFile dbf = new DbfFile(aFiles[i]))
            {
              while (dbf.Read())
              {
                string OffName = dbf.GetString("offname");
                if (OffName.Length > MaxOffNameLen)
                {
                  MaxOffNameLen = OffName.Length;
                  MaxOffNameStr = OffName;
                }
              }
            }

            /*
          if (fp.FileNameWithoutExtension.StartsWith("house", StringComparison.OrdinalIgnoreCase))
          {
            using (DbfFile dbf = new DbfFile(aFiles[i]))
            {
              while (dbf.Read())
              {
                string h = dbf.GetString("housenum");
                if (h.Length > MaxHouseLen)
                {
                  MaxHouseLen = h.Length;
                  MaxHouseStr = h;
                }

                h = dbf.GetString("buildnum");
                if (h.Length > MaxHouseLen)
                {
                  MaxHouseLen = h.Length;
                  MaxHouseStr = h;
                }
              }
            }
          }   */
          }

          spl.IncPercent();
        }
      }

      EFPApp.MessageBox("Макс. длина адресного объекта: " + MaxOffNameLen.ToString() + " (" + MaxOffNameStr + ")" +
        Environment.NewLine +
        "Макс. длина номера дома: " + MaxHouseLen.ToString() + " (" + MaxHouseStr + ")");
    }

    private void btnGetAllDownloadFileInfo_Click(object sender, EventArgs args)
    {   /*
      try
      {
        DataTable table = FiasDBWebUpdater.GetAllDownloadFileInfo();
        DebugTools.DebugDataTable(table, "Список доступных обновлений");
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка получения списка обновлений");
      }   */
    }

    #endregion


#if XXX
    private void btnTest_Click(object sender, EventArgs args)
    {
      FiasHandler handler = new FiasHandler(fiasDB.Source);
      ErrorMessageList errors = new ErrorMessageList();

      using (Splash spl = new Splash("Создание DBF-файла"))
      {
        DbfStruct dbs = new DbfStruct();
        dbs.AddString("TEXT", 255);
        dbs.AddNum("AOLEVEL", 2);
        dbs.AddString("AOGUID", 38);
        dbs.AddString("AOID", 38);
        dbs.AddString("HOUSEGUID", 38);
        dbs.AddString("HOUSEID", 38);
        dbs.AddString("POSTALCODE", 6);
        dbs.AddString("DISTRICT", 30);
        dbs.AddString("CITY", 20);
        dbs.AddString("VILLAGE", 30);
        dbs.AddString("VILLAGEA", 20);
        dbs.AddString("STREET", 30);
        dbs.AddString("STREETA", 20);
        dbs.AddString("HOUSE", 30);
        dbs.AddString("HOUSEA", 20);
        dbs.AddString("BUILDING", 30);
        dbs.AddString("STRUCT", 30);
        dbs.AddString("STRUCTA", 20);
        //dbs.AddString("ERRORS", 255);
        using (DbfFile dbf = new DbfFile(@"c:\temp\addr72.dbf", dbs, Encoding.GetEncoding(1251), DbfFileFormat.dBase3))
        {
          FiasEnumerable en = new FiasEnumerable(fiasDB.Source);
          en.BaseAddress.AOGuid = new Guid("54049357-326d-4b8f-b224-3c6dc25d6dd3"); // Тюменская область
          //en.BaseAddress.AOGuid = new Guid("2771f01d-b335-40ba-ac30-11a7d3b43c26");

          en.BottomLevel = FiasLevel.House;
          spl.AllowCancel = true;
          foreach (FiasAddress a in en)
          {
            if ((dbf.RecordCount % 100) == 0)
              spl.PhaseText = "Записей: " + dbf.RecordCount.ToString() + ". " + a.ToString();
            dbf.AppendRecord();
            string s = handler.GetTextWithoutPostalCode(a);
            s = RemoveIfStartsWith(s, "Тюменская Область, ");
            dbf.SetString("TEXT", s);
            dbf.SetInt("AOLEVEL", (int)(a.GuidBottomLevel));
            dbf.SetString("AOGUID", a.AOGuid.ToString());
            dbf.SetString("AOID", a.AORecId.ToString());
            if (a.GuidBottomLevel == FiasLevel.House)
            {
              dbf.SetString("HOUSEGUID", a.GetGuid(FiasLevel.House).ToString());
              dbf.SetString("HOUSEID", a.GetRecId(FiasLevel.House).ToString());
            }

            dbf.SetString("POSTALCODE", a.PostalCode);
            dbf.SetString("DISTRICT", a.GetName(FiasLevel.District));
            dbf.SetString("CITY", a.GetName(FiasLevel.City));
            dbf.SetString("VILLAGE", a.GetName(FiasLevel.Village));
            dbf.SetString("VILLAGEA", a.GetAOType(FiasLevel.Village));
            dbf.SetString("STREET", a.GetName(FiasLevel.Street));
            dbf.SetString("STREETA", a.GetAOType(FiasLevel.Street));
            dbf.SetString("HOUSE", a.GetName(FiasLevel.House));
            dbf.SetString("HOUSEA", a.GetAOType(FiasLevel.House));
            dbf.SetString("BUILDING", a.GetName(FiasLevel.Building));
            dbf.SetString("STRUCT", a.GetName(FiasLevel.Structure));
            dbf.SetString("STRUCTA", a.GetAOType(FiasLevel.Structure));
            if (a.Messages.Count > 0)
              //dbf.SetString("ERRORS", String.Join("|", a.Errors.AllLines));
              errors.Add(a.Messages, "RECNO=" + dbf.RecNo.ToString() + ". ", String.Empty);
            spl.CheckCancelled();
          }
        }
      }

      EFPApp.ShowErrorMessageListDialog(errors, "Результаты загрузки");
    }

    public static string RemoveIfStartsWith(string s, string value)
    {
      if (s.StartsWith(value))
        return s.Substring(value.Length);
      else
        return s;
    }
#endif


#if XXX
    private void btnTest_Click(object sender, EventArgs args)
    {
      try
      {
        string srch = "лЕнИн";
        DataTable table;
        using (DBxConBase con = fiasDB.DB.MainEntry.CreateCon())
        {
          DBxSqlBuffer buffer = new DBxSqlBuffer(fiasDB.DB.Formatter);
          buffer.SB.Append("SELECT AddrOb.OFFNAME, AddrOb.AOGUID FROM AddrObFTS JOIN AddrOb ON AddrObFTS.rowid=AddrOb.NameId WHERE AddrObFTS.OFFNAME MATCH ");
          buffer.FormatValue(FiasTools.PrepareForFTS(srch) + "*", DBxColumnType.String);
          buffer.SB.Append(" AND AddrOb.Actual=1 ORDER BY AddrOb.OFFNAME");

          table = con.SQLExecuteDataTable(buffer.SB.ToString());
        }

        DebugTools.DebugDataTable(table, "Поиск \"" + srch + "\"");
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка поиска");
      }
    }
#endif

    #region Поиск

    /// <summary>
    /// Базовый адрес для поиска
    /// </summary>
    private OldFiasParseSettings ParseSettings;

    private void UpdateParseSettings()
    {
      try
      {
        edParseSettingsText.Text = ParseSettings.ToString();
      }
      catch (Exception e)
      {
        edParseSettingsText.Text = "Ошибка. " + e.Message;
      }
    }

    EFPListComboBox efpSearchLevel;

    EFPTextBox efpSearchText;

    void efpBaseAddressButton_Click(object sender, EventArgs args)
    {
      FiasAddressDialog dlg = new FiasAddressDialog(UI);
      dlg.Title = "Базовый адрес для поиска";
      dlg.EditorLevel = FiasEditorLevel.Village;
      dlg.CanBeEmpty = true;
      dlg.CanBePartial = true;
      dlg.Address = ParseSettings.BaseAddress;
      if (dlg.ShowDialog() != DialogResult.OK)
        return;
      ParseSettings.BaseAddress = dlg.Address;
      ParseSettings.WriteConfig(cfg.GetChild("ParseSettings", true));
      UpdateParseSettings();
    }


    void efpSearchLevel_ValueChanged(object sender, EventArgs args)
    {
      ParseSettings.EditorLevel = (FiasEditorLevel)Enum.Parse(typeof(FiasEditorLevel), efpSearchLevel.SelectedCode);
      UpdateParseSettings();
    }

    private class SearchResForm : SimpleGridForm
    {
      #region Конструктор

      public SearchResForm(FiasUI ui, OldFiasParseSettings parseSettings, string[] lines, FiasAddress[] addresses, FiasAddressConvertGuidMode guidMode)
      {
        if (addresses.Length != lines.Length)
          throw new BugException("Разная длина массивов");

        _UI = ui;
        _Lines = lines;
        _Addresses = addresses;
        _ParseSettings = parseSettings;
        _Convert = new FiasAddressConvert(ui.Source);
        _Convert.GuidMode = guidMode;

        Text = "Результаты поиска адресов (" + lines.Length.ToString() + ")";
        Icon = EFPApp.MainImageIcon("Find");

        gh = new EFPDataGridView(base.ControlWithToolBar);
        gh.Columns.AddText("Line", false, "Строка поиска", 30, 25);
        gh.Columns.AddText("AddressText", false, "Найденный адрес", 70, 25);
        gh.Columns.AddImage("Error");
        gh.Columns.AddText("BottomLevel", false, "Найден уровень", 8, 3);
        gh.Columns.AddText("FiasAddressConvert", false, "FiasAddressConvert", 40, 10);
        gh.Control.RowCount = lines.Length;
        gh.DisableOrdering();
        gh.GetCellAttributes += new EFPDataGridViewCellAttributesEventHandler(gh_GetCellAttributes);

        gh.ReadOnly = false;
        gh.Control.ReadOnly = true;
        gh.CanInsert = false;
        gh.CanDelete = false;
        gh.CanView = true;
        gh.Control.MultiSelect = true;
        gh.EditData += new EventHandler(gh_EditData);

        EFPCommandItem ciDetails = new EFPCommandItem("View", "Details");
        ciDetails.MenuText = "Подробности";
        ciDetails.ImageKey = "Fias.Details";
        ciDetails.ToolTipText = "Подробная информация по каждому уровню классификатора ФИАС";
        ciDetails.Click += new EventHandler(ciDetails_Click);
        ciDetails.GroupBegin = true;
        ciDetails.GroupEnd = true;
        gh.CommandItems.Add(ciDetails);

        //gh.TopLeftCellToolTipText = "Строк в просмотре: " + lines.Length.ToString();

      }

      #endregion

      #region Поля

      private FiasUI _UI;
      private OldFiasParseSettings _ParseSettings;
      private string[] _Lines;
      private FiasAddress[] _Addresses;

      EFPDataGridView gh;

      FiasAddressConvert _Convert;

      #endregion

      #region Обработчики табличного просмотра

      void gh_GetCellAttributes(object sender, EFPDataGridViewCellAttributesEventArgs args)
      {
        switch (args.ColumnName)
        {
          case "Line":
            args.Value = _Lines[args.RowIndex];
            break;
          case "AddressText":
            args.Value = _Addresses[args.RowIndex].ToString();
            break;
          case "Error":
            args.Value = EFPApp.MainImages.Images[EFPApp.GetErrorImageKey(_Addresses[args.RowIndex].Messages)];
            break;
          case "BottomLevel":
            args.Value = FiasEnumNames.ToString(_Addresses[args.RowIndex].GuidBottomLevel, false);
            break;
          case "FiasAddressConvert":
            args.Value = _Convert.ToString(_Addresses[args.RowIndex]);
            break;
        }
      }


      void gh_EditData(object sender, EventArgs args)
      {
        if (!gh.CheckSingleRow())
          return;

        FiasAddressDialog dlg = new FiasAddressDialog(_UI);
        dlg.Title = "Найденный адрес для строки " + (gh.CurrentRowIndex + 1).ToString();
        dlg.Address = _Addresses[gh.CurrentRowIndex];
        dlg.ReadOnly = gh.State == EFPDataGridViewState.View;
        dlg.EditorLevel = _ParseSettings.EditorLevel;
        if (dlg.ShowDialog() == DialogResult.OK)
          _Addresses[gh.CurrentRowIndex] = dlg.Address;
        gh.InvalidateSelectedRows();
      }


      #endregion

      #region Команды меню

      private void ciDetails_Click(object sender, EventArgs args)
      {
        if (!gh.CheckSingleRow())
          return;

        FiasAddress addr = _Addresses[gh.CurrentRowIndex];
        _UI.ShowDetails(addr);
      }

      #endregion
    }

    void efpSearch_Click(object sender, EventArgs args)
    {
      string[] lines = efpSearchText.Control.Lines;
      if (lines.Length == 0)
      {
        EFPApp.ShowTempMessage("Не задано ни одного адреса");
        return;
      }

      for (int i = 0; i < lines.Length; i++)
        lines[i] = lines[i].Replace('\t', ',');

      FiasHandler handler = new FiasHandler(fiasDB.Source);
      FiasAddress[] a;
      using (Splash spl = new Splash("Поиск адресов"))
      {
        a = handler.ParseAddresses(lines, ParseSettings);
      }


      FiasAddressConvertGuidMode guidMode = FiasAddressConvertGuidMode.AOGuid;
      if (efpVFormat.SelectedIndex >= 0)
        guidMode = (FiasAddressConvertGuidMode)(Enum.Parse(typeof(FiasAddressConvertGuidMode), cbVFormat.SelectedItem.ToString()));

      SearchResForm resfrm = new SearchResForm(UI, ParseSettings, lines, a, guidMode);
      EFPApp.ShowDialog(resfrm, true);
    }

    #endregion


    private void btnTest_Click(object sender, EventArgs args)
    {
      FiasHandler handler = new FiasHandler(UI.Source);
      FiasParseSettings ps = new FiasParseSettings(UI.Source);
      //ps.BaseAddress.AOGuid = handler.GetRegionAOGuid("72");
      ps.BaseAddress.AOGuid = new Guid("4af5a970-b80c-422e-bc42-406cbf579c85");

      handler.FillAddress(ps.BaseAddress);

      ps.CellLevels = new FiasLevelSet[6];
      ps.CellLevels[0] = FiasLevelSet.FromLevel(FiasLevel.City);
      ps.CellLevels[1] = FiasLevelSet.FromLevel(FiasLevel.Village);
      ps.CellLevels[2] = FiasLevelSet.FromLevel(FiasLevel.PlanningStructure) | FiasLevelSet.FromLevel(FiasLevel.Street);
      ps.CellLevels[3] = FiasLevelSet.FromLevel(FiasLevel.House);
      ps.CellLevels[4] = FiasLevelSet.FromLevel(FiasLevel.Building);
      ps.CellLevels[5] = FiasLevelSet.FromLevel(FiasLevel.Structure);

      // рп Голышманово, ул Карла Маркса, Дом 1

      string[] cells = new string[6];
      cells[0] = "";
      cells[1] = "рп Голышманово";
      cells[2] = "ул. Карла Маркса";
      cells[3] = "Дом 1";
      cells[4] = "";
      cells[5] = "";

      FiasAddress a = handler.ParseAddress(cells, ps);
      FiasAddressDialog dlg = new FiasAddressDialog(UI);
      dlg.ReadOnly = true;
      dlg.Address = a;
      dlg.ShowDialog();
  }
  }
}