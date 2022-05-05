using Microsoft.UI.Xaml.Controls;

namespace Mmfm.ViewModel
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
                    contentDialog.PrimaryButtonText = Properties.Resources.MessageBox_OK;
                    contentDialog.DefaultButton = ContentDialogButton.Primary;
                    break;
                case MessageBoxButton.OKCancel:
                    contentDialog.PrimaryButtonText = Properties.Resources.MessageBox_OK;
                    contentDialog.CloseButtonText = Properties.Resources.MessageBox_Cancel;
                    contentDialog.DefaultButton = (messageBox.DefaultResult == MessageBoxResult.OK) ? ContentDialogButton.Primary : ContentDialogButton.Close;
                    break;
                case MessageBoxButton.YesNo:
                    contentDialog.PrimaryButtonText = Properties.Resources.MessageBox_Yes;
                    contentDialog.SecondaryButtonText = Properties.Resources.MessageBox_No;
                    contentDialog.DefaultButton = (messageBox.DefaultResult == MessageBoxResult.Yes) ? ContentDialogButton.Primary : ContentDialogButton.Secondary;
                    break;
                case MessageBoxButton.YesNoCancel:
                    contentDialog.PrimaryButtonText = Properties.Resources.MessageBox_Yes;
                    contentDialog.SecondaryButtonText = Properties.Resources.MessageBox_No;
                    contentDialog.CloseButtonText = Properties.Resources.MessageBox_Cancel;
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
