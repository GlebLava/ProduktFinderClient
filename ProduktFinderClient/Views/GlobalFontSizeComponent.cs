using ProduktFinderClient.Models;
using System;
using System.Windows;
using System.Windows.Input;

namespace ProduktFinderClient.Views;

public class GlobalFontSizeComponent
{
    public static int GlobalFontSize { get; private set; }


    public delegate void Notify(object? sender);
    public static event Notify? OnGlobalFontSizeIncreased;
    public static event Notify? OnGlobalFontSizeDecreased;
    public static event Notify? OnGlobalFontSizeChanged;

    public static readonly int MAX_FONT_SIZE = 50;
    public static readonly int MIN_FONT_SIZE = 5;

    private readonly Window _window;

    static GlobalFontSizeComponent()
    {
        GlobalFontSize = LoadSaveSystem.LoadGlobalFontSize();
        OnGlobalFontSizeChanged += (o) => LoadSaveSystem.SaveGlobalFontSize(GlobalFontSize);
    }


    public GlobalFontSizeComponent(Window window)
    {
        window.PreviewMouseWheel += OnPreviewMouseWheel;
        _window = window;
        _window.FontSize = GlobalFontSize;

        OnGlobalFontSizeChanged += AssignGlobalFontSizeToWindow;
    }

    /// <summary>
    /// This method does Invoke OnGlobalFontSizeIncreased and OnGlobalFontSizeChanged!
    /// </summary>
    /// <returns>If the global Font Size was actually increased</returns>
    public static bool IncreaseGlobalFontSize(object? sender)
    {
        if (GlobalFontSize < MAX_FONT_SIZE)
        {
            GlobalFontSize++;
            OnGlobalFontSizeIncreased?.Invoke(sender);
            OnGlobalFontSizeChanged?.Invoke(sender);
            return true;
        }
        return false;
    }

    /// <summary>
    /// This method does Invoke OnGlobalFontSizeDecreased and OnGlobalFontSizeChanged!
    /// </summary>
    /// <returns>If the global Font Size was actually decreased</returns>
    public static bool DecreaseGlobalFontSize(object? sender)
    {
        if (GlobalFontSize > MIN_FONT_SIZE)
        {
            GlobalFontSize--;
            OnGlobalFontSizeDecreased?.Invoke(sender);
            OnGlobalFontSizeChanged?.Invoke(sender);
            return true;
        }
        return false;
    }

    private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (Keyboard.IsKeyDown(Key.LeftCtrl))
        {
            if (e.Delta < 0 && GlobalFontSize > MIN_FONT_SIZE)
            {
                GlobalFontSize--;
                OnGlobalFontSizeDecreased?.Invoke(this);
                OnGlobalFontSizeChanged?.Invoke(this);
            }
            else if (GlobalFontSize < MAX_FONT_SIZE)
            {
                GlobalFontSize++;
                OnGlobalFontSizeIncreased?.Invoke(this);
                OnGlobalFontSizeChanged?.Invoke(this);
            }
        }
    }


    private void AssignGlobalFontSizeToWindow(object? sender)
    {
        _window.FontSize = GlobalFontSize;
    }


}
