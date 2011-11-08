using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using Reactor.ClientData.Services;
using Reactor.Configuration;

namespace Reactor.ServiceGrid
{
    public class ReactorCore : ReactorServiceBase
    {
        #region Fields

        private StreamServiceFactory _streamServiceFactory;
        private Uri _wcfServiceUri;
        private ServiceHost _wcfServiceHost;

        #endregion

        public ReactorCore(IConfigurationAggregator configurationAggregator) : base(configurationAggregator) {}

        protected override void OnBaseStarted()
        {
            CreateStreamServiceFactory();
            CreateWcfService();
        }

        private void CreateWcfService()
        {
            _wcfServiceUri = new Uri(string.Format("http://{0}:6969/core", Identifier.Name));
            _wcfServiceHost = new ServiceHost(typeof(ReactorCoreSvc), _wcfServiceUri);

            // Add service endpoint
            _wcfServiceHost.AddServiceEndpoint(typeof (ICoreService), new WSHttpBinding(), string.Empty);

            // Enable metadata publishing.
            ServiceMetadataBehavior smb = new ServiceMetadataBehavior
            {
                HttpGetEnabled = true,
                MetadataExporter = {PolicyVersion = PolicyVersion.Policy15}
            };
            _wcfServiceHost.Description.Behaviors.Add(smb);
            _wcfServiceHost.Open(TimeSpan.FromSeconds(10));

            Log.InfoFormat("Client data service started and listening at: {0}", _wcfServiceUri);
        }

        protected override void OnShuttingDown()
        {
            _streamServiceFactory.Stop();

            if(_wcfServiceHost != null && (_wcfServiceHost.State == CommunicationState.Opened || _wcfServiceHost.State == CommunicationState.Opening))
            {
                _wcfServiceHost.Close(TimeSpan.FromSeconds(10));

                Log.Info("Client data service stopped");
            }
        }

        private void CreateStreamServiceFactory()
        {
            _streamServiceFactory = new StreamServiceFactory(ConfigurationAggregator);
            _streamServiceFactory.Start();
        }
    }
}
