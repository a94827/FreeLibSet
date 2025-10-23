// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Data.Docs;
using FreeLibSet.DependedValues;
using System.Windows.Forms;
using FreeLibSet.Data;
using FreeLibSet.Controls;
using FreeLibSet.UICore;
using FreeLibSet.Core;

namespace FreeLibSet.Forms.Docs
{
  /// <summary>
  /// Базовый класс для <see cref="EFPDocComboBoxBase"/> и <see cref="EFPMultiDocComboBoxBase"/>.
  /// Не содержит ссылок на идентификаторы Id или Ids
  /// </summary>
  public abstract class EFPAnyDocComboBoxBase : EFPUserSelComboBox
  {
    #region Константы

    /// <summary>
    /// Текст комбоблока по умолчанию, когда не выбран документ
    /// </summary>
    public static string DefaultEmptyText { get{return Res.EFPComboBox_Msg_Empty; } }

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
      _EmptyImageKey = String.Empty;
      _CanBeEmptyMode = UIValidateState.Error;
      control.ClearButton = false; // 08.11.2021
      control.PopupClick += new EventHandler(Control_PopupClick);
      control.ClearClick += new EventHandler(Control_ClearClick);

      ClearButtonEnabled = false;

      _CanBeDeletedMode = UIValidateState.Error;

      SelectableEx.ValueChanged += SelectableEx_ValueChanged;
    }

    #endregion

    #region Свойство UI

    /// <summary>
    /// Доступ к интерфейсу пользователя
    /// </summary>
    public DBUI UI { get { return _UI; } }
    private readonly DBUI _UI;

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
    /// Инициализирует текстовое представление и значок
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
        EFPApp.ShowException(e, String.Format(Res.EFPComboBox_ErrTitle_PopupClick, DisplayName));
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
    /// диалог выбора документа и установить требуемые свойства после выбора.
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
        EFPApp.ShowException(e, String.Format(Res.EFPComboBox_ErrTitle_ClearClick, DisplayName));
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
    /// Управляемое свойство для <see cref="EmptyText"/>.
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
        _EmptyTextEx = new DepInput<string>(EmptyText,EmptyTextEx_ValueChanged);
        _EmptyTextEx.OwnerInfo = new DepOwnerInfo(this, "EmptyTextEx");
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
    /// Изображение должно быть в коллекции <see cref="EFPApp.MainImages"/>.
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
    /// Управляемое свойство для <see cref="EmptyImageKey"/>.
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
        _EmptyImageKeyEx = new DepInput<string>(EmptyImageKey,EmptyImageKeyEx_ValueChanged);
        _EmptyImageKeyEx.OwnerInfo = new DepOwnerInfo(this, "EmptyImageKeyEx");
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
    /// Режим проверки пустого значения.
    /// По умолчанию - <see cref="UIValidateState.Error"/> (пустое значение не разрешается).
    /// Это свойство переопределяется для нестандартных элементов, содержащих
    /// кнопку очистки справа от элемента.
    /// </summary>
    public UIValidateState CanBeEmptyMode
    {
      get { return _CanBeEmptyMode; }
      set
      {
        if (value == _CanBeEmptyMode)
          return;
        _CanBeEmptyMode = value;
        Control.ClearButton = (value != UIValidateState.Error);
        Validate();
      }
    }
    private UIValidateState _CanBeEmptyMode;

    /// <summary>
    /// True, если ли элемент содержать пустое значение (нулевой идентификатор для элементов выбора единственного значения, или список нулевой длины для элементов выбора нескольких значений).
    /// Дублирует <see cref="CanBeEmptyMode"/>.
    /// </summary>
    public bool CanBeEmpty
    {
      get { return CanBeEmptyMode != UIValidateState.Error; }
      set { CanBeEmptyMode = value ? UIValidateState.Ok : UIValidateState.Error; }
    }

    #endregion

    #region Свойство CanBeDeleted

    /// <summary>
    /// Используется при проверке корректности введенного значения.
    /// Если установлено в <see cref="UIValidateState.Ok"/>, то разрешается выбирать удаленные документы или
    /// поддокументы. По умолчанию (<see cref="UIValidateState.Error"/>), если выбранный документ/поддокумент
    /// удален, то выдается ошибка.
    /// Также может выдаваться предупреждение.
    /// </summary>
    public UIValidateState CanBeDeletedMode
    {
      get { return _CanBeDeletedMode; }
      set
      {
        if (value == _CanBeDeletedMode)
          return;
        _CanBeDeletedMode = value;
        Validate();
      }
    }
    private UIValidateState _CanBeDeletedMode;

    /// <summary>
    /// Используется при проверке корректности введенного значения.
    /// Если установлено в true, то разрешается выбирать удаленные документы или
    /// поддокументы. По умолчанию (false), если выбранный документ/поддокумент
    /// удален, то выдается ошибка.
    /// Дубирует свойство <see cref="CanBeDeletedMode"/>.
    /// </summary>
    public bool CanBeDeleted
    {
      get { return CanBeDeletedMode != UIValidateState.Error; }
      set { CanBeDeletedMode = value ? UIValidateState.Ok : UIValidateState.Error; }
    }

    #endregion

    #region Выборка документов

    /// <summary>
    /// Свойство возвращает true, если объект поддерживает получение выборки документов.
    /// Это - константное свойство. Возвращаемое значение не зависит от текущего выбранного значения.
    /// </summary>
    public virtual bool GetDocSelSupported { get { return false; } }

    /// <summary>
    /// Свойство возвращает true, если объект поддерживает присвоение выборки документов.
    /// Это - константное свойство. Возвращаемое значение не зависит от текущего выбранного значения.
    /// </summary>
    public virtual bool SetDocSelSupported { get { return false; } }

    /// <summary>
    /// Общедоступный метод для получения выборки документов.
    /// Вызывает виртуальный метод <see cref="OnGetDocSel(EFPDBxViewDocSelReason)"/>.
    /// Для комбоблоков выбора документов возвращает выборку, содержащую выбранный документ
    /// (или документы) и, возможно, связанные документы.
    /// Для комбоблоков выбора поддокументов возвращает документ-владелец и, возможно, связанные документы.
    /// Если нет выбранных документов в комбоблоке, то возвращает null.
    /// </summary>
    /// <param name="reason">Причина получения выборки</param>
    /// <returns>Выборка документов</returns>
    public DBxDocSelection PerformGetDocSel(EFPDBxViewDocSelReason reason)
    {
      return OnGetDocSel(reason);
    }

    /// <summary>
    /// Получение выборки документов.
    /// Непереопределенный метод возвращает null.
    /// </summary>
    /// <param name="reason">Причина получения выборки</param>
    /// <returns>Выборка документов</returns>
    protected virtual DBxDocSelection OnGetDocSel(EFPDBxViewDocSelReason reason)
    {
      return null;
    }

    /// <summary>
    /// Общедоступный метод присвоения выборки документов.
    /// Вызывает виртуальный метод <see cref="OnSetDocSel(DBxDocSelection)"/>.
    /// </summary>
    /// <param name="docSel">Выборка документов</param>
    public void PerformSetDocSel(DBxDocSelection docSel)
    {
      OnSetDocSel(docSel);
    }

    /// <summary>
    /// Присвоение выборки документов.
    /// Непереопределенный метод вызывает исключение.
    /// </summary>
    /// <param name="docSel">Выборка документов</param>
    protected virtual void OnSetDocSel(DBxDocSelection docSel)
    {
      throw ExceptionFactory.MustBeReimplemented(this, "OnSetDocSel");
    }

    /// <summary>
    /// Возвращает true, если получение информации о документе должно быть доступно.
    /// Используется <see cref="EFPDocComboBox"/>.
    /// Это - константное свойство. Возвращаемое значение не зависит от текущего выбранного значения.
    /// </summary>
    public virtual bool DocInfoSupported { get { return false; } }

    #endregion

    #region Локальное меню

    /// <summary>
    /// Возвращает набор команд <see cref="EFPAnyDocComboBoxBaseCommandItems"/>.
    /// </summary>
    /// <returns></returns>
    protected override EFPControlCommandItems CreateCommandItems()
    {
      if (!GetDocSelSupported)
        return base.CreateCommandItems();

      EFPAnyDocComboBoxBaseCommandItems items = new EFPAnyDocComboBoxBaseCommandItems(this);
      items.InitEnabled();
      return items;
    }


    void SelectableEx_ValueChanged(object sender, EventArgs args)
    {
      InitTextAndImage(); // могла поменяться кнопка редактирования

      if (CommandItemsAssigned)
      {
        if (CommandItems is EFPAnyDocComboBoxBaseCommandItems)
          ((EFPAnyDocComboBoxBaseCommandItems)CommandItems).InitEnabled();
      }
    }

    #endregion
  }


  /// <summary>
  /// Команды локального меню для <see cref="EFPAnyDocComboBoxBase"/>
  /// </summary>
  public class EFPAnyDocComboBoxBaseCommandItems : EFPControlCommandItems
  {
    #region Конструктор

    /// <summary>
    /// Создает набор команд
    /// </summary>
    /// <param name="controlProvider">Провайдер комбоблока</param>
    public EFPAnyDocComboBoxBaseCommandItems(EFPAnyDocComboBoxBase controlProvider)
      :base(controlProvider)
    {
      if (controlProvider.SetDocSelSupported)
      {
        ciCut = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Cut);
        ciCut.GroupBegin = true;
        ciCut.Click += new EventHandler(ciCut_Click);
        Add(ciCut);
      }

      if (controlProvider.GetDocSelSupported)
      {
        ciCopy = EFPApp.CommandItems.CreateContext(EFPAppStdCommandItems.Copy);
        ciCopy.Click += new EventHandler(ciCopy_Click);
        Add(ciCopy);
      }

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
        ciShowDocInfo.MenuText = Res.Cmd_Menu_DocInfo;
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

    /// <summary>
    /// Провайдер управляющего элемента
    /// </summary>
    protected new EFPAnyDocComboBoxBase ControlProvider { get { return (EFPAnyDocComboBoxBase)(base.ControlProvider); } }

    #endregion

    #region Инициализация доступности команд

    /// <summary>
    /// Управляет доступностью команд, в зависимости от наличия выбранной записи
    /// </summary>
    public void InitEnabled()
    {
      if (ciCut != null)
        ciCut.Enabled = ControlProvider.IsNotEmpty && ControlProvider.Selectable;
      if (ciCopy != null)
        ciCopy.Enabled = ControlProvider.IsNotEmpty;
      if (ciPaste != null)
        ciPaste.Enabled = ControlProvider.Selectable;

      if (ciShowDocInfo != null)
      {
        // 11.04.2016
        DBxDocSelection docSel;
        try
        {
          docSel = ControlProvider.PerformGetDocSel(EFPDBxViewDocSelReason.Copy);
          UserPermissions ups = ControlProvider.UI.DocProvider.UserPermissions;
          if (docSel.IsEmpty || ups == null)
            ciShowDocInfo.Enabled = false;
          else
          {
            string docTypeName = docSel.TableNames[0];
            ciShowDocInfo.Enabled = DocTypeViewHistoryPermission.GetAllowed(ups, docTypeName);
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

    private readonly EFPCommandItem ciCut, ciCopy, ciPaste;

    void ciCut_Click(object sender, EventArgs args)
    {
      ciCopy_Click(null, null);
      ControlProvider.Clear();
    }

    void ciCopy_Click(object sender, EventArgs args)
    {
      if (!ControlProvider.IsNotEmpty)
      {
        EFPApp.ShowTempMessage(Res.EFPComboBox_Err_IsEmpty);
        return;
      }
      DBxDocSelection docSel = ControlProvider.PerformGetDocSel(EFPDBxViewDocSelReason.Copy);
      DataObject dObj = new DataObject();
      dObj.SetData(docSel);
      ControlProvider.UI.OnAddCopyFormats(dObj, docSel); // 06.02.2021
      dObj.SetText(ControlProvider.Control.Text);
      new EFPClipboard().SetDataObject(dObj, true);
    }

    void ciPaste_Click(object sender, EventArgs args)
    {
      DBxDocSelection docSel = ControlProvider.UI.PasteDocSel();
      if (docSel == null)
      {
        EFPApp.ShowTempMessage(Res.Clipboard_Err_NoDocSel);
        return;
      }
      // Сами не нормализуем
      // Это может делать виртуальный метод SetDocSel()
      ControlProvider.PerformSetDocSel(docSel);
    }

    #endregion

    #region Информация о документе

    private readonly EFPCommandItem ciShowDocInfo;

    void ciShowDocInfo_Click(object sender, EventArgs args)
    {
      if (!ControlProvider.IsNotEmpty)
      {
        EFPApp.ShowTempMessage(Res.EFPComboBox_Err_IsEmpty);
        return;
      }
      DBxDocSelection docSel = ControlProvider.PerformGetDocSel(EFPDBxViewDocSelReason.Copy);
      if (docSel.IsEmpty)
      {
        EFPApp.ShowTempMessage(Res.Common_ToolTipText_NoDoc);
        return;
      }

      // Наш тип документа - первый в списке
      string docTypeName = docSel.TableNames[0];
      Int32 docId = docSel[docTypeName].SingleId;

      DocTypeUI dtui = ControlProvider.UI.DocTypes[docTypeName];
      dtui.ShowDocInfo(docId);
    }

    #endregion
  }
}
