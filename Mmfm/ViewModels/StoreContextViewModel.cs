using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Services.Store;

namespace Mmfm
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
                title = $"Trial period will end in a {(int)(appLicense.ExpirationDate - DateTime.Now).TotalDays} days.";
                content = "If you were touched, please puchase.";
            }
            else
            {
                title = "Trial period has been expired.";
                content = "Thank you for using mmfm.<LineBreak/>If you would like to continue to use, please purchase.";
            }
        }

        public bool IsNotExpired => appLicense.ExpirationDate > DateTime.Now;
        
        public bool IsExpired => !IsNotExpired;

        public string Title => title;

        public string Content => content;
    }
}
