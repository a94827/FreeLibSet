using AgeyevAV.Config;
using System;
using System.Collections.Generic;
using System.Text;

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
  #region ПеречислениеEFPConfigMode

  /// <summary>
  /// Зачем выполняется загрузка секции конфигурации - для чтения или для записи
  /// </summary>
  public enum EFPConfigMode
  {
    /// <summary>
    /// Выполняется загрузка данных в управляющий элемент.
    /// </summary>
    Read = 0,

    /// <summary>
    /// Выполняется сохранение изменений, внесенных пользователем
    /// </summary>
    Write = 1
  }

  #endregion

  #region Перечисление EFPConfigPersistence

  /// <summary>
  /// Способ хранения настроек пользователя.
  /// Значение, возвращаемое свойством IEFPConfigManager.Persistence
  /// </summary>
  public enum EFPConfigPersistence
  { 
    /// <summary>
    /// Настройки не сохраняются даже в течение сеанса работы программы.
    /// Это значение возвращается EFPDummyConfigManager
    /// </summary>
    None,

    /// <summary>
    /// Настройки сохраняются только на время работы программы, а при завершении работы - удаляются.
    /// Это значение возвращается EFPRuntimeOnlyConfigManager
    /// </summary>
    Runtime,

    /// <summary>
    /// Настройки сохраняются между сеансами работы программы.
    /// Настройки либо храняется локально на компьютере, либо доступны только для текущего компьютера.
    /// Это значение возвращается, например, EFPRegistryConfigManager
    /// </summary>
    Machine,

    /// <summary>
    /// Настройки сохраняются между сеансами работы программы.
    /// Программа является сетевой.
    /// Часть настроек может относиться к пользователю, независимо от того, с какого компьютера
    /// выполнен вход.
    /// Для определения того, какие секции относятся к пользователю в-целом, а какие применимы
    /// только для текущего компьютера, пользовательский код, реализуюзий интерфейс IEFPConfigManager,
    /// может использовать метод EFPConfigCategories.IsMachineDepended()
    /// </summary>
    Network,
  }

  #endregion

  /// <summary>
  /// Информация о предварительной загрузке одной секции конфигурации
  /// </summary>
  [Serializable]
  public class EFPConfigSectionInfo : IEquatable<EFPConfigSectionInfo>
  {
    #region Конструкторы

    /// <summary>
    /// Создание объекта.
    /// Эта версия конструктора позволяет задать свойство "UserSetName"
    /// </summary>
    /// <param name="configSectionName">Имя секции конфигурации для разделения данных разных объектов.
    /// Обязательный параметр</param>
    /// <param name="category">Категория сохраняемых данных ("Filters", "Order", ...).
    /// Обязательный параметр</param>
    /// <param name="userSetName">Название пользовательского набора данных. 
    /// Необязательный параметр</param>
    public EFPConfigSectionInfo(string configSectionName, string category, string userSetName)
    {
      if (String.IsNullOrEmpty(configSectionName))
        throw new ArgumentNullException("configSectionName");
      if (String.IsNullOrEmpty(category))
        throw new ArgumentNullException("category");

      _ConfigSectionName = configSectionName;
      _Category = category;
      if (userSetName == null)
        _UserSetName = String.Empty;
      else
        _UserSetName = userSetName;
    }

    /// <summary>
    /// Создание объекта.
    /// Эта версия конструктора не задает свойство "UserSetName"
    /// </summary>
    /// <param name="configSectionName">Имя секции конфигурации для разделения данных разных объектов.
    /// Обязательный параметр</param>
    /// <param name="category">Категория сохраняемых данных ("Filters", "Order", ...).
    /// Обязательный параметр</param>
    public EFPConfigSectionInfo(string configSectionName, string category)
      : this(configSectionName, category, String.Empty)
    {
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя секции конфигурации для разделения данных разных объектов. 
    /// Не может быть null или пустой строкой.
    /// </summary>
    public string ConfigSectionName { get { return _ConfigSectionName; } }
    private string _ConfigSectionName;

    /// <summary>
    /// Категория сохраняемых данных ("Filters", "Order", ...). 
    /// Модули ExtForms и ExtDBDocForms используют константы из EFPConfigCategories. 
    /// Пользовательский код может использовать собственные имена категорий. 
    /// Не может быть null или пустой строкой.
    /// </summary>
    public string Category { get { return _Category; } }
    private string _Category;

    /// <summary>
    /// Название пользовательского набора данных. 
    /// Также используется для хранения истории.
    /// Обычно, пустая строка
    /// </summary>
    public string UserSetName { get { return _UserSetName; } }
    private string _UserSetName;

    #endregion

    #region ToString()

    /// <summary>
    /// Текстовое представление в виде "ConfigSectionName|Category" или 
    /// "ConfigSectionName|Category|UserSetName".
    /// Можно использовать в качестве ключа в коллекциях.
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      string s = ConfigSectionName + "|" + Category;
      if (UserSetName.Length > 0)
        s += "|" + UserSetName;
      return s;
    }

    #endregion

    #region Методы сравнения

    /// <summary>
    /// Возвращает true, если объекты одинаковые.
    /// Сравниваются все свойства объекта.
    /// </summary>
    /// <param name="a">Первый сравниваемый объект</param>
    /// <param name="b">Второй сравниваемый объект</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator ==(EFPConfigSectionInfo a, EFPConfigSectionInfo b)
    {
      if (Object.ReferenceEquals(a, null) && Object.ReferenceEquals(b, null))
        return true;
      if (Object.ReferenceEquals(a, null) || Object.ReferenceEquals(b, null))
        return false;

      return String.Equals(a.ConfigSectionName, b.ConfigSectionName) &&
        String.Equals(a.Category, b.Category) &&
        String.Equals(a.UserSetName, b.UserSetName);
    }

    /// <summary>
    /// Возвращает true, если объекты разные.
    /// Сравниваются все свойства объекта.
    /// </summary>
    /// <param name="a">Первый сравниваемый объект</param>
    /// <param name="b">Второй сравниваемый объект</param>
    /// <returns>Результат сравнения</returns>
    public static bool operator !=(EFPConfigSectionInfo a, EFPConfigSectionInfo b)
    {
      return !(a == b);
    }

    /// <summary>
    /// Возвращает true, если объекты одинаковые.
    /// Сравниваются все свойства объекта.
    /// </summary>
    /// <param name="obj">Второй сравниваемый объект</param>
    /// <returns>Результат сравнения</returns>
    public override bool Equals(object obj)
    {
      EFPConfigSectionInfo b = obj as EFPConfigSectionInfo;
      return this == b;
    }

    /// <summary>
    /// Возвращает true, если объекты одинаковые.
    /// Сравниваются все свойства объекта.
    /// </summary>
    /// <param name="obj">Второй сравниваемый объект</param>
    /// <returns>Результат сравнения</returns>
    public bool Equals(EFPConfigSectionInfo obj)
    {
      return this == obj;
    }

    /// <summary>
    /// Возвращает ConfigSectionName.GetHashCode()
    /// </summary>
    /// <returns>Хэш-код</returns>
    public override int GetHashCode()
    {
      return ConfigSectionName.GetHashCode();
    }

    #endregion
  }

  #region IEFPConfigManager

  /// <summary>
  /// Интефрейс для записи и чтения конфигурационных данных.
  /// Если программа хранит настройки в базе данных, должна быть создана реаоизация интефрейса.
  /// Для локального хранения настроек может быть использован, например, готовый класс EFPRegistryConfigManager
  /// </summary>
  public interface IEFPConfigManager
  {
    /// <summary>
    /// Получить секцию конфигурации для чтения или записи.
    /// Значение, полученное после вызова метода должно быть удалено по завершении чтения или записи
    /// вызовом метода Dispose() для полученного результата метода (не аргумента <paramref name="cfg"/>!).
    /// Удобно использовать конструкцию using.
    /// </summary>
    /// <param name="configInfo">Данные секции конфигурации. Обязательный параметр</param>
    /// <param name="rwMode">Режим работы (Read или Write)</param>
    /// <param name="cfg">Выход: Требуемая секция конфигурации</param>
    /// <returns>Некие внутренние данные, используемые реализацией менеджера</returns>
    IDisposable GetConfig(EFPConfigSectionInfo configInfo, EFPConfigMode rwMode, out CfgPart cfg);


    /// <summary>
    /// Свойство возвращает "сохранность" настроек.
    /// Менеджер EFPDummyConfigManager, используемый по умолчанию, возвращает None
    /// </summary>
    EFPConfigPersistence Persistence { get; }

    /// <summary>
    /// Упреждающая загрузка секций.
    /// Этот метод вызывается при показе формы и в других случаях, когда ожидается обращение к большому
    /// числу секций конфигурации.
    /// Если программа хранит данные на сервере, имеет смысл сделать реализацию этого метода для
    /// размещения секция в памяти клиента. Это позволает уменьшить количество последующих сетевых
    /// обращений.
    /// Для простых реализаций, например, хранящих настройки в реестре Windows, этот метод может
    /// ничего не делать.
    /// </summary>
    /// <param name="configInfos">Описания секций конфигурации для загрузки</param>
    /// <param name="rwMode">Предстоящий режим доступа (чтение или запись)</param>
    void Preload(EFPConfigSectionInfo[] configInfos, EFPConfigMode rwMode);
  }

  #endregion

  /// <summary>
  /// Класс содержит строковые константы, используемые при вызове метода IEFPConfigManager.GetConfig()
  /// для параметра Category.
  /// Перечислены все категории, используемые модулями ExtForms и ExtDBDocForms
  /// Пользовательский код может использовать собственные категории с другими именами
  /// Пользовательский код не может изменить значения констант. Если требуется хранить данные под другими именами,
  /// следует создать собственную реализацию IEFPConfigManager
  /// </summary>
  public static class EFPConfigCategories
  {
    #region Константы

    /// <summary>
    /// Используется EFPConfigurableDataGridView и EFPDataTreeViewWithFilters для хранения текущего фильтра 
    /// установленного пользователем (при условии, что пользователь имеет права на установку фильтров).
    /// Содержит по одной дочерней CfgPart для каждого фильтра, включая пустые.
    /// Также используется для хранения истории фильтров (если предусмотрено) с использованием UserSetName
    /// </summary>
    public const string Filters = "Filters";

    /// <summary>
    /// Используется EFPConfigurableDataGridView для хранения списка истории фильтров
    /// </summary>
    public const string FiltersHistory = "FiltersHistory";

    /// <summary>
    /// Используется EFPDataGridViewWithOrders для хранения выбранного порядка сортировки (строкое значение "Order")
    /// и выбранной настройки табличного просмотра (строковое значение "GridConfig")
    /// Для хранения текущих настроек фильтра используется отдельная секция Filters, а для 
    /// хранения размеров столбцов и их наличия - GridConfigs
    /// </summary>
    public const string GridView = "GridView";

    /// <summary>
    /// Используется в классах, производных от EFPTreeViewAdv для хранения выбранной настройки просмотра (строковое значение "GridConfig")
    /// Для хранения размеров столбцов и их наличия используется отдельная секция GridConfigs
    /// </summary>
    public const string TreeView = "TreeView";

    /// <summary>
    /// Используется для хранения пользовательских "именных" настроек табличного (или древовидного) просмотра (объектов EFPDataGridViewConfig)
    /// Имя настройки задается как UserSetName.
    /// Настройки могут относиться к таблицам EFPDBxGridView и EFPDBxTreeView
    /// Текущая выбранная настройка хранится как поле "GridConfig" в отдельной секции GridView или TreeView
    /// </summary>
    public const string GridConfig = "GridConfig";

    /// <summary>
    /// Используется EFPConfigurableDataGridView для хранения списка истории настроек
    /// </summary>
    public const string GridConfigHistory = "GridConfigHistory";

    /// <summary>
    /// Используется EFPConfigurableDataGridView для хранения настроек отправки в Excel и OpenOffice.
    /// Кроме секции с именем, задаваемым свойством ConfigSectionName, используется секция с именем "Default"
    /// </summary>
    public const string SendToExcel = "SendToExcel";

    /// <summary>
    /// Хранение параметров отчета.
    /// Вложенные секции и значения определяются прикладным модулем, реализующим отчет.
    /// Секция без указанного UserSetName задает последний набор параметров отчета, установленных пользоваталем.
    /// Секции с UserSetName используются для хранения истории и "именных" настроек, сохраненным пользователем
    /// Секция без указанного UserSetName задает также режим отображения отчета (поле "Maximized").
    /// 
    /// При использовании EFPReportExtParams эта секция используется для параметров 
    /// EFPReportExtParamsPart.User и EFPReportExtParamsPart.NoHistory.
    /// </summary>
    public const string ReportParams = "ReportParams";

    /// <summary>
    /// Используется для хранения истории параметров отчета EFPReportExtParams.
    /// </summary>
    public const string ReportHistory = "ReportHistory";

    /// <summary>
    /// Используется для хранения параметров отчета EFPReportExtParams для режима EFPReportExtParamsPart.Files.
    /// Менеджер хранения конфигураций для сетевого приложения может обрабатывать секции этой категории
    /// особым образом, чтобы данные привязывались не только к пользователю, но и компьютеру
    /// </summary>
    public const string ReportFiles = "ReportFiles";

    /// <summary>
    /// Хранение произвольных пользовательских параметров.
    /// Используется по умолчанию в EFPConfigParamSetComboBox
    /// </summary>
    public const string UserParams = "UserParams";

    /// <summary>
    /// Хранение истории произвольных пользовательских параметров.
    /// Используется по умолчанию в EFPConfigParamSetComboBox
    /// </summary>
    public const string UserHistory = "UserHistory";

    /// <summary>
    /// Параметры формы, например, видимость отдельных элементов.
    /// Для сетевой программы эти настройки, в отличие от секции "UI", относятся к пользователю в-целом.
    /// </summary>
    public const string Form = "Form";

    /// <summary>
    /// Размеры и положение формы на экране.
    /// Для сетевой программы эти настройки должны быть привязаны к компьютеру
    /// </summary>
    public const string FormBounds = "FormBounds";

    /// <summary>
    /// Хранит видимость панелей инструментов и статусной строки.
    /// В сетевой программе эти настройки относятся к пользователю в-целом, без привязки к компьютеру.
    /// </summary>
    public const string MainWindow = "MainWindow";

    /// <summary>
    /// Композиция рабочего стола.
    /// Используется единственное имя секции конфигурации: "Composition".
    /// В сетевой программе эти настройки должны быть привязаны к компьютеру.
    /// В отличие от других настроек с историей, "именные" настройки хранятся в секциях отдельной категории "UIUser".
    /// Изображение для предварительного просмотра в окне выбора композиции хранится в секции с категорией "UISnapshot".
    /// </summary>
    public const string UI = "UI";

    /// <summary>
    /// Хранение списка истории композиций рабочего стола
    /// Используется единственное имя секции конфигурации: "Composition".
    /// В сетевой программе эти настройки должны быть привязаны к компьютеру.
    /// В отличие от других настроек с историей, список "именных" настроек пользователя хранится в секции отдельной категории "UIUserHistory".
    /// </summary>
    public const string UIHistory = "UIHistory";

    /// <summary>
    /// Пользовательские именные композиции рабочего стола.
    /// Используется единственное имя секции конфигурации: "Composition".
    /// В сетевой программе эти настройки относятся к пользователю в-целом, без привязки к компьютеру.
    /// Изображение для предварительного просмотра в окне выбора композиции хранится в секции с категорией "UIUserSnapshot".
    /// </summary>
    public const string UIUser = "UIUser";

    /// <summary>
    /// Список именных композиций рабочего стола. Сами композиции хранятся в секциях категории "UIUser"
    /// Используется единственное имя секции конфигурации: "Composition".
    /// В сетевой программе эти настройки относятся к пользователю в-целом, без привязки к компьютеру
    /// </summary>
    public const string UIUserHistory = "UIUserHistory";

    /// <summary>
    /// Хранение изображения интерфейса.
    /// Используется в паре с секцией "UI".
    /// В сетевой программе эти настройки должны быть привязаны к компьютеру.
    /// Отдельная секция нужна, чтобы:
    /// 1. Уменьшить размер загружаемых данных при вызове LoadInterface().
    /// 2. Изображение не входит в расчет контрольной суммы MD5, чтобы идентичное расположение окон
    /// не "занимало" строчку историю, если изображение не совпадает (например, при наличии часов в статусной строке)
    /// </summary>
    public const string UISnapshot = "UISnapshot";

    /// <summary>
    /// Хранение изображения интерфейса.
    /// Используется в паре с секцией "UIUser".
    /// В сетевой программе эти настройки относятся к пользователю в-целом, без привязки к компьютеру
    /// </summary>
    public const string UIUserSnapshot = "UIUserSnapshot";

    #endregion

    #region Методы

    /// <summary>
    /// Возвращает true, если данная категория настроек содержит данные, которые являются специфическими
    /// для данного компьютера (хранит пути к файлам и каталогам)
    /// </summary>
    /// <param name="category">Категория настроек</param>
    /// <returns>true, если данные должны раздельно для каждого компьютера</returns>
    public static bool IsMachineDepended(string category)
    {
      if (String.IsNullOrEmpty(category))
        throw new ArgumentNullException("category");
      switch (category)
      {
        case ReportFiles:
        case UI:
        case UIHistory:
        case UISnapshot:
        case FormBounds:
          return true;
        default:
          return false;
      }
    }

    #endregion
  }

  /// <summary>
  /// Менеджер-заглушка для "сохранения" конфигурации.
  /// Ничего не сохраняет, каждый раз возвращает секцию-пустышку
  /// Используется свойством EFPApp.ConfigManager в качестве значения по умолчанию
  /// </summary>
  public sealed class EFPDummyConfigManager : IEFPConfigManager
  {
    #region IEFPConfigManager members

    /// <summary>
    /// Возвращает новый объект TempCfg
    /// </summary>
    /// <param name="configInfo">Не используется</param>
    /// <param name="rwMode">Не используется</param>
    /// <param name="cfg">Сюда записывается секция конфигурации</param>
    /// <returns>Фиктивный объект DisposableObject</returns>
    public IDisposable GetConfig(EFPConfigSectionInfo configInfo, EFPConfigMode rwMode, out CfgPart cfg)
    {
      cfg = new TempCfg();
      return new SimpleDisposableObject(); // 03.01.2021
    }

    /// <summary>
    /// Возвращает None
    /// </summary>
    public EFPConfigPersistence Persistence { get { return EFPConfigPersistence.None; } }
       
    /// <summary>
    /// Ничего не делает
    /// </summary>
    /// <param name="configInfos">Не используется</param>
    /// <param name="rwMode">Не используется</param>
    public void Preload(EFPConfigSectionInfo[] configInfos, EFPConfigMode rwMode)
    {
    }

    #endregion
  }

  /// <summary>
  /// Менеджер-заглушка для сохранения конфигурации в течение сеанса работы
  /// Содержит коллекцию временных секций конфигурации
  /// </summary>
  public class EFPRuntimeOnlyConfigManager : IEFPConfigManager
  {
    #region Конструктор

    /// <summary>
    /// Создает менеджер.
    /// </summary>
    public EFPRuntimeOnlyConfigManager()
    {
      _Dict = new Dictionary<EFPConfigSectionInfo, TempCfg>();
    }

    private Dictionary<EFPConfigSectionInfo, TempCfg> _Dict;

    #endregion

    #region IEFPConfigManager members

    /// <summary>
    /// Возвращает секцию TempCfg из внутреннего словаря. При необходимости, новая секция добавляется в словарь
    /// </summary>
    /// <param name="configInfo">Информация о секции конфигурации. Используется для поиска в словаре</param>
    /// <param name="rwMode">Не используется</param>
    /// <param name="cfg">Сюда помещается секция конфигурации</param>
    /// <returns>Фиктивный объект DisposableObject</returns>
    public IDisposable GetConfig(EFPConfigSectionInfo configInfo, EFPConfigMode rwMode, out CfgPart cfg)
    {
      TempCfg Cfg2;
      if (!_Dict.TryGetValue(configInfo, out Cfg2))
      {
        Cfg2 = new TempCfg();
        _Dict.Add(configInfo, Cfg2);
      }
      cfg = Cfg2;
      return new SimpleDisposableObject(); // 03.01.2021
    }

    /// <summary>
    /// Возвращает Runtime
    /// </summary>
    public EFPConfigPersistence Persistence { get { return EFPConfigPersistence.Runtime; } }

    /// <summary>
    /// Ничего не делает
    /// </summary>
    /// <param name="configInfos">Не используется</param>
    /// <param name="rwMode">Не используется</param>
    public void Preload(EFPConfigSectionInfo[] configInfos, EFPConfigMode rwMode)
    {
    }

    #endregion
  }

  /// <summary>
  /// Менеджер для сохранения настроек пользователя в реестре Windows
  /// </summary>
  public class EFPRegistryConfigManager : IEFPConfigManager
  {
    #region Конструктор

    /// <summary>
    /// Создает менеджер
    /// </summary>
    /// <param name="keyName">Имя корневого раздела реестра</param>
    public EFPRegistryConfigManager(string keyName)
    {
      _KeyName = keyName;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Имя корневого раздела реестра.
    /// Обычно, это HKEY_CURRENT_USER/Имя приложения
    /// В нем будут создаваться подразделы для ConfigSectionName
    /// </summary>
    public string KeyName { get { return _KeyName; } }
    private string _KeyName;

    #endregion

    #region IEFPConfigManager members

    /// <summary>
    /// Создает объект RegistryCfg.
    /// </summary>
    /// <param name="configInfo">Информация о секции конфигурации.
    /// Определяет имя вложенного раздела реестра относительно заданного свойством KeyName</param>
    /// <param name="rwMode">Определяет режим доступа к разделу реестра: Чтение или запись</param>
    /// <param name="cfg">Сюда помещается созданный RegistryCfg.</param>
    /// <returns></returns>
    public IDisposable GetConfig(EFPConfigSectionInfo configInfo, EFPConfigMode rwMode, out CfgPart cfg)
    {
      string ThisKeyName = KeyName + "\\" + configInfo.ConfigSectionName + "\\" + configInfo.Category;
      if (String.IsNullOrEmpty(configInfo.UserSetName))
        ThisKeyName += "\\Default";
      else
        ThisKeyName += "\\UserSets\\" + configInfo.UserSetName;

      RegistryCfg RegCfg = new RegistryCfg(ThisKeyName, rwMode == EFPConfigMode.Read);
      cfg = RegCfg;
      return RegCfg;
    }

    /// <summary>
    /// Возвращает Machine
    /// </summary>
    public EFPConfigPersistence Persistence { get { return EFPConfigPersistence.Machine; } }

    /// <summary>
    /// Этот метод ничего не делает, т.к. упреждающая загрузка не нужна
    /// </summary>
    /// <param name="configInfos">Не используется</param>
    /// <param name="rwMode">Не используется</param>
    public void Preload(EFPConfigSectionInfo[] configInfos, EFPConfigMode rwMode)
    {
    }

    #endregion
  }

  internal static class EFPConfigTools
  {
    public static bool IsPersist(EFPConfigPersistence persistence)
    {
      switch (persistence)
      { 
        case EFPConfigPersistence.Machine:
        case EFPConfigPersistence.Network:
          return true;
        default:
          return false;
      }
    }
  }
}
