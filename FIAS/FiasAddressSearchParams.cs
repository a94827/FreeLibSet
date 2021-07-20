using System;
using System.Collections.Generic;
using System.Text;

/*
 * The BSD License
 * 
 * Copyright (c) 2020, Ageyev A.V.
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

namespace AgeyevAV.FIAS
{
  /// <summary>
  /// Параметры поиска адресных объектов
  /// </summary>
  [Serializable]
  public sealed class FiasAddressSearchParams
  {
    #region Конструктор

    /// <summary>
    /// Создает набор параметров
    /// </summary>
    public FiasAddressSearchParams()
    {
      _ActualOnly = true;
      _StartAddress = new FiasAddress();
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Текст в наименовании адресного объекта
    /// </summary>
    public string Text { get { return _Text; } set { _Text = value; } }
    private string _Text;

    /// <summary>
    /// Уровни, среди которых выполняется поиск.
    /// Если null, то среди всех уровней
    /// </summary>
    public FiasLevel[] Levels { get { return _Levels; } set { _Levels = value; } }
    private FiasLevel[] _Levels;

    /// <summary>
    /// Если true (по умолчанию), то выполнять поиск только среди актуальных записей.
    /// Значение false не может использоваться, если FiasDBConfig.UseHistory=false.
    /// </summary>
    public bool ActualOnly { get { return _ActualOnly; } set { _ActualOnly = value; } }
    private bool _ActualOnly;

    /// <summary>
    /// Начальный адрес (например, город), в котором нужно выполнять поиск (улиц)
    /// </summary>
    public FiasAddress StartAddress { get { return _StartAddress; } set { _StartAddress = value; } }
    private FiasAddress _StartAddress;

    /// <summary>
    /// Возвращает true, если параметры не заданы
    /// </summary>
    public bool IsEmpty { get { return String.IsNullOrEmpty(Text); } }

    #endregion

    #region Текстовое представление

    /// <summary>
    /// Текстовое представление параметров поиска
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      if (IsEmpty)
        return "Параметры поиска не заданы";

      StringBuilder sb = new StringBuilder();
      sb.Append("Искать \"");
      sb.Append(_Text);
      sb.Append("\"");
      sb.Append(", уровни: ");
      if (_Levels == null)
        sb.Append(" Все адресные объекты");
      else
      {
        for (int i = 0; i < _Levels.Length; i++)
        {
          if (i > 0)
            sb.Append(", ");
          sb.Append(FiasEnumNames.ToString(_Levels[i], false));
        }
      }
      if (_ActualOnly)
        sb.Append(", только актуальные");
      else
        sb.Append(", включая исторические");
      sb.Append(", в: ");
      if(_StartAddress.IsEmpty)
        sb.Append("Весь справочник");
      else
        sb.Append(_StartAddress.ToString());

      return sb.ToString();
    }

    #endregion
  }
}
