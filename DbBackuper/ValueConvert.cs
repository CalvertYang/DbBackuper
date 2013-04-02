using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DbBackuper
{
    /// <summary>
    /// Converter 給 xaml 轉換 Binding 資料 : Bool 轉 Width 120px
    /// </summary>
    public class DataContextSetConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            try
            {
                if (value == null || !((bool)value)) return 0.0;

                return 120;
            }
            catch (InvalidCastException) { }
            return "<Unknown Value>";
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
