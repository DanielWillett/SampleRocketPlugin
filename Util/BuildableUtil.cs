/*
 * This template is originally from
 * https://github.com/DanielWillett/SampleRocketPlugin
 */

using System.Runtime.CompilerServices;

namespace SampleRocketPlugin.Util;
/// <summary>
/// Has some helper functions for dealing with barricades and structures.
/// </summary>
internal static class BuildableUtil
{
    private static ClientInstanceMethod<byte[]>? SendUpdateBarricadeState;
    private static bool hasTriedToGetRPC;

    /// <summary>
    /// A catch-all function to set the owner and/or group of a barricade. Updates correctly to clients immediately.
    /// </summary>
    public static void SetOwnerOrGroup(this BarricadeDrop drop, ulong? owner = null, ulong? group = null)
    {
        ThreadUtil.assertIsGameThread();
        if (!hasTriedToGetRPC)
        {
            SendUpdateBarricadeState = Accessor.GetRPC<ClientInstanceMethod<byte[]>, BarricadeDrop>("SendUpdateState", false);
            hasTriedToGetRPC = true;
        }
        if (!owner.HasValue && !group.HasValue)
            return;
        BarricadeData bdata = drop.GetServersideData();
        ulong o = owner ?? bdata.owner;
        ulong g = group ?? bdata.group;
        BarricadeManager.changeOwnerAndGroup(drop.model, o, g);
        byte[] oldSt = bdata.barricade.state;
        byte[] state;
        if (drop.interactable is InteractableStorage storage)
        {
            if (oldSt.Length < sizeof(ulong) * 2)
                oldSt = new byte[sizeof(ulong) * 2];
            Buffer.BlockCopy(BitConverter.GetBytes(o), 0, oldSt, 0, sizeof(ulong));
            Buffer.BlockCopy(BitConverter.GetBytes(g), 0, oldSt, sizeof(ulong), sizeof(ulong));
            BarricadeManager.updateState(drop.model, oldSt, oldSt.Length);
            drop.ReceiveUpdateState(oldSt);
            if (SendUpdateBarricadeState != null && BarricadeManager.tryGetRegion(drop.model, out byte x, out byte y, out ushort plant, out _))
            {
                if (storage.isDisplay)
                {
                    Block block = new Block();
                    if (storage.displayItem != null)
                        block.write(storage.displayItem.id, storage.displayItem.quality,
                            storage.displayItem.state ?? Array.Empty<byte>());
                    else
                        block.step += 4;
                    block.write(storage.displaySkin, storage.displayMythic,
                        storage.displayTags ?? string.Empty,
                        storage.displayDynamicProps ?? string.Empty, storage.rot_comp);
                    byte[] b = block.getBytes(out int size);
                    state = new byte[size + sizeof(ulong) * 2];
                    Buffer.BlockCopy(b, 0, state, sizeof(ulong) * 2, size);
                }
                else
                    state = new byte[sizeof(ulong) * 2];
                Buffer.BlockCopy(oldSt, 0, state, 0, sizeof(ulong) * 2);
                SendUpdateBarricadeState.Invoke(drop.GetNetId(), ENetReliability.Reliable,
                    BarricadeManager.EnumerateClients_Remote(x, y, plant), state);
            }
        }
        else
        {
            switch (drop.asset.build)
            {
                case EBuild.DOOR:
                case EBuild.GATE:
                case EBuild.SHUTTER:
                case EBuild.HATCH:
                    state = new byte[17];
                    Buffer.BlockCopy(BitConverter.GetBytes(o), 0, state, 0, sizeof(ulong));
                    Buffer.BlockCopy(BitConverter.GetBytes(g), 0, state, sizeof(ulong), sizeof(ulong));
                    state[16] = (byte)(oldSt[16] > 0 ? 1 : 0);
                    break;
                case EBuild.BED:
                    state = BitConverter.GetBytes(o);
                    break;
                case EBuild.STORAGE:
                case EBuild.SENTRY:
                case EBuild.SENTRY_FREEFORM:
                case EBuild.SIGN:
                case EBuild.SIGN_WALL:
                case EBuild.NOTE:
                case EBuild.LIBRARY:
                case EBuild.MANNEQUIN:
                    if (oldSt.Length < sizeof(ulong) * 2)
                        state = new byte[sizeof(ulong) * 2];
                    else
                    {
                        state = new byte[oldSt.Length];
                        Buffer.BlockCopy(oldSt, 0, state, 0, state.Length);
                    }
                    Buffer.BlockCopy(BitConverter.GetBytes(o), 0, state, 0, sizeof(ulong));
                    Buffer.BlockCopy(BitConverter.GetBytes(g), 0, state, sizeof(ulong), sizeof(ulong));
                    break;
                case EBuild.SPIKE:
                case EBuild.WIRE:
                case EBuild.CHARGE:
                case EBuild.BEACON:
                case EBuild.CLAIM:
                    state = oldSt.Length == 0 ? oldSt : Array.Empty<byte>();
                    if (drop.interactable is InteractableCharge charge)
                    {
                        charge.owner = o;
                        charge.group = g;
                    }
                    else if (drop.interactable is InteractableClaim claim)
                    {
                        claim.owner = o;
                        claim.group = g;
                    }
                    break;
                default:
                    state = oldSt;
                    break;
            }
            bool diff = state.Length != oldSt.Length;
            if (!diff)
            {
                for (int i = 0; i < state.Length; ++i)
                {
                    if (state[i] != oldSt[i])
                    {
                        diff = true;
                        break;
                    }
                }
            }
            if (diff)
            {
                BarricadeManager.updateReplicatedState(drop.model, state, state.Length);
            }
        }
    }

    /// <summary>
    /// A catch-all function to set the owner and/or group of a barricade. Updates correctly to clients immediately.
    /// </summary>
    public static void SetOwnerOrGroup(this StructureDrop drop, ulong? owner = null, ulong? group = null)
    {
        ThreadUtil.assertIsGameThread();
        if (!owner.HasValue && !group.HasValue)
            return;
        StructureData sdata = drop.GetServersideData();
        StructureManager.changeOwnerAndGroup(drop.model, owner ?? sdata.owner, group ?? sdata.group);
    }

    /// <summary>
    /// Finds a barricade from it's instance id. Use <see cref="FindBarricade(uint, Vector3)"/> if you know where it should be.
    /// </summary>
    public static BarricadeDrop? FindBarricade(uint instanceID)
    {
        for (byte x = 0; x < Regions.WORLD_SIZE; x++)
        {
            for (byte y = 0; y < Regions.WORLD_SIZE; y++)
            {
                BarricadeRegion region = BarricadeManager.regions[x, y];
                foreach (BarricadeDrop barricade in region.drops)
                {
                    if (barricade.instanceID == instanceID)
                        return barricade;
                }
            }
        }

        for (int i = 0; i < BarricadeManager.vehicleRegions.Count; ++i)
        {
            foreach (BarricadeDrop barricade in BarricadeManager.vehicleRegions[i].drops)
            {
                if (barricade.instanceID == instanceID)
                    return barricade;
            }
        }

        return null;
    }

    /// <summary>
    /// Finds a barricade from it's instance id and uses an expected position to optimize the search.
    /// </summary>
    /// <remarks><paramref name="expectedPosition"/> does not help on planted barricades.</remarks>
    public static BarricadeDrop? FindBarricade(uint instanceID, Vector3 expectedPosition)
    {
        if (BarricadeManager.regions == null)
            throw new InvalidOperationException("Barricade manager has not yet been initialized.");
        bool f = false;
        if (Regions.tryGetCoordinate(expectedPosition, out byte x1, out byte y1))
        {
            f = true;
            BarricadeDrop? drop = ScanBarricadeRegion(instanceID, x1, y1);
            if (drop != null) return drop;
            drop = ScanBarricadeRegion(instanceID, (byte)(x1 - 1), y1);
            if (drop != null) return drop;
            drop = ScanBarricadeRegion(instanceID, (byte)(x1 + 1), y1);
            if (drop != null) return drop;
            drop = ScanBarricadeRegion(instanceID, x1, (byte)(y1 - 1));
            if (drop != null) return drop;
            drop = ScanBarricadeRegion(instanceID, x1, (byte)(y1 + 1));
            if (drop != null) return drop;
            drop = ScanBarricadeRegion(instanceID, (byte)(x1 - 1), (byte)(y1 - 1));
            if (drop != null) return drop;
            drop = ScanBarricadeRegion(instanceID, (byte)(x1 - 1), (byte)(y1 + 1));
            if (drop != null) return drop;
            drop = ScanBarricadeRegion(instanceID, (byte)(x1 + 1), (byte)(y1 - 1));
            if (drop != null) return drop;
            drop = ScanBarricadeRegion(instanceID, (byte)(x1 + 1), (byte)(y1 + 1));
            if (drop != null) return drop;
        }
        for (int x = 0; x < Regions.WORLD_SIZE; ++x)
        {
            for (int y = 0; y < Regions.WORLD_SIZE; ++y)
            {
                if (f && (x - x1) is -1 or 0 or 1 && (y - y1) is -1 or 0 or 1)
                    continue;
                BarricadeRegion region = BarricadeManager.regions[x, y];
                foreach (BarricadeDrop drop in region.drops)
                    if (drop.instanceID == instanceID)
                        return drop;
            }
        }
        for (int vr = 0; vr < BarricadeManager.vehicleRegions.Count; ++vr)
        {
            VehicleBarricadeRegion region = BarricadeManager.vehicleRegions[vr];
            for (int i = 0; i < region.drops.Count; ++i)
                if (region.drops[i].instanceID == instanceID)
                    return region.drops[i];
        }
        return default;
    }

    /// <summary>
    /// Finds a structure from it's instance id. Use <see cref="FindStructure(uint, Vector3)"/> if you know where it should be.
    /// </summary>
    public static StructureDrop? FindStructure(uint instanceID)
    {
        for (int x = 0; x < Regions.WORLD_SIZE; x++)
        {
            for (int y = 0; y < Regions.WORLD_SIZE; y++)
            {
                StructureRegion region = StructureManager.regions[x, y];
                if (region == default) continue;
                foreach (StructureDrop drop in region.drops)
                {
                    if (drop.instanceID == instanceID)
                    {
                        return drop;
                    }
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Finds a structure from it's instance id and uses an expected position to optimize the search.
    /// </summary>
    public static StructureDrop? FindStructure(uint instanceID, Vector3 expectedPosition)
    {
        if (StructureManager.regions == null)
            throw new InvalidOperationException("Structure manager has not yet been initialized.");
        bool f = false;
        if (Regions.tryGetCoordinate(expectedPosition, out byte x1, out byte y1))
        {
            f = true;
            StructureDrop? drop = ScanStructureRegion(instanceID, x1, y1);
            if (drop != null) return drop;
            drop = ScanStructureRegion(instanceID, (byte)(x1 - 1), y1);
            if (drop != null) return drop;
            drop = ScanStructureRegion(instanceID, (byte)(x1 + 1), y1);
            if (drop != null) return drop;
            drop = ScanStructureRegion(instanceID, x1, (byte)(y1 - 1));
            if (drop != null) return drop;
            drop = ScanStructureRegion(instanceID, x1, (byte)(y1 + 1));
            if (drop != null) return drop;
            drop = ScanStructureRegion(instanceID, (byte)(x1 - 1), (byte)(y1 - 1));
            if (drop != null) return drop;
            drop = ScanStructureRegion(instanceID, (byte)(x1 - 1), (byte)(y1 + 1));
            if (drop != null) return drop;
            drop = ScanStructureRegion(instanceID, (byte)(x1 + 1), (byte)(y1 - 1));
            if (drop != null) return drop;
            drop = ScanStructureRegion(instanceID, (byte)(x1 + 1), (byte)(y1 + 1));
            if (drop != null) return drop;
        }
        for (int x = 0; x < Regions.WORLD_SIZE; ++x)
        {
            for (int y = 0; y < Regions.WORLD_SIZE; ++y)
            {
                if (f && x - x1 is -1 or 0 or 1 && y - y1 is -1 or 0 or 1)
                    continue;
                StructureRegion region = StructureManager.regions[x, y];
                foreach (StructureDrop drop in region.drops)
                    if (drop.instanceID == instanceID)
                        return drop;
            }
        }
        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static BarricadeDrop? ScanBarricadeRegion(uint instanceID, byte x, byte y)
    {
        if (x > Regions.WORLD_SIZE || y > Regions.WORLD_SIZE)
            return null;
        BarricadeRegion region = BarricadeManager.regions[x, y];
        for (int i = 0; i < region.drops.Count; i++)
        {
            if (region.drops[i].instanceID == instanceID)
                return region.drops[i];
        }

        return null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static StructureDrop? ScanStructureRegion(uint instanceID, byte x, byte y)
    {
        if (x > Regions.WORLD_SIZE || y > Regions.WORLD_SIZE)
            return null;
        StructureRegion region = StructureManager.regions[x, y];
        for (int i = 0; i < region.drops.Count; i++)
        {
            if (region.drops[i].instanceID == instanceID)
                return region.drops[i];
        }

        return null;
    }
}
