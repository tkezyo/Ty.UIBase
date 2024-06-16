using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System.Reactive;
using System.Reactive.Linq;
using Ty.AvaloniaBase.Views;
using Ty.Services;
using Ty.ViewModels;

namespace Ty
{
    public class MessageBoxManager : IMessageBoxManager
    {
        /// <summary>
        /// 确认
        /// </summary>
        public Interaction<ConformInfo, bool> Conform { get; }
        /// <summary>
        /// 提示
        /// </summary>
        public Interaction<AlertInfo, Unit> Alert { get; }
        /// <summary>
        /// 模态框
        /// </summary>
        public Interaction<ModalInfo, bool> Modals { get; }
        /// <summary>
        /// 打开窗口
        /// </summary>
        public Interaction<ModalInfo, bool> Window { get; }
        /// <summary>
        /// 保存文件
        /// </summary>
        public Interaction<SaveFilesInfo, string?> SaveFile { get; }
        /// <summary>
        /// 打开文件
        /// </summary>
        public Interaction<OpenFilesInfo, string[]?> OpenFiles { get; }
        public Interaction<PromptInfo, PromptResult> Prompt { get; }

        public Interaction<string, string?> SelectFolder { get; }
        public Interaction<NotifyInfo, Unit> Notify { get; }

        public MessageBoxManager()
        {
            Conform = new Interaction<ConformInfo, bool>();
            Alert = new Interaction<AlertInfo, Unit>();
            Modals = new Interaction<ModalInfo, bool>();
            Window = new Interaction<ModalInfo, bool>();
            SaveFile = new Interaction<SaveFilesInfo, string?>();
            OpenFiles = new Interaction<OpenFilesInfo, string[]?>();
            Prompt = new Interaction<PromptInfo, PromptResult>();
            SelectFolder = new Interaction<string, string?>();
            Notify = new Interaction<NotifyInfo, Unit>();

            Alert.RegisterHandler(DoAlertAsync);
            Modals.RegisterHandler(DoShowDialogAsync);
            Window.RegisterHandler(DoShowWindowAsync);
            SaveFile.RegisterHandler(SaveFileAsync);
            OpenFiles.RegisterHandler(FileDialogAsync);
            Conform.RegisterHandler(ConformDialogAsync);
            Prompt.RegisterHandler(PromptDialogAsync);
            SelectFolder.RegisterHandler(FileFolderAsync);
            Notify.RegisterHandler(NotifyAsync);
        }
        public WindowNotificationManager? Manager { get; set; }

        /// <summary>
        /// 需要在MainWindow的OnOpened中调用
        /// </summary>
        /// <param name="visual"></param>
        public void SetNotifyManager(Visual visual)
        {
            var topLevel = TopLevel.GetTopLevel(visual);
            Manager = new WindowNotificationManager(topLevel) { MaxItems = 3 };
        }

        protected virtual async Task NotifyAsync(IInteractionContext<NotifyInfo, Unit> interaction)
        {
            NotificationType notificationType = interaction.Input.Level switch
            {
                NotifyLevel.Info => NotificationType.Information,
                NotifyLevel.Warning => NotificationType.Warning,
                NotifyLevel.Error => NotificationType.Error,
                _ => NotificationType.Information,

            };

            Manager?.Show(new Avalonia.Controls.Notifications.Notification(interaction.Input.Title, interaction.Input.Message, notificationType, interaction.Input.Expiration));
            await Task.CompletedTask;
            interaction.SetOutput(Unit.Default);
        }

        protected virtual async Task ConformDialogAsync(IInteractionContext<ConformInfo,
                                  bool> interaction)
        {
            var window = GetCurrentWindow(interaction.Input.OwnerTitle);


            var dialog = new ConformDialog
            {
                Title = interaction.Input.Title,
                DataContext = interaction.Input.Message
            };


            var r = await dialog.ShowDialog<bool>(window);
            interaction.SetOutput(r);
        }
        protected virtual async Task PromptDialogAsync(IInteractionContext<PromptInfo,
                                  PromptResult> interaction)
        {
            var dialog = TyApp.ServiceProvider.GetRequiredKeyedService<IViewFor>(typeof(PromptDialogViewModel).FullName);
            var viewModel = TyApp.ServiceProvider.GetRequiredService<PromptDialogViewModel>();

            viewModel.Title = interaction.Input.Title;
            viewModel.DefaultValue = interaction.Input.DefaultValue;
            dialog.ViewModel = viewModel;
            IDisposable? disposable = null;
            TaskCompletionSource<PromptResult> tcs = new();
            if (dialog is Window window)
            {
                window.DataContext = viewModel;
                var o = Observable.FromEventPattern<EventHandler, EventArgs>(c => window.Closed += c, c => window.Closed -= c).Select(c => false);
                disposable = viewModel.CloseCommand
                    .Merge(o)
                    .Subscribe(c =>
                    {
                        tcs.SetResult(new PromptResult
                        {
                            Ok = c,
                            Value = viewModel.DefaultValue
                        });
                        disposable?.Dispose();
                    });
                var ownerWindow = GetCurrentWindow(interaction.Input.OwnerTitle);
                _ = window.ShowDialog(ownerWindow);
            }
            var r = await tcs.Task;
            interaction.SetOutput(r);
            return;
        }
        protected virtual async Task DoShowDialogAsync(IInteractionContext<ModalInfo,
                                           bool> interaction)
        {
            var dialog = TyApp.ServiceProvider.GetRequiredKeyedService<IViewFor>(typeof(ModalDialogViewModel).FullName);
            var viewModel = TyApp.ServiceProvider.GetRequiredService<ModalDialogViewModel>();

            viewModel.Title = interaction.Input.Title;
            viewModel.Width = interaction.Input.Width;
            viewModel.Height = interaction.Input.Height;
            viewModel.ModalViewModel = interaction.Input.ViewModel;
            IDisposable? disposable = null;
            TaskCompletionSource<bool> tcs = new();

            if (dialog is Window window)
            {
                window.DataContext = viewModel;
                var o = Observable.FromEventPattern<EventHandler, EventArgs>(c => window.Closed += c, c => window.Closed -= c).Select(c => false);
                disposable = viewModel.ModalViewModel.CloseCommand.Merge(o).Subscribe(c =>
                {
                    tcs.SetResult(c);
                    disposable?.Dispose();
                });
                var ownerWindow = GetCurrentWindow(interaction.Input.OwnerTitle);
                _ = window.ShowDialog(ownerWindow);
            }
            var r = await tcs.Task;
            interaction.SetOutput(r);
        }
        protected virtual async Task DoShowWindowAsync(IInteractionContext<ModalInfo,
                                       bool> interaction)
        {
            var dialog = TyApp.ServiceProvider.GetRequiredKeyedService<IViewFor>(typeof(ModalDialogViewModel).FullName);
            var viewModel = TyApp.ServiceProvider.GetRequiredService<ModalDialogViewModel>();

            viewModel.Title = interaction.Input.Title;
            viewModel.Width = interaction.Input.Width;
            viewModel.Height = interaction.Input.Height;
            viewModel.ModalViewModel = interaction.Input.ViewModel;
            IDisposable? disposable = null;
            TaskCompletionSource<bool> tcs = new();

            if (global::Avalonia.Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
                dialog is Window window)
            {
                window.DataContext = viewModel;
                var o = Observable.FromEventPattern<EventHandler, EventArgs>(c => window.Closed += c, c => window.Closed -= c).Select(c => false);
                disposable = viewModel.ModalViewModel.CloseCommand.Merge(o).Subscribe(c =>
                {
                    tcs.SetResult(c);
                    disposable?.Dispose();
                });
                var ownerWindow = GetCurrentWindow(interaction.Input.OwnerTitle);
                window.Show(ownerWindow);
            }
            var r = await tcs.Task;
            interaction.SetOutput(r);
        }
        protected virtual async Task DoAlertAsync(IInteractionContext<AlertInfo,
                                          Unit> interaction)
        {
            var window = GetCurrentWindow(interaction.Input.OwnerTitle);

            var box2 = MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard(new MsBox.Avalonia.Dto.MessageBoxStandardParams
            {
                ContentTitle = interaction.Input.Title ?? "提示",
                ContentMessage = interaction.Input.Message,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            });
            await box2.ShowWindowDialogAsync(window);
            interaction.SetOutput(Unit.Default);
            return;



            //var dialog = new MessageDialog();
            //dialog.DataContext = interaction.Input;
            //if (global::Avalonia.Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            //{

            //    await dialog.ShowDialog(desktop.MainWindow!);
            //}
            //interaction.SetOutput(Unit.Default);
        }

        protected virtual async Task FileDialogAsync(IInteractionContext<OpenFilesInfo,
                                         string[]?> interaction)
        {
            IStorageProvider? sp = GetStorageProvider();
            if (sp is null) return;

            FilePickerFileType filePickerFileType = FilePickerFileTypes.All;
            if (!string.IsNullOrEmpty(interaction.Input.Filter) && !string.IsNullOrEmpty(interaction.Input.FilterName))
            {
                filePickerFileType = new FilePickerFileType(interaction.Input.FilterName)
                {
                    Patterns = [interaction.Input.Filter],
                    MimeTypes = [interaction.Input.Filter]
                };
            }

            var result = await sp.OpenFilePickerAsync(new FilePickerOpenOptions()
            {
                Title = interaction.Input.Title,
                FileTypeFilter = [filePickerFileType],
                AllowMultiple = interaction.Input.Multiselect,
            });

            interaction.SetOutput(result.Select(c => c.TryGetLocalPath() ?? string.Empty).ToArray());
            return;


            //var dlg = new OpenFileDialog();
            //dlg.Filters.Add(new FileDialogFilter() { Name = interaction.Input, Extensions = { interaction.Input } });
            //dlg.AllowMultiple = true;

            //if (Avalonia.Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            //{
            //    var files = await dlg.ShowAsync(desktop.MainWindow);
            //    interaction.SetOutput(files);
            //    return;
            //}
            //interaction.SetOutput(Array.Empty<string>());
        }

        protected virtual async Task FileFolderAsync(IInteractionContext<string,
                                        string?> interaction)
        {
            IStorageProvider? sp = GetStorageProvider();
            if (sp is null) return;
            var result = await sp.OpenFolderPickerAsync(new FolderPickerOpenOptions()
            {
                Title = interaction.Input,
                AllowMultiple = false,
            });
            if (result.Count == 0)
            {
                interaction.SetOutput(null);
                return;
            }
            interaction.SetOutput(result[0].Path.LocalPath);
        }
        protected virtual async Task SaveFileAsync(IInteractionContext<SaveFilesInfo,
                                    string?> interaction)
        {
            IStorageProvider? sp = GetStorageProvider();
            if (sp is null) return;


            FilePickerFileType filePickerFileType = FilePickerFileTypes.All;
            if (!string.IsNullOrEmpty(interaction.Input.Filter) && !string.IsNullOrEmpty(interaction.Input.FilterName))
            {
                filePickerFileType = new FilePickerFileType(interaction.Input.FilterName)
                {
                    Patterns = [interaction.Input.Filter],
                    MimeTypes = [interaction.Input.Filter]
                };
            }

            var result = await sp.SaveFilePickerAsync(new FilePickerSaveOptions()
            {
                Title = interaction.Input.Title,
                DefaultExtension = interaction.Input.DefaultExtension,
                SuggestedFileName = interaction.Input.FileName,
                FileTypeChoices = [filePickerFileType],
            });
            //var dlg = new SaveFileDialog();
            //dlg.DefaultExtension = interaction.Input;

            //if (Avalonia.Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            //{
            //    var files = await dlg.ShowAsync(desktop.MainWindow);
            //    interaction.SetOutput(files);
            //    return;
            //}
            interaction.SetOutput(result?.Name);
        }

        protected virtual IStorageProvider? GetStorageProvider()
        {
            if (Avalonia.Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var topLevel = TopLevel.GetTopLevel(desktop.MainWindow);
                return topLevel?.StorageProvider;
            }
            return null;
        }
        private static Window GetCurrentWindow(string? ownerTitle)
        {
            if (Avalonia.Application.Current!.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                foreach (Window item in desktop.Windows)
                {
                    if (item.Title == ownerTitle)
                    {
                        return item;
                    }
                }
                return GetLastWindow(desktop.MainWindow!);
            }
            return null!;
        }
        private static Window GetLastWindow(Window window)
        {
            if (window.OwnedWindows.Count == 0)
            {
                return window;
            }
            return GetLastWindow(window.OwnedWindows[0]);
        }
    }
}
