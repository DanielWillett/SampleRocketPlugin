/*
 * This template is originally from
 * https://github.com/DanielWillett/SampleRocketPlugin
 */

using System.Globalization;

namespace SampleRocketPlugin.Util;

/// <summary>
/// High-performance chat message formatting.
/// </summary>
internal static class Chat
{
    /*
     * Will be replaced by angle brackets during translation for rich text support.
     * Double up these to escape them. ('[[' will translate to '[' instead of '<<')
     * If you need two angle brackets put a backslash in front of the double placeholders ('\[[' = '<<')
     *
     * I decided to not use RocketMod's wrapping for this because of the issues it causes with rich text.
     * Feel free to replace the vanilla ChatManager.serverSendMessage with UnturnedChat.Say if wrapping is needed.
     */
    private const char AngleBracketPlaceholderLeft  = '[';
    private const char AngleBracketPlaceholderRight = ']';
    private const string NullPlaceholder = "No Value";
    private const string DefaultDecimalFormatting = "0.###";
    private const int MaxStringLength = 1 << 11; // max string length of net invokers compensating for player name extensions
    internal static string? DefaultIconUrl { get; set; }
    internal static IFormatProvider FormatProvider { get; set; } = CultureInfo.CurrentCulture;
    internal static void SendChat(this IRocketPlayer player, string key, Color background = default, string? iconUrlOverride = null, params object[] parameters)
    {
        ThreadUtil.assertIsGameThread();

        if (player is null)
            throw new ArgumentNullException(nameof(player));
        if (key is null)
            throw new ArgumentNullException(nameof(key));

        SteamPlayer? target = null;
        if (player is UnturnedPlayer pl)
            target = pl.Player.channel.owner;
        else if (ulong.TryParse(player.Id, NumberStyles.Number, CultureInfo.InvariantCulture, out ulong s64))
        {
            if (s64.IsValidPlayerId())
                target = PlayerTool.getSteamPlayer(s64);
            else if (s64 != 0ul)
            {
                throw new ArgumentException(
                    "Invalid player: " + player.DisplayName + " {" + s64.ToString("G17", CultureInfo.InvariantCulture) + "}.",
                    nameof(player));
            }
        }
        else if (!string.IsNullOrEmpty(player.Id) && !player.Id.Equals("Console", StringComparison.Ordinal))
            throw new ArgumentException("Invalid player: " + player.DisplayName + " {" + player.Id + "}.", nameof(player));

        if (target != null && !target.player.isActiveAndEnabled)
            throw new ArgumentException("Player is offline.", nameof(player));

        PreSendChat(ref key, ref background, ref iconUrlOverride, parameters);

        if (target != null)
            ChatManager.serverSendMessage(key, background, null, target, EChatMode.SAY, iconUrlOverride, true);
        else
            Logger.Log(key);
    }
    internal static void SendChat(this IRocketPlayer player, string key, Color background = default, string? iconUrlOverride = null)
    {
        player.SendChat(key, background, iconUrlOverride, Array.Empty<byte>());
    }
    internal static void SendChat(this IRocketPlayer player, string key, object arg1, Color background = default, string? iconUrlOverride = null)
    {
        ThreadUtil.assertIsGameThread();

        object[] b = ParamBuffers[0];
        b[0] = arg1;
        player.SendChat(key, background, iconUrlOverride, b);
    }
    internal static void SendChat(this IRocketPlayer player, string key, object arg1, object arg2, Color background = default, string? iconUrlOverride = null)
    {
        ThreadUtil.assertIsGameThread();

        object[] b = ParamBuffers[1];
        b[0] = arg1;
        b[1] = arg2;
        player.SendChat(key, background, iconUrlOverride, b);
    }
    internal static void SendChat(this IRocketPlayer player, string key, object arg1, object arg2, object arg3, Color background = default, string? iconUrlOverride = null)
    {
        ThreadUtil.assertIsGameThread();

        object[] b = ParamBuffers[2];
        b[0] = arg1;
        b[1] = arg2;
        b[2] = arg3;
        player.SendChat(key, background, iconUrlOverride, b);
    }
    internal static void SendChat(this IRocketPlayer player, string key, object arg1, object arg2, object arg3, object arg4, Color background = default, string? iconUrlOverride = null)
    {
        ThreadUtil.assertIsGameThread();

        object[] b = ParamBuffers[3];
        b[0] = arg1;
        b[1] = arg2;
        b[2] = arg3;
        b[3] = arg4;
        player.SendChat(key, background, iconUrlOverride, b);
    }
    internal static void SendChat(this SteamPlayer player, string key, Color background = default, string? iconUrlOverride = null, params object[] parameters)
    {
        ThreadUtil.assertIsGameThread();

        if (player is null)
            throw new ArgumentNullException(nameof(player));
        if (key is null)
            throw new ArgumentNullException(nameof(key));
        if (!player.player.isActiveAndEnabled)
            throw new ArgumentException("Player is offline.", nameof(player));

        PreSendChat(ref key, ref background, ref iconUrlOverride, parameters);

        ChatManager.serverSendMessage(key, background, null, player, EChatMode.SAY, iconUrlOverride, true);
    }
    internal static void SendChat(this SteamPlayer player, string key, Color background = default, string? iconUrlOverride = null)
    {
        player.SendChat(key, background, iconUrlOverride, Array.Empty<byte>());
    }
    internal static void SendChat(this SteamPlayer player, string key, object arg1, Color background = default, string? iconUrlOverride = null)
    {
        ThreadUtil.assertIsGameThread();

        object[] b = ParamBuffers[0];
        b[0] = arg1;
        player.SendChat(key, background, iconUrlOverride, b);
    }
    internal static void SendChat(this SteamPlayer player, string key, object arg1, object arg2, Color background = default, string? iconUrlOverride = null)
    {
        ThreadUtil.assertIsGameThread();

        object[] b = ParamBuffers[1];
        b[0] = arg1;
        b[1] = arg2;
        player.SendChat(key, background, iconUrlOverride, b);
    }
    internal static void SendChat(this SteamPlayer player, string key, object arg1, object arg2, object arg3, Color background = default, string? iconUrlOverride = null)
    {
        ThreadUtil.assertIsGameThread();

        object[] b = ParamBuffers[2];
        b[0] = arg1;
        b[1] = arg2;
        b[3] = arg3;
        player.SendChat(key, background, iconUrlOverride, b);
    }
    internal static void SendChat(this SteamPlayer player, string key, object arg1, object arg2, object arg3, object arg4, Color background = default, string? iconUrlOverride = null)
    {
        ThreadUtil.assertIsGameThread();

        object[] b = ParamBuffers[3];
        b[0] = arg1;
        b[1] = arg2;
        b[2] = arg3;
        b[3] = arg4;
        player.SendChat(key, background, iconUrlOverride, b);
    }
    internal static void Broadcast(string key, Color background = default, string? iconUrlOverride = null, params object[] parameters)
    {
        ThreadUtil.assertIsGameThread();

        if (key is null)
            throw new ArgumentNullException(nameof(key));

        PreSendChat(ref key, ref background, ref iconUrlOverride, parameters);

        ChatManager.serverSendMessage(key, background, null, null, EChatMode.SAY, iconUrlOverride, true);
    }
    internal static void Broadcast(string key, Color background = default, string? iconUrlOverride = null)
    {
        Broadcast(key, background, iconUrlOverride, Array.Empty<object>());
    }
    internal static void Broadcast(string key, object arg1, Color background = default, string? iconUrlOverride = null)
    {
        ThreadUtil.assertIsGameThread();

        object[] b = ParamBuffers[0];
        b[0] = arg1;
        Broadcast(key, background, iconUrlOverride, b);
    }
    internal static void Broadcast(string key, object arg1, object arg2, Color background = default, string? iconUrlOverride = null)
    {
        ThreadUtil.assertIsGameThread();

        object[] b = ParamBuffers[1];
        b[0] = arg1;
        b[1] = arg2;
        Broadcast(key, background, iconUrlOverride, b);
    }
    internal static void Broadcast(string key, object arg1, object arg2, object arg3, Color background = default, string? iconUrlOverride = null)
    {
        ThreadUtil.assertIsGameThread();

        object[] b = ParamBuffers[2];
        b[0] = arg1;
        b[1] = arg2;
        b[2] = arg3;
        Broadcast(key, background, iconUrlOverride, b);
    }
    internal static void Broadcast(string key, object arg1, object arg2, object arg3, object arg4, Color background = default, string? iconUrlOverride = null)
    {
        ThreadUtil.assertIsGameThread();

        object[] b = ParamBuffers[3];
        b[0] = arg1;
        b[1] = arg2;
        b[2] = arg3;
        b[3] = arg3;
        Broadcast(key, background, iconUrlOverride, b);
    }
    internal static void BroadcastTo(Predicate<SteamPlayer> selector, string key, Color background = default, string? iconUrlOverride = null, params object[] parameters)
    {
        ThreadUtil.assertIsGameThread();

        if (key is null)
            throw new ArgumentNullException(nameof(key));

        PreSendChat(ref key, ref background, ref iconUrlOverride, parameters);

        for (int i = 0; i < Provider.clients.Count; i++)
        {
            SteamPlayer player = Provider.clients[i];
            if (selector(player))
                ChatManager.serverSendMessage(key, background, null, player, EChatMode.SAY, iconUrlOverride, true);
        }
    }
    internal static void BroadcastTo(Predicate<SteamPlayer> selector, string key, Color background = default, string? iconUrlOverride = null)
    {
        BroadcastTo(selector, key, background, iconUrlOverride, Array.Empty<object>());
    }
    internal static void BroadcastTo(Predicate<SteamPlayer> selector, string key, object arg1, Color background = default, string? iconUrlOverride = null)
    {
        ThreadUtil.assertIsGameThread();

        object[] b = ParamBuffers[0];
        b[0] = arg1;
        BroadcastTo(selector, key, background, iconUrlOverride, b);
    }
    internal static void BroadcastTo(Predicate<SteamPlayer> selector, string key, object arg1, object arg2, Color background = default, string? iconUrlOverride = null)
    {
        ThreadUtil.assertIsGameThread();

        object[] b = ParamBuffers[1];
        b[0] = arg1;
        b[1] = arg2;
        BroadcastTo(selector, key, background, iconUrlOverride, b);
    }
    internal static void BroadcastTo(Predicate<SteamPlayer> selector, string key, object arg1, object arg2, object arg3, Color background = default, string? iconUrlOverride = null)
    {
        ThreadUtil.assertIsGameThread();

        object[] b = ParamBuffers[2];
        b[0] = arg1;
        b[1] = arg2;
        b[2] = arg3;
        BroadcastTo(selector, key, background, iconUrlOverride, b);
    }
    internal static void BroadcastTo(Predicate<SteamPlayer> selector, string key, object arg1, object arg2, object arg3, object arg4, Color background = default, string? iconUrlOverride = null)
    {
        ThreadUtil.assertIsGameThread();

        object[] b = ParamBuffers[3];
        b[0] = arg1;
        b[1] = arg2;
        b[2] = arg3;
        b[3] = arg3;
        BroadcastTo(selector, key, background, iconUrlOverride, b);
    }
    internal static void SendChat(this Player player, string key, Color background = default, string? iconUrlOverride = null, params object[] parameters)
        => SendChat(player.channel.owner, key, background, iconUrlOverride, parameters);
    internal static void SendChat(this Player player, string key, Color background = default, string? iconUrlOverride = null)
        => player.channel.owner.SendChat(key, background, iconUrlOverride);
    internal static void SendChat(this Player player, object arg1, string key, Color background = default, string? iconUrlOverride = null)
        => player.channel.owner.SendChat(key, arg1, background, iconUrlOverride);
    internal static void SendChat(this Player player, object arg1, object arg2, string key, Color background = default, string? iconUrlOverride = null)
        => player.channel.owner.SendChat(key, arg1, arg2, background, iconUrlOverride);
    internal static void SendChat(this Player player, object arg1, object arg2, object arg3, string key, Color background = default, string? iconUrlOverride = null)
        => player.channel.owner.SendChat(key, arg1, arg2, arg3, background, iconUrlOverride);
    internal static void SendChat(this Player player, object arg1, object arg2, object arg3, object arg4, string key, Color background = default, string? iconUrlOverride = null)
        => player.channel.owner.SendChat(key, arg1, arg2, arg3, arg4, background, iconUrlOverride);

    private static readonly object[][] ParamBuffers =
    {
        new object[1],
        new object[2],
        new object[3],
        new object[4]
    };
    internal static TranslationList ConvertDefaultTranslations(TranslationList list)
    {
        TranslationList newList = new TranslationList();
        string leftSngl = new string(AngleBracketPlaceholderLeft, 1), rightSngl = new string(AngleBracketPlaceholderRight, 1);
        string leftDbl = new string(AngleBracketPlaceholderLeft, 2), rightDbl = new string(AngleBracketPlaceholderRight, 2);
        foreach (TranslationListEntry entry in list)
        {
            newList.Add(
                new TranslationListEntry(entry.Id,
                    entry.Value
                        .Replace(leftDbl, "\\" + leftDbl)
                        .Replace(rightDbl, "\\" + rightDbl)
                        .Replace(leftSngl, leftDbl)
                        .Replace(rightSngl, rightDbl)
                        .Replace("<", leftSngl)
                        .Replace(">", rightSngl))
                );
        }

        return newList;
    }
    private static void PreSendChat(ref string text, ref Color background, ref string? iconUrlOverride, object[] parameters)
    {
        if (background == default)
            background = Palette.AMBIENT;
        
        if ((iconUrlOverride ??= DefaultIconUrl) is not { Length: > 0 })
            iconUrlOverride = string.Empty;

        text = Translate(text, parameters);

        // wrapping is disabled, read above
        if (text.Length > MaxStringLength)
            text = text.Substring(0, MaxStringLength);
    }
    private static string Translate(string key, object[] parameters)
    {
        TranslationList list = SampleRocketPlugin.Instance.Translations.Instance;
        ConvertParameters(parameters);
        bool exception = false;
        // check file-provided keys
        foreach (TranslationListEntry item in list)
        {
            if (item.Id.Equals(key, StringComparison.Ordinal))
            {
                try
                {
                    return string.Format(ConvertTranslationValue(item.Value), parameters);
                }
                catch (FormatException ex)
                {
                    Logger.LogException(ex, "Format failed for key: {" + key + "} with " + parameters.Length +
                                            " formatting argument(s) expected.");
                    exception = true;
                }
            }
        }

        // check default keys
        list = SampleRocketPlugin.Instance.DefaultTranslations;
        foreach (TranslationListEntry item in list)
        {
            if (item.Id.Equals(key, StringComparison.Ordinal))
            {
                try
                {
                    return string.Format(ConvertTranslationValue(item.Value), parameters);
                }
                catch (FormatException ex)
                {
                    Logger.LogException(ex, "Format failed for key: {" + key + "} with " + parameters.Length +
                                            " formatting argument(s) expected.");
                    exception = true;
                }
            }
        }
        if (parameters.Length > 0)
            key += " {" + string.Join(", ", parameters) + "}";

        if (!exception)
            Logger.LogWarning("Key not found: {" + key + "} with " + parameters.Length + " formatting argument(s) expected.");
        return key;
    }
    private static void ConvertParameters(object[] parameters)
    {
        for (int i = 0; i < parameters.Length; ++i)
        {
            object? obj = parameters[i];
            if (obj != null)
            {
                parameters[i] = obj switch
                {
                    float v => v.ToString(DefaultDecimalFormatting, FormatProvider),
                    double v => v.ToString(DefaultDecimalFormatting, FormatProvider),
                    decimal v => v.ToString(DefaultDecimalFormatting, FormatProvider),
                    int v => v.ToString(FormatProvider),
                    ulong v => v.ToString(FormatProvider),
                    CSteamID v => v.m_SteamID.ToString("G17", FormatProvider),
                    Asset asset => asset.FriendlyName,
                    InteractableVehicle v => v.asset.FriendlyName,
                    BarricadeDrop v => v.asset.FriendlyName,
                    StructureDrop v => v.asset.FriendlyName,
                    IRocketPlayer v => v.DisplayName,
                    Player v => v.channel.owner.playerID.characterName,
                    SteamPlayer v => v.playerID.characterName,
                    _ => obj.ToString()
                };
            }
            else parameters[i] = NullPlaceholder;
        }
    }
    private static unsafe string ConvertTranslationValue(string output)
    {
        char* text = stackalloc char[output.Length];
        int textIndex = -1;
        for (int i = 0; i < output.Length; ++i)
        {
            char c = output[i];
            bool left = c == AngleBracketPlaceholderLeft;
            if (left || c == AngleBracketPlaceholderRight)
            {
                if (i < output.Length - 1 && output[i + 1] == c)
                {
                    // double placeholder detected
                    if (i != 0 && output[i - 1] == '\\')
                    {
                        // double placeholder escaped, set last and current to angle bracket
                        text[textIndex] = c = left ? '<' : '>';
                    }
                    ++i;
                }
                else
                {
                    c = left ? '<' : '>';
                }
            }

            text[++textIndex] = c;
        }

        return textIndex == -1 ? string.Empty : new string(text, 0, textIndex + 1);
    }
}