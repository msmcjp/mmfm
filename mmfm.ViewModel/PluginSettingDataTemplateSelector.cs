using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Mmfm.ViewModel
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
