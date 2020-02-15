using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Xml;
using System.Xml.Linq;
using HtmlAgilityPack;

namespace Plutonium
{
    public partial class ConnectionWindow : Window
    {
        private readonly Socket _client;

        private readonly byte[] _clientBuffer;
        private readonly Socket _server;
        private readonly byte[] _serverBuffer;

        private string _clientMesage = "";
        private string _serverMessage = "";

        public ConnectionWindow(Socket client)
        {
            _clientBuffer = new byte[1024 * 32];
            _client = client;

            _serverBuffer = new byte[1024 * 32];
            _server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var socketAsyncEventArgs = new SocketAsyncEventArgs();
            socketAsyncEventArgs.RemoteEndPoint =
                new IPEndPoint(IPAddress.Parse(PlEnvironment.IpAddress), PlEnvironment.Port);
            socketAsyncEventArgs.Completed += ServerConnected;

            if (!_server.ConnectAsync(socketAsyncEventArgs))
            {
                ServerConnected(null, socketAsyncEventArgs);
            }

            InitializeComponent();

            Title = $"Подключение";
        }

        private void ServerConnected(object sender, SocketAsyncEventArgs e)
        {
            WaitClientData();
            WaitServerData();
        }

        private void WaitClientData()
        {
            var socketAsyncEventArgs = new SocketAsyncEventArgs();
            socketAsyncEventArgs.SetBuffer(_clientBuffer, 0, _clientBuffer.Length);
            socketAsyncEventArgs.Completed += ClientReceived;

            if (!_client.ReceiveAsync(socketAsyncEventArgs))
            {
                ClientReceived(null, socketAsyncEventArgs);
            }
        }

        private void WaitServerData()
        {
            var socketAsyncEventArgs = new SocketAsyncEventArgs();
            socketAsyncEventArgs.SetBuffer(_serverBuffer, 0, _serverBuffer.Length);
            socketAsyncEventArgs.Completed += ServerReceived;

            if (!_server.ReceiveAsync(socketAsyncEventArgs))
            {
                ServerReceived(null, socketAsyncEventArgs);
            }
        }

        private void ClientReceived(object sender, SocketAsyncEventArgs socketAsyncEventArgs)
        {
            try
            {
                if (!_client.Connected) return;

                var numReceivedBytes = socketAsyncEventArgs.BytesTransferred;

                if (numReceivedBytes > 0)
                {
                    var bytes = ByteUtil.ChompBytes(socketAsyncEventArgs.Buffer, 0, numReceivedBytes);

                    _server.SendAsync(bytes, SocketFlags.None);

                    var data = Encoding.UTF8.GetString(bytes);

                    _clientMesage += data;

                    if (_clientMesage.Contains((char) 0x00))
                    {
                        var messages = _clientMesage.Split((char) 0x00);

                        foreach (var message in messages)
                        {
                            if (message.Length == 0) continue;

                            WriteData(message, "SERVER");
                        }

                        _clientMesage = messages[^1];
                    }

                    WaitClientData();
                }
            }
            catch (Exception)
            {
                Disconnect();
            }
        }


        private void ServerReceived(object sender, SocketAsyncEventArgs socketAsyncEventArgs)
        {
            try
            {
                if (!_server.Connected) return;

                var numReceivedBytes = socketAsyncEventArgs.BytesTransferred;

                if (numReceivedBytes > 0)
                {
                    var bytes = ByteUtil.ChompBytes(socketAsyncEventArgs.Buffer, 0, numReceivedBytes);

                    _client.SendAsync(bytes, SocketFlags.None);

                    var data = Encoding.UTF8.GetString(bytes);

                    _serverMessage += data;

                    if (_serverMessage.Contains((char) 0x00))
                    {
                        var messages = _serverMessage.Split((char) 0x00);

                        foreach (var message in messages)
                        {
                            if (message.Length == 0) continue;

                            var unpack = ByteUtil.Unpack(message);
                            WriteData(unpack, "CLIENT");

                            if (unpack.StartsWith("<OK"))
                            {
                                var login = FindParam(unpack, "l");

                                SetTitle($"{login}");
                            }
                        }

                        _serverMessage = messages[^1];
                    }

                    WaitServerData();
                }
                else
                {
                    Disconnect();
                }
            }
            catch (Exception)
            {
                Disconnect();
            }
        }

        private void SendToClient(string data)
        {
            _client.SendAsync(Encoding.UTF8.GetBytes(data), SocketFlags.None);
        }

        private void SendToServer(string data)
        {
            _server.SendAsync(Encoding.UTF8.GetBytes(data), SocketFlags.None);
        }

        private void WriteData(string data, string type)
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(() => WriteData(data, type));
                return;
            }

            if (LogToServer.IsChecked != null && !LogToServer.IsChecked.Value && type == "SERVER")
                return;

            if (LogToClient.IsChecked != null && !LogToClient.IsChecked.Value && type == "CLIENT")
                return;

            try
            {
                var web = new HtmlDocument {OptionOutputOriginalCase = true};
                web.LoadHtml(data);

                var stringWriter = new StringWriter();
                var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings
                {
                    OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment, CloseOutput = false
                });

                web.Save(xmlWriter);

                var xElement = XElement.Parse("<root>" + stringWriter + "</root>");

                foreach (var element in xElement.Elements())
                {
                    foreach (var b in PrettyXmlConverter.RenderElement(type, element, 0))
                    {
                        Logs.Document.Blocks.Add(b);
                    }
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine(e); BAD PACKET
                //throw;
            }

            Logs.ScrollToEnd();
        }

        private void ClearLogs_Click(object sender, RoutedEventArgs e)
        {
            Logs.Document.Blocks.Clear();
        }

        private void SendToClient_Click(object sender, RoutedEventArgs e)
        {
            var data = ToClient.Text;
            SendToClient(data + (char) 0);
        }

        private void SendToServer_Click(object sender, RoutedEventArgs e)
        {
            var data = ToServer.Text;
            SendToServer(data + (char) 0);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Disconnect();
        }

        private void AlwaysOnTop_OnChecked(object sender, RoutedEventArgs e)
        {
            if (AlwaysOnTop.IsChecked != null) Topmost = AlwaysOnTop.IsChecked.Value;
        }

        private string FindParam(string data, string param)
        {
            var pFrom = data.IndexOf(param + "=\"") + (param + "=\"").Length;
            var data2 = data.Substring(pFrom);
            var pTo = data2.IndexOf("\"");

            return data2.Substring(0, pTo);
        }

        private void SetTitle(string title)
        {
            if (!CheckAccess())
                Dispatcher.Invoke(() => SetTitle(title));
            else
                Title = title;
        }

        private void Disconnect()
        {
            WriteData("Disconnect", "LOG");
            _server.Disconnect(false);
            _client.Disconnect(false);

            PlEnvironment.MainWindow.Disconnect(this);
        }

        private void DisableButtons()
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(DisableButtons);
                return;
            }

            SendToClientBtn.IsEnabled = false;
            SendToServerBtn.IsEnabled = false;
        }

        public void CloseConnection()
        {
            WriteData("CloseConnection", "LOG");

            DisableButtons();

            Disconnect();

            Close();
        }
    }
}