using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SkypeBotTest
{
    class WebBrowserConsole : IDisposable
    {
        private WebBrowser _webBrowser;
        private MessageLoopApartment _apartment;

        public WebBrowserConsole()
        {
            _apartment = new MessageLoopApartment();
            _webBrowser = _apartment.Invoke(() => new WebBrowser());
        }

        public Task<string> GetWebPage(string url, CancellationToken tokenSource)
        {
            return _apartment.Run(() =>
                _webBrowser.NavigateAsync(url, tokenSource), tokenSource);
        }

        public void Invoke(Action<WebBrowser> act)
        {
            _apartment.Invoke(() => act(_webBrowser));
        }

        public Task Run(Action<WebBrowser> act)
        {
            return _apartment.Run(() => act(_webBrowser), new CancellationToken());
        }
        public Task<TReturn> Run<TReturn>(Func<WebBrowser, TReturn> act)
        {
            return _apartment.Run(() => act(_webBrowser), new CancellationToken());
        }
        public Task<TReturn> Run<TReturn>(Func<WebBrowser, Task<TReturn>> act)
        {
            return _apartment.Run(() => act(_webBrowser), new CancellationToken());
        }

        public void Dispose()
        {
            _apartment.Invoke(() => _webBrowser.Dispose());
            _apartment.Dispose();
        }
    }
}