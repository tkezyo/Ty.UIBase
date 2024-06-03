using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ReactiveUI.Validation.Extensions;
using ReactiveUI.Validation.Helpers;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using TextCopy;
using Ty.Services.Configs;

namespace Ty.ViewModels.Configs;

public class ConfigEditViewModel : ReactiveValidationObject
{
    private readonly ConfigManager _configManager;

    public ConfigEditViewModel(ConfigManager configManager)
    {
        AddPropertyCommand = ReactiveCommand.Create<ConfigViewModel>(AddProperty);
        AddArrayCommand = ReactiveCommand.Create<ConfigViewModel>(AddArray);
        SetObjectCommand = ReactiveCommand.Create<ConfigViewModel>(SetObject);
        CopyCommand = ReactiveCommand.Create<ConfigViewModel>(Copy);
        PasteCommand = ReactiveCommand.Create<ConfigViewModel>(Paste);

        this.Configs.ToObservableChangeSet().ActOnEveryObject(c =>
        {
            this.ValidationContext.Add(c.ValidationContext);
        }, c =>
        {
            this.ValidationContext.Remove(c.ValidationContext);
        });
        this._configManager = configManager;
    }


    public ObservableCollection<ConfigViewModel> Configs { get; set; } = [];

    public void LoadConfig(List<PropertyModel> configModels, JsonObject? config)
    {
        Configs.Clear();
        foreach (var property in configModels)
        {
            SetConfigViewModel(property, config, Configs);
        }
    }
    public JsonObject? GetResult()
    {
        var result = new JsonObject();

        foreach (var item2 in Configs)
        {
            result[item2.Name] = SetJsonNode(item2);
        }
        return result;
    }

    public List<NameValue> GetNameValues()
    {
        List<NameValue> nameValues = [];
        foreach (var item2 in Configs)
        {
            nameValues.Add(new NameValue(item2.Name, SetJsonNode(item2).ToString()));
        }
        return nameValues;
    }

    /// <summary>
    /// 转换为JsonNode
    /// </summary>
    /// <param name="config"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    static JsonNode SetJsonNode(ConfigViewModel config)
    {
        if (config.Required)
        {
            if (config.Value is null)
            {
                throw new Exception($"{config.Name}是必填项");
            }
        }
        if (config.AllowedValues is not null && !config.AllowedValues.Contains(config.Value ?? string.Empty))
        {
            throw new Exception($"{config.Name}的值不在允许的范围内");
        }
        if (config.DeniedValues is not null && config.DeniedValues.Contains(config.Value ?? string.Empty))
        {
            throw new Exception($"{config.Name}的值在禁止的范围内");
        }

        //验证数据是否合法
        if (config.Type == ConfigModelType.Number)
        {
            double v = 0;
            if (config.Value is not null && !double.TryParse(config.Value, out v))
            {
                throw new Exception($"{config.Name}的值不是数字");
            }
            if (!double.IsNaN(config.Maximum))
            {
                if (v > config.Maximum)
                {
                    throw new Exception($"{config.Name}的值大于最大值");
                }
            }
            if (!double.IsNaN(config.Minimum))
            {
                if (v < config.Minimum)
                {
                    throw new Exception($"{config.Name}的值小于最小值");
                }
            }
        }
        else if (config.Type == ConfigModelType.Boolean)
        {
            if (config.Value is not null && !bool.TryParse(config.Value, out _))
            {
                throw new Exception($"{config.Name}的值不是布尔值");
            }
        }
        else if (config.Type == ConfigModelType.DateTime)
        {
            if (config.Value is not null && !DateTime.TryParse(config.Value, out _))
            {
                throw new Exception($"{config.Name}的值不是日期时间");
            }
        }
        else if (config.Type == ConfigModelType.DateOnly)
        {
            if (config.Value is not null && !DateOnly.TryParse(config.Value.Split(' ')[0], out _))
            {
                throw new Exception($"{config.Name}的值不是日期");
            }
        }
        else if (config.Type == ConfigModelType.TimeOnly)
        {
            if (config.Value is null)
            {
                return (JsonNode)string.Empty;
            }
            var timeStr = config.Value.Contains(' ') ? config.Value.Split(' ')[1] : config.Value;
            if (!TimeOnly.TryParse(timeStr, out var time))
            {
                throw new Exception($"{config.Name}的值不是时间");
            }
        }
        else if (config.Type == ConfigModelType.String)
        {
            if (config.Value is not null && config.RegularExpression is not null && !Regex.IsMatch(config.Value, config.RegularExpression))
            {
                throw new Exception($"{config.Name}的值不符合正则表达式");
            }
            if (config.Value?.Length > config.Maximum)
            {
                throw new Exception($"{config.Name}的值超长");
            }
            if (config.Value?.Length < config.Minimum)
            {
                throw new Exception($"{config.Name}的值过短");
            }
        }

        if (config.Type == ConfigModelType.Object)
        {
            if (string.IsNullOrEmpty(config.Value))
            {
                return JsonNode.Parse("null")!;
            }
            JsonObject jsonObj = new JsonObject();
            foreach (var item in config.Properties)
            {
                jsonObj[item.Name] = SetJsonNode(item);
            }
            return jsonObj;
        }
        else if (config.Type == ConfigModelType.Array)
        {
            var array = new JsonArray();
            //如果是数组，需要确认维度
            if (config.Dim > 0)
            {
                for (var i = 0; i < config.Properties.Count; i++)
                {
                    var item = config.Properties[i];

                    array.Add(SetJsonNode(item));
                }
            }
            return array;
        }
        //如果是布尔值，需要转换为布尔值
        else if (config.Type == ConfigModelType.Boolean)
        {
            return (JsonNode)bool.Parse(config.Value ?? "false");
        }
        //如果是数字，需要转换为数字
        else if (config.Type == ConfigModelType.Number)
        {
            return (JsonNode)decimal.Parse(config.Value ?? "0");
        }
        else if (config.Type == ConfigModelType.DateTime)
        {
            //config.value的格式为 4/2/2024 10:19:02 PM 需要转换为 json的格式
            return (JsonNode)DateTime.Parse(config.Value ?? string.Empty).ToString("yyyy-MM-ddTHH:mm:ss");
        }
        else if (config.Type == ConfigModelType.DateOnly && !string.IsNullOrEmpty(config.Value))
        {
            //config.value的格式为 4/2/2024 10:19:02 PM 需要转换为 json的格式
            return (JsonNode)DateTime.Parse(config.Value.Split(' ')[0] ?? string.Empty).ToString("yyyy-MM-dd");
        }
        else if (config.Type == ConfigModelType.TimeOnly && !string.IsNullOrEmpty(config.Value))
        {
            //config.value的格式为 4/2/2024 10:19:02 PM 需要转换为 json的格式
            return (JsonNode)DateTime.Parse(config.Value).ToString("HH:mm:ss");
        }
        else
        {
            return (JsonNode)(config.Value ?? string.Empty);
        }
    }

    /// <summary>
    /// Json转ConfigViewModel
    /// </summary>
    /// <param name="propertyModel"></param>
    /// <param name="config"></param>
    /// <param name="configViewModel"></param>
    void SetConfigViewModel(PropertyModel propertyModel, JsonObject? config, ObservableCollection<ConfigViewModel> configViewModel)
    {
        var configViewModelProperty = new ConfigViewModel(propertyModel.Name)
        {
            Type = propertyModel.Type,
            SubTypeName = propertyModel.SubTypeName,
            SubType = propertyModel.SubType,
            DisplayName = propertyModel.DisplayName ?? propertyModel.Name,
            GroupName = propertyModel.GroupName,
            Description = propertyModel.Description,
            Prompt = propertyModel.Prompt,
            Minimum = propertyModel.Minimum ?? double.NaN,
            Maximum = propertyModel.Maximum ?? double.NaN,
            AllowedValues = propertyModel.AllowedValues is not null ? new ObservableCollection<string>(propertyModel.AllowedValues) : null,
            DeniedValues = propertyModel.DeniedValues is not null ? new ObservableCollection<string>(propertyModel.DeniedValues) : null,
            Options = new ObservableCollection<KeyValuePair<string, string>>(propertyModel.Options ?? []),
            Required = propertyModel.Required ?? false,
            RegularExpression = propertyModel.RegularExpression,
            Dim = propertyModel.Dim,
            Order = propertyModel.Order,
            AllowedValuesErrorMessage = propertyModel.AllowedValuesErrorMessage,
            DeniedValuesErrorMessage = propertyModel.DeniedValuesErrorMessage,
            RegularExpressionErrorMessage = propertyModel.RegularExpressionErrorMessage,
            RequiredErrorMessage = propertyModel.RequiredErrorMessage,
            RangeErrorMessage = propertyModel.RangeErrorMessage,
            LengthErrorMessage = propertyModel.LengthErrorMessage,
        };

        configViewModelProperty.SetValidationRule();

        if (config is not null)
        {
            if (propertyModel.Type == ConfigModelType.Object && config[propertyModel.Name] is JsonObject jsonObj)
            {
                var properties = ConfigManager.GetConfigModel(propertyModel.SubTypeName);
                if (properties is not null)
                {
                    configViewModelProperty.Value = "create";
                    foreach (var item in properties)
                    {
                        SetConfigViewModel(item, jsonObj, configViewModelProperty.Properties);
                    }
                }
            }
            //如果是数组
            else if (propertyModel.Type == ConfigModelType.Array && config[propertyModel.Name] is JsonArray array)
            {
                ConfigViewModel? Create(JsonNode jsonNode, int dim)
                {
                    if (dim >= 1)
                    {
                        if (jsonNode is JsonArray jsonArray)
                        {
                            var subConfigViewModel = new ConfigViewModel(propertyModel.SubTypeName ?? "")
                            {
                                Type = propertyModel.Type,
                                SubType = propertyModel.SubType,
                                SubTypeName = propertyModel.SubTypeName,
                                Dim = dim,
                                DisplayName = propertyModel.DisplayName ?? propertyModel.Name,
                                GroupName = propertyModel.GroupName,
                                Description = propertyModel.Description,
                                Prompt = propertyModel.Prompt,
                                Minimum = propertyModel.Minimum ?? double.NaN,
                                Maximum = propertyModel.Maximum ?? double.NaN,
                                AllowedValues = propertyModel.AllowedValues is not null ? new ObservableCollection<string>(propertyModel.AllowedValues) : null,
                                DeniedValues = propertyModel.DeniedValues is not null ? new ObservableCollection<string>(propertyModel.DeniedValues) : null,
                                Options = new ObservableCollection<KeyValuePair<string, string>>(propertyModel.Options ?? []),
                                Required = propertyModel.Required ?? false,
                                RegularExpression = propertyModel.RegularExpression,
                                AllowedValuesErrorMessage = propertyModel.AllowedValuesErrorMessage,
                                DeniedValuesErrorMessage = propertyModel.DeniedValuesErrorMessage,
                                RegularExpressionErrorMessage = propertyModel.RegularExpressionErrorMessage,
                                RequiredErrorMessage = propertyModel.RequiredErrorMessage,
                                RangeErrorMessage = propertyModel.RangeErrorMessage,
                                LengthErrorMessage = propertyModel.LengthErrorMessage,
                            };
                            subConfigViewModel.SetValidationRule();

                            foreach (var item in jsonArray)
                            {
                                if (item is null)
                                {
                                    continue;
                                }
                                var result = Create(item, dim - 1);
                                if (result is not null)
                                {
                                    subConfigViewModel.Properties.Add(result);
                                }
                            }
                            return subConfigViewModel;
                        }
                    }
                    else
                    {
                        var subConfigViewModel = new ConfigViewModel(propertyModel.SubTypeName ?? "")
                        {
                            Type = propertyModel.SubType ?? ConfigModelType.Object,
                            DisplayName = propertyModel.DisplayName ?? propertyModel.Name,
                            GroupName = propertyModel.GroupName,
                            Description = propertyModel.Description,
                            Prompt = propertyModel.Prompt,
                            Minimum = propertyModel.Minimum ?? double.NaN,
                            Maximum = propertyModel.Maximum ?? double.NaN,
                            AllowedValues = propertyModel.AllowedValues is not null ? new ObservableCollection<string>(propertyModel.AllowedValues) : null,
                            DeniedValues = propertyModel.DeniedValues is not null ? new ObservableCollection<string>(propertyModel.DeniedValues) : null,
                            Options = new ObservableCollection<KeyValuePair<string, string>>(propertyModel.Options ?? []),
                            Required = propertyModel.Required ?? false,
                            RegularExpression = propertyModel.RegularExpression,
                            AllowedValuesErrorMessage = propertyModel.AllowedValuesErrorMessage,
                            DeniedValuesErrorMessage = propertyModel.DeniedValuesErrorMessage,
                            RegularExpressionErrorMessage = propertyModel.RegularExpressionErrorMessage,
                            RequiredErrorMessage = propertyModel.RequiredErrorMessage,
                            RangeErrorMessage = propertyModel.RangeErrorMessage,
                            LengthErrorMessage = propertyModel.LengthErrorMessage,

                        };
                        subConfigViewModel.SetValidationRule();

                        if (propertyModel.SubType == ConfigModelType.Object)
                        {
                            var properties = ConfigManager.GetConfigModel(propertyModel.SubTypeName);
                            if (properties is not null)
                            {
                                subConfigViewModel.Value = "create";
                                foreach (var item in properties)
                                {
                                    SetConfigViewModel(item, jsonNode as JsonObject, subConfigViewModel.Properties);
                                }
                            }
                        }
                        else
                        {
                            subConfigViewModel.Value = jsonNode.ToJsonString();
                        }

                        return subConfigViewModel;
                    }
                    return null;
                }

                foreach (var item in array)
                {
                    if (item is null)
                    {
                        continue;
                    }
                    var result = Create(item, (propertyModel.Dim ?? 0) - 1);
                    if (result is not null)
                    {
                        configViewModelProperty.Properties.Add(result);
                    }
                }
            }
            else if (config[propertyModel.Name] is not null)
            {
                if (propertyModel.Type == ConfigModelType.String)
                {
                    if (config[propertyModel.Name]?.ToString() != "\u0000")
                    {
                        configViewModelProperty.Value = config[propertyModel.Name]?.ToString();
                    }
                    else
                    {
                        configViewModelProperty.Value = "0";
                    }
                }
                else
                {
                    configViewModelProperty.Value = config[propertyModel.Name]?.ToString();
                }
            }
        }
        configViewModel.Add(configViewModelProperty);
    }

    public ReactiveCommand<ConfigViewModel, Unit> AddArrayCommand { get; }
    public void AddArray(ConfigViewModel configViewModel)
    {
        //如果是数组，需要确认维度
        if (configViewModel.Type != ConfigModelType.Array || !configViewModel.SubType.HasValue)
        {
            return;
        }

        var subDim = configViewModel.Dim - 1;

        if (subDim >= 1)//子类型还是数组
        {
            ConfigViewModel configViewModel1 = new(configViewModel.Name)
            {
                Type = configViewModel.Type,
                AllowedValues = configViewModel.AllowedValues,
                DeniedValues = configViewModel.DeniedValues,
                Options = configViewModel.Options,
                Required = configViewModel.Required,
                RegularExpression = configViewModel.RegularExpression,
                Order = configViewModel.Order,
                GroupName = configViewModel.GroupName,
                Description = configViewModel.Description,
                Prompt = configViewModel.Prompt,
                Minimum = configViewModel.Minimum,
                Maximum = configViewModel.Maximum,
                DisplayName = configViewModel.DisplayName ?? configViewModel.Name,
                Dim = subDim,
                SubType = configViewModel.SubType,
                SubTypeName = configViewModel.SubTypeName,
                AllowedValuesErrorMessage = configViewModel.AllowedValuesErrorMessage,
                DeniedValuesErrorMessage = configViewModel.DeniedValuesErrorMessage,
                RegularExpressionErrorMessage = configViewModel.RegularExpressionErrorMessage,
                RequiredErrorMessage = configViewModel.RequiredErrorMessage,
                RangeErrorMessage = configViewModel.RangeErrorMessage,
                LengthErrorMessage = configViewModel.LengthErrorMessage,

            };
            configViewModel1.SetValidationRule();

            configViewModel.Properties.Add(configViewModel1);
        }
        else
        {
            ConfigViewModel configViewModel1 = new(configViewModel.Name)
            {
                Type = configViewModel.SubType.Value,
                AllowedValues = configViewModel.AllowedValues,
                DeniedValues = configViewModel.DeniedValues,
                Options = configViewModel.Options,
                Required = configViewModel.Required,
                RegularExpression = configViewModel.RegularExpression,
                Order = configViewModel.Order,
                GroupName = configViewModel.GroupName,
                Description = configViewModel.Description,
                Prompt = configViewModel.Prompt,
                Minimum = configViewModel.Minimum,
                Maximum = configViewModel.Maximum,
                DisplayName = configViewModel.DisplayName,
                AllowedValuesErrorMessage = configViewModel.AllowedValuesErrorMessage,
                DeniedValuesErrorMessage = configViewModel.DeniedValuesErrorMessage,
                RegularExpressionErrorMessage = configViewModel.RegularExpressionErrorMessage,
                RequiredErrorMessage = configViewModel.RequiredErrorMessage,
                RangeErrorMessage = configViewModel.RangeErrorMessage,
                LengthErrorMessage = configViewModel.LengthErrorMessage,
            };
            configViewModel1.SetValidationRule();

            if (configViewModel.SubType == ConfigModelType.Object)
            {
                var properties = ConfigManager.GetConfigModel(configViewModel.SubTypeName);
                if (properties is null)
                {
                    return;
                }
                configViewModel1.Value = "create";
                foreach (var item in properties)
                {
                    SetConfigViewModel(item, [], configViewModel1.Properties);
                }
            }
            configViewModel.Properties.Add(configViewModel1);
        }


    }

    public ReactiveCommand<ConfigViewModel, Unit> SetObjectCommand { get; }
    public void SetObject(ConfigViewModel configViewModel)
    {
        var properties = ConfigManager.GetConfigModel(configViewModel.SubTypeName);
        if (properties is null)
        {
            return;
        }
        configViewModel.Value = "create";
        foreach (var item in properties)
        {
            SetConfigViewModel(item, [], configViewModel.Properties);
        }
    }

    public ReactiveCommand<ConfigViewModel, Unit> AddPropertyCommand { get; }
    public void AddProperty(ConfigViewModel configViewModel)
    {
        if (configViewModel.SubType.HasValue)
        {
            ConfigViewModel configViewModel1 = new(string.Empty)
            {
                Type = configViewModel.SubType.Value
            };
            configViewModel1.SetValidationRule();
            if (configViewModel.SubType == ConfigModelType.Object)
            {
                var properties = ConfigManager.GetConfigModel(configViewModel.SubTypeName);
                if (properties is null)
                {
                    return;
                }
                configViewModel.Value = "create";
                foreach (var item in properties)
                {
                    SetConfigViewModel(item, [], configViewModel.Properties);
                }
            }
            configViewModel.Properties.Add(configViewModel1);
        }

    }

    //CopyCommand
    public ReactiveCommand<ConfigViewModel, Unit> CopyCommand { get; }
    public void Copy(ConfigViewModel configViewModel)
    {
        //将 configViewModel转换为 json，并复制到剪贴板
        var json = SetJsonNode(configViewModel).ToJsonString();
        Clipboard clipboard = new();

        clipboard.SetText(configViewModel.ToString() + "|" + json);
    }

    //PasteCommand
    public ReactiveCommand<ConfigViewModel, Unit> PasteCommand { get; }
    public void Paste(ConfigViewModel configViewModel)
    {
        //从剪贴板读取 json，并转换为 configViewModel
        Clipboard clipboard = new();
        var json = clipboard.GetText();
        if (string.IsNullOrEmpty(json))
        {
            return;
        }

        var index = json.IndexOf('|');
        if (index == -1)
        {
            return;
        }

        if (!json.StartsWith(configViewModel.ToString() + "|"))
        {
            return;
        }

        json = json[(index + 1)..];



        if (configViewModel.Type == ConfigModelType.Object)
        {
            //如果是对象，需要转换为JsonObject,然后递归设置
            var jsonObj = JsonSerializer.Deserialize<JsonObject>(json);

            var properties = ConfigManager.GetConfigModel(configViewModel.SubTypeName);
            if (properties is null)
            {
                return;
            }
            configViewModel.Value = "create";
            foreach (var item in properties)
            {
                SetConfigViewModel(item, jsonObj, configViewModel.Properties);
            }
        }
        else if (configViewModel.Type == ConfigModelType.Array)
        {
            //如果是数组，需要转换为JsonArray,然后递归设置
            var array = JsonSerializer.Deserialize<JsonArray>(json);
            ConfigViewModel? Create(JsonNode jsonNode, int dim)
            {
                if (dim >= 1)
                {
                    if (jsonNode is JsonArray jsonArray)
                    {
                        var subConfigViewModel = new ConfigViewModel(configViewModel.SubTypeName ?? "")
                        {
                            Type = configViewModel.Type,
                            SubType = configViewModel.SubType,
                            SubTypeName = configViewModel.SubTypeName,
                            Dim = dim,
                            DisplayName = configViewModel.DisplayName ?? configViewModel.Name,
                            GroupName = configViewModel.GroupName,
                            Description = configViewModel.Description,
                            Prompt = configViewModel.Prompt,
                            Minimum = configViewModel.Minimum,
                            Maximum = configViewModel.Maximum,
                            AllowedValues = configViewModel.AllowedValues is not null ? new ObservableCollection<string>(configViewModel.AllowedValues) : null,
                            DeniedValues = configViewModel.DeniedValues is not null ? new ObservableCollection<string>(configViewModel.DeniedValues) : null,
                            Options = new ObservableCollection<KeyValuePair<string, string>>(configViewModel.Options ?? []),
                            Required = configViewModel.Required,
                            RegularExpression = configViewModel.RegularExpression,
                            AllowedValuesErrorMessage = configViewModel.AllowedValuesErrorMessage,
                            DeniedValuesErrorMessage = configViewModel.DeniedValuesErrorMessage,
                            RegularExpressionErrorMessage = configViewModel.RegularExpressionErrorMessage,
                            RequiredErrorMessage = configViewModel.RequiredErrorMessage,
                            RangeErrorMessage = configViewModel.RangeErrorMessage,
                            LengthErrorMessage = configViewModel.LengthErrorMessage,
                        };
                        subConfigViewModel.SetValidationRule();

                        foreach (var item in jsonArray)
                        {
                            if (item is null)
                            {
                                continue;
                            }
                            var result = Create(item, dim - 1);
                            if (result is not null)
                            {
                                subConfigViewModel.Properties.Add(result);
                            }
                        }
                        return subConfigViewModel;
                    }
                }
                else
                {
                    var subConfigViewModel = new ConfigViewModel(configViewModel.SubTypeName ?? "")
                    {
                        Type = configViewModel.SubType ?? ConfigModelType.Object,
                        DisplayName = configViewModel.DisplayName ?? configViewModel.Name,
                        GroupName = configViewModel.GroupName,
                        Description = configViewModel.Description,
                        Prompt = configViewModel.Prompt,
                        Minimum = configViewModel.Minimum,
                        Maximum = configViewModel.Maximum,
                        AllowedValues = configViewModel.AllowedValues is not null ? new ObservableCollection<string>(configViewModel.AllowedValues) : null,
                        DeniedValues = configViewModel.DeniedValues is not null ? new ObservableCollection<string>(configViewModel.DeniedValues) : null,
                        Options = new ObservableCollection<KeyValuePair<string, string>>(configViewModel.Options ?? []),
                        Required = configViewModel.Required,
                        RegularExpression = configViewModel.RegularExpression,
                        AllowedValuesErrorMessage = configViewModel.AllowedValuesErrorMessage,
                        DeniedValuesErrorMessage = configViewModel.DeniedValuesErrorMessage,
                        RegularExpressionErrorMessage = configViewModel.RegularExpressionErrorMessage,
                        RequiredErrorMessage = configViewModel.RequiredErrorMessage,
                        RangeErrorMessage = configViewModel.RangeErrorMessage,
                        LengthErrorMessage = configViewModel.LengthErrorMessage,

                    };
                    subConfigViewModel.SetValidationRule();

                    if (configViewModel.SubType == ConfigModelType.Object)
                    {
                        var properties = ConfigManager.GetConfigModel(configViewModel.SubTypeName);
                        if (properties is not null)
                        {
                            subConfigViewModel.Value = "create";
                            foreach (var item1 in properties)
                            {
                                SetConfigViewModel(item1, jsonNode as JsonObject, subConfigViewModel.Properties);
                            }
                        }

                    }
                    else
                    {
                        subConfigViewModel.Value = jsonNode.ToJsonString();
                    }

                    return subConfigViewModel;
                }
                return null;
            }

            if (array is null)
            {
                return;
            }

            configViewModel.Properties.Clear();


            foreach (var item in array)
            {
                if (item is null)
                {
                    continue;
                }
                var result = Create(item, (configViewModel.Dim ?? 0) - 1);
                if (result is not null)
                {
                    configViewModel.Properties.Add(result);
                }
            }

        }
        else if (configViewModel.Type == ConfigModelType.String)
        {
            configViewModel.Value = json.Trim('"');
        }
        else
        {
            configViewModel.Value = json;
        }
    }
}

public class ConfigInfoViewModel : ReactiveValidationObject
{
    public ConfigInfoViewModel()
    {
        this.Configs.ToObservableChangeSet().ActOnEveryObject(c =>
        {
            this.ValidationContext.Add(c.ValidationContext);
        }, c =>
        {
            this.ValidationContext.Remove(c.ValidationContext);
        });
    }
    [Reactive]
    public required string Name { get; set; }
    [Reactive]
    public ObservableCollection<ConfigViewModel> Configs { get; set; } = [];
}

public class ConfigViewModel : ReactiveValidationObject
{
    public ConfigViewModel(string name)
    {
        this.Name = name;

        this.Properties.ToObservableChangeSet().ActOnEveryObject(c =>
        {
            this.ValidationContext.Add(c.ValidationContext);
        }, c =>
        {
            this.ValidationContext.Remove(c.ValidationContext);
        });
    }
    public void SetValidationRule()
    {
        if (!string.IsNullOrEmpty(RequiredErrorMessage))
        {
            RequiredErrorMessage = string.Format(RequiredErrorMessage, Name);
        }

        if (!string.IsNullOrEmpty(AllowedValuesErrorMessage))
        {
            AllowedValuesErrorMessage = string.Format(AllowedValuesErrorMessage, Name);
        }

        if (!string.IsNullOrEmpty(DeniedValuesErrorMessage))
        {
            DeniedValuesErrorMessage = string.Format(DeniedValuesErrorMessage, Name);
        }

        if (!string.IsNullOrEmpty(RegularExpressionErrorMessage))
        {
            RegularExpressionErrorMessage = string.Format(RegularExpressionErrorMessage, Name);
        }

        if (!string.IsNullOrEmpty(RangeErrorMessage))
        {
            RangeErrorMessage = string.Format(RangeErrorMessage, Name);
        }

        if (!string.IsNullOrEmpty(LengthErrorMessage))
        {
            LengthErrorMessage = string.Format(LengthErrorMessage, Name);
        }




        this.ValidationRule(
        viewModel => viewModel.Value,
        name =>
        {
            if (Required && string.IsNullOrWhiteSpace(name))
            {
                return false;
            }
            return true;
        }, RequiredErrorMessage ?? "必填");

        this.ValidationRule(
            viewModel => viewModel.Value,
            name =>
            {
                if (AllowedValues is not null && !AllowedValues.Contains(name ?? string.Empty))
                {
                    return false;
                }
                return true;
            }, AllowedValuesErrorMessage ?? "不在允许的范围内");

        this.ValidationRule(
            viewModel => viewModel.Value,
            name =>
            {
                if (DeniedValues is not null && DeniedValues.Contains(name ?? string.Empty))
                {
                    return false;
                }
                return true;
            }, DeniedValuesErrorMessage ?? "在禁止的范围内");

        this.ValidationRule(
            viewModel => viewModel.Value,
            name =>
            {
                if (Type == ConfigModelType.Number)
                {
                    double v = 0;
                    if (name is not null && !double.TryParse(name, out v))
                    {
                        return false;
                    }
                    if (!double.IsNaN(Maximum))
                    {
                        if (v > Maximum)
                        {
                            return false;
                        }
                    }
                    if (!double.IsNaN(Minimum))
                    {
                        if (v < Minimum)
                        {
                            return false;
                        }
                    }
                }
                return true;
            }, RangeErrorMessage ?? "范围错误");

        this.ValidationRule(
            viewModel => viewModel.Value,
            name =>
            {
                if (Type == ConfigModelType.Boolean)
                {
                    if (name is not null && !bool.TryParse(name, out _))
                    {
                        return false;
                    }
                }
                return true;
            }, "不是布尔值");

        this.ValidationRule(
            viewModel => viewModel.Value,
            name =>
            {
                if (Type == ConfigModelType.DateTime)
                {
                    if (name is not null && !DateTime.TryParse(name, out _))
                    {
                        return false;
                    }
                }
                return true;
            }, "不是日期时间");

        this.ValidationRule(
            viewModel => viewModel.Value,
            name =>
            {
                if (Type == ConfigModelType.DateOnly)
                {
                    if (name is not null && !DateTime.TryParse(name, out _))
                    {
                        return false;
                    }
                }
                return true;
            }, "不是日期");

        this.ValidationRule(
            viewModel => viewModel.Value,
            name =>
            {
                if (Type == ConfigModelType.TimeOnly)
                {
                    if (name is not null && !DateTime.TryParse(name, out _))
                    {
                        return false;
                    }
                }
                return true;
            }, "不是时间");

        this.ValidationRule(
            viewModel => viewModel.Value,
            name =>
            {
                if (Type == ConfigModelType.String)
                {
                    if (name is not null && RegularExpression is not null && !Regex.IsMatch(name, RegularExpression))
                    {
                        return false;
                    }
                }
                return true;
            }, RegularExpressionErrorMessage ?? "不符合正则表达式");

        this.ValidationRule(
              viewModel => viewModel.Value,
              name =>
              {
                  if (Type == ConfigModelType.String)
                  {
                      if (name?.Length > Maximum)
                      {
                          return false;
                      }
                      if (name?.Length < Minimum)
                      {
                          return false;
                      }
                  }
                  return true;
              }, LengthErrorMessage ?? "长度错误");
    }

    public override string? ToString()
    {
        return $"{Type}:{SubType}:{SubTypeName}:{Dim}";
    }

    [Reactive]
    public string Name { get; set; }
    [Reactive]
    public ConfigModelType Type { get; set; }
    [Reactive]
    public int? Order { get; set; }
    [Reactive]
    public ConfigModelType? SubType { get; set; }
    [Reactive]
    public string? SubTypeName { get; set; }
    [Reactive]
    public string? DisplayName { get; set; }
    [Reactive]
    public string? GroupName { get; set; }
    [Reactive]
    public string? Description { get; set; }
    [Reactive]
    public string? Prompt { get; set; }
    [Reactive]
    public double Minimum { get; set; }
    [Reactive]
    public double Maximum { get; set; }
    [Reactive]
    public ObservableCollection<string>? AllowedValues { get; set; }
    [Reactive]
    public ObservableCollection<string>? DeniedValues { get; set; }
    [Reactive]
    public ObservableCollection<KeyValuePair<string, string>>? Options { get; set; }
    [Reactive]
    public bool Required { get; set; }
    [Reactive]
    public string? RegularExpression { get; set; }
    [Reactive]
    public string? Value { get; set; }

    [Reactive]
    public int? Dim { get; set; }

    public string? LengthErrorMessage { get; set; }
    public string? AllowedValuesErrorMessage { get; set; }
    public string? DeniedValuesErrorMessage { get; set; }
    public string? RegularExpressionErrorMessage { get; set; }
    public string? RequiredErrorMessage { get; set; }
    public string? RangeErrorMessage { get; set; }
    [Reactive]
    public ObservableCollection<ConfigViewModel> Properties { get; set; } = [];
}

public class DimViewModel : ReactiveObject
{
    public DimViewModel(int length, string displayName)
    {
        if (length < 1)
        {
            length = 1;
            ReadOnly = false;
        }
        else
        {
            ReadOnly = true;
        }
        Length = length;
        DisplayName = displayName;
        if (!ReadOnly)
        {
            DisplayName += "*";
        }
    }
    [Reactive]
    public int Length { get; set; }

    [Reactive]
    public bool ReadOnly { get; set; }

    [Reactive]
    public string DisplayName { get; set; }
}
