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
using Windows.System;
using Windows.UI.Popups;
using Projeto_Deploy.Models;
using Windows.ApplicationModel.Background;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.Foundation.Metadata;
using Windows.Media.SpeechSynthesis;
using Windows.ApplicationModel.Resources.Core;
using System.Linq;

//Desenvolvido por Igor Sanches em Anajatuba, Maranhão - Brasil

namespace Projeto_Deploy
{
    /// <summary>
    /// Uma página vazia que pode ser usada isoladamente ou navegada dentro de um Quadro.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //Atualizações da Live Tile
        private const string taskName = "TileUpdate";
        private const string taskEntrePoint = "TileUpdate.Tarefa";

     
        StorageFile packageInContext;
        List<Uri> dependencies = new List<Uri>();
        //ValueSet cannot contain values of the URI class which is why there is another list below.
        //This is required to update the progress in a notification using a background task.
        List<string> dependenciesAsString = new List<string>();

        public MainPage()
        {
            this.InitializeComponent();
            DataContext = this;
            DisplayN.Text = Package.Current.DisplayName;
            ProgressBar2.Visibility = Visibility.Collapsed;
            ProgressBar1.Visibility = Visibility.Collapsed;
            AppIcon.Text = "Deploy";
            RegisterBackgrountTask();
             //Inicie com o ideioma escolhido
             
            //Busque por respostas*
            //Verifique o idioma no DB.
            //E as configurações
         }

        
        

        //Ações em segundo plano
        private async void RegisterBackgrountTask()
        {
            var AccessStatus = await BackgroundExecutionManager.RequestAccessAsync();

            if (AccessStatus == BackgroundAccessStatus.AllowedMayUseActiveRealTimeConnectivity || AccessStatus == BackgroundAccessStatus.AllowedWithAlwaysOnRealTimeConnectivity)
            {
                foreach (var task in BackgroundTaskRegistration.AllTasks)
                {
                    if (task.Value.Name == taskName)
                    {
                        task.Value.Unregister(true);
                    }
                }

                BackgroundTaskBuilder taskBuild = new BackgroundTaskBuilder();
                taskBuild.Name = taskName;
                taskBuild.TaskEntryPoint = taskEntrePoint;
                taskBuild.SetTrigger(new TimeTrigger(15, false));
                var resgistracion = taskBuild.Register();
            }
        }

       
        //Menu Hamburger
        private void BotaoMenu(object sender, TappedRoutedEventArgs e)
        {
            MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
        }

        //Botão Informações
        private void BotaoInfo(object sender, TappedRoutedEventArgs e)
        {
            PrincipalPainel.Visibility = Visibility.Visible;
              MySplitView1.IsPaneOpen = !MySplitView1.IsPaneOpen;
        }
       
        //Botão para buscar os arquivos que tem suporte.
        private async void Buscar(object sender, RoutedEventArgs e)
        {
            //Ter suporte a .appx, .appxbundle e .xap caso o sistem aceito o formado
            var File1 = new FileOpenPicker();
            File1.FileTypeFilter.Add(".xap");
            File1.FileTypeFilter.Add(".appx");
            File1.FileTypeFilter.Add(".appxbundle");
            File1.FileTypeFilter.Add(".appxmanifest");
            File1.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            File1.ViewMode = PickerViewMode.List;
            StorageFile file = await File1.PickSingleFileAsync();
            if (File1 != null)
            {
              
                //Os suportados continuem para instalação.
                packageInContext = file;
                InstallText.Text = file.Path;
                InstallButton.IsEnabled = true;
            }

        }
       
        //Botão para buscar Dependências
        private async void Dependency(object sender, RoutedEventArgs e)
        {
            dependencies = new List<Uri>();
            var File1 = new FileOpenPicker();
            File1.FileTypeFilter.Add(".appx");
            File1.FileTypeFilter.Add(".appxbundle");
            File1.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            File1.ViewMode = PickerViewMode.List;
            var files = await File1.PickMultipleFilesAsync();
            if (File1 != null)
            {
                foreach (var Dependency in files)
                {
                    dependencies.Add(new Uri(Dependency.Path));
                }
                foreach (var Dependency in files)
                {
                    dependenciesAsString.Add(Dependency.Path);
                }
            }
        }

        //Navegação para esses parametros entes de todos
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

        //Botão da instalação
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
            Dep.IsEnabled = false;
            {
                //Crie um novo pacote.
                PackageManager pkgManager = new PackageManager();

                //Crie um progresso para instalação
                Progress<DeploymentProgress> progressCallback = new Progress<DeploymentProgress>(progress);

              
                if (dependencies != null && dependencies.Count > 0)
                {
                    try
                    {
                        var result = await pkgManager.AddPackageAsync(new Uri(packageInContext.Path), dependencies, DeploymentOptions.ForceTargetApplicationShutdown).AsTask(progressCallback);
                    }
                    catch (Exception X)
                    {
                       
                            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                            var text10 = loader.GetString("text10");
                            var text2 = loader.GetString("text2");
                            BorderOption.Visibility = Visibility.Visible;
                            InstallOption.IsEnabled = true;
                            RegisterOption.IsEnabled = true;
                            UpdateOption.IsEnabled = true;
                            ProgressBar1.Visibility = Visibility.Collapsed;
                            texterror.Text = text2 + X.Message;
                            InstallButton.IsEnabled = true;
                            InstallText.IsEnabled = true;
                            BrowserButton.IsEnabled = true;
                            Dep.IsEnabled = true;
                            ProgressValue.Text = text10;
                            texterror.Visibility = Visibility.Visible;
                         
                    }
                }
                else
                {
                    try
                    {
                        var result = await pkgManager.AddPackageAsync(new Uri(packageInContext.Path), null, DeploymentOptions.ForceTargetApplicationShutdown).AsTask(progressCallback);

                    }
                    catch (Exception x)
                    {
                         
                            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                            var text10 = loader.GetString("text10");
                            var text2 = loader.GetString("text2");
                            BorderOption.Visibility = Visibility.Visible;
                            InstallOption.IsEnabled = true;
                            RegisterOption.IsEnabled = true;
                            UpdateOption.IsEnabled = true;
                            ProgressBar1.Visibility = Visibility.Collapsed;
                            InstallButton.IsEnabled = true;
                            InstallText.IsEnabled = true;
                            BrowserButton.IsEnabled = true;
                            Dep.IsEnabled = true;
                            ProgressValue.Text = text10;
                            texterror.Visibility = Visibility.Visible;
                            texterror.Text = text2 + x.Message;
                        

                    }
                }

            }

        }

       
        //Progresso da Instalação
        private async void progress(DeploymentProgress progress)
        {
            
                var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                var text11 = loader.GetString("text11");
                var text12 = loader.GetString("text12");
                var text13 = loader.GetString("text13");
                texterror.Visibility = Visibility.Collapsed;
                double installPercentage = progress.percentage;
                string percentageAsString = String.Format($"{installPercentage}%");
                ProgressValue.Text = text11 + percentageAsString;
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
                    Dep.IsEnabled = true;
                    ProgressValue.Text = text12;
                    {
                        ToastTemplateType toastTemplate = ToastTemplateType.ToastImageAndText02;
                        XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);
                        XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
                        toastTextElements[0].AppendChild(toastXml.CreateTextNode(packageInContext.DisplayName));
                        toastTextElements[1].AppendChild(toastXml.CreateTextNode(text13));
                        XmlNodeList toastImageAttributes = toastXml.GetElementsByTagName("image");
                        ((XmlElement)toastImageAttributes[0]).SetAttribute("src", "ms-appx:///Assets/AppInstallAndUpadateAction.png");
                        ToastNotificationManager.CreateToastNotifier().Show(new ToastNotification(toastXml));
                    }

                }

            
             

        }


        private void InstallSelect(object sender, RoutedEventArgs e)
        {
           
        }
        private void RegisterSelect(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(PageResgistrar));
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