using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkMsgIds {
	public const short GenericJson = MsgType.Highest + 1;
	public const short NamedLocationUpdate = MsgType.Highest + 2;
	public const short CommsAudioMessage = MsgType.Highest + 3;

	public const short Highest = 2000;
}
