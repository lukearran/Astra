using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Astra.Gtk.Functions;

public static class SystemFunction
{
    public static void TryOpenUrlInBrowser(string url)
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
        }
        catch
        {
            return;
        }
    }
}