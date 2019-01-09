namespace SpotlightImageViewer
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Media.Imaging;
    using Caliburn.Micro;
    using Unity;
    using ViewModels;

    public class AppBootstrapper : BootstrapperBase
    {
        private readonly IUnityContainer _container = new UnityContainer();

        public AppBootstrapper()
        {
            Initialize();
        }

        protected override void Configure()
        {
            _container.RegisterInstance<IWindowManager>(new WindowManager());
            _container.RegisterInstance<IEventAggregator>(new EventAggregator());

            _container.RegisterType<IShell, ShellViewModel>();
        }

        protected override object GetInstance(Type service, string key)
        {
            var instance = _container.Resolve(service, key);

            if (instance != null)
                return instance;

            throw new InvalidOperationException($"Could not locate any instances of {service.Namespace}.{service.Name}.");
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.ResolveAll(service);
        }

        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            // add App.ico to the displayed window
            var settings = new Dictionary<string, object>
            {
                // details at: https://msdn.microsoft.com/en-us/library/aa970069%28v=vs.100%29.aspx
                {
                    "Icon",
                    new BitmapImage(new Uri("pack://application:,,,/SpotlightImageViewer;component/App.ico"))
                }
            };

            DisplayRootViewFor<IShell>(settings);
        }
    }
}