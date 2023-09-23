using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace StayFocused.Activities.Handlers
{
    internal class EdgeActivityHandler : IActivityHandler
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("oleacc.dll")]
        private static extern int AccessibleObjectFromWindow(IntPtr hwnd, uint dwObjectID, byte[] riid, out IAccessible ppvObject);

        [DllImport("oleacc.dll")]
        private static extern int ObjectFromLresult(IntPtr lResult, ref Guid riid, uint wParam, out IAccessible ppvObject);
       
        [DllImport("ole32.dll")]
        private static extern int CoGetObject(string pszName, IntPtr pBindOptions, ref Guid riid, out IntPtr ppv);

        private static Guid IID_IAccessible = new Guid("{618736E0-3C3D-11CF-810C-00AA00389B71}");
       
        public string GetActivityDescription(IntPtr hWnd)
        {
            Guid IID_IUnknown = new Guid("00000000-0000-0000-C000-000000000046");

            uint processId;
            WinApi.GetWindowThreadProcessId(hWnd, out processId);
            Type shellType = Type.GetTypeFromProgID("Shell.Application");
            dynamic shell = Activator.CreateInstance(shellType);

            // Get Windows of type "Internet Explorer"
            var windows = shell.Windows();
            foreach (dynamic window in windows)
            {
                if (window.FullName.ToLower().Contains("msedge"))
                {
                    Console.WriteLine($"Found Edge window: {window.LocationURL}");
                }
            }

            string monikerName = "InternetExplorer.Application";

            IntPtr pUnk;
            int hr = CoGetObject(monikerName, IntPtr.Zero, ref IID_IUnknown, out pUnk);

            if (hr == 0)
            {
                // You have a pointer to the running object (pUnk)
                // Now you can interact with the object as needed

                // Don't forget to release the COM object when you're done
                Marshal.Release(pUnk);
            }
            else
            {
                Console.WriteLine("CoGetObject failed with HRESULT: " + hr);
            }

            //CoreWebView2 webView = new CoreWebView2(environment);

            throw new NotImplementedException();
        }

        private bool TryGetEdgeUrl(uint processId, out string url)
        {
            IAccessible accessible = null;
            url = null;

            // Find the IAccessible interface from the Edge process window
            AccessibleObjectFromWindow(GetForegroundWindow(), 0xFFFFFFF0, IID_IAccessible.ToByteArray(), out accessible);

            // Retrieve the URL using IAccessible (specific implementation details may vary)
            if (accessible != null)
            {
                url = RetrieveUrlFromAccessible(accessible);
                Marshal.ReleaseComObject(accessible);
            }

            return url != null;
        }

        private static string RetrieveUrlFromAccessible(IAccessible accessible)
        {
            // Implement the logic to retrieve the URL from the IAccessible object
            // This may require traversing the accessible tree and inspecting accessible properties
            // The specific details will depend on Edge's COM accessibility implementation
            return null;
        }

        public IActivity GetBasicActivity(string description)
        {
            return new Activity() { ProcessName = description };
        }


    }

    [ComImport, Guid("618736E0-3C3D-11CF-810C-00AA00389B71")]
    internal interface IAccessible
    {
    }
}
