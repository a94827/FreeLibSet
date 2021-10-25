using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Data;
using System.ComponentModel;
using System.Diagnostics;
using FreeLibSet.Core;
using FreeLibSet.Forms.Diagnostics;

/*
 * The BSD License
 * 
 * Copyright (c) 2016, Ageyev A.V.
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

/*
 * Просмотр объектов OpenOffice / LibreOffice
 * 
 * Этот модуль не входит в ExtForms.dll, т.к. требует компиляции библиотек cli_*.dll,
 * входящих в состав OpenOffice SDK
 */

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Отладочные средства взаимодействия OpenOffice / LibreOfice через CLI
  /// </summary>
  public static class OpenOfficeDebugTools
  {
    #region Основной метод

    /// <summary>
    /// Просмотр на экране UNO-объекта.
    /// Если объект не относится к UNO, то вызывается обычный отладочный метод WinForms DebugTools.DebugObject()
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="title"></param>
    public static void DebugObject(object obj, string title)
    {
      // Не используем EFP, т.к. может вызываться не из основного потока приложения, а например,
      // из обработчика события

      unoidl.com.sun.star.beans.XPropertySet ps = obj as unoidl.com.sun.star.beans.XPropertySet;

      //  Get the interface XServiceInfo because of the implementation name
      unoidl.com.sun.star.lang.XServiceInfo si = obj as unoidl.com.sun.star.lang.XServiceInfo;


      unoidl.com.sun.star.uno.XComponentContext ctx = uno.util.Bootstrap.bootstrap();
      unoidl.com.sun.star.lang.XMultiComponentFactory sm = ctx.getServiceManager();
      unoidl.com.sun.star.beans.XIntrospection intr = sm.createInstanceWithContext("com.sun.star.beans.Introspection", ctx) as unoidl.com.sun.star.beans.XIntrospection;
      unoidl.com.sun.star.beans.XIntrospectionAccess ia = null;
      try
      {
        ia = intr.inspect(new uno.Any(obj.GetType(), obj));
      }
      catch { }

      if (ps != null || si != null || ia != null)
      {
        Form frm = new Form();
        frm.Text = title;
        frm.Icon = EFPApp.MainImageIcon("Debug");
        Size sz = Screen.PrimaryScreen.Bounds.Size;
        frm.Size = new Size(sz.Width, sz.Height);
        frm.StartPosition = FormStartPosition.WindowsDefaultLocation;
        TabControl TheTabControl = new TabControl();
        if (EFPApp.IsMainThread)
          TheTabControl.ImageList = EFPApp.MainImages;
        TheTabControl.Dock = DockStyle.Fill;
        frm.Controls.Add(TheTabControl);

        unoidl.com.sun.star.beans.Property[] props = new unoidl.com.sun.star.beans.Property[0];
        if (ps != null)
        {
          unoidl.com.sun.star.beans.XPropertySetInfo psi = ps.getPropertySetInfo();
          props = psi.getProperties();
        }
        if (ia != null)
        {
          unoidl.com.sun.star.beans.Property[] props2 = ia.getProperties(unoidl.com.sun.star.beans.PropertyConcept.ALL);
          props = DataTools.MergeArrays<unoidl.com.sun.star.beans.Property>(props, props2);
        }

        if (props.Length > 0)
        {
          TabPage tpProps = new TabPage("Свойства");
          tpProps.ImageKey = "Properties";
          TheTabControl.TabPages.Add(tpProps);

          DataGridView grProps = new DataGridView();
          PrepareGrid(grProps);

          DataGridViewTextBoxColumn colPropName = new DataGridViewTextBoxColumn();
          colPropName.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
          colPropName.HeaderText = "Свойство";
          grProps.Columns.Add(colPropName);

          DataGridViewTextBoxColumn colPropValue = new DataGridViewTextBoxColumn();
          colPropValue.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
          colPropValue.HeaderText = "Значение";
          grProps.Columns.Add(colPropValue);

          DataGridViewTextBoxColumn colPropType = new DataGridViewTextBoxColumn();
          colPropType.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
          colPropType.HeaderText = "Тип";
          grProps.Columns.Add(colPropType);

          tpProps.Controls.Add(grProps);

          grProps.RowCount = props.Length;

          for (int i = 0; i < props.Length; i++)
          {
            object Value;
            string TypeText;
            GetPropValue(ps, props[i].Name, out Value, out TypeText);
            grProps[0, i].Value = props[i].Name;
            try
            {
              grProps[1, i].Value = ValueToString(Value);
            }
            catch (Exception e)
            {
              grProps[1, i].Value = "*** Ошибка *** " + e.Message;
            }
            grProps[2, i].Value = TypeText;
            grProps.Rows[i].Tag = ExpandAny(Value);
          }
          grProps.CellDoubleClick += new DataGridViewCellEventHandler(grProps_CellDoubleClick);
        }
        if (si != null)
        {
          string[] aNames = si.getSupportedServiceNames();
          TabPage tpServ = new TabPage("Сервисы");
          tpServ.ImageKey = "Table";
          TheTabControl.TabPages.Add(tpServ);

          DataGridView grServ = new DataGridView();
          PrepareGrid(grServ);

          DataGridViewTextBoxColumn colServName = new DataGridViewTextBoxColumn();
          colServName.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
          colServName.HeaderText = "Служба";
          colServName.DataPropertyName = "Служба";
          grServ.Columns.Add(colServName);

          tpServ.Controls.Add(grServ);

          DataTable TableServ = new DataTable();
          TableServ.Columns.Add("Служба", typeof(String));
          for (int i = 0; i < aNames.Length; i++)
          {
            TableServ.Rows.Add(aNames[i]);
          }
          grServ.DataSource = TableServ;
        }
        if (ia != null)
        {
          TabPage tpMethods = new TabPage("Методы");
          TheTabControl.TabPages.Add(tpMethods);

          DataGridView grMethods = new DataGridView();
          PrepareGrid(grMethods);

          DataGridViewTextBoxColumn colMethodName = new DataGridViewTextBoxColumn();
          colMethodName.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
          colMethodName.HeaderText = "Метод";
          colMethodName.DataPropertyName = "Метод";
          grMethods.Columns.Add(colMethodName);

          DataGridViewTextBoxColumn colClassName = new DataGridViewTextBoxColumn();
          colClassName.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
          colClassName.HeaderText = "Класс";
          colClassName.DataPropertyName = "Класс";
          grMethods.Columns.Add(colClassName);

          tpMethods.Controls.Add(grMethods);

          DataTable TableMethods = new DataTable();
          TableMethods.Columns.Add("Метод", typeof(String));
          TableMethods.Columns.Add("Класс", typeof(String));

          unoidl.com.sun.star.reflection.XIdlMethod[] Methods = ia.getMethods(unoidl.com.sun.star.beans.MethodConcept.ALL);

          for (int i = 0; i < Methods.Length; i++)
          {
            unoidl.com.sun.star.reflection.XIdlClass cls = Methods[i].getDeclaringClass();
            string ClassName = cls.getName();
            TableMethods.Rows.Add(Methods[i].getName(), ClassName);
          }
          grMethods.DataSource = TableMethods;
        }


        frm.ShowDialog();
        frm.Dispose();
        return;
      }

      // Обычный вызов
      DebugTools.DebugObject(obj, title);
    }

    [DebuggerStepThrough] // часто бывают исключения
    private static void GetPropValue(unoidl.com.sun.star.beans.XPropertySet ps, string propName, out object value, out string typeText)
    {
      typeText = String.Empty;
      try
      {
        if (ps == null)
          value = "[ Интерфейс XPropertySet не реализован ]";
        else
        {
          value = ps.getPropertyValue(propName);
          if (value != null)
          {
            if (value is uno.Any)
            {
              value = ((uno.Any)value).Value;
              if (value == null)
                typeText = "Any/null";
              else
                typeText = "Any/" + value.GetType().ToString();
            }
            else
              typeText = value.GetType().ToString();
          }
        }
      }
      catch (Exception e)
      {
        if (String.IsNullOrEmpty(e.Message))
          value = "[ Ошибка ] " + e.GetType().ToString();
        else
          value = "[ Ошибка ] " + e.Message;
      }
    }

    private static void PrepareGrid(DataGridView gr)
    {
      gr.Dock = DockStyle.Fill;
      gr.AutoGenerateColumns = false;
      gr.ReadOnly = true;
      gr.AllowUserToAddRows = false;
      gr.AllowUserToDeleteRows = false;
      gr.AllowUserToOrderColumns = false;
      gr.AllowUserToResizeRows = false;
    }

    static void grProps_CellDoubleClick(object sender, DataGridViewCellEventArgs args)
    {
      try
      {
        DataGridView grProps = (DataGridView)sender;
        DataGridViewRow grRow = grProps.Rows[args.RowIndex];
        object Value = grRow.Tag;
        DebugObject(Value, (string)(grRow.Cells[0].Value));
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Ошибка просмотра свойства");
      }
    }

    #endregion

    #region Методы, обрабатывающие тип Any

    /// <summary>
    /// Если переданный объект имеет тип Any, то возвращается Any.Value, иначе объект возвращается без изменений
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static object ExpandAny(object value)
    {
      if (value is uno.Any)
      {
        if (((uno.Any)value).hasValue())
          return ((uno.Any)value).Value;
        else
          return null;
      }
      else
        return value;
    }

    /// <summary>
    /// Преобразует объект в строку.
    /// Раскрывается тип Any.
    /// Для некоторых типов выводится более осмысленный текст
    /// </summary>
    /// <param name="value"></param>
    public static string ValueToString(object value)
    {
      value = ExpandAny(value);
      if (value == null)
        return "[null]";

      if (value is unoidl.com.sun.star.util.Date)
      {
        try
        {
          unoidl.com.sun.star.util.Date dt = (unoidl.com.sun.star.util.Date)value;
          if (dt.Year == 0)
            return "Empty date";
          DateTime dt2 = new DateTime(dt.Year, dt.Month, dt.Day);
          return dt2.ToString("d");
        }
        catch (Exception e)
        {
          return "Error date. " + e.Message;
        }
      }

      return value.ToString();
    }

    #endregion

    #region Специальные методы

    /// <summary>
    /// Вывод списка числовых форматов, полученных с помощтю метода GetAllNumberFormats
    /// </summary>
    /// <param name="dict"></param>
    /// <param name="title"></param>
    public static void DebugNumberFormats(IDictionary<int, OpenOffice.Calc.NumberFormatProperties> dict, string title)
    {
      DataTable Table = CreateNumberFormatsTable(dict);
      DebugTools.DebugDataTable(Table, title);
    }

    public static DataTable CreateNumberFormatsTable(IDictionary<int, OpenOffice.Calc.NumberFormatProperties> dict)
    {
      DataTable Table = new DataTable();
      Table.Columns.Add("Key", typeof(int));
      Table.Columns.Add("FormatString", typeof(string));
      Table.Columns.Add("Locale", typeof(string));
      Table.Columns.Add("LocaleVariant", typeof(string));
      Table.Columns.Add("Type", typeof(int));
      Table.Columns.Add("TypeText", typeof(string));
      Table.Columns.Add("Comment", typeof(string));

      foreach (KeyValuePair<int, OpenOffice.Calc.NumberFormatProperties> Pair in dict)
      {
        DataRow Row = Table.NewRow();
        Row["Key"] = Pair.Key;
        Row["FormatString"] = Pair.Value.FormatString;
        Row["Locale"] = Pair.Value.Locale.Language + "-" + Pair.Value.Locale.Country;
        Row["LocaleVariant"] = Pair.Value.Locale.Variant;
        Row["Type"] = Pair.Value.Type;
        string TypeText;
        int Type1 = Pair.Value.Type & (~unoidl.com.sun.star.util.NumberFormat.DEFINED);
        switch (Type1)
        {
          case unoidl.com.sun.star.util.NumberFormat.DATE: TypeText = "DATE"; break;
          case unoidl.com.sun.star.util.NumberFormat.DATETIME: TypeText = "DATETIME"; break;
          case unoidl.com.sun.star.util.NumberFormat.TIME: TypeText = "TIME"; break;
          case unoidl.com.sun.star.util.NumberFormat.NUMBER: TypeText = "NUMBER"; break;
          case unoidl.com.sun.star.util.NumberFormat.CURRENCY: TypeText = "CURRENCY"; break;
          case unoidl.com.sun.star.util.NumberFormat.FRACTION: TypeText = "FRACTION"; break;
          case unoidl.com.sun.star.util.NumberFormat.PERCENT: TypeText = "PERCENT"; break;
          case unoidl.com.sun.star.util.NumberFormat.SCIENTIFIC: TypeText = "SCIENTIFIC"; break;
          case unoidl.com.sun.star.util.NumberFormat.LOGICAL: TypeText = "LOGICAL"; break;
          case unoidl.com.sun.star.util.NumberFormat.TEXT: TypeText = "TEXT"; break;
          //case unoidl.com.sun.star.util.NumberFormat.UNDEFINED: TypeText = "UNDEFINED"; break;

          default: TypeText = Type1.ToString(); break;
        }
        if ((Pair.Value.Type & unoidl.com.sun.star.util.NumberFormat.DEFINED) != 0)
          TypeText += " DEFINED";

        Row["TypeText"] = TypeText;
        Row["Comment"] = Pair.Value.Comment;
        Table.Rows.Add(Row);
      }

      return Table;
    }

    #endregion
  }
}