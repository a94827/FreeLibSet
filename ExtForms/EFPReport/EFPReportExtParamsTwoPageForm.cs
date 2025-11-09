// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

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
  /// возможностей.
  /// Форма содержит <see cref="TabControl"/> с двумя вкладками: "Общие" и "Фильтры".
  /// Внизу формы есть панель с кнопками "ОК" и "Отмена", а также список с кнопками
  /// для выбора готовых наборов параметров.
  /// Производный класс должен добавить управляющие элементы на вкладку <see cref="MainTabPage"/>.
  /// При передаче параметров отчета в форму в переопределенном методе <see cref="EFPReportExtParams.WriteFormValues(EFPReportExtParamsForm, Config.SettingsPart)"/> должно быть присвоение
  /// <see cref="EFPReportExtParamsTwoPageForm.FiltersControlProvider"/>.Filters = filters.
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

      if (EFPApp.AppHasBeenInit)
      {
        TheTabControl.ImageList = EFPApp.MainImages.ImageList;
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
    /// Пользовательский код должен устанавливать свойство Filters этого объекта в переопределенном методе <see cref="EFPReportExtParams.WriteFormValues(EFPReportExtParamsForm, Config.SettingsPart)"/>.
    /// </summary>
    public EFPGridFilterEditorGridView FiltersControlProvider { get { return _FiltersControlProvider; } }
    private readonly EFPGridFilterEditorGridView _FiltersControlProvider;

    #endregion

    #region Сохранение выбранной вкладки между вызовами

    /// <summary>
    /// Ключ - класс формы.
    /// Значение - Последняя выбранная вкладка.
    /// </summary>
    private static readonly Dictionary<Type, int> _LastSelectedPageIndices = new Dictionary<Type, int>();

    void FormProvider_Shown(object sender, EventArgs args)
    {
      int pageIndex;
      if (_LastSelectedPageIndices.TryGetValue(this.GetType(), out pageIndex))
        TheTabControl.SelectedIndex = pageIndex;
    }

    void FormProvider_Hidden(object sender, EventArgs args)
    {
      if (base.DialogResult == DialogResult.OK)
        _LastSelectedPageIndices[this.GetType()] = TheTabControl.SelectedIndex;
    }

    #endregion
  }
}
