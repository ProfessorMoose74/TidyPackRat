using System;
using System.IO;
using System.Media;
using System.Reflection;
using System.Windows.Forms;

namespace TidyPackRat.Helpers
{
    /// <summary>
    /// Handles Windows notifications and sound effects
    /// </summary>
    public static class NotificationHelper
    {
        private static NotifyIcon _notifyIcon;
        private static bool _soundEnabled = true;

        /// <summary>
        /// Initialize the notification system with a notify icon
        /// </summary>
        public static void Initialize(NotifyIcon notifyIcon)
        {
            _notifyIcon = notifyIcon;
        }

        /// <summary>
        /// Enable or disable sound effects
        /// </summary>
        public static bool SoundEnabled
        {
            get => _soundEnabled;
            set => _soundEnabled = value;
        }

        /// <summary>
        /// Show a balloon notification from the system tray
        /// </summary>
        public static void ShowNotification(string title, string message, ToolTipIcon icon = ToolTipIcon.Info, int timeout = 3000)
        {
            if (_notifyIcon != null && _notifyIcon.Visible)
            {
                _notifyIcon.ShowBalloonTip(timeout, title, message, icon);
            }
        }

        /// <summary>
        /// Show notification when files are organized
        /// </summary>
        public static void ShowOrganizedNotification(int fileCount, long totalBytes)
        {
            if (fileCount == 0)
            {
                ShowNotification(
                    "TidyPackRat",
                    "No files to organize. Your folders are already tidy!",
                    ToolTipIcon.Info);
            }
            else
            {
                string sizeStr = Models.Statistics.FormatBytes(totalBytes);
                string fileWord = fileCount == 1 ? "file" : "files";
                ShowNotification(
                    "TidyPackRat - Files Organized!",
                    $"Moved {fileCount} {fileWord} ({sizeStr})",
                    ToolTipIcon.Info);

                if (_soundEnabled)
                {
                    PlayOrganizeSound();
                }
            }
        }

        /// <summary>
        /// Show notification for file watcher activity
        /// </summary>
        public static void ShowFileWatcherNotification(string fileName, string category)
        {
            ShowNotification(
                "TidyPackRat",
                $"Moved '{fileName}' to {category}",
                ToolTipIcon.Info,
                2000);

            if (_soundEnabled)
            {
                PlayOrganizeSound();
            }
        }

        /// <summary>
        /// Show error notification
        /// </summary>
        public static void ShowErrorNotification(string message)
        {
            ShowNotification(
                "TidyPackRat - Error",
                message,
                ToolTipIcon.Error);
        }

        /// <summary>
        /// Play the organize complete sound effect
        /// </summary>
        public static void PlayOrganizeSound(string customSoundPath = null)
        {
            if (!_soundEnabled)
                return;

            try
            {
                if (!string.IsNullOrEmpty(customSoundPath) && File.Exists(customSoundPath))
                {
                    using (var player = new SoundPlayer(customSoundPath))
                    {
                        player.Play();
                    }
                }
                else
                {
                    // Play Windows system sound as fallback
                    SystemSounds.Asterisk.Play();
                }
            }
            catch
            {
                // Sound playing is not critical, ignore errors
            }
        }

        /// <summary>
        /// Play a subtle "whoosh" effect using system sounds
        /// </summary>
        public static void PlayWhooshSound()
        {
            if (!_soundEnabled)
                return;

            try
            {
                // Use Navigation Start sound for a subtle whoosh effect
                SystemSounds.Exclamation.Play();
            }
            catch
            {
                // Sound playing is not critical
            }
        }
    }
}
