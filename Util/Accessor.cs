/*
 * This template is originally from
 * https://github.com/DanielWillett/SampleRocketPlugin
 */

using System.Reflection;
using System.Reflection.Emit;

namespace SampleRocketPlugin.Util;

/// <summary>
/// Various tools for accessing private/internal members commonly needed while making unturned plugins.
/// </summary>
internal static class Accessor
{
    /// <summary>
    /// Retreives a RPC from SDG code.<br/>
    /// Usage:
    /// <code>
    /// ClientStaticMethod&lt;byte, byte, uint, bool&gt;? SendDestroyItem = GetRPC&lt;ClientStaticMethod&lt;byte, byte, uint, bool&gt;, ItemManager&gt;("SendDestroyItem");
    /// </code>
    /// </summary>
    /// <exception cref="MissingFieldException">Thrown when <paramref name="throwError"/> is <see langword="true"/> and the RPC can't be found.</exception>
    /// <returns>
    /// If <paramref name="throwError"/> is <see langword="false"/> and a RPC can't be found, this method may return <see langword="null"/>. Otherwise it will not.
    /// </returns>
    public static TRequest? GetRPC<TRequest, TOwner>(string name, bool throwError = true) where TRequest : ClientMethodHandle
    {
        Exception? ex2 = null;
        try
        {
            FieldInfo? info = typeof(TOwner).GetField(name, BindingFlags.NonPublic | BindingFlags.Static);
            if (info != null && info.GetValue(null) is TRequest req)
            {
                Logger.Log("Found RPC: \"" + typeof(TOwner).Name + "." + name + "\".", ConsoleColor.Blue);
                return req;
            }
        }
        catch (Exception ex)
        {
            Logger.LogException(ex);
            ex2 = ex;
        }

        string msg = "Unable to retreive the RPC \"" + typeof(TOwner).Name + "." +
                     name + "\" with parameters: [" +
                     (!typeof(TRequest).IsGenericType ? "<none>" :
                         string.Join(", ", typeof(TRequest).GetGenericArguments().Select(x => x.Name))) +
                     "]";
        Logger.LogError(msg);
        if (throwError)
            throw new MissingFieldException(msg, ex2);

        return null;
    }

    /// <summary>
    /// Generates a method to quickly set a (usually private) instance field.
    /// </summary>
    /// <typeparam name="TInstance">Class that owns the field.</typeparam>
    /// <typeparam name="TValue">Value the field returns.</typeparam>
    /// <param name="fieldName"></param>
    /// <param name="flags">Usually will be <see cref="BindingFlags.NonPublic"/>.</param>
    /// <exception cref="FieldAccessException">Thrown when the field can't be found or does not match the type parameters.</exception>
    public static InstanceSetter<TInstance, TValue> GenerateInstanceSetter<TInstance, TValue>(string fieldName, BindingFlags flags)
    {
        flags |= BindingFlags.Instance;
        flags &= ~BindingFlags.Static;
        FieldInfo? field = typeof(TInstance).GetField(fieldName, flags);
        if (field is null || field.IsStatic || !field.FieldType.IsAssignableFrom(typeof(TValue)))
            throw new FieldAccessException("Field not found or invalid.");
        MethodAttributes attr = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
        DynamicMethod method = new DynamicMethod("set_" + fieldName, attr, CallingConventions.HasThis, typeof(void), new Type[] { typeof(TInstance), field.FieldType }, typeof(TInstance), true);
        method.DefineParameter(1, ParameterAttributes.None, "value");
        ILGenerator il = method.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Stfld, field);
        il.Emit(OpCodes.Ret);
        return (InstanceSetter<TInstance, TValue>)method.CreateDelegate(typeof(InstanceSetter<TInstance, TValue>));
    }

    /// <summary>
    /// Generates a method to quickly get the value of a (usually private) instance field.
    /// </summary>
    /// <typeparam name="TInstance">Class that owns the field.</typeparam>
    /// <typeparam name="TValue">Value the field returns.</typeparam>
    /// <param name="fieldName"></param>
    /// <param name="flags">Usually will be <see cref="BindingFlags.NonPublic"/>.</param>
    /// <exception cref="FieldAccessException">Thrown when the field can't be found or does not match the type parameters.</exception>
    public static InstanceGetter<TInstance, TValue> GenerateInstanceGetter<TInstance, TValue>(string fieldName, BindingFlags flags)
    {
        flags |= BindingFlags.Instance;
        flags &= ~BindingFlags.Static;
        FieldInfo? field = typeof(TInstance).GetField(fieldName, flags);
        if (field is null || field.IsStatic || !field.FieldType.IsAssignableFrom(typeof(TValue)))
            throw new FieldAccessException("Field not found or invalid.");
        MethodAttributes attr = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
        DynamicMethod method = new DynamicMethod("get_" + fieldName, attr, CallingConventions.HasThis, typeof(TValue), new Type[] { typeof(TInstance) }, typeof(TInstance), true);
        ILGenerator il = method.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldfld, field);
        il.Emit(OpCodes.Ret);
        return (InstanceGetter<TInstance, TValue>)method.CreateDelegate(typeof(InstanceGetter<TInstance, TValue>));
    }

    /// <summary>
    /// Generates a method to quickly set a (usually private) static field.
    /// </summary>
    /// <typeparam name="TInstance">Class that owns the field.</typeparam>
    /// <typeparam name="TValue">Value the field returns.</typeparam>
    /// <param name="fieldName"></param>
    /// <param name="flags">Usually will be <see cref="BindingFlags.NonPublic"/>.</param>
    /// <exception cref="FieldAccessException">Thrown when the field can't be found or does not match the type parameters.</exception>
    public static StaticSetter<TValue> GenerateStaticSetter<TInstance, TValue>(string fieldName, BindingFlags flags)
    {
        flags |= BindingFlags.Static;
        flags &= ~BindingFlags.Instance;
        FieldInfo? field = typeof(TInstance).GetField(fieldName, flags);
        if (field is null || !field.IsStatic || !field.FieldType.IsAssignableFrom(typeof(TValue)))
            throw new FieldAccessException("Field not found or invalid.");
        MethodAttributes attr = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
        DynamicMethod method = new DynamicMethod("set_" + fieldName, attr, CallingConventions.Standard, typeof(void), new Type[] { field.FieldType }, typeof(TInstance), true);
        method.DefineParameter(1, ParameterAttributes.None, "value");
        ILGenerator il = method.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Stsfld, field);
        il.Emit(OpCodes.Ret);
        return (StaticSetter<TValue>)method.CreateDelegate(typeof(StaticSetter<TValue>));
    }

    /// <summary>
    /// Generates a method to quickly get the value of a (usually private) static field.
    /// </summary>
    /// <typeparam name="TInstance">Class that owns the field.</typeparam>
    /// <typeparam name="TValue">Value the field returns.</typeparam>
    /// <param name="fieldName"></param>
    /// <param name="flags">Usually will be <see cref="BindingFlags.NonPublic"/>.</param>
    /// <exception cref="FieldAccessException">Thrown when the field can't be found or does not match the type parameters.</exception>
    public static StaticGetter<TValue> GenerateStaticGetter<TInstance, TValue>(string fieldName, BindingFlags flags)
    {
        flags |= BindingFlags.Static;
        flags &= ~BindingFlags.Instance;
        FieldInfo? field = typeof(TInstance).GetField(fieldName, flags);
        if (field is null || !field.IsStatic || !field.FieldType.IsAssignableFrom(typeof(TValue)))
            throw new FieldAccessException("Field not found or invalid.");
        MethodAttributes attr = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
        DynamicMethod method = new DynamicMethod("get_" + fieldName, attr, CallingConventions.Standard, typeof(TValue), Array.Empty<Type>(), typeof(TInstance), true);
        ILGenerator il = method.GetILGenerator();
        il.Emit(OpCodes.Ldsfld, field);
        il.Emit(OpCodes.Ret);
        return (StaticGetter<TValue>)method.CreateDelegate(typeof(StaticGetter<TValue>));
    }

    internal static MethodInfo? GetMethodInfo(Delegate method)
    {
        try
        {
            return method.GetMethodInfo();
        }
        catch (MemberAccessException)
        {
            return null!;
        }
    }
}

public delegate void InstanceSetter<in TInstance, in T>(TInstance owner, T value);
public delegate T InstanceGetter<in TInstance, out T>(TInstance owner);
public delegate void StaticSetter<in T>(T value);
public delegate T StaticGetter<out T>();
