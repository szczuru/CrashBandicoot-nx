using System.Reflection;
using RecompOne.Runtime.Memory;

namespace CrashBandicoot.Switch;

public static class GameAssemblyLoader
{
    public static string? FindGameDll(string root)
    {
        var gameRoot = Path.Combine(root, "game");
        var candidates = new List<string>();

        void AddIfExists(string p)
        {
            try
            {
                if (File.Exists(p))
                    candidates.Add(p);
            }
            catch { /* ignore */ }
        }

        AddIfExists(Path.Combine(gameRoot, "game.recomp.dll"));
        AddIfExists(Path.Combine(root, "game.recomp.dll"));

        try
        {
            if (Directory.Exists(gameRoot))
            {
                foreach (var dir in Directory.GetDirectories(gameRoot))
                {
                    AddIfExists(Path.Combine(dir, "game.recomp.dll"));
                    foreach (var f in Directory.GetFiles(dir, "*.dll"))
                    {
                        var name = Path.GetFileName(f);
                        if (name.Contains("recomp", StringComparison.OrdinalIgnoreCase) ||
                            name.Equals("CrashBandicoot.Game.dll", StringComparison.OrdinalIgnoreCase))
                            candidates.Add(f);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Switch] FindGameDll scan error: {ex.Message}");
        }

        candidates = candidates.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var preferred = candidates.FirstOrDefault(c =>
            Path.GetFileName(c).Contains("recomp", StringComparison.OrdinalIgnoreCase));
        return preferred ?? candidates.FirstOrDefault();
    }

    public static void LogDependencyHints(string root, string gameDllPath)
    {
        var dirs = new[]
        {
            root,
            Path.GetDirectoryName(gameDllPath) ?? root,
            Path.Combine(root, "game"),
        };

        Console.WriteLine("[Switch] Dependency probe (RecompOne.Runtime.dll):");
        foreach (var dir in dirs.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var runtime = Path.Combine(dir, "RecompOne.Runtime.dll");
            var exists = false;
            try { exists = File.Exists(runtime); } catch { /* ignore */ }
            Console.WriteLine($"  {runtime} exists={exists}");
        }
    }

    public static Assembly LoadGame(string dllPath)
    {
        Console.WriteLine($"[Switch] Loading game assembly: {dllPath}");
        try
        {
            var cwd = Directory.GetCurrentDirectory();
            LogDependencyHints(cwd, dllPath);
        }
        catch { /* ignore */ }

        var asm = Assembly.LoadFrom(dllPath);
        Console.WriteLine($"[Switch] Assembly: {asm.FullName}");
        return asm;
    }

    public static void Inspect(Assembly asm)
    {
        Type[] types;
        try
        {
            types = asm.GetExportedTypes();
        }
        catch (ReflectionTypeLoadException rtle)
        {
            Console.WriteLine("[Switch] ReflectionTypeLoadException:");
            if (rtle.LoaderExceptions != null)
            {
                foreach (var le in rtle.LoaderExceptions)
                {
                    if (le != null)
                        Console.WriteLine($"  LOADER: {le.GetType().Name}: {le.Message}");
                }
            }
            types = rtle.Types?.Where(t => t != null).Cast<Type>().ToArray() ?? Array.Empty<Type>();
        }

        Type? entryType = null;
        foreach (var t in types)
        {
            if (t.Name is "Entry" or "Program")
            {
                Console.WriteLine($"[Switch] type: {t.FullName}");
                entryType ??= t;
            }
        }

        if (entryType is null)
        {
            Console.WriteLine("[Switch] Brak Entry/Program. Sample types:");
            foreach (var t in types.Take(30))
                Console.WriteLine($"  {t.FullName}");
            return;
        }

        foreach (var m in entryType.GetMethods(BindingFlags.Public | BindingFlags.Static))
        {
            var args = string.Join(", ", m.GetParameters().Select(p => p.ParameterType.Name));
            Console.WriteLine($"[Switch] method: {entryType.Name}.{m.Name}({args})");
        }

        Console.WriteLine("[Switch] Inspect OK.");
    }

    /// <summary>
    /// Wywołuje Recompiled.Entry.Run(IMemory, string cuePath).
    /// </summary>
    public static void InvokeEntryRun(Assembly asm, IMemory memory, string cuePath)
    {
        Type? entryType = null;
        try
        {
            entryType = asm.GetExportedTypes().FirstOrDefault(t => t.Name == "Entry");
        }
        catch (ReflectionTypeLoadException rtle)
        {
            entryType = rtle.Types?.FirstOrDefault(t => t != null && t.Name == "Entry");
        }

        if (entryType is null)
            throw new InvalidOperationException("Typ Recompiled.Entry nie znaleziony.");

        var run = entryType.GetMethod(
            "Run",
            BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types: new[] { typeof(IMemory), typeof(string) },
            modifiers: null);

        if (run is null)
            throw new InvalidOperationException("Entry.Run(IMemory, String) nie znalezione.");

        Console.WriteLine($"[Switch] Calling Entry.Run(memory, \"{cuePath}\") ...");
        run.Invoke(null, new object[] { memory, cuePath });
        Console.WriteLine("[Switch] Entry.Run returned.");
    }
}
