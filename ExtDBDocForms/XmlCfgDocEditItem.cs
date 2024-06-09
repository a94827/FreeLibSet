// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Data.Docs;
using FreeLibSet.Config;
using FreeLibSet.DependedValues;

namespace FreeLibSet.Forms.Docs
{

  /// <summary>
  /// Представление текстового поля для хранения конфигурационных данных как <see cref="XmlCfgPart"/>.
  /// Класс может использоваться в качестве базового для собственной реализации чтения/записи данных,
  /// либо самостоятельно. В последнем случае создаются отдельные объекты, реализующие <see cref="IDocEditItem"/>
  /// (например <see cref="DocValueTextBox"/>) и присоединяются как дочерние элементы. При их создании им передаются
  /// <see cref="DBxDocValue"/>, полученные из свойства Values этого объекта.
  /// </summary>
  public class XmlCfgDocEditItem : DocEditItemWithChildren
  {
    #region Конструктор

    /// <summary>
    /// Создает объект, присоединенный к текстовому полю в формате XML
    /// </summary>
    /// <param name="docValue">Объект для доступа к значению текстового поля</param>
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
    private /* не может быть readonly */ DBxDocValue _DocValue; 

    /// <summary>
    /// Конфигурационные данные, которые должны читаться и записываться
    /// </summary>
    public TempCfg Data { get { return _Data; } }
    private readonly TempCfg _Data;

    private string _OrgValue;

    private readonly DepChangeInfoItem _DataChangeInfo;

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
    /// Загружает в объект Data значения из текстового поля <see cref="DocValue"/>.
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
    /// Записывает из объекта Data значения в текстовое поля <see cref="DocValue"/>.
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
