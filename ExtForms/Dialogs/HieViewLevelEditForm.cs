// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using FreeLibSet.Config;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Форма настройки уровней.
  /// Может использоваться как форма диалога или встраивать панель MainPanel в 
  /// окно параметров отчета
  /// Используется HieViewLevelConfigEditHandler
  /// </summary>
  internal partial class HieViewLevelEditForm : Form
  {
    #region Конструктор

    public HieViewLevelEditForm()
    {
      InitializeComponent();
      Icon = EFPApp.MainImageIcon("Детализация");
    }

    #endregion
  }

#if XXX

  /// <summary>
  /// Редактор уровней детализации (флажки использования и порядок уровней)
  /// с помощью табличного просмотра.
  /// Редактор используется в блоках диалога параметров иерархического отчета.
  /// В конструкторе указывается ссылка на пустую панель для размещения элементов редактора
  /// и BaseProvider.
  /// Затем устанавливается свойство HieConfig, которое ссылается на объект,
  /// содержащий текущий список уровней. При этом, в табличный просмотр добавляются строки.
  /// Когда пользователь изменяет выбранность строк или переставляет уровни,
  /// изменение немедленно вносятся в присоединенный EFPHieViewConfig.
  /// Если изменения в других частях диалога параметров отчета приводят к изменению
  /// списка доступных уровней иерархии, то устанавливаются свойства объектов
  /// HieViewLevel.Visible, Required и др. Затем должен быть вызван метод
  /// ЭТОГО ОБЪЕКТА RefreshTable() для обновления строк табличного просмотра и
  /// поля образца.
  /// 
  /// Если требуется нестандартный редактор уровней, его можно собрать из отдельных частей,
  /// используя дочерние классы компонентов
  /// </summary>
  public class EFPHieViewConfigEditor
  {
    #region Табличный просмотр основного редактора

    /// <summary>
    /// Табличный просмотр для редактирования списка уровней
    /// </summary>
    public class EFPLevelGridView : EFPDataGridView
    {
      #region Конструктор

      public EFPLevelGridView(Control ParentControl, EFPBaseProvider BaseProvider)
        : base(BaseProvider, new DataGridView())
      {
        base.Control.Dock = DockStyle.Fill;
        ParentControl.Controls.Add(base.Control);

        Panel ToolBarPanel = new Panel();
        ToolBarPanel.Dock = DockStyle.Top;
        ParentControl.Controls.Add(ToolBarPanel);

        base.Control.AutoGenerateColumns = false;
        base.Columns.AddImage();
        base.Columns.AddBool("Flag", true, "Флаг");
        base.Columns.AddTextFill("Name", true, "Название", 100, 10);
        base.Columns.AddInt("RowOrder", true, "RowOrder", 3);
        base.Columns.LastAdded.GridColumn.ReadOnly = true;
        base.DisableOrdering();
        base.Control.ColumnHeadersVisible = false;
        //MainGridHandler.MainGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        base.ReadOnly = true;
        base.CanView = false;
        base.CanInsert = false;
        base.CanDelete = false;
        base.Control.ReadOnly = false;
        base.CanMultiEdit = false;
        base.EditData += new EventHandler(MainGridHandler_EditData);

        base.MarkRowsColumnIndex = 1;
        base.UseRowImages = true;
        base.GetRowAttributes += new EFPDataGridViewRowAttributesEventHandler(MainGridHandler_GetRowAttributes);
        base.GetCellAttributes += new EFPDataGridViewCellAttributesEventHandler(MainGridHandler_GetCellAttributes);
        base.Control.DataBindingComplete += new DataGridViewBindingCompleteEventHandler(MainGrid_DataBindingComplete);
        base.Control.CellValueChanged += new DataGridViewCellEventHandler(MainGrid_CellValueChanged);

        base.CommandItems.ManualOrderColumn = "RowOrder";
        base.CommandItems.DefaultManualOrderColumn = "DefRowOrder";
        base.CommandItems.UseRowErrors = false;

        base.ToolBarPanel = ToolBarPanel;
      }

      #endregion

      #region Обработчики

      private bool OrderRuleViolated;

      void MainGridHandler_GetRowAttributes(object Sender, EFPDataGridViewRowAttributesEventArgs Args)
      {
        OrderRuleViolated = false;

        DataRow Row = Args.DataRow;
        if (Row == null)
          return;
        // !!! Не реализовано
        //if (DataTools.GetBool(Row, "RowOrderIsFixed"))
        //  Args.UserImage = EFPApp.MainImages.Images["Anchor"];
        if (DataTools.GetBool(Row, "FlagIsFixed"))
        {
          Args.Grayed = true;
          if (!DataTools.GetBool(Row, "Flag"))
            Args.AddRowError("Этот уровень детализации является обязательным");
        }

        if (!Args.DataRow.IsNull("ViolatedOrderRuleIndex"))
        {
          int RuleIndex = DataTools.GetInt(Args.DataRow, "ViolatedOrderRuleIndex");
          EFPHieViewLevel lvl1 = HieConfig.AllLevels[HieConfig.OrderRules[RuleIndex].UpperLevelIndex];
          EFPHieViewLevel lvl2 = HieConfig.AllLevels[HieConfig.OrderRules[RuleIndex].LowerLevelIndex];
          Args.AddRowWarning("Уровень \"" + lvl1.DisplayName + "\" должен идти перед уровнем \"" + lvl2.DisplayName + "\"");
          OrderRuleViolated = true;
        }
      }

      void MainGridHandler_GetCellAttributes(object Sender, EFPDataGridViewCellAttributesEventArgs Args)
      {
        DataRow Row = Args.DataRow;
        if (Row == null)
          return;
        switch (Args.ColumnIndex)
        {
          case 0:
            Args.Value = EFPApp.MainImages.Images[DataTools.GetString(Row, "ImageKey")];
            string Name = DataTools.GetString(Args.DataRow, "Name");
            string Cfg = null;
            int p = Name.IndexOf(':');
            if (p >= 0)
            {
              Cfg = Name.Substring(p + 1);
              Name = Name.Substring(0, p);
            }
            Args.ToolTipText = "Уровень: " + Name;
            if (Cfg != null)
              Args.ToolTipText += Environment.NewLine+ + "Настройка: " + Cfg;
            break;
          case 2:
            Args.IndentLevel = Args.RowIndex;
            Args.ToolTipText = DataTools.GetString(Args.DataRow, "Name");
            if (OrderRuleViolated)
              Args.ColorType = EFPDataGridViewColorType.Warning;
            break;
        }
      }

      void MainGrid_CellValueChanged(object Sender, DataGridViewCellEventArgs Args)
      {
        OnChanged();
      }

      void MainGrid_DataBindingComplete(object Sender, DataGridViewBindingCompleteEventArgs Args)
      {
        OnChanged();
      }

      void Checker_Validating(object Sender, EFPValidatingEventArgs Args)
      {
        if (FHieConfig != null)
        {
          if (FHieConfig.SelectedLevelCount == 0)
          {
            Args.SetError("Должен быть выбран хотя бы один уровень иерархии");
            return;
          }

          for (int i = 0; i < FHieConfig.AllLevels.Length; i++)
          {
            if (FHieConfig.AllLevels[i].Visible && FHieConfig.AllLevels[i].Requred)
            {
              if (!FHieConfig.GetSelected(FHieConfig.AllLevels[i].Name))
              {
                Args.SetError("Уровень \"" + FHieConfig.AllLevels[i].DisplayName + "\" должен быть выбран");
                return;
              }
            }
          }
        }
      }

      void MainGridHandler_EditData(object Sender, EventArgs Args)
      {
        if (!base.CheckSingleRow())
          return;

        int LevelIndex = DataTools.GetInt(base.CurrentDataRow, "LevelIndex");
        EFPHieViewLevel Level = HieConfig.AllLevels[LevelIndex];
        if (Level.ParamEditor == null)
        {
          EFPApp.ShowTempMessage("Этот уровень иерархии не настраивается");
          return;
        }

        if (Level.ParamEditor.PerformEdit(Level))
        {
          HieConfig.RefreshTable();
          OnChanged();
        }
      }

      #endregion

  #region Свойство HieConfig

      /// <summary>
      /// Присоединенная конфигурация уровней. Действия пользователя сразу приводят
      /// к изменению ее свойств
      /// При установке свойства выполняется присоединение источника данных
      /// HieViewLevelConfig.View к табличному просмотру
      /// </summary>
      public EFPHieViewConfig HieConfig
      {
        get { return FHieConfig; }
        set
        {
          FHieConfig = value;
          if (value == null)
          {
            base.Control.DataSource = null;
            base.ReadOnly = true;
          }
          else
          {
            base.Control.DataSource = value.View;
            base.ReadOnly = !value.HasParamEditor;
          }

          //OnChanged();
        }
      }
      private EFPHieViewConfig FHieConfig;

  #endregion

  #region Событие Changed

      /// <summary>
      /// Событие вызывается при изменении настройки
      /// </summary>
      public event EventHandler Changed;

      protected virtual void OnChanged()
      {
        if (Changed != null)
          Changed(this, EventArgs.Empty);
      }

  #endregion
    }

    #endregion

  #region Табличный просмотр примера

    /// <summary>
    /// Табличный просмотр примера
    /// </summary>
    public class EFPSampleGridView : EFPDataGridView
    {
  #region Конструктор примера

      public EFPSampleGridView(Control ParentControl, EFPBaseProvider BaseProvider)
        : base(BaseProvider, new DataGridView())
      {
        base.Control.Dock = DockStyle.Fill;
        ParentControl.Controls.Add(base.Control);

        SampleTable = new DataTable();
        SampleTable.Columns.Add("Текст", typeof(string));
        SampleTable.Columns.Add("Отступ", typeof(int));
        SampleTable.Columns.Add("Уровень", typeof(int));

        base.Control.ReadOnly = true;
        base.Control.ColumnHeadersVisible = false;
        base.Control.RowHeadersVisible = false;
        base.Control.MultiSelect = false;
        base.Control.AutoGenerateColumns = false;

        base.Columns.AddTextFill("Текст", true, String.Empty, 100, 5);
        base.GetCellAttributes += new EFPDataGridViewCellAttributesEventHandler(SampleHandler_GetCellAttributes);
        base.HideSelection = true;
        base.ReadOnly = true;
        base.CanView = false;

        // Команды меню не нужны
        // SampleHandler.SetCommandItems(null);

        base.Control.DataSource = SampleTable;

        // Обновление примера выполняем по таймеру, т.к. события, приводящие к 
        // обновлению, могут возникать много раз за одно реальное изменение
        SampleTimer = new Timer();
        SampleTimer.Interval = 100;
        SampleTimer.Tick += new EventHandler(SampleTimer_Tick);
        SampleTimer.Enabled = true;
        base.Control.Disposed += new EventHandler(SampleGrid_Disposed);
      }

  #endregion

  #region Таблица данных для просмотра

      private DataTable SampleTable;

  #endregion

  #region Свойство HieConfig

      /// <summary>
      /// Отображаемая конфигурация.
      /// Установка свойства приводит к обновлению окна примера
      /// </summary>
      public EFPHieViewConfig HieConfig
      {
        get { return FHieConfig; }
        set
        {
          FHieConfig = value;
          SampleIsDirty = true;
        }
      }
      private EFPHieViewConfig FHieConfig;

  #endregion

  #region Обработка таблчиного просмотра

      void SampleHandler_GetCellAttributes(object Sender, EFPDataGridViewCellAttributesEventArgs Args)
      {
        DataRow Row = Args.DataRow;
        if (Row == null)
          return;
        Args.IndentLevel = DataTools.GetInt(Row, "Отступ");
        int Level = DataTools.GetInt(Row, "Уровень");
        switch (Level)
        {
          case -1:
            Args.ColorType = EFPDataGridViewColorType.TotalRow;
            break;
          case -2:
            Args.ColorType = EFPDataGridViewColorType.Header;
            break;
          case 0:
            break;
          default:
            if (Level % 2 == 1)
              Args.ColorType = EFPDataGridViewColorType.Total2;
            else
              Args.ColorType = EFPDataGridViewColorType.Total1;
            break;
        }
      }

  #endregion

  #region Обработка таймера

      /// <summary>
      /// Отложенное обновление окна примера
      /// </summary>
      private bool SampleIsDirty;

      private Timer SampleTimer;

      void SampleGrid_Disposed(object Sender, EventArgs Args)
      {
        SampleTimer.Enabled = false;
        SampleTimer.Dispose();
        SampleTimer = null;
      }

      void SampleTimer_Tick(object Sender, EventArgs Args)
      {
        if (SampleIsDirty)
        {
          SampleIsDirty = false;
          SampleTable.BeginLoadData();
          try
          {
            SampleTable.Rows.Clear();
            if (HieConfig != null)
            {
              EFPHieViewLevel[] Levels = HieConfig.SelectedLevels;
              for (int i = Levels.Length - 1; i > 0; i--)
              {
                EFPHieViewLevel Level = Levels[i];
                SampleTable.Rows.Add(Level.DisplayName, Levels.Length - i - 1, -2);
              }
              if (Levels.Length > 0)
                SampleTable.Rows.Add(Levels[0].DisplayName, Levels.Length - 1, 0);
              for (int i = 1; i < Levels.Length; i++)
              {
                EFPHieViewLevel Level = Levels[i];
                SampleTable.Rows.Add("Итого: " + Level.DisplayName, Levels.Length - i, i);
              }
              SampleTable.Rows.Add("Итого", 0, -1);
            }
          }
          finally
          {
            SampleTable.EndLoadData();
          }
          base.Control.Invalidate();

          if (FHieConfig != null)
          {
            FHieConfig.CheckOrderRules();
            base.Control.Invalidate();
          }

          SampleIsDirty = false; // еще раз, т.к. испорчено CheckOrderRules
        } // SampleIsDirty
      }

  #endregion
    }

  #endregion

  #region Обработчик закладки TabControl

    /// <summary>
    /// Обработчик объекта TabPage для инициализации значка и сообщения об ошибке
    /// </summary>
    public class TabPageController
    {
  #region Конструктор

      public TabPageController(TabPage TabPage)
      {
        if (TabPage == null)
          throw new ArgumentNullException("TabPage");
        FTabPage = TabPage;
      }

      public TabPage TabPage { get { return FTabPage; } }
      private TabPage FTabPage;

  #endregion

  #region Свойство HieConfig

      /// <summary>
      /// Установка свойства приводит к обновлению текста закладки
      /// </summary>
      public EFPHieViewConfig HieConfig
      {
        get { return FHieConfig; }
        set
        {
          FHieConfig = value;

          if (FTabPage.Parent != null)
            ((TabControl)(TabPage.Parent)).ShowToolTips = true;

          if (FHieConfig == null)
          {
            TabPage.ToolTipText = "Уровни не присоединены";
            TabPage.ImageKey = "UnknownState";
          }
          else
          {
            if (HieConfig.SelectedLevelCount == 0)
            {
              TabPage.ToolTipText = "Уровни не выбраны";
              TabPage.ImageKey = "Error";
            }
            else if (HieConfig.UnselectedRequiredLevels.Length > 0)
            {
              if (HieConfig.UnselectedRequiredLevels.Length > 1)
                TabPage.ToolTipText = "Не выбраны обязательные уровни: " + HieConfig.UnselectedRequiredLevelsTitle;
              else
                TabPage.ToolTipText = "Не выбран обязательный уровень: " + HieConfig.UnselectedRequiredLevelsTitle;
              TabPage.ImageKey = "Error";
            }
            else
            {
              TabPage.ToolTipText = "Выбранные уровни: " + HieConfig.SelectedLevelsTitle;
              if (HieConfig.OrderRules.HasViolation)
              {
                TabPage.ImageKey = "Warning";
                TabPage.ToolTipText += Environment.NewLine+"Неправильный порядок уровней";
              }
              else
                TabPage.ImageKey = "Детализация";
            }
          }

        }
      }
      private EFPHieViewConfig FHieConfig;

  #endregion
    }

  #endregion

  #region Конструктор

    /// <summary>
    /// Размещение на панели табличного просмотра с панелью инструментов и
    /// CheckBox'а с параметрами
    /// </summary>
    /// <param name="ParentControl"></param>
    public EFPHieViewConfigEditor(Control ParentControl, EFPBaseProvider BaseProvider)
    {
      HieViewLevelEditForm Form = new HieViewLevelEditForm();
      ParentControl.Controls.Add(Form.MainPanel);
      Form.Dispose();

      DoInit(Form, BaseProvider);

      if (ParentControl is TabPage)
        TPController = new TabPageController((TabPage)ParentControl);
    }

    private EFPHieViewConfigEditor(HieViewLevelEditForm Form)
    {
      EFPFormProvider efpForm = new EFPFormProvider(Form);
      DoInit(Form, efpForm);
    }

    private void DoInit(HieViewLevelEditForm Form, EFPBaseProvider BaseProvider)
    {
      efpMainGridHandler = new EFPLevelGridView(Form.grpMain, BaseProvider);

      efpHideExtraSumRows = new EFPCheckBox(BaseProvider, Form.cbHideExtraSumRows);
      efpHideExtraSumRows.ToolTipText = "Если флажок установлен, то для уровней иерархии, содержащих единственную строку данных не будет выводиться итоговая строка." +Environment.NewLine+
        "Если флажок снят, то строка промежуточных итогов будет выводиться всегда";
      efpHideExtraSumRows.CheckedEx.ValueChanged += new EventHandler(efpHideExtraSumRows_CheckedChanged);



      efpSample = new EFPSampleGridView(Form.grpSample, BaseProvider);

      // Запрет закрытия формы, если не выбрано ни одного уровня
      EFPFormCheck Checker = new EFPFormCheck();
      Checker.Validating += new EFPValidatingEventHandler(Checker_Validating);
      Checker.FocusControl = efpMainGridHandler.Control;
      BaseProvider.Add(Checker);

      efpMainGridHandler.Changed += new EventHandler(efpMainGridHandler_Changed);
    }

  #endregion

  #region Элементы управления

    /// <summary>
    /// Основной обработчик табличного просмотра (создается в конструкторе)
    /// </summary>
    private EFPLevelGridView efpMainGridHandler;

    private EFPCheckBox efpHideExtraSumRows;

    private EFPSampleGridView efpSample;

    private TabPageController TPController;

  #endregion

  #region Редактируемая конфигурация

    /// <summary>
    /// Присоединенная конфигурация уровней. Действия пользователя сразу приводят
    /// к изменению ее свойств
    /// При установке свойства выполняется присоединение источника данных
    /// HieViewLevelConfig.View к табличному просмотру
    /// </summary>
    public EFPHieViewConfig HieConfig
    {
      get { return efpMainGridHandler.HieConfig; }
      set
      {
        efpMainGridHandler.HieConfig = value;
        if (value == null)
        {
          efpHideExtraSumRows.Enabled = false;
        }
        else
        {
          efpHideExtraSumRows.Checked = value.HideExtraSumRows;
          efpHideExtraSumRows.Enabled = !value.HideExtraSumRowsFixed;
        }
        efpSample.HieConfig = value;
        if (TabPageController != null)
          TabPageController.HieConfig = value;
      }
    }

  #endregion


  #region Обработчики

    void efpHideExtraSumRows_CheckedChanged(object Sender, EventArgs Args)
    {
      if (HieConfig != null)
        HieConfig.HideExtraSumRows = efpHideExtraSumRows.Checked;
    }

    void efpMainGridHandler_Changed(object Sender, EventArgs Args)
    {
      efpSample.HieConfig = value;
      if (TabPageController != null)
        TabPageController.HieConfig = value;
    }


  #endregion

  #region Статический метод редактирования - открытие диалога

    public static bool PerformEdit(EFPHieViewConfig HieConfig)
    {
      HieViewLevelEditForm Form = new HieViewLevelEditForm();
      //EFPFormProvider efpForm = new EFPFormProvider(Form);

      // Подбираем высоту строки
      int RH = Form.MainGrid.Font.Height + 9;
      int WantedGridH = RH * HieConfig.AllLevels.Length;
      int IncH = WantedGridH - Form.MainGrid.ClientSize.Height;
      if (IncH > 0)
        Form.Height = Math.Min(Form.Height + IncH, Screen.FromControl(Form).WorkingArea.Height);

      EFPHieViewConfigEditor Handler = new EFPHieViewConfigEditor(Form);
      Handler.HieConfig = HieConfig;
      return EFPApp.ShowDialog(Form, true) == DialogResult.OK;
    }

  #endregion
  }
#endif
}