namespace Ty.Services.Configs;

public class PropertyModel(string name)
{
    public string Name { get; set; } = name;

    public int? Order { get; set; }
    public ConfigModelType Type { get; set; }
    public ConfigModelType? SubType { get; set; }
    /// <summary>
    /// 如果是数组类型，那么这个值表示数组的维度
    /// </summary>
    public int? Dim { get; set; }
    public string? SubTypeName { get; set; }


    public string? DisplayName { get; set; }
    public string? GroupName { get; set; }
    public string? Description { get; set; }
    public string? Prompt { get; set; }
    public double? Minimum { get; set; }
    public double? Maximum { get; set; }
    public List<string>? AllowedValues { get; set; }
    public List<string>? DeniedValues { get; set; }
    public bool? Required { get; set; }
    public string? RegularExpression { get; set; }
    public string? LengthErrorMessage { get; set; }
    public string? AllowedValuesErrorMessage { get; set; }
    public string? DeniedValuesErrorMessage { get; set; }
    public string? RegularExpressionErrorMessage { get; set; }
    public string? RequiredErrorMessage { get; set; }
    public string? RangeErrorMessage { get; set; }

    public string? OptionProvider { get; set; }
    public List<KeyValuePair<string, string>>? Options { get; set; }

    public List<string> SubTypeNames { get; set; } = [];
}

public enum ConfigModelType
{
    String,
    Number,
    Boolean,
    DateTime,
    DateOnly,
    TimeOnly,
    Object,
    Array,

    FilePath,
    FolderPath,
}
