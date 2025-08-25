using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AI_Knowledge_Generator.Converters
{
    public class InverseBooleanConverter : IValueConverter
    {
        public static readonly InverseBooleanConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return false;
        }
    }

    public class BooleanToVisibilityConverter : IValueConverter
    {
        public static readonly BooleanToVisibilityConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
                return visibility == Visibility.Visible;
            return false;
        }
    }

    public class MultiValueVisibilityConverter : IMultiValueConverter
    {
        public static readonly MultiValueVisibilityConverter Instance = new();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // For showing "No languages detected" message
            // Show when: DetectedLanguages.Count == 0 AND IsDetectingLanguages == false AND InputDirectory is not empty
            if (values.Length >= 3)
            {
                var count = values[0] as int? ?? 0;
                var isDetecting = values[1] as bool? ?? false;
                var inputDir = values[2] as string ?? "";

                if (count == 0 && !isDetecting && !string.IsNullOrEmpty(inputDir))
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }

            // For showing language list
            // Show when: DetectedLanguages.Count > 0 AND IsDetectingLanguages == false
            if (values.Length >= 2)
            {
                var count = values[0] as int? ?? 0;
                var isDetecting = values[1] as bool? ?? false;

                if (count > 0 && !isDetecting)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}