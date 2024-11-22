// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using FreeLibSet.UICore;

// Система проверки корректности введенных значений

namespace FreeLibSet.Forms
{
  #region EFPErrorInfo

  /// <summary>
  /// Информация об одной ошибке или предупреждении.
  /// Класс однократной записи.
  /// </summary>
  public class EFPErrorInfo
  {
    #region Конструктор

    /// <summary>
    /// Устанавливает значения свойств.
    /// </summary>
    /// <param name="message">Строка сообщения</param>
    /// <param name="isError">true- ошибка, false - предупреждение</param>
    /// <param name="focusedControl">Управляющий элемент, которому должен быть передан фокус ввода.
    /// Может быть null.</param>
    public EFPErrorInfo(string message, bool isError, Control focusedControl)
    {
#if DEBUG
      if (String.IsNullOrEmpty(message))
        throw new ArgumentNullException("message");
#endif
      _Message = message;
      _IsError = isError;
      _FocusedControl = focusedControl;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Текст сообщения об ошибке
    /// </summary>
    public string Message { get { return _Message; } }
    private readonly string _Message;

    /// <summary>
    /// True, если сообщение об ошибке, False - если предупрежедение
    /// </summary>
    public bool IsError { get { return _IsError; } }
    private readonly bool _IsError;

    /// <summary>
    /// Управляющий элемент, которому надо передать фокус ввода.
    /// Может быть null.
    /// </summary>
    public Control FocusedControl { get { return _FocusedControl; } }
    private readonly Control _FocusedControl;

    /// <summary>
    /// Возвращает свойство <see cref="Message"/>
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return _Message;
    }

    #endregion
  }

  #endregion

  /// <summary>
  /// "Базовый" провайдер, к которому можно присоединять управляющие элементы.
  /// Базовые провайдеры могут образовывать иерархию. С помощью свойства <see cref="EFPBaseProvider.Parent"/> можно присоединять и отсоединять
  /// провайдеры в иерархии.
  /// Корнем иерархии для формы является <see cref="EFPFormProvider"/>, который наследует <see cref="EFPBaseProvider"/>.
  /// Провайдеры управляющих элементов, производные от <see cref="EFPControlBase"/> и объекты <see cref="EFPFormCheck"/> присоединяются к <see cref="EFPBaseProvider "/> в конструкторе и не могут
  /// быть отсоединены. Для отключения управляющих элементов следует отключить от формы соответствующий <see cref="EFPBaseProvider"/>.
  /// </summary>
  public class EFPBaseProvider
  {
    #region Конструктор

    /// <summary>
    /// Создает базовый провайдер, не прекрепленный ни к чему, и не имеющий дочерних элементов.
    /// Для присоединения к родительскому провайдеру или к <see cref="EFPFormProvider"/> должно быть установлено свойство <see cref="Parent"/>.
    /// </summary>
    public EFPBaseProvider()
    {
    }

    #endregion

    #region Иерархия EFPBaseProvider

    /// <summary>
    /// Родительский элемент в иерархии провайдеров.
    /// </summary>
    public EFPBaseProvider Parent
    {
      get { return _Parent; }
      set
      {
        if (value == _Parent)
          return; // ничего не поменялось
        if (value == this)
          throw new InvalidOperationException("Нельзя присоединить EFPBaseProvider к самому себе");

        if (_Parent != null)
        {
          _Parent._Children.Remove(this);
          _Parent.OnChildRemoved(this);
          _Parent.ItemStateChanged();
        }
        _Parent = value;
        if (value != null)
        {
          if (_Parent._Children == null)
            _Parent._Children = new List<EFPBaseProvider>();

          _Parent._Children.Add(this);
          _Parent.OnChildAdded(this);
          _Parent.ItemStateChanged();
        }

        ParentProviderChanged();
      }
    }
    private EFPBaseProvider _Parent;

    /// <summary>
    /// Создается только при добавлении дочернего элемента
    /// </summary>
    private List<EFPBaseProvider> _Children;

    /// <summary>
    /// Вызывается после того, как к текущему провайдеру был добавлен дочерний <see cref="EFPBaseProvider"/>
    /// </summary>
    /// <param name="child">Добавленный дочерний провайдер</param>
    protected virtual void OnChildAdded(EFPBaseProvider child)
    {
    }

    /// <summary>
    /// Вызывается после того, как от текущего провайдера был отсоединен дочерний <see cref="EFPBaseProvider"/>
    /// </summary>
    /// <param name="child">Отсоединенный дочерний провайдер</param>
    protected virtual void OnChildRemoved(EFPBaseProvider child)
    {
    }

    /// <summary>
    /// Коллекция для свойства <see cref="Children"/>
    /// </summary>
    public struct ChildCollection : IEnumerable<EFPBaseProvider>
    {
      #region Конструктор

      internal ChildCollection(EFPBaseProvider owner)
      {
        _Owner = owner;
      }

      #endregion

      #region Коллекция

      private EFPBaseProvider _Owner;

      /// <summary>
      /// Возвращает количество дочерних провайдеров <see cref="EFPBaseProvider"/>
      /// </summary>
      public int Count
      {
        get
        {
          if (_Owner._Children == null)
            return 0;
          else
            return _Owner._Children.Count;
        }
      }

      /// <summary>
      /// Доступ к дочернему провайдеру по индексу
      /// </summary>
      /// <param name="index">Индекс</param>
      /// <returns>Провайдер</returns>
      public EFPBaseProvider this[int index]
      {
        get
        {
          if (index < 0 || index >= Count)
            throw new ArgumentOutOfRangeException("index", index, "Индекс должен быть в диапазоне от 0 до " + (index - 1).ToString());
          return _Owner._Children[index];
        }
      }

      #endregion

      #region Перечислитель

      private static readonly List<EFPBaseProvider> _DummyList = new List<EFPBaseProvider>();

      /// <summary>
      /// Создает перечислитель
      /// </summary>
      /// <returns>Перечислитель по списку</returns>
      public List<EFPBaseProvider>.Enumerator GetEnumerator()
      {
        if (_Owner._Children == null)
          return _DummyList.GetEnumerator();
        else
          return _Owner._Children.GetEnumerator();
      }

      IEnumerator<EFPBaseProvider> IEnumerable<EFPBaseProvider>.GetEnumerator()
      {
        return GetEnumerator();
      }

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
        return GetEnumerator();
      }

      #endregion
    }

    /// <summary>
    /// Коллекция дочерних <see cref="EFPBaseProvider"/>.
    /// В коллекцию входят только непосредственные дочерние объекты, без рекурсии
    /// </summary>
    public ChildCollection Children { get { return new ChildCollection(this); } }

    /// <summary>
    /// Этот метод вызывается точкой проверки, когда состояние ошибки меняется
    /// </summary>
    public virtual void ItemStateChanged()
    {
      if (Parent != null)
        Parent.ItemStateChanged();
    }

    /// <summary>
    /// Рекурсивно создает массив из всех объектов <see cref="EFPBaseProvider"/>, входящих в иерархию.
    /// Нулевым элементом массива будет текущий объект.
    /// </summary>
    /// <returns></returns>
    public EFPBaseProvider[] GetAllBaseProviders()
    {
      List<EFPBaseProvider> list = new List<EFPBaseProvider>();
      DoAddBaseProviders(list);
      return list.ToArray();
    }

    private void DoAddBaseProviders(List<EFPBaseProvider> list)
    {
      list.Add(this);
      if (_Children != null)
      {
        for (int i = 0; i < _Children.Count; i++)
          _Children[i].DoAddBaseProviders(list); // рекурсивный вызов
      }
    }

    #endregion

    #region Провайдеры управляющих элементов

    private List<EFPControlBase> _ControlProviders;

    /// <summary>
    /// Вызывается из конструктора EFPControlBase
    /// </summary>
    internal void AddControlProvider(EFPControlBase controlProvider)
    {
      if (_ControlProviders == null)
        _ControlProviders = new List<EFPControlBase>();
      _ControlProviders.Add(controlProvider);
      OnControlProviderAdded(controlProvider);
    }

    /// <summary>
    /// Вызывается после добавления провайдера управляющего элемента.
    /// Этот метод вызывается из конструктора <see cref="EFPControlBase"/>, поэтому конструирование провайдера еще не завершено!
    /// </summary>
    /// <param name="controlBase">Провайдер управляющего элемента</param>
    protected virtual void OnControlProviderAdded(EFPControlBase controlBase)
    {
    }


    /// <summary>
    /// Коллекция для свойства <see cref="ControlProviders"/>
    /// </summary>
    public struct ControlProviderCollection : IEnumerable<EFPControlBase>
    {
      #region Конструктор

      internal ControlProviderCollection(EFPBaseProvider owner)
      {
        _Owner = owner;
      }

      #endregion

      #region Коллекция

      private EFPBaseProvider _Owner;

      /// <summary>
      /// Возвращает количество подключенных провайдеров управляющих элементов
      /// </summary>
      public int Count
      {
        get
        {
          if (_Owner._ControlProviders == null)
            return 0;
          else
            return _Owner._ControlProviders.Count;
        }
      }

      /// <summary>
      /// Доступ к провайдеру по индексу
      /// </summary>
      /// <param name="index">Индекс</param>
      /// <returns>Провайдер</returns>
      public EFPControlBase this[int index]
      {
        get
        {
          if (index < 0 || index >= Count)
            throw new ArgumentOutOfRangeException("index", index, "Индекс должен быть в диапазоне от 0 до " + (index - 1).ToString());
          return _Owner._ControlProviders[index];
        }
      }

      #endregion

      #region Перечислитель

      private static readonly List<EFPControlBase> _DummyList = new List<EFPControlBase>();

      /// <summary>
      /// Создает перечислитель
      /// </summary>
      /// <returns>Перечислитель по списку</returns>
      public List<EFPControlBase>.Enumerator GetEnumerator()
      {
        if (_Owner._ControlProviders == null)
          return _DummyList.GetEnumerator();
        else
          return _Owner._ControlProviders.GetEnumerator();
      }

      IEnumerator<EFPControlBase> IEnumerable<EFPControlBase>.GetEnumerator()
      {
        return GetEnumerator();
      }

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
        return GetEnumerator();
      }

      #endregion
    }

    /// <summary>
    /// Коллекция подключенных управляющих элементов.
    /// В коллекцию входят только провайдеры, подключенные к этому <see cref="EFPBaseProvider"/>, но не к его дочерним элементам.
    /// </summary>
    public ControlProviderCollection ControlProviders { get { return new ControlProviderCollection(this); } }

    #endregion

    #region FindControlProvider

    /// <summary>
    /// Выполняет рекурсивный поиск провайдера для управляющего элемента, начиная с текущего <see cref="EFPBaseProvider"/>.
    /// Если нет провайдера для этого элемента, возвращает null.
    /// Поиск в родительском провайдере, если он задан, не выполняется.
    /// </summary>
    /// <param name="control">Управляющий элемент, для которого выполняется поиск</param>
    /// <returns>Найденный провайдер или null</returns>
    public EFPControlBase FindControlProvider(Control control)
    {
      if (control == null)
        return null;

      if (_ControlProviders != null)
      {
        for (int i = 0; i < _ControlProviders.Count; i++)
        {
          if (_ControlProviders[i].ContainsControl(control))
            return _ControlProviders[i];
        }
      }
      if (_Children != null)
      {
        for (int i = 0; i < _Children.Count; i++)
        {
          EFPControlBase res = _Children[i].FindControlProvider(control);
          if (res != null)
            return res;
        }
      }
      return null;
    }

    #endregion

    #region Рекурсивный поиск элментов

    /// <summary>
    /// Рекурсивный поиск всех провайдеров управляющих элементов заданного вида.
    /// </summary>
    /// <typeparam name="T">Тип элементов. Для поиска всех провайдеров управляющих элементов используйте тип <see cref="EFPControlBase"/></typeparam>
    /// <param name="list">Заполняемый список</param>
    public void GetControlProviders<T>(ICollection<T> list)
      where T : EFPControlBase
    {
      if (list == null)
        throw new ArgumentNullException("list");

      if (_Children != null)
      {
        for (int i = 0; i < _Children.Count; i++)
          _Children[i].GetControlProviders<T>(list);
      }
      if (_ControlProviders != null)
      {
        for (int i = 0; i < _ControlProviders.Count; i++)
        {
          if (_ControlProviders[i] is T)
            list.Add((T)(_ControlProviders[i]));
        }
      }
    }

    /// <summary>
    /// Рекурсивный поиск первого элемента заданного вида
    /// </summary>
    /// <typeparam name="T">Тип элемента. Для поиска провайдера управляющего элемента используйте тип <see cref="EFPControlBase"/></typeparam>
    /// <returns>Первый найденный элемент или null</returns>
    public T GetFirstControlProvider<T>()
      where T : EFPControlBase
    {
      if (_Children != null)
      {
        for (int i = 0; i < _Children.Count; i++)
        {
          T res = _Children[i].GetFirstControlProvider<T>();
          if (res != null)
            return res;
        }
      }
      if (_ControlProviders != null)
      {
        for (int i = 0; i < _ControlProviders.Count; i++)
        {
          if (_ControlProviders[i] is T)
            return (T)(_ControlProviders[i]);
        }
      }

      return null;
    }

    /// <summary>
    /// Возвращает массив всех провайдеров управляющих элементов, прямо или косвенно подключенных к текущему базовому провайдеру
    /// </summary>
    /// <returns>Массив объектов <see cref="EFPControlBase"/></returns>
    public EFPControlBase[] GetAllControlProviders()
    {
      List<EFPControlBase> list = new List<EFPControlBase>();
      GetControlProviders<EFPControlBase>(list);
      return list.ToArray();
    }

    #endregion

    #region Объекты EFPFormCheck

    private List<EFPFormCheck> _FormChecks;

    /// <summary>
    /// Вызывается из конструктора EFPFormCheck
    /// </summary>
    internal void AddFormCheck(EFPFormCheck formCheck)
    {
      if (_FormChecks == null)
        _FormChecks = new List<EFPFormCheck>();
      _FormChecks.Add(formCheck);
      OnFormCheckAdded(formCheck);
    }

    /// <summary>
    /// Вызывается после добавления объекта проверки формы.
    /// Этот метод вызывается из конструктора <see cref="EFPFormCheck"/>, поэтому конструирование объекта еще не завершено!
    /// </summary>
    /// <param name="formCheck">Объект проверки</param>
    protected virtual void OnFormCheckAdded(EFPFormCheck formCheck)
    {
    }

    /// <summary>
    /// Коллекция для свойства <see cref="FormChecks"/>
    /// </summary>
    public struct FormCheckCollection : IEnumerable<EFPFormCheck>
    {
      #region Конструктор

      internal FormCheckCollection(EFPBaseProvider owner)
      {
        _Owner = owner;
      }

      #endregion

      #region Коллекция

      private EFPBaseProvider _Owner;

      /// <summary>
      /// Возвращает количество подключенных проверяющих объектов
      /// </summary>
      public int Count
      {
        get
        {
          if (_Owner._FormChecks == null)
            return 0;
          else
            return _Owner._FormChecks.Count;
        }
      }

      #endregion

      #region Перечислитель

      private static readonly List<EFPFormCheck> _DummyList = new List<EFPFormCheck>();

      /// <summary>
      /// Создает перечислитель
      /// </summary>
      /// <returns>Перечислитель по списку</returns>
      public List<EFPFormCheck>.Enumerator GetEnumerator()
      {
        if (_Owner._FormChecks == null)
          return _DummyList.GetEnumerator();
        else
          return _Owner._FormChecks.GetEnumerator();
      }

      IEnumerator<EFPFormCheck> IEnumerable<EFPFormCheck>.GetEnumerator()
      {
        return GetEnumerator();
      }

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
        return GetEnumerator();
      }

      #endregion
    }

    /// <summary>
    /// Коллекция подключенных проверяющих объектов формы.
    /// В коллекцию входят только объекты, подключенные к этому <see cref="EFPBaseProvider"/>, но не к его дочерним элементам.
    /// </summary>
    public FormCheckCollection FormChecks { get { return new FormCheckCollection(this); } }

    /// <summary>
    /// Возвращает массив всех объектов проверки формы <see cref="EFPFormCheck"/>, прямо или косвенно присоединенных к текущему базовому провайдеру
    /// </summary>
    /// <returns></returns>
    public EFPFormCheck[] GetAllFormChecks()
    {
      List<EFPFormCheck> list = new List<EFPFormCheck>();
      DoAddFormChecks(list);
      return list.ToArray();
    }

    private void DoAddFormChecks(List<EFPFormCheck> list)
    {
      if (_FormChecks != null)
        list.AddRange(_FormChecks);
      if (_Children != null)
      {
        for (int i = 0; i < _Children.Count; i++)
          _Children[i].DoAddFormChecks(list); // рекурсивный вызов
      }
    }

    #endregion

    #region Объект формы

    /// <summary>
    /// Корневой провайдер в дереве (провайдер формы) или null, если его нет
    /// </summary>
    public virtual EFPFormProvider FormProvider
    {
      get
      {
        if (Parent == null)
          return null;
        else
          return Parent.FormProvider;
      }
    }

    /// <summary>
    /// True, если провайдер подключен к форме и форма выведена на экран
    /// </summary>
    public bool IsFormVisible
    {
      get
      {
        EFPFormProvider fp = this.FormProvider;
        if (fp == null)
          return false;
        else
          return fp.Visible; // а не fp.Form.Visible
      }
    }

    /// <summary>
    /// Используемый блокировщик вложенных вызовов методовю
    /// </summary>
    public virtual IEFPReentranceLocker ReentranceLocker
    {
      get
      {
        if (Parent == null)
          return EFPDummyReentranceLocker.TheLocker;
        else
          return Parent.ReentranceLocker;
      }
    }

    #endregion

    #region Средства для вывода сообщений

    #region Буферизация подсказок

    private class SetToolTipInfo
    {
      #region Поля

      public string Title;
      public string MainInfo;
      public string ValueInfo;
      public UIValidateState State;
      public string ErrorMessage;

      #endregion
    }

    /// <summary>
    /// Отложенные подсказки.
    /// Используется, когда свойство Parent не установлено. Когда родительский провайдер присоединяется позднее,
    /// для него выполняется вызов SetToolTip()
    /// (20.05.2015)
    /// </summary>
    private Dictionary<Control, SetToolTipInfo> _DelayedToolTips;

    #endregion

    /// <summary>
    /// Установка подсказки для управляющего элемента
    /// </summary>
    /// <param name="control">Управляющий элемент (не может быть null)</param>
    /// <param name="title">Заголовок</param>
    /// <param name="mainInfo">Основная подсказка для элемента (назначение элемента)</param>
    /// <param name="valueInfo">Подсказка по текущему значению</param>
    /// <param name="state">Наличие ошибки или предупреждения</param>
    /// <param name="errorMessage">Сообщение об ошибке или предупреждении</param>
    internal virtual void SetToolTip(Control control, string title, string mainInfo, string valueInfo, UIValidateState state, string errorMessage)
    {
#if DEBUG
      if (control == null)
        throw new ArgumentNullException("control");
#endif

      if (Parent != null)
        // Немедленная установка
        Parent.SetToolTip(control, title, mainInfo, valueInfo, state, errorMessage);
      else
      {
        // Отложенная установка
        if (_DelayedToolTips == null)
          _DelayedToolTips = new Dictionary<Control, SetToolTipInfo>();

        // Убираем предыдущую подсказку
        if (_DelayedToolTips.ContainsKey(control))
          _DelayedToolTips.Remove(control);

        SetToolTipInfo info = new SetToolTipInfo();
        info.Title = title;
        info.MainInfo = mainInfo;
        info.ValueInfo = valueInfo;
        info.State = state;
        info.ErrorMessage = errorMessage;

        _DelayedToolTips.Add(control, info);
      }
    }

    /// <summary>
    /// Сброс отложенных подсказок родителю
    /// </summary>
    private void FlushToolTipsToParent()
    {
      if (_DelayedToolTips == null)
        return;

      foreach (KeyValuePair<Control, SetToolTipInfo> Pair in _DelayedToolTips)
        Parent.SetToolTip(Pair.Key, Pair.Value.Title, Pair.Value.MainInfo, Pair.Value.ValueInfo,
          Pair.Value.State, Pair.Value.ErrorMessage);

      _DelayedToolTips = null;
    }

    #endregion

    #region Менеджер конфигурации

    /// <summary>
    /// Менеджер конфигурации.
    /// Если свойство не было установлено явно, возвращается свойство родительского элемента. 
    /// Если родительского элемента нет (<see cref="EFPFormProvider"/>), возвращается значение <see cref="EFPApp.ConfigManager"/>.
    /// Свойство никогда не возвращает null.
    /// </summary>
    public IEFPConfigManager ConfigManager
    {
      get
      {
        if (_ConfigManager == null)
        {
          if (Parent == null)
            return EFPApp.ConfigManager;
          else
            return Parent.ConfigManager;
        }
        else
          return _ConfigManager;
      }
      set
      {
        _ConfigManager = value;
      }
    }
    private IEFPConfigManager _ConfigManager;

    #endregion

    #region Проверка

    /// <summary>
    /// Проверка корректости.
    /// Рекурсивно вызывает <see cref="EFPControlBase.Validate()"/> для всех дочерних элементов и 
    /// <see cref="EFPFormCheck.Validate()"/> для всех проверяющих объектов.
    /// </summary>
    public void Validate()
    {
      if (_Children != null)
      {
        for (int i = 0; i < _Children.Count; i++)
          _Children[i].Validate();
      }
      if (_ControlProviders != null)
      {
        for (int i = 0; i < _ControlProviders.Count; i++)
          _ControlProviders[i].Validate();
      }
      if (_FormChecks != null)
      {
        for (int i = 0; i < _FormChecks.Count; i++)
          _FormChecks[i].Validate();
      }
    }

    /// <summary>
    /// Возвращает значение <see cref="EFPFormProvider.ValidateReason"/> или <see cref="EFPFormValidateReason.Unknown"/>, если текущий провайдер не присоединен к форме.
    /// </summary>
    public EFPFormValidateReason ValidateReason
    {
      get
      {
        if (FormProvider == null)
          return EFPFormValidateReason.Unknown;
        else
          return FormProvider.ValidateReason;
      }
    }

    /// <summary>
    /// Возвращает количество дочерних элементов с ошибками.
    /// Получает количество управляющих элементов с ошибками <see cref="EFPControlBase.ValidateState"/>=<see cref="UIValidateState.Error"/> плюс
    /// количество проверяющих объектов с <see cref="EFPFormCheck.ValidateState"/>=<see cref="UIValidateState.Error"/>.
    /// Вызов является рекурсивным.
    /// </summary>
    public int ErrorCount
    {
      get
      {
        int n = 0;
        if (_Children != null)
        {
          for (int i = 0; i < _Children.Count; i++)
            n += _Children[i].ErrorCount;
        }
        if (_ControlProviders != null)
        {
          for (int i = 0; i < _ControlProviders.Count; i++)
            n += _ControlProviders[i].ErrorCount;
        }
        if (_FormChecks != null)
        {
          for (int i = 0; i < _FormChecks.Count; i++)
            n += _FormChecks[i].ErrorCount;
        }
        return n;
      }
    }

    /// <summary>
    /// Возвращает количество дочерних элементов с предупреждениями.
    /// Получает количество управляющих элементов с предупреждениями <see cref="EFPControlBase.ValidateState"/>=<see cref="UIValidateState.Warning"/> плюс
    /// количество проверяющих объектов с <see cref="EFPFormCheck.ValidateState"/>=<see cref="UIValidateState.Warning"/>.
    /// Вызов является рекурсивным.
    /// </summary>
    public int WarningCount
    {
      get
      {
        int n = 0;
        if (_Children != null)
        {
          for (int i = 0; i < _Children.Count; i++)
            n += _Children[i].WarningCount;
        }
        if (_ControlProviders != null)
        {
          for (int i = 0; i < _ControlProviders.Count; i++)
            n += _ControlProviders[i].WarningCount;
        }
        if (_FormChecks != null)
        {
          for (int i = 0; i < _FormChecks.Count; i++)
            n += _FormChecks[i].WarningCount;
        }
        return n;
      }
    }

    /// <summary>
    /// Получает список сообщений об ошибках и предупреждений.
    /// Вызов является рекурсивным
    /// </summary>
    /// <param name="errorList">Заполняемый список</param>
    public void GetErrorMessages(List<EFPErrorInfo> errorList)
    {
      if (_Children != null)
      {
        for (int i = 0; i < _Children.Count; i++)
          _Children[i].GetErrorMessages(errorList);
      }
      if (_ControlProviders != null)
      {
        for (int i = 0; i < _ControlProviders.Count; i++)
          _ControlProviders[i].GetErrorMessages(errorList);
      }
      if (_FormChecks != null)
      {
        for (int i = 0; i < _FormChecks.Count; i++)
          _FormChecks[i].GetErrorMessages(errorList);
      }
    }

    /// <summary>
    /// Вызывает при смене одного из родительских провайдеров в цепочке.
    /// Посылает извещение всем дочерним элементам.
    /// </summary>
    public void ParentProviderChanged()
    {
      if (_Children != null)
      {
        for (int i = 0; i < _Children.Count; i++)
          _Children[i].ParentProviderChanged();
      }
      if (_ControlProviders != null)
      {
        for (int i = 0; i < _ControlProviders.Count; i++)
          _ControlProviders[i].ParentProviderChanged();
      }

      // Обязательно после рекурсивного вызова
      if (_Parent != null)
        FlushToolTipsToParent();
    }

    /// <summary>
    /// Отображаемое имя.
    /// Если свойство не задано в явном виде, возвращает <see cref="System.Type.ToString()"/>.
    /// </summary>
    public virtual string DisplayName
    {
      get
      {
        if (String.IsNullOrEmpty(_DisplayName))
          return GetType().ToString();
        else
          return _DisplayName;
      }
      set
      {
        _DisplayName = value;
      }
    }
    private string _DisplayName;

    /// <summary>
    /// Посылает дочерним элементам уведомление об изменении видимости формы.
    /// </summary>
    public virtual void FormVisibleChanged()
    {
      if (_Children != null)
      {
        for (int i = 0; i < _Children.Count; i++)
          _Children[i].FormVisibleChanged();
      }
      if (_ControlProviders != null)
      {
        for (int i = 0; i < _ControlProviders.Count; i++)
          _ControlProviders[i].FormVisibleChanged();
      }
      //if (_FormChecks != null)
      //{
      //  for (int i = 0; i < _FormChecks.Count; i++)
      //    _FormChecks[i].FormVisibleChanged();
      //}
    }

    #endregion

    #region Список команд меню

    /// <summary>
    /// Этот метод вызывается в <see cref="EFPControlBase.PrepareContextMenu()"/>, чтобы добавить в контекстное меню команды, относящиеся к родительским элементам.
    /// Если производный класс переопределяет метод, он должен вызвать базовый метод в конце, чтобы команды располагались в правильном порядке.
    /// Непереопределенный метод вызывает метод родительского объекта <see cref="Parent"/>.
    /// </summary>
    /// <param name="list">Заполняемый список команд</param>
    public virtual void InitCommandItemList(List<EFPCommandItems> list)
    {
      if (Parent != null)
        Parent.InitCommandItemList(list);
    }

    #endregion

    #region Отладочные свойства

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return DisplayName;
    }

#if DEBUG
#if XXX
    /// <summary>
    /// Отладочное свойство.
    /// Не должно использоваться.
    /// </summary>
    public IEFPCheckItem[] DebugCheckItems { get { return _Items.ToArray(); } set { } }

    /// <summary>
    /// Отладочное свойство.
    /// Не должно использоваться.
    /// </summary>
    public object[] DebugAllCheckItems
    {
      get
      {
        List<object> a = new List<object>();
        for (int i = 0; i < _Items.Count; i++)
        {
          a.Add((object)(_Items[i]));
          if (_Items[i] is EFPBaseProvider)
            a.AddRange(((EFPBaseProvider)(_Items[i])).DebugAllCheckItems);
        }
        return a.ToArray();
      }
      set
      {
      }
    }
#endif
#endif

    #endregion
  }

  /// <summary>
  /// Класс для проверки ошибок формы, не связанных с конкретным управляющим элементом
  /// </summary>
  public class EFPFormCheck : IUIValidableObject
  {
    #region Конструктор

    /// <summary>
    /// Создает пустой объект и добавляет его в переданный базовый провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер. Обычно проверка присоединяется непосредственно к форме <see cref="EFPFormProvider"/>.
    /// Не может быть null.</param>
    public EFPFormCheck(EFPBaseProvider baseProvider)
    {
      if (baseProvider == null)
        throw new ArgumentNullException("baseProvider");
      _BaseProvider = baseProvider;
      baseProvider.AddFormCheck(this);
    }

    /// <summary>
    /// Базовый провайдер, к которому присоединен объект. Задается в конструкторе.
    /// </summary>
    public EFPBaseProvider BaseProvider { get { return _BaseProvider; } }
    private readonly EFPBaseProvider _BaseProvider;

    #endregion

    #region Проверка

    /// <summary>
    /// Выполнить проверку значений
    /// </summary>
    public void Validate()
    {
      UIValidateState PrevState = _ValidateState;

      _ValidateState = UIValidateState.Ok;
      _ErrorMessage = null;

      if (Validating != null)
      {
        if (_ValidatingArgs == null)
          _ValidatingArgs = new UIValidatingEventArgs(this);
        Validating(this, _ValidatingArgs);
      }
      if (_ValidateState != PrevState)
      {
        // Состояние ошибки изменилось
        BaseProvider.ItemStateChanged();
      }
    }

    /// <summary>
    /// Возвращает 1, если установлено состояние ошибки
    /// </summary>
    internal int ErrorCount
    {
      get { return (ValidateState == UIValidateState.Error) ? 1 : 0; }
    }

    /// <summary>
    /// Возвращает 1, если установлено предупреждение
    /// </summary>
    internal int WarningCount
    {
      get { return (ValidateState == UIValidateState.Warning) ? 1 : 0; }
    }

    /// <summary>
    /// Добавляет в список единственное сообшение об ошибке или предупреждение.
    /// Для состояния OK не выполняет никаких действий.
    /// </summary>
    /// <param name="errorList">Заполняемый список</param>
    internal void GetErrorMessages(List<EFPErrorInfo> errorList)
    {
      if (ValidateState == UIValidateState.Ok)
        return;
      EFPErrorInfo Info = new EFPErrorInfo(ErrorMessage, ValidateState == UIValidateState.Error, FocusControl);
      errorList.Add(Info);
    }

    #endregion

    #region IEFPValidator Members

    /// <summary>
    /// Установить сообщение об ошибке.
    /// Если уже было установлена ошибка, вызов игнорируется.
    /// </summary>
    /// <param name="message">Текст сообщений</param>
    public void SetError(string message)
    {
      if (_ValidateState == UIValidateState.Error)
        return;
      _ValidateState = UIValidateState.Error;
      _ErrorMessage = message;
    }


    /// <summary>
    /// Установить предупреждение.
    /// Если уже было установлена ошибка или предупреждение, вызов игнорируется.
    /// </summary>
    /// <param name="message">Текст сообщений</param>
    public void SetWarning(string message)
    {
      if (_ValidateState != UIValidateState.Ok)
        return;
      _ValidateState = UIValidateState.Error;
      _ErrorMessage = message;
    }

    /// <summary>
    /// Текущее состояние проверки ошибок
    /// </summary>
    public UIValidateState ValidateState { get { return _ValidateState; } }
    private UIValidateState _ValidateState;

    #endregion

    #region Прочие свойства, методы и события проверки ошибок

    /// <summary>
    /// Используем один объект аргументов, чтобы не создавать каждый раз
    /// </summary>
    private UIValidatingEventArgs _ValidatingArgs;

    /// <summary>
    /// Текущее сообщение об ошибке или предупреждении
    /// </summary>
    public string ErrorMessage { get { return _ErrorMessage; } }
    private string _ErrorMessage;

    /// <summary>
    /// Пользовательский обработчик для проверки ошибок
    /// </summary>
    public event UIValidatingEventHandler Validating;

    /// <summary>
    /// Необязательный управляющий элемент, куда будет передаваться фокус ввода
    /// при выводе сообщения об ошибке.
    /// </summary>
    public Control FocusControl
    {
      get { return _FocusControl; }
      set { _FocusControl = value; }
    }
    private Control _FocusControl;

    /// <summary>
    /// Произвольные пользовательские данные
    /// </summary>
    public object Tag { get { return _Tag; } set { _Tag = value; } }
    private object _Tag;

    #endregion
  }

  /// <summary>
  /// Интерфейс объекта, выполняющего блокировку реентрантного вызова.
  /// Реализуется <see cref="EFPReentranceLocker"/>.
  /// </summary>
  public interface IEFPReentranceLocker
  {
    /// <summary>
    /// Выполнить попытку входа и установки блокировки
    /// </summary>
    /// <param name="displayName">Наименование действия, которое предполагается выполнить</param>
    /// <returns>true в случае успеха</returns>
    bool TryLock(string displayName);

    /// <summary>
    /// Завершение входа и снятие блокировки.
    /// </summary>
    void Unlock();
  }

  /// <summary>
  /// Блокировщик вложенного выполнения команд.
  /// В отличие от <see cref="System.Threading.Monitor"/> и других блокирующих классов Net Framework, этот класс предназначен
  /// для работы с одним потоком (основным потоком приложения).
  /// Используется <see cref="EFPFormProvider"/>, чтобы предотвратить закрытие формы, пока выполняется обработки нажатия
  /// кнопки, или предотвратить нажатие другой кнопки, если предыдущая обработка еще не выполнена.
  /// </summary>
  /// <remarks>
  /// Обработчик нажатия кнопки или другого события должен выполнить следующие действия:
  /// 1. Получить доступ к <see cref="EFPReentranceLocker"/>.
  /// 2. Вызвать <see cref="EFPReentranceLocker.TryLock(string)"/>. Если метод вернул false, обработчик завершается без выполнения действий.
  /// 3. Выполнить основное действие.
  /// 4. Вызвать <see cref="EFPReentranceLocker.Unlock()"/>. 
  /// Должен использоваться try-finally блок.
  /// </remarks>
  public class EFPReentranceLocker : IEFPReentranceLocker
  {
    #region Конструктор

    /// <summary>
    /// Конструктор
    /// </summary>
    public EFPReentranceLocker()
    {
    }


    #endregion

    #region Проверка потока

#if DEBUG

    private void CheckThread()
    {
      EFPApp.CheckMainThread();
    }

#endif

    #endregion

    #region Выполнение блокировки

    /// <summary>
    /// Выполнить попытку блокировки. Возвращает true, если блокировщик не занят.
    /// Иначе пользователю выводится сообщение с помощью <see cref="EFPApp.ShowTempMessage(string)"/> и возвращается false.
    /// </summary>
    /// <param name="displayName">Наименование действия, которое предполагается выполнить</param>
    /// <returns>true в случае успеха</returns>
    public bool TryLock(string displayName)
    {
#if DEBUG
      CheckThread();
#endif

      if (String.IsNullOrEmpty(displayName))
        displayName = "Неизвестное действие";

      if (_LockedDisplayName != null)
      {
        EFPApp.ShowTempMessage("Предыдущая операция еще не закончена: " + _LockedDisplayName);
        return false;
      }

      _LockedDisplayName = displayName;
      return true;
    }

    /// <summary>
    /// Снять ранее установленную блокировку
    /// </summary>
    public void Unlock()
    {
#if DEBUG
      CheckThread();
#endif
      if (_LockedDisplayName == null)
        throw new InvalidOperationException("Не было вызова TryLock()");
      _LockedDisplayName = null;
    }

    /// <summary>
    /// Наименование действия, которое удерживает блокировку.
    /// Это поле используется как флаг блокировки.
    /// </summary>
    private string _LockedDisplayName;

    /// <summary>
    /// Возвращает true, если есть установленная блокировка
    /// </summary>
    public bool IsLocked { get { return _LockedDisplayName != null; } }

    #endregion
  }

  /// <summary>
  /// Фиктивный блокировщик
  /// </summary>
  public sealed class EFPDummyReentranceLocker : IEFPReentranceLocker
  {
    #region IEFPReentranceLocker Members

    /// <summary>
    /// Возвращает true
    /// </summary>
    /// <param name="displayName">Игнорируется</param>
    /// <returns>Всегда true</returns>
    public bool TryLock(string displayName)
    {
      return true;
    }

    /// <summary>
    /// Ничего не делает
    /// </summary>
    public void Unlock()
    {
    }

    #endregion

    #region Статический экземпляр

    /// <summary>
    /// Статический экземпляр объекта
    /// </summary>
    public static readonly EFPDummyReentranceLocker TheLocker = new EFPDummyReentranceLocker();

    #endregion
  }
}
