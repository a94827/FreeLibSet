using System;
using System.Collections.Generic;
using System.Text;

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

namespace AgeyevAV.ExtDB
{
  /// <summary>
  /// Точка входа в базу данных
  /// </summary>
  public abstract class DBxEntry
  {
    #region Конструктор

    /// <summary>
    /// Инициализация объека
    /// </summary>
    /// <param name="db">База данных</param>
    /// <param name="permissions">Разрешения на доступ к базе данных</param>
    /// <param name="isMainEntry">Если true, то свойство DBx.MainEntry будет ссылаться на
    /// этот объект.</param>
    public DBxEntry(DBx db, DBxPermissions permissions, bool isMainEntry)
    {
      if (db == null)
        throw new ArgumentNullException("db");
      if (permissions == null)
        throw new ArgumentNullException("permissions");
      permissions.SetReadOnly();

      _DB = db;
      _Permissions = permissions;

      if (isMainEntry)
      {
        _DisplayName = "MainEntry";
        db.MainEntry = this;
      }
      else
        _DisplayName = String.Empty;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// База данных, к которой относится точка подключения. Не может быть null
    /// </summary>
    public DBx DB { get { return _DB; } }
    private DBx _DB;

    /// <summary>
    /// Разрешения на доступ к таблицам. Не может быть null
    /// </summary>
    public DBxPermissions Permissions { get { return _Permissions; } }
    private DBxPermissions _Permissions;

    /// <summary>
    /// Отображаемое имя (для отладки).
    /// По умолчанию имеет значение "MainEntry" для главной точки входа и "" для других точек
    /// </summary>
    public string DisplayName
    {
      get { return _DisplayName; }
      set
      {
        if (value == null)
          _DisplayName = String.Empty;
        else
          _DisplayName = value;
      }
    }
    private string _DisplayName;

    /// <summary>
    /// Возвращает строку подключения, не содержащую пароль.
    /// Если строки подключения не поддерживается базой данных, возвращает пустую строку.
    /// </summary>
    public virtual string UnpasswordedConnectionString { get { return String.Empty; } }

    /// <summary>
    /// Возвращает true, если точка входа является главной для базы данных
    /// </summary>
    public bool IsMainEntry { get { return Object.ReferenceEquals(this, DB.MainEntry); } }

    /// <summary>
    /// Описание точки входа в формате "DisplayName on DB.DisplayName"
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return "\"" + DisplayName + "\" on \"" + DB.DisplayName + "\"";
    }

    #endregion

    #region Список соединений

    /// <summary>
    /// Создать новое соединение
    /// Этот метод может вызываться асинхронно
    /// </summary>
    /// <returns>Соединение с базовй данных</returns>
    public abstract DBxConBase CreateCon();

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Создает копию точки входа с другим набором прав.
    /// Свойство DisplayName не копируется.
    /// </summary>
    /// <param name="newPermissions">Требуемые разрешения на доступ к объектам базы данных</param>
    /// <returns>Новая точка входа</returns>
    public abstract DBxEntry Clone(DBxPermissions newPermissions);

    #endregion
  }
}
