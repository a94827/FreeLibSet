using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Data;
using FreeLibSet.DependedValues;
using FreeLibSet.Forms.Data;

namespace FreeLibSet.Forms.Docs
{
  // ******************************************************************************
  // Переходники для управляющих элементов, объявленных в EFPDocComboBox

  /// <summary>
  /// Базовый класс переходника для управляющего элемента, производного от <see cref="EFPDocComboBoxBase"/>.
  /// Управляет очисткой свойства <see cref="EFPAnyDocComboBoxBase.EmptyText"/>, если в элементе выведены "серые" значения.
  /// </summary>
  /// <typeparam name="TValue">Тип редактируемого значения (Int32)</typeparam>
  /// <typeparam name="TControlProvider">Тип провайдера управляющего элемента, производный от <see cref="EFPAnyDocComboBoxBase"/></typeparam>
  public abstract class ExtValueAnyDocComboBoxBase<TValue, TControlProvider> : ExtValueControl<TValue, TControlProvider>
    where TControlProvider : EFPAnyDocComboBoxBase
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="extValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public ExtValueAnyDocComboBoxBase(DBxExtValue extValue, TControlProvider controlProvider, bool canMultiEdit)
      : base(extValue, controlProvider, true, canMultiEdit)
    {
      DepAnd.AttachInput(controlProvider.SelectableEx, EnabledEx);

      if (canMultiEdit)
      {
        _OrgEmptyText = controlProvider.EmptyText;
        controlProvider.EmptyTextEx.ValueChanged += new EventHandler(EmptyTextEx_ValueChanged);
      }
      else
        _OrgEmptyText = null; // маркер !CanMultiEdit
      _InsideInitEnabled = false;
    }

    #endregion

    #region Управление свойством EmptyText

    private string _OrgEmptyText;

    private bool _InsideInitEnabled;

    void EmptyTextEx_ValueChanged(object sender, EventArgs args)
    {
      if (_InsideInitEnabled)
        return;
      _OrgEmptyText = ControlProvider.EmptyText;
    }

    /// <summary>
    /// Вызывается при изменении состояния Grayed или UserEnabled
    /// </summary>
    protected override void InitEnabled()
    {
      base.InitEnabled();

      if (_InsideInitEnabled)
        return; // по идее, никогда не должно быть

      if (_OrgEmptyText != null)
      {
        _InsideInitEnabled = true;
        try
        {
          // Метод вызывается, в частности, при изменении свойства Grayed
          if (base.GrayedEx.Value)
            ControlProvider.EmptyText = String.Empty;
          else
            ControlProvider.EmptyText = _OrgEmptyText;
        }
        finally
        {
          _InsideInitEnabled = false;
        }
      }
    }

    #endregion
  }

  /// <summary>
  /// Переходник для <see cref="EFPDocComboBox"/>
  /// </summary>
  public class ExtValueDocComboBox : ExtValueAnyDocComboBoxBase<Int32, EFPDocComboBox>
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="extValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public ExtValueDocComboBox(DBxExtValue extValue, EFPDocComboBox controlProvider, bool canMultiEdit)
      : base(extValue, controlProvider, canMultiEdit)
    {
      SetCurrentValueEx(controlProvider.DocIdEx);
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Инициализация управляющего элемента текущим значением
    /// </summary>
    protected override void ValueToControl()
    {
      CurrentValueEx.Value = ExtValue.AsInteger;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      if (CurrentValueEx.Value == 0)
        ExtValue.SetNull();
      else
        ExtValue.SetInteger(CurrentValueEx.Value);
    }

    #endregion
  }

  /// <summary>
  /// Переходник для <see cref="EFPSubDocComboBox"/>
  /// </summary>
  public class ExtValueSubDocComboBox : ExtValueAnyDocComboBoxBase<Int32, EFPSubDocComboBox>
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="extValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public ExtValueSubDocComboBox(DBxExtValue extValue, EFPSubDocComboBox controlProvider, bool canMultiEdit)
      : base(extValue, controlProvider, canMultiEdit)
    {
      SetCurrentValueEx(controlProvider.SubDocIdEx);
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Инициализация управляющего элемента текущим значением
    /// </summary>
    protected override void ValueToControl()
    {
      CurrentValueEx.Value = ExtValue.AsInteger;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      if (CurrentValueEx.Value == 0)
        ExtValue.SetNull();
      else
        ExtValue.SetInteger(CurrentValueEx.Value);
    }

    #endregion
  }

#if XXX
  // ??? Сомнительная реализация
  public class DocValueDocComboBoxByTableId : DocValueControl<int, EFPDocComboBox>
  {
  #region Конструктор

    public DocValueDocComboBoxByTableId(IDocValue DocValue, EFPDocComboBox ControlProvider, bool CanMultiEdit)
      : base(DocValue, ControlProvider, true, CanMultiEdit)
    {
      SetCurrentValue(ControlProvider.DocTableIdEx);
      CurrentValue.ValueChanged += new EventHandler(ControlChanged);
      DepAnd.AttachInput(ControlProvider.SelectableEx, EnabledEx);
    }

  #endregion

  #region Переопределенные методы

    protected override void ValueToControl()
    {
      CurrentValue.Value = DocValue.AsInteger;
    }

    protected override void ValueFromControl()
    {
      if (CurrentValue.Value == 0)
        DocValue.SetNull();
      else
        DocValue.AsInteger = CurrentValue.Value;
    }

  #endregion
  }

  /// <summary>
  /// Переходник для EFPSubDocComboBox
  /// </summary>
  public class DocValueSubDocComboBox : DocValueControl<Int32, EFPSubDocComboBox>
  {
  #region Конструктор

    public DocValueSubDocComboBox(IDocValue DocValue, EFPSubDocComboBox ControlProvider, bool CanMultiEdit)
      : base(DocValue, ControlProvider, true, CanMultiEdit)
    {
      SetCurrentValue(ControlProvider.SubDocIdEx);
      CurrentValue.ValueChanged += new EventHandler(ControlChanged);
      DepAnd.AttachInput(ControlProvider.SelectableEx, EnabledEx);
    }

  #endregion

  #region Переопределенные методы

    protected override void ValueToControl()
    {
      CurrentValue.Value = DocValue.AsInteger;
    }

    protected override void ValueFromControl()
    {
      if (CurrentValue.Value == 0)
        DocValue.SetNull();
      else
        DocValue.AsInteger = CurrentValue.Value;
    }


    /// <summary>
    /// На время чтения значений отключаем свойство автоматического выбора
    /// единственного поддокумента
    /// </summary>
    public override void BeforeReadValues()
    {
      base.BeforeReadValues();
      AutoSetIfSingle = ControlProvider.AutoSetIfSingle;
      ControlProvider.AutoSetIfSingle = false;
    }

    public override void AfterReadValues()
    {
      // 13.03.2010
      // Провайдер EFPSubDocComboBox может быть связан с помощью свойства DocId
      // с другим управляющим элементом, например с EFPDocComboBox, в котором 
      // выбирается основной документ.
      // Для нового документа в результате взаимодействия сохраненных предыдущих
      // значений ClientFields и текущих установленных фильтров IDocumentEditorCaller
      // могут возникнуть несовместимые значения полей.
      // Например, сначала было записано платежное поручение на одну организацию и
      // с расчетным счетом, относящимся к этой организации. Затем пользователь
      // устанавливает фильтр по контрагенту в журнале платежных поручений, который
      // не совпадает с первой организацией. Затем пользователь создает новое
      // платежное поручение. Сначала ClientFields устанавливают поля на предыдущего
      // контрагента и его счет (согласованные данные). Но затем фильтр устанавливает
      // контрагента на другую организацию, а счет не изменяет. Получаются 
      // несогласованные начальные значения полей

      // Решение проблемы. Проверяем совпадение текущего значения EFPDocComboBox.DocId
      // (которое вычислено исходя из заданного SubDocId) с тем DocId, на который 
      // установлена связь

      if (ControlProvider.Enabled && // то есть не Grayed и не DataReadOnly
        ControlProvider.SubDocId != 0 && // есть выбранный поддокумент
        ControlProvider.DocIdExSource != null) // и есть внешнее управление
      {
        if (ControlProvider.DocId != ControlProvider.DocIdExSource.Value &&
          ControlProvider.DocIdExSource.Value != 0) // 12.06.2011
        {
          EFPApp.WarningMessageBox("Поле \"" + ControlProvider.DisplayName +
            "\" изменено, т.к. выбранный ранее поддокумент " + ControlProvider.SubDocType.SingularTitle + " \"" +
            ControlProvider.SubDocType.GetTextValue(ControlProvider.SubDocId) +
            "\" относится к документу " + ControlProvider.DocType.SingularTitle +
            " \"" + ControlProvider.DocType.GetTextValue(ControlProvider.DocId) +
            "\", а не к \"" + ControlProvider.DocType.GetTextValue(ControlProvider.DocIdExSource.Value) + "\"");
          ControlProvider.DocId = 0; // Иначе не сбросится
          ControlProvider.DocId = ControlProvider.DocIdExSource.Value;
        }
      }


      base.AfterReadValues();
      ControlProvider.AutoSetIfSingle = AutoSetIfSingle;
    }

    private bool AutoSetIfSingle;

  #endregion
  }

#endif

  /// <summary>
  /// Переходник для <see cref="EFPInsideSubDocComboBox"/>
  /// </summary>
  public class ExtValueInsideSubDocComboBox : ExtValueAnyDocComboBoxBase<Int32, EFPInsideSubDocComboBox>
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="extValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public ExtValueInsideSubDocComboBox(DBxExtValue extValue, EFPInsideSubDocComboBox controlProvider, bool canMultiEdit)
      : base(extValue, controlProvider, canMultiEdit)
    {
      SetCurrentValueEx(controlProvider.SubDocIdEx);
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Инициализация управляющего элемента текущим значением
    /// </summary>
    protected override void ValueToControl()
    {
      // TODO: 21.11.2019 Затычка
      // Если текущее значение DocValue является неправильным (относится к другому документу), то предыдущий оператор приводит к некорректному состоянию.
      // Свойство EFPInsideSubDocComboBox.SubDicId остается равным 0, но SubDicIdEx остается с неправильным значением.
      //CurrentValueEx.Value = DocValue.AsInteger;

      // Так не будет бяки
      ControlProvider.SubDocId = ExtValue.AsInteger;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      ExtValue.SetInteger(CurrentValueEx.Value);
    }

    #endregion
  }

  /// <summary>
  /// Переходник для <see cref="EFPDocTypeComboBox"/> с использованием в качестве значения идентификатора таблицы
  /// </summary>
  public class ExtValueDocTypeComboBoxByTableId : ExtValueControl<int, EFPDocTypeComboBox>
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="extValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public ExtValueDocTypeComboBoxByTableId(DBxExtValue extValue, EFPDocTypeComboBox controlProvider, bool canMultiEdit)
      : base(extValue, controlProvider, true, canMultiEdit)
    {
      SetCurrentValueEx(controlProvider.DocTableIdEx);
      DepAnd.AttachInput(controlProvider.EnabledEx, EnabledEx);
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Инициализация управляющего элемента текущим значением
    /// </summary>
    protected override void ValueToControl()
    {
      CurrentValueEx.Value = ExtValue.AsInteger;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      ExtValue.SetInteger(CurrentValueEx.Value);
    }

    #endregion
  }

  /// <summary>
  /// Переходник для <see cref="EFPDocTypeComboBox"/> с использованием в качестве значения имени таблицы
  /// </summary>
  public class ExtValueDocTypeComboBoxByName : ExtValueControl<string, EFPDocTypeComboBox>
  {
    #region Конструктор

    /// <summary>
    /// Создает переходник.
    /// </summary>
    /// <param name="extValue">Доступ к значению поля</param>
    /// <param name="controlProvider">Провайдер управляющего элемента</param>
    /// <param name="canMultiEdit">Если true, то разрешается групповое редактирования для нескольких документов сразу.
    /// Если false, то при групповом редактировании поле скрывается</param>
    public ExtValueDocTypeComboBoxByName(DBxExtValue extValue, EFPDocTypeComboBox controlProvider, bool canMultiEdit)
      : base(extValue, controlProvider, true, canMultiEdit)
    {
      UserDisabledValue = String.Empty;

      SetCurrentValueEx(controlProvider.DocTypeNameEx);
      DepAnd.AttachInput(controlProvider.EnabledEx, EnabledEx);
    }

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Инициализация управляющего элемента текущим значением
    /// </summary>
    protected override void ValueToControl()
    {
      CurrentValueEx.Value = ExtValue.AsString;
    }

    /// <summary>
    /// Инициализация текущего значения из управляющего элемента
    /// </summary>
    protected override void ValueFromControl()
    {
      ExtValue.SetString(CurrentValueEx.Value);
    }

    #endregion
  }
}
