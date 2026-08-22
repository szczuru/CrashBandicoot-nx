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
                // mono-nx
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
                Console.WriteLine("[Switch] Kontynuacja bez disc (dev/CI).");
            }
            else
            {
                Console.WriteLine($"[Switch] Znaleziono .cue: {cuePath}");
            }

            var dataRoot = cwd;
            try
            {
                Directory.CreateDirectory(Path.Combine(dataRoot, "save"));
                Directory.CreateDirectory(Path.Combine(dataRoot, "game"));
                Directory.CreateDirectory(Path.Combine(dataRoot, "logs"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Switch] dirs: {ex.Message}");
            }

            // Probe Runtime obok hosta
            var runtimeBesideHost = Path.Combine(dataRoot, "RecompOne.Runtime.dll");
            Console.WriteLine($"[Switch] RecompOne.Runtime.dll @ host: exists={File.Exists(runtimeBesideHost)} path={runtimeBesideHost}");

            var gameDll = GameAssemblyLoader.FindGameDll(dataRoot);
            if (gameDll is null)
            {
                Console.WriteLine("[Switch] Brak game.recomp.dll pod /switch/game/...");
                Console.WriteLine("[Switch] Skopiuj z PC folder game/<fingerprint>/ po prepare.");
                TryListGame(dataRoot);
            }
            else
            {
                try
                {
                    GameAssemblyLoader.Inspect(gameDll);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Switch] Load/Inspect game DLL failed: {ex}");
                }
            }

            using var host = new SwitchPlatformHost();
            host.Initialize();

            Console.WriteLine("[Switch] Host OK (smoke). Entry.Run po udanym Inspect + Runtime.");
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

        TryList(cwd);
        TryList(Path.Combine(cwd, "crash"));
        TryList("/switch");
        TryList("/switch/crash");
        TryList("/crash");
        TryList("/");

        return null;
    }

    private static void TryListGame(string root)
    {
        TryList(Path.Combine(root, "game"));
        var game = Path.Combine(root, "game");
        if (!Directory.Exists(game))
            return;
        try
        {
            foreach (var d in Directory.GetDirectories(game))
                TryList(d);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Switch] TryListGame: {ex.Message}");
        }
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
