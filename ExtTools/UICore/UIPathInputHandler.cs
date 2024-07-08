using System;
using System.Collections.Generic;
using System.Text;
using FreeLibSet.DependedValues;
using FreeLibSet.IO;

namespace FreeLibSet.UICore
{
  //public enum UIPathMode
  //{
  //  /// <summary>
  //  /// Путь всегда хранится как абсолютный.
  //  /// Если пользователь ввел относительный путь, то он преобразуется в абсолютный с использованием BaseDir.
  //  /// </summary>
  //  Abs,

  //  /// <summary>
  //  /// Преобразование не выполняется. Путь хранится в том виде, как задан. 
  //  /// </summary>
  //  AsIs,

  //  /// <summary>
  //  /// Предпочтение отдается относительному пути. Если введен абсолютный путь, который можно сделать относительным, выполняется преобразование. 
  //  /// Например, если BasePath="C:\Work\123", а введен путь "C:\Work\456", то путь будет преобразован в "..\456".
  //  /// Абсолютный путь используется, только если нельзя использовать относительный путь, например введен путь для другого диска.
  //  /// Также абсолютный путь используется, если требуется переход к корневому пути, например BaseDir="C:\Work\123", а введено "C:\456".
  //  /// </summary>
  //  RelPreferred,

  //  /// <summary>
  //  /// Предпочтение отдается относительному пути, если он является BasePath или его подкаталогом. 
  //  /// Например, если BasePath="C:\Work\123", а введен путь "C:\Work\123\789", то будет сохранено «.\789». 
  //  /// Если же введен путь "C:\Work\456", то он не будет преобразован. Также, если введен путь с указанием родительского каталога, например, "..\456", он будет преобразован в абсолютный путь.
  //  /// </summary>
  //  RelIfNoParentDir
  //}

  /// <summary>
  /// Объект для преобразования строки поля ввода в объекты абсолютного <see cref="Path"/> и относительного <see cref="RelPath"/> пути.
  /// Используется управляющими элементами.
  /// </summary>
  public sealed class UIPathInputHandler
  {
    #region Конструктор

    /// <summary>
    /// Создает обработчик, присоединенный к полю ввода
    /// </summary>
    /// <param name="textEx">Управляемое свойство Text провайдера управляющего элемента</param>
    public UIPathInputHandler(DepValue<string> textEx)
    {
      if (textEx == null)
        throw new ArgumentNullException("textEx");
      _TextEx = (DepInput<string>)textEx;
      _TextEx.ValueChanged += TextEx_ValueChanged;

      _BasePath = new AbsPath(Environment.CurrentDirectory);

      TextEx_ValueChanged(null, null);
    }

    #endregion

    #region TextEx

    /// <summary>
    /// Свойство управляющего элемента для ввода текста
    /// </summary>
    private readonly DepInput<string> _TextEx;

    /// <summary>
    /// Чтение и установка текста управляющего элемента
    /// </summary>
    private string Text
    {
      get { return _TextEx.Value; }
      set { _TextEx.Value = value; }
    }

    private void TextEx_ValueChanged(object sender, EventArgs args)
    {
      if (_InsideValueChanged)
        return;

      _InsideValueChanged = true;
      try
      {
        if (String.IsNullOrEmpty(Text))
        {
          Path = AbsPath.Empty;
          RelPath = RelPath.Empty;
        }
        else
        {
          try
          {
            RelPath = RelPath.Create(Text);
            Path = BasePath + RelPath;
          }
          catch
          {
            Path = AbsPath.Empty;
            RelPath = RelPath.Empty;
          }
        }
      }
      finally
      {
        _InsideValueChanged = false;
      }
    }

    private bool _InsideValueChanged;

    #endregion

    #region BasePath

    /// <summary>
    /// Базовый каталог, относительно которого выполняются преобразования.
    /// По умолчанию используется текущий каталог приложения <see cref="Environment.CurrentDirectory"/>.
    /// </summary>
    public AbsPath BasePath
    {
      get { return _BasePath; }
      set
      {
        if (value.IsEmpty)
          _BasePath = new AbsPath(Environment.CurrentDirectory);
        else
          _BasePath = value;

        TextEx_ValueChanged(null, null);
      }
    }
    private AbsPath _BasePath;

    /// <summary>
    /// Если true, то предполагается, что поле ввода предназначено для ввода пути к каталогу, а не файлу, и ввод должен заканчиваться символом разделителя каталога.
    /// По умолчанию - false.
    /// </summary>
    public bool UseSlashedPath
    {
      get { return _UseSlashedPath; }
      set
      {
        if (value == _UseSlashedPath)
          return;
        _UseSlashedPath = value;
        if (!RelPath.IsEmpty)
        {
          _InsideValueChanged = true;
          try
          {
            if (UseSlashedPath)
              Text = RelPath.SlashedPath;
            else
              Text = RelPath.Path;
          }
          finally
          {
            _InsideValueChanged = false;
          }
        }
      }
    }
    private bool _UseSlashedPath;

    #endregion

    #region AbsPath

    /// <summary>
    /// Абсолютный путь, заданный в поле ввода.
    /// Установка свойства задает путь в абсолютном формате, даже если возможен относительный путь.
    /// Если в поле задан относительный путь, то при чтении свойства он преобразуется в асболютный с использованием <see cref="BasePath"/>.
    /// </summary>
    public AbsPath Path
    {
      get { return _Path; }
      set
      {
        _Path = value;
        if (_PathEx != null)
          _PathEx.OwnerSetValue(value);
        if (!_InsideValueChanged)
        {
          _InsideValueChanged = true;
          try
          {
            if (UseSlashedPath)
              Text = _Path.SlashedPath;
            else
              Text = _Path.Path;
            RelPath = (RelPath)_Path;
          }
          finally
          {
            _InsideValueChanged = false;
          }
        }
      }
    }
    private AbsPath _Path;

    /// <summary>
    /// Управляемое свойство для <see cref="Path"/>.
    /// </summary>
    public DepValue<AbsPath> PathEx
    {
      get
      {
        InitPathEx();
        return _PathEx;
      }
      set
      {
        InitPathEx();
        _PathEx.Source = value;
      }
    }
    private DepInput<AbsPath> _PathEx;

    private void InitPathEx()
    {
      if (_PathEx == null)
      {
        _PathEx = new DepInput<AbsPath>(_Path, PathEx_ValueChanged);
        _PathEx.OwnerInfo = new DepOwnerInfo(this, "PathEx");
      }
    }

    private void PathEx_ValueChanged(object sender, EventArgs args)
    {
      this.Path = _PathEx.Value;
    }

    #endregion

    #region RelPath

    /// <summary>
    /// Основное свойство - путь, заданный в поле ввода.
    /// Путь может быть задан в относительном формате (с использованием <see cref="BasePath"/>) или в абсолютном.
    /// Установка свойства устанавливает текст в поле ввода с учетом свойства <see cref="UseSlashedPath"/>. 
    /// Путь выводится как есть. Если необходимо преобразовать его в относительный или абсолютный формат, используйте методы
    /// <see cref="FreeLibSet.IO.RelPath.ToRelative(AbsPath)"/> или <see cref="FreeLibSet.IO.RelPath.ToAbsolute(AbsPath)"/> соответственно перед присвоением значения свойству.
    /// </summary>
    public RelPath RelPath
    {
      get { return _RelPath; }
      set
      {
        _RelPath = value;
        if (_RelPathEx != null)
          _RelPathEx.OwnerSetValue(value);
        if (!_InsideValueChanged)
        {
          _InsideValueChanged = true;
          try
          {
            if (UseSlashedPath)
              Text = _RelPath.SlashedPath;
            else
              Text = _RelPath.Path;

            Path = (AbsPath)_RelPath;
          }
          finally
          {
            _InsideValueChanged = false;
          }
        }
      }
    }
    private RelPath _RelPath;

    /// <summary>
    /// Управляемое свойство <see cref="RelPath"/>
    /// </summary>
    public DepValue<RelPath> RelPathEx
    {
      get
      {
        InitRelPathEx();
        return _RelPathEx;
      }
      set
      {
        InitRelPathEx();
        _RelPathEx.Source = value;
      }
    }
    private DepInput<RelPath> _RelPathEx;

    private void InitRelPathEx()
    {
      if (_RelPathEx == null)
      {
        _RelPathEx = new DepInput<RelPath>(_RelPath, RelPathEx_ValueChanged);
        _RelPathEx.OwnerInfo = new DepOwnerInfo(this, "RelPathEx");
      }
    }

    private void RelPathEx_ValueChanged(object sender, EventArgs args)
    {
      this.RelPath = _RelPathEx.Value;
    }

    #endregion

    #region Свойство IsNotEmptyEx

    /// <summary>
    /// Управляемое свойство, которое возвращает true, если <see cref="RelPath"/>.IsEmpty=false.
    /// Свойство будет возвращать false, если введенный текст задает путь в неправильном формате.
    /// </summary>
    public DepValue<bool> IsNotEmptyEx
    {
      get
      {
        if (_IsNotEmptyEx == null)
        {
          _IsNotEmptyEx = new DepExpr1<bool, RelPath>(RelPathEx, CalcIsNotEmpty);
          _IsNotEmptyEx.OwnerInfo = new DepOwnerInfo(this, "IsNotEmptyEx");
        }
        return _IsNotEmptyEx;
      }
    }
    private DepValue<bool> _IsNotEmptyEx;

    private static bool CalcIsNotEmpty(RelPath path)
    {
      return !path.IsEmpty;
    }

    #endregion
  }
}
