using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Controls;
using FreeLibSet.Data.Docs;

namespace FreeLibSet.Forms.Docs
{
  /// <summary>
  /// Провайдер комбоблока, который предназначен для показа таблицы EFPSubDocGridView в редакторе документа.
  /// Используется, в основном, для поддокументов, предназначенных для создания отношения "многие-ко-многим".
  /// Комбоблок не используется для выбора чего-либо, а только для замены постоянно присутствующей в редакторе таблички поддокументов.
  /// </summary>
  public class EFPAllSubDocComboBox:EFPUserSelComboBox
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
    /// Имя столбца, предназначенного для ручной сортировки строк или null, если
    /// строки сортируются каким-то другим способом
    /// </summary>
    public string ManualOrderColumn
    {
      get { return _ManualOrderColumn; }
      set { _ManualOrderColumn = value; }
    }
    private string _ManualOrderColumn;

    /// <summary>
    /// Интерфейс доступа к поддокументам
    /// </summary>
    public SubDocTypeUI SubDocTypeUI { get { return _SubDocTypeUI; } }
    private SubDocTypeUI _SubDocTypeUI;


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
      form.Icon = EFPApp.MainImageIcon(this.SubDocTypeUI.TableImageKey);
      WinFormsTools.OkCancelFormToOkOnly(form);
      EFPSubDocGridView efpGrid;
      if (MainEditor == null)
        efpGrid = new EFPSubDocGridView(form.ControlWithToolBar, SubDocs, SubDocTypeUI.UI);
      else
        efpGrid = new EFPSubDocGridView(form.ControlWithToolBar, MainEditor, SubDocs);

      // Копируем свойства
      efpGrid.ManualOrderColumn = this.ManualOrderColumn;
      efpGrid.ValidateBeforeEdit = this.ValidateBeforeEdit;
      efpGrid.ConfirmDeletion = this.ConfirmDeletion;

      EFPDialogPosition dlgPos=new EFPDialogPosition(Control);
      EFPApp.ShowDialog(form, true, dlgPos);
    }

    #endregion
  }
}
