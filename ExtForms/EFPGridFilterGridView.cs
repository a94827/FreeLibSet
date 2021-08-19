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


namespace AgeyevAV.ExtForms
{
  /// <summary>
  /// Маленькая табличка фильтров
  /// Содержит три колонки без заголовков: значок, название фильтра и значение.
  /// Не получает фокуса ввода, но может обрабатывать двойное нажатие мыши
  /// </summary>
  public class EFPBaseFilterGridView : EFPControl<DataGridView>
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер таблички фильтров
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент DataGridView</param>
    public EFPBaseFilterGridView(EFPBaseProvider baseProvider, DataGridView control)
      : base(baseProvider, control, false)
    {
      base.DisplayName = "FilterGrid";
      InitFilterGrid();
    }

    /// <summary>
    /// Инициализация столбцов и внешнего вида таблички фильтров
    /// </summary>
    private void InitFilterGrid()
    {
      base.ToolBarPanel = null;
      Control.AllowUserToAddRows = false;
      Control.AllowUserToDeleteRows = false;
      Control.AllowUserToOrderColumns = false;
      Control.AllowUserToResizeRows = false;
      Control.AllowUserToResizeColumns = false;
      Control.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      Control.ColumnHeadersVisible = false;
      Control.DefaultCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
      Control.ReadOnly = true;
      Control.Enabled = true;
      Control.TabStop = false;
      Control.RowHeadersVisible = false;
      Control.RowTemplate.Height = 20;

      // Отменяем выделение текущей ячейки
      Control.DefaultCellStyle.SelectionBackColor = Control.DefaultCellStyle.BackColor;
      Control.DefaultCellStyle.SelectionForeColor = Control.DefaultCellStyle.ForeColor;

      DataGridViewImageColumn col1 = new DataGridViewImageColumn();
      col1.Width = 20;
      // 06.04.2018
      // Так не работает в Mono
      // col1.Image = EFPApp.MainImages.Images["Filter"];
      col1.SortMode = DataGridViewColumnSortMode.NotSortable;
      Control.Columns.Add(col1);

      DataGridViewTextBoxColumn col2 = new DataGridViewTextBoxColumn();
      col2.Width = 200;
      col2.SortMode = DataGridViewColumnSortMode.NotSortable;
      Control.Columns.Add(col2);

      DataGridViewTextBoxColumn col3 = new DataGridViewTextBoxColumn();
      col3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
      col3.SortMode = DataGridViewColumnSortMode.NotSortable;
      Control.Columns.Add(col3);

      if (!EFPApp.ShowListImages)
        col1.Visible = false; // 14.09.2016

      Control.Click += new EventHandler(Control_Click);
      Control.CellDoubleClick += new DataGridViewCellEventHandler(FFilterGrid_CellDoubleClick);

      RowCount = 0;
      Control.Visible = true; // сами управляем
    }

    #endregion

    #region Обработчики таблички

    void Control_Click(object sender, EventArgs args)
    {
      SelectMainControl();
      // Предотвращаем активацию таблицы
    }

    private void SelectMainControl()
    {
      Control NextCtrl = Control.GetNextControl(Control, true);
      if (NextCtrl != null)
        NextCtrl.Select();
    }

    /// <summary>
    /// Двойной щелчок в таблице установленных фильров выводит окно установки фильтров
    /// </summary>
    void FFilterGrid_CellDoubleClick(object sender, DataGridViewCellEventArgs args)
    {
      try
      {
        SelectMainControl();
        OnDoubleClick(args.RowIndex);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка при установке фильтра");
      }
    }

    /// <summary>
    /// Вызывается при двойном щелчке мыши на строке фильтра
    /// </summary>
    /// <param name="filterIndex">Индекс строки фильтра</param>
    protected virtual void OnDoubleClick(int filterIndex)
    {
    }

    #endregion

    #region RowCount и установка значений

    /// <summary>
    /// Установка свойства RowCount управляет видимостью таблички и ее высотой
    /// </summary>
    public int RowCount
    {
      get { return Control.RowCount; }
      set
      {
        Control.RowCount = value;
        if (HasBeenCreated) // условие добавлено 09.07.2019
          if (ProviderState==EFPControlProviderState.Attached) // 19.08.2021
            Control.Visible = value > 0;
        if (value > 0)
          Control.Height = 4 + value * Control.RowTemplate.Height;
      }
    }

    ///// <summary>
    ///// Метод вызывается при первом появлении элемента на экране.
    ///// Устанавливает свойство Control.Visible, в зависимости от количества строк фильтров.
    ///// Если табличка не содержит ни одного фильтра, она не выводится.
    ///// </summary>
    //protected override void OnCreated()
    //{
    //  base.OnCreated();

    //  // Control.Visible = Control.RowCount > 0; // 09.07.2019
    //}

    /// <summary>
    /// Вызывает событие Attached. См. описание события.
    /// Устанавливает свойство Control.Visible, в зависимости от количества строк фильтров.
    /// </summary>
    protected override void OnAttached()
    {
      base.OnAttached();

      Control.Visible = Control.RowCount > 0; // 09.07.2019, 19.08.2021
    }

    /// <summary>
    /// Ручная установка строки таблички фильтров.
    /// Метод должен вызываться после установки свойства RowCount
    /// </summary>
    /// <param name="rowIndex">Индекс строки фильтра. Должен быть в диапазоне от 0 до RowCount-1</param>
    /// <param name="filterName">Название фильтра</param>
    /// <param name="filterValue">Текстовое представление значения фильтра</param>
    public void SetRow(int rowIndex, string filterName, string filterValue)
    {
      SetRow(rowIndex, filterName, filterValue, "Filter");
    }

    /// <summary>
    /// Ручная установка строки таблички фильтров.
    /// Метод должен вызываться после установки свойства RowCount
    /// </summary>
    /// <param name="rowIndex">Индекс строки фильтра. Должен быть в диапазоне от 0 до RowCount-1</param>
    /// <param name="filterName">Название фильтра</param>
    /// <param name="filterValue">Текстовое представление значения фильтра</param>
    /// <param name="imageKey">Имя изображения из списка EFPApp.MainImages.
    /// Если нужно задать пустое изображение, передавайте "EmptyImage"</param>
    public void SetRow(int rowIndex, string filterName, string filterValue, string imageKey)
    {
      if (rowIndex < 0 || rowIndex >= RowCount)
        throw new ArgumentOutOfRangeException("rowIndex", rowIndex, "Индекс строки должен быть в диапазоне от 0 до " + (RowCount - 1).ToString());

      if (String.IsNullOrEmpty(imageKey))
        Control[0, rowIndex].Value = EFPApp.MainImages.Images["Filter"];
      else
        Control[0, rowIndex].Value = EFPApp.MainImages.Images[imageKey];
      Control[1, rowIndex].Value = filterName;
      Control[2, rowIndex].Value = filterValue;
    }

    /// <summary>
    /// Получение строки таблички фильтров.
    /// Метод может вызываться после установки свойства RowCount
    /// </summary>
    /// <param name="rowIndex">Индекс строки фильтра. Должен быть в диапазоне от 0 до RowCount-1</param>
    /// <param name="filterName">Сюда помещается название фильтра</param>
    /// <param name="filterValue">Сюда помещается текстовое представление значения фильтра</param>
    public void GetRow(int rowIndex, out string filterName, out string filterValue)
    {
      if (rowIndex < 0 || rowIndex >= RowCount)
        throw new ArgumentOutOfRangeException("rowIndex", rowIndex, "Индекс строки должен быть в диапазоне от 0 до " + (RowCount - 1).ToString());

      filterName = DataTools.GetString(Control[1, rowIndex].Value);
      filterValue = DataTools.GetString(Control[2, rowIndex].Value);
    }

    #endregion
  }

  /// <summary>
  /// Маленькая табличка фильтров для EFPConfigurableDataGridView
  /// Содержит три колонки без заголовков: значок, название фильтра и значение.
  /// Не получает фокуса ввода, но может обрабатывать двойное нажатие мыши.
  /// Не путать с таблицей для редактирования списка фильтров EFPGridFilterEditorGridView
  /// </summary>
  public class EFPGridFilterGridView : EFPBaseFilterGridView
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер
    /// </summary>
    /// <param name="mainControlProvider">Провайдер основного табличного просмотра</param>
    /// <param name="filterGridControl">Управляющий элемент таблички фильтров</param>
    public EFPGridFilterGridView(EFPConfigurableDataGridView mainControlProvider, DataGridView filterGridControl)
      //: base(new EFPBaseProvider(), filterGridControl) // нельзя использовать родительский BaseProvider
      : base(mainControlProvider.BaseProvider, filterGridControl) // 09.06.2021. Почему нельзя? Вроде бы, можно
    {
      _MainControlProvider = mainControlProvider;
      _MainControlProvider.RefreshData += new EventHandler(MainControlProvider_RefreshData);
      _MainControlProvider.AfterSetFilter += new EventHandler(Filters_Changed);
      RefreshFilterGrid();
    }

    //private static EFPBaseProvider GetBaseProvider(EFPConfigurableDataGridView MainControlProvider)
    //{
    //  if (MainControlProvider.BaseProvider is IEFPControlWithToolBar<DataGridView>)
    //    return ((IEFPControlWithToolBar<DataGridView>)(MainControlProvider.BaseProvider)).BaseProvider;
    //  else
    //    return MainControlProvider.BaseProvider;
    //}

    #endregion

    #region Свойства

    /// <summary>
    /// Основной табличный просмотр, содержащий настраиваемые фильтры
    /// </summary>
    public EFPConfigurableDataGridView MainControlProvider { get { return _MainControlProvider; } }
    private EFPConfigurableDataGridView _MainControlProvider;

    #endregion

    #region Заполнение таблички

    void MainControlProvider_RefreshData(object sender, EventArgs args)
    {
      RefreshFilterGrid();
    }

    void Filters_Changed(object sender, EventArgs args)
    {
      RefreshFilterGrid();
    }

    /// <summary>
    /// Инициализация информации о фильтрах
    /// </summary>
    private void RefreshFilterGrid()
    {
      int cnt = 0;
      if (MainControlProvider.HasFilters)
      {
        for (int i = 0; i < MainControlProvider.Filters.Count; i++)
        {
          if (!MainControlProvider.Filters[i].IsEmpty)
            cnt++;
        }
      }
      base.RowCount = cnt;

      cnt = 0;
      if (MainControlProvider.HasFilters)
      {
        for (int i = 0; i < MainControlProvider.Filters.Count; i++)
        {
          IEFPGridFilter filter = MainControlProvider.Filters[i];
          if (!filter.IsEmpty)
          {
            // 06.04.2018
            // В Mono свойство DataGridViewImageColumn.Image, видимо не работает, так как рисуется изображение по умолчанию

            string imageKey = null;
            IEFPGridFilterWithImageKey filter2 = filter as IEFPGridFilterWithImageKey;
            if (filter2 != null)
              imageKey = filter2.FilterImageKey;
            if (String.IsNullOrEmpty(imageKey))
              imageKey = EFPGridFilterTools.DefaultFilterImageKey;
            Control.Rows[cnt].Cells[0].Value = EFPApp.MainImages.Images[imageKey];

            Control.Rows[cnt].Cells[1].Value = filter.DisplayName;
            Control.Rows[cnt].Cells[2].Value = filter.FilterText;
            Control.Rows[cnt].Tag = MainControlProvider.Filters[i];
            cnt++;
          }
        }
      }
      Control.CurrentCell = null;
    }

    #endregion

    #region Редактирование фильтров

    /// <summary>
    /// Выполнен двойной щелчок по строке фильтра.
    /// Вызывает метод EFPConfigurableDataGridView.ShowFilterDialog()
    /// </summary>
    /// <param name="FilterIndex">Индекс строки фильтра</param>
    protected override void OnDoubleClick(int FilterIndex)
    {
      if (!MainControlProvider.CommandItems.CanEditFilters)
      {
        EFPApp.ShowTempMessage("Изменение установленного фильтра запрещено");
        return;
      }

      IEFPGridFilter Filter = (IEFPGridFilter)(Control.Rows[FilterIndex].Tag);
      MainControlProvider.ShowFilterDialog(Filter.Code);
    }

    #endregion
  }

  /// <summary>
  /// Маленькая табличка фильтров для EFPReport. Также может использоваться в прикладном коде.
  /// Свойство Filters является массивом элементов EFPReportFilterItem и управляет отображаемыми фильтрами
  /// Содержит три колонки без заголовков: значок, название фильтра и значение.
  /// Не получает фокуса ввода и не реагирует на сообщения мыши.
  /// </summary>
  public class EFPReportFilterGridView : EFPBaseFilterGridView
  {
    #region Конструктор

    /// <summary>
    /// Создает провайдер таблички фильтров
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="control">Управляющий элемент DataGridView</param>
    public EFPReportFilterGridView(EFPBaseProvider baseProvider, DataGridView control)
      : base(baseProvider, control)
    {
      _Filters = _EmptyFilters;
    }

    #endregion

    #region Свойство Filters

    /// <summary>
    /// Основное свойство - массив отображаемых фильтров
    /// </summary>
    public EFPReportFilterItem[] Filters
    {
      get { return _Filters; }
      set
      { 
        if (value==null)
          value =_EmptyFilters;

        _Filters=value;
        RowCount = value.Length;
        for (int i = 0; i < value.Length; i++)
          SetRow(i, value[i].DisplayName, value[i].Value, value[i].ImageKey);
      }
    }
    private EFPReportFilterItem[] _Filters;

    private static readonly EFPReportFilterItem[] _EmptyFilters = new EFPReportFilterItem[0];

    #endregion
  }
}
