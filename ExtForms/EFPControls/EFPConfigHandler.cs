// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.Config;
using FreeLibSet.Core;
using FreeLibSet.Collections;

namespace FreeLibSet.Forms
{
  #region Перечисление EFPConfigPurpose

  /// <summary>
  /// Назначение действия, выполняемого <see cref="EFPConfigHandler"/>
  /// </summary>
  public enum EFPConfigPurpose
  {
    /// <summary>
    /// Чтение/запись именной секции конфигурации с именем, заданным свойством <see cref="EFPConfigHandler.ConfigSectionName"/>.
    /// </summary>
    Config,

    /// <summary>
    /// Чтение/сохранение композиции открытых окон пользовательского интерфейса
    /// </summary>
    Composition,

    /// <summary>
    /// Идет сбор именных секций конфигурации для предварительной загрузки при показе формы
    /// </summary>
    Preload,
  }

  #endregion

  /// <summary>
  /// Дополнительная информация, передаваемая методам интерфейса <see cref="IEFPConfigurable"/>.
  /// В текущей реализации содержит только назначение действия (чтение/запись именной секции конфигурации
  /// или композиции окон)
  /// </summary>
  public class EFPConfigActionInfo : IReadOnlyObject
  {
    #region Свойства

    /// <summary>
    /// Назначение выполняемого действия
    /// </summary>
    public EFPConfigPurpose Purpose
    {
      get { return _Purpose; }
      set
      {
        CheckNotReadOnly();
        _Purpose = value;
      }
    }
    private EFPConfigPurpose _Purpose;

    /// <summary>
    /// Возвращает <see cref="Purpose"/>
    /// </summary>
    /// <returns>Тестовое представление</returns>
    public override string ToString()
    {
      return Purpose.ToString();
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Возвращает true, если объект был переведен в режим "только чтение" вызовом <see cref="SetReadOnly()"/>.
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// Выбрасывает исключение, если <see cref="IsReadOnly"/>=true
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    /// <summary>
    /// Переводит объект в режим "только чтение".
    /// Вызывается из <see cref="EFPConfigHandler"/>.
    /// </summary>
    public void SetReadOnly()
    {
      _IsReadOnly = true;
    }

    #endregion
  }

  /// <summary>
  /// Интерфейс, реализуемый <see cref="EFPControlBase"/>, <see cref="EFPFormProvider"/> и другими классами, использующими <see cref="EFPConfigHandler"/>.
  /// Определяет методы чтения / записи секций конфигурации и получения списка категорий.
  /// </summary>
  public interface IEFPConfigurable
  {
    #region Методы

    /// <summary>
    /// Получить список категорий, которые должны быть обработаны
    /// </summary>
    /// <param name="categories">Список для добавления категорий</param>
    /// <param name="rwMode">Чтение или запись</param>
    /// <param name="actionInfo">Информация о выполняемом действии</param>
    void GetConfigCategories(ICollection<string> categories, EFPConfigMode rwMode, EFPConfigActionInfo actionInfo);

    /// <summary>
    /// Выполнить запись одной секции конфигурации
    /// </summary>
    /// <param name="category">Категория записываемой секции</param>
    /// <param name="cfg">Объект для записb значений</param>
    /// <param name="actionInfo">Информация о выполняемом действии</param>
    void WriteConfigPart(string category, CfgPart cfg, EFPConfigActionInfo actionInfo);

    /// <summary>
    /// Выполнить чтение одной секции конфигурации
    /// </summary>
    /// <param name="category">Категория считываемой секции</param>
    /// <param name="cfg">Объект для чтения значений</param>
    /// <param name="actionInfo">Информация о выполняемом действии</param>
    void ReadConfigPart(string category, CfgPart cfg, EFPConfigActionInfo actionInfo);

    #endregion
  }

  /// <summary>
  /// Содержит имя секции конфигурации, список категорий, флажки изменений и события чтения/записи. 
  /// Используется в <see cref="EFPFormProvider"/> и <see cref="EFPControlBase"/>.
  /// Не применяется для сохранения истории и пользовательских наборов, т.к не содержит свойства UserSetName.
  /// 
  /// Реализация интерфейса <see cref="IEFPConfigurable"/> выполняет опрос присоединенных источников.
  /// </summary>
  public sealed class EFPConfigHandler : IEFPConfigurable
  {
    #region Конструктор

    /// <summary>
    /// Создает новый объект без установки свойства <see cref="ConfigSectionName"/>.
    /// Список категорий пустой.
    /// </summary>
    public EFPConfigHandler()
    {
      _Sources = new List<IEFPConfigurable>();
      _Changed = new ChangeFlags(this);
      _ConfigSectionName = String.Empty;
      _Categories = new SingleScopeStringList(false);
    }

    #endregion

    #region Свойство ConfigSectionName

    /// <summary>
    /// Имя секции конфигурации.
    /// По умолчанию – пустая строка.
    /// </summary>
    public string ConfigSectionName
    {
      get { return _ConfigSectionName; }
      set
      {
        if (value == null)
          _ConfigSectionName = String.Empty;
        else
          _ConfigSectionName = value;
      }
    }
    private string _ConfigSectionName;

    #endregion

    #region Источники данных

    /// <summary>
    /// Список источников данных.
    /// <see cref="EFPControlBase"/> и <see cref="EFPFormProvider"/> добавляют себя в этот список сразу после создания <see cref="EFPConfigHandler"/>.
    /// </summary>
    public IList<IEFPConfigurable> Sources { get { return _Sources; } }
    private readonly List<IEFPConfigurable> _Sources;

    #endregion

    #region IEFPConfigurable Members

    /// <summary>
    /// Построение списка категорий.
    /// Опрашивает все источники данных в списке <see cref="Sources"/>.
    /// </summary>
    /// <param name="categories">Заполняемый список категорий</param>
    /// <param name="rwMode">Режим: Чтение или запись данных</param>
    /// <param name="actionInfo">Информация о выполняемом действии</param>
    public void GetConfigCategories(ICollection<string> categories, EFPConfigMode rwMode, EFPConfigActionInfo actionInfo)
    {
      for (int i = 0; i < _Sources.Count; i++)
        _Sources[i].GetConfigCategories(categories, rwMode, actionInfo);
    }

    /// <summary>
    /// Записывает секцию конфигурации.
    /// Вызывает методы всех объектов из списка <see cref="Sources"/>.
    /// </summary>
    /// <param name="category">Категория</param>
    /// <param name="cfg">Записываемая секция конфигурации</param>
    /// <param name="actionInfo">Информация о выполняемом действии</param>
    public void WriteConfigPart(string category, CfgPart cfg, EFPConfigActionInfo actionInfo)
    {
      for (int i = 0; i < _Sources.Count; i++)
        _Sources[i].WriteConfigPart(category, cfg, actionInfo);
    }

    /// <summary>
    /// Считывает секцию конфигурации.
    /// Вызывает методы всех объектов из списка <see cref="Sources"/>.
    /// </summary>
    /// <param name="category">Категория</param>
    /// <param name="cfg">Считываемая секция конфигурации</param>
    /// <param name="actionInfo">Информация о выполняемом действии</param>
    public void ReadConfigPart(string category, CfgPart cfg, EFPConfigActionInfo actionInfo)
    {
      for (int i = 0; i < _Sources.Count; i++)
        _Sources[i].ReadConfigPart(category, cfg, actionInfo);
    }

    #endregion

    #region Флажки

    /// <summary>
    /// Для эмуляции именного индексированного свойства <see cref="EFPConfigHandler.Changed"/>.
    /// </summary>
    public sealed class ChangeFlags
    {
      #region Защищенный конструктор

      internal ChangeFlags(EFPConfigHandler owner)
      {
        _Owner = owner;
        _Flags = new Dictionary<string, object>();
      }

      #endregion

      #region Свойства

      private readonly EFPConfigHandler _Owner;

      /// <summary>
      /// Коллекция установленных флажков.
      /// Ключ - категория.
      /// Значение - не используется.
      /// </summary>
      private readonly Dictionary<string, object> _Flags;

      /// <summary>
      /// Чтение или установка признака изменения для категории
      /// Это свойство устанавливается, например, после изменения фильтров.
      /// Через некоторое время, по таймеру будет вызван метод ReadConfig().
      /// Если, например, поле CurrentId нужно записать в секцию <see cref="EFPConfigCategories.GridView"/> только при закрытии формы, 
      /// следует установить флаг в обработчике OnHidden() перед вызовом метода базового класса.
      /// Флажки можно задавать и для несуществующих категорий. Они игнорируются.
      /// Нужно, т.к. внешний код может убрать категорию из списка, вызвав Remove().
      /// </summary>
      /// <param name="category">Категория (обычно из <see cref="EFPConfigCategories"/>)</param>
      /// <returns>Наличие изменений</returns>
      public bool this[string category]
      {
        get { return _Flags.ContainsKey(category); }
        set
        {
          if (value)
            _Flags[category] = null;
          else
            _Flags.Remove(category);
        }
      }

      #endregion

      #region Методы

      internal void Clear()
      {
        _Flags.Clear();
      }

      internal bool IsEmpty { get { return _Flags.Count == 0; } }

      #endregion
    }

    /// <summary>
    /// Флажки изменений по категориям
    /// </summary>
    public ChangeFlags Changed { get { return _Changed; } }
    private readonly ChangeFlags _Changed;

    #endregion

    #region Объекты EFPConfigActionInfo

    static EFPConfigHandler()
    {
      _ConfigInfoObject = new EFPConfigActionInfo();
      _ConfigInfoObject.Purpose = EFPConfigPurpose.Config;
      _ConfigInfoObject.SetReadOnly();

      _PreloadInfoObject = new EFPConfigActionInfo();
      _PreloadInfoObject.Purpose = EFPConfigPurpose.Preload;
      _PreloadInfoObject.SetReadOnly();

      _CompositionInfoObject = new EFPConfigActionInfo();
      _CompositionInfoObject.Purpose = EFPConfigPurpose.Composition;
      _CompositionInfoObject.SetReadOnly();
    }

    private static readonly EFPConfigActionInfo _ConfigInfoObject;
    private static readonly EFPConfigActionInfo _CompositionInfoObject;
    private static readonly EFPConfigActionInfo _PreloadInfoObject;

    #endregion

    #region Чтение и запись

    /// <summary>
    /// Единственный список для сбора списка категорий
    /// </summary>
    private readonly SingleScopeStringList _Categories;

    /// <summary>
    /// Записывает все данные, независимо от флажков.
    /// Флажки сбрасываются.
    /// Если свойство <see cref="ConfigSectionName"/> не установлено, никаких действий не выполняется.
    /// Перед использованием этого метода рекомендуется вызывать <see cref="IEFPConfigManager.Preload(EFPConfigSectionInfo[], EFPConfigMode)"/>.
    /// </summary>
    /// <param name="configManager">Менеджер секций конфигурации, обычно <see cref="EFPApp.ConfigManager"/></param>
    public void WriteConfig(IEFPConfigManager configManager)
    {
      if (configManager == null)
        throw new ArgumentNullException("configManager");

      if (ConfigSectionName.Length == 0)
        return;

      _Categories.Clear();
      GetConfigCategories(_Categories, EFPConfigMode.Write, _ConfigInfoObject);

      for (int i = 0; i < _Categories.Count; i++)
      {
        EFPConfigSectionInfo configInfo = new EFPConfigSectionInfo(ConfigSectionName, _Categories[i]);
        CfgPart cfg;
        using (configManager.GetConfig(configInfo, EFPConfigMode.Write, out cfg))
        {
          WriteConfigPart(_Categories[i], cfg, _ConfigInfoObject);
        }
      }

      Changed.Clear();
    }

    /// <summary>
    /// Записывает только измененные данные и сбрасывает флажки   
    /// Если свойство <see cref="ConfigSectionName"/> не установлено, никаких действий не выполняется.
    /// </summary>
    /// <param name="configManager">Менеджер секций конфигурации, обычно <see cref="EFPApp.ConfigManager"/></param>
    public void WriteConfigChanges(IEFPConfigManager configManager)
    {
      if (configManager == null)
        throw new ArgumentNullException("configManager");

      if (ConfigSectionName.Length == 0)
        return;

      if (Changed.IsEmpty)
        return;

      _Categories.Clear();
      GetConfigCategories(_Categories, EFPConfigMode.Write, _ConfigInfoObject);

      for (int i = 0; i < _Categories.Count; i++)
      {
        if (Changed[_Categories[i]])
        {
          EFPConfigSectionInfo configInfo = new EFPConfigSectionInfo(ConfigSectionName, _Categories[i]);
          CfgPart cfg;
          using (configManager.GetConfig(configInfo, EFPConfigMode.Write, out cfg))
          {
            WriteConfigPart(_Categories[i], cfg, _ConfigInfoObject);
          }
        }
      }

      Changed.Clear();
    }


    /// <summary>
    /// Считывает данные для всех категорий, сбрасывает флажки
    /// Если свойство <see cref="ConfigSectionName"/> не установлено, никаких действий не выполняется.
    /// Перед использованием этого метода рекомендуется вызывать <see cref="IEFPConfigManager.Preload(EFPConfigSectionInfo[], EFPConfigMode)"/>.
    /// </summary>
    /// <param name="configManager">Менеджер секций конфигурации, обычно <see cref="EFPApp.ConfigManager"/></param>
    public void ReadConfig(IEFPConfigManager configManager)
    {
      if (configManager == null)
        throw new ArgumentNullException("configManager");

      if (ConfigSectionName.Length == 0)
        return;

      _Categories.Clear();
      GetConfigCategories(_Categories, EFPConfigMode.Read, _ConfigInfoObject);

      for (int i = 0; i < _Categories.Count; i++)
      {
        EFPConfigSectionInfo configInfo = new EFPConfigSectionInfo(ConfigSectionName, _Categories[i]);
        CfgPart cfg;
        using (configManager.GetConfig(configInfo, EFPConfigMode.Read, out cfg))
        {
          ReadConfigPart(_Categories[i], cfg, _ConfigInfoObject);
        }
      }

      Changed.Clear();
    }


    /// <summary>
    /// Вызывает <see cref="WriteConfigPart(string, CfgPart, EFPConfigActionInfo)"/> для всех категорий, несмотря на флажки. Флажки не сбрасываются.
    /// Для каждой категории создается вложенная секция с именем, равным категории.
    /// Используется при сохранении композиции рабочего стола.
    /// Работа метода не зависит от свойства <see cref="ConfigSectionName"/>.
    /// </summary>
    /// <param name="cfg">Секция для создания дочерних элементов</param>
    public void WriteComposition(CfgPart cfg)
    {
      if (cfg == null)
        throw new ArgumentNullException("cfg");


      _Categories.Clear();
      GetConfigCategories(_Categories, EFPConfigMode.Write, _CompositionInfoObject);

      for (int i = 0; i < _Categories.Count; i++)
      {
        CfgPart cfgChild = cfg.GetChild(_Categories[i], true);

        WriteConfigPart(_Categories[i], cfgChild, _CompositionInfoObject);
      }
    }


    /// <summary>
    /// Вызывает <see cref="ReadConfigPart(string, CfgPart, EFPConfigActionInfo)"/> для всех категорий.
    /// Для каждой категории используется вложенная секция с именем, равным категории.
    /// Используется при восстановлении композиции рабочего стола. Флажки сбрасываются.
    /// Работа метода не зависит от свойства <see cref="ConfigSectionName"/>.
    /// </summary>
    /// <param name="cfg">Секция для создания дочерних элементов</param>
    public void ReadComposition(CfgPart cfg)
    {
      if (cfg == null)
        throw new ArgumentNullException("cfg");

      _Categories.Clear();
      GetConfigCategories(_Categories, EFPConfigMode.Read, _CompositionInfoObject);

      for (int i = 0; i < _Categories.Count; i++)
      {
        CfgPart cfgChild = cfg.GetChild(_Categories[i], false);
        if (cfgChild == null)
          cfgChild = CfgPart.Empty; // вызывать ReadConfigPart() все равно нужно, иначе источник окажется в некомплектном состоянии
        ReadConfigPart(_Categories[i], cfgChild, _CompositionInfoObject);
      }
    }

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Добавляет в список секции конфигурации для последующей предварительной загрузки
    /// </summary>
    /// <param name="configInfos">Заполняемый список</param>
    /// <param name="rwMode">Предополагаемый режим работы - чтение или запись секций</param>
    public void GetPreloadConfigSections(ICollection<EFPConfigSectionInfo> configInfos, EFPConfigMode rwMode)
    {
      if (ConfigSectionName.Length == 0)
        return;

      _Categories.Clear();
      GetConfigCategories(_Categories, rwMode, _PreloadInfoObject);

      for (int i = 0; i < _Categories.Count; i++)
      {
        EFPConfigSectionInfo configInfo = new EFPConfigSectionInfo(ConfigSectionName, _Categories[i]);
        configInfos.Add(configInfo);
      }
    }

    #endregion
  }

  /// <summary>
  /// Подавляет чтение/запись секции конфигурации одной категории.
  /// Этот объект может быть добавлен к списку <see cref="EFPConfigHandler.Sources"/>.
  /// </summary>
  public sealed class EFPConfigCategorySuppressor : IEFPConfigurable
  {
    #region Конструктор

    /// <summary>
    /// Создает подавитель
    /// </summary>
    /// <param name="suppressedCategory">Категория, чтение/запись которой нужно запретить</param>
    public EFPConfigCategorySuppressor(string suppressedCategory)
    {
      if (String.IsNullOrEmpty(suppressedCategory))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("suppressedCategory");
      _SuppressedCategory = suppressedCategory;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Подавляемая категория
    /// </summary>
    public string SuppressedCategory { get { return _SuppressedCategory; } }
    private readonly string _SuppressedCategory;

    /// <summary>
    /// Возвращает отладочную информацию
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return "SuppressedCategory=" + SuppressedCategory;
    }

    #endregion

    #region IEFPConfigurable Members

    /// <summary>
    /// Удаляет из списка категорию <see cref="SuppressedCategory"/>.
    /// </summary>
    /// <param name="categories">Заполняеымый список</param>
    /// <param name="rwMode">Игнорируется</param>
    /// <param name="actionInfo">Игнорируется</param>
    public void GetConfigCategories(ICollection<string> categories, EFPConfigMode rwMode, EFPConfigActionInfo actionInfo)
    {
      categories.Remove(SuppressedCategory);
    }

    void IEFPConfigurable.WriteConfigPart(string category, CfgPart cfg, EFPConfigActionInfo actionInfo)
    {
    }

    void IEFPConfigurable.ReadConfigPart(string category, CfgPart cfg, EFPConfigActionInfo actionInfo)
    {
    }

    #endregion
  }
}
