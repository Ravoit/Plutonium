using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace Plutonium
{
    public static class HostUtil
    {
        private static string FilePath =>
            Environment.GetEnvironmentVariable("SystemRoot") + "\\System32\\drivers\\etc\\hosts";

        public static void AddHost(string host)
        {
            var data = "127.0.0.1 " + host;
            var fileName = FilePath;

            try
            {
                if (File.ReadAllLines(fileName).Contains(data))
                    return;

                File.AppendAllText(fileName, Environment.NewLine + data);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Run as admin", "No admin rights");
                Environment.Exit(0);
            }
        }

        public static void DeleteHost(string host)
        {
            var data = "127.0.0.1 " + host;
            var fileName = FilePath;

            try
            {
                File.WriteAllLines(fileName,
                    File.ReadLines(fileName).Where(l => l != data).ToList());
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Run as admin", "No admin rights");
                Environment.Exit(0);
            }
        }
    }
}