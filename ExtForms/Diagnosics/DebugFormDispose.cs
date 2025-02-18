// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Core;

namespace FreeLibSet.Forms.Diagnostics
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
        throw new ObjectDisposedException("Form already disposed");
      if (Exists(form))
        throw new InvalidOperationException("Form " + form.ToString() + " already in the debug list");
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
      return "DebugFormDispose for " + _Form.ToString();
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
      throw new ArgumentException("Form has not been added", "form");
    }

    /// <summary>
    /// Свойство возвращает true, если есть незакрытые формы.
    /// Формы MDI Child, добавленные к оснвоной форме, не считаются
    /// </summary>
    public static bool HasUndisposed
    {
      get
      {
        Form[] mainChildren;
        if (EFPApp.MainWindow == null)
          mainChildren = new Form[0];
        else
          mainChildren = EFPApp.MainWindow.MdiChildren;
        for (int i = 0; i < _List.Count; i++)
        {
          if (_List[i].Persist)
            continue;
          Form frm = _List[i].Form;
          if (Array.IndexOf<Form>(mainChildren, frm) >= 0)
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
        EFPApp.MessageBox("There is no form without Dispose() called");
        return;
      }

      ObjectDebugControl.ShowDebugObject(_List.ToArray(), "Forms without Dispose() call");

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
