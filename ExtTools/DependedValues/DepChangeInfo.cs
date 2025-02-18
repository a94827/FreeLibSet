// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Core;

namespace FreeLibSet.DependedValues
{
  /// <summary>
  /// Базовый класс для DepChangeInfoItem и DepChangeInfoList
  /// Содержит свойство Changed только для чтения
  /// </summary>
  public abstract class DepChangeInfo
  {
    #region Конструктор

    /// <summary>
    /// Защищенный конструктор
    /// </summary>
    protected DepChangeInfo()
    {
      _Parent = null;
      _Changed = false;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Возвращает true, если есть изменения
    /// </summary>
    public bool Changed { get { return _Changed; } }
    private bool _Changed;

    /// <summary>
    /// Устанавливает свойство Changed и вызывает событие ChangedChanged, если значение свойства изменилось.
    /// </summary>
    /// <param name="value">Новое значение свойства</param>
    protected void SetChanged(bool value)
    {
      if (value == _Changed)
        return;
      _Changed = value;

      OnChangedChanged(EventArgs.Empty);

      // Сообщаем родительскому списку об изменениях
      if (_Parent != null)
      {
        if (value)
          _Parent.IncChanges();
        else
          _Parent.DecChanges();
      }
    }

    /// <summary>
    /// Родительский список
    /// </summary>
    public DepChangeInfoList Parent
    {
      get { return _Parent; }
      set
      {
        if (value == _Parent)
          return; // Нет изменений

        if (_Parent != null)
        {
          _Parent.Items.Remove(this);
          if (_Changed)
            _Parent.DecChanges();
        }

        _Parent = value;

        if (_Parent != null)
        {
          // Проверка зацикливания цепочки
          DepChangeInfoList xx = _Parent;
          while (xx != null)
          {
            if (xx == this)
            {
              _Parent = null;
              throw new InvalidOperationException(String.Format(Res.DepChangeInfo_Err_ChainLoop, this.DisplayName, value.DisplayName));
            }
            xx = xx.Parent;
          }


          _Parent.Items.Add(this);
          if (_Changed)
            _Parent.IncChanges();
        }
      }
    }
    private DepChangeInfoList _Parent;

    /// <summary>
    /// Название позиции (для отладки)
    /// </summary>
    public virtual string DisplayName
    {
      get
      {
        if (String.IsNullOrEmpty(_DisplayName))
          return GetType().ToString();
        else
          return _DisplayName;
      }
      set
      {
        _DisplayName = value;
      }
    }
    private string _DisplayName;

    /// <summary>
    /// Свойство возвращает false для DepChangeInfoItem. Для списка возвращается true при Count=0
    /// </summary>
    public abstract bool IsEmpty { get; }

    /// <summary>
    /// Возвращает DisplayName
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return DisplayName;
    }

    #endregion

    #region События

    /// <summary>
    /// Событие вызывается, когда меняется значение свойства Changed.
    /// </summary>
    public event EventHandler ChangedChanged;

    /// <summary>
    /// Метод вызывается, когда меняется значение свойства Changed.
    /// Непереопределенный метод вызывает событие ChangedChanged.
    /// </summary>
    /// <param name="args">Фиктивный аргумент</param>
    protected virtual void OnChangedChanged(EventArgs args)
    {
      if (ChangedChanged != null)
        ChangedChanged(this, args);
    }

    #endregion
  }

  /// <summary>
  /// Элемент для отслеживания одиночного изменения
  /// </summary>
  public class DepChangeInfoItem : DepChangeInfo
  {
    #region Свойства

    /// <summary>
    /// Наличие изменений для данной позиции. Изменение значения свойства в True 
    /// вызывает ParentControl.IncChanges, а в False - DecChanges
    /// </summary>
    public new bool Changed
    {
      get { return base.Changed; }
      set { SetChanged(value); }
    }

    /// <summary>
    /// Всегда возвращается false
    /// </summary>
    public override bool IsEmpty { get { return false; } }

    #endregion
  }

  /// <summary>
  /// Элемент для отслеживания изменений в значении
  /// Хранит исходное и текущее значения (для целей отладки)
  /// </summary>
  public class DepChangeInfoValueItem : DepChangeInfoItem
  {
    #region Значение

    /// <summary>
    /// Исходное значение
    /// </summary>
    public object OriginalValue { get { return _OriginalValue; } set { _OriginalValue = value; } }
    private object _OriginalValue;

    /// <summary>
    /// Текущее значение
    /// </summary>
    public object CurrentValue { get { return _CurrentValue; }set { _CurrentValue = value; } }
    private object _CurrentValue;

    #endregion
  }

  /// <summary>
  /// Список для отслеживания изменений.
  /// Может содержать элементы DepChangeInfoItem и вложенные списки (дерево)
  /// Свойство Changed возвращает true, если есть изменения хотя бы в одном из дочерних элементов
  /// </summary>
  public class DepChangeInfoList : DepChangeInfo, ICollection<DepChangeInfo>
  {
    #region Конструктор

    /// <summary>
    /// Создает пустой список
    /// </summary>
    public DepChangeInfoList()
    {
      _Items = new List<DepChangeInfo>();
      _ChangeCount = 0;
    }

    #endregion

    #region Методы

    /// <summary>
    /// Сброс всех изменений
    /// </summary>
    public void ResetChanges()
    {
      for (int i = 0; i < _Items.Count; i++)
      {
        DepChangeInfo item = _Items[i];
        if (item is DepChangeInfoItem)
          ((DepChangeInfoItem)item).Changed = false;
        else if (item is DepChangeInfoList)
          ((DepChangeInfoList)item).ResetChanges();
      }
    }

    /// <summary>
    /// Копирует все дочерние элементы в массив
    /// </summary>
    /// <returns>Массив дочерних элементов</returns>
    public DepChangeInfo[] ToArray()
    {
      return _Items.ToArray();
    }

    #endregion

    #region Внутренняя реализация

    internal List<DepChangeInfo> Items { get { return _Items; } }
    private readonly List<DepChangeInfo> _Items;

    private int _ChangeCount;

    internal void IncChanges()
    {
      _ChangeCount++;
      SetChanged(true);
    }
    internal void DecChanges()
    {
      _ChangeCount--;
      SetChanged(_ChangeCount > 0);
    }


    /// <summary>
    /// Возвращает true, если Count=0
    /// </summary>
    public override bool IsEmpty
    {
      get { return _Items.Count == 0; }
    }

    #endregion

    #region IEnumerator<DepChangeInfoBase>

    /// <summary>
    /// Возвращает перечислитель по дочерним элементам
    /// 
    /// Тип возвращаемого значения может измениться в будущем, 
    /// гарантируется только реализация интерфейса перечислителя.
    /// Поэтому в прикладном коде метод должен использоваться исключительно для использования в операторе foreach.
    /// </summary>
    /// <returns>Перечислитель</returns>
    public List<DepChangeInfo>.Enumerator GetEnumerator()
    {
      return _Items.GetEnumerator();
    }

    IEnumerator<DepChangeInfo> IEnumerable<DepChangeInfo>.GetEnumerator()
    {
      return _Items.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return _Items.GetEnumerator();
    }

    #endregion

    #region ICollection<DepChangeInfo> Members

    /// <summary>
    /// Добавляет элемент в список
    /// </summary>
    /// <param name="item">Добавляемый элемент</param>
    public void Add(DepChangeInfo item)
    {
      item.Parent = this;
    }

    /// <summary>
    /// Очищает список
    /// </summary>
    public void Clear()
    {
      DepChangeInfo[] a = ToArray();
      for (int i = a.Length - 1; i >= 0; i--)
        a[i].Parent = null;
    }

    /// <summary>
    /// Возвращает true, если элемент есть в списке
    /// </summary>
    /// <param name="item">Проверяемый элемент</param>
    /// <returns>Наличие в списке</returns>
    public bool Contains(DepChangeInfo item)
    {
      return _Items.Contains(item);
    }

    /// <summary>
    /// Копирует список в массив
    /// </summary>
    /// <param name="array">Массив для заполнения</param>
    public void CopyTo(DepChangeInfo[] array)
    {
      _Items.CopyTo(array);
    }

    /// <summary>
    /// Копирует список в массив
    /// </summary>
    /// <param name="array">Массив для заполнения</param>
    /// <param name="arrayIndex">Индекс в заполняемом массиве</param>
    public void CopyTo(DepChangeInfo[] array, int arrayIndex)
    {
      _Items.CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Копирует список в массив
    /// </summary>
    /// <param name="index">Индекс первого элемента в текущем списке, с которого начинать копирование</param>
    /// <param name="array">Заполняемый массив</param>
    /// <param name="arrayIndex">Начальный индекс в массиве для заполнения</param>
    /// <param name="count">Количество элементов, которые нужно скопировать</param>
    public void CopyTo(int index, DepChangeInfo[] array, int arrayIndex, int count)
    {
      _Items.CopyTo(index, array, arrayIndex, count);
    }

    /// <summary>
    /// Возвращает количество элементов в списке
    /// </summary>
    public int Count { get { return _Items.Count; } }

    bool ICollection<DepChangeInfo>.IsReadOnly { get { return false; } }

    /// <summary>
    /// Удаляет элемент из списка
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Remove(DepChangeInfo item)
    {
      if (item.Parent == this)
      {
        item.Parent = null;
        return true;
      }
      else
        return false;
    }

    #endregion
  }
}
