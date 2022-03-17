// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using FreeLibSet.Core;

// Реализации WizardStep стандартных видов

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Расширение класса WizardStep, содержащее элемент управления GroupBox и два
  /// текстовых блока (сверху и снизу). Текстовые блоки автоматически подбирают
  /// свои размеры. GroupBox занимает всю площадь панели, кроме текстовых блоков
  /// </summary>
  public class WizardStepWithGroupBox : WizardStep
  {
    #region Конструктор

    /// <summary>
    /// Создает шаг мастера
    /// </summary>
    public WizardStepWithGroupBox()
      : base(new Panel(), true)
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
  /// Расширение класса WizardStep, предназначенное для вывода сообщения
  /// </summary>
  public class WizardStepWithMessage : WizardStep
  {
    #region Конструктор

    /// <summary>
    /// Инициализация шага мастера
    /// </summary>
    public WizardStepWithMessage()
      : base(new Panel(), true)
    {
      _TheGroupBox = new GroupBox();
      _TheGroupBox.Dock = DockStyle.Fill;
      base.Control.Controls.Add(_TheGroupBox);

      _TheLabel = new Label();
      _TheLabel.AutoSize = false;
      _TheLabel.Dock = DockStyle.Fill;
      _TheLabel.UseMnemonic = false;
      _TheLabel.Padding = new Padding(3);
      _TheLabel.TextAlign = ContentAlignment.MiddleLeft;
      _TheGroupBox.Controls.Add(_TheLabel);

      _ThePicture = new PictureBox();
      _ThePicture.Dock = DockStyle.Left;
      _ThePicture.SizeMode = PictureBoxSizeMode.CenterImage;
      _ThePicture.Visible = false;
      _TheGroupBox.Controls.Add(_ThePicture);

      _Icon = MessageBoxIcon.None;
    }

    #endregion

    #region Свойства

    private GroupBox _TheGroupBox;
    private PictureBox _ThePicture;

    private Label _TheLabel;

    /// <summary>
    /// Текст сообщения
    /// </summary>
    public string Text
    {
      get { return _TheLabel.Text; }
      set { _TheLabel.Text = value; }
    }

    /// <summary>
    /// Заголовок сообщения (но не всего окна мастера)
    /// </summary>
    public string Caption
    {
      get { return _TheGroupBox.Text; }
      set { _TheGroupBox.Text = value; }
    }


    /// <summary>
    /// Значок: нет (по умолчанию), информация, предупреждение, ошибка
    /// </summary>
    public MessageBoxIcon Icon
    {
      get { return _Icon; }
      set
      {
        if (value == _Icon)
          return;
        Icon newIcon = WinFormsTools.GetSystemIcon(value);
        if (newIcon == null)
          _ThePicture.Visible = false;
        else
        {
          _ThePicture.Image = newIcon.ToBitmap();
          newIcon.Dispose();
          _ThePicture.Width = _ThePicture.Image.Width + 8;
          _ThePicture.Visible = true;
        }
        _Icon = value;
      }
    }
    private MessageBoxIcon _Icon;

    #endregion
  }

  /// <summary>
  /// Шаг мастера с единственной группой радиокнопок
  /// </summary>
  public class WizardStepWithRadioButtons : WizardStepWithGroupBox
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
        throw new ArgumentException("Список строк пустой", "items");

      TableLayoutPanel panel = new TableLayoutPanel();
      panel.RowCount = items.Length;
      panel.ColumnCount = 1;
      panel.Dock = DockStyle.Fill;
      TheGroupBox.Controls.Add(panel);
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
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер для управления кнопками. Используйте его свойства для
    /// определения выбранной позиции или для блокировки отдельных кнопок
    /// </summary>
    public EFPRadioButtons TheButtons { get { return _TheButtons; } }
    private EFPRadioButtons _TheButtons;

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Инициализация при переходе к шагу мастера
    /// </summary>
    /// <param name="action">Причина вызова метода</param>
    internal protected override void OnBeginStep(WizardAction action)
    {
      base.OnBeginStep(action);

      _TheButtons.PrepareCommandItems(); // 01.02.2013
      _TheButtons.CommandItems.Active = true; // 01.02.2013
    }

    #endregion
  }

  /// <summary>
  /// Шаг мастера со списком для выбора одной строки
  /// </summary>
  public class WizardStepWithListView : WizardStepWithGroupBox
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
      control.SmallImageList = EFPApp.MainImages;
      control.LabelEdit = false;
      control.FullRowSelect = true;
      control.HideSelection = false;
      TheGroupBox.Controls.Add(control);

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
    private string[] _Items;

    /// <summary>
    /// Провайдер для управления списком. Используйте его свойства для
    /// определения выбранной позиции 
    /// </summary>
    public EFPListView TheListView { get { return _TheListView; } }
    private EFPListView _TheListView;

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
          throw new ArgumentException("Неправильная длина массива");
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
          throw new InvalidOperationException("Повторная установка свойства");
        if (value.Length != _Items.Length)
          throw new ArgumentException("Неправильная длина массива");
        _TheListView.Control.Columns.Add("Значения");
        _SubItems = value;
        for (int i = 0; i < _TheListView.Control.Items.Count; i++)
          _TheListView.Control.Items[i].SubItems.Add(value[i]);
      }
    }
    private string[] _SubItems;

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Инициализация при переходе к шагу мастера
    /// </summary>
    /// <param name="action">Причина вызова метода</param>
    internal protected override void OnBeginStep(WizardAction action)
    {
      base.OnBeginStep(action);

      TheListView.PrepareCommandItems(); // 01.02.2013
      TheListView.CommandItems.Active = true; // 01.02.2013
    }

    #endregion

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
  }

  /// <summary>
  /// Шаг мастера с табличным просмотром
  /// </summary>
  public class WizardStepWithDataGridView : WizardStepWithGroupBox
  {
    #region Конструктор

    /// <summary>
    /// Создает шаг мастера с провадером EFPDataGridView.
    /// </summary>
    public WizardStepWithDataGridView()
    {
      DoInit(new EFPDataGridView(BaseProvider, new DataGridView()));
    }

    /// <summary>
    /// Создает шаг мастера с указанным провадером
    /// </summary>
    /// <param name="controlProvider">Созданный провайдер, производный от EFPDataGridView</param>
    public WizardStepWithDataGridView(EFPDataGridView controlProvider)
    {
#if DEBUG
      if (controlProvider == null)
        throw new ArgumentNullException("controlProvider");
      if (controlProvider.BaseProvider.Parent != null)
        throw new ArgumentException("Свойство EFPDataGridView.BaseProvider.Parent уже установлено. " +
          "Этот конструктор может получать только свежесозданный, ни к чему не присоединенный обработчик табличного просмотра", "controlProvider");
#endif

      // Присоединяем BaseProvider
      controlProvider.BaseProvider.Parent = BaseProvider;

      DoInit(controlProvider);
    }

    /// <summary>
    /// Защищенная версия конструктора, не создающая табличный просмотр и провайдер.
    /// Предполагается, что конструктор производного класса вызывает метод DoInit()
    /// </summary>
    /// <param name="dummy">Не используется</param>
    protected WizardStepWithDataGridView(bool dummy)
    {
    }

    /// <summary>
    /// Используется конструктором производного класса для присоединения табличного просмотра.
    /// </summary>
    /// <param name="controlProvider">Созданный провайдер табличного просмотра</param>
    protected void DoInit(EFPDataGridView controlProvider)
    {
      _TheControlProvider = controlProvider;
      controlProvider.Control.Dock = DockStyle.Fill;
      TheGroupBox.Controls.Add(controlProvider.Control);

      _TheSpeedPanel = new Panel();
      _TheSpeedPanel.Dock = DockStyle.Top;
      TheGroupBox.Controls.Add(_TheSpeedPanel);
      TheControlProvider.ToolBarPanel = _TheSpeedPanel;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер табличного просмотра.
    /// Инициализируется в конструкторе.
    /// </summary>
    public EFPDataGridView TheControlProvider { get { return _TheControlProvider; } }
    private EFPDataGridView _TheControlProvider;

    private Panel _TheSpeedPanel;

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Переход к шагу мастера.
    /// Выполняет необходимую инциализацию EFPDataGridView.
    /// </summary>
    /// <param name="action">Действие</param>
    internal protected override void OnBeginStep(WizardAction action)
    {
      if (_TheControlProvider.CommandItems.Control == null)
        _TheControlProvider.ToolBarPanel = _TheSpeedPanel;

      if (_TheControlProvider.UseRowImages &&
        (!_TheControlProvider.UseRowImagesDataError) &&
        _TheControlProvider.TopLeftCellUserImage == null)

        _TheControlProvider.InitTopLeftCellTotalInfo();

      base.OnBeginStep(action);

      _TheControlProvider.PrepareCommandItems();
      _TheControlProvider.CommandItems.Active = true;
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
  }

  /// <summary>
  /// Шаг мастера для отображения списка ошибок
  /// </summary>
  public class WizardStepWithErrorDataGridView : WizardStepWithDataGridView
  {
    #region Конструктор

    /// <summary>
    /// Инициализация шага мастера.
    /// Эта версия позволяет выводить код ошибок в просмотре
    /// </summary>
    /// <param name="codeWidth">Ширина столба для вывода кода ошибки в символах. 0-нет столбца</param>
    public WizardStepWithErrorDataGridView(int codeWidth)
      : base(false)
    {
      DoInit(new EFPErrorDataGridView(BaseProvider, new DataGridView()));
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
}
