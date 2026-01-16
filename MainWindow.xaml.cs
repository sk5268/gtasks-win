using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Google_Tasks_Client.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace Google_Tasks_Client
{
    public sealed partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; }

        public MainWindow()
        {
            this.InitializeComponent();
            ViewModel = new MainViewModel();
            RootGrid.DataContext = ViewModel;
            this.Closed += MainWindow_Closed;
        }

        public void NewTaskAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            NewTaskTextBox.Focus(FocusState.Programmatic);
            args.Handled = true;
        }

        public void NewTaskListAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            NewTaskListTextBox.Focus(FocusState.Programmatic);
            args.Handled = true;
        }

        public void NewTaskTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (ViewModel.AddTaskCommand.CanExecute(null))
                {
                    ViewModel.AddTaskCommand.Execute(null);
                    e.Handled = true;
                }
            }
        }

        public void NewTaskListTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (ViewModel.AddTaskListCommand.CanExecute(null))
                {
                    ViewModel.AddTaskListCommand.Execute(null);
                    e.Handled = true;
                }
            }
        }

        private async void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            await ViewModel.ShutdownAsync();
        }
    }
}