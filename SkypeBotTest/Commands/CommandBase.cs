using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManyConsole;
using Nito.AsyncEx;
using SKYPE4COMLib;

namespace SkypeBotTest.Commands
{
    abstract class CommandBase : ConsoleCommand
    {
        private readonly Configuration _config;
        private User _user;

        public CommandBase(Func<User> user, Configuration config)
        {
            _config = config;
            _user = user();
        }

        public override int Run(string[] remainingArguments)
        {
            return AsyncContext.Run(() => RunAsync(remainingArguments));
        }

        protected virtual Task<int> RunAsync(string[] remainingArguments)
        {
            throw new NotImplementedException();
        }

        public virtual int AccessLevel { get; } = 0;

        public virtual bool ShouldDisplay()
        {
            return AccessLevel <=
                   (_config.AccessLevels.ContainsKey(_user.Handle)
                       ? _config.AccessLevels[_user.Handle]
                       : 0);
        }
    }
}
