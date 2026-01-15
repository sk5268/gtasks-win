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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Google_Tasks_Client
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
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

        

                private async void MainWindow_Closed(object sender, WindowEventArgs args)

                {

                    // Note: In WinUI 3, this might be synchronous or fire after window is gone,

                    // but we call our async shutdown. 

                    // For a production app, we might use a deferral if available or a task wait.

                    await ViewModel.ShutdownAsync();

                }

            }

        }

        