﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
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

            Alert.RegisterHandler(DoAlertAsync);
            Modals.RegisterHandler(DoShowDialogAsync);
            Window.RegisterHandler(DoShowWindowAsync);
            SaveFile.RegisterHandler(SaveFileAsync);
            OpenFiles.RegisterHandler(FileDialogAsync);
            Conform.RegisterHandler(ConformDialogAsync);
            Prompt.RegisterHandler(PromptDialogAsync);
            SelectFolder.RegisterHandler(FileFolderAsync);
        }
        protected virtual async Task FileFolderAsync(IInteractionContext<string,
                                     string?> interaction)
        {
            var dialog = new OpenFolderDialog
            {
                Multiselect = false,
                Title = interaction.Input
            };
            var result = dialog.ShowDialog();
            if (result == true)
            {
                interaction.SetOutput(dialog.FolderName);
                return;
            }
            interaction.SetOutput(null);
            await Task.CompletedTask;
        }
        protected virtual void ConformDialogAsync(IInteractionContext<ConformInfo,
                                    bool> interaction)
        {

            var window = GetCurrentWindow(interaction.Input.OwnerTitle);

            var rr = MessageBox.Show(window, interaction.Input.Message, interaction.Input.Title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            interaction.SetOutput(rr == MessageBoxResult.Yes);
            return;

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
                window.Owner = ownerWindow;
                window.Show();
            }
            var r = await tcs.Task;
            interaction.SetOutput(r);
            return;
        }
        protected virtual void FileDialogAsync(IInteractionContext<OpenFilesInfo,
                                            string[]?> interaction)
        {
            var dlg = new OpenFileDialog
            {
                Multiselect = interaction.Input.Multiselect,
                Filter = $"({interaction.Input.FilterName})|{interaction.Input.Filter}",
                Title = interaction.Input.Title
            };

            dlg.ShowDialog();
            interaction.SetOutput(dlg.FileNames);
            return;
        }
        protected virtual void SaveFileAsync(IInteractionContext<SaveFilesInfo,
                                       string?> interaction)
        {
            var dlg = new SaveFileDialog
            {
                DefaultExt = interaction.Input.DefaultExtension,
                Title = interaction.Input.Title,
                Filter = interaction.Input.Filter,
                FileName = interaction.Input.FileName
            };


            dlg.ShowDialog();

            interaction.SetOutput(dlg.FileName);
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
            dialog.ViewModel = viewModel;
            IDisposable? disposable = null;
            TaskCompletionSource<bool> tcs = new();
            if (dialog is Window window)
            {
                window.DataContext = viewModel;
                window.Height = viewModel.Height;
                window.Width = viewModel.Width;
                var o = Observable.FromEventPattern<EventHandler, EventArgs>(c => window.Closed += c, c => window.Closed -= c).Select(c => false);
                disposable = viewModel.ModalViewModel.CloseCommand
                    .Merge(o)
                    .Subscribe(c =>
                {
                    tcs.SetResult(c);
                    disposable?.Dispose();
                });
                var ownerWindow = GetCurrentWindow(interaction.Input.OwnerTitle);
                window.Owner = ownerWindow;

                RxApp.MainThreadScheduler.Schedule(X =>
                {
                    window.ShowDialog();
                });
            }
            var r = await tcs.Task;
            interaction.SetOutput(r);
        }
        protected virtual async Task DoShowWindowAsync(IInteractionContext<ModalInfo,
                                          bool> interaction)
        {
            if (interaction.Input.OnlyOne)
            {
                var old = FindWindow(interaction.Input.Title);
                if (old is not null)
                {
                    if (old.WindowState == WindowState.Minimized)
                    {
                        old.WindowState = WindowState.Normal;
                    }
                    old.Focus();
                    interaction.SetOutput(true);
                    return;
                }
            }
            IDisposable? disposable = null;
            var dialog = TyApp.ServiceProvider.GetRequiredKeyedService<IViewFor>(typeof(ModalDialogViewModel).FullName);
            var viewModel = TyApp.ServiceProvider.GetRequiredService<ModalDialogViewModel>();

            viewModel.Title = interaction.Input.Title;
            viewModel.Width = interaction.Input.Width;
            viewModel.Height = interaction.Input.Height;
            viewModel.ModalViewModel = interaction.Input.ViewModel;
            dialog.ViewModel = viewModel;
            TaskCompletionSource<bool> tcs = new();
            if (dialog is Window window)
            {
                window.DataContext = viewModel;
                window.Height = viewModel.Height;
                window.Width = viewModel.Width;
                var o = Observable.FromEventPattern<EventHandler, EventArgs>(c => window.Closed += c, c => window.Closed -= c).Select(c => false);
                disposable = viewModel.ModalViewModel.CloseCommand.Merge(o).Subscribe(c =>
                {
                    tcs.SetResult(c);
                    disposable?.Dispose();
                });
                if (interaction.Input.OwnerTitle is not null)
                {
                    var ownerWindow = GetCurrentWindow(interaction.Input.OwnerTitle);
                    window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    window.Owner = ownerWindow;
                }
                else
                {
                    window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }

                window.Show();
            }
            if (!interaction.Input.OnlyOne)
            {
                var r = await tcs.Task;
                interaction.SetOutput(r);
            }
            else
            {
                interaction.SetOutput(true);
            }
        }
        protected virtual async Task DoAlertAsync(IInteractionContext<AlertInfo,
                                          Unit> interaction)
        {
            var window = GetCurrentWindow(interaction.Input.OwnerTitle);
            if (window is not null)
            {
                MessageBox.Show(window, interaction.Input.Message, interaction.Input.Title ?? "提示");
                interaction.SetOutput(Unit.Default);
                return;
            }


            MessageBox.Show(interaction.Input.Message, interaction.Input.Title);
            interaction.SetOutput(Unit.Default);
            await Task.CompletedTask;
        }

        private static Window? FindWindow(string? title)
        {
            foreach (Window item in Application.Current.Windows)
            {
                if (item.Title == title)
                {
                    return item;
                }
            }
            return null;
        }

        private static Window GetCurrentWindow(string? ownerTitle)
        {
            foreach (Window item in Application.Current.Windows)
            {
                if (item.Title == ownerTitle)
                {
                    return item;
                }
            }
            return GetLastWindow(Application.Current.MainWindow);
        }
        private static Window GetLastWindow(Window window)
        {
            if (window.OwnedWindows.Count==0)
            {
                return window;
            }
            return GetLastWindow(window.OwnedWindows[0]);
        }
    }
}
