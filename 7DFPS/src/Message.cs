using System;

namespace DFPS {
	public enum Message : ushort {
		SetUsername,
		Move,
		Die,
		Respawn,
		UpdateLookingAt,
		UpdateHealth,
		// TODO: MAKE THIS SERVER SIDE OMG!!!
		LeftClick,
		LeftUnclick,
		DropInventoryItem,
		SetInventoryItem,
		ConnectPlayer,
		DisconnectPlayer,
		OtherPlayerMove,
		UpdateWorld,
		Shot1Confirmed,
		Shot2Confirmed,
		Shot3Confirmed,
		ReloadConfimed,
		RemoveCrate,
		AddCrate,
		RemoveHealthPack,
		AddHealthPack,
		ThrowGrenade,
		DropItem,
		UpdateDirectionFacing,
		Chat,
		Last,
	}
}
