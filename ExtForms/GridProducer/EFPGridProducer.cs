// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Controls.TreeViewAdvNodeControls;
using System.Windows.Forms;
using System.Data;
using FreeLibSet.Core;
using FreeLibSet.Collections;
using FreeLibSet.Controls;
using FreeLibSet.Config;

namespace FreeLibSet.Forms
{
  #region IEFPGridProducer

  /// <summary>
  /// Обобщенный интерфейс генератора табличного просмотра.
  /// Не уточняет способа, каким инициализируются столбцы табличного просмотра.
  /// </summary>
  public interface IEFPGridProducer : IReadOnlyObject
  {
    /// <summary>
    /// Возвращает количество возможных вариантов сортировки для определения видимости команд меню.
    /// </summary>
    int OrderCount { get; }

    /// <summary>
    /// Метод должен создать столбцы в табличном просмотре в соответствии с установленной конфигурацией
    /// (свойство <see cref="EFPDataGridView.CurrentConfig"/>).
    /// На момент вызова табличный просмотр не содержит столбцов.
    /// После вызова этого метода следует вызвать <see cref="EFPDataGridView.PerformGridProducerPostInit()"/>.
    /// </summary>
    /// <param name="controlProvider">Заполняемый просмотр</param>
    /// <param name="reInit">true, если инициализация выполняется повторно</param>
    void InitGridView(EFPDataGridView controlProvider, bool reInit);

    /// <summary>
    /// Метод должен создать столбцы в иерархическом просмотре в соответствии с установленной конфигурацией
    /// (свойство <see cref="EFPDataTreeView.CurrentConfig"/>)
    /// На момент вызова табличный просмотр не содержит столбцов.
    /// После вызова этого метода следует вызвать <see cref="EFPDataTreeView.PerformGridProducerPostInit()"/>.
    /// </summary>
    /// <param name="controlProvider">Заполняемый просмотр</param>
    /// <param name="reInit">true, если инициализация выполняется повторно</param>
    void InitTreeView(EFPDataTreeView controlProvider, bool reInit);

    /// <summary>
    /// Дополнение к интерфейсу <see cref="IReadOnlyObject"/>.
    /// Вызывается при присоединении к табличному просмотру.
    /// </summary>
    void SetReadOnly();
  }

  #endregion

  #region IEFPGridProducerColumn

  /// <summary>
  /// Интерейс объекта, управляющего поведением столбца, созданного генератором табличного просмотра <see cref="IEFPConfigurableGridProducer"/>.
  /// Реализация <see cref="IEFPGridProducer"/> может не использовать этот интерфейс, или использовать его не для всех
  /// столбцов, или использовать один объект для нескольких столбцов.
  /// </summary>
  public interface IEFPGridProducerColumn
  {
    /// <summary>
    /// Выполнить редактирование для ячейки.
    /// Вызывается из <see cref="EFPDataGridView.PerformEditData(EFPDataGridViewState)"/>.
    /// Если метод выполнил действия, связанные с редактированием, следует вернуть true. В этом случае дальнейшая обработрка не выполняется
    /// Если следует выполнить обычные действия по редактированию, в частности, вызвать событие <see cref="EFPDataGridView.EditData"/>,
    /// следует вернуть false.
    /// </summary>
    /// <param name="rowInfo">Информация о текущей строки в табличном просмотре</param>
    /// <param name="columnName">Имя столбца</param>
    /// <returns>true, если обработка выполнена</returns>
    bool PerformCellEdit(EFPDataViewRowInfo rowInfo, string columnName);

    /// <summary>
    /// Методы вызывается при щелчке мыши на ячейке
    /// </summary>
    /// <param name="rowInfo">Информация о текущей строки в табличном просмотре</param>
    /// <param name="columnName">Имя столбца</param>
    void PerformCellClick(EFPDataViewRowInfo rowInfo, string columnName);
  }

  #endregion

  #region IEFPConfigurableGridProducer

  /// <summary>
  /// Интерфейс настраиваемого генератора табличного просмотра Реализуется классом <see cref="EFPGridProducer"/>.
  /// В отличие от <see cref="IEFPGridProducer"/>, предполагает возможность пользователя настраивать индивидуальные столбцы
  /// на уровне <see cref="EFPDataGridViewConfig"/>.
  /// Интерфейс используется на уровне <see cref="EFPConfigurableDataGridView"/> и <see cref="EFPConfigurableDataTreeView"/>, а не <see cref="EFPDataGridView"/>.
  /// </summary>
  public interface IEFPConfigurableGridProducer : IEFPGridProducer
  {
    /// <summary>
    /// Инициализация редактора настройки столбцов табличного просмотра.
    /// Метод должен добавить управляющие элементы в форму редактора и вернуть интерфейс управления.
    /// Загружать начальные значения в редактор не следует.
    /// </summary>
    /// <param name="parentControl">Панель в окне настройки формы для размещения элементов редактора</param>
    /// <param name="baseProvider">Базовый провайдер редактора настроек</param>
    /// <param name="callerControlProvider">Провайдер настраиваемого табличного просмотра</param>
    /// <returns>Интерфейс объекта редактора</returns>
    IEFPGridProducerEditor CreateEditor(Control parentControl, EFPBaseProvider baseProvider, IEFPDataView callerControlProvider);
  }

  #endregion

  /// <summary>
  /// Реализация <see cref="IEFPConfigurableGridProducer"/>.
  /// Описание всех возможных полей и настроек по умолчанию для
  /// табличного просмотра <see cref="EFPDataGridView"/> или иерархического просмотра <see cref="EFPDataTreeView"/>. 
  /// Класс не является потокобезопасным. Он должен вызываться только из основного потока приложения <see cref="EFPApp.MainThread"/>.
  /// </summary>
  public partial class EFPGridProducer : IEFPConfigurableGridProducer
  {
    #region Константы

    /// <summary>
    /// Имя конфигурации по умолчанию для отображения в списках
    /// </summary>
    public const string DefaultConfigDisplayName = "< По умолчанию >";

    #endregion

    #region Конструктор

    /// <summary>
    /// Создает пустой объект
    /// </summary>
    public EFPGridProducer()
    {
      _OutItem = new Reporting.BRGridProducerMenuOutItem();
    }

#if DEBUG
    private bool _InsideInitViewFlag;
#endif

    #endregion

    #region Основные коллекции

    /// <summary>
    /// Полный список столбцов, которые могут быть отображены в табличном просмотре
    /// </summary>
    public EFPGridProducerColumns Columns
    {
      get
      {
        if (_Columns == null)
        {
          _Columns = CreateColumns();
        }
        return _Columns;
      }
    }
    private EFPGridProducerColumns _Columns;

    /// <summary>
    /// Производный класс может создать объект производного класса для списка столбцов
    /// </summary>
    /// <returns>Коллекция</returns>
    protected virtual EFPGridProducerColumns CreateColumns() { return new EFPGridProducerColumns(this); }

    /// <summary>
    /// Возвращает количество столбцов, которые добавлены в коллекцию Columns
    /// </summary>
    public int ColumnCount
    {
      get
      {
        if (_Columns == null)
          return 0;
        else
          return _Columns.Count;
      }
    }

    /// <summary>
    /// Список строк всплывающих подсказок, которые могут быть отображены для строки табличного просмотра
    /// </summary>
    public EFPGridProducerToolTips ToolTips
    {
      get
      {
        if (_ToolTips == null)
          _ToolTips = CreateToolTips();
        return _ToolTips;
      }
    }
    private EFPGridProducerToolTips _ToolTips;

    /// <summary>
    /// Производный класс может создать объект производного класса для списка подсказок
    /// </summary>
    /// <returns>Коллекция</returns>
    protected virtual EFPGridProducerToolTips CreateToolTips() { return new EFPGridProducerToolTips(); }

    /// <summary>
    /// Возвращает количество всплывающих подсказок, добавленных в коллекцию <see cref="ToolTips"/>
    /// </summary>
    public int ToolTipCount
    {
      get
      {
        if (_ToolTips == null)
          return 0;
        else
          return _ToolTips.Count;
      }
    }

    /// <summary>
    /// Список возможных порядков сортировки табличного просмотра
    /// </summary>
    public EFPDataViewOrders Orders
    {
      get
      {
        if (_Orders == null)
          _Orders = CreateOrders();
        return _Orders;
      }
    }
    private EFPDataViewOrders _Orders;

    /// <summary>
    /// Производный класс может создать объект производного класса для списка порядков сортировки
    /// </summary>
    /// <returns>Коллекция</returns>
    protected virtual EFPDataViewOrders CreateOrders() { return new EFPDataViewOrders(); }

    /// <summary>
    /// Количество записей в списке <see cref="Orders"/>
    /// </summary>
    public int OrderCount
    {
      get
      {
        if (_Orders == null)
          return 0;
        else
          return _Orders.Count;
      }
    }

    /// <summary>
    /// Список имен "обязательных" полей, которые всегда добавляются в список usedColumns
    /// при вызове <see cref="InitGridView(EFPConfigurableDataGridView, bool, EFPDataGridViewConfig, IList{string})"/> и 
    /// <see cref="InitTreeView(EFPConfigurableDataTreeView, bool, EFPDataGridViewConfig, IList{string})"/>.
    /// По умолчанию - список пуст.
    /// </summary>
    public IList<string> FixedColumns
    {
      get
      {
        if (_FixedColumns == null)
          _FixedColumns = new SingleScopeList<string>();
        return _FixedColumns;
      }
    }
    private SingleScopeList<string> _FixedColumns;

    /// <summary>
    /// Вовзаращает список имен "обязательных" полей <see cref="FixedColumns"/>, которые всегда добавляются в список, в виде массива
    /// </summary>
    /// <returns>Массив имен</returns>
    public string[] GetFixedColumnArray()
    {
      if (_FixedColumns == null)
        return DataTools.EmptyStrings;
      else
        return _FixedColumns.ToArray();
    }

    #endregion

    #region Конфигурации

    /// <summary>
    /// Конфигурация табличного просмотра по умолчанию.
    /// Если конфигурация не была задана в явном виде, она создается автоматически при
    /// каждом обращении к свойству.
    /// </summary>
    public EFPDataGridViewConfig DefaultConfig
    {
      get
      {
        if (_DefaultConfig == null)
          return MakeDefaultConfig();
        else
          return _DefaultConfig;
      }
      set
      {
        _DefaultConfig = value;
      }
    }

    private EFPDataGridViewConfig _DefaultConfig;

    /// <summary>
    /// Именованные секции конфигурации
    /// </summary>
    private Dictionary<string, EFPDataGridViewConfig> _NamedConfigs;

    #endregion

    #region Инициализация табличного просмотра

    /// <summary>
    /// Инициализация табличного просмотра
    /// </summary>
    /// <param name="controlProvider">Обработчик табличного просмотра</param>
    /// <param name="reInit">При первом показе табличного просмотра получает значение false.
    /// При повторных вызовах, когда табличный просмотр уже был инициализирован, получает значение true</param>
    public void InitGridView(EFPDataGridView controlProvider, bool reInit)
    {
      if (!(controlProvider is EFPConfigurableDataGridView))
        throw new ArgumentException("Ожидался EFPConfigurableDataGridView", "controlProvider");
      List<string> dummyColumns = new List<string>();
      InitGridView((EFPConfigurableDataGridView)controlProvider, reInit, controlProvider.CurrentConfig, dummyColumns);
    }


    /// <summary>
    /// Инициализация табличного просмотра
    /// </summary>
    /// <param name="controlProvider">Обработчик табличного просмотра</param>
    /// <param name="reInit"></param>
    /// <param name="config">Конфигурация или null для использования конфигурации по умолчанию</param>
    /// <param name="usedColumns">Сюда добавляются имена полей, которые должны быть в наборе данных</param>
    public void InitGridView(EFPConfigurableDataGridView controlProvider, bool reInit, EFPDataGridViewConfig config, IList<string> usedColumns)
    {
#if DEBUG
      if (_InsideInitViewFlag)
        throw new ReenteranceException();
      _InsideInitViewFlag = true;
      try
      {
#endif
        DoInitGridView(controlProvider, reInit, config, usedColumns);
#if DEBUG
      }
      finally
      {
        _InsideInitViewFlag = false;
      }
#endif
    }
    private void DoInitGridView(EFPConfigurableDataGridView controlProvider, bool reInit, EFPDataGridViewConfig config, IList<string> usedColumns)
    {
      if (controlProvider == null)
        throw new ArgumentNullException("controlProvider");
      if (reInit)
      {
        if (controlProvider.GridProducer != this)
          throw new InvalidOperationException("Запрошена повторная инициализация, но свойство EFPDataGridView.GridProducer не установлено или установлено неверно");
      }
      if (usedColumns == null)
        throw new ArgumentNullException("usedColumns");
      //usedColumns.CheckNotReadOnly();

#if XXX // ????????????????
      if (Config == null && (!String.IsNullOrEmpty(ControlProvider.CurrentConfigName)))
      {
        try
        {
          Config = ControlProvider.ReadGridConfig(ControlProvider.CurrentConfigName);

          if (Config == null)
            EFPApp.MessageBox("Настройка просмотра \"" + ControlProvider.CurrentConfigName + "\" была удалена. Загружается настройка по умолчанию");
        }
        catch (Exception e)
        {
          EFPApp.MessageBox("Ошибка при загрузке настройки \"" + ControlProvider.CurrentConfigName + "\": " +
            e.Message + ". Будет использована настройка по умолчанию");
          Config = null;
        }
      }
#endif

      if (config == null)
      {
        if (String.IsNullOrEmpty(controlProvider.DefaultConfigName))
          config = DefaultConfig;
        else
        {
          config = GetNamedConfig(controlProvider.DefaultConfigName);
          if (config == null)
            throw new BugException("Не найдена именная конфигурация \"" + controlProvider.DefaultConfigName +
              "\". Неправильное значение свойства EFPAccDepGrid.DefaultConfigName");
        }
        // TODO: ????? ControlProvider.CurrentConfigName = String.Empty;
      }


      // предотвращаем Stack overflow
      if (config != controlProvider.CurrentConfig)
      {
        controlProvider.CurrentConfig = config;
        //ControlProvider.GridProducer = this;
      }

      #region Фиксированные поля

      if (_FixedColumns != null)
      {
        foreach (string fixedName in FixedColumns)
          usedColumns.Add(fixedName);
      }

      #endregion

      #region Добавление столбцов в просмотр

      int maxTextRowHeight = 1;
      for (int i = 0; i < config.Columns.Count; i++)
      {
        string columnName = config.Columns[i].ColumnName;
        EFPGridProducerColumn colDef = Columns[columnName];
        if (colDef == null)
          // Нет в с списке доступных столбцов
          continue;
        EFPDataGridViewColumn column = colDef.CreateGridColumn(controlProvider);
        colDef.ApplyConfig(column.GridColumn, config.Columns[i], controlProvider);

        // Запоминаем поля, которые нужны
        colDef.GetColumnNames(usedColumns);

        maxTextRowHeight = Math.Max(maxTextRowHeight, colDef.TextRowHeight);
      }

      if (config.FrozenColumns > 0 &&
        config.FrozenColumns < config.Columns.Count &&
        config.FrozenColumns < controlProvider.Columns.Count) // 22.02.2023
        controlProvider.Control.Columns[config.FrozenColumns - 1].Frozen = true;

      controlProvider.TextRowHeight = maxTextRowHeight;

      if (!String.IsNullOrEmpty(config.StartColumnName))
      {
        int startColumnIndex = controlProvider.Columns.IndexOf(config.StartColumnName);
        //else
        //  // Активируем первый столбец с автоинкрементом
        //  16.05.2018
        //  Не надо. Активация нужного столбца перенесена в EFPDataGridView
        //  StartColumnIndex = ControlProvider.FirstIncSearchColumnIndex;

        controlProvider.CurrentColumnIndex = startColumnIndex;
        controlProvider.SaveCurrentColumnAllowed = false; // чтобы не восстанавливался столбец из сохраненной секции конфигурации
      }
      else
        controlProvider.SaveCurrentColumnAllowed = true;

      // 16.07.2021
      // При добавлении порядков сортировки больше не будем использовать поля, которые нужны только для всплывающих подсказок
      SingleScopeList<string> usedColumnsForOrders = new SingleScopeList<string>(usedColumns);

      #endregion

      #region Всплывающие подсказки

      for (int i = 0; i < config.ToolTips.Count; i++)
      {
        EFPGridProducerToolTip item = ToolTips[config.ToolTips[i].ToolTipName];
        if (item == null)
          continue;
        item.GetColumnNames(usedColumns);
      }

      #endregion

      if (!reInit)
        controlProvider.GetCellAttributes += EFPDataGridView_GetCellAttributes;

      #region Доступные порядки сортировки

      if (controlProvider.UseGridProducerOrders)
      {
        controlProvider.Orders.Clear();
        if (this.OrderCount > 0)
        {
          for (int i = 0; i < Orders.Count; i++)
          {
            // Добавляем только объекты сортировки, для которых существуют столбцы просмотра или которые объявлены фиксированными
            if (Orders[i].AreAllColumnsPresented(usedColumnsForOrders)) // есть все необходимые поля ?
              controlProvider.Orders.Add(Orders[i]);
          }
        }
      } // ControlProvider.UseGridProducerOrders

      #endregion

      // Параметры печати
      if ((!reInit) && controlProvider.DefaultOutItem != null)
        OutItem.SettingsData.CopyTo(controlProvider.DefaultOutItem.SettingsData);
    }

    private void EFPDataGridView_GetCellAttributes(object sender, EFPDataGridViewCellAttributesEventArgs args)
    {
      EFPDataGridView controlProvider = (EFPDataGridView)sender;

      EFPGridProducerColumn colDef = args.Column.ColumnProducer as EFPGridProducerColumn;
      if (colDef == null)
        return;

      colDef.OnGetCellAttributes(args);

      switch (args.Reason)
      {
        case EFPDataGridViewAttributesReason.View:
        case EFPDataGridViewAttributesReason.Print:
          DoGetValue(controlProvider, args, colDef);
          break;
        case EFPDataGridViewAttributesReason.ToolTip:
          try
          {
            DoGetToolTipText(controlProvider, args, colDef);
          }
          catch (Exception e) // 21.05.2021
          {
            args.ToolTipText = "Ошибка при получении подсказки: " + e.Message;
          }
          break;
      }
    }

    private void DoGetValue(EFPDataGridView controlProvider, EFPDataGridViewCellAttributesEventArgs args, EFPGridProducerColumn colDef)
    {
      if (args.Value != null)
        return; // уже определено

      DataRow sourceRow = controlProvider.GetDataRow(args.RowIndex);

      try
      {
        EFPDataViewRowInfo rowInfo = controlProvider.GetRowInfo(args.RowIndex);
        args.Value = colDef.GetValue(rowInfo);
        controlProvider.FreeRowInfo(rowInfo);
      }
      catch
      {
        args.Value = null;
      }
    }

    private void DoGetToolTipText(EFPDataGridView controlProvider, EFPDataGridViewCellAttributesEventArgs args, EFPGridProducerColumn colDef)
    {
#if DEBUG
      if (controlProvider.CurrentConfig == null)
        throw new NullReferenceException("Не задано свойство EFPDataGridView.CurrentConfig");
#endif

      if (!controlProvider.CurrentConfig.CurrentCellToolTip)
        args.ToolTipText = String.Empty;
      else
      {
        EFPDataViewRowInfo rowInfo = controlProvider.GetRowInfo(args.RowIndex);
        string s2 = colDef.GetCellToolTipText(rowInfo, args.ColumnName);
        controlProvider.FreeRowInfo(rowInfo);
        if (s2.Length > 0)
          args.ToolTipText = s2;
      }

      if (controlProvider.CurrentConfig.ToolTips.Count == 0)
        return;

      List<string> lst2 = new List<string>();
      for (int i = 0; i < controlProvider.CurrentConfig.ToolTips.Count; i++)
      {
        EFPGridProducerToolTip toolTip = this.ToolTips[controlProvider.CurrentConfig.ToolTips[i].ToolTipName];
        if (toolTip == null)
          continue; // ерунда какая-то

        // Если в подсказки входит столбец, на который наведена мышь, то пропускаем подсказку
        List<string> lst1 = new List<string>();
        toolTip.GetColumnNames(lst1);
        if (lst1.Contains(args.ColumnName))
          continue;

        string s;
        try
        {
          EFPDataViewRowInfo rowInfo = controlProvider.GetRowInfo(args.RowIndex);
          s = toolTip.GetToolTipText(rowInfo);
          controlProvider.FreeRowInfo(rowInfo);
        }
        catch (Exception e)
        {
          s = toolTip.DisplayName + ": Ошибка! " + e.Message;
        }
        if (!String.IsNullOrEmpty(s))
          lst2.Add(s);
      }
      if (lst2.Count > 0)
      {
        lst2.Insert(0, new string('-', 32));
        if (!String.IsNullOrEmpty(args.ToolTipText))
          lst2.Insert(0, args.ToolTipText);

        args.ToolTipText = String.Join(Environment.NewLine, lst2.ToArray());
      }
    }

    #endregion

    #region Инициализация древовидного просмотра

    /// <summary>
    /// Инициализация иерархического просмотра
    /// </summary>
    /// <param name="controlProvider">Обработчик просмотра</param>
    /// <param name="reInit">При первом показе табличного просмотра получает значение false.
    /// При повторных вызовах, когда табличный просмотр уже был инициализирован, получает значение true</param>
    public void InitTreeView(EFPDataTreeView controlProvider, bool reInit)
    {
      if (!(controlProvider is EFPConfigurableDataTreeView))
        throw new ArgumentException("Ожидался EFPConfigurableDataTreeView", "controlProvider");
      List<string> dummyColumns = new List<string>();
      InitTreeView((EFPConfigurableDataTreeView)controlProvider, reInit, controlProvider.CurrentConfig, dummyColumns);
    }

    /// <summary>
    /// Инициализация иерархического просмотра
    /// </summary>
    /// <param name="controlProvider">Обработчик просмотра</param>
    /// <param name="reInit"></param>
    /// <param name="config">Конфигурация или null для использования конфигурации по умолчанию</param>
    /// <param name="usedColumns">Сюда добавляются имена полей, которые должны быть в наборе данных</param>
    public void InitTreeView(EFPConfigurableDataTreeView controlProvider, bool reInit, EFPDataGridViewConfig config, IList<string> usedColumns)
    {
#if DEBUG
      if (_InsideInitViewFlag)
        throw new ReenteranceException();
      _InsideInitViewFlag = true;
      try
      {
#endif
        DoInitTreeView(controlProvider, reInit, config, usedColumns);
#if DEBUG
      }
      finally
      {
        _InsideInitViewFlag = false;
      }
#endif
    }
    private void DoInitTreeView(EFPConfigurableDataTreeView controlProvider, bool reInit, EFPDataGridViewConfig config, IList<string> usedColumns)
    {
      if (controlProvider == null)
        throw new ArgumentNullException("controlProvider");
      if (reInit)
      {
        if (controlProvider.GridProducer != this)
          throw new InvalidOperationException("Запрошена повторная инициализация, но свойство EFPDataGridView.GridProducer не установлено или установлено неверно");
      }
      if (usedColumns == null)
        throw new ArgumentNullException("usedColumns");
      //usedColumns.CheckNotReadOnly();

#if XXX //???????????????
      if (config == null && (!String.IsNullOrEmpty(controlProvider.CurrentConfigName)))
      {
        try
        {
          config = controlProvider.ReadGridConfig(controlProvider.CurrentConfigName);

          if (config == null)
            EFPApp.MessageBox("Настройка просмотра \"" + controlProvider.CurrentConfigName + "\" была удалена. Загружается настройка по умолчанию");
        }
        catch (Exception e)
        {
          EFPApp.MessageBox("Ошибка при загрузке настройки \"" + controlProvider.CurrentConfigName + "\": " +
            e.Message + ". Будет использована настройка по умолчанию");
          config = null;
        }
      }
#endif

      if (config == null)
      {
        if (String.IsNullOrEmpty(controlProvider.DefaultConfigName))
          config = this.DefaultConfig;
        else
        {
          config = this.GetNamedConfig(controlProvider.DefaultConfigName);
          if (config == null)
            throw new BugException("Не найдена именная конфигурация \"" + controlProvider.DefaultConfigName +
              "\". Неправильное значение свойства EFPAccDepGrid.DefaultConfigName");
        }
        // TODO: ????? controlProvider.CurrentConfigName = String.Empty;
      }


      // предотвращаем Stack overflow
      if (config != controlProvider.CurrentConfig)
      {
        controlProvider.CurrentConfig = config;
        //controlProvider.GridProducer = this;
      }

      controlProvider.Control.UseColumns = true;

      if (_FixedColumns != null)
      {
        foreach (string fixedName in FixedColumns)
          usedColumns.Add(fixedName);
      }

      int maxTextRowHeight = 1;
      for (int i = 0; i < config.Columns.Count; i++)
      {
        string columnName = config.Columns[i].ColumnName;
        EFPGridProducerColumn colDef = this.Columns[columnName];
        if (colDef == null)
          // Нет в с списке доступных столбцов
          continue;

        EFPDataTreeViewColumn column = colDef.CreateTreeColumn(controlProvider);
        colDef.ApplyConfig(column, config.Columns[i], controlProvider);


        // Запоминаем поля, которые нужны
        colDef.GetColumnNames(usedColumns);

        maxTextRowHeight = Math.Max(maxTextRowHeight, colDef.TextRowHeight);
      }

      //ControlProvider.TextRowHeight = MaxTextRowHeight;

      /*
      for (int i = 0; i < Config.ToolTips.Count; i++)
      {
        GridProducerToolTip Item = ToolTips[Config.ToolTips[i].ToolTipName];
        if (Item == null)
          continue;
        Item.GetColumnNames(UsedColumns);
      }
       */

      // Для отображения всплывающих подсказок и получения значений нужен отдельный 
      // объект, который будет хранить только выбранные части
      // подсказки и содержать обработчик
      if (!reInit)
      {              /*
        GridProducerHandler tth = new GridProducerHandler(this, ControlProvider);

        ControlProvider.Control.CellValueNeeded += new DataGridViewCellValueEventHandler(tth.CellValueNeeded);
        ControlProvider.Control.VirtualMode = true;*/
      }

      // Попытаемся сохранить старый порядок сортировки
      string orgOrderDisplayName = String.Empty;
      if (reInit && controlProvider.CurrentOrder != null)
        orgOrderDisplayName = controlProvider.CurrentOrder.DisplayName;

      if (controlProvider.OrderCount > 0)
        controlProvider.Orders.Clear();
      else
        controlProvider.DisableOrdering(); // запрет щелкать по заголовкам
#if XXX
      int NewOrderIndex = -1;
      if (Orders.Count > 0)
      {
        for (int i = 0; i < Orders.Count; i++)
        {
          // Добавляем только объекты сортировки, для которых существуют поля
          // в массиве Fields
          if (UsedColumns.Contains(Orders[i].RequiredColumns)) // есть все необходимые поля ?
          {
            ControlProvider.Orders.Add(Orders[i].ColumnNames, Orders[i].DisplayName, Orders[i].SortInfo);
            if (Orders[i].DisplayName == OrgOrderDisplayName)
              NewOrderIndex = ControlProvider.Orders.Count - 1;
          }
        }
        ControlProvider.AutoSort = ControlProvider.Orders.Count > 0;
        if (ReInit)
        {
          if (NewOrderIndex >= 0)
            ControlProvider.CurrentOrderIndex = NewOrderIndex;
          else
          {
            if (ControlProvider.OrderCount > 0)
              ControlProvider.CurrentOrderIndex = 0;
          }
        }
      }
      if (ReInit)
        ControlProvider.CommandItems.RefreshOrderItems();
#endif

      // Параметры печати
      if ((!reInit) && controlProvider.DefaultOutItem != null)
        OutItem.SettingsData.CopyTo(controlProvider.DefaultOutItem.SettingsData);
    }

    #endregion

    #region Работа с настройками просмотра

    /// <summary>
    /// Создает новый объект для <see cref="DefaultConfig"/> и опционально заполняет его всеми
    /// имеющимися столбцами <see cref="Columns"/> и подсказками <see cref="ToolTips"/>'.
    /// Добавляются только столбцы и подсказки, которые существуют на момент вызова; затем могут быть добавлены столбцы и подсказки, которые не входят в конфигурацию по умолчанию.
    /// </summary>
    /// <param name="addAll">Если true, то будут заполнены списки <see cref="EFPDataGridViewConfig.Columns"/> и <see cref="EFPDataGridViewConfig.ToolTips"/>.
    /// Если false, то списки останутся пустыми</param>
    public void NewDefaultConfig(bool addAll)
    {
      DefaultConfig = new EFPDataGridViewConfig();
      if (addAll)
        DefaultConfig = CreateDefaultConfig();
      else
        DefaultConfig = new EFPDataGridViewConfig();
    }


    /// <summary>
    /// Создает новый объект конфигурации и заполняет списки <see cref="EFPDataGridViewConfig.Columns"/> и <see cref="EFPDataGridViewConfig.ToolTips"/>
    /// вcеми существующими столбцами <see cref="Columns"/> и подсказками <see cref="ToolTips"/>.
    /// </summary>
    /// <returns>Новая конфигурация</returns>
    public EFPDataGridViewConfig CreateDefaultConfig()
    {
      EFPDataGridViewConfig config = new EFPDataGridViewConfig();
      for (int i = 0; i < Columns.Count; i++)
        config.Columns.Add(Columns[i].Name);
      for (int i = 0; i < ToolTips.Count; i++)
        config.ToolTips.Add(ToolTips[i].Name);
      return config;
    }

    /// <summary>
    /// Создать настройку с фиксированным именем.
    /// Столбцы и подсказки не добавляются.
    /// </summary>
    /// <param name="fixedName">Имя настройки</param>
    /// <returns>Пустая конфигурация, которую нужно заполнить</returns>
    public EFPDataGridViewConfig NewNamedConfig(string fixedName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(fixedName))
        throw new ArgumentNullException(fixedName);
#endif

      if (_NamedConfigs == null)
        _NamedConfigs = new Dictionary<string, EFPDataGridViewConfig>();
      EFPDataGridViewConfig config = new EFPDataGridViewConfig();
      _NamedConfigs.Add(fixedName, config);
      return config;
    }

    /// <summary>
    /// Получить настройку с фиксированным именем.
    /// Если настройка не была создана, генерируется исключение.
    /// </summary>
    /// <param name="fixedName">Имя настройки</param>
    /// <returns></returns>
    public EFPDataGridViewConfig GetNamedConfig(string fixedName)
    {
#if DEBUG
      if (String.IsNullOrEmpty(fixedName))
        throw new ArgumentNullException(fixedName);
#endif
      if (_NamedConfigs == null)
        return null;

      EFPDataGridViewConfig config;
      if (_NamedConfigs.TryGetValue(fixedName, out config))
        return config;
      else
        throw new ArgumentException("Фиксированная настройка табличного просмотра с именем \"" + fixedName +
          "\" не была объявлена в генераторе табличного просмотра", "fixedName");
    }

    /// <summary>
    /// Возвращает имена настроек, добавленных с помощью <see cref="NewNamedConfig(string)"/>.
    /// </summary>
    /// <returns>Список имен</returns>
    public string[] GetNamedConfigNames()
    {
      if (_NamedConfigs == null)
        return DataTools.EmptyStrings;
      else
      {
        string[] a = new string[_NamedConfigs.Count];
        _NamedConfigs.Keys.CopyTo(a, 0);
        return a;
      }
    }

    /// <summary>
    /// Загрузить сохраненную ранее конфигурацию с заданным именем. Если имя не задано,
    /// то возвращается <see cref="DefaultConfig"/>. Если имя неправильное, или настройка была удалена,
    /// то возвращается null.
    /// </summary>
    /// <param name="configSectionName">Имя секции конфигурации, используемое табличным просмотром</param>
    /// <param name="defaulConfigName">Имя фиксированной настройки или пустая строка, если используется настройка по умолчанию</param>
    /// <param name="cfgName">Имя сохраненной секции</param>
    /// <returns></returns>
    public EFPDataGridViewConfig LoadConfig(string configSectionName, string defaulConfigName, string cfgName)
    {
      if (String.IsNullOrEmpty(cfgName))
      {
        if (String.IsNullOrEmpty(defaulConfigName))
          return DefaultConfig;
        else
          return GetNamedConfig(defaulConfigName);
      }
      else
      {
        throw new NotImplementedException();
        //TODO: return GridHandlerConfigs.GetConfig(ConfigSectionName, CfgName);
      }
    }

    /// <summary>
    /// Загрузить сохраненную ранее конфигурацию, определив имя текущей конфигурации. 
    /// Если имя не было сохранено или неправильное, или настройка была удалена,
    /// то возвращается <see cref="DefaultConfig"/>.
    /// </summary>
    /// <param name="configSectionName">Имя секции конфигурации, используемое табличным просмотром</param>
    /// <param name="defaultConfigName">Имя фиксированной настройки или пустая строка, если используется настройка по умолчанию</param>
    /// <returns>Загруженная секция или <see cref="DefaultConfig"/></returns>
    public EFPDataGridViewConfig LoadConfig(string configSectionName, string defaultConfigName)
    {
      string cfgName = GetCurrentConfigName(configSectionName);
      EFPDataGridViewConfig config = LoadConfig(configSectionName, defaultConfigName, cfgName);
      if (config == null)
        config = DefaultConfig;
      return config;
    }

    /// <summary>
    /// Определить имя текущей конфигурации, которая должна использоваться просмотром
    /// </summary>
    /// <param name="configSectionName"></param>
    /// <returns></returns>
    public static string GetCurrentConfigName(string configSectionName)
    {
      return String.Empty;
      // TODO:
      /*
      if (String.IsNullOrEmpty(ConfigSectionName))
        throw new ArgumentNullException("ConfigSectionName");
      ConfigSection Sect = AccDepClientExec.ConfigSections[ConfigSectionName, "Просмотр"];
      return Sect.GetString("Настройка");
       * */
    }

    /// <summary>
    /// Получить список полей, необходимых для заданной конфигурации
    /// </summary>
    /// <param name="config">Конфигурация. Если null, то возвращается <see cref="FixedColumns"/>.</param>
    /// <param name="usedColumns">Сюда добавляются имена полей</param>
    public void GetColumnNames(EFPDataGridViewConfig config, IList<string> usedColumns)
    {
      if (usedColumns == null)
        throw new ArgumentNullException();
      //usedColumns.CheckNotReadOnly();

      if (_FixedColumns != null)
      {
        foreach (string fixedName in FixedColumns)
          usedColumns.Add(fixedName);
      }
      if (config != null)
      {
        // Столбцы
        for (int i = 0; i < config.Columns.Count; i++)
        {
          string columnName = config.Columns[i].ColumnName;
          EFPGridProducerColumn colDef = Columns[columnName];
          if (colDef == null)
            // Нет в с списке доступных столбцов
            continue;
          // Запоминаем поля, которые нужны
          colDef.GetColumnNames(usedColumns);
        }

        // Всплывающие подсказки
        for (int i = 0; i < config.ToolTips.Count; i++)
        {
          EFPGridProducerToolTip item = ToolTips[config.ToolTips[i].ToolTipName];
          if (item == null)
            continue;
          item.GetColumnNames(usedColumns);
        }
      }
    }

    /// <summary>
    /// Получить список полей, необходимых для табличного просмотра, при использовании
    /// конфигурации, запомненной пользователем
    /// </summary>
    /// <param name="configSectioName">Имя секции конфигурации для табличного просмотра</param>
    /// <param name="defaultConfigName">Имя фиксированной настройки или пустая строка, если используется настройка по умолчанию</param>
    /// <param name="usedColumns">Сюда добавляются имена полей</param>
    public void GetColumnNames(string configSectioName, string defaultConfigName, IList<string> usedColumns)
    {
      GetColumnNames(LoadConfig(configSectioName, defaultConfigName), usedColumns);
    }

    private EFPDataGridViewConfig MakeDefaultConfig()
    {
      EFPDataGridViewConfig res = new EFPDataGridViewConfig();
      for (int i = 0; i < Columns.Count; i++)
      {
        res.Columns.Add(new EFPDataGridViewConfigColumn(Columns[i].Name));
        // Если определен только один столбец - делаем его с заполнением
        if (Columns.Count == 1)
          res.Columns[0].FillMode = true;
      }

      for (int i = 0; i < ToolTips.Count; i++)
        res.ToolTips.Add(ToolTips[i].Name);

      res.SetReadOnly();
      return res;
    }

    #endregion

    #region Редактор

    /// <summary>
    /// Инициализация редактора настройки столбцов табличного просмотра.
    /// Метод должен добавить управляющие элементы в форму редактора и вернуть интерфейс управления.
    /// Загружать начальные значения в редактор не следует
    /// </summary>
    /// <param name="parentControl">Панель в окне настройки формы для размещения элементов редактора</param>
    /// <param name="baseProvider">Базовый провайдер редактора настроек</param>
    /// <param name="callerControlProvider">Провайдер настраиваемого табличного просмотра</param>
    /// <returns>Интерфейс объекта редактора</returns>
    public IEFPGridProducerEditor CreateEditor(Control parentControl, EFPBaseProvider baseProvider, IEFPDataView callerControlProvider)
    {
      EFPGridProducerEditor editor = new EFPGridProducerEditor(this, callerControlProvider, baseProvider);
      parentControl.Controls.Add(editor.TheTabControl);
      return editor;
    }

    #endregion

    #region IReadOnlyObject

    /// <summary>
    /// Возвращает true, если <see cref="EFPGridProducer"/> был переведен в режим "Только чтение".
    /// Переводится при первом присоединении к просмотру <see cref="EFPConfigurableDataGridView"/>, <see cref="EFPConfigurableDataTreeView"/> или по окончании инициализации объекта DBUI в ExtDBDocForms.dll
    /// </summary>
    public bool IsReadOnly
    {
      get
      {
        if (_Columns == null)
          return false;
        else
          return _Columns.IsReadOnly;
      }
    }

    /// <summary>
    /// Генерирует исключение, если <see cref="EFPGridProducer"/> был переведен в режим "Только чтение".
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    /// <summary>
    /// Перевод <see cref="EFPGridProducer"/> в режим "только чтение".
    /// При первом вызове выполняется проверка корректности данных.
    /// Повторные вызовы игнорируются.
    /// </summary>
    public void SetReadOnly()
    {
      if (IsReadOnly)
        return;

      Columns.SetReadOnly();
      ToolTips.SetReadOnly();
      Orders.SetReadOnly();

      // Остальное переводить неохота

      Validate();
    }

    #endregion

    #region Проверка корректности данных

    /// <summary>
    /// Проверка корректности данных.
    /// Вызывается однократно при первом вызове <see cref="SetReadOnly()"/>
    /// </summary>
    protected virtual void Validate()
    {
      // Полный список имен полей, которые должны быть в базе данных
      SingleScopeStringList srcColumnNames = new SingleScopeStringList(true); // без учета регистра
      foreach (EFPGridProducerColumn col in Columns)
      {
        ValidateItemBase(col);
        col.GetColumnNames(srcColumnNames);
      }
      foreach (EFPGridProducerToolTip tt in ToolTips)
      {
        ValidateItemBase(tt);
        tt.GetColumnNames(srcColumnNames);
      }

      SingleScopeStringList calcColumnNames = new SingleScopeStringList(true); // для проверки порядков сортировки

      // проверяем, что они не совпадают с именами вычисляемых полей
      foreach (EFPGridProducerColumn col in Columns)
      {
        if (col.SourceColumnNames != null)
        {
          if (srcColumnNames.Contains(col.Name))
            throw new EFPGridProducerValidationException("Неправильное имя вычисляемого столбца \"" + col.Name + "\", так как это имя есть в списке исходных столбцов в других объектах EFPGridProducer");
          calcColumnNames.Add(col.Name);
        }
      }
      foreach (EFPGridProducerToolTip tt in ToolTips)
      {
        if (tt.SourceColumnNames != null)
        {
          if (srcColumnNames.Contains(tt.Name))
            throw new EFPGridProducerValidationException("Неправильное имя вычисляемой всплывающей подсказки \"" + tt.Name + "\", так как это имя есть в списке исходных столбцов в других объектах EFPGridProducer");

          // имена вычисляемых подсказок не интересны для порядка сортировки
        }
      }

      List<string> orderColumnNames = new List<string>();
      foreach (EFPDataViewOrder order in Orders)
      {
        orderColumnNames.Clear();
        order.GetColumnNames(orderColumnNames);
        for (int i = 0; i < orderColumnNames.Count; i++)
        {
          if (!calcColumnNames.Contains(orderColumnNames[i])) // сортировка по вычисляемому столбцу
          {
            // сортировка по реальному столбцу из базы данных

            string errorText;
            if (!IsValidSourceColumnName(orderColumnNames[i], out errorText))
              throw new EFPGridProducerValidationException("Неправильное имя исходного столбца \"" + calcColumnNames + "\", используемого в порядке сортировки \"" + order.Name + "\". " + errorText);
          }
        }
      }
    }

    private void ValidateItemBase(EFPGridProducerItemBase item)
    {
      string errorText;
      if (item.SourceColumnNames == null)
      {
        if (!IsValidSourceColumnName(item.Name, out errorText))
          throw new EFPGridProducerValidationException("Неправильное имя столбца \"" + item.Name + "\". " + errorText);
      }
      else
      {
        for (int i = 0; i < item.SourceColumnNames.Length; i++)
        {
          if (!IsValidSourceColumnName(item.SourceColumnNames[i], out errorText))
            throw new EFPGridProducerValidationException("Неправильное имя исходного столбца \"" + item.SourceColumnNames[i] + "\" в вычисляемом столбце/подсказке \"" + item.Name + "\". " + errorText);
        }
      }
    }

    /// <summary>
    /// Проверка корректности имени исходного столбца.
    /// </summary>
    /// <param name="columnName">Имя столбца</param>
    /// <param name="errorText">Сюда должно быть помещено сообщение об ошибке</param>
    /// <returns>true, если имя является допустимым</returns>
    public virtual bool IsValidSourceColumnName(string columnName, out string errorText)
    {
      if (String.IsNullOrEmpty(columnName))
      {
        errorText = "Имя не задано";
        return false;
      }

      if (columnName.IndexOf(',') >= 0)
      {
        errorText = "Имя не может содержать запятые";
        return false;
      }

      if (columnName.IndexOf(' ') >= 0)
      {
        errorText = "Имя не может содержать пробелы";
        return false;
      }

      if (columnName[0] == '.' || columnName[columnName.Length - 1] == '.')
      {
        errorText = "Имя не может начинаться или заканичиваться точкой";
        return false;
      }

      if (columnName.IndexOf("..", StringComparison.Ordinal) >= 0)
      {
        errorText = "Имя не может содержать 2 точки подряд";
        return false;
      }

      errorText = null;
      return true;
    }


    #endregion

    #region Параметры печати

    /// <summary>
    /// Параметры печати для столбцов табличного просмотра
    /// </summary>
    public Reporting.BRGridProducerMenuOutItem OutItem { get { return _OutItem; } }
    private readonly Reporting.BRGridProducerMenuOutItem _OutItem;

    #endregion
  }

  /// <summary>
  /// Исключение, возникающее при переводе объекта <see cref="EFPGridProducer"/> в режим "только чтение", в процессе проверки корректности данных
  /// </summary>
  [Serializable]
  public class EFPGridProducerValidationException : ApplicationException
  {
    #region Конструктор

    /// <summary>
    /// Создает объект исключения с заданным сообщением
    /// </summary>
    /// <param name="message">Текст сообщения</param>
    public EFPGridProducerValidationException(string message)
      : this(message, null)
    {
    }


    /// <summary>
    /// Создает объект исключения с заданным сообщением и вложенным исключением
    /// </summary>
    /// <param name="message">Текст сообщения</param>
    /// <param name="innerException">Вложенное исключение</param>
    public EFPGridProducerValidationException(string message, Exception innerException)
      : base(message, innerException)
    {
    }

    /// <summary>
    /// Эта версия конструктора нужна для правильной десериализации
    /// </summary>
    protected EFPGridProducerValidationException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
      : base(info, context)
    {
    }

    #endregion
  }
}

namespace FreeLibSet.Forms.Reporting
{
  /// <summary>
  /// Реализация свойства <see cref="EFPGridProducer.OutItem"/>
  /// </summary>
  public sealed class BRGridProducerMenuOutItem
  {
    #region Защищенный конструктор

    internal BRGridProducerMenuOutItem()
    {
      _SettingsData = new SettingsDataList();
      _SettingsData.Add(new FreeLibSet.Reporting.BRPageSettingsDataItem());
      _SettingsData.Add(new FreeLibSet.Reporting.BRFontSettingsDataItem());
      _SettingsData.Add(new Reporting.BRDataViewSettingsDataItem());
    }

    #endregion

    /// <summary>
    /// Список данных для печати просмотра с возможностью именных настроек.
    /// Содержит данные параметров страницы, шрифта, столбцов и настроек просмотра.
    /// При инициализации <see cref="EFPDataGridView"/> и <see cref="EFPDataTreeView"/> настройки копируются в набор печати по умолчанию
    /// </summary>
    public SettingsDataList SettingsData { get { return _SettingsData; } }
    private readonly SettingsDataList _SettingsData;

    /// <summary>
    /// Доступ к настройкам печати просмотра по умолчанию
    /// </summary>
    public BRDataViewMenuOutSettings Default { get { return new Reporting.BRDataViewMenuOutSettings(SettingsData); } }

    /// <summary>
    /// Доступ к именным настройкам печати просмотра.
    /// </summary>
    /// <param name="defCfgCode">Код для именной настройки</param>
    /// <returns>Объект для доступа к настройкам</returns>
    public BRDataViewMenuOutSettings this[string defCfgCode]
    {
      get
      {
        if (String.IsNullOrEmpty(defCfgCode))
          return Default;
        else
          return new BRDataViewMenuOutSettings(SettingsData.DefaultConfigs[defCfgCode]);
      }
    }

    /// <summary>
    /// Добавление именной настройки печати просмотра
    /// </summary>
    /// <param name="defCfgCode">Код для именной настройки</param>
    /// <param name="displayName">Отображаемое название настройки</param>
    /// <returns>Объект для доступа к настройкам</returns>
    public BRDataViewMenuOutSettings Add(string defCfgCode, string displayName)
    {
      if (String.IsNullOrEmpty(defCfgCode))
        throw new ArgumentNullException("defCfgCode");
      SettingsData.DefaultConfigs[defCfgCode].DisplayName = displayName;
      return new Reporting.BRDataViewMenuOutSettings(SettingsData.DefaultConfigs[defCfgCode]);
    }
  }
}
