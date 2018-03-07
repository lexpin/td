﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Td = Telegram.Td;
using TdApi = Telegram.Td.Api;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace TdApp
{
    public sealed partial class MainPage : Page
    {
        public System.Collections.ObjectModel.ObservableCollection<string> Items { get; set; }

        private static MyClientResultHandler _handler;

        public MainPage()
        {
            this.InitializeComponent();

            Items = new System.Collections.ObjectModel.ObservableCollection<string>();
            _handler = new MyClientResultHandler(this);

            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    Td.Log.SetFilePath(Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "log"));
                    _client = Td.Client.Create(_handler);
                    var parameters = new TdApi.TdlibParameters();
                    parameters.DatabaseDirectory = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
                    parameters.UseSecretChats = true;
                    parameters.UseMessageDatabase = true;
                    parameters.ApiId = 94575;
                    parameters.ApiHash = "a3406de8d171bb422bb6ddf3bbd800e2";
                    parameters.SystemLanguageCode = "en";
                    parameters.DeviceModel = "en";
                    parameters.SystemVersion = "en";
                    parameters.ApplicationVersion = "1.0.0";
                    _client.Send(new TdApi.SetTdlibParameters(parameters), null);
                    _client.Send(new TdApi.CheckDatabaseEncryptionKey(), null);
                    _client.Run();
                }
                catch (Exception ex)
                {
                    Print(ex.ToString());
                }
            });
        }

        public void Print(String str)
        {
            var delayTask = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Items.Insert(0, str.Substring(0, Math.Min(1024, str.Length)));
            });
        }

        private static Td.Client _client;

        private void AcceptCommand(String command)
        {
            Input.Text = string.Empty;
            Items.Insert(0, string.Format(">>{0}", command));
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var command = Input.Text;

            if (command.StartsWith("DESTROY"))
            {
                AcceptCommand("Destroy");
                _client.Send(new TdApi.Destroy(), _handler);
            }
            else if (command.StartsWith("lo"))
            {
                AcceptCommand("LogOut");
                _client.Send(new TdApi.LogOut(), _handler);
            }
            else if (command.StartsWith("gas"))
            {
                AcceptCommand(command);
                _client.Send(new TdApi.GetAuthorizationState(), _handler);
            }
            else if (command.StartsWith("sap"))
            {
                var args = command.Split(" ".ToCharArray(), 2);
                AcceptCommand(command);
                _client.Send(new TdApi.SetAuthenticationPhoneNumber(args[1], false, false), _handler);
            }
            else if (command.StartsWith("cac"))
            {
                var args = command.Split(" ".ToCharArray(), 2);
                AcceptCommand(command);
                _client.Send(new TdApi.CheckAuthenticationCode(args[1], String.Empty, String.Empty), _handler);
            }
            else if (command.StartsWith("cap"))
            {
                var args = command.Split(" ".ToCharArray(), 2);
                AcceptCommand(command);
                _client.Send(new TdApi.CheckAuthenticationPassword(args[1]), _handler);
            }
            else if (command.StartsWith("gco"))
            {
                var args = command.Split(" ".ToCharArray(), 2);
                AcceptCommand(command);
                _client.Send(new TdApi.SearchContacts(), _handler);
            }
            else if (command.StartsWith("df"))
            {
                var args = command.Split(" ".ToCharArray(), 2);
                AcceptCommand(command);
                _client.Send(new TdApi.DownloadFile(Int32.Parse(args[1]), 1), _handler);
            }
            else if (command.StartsWith("bench"))
            {
                var args = command.Split(" ".ToCharArray(), 2);
                AcceptCommand(command);
                var cnt = Int32.Parse(args[1]);
                var handler = new BenchSimpleHandler(this, cnt);
                for (int i = 0; i < cnt; i++)
                {
                    _client.Send(new TdApi.TestSquareInt(123), handler);
                }
            }
        }
    }

    class MyClientResultHandler : Td.ClientResultHandler
    {
        private MainPage _page;

        public MyClientResultHandler(MainPage page)
        {
            _page = page;
        }

        public void OnResult(TdApi.BaseObject obj)
        {
            var str = obj.ToString();
            _page.Print(str);
        }
    }

    class BenchSimpleHandler : Td.ClientResultHandler
    {
        private MainPage _page;
        private int _cnt;

        public BenchSimpleHandler(MainPage page, int cnt)
        {
            _page = page;
            _cnt = cnt;
        }

        public void OnResult(TdApi.BaseObject obj)
        {
            _cnt--;
            if (_cnt == 0)
            {
                _page.Print("DONE");
            }
        }
    }
}
