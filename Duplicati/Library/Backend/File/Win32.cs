using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Duplicati.Library.Backend
{
    internal class Win32
    {
        [DllImport("mpr.dll")]
        private static extern int WNetAddConnection2(ref NETRESOURCE netResource,
           string password, string username, int flags);

        private struct NETRESOURCE
        {
            public ResourceScope dwScope;
            public ResourceType dwType;
            public ResourceDisplayType dwDisplayType;
            public ResourceUsage dwUsage;
            public string LocalName;
            public string RemoteName;
            public string Comment;
            public string Provider;
        }

        private enum ResourceScope : int
        {
            RESOURCE_CONNECTED = 1,
            RESOURCE_GLOBALNET,
            RESOURCE_REMEMBERED,
            RESOURCE_RECENT,
            RESOURCE_CONTEXT
        };

        private enum ResourceType : int
        {
            RESOURCETYPE_ANY,
            RESOURCETYPE_DISK,
            RESOURCETYPE_PRINT,
            RESOURCETYPE_RESERVED
        };

        private enum ResourceDisplayType : int
        {
            RESOURCEDISPLAYTYPE_GENERIC,
            RESOURCEDISPLAYTYPE_DOMAIN,
            RESOURCEDISPLAYTYPE_SERVER,
            RESOURCEDISPLAYTYPE_SHARE,
            RESOURCEDISPLAYTYPE_FILE,
            RESOURCEDISPLAYTYPE_GROUP,
            RESOURCEDISPLAYTYPE_NETWORK,
            RESOURCEDISPLAYTYPE_ROOT,
            RESOURCEDISPLAYTYPE_SHAREADMIN,
            RESOURCEDISPLAYTYPE_DIRECTORY,
            RESOURCEDISPLAYTYPE_TREE,
            RESOURCEDISPLAYTYPE_NDSCONTAINER
        };

        private enum ResourceUsage : int
        {
            RESOURCEUSAGE_CONNECTABLE = 0x00000001,
            RESOURCEUSAGE_CONTAINER = 0x00000002,
            RESOURCEUSAGE_NOLOCALDEVICE = 0x00000004,
            RESOURCEUSAGE_SIBLING = 0x00000008,
            RESOURCEUSAGE_ATTACHED = 0x00000010,
            RESOURCEUSAGE_ALL = (RESOURCEUSAGE_CONNECTABLE | RESOURCEUSAGE_CONTAINER | RESOURCEUSAGE_ATTACHED),
        };

        private const int CONNECT_UPDATE_PROFILE = 0x1;

        internal static bool PreAuthenticate(string path, string username, string password)
        {
            //Strip it down from \\server\share\folder1\folder2\filename.extension to
            // \\server\share
            string minpath = path;
            if (!minpath.StartsWith("\\\\"))
                return false;

            int first = minpath.IndexOf("\\", 2);
            if (first <= 0)
                return false;
            int next = minpath.IndexOf("\\", first + 1);
            if (next >= 0)
                minpath = minpath.Substring(0, next);

            //This only works on Windows, and probably not on Win95

            try
            {
                NETRESOURCE rsc = new NETRESOURCE();

                rsc.dwScope = ResourceScope.RESOURCE_GLOBALNET;
                rsc.dwType = ResourceType.RESOURCETYPE_DISK;
                rsc.dwDisplayType = ResourceDisplayType.RESOURCEDISPLAYTYPE_SHARE;
                rsc.dwUsage = ResourceUsage.RESOURCEUSAGE_CONNECTABLE;
                rsc.LocalName = null;
                rsc.RemoteName = minpath;

                return WNetAddConnection2(ref rsc, password, username, CONNECT_UPDATE_PROFILE) == 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
