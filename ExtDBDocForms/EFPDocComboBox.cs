// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Windows.Forms;
using FreeLibSet.Forms;
using System.Data;
using System.Drawing;
using System.Collections.Generic;
using FreeLibSet.DependedValues;
using FreeLibSet.Data.Docs;
using FreeLibSet.Data;
using FreeLibSet.Controls;
using FreeLibSet.Core;
using FreeLibSet.UICore;

namespace FreeLibSet.Forms.Docs
{
  #region Делегаты

  /// <summary>
  /// Аргументы события <see cref="EFPDocComboBoxBase.TextValueNeeded"/>
  /// </summary>
  public class EFPDocComboBoxTextValueNeededEventArgs : EFPComboBoxTextValueNeededEventArgs
  {
    #region Конструктор

    /// <summary>
    /// Создается в <see cref="EFPDocComboBoxBase"/>.
    /// </summary>
    /// <param name="owner"></param>
    public EFPDocComboBoxTextValueNeededEventArgs(EFPDocComboBoxBase owner)
    {
      _Owner = owner;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Выбранный идентификатор
    /// </summary>
    public Int32 Id { get { return _Owner.Id; } }

    private readonly EFPDocComboBoxBase _Owner;

    #endregion
  }

  /// <summary>
  /// Делегат события <see cref="EFPDocComboBoxBase.TextValueNeeded"/>
  /// </summary>
  /// <param name="sender">Провайдер комбоблока, производный от <see cref="EFPDocComboBoxBase"/></param>
  /// <param name="args">Аргументы события</param>
  public delegate void EFPDocComboBoxTextValueNeededEventHandler(object sender,
    EFPDocComboBoxTextValueNeededEventArgs args);

  #endregion

  /// <summary>
  /// Базовый класс обработчиков для комбоблока, предназначенного для выбора одного документа или 
  /// поддокумента. Для выбора нескольких ссылок используйте <see cref="EFPMultiDocComboBoxBase"/>.
  /// </summary>
  public abstract class EFPDocComboBoxBase : EFPAnyDocComboBoxBase, IDepSyncObject
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    /// <param name="ui">Интерфейс доступа к документам</param>
    protected EFPDocComboBoxBase(EFPBaseProvider baseProvider, UserSelComboBox control, DBUI ui)
      : base(baseProvider, control, ui)
    {
      _TextValueNeededArgs = new EFPDocComboBoxTextValueNeededEventArgs(this);
    }

    #endregion

    #region Свойство Id - идентификатор строки

    /// <summary>
    /// Идентификатор выбранного документа или поддокумента
    /// </summary>
    internal protected virtual Int32 Id
    {
      get { return _Id; }
      set
      {
        if (value == _Id)
          return;
        _Id = value;
        if (_IdEx != null)
          _IdEx.Value = value;
        if (_DeletedEx != null)
          _DeletedEx.SetDelayed();
        InitTextAndImage();
        ClearButtonEnabled = (_Id != 0);
        if (IdValueChangedBeforeValidate != null)
          IdValueChangedBeforeValidate(this, EventArgs.Empty);
        Validate();
        DoSyncValueChanged();

        if (CommandItems is EFPAnyDocComboBoxBaseCommandItems)
          ((EFPAnyDocComboBoxBaseCommandItems)CommandItems).InitEnabled(); // 21.01.2016
      }
    }
    private Int32 _Id;

    /// <summary>
    /// Идентификатор строки выбранного документа. В классах-наследниках свойство
    /// переименовывается.
    /// </summary>
    internal protected DepValue<Int32> IdEx
    {
      get
      {
        InitIdEx();
        return _IdEx;
      }
      set
      {
        InitIdEx();
        _IdEx.Source = value;
      }
    }

    private void InitIdEx()
    {
      if (_IdEx == null)
      {
        _IdEx = new DepInput<Int32>(Id, IdEx_ValueChanged);
        _IdEx.OwnerInfo = new DepOwnerInfo(this, "IdEx");
      }
    }
    private DepInput<Int32> _IdEx;

    /// <summary>
    /// 16.12.2008
    /// Это событие вызывается при изменении текущего значения идентификатора, но
    /// до вызова метода Validate(). Ссылочные объекты на значения полей будут
    /// обновлены до проверки корректности и могут быть использованы в ней
    /// </summary>
    internal event EventHandler IdValueChangedBeforeValidate;

    private void IdEx_ValueChanged(object sender, EventArgs args)
    {
      Id = _IdEx.Value;
    }

    /// <summary>
    /// Возвращает true при <see cref="Id"/> не равном 0.
    /// </summary>
    public override bool IsNotEmpty { get { return Id != 0; } }

    /// <summary>
    /// Объект-функция возвращает true, если идентификатор не равен 0.
    /// Управляемое свойство для <see cref="IsNotEmpty"/>.
    /// </summary>
    public DepValue<bool> IsNotEmptyEx
    {
      get
      {
        if (_IsNotEmptyEx == null)
          _IsNotEmptyEx = new DepExpr1<bool, Int32>(IdEx, CalcIsNotEmpty);
        return _IsNotEmptyEx;
      }
    }
    private DepValue<bool> _IsNotEmptyEx;

    private static bool CalcIsNotEmpty(Int32 id)
    {
      return id != 0;
    }

    #endregion

    #region Событие TextValueNeeded

    /// <summary>
    /// Это событие вызывается после выбора значения из списка или установки свойства
    /// <see cref="Id"/> и позволяет переопределить текст в комбоблоке, текст всплываюующей подсказки
    /// и изображение. Событие вызывается в том числе и при <see cref="Id"/>=0.
    /// Также вызывается при обращении к свойству <see cref="Text"/>.
    /// </summary>
    public event EFPDocComboBoxTextValueNeededEventHandler TextValueNeeded
    {
      // 18.03.2016
      // После установки обработчика требуется обновить текст, т.к. обработчик может изменить текст для Id=0
      // или свойство Id могло быть уже установлено до присоединения обработчика.
      // Альтернатива:
      // Вызвать InitTextAndImage() из OnShown(), но тогда свойства TextValue и прочие будут иметь
      // некорректное значение до вывода на экран.
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
    private EFPDocComboBoxTextValueNeededEventHandler _TextValueNeeded;

    #endregion

    #region InitTextAndImage

    /// <summary>
    /// Чтобы не создавать объект каждый раз, создаем его в конструкторе.
    /// Также используем для хранения изображения между вызовом InitText() и
    /// его выводом в комбоблоке
    /// </summary>
    private EFPDocComboBoxTextValueNeededEventArgs _TextValueNeededArgs;

    /// <summary>
    /// Установка текста элемента
    /// EFPDocComboBox доопределяет метод для установки доступности кнопки "Edit".
    /// </summary>
    protected override void InitTextAndImage()
    {
      try
      {
        _TextValueNeededArgs.Clear();
        // Стандартные значения текста, подсказки и изображения
        if (Id == 0)
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
        base.InitTextAndImage(_TextValueNeededArgs);

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
        Control.Text = "Id=" + Id.ToString() + " " + Control.Text;
    }

    /// <summary>
    /// Получение текста для текущего значения, если <see cref="Id"/>!=0.
    /// </summary>
    /// <returns>Текстовое представление</returns>
    protected abstract string DoGetText();

    /// <summary>
    /// Получение изображения для текущего значения, если <see cref="Id"/>!=0.
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
    /// Получение подсказки для текущего значения, если <see cref="Id"/>!=0.
    /// </summary>
    /// <returns>Подсказка по значению</returns>
    protected virtual string DoGetValueToolTipText()
    {
      return String.Empty;
    }

    #endregion

    #region Свойство Deleted

    /// <summary>
    /// Возвращает true, если выбранный документ или поддокумент удален.
    /// Если документ не выбран, то возвращается false.
    /// </summary>
    public bool Deleted
    {
      get
      {
        if (Id == 0)
          return false;
        else
        {
          string Message;
          return GetDeletedValue(out Message);
        }
      }
    }

    /// <summary>
    /// Возвращает true, если выбранный документ или поддокумент удален.
    /// Если документ не выбран, то возвращается false.
    /// Управляемое свойство для <see cref="Deleted"/>.
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
    /// Определить, что выбранный документ удален.
    /// </summary>
    /// <param name="message">Если метод возвращает true, сюда помещается текст сообщения для вывода пользователю</param>
    /// <returns>true, если документ удален</returns>
    protected abstract bool GetDeletedValue(out string message);

    /// <summary>
    /// Извещает, что свойство Deleted, возможно, изменилось
    /// </summary>
    public void SetDeletedChanged()
    {
      if (_DeletedEx != null)
        _DeletedEx.SetDelayed();
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Проверка наличия выбранного значения при <see cref="EFPAnyDocComboBoxBase.CanBeEmpty"/>=false.
    /// Также проверяется, не помечен ли документ или поддокумент на удаление (используется метод <see cref="GetDeletedValue(out string)"/>).
    /// </summary>
    protected override void OnValidate()
    {
      if (Id == 0)
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

      if (Id != 0 && (!CanBeDeleted))
      {
        string Message;
        if (GetDeletedValue(out Message))
        {
          if (CanBeDeletedMode == UIValidateState.Warning)
            SetWarning(Message);
          else
            SetError(Message);
        }
      }
    }

    /// <summary>
    /// Возвращает или устанавливает синхронизированное значение свойства <see cref="Id"/> для реализации интерфейса <see cref="IDepSyncObject"/>.
    /// </summary>
    public override object SyncValue
    {
      get { return Id; }
      set { Id = (Int32)value; }
    }

    /// <summary>
    /// Устанавливает <see cref="Id"/>=0
    /// </summary>
    public override void Clear()
    {
      Id = 0;
    }

    #endregion

    #region Значения полей строки документа или поддокумента

    /// <summary>
    /// Получить значение поля, соответствующее выбранному документу или поддокументу
    /// </summary>
    /// <param name="сolumnName">Имя поля</param>
    /// <returns>Необработанное значение поле или null, если значение не выбрано</returns>
    public abstract object GetColumnValue(string сolumnName);

    #endregion
  }

  /// <summary>
  /// Расширяет EFPDocComboBoxBase свойством Filters.
  /// </summary>
  public abstract class EFPDocComboBoxBaseWithFilters : EFPDocComboBoxBase
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    /// <param name="ui">Интерфейс доступа к документам</param>
    public EFPDocComboBoxBaseWithFilters(EFPBaseProvider baseProvider, UserSelComboBox control, DBUI ui)
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
    /// Возвращает число фильтров (свойство <see cref="Filters"/>.Count, включая пустые фильтры)
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
    /// Вызывается при изменении фильтров (событии <see cref="Filters"/>.Changed)
    /// </summary>
    protected virtual void OnFiltersChanged()
    {
      FilterPassed = TestFilter();
      if (ClearByFilter)
      {
        if (!FilterPassed)
          Id = 0;
      }
      else
        Validate();
    }

    /// <summary>
    /// Проверка соответствия текущего документа или поддокумента <see cref="EFPDocComboBoxBase.Id"/> фильтрам <see cref="Filters"/>.
    /// </summary>
    /// <returns>True, если документ соответствует, или <see cref="EFPDocComboBoxBase.Id"/>=0 или <see cref="Filters"/> неактивны</returns>
    public bool TestFilter()
    {
      DBxCommonFilter badFilter;
      return TestFilter(out badFilter);
    }

    /// <summary>
    /// Проверка соответствия текущего документа или поддокумента <see cref="EFPDocComboBoxBase.Id"/> фильтрам <see cref="Filters"/>.
    /// </summary>
    /// <param name="badFilter">Сюда записывается ссылка на первый фильтр в списке <see cref="Filters"/>, который не соответствует выбранному документу</param>
    /// <returns>True, если документ соответствует, или <see cref="EFPDocComboBoxBase.Id"/>=0 или <see cref="Filters"/> неактивны</returns>
    public bool TestFilter(out DBxCommonFilter badFilter)
    {
      badFilter = null;
      if (Id == 0)
        return true;
      if (FilterCount == 0)
        return true;
      if (_Filters.IsEmpty)
        return true;

      return DoTestFilter(out badFilter);
    }

    /// <summary>
    /// Проверка соответствия текущего документа или поддокумента <see cref="EFPDocComboBoxBase.Id"/> фильтрам <see cref="Filters"/>.
    /// Этот абстрактный метод вызывается из <see cref="TestFilter(out DBxCommonFilter)"/>, если предварительные проверки выполнены.
    /// </summary>
    /// <param name="badFilter">Сюда записывается ссылка на первый фильтр в списке <see cref="Filters"/>, который не соответствует выбранному документу</param>
    /// <returns>True, если документ соответствует, или <see cref="EFPDocComboBoxBase.Id"/>=0 или <see cref="Filters"/> неактивны</returns>
    protected abstract bool DoTestFilter(out DBxCommonFilter badFilter);

    /// <summary>
    /// Доопределяет проверку корректности значения.
    /// Вызывает <see cref="TestFilter(out DBxCommonFilter)"/>.
    /// </summary>
    protected override void OnValidate()
    {
      base.OnValidate();
      if (ValidateState == UIValidateState.Error)
        return;

      try
      {
        DBxCommonFilter badFilter;
        if (!TestFilter(out badFilter))
          SetError("Выбраное значение не проходит фильтр \"" + badFilter.DisplayName + "\" (" + ((IEFPGridFilter)(badFilter)).FilterText + ")");
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
    /// проходит условие для нового фильтра.
    /// Значение по умолчанию - true (очищать)
    /// Если установит в false, то "неподходящее" значение останется выбранным, но
    /// при проверке <see cref="EFPControlBase.Validate()"/> будет выдаваться ошибка.
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
    /// Управляемое свойство для <see cref="ClearByFilter"/>.
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
    /// Свойство возвращает true, если текущий выбранный документ (<see cref="EFPDocComboBoxBase.Id"/>) проходит условие фильтра.
    /// Если свойство <see cref="ClearByFilter"/> имеет значение true (по умолчанию), то <see cref="FilterPassed"/> всегда
    /// возвращает true, т.к. неподходящие значения обнуляются автоматически.
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
    /// Управляемое свойство для <see cref="FilterPassed"/>.
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
  /// Обработчик для комбоблока, предназначенного для выбора документа
  /// </summary>
  public class EFPDocComboBox : EFPDocComboBoxBaseWithFilters
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    /// <param name="docTypeUI">Интерфейс доступа к документам</param>
    public EFPDocComboBox(EFPBaseProvider baseProvider, UserSelComboBox control, DocTypeUI docTypeUI)
      : base(baseProvider, control, docTypeUI.UI)
    {
      //if (docTypeUI == null)
      //  throw new ArgumentNullException("docTypeUI");
      this.DocType = docTypeUI.DocType;

      control.EditButton = true;
      control.EditClick += new EventHandler(Control_EditClick);
      if (EFPApp.ShowToolTips) // 15.03.2018
        control.ClearButtonToolTipText = "Очистить поле выбора";
    }

    /// <summary>
    /// Объект пользовательского интерфейса для вида документов.
    /// Свойства <see cref="DocType"/>, <see cref="DocTypeUI"/>, <see cref="DocTypeName"/> и <see cref="DocTableId"/> являются синхронизированными.
    /// </summary>
    public DocTypeUI DocTypeUI
    {
      get { return UI.DocTypes[DocType.Name]; }
      set
      {
        if (value == null)
          DocType = null;
        else
          DocType = value.DocType;
      }
    }

    #endregion

    #region Текущий тип документа

    #region DocType

    /// <summary>
    /// Описание вида документов.
    /// Свойства <see cref="DocType"/>, <see cref="DocTypeUI"/>, <see cref="DocTypeName"/> и <see cref="DocTableId"/> являются синхронизированными.
    /// </summary>
    public DBxDocType DocType
    {
      get { return _DocType; }
      set
      {
        if (value == _DocType)
          return;
        _DocType = value;
        DocId = 0;
        if (_DocTableIdEx != null)
          _DocTableIdEx.Value = DocTableId;
        if (_DocTypeNameEx != null)
          _DocTypeNameEx.Value = DocTypeName;

        InitTextAndImage();
        Validate();

        // 13.06.2021
        if (EFPApp.ShowToolTips)
        {
          if (_DocType != null)
            Control.PopupButtonToolTipText = "Выбрать: " + _DocType.SingularTitle;
          else
            Control.PopupButtonToolTipText = "Выбрать документ";
        }
      }
    }

    private DBxDocType _DocType;

    #endregion

    #region DocTypeName

    /// <summary>
    /// Имя вида документов.
    /// Свойства <see cref="DocType"/>, <see cref="DocTypeUI"/>, <see cref="DocTypeName"/> и <see cref="DocTableId"/> являются синхронизированными.
    /// </summary>
    public string DocTypeName
    {
      get
      {
        if (DocType == null)
          return String.Empty;
        else
          return DocType.Name;
      }
      set
      {
        if (String.IsNullOrEmpty(value))
          DocType = null;
        else
          DocType = UI.DocTypes[value].DocType;
      }
    }

    /// <summary>
    /// Управляемое свойство для <see cref="DocTypeName"/>
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

    #region DocTableId

    /// <summary>
    /// Идентификатор таблицы вида документов (свойство DBxDocType.TableId).
    /// Свойства <see cref="DocType"/>, <see cref="DocTypeUI"/>, <see cref="DocTypeName"/> и <see cref="DocTableId"/> являются синхронизированными.
    /// </summary>
    public Int32 DocTableId
    {
      get
      {
        if (_DocType == null)
          return 0;
        else
          return _DocType.TableId;
      }
      set
      {
        if (value == 0)
          DocType = null;
        else
          DocType = UI.DocProvider.DocTypes.FindByTableId(value);
      }
    }

    /// <summary>
    /// Управляемое свойство для <see cref="DocTableId"/>. 
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

    #region Выбранный идентификатор документа

    /// <summary>
    /// Идентификатор выбрааного документа или 0.
    /// </summary>
    public virtual Int32 DocId
    {
      get { return base.Id; }
      set { base.Id = value; }
    }

    /// <summary>
    /// Управляемое свойство для <see cref="DocId"/>
    /// </summary>
    public DepValue<Int32> DocIdEx
    {
      get { return base.IdEx; }
      set { base.IdEx = value; }
    }

    #endregion

    #region Свойство FixedDocIds

    // TODO: Черновая реализация FixedDocIds. Не взаимодействует с Filters!

    /// <summary>
    /// Фиксированный список идентификаторов
    /// </summary>
    public IdList FixedDocIds
    {
      get { return _FixedDocIds; }
      set { _FixedDocIds = value; }
    }
    private IdList _FixedDocIds;

    #endregion

    /// <summary>
    /// Возможность задавать начальные значения при создании документа в выпадающем списке
    /// Если свойство не установлено (по умолчанию), то начальные значения определяются
    /// фильтрами (свойством <see cref="EFPDocComboBoxBaseWithFilters.Filters"/>, если задано, или текущими установленными
    /// фильтрами в табличном просмотре справочника).
    /// </summary>
    public DocumentViewHandler EditorCaller
    {
      get { return _EditorCaller; }
      set { _EditorCaller = value; }
    }
    private DocumentViewHandler _EditorCaller;

    #region Автоматическая установка значения

    /// <summary>
    /// Если установлено в true, то при изменении фильтров проверяется число подходящих
    /// записей. Если ровно одна запись проходит условие фильтра, то устанавливается
    /// значение <see cref="DocId"/>.
    /// По умолчанию - false.
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
    /// Вызывается при изменении фильтров
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
    /// Установить значение <see cref="DocId"/>, проходящее условия фильтра, если имеется только один
    /// такой документ.
    /// </summary>
    /// <returns>true, если есть единственный документ, false, если обнаружено больше
    /// одного подходящего документа или не найдено ни одного подходящего</returns>
    public bool SelectByFilter()
    {
      DBxFilter filter = Filters.GetSqlFilter();
      if (UI.DocProvider.DocTypes.UseDeleted) // 23.05.2021
        filter &= DBSDocType.DeletedFalseFilter;
      Int32 newId = UI.DocProvider.FindRecord(DocTypeName, filter, true);
      if (newId == 0)
        return false;

      DocId = newId;
      return true;
    }

    #endregion

    #region Внутренняя реализация

    /// <summary>
    /// Возвращает <see cref="DocType"/>.SingularTitle вместо "Без названия"
    /// </summary>
    protected override string DefaultDisplayName
    {
      get { return DocTypeUI.DocType.SingularTitle; }
    }

    /// <summary>
    /// Вызывает <see cref="DocTypeUIBase.GetTextValue(int)"/>
    /// </summary>
    /// <returns>Текстовое представление документа</returns>
    protected override string DoGetText()
    {
      return DocTypeUI.GetTextValue(DocId);
    }

    /// <summary>
    /// Вызывает <see cref="DocTypeUIBase.GetImageKey(int)"/>
    /// </summary>
    /// <returns>Имя изображения для документа</returns>
    protected override string DoGetImageKey()
    {
      return DocTypeUI.GetImageKey(DocId);
    }

    /// <summary>
    /// Вызывает <see cref="DocTypeUIBase.GetRowColor(EFPDataGridViewRowAttributesEventArgs)"/>.
    /// </summary>
    /// <param name="colorType">Сюда помещается цвет строки в справочнике</param>
    /// <param name="grayed">Сюда помещается true, если запись должна быть отмечена серым цветом</param>
    protected override void DoGetValueColor(out EFPDataGridViewColorType colorType, out bool grayed)
    {
      DocTypeUI.GetRowColor(DocId, out colorType, out grayed);
    }

    /// <summary>
    /// Вызывает <see cref="DocTypeUIBase.GetToolTipText(int)"/>
    /// </summary>
    /// <returns>Текст всплывающей подсказки для документа</returns>
    protected override string DoGetValueToolTipText()
    {
      return DocTypeUI.GetToolTipText(DocId);
    }

    /// <summary>
    /// Показывает диалог выбора документа с помощью <see cref="DocSelectDialog"/>
    /// </summary>
    protected override void DoPopup()
    {
      if (_DocType == null)
      {
        EFPApp.ShowTempMessage("Тип документа не задан");
        return;
      }
      DocSelectDialog dlg = new DocSelectDialog(DocTypeUI);
      if (!String.IsNullOrEmpty(DisplayName))
        dlg.Title = DisplayName;
      dlg.CanBeEmpty = CanBeEmpty;
      dlg.EditorCaller = EditorCaller;
      dlg.DialogPosition.PopupOwnerControl = Control;

      if (FixedDocIds == null)
      {
        if (Filters.Count > 0)
          dlg.Filters = Filters;// Иначе будут отключены стандартные фильтры
      }
      else
      {
        dlg.FixedDocIds = FixedDocIds;
      }

      dlg.DocId = DocId;
      if (dlg.ShowDialog() != DialogResult.OK)
        return;

      if (dlg.DocId == DocId)
        InitTextAndImage();
      else
        DocId = dlg.DocId;
    }

    /// <summary>
    /// Получить значение поля для выбранного документа. Возвращает null, если
    /// <see cref="DocId"/>=0.
    /// </summary>
    /// <param name="columnName">Имя поля, значение которого нужно получить</param>
    /// <returns>Значение поля</returns>
    public override object GetColumnValue(string columnName)
    {
      if (DocType == null || DocId == 0)
        return null;
      return UI.TextHandlers.DBCache[DocType.Name].GetValue(DocId, columnName);
    }

    /// <summary>
    /// Открытие на редактирование текущего выбранного документа
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    void Control_EditClick(object sender, EventArgs args)
    {
      try
      {
        if (DocId == 0)
        {
          EFPApp.ShowTempMessage("Документ не выбран");
          return;
        }
        UI.DocTypes[DocType.Name].PerformEditing(DocId, Control.EditButtonKind == UserComboBoxEditButtonKind.View);
        InitTextAndImage();
        SetDeletedChanged();
        Validate();
        DocIdEx.OnValueChanged();
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка редактирования документа");
      }
    }

    /// <summary>
    /// Доопределяем доступность кнопки "Редактировать"
    /// </summary>
    protected override void InitTextAndImage()
    {
      base.InitTextAndImage();

      if (DocType == null || DocId == 0)
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

    /// <summary>
    /// Определяет, помечен ли документ на удаление
    /// </summary>
    /// <param name="message">Сюда записывает текст сообщения "Выбранный документ XXX удален"</param>
    /// <returns>true, если документ помечен на удаление</returns>
    protected override bool GetDeletedValue(out string message)
    {
      if (!UI.DocProvider.DocTypes.UseDeleted)
      {
        message = null;
        return false;
      }

      if (DataTools.GetBool(UI.TextHandlers.DBCache[DocType.Name].GetBool(DocId, "Deleted")))
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
    /// Проверка попадания текущего документа в фильтр
    /// </summary>
    /// <param name="badFilter">Сюда помещается фильтр, который вызвал ошибку</param>
    /// <returns>True, если документ проходит условия всех фильтров</returns>
    protected override bool DoTestFilter(out DBxCommonFilter badFilter)
    {
      return DoTestFilter(this.DocId, out badFilter);
    }

    private bool DoTestFilter(Int32 docId, out DBxCommonFilter badFilter)
    {
      badFilter = null;
      if (DocType == null)
        return true;

      // Получаем данные для фильтрации
      DBxColumnList colList = new DBxColumnList();
      Filters.GetColumnNames(colList);
      DBxColumns columnNames = new DBxColumns(colList);

      object[] values = UI.TextHandlers.DBCache[DocTypeName].GetValues(docId, columnNames);
      return Filters.TestValues(columnNames, values, out badFilter);
    }

    /// <summary>
    /// Доопределяет проверку корректности выбранного документа.
    /// Проверяет при установленном свойстве <see cref="FixedDocIds"/>, что документ есть в списке допустимых для выбора.
    /// </summary>
    protected override void OnValidate()
    {
      base.OnValidate();
      if (base.ValidateState == UIValidateState.Error)
        return;

      if (FixedDocIds != null && DocId != 0)
      {
        if (!FixedDocIds.Contains(DocId))
          base.SetError("Выбранного документа нет в списке допустимых");
      }
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
    /// Возвращает выборку, состоящую из единственного выбранного документа.
    /// В выборке могут быть дополнительные документы, добавляемые <see cref="DocTypeUI.PerformGetDocSel(DBxDocSelection, int[], EFPDBxViewDocSelReason)"/>.
    /// </summary>
    /// <param name="reason">Причина построения выборки</param>
    /// <returns>Выборка документов</returns>
    protected override DBxDocSelection OnGetDocSel(EFPDBxViewDocSelReason reason)
    {
      DBxDocSelection docSel = new DBxDocSelection(UI.DocProvider.DBIdentity);
      if (DocType != null && DocId != 0)
        DocTypeUI.PerformGetDocSel(docSel, DocId, reason);
      return docSel;
    }

    /// <summary>
    /// Установка свойства <see cref="DocId"/> на основании выборки документов.
    /// Проверяет наличие документов вида <see cref="DocTypeName "/> в выборке <paramref name="docSel"/>.
    /// Если выборка содержит подходящие документы, свойство <see cref="DocId"/> устанавливается.
    /// Если в выборке есть несколько подходящих документов, то какой из них будет использован,
    /// не определено.
    /// </summary>
    /// <param name="docSel">Выборка документов</param>
    protected override void OnSetDocSel(DBxDocSelection docSel)
    {
      // TODO:
      //if (DocTypeName != "Выборки")
      //  DocSel.Normalize(AccDepClientExec.BufTables);
      Int32 newId = docSel.GetSingleId(DocTypeName);
      if (newId == 0)
        EFPApp.ShowTempMessage("В буфере обмена нет ссылки на документ \"" + DocType.SingularTitle + "\"");
      else
        Id = newId;
    }

    /// <summary>
    /// Возвращает true
    /// </summary>
    public override bool DocInfoSupported { get { return true; } }

    #endregion

    #region Проверка идентификатора документа

    /// <summary>
    /// Проверяет возможность присвоения заданного идентификатора документа без реальной установки фильтра.
    /// Возвращает false, если документ удален, а свойство <see cref="EFPAnyDocComboBoxBase.CanBeDeleted"/>=false 
    /// (значение по умолчанию). Также возвращает false, если документ не проходит условие фильтра.
    /// Если <paramref name="docId"/>=0, то проверяется свойство <see cref="EFPAnyDocComboBoxBase.CanBeEmpty"/>.
    /// </summary>
    /// <param name="docId">Идентификатор проверяемого документа</param>
    /// <returns>True, если документ является подходящим</returns>
    public bool TestDocId(Int32 docId)
    {
      string message;
      return TestDocId(docId, out message);
    }

    /// <summary>
    /// Проверяет возможность присвоения заданного идентификатора документа без реальной установки фильтра.
    /// Возвращает false, если документ удален, а свойство <see cref="EFPAnyDocComboBoxBase.CanBeDeleted"/>=false 
    /// (значение по умолчанию). Также возвращает false, если документ не проходит условие фильтра.
    /// Если <paramref name="docId"/>=0, то проверяется свойство <see cref="EFPAnyDocComboBoxBase.CanBeEmpty"/>.
    /// </summary>
    /// <param name="docId">Идентификатор проверяемого документа</param>
    /// <param name="message">Сюда помещается сообщение, почему документ является непродходящим</param>
    /// <returns>True, если документ является подходящим</returns>
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

      DBxCommonFilter badFilter;
      if (!DoTestFilter(docId, out badFilter))
      {
        message = "Документ не проходит фильтр \"" + badFilter.DisplayName + "\"";
        return false;
      }

      message = null;
      return true;
    }

    #endregion
  }

  /// <summary>
  /// Обработчик для комбоблока, позволяющего выбирать тип документа (для реализации
  /// переменных ссылок, настраиваемых пользователем)
  /// </summary>
  public class EFPDocTypeComboBox : EFPListComboBox
  {
    #region Конструкторы

    /// <summary>
    /// Создание провайдера.
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляюший элемент комбоблока</param>
    /// <param name="ui">Пользовательский интерфейс для работы с документами</param>
    /// <param name="docTypeNames">Список типов документов, из которого можно выбирать. Если задано null, 
    /// то выбор производится из всех существующих видов документов</param>
    public EFPDocTypeComboBox(EFPBaseProvider baseProvider, ComboBox control, DBUI ui, string[] docTypeNames)
      : base(baseProvider, control)
    {
      if (ui == null)
        throw new ArgumentNullException("ui");
      _UI = ui;

      if (docTypeNames == null)
        docTypeNames = ui.DocProvider.DocTypes.GetDocTypeNames();

      string[] pluralTitles = new string[docTypeNames.Length];
      _DocTypeUIs = new DocTypeUI[docTypeNames.Length];
      for (int i = 0; i < docTypeNames.Length; i++)
      {
        _DocTypeUIs[i] = ui.DocTypes[docTypeNames[i]];
        if (_DocTypeUIs[i] == null)
          throw new ArgumentException("Неизвестный тип документа \"" + docTypeNames[i] + "\"", "docTypeNames");
        pluralTitles[i] = _DocTypeUIs[i].DocType.PluralTitle;
      }
      control.Items.AddRange(pluralTitles);
      Codes = docTypeNames;

      _EmptyText = "[ Нет ]";
      _EmptyImageKey = "No";

      if (EFPApp.ShowListImages)
        new ListControlImagePainter(control, new ListControlImageEventHandler(DrawImage));
    }

    /// <summary>
    /// Создание провайдера.
    /// Выбор производится из всех существующих видов документов.
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляюший элемент комбоблока</param>
    /// <param name="ui">Пользовательский интерфейс для работы с документами</param>
    public EFPDocTypeComboBox(EFPBaseProvider baseProvider, ComboBox control, DBUI ui)
      : this(baseProvider, control, ui, null)
    {
    }

    /*
    public EFPDocTypeComboBox(EFPBaseProvider BaseProvider, ComboBox Control, DBUI UI, ClientDocType[] DocTypes)
      : base(BaseProvider, Control)
    {

      string[] DocTypeNames = new string[DocTypes.Length];
      string[] PluralTitles = new string[DocTypes.Length];

      for (int i = 0; i < DocTypes.Length; i++)
      {
        if (DocTypes[i] == null)
          throw new ArgumentException("Тип документа не задан в позиции " + i.ToString(), "DocTypes");
        DocTypeNames[i] = DocTypes[i].Name;
        PluralTitles[i] = DocTypes[i].PluralTitle;
      }

      FDocTypes = DocTypes;
      Control.Items.AddRange(PluralTitles);
      Codes = DocTypeNames;

      new ListControlImagePainter(Control, new ListControlImageEventHandler(DrawImage));
    }
    */

    #endregion

    #region Свойства

    /// <summary>
    /// Интерфейс работы с базами данных.
    /// Задается в конструкторе
    /// </summary>
    public DBUI UI { get { return _UI; } }
    private DBUI _UI;

    /// <summary>
    /// Список типов документов, из которого можно выбирать
    /// Задается в конструкторе
    /// </summary>
    public DocTypeUI[] DocTypeUIs { get { return _DocTypeUIs; } }
    private DocTypeUI[] _DocTypeUIs;

    /// <summary>
    /// Текст, который выводится при отсутствии выбранного вида документов. По умолчанию - "[ Нет ]"
    /// </summary>
    public string EmptyText { get { return _EmptyText; } set { _EmptyText = value; } }
    private string _EmptyText;

    /// <summary>
    /// Имя изображения из списка <see cref="EFPApp.MainImages"/>, которое используется при отсутствии выбранного вида документов. 
    /// По умолчанию - "No".
    /// </summary>
    public string EmptyImageKey { get { return _EmptyImageKey; } set { _EmptyImageKey = value; } }
    private string _EmptyImageKey;

    #endregion

    #region Изображение

    private void DrawImage(object sender, ListControlImageEventArgs args)
    {
      if (args.ItemIndex >= _DocTypeUIs.Length)
        return;

      if (args.ItemIndex < 0)
      {
        args.Text = EmptyText;
        args.ImageKey = EmptyImageKey;
      }
      else
      {
        args.ImageKey = _DocTypeUIs[args.ItemIndex].TableImageKey;
        args.ValidateState = UIValidateState.Ok;
      }
    }

    #endregion

    #region Выбранное значение

    #region Как ссылка на объявление типа

    /// <summary>
    /// Выбранный тип документа.
    /// Свойства <see cref="DocType"/>, <see cref="DocTypeName"/> и <see cref="DocTableId"/> являются взаимозависимыми.
    /// </summary>
    public DBxDocType DocType
    {
      get
      {
        if (SelectedIndex < 0)
          return null;
        else
          return _DocTypeUIs[SelectedIndex].DocType;
      }
      set
      {
        if (value == null)
          SelectedIndex = -1;
        else
          SelectedCode = value.Name;
      }
    }

    #endregion

    #region Как идентификатор таблицы

    /// <summary>
    /// Идентификатор таблицы для выбранного типа документа.
    /// Свойства <see cref="DocType"/>, <see cref="DocTypeName"/> и <see cref="DocTableId"/> являются взаимозависимыми.
    /// </summary>
    public Int32 DocTableId
    {
      get
      {
        if (DocType == null)
          return 0;
        else
          return DocType.TableId;
      }
      set
      {
        DocType = UI.DocProvider.DocTypes.FindByTableId(value);
      }
    }

    /// <summary>
    /// Управляемое свойство для <see cref="DocTableId"/>.
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
    private DepInput<Int32> _DocTableIdEx;

    private void InitDocTableIdEx()
    {
      if (_DocTableIdEx == null)
      {
        _DocTableIdEx = new DepInput<Int32>(DocTableId, SourceTableIdChanged);
        _DocTableIdEx.OwnerInfo = new DepOwnerInfo(this, "DocTableIdEx");
        SelectedIndexEx.ValueChanged += new EventHandler(SelectedIndex_ValueChanged);
      }
    }

    void SelectedIndex_ValueChanged(object sender, EventArgs args)
    {
      _DocTableIdEx.Value = DocTableId;
    }

    private void SourceTableIdChanged(object sender, EventArgs args)
    {
      DocTableId = _DocTableIdEx.Value;
    }

    #endregion

    #region Как имя таблицы

    /// <summary>
    /// Выбранный в данный момент вид документов.
    /// Свойства <see cref="DocType"/>, <see cref="DocTypeName"/> и <see cref="DocTableId"/> являются взаимозависимыми.
    /// </summary>
    public string DocTypeName
    {
      get { return base.SelectedCode; }
      set { base.SelectedCode = value; }
    }

    /// <summary>
    /// Управляемое свойство для <see cref="DocTypeName"/>.
    /// </summary>
    public DepValue<string> DocTypeNameEx
    {
      get { return base.SelectedCodeEx; }
      set { base.SelectedCodeEx = value; }
    }

    #endregion

    #endregion
  }

  /// <summary>
  /// Обработчик для комбоблока, предназначенного для выбора поддокумента.
  /// Поддокументы должны быть сохранены в базе данных, а не относиться к текущему редактируемому документу.
  /// Для выбора поддокумента из списка редактируемых используйте <see cref="EFPInsideSubDocComboBox"/>.
  /// </summary>
  public class EFPSubDocComboBox : EFPDocComboBoxBase
  {
    #region Конструкторы

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    /// <param name="subDocTypeUI">Интерфейс доступа к поддокументам</param>
    public EFPSubDocComboBox(EFPBaseProvider baseProvider, UserSelComboBox control, SubDocTypeUI subDocTypeUI)
      : base(baseProvider, control, subDocTypeUI.UI)
    {
      //if (subDocTypeUI == null)
      //  throw new ArgumentNullException("SubDocTypeUI");

      this._SubDocTypeUI = subDocTypeUI;

      if (EFPApp.ShowToolTips)
      {
        Control.PopupButtonToolTipText = "Выбрать: " + _SubDocTypeUI.SubDocType.SingularTitle;
        control.ClearButtonToolTipText = "Очистить поле выбора";
      }

      _DocId = 0;
      _DocIdWasSet = false;
    }


    /// <summary>
    /// Создает провайдер, связанный с уже добавленным провайдером комбоблока выбора документа.
    /// Кроме обычной инициализации, устанавливает связь для свойства <see cref="DocIdEx"/>.
    /// </summary>
    /// <param name="docComboBoxProvider">Провайдер комбоблока выбора документа. Не может быть null</param>
    /// <param name="control">Управляющий элемент</param>
    /// <param name="subDocTypeName">Имя вида поддокументов в <paramref name="docComboBoxProvider"/>.DocType.subDocs.</param>
    public EFPSubDocComboBox(EFPDocComboBox docComboBoxProvider, UserSelComboBox control, string subDocTypeName)
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
    /// Свойства <see cref="SubDocTypeUI"/>, <see cref="SubDocType"/> и <see cref="SubDocTypeName"/> являются синхронизированными.
    /// </summary>
    public SubDocTypeUI SubDocTypeUI
    {
      get { return _SubDocTypeUI; }
      set
      {
        if (value == _SubDocTypeUI)
          return;

        _SubDocTypeUI = value;
        SubDocId = 0;
        InitTextAndImage();
      }
    }
    private SubDocTypeUI _SubDocTypeUI;

    /// <summary>
    /// Имя таблицы поддокументов.
    /// Свойства <see cref="SubDocTypeUI"/>, <see cref="SubDocType"/> и <see cref="SubDocTypeName"/> являются синхронизированными.
    /// </summary>
    public string SubDocTypeName
    {
      get
      {
        if (_SubDocTypeUI == null)
          return String.Empty;
        else
          return SubDocTypeUI.SubDocType.Name;
      }
    }

    /// <summary>
    /// Описание основного вида документов, к которому относятся поддокументы.
    /// </summary>
    public DBxDocType DocType
    {
      get
      {
        if (_SubDocTypeUI == null)
          return null;
        else
          return _SubDocTypeUI.DocType;
      }
    }

    /// <summary>
    /// Описание вида поддокументов.
    /// Свойства <see cref="SubDocTypeUI"/>, <see cref="SubDocType"/> и <see cref="SubDocTypeName"/> являются синхронизированными.
    /// </summary>
    public DBxSubDocType SubDocType
    {
      get
      {
        if (_SubDocTypeUI == null)
          return null;
        else
          return _SubDocTypeUI.SubDocType;
      }
    }

    #endregion

    #region Свойство DocId

    /// <summary>
    /// Идентификатор документа, из которого выбираются поддокументы.
    /// Если свойство установлено в явном виде, то попытка выбрать "чужой" поддокумент
    /// приводит к индикации ошибки.
    /// Если свойство не установлено в явном виде, то разрешается установка произвольного значения
    /// свойства <see cref="SubDocId"/>, при этом возвращаемое значение <see cref="DocId"/> будет меняться в зависимости от того,
    /// к какому документу относится выбранный поддокумент.
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
          InitSubDocIdOnDocId();
        }
        else
        {
          // Свойство DocId может повторно устанавливаться из связанного EFPDocComboBox.DocId
          // после редактирования основного объекта, выполненного пользователем
          // Возможно, появился поддокумент, из которого можно выбрать; 
          // например, расчетный счет организации
          if (SubDocId == 0)
            InitSubDocIdOnDocId();
        }

        Validate(); // 29.08.2016
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
    /// Для внутреннего использования
    /// </summary>
    /// <param name="value"></param>
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

    private void InitSubDocIdOnDocId()
    {
      if (_InsideSetSubDocId)
        return;
      PerformInitDefSubDoc();
    }

    /// <summary>
    /// Инициализация свойства <see cref="SubDocId"/>.
    /// Устанавливает значение равным 0. Затем, если есть <see cref="DocId"/>!=0, <see cref="AutoSetIfSingle"/>=true и документ содержит ровно один
    /// поддокумент, выбирается он. Иначе, если документ выбран и есть обработчик события <see cref="InitDefSubDoc"/>, вызывается это событие.
    /// </summary>
    public void PerformInitDefSubDoc()
    {
      SubDocId = 0;
      if ((DocId != 0) && (SubDocTypeUI != null))
      {
        if (AutoSetIfSingle)
        {
          Int32[] ids = SubDocTypeUI.GetSubDocIds(DocId);
          if (ids.Length == 1)
          {
            SubDocId = ids[0];
            return;
          }
        }

        if (InitDefSubDoc != null)
          InitDefSubDoc(this, EventArgs.Empty);
      }
    }

    /// <summary>
    /// Управляемое свойство для <see cref="DocId"/>.
    /// Если в диалоге требуется выбирать документ и поддокумент, относящийся к этому документу, это свойство присоединяют как зависимое свойство
    /// к <see cref="EFPDocComboBox.DocIdEx"/>, или используют специальную перегрузку конструктора.
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
    /// или null, если внешнего управления нет.
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
    /// <see cref="DocId"/> (при этом <see cref="SubDocId"/> сбрасывается). Обработчик может
    /// установить желаемый идентификатор <see cref="SubDocId"/>. 
    /// Событие не вызывается, если установлено свойство <see cref="AutoSetIfSingle"/>=true и в документе есть ровно
    /// один поддокумент.
    /// </summary>
    public event EventHandler InitDefSubDoc;

    #endregion

    #region Свойство SubDocId

    /// <summary>
    /// Текущий выбранный поддокумент
    /// </summary>
    public virtual Int32 SubDocId
    {
      // Нужно обязательно использовать базовое свойство Id, т.к. к нему приделана обработка свойства IdEx
      get { return Id; }
      set { Id = value; }
    }

    /// <summary>
    /// Дублирует SubDocId.
    /// </summary>
    protected internal override Int32 Id
    {
      get { return base.Id; }
      set
      {
        if (value == base.Id)
          return;
        if (_InsideSetSubDocId)
          return;

        _InsideSetSubDocId = true;
        try
        {
          base.Id = value;
          if (value != 0)
            InternalSetDocId(SubDocTypeUI.TableCache.GetInt(value, "DocId"));
          else
            InternalSetDocId(0);
        }
        finally
        {
          _InsideSetSubDocId = false;
        }

        Validate();
      }
    }

    private bool _InsideSetSubDocId = false;

    /// <summary>
    /// Идентификатор выбранного поддокумента.
    /// Управляемое свойство для <see cref="SubDocId"/>.
    /// </summary>
    public DepValue<Int32> SubDocIdEx
    {
      get { return base.IdEx; }
      set { base.IdEx = value; }
    }

    #endregion

    #region Свойство AutoSetIfSingle

    /// <summary>
    /// Если установать в true, то при установке ненулевого значения <see cref="DocId"/> перед вызовом 
    /// события <see cref="InitDefSubDoc"/> будет загружен список поддокументов (кроме удаленных).
    /// Если в списке есть ровно один поддокумент, то его идентификатор присваивается
    /// свойству <see cref="SubDocId"/> и событие <see cref="InitDefSubDoc"/> не вызывается.
    /// Значение по умолчанию - false.
    /// </summary>
    public bool AutoSetIfSingle
    {
      get { return _AutoSetIfSingle; }
      set
      {
        if (value == _AutoSetIfSingle)
          return;
        _AutoSetIfSingle = value;

        if (_AutoSetIfSingleEx != null)
          _AutoSetIfSingleEx.Value = value;

        if (value && (DocId != 0) && (SubDocId == 0) &&
          (SubDocTypeUI != null))
        {
          Int32[] ids = SubDocTypeUI.GetSubDocIds(DocId);
          if (ids.Length == 1)
            SubDocId = ids[0];
        }

        Validate(); // 29.08.2016
      }
    }
    private bool _AutoSetIfSingle;

    /// <summary>
    /// Управляемое свойство для <see cref="AutoSetIfSingle"/>.
    /// </summary>
    public DepValue<Boolean> AutoSetIfSingleEx
    {
      get
      {
        InitAutoSetIfSingleEx();
        return _AutoSetIfSingleEx;
      }
      set
      {
        InitAutoSetIfSingleEx();
        _AutoSetIfSingleEx.Source = value;
      }
    }

    private void InitAutoSetIfSingleEx()
    {
      if (_AutoSetIfSingleEx == null)
      {
        _AutoSetIfSingleEx = new DepInput<bool>(AutoSetIfSingle, AutoSetIfSingleEx_ValueChanged);
        _AutoSetIfSingleEx.OwnerInfo = new DepOwnerInfo(this, "AutoSetIfSingleEx");
      }
    }
    private DepInput<Boolean> _AutoSetIfSingleEx;

    void AutoSetIfSingleEx_ValueChanged(object sender, EventArgs args)
    {
      AutoSetIfSingle = _AutoSetIfSingleEx.Value;
    }

    #endregion

    #region Проверка

    /// <summary>
    /// Проверка корректности выбранного значения.
    /// Проверяет, что поддокумент относится к документу, если свойство <see cref="DocId"/> установлено.
    /// </summary>
    protected override void OnValidate()
    {
      base.OnValidate();
      if (base.ValidateState == UIValidateState.Error)
        return;

      // 29.08.2016
      // Проверяем, что поддокумент относится к выбранному документу
      if (SubDocId != 0 && DocId != 0)
      {
        Int32 docId2 = SubDocTypeUI.TableCache.GetInt(SubDocId, "DocId");
        if (docId2 != DocId)
          SetError("Выбранный поддокумент \"" + SubDocTypeUI.SubDocType.SingularTitle + "\" относится к документу \"" +
            SubDocTypeUI.DocTypeUI.GetTextValue(docId2) + "\", а не \"" + SubDocTypeUI.DocTypeUI.GetTextValue(DocId) + "\"");
      }
    }

    #endregion

    #region Внутренняя реализация

    /// <summary>
    /// Возвращает <see cref="SubDocType"/>.SingularTitle вместо "Без названия".
    /// </summary>
    protected override string DefaultDisplayName
    {
      get { return SubDocTypeUI.SubDocType.SingularTitle; }
    }

    /// <summary>
    /// Возвращает <see cref="SubDocTypeUI"/>.GetTextValue(<see cref="SubDocId"/>).
    /// </summary>
    /// <returns>Текстовое представление для поддокумента</returns>
    protected override string DoGetText()
    {
      return SubDocTypeUI.GetTextValue(SubDocId);
    }

    /// <summary>
    /// Возвращает <see cref="SubDocTypeUI"/>.GetImageKey(<see cref="SubDocId"/>).
    /// </summary>
    /// <returns>Имя изображения для поддокумента</returns>
    protected override string DoGetImageKey()
    {
      return SubDocTypeUI.GetImageKey(SubDocId);
    }

    /// <summary>
    /// Возвращает <see cref="SubDocTypeUI"/>.GetToolTipText(<see cref="SubDocId"/>).
    /// </summary>
    /// <returns>Всплывающая подсказка для поддокумента</returns>
    protected override string DoGetValueToolTipText()
    {
      return SubDocTypeUI.GetToolTipText(SubDocId);
    }

    /// <summary>
    /// Возвращает <see cref="SubDocTypeUI"/>.GetRowColor(<see cref="SubDocId"/>).
    /// </summary>
    /// <param name="colorType">Сюда помещается цвет строки в справочнике</param>
    /// <param name="grayed">Сюда помещается true, если запись должна быть отмечена серым цветом</param>
    protected override void DoGetValueColor(out EFPDataGridViewColorType colorType, out bool grayed)
    {
      SubDocTypeUI.GetRowColor(SubDocId, out colorType, out grayed);
    }

    /// <summary>
    /// Показывает диалог выбора поддокумента с помощью <see cref="SubDocSelectDialog"/>.
    /// Если свойство <see cref="DocId"/>=0, то выдается сообщение об ошибке.
    /// </summary>
    protected override void DoPopup()
    {
      //if (DocType == null)
      //{
      //  EFPApp.ShowTempMessage("Не задан тип основного документа");
      //  return;
      //}
      //if (SubDocTypeUI == null)
      //{
      //  EFPApp.ShowTempMessage("Не задан тип поддокумента для выбора");
      //  return;
      //}
      if (DocId == 0)
      {
        EFPApp.ShowTempMessage("Не задан документ \"" + DocType.SingularTitle + "\", из которого можно выбирать");
        return;
      }

#if XXX
      RefDocGridFilter fltDocId = (RefDocGridFilter)(Filters["DocId"]);
      if (fltDocId != null)
      {
        if (fltDocId.DocTypeName != DocType.Name)
        {
          Filters.Remove(fltDocId);
          fltDocId = null;
        }
      }
      if (fltDocId == null)
      {
        fltDocId = new RefDocGridFilter(UI.DocTypes[DocType.Name], "DocId");
        fltDocId.DisplayName = DocType.SingularTitle;
        Filters.Insert(0, fltDocId);
      }
      fltDocId.SingleDocId = DocId;
#endif

      DBxDocSet docSet = new DBxDocSet(UI.DocProvider);
      DBxSingleDoc doc = docSet[DocType.Name].View(DocId);

      DBxMultiSubDocs mSubDocs = doc.SubDocs[SubDocTypeName].SubDocs;
#if DEBUG
      if (!Object.ReferenceEquals(mSubDocs.SubDocType, SubDocTypeUI.SubDocType))
        throw new BugException("Объекты DBxMultiSubDocs и SubDocTypeUI содержат ссылки на разные объекты SubDocType");
#endif

      SubDocSelectDialog dlg = new SubDocSelectDialog(SubDocTypeUI, mSubDocs);
      if (!String.IsNullOrEmpty(DisplayName))
        dlg.Title = DisplayName;
      dlg.CanBeEmpty = CanBeEmpty;
      dlg.DialogPosition.PopupOwnerControl = Control;
      dlg.SubDocId = SubDocId;
      if (dlg.ShowDialog() != DialogResult.OK)
        return;

      if (dlg.SubDocId == SubDocId)
      {
        InitTextAndImage();
      }
      else
        SubDocId = dlg.SubDocId;
    }

    /// <summary>
    /// Возвращает значение поля с помощью вызова <see cref="SubDocTypeUI"/>.GetValue(<see cref="SubDocId"/>, <paramref name="columnName"/>).
    /// Если нет выбранного поддокумента, возвращает null.
    /// </summary>
    /// <param name="columnName">Имя поля поддокумента. Допускаются ссылочные поля с точками</param>
    /// <returns>Значение поля.</returns>
    public override object GetColumnValue(string columnName)
    {
      if (SubDocTypeUI == null || SubDocId == 0)
        return null;
      return SubDocTypeUI.GetValue(SubDocId, columnName);
    }


    /// <summary>
    /// Возвращает true, если поддокумент или документ, к которому он относится, удалены.
    /// Если <see cref="DBxDocTypes.UseDeleted"/>=false, всегда возвращается false.
    /// </summary>
    /// <param name="message">Сюда записывается сообщение, что именно удалено.</param>
    /// <returns>True для удаленного (под)документа</returns>
    protected override bool GetDeletedValue(out string message)
    {
      if (!UI.DocProvider.DocTypes.UseDeleted)
      {
        message = null;
        return false;
      }

      object[] a = SubDocTypeUI.GetValues(SubDocId, "Deleted,DocId");
      if (DataTools.GetBool(a[0]))
      {
        message = "Выбранный поддокумент \"" + SubDocType.SingularTitle + "\" удален";
        return true; // удален поддокумент
      }
      Int32 docId = DataTools.GetInt(a[1]);
      if (DataTools.GetBool(UI.TextHandlers.DBCache[DocType.Name].GetBool(docId, "Deleted")))
      {
        string docText;
        try
        {
          docText = UI.DocTypes[DocType.Name].GetTextValue(docId) + " (DocId=" + docId.ToString() + ")";
        }
        catch (Exception e)
        {
          docText = "Id=" + docId.ToString() + ". Ошибка получения текста: " + e.Message;
        }
        message = "Документ \"" + DocType.SingularTitle + "\" (" + docText + "), к которому относится выбранный поддокумент, удален";
        return true;
      }
      else
      {
        message = null;
        return false;
      }
    }

#if XXX
    protected override bool DoTestFilter(out GridFilter BadFilter)
    {
      BadFilter = null;
      if (DocType == null || SubDocTypeUI == null)
        return true;

      // Получаем данные для фильтрации
      DBxColumnList FilterColumns = new DBxColumnList();
      Filters.GetColumnNames(FilterColumns);

      object[] Values = SubDocTypeUI.GetValues(SubDocId, FilterColumns.ToArray());
      return Filters.TestValues(new DBxColumns(FilterColumns), Values, out BadFilter);
    }
#endif

    #endregion

    #region Выборка документов

    /// <summary>
    /// Возвращает true, если есть обработчик события <see cref="SubDocTypeUI"/>.GetDocSel.
    /// </summary>
    public override bool GetDocSelSupported { get { return SubDocTypeUI.HasGetDocSel; } } // 31.10.2016

    /// <summary>
    /// Вызывает обработчик события <see cref="SubDocTypeUI"/>.GetDocSel для получения ссылок на документы,
    /// если это предусмотрено.
    /// Ссылка на документ, к которому относится выбранный поддокумент, НЕ ДОБАВЛЯЕТСЯ в выборку.
    /// </summary>
    /// <param name="Reason">Причина создания выборки</param>
    /// <returns>Выборка документов</returns>
    protected override DBxDocSelection OnGetDocSel(EFPDBxViewDocSelReason Reason)
    {
      DBxDocSelection docSel = new DBxDocSelection(UI.DocProvider.DBIdentity);
      if (SubDocId != 0)
      {
        SubDocTypeUI.PerformGetDocSel(docSel, SubDocId, Reason);
        docSel.Add(DocType.Name, DocId);
      }
      return docSel;
    }

    #endregion
  }

  /// <summary>
  /// Обработчик для комбоблока, предназначенного для выбора поддокумента в редакторе документа.
  /// Поддокументы относятся к текущему редактируемому документу.
  /// Для выбора поддокументов, относящихся к другим документам в базе данных, а не в текущем редактируемом документе, используйте <see cref="EFPSubDocComboBox"/>.
  /// </summary>
  public class EFPInsideSubDocComboBox : EFPDocComboBoxBase
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер комбоблока
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    /// <param name="subDocs">Список поддокументов, из которых можно выбирать</param>
    /// <param name="ui">Интерфейс для доступа к документам</param>
    public EFPInsideSubDocComboBox(EFPBaseProvider baseProvider, UserSelComboBox control, DBxMultiSubDocs subDocs, DBUI ui)
      : base(baseProvider, control, ui)
    {
      if (subDocs == null)
        throw new ArgumentNullException("subDocs");
      _SubDocs = subDocs;
      //if (!Object.ReferenceEquals(this.SubDocTypeUI.SubDocType, subDocs.SubDocType))
      //  throw new ArgumentException("SubDocTypeUI и SubDocs относятся к разным объектам SubDocType", "subDocs");


      control.PopupButtonToolTipText = "Выбрать: " + subDocs.SubDocType.SingularTitle; // 13.06.2021
      control.ClearButtonToolTipText = "Очистить поле выбора";

      _SubDocIndex = -1; // нет выбранного поддокумента

      //DebugTools.DebugDataTable(subDocs.SubDocsView.Table, subDocs.SubDocType.Name);
    }

    #endregion

    #region Вспомогательные свойства

    /// <summary>
    /// Поддокументы, из которых можно выбирать
    /// </summary>
    public DBxMultiSubDocs SubDocs { get { return _SubDocs; } }
    private DBxMultiSubDocs _SubDocs;

    /// <summary>
    /// Описание вида документов
    /// </summary>
    public DBxDocType DocType { get { return _SubDocs.Owner.DocType; } }

    /// <summary>
    /// Описание вида поддокументов.
    /// Свойства <see cref="SubDocType"/>, <see cref="SubDocTypeName"/> и <see cref="SubDocTypeUI"/> возвращают связанные значения.
    /// </summary>
    public DBxSubDocType SubDocType { get { return _SubDocs.SubDocType; } }

    /// <summary>
    /// Имя таблицы поддокументов.
    /// Свойства <see cref="SubDocType"/>, <see cref="SubDocTypeName"/> и <see cref="SubDocTypeUI"/> возвращают связанные значения.
    /// </summary>
    public string SubDocTypeName { get { return SubDocType.Name; } }

    /// <summary>
    /// Интерфейс для доступа к поддокументам.
    /// Свойства <see cref="SubDocType"/>, <see cref="SubDocTypeName"/> и <see cref="SubDocTypeUI"/> возвращают связанные значения.
    /// </summary>
    public SubDocTypeUI SubDocTypeUI
    {
      get
      {
        return base.UI.DocTypes[DocType.Name].SubDocTypes[SubDocType.Name];
      }
    }

    #endregion

    #region Свойство SubDocId

    /*
     * Нужно использовать в качестве текущего значения не идентификатор поддокумента, а номер поддокумента
     * в списке DBxMultiSubDocs. В противном случае значение станет недействительным, если был выбран новый
     * поддокумент (с фиктивным идентификатором), а затем вызывается DBxDocSet.ApplyChanges()
     */

    private int _SubDocIndex;

    /// <summary>
    /// Свойство SubDocId
    /// </summary>
    protected internal override Int32 Id
    {
      get
      {
        if (_SubDocIndex < 0)
          return 0;
        if (_SubDocs == null)
          return 0; // вызов из конструктора базового класса
        DBxSubDoc subDoc = _SubDocs[_SubDocIndex];
        return subDoc.SubDocId;
      }
      set
      {
        _SubDocIndex = _SubDocs.IndexOfSubDocId(value);
        if (_SubDocIndex < 0)
          base.Id = 0;
        else
          base.Id = value;
      }
    }

    /// <summary>
    /// Идентификатор выбранного поддокумента.
    /// Может быть фиктивным идентификатором, если поддокумент был добавлен, но не было еще
    /// вызова <see cref="DBxDocSet.ApplyChanges(bool)"/>.
    /// </summary>
    public Int32 SubDocId
    {
      get { return this.Id; }
      set { this.Id = value; }
    }

    /// <summary>
    /// Идентификатор выбранного поддокумента.
    /// Управляемое свойство для <see cref="SubDocId"/>.
    /// </summary>
    public DepValue<Int32> SubDocIdEx
    {
      get { return base.IdEx; }
      set { base.IdEx = value; }
    }

    #endregion

    #region Внутренняя реализация

    /// <summary>
    /// Возвращает <see cref="SubDocType"/>.SingularTitle вместо "Без названия"
    /// </summary>
    protected override string DefaultDisplayName
    {
      get { return SubDocTypeUI.SubDocType.SingularTitle; }
    }

    /// <summary>
    /// Возвращает текст поддокумента вызовом <see cref="DBxDocTextHandlers.GetTextValue(DBxSubDoc)"/>.
    /// </summary>
    /// <returns>Текстовое представление поддокумента</returns>
    protected override string DoGetText()
    {
      DBxSubDoc subDoc = SubDocs.GetSubDocById(SubDocId);
      return UI.TextHandlers.GetTextValue(subDoc);
      /*
      DataRow Row = CurrentRow;
      if (Row == null)
        return "!! Идентификатор " + SubDocIdEx.Value.ToString() + " не найден !!";
      return SubDocType.GetTextValue(Row);
       * */
    }

    /// <summary>
    /// Вызывает <see cref="DBxDocImageHandlers.GetImageKey(DBxSubDoc)"/>.
    /// </summary>
    /// <returns>Имя изображения для поддокумента</returns>
    protected override string DoGetImageKey()
    {
      DBxSubDoc subDoc = SubDocs.GetSubDocById(SubDocId);
      return UI.ImageHandlers.GetImageKey(subDoc);
    }

    /// <summary>
    /// Вызывает <see cref="DBxDocImageHandlers.GetRowColor(DBxSubDoc, out EFPDataGridViewColorType, out bool)"/>.
    /// </summary>
    /// <param name="colorType">Сюда записывается цветовое оформление строки</param>
    /// <param name="grayed">Сюда записывается true, если строка должна быть выделена серым цветом</param>
    protected override void DoGetValueColor(out EFPDataGridViewColorType colorType, out bool grayed)
    {
      DBxSubDoc subDoc = SubDocs.GetSubDocById(SubDocId);
      UI.ImageHandlers.GetRowColor(subDoc, out colorType, out grayed);
    }

    /*
    private DataRow CurrentRow
    {
      get
      {
        if (SubDocId == 0)
          return null;
        DataTable Table = MainDoc.Owner.subDocs[SubDocTypeName].SubDocsData;
        return Table.Rows.Find(SubDocIdEx.Value);
      }
    }
     * */

    /// <summary>
    /// Показывает диалог выбора поддокумента с помощью <see cref="SubDocSelectDialog"/>.
    /// </summary>
    protected override void DoPopup()
    {
      SubDocSelectDialog dlg = new SubDocSelectDialog(SubDocTypeUI, SubDocs);
      dlg.SelectionMode = DocSelectionMode.MultiSelect;
      if (!String.IsNullOrEmpty(DisplayName))
        dlg.Title = DisplayName;
      dlg.CanBeEmpty = CanBeEmpty;
      dlg.SubDocId = SubDocId;
      dlg.DialogPosition.PopupOwnerControl = Control;

      if (dlg.ShowDialog() != DialogResult.OK)
        return;

      SubDocId = dlg.SubDocId;
    }

    /// <summary>
    /// Возвращает значение поля выбранного поддокумента.
    /// Если поддокумент не выбран, возвращает null.
    /// </summary>
    /// <param name="columnName">Имя поля. Может содержать ссылочные поля с точками</param>
    /// <returns>Значение поля</returns>
    public override object GetColumnValue(string columnName)
    {
      if (SubDocId == 0)
        return null;

      //DBxSubDoc SubDoc = subDocs.GetSubDocById(SubDocId);
      return UI.TextHandlers.DBCache[SubDocType.Name].GetValue(SubDocId, columnName, SubDocs.DocSet.DataSet);
    }

    /// <summary>
    /// Возвращает true, если поддокумент помечен на удаление или
    /// помечен на удаление документ, к которому относится поддокумент.
    /// Если <see cref="DBxDocTypes.UseDeleted"/>=false, то возвращается false.
    /// </summary>
    /// <param name="message">Сюда записывается сообщение</param>
    /// <returns>True, если (под)документ удален</returns>
    protected override bool GetDeletedValue(out string message)
    {
      if (UI.DocProvider.DocTypes.UseDeleted)
      {
        if (_SubDocIndex >= 0)
        {
          DBxSubDoc subDoc = _SubDocs[_SubDocIndex];
          if (subDoc.SubDocState == DBxDocState.Delete)
          {
            message = "Выбранный поддокумент \"" + subDoc.SubDocType.SingularTitle + "\" удален";
            return true;
          }
          if (subDoc.Doc.DocState == DBxDocState.Delete)
          {
            message = "Документ \"" + subDoc.DocType.SingularTitle + "\", к которому относится выбранный поддокумент, удален";
            return true;
          }
        }
      }
      message = null;
      return false;
    }

    #endregion
  }
}
