using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Converters;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace beforewindeploy_custom_recovery
{
    /// <summary>
    /// Interaction logic for ComponentSelection.xaml
    /// </summary>
    public partial class ComponentSelection : Page
    {
        static async Task<bool> IsWiFiConnected()
        {
            try
            {
                Ping ping = new Ping();
                PingReply reply = await ping.SendPingAsync("192.168.0.1");
                if (reply.Status == IPStatus.Success)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        private async Task Delay(int ms)
        {
            await Task.Delay(ms);
        }

        private List<string> thingsToDo = new List<string>();
        private char localDriveLetter;

        private async void ConnectToWiFi()
        {
            bool networkDone = false;
            while (networkDone == false)
            {
                try
                {
                    using (Process process = new Process())
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo
                        {
                            FileName = "netsh",
                            RedirectStandardInput = true,
                            RedirectStandardOutput = true,
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            RedirectStandardError = true
                        };

                        process.StartInfo = startInfo;
                        process.Start();
                        Directory.CreateDirectory(@"C:\SGBono\Windows 11 Debloated");
                        File.WriteAllText(@"C:\SGBono\Windows 11 Debloated\SGBono Internal.xml", Properties.Resources.SGBono);
                        process.StandardInput.WriteLine("wlan add profile filename=\"C:\\SGBono\\Windows 11 Debloated\\SGBono Internal.xml\"");
                        process.StandardInput.WriteLine($"wlan connect name=\"SGBono Internal\" ssid=\"SGBono Internal\" interface=\"Wi-Fi\"");
                        process.StandardInput.Close();

                        ProcessStartInfo connectInfo = new ProcessStartInfo
                        {
                            FileName = "netsh",
                            Arguments = "wlan connect name=\"SGBono Internal\"",
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };

                        Process connectProcess = new Process { StartInfo = connectInfo };
                        connectProcess.Start();
                        connectProcess.WaitForExit();

                        int attempts = 0;
                        while (!await IsWiFiConnected())
                        {
                            if (attempts == 31)
                            {
                                throw new Exception("The network connection timed out.");
                            }
                            else
                            {
                                await Delay(500);
                                attempts++;
                            }
                        }

                        process.WaitForExit();
                        break;
                    }
                }
                catch (Exception ex)
                {
                    ErrorScreen errorScreen = new ErrorScreen(ex.Message);
                    this.Visibility = Visibility.Visible;
                    grid.Visibility = Visibility.Collapsed;
                    frame.Content = errorScreen;
                }
            }
        }

        public ComponentSelection()
        {
            InitializeComponent();
            this.Visibility = Visibility.Hidden;
            driversCheckbox.Visibility = Visibility.Collapsed;
            Applications.Visibility = Visibility.Collapsed;
            Main2();
        }

        private async void Main2()
        {
            try
            {
                await Delay(500);
                //GPU driver detection (Assuming that if GPU driver is present, all drivers are present)
                bool driversPresent = true;
                using (var searcher1 = new ManagementObjectSearcher("select * from Win32_VideoController"))
                {
                    foreach (ManagementObject obj in searcher1.Get())
                    {
                        if (obj["Name"].ToString() == "Microsoft Basic Display Adapter")
                        {
                            driversCheckbox.Visibility = Visibility.Visible;
                            driversPresent = false;
                            break;
                        }
                    }
                }

                if(driversPresent == true)
                {
                    fixListBox.Items.Remove(driversCheckbox);
                }

                //Check if software is present on USB
                for (char c = 'C'; c <= 'Z'; c++)
                {
                    try
                    {
                        if (Directory.Exists(c + @":\Software"))
                        {
                            onlineOfflineLabel.Content = "OS Recovery will use files available on this drive.";
                            localDriveLetter = c;
                            break;
                        }
                        else if (c == 'Z')
                        {
                            if (await FallbackToOnline() == 1) throw new Exception("Stop execution");
                        }
                    }
                    catch
                    {
                        if (c == 'Z')
                        {
                            if (await FallbackToOnline() == 1) throw new Exception("Stop execution");
                        }
                    }
                }
                ApplicationsCheck();
            }
            catch (Exception ex)
            {
                if (ex.Message == "Stop execution")
                {
                    return;
                }
            }
        }

        private void ApplicationsCheck()
        {
            RegistryKey uninstallKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall");
            string path = "";
            if ((string)onlineOfflineLabel.Content == "OS Recovery will use files available on this drive.")
            {
                path = $@"{localDriveLetter}:\Software\ProgramsList.xml";
            } 
            else if ((string)onlineOfflineLabel.Content == "OS Recovery will attempt to download required files from the server.")
            {
                path = @"Y:\ProgramsList.xml";
            }

            XDocument programsList = XDocument.Load(path);
            
            foreach (var element in programsList.Root.Elements())
            {
                string programName = element.Element("name").Value;
                bool isProgramFound = false;

                foreach (var subkeyName in uninstallKey.GetSubKeyNames())
                {
                    using (var subKey = uninstallKey.OpenSubKey(subkeyName))
                    {
                        var displayName = Convert.ToString(subKey.GetValue("DisplayName"));
                        if (displayName.Contains(programName))
                        {
                            isProgramFound = true;
                            break;
                        }
                    }
                }

                if (!isProgramFound)
                {
                    CheckBox checkBox = new CheckBox
                    {
                        Content = programName,
                        FontFamily = new FontFamily("Segoe UI Variable Text"),
                        IsChecked = true
                    };
                    thingsToDo.Add(Convert.ToString(checkBox.Content));
                    checkBox.Checked += (object sender, RoutedEventArgs e) =>
                    {
                        if (checkBox.IsChecked == true)
                        {
                            thingsToDo.Add(Convert.ToString(checkBox.Content));
                        }
                        else
                        {
                            if (thingsToDo.Contains(Convert.ToString(checkBox.Content)))
                            {
                                thingsToDo.Remove(Convert.ToString(checkBox.Content));
                            }
                        }
                    };

                    TreeViewItem treeViewItem = new TreeViewItem();
                    treeViewItem.Header = checkBox;

                    Applications.Items.Add(treeViewItem);
                }
            }
            this.Visibility = Visibility.Visible;
            List<string> ApplicationsToDo = thingsToDo;
            ApplicationsToDo.Remove("Drivers");
            ApplicationsToDo.Remove("System Report");
            if (ApplicationsToDo.Count == 0)
            {
                Applications.Visibility = Visibility.Collapsed;
            }
            else
            {
                Applications.Visibility = Visibility.Visible;
            }
            var window = Window.GetWindow(this) as MainWindow;
            window.LoadingScreen.Visibility = Visibility.Collapsed;
        }

        private async Task<int> FallbackToOnline()
        {
            try
            {
                ConnectToWiFi();
                XDocument credentials = XDocument.Load(@"Credentials.xml");
                var credential = credentials.Root;
                //MessageBox.Show(Convert.ToString(credential));
                var username = credential.Element("Username").Value;
                var password = credential.Element("Password").Value;
                await Task.Run(() =>
                {
                    Process mountNetworkDrive = new Process();
                    mountNetworkDrive.StartInfo.FileName = "net.exe";
                    mountNetworkDrive.StartInfo.Arguments = $@"use Y: \\SGBonoServ\Software /user:{username} {password}";
                    mountNetworkDrive.StartInfo.UseShellExecute = false;
                    mountNetworkDrive.StartInfo.RedirectStandardOutput = true;
                    mountNetworkDrive.StartInfo.CreateNoWindow = true;
                    mountNetworkDrive.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    mountNetworkDrive.Start();
                    mountNetworkDrive.WaitForExit();
                });
                return 0;
            } catch (Exception ex)
            {
                frame.Content = new ErrorScreen($"An error occured while attempting to read from server. \n{ex.Message}");
                this.Visibility = Visibility.Visible;
                grid.Visibility = Visibility.Collapsed;
                return 1;
            }
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
