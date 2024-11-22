// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Data;
using System.Data;
using System.Windows.Forms;
using FreeLibSet.Forms.Data;

namespace FreeLibSet.Forms.Docs
{
  /// <summary>
  /// Страница отчета, содержащая таблицу со списком документов одного вида.
  /// Содержит табличный просмотр <see cref="EFPDocGridView"/>.
  /// </summary>
  public class EFPReportDocGridPage : EFPReportDBxGridPage /* 06.07.2021 */
  {
    // TODO: 06.07.2021. Пока не знаю как быть со свойством DataSource. Его установка, вероятно, будет несовместима с возможностью выбора конфигурации

    #region Конструктор

    /// <summary>
    /// Создает страницу отчета
    /// </summary>
    /// <param name="docTypeUI">Интерфейс для вида документа</param>
    public EFPReportDocGridPage(DocTypeUI docTypeUI)
      : base(docTypeUI.UI)
    {
      //if (docTypeUI == null)
      //  throw new ArgumentNullException("docTypeUI");
      _DocTypeUI = docTypeUI;

      base.ShowToolBar = true;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Интерфейс для вида документа
    /// </summary>
    public DocTypeUI DocTypeUI { get { return _DocTypeUI; } }
    private readonly DocTypeUI _DocTypeUI;

    /// <summary>
    /// Альтернативный метод присоединения источника данных.
    /// Задает фиксированный список идентификаторов документов, которые можно просматривать.
    /// Свойство <see cref="EFPDocGridView.FixedDocIds"/>.
    /// </summary>
    public IdList FixedDocIds
    {
      get
      {
        return _FixedDocIds;
      }
      set
      {
        _FixedDocIds = value;
        if (ControlProvider != null)
          ControlProvider.FixedDocIds = value;
      }
    }
    private IdList _FixedDocIds;

    #endregion

    #region Табличный просмотр

    /// <summary>
    /// Провайдер табличного просмотра
    /// </summary>
    public new EFPDocGridView ControlProvider
    {
      get { return (EFPDocGridView)(base.ControlProvider); }
    }

    /// <summary>
    /// Создает <see cref="EFPDocGridView"/>
    /// </summary>
    /// <param name="control">Табличный просмотр Windows Forms</param>
    /// <returns>Провайдер управляющего элемента</returns>
    protected override EFPDataGridView DoCreateControlProvider(DataGridView control)
    {
      EFPDocGridView res = new EFPDocGridView(BaseProvider, control, DocTypeUI);
      res.Filters = new EFPDBxGridFilters();
      res.CommandItems.CanEditFilters = false; // 19.10.2015
      // 06.07.2021
      // Можно задавать порядок сортировки
      //Res.UseGridProducerOrders = false;
      //Res.CustomOrderAllowed = false; // 05.07.2021
      return res;
    }

    /// <summary>
    /// Устанавливает свойство <see cref="EFPDocGridView.FixedDocIds"/>
    /// </summary>
    protected override void InitData()
    {
      if (FixedDocIds != null)
        ControlProvider.FixedDocIds = FixedDocIds;
      base.InitData();
    }

    #endregion
  }
}
