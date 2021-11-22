// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.DependedValues;
using FreeLibSet.Data.Docs;
using FreeLibSet.Data;
using System.Windows.Forms;
using FreeLibSet.Controls;
using FreeLibSet.Core;
using FreeLibSet.UICore;

namespace FreeLibSet.Forms.Docs
{
  // Комбоблоки выбора нескольких документов

  #region Делегаты

  /// <summary>
  /// Аргументы события EFPMultiDocComboBoxBase.TextValueNeeded
  /// </summary>
  public class EFPMultiDocComboBoxTextValueNeededEventArgs : EFPComboBoxTextValueNeededEventArgs
  {
    #region Конструктор

    /// <summary>
    /// Создается комбоблоком
    /// </summary>
    /// <param name="owner">Объект-владелец</param>
    public EFPMultiDocComboBoxTextValueNeededEventArgs(EFPMultiDocComboBoxBase owner)
    {
      _Owner = owner;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Выбранные идентификаторы
    /// </summary>
    public Int32[] Ids { get { return _Owner.Ids; } }

    private EFPMultiDocComboBoxBase _Owner;

    #endregion
  }

  /// <summary>
  /// Делегат события EFPMultiDocComboBoxBase.TextValueNeeded
  /// </summary>
  /// <param name="sender">Комбоблок</param>
  /// <param name="args">Аргументы события</param>
  public delegate void EFPMultiDocComboBoxTextValueNeededEventHandler(object sender,
    EFPMultiDocComboBoxTextValueNeededEventArgs args);

  #endregion

  /// <summary>
  /// Базовый класс провайдера комбоблока выбора нескольких документов.
  /// Реализует свойство Ids как массив идентификаторов.
  /// Нельзя использовать IdList, т.к. порядок поддокументов важен
  /// </summary>
  public abstract class EFPMultiDocComboBoxBase : EFPAnyDocComboBoxBase, IDepSyncObject
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    /// <param name="ui">Пользовательский интерфейс для доступа к документам</param>
    public EFPMultiDocComboBoxBase(EFPBaseProvider baseProvider, UserSelComboBox control, DBUI ui)
      : base(baseProvider, control, ui)
    {
      _TextValueNeededArgs = new EFPMultiDocComboBoxTextValueNeededEventArgs(this);
      _Ids = DataTools.EmptyIds;

      _MaxTextItemCount = 1;
    }

    #endregion

    #region Свойство Ids - идентификаторы строк

    /// <summary>
    /// Устанавливается на время
    /// </summary>
    private bool _InsideSetIds;

    /// <summary>
    /// Массив выбранных идентификаторов.
    /// Если документ не выбран, свойство содержит пустой массив.
    /// В классах-наследниках свойство переименовывается.
    /// </summary>
    internal protected virtual Int32[] Ids
    {
      get { return _Ids; }
      set
      {
        if (_InsideSetIds)
          return;

        if (value == null)
          value = DataTools.EmptyIds;
        if (Object.ReferenceEquals(value, _Ids))
          return;

        for (int i = 0; i < value.Length; i++)
        {
          if (value[i] == 0)
            throw new ArgumentException("Массив идентификатор не может содержать значение 0");
        }

        _InsideSetIds = true;
        try
        {
          _Ids = value;

          if (_IdsEx != null)
            _IdsEx.Value = value;
          if (_SingleIdEx != null)
            _SingleIdEx.Value = SingleId;
          if (_DeletedEx != null)
            _DeletedEx.SetDelayed();
          InitTextAndImage();
          ClearButtonEnabled = (_Ids.Length > 0);
          //if (IdValueChangedBeforeValidate != null)
          //  IdValueChangedBeforeValidate(this, EventArgs.Empty);
          Validate();
          DoSyncValueChanged();

          if (CommandItems is EFPAnyDocComboBoxBaseCommandItems)
            ((EFPAnyDocComboBoxBaseCommandItems)CommandItems).InitEnabled();
        }
        finally
        {
          _InsideSetIds = false;
        }
      }
    }
    private Int32[] _Ids;

    /// <summary>
    /// Идентификатор строк выбранного документа. 
    /// В классах-наследниках свойство переименовывается.
    /// </summary>
    internal protected DepValue<Int32[]> IdsEx
    {
      get
      {
        InitIdsEx();
        return _IdsEx;
      }
      set
      {
        InitIdsEx();
        _IdsEx.Source = value;
      }
    }

    private void InitIdsEx()
    {
      if (_IdsEx == null)
      {
        _IdsEx = new DepInput<Int32[]>(Ids, IdsEx_ValueChanged);
        _IdsEx.OwnerInfo = new DepOwnerInfo(this, "IdsEx");
      }
    }
    private DepInput<Int32[]> _IdsEx;

    ///// <summary>
    ///// Это событие вызывается при изменении текущего значения идентификатора, но
    ///// до вызова метода Validate(). 
    ///// </summary>
    //internal event EventHandler IdsValueChangedBeforeValidate;

    private void IdsEx_ValueChanged(object sender, EventArgs args)
    {
      Ids = _IdsEx.Value;
    }


    /// <summary>
    /// Возвращает true при Id не равном 0.
    /// </summary>
    public override bool IsNotEmpty { get { return Ids.Length > 0; } }

    /// <summary>
    /// Объект-функция возвращает true, если есть хотя бы один выбранный идентификатор.
    /// </summary>
    public DepValue<bool> IsNotEmptyEx
    {
      get
      {
        if (_IsNotEmptyEx == null)
          _IsNotEmptyEx = new DepExpr1<bool, Int32[]>(IdsEx, CalcIsNotEmpty);
        return _IsNotEmptyEx;
      }
    }
    private DepValue<bool> _IsNotEmptyEx;

    private static bool CalcIsNotEmpty(Int32[] ids)
    {
      return ids.Length > 0;
    }

    #endregion

    #region Свойство SingleId

    /// <summary>
    /// Идентификатор единственного выбранного документа. 
    /// В классах-наследниках свойство переименовывается.
    /// </summary>
    protected Int32 SingleId
    {
      get
      {
        if (Ids.Length == 1)
          return Ids[0];
        else
          return 0;
      }
      set
      {
        if (_InsideSetIds)
          return;

        if (value == 0)
          Ids = DataTools.EmptyIds;
        else
          Ids = new Int32[] { value };
      }
    }


    /// <summary>
    /// Идентификатор единственного выбранного документа. 
    /// В классах-наследниках свойство переименовывается.
    /// </summary>
    protected DepValue<Int32> SingleIdEx
    {
      get
      {
        InitSingleIdEx();
        return _SingleIdEx;
      }
      set
      {
        InitSingleIdEx();
        _SingleIdEx.Source = value;
      }
    }

    private void InitSingleIdEx()
    {
      if (_SingleIdEx == null)
      {
        _SingleIdEx = new DepInput<Int32>(SingleId, SingleIdEx_ValueChanged);
        _SingleIdEx.OwnerInfo = new DepOwnerInfo(this, "SingleIdEx");
      }
    }
    private DepInput<Int32> _SingleIdEx;

    private void SingleIdEx_ValueChanged(object sender, EventArgs args)
    {
      SingleId = _SingleIdEx.Value;
    }

    #endregion

    #region Событие TextValueNeeded

    /// <summary>
    /// Это событие вызывается после выбора значения из списка или установки свойства
    /// Id и позволяет переопределить текст в комбоблоке, текст всплываюующей подсказки
    /// и изображение. Событие вызывается в том числе и при Id=0
    /// Также вызывается при обращении к свойству TextValue
    /// </summary>
    public event EFPMultiDocComboBoxTextValueNeededEventHandler TextValueNeeded
    {
      // 18.03.2016
      // После установки обработчика требуется обновить текст, т.к. обработчик может изменить текст для Id=0
      // или свойство Id могло быть уже установлено до присоединения обработчика
      // Альтернатива:
      // Вызвать InitTextAndImage() из OnShown(), но тогда свойства TextValue и прочие будут иметь
      // некорректное значение до вывода на экран
      add
      {
        _TextValueNeeded += value;
        InitTextAndImage();
      }
      remove
      {
        _TextValueNeeded -= value;
        InitTextAndImage();
      }
    }
    private EFPMultiDocComboBoxTextValueNeededEventHandler _TextValueNeeded;

    #endregion

    #region InitTextAndImage

    /// <summary>
    /// Чтобы не создавать объект каждый раз, создаем его в конструкторе.
    /// Также используем для хранения изображения между вызовом InitText() и
    /// его выводом в комбоблоке
    /// </summary>
    private EFPMultiDocComboBoxTextValueNeededEventArgs _TextValueNeededArgs;

    /// <summary>
    /// Установка текста элемента
    /// EFPDocComboBox доопределяет метод для установки доступности кнопки Edit
    /// </summary>
    protected override void InitTextAndImage()
    {
      try
      {
        _TextValueNeededArgs.Clear();
        // Стандартные значения текста, подсказки и изображения
        if (Ids.Length == 0)
        {
          _TextValueNeededArgs.TextValue = EmptyText;
          _TextValueNeededArgs.ImageKey = EmptyImageKey;
        }
        else
        {
          _TextValueNeededArgs.TextValue = DoGetText();
          if (EFPApp.ShowListImages)
          {
            _TextValueNeededArgs.ImageKey = DoGetImageKey();

            EFPDataGridViewColorType ColorType;
            bool Grayed;
            DoGetValueColor(out ColorType, out Grayed);
            _TextValueNeededArgs.Grayed = Grayed;
          }
          else
            _TextValueNeededArgs.ImageKey = String.Empty;
          if (EFPApp.ShowToolTips)
            _TextValueNeededArgs.ToolTipText = DoGetValueToolTipText();
          else
            _TextValueNeededArgs.ToolTipText = String.Empty;
        }

        // Пользовательский обработчик
        if (_TextValueNeeded != null)
          _TextValueNeeded(this, _TextValueNeededArgs);

        // Устанавливаем значения. Изображение используется отдельно
        Control.Text = _TextValueNeededArgs.TextValue;
        if (EFPApp.ShowListImages)
        {
          if (String.IsNullOrEmpty(_TextValueNeededArgs.ImageKey))
            Control.Image = null;
          else
            Control.Image = EFPApp.MainImages.Images[_TextValueNeededArgs.ImageKey];
        }
        if (EFPApp.ShowToolTips)
          ValueToolTipText = _TextValueNeededArgs.ToolTipText;
      }
      catch (Exception e)
      {
        Control.Text = "!!! Ошибка !!! " + e.Message;
        if (EFPApp.ShowListImages)
          Control.Image = EFPApp.MainImages.Images["Error"];
        EFPApp.ShowTempMessage("Ошибка при получении текста: " + e.Message);
      }
      if (UI.DebugShowIds)
        Control.Text = "Id=" + StdConvert.ToString(Ids) + " " + Control.Text;
    }

    /// <summary>
    /// Получение текста для текущего значения, если Id!=0
    /// </summary>
    /// <returns>Текстовое представление</returns>
    protected abstract string DoGetText();

    /// <summary>
    /// Получение изображения для текущего значения, если Id!=0
    /// </summary>
    /// <returns>Имя изображения в EFPApp.MainImages</returns>
    protected virtual string DoGetImageKey()
    {
      return String.Empty;
    }

    /// <summary>
    /// Получение раскраски для строки документа / поддокумента
    /// </summary>
    /// <param name="colorType">Сюда помещается цвет строки.
    /// Это значение не используется</param>
    /// <param name="grayed">Сюда должно быть записано true, если документ должен быть помечен серым цветом</param>
    protected virtual void DoGetValueColor(out EFPDataGridViewColorType colorType, out bool grayed)
    {
      colorType = EFPDataGridViewColorType.Normal;
      grayed = false;
    }

    /// <summary>
    /// Получение подсказки для текущего значения, если Id!=0
    /// </summary>
    /// <returns>Подсказка по значению</returns>
    protected virtual string DoGetValueToolTipText()
    {
      return String.Empty;
    }

    #endregion

    #region Свойство

    /// <summary>
    /// Максимальное количество идентификаторов, которое может быть выбрано, при котором
    /// отображаются названия всех элементов через запятую.
    /// Когда выбрано больше элементов, их количество выводится в скобках.
    /// По умолчанию равно 1.
    /// </summary>
    public int MaxTextItemCount
    {
      get { return _MaxTextItemCount; }
      set
      {
        if (value == _MaxTextItemCount)
          return;
        _MaxTextItemCount = value;
        InitTextAndImage();
      }
    }
    private int _MaxTextItemCount;

    #endregion

    #region Свойство Deleted

    /// <summary>
    /// Возвращает true, если выбранный документ или поддокумент удален
    /// Если документ не выбран, то возвращается false
    /// </summary>
    public bool Deleted
    {
      get
      {
        for (int i = 0; i < Ids.Length; i++)
        {
          string Message;
          if (GetDeletedValue(Ids[i], out Message))
            return true;
        }
        return false;
      }
    }

    /// <summary>
    /// Возвращает true, если выбранный документ или поддокумент удален
    /// Если документ не выбран, то возвращается false.
    /// Управляемое свойство для Deleted
    /// </summary>
    public DepValue<bool> DeletedEx
    {
      get
      {
        if (_DeletedEx == null)
        {
          _DeletedEx = new DepDelayedValue<bool>(Deleted_ValueNeeded);
          _DeletedEx.OwnerInfo = new DepOwnerInfo(this, "DeletedEx");
        }
        return _DeletedEx;
      }
    }
    private DepDelayedValue<bool> _DeletedEx;

    void Deleted_ValueNeeded(object sender, DepValueNeededEventArgs<bool> args)
    {
      args.Value = Deleted;
    }

    /// <summary>
    /// Определить, что выбранный документ удален
    /// </summary>
    /// <returns></returns>
    protected abstract bool GetDeletedValue(Int32 id, out string message);

    /// <summary>
    /// Извещает, что свойство Deleted, возможно, изменилось
    /// </summary>
    protected void SetDeletedChanged()
    {
      if (_DeletedEx != null)
        _DeletedEx.SetDelayed();
    }


    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Проверка корректности значения.
    /// Идентификаторы проверяются в зависимости от свойств CanBeEmpty и 
    /// CanBeDeleted (с исвпользованием виртуального метода GetDeletedValue())
    /// </summary>
    protected override void OnValidate()
    {
      if (Ids.Length == 0)
      {
        switch (CanBeEmptyMode)
        {
          case UIValidateState.Error:
            SetError("Значение \"" + DisplayName + "\" должно быть выбрано из списка");
            break;
          case UIValidateState.Warning:
            SetWarning("Значение \"" + DisplayName + "\", вероятно, должно быть выбрано из списка");
            break;
        }
      }
      else if (!CanBeDeleted)
      {
        for (int i = 0; i < Ids.Length; i++)
        {
          string Message;
          if (GetDeletedValue(Ids[i], out Message))
          {
            if (CanBeDeletedMode == UIValidateState.Warning)
              SetWarning(Message);
            else
              SetError(Message);
          }
        }
      }
    }

    /// <summary>
    /// Возвращает или устанавливает синхронизированное значение свойства Id для реализации интерфейса IDepSyncObject.
    /// </summary>
    public override object SyncValue
    {
      get
      {
        return Ids;
      }
      set
      {
        Ids = (Int32[])value;
      }
    }

    /// <summary>
    /// Устанавливает Id=0
    /// </summary>
    public override void Clear()
    {
      Ids = DataTools.EmptyIds;
    }

    #endregion

    //#region Значения полей строки документа или поддокумента

    ///// <summary>
    ///// Получить значение поля, соответствующее выбранному документу или поддокументу
    ///// </summary>
    ///// <param name="ColumnName">Имя поля</param>
    ///// <returns>Необработанное значение поле или null, если значение не выбрано</returns>
    //public abstract object GetColumnValue(string ColumnName);

    //#endregion
  }

  /// <summary>
  /// Базовый класс для EFPMultiDocComboBox.
  /// Добавляет к EFPMultiDocComboBoxBase поддержку фильтров
  /// </summary>
  public abstract class EFPMultiDocComboBoxBaseWithFilters : EFPMultiDocComboBoxBase
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    /// <param name="ui">Пользовательский интерфейс для доступа к документам</param>
    public EFPMultiDocComboBoxBaseWithFilters(EFPBaseProvider baseProvider, UserSelComboBox control, DBUI ui)
      : base(baseProvider, control, ui)
    {
      _ClearByFilter = true;
    }

    #endregion

    #region Свойство Filters

    /// <summary>
    /// Дополнительные фильтры для выбора документов из справочника
    /// </summary>
    public GridFilters Filters
    {
      get
      {
        if (_Filters == null)
        {
          _Filters = new GridFilters();
          _Filters.Changed += new EventHandler(HandleFiltersChanged);
        }
        return _Filters;
      }
    }
    private GridFilters _Filters;

    /// <summary>
    /// Возвращает число фильтров (свойство DocFilters.Count, включая пустые фильтры)
    /// </summary>
    public int FilterCount
    {
      get
      {
        if (_Filters == null)
          return 0;
        else
          return _Filters.Count;
      }
    }


    /// <summary>
    /// Вызывается при изменении фильтров
    /// </summary>
    private void HandleFiltersChanged(object sender, EventArgs args)
    {
      OnFiltersChanged();
    }

    /// <summary>
    /// Вызывается при изменении фильтров
    /// </summary>
    protected virtual void OnFiltersChanged()
    {
      FilterPassed = TestFilter();
      if (ClearByFilter)
      {
        if (!FilterPassed)
          Ids = DataTools.EmptyIds;
      }
      else
        Validate();
    }

    /// <summary>
    /// Проверка соответствия всех выбранных документов фильтру DocFilters
    /// </summary>
    /// <returns>True, если все документы соответствует фильтру или нет документов или DocFilters неактивны</returns>
    public bool TestFilter()
    {
      Int32 BadId;
      DBxCommonFilter BadFilter;
      return TestFilter(out BadId, out BadFilter);
    }

    /// <summary>
    /// Проверка соответствия документа Id фильтру DocFilters
    /// </summary>
    /// <param name="badId">Сюда записывается идентификатор документа, не прошедшего фильтр</param>
    /// <param name="badFilter">Сюда записывается ссылка на первый фильтр в списке Filters, который не соответствует выбранному документу</param>
    /// <returns>True, если документ соответствует, или DocId=0 или DocFilters неактивны</returns>
    public bool TestFilter(out Int32 badId, out DBxCommonFilter badFilter)
    {
      badFilter = null;
      badId = 0;

      if (FilterCount == 0)
        return true;
      if (_Filters.IsEmpty)
        return true;

      for (int i = 0; i < Ids.Length; i++)
      {
        if (!DoTestFilter(Ids[i], out badFilter))
        {
          badId = Ids[i];
          return false;
        }
      }

      return true;
    }

    /// <summary>
    /// Проверка попадания в фильтр.
    /// Метод проверяет, что запись с идентификатором <paramref name="id"/> проходит
    /// условия фильтров.
    /// </summary>
    /// <param name="id">Идентификатор документа или поддокумента</param>
    /// <param name="badFilter">Если какой-либо фильтр не проходит, то возвращается
    /// первый фильтр в списке Filters, который "не пропускает" запись.
    /// Если запись проходит все фильтры, сюда помещается null</param>
    /// <returns>True, если запись проходит все фильтры</returns>
    protected abstract bool DoTestFilter(Int32 id, out DBxCommonFilter badFilter);

    /// <summary>
    /// Проверка корректности значения.
    /// Дополнительно к базовому методу, проверяется попадание выбранной записи в фильтр
    /// </summary>
    protected override void OnValidate()
    {
      base.OnValidate();
      if (ValidateState == UIValidateState.Error)
        return;

      try
      {
        Int32 BadId;
        DBxCommonFilter BadFilter;
        if (!TestFilter(out BadId, out BadFilter))
          SetError("Выбраное значение не проходит фильтр \"" + BadFilter.DisplayName + "\" (" + ((IEFPGridFilter)(BadFilter)).FilterText + ")");
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка проверки соответствия значения \"" + DisplayName + "\" фильтру");
        SetError(e.Message);
      }
    }

    #endregion

    #region Свойство ClearByFilter

    /// <summary>
    /// Выполнять ли очистку текущего значения при смене фильтра, если значение не
    /// проходит условие для нового фильтра
    /// Значение по умолчанию - true (очищать)
    /// Если установит в false, то "неподходящее" значение останется выбранным, но
    /// при проверке Validate() будет выдаваться ошибка
    /// </summary>
    public bool ClearByFilter
    {
      get { return _ClearByFilter; }
      set
      {
        if (value == _ClearByFilter)
          return;
        _ClearByFilter = value;
        if (_ClearByFilterEx != null)
          _ClearByFilterEx.Value = value;

        // При переключении способа фильтрации - тоже, что и при изменении фильтра
        OnFiltersChanged();
      }
    }
    private bool _ClearByFilter;

    /// <summary>
    /// Действие, выполняемое при изменении фильтра, когда текущее значение DocId
    /// не соответствует новому фильтру
    /// True (по умолчанию) - сбросить DocId в 0
    /// False - оставить текущее значение, но показать ошибку.
    /// Значение свойства не влияет на действия, выполняемые при установке недопустимого
    /// значения DocId. В этом случае всегда показывается ошибка
    /// </summary>
    public DepValue<Boolean> ClearByFilterEx
    {
      get
      {
        InitClearByFilterEx();
        return _ClearByFilterEx;
      }
      set
      {
        InitClearByFilterEx();
        _ClearByFilterEx.Source = value;
      }
    }

    private void InitClearByFilterEx()
    {
      if (_ClearByFilterEx == null)
      {
        _ClearByFilterEx = new DepInput<bool>(ClearByFilter, ClearByFilterEx_ValueChanged);
        _ClearByFilterEx.OwnerInfo = new DepOwnerInfo(this, "ClearByFilterEx");
      }
    }
    private DepInput<Boolean> _ClearByFilterEx;

    private void ClearByFilterEx_ValueChanged(object sender, EventArgs args)
    {
      ClearByFilter = _ClearByFilterEx.Value;
    }

    #endregion

    #region Свойство FilterPassed

    /// <summary>
    /// Свойство возвращает true, если текущий выбранный документ (Id) проходит условие фильтра.
    /// Если свойство ClearByFilter имеет значение true (по умолчанию), то FilterPassed всегда
    /// возвращает true, т.к. неподходящие значения обнуляются автоматически
    /// </summary>
    public bool FilterPassed
    {
      get { return _FilterPassed; }
      private set
      {
        _FilterPassed = value;
        if (_FilterPassedEx != null)
          _FilterPassedEx.OwnerSetValue(value);
      }
    }
    private bool _FilterPassed;

    /// <summary>
    /// Версия свойства FilterPassed, с помощью которой можно управлять зависимыми значениями
    /// </summary>
    public DepValue<bool> FilterPassedEx
    {
      get
      {
        if (_FilterPassedEx == null)
        {
          _FilterPassedEx = new DepOutput<bool>(_FilterPassed);
          _FilterPassedEx.OwnerInfo = new DepOwnerInfo(this, "FilterPassedEx");
        }
        return _FilterPassedEx;
      }
    }
    private DepOutput<bool> _FilterPassedEx;

    #endregion
  }

  /// <summary>
  /// Комбоблок выбора нескольких документов
  /// </summary>
  public class EFPMultiDocComboBox : EFPMultiDocComboBoxBaseWithFilters
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    /// <param name="docTypeUI">Пользовательский интерфейс для доступа к документам</param>
    public EFPMultiDocComboBox(EFPBaseProvider baseProvider, UserSelComboBox control, DocTypeUI docTypeUI)
      : base(baseProvider, control, docTypeUI.UI)
    {
      if (docTypeUI == null)
        throw new ArgumentNullException("docTypeUI");
      this.DocType = docTypeUI.DocType;

      control.EditButton = true;
      control.EditClick += new EventHandler(Control_EditClick);
      if (EFPApp.ShowToolTips) // 15.03.2018
      {
        control.PopupButtonToolTipText = "Выбрать: " + DocType.PluralTitle;
        control.ClearButtonToolTipText = "Очистить поле выбора";
      }
    }

    /// <summary>
    /// Объект пользовательского интерфейса для вида документов.
    /// Не может быть null.
    /// Свойства DocType, DocTypeUI и DocTypeName синхронизированы.
    /// </summary>
    public DocTypeUI DocTypeUI
    {
      get { return UI.DocTypes[DocType.Name]; }
      set
      {
        if (value == null)
          throw new ArgumentNullException();
        DocType = value.DocType;
      }
    }

    #endregion

    #region Текущий тип документа

    #region DocType

    /// <summary>
    /// Описание вида документа, из которого могут выбираться документы.
    /// Не может быть null.
    /// Свойства DocType, DocTypeUI, DocTypeName и DocTableId синхронизированы.
    /// </summary>
    public DBxDocType DocType
    {
      get { return _DocType; }
      set
      {
        if (value == null)
          throw new ArgumentException();
        if (value == _DocType)
          return;
        _DocType = value;
        DocIds = DataTools.EmptyIds;
        if (_DocTableIdEx != null)
          _DocTableIdEx.Value = DocTableId;
        if (_DocTypeNameEx != null)
          _DocTypeNameEx.Value = DocTypeName;

        InitTextAndImage();
        Validate();
      }
    }

    private DBxDocType _DocType;

    #endregion

    #region DocTypeName

    /// <summary>
    /// Имя таблицы документов, из которого могут выбираться документы.
    /// Не может быть null или пустой строкой.
    /// Свойства DocType, DocTypeUI, DocTypeName и DocTableId синхронизированы.
    /// </summary>
    public string DocTypeName
    {
      get
      {
        return DocType.Name;
      }
      set
      {
        if (String.IsNullOrEmpty(value))
          throw new ArgumentNullException();
        DocType = UI.DocTypes[value].DocType;
      }
    }

    /// <summary>
    /// Управляемое свойство для DocTypeName.
    /// </summary>
    public DepValue<string> DocTypeNameEx
    {
      get
      {
        InitDocTypeNameEx();
        return _DocTypeNameEx;
      }
      set
      {
        InitDocTypeNameEx();
        _DocTypeNameEx.Source = value;
      }
    }

    private void InitDocTypeNameEx()
    {
      if (_DocTypeNameEx == null)
      {
        _DocTypeNameEx = new DepInput<string>(DocTypeName, DocTypeNameEx_ValueChanged);
        _DocTypeNameEx.OwnerInfo = new DepOwnerInfo(this, "DocTypeNameEx");
      }
    }

    private DepInput<string> _DocTypeNameEx;

    private void DocTypeNameEx_ValueChanged(object sender, EventArgs args)
    {
      DocTypeName = _DocTypeNameEx.Value;
    }

    #endregion

    #region TableId

    /// <summary>
    /// Идентификатор таблицы вида документов (свойство DBxDocType.TableId).
    /// Свойства DocType, DocTypeUI, DocTypeName и DocTableId синхронизированы.
    /// </summary>
    public Int32 DocTableId
    {
      get
      {
        return _DocType.TableId;
      }
      set
      {
        DBxDocType NewDocType = UI.DocProvider.DocTypes.FindByTableId(value);
        if (NewDocType == null)
        {
          if (value == 0)
            throw new ArgumentException("Идентификатор таблицы документов не может быть равен 0");
          else
            throw new ArgumentException("Неизвестный идентификатор таблицы документов " + value.ToString());
        }
        DocType = NewDocType;
      }
    }

    /// <summary>
    /// Идентификатор таблицы вида документов (свойство DBxDocType.TableId).
    /// Управляемое свойство для DocTableId
    /// </summary>
    public DepValue<Int32> DocTableIdEx
    {
      get
      {
        InitDocTableIdEx();
        return _DocTableIdEx;
      }
      set
      {
        InitDocTableIdEx();
        _DocTableIdEx.Source = value;
      }
    }

    private void InitDocTableIdEx()
    {
      if (_DocTableIdEx == null)
      {
        _DocTableIdEx = new DepInput<Int32>(DocTableId, DocTableIdEx_ValueChanged);
        _DocTableIdEx.OwnerInfo = new DepOwnerInfo(this, "DocTableIdEx");
      }
    }

    private DepInput<Int32> _DocTableIdEx;

    private void DocTableIdEx_ValueChanged(object sender, EventArgs args)
    {
      DocTableId = _DocTableIdEx.Value;
    }

    #endregion

    #endregion

    #region Выбранные идентификаторы документов

    /// <summary>
    /// Идентификаторы выбранных документов
    /// </summary>
    public virtual Int32[] DocIds
    {
      get { return base.Ids; }
      set { base.Ids = value; }
    }

    /// <summary>
    /// Идентификаторы выбранных документов.
    /// Управляемое свойство
    /// </summary>
    public DepValue<Int32[]> DocIdsEx
    {
      get { return base.IdsEx; }
      set { base.IdsEx = value; }
    }

    #endregion

    #region Вспомогательные свойства для идентификатора документа

    /// <summary>
    /// Идентификатор единственного выбранного документа.
    /// Если нет выбранных документов или выбрано больше одного документа, свойство возвращает 0.
    /// Установка свойства в 0 очищает список выбранных документов DocIds.
    /// Установка ненулевого значения делает выбранным единственный документ
    /// </summary>
    public Int32 SingleDocId
    {
      get { return base.SingleId; }
      set { base.SingleId = value; }
    }

    /// <summary>
    /// Идентификатор единственного выбранного документа.
    /// Управляемое свойство для SingleDocId
    /// </summary>
    public DepValue<Int32> SingleDocIdEx
    {
      get { return base.SingleIdEx; }
      set { base.SingleIdEx = value; }
    }

    #endregion

    #region EditorCaller

    /// <summary>
    /// Возможность задавать начальные значения при создании документа в выпадающем списке
    /// Если свойство не установлено (по умолчанию), то начальные значения определяются
    /// фильтрами (свойством GridFilters, если задано, или текущими установленными
    /// фильтрами в табличном просмотре справочника)
    /// </summary>
    public DocumentViewHandler EditorCaller
    {
      get { return _EditorCaller; }
      set { _EditorCaller = value; }
    }
    private DocumentViewHandler _EditorCaller;

    #endregion

    #region Автоматическая установка значения

    /// <summary>
    /// Если установлено в true, то при изменении фильтров проверяется число подходящих
    /// записей. Если ровно одна запись проходит условие фильтра, то устанавливается
    /// значение DocId.
    /// По умолчанию - false
    /// </summary>
    public bool AutoSelectByFilter
    {
      get { return _AutoSelectByFilter; }
      set
      {
        _AutoSelectByFilter = value;
        if (value)
          SelectByFilter();
      }
    }
    private bool _AutoSelectByFilter;

    /// <summary>
    /// Вызывается при изменении фильтров.
    /// Выполняет выбор записи, если установлено свойство AutoSelectByFilter
    /// </summary>
    protected override void OnFiltersChanged()
    {
      if (AutoSelectByFilter)
      {
        if (SelectByFilter())
          return;
      }

      base.OnFiltersChanged();
    }

    /// <summary>
    /// Установить значение DocIds, проходящее условия фильтра, если имеется только один
    /// такой документ.
    /// </summary>
    /// <returns>true, если есть единственный документ, false, если обнаружено больше
    /// одного подходящего документа или не найдено ни одного подходящего</returns>
    public bool SelectByFilter()
    {
      DBxFilter Filter = Filters.GetSqlFilter();
      if (UI.DocProvider.DocTypes.UseDeleted) // 23.05.2021
        Filter &= DBSDocType.DeletedFalseFilter;
      Int32 NewId = UI.DocProvider.FindRecord(DocTypeName, Filter, true);
      if (NewId == 0)
        return false;

      DocIds = new Int32[] { NewId };
      return true;
    }

    #endregion

    #region Текст и значок

    /// <summary>
    /// Возвращает текстовое представление для выбранных документов.
    /// В зависимости от количества выбранных документов и свойства MaxTextItemCount,
    /// возвращаются либо текстовые представления документов черех запятую, либо общее количество
    /// выбранных документов
    /// </summary>
    /// <returns>Текст для комбоблока</returns>
    protected override string DoGetText()
    {
      if (DocIds.Length == 1)
        return UI.TextHandlers.GetTextValue(DocType.Name, DocIds[0]);
      else if (DocIds.Length <= MaxTextItemCount)
      {
        string[] a = new string[DocIds.Length];
        for (int i = 0; i < DocIds.Length; i++)
          a[i] = UI.TextHandlers.GetTextValue(DocType.Name, DocIds[i]);
        return String.Join(", ", a);
      }
      else
        return DocType.PluralTitle + " (" + DocIds.Length.ToString() + ")";
    }

    /// <summary>
    /// Возвращает имя значка из EFPApp.MainImages для выбранного документа.
    /// Если выбрано несколько документов и для них определены неодинаковые значки,
    /// то возвращается значок "DBxDocSelection".
    /// Метод не вызывается, если нет выбранных документов.
    /// </summary>
    /// <returns>Имя изображения</returns>
    protected override string DoGetImageKey()
    {
      if (DocIds.Length < 1)
        return "UnknownState"; // ошибка
      string ImageKey = UI.DocTypes[DocTypeName].GetImageKey(DocIds[0]);
      for (int i = 1; i < DocIds.Length; i++)
      {
        string ImageKey2 = UI.DocTypes[DocTypeName].GetImageKey(DocIds[i]);
        if (ImageKey2 != ImageKey)
          return "DBxDocSelection";
      }
      return ImageKey;
    }

    /// <summary>
    /// Возвращает цвет и признак выделения серым цветом, если выбран ровно один документ.
    /// Если выбрано несколько документов, используется
    /// обычная раскраска комбоблока.
    /// Метод не вызывается, если нет выбранных документов.
    /// </summary>
    /// <param name="colorType">Сюда помещается цвет для строки документа</param>
    /// <param name="grayed">Сюда записывается true, если документ выделяется серым цветом</param>
    protected override void DoGetValueColor(out EFPDataGridViewColorType colorType, out bool grayed)
    {
      if (DocIds.Length == 1)
        UI.DocTypes[DocTypeName].GetRowColor(DocIds[0], out colorType, out grayed);
      else
        base.DoGetValueColor(out colorType, out grayed);
    }

    /// <summary>
    /// Возвращает всплывающую подсказку для выбранного документа, если он один.
    /// Иначе возвращается текст "Выбрано документов: X".
    /// Метод не вызывается, если нет выбранных документов.
    /// </summary>
    /// <returns>Текст для всплывающей подсказки</returns>
    protected override string DoGetValueToolTipText()
    {
      if (DocIds.Length == 1)
        return UI.DocTypes[DocTypeName].GetToolTipText(DocIds[0]);
      else
        return "Выбрано документов: " + DocIds.Length.ToString();
    }

    /// <summary>
    /// Доопределяем доступность кнопки "Редактировать"
    /// </summary>
    protected override void InitTextAndImage()
    {
      base.InitTextAndImage();

      if (DocType == null || DocIds.Length != 1)
      {
        Control.EditButtonEnabled = false;
        if (Selectable)
          Control.EditButtonKind = UserComboBoxEditButtonKind.Edit;
        else
          Control.EditButtonKind = UserComboBoxEditButtonKind.View;
        Control.EditButtonToolTipText = "Нельзя редактировать, т.к. нет выбранного документа";
        return;
      }

      switch (UI.DocProvider.DBPermissions.TableModes[DocTypeName])
      {
        case DBxAccessMode.Full:
          Control.EditButtonEnabled = true;
          if (Selectable)
          {
            Control.EditButtonKind = UserComboBoxEditButtonKind.Edit;
            Control.EditButtonToolTipText = "Редактировать выбранный документ \"" + DocType.SingularTitle + "\"";
          }
          else
          {
            // Не стоит вызывать DocType.TestEditable(), т.к. будет медленно
            Control.EditButtonKind = UserComboBoxEditButtonKind.View;
            Control.EditButtonToolTipText = "Просмотреть выбранный документ \"" + DocType.SingularTitle + "\"";
          }
          break;

        case DBxAccessMode.ReadOnly:
          Control.EditButtonEnabled = true;
          Control.EditButtonKind = UserComboBoxEditButtonKind.View;
          Control.EditButtonToolTipText = "Просмотреть выбранный документ \"" + DocType.SingularTitle + "\"";
          if (Selectable)
            Control.EditButtonToolTipText += ". У Вас нет прав для редактирования документов";
          break;

        case DBxAccessMode.None:
          Control.EditButtonEnabled = false;
          Control.EditButtonKind = UserComboBoxEditButtonKind.View;
          Control.EditButtonToolTipText += "У Вас нет прав для просмотра документов \"" + DocType.PluralTitle + "\"";
          break;
      }
    }

    #endregion

    #region Выпадающий список

    /// <summary>
    /// Показывает блок диалога для выбора нескольких документов.
    /// Используется метод DocTypeUI.SelectDocs().
    /// Затем устанавливается свойство DocIds.
    /// </summary>
    protected override void DoPopup()
    {
      if (_DocType == null)
      {
        EFPApp.ShowTempMessage("Тип документа не задан");
        return;
      }

      DocSelectDialog dlg = new DocSelectDialog(DocTypeUI);
      dlg.SelectionMode = DocSelectionMode.MultiList;
      if (!String.IsNullOrEmpty(DisplayName))
        dlg.Title = DisplayName;
      dlg.CanBeEmpty = CanBeEmpty;
      if (Filters.Count > 0)
        dlg.Filters = Filters; // Иначе будут отключены стандартные фильтры
      dlg.DocIds = DocIds;
      dlg.EditorCaller = EditorCaller;
      dlg.DialogPosition.PopupOwnerControl = Control;
      if (dlg.ShowDialog() != DialogResult.OK)
        return;
      DocIds = dlg.DocIds;
    }

    ///// <summary>
    ///// Получить значение поля для выбранного документа. Возвращает null, если
    ///// DocIdValue=0
    ///// </summary>
    ///// <param name="ColumnName">Имя поля, значение которого нужно получить</param>
    ///// <returns>Значение поля</returns>
    //public override object GetColumnValue(string ColumnName)
    //{
    //  if (DocType == null || DocId == 0)
    //    return null;
    //  return UI.TextHandlers.DBCache[DocType.Name].GetValue(DocId, ColumnName);
    //}

    /// <summary>
    /// Открытие на редактирование текущего выбранного документа
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void Control_EditClick(object sender, EventArgs args)
    {
      try
      {
        switch (DocIds.Length)
        {
          case 0:
            EFPApp.ShowTempMessage("Документ не выбран");
            break;
          case 1:
            UI.DocTypes[DocType.Name].PerformEditing(DocIds[0], Control.EditButtonKind == UserComboBoxEditButtonKind.View);
            InitTextAndImage();
            SetDeletedChanged();
            Validate();
            DocIdsEx.OnValueChanged();
            break;
          default:
            EFPApp.ShowTempMessage("Выбрано несколько документов");
            break;
        }
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка редактирования документа");
      }
    }

    #endregion

    #region Другие методы

    /// <summary>
    /// Возвращает DBxDocType.PluralTitle вместо "Без названия"
    /// </summary>
    protected override string DefaultDisplayName
    {
      get { return DocTypeUI.DocType.PluralTitle; }
    }

    /// <summary>
    /// Возвращает true, если документ с идентификатором <paramref name="id"/> помечен на удаление
    /// </summary>
    /// <param name="id">Идентификатор проверяемого документа</param>
    /// <param name="message">Сюда записывается сообщение "Выбранный документ удален"</param>
    /// <returns>Пометка на удаление</returns>
    protected override bool GetDeletedValue(Int32 id, out string message)
    {
      if (!UI.DocProvider.DocTypes.UseDeleted)
      {
        message = null;
        return false;
      }

      if (DataTools.GetBool(UI.TextHandlers.DBCache[DocType.Name].GetBool(id, "Deleted")))
      {
        message = "Выбранный документ \"" + DocType.SingularTitle + "\" удален";
        return true;
      }
      else
      {
        message = null;
        return false;
      }
    }

    /// <summary>
    /// Возвращает true, если документ проходит все фильтры в списке Filters
    /// </summary>
    /// <param name="id">Идентификатор проверяемого документа</param>
    /// <param name="badFilter">Сюда записывается ссылка на первый фильтр, который не пропускает документ</param>
    /// <returns>Прохождение фильтров</returns>
    protected override bool DoTestFilter(Int32 id, out DBxCommonFilter badFilter)
    {
      badFilter = null;
      if (DocType == null)
        return true;

      // Получаем данные для фильтрации
      DBxColumnList ColList = new DBxColumnList();
      Filters.GetColumnNames(ColList);
      DBxColumns ColumnNames = new DBxColumns(ColList);

      object[] Values = UI.TextHandlers.DBCache[DocTypeName].GetValues(id, ColumnNames);
      return Filters.TestValues(ColumnNames, Values, out badFilter);
    }

    #endregion

    #region Выборка документов

    /// <summary>
    /// Возвращает true
    /// </summary>
    public override bool GetDocSelSupported { get { return true; } }

    /// <summary>
    /// Возвращает true
    /// </summary>
    public override bool SetDocSelSupported { get { return true; } }

    /// <summary>
    /// Получение выборки документов
    /// </summary>
    /// <param name="reason">Причина получения</param>
    /// <returns>Выборка</returns>
    protected override DBxDocSelection OnGetDocSel(EFPDBxGridViewDocSelReason reason)
    {
      DBxDocSelection DocSel = new DBxDocSelection(UI.DocProvider.DBIdentity);
      if (DocType != null && DocIds.Length > 0)
        UI.DocTypes[DocType.Name].PerformGetDocSel(DocSel, DocIds, reason);
      return DocSel;
    }

    /// <summary>
    /// Установка выборки документов.
    /// Если в выборке <paramref name="docSel"/> есть документы DocTypeName, то
    /// они устанавливаются в качестве выбранных (свойство DocIds), полностью заменяя выбранные ранее.
    /// Иначе выдается сообщение об ошибке и текущий выбор не меняется.
    /// </summary>                                
    /// <param name="docSel">Выборка документов</param>
    protected override void OnSetDocSel(DBxDocSelection docSel)
    {
      Int32[] NewIds = docSel[DocTypeName];
      if (NewIds.Length == 0)
        EFPApp.ShowTempMessage("В буфере обмена нет ссылки на документ \"" + DocType.SingularTitle + "\"");
      else
        DocIds = NewIds;
    }

    /// <summary>
    /// Возвращает true
    /// </summary>
    public override bool DocInfoSupported { get { return true; } }

    #endregion

    #region Проверка идентификатора документа

    /// <summary>
    /// Проверяет возможность присвоения заданного идентификатора документа <paramref name="docId"/> без реальной установки свойства DocIds.
    /// Возвращает false, если документ удален, а свойство CanBeDeleted=false 
    /// (значение по умолчанию). Также возвращает false, если документ не проходит условие какого-либо фильтра в списке Filters.
    /// </summary>
    /// <param name="docId">Идентификатор проверяемого документа</param>
    /// <returns>Возможность присвоения идентификатора</returns>
    public bool TestDocId(Int32 docId)
    {
      string Message;
      return TestDocId(docId, out Message);
    }

    /// <summary>
    /// Проверяет возможность присвоения заданного идентификатора документа <paramref name="docId"/> без реальной установки свойства DocIds.
    /// Возвращает false, если документ удален, а свойство CanBeDeleted=false
    /// (значение по умолчанию). Также возвращает false, если документ не проходит условие какого-либо фильтра в списке Filters.
    /// </summary>
    /// <param name="docId">Идентификатор проверяемого документа</param>
    /// <param name="message">Сюда записывается сообщение об ошибке, если присвоение невозможно</param>
    /// <returns>Возможность присвоения идентификатора</returns>
    public bool TestDocId(Int32 docId, out string message)
    {
      if (docId == 0)
      {
        if (CanBeEmpty)
        {
          message = null;
          return true;
        }

        message = "Не задан идентификатор документа";
        return false;
      }
      UI.DocProvider.CheckIsRealDocId(docId);


      if (UI.DocProvider.DocTypes.UseDeleted)
      {
        if (DataTools.GetBool(UI.TextHandlers.DBCache[DocType.Name].GetBool(docId, "Deleted")))
        {
          if (!(CanBeDeleted))
          {
            message = "Документ \"" + DocType.SingularTitle + "\" удален";
            return false;
          }
        }
      }

      DBxCommonFilter BadFilter;
      if (!DoTestFilter(docId, out BadFilter))
      {
        message = "Документ не проходит фильтр \"" + BadFilter.DisplayName + "\"";
        return false;
      }

      message = null;
      return true;
    }


    #endregion
  }


  /// <summary>
  /// Обработчик для комбоблока, предназначенного для выбора нескольких поддокументов из одного документа
  /// Поддокументы должны быть сохранены в базе данных, а не относиться к текущему редактируемому документу
  /// </summary>
  public class EFPMultiSubDocComboBox : EFPMultiDocComboBoxBase
  {
    #region Конструкторы

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    /// <param name="subDocTypeUI">Пользовательский интерфейс для доступа к поддокументам</param>
    public EFPMultiSubDocComboBox(EFPBaseProvider baseProvider, UserSelComboBox control, SubDocTypeUI subDocTypeUI)
      : base(baseProvider, control, subDocTypeUI.UI)
    {
      //if (subDocTypeUI == null)
      //  throw new ArgumentNullException("SubDocTypeUI");

      this._SubDocTypeUI = subDocTypeUI;

      control.PopupButtonToolTipText = "Выбрать: " + subDocTypeUI.SubDocType.PluralTitle; // 13.06.2021
      control.ClearButtonToolTipText = "Очистить поле выбора";

      _DocId = 0;
      _DocIdWasSet = false;
    }

    /// <summary>
    /// Создает провайдер, связанный с уже добавленным провайдером комбоблока выбора документа.
    /// Кроме обычной инициализации, устанавливает связь для свойства DocIdEx
    /// </summary>
    /// <param name="docComboBoxProvider">Провайдер комбоблока выбора документа</param>
    /// <param name="control">Управляющий элемент</param>
    /// <param name="subDocTypeName">Имя вида поддокументов в <paramref name="docComboBoxProvider"/>.DocType.SubDocs.</param>
    public EFPMultiSubDocComboBox(EFPDocComboBox docComboBoxProvider, UserSelComboBox control, string subDocTypeName)
      : this(docComboBoxProvider.BaseProvider, control, GetSubDocTypeUI(docComboBoxProvider, subDocTypeName))
    {
      this.DocIdEx = docComboBoxProvider.DocIdEx;
    }

    private static SubDocTypeUI GetSubDocTypeUI(EFPDocComboBox docComboBoxProvider, string subDocTypeName)
    {
      if (String.IsNullOrEmpty(subDocTypeName))
        throw new ArgumentNullException("subDocTypeName");
      return docComboBoxProvider.DocTypeUI.SubDocTypes[subDocTypeName];
    }

    #endregion

    #region Вспомогательные свойства

    /// <summary>
    /// Объект пользовательского интерфейса для вида поддокументов.
    /// Не может быть null.
    /// Свойства SubDocTypeUI и SubDocTypeName синхронизированы.
    /// </summary>
    public SubDocTypeUI SubDocTypeUI
    {
      get { return _SubDocTypeUI; }
      set
      {
        if (value == null)
          throw new ArgumentNullException();
        if (value == _SubDocTypeUI)
          return;
        _SubDocTypeUI = value;
        SubDocIds = DataTools.EmptyIds;
        InitTextAndImage();
      }
    }
    private SubDocTypeUI _SubDocTypeUI;

    /// <summary>
    /// Имя таблицы поддокументов.
    /// Свойства SubDocType, SubDocTypeUI и SubDocTypeName синхронизированы.
    /// </summary>
    public string SubDocTypeName
    {
      get
      {
        if (_SubDocTypeUI == null)
          return "";
        else
          return SubDocTypeUI.SubDocType.Name;
      }
    }

    /// <summary>
    /// Описание вида документа, к которому относятся поддокументы
    /// </summary>
    public DBxDocType DocType { get { return _SubDocTypeUI.DocType; } }

    /// <summary>
    /// Описание вида поддокументов
    /// </summary>
    public DBxSubDocType SubDocType { get { return _SubDocTypeUI.SubDocType; } }

    #endregion

    #region Свойство DocId

    /// <summary>
    /// Идентификатор документа, из которого выбираются поддокументы.
    /// Если свойство установлено в явном виде, то попытка выбрать "чужие" поддокументы
    /// приводит к индикации ошибки.
    /// Если свойство не установлено в явном виде, то разрешается установка произвольного значения
    /// свойства SubDocIds (при условии, что все поддокументы относятся к одному документу), 
    /// при этом возвращаемое значение будет меняться
    /// </summary>
    public virtual Int32 DocId
    {
      get { return _DocId; }
      set
      {
        if (value != DocId || (!_DocIdWasSet))
        {
          _DocId = value;
          _DocIdWasSet = true;

          if (_OutDocIdEx != null)
            _OutDocIdEx.OwnerSetValue(value);
          InitSubDocIdsOnDocId();
        }
        else
        {
          // Свойство DocId может повторно устанавливаться из связанного EFPDocComboBox.DocId
          // после редактирования основного объекта, выполненного пользователем
          // Возможно, появился поддокумент, из которого можно выбрать; 
          // например, расчетный счет организации
          if (SubDocIds.Length == 0)
            InitSubDocIdsOnDocId();
        }

        Validate();
      }
    }
    /// <summary>
    /// Текущее значение свойства
    /// </summary>
    private Int32 _DocId;

    /// <summary>
    /// true, если свойство было установлено в явном виде снаружи, false, если свойство извлекается из SubDocId
    /// </summary>
    private bool _DocIdWasSet;


    /// <summary>
    /// Внутренний метод установки свойства DocId.
    /// Не предназначен для установки из пользовательского кода
    /// </summary>
    /// <param name="value">Идентификатор документа</param>
    protected void InternalSetDocId(Int32 value)
    {
      if (_DocIdWasSet)
      {
        if (_DocId != 0)
          return; // изменение внешнего значения не допускается
      }
      _DocId = value;
      _DocIdWasSet = false;

      if (_OutDocIdEx != null)
        _OutDocIdEx.OwnerSetValue(value);
    }

    private void InitSubDocIdsOnDocId()
    {
      if (_InsideSetSubDocIds)
        return;
      SubDocIds = DataTools.EmptyIds;
      if ((DocId != 0) && (SubDocTypeUI != null))
      {
        if (AutoSetAll)
        {
          SubDocIds = SubDocTypeUI.GetSubDocIds(DocId);
          return;
        }

        if (InitDefSubDocs != null)
          InitDefSubDocs(this, EventArgs.Empty);
      }
    }

    /// <summary>
    /// Управляемой свойство для DocId
    /// </summary>
    public DepValue<Int32> DocIdEx
    {
      get
      {
        InitOutDocIdEx();
        return _OutDocIdEx;
      }
      set
      {
        InitInDocIdEx();
        _InDocIdEx.Source = value;
      }
    }
    private DepInput<Int32> _InDocIdEx;

    private void InitInDocIdEx()
    {
      if (_InDocIdEx == null)
      {
        _InDocIdEx = new DepInput<int>(0, InDocIdEx_ValueChanged);
        _InDocIdEx.OwnerInfo = new DepOwnerInfo(this, "InDocIdEx");
      }
    }


    void InDocIdEx_ValueChanged(object sender, EventArgs args)
    {
      DocId = _InDocIdEx.Value; // жесткая установка
    }

    private void InitOutDocIdEx()
    {
      if (_OutDocIdEx == null)
      {
        _OutDocIdEx = new DepOutput<Int32>(DocId);
        _OutDocIdEx.OwnerInfo = new DepOwnerInfo(this, "OutDocIdEx");
      }
    }
    private DepOutput<Int32> _OutDocIdEx;

    /// <summary>
    /// Свойство возвращает источник, управляющий текущим основным документом 
    /// (то есть значение, которое было присвоено свойству DocIdEx)
    /// или null, если внешнего управления нет
    /// 
    /// Пилотное свойство. Возможно, такие конструкции надо приделать всем 
    /// управляемым свойствам всех провайдеров
    /// </summary>
    public DepValue<Int32> DocIdExSource
    {
      get
      {
        if (_InDocIdEx == null)
          return null;
        else
          return _InDocIdEx.Source;
      }
    }

    /// <summary>
    /// Вызывается после установки ненулевого значения свойства
    /// DocId (при этом SubDocIds сбрасывается). Обработчик может
    /// установить желаемые SubDocIds. 
    /// Событие не вызывается, если установлено свойство AutoSetAll.
    /// </summary>
    public event EventHandler InitDefSubDocs;

    #endregion

    #region Свойство SubDocIds

    /// <summary>
    /// Текущие выбранные поддокументы.
    /// Если нет ни одного выбранного поддокумента, возвращается пустой массив
    /// </summary>
    public virtual Int32[] SubDocIds
    {
      // Нужно обязательно использовать базовое свойство Id, т.к. к нему приделана обработка свойства IdEx
      get { return Ids; }
      set { Ids = value; }
    }


    /// <summary>
    /// Массив выбранных идентификаторов поддокументов.
    /// При установке свойства вызывается InternalSetDocId(), так как выбранный документ мог тоже иземениться
    /// </summary>
    protected internal override Int32[] Ids
    {
      get { return base.Ids; }
      set
      {
        if (_InsideSetSubDocIds)
          return;

        if (value == null)
          value = DataTools.EmptyIds;

        if (DataTools.AreArraysEqual<Int32>(value, base.Ids))
          return;

        _InsideSetSubDocIds = true;
        try
        {
          base.Ids = value;
          if (value.Length > 0)
            InternalSetDocId(SubDocTypeUI.TableCache.GetInt(value[0], "DocId"));
          else
            InternalSetDocId(0);
        }
        finally
        {
          _InsideSetSubDocIds = false;
        }

        Validate();
      }
    }

    private bool _InsideSetSubDocIds = false;

    /// <summary>
    /// Идентификаторы выбранных поддокументов.
    /// Управляемое свойство для SubDocIds
    /// </summary>
    public DepValue<Int32[]> SubDocIdsEx
    {
      get { return base.IdsEx; }
      set { base.IdsEx = value; }
    }

    #endregion

    #region Свойство AutoSetAll

    /// <summary>
    /// Если установать в true, то при установке значения DocId будет загружен список всех поддокументов 
    /// (кроме удаленных).
    /// Событие InitDefSubSoc не вызывается.
    /// Значение по умолчанию - false
    /// </summary>
    public bool AutoSetAll
    {
      get { return _AutoSetAll; }
      set
      {
        if (value == _AutoSetAll)
          return;
        _AutoSetAll = value;

        if (_AutoSetAllEx != null)
          _AutoSetAllEx.Value = value;

        if (value && (DocId != 0) && (SubDocIds.Length == 0) &&
          (SubDocTypeUI != null))
        {
          SubDocIds = SubDocTypeUI.GetSubDocIds(DocId);
        }

        Validate(); // 29.08.2016
      }
    }
    private bool _AutoSetAll;

    /// <summary>
    /// Если установать в true, то при установке значения DocId будет загружен список всех поддокументов 
    /// (кроме удаленных).
    /// Событие InitDefSubSoc не вызывается.
    /// Управляемое свойство для AutoSetAll.
    /// </summary>
    public DepValue<Boolean> AutoSetAllEx
    {
      get
      {
        InitAutoSetAllEx();
        return _AutoSetAllEx;
      }
      set
      {
        InitAutoSetAllEx();
        _AutoSetAllEx.Source = value;
      }
    }

    private void InitAutoSetAllEx()
    {
      if (_AutoSetAllEx == null)
      {
        _AutoSetAllEx = new DepInput<bool>(AutoSetAll, AutoSetAllEx_ValueChanged);
        _AutoSetAllEx.OwnerInfo = new DepOwnerInfo(this, "AutoSetAllEx");
      }
    }
    private DepInput<Boolean> _AutoSetAllEx;

    void AutoSetAllEx_ValueChanged(object sender, EventArgs args)
    {
      AutoSetAll = _AutoSetAllEx.Value;
    }

    #endregion

    #region Проверка

    /// <summary>
    /// Проверка корректности выбранных поддокументов.
    /// Кроме проверок базового класса, определяется, что все поддокументы в списке относятся
    /// к документу с идентификатором DocId.
    /// </summary>
    protected override void OnValidate()
    {
      base.OnValidate();
      if (base.ValidateState == UIValidateState.Error)
        return;

      // Проверяем, что поддокумент относится к выбранному документу
      Int32 DummyDocId = 0;
      for (int i = 0; i < SubDocIds.Length; i++)
      {
        Int32 DocId2 = SubDocTypeUI.TableCache.GetInt(SubDocIds[i], "DocId");
        if (DocId != 0)
        {
          if (DocId2 != DocId)
          {
            SetError("Выбранный поддокумент \"" + SubDocTypeUI.SubDocType.SingularTitle + "\" " +
              SubDocTypeUI.GetTextValue(SubDocIds[i]) + " относится к документу \"" +
              SubDocTypeUI.DocTypeUI.GetTextValue(DocId2) + "\", а не \"" + SubDocTypeUI.DocTypeUI.GetTextValue(DocId) + "\"");
            return;
          }
        }
        else
        {
          if (i == 0)
            DummyDocId = DocId2;
          else
          {
            if (DocId2 != DummyDocId)
            {
              SetError("Выбранный поддокументы относятся к разным документам");
              return;
            }
          }
        }
      }
    }

    #endregion

    #region Текст и значок

    /// <summary>
    /// Текстовое представление для выбранных поддокументов.
    /// Если выбрано поддокументов не более, чем определено свойством MaxTextItemCount,
    /// то возвращаются текстовые представления, разделенные запятыми.
    /// Если выбрано больше поддокументов, то возвращается только количество поддокументов.
    /// Метод не вызывается, если нет выбранных поддокументов.
    /// </summary>
    /// <returns>Строка для комбоблока</returns>
    protected override string DoGetText()
    {
      if (SubDocIds.Length == 1)
        return SubDocTypeUI.GetTextValue(SubDocIds[0]);
      else if (SubDocIds.Length <= MaxTextItemCount)
      {
        string[] a = new string[SubDocIds.Length];
        for (int i = 0; i < SubDocIds.Length; i++)
          a[i] = SubDocTypeUI.GetTextValue(SubDocIds[i]);
        return String.Join(", ", a);
      }
      else
        return SubDocType.PluralTitle + " (" + SubDocIds.Length.ToString() + ")";
    }

    /// <summary>
    /// Возвращает имя изображения из списка EFPApp.MainImages.
    /// Если выбран один поддокумент или для выбранных поддокументов задан одинаковый значок,
    /// то он возвращается.
    /// Иначе возвращается "DBxDocSelection".
    /// Метод не вызывается, если нет выбранных поддокументов.
    /// </summary>
    /// <returns>Значок для комбоблока</returns>
    protected override string DoGetImageKey()
    {
      if (SubDocIds.Length < 1)
        return "UnknownState"; // ошибка
      string ImageKey = SubDocTypeUI.GetImageKey(SubDocIds[0]);
      for (int i = 1; i < SubDocIds.Length; i++)
      {
        string ImageKey2 = SubDocTypeUI.GetImageKey(SubDocIds[i]);
        if (ImageKey2 != ImageKey)
          return "DBxDocSelection";
      }
      return ImageKey;
    }

    /// <summary>
    /// Получить цветовое оформление для комбоблока.
    /// Если выбрано больше одного поддокумента, то используется стандартное оформление
    /// </summary>
    /// <param name="colorType">Сюда записывается цвет, определенный для документа</param>
    /// <param name="grayed">Получает значение True, если поддокумент выделяется серым цветом</param>
    protected override void DoGetValueColor(out EFPDataGridViewColorType colorType, out bool grayed)
    {
      if (SubDocIds.Length == 1)
        SubDocTypeUI.GetRowColor(SubDocIds[0], out colorType, out grayed);
      else
        base.DoGetValueColor(out colorType, out grayed);
    }

    /// <summary>
    /// Возвращает текст всплывающей подсказки для выбранного поддокумента.
    /// Если выбрано несколько подоокументов, возвращает "Выбрано поддокументов: X".
    /// </summary>
    /// <returns>Текст всплывающей подсказки</returns>
    protected override string DoGetValueToolTipText()
    {
      if (SubDocIds.Length == 1)
        return SubDocTypeUI.GetToolTipText(SubDocIds[0]);
      else
        return "Выбрано поддокументов: " + SubDocIds.Length.ToString();
    }

    #endregion

    #region Выпадающий список

    /// <summary>
    /// Показывает диалог выбора одного или нескольких поддокументов для заданного документа 
    /// c помощью SubDocTypeUI.SelectSubDocs().
    /// Затем устанавливается свойство SubDocIds.
    /// </summary>
    protected override void DoPopup()
    {
      if (DocId == 0)
      {
        EFPApp.ShowTempMessage("Не задан документ \"" + DocType.SingularTitle + "\", из которого можно выбирать");
        return;
      }

      Int32[] ThisSubDocIds = SubDocIds;

      DBxDocSet DocSet = new DBxDocSet(UI.DocProvider);
      DBxSingleDoc Doc = DocSet[DocType.Name].View(DocId);

      SubDocSelectDialog dlg = new SubDocSelectDialog(SubDocTypeUI, Doc.SubDocs[SubDocTypeName].SubDocs);
      dlg.SelectionMode = DocSelectionMode.MultiSelect;
      if (!String.IsNullOrEmpty(DisplayName))
        dlg.Title = DisplayName;
      dlg.CanBeEmpty = CanBeEmpty;
      dlg.SubDocIds = SubDocIds;
      dlg.DialogPosition.PopupOwnerControl = Control;

      if (dlg.ShowDialog() != DialogResult.OK)
        return;

      if (DataTools.AreArraysEqual<Int32>(dlg.SubDocIds, SubDocIds))
        InitTextAndImage();
      else
        SubDocIds = dlg.SubDocIds;
    }

    #endregion

    #region Другие методы

    /// <summary>
    /// Возвращает DBxSubDocType.PluralTitle вместо "Без названия"
    /// </summary>
    protected override string DefaultDisplayName
    {
      get { return SubDocTypeUI.SubDocType.PluralTitle; }
    }

    /// <summary>
    /// Возвращает true, если поддокумент с заданным идентификатором или документ,
    /// к которому он относится, помечены на удаление
    /// </summary>
    /// <param name="subDocId">Идентификатор поддокумента</param>
    /// <param name="message">Сюда записывается сообщение, если поддокумент удален</param>
    /// <returns>True, если поддокумент удален</returns>
    protected override bool GetDeletedValue(Int32 subDocId, out string message)
    {
      if (!UI.DocProvider.DocTypes.UseDeleted)
      {
        message = null;
        return false;
      }

      object[] a = SubDocTypeUI.GetValues(subDocId, "Deleted,DocId");
      if (DataTools.GetBool(a[0]))
      {
        message = "Выбранный поддокумент \"" + SubDocType.SingularTitle + "\" удален";
        return true; // удален поддокумент
      }
      Int32 DocId = DataTools.GetInt(a[1]);
      if (DataTools.GetBool(UI.TextHandlers.DBCache[DocType.Name].GetBool(DocId, "Deleted")))
      {
        string DocText;
        try
        {
          DocText = UI.DocTypes[DocType.Name].GetTextValue(DocId) + " (DocId=" + DocId.ToString() + ")";
        }
        catch (Exception e)
        {
          DocText = "Id=" + DocId.ToString() + ". Ошибка получения текста: " + e.Message;
        }
        message = "Документ \"" + DocType.SingularTitle + "\" (" + DocText + "), к которому относится выбранный поддокумент, удален";
        return true;
      }
      else
      {
        message = null;
        return false;
      }
    }

    #endregion

    #region Выборка документов

    /// <summary>
    /// Возвращает значение свойства SubDocTypeUI.HasGetDocSel, так как не у всех видов
    /// поддокументов есть ссылочные поля на документы, а сами поддокументы не образуют выборки.
    /// 
    /// Вставка выборки из буфера обмена не предусмотрена
    /// </summary>
    public override bool GetDocSelSupported { get { return SubDocTypeUI.HasGetDocSel; } }

    /// <summary>
    /// Вызывает SubDocTypeUI.PerformGetDocSel() для всех выбранных поддокументов
    /// </summary>
    /// <param name="reason">Причина создания выборки</param>
    /// <returns>Выборка документов</returns>
    protected override DBxDocSelection OnGetDocSel(EFPDBxGridViewDocSelReason reason)
    {
      DBxDocSelection DocSel = new DBxDocSelection(UI.DocProvider.DBIdentity);
      for (int i = 0; i < SubDocIds.Length; i++)
      {
        SubDocTypeUI.PerformGetDocSel(DocSel, SubDocIds[i], reason);
      }

      DocSel.Add(DocType.Name, DocId);
      return DocSel;
    }

    #endregion
  }
}
