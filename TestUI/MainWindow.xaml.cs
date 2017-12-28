﻿using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using EvilBaschdi.Core.Application;
using EvilBaschdi.Core.Browsers;
using EvilBaschdi.Core.DirectoryExtensions;
using EvilBaschdi.Core.DotNetExtensions;
using EvilBaschdi.Core.Security;
using EvilBaschdi.Core.Threading;
using EvilBaschdi.Core.Wpf;
using EvilBaschdi.TestUI.Properties;
using Hardcodet.Wpf.TaskbarNotification;
using MahApps.Metro;
using MahApps.Metro.Controls;

namespace EvilBaschdi.TestUI
{
    /// <inheritdoc cref="MetroWindow" />
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public partial class MainWindow : MetroWindow
    {
        private readonly ISettings _coreSettings;
        private readonly IDialogService _dialogService;
        private readonly IEncryption _encryption;

        // ReSharper disable once NotAccessedField.Local
        private readonly IFilePath _filePath;

        private readonly int _overrideProtection;
        private readonly IMetroStyle _style;
        private INetworkBrowser _networkBrowser;

        public MainWindow()
        {
            InitializeComponent();
            IMultiThreadingHelper multiThreadingHelper = new MultiThreadingHelper();
            _encryption = new Encryption();
            _filePath = new FilePath(multiThreadingHelper);
            //LoadNetworkBrowserToArrayList();
            //MessageBox.Show(VersionHelper.GetWindowsClientVersion());

            IApplicationSettingsBaseHelper applicationSettingsBaseHelper = new ApplicationSettingsBaseHelper(Settings.Default);
            _coreSettings = new CoreSettings(applicationSettingsBaseHelper);
            IThemeManagerHelper themeManagerHelper = new ThemeManagerHelper();
            _style = new MetroStyle(this, Accent, ThemeSwitch, _coreSettings, themeManagerHelper);
            // ReSharper disable once UnusedVariable
            _style.Load();
            _dialogService = new DialogService(this);
            //flyout.Run();

            var filePath = Assembly.GetEntryAssembly()?.Location;
            if (filePath != null)
            {
                TestTaskbarIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(filePath);
            }
            var contextMenu = new ContextMenu();

            //foreach (string accentItem in _style.Accent.Items)
            //{
            //    var menuItem = new MenuItem
            //                   {
            //                       Header = accentItem
            //                   };

            //    contextMenu.Items.Add(menuItem);
            //}


            TestTaskbarIcon.ContextMenu = contextMenu;
            _overrideProtection = 1;
        }

        // ReSharper disable once UnusedMember.Local
        private void LoadNetworkBrowserToArrayList()
        {
            _networkBrowser = new NetworkBrowser();
            var networkBrowserValue = _networkBrowser.Value;
            if (networkBrowserValue != null && networkBrowserValue.Any())
            {
                UpdateCombo(cboNetwork, networkBrowserValue);
            }
            else
            {
                _dialogService.ShowMessage("Problem :-/", _networkBrowser.Exception.Message);
            }
        }

        private void BtnEncrypt_Click(object sender, RoutedEventArgs e)
        {
            txtEncrypted.Text = _encryption.EncryptString(txtInput.Text, "ABC");
        }

        private void BtnDecrypt_Click(object sender, RoutedEventArgs e)
        {
            txtOutput.Text = _encryption.DecryptString(txtEncrypted.Text, "ABC");
        }

        private void BtnCompare_Click(object sender, RoutedEventArgs e)
        {
            if (txtInput.Text == txtOutput.Text)
            {
                txtInput.Background = Brushes.DarkGreen;
                txtOutput.Background = Brushes.DarkGreen;
            }
            else
            {
                txtInput.Background = Brushes.DarkRed;
                txtOutput.Background = Brushes.DarkRed;
            }

            TestTaskbarIcon.Visibility = Visibility.Visible;
            TestTaskbarIcon.ShowBalloonTip("Wichtig!", "Hallo Welt...", BalloonIcon.Info);

            //var currentDirectory = Directory.GetCurrentDirectory();
            //var configuration = currentDirectory.EndsWith("Release") ? "Release" : "Debug";
            //var root = currentDirectory.Replace($@"EvilBaschdi.Core\TestUI\bin\{configuration}", "");
            //var coreProject = new CoreProject();
            //var coreNuGetPackagesConfig = currentDirectory.Replace($@"TestUI\bin\{configuration}", @"EvilBaschdi.Core\packages.config");
            //var includeList = new List<string>
            //                  {
            //                      "csproj"
            //                  };
            //var childProjects = _filePath.GetFileList(root, includeList, null).Where(file => !file.ToLower().Contains("evilbaschdi.core"));
            //var childConfigs = new ConcurrentBag<string>();


            #region nuget

            //var mahAppsId = "MahApps.Metro";
            //var corePackageConfig = new PackageConfig();
            //var coreCollection = corePackageConfig.Read(coreNuGetPackagesConfig);
            //var coreMahAppsVersion = corePackageConfig.Version(mahAppsId, coreCollection);
            //var coreMahAppsTargetFramework = corePackageConfig.TargetFramework(mahAppsId, coreCollection);
            //Parallel.ForEach(childConfigs,
            //    childConfig =>
            //    {
            //        var targetPackageConfig = new PackageConfig();
            //        var targetCollection = targetPackageConfig.Read(childConfig);
            //        var targetMahAppsVersion = targetPackageConfig.Version(mahAppsId, targetCollection);
            //        if (!string.IsNullOrWhiteSpace(targetMahAppsVersion) && targetMahAppsVersion != coreMahAppsVersion)
            //        {
            //            targetPackageConfig.SetVersion(mahAppsId, coreMahAppsVersion, targetCollection);
            //            targetPackageConfig.Write(childConfig, targetCollection);
            //        }
            //    });

            #endregion nuget
        }

        private void UpdateCombo(Selector selector, IEnumerable enumerable)
        {
            selector.Items.Clear();
            foreach (string value in enumerable)
            {
                selector.Items.Add(value);
            }
            selector.SelectedIndex = 0;
        }

        private void CustomColorOnLostFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(CustomColor.Text))
            {
                try
                {
                    var themeManagerHelper = new ThemeManagerHelper();
                    themeManagerHelper.CreateAppStyleBy(CustomColor.Text.ToColor(), CustomColor.Text);

                    var styleAccent = ThemeManager.GetAccent(CustomColor.Text);
                    var styleTheme = ThemeManager.GetAppTheme(_coreSettings.Theme);
                    ThemeManager.ChangeAppStyle(Application.Current, styleAccent, styleTheme);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                }
            }
        }

        #region Fly-out

        private void ToggleSettingsFlyoutClick(object sender, RoutedEventArgs e)
        {
            ToggleFlyout(0);
        }

        private void ToggleFlyout(int index, bool stayOpen = false)
        {
            var activeFlyout = (Flyout) Flyouts.Items[index];
            if (activeFlyout == null)
            {
                return;
            }

            foreach (
                var nonactiveFlyout in
                Flyouts.Items.Cast<Flyout>()
                       .Where(nonactiveFlyout => nonactiveFlyout.IsOpen && nonactiveFlyout.Name != activeFlyout.Name))
            {
                nonactiveFlyout.IsOpen = false;
            }

            if (activeFlyout.IsOpen && stayOpen)
            {
                activeFlyout.IsOpen = true;
            }
            else
            {
                activeFlyout.IsOpen = !activeFlyout.IsOpen;
            }
        }

        #endregion Fly-out

        #region MetroStyle

        private void SaveStyleClick(object sender, RoutedEventArgs e)
        {
            if (_overrideProtection != 0)
            {
                _style.SaveStyle();
            }
        }

        private void Theme(object sender, EventArgs e)
        {
            if (_overrideProtection != 0)
            {
                _style.SetTheme(sender);
            }
        }

        private void AccentOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_overrideProtection != 0)
            {
                _style.SetAccent(sender, e);
            }
        }

        #endregion MetroStyle
    }
}