// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Config;
using FreeLibSet.Core;

namespace FreeLibSet.FIAS
{
  /// <summary>
  /// Параметры парсинга адресов из текста (без колонок).
  /// </summary>                
  public class OldFiasParseSettings
  {
    #region Конструктор

    /// <summary>
    /// Создает набор параметров
    /// </summary>
    /// <param name="source">Источник данных ФИАС. Не может быть null</param>
    public OldFiasParseSettings(IFiasSource source)
    {
      if (source == null)
        throw new ArgumentNullException("source");
      _Handler = new FiasHandler(source);
      _BaseAddress = new FiasAddress();
      _EditorLevel = FiasEditorLevel.Room;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Источник данных ФИАС. Не может быть null.
    /// </summary>
    public IFiasSource Source { get { return _Handler.Source; } }
    private FiasHandler _Handler;

    /// <summary>
    /// Базовый адрес, от которого выполняется поиск.
    /// По умолчанию - пустой адрес - вся РФ
    /// </summary>
    public FiasAddress BaseAddress
    {
      get { return _BaseAddress; }
      set
      {
        if (value == null)
          _BaseAddress = new FiasAddress();
        else
          _BaseAddress = value;
      }
    }
    private FiasAddress _BaseAddress;

    /// <summary>
    /// Последний уровень, до которого выполняется парсинг.
    /// По умолчанию - до уровня квартиры/помещения включительно
    /// </summary>
    public FiasEditorLevel EditorLevel
    {
      get { return _EditorLevel; }
      set
      {
        _EditorLevel = value;
      }
    }
    private FiasEditorLevel _EditorLevel;

    /// <summary>
    /// Заменяет уровень для домов и помещений
    /// </summary>
    internal FiasLevel InternalBottomLevel
    {
      get
      {
        switch (_EditorLevel)
        {
          case FiasEditorLevel.Village:
            return FiasLevel.Village;
          case FiasEditorLevel.Street:
            return FiasLevel.Street;
          case FiasEditorLevel.House:
            return FiasLevel.Structure;

          case FiasEditorLevel.Room:
            return FiasLevel.Room;

          default:
            throw new BugException("EditorLevel=" + EditorLevel.ToString());
        }
      }
    }

    ///// <summary>
    ///// Список доступных уровней для свойства BottomLevel
    ///// </summary>
    //public FiasLevel[] AvailableBottomLevels    {     }
    //private static readonly FiasLevel[]_AvailableBottomLevels    =new FiasLevel[]{FiasLevel.Village, FiasLevel.Street, FiasLevel.Structure, FIAS}

    #endregion

    #region Текстовое представление

    /// <summary>
    /// Текстовое представление набора параметров
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      if (_BaseAddress.IsEmpty)
        return FiasTools.RF;
      else
        return _Handler.GetTextWithoutPostalCode(_BaseAddress);
    }

    #endregion

    #region Чтение и запись настроек

    /// <summary>
    /// Запись набора параметров в секцию конфигурации
    /// </summary>
    /// <param name="cfg">Секция для записи</param>
    public void WriteConfig(CfgPart cfg)
    {
      FiasAddressConvert convert = new FiasAddressConvert(_Handler.Source);
      convert.GuidMode = FiasAddressConvertGuidMode.AOGuid;
      cfg.SetString("BaseAddress", convert.ToString(_BaseAddress));
    }

    /// <summary>
    /// Чтение набора параметров из секции конфигурации
    /// </summary>
    /// <param name="cfg"></param>
    public void ReadConfig(CfgPart cfg)
    {
      FiasAddressConvert convert = new FiasAddressConvert(_Handler.Source);
      _BaseAddress = convert.Parse(cfg.GetString("BaseAddress"));
    }

    #endregion
  }

  /// <summary>
  /// Параметры парсинга адресов из текста, разбитого на колонки.
  /// </summary>                
  public class FiasParseSettings
  {
    #region Конструктор

    /// <summary>
    /// Создает набор параметров
    /// </summary>
    /// <param name="source">Источник данных ФИАС. Не может быть null</param>
    public FiasParseSettings(IFiasSource source)
    {
      if (source == null)
        throw new ArgumentNullException("source");
      _Handler = new FiasHandler(source);
      _BaseAddress = new FiasAddress();
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Источник данных ФИАС. Не может быть null.
    /// </summary>
    public IFiasSource Source { get { return _Handler.Source; } }
    private FiasHandler _Handler;

    /// <summary>
    /// Базовый адрес, от которого выполняется поиск.
    /// По умолчанию - пустой адрес - вся РФ
    /// </summary>
    public FiasAddress BaseAddress
    {
      get { return _BaseAddress; }
      set
      {
        if (value == null)
          _BaseAddress = new FiasAddress();
        else
          _BaseAddress = value;
      }
    }
    private FiasAddress _BaseAddress;

    /// <summary>
    /// Уровни для колонок
    /// </summary>
    public FiasLevelSet[] CellLevels
    {
      get { return _CellLevels; }
      set { _CellLevels = value; }
    }
    private FiasLevelSet[] _CellLevels;

    #endregion

    #region Дополнительные методы

    /// <summary>
    /// Устанавливает массив CellLevels равным одному элементу, исходя из уже установленного свойства BaseAddress
    /// </summary>
    /// <param name="editorLevel">Уровень редактора</param>
    public void SetSingle(FiasEditorLevel editorLevel)
    {
      FiasLevelSet set1 = FiasLevelSet.FromBottomLevel(_BaseAddress.NameBottomLevel, true);
      FiasLevelSet set2 = FiasLevelSet.FromEditorLevel(editorLevel);
      _CellLevels = new FiasLevelSet[1] { set2 - set1 };
    }

    #endregion

    #region Текстовое представление

    /// <summary>
    /// Текстовое представление набора параметров
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      if (_BaseAddress.IsEmpty)
        return FiasTools.RF;
      else
        return _Handler.GetTextWithoutPostalCode(_BaseAddress);
    }

    #endregion

    #region Чтение и запись настроек

    /// <summary>
    /// Запись набора параметров в секцию конфигурации
    /// </summary>
    /// <param name="cfg">Секция для записи</param>
    public void WriteConfig(CfgPart cfg)
    {
      FiasAddressConvert convert = new FiasAddressConvert(_Handler.Source);
      convert.GuidMode = FiasAddressConvertGuidMode.AOGuid;
      cfg.SetString("BaseAddress", convert.ToString(_BaseAddress));
      if (_CellLevels != null)
      {
        cfg.SetInt("CellCount", _CellLevels.Length);
        for (int i = 0; i < _CellLevels.Length; i++)
          cfg.SetString("CellLevels" + (i + 1).ToString(), FiasLevelSetConvert.ToString(_CellLevels[i]));
      }
      else
        cfg.SetInt("CellCount", 0);
    }

    /// <summary>
    /// Чтение набора параметров из секции конфигурации
    /// </summary>
    /// <param name="cfg"></param>
    public void ReadConfig(CfgPart cfg)
    {
      FiasAddressConvert convert = new FiasAddressConvert(_Handler.Source);
      _BaseAddress = convert.Parse(cfg.GetString("BaseAddress"));
      int n = cfg.GetInt("CellCount");
      _CellLevels = new FiasLevelSet[n];
      for (int i = 0; i < n; i++)
      {
        string s = cfg.GetString("CellLevels" + (i + 1).ToString());
        FiasLevelSet ls;
        FiasLevelSetConvert.TryParse(s, out ls);
        _CellLevels[i] = ls;
      }
    }

    #endregion
  }
}
