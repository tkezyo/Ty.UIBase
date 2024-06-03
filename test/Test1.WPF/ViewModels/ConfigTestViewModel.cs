using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using Ty.Services;
using Ty.Services.Configs;
using Ty.ViewModels;
using Ty.ViewModels.Configs;

namespace Test1.WPF.ViewModels;

public class ConfigTestViewModel : ViewModelBase
{
    private readonly ConfigManager _configManager;
    private readonly IMessageBoxManager _messageBoxManager;

    public ConfigEditViewModel Config { get; set; }
    public ConfigTestViewModel(ConfigManager configManager, ConfigEditViewModel configEditViewModel, IMessageBoxManager messageBoxManager)
    {
        Config = configEditViewModel;
        this._configManager = configManager;
        this._messageBoxManager = messageBoxManager;
        ShowResultCommand = ReactiveCommand.CreateFromTask(ShowResult);
    }

    public override Task Activate()
    {
        var models = ConfigManager.GetConfigModel<DemoConfig>();
        Config.LoadConfig(models, null);
        return base.Activate();
    }

    public ReactiveCommand<Unit, Unit> ShowResultCommand { get; set; }
    public async Task ShowResult()
    {
        var json = Config.GetResult();
        await _messageBoxManager.Alert.Handle(new AlertInfo(json?.ToString() ?? string.Empty));
    }
}
