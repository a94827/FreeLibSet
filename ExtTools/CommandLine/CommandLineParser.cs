// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using FreeLibSet.Collections;
using FreeLibSet.Core;

namespace FreeLibSet.CommandLine
{
  /// <summary>
  /// Использование действий в командной строке
  /// </summary>
  public enum CommandLineActionMode
  {
    /// <summary>
    /// Действия не используются.
    /// Аргументы без префиксов считаются обычными аргументами, а не командами
    /// Режим по умолчанию.
    /// </summary>
    None,

    /// <summary>
    /// Первый аргумент без префиксов является действиес, а остальные - обычными аргументами.
    /// </summary>
    FirstOnly,

    // Пока не надо
    ///// <summary>
    ///// Может быть несколько действий
    ///// </summary>
    //Multi,
  }

  /// <summary>
  /// Описатель действия командной строки.
  /// Описатели должны быть добавлены в список <see cref="CommandLineParser.ActionDefs"/>.
  /// В отличие от опций, действия не имеют префиксов "--", "-" или "/". Также действия не могут содержать значения.
  /// </summary>
  public sealed class CommandLineAction : IObjectWithCode
  {
    /// <summary>
    /// Создает описатель действия с одним или несколькими кодами
    /// </summary>
    /// <param name="actions">Строки действий, через запятую, например, "add,a"</param>
    public CommandLineAction(string actions)
    {
      if (String.IsNullOrEmpty(actions))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("actions");
      _Actions = actions.Split(',');
      for (int i = 0; i < _Actions.Length; i++)
      {
        if (!Regex.IsMatch(_Actions[i], "^[A-Za-z0-9]+$"))
          throw new ArgumentException(String.Format(Res.CommandLineParser_Arg_BadActionFormat, _Actions[i]), "actions");
      }
      _Code = _Actions[0];

      _Enabled = true;
    }

    /// <summary>
    /// Коды действий.
    /// Первый ключ в списке является основным и используется для поиска.
    /// </summary>
    public string[] Actions { get { return _Actions; } }
    private readonly string[] _Actions;

    /// <summary>
    /// Код для поиска действия
    /// </summary>
    public string Code { get { return _Code; } }
    private readonly string _Code;

    /// <summary>
    /// True (по умолчанию), если команда ожидается при разборе командной строки.
    /// Некоторые действия могут появляться в командной строке только при наличии определенных аргументов,
    /// поэтому действия могут выключаться и включаться динамически в процессе парсинга.
    /// </summary>
    public bool Enabled { get { return _Enabled; } set { _Enabled = value; } }
    private bool _Enabled;

    /// <summary>
    /// Сообщение об ошибке, используемое при <see cref="Enabled"/>=false.
    /// </summary>
    public string EnabledErrorMessage
    {
      get
      {
        if (String.IsNullOrEmpty(_EnabledErrorMessage))
        {
          if (Enabled)
            return String.Empty;
          else
            return Res.CommandLineParser_Err_ActionDisabled;
        }
        else
          return _EnabledErrorMessage;
      }
      set { _EnabledErrorMessage = value; }
    }
    private string _EnabledErrorMessage;
  }

  /// <summary>
  /// Необходимость задания значения после опции в виде "--опция=значение" или "--опция значение".
  /// </summary>
  public enum CommandLineOptionValueMode
  {
    /// <summary>
    /// Опция не предусматривает значения и может встречаться не более 1 раза.
    /// </summary>
    None,

    /// <summary>
    /// Опция обязательно должна содержать значение и может встречаться не более 1 раза.
    /// </summary>
    Single,

    /// <summary>
    /// Значение может быть задано, но не является обязательным. Опция может встречаться не более 1 раза.
    /// </summary>
    Optional,

    /// <summary>
    /// Опция может встречаться несколько раз, обязательно со значением
    /// </summary>
    Multi,
  }

  /// <summary>
  /// Описатель опции (option, switch) командной строки.
  /// Описатели должны быть добавлены в список <see cref="CommandLineParser.OptionDefs"/>.
  /// Опции начинаются с "--", "-" или "/" (в зависимости от свойства <see cref="CommandLineParser.SlashedOptionsEnabled"/>).
  /// </summary>
  public sealed class CommandLineOption : IObjectWithCode
  {
    /// <summary>
    /// Создает опцию с одним или несколькими вариатами задания в командной строке.
    /// </summary>
    /// <param name="options">Описатели опций, через запятую, например, "--list,-L"</param>
    public CommandLineOption(string options)
    {
      if (String.IsNullOrEmpty(options))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("switches");
      _Options = options.Split(',');
      for (int i = 0; i < _Options.Length; i++)
      {
        if (!Regex.IsMatch(_Options[i], "^[-][-]?[A-Za-z0-9]+$"))
          throw new ArgumentException(String.Format(Res.CommandLineParser_Arg_BadOptionFormat, _Options[i]), "options");
      }

      if (_Options[0].StartsWith("--", StringComparison.Ordinal))
        _Code = _Options[0].Substring(2);
      else
        _Code = _Options[0].Substring(1);

      _ValueMode = CommandLineOptionValueMode.None;
      _MultiValueSeparator = ',';
      _Values = new List<string>();

      _Enabled = true;
    }

    /// <summary>
    /// Коды ключей, начинающиеся с "--" (длинная форма) или "-" (короткая форма).
    /// Первый ключ в списке является основным и используется для поиска значений (без дефиса).
    /// </summary>
    public string[] Options { get { return _Options; } }
    private readonly string[] _Options;

    /// <summary>
    /// Код для поиска опции (без префиксов "--" или "-"
    /// </summary>
    public string Code { get { return _Code; } }
    private readonly string _Code;

    /// <summary>
    /// Определяет наличие значения после опции.
    /// По умолчанию - <see cref="CommandLineOptionValueMode.None"/>.
    /// </summary>
    public CommandLineOptionValueMode ValueMode { get { return _ValueMode; } set { _ValueMode = value; } }
    private CommandLineOptionValueMode _ValueMode;

    /// <summary>
    /// Используется в режиме <see cref="ValueMode"/>=<see cref="CommandLineOptionValueMode.Multi"/>.
    /// Разделяет несколько значений в результатах парснига <see cref="CommandLineParser.OptionValues"/>.
    /// По умолчанию - запятая.
    /// </summary>
    public char MultiValueSeparator { get { return _MultiValueSeparator; } set { _MultiValueSeparator = value; } }
    private char _MultiValueSeparator;

    /// <summary>
    /// True (по умолчанию), если опция ожидается при разборе командной строки.
    /// Некоторые опции могут появляться в командной строке только при наличии определенных аргументов,
    /// поэтому опции могут выключаться и включаться динамически в процессе парсинга.
    /// </summary>
    public bool Enabled { get { return _Enabled; } set { _Enabled = value; } }
    private bool _Enabled;

    /// <summary>
    /// Сообщение об ошибке, используемое при <see cref="Enabled"/>=false.
    /// </summary>
    public string EnabledErrorMessage
    {
      get
      {
        if (String.IsNullOrEmpty(_EnabledErrorMessage))
        {
          if (Enabled)
            return String.Empty;
          else
            return Res.CommandLineParser_Err_OptionDisabled;
        }
        else
          return _EnabledErrorMessage;
      }
      set { _EnabledErrorMessage = value; }
    }
    private string _EnabledErrorMessage;

    /// <summary>
    /// Список допустимых значений.
    /// Если в списке есть значения, то выполняется проверка значения (после знака равенства) на попадание
    /// в список с учетом свойства <see cref="CommandLineParser.IgnoreCase"/>.
    /// Действует при всех <see cref="ValueMode"/>, кроме <see cref="CommandLineOptionValueMode.None"/>.
    /// Пустое значение для <see cref="ValueMode"/>=<see cref="CommandLineOptionValueMode.Optional"/> добавлять не нужно.
    /// </summary>
    public IList<string> Values { get { return _Values; } }
    private readonly List<string> _Values;
  }

  /// <summary>
  /// Аргументы события <see cref="CommandLineParser.ActionFound"/>
  /// </summary>
  public sealed class CommandLineActionEventArgs : EventArgs
  {
    #region Конструктор

    internal CommandLineActionEventArgs(CommandLineAction action, string actionString)
    {
      _Action = action;
      _ActionString = actionString;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Описатель действия, которое была найдено в командной строке
    /// </summary>
    public CommandLineAction Action { get { return _Action; } }
    private readonly CommandLineAction _Action;

    /// <summary>
    /// Код команды, как он задан в командной строке.
    /// </summary>
    public string ActionString { get { return _ActionString; } }
    private readonly string _ActionString;

    #endregion
  }

  /// <summary>
  /// Делегат события <see cref="CommandLineParser.ActionFound"/>
  /// </summary>
  /// <param name="sender">Ссылка на <see cref="CommandLineParser"/></param>
  /// <param name="args">Аргументы события</param>
  public delegate void CommandLineActionEventHandler(object sender, CommandLineActionEventArgs args);

  /// <summary>
  /// Аргументы события <see cref="CommandLineParser.OptionFound"/>
  /// </summary>
  public sealed class CommandLineOptionEventArgs : EventArgs
  {
    #region Конструктор

    internal CommandLineOptionEventArgs(CommandLineOption option, string optionString, string value)
    {
      _Option = option;
      _OptionString = optionString;
      _Value = value;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Описатель опции, которая была найдена в командной строке
    /// </summary>
    public CommandLineOption Option { get { return _Option; } }
    private readonly CommandLineOption _Option;

    /// <summary>
    /// Код опции, как он задан в командной строке.
    /// Содержит префикс "--", "-" или "/".
    /// Не содержит значения или знака равенства.
    /// </summary>
    public string OptionString { get { return _OptionString; } }
    private readonly string _OptionString;

    /// <summary>
    /// Значение опции после знака "=" или значение следующего аргумента после пробела.
    /// Если опция не содержит значения (<see cref="CommandLineOption.ValueMode"/>==<see cref="CommandLineOptionValueMode.None"/> или <see cref="CommandLineOptionValueMode.Optional"/>), свойство возвращает пустую строку.
    /// </summary>
    public string Value { get { return _Value; } }
    private readonly string _Value;

    #endregion
  }

  /// <summary>
  /// Делегат события <see cref="CommandLineParser.OptionFound"/>
  /// </summary>
  /// <param name="sender">Ссылка на <see cref="CommandLineParser"/></param>
  /// <param name="args">Аргументы события</param>
  public delegate void CommandLineOptionEventHandler(object sender, CommandLineOptionEventArgs args);

  /// <summary>
  /// Аргументы события <see cref="CommandLineParser.CommonArgFound"/>
  /// </summary>
  public sealed class CommandLineCommonArgEventArgs : EventArgs
  {
    #region Конструктор

    internal CommandLineCommonArgEventArgs(string value)
    {
      _Value = value;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Значение аргумента, например, имя файла
    /// </summary>
    public string Value { get { return _Value; } }
    private readonly string _Value;

    #endregion
  }

  /// <summary>
  /// Делегат события <see cref="CommandLineParser.CommonArgFound"/>
  /// </summary>
  /// <param name="sender">Ссылка на <see cref="CommandLineParser"/></param>
  /// <param name="args">Аргументы события</param>
  public delegate void CommandLineCommonArgEventHandler(object sender, CommandLineCommonArgEventArgs args);

  /// <summary>
  /// Парсер командной строки.
  /// Упрощает поиск аргументов-команд, аргументов-опций (ключи с префиксом "--" или "-") и обычных аргументов (например, имен файлов).
  /// </summary>
  public sealed class CommandLineParser
  {
    #region Конструктор

    /// <summary>
    /// Создает пустой парсер.
    /// </summary>
    public CommandLineParser()
    {
      _ActionDefs = new ActionCollection();
      _ActionMode = CommandLineActionMode.None;
      _OptionDefs = new OptionCollection();
      _IgnoreCase = false;

      switch (Environment.OSVersion.Platform)
      {
        case PlatformID.Win32NT:
        case PlatformID.Win32Windows:
        case PlatformID.Win32S:
        case PlatformID.WinCE:
          _SlashedOptionsEnabled = true;
          break;
        default:
          _SlashedOptionsEnabled = false;
          break;
      }
    }

    #endregion

    #region Управляющие свойства

    /// <summary>
    /// Являются ли действия и опции нечувствительными к регистру?
    /// По умолчанию - false.
    /// </summary>
    public bool IgnoreCase { get { return _IgnoreCase; } set { _IgnoreCase = value; } }
    private bool _IgnoreCase;

    #region Действия

    /// <summary>
    /// Использование действий.
    /// По умолчанию <see cref="CommandLineActionMode.None"/> - действия не используются.
    /// Свойство должно быть установлено, если используется коллекция <see cref="ActionDefs"/>.
    /// </summary>
    public CommandLineActionMode ActionMode { get { return _ActionMode; }set { _ActionMode = value; } }
    private CommandLineActionMode _ActionMode;

    /// <summary>
    /// Реализация свойства <see cref="ActionDefs"/>
    /// </summary>
    public sealed class ActionCollection : NamedList<CommandLineAction>
    {
      #region Защищенный конструктор

      internal ActionCollection()
      {
      }

      #endregion

      #region Методы добавления

      /// <summary>
      /// Создает команду и добавляет ее в список
      /// </summary>
      /// <param name="actions">Строки действий, через запятую, например, "add,a"</param>
      /// <returns>Созданный описатель команды</returns>
      public CommandLineAction Add(string actions)
      {
        CommandLineAction act = new CommandLineAction(actions);
        base.Add(act);
        return act;
      }

      #endregion

      #region Блокирование / разблокирование

      /// <summary>
      /// Устанавливает <see cref="CommandLineAction.Enabled"/>=true для всех действий
      /// </summary>
      public void EnableAll()
      {
        foreach (CommandLineAction action in this)
          action.Enabled = true;
      }

      /// <summary>
      /// Устанавливает <see cref="CommandLineAction.Enabled"/>=false для всех действий.
      /// Также может устанавливать свойство <see cref="CommandLineAction.EnabledErrorMessage"/>.
      /// </summary>
      /// <param name="errorMessage">Если задана непустая строка, то будет установлено свойство <see cref="CommandLineAction.EnabledErrorMessage"/></param>
      public void DisableAll(string errorMessage)
      {
        foreach (CommandLineAction action in this)
        {
          action.Enabled = false;
          if (!String.IsNullOrEmpty(errorMessage))
            action.EnabledErrorMessage = errorMessage;
        }
      }

      #endregion
    }

    /// <summary>
    /// Описания действий.
    /// При ипсользовании действий должно быть включено управляющее свойство <see cref="CommandLineParser.ActionMode"/>.
    /// </summary>
    public ActionCollection ActionDefs { get { return _ActionDefs; } }
    private readonly ActionCollection _ActionDefs;

    /// <summary>
    /// Разрешено ли наличие нескольких действий?
    /// По умолчанию - false - может быть только одна команда
    /// </summary>
    public bool MultiActionsEnabled { get { return _MultiActionsEnabled; }set { _MultiActionsEnabled = value; } }
    private bool _MultiActionsEnabled;

    #endregion

    #region Опции

    /// <summary>
    /// Реализация свойства <see cref="OptionDefs"/>
    /// </summary>
    public sealed class OptionCollection : NamedList<CommandLineOption>
    {
      #region Защищенный конструктор

      internal OptionCollection()
      {
      }

      #endregion

      #region Методы добавления

      /// <summary>
      /// Создает <see cref="CommandLineOption"/> без поддержки значений (простая опция) и добавляет ее в список
      /// </summary>
      /// <param name="options">Описатели опций, через запятую, например, "--list,-L"</param>
      /// <returns>Созданный описатель опции</returns>
      public CommandLineOption Add(string options)
      {
        CommandLineOption opt = new CommandLineOption(options);
        base.Add(opt);
        return opt;
      }

      /// <summary>
      /// Создает <see cref="CommandLineOption"/> с заданным режимом использования значений и добавляет ее в список
      /// </summary>
      /// <param name="options">Описатели опций, через запятую, например, "--list,-L"</param>
      /// <param name="valueMode">Режим использования значений</param>
      /// <returns>Созданный описатель опции</returns>
      public CommandLineOption Add(string options, CommandLineOptionValueMode valueMode)
      {
        CommandLineOption opt = new CommandLineOption(options);
        opt.ValueMode = valueMode;
        base.Add(opt);
        return opt;
      }

      /// <summary>
      /// Создает <see cref="CommandLineOption"/> со списком допустимых значений и добавляет ее в список.
      /// </summary>
      /// <param name="options">Описатели опций, через запятую, например, "--list,-L"</param>
      /// <param name="valueMode">Режим использования значений. 
      /// Не может быть <see cref="CommandLineOptionValueMode.None"/></param>
      /// <param name="values">Допустимые значения</param>
      /// <returns>Созданный описатель опции</returns>
      public CommandLineOption Add(string options, CommandLineOptionValueMode valueMode, params string[] values)
      {
        if (valueMode == CommandLineOptionValueMode.None && values.Length>0)
          throw new ArgumentException(Res.CommandLineParser_Arg_AddOptionWithValues, "valueMode");
        CommandLineOption opt = new CommandLineOption(options);
        opt.ValueMode = valueMode;
        foreach (string v in values)
          opt.Values.Add(v);
        base.Add(opt);
        return opt;
      }

      #endregion

      #region Блокирование / разблокирование

      /// <summary>
      /// Устанавливает <see cref="CommandLineOption.Enabled"/>=true для всех опций
      /// </summary>
      public void EnableAll()
      {
        foreach (CommandLineOption option in this)
          option.Enabled = true;
      }

      /// <summary>
      /// Устанавливает <see cref="CommandLineOption.Enabled"/>=false для всех опций.
      /// Также может устанавливать свойство <see cref="CommandLineOption.EnabledErrorMessage"/>.
      /// </summary>
      /// <param name="errorMessage">Если задана непустая строка, то будет установлено свойство <see cref="CommandLineOption.EnabledErrorMessage"/></param>
      public void DisableAll(string errorMessage)
      {
        foreach (CommandLineOption option in this)
        {
          option.Enabled = false;
          if (!String.IsNullOrEmpty(errorMessage))
            option.EnabledErrorMessage = errorMessage;
        }
      }

      #endregion
    }

    /// <summary>
    /// Описания опций
    /// </summary>
    public OptionCollection OptionDefs { get { return _OptionDefs; } }
    private readonly OptionCollection _OptionDefs;

    /// <summary>
    /// Разрешены ли опции в формате с начальным слэшем ("/list")?
    /// По умолчанию разрешено в Windows и запрещено в Linux.
    /// Если разрешено, то "/" заменяет как "--", так и "-".
    /// </summary>
    public bool SlashedOptionsEnabled
    {
      get { return _SlashedOptionsEnabled; }
      set { _SlashedOptionsEnabled = value; }
    }
    private bool _SlashedOptionsEnabled;

    #endregion

    #region Обычные аргументы

    /// <summary>
    /// Допускаются ли обычные аргументы командной строки (не-опции, и не-действия, например, имена файлов)?
    /// По умолчанию - false.
    /// </summary>
    public bool CommonArgsEnabled { get { return _CommonArgsEnabled; } set { _CommonArgsEnabled = value; } }
    private bool _CommonArgsEnabled;

    /// <summary>
    /// Сообщение, которое выдается при появлении простого аргумента, когда <see cref="CommonArgsEnabled"/>=false.
    /// </summary>
    public string CommonArgsErrorMessage
    {
      get
      {
        if (String.IsNullOrEmpty(_CommonArgsErrorMessage))
        {
          if (CommonArgsEnabled)
            return String.Empty;
          else
            return Res.CommandLineParser_Err_CommonArgsErrorMessage;
        }
        else
          return _CommonArgsErrorMessage;
      }
      set
      {
        _CommonArgsErrorMessage = value;
      }
    }
    private string _CommonArgsErrorMessage;

    #endregion

    #endregion

    #region Парсинг командной строки

    /// <summary>
    /// Выполняет парсинг командной строки, возвращаемой <see cref="Environment.GetCommandLineArgs()"/>.
    /// В случае успеха заполняются массивы <see cref="ActionValues"/>, <see cref="OptionValues"/> и <see cref="CommonValues"/>.
    /// В случае ошибки устанавливается свойство <see cref="ErrorMessage"/>.
    /// </summary>
    /// <returns>True, если парсинг успешно выполнен</returns>
    public bool Parse()
    {
      return Parse(Environment.GetCommandLineArgs(), true);
    }

    /// <summary>
    /// Выполняет парсинг командной строки, аргументы которой переданы в функцию Main() или
    /// возвращаемые <see cref="Environment.GetCommandLineArgs()"/>.
    /// В случае успеха заполняются массивы <see cref="ActionValues"/>, <see cref="OptionValues"/> и <see cref="CommonValues"/>.
    /// В случае ошибки устанавливается свойство <see cref="ErrorMessage"/>.
    /// </summary>
    /// <param name="args">Массив аргументов</param>
    /// <param name="hasExePath">True, если первый элемент массива <paramref name="args"/> является путем к выполняемому файлу, а не аргументом командной строки</param>
    /// <returns>True, если парсинг успешно выполнен</returns>
    public bool Parse(string[] args, bool hasExePath)
    {
      ArrayEnumerable<string>.Enumerator en = new ArrayEnumerable<string>(args).GetEnumerator();
      Queue<string> args2 = new Queue<string>(args);
      if (hasExePath)
        args2.Dequeue();
      return DoParse(args2);
    }


    private bool DoParse(Queue<string> args)
    {
      _ErrorMessage = null;
      _ActionValues = new List<string>();
      _OptionValues = new Dictionary<string, string>();
      _CommonValues = new List<string>();

      // Словарь для всех форм опций
      TypedStringDictionary<CommandLineOption> optDefs = new TypedStringDictionary<CommandLineOption>(IgnoreCase);
      foreach (CommandLineOption opt in OptionDefs)
      {
        foreach (string s in opt.Options)
          optDefs.Add(s, opt);
      }

      // Словарь для всех форм действий
      if (ActionDefs.Count > 0 && ActionMode == CommandLineActionMode.None)
        throw new InvalidOperationException(Res.CommandLineParser_Err_ActionModeNone);
      TypedStringDictionary<CommandLineAction> actDefs = new TypedStringDictionary<CommandLineAction>(IgnoreCase);
      foreach (CommandLineAction act in ActionDefs)
      {
        foreach (string s in act.Actions)
          actDefs.Add(s, act);
      }

      // Цикл по аргументам командной строки
      while (args.Count > 0)
      {
        string arg = args.Dequeue();
        if (String.IsNullOrEmpty(arg))
          continue;
        if (arg[0] == '-')
        {
          string sOpt, sValue;
          SplitOptionAndValue(arg, out sOpt, out sValue);
          CommandLineOption opt;
          if (optDefs.TryGetValue(sOpt, out opt))
          {
            if (!DoParseOption(sOpt, sValue, opt, args))
              return false;
          }
          else
          {
            SetError(String.Format(Res.CommandLineParser_Err_UnknownOption, sOpt));
            return false;
          }
        }
        else if (SlashedOptionsEnabled && arg[0] == '/')
        {
          string sOpt, sValue;
          SplitOptionAndValue(arg, out sOpt, out sValue);
          CommandLineOption opt;

          string sOpt1 = "--" + sOpt.Substring(1);
          string sOpt2 = "-" + sOpt.Substring(1);
          if (optDefs.TryGetValue(sOpt1, out opt))
          {
            if (!DoParseOption(sOpt, sValue, opt, args))
              return false;
          }
          else if (optDefs.TryGetValue(sOpt2, out opt))
          {
            if (!DoParseOption(sOpt, sValue, opt, args))
              return false;
          }
          else
          {
            SetError(String.Format(Res.CommandLineParser_Err_UnknownOption, sOpt));
            return false;
          }
        }
        else // действие или обычный аргумент 
        {
          if (ActionMode == CommandLineActionMode.FirstOnly && ActionValues.Count == 0)
          {
            if (actDefs.ContainsKey(arg))
            {
              CommandLineAction act = actDefs[arg];
              if (!act.Enabled)
              {
                SetError(act.EnabledErrorMessage);
                return false;
              }

              _ActionValues.Add(act.Code);
              if (ActionFound != null)
              {
                CommandLineActionEventArgs args2 = new CommandLineActionEventArgs(act, arg);
                ActionFound(this, args2);
              }
              continue;
            }
            else
            {
              SetError(String.Format(Res.CommandLineParser_Err_UnknownAction, arg));
              return false;
            }
          }

          if (CommonArgsEnabled)
          {
            _CommonValues.Add(arg);
            if (CommonArgFound != null)
            {
              CommandLineCommonArgEventArgs args2 = new CommandLineCommonArgEventArgs(arg);
              CommonArgFound(this, args2);
            }
          }
          else
          {
            SetError(CommonArgsErrorMessage);
            return false;
          }
        }
      } // цикл по аргументам
      return true;
    }

    private static void SplitOptionAndValue(string s, out string sOpt, out string sValue)
    {
      int p = s.IndexOf('=');
      if (p >= 0)
      {
        sOpt = s.Substring(0, p);
        sValue = s.Substring(p + 1);
      }
      else
      {
        sOpt = s;
        sValue = String.Empty;
      }
    }

    private bool DoParseOption(string sOpt, string sValue, CommandLineOption opt, Queue<string> args)
    {
      if (!opt.Enabled)
      {
        SetError(String.Format(Res.CommandLineParser_Err_MisplacedOption, sOpt, opt.EnabledErrorMessage));
        return false;
      }

      if (opt.ValueMode == CommandLineOptionValueMode.Multi)
      {
        if (!GetOptionValue(sOpt, ref sValue, args))
        {
          SetError(String.Format(Res.CommandLineParser_Err_OptionValueRequired, sOpt));
          return false;
        }

        if (_OptionValues.ContainsKey(opt.Code))
          _OptionValues[opt.Code] += opt.MultiValueSeparator + sValue;
        else
        {
          if (ValidateOptionValue(sOpt, opt, sValue))
            AddOptionValue(sOpt, opt, sValue);
          else
            return false;
        }
      }
      else
      {
        if (_OptionValues.ContainsKey(opt.Code))
        {
          SetError(String.Format(Res.CommandLineParser_Err_OptionTwice, sOpt));
          return false;
        }

        switch (opt.ValueMode)
        {
          case CommandLineOptionValueMode.None:
            if (sValue.Length > 0)
            {
              SetError(String.Format(Res.CommandLineParser_Err_OptionValueUnwanted, sOpt));
              return false;
            }
            AddOptionValue(sOpt, opt, String.Empty);
            break;

          case CommandLineOptionValueMode.Single:
            if (!GetOptionValue(sOpt, ref sValue, args))
            {
              SetError(String.Format(Res.CommandLineParser_Err_OptionValueRequired, sOpt));
              return false;
            }
            if (ValidateOptionValue(sOpt, opt, sValue))
              AddOptionValue(sOpt, opt, sValue);
            else
              return false;
            break;

          case CommandLineOptionValueMode.Optional:
            if (GetOptionValue(sOpt, ref sValue, args))
            {
              if (ValidateOptionValue(sOpt, opt, sValue))
                AddOptionValue(sOpt, opt, sValue);
              else
                return false;
            }
            else
              AddOptionValue(sOpt, opt, String.Empty);
            break;
          default:
            throw new BugException("ValueMode=" + opt.ValueMode);
        }
      }
      return true;
    }

    private bool ValidateOptionValue(string sOpt, CommandLineOption opt, string sValue)
    {
      if (opt.Values.Count == 0)
        return true; // нет списка значений, не проверяем

      if (IgnoreCase)
      {
        foreach (string availValue in opt.Values)
        {
          if (String.Equals(sValue, availValue, StringComparison.OrdinalIgnoreCase))
            return true;
        }
      }
      else
      {
        if (opt.Values.Contains(sValue))
          return true;
      }

      SetError(String.Format(Res.CommandLineParser_Err_OptionUnknownValue,
        sValue,
        sOpt,
        UICore.UITools.ValueListToString(opt.Values)));
      return false;
    }

    private void AddOptionValue(string sOpt, CommandLineOption opt, string sValue)
    {
      _OptionValues.Add(opt.Code, sValue);
      if (OptionFound != null)
      {
        CommandLineOptionEventArgs args = new CommandLineOptionEventArgs(opt, sOpt, sValue);
        OptionFound(this, args);
      }
    }

    private bool GetOptionValue(string sOpt, ref string sValue, Queue<string> args)
    {
      if (sValue.Length == 0)
      {
        if (args.Count == 0) // больше нет аргументов
          return false;

        string s2 = args.Peek();
        if (s2.StartsWith("-", StringComparison.Ordinal))
          return false;
        if (SlashedOptionsEnabled && s2.StartsWith("/", StringComparison.Ordinal))
          return false;

        sValue = s2;
        args.Dequeue();
      }
      else
      {
        if (sValue.Length == 0)
        {
          SetError(String.Format(Res.CommandLineParser_Err_NoValueAfterEq, sOpt));
          return false;
        }
      }

      return true;
    }

    /// <summary>
    /// Сообщение об ошибке парсинга, которое должно быть выдано пользователю.
    /// </summary>
    public string ErrorMessage { get { return _ErrorMessage; } }
    private string _ErrorMessage;

    private void SetError(string s)
    {
      if (String.IsNullOrEmpty(s))
        throw ExceptionFactory.ArgStringIsNullOrEmpty("s");
      if (!String.IsNullOrEmpty(_ErrorMessage))
        return;
      _ErrorMessage = s;
    }

    /// <summary>
    /// Значения опций.
    /// Ключом словаря является <see cref="CommandLineOption.Code"/>.
    /// Для опций без значений в словаре содержится <see cref="String.Empty"/>.
    /// </summary>
    public IDictionary<string, string> OptionValues { get { return _OptionValues; } }
    private Dictionary<string, string> _OptionValues;

    /// <summary>
    /// Обычные аргументы командной строки (не опции).
    /// Появляются только при <see cref="_CommonArgsEnabled"/>=true.
    /// </summary>
    public IList<string> CommonValues { get { return _CommonValues; } }
    private List<string> _CommonValues;

    /// <summary>
    /// Аргументы-действия командной строки.
    /// Значениями являются <see cref="CommandLineAction.Code"/>.
    /// </summary>
    public IList<string> ActionValues { get { return _ActionValues; } }
    private List<string> _ActionValues;

    #endregion

    #region События при парсинге

    /// <summary>
    /// Событие вызывается после того, как был найден аргумент-действие
    /// </summary>
    public event CommandLineActionEventHandler ActionFound;

    /// <summary>
    /// Событие вызывается в процессе парсинга после того, как была найдена очередная опция
    /// </summary>
    public event CommandLineOptionEventHandler OptionFound;

    /// <summary>
    /// Событие вызывается после того, как был найден обычный аргумент (например, имя файла)
    /// </summary>
    public event CommandLineCommonArgEventHandler CommonArgFound;

    #endregion

    #region Методы извлечения значений

    /// <summary>
    /// Возвращает значение опции.
    /// Если опция не встретилась в командной строке, возвращается пустая строка
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public string GetOptionValue(string code)
    {
      CheckParsed();

      CommandLineOption optDef = _OptionDefs.GetRequired(code);
      string value;
      if (_OptionValues.TryGetValue(code, out value))
        return value;
      else
        return String.Empty;
    }

    /// <summary>
    /// Возвращает массив строк для опции.
    /// Если опция ни разу ни встретилась в командной строке, возвращается пустой массив
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public string[] GetMultiOptionValues(string code)
    {
      CheckParsed();

      CommandLineOption optDef = _OptionDefs.GetRequired(code);
      string value;
      if (!_OptionValues.TryGetValue(code, out value))
        return DataTools.EmptyStrings;
      if (String.IsNullOrEmpty(value))
        return DataTools.EmptyStrings;

      if (optDef.ValueMode == CommandLineOptionValueMode.Multi)
        return value.Split(optDef.MultiValueSeparator);
      else
        return new string[1] { value };
    }

    private void CheckParsed()
    {
      if (_OptionValues == null)
        throw ExceptionFactory.ObjectMethodNotCalled(this, "Parse()");
    }


    #endregion
  }
}
