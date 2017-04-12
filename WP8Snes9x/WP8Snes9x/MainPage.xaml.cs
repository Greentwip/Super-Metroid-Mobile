using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PhoneDirect3DXamlAppInterop.Resources;
using Windows.Storage;
using System.Threading.Tasks;
using PhoneDirect3DXamlAppComponent;
using PhoneDirect3DXamlAppInterop.Database;
using Coding4Fun.Phone.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace PhoneDirect3DXamlAppInterop
{
    class ROMEntry
    {
        public String Name { get; set; }
    }

    class LoadROMParameter
    {
        public StorageFile file;
        public StorageFolder folder;
    }

    public partial class MainPage : PhoneApplicationPage
    {
        private ApplicationBarIconButton resumeButton;
        private ROMDatabase db;
        private Task createFolderTask, copyDemoTask, initTask;

        public MainPage()
        {
            InitializeComponent();

            this.initTask = this.Initialize();

            this.Loaded += MainPage_Loaded;
        }

        async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            await this.initTask;
            
            this.LoadInitialSettings();

            try
            {
                String romFileName = "rom.sfc";//NavigationContext.QueryString[FileHandler.ROM_URI_STRING];
                NavigationContext.QueryString.Remove(FileHandler.ROM_URI_STRING);

                ROMDBEntry entry = this.db.GetROM(romFileName);
                await this.StartROM(entry);
            }
            catch (KeyNotFoundException)
            { }
            catch (Exception)
            {
                MessageBox.Show(AppResources.TileOpenError, AppResources.ErrorCaption, MessageBoxButton.OK);
            }

            try
            {
                String importRomID = NavigationContext.QueryString["fileToken"];
                NavigationContext.QueryString.Remove("fileToken");

                ROMDBEntry entry = await FileHandler.ImportRomBySharedID(importRomID);
                await this.StartROM(entry);
            }
            catch (KeyNotFoundException)
            { }
            catch (Exception)
            {
                MessageBox.Show(AppResources.FileAssociationError, AppResources.ErrorCaption, MessageBoxButton.OK);
            }
        }

        private async Task Initialize()
        {
            createFolderTask = FileHandler.CreateInitialFolderStructure();
            copyDemoTask = this.CopyDemoROM();

            await createFolderTask;
            await copyDemoTask;

            this.db = ROMDatabase.Current;
            if (db.Initialize())
            {
                await FileHandler.FillDatabaseAsync();
            }
            this.db.Commit += () =>
            {
                
            };
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            //await this.createFolderTask;
            //await this.copyDemoTask;
            //await this.initTask;

            //this.LoadInitialSettings();

            //this.RefreshROMList();

            //this.resumeButton.IsEnabled = EmulatorPage.ROMLoaded;
            
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            FileHandler.UpdateLiveTile();

            base.OnNavigatedFrom(e);
        }

        private async Task CopyDemoROM()
        {
            IsolatedStorageSettings isoSettings = IsolatedStorageSettings.ApplicationSettings;

            if (!isoSettings.Contains("ROMCOPIED"))
            {
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                StorageFolder romFolder = await localFolder.CreateFolderAsync("roms", CreationCollisionOption.OpenIfExists);
                StorageFile file = await StorageFile.GetFileFromPathAsync("Assets/rom.sfc");
                await file.CopyAsync(romFolder);

                isoSettings["ROMCOPIED"] = true;
                isoSettings.Save();
            }
        }

        private class SettingsPage
        {
            public const String VControllerPosKey = "VirtualControllerOnTop";
            public const String VControllerSizeKey = "VirtualControllerLarge";
            public const String EnableSoundKey = "EnableSound";
            public const String LowFreqModeKey = "LowFrequencyModeNew";
            //public const String LowFreqModeMeasuredKey = "LowFrequencyModeMeasured";
            public const String VControllerButtonStyleKey = "VirtualControllerStyle";
            public const String OrientationKey = "Orientation";
            public const String ControllerScaleKey = "ControllerScale";
            public const String StretchKey = "FullscreenStretch";
            public const String OpacityKey = "ControllerOpacity";
            public const String SkipFramesKey = "SkipFramesKey";
            public const String ImageScalingKey = "ImageScalingKey";
            public const String TurboFrameSkipKey = "TurboSkipFramesKey";
            public const String SyncAudioKey = "SynchronizeAudioKey";
            public const String PowerSaverKey = "PowerSaveSkipKey";
            public const String DPadStyleKey = "DPadStyleKey";
            public const String DeadzoneKey = "DeadzoneKey";
            public const String CameraAssignKey = "CameraAssignmentKey";
            public const String ConfirmationKey = "ConfirmationKey";
            public const String ConfirmationLoadKey = "ConfirmationLoadKey";
            public const String AutoIncKey = "AutoIncKey";
            public const String SelectLastState = "SelectLastStateKey";
            public const String CreateManualSnapshotKey = "ManualSnapshotKey";
        }

        private void LoadInitialSettings()
        {
            EmulatorSettings settings = EmulatorSettings.Current;
            if (!settings.Initialized)
            {
                IsolatedStorageSettings isoSettings = IsolatedStorageSettings.ApplicationSettings;
                settings.Initialized = true;

                if (!isoSettings.Contains(SettingsPage.EnableSoundKey))
                {
                    isoSettings[SettingsPage.EnableSoundKey] = true;
                }
                if (!isoSettings.Contains(SettingsPage.VControllerPosKey))
                {
                    isoSettings[SettingsPage.VControllerPosKey] = false;
                }
                if (!isoSettings.Contains(SettingsPage.LowFreqModeKey))
                {
                    isoSettings[SettingsPage.LowFreqModeKey] = false;
                }
                //if (!isoSettings.Contains(SettingsPage.LowFreqModeMeasuredKey))
                //{
                //    isoSettings[SettingsPage.LowFreqModeMeasuredKey] = false;
                //}
                if (!isoSettings.Contains(SettingsPage.VControllerSizeKey))
                {
                    isoSettings[SettingsPage.VControllerSizeKey] = false;
                }
                if (!isoSettings.Contains(SettingsPage.VControllerButtonStyleKey))
                {
                    isoSettings[SettingsPage.VControllerButtonStyleKey] = false;
                }
                if (!isoSettings.Contains(SettingsPage.StretchKey))
                {
                    isoSettings[SettingsPage.StretchKey] = false;
                }
                if (!isoSettings.Contains(SettingsPage.OrientationKey))
                {
                    isoSettings[SettingsPage.OrientationKey] = 0;
                }
                if (!isoSettings.Contains(SettingsPage.ControllerScaleKey))
                {
                    isoSettings[SettingsPage.ControllerScaleKey] = 100;
                }
                if (!isoSettings.Contains(SettingsPage.OpacityKey))
                {
                    isoSettings[SettingsPage.OpacityKey] = 30;
                }
                if (!isoSettings.Contains(SettingsPage.SkipFramesKey))
                {
                    isoSettings[SettingsPage.SkipFramesKey] = 0;
                }
                if (!isoSettings.Contains(SettingsPage.TurboFrameSkipKey))
                {
                    isoSettings[SettingsPage.TurboFrameSkipKey] = 4;
                }
                if (!isoSettings.Contains(SettingsPage.SyncAudioKey))
                {
                    isoSettings[SettingsPage.SyncAudioKey] = true;
                }
                if (!isoSettings.Contains(SettingsPage.PowerSaverKey))
                {
                    isoSettings[SettingsPage.PowerSaverKey] = 0;
                }
                if (!isoSettings.Contains(SettingsPage.DPadStyleKey))
                {
                    isoSettings[SettingsPage.DPadStyleKey] = 0;
                }
                if (!isoSettings.Contains(SettingsPage.DeadzoneKey))
                {
                    isoSettings[SettingsPage.DeadzoneKey] = 10.0f;
                }
                if (!isoSettings.Contains(SettingsPage.ImageScalingKey))
                {
                    isoSettings[SettingsPage.ImageScalingKey] = 100;
                }
                if (!isoSettings.Contains(SettingsPage.CameraAssignKey))
                {
                    isoSettings[SettingsPage.CameraAssignKey] = 0;
                }
                if (!isoSettings.Contains(SettingsPage.ConfirmationKey))
                {
                    isoSettings[SettingsPage.ConfirmationKey] = false;
                }
                if (!isoSettings.Contains(SettingsPage.ConfirmationLoadKey))
                {
                    isoSettings[SettingsPage.ConfirmationLoadKey] = false;
                }
                if (!isoSettings.Contains(SettingsPage.AutoIncKey))
                {
                    isoSettings[SettingsPage.AutoIncKey] = false;
                }
                if (!isoSettings.Contains(SettingsPage.SelectLastState))
                {
                    isoSettings[SettingsPage.SelectLastState] = true;
                }
                if (!isoSettings.Contains(SettingsPage.CreateManualSnapshotKey))
                {
                    isoSettings[SettingsPage.CreateManualSnapshotKey] = false;
                }
                isoSettings.Save();

                settings.LowFrequencyMode = (bool)isoSettings[SettingsPage.LowFreqModeKey];
                settings.SoundEnabled = (bool)isoSettings[SettingsPage.EnableSoundKey];
                settings.VirtualControllerOnTop = (bool)isoSettings[SettingsPage.VControllerPosKey];
                //settings.LowFrequencyModeMeasured = (bool)isoSettings[SettingsPage.LowFreqModeMeasuredKey];
                settings.LargeVController = (bool)isoSettings[SettingsPage.VControllerSizeKey];
                settings.GrayVControllerButtons = (bool)isoSettings[SettingsPage.VControllerButtonStyleKey];
                settings.Orientation = (int)isoSettings[SettingsPage.OrientationKey];
                settings.FullscreenStretch = (bool)isoSettings[SettingsPage.StretchKey];
                settings.ControllerScale = (int)isoSettings[SettingsPage.ControllerScaleKey];
                settings.ControllerOpacity = (int)isoSettings[SettingsPage.OpacityKey];
                settings.FrameSkip = (int)isoSettings[SettingsPage.SkipFramesKey];
                settings.ImageScaling = (int)isoSettings[SettingsPage.ImageScalingKey];
                settings.TurboFrameSkip = (int)isoSettings[SettingsPage.TurboFrameSkipKey];
                settings.SynchronizeAudio = (bool)isoSettings[SettingsPage.SyncAudioKey];
                settings.PowerFrameSkip = (int)isoSettings[SettingsPage.PowerSaverKey];
                settings.DPadStyle = (int)isoSettings[SettingsPage.DPadStyleKey];
                settings.Deadzone = (float)isoSettings[SettingsPage.DeadzoneKey];
                settings.CameraButtonAssignment = (int)isoSettings[SettingsPage.CameraAssignKey];
                settings.AutoIncrementSavestates = (bool)isoSettings[SettingsPage.AutoIncKey];
                settings.HideConfirmationDialogs = (bool)isoSettings[SettingsPage.ConfirmationKey];
                settings.HideLoadConfirmationDialogs = (bool)isoSettings[SettingsPage.ConfirmationLoadKey];
                settings.SelectLastState = (bool)isoSettings[SettingsPage.SelectLastState];
                settings.ManualSnapshots = (bool)isoSettings[SettingsPage.CreateManualSnapshotKey];

                settings.SettingsChanged = this.SettingsChangedDelegate;
            }
        }

        private void SettingsChangedDelegate()
        {
            EmulatorSettings settings = EmulatorSettings.Current;
            IsolatedStorageSettings isoSettings = IsolatedStorageSettings.ApplicationSettings;

            isoSettings[SettingsPage.EnableSoundKey] = settings.SoundEnabled;
            isoSettings[SettingsPage.VControllerPosKey] = settings.VirtualControllerOnTop;
            isoSettings[SettingsPage.LowFreqModeKey] = settings.LowFrequencyMode;
            //isoSettings[SettingsPage.LowFreqModeMeasuredKey] = settings.LowFrequencyModeMeasured;
            isoSettings[SettingsPage.VControllerSizeKey] = settings.LargeVController;
            isoSettings[SettingsPage.VControllerButtonStyleKey] = settings.GrayVControllerButtons;
            isoSettings[SettingsPage.OrientationKey] = settings.Orientation;
            isoSettings[SettingsPage.StretchKey] = settings.FullscreenStretch;
            isoSettings[SettingsPage.ControllerScaleKey] = settings.ControllerScale;
            isoSettings[SettingsPage.OpacityKey] = settings.ControllerOpacity;
            isoSettings[SettingsPage.ImageScalingKey] = settings.ImageScaling;
            isoSettings[SettingsPage.TurboFrameSkipKey] = settings.TurboFrameSkip;
            isoSettings[SettingsPage.SyncAudioKey] = settings.SynchronizeAudio;
            isoSettings[SettingsPage.PowerSaverKey] = settings.PowerFrameSkip;
            isoSettings[SettingsPage.SkipFramesKey] = settings.FrameSkip;
            isoSettings[SettingsPage.DPadStyleKey] = settings.DPadStyle;
            isoSettings[SettingsPage.DeadzoneKey] = settings.Deadzone;
            isoSettings[SettingsPage.CameraAssignKey] = settings.CameraButtonAssignment;
            isoSettings[SettingsPage.ConfirmationKey] = settings.HideConfirmationDialogs;
            isoSettings[SettingsPage.AutoIncKey] = settings.AutoIncrementSavestates;
            isoSettings[SettingsPage.ConfirmationLoadKey] = settings.HideLoadConfirmationDialogs;
            isoSettings[SettingsPage.SelectLastState] = settings.SelectLastState;
            isoSettings[SettingsPage.CreateManualSnapshotKey] = settings.ManualSnapshots;
            isoSettings.Save();
        }

        private async void StartROMFromList(ListBox list)
        {
            if (list.SelectedItem == null)
                return;

            ROMDBEntry entry = (ROMDBEntry)list.SelectedItem;
            list.SelectedItem = null;

            await StartROM(entry);
        }

        private async Task StartROM(ROMDBEntry entry)
        {
            LoadROMParameter param = await FileHandler.GetROMFileToPlayAsync(entry.FileName);
            
            entry.LastPlayed = DateTime.Now;
            this.db.CommitChanges();

            PhoneApplicationService.Current.State["parameter"] = param;
            this.NavigationService.Navigate(new Uri("/EmulatorPage.xaml", UriKind.Relative));
        }
               
        void helpButton_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/HelpPage.xaml", UriKind.Relative));
        }

        void aboutItem_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));
        }

        void settingsButton_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
        }

        void importButton_Click(object sender, EventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/ImportPage.xaml", UriKind.Relative));
        }

        private void RenameListEntry(object sender, ListBox list)
        {
            ListBoxItem contextMenuListItem = list.ItemContainerGenerator.ContainerFromItem((sender as MenuItem).DataContext) as ListBoxItem;
            ROMDBEntry re = contextMenuListItem.DataContext as ROMDBEntry;
            InputPrompt prompt = new InputPrompt();
            prompt.Completed += (o, e2) =>
            {
                if (e2.PopUpResult == PopUpResult.Ok)
                {
                    if (String.IsNullOrWhiteSpace(e2.Result))
                    {
                        MessageBox.Show(AppResources.RenameEmptyString, AppResources.ErrorCaption, MessageBoxButton.OK);
                    }
                    else
                    {
                        if (e2.Result.ToLower().Equals(re.DisplayName.ToLower()))
                        {
                            return;
                        }
                        if (this.db.IsDisplayNameUnique(e2.Result))
                        {
                            re.DisplayName = e2.Result;
                            this.db.CommitChanges();
                            FileHandler.UpdateROMTile(re.FileName);
                        }
                        else
                        {
                            MessageBox.Show(AppResources.RenameNameAlreadyExisting, AppResources.ErrorCaption, MessageBoxButton.OK);
                        }
                    }
                }
            };
            prompt.Title = AppResources.RenamePromptTitle;
            prompt.Message = AppResources.RenamePromptMessage;
            prompt.Value = re.DisplayName;
            prompt.Show();
        }

        private static void PinToStart(object sender, ListBox list)
        {
            ListBoxItem contextMenuListItem = list.ItemContainerGenerator.ContainerFromItem((sender as MenuItem).DataContext) as ListBoxItem;
            ROMDBEntry re = contextMenuListItem.DataContext as ROMDBEntry;

            try
            {
                FileHandler.CreateROMTile(re);
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show(AppResources.MaximumTilesPinned);
            }
        }
        
    }
}