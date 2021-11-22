// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Windows.Forms;
using System.Data;
using System.Drawing;
using FreeLibSet.Data;
using FreeLibSet.DBF;
using FreeLibSet.Data.Docs;
using FreeLibSet.Formatting;
using FreeLibSet.Controls.TreeViewAdvNodeControls;
using FreeLibSet.Calendar;
using FreeLibSet.Core;

namespace FreeLibSet.Forms.Docs
{
  /// <summary>
  /// Расширенный класс продюсера табличного просмотра EFPDBxGridView и EFPDBxTreeView.
  /// Содержит расширенные коллекции Columns, ToolTips и Orders, поддерживающие методы для добавления 
  /// вычисляемых ссылочных полей и подсказок, и порядков сортировки, основанных на вычисляемых выражениях.
  /// </summary>
  public class EFPDBxGridProducer : EFPGridProducer
  {
    #region Конструктор

    /// <summary>
    /// Создает объект GridProducer
    /// </summary>
    /// <param name="ui">Объект пользовательского интерфейса</param>
    public EFPDBxGridProducer(DBUI ui)
    {
      if (ui == null)
        throw new ArgumentNullException("ui");

      _UI = ui;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Объект пользовательского интерфейса
    /// </summary>
    public DBUI UI { get { return _UI; } }
    private DBUI _UI;

    #endregion

    #region Переопределенные коллекции

    /// <summary>
    /// Полный список столбцов, которые могут быть отображены в табличном просмотре
    /// </summary>
    public new EFPDBxGridProducerColumns Columns { get { return (EFPDBxGridProducerColumns)(base.Columns); } }

    /// <summary>
    /// Создает объект коллекции
    /// </summary>
    /// <returns>Новый объект</returns>
    protected override EFPGridProducerColumns CreateColumns()
    {
      return new EFPDBxGridProducerColumns(this);
    }

    /// <summary>
    /// Список строк всплывающих подсказок, которые могут быть отображены для строки табличного просмотра
    /// </summary>
    public new EFPDBxGridProducerToolTips ToolTips { get { return (EFPDBxGridProducerToolTips)(base.ToolTips); } }

    /// <summary>
    /// Создает объект коллекции
    /// </summary>
    /// <returns>Новый объект</returns>
    protected override EFPGridProducerToolTips CreateToolTips()
    {
      return new EFPDBxGridProducerToolTips(this);
    }

    /// <summary>
    /// Список возможных порядков сортировки табличного просмотра
    /// </summary>
    public new EFPDBxViewOrders Orders { get { return (EFPDBxViewOrders)(base.Orders); } }

    /// <summary>
    /// Создает объект коллекции
    /// </summary>
    /// <returns>Новый объект</returns>
    protected override EFPDataViewOrders CreateOrders()
    {
      return new EFPDBxViewOrders();
    }

    #endregion

    #region Получение значений полей

#if XXX
    /// <summary>
    /// Присоединить объект к DBxCache.
    /// После этого, метод OnGetColumnValue() будет возвращать значение из кэша.
    /// Метод может вызываться только сразу после конструктора
    /// </summary>
    public void SetCache(DBxCache cache, string tableName)
    {
      if (cache == null)
        throw new ArgumentNullException("cache");
      if (String.IsNullOrEmpty(tableName))
        throw new ArgumentNullException("tableName");
      if (_Cache != null)
        throw new InvalidOperationException("Повторный вызов метода");

      _Cache = cache;
      _TableName = tableName;
    }

    /// <summary>
    /// Система кэширования таблиц.
    /// Устанавливается методом SetCache(), иначе содержит null.
    /// </summary>
    public DBxCache Cache { get { return _Cache; } }
    private DBxCache _Cache;

    /// <summary>
    /// Имя таблицы в системе кэширования таблиц.
    /// Устанавливается методом SetCache(), иначе содержит null.
    /// </summary>
    public string TableName { get { return _TableName; } }
    private string _TableName;

    /// <summary>
    /// Получение значения для отсутствующего в строке поля.
    /// Если метод не переопределен, то возвращает значение из кэша по заданному идентификатору
    /// (поле Id).
    /// Иначе возвращает null.
    /// </summary>
    /// <param name="row">Строка данных</param>
    /// <param name="columnName">Имя поля, которого нет в строке данных</param>
    /// <returns>Значение поля</returns>
    internal protected virtual object OnGetColumnValue(DataRow row, string columnName)
    {
      if (_Cache == null)
        return null;
      else
        return _Cache[_TableName].GetValue(DataTools.GetInt(row, "Id"), columnName);
    }
#endif

    #endregion

    #region Внутренний класс DocRefInfo

    internal class DocRefInfo
    {
      #region Конструкторы

      /// <summary>
      /// Для RefDocTextColumn и RefDocImageColumn
      /// </summary>
      /// <param name="refColumnName"></param>
      /// <param name="masterUI"></param>
      public DocRefInfo(string refColumnName, DocTypeUIBase masterUI)
      {
        if (String.IsNullOrEmpty(refColumnName))
          throw new ArgumentNullException("refColumnName");
        if (masterUI == null)
          throw new ArgumentNullException("masterUI");
        _RefColumnName = refColumnName;
        _MasterUI = masterUI;
      }

      /// <summary>
      /// Для RefXXXColumn
      /// </summary>
      /// <param name="refColumnName"></param>
      /// <param name="masterUI"></param>
      /// <param name="valueColumnName"></param>
      public DocRefInfo(string refColumnName, DocTypeUIBase masterUI, string valueColumnName)
        : this(refColumnName, masterUI)
      {
        if (String.IsNullOrEmpty(valueColumnName))
          throw new ArgumentNullException("valueColumnName");
        string columnName1;
        int p = valueColumnName.IndexOf('.');
        if (p >= 0)
          columnName1 = valueColumnName.Substring(0, p);
        else
          columnName1 = valueColumnName;
        if (!masterUI.DocTypeBase.Struct.Columns.Contains(columnName1))
          throw new ArgumentException("Описание структуры " + (masterUI.DocTypeBase.IsSubDoc ? "поддокумента" : "документа") +
            " \"" + masterUI.DocTypeBase.Name + "\" не содержит поля \"" + columnName1 + "\"", "valueColumnName");

        _ValueColumnName = valueColumnName;
      }

      /// <summary>
      /// Для RefXXXRangeColumn
      /// </summary>
      /// <param name="refColumnName"></param>
      /// <param name="masterUI"></param>
      /// <param name="valueColumnName"></param>
      /// <param name="valueColumnName2"></param>
      public DocRefInfo(string refColumnName, DocTypeUIBase masterUI, string valueColumnName, string valueColumnName2)
        : this(refColumnName, masterUI, valueColumnName)
      {
        if (String.IsNullOrEmpty(valueColumnName2))
          throw new ArgumentNullException("valueColumnName2");
        string columnName2;
        int p = valueColumnName2.IndexOf('.');
        if (p >= 0)
          columnName2 = valueColumnName2.Substring(0, p);
        else
          columnName2 = valueColumnName2;
        if (!masterUI.DocTypeBase.Struct.Columns.Contains(columnName2))
          throw new ArgumentException("Описание структуры " + (masterUI.DocTypeBase.IsSubDoc ? "поддокумента" : "документа") +
            " \"" + masterUI.DocTypeBase.Name + "\" не содержит поля \"" + columnName2 + "\"", "valueColumnName2");

        _ValueColumnName2 = valueColumnName2;
      }

      #endregion

      #region Поля

      public DocTypeUIBase MasterUI { get { return _MasterUI; } }
      private DocTypeUIBase _MasterUI;

      public string RefColumnName { get { return _RefColumnName; } }
      private string _RefColumnName;

      public string ValueColumnName { get { return _ValueColumnName; } }
      private string _ValueColumnName;

      public string ValueColumnName2 { get { return _ValueColumnName2; } }
      private string _ValueColumnName2;

      /// <summary>
      /// Возвращает имя результирующего столбца
      /// </summary>
      public string ResName { get { return _RefColumnName + "." + _ValueColumnName; } }

      #endregion

      #region Получение ссылочных значений

      public void RefDocTextColumn_ValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args)
      {
        Int32 Id = args.GetInt(_RefColumnName);
        args.Value = _MasterUI.GetTextValue(Id);
      }

      public void RefDocImageColumn_ValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args)
      {
        Int32 Id = args.GetInt(_RefColumnName);
        switch (args.Reason)
        {
          case EFPGridProducerValueReason.Value:
            args.Value = _MasterUI.GetImageValue(Id);
            break;
          case EFPGridProducerValueReason.ToolTipText:
            //args.ToolTipText = _MasterUI.GetToolTipText(Id);
            args.ToolTipText = _MasterUI.GetTextValue(Id);
            break;
        }
      }

      public void RefValueColumn_ValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args)
      {
        Int32 Id = args.GetInt(_RefColumnName);
        args.Value = _MasterUI.TableCache.GetValue(Id, _ValueColumnName);
      }

      public object GetRefValue(EFPGridProducerValueNeededEventArgs args)
      {
        Int32 Id = args.GetInt(_RefColumnName);
        return _MasterUI.TableCache.GetValue(Id, _ValueColumnName);
      }

      #endregion

      #region Вычисление значений для столбцов дат

      #region DateRange

      internal void DateRangeColumn_LongValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args)
      {
        DateRangeColumn_ValueNeeded(sender, args, true);
      }

      internal void DateRangeColumn_ShortValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args)
      {
        DateRangeColumn_ValueNeeded(sender, args, false);
      }

      private void DateRangeColumn_ValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args, bool longFormat)
      {
        Int32 Id = args.GetInt(0);
        DateTime? FirstDate = _MasterUI.TableCache.GetNullableDateTime(Id, _ValueColumnName);
        DateTime? LastDate = _MasterUI.TableCache.GetNullableDateTime(Id, _ValueColumnName2);
        if (FirstDate.HasValue || LastDate.HasValue)
          args.Value = DateRangeFormatter.Default.ToString(FirstDate, LastDate, longFormat);
      }

      #endregion

      #region MonthDay

      internal void MonthDayColumn_LongValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args)
      {
        MonthDayColumn_ValueNeeded(sender, args, true);
      }

      internal void MonthDayColumn_ShortValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args)
      {
        MonthDayColumn_ValueNeeded(sender, args, false);
      }

      private void MonthDayColumn_ValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args, bool longFormat)
      {
        Int32 Id = args.GetInt(0);
        int v = _MasterUI.TableCache.GetInt(Id, _ValueColumnName);
        if (v == 0)
          return;
        else if (v < 1 || v > 365)
          args.Value = "?? " + v.ToString();
        else
        {
          MonthDay md = new MonthDay(v);
          args.Value = DateRangeFormatter.Default.ToString(md, longFormat);
        }
      }

      #endregion

      #region MonthDayRange

      internal void MonthDayRangeColumn_LongValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args)
      {
        MonthDayRangeColumn_ValueNeeded(sender, args, true);
      }

      internal void MonthDayRangeColumn_ShortValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args)
      {
        MonthDayRangeColumn_ValueNeeded(sender, args, false);
      }

      private void MonthDayRangeColumn_ValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args, bool longFormat)
      {
        Int32 Id = args.GetInt(0);
        int v1 = _MasterUI.TableCache.GetInt(Id, _ValueColumnName);
        int v2 = _MasterUI.TableCache.GetInt(Id, _ValueColumnName2);
        if (v1 == 0 && v2 == 0)
          return;
        else if (v1 < 1 || v1 > 365 || v2 < 1 || v2 > 365)
          args.Value = "?? " + v1.ToString() + "-" + v2.ToString();
        else
        {
          MonthDayRange r = new MonthDayRange(new MonthDay(v1), new MonthDay(v2));
          args.Value = DateRangeFormatter.Default.ToString(r, longFormat);
        }
      }

      #endregion

      #endregion
    }

    #endregion
  }

  /// <summary>
  /// Реализация свойства EFPDBxGridProducer.Columns.
  /// Поддерживает методы добавления ссылочных столбцов
  /// </summary>
  public class EFPDBxGridProducerColumns : EFPGridProducerColumns
  {
    #region Конструктор

    /// <summary>
    /// Создает коллекцию
    /// </summary>
    /// <param name="gridProducer">Объект-владелец</param>
    public EFPDBxGridProducerColumns(EFPDBxGridProducer gridProducer)
    {
      if (gridProducer == null)
        throw new ArgumentNullException("gridProducer");
      _GridProducer = gridProducer;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Объект-владелец
    /// </summary>
    public EFPDBxGridProducer GridProducer { get { return _GridProducer; } }
    private EFPDBxGridProducer _GridProducer;

    #endregion

    #region Методы добавления столбцов

    #region Текстовое представление документа

    /// <summary>
    /// Добавить текстовое поле для отображения ссылки на документ или поддокумент. Поле будет
    /// содержать значение, возвращаемое DocTypeUI/SubDocTypeUI.GetTextValue(), если ссылочное поле непустое. 
    /// Столбец будет иметь имя "RefColumnName_Text"
    /// </summary>
    /// <param name="refColumnName">Имя ссылочного поля</param>
    /// <param name="masterUI">Интерфейс документа или поддокумента, на который ссылается поле</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="textWidth">Ширина столбца в символах</param>
    /// <param name="minTextWidth">Минимальная ширина столбца в символах</param>
    /// <returns>Объект EFPGridProducerColumn</returns>
    public EFPGridProducerColumn AddRefDocText(string refColumnName, DocTypeUIBase masterUI,
      string headerText, int textWidth, int minTextWidth)
    {
      EFPDBxGridProducer.DocRefInfo dri = new EFPDBxGridProducer.DocRefInfo(refColumnName, masterUI);

      return AddUserText(refColumnName + "_Text", refColumnName, new EFPGridProducerValueNeededEventHandler(dri.RefDocTextColumn_ValueNeeded),
        headerText, textWidth, minTextWidth);
    }

    /// <summary>
    /// Добавить текстовое поле для отображения ссылки на документ произвольного типа,
    /// когда в таблице данных есть поле с кодом таблицы и поле с идентификатором документа. 
    /// Текстовое поле будет содержать значение, возвращаемое DocTypeUI.GetTextValue(), если ссылочное поле непустое. 
    /// Столбец будет иметь имя "RefColumnName_Text"
    /// </summary>
    /// <param name="tableIdColumnName">Имя числового столбца, содержащего идентификатор таблицы документа</param>
    /// <param name="refColumnName">Имя числового столбца, содержащего идентификатор документа</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="textWidth">Ширина столбца в символах</param>
    /// <param name="minTextWidth">Минимальная ширина столбца в символах</param>
    /// <returns>Объект EFPGridProducerColumn</returns>
    public EFPGridProducerColumn AddVTRefDocText(string tableIdColumnName, string refColumnName,
      string headerText, int textWidth, int minTextWidth)
    {
      if (String.IsNullOrEmpty(tableIdColumnName))
        throw new ArgumentNullException("tableIdColumnName");
      if (String.IsNullOrEmpty(refColumnName))
        throw new ArgumentNullException("refColumnName");
      if (String.Compare(tableIdColumnName, refColumnName, StringComparison.OrdinalIgnoreCase) == 0)
        throw new ArgumentException("Имена исходных столбцов не могут совпадать");
      return AddUserText(refColumnName + "_Text", tableIdColumnName + "," + refColumnName, new EFPGridProducerValueNeededEventHandler(VTRefDocTextColumn_ValueNeeded),
        headerText, textWidth, minTextWidth);
    }

    private void VTRefDocTextColumn_ValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args)
    {
      Int32 TableId = args.GetInt(0);
      Int32 DocId = args.GetInt(1);
      args.Value = _GridProducer.UI.TextHandlers.GetTextValue(TableId, DocId);
    }

    #endregion

    #region Значок документа

    /// <summary>
    /// Добавить изображение для отображения ссылочного поля. Поле будет
    /// содержать значок, если ссылочное поле непустое. Если связанный документ или поддокумент
    /// имеет ошибки или предупреждение, то отображается соответстввующий значок
    /// Столбец будет иметь имя "RefColumnName_Image"
    /// Имя столбца может быть "Id" для отображения значка текущего документа
    /// </summary>
    /// <param name="refColumnName">Имя ссылочного поля или "Id"</param>
    /// <param name="masterUI">Интерфейс документа или поддокумента, на который ссылается поле</param>
    /// <param name="headerText">Заголовок</param>
    /// <returns>Объект GridProducerColumn</returns>
    public EFPGridProducerImageColumn AddRefDocImage(string refColumnName, DocTypeUIBase masterUI,
      string headerText)
    {
      EFPDBxGridProducer.DocRefInfo dri = new EFPDBxGridProducer.DocRefInfo(refColumnName, masterUI);

      return AddUserImage(refColumnName + "_Image", refColumnName, new EFPGridProducerValueNeededEventHandler(dri.RefDocImageColumn_ValueNeeded),
        headerText);
    }

    /// <summary>
    /// Добавить изображение для отображения ссылочного поля. Поле будет
    /// содержать значок, если ссылочное поле непустое. Если связанный документ или поддокумент
    /// имеет ошибки или предупреждение, то отображается соответстввующий значок
    /// Столбец будет иметь имя "RefColumnName_Image"
    /// Имя столбца может быть "Id" для отображения значка текущего документа
    /// </summary>
    /// <param name="refColumnName">Имя ссылочного поля или "Id"</param>
    /// <param name="masterUI">Интерфейс документа или поддокумента, на который ссылается поле</param>
    /// <returns>Объект GridProducerColumn</returns>
    public EFPGridProducerImageColumn AddRefDocImage(string refColumnName, DocTypeUIBase masterUI)
    {
      return AddRefDocImage(refColumnName, masterUI, String.Empty);
    }

    /// <summary>
    /// Добавить два столбца: текст и изображение, для отображения ссылочного поля. 
    /// Комбинация вызовов AddRefDocText() и AddRefDocImage().
    /// Столбцы будут иметь имена "RefColumnName_Text" и "RefColumnName_Image"
    /// </summary>
    /// <param name="refColumnName">Имя ссылочного поля или "Id"</param>
    /// <param name="masterUI">Интерфейс документа или поддокумента, на который ссылается поле</param>
    /// <param name="headerText">Заголовок</param>
    /// <param name="textWidth">Ширина текстового столбца в символах</param>
    /// <param name="minTextWidth">Минимальная ширина текстового столбца в символах</param>
    public void AddRefDocTextAndImage(string refColumnName, DocTypeUIBase masterUI,
      string headerText, int textWidth, int minTextWidth)
    {
      AddRefDocText(refColumnName, masterUI, headerText, textWidth, minTextWidth).DisplayName = headerText + " (текст)";
      AddRefDocImage(refColumnName, masterUI).DisplayName = headerText + " (значок)";
    }

    /// <summary>
    /// Добавить поле значка для отображения ссылки на документ произвольного типа,
    /// когда в таблице данных есть поле с кодом таблицы и поле с идентификатором документа. 
    /// Поле будет содержать изображение, возвращаемое DocTypeUI.ImageHandlers.GetImageValue(), если ссылочное поле непустое. 
    /// Столбец будет иметь имя "RefColumnName_Image"
    /// </summary>
    /// <param name="tableIdColumnName">Имя числового столбца, содержащего идентификатор таблицы документа</param>
    /// <param name="refColumnName">Имя числового столбца, содержащего идентификатор документа</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <returns>Объект EFPGridProducerColumn</returns>
    public EFPGridProducerImageColumn AddVTRefDocImage(string tableIdColumnName, string refColumnName,
      string headerText)
    {
      if (String.IsNullOrEmpty(tableIdColumnName))
        throw new ArgumentNullException("tableIdColumnName");
      if (String.IsNullOrEmpty(refColumnName))
        throw new ArgumentNullException("refColumnName");
      if (String.Compare(tableIdColumnName, refColumnName, StringComparison.OrdinalIgnoreCase) == 0)
        throw new ArgumentException("Имена исходных столбцов не могут совпадать");
      return AddUserImage(refColumnName + "_Image", tableIdColumnName + "," + refColumnName, new EFPGridProducerValueNeededEventHandler(VTRefDocImageColumn_ValueNeeded),
        headerText);
    }


    /// <summary>
    /// Добавить поле значка для отображения ссылки на документ произвольного типа,
    /// когда в таблице данных есть поле с кодом таблицы и поле с идентификатором документа. 
    /// Поле будет содержать изображение, возвращаемое DocTypeUI.ImageHandlers.GetImageValue(), если ссылочное поле непустое. 
    /// Столбец будет иметь имя "RefColumnName_Image"
    /// </summary>
    /// <param name="tableIdColumnName">Имя числового столбца, содержащего идентификатор таблицы документа</param>
    /// <param name="refColumnName">Имя числового столбца, содержащего идентификатор документа</param>
    /// <returns>Объект EFPGridProducerColumn</returns>
    public EFPGridProducerImageColumn AddVTRefDocImage(string tableIdColumnName, string refColumnName)
    {
      return AddVTRefDocImage(tableIdColumnName, refColumnName, String.Empty);
    }

    private void VTRefDocImageColumn_ValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args)
    {
      Int32 TableId = args.GetInt(0);
      Int32 DocId = args.GetInt(1);
      switch (args.Reason)
      {
        case EFPGridProducerValueReason.Value:
          string ImageKey = _GridProducer.UI.ImageHandlers.GetImageKey(TableId, DocId);
          args.Value = EFPApp.MainImages.Images[ImageKey];
          break;
        case EFPGridProducerValueReason.ToolTipText:
          //args.ToolTipText = _GridProducer.UI.ImageHandlers.GetToolTipText(TableId, DocId);
          args.ToolTipText = _GridProducer.UI.TextHandlers.GetTextValue(TableId, DocId);
          break;
      }
    }

    #endregion

    #region Ссылочные поля

    #region Текстовые столбцы

    /// <summary>
    /// Добавляет текстовый столбец для ссылочного поля.
    /// Для получения значения, сначала извлекается идентификатор документа или поддокумента из
    /// ссылочного поля с именем <paramref name="refColumnName"/>.
    /// Далее вызывается метод DBxTableCache.GetValue() для получения значения. Методу передается идентификатор
    /// и имя поля <paramref name="valueColumnName"/>.
    /// Добавляемый столбец будет иметь имя "<paramref name="refColumnName"/>.<paramref name="valueColumnName"/>".
    /// </summary>
    /// <param name="refColumnName">Имя числового столбца, содержащего идентификатор документа или поддокумента.
    /// Это поле должно присутствовать в таблице.</param>
    /// <param name="masterUI">Интерфейс документа или поддокумента, на который ссылается поле</param>
    /// <param name="valueColumnName">Имя поля в структуре документа/поддокумента, которое требуется выводить.
    /// Имя поля может содержать точку, если само является ссылочным. 
    /// Это поле не относится к таблице, для которой создается просмотр. Оно передается методу DBxTableCache.GetValue().</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="textWidth">Ширина в текстовых единицах</param>
    /// <param name="minTextWidth">Минимальная ширина в текстовых единицах</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerColumn AddRefText(string refColumnName, DocTypeUIBase masterUI, string valueColumnName,
      string headerText, int textWidth, int minTextWidth)
    {
      EFPDBxGridProducer.DocRefInfo dri = new EFPDBxGridProducer.DocRefInfo(refColumnName, masterUI, valueColumnName);
      return AddUserText(dri.ResName, refColumnName,
        new EFPGridProducerValueNeededEventHandler(dri.RefValueColumn_ValueNeeded), headerText, textWidth, minTextWidth);
    }

    /// <summary>
    /// Добавить столбец для отображения числового поля.
    /// Значения должны быть целочисленными.
    /// Для получения значения, сначала извлекается идентификатор документа или поддокумента из
    /// ссылочного поля с именем <paramref name="refColumnName"/>.
    /// Далее вызывается метод DBxTableCache.GetValue() для получения значения. Методу передается идентификатор
    /// и имя поля <paramref name="valueColumnName"/>.
    /// Добавляемый столбец будет иметь имя "<paramref name="refColumnName"/>.<paramref name="valueColumnName"/>".
    /// </summary>
    /// <param name="refColumnName">Имя числового столбца, содержащего идентификатор документа или поддокумента.
    /// Это поле должно присутствовать в таблице.</param>
    /// <param name="masterUI">Интерфейс документа или поддокумента, на который ссылается поле</param>
    /// <param name="valueColumnName">Имя поля в структуре документа/поддокумента, которое требуется выводить.
    /// Имя поля может содержать точку, если само является ссылочным. 
    /// Это поле не относится к таблице, для которой создается просмотр. Оно передается методу DBxTableCache.GetValue().</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="textWidth">Ширина столбца в текстовых единицах</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerColumn AddRefInt(string refColumnName, DocTypeUIBase masterUI, string valueColumnName,
      string headerText, int textWidth)
    {
      EFPDBxGridProducer.DocRefInfo dri = new EFPDBxGridProducer.DocRefInfo(refColumnName, masterUI, valueColumnName);
      return AddUserInt(dri.ResName, refColumnName, dri.RefValueColumn_ValueNeeded, headerText, textWidth);
    }

    /// <summary>
    /// Добавить столбец для отображения дробных (или целых) чисел.
    /// Для получения значения, сначала извлекается идентификатор документа или поддокумента из
    /// ссылочного поля с именем <paramref name="refColumnName"/>.
    /// Далее вызывается метод DBxTableCache.GetValue() для получения значения. Методу передается идентификатор
    /// и имя поля <paramref name="valueColumnName"/>.
    /// Добавляемый столбец будет иметь имя "<paramref name="refColumnName"/>.<paramref name="valueColumnName"/>".
    /// </summary>
    /// <param name="refColumnName">Имя числового столбца, содержащего идентификатор документа или поддокумента.
    /// Это поле должно присутствовать в таблице.</param>
    /// <param name="masterUI">Интерфейс документа или поддокумента, на который ссылается поле</param>
    /// <param name="valueColumnName">Имя поля в структуре документа/поддокумента, которое требуется выводить.
    /// Имя поля может содержать точку, если само является ссылочным. 
    /// Это поле не относится к таблице, для которой создается просмотр. Оно передается методу DBxTableCache.GetValue().</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="textWidth">Ширина столбца в текстовых единицах</param>
    /// <param name="decimalPlaces">Количество знаков после десятичной точки. 0 - для отображения целых чисел</param>
    /// <param name="sizeGroup">Группа однотипных столбцов</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerColumn AddRefFixedPoint(string refColumnName, DocTypeUIBase masterUI, string valueColumnName,
      string headerText, int textWidth, int decimalPlaces, string sizeGroup)
    {
      EFPDBxGridProducer.DocRefInfo dri = new EFPDBxGridProducer.DocRefInfo(refColumnName, masterUI, valueColumnName);
      return AddUserFixedPoint(dri.ResName, refColumnName, dri.RefValueColumn_ValueNeeded, headerText, textWidth, decimalPlaces, sizeGroup);
    }

    /// <summary>
    /// Добавить столбец для отображения даты (без компонента времени).
    /// Для получения значения, сначала извлекается идентификатор документа или поддокумента из
    /// ссылочного поля с именем <paramref name="refColumnName"/>.
    /// Далее вызывается метод DBxTableCache.GetValue() для получения значения. Методу передается идентификатор
    /// и имя поля <paramref name="valueColumnName"/>.
    /// Добавляемый столбец будет иметь имя "<paramref name="refColumnName"/>.<paramref name="valueColumnName"/>".
    /// </summary>
    /// <param name="refColumnName">Имя числового столбца, содержащего идентификатор документа или поддокумента.
    /// Это поле должно присутствовать в таблице.</param>
    /// <param name="masterUI">Интерфейс документа или поддокумента, на который ссылается поле</param>
    /// <param name="valueColumnName">Имя поля в структуре документа/поддокумента, которое требуется выводить.
    /// Имя поля может содержать точку, если само является ссылочным. 
    /// Это поле не относится к таблице, для которой создается просмотр. Оно передается методу DBxTableCache.GetValue().</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerColumn AddRefDate(string refColumnName, DocTypeUIBase masterUI, string valueColumnName,
      string headerText)
    {
      EFPDBxGridProducer.DocRefInfo dri = new EFPDBxGridProducer.DocRefInfo(refColumnName, masterUI, valueColumnName);
      return AddUserDate(dri.ResName, refColumnName, dri.RefValueColumn_ValueNeeded, headerText);
    }

    /// <summary>
    /// Добавить столбец для отображения даты и времени.
    /// Для получения значения, сначала извлекается идентификатор документа или поддокумента из
    /// ссылочного поля с именем <paramref name="refColumnName"/>.
    /// Далее вызывается метод DBxTableCache.GetValue() для получения значения. Методу передается идентификатор
    /// и имя поля <paramref name="valueColumnName"/>.
    /// Добавляемый столбец будет иметь имя "<paramref name="refColumnName"/>.<paramref name="valueColumnName"/>".
    /// </summary>
    /// <param name="refColumnName">Имя числового столбца, содержащего идентификатор документа или поддокумента.
    /// Это поле должно присутствовать в таблице.</param>
    /// <param name="masterUI">Интерфейс документа или поддокумента, на который ссылается поле</param>
    /// <param name="valueColumnName">Имя поля в структуре документа/поддокумента, которое требуется выводить.
    /// Имя поля может содержать точку, если само является ссылочным. 
    /// Это поле не относится к таблице, для которой создается просмотр. Оно передается методу DBxTableCache.GetValue().</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerColumn AddRefDateTime(string refColumnName, DocTypeUIBase masterUI, string valueColumnName,
      string headerText)
    {
      EFPDBxGridProducer.DocRefInfo dri = new EFPDBxGridProducer.DocRefInfo(refColumnName, masterUI, valueColumnName);
      return AddUserDateTime(dri.ResName, refColumnName, dri.RefValueColumn_ValueNeeded, headerText);
    }

    /// <summary>
    /// Добавить столбец для отображения даты и/или времени.
    /// Для получения значения, сначала извлекается идентификатор документа или поддокумента из
    /// ссылочного поля с именем <paramref name="refColumnName"/>.
    /// Далее вызывается метод DBxTableCache.GetValue() для получения значения. Методу передается идентификатор
    /// и имя поля <paramref name="valueColumnName"/>.
    /// Добавляемый столбец будет иметь имя "<paramref name="refColumnName"/>.<paramref name="valueColumnName"/>".
    /// </summary>
    /// <param name="refColumnName">Имя числового столбца, содержащего идентификатор документа или поддокумента.
    /// Это поле должно присутствовать в таблице.</param>
    /// <param name="masterUI">Интерфейс документа или поддокумента, на который ссылается поле</param>
    /// <param name="valueColumnName">Имя поля в структуре документа/поддокумента, которое требуется выводить.
    /// Имя поля может содержать точку, если само является ссылочным. 
    /// Это поле не относится к таблице, для которой создается просмотр. Оно передается методу DBxTableCache.GetValue().</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="kind">Тип даты/времени</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerColumn AddRefDateTime(string refColumnName, DocTypeUIBase masterUI, string valueColumnName,
      string headerText, EditableDateTimeFormatterKind kind)
    {
      EFPDBxGridProducer.DocRefInfo dri = new EFPDBxGridProducer.DocRefInfo(refColumnName, masterUI, valueColumnName);
      return AddUserDateTime(dri.ResName, refColumnName, dri.RefValueColumn_ValueNeeded, headerText, kind);
    }

    /// <summary>
    /// Добавить столбец для отображения денежных сумм.
    /// Для получения значения, сначала извлекается идентификатор документа или поддокумента из
    /// ссылочного поля с именем <paramref name="refColumnName"/>.
    /// Далее вызывается метод DBxTableCache.GetValue() для получения значения. Методу передается идентификатор
    /// и имя поля <paramref name="valueColumnName"/>.
    /// Добавляемый столбец будет иметь имя "<paramref name="refColumnName"/>.<paramref name="valueColumnName"/>".
    /// </summary>
    /// <param name="refColumnName">Имя числового столбца, содержащего идентификатор документа или поддокумента.
    /// Это поле должно присутствовать в таблице.</param>
    /// <param name="masterUI">Интерфейс документа или поддокумента, на который ссылается поле</param>
    /// <param name="valueColumnName">Имя поля в структуре документа/поддокумента, которое требуется выводить.
    /// Имя поля может содержать точку, если само является ссылочным. 
    /// Это поле не относится к таблице, для которой создается просмотр. Оно передается методу DBxTableCache.GetValue().</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerColumn AddRefMoney(string refColumnName, DocTypeUIBase masterUI, string valueColumnName,
      string headerText)
    {
      return AddRefMoney(refColumnName, masterUI, valueColumnName, headerText, false);
    }

    /// <summary>
    /// Добавить столбец для отображения денежных сумм.
    /// Для получения значения, сначала извлекается идентификатор документа или поддокумента из
    /// ссылочного поля с именем <paramref name="refColumnName"/>.
    /// Далее вызывается метод DBxTableCache.GetValue() для получения значения. Методу передается идентификатор
    /// и имя поля <paramref name="valueColumnName"/>.
    /// Добавляемый столбец будет иметь имя "<paramref name="refColumnName"/>.<paramref name="valueColumnName"/>".
    /// </summary>
    /// <param name="refColumnName">Имя числового столбца, содержащего идентификатор документа или поддокумента.
    /// Это поле должно присутствовать в таблице.</param>
    /// <param name="masterUI">Интерфейс документа или поддокумента, на который ссылается поле</param>
    /// <param name="valueColumnName">Имя поля в структуре документа/поддокумента, которое требуется выводить.
    /// Имя поля может содержать точку, если само является ссылочным. 
    /// Это поле не относится к таблице, для которой создается просмотр. Оно передается методу DBxTableCache.GetValue().</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="showPlusSign">Если true, то для положительных числовых значений будет
    /// отображаться знак "+". 
    /// Может быть удобно для столбцов, содержащих разности</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerColumn AddRefMoney(string refColumnName, DocTypeUIBase masterUI, string valueColumnName,
      string headerText, bool showPlusSign)
    {
      EFPDBxGridProducer.DocRefInfo dri = new EFPDBxGridProducer.DocRefInfo(refColumnName, masterUI, valueColumnName);
      return AddUserMoney(dri.ResName, refColumnName, dri.RefValueColumn_ValueNeeded, headerText, showPlusSign);
    }

    #endregion

    #region Перечислимые значения

    /// <summary>
    /// Добавляет текстовый столбец для отображения перечислимого значения, которое
    /// вычисляется на основании числового поля.
    /// Перечислимые значения должны идти по порядку (0,1,2, ...).
    /// Для получения значения, сначала извлекается идентификатор документа или поддокумента из
    /// ссылочного поля с именем <paramref name="refColumnName"/>.
    /// Далее вызывается метод DBxTableCache.GetValue() для получения значения. Методу передается идентификатор
    /// и имя поля <paramref name="valueColumnName"/>.
    /// Добавляемый столбец будет иметь имя "<paramref name="refColumnName"/>.<paramref name="valueColumnName"/>_Text".
    /// </summary>
    /// <param name="refColumnName">Имя числового столбца, содержащего идентификатор документа или поддокумента.
    /// Это поле должно присутствовать в таблице.</param>
    /// <param name="masterUI">Интерфейс документа или поддокумента, на который ссылается поле</param>
    /// <param name="valueColumnName">Имя поля в структуре документа/поддокумента, которое требуется выводить.
    /// Имя поля может содержать точку, если само является ссылочным. 
    /// Это поле не относится к таблице, для которой создается просмотр. Оно передается методу DBxTableCache.GetValue().</param>
    /// <param name="textValues">Список текстовых значений, которые показываются в создаваемом
    /// столбце</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="textWidth">Ширина в текстовых единицах</param>
    /// <param name="minTextWidth">Минимальная ширина в текстовых единицах</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerEnumColumn AddRefEnumText(string refColumnName, DocTypeUIBase masterUI, string valueColumnName,
      string[] textValues,
      string headerText, int textWidth, int minTextWidth)
    {
      EFPDBxGridProducer.DocRefInfo dri = new EFPDBxGridProducer.DocRefInfo(refColumnName, masterUI, valueColumnName);
      EFPGridProducerEnumColumn Item = new EFPDBxGridProducerEnumColumn(dri, textValues);
      Item.HeaderText = headerText;
      Item.TextAlign = HorizontalAlignment.Left;
      Item.TextWidth = textWidth;
      Item.MinTextWidth = minTextWidth;

      Add(Item);
      return Item;
    }

    private class EFPDBxGridProducerEnumColumn : EFPGridProducerEnumColumn
    {
      #region Конструктор

      public EFPDBxGridProducerEnumColumn(EFPDBxGridProducer.DocRefInfo refInfo , string[] textValues)
        : base(refInfo.ResName + "_Text", new string[] { refInfo.RefColumnName }, textValues)
      {
        _RefInfo = refInfo;
      }

      #endregion

      #region Переопределенные методы

      private EFPDBxGridProducer.DocRefInfo _RefInfo;

      protected override object GetSourceValue(EFPGridProducerValueNeededEventArgs args)
      {
        return _RefInfo.GetRefValue(args);
      }

      #endregion
    }

    /// <summary>
    /// Добавляет столбец значка для отображения перечислимого значения, которое
    /// вычисляется на основании числового поля.
    /// Перечислимые значения должны идти по порядку (0,1,2, ...).
    /// Для получения значения, сначала извлекается идентификатор документа или поддокумента из
    /// ссылочного поля с именем <paramref name="refColumnName"/>.
    /// Далее вызывается метод DBxTableCache.GetValue() для получения значения. Методу передается идентификатор
    /// и имя поля <paramref name="valueColumnName"/>.
    /// Добавляемый столбец будет иметь имя "<paramref name="refColumnName"/>.<paramref name="valueColumnName"/>_Image".
    /// </summary>
    /// <param name="refColumnName">Имя числового столбца, содержащего идентификатор документа или поддокумента.
    /// Это поле должно присутствовать в таблице.</param>
    /// <param name="masterUI">Интерфейс документа или поддокумента, на который ссылается поле</param>
    /// <param name="valueColumnName">Имя поля в структуре документа/поддокумента, которое требуется выводить.
    /// Имя поля может содержать точку, если само является ссылочным. 
    /// Это поле не относится к таблице, для которой создается просмотр. Оно передается методу DBxTableCache.GetValue().</param>
    /// <param name="imageKeys">Список тегов в EFPApp.MainImages, которые показываются в создаваемом
    /// столбце</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerEnumImageColumn AddRefEnumImage(string refColumnName, DocTypeUIBase masterUI, string valueColumnName,
      string[] imageKeys,
      string headerText)
    {
      EFPDBxGridProducer.DocRefInfo dri = new EFPDBxGridProducer.DocRefInfo(refColumnName, masterUI, valueColumnName);
      EFPGridProducerEnumImageColumn Item = new EFPDBxGridProducerEnumImageColumn(dri, imageKeys);
      Item.HeaderText = headerText;

      Add(Item);
      return Item;
    }

    private class EFPDBxGridProducerEnumImageColumn : EFPGridProducerEnumImageColumn
    {
      #region Конструктор

      public EFPDBxGridProducerEnumImageColumn(EFPDBxGridProducer.DocRefInfo refInfo , string[] textValues)
        : base(refInfo.ResName + "_Image", new string[] { refInfo.RefColumnName }, textValues)
      {
        _RefInfo = refInfo;
      }

      #endregion

      #region Переопределенные методы

      private EFPDBxGridProducer.DocRefInfo _RefInfo;

      protected override object GetSourceValue(EFPGridProducerValueNeededEventArgs args)
      {
        return _RefInfo.GetRefValue(args);
      }

      #endregion
    }

    /// <summary>
    /// Добавляет столбец значка для отображения перечислимого значения, которое
    /// вычисляется на основании числового поля.
    /// Перечислимые значения должны идти по порядку (0,1,2, ...).
    /// Для получения значения, сначала извлекается идентификатор документа или поддокумента из
    /// ссылочного поля с именем <paramref name="refColumnName"/>.
    /// Далее вызывается метод DBxTableCache.GetValue() для получения значения. Методу передается идентификатор
    /// и имя поля <paramref name="valueColumnName"/>.
    /// Добавляемый столбец будет иметь имя "<paramref name="refColumnName"/>.<paramref name="valueColumnName"/>_Image".
    /// </summary>
    /// <param name="refColumnName">Имя числового столбца, содержащего идентификатор документа или поддокумента.
    /// Это поле должно присутствовать в таблице.</param>
    /// <param name="masterUI">Интерфейс документа или поддокумента, на который ссылается поле</param>
    /// <param name="valueColumnName">Имя поля в структуре документа/поддокумента, которое требуется выводить.
    /// Имя поля может содержать точку, если само является ссылочным. 
    /// Это поле не относится к таблице, для которой создается просмотр. Оно передается методу DBxTableCache.GetValue().</param>
    /// <param name="imageKeys">Список тегов в EFPApp.MainImages, которые показываются в создаваемом
    /// столбце</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerEnumImageColumn AddRefEnumImage(string refColumnName, DocTypeUIBase masterUI, string valueColumnName,
      string[] imageKeys)
    {
      return AddRefEnumImage(refColumnName, masterUI, valueColumnName, imageKeys, String.Empty);
    }

    #endregion

    #region Текстовые представления даты

#if XXX
    /// <summary>
    /// Добавление расчетного столбца для вывода месяца и года в виде текста ("Март 2013 г.")
    /// </summary>
    /// <param name="columnPrefixName">Префикс имени столбца. Таблица должна содержать столбцы "ПрефиксГод" и "ПрефиксМесяц"</param>
    /// <param name="HeaderText">Заголовок столбца</param>
    /// <returns>Описание столбца</returns>
    public GridProducerUserColumn AddYearMonth(string columnPrefixName, string HeaderText)
    {
      GridProducerUserColumn Col = AddUserText(columnPrefixName, columnPrefixName + "Год," + columnPrefixName + "Месяц",
        new GridProducerUserColumnValueNeededEventHandler(YearMonthColumnValueNeeded),
        HeaderText,
        16 /* "Сентябрь 2012 г."*/,
        12);
      Col.TextAlign = HorizontalAlignment.Center;
      Col.SizeGroup = "YearMonth";
      return Col;
    }

    private static void YearMonthColumnValueNeeded(object Sender, GridProducerUserColumnValueNeededEventArgs Args)
    {
      // TODO:
      /*
      GridProducerUserColumn Col = (GridProducerUserColumn)Sender;
      int Year = DataTools.GetInt(Args.Row, Col.FieldNames[0]);
      int Month = DataTools.GetInt(Args.Row, Col.FieldNames[1]);
      if (Year != 0)
        Args.Value = DataConv.DateLongStr(Year, Month);
       * */
    }
#endif

    /// <summary>
    /// Добавить вычисляемый текстовый столбец для отображения интервала дат на основании двух
    /// полей с датой.
    /// Для отображения текста используется DateRangeFormatter.
    /// </summary>
    /// <param name="name">Условное имя вычисляемого столбца</param>
    /// <param name="refColumnName">Имя числового столбца, содержащего идентификатор документа или поддокумента.
    /// Это поле должно присутствовать в таблице.</param>
    /// <param name="masterUI">Интерфейс документа или поддокумента, на который ссылается поле</param>
    /// <param name="firstValueColumnName">Имя поля с начальной датой диапазона</param>
    /// <param name="lastValueColumnName">Имя поля с конечной датой диапазона</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="longFormat">Использование длинного (true) или короткого (false) формата отображения.
    /// См. класс DateRangeFormatter</param>
    /// <param name="textWidth">Ширина в текстовых единицах</param>
    /// <param name="minTextWidth">Минимальная ширина в текстовых единицах</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerColumn AddRefDateRange(string name, string refColumnName, DocTypeUIBase masterUI, string firstValueColumnName, string lastValueColumnName,
      string headerText, bool longFormat, int textWidth, int minTextWidth)
    {
      EFPGridProducerColumn Column = AddRefDateRange(name, refColumnName, masterUI, firstValueColumnName, lastValueColumnName, headerText, longFormat);
      Column.TextWidth = textWidth;
      Column.MinTextWidth = minTextWidth;
      return Column;
    }

    /// <summary>
    /// Добавить вычисляемый текстовый столбец для отображения интервала дат на основании двух
    /// полей с датой.
    /// Для отображения текста используется DateRangeFormatter.
    /// Эта версия вычисляет ширину столбца автоматически.
    /// </summary>
    /// <param name="name">Условное имя вычисляемого столбца</param>
    /// <param name="refColumnName">Имя числового столбца, содержащего идентификатор документа или поддокумента.
    /// Это поле должно присутствовать в таблице.</param>
    /// <param name="masterUI">Интерфейс документа или поддокумента, на который ссылается поле</param>
    /// <param name="firstValueColumnName">Имя поля с начальной датой диапазона</param>
    /// <param name="lastValueColumnName">Имя поля с конечной датой диапазона</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="longFormat">Использование длинного (true) или короткого (false) формата отображения.
    /// См. класс DateRangeFormatter</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerColumn AddRefDateRange(string name, string refColumnName, DocTypeUIBase masterUI, string firstValueColumnName, string lastValueColumnName,
      string headerText, bool longFormat)
    {
      EFPDBxGridProducer.DocRefInfo dri = new EFPDBxGridProducer.DocRefInfo(refColumnName, masterUI, firstValueColumnName, lastValueColumnName);
      EFPGridProducerColumn Column = new EFPGridProducerColumn(name, new string[] { refColumnName });

      Column.HeaderText = headerText;
      if (longFormat)
        Column.ValueNeeded += new EFPGridProducerValueNeededEventHandler(dri.DateRangeColumn_LongValueNeeded);
      else
        Column.ValueNeeded += new EFPGridProducerValueNeededEventHandler(dri.DateRangeColumn_ShortValueNeeded);
      if (longFormat)
        Column.TextWidth = DateRangeFormatter.Default.DateRangeLongTextLength;
      else
        Column.TextWidth = DateRangeFormatter.Default.DateRangeShortTextLength;
      Column.MinTextWidth = Column.TextWidth;
      Column.EmptyValue = DateRangeFormatter.Default.ToString(null, null, longFormat);

      Column.SizeGroup = longFormat ? "DateRangeLong" : "DateRangeShort";
      Add(Column);
      return Column;
    }

    /// <summary>
    /// Создает текстовый столбец для отображения числового поля с номером дня в диапазоне от 1 до 365 как месяца и дня (структура MonthDay).
    /// Для текстового представления используется класс DateRangeFormatter.
    /// </summary>
    /// <param name="name">Имя вычисляемого столбца табличного просмотра</param>
    /// <param name="refColumnName">Имя числового столбца, содержащего идентификатор документа или поддокумента.
    /// Это поле должно присутствовать в таблице.</param>
    /// <param name="masterUI">Интерфейс документа или поддокумента, на который ссылается поле</param>
    /// <param name="valueColumnName">Имя числового столбца в базе данных</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="longFormat">true - использовать длинный формат, false-использовать короткий формат</param>
    /// <param name="textWidth">Ширина в текстовых единицах</param>
    /// <param name="minTextWidth">Минимальная ширина в текстовых единицах</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerColumn AddRefMonthDay(string name, string refColumnName, DocTypeUIBase masterUI, string valueColumnName, string headerText, bool longFormat, int textWidth, int minTextWidth)
    {
      EFPGridProducerColumn Column = AddRefMonthDay(name, refColumnName, masterUI, valueColumnName, headerText, longFormat);
      Column.TextWidth = textWidth;
      Column.MinTextWidth = minTextWidth;
      return Column;
    }

    /// <summary>
    /// Создает текстовый столбец для отображения числового поля с номером дня в диапазоне от 1 до 365 как месяца и дня (структура MonthDay).
    /// Для текстового представления используется класс DateRangeFormatter.
    /// Эта версия вычисляет ширину столбца автоматически.
    /// </summary>
    /// <param name="name">Имя вычисляемого столбца табличного просмотра</param>
    /// <param name="refColumnName">Имя числового столбца, содержащего идентификатор документа или поддокумента.
    /// Это поле должно присутствовать в таблице.</param>
    /// <param name="masterUI">Интерфейс документа или поддокумента, на который ссылается поле</param>
    /// <param name="valueColumnName">Имя числового столбца в базе данных</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="longFormat">true - использовать длинный формат, false-использовать короткий формат</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerColumn AddRefMonthDay(string name, string refColumnName, DocTypeUIBase masterUI, string valueColumnName, string headerText, bool longFormat)
    {
      EFPDBxGridProducer.DocRefInfo dri = new EFPDBxGridProducer.DocRefInfo(refColumnName, masterUI, valueColumnName);
      EFPGridProducerColumn Column = new EFPGridProducerColumn(name, new string[] { refColumnName });
      if (longFormat)
        Column.ValueNeeded += new EFPGridProducerValueNeededEventHandler(dri.MonthDayColumn_LongValueNeeded);
      else
        Column.ValueNeeded += new EFPGridProducerValueNeededEventHandler(dri.MonthDayColumn_ShortValueNeeded);
      Column.EmptyValue = DateRangeFormatter.Default.ToString(MonthDayRange.Empty, longFormat);
      Column.HeaderText = headerText;
      if (longFormat)
        Column.TextWidth = DateRangeFormatter.Default.MonthDayLongTextLength;
      else
        Column.TextWidth = DateRangeFormatter.Default.MonthDayShortTextLength;
      Column.MinTextWidth = Column.TextWidth;
      Column.SizeGroup = longFormat ? "MonthDayLong" : "MonthDayShort";
      Add(Column);
      return Column;
    }

    /// <summary>
    /// Создает столбец для отображения двух столбоц, содержащих номер дня в диапазоне от 1 до 365 как диапазона дней в году (структура MonthDayRange).
    /// Для текстового представления используется класс DateRangeFormatter.
    /// </summary>
    /// <param name="name">Имя вычисляемого столбца табличного просмотра</param>
    /// <param name="refColumnName">Имя числового столбца, содержащего идентификатор документа или поддокумента.
    /// Это поле должно присутствовать в таблице.</param>
    /// <param name="masterUI">Интерфейс документа или поддокумента, на который ссылается поле</param>
    /// <param name="firstValueColumnName">Имя числового столбца в базе данных, задающего первый день диапазона</param>
    /// <param name="lastValueColumnName">Имя числового столбца в базе данных, задающего последний день диапазона</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="longFormat">true - использовать длинный формат, false-использовать короткий формат</param>
    /// <param name="textWidth">Ширина в текстовых единицах</param>
    /// <param name="minTextWidth">Минимальная ширина в текстовых единицах</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerColumn AddRefMonthDayRange(string name, string refColumnName, DocTypeUIBase masterUI, string firstValueColumnName, string lastValueColumnName, string headerText, bool longFormat, int textWidth, int minTextWidth)
    {
      EFPGridProducerColumn Column = AddRefMonthDayRange(name, refColumnName, masterUI, firstValueColumnName, lastValueColumnName, headerText, longFormat);
      Column.TextWidth = textWidth;
      Column.MinTextWidth = minTextWidth;
      return Column;
    }

    /// <summary>
    /// Создает столбец для отображения двух столбоц, содержащих номер дня в диапазоне от 1 до 365 как диапазона дней в году (структура MonthDayRange).
    /// Для текстового представления используется класс DateRangeFormatter.
    /// Эта версия вычисляет ширину столбца автоматически.
    /// </summary>
    /// <param name="name">Имя вычисляемого столбца табличного просмотра</param>
    /// <param name="refColumnName">Имя числового столбца, содержащего идентификатор документа или поддокумента.
    /// Это поле должно присутствовать в таблице.</param>
    /// <param name="masterUI">Интерфейс документа или поддокумента, на который ссылается поле</param>
    /// <param name="firstValueColumnName">Имя числового столбца в базе данных, задающего первый день диапазона</param>
    /// <param name="lastValueColumnName">Имя числового столбца в базе данных, задающего последний день диапазона</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <param name="longFormat">true - использовать длинный формат, false-использовать короткий формат</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerColumn AddRefMonthDayRange(string name, string refColumnName, DocTypeUIBase masterUI, string firstValueColumnName, string lastValueColumnName, string headerText, bool longFormat)
    {
      EFPDBxGridProducer.DocRefInfo dri = new EFPDBxGridProducer.DocRefInfo(refColumnName, masterUI, firstValueColumnName, lastValueColumnName);
      EFPGridProducerColumn Column = new EFPGridProducerColumn(name, new string[] { refColumnName });
      if (longFormat)
        Column.ValueNeeded += new EFPGridProducerValueNeededEventHandler(dri.MonthDayRangeColumn_LongValueNeeded);
      else
        Column.ValueNeeded += new EFPGridProducerValueNeededEventHandler(dri.MonthDayRangeColumn_ShortValueNeeded);
      Column.HeaderText = headerText;
      if (longFormat)
        Column.TextWidth = DateRangeFormatter.Default.MonthDayRangeLongTextLength;
      else
        Column.TextWidth = DateRangeFormatter.Default.MonthDayRangeShortTextLength;
      Column.MinTextWidth = Column.TextWidth;
      Column.SizeGroup = longFormat ? "MonthDayRangeLong" : "MonthDayRangeShort";
      Add(Column);
      return Column;
    }

    #endregion

    #region CheckBox

    /// <summary>
    /// Добавить столбец-флажок для логического поля.
    /// Для получения значения, сначала извлекается идентификатор документа или поддокумента из
    /// ссылочного поля с именем <paramref name="refColumnName"/>.
    /// Далее вызывается метод DBxTableCache.GetValue() для получения значения. Методу передается идентификатор
    /// и имя поля <paramref name="valueColumnName"/>.
    /// Добавляемый столбец будет иметь имя "<paramref name="refColumnName"/>.<paramref name="valueColumnName"/>".
    /// </summary>
    /// <param name="refColumnName">Имя числового столбца, содержащего идентификатор документа или поддокумента.
    /// Это поле должно присутствовать в таблице.</param>
    /// <param name="masterUI">Интерфейс документа или поддокумента, на который ссылается поле</param>
    /// <param name="valueColumnName">Имя поля в структуре документа/поддокумента, которое требуется выводить.
    /// Имя поля может содержать точку, если само является ссылочным. 
    /// Это поле не относится к таблице, для которой создается просмотр. Оно передается методу DBxTableCache.GetValue().</param>
    /// <param name="headerText">Заголовок столбца</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerCheckBoxColumn AddRefBool(string refColumnName, DocTypeUIBase masterUI, string valueColumnName,
      string headerText)
    {
      EFPDBxGridProducer.DocRefInfo dri = new EFPDBxGridProducer.DocRefInfo(refColumnName, masterUI, valueColumnName);
      return AddUserBool(dri.ResName, refColumnName, dri.RefValueColumn_ValueNeeded, headerText);
    }

    /// <summary>
    /// Добавить столбец-флажок для логического поля.
    /// Для получения значения, сначала извлекается идентификатор документа или поддокумента из
    /// ссылочного поля с именем <paramref name="refColumnName"/>.
    /// Далее вызывается метод DBxTableCache.GetValue() для получения значения. Методу передается идентификатор
    /// и имя поля <paramref name="valueColumnName"/>.
    /// Добавляемый столбец будет иметь имя "<paramref name="refColumnName"/>.<paramref name="valueColumnName"/>".
    /// </summary>
    /// <param name="refColumnName">Имя числового столбца, содержащего идентификатор документа или поддокумента.
    /// Это поле должно присутствовать в таблице.</param>
    /// <param name="masterUI">Интерфейс документа или поддокумента, на который ссылается поле</param>
    /// <param name="valueColumnName">Имя поля в структуре документа/поддокумента, которое требуется выводить.
    /// Имя поля может содержать точку, если само является ссылочным. 
    /// Это поле не относится к таблице, для которой создается просмотр. Оно передается методу DBxTableCache.GetValue().</param>
    /// <returns>Описание столбца</returns>
    public EFPGridProducerCheckBoxColumn AddRefBool(string refColumnName, DocTypeUIBase masterUI, string valueColumnName)
    {
      return AddRefBool(refColumnName, masterUI, valueColumnName, String.Empty);
    }

    #endregion

    #endregion

    #endregion
  }

  /// <summary>
  /// Реализация свойства EFPDBxGridProducer.ToolTips.
  /// </summary>
  public class EFPDBxGridProducerToolTips : EFPGridProducerToolTips
  {
    #region Конструктор

    /// <summary>
    /// Создает коллекцию
    /// </summary>
    /// <param name="gridProducer">Объект-владелец</param>
    public EFPDBxGridProducerToolTips(EFPDBxGridProducer gridProducer)
    {
      if (gridProducer == null)
        throw new ArgumentNullException("gridProducer");
      _GridProducer = gridProducer;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Объект-владелец
    /// </summary>
    public EFPDBxGridProducer GridProducer { get { return _GridProducer; } }
    private EFPDBxGridProducer _GridProducer;

    #endregion

    #region Методы добавления подсказок

    #region Текстовое представление документа

    /// <summary>
    /// Добавить подсказку для отображения ссылки на документ или поддокумент. Текст подсказки будет
    /// содержать значение, возвращаемое DocTypeUI/SubDocTypeUI.GetTextValue(), если ссылочное поле непустое. 
    /// Подсказка будет иметь имя "RefColumnName_Text"
    /// </summary>
    /// <param name="refColumnName">Имя ссылочного поля</param>
    /// <param name="masterUI">Интерфейс документа или поддокумента, на который ссылается поле</param>
    /// <param name="prefixText">Текст, который будет выведен перед значением в виде "DisplayName : Значение".
    /// Если null, то будет выводиться имя поля. 
    /// Если пустая строка, то ничего выводится перед значением не будет</param>
    /// <returns>Объект выдачи подсказки</returns>
    public EFPGridProducerToolTip AddRefDocText(string refColumnName, DocTypeUIBase masterUI,
      string prefixText)
    {
      EFPDBxGridProducer.DocRefInfo dri = new EFPDBxGridProducer.DocRefInfo(refColumnName, masterUI);
      return AddUserItem(refColumnName + "_Text", refColumnName, new EFPGridProducerValueNeededEventHandler(dri.RefDocTextColumn_ValueNeeded), prefixText);
    }

    /// <summary>
    /// Добавить подсказку для отображения ссылки на документ произвольного типа,
    /// когда в таблице данных есть поле с кодом таблицы и поле с идентификатором документа. 
    /// Подсказка будет содержать значение, возвращаемое DocTypeUI.GetTextValue(), если ссылочное поле непустое. 
    /// Столбец будет иметь имя "RefColumnName_Text"
    /// </summary>
    /// <param name="tableIdColumnName">Имя числового столбца, содержащего идентификатор таблицы документа</param>
    /// <param name="refColumnName">Имя числового столбца, содержащего идентификатор документа</param>
    /// <param name="prefixText">Текст, который будет выведен перед значением в виде "DisplayName : Значение".
    /// Если пустая строка, то ничего выводится перед значением не будет</param>
    /// <returns>Объект EFPGridProducerToolTip</returns>
    public EFPGridProducerToolTip AddVTRefDocText(string tableIdColumnName, string refColumnName,
      string prefixText)
    {
      if (String.IsNullOrEmpty(tableIdColumnName))
        throw new ArgumentNullException("tableIdColumnName");
      if (String.IsNullOrEmpty(refColumnName))
        throw new ArgumentNullException("refColumnName");
      if (String.Compare(tableIdColumnName, refColumnName, StringComparison.OrdinalIgnoreCase) == 0)
        throw new ArgumentException("Имена исходных столбцов не могут совпадать");
      return AddUserItem(refColumnName + "_Text", tableIdColumnName + "," + refColumnName, new EFPGridProducerValueNeededEventHandler(VTRefDocTextColumn_ValueNeeded),
        prefixText);
    }

    private void VTRefDocTextColumn_ValueNeeded(object sender, EFPGridProducerValueNeededEventArgs args)
    {
      Int32 TableId = args.GetInt(0);
      Int32 DocId = args.GetInt(1);
      args.Value = _GridProducer.UI.TextHandlers.GetTextValue(TableId, DocId);
    }


    #endregion

    #endregion
  }

  /// <summary>
  /// Получение значения с использованием поля таблицы или объекта DBxCache
  /// </summary>
  public class TreeViewCachedValueAdapter
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник
    /// </summary>
    /// <param name="controlProvider">Провайдер иерархического просмотра</param>
    /// <param name="nodeControl">Элемент в TreeViewAdv, к которому присоединяется обработчик события ValueNeeded</param>
    /// <param name="cache">Система кэширования данных</param>
    /// <param name="column">Генератор столбца табличного просмотра</param>
    public TreeViewCachedValueAdapter(EFPDataTreeView controlProvider, BindableControl nodeControl, DBxCache cache, EFPGridProducerColumn column)
    {
      if (controlProvider == null)
        throw new ArgumentNullException("controlProvider");
      if (String.IsNullOrEmpty(nodeControl.DataPropertyName))
        throw new ArgumentException("Свойство BaseTextControl.DataPropertyName не установлено");


      _ControlProvider = controlProvider;
      _UserColumn = column;
      if (_UserColumn != null)
        nodeControl.ValueNeeded += new EventHandler<NodeControlValueEventArgs>(NodeControl_UserValueNeeded);
      else
        nodeControl.ValueNeeded += new EventHandler<NodeControlValueEventArgs>(NodeControl_ValueNeeded);
      _Cache = cache;
    }

    #endregion

    #region Свойство

    private EFPDataTreeView _ControlProvider;

    /// <summary>
    /// Система кэширования данных
    /// </summary>
    public DBxCache Cache { get { return _Cache; } }
    private DBxCache _Cache;

    private string _TableName;

    private string _ColumnName;

    private bool _UseCache;

    private EFPGridProducerColumn _UserColumn;

    #endregion

    #region Получение значения

    private void NodeControl_ValueNeeded(object sender, NodeControlValueEventArgs args)
    {
      BindableControl NodeControl = (BindableControl)sender;
      DataRow Row = args.Node.Tag as DataRow;
      if (Row != null)
      {
        if (_ColumnName == null)
        {
          // Инициализация при первом вызове
          if (Row.Table.Columns.Contains(NodeControl.DataPropertyName))
          {
            _ColumnName = NodeControl.DataPropertyName;
            _UseCache = false;
          }
          else
          {
            int p = NodeControl.DataPropertyName.IndexOf('.');
            if (p < 0)
              throw new InvalidOperationException("Таблица \"" + Row.Table.TableName + "\" не содержит столбца \"" +
                NodeControl.DataPropertyName + "\"");
            _ColumnName = NodeControl.DataPropertyName.Substring(0, p);
            _TableName = Row.Table.TableName;
            _UseCache = true;
          }
        }

        if (Row.RowState == DataRowState.Deleted)
          args.Value = null;
        else
        {
          if (_UseCache)
          {
            Int32 RefId = DataTools.GetInt(Row, _ColumnName);
            args.Value = Cache[_TableName].GetRefValue(NodeControl.DataPropertyName, RefId, Row.Table.DataSet);
          }
          else
            args.Value = DataTools.GetString(Row, NodeControl.DataPropertyName);
        }
      }
      else
        args.Value = null;
    }

    private void NodeControl_UserValueNeeded(object sender, NodeControlValueEventArgs args)
    {
      //BindableControl NodeControl = (BindableControl)sender;
      EFPDataViewRowInfo rowInfo = _ControlProvider.GetRowInfo(args.Node);
      if (rowInfo.DataBoundItem != null)
      {
        //if (Row.RowState == DataRowState.Deleted)
        //  args.Value = "Строка удалена";
        //else
        //{
        args.Value = _UserColumn.GetValue(rowInfo);
        //}
      }
      else
        args.Value = null;
    }

    #endregion

    #region Статический метод инициализации

    /// <summary>
    /// Инициализация столбцов иерархического просмотра.
    /// Находит все объекты BindableControl, созданные в <paramref name="controlProvider"/>.Control,
    /// и создает для них экземпляры TreeViewCachedValueAdapter.
    /// </summary>
    /// <param name="controlProvider">Провайдер иерархического просмотра</param>
    /// <param name="cache">Система кэширования данных</param>
    /// <param name="gridProducer">Хранилище описаний столбцов</param>
    public static void InitColumns(EFPDataTreeView controlProvider, DBxCache cache, EFPGridProducer gridProducer)
    {
      foreach (NodeControl nc in controlProvider.Control.NodeControls)
      {
        BindableControl bc = nc as BindableControl;
        if (bc != null)
        {
          EFPGridProducerColumn Column = gridProducer.Columns[bc.DataPropertyName];
          if (Column == null)
            continue; // неизвестно что
          new TreeViewCachedValueAdapter(controlProvider, bc, cache, Column);
        }
      }
    }

    #endregion
  }

  /// <summary>
  /// Базовый класс для EFPDocTypeGridProducer и EFPSubDocTypeGridProducer
  /// </summary>
  public abstract class EFPDocTypeBaseGridProducer : EFPDBxGridProducer
  {
    #region Конструктор

    internal EFPDocTypeBaseGridProducer(DBUI ui, DBxDocTypeBase docTypeBase)
      : base(ui)
    {
      if (docTypeBase == null)
        throw new ArgumentNullException("docTypeBase");
      _DocTypeBase = docTypeBase;
    }

    #endregion

    #region Свойства

    internal DBxDocTypeBase DocTypeBase { get { return _DocTypeBase; } }
    private DBxDocTypeBase _DocTypeBase;

    #endregion
  }

  /// <summary>
  /// Продюсер табличного просмотра, предназначенного для просмотра документов заданного вида
  /// </summary>
  public class EFPDocTypeGridProducer : EFPDocTypeBaseGridProducer
  {
    #region Конструктор

    /// <summary>
    /// Создает GridProducer
    /// </summary>
    /// <param name="docTypeUI">Интерфейс вида документов</param>
    public EFPDocTypeGridProducer(DocTypeUI docTypeUI)
      : base(docTypeUI.UI, docTypeUI.DocType)
    {
      _DocTypeUI = docTypeUI;

      FixedColumns.Add(DBSDocType.Id);
      if (!String.IsNullOrEmpty(docTypeUI.DocType.GroupRefColumnName))
        FixedColumns.Add(docTypeUI.DocType.GroupRefColumnName);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Интерфейс вида документов, к которому относится GridProducer
    /// </summary>
    public DocTypeUI DocTypeUI { get { return _DocTypeUI; } }
    private DocTypeUI _DocTypeUI;

    #endregion
  }

  /// <summary>
  /// Продюсер табличного просмотра, предназначенного для просмотра поддокументов заданного вида
  /// </summary>
  public class EFPSubDocTypeGridProducer : EFPDocTypeBaseGridProducer
  {
    #region Конструктор

    /// <summary>
    /// Создает GridProducer
    /// </summary>
    /// <param name="subDocTypeUI">Интерфейс вида поддокументов</param>
    public EFPSubDocTypeGridProducer(SubDocTypeUI subDocTypeUI)
      : base(subDocTypeUI.UI, subDocTypeUI.SubDocType)
    {
      _SubDocTypeUI = subDocTypeUI;

      FixedColumns.Add(DBSSubDocType.Id);
      FixedColumns.Add(DBSSubDocType.DocId);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Интерфейс вида поддокументов, к которому относится GridProducer
    /// </summary>
    public SubDocTypeUI SubDocTypeUI { get { return _SubDocTypeUI; } }
    private SubDocTypeUI _SubDocTypeUI;

    #endregion
  }
}
