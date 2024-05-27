using System;
using System.Windows;
using System.Windows.Data;
using Ty.Services.Configs;
using Ty.ViewModels.Configs;

namespace Ty.Converters
{
    public class PropertyWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values[0] is not ConfigViewModel vm)
            {
                return 200;
            }
            if (values[1] is not double v)
            {
                return 200;
            }
            v= v - 40;
            if (vm.Type == ConfigModelType.Object || vm.Type == ConfigModelType.Array)
            {
                return v ;
            }
            else
            {
                //宽度在200-300之间，根据屏幕的宽度v来调整，使其占满屏幕,每一行可能有多个属性，所以要除以行数

                var count = v / 200;

                //向下取整
                count = (int)count;

                var width = v / count;

                return width;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return [];
        }
    }
}
