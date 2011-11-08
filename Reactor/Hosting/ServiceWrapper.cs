using System;
using System.ServiceProcess;
using Reactor.ServiceGrid;

namespace Reactor.Hosting
{
    public partial class ServiceWrapper : ServiceBase
    {
        private readonly IReactorService _currentReactorService;

        public ServiceWrapper(IReactorService currentReactorService)
        {
            if (currentReactorService == null) throw new ArgumentNullException("currentReactorService");

            _currentReactorService = currentReactorService;

            InitializeComponent();

            ServiceName = _currentReactorService.Identifier.Name;
        }

        protected override void OnStart(string[] args)
        {
            _currentReactorService.Start();
        }

        protected override void OnStop()
        {
            _currentReactorService.Stop();
        }
    }
}
