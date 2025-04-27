// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using FreeLibSet.Core;
using FreeLibSet.Text;

namespace FreeLibSet.Forms
{
#if XXX
  /// <summary>
  /// Класс реализует свойство <see cref="EFPApp.Clipboard"/>.
  /// Выполняет создание
  /// Расширение работы с буфером обмена.
  /// Методы чтения/записи выполняют по несколько попыток обращения к буферу обмена, прежде чем выдать сообщение об ошибке.
  /// В случае неустранимой ошибки выдается сообщение с помощью <see cref="MessageBox"/>.
  /// </summary>
  public sealed class EFPAppClipboard
  {
    #region Защищенный конструктор

    internal EFPAppClipboard()
    {
      _RepeatCount = 10;
      _RepeatDelay = 100;


      // Текстовый формат по умолчанию

      // В исходных текстах Net Framework так:
      //if (Environment.OSVersion.Platform != System.PlatformID.Win32NT ||
      //    Environment.OSVersion.Version.Major < 5)
      //  _DefaultTextFormat = TextDataFormat.Text;
      //else
      //  _DefaultTextFormat = TextDataFormat.UnicodeText;

      // В Mono задано использование TextDataFormat.UnicodeText всегда.

      // Комбинируем
      switch (Environment.OSVersion.Platform)
      {
        case PlatformID.Win32NT:
          if (Environment.OSVersion.Version.Major < 5)
            _DefaultTextFormat = TextDataFormat.Text;
          else
            _DefaultTextFormat = TextDataFormat.UnicodeText;
          break;
        case PlatformID.Win32Windows:
        case PlatformID.WinCE:
        case PlatformID.Win32S:
          _DefaultTextFormat = TextDataFormat.Text;
          break;
        default:
          _DefaultTextFormat = TextDataFormat.UnicodeText;
          break;
      }
    }

    #endregion

    #region Управляющие свойства

    /// <summary>
    /// Количество попыток выполнить операцию с буфером обмена, прежде чем выдать сообщение об ошибке.
    /// По умолчанию делается 10 попыток.
    /// </summary>
    public int RepeatCount
    {
      get { return _RepeatCount; }
      set
      {
        if (value < 1)
          throw new ArgumentOutOfRangeException();
        _RepeatCount = value;
      }
    }
    private int _RepeatCount;

    /// <summary>
    /// Интервал времени в миллисекундах между попытками выполнить операцию с буфером обмена.
    /// Значение по умолчанию - 100 мс. Минимальное значение 1.
    /// Свойство имеет смысл при <see cref="RepeatCount"/> больше 1.
    /// </summary>
    public int RepeatDelay
    {
      get { return _RepeatDelay; }
      set
      {
        if (value < 1)
          throw new ArgumentOutOfRangeException();
        _RepeatDelay = value;
      }
    }
    private int _RepeatDelay;

    #endregion

    #region Чтение и запись в буфер

    /// <summary>
    /// После вызовов GetXXX() и SetXXX() свойство возвращает true, если возникла ошибка при работе с буфером обмена и
    /// false, если операция выполнена успешно.
    /// Признак ошибки не выставляется, если в буфере нет данных в подходящем формате.
    /// </summary>
    public bool HasError { get { return _HasError; } }
    private bool _HasError;

    #region Text

    /// <summary>
    /// Возвращает <see cref="TextDataFormat.UnicodeText"/> или <see cref="TextDataFormat.Text"/>, в зависимости от операционной системы
    /// </summary>
    public TextDataFormat DefaultTextFormat { get { return _DefaultTextFormat; } }
    private readonly TextDataFormat _DefaultTextFormat;

    /// <summary>
    /// Помещает текст в буфер обмена.
    /// Выполняется <see cref="RepeatCount"/> попыток с задержкой <see cref="RepeatDelay"/> между попытками. Потом выводится сообщение об ошибке.
    /// Свойство <see cref="HasError"/> может использоваться для проверки результатов копирования в прикладном коде.
    /// </summary>
    /// <param name="s">Копируемый текст</param>
    public void SetText(string s)
    {
      EFPApp.BeginWait(Res.Clipboard_Phase_SetData, "Copy");
      try
      {
        _HasError = true;
        for (int i = RepeatCount - 1; i >= 0; i--)
        {
          try
          {
            if (String.IsNullOrEmpty(s))
              Clipboard.Clear();
            else
              Clipboard.SetText(s);
            _HasError = false;
            break;
          }
          catch (Exception e)
          {
            if (i > 0)
              Thread.Sleep(RepeatDelay);
            else
            {
              EFPApp.ErrorMessageBox(e.Message, Res.Clipboard_ErrTitle_SetData);
              break;
            }
          }
        }
      }
      finally
      {
        EFPApp.EndWait();
      }
    }

    /// <summary>
    /// Извлекает текст из буфера обмена.
    /// Выполняется <see cref="RepeatCount"/> попыток с задержкой <see cref="RepeatDelay"/> между попытками. 
    /// Если буфер обмена пуст или не содержит текста, сообщение об ошибке не выдается.
    /// Свойство <see cref="HasError"/> может использоваться для проверки результатов извлечения текста в прикладном коде.
    /// </summary>
    /// <returns>Текст из буфера обмена или пустая строка</returns>
    public string GetText()
    {
      return GetText(DefaultTextFormat, false);
    }


    /// <summary>
    /// Извлекает текст из буфера обмена.
    /// Выполняется <see cref="RepeatCount"/> попыток с задержкой <see cref="RepeatDelay"/> между попытками. Потом выводится сообщение об ошибке,
    /// если <paramref name="messageIfEmpty"/>=true.
    /// Свойство <see cref="HasError"/> может использоваться для проверки результатов извлечения текста в прикладном коде.
    /// </summary>
    /// <param name="messageIfEmpty">Если true, то будет выведено сообщение об отсутствии текста в буфере обмена</param>
    /// <returns>Текст из буфера обмена или пустая строка</returns>
    public string GetText(bool messageIfEmpty)
    {
      return GetText(DefaultTextFormat, messageIfEmpty);
    }

    /// <summary>
    /// Извлекает текст из буфера обмена.
    /// Выполняется <see cref="RepeatCount"/> попыток с задержкой <see cref="RepeatDelay"/> между попытками. Потом выводится сообщение об ошибке,
    /// если <paramref name="messageIfEmpty"/>=true.
    /// Свойство <see cref="HasError"/> может использоваться для проверки результатов извлечения текста в прикладном коде.
    /// </summary>
    /// <param name="format">Формат данных</param>
    /// <param name="messageIfEmpty">Если true, то будет выведено сообщение об отсутствии текста в буфере обмена</param>
    /// <returns>Текст из буфера обмена или пустая строка</returns>
    public string GetText(TextDataFormat format, bool messageIfEmpty)
    {
      string res = String.Empty;
      EFPApp.BeginWait(Res.Clipboard_Phase_GetData, "Paste");
      try
      {
        _HasError = true;
        for (int i = RepeatCount - 1; i >= 0; i--)
        {
          try
          {
            res = Clipboard.GetText(format);
            if (res == null)
              res = String.Empty; // на всякий случай
            _HasError = false;
            break;
          }
          catch (Exception e)
          {
            if (i > 0)
              Thread.Sleep(RepeatDelay);
            else
            {
              EFPApp.ErrorMessageBox(e.Message, Res.Clipboard_ErrTitle_GetData);
              messageIfEmpty = false;
              break;
            }
          }
        }
      }
      finally
      {
        EFPApp.EndWait();
      }

      if (res.Length == 0 && messageIfEmpty)
        EFPApp.ErrorMessageBox(Res.Clipboard_Err_NoText);

      return res;
    }

    /// <summary>
    /// Возвращает прямоугольный блок текста из формата CSV или тектового формата.
    /// Выполняется <see cref="RepeatCount"/> попыток с задержкой <see cref="RepeatDelay"/> между попытками. 
    /// Используются данные в формате <see cref="TextDataFormat.CommaSeparatedValue"/>, а при отсутствии - в <see cref="TextDataFormat.Text"/>.
    /// При отсутствии текста в буфере обмена сообщение об ошибке не выводится.
    /// Обычно следует использовать объект <see cref="EFPPasteTextMatrixFormat"/> для реализации вставки.
    /// Свойство <see cref="HasError"/> может использоваться для проверки результатов извлечения текста в прикладном коде.
    /// </summary>
    /// <returns>Матрица текста или null, если буфер обмена пуст, или возникла ошибка</returns>
    public string[,] GetTextMatrix()
    {
      return GetTextMatrix(false);
    }

    /// <summary>
    /// Возвращает прямоугольный блок текста из формата CSV или текстового формата.
    /// Выполняется <see cref="RepeatCount"/> попыток с задержкой <see cref="RepeatDelay"/> между попытками. Потом выводится сообщение об ошибке,
    /// если <paramref name="messageIfEmpty"/>=true.
    /// Используются данные в формате <see cref="TextDataFormat.CommaSeparatedValue"/>, а при отсутствии - в <see cref="TextDataFormat.Text"/>.
    /// Обычно следует использовать объект <see cref="EFPPasteTextMatrixFormat"/> для реализации вставки.
    /// Свойство <see cref="HasError"/> может использоваться для проверки результатов извлечения текста в прикладном коде.
    /// </summary>
    /// <param name="messageIfEmpty">Если true, то будет выведено сообщение об отсутствии текста в буфере обмена</param>
    /// <returns>Матрица текста или null, если буфер обмена пуст, или возникла ошибка</returns>
    public string[,] GetTextMatrix(bool messageIfEmpty)
    {
      IDataObject dobj = GetDataObject();
      if (dobj == null)
        return null;

      return WinFormsTools.GetTextMatrix(dobj);
    }

    /// <summary>
    /// Помещает в буфер обмена данные в форматах Text и CSV.
    /// Выполняется <see cref="RepeatCount"/> попыток с задержкой <see cref="RepeatDelay"/> между попытками. Потом выводится сообщение об ошибке.
    /// Свойство <see cref="HasError"/> может использоваться для проверки результатов копирования в прикладном коде.
    /// </summary>
    /// <param name="a">Двумерный массив строк</param>
    public void SetTextMatrix(string[,] a)
    {
      DataObject dobj = new DataObject();
      WinFormsTools.SetTextMatrix(dobj, a);
      SetDataObject(dobj, true);
    }

    #endregion

    #region DataObject

    /// <summary>
    /// Помещает данные в буфер обмена.
    /// Выполняется <see cref="RepeatCount"/> попыток с задержкой <see cref="RepeatDelay"/> между попытками. Потом выводится сообщение об ошибке.
    /// Свойство <see cref="HasError"/> может использоваться для проверки результатов копирования в прикладном коде.
    /// </summary>
    /// <param name="data">Данные</param>
    /// <param name="copy">Если нужно сделать данные сохраняющимися в буфере обмена после завершения приложения</param>
    public void SetDataObject(object data, bool copy)
    {
      EFPApp.BeginWait(Res.Clipboard_Phase_SetData, "Copy");
      try
      {
        _HasError = true;
        for (int i = RepeatCount - 1; i >= 0; i--)
        {
          try
          {
            Clipboard.SetDataObject(data, copy, 1, 0);
            _HasError = false;
            break;
          }
          catch (Exception e)
          {
            if (i > 0)
              Thread.Sleep(RepeatDelay);
            else
            {
              EFPApp.ErrorMessageBox(e.Message, Res.Clipboard_ErrTitle_SetData);
              break;
            }
          }
        }
      }
      finally
      {
        EFPApp.EndWait();
      }
    }

    /// <summary>
    /// Извлекает данные из буфера обмена.
    /// Выполняется <see cref="RepeatCount"/> попыток с задержкой <see cref="RepeatDelay"/> между попытками. Потом выводится сообщение об ошибке.
    /// </summary>
    /// <returns>Данные из буфера обмена или null в случае ошибки</returns>
    public IDataObject GetDataObject()
    {
      IDataObject res = null;
      EFPApp.BeginWait(Res.Clipboard_Phase_GetData, "Paste");
      try
      {
        _HasError = true;
        for (int i = RepeatCount - 1; i >= 0; i--)
        {
          try
          {
            res = Clipboard.GetDataObject();
            _HasError = false;
            break;
          }
          catch (Exception e)
          {
            if (i > 0)
              Thread.Sleep(RepeatDelay);
            else
            {
              EFPApp.ErrorMessageBox(e.Message, Res.Clipboard_ErrTitle_GetData);
              break;
            }
          }
        }
      }
      finally
      {
        EFPApp.EndWait();
      }
      return res;
    }

    #endregion

    #region Data

    /// <summary>
    /// Помещает данные в буфер обмена.
    /// Выполняется <see cref="RepeatCount"/> попыток с задержкой <see cref="RepeatDelay"/> между попытками. Потом выводится сообщение об ошибке.
    /// Свойство <see cref="HasError"/> может использоваться для проверки результатов копирования в прикладном коде.
    /// </summary>
    /// <param name="format">Формат данных</param>
    /// <param name="data">Данные</param>
    public void SetData(string format, object data)
    {
      EFPApp.BeginWait(Res.Clipboard_Phase_SetData, "Copy");
      try
      {
        _HasError = true;
        for (int i = RepeatCount - 1; i >= 0; i--)
        {
          try
          {
            Clipboard.SetData(format, data);
            _HasError = false;
            break;
          }
          catch (Exception e)
          {
            if (i > 0)
              Thread.Sleep(RepeatDelay);
            else
            {
              EFPApp.ErrorMessageBox(e.Message, Res.Clipboard_ErrTitle_SetData);
              break;
            }
          }
        }
      }
      finally
      {
        EFPApp.EndWait();
      }
    }

    /// <summary>
    /// Извлекает данные из буфера обмена.
    /// Выполняется <see cref="RepeatCount"/> попыток с задержкой <see cref="RepeatDelay"/> между попытками. Потом выводится сообщение об ошибке.
    /// Свойство <see cref="HasError"/> может использоваться для проверки результатов извлечения данных в прикладном коде.
    /// </summary>
    /// <param name="format">Требуемый формат данных</param>
    /// <returns>Данные из буфера обмена или null в случае ошибки или если нет данных в таком формате</returns>
    public object GetData(string format)
    {
      object res = null;
      EFPApp.BeginWait(Res.Clipboard_Phase_GetData, "Paste");
      try
      {
        _HasError = true;
        for (int i = RepeatCount - 1; i >= 0; i--)
        {
          try
          {
            res = Clipboard.GetData(format);
            _HasError = false;
            break;
          }
          catch (Exception e)
          {
            if (i > 0)
              Thread.Sleep(RepeatDelay);
            else
            {
              EFPApp.ErrorMessageBox(e.Message, Res.Clipboard_ErrTitle_GetData);
              break;
            }
          }
        }
      }
      finally
      {
        EFPApp.EndWait();
      }
      return res;
    }

    #endregion

    #region Image

    /// <summary>
    /// Помещает изображение в буфер обмена.
    /// Выполняется <see cref="RepeatCount"/> попыток с задержкой <see cref="RepeatDelay"/> между попытками. Потом выводится сообщение об ошибке.
    /// Свойство <see cref="HasError"/> может использоваться для проверки результатов копирования в прикладном коде.
    /// </summary>
    /// <param name="image">Изображение</param>
    public void SetImage(Image image)
    {
      EFPApp.BeginWait(Res.Clipboard_Phase_SetData, "Copy");
      try
      {
        _HasError = true;
        for (int i = RepeatCount - 1; i >= 0; i--)
        {
          try
          {
            Clipboard.SetImage(image);
            _HasError = false;
            break;
          }
          catch (Exception e)
          {
            if (i > 0)
              Thread.Sleep(RepeatDelay);
            else
            {
              EFPApp.ErrorMessageBox(e.Message, Res.Clipboard_ErrTitle_SetData);
              break;
            }
          }
        }
      }
      finally
      {
        EFPApp.EndWait();
      }
    }

    /// <summary>
    /// Извлекает изображение из буфера обмена.
    /// Выполняется <see cref="RepeatCount"/> попыток с задержкой <see cref="RepeatDelay"/> между попытками. Потом выводится сообщение об ошибке.
    /// Свойство <see cref="HasError"/> может использоваться для проверки результатов извлечения данных в прикладном коде.
    /// </summary>
    /// <returns>Данные из буфера обмена или null в случае ошибки или если нет данных в таком формате</returns>
    public Image GetImage()
    {
      Image res = null;
      EFPApp.BeginWait(Res.Clipboard_Phase_GetData, "Paste");
      try
      {
        _HasError = true;
        for (int i = RepeatCount - 1; i >= 0; i--)
        {
          try
          {
            res = Clipboard.GetImage();
            _HasError = false;
            break;
          }
          catch (Exception e)
          {
            if (i > 0)
              Thread.Sleep(RepeatDelay);
            else
            {
              EFPApp.ErrorMessageBox(e.Message, Res.Clipboard_ErrTitle_GetData);
              break;
            }
          }
        }
      }
      finally
      {
        EFPApp.EndWait();
      }
      return res;
    }

    #endregion

    #endregion
  }
#endif

  /// <summary>
  /// Действия, выполняемые при возникновении ошибки при работе с буфером обмена
  /// </summary>
  public enum EFPClipboardErrorHandling
  {
    /// <summary>
    /// Вывести сообщение с помощью <see cref="EFPApp.ErrorMessageBox(string, string)"/>.
    /// </summary>
    MessageBox,

    /// <summary>
    /// Не перехватывать ошибку. Исключения перехватываются в вызывающем коде.
    /// </summary>
    ThrowException,
  }

  /// <summary>
  /// Расширение работы с буфером обмена (с <see cref="System.Windows.Forms.Clipboard"/>).
  /// Методы чтения/записи выполняют по несколько попыток обращения к буферу обмена, прежде чем признать ошибку.
  /// Вызывающий код, например, обработчик команды копирования в управляющем элементе, создает экземпляр объекта
  /// <see cref="EFPClipboard"/> и вызывает один из методов GetXXX() или SetXXX().
  /// 
  /// Класс не является потокобезопасным, но методы выполняют блокировку на случай обращения к буферу обмена из нескольких потоков.
  /// Также генерируется ошибка в случае реентрантного вызова.
  /// </summary>
  public sealed class EFPClipboard
  {
    #region Конструктор

    /// <summary>
    /// Инициализация значений по умолчанию
    /// </summary>
    public EFPClipboard()
    {
      _RepeatCount = DefaultRepeatCount;
      _RepeatDelay = DefaultRepeatDelay;
      _ErrorIfEmpty = false;
      _ErrorHandling = EFPClipboardErrorHandling.MessageBox;
    }

    static EFPClipboard()
    {
      #region Текстовый формат по умолчанию

      // В исходных текстах Net Framework так:
      //if (Environment.OSVersion.Platform != System.PlatformID.Win32NT ||
      //    Environment.OSVersion.Version.Major < 5)
      //  _DefaultTextFormat = TextDataFormat.Text;
      //else
      //  _DefaultTextFormat = TextDataFormat.UnicodeText;

      // В Mono задано использование TextDataFormat.UnicodeText всегда.

      // Комбинируем
      switch (Environment.OSVersion.Platform)
      {
        case PlatformID.Win32NT:
          if (Environment.OSVersion.Version.Major < 5)
            _DefaultTextFormat = TextDataFormat.Text;
          else
            _DefaultTextFormat = TextDataFormat.UnicodeText;
          break;
        case PlatformID.Win32Windows:
        case PlatformID.WinCE:
        case PlatformID.Win32S:
          _DefaultTextFormat = TextDataFormat.Text;
          break;
        default:
          _DefaultTextFormat = TextDataFormat.UnicodeText;
          break;
      }

      #endregion
    }

    #endregion

    #region Управляющие свойства

    /// <summary>
    /// Количество попыток выполнить операцию с буфером обмена, прежде чем выдать сообщение об ошибке.
    /// По умолчанию используется значение <see cref="DefaultRepeatCount"/>.
    /// </summary>
    public int RepeatCount
    {
      get { return _RepeatCount; }
      set
      {
        if (value < 1)
          throw ExceptionFactory.ArgOutOfRange("value", value, 1, null);
        _RepeatCount = value;
      }
    }
    private int _RepeatCount;

    /// <summary>
    /// Интервал времени в миллисекундах между попытками выполнить операцию с буфером обмена.
    /// По умолчанию используется <see cref="DefaultRepeatDelay"/>. Минимальное значение 1 мс.
    /// Свойство имеет смысл при <see cref="RepeatCount"/> больше 1.
    /// </summary>
    public int RepeatDelay
    {
      get { return _RepeatDelay; }
      set
      {
        if (value < 1)
          throw ExceptionFactory.ArgOutOfRange("value", value, 1, null);
        _RepeatDelay = value;
      }
    }
    private int _RepeatDelay;

    /// <summary>
    /// Если true, то методы GetXXX() выводят сообщение или выбрасывают исключение, если буфер обмена пустой или не содержит данных в нужном формате.
    /// По умолчанию - false.
    /// </summary>
    public bool ErrorIfEmpty { get { return _ErrorIfEmpty; } set { _ErrorIfEmpty = value; } }
    private bool _ErrorIfEmpty;

    /// <summary>
    /// Способ обработки ошибок с буфером обмена.
    /// По умолчанию - <see cref="EFPClipboardErrorHandling.MessageBox"/> - выводится сообщение.
    /// </summary>
    public EFPClipboardErrorHandling ErrorHandling { get { return _ErrorHandling; } set { _ErrorHandling = value; } }
    private EFPClipboardErrorHandling _ErrorHandling;

    #endregion

    #region Обработка ошибок

    // Алгоритм использования для простого метода:
    // 
    // _Exception = null; // очистка признака ошибки
    // try
    // {
    //   using (new ClipboardLocker(this, "текст заставки", "значок заставки"))
    //   {
    //     OpCounter counter=new OpCounter(this); // инициализация счетчика
    //     while (true)
    //     {
    //       try
    //       {
    //         Clipboard.XXX() // действие, которое может привести к ошибке
    //         break; // если нет ошибки, то все ок.
    //       }
    //       catch (Exception e1)  
    //       {
    //         if (counter.NeedsRetrow(e1)) // проверяет счетчик и выполяет задержку 100мс
    //           throw; // важно, чтобы исключение было перевыброшено с оригинальным стеком вызовов
    //       }
    //     }
    //   }
    // }
    // catch(Exception e2)
    // {
    //   if (this.NeedsRetrow(e2)) // вызывает EFPApp.ErrorMessageBox()
    //     throw; // важно, чтобы исключение было перевыброшено с оригинальным стеком вызовов
    // }  

    // Алгоритм использования для составных операций чтения
    // 
    // x = GetXXX(); // вызов основного метода
    // if (Exception != null) 
    //   return null; // если была ошибка, то сообщение уже выдано. Просто возвращаем пустое значение
    // try
    // {
    //   Выполняем преобразование, которое может выбросить исколючение.
    //   Это действие не надо выполнять в цикле
    //   return Преобразование(x); 
    // }
    // catch(Exception e)  
    // {
    //   if (this.NeedsRetrow(e)) // вызывает EFPApp.ErrorMessageBox()
    //     throw; // важно, чтобы исключение было перевыброшено с оригинальным стеком вызовов
    // }  
    // return null;


    /// <summary>
    /// Объект исключения при возникновении ошибки.
    /// Используйте проверку свойства на null при реализации команд типа "Cut", когда важно убедиться,
    /// что данные были скопированы в буфер обмена.
    /// Если ошибка была показана из-за установки свойства <see cref="ErrorIfEmpty"/>=true, то 
    /// объект исключения также сохраняется.
    /// </summary>
    public Exception Exception { get { return _Exception; } }
    private Exception _Exception;

    /// <summary>
    /// Повторитель операций с буфером обмена
    /// </summary>
    private struct OpCounter
    {
      #region Конструктор  

      public OpCounter(EFPClipboard owner)
      {
        _Owner = owner;
        _Count = owner.RepeatCount;
      }

      private readonly EFPClipboard _Owner;
      private int _Count;

      #endregion

      #region Выполнение проверки

      public bool NeedsRetrow(Exception e)
      {
        _Count--;
        if (_Count < 1)
          return true;

        Application.DoEvents();
        Thread.Sleep(_Owner.RepeatDelay);
        return false;
      }

      #endregion
    }

    private bool NeedsRetrow(Exception e, bool isSetData)
    {
      _Exception = e;
      switch (ErrorHandling)
      {
        case EFPClipboardErrorHandling.MessageBox:
          EFPApp.ErrorMessageBox(e.Message,
            isSetData ? Res.Clipboard_ErrTitle_SetData : Res.Clipboard_ErrTitle_GetData);
          return false;
        case EFPClipboardErrorHandling.ThrowException:
          return true;
        default:
          throw new BugException("ErrorHandling=" + ErrorHandling.ToString());
      }
    }

    #endregion

    #region Многопоточность и реентрантный вызов

    private static readonly object _LockObject = new object();

    [ThreadStatic]
    private static bool _InsideCall;

    private struct ClipboardLocker : IDisposable
    {
      public ClipboardLocker(EFPClipboard owner, string waitMessage, string imageKey)
      {
        if (_InsideCall)
          throw new ReenteranceException(); // TODO: текст
        _InsideCall = true;
        Monitor.Enter(_LockObject);
        EFPApp.BeginWait(waitMessage, imageKey);
      }

      public void Dispose()
      {
        EFPApp.EndWait();
        Monitor.Exit(_LockObject);
        _InsideCall = false;
      }
    }

    #endregion

    #region Text

    /// <summary>
    /// Возвращает <see cref="TextDataFormat.UnicodeText"/> или <see cref="TextDataFormat.Text"/>, в зависимости от операционной системы
    /// </summary>
    public static TextDataFormat DefaultTextFormat { get { return _DefaultTextFormat; } }
    private static readonly TextDataFormat _DefaultTextFormat;

    /// <summary>
    /// Помещает текст в буфер обмена.
    /// </summary>
    /// <param name="s">Копируемый текст</param>
    public void SetText(string s)
    {
      _Exception = null;
      try
      {
        using (new ClipboardLocker(this, Res.Clipboard_Phase_SetData, "Copy"))
        {
          OpCounter counter = new OpCounter(this);
          while (true)
          {
            try
            {
              if (String.IsNullOrEmpty(s))
                Clipboard.Clear();
              else
                Clipboard.SetText(s);
              break;
            }
            catch (Exception e1)
            {
              if (counter.NeedsRetrow(e1))
                throw;
            }
          }
        }
      }
      catch (Exception e2)
      {
        if (this.NeedsRetrow(e2, true))
          throw;
      }
    }

    /// <summary>
    /// Извлекает текст из буфера обмена.
    /// Если буфер обмена пуст или не содержит текста, ошибка выдается в зависимости от <see cref="ErrorIfEmpty"/>.
    /// 
    /// Если требуется извлечь текст из объекта <see cref="IDataObject"/>, используйте <see cref="WinFormsTools.GetText(IDataObject)"/>.
    /// </summary>
    /// <returns>Текст из буфера обмена или пустая строка</returns>
    public string GetText()
    {
      return GetText(DefaultTextFormat);
    }


    /// <summary>
    /// Извлекает текст из буфера обмена.
    /// Если буфер обмена пуст или не содержит текста, ошибка выдается в зависимости от <see cref="ErrorIfEmpty"/>.
    /// </summary>
    /// <param name="format">Формат данных</param>
    /// <returns>Текст из буфера обмена или пустая строка</returns>
    public string GetText(TextDataFormat format)
    {
      string res = String.Empty;
      _Exception = null;
      try
      {
        using (new ClipboardLocker(this, Res.Clipboard_Phase_GetData, "Paste"))
        {
          OpCounter counter = new OpCounter(this);
          while (true)
          {
            try
            {
              res = Clipboard.GetText(format);
              if (res == null)
                res = String.Empty; // на всякий случай
              break;
            }
            catch (Exception e1)
            {
              if (counter.NeedsRetrow(e1))
                throw;
            }
          }
        }

        if (res.Length == 0 && ErrorIfEmpty)
          throw new InvalidOperationException(Res.Clipboard_Err_NoText);
      }
      catch (Exception e2)
      {
        if (this.NeedsRetrow(e2, false))
          throw;
      }

      return res;
    }

    /// <summary>
    /// Возвращает прямоугольный блок текста из формата CSV или текстового формата.
    /// Используются данные в формате <see cref="TextDataFormat.CommaSeparatedValue"/>, а при отсутствии - в <see cref="TextDataFormat.Text"/>.
    /// Обычно следует использовать объект <see cref="EFPPasteTextMatrixFormat"/> для реализации вставки.
    /// Если буфер обмена пуст или не содержит текста, ошибка выдается в зависимости от <see cref="ErrorIfEmpty"/>.
    /// 
    /// Если требуется извлечь текст из объекта <see cref="IDataObject"/>, используйте <see cref="WinFormsTools.GetTextMatrix(IDataObject)"/>, 
    /// <see cref="WinFormsTools.GetTextMatrixText(IDataObject)"/> или <see cref="WinFormsTools.GetTextMatrixCsv(IDataObject)"/>.
    /// </summary>
    /// <returns>Матрица текста или null, если буфер обмена пуст, или возникла ошибка</returns>
    public string[,] GetTextMatrix()
    {
      IDataObject dobj = GetDataObject();
      if (dobj == null || Exception != null)
        return null;

      try
      {
        string[,] res = WinFormsTools.GetTextMatrix(dobj);
        if (res == null && ErrorIfEmpty)
          throw new InvalidOperationException(Res.Clipboard_Err_NoText);
        return res;
      }
      catch (Exception e)
      {
        if (this.NeedsRetrow(e, false))
          throw;
      }
      return null;
    }

    /// <summary>
    /// Помещает в буфер обмена данные в форматах Text и CSV.
    /// </summary>
    /// <param name="a">Двумерный массив строк</param>
    public void SetTextMatrix(string[,] a)
    {
      DataObject dobj = new DataObject();
      WinFormsTools.SetTextMatrix(dobj, a);
      SetDataObject(dobj, true);
    }

    #endregion

    #region DataObject

    /// <summary>
    /// Помещает данные в буфер обмена.
    /// </summary>
    /// <param name="data">Данные</param>
    /// <param name="copy">Если нужно сделать данные сохраняющимися в буфере обмена после завершения приложения</param>
    public void SetDataObject(object data, bool copy)
    {
      _Exception = null;
      try
      {
        using (new ClipboardLocker(this, Res.Clipboard_Phase_SetData, "Copy"))
        {
          OpCounter counter = new OpCounter(this);
          while (true)
          {
            try
            {
              Clipboard.SetDataObject(data, copy, 1, 0);
              break;
            }
            catch (Exception e1)
            {
              if (counter.NeedsRetrow(e1))
                throw;
            }
          }
        }
      }
      catch (Exception e2)
      {
        if (this.NeedsRetrow(e2, true))
          throw;
      }
    }

    /// <summary>
    /// Извлекает данные из буфера обмена.
    /// Выполняется <see cref="RepeatCount"/> попыток с задержкой <see cref="RepeatDelay"/> между попытками. Потом выводится сообщение об ошибке.
    /// Если буфер обмена пуст или не содержит текста, ошибка выдается в зависимости от <see cref="ErrorIfEmpty"/>.
    /// </summary>
    /// <returns>Данные из буфера обмена или null в случае ошибки</returns>
    public IDataObject GetDataObject()
    {
      IDataObject res = null;
      _Exception = null;
      try
      {
        using (new ClipboardLocker(this, Res.Clipboard_Phase_GetData, "Paste"))
        {
          OpCounter counter = new OpCounter(this);
          while (true)
          {
            try
            {
              res = Clipboard.GetDataObject();
              break;
            }
            catch (Exception e1)
            {
              if (counter.NeedsRetrow(e1))
                throw;
            }
          }
        }

        if ((res == null || res.GetFormats().Length == 0) && ErrorIfEmpty)
          throw new InvalidOperationException(Res.Clipboard_Err_Empty);
      }
      catch (Exception e2)
      {
        if (this.NeedsRetrow(e2, false))
          throw;
      }

      return res;
    }

    #endregion

    #region Data

    /// <summary>
    /// Помещает данные в буфер обмена.
    /// </summary>
    /// <param name="format">Формат данных</param>
    /// <param name="data">Данные</param>
    public void SetData(string format, object data)
    {
      _Exception = null;
      try
      {
        using (new ClipboardLocker(this, Res.Clipboard_Phase_SetData, "Copy"))
        {
          OpCounter counter = new OpCounter(this);
          while (true)
          {
            try
            {
              Clipboard.SetData(format, data);
              break;
            }
            catch (Exception e1)
            {
              if (counter.NeedsRetrow(e1))
                throw;
            }
          }
        }
      }
      catch (Exception e2)
      {
        if (this.NeedsRetrow(e2, true))
          throw;
      }
    }

    /// <summary>
    /// Извлекает данные из буфера обмена.
    /// </summary>
    /// <param name="format">Требуемый формат данных</param>
    /// <returns>Данные из буфера обмена или null в случае ошибки или если нет данных в таком формате</returns>
    public object GetData(string format)
    {
      object res = null;
      _Exception = null;
      try
      {
        using (new ClipboardLocker(this, Res.Clipboard_Phase_GetData, "Paste"))
        {
          OpCounter counter = new OpCounter(this);
          while (true)
          {
            try
            {
              res = Clipboard.GetData(format);
              break;
            }
            catch (Exception e1)
            {
              if (counter.NeedsRetrow(e1))
                throw;
            }
          }
        }

        if (res == null && ErrorIfEmpty)
          throw new InvalidOperationException(String.Format(Res.Clipboard_Err_NoDataFormat, format));
      }
      catch (Exception e2)
      {
        if (this.NeedsRetrow(e2, false))
          throw;
      }

      return res;
    }

    #endregion

    #region Image

    /// <summary>
    /// Помещает изображение в буфер обмена.
    /// </summary>
    /// <param name="image">Изображение</param>
    public void SetImage(Image image)
    {
      _Exception = null;
      try
      {
        using (new ClipboardLocker(this, Res.Clipboard_Phase_SetData, "Copy"))
        {
          OpCounter counter = new OpCounter(this);
          while (true)
          {
            try
            {
              if (image == null)
                Clipboard.Clear();
              else
                Clipboard.SetImage(image);
              break;
            }
            catch (Exception e1)
            {
              if (counter.NeedsRetrow(e1))
                throw;
            }
          }
        }
      }
      catch (Exception e2)
      {
        if (this.NeedsRetrow(e2, true))
          throw;
      }
    }

    /// <summary>
    /// Извлекает изображение из буфера обмена.
    /// </summary>
    /// <returns>Данные из буфера обмена или null в случае ошибки или если нет данных в таком формате</returns>
    public Image GetImage()
    {
      Image res = null;
      _Exception = null;
      try
      {
        using (new ClipboardLocker(this, Res.Clipboard_Phase_GetData, "Paste"))
        {
          OpCounter counter = new OpCounter(this);
          while (true)
          {
            try
            {
              res = Clipboard.GetImage();
              break;
            }
            catch (Exception e1)
            {
              if (counter.NeedsRetrow(e1))
                throw;
            }
          }
        }

        if (res == null && ErrorIfEmpty)
          throw new InvalidOperationException(Res.Clipboard_Err_NoImage);
      }
      catch (Exception e2)
      {
        if (this.NeedsRetrow(e2, false))
          throw;
      }
      return res;
    }

    #endregion

    #region Статические свойства

    /// <summary>
    /// Количество попыток выполнить операцию с буфером обмена, прежде чем выдать сообщение об ошибке.
    /// По умолчанию делается 10 попыток.
    /// </summary>
    public static int DefaultRepeatCount
    {
      get { return _DefaultRepeatCount; }
      set
      {
        if (value < 1)
          throw ExceptionFactory.ArgOutOfRange("value", value, 1, null);
        _DefaultRepeatCount = value;
      }
    }
    private static int _DefaultRepeatCount = 10;

    /// <summary>
    /// Интервал времени в миллисекундах между попытками выполнить операцию с буфером обмена.
    /// Значение по умолчанию - 100 мс. Минимальное значение 1.
    /// </summary>
    public static int DefaultRepeatDelay
    {
      get { return _DefaultRepeatDelay; }
      set
      {
        if (value < 1)
          throw ExceptionFactory.ArgOutOfRange("value", value, 1, null);
        _DefaultRepeatDelay = value;
      }
    }
    private static int _DefaultRepeatDelay = 100;

    #endregion
  }
}
