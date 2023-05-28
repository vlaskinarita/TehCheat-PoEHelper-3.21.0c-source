namespace ExileCore.Shared;

public interface ICoroutine
{
	bool IsCompleted { get; }

	ICoroutine[] Children { get; }

	void Continue();
}
