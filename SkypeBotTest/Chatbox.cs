using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using HtmlAgilityPack;
using Nito.AsyncEx;
using SKYPE4COMLib;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace SkypeBotTest
{
    class Chatbox
    {
        private CancellationTokenSource _cancelTokenSource;
        private int _delay;
        private Regex _timestampRegex = new Regex("[ ]?\\[[^\\]]*\\]:", RegexOptions.Compiled);
        private Dictionary<string, Chat> _chats = new Dictionary<string, Chat>();
        private bool _first = true;
        private WebBrowserConsole _sendMessageWebBrowser;
        private string _id;

        public Chatbox(Configuration config)
        {
            _delay = config.Delay;
            _sendMessageWebBrowser = new WebBrowserConsole();
        }

        public void Start(Chat chat)
        {
            if (_chats.ContainsKey(chat.Name))
                return;
            _chats.Add(chat.Name, chat);
            if (_cancelTokenSource != null)
                return;
            _cancelTokenSource = new CancellationTokenSource();
            Task.Factory.Run(() => DoWork());
        }

        async Task DoWork()
        {
            _first = true;
            while (!_cancelTokenSource.IsCancellationRequested)
            {
                try
                {
                    var text = await GetChatBoxText();
                    if (text != null && !_first)
                    {
                        foreach (var chat in _chats)
                        {
                            chat.Value.SendMessage(text);
                        }
                    }
                    _first = false;
                    await Task.Delay(_delay);
                }
                catch (Exception)
                {
                    
                    foreach (var chat in _chats)
                    {
                        chat.Value.SendNickedMessage("Fatal Error Talking to Chatbox");
                    }
                    await Task.Delay(10000);
                }
            }
        }

        private async Task<string> GetChatBoxText()
        {
            return await Program._aprt.Run(async () =>
            {
                ShoutboxMessageGroup s;
                try
                {
                    s = await GetChatBoxCData();
                }
                catch (Exception ex)
                {
                    return null;
                    throw ex;
                }
                if (s == null)
                    return null;
                var sb = new StringBuilder();
                foreach (var message in s)
                {
                    sb.AppendLine(message.Username + ": \n          " + message.Message);
                }
                var s1 = sb.ToString();
                return s1;
            }, new CancellationToken());
        }

        private async Task<ShoutboxMessageGroup> GetChatBoxCData()
        {
            var innerText = await Program._aprt.Run(async () =>
            {
                await Program._frm.WebBrowser.NavigateAsync(
                "http://forum.legendsofequestria.com/index.php?action=shoutbox;sa=get;xml;row=20",
                new CancellationToken());

                return Program._frm.WebBrowser.Document.Body.InnerText;
            }, new CancellationToken());
            if (innerText == null)
                return null;
            return new ShoutboxMessageGroup(innerText);
        }

        public void Stop(Chat chat)
        {
            if (!_chats.ContainsKey(chat.Name))
                return;
            _chats.Remove(chat.Name);
            if (_chats.Count != 0)
                return;
            if (_cancelTokenSource == null)
                return;
            _cancelTokenSource.Cancel();
            _cancelTokenSource = null;
        }

        public async Task SendMessage(string message)
        {
            if (_id == null)
            {
                var document = await _sendMessageWebBrowser.GetWebPage("http://forum.legendsofequestria.com/index.php?action=shoutbox", new CancellationToken());
                _id = new Regex("Shoutbox_SentMsg\\(\'(?<code>[^\']*)\'\\)").Match(document).Groups["code"].Value;
            }
            var postdata = "msg=" + HttpUtility.HtmlEncode(message);

            var a = new ASCIIEncoding();

            byte[] byte1 = a.GetBytes(postdata);
            await _sendMessageWebBrowser.Run(async (wb) =>
            {
                await wb.NavigateAsync("http://forum.legendsofequestria.com/index.php?action=shoutbox;sa=send;sesc=" + _id + ";xml;row=20", "", byte1, "Content-Type: application/x-www-form-urlencoded", new CancellationToken());
            });
            //Shoutbox_SentMsg('9c31bc4384abb660e7d0d612926b4b93');

        }
    }

    class ShoutboxMessageGroup : IReadOnlyList<ShoutboxMessage>
    {
        private readonly List<ShoutboxMessage> _messages = new List<ShoutboxMessage>();

        public ShoutboxMessageGroup(string rawHtml)
        {
            var doc = GetDocument(rawHtml);
            foreach (HtmlNode tr in doc.DocumentNode.SelectNodes("//tr"))
            {
                if (tr.Id == "shoutbox_msgs")
                    continue;
                _messages.Add(new ShoutboxMessage(tr));
            }
        }

        private static HtmlDocument GetDocument(string rawHtml)
        {
            var substring = rawHtml.Substring(rawHtml.IndexOf("<![CDATA[") + 10);
            var s = substring.Substring(0, substring.IndexOf("]]>"));
            var doc = new HtmlDocument();
            doc.LoadHtml(s);
            return doc;
        }

        public IEnumerator<ShoutboxMessage> GetEnumerator()
        {
            return _messages.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => _messages.Count;
        
        public ShoutboxMessage this[int index] => _messages[index];
        
    }

    class ShoutboxMessage
    {
        private readonly string _username;
        private readonly string _message;

        public ShoutboxMessage(HtmlNode messageNode)
        {
            var leftCell = messageNode.ChildNodes[0];
            var rightCell = messageNode.ChildNodes[1];
            var leftUsername = leftCell.SelectNodes(leftCell.XPath + "/a");

            HtmlNodeCollection images;
            HtmlNodeCollection links;
            

            if (leftUsername != null && leftUsername.Count == 1)
            {
                SetCells(rightCell, rightCell.XPath + "/span", out images, out links);
                _username = leftUsername[0].InnerText;
                _message = rightCell.InnerText;
            }
            else
            {
                var xpath = rightCell.XPath + "/span/span";
                var rightUsername = rightCell.SelectNodes(xpath);
                SetCells(rightCell, rightUsername[0].XPath, out images, out links);
                _message = rightUsername[0].InnerText;
                _username = rightCell.InnerText.Substring(0, rightCell.InnerText.IndexOf(rightUsername[0].InnerText));
            }

            _username = _username.HtmlDecode();
            _message = _message.HtmlDecode();
        }

        private static void SetCells(HtmlNode rightCell, string xpath, out HtmlNodeCollection images, out HtmlNodeCollection links)
        {
            images = rightCell.SelectNodes(xpath + "/img");
            if (images != null)
                foreach (var node in images)
                {

                    //var test = HtmlTextNode.CreateNode(node.Attributes["alt"].Value);
                    node.InnerHtml = node.Attributes["alt"].Value;
                    //node.ParentNode.ReplaceChild(node, HtmlTextNode.CreateNode(node.Attributes["alt"].Value));
                }
            links = rightCell.SelectNodes(xpath + "/a");
            if (links != null)
                foreach (var node in links)
                {
                    if (node.InnerText.StartsWith("http"))
                    {
                        //var test = HtmlTextNode.CreateNode(node.Attributes["href"].Value);
                        node.InnerHtml = HttpUtility.HtmlEncode(node.Attributes["href"].Value);
                        //node.ParentNode.ReplaceChild(node, test);
                    }
                        
                }
        }

        public string Username => _username;
        public string Message => _message;

    }

    public static class Extensions
    {
        public static string HtmlDecode(this string @this)
        {
            return HttpUtility.HtmlDecode(@this);
        }
    }
}