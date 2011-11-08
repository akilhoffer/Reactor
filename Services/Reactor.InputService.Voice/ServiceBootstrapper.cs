using System;
using System.IO;
using System.Reflection;
using MassTransit;
using Reactor.ServiceGrid;
using Topshelf.Configuration.Dsl;
using log4net;
using log4net.Config;

namespace Reactor.InputService.Voice
{
    public class ServiceBootstrapper : ServiceBootstrapperBase<Service>
    {
        private const string ServiceName = "VoiceInputService";

        public override void InitializeHostedService(IServiceConfigurator<Service> cfg)
        {
            // Configure the service bus
            Bus.Initialize(sbc =>
            {
                sbc.UseRabbitMq();
                sbc.UseRabbitMqRouting();
                sbc.ReceiveFrom(string.Format("rabbitmq://localhost/{0}", ServiceName));
            });

            // Continue service setup
            cfg.HowToBuildService(n => new Service(Bus.Instance, InitializationCustomizationManager));
            cfg.WhenStarted(s =>
            {
                XmlConfigurator.Configure(new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, string.Format("{0}.log4net.config", ServiceName))));

                s.Identifier = new ServiceIdentifier(ServiceName, Assembly.GetExecutingAssembly().GetName().Version);
                s.Start();
            });
            cfg.WhenStopped(s =>
            {
                s.Stop();
                LogManager.Shutdown();
            });
        }
    }
}
