using RecompOne.Runtime;
using RecompOne.Runtime.Memory;

namespace CrashBandicoot.Switch;

internal static class Program
{
    private static readonly string[] LogPaths =
    {
        "sdmc:/switch/aot_crash_log.txt",
        "/switch/aot_crash_log.txt",
        "/aot_crash_log.txt",
        "aot_crash_log.txt",
    };

    private static void BootLog(string msg)
    {
        try { Console.WriteLine(msg); } catch { /* ignore */ }

        foreach (var path in LogPaths)
        {
            try
            {
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && dir != "/" && !dir.StartsWith("sdmc:", StringComparison.Ordinal))
                {
                    try { Directory.CreateDirectory(dir); } catch { /* ignore */ }
                }
                File.AppendAllText(path, msg + Environment.NewLine);
                return;
            }
            catch
            {
                // try next path
            }
        }
    }

    private static int Main(string[] args)
    {
        BootLog("[AOT] Main start");
        try
        {
            var root = AppContext.BaseDirectory;
            if (string.IsNullOrEmpty(root) || root == "/")
                root = "/";

            try { Directory.SetCurrentDirectory(root); }
            catch { /* mono-nx */ }

            var cwd = Directory.GetCurrentDirectory();

            BootLog("[Switch] Crash Bandicoot (RecompOne + platform host)");
            BootLog($"[Switch] BaseDirectory: {AppContext.BaseDirectory}");
            BootLog($"[Switch] CWD: {cwd}");

            try
            {
                AppPaths.SetRoot(cwd);
                AppPaths.EnsureCreated();
                BootLog("[Switch] AppPaths OK");
            }
            catch (Exception ex)
            {
                BootLog($"[Switch] AppPaths: {ex}");
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
                BootLog("[Switch] Brak .cue — exit 1");
                return 1;
            }
            BootLog($"[Switch] Znaleziono .cue: {cuePath}");

            var gameDll = GameAssemblyLoader.FindGameDll(cwd);
            if (gameDll is null)
            {
                BootLog("[Switch] Brak game.recomp.dll — exit 1");
                return 1;
            }
            BootLog($"[Switch] game dll: {gameDll}");

            BootLog("[Switch] LoadGame...");
            var asm = GameAssemblyLoader.LoadGame(gameDll);
            GameAssemblyLoader.Inspect(asm);
            BootLog("[Switch] Inspect OK");

            using var host = new SwitchPlatformHost();
            Runtime.SetPlatformHost(host);
            BootLog("[Switch] Runtime.SetPlatformHost(SwitchPlatformHost) OK");

            BootLog("[Switch] Creating PSMemory...");
            IMemory memory = new PSMemory();
            BootLog("[Switch] PSMemory created.");

            try
            {
                BootLog($"[Switch] Entry.Run(..., \"{cuePath}\") ...");
                GameAssemblyLoader.InvokeEntryRun(asm, memory, cuePath);
                BootLog("[Switch] Entry.Run returned normally.");
            }
            catch (Exception ex)
            {
                var inner = ex is System.Reflection.TargetInvocationException tie && tie.InnerException != null
                    ? tie.InnerException
                    : ex;
                BootLog($"[Switch] Entry.Run FAILED: {inner}");
                BootLog(inner.StackTrace ?? "(no stack)");
            }
            finally
            {
                try { Runtime.SetPlatformHost(null); } catch { /* ignore */ }
                try { Runtime.Shutdown(); } catch { /* ignore */ }
                try { host.Shutdown(); } catch { /* ignore */ }
                BootLog("[Switch] Shutdown done");
            }

            BootLog("[AOT] Main end OK");
            return 0;
        }
        catch (Exception ex)
        {
            BootLog($"[Switch] FATAL: {ex}");
            BootLog(ex.StackTrace ?? "(no stack)");
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
            "sdmc:/switch/crash/Crash Bandicoot.cue",
            "/crash/Crash Bandicoot.cue",
        };

        foreach (var p in candidates)
        {
            bool exists = false;
            try { exists = File.Exists(p); } catch { /* ignore */ }
            BootLog($"[Switch] check: {p} exists={exists}");
            if (exists) return p;
        }
        return null;
    }
}
