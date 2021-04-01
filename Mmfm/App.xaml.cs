using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Mmfm
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            DispatcherUnhandledException += Application_DispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        public static string SettingsJsonPath
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                var attrs = assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);

                var company = attrs.First() as AssemblyCompanyAttribute;

                return System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    company.Company.Replace(" ", "_"),
                    assembly.GetName().Name,
                    assembly.GetName().Version.ToString(),
                    "Settings.json"
                );
            }
        }

        private void LogException(Exception ex)
        {
            
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) => LogException((Exception)e.ExceptionObject);

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e) => LogException(e.Exception);

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) => LogException(e.Exception);      

    }
}
