using iNKORE.UI.WPF.Modern;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
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

            HandleInstall();
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

                if (tasks.ContainsKey("Drivers") != false) driversCount = tasks["Drivers"].Count;
                if (tasks.ContainsKey("Applications") != false) appsCount = tasks["Applications"].Count;
                if (tasks.ContainsKey("Post-applications") != false) postAppsCount = tasks["Post-applications"].Count;

                await Delay(2000);
                progressBar.Value += 100 / (driversCount + appsCount + postAppsCount);

                if (isOnline == false)
                {
                    foreach (var item in tasks["Applications"].Where(x => x.isSelected == true))
                    {
                        XDocument program = XDocument.Load($@"{localDriveLetter}:\Software\ProgramsList.xml");
                        bool programInstalled = false;
                        var name = program.Root.Element(item.name).Element("name").Value;
                        var path = program.Root.Element(item.name).Element("path").Value;
                        var run = program.Root.Element(item.name).Element("run").Value;
                        var arguments = program.Root.Element(item.name).Element("arguments").Value;
                        var customcscode = program.Root.Element(item.name).Element("customcscode").Value;
                        while (programInstalled == false)
                        {
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
                                if (string.IsNullOrEmpty(customcscode))
                                {
                                    programInstalled = true;
                                }
                                else
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
                                        programInstalled = true;
                                    }
                                }
                            });
                        }
                        progressBar.Value += 100 / (driversCount + appsCount + postAppsCount);
                    }
                }
            } catch (Exception ex)
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

            if (tasks.ContainsKey("Drivers") != false) driversCount = tasks["Drivers"].Count;
            if (tasks.ContainsKey("Applications") != false) appsCount = tasks["Applications"].Count;
            if (tasks.ContainsKey("Post-applications") != false) postAppsCount = tasks["Post-applications"].Count;

            await Delay(2000);
        }

        private async Task Delay(int howlong)
        {
            await Task.Delay(howlong);
        }
    }
}
