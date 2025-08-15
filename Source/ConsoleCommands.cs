using Mafi;
using Mafi.Core.Console;
using System;

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
}
