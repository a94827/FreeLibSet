using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Data.Docs;
using FreeLibSet.DependedValues;
using System.Windows.Forms;
using FreeLibSet.Data;
using FreeLibSet.Controls;

namespace FreeLibSet.Forms.Docs
{
  /// <summary>
  /// Базовый класс для EFPDocComboBoxBase и EFPMultiDocComboBoxBase.
  /// Не содержит ссылок на идентификаторы Id или Ids
  /// </summary>
  public abstract class EFPAnyDocComboBoxBase : EFPUserSelComboBox
  {
    #region Константы

    /// <summary>
    /// Текст комбоблока по умолчанию, когда не выбран документ
    /// </summary>
    public const string DefaultEmptyText = "[ нет ]";

    #endregion

    #region Конструктор

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент</param>
    /// <param name="ui">Доступ к интерфейсу пользователя</param>
    protected EFPAnyDocComboBoxBase(EFPBaseProvider baseProvider, UserSelComboBox control, DBUI ui)
      : base(baseProvider, control)
    {
      if (ui == null)
        throw new ArgumentNullException("ui");
      _UI = ui;

      _EmptyText = DefaultEmptyText;
      control.ClearButton = true;
      control.PopupClick += new EventHandler(Control_PopupClick);
      control.ClearClick += new EventHandler(Control_ClearClick);

      ClearButtonEnabled = false;

      _CanBeDeleted = false;

      SelectableEx.ValueChanged += SelectableEx_ValueChanged;
    }

    #endregion

    #region Свойство UI

    /// <summary>
    /// Доступ к интерфейсу пользователя
    /// </summary>
    public DBUI UI { get { return _UI; } }
    private DBUI _UI;

    #endregion

    #region InitTextAndImage

    /// <summary>
    /// Вызывает InitTextAndImage()
    /// </summary>
    protected override void OnCreated()
    {
      base.OnCreated();
      InitTextAndImage();
    }


    /// <summary>
    /// Инициализирует текстовое представление (свойство TextValue) и значок
    /// </summary>
    protected abstract void InitTextAndImage();

    /// <summary>
    /// Принудительное обновление текста и изображения в выпадающем списке
    /// </summary>
    public void UpdateText()
    {
      InitTextAndImage();
    }

    #endregion

    #region Выбор значения Popup

    private void Control_PopupClick(object sender, EventArgs args)
    {
      try
      {
        PerformPopup();
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка при выборе из комбоблока \"" + DisplayName + "\"");
        //EFPApp.MessageBox(e.Message, "Ошибка при выборе из списка",
        //  MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    /// <summary>
    /// Этот метод должен вызвать блок диалога и установить значение RefId.ValueEx
    /// </summary>
    protected abstract void DoPopup();

    /// <summary>
    /// Выполнить действия, аналогичные нажатию стрелочки выпадающего списка
    /// </summary>
    public void PerformPopup()
    {
      if (Popup == null)
        DoPopup();
      else
        Popup(this, EventArgs.Empty);
    }

    /// <summary>
    /// Если обработчик события установлен, то он вызывается вместо вывода
    /// стандартного диалога. Обработчик должен вывести пользователю собственный
    /// диалог выбора документа и установить требуемые свойства после выбора
    /// </summary>
    public event EventHandler Popup;

    #endregion

    #region Очистка значения Clear

    private void Control_ClearClick(object sender, EventArgs args)
    {
      try
      {
        Clear(); // здесь тоже может возникнуть иключение
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка очистки значения");
      }
    }

    /// <summary>
    /// Очищает выбранное значение.
    /// </summary>
    public abstract void Clear();

    /// <summary>
    /// Свойство возвращает true, если в комбоблоке выбрано значение
    /// </summary>
    public abstract bool IsNotEmpty { get;}

    #endregion

    #region Свойство EmptyText

    /// <summary>
    /// Текст, выводимый в комбоблоке, когда нет выбранного значения.
    /// (по умолчанию "[ нет ]")
    /// </summary>
    public string EmptyText
    {
      get { return _EmptyText; }
      set
      {
        if (value == null)
          value = String.Empty; // 13.06.2019. Нужно для правильной работы переходника DocValueAnyDocComboBoxBase

        if (value == _EmptyText)
          return;
        _EmptyText = value;
        if (_EmptyTextEx != null)
          _EmptyTextEx.Value = value;
        InitTextAndImage();
      }
    }
    private string _EmptyText;

    /// <summary>
    /// Текст, выводимый в комбоблоке, когда нет выбранного значения.
    /// Управляемое свойство для EmptyText.
    /// </summary>
    public DepValue<String> EmptyTextEx
    {
      get
      {
        InitEmptyTextEx();
        return _EmptyTextEx;
      }
      set
      {
        InitEmptyTextEx();
        _EmptyTextEx.Source = value;
      }
    }

    private void InitEmptyTextEx()
    {
      if (_EmptyTextEx == null)
      {
        _EmptyTextEx = new DepInput<string>();
        _EmptyTextEx.OwnerInfo = new DepOwnerInfo(this, "EmptyTextEx");
        _EmptyTextEx.Value = EmptyText;
        _EmptyTextEx.ValueChanged += new EventHandler(EmptyTextEx_ValueChanged);
      }
    }

    private DepInput<String> _EmptyTextEx;

    void EmptyTextEx_ValueChanged(object sender, EventArgs args)
    {
      EmptyText = _EmptyTextEx.Value;
    }

    #endregion

    #region Свойство EmptyImageKey

    /// <summary>
    /// Значок, выводимый в комбоблоке, когда нет выбранного значения.
    /// Изображение должно быть в коллекции EFPApp.MainImages.
    /// По умолчанию "" - нет значка.
    /// </summary>
    public string EmptyImageKey
    {
      get { return _EmptyImageKey; }
      set
      {
        if (value == _EmptyImageKey)
          return;
        _EmptyImageKey = value;
        if (_EmptyImageKeyEx != null)
          _EmptyImageKeyEx.Value = value;
        InitTextAndImage();
      }
    }
    private string _EmptyImageKey;

    /// <summary>
    /// Значок, выводимый в комбоблоке, когда нет выбранного значения.
    /// Управляемое свойство для EmptyImageKey.
    /// </summary>
    public DepValue<String> EmptyImageKeyEx
    {
      get
      {
        InitEmptyImageKeyEx();
        return _EmptyImageKeyEx;
      }
      set
      {
        InitEmptyImageKeyEx();
        _EmptyImageKeyEx.Source = value;
      }
    }

    private void InitEmptyImageKeyEx()
    {
      if (_EmptyImageKeyEx == null)
      {
        _EmptyImageKeyEx = new DepInput<string>();
        _EmptyImageKeyEx.OwnerInfo = new DepOwnerInfo(this, "EmptyImageKeyEx");
        _EmptyImageKeyEx.Value = EmptyImageKey;
        _EmptyImageKeyEx.ValueChanged += new EventHandler(EmptyImageKeyEx_ValueChanged);
      }
    }

    private DepInput<String> _EmptyImageKeyEx;

    void EmptyImageKeyEx_ValueChanged(object sender, EventArgs args)
    {
      EmptyImageKey = _EmptyImageKeyEx.Value;
    }

    #endregion

    #region Свойство CanBeEmpty

    /// <summary>
    /// Используется при проверке корректности введенного значения.
    /// True, если можно выбирать пустое значение. По умолчанию - true.
    /// Определяет видимость кнопки "Нет" при выборе из справочника, наличие кнопочки
    /// "X" рядом с комбоблоком. При значении false выполняется проверка ошибки.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return Control.ClearButton; }
      set
      {
        if (value == Control.ClearButton)
          return;
        Control.ClearButton = value;
        if (_CanBeEmptyEx != null)
          _CanBeEmptyEx.Value = value;
        Validate();
      }
    }

    /// <summary>
    /// Используется при проверке корректности введенного значения.
    /// True, если можно выбирать пустое значение. 
    /// Управляемое свойство для CanBeEmpty.
    /// </summary>
    public DepValue<Boolean> CanBeEmptyEx
    {
      get
      {
        InitCanBeEmptyEx();
        return _CanBeEmptyEx;
      }
      set
      {
        InitCanBeEmptyEx();
        _CanBeEmptyEx.Source = value;
      }
    }

    private void InitCanBeEmptyEx()
    {
      if (_CanBeEmptyEx == null)
      {
        _CanBeEmptyEx = new DepInput<bool>();
        _CanBeEmptyEx.OwnerInfo = new DepOwnerInfo(this, "CanBeEmptyEx");
        _CanBeEmptyEx.Value = CanBeEmpty;
        _CanBeEmptyEx.ValueChanged += new EventHandler(CanBeEmptyEx_ValueChanged);
      }
    }

    private DepInput<Boolean> _CanBeEmptyEx;

    void CanBeEmptyEx_ValueChanged(object sender, EventArgs args)
    {
      CanBeEmpty = _CanBeEmptyEx.Value;
    }

    #endregion

    #region Свойство WarningIfEmpty

    /// <summary>
    /// Используется при проверке корректности введенного значения.
    /// Выдавать предупреждение, если значение не выбрано, при условии, что CanBeEmpty=true.
    /// Иначе свойство WarningIfEmpty игнорируется.
    /// </summary>
    public bool WarningIfEmpty
    {
      get { return _WarningIfEmpty; }
      set
      {
        if (value == _WarningIfEmpty)
          return;
        _WarningIfEmpty = value;
        if (_WarningIfEmptyEx != null)
          _WarningIfEmptyEx.Value = value;
        Validate();
      }
    }
    private bool _WarningIfEmpty;

    /// <summary>
    /// Используется при проверке корректности введенного значения.
    /// Выдавать предупреждение, если значение не выбрано, при условии, что CanBeEmpty=true.
    /// Управляемое свойство для WarningIfEmpty
    /// </summary>
    public DepValue<Boolean> WarningIfEmptyEx
    {
      get
      {
        InitWarningIfEmptyEx();
        return _WarningIfEmptyEx;
      }
      set
      {
        InitWarningIfEmptyEx();
        _WarningIfEmptyEx.Source = value;
      }
    }

    private void InitWarningIfEmptyEx()
    {
      if (_WarningIfEmptyEx == null)
      {
        _WarningIfEmptyEx = new DepInput<bool>();
        _WarningIfEmptyEx.OwnerInfo = new DepOwnerInfo(this, "WarningIfEmptyEx");
        _WarningIfEmptyEx.Value = WarningIfEmpty;
        _WarningIfEmptyEx.ValueChanged += new EventHandler(WarningIfEmptyEx_ValueChanged);
      }
    }
    private DepInput<Boolean> _WarningIfEmptyEx;

    void WarningIfEmptyEx_ValueChanged(object sender, EventArgs args)
    {
      WarningIfEmpty = _WarningIfEmptyEx.Value;
    }

    #endregion

    #region Свойства CanBeDeleted и WarningIfDeleted

    /// <summary>
    /// Используется при проверке корректности введенного значения.
    /// Если установлено в true, то разрешается выбирать удаленные документы или
    /// поддокументы. По умолчанию (false), если выбранный документ/поддокумент
    /// удален, то выдается ошибка или предупреждение в зависимости от свойства
    /// WarningIfDeleted
    /// </summary>
    public bool CanBeDeleted
    {
      get { return _CanBeDeleted; }
      set
      {
        if (value == _CanBeDeleted)
          return;
        _CanBeDeleted = value;
        if (_CanBeDeletedEx != null)
          _CanBeDeletedEx.Value = value;
        Validate();
      }
    }
    private bool _CanBeDeleted;

    /// <summary>
    /// Используется при проверке корректности введенного значения.
    /// Если установлено в true, то разрешается выбирать удаленные документы или
    /// поддокументы. 
    /// Управляемое свойство для CanBeDeleted
    /// </summary>
    public DepValue<Boolean> CanBeDeletedEx
    {
      get
      {
        InitCanBeDeletedEx();
        return _CanBeDeletedEx;
      }
      set
      {
        InitCanBeDeletedEx();
        _CanBeDeletedEx.Source = value;
      }
    }

    private void InitCanBeDeletedEx()
    {
      if (_CanBeDeletedEx == null)
      {
        _CanBeDeletedEx = new DepInput<bool>();
        _CanBeDeletedEx.OwnerInfo = new DepOwnerInfo(this, "CanBeDeletedEx");
        _CanBeDeletedEx.Value = CanBeDeleted;
        _CanBeDeletedEx.ValueChanged += new EventHandler(CanDeletedEx_ValueChanged);
      }
    }

    private DepInput<Boolean> _CanBeDeletedEx;

    void CanDeletedEx_ValueChanged(object sender, EventArgs args)
    {
      CanBeDeleted = _CanBeDeletedEx.Value;
    }


    /// <summary>
    /// Используется при проверке корректности введенного значения.
    /// Если свойство CanBeDeleted не установлено, а выбранный документ/поддокумент 
    /// удален, то выдается ошибка (при значении false) или предупреждение (при
    /// значение true). Значение по умолчанию - false (ошибка).
    /// При CanBeDeleted=true, свойство WarningIfDeleted игнорируется.
    /// </summary>
    public bool WarningIfDeleted
    {
      get { return _WarningIfDeleted; }
      set
      {
        if (value == _WarningIfDeleted)
          return;
        _WarningIfDeleted = value;
        if (_WarningIfDeletedEx != null)
          _WarningIfDeletedEx.Value = value;
        Validate();
      }
    }
    private bool _WarningIfDeleted;


    /// <summary>
    /// Используется при проверке корректности введенного значения.
    /// Если свойство CanBeDeleted не установлено, а выбранный документ/поддокумент 
    /// удален, то выдается ошибка (при значении false) или предупреждение (при
    /// значение true). 
    /// Управляемое свойство для WarningIfDeleted.
    /// </summary>
    public DepValue<Boolean> WarningIfDeletedEx
    {
      get
      {
        InitWarningIfDeletedEx();
        return _WarningIfDeletedEx;
      }
      set
      {
        InitWarningIfDeletedEx();
        _WarningIfDeletedEx.Source = value;
      }
    }

    private void InitWarningIfDeletedEx()
    {
      if (_WarningIfDeletedEx == null)
      {
        _WarningIfDeletedEx = new DepInput<bool>();
        _WarningIfDeletedEx.OwnerInfo = new DepOwnerInfo(this, "WarningIfDeletedEx");
        _WarningIfDeletedEx.Value = WarningIfDeleted;
        _WarningIfDeletedEx.ValueChanged += new EventHandler(WarningIfDeletedEx_ValueChanged);
      }
    }

    private DepInput<Boolean> _WarningIfDeletedEx;

    void WarningIfDeletedEx_ValueChanged(object sender, EventArgs args)
    {
      WarningIfDeleted = _WarningIfDeletedEx.Value;
    }

    #endregion

    #region Выборка документов

    /// <summary>
    /// Свойство возвращает true, если объект поддерживает получение выборки документов.
    /// Это - константное свойство. Возвращаемое значение не зависит от текущего выбранного значения.
    /// </summary>
    public virtual bool GetDocSelSupported { get { return false; } }

    /// <summary>
    /// Свойство возвращает true, если объект поддерживает присвоение выборки документов
    /// Это - константное свойство. Возвращаемое значение не зависит от текущего выбранного значения.
    /// </summary>
    public virtual bool SetDocSelSupported { get { return false; } }

    /// <summary>
    /// Общедоступный метод для получения выборки документов.
    /// Вызывает виртуальный метод OnGetDocSel().
    /// Для комбоблоков выбора документов возвращает выборку, содержащую выбранный документ
    /// (или документы) и, возможно, связанные документы.
    /// Для комбоблоков выбора поддокументов возвращает документ-владелец и, возможно, связанные документы.
    /// Если нет выбранных документов в комбоблоке, то возвращает null.
    /// </summary>
    /// <param name="reason">Причина получения выборки</param>
    /// <returns>Выборка документов</returns>
    public DBxDocSelection PerformGetDocSel(EFPDBxGridViewDocSelReason reason)
    {
      return OnGetDocSel(reason);
    }

    /// <summary>
    /// Получение выборки документов.
    /// Непереопределенный метод возвращает null.
    /// </summary>
    /// <param name="reason">Причина получения выборки</param>
    /// <returns>Выборка документов</returns>
    protected virtual DBxDocSelection OnGetDocSel(EFPDBxGridViewDocSelReason reason)
    {
      return null;
    }

    /// <summary>
    /// Общедоступный метод присвоения выборки документов.
    /// Вызывает виртуальный метод OnSetDocSel().
    /// </summary>
    /// <param name="docSel">Выборка документов</param>
    public void PerformSetDocSel(DBxDocSelection docSel)
    {
      OnSetDocSel(docSel);
    }

    /// <summary>
    /// Присвоение выборки документов.
    /// Непереопределенный метод вызывает исключение
    /// </summary>
    /// <param name="docSel">Выборка документов</param>
    protected virtual void OnSetDocSel(DBxDocSelection docSel)
    {
      throw new NotSupportedException("Присвоение выборки документов не реализовано");
    }

    /// <summary>
    /// Возвращает true, если получение информации о документе должно быть доступно.
    /// Используется EFPDocComboBox.
    /// Это - константное свойство. Возвращаемое значение не зависит от текущего выбранного значения.
    /// </summary>
    public virtual bool DocInfoSupported { get { return false; } }

    #endregion

    #region Локальное меню

    /// <summary>
    /// Возвращает набор команд EFPAnyDocComboBoxBaseControlItems.
    /// </summary>
    /// <returns></returns>
    protected override EFPControlCommandItems GetCommandItems()
    {
      if (!GetDocSelSupported)
        return base.GetCommandItems();

      EFPAnyDocComboBoxBaseControlItems Items = new EFPAnyDocComboBoxBaseControlItems(this);
      Items.InitEnabled();
      return Items;
    }


    void SelectableEx_ValueChanged(object sender, EventArgs args)
    {
      InitTextAndImage(); // могла поменяться кнопка редактирования

      if (CommandItemsAssigned)
      {
        if (CommandItems is EFPAnyDocComboBoxBaseControlItems)
          ((EFPAnyDocComboBoxBaseControlItems)CommandItems).InitEnabled();
      }
    }

    #endregion
  }


  /// <summary>
  /// Команды локального меню
  /// </summary>
  public class EFPAnyDocComboBoxBaseControlItems : EFPControlCommandItems
  {
    #region Конструктор

    /// <summary>
    /// Создает набор команд
    /// </summary>
    /// <param name="controlProvider">Провайдер комбоблока</param>
    public EFPAnyDocComboBoxBaseControlItems(EFPAnyDocComboBoxBase controlProvider)
    {
      _ControlProvider = controlProvider;

      if (controlProvider.SetDocSelSupported)
      {
        ciCut = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Cut);
        ciCut.GroupBegin = true;
        ciCut.Click += new EventHandler(ciCut_Click);
        Add(ciCut);
      }

      ciCopy = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Copy);
      ciCopy.Click += new EventHandler(ciCopy_Click);
      Add(ciCopy);

      if (controlProvider.SetDocSelSupported)
      {
        ciPaste = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Paste);
        ciPaste.GroupEnd = true;
        ciPaste.Click += new EventHandler(ciPaste_Click);
        Add(ciPaste);
      }

      if (controlProvider.DocInfoSupported)
      {
        ciShowDocInfo = new EFPCommandItem("View", "DocInfo");
        ciShowDocInfo.MenuText = "Информация о документе";
        ciShowDocInfo.ShortCut = Keys.F12;
        ciShowDocInfo.ImageKey = "Information";
        ciShowDocInfo.Click += new EventHandler(ciShowDocInfo_Click);
        ciShowDocInfo.GroupBegin = true;
        ciShowDocInfo.GroupEnd = true;
        Add(ciShowDocInfo);
      }
    }

    #endregion

    #region Свойства

    private EFPAnyDocComboBoxBase _ControlProvider;

    #endregion

    #region Инициализация доступности команд

    /// <summary>
    /// Управляет доступностью команд, в зависимости от наличия выбранной записи
    /// </summary>
    public void InitEnabled()
    {
      if (ciCut != null)
        ciCut.Enabled = _ControlProvider.IsNotEmpty && _ControlProvider.Selectable;
      if (ciCopy != null)
        ciCopy.Enabled = _ControlProvider.IsNotEmpty;
      if (ciPaste != null)
        ciPaste.Enabled = _ControlProvider.Selectable;

      if (ciShowDocInfo != null)
      {
        // 11.04.2016
        DBxDocSelection DocSel;
        try
        {
          DocSel = _ControlProvider.PerformGetDocSel(EFPDBxGridViewDocSelReason.Copy);
          UserPermissions ups = _ControlProvider.UI.DocProvider.UserPermissions;
          if (DocSel.IsEmpty || ups == null)
            ciShowDocInfo.Enabled = false;
          else
          {
            string DocTypeName = DocSel.TableNames[0];
            ciShowDocInfo.Enabled = DocTypeViewHistoryPermission.GetAllowed(ups, DocTypeName);
          }
        }
        catch
        {
          ciShowDocInfo.Enabled = false;
        }
      }
    }

    #endregion

    #region Команды буфера обмена

    private EFPCommandItem ciCut, ciCopy, ciPaste;

    void ciCut_Click(object sender, EventArgs args)
    {
      ciCopy_Click(null, null);
      _ControlProvider.Clear();
    }

    void ciCopy_Click(object sender, EventArgs args)
    {
      if (!_ControlProvider.IsNotEmpty)
      {
        EFPApp.ShowTempMessage("Значение не выбрано");
        return;
      }
      DBxDocSelection DocSel = _ControlProvider.PerformGetDocSel(EFPDBxGridViewDocSelReason.Copy);
      DataObject DObj = new DataObject();
      DObj.SetData(DocSel);
      _ControlProvider.UI.OnAddCopyFormats(DObj, DocSel); // 06.02.2021
      DObj.SetText(_ControlProvider.Control.Text);
      EFPApp.Clipboard.SetDataObject(DObj, true);
    }

    void ciPaste_Click(object sender, EventArgs args)
    {
      DBxDocSelection DocSel = _ControlProvider.UI.PasteDocSel();
      if (DocSel == null)
      {
        EFPApp.ShowTempMessage("Буфер обмена не содержит ссылок на документы");
        return;
      }
      // Сами не нормализуем
      // Это может делать виртуальный метод SetDocSel()
      _ControlProvider.PerformSetDocSel(DocSel);
    }

    #endregion

    #region Информация о документе

    EFPCommandItem ciShowDocInfo;

    void ciShowDocInfo_Click(object sender, EventArgs args)
    {
      if (!_ControlProvider.IsNotEmpty)
      {
        EFPApp.ShowTempMessage("Значение не выбрано");
        return;
      }
      DBxDocSelection DocSel = _ControlProvider.PerformGetDocSel(EFPDBxGridViewDocSelReason.Copy);
      if (DocSel.IsEmpty)
      {
        EFPApp.ShowTempMessage("Нет выбранного документа");
        return;
      }

      // Наш тип документа - первый в списке
      string DocTypeName = DocSel.TableNames[0];
      Int32 DocId = DocSel[DocTypeName][0];

      DocTypeUI dtui = _ControlProvider.UI.DocTypes[DocTypeName];
      dtui.ShowDocInfo(DocId);
    }

    #endregion
  }
}
