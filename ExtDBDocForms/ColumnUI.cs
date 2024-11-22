// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using FreeLibSet.Config;
using FreeLibSet.Data.Docs;
using FreeLibSet.Core;
using FreeLibSet.Data;

// Сохранение значений редактируемых полей документов и поддокументов на стороне клиента

namespace FreeLibSet.Forms.Docs
{
  #region ColumnNewMode

  /// <summary>
  /// Способ установки значения для поля при добавлении (метод Insert()) или копировании (метод InsertCopy()) записи (свойство ColumnUI.NewMode)
  /// </summary>
  public enum ColumnNewMode
  {
    /// <summary>
    /// Действие по умолчанию. При выполнении Insert() поле получает значение по умолчанию (ColumnUI.DefaultValue),
    /// а при InsertCopy - значение оригинала.
    /// </summary>
    Default,

    /// <summary>
    /// При вызове Insert() берется ранее сохраненное значение. При InsertCopy() сохраняется значение оригинала.
    /// Используется, например, для ссылки на документ, который является в каком-либо смысле "родителем" для данного документа
    /// (например, для поля "Организация" в справочнике сотрудников). Предполагается, что когда создается много документов данного вида,
    /// с большой долей вероятности поле будет иметь одинаковое значение.
    /// </summary>
    Saved,

    /// <summary>
    /// При Insert() и InsertCopy() берется значение по умолчанию (ColumnUI.DefaultValue).
    /// Используется полей, которые обычно являются уникальными для каждого документа (например, Ф.И.О. человека или ИНН).
    /// </summary>
    AlwaysDefaultValue,

    /// <summary>
    /// При вызове Insert() берется предыдущее сохраненное значение, как в режиме Saved, при условии, что оно было введено вручную.
    /// Если пользователь не вводил значение, то работает аналогично режиму Default.
    /// При вызове InsertCopy() берется значение поля оригинального документа
    /// Используется для даты документа.
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
    /// Способ установки значения для поля при добавлении или копировании записи.
    /// По умолчанию - Default.
    /// </summary>
    public ColumnNewMode NewMode { get { return _NewMode; } set { _NewMode = value; } }
    private ColumnNewMode _NewMode;

    /// <summary>
    /// Значение по умолчанию для режимов NewMode=Default, AlwaysDefaultValue и SavedIfChangedElseDefault.
    /// Свойство либо однократно устанавливается при инициализации программы, либо
    /// каждый раз, когда требуется, с помощью обработчика события DefaultValueNeeded.
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
    /// Если обработчик события сам читает свойство DefaultValue, то рекурсивный вызов обработчика не выполняется.
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
    public void PerformInsert(IDBxExtValues values, bool insertCopy)
    {
      //AccDepClientExec.DocTypes.LoadSavedColumnValues();

      foreach (DBxExtValue value in values)
      {
        if (value.IsReadOnly)
          continue;
        if (_Owner.DocTypeBase.CalculatedColumns.Contains(value.Name))
          continue; // 10.06.2019
        ColumnUI colUI = this[value.Name];

        switch (colUI.NewMode)
        {
          case ColumnNewMode.Default:
            if (!insertCopy)
              SetIfNotNull(value, colUI.DefaultValue);
            break;

          case ColumnNewMode.AlwaysDefaultValue:
            object v = colUI.DefaultValue;
            //SetIfNotNull(Value, v);
            // 03.11.2015, 05.11.2015
            if ((v == null) || (v is DBNull))
            {
              if (insertCopy)
                value.SetNull();
            }
            else
              value.SetValue(v);
            break;

          case ColumnNewMode.Saved:
            if (insertCopy)
              continue;

            // Условия убраны 07.01.2022
            //if (Value.Value == null || Value.Value is DBNull)
            //{
              SetIfNotNull(value, colUI.Value);
            //}
            break;

          case ColumnNewMode.SavedIfChangedElseDefault:
            SetIfNotNull(value, colUI.Value);
            break;
          default:
            throw new BugException("Неизвестный NewMode=" + colUI.NewMode.ToString());
        }
      }
    }

    private static void SetIfNotNull(DBxExtValue docValue, object v)
    {
      if (v == null)
        return;
      if (v is DBNull)
        return;
      docValue.SetValue(v);
    }

    internal void PerformPost(IDBxExtValues values, IDBxExtValues orgValues)
    {
      foreach (DBxExtValue value in values)
      {
        if (value.IsReadOnly)
          continue;
        if (_Owner.DocTypeBase.CalculatedColumns.Contains(value.Name))
          continue; // 10.06.2019
        if (value.Grayed)
          continue;

        ColumnUI colUI = this[value.Name];

        switch (colUI.NewMode)
        {
          case ColumnNewMode.Default: // 10.06.2019
          case ColumnNewMode.Saved:
            colUI.SavedValue = value.Value;
            break;

          case ColumnNewMode.SavedIfChangedElseDefault:
            bool saveFlag;
            if (orgValues == null)
              saveFlag = true;
            else
            {
              DBxExtValue orgValue = orgValues[value.Name];
              saveFlag = !Object.Equals(value.Value, orgValue.Value);
            }
            if (saveFlag)
              colUI.SavedValue = value.Value;
            break;
          case ColumnNewMode.AlwaysDefaultValue:
            // не сохраняем
            break;
          default:
            throw new BugException("Неизвестный NewMode=" + colUI.NewMode.ToString());
        }
      }
    }

    #endregion
  }
}
