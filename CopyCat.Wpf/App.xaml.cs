using CopyCat.Common.Modules;
using CopyCat.Wpf.Views;
using Prism.Ioc;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace CopyCat.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            return new ShellView();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<PoseTracker>();
        }

        protected override void OnInitialized()
        {
            var poseTracker = Container.Resolve<PoseTracker>();

            base.OnInitialized();
        }
    }
}
