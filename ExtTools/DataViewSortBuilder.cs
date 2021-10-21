using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using FreeLibSet.Collections;
using FreeLibSet.Core;

/*
 * The BSD License
 * 
 * Copyright (c) 2012-2015, Ageyev A.V.
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

namespace FreeLibSet.Data
{
  /// <summary>
  /// Построитель для свойства DataView.Sort.
  /// Удобно использовать для создания сортировки по нескольким полям, когда имена полей извлекаются откуда-нибудь.
  /// Используйте методы Add() и SetSort() для добавления частей сортировки. Затем присвойте свойству DataView.Sort значение DataViewSortBuilder.ToString()
  /// </summary>
  public sealed class DataViewSortBuilder
  {
    #region Конструктор

    /// <summary>
    /// Создает объект, готовый для вызова метода Add()
    /// </summary>
    public DataViewSortBuilder()
    {
      _Parts = new List<PartInfo>();
      _State = StateValue.Empty;
    }

    #endregion

    #region Поля

    private struct PartInfo
    {
      #region Поля

      public string Name;

      public ListSortDirection SortOrder;

      #endregion
    }

    private List<PartInfo> _Parts;

    private enum StateValue { Empty, NameSet, SortSet }

    private StateValue _State;

    #endregion

    #region Методы создания

    /// <summary>
    /// Очищает все данные и делает DataViewSortBuilder готовым к созданию нового объекта
    /// </summary>
    public void Clear()
    {
      _Parts.Clear();
      _State = StateValue.Empty;
    }

    /// <summary>
    /// Добавляет новое поле для сортировки.
    /// </summary>
    /// <param name="name">Имя поля</param>
    public void Add(string name)
    {
      ValidateName(name);
      PartInfo pi = new PartInfo();
      pi.Name = name;
      pi.SortOrder = ListSortDirection.Ascending;
      _Parts.Add(pi);
      _State = StateValue.NameSet;
    }

    private static readonly CharArrayIndexer _BadChars = new CharArrayIndexer(" ,[]");

    private void ValidateName(string name)
    {
      if (String.IsNullOrEmpty(name))
        throw new ArgumentNullException("name");

      int pBad = DataTools.IndexOfAny(name, _BadChars);
      if (pBad >= 0)
        throw new ArgumentException("Недопустимый символ \"" + name[pBad] + "\"", "name");

      if (name[0] == '.' || name[name.Length - 1] == '.')
        throw new ArgumentException("Имя не может начинаться или заканчиваться точкой", "name");

      if (name.IndexOf("..") >= 0)
        throw new ArgumentException("Имя не может содержать 2 точки подряд", "name");
    }

    /// <summary>
    /// Добавляет к только что добавленному имени точку и имя ссылочного поля.
    /// Непосредственно перед вызовом метода должен быть вызов Add() или другой вызов AddSubName()
    /// </summary>
    /// <param name="subName"></param>
    public void AddSubName(string subName)
    {
      ValidateName(subName);
      if (_State != StateValue.NameSet)
        throw new InvalidOperationException("Перед этим вызовом непосредственно должен быть вызов метода Add() или AddSubName()");

      PartInfo pi = _Parts[_Parts.Count - 1];
      pi.Name += "." + subName;
      _Parts[_Parts.Count - 1] = pi;
    }

    /// <summary>
    /// Устанавливает порядок сортировки для последнего добавленного поля.
    /// Перед вызовом метода должен быть вызов Add() или AddSubName().
    /// Если метод не вызывается, то используется сортировка по возрастанию
    /// </summary>
    /// <param name="sortOrder"></param>
    public void SetSort(ListSortDirection sortOrder)
    {
      if (_State != StateValue.NameSet)
        throw new InvalidOperationException("Перед этим вызовом непосредственно должен быть вызов метода Add() или AddSubName()");

      PartInfo pi = _Parts[_Parts.Count - 1];
      pi.SortOrder = sortOrder;
      _Parts[_Parts.Count - 1] = pi;

      _State = StateValue.SortSet;
    }

    #endregion

    #region ToString()

    /// <summary>
    /// Возвращает текстовое значение, которое можно присвоить свойству DataView.Sort.
    /// Если не были добавлены поля, то возвращает пустую строку
    /// </summary>
    /// <returns>Порядок сортировки</returns>
    public override string ToString()
    {
      if (_Parts.Count == 0)
        return String.Empty;

      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < _Parts.Count; i++)
      {
        if (i > 0)
          sb.Append(',');
        sb.Append(_Parts[i].Name);
        if (_Parts[i].SortOrder == ListSortDirection.Descending)
          sb.Append(" DESC");
      }

      return sb.ToString();
    }

    #endregion
  }
}
