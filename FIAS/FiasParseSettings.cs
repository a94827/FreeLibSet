using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Config;
using FreeLibSet.Core;

/*
 * The BSD License
 * 
 * Copyright (c) 2020, Ageyev A.V.
 * 
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, 
 * this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice, 
 * this list of conditions and the following disclaimer in the documentation 
 * and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE 
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

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
