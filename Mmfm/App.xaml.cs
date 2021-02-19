using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace Mmfm
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        public static string SettingsPath
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

    }
}
