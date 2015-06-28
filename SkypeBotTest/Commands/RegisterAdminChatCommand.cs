using System;
using System.Threading.Tasks;
using SKYPE4COMLib;

namespace SkypeBotTest.Commands
{
    class RegisterAdminChatCommand : CommandBase
    {
        private readonly ChatMessage _message;
        private readonly Configuration _config;


        public RegisterAdminChatCommand(Func<ChatMessage> message, Func<User> user, Configuration config) : base(user, config)
        {
            _message = message();
            _config = config;

            IsCommand("registerAdminChat", "Register Chat For Shoutbox (Logs everything)");
        }

        public override int Run(string[] remainingArguments)
        {
            _config.RegisteredAdminChats.Add(_message.Chat.Name);
            _message.Chat.SendNickedMessage("Registered Admin Chat: " + _message.Chat.Name);

            return 0;
        }

        public override int AccessLevel { get; } = 255;
    }
}