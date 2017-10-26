using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using SidebarDiagnostics.Windows;

namespace SidebarDiagnostics.Converters
{
    public class IntToStringConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            String _format = (String)parameter;

            if (String.IsNullOrEmpty(_format))
            {
                return value.ToString();
            }
            else
            {
                return String.Format(culture, _format, value);
            }
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            Int32 _return = 0;

            Int32.TryParse(value.ToString(), out _return);

            return _return;
        }
    }

    public class HotkeyToStringConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            Hotkey _hotkey = (Hotkey)value;

            if (_hotkey == null)
            {
                return "None";
            }

            return
                (_hotkey.AltMod ? "Alt + " : "") +
                (_hotkey.CtrlMod ? "Ctrl + " : "") +
                (_hotkey.ShiftMod ? "Shift + " : "") +
                (_hotkey.WinMod ? "Win + " : "") +
                new KeyConverter().ConvertToString(_hotkey.WinKey);
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class PercentConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            Double _value = (Double)value;

            return String.Format("{0:0}%", _value);
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class BoolInverseConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            return !(Boolean)value;
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            return !(Boolean)value;
        }
    }

    public class MetricLabelConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            String _value = (String)value;

            return String.Format("{0}:", _value);
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class FontToSpaceConverter : IValueConverter
    {
        public Object Convert(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            Int32 _value = (Int32)value;

            return new Thickness(0, 0, _value * 0.4d, 0);
        }

        public Object ConvertBack(Object value, Type targetType, Object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
