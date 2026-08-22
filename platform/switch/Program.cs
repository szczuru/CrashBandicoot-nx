namespace CrashBandicoot.Switch;

internal static class Program
{
    // Domyślna lokalizacja disc na SD (użytkownik dostarcza własny dump)
    private const string DefaultCueRelative = "switch/crash/Crash Bandicoot.cue";

    private static int Main(string[] args)
    {
        try
        {
            var root = AppContext.BaseDirectory;
            if (string.IsNullOrEmpty(root) || root == "/")
                root = "/";

            try
            {
                Directory.SetCurrentDirectory(root);
            }
            catch
            {
                // mono-nx / hbmenu — CWD może być read-only
            }

            var cuePath = args.Length > 0
                ? args[0]
                : Path.Combine(root, DefaultCueRelative);

            Console.WriteLine("[Switch] Crash Bandicoot (RecompOne host stub)");
            Console.WriteLine($"[Switch] CWD: {Directory.GetCurrentDirectory()}");
            Console.WriteLine($"[Switch] Cue: {cuePath}");

            if (!File.Exists(cuePath))
            {
                Console.WriteLine("[Switch] Brak pliku .cue — połóż legalny dump NTSC-U (SCUS-94900).");
                Console.WriteLine($"[Switch] Oczekiwana ścieżka: {cuePath}");
                // Nie wychodzimy z kodem 1 w CI bez disc — tylko log
                Console.WriteLine("[Switch] Kontynuacja smoke bez disc (dev/CI).");
            }
            else
            {
                Console.WriteLine("[Switch] Znaleziono .cue");
            }

            using var host = new SwitchPlatformHost();
            host.Initialize();

            // TODO: AppPaths, settings, prepare/recomp, Entry.Run(...)
            Console.WriteLine("[Switch] Host OK. Podłącz RecompOne.Runtime Entry gdy game/ będzie gotowe.");
            host.RunSmokeLoop(seconds: 2);

            host.Shutdown();
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Switch] FATAL: {ex}");
            return 1;
        }
    }
}
