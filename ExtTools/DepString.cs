using System;
using System.Collections.Generic;
using System.Text;

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

// Функции для обработки строк
// Нужно бы сделать сериализуемым, но DepExpr3 таковым не является

namespace AgeyevAV.DependedValues
{

  /// <summary>
  /// Разновидность DepExpr1, которая выполняет преобразование ToString()
  /// Применяется, в основном, для отладочных целей.
  /// </summary>
  /// <typeparam name="T">Тип аргумента</typeparam>
  [Serializable]
  public sealed class DepToString<T> : DepExpr1<string, T>
  {
    #region Конструктор

    /// <summary>
    /// Конструктор с ссылкой на исходные данные
    /// </summary>
    /// <param name="source">Источник исходных данных</param>
    public DepToString(DepValue<T> source)
      : base(source)
    {
    }

    #endregion

    #region Внутренняя реализация

    /// <summary>
    /// Возвращает Source.Value.ToString()
    /// </summary>
    /// <returns></returns>
    protected override string Calculate()
    {
      return Source.Value.ToString();
    }

    #endregion

    #region ToString()

    /// <summary>
    /// Возвращает "TOSTRING", если свойство OwnerInfo не установлено
    /// </summary>
    /// <returns>Текстовое представление для отладки</returns>
    public override string ToString()
    {
      if (OwnerInfo.Owner == null)
        return "TOSTRING";
      else
        return base.ToString();
    }

    #endregion

  }

  /// <summary>
  /// Вычислитель, извлекающий подстроку
  /// </summary>
  [Serializable]
  public sealed class DepSubstring : DepExpr3<String, String, int, int>
  {
    #region Конструктор

    /// <summary>
    /// Создает вычислитель
    /// </summary>
    /// <param name="sourceString">Исходная строка</param>
    /// <param name="startIndex">Начальный индекс</param>
    /// <param name="length">Длина подстроки</param>
    public DepSubstring(DepValue<String> sourceString, DepValue<int> startIndex, DepValue<int> length)
      : base(sourceString, startIndex, length)
    {
    }

    /// <summary>
    /// Создает вычислитель
    /// </summary>
    /// <param name="sourceString">Исходная строка</param>
    /// <param name="startIndex">Начальный индекс</param>
    /// <param name="length">Длина подстроки</param>
    public DepSubstring(DepValue<String> sourceString, int startIndex, int length)
      :base(sourceString, new DepConst<int>(startIndex), new DepConst<int>(length))
    {
    }

    #endregion

    #region Вычисление

    /// <summary>
    /// Вычисляет значение
    /// </summary>
    /// <returns></returns>
    protected override string Calculate()
    {
      string Arg1 = Source1.Value;
      int Arg2 = Source2.Value;
      int Arg3 = Source3.Value;

      if (String.IsNullOrEmpty(Arg1))
        return String.Empty;
      if (Arg1.Length < Arg2)
        return String.Empty;

      if (Arg1.Length < (Arg2 + Arg3))
        return Arg1.Substring(Arg2);
      else
        return Arg1.Substring(Arg2, Arg3);
    }

    #endregion

    #region ToString()

    /// <summary>
    /// Возвращает "SUBSTRING", если свойство OwnerInfo не установлено
    /// </summary>
    /// <returns>Текстовое представление для отладки</returns>
    public override string ToString()
    {
      if (OwnerInfo.Owner == null)
        return "SUBSTRING";
      else
        return base.ToString();
    }

    #endregion
  }
}
