// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using FreeLibSet.Config;
using FreeLibSet.Core;
using FreeLibSet.Collections;

namespace FreeLibSet.FIAS
{
  /// <summary>
  /// Однократные настройки для исключения ненужных данных классификатора.
  /// Свойства объекта можно устанавливать до присоединения к FiasDB. После этого объект становится потокобезопасным
  /// </summary>
  [Serializable]
  public sealed class FiasDBSettings : IReadOnlyObject
  {
    #region Конструктор

    /// <summary>
    /// Создает объект, в котором можно задать настройки
    /// </summary>
    public FiasDBSettings()
    {
      _UseHouse = true;
      _UseRoom = true;
      _RegionCodes = new RegionCodeList();
      _UseOKATO = true;
      _UseOKTMO = true;
      _UseIFNS = true;
      _UseHistory = false;
    }

    //private FiasDBSettings(bool dummy)
    //  : this()
    //{
    //  SetReadOnly();
    //}

    #endregion

    #region Свойства

    /// <summary>
    /// Будут ли включены в классификатор номера домов?
    /// По умолчанию - true.
    /// </summary>
    public bool UseHouse
    {
      get { return _UseHouse; }
      set
      {
        CheckNotReadOnly();
        _UseHouse = value;
      }
    }
    private bool _UseHouse;

    /// <summary>
    /// Будут ли включены в классификатор номера помещений (квартир)?
    /// По умолчанию - true.
    /// </summary>
    public bool UseRoom
    {
      get { return _UseRoom; }
      set
      {
        CheckNotReadOnly();
        _UseRoom = value;
      }
    }
    bool _UseRoom;

    [Serializable]
    private class RegionCodeList : SingleScopeList<string>
    {
      public new void SetReadOnly()
      {
        base.SetReadOnly();
      }
    }

    /// <summary>
    /// Список включаемых кодов региона.
    /// По умолчанию список пустой, что означает включение в классификатор всех регионов.
    /// </summary>
    public ICollection<string> RegionCodes { get { return _RegionCodes; } }
    private RegionCodeList _RegionCodes;

    /// <summary>
    /// Будут ли включены в классификатор поля с кодами ОКАТО?
    /// По умолчанию - true.
    /// </summary>
    public bool UseOKATO
    {
      get { return _UseOKATO; }
      set
      {
        CheckNotReadOnly();
        _UseOKATO = value;
      }
    }
    private bool _UseOKATO;

    /// <summary>
    /// Будут ли включены в классификатор поля с кодами ОКТМО?
    /// По умолчанию - true.
    /// </summary>
    public bool UseOKTMO
    {
      get { return _UseOKTMO; }
      set
      {
        CheckNotReadOnly();
        _UseOKTMO = value;
      }
    }
    private bool _UseOKTMO;

    /// <summary>
    /// Будут ли включены в классификатор поля с кодами ИФНС?
    /// По умолчанию - true.
    /// </summary>
    public bool UseIFNS
    {
      get { return _UseIFNS; }
      set
      {
        CheckNotReadOnly();
        _UseIFNS = value;
      }
    }
    private bool _UseIFNS;

    /// <summary>
    /// Будут ли включены в классификатор исторические сведения (true) или только актуальные (false).
    /// По умолчанию - false - только актуальные сведения
    /// </summary>
    public bool UseHistory
    {
      get { return _UseHistory; }
      set
      {
        CheckNotReadOnly();
        _UseHistory = value;
        if (value)
          _UseDates = true;
      }
    }
    private bool _UseHistory;

    /// <summary>
    /// Наличие полей даты начала и окончания действия записей.
    /// По умолчанию - false.
    /// Установка свойства UseHistory=true вызывает принудительную установку и этого свойства
    /// </summary>
    public bool UseDates
    {
      get { return _UseDates; }
      set
      {
        CheckNotReadOnly();
        _UseDates = value;
      }
    }
    private bool _UseDates;

    #endregion

    #region Методы проверки

    internal void CheckUseHouse()
    {
      if (!_UseHouse)
        throw new FiasDBSettingsException("В настройках базы данных ФИАС отключено использование справочника зданий (FiasDBSettings.UseHouse=false)");
    }

    internal void CheckUseRoom()
    {
      if (!_UseRoom)
        throw new FiasDBSettingsException("В настройках базы данных ФИАС отключено использование справочника помещений (FiasDBSettings.UseRoom=false)");
    }

    internal void CheckUseHistory()
    {
      if (!_UseHistory)
        throw new FiasDBSettingsException("В настройках базы данных ФИАС отключено использование исторических данных (FiasDBSettings.UseHistory=false)");
    }

    internal void CheckUseIFNS()
    {
      if (!_UseIFNS)
        throw new FiasDBSettingsException("В настройках базы данных ФИАС отключено использование полей ИФНС (FiasDBSettings.UseIFNS=false)");
    }

    internal void CheckUseOKATO()
    {
      if (!_UseOKATO)
        throw new FiasDBSettingsException("В настройках базы данных ФИАС отключено использование полей кодов ОКАТО (FiasDBSettings.UseOKATO=false)");
    }

    internal void CheckUseOKTMO()
    {
      if (!_UseOKTMO)
        throw new FiasDBSettingsException("В настройках базы данных ФИАС отключено использование полей ОКТМО (FiasDBSettings.UseOKTMO=false)");
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Возвращает true после вызова конструктора FiasDB.
    /// </summary>
    public bool IsReadOnly
    {
      get { return _RegionCodes.IsReadOnly; }
    }

    /// <summary>
    /// Генерирует исключение, если IsReadOnly=true
    /// </summary>
    public void CheckNotReadOnly()
    {
      _RegionCodes.CheckNotReadOnly();
    }

    /// <summary>
    /// Переводит набор настроек в режим "только чтение".
    /// Вызывается конструктором FiasDB.
    /// </summary>
    public void SetReadOnly()
    {
      if (IsReadOnly)
        return;

      if (_UseRoom && (!_UseHouse))
        throw new InvalidOperationException("Неправильные настройки. Не может быть UseFlats=true при UseHouses=false");

      if (_UseHistory && (!_UseDates))
        throw new InvalidOperationException("Неправильные настройки. Не может быть UseHistory=true при UseDates=false");

      _RegionCodes.SetReadOnly();
    }

    #endregion

    #region Преобразование в XML и секцию конфигурации

    /// <summary>
    /// Записывает настройки в секцию конфигурации
    /// </summary>
    /// <param name="cfg">Заполняемая секция</param>
    public void WriteConfig(CfgPart cfg)
    {
      cfg.SetBool("UseHouse", UseHouse);
      cfg.SetBool("UseRoom", UseRoom);
      if (RegionCodes.Count == 0)
        cfg.SetString("RegionCodes", String.Empty);
      else
      {
        string[] aRegionCodes = new string[RegionCodes.Count];
        RegionCodes.CopyTo(aRegionCodes, 0);
        cfg.SetString("RegionCodes", String.Join(",", aRegionCodes));
      }
      cfg.SetBool("UseOKATO", UseOKATO);
      cfg.SetBool("UseOKTMO", UseOKTMO);
      cfg.SetBool("UseIFNS", UseIFNS);
      cfg.SetBool("UseHistory", UseHistory);
      cfg.SetBool("UseDates", UseDates);
    }

    /// <summary>
    /// Чтение настроек из секции конфигурации.
    /// Настройки, которых нет в секции, сохраняются неизмененными.
    /// </summary>
    /// <param name="cfg">Считываемая секция конфигурации</param>
    public void ReadConfig(CfgPart cfg)
    {
      CheckNotReadOnly();

      UseHouse = cfg.GetBoolDef("UseHouse", UseHouse);
      if (UseHouse)
        UseRoom = cfg.GetBoolDef("UseRoom", UseRoom);
      else
        UseRoom = false;

      RegionCodes.Clear();
      string sRegionCodes = cfg.GetString("RegionCodes");
      if (!String.IsNullOrEmpty(sRegionCodes))
      {
        string[] aRegionCodes = sRegionCodes.Split(',');
        for (int i = 0; i < aRegionCodes.Length; i++)
          RegionCodes.Add(aRegionCodes[i]);
      }

      UseOKATO = cfg.GetBoolDef("UseOKATO", UseOKATO);
      UseOKTMO = cfg.GetBoolDef("UseOKTMO", UseOKTMO);
      UseIFNS = cfg.GetBoolDef("UseIFNS", UseIFNS);
      UseHistory = cfg.GetBoolDef("UseHistory", UseHistory);
      if (!UseHistory)
        UseDates = cfg.GetBoolDef("UseDates", UseDates);

    }

    /// <summary>
    /// Настройки в виде текста XML
    /// </summary>
    public string AsXmlText
    {
      get
      {
        TempCfg cfg = new TempCfg();
        WriteConfig(cfg);
        return cfg.AsXmlText;
      }
      set
      {
        TempCfg cfg = new TempCfg();
        cfg.AsXmlText = value;
        ReadConfig(cfg);
      }
    }

    #endregion

    #region Текстовое представление

    /// <summary>
    /// Текстовое представление (для отладки)
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();
      sb.Append("Дома: ");
      sb.Append(_UseHouse ? "есть" : "нет");
      if (_UseHouse)
      {
        sb.Append(", квартиры: ");
        sb.Append(_UseRoom ? "есть" : "нет");
      }
      sb.Append(", регионы: ");
      if (_RegionCodes.Count == 0)
        sb.Append("все");
      else
      {
        for (int i = 0; i < _RegionCodes.Count; i++)
        {
          if (i > 0)
            sb.Append(',');
          sb.Append(_RegionCodes[i]);
        }
      }

      sb.Append(", коды ОКАТО: ");
      sb.Append(UseOKATO ? "есть" : "нет");

      sb.Append(", коды ОКТМО: ");
      sb.Append(UseOKTMO ? "есть" : "нет");

      sb.Append(", коды ИФНС: ");
      sb.Append(UseIFNS ? "есть" : "нет");

      sb.Append(", исторические сведения: ");
      sb.Append(UseHistory ? "есть" : "нет");

      if (!UseHistory)
      {
        sb.Append(", даты действия записей: ");
        sb.Append(UseDates ? "есть" : "нет");
      }

      return sb.ToString();
    }

    #endregion

    #region Статический экземпляр

    /// <summary>
    /// Настройки по умолчанию.
    /// У этого объекта нельзя менять свойства
    /// </summary>
    public static readonly FiasDBSettings DefaultSettings = new FiasDBSettings();

    #endregion
  }

  [Serializable]
  internal enum FiasFTSMode { None, FTS3 }

  /// <summary>
  /// Внутренние параметры классификатора.
  /// Не используется в пользовательском коде
  /// </summary>
  [Serializable]
  public sealed class FiasInternalSettings
  {
    #region Свойства

    /// <summary>
    /// Имя провайдера базы данных - константа из класса DBxProviderNames
    /// </summary>
    public string ProviderName
    {
      get { return _ProviderName; }
      internal set { _ProviderName = value; }
    }
    private string _ProviderName;

    /// <summary>
    /// Использование отдельных таблиц идентификаторов для уменьшения размера
    /// </summary>
    internal bool UseIdTables;

    /// <summary>
    /// Возвращает true для базы данных SQLite.
    /// При этом вместо полей "STARTDATE" и "ENDDATE" типа DATE используются поля "dStartDate" и "dEndDate" типа INT.
    /// Для преобразования даты в число используется функция DateTime.ToOADate(), дате 01.01.1900 соответствует значение 1.
    /// </summary>
    public bool UseOADates
    {
      get { return _UseOADates; }
      internal set { _UseOADates = value; }
    }
    private bool _UseOADates;

    /// <summary>
    /// Использование полнотекстного поиска
    /// </summary>
    internal FiasFTSMode FTSMode;

    #endregion
  }
}
