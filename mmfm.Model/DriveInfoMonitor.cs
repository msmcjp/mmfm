using System;
using System.Runtime.InteropServices;

namespace Mmfm.Model
{
    public class DriveInfoMonitor
    {
        private enum DBT
        {
            DBT_DEVICEARRIVAL = 0x8000,
            DBT_DEVICEQUERYREMOVE = 0x8001,
            DBT_DEVICEQUERYREMOVEFAILED = 0x8002,
            DBT_DEVICEREMOVEPENDING = 0x8003,
            DBT_DEVICEREMOVECOMPLETE = 0x8004,
        }

        private struct DEV_BROADCAST_VOLUME
        {
            public uint dbcv_size;
            public DBT_DEVTP dbcv_devicetype;
            public uint dbcv_reserved;
            public uint dbcv_unitmask;
        }

        private enum DBT_DEVTP
        {
            DBT_DEVTYP_OEM = 0x0000,
            DBT_DEVTYP_DEVNODE = 0x0001,
            DBT_DEVTYP_VOLUME = 0x0002,
            DBT_DEVTYP_PORT = 0x0003,
            DBT_DEVTYP_NET = 0x0004,
            DBT_DEVTYP_DEVICEINTERFACE = 0x0005,
            DBT_DEVTYP_HANDLE = 0x0006,
        }

        // TODO: public HwndSourceHook WndProc => MessageHook;

        public EventHandler DrivesChanged;

        private IntPtr MessageHook(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handle)
        {
            switch (msg)
            {
                case 0x219:
                    OnDeviceChanged(
                        (DBT)wParam.ToInt32(), 
                        lParam != IntPtr.Zero ? (DEV_BROADCAST_VOLUME)Marshal.PtrToStructure(lParam, typeof(DEV_BROADCAST_VOLUME)) : new DEV_BROADCAST_VOLUME()
                    );
                    break;
                default:
                    break;
            }

            return IntPtr.Zero;
        }

        private void OnDeviceChanged(DBT dbt, DEV_BROADCAST_VOLUME volume)
        {
            if(volume.dbcv_devicetype != DBT_DEVTP.DBT_DEVTYP_VOLUME)
            {
                return;
            }

            switch(dbt)
            {
                case DBT.DBT_DEVICEARRIVAL:
                case DBT.DBT_DEVICEREMOVECOMPLETE:
                    DrivesChanged?.Invoke(this, EventArgs.Empty);
                    break;
                default:
                    break;
            }
        }
    }
}
