using System;
using System.ComponentModel.Composition;
using MassTransit;
using Reactor.Customization;
using Reactor.ServiceGrid;

namespace Reactor.MessagingTransport
{
    [Export(typeof(ICustomizeReactorInitialization))]
    public class TransportCustomizer : ICustomizeReactorInitialization
    {
        #region Implementation of ICustomizeReactorInitialization

        /// <summary>
        /// Gets the execution order for this initializer. The lower the value, the earlier it will 
        /// be executed in a chain of initializers.
        /// </summary>
        /// <value>The execution order.</value>
        public uint ExecutionOrder
        {
            get { return 2; }
        }

        /// <summary>
        /// Initializes Reactor. Implementers can provide complete custom initialization of the Reactor Service, bypassing 
        /// all default initialization. Upon completion, validation of the Reactor Context is performed. If critical services are 
        /// not present on the Context, a fatal exception will cause the Reactor Service to terminate.
        /// </summary>
        public void InitializeReactor()
        {
            // We need a service name in order create the bus endpoint and subscriptions
            if(string.IsNullOrEmpty(InitializationContext.ServiceName))
                throw new InvalidOperationException("No ServiceName provided on InitializationContext.");

            // Configure the service bus
            Bus.Initialize(sbc =>
            {
                sbc.UseRabbitMq();
                sbc.UseRabbitMqRouting();
                sbc.ReceiveFrom(string.Format("rabbitmq://localhost/{0}", InitializationContext.ServiceName));
            });

            // Set reference on the initialization context so the service starting up can pick it up
            InitializationContext.Bus = Bus.Instance;
        }

        #endregion
    }
}
