﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using FreeLibSet.Config;
using FreeLibSet.Core;
using FreeLibSet.Collections;

/*
 * The BSD License
 * 
 * Copyright (c) 2006-2015, Ageyev A.V.
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

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Конфигурация табличного просмотра с возможностью записи в CfgPart
  /// Хранит порядок и размеры столбцов, всплывающие подсказки
  /// </summary>
  [Serializable]
  public sealed class EFPDataGridViewConfig : IReadOnlyObject, ICloneable
  {
    #region Конструктор

    /// <summary>
    /// Создает пустой объект конфигурации
    /// </summary>
    public EFPDataGridViewConfig()
    {
      _Columns = new EFPDataGridViewConfigColumns(this);
      _FrozenColumns = 0;
      _StartColumnName = String.Empty;
      _CurrentCellToolTip = true;
      _ToolTips = new EFPDataGridViewConfigToolTips(this);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Столбцы, входящие в просмотр
    /// </summary>
    public EFPDataGridViewConfigColumns Columns { get { return _Columns; } }
    private EFPDataGridViewConfigColumns _Columns;

    /// <summary>
    /// Количество замороженных столбцов в левой части просмотра
    /// </summary>
    public int FrozenColumns
    {
      get { return _FrozenColumns; }
      set
      {
        CheckNotReadOnly();
        _FrozenColumns = value;
      }
    }
    private int _FrozenColumns;

    /// <summary>
    /// Имя столбца, который становится активным при открытии просмотра
    /// Пустая строка - текущий столбец сохраняется в секции конфигурации категории "GridView" между сеансами работы программы
    /// или выбирается просмотром автоматически.
    /// </summary>
    public string StartColumnName
    {
      get { return _StartColumnName; }
      set
      {
        CheckNotReadOnly();
        if (value == null)
          value = String.Empty;
        _StartColumnName = value;
      }
    }
    private string _StartColumnName;

    /// <summary>
    /// Надо ли выводить в подсказке содержимое текущей ячейки?
    /// По умолчанию - true (выводить)
    /// </summary>
    public bool CurrentCellToolTip
    {
      get { return _CurrentCellToolTip; }
      set
      {
        CheckNotReadOnly();
        _CurrentCellToolTip = value;
      }
    }
    private bool _CurrentCellToolTip;

    /// <summary>
    /// Имена частей всплывающих подсказок для строки
    /// </summary>
    public EFPDataGridViewConfigToolTips ToolTips { get { return _ToolTips; } }
    private EFPDataGridViewConfigToolTips _ToolTips;

    /// <summary>
    /// Значок настройки, используемый в диалоге настройки табличного просмотра.
    /// Используется только для настройки по умолчанию и именных настройках, заданных в GridProducer.
    /// Если свойство не установлено (по умолчанию), используется значок "No"
    /// </summary>
    public string ImageKey
    {
      get { return _ImageKey; }
      set { _ImageKey = value; }
    }
    private string _ImageKey;

    #endregion

    #region Методы

    /// <summary>
    /// Создание копии конфигурации с учетом реального расположения столбцов в просмотре
    /// </summary>
    /// <param name="controlProvider"></param>
    /// <returns></returns>
    public EFPDataGridViewConfig Clone(IEFPGridControl controlProvider)
    {
      EFPDataGridViewConfig Res = new EFPDataGridViewConfig();

      EFPDataViewColumnInfo[] RealColInfos = controlProvider.GetVisibleColumnsInfo();

      for (int i = 0; i < RealColInfos.Length; i++)
      {
        if (RealColInfos[i].ColumnProducer == null) // чужой столбец
          continue;
        EFPDataGridViewConfigColumn Col2 = new EFPDataGridViewConfigColumn(RealColInfos[i].Name); // ??
        controlProvider.InitColumnConfig(Col2);

        if (Res.Columns.Contains(Col2.ColumnName))
          continue; // бяка - два одинаковых столбца
        Res.Columns.Add(Col2);
      }
      // Не учитываем количество замороженных столбцов в просмотре. Там могут быть
      // лишние столбцы. Берем количество из текущей настройки
      //Res.FrozenColumns = DocGridHandler.FrozenColumns;
      Res.FrozenColumns = FrozenColumns;
      Res.StartColumnName = StartColumnName;

      // Всплывающие подсказки просто копируются
      Res.CurrentCellToolTip = CurrentCellToolTip;
      for (int i = 0; i < ToolTips.Count; i++)
        Res.ToolTips.Add(ToolTips[i]);
      return Res;
    }

    #endregion

    #region Чтение и запись конфигурации

    /// <summary>
    /// Записать параметры в секцию конфигурации
    /// </summary>
    /// <param name="cfg">Записываемая секция конфигурации</param>
    public void WriteConfig(CfgPart cfg)
    {
#if DEBUG
      if (cfg == null)
        throw new ArgumentNullException("cfg");
#endif


#if DEBUG // TODO: 20.03.2018. Убрать, когда найду ошибку в mono

      if (EnvironmentTools.IsMono)
      {
        try
        {
          if (Columns.Count == 0)
          {
            throw new BugException("Конфигурация не содержит ни одного столбца");
          }
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e, "Ошибка проверки при записи конфигурации табличного просмотра");
        }
      }

#endif

      string s = String.Empty;
      for (int i = 0; i < Columns.Count; i++)
      {
        if (i > 0)
          s += ",";
        s += Columns[i].ColumnName;
      }
      cfg.SetString("VisibleColumns", s);

      CfgPart cfg2 = cfg.GetChild("Columns", true);
      for (int i = 0; i < Columns.Count; i++)
      {
        CfgPart cfg3 = cfg2.GetChild(Columns[i].ColumnName, true);
        cfg3.SetInt("Width", Columns[i].Width);
        cfg3.SetBool("FillMode", Columns[i].FillMode);
        if (Columns[i].FillMode)
          cfg3.SetInt("FillWeight", Columns[i].FillWeight);
      }

      cfg.SetInt("FrozenColumns", FrozenColumns);
      cfg.SetString("StartColumn", StartColumnName);

      // Записываем настройки подсказок обязательно
      //if (EFPApp.ShowToolTips)
      //{
      cfg.SetBool("NoCurrentCellToolTip", !CurrentCellToolTip);
      s = String.Empty;
      for (int i = 0; i < ToolTips.Count; i++)
      {
        if (i > 0)
          s += ",";
        s += ToolTips[i];
      }
      cfg.SetString("ToolTips", s);
      //}
    }

    /// <summary>
    /// Прочитать данные из секции конфигурации и поместить их в текущий объект
    /// </summary>
    /// <param name="cfg">Секция конфигурации</param>
    public void ReadConfig(CfgPart cfg)
    {
#if DEBUG
      if (cfg == null)
        throw new ArgumentNullException("cfg");
#endif

      Columns.Clear();
      string VisibleCols = cfg.GetString("VisibleColumns");
      string[] a = VisibleCols.Split(',');
      CfgPart cfg2 = cfg.GetChild("Columns", true);
      for (int i = 0; i < a.Length; i++)
      {
        string ColumnName = a[i].Trim();
        if (String.IsNullOrEmpty(ColumnName))
          continue;
        EFPDataGridViewConfigColumn Col = new EFPDataGridViewConfigColumn(ColumnName);
        CfgPart cfg3 = cfg2.GetChild(ColumnName, false);
        if (cfg3 != null)
        {
          Col.Width = cfg3.GetInt("Width");
          Col.FillMode = cfg3.GetBool("FillMode");
          int x = cfg3.GetInt("FillWeight");
          if (x > 0)
            Col.FillWeight = x;
        }
        Columns.Add(Col);
      }
      if (Columns.Count == 0)
        throw new InvalidOperationException("В конфигурации не задано ни одного столбца");

      FrozenColumns = cfg.GetInt("FrozenColumns");
      StartColumnName = cfg.GetString("StartColumn");

      CurrentCellToolTip = !cfg.GetBool("NoCurrentCellToolTip");
      string ToolTipNames = cfg.GetString("ToolTips");
      ToolTips.Clear();
      if (!String.IsNullOrEmpty(ToolTipNames))
      {
        a = ToolTipNames.Split(',');
        for (int i = 0; i < a.Length; i++)
          ToolTips.Add(a[i].Trim());
      }
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Возвращает true, если изменение параметров запрещено
    /// </summary>
    public bool IsReadOnly { get { return _Columns.IsReadOnly; } }

    /// <summary>
    /// Генерирует исключение при IsReadOnly=true.
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    /// <summary>
    /// Переводит конфигурацию в режим "Только чтение".
    /// Повторные вызовы игнорируются.
    /// </summary>
    public void SetReadOnly()
    {
      _Columns.SetReadOnly();
      _ToolTips.SetReadOnly();
    }

    #endregion

    #region ICloneable Members

    /// <summary>
    /// Создает копию конфигурации
    /// </summary>
    /// <returns></returns>
    public EFPDataGridViewConfig Clone()
    {
      EFPDataGridViewConfig Res = new EFPDataGridViewConfig();
      Columns.CopyTo(Res.Columns);
      Res.FrozenColumns = FrozenColumns;
      Res.StartColumnName = StartColumnName;
      Res.CurrentCellToolTip = CurrentCellToolTip;
      ToolTips.CopyTo(Res.ToolTips);
      return Res;
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    #endregion
  }

  /// <summary>
  /// Реализация свойства EFPDataGridViewConfig.Columns
  /// </summary>
  [Serializable]
  public sealed class EFPDataGridViewConfigColumns : NamedList<EFPDataGridViewConfigColumn>
  {
    #region Конструктор

    internal EFPDataGridViewConfigColumns(EFPDataGridViewConfig owner)
    {
      _Owner = owner;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Объект-владелец
    /// </summary>
    public EFPDataGridViewConfig Owner { get { return _Owner; } }
    private EFPDataGridViewConfig _Owner;

    #endregion

    #region Методы

    /// <summary>
    /// Добавить столбец
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    /// <returns>Объект EFPDataGridViewConfigColumn</returns>
    public EFPDataGridViewConfigColumn Add(string columnName)
    {
      CheckNotReadOnly();

      EFPDataGridViewConfigColumn Item = new EFPDataGridViewConfigColumn(columnName);
      Add(Item);
      return Item;
    }

    /// <summary>
    /// Добавить столбец, который будет занимать определенную часть табличного просмотра
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    /// <param name="fillWeight">Процент табличного просмотра для столбца</param>
    /// <returns>Объект EFPDataGridViewConfigColumn</returns>
    public EFPDataGridViewConfigColumn AddFill(string columnName, int fillWeight)
    {
      CheckNotReadOnly();

      EFPDataGridViewConfigColumn Item = new EFPDataGridViewConfigColumn(columnName);
      Item.FillMode = true;
      Item.FillWeight = fillWeight;
      Add(Item);
      return Item;
    }

    /// <summary>
    /// Добавить столбец, который будет 100% табличного просмотра
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    /// <returns>Объект EFPDataGridViewConfigColumn</returns>
    public EFPDataGridViewConfigColumn AddFill(string columnName)
    {
      return AddFill(columnName, 100);
    }

    /// <summary>
    /// Копирование списка настроек столбов в другую конфигурацию
    /// Создаются копии объектов ColumnConfig
    /// </summary>
    /// <param name="other">Заполняемый список</param>
    public void CopyTo(EFPDataGridViewConfigColumns other)
    {
      for (int i = 0; i < Count; i++)
        other.Add(this[i].Clone());
    }

    internal new void SetReadOnly()
    {
      base.SetReadOnly();
      for (int i = 0; i < Count; i++)
        this[i].SetReadOnly();
    }

    #endregion
  }

  /// <summary>
  /// Параметры ширины табличного просмотра
  /// </summary>
  [Serializable]
  public sealed class EFPDataGridViewConfigColumn : ObjectWithCode, IReadOnlyObject, ICloneable
  {
    #region Конструктор

    /// <summary>
    /// Создает объект для столбца
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    public EFPDataGridViewConfigColumn(string columnName)
      : base(columnName)
    {
      _Width = 0; // автоматически
      _FillMode = false;
      _FillWeight = 100;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя столбца (свойство EFPDataGridViewColumn.Name)
    /// </summary>
    public string ColumnName { get { return base.Code; } }

    /// <summary>
    /// Ширина столба в пикселах
    /// </summary>
    public int Width
    {
      get { return _Width; }
      set
      {
        CheckNotReadOnly();
        _Width = value;
      }
    }
    private int _Width;

    /// <summary>
    /// Режим изменения размера столбца по размеру табличного просмотра на основании свойства FillWeight
    /// </summary>
    public bool FillMode
    {
      get { return _FillMode; }
      set
      {
        CheckNotReadOnly();
        _FillMode = value;
      }
    }
    private bool _FillMode;

    /// <summary>
    /// Процент ширины столбца относительно ширины табличного просмотра.
    /// Свойство используется при FillMode=true
    /// </summary>
    public int FillWeight
    {
      get { return _FillWeight; }
      set
      {
        CheckNotReadOnly();
        _FillWeight = value;
      }
    }
    private int _FillWeight;

    #endregion

    #region ICloneable Members

    /// <summary>
    /// Создает копию объекта
    /// </summary>
    /// <returns>Новый объект EFPDataGridViewConfigColumn</returns>
    public EFPDataGridViewConfigColumn Clone()
    {
      EFPDataGridViewConfigColumn NewCol = new EFPDataGridViewConfigColumn(ColumnName);
      NewCol.Width = Width;
      NewCol.FillMode = FillMode;
      NewCol.FillWeight = FillWeight;
      return NewCol;
    }

    object ICloneable.Clone()
    {
      return this.Clone();
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Возвращает true, если объект-владелец EFPDataGridViewConfig переведен в режим "Только чтение"
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// Ненерирует исключение при IsReadOnly=true
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    internal void SetReadOnly()
    {
      _IsReadOnly = true;
    }

    #endregion
  }

  /// <summary>
  /// Реализация свойства EFPDataGridViewConfig.ToolTips
  /// </summary>
  [Serializable]
  public sealed class EFPDataGridViewConfigToolTips : NamedList<EFPDataGridViewConfigToolTip>
  {
    #region Конструктор

    internal EFPDataGridViewConfigToolTips(EFPDataGridViewConfig owner)
    {
      _Owner = owner;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Объект-владелец
    /// </summary>
    public EFPDataGridViewConfig Owner { get { return _Owner; } }
    private EFPDataGridViewConfig _Owner;

    #endregion

    #region Методы

    /// <summary>
    /// Добавляет подсказку в список.
    /// </summary>
    /// <param name="toolTipName">Имя подсказки для хранения в секции конфигурации.</param>
    /// <returns>Созданный объект EFPDataGridViewConfigToolTip</returns>
    public EFPDataGridViewConfigToolTip Add(string toolTipName)
    {
      CheckNotReadOnly();

      EFPDataGridViewConfigToolTip Item = new EFPDataGridViewConfigToolTip(toolTipName);
      Add(Item);
      return Item;
    }

    /// <summary>
    /// Копирование списка настроек столбов в другую конфигурацию
    /// Создаются копии объектов ColumnConfig
    /// </summary>
    /// <param name="other"></param>
    public void CopyTo(EFPDataGridViewConfigToolTips other)
    {
      for (int i = 0; i < Count; i++)
        other.Add(this[i].Clone());
    }

    internal new void SetReadOnly()
    {
      base.SetReadOnly();
      for (int i = 0; i < Count; i++)
        this[i].SetReadOnly();
    }

    #endregion
  }

  /// <summary>
  /// Настройка для одной всплывающей подсказуи
  /// В текущей реализации не содержит никаких настраиваемых параметров
  /// </summary>
  [Serializable]
  public sealed class EFPDataGridViewConfigToolTip : ObjectWithCode, IReadOnlyObject, ICloneable
  {
    #region Конструктор

    /// <summary>
    /// Создает объект настройки подсказки
    /// </summary>
    /// <param name="toolTipName">Имя подсказки для сохранения в секции конфигурации</param>
    public EFPDataGridViewConfigToolTip(string toolTipName)
      : base(toolTipName)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя подсказки для сохранения в секции конфигурации
    /// </summary>
    public string ToolTipName { get { return base.Code; } }

    #endregion

    #region ICloneable Members

    /// <summary>
    /// Создает копию объекта EFPDataGridViewConfigToolTip.
    /// </summary>
    /// <returns>Новый объект</returns>
    public EFPDataGridViewConfigToolTip Clone()
    {
      return new EFPDataGridViewConfigToolTip(ToolTipName);
    }

    object ICloneable.Clone()
    {
      return this.Clone();
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// В текущей реализации не имеет смысла, т.к. нет настраиваемых параметров
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// Генерирует исключение, если IsReadOnly=true.
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    internal void SetReadOnly()
    {
      _IsReadOnly = true;
    }

    #endregion
  }


  /// <summary>
  /// Диалог для редактирования настроек табличного просмотра
  /// </summary>
  public sealed class EFPDataGridViewConfigDialog
  {
    #region Конструктор

    /// <summary>
    /// Создает диалог
    /// </summary>
    public EFPDataGridViewConfigDialog()
    {
      _ConfigCategory = EFPConfigCategories.GridConfig;
      _HistoryCategory = EFPConfigCategories.GridConfigHistory;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер табличного просмотра, который настраивается.
    /// Используются свойство ConfigManager и ConfigHandler.
    /// Свойство должно быть установлено до вызова ShowDialog()
    /// </summary>
    public IEFPGridControl CallerControlProvider
    {
      get { return _CallerControlProvider; }
      set { _CallerControlProvider = value; }
    }
    private IEFPGridControl _CallerControlProvider;

    /// <summary>
    /// Источник настроек.
    /// Свойство должно быть установлено до вызова ShowDialog()
    /// </summary>
    public IEFPConfigurableGridProducer GridProducer
    {
      get { return _GridProducer; }
      set { _GridProducer = value; }
    }
    private IEFPConfigurableGridProducer _GridProducer;

    /// <summary>
    /// Вход и выход: редактируемая настройка
    /// </summary>
    public EFPDataGridViewConfig Value
    {
      get { return _Value; }
      set { _Value = value; }
    }
    private EFPDataGridViewConfig _Value;

    /// <summary>
    /// Категория, используемая для хранения настроек и пользовательских настроек.
    /// По умолчанию EFPConfigCategories.GridConfig
    /// </summary>
    public string ConfigCategory
    {
      get { return _ConfigCategory; }
      set
      {
        if (String.IsNullOrEmpty(value))
          throw new ArgumentNullException();
        _ConfigCategory = value;
      }
    }
    private string _ConfigCategory;

    /// <summary>
    /// Категория, используемая для хранения списка пользовательских настроек.
    /// По умолчанию EFPConfigCategories.GridConfigHistory
    /// </summary>
    public string HistoryCategory
    {
      get { return _HistoryCategory; }
      set
      {
        if (String.IsNullOrEmpty(value))
          throw new ArgumentNullException();
        _HistoryCategory = value;
      }
    }
    private string _HistoryCategory;

    #endregion

    #region Показ диалога

    /// <summary>
    /// Выводит диалог настроек на экран.
    /// </summary>
    /// <returns>Ok, если пользователь сохранил настройки</returns>
    public DialogResult ShowDialog()
    {
      if (CallerControlProvider == null)
        throw new NullReferenceException("Свойство CallerControlProvider не установлено");
      if (GridProducer == null)
        throw new NullReferenceException("Свойство GridProducer должно быть установлено");

      if (Value == null)
        Value = new EFPDataGridViewConfig(); // пустышка. Может

      GridConfigForm Form = new GridConfigForm(CallerControlProvider, ConfigCategory, HistoryCategory);
      Form.Editor = GridProducer.CreateEditor(Form.MainPanel, Form.FormProvider, CallerControlProvider);

      Form.FillSetItems();

      // Существующие данные 
      Form.Editor.WriteFormValues(Value);
      TempCfg OriginalConfigSection = new TempCfg();
      Value.WriteConfig(OriginalConfigSection);
      Form.SetComboBox.SelectedMD5Sum = OriginalConfigSection.MD5Sum(); // выбираем подходящий набор, если есть

      DialogResult res = EFPApp.ShowDialog(Form, true);
      if (res == DialogResult.OK)
        Value = Form.ResultConfig;
      return res;
    }

    #endregion
  }
}
