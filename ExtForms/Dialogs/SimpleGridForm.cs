// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace FreeLibSet.Forms
{

  /// <summary>
  /// Форма, содержащая табличный просмотр и дополнительную табличку фильтров.
  /// Объект содержит FormProvider, но не провайдеры табличных просмотров.
  /// </summary>
  public class SimpleGridForm : SimpleForm<DataGridView>
  {
    #region Конструктор

    /// <summary>
    /// Создает форму.
    /// Табличка фильтров скрыта.
    /// </summary>
    public SimpleGridForm()
    {
      Icon = EFPApp.MainImageIcon("Table");
      EFPApp.SetFormSize(this, 50, 50);

      _FilterGrid = new DataGridView();
      _FilterGrid.Dock = DockStyle.Top;
      _FilterGrid.Visible = false;
      base.Controls.Add(_FilterGrid);
      _FilterGrid.TabIndex = 0;
      MainPanel.TabIndex = 1;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Табличка фильтров, располагающаяся над основным табличным просмотром и его 
    /// кнопками. По умолчанию - невидима. Для включения таблички должен быть 
    /// создан обработчик таблички фильтров
    /// </summary>
    public DataGridView FilterGrid { get { return _FilterGrid; } }
    private DataGridView _FilterGrid;

    #endregion
  }

  /// <summary>
  /// Форма, содержащая кнопки "ОК" и "Отмена", табличный просмотр и дополнительную табличку фильтров
  /// Объект содержит FormProvider, но не провайдеры табличных просмотров.
  /// </summary>
  public class OKCancelGridForm : OKCancelSimpleForm<DataGridView>
  {
    #region Конструктор

    /// <summary>
    /// Создает форму.
    /// Табличка фильтров скрыта.
    /// Эта версия конструктора не создает дополнительную рамку вокруг таблицы
    /// </summary>
    public OKCancelGridForm()
      :this(false)
    {
    }

    /// <summary>
    /// Создает форму.
    /// Табличка фильтров скрыта.
    /// </summary>
    /// <param name="useGroupBox">Если true, то будет создана дополнительная рамка вокруг элемента</param>
    public OKCancelGridForm(bool useGroupBox)
      :base(useGroupBox)
    {
      Icon = EFPApp.MainImageIcon("Table");
      EFPApp.SetFormSize(this, 50, 50);

      _FilterGrid = new DataGridView();
      _FilterGrid.Dock = DockStyle.Top;
      _FilterGrid.Visible = false;
      base.Controls.Add(_FilterGrid);
      _FilterGrid.TabIndex = 0;
      MainPanel.TabIndex = 1;
      ButtonsPanel.TabIndex = 2;
    }

    #region Свойства

    /// <summary>
    /// Табличка фильтров, располагающаяся над основным табличным просмотром и его 
    /// кнопками. По умолчанию - неивидима. Для включения таблички должен быть 
    /// создан обработчик таблички фильтров
    /// </summary>
    public DataGridView FilterGrid { get { return _FilterGrid; } }
    private DataGridView _FilterGrid;

    #endregion

    #endregion
  }
}
