// Part of FreeLibSet.
// See copyright notices in "license" file in the FreeLibSet root directory.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace FreeLibSet.Forms
{
  #region Интерфейс IEFPControlWithToolBar<T>

  /// <summary>
  /// Интерфейс управляющего элемента с панелью инструментов.
  /// Обычно используется шаблонный класс EFPControlWithToolBar.
  /// Нетипизированная версия интерфейса
  /// </summary>
  public interface IEFPControlWithToolBar
  {
    /// <summary>
    /// BaseProvider
    /// </summary>
    EFPBaseProvider BaseProvider { get;}

    /// <summary>
    /// Основной управляющий элемент
    /// </summary>
    Control Control { get;}

    /// <summary>
    /// Панель для размещения инструментов.
    /// Может быть null, если EFPApp.ShowControlToolBars=false
    /// </summary>
    Panel ToolBarPanel { get;}
  }

  /// <summary>
  /// Интерфейс управляющего элемента с панелью инструментов.
  /// Обычно используется шаблонный класс EFPControlWithToolBar
  /// </summary>
  /// <typeparam name="T">Тип управляющего элемента</typeparam>
  public interface IEFPControlWithToolBar<T> : IEFPControlWithToolBar
    where T : Control
  {
    /// <summary>
    /// Основной управляющий элемент
    /// </summary>
    new T Control { get;}
  }

  #endregion

  /// <summary>
  /// Управляющий элемент с панелью инструментов
  /// </summary>
  /// <typeparam name="T">Тип управляющего элемента</typeparam>
  public class EFPControlWithToolBar<T> : IEFPControlWithToolBar<T>
    where T : Control, new()
  {
    #region Вложенный класс провайдера

    private class ExProvider : EFPBaseProvider
    {
      #region Конструктор

      public ExProvider(Panel toolBarPanel)
      {
        _ToolBarPanel = toolBarPanel;
      }

      #endregion

      #region Поля

      private Panel _ToolBarPanel;

      private bool _ToolBarPanelAssigned;

      #endregion

      #region Переопределенные методы

      // 19.06.2019
      // Присоединение панели инструментов выполняется не в методе InitCommandItemList(), а в Add() (с учетом возможного удаления и замены элементов)
      // В старом варианте панель инструментов не рисовалась, если форма выводилась на экран при запуске программы из EFPApp.LoadComposition()

      protected override void OnControlProviderAdded(EFPControlBase controlProvider)
      {
        base.OnControlProviderAdded(controlProvider);

        if (_ToolBarPanel != null)
        {
          if (!_ToolBarPanelAssigned)
          {
            controlProvider.ToolBarPanel = _ToolBarPanel;
            _ToolBarPanelAssigned = true;
          }
        }
      }

      #endregion
    }

    #endregion

    #region Конструкторы

    /// <summary>
    /// Создает новый управляющий элемент типа <typeparamref name="T"/> и панель инструментов. 
    /// Элементы размещаются в <paramref name="parent"/>.Control.
    /// В качестве <paramref name="parent"/> обычно используется EFPTabPage
    /// </summary>
    /// <param name="parent">Провайдер родительского элемента</param>
    public EFPControlWithToolBar(EFPControlBase parent)
      : this(parent.BaseProvider, parent.Control, new T())
    {
    }

    /// <summary>
    /// Создает новый управляющий элемент типа <typeparamref name="T"/> и панель инструментов. 
    /// Элементы размещаются в родительском элементе <paramref name="parent"/>.
    /// Обычно это Panel или другая панель.
    /// Основная версия конструктора.
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="parent">Родительский управляюший элемент</param>
    public EFPControlWithToolBar(EFPBaseProvider baseProvider, Control parent)
      : this(baseProvider, parent, new T())
    {
    }

    /// <summary>
    /// Использует готовый управляюший элемент <paramref name="control"/> и создает новую панель инструментов. 
    /// Элементы размещаются в родительском элементе <paramref name="parent"/>.
    /// Обычно это Panel или другая панель.
    /// </summary>
    /// <param name="baseProvider">Базовый провайдер</param>
    /// <param name="parent">Родительский управляюший элемент</param>
    /// <param name="control">Основной управляющий элемент</param>
    public EFPControlWithToolBar(EFPBaseProvider baseProvider, Control parent, T control)
    {
#if DEBUG
      if (baseProvider == null)
        throw new ArgumentNullException("baseProvider");
      if (parent == null)
        throw new ArgumentNullException("parent");
      if (parent.Controls.Count > 0)
        throw new InvalidOperationException("Родительский управляющий элемент " + parent.ToString() + " уже содержит элементы");
      if (control == null)
        throw new ArgumentNullException("control");
      if (control.Parent != null)
        throw new ArgumentException("Основной управлящий элемент не должен иметь родителя", "control");
#endif


      _Control = control;
      _Control.Dock = DockStyle.Fill;
      parent.Controls.Add(_Control);

      if (EFPApp.ShowControlToolBars)
      {
        _ToolBarPanel = new Panel();
        _ToolBarPanel.Size = new Size(24, 24);
        _ToolBarPanel.Dock = DockStyle.Top;
        _ToolBarPanel.Visible = false; // 10.09.2012
        parent.Controls.Add(_ToolBarPanel);
      }

      _BaseProvider = new ExProvider(_ToolBarPanel);
      _BaseProvider.Parent = baseProvider;
    }

    #endregion

    #region Свойства

    /// <summary>
    /// Задается в конструкторе
    /// </summary>
    public EFPBaseProvider BaseProvider { get { return _BaseProvider; } }
    private ExProvider _BaseProvider;

    /// <summary>
    /// Основной управляющий элемент. Задается в конструкторе
    /// </summary>
    public T Control { get { return _Control; } }
    private T _Control;

    Control IEFPControlWithToolBar.Control { get { return _Control; } }

    /// <summary>
    /// Панель для размещения инструментов. Создается в конструкторе. Может быть
    /// null, если EFPApp.ShowControlToolBars=false
    /// </summary>
    public Panel ToolBarPanel { get { return _ToolBarPanel; } }
    private Panel _ToolBarPanel;

    #endregion
  }
}
