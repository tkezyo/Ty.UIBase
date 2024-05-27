using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ty.ViewModels.Configs;

namespace Ty.Controls
{
    /// <summary>
    /// PropertiyView.xaml 的交互逻辑
    /// </summary>
    public partial class PropertiyView : UserControl
    {
        //绑定属性 AddCommand
        public static readonly DependencyProperty AddCommandProperty = DependencyProperty.Register("AddCommand", typeof(ICommand), typeof(PropertiyView));
        public ICommand AddCommand
        {
            get { return (ICommand)GetValue(AddCommandProperty); }
            set { SetValue(AddCommandProperty, value); }
        }


        public static readonly DependencyProperty SetObjectCommandProperty = DependencyProperty.Register("SetObjectCommand", typeof(ICommand), typeof(PropertiyView));
        public ICommand SetObjectCommand
        {
            get { return (ICommand)GetValue(SetObjectCommandProperty); }
            set { SetValue(SetObjectCommandProperty, value); }
        }

        //绑定属性 RemoveCommand
        public ReactiveCommand<ConfigViewModel, Unit> RemoveCommand { get; set; }
        public void Remove(ConfigViewModel configViewModel)
        {
            if (DataContext is ConfigViewModel config)
            {
                config.Properties.Remove(configViewModel);
            }
        }

        //UpCommand
        public ReactiveCommand<ConfigViewModel, Unit> UpCommand { get; }
        public void Up(ConfigViewModel configViewModel)
        {
            if (DataContext is ConfigViewModel config)
            {
                //如果是第一个则不处理
                var index = config.Properties.IndexOf(configViewModel);
                if (index == 0)
                {
                    return;
                }

                config.Properties.Move(index, index - 1);
            }
        }

        //DownCommand
        public ReactiveCommand<ConfigViewModel, Unit> DownCommand { get; }
        public void Down(ConfigViewModel configViewModel)
        {
            if (DataContext is ConfigViewModel config)
            {
                //如果是第一个则不处理
                var index = config.Properties.IndexOf(configViewModel);
                if (index == config.Properties.Count - 1)
                {
                    return;
                }

                config.Properties.Move(index, index + 1);
            }
        }

        //CopyCommand
        public static readonly DependencyProperty CopyCommandProperty = DependencyProperty.Register("CopyCommand", typeof(ICommand), typeof(PropertiyView));
        public ICommand CopyCommand
        {
            get { return (ICommand)GetValue(CopyCommandProperty); }
            set { SetValue(CopyCommandProperty, value); }
        }

        //PasteCommand
        public static readonly DependencyProperty PasteCommandProperty = DependencyProperty.Register("PasteCommand", typeof(ICommand), typeof(PropertiyView));
        public ICommand PasteCommand
        {
            get { return (ICommand)GetValue(PasteCommandProperty); }
            set { SetValue(PasteCommandProperty, value); }
        }

        public PropertiyView()
        {
            RemoveCommand = ReactiveCommand.Create<ConfigViewModel>(Remove);
            UpCommand = ReactiveCommand.Create<ConfigViewModel>(Up);
            DownCommand = ReactiveCommand.Create<ConfigViewModel>(Down);
            InitializeComponent();
        }
    }
}
