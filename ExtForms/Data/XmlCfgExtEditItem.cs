// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Config;
using FreeLibSet.DependedValues;
using FreeLibSet.Data;
using FreeLibSet.UICore;

namespace FreeLibSet.Forms.Data
{

  /// <summary>
  /// Представление текстового поля для хранения конфигурационных данных как <see cref="XmlCfgPart"/>.
  /// Класс может использоваться в качестве базового для собственной реализации чтения/записи данных,
  /// либо самостоятельно. В последнем случае создаются отдельные объекты, реализующие <see cref="IUIExtEditItem"/>
  /// (например <see cref="ExtValueTextBox"/>) и присоединяются как дочерние элементы. При их создании им передаются
  /// <see cref="DBxExtValue"/>, полученные из свойства Values этого объекта.
  /// </summary>
  public class XmlCfgExtEditItem : UIExtEditItemWithChildren
  {
    #region Конструктор

    /// <summary>
    /// Создает объект, присоединенный к текстовому полю в формате XML
    /// </summary>
    /// <param name="extValue">Объект для доступа к значению текстового поля</param>
    public XmlCfgExtEditItem(DBxExtValue extValue)
    {
      _ExtValue = extValue;

      base.ChangeInfo.DisplayName = extValue.DisplayName;
      _DataChangeInfo = new DepChangeInfoItem();
      base.ChangeInfoList.Add(_DataChangeInfo);


      _Data = new TempCfg();
      //FData.Changed += new EventHandler(Data_Changed);

      _Values = new DBxCfgExtValues(_Data, extValue.IsReadOnly);
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
    public DBxExtValue ExtValue { get { return _ExtValue; } }
    private /* не может быть readonly, так как структура */ DBxExtValue _ExtValue; 

    /// <summary>
    /// Конфигурационные данные, которые должны читаться и записываться
    /// </summary>
    public TempCfg Data { get { return _Data; } }
    private readonly TempCfg _Data;

    private string _OrgValue;

    private readonly DepChangeInfoItem _DataChangeInfo;

    #endregion

    #region Доступ к значениям как к DBxExtValue

    /// <summary>
    /// Подключение к значениям
    /// </summary>
    public DBxCfgExtValues Values { get { return _Values; } }
    private readonly DBxCfgExtValues _Values;

    #endregion

    #region Чтение и запись

    /// <summary>
    /// Загружает в объект Data значения из текстового поля <see cref="ExtValue"/>.
    /// </summary>
    public override void ReadValues()
    {
      _OrgValue = _ExtValue.AsString;
      try
      {
        _Data.AsXmlText = _ExtValue.AsString;
      }
      catch
      {
      }

      base.ReadValues();
    }

    /// <summary>
    /// Записывает из объекта Data значения в текстовое поля <see cref="ExtValue"/>.
    /// </summary>
    public override void WriteValues()
    {
      base.WriteValues();

      _ExtValue.SetString(_Data.AsXmlText);
      // если после каждого события Changed проверять совпадение исходного и текущего значений, будет медленно
      _DataChangeInfo.Changed = (_ExtValue.AsString != _OrgValue);
    }

    #endregion
  }
}
