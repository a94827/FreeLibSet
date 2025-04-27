// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.DependedValues;
using FreeLibSet.Forms.Diagnostics;
using FreeLibSet.Logging;
using FreeLibSet.UICore;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Класс формы для редактора документов.
  /// Экземпляр формы создается одновременно с <see cref="ExtEditDialog"/>
  /// </summary>
  internal partial class ExtEditDialogForm : Form
  {
    #region Конструктор и Dispose

    internal ExtEditDialogForm(ExtEditDialog owner)
    {
      InitializeComponent();
      _Owner = owner;

      _MaxPageSize = new Size(336, 48);

      _FormProvider = new EFPFormProvider(this);
      //_FormProvider.AddFormCheck(FormValidating);
      _FormProvider.ChangeInfo = _Owner.ChangeInfoList;
      _FormProvider.Shown += new EventHandler(FormProvider_Shown);
      _FormProvider.Hidden += new EventHandler(FormProvider_Hidden);

      #region Просмотр с вкладками

      _TabControlProvider = new EFPTabControl(_FormProvider, MainTabControl);

      #endregion

      #region Кнопки

      btnOK.DialogResult = DialogResult.None;
      _OKButtonProvider = new EFPButton(_FormProvider, btnOK);
      _OKButtonProvider.Click += btnOK_Click;

      btnCancel.DialogResult = DialogResult.None;
      _CancelButtonProvider = new EFPButton(_FormProvider, btnCancel);

      btnApply.ImageList = EFPApp.MainImages.ImageList;
      btnApply.ImageKey = "Apply";
      _ApplyButtonProvider = new EFPButton(_FormProvider, btnApply);
      _ApplyButtonProvider.Click += btnApply_Click;

      btnMore.ImageList = EFPApp.MainImages.ImageList;
      btnMore.ImageKey = "MenuButton";
      _MoreButtonProvider = new EFPButtonWithMenu(_FormProvider, btnMore);

      btnCancel.Click += btnCancel_Click;

      #endregion;
    }

    #endregion

    #region Провайдеры элементов формы

    /// <summary>
    /// Провайдер обработки ошибок для формы в-целом
    /// </summary>
    public EFPFormProvider FormProvider { get { return _FormProvider; } }
    private readonly EFPFormProvider _FormProvider;

    /// <summary>
    /// Провайдер элемента с закладками
    /// </summary>
    public EFPTabControl TabControlProvider { get { return _TabControlProvider; } }
    private readonly EFPTabControl _TabControlProvider;

    /// <summary>
    /// Провайдер кнопки "ОК"
    /// </summary>
    public EFPButton OKButtonProvider { get { return _OKButtonProvider; } }
    private readonly EFPButton _OKButtonProvider;

    /// <summary>
    /// Провайдер кнопки "Отмена"
    /// </summary>
    public EFPButton CancelButtonProvider { get { return _CancelButtonProvider; } }
    private readonly EFPButton _CancelButtonProvider;

    /// <summary>
    /// Провайдер кнопки "Запись"
    /// </summary>
    public EFPButton ApplyButtonProvider { get { return _ApplyButtonProvider; } }
    private readonly EFPButton _ApplyButtonProvider;

    /// <summary>
    /// Провайдер кнопки "Еще"
    /// </summary>
    public EFPButtonWithMenu MoreButtonProvider { get { return _MoreButtonProvider; } }
    private readonly EFPButtonWithMenu _MoreButtonProvider;

    private void btnOK_Click(object sender, EventArgs args)
    {
      if (_Owner.ReadOnly)
        return;

      if (!_Owner.DoWrite(ExtEditDialogState.OKClicked))
        return;

      this.DialogResult = DialogResult.No; // Чтобы не вызывалась еще раз проверка данных
      this.Close();
      this.DialogResult = DialogResult.OK; // для упрощения отладки
    }

    private void btnApply_Click(object sender, EventArgs args)
    {
      _Owner.DoWrite(ExtEditDialogState.ApplyClicked);
    }

    private void btnCancel_Click(object sender, EventArgs args)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }


    #endregion

    #region Другие свойства

    /// <summary>
    /// Владелец формы.
    /// </summary>
    public new ExtEditDialog Owner { get { return _Owner; } }
    private readonly ExtEditDialog _Owner;

    #endregion

    #region Корректировка размера формы

    private Size _MaxPageSize;

    internal void CorrectSize()
    {
      if (MainTabControl.TabCount == 0)
        return;
      //      Size sz=Size;
      //      Size=new Size(sz.Width+MyMaxPageSize.Width-336, 
      //        sz.Height+MyMaxPageSize.Height-48);

      PerformLayout();

      // Размеры заголовка и рамок формы и панели с кнопками
      Size sz2 = new Size();
      sz2.Width = Size.Width - ClientSize.Width;
      sz2.Height = Size.Height - ClientSize.Height + ButtonsPanel.Size.Height;

      // Размеры области закладок
      Size sz3 = new Size();
      //sz3.Width=10;
      //sz3.Height=36;
      sz3.Width = MainTabControl.Width - MainTabControl.DisplayRectangle.Width;
      sz3.Height = MainTabControl.Height - MainTabControl.DisplayRectangle.Height;
      Size = new Size(_MaxPageSize.Width + sz2.Width + sz3.Width,
                    _MaxPageSize.Height + sz2.Height + sz3.Height);
    }

    internal void RegPageSize(Size sz)
    {
      _MaxPageSize.Width = Math.Max(_MaxPageSize.Width, sz.Width);
      _MaxPageSize.Height = Math.Max(_MaxPageSize.Height, sz.Height);
    }

    #endregion

    #region Сохранение выбранной вкладки

    private static Dictionary<string, string> _SelectedTabs = new Dictionary<string, string>();

    void FormProvider_Shown(object sender, EventArgs args)
    {
      string pageTitle;
      if (_SelectedTabs.TryGetValue(FormProvider.ConfigSectionName, out pageTitle))
      {

        if (_Owner.SaveCurrentPage)
        {
          for (int i = 0; i < MainTabControl.TabCount; i++)
          {
            if (String.CompareOrdinal(MainTabControl.TabPages[i].Text, pageTitle) == 0)
            {
              MainTabControl.SelectedIndex = i;
              break;
            }
          }
        }
      }

      _Owner.CallFormShown();
    }

    void FormProvider_Hidden(object sender, EventArgs args)
    {
      if (MainTabControl.SelectedTab != null)
        _SelectedTabs[FormProvider.ConfigSectionName] = MainTabControl.SelectedTab.Text;
    }

    #endregion
  }

  #region Перечисления

  /// <summary>
  /// Текущее состояние диалога <see cref="ExtEditDialog.FormState"/>
  /// </summary>
  public enum ExtEditDialogState
  {
    /// <summary>
    /// Исходное состояние, диалог не был запущен
    /// </summary>
    Initialization,

    /// <summary>
    /// Диалог выведен на экран
    /// </summary>
    Shown,

    /// <summary>
    /// Нажата кнопка "Запись" (выполняется проверка корректности значений полей или выполняется обработчик <see cref="ExtEditDialog.Writing"/>).
    /// </summary>
    ApplyClicked,

    /// <summary>
    /// Нажата кнопка "ОК" (выполняется проверка корректности значений полей или выполняется обработчик <see cref="ExtEditDialog.Writing"/>).
    /// </summary>
    OKClicked,

    /// <summary>
    /// Из прикладного кода вызван метод <see cref="ExtEditDialog.WriteData()"/>.
    /// </summary>
    WriteData,

    /// <summary>
    /// Работа диалога завершена.
    /// </summary>
    Closed,
  }

  #endregion

  /// <summary>
  /// Диалог редактирования значений с использованием интерфейса <see cref="IUIExtEditItem"/>.
  /// При показе форма содержит <see cref="System.Windows.Forms.TabControl"/> и кнопки "ОК", "Отмена", "Запись" и "Еще" (выпадающее меню).
  /// Кнопки "Запись" и "Еще" показываются только при наличии присоединенных обработчиков.
  /// Поддерживается режим просмотра с объединением кнопок "ОК" и "Отмена".
  /// Поддерживаются модальный и немодальный режимы работы.
  /// При нажатии кнопок "ОК" или "Запись" выполняется проверка полей формы, а затем вызывается событие записи.
  /// Прикладной код имеет возможность отменить закрытие формы или запись данных, либо используя проверку данных (на уровне управляющих
  /// элементов или с помощью объекта <see cref="EFPFormCheck"/>, либо в обработчике события <see cref="ExtEditDialog.Writing"/>.
  /// Объект является "одноразовым", повторный показ закрытой формы не допускается.
  /// </summary>
  public class ExtEditDialog
  {
    #region Конструктор

    /// <summary>
    /// Создает пустой диалог
    /// </summary>
    public ExtEditDialog()
    {
      _Form = new ExtEditDialogForm(this);
      _Form.FormProvider.FormClosing += FormProvider_FormClosing;
      _Form.FormProvider.FormClosed += FormProvider_FormClosed;
      _Pages = new PageCollection(this);
      _EditItems = new UIExtEditItemList();
      _ChangeInfoList = new DepChangeInfoList();
      _ChangeInfoList.DisplayName = Res.ExtEditDialogForm_Name_ChangedInfoList;
      _Form.FormProvider.ChangeInfo = _ChangeInfoList;
      _DisposeFormList = new List<System.Windows.Forms.Form>();
      _CheckUnsavedChanges = true;
      _ReadOnly = false;
      _ShowApplyButton = true;
      _FormState = ExtEditDialogState.Initialization;
    }

    #endregion

    #region Основные свойства

    /// <summary>
    /// Заголовок формы.
    /// Свойство может устанавливаться динамически.
    /// </summary>
    public string Title
    {
      get { return _Form.Text; }
      set { _Form.Text = value; }
    }

    /// <summary>
    /// Изображение для значка формы, извлекаемое из коллекции <see cref="EFPApp.MainImages"/>.
    /// По умолчанию - нет значка.
    /// Свойство может устанавливаться динамически.
    /// </summary>
    public string ImageKey
    {
      get { return _ImageKey ?? String.Empty; }
      set
      {
        _Image = null;
        _ImageKey = value;
        if (String.IsNullOrEmpty(value))
          _Form.ShowIcon = false;
        else
        {
          _Form.Icon = EFPApp.MainImages.Icons[value];
          _Form.ShowIcon = true;
        }
      }
    }
    private string _ImageKey;

    /// <summary>
    /// Задание произвольного изображения в качестве значка формы. Альтернатива свойству <see cref="ImageKey"/>.
    /// Свойство может устанавливаться динамически.
    /// </summary>
    public Image Image
    {
      get { return _Image; }
      set
      {
        _ImageKey = null;
        _Image = value;
        if (value == null)
          _Form.ShowIcon = false;
        else
          WinFormsTools.InitIcon(_Form, value);
      }
    }
    private Image _Image;

    /// <summary>
    /// Сюда добавляются объекты, реализующие интерфейс <see cref="IUIExtEditItem"/>.
    /// </summary>
    public UIExtEditItemList EditItems { get { return _EditItems; } }
    private readonly UIExtEditItemList _EditItems;

    /// <summary>
    /// Отслеживаемые изменения. 
    /// Для каждой страницы <see cref="ExtEditPage"/> создается дочерний список изменений.
    /// </summary>
    public DepChangeInfoList ChangeInfoList { get { return _ChangeInfoList; } }
    private readonly DepChangeInfoList _ChangeInfoList;

    /// <summary>
    /// Имя секции конфигурации для хранения положения и размеров формы (<see cref="EFPFormProvider.ConfigSectionName"/>.
    /// Свойство может устанавливаться только до вывода формы на экран.
    /// </summary>
    public string ConfigSectionName
    {
      get { return _Form.FormProvider.ConfigSectionName; }
      set { _Form.FormProvider.ConfigSectionName = value; }
    }

    /// <summary>
    /// Если true (по умолчанию), то будет запоминаться текущая выбранная вкладка между показами диалога.
    /// Вкладка запоминается с привязкой к секции <see cref="ConfigSectionName"/>.
    /// Свойство может устанавливаться только до вывода формы на экран.
    /// </summary>
    public bool SaveCurrentPage
    {
      get { return _SaveCurrentPage; }
      set
      {
        _Form.FormProvider.CheckHasNotBeenShown();
        _SaveCurrentPage = value;
      }
    }
    private bool _SaveCurrentPage;

    /// <summary>
    /// Если True, то в диалоге будет только кнопка "ОК", а не "ОК" и "Отмена".
    /// По умолчанию - false.
    /// Свойство может устанавливаться только до вывода формы на экран.
    /// </summary>
    public bool ReadOnly
    {
      get { return _ReadOnly; }
      set
      {
        _Form.FormProvider.CheckHasNotBeenShown();
        _ReadOnly = value;
      }
    }
    private bool _ReadOnly;

    /// <summary>
    /// Должна ли показываться кнопка "Запись".
    /// По умолчанию - true.
    /// Свойство действует только при <see cref="ReadOnly"/>=false.
    /// Свойство может устанавливаться только до вывода формы на экран.
    /// </summary>
    public bool ShowApplyButton
    {
      get { return _ShowApplyButton; }
      set
      {
        _Form.FormProvider.CheckHasNotBeenShown();
        _ShowApplyButton = value;
      }
    }
    private bool _ShowApplyButton;

    /// <summary>
    /// Команды меню для кнопки "Еще".
    /// Если ни одна команда не добавлена, кнопка не показывается.
    /// </summary>
    public EFPCommandItems MoreCommandItems { get { return _Form.MoreButtonProvider.CommandItems; } }

    /// <summary>
    /// Контекст справки, вызываемой по F1
    /// Свойство может устанавливаться только до вывода формы на экран.
    /// </summary>
    public string HelpContext
    {
      get { return _HelpContext ?? String.Empty; }
      set
      {
        _Form.FormProvider.CheckHasNotBeenShown();
        _HelpContext = value;
      }
    }
    private string _HelpContext;

    /// <summary>
    /// Позиция блока диалога на экране.
    /// По умолчанию блок диалога центрируется относительно <see cref="EFPApp.DefaultScreen"/>.
    /// Свойство может устанавливаться только до вывода формы на экран.
    /// </summary>
    public EFPDialogPosition DialogPosition
    {
      get { return _DialogPosition; }
      set
      {
        _Form.FormProvider.CheckHasNotBeenShown();
        if (value == null)
          _DialogPosition = new EFPDialogPosition();
        else
          _DialogPosition = value;
      }
    }
    private EFPDialogPosition _DialogPosition;

    /// <summary>
    /// Форма в процессе работы диалога
    /// </summary>
    public Form Form { get { return _Form; } }
    private readonly ExtEditDialogForm _Form;

    internal ExtEditDialogForm FormInternal { get { return _Form; } }

    /// <summary>
    /// Провайдер формы в процессе работы
    /// </summary>
    internal EFPFormProvider FormProvider
    {
      get
      {
        //if (_Form == null)
        //  return null;
        //else
        return _Form.FormProvider;
      }
    }


    /// <summary>
    /// Объекты синхронизации для редактора
    /// </summary>
    public DepSyncCollection Syncs { get { return _Form.FormProvider.Syncs; } }

    /// <summary>
    /// Провайдер синхронизации значений. К нему будет подключен объект Syncs
    /// на время запуска формы.
    /// Значение по-умолчанию можно переопределить для использования чужого провайдера
    /// </summary>
    public DepSyncProvider SyncProvider
    {
      get { return _Form.FormProvider.SyncProvider; }
      set
      {
        _Form.FormProvider.CheckHasNotBeenShown();
        _Form.FormProvider.SyncProvider = value;
      }
    }

    /// <summary>
    /// Если true, то кнопка "Отмена" переименовывается в "Закрыть".
    /// По умолчанию - false.
    /// Свойство может устанавливаться динамически.
    /// </summary>
    public bool CancelButtonAsClose
    {
      get { return _CancelButtonAsClose; }
      set
      {
        if (value == _CancelButtonAsClose)
          return;
        _CancelButtonAsClose = value;
        _Form.CancelButtonProvider.Text = value ? Res.Btn_Text_Close : Res.Btn_Text_Cancel;
      }
    }
    private bool _CancelButtonAsClose;

    /// <summary>
    /// Если true (по умолчанию), то при закрытии формы крестиком или кнопкой "Отмена" проверяется наличие изменений.
    /// Запрашивается подтверждение закрытия формы без сохранения изменений.
    /// Если свойство сброшено в false, то форма закрывается сразу.
    /// Свойство может устанавливаться динамически.
    /// </summary>
    public bool CheckUnsavedChanges
    {
      get { return _CheckUnsavedChanges; }
      set { _CheckUnsavedChanges = value; }
    }
    private bool _CheckUnsavedChanges;

    /// <summary>
    /// Реализация свойства <see cref="FormChecks"/>
    /// </summary>
    public struct FormCheckCollection : ICollection<EFPFormCheck>
    {
      #region Конструктор

      internal FormCheckCollection(ExtEditDialog owner)
      {
        _Owner = owner;
      }

      #endregion

      #region Свойства

      private readonly ExtEditDialog _Owner;

      /// <summary>
      /// Количество объектов проверки
      /// </summary>
      public int Count { get { return _Owner._Form.FormProvider.FormChecks.Count; } }

      bool ICollection<EFPFormCheck>.IsReadOnly { get { return _Owner._Form.FormProvider.HasBeenShown; } }

      /// <summary>
      /// Добавляет объект проверки формы
      /// </summary>
      /// <param name="item">Объект проверки, еще не присоединенный к форме</param>
      public void Add(EFPFormCheck item)
      {
        _Owner.FormProvider.CheckHasNotBeenShown();
        _Owner.FormProvider.FormChecks.Add(item);
      }


      /// <summary>
      /// Упрощенный способ добавления проверки ошибки в форме
      /// </summary>
      /// <param name="validating">Обработчик, выполняющий проверку</param>
      /// <param name="focusControl">Управляющий элемент, который получит фокус ввода в случае ошибки</param>
      public void Add(UIValidatingEventHandler validating, Control focusControl)
      {
        _Owner.FormProvider.CheckHasNotBeenShown();
        _Owner.FormProvider.FormChecks.Add(validating, focusControl);
      }

      /// <summary>
      /// Упрощенный способ добавления проверки ошибки в форме
      /// </summary>
      /// <param name="validating">Обработчик, выполняющий проверку</param>
      public void Add(UIValidatingEventHandler validating)
      {
        Add(validating, (Control)null);
      }

      /// <summary>
      /// Очищает список объектов проверки формы
      /// </summary>
      public void Clear()
      {
        _Owner.FormProvider.CheckHasNotBeenShown();
        _Owner.FormProvider.FormChecks.Clear();
      }

      /// <summary>
      /// Возращает true, если в списке есть указанный объект проверки
      /// </summary>
      /// <param name="item">Объект проверки</param>
      /// <returns>Наличие в списке</returns>
      public bool Contains(EFPFormCheck item)
      {
        return _Owner.FormProvider.FormChecks.Contains(item);
      }

      /// <summary>
      /// Копирование всех объектов проверки в массив
      /// </summary>
      /// <param name="array">Заполняемый массив</param>
      /// <param name="arrayIndex">Позиция, с которой заполняется массив</param>
      public void CopyTo(EFPFormCheck[] array, int arrayIndex)
      {
        _Owner._Form.FormProvider.FormChecks.CopyTo(array, arrayIndex);
      }

      /// <summary>
      /// Возвращает перечислитель объектов проверки
      /// </summary>
      /// <returns>Новый перечислитель</returns>
      public IEnumerator<EFPFormCheck> GetEnumerator()
      {
        return _Owner._Form.FormProvider.FormChecks.GetEnumerator();
      }

      /// <summary>
      /// Удалить объект проверки из списка
      /// </summary>
      /// <param name="item">Удаляемый объект</param>
      /// <returns>true, если объект был в списке и успешно удален</returns>
      public bool Remove(EFPFormCheck item)
      {
        _Owner.FormProvider.CheckHasNotBeenShown();
        return _Owner.FormProvider.FormChecks.Remove(item);
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return _Owner.FormProvider.FormChecks.GetEnumerator();
      }

      #endregion
    }

    /// <summary>
    /// Объекты проверки формы
    /// </summary>
    public FormCheckCollection FormChecks { get { return new FormCheckCollection(this); } }

    /// <summary>
    /// Произвольные пользовательские данные
    /// </summary>
    public object Tag { get { return _Tag; } set { _Tag = value; } }
    private object _Tag;

    #endregion

    #region События

    /// <summary>
    /// Событие вызывается при нажатии кнопки "ОК" (<see cref="FormState"/>=<see cref="ExtEditDialogState.OKClicked"/>) или "Запись" (<see cref="ExtEditDialogState.ApplyClicked"/>).
    /// Обработчик должен сохранить введенные данные.
    /// Если обработчик события установит <see cref="CancelEventArgs.Cancel"/>=true, то форма не будет закрыта.
    /// Событие не вызывается при <see cref="ReadOnly"/>=true.
    /// На момент вызова уже успешно выполнена проверка корректности введенных значений.
    /// </summary>
    public event CancelEventHandler Writing;

    /// <summary>
    /// Вызывается после закрытия формы
    /// </summary>
    public event EventHandler FormClosed;

    #endregion

    #region Страницы диалога

    /// <summary>
    /// Коллекция объектов <see cref="ExtEditPage"/> (свойство DocumentEditor.TheTabControl)
    /// </summary>
    public sealed class PageCollection : List<ExtEditPage>
    {
      #region Защищенный конструктор

      internal PageCollection(ExtEditDialog owner)
      {
        _Owner = owner;
      }

      #endregion

      #region Свойства

      private readonly ExtEditDialog _Owner;

      #endregion

      #region Методы

      /// <summary>
      /// Добавляет страницу к диалогу
      /// </summary>
      /// <param name="control">Управляюший элемент (контейнер), добавляемый к форме в качестве вкладки</param>
      /// <returns>Интерфейс управления</returns>
      public ExtEditPage Add(Control control)
      {
#if DEBUG
        if (control == null)
          throw new ArgumentNullException("control");
#endif
        _Owner.FormProvider.CheckHasNotBeenShown();
        _Owner.FormInternal.RegPageSize(control.Size);
        EFPTabPage tabPage = new EFPTabPage();
        _Owner._Form.TabControlProvider.TabPages.Add(tabPage);
        tabPage.Control.Controls.Add(control);
        ExtEditPage page = new ExtEditPage(_Owner, tabPage);
        base.Add(page);
        return page;
      }

      #endregion
    }

    /// <summary>
    /// Список страниц диалога
    /// </summary>
    public PageCollection Pages { get { return _Pages; } }
    private readonly PageCollection _Pages;


    #region События

    /// <summary>
    /// Событие вызывается однократно при показе формы
    /// </summary>
    public event EventHandler FormShown;

    internal void CallFormShown()
    {
      if (FormShown != null)
        FormShown(this, EventArgs.Empty);
    }


    /// <summary>
    /// Событие вызывается при активации страницы редактора.
    /// Событие вызывается после событий <see cref="ExtEditPage.FirstShow"/> и <see cref="ExtEditPage.PageShow"/>, 
    /// поэтому страница уже создана на момент вызова.
    /// </summary>
    public event ExtEditPageEventHandler PageShow;

    internal void OnPageShow(ExtEditPage page)
    {
      if (PageShow == null)
        return;
      ExtEditPageEventArgs args = new ExtEditPageEventArgs(page);
      try
      {
        PageShow(this, args);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e);
      }
    }

    #endregion

    /// <summary>
    /// Текущая страница в окне
    /// </summary>
    public ExtEditPage SelectedPage
    {
      get
      {
        if (_Form.TabControlProvider.SelectedTab == null)
          return null;
        else
          return _Form.TabControlProvider.SelectedTab.Tag as ExtEditPage;
      }
      set
      {
        if (value == null)
          return;
        _Form.TabControlProvider.SelectedTab = value.TabPage;
      }
    }

    #endregion

    #region Список синхронно удаляемых форм

    /// <summary>
    /// Список форм, подлежащих разрушению вместе с данной формой
    /// </summary>
    public List<Form> DisposeFormList { get { return _DisposeFormList; } }
    private readonly List<Form> _DisposeFormList;

    #endregion

    #region Всплывающие подсказки

    /// <summary>
    /// Всплывающая подсказка для кнопки "ОК" (свойство <see cref="EFPControlBase.ToolTipText"/>)
    /// </summary>
    public string OKButtonToolTipText { get { return _Form.OKButtonProvider.ToolTipText; } set { _Form.OKButtonProvider.ToolTipText = value; } }

    /// <summary>
    /// Всплывающая подсказка для кнопки "Отмена" (свойство <see cref="EFPControlBase.ToolTipText"/>)
    /// </summary>
    public string CancelButtonToolTipText { get { return _Form.CancelButtonProvider.ToolTipText; } set { _Form.CancelButtonProvider.ToolTipText = value; } }

    /// <summary>
    /// Всплывающая подсказка для кнопки "Запись" (свойство <see cref="EFPControlBase.ToolTipText"/>)
    /// </summary>
    public string ApplyButtonToolTipText { get { return _Form.ApplyButtonProvider.ToolTipText; } set { _Form.ApplyButtonProvider.ToolTipText = value; } }

    /// <summary>
    /// Всплывающая подсказка для кнопки "Еще" (свойство <see cref="EFPControlBase.ToolTipText"/>)
    /// </summary>
    public string MoreButtonToolTipText { get { return _Form.MoreButtonProvider.ToolTipText; } set { _Form.MoreButtonProvider.ToolTipText = value; } }

    #endregion

    #region Отладка

    /// <summary>
    /// Добавляет к кнопке "Еще" отладочные команды
    /// </summary>
    public void AddDebugCommandItems()
    {
      _Form.FormProvider.CheckHasNotBeenShown();

      EFPCommandItem ciDebugChanges = new EFPCommandItem("Debug", "Changes");
      ciDebugChanges.MenuText = Res.Cmd_Menu_Debug_Changes;
      ciDebugChanges.Click += new EventHandler(ciDebugChanges_Click);
      ciDebugChanges.GroupBegin = true;
      MoreCommandItems.Add(ciDebugChanges);

      EFPCommandItem ciDebugCheckItems = new EFPCommandItem("Debug", "Form");
      ciDebugCheckItems.MenuText = Res.Cmd_Menu_Debug_Form;
      ciDebugCheckItems.Click += new EventHandler(ciDebugCheckItems_Click);
      ciDebugCheckItems.GroupEnd = true;
      MoreCommandItems.Add(ciDebugCheckItems);
    }

    private void ciDebugChanges_Click(object sender, EventArgs args)
    {
      EFPCommandItem ci = (EFPCommandItem)sender;
      DebugTools.DebugChangeInfo(ChangeInfoList, ci.MenuTextWithoutMnemonic);
    }

    private void ciDebugCheckItems_Click(object sender, EventArgs args)
    {
      EFPCommandItem ci = (EFPCommandItem)sender;
      DebugTools.DebugBaseProvider(_Form.FormProvider, ci.MenuTextWithoutMnemonic);
    }

    #endregion

    #region Показ диалога / формы

    /// <summary>
    /// Показ формы в модальном режиме.
    /// Метод <see cref="ShowDialog()"/> или <see cref="Show()"/> может быть вызван только один раз.
    /// </summary>
    /// <returns>Результат выполнения диалога</returns>
    public DialogResult ShowDialog()
    {
      PrepareForm();
      return EFPApp.ShowDialog(_Form, true, DialogPosition);
    }

    /// <summary>
    /// Показ немодальной формы.
    /// Метод <see cref="ShowDialog()"/> или <see cref="Show()"/> может быть вызван только один раз.
    /// </summary>
    public void Show()
    {
      PrepareForm();
      EFPApp.ShowFormOrDialog(_Form);
    }

    private void PrepareForm()
    {
      _Form.FormProvider.CheckHasNotBeenShown();

      if (ReadOnly)
      {
        _Form.CancelButtonProvider.Visible = false;
        _Form.CancelButton = _Form.OKButtonProvider.Control;
        _Form.ApplyButtonProvider.Visible = false;
      }
      else
      {
        if ((!ShowApplyButton) || ReadOnly)
          _Form.ApplyButtonProvider.Visible = false;
      }
      _Form.MoreButtonProvider.Visible = (_Form.MoreButtonProvider.CommandItems.Count > 0);
      _Form.CorrectSize();
      _FormState = ExtEditDialogState.Shown;
    }

    /// <summary>
    /// Текущее состояние формы
    /// </summary>
    public ExtEditDialogState FormState { get { return _FormState; } }
    private ExtEditDialogState _FormState;

    /// <summary>
    /// Выполняет внеплановую запись данных. 
    /// При записи свойство <see cref="FormState"/> устанавливается равным <see cref="ExtEditDialogState.WriteData"/>
    /// </summary>
    /// <returns>Возвращает true, если форма содержит корректные данные и обработчик события <see cref="Writing"/> не установил
    /// <see cref="CancelEventArgs.Cancel"/>=true.</returns>
    public bool WriteData()
    {
      if (FormState != ExtEditDialogState.Shown)
        throw new InvalidOperationException(Res.ExtEditDialogForm_Err_WriteDataCall);
      return DoWrite(ExtEditDialogState.WriteData);
    }


    /// <summary>
    /// Закрывает открытую форму эмуляцией нажатия кнопки "ОК" или "Отмена".
    /// Если форма уже закрыта, возвращает true.
    /// Если форма не была открыта или в данный момент обрабатывается нажатие кнопок "ОК" или "Запись", выбрасывается исключение.
    /// </summary>
    /// <param name="isOk">true - нажатие кнопки "ОК", false - "Отмена"</param>
    /// <returns>true, если форма успешно закрыта. Возвращает false, если <paramref name="isOk"/>=true но проверка корректности введенных данных или
    /// обработчик <see cref="Writing"/> отменили закрытие формы.</returns>
    public bool CloseForm(bool isOk)
    {
      switch (FormState)
      {
        case ExtEditDialogState.Closed:
          return true;
        case ExtEditDialogState.Shown:
          if (isOk) // 20.04.2025
            _Form.OKButtonProvider.Control.PerformClick();
          else
            _Form.FormProvider.CloseForm(DialogResult.Cancel); // 20.04.2025
          //return FormState == ExtEditDialogState.Closed;
          return !_Form.Visible; // 21.04.2025
        default:
          throw new InvalidOperationException(String.Format(Res.ExtEditDialogForm_Err_InvalidState, FormState.ToString()));
      }
    }

    #endregion

    #region Обработчики формы

    private void FormProvider_FormClosing(object sender, FormClosingEventArgs args)
    {
      if (_Form.DialogResult != DialogResult.No && CheckUnsavedChanges && (!ReadOnly) && ChangeInfoList.Changed)
      {
        EFPApp.Activate(_Form); // 07.06.2021
        if (EFPApp.MessageBox(String.Format(Res.ExtEditDialogForm_Msg_CancelWarning, Title),
          Res.ExtEditDialogForm_Title_CancelWarning, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
        {
          args.Cancel = true;
          return;
        }
      }
    }

    internal bool DoWrite(ExtEditDialogState state)
    {
      if (FormState != ExtEditDialogState.Shown)
      {
        EFPApp.ShowTempMessage(Res.ExtEditDialogForm_Err_WritingInProgress);
        return false;
      }
      _FormState = state;
      bool res;
      try
      {
        res = _Form.FormProvider.ValidateForm();
        if (res)
        {
          if (Writing != null)
          {
            CancelEventArgs args = new CancelEventArgs();
            Writing(this, args);
            res = !args.Cancel;
          }
        }
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, Res.ExtEditDialogForm_ErrTitle_Writing);
        res = false;
      }
      _FormState = ExtEditDialogState.Shown;
      return res;
    }

    private void FormProvider_FormClosed(object sender, FormClosedEventArgs args)
    {
      if (FormClosed != null)
        FormClosed(this, args);
      _FormState = ExtEditDialogState.Closed;
    }


    /// <summary>
    /// Выполнить проверку формы
    /// </summary>
    /// <returns>True, если проверка успешно пройдена.
    /// False, если</returns>
    public bool ValidateForm()
    {
      bool res;
      // 12.08.2007
      // На время проверки надо устанавливать признак OK даже если нажимается
      // кнопка "Запись", иначе проверка не будет выполнена
      DialogResult oldDR = _Form.DialogResult;
      try
      {
        _Form.DialogResult = DialogResult.OK;
        res = _Form.FormProvider.ValidateForm();
      }
      finally
      {
        _Form.DialogResult = oldDR;
      }
      return res;
    }

    #endregion

    #region Список открытых форм

    /// <summary>
    /// Возвращает список открытых форм, как модальных, так и не модальных
    /// </summary>
    /// <returns>Массив объектов с открытыми формами</returns>
    public static ExtEditDialog[] GetOpenForms()
    {
      List<ExtEditDialog> lst = new List<ExtEditDialog>();
      IEnumerable forms = EFPApp.GetDialogStack();
      foreach (Form frm in forms)
      {
        ExtEditDialogForm frm2 = frm as ExtEditDialogForm;
        if (frm2 != null)
          lst.Add(frm2.Owner);
      }


      if (EFPApp.Interface != null)
        forms = EFPApp.Interface.GetChildForms(false);
      else
        forms = Application.OpenForms;

      foreach (Form frm in forms)
      {
        ExtEditDialogForm frm2 = frm as ExtEditDialogForm;
        if (frm2 != null && (!frm.Modal) /* диалоги уже перечислены */)
          lst.Add(frm2.Owner);
      }

      return lst.ToArray();
    }

    #endregion
  }

  #region Делегаты, связанные со страницей документа

  /// <summary>
  /// Аргументы событий, связанных со страницей редактора.
  /// Объекты создаются редактором документа.
  /// </summary>
  public sealed class ExtEditPageEventArgs : EventArgs
  {
    internal ExtEditPageEventArgs(ExtEditPage page)
    {
      _Page = page;
    }

    /// <summary>
    /// Ссылка на объект страницы редактируемого документа
    /// </summary>
    public ExtEditPage Page { get { return _Page; } }
    private readonly ExtEditPage _Page;
  }

  /// <summary>
  /// Делегат событий, связанных со страницей редактора
  /// </summary>
  /// <param name="sender">Источник события</param>
  /// <param name="args">Аргументы события</param>
  public delegate void ExtEditPageEventHandler(object sender, ExtEditPageEventArgs args);

  #endregion

  /// <summary>
  /// Одна страница формы <see cref="ExtEditDialog"/>
  /// </summary>
  public class ExtEditPage : IEFPTabPageControl
  {
    #region Защищенный конструктор

    internal ExtEditPage(ExtEditDialog owner, EFPTabPage tabPage)
    {
      _Owner = owner;
      _HasBeenShown = false;
      _TabPage = tabPage;
      tabPage.Tag = this;
      tabPage.PageSelected += new EventHandler(TabPage_PageSelected);
      tabPage.Control.Resize += new EventHandler(TabPageResize);

      _ChangeInfoList = new DepChangeInfoList();
      owner.ChangeInfoList.Add(_ChangeInfoList);
    }

    #endregion

    #region Общие свойства

    /// <summary>
    /// Была ли страница предъявлена пользователю, то есть, был ли вызов
    /// события <see cref="FirstShow"/>.
    /// </summary>
    public bool HasBeenShown { get { return _HasBeenShown; } }
    private bool _HasBeenShown;

    /// <summary>
    /// Закладка в форме
    /// </summary>
    internal EFPTabPage TabPage { get { return _TabPage; } }
    private readonly EFPTabPage _TabPage;

    /// <summary>
    /// Основной управляющий элемент
    /// </summary>
    public Control MainControl
    {
      get
      {
        if (_TabPage.Control.HasChildren)
          return _TabPage.Control.Controls[0];
        else
          return null;
      }
    }

    /// <summary>
    /// Заголовок вкладки окна редактора. Допускается динамическая установка текста.
    /// </summary>
    public string Title
    {
      get { return _TabPage.Text; }
      set
      {
        _TabPage.Text = value;
        _ChangeInfoList.DisplayName = String.Format(Res.ExtEditPage_Name_ChangedInfoList, value);
      }
    }

    /// <summary>
    /// Значок вкладки окна редактора. Допускается динамическая установка значка.
    /// Задается имя изображения из <see cref="EFPApp.MainImages"/>.
    /// </summary>
    public string ImageKey
    {
      get { return _TabPage.Control.ImageKey; }
      set { _TabPage.Control.ImageKey = value; }
    }

    /// <summary>
    /// Всплывающая подсказка вкладки окна редактора. Допускается динамическая установка текста.
    /// </summary>
    public string ToolTipText
    {
      get { return _TabPage.ToolTipText; }
      set { _TabPage.ToolTipText = value; }
    }

    /// <summary>
    /// Базовый провайдер, необходимый при инициализации провайдеров управляющих элементов на странице.
    /// </summary>
    public EFPBaseProvider BaseProvider { get { return _TabPage.BaseProvider; } }

    /// <summary>
    /// Отслеживаемые изменения. 
    /// Для каждой страницы создается дочерний список изменений.
    /// </summary>
    public DepChangeInfoList ChangeInfoList { get { return _ChangeInfoList; } }
    private readonly DepChangeInfoList _ChangeInfoList;

    #endregion

    #region События

    /// <summary>
    /// Вызывается перед первым предъявлением закладки окна редактирования
    /// пользователю. Используется для загрузки дополнительных данных документа. 
    /// Вызывается до события <see cref="PageShow"/>.
    /// </summary>
    public event ExtEditPageEventHandler FirstShow;

    /// <summary>
    /// Вызывается перед каждым выводом закладки пользователю. Используется для
    /// обновления синхронизированных между закладками данных.
    /// </summary>
    public event ExtEditPageEventHandler PageShow;

    #endregion

    #region Методы

    /// <summary>
    /// Сделать эту страницу в редакторе текущей.
    /// </summary>
    public void SelectPage()
    {
      _TabPage.SetFocus();
    }

    /// <summary>
    /// Возвращает true, если эта вкладка редактора является активной.
    /// </summary>
    public bool IsSelectedPage
    {
      get
      {
        return _TabPage.Parent.SelectedTab == _TabPage;
      }
    }

    /// <summary>
    /// Вывод посередине панели сообщения (например, что групповое
    /// редактирование для этой страницы невозможно).
    /// </summary>
    /// <param name="message">Текст сообщения</param>
    /// <param name="panel">Пустая панель для размещения сообщения</param>
    public void SetPanelMessage(string message, Panel panel)
    {
      Label lbl = new Label();
      lbl.Dock = DockStyle.Fill;
      lbl.TextAlign = ContentAlignment.MiddleCenter;
      lbl.Text = message;
      panel.Controls.Add(lbl);
    }

    /// <summary>
    /// Возвращает <see cref="Title"/>
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return Title;
    }

    #endregion

    #region Внутренняя реализация

    private ExtEditDialog _Owner;

    /// <summary>
    /// Вызывается при активации закладки
    /// </summary>
    private void TabPage_PageSelected(object sender, EventArgs args)
    {
      if (!HasBeenShown)
      {
        if (FirstShow != null)
        {
          ExtEditPageEventArgs ea = new ExtEditPageEventArgs(this);
          try
          {
            FirstShow(this, ea);
            /*
            // 02.09.2010  Мерзкая затычка как в EFPFormProvider'е, чтобы выполнялось размещение
            // элементов на добавленной странице, когда задан масштаб изображения не 1 (не 96 dpi)
            if (FOwner.EditorForm.WindowState == FormWindowState.Normal)
            {
              FOwner.EditorForm.Width--;
              FOwner.EditorForm.Width++;
              //FOwner.Editor.Form.PerformLayout(FPage, "Bounds");
            }
             * */
          }
          catch (Exception e)
          {
            EFPApp.ShowException(e, LogoutTools.GetTitleForCall("ExtEditDialogPage.FirstShow"));
          }
        }
        _HasBeenShown = true;

        if (MainControl != null)
          MainControl.Visible = true; // 31.07.2012
      }
      if (PageShow != null)
      {
        ExtEditPageEventArgs ea = new ExtEditPageEventArgs(this);
        try
        {
          PageShow(this, ea);
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e, LogoutTools.GetTitleForCall("ExtEditDialogPage.PageShow"));
        }
      }
      // Событие у объекта-владельца
      _Owner.OnPageShow(this);
    }

    private void TabPageResize(object sender, EventArgs args)
    {
      if (_TabPage.Control.Controls.Count > 0) // 23.04.2018. Вообще-то так быть не должно
      {
        Control ctrl = _TabPage.Control.Controls[0];
        ctrl.Location = new Point(0, 0);
        ctrl.Size = new Size(_TabPage.Control.ClientSize.Width, _TabPage.Control.ClientSize.Height);
      }
    }

    #endregion

    #region IEFPTabPageControl Members

    string IEFPTabPageControl.Text
    {
      get { return Title; }
      set { Title = value; }
    }

    #endregion
  }


}
