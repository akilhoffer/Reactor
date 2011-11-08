using System;
using System.Diagnostics;
using System.ServiceProcess;
using Reactor.Resources;

namespace Reactor.Extensions
{
    public static class ServiceControllerExtensions
    {
        public static bool TryStart(this ServiceController serviceController, TimeSpan timeoutPeriod, out string failureReason)
        {
            var stopwatch = new Stopwatch();

            failureReason = string.Empty;
            try
            {
                serviceController.Refresh();
                switch (serviceController.Status)
                {
                    case ServiceControllerStatus.ContinuePending:
                        return true;
                    case ServiceControllerStatus.Paused:
                    case ServiceControllerStatus.PausePending:
                        stopwatch.Start();
                        serviceController.Continue();
                        serviceController.WaitForStatus(ServiceControllerStatus.Running, timeoutPeriod);
                        stopwatch.Stop();

                        if (serviceController.Status != ServiceControllerStatus.Running)
                        {
                            failureReason = CommonResources.Error_ServiceStartTimedOut;
                            return false;
                        }
                        break;
                    case ServiceControllerStatus.Running:
                        return true;
                    case ServiceControllerStatus.StartPending:
                        stopwatch.Start();
                        serviceController.WaitForStatus(ServiceControllerStatus.Running, timeoutPeriod);
                        stopwatch.Stop();

                        if (serviceController.Status != ServiceControllerStatus.Running)
                        {
                            failureReason = CommonResources.Error_ServiceStartTimedOut;
                            return false;
                        }
                        break;
                    case ServiceControllerStatus.Stopped:
                    case ServiceControllerStatus.StopPending:
                        stopwatch.Start();
                        serviceController.Start();
                        serviceController.WaitForStatus(ServiceControllerStatus.Running, timeoutPeriod);
                        stopwatch.Stop();

                        if (serviceController.Status != ServiceControllerStatus.Running)
                        {
                            failureReason = CommonResources.Error_ServiceStartTimedOut;
                            return false;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                failureReason = e.Message;
            }

            return true;
        }

        public static bool TryStop(this ServiceController serviceController, TimeSpan timeoutPeriod, out string failureReason)
        {
            var stopwatch = new Stopwatch();

            failureReason = string.Empty;
            try
            {
                serviceController.Refresh();
                switch (serviceController.Status)
                {
                    case ServiceControllerStatus.ContinuePending:
                    case ServiceControllerStatus.Paused:
                    case ServiceControllerStatus.PausePending:
                    case ServiceControllerStatus.Running:
                    case ServiceControllerStatus.StartPending:
                        stopwatch.Start();
                        serviceController.Stop();
                        serviceController.WaitForStatus(ServiceControllerStatus.Stopped, timeoutPeriod);
                        stopwatch.Stop();

                        if (serviceController.Status != ServiceControllerStatus.Stopped)
                        {
                            failureReason = CommonResources.Error_ServiceStopTimedOut;
                            return false;
                        }
                        break;
                    case ServiceControllerStatus.Stopped:
                        return true;
                    case ServiceControllerStatus.StopPending:
                        stopwatch.Start();
                        serviceController.WaitForStatus(ServiceControllerStatus.Stopped, timeoutPeriod);
                        stopwatch.Stop();

                        if (serviceController.Status != ServiceControllerStatus.Stopped)
                        {
                            failureReason = CommonResources.Error_ServiceStopTimedOut;
                            return false;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                failureReason = e.Message;
            }

            return true;
        }

        public static bool TryRestart(this ServiceController serviceController, TimeSpan timeoutPeriod, out string failureReason)
        {
            var stopwatch = new Stopwatch();

            try
            {
                serviceController.Refresh();
                switch (serviceController.Status)
                {
                    case ServiceControllerStatus.ContinuePending:
                    case ServiceControllerStatus.Paused:
                    case ServiceControllerStatus.PausePending:
                    case ServiceControllerStatus.Running:
                    case ServiceControllerStatus.StartPending:
                        stopwatch.Start();
                        serviceController.Stop();
                        serviceController.WaitForStatus(ServiceControllerStatus.Stopped, timeoutPeriod);
                        stopwatch.Stop();

                        if (serviceController.Status != ServiceControllerStatus.Stopped)
                        {
                            failureReason = CommonResources.Error_ServiceStopTimedOut;
                            return false;
                        }

                        return TryStart(serviceController, timeoutPeriod, out failureReason);
                    case ServiceControllerStatus.Stopped:
                    case ServiceControllerStatus.StopPending:
                        return TryStart(serviceController, timeoutPeriod, out failureReason);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                failureReason = e.Message;
            }

            return true;
        }
    }
}
