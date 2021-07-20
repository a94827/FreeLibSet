using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

/*
 * The BSD License
 * 
 * Copyright (c) 2015, Ageyev A.V.
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

namespace AgeyevAV.ExtDB.Docs
{
  /// <summary>
  /// Реализация свойства DBxSingleDoc.SubDocs
  /// </summary>
  [StructLayout(LayoutKind.Auto)]
  public struct DBxSingleDocSubDocs
  {
    #region Конструктор

    internal DBxSingleDocSubDocs(DBxSingleDoc owner)
    {
      _Owner = owner;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Документ, к которому относятся поддокументы
    /// </summary>
    public DBxSingleDoc Owner { get { return _Owner; } }
    private DBxSingleDoc _Owner;

    /// <summary>
    /// Возвращает количество видов поддокументов, определенных в DBxDocType.SubDocs
    /// </summary>
    public int Count { get { return _Owner.MultiDocs.SubDocs.Count; } }

    /// <summary>
    /// Возвращает поддокументы требуемого вида.
    /// Если для документов не определены поддокументы вида <paramref name="subDocTypeName"/>,
    /// генерируется исключение
    /// </summary>
    /// <param name="subDocTypeName">Имя таблицы поддокументов</param>
    /// <returns>Объект для доступа к однотипным документам</returns>
    public DBxSingleSubDocs this[string subDocTypeName]
    {
      get
      { 
        DBxMultiSubDocs SubDocs=_Owner.MultiDocs.SubDocs[subDocTypeName];
        if (SubDocs == null)
        {
          if (String.IsNullOrEmpty(subDocTypeName))
            throw new ArgumentNullException("subDocTypeName");
          else
            throw new ArgumentException("Неизвестный вид поддокументов \"" + subDocTypeName + "\"", "subDocTypeName");
        }
        return new DBxSingleSubDocs(_Owner, SubDocs);
      }
    }

    /// <summary>
    /// Возвращает поддокументы требуемого вида.
    /// </summary>
    /// <param name="index">Индекс вида поддокументов в диапазоне от 0 до (Count-1)</param>
    /// <returns>Объект для доступа к однотипным документам</returns>
    public DBxSingleSubDocs this[int index]
    {
      get
      {
        DBxMultiSubDocs SubDocs = _Owner.MultiDocs.SubDocs[index];
        return new DBxSingleSubDocs(_Owner, SubDocs);
      }
    }

    /// <summary>
    /// Для отладки
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return "Поддокументы для " + Owner.ToString();
    }

    #endregion
  }
}
