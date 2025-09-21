using Mafi;
using Mafi.Core.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CoI.ModToolsAutogen;

[GlobalDependency(RegistrationMode.AsSelf)]
internal class ConsoleCommands
{
    private readonly IResolver m_resolver;

    public ConsoleCommands(IResolver resolver)
    {
        m_resolver = resolver;
    }

    [ConsoleCommand(true, false, null, null)]
    internal GameCommandResult generateLayoutEntityIcons(string? idSubstring = null, int pitchDegrees = 35, int yawDegrees = 120, int fovDegrees = 20)
    {
        try {
            var gen = m_resolver.Instantiate<IconGenerator>();
            int num = gen.GenerateIcons(idSubstring, "GeneratedIconAssets", pitchDegrees.Degrees(), yawDegrees.Degrees(), fovDegrees.Degrees());

            return GameCommandResult.Success(string.Format("Generated {0} icons, look in '{1}' directory.", num, System.IO.Path.Combine(Environment.CurrentDirectory, "GeneratedIconAssets")), false);
        } catch (Exception ex) {
            Log.Exception(ex, "Failed to generate entity icon.");
            return GameCommandResult.Error($"Failed: {ex}", false);
        }
    }

    [ConsoleCommand(true, false, null, null)]
    internal GameCommandResult generateLayoutEntityAnimationTexture(string? idSubstring = null)
    {
        try {
            // Instantiate an object of type 'Mafi.Unity.TexturesGenerators.AnimationTexturesGenerator'.
            var type = GetTypeFromAssembly("Mafi.Unity", "Mafi.Unity.TexturesGenerators.AnimationTexturesGenerator");
            var generator = ((DependencyResolver)m_resolver).Instantiate(type);

            // Set properties of generator.
            type.GetProperty("NameSubstr").SetValue(generator, Option.Create(idSubstring));

            // Run generator.
            string basePath = System.IO.Path.Combine(Environment.CurrentDirectory, "GeneratedAnimationTextures");
            var genAsync = (IEnumerable<string>)type.GetMethod("GenerateLayoutEntities").Invoke(generator, [basePath]);
            var outs = genAsync.Where(x => !string.IsNullOrEmpty(x)).ToList(); // Run async method.

            return GameCommandResult.Success(string.Format("Generated {0} animation textures, look in '{1}' directory.", outs.Count, basePath), false);
        } catch (Exception ex) {
            return GameCommandResult.Error($"Failed: {ex}", false);
        }
    }

    private static Type GetTypeFromAssembly(string assembly, string type)
    {
        var a = AppDomain.CurrentDomain.GetAssemblies().Where(x => new AssemblyName(x.FullName).Name == assembly).FirstOrDefault();
        return a?.GetTypes().Where(x => x.FullName == type).FirstOrDefault() ?? throw new TypeLoadException($"Type {type} not found");
    }
}
