﻿using Messages.Client;
using Mirror;

namespace Messages.Client.Admin
{
	public class AdminReplyMessage : ClientMessage<AdminReplyMessage.NetMessage>
	{
		public struct NetMessage : NetworkMessage
		{
			public string Message;
		}

		public override void Process(NetMessage msg)
		{
			UIManager.Instance.adminChatWindows.adminPlayerChat.ServerAddChatRecord(msg.Message, SentByPlayer);
		}

		public static NetMessage Send(string message)
		{
			NetMessage msg = new NetMessage
			{
				Message = message
			};

			Send(msg);
			return msg;
		}
	}
}
