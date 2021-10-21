using System;
using System.Collections;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using FreeLibSet.Forms;

/*
 * The BSD License
 * 
 * Copyright (c) 2015, Ageyev A.V.
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

namespace FreeLibSet.Forms.Docs
{
  /// <summary>
  /// Одна страница редактора документа
  /// </summary>
  public class DocEditPage : IEFPTabPageControl
  {
    #region Защищенный конструктор

    internal DocEditPage(DocEditPages owner, EFPTabPage tabPage)
    {
      _Owner = owner;
      _Owner._Items.Add(this);
      //FMultiDocs = MultiDocs;
      _HasBeenShown = false;
      _TabPage = tabPage;
      //HelpContext = "BuxBase.chm::DocEdit.htm";
      //FIsActivePage = false;
      //FPage.Enter += new EventHandler(Page_Enter);
      //FPage.Leave += new EventHandler(Page_Leave);
      tabPage.Tag = this;
      tabPage.PageSelected += new EventHandler(TabPage_PageSelected);
    }

    #endregion

    #region Общие свойства

    //public DocumentEditor Editor { get { return FOwner.Editor; } }

    ///// <summary>
    ///// Ссылка на контекст справки в виде "ФайлСправки.chm::Страница.htm#Закладка"
    ///// </summary>
    //public string HelpContext
    //{
    //  get { return Editor.Form.FormProvider.GetHelpContext(FPage); }
    //  set { Editor.Form.FormProvider.SetHelpContext(FPage, value); }
    //}

    /// <summary>
    /// Была ли страница предъявлена пользователю, то есть, был ли вызов
    /// события FirstShow ?
    /// </summary>
    public bool HasBeenShown { get { return _HasBeenShown; } }
    private bool _HasBeenShown;

    ///// <summary>
    ///// Документы, для которых предназначена страница
    ///// </summary>
    //public MultiDocs MultiDocs { get { return FMultiDocs; } }
    //private MultiDocs FMultiDocs;

    /// <summary>
    /// Закладка в форме
    /// </summary>
    private EFPTabPage _TabPage;

    /// <summary>
    /// Основной управляющий элемент
    /// </summary>
    public Control MainControl
    {
      get
      {
        if (_TabPage.Control.HasChildren)
          return _TabPage.Control.Controls[0];
        else
          return null;
      }
    }

    /// <summary>
    /// Заголовок закладки
    /// </summary>
    public string Title { get { return _TabPage.Text; } set { _TabPage.Text = value; } }

    /// <summary>
    /// Значок закладки. Допускается динамическая установка значка
    /// </summary>
    public string ImageKey
    {
      get { return _TabPage.Control.ImageKey; }
      set { _TabPage.Control.ImageKey = value; }
    }

    /// <summary>
    /// Всплывающая подсказка
    /// </summary>
    public string ToolTipText
    {
      get { return _TabPage.ToolTipText; }
      set { _TabPage.ToolTipText = value; }
    }

    /// <summary>
    /// Базовый провайдер, необходимый при инициализации провайдеров управляющих элементов на странице
    /// </summary>
    public EFPBaseProvider BaseProvider { get { return _TabPage.BaseProvider; } }

    #endregion

    #region События

    /// <summary>
    /// Вызывается перед первым предъявлением закладки окна редактирования
    /// пользователю. Используется для загрузки дополнительных данных
    /// документа. Вызывается до PageShow
    /// </summary>
    public event DocEditPageEventHandler FirstShow;

    /// <summary>
    /// Вызывается перед каждым выводом закладки пользователю. Используется для
    /// обновления синхронизированных между закладками данных
    /// </summary>
    public event DocEditPageEventHandler PageShow;

    #endregion

    #region Методы

    /// <summary>
    /// Сделать эту страницу в редакторе текущей
    /// </summary>
    public void SelectPage()
    {
      _TabPage.SetFocus();
    }

    /// <summary>
    /// Возвращает true, если эта вкладка редактора является активной
    /// </summary>
    public bool IsSelectedPage
    {
      get
      {
        return _TabPage.Parent.SelectedTab == _TabPage;
      }
    }

    /// <summary>
    /// Вывод посередине панели сообщения (например, что групповое
    /// редактирование для этой страницы невозможно)
    /// </summary>
    /// <param name="message">Текст сообщения</param>
    /// <param name="panel">Пустая панель для размещения сообщения</param>
    public void SetPanelMessage(string message, Panel panel)
    {
      Label lbl = new Label();
      lbl.Dock = DockStyle.Fill;
      lbl.TextAlign = ContentAlignment.MiddleCenter;
      lbl.Text = message;
      panel.Controls.Add(lbl);
    }

    /// <summary>
    /// Возвращает Title
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return Title;
    }

    #endregion

    #region Внутренняя реализация

    private DocEditPages _Owner;

    /// <summary>
    /// Вызывается при активации закладки
    /// </summary>
    private void TabPage_PageSelected(object sender, EventArgs args)
    {
      if (!HasBeenShown)
      {
        if (FirstShow != null)
        {
          DocEditPageEventArgs ea = new DocEditPageEventArgs(this);
          try
          {
            FirstShow(this, ea);
            /*
            // 02.09.2010  Мерзкая затычка как в EFPFormProvider'е, чтобы выполнялось размещение
            // элементов на добавленной странице, когда задан масштаб изображения не 1 (не 96 dpi)
            if (FOwner.EditorForm.WindowState == FormWindowState.Normal)
            {
              FOwner.EditorForm.Width--;
              FOwner.EditorForm.Width++;
              //FOwner.Editor.Form.PerformLayout(FPage, "Bounds");
            }
             * */
          }
          catch (Exception e)
          {
            EFPApp.ShowException(e, "Ошибка при обработке события DocEditPage.FirstShow");
          }
        }
        _HasBeenShown = true;

        if (MainControl != null)
          MainControl.Visible = true; // 31.07.2012
      }
      if (PageShow != null)
      {
        DocEditPageEventArgs ea = new DocEditPageEventArgs(this);
        try
        {
          PageShow(this, ea);
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e, "Ошибка при обработке события DocEditPage.PageShow");
        }
      }
      // Событие у объекта-владельца
      _Owner.OnPageShow(this);
    }

    #endregion

    #region IEFPTabPageControl Members

    string IEFPTabPageControl.Text
    {
      get { return Title; }
      set { Title = value; }
    }

    #endregion
  }

  /// <summary>
  /// Коллекция объектов DocEditPage (свойство DocumentEditor.TheTabControl)
  /// </summary>
  public class DocEditPages : IEnumerable<DocEditPage>
  {
    #region Защищенный конструктор

    internal DocEditPages(DocEditForm editorForm)
    {
      _EditorForm = editorForm;
      _Items = new List<DocEditPage>();
    }

    #endregion

    #region Общие свойства

    internal DocEditForm EditorForm { get { return _EditorForm; } }
    private DocEditForm _EditorForm;

    #endregion

    #region Доступ к страницам

    internal List<DocEditPage> _Items; // Нужен доступ из DocEditPage

    /// <summary>
    /// Доступ к добавленной странице редактора по индексу
    /// </summary>
    /// <param name="index">Индекс страницы от 0 до (Count-1)</param>
    /// <returns>Страница редактора</returns>
    public DocEditPage this[int index]
    {
      get
      {
        return _Items[index];
      }
    }

    /// <summary>
    /// Возвращает количество добавленных страниц
    /// </summary>
    public int Count { get { return _Items.Count; } }

    /// <summary>
    /// Текущая страница в редакторе документа
    /// </summary>
    public DocEditPage SelectedPage
    {
      get
      {
        if (EditorForm.TabControlProvider.SelectedTab == null)
          return null;
        return EditorForm.TabControlProvider.SelectedTab.Tag as DocEditPage;
      }
      set
      {
        if (value == null)
          return;
        value.SelectPage();
      }
    }

    #endregion

    #region События

    /// <summary>
    /// Событие вызывается при активации страницы редактора
    /// Событие вызывается после DocEditPage.FirstPageShow и DocEditPage.PageShow, 
    /// поэтому страница уже создана на момент вызова
    /// </summary>
    public event DocEditPageEventHandler PageShow;

    internal void OnPageShow(DocEditPage page)
    {
      if (PageShow == null)
        return;
      DocEditPageEventArgs args = new DocEditPageEventArgs(page);
      try
      {
        PageShow(this, args);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка при обработке события DocEditPages.PageShow");
      }
    }

    #endregion

    #region IEnumerable<DocEditPage> Members

    /// <summary>
    /// Возвращает перечислитель по всем страницам редактора
    /// </summary>
    /// <returns>Перечислитель</returns>
    public List<DocEditPage>.Enumerator GetEnumerator()
    {
      return _Items.GetEnumerator();
    }

    /// <summary>
    /// Возвращает перечислитель по всем страницам редактора
    /// </summary>
    /// <returns>Перечислитель</returns>
    IEnumerator<DocEditPage> IEnumerable<DocEditPage>.GetEnumerator()
    {
      return _Items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return _Items.GetEnumerator();
    }

    #endregion
  }


  #region Делегаты, связанные со страницей документа

  /// <summary>
  /// Аргументы событий, связанных со страницей редактора.
  /// Объекты создаются редактором документа
  /// </summary>
  public class DocEditPageEventArgs : EventArgs
  {
    internal DocEditPageEventArgs(DocEditPage page)
    {
      _Page = page;
    }

    /// <summary>
    /// Ссылка на объект страницы редактируемого документа
    /// </summary>
    public DocEditPage Page { get { return _Page; } }
    private DocEditPage _Page;

    //public MultiDocs MultiDocs { get { return FPage.MultiDocs; } }
  }

  /// <summary>
  /// Делегат событий, связанных со страницей редактора
  /// </summary>
  /// <param name="sender">Источник события</param>
  /// <param name="args">Аргументы события</param>
  public delegate void DocEditPageEventHandler(object sender, DocEditPageEventArgs args);

  #endregion
}
