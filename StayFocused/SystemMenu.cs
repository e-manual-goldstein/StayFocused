using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace StayFocused
{
    public class SystemMenu
    {
        private NotifyIcon notifyIcon;
        private ContextMenuStrip contextMenuStrip;
        public Action Shutdown;

        private Icon GetIcon()
        {
            using (FileStream fileStream = new FileStream("icon.ico", FileMode.Open, FileAccess.Read))
            {
                // Create the Icon from the FileStream
                return new Icon(fileStream);
            }
        }

        public void Initialise(System.Windows.Application application)
        {
            // Create the NotifyIcon instance
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = GetIcon(); // You can set your custom icon here
            notifyIcon.Text = "StayFocused";
            notifyIcon.Visible = true;

            // Set up a context menu for the NotifyIcon (using ContextMenuStrip)
            contextMenuStrip = CreateMenu();

            // Add a click event handler for the NotifyIcon (optional)
            notifyIcon.Click += OnNotifyIconClick;

            // Add a double-click event handler for the NotifyIcon (optional)
            notifyIcon.DoubleClick += OnNotifyIconDoubleClick;

            Shutdown += application.Shutdown;
        }

        private ContextMenuStrip CreateMenu()
        {
            var menuStrip = new ContextMenuStrip();
            AddExitAction(menuStrip);
            notifyIcon.ContextMenuStrip = menuStrip;
            return menuStrip;
        }

        private void AddExitAction(ContextMenuStrip contextMenuStrip)
        {
            contextMenuStrip.Items.Add("Exit", null, OnExit);
        }

        private void AddDailySummaryAction(ContextMenuStrip contextMenuStrip)
        {
            contextMenuStrip.Items.Add("Daily Summary", null, ShowDailySummary);
        }

        private void ShowDailySummary(object sender, EventArgs e)
        {
            
        }

        private void OnNotifyIconClick(object sender, EventArgs e)
        {
            // Handle click event here (e.g., show a tooltip or perform an action)
            //notifyIcon.ShowBalloonTip(1000, "StayFocused", "Application is running!", ToolTipIcon.Info);
        }

        private void OnNotifyIconDoubleClick(object sender, EventArgs e)
        {
            // Handle double-click event here (e.g., open a window or perform an action)
            // In this example, we'll exit the application on double-click
            Shutdown();
        }

        public void OnExit(object sender, EventArgs e)
        {
            // Clean up resources and exit the application when "Exit" is clicked from the context menu
            notifyIcon.Visible = false;
            notifyIcon.Dispose();

        }
    }
}
