using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Windows;

namespace Plutonium
{
    public partial class MainWindow : Window
    {
        private readonly List<ConnectionWindow> _connectionWindows = new List<ConnectionWindow>();

        private readonly List<string> _serverList =
            new List<string> {"city1", "city2", "chat"};

        private TcpListener _tcpListener;

        public MainWindow()
        {
            PlEnvironment.MainWindow = this;

            InitializeComponent();

            foreach (var s in _serverList)
                ServerList.Items.Add(s + ".timezero.ru");
        }

        private void ServerListConnect_Click(object sender, RoutedEventArgs e)
        {
            ConnectButton.IsEnabled = false;

            if (ServerList.Text.Length == 0)
            {
                MessageBox.Show("Incomplete data!", "Try again");
                ConnectButton.IsEnabled = true;
                return;
            }

            try
            {
                PlEnvironment.Host = ServerList.Text;
                HostUtil.DeleteHost(PlEnvironment.Host);

                var hosts = Dns.GetHostAddresses(PlEnvironment.Host);

                PlEnvironment.IpAddress = hosts[0].ToString();

                HostUtil.AddHost(PlEnvironment.Host);

                Start();

                ConnectButton.Content = "Можно входить в игру";
                MessageBox.Show("Теперь Вы можете войти в игру.", "Программа запущена");
            }
            catch
            {
                MessageBox.Show("Не удалось подключиться к серверу", "Ошибка");
                ConnectButton.IsEnabled = true;
            }
        }

        private void Start()
        {
            _tcpListener = new TcpListener(IPAddress.Any, PlEnvironment.Port);
            _tcpListener.Start();

            WaitForDataListener();
        }

        private void WaitForDataListener()
        {
            _tcpListener.BeginAcceptSocket(StartupConnection, null);
        }

        private void StartupConnection(IAsyncResult iAr)
        {
            var client = _tcpListener.EndAcceptSocket(iAr);

            void StartConnection()
            {
                var connectionWindow = new ConnectionWindow(client);
                _connectionWindows.Add(connectionWindow);
                connectionWindow.Show();
            }

            if (!CheckAccess())
            {
                Dispatcher.Invoke(StartConnection);
            }
            else
            {
                StartConnection();
            }

            WaitForDataListener();
        }

        public void Disconnect(ConnectionWindow connectionWindow)
        {
            _connectionWindows.Remove(connectionWindow);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            lock (_connectionWindows)
            {
                foreach (var connection in _connectionWindows)
                    connection?.CloseConnection();
            }

            HostUtil.DeleteHost(PlEnvironment.Host);

            Environment.Exit(0);
        }
    }
}