using System;
using System.Threading.Tasks;
using SKYPE4COMLib;

namespace SkypeBotTest.Commands
{
    class SendChatMessageCommand : CommandBase
    {
        private readonly ChatMessage _message;
        private readonly Configuration _config;
        private readonly Chatbox _chatbox;

        public SendChatMessageCommand(Func<ChatMessage> message, Func<User> user, Configuration config, Chatbox chatbox) : base(user, config)
        {
            _message = message();
            _config = config;
            _chatbox = chatbox;

            IsCommand("m", "Send a message to chat!");
            HasAdditionalArguments(null);
        }

        protected override async Task<int> RunAsync(string[] remainingArguments)
        {
            await _chatbox.SendMessage(string.Join(" ", remainingArguments));
            return 0;
        }

        public override int AccessLevel { get; } = 255;

        public override bool ShouldDisplay()
        {
            return base.ShouldDisplay() && _message.Sender.Handle == "oliver_c.baker";
        }
    }
}