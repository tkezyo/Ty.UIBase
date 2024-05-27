using System;
using System.Windows;
using System.Windows.Controls;
using Ty.Services.Configs;
using Ty.ViewModels.Configs;

namespace Ty.Converters
{
    public class PropertyTypeConverter : DataTemplateSelector
    {
        public DataTemplate? StringTemplate { get; set; }
        public DataTemplate? NumberTemplate { get; set; }
        public DataTemplate? BooleanTemplate { get; set; }
        public DataTemplate? DateTimeTemplate { get; set; }
        public DataTemplate? DateOnlyTemplate { get; set; }
        public DataTemplate? TimeOnlyTemplate { get; set; }
        public DataTemplate? HasOptionTemplate { get; set; }
        public DataTemplate? ObjectTemplate { get; set; }
        public DataTemplate? ArrayTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            //container的资源中获取模板
            //获取property的类型

            if (item is null)
            {
                return null;
            }
            if (item is not ConfigViewModel obj)
            {
                return null;
            }
            if (obj.Options is not null && obj.Options.Count > 0)
            {
                return HasOptionTemplate;
            }
            if (obj.Type == ConfigModelType.String)
            {
                return StringTemplate;
            }
            if (obj.Type == ConfigModelType.Number)
            {
                return NumberTemplate;
            }
            if (obj.Type == ConfigModelType.Boolean)
            {
                return BooleanTemplate;
            }
            if (obj.Type == ConfigModelType.DateTime)
            {
                return DateTimeTemplate;
            }
            if (obj.Type == ConfigModelType.DateOnly)
            {
                return DateOnlyTemplate;
            }
            if (obj.Type == ConfigModelType.TimeOnly)
            {
                return TimeOnlyTemplate;
            }
            if (obj.Type == ConfigModelType.Object)
            {
                return ObjectTemplate;
            }
            if (obj.Type == ConfigModelType.Array)
            {
                return ArrayTemplate;
            }

            return null;
        }
    }
}
