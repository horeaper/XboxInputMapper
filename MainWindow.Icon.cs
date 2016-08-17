using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace XboxInputMapper
{
	partial class MainWindow
	{
		NotifyIcon m_notifyIcon;
		ContextMenu m_menu;
		MenuItem m_menuVisualizeTouch;
		MenuItem m_menuTriggerHappy;
		MenuItem m_menuReverseAxis;

		void InitializeNotifyIcon()
		{
			m_menuVisualizeTouch = new MenuItem("Visualize Touch");
			m_menuVisualizeTouch.Click += MenuVisualizeTouch_Click;
			m_menuTriggerHappy = new MenuItem("Trigger Happy");
			m_menuTriggerHappy.Click += MenuTriggerHappy_Click;
			m_menuReverseAxis = new MenuItem("Reverse Axis");
			m_menuReverseAxis.Click += MenuReverseAxis_Click;
			var menuExit = new MenuItem("Exit");
			menuExit.Click += MenuExit_Click;

			m_menu = new ContextMenu();
			m_menu.MenuItems.AddRange(new[] { m_menuVisualizeTouch, m_menuTriggerHappy, m_menuReverseAxis, new MenuItem("-"), menuExit });
			m_menu.Popup += ContextMenu_Popup;

			m_notifyIcon = new NotifyIcon();
			m_notifyIcon.Icon = Properties.Resources.Program;
			m_notifyIcon.Visible = false;
			m_notifyIcon.Text = "Xbox Input Mapper";
			m_notifyIcon.ContextMenu = m_menu;
			m_notifyIcon.MouseClick += NotifyIcon_MouseClick;
		}

		private void ContextMenu_Popup(object sender, EventArgs e)
		{
			m_menuVisualizeTouch.Checked = Settings.IsVisualizeTouch;
			m_menuTriggerHappy.Checked = Settings.IsTriggerHappy;
			m_menuReverseAxis.Checked = Settings.IsReverseAxis;
		}

		private void MenuVisualizeTouch_Click(object sender, EventArgs e)
		{
			checkTouchVisible.IsChecked = !Settings.IsVisualizeTouch;
		}

		private void MenuTriggerHappy_Click(object sender, EventArgs e)
		{
			checkTriggerHappy.IsChecked = !Settings.IsTriggerHappy;
		}

		private void MenuReverseAxis_Click(object sender, EventArgs e)
		{
			checkReverseAxis.IsChecked = !Settings.IsReverseAxis;
		}

		private void MenuExit_Click(object sender, EventArgs e)
		{
			m_notifyIcon.Visible = false;
			Close();
		}

		private void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left) {
				Show();
				WindowState = WindowState.Normal;
				m_notifyIcon.Visible = false;
			}
		}
	}
}
