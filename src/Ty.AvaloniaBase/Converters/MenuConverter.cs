//using Avalonia.Data.Converters;
//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Volo.Abp.UI.Navigation;

//namespace Ty.Converters
//{
//    public class MenuConverter : IMultiValueConverter
//    {
//        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
//        {
//            if (values[0] is ApplicationMenuItem menuItem && values[1] is not null)
//            {
//                string pageName = values[1].ToString();
//                if (menuItem.Name == pageName)
//                {
//                    return true;
//                }
//                if (menuItem.Items.Any(c => c.Name == pageName))
//                {
//                    return true;
//                }
//            }

//            return false;
//        }
//    }
//}
