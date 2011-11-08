using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using log4net;
using Microsoft.Win32;

namespace Reactor.Hosting
{
    public class ServiceInstaller
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ServiceInstaller));

        #region

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), DllImport("advapi32.dll")]
        private static extern IntPtr OpenSCManager(string lpMachineName, string lpSCDB, int scParameter);
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "8"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "7"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "12"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "11"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "10"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), DllImport("Advapi32.dll")]
        private static extern IntPtr CreateService(IntPtr SC_HANDLE, string lpSvcName, string lpDisplayName,
                                                int dwDesiredAccess, int dwServiceType, int dwStartType, int dwErrorControl, string lpPathName,
                                                string lpLoadOrderGroup, int lpdwTagId, string lpDependencies, string lpServiceStartName, string lpPassword);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "return"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), DllImport("advapi32.dll")]
        private static extern void CloseServiceHandle(IntPtr SCHANDLE);
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), DllImport("advapi32.dll")]
        private static extern int StartService(IntPtr SVHANDLE, int dwNumServiceArgs, string lpServiceArgVectors);
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA2101:SpecifyMarshalingForPInvokeStringArguments", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), DllImport("advapi32.dll", SetLastError = true)]
        private static extern IntPtr OpenService(IntPtr SCHANDLE, string lpSvcName, int dwNumServiceArgs);
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), DllImport("advapi32.dll")]
        private static extern int DeleteService(IntPtr SVHANDLE);
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), DllImport("kernel32.dll")]
        private static extern int GetLastError();
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass"), DllImport("Kernel32")]
        private extern static Boolean CloseHandle(IntPtr handle);

        #endregion DLLImport

        #region Constants declaration.
        const int SC_MANAGER_CREATE_SERVICE = 0x0002;
        const int SERVICE_WIN32_OWN_PROCESS = 0x00000010;
        const int SERVICE_DEMAND_START = 0x00000003;
        const int SERVICE_ERROR_NORMAL = 0x00000001;
        const int STANDARD_RIGHTS_REQUIRED = 0xF0000;
        const int SERVICE_QUERY_CONFIG = 0x0001;
        const int SERVICE_CHANGE_CONFIG = 0x0002;
        const int SERVICE_QUERY_STATUS = 0x0004;
        const int SERVICE_ENUMERATE_DEPENDENTS = 0x0008;
        const int SERVICE_START = 0x0010;
        const int SERVICE_STOP = 0x0020;
        const int SERVICE_PAUSE_CONTINUE = 0x0040;
        const int SERVICE_INTERROGATE = 0x0080;
        const int SERVICE_USER_DEFINED_CONTROL = 0x0100;
        const int SERVICE_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED |
                                            SERVICE_QUERY_CONFIG |
                                            SERVICE_CHANGE_CONFIG |
                                            SERVICE_QUERY_STATUS |
                                            SERVICE_ENUMERATE_DEPENDENTS |
                                            SERVICE_START |
                                            SERVICE_STOP |
                                            SERVICE_PAUSE_CONTINUE |
                                            SERVICE_INTERROGATE |
                                            SERVICE_USER_DEFINED_CONTROL);
        const int SERVICE_AUTO_START = 0x00000002;

        // Error codes
        const int ERROR_ACCESS_DENIED = 0x5;
        const int ERROR_INVALID_HANDLE = 6;
        const int ERROR_SERVICE_MARKED_FOR_DELETE = 0X430;

        #endregion Constants declaration.

        /// <summary>
        /// This method installs and runs the service in the service control manager.
        /// </summary>
        /// <param name="svcPath">The complete path of the service.</param>
        /// <param name="svcName">Name of the service.</param>
        /// <param name="svcDispName">Display name of the service.</param>
        /// <returns>True if the process went through successfully. False if there was any error.</returns> 
        public static bool InstallService(string svcPath, string svcName, string svcDispName, string description, bool autoStart, bool startNow)
        {
            int dwStartType = SERVICE_AUTO_START;
            if (autoStart == false) dwStartType = SERVICE_DEMAND_START;

            IntPtr sc_handle = OpenSCManager(null, null, SC_MANAGER_CREATE_SERVICE);
            if (sc_handle.ToInt32() == 0)
            {
                Log.Error("Unable to open Service Control Manager");
                return false;
            }

            IntPtr sv_handle = CreateService(sc_handle, svcName, svcDispName, SERVICE_ALL_ACCESS, SERVICE_WIN32_OWN_PROCESS, dwStartType, SERVICE_ERROR_NORMAL, svcPath, null, 0, null, null, null);
            if (sv_handle.ToInt32() == 0)
            {
                var lastError = GetLastError();
                Log.ErrorFormat("CreateService Win32 API call failed. GetLastError result: {0}", lastError);
                CloseServiceHandle(sc_handle);
                return false;
            }

            if (startNow)
            {
                //now trying to start the service
                int i = StartService(sv_handle, 0, null);
                // If the value i is zero, then there was an error starting the service.
                // note: error may arise if the service is already running or some other problem.
                if (i == 0)
                {
                    Log.Error("StartService Win32 API call failed.");
                    return false;
                }
            }

            try
            {
                // Write service description
                SetServiceDescription(svcName, description);
            }
            catch (Exception e)
            {
                Log.Error("Unable to set service description.", e);
            }

            CloseServiceHandle(sc_handle);
            CloseServiceHandle(sv_handle);
            return true;
        }

        /// <summary>
        /// This method uninstalls the service from the service conrol manager.
        /// </summary>
        /// <param name="svcName">Name of the service to uninstall.</param>
        public static bool UnInstallService(string svcName)
        {
            var GENERIC_WRITE = 0x40000000;
            var sc_hndl = OpenSCManager(null, null, GENERIC_WRITE);
            if (sc_hndl.ToInt32() == 0)
            {
                Log.Warn("Unable to obtain Service Control Manager handle from Win API.");
                return false;
            }

            var DELETE = 0x10000;
            var svcHndl = OpenService(sc_hndl, svcName, DELETE);
            if (Log.IsDebugEnabled) Log.DebugFormat("Service handle retrieved from Win API: {0}", svcHndl);

            if (svcHndl.ToInt32() != 0)
            {
                // Attept to delete service
                var i = DeleteService(svcHndl);
                CloseServiceHandle(sc_hndl);
                CloseServiceHandle(svcHndl);

                if (i == 0)
                {
                    var lastError = GetLastError();
                    if (Log.IsWarnEnabled) Log.WarnFormat("Unable to delete service. Return code from Win API: {0} ({1})", lastError, GetMessageFromSystemCode(lastError));
                    return false;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the specified <seealso cref="ServiceController"/> instance. If no service by the 
        /// specified name exists, this method will return null.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        /// <returns><seealso cref="ServiceController"/> instance if found. Otherwise, returns null.</returns>
        public static ServiceController GetServiceByName(string serviceName)
        {
            return ServiceController.GetServices().SingleOrDefault(s => s.ServiceName == serviceName);
        }

        private static string GetMessageFromSystemCode(int systemCode)
        {
            switch (systemCode)
            {
                case ERROR_ACCESS_DENIED:
                    return "Access denied";
                case ERROR_INVALID_HANDLE:
                    return "Invalid handle";
                case ERROR_SERVICE_MARKED_FOR_DELETE:
                    // See this article: http://support.microsoft.com/kb/287516
                    return "Service marked for delete";
            }

            return systemCode.ToString();
        }

        public static void SetServiceDescription(string serviceName, string description)
        {
            RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"System\CurrentControlSet\Services\" + serviceName, true);
            regKey.SetValue("Description", description);
            regKey.Close();
        }
    }
}
