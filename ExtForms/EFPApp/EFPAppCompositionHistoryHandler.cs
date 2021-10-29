using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using FreeLibSet.Config;
using System.Data;
using FreeLibSet.Core;

namespace FreeLibSet.Forms
{
  #region Перечисление EFPAppCompositionHistoryItemKind

  /// <summary>
  /// Возможные значения свойства EFPAppCompositionHistoryItem.Kind
  /// </summary>
  public enum EFPAppCompositionHistoryItemKind
  {
    /// <summary>
    /// Автоматически созданная запись в списке истории
    /// </summary>
    History,

    /// <summary>
    /// Именная запись, созданная пользователем
    /// </summary>
    User,
  }

  #endregion

  /// <summary>
  /// Информация о сохраненной композиции пользовательского интерфейса в списке истории.
  /// Объекты создаются в классе EFPAppCompositionHistoryHandler
  /// </summary>
  public sealed class EFPAppCompositionHistoryItem
  {
    #region Защищенный конструктор

    internal EFPAppCompositionHistoryItem(EFPAppCompositionHistoryItemKind kind,
      string userSetName, string displayName, DateTime time, string md5)
    {
#if DEBUG
      //if (String.IsNullOrEmpty(UserSetName))
      //  throw new ArgumentNullException("UserSetName");
      if (String.IsNullOrEmpty(displayName))
        throw new ArgumentNullException("displayName");
      if (md5.Length != 32)
        throw new ArgumentException("MD5", "md5");
#endif

      _Kind = kind;
      _UserSetName = userSetName;
      _DisplayName = displayName;
      _Time = time;
      _MD5 = md5;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Вид композиции: из списка истории или именная пользовательская
    /// </summary>
    public EFPAppCompositionHistoryItemKind Kind { get { return _Kind; } }
    private EFPAppCompositionHistoryItemKind _Kind;

    /// <summary>
    /// Внутреннее имя, например "Hist1", "User1".
    /// Может быть пустой строкой для последней сохраненной композиции
    /// </summary>
    public string UserSetName { get { return _UserSetName; } }
    private string _UserSetName;

    /// <summary>
    /// Отображаемое имя.
    /// Для именнной пользовательской композиции - заданное пользователем имя.
    /// Для композиции из истории - текст в виде "Последний", "Предпоследний", ...
    /// </summary>
    public string DisplayName { get { return _DisplayName; } }
    private string _DisplayName;

    /// <summary>
    /// Время сохранения интерфейса
    /// </summary>
    public DateTime Time { get { return _Time; } }
    private DateTime _Time;

    /// <summary>
    /// Контрольная сумма, используемая для сравнения композиций
    /// </summary>
    public string MD5 { get { return _MD5; } }
    private string _MD5;

    /// <summary>
    /// Возвращает свойство DisplayName.
    /// </summary>
    /// <returns>Текстовое представление</returns>
    public override string ToString()
    {
      return DisplayName;
    }

    #endregion
  }

  /// <summary>
  /// Вспомогательный класс для хранения истории сохранения композиций пользовательского интерфейса
  /// и именных сохраненных композиций.
  /// Используется в EFPApp.SaveComposition(), в диалоге выбора сохраненной композиции.
  /// Также может использоваться в пользовательском коде.
  /// </summary>
  public static class EFPAppCompositionHistoryHandler
  {
    #region Методы

    /// <summary>
    /// Возвращает список последних сохраненных композиций.
    /// Список отсортирован от новых к старым
    /// </summary>
    /// <returns>Массив описаний композиций</returns>
    public static EFPAppCompositionHistoryItem[] GetHistoryItems()
    {
      // Требуется промежуточная таблица для сортировки записей
      DataTable Table = new DataTable();
      Table.Columns.Add("UserSetName", typeof(string)); // "Hist1", "Hist2", ...
      Table.Columns.Add("Time", typeof(DateTime));
      Table.Columns.Add("MD5", typeof(string));
      DataTools.SetPrimaryKey(Table, "UserSetName");

      #region Чтение данных

      EFPConfigSectionInfo ConfigInfoHist = new EFPConfigSectionInfo(EFPApp.CompositionConfigSectionName,
        EFPConfigCategories.UIHistory, String.Empty);
      CfgPart cfgHist;
      using (EFPApp.ConfigManager.GetConfig(ConfigInfoHist, EFPConfigMode.Read, out cfgHist))
      {
        string[] Names = cfgHist.GetChildNames();

        for (int i = 0; i < Names.Length; i++)
        {
          if (Names[i].StartsWith("Hist"))
          {
            CfgPart cfgOne = cfgHist.GetChild(Names[i], false);
            /*DataRow Row = */ Table.Rows.Add(Names[i], cfgOne.GetDateTime("Time"),
              cfgOne.GetString("MD5"));
          }
        }
      }

      #endregion

      Table.DefaultView.Sort = "Time DESC"; // по убыванию

      #region Создание записей

      EFPAppCompositionHistoryItem[] Items = new EFPAppCompositionHistoryItem[Table.DefaultView.Count];
      for (int i = 0; i < Table.DefaultView.Count; i++)
      {
        DataRow Row = Table.DefaultView[i].Row;
        Items[i] = CreateHistItem(i, Row["UserSetName"].ToString(),
          (DateTime)(Row["Time"]),
          Row["MD5"].ToString());
      }

      #endregion

      return Items;
    }

    private static EFPAppCompositionHistoryItem CreateHistItem(int itemIndex, string userSetName, DateTime time, string md5)
    {
      string Prefix;
      switch (itemIndex)
      {
        case 0: Prefix = "Последняя"; break;
        case 1: Prefix = "Предпоследняя"; break;
        default: Prefix = "Предыдущая №" + (itemIndex + 1).ToString(); break;
      }

      return new EFPAppCompositionHistoryItem(EFPAppCompositionHistoryItemKind.History,
        userSetName,
        "[ " + Prefix + " " + time.ToString("g") + " ]",
        time, md5);
    }

    /// <summary>
    /// Возвращает список именных композиций.
    /// Список отсортирован по алфавиту (свойство DisplayName)
    /// </summary>
    /// <returns>Массив описаний композиций</returns>
    public static EFPAppCompositionHistoryItem[] GetUserItems()
    {
      // Требуется промежуточная таблица для сортировки записей
      DataTable Table = new DataTable();
      Table.Columns.Add("UserSetName", typeof(string)); // "User1", "User2", ...
      Table.Columns.Add("Name", typeof(string)); // имя, заданное пользователем
      Table.Columns.Add("Time", typeof(DateTime));
      Table.Columns.Add("MD5", typeof(string));
      DataTools.SetPrimaryKey(Table, "UserSetName");

      #region Чтение данных

      EFPConfigSectionInfo ConfigInfoHist = new EFPConfigSectionInfo(EFPApp.CompositionConfigSectionName,
        EFPConfigCategories.UIUserHistory, String.Empty);
      CfgPart cfgHist;
      using (EFPApp.ConfigManager.GetConfig(ConfigInfoHist, EFPConfigMode.Read, out cfgHist))
      {
        string[] Names = cfgHist.GetChildNames();

        for (int i = 0; i < Names.Length; i++)
        {
          if (Names[i].StartsWith("User"))
          {
            CfgPart cfgOne = cfgHist.GetChild(Names[i], false);
            /*DataRow Row = */Table.Rows.Add(Names[i], cfgOne.GetString("Name"),
              cfgOne.GetDateTime("Time"),
              cfgOne.GetString("MD5"));
          }
        }
      }

      #endregion

      Table.DefaultView.Sort = "Name";

      #region Создание записей

      EFPAppCompositionHistoryItem[] Items = new EFPAppCompositionHistoryItem[Table.DefaultView.Count];
      for (int i = 0; i < Table.DefaultView.Count; i++)
      {
        DataRow Row = Table.DefaultView[i].Row;
        Items[i] = CreateUserItem(Row["UserSetName"].ToString(),
          Row["Name"].ToString(),
          (DateTime)(Row["Time"]),
          Row["MD5"].ToString());
      }

      #endregion

      return Items;
    }

    private static EFPAppCompositionHistoryItem CreateUserItem(string userSetName, string name, DateTime time, string md5)
    {
      return new EFPAppCompositionHistoryItem(EFPAppCompositionHistoryItemKind.User,
        userSetName, name, time, md5);
    }

    /// <summary>
    /// Возвращает описание для пользовательской композиции с заданным именем.
    /// Если имя <paramref name="name"/> не задано или нет пользовательской конфигурации с таким
    /// именем (например, была удалена), возвращает null.
    /// </summary>
    /// <param name="name">Имя конфигурации, заданное пользователем. Не путать с UserSetName</param>
    /// <returns>Найденное описание или null</returns>
    public static EFPAppCompositionHistoryItem GetUserItem(string name)
    {
      if (String.IsNullOrEmpty(name))
        return null;

      #region Чтение данных

      EFPAppCompositionHistoryItem Item = null;

      EFPConfigSectionInfo ConfigInfoHist = new EFPConfigSectionInfo(EFPApp.CompositionConfigSectionName,
        EFPConfigCategories.UIUserHistory, String.Empty);
      CfgPart cfgHist;
      using (EFPApp.ConfigManager.GetConfig(ConfigInfoHist, EFPConfigMode.Read, out cfgHist))
      {
        string[] Names = cfgHist.GetChildNames();

        for (int i = 0; i < Names.Length; i++)
        {
          if (Names[i].StartsWith("User"))
          {
            CfgPart cfgOne = cfgHist.GetChild(Names[i], false);
            string UserName = cfgOne.GetString("Name");
            if (UserName == name)
            {
              Item = CreateUserItem(Names[i], UserName, cfgOne.GetDateTime("Time"),
                cfgOne.GetString("MD5"));
              break;
            }
          }
        }
      }

      #endregion

      return Item;
    }

    /// <summary>
    /// Получить описание секции конфигурации для выбранной сохраненной композиции.
    /// Полученный объект может быть передан методу EFPApp.ConfigManager.GetConfig() для загрузки данных
    /// </summary>
    /// <param name="item">Сохраненный элемент истории</param>
    /// <returns>Имя, категория и пользовательский набор секции конфигурации</returns>
    public static EFPConfigSectionInfo GetConfigInfo(EFPAppCompositionHistoryItem item)
    {
      switch (item.Kind)
      {
        case EFPAppCompositionHistoryItemKind.History:
          return new EFPConfigSectionInfo(EFPApp.CompositionConfigSectionName,
            EFPConfigCategories.UI, item.UserSetName);
        case EFPAppCompositionHistoryItemKind.User:
          return new EFPConfigSectionInfo(EFPApp.CompositionConfigSectionName,
            EFPConfigCategories.UIUser, item.UserSetName);
        default:
          throw new ArgumentException("Неизвестный Kind=" + item.Kind.ToString(), "item");
      }
    }


    /// <summary>
    /// Получить описание секции конфигурации со снимком для выбранной сохраненной композиции.
    /// Полученный объект может быть передан методу EFPApp.ConfigManager.GetConfig() для загрузки данных
    /// </summary>
    /// <param name="item">Сохраненный элемент истории</param>
    /// <returns>Имя, категория и пользовательский набор секции конфигурации</returns>
    public static EFPConfigSectionInfo GetSnapshotConfigInfo(EFPAppCompositionHistoryItem item)
    {
      switch (item.Kind)
      {
        case EFPAppCompositionHistoryItemKind.History:
          return new EFPConfigSectionInfo(EFPApp.CompositionConfigSectionName,
            EFPConfigCategories.UISnapshot, item.UserSetName);
        case EFPAppCompositionHistoryItemKind.User:
          return new EFPConfigSectionInfo(EFPApp.CompositionConfigSectionName,
            EFPConfigCategories.UIUserSnapshot, item.UserSetName);
        default:
          throw new ArgumentException("Неизвестный Kind=" + item.Kind.ToString(), "item");
      }
    }

    /// <summary>
    /// Сохранить композицию окна, полученную вызовом EFPAppInterface.SaveComposition().
    /// При записи проверяется наличие секции с такой MD5. Если уже есть такая секция, то она
    /// "продвигается" наверх списка.
    /// Этот метод вызывается в EFPApp.SaveComposition().
    /// </summary>
    /// <param name="cfg">Записанная секция</param>
    /// <param name="snapshot">Изображение</param>
    /// <returns>Описание композиции</returns>
    public static EFPAppCompositionHistoryItem SaveHistory(CfgPart cfg, Bitmap snapshot)
    {
      if (cfg == null)
        throw new ArgumentNullException("cfg");
      if (cfg.IsEmpty)
        throw new ArgumentException("Композиция не записана", "cfg");

      if (EFPApp.CompositionHistoryCount == 0)
        throw new InvalidOperationException("EFPApp.CompositionHistoryCount=0");

      string MD5 = cfg.MD5Sum();

      string UserSetName = null; // Название набора для секции истории. 
      // Если не надо добавлять секцию, остается null.

      EFPAppCompositionHistoryItem Item = null;

      #region Секция UIHistory

      EFPConfigSectionInfo ConfigInfoHist = new EFPConfigSectionInfo(EFPApp.CompositionConfigSectionName,
        EFPConfigCategories.UIHistory, String.Empty);
      CfgPart cfgHist;
      int CurrentIndex = -1; // поиск совпадения по MD5
      using (EFPApp.ConfigManager.GetConfig(ConfigInfoHist, EFPConfigMode.Write, out cfgHist))
      {
        string[] Names = cfgHist.GetChildNames();

        DataTable Table = new DataTable();
        Table.Columns.Add("UserSetName", typeof(string)); // "Hist1", "Hist2", ...
        Table.Columns.Add("Time", typeof(DateTime));
        Table.Columns.Add("MD5", typeof(string));
        DataTools.SetPrimaryKey(Table, "UserSetName");

        DateTime Time = DateTime.Now;

        for (int i = 0; i < Names.Length; i++)
        {
          if (Names[i].StartsWith("Hist"))
          {
            CfgPart cfgOne = cfgHist.GetChild(Names[i], false);
            DataRow Row = Table.Rows.Add(Names[i], cfgOne.GetDateTime("Time"),
              cfgOne.GetString("MD5"));
            if (Row["MD5"].ToString() == MD5)
            {
              CurrentIndex = i;
              cfgOne.SetDateTime("Time", Time); // просто заменяем
              Item = CreateHistItem(0, Names[i], Time, MD5);
            }
            // перебираем все break;
          }
        }

        if (CurrentIndex < 0)
        {
          // Требуется добавить новую запись в историю
          // Убираем самую старую секцию
          Table.DefaultView.Sort = "Time"; // по возрастанию
          while (Table.DefaultView.Count >= EFPApp.CompositionHistoryCount)
            Table.DefaultView[0].Row.Delete();

          // Придумываем новое имя секции
          for (int i = 1; i <= EFPApp.CompositionHistoryCount; i++)
          {
            string UserSetName2 = "Hist" + i.ToString();
            if (Table.Rows.Find(UserSetName2) == null)
            {
              UserSetName = UserSetName2;
              break;
            }
          }

          if (UserSetName == null)
            throw new BugException("Не нашли имени набора для записи истории");
          Table.Rows.Add(UserSetName, DateTime.Now, MD5);
          Table.AcceptChanges();

          // Заново записываем всю секцию
          cfgHist.Clear();
          foreach (DataRow Row in Table.Rows)
          {
            string UserSetName2 = Row["UserSetName"].ToString();
            CfgPart cfgOne = cfgHist.GetChild(UserSetName2, true);
            cfgOne.SetDateTime("Time", (DateTime)(Row["Time"]));
            cfgOne.SetString("MD5", Row["MD5"].ToString());
          }

          Item = CreateHistItem(0, UserSetName, Time, MD5);
        } // CurrentIndex<0
      } // SectHist

      #endregion

      #region Секция UI для истории

      if (UserSetName != null)
      {
        EFPConfigSectionInfo ConfigInfo2 = new EFPConfigSectionInfo(EFPApp.CompositionConfigSectionName,
          EFPConfigCategories.UI, UserSetName);
        CfgPart cfg2;
        using (EFPApp.ConfigManager.GetConfig(ConfigInfo2, EFPConfigMode.Write, out cfg2))
        {
          cfg2.Clear();
          cfg.CopyTo(cfg2);
        }
      }

      #endregion

      #region Snapshot

      // Записываем Snapshot, только если секция новая
      if (UserSetName != null)
      {
        try
        {
          EFPConfigSectionInfo ConfigInfoSnapshot = new EFPConfigSectionInfo(EFPApp.CompositionConfigSectionName,
            EFPConfigCategories.UISnapshot, UserSetName);
          EFPApp.SnapshotManager.SaveSnapshot(ConfigInfoSnapshot, snapshot);
        }
        catch (Exception e)
        {
          EFPApp.ShowException(e, "Не удалось сохранить изображение для предварительного просмотра");
        }
      }

      #endregion

#if DEBUG
      if (Item == null)
        throw new BugException("Item=null");
#endif

      return Item;
    }

    /// <summary>
    /// Сохранение именной пользователькой композиции.
    /// Если композиция с таким именем уже существует, она перезаписывается
    /// </summary>
    /// <param name="name">Имя композиции, заданное пользователем. Должно быть задано обязательно</param>
    /// <param name="cfg">Записанная секция</param>
    /// <param name="snapshot">Изображение</param>
    /// <returns>Описание композиции</returns>
    public static EFPAppCompositionHistoryItem SaveUser(string name, CfgPart cfg, Bitmap snapshot)
    {
      if (String.IsNullOrEmpty(name))
        throw new ArgumentNullException(name);
      if (cfg == null)
        throw new ArgumentNullException("cfg");
      if (cfg.IsEmpty)
        throw new ArgumentException("Композиция не записана", "cfg");

      string MD5 = cfg.MD5Sum();

      string UserSetName = null; // Название набора для секции истории. 

      EFPAppCompositionHistoryItem Item = null;

      #region Секция UIHistory

      EFPConfigSectionInfo ConfigInfoHist = new EFPConfigSectionInfo(EFPApp.CompositionConfigSectionName,
        EFPConfigCategories.UIUserHistory, String.Empty);
      CfgPart cfgHist;
      int CurrentIndex = -1; // поиск совпадения по имени
      using (EFPApp.ConfigManager.GetConfig(ConfigInfoHist, EFPConfigMode.Write, out cfgHist))
      {
        string[] Names = cfgHist.GetChildNames();

        DataTable Table = new DataTable();
        Table.Columns.Add("UserSetName", typeof(string)); // "User1", "User2", ...
        Table.Columns.Add("Name", typeof(string)); // пользовательское имя
        Table.Columns.Add("Time", typeof(DateTime));
        Table.Columns.Add("MD5", typeof(string));
        DataTools.SetPrimaryKey(Table, "UserSetName");

        DateTime Time = DateTime.Now;

        for (int i = 0; i < Names.Length; i++)
        {
          if (Names[i].StartsWith("User"))
          {
            CfgPart cfgOne = cfgHist.GetChild(Names[i], false);
            DataRow Row = Table.Rows.Add(Names[i],
              cfgOne.GetString("Name"),
              cfgOne.GetDateTime("Time"),
              cfgOne.GetString("MD5"));
            if (String.Equals(Row["Name"].ToString(), name, StringComparison.OrdinalIgnoreCase))
            {
              CurrentIndex = i;
              cfgOne.SetDateTime("Time", Time); // просто заменяем
              Item = CreateUserItem(Names[i], Row["Name"].ToString(), Time, MD5);
              UserSetName = Names[i];
            }
            // перебираем все break;
          }
        }

        if (CurrentIndex < 0)
        {
          // Придумываем новое имя секции
          for (int i = 1; i <= int.MaxValue; i++)
          {
            string UserSetName2 = "User" + i.ToString();
            if (Table.Rows.Find(UserSetName2) == null)
            {
              UserSetName = UserSetName2;
              break;
            }
          }
          if (UserSetName == null)
            throw new BugException("Не нашли имени набора для записи истории");

          Table.Rows.Add(UserSetName, name, DateTime.Now, MD5);
          Table.AcceptChanges();

          // Заново записываем всю секцию
          cfgHist.Clear();
          foreach (DataRow Row in Table.Rows)
          {
            string UserSetName2 = Row["UserSetName"].ToString();
            CfgPart cfgOne = cfgHist.GetChild(UserSetName2, true);
            cfgOne.SetString("Name", Row["Name"].ToString());
            cfgOne.SetDateTime("Time", (DateTime)(Row["Time"]));
            cfgOne.SetString("MD5", Row["MD5"].ToString());
          }

          Item = CreateUserItem(UserSetName, name, Time, MD5);
        } // CurrentIndex<0
      } // SectHist

      #endregion

      #region Секция UI для истории

      EFPConfigSectionInfo ConfigInfo2 = new EFPConfigSectionInfo(EFPApp.CompositionConfigSectionName,
        EFPConfigCategories.UIUser, UserSetName);
      CfgPart cfg2;
      using (EFPApp.ConfigManager.GetConfig(ConfigInfo2, EFPConfigMode.Write, out cfg2))
      {
        cfg2.Clear();
        cfg.CopyTo(cfg2);
      }

      #endregion

      #region Snapshot

      // Записываем Snapshot, только если секция новая
      try
      {
        EFPConfigSectionInfo ConfigInfoSnapshot = new EFPConfigSectionInfo(EFPApp.CompositionConfigSectionName,
          EFPConfigCategories.UIUserSnapshot, UserSetName);
        EFPApp.SnapshotManager.SaveSnapshot(ConfigInfoSnapshot, snapshot);
      }
      catch (Exception e)
      {
        EFPApp.ShowException(e, "Не удалось сохранить изображение для предварительного просмотра");
      }

      #endregion

#if DEBUG
      if (Item == null)
        throw new BugException("Item=null");
#endif

      return Item;
    }

    /// <summary>
    /// Удаляет сохраненную композицию из истории или именную пользовательскую композицию.
    /// </summary>
    /// <param name="item">Описание сохраненной композиции</param>
    public static void Delete(EFPAppCompositionHistoryItem item)
    {
#if DEBUG
      if (item == null)
        throw new ArgumentNullException("item");
#endif

      EFPConfigSectionInfo ConfigInfoHist;
      CfgPart cfgHist;
      switch (item.Kind)
      {
        case EFPAppCompositionHistoryItemKind.History:
          ConfigInfoHist = new EFPConfigSectionInfo(EFPApp.CompositionConfigSectionName,
          EFPConfigCategories.UIHistory, String.Empty);
          using (EFPApp.ConfigManager.GetConfig(ConfigInfoHist, EFPConfigMode.Write, out cfgHist))
          {
            cfgHist.Remove(item.UserSetName);
          }
          break;

        case EFPAppCompositionHistoryItemKind.User:
          ConfigInfoHist = new EFPConfigSectionInfo(EFPApp.CompositionConfigSectionName,
          EFPConfigCategories.UIUserHistory, String.Empty);
          using (EFPApp.ConfigManager.GetConfig(ConfigInfoHist, EFPConfigMode.Write, out cfgHist))
          {
            cfgHist.Remove(item.UserSetName);
          }
          break;

        default:
          throw new ArgumentException("Неизвестный Kind=" + item.Kind.ToString(), "item");
      }
    }

    #endregion
  }
}
