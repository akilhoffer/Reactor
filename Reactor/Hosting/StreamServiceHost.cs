using System.Configuration;

namespace Reactor.Hosting
{
    internal class StreamServiceHost : ServiceHostFoundation
    {
        #region Fields

        private string _serviceName;
        private string _serviceDescription;

        #endregion

        #region Overrides of ServiceHostFoundation

        public override string ServiceName
        {
            get
            {
                if (string.IsNullOrEmpty(_serviceName))
                {
                    // Obtain service name from configuration if possible, otherwise default it
                    _serviceName = ConfigurationManager.AppSettings["ServiceName"];
                    if (string.IsNullOrEmpty(_serviceName))
                        _serviceName = DetermineDefaultServiceName();
                }

                return _serviceName;
            }
        }

        public override string ServiceDescription
        {
            get
            {
                if (string.IsNullOrEmpty(_serviceDescription))
                {
                    // Obtain service description from configuration if possible, otherwise default it
                    _serviceDescription = ConfigurationManager.AppSettings["ServiceDescription"];
                    if (string.IsNullOrEmpty(_serviceDescription))
                        _serviceDescription = "Reactor Stream Service";
                }

                return _serviceDescription;
            }
        }

        #endregion
    }
}
