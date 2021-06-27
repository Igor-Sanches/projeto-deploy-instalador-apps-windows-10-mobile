using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.UI.Notifications;//0e1a22    //0a4046

namespace TileUpdate
{
    public sealed class Tarefa : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();
            StorageFolder local = ApplicationData.Current.LocalFolder;
            var dataFolder1 = await local.CreateFolderAsync("Install", CreationCollisionOption.OpenIfExists);
            var file = await dataFolder1.CreateFileAsync("InstallTimeAndName.txt", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, $"Deploy\n{DateTime.Now}");

            var dataFolder = await local.GetFolderAsync("Install");
            var fileTheme = await dataFolder.GetFileAsync("InstallTimeAndName.txt");
            String ThemeSettings = await FileIO.ReadTextAsync(fileTheme);

            ChamaTile(ThemeSettings);

            deferral.Complete();

        }

        private void ChamaTile(string message)
        {
            var tile = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWide310x150BlockAndText01);
            var tile2 = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150Text01);

            var tileAtributos = tile.GetElementsByTagName("text");
            tileAtributos[0].AppendChild(tile.CreateTextNode(message));

            var tileNotificar = new TileNotification(tile);
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotificar);

            var tileAtributos2 = tile2.GetElementsByTagName("text");
            tileAtributos2[0].AppendChild(tile2.CreateTextNode(message));

            var tileNotificar2 = new TileNotification(tile);
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotificar2);
        }


    }
}
