using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Mmfm
{
    public class PluginSettingDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {            
            var pluginSetting = item as PluginSettingEditViewModel;
            if (pluginSetting == null)
            {
                return null;
            }

            var dataTemplateKey = $"{pluginSetting.Name}SettingsDataTemplate";
            return App.Current.TryFindResource(dataTemplateKey) as DataTemplate ?? new DataTemplate(); 
        }
    }
}
