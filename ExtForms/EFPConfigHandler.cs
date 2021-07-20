using System;
using System.Collections.Generic;
using System.Text;
using AgeyevAV.Config;

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


namespace AgeyevAV.ExtForms
{
  #region Перечисление EFPConfigPurpose

  /// <summary>
  /// Назначение действия, выполняемого EFPConfigHandler
  /// </summary>
  public enum EFPConfigPurpose
  {
    /// <summary>
    /// Чтение/запись именной секции конфигурации с именем, заданным свойством ConfigSectionName
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
  /// Дополнительная информация, передаваемая методам интерфейса IEFPConfigurable.
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
    /// Возвращает Purpose
    /// </summary>
    /// <returns>Тестовое представление</returns>
    public override string ToString()
    {
      return Purpose.ToString();
    }

    #endregion

    #region IReadOnlyObject Members

    /// <summary>
    /// Возвращает true, если объект был переведен в режим "только чтение" вызовом SetReadOnly().
    /// </summary>
    public bool IsReadOnly { get { return _IsReadOnly; } }
    private bool _IsReadOnly;

    /// <summary>
    /// Выбрасывает исключение, если IsReadOnly=true
    /// </summary>
    public void CheckNotReadOnly()
    {
      if (IsReadOnly)
        throw new ObjectReadOnlyException();
    }

    /// <summary>
    /// Переводит объект в режим "только чтение".
    /// Вызывается из EFPConfigHandler
    /// </summary>
    public void SetReadOnly()
    {
      _IsReadOnly = true;
    }

    #endregion
  }

  /// <summary>
  /// Интерфейс, реализуемый EFPControlBase, EFPFormProvider и другими классами, использующими EFPControlHandler.
  /// Определяет методы чтения / записи секций конфигурации и получения списка категорий
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
  /// Используется в EFPFormProvider и EFPControlBase.
  /// Не применяется для сохранения истории и пользовательских наборов, т.к не содержит свойсва UserSetName.
  /// 
  /// Реализация интерфейса IEFPConfigurable выполняет опрос присоединенных источников
  /// </summary>
  public sealed class EFPConfigHandler : IEFPConfigurable
  {
    #region Конструктор

    /// <summary>
    /// Создает новый объект без установки ConfigSectionName.
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
    /// EFPControlBase и EFPFormProvider добавляют себя в этот список сразу после создания EFPConfigHandler
    /// </summary>
    public IList<IEFPConfigurable> Sources { get { return _Sources; } }
    private List<IEFPConfigurable> _Sources;

    #endregion

    #region IEFPConfigurable Members

    /// <summary>
    /// Построение списка категорий.
    /// Опрашивает все источники данных в списке Sources.
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
    /// Вызывает методы всех объектов из списка Sources
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
    /// Вызывает методы всех объектов из списка Sources
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
    /// Для эмуляции именного индексированного свойства Changed.
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

      private EFPConfigHandler _Owner;

      /// <summary>
      /// Коллекция установленных флажков
      /// Ключ - категория
      /// Значение - не используется
      /// </summary>
      private Dictionary<string, object> _Flags;

      /// <summary>
      /// Чтение или установка признака изменения для категории
      /// Это свойство устанавливается, например, после изменения фильтров.
      /// Через некоторое время, по таймеру будет вызван метод ReadConfig().
      /// Если, например, поле CurrentId нужно записать в секцию "GridView" только при закрытии формы, 
      /// следует установить флаг в обработчике OnHidden() перед вызовом метода базового класса.
      /// Флажки можно задавать и для несуществующих категорий. Они игнорируются.
      /// Нужно, т.к. внешний код может убрать категорию из списка, вызвав Remove().
      /// </summary>
      /// <param name="category">Категория</param>
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
    private ChangeFlags _Changed;

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
    private SingleScopeStringList _Categories;

    /// <summary>
    /// Записывает все данные, независимо от флажков.
    /// Флажки сбрасываются.
    /// Если свойство ConfigSectionName не установлено, никаких действий не выполняется.
    /// Перед использованием этого метода рекомендуется вызывать IEFPConfigManager.Preload()
    /// </summary>
    /// <param name="configManager">Менеджер секций конфигурации EFPApp.ConfigManager</param>
    public void WriteConfig(IEFPConfigManager configManager)
    {
      if (configManager == null)
        throw new NullReferenceException("configManager");

      if (ConfigSectionName.Length == 0)
        return;

      _Categories.Clear();
      GetConfigCategories(_Categories, EFPConfigMode.Write, _ConfigInfoObject);

      for (int i = 0; i < _Categories.Count; i++)
      {
        EFPConfigSectionInfo ConfigInfo = new EFPConfigSectionInfo(ConfigSectionName, _Categories[i]);
        CfgPart cfg;
        using (configManager.GetConfig(ConfigInfo, EFPConfigMode.Write, out cfg))
        {
          WriteConfigPart(_Categories[i], cfg, _ConfigInfoObject);
        }
      }

      Changed.Clear();
    }

    /// <summary>
    /// Записывает только измененные данные и сбрасывает флажки   
    /// Если свойство ConfigSectionName не установлено, никаких действий не выполняется.
    /// </summary>
    /// <param name="configManager">Менеджер секций конфигурации EFPApp.ConfigManager</param>
    public void WriteConfigChanges(IEFPConfigManager configManager)
    {
      if (configManager == null)
        throw new NullReferenceException("ConfigManager");

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
          EFPConfigSectionInfo ConfigInfo = new EFPConfigSectionInfo(ConfigSectionName, _Categories[i]);
          CfgPart cfg;
          using (configManager.GetConfig(ConfigInfo, EFPConfigMode.Write, out cfg))
          {
            WriteConfigPart(_Categories[i], cfg, _ConfigInfoObject);
          }
        }
      }

      Changed.Clear();
    }


    /// <summary>
    /// Считывает данные для всех категорий, сбрасывает флажки
    /// Если свойство ConfigSectionName не установлено, никаких действий не выполняется.
    /// Перед использованием этого метода рекомендуется вызывать IEFPConfigManager.Preload()
    /// </summary>
    /// <param name="configManager">Менеджер секций конфигурации EFPApp.ConfigManager</param>
    public void ReadConfig(IEFPConfigManager configManager)
    {
      if (configManager == null)
        throw new NullReferenceException("configManager");

      if (ConfigSectionName.Length == 0)
        return;

      _Categories.Clear();
      GetConfigCategories(_Categories, EFPConfigMode.Read, _ConfigInfoObject);

      for (int i = 0; i < _Categories.Count; i++)
      {
        EFPConfigSectionInfo ConfigInfo = new EFPConfigSectionInfo(ConfigSectionName, _Categories[i]);
        CfgPart cfg;
        using (configManager.GetConfig(ConfigInfo, EFPConfigMode.Read, out cfg))
        {
          ReadConfigPart(_Categories[i], cfg, _ConfigInfoObject);
        }
      }

      Changed.Clear();
    }


    /// <summary>
    /// Вызывает Write для всех категорий, несмотря на флажки. Флажки не сбрасываются.
    /// Для каждой категории создается вложенная секция с именем, равным категории.
    /// Используется при сохранении композиции рабочего стола.
    /// Работа метода не зависит от свойства ConfigSectionName.
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
    /// Вызывает Read для всех категорий.
    /// Для каждой категории используется вложенная секция с именем, равным категории.
    /// Если какой-либо секции нет, событие Read не вызывается
    /// Используется при восстановлении композиции рабочего стола. Флажки сбрасываются.
    /// Работа метода не зависит от свойства ConfigSectionName.
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
        EFPConfigSectionInfo ConfigInfo = new EFPConfigSectionInfo(ConfigSectionName, _Categories[i]);
        configInfos.Add(ConfigInfo);
      }
    }

    #endregion
  }

  /// <summary>
  /// Подавляет чтение/запись секции конфигурации одной категории.
  /// Этот объект может быть добавлен к списку EFPConfigHandler.Sources
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
        throw new ArgumentNullException("suppressedCategory");
      _SuppressedCategory = suppressedCategory;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Подавляемая категория
    /// </summary>
    public string SuppressedCategory { get { return _SuppressedCategory; } }
    private string _SuppressedCategory;

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
    /// Удаляет из списка категорию SuppressedCategory
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
