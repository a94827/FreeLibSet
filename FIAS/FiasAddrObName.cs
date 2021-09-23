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
  /// Хранит наименование адресного объекта, разбитое на лексемы, чтобы его можно было сравнивать при неточном сравнении.
  /// Например, "МАРКСА", "К.Маркса" и "Карла Маркса" - это одна и та же улица
  /// </summary>
  internal sealed class FiasAddrObName : IEquatable<FiasAddrObName>
  {
    #region Конструктор

    public FiasAddrObName(string name)
    {
      if (name == null)
        name = String.Empty;
      _Name = name.Trim();

      string[] a = name.ToLowerInvariant().Replace('ё', 'е').Split(' ', '.', '-', '№');
      List<string> lst = new List<string>(a.Length);
      for (int i = 0; i < a.Length; i++)
      {
        if (a[i].Length == 0)
          continue; // пустые лексемы могут встречаться
        if (lst.Count > 0 && a[i].Length <= 2)
        {
          // отбрасываем окончания типа "1-ый", "2-я"
          switch (a[i])
          {
            case "ый":
            case "ой":
            case "ий":
            case "й":
            case "ая":
            case "яя":
            case "я":
            case "ое":
            case "ее":
            case "е":
              continue;
          }
        }

        // Добавляем лексему
        lst.Add(a[i]);
      }
      _Tokens = lst.ToArray();
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя, как оно задано в конструкторе
    /// </summary>
    public string Name { get { return _Name; } }
    private readonly string _Name;

    /// <summary>
    /// Список лексем
    /// </summary>
    private readonly string[] _Tokens;

    public override string ToString()
    {
      return _Name;
    }

    #endregion

    #region Сравнение

    public static bool operator ==(FiasAddrObName a, FiasAddrObName b)
    {
      // Флажки использованных лексем
      bool[] flags_a = new bool[a._Tokens.Length];
      bool[] flags_b = new bool[b._Tokens.Length];

      #region Точное сравнение лексем

      for (int i = 0; i < a._Tokens.Length; i++)
      {
        for (int j = 0; j < b._Tokens.Length; j++)
        {
          if (flags_b[j]) continue;

          if (String.Equals(a._Tokens[i], b._Tokens[j], StringComparison.Ordinal))
          {
            flags_a[i] = true;
            flags_b[j] = true;
            break;
          }
        }
      }

      #endregion

      #region Неточное сравнение

      ComparePartial(a._Tokens, flags_a, b._Tokens, flags_b);
      ComparePartial(b._Tokens, flags_b, a._Tokens, flags_a);

      #endregion

      #region Проверка

      int za = CalcFalse(flags_a);
      int zb = CalcFalse(flags_b);
      if (za == 0 && zb == 0)
        return true;
      if (za > 0 && zb > 0)
        return false;

      if (za > 0)
        return CalcCanSkipToken(a._Tokens, flags_a);
      else
        return CalcCanSkipToken(b._Tokens, flags_b);

      #endregion
    }

    /// <summary>
    /// Сравниваем лексемы с сокращениями.
    /// Например, улица "К. Маркса" = "Карла Маркса"
    /// </summary>
    /// <param name="tokens_a"></param>
    /// <param name="flags_a"></param>
    /// <param name="tokens_b"></param>
    /// <param name="flags_b"></param>
    private static void ComparePartial(string[] tokens_a, bool[] flags_a, string[] tokens_b, bool[] flags_b)
    {
      for (int i = 0; i < (tokens_a.Length - 1); i++) // фамилия должна быть в конце
      {
        if (flags_a[i]) continue;
        if (tokens_a[i].Length == 1 && tokens_a[i][0] >= 'а' && tokens_a[i][0] <= 'я')
        {
          for (int j = 0; j < tokens_b.Length; j++)
          {
            if (flags_b[j]) continue;
            if (tokens_b[j][0] == tokens_a[i][0])
            {
              flags_a[i] = true;
              flags_b[j] = true;
              break;
            }
          }
        }
      }
    }

    /// <summary>
    /// Подсчитывает количество не установленных флажков
    /// </summary>
    /// <param name="flags"></param>
    /// <returns></returns>
    private static int CalcFalse(bool[] flags)
    {
      int cnt = 0;
      for (int i = 0; i < flags.Length; i++)
      {
        if (!flags[i])
          cnt++;
      }
      return cnt;
    }

    /// <summary>
    /// Возвращает true, если можно пропустить оставшиеся лексемы
    /// </summary>
    /// <param name="tokens"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    private static bool CalcCanSkipToken(string[] tokens, bool[] flags)
    {
      if (tokens.Length < 2 || tokens.Length > 4)
        return false; // Фамилия может быть с одним дефисом. Может нехватать имени и отчества

      if (!flags[flags.Length - 1])
        return false; // последняя лексема значима
      int zCount = 0; // Количество ненайденных лексем. Ненайденные лексемы могут быть только в конце
      for (int i = 0; i < (tokens.Length - 1); i++)
      {
        if (flags[i])
        {
          for (int j = i + 1; j < (tokens.Length - 1); j++)
          {
            if (!flags[j])
              return false;
          }
          break;
        }
        else
          zCount++;
      }

      // 02.10.2020
      // Есть улица "Ю.-Р.Г.Эрвье"
      // В ней 4 части, но фамилия не составная
      if (zCount >= 3)
        return false;

#if DEBUG
      if (zCount < 1 || zCount > 2)
        throw new BugException("zCount=" + zCount.ToString());
#endif

      for (int i = 0; i < zCount; i++)
      {
#if DEBUG
        if (flags[i])
          throw new BugException("flags[i]=true");
#endif

        string t = tokens[i];
        for (int j = 0; j < t.Length; j++)
        {
          if (t[j] < 'а' || t[j] > 'я')
            return false; // небуквенный символ - не может быть именем человека
        }
        //  if (tokens[i].Length > 1)
        //    return false; // TODO: А как же сравнение улиц "Маркса" и "Карла Маркса"?

      }

      return true;
    }

    public static bool operator !=(FiasAddrObName a, FiasAddrObName b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      if (obj is FiasAddrObName)
        return this == (FiasAddrObName)obj;
      else
        return false;
    }

    public bool Equals(FiasAddrObName obj)
    {
      return this == obj;
    }

    public override int GetHashCode()
    {
      // Нет возможности просто посчитать хэш-сумму

      return 0;
    }

    #endregion
  }
}
