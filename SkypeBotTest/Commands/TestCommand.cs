using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;
using SKYPE4COMLib;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace SkypeBotTest.Commands
{
    class TestCommand : CommandBase
    {
        private readonly ChatMessage _message;
        private readonly Configuration _config;


        public TestCommand(Func<ChatMessage> message, Func<User> user, Configuration config) : base(user, config)
        {
            _message = message();
            _config = config;

            IsCommand("test", "test the webbrowser");
        }

        protected override async Task<int> RunAsync(string[] remainingArguments)
        {
            var text = await Program._aprt.Run(async () =>
            {
                await Program._frm.WebBrowser.NavigateAsync(
                    "http://forum.legendsofequestria.com/index.php?action=shoutbox;sa=get;xml;row=0;restart",
                    new CancellationToken());
                var innerText = Program._frm.WebBrowser.Document.Body.InnerText;
                var substring = innerText.Substring(innerText.IndexOf("<![CDATA[") + 10);
                var s = substring.Substring(0, substring.IndexOf("]]>"));
                var doc = new HtmlDocument();
                Console.WriteLine(s);
                doc.LoadHtml(s);
                var sb = new StringBuilder();
                foreach (HtmlNode tr in doc.DocumentNode.SelectNodes("//tr"))
                {
                    sb.AppendLine(tr.InnerText);
                }
                return sb.ToString();
            }, new CancellationToken());
            _message.Chat.SendNickedMessage("\n" + text);

            return 0;
        }

        public override int AccessLevel { get; } = 255;
    }

    //class WatchChatCommand : CommandBase
    //{
    //    private readonly ChatMessage _message;
    //    private readonly Configuration _config;
    //    private readonly Chatbox _chatbox;


    //    public WatchChatCommand(Func<ChatMessage> message, Func<User> user, Configuration config, Chatbox chatbox) : base(user, config)
    //    {
    //        _message = message();
    //        _config = config;
    //        _chatbox = chatbox;

    //        IsCommand("watchChat", "Watch The Chat");
    //    }

    //    public override int Run(string[] remainingArguments)
    //    {
    //        _message.Chat.SendNickedMessage("Watching chat");
            
    //        _chatbox.Start(_message.Chat);

    //        return 0;
    //    }

    //    public override int AccessLevel { get; } = 255;
    //}
}