using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.SceneManagement;

public class Comms : MonoBehaviour {

	public double TimeDelay = 2.5f;
	public List<KeyCode> MicKeys = new List<KeyCode>() {
		KeyCode.Space, KeyCode.JoystickButton3
	};
	private AudioSource _audioSource = null;
	private AudioClip _audioClip = null;

	private const int NETWORK_CHUNK_SIZE = 62 * 1024;
	private Dictionary<Guid, List<DelayedAudioMessageChunk>> _audioChunkAssemblyZone = new Dictionary<Guid, List<DelayedAudioMessageChunk>>();

	public class DelayedAudioClip {
		public Guid id;
		public double timeDelay = 0.0;
		public AudioClip audioClip;

		public static AudioClip CreateAudioClip(int samples, int channels, int frequency, byte[] audioSamplesBytes) {
			var bf = new BinaryFormatter();
			using (var msUncompressed = new MemoryStream(audioSamplesBytes))
			{
				float[] audioSamples = bf.Deserialize(msUncompressed) as float[];
				var audioClip = AudioClip.Create("<from network>", samples, channels, frequency, false);
				audioClip.SetData(audioSamples, 0);
				return audioClip;
			}
		}

		public static byte[] AudioClipToBytes(AudioClip audioClip) {
			float[] samples = new float[audioClip.samples * audioClip.channels];
			audioClip.GetData(samples, 0);
			BinaryFormatter bf = new BinaryFormatter();
			using (var msUncompressed = new MemoryStream()) {
				bf.Serialize(msUncompressed, samples);
				long uncompressedSize = msUncompressed.Length;

				return msUncompressed.ToArray();
			}
		}

		public static AudioClip CreateAudioClipFromCompressed(int samples, int channels, int frequency, byte[] audioSamplesBytes) {
			var bf = new BinaryFormatter();
			using (var msCompressed = new MemoryStream(audioSamplesBytes))
			using (var msUncompressed = new MemoryStream())
			using (var gzipStream = new GZipStream(msCompressed, CompressionMode.Decompress))
			{
				CopyTo(gzipStream, msUncompressed);

				float[] audioSamples = bf.Deserialize(msUncompressed) as float[];
				var audioClip = AudioClip.Create("<from network>", samples, channels, frequency, false);
				audioClip.SetData(audioSamples, 0);
				return audioClip;
			}
		}

		public static byte[] AudioClipToCompressedBytes(AudioClip audioClip) {
			float[] samples = new float[audioClip.samples * audioClip.channels];
			audioClip.GetData(samples, 0);
			BinaryFormatter bf = new BinaryFormatter();
			using (var msUncompressed = new MemoryStream()) {
				bf.Serialize(msUncompressed, samples);
				long uncompressedSize = msUncompressed.Length;

				using (var msCompressed = new MemoryStream())
				using (var gzipStream = new GZipStream(msCompressed, CompressionMode.Compress)) {
					CopyTo(msUncompressed, gzipStream);
					long compressedSize = msCompressed.Length;
					Debug.Log(string.Format(
						"Compressed audio clip  uncompressed= {0}  {1} kB  compressed= {2}  {3} kB",
						uncompressedSize, ((float)uncompressedSize) / 1024.0f,
						compressedSize, ((float)compressedSize) / 1024.0f
					));

					return msCompressed.ToArray();
				}
			}
		}
		public static void CopyTo(Stream src, Stream dest) {
			byte[] bytes = new byte[4096];

			int cnt;

			while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0) {
				dest.Write(bytes, 0, cnt);
			}
		}


	}

	// public class DelayedAudioMessage : MessageBase {
	// 	public double timeDelay = 0.0;

	// 	public int samples = 0;
	// 	public int channels = 0;
	// 	public int frequency = 0;
	// 	public byte[] audioSamplesSerialized = null;


	// 	// public override void Deserialize(NetworkReader reader) {
	// 	// 	timeDelay = reader.ReadDouble();

	// 	// 	samples = reader.ReadInt32();
	// 	// 	channels = reader.ReadInt32();
	// 	// 	frequency = reader.ReadInt32();
	// 	// 	audioSamplesSerialized = reader.ReadBytesAndSize();
	// 	// }

	// 	// public override void Serialize(NetworkWriter writer) {
	// 	// 	writer.Write((double)timeDelay);

	// 	// 	writer.Write((int)samples);
	// 	// 	writer.Write((int)channels);
	// 	// 	writer.Write((int)frequency);
	// 	// 	// writer.WriteBytesAndSize(audioSamplesSerialized, audioSamplesSerialized.Count());
	// 	// 	writer.WriteBytesFull(audioSamplesSerialized);
	// 	// }
	// }

	public class DelayedAudioMessageChunk : MessageBase {
		public string id;
		public uint chunkIndex;
		public uint chunkCount;

		public double timeDelay = 0.0;

		public int samples = 0;
		public int channels = 0;
		public int frequency = 0;
		public byte[] audioSamplesSerialized = null;
	}

	IEnumerator Start () {
		var microphonesMsg = string.Format(
			"Found microphones: \"{0}\"",
			string.Join("\", \"", Microphone.devices));
		Debug.Log(microphonesMsg);

		yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
        if (Application.HasUserAuthorization(UserAuthorization.Microphone)) {
        } else {
            Debug.LogError("We need the microphone permissions");
        }

		_audioSource = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {
		if (SceneManager.GetActiveScene().name == "FlightDirector") {
			var rover = GameObject.Find("carRoot(Clone)");
			if (rover != null) {
				List<AudioListener> silencedAudioListeners = new List<AudioListener>();
				AudioListener[] audioListeners = rover.GetComponentsInChildren<AudioListener>();
				foreach (var audioListener in audioListeners) {
					if (audioListener.enabled) {
						silencedAudioListeners.Add(audioListener);
					}
					audioListener.enabled = false;
				}
				if (silencedAudioListeners.Count > 0) {
					Debug.LogWarningFormat("Silenced audio listeners: {0}", string.Join(", ", silencedAudioListeners.Select((al) => al.name).ToArray()));
				}
			}
		}

		// UNCOMMENT HERE TO ENABLE COMMS
		foreach (var micKey in MicKeys) {
			if (Input.GetKeyDown(micKey)) {
				int sampleRate = 44100;
				_audioClip = Microphone.Start(null, true, 8, sampleRate);
			} else if (Input.GetKeyUp(micKey)) {
				Microphone.End(null);
				var audioMessage = new DelayedAudioClip() {
					id = System.Guid.NewGuid(),
					audioClip = _audioClip,
					timeDelay = TimeDelay
				};
				_audioClip = null;

				// this is instant and local
				// _audioSource.clip = audioMessage.audioClip;
				// _audioSource.Play();

				StartCoroutine(SendWithTimeDelay(audioMessage));
			}
		}
	}

	public IEnumerator SendWithTimeDelay(DelayedAudioClip audioMessage) {
		yield return new WaitForSeconds((float)(audioMessage.timeDelay));

		// this is delayed and local
		// _audioSource.clip = audioMessage.AudioClip;
		// _audioSource.Play();

		// this is delayed and sent to the network
		var networkManager = GetComponent<NetworkManager>() as NetworkManager;
		byte[] audioClipSerialized = DelayedAudioClip.AudioClipToBytes(audioMessage.audioClip);
		uint chunkCount = (uint)(audioClipSerialized.Length / NETWORK_CHUNK_SIZE + 1);
		DelayedAudioMessageChunk[] audioMessageChunks = new DelayedAudioMessageChunk[chunkCount];
		for (uint chunkIndex = 0; chunkIndex < chunkCount; chunkIndex++) {
			// Buffer.BlockCopy
			var currentChunk = new DelayedAudioMessageChunk() {
				id = audioMessage.id.ToString(),
				chunkCount = chunkCount,
				chunkIndex = chunkIndex,
				timeDelay = audioMessage.timeDelay,
				samples = audioMessage.audioClip.samples,
				channels = audioMessage.audioClip.channels,
				frequency = audioMessage.audioClip.frequency,
				audioSamplesSerialized = audioClipSerialized
					.Skip((int)(chunkIndex * NETWORK_CHUNK_SIZE))
					.Take((int)NETWORK_CHUNK_SIZE).ToArray()
			};
			audioMessageChunks[chunkIndex] = currentChunk;

			NetworkServer.SendToAll(NetworkMsgIds.CommsAudioMessage, currentChunk);
		}
	}

	private static byte[] CombineByteArray(params byte[][] arrays)
	{
		byte[] ret = new byte[arrays.Sum(x => x.Length)];
		int offset = 0;
		foreach (byte[] data in arrays)
		{
			Buffer.BlockCopy(data, 0, ret, offset, data.Length);
			offset += data.Length;
		}
		return ret;
	}

	public void OnMsgCommsAudioMessage(NetworkMessage msg) {
		Comms.DelayedAudioMessageChunk networkAudioMessage = msg.ReadMessage<Comms.DelayedAudioMessageChunk>();
		Guid guid = new Guid(networkAudioMessage.id);

		if (!_audioChunkAssemblyZone.ContainsKey(guid)) {
			_audioChunkAssemblyZone[guid] = new List<DelayedAudioMessageChunk>();
		}
		var chunksList = _audioChunkAssemblyZone[guid];
		_audioChunkAssemblyZone[guid].Add(networkAudioMessage);

		Debug.Log(string.Format(
			"Received Audio chunk  id= {0}  chunkIndex={1}  chunkCount={2}  receivedChunksCount={3}",
			guid, networkAudioMessage.chunkIndex, networkAudioMessage.chunkCount,
			chunksList.Count
		));

		if (chunksList.Count == networkAudioMessage.chunkCount) {
			chunksList = chunksList.OrderBy(e => e.chunkIndex).ToList();

			byte[] joinedBytes = chunksList.SelectMany(e => e.audioSamplesSerialized).ToArray();
			AudioClip audioClip = DelayedAudioClip.CreateAudioClip(
				networkAudioMessage.samples, networkAudioMessage.channels,
				networkAudioMessage.frequency, joinedBytes);

			Debug.Log(string.Format(
				"Recevied CommsAudio  delay={0}s  audioClip= {1} {2} samples={3} channels={4} freq={5} duration={6}",
				networkAudioMessage.timeDelay, audioClip,
				audioClip.name, audioClip.samples,
				audioClip.channels, audioClip.frequency,
				audioClip.length));

			_audioSource.clip = audioClip;
			_audioSource.Play();

			_audioChunkAssemblyZone.Remove(guid);
		}
	}
}	
