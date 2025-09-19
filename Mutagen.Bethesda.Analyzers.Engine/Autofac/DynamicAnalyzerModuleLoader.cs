using System.Reflection;
using Autofac;
using Mutagen.Bethesda.Analyzers.SDK;
using Module = Autofac.Module;

namespace Mutagen.Bethesda.Analyzers.Autofac;

/// <summary>
/// Service responsible for dynamically loading game-specific analyzer modules at runtime
/// </summary>
internal static class DynamicAnalyzerModuleLoader
{
    /// <summary>
    /// Loads and registers the appropriate analyzer module for the specified game release
    /// </summary>
    /// <param name="builder">The container builder to register the module with</param>
    /// <param name="gameRelease">The game release to load analyzers for</param>
    /// <exception cref="InvalidOperationException">Thrown when the required analyzer assembly or module cannot be found</exception>
    public static void LoadAnalyzerModule(ContainerBuilder builder, GameRelease gameRelease)
    {
        var assemblyName = $"Mutagen.Bethesda.Analyzers.{gameRelease.ToCategory()}";

        // Try to load the assembly
        Assembly? assembly;
        try
        {
            assembly = Assembly.Load(assemblyName);
        }
        catch (FileNotFoundException)
        {
            throw new InvalidOperationException(
                $"Required analyzer assembly '{assemblyName}' not found. " +
                $"Make sure the {assemblyName} package is referenced in your project.");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to load analyzer assembly '{assemblyName}': {ex.Message}", ex);
        }

        // Find the IAnalyzerModule implementation
        var moduleTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IAnalyzerModule).IsAssignableFrom(t))
            .ToArray();

        if (moduleTypes.Length == 0)
        {
            throw new InvalidOperationException(
                $"No IAnalyzerModule implementation found in assembly '{assemblyName}'. " +
                $"The assembly must contain exactly one class implementing IAnalyzerModule.");
        }

        if (moduleTypes.Length > 1)
        {
            throw new InvalidOperationException(
                $"Multiple IAnalyzerModule implementations found in assembly '{assemblyName}': " +
                $"{string.Join(", ", moduleTypes.Select(t => t.Name))}. " +
                $"The assembly must contain exactly one class implementing IAnalyzerModule.");
        }

        var moduleType = moduleTypes[0];

        Module? module;
        try
        {
            module = Activator.CreateInstance(moduleType) as Module;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to create instance of {moduleType.Name}: {ex.Message}", ex);
        }

        if (module == null)
        {
            throw new InvalidOperationException(
                $"Failed to create instance of {moduleType.Name}. " +
                $"Ensure the class has a parameterless constructor.");
        }

        builder.RegisterModule(module);
    }
}
