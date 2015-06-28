using System;
using SKYPE4COMLib;

namespace SkypeBotTest
{
    public static class ChatExtensions
    {
        public static void SendNickedMessage(this Chat chat, string message)
        {
            chat.SendMessage("LoEChat: " + message);
        }

        public static Chat Chat(this Func<ChatMessage> chatMessage)
        {
            return chatMessage().Chat;
        }
    }
}