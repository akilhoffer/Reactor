namespace Reactor.Customization
{
    public interface ICustomizeReactorInitialization
    {
        /// <summary>
        /// Gets the execution order for this initializer. The lower the value, the earlier it will 
        /// be executed in a chain of initializers.
        /// </summary>
        /// <value>The execution order.</value>
        uint ExecutionOrder { get; }

        /// <summary>
        /// Initializes Reactor. Implementers can provide complete custom initialization of the Reactor Service, bypassing 
        /// all default initialization. Upon completion, validation of the Reactor Context is performed. If critical services are 
        /// not present on the Context, a fatal exception will cause the Reactor Service to terminate.
        /// </summary>
        void InitializeReactor();
    }
}
