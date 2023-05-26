// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Controls;
using FreeLibSet.Core;
using FreeLibSet.UICore;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Простая форма с кнопками "ОК" и "Отмена".
  /// Используйте свойство MainPanel для добавления управляющих элементов.
  /// Также есть две дополнительные панели TopPanel и BottomPanel. Эти панели автоматически подбирают свои размеры под содержимое.
  /// Эти панели изначально пусты и скрыты. В них можно добавлять свои элементы, установив свойство Visible=true.
  /// Форма содержит FormProvider.
  /// При необходимости использовать форму в немодальном режиме, добавляйте обработчик
  /// к efpOk.Click или Form.Closed, проверяя DialogResult
  /// </summary>
  public partial class OKCancelForm : Form
  {
    /// <summary>
    /// Создает форму с кнопками "ОК" и "Отмена".
    /// Устанавливается рамка формы Fixed3D, кнопки минимизации и максимизации отключаются
    /// </summary>
    public OKCancelForm()
      : this(true)
    {
    }

    /// <summary>
    /// Создает форму с кнопками "ОК" и "Отмена".
    /// </summary>
    /// <param name="initBorder">Если true, то устанавливается рамка формы Fixed3D, кнопки минимизации и максимизации отключаются.
    /// Если false, используется рамка формы по умолчанию с возможностью установки размеров формы</param>
    public OKCancelForm(bool initBorder)
    {
      InitializeComponent();
      if (initBorder)
      {
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      }

      EFPApp.InitFormImages(this);

      _FormProvider = new EFPFormProvider(this);
      _OKButtonProvider = new EFPButton(FormProvider, btnOk);
      _CancelButtonProvider = new EFPButton(FormProvider, btnCancel);
      _NoButtonProvider = new EFPButton(FormProvider, btnNo);
      FormProvider.AddFormCheck(new UIValidatingEventHandler(DoFormValidating));

      //BottomPanel.BackColor = Color.Red;
      //this.panel1.BackColor = Color.Yellow;

      // 31.03.2020
      this.TopPanel.Layout += new LayoutEventHandler(TopOrBottomPanel_Layout);
      this.TopPanel.Resize += new EventHandler(TopOrBottomPanel_Layout);
      this.BottomPanel.Layout += new LayoutEventHandler(TopOrBottomPanel_Layout);
      this.BottomPanel.Resize += new EventHandler(TopOrBottomPanel_Layout);
    }

    #region Провайдеры формы

    /// <summary>
    /// Провайдер формы
    /// </summary>
    public EFPFormProvider FormProvider { get { return _FormProvider; } }
    private EFPFormProvider _FormProvider;

    /// <summary>
    /// Провайдер кнопки "ОК"
    /// </summary>
    public EFPButton OKButtonProvider { get { return _OKButtonProvider; } }
    private EFPButton _OKButtonProvider;

    /// <summary>
    /// Провайдер кнопки "Отмена"
    /// </summary>
    public EFPButton CancelButtonProvider { get { return _CancelButtonProvider; } }
    private EFPButton _CancelButtonProvider;

    /// <summary>
    /// Провайдер кнопки "Нет".
    /// По умолчанию эта кнопка невидима
    /// </summary>
    public EFPButton NoButtonProvider { get { return _NoButtonProvider; } }
    private EFPButton _NoButtonProvider;

    #endregion

    #region События

    /// <summary>
    /// Вызывается при нажатии кнопки ОК для проверки корректности значений
    /// В отличие от обработчиков, присоединяемых efpForm.AddFormCheck(), 
    /// здесь параметром Object будет объект OKCancelForm
    /// </summary>
    public event UIValidatingEventHandler FormValidating;

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// Минимальные размеры по умолчанию - чтобы панель с кнопками была видна
    /// </summary>
    protected override Size DefaultMinimumSize
    {
      get
      {
        if (ButtonsPanel == null)
          return base.DefaultMinimumSize;

        int leftPos = int.MaxValue;
        int rightPos = 0;
        bool flag = false;
        foreach (Control btn in ButtonsPanel.Controls)
        {
          if (btn.Visible)
          {
            flag = true;
            leftPos = Math.Min(leftPos, btn.Left);
            rightPos = Math.Max(rightPos, btn.Right);
          }
        }

        if (!flag) // нет ни одной кнопки
          return base.DefaultMinimumSize;


        int w = rightPos + leftPos; // отступ в 8 пикселей от правого края кнопки

        int h = base.Height - base.ClientSize.Height + ButtonsPanel.Height;
        if (TopPanel.Visible)
          h += TopPanel.Height;
        if (BottomPanel.Visible)
          h += BottomPanel.Height;

        return new Size(w + base.Width - ButtonsPanel.Width,
          h + 8);
      }
    }

    #endregion

    #region Дополнительно

    /// <summary>
    /// Создает и добавляет информационное сообщение в верхней или нижней части формы.
    /// Объект InfoLabel присоединяется к панели TopPanel или BottomPanel, в зависимости от значения <paramref name="dock"/>.
    /// Для добавления элементов других типов используйте верхнюю и нижние панели самостоятельно.
    /// </summary>
    /// <param name="dock">Расположение метки. Допустимые значения: Top и Bottom</param>
    /// <returns></returns>
    public InfoLabel AddInfoLabel(DockStyle dock)
    {
      Panel parent;
      switch (dock)
      {
        case DockStyle.Top:
          parent = TopPanel;
          break;
        case DockStyle.Bottom:
          parent = BottomPanel;
          break;
        default:
          throw new ArgumentException("dock");
      }

      InfoLabel lbl = new InfoLabel();
      lbl.Dock = dock;
      lbl.AutoSize = true;
      parent.Controls.Add(lbl);
      parent.Visible = true;
      return lbl;
    }

    #endregion

    #region Обработчики

    /// <summary>
    /// Обработчик изменения размеров панелей TopPanel и BottomPanel, а также изменения дочерних элементов.
    /// Можно использовать один обработчик для событий Panel.Resize и Panel.Layot, хотя аргументы имеют разный тип
    /// </summary>
    /// <param name="sender">Панель TopPanel или BottomPanel</param>
    /// <param name="args">Не используется</param>
    void TopOrBottomPanel_Layout(object sender, EventArgs args)
    {
      // 31.03.2020
      // Больше не используем свойства Panel.AutoSize и AutoSizeMode для управления размерами панели,
      // т.к. они неправильно работают с InfoLabel (это может быть и бяка в InfoLabel)
      // Вместо этого, устанавливаем размеры самостоятельно

      Panel tbPanel = (Panel)sender;
      tbPanel.Visible = tbPanel.Controls.Count > 0;
      if (tbPanel.Controls.Count == 0)
        return;

      MinMax<int> mm = new MinMax<int>();
      // Дочерние элементы могут располагаться в любом месте панели, и сверху и снизу.
      // Они получат нормальные координаты только после изменения размеров, а не сейчас
      foreach (Control child in tbPanel.Controls)
      {
        mm += child.Top;
        mm += child.Bottom;
      }
      tbPanel.Height = mm.MaxValue - mm.MinValue;
    }

    private void btn_Click(object sender, EventArgs args)
    {
      Button button = (Button)sender;
      DialogResult = button.DialogResult;
      Close();
    }

    private void DoFormValidating(object sender, UIValidatingEventArgs args)
    {
      if (FormValidating == null)
        return;
      FormValidating(this, args);
    }

    #endregion
  }


  /// <summary>
  /// Простая форма, содержащая единственный управляющий элемент и панель инстументов.
  /// Также есть две дополнительные панели TopPanel и BottomPanel. Эти панели автоматически подбирают свои размеры под содержимое.
  /// Эти панели изначально пусты и скрыты. В них можно добавлять свои элементы, установив свойство Visible=true.
  /// Для управляющего элемента требуется отдельно создать подходящий провайдер,
  /// производный от EFPControlBase.
  /// </summary>
  /// <typeparam name="T">Тип управляющего элемента</typeparam>
  public class SimpleForm<T> : Form, IEFPControlWithToolBar<T>
    where T : Control, new()
  {
    #region Конструктор

    /// <summary>
    /// Создает форму с управляющим элементом и панелью инструментов
    /// </summary>
    public SimpleForm()
    {
      _FormProvider = new EFPFormProvider(this);

      _MainPanel = new Panel();
      _MainPanel.Dock = DockStyle.Fill;
      _MainPanel.TabIndex = 1;
      Controls.Add(_MainPanel);

      _TopPanel = new Panel();
      _TopPanel.Dock = DockStyle.Top;
      //_TopPanel.AutoSize = true;
      //_TopPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      _TopPanel.Visible = false;
      _TopPanel.TabIndex = 0;
      Controls.Add(_TopPanel);

      _BottomPanel = new Panel();
      _BottomPanel.Dock = DockStyle.Bottom;
      //_BottomPanel.AutoSize = true;
      //_BottomPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      _BottomPanel.Visible = false;
      Controls.Add(_BottomPanel);

      _ControlWithToolBar = new EFPControlWithToolBar<T>(_FormProvider, _MainPanel);
      _ControlWithToolBar.Control.Name = "MainControl"; // 27.06.2019 Требуется, чтобы табличный просмотр и другие элементы могли сохранять свои параметры для композиции рабочего стола.

      EFPApp.SetFormSize(this, 50, 50);

      // 31.03.2020
      _TopPanel.Layout += new LayoutEventHandler(TopOrBottomPanel_Layout);
      _TopPanel.Resize += new EventHandler(TopOrBottomPanel_Layout);
      _BottomPanel.Layout += new LayoutEventHandler(TopOrBottomPanel_Layout);
      _BottomPanel.Resize += new EventHandler(TopOrBottomPanel_Layout);

    }

    #endregion

    #region Свойства

    /// <summary>
    /// Основная панель - занимает всю форму. На ней располагается основной управляющий
    /// элемент Control и панель кнопок, если есть
    /// </summary>
    public Panel MainPanel { get { return _MainPanel; } }
    private Panel _MainPanel;

    /// <summary>
    /// Дополнительная  панель в верхней части окна. По умолчанию она пуста и скрыта.
    /// </summary>
    public Panel TopPanel { get { return _TopPanel; } }
    private Panel _TopPanel;

    /// <summary>
    /// Дополнительная  панель в нижней части окна. По умолчанию она пуста и скрыта.
    /// </summary>
    public Panel BottomPanel { get { return _BottomPanel; } }
    private Panel _BottomPanel;

    /// <summary>
    /// Провайдер формы
    /// </summary>
    public EFPFormProvider FormProvider { get { return _FormProvider; } }
    private EFPFormProvider _FormProvider;

    /// <summary>
    /// Управляющий элемент и панель инструментов
    /// </summary>
    public EFPControlWithToolBar<T> ControlWithToolBar { get { return _ControlWithToolBar; } }
    private EFPControlWithToolBar<T> _ControlWithToolBar;

    #endregion

    #region IEFPControlWithToolBar<T> Members

    EFPBaseProvider IEFPControlWithToolBar.BaseProvider
    {
      get { return _ControlWithToolBar.BaseProvider; }
    }

    /// <summary>
    /// Управляющий элемент
    /// </summary>
    public T Control
    {
      get { return _ControlWithToolBar.Control; }
    }

    Control IEFPControlWithToolBar.Control { get { return _ControlWithToolBar.Control; } }

    /// <summary>
    /// Панель для размещения кнопок
    /// </summary>
    Panel IEFPControlWithToolBar.ToolBarPanel
    {
      get { return _ControlWithToolBar.ToolBarPanel; }
    }

    #endregion

    #region Обработчики

    /// <summary>
    /// Обработчик изменения размеров панелей TopPanel и BottomPanel, а также изменения дочерних элементов.
    /// Можно использовать один обработчик для событий Panel.Resize и Panel.Layot, хотя аргументы имеют разный тип
    /// </summary>
    /// <param name="sender">Панель TopPanel или BottomPanel</param>
    /// <param name="args">Не используется</param>
    void TopOrBottomPanel_Layout(object sender, EventArgs args)
    {
      // 31.03.2020
      // Больше не используем свойства Panel.AutoSize и AutoSizeMode для управления размерами панели,
      // т.к. они неправильно работают с InfoLabel (это может быть и бяка в InfoLabel)
      // Вместо этого, устанавливаем размеры самостоятельно

      Panel tbPanel = (Panel)sender;
      tbPanel.Visible = tbPanel.Controls.Count > 0;
      if (tbPanel.Controls.Count == 0)
        return;

      MinMax<int> mm = new MinMax<int>();
      // Дочерние элементы могут располагаться в любом месте панели, и сверху и снизу.
      // Они получат нормальные координаты только после изменения размеров, а не сейчас
      foreach (Control child in tbPanel.Controls)
      {
        mm += child.Top;
        mm += child.Bottom;
      }
      tbPanel.Height = mm.MaxValue - mm.MinValue;
    }

    #endregion

    #region Дополнительно

    /// <summary>
    /// Создает и добавляет информационное сообщение в верхней или нижней части формы.
    /// Объект InfoLabel присоединяется к панели TopPanel или BottomPanel, в зависимости от значения <paramref name="dock"/>.
    /// Для добавления элементов других типов используйте верхнюю и нижние панели самостоятельно.
    /// </summary>
    /// <param name="dock">Расположение метки. Допустимые значения: Top и Bottom</param>
    /// <returns></returns>
    public InfoLabel AddInfoLabel(DockStyle dock)
    {
      Panel parent;
      switch (dock)
      {
        case DockStyle.Top:
          parent = TopPanel;
          break;
        case DockStyle.Bottom:
          parent = BottomPanel;
          break;
        default:
          throw new ArgumentException("dock");
      }

      InfoLabel lbl = new InfoLabel();
      lbl.Dock = dock;
      lbl.AutoSize = true;
      parent.Controls.Add(lbl);
      parent.Visible = true;
      return lbl;
    }

    #endregion
  }

  /// <summary>
  /// Простая форма, содержащая единственный управляющий элемент, панель инстументов
  /// и кнопки "ОК" и "Отмена".
  /// Для управляющего элемента обычно требуется отдельно создать подходящий провайдер,
  /// производный от EFPControlBase.
  /// </summary>
  /// <typeparam name="T">Тип управляющего элемента</typeparam>
  public class OKCancelSimpleForm<T> : OKCancelForm, IEFPControlWithToolBar<T>
    where T : Control, new()
  {
    #region Конструктор

    /// <summary>
    /// Создает форму, управляющий элемент, панель инструментов и кнопки.
    /// Для управляющего элемента требуется отдельно создать подходящий провайдер,
    /// производный от EFPControlBase.
    /// Эта версия конструктора не создает дополнительную рамку
    /// </summary>
    public OKCancelSimpleForm()
      :this(false)
    {
    }

    /// <summary>
    /// Создает форму, управляющий элемент, панель инструментов и кнопки.
    /// Для управляющего элемента требуется отдельно создать подходящий провайдер,
    /// производный от EFPControlBase.
    /// <param name="useGroupBox">Если true, то будет создана дополнительная рамка вокруг элемента</param>
    /// </summary>
    public OKCancelSimpleForm(bool useGroupBox)
    {
      // Возвращаем стандартные значения
      FormBorderStyle = FormBorderStyle.Sizable;
      MaximizeBox = true;
      MinimizeBox = true;

      Control parent = MainPanel;
      if (useGroupBox)
      {
        _GroupBox = new GroupBox();
        _GroupBox.Dock = DockStyle.Fill;
        MainPanel.Controls.Add(_GroupBox);
        parent = _GroupBox;
      }

      _ControlWithToolBar = new EFPControlWithToolBar<T>(base.FormProvider, parent);
      _ControlWithToolBar.Control.Name = "MainControl"; // 27.06.2019 Требуется, чтобы табличный просмотр и другие элементы могли сохранять свои параметры для композиции рабочего стола.

      EFPApp.SetFormSize(this, 50, 50);
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Рамка вокруг управляющего элемента и панели инструментов.
    /// Если использован конструктор, не создающий рамку, то свойство возвращает null.
    /// </summary>
    public GroupBox GroupBox { get { return _GroupBox; } }
    private GroupBox _GroupBox;

    /// <summary>
    /// Управляющий элемент и панель инструментов
    /// </summary>
    public EFPControlWithToolBar<T> ControlWithToolBar { get { return _ControlWithToolBar; } }
    private EFPControlWithToolBar<T> _ControlWithToolBar;

    #endregion

    #region IEFPControlWithToolBar<T> Members

    EFPBaseProvider IEFPControlWithToolBar.BaseProvider
    {
      get { return _ControlWithToolBar.BaseProvider; }
    }

    /// <summary>
    /// Управляющий элемент
    /// </summary>
    public T Control
    {
      get { return _ControlWithToolBar.Control; }
    }

    Control IEFPControlWithToolBar.Control
    {
      get { return _ControlWithToolBar.Control; }
    }

    /// <summary>
    /// Панель для размещения кнопок
    /// </summary>
    Panel IEFPControlWithToolBar.ToolBarPanel
    {
      get { return _ControlWithToolBar.ToolBarPanel; }
    }

    #endregion
  }


  /// <summary>
  /// Форма со списком ListView, кнопками "ОК" и "Отмена"
  /// Объект ListView имеет стиль списка и единственную колонку без заголовка,
  /// ширина которой синхронизируется с размерами окна
  /// </summary>
  public class ListViewOKCancelForm : OKCancelForm
  {
    #region Конструктор

    /// <summary>
    /// Создает форму
    /// </summary>
    public ListViewOKCancelForm()
    {
      _TheGroupBox = new GroupBox();
      _TheGroupBox.Dock = DockStyle.Fill;
      MainPanel.Controls.Add(_TheGroupBox);

      _TheListView = new ListView();
      _TheListView.Dock = DockStyle.Fill;
      _TheListView.View = View.Details;
      _TheListView.HeaderStyle = ColumnHeaderStyle.None;
      _TheListView.Columns.Add(String.Empty);
      _TheListView.SmallImageList = EFPApp.MainImages.ImageList;
      _TheGroupBox.Controls.Add(_TheListView);
      this.Resize += new EventHandler(Form_Resize);
      Form_Resize(null, null);
    }

    void Form_Resize(object sender, EventArgs args)
    {
      _TheListView.Columns[0].Width = -2;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Управляющий элемент списка
    /// </summary>
    public ListView TheListView { get { return _TheListView; } }
    private ListView _TheListView;

    private GroupBox _TheGroupBox;

    /// <summary>
    /// Заголовок на рамке, окружающей список
    /// </summary>
    public string GroupTitle
    {
      get { return _TheGroupBox.Text; }
      set { _TheGroupBox.Text = value; }
    }

    #endregion
  }
}
