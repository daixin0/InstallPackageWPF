using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace InstallProgramWPF.Converters
{
    public class InstallStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;
            SetupState setupState = (SetupState)value;
            if (setupState == (SetupState)parameter)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed; 
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
