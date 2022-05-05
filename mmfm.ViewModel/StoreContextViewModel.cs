using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Services.Store;

namespace Mmfm.ViewModel
{
    public class StoreContextViewModel : ContentDialogViewModel
    {
        StoreAppLicense appLicense;
        string title, content;

        public StoreContextViewModel(StoreAppLicense appLicense)
        {
            this.appLicense = appLicense;
            if (IsNotExpired)
            {
                title = string.Format(Properties.Resources.Store_NotExipredTitle, (int)(appLicense.ExpirationDate - DateTime.Now).TotalDays);
                content = Properties.Resources.Store_NotExpiredContent;
            }
            else
            {
                title = Properties.Resources.Store_ExpiredTitle;
                content = Properties.Resources.Store_ExpiredContent;
            }
        }

        public bool IsNotExpired => appLicense.ExpirationDate > DateTime.Now;
        
        public bool IsExpired => !IsNotExpired;

        public string Title => title;

        public string Content => content;
    }
}
