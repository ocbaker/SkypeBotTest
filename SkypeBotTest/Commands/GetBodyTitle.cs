using System;
using System.Threading;
using System.Threading.Tasks;
using SKYPE4COMLib;

namespace SkypeBotTest.Commands
{
    class GetBodyTitle : CommandBase
    {
        private readonly Func<ChatMessage> _message;


        public GetBodyTitle(Func<ChatMessage> message, Func<User> user, Configuration config) : base(user, config)
        {
            _message = message;
            IsCommand("getPageTitle", "gets the title of any webpage!");
            HasAdditionalArguments(1, "<url>");
        }

        protected override async Task<int> RunAsync(string[] remainingArguments)
        {
            WebBrowserConsole wbc = new WebBrowserConsole();

            await wbc.GetWebPage(remainingArguments[0], new CancellationToken());

            await wbc.Run((wbi) =>
            {
                _message().Chat.SendNickedMessage(wbi.DocumentTitle);
            });

            return 0;
        }
    }
}