// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using FreeLibSet.Core;
using FreeLibSet.Controls;
using System.ComponentModel;

// Реализации WizardStep стандартных видов

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Расширение класса <see cref="WizardStep"/>, содержащее элемент управления <see cref="GroupBox"/> и два
  /// текстовых блока (сверху и снизу). Текстовые блоки автоматически подбирают
  /// свои размеры. <see cref="GroupBox"/> занимает всю площадь панели, кроме текстовых блоков.
  /// 
  /// Этот класс считается устаревшим. Используйте <see cref="ExtWizardStep"/>
  /// </summary>
  public class WizardStepWithGroupBox : WizardStep
  {
    #region Конструктор

    /// <summary>
    /// Создает шаг мастера
    /// </summary>
    public WizardStepWithGroupBox()
    {
      _TheGroupBox = new GroupBox();
      _TheGroupBox.Dock = DockStyle.Fill;
      base.Control.Controls.Add(_TheGroupBox);

      _TopLabel = new Label();
      _TopLabel.AutoSize = true;
      _TopLabel.Dock = DockStyle.Top;
      _TopLabel.UseMnemonic = false;
      _TopLabel.Padding = new Padding(3);
      _TopLabel.Visible = false;
      base.Control.Controls.Add(_TopLabel);

      _BottomLabel = new Label();
      _BottomLabel.AutoSize = true;
      _BottomLabel.Dock = DockStyle.Bottom;
      _BottomLabel.UseMnemonic = false;
      _BottomLabel.Padding = new Padding(3);
      _BottomLabel.Visible = false;
      base.Control.Controls.Add(_BottomLabel);

      _TopLabel.TextChanged += new EventHandler(TopLabel_TextChanged);
      _BottomLabel.TextChanged += new EventHandler(BottomLabel_TextChanged);
    }

    void TopLabel_TextChanged(object sender, EventArgs args)
    {
      _TopLabel.Visible = !String.IsNullOrEmpty(_TopLabel.Text);
    }

    void BottomLabel_TextChanged(object sender, EventArgs args)
    {
      _BottomLabel.Visible = !String.IsNullOrEmpty(_BottomLabel.Text);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Управляющий элемент рамки. Используйте свойство Text для задания заголовка
    /// </summary>
    public GroupBox TheGroupBox { get { return _TheGroupBox; } }
    private GroupBox _TheGroupBox;

    /// <summary>
    /// Верхняя панель.
    /// Используйте свойство Text, при этом видимость панели будет установлена автоматически.
    /// </summary>
    public Label TopLabel { get { return _TopLabel; } }
    private Label _TopLabel;

    /// <summary>
    /// Нижняя панель.
    /// Используйте свойство Text, при этом видимость панели будет установлена автоматически.
    /// </summary>
    public Label BottomLabel { get { return _BottomLabel; } }
    private Label _BottomLabel;

    #endregion
  }

  /// <summary>
  /// Расширение класса <see cref="WizardStep"/>. Основную часть занимает панель <see cref="ExtWizardStep.MainPanel"/>, в которую следует добавлять элементы.
  /// Если установить свойство <see cref="ExtWizardStep.GroupTitle"/>, то будет добавлен контейнер <see cref="GroupBox"/>, а панель перемещена внутрь элемента.
  /// В нижней части может находиться информационная панель <see cref="InfoLabel"/>, в которой выводятся пояснения.
  /// Видимость информационной панели определяется наличием текста в ней.
  /// </summary>
  public class ExtWizardStep : WizardStep
  {
    #region Конструктор

    /// <summary>
    /// Создает объект с пустым контейнером <see cref="MainPanel"/>
    /// </summary>
    public ExtWizardStep()
    {
      _MainPanel = new Panel();
      _MainPanel.Dock = DockStyle.Fill;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Основной контейнер для добавления управляющих элементов
    /// </summary>
    public Panel MainPanel { get { return _MainPanel; } }
    private readonly Panel _MainPanel;

    private GroupBox _GroupBox;

    private InfoLabel _InfoLabel;

    /// <summary>
    /// Заголовок контейнера <see cref="GroupBox"/>.
    /// По умолчанию - пустая строка, рамка не используется. Содержимое <see cref="MainPanel"/> выводится непосредственно в панели шага мастера.
    /// Установка свойства в непустое значение создает элемент <see cref="GroupBox"/>.
    /// </summary>
    public string GroupTitle
    {
      get
      {
        if (_GroupBox == null)
          return String.Empty;
        else
          return _GroupBox.Text;
      }
      set
      {
        if (_GroupBox == null)
        {
          _GroupBox = new GroupBox();
          _GroupBox.Dock = DockStyle.Fill;
        }
        _GroupBox.Text = value;
        _LayoutComplete = false;
      }
    }

    /// <summary>
    /// Текст информационного сообщения в нижней части кадра мастера.
    /// Если свойство установлено, будет показано добавлен элемент <see cref="InfoLabel"/>
    /// </summary>
    public string InfoText
    {
      get
      {
        if (_InfoLabel == null)
          return String.Empty;
        else
          return _InfoLabel.Text;
      }
      set
      {
        if (_InfoLabel == null)
        {
          _InfoLabel = new InfoLabel();
          _InfoLabel.AutoSize = true;
          _InfoLabel.Dock = DockStyle.Bottom;
        }
        _InfoLabel.Text = value;
        _LayoutComplete = false;
      }
    }

    /// <summary>
    /// Значок информационного сообщения. По умолчанию <see cref="MessageBoxIcon.None"/>.
    /// Свойство действует, только если задан текст сообщения <see cref="InfoText"/>.
    /// </summary>
    public MessageBoxIcon InfoIcon
    {
      get
      {
        if (_InfoLabel == null)
          return MessageBoxIcon.None;
        else
          return _InfoLabel.Icon;
      }
      set
      {
        if (_InfoLabel == null)
        {
          _InfoLabel = new InfoLabel();
          _InfoLabel.AutoSize = true;
          _InfoLabel.Dock = DockStyle.Bottom;
        }
        _InfoLabel.Icon = value;
        _LayoutComplete = false;
      }
    }

    /// <summary>
    /// Размер значка информационного сообщения. По умолчанию <see cref="MessageBoxIconSize.Small"/>
    /// Свойство действует, только если задан текст сообщения <see cref="InfoText"/> и задано свойство <see cref="InfoIcon"/>.
    /// </summary>
    public MessageBoxIconSize InfoIconSize
    {
      get
      {
        if (_InfoLabel == null)
          return MessageBoxIconSize.Small;
        else
          return _InfoLabel.IconSize;
      }
      set
      {
        if (_InfoLabel == null)
        {
          _InfoLabel = new InfoLabel();
          _InfoLabel.AutoSize = true;
          _InfoLabel.Dock = DockStyle.Bottom;
        }
        _InfoLabel.IconSize = value;
        _LayoutComplete = false;
      }
    }

    #endregion

    #region Заглушка

    /// <summary>
    /// Не должно использоваться
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new Control Control { get { return base.Control; } }

    #endregion

    #region Переопределенные методы

    private bool _LayoutComplete;

    /// <summary>
    /// Выполняет размещение элементов
    /// </summary>
    /// <param name="action">Причина открытия шага</param>
    protected internal override void OnBeginStep(WizardAction action)
    {
      base.OnBeginStep(action);

      if (!_LayoutComplete)
      {
        base.Control.Controls.Clear();

        // Сначала основной элемент
        if (_GroupBox != null && (!String.IsNullOrEmpty(_GroupBox.Text)))
        {
          base.Control.Controls.Add(_GroupBox);
          _MainPanel.Parent = _GroupBox;
        }
        else
        {
          base.Control.Controls.Add(_MainPanel);
        }

        // Затем - метка
        if (_InfoLabel != null && (!String.IsNullOrEmpty(_InfoLabel.Text)))
        {
          base.Control.Controls.Add(_InfoLabel);
        }

        _LayoutComplete = true;
      }
    }

    #endregion
  }

  /// <summary>
  /// Расширение класса <see cref="WizardStep"/>, предназначенное для вывода сообщения
  /// </summary>
  public class WizardStepWithMessage : ExtWizardStep
  {
    #region Конструктор

    /// <summary>
    /// Инициализация шага мастера
    /// </summary>
    public WizardStepWithMessage()
    {
      _MainControl = new InfoLabel();
      _MainControl.Dock = DockStyle.Fill;
      UseInfoColor = false;

      base.MainPanel.Controls.Add(_MainControl);
    }

    #endregion

    #region Свойства

    private InfoLabel _MainControl;

    /// <summary>
    /// Текст сообщения
    /// </summary>
    public string Text
    {
      get { return _MainControl.Text; }
      set { _MainControl.Text = value; }
    }

    /// <summary>
    /// Значок: нет (по умолчанию), информация, предупреждение, ошибка
    /// </summary>
    public MessageBoxIcon Icon
    {
      get { return _MainControl.Icon; }
      set { _MainControl.Icon = value; }
    }

    /// <summary>
    /// Если true, то для сообщения будет использовано цветовое оформление <see cref="SystemColors.Info"/> / <see cref="SystemColors.Info"/> (желтый фон).
    /// Если false (по умолчанию), то <see cref="SystemColors.Info"/> / <see cref="SystemColors.Info"/> (обычный цвет управляющих элементов).
    /// </summary>
    public bool UseInfoColor
    {
      get { return _UseInfoColor; }
      set
      {
        _UseInfoColor = value;
        if (value)
        {
          _MainControl.ResetBackColor();
          _MainControl.ResetForeColor();
        }
        else
        {
          _MainControl.BackColor = SystemColors.Window;
          _MainControl.ForeColor = SystemColors.WindowText;
        }
      }
    }
    private bool _UseInfoColor;

    #endregion

    #region Заглушка

    /// <summary>
    /// Не должно использоваться
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new Panel MainPanel { get { return base.MainPanel; } }

    #endregion
  }

  /// <summary>
  /// Шаг мастера с единственной группой радиокнопок
  /// </summary>
  public class WizardStepWithRadioButtons : ExtWizardStep
  {
    #region Конструктор

    /// <summary>
    /// Инициализация шага мастера
    /// </summary>
    /// <param name="items">Надписи для радиокнопок</param>
    public WizardStepWithRadioButtons(string[] items)
    {
#if DEBUG
      if (items == null)
        throw new ArgumentNullException("items");
#endif
      if (items.Length == 0)
        throw ExceptionFactory.ArgIsEmpty("items");

      TableLayoutPanel panel = new TableLayoutPanel();
      panel.RowCount = items.Length;
      panel.ColumnCount = 1;
      panel.Dock = DockStyle.Fill;
      MainPanel.Controls.Add(panel);
      RadioButton[] btns = new RadioButton[items.Length];
      for (int i = 0; i < items.Length; i++)
      {
        //  Panel.RowStyles[i].SizeType = SizeType.Percent;
        btns[i] = new RadioButton();
        btns[i].Text = items[i];
        // btns[i].Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
        btns[i].AutoSize = true;
        panel.Controls.Add(btns[i], 0, i);
      }

      _TheButtons = new EFPRadioButtons(BaseProvider, btns);
      _TheButtons.SelectedIndex = 0; // 07.05.2022
      _TheButtons.SelectedIndexEx.ValueChanged += SelectedIndexEx_ValueChanged;
    }

    private void SelectedIndexEx_ValueChanged(object sender, EventArgs args)
    {
      if (_ItemInfoTextArray != null && _TheButtons.SelectedIndex >= 0)
        InfoText = _ItemInfoTextArray[_TheButtons.SelectedIndex];
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер для управления кнопками. Используйте его свойства для
    /// определения выбранной позиции или для блокировки отдельных кнопок.
    /// </summary>
    public EFPRadioButtons TheButtons { get { return _TheButtons; } }
    private readonly EFPRadioButtons _TheButtons;

    /// <summary>
    /// Поясняющий текст для каждой кнопки.
    /// Если массив задан, то при выборе пользователем кнопки меняется текст <see cref="ExtWizardStep.InfoText"/>. Длина массива должна быть равна <see cref="TheButtons"/>.Count.
    /// По умолчанию - false - управление информационным текстом может выполняться прикладным кодом.
    /// </summary>
    public string[] ItemInfoTextArray
    {
      get { return _ItemInfoTextArray; }
      set
      {
        if (value != null)
        {
          if (value.Length != _TheButtons.Count)
            throw ExceptionFactory.ArgWrongCollectionCount("value", value, _TheButtons.Count);
          InfoText = "?";
        }
        _ItemInfoTextArray = value;
      }
    }
    private string[] _ItemInfoTextArray;

    /// <summary>
    /// Инициализация текста подсказки
    /// </summary>
    /// <param name="action"></param>
    protected internal override void OnBeginStep(WizardAction action)
    {
      base.OnBeginStep(action);
      SelectedIndexEx_ValueChanged(null, null);
    }

    #endregion

    // Убрано 07.05.2022
    //#region Переопределенные методы

    ///// <summary>
    ///// Инициализация при переходе к шагу мастера
    ///// </summary>
    ///// <param name="action">Причина вызова метода</param>
    //internal protected override void OnBeginStep(WizardAction action)
    //{
    //  base.OnBeginStep(action);

    //  _TheButtons.PrepareCommandItems(); // 01.02.2013
    //  _TheButtons.CommandItems.Active = true; // 01.02.2013
    //}

    //#endregion

    #region Заглушка

    /// <summary>
    /// Не должно использоваться
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new Panel MainPanel { get { return base.MainPanel; } }

    #endregion
  }

#if XXX // Не очень полезный класс 

  /// <summary>
  /// Шаблонный класс шага мастера с одним управляющим элементом, занимающим весь экран, и панелью инструментов
  /// </summary>
  /// <typeparam name="TControl"></typeparam>
  /// <typeparam name="TControlProvider"></typeparam>
  public class WizardStepWithToolBar<TControl, TControlProvider> : ExtWizardStepBase
    where TControl:Control, new()
    where TControlProvider : EFPControlBase
  {
  #region Конструктор

    public delegate TControlProvider ProviderCreator(EFPControlWithToolBar<TControl> cwt);

    public WizardStepWithToolBar(ProviderCreator creator)
    {
      EFPControlWithToolBar<TControl> cwt = new EFPControlWithToolBar<TControl>(BaseProvider, MainPanel);
      _ControlProvider = creator(cwt);
    }

  #endregion

  #region Свойства

    public TControlProvider ControlProvider { get { return _ControlProvider; } }
    private TControlProvider _ControlProvider;

  #endregion
  }

#endif

#if XXX //Есть WizardStepWithListSelection

  /// <summary>
  /// Шаг мастера со списком для выбора одной строки
  /// </summary>
  public class WizardStepWithListView : ExtWizardStep
  {
  #region Конструктор

    /// <summary>
    /// Инициализация шага мастера
    /// </summary>
    /// <param name="items">Строки для списка</param>
    public WizardStepWithListView(string[] items)
    {
      ListView control = new ListView();
      control.Dock = DockStyle.Fill;
      control.View = View.Details;
      control.HeaderStyle = ColumnHeaderStyle.None;
      control.Columns.Add("Item");
      control.Columns[0].Width = -1;
      control.SmallImageList = EFPApp.MainImages.ImageList;
      control.LabelEdit = false;
      control.FullRowSelect = true;
      control.HideSelection = false;
      MainPanel.Controls.Add(control);

      _Items = items;
      for (int i = 0; i < items.Length; i++)
        control.Items.Add(items[i]);

      _TheListView = new EFPListView(BaseProvider, control);
      control.Resize += new EventHandler(Control_Resize);
    }


  #endregion

  #region Свойства

    /// <summary>
    /// Строки для списка.
    /// Задаются в конструкторе
    /// </summary>
    public string[] Items { get { return _Items; } }
    private readonly string[] _Items;

    /// <summary>
    /// Провайдер для управления списком. Используйте его свойства для
    /// определения выбранной позиции 
    /// </summary>
    public EFPListView TheListView { get { return _TheListView; } }
    private readonly EFPListView _TheListView;

    /// <summary>
    /// Общий значок для всех строк списка.
    /// Для задания различных значков используйте свойство ImageKeys вместо этого свойства.
    /// </summary>
    public string ImageKey
    {
      get { return _ImageKey; }
      set
      {
        _ImageKey = value;
        for (int i = 0; i < _TheListView.Control.Items.Count; i++)
          _TheListView.Control.Items[i].ImageKey = value;
      }
    }
    private string _ImageKey;

    /// <summary>
    /// Индивидуальные значки для элементов списка.
    /// </summary>
    public string[] ImageKeys
    {
      get { return _ImageKeys; }
      set
      {
        if (value.Length != _Items.Length)
          throw ExceptionFactory.ArgWrongCollectionCount("value", value, _Items.Length);
        _ImageKeys = value;
        for (int i = 0; i < _TheListView.Control.Items.Count; i++)
          _TheListView.Control.Items[i].ImageKey = value[i];
      }
    }
    private string[] _ImageKeys;

    /// <summary>
    /// Текст для дополнительного столбца в списке.
    /// Если свойство не установлено (по умолчанию), то в списке только один столбец со строками Items.
    /// Длина массива должна быть равна количеству элементов в списке Items.
    /// Повторная установка свойства не допускается.
    /// </summary>
    public string[] SubItems
    {
      get { return _SubItems; }
      set
      {
        if (_SubItems != null)
          throw ExceptionFactory.RepeatedCall(this, "SubItems");
        if (value.Length != _Items.Length)
          throw ExceptionFactory.ArgWrongCollectionCount("value", value, _Items.Length);
        _TheListView.Control.Columns.Add(Res.ListSelectDialog_ColTitle_Sub);
        _SubItems = value;
        for (int i = 0; i < _TheListView.Control.Items.Count; i++)
          _TheListView.Control.Items[i].SubItems.Add(value[i]);
      }
    }
    private string[] _SubItems;

  #endregion

    // Убрано 07.05.2022
    //#region Переопределенные методы

    ///// <summary>
    ///// Инициализация при переходе к шагу мастера
    ///// </summary>
    ///// <param name="action">Причина вызова метода</param>
    //internal protected override void OnBeginStep(WizardAction action)
    //{
    //  base.OnBeginStep(action);

    //  TheListView.PrepareCommandItems(); // 01.02.2013
    //  TheListView.CommandItems.Active = true; // 01.02.2013
    //}

    //#endregion

  #region Внутренняя реализация

    private bool _InsideResize = false;

    void Control_Resize(object sender, EventArgs args)
    {
      if (_InsideResize)
        return;

      _InsideResize = true;
      try
      {
        if (_TheListView.Control.Columns.Count == 1)
          _TheListView.Control.Columns[0].Width = -2;
        else
        {
          _TheListView.Control.Columns[0].Width = -1;
          _TheListView.Control.Columns[1].Width = -2;
        }
      }
      finally
      {
        _InsideResize = false;
      }
    }

  #endregion

  #region Заглушка

    /// <summary>
    /// Не должно использоваться
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new Panel MainPanel { get { return base.MainPanel; } }

  #endregion
  }
#endif

  /// <summary>
  /// Шаг мастера с табличным просмотром
  /// </summary>
  public class WizardStepWithDataGridView : ExtWizardStep
  {
    #region Конструктор

    /// <summary>
    /// Делегат метода для создания провайдера табличного просмотра
    /// </summary>
    /// <param name="cwt">Базовый провайдер плюс управляющий элемент. 
    /// Обеспечивается <see cref="WizardStepWithDataGridView"/>.</param>
    /// <returns>Провайдер, созданный прикладным кодом</returns>
    public delegate EFPDataGridView ProviderCreator(EFPControlWithToolBar<DataGridView> cwt);

    /// <summary>
    /// Создает шаг мастера с провайдером, который создается с помощью передаваемого делегата.
    /// </summary>
    /// <param name="creator">Делегат, который создает провайдер табличного просмотра</param>
    public WizardStepWithDataGridView(ProviderCreator creator)
    {
      EFPControlWithToolBar<DataGridView> cwt = new EFPControlWithToolBar<DataGridView>(BaseProvider, MainPanel);
      _TheControlProvider = creator(cwt);
      if (_TheControlProvider == null)
        throw ExceptionFactory.ObjectPropertyNotSet(this, "TheControlProvider");

      // Присоединяем BaseProvider
      cwt.BaseProvider.Parent = BaseProvider;
    }

    /// <summary>
    /// Создает шаг мастера с провайдером <see cref="EFPDataGridView"/>.
    /// </summary>
    public WizardStepWithDataGridView()
      : this(delegate (EFPControlWithToolBar<DataGridView> cwt) { return new EFPDataGridView(cwt); })
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер табличного просмотра.
    /// Инициализируется в конструкторе.
    /// </summary>
    public EFPDataGridView TheControlProvider { get { return _TheControlProvider; } }
    private readonly EFPDataGridView _TheControlProvider;

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Переход к шагу мастера.
    /// Выполняет необходимую инциализацию <see cref="EFPDataGridView"/>.
    /// </summary>
    /// <param name="action">Действие</param>
    internal protected override void OnBeginStep(WizardAction action)
    {
      // Перенесено наверх 26.07.2023
      // Обработчик события BeginStep может присоединить таблицу к просмотру
      base.OnBeginStep(action);

      //if (_TheControlProvider.UseRowImages &&
      //  (!_TheControlProvider.UseRowImagesDataError) &&
      //  _TheControlProvider.TopLeftCellUserImage == null)

      //  _TheControlProvider.InitTopLeftCellTotalInfo();

      // Убрано 07.05.2022
      //_TheControlProvider.PrepareCommandItems();
      //_TheControlProvider.CommandItems.Active = true;
    }

    // ReSharper disable once RedundantOverriddenMember
    /// <summary>
    /// Вызывает метод базового класса
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    internal protected override bool OnEndStep(WizardAction action)
    {
      /*
      // 31.07.2013
      // Сохраняем настройку табличного просмотра
      if (!FTheGridHandler.SaveGridConfigIfRequired())
        return false;
      */
      return base.OnEndStep(action);
    }

    #endregion

    #region Заглушка

    /// <summary>
    /// Не должно использоваться
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new Panel MainPanel { get { return base.MainPanel; } }

    #endregion
  }

  /// <summary>
  /// Шаг мастера для отображения списка ошибок
  /// </summary>
  public class WizardStepWithErrorDataGridView : WizardStepWithDataGridView
  {
    #region Конструктор

    /// <summary>
    /// Инициализация шага мастера.
    /// Эта версия позволяет выводить код ошибок в просмотре.
    /// </summary>
    /// <param name="codeWidth">Ширина столба для вывода кода ошибки в символах. 0-нет столбца</param>
    public WizardStepWithErrorDataGridView(int codeWidth)
      : base(delegate (EFPControlWithToolBar<DataGridView> cwt) { return new EFPErrorDataGridView(cwt); })
    {
      TheControlProvider.CodeWidth = codeWidth;
    }

    /// <summary>
    /// Инициализация шага мастера.
    /// Коды ошибок не выводятся
    /// </summary>
    public WizardStepWithErrorDataGridView()
      : this(0)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Обработчик табличного просмотра
    /// </summary>
    public new EFPErrorDataGridView TheControlProvider
    {
      get { return (EFPErrorDataGridView)(base.TheControlProvider); }
    }

    /// <summary>
    /// Присоединенный список ошибок
    /// </summary>
    public ErrorMessageList ErrorMessages
    {
      get { return TheControlProvider.ErrorMessages; }
      set { TheControlProvider.ErrorMessages = value; }
    }

    #endregion
  }

  /// <summary>
  /// Шаг мастера для привязки строк и столбцов.
  /// Используется в мастерах вставки в справочники из буфера обмена.
  /// </summary>
  public class WizardStepSelRC : WizardStepWithDataGridView
  {
    #region Конструктор

    /// <summary>
    /// Создает шаг мастера
    /// </summary>
    public WizardStepSelRC()
      : base(delegate (EFPControlWithToolBar<DataGridView> cwt) { return new EFPSelRCDataGridView(cwt); })
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер табличного просмотра
    /// </summary>
    public new EFPSelRCDataGridView TheControlProvider
    {
      get { return (EFPSelRCDataGridView)(base.TheControlProvider); }
    }

    #endregion
  }

  /// <summary>
  /// Шаг мастера с табличным просмотром, предназначенным для ввода данных пользователем
  /// </summary>
  public class WizardStepWithInputDataGridView : WizardStepWithDataGridView
  {
    #region Конструктор

    /// <summary>
    /// Создает шаг мастера
    /// </summary>
    public WizardStepWithInputDataGridView()
      : base(delegate (EFPControlWithToolBar<DataGridView> cwt) { return new EFPInputDataGridView(cwt); })
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер табличного просмотра
    /// </summary>
    public new EFPInputDataGridView TheControlProvider
    {
      get { return (EFPInputDataGridView)(base.TheControlProvider); }
    }

    #endregion
  }

  /// <summary>
  /// Шаг мастера с несколькими вкладками
  /// </summary>
  public class WizardStepWithTabControl : ExtWizardStep
  {
    #region Конструктор

    /// <summary>
    /// Создает пустой <see cref="TabControl"/> без вкладок
    /// </summary>
    public WizardStepWithTabControl()
    {
      TabControl tc = new TabControl();
      tc.Dock = DockStyle.Fill;
      tc.ImageList = EFPApp.MainImages.ImageList;
      MainPanel.Controls.Add(tc);
      _TheTabControl = new EFPTabControl(this.BaseProvider, tc);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер для <see cref="TabControl"/>.
    /// Используйте его для добавление вкладок.
    /// </summary>
    public EFPTabControl TheTabControl { get { return _TheTabControl; } }
    private readonly EFPTabControl _TheTabControl;

    /// <summary>
    /// Использовать рамку вокруг TabControl не следует
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new string GroupTitle
    {
      get { return base.GroupTitle; }
      set { base.GroupTitle = value; }
    }

    #region Заглушка

    /// <summary>
    /// Не должно использоваться
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public new Panel MainPanel { get { return base.MainPanel; } }

    #endregion

    #endregion
  }
}
