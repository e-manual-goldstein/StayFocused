using System;
using System.Runtime.InteropServices;
using Outlook = Microsoft.Office.Interop.Outlook;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayFocused.Activities
{
    public class OutlookHandler : IActivityHandler
    {
        public string GetActivityDescription(IntPtr hWnd)
        {
            Handle();
            return "";
        }

        public void Handle()
        {
            // Try to get a running instance of Outlook
            Outlook.Application outlookApp = null;
            try
            {
                outlookApp = CoreMarshal.GetActiveObject("Outlook.Application") as Outlook.Application;
            }
            catch (COMException)
            {
                Console.WriteLine("Outlook is not running or not registered as an active COM object.");
                return;
            }

            if (outlookApp != null)
            {
                // Get the currently active item in Outlook (e.g., the currently displayed email)
                Outlook.Explorer explorer = outlookApp.ActiveExplorer();
                if (explorer != null && explorer.Selection.Count > 0)
                {
                    Outlook.MailItem mailItem = explorer.Selection[1] as Outlook.MailItem;
                    if (mailItem != null)
                    {
                        // Retrieve the subject (title) of the email
                        string emailTitle = mailItem.Subject;
                        Console.WriteLine("Title of the currently displayed email: " + emailTitle);
                    }
                }

                // Release COM objects
                Marshal.ReleaseComObject(explorer);
                Marshal.ReleaseComObject(outlookApp);
            }
        }
    }
}