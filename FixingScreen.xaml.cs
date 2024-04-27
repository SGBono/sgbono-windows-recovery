using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

        public FixingScreen()
        {
            InitializeComponent();
            progressBar.Value = 0;
            etaLabel.Content = "Estimating time remaining";
            nowInstallingLabel.Content = "";

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
            if (tasks.ContainsKey("Drivers") && tasks["Drivers"].FirstOrDefault(x => x.name == "Drivers").isSelected == true)
            {
                await InstallDrivers();
            }

            if (tasks.ContainsKey("Applications") && tasks["Applications"].Any(x => x.isSelected == true) == true)
            {
                await InstallApplications();
            }

            if (tasks.ContainsKey("Post-applications") && tasks["Post-applications"].Any(x => x.isSelected == true) == true)
            {
                await InstallPostApplications();
            }
        }

        private async Task InstallDrivers()
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
                    var serverCredential = serverCredentials.Root.Elements().First();
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

                    Process.Start(driverInstallServer).WaitForExit();
                });
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
                                return;
                            }
                        }
                        else if (c == 'Z')
                        {
                            ErrorScreen errorScreen = new ErrorScreen("An error occurred while installing drivers. \nThe required files could not be found.");
                            this.Visibility = Visibility.Visible;
                            grid.Visibility = Visibility.Collapsed;
                            frame.Content = errorScreen;
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorScreen errorScreen = new ErrorScreen("An error occurred while installing drivers. \n" + ex.Message);
                        this.Visibility = Visibility.Visible;
                        grid.Visibility = Visibility.Collapsed;
                        frame.Content = errorScreen;
                        return;
                    }
                }
            }
        }

        private char localDriveLetter = ComponentSelection.localDriveLetter;

        private async Task InstallApplications()
        {
            try
            {
                int driversCount = 0;
                int appsCount = 0;
                int postAppsCount = 0;

                if (tasks.ContainsKey("Drivers") != false) driversCount = tasks["Drivers"].Where(x => x.isSelected == true).Count();
                if (tasks.ContainsKey("Applications") != false) appsCount = tasks["Applications"].Where(x => x.isSelected == true).Count();
                if (tasks.ContainsKey("Post-applications") != false) postAppsCount = tasks["Post-applications"].Where(x => x.isSelected == true).Count();

                await Delay(2000);
                progressBar.Value += 100 / (driversCount + appsCount + postAppsCount);

                if (isOnline == false)
                {
                    foreach (var item in tasks["Applications"].Where(x => x.isSelected == true))
                    {
                        XDocument document = XDocument.Load($@"{localDriveLetter}:\Software\ProgramsList.xml");
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
                                    Process setup = new Process();
                                    setup.StartInfo.FileName = $@"{localDriveLetter}:\Software\{path}";
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
                                return;
                            }
                            await Delay(500);
                            await Task.Run(() =>
                            {
                                if (!string.IsNullOrEmpty(customcscode))
                                {
                                    CompilerParameters parameters = new CompilerParameters();
                                    parameters.ReferencedAssemblies.Add("System.dll");
                                    parameters.ReferencedAssemblies.Add(@"C:\Windows\System32\oobe\Automation\Interop.IWshRuntimeLibrary.dll");
                                    parameters.GenerateInMemory = true;
                                    CompilerResults results = new CSharpCodeProvider().CompileAssemblyFromSource(parameters, customcscode);
                                    if (results.Errors.Count > 0)
                                    {
                                        foreach (CompilerError error in results.Errors)
                                        {
                                            Application.Current.Dispatcher.Invoke(() =>
                                            {
                                                ErrorScreen errorScreen = new ErrorScreen($"There was an error running package scripts for {name}\n" + error.ErrorText);
                                                this.Visibility = Visibility.Visible;
                                                grid.Visibility = Visibility.Collapsed;
                                                frame.Content = errorScreen;
                                                return;
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
                }
                else if (isOnline == true)
                {
                    ConnectToServer("Software");
                    foreach (var item in tasks["Applications"].Where(x => x.isSelected == true))
                    {
                        XDocument document = XDocument.Load($@"Y:\ProgramsList.xml");
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
                                    Process setup = new Process();
                                    setup.StartInfo.FileName = $@"Y:\{path}";
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
                                    setup.StartInfo.FileName = run;
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
                                return;
                            }
                            await Delay(500);
                            await Task.Run(() =>
                            {
                                if (!string.IsNullOrEmpty(customcscode))
                                {
                                    CompilerParameters parameters = new CompilerParameters();
                                    parameters.ReferencedAssemblies.Add("System.dll");
                                    parameters.ReferencedAssemblies.Add(@"C:\Windows\System32\oobe\Automation\Interop.IWshRuntimeLibrary.dll");
                                    parameters.GenerateInMemory = true;
                                    CompilerResults results = new CSharpCodeProvider().CompileAssemblyFromSource(parameters, customcscode);
                                    if (results.Errors.Count > 0)
                                    {
                                        foreach (CompilerError error in results.Errors)
                                        {
                                            Application.Current.Dispatcher.Invoke(() =>
                                            {
                                                ErrorScreen errorScreen = new ErrorScreen($"There was an error running package scripts for {name}\n" + error.ErrorText);
                                                this.Visibility = Visibility.Visible;
                                                grid.Visibility = Visibility.Collapsed;
                                                frame.Content = errorScreen;
                                                return;
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
                }
            }
            catch (Exception ex)
            {
                ErrorScreen errorScreen = new ErrorScreen($"There was an error in the applications install phase.\n" + ex.Message);
                this.Visibility = Visibility.Visible;
                grid.Visibility = Visibility.Collapsed;
                frame.Content = errorScreen;
                return;
            }
        }

        private async Task InstallPostApplications()
        {
            int driversCount = 0;
            int appsCount = 0;
            int postAppsCount = 0;

            if (tasks.ContainsKey("Drivers") != false) driversCount = tasks["Drivers"].Where(x => x.isSelected == true).Count();
            if (tasks.ContainsKey("Applications") != false) appsCount = tasks["Applications"].Where(x => x.isSelected == true).Count();
            if (tasks.ContainsKey("Post-applications") != false) postAppsCount = tasks["Post-applications"].Where(x => x.isSelected == true).Count();

            await Delay(2000);
        }

        private async Task Delay(int howlong)
        {
            await Task.Delay(howlong);
        }
    }
}
