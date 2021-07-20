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
#if DEBUG

  /// <summary>
  /// Отладка вызова для каждой открытой формы метода Dispose()
  /// Для каждой создаваемой формы создается парный объект DebugFormDispose,
  /// который отслеживает вызов Dispose
  /// </summary>
  public class DebugFormDispose : SimpleDisposableObject
  {
    #region Конструктор

    /// <summary>
    /// Создает ссылку на форму
    /// </summary>
    /// <param name="form">Форма</param>
    public DebugFormDispose(Form form)
    {
      if (form == null)
        throw new ArgumentNullException("form");
      if (form.IsDisposed)
        throw new ObjectDisposedException("Форма уже разрушена");
      if (Exists(form))
        throw new InvalidOperationException("Повторное добавление формы " + form.ToString() + " в список для отладки");
      _Form = form;
      _Form.Disposed += new EventHandler(Form_Disposed);
      _StackTrace = Environment.StackTrace;
      _List.Add(this);
    }

    void Form_Disposed(object sender, EventArgs args)
    {
      this.Dispose();
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Ссылка на форму
    /// </summary>
    public Form Form { get { return _Form; } }
    private readonly Form _Form;

    /// <summary>
    /// Возвращает true, если форма объявлена как "устойчивая", то есть которая
    /// может не разрушаться до завершения работы клиента
    /// </summary>
    public bool Persist { get { return _Persist; } }
    private bool _Persist;

    /// <summary>
    /// Стек вызовов на момент инициализации формы
    /// </summary>
    public string StackTrace { get { return _StackTrace; } }
    private readonly string _StackTrace;

    #endregion

    #region Переопределенные методы

    /// <summary>
    /// 
    /// </summary>
    /// <param name="disposing"></param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
        _List.Remove(this);
      base.Dispose(disposing);
    }

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return "DebugFormDispose для " + _Form.ToString();
    }

    #endregion

    #region Статический список форм

    private static readonly List<DebugFormDispose> _List = new List<DebugFormDispose>();

    /// <summary>
    /// Проверить, была ли форма зарегистрирована в списке и еще не разрушена
    /// </summary>
    /// <param name="form">Проверяемая форма</param>
    /// <returns>Наличие в списке</returns>
    public static bool Exists(Form form)
    {
      if (form == null)
        throw new ArgumentNullException("form");
      for (int i = 0; i < _List.Count; i++)
      {
        if (_List[i].Form == form)
          return true;
      }
      return false;
    }

    /// <summary>
    /// Добавление формы в список для отладки, если ее там нет
    /// </summary>
    /// <param name="form"></param>
    public static void Add(Form form)
    {
      //if (!Exists(FForm))
      new DebugFormDispose(form);
    }

    /// <summary>
    /// Объявить форму "устойчивой".
    /// Для такой формы не будет выдаваться предупреждение при закрытии 
    /// приложения клиента
    /// </summary>
    /// <param name="form"></param>
    public static void SetPersist(Form form)
    {
      if (form == null)
        throw new ArgumentNullException("form");
      for (int i = 0; i < _List.Count; i++)
      {
        if (_List[i].Form == form)
        {
          _List[i]._Persist = true;
          return;
        }
      }
      throw new ArgumentException("Форма не была добавлена");
    }

    /// <summary>
    /// Свойство возвращает true, если есть незакрытые формы.
    /// Формы MDI Child, добавленные к оснвоной форме, не считаются
    /// </summary>
    public static bool HasUndisposed
    {
      get
      {
        Form[] MainChildren;
        if (EFPApp.MainWindow == null)
          MainChildren = new Form[0];
        else
          MainChildren = EFPApp.MainWindow.MdiChildren;
        for (int i = 0; i < _List.Count; i++)
        {
          if (_List[i].Persist)
            continue;
          Form frm = _List[i].Form;
          if (Array.IndexOf<Form>(MainChildren, frm) >= 0)
            continue;
          return true;
        }
        return false;
      }
    }

    #endregion

    #region Вывод списка открытых форм

    /// <summary>
    /// Вывод списка открытых форм
    /// </summary>
    public static void ShowList()
    {
      if (_List.Count == 0)
      {
        EFPApp.MessageBox("Нет форм, для которых не было вызова Dispose()");
        return;
      }

      ObjectDebugControl.ShowDebugObject(_List.ToArray(), "Формы, для которых не было вызова Dispose()");

      //string[] Items = new string[List.Count];
      //for (int i = 0; i < Items.Length; i++)
      //  Items[i] = List[i].TheForm.ToString()+". Стек вызовов: "+List[i].StackTrace;
      ////DebugFormDispose[] Items = List.ToArray();

      //DebugTools.DebugObject(Items, "Формы, для которых не было вызова Dispose()");

      //DataGridView Grid = new DataGridView();
      //Grid.ReadOnly = true;
      //Grid.AllowUserToAddRows = false;
      //Grid.AllowUserToDeleteRows = false;
    }

    #endregion

  }

#endif
}
