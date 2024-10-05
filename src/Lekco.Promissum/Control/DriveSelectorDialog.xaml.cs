using Lekco.Promissum.Model.Sync.Base;
using Lekco.Promissum.Model.Sync.Disk;
using Lekco.Promissum.Model.Sync.MTP;
using Lekco.Wpf.Control;
using Lekco.Wpf.MVVM.Command;
using Lekco.Wpf.Utility;
using MediaDevices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace Lekco.Promissum.Control
{
    public partial class DriveSelectorDialog : CustomWindow, IInteractive, INotifyPropertyChanged
    {
        public IEnumerable<DiskDrive>? DiskDrives { get; }

        public IEnumerable<MTPDrive>? MTPDrives { get; }

        public IEnumerable? Drives { get; set; }

        public DriveBase? SelectedDrive { get; set; }

        public bool DiskEnabled { get; }

        public bool MTPEnabled { get; }

        public bool DiskChecked { get; set; }

        public bool MTPChecked { get; set; }

        public bool CanOK { get; set; }

        public RelayCommand CheckedCommand => new RelayCommand(Checked);

        public RelayCommand<DriveBase> DriveCheckedCommand => new RelayCommand<DriveBase>(DriveChecked);

        public ICommand OKCommand => new RelayCommand(OK);

        public ICommand CancelCommand => new RelayCommand(Cancel);

        public bool IsOK { get; protected set; }

        protected DriveSelectorDialog(DriveCategory category)
        {
            InitializeComponent();
            DataContext = this;

            if (((int)category & 0x10) > 0)
            {
                MTPDrives = MediaDevice.GetDevices().Select(drive => new MTPDrive(drive));
                MTPEnabled = true;
                MTPChecked = true;
                Drives = MTPDrives;
            }
            if (((int)category & 0x01) > 0)
            {
                DiskDrives = DriveInfo.GetDrives().Select(drive => new DiskDrive(drive));
                DiskEnabled = true;
                MTPChecked = false;
                DiskChecked = true;
                Drives = DiskDrives;
            }
        }

        private void OK()
        {
            IsOK = true;
            Close();
        }

        private void Cancel()
        {
            Close();
        }

        private void DriveChecked(DriveBase drive)
        {
            SelectedDrive = drive;
            CanOK = SelectedDrive != null;
        }

        private void Checked()
        {
            if (MTPChecked)
            {
                Drives = MTPDrives;
            }
            if (DiskChecked)
            {
                Drives = DiskDrives;
            }

            SelectedDrive = null;
            CanOK = false;
        }

        private void DoubleClick(object sender, MouseButtonEventArgs e)
        {
            OK();
        }

        public static bool ShowDialog(DriveCategory category, [MaybeNullWhen(false)] out DriveBase targetDrive)
        {
            targetDrive = null;
            var dialog = new DriveSelectorDialog(category);
            dialog.ShowDialog();
            if (dialog.IsOK)
            {
                targetDrive = dialog.SelectedDrive!;
                return true;
            }
            return false;
        }
    }

    [Flags]
    public enum DriveCategory
    {
        None = 0x00,
        DiskDrive = 0x01,
        MTPDrive = 0x10,
        All = DiskDrive | MTPDrive,
    }
}