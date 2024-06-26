﻿using DynamicData.Kernel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using Ty.Services;
using Ty.ViewModels.Configs;

namespace Ty.ViewModels.CustomPages;

public class CustomPageViewModel : ViewModelBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMessageBoxManager _messageBoxManager;
    private readonly MenuService _menuService;
    private readonly PermissionService _permissionService;
    private readonly CustomPageOption _customPageOption;

    public CustomPageViewModel(IServiceProvider serviceProvider, IMessageBoxManager messageBoxManager, IOptions<CustomPageOption> options, MenuService menuService, PermissionService permissionService)
    {
        _serviceProvider = serviceProvider;
        _messageBoxManager = messageBoxManager;
        this._menuService = menuService;
        this._permissionService = permissionService;
        _customPageOption = options.Value;

        DeleteBoxCommand = ReactiveCommand.Create(DeleteBox);
        ChangeBoxShowViewCommand = ReactiveCommand.Create(ChangeBoxShowView);
        ChangeBoxCustomViewGroupCommand = ReactiveCommand.Create<string>(ChangeBoxCustomViewGroup);
        ChangeBoxCustomViewNameCommand = ReactiveCommand.Create<string>(ChangeBoxCustomViewName);

        AddHeightCommand = ReactiveCommand.Create<bool>(AddHeight);
        AddWidthCommand = ReactiveCommand.Create<bool>(AddWidth);
        LeftCommand = ReactiveCommand.Create<bool>(Left);
        TopCommand = ReactiveCommand.Create<bool>(Top);
        SelectResizableCommand = ReactiveCommand.Create<IChangeable?>(SelectResizable);
        SelectBoxCommand = ReactiveCommand.Create<SpikeBoxViewModel>(SelectBox);
        SaveCommand = ReactiveCommand.CreateFromTask(Save);
        ChangeEditCommand = ReactiveCommand.Create(ChangeEdit);
        LoadTabsCommand = ReactiveCommand.CreateFromTask(LoadTabs);

        this.WhenAnyValue(c => c.CurrentTab).Subscribe(c =>
        {
            if (CurrentBox is not null)
            {
                CurrentBox.Editing = false;
                CurrentBox = null;
            }
            Reload();
        });

        foreach (var item in options.Value.Group.Keys)
        {
            CustomGroups.Add(item);
        }

        LoadMenu();
    }

    [Reactive]
    public ObservableCollection<MenuViewModel> Tools { get; set; } = [];

    public override async Task Activate()
    {
        await LoadTabs();
    }

    public async Task ToolExecute(MenuViewModel menuViewModel)
    {
        switch (menuViewModel.FullName)
        {
            case "Menu.自定义页面.编辑":
                ChangeEdit();
                LoadMenu();
                break;
            case "Menu.自定义页面.保存":
                await Save();
                break;
            case "Menu.自定义页面.标签":
                LoadMenu("tab");
                break;
            case "Menu.自定义页面.标签.添加":
                await AddTab();
                break;
            case "Menu.自定义页面.标签.删除":
                DeleteTab();
                break;
            case "Menu.自定义页面.标签.前移":
                MoveTab(false);
                break;
            case "Menu.自定义页面.标签.后移":
                MoveTab(true);
                break;
            case "Menu.自定义页面.添加盒子":
                Tools.Clear();

                foreach (var item in _customPageOption.Group.Keys)
                {
                    Tools.Add(new MenuViewModel(new MenuInfo() { DisplayName = item, GroupName = item, Name = "CustomPage.Group", Show = true }, _permissionService, ToolExecute));
                }
                break;
            case "CustomPage.Group":
                Tools.Clear();

                foreach (var item in _customPageOption.Group[menuViewModel.DisplayName!])
                {
                    Tools.Add(new MenuViewModel(new MenuInfo() { DisplayName = item.Name, GroupName = item.Category, Name = "CustomPage.Name", Show = true }, _permissionService, ToolExecute));
                }
                break;
            case "CustomPage.Name":
                await AddBoxAsync(menuViewModel.GroupName, menuViewModel.DisplayName);
                LoadMenu();
                break;
            case "Menu.自定义页面.盒子.删除":
                DeleteBox();
                break;
            default:
                LoadMenu();
                break;
        }

    }
    private IDisposable? menuDisposable;
    public void LoadMenu(string? type = null)
    {
        if (menuDisposable is not null)
        {
            menuDisposable.Dispose();
            Tools.Clear();
        }

        switch (type)
        {
            case "tab":
                menuDisposable = _menuService.CreateMenu("Menu.自定义页面.标签", Tools, ToolExecute);
                break;
            case "box":
                menuDisposable = _menuService.CreateMenu("Menu.自定义页面.盒子", Tools, ToolExecute);
                if (Edit)
                {
                    if (CurrentBox is not null)
                    {
                        _menuService.ChangeShow("Menu.自定义页面.盒子.删除", true);
                        _menuService.ChangeShow("Menu.自定义页面.盒子.返回", true);
                    }
                    else
                    {
                        _menuService.ChangeShow("Menu.自定义页面.盒子.删除", false);
                        _menuService.ChangeShow("Menu.自定义页面.盒子.返回", false);
                    }
                }
                break;
            default:
                menuDisposable = _menuService.CreateMenu("Menu.自定义页面", Tools, ToolExecute);
                if (Edit)
                {
                    _menuService.ChangeShow("Menu.自定义页面.保存", true);
                    _menuService.ChangeShow("Menu.自定义页面.编辑", false);
                    _menuService.ChangeShow("Menu.自定义页面.标签", true);
                    _menuService.ChangeShow("Menu.自定义页面.添加盒子", CurrentTab is not null);
                }
                else
                {
                    _menuService.ChangeShow("Menu.自定义页面.保存", false);
                    _menuService.ChangeShow("Menu.自定义页面.编辑", true);
                    _menuService.ChangeShow("Menu.自定义页面.标签", false);
                    _menuService.ChangeShow("Menu.自定义页面.添加盒子", false);
                }
                break;
        }

    }

    public ReactiveCommand<Unit, Unit> ChangeEditCommand { get; }
    public void ChangeEdit()
    {
        Edit = !Edit;
        if (!Edit)
        {
            if (CurrentBox is not null)
            {
                CurrentBox.Editing = false;
                CurrentBox = null;
            }
        }

        Reload();
    }

    public ReactiveCommand<Unit, Unit> ChangeBoxShowViewCommand { get; }
    public void ChangeBoxShowView()
    {
        if (CurrentBox is not null)
        {
            CurrentBox.Inputs = null;
            CurrentBox.ViewCategory = null;
            CurrentBox.ViewName = null;
        }
        Reload();
    }
    public ReactiveCommand<string, Unit> ChangeBoxCustomViewGroupCommand { get; }
    public void ChangeBoxCustomViewGroup(string group)
    {
        if (CurrentBox is not null)
        {
            CurrentBox.ViewCategory = group;
        }
        Reload(group);
    }
    public ReactiveCommand<string, Unit> ChangeBoxCustomViewNameCommand { get; }
    public void ChangeBoxCustomViewName(string name)
    {
        if (CurrentBox is null || string.IsNullOrEmpty(CurrentBox.ViewCategory))
        {
            return;
        }
        CurrentBox.Router.NavigationStack.Clear();
        CurrentBox.ViewName = name;
        var r = _customPageOption.Group[CurrentBox.ViewCategory].First(c => c.Name == name);

        var configEditVM = _serviceProvider.GetRequiredService<ConfigEditViewModel>();
        CurrentBox.ConfigEditViewModel = configEditVM;
        CurrentBox.ConfigEditViewModel.LoadConfig(r.Data, new JsonObject());

        Reload();
    }


    public void Reload(string? group = null)
    {

    }
    public ReactiveCommand<bool, Unit> AddHeightCommand { get; }
    public void AddHeight(bool c)
    {
        int span = 5;
        if (Changeable is null || Changeable is not IResizable resizable)
        {
            return;
        }
        if (resizable is SpikeBoxResizableViewModel)
        {
            span = 1;
        }

        if (c)
        {
            resizable.Height += span;
        }
        else
        {
            if (resizable.Height <= 1)
            {
                return;
            }
            resizable.Height -= span;
        }
    }
    public ReactiveCommand<bool, Unit> AddWidthCommand { get; }
    public void AddWidth(bool c)
    {
        int span = 5;
        if (Changeable is null || Changeable is not IResizable resizable)
        {
            return;
        }
        if (Changeable is SpikeBoxResizableViewModel)
        {
            span = 1;
        }
        if (c)
        {
            resizable.Width += span;
        }
        else
        {
            if (resizable.Width <= 1)
            {
                return;
            }
            resizable.Width -= span;
        }
    }
    public ReactiveCommand<bool, Unit> LeftCommand { get; }
    public void Left(bool c)
    {
        int span = 5;
        if (Changeable is null || Changeable is not IMoveable moveable)
        {
            return;
        }
        if (Changeable is SpikeBoxResizableViewModel)
        {
            span = 1;
        }
        if (c)
        {
            moveable.Left += span;
        }
        else
        {
            if (moveable.Left <= 0)
            {
                return;
            }
            moveable.Left -= span;
        }
    }
    public ReactiveCommand<bool, Unit> TopCommand { get; }
    public void Top(bool c)
    {
        int span = 5;
        if (Changeable is null || Changeable is not IMoveable moveable)
        {
            return;
        }
        if (Changeable is SpikeBoxResizableViewModel)
        {
            span = 1;
        }
        if (c)
        {
            moveable.Top += span;
        }
        else
        {
            if (moveable.Top <= 0)
            {
                return;
            }
            moveable.Top -= span;
        }
    }
    public ReactiveCommand<IChangeable?, Unit> SelectResizableCommand { get; }
    public IChangeable? Changeable { get; set; }
    public void SelectResizable(IChangeable? changeableViewModel)
    {
        Changeable = changeableViewModel;
    }
    [Reactive]
    public ObservableCollection<string> SpikeViewModels { get; set; } = [];
    [Reactive]
    public bool Edit { get; set; }



    public ReactiveCommand<Unit, Unit> LoadTabsCommand { get; set; }
    /// <summary>
    /// 通过设备类型获取Tabs
    /// </summary>
    /// <returns></returns>
    public async Task LoadTabs()
    {
        Tabs.Clear();

        if (!Directory.Exists(_customPageOption.RootPath))
        {
            Directory.CreateDirectory(_customPageOption.RootPath);
        }
        var path = Path.Combine(_customPageOption.RootPath, _customPageOption.Name + ".json");
        if (!File.Exists(path))
        {
            return;
        }
        var str = await File.ReadAllTextAsync(path);
        if (string.IsNullOrEmpty(str))
        {
            return;
        }
        foreach (var item in JsonSerializer.Deserialize<List<SpikeTab>>(str) ?? [])
        {
            SpikeTabViewModel spikeTabViewModel = new() { Name = item.Name };
            foreach (var box in item.Boxes)
            {
                var boxViewModel = new SpikeBoxViewModel()
                {
                    Name = box.Name,
                    ViewName = box.ViewName,
                    ViewCategory = box.ViewCategory,
                    Size = new SpikeBoxResizableViewModel() { Height = box.Size.Height, Width = box.Size.Width, Left = box.Size.Left, Top = box.Size.Top },
                };

                var vm = _serviceProvider.GetKeyedService<ICustomPageInjectViewModel>(box.ViewCategory + ":" + box.ViewName);

                if (vm is ICustomPageViewModel sVm)
                {
                    var configEditVM = _serviceProvider.GetRequiredService<ConfigEditViewModel>();
                    var vinfo = _customPageOption.Group[box.ViewCategory].FirstOrDefault(c => c.Name == box.ViewName);
                    if (vinfo is not null)
                    {
                        configEditVM.LoadConfig(vinfo.Data, null);
                        boxViewModel.SpikeViewModel = sVm;

                        await boxViewModel.DisplayView(box.Inputs);
                    }
                }

                spikeTabViewModel.Boxes.Add(boxViewModel);
            }

            Tabs.Add(spikeTabViewModel);
        }
        if (Tabs.Count > 0)
        {
            CurrentTab = Tabs[0];
        }

    }
    public ReactiveCommand<Unit, Unit> SaveCommand { get; set; }
    public async Task Save()
    {
        if (!Directory.Exists(_customPageOption.RootPath))
        {
            Directory.CreateDirectory(_customPageOption.RootPath);
        }
        var path = Path.Combine(_customPageOption.RootPath, _customPageOption.Name + ".json");
        List<SpikeTab> spikeTabs = [];
        foreach (var item in Tabs)
        {

            SpikeTab spikeTab = new() { Name = item.Name };
            foreach (var box in item.Boxes)
            {
                if (box.Inputs is null)
                {
                    continue;
                }

                SpikeBox spikeBox = new()
                {
                    Name = box.Name,
                    ViewName = box.ViewName,
                    ViewCategory = box.ViewCategory,
                    Inputs = box.Inputs,
                    Size = new SpikeMoveAndResizable() { Height = box.Size.Height, Width = box.Size.Width, Left = box.Size.Left, Top = box.Size.Top }
                };
                spikeTab.Boxes.Add(spikeBox);
            }

            spikeTabs.Add(spikeTab);
        }
        var r = JsonSerializer.Serialize(spikeTabs);
        await File.WriteAllTextAsync(path, r);
        Edit = false;
        if (CurrentBox is not null)
        {
            CurrentBox.Editing = false;
        }
        CurrentBox = null;

        LoadMenu();
    }

    [Reactive]
    public ObservableCollection<SpikeTabViewModel> Tabs { get; set; } = [];

    [Reactive]
    public SpikeTabViewModel? CurrentTab { get; set; }
    public void MoveTab(bool back)
    {
        if (CurrentTab is null)
        {
            return;
        }
        var old = Tabs.IndexOf(CurrentTab);
        int newIndex;
        if (back)
        {
            newIndex = old + 1;
            if (newIndex >= Tabs.Count)
            {
                return;
            }
        }
        else
        {
            newIndex = old - 1;
            if (newIndex < 0)
            {
                return;
            }
        }
        Tabs.Move(old, newIndex);
    }
    public async Task AddTab()
    {
        var r = await _messageBoxManager.Prompt.Handle(new PromptInfo("请输入标签名称") { DefaultValue = "New Tab" + (Tabs.Count + 1), OwnerTitle = WindowTitle });
        if (r.Ok && !string.IsNullOrEmpty(r.Value))
        {
            Tabs.Add(new SpikeTabViewModel() { Name = r.Value });
            if (Tabs.Count == 1)
            {
                CurrentTab = Tabs.First();
            }
        }

    }

    public async Task AddBoxAsync(string viewCategory, string viewName)
    {
        if (CurrentTab is null)
        {
            return;
        }
        var r = await _messageBoxManager.Prompt.Handle(new PromptInfo("请输入分组名称") { DefaultValue = "New Box" + (CurrentTab.Boxes.Count + 1), OwnerTitle = WindowTitle });
        if (r.Ok && !string.IsNullOrEmpty(r.Value))
        {
            var left = 0;
            var top = 0;
            if (CurrentTab.Boxes.Any())
            {
                left = CurrentTab.Boxes.Max(c => c.Size.Left + c.Size.Width);
                if (left >= 6)
                {
                    top = CurrentTab.Boxes.Max(c => c.Size.Top + c.Size.Height);
                    left = 0;
                }
            }

            if (left > 6)
            {
                left = 6;
            }
            if (top > 6)
            {
                top = 6;
            }
            var box = new SpikeBoxViewModel() { Name = r.Value };
            box.Size.Left = left;
            box.Size.Top = top;
            box.ViewCategory = viewCategory;
            box.ViewName = viewName;

            var vm = _serviceProvider.GetKeyedService<ICustomPageInjectViewModel>(box.ViewCategory + ":" + box.ViewName);

            if (vm is ICustomPageViewModel sVm)
            {
                var configEditVM = _serviceProvider.GetRequiredService<ConfigEditViewModel>();
                var vinfo = _customPageOption.Group[viewCategory].FirstOrDefault(c => c.Name == viewName);
                if (vinfo is not null)
                {
                    configEditVM.LoadConfig(vinfo.Data, null);
                    box.ConfigEditViewModel = configEditVM;
                    box.SpikeViewModel = sVm;
                }

            }
            CurrentTab.Boxes.Add(box);
        }
    }
    public void DeleteTab()
    {
        if (CurrentTab is null)
        {
            return;
        }
        Tabs.Remove(CurrentTab);
    }
    public SpikeBoxViewModel? CurrentBox { get; set; }
    public ReactiveCommand<SpikeBoxViewModel, Unit> SelectBoxCommand { get; }
    public void SelectBox(SpikeBoxViewModel spikeBoxViewModel)
    {
        if (!Edit)
        {
            return;
        }
        if (CurrentBox is null)
        {
            CurrentBox = spikeBoxViewModel;
            CurrentBox.Editing = true;
            SelectResizable(CurrentBox.Size);
            LoadMenu("box");
        }
        else
        {
            CurrentBox.Editing = false;
            if (CurrentBox == spikeBoxViewModel)
            {
                CurrentBox = null;
                SelectResizable(null);
                Reload();
                LoadMenu();

                return;
            }
            else
            {
                CurrentBox = spikeBoxViewModel;
                CurrentBox.Editing = true;
                LoadMenu("box");
            }
            SelectResizable(CurrentBox.Size);
        }
    }
    public ReactiveCommand<Unit, Unit> DeleteBoxCommand { get; }
    public void DeleteBox()
    {
        if (CurrentTab is null || CurrentBox is null)
        {
            return;
        }

        CurrentTab.Boxes.Remove(CurrentBox);
        LoadMenu();
    }
    [Reactive]
    public ObservableCollection<string> CustomGroups { get; set; } = [];
    [Reactive]
    public ObservableCollection<string> CustomViews { get; set; } = [];
}
public class SpikeTab
{
    public required string Name { get; set; }

    public List<SpikeBox> Boxes { get; set; } = [];

}
public class SpikeBox
{
    public required string Name { get; set; }

    public string? ViewCategory { get; set; }
    public string? ViewName { get; set; }
    public List<NameValue> Inputs { get; set; } = [];

    public SpikeMoveAndResizable Size { get; set; } = new SpikeMoveAndResizable();
}

public class SpikeTabViewModel : ReactiveObject
{
    [Reactive]
    public required string Name { get; set; }
    [Reactive]
    public ObservableCollection<SpikeBoxViewModel> Boxes { get; set; } = [];
}
public class SpikeBoxViewModel : ReactiveObject, IScreen
{
    public SpikeBoxViewModel()
    {
        DisplayViewCommand = ReactiveCommand.CreateFromTask(DisplayView);
    }
    [Reactive]
    public bool Editing { get; set; }
    public required string Name { get; set; }
    [Reactive]
    public string? ViewName { get; set; }
    [Reactive]
    public string? ViewCategory { get; set; }

    [Reactive]
    public ICustomPageViewModel? SpikeViewModel { get; set; }


    [Reactive]
    public SpikeBoxResizableViewModel Size { get; set; } = new();

    [Reactive]
    public ConfigEditViewModel? ConfigEditViewModel { get; set; }

    public List<NameValue>? Inputs { get; set; }

    [Reactive]
    public RoutingState Router { get; set; } = new RoutingState();

    public ReactiveCommand<Unit, Unit> DisplayViewCommand { get; }

    public async Task DisplayView()
    {
        if (ConfigEditViewModel is not null)
        {
            await DisplayView(ConfigEditViewModel.GetNameValues());
        }
    }

    public async Task DisplayView(List<NameValue> data)
    {
        Inputs = data;
        if (SpikeViewModel is ICustomPageViewModel modelBase)
        {
            modelBase.SetCustomPageValue(data);
            modelBase.SetScreen(this);
            await Router.Navigate.Execute(modelBase);
        }
    }
}


public class SpikeBoxResizableViewModel : ReactiveObject, IResizable, IMoveable
{
    [Reactive]
    public int Left { get; set; }
    [Reactive]
    public int Top { get; set; }
    [Reactive]
    public int Height { get; set; } = 2;
    [Reactive]
    public int Width { get; set; } = 2;

}
public class SpikeMoveAndResizable : IResizable, IChangeable, IMoveable
{
    public int Width { get; set; }

    public int Height { get; set; }

    public int Left { get; set; }

    public int Top { get; set; }
}

public interface IChangeable
{
}

public interface IResizable : IChangeable
{
    int Width { get; set; }

    int Height { get; set; }
}
public interface IMoveable : IChangeable
{
    int Left { get; set; }

    int Top { get; set; }
}