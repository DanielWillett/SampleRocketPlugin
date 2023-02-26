using HarmonyLib;

namespace SampleRocketPlugin;

public sealed class SampleRocketPlugin : RocketPlugin<SampleRocketPluginConfiguration>
{
    public static SampleRocketPlugin Instance { get; private set; } = null!;
    protected override void Load()
    {
        Logger.Log($"Loading {Assembly.GetName().Name} v{Assembly.GetName().Version}.");
        Instance = this;
        Patching.PatchHost = new Harmony("com.plugin.sample");
    }

    protected override void Unload()
    {
        Logger.Log($"Unloading {Assembly.GetName().Name}.");
        Patching.PatchHost?.UnpatchAll(Patching.PatchHost.Id);
        Patching.PatchHost = null;
        Instance = null!;
    }

    public override TranslationList DefaultTranslations { get; } = Chat.ConvertDefaultTranslations(new TranslationList
    {
        /* Echo command */

        // {0} = text
        { "echo_response", "<#ffff99>You typed: <b>{0}</b>" },

        // {0} = text, {1} = profile real name
        { "echo_response_with_profile", "<#ffff99>{1} typed: <b>{0}</b>" }
    });
}