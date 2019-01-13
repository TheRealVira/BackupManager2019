using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using BackupManager.Annotations;
using Ionic.Zip;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace BackupManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
            Manager = new MyManager("");

            DataContext = Manager;
        }

        public MyManager Manager;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog
            {
                InitialDirectory = "C:\\Users", IsFolderPicker = true
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Manager.SelectedDirectories += Environment.NewLine + dialog.FileName;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonSaveFileDialog
            {
                InitialDirectory = "C:\\Users",
                Filters = { new CommonFileDialogFilter("PackageManagerExtension (PME)", "*.PME")}
            };

            if (dialog.ShowDialog() != CommonFileDialogResult.Ok) return;
            SafeListOfDirectories(dialog.FileName);
        }

        private void SafeListOfDirectories(string path)
        {
            if (!path.EndsWith(".PME"))
            {
                path += ".PME";
            }

            using (var writer = new StreamWriter(path))
            {
                writer.Write(Manager.SelectedDirectories);
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog
            {
                InitialDirectory = "C:\\Users",
                Filters = { new CommonFileDialogFilter("PackageManagerExtension (PME)", "*.PME")}
            };

            if (dialog.ShowDialog() != CommonFileDialogResult.Ok) return;
            using (var reader = new StreamReader(dialog.FileName))
            {
                Manager.SelectedDirectories = reader.ReadToEnd();
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog
            {
                InitialDirectory = "C:\\Users", IsFolderPicker = true
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Manager.OutputDirectory = dialog.FileName;
            }
        }

        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Btn_Backup.Visibility = Manager.OutputDirectory.Equals(string.Empty)?Visibility.Hidden:Visibility.Visible;
            btn_blurredNdisabled.Visibility = Btn_Backup.Visibility.Equals(Visibility.Hidden)
                ? Visibility.Visible
                : Visibility.Hidden;
        }

        private void Btn_Backup_Click(object sender, RoutedEventArgs e)
        {
            Txtbx_Progress.Visibility = Visibility.Visible;

            Task.Factory.StartNew(BackupDataBackgroundWork);
        }

        private void BackupDataBackgroundWork()
        {
            try
            {
                var backupName = (DateTime.Now.ToLongDateString() + ":" + DateTime.Now.ToShortTimeString())
                    .Replace(':', '_').Replace(' ', '_');

                var chckbxDataAsNameValue = false;

                Dispatcher.Invoke(() =>
                {
                    chckbxDataAsNameValue =
                        Chckbx_DateAsName.IsChecked != null && !Chckbx_DateAsName.IsChecked.Value;
                });

                if (chckbxDataAsNameValue)
                {
                    var chooseOK = false;
                    var name = string.Empty;
                    Application.Current.Dispatcher.Invoke( () =>
                    {
                        var chooseNameDialog = new EnterNameDialog();
                        chooseOK = (bool) chooseNameDialog.ShowDialog();
                        if (chooseOK)
                        {
                            name = chooseNameDialog.ResponseText;
                        }
                    });
                    if (chooseOK)
                    {
                        backupName = name;
                    }
                    else
                    {
                        return;
                    }
                }

                backupName = $"{Manager.OutputDirectory}\\{backupName}";
                Directory.CreateDirectory(backupName);
                SafeListOfDirectories(backupName + "\\backupData");

                var chckbxCompressedValue = false;

                Dispatcher.Invoke(() =>
                {
                    chckbxCompressedValue = Chckbx_Compress.IsChecked != null && !Chckbx_Compress.IsChecked.Value;
                });

                if (chckbxCompressedValue)
                {
                    foreach (var sourceDirectory in Manager.SelectedDirectories.Split(new[] {Environment.NewLine},
                        StringSplitOptions.None))
                    {
                        if (!Directory.Exists(sourceDirectory))
                        {
                            continue;
                        }
                        
                        Dispatcher.Invoke(() => { Txtbx_Progress.Text += Environment.NewLine + "Copying: "+sourceDirectory; });
                        DirectoryCopy(sourceDirectory, backupName + "\\" + sourceDirectory.Substring(3), true);
                    }
                }
                else
                {
                    BackupDataUsingZipTechs(backupName);
                }

                MessageBox.Show("Finished backing up your data! Have a great day!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("An exception has been thrown..:" + Environment.NewLine + ex.Message);
            }
            finally
            {
                Dispatcher.Invoke(() =>
                {
                    Txtbx_Progress.Visibility = Visibility.Hidden;
                    Txtbx_Progress.Clear();
                });
            }
        }

        private void BackupDataUsingZipTechs(string backupPath)
        {
            using (var zip = new ZipFile())
            {
                foreach (var sourceDirectory in Manager.SelectedDirectories.Split(
                    new[] {Environment.NewLine},
                    StringSplitOptions.None))
                {
                    if (!Directory.Exists(sourceDirectory))
                    {
                        continue;
                    }

                    zip.AddDirectory(sourceDirectory,sourceDirectory.Substring(3));
                    Dispatcher.Invoke(() => { Txtbx_Progress.Text += Environment.NewLine + "Adding: "+sourceDirectory; });
                }

                zip.Comment = "This zip was created at " + DateTime.Now.ToString("G");
                
                Dispatcher.Invoke(() => { Txtbx_Progress.Text += Environment.NewLine + "Saving files..."; });
                zip.Save(backupPath + "\\backup.zip");
            }
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            var dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            var dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            var files = dir.GetFiles();
            foreach (var file in files)
            {
                var temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (!copySubDirs) return;
            {
                foreach (var subdir in dirs)
                {
                    var temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
    }
}
