using ModernWpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Mmfm
{
    public static class MessageBoxViewModelExtension
    {
        public static ContentDialog BuildContentDialog(this MessageBoxViewModel messageBox)
        {
            var contentDialog = new ContentDialog
            {
                Title = messageBox.Caption,
                Content = messageBox.Text
            };

            switch (messageBox.Button)
            {
                case MessageBoxButton.OK:
                    contentDialog.PrimaryButtonText = "_OK";
                    contentDialog.DefaultButton = ContentDialogButton.Primary;
                    break;
                case MessageBoxButton.OKCancel:
                    contentDialog.PrimaryButtonText = "_OK";
                    contentDialog.CloseButtonText = "_Cancel";
                    contentDialog.DefaultButton = (messageBox.DefaultResult == MessageBoxResult.OK) ? ContentDialogButton.Primary : ContentDialogButton.Close;
                    break;
                case MessageBoxButton.YesNo:
                    contentDialog.PrimaryButtonText = "_Yes";
                    contentDialog.SecondaryButtonText = "_No";
                    contentDialog.DefaultButton = (messageBox.DefaultResult == MessageBoxResult.Yes) ? ContentDialogButton.Primary : ContentDialogButton.Secondary;
                    break;
                case MessageBoxButton.YesNoCancel:
                    contentDialog.PrimaryButtonText = "_Yes";
                    contentDialog.SecondaryButtonText = "_No";
                    contentDialog.CloseButtonText = "_Cancel";
                    contentDialog.DefaultButton = (messageBox.DefaultResult == MessageBoxResult.Yes) ? ContentDialogButton.Primary :
                        (messageBox.DefaultResult == MessageBoxResult.No) ? ContentDialogButton.Secondary : ContentDialogButton.Close;
                    break;
            }
            return contentDialog;
        }

        public static MessageBoxResult ToMessageBoxResult(this MessageBoxViewModel messageBox, ContentDialogResult result)
        {
            switch (messageBox.Button)
            {
                case MessageBoxButton.OK:
                case MessageBoxButton.OKCancel:
                    if (result == ContentDialogResult.Primary)
                    {
                        return MessageBoxResult.OK;
                    }
                    break;
                case MessageBoxButton.YesNo:
                case MessageBoxButton.YesNoCancel:
                    if (result == ContentDialogResult.Primary)
                    {
                        return MessageBoxResult.Yes;
                    }
                    else if(result == ContentDialogResult.Secondary)
                    {
                        return MessageBoxResult.No;
                    }
                    break;
            }
            return MessageBoxResult.None;
        }
    }
}
