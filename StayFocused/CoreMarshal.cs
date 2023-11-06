using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace StayFocused
{

    public static class CoreMarshal
    {
        [DllImport("ole32.dll")]
        private static extern int GetRunningObjectTable(int reserved, out IRunningObjectTable pprot);

        [DllImport("ole32.dll")]
        private static extern int CreateBindCtx(int reserved, out IBindCtx pctx);

        public static object GetActiveObject(string progId)
        {
            IRunningObjectTable runningObjectTable;
            IEnumMoniker enumMoniker;

            if (GetRunningObjectTable(0, out runningObjectTable) == 0)
            {
                runningObjectTable.EnumRunning(out enumMoniker);

                IMoniker[] moniker = new IMoniker[1];
                IntPtr fetched = IntPtr.Zero;

                while (enumMoniker.Next(1, moniker, fetched) == 0)
                {
                    IBindCtx bindCtx;
                    CreateBindCtx(0, out bindCtx);

                    string displayName;
                    moniker[0].GetDisplayName(bindCtx, null, out displayName);

                    if (displayName.StartsWith("!" + progId))
                    {
                        object comObject;
                        runningObjectTable.GetObject(moniker[0], out comObject);
                        Marshal.ReleaseComObject(bindCtx);

                        return comObject;
                    }

                    Marshal.ReleaseComObject(bindCtx);
                    Marshal.ReleaseComObject(moniker[0]);
                }
            }

            return null;
        }
    }

}
