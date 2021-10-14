using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using System.Linq;

namespace PlayerInfo
{
	[ApiVersion(2, 1)]

	public class PlayerInfo : TerrariaPlugin
	{
		public override Version Version
		{
			get { return new Version(1, 8); }
		}

		public override string Name
		{
			get { return "PlayerInfo"; }
		}

		public override string Author
		{
			get { return "Comdar"; }
		}

		public override string Description
		{
			get { return "Player info plugin"; }
		}

		public PlayerInfo(Main game)
			: base(game)
		{
			Order = +4;
		}

		public override void Initialize()
		{
			Commands.ChatCommands.Add(new Command("playerinfo.hpnmana", SetStats, "stats"));
			Commands.ChatCommands.Add(new Command("playerinfo.selfname", SelfName, "nick", "name"));
			Commands.ChatCommands.Add(new Command("playerinfo.checkinfo", CheckInfo, "checkinfo", "cinfo"));
		}

		void CheckInfo(CommandArgs args)
		{
			TSPlayer plr = args.Player;
			if (args.Parameters.Count != 1)
			{
				plr.SendErrorMessage("Invalid Syntax! Proper Syntax: /checkinfo <player>");
				return;
			}
			var foundPlr = TSPlayer.FindByNameOrID(args.Parameters[0]);
			if (foundPlr.Count == 0)
			{
				args.Player.SendErrorMessage("Invalid player!");
				return;
			}
			else {
				var iPlayer = foundPlr[0];
				string name = iPlayer.Name;
				var hp = iPlayer.TPlayer.statLifeMax2;
				var mana = iPlayer.TPlayer.statManaMax2;
				var currentHp = iPlayer.TPlayer.statLife;
				var currentMana = iPlayer.TPlayer.statMana;
				var groupName = iPlayer.Group.Name;
				var groupPrefix = iPlayer.Group.Prefix;
				string array = "Name: " + name + "\nHP: " + hp + ", Current HP: " + currentHp + "\nMana: " + mana + ", Current Mana: " + currentMana + "\nGroup: " + groupName + ", Group Prefix: " + groupPrefix;
				plr.SendMessage(array, Color.LightSteelBlue);
			}
		}

		void SetStats(CommandArgs args)
		{
			if (args.Parameters.Count < 3 || args.Parameters.Count > 4)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /stats <player> <health> <mana>");
				return;
			}
			int health = 0;
			int mana = 0;
			var foundPlr = TSPlayer.FindByNameOrID(args.Parameters[0]);
			if (foundPlr.Count == 0)
			{
				args.Player.SendErrorMessage("Invalid player!");
				return;
			}
			//            if (foundplr.Count > 1)
			//            {
			//                TShock.Utils.SendMultipleMatchError(args.Player, foundPlr.Select(p => p.Name));
			//                return;
			//            }
			else
			{
				var iPlayer = foundPlr[0];
				if (!int.TryParse(args.Parameters[1], out health))
				{
					if (health < 100)
					{
						args.Player.SendErrorMessage("Invalid Health Amount!");
						return;
					}
				}
				if (!int.TryParse(args.Parameters[2], out mana))
				{
					if (mana < 20)
					{
						args.Player.SendErrorMessage("Invalid Mana Amount!");
						return;
					}
				}
				iPlayer.TPlayer.statLifeMax = health;
				iPlayer.TPlayer.statManaMax = mana;
				NetMessage.SendData(16, -1, -1, null, iPlayer.Index, 0f, 0f, 0f, 0); // Sends Health Packet
				NetMessage.SendData(42, -1, -1, null, iPlayer.Index, 0f, 0f, 0f, 0); // Sends Mana Packet
				args.Player.SendSuccessMessage(string.Format("The Player's Stats have been set!"));
                iPlayer.SendSuccessMessage(string.Format("Your Stats have been modified!"));
            }
		}
		void SelfName(CommandArgs args)
		{
			if (args.Player == null) return;

			var plr = args.Player;
			if (args.Parameters.Count < 1)
			{
				args.Player.SendErrorMessage("Invalid syntax! Proper syntax: /nick <newname>");
				return;
			}
            string newName = string.Join(" ", args.Parameters).Trim();

			#region Checks
			if (newName.Length < 2)
			{
				args.Player.SendMessage("A name must be at least 2 characters long.", Color.DeepPink);
				return;
			}

			if (newName.Length > 20)
			{
				args.Player.SendMessage("A name must not be longer than 20 characters.", Color.DeepPink);
				return;
			}

			List<TSPlayer> SameName = TShock.Players.Where(player => (player != null && player.Name == newName)).ToList();
			if (SameName.Count > 0)
			{
				args.Player.SendMessage("This name is taken by another player.", Color.DeepPink);
				return;
			}
			#endregion Checks

			string oldName = plr.TPlayer.name;
			plr.TPlayer.name = newName;
			TShock.Utils.Broadcast(string.Format("{0} has changed his name to {1}.", oldName, newName), Color.DeepPink);
			plr.SendData(PacketTypes.PlayerInfo, newName, plr.Index);
		}
	}
}
