using System;
using System.Collections.Generic;
using ExileCore.Shared.Cache;
using ExileCore.Shared.Helpers;
using GameOffsets;
using GameOffsets.Native;

namespace ExileCore.PoEMemory.Components;

public class StateMachine : Component
{
	private readonly CachedValue<StateMachineComponentOffsets> _stateMachine;

	public IList<StateMachineState> States => ReadStates();

	public bool CanBeTarget => base.M.Read<byte>(base.Address + 160) == 1;

	public bool InTarget => base.M.Read<byte>(base.Address + 162) == 1;

	[Obsolete("Use ReadStates() instead")]
	private long MachineStates => base.M.Read<long>(base.Address + 56);

	[Obsolete("Use ReadStates() instead")]
	public int EnergyType => base.M.Read<int>(MachineStates);

	[Obsolete("Use ReadStates() instead")]
	public bool IsVisuallyFeeding => base.M.Read<bool>(MachineStates + 4);

	public StateMachine()
	{
		_stateMachine = new FrameCache<StateMachineComponentOffsets>(() => (base.Address != 0L) ? base.M.Read<StateMachineComponentOffsets>(base.Address) : default(StateMachineComponentOffsets));
	}

	public unsafe IList<StateMachineState> ReadStates()
	{
		StateMachineComponentOffsets value = _stateMachine.Value;
		long size = value.StatesValues.Size;
		long num = size / 8;
		List<StateMachineState> list = new List<StateMachineState>();
		if (num <= 0)
		{
			return list;
		}
		if (num > 100)
		{
			Logger.Log.Error("Error reading states in StateMachine component");
			return list;
		}
		byte[] array = base.M.ReadBytes(value.StatesValues.First, size);
		long[] array2 = new long[num];
		fixed (byte* ptr = array)
		{
			void* source = ptr;
			fixed (long* ptr2 = array2)
			{
				void* destination = ptr2;
				Buffer.MemoryCopy(source, destination, array.Length, array.Length);
			}
		}
		long num2 = base.M.Read<long>(value.StatesPtr + 16);
		for (int i = 0; i < num; i++)
		{
			long addr = num2 + i * 192;
			string name = base.M.Read<NativeUtf8Text>(addr).ToString(base.M);
			long value2 = array2[i];
			list.Add(new StateMachineState(name, value2));
		}
		return list;
	}
}
