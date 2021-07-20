using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using AgeyevAV;
using AgeyevAV.Config;
using AgeyevAV.ExtDB.Docs;

/*
 * The BSD License
 * 
 * Copyright (c) 2015, Ageyev A.V.
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

/*
 * Сохранение значений редактируемых полей документов и поддокументов на стороне клиента
 */

namespace AgeyevAV.ExtForms.Docs
{
  #region ColumnNewMode

  /// <summary>
  /// Способ установки значения для поля при добавлении или копировании записи
  /// </summary>
  public enum ColumnNewMode
  {
    /// <summary>
    /// Действие по умолчанию. При выполнении Insert поле значение по умолчанию,
    /// а при InsertCopy - значение оригинала
    /// </summary>
    Default,

    /// <summary>
    /// При Insert берется ранее сохраненное значение. При InsertCopy сохраняется
    /// значение оригинала.
    /// Используйте, например, для ссылки на организацию
    /// </summary>
    Saved,

    /// <summary>
    /// При Insert и InsertCopy берется значение по умолчанию
    /// </summary>
    AlwaysDefaultValue,

    /// <summary>
    /// Берется предыдущее сохраненное значение Saved при условии, что оно было
    /// введено вручную.
    /// Используется для даты документа
    /// </summary>
    SavedIfChangedElseDefault,
  }

  #endregion

  /// <summary>
  /// Параметры поля документа или поддокумента для клиента
  /// </summary>
  public sealed class ColumnUI
  {
    #region Защищенный конструктор

    internal ColumnUI(string columnName)
    {
      _ColumnName = columnName;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя столбца в базе данных
    /// </summary>
    public string ColumnName { get { return _ColumnName; } }
    private string _ColumnName;

    /// <summary>
    /// Способ установки значения для поля при добавлении или копировании записи
    /// </summary>
    public ColumnNewMode NewMode { get { return _NewMode; } set { _NewMode = value; } }
    private ColumnNewMode _NewMode;

    /// <summary>
    /// Значение по умолчанию для режимов Default, AlwaysDefaultValue и SavedIfChangedElseDefault.
    /// Оно либо однократно устанавливается при инициализации программы, либо
    /// каждый раз, когда требуется, с помощью события DefaultValueNeeded.
    /// </summary>
    public object DefaultValue 
    { 
      get 
      {
        if (DefaultValueNeeded != null && (!_InsideDefaultValueNeeded))
        {
          _InsideDefaultValueNeeded = true;
          try
          {
            DefaultValueNeeded(this, EventArgs.Empty);
          }
          finally
          {
            _InsideDefaultValueNeeded = false;
          }
        }
        return _DefaultValue; 
      } 
      set 
      { 
        _DefaultValue = value; 
      } 
    }
    private object _DefaultValue;

    internal object SavedValue
    {
      get { return _SavedValue; }
      set
      {
        _SavedValue = value;
        _HasSavedValue = true;
      }
    }
    private object _SavedValue;

    private bool _HasSavedValue;

    #endregion

    #region События

    /// <summary>
    /// Событие каждый раз вызывается, когда требуется значение по умолчанию.
    /// Обработчик может установить свойство DefaultValue.
    /// Событие вызывается при каждом чтении свойства DefaultValue. Если требуется выполнение длительных
    /// действий, следует предусмотреть какую-нибудь проверку, чтобы не выполнять их многократно.
    /// Если обработчик события сам читает свойство DefaultValue, то рекурсивный вызов не выполняется.
    /// </summary>
    public event EventHandler DefaultValueNeeded;

    /// <summary>
    /// Устанавливается на время вызова события DefaultValueNeeded, чтобы исключить реентрантный вызов
    /// </summary>
    private bool _InsideDefaultValueNeeded;

    #endregion

    #region Методы

    /// <summary>
    /// Возвращает свойство ColumnName
    /// </summary>
    /// <returns>текстовое представление</returns>
    public override string ToString()
    {
      return ColumnName;
    }

    #endregion

    #region Для доступа не из редактора

    #region Неформатированный доступ

    /// <summary>
    /// Свойство может использоваться при использовании вне редактора документа в
    /// диалогах, работающих со значением поля. 
    /// При чтении возвращается сохраненное
    /// значение (если сохранение выполнялось) или значение по умолчанию. При записи свойства значение сохраняется.
    /// Работа этого свойства не зависит от свойства NewMode
    /// </summary>
    public object Value
    {
      //get
      //{
      //  if (HasSavedValue)
      //    return SavedValue;
      //  else
      //    return DefaultValue;
      //}
      //set
      //{
      //  if (NewMode == ColumnNewMode.Saved || NewMode == ColumnNewMode.SavedIfChangedElseDefault)
      //  {
      //    SavedValue = value;
      //    HasSavedValue = true;
      //  }
      //}

      // 10.06.2019
      // Реализация этого свойства больше не зависит от NewMode
      get
      {
        if (_HasSavedValue)
          return SavedValue;
        else
          return DefaultValue;
      }
      set
      {
        SavedValue = value;
      }
    }

    #endregion

    #region Форматированный доступ

    /// <summary>
    /// Форматированный доступ к сохраняемому значению.
    /// См. описание свойства Value.
    /// </summary>
    public string AsString
    {
      get { return DataTools.GetString(Value); }
      set
      {
        if (String.IsNullOrEmpty(value))
          Value = null;
        else
          Value = value;
      }
    }

    /// <summary>
    /// Форматированный доступ к сохраняемому значению.
    /// См. описание свойства Value.
    /// </summary>
    public bool AsBoolean
    {
      get { return DataTools.GetBool(Value); }
      set { Value = value; }
    }

    /// <summary>
    /// Форматированный доступ к сохраняемому значению.
    /// См. описание свойства Value.
    /// </summary>
    public int AsInteger
    {
      get { return DataTools.GetInt(Value); }
      set
      {
        if (value == 0)
          Value = null;
        else
          Value = value;
      }
    }

    /// <summary>
    /// Форматированный доступ к сохраняемому значению.
    /// См. описание свойства Value.
    /// </summary>
    public float AsSingle
    {
      get { return DataTools.GetSingle(Value); }
      set
      {
        if (value == 0f)
          Value = null;
        else
          Value = value;
      }
    }

    /// <summary>
    /// Форматированный доступ к сохраняемому значению.
    /// См. описание свойства Value.
    /// </summary>
    public double AsDouble
    {
      get { return DataTools.GetDouble(Value); }
      set
      {
        if (value == 0.0)
          Value = null;
        else
          Value = value;
      }
    }

    /// <summary>
    /// Форматированный доступ к сохраняемому значению.
    /// См. описание свойства Value.
    /// </summary>
    public decimal AsDecimal
    {
      get { return DataTools.GetDecimal(Value); }
      set
      {
        if (value == 0m)
          Value = null;
        else
          Value = value;
      }
    }

    /// <summary>
    /// Форматированный доступ к сохраняемому значению.
    /// См. описание свойства Value.
    /// </summary>
    public DateTime? AsNullableDate
    {
      get { return DataTools.GetNullableDateTime(Value); }
      set
      {
        if (value.HasValue)
          Value = value.Value;
        else
          Value = null;
      }
    }

    #endregion

    #endregion

#if XXX                             
    #region Стандартные реализации получения значений по умолчанию

    /// <summary>
    /// Используется для поля, содержащего дату документа. 
    /// Для такого поля по умолчанию используется рабочая дата
    /// </summary>
    public void SetForWorkDate()
    {
      NewMode = ColumnNewMode.SavedIfChangedElseDefault;
      DefaultValueNeeded += new EventHandler(WorkDate_DefaultValueNeeded);
    }

    void WorkDate_DefaultValueNeeded(object Sender, EventArgs Args)
    {
      DefaultValue = AccDepClientExec.WorkDate;
    }

    /// <summary>
    /// Используется для поля, содержащего ссылку на нашу организацию
    /// </summary>
    public void SetForOurOrg()
    {
      NewMode = ColumnNewMode.Saved;
      DefaultValueNeeded += new EventHandler(OurOrg_DefaultValueNeeded);
    }

    void OurOrg_DefaultValueNeeded(object Sender, EventArgs Args)
    {
      DefaultValue = AccDepClientExec.OurOrgId;
    }

    #endregion
#endif
  }

  /// <summary>
  /// Коллекция описаний полей для объектов DocTypeUI или SubDocTypeUI
  /// (свойство Columns)
  /// </summary>
  public sealed class ColumnUIList
  {
    #region Защищенный Конструктор

    internal ColumnUIList(DocTypeUIBase owner)
    {
      _Items = new Dictionary<string, ColumnUI>();
      _Owner = owner;
    }

    private DocTypeUIBase _Owner;

    #endregion

    #region Доступ к полям

    private Dictionary<string, ColumnUI> _Items;

    /// <summary>
    /// Доступ к столбцу по имени
    /// </summary>
    /// <param name="columnName">Имя столбца документа или поддокумента.
    /// Ссылочные имена (с точками) не допускаются</param>
    /// <returns>Объект для столбца</returns>
    public ColumnUI this[string columnName]
    {
      get                                          
      {
        if (String.IsNullOrEmpty(columnName))
          throw new ArgumentNullException("columnName");
        ColumnUI res;
        if (!_Items.TryGetValue(columnName, out res))
        {
          if (columnName.IndexOf('.') >= 0)
            throw new ArgumentException("Ссылочные поля не допускаются", "columnName");

          res = new ColumnUI(columnName);
          _Items.Add(columnName, res);
        }
        return res;
      }
    }

    #endregion

    #region Сохранение параметров между вызовами программы

#if XXX

    internal void SaveConfig(CfgPart Part)
    {
      for (int i = 0; i < Count; i++)
      {
        if (this[i].NewMode == ColumnNewMode.Saved || this[i].NewMode == ColumnNewMode.SavedIfChangedElseDefault)
        {
          CfgPart Part2 = Part.GetChild(this[i].ColumnName, true);
          Part2.SetBool("HasValue", this[i].HasSavedValue);
          if (this[i].HasSavedValue)
          {
            object v = this[i].SavedValue;
            if (v == null || v is DBNull)
            {
              Part2.SetString("Type", "Null");
              Part2.SetString("Value", null);
              continue;
            }
            if (v is String)
            {
              Part2.SetString("Type", "String");
              Part2.SetString("Value", (string)v);
              continue;
            }
            if (v is DateTime)
            {
              Part2.SetString("Type", "DateTime");
              Part2.SetNullableDateTime("Value", (DateTime)v);
              continue;
            }
            if (v is float)
            {
              Part2.SetString("Type", "Decimal");
              Part2.SetDecimal("Value", (decimal)(float)v);
              continue;
            }
            if (v is double)
            {
              Part2.SetString("Type", "Decimal");
              Part2.SetDecimal("Value", (decimal)(double)v);
              continue;
            }
            if (v is decimal)
            {
              Part2.SetString("Type", "Decimal");
              Part2.SetDecimal("Value", (decimal)v);
              continue;
            }
            if (v is bool)
            {
              Part2.SetString("Type", "Boolean");
              Part2.SetBool("Value", (bool)v);
              continue;
            }
            Part2.SetString("Type", "Int");
            Part2.SetInt("Value", DataTools.GetInt(v));
          }
        }
      }
    }

    internal void LoadConfig(CfgPart Part)
    {
      for (int i = 0; i < Count; i++)
      {
        if (this[i].NewMode == ColumnNewMode.Saved /*|| this[i].NewMode == ColumnNewMode.SavedIfChangedElseDefault*/)
        {
          CfgPart Part2 = Part.GetChild(this[i].ColumnName, false);
          if (Part2 == null)
            continue;
          this[i].HasSavedValue = Part2.GetBool("HasValue");
          if (this[i].HasSavedValue)
          {
            this[i].SavedValue = null;
            string TypeStr = Part2.GetString("Type");
            switch (TypeStr)
            {
              case "String":
                this[i].SavedValue = Part2.GetString("Value");
                break;
              case "DateTime":
                Nullable<DateTime> dt = Part2.GetNullableDateTime("Value");
                if (dt.HasValue)
                  this[i].SavedValue = dt.Value;
                break;
              case "Decimal":
                this[i].SavedValue = Part2.GetDecimal("Value");
                break;
              case "Boolean":
                this[i].SavedValue = Part2.GetBool("Value");
                break;
              case "Int":
                this[i].SavedValue = Part2.GetInt("Value");
                break;
            }
          }
        }
      }
    }
    internal bool HasSaveableColumns
    {
      get
      {
        for (int i = 0; i < Count; i++)
        {
          if (this[i].NewMode == ColumnNewMode.Saved || this[i].NewMode == ColumnNewMode.SavedIfChangedElseDefault)
            return true;
        }
        return false;
      }
    }

#endif

    #endregion

    #region Команды, вызываемые редактором

    /// <summary>
    /// Вызывается для начальной инициализации значений в режимах Insert и InsertCopy
    /// </summary>
    /// <param name="values">Значения</param>
    /// <param name="insertCopy">true для режима InsertCopy, false для Insert</param>
    public void PerformInsert(IDBxDocValues values, bool insertCopy)
    {
      //AccDepClientExec.DocTypes.LoadSavedColumnValues();

      foreach (DBxDocValue Value in values)
      {
        if (Value.IsReadOnly)
          continue;
        if (_Owner.DocTypeBase.CalculatedColumns.Contains(Value.Name))
          continue; // 10.06.2019
        ColumnUI UI = this[Value.Name];

        switch (UI.NewMode)
        {
          case ColumnNewMode.Default:
            if (!insertCopy)
              SetIfNotNull(Value, UI.DefaultValue);
            break;

          case ColumnNewMode.AlwaysDefaultValue:
            object v = UI.DefaultValue;
            //SetIfNotNull(Value, v);
            // 03.11.2015, 05.11.2015
            if ((v == null) || (v is DBNull))
            {
              if (insertCopy)
                Value.SetNull();
            }
            else
              Value.SetValue(v);
            break;

          case ColumnNewMode.Saved:
            if (insertCopy)
              continue;
            if (Value.Value == null || Value.Value is DBNull)
            {
              SetIfNotNull(Value, UI.Value);
            }
            break;

          case ColumnNewMode.SavedIfChangedElseDefault:
            SetIfNotNull(Value, UI.Value);
            break;
          default:
            throw new BugException("Неизвестный NewMode=" + UI.NewMode.ToString());
        }
      }
    }

    private static void SetIfNotNull(DBxDocValue docValue, object v)
    {
      if (v == null)
        return;
      if (v is DBNull)
        return;
      docValue.SetValue(v);
    }

    internal void PerformPost(IDBxDocValues values, IDBxDocValues orgValues)
    {
      foreach (DBxDocValue Value in values)
      {
        if (Value.IsReadOnly)
          continue;
        if (_Owner.DocTypeBase.CalculatedColumns.Contains(Value.Name))
          continue; // 10.06.2019
        if (Value.Grayed)
          continue;

        ColumnUI UI = this[Value.Name];

        switch (UI.NewMode)
        {
          case ColumnNewMode.Default: // 10.06.2019
          case ColumnNewMode.Saved:
            UI.SavedValue = Value.Value;
            break;

          case ColumnNewMode.SavedIfChangedElseDefault:
            bool SaveFlag;
            if (orgValues == null)
              SaveFlag = true;
            else
            {
              DBxDocValue OrgValue = orgValues[Value.Name];
              SaveFlag = !Object.Equals(Value.Value, OrgValue.Value);
            }
            if (SaveFlag)
              UI.SavedValue = Value.Value;
            break;
          case ColumnNewMode.AlwaysDefaultValue:
            // не сохраняем
            break;
          default:
            throw new BugException("Неизвестный NewMode=" + UI.NewMode.ToString());
        }
      }
    }

    #endregion
  }
}
