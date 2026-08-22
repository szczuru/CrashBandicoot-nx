using RecompOne.Runtime;
using RecompOne.Runtime.Memory;

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

            try { Directory.SetCurrentDirectory(root); }
            catch { /* mono-nx */ }

            var cwd = Directory.GetCurrentDirectory();

            Console.WriteLine("[Switch] Crash Bandicoot (RecompOne host)");
            Console.WriteLine($"[Switch] BaseDirectory: {AppContext.BaseDirectory}");
            Console.WriteLine($"[Switch] CWD: {cwd}");

            // Writable root (save/game/logs obok DLL)
            try
            {
                AppPaths.SetRoot(cwd);
                AppPaths.EnsureCreated();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Switch] AppPaths: {ex.Message}");
                try
                {
                    Directory.CreateDirectory(Path.Combine(cwd, "save"));
                    Directory.CreateDirectory(Path.Combine(cwd, "game"));
                    Directory.CreateDirectory(Path.Combine(cwd, "logs"));
                }
                catch { /* ignore */ }
            }

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
                Console.WriteLine("[Switch] Brak .cue — nie da się wywołać Entry.Run.");
                return 1;
            }
            Console.WriteLine($"[Switch] Znaleziono .cue: {cuePath}");

            var runtimeBesideHost = Path.Combine(cwd, "RecompOne.Runtime.dll");
            Console.WriteLine($"[Switch] RecompOne.Runtime.dll @ host: exists={File.Exists(runtimeBesideHost)} path={runtimeBesideHost}");

            var gameDll = GameAssemblyLoader.FindGameDll(cwd);
            if (gameDll is null)
            {
                Console.WriteLine("[Switch] Brak game.recomp.dll.");
                TryListGame(cwd);
                return 1;
            }

            var asm = GameAssemblyLoader.LoadGame(gameDll);
            GameAssemblyLoader.Inspect(asm);

            using var host = new SwitchPlatformHost();
            host.Initialize();

            // PS1 memory — jak na desktopie
            IMemory memory = new PSMemory();
            Console.WriteLine("[Switch] PSMemory created.");

            try
            {
                GameAssemblyLoader.InvokeEntryRun(asm, memory, cuePath);
            }
            catch (Exception ex)
            {
                // TargetInvocationException owija właściwy błąd
                var inner = ex is System.Reflection.TargetInvocationException tie && tie.InnerException != null
                    ? tie.InnerException
                    : ex;
                Console.WriteLine($"[Switch] Entry.Run FAILED: {inner}");
                Console.WriteLine(inner.StackTrace);
            }

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
        };

        foreach (var p in candidates)
        {
            bool exists = false;
            try { exists = File.Exists(p); }
            catch (Exception ex) { Console.WriteLine($"[Switch] check error: {p} → {ex.Message}"); }
            Console.WriteLine($"[Switch] check: {p} exists={exists}");
            if (exists) return p;
        }
        return null;
    }

    private static void TryListGame(string root)
    {
        TryList(Path.Combine(root, "game"));
        var game = Path.Combine(root, "game");
        if (!Directory.Exists(game)) return;
        try
        {
            foreach (var d in Directory.GetDirectories(game))
                TryList(d);
        }
        catch (Exception ex) { Console.WriteLine($"[Switch] TryListGame: {ex.Message}"); }
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
