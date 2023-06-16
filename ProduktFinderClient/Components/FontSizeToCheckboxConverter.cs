using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace ProduktFinderClient.Components;

public class FontSizeToCheckboxConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double fontSize)
        {
            return fontSize / 20;
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double fontSize)
        {
            return fontSize * 20;
        }

        return value;
    }
}
