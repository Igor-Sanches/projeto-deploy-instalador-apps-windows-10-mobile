using Windows.Data.Xml.Dom;
using Windows.Storage.Pickers;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Management.Deployment;
using Windows.Storage;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel;
using Windows.UI.Popups;
using System.Collections;
using Windows.System;
using Windows.UI.Xaml.Media;
using Windows.UI;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Projeto_Deploy
{
    public sealed partial class PageResgistrar : Page
    {
        StorageFile packageInContext;
        List<Uri> dependency = new List<Uri>();
        //ValueSet cannot contain Values of the URI class which is why there is abother list below.
        //This is required to update the progress in ProgressBar1 using a background task.
        List<string> dependencyAsString = new List<string>();
        public PageResgistrar()
        {
            this.InitializeComponent();
            DataContext = this;
            DisplayN.Text = Package.Current.DisplayName;
            Versao.Text = CapVersion;
            ProgressBar1.Visibility = Visibility.Collapsed;
            AppIcon.Text = Package.Current.DisplayName;
        }
        public string CapVersion
        {
            get
            {
                var CapVersion = Package.Current.Id.Version;
                return $"{CapVersion.Major}.{CapVersion.Minor}.{CapVersion.Build}.{CapVersion.Revision}";
            }
        }


        private void BotaoMenu(object sender, TappedRoutedEventArgs e)
        {
            MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
        }
        private void BotaoInfo(object sender, TappedRoutedEventArgs e)
        {
            MySplitView1.IsPaneOpen = !MySplitView1.IsPaneOpen;
        }
        /// <summary>
        /// Retreives an appx/appxbundle file using the fole picker
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Buscar(object sender, RoutedEventArgs e)
        {
            var File1 = new FileOpenPicker();
            File1.FileTypeFilter.Add(".xml");
            File1.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            File1.ViewMode = PickerViewMode.List;
            StorageFile file = await File1.PickSingleFileAsync();
            if (File1 != null)
            {
                //UI changes to allow the user to install the package
                packageInContext = file;
                InstallText.Text = file.Path;
                InstallButton.IsEnabled = true;
            }

        }
        private async void Dependency(object sender, RoutedEventArgs e)
        {
            dependency = new List<Uri>();
            var File1 = new FileOpenPicker();
            File1.FileTypeFilter.Add(".xap");
            File1.FileTypeFilter.Add(".appx");
            File1.FileTypeFilter.Add(".appxbundle");
            File1.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            File1.ViewMode = PickerViewMode.List;
            var files = await File1.PickMultipleFilesAsync();
            if (File1 != null)
            {
                foreach (var Dependency in files)
                {
                    dependency.Add(new Uri(Dependency.Path));
                }
                foreach (var Dependency in files)
                {
                    dependencyAsString.Add(Dependency.Path);
                }
            }
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            try
            {
                StorageFile package = (StorageFile)e.Parameter;
                packageInContext = package;
            }
            catch (Exception x) { Debug.WriteLine(x.Message); }
        }

        private async void Install(object sender, RoutedEventArgs e)
        {
            BorderOption.Visibility = Visibility.Collapsed;
            InstallOption.IsEnabled = false;
            RegisterOption.IsEnabled = false;
            UpdateOption.IsEnabled = false;
            ProgressBar1.Visibility = Visibility.Visible;
            InstallButton.IsEnabled = false;
            InstallText.IsEnabled = false;
            BrowserButton.IsEnabled = false;
            {
                PackageManager pkgPackage = new PackageManager();
                Progress<DeploymentProgress> progress = new Progress<DeploymentProgress>(inatallProgress);
                if (dependency != null && dependency.Count > 0)
                {
                    try
                    {
                        var result = await pkgPackage.RegisterPackageAsync(new Uri(packageInContext.Path), dependency, DeploymentOptions.DevelopmentMode).AsTask(progress);
                    }
                    catch (Exception X)
                    {
                        BorderOption.Visibility = Visibility.Visible;
                        InstallOption.IsEnabled = true;
                        RegisterOption.IsEnabled = true;
                        UpdateOption.IsEnabled = true;
                        ProgressBar1.Visibility = Visibility.Collapsed;
                        var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                        var text9 = loader.GetString("text9");
                        var text2 = loader.GetString("text2");
                        texterror.Text = text2 + X.Message;
                        InstallButton.IsEnabled = true;
                        InstallText.IsEnabled = true;
                        BrowserButton.IsEnabled = true;
                        ProgressValue.Text = text9;
                        texterror.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    try
                    {
                        var result = await pkgPackage.RegisterPackageAsync(new Uri(packageInContext.Path), null, DeploymentOptions.DevelopmentMode).AsTask(progress);
                    }
                    catch (Exception x)
                    {
                        BorderOption.Visibility = Visibility.Visible;
                        InstallOption.IsEnabled = true;
                        RegisterOption.IsEnabled = true;
                        UpdateOption.IsEnabled = true;
                        ProgressBar1.Visibility = Visibility.Collapsed;
                        var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                        var text9 = loader.GetString("text9");
                        var text2 = loader.GetString("text2");
                        InstallButton.IsEnabled = true;
                        InstallText.IsEnabled = true;
                        BrowserButton.IsEnabled = true;
                        ProgressValue.Text = text9;
                        texterror.Visibility = Visibility.Visible;
                        texterror.Text = text2 + x.Message;
                    }
                }

            }
        }

       
        private void inatallProgress(DeploymentProgress inatallProgress)
        {
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            var text8 = loader.GetString("text8");
            var text7 = loader.GetString("text7");
            var text6 = loader.GetString("text6");
            var I = loader.GetString("Parentes1");
            var E = loader.GetString("Parentes2");
            double installPercentage = inatallProgress.percentage;
            string percentageAsString = String.Format($"{installPercentage}%");
            ProgressValue.Text = text8 + percentageAsString;
            texterror.Visibility = Visibility.Collapsed;
            ProgressBar1.Value = installPercentage;
            texterror.Visibility = Visibility.Collapsed;
            if (installPercentage == 100)
            {
                BorderOption.Visibility = Visibility.Visible;
                InstallOption.IsEnabled = true;
                RegisterOption.IsEnabled = true;
                UpdateOption.IsEnabled = true;
                ProgressBar1.Visibility = Visibility.Collapsed;
                InstallButton.IsEnabled = true;
                InstallText.IsEnabled = true;
                BrowserButton.IsEnabled = true;
                ProgressValue.Text = text7;
                {
                    ToastTemplateType toastTemplate = ToastTemplateType.ToastImageAndText02;
                    XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);
                    XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
                    toastTextElements[0].AppendChild(toastXml.CreateTextNode(packageInContext.DisplayName));
                    toastTextElements[1].AppendChild(toastXml.CreateTextNode(text6 + I + packageInContext.Path + E));
                    XmlNodeList toastImageAttributes = toastXml.GetElementsByTagName("image");
                    ((XmlElement)toastImageAttributes[0]).SetAttribute("src", "ms-appx:///Assets/AppRegisterAction.png");
                    ToastNotificationManager.CreateToastNotifier().Show(new ToastNotification(toastXml));
                }

            }
        }
        private void InstallSelect(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }
        private void RegisterSelect(object sender, RoutedEventArgs e)
        {

        }
        private void UpdateSelect(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(PageAtualizar));
        }
       
        private async void Blog(object sender, TappedRoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://meu-windows10channel.blogspot.com.br"));
        }

        private async void UpdatesVeri(object sender, TappedRoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://meu-windows10channel.blogspot.com.br/2016/11/deploy-updates.html"));

        }

        private async void GrupoFace(object sender, TappedRoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://m.facebook.com/?hrc=1&refsrc=http:%2F%2Fh.facebook.com%2Fhr%2Fr&_rdr#!/groups/106958043266090?ref=bookmarks"));

        }

        private async void Canal(object sender, TappedRoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://m.youtube.com/channel/UCSgEAP2j1myZ9htqlaR-qkg"));

        }

        private async void PaginaFace(object sender, TappedRoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://m.facebook.com/Windows10Channel/#!/Windows10Channel/?ref=bookmarks"));
        }

        private async void Support(object sender, TappedRoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("mailto:igorsanchesinc.17@hotmail.com"));

        }

        private async void Instagram(object sender, TappedRoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("http://www.instagram.com/igor_sanches"));

        }

        private async void Twitter(object sender, TappedRoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("http://www.twitter.com/igordutra2014"));

        }

        private async void Facebook(object sender, TappedRoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("http://www.facebook.com/igor.dutra.3557"));

        }
    }
}
