// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Controls;
using FreeLibSet.Data.Docs;
using FreeLibSet.DependedValues;
using System.Data;
using FreeLibSet.Core;

// TODO: Недоделано. В частности, не реализованы операции с буфером обмена

namespace FreeLibSet.Forms.Docs
{
  #region Делегаты

  /// <summary>
  /// Аргументы события EFPAllSubDocComboBox.TextValueNeeded
  /// </summary>
  public class EFPAllSubDocComboBoxTextValueNeededEventArgs : EFPComboBoxTextValueNeededEventArgs
  {
    #region Конструктор

    /// <summary>
    /// Создается комбоблоком
    /// </summary>
    /// <param name="owner">Объект-владелец</param>
    public EFPAllSubDocComboBoxTextValueNeededEventArgs(EFPAllSubDocComboBox owner)
    {
      _Owner = owner;
    }

    #endregion

    #region Свойства

    private EFPAllSubDocComboBox _Owner;

    // Пока нет дополнительных свойств

    #endregion
  }

  /// <summary>
  /// Делегат события EFPAllSubDocComboBox.TextValueNeeded
  /// </summary>
  /// <param name="sender">Комбоблок</param>
  /// <param name="args">Аргументы события</param>
  public delegate void EFPAllSubDocComboBoxTextValueNeededEventHandler(object sender,
    EFPAllSubDocComboBoxTextValueNeededEventArgs args);

  #endregion

  /// <summary>
  /// Провайдер комбоблока, который предназначен для показа таблицы EFPSubDocGridView в редакторе документа.
  /// Используется, в основном, для поддокументов, предназначенных для создания отношения "многие-ко-многим".
  /// Комбоблок не используется для выбора чего-либо, а только для замены постоянно присутствующей в редакторе таблички поддокументов.
  /// </summary>
  public class EFPAllSubDocComboBox : EFPUserSelComboBox
  {
    #region Конструкторы

    /// <summary>
    /// Создает провайдер для встраивания в редактор документа
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент комбоблока</param>
    /// <param name="mainEditor">Редактор основного документа, на вкладку которого добавляется комбоблок</param>
    /// <param name="subDocs">Список просматриваемых поддокументов</param>
    public EFPAllSubDocComboBox(EFPBaseProvider baseProvider, UserSelComboBox control, DocumentEditor mainEditor, DBxMultiSubDocs subDocs)
      : base(baseProvider, control)
    {
      Init(mainEditor, subDocs, mainEditor.UI);
    }

    /// <summary>
    /// Создает провайдер для автономного просмотра списка поддокументов без встраивания в редактор документа.
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент комбоблока</param>
    /// <param name="subDocs">Список просматриваемых поддокументов</param>
    /// <param name="ui">Пользовательский интерфейс для документов</param>
    public EFPAllSubDocComboBox(EFPBaseProvider baseProvider, UserSelComboBox control, DBxMultiSubDocs subDocs, DBUI ui)
      : base(baseProvider, control)
    {
      Init(null, subDocs, ui);
    }

    private void Init(DocumentEditor mainEditor, DBxMultiSubDocs subDocs, DBUI ui)
    {
#if DEBUG
      if (subDocs == null)
        throw new ArgumentNullException("subDocs");
      if (ui == null)
        throw new ArgumentNullException("ui");
#endif

      _MainEditor = mainEditor;

      _SubDocs = subDocs;
      _SubDocTypeUI = ui.DocTypes[subDocs.Owner.DocType.Name].SubDocTypes[subDocs.SubDocType.Name]; // чтобы свойство работало быстро

      _ValidateBeforeEdit = false;

      _ConfirmDeletion = true;

      Control.PopupClick += new EventHandler(Control_PopupClick);

      _MaxTextItemCount = 1;
      _TextValueNeededArgs = new EFPAllSubDocComboBoxTextValueNeededEventArgs(this);
      _EmptyText = EFPAnyDocComboBoxBase.DefaultEmptyText;
      _EmptyImageKey = String.Empty;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Основной редактор документа, использующий данный просмотр.
    /// Задается в конструкторе.
    /// Может быть null, если просмотр используется вне редактора.
    /// </summary>
    public DocumentEditor MainEditor { get { return _MainEditor; } }
    private DocumentEditor _MainEditor;

    /// <summary>
    /// Тип редактируемых поддокументов. Задается в конструкторе.
    /// </summary>
    public DBxSubDocType SubDocType { get { return _SubDocTypeUI.SubDocType; } }

    /// <summary>
    /// Редактируемые поддокументы (объект доступа). Определяется в конструкторе.
    /// </summary>
    public DBxMultiSubDocs SubDocs { get { return _SubDocs; } }
    private DBxMultiSubDocs _SubDocs;

    /// <summary>
    /// Провайдер для доступа к документам
    /// </summary>
    public DBxDocProvider DocProvider { get { return _SubDocs.DocSet.DocProvider; } }

    /// <summary>
    /// Интерфейс доступа к поддокументам
    /// </summary>
    public SubDocTypeUI SubDocTypeUI { get { return _SubDocTypeUI; } }
    private SubDocTypeUI _SubDocTypeUI;

    /// <summary>
    /// Интферфейс для доступа к документам.
    /// </summary>
    public DBUI UI { get { return _SubDocTypeUI.UI; } }

    /// <summary>
    /// Если установлено в true, то перед добавлением и редактированием записей
    /// вызывается MainEditor.ValidateDate(). В этом случае редактор поддокумента
    /// может использовать актуальные значения полей основного документа
    /// По умолчанию (false) проверка не выполняется. Допускается редактирование
    /// поддокументов, даже если на какой-либо вкладке редактора основного документа
    /// есть некорректно заполненные поля.
    /// </summary>
    public bool ValidateBeforeEdit
    {
      get { return _ValidateBeforeEdit; }
      set
      {
        if (value && _MainEditor == null)
          throw new InvalidOperationException("Нельзя устанавливать свойство ValidateBeforeEdit в true, т.к. просмотр не относится к DocumentEditor"); // 21.01.2022
        _ValidateBeforeEdit = value;
      }
    }
    private bool _ValidateBeforeEdit;

    /// <summary>
    /// Если true (по умолчанию), то перед удалением выбранных поддокументов запрашивается подтверждение.
    /// Если false, то удаление выполняется немедленно без запроса.
    /// </summary>
    public bool ConfirmDeletion { get { return _ConfirmDeletion; } set { _ConfirmDeletion = value; } }
    private bool _ConfirmDeletion;

    #endregion

    #region Выпадающий список

    void Control_PopupClick(object sender, EventArgs args)
    {
      OKCancelGridForm form = new OKCancelGridForm();
      form.Text = this.DisplayName;
      form.Icon = EFPApp.MainImages.Icons[this.SubDocTypeUI.TableImageKey];
      WinFormsTools.OkCancelFormToOkOnly(form);
      EFPSubDocGridView efpGrid;
      if (MainEditor == null)
        efpGrid = new EFPSubDocGridView(form.ControlWithToolBar, SubDocs, SubDocTypeUI.UI);
      else
        efpGrid = new EFPSubDocGridView(form.ControlWithToolBar, MainEditor, SubDocs);

      // Копируем свойства
      efpGrid.ValidateBeforeEdit = this.ValidateBeforeEdit;
      efpGrid.ConfirmDeletion = this.ConfirmDeletion;

      EFPDialogPosition dlgPos = new EFPDialogPosition(Control);
      EFPApp.ShowDialog(form, true, dlgPos);

      InitTextAndImage();
    }

    #endregion

    #region Событие TextValueNeeded

    /// <summary>
    /// Это событие вызывается после редактирования списка поддокументов и позволяет переопределить текст в комбоблоке, текст всплываюующей подсказки
    /// и изображение. Событие вызывается в том числе и при пустом списке поддокументов.
    /// Также вызывается при обращении к свойству TextValue
    /// </summary>
    public event EFPAllSubDocComboBoxTextValueNeededEventHandler TextValueNeeded
    {
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
    private EFPAllSubDocComboBoxTextValueNeededEventHandler _TextValueNeeded;

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
    /// Чтобы не создавать объект каждый раз, создаем его в конструкторе.
    /// Также используем для хранения изображения между вызовом InitText() и
    /// его выводом в комбоблоке
    /// </summary>
    private EFPAllSubDocComboBoxTextValueNeededEventArgs _TextValueNeededArgs;

    /// <summary>
    /// Установка текста элемента
    /// EFPDocComboBox доопределяет метод для установки доступности кнопки Edit
    /// </summary>
    private void InitTextAndImage()
    {
      try
      {
        _TextValueNeededArgs.Clear();
        // Стандартные значения текста, подсказки и изображения
        List<DBxSubDoc> viewedSubDocs = new List<DBxSubDoc>();
        foreach (DBxSubDoc sd in SubDocs)
        {
          if (sd.SubDocState != DBxDocState.Delete)
            viewedSubDocs.Add(sd);
        }
        if (viewedSubDocs.Count == 0)
        {
          _TextValueNeededArgs.TextValue = EmptyText;
          _TextValueNeededArgs.ImageKey = EmptyImageKey;
        }
        else
        {
          if (viewedSubDocs.Count > MaxTextItemCount)
            _TextValueNeededArgs.TextValue = SubDocTypeUI.SubDocType.PluralTitle + " (" + viewedSubDocs.Count.ToString() + ")";
          else
          {
            string[] a = new string[viewedSubDocs.Count];
            for (int i = 0; i < viewedSubDocs.Count; i++)
              a[i] = SubDocTypeUI.UI.TextHandlers.GetTextValue(viewedSubDocs[i]);
            _TextValueNeededArgs.TextValue = String.Join(", ", a);
          }

          if (EFPApp.ShowListImages)
          {
            if (viewedSubDocs.Count == 1)
              _TextValueNeededArgs.ImageKey = UI.ImageHandlers.GetImageKey(viewedSubDocs[0]);
            else
              _TextValueNeededArgs.ImageKey = SubDocTypeUI.TableImageKey;
          }
          else
            _TextValueNeededArgs.ImageKey = String.Empty;

          if (EFPApp.ShowToolTips)
          {
            if (viewedSubDocs.Count == 1)
              _TextValueNeededArgs.ToolTipText = UI.ImageHandlers.GetToolTipText(viewedSubDocs[0]);
            else
              _TextValueNeededArgs.ToolTipText = SubDocTypeUI.SubDocType.PluralTitle + " (" + viewedSubDocs.Count.ToString() + ")";
          }
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
    }

    #endregion

    #region Свойство MaxTextItemCount

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
        _EmptyTextEx = new DepInput<string>(EmptyText, EmptyTextEx_ValueChanged);
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
        _EmptyImageKeyEx = new DepInput<string>(EmptyImageKey, EmptyImageKeyEx_ValueChanged);
        _EmptyImageKeyEx.OwnerInfo = new DepOwnerInfo(this, "EmptyImageKeyEx");
      }
    }

    private DepInput<String> _EmptyImageKeyEx;

    void EmptyImageKeyEx_ValueChanged(object sender, EventArgs args)
    {
      EmptyImageKey = _EmptyImageKeyEx.Value;
    }

    #endregion
  }
}
