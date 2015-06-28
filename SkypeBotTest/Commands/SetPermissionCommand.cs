using System;
using System.Threading.Tasks;
using SKYPE4COMLib;

namespace SkypeBotTest.Commands
{
    class SetPermissionCommand : CommandBase
    {
        private readonly ChatMessage _message;
        private readonly Configuration _config;
        private readonly SkypeCommander _commander;


        public SetPermissionCommand(Func<ChatMessage> message, Func<User> user, Configuration config, SkypeCommander commander) : base(user, config)
        {
            _message = message();
            _config = config;
            _commander = commander;

            IsCommand("setPermission");

            HasAdditionalArguments(2, "<username> <permissionlevel>");
        }

        public override int Run(string[] remainingArguments)
        {
            var username = remainingArguments[0];
            var accessLevel = Convert.ToInt32(remainingArguments[1]);
            var oldAccessLevel = _config.AccessLevels.ContainsKey(_message.Sender.Handle) ? _config.AccessLevels[_message.Sender.Handle] : 0;
            if (username == "oliver_c.baker")
            {
                _message.Chat.SendNickedMessage("I'm afraid I can't left you do that " + _message.Sender.FullName);
                return 0;
            }
            if(!_config.AccessLevels.ContainsKey(username))
                _config.AccessLevels.Add(username, accessLevel);
            else
                _config.AccessLevels[username] = accessLevel;
            _message.Chat.SendNickedMessage("Set User " + username + " to " + accessLevel);

            if (accessLevel == 255)
            {
                _commander.DoOnAdminChats(chat => chat.SendMessage("/add " + username));
            }
            else
            {
                if(oldAccessLevel == 255)
                    _commander.DoOnAdminChats(chat => chat.Kick(username));
            }

            return 0;
        }

        public override int AccessLevel { get; } = 255;
    }
}