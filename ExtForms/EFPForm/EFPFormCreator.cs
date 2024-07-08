// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using FreeLibSet.Config;

namespace FreeLibSet.Forms
{
  /// <summary>
  /// Параметры для вызова метода интерфейса <see cref="IEFPFormCreator.CreateForm(EFPFormCreatorParams)"/>.
  /// </summary>
  public sealed class EFPFormCreatorParams
  {
    #region Защищенный конструктор

    internal EFPFormCreatorParams(CfgPart cfg)
    {
#if DEBUG
      if (cfg == null)
        throw new ArgumentNullException("cfg");
#endif

      _Config = cfg;
      _ClassName = cfg.GetString("Class");
      _ConfigSectionName = cfg.GetString("ConfigSectionName");
      _Title = cfg.GetString("Title");
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Значение тега "Class"
    /// </summary>
    public string ClassName { get { return _ClassName; } }
    private readonly string _ClassName;

    /// <summary>
    /// Значение тега "ConfigName"
    /// </summary>
    public string ConfigSectionName { get { return _ConfigSectionName; } }
    private readonly string _ConfigSectionName;

    /// <summary>
    /// Все данные для создаваемой формы из секции конфигурации "Composition"-"UI"
    /// </summary>
    public CfgPart Config { get { return _Config; } }
    private readonly CfgPart _Config;

    /// <summary>
    /// Заголовок формы
    /// </summary>
    public string Title { get { return _Title; } }
    private readonly string _Title;

    /// <summary>
    /// Текстовое представление возвращает заголовок окна
    /// или "[ Без заголовка ]"
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      if (String.IsNullOrEmpty(Title))
        return "[ Без заголовка ]";
      else
        return Title;
    }

    #endregion
  }

  /// <summary>
  /// Объекты, реализующие интерфейс, должны быть добавлены в список <see cref="EFPApp.FormCreators"/>
  /// </summary>
  public interface IEFPFormCreator
  {
    /// <summary>
    /// Создать форму по заданным параметрам.
    /// Метод возвращает созданную форму или null, если переданный <paramref name="creatorParams"/>.ClassName не обрабатывается этим объектом.
    /// Если форма не может быть создана по другим причинам, например, из-за отсутствия у пользователя прав,
    /// должно быть выброшено осмысленное исключение.
    /// </summary>
    /// <param name="creatorParams">Список параметров для создания формы</param>
    /// <returns>Созданная форма или null</returns>
    Form CreateForm(EFPFormCreatorParams creatorParams);
  }
}
