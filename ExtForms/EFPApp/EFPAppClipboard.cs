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
  /// <summary>
  /// Расширение работы с буфером обмена.
  /// Методы чтения/записи выполняют по несколько попыток обращения к буферу обмена, прежде чем выдать сообщение об ошибке.
  /// В случае неустранимой ошибки выдается сообщение с помощью MessageBox().
  /// Класс реализует свойство EFPApp.Clipboard
  /// </summary>
  public sealed class EFPAppClipboard
  {
    #region Конструктор

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
    /// По умолчанию делается 10 попыток
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
    /// Свойство имеет смысл при RepeatCount больше 1.
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
    /// Признак ошибки не выставляется, если в буфере нет данных в подходящем формате
    /// </summary>
    public bool HasError { get { return _HasError; } }
    private bool _HasError;

    #region Text

    /// <summary>
    /// Возвращает TextDataFormat.UnicodeText или TextDataFormat.Text, в зависимости от операционной системы
    /// </summary>
    public TextDataFormat DefaultTextFormat { get { return _DefaultTextFormat; } }
    private TextDataFormat _DefaultTextFormat;

    /// <summary>
    /// Помещает текст в буфер обмена.
    /// Выполняется RepeatCount попыток с задержкой RepeatDelay между попытками. Потом выводится сообщение об ошибке.
    /// </summary>
    /// <param name="s">Копируемый текст</param>
    public void SetText(string s)
    {
      EFPApp.BeginWait("Копирование текста в буфер обмена", "Copy");
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
              EFPApp.ErrorMessageBox(e.Message, "Ошибка записи в буфер обмена");
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
    /// Выполняется RepeatCount попыток с задержкой RepeatDelay между попытками. 
    /// Если буфер обмена пуст или не содержит текста, сообщение об ошибке не выдается.
    /// </summary>
    /// <returns>Текст из буфера обмена или пустая строка</returns>
    public string GetText()
    {
      return GetText(DefaultTextFormat, false);
    }


    /// <summary>
    /// Извлекает текст из буфера обмена.
    /// Выполняется RepeatCount попыток с задержкой RepeatDelault между попытками. Потом выводится сообщение об ошибке,
    /// если <paramref name="messageIfEmpty"/>=true.
    /// </summary>
    /// <param name="messageIfEmpty">Если true, то будет выведено сообщение об отсутствии текста в буфере обмена</param>
    /// <returns>Текст из буфера обмена или пустая строка</returns>
    public string GetText(bool messageIfEmpty)
    {
      return GetText(DefaultTextFormat, messageIfEmpty);
    }

    /// <summary>
    /// Извлекает текст из буфера обмена.
    /// Выполняется RepeatCount попыток с задержкой RepeatDelay между попытками. Потом выводится сообщение об ошибке,
    /// если <paramref name="messageIfEmpty"/>=true.
    /// </summary>
    /// <param name="format">Формат данных</param>
    /// <param name="messageIfEmpty">Если true, то будет выведено сообщение об отсутствии текста в буфере обмена</param>
    /// <returns>Текст из буфера обмена или пустая строка</returns>
    public string GetText(TextDataFormat format, bool messageIfEmpty)
    {
      string Res = String.Empty;
      EFPApp.BeginWait("Извлечение текста из буфера обмена", "Paste");
      try
      {
        _HasError = true;
        for (int i = RepeatCount - 1; i >= 0; i--)
        {
          try
          {
            Res = Clipboard.GetText(format);
            if (Res == null)
              Res = String.Empty; // на всякий случай
            _HasError = false;
            break;
          }
          catch (Exception e)
          {
            if (i > 0)
              Thread.Sleep(RepeatDelay);
            else
            {
              EFPApp.ErrorMessageBox(e.Message, "Ошибка чтения из буфера обмена");
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

      if (Res.Length == 0 && messageIfEmpty)
        EFPApp.ErrorMessageBox("Буфер обмена не содержит текста");

      return Res;
    }


    /// <summary>
    /// Возвращает прямоугольный блок текста из формата CSV или тектового формата.
    /// Выполняется RepeatCount попыток с задержкой RepeatDelay между попытками. 
    /// Используются данные в формате TextDataFormat.CommaSeparatedValue, а при отсутствии - в TextDataFormat.Text.
    /// При отсутствии текста в буфере обмена сообщение об ошибке не выводится.
    /// Обычно следует использовать объект EFPPasteTextMatrixFormat для реализации вставки.
    /// </summary>
    /// <returns>Матрица текста или null, если буфер обмена пуст, или возникла ошибка</returns>
    public string[,] GetTextMatrix()
    {
      return GetTextMatrix(false);
    }

    /// <summary>
    /// Возвращает прямоугольный блок текста из формата CSV или текстового формата.
    /// Выполняется RepeatCount попыток с задержкой RepeatDelay между попытками. Потом выводится сообщение об ошибке,
    /// если <paramref name="messageIfEmpty"/>=true.
    /// Используются данные в формате TextDataFormat.CommaSeparatedValue, а при отсутствии - в TextDataFormat.Text.
    /// Обычно следует использовать объект EFPPasteTextMatrixFormat для реализации вставки.
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
    /// Выполняется RepeatCount попыток с задержкой RepeatDelay между попытками. Потом выводится сообщение об ошибке.
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
    /// Выполняется RepeatCount попыток с задержкой RepeatDelay между попытками. Потом выводится сообщение об ошибке.
    /// </summary>
    /// <param name="data">Данные</param>
    /// <param name="copy">Если нужно сделать данными сохраняющимися после завершения приложения</param>
    public void SetDataObject(object data, bool copy)
    {
      EFPApp.BeginWait("Копирование данных в буфер обмена", "Copy");
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
              EFPApp.ErrorMessageBox(e.Message, "Ошибка записи в буфер обмена");
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
    /// Выполняется RepeatCount попыток с задержкой RepeatDelay между попытками. Потом выводится сообщение об ошибке.
    /// </summary>
    /// <returns>Данные из буфера обмена или null в случае ошибки</returns>
    public IDataObject GetDataObject()
    {
      IDataObject res = null;
      EFPApp.BeginWait("Извлечение данных из буфера обмена", "Paste");
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
              EFPApp.ErrorMessageBox(e.Message, "Ошибка чтения из буфера обмена");
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
    /// Выполняется RepeatCount попыток с задержкой RepeatDelay между попытками. Потом выводится сообщение об ошибке.
    /// </summary>
    /// <param name="format">Формат данных</param>
    /// <param name="data">Данные</param>
    public void SetData(string format, object data)
    {
      EFPApp.BeginWait("Копирование данных в буфер обмена", "Copy");
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
              EFPApp.ErrorMessageBox(e.Message, "Ошибка записи в буфер обмена");
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
    /// Выполняется RepeatCount попыток с задержкой RepeatDelay между попытками. Потом выводится сообщение об ошибке.
    /// </summary>
    /// <param name="format">Требуемый формат данных</param>
    /// <returns>Данные из буфера обмена или null в случае ошибки или если нет данных в таком формате</returns>
    public object GetData(string format)
    {
      object res = null;
      EFPApp.BeginWait("Извлечение данных из буфера обмена", "Paste");
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
              EFPApp.ErrorMessageBox(e.Message, "Ошибка чтения из буфера обмена");
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
    /// Выполняется RepeatCount попыток с задержкой RepeatDelay между попытками. Потом выводится сообщение об ошибке.
    /// </summary>
    /// <param name="image">Изображение</param>
    public void SetImage(Image image)
    {
      EFPApp.BeginWait("Копирование изображения в буфер обмена", "Copy");
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
              EFPApp.ErrorMessageBox(e.Message, "Ошибка записи в буфер обмена");
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
    /// Выполняется RepeatCount попыток с задержкой RepeatDelay между попытками. Потом выводится сообщение об ошибке.
    /// </summary>
    /// <returns>Данные из буфера обмена или null в случае ошибки или если нет данных в таком формате</returns>
    public Image GetImage()
    {
      Image res = null;
      EFPApp.BeginWait("Извлечение изображения из буфера обмена", "Paste");
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
              EFPApp.ErrorMessageBox(e.Message, "Ошибка чтения из буфера обмена");
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
}
