using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using FreeLibSet.Config;

#if XXX
namespace FreeLibSet.Forms
{
  /// <summary>
  /// Конфигурация уровней иерархии в просмотре EFPHieView.
  /// Хранит список отмеченных уровней и их порядок.
  /// Этот объект не привязан к управляющим элементам и должен хранится
  /// в параметрах отчета (классе, производном от ReportParams).
  /// Для редактирования используется класс HieViewLevelConfigHandler
  /// Текущая конфигурация хранится в таблице, доступ к которой возможен через
  /// свойство View
  /// </summary>
  public class EFPHieViewConfig
  {
#region Конструктор

    /// <summary>
    /// Создать настраиваемую конфигурацию уровней иерархии
    /// Полный список уровней должен быть заполнен в порядке сортировки уровней
    /// по умолчанию. Нулевым элементом коллекции должен быть наиболее вложенный
    /// уровень, а последним - самый внешний
    /// </summary>
    /// <param name="AllLevels">Полный список возможных уровней</param>
    public EFPHieViewConfig(ICollection<EFPHieViewLevel> AllLevels)
    {
      FAllLevels = new EFPHieViewLevel[AllLevels.Count];
      AllLevels.CopyTo(FAllLevels, 0);

      FTable = new DataTable();
      FTable.Columns.Add("LevelIndex", typeof(int)); // порядок в AllLevels
      FTable.Columns.Add("Flag", typeof(bool)); // Наличие флажка
      FTable.Columns.Add("Name", typeof(string)); // название
      FTable.Columns.Add("ImageKey", typeof(string)); // изображение для просмотра
      FTable.Columns.Add("FlagIsFixed", typeof(bool)); // true, если флажок нельзя сбрасывать
      FTable.Columns.Add("Visible", typeof(bool)); // false для скрытых строк
      FTable.Columns.Add("RowOrder", typeof(int)); // порядок следования уровней
      FTable.Columns.Add("DefRowOrder", typeof(int)); // порядок следования только видимых уровней по умолчанию
      FTable.Columns.Add("RowOrderIsFixed", typeof(bool)); // true, если эту строку нельзя передвигать
      FTable.Columns.Add("ViolatedOrderRuleIndex", typeof(int)); // номер правила, которое было нарушено или DBNull

      DataTools.SetPrimaryKey(FTable, "LevelIndex");
      FTable.DefaultView.Sort = "RowOrder";
      FTable.DefaultView.RowFilter = "Visible=1";

      for (int i = 0; i < AllLevels.Count; i++)
      {
        DataRow Row = FTable.NewRow();
        Row["LevelIndex"] = i;
        FTable.Rows.Add(Row);
      }

      FOrderRules = new OrderRuleList(this);

      RefreshTable();
      SetDefaultConfig();
    }

#endregion

#region Свойства

    /// <summary>
    /// Список всех возможных уровней иерархии
    /// Задается в конструкторе. При сортировке по умолчанию AllLevels[0]
    /// соответствует самому вложенному уровню
    /// </summary>
    public EFPHieViewLevel[] AllLevels { get { return FAllLevels; } }
    private EFPHieViewLevel[] FAllLevels;

    /// <summary>
    /// Возвращает объект уровня из массива AllLevels по коду (свойству HieViewLevel.NamePart)
    /// Возвращает null, если уровень не найден
    /// </summary>
    /// <param name="LevelName">Имя уровня для поиска</param>
    /// <returns>Объект уровня или null</returns>
    public EFPHieViewLevel this[string LevelName]
    {
      get
      {
        for (int i = 0; i < AllLevels.Length; i++)
        {
          if (AllLevels[i].Name == LevelName)
            return AllLevels[i];
        }
        return null;
      }
    }

    /// <summary>
    /// Возвращает индекс заданного упровня иерерахии (LevelIndex) в массиве
    /// всех уровней. Индекс не зависит от наличия выбранных уровней и порядка
    /// сортировки.
    /// </summary>
    /// <param name="LevelName">Имя уровня</param>
    /// <returns>Индекс в массиве AllLevels</returns>
    public int IndexOf(string LevelName)
    {
      for (int i = 0; i < AllLevels.Length; i++)
      {
        if (AllLevels[i].Name == LevelName)
          return i;
      }

      return -1;
    }

    /// <summary>
    /// ВОзвращает true, если хотя бы для одного уровня иерархии есть редактор параметра
    /// </summary>
    public bool HasParamEditor
    {
      get
      {
        for (int i = 0; i < AllLevels.Length; i++)
        {
          if (AllLevels[i].ParamEditor != null)
            return true;
        }
        return false;
      }
    }

    /// <summary>
    /// Список выбранных уровней в заданном порядке
    /// Нулевой элемент соответствует самому вложенному уровню (с номерами строк),
    /// последний элемент - самому внешнему, то есть в порядке, который использует
    /// свойство HieViewHandler.Levels
    /// </summary>
    public EFPHieViewLevel[] SelectedLevels
    {
      get
      {
        List<EFPHieViewLevel> lst = new List<EFPHieViewLevel>();
        foreach (DataRowView drv in View)
        {
          if (DataTools.GetBool(drv.Row, "Flag"))
          {
            int LevelIndex = DataTools.GetInt(drv.Row, "LevelIndex");
            lst.Insert(0, FAllLevels[LevelIndex]);
          }
        }
        return lst.ToArray();
      }
      set
      {
        if (value == null)
          value = new EFPHieViewLevel[0];
        for (int i = 0; i < FTable.Rows.Count; i++)
        {
          int Index = Array.IndexOf<EFPHieViewLevel>(value, AllLevels[i]);
          if (Index < 0)
            FTable.Rows[i]["Flag"] = false;
          else
          {
            FTable.Rows[i]["Flag"] = true;
            FTable.Rows[i]["RowOrder"] = value.Length - Index - 1;
          }
        }
      }
    }

    /// <summary>
    /// Список имен выбранных уровней иерархии в заданном порядке
    /// Свойство может быть использовано для передачи списка уровней серверу
    /// Нулевой элемент массива соответствует наиболее вложенному уровню
    /// </summary>
    public string[] SelectedNames
    {
      get
      {
        List<string> lst = new List<string>();
        foreach (DataRowView drv in View)
        {
          if (DataTools.GetBool(drv.Row, "Flag"))
          {
            int LevelIndex = DataTools.GetInt(drv.Row, "LevelIndex");
            lst.Insert(0, FAllLevels[LevelIndex].Name);
          }
        }
        return lst.ToArray();
      }
      set
      {
        if (value == null)
          value = DataTools.EmptyStrings;
        for (int i = 0; i < FTable.Rows.Count; i++)
        {
          int Index = Array.IndexOf<string>(value, AllLevels[i].Name);
          if (Index < 0)
            FTable.Rows[i]["Flag"] = false;
          else
          {
            FTable.Rows[i]["Flag"] = true;
            FTable.Rows[i]["RowOrder"] = value.Length - Index - 1;
          }
        }
      }
    }

    /// <summary>
    /// Альтернативный доступ к списку имен выбранных уровней (свойство SelectedNames)
    /// в виде строки с именами, разделенных запятыми
    /// Уровни задаются от наиболее вложенного к внешнему
    /// </summary>
    public string SelectedNamesCSV
    {
      get
      {
        return String.Join(",", SelectedNames);
      }
      set
      {
        if (String.IsNullOrEmpty(value))
          SelectedNames = DataTools.EmptyStrings;
        else
          SelectedNames = value.Split(',');
      }
    }

    /// <summary>
    /// Возвращает отображаемые названия DisplayName выбранных уровней SelectedLevels в виде
    /// строки, разделенной запятыми.
    /// Свойство может быть использовано в качестве заголовка поля "Hie_Text" при 
    /// инициализации столбцов табличного просмотра.
    /// Порядок элементов является обратным по отношению к SelectedLevels. То есть
    /// сначала идет самый внешний уровень иерархии, а потом - вложенные
    /// </summary>
    public string SelectedLevelsTitle
    {
      get
      {
        List<string> lst = new List<string>();
        foreach (DataRowView drv in View)
        {
          if (DataTools.GetBool(drv.Row, "Flag"))
          {
            int LevelIndex = DataTools.GetInt(drv.Row, "LevelIndex");
            lst.Add(FAllLevels[LevelIndex].DisplayName);
          }
        }
        return String.Join(", ", lst.ToArray());
      }
    }

    /// <summary>
    /// Возвращает количество выбранных уровней SelectedLevels.Length,
    /// но работает быстрее, чем обращение к SelectedLevels
    /// </summary>
    public int SelectedLevelCount
    {
      get
      {
        int cnt = 0;
        foreach (DataRowView drv in View)
        {
          if (DataTools.GetBool(drv.Row, "Flag"))
            cnt++;
        }
        return cnt;
      }
    }

    /// <summary>
    /// Таблица, используемая в редакторе уровней.
    /// Список строк соответствует текущему порядку уровней (поле "RowOrder").
    /// В отличие от списка уровней, в таблиц порядок строк обратный. Первой строкой
    /// идет самый внешний уровень, а последней - самый внутренний. Строка "Итого"
    /// в таблицу не входит
    /// </summary>
    public DataView View { get { return FTable.DefaultView; } }
    private DataTable FTable;

    /// <summary>
    /// true, если в иерархическом просмотре должны быть скрыты лишние строки сумм
    /// По умолчанию - false
    /// </summary>
    public bool HideExtraSumRows;

    /// <summary>
    /// Если свойство установлено, то пользователь не может изменять свойство
    /// HideExtraSumRows в редакторе. Оно также не будет читаться и записываться
    /// в секции конфигурации
    /// По умолчанию - false (можно редактировать)
    /// Установка свойства должна выполняться до вызова ReadConfig() и до создания
    /// редактора
    /// </summary>
    public bool HideExtraSumRowsFixed;

    /// <summary>
    /// Массив уровней, у которых выставлены признаки Required и Visible, но которые
    /// не выбраны (ошибки)
    /// </summary>
    public EFPHieViewLevel[] UnselectedRequiredLevels
    {
      get
      {
        List<EFPHieViewLevel> lvls = new List<EFPHieViewLevel>();
        foreach (DataRowView drv in View)
        {
          if (DataTools.GetBool(drv.Row, "Visible") &&
            DataTools.GetBool(drv.Row, "FlagIsFixed"))
          {
            if (DataTools.GetBool(drv.Row, "Flag"))
              continue;

            int LevelIndex = DataTools.GetInt(drv.Row, "LevelIndex");
            lvls.Add(FAllLevels[LevelIndex]);
          }
        }
        return lvls.ToArray();
      }
    }

    public string UnselectedRequiredLevelsTitle
    {
      get
      {
        EFPHieViewLevel[] a1 = UnselectedRequiredLevels;
        string[] a2 = new string[a1.Length];
        for (int i = 0; i < a1.Length; i++)
          a2[i] = a1[i].DisplayName;
        return String.Join(", ", a2);
      }
    }

#endregion

#region Методы

    public void RefreshTable()
    {
      int cntVisible = 0;
      for (int i = FAllLevels.Length - 1; i >= 0; i--)
      {
        EFPHieViewLevel lvl = FAllLevels[i];
        DataRow Row = FTable.Rows[i];
        string s = lvl.DisplayName;
        if (lvl.ParamEditor != null)
          s += ": " + lvl.ParamEditor.GetText(lvl);
        Row["Name"] = s;
        Row["ImageKey"] = lvl.ImageKey;
        Row["FlagIsFixed"] = lvl.Requred || (!lvl.Visible);
        if (lvl.Requred)
          Row["Flag"] = true;
        if (!lvl.Visible)
          Row["Flag"] = false;
        Row["Visible"] = lvl.Visible;
        Row["RowOrderIsFixed"] = lvl.Position != EFPHieViewLevelPosition.Normal;
        if (lvl.Visible)
        {
          cntVisible++;
          Row["DefRowOrder"] = cntVisible;
        }
        else
          Row["DefRowOrder"] = 0;

      }

      CheckOrderRules();
    }

    /// <summary>
    /// Заполнить поля ViolatedOrderRuleIndex
    /// </summary>
    public void CheckOrderRules()
    {
      foreach (DataRow Row in FTable.Rows)
        Row["ViolatedOrderRuleIndex"] = DBNull.Value;

      for (int i = 0; i < OrderRules.Count; i++)
      {
        OrderRules[i].FViolated = false;
        DataRow Row1 = FTable.Rows.Find(OrderRules[i].UpperLevelIndex);
        DataRow Row2 = FTable.Rows.Find(OrderRules[i].LowerLevelIndex);
        if (!DataTools.GetBool(Row1, "Flag"))
          continue;
        if (!DataTools.GetBool(Row2, "Flag"))
          continue;

        int RowOrder1 = DataTools.GetInt(Row1, "RowOrder");
        int RowOrder2 = DataTools.GetInt(Row2, "RowOrder");

        if (RowOrder1 < RowOrder2)
          continue;

        // Устанавливаем признак ошибки для обеих строк
        Row1["ViolatedOrderRuleIndex"] = i;
        Row2["ViolatedOrderRuleIndex"] = i;
        OrderRules[i].FViolated = true;
      }
    }

    /// <summary>
    /// Применить установленную конфигурацию к просмотру
    /// </summary>
    /// <param name="HieView"></param>
    public void CopyTo(EFPHieView HieView)
    {
      HieView.Levels = SelectedLevels;
      HieView.HideExtraSumRows = HideExtraSumRows;
    }

#endregion

#region Правила для порядка уровней

    /// <summary>
    /// Определение одного правила порядка следования двух уровней
    /// </summary>
    public class OrderRule
    {
#region Конструктор

      /// <summary>
      /// Создает новое правило
      /// </summary>
      /// <param name="UpperLevelName">Уровень, который дожен быть выше</param>
      /// <param name="LowerLevelName">Уровень, который дожен быть ниже</param>
      public OrderRule(string UpperLevelName, string LowerLevelName)
      {
#if DEBUG
        if (String.IsNullOrEmpty(UpperLevelName))
          throw new ArgumentNullException("UpperLevelName");
        if (String.IsNullOrEmpty(LowerLevelName))
          throw new ArgumentNullException("LowerLevelName");
        if (UpperLevelName == LowerLevelName)
          throw new BugException("Верхний и нижний уровни не могут совпадать: " + UpperLevelName);
#endif

        FUpperLevelName = UpperLevelName;
        FLowerLevelName = LowerLevelName;
      }

#endregion

#region Свойства

      /// <summary>
      /// Имя уровня иерархии, который дожен быть выше.
      /// Свойство задается в конструкторе
      /// </summary>
      public string UpperLevelName { get { return FUpperLevelName; } }
      private string FUpperLevelName;

      /// <summary>
      /// Имя уровня иерархии, который дожен быть ниже.
      /// Свойство задается в конструкторе
      /// </summary>
      public string LowerLevelName { get { return FLowerLevelName; } }
      private string FLowerLevelName;

      internal int UpperLevelIndex;

      internal int LowerLevelIndex;

      /// <summary>
      /// Возвращает true, если правило нарушено
      /// </summary>
      public bool Violated { get { return FViolated; } }
      internal bool FViolated;

#endregion
    }

    /// <summary>
    /// Список правил следования уровней в иерархии
    /// </summary>
    public class OrderRuleList : List<OrderRule>
    {
#region Конструктор

      internal OrderRuleList(EFPHieViewConfig Owner)
      {
        FOwner = Owner;
      }

#endregion

#region Свойства

      private EFPHieViewConfig FOwner;

      /// <summary>
      /// Возвращает true, если хотя бы одно из правил нарушено
      /// </summary>
      public bool HasViolation
      {
        get
        {
          for (int i = 0; i < Count; i++)
          {
            if (this[i].Violated)
              return true;
          }
          return false;
        }
      }

#endregion

#region Методы
                  
      // TODO: !!! Это надо переделать

      public new void Add(OrderRule Rule)
      {
        if (Rule == null)
          return;

        int UpperLevelIndex = FOwner.IndexOf(Rule.UpperLevelName);
        int LowerLevelIndex = FOwner.IndexOf(Rule.LowerLevelName);
        if (UpperLevelIndex < 0 || LowerLevelIndex < 0)
          return;

        Rule.UpperLevelIndex = UpperLevelIndex;
        Rule.LowerLevelIndex = LowerLevelIndex;
        base.Add(Rule);
      }

      public void Add(string UpperLevelName, string LowerLevelName)
      {
        OrderRule Rule = new OrderRule(UpperLevelName, LowerLevelName);
        Add(Rule);
      }

#endregion
    }

    /// <summary>
    /// Список правил взаимного расположения уровней
    /// </summary>
    public OrderRuleList OrderRules { get { return FOrderRules; } }
    private OrderRuleList FOrderRules;

#endregion

#region Вспомогательные методы установки значений

    /// <summary>
    /// Устанавливает свойство "Visible" для уровня, если он существует
    /// </summary>
    /// <param name="LevelName">Имя уровня</param>
    /// <param name="Visible">Видимость уровня</param>
    public void SetVisible(string LevelName, bool Visible)
    {
      EFPHieViewLevel lvl = this[LevelName];
      if (lvl != null)
        lvl.Visible = Visible;
    }

#if XXX
    /// <summary>
    /// Устанавливает свойство "Visible" для уровня, если он существует.
    /// Может быть также сброшено свойство Selected, когда Visible=false,
    /// а UnselectInvisible=true
    /// </summary>
    /// <param name="LevelName">Имя уровня иерархии</param>
    /// <param name="Visible">Значение свойства Visible</param>
    /// <param name="UnselectInvisible">Если true, то будет установлено Selected=false при Visible=false</param>
    public void SetVisible(string LevelName, bool Visible, bool UnselectInvisible)
    {
      HieViewLevel lvl = this[LevelName];
      if (lvl != null)
      {
        lvl.Visible = Visible;
        if (UnselectInvisible && (!Visible))
          SetSelected(LevelName, false);
      }
    }
#endif

    /// <summary>
    /// Возвращает признак видимости уровня, если он существует
    /// </summary>
    /// <param name="LevelName">Имя уровня</param>
    /// <returns>Признак видимости</returns>
    public bool GetVisible(string LevelName)
    {
      EFPHieViewLevel lvl = this[LevelName];
      if (lvl != null)
        return lvl.Visible;
      else
        return false;
    }

    /// <summary>
    /// Устанавливает свойство "Required" для уровня, если он существует
    /// </summary>
    /// <param name="LevelName">Имя уровня</param>
    /// <param name="Required">Свойство "Required"</param>
    public void SetRequired(string LevelName, bool Required)
    {
      EFPHieViewLevel lvl = this[LevelName];
      if (lvl != null)
        lvl.Requred = Required;
    }

    /// <summary>
    /// Возвращает свойство "Required" для уровня, если он существует
    /// </summary>
    /// <param name="LevelName">Имя уровня</param>
    /// <returns>Свойство "Required"</returns>
    public bool GetRequired(string LevelName)
    {
      EFPHieViewLevel lvl = this[LevelName];
      if (lvl != null)
        return lvl.Requred;
      else
        return false;
    }

    public void SetSelected(string LevelName, bool Selected)
    {
      int LevelIndex = IndexOf(LevelName);
      if (LevelIndex < 0)
        return;
      DataRow Row = FTable.Rows.Find(LevelIndex);
      Row["Flag"] = Selected;
    }

    public bool GetSelected(string LevelName)
    {
      int LevelIndex = IndexOf(LevelName);
      if (LevelIndex < 0)
        return false;
      DataRow Row = FTable.Rows.Find(LevelIndex);
      return DataTools.GetBool(Row, "Flag");
    }

#endregion

#region Чтение и запись конфигурации

    /// <summary>
    /// Установка порядка строк и флажков в значение по умолчанию
    /// </summary>
    public void SetDefaultConfig()
    {
      for (int i = FAllLevels.Length - 1; i >= 0; i--)
      {
        EFPHieViewLevel lvl = FAllLevels[i];
        DataRow Row = FTable.Rows[i];
        Row["RowOrder"] = Row["DefRowOrder"];
        if ((!lvl.Requred) && lvl.Visible)
          Row["Flag"] = lvl.DefaultSelected;
      }

      if (!HideExtraSumRowsFixed)
        HideExtraSumRows = false;
    }

    public void WriteConfig(CfgPart Config)
    {
      if (Config == null)
        return;

      CfgPart PartFlag = Config.GetChild("HieLevels", true);
      CfgPart PartOrd = Config.GetChild("HieOrders", true);
      CfgPart PartParams = null;
      if (HasParamEditor)
        PartParams = Config.GetChild("HieLevelParams", true);
      for (int i = 0; i < AllLevels.Length; i++)
      {
        if (AllLevels[i].Visible)
        {
          DataRow Row = FTable.Rows[i];
          bool Flag = DataTools.GetBool(Row, "Flag");
          int Order = DataTools.GetInt(Row, "RowOrder");
          PartFlag.SetBool(AllLevels[i].Name, Flag);
          PartOrd.SetInt(AllLevels[i].Name, Order);
          if (AllLevels[i].ParamEditor != null)
          {
            CfgPart PartParam = PartParams.GetChild(AllLevels[i].Name, true);
            AllLevels[i].ParamEditor.WriteConfig(AllLevels[i], PartParam);
          }
        }
      }

      if (!HideExtraSumRowsFixed)
        Config.SetBool("HideExtraSubTotals", HideExtraSumRows);
    }

    public void ReadConfig(CfgPart Config)
    {
      if (Config == null)
      {
        SetDefaultConfig();
        return;
      }
      CfgPart PartFlag = Config.GetChild("HieLevels", false);
      CfgPart PartOrd = Config.GetChild("HieOrders", false);
      CfgPart PartParams = Config.GetChild("HieLevelParams", false);
      for (int i = 0; i < AllLevels.Length; i++)
      {
        if (AllLevels[i].Visible)
        {
          DataRow Row = FTable.Rows[i];
          bool Flag = DataTools.GetBool(Row, "Flag");
          int Order = DataTools.GetInt(Row, "RowOrder");
          if (PartFlag != null)
            PartFlag.GetBool(AllLevels[i].Name, ref Flag);
          if (PartOrd != null)
            PartOrd.GetInt(AllLevels[i].Name, ref Order);
          Row["Flag"] = Flag;
          Row["RowOrder"] = Order;
          if (AllLevels[i].ParamEditor != null)
          {
            // Вызвать метод чтения значения параметра надо, даже если для него
            // нет секции конфигурации. В этом случае используется пустая секция
            CfgPart PartParam = null;
            if (PartParams != null)
              PartParam = PartParams.GetChild(AllLevels[i].Name, false);
            if (PartParam == null)
              PartParam = new TempCfg();
            AllLevels[i].ParamEditor.ReadConfig(AllLevels[i], PartParam);
          }
        }
      }

      if (!HideExtraSumRowsFixed)
        HideExtraSumRows = Config.GetBool("HideExtraSubTotals");

      RefreshTable(); // мог поменяться текст для поля "Название"
    }

#endregion
  }

}
#endif

