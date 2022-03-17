// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using FreeLibSet.DependedValues;
using FreeLibSet.UICore;

namespace FreeLibSet.Forms
{
  #region Перечисление

  /// <summary>
  /// Какое свойство будет синхронизироваться в EFPListComboBox, EFPListBox и EFPRadioButtons
  /// </summary>
  public enum EFPListControlSyncValueType
  {
    /// <summary>
    /// Синхронизированным будет целочисленное свойство, задающее индекс выбранной позиции.
    /// Этот режим используется по умолчанию.
    /// </summary>
    SelectedIndex,

    /// <summary>
    /// Синхронизированным будет строковое свойство, задающее код выбранной позиции.
    /// </summary>
    SelectedCode
  }

  #endregion

  #region Интерфейс

  /// <summary>
  /// Интерфейс провайдера управляющего элемента со списком.
  /// </summary>
  public interface IEFPListControl : IEFPControl
  {
    /// <summary>
    /// Выбранная позиция в списке. (-1) - нет выбранного элемента
    /// </summary>
    int SelectedIndex { get; set; }

    /// <summary>
    /// Управляемое свойство SelectedIndex
    /// </summary>
    DepValue<int> SelectedIndexEx { get; set; }

    /// <summary>
    /// Разрешение на использование "серого" значения
    /// </summary>
    bool AllowDisabledSelectedIndex { get; set; }

    /// <summary>
    /// "Серое" значение, используемое, когда Enabled=false
    /// </summary>
    int DisabledSelectedIndex { get; set; }

    /// <summary>
    /// Управляемое свойство DisabledSelectedIndex.
    /// </summary>
    DepValue<int> DisabledSelectedIndexEx { get; set; }

    /// <summary>
    /// Выбранный код, если массив Codes задан.
    /// Значение UnselectedCode задает отсутствие выбранной позиции.
    /// </summary>
    string SelectedCode { get; set; }

    /// <summary>
    /// Управляемое свойство SelectedCode.
    /// </summary>
    DepValue<string> SelectedCodeEx { get; set; }

    /// <summary>
    /// Массив кодов. Должен задаваться, если требуется использование свойства SelectedCode.
    /// Свойство должно устанавливаться до вывода элемента на экран.
    /// </summary>
    string[] Codes { get; set; }

    /// <summary>
    /// Код, используемый, когда нет выбранной позиции в списке.
    /// По умолчанию - пустая строка.
    /// Свойство должно устанавливаться до вывода элемента на экран.
    /// </summary>
    string UnselectedCode { get; set; }

    /// <summary>
    /// Управляет синхронизацией значения (SelectedIndex или SelectedCode).
    /// По умодчанию используется SelectedIndex.
    /// Свойство должно устанавливаться до вывода элемента на экран.
    /// </summary>
    EFPListControlSyncValueType SyncValueType { get; set; }
  }

  #endregion

  /// <summary>
  /// Базовый класс для EFPListBox, EFPListComboBox и EFPListView
  /// </summary>
  public abstract class EFPListControl : EFPSyncControl<Control>, IEFPListControl
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер элемента
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPListControl(EFPBaseProvider baseProvider, Control control)
      : base(baseProvider, control, true)
    {
      _CanBeEmptyMode = UIValidateState.Error;
      _SavedSelectedIndex = -1;

      _Codes = null;
      _UnselectedCode = String.Empty;
      _SyncValueType = EFPListControlSyncValueType.SelectedIndex;
    }

    #endregion

    #region Переопределяемые методы

    /// <summary>
    /// Проверка данных.
    /// Если свойство CanBeEmpty=false и элемент не выбран в списке, устанавливает сообщение об ошибке.
    /// </summary>
    protected override void OnValidate()
    {
      base.OnValidate();

      if (SelectedIndex < 0)
      {
        switch (CanBeEmptyMode)
        {
          case UIValidateState.Error:
            SetError("Значение должно быть выбрано из списка");
            break;
          case UIValidateState.Warning:
            SetWarning("Значение, вероятно, должно быть выбрано из списка");
            break;
        }
      }
    }

    /// <summary>
    /// Инициализация "серого" значения.
    /// </summary>
    protected override void OnEnabledStateChanged()
    {
      base.OnEnabledStateChanged();
      if (AllowDisabledSelectedIndex && EnabledState)
        _HasSavedSelectedIndex = true;
      InitControlSelectedIndex();
    }

    #endregion

    #region Свойство SelectedIndex

    /// <summary>
    /// Выбранная позиция в списке. (-1) - нет выбранного элемента
    /// </summary>
    public int SelectedIndex
    {
      get { return ControlSelectedIndex; }
      set
      {
        //if (value == _SavedSelectedIndex && ControlSelectedIndex >= 0)
        //  return;
        _SavedSelectedIndex = value;
        _HasSavedSelectedIndex = true;
        InitControlSelectedIndex();
      }
    }
    private int _SavedSelectedIndex;
    private bool _HasSavedSelectedIndex;

    private void InitControlSelectedIndex()
    {
      // Не нужно, иначе может не обновляться
      //if (InsideSelectedIndexChanged)
      //  return;

      if (AllowDisabledSelectedIndex && (!EnabledState))
        ControlSelectedIndex = DisabledSelectedIndex;
      else if (_HasSavedSelectedIndex)
      {
        _HasSavedSelectedIndex = false;
        ControlSelectedIndex = _SavedSelectedIndex;
      }
    }

    private bool _InsideSelectedIndexChanged;

    /// <summary>
    /// Вызывается, когда изменятся значение в управляющем элементе
    /// </summary>
    protected void Control_SelectedIndexChanged(object sender, EventArgs args)
    {
      try
      {
        if (!_InsideSelectedIndexChanged)
        {
          _InsideSelectedIndexChanged = true;
          try
          {
            OnSelectedIndexChanged();
          }
          finally
          {
            _InsideSelectedIndexChanged = false;
          }
        }
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка обработчика Control.SelectedIndexChanged");
      }
    }

    /// <summary>
    /// Метод вызывается при изменении выбранной позиции в управляющем элементе.
    /// При переопределении обязательно должен вызываться базовый метод
    /// </summary>
    protected virtual void OnSelectedIndexChanged()
    {
      if (_SelectedIndexEx != null)
        _SelectedIndexEx.Value = SelectedIndex;
      if (_SelectedCodeEx != null)
        _SelectedCodeEx.Value = SelectedCode;

      if (AllowDisabledSelectedIndex && EnabledState)
        _SavedSelectedIndex = SelectedIndex;

      Validate();
      DoSyncValueChanged();
    }

    /// <summary>
    /// Получение и установка выбранной позиции в реальном управляющем элементе
    /// </summary>
    protected abstract int ControlSelectedIndex { get; set; }


    /// <summary>
    /// Получить число реально существующих строк в списке
    /// </summary>
    protected abstract int ControlItemCount { get; }

    /// <summary>
    /// Управляемое свойство SelectedIndex.
    /// Выбранная позиция в группе. Значение (-1) (значение по умолчанию) означает, 
    /// что ни одна позиция не выбрана.
    /// </summary>
    public DepValue<int> SelectedIndexEx
    {
      get
      {
        InitSelectedIndexEx();
        return _SelectedIndexEx;
      }
      set
      {
        InitSelectedIndexEx();
        _SelectedIndexEx.Source = value;
      }
    }

    private void InitSelectedIndexEx()
    {
      if (_SelectedIndexEx == null)
      {
        _SelectedIndexEx = new DepInput<int>(SelectedIndex, SelectedIndexEx_ValueChanged);
        _SelectedIndexEx.OwnerInfo = new DepOwnerInfo(this, "SelectedIndexEx");
      }
    }

    private DepInput<int> _SelectedIndexEx;

    void SelectedIndexEx_ValueChanged(object sender, EventArgs args)
    {
      SelectedIndex = _SelectedIndexEx.Value;
    }

    #endregion

    #region Свойство IsNotEmptyEx

    /// <summary>
    /// Объект содержит true, если есть выбранное значение (SelectedIndex>=0)
    /// </summary>
    public DepValue<bool> IsNotEmptyEx
    {
      get
      {
        if (_IsNotEmptyEx == null)
          _IsNotEmptyEx = new DepExpr1<bool, int>(SelectedIndexEx, CalcIsNotEmpty);
        return _IsNotEmptyEx;
      }
    }
    private DepValue<bool> _IsNotEmptyEx;

    private static bool CalcIsNotEmpty(int SelectedIndex)
    {
      return SelectedIndex >= 0;
    }

    #endregion

    #region Свойство DisabledSelectedIndex

    /// <summary>
    /// Доступ к DisabledSelectedIndex.ValueEx без принудительного создания объекта
    /// По умолчанию - (-1)
    /// </summary>
    public int DisabledSelectedIndex
    {
      get { return _DisabledSelectedIndex; }
      set
      {
        if (value == _DisabledSelectedIndex)
          return;
        _DisabledSelectedIndex = value;
        if (_DisabledSelectedIndexEx != null)
          _DisabledSelectedIndexEx.Value = value;
        InitControlSelectedIndex();
      }
    }
    private int _DisabledSelectedIndex;

    /// <summary>
    /// Это значение замещает свойство SelectedIndex, когда Enabled=false
    /// Свойство действует при установленном свойстве AllowDisabledSelectedIndex
    /// По умолчанию DisabledSelectedIndex.ValueEx=-1 (нет выбранной кнопки)
    /// </summary>
    public DepValue<int> DisabledSelectedIndexEx
    {
      get
      {
        InitDisabledSelectedIndexEx();
        return _DisabledSelectedIndexEx;
      }
      set
      {
        InitDisabledSelectedIndexEx();
        _DisabledSelectedIndexEx.Source = value;
      }
    }

    private void InitDisabledSelectedIndexEx()
    {
      if (_DisabledSelectedIndexEx == null)
      {
        _DisabledSelectedIndexEx = new DepInput<int>(DisabledSelectedIndex, DisabledSelectedIndexEx_ValueChanged);
        _DisabledSelectedIndexEx.OwnerInfo = new DepOwnerInfo(this, "DisabledSelectedIndexEx");
      }
    }
    private DepInput<int> _DisabledSelectedIndexEx;

    /// <summary>
    /// Вызывается, когда снаружи было изменено свойство DisabledText
    /// </summary>
    private void DisabledSelectedIndexEx_ValueChanged(object sender, EventArgs args)
    {
      DisabledSelectedIndex = _DisabledSelectedIndexEx.Value;
    }

    /// <summary>
    /// Разрешает использование свойства DisabledSelectedIndex
    /// </summary>
    public bool AllowDisabledSelectedIndex
    {
      get { return _AllowDisabledSelectedIndex; }
      set
      {
        if (value == _AllowDisabledSelectedIndex)
          return;
        _AllowDisabledSelectedIndex = value;
        InitControlSelectedIndex();
      }
    }
    private bool _AllowDisabledSelectedIndex;

    #endregion

    #region Свойство CanBeEmpty

    /// <summary>
    /// Режим проверки пустого значения.
    /// По умолчанию - Error.
    /// Это свойство переопределяется для нестандартных элементов, содержащих
    /// кнопку очистки справа от элемента
    /// </summary>
    public UIValidateState CanBeEmptyMode
    {
      get { return _CanBeEmptyMode; }
      set
      {
        if (value == _CanBeEmptyMode)
          return;
        _CanBeEmptyMode = value;
        Validate();
      }
    }
    private UIValidateState _CanBeEmptyMode;

    /// <summary>
    /// True, если ли элемент может содержать невыбранное значение (отрицательный SelectedIndex).
    /// Дублирует CanBeEmptyMode
    /// </summary>
    public bool CanBeEmpty
    {
      get { return CanBeEmptyMode != UIValidateState.Error; }
      set { CanBeEmptyMode = value ? UIValidateState.Ok : UIValidateState.Error; }
    }

    #endregion

    #region Подстановочные значения

    /// <summary>
    /// Необязательный список подстановочных значений, соответствующих элементам списка.
    /// Сначала должны быть заполнен основной список элементов ListBox/ComboBox.Items, а
    /// затем - установлено свойство Codes.
    /// Длина массива Codes должна быть равна числу элементов списка
    /// </summary>
    public string[] Codes
    {
      get { return _Codes; }
      set
      {
#if DEBUG
        if (value != null)
        {
          if (value.Length != ControlItemCount)
            throw new ArgumentException("Неправильная длина массива (" + value.Length + "). Строк в списке: " + ControlItemCount.ToString());
          foreach (string s in value)
          {
            if (s == null)
              throw new ArgumentException("Значения null не допускаются. Используйте String.Empty");
          }
        }
#endif
        _Codes = value;
        if (_SelectedCodeEx != null)
          _SelectedCodeEx.Value = SelectedCode;
      }
    }
    private string[] _Codes;

    /// <summary>
    /// Подстановочное значение для SelectedCodeEx при SelectedIndexEx=-1
    /// Если UnselectedCode совпадает с одним из значений массива Codes, то при
    /// установке SelectedCodeEx.ValueEx=UnselectedCode свойство SelectedIndexEx примет
    /// значение IndexOf(Codes), а не -1
    /// </summary>
    public string UnselectedCode
    {
      get { return _UnselectedCode; }
      set
      {
        if (value == null)
          value = String.Empty;
        if (value == _UnselectedCode)
          return;
        _UnselectedCode = value;
        if (_SelectedCodeEx != null)
          _SelectedCodeEx.Value = SelectedCode;
      }
    }
    private string _UnselectedCode;

    /// <summary>
    /// Доступ к SelectedCodeEx.Value без принудительного создания объекта
    /// </summary>
    public string SelectedCode
    {
      get
      {
        if (Codes == null || SelectedIndex < 0)
          return UnselectedCode;
        if (SelectedIndex >= Codes.Length)
          return UnselectedCode;
        return Codes[SelectedIndex];
      }
      set
      {
        // Не нужно, иначе может не обновляться
        //if (InsideSelectedIndexChanged)
        //  return;
        if (Codes == null)
          return;
        if (value == null)
          value = String.Empty;
        SelectedIndex = Array.IndexOf<string>(Codes, value);
      }
    }

    /// <summary>
    /// Текущая выбранная позиция в виде кода из массива Codes. Если нет выбранной
    /// кнопки (SelectedIndexEx.ValueEx=-1), то принимает значение UnselectedCode
    /// </summary>
    public DepValue<string> SelectedCodeEx
    {
      get
      {
        InitSelectedCodeEx();
        return _SelectedCodeEx;
      }
      set
      {
        InitSelectedCodeEx();
        _SelectedCodeEx.Source = value;
      }
    }

    private void InitSelectedCodeEx()
    {
      if (_SelectedCodeEx == null)
      {
        _SelectedCodeEx = new DepInput<string>(SelectedCode, SelectedCodeEx_ValueChanged);
        _SelectedCodeEx.OwnerInfo = new DepOwnerInfo(this, "SelectedCodeEx");
      }
    }

    private DepInput<string> _SelectedCodeEx;

    void SelectedCodeEx_ValueChanged(object sender, EventArgs args)
    {
      SelectedCode = _SelectedCodeEx.Value;
    }

    #endregion

    #region Синхронизация

    /// <summary>
    /// Какое свойство будет использовано для синхронизации в SyncGroup
    /// (по умолчанию - SelectedIndexEx)
    /// Свойство должно устанавливаться жо добавления объекта в список синхронизации
    /// </summary>
    public EFPListControlSyncValueType SyncValueType
    {
      get { return _SyncValueType; }
      set
      {
        if (SyncGroup != null)
          throw new InvalidOperationException("Нельзя устанавливать свойство SyncValueType, когда объект уже добавлен в группу");
        _SyncValueType = value;
      }
    }
    private EFPListControlSyncValueType _SyncValueType;

    /// <summary>
    /// Синхронизированное значение.
    /// Используется SelectedIndex или SelectedCode, в зависимости от свойства SyncValueType.
    /// </summary>
    public override object SyncValue
    {
      get
      {
        if (SyncValueType == EFPListControlSyncValueType.SelectedIndex)
          return SelectedIndex;
        else
          return SelectedCode;
      }
      set
      {
        if (SyncValueType == EFPListControlSyncValueType.SelectedIndex)
          SelectedIndex = (int)value;
        else
          SelectedCode = (string)value;
      }
    }

    #endregion
  }

  /// <summary>
  /// Обработчик для ListBox, предназначенного для выбора единственной позиции из списка
  /// с помощью свойства SelectedIndex.
  /// Множественный выбор строк не поддерживается.
  /// </summary>
  public class EFPListBox : EFPListControl
  {
    #region Конструкторы

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPListBox(EFPBaseProvider baseProvider, ListBox control)
      : base(baseProvider, control)
    {
      Init();
    }

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="controlWithToolBar">Управляющий элемент и панель инструментов</param>
    public EFPListBox(IEFPControlWithToolBar<ListBox> controlWithToolBar)
      : base(controlWithToolBar.BaseProvider, controlWithToolBar.Control)
    {
      Init();
    }

    private void Init()
    {
      _DoubleClickAsOk = true;
      if (!DesignMode)
      {
        Control.SelectedIndexChanged += new EventHandler(Control_SelectedIndexChanged);
        Control.MouseDoubleClick += new MouseEventHandler(Control_MouseDoubleClick);
      }
    }

    #endregion

    #region Переопределяемые методы и свойства

    /// <summary>
    /// Возвращает управляющий элемент
    /// </summary>
    public new ListBox Control { get { return (ListBox)(base.Control); } }

    /// <summary>
    /// Возвращает количество позиций в списке
    /// </summary>
    protected override int ControlItemCount
    {
      get { return Control.Items.Count; }
    }

    /// <summary>
    /// Возвращает или устанавливает индекс текущей позиции в списке
    /// </summary>
    protected override int ControlSelectedIndex
    {
      get { return Control.SelectedIndex; }
      set { Control.SelectedIndex = value; }
    }

    #endregion

    #region Свойство DoubleClickAsOk

    /// <summary>
    /// Если true (значение по умолчанию), то двойной щелчок мыши на строке списка
    /// приводит к эмуляции нажатия кнопки по умолчанию в блоке диалога
    /// </summary>
    public bool DoubleClickAsOk
    {
      get { return _DoubleClickAsOk; }
      set { _DoubleClickAsOk = value; }
    }
    private bool _DoubleClickAsOk;

    void Control_MouseDoubleClick(object sender, MouseEventArgs args)
    {
      try
      {
        if (DoubleClickAsOk && args.Button == MouseButtons.Left)
        {
          if (Control.SelectedItem == null)
            return;
          Rectangle r = Control.GetItemRectangle(Control.SelectedIndex);
          if (r.Contains(args.X, args.Y))
          {
            if (Control.FindForm().AcceptButton != null)
              Control.FindForm().AcceptButton.PerformClick();
          }
        }
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка обработчика ListBox.MouseDoubleClick");
      }
    }

    #endregion

    #region Вспомогательные свойства  ItemStrings и SelectedItemString

    /// <summary>
    /// Получение или заполнение списка элементов как массива строк.
    /// Управляющий элемент ListBox может хранить в качестве элементов не только строки
    /// </summary>
    public string[] ItemStrings
    {
      get
      {
        string[] a = new string[Control.Items.Count];
        for (int i = 0; i < a.Length; i++)
          a[i] = Control.Items[i].ToString();
        return a;
      }
      set
      {
        Control.BeginUpdate();
        try
        {
          Control.Items.Clear();
          Control.Items.AddRange(value);
        }
        finally
        {
          Control.EndUpdate();
        }
      }
    }

    /// <summary>
    /// Текущий выбранный элемент как строка.
    /// Может использоваться, только если список хранит элементы-строки, а не объекты других типов.
    /// Пустая строка означает отсутствие выбранного элемента.
    /// Обычно следует использовать свойство SelectedCode.
    /// </summary>
    public string SelectedItemString
    {
      get
      {
        if (SelectedIndex < 0)
          return String.Empty;
        else
          return Control.Items[SelectedIndex].ToString();
      }
      set
      {
        if (String.IsNullOrEmpty(value))
          SelectedIndex = -1;
        else
        {
          for (int i = 0; i < Control.Items.Count; i++)
          {
            if (Control.Items[i].ToString() == value)
            {
              SelectedIndex = i;
              break;
            }
          }
        }
      }
    }

    /// <summary>
    /// Дополнительная обработка для SelectedItemStringEx
    /// </summary>
    protected override void OnSelectedIndexChanged()
    {
      base.OnSelectedIndexChanged();

      if (_SelectedItemStringEx != null)
        _SelectedItemStringEx.Value = SelectedItemString;
    }

    /// <summary>
    /// Управляемое свойство для SelectedItemString
    /// </summary>
    public DepValue<string> SelectedItemStringEx
    {
      get
      {
        InitSelectedItemStringEx();
        return _SelectedItemStringEx;
      }
      set
      {
        InitSelectedItemStringEx();
        _SelectedItemStringEx.Source = value;
      }
    }
    private DepInput<string> _SelectedItemStringEx;

    private void InitSelectedItemStringEx()
    {
      if (_SelectedItemStringEx == null)
      {
        _SelectedItemStringEx = new DepInput<string>(SelectedItemString, SelectedItemStringEx_ValueChanged);
        _SelectedItemStringEx.OwnerInfo = new DepOwnerInfo(this, "SelectedItemStringEx");
      }
    }

    void SelectedItemStringEx_ValueChanged(object sender, EventArgs args)
    {
      SelectedItemString = _SelectedItemStringEx.Value;
    }

    #endregion
  }

  /// <summary>
  /// Обработчик для ComboBox, предназначенного для выбора позиции из списка
  /// с помощью свойства SelectedIndexEx
  /// </summary>
  public class EFPListComboBox : EFPListControl
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер.
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPListComboBox(EFPBaseProvider baseProvider, ComboBox control)
      : base(baseProvider, control)
    {
      if (!DesignMode)
      {
        control.DropDownStyle = ComboBoxStyle.DropDownList;
        control.SelectedIndexChanged += new EventHandler(Control_SelectedIndexChanged);
      }
    }

    #endregion

    #region Переопределяемые методы и свойства

    /// <summary>
    /// Управляющий элемент
    /// </summary>
    public new ComboBox Control { get { return (ComboBox)(base.Control); } }

    /// <summary>
    /// Возвращает ComboBox.Items.Count
    /// </summary>
    protected override int ControlItemCount
    {
      get { return Control.Items.Count; }
    }

    /// <summary>
    /// Возвращает или устанавливает ComboBox.SelectedIndex.
    /// </summary>
    protected override int ControlSelectedIndex
    {
      get { return Control.SelectedIndex; }
      set { Control.SelectedIndex = value; }
    }

    #endregion

    #region Вспомогательные свойства  ItemStrings и SelectedItemString

    /// <summary>
    /// Получение или заполнение списка элементов как массива строк.
    /// Управляющий элемент ComboBox может хранить в качестве элементов не только строки
    /// </summary>
    public string[] ItemStrings
    {
      get
      {
        string[] a = new string[Control.Items.Count];
        for (int i = 0; i < a.Length; i++)
          a[i] = Control.Items[i].ToString();
        return a;
      }
      set
      {
        Control.BeginUpdate();
        try
        {
          Control.Items.Clear();
          Control.Items.AddRange(value);
        }
        finally
        {
          Control.EndUpdate();
        }
      }
    }

    /// <summary>
    /// Текущий выбранный элемент как строка.
    /// Может использоваться, только если комбоблок хранит элементы-строки, а не объекты других типов.
    /// Пустая строка означает отсутствие выбранного элемента.
    /// Обычно следует использовать свойство SelectedCode.
    /// </summary>
    public string SelectedItemString
    {
      get
      {
        if (SelectedIndex < 0)
          return String.Empty;
        else
          return Control.Items[SelectedIndex].ToString();
      }
      set
      {
        if (String.IsNullOrEmpty(value))
          SelectedIndex = -1;
        else
        {
          for (int i = 0; i < Control.Items.Count; i++)
          {
            if (Control.Items[i].ToString() == value)
            {
              SelectedIndex = i;
              break;
            }
          }
        }
      }
    }

    /// <summary>
    /// Дополнительная обработка для SelectedItemStringEx
    /// </summary>
    protected override void OnSelectedIndexChanged()
    {
      base.OnSelectedIndexChanged();

      if (_SelectedItemStringEx != null)
        _SelectedItemStringEx.Value = SelectedItemString;
    }

    /// <summary>
    /// Управляемое свойство для SelectedItemString
    /// </summary>
    public DepValue<string> SelectedItemStringEx
    {
      get
      {
        InitSelectedItemStringEx();
        return _SelectedItemStringEx;
      }
      set
      {
        InitSelectedItemStringEx();
        _SelectedItemStringEx.Source = value;
      }
    }
    private DepInput<string> _SelectedItemStringEx;

    private void InitSelectedItemStringEx()
    {
      if (_SelectedItemStringEx == null)
      {
        _SelectedItemStringEx = new DepInput<string>(SelectedItemString, SelectedItemStringEx_ValueChanged);
        _SelectedItemStringEx.OwnerInfo = new DepOwnerInfo(this, "SelectedItemStringEx");
      }
    }

    void SelectedItemStringEx_ValueChanged(object sender, EventArgs args)
    {
      SelectedItemString = _SelectedItemStringEx.Value;
    }

    #endregion
  }

  /// <summary>
  /// Обработчик для ListView, предназначенного для выбора единственной позиции из списка
  /// с помощью свойства SelectedIndex
  /// </summary>
  public class EFPListView : EFPListControl
  {
    #region Конструкторы

    /// <summary>
    /// Создает провайдер.
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    public EFPListView(EFPBaseProvider baseProvider, ListView control)
      : base(baseProvider, control)
    {
      Init();
    }

    /// <summary>
    /// Создает провайдер.
    /// </summary>
    /// <param name="controlWithToolBar">Управляющий элемент с панелью инструментов</param>
    public EFPListView(IEFPControlWithToolBar<ListView> controlWithToolBar)
      : base(controlWithToolBar.BaseProvider, controlWithToolBar.Control)
    {
      Init();
    }

    private void Init()
    {
      Control.MultiSelect = false;
      _DoubleClickAsOk = true;
      if (!DesignMode)
      {
        Control.SelectedIndexChanged += new EventHandler(Control_SelectedIndexChanged);
        Control.MouseDoubleClick += new MouseEventHandler(Control_MouseDoubleClick);
      }
    }

    #endregion

    #region Переопределяемые методы и свойства

    /// <summary>
    /// Возвращает управляющий элемент
    /// </summary>
    public new ListView Control { get { return (ListView)(base.Control); } }

    /// <summary>
    /// Возвращает ListView.Items.Count
    /// </summary>
    protected override int ControlItemCount
    {
      get { return Control.Items.Count; }
    }

    /// <summary>
    /// Возвращает или устанавливает текущую выбранную позицию в списке.
    /// Так как ListView не содержит свойства SelectedIndex, используется SelectedIndices.
    /// </summary>
    protected override int ControlSelectedIndex
    {
      get
      {
        if (Control.SelectedIndices.Count == 0)
          return -1;
        else
          return Control.SelectedIndices[0];
      }
      set
      {
        if (value == ControlSelectedIndex)
          return;
        ListViewItem[] currSels = new ListViewItem[Control.SelectedItems.Count];
        Control.SelectedItems.CopyTo(currSels, 0);
        for (int i = 0; i < currSels.Length; i++)
          currSels[i].Selected = false;
        if (value >= 0)
        {
          Control.Items[value].Selected = true;
          Control.EnsureVisible(value);
        }
      }
    }

    #endregion

    #region Свойство DoubleClickAsOk

    /// <summary>
    /// Если true (значение по умолчанию), то двойной щелчок мыши на строке списка
    /// приводит к эмуляции нажатия кнопки по умолчанию в блоке диалога
    /// </summary>
    public bool DoubleClickAsOk
    {
      get { return _DoubleClickAsOk; }
      set { _DoubleClickAsOk = value; }
    }
    private bool _DoubleClickAsOk;

    void Control_MouseDoubleClick(object sender, MouseEventArgs args)
    {
      try
      {
        if (DoubleClickAsOk && args.Button == MouseButtons.Left)
        {
          ListViewItem li = Control.GetItemAt(args.X, args.Y);
          if (li != null)
          {
            if (Control.FindForm().AcceptButton != null)
              Control.FindForm().AcceptButton.PerformClick();
          }
        }
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка обработчика ListView.MouseDoubleClick");
      }
    }

    #endregion
  }
}
