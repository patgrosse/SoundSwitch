﻿using System;
using System.Windows.Forms;
using AudioEndPointControllerWrapper;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.NotificationManager.Notification;
using SoundSwitch.Framework.NotificationManager.Notification.Configuration;
using SoundSwitch.Model;
using SoundSwitch.Properties;

namespace SoundSwitch.Framework.NotificationManager
{
    public class NotificationManager
    {
        private readonly IAppModel _model;
        private string _lastDeviceId;
        private INotification _notification;
        private readonly NotificationFactory _notificationFactory;

        public NotificationManager(IAppModel model)
        {
            _model = model;
            _notificationFactory = new NotificationFactory();
        }

        public void Init()
        {
            _model.DefaultDeviceChanged += ModelOnDefaultDeviceChanged;
            _model.NotificationSettingsChanged += ModelOnNotificationSettingsChanged;
            SetNotification(_model.NotificationSettings);
            _model.CustomSoundChanged += ModelOnCustomSoundChanged;
        }

        private void ModelOnCustomSoundChanged(object sender, CustomSoundChangedEvent customSoundChangedEvent)
        {
            _notification.OnSoundChanged(customSoundChangedEvent.NewSound);
        }

        private void ModelOnNotificationSettingsChanged(object sender, NotificationSettingsUpdatedEvent notificationSettingsUpdatedEvent)
        {
            var notificationTypeEnum = notificationSettingsUpdatedEvent.NewSettings;
            SetNotification(notificationTypeEnum);
        }

        private void SetNotification(NotificationTypeEnum notificationTypeEnum)
        {
            _notification = _notificationFactory.Get(notificationTypeEnum);
            _notification.Configuration = new NotificationConfiguration()
            {
                Icon = AppModel.Instance.TrayIcon.NotifyIcon,
                DefaultSound = Resources.NotificationSound
            };
            try
            {
                _notification.Configuration.CustomSound = AppModel.Instance.CustomNotificationSound;
            }
            catch (CachedSoundFileNotExistsException)
            {
                if (!_notification.NeedCustomSound())
                {
                    return;
                }

                MessageBox.Show(string.Format(Properties.Notifications.AudioFileNotFound, Application.ProductName,
                        Properties.Notifications.NotifSound),Properties.Notifications.AudioFileNotFoundTitle,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                _model.NotificationSettings = NotificationTypeEnum.SoundNotification;
            }
        }

        private void ModelOnDefaultDeviceChanged(object sender, DeviceDefaultChangedEvent deviceDefaultChangedEvent)
        {
            if (_lastDeviceId == deviceDefaultChangedEvent.device.Id)
                return;

           
            _notification.NotifyDefaultChanged(deviceDefaultChangedEvent.device);
            _lastDeviceId = deviceDefaultChangedEvent.device.Id;
        }

        ~NotificationManager()
        {
            _model.DefaultDeviceChanged -= ModelOnDefaultDeviceChanged;
            _model.NotificationSettingsChanged -= ModelOnNotificationSettingsChanged;
            _model.CustomSoundChanged -= ModelOnCustomSoundChanged;
        }
    }
}