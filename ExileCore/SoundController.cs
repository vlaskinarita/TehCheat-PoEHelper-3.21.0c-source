using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharpDX.Multimedia;
using SharpDX.XAudio2;

namespace ExileCore;

public class SoundController : IDisposable
{
	private readonly List<SourceVoice> _list = new List<SourceVoice>();

	private readonly bool initialized;

	private readonly MasteringVoice masteringVoice;

	private readonly Dictionary<string, MyWave> Sounds;

	private readonly string soundsDir;

	private readonly XAudio2 xAudio2;

	public SoundController(string dir)
	{
		soundsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dir);
		if (!Directory.Exists(soundsDir))
		{
			initialized = false;
			DebugWindow.LogError("Sounds dir not found, continue working without any sound.");
			return;
		}
		xAudio2 = new XAudio2();
		xAudio2.StartEngine();
		masteringVoice = new MasteringVoice(xAudio2);
		string[] files = Directory.GetFiles(soundsDir, "*.wav");
		Sounds = new Dictionary<string, MyWave>(files.Length);
		initialized = true;
	}

	public void Dispose()
	{
		foreach (KeyValuePair<string, MyWave> sound in Sounds)
		{
			sound.Value.Buffer.Stream.Dispose();
		}
		xAudio2.StopEngine();
		masteringVoice?.Dispose();
		xAudio2?.Dispose();
	}

	public void PlaySound(string name)
	{
		if (!initialized)
		{
			return;
		}
		if (Sounds.TryGetValue(name, out var value))
		{
			if (value == null)
			{
				value = LoadSound(name);
			}
		}
		else
		{
			value = LoadSound(name);
		}
		if (value == null)
		{
			DebugWindow.LogError("Sound file: " + name + ".wav not found.");
			return;
		}
		SourceVoice sourceVoice = new SourceVoice(xAudio2, value.WaveFormat, enableCallbackEvents: true);
		sourceVoice.SubmitSourceBuffer(value.Buffer, value.DecodedPacketsInfo);
		sourceVoice.Start();
		_list.Add(sourceVoice);
		for (int i = 0; i < _list.Count; i++)
		{
			SourceVoice sourceVoice2 = _list[i];
			if (sourceVoice2.State.BuffersQueued <= 0)
			{
				sourceVoice2.Stop();
				sourceVoice2.DestroyVoice();
				sourceVoice2.Dispose();
				_list.RemoveAt(i);
			}
		}
	}

	public void PreloadSound(string name)
	{
		LoadSound(name);
	}

	private MyWave LoadSound(string name)
	{
		if (name.IndexOf(".wav", StringComparison.Ordinal) == -1)
		{
			name = Path.Combine(soundsDir, name + ".wav");
		}
		FileInfo fileInfo = new FileInfo(name);
		if (!fileInfo.Exists)
		{
			return null;
		}
		SoundStream soundStream = new SoundStream(File.OpenRead(name));
		WaveFormat format = soundStream.Format;
		AudioBuffer buffer = new AudioBuffer
		{
			Stream = soundStream.ToDataStream(),
			AudioBytes = (int)soundStream.Length,
			Flags = BufferFlags.EndOfStream
		};
		soundStream.Close();
		MyWave myWave = new MyWave
		{
			Buffer = buffer,
			WaveFormat = format,
			DecodedPacketsInfo = soundStream.DecodedPacketsInfo
		};
		Sounds[fileInfo.Name.Split('.').First()] = myWave;
		Sounds[fileInfo.Name] = myWave;
		return myWave;
	}

	public void SetVolume(float volume)
	{
		masteringVoice.SetVolume(volume);
	}
}
