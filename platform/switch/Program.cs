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

            Console.WriteLine("[Switch] Crash Bandicoot (RecompOne + platform host)");
            Console.WriteLine($"[Switch] BaseDirectory: {AppContext.BaseDirectory}");
            Console.WriteLine($"[Switch] CWD: {cwd}");

            try
            {
                AppPaths.SetRoot(cwd);
                AppPaths.EnsureCreated();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Switch] AppPaths: {ex.Message}");
            }

            string? cuePath = null;
            if (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]))
            {
                cuePath = args[0];
                if (!File.Exists(cuePath))
                    cuePath = null;
            }
            cuePath ??= FindCue(cwd);

            if (cuePath is null)
            {
                Console.WriteLine("[Switch] Brak .cue");
                return 1;
            }
            Console.WriteLine($"[Switch] Znaleziono .cue: {cuePath}");

            var gameDll = GameAssemblyLoader.FindGameDll(cwd);
            if (gameDll is null)
            {
                Console.WriteLine("[Switch] Brak game.recomp.dll");
                return 1;
            }

            var asm = GameAssemblyLoader.LoadGame(gameDll);
            GameAssemblyLoader.Inspect(asm);

            // === KLUCZ: zarejestruj host PRZED Entry.Run ===
            using var host = new SwitchPlatformHost();
            Runtime.SetPlatformHost(host);
            Console.WriteLine("[Switch] Runtime.SetPlatformHost(SwitchPlatformHost) OK");

            IMemory memory = new PSMemory();
            Console.WriteLine("[Switch] PSMemory created.");

            try
            {
                GameAssemblyLoader.InvokeEntryRun(asm, memory, cuePath);
                Console.WriteLine("[Switch] Entry.Run returned normally.");
            }
            catch (Exception ex)
            {
                var inner = ex is System.Reflection.TargetInvocationException tie && tie.InnerException != null
                    ? tie.InnerException
                    : ex;
                Console.WriteLine($"[Switch] Entry.Run FAILED: {inner}");
                Console.WriteLine(inner.StackTrace);
            }
            finally
            {
                try { Runtime.SetPlatformHost(null); } catch { /* ignore */ }
                try { Runtime.Shutdown(); } catch { /* ignore */ }
                host.Shutdown();
            }

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
            "/switch/crash/Crash Bandicoot.cue",
            "/crash/Crash Bandicoot.cue",
        };

        foreach (var p in candidates)
        {
            bool exists = false;
            try { exists = File.Exists(p); } catch { /* ignore */ }
            Console.WriteLine($"[Switch] check: {p} exists={exists}");
            if (exists) return p;
        }
        return null;
    }
}
