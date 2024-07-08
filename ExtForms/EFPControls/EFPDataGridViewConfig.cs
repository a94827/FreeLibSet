// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Config;
using FreeLibSet.Core;
using FreeLibSet.Collections;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Конфигурация табличного просмотра с возможностью записи в <see cref="CfgPart"/>
  /// Хранит признак видимости, порядок и размеры столбцов, флажки отображения всплывающих подсказок.
  /// Обычно используется совместно с <see cref="EFPGridProducer"/> и <see cref="EFPStdConfigurableDataGridView"/>, но может быть и собственная реализация
  /// выбора столбцов в прикладном коде на уровне <see cref="EFPDataGridView"/>.
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
      _Columns = new ColumnCollection(this);
      _FrozenColumns = 0;
      _StartColumnName = String.Empty;
      _CurrentCellToolTip = true;
      _ToolTips = new ToolTipCollection(this);
    }

    #endregion

    #region Столбцы

    /// <summary>
    /// Реализация свойства <see cref="Columns"/>
    /// </summary>
    [Serializable]
    public sealed class ColumnCollection : NamedList<EFPDataGridViewConfigColumn>
    {
      #region Конструктор

      internal ColumnCollection(EFPDataGridViewConfig owner)
      {
        _Owner = owner;
      }

      private EFPDataGridViewConfig _Owner;

      #endregion

      #region Методы

      /// <summary>
      /// Добавить столбец
      /// </summary>
      /// <param name="columnName">Имя столбца</param>
      /// <returns>Объект <see cref="EFPDataGridViewConfigColumn"/></returns>
      public EFPDataGridViewConfigColumn Add(string columnName)
      {
        CheckNotReadOnly();

        EFPDataGridViewConfigColumn item = new EFPDataGridViewConfigColumn(columnName);
        Add(item);
        return item;
      }

      /// <summary>
      /// Добавить столбец, который будет занимать определенную часть табличного просмотра
      /// </summary>
      /// <param name="columnName">Имя столбца</param>
      /// <param name="fillWeight">Процент табличного просмотра для столбца</param>
      /// <returns>Объект <see cref="EFPDataGridViewConfigColumn"/></returns>
      public EFPDataGridViewConfigColumn AddFill(string columnName, int fillWeight)
      {
        CheckNotReadOnly();

        EFPDataGridViewConfigColumn item = new EFPDataGridViewConfigColumn(columnName);
        item.FillMode = true;
        item.FillWeight = fillWeight;
        Add(item);
        return item;
      }

      /// <summary>
      /// Добавить столбец, который будет 100% табличного просмотра
      /// </summary>
      /// <param name="columnName">Имя столбца</param>
      /// <returns>Объект <see cref="EFPDataGridViewConfigColumn"/></returns>
      public EFPDataGridViewConfigColumn AddFill(string columnName)
      {
        return AddFill(columnName, 100);
      }

      /// <summary>
      /// Копирование списка настроек столбцов в другую конфигурацию
      /// Создаются копии объектов ColumnConfig
      /// </summary>
      /// <param name="other">Заполняемый список</param>
      internal void CopyTo(ColumnCollection other)
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
    /// Столбцы, входящие в просмотр
    /// </summary>
    public ColumnCollection Columns { get { return _Columns; } }
    private readonly ColumnCollection _Columns;

    /// <summary>
    /// Количество замороженных столбцов в левой части просмотра (из списка <see cref="Columns"/>)
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
    /// Имя столбца, который становится активным при открытии просмотра.
    /// Пустая строка - текущий столбец сохраняется в секции конфигурации категории <see cref="EFPConfigCategories.GridView"/> между сеансами работы программы
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

    #endregion

    #region Всплывающие подсказки

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
    /// Реализация свойства <see cref="ToolTips"/>
    /// </summary>
    [Serializable]
    public sealed class ToolTipCollection : NamedList<EFPDataGridViewConfigToolTip>
    {
      #region Конструктор

      internal ToolTipCollection(EFPDataGridViewConfig owner)
      {
        _Owner = owner;
      }

      private EFPDataGridViewConfig _Owner;

      #endregion

      #region Методы

      /// <summary>
      /// Добавляет подсказку в список.
      /// </summary>
      /// <param name="toolTipName">Имя подсказки для хранения в секции конфигурации.</param>
      /// <returns>Созданный объект <see cref="EFPDataGridViewConfigToolTip"/></returns>
      public EFPDataGridViewConfigToolTip Add(string toolTipName)
      {
        CheckNotReadOnly();

        EFPDataGridViewConfigToolTip item = new EFPDataGridViewConfigToolTip(toolTipName);
        Add(item);
        return item;
      }

      /// <summary>
      /// Копирование списка настроек столбцов в другую конфигурацию
      /// Создаются копии объектов
      /// </summary>
      /// <param name="other"></param>
      internal void CopyTo(ToolTipCollection other)
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
    /// Имена частей всплывающих подсказок для строки
    /// </summary>
    public ToolTipCollection ToolTips { get { return _ToolTips; } }
    private readonly ToolTipCollection _ToolTips;

    #endregion

    #region Прочие свойства

    /// <summary>
    /// Значок настройки, используемый в диалоге настройки табличного просмотра.
    /// Используется при выборе готовых настроек из выпадающего списка (для настройки по умолчанию и именных настроек, заданных в <see cref="EFPGridProducer"/>).
    /// Если свойство не установлено (по умолчанию), используется значок "No".
    /// </summary>
    public string ImageKey
    {
      get { return _ImageKey; }
      set { _ImageKey = value; }
    }
    private string _ImageKey;

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
      string visibleCols = cfg.GetString("VisibleColumns");
      string[] a = visibleCols.Split(',');
      CfgPart cfg2 = cfg.GetChild("Columns", false); // может быть null
      for (int i = 0; i < a.Length; i++)
      {
        string columnName = a[i].Trim();
        if (String.IsNullOrEmpty(columnName))
          continue;
        EFPDataGridViewConfigColumn col = new EFPDataGridViewConfigColumn(columnName);
        if (cfg2 != null)
        {
          CfgPart cfg3 = cfg2.GetChild(columnName, false);
          if (cfg3 != null)
          {
            col.Width = cfg3.GetInt("Width");
            col.FillMode = cfg3.GetBool("FillMode");
            int x = cfg3.GetInt("FillWeight");
            if (x > 0)
              col.FillWeight = x;
          }
        }
        Columns.Add(col);
      }
      if (Columns.Count == 0)
        throw new InvalidOperationException("В конфигурации не задано ни одного столбца");

      FrozenColumns = cfg.GetInt("FrozenColumns");
      StartColumnName = cfg.GetString("StartColumn");

      CurrentCellToolTip = !cfg.GetBool("NoCurrentCellToolTip");
      string toolTipNames = cfg.GetString("ToolTips");
      ToolTips.Clear();
      if (!String.IsNullOrEmpty(toolTipNames))
      {
        a = toolTipNames.Split(',');
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
    /// Генерирует исключение при <see cref="IsReadOnly"/>=true.
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
    /// <returns>Новый объект конфигурации</returns>
    public EFPDataGridViewConfig Clone()
    {
      EFPDataGridViewConfig res = new EFPDataGridViewConfig();
      Columns.CopyTo(res.Columns);
      res.FrozenColumns = FrozenColumns;
      res.StartColumnName = StartColumnName;
      res.CurrentCellToolTip = CurrentCellToolTip;
      ToolTips.CopyTo(res.ToolTips);
      return res;
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    /// <summary>
    /// Создание копии конфигурации с учетом реального расположения столбцов в просмотре.
    /// Для создания точной копии объекта используйте перегрузку <see cref="Clone()"/> без аргументов.
    /// </summary>
    /// <param name="controlProvider">Провайдер табличного просмотра с добавленными столбцами. Не может быть null</param>
    /// <returns>Новый объект конфигурации</returns>
    public EFPDataGridViewConfig Clone(IEFPDataView controlProvider)
    {
      EFPDataGridViewConfig res = new EFPDataGridViewConfig();

      IEFPDataViewColumn[] realColInfos = ((IEFPDataView)controlProvider).VisibleColumns;

      for (int i = 0; i < realColInfos.Length; i++)
      {
        if (realColInfos[i].ColumnProducer == null) // чужой столбец
          continue;
        EFPDataGridViewConfigColumn col2 = new EFPDataGridViewConfigColumn(realColInfos[i].Name); // ??
        controlProvider.InitColumnConfig(col2);

        if (res.Columns.Contains(col2.ColumnName))
          continue; // бяка - два одинаковых столбца
        res.Columns.Add(col2);
      }
      // Не учитываем количество замороженных столбцов в просмотре. Там могут быть
      // лишние столбцы. Берем количество из текущей настройки
      //Res.FrozenColumns = DocGridHandler.FrozenColumns;
      res.FrozenColumns = FrozenColumns;
      res.StartColumnName = StartColumnName;

      // Всплывающие подсказки просто копируются
      res.CurrentCellToolTip = CurrentCellToolTip;
      for (int i = 0; i < ToolTips.Count; i++)
        res.ToolTips.Add(ToolTips[i]);
      return res;
    }

    #endregion
  }

  /// <summary>
  /// Параметры ширины табличного просмотра в настройке <see cref="EFPDataGridViewConfig"/>
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
    /// Имя столбца (свойство <see cref="EFPDataGridViewColumn.Name"/>)
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
    /// Режим изменения размера столбца по размеру табличного просмотра на основании свойства <see cref="FillWeight"/>.
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
    /// Свойство используется при <see cref="FillMode"/>=true.
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
    /// <returns>Новый объект <see cref="EFPDataGridViewConfigColumn"/></returns>
    public EFPDataGridViewConfigColumn Clone()
    {
      EFPDataGridViewConfigColumn newCol = new EFPDataGridViewConfigColumn(ColumnName);
      newCol.Width = Width;
      newCol.FillMode = FillMode;
      newCol.FillWeight = FillWeight;
      return newCol;
    }

    object ICloneable.Clone()
    {
      return this.Clone();
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Возвращает true, если объект-владелец <see cref="EFPDataGridViewConfig"/> переведен в режим "Только чтение"
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// Генерирует исключение при <see cref="IsReadOnly"/>=true
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
  /// Настройка для одной всплывающей подсказки в <see cref="EFPDataGridViewConfig"/>.
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
    /// Создает копию объекта <see cref="EFPDataGridViewConfigToolTip"/>.
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
    /// Генерирует исключение, если <see cref="IsReadOnly"/>=true.
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
    /// Используются свойство <see cref="IEFPControl.ConfigManager"/> и <see cref="IEFPControl.ConfigHandler"/>.
    /// Свойство должно быть установлено до вызова <see cref="ShowDialog()"/>.
    /// </summary>
    public IEFPDataView CallerControlProvider
    {
      get { return _CallerControlProvider; }
      set { _CallerControlProvider = value; }
    }
    private IEFPDataView _CallerControlProvider;

    /// <summary>
    /// Источник настроек.
    /// Свойство должно быть установлено до вызова <see cref="ShowDialog()"/>.
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
    /// По умолчанию <see cref="EFPConfigCategories.GridConfig"/>.
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
    /// По умолчанию <see cref="EFPConfigCategories.GridConfigHistory"/>.
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
        Value = new EFPDataGridViewConfig(); // пустышка. 

      GridConfigForm form = new GridConfigForm(CallerControlProvider, ConfigCategory, HistoryCategory);
      form.Editor = GridProducer.CreateEditor(form.MainPanel, form.FormProvider, CallerControlProvider);

      form.FillSetItems();

      // Существующие данные 
      form.Editor.WriteFormValues(Value);
      TempCfg originalConfigSection = new TempCfg();
      Value.WriteConfig(originalConfigSection);
      form.SetComboBox.SelectedMD5Sum = originalConfigSection.MD5Sum(); // выбираем подходящий набор, если есть

      DialogResult res = EFPApp.ShowDialog(form, true);
      if (res == DialogResult.OK)
        Value = form.ResultConfig;
      return res;
    }

    #endregion
  }
}
