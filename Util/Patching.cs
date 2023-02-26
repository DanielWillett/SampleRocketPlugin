/*
 * This template is originally from
 * https://github.com/DanielWillett/SampleRocketPlugin
 */

using HarmonyLib;
using System.Reflection;

namespace SampleRocketPlugin.Util;

internal static class Patching
{
    internal static Harmony? PatchHost { get; set; }
    internal static bool PatchMethod(Delegate original, Delegate? prefix = null, Delegate? postfix = null, Delegate? transpiler = null, Delegate? finalizer = null, string? desc = null)
    {
        CheckPatchHostAssigned();
        if (original is null || (prefix is null && postfix is null && transpiler is null && finalizer is null)) return false;
        try
        {
            MethodInfo? originalInfo = original.Method;
            MethodInfo? prefixInfo = prefix?.Method;
            MethodInfo? postfixInfo = prefix?.Method;
            MethodInfo? transpilerInfo = prefix?.Method;
            MethodInfo? finalizerInfo = prefix?.Method;
            if (originalInfo is null)
            {
                Logger.LogWarning("Error getting method info for patching" + (desc != null ? " " + desc : string.Empty) + ".");
                return false;
            }
            if (prefixInfo is null && postfixInfo is null && transpilerInfo is null && finalizerInfo is null)
            {
                Logger.LogWarning("Error getting method info for patching " + originalInfo.FullDescription());
                return false;
            }
            if (prefix is not null && prefixInfo is null)
                Logger.LogWarning("Error getting prefix info for patching " + originalInfo.FullDescription());
            if (postfix is not null && postfixInfo is null)
                Logger.LogWarning("Error getting postfix info for patching " + originalInfo.FullDescription());
            if (transpiler is not null && transpilerInfo is null)
                Logger.LogWarning("Error getting transpiler info for patching " + originalInfo.FullDescription());
            if (finalizer is not null && finalizerInfo is null)
                Logger.LogWarning("Error getting finalizer info for patching " + originalInfo.FullDescription());
            return PatchMethod(originalInfo, prefixInfo, postfixInfo, transpilerInfo, finalizerInfo, desc);
        }
        catch (MemberAccessException ex)
        {
            Logger.LogException(ex, "Error getting method info for patching" + (desc != null ? " " + desc : string.Empty) + ".");
            return false;
        }
    }
    internal static bool PatchMethod(MethodInfo? original, MethodInfo? prefix = null, MethodInfo? postfix = null, MethodInfo? transpiler = null, MethodInfo? finalizer = null, string? desc = null)
    {
        bool fail = false;
        PatchMethod(original, ref fail, prefix, postfix, transpiler, finalizer, desc);
        return fail;
    }
    internal static void PatchMethod(MethodInfo? original, ref bool fail, MethodInfo? prefix = null, MethodInfo? postfix = null, MethodInfo? transpiler = null, MethodInfo? finalizer = null, string? desc = null)
    {
        CheckPatchHostAssigned();
        if ((prefix is null && postfix is null && transpiler is null && finalizer is null))
        {
            fail = true;
            return;
        }
        if (original is null)
        {
            MethodInfo m = prefix ?? postfix ?? transpiler ?? finalizer!;
            Logger.LogWarning("Failed to find original method for patch " + m.FullDescription() + ".");
            fail = true;
            return;
        }

        HarmonyMethod? prfx2 = prefix is null ? null : new HarmonyMethod(prefix);
        HarmonyMethod? pofx2 = postfix is null ? null : new HarmonyMethod(postfix);
        HarmonyMethod? tplr2 = transpiler is null ? null : new HarmonyMethod(transpiler);
        HarmonyMethod? fnlr2 = finalizer is null ? null : new HarmonyMethod(finalizer);
        try
        {
            PatchHost!.Patch(original, prefix: prfx2, postfix: pofx2, transpiler: tplr2, finalizer: fnlr2);
            Logger.Log("Patched " + (desc ?? original.Name) + ".");
        }
        catch (Exception ex)
        {
            Logger.LogException(ex, "Error patching " + original.FullDescription());
            fail = true;
        }
    }
    private static void CheckPatchHostAssigned()
    {
        if (PatchHost == null)
        {
            throw new InvalidOperationException("Patching.PatchHost must be set before attempting to patch something.");
        }
    }
}
