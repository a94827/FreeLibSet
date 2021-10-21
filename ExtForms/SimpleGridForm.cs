using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

/*
 * The BSD License
 * 
 * Copyright (c) 2006-2015, Ageyev A.V.
 * 
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice, 
 * this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice, 
 * this list of conditions and the following disclaimer in the documentation 
 * and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE 
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; 
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

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
