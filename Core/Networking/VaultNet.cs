using DragonVault.Core.Systems;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace DragonVault.Core.Networking
{
	internal class VaultNet : ModSystem
	{
		public static void SendWithdrawl(int amount, Item item, int toClient = -1, int ignoreClient = -1)
		{
			if (Main.netMode == NetmodeID.SinglePlayer) //single player dosent care about packets
				return;

			ModLoader.GetMod("DragonVault").Logger.Info($"Sending withdrawl of {amount} {item.Name}");

			ModPacket packet = ModLoader.GetMod("DragonVault").GetPacket();
			packet.Write("Withdrawl");
			packet.Write(amount);
			ItemIO.Send(item, packet);

			packet.Send(toClient, ignoreClient);
		}

		public static void SendDeposit(int amount, Item item, int toClient = -1, int ignoreClient = -1)
		{
			if (Main.netMode == NetmodeID.SinglePlayer) //single player dosent care about packets
				return;

			ModLoader.GetMod("DragonVault").Logger.Info($"Sending deposit of {amount} {item.Name}");

			ModPacket packet = ModLoader.GetMod("DragonVault").GetPacket();
			packet.Write("Deposit");
			packet.Write(amount);
			ItemIO.Send(item, packet);

			packet.Send(toClient, ignoreClient);
		}

		/// <summary>
		/// Sent by the client to the server on join, requesting a given sequence of item in the vault
		/// </summary>
		/// <param name="sequence"></param>
		/// <param name="maxSequence"></param>
		/// <param name="toClient"></param>
		/// <param name="ignoreClient"></param>
		public static void OnJoinReq(int sequence, int toClient = -1, int ignoreClient = -1)
		{
			if (Main.netMode == NetmodeID.SinglePlayer) //single player dosent care about packets
				return;

			ModPacket packet = ModLoader.GetMod("DragonVault").GetPacket();
			packet.Write("JoinReq");
			packet.Write(sequence);

			packet.Send(toClient, ignoreClient);
		}

		/// <summary>
		/// Send by the server to respond to an OnJoinReq packet, sending back the requested item based on sequence number
		/// </summary>
		/// <param name="sequence"></param>
		/// <param name="maxSequence"></param>
		/// <param name="toClient"></param>
		/// <param name="ignoreClient"></param>
		public static void SendOnJoin(int sequence, int maxSequence, int toClient = -1, int ignoreClient = -1)
		{
			if (Main.netMode == NetmodeID.SinglePlayer) //single player dosent care about packets
				return;

			ItemEntry thisItem = StorageSystem.vault[sequence];

			ModPacket packet = ModLoader.GetMod("DragonVault").GetPacket();
			packet.Write("Join");
			packet.Write(thisItem.simStack);
			packet.Write(sequence);
			packet.Write(maxSequence);
			ItemIO.Send(thisItem.item, packet);

			packet.Send(toClient, ignoreClient);
		}

		public static void DataReq(int toClient = -1, int ignoreClient = -1)
		{
			if (Main.netMode == NetmodeID.SinglePlayer) //single player dosent care about packets
				return;

			ModPacket packet = ModLoader.GetMod("DragonVault").GetPacket();
			packet.Write("DataReq");

			packet.Send(toClient, ignoreClient);
		}

		public static void Data(int toClient = -1, int ignoreClient = -1)
		{
			if (Main.netMode == NetmodeID.SinglePlayer) //single player dosent care about packets
				return;

			ModPacket packet = ModLoader.GetMod("DragonVault").GetPacket();
			packet.Write("Data");
			packet.Write(StorageSystem.baseCapacity);
			packet.Write((int)StorageSystem.stoneFlags);

			packet.Send(toClient, ignoreClient);
		}

		public static void CraftingDataRequest(int toClient = -1, int ignoreClient = -1)
		{
			if (Main.netMode == NetmodeID.SinglePlayer) //single player dosent care about packets
				return;

			ModPacket packet = ModLoader.GetMod("DragonVault").GetPacket();
			packet.Write("CraftingDataReq");

			packet.Send(toClient, ignoreClient);
		}

		public static void CraftingData(int toClient = -1, int ignoreClient = -1)
		{
			if (Main.netMode == NetmodeID.SinglePlayer) //single player dosent care about packets
				return;

			ModPacket packet = ModLoader.GetMod("DragonVault").GetPacket();
			packet.Write("CraftingData");
			packet.Write(CraftingSystem.slots);
			packet.Write(CraftingSystem.stations.Count);

			for (int k = 0; k < CraftingSystem.stations.Count; k++)
			{
				ItemIO.Send(CraftingSystem.stations[k], packet);
			}

			packet.Send(toClient, ignoreClient);
		}
	}

	internal class NetPlayer : ModPlayer
	{
		public override void OnEnterWorld()
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
			{
				VaultNet.DataReq();
				VaultNet.CraftingDataRequest();
				VaultNet.OnJoinReq(0);
			}
		}
	}
}
