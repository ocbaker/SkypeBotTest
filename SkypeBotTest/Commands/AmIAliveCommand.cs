using System;
using System.Threading.Tasks;
using SKYPE4COMLib;

namespace SkypeBotTest.Commands
{
    class AmIAliveCommand : CommandBase
    {
        private readonly Func<ChatMessage> _message;
        
        public AmIAliveCommand(Func<ChatMessage> message, Func<User> user, Configuration config) : base(user, config)
        {
            _message = message;
            IsCommand("amIAlive", "Check to see if  the server is alive and knows who you are");
        }

        public override int Run(string[] remainingArguments)
        {
            _message().Chat.SendNickedMessage("Yes I am " + _message().Sender.Handle);

            return 0;
        }
    }
}