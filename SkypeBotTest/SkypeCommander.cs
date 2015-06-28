using System;
using SKYPE4COMLib;

namespace SkypeBotTest
{
    public class SkypeCommander
    {
        private readonly Configuration _config;
        public Skype _skype;

        public SkypeCommander(Configuration config)
        {
            _config = config;
        }

        public void SetUp()
        {
            _skype = new Skype();
            if (!_skype.Client.IsRunning)
            {
                // start minimized with no splash screen
                _skype.Client.Start(true, true);
            }

            // wait for the client to be connected and ready
            _skype.Attach(7, true);
        }

        public void OnMessageRecieved(Action<ChatMessage, TChatMessageStatus> action)
        {
            _skype.MessageStatus += (message, status) =>
            {
                Console.WriteLine(message.Body);
                Console.WriteLine(message.ChatName);
                if (TChatMessageStatus.cmsRead == status || TChatMessageStatus.cmsSending == status)
                {
                    return;
                }
                action(message, status);
            };
        }

        public void SendMessage(string user, string message)
        {
            _skype.SendMessage(user, message);
        }

        public void DoOnAdminChats(Action<Chat> action)
        {
            foreach (var chat in _config.RegisteredAdminChats)
            {
                action(_skype.Chat[chat]);
            }
        }
    }
}