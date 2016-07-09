using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using XboxInputMapper.Native;

namespace XboxInputMapper
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		internal static ProgramSettings Settings { get; private set; }

		InputMapperLibrary m_inputLibrary;
		IntPtr m_gameWindow = IntPtr.Zero;
		DispatcherTimer m_timer = new DispatcherTimer();

		System.Windows.Forms.NotifyIcon m_notifyIcon;

		const int ThumbDeadzone = short.MaxValue / 2;
		const int TriggerDeadzone = byte.MaxValue / 4;
		bool m_isDirectionInEffect;
		XInput.Gamepad m_previousGamepad;

		public MainWindow()
		{
			InitializeComponent();

			Settings = ProgramSettings.Load();
			m_inputLibrary = new InputMapperLibrary(Settings.IsVisualizeTouch);
			textAppTitle.Text = Settings.ApplicationTitle;
			checkTouchVisible.IsChecked = Settings.IsVisualizeTouch;

			m_notifyIcon = new System.Windows.Forms.NotifyIcon();
			m_notifyIcon.Icon = Properties.Resources.Program;
			m_notifyIcon.Visible = false;
			m_notifyIcon.Text = "Xbox Input Mapper";
			m_notifyIcon.Click += (sender, e) => {
				Show();
				WindowState = WindowState.Normal;
				m_notifyIcon.Visible = false;
			};
		}

		private void MainWindow_Initialized(object sender, EventArgs e)
		{
			m_timer.Tick += timer_Tick;
			m_timer.Interval = TimeSpan.FromSeconds(1.0 / 60.0);
			m_timer.Start();
		}

		private void MainWindow_Closing(object sender, CancelEventArgs e)
		{
		tagRetry:
			if (!Settings.Save()) {
				var result = MessageBox.Show(this, "Cannot save program settings. Retry?", "Xbox Input Mapper", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
				if (result == MessageBoxResult.Yes) {
					goto tagRetry;
				}
			}

			m_timer.Stop();
			m_inputLibrary.Dispose();
		}

		private void MainWindow_StateChanged(object sender, EventArgs e)
		{
			if (WindowState == WindowState.Minimized) {
				Hide();
				m_notifyIcon.Visible = true;
			}
		}

		private void TextAppTitle_TextChanged(object sender, TextChangedEventArgs e)
		{
			Settings.ApplicationTitle = textAppTitle.Text;
			m_gameWindow = IntPtr.Zero;
			textAppTitle.Background = Brushes.Red;
		}

		private void CheckTouchVisible_CheckedChanged(object sender, RoutedEventArgs e)
		{
			Settings.IsVisualizeTouch = checkTouchVisible.IsChecked == true;
			m_inputLibrary.SetTouchVisualize(Settings.IsVisualizeTouch);
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			var profileWindow = new TouchProfileWindow();
			profileWindow.Owner = this;
			profileWindow.ShowDialog();
		}

		private void timer_Tick(object sender, EventArgs e)
		{
			//Validate game window
			if (m_gameWindow != IntPtr.Zero && !Imports.IsWindow(m_gameWindow)) {
				m_gameWindow = IntPtr.Zero;
				Dispatcher.Invoke(() => textAppTitle.Background = Brushes.Red);
			}
			if (m_gameWindow == IntPtr.Zero) {
				var gameWindow = Imports.FindWindow(null, Settings.ApplicationTitle);
				if (gameWindow == IntPtr.Zero) {
					return;
				}
				m_gameWindow = gameWindow;
				Dispatcher.Invoke(() => {
					textAppTitle.Background = Brushes.LightGreen;
				});
			}

			//Validate controller
			XInput.State state;
			if (XInput.GetState(0, out state) == XInput.ErrorSuccess) {
				Imports.RECT screenRect;
				Imports.GetWindowRect(m_gameWindow, out screenRect);
				var windowPosition = new Point(screenRect.Left, screenRect.Top);

				//Axis
				if (Settings.AxisCenter.HasValue && Settings.AxisRadius > 0) {
					var center = Settings.AxisCenter.Value;

					var direction = new Vector(state.Gamepad.ThumbLX, state.Gamepad.ThumbLY);
					if (Math.Abs(direction.X) <= ThumbDeadzone) {
						direction.X = 0;
					}
					if (Math.Abs(direction.Y) <= ThumbDeadzone) {
						direction.Y = 0;
					}
					if (direction.X == 0 && direction.Y == 0) {	//No direction
						if (m_isDirectionInEffect) {
							m_inputLibrary.DirectionUp();
							m_isDirectionInEffect = false;
						}
					}
					else {
						direction.Normalize();
						direction *= Settings.AxisRadius;

						var point = new Point(windowPosition.X + center.X + direction.X, windowPosition.Y + center.Y - direction.Y);
						if (!m_isDirectionInEffect) {
							m_inputLibrary.DirectionDown(point);
							m_isDirectionInEffect = true;
						}
						else {
							m_inputLibrary.DirectionUpdate(point);
						}
					}
				}

				var gamepad = state.Gamepad;

				//Button
				for (int buttonId = 0; buttonId < Constants.ButtonValue.Length; ++buttonId) {
					var value = Constants.ButtonValue[buttonId];
					bool isButtonInEffect = m_previousGamepad.Buttons.HasFlag(value);
					if (!gamepad.Buttons.HasFlag(value)) {	//No button
						if (isButtonInEffect) {
							m_inputLibrary.ButtonUp(buttonId);
						}
					}
					else {
						if (!isButtonInEffect) {
							m_inputLibrary.ButtonDown(buttonId, Settings.ButtonPositions[buttonId], windowPosition);
						}
						else {
							m_inputLibrary.ButtonUpdate(buttonId, Settings.ButtonPositions[buttonId], windowPosition);
						}
					}
				}

				//Trigger
				bool isLeftTriggerInEffect = m_previousGamepad.LeftTrigger > TriggerDeadzone;
				if (gamepad.LeftTrigger <= TriggerDeadzone) {	//No trigger
					if (isLeftTriggerInEffect) {
						m_inputLibrary.ButtonUp(InputMapperLibrary.LeftTriggerButtonId);
					}
				}
				else {
					if (!isLeftTriggerInEffect) {
						m_inputLibrary.ButtonDown(InputMapperLibrary.LeftTriggerButtonId, Settings.LeftTriggerPositions, windowPosition);
					}
					else {
						m_inputLibrary.ButtonUpdate(InputMapperLibrary.LeftTriggerButtonId, Settings.LeftTriggerPositions, windowPosition);
					}
				}
				bool isRightTriggerInEffect = m_previousGamepad.RightTrigger > TriggerDeadzone;
				if (gamepad.RightTrigger <= TriggerDeadzone) {   //No trigger
					if (isRightTriggerInEffect) {
						m_inputLibrary.ButtonUp(InputMapperLibrary.RightTriggerButtonId);
					}
				}
				else {
					if (!isRightTriggerInEffect) {
						m_inputLibrary.ButtonDown(InputMapperLibrary.RightTriggerButtonId, Settings.RightTriggerPositions, windowPosition);
					}
					else {
						m_inputLibrary.ButtonUpdate(InputMapperLibrary.RightTriggerButtonId, Settings.RightTriggerPositions, windowPosition);
					}
				}

				m_previousGamepad = gamepad;
			}
			else {
				m_previousGamepad = new XInput.Gamepad();
			}

			m_inputLibrary.SendTouchData();
		}
	}
}
