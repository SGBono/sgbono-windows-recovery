using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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

        class FixTask
        {
            public readonly string name;
            public bool isSelected = true;

            public FixTask(string name)
            {
                this.name = name;
            }
        }

        private Dictionary<string, List<FixTask>> tasks = new Dictionary<string, List<FixTask>>();

        /**
         * {
         *      "Drivers": [
         *          FixTask("Drivers")
         *      ],
         * 
         *      "Applications": [
         *          FixTask("Google Chrome")
         *          FixTask("LibreOffice")
         *          FixTask("Microsoft Teams")
         *      ],
         *      
         *      "Post-applications": [
         *          FixTask("System Report")
         *          FixTask("Cleanup")
         *      ],
         * }
         * 
         * Applications indeterminate "-" means that list of tasks mapped to isSelected some is true
         * 
         * 
         **/

        private char localDriveLetter;

        private bool thatWasMe = false;

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

                if (driversPresent == true)
                {
                    fixListBox.Items.Remove(driversCheckbox);
                }

                //Check if software is present on USB
                // !!IMPORTANT!! - Change start character from C to D before deployment
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
                else
                {
                    ErrorScreen errorScreen = new ErrorScreen(ex.Message);
                    this.Visibility = Visibility.Visible;
                    grid.Visibility = Visibility.Collapsed;
                    frame.Content = errorScreen;
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

            List<FixTask> applicationsTaskList = new List<FixTask>();
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
                    FixTask taskInfo = new FixTask(programName);
                    applicationsTaskList.Add(taskInfo);

                    checkBox.Checked += (object sender, RoutedEventArgs e) =>
                    {
                        tasks["Applications"].FirstOrDefault(x => x.name == programName).isSelected = true;
                        CheckBox_Checked();
                    };
                    checkBox.Unchecked += (object sender, RoutedEventArgs e) =>
                    {
                        if (tasks["Applications"].FirstOrDefault(x => x.name == programName) != null)
                        {
                            tasks["Applications"].FirstOrDefault(x => x.name == programName).isSelected = false;
                        }
                        CheckBox_Checked();
                    };

                    /*Binding binding = new Binding("IsChecked");
                    binding.Source = applicationsCheckbox;
                    binding.Mode = BindingMode.TwoWay;
                    checkBox.SetBinding(CheckBox.IsCheckedProperty, binding);*/

                    TreeViewItem treeViewItem = new TreeViewItem();
                    treeViewItem.Header = checkBox;

                    Applications.Items.Add(treeViewItem);
                }
            }

            this.Visibility = Visibility.Visible;

            tasks.Add("Applications", applicationsTaskList);
            List<FixTask> postApplicationsTaskList = new List<FixTask>
            {
                new FixTask("System Report"),
                new FixTask("Cleanup")
            };
            tasks.Add("Post-applications", postApplicationsTaskList);

            if (applicationsTaskList.Count == 0)
            {
                fixListBox.Items.Remove("ApplicationsRoot");
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
                if (!File.Exists(@"Y:\ProgramsList.xml")) throw new Exception("You do not have the required files locally and the server is unreachable.");
                return 0;
            }
            catch (Exception ex)
            {
                frame.Content = new ErrorScreen($"An error occurred while attempting to read from the server. \n{ex.Message}");
                this.Visibility = Visibility.Visible;
                grid.Visibility = Visibility.Collapsed;
                frame.Visibility = Visibility.Visible;
                var window = Window.GetWindow(this) as MainWindow;
                window.LoadingScreen.Visibility = Visibility.Collapsed;
                return 1;
            }
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void cleanupCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            if (tasks.ContainsKey("Post-applications"))
            {
                tasks["Post-applications"].FirstOrDefault(x => x.name == "Cleanup").isSelected = true;
            }
        }

        private void cleanupCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            var result = iNKORE.UI.WPF.Modern.Controls.MessageBox.Show("Unchecking this may seriously compromise our operational security. \nYou should NOT turn this off except for debugging purposes approved by the app developers. \nAre you sure you want to do this?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.No)
            {
                cleanupCheckbox.IsChecked = true;
            }
            else if (result == MessageBoxResult.Yes)
            {
                tasks["Post-applications"].FirstOrDefault(x => x.name == "Cleanup").isSelected = false;
            }
        }

        private void systemReportCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            if (tasks.ContainsKey("Post-applications"))
            {
                tasks["Post-applications"].FirstOrDefault(x => x.name == "System Report").isSelected = true;
            }
        }

        private void systemReportCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (tasks.ContainsKey("Post-applications"))
            {
                tasks["Post-applications"].FirstOrDefault(x => x.name == "System Report").isSelected = false;
            }
        }

        private void CheckBox_Checked()
        {
            if (thatWasMe == true)
            {
                thatWasMe = false;
            }
            else
            {
                if (tasks["Applications"].All(x => x.isSelected == true))
                {
                    //If all checkboxes are checked
                    thatWasMe = true;
                    applicationsCheckbox.IsChecked = true;
                }
                else if (tasks["Applications"].Any(x => x.isSelected == true))
                {
                    //If some checkboxes are checked
                    thatWasMe = true;
                    applicationsCheckbox.IsChecked = null;
                }
                else if (tasks["Applications"].All(x => x.isSelected == false))
                {
                    //If no checkboxes are checked
                    thatWasMe = true;
                    applicationsCheckbox.IsChecked = false;
                }
            }
        }

        private void applicationsCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            if (tasks.ContainsKey("Applications"))
            {
                if (thatWasMe == true)
                {
                    thatWasMe = false;
                    return;
                }
                else
                {
                    tasks["Applications"].ForEach(x => x.isSelected = true);
                    foreach (TreeViewItem item in Applications.Items)
                    {
                        thatWasMe = true;
                        CheckBox checkBox = item.Header as CheckBox;
                        checkBox.IsChecked = true;
                    }
                }
            }
        }

        private void applicationsCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (tasks.ContainsKey("Applications"))
            {
                if (thatWasMe == true)
                {
                    thatWasMe = false;
                    return;
                }
                else
                {
                    tasks["Applications"].ForEach(x => x.isSelected = false);
                    foreach (TreeViewItem item in Applications.Items)
                    {
                        thatWasMe = true;
                        CheckBox checkBox = item.Header as CheckBox;
                        checkBox.IsChecked = false;
                    }
                }
            }
        }

        private void applicationsCheckbox_Indeterminate(object sender, RoutedEventArgs e)
        {
            if (thatWasMe == true)
            {
                thatWasMe = false;
                return;
            }
            else
            {
                applicationsCheckbox.IsChecked = false;
            }
        }
    }
}