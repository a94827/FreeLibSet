using System;
using System.Collections.Generic;
using System.Text;
using AgeyevAV.ExtDB.Docs;
using AgeyevAV.Config;
using AgeyevAV.DependedValues;

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

namespace AgeyevAV.ExtForms.Docs
{

  /// <summary>
  /// Представление текстового поля для хранения конфигурационных данных как XmlCfgPart.
  /// Класс может использоваться в качестве базового для собственной реализации чтения/записи данных,
  /// либо самостоятельно. В последнем случае создаются отдельные объекты, реализующие IDocEditItem
  /// (например DocValueTextBox) и присоединяются как дочерние элементы. При их создании им передаются
  /// DBxDocValues, полученные из свойства Values этого объекта
  /// </summary>
  public class XmlCfgDocEditItem : DocEditItemWithChildren
  {
    #region Конструктор

    /// <summary>
    /// Создает объект, присоединенный к текстовому полю в формате XML
    /// </summary>
    /// <param name="docValue">Объект для доступа к значению</param>
    public XmlCfgDocEditItem(DBxDocValue docValue)
    {
      _DocValue = docValue;

      base.ChangeInfo.DisplayName = docValue.DisplayName;
      _DataChangeInfo = new DepChangeInfoItem();
      base.ChangeInfoList.Add(_DataChangeInfo);


      _Data = new TempCfg();
      //FData.Changed += new EventHandler(Data_Changed);

      _Values = new XmlCfgDocValues(_Data, docValue.IsReadOnly);
    }

    //void Data_Changed(object Sender, EventArgs Args)
    //{
    //  ChangeInfo.Changed = true;
    //}

    #endregion

    #region Свойства

    /// <summary>
    /// Значение XML-поля, хранящегося в базе данных.
    /// Задается в конструкторе.
    /// </summary>
    public DBxDocValue DocValue { get { return _DocValue; } }
    private DBxDocValue _DocValue;

    /// <summary>
    /// Конфигурационные данные, которые должны читаться и записываться
    /// </summary>
    public TempCfg Data { get { return _Data; } }
    private TempCfg _Data;

    private string _OrgValue;

    private DepChangeInfoItem _DataChangeInfo;

    #endregion

    #region Доступ к значениям как к DBxDocValue

    /// <summary>
    /// Подключение к значениям
    /// </summary>
    public XmlCfgDocValues Values { get { return _Values; } }
    private XmlCfgDocValues _Values;

    #endregion

    #region Чтение и запись

    /// <summary>
    /// Загружает в объект Data значения из текстового поля DocValue
    /// </summary>
    public override void ReadValues()
    {
      _OrgValue = _DocValue.AsString;
      try
      {
        _Data.AsXmlText = _DocValue.AsString;
      }
      catch
      {
      }

      base.ReadValues();
    }

    /// <summary>
    /// Записывает из объекта Data значения в текстовое поля DocValue
    /// </summary>
    public override void WriteValues()
    {
      base.WriteValues();

      _DocValue.SetString(_Data.AsXmlText);
      // если после каждого события Changed проверять совпадение исходного и текущего значений, будет медленно
      _DataChangeInfo.Changed = (_DocValue.AsString != _OrgValue);
    }

    #endregion
  }
}
