﻿using EnhancePoE.Model;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace EnhancePoE
{
    /// <summary>
    /// Interaction logic for ChaosRecipeEnhancer.xaml
    /// </summary>
    public partial class ChaosRecipeEnhancer : Window, INotifyPropertyChanged
    {


        public static bool FetchingActive { get; set;} = false;
        private static System.Timers.Timer aTimer;

        private static readonly double deactivatedOpacity = .1;
        private static readonly double activatedOpacity = 1;

        private string _warningMessage;
        public string WarningMessage
        {
            get
            {
                return _warningMessage;
            }
            set
            {
                _warningMessage = value;
                OnPropertyChanged("WarningMessage");
            }
        }

        private Visibility _warningMessageVisibility = Visibility.Hidden;
        public Visibility WarningMessageVisibility
        {
            get
            {
                return _warningMessageVisibility;
            }
            set
            {
                _warningMessageVisibility = value;
                OnPropertyChanged("WarningMessageVisibility");
            }
        }
        private double _shadowOpacity = 0;
        public double ShadowOpacity
        {
            get
            {
                return _shadowOpacity;
            }
            set
            {
                _shadowOpacity = value;
                OnPropertyChanged("ShadowOpacity");
            }
        }

        public static int FullSets { get; set; } = 0;

        private static bool isOpen = false;

        public ChaosRecipeEnhancer()
        {
            InitializeComponent();
            DataContext = this;
            aTimer = new System.Timers.Timer();
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = false;
            this.FullSetsTextBlock.Text = "0";
        }


        private async void FetchData()
        {

            MainWindow.overlay.WarningMessage = "";
            MainWindow.overlay.ShadowOpacity = 0;
            MainWindow.overlay.WarningMessageVisibility = System.Windows.Visibility.Hidden;
            await this.Dispatcher.Invoke(async() =>
            {
                GetFrequency();

                await ApiAdapter.GenerateUri();

                await ApiAdapter.GetItems();
            });
            if (ApiAdapter.FetchingDone)
            {
                Data.CheckActives();
                SetOpacity();
            }
        }


        public void RunFetching()
        {
            if (MainWindow.SettingsComplete)
            {
                if (!isOpen)
                {
                    return;
                }
                if (aTimer.Enabled)
                {
                    aTimer.Enabled = false;
                    FetchingActive = false;
                    RefreshButton.Content = "Fetch";
                }
                else
                {
                    if (MainWindow.stashTabOverlay.IsOpen)
                    {
                        MainWindow.stashTabOverlay.Hide();
                    }
                    FetchData();
                    //aTimer.Interval = 1000;
                    aTimer.Enabled = true;
                    FetchingActive = true;
                    RefreshButton.Content = "Stop";
                }
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RunFetching();
        }

        // TODO: find better algo for getting frequency, maybe implementing response header thresholds
        private static void GetFrequency()
        {
            int addedTime = 0;
            if(Properties.Settings.Default.StashTabIndices != "")
            {
                addedTime += (Properties.Settings.Default.StashTabIndices.Length / 2) * 1000;
            }
            aTimer.Interval = (Properties.Settings.Default.RefreshRate * 1000) + addedTime;
        }

        private void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            FetchData();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {

            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void SetOpacity()
        {

            Dispatcher.Invoke(() =>
            {
                if (!Data.ActiveItems.HelmetActive)
                {
                    Helmet.Opacity = deactivatedOpacity;
                }
                else
                {
                    Helmet.Opacity = activatedOpacity;
                }
                if (!Data.ActiveItems.GlovesActive)
                {
                    Gloves.Opacity = deactivatedOpacity;
                }
                else
                {
                    Gloves.Opacity = activatedOpacity;
                }
                if (!Data.ActiveItems.BootsActive)
                {
                    Boots.Opacity = deactivatedOpacity;
                }
                else
                {
                    Boots.Opacity = activatedOpacity;
                }
                if (!Data.ActiveItems.WeaponActive)
                {
                    Weapon.Opacity = deactivatedOpacity;
                }
                else
                {
                    Weapon.Opacity = activatedOpacity;
                }
                if (!Data.ActiveItems.ChestActive)
                {
                    Chest.Opacity = deactivatedOpacity;
                }
                else
                {
                    Chest.Opacity = activatedOpacity;
                }
            });
        }

        public new virtual void Hide()
        {
            isOpen = false;

            aTimer.Enabled = false;
            base.Hide();
        }

        public new virtual void Show()
        {


            isOpen = true;
            if (FetchingActive)
            {
                aTimer.Enabled = true;
                FetchData();
            }

            base.Show();

        }

        private void OpenStashTabOverlay_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.RunStashTabOverlay();
        }

        private void HandleEditButton()
        {
            if (MainWindow.stashTabOverlay.IsEditing)
            {
                MainWindow.stashTabOverlay.IsEditing = false;
                MainWindow.stashTabOverlay.Transparentize();
                EditStashTabOverlay.Content = "Edit";
                MouseHook.Start();
            }
            else
            {
                MouseHook.Stop();
                MainWindow.stashTabOverlay.IsEditing = true;
                MainWindow.stashTabOverlay.Normalize();
                EditStashTabOverlay.Content = "Save";
            }
        }

        private void EditStashTabOverlay_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.stashTabOverlay.IsOpen)
            {
                HandleEditButton();
            }
            else
            {
                MainWindow.RunStashTabOverlay();
                HandleEditButton();
            }
        }

        //private void Window_Deactivated(object sender, EventArgs e)
        //{
        //    // The Window was deactivated 
        //    MainWindow.overlay.Topmost = true;
        //}

        #region INotifyPropertyChanged implementation
        // Basically, the UI thread subscribes to this event and update the binding if the received Property Name correspond to the Binding Path element
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion


    }
}
