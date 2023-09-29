// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.
//
// Original TreeViewAdv component from Aga.Controls.dll
// Copyright (c) 2009, Andrey Gliznetsov (a.gliznetsov@gmail.com)
// http://www.codeproject.com/Articles/14741/Advanced-TreeView-for-NET
// http://sourceforge.net/projects/treeviewadv/

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.Data;

namespace FreeLibSet.Controls.TreeViewAdvNodeControls
{
  /// <summary>
  /// Элемент иерархического просмотра, который может извлекать данные из узлов модели дерева.
  /// С помощью свойства <see cref="BindableControl.DataPropertyName"/> можно выполнять привязку к публично доступным свойстам или полям объектов,
  /// которые являются узлами дерева. Также поддерживается привязка к полям таблицы <see cref="DataTable"/>, если узлами модели являются <see cref="DataRow"/> или <see cref="DataRowView"/>.
  /// В пределах модели узлы могут иметь разные типы данных, при этом в элементе будут отображаться свойства при их наличии в конкретном объекте.
  /// Также может быть задан виртуальный режим, когда данные извлекаются с помощью пользовательских обработчиков событий.
  /// 
  /// Наследуется классом <see cref="InteractiveControl"/> для поддержки взаимодействия с пользователем.
  /// </summary>
  public abstract class BindableControl : NodeControl
  {
    #region Member adapters

#if XXX
    private struct MemberAdapter
    {
      private object _obj;
      private PropertyInfo _pi;
      private FieldInfo _fi;

      public static readonly MemberAdapter Empty = new MemberAdapter();

      public Type MemberType
      {
        get
        {
          if (_pi != null)
            return _pi.PropertyType;
          else if (_fi != null)
            return _fi.FieldType;
          else
            return null;
        }
      }

      public object Value
      {
        get
        {
          if (_pi != null && _pi.CanRead)
            return _pi.GetValue(_obj, null);
          else if (_fi != null)
            return _fi.GetValue(_obj);
          else
            return null;
        }
        set
        {
          if (_pi != null && _pi.CanWrite)
            _pi.SetValue(_obj, value, null);
          else if (_fi != null)
            _fi.SetValue(_obj, value);
        }
      }

      public MemberAdapter(object obj, PropertyInfo pi)
      {
        _obj = obj;
        _pi = pi;
        _fi = null;
      }

      public MemberAdapter(object obj, FieldInfo fi)
      {
        _obj = obj;
        _fi = fi;
        _pi = null;
      }
    }

#endif

    // 22.08.2023 Агеев А.В.
    // Реализация member adapter изменена для поддержки доступа к DataRow/DataRowView
    // - Адаптер привязывается не к экземпляру данных, а к типу. Адаптеры буферизуются в словаре.
    // - Используются классы с реализацией интерфейса, а не структура. 

    private interface IMemberAdapter
    {
      object GetValue(object obj);
      void SetValue(object obj, object value);
      Type GetMemberType(object obj);
    }

    private class PropertyMemberAdapter : IMemberAdapter
    {
      public PropertyMemberAdapter(PropertyInfo pi)
      {
        _PI = pi;
      }

      PropertyInfo _PI;

      public object GetValue(object obj)
      {
        return _PI.GetValue(obj, null);
      }

      public void SetValue(object obj, object value)
      {
        _PI.SetValue(obj, value, null);
      }

      public Type GetMemberType(object obj)
      {
        return _PI.PropertyType;
      }
    }

    private class FieldMemberAdapter : IMemberAdapter
    {
      public FieldMemberAdapter(FieldInfo fi)
      {
        _FI = fi;
      }

      FieldInfo _FI;

      public object GetValue(object obj)
      {
        return _FI.GetValue(obj);
      }

      public void SetValue(object obj, object value)
      {
        _FI.SetValue(obj, value);
      }

      public Type GetMemberType(object obj)
      {
        return _FI.FieldType;
      }
    }

    private class DataRowMemberAdapter : IMemberAdapter
    {
      public DataRowMemberAdapter(string columnName)
      {
        _ColumnName = columnName;
      }

      string _ColumnName;

      public object GetValue(object obj)
      {
        object res = ((DataRow)obj)[_ColumnName];
        if (res is DBNull)
          return null;
        else
          return res;
      }

      public void SetValue(object obj, object value)
      {
        if (value == null)
          ((DataRow)obj)[_ColumnName] = DBNull.Value;
        else
          ((DataRow)obj)[_ColumnName] = value;
      }

      public Type GetMemberType(object obj)
      {
        DataColumn col = ((DataRow)obj).Table.Columns[_ColumnName];
        return col.DataType;
      }
    }

    private class DataRowViewMemberAdapter : IMemberAdapter
    {
      public DataRowViewMemberAdapter(string columnName)
      {
        _ColumnName = columnName;
      }

      string _ColumnName;

      public object GetValue(object obj)
      {
        object res = ((DataRowView)obj).Row[_ColumnName];
        if (res is DBNull)
          return null;
        else
          return res;
      }

      public void SetValue(object obj, object value)
      {
        if (value == null)
          ((DataRowView)obj).Row[_ColumnName] = DBNull.Value;
        else
          ((DataRowView)obj).Row[_ColumnName] = value;
      }

      public Type GetMemberType(object obj)
      {
        DataColumn col = ((DataRowView)obj).Row.Table.Columns[_ColumnName];
        return col.DataType;
      }
    }

    private class EmptyMemberAdapter : IMemberAdapter
    {
      public Type GetMemberType(object obj)
      {
        return null;
      }

      public object GetValue(object obj)
      {
        return null;
      }

      public void SetValue(object obj, object value)
      {
        throw new NotImplementedException();
      }

      public static readonly EmptyMemberAdapter Empty = new EmptyMemberAdapter();
    }

    private Dictionary<Type, IMemberAdapter> _AdapterDict;

    private IMemberAdapter GetMemberAdapter(TreeNodeAdv node)
    {
      if (node.Tag != null && !string.IsNullOrEmpty(DataPropertyName))
      {
        Type type = node.Tag.GetType();
        if (_AdapterDict == null)
          _AdapterDict = new Dictionary<Type, IMemberAdapter>();
        IMemberAdapter res;
        if (!_AdapterDict.TryGetValue(type, out res))
        {
          res = DoCreateMemberAdapter(type);
          _AdapterDict.Add(type, res);
        }
        return res;
      }
      return EmptyMemberAdapter.Empty;
    }

    private IMemberAdapter DoCreateMemberAdapter(Type type)
    {
      if (type == typeof(DataRow)) // но не "is DataRow", т.к. могут быть проиводные классы типизированных строк
        return new DataRowMemberAdapter(DataPropertyName);
      if (type==typeof(DataRowView))
        return new DataRowViewMemberAdapter(DataPropertyName);

      PropertyInfo pi = type.GetProperty(DataPropertyName);
      if (pi != null)
        return new PropertyMemberAdapter(pi);
      FieldInfo fi = type.GetField(DataPropertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      if (fi != null)
        return new FieldMemberAdapter(fi);

      return EmptyMemberAdapter.Empty;
    }


    #endregion

    #region Properties

    /// <summary>
    /// Если true, то будет использован виртуальный режим работы.
    /// Данные извлекаются с помощью пользовательского обработчика события <see cref="ValueNeeded"/>, а записываться после редактирования - <see cref="ValuePushed"/>.
    /// Если false (по умолчанию), то должно быть установлено свойство <see cref="DataPropertyName"/> для привязки к полям модели.
    /// </summary>
    [DefaultValue(false), Category("Data")]
    public bool VirtualMode
    {
      get { return _virtualMode; }
      set
      {
        _virtualMode = value;
        _AdapterDict = null;
      }
    }
    private bool _virtualMode = false;

    /// <summary>
    /// Привязка к полям модели в режиме <see cref="VirtualMode"/>=false.
    /// Данные извлекаются из объектов строки модели <see cref="TreeNodeAdv.Tag"/>.
    /// Свойство Может задавать: 
    /// - Имя свойства или имя поля в объекта. Свойство/поле должно быть нестатическим и иметь модификатор public.
    /// - Имя поля таблицы данных, если объект узла является <see cref="DataRow"/> или <see cref="DataRowView"/>.
    /// В модели могут быть узлы, содержащие объекты разных типов. Данные элемента отображаются при наличии свойства/поля/столбца таблицы 
    /// в объекте конкретного узла. При отстутствии привязки значение считается равным null.
    /// </summary>
    [DefaultValue(""), Category("Data")]
    public string DataPropertyName
    {
      get { return _propertyName; }
      set
      {
        //if (_propertyName == null)
        if (value == null) // 23.04.2021 Исправил Агеев А.В.
          _propertyName = string.Empty;
        else
          _propertyName = value;
        _AdapterDict = null;
      }
    }
    private string _propertyName = "";

    /// <summary>
    /// True, если разрешен поиск по первым буквам
    /// </summary>
    [DefaultValue(false)]
    public bool IncrementalSearchEnabled
    {
      get { return _incrementalSearchEnabled; }
      set { _incrementalSearchEnabled = value; }
    }
    private bool _incrementalSearchEnabled = false;

    #endregion

    /// <summary>
    /// Возвращает значение для узла дерева
    /// </summary>
    /// <param name="node">Узел дерева</param>
    /// <returns>Значение</returns>
    public virtual object GetValue(TreeNodeAdv node)
    {
      if (VirtualMode)
      {
        NodeControlValueEventArgs args = new NodeControlValueEventArgs(node);
        OnValueNeeded(args);
        return args.Value;
      }
      else
      {
        try
        {
          return GetMemberAdapter(node).GetValue(node.Tag);
        }
        catch (TargetInvocationException ex)
        {
          if (ex.InnerException != null)
            throw new ArgumentException(ex.InnerException.Message, ex.InnerException);
          else
            throw new ArgumentException(ex.Message);
        }
      }
    }

    /// <summary>
    /// Присваивает значение узлу дерева.
    /// Может генерировать исключение если, например, при <see cref="VirtualMode"/>=false привязанное свойство недоступно для записи.
    /// </summary>
    /// <param name="node">Узел дерева</param>
    /// <param name="value">Значение</param>
    public virtual void SetValue(TreeNodeAdv node, object value)
    {
      if (VirtualMode)
      {
        NodeControlValueEventArgs args = new NodeControlValueEventArgs(node);
        args.Value = value;
        OnValuePushed(args);
      }
      else
      {
        try
        {
          IMemberAdapter ma = GetMemberAdapter(node);
          ma.SetValue(node.Tag, value);
        }
        catch (TargetInvocationException ex)
        {
          if (ex.InnerException != null)
            throw new ArgumentException(ex.InnerException.Message, ex.InnerException);
          else
            throw new ArgumentException(ex.Message);
        }
      }
    }

    /// <summary>
    /// Возращает тип данных свойства/поля/столбца таблицы, к которому выполнена привязка с помощью <see cref="DataPropertyName"/>.
    /// В режиме <see cref="VirtualMode"/>=true не используется, возвращает null.
    /// </summary>
    /// <param name="node">Узел дерева</param>
    /// <returns>Тип данных или null</returns>
    public Type GetPropertyType(TreeNodeAdv node)
    {
      return GetMemberAdapter(node).GetMemberType(node.Tag);
    }

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      if (string.IsNullOrEmpty(DataPropertyName))
        return GetType().Name;
      else
        return string.Format("{0} ({1})", GetType().Name, DataPropertyName);
    }

    /// <summary>
    /// Событие вызывается для получения значения при <see cref="VirtualMode"/>=true.
    /// </summary>
    public event EventHandler<NodeControlValueEventArgs> ValueNeeded;
    private void OnValueNeeded(NodeControlValueEventArgs args)
    {
      if (ValueNeeded != null)
        ValueNeeded(this, args);
    }

    /// <summary>
    /// Событие вызывается для записи значения при <see cref="VirtualMode"/>=true.
    /// </summary>
    public event EventHandler<NodeControlValueEventArgs> ValuePushed;
    private void OnValuePushed(NodeControlValueEventArgs args)
    {
      if (ValuePushed != null)
        ValuePushed(this, args);
    }
  }
}
