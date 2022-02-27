using System.Linq;

namespace ModManager.Helper;

public static class Process
{
    public static bool IsMonBazouRunning() => System.Diagnostics.Process.GetProcessesByName("Mon Bazou").Any();
}