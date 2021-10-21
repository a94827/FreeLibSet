using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Базовый класс Формы для параметров отчета с использованимем расширенных
  /// возможностей
  /// Форма содержит TabControl с двумя вкладками: "Общие" и "Фильтры".
  /// Внизу формы есть панель с кнопками "ОК" и "Отмена", а также список с кнопками
  /// для выбора готовых наборов параметров.
  /// Производный класс должен добавить управляющие элементы на вкладку MainTabPage.
  /// При передаче параметров отчета в форму в переопределенном методе EFPReportExtParams.WriteFormValues() должно быть присвоение
  /// EFPReportExtParamsTwoPageForm.FiltersControlProvider.Filters = Filters.
  /// </summary>
  public partial class EFPReportExtParamsTwoPageForm : EFPReportExtParamsForm
  {
    #region Конструктор формы

    /// <summary>
    /// Создает форму и объект EFPGridFilterEditorGridView
    /// </summary>
    public EFPReportExtParamsTwoPageForm()
    {
      InitializeComponent();

      if (EFPApp.AppWasInit)
      {
        TheTabControl.ImageList = EFPApp.MainImages;
        MainTabPage.ImageKey = "Properties";
        FiltersTabPage.ImageKey = "Filter";

        EFPControlWithToolBar<DataGridView> cwt = new EFPControlWithToolBar<DataGridView>(FormProvider, FiltersTabPage);
        _FiltersControlProvider = new EFPGridFilterEditorGridView(cwt);

        FormProvider.Shown += new EventHandler(FormProvider_Shown);
        FormProvider.Hidden += new EventHandler(FormProvider_Hidden);
      }
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Провайдер табличного просмотра для таблицы редактирования фильтров.
    /// Пользовательский код должен устанавливать свойство Filters этого объекта в переопределенном методе EFPReportExtParams.WriteFormValues().
    /// </summary>
    public EFPGridFilterEditorGridView FiltersControlProvider { get { return _FiltersControlProvider; } }
    private EFPGridFilterEditorGridView _FiltersControlProvider;

    #endregion

    #region Сохранение выбранной вкладки между вызовами

    /// <summary>
    /// Ключ - класс формы
    /// Значение - Последняя выбранная вкладка
    /// </summary>
    private static Dictionary<Type, int> _LastSelectedPageIndexes = new Dictionary<Type, int>();

    void FormProvider_Shown(object sender, EventArgs args)
    {
      int PageIndex;
      if (_LastSelectedPageIndexes.TryGetValue(this.GetType(), out PageIndex))
        TheTabControl.SelectedIndex = PageIndex;
    }

    void FormProvider_Hidden(object sender, EventArgs args)
    {
      if (base.DialogResult == DialogResult.OK)
        _LastSelectedPageIndexes[this.GetType()] = TheTabControl.SelectedIndex;
    }

    #endregion
  }
}