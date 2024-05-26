using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Ty.Module.Configs;

namespace Ty.Services.Configs
{
    public class ConfigManager
    {
        private Dictionary<string, List<ConfigModel>> ConfigModels { get; set; } = [];

        public void SetConfigModels(string name, List<ConfigModel> configModels)
        {
            ConfigModels[name] = configModels;
        }

        /// <summary>
        /// 生成 json,及 definition.json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<ConfigModel> GetConfigModel<T>()
            where T : class
        {
            if (ConfigModels.TryGetValue(typeof(T).FullName!, out var list))
            {
                return list;
            }

            List<ConfigModel> configs = [];
            GenerateConfigModel(typeof(T), configs);

            return configs;
        }

        /// <summary>
        /// 生成属性
        /// </summary>
        /// <param name="type"></param>
        /// <param name="typeModels"></param>
        /// <param name="mainType"></param>
        private static void GenerateConfigModel(Type type, List<ConfigModel> typeModels, bool mainType = true)
        {
            //如果是基础类型，那么直接返回
            if (type.IsPrimitive || type == typeof(string) || type == typeof(char) || type == typeof(decimal) || type == typeof(DateTime) || type == typeof(TimeSpan) || type == typeof(DateOnly) || type == typeof(TimeOnly))
            {
                return;
            }

            if (typeModels.Any(c => c.TypeName == type.Name))
            {
                return;
            }
            ConfigModel typeModel = new() { TypeName = type.Name, MainType = mainType };
            typeModels.Add(typeModel);
            //使用反射遍历所有属性
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                var configModel = new PropertyModel(property.Name);
                SetLength(property.PropertyType);
                //如果是String
                if (property.PropertyType == typeof(string) ||
                        property.PropertyType == typeof(char))
                {
                    configModel.Type = ConfigModelType.String;

                }
                //如果是Number
                else if (property.PropertyType == typeof(int) ||
                        property.PropertyType == typeof(double) ||
                        property.PropertyType == typeof(float) ||
                        property.PropertyType == typeof(decimal) ||
                        property.PropertyType == typeof(long) ||
                        property.PropertyType == typeof(ulong) ||
                        property.PropertyType == typeof(uint) ||
                        property.PropertyType == typeof(short) ||
                        property.PropertyType == typeof(ushort) ||
                        property.PropertyType == typeof(byte) ||
                        property.PropertyType == typeof(sbyte))
                {
                    configModel.Type = ConfigModelType.Number;
                }
                //如果是Boolean
                else if (property.PropertyType == typeof(bool))
                {
                    configModel.Type = ConfigModelType.Boolean;
                }
                //如果是Enum
                else if (property.PropertyType.IsEnum)
                {
                    configModel.Type = ConfigModelType.Number;
                    // 显示内容为枚举的名称，值为枚举的值
                    configModel.Options = property.PropertyType.GetEnumNames().Select(c => new KeyValuePair<string, string>(c, ((int)Enum.Parse(property.PropertyType, c)).ToString())).ToList();
                }
                //如果是Array 或者是List
                else if (property.PropertyType.IsArray || property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    configModel.Type = ConfigModelType.Array;
                    var subType = GetSubType(property.PropertyType);
                    configModel.SubType = subType switch
                    {
                        Type t when t == typeof(string) => ConfigModelType.String,
                        Type t when t == typeof(TimeSpan) => ConfigModelType.String,
                        Type t when t == typeof(int) || t == typeof(double) || t == typeof(float) || t == typeof(decimal) || t == typeof(long) || t == typeof(ulong) || t == typeof(uint) || t == typeof(short) || t == typeof(ushort) || t == typeof(byte) || t == typeof(sbyte) || t == typeof(char) => ConfigModelType.Number,
                        Type t when t == typeof(bool) => ConfigModelType.Boolean,
                        Type t when t == typeof(DateTime) => ConfigModelType.DateTime,
                        Type t when t == typeof(DateOnly) => ConfigModelType.DateOnly,
                        Type t when t == typeof(TimeOnly) => ConfigModelType.TimeOnly,
                        Type t when t.IsEnum => ConfigModelType.Number,
                        _ => ConfigModelType.Object
                    };
                    int GetDim(Type type)
                    {
                        if (type.IsArray)
                        {
                            return GetDim(type.GetElementType()!) + 1;
                        }
                        else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                        {
                            return GetDim(type.GetGenericArguments()[0]) + 1;
                        }
                        else
                        {
                            return 0;
                        }
                    }
                    Type GetSubType(Type type)
                    {
                        var elementType = type.GetElementType();
                        if (elementType is null)
                        {
                            elementType = type.GetGenericArguments()[0];
                        }
                        if (type.IsArray)
                        {
                            //如果多个数组嵌套
                            if (elementType.IsArray)
                            {
                                return GetSubType(elementType);
                            }
                            else
                            {
                                return elementType;
                            }
                        }
                        else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                        {
                            //如果多个List嵌套
                            if (type.GetGenericArguments()[0].IsGenericType && type.GetGenericArguments()[0].GetGenericTypeDefinition() == typeof(List<>))
                            {
                                return GetSubType(type.GetGenericArguments()[0]);
                            }
                            return type.GetGenericArguments()[0];
                        }
                        else
                        {
                            return type;
                        }
                    }

                    SetLength(subType);
                    configModel.Dim = GetDim(property.PropertyType);
                    configModel.SubTypeName = subType.Name;
                    GenerateConfigModel(subType, typeModels, false);
                }
                //如果是Object
                else if (property.PropertyType.IsClass)
                {
                    configModel.Type = ConfigModelType.Object;
                    configModel.SubTypeName = property.PropertyType.Name;
                    GenerateConfigModel(property.PropertyType, typeModels, false);
                }
                //如果是DateTime
                else if (property.PropertyType == typeof(DateTime))
                {
                    configModel.Type = ConfigModelType.DateTime;
                }
                //如果是TimeSpan
                else if (property.PropertyType == typeof(TimeSpan))
                {
                    configModel.Type = ConfigModelType.String;
                    //设置正则表达式,格式为10675199.02:48:05.4775807
                    configModel.RegularExpression = @"^(-)?((\d+\.(\d+))|(\d+)):(\d+):(\d+)(\.(\d+))?$";
                    configModel.RegularExpressionErrorMessage = "时间格式不正确(天.时:分:秒.毫秒)";
                }
                //如果是DateOnly
                else if (property.PropertyType == typeof(DateOnly))
                {
                    configModel.Type = ConfigModelType.DateOnly;
                }
                //如果是TimeOnly
                else if (property.PropertyType == typeof(TimeOnly))
                {
                    configModel.Type = ConfigModelType.TimeOnly;
                }
                else
                {
                    throw new NotImplementedException("不支持此类型 " + property.Name);
                }
                void SetLength(Type type)
                {
                    //根据类型设置最大值和最小值
                    if (type == typeof(char))
                    {
                        //设置长度
                        configModel.Minimum = 1;
                        configModel.Maximum = 1;
                    }
                    else if (type == typeof(int))
                    {
                        configModel.Minimum = int.MinValue;
                        configModel.Maximum = int.MaxValue;
                    }
                    else if (type == typeof(double))
                    {
                        configModel.Minimum = double.MinValue;
                        configModel.Maximum = double.MaxValue;
                    }
                    else if (type == typeof(float))
                    {
                        configModel.Minimum = float.MinValue;
                        configModel.Maximum = float.MaxValue;
                    }
                    else if (type == typeof(decimal))
                    {
                        configModel.Minimum = double.MinValue;
                        configModel.Maximum = double.MaxValue;
                    }
                    else if (type == typeof(long))
                    {
                        configModel.Minimum = long.MinValue;
                        configModel.Maximum = long.MaxValue;
                    }
                    else if (type == typeof(ulong))
                    {
                        configModel.Minimum = 0;
                        configModel.Maximum = ulong.MaxValue;
                    }
                    else if (type == typeof(uint))
                    {
                        configModel.Minimum = 0;
                        configModel.Maximum = uint.MaxValue;
                    }
                    else if (type == typeof(short))
                    {
                        configModel.Minimum = short.MinValue;
                        configModel.Maximum = short.MaxValue;
                    }
                    else if (type == typeof(ushort))
                    {
                        configModel.Minimum = 0;
                        configModel.Maximum = ushort.MaxValue;
                    }
                    else if (type == typeof(byte))
                    {
                        configModel.Minimum = byte.MinValue;
                        configModel.Maximum = byte.MaxValue;
                    }
                    else if (type == typeof(sbyte))
                    {
                        configModel.Minimum = sbyte.MinValue;
                        configModel.Maximum = sbyte.MaxValue;
                    }

                }

                //获取属性上的特性
                var attributes = property.GetCustomAttributes(true);
                foreach (var attribute in attributes)
                {
                    //如果是Display
                    if (attribute is DisplayAttribute displayAttribute)
                    {
                        configModel.DisplayName = displayAttribute.Name;
                        configModel.Description = displayAttribute.GetDescription();
                        configModel.GroupName = displayAttribute.GetGroupName();
                        configModel.Order = displayAttribute.GetOrder() ?? 0;
                        configModel.Prompt = displayAttribute.GetPrompt();
                    }
                    //如果是Range
                    if (attribute is RangeAttribute rangeAttribute)
                    {
                        configModel.Minimum = (int)rangeAttribute.Minimum;
                        configModel.Maximum = (int)rangeAttribute.Maximum;
                        configModel.RangeErrorMessage = rangeAttribute.ErrorMessage;
                    }
                    //如果是Required
                    if (attribute is RequiredAttribute requiredAttribute)
                    {
                        configModel.Required = true;
                        configModel.RequiredErrorMessage = requiredAttribute.ErrorMessage;
                    }
                    //如果是AllowedValues
                    if (attribute is AllowedValuesAttribute allowedValuesAttribute)
                    {
                        configModel.AllowedValues = allowedValuesAttribute.Values.Select(c => c?.ToString() ?? string.Empty).ToList();

                        configModel.AllowedValuesErrorMessage = allowedValuesAttribute.ErrorMessage;
                    }
                    //如果是DeniedValues
                    if (attribute is DeniedValuesAttribute deniedValuesAttribute)
                    {
                        configModel.DeniedValues = deniedValuesAttribute.Values.Select(c => c?.ToString() ?? string.Empty).ToList();
                        configModel.DeniedValuesErrorMessage = deniedValuesAttribute.ErrorMessage;
                    }
                    if (attribute is LengthAttribute lengthAttribute)
                    {
                        configModel.Minimum = lengthAttribute.MinimumLength;
                        configModel.Maximum = lengthAttribute.MaximumLength;
                        configModel.LengthErrorMessage = lengthAttribute.ErrorMessage;
                    }
                    //MaxLength
                    if (attribute is MaxLengthAttribute maxLengthAttribute)
                    {
                        configModel.Maximum = maxLengthAttribute.Length;
                        configModel.LengthErrorMessage = maxLengthAttribute.ErrorMessage;
                    }
                    //minLength
                    if (attribute is MinLengthAttribute minLengthAttribute)
                    {
                        configModel.Minimum = minLengthAttribute.Length;
                        configModel.LengthErrorMessage = minLengthAttribute.ErrorMessage;
                    }
                    //RegularExpression
                    if (attribute is RegularExpressionAttribute regularExpressionAttribute)
                    {
                        configModel.RegularExpression = regularExpressionAttribute.Pattern;
                        configModel.RegularExpressionErrorMessage = regularExpressionAttribute.ErrorMessage;
                    }
                    //Option
                    if (attribute is OptionAttribute optionAttribute)
                    {
                        configModel.Options ??= [];
                        configModel.Options.Add(new KeyValuePair<string, string>(optionAttribute.DisplayName, optionAttribute.Value));
                    }
                    if (attribute is OptionProviderAttribute optionProviderAttribute)
                    {
                        configModel.OptionProvider = optionProviderAttribute.Name;
                    }

                    if (attribute is FilePathAttribute filePathAttribute)
                    {
                        if (configModel.Type == ConfigModelType.String)
                        {
                            configModel.Type = ConfigModelType.FilePath;
                        }
                        if (configModel.SubType == ConfigModelType.String)
                        {
                            configModel.SubType = ConfigModelType.FilePath;
                        }
                    }
                    if (attribute is FolderPathAttribute folderPathAttribute)
                    {
                        if (configModel.Type == ConfigModelType.String)
                        {
                            configModel.Type = ConfigModelType.FolderPath;
                        }
                        if (configModel.SubType == ConfigModelType.String)
                        {
                            configModel.SubType = ConfigModelType.FolderPath;
                        }
                    }
                }

                typeModel.PropertyModels.Add(configModel);
            }
        }
    }
}
