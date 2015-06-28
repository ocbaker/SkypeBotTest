using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using ManyConsole;
using Nito.AsyncEx;
using Nito.AsyncEx.Internal.PlatformEnlightenment;
using SimpleInjector;
using SkypeBotTest.Commands;
using SKYPE4COMLib;
using Application = System.Windows.Forms.Application;

namespace SkypeBotTest
{
    class Program
    {
        private static SkypeCommander _skypeCommander;
        private static Configuration _config;
        public static MessageLoopApartment _aprt;
        public static Form1 _frm;
        private static Chatbox _chatbox;

        static void Main(string[] args)
        {
            AsyncContext.Run(() => Work());
        }

        private static async Task Work()
        {
            _config = Configuration.LoadConfig("config.json");
            _chatbox = new Chatbox(_config);
            _skypeCommander = new SkypeCommander(_config);
            _skypeCommander.SetUp();
            //_skypeCommander._skype.SendMessage("princess_bot", "[b]test[/b]");

            _aprt = new MessageLoopApartment();
            _aprt.Invoke(() =>
            {
                Application.EnableVisualStyles();
                _frm = new Form1();
                _frm.Show();
            });
            _skypeCommander.OnMessageRecieved((message, status) =>
            {
                Console.WriteLine("Message Recieved: " + message.Body);
                if (message.Body.IndexOf("|") == 0 && message.Body.Length != 1)
                {
                    Container container = CreateContainer(message, status);

                    using (var sw = new StringWriter())
                    {
                        var i = -1;

                        _skypeCommander.DoOnAdminChats(chat => chat.SendMessage(message.Sender.Handle + " - " + message.Body));
                        
                        try
                        {
                            var commands = container.GetAllInstances<CommandBase>().Where(x => x.ShouldDisplay());

                            var commandLineToArgs = CommandLineToArgs(message.Body.Substring(1));
                            foreach (var command in commandLineToArgs)
                            {
                                Console.WriteLine(command);
                            }
                            i = ConsoleCommandDispatcher.DispatchCommand(
                                commands,
                                commandLineToArgs, sw, false);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            i = 2;
                        }

                        if (i == -2)
                        {
                            message.Chat.SendNickedMessage("Error Processing Request");
                        }
                        else
                        {
                            if(i != 0)
                                message.Chat.SendNickedMessage(sw.GetStringBuilder().ToString().Replace("SkypeBotTest.vshost.exe", ""));
                        }
                        _config.Save();
                    }
                    
                }
            });

            foreach (var chatHandle in _config.RegisteredChats)
            {
                _chatbox.Start(_skypeCommander._skype.Chat[chatHandle]);
            }

            while (true)
            {
                Console.WriteLine("Waiting For Input!");
                Console.ReadLine();
            }
        }

        private static Container CreateContainer(ChatMessage message, TChatMessageStatus status)
        {
            var container = new Container();

            container.RegisterSingle<Func<ChatMessage>>(() => () => message);
            container.RegisterSingle<Func<User>>(() => () => message.Sender);
            container.RegisterSingle(() => _skypeCommander); 
            container.RegisterSingle(() => _config);
            container.RegisterSingle(() => _chatbox);
            container.RegisterPlugins<CommandBase>(new []{ typeof(CommandBase).Assembly});

            return container;
        }

        [DllImport("shell32.dll", SetLastError = true)]
        static extern IntPtr CommandLineToArgvW(
    [MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);

        public static string[] CommandLineToArgs(string commandLine)
        {
            int argc;
            var argv = CommandLineToArgvW(commandLine, out argc);
            if (argv == IntPtr.Zero)
                throw new System.ComponentModel.Win32Exception();
            try
            {
                var args = new string[argc];
                for (var i = 0; i < args.Length; i++)
                {
                    var p = Marshal.ReadIntPtr(argv, i * IntPtr.Size);
                    args[i] = Marshal.PtrToStringUni(p);
                }

                return args;
            }
            finally
            {
                Marshal.FreeHGlobal(argv);
            }
        }
    }
}
