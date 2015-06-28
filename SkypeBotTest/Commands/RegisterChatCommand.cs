using System;
using System.Threading.Tasks;
using SKYPE4COMLib;

namespace SkypeBotTest.Commands
{
    class RegisterChatCommand : CommandBase
    {
        private readonly ChatMessage _message;
        private readonly Configuration _config;

        public RegisterChatCommand(Func<ChatMessage> message, Func<User> user, Configuration config) : base(user, config)
        {
            _message = message();
            _config = config;

            IsCommand("registerChat", "Register Chat For Shoutbox");
        }

        public override int Run(string[] remainingArguments)
        {
            _config.RegisteredChats.Add(_message.Chat.Name);
            _message.Chat.SendNickedMessage("Registered Chat: " + _message.Chat.Name);

            return 0;
        }

        public override int AccessLevel { get; } = 255;

        public override bool ShouldDisplay()
        {
            return base.ShouldDisplay() && !_config.RegisteredChats.Contains(_message.Chat.Name);
        }
    }
    class UnRegisterChatCommand : CommandBase
    {
        private readonly ChatMessage _message;
        private readonly Configuration _config;
        private readonly Chatbox _chatbox;

        public UnRegisterChatCommand(Func<ChatMessage> message, Func<User> user, Configuration config, Chatbox chatbox) : base(user, config)
        {
            _message = message();
            _config = config;
            _chatbox = chatbox;

            IsCommand("unregisterChat", "Unregister Chat For Shoutbox");
        }

        public override int Run(string[] remainingArguments)
        {
            _config.RegisteredChats.Remove(_message.Chat.Name);
            _message.Chat.SendNickedMessage("Unregistered Chat: " + _message.Chat.Name);
            _chatbox.Stop(_message.Chat);
            return 0;
        }

        public override int AccessLevel { get; } = 255;

        public override bool ShouldDisplay()
        {
            return base.ShouldDisplay() && _config.RegisteredChats.Contains(_message.Chat.Name);
        }
    }
}