// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;

namespace FreeLibSet.FIAS
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
      if (_StartAddress.IsEmpty)
        sb.Append("Весь справочник");
      else
        sb.Append(_StartAddress.ToString());

      return sb.ToString();
    }

    #endregion
  }
}
