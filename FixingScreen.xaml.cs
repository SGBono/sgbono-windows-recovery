using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace beforewindeploy_custom_recovery
{
    /// <summary>
    /// Interaction logic for ComponentSelection.xaml
    /// </summary>
    public partial class FixingScreen : Page
    {
        Dictionary<string, List<ComponentSelection.FixTask>> tasks = ComponentSelection.tasks;
        private bool isOnline = ComponentSelection.isOnline;
        private bool didSystemReport = false;
        private string systemReportLocation = "";

        public FixingScreen()
        {
            InitializeComponent();
            progressBar.Value = 0;
            etaLabel.Content = "Estimating time remaining";
            nowInstallingLabel.Content = "Starting recovery";

            HandleInstall();
        }

        private async void ConnectToServer(string whereTo)
        {
            try
            {
                if (whereTo == "Software")
                {
                    await Task.Run(() =>
                    {
                        XDocument serverCredentials = XDocument.Load(@"Credentials.xml");
                        var serverCredential = serverCredentials.Root;
                        var serverUsername = serverCredential.Element("Username").Value;
                        var serverPassword = serverCredential.Element("Password").Value;
                        Process mountNetworkDrive = new Process();
                        mountNetworkDrive.StartInfo.FileName = "net.exe";
                        mountNetworkDrive.StartInfo.Arguments = $@"use Z: \\SGBonoServ\Software /user:{serverUsername} {serverPassword}";
                        mountNetworkDrive.StartInfo.UseShellExecute = false;
                        mountNetworkDrive.StartInfo.RedirectStandardOutput = true;
                        mountNetworkDrive.StartInfo.CreateNoWindow = true;
                        mountNetworkDrive.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        mountNetworkDrive.Start();
                        mountNetworkDrive.WaitForExit();
                    });
                }
                else if (whereTo == "Drivers")
                {
                    await Task.Run(() =>
                    {
                        XDocument serverCredentials = XDocument.Load(@"Credentials.xml");
                        var serverCredential = serverCredentials.Root;
                        var serverUsername = serverCredential.Element("Username").Value;
                        var serverPassword = serverCredential.Element("Password").Value;
                        Process mountNetworkDrive = new Process();
                        mountNetworkDrive.StartInfo.FileName = "net.exe";
                        mountNetworkDrive.StartInfo.Arguments = $@"use Z: \\SGBonoServ\Drivers /user:{serverUsername} {serverPassword}";
                        mountNetworkDrive.StartInfo.UseShellExecute = false;
                        mountNetworkDrive.StartInfo.RedirectStandardOutput = true;
                        mountNetworkDrive.StartInfo.CreateNoWindow = true;
                        mountNetworkDrive.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        mountNetworkDrive.Start();
                        mountNetworkDrive.WaitForExit();
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorScreen errorScreen = new ErrorScreen(ex.Message);
                this.Visibility = Visibility.Visible;
                grid.Visibility = Visibility.Collapsed;
                frame.Content = errorScreen;
                return;
            }
        }

        private async void HandleInstall()
        {
            try
            {
                if (tasks.ContainsKey("Drivers") && tasks["Drivers"].FirstOrDefault(x => x.name == "Drivers").isSelected == true)
                {
                    if (await InstallDrivers() == 1) throw new Exception("Stop execution");
                }

                if (tasks.ContainsKey("Applications") && tasks["Applications"].Any(x => x.isSelected == true) == true)
                {
                    if (await InstallApplications() == 1) throw new Exception("Stop execution");
                }

                if (tasks.ContainsKey("Post-applications") && tasks["Post-applications"].Any(x => x.isSelected == true) == true)
                {
                    if (await InstallPostApplications() == 1) throw new Exception("Stop execution");
                }

                progressBar.Value = 100;
                nowInstallingLabel.Content = "All done.";
                await Delay(1000);
                grid.Visibility = Visibility.Collapsed;
                frame.Visibility = Visibility.Visible;
                FinishScreen finishScreen = new FinishScreen();
                frame.Content = finishScreen;
                if (didSystemReport == true)
                {
                    finishScreen.systemReportLocation.Visibility = Visibility.Visible;
                    finishScreen.systemReportLocation.Content = $"The system report has been saved to {systemReportLocation}.";
                }
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

        private async Task<int> InstallDrivers()
        {
            try
            {
                int driversCount = 0;
                int appsCount = 0;
                int postAppsCount = 0;

                if (tasks.ContainsKey("Drivers") != false) driversCount = tasks["Drivers"].Where(x => x.isSelected == true).Count();
                if (tasks.ContainsKey("Applications") != false) appsCount = tasks["Applications"].Where(x => x.isSelected == true).Count();
                if (tasks.ContainsKey("Post-applications") != false) postAppsCount = tasks["Post-applications"].Where(x => x.isSelected == true).Count();

                nowInstallingLabel.Content = "We are now installing drivers.";
                if (isOnline == true)
                {
                    await Task.Run(() =>
                    {
                        XDocument serverCredentials = XDocument.Load(@"Credentials.xml");
                        var serverCredential = serverCredentials.Root;
                        var serverUsername = serverCredential.Element("Username").Value;
                        var serverPassword = serverCredential.Element("Password").Value;
                        Process mountNetworkDrive = new Process();
                        mountNetworkDrive.StartInfo.FileName = "net.exe";
                        mountNetworkDrive.StartInfo.Arguments = $@"use Z: \\SGBonoServ\Drivers /user:{serverUsername} {serverPassword}";
                        mountNetworkDrive.StartInfo.UseShellExecute = false;
                        mountNetworkDrive.StartInfo.RedirectStandardOutput = true;
                        mountNetworkDrive.StartInfo.CreateNoWindow = true;
                        mountNetworkDrive.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        mountNetworkDrive.Start();
                        mountNetworkDrive.WaitForExit();
                    });

                    await Task.Run(() =>
                    {
                        ProcessStartInfo driverInstallServer = new ProcessStartInfo();
                        driverInstallServer.FileName = "cmd.exe";
                        driverInstallServer.Arguments = @"/c SDI_x64_R2309.exe";
                        driverInstallServer.CreateNoWindow = true;
                        driverInstallServer.WindowStyle = ProcessWindowStyle.Hidden;
                        driverInstallServer.UseShellExecute = false;
                        driverInstallServer.WorkingDirectory = @"Z:";

                        Process.Start(driverInstallServer);
                    });
                    await Delay(10000);
                    try
                    {
                        await Task.Run(() =>
                        {
                            Process sdiProcess = Process.GetProcessesByName("SDI_x64_R2309").FirstOrDefault();
                            sdiProcess.WaitForExit();
                        });
                    }
                    catch (Exception ex)
                    {
                        ErrorScreen errorScreen = new ErrorScreen("An error occurred while installing drivers. \n" + ex.Message);
                        this.Visibility = Visibility.Visible;
                        grid.Visibility = Visibility.Collapsed;
                        frame.Content = errorScreen;
                        return 1;
                    }
                }
                else
                {
                    for (char c = 'D'; c <= 'Z'; c++)
                    {
                        try
                        {
                            if (File.Exists(c + @":\Drivers\SDI_x64_R2309.exe"))
                            {
                                Process process = new Process();
                                process.StartInfo.FileName = "cmd.exe";
                                process.StartInfo.Arguments = "/c SDI_x64_R2309.exe";
                                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                                process.StartInfo.CreateNoWindow = true;
                                process.StartInfo.WorkingDirectory = c + @":\Drivers";
                                process.Start();
                                await Delay(10000);
                                try
                                {
                                    await Task.Run(() =>
                                    {
                                        Process sdiProcess = Process.GetProcessesByName("SDI_x64_R2309").FirstOrDefault();
                                        sdiProcess.WaitForExit();
                                    });
                                    break;
                                }
                                catch (Exception ex)
                                {
                                    ErrorScreen errorScreen = new ErrorScreen("An error occurred while installing drivers. \n" + ex.Message);
                                    this.Visibility = Visibility.Visible;
                                    grid.Visibility = Visibility.Collapsed;
                                    frame.Content = errorScreen;
                                    return 1;
                                }
                            }
                            else if (c == 'Z')
                            {
                                ErrorScreen errorScreen = new ErrorScreen("An error occurred while installing drivers. \nThe required files could not be found.");
                                this.Visibility = Visibility.Visible;
                                grid.Visibility = Visibility.Collapsed;
                                frame.Content = errorScreen;
                                return 1;
                            }
                        }
                        catch (Exception ex)
                        {
                            ErrorScreen errorScreen = new ErrorScreen("An error occurred while installing drivers. \n" + ex.Message);
                            this.Visibility = Visibility.Visible;
                            grid.Visibility = Visibility.Collapsed;
                            frame.Content = errorScreen;
                            return 1;
                        }
                    }
                }
                progressBar.Value += 100 / (driversCount + appsCount + postAppsCount);
                await Delay(1000);
                return 0;
            }
            catch (Exception ex)
            {
                ErrorScreen errorScreen = new ErrorScreen($"There was an error in the drivers installation phase. \n{ex.Message}\n{ex.StackTrace}");
                this.Visibility = Visibility.Visible;
                grid.Visibility = Visibility.Collapsed;
                frame.Content = errorScreen;
                return 1;
            }
        }

        private char localDriveLetter = ComponentSelection.localDriveLetter;

        private async Task<int> InstallApplications()
        {
            try
            {
                int driversCount = 0;
                int appsCount = 0;
                int postAppsCount = 0;

                if (tasks.ContainsKey("Drivers") != false) driversCount = tasks["Drivers"].Where(x => x.isSelected == true).Count();
                if (tasks.ContainsKey("Applications") != false) appsCount = tasks["Applications"].Where(x => x.isSelected == true).Count();
                if (tasks.ContainsKey("Post-applications") != false) postAppsCount = tasks["Post-applications"].Where(x => x.isSelected == true).Count();


                foreach (var item in tasks["Applications"].Where(x => x.isSelected == true))
                {
                    string documentPath = "";
                    if (isOnline == true)
                    {
                        ConnectToServer("Software");
                        documentPath = $@"Y:\ProgramsList.xml";
                    }
                    else if (isOnline == false)
                    {
                        documentPath = $@"{localDriveLetter}:\Software\ProgramsList.xml";
                    }
                    XDocument document = XDocument.Load(documentPath);
                    foreach (var program in document.Root.Elements().Where(p => p.Element("id").Value == item.id))
                    {
                        var name = program.Element("name").Value;
                        var path = program.Element("path").Value;
                        var run = program.Element("run").Value;
                        var arguments = program.Element("arguments").Value;
                        var customcscode = program.Element("customcscode").Value;
                        nowInstallingLabel.Content = $"We are now installing {name}.";
                        try
                        {
                            await Delay(500);
                            if (!string.IsNullOrEmpty(path))
                            {
                                string setupPath = "";
                                if (isOnline == true)
                                {
                                    setupPath = $@"Y:\{path}";
                                }
                                else if (isOnline == false)
                                {
                                    setupPath = $@"{localDriveLetter}:\Software\{path}";
                                }
                                Process setup = new Process();
                                setup.StartInfo.FileName = setupPath;
                                setup.StartInfo.Arguments = arguments;
                                setup.StartInfo.CreateNoWindow = true;
                                setup.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                                await Task.Run(() =>
                                {
                                    setup.Start();
                                    setup.WaitForExit();
                                });
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(run))
                                {
                                    throw new Exception("The XML file was not configured correctly - both path and run elements are missing.");
                                }
                                Process setup = new Process();
                                setup.StartInfo.FileName = $@"{run}";
                                setup.StartInfo.Arguments = arguments;
                                setup.StartInfo.CreateNoWindow = true;
                                setup.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                                await Task.Run(() =>
                                {
                                    setup.Start();
                                    setup.WaitForExit();
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            ErrorScreen errorScreen = new ErrorScreen($"There was an error installing {name}.\n" + ex.Message);
                            this.Visibility = Visibility.Visible;
                            grid.Visibility = Visibility.Collapsed;
                            frame.Content = errorScreen;
                            return 1;
                        }
                        await Delay(500);
                        await Task.Run(() =>
                        {
                            if (!string.IsNullOrEmpty(customcscode))
                            {
                                CompilerParameters parameters = new CompilerParameters();
                                parameters.ReferencedAssemblies.Add("System.dll");
                                parameters.ReferencedAssemblies.Add($@"{Environment.CurrentDirectory}\Interop.IWshRuntimeLibrary.dll");
                                parameters.GenerateInMemory = true;
                                CompilerResults results = new CSharpCodeProvider().CompileAssemblyFromSource(parameters, customcscode);
                                if (results.Errors.Count > 0)
                                {
                                    foreach (CompilerError error in results.Errors)
                                    {
                                        Application.Current.Dispatcher.Invoke(() =>
                                        {
                                            throw new Exception($"There was an error running package scripts for {name}.\n{error.ErrorText}");
                                        });
                                    }
                                }
                                else
                                {
                                    Type customType = results.CompiledAssembly.GetType("CustomCode");
                                    MethodInfo method = customType.GetMethod("Execute");
                                    method.Invoke(null, null);
                                }
                            }
                        });
                        progressBar.Value += 100 / (driversCount + appsCount + postAppsCount);
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("There was an error"))
                {
                    ErrorScreen errorScreen = new ErrorScreen(ex.Message);
                    this.Visibility = Visibility.Visible;
                    grid.Visibility = Visibility.Collapsed;
                    frame.Content = errorScreen;
                    return 1;
                }
                else
                {
                    ErrorScreen errorScreen = new ErrorScreen($"There was an error in the applications install phase.\n{ex.Message}");
                    this.Visibility = Visibility.Visible;
                    grid.Visibility = Visibility.Collapsed;
                    frame.Content = errorScreen;
                    return 1;
                }
            }
        }

        private async Task<int> InstallPostApplications()
        {
            try
            {
                int driversCount = 0;
                int appsCount = 0;
                int postAppsCount = 0;

                if (tasks.ContainsKey("Drivers") != false) driversCount = tasks["Drivers"].Where(x => x.isSelected == true).Count();
                if (tasks.ContainsKey("Applications") != false) appsCount = tasks["Applications"].Where(x => x.isSelected == true).Count();
                if (tasks.ContainsKey("Post-applications") != false) postAppsCount = tasks["Post-applications"].Where(x => x.isSelected == true).Count();

                await Delay(1000);

                if (tasks["Post-applications"].FirstOrDefault(x => x.name == "System Report").isSelected == true)
                {
                    nowInstallingLabel.Content = "We are now generating the system report.";
                    await Delay(1000);

                    string cpuName = "";
                    string gpuName = "";
                    string ramInfo = "";
                    string storageSize = "";
                    string batteryHealth = "";

                    //CPU
                    ManagementObjectSearcher mos = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
                    foreach (ManagementObject mo in mos.Get())
                    {
                        cpuName = "CPU: " + (string)mo["Name"];
                    }

                    //GPU
                    bool hasiGPU = false;
                    string iGPUName = "";
                    using (var searcher1 = new ManagementObjectSearcher("select * from Win32_VideoController"))
                    {
                        foreach (ManagementObject obj in searcher1.Get())
                        {
                            if (obj["Name"].ToString() == "Microsoft Basic Display Adapter")
                            {
                                gpuName = "GPU: No GPU drivers installed";
                            }
                            //Improved iGPU detector - should work theoretically though this requires testing
                            else if (obj["Name"].ToString() == "AMD Radeon(TM) Graphics" || obj["Name"].ToString().Contains("Intel") && !obj["Name"].ToString().Contains("Intel Arc") && obj["Name"].ToString() != "Intel(R) Arc(TM) Graphics")
                            {
                                hasiGPU = true;
                                iGPUName = obj["Name"].ToString();
                            }
                            else
                            {
                                gpuName = "GPU: " + (string)obj["Name"];
                                break;
                            }
                        }
                        if (gpuName == "" && hasiGPU == true)
                        {
                            gpuName = "GPU: " + iGPUName + "(iGPU)";
                        }
                    }

                    //RAM
                    ObjectQuery objectQuery = new ObjectQuery("SELECT * FROM Win32_OperatingSystem");
                    ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(objectQuery);

                    ManagementObjectSearcher searcher2 = new ManagementObjectSearcher("Select * from Win32_PhysicalMemory");
                    var ramspeed = "";
                    var newram = 0;
                    var newMemoryType = "";
                    foreach (ManagementObject obj in searcher2.Get())
                    {
                        try
                        {
                            ramspeed = Convert.ToString(obj["ConfiguredClockSpeed"]);
                        }
                        catch { }
                    }
                    foreach (ManagementObject managementObject in managementObjectSearcher.Get())
                    {
                        newram = Convert.ToInt32(managementObject["TotalVisibleMemorySize"]) / 1000 / 1000;
                    }
                    foreach (ManagementObject managementObject in searcher2.Get())
                    {
                        string memoryType = managementObject["MemoryType"].ToString();
                        switch (memoryType)
                        {
                            case "20":
                                newMemoryType = "DDR";
                                break;
                            case "21":
                                newMemoryType = "DDR2";
                                break;
                            case "24":
                                newMemoryType = "DDR3";
                                break;
                            case "26":
                                newMemoryType = "DDR4";
                                break;
                            case "34":
                                newMemoryType = "DDR5";
                                break;
                            case "0":
                                string memoryType2 = managementObject["SMBIOSMemoryType"]?.ToString() ?? "0";
                                if (memoryType2 == "34")
                                {
                                    newMemoryType = "DDR5";
                                }
                                else if (memoryType2 == "20")
                                {
                                    newMemoryType = "DDR";
                                }
                                else if (memoryType2 == "21")
                                {
                                    newMemoryType = "DDR2";
                                }
                                else if (memoryType2 == "24")
                                {
                                    newMemoryType = "DDR3";
                                }
                                else if (memoryType2 == "26")
                                {
                                    newMemoryType = "DDR4";
                                }
                                else
                                {
                                    newMemoryType = "Unknown";
                                }
                                break;
                            default:
                                newMemoryType = "Unknown";
                                break;
                        }
                    }
                    if (ramspeed == null || ramspeed == "" || ramspeed == "0")
                    {
                        ramspeed = "Unknown ";
                    }
                    else if (newMemoryType == "Unknown")
                    {
                        //Last last resort RAM type check
                        //Banking on nobody being able to reach 4800 MT/s on DDR4 (DDR5 JEDEC = 4800 MT/s)
                        //Also not considering the LPDDR5/LPDDR5x users
                        if (Convert.ToInt32(ramspeed) >= 4800)
                        {
                            newMemoryType = "DDR5";
                        }
                    }

                    ramInfo = "RAM: " + newram + "GB " + ramspeed + "MT/s " + newMemoryType;

                    //Storage
                    DriveInfo mainDrive = new DriveInfo(System.IO.Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.System)));
                    var totalsize = mainDrive.TotalSize / 1000 / 1000 / 1000;
                    storageSize = "Storage on Windows drive: " + totalsize + "GB";
                    await Delay(200);

                    //Battery health
                    ManagementObjectSearcher batteryStaticData = new ManagementObjectSearcher("root/WMI", "SELECT * FROM BatteryStaticData");
                    ManagementObjectSearcher batteryFullChargedCapacity = new ManagementObjectSearcher("root/WMI", "SELECT * FROM BatteryFullChargedCapacity");
                    
                    int designCapacity = 0;
                    int fullChargeCapacity = 0;

                    foreach (ManagementObject queryObj in batteryStaticData.Get())
                    {
                        designCapacity = Convert.ToInt32(queryObj["DesignedCapacity"]);
                    }

                    foreach (ManagementObject queryObj in batteryFullChargedCapacity.Get())
                    {
                        fullChargeCapacity = Convert.ToInt32(queryObj["FullChargedCapacity"]);
                    }

                    if (designCapacity == 0 || fullChargeCapacity == 0)
                    {
                        batteryHealth = "Battery health: No battery detected";
                    }
                    else
                    {
                        double batteryHealthPercentage = Math.Round((double)fullChargeCapacity / designCapacity * 100, 1);
                        batteryHealth = "Battery health: " + batteryHealthPercentage + "%";
                    }

                    File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\System Report.txt", $"======System Report======\n\n<Note the following down>\n{cpuName}\n{gpuName}\n{ramInfo}\n{storageSize}\n{batteryHealth}\n\n<Additional information>\nOriginal battery capacity: {Math.Round((double)designCapacity / 1000)}Wh\nFull charge capacity: {Math.Round((double)fullChargeCapacity / 1000)}Wh");
                    didSystemReport = true;
                    systemReportLocation = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\System Report.txt";
                    progressBar.Value += 100 / (driversCount + appsCount + postAppsCount);
                }

                if (tasks["Post-applications"].FirstOrDefault(x => x.name == "Cleanup").isSelected == true)
                {
                    nowInstallingLabel.Content = "We are now cleaning up.";
                    await Delay(1000);
                    Process process = new Process();
                    process.StartInfo.FileName = "netsh";
                    process.StartInfo.Arguments = "wlan delete profile name=\"SST-External\"";
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();
                    process.WaitForExit();
                    Process process2 = new Process();
                    process2.StartInfo.FileName = "netsh";
                    process2.StartInfo.Arguments = "wlan delete profile name=\"SGBono Internal\"";
                    process2.StartInfo.RedirectStandardOutput = true;
                    process2.StartInfo.UseShellExecute = false;
                    process2.StartInfo.CreateNoWindow = true;
                    process2.Start();
                    process2.WaitForExit();
                    Process process4 = new Process();
                    process4.StartInfo.FileName = "cmd.exe";
                    process4.StartInfo.Arguments = "/c net use /delete Z:";
                    process4.StartInfo.RedirectStandardOutput = true;
                    process4.StartInfo.UseShellExecute = false;
                    process4.StartInfo.CreateNoWindow = true;
                    process4.Start();
                    process4.WaitForExit();
                    if (Directory.Exists(@"C:\SGBono"))
                    {
                        Directory.Delete(@"C:\SGBono", true);
                        while (Directory.Exists(@"C:\SGBono"))
                        {
                            await Delay(500);
                        }
                    }
                    if (Directory.Exists(@"C:\Windows\System32\oobe\Automation"))
                    {
                        Directory.Delete(@"C:\Windows\System32\oobe\Automation", true);
                        while (Directory.Exists(@"C:\Windows\System32\oobe\Automation"))
                        {
                            await Delay(500);
                        }
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                ErrorScreen errorScreen = new ErrorScreen("There was an error in the post-applications install phase. \n" + ex.Message);
                this.Visibility = Visibility.Visible;
                grid.Visibility = Visibility.Collapsed;
                frame.Content = errorScreen;
                return 1;
            }
        }

        private async Task Delay(int howlong)
        {
            await Task.Delay(howlong);
        }
    }
}
