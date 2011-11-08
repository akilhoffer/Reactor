using System;
using Reactor.Resources;

namespace Reactor.Hosting
{
    internal class CoreServiceHost : ServiceHostFoundation
    {
        #region Overrides of ServiceHostFoundation

        public override string ServiceName
        {
            get { return Environment.MachineName; }
        }

        public override string ServiceDescription
        {
            get { return CommonResources.Core_ServiceDescription; }
        }

        #endregion
    }
}
