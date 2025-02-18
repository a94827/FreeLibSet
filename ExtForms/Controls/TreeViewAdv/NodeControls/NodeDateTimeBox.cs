using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Core;
using FreeLibSet.Formatting;

namespace FreeLibSet.Controls.TreeViewAdvNodeControls
{
  /// <summary>
  /// Реализация NodeControl для редактирования полей даты и/или времени с помощью <see cref="DateTimeBox"/> 
  /// </summary>
  public class NodeDateTimeBox : BaseFormattedTextControl
  {
    #region Конструктор

    /// <summary>
    /// Создает узел со значениями свойств по умолчанию
    /// </summary>
    public NodeDateTimeBox()
    {
      Kind = EditableDateTimeFormatterKind.Date;
      TextAlign = HorizontalAlignment.Center;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Формат даты/времени.
    /// По умолчанию - <see cref="EditableDateTimeFormatterKind.Date"/>
    /// </summary>
    public EditableDateTimeFormatterKind Kind
    {
      get { return _Formatter.Kind; }
      set { Formatter = EditableDateTimeFormatters.Get(value); }
    }

    /// <summary>
    /// Форматизатор вывода даты и/или времени
    /// </summary>
    public EditableDateTimeFormatter Formatter
    {
      get { return _Formatter; }
      set { _Formatter = value; }
    }
    private EditableDateTimeFormatter _Formatter;


    /// <summary>
    /// Год по умолчанию. Если задано ненулевое значение, то пользователь может ввести только день и месяц, после чего дата будет считаться введенной"
    /// </summary>
    public int DefaultYear
    {
      get { return _DefaultYear; }
      set
      {
        if (value < 0 || value > DateTime.MaxValue.Year)
          throw ExceptionFactory.ArgOutOfRange("value", value, 0, DateTime.MaxValue.Year);
        _DefaultYear = value;
      }
    }
    private int _DefaultYear;

    #endregion

    #region Переопределенные методы и свойства

    /// <summary>
    /// Возвращает формат.
    /// Установка свойства не допускается.
    /// </summary>
    public override string Format
    {
      get { return Formatter.Format; }
      set
      {
        throw new InvalidOperationException("Property cannot be set. Use \"Formatter\" property");
      }
    }

    /// <summary>
    /// Возвращает <see cref="IFormatProvider"/>.
    /// Установка свойства не допускается.
    /// </summary>
    public override IFormatProvider FormatProvider
    {
      get { return Formatter.FormatProvider; }
      set
      {
        throw new InvalidOperationException("Property cannot be set. Use \"Formatter\" property");
      }
    }

    /// <summary>
    /// Создает <see cref="DateTimeBox"/>
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    protected override Control CreateEditor(TreeNodeAdv node)
    {
      DateTimeBox control = new DateTimeBox();
      control.Kind = Kind;
      control.DefaultYear = DefaultYear;
      control.NValue = (DateTime?)GetValue(node);
      SetEditControlProperties(control, node);
      return control;
    }

    /// <summary>
    /// Ничего не делает
    /// </summary>
    /// <param name="editor"></param>
    protected override void DisposeEditor(Control editor)
    {
    }

    /// <summary>
    /// Возвращает размер для редактора значения
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    protected override Size CalculateEditorSize(TreeViewAdvEditorContext context)
    {
      if (Parent.UseColumns)
        return context.Bounds.Size;
      else
      {
        string s = Formatter.ToString(DateTime.MaxValue);
        int w = (int)context.DrawContext.Graphics.MeasureString(s, Parent.Font).Width + 2 * SystemInformation.BorderSize.Width;
        return new Size(w, context.Bounds.Height);
      }
    }

    /// <summary>
    /// Записывает отредактированное значение
    /// </summary>
    /// <param name="node"></param>
    /// <param name="editor"></param>
    protected override void DoApplyChanges(TreeNodeAdv node, Control editor)
    {
      SetValue(node, (editor as FreeLibSet.Controls.DateTimeBox).NValue);
    }

    #endregion
  }
}
