namespace CrashBandicoot.Switch;

internal static class Program
{
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
                // mono-nx / hbmenu — CWD może być już ustawione (np. /switch)
            }

            var cwd = Directory.GetCurrentDirectory();

            Console.WriteLine("[Switch] Crash Bandicoot (RecompOne host stub)");
            Console.WriteLine($"[Switch] BaseDirectory: {AppContext.BaseDirectory}");
            Console.WriteLine($"[Switch] CWD: {cwd}");

            string? cuePath = null;
            if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
            {
                cuePath = args[0];
                Console.WriteLine($"[Switch] Cue z argv: {cuePath} exists={File.Exists(cuePath)}");
                if (!File.Exists(cuePath))
                    cuePath = null;
            }

            cuePath ??= FindCue(cwd);

            if (cuePath is null)
            {
                Console.WriteLine("[Switch] Nie znaleziono .cue w żadnej znanej ścieżce.");
                Console.WriteLine("[Switch] Kontynuacja smoke bez disc (dev/CI).");
            }
            else
            {
                Console.WriteLine($"[Switch] Znaleziono .cue: {cuePath}");
                // TODO: podaj cuePath do Runtime (CdPath / Entry.Run)
            }

            using var host = new SwitchPlatformHost();
            host.Initialize();

            // TODO: AppPaths, settings, prepare/recomp, Entry.Run(cuePath)
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

    /// <summary>
    /// Szuka legalnego dumpa NTSC-U w typowych lokalizacjach na SD.
    /// </summary>
    private static string? FindCue(string cwd)
    {
        string[] candidates =
        {
            Path.Combine(cwd, "crash", "Crash Bandicoot.cue"),
            Path.Combine(cwd, "Crash Bandicoot.cue"),
            Path.Combine(cwd, "crash", "crash.cue"),
            "/switch/crash/Crash Bandicoot.cue",
            "/switch/Crash Bandicoot.cue",
            "/crash/Crash Bandicoot.cue",
            "/switch/crash/CRASH.CUE",
            "/switch/crash/crash.cue",
        };

        foreach (var p in candidates)
        {
            bool exists = false;
            try
            {
                exists = File.Exists(p);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Switch] check error: {p} → {ex.Message}");
            }

            Console.WriteLine($"[Switch] check: {p} exists={exists}");
            if (exists)
                return p;
        }

        // Listing — w logu widać realne nazwy plików na karcie
        TryList(cwd);
        TryList(Path.Combine(cwd, "crash"));
        TryList("/switch");
        TryList("/switch/crash");
        TryList("/crash");
        TryList("/");

        return null;
    }

    private static void TryList(string dir)
    {
        try
        {
            if (!Directory.Exists(dir))
            {
                Console.WriteLine($"[Switch] list: {dir} — BRAK KATALOGU");
                return;
            }

            Console.WriteLine($"[Switch] list: {dir}");
            foreach (var d in Directory.GetDirectories(dir))
                Console.WriteLine($"  dir:  {d}");
            foreach (var f in Directory.GetFiles(dir))
                Console.WriteLine($"  file: {f}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Switch] list fail {dir}: {ex.Message}");
        }
    }
}
