using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProduktFinderClient.Models;

public enum TextPreviewMethods
{
    Number,
}

public static class Utils
{
    public static Action<object?, TextCompositionEventArgs> GetPreviewMethod(TextPreviewMethods t)
    {
        switch (t)
        {
            case (TextPreviewMethods.Number):
                return TextPreviewNumber;
            default:
                throw new Exception("No Method found for " + t.ToString());
        }
    }

    public static T[] Concat<T>(this T[] x, T[] y)
    {
        if (x == null) throw new ArgumentNullException("x");
        if (y == null) throw new ArgumentNullException("y");
        int oldLen = x.Length;
        Array.Resize(ref x, x.Length + y.Length);
        Array.Copy(y, 0, x, oldLen, y.Length);
        return x;
    }


    public static void TextPreviewNumber(object? sender, TextCompositionEventArgs e)
    {
        // Only allow numbers
        Regex regex = new("^[0-9]*$");
        e.Handled = !regex.IsMatch(e.Text); // e.Handled = true blocks e.Handled = false lets the input through
    }




}
