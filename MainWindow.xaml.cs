using System;
using System.Collections.Generic;
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

		InputMapper m_inputMapper = new InputMapper();
		IntPtr m_gameWindow = IntPtr.Zero;
		DispatcherTimer m_timer = new DispatcherTimer();

		const int ThumbDeadzone = short.MaxValue / 4;
		const int TriggerDeadzone = byte.MaxValue / 2;
		XInput.Gamepad m_previousGamepad;
		bool m_isDirectionInEffect;
		bool m_isLeftTriggerDown;
		bool m_isRightTriggerDown;
		int m_rightTriggerFrame;
		bool m_isShadowAxis;

		Dictionary<Point, int> m_posMap = new Dictionary<Point, int>();

		public MainWindow()
		{
			InitializeComponent();

			Settings = ProgramSettings.Load();
			m_inputMapper.SetTouchVisualize(Settings.IsVisualizeTouch);
			textAppTitle.Text = Settings.ApplicationTitle;
			checkTouchVisible.IsChecked = Settings.IsVisualizeTouch;
			checkTriggerHappy.IsChecked = Settings.IsTriggerHappy;
			RefreshPositionIndex();
			InitializeNotifyIcon();
		}

		void RefreshPositionIndex()
		{
			m_posMap.Clear();
			int index = 0;
			foreach (var positions in Settings.ButtonPositions) {
				foreach (var point in positions) {
					m_posMap.Add(point, index++);
				}
			}
			foreach (var point in Settings.LeftTriggerPositions) {
				m_posMap.Add(point, index++);
			}
			foreach (var point in Settings.RightTriggerPositions) {
				m_posMap.Add(point, index++);
			}
		}

		void Reset()
		{
			m_previousGamepad = new XInput.Gamepad();
			m_isDirectionInEffect = false;
			m_isLeftTriggerDown = false;
			m_isRightTriggerDown = false;
			m_inputMapper.SetTouchVisualize(Settings.IsVisualizeTouch);
		}

		private void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			m_timer.Tick += timer_Tick;
			m_timer.Interval = TimeSpan.FromSeconds(1.0 / 60.0);
			m_timer.Start();

			WindowState = Settings.IsMinimized ? WindowState.Minimized : WindowState.Normal;
			MainWindow_StateChanged(null, null);
		}

		private void MainWindow_Closing(object sender, CancelEventArgs e)
		{
			Settings.IsMinimized = WindowState == WindowState.Minimized;

		tagRetry:
			if (!Settings.Save()) {
				var result = MessageBox.Show(this, "Cannot save program settings. Retry?", "Xbox Input Mapper", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
				if (result == MessageBoxResult.Yes) {
					goto tagRetry;
				}
			}

			m_timer.Stop();
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
			Reset();
		}

		private void CheckTriggerHappy_CheckedChanged(object sender, RoutedEventArgs e)
		{
			Settings.IsTriggerHappy = checkTriggerHappy.IsChecked == true;
			Reset();
		}

		private void CheckReverseAxis_CheckedChanged(object sender, RoutedEventArgs e)
		{
			Settings.IsReverseAxis = checkReverseAxis.IsChecked == true;
		}

		private void ProfileButton_Click(object sender, RoutedEventArgs e)
		{
			var profileWindow = new TouchProfileWindow();
			profileWindow.Owner = this;
			profileWindow.ShowDialog();
			RefreshPositionIndex();
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

			XInput.State state;
			if (XInput.GetState(0, out state) == XInput.ErrorSuccess) {
				Imports.RECT screenRect;
				Imports.GetWindowRect(m_gameWindow, out screenRect);
				var windowOffset = new Vector(screenRect.Left, screenRect.Top);

				//Axis
				if (Settings.AxisCenter.HasValue && Settings.AxisRadius > 0) {
					var direction = new Vector(state.Gamepad.ThumbLX, state.Gamepad.ThumbLY);
					if (Math.Abs(direction.X) <= ThumbDeadzone) {
						direction.X = 0;
					}
					if (Math.Abs(direction.Y) <= ThumbDeadzone) {
						direction.Y = 0;
					}
					if (direction.X == 0 && direction.Y == 0) {    //No direction
						if (m_isDirectionInEffect) {
							m_inputMapper.TouchUp(InputMapper.MaxTouchCount - 1);
							m_isDirectionInEffect = false;
						}
					}
					else {
						direction.Normalize();
						direction *= Settings.AxisRadius;

						//Reverse axis
						if (Settings.IsReverseAxis) {
							direction.X = -direction.X;
							direction.Y = -direction.Y;
						}

						//Shadow axis
						if (direction.X > 0) {
							m_isShadowAxis = false;
						}
						else if (direction.X < 0) {
							m_isShadowAxis = true;
						}
						var axisCenter = Settings.AxisCenter.Value;
						if (m_isShadowAxis) {
							axisCenter.X += Settings.ShadowAxisOffset;
						}

						//Output
						var point = new Point(windowOffset.X + axisCenter.X + direction.X, windowOffset.Y + axisCenter.Y - direction.Y);
						if (!m_isDirectionInEffect) {
							m_inputMapper.TouchDown(InputMapper.MaxTouchCount - 1, point);
							m_isDirectionInEffect = true;
						}
						else {
							m_inputMapper.TouchUpdate(InputMapper.MaxTouchCount - 1, point);
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
							foreach (var point in Settings.ButtonPositions[buttonId]) {
								m_inputMapper.TouchUp(m_posMap[point]);
							}
						}
					}
					else {
						if (!isButtonInEffect) {
							foreach (var point in Settings.ButtonPositions[buttonId]) {
								m_inputMapper.TouchDown(m_posMap[point], point + windowOffset);
							}
						}
						else {
							foreach (var point in Settings.ButtonPositions[buttonId]) {
								m_inputMapper.TouchUpdate(m_posMap[point], point + windowOffset);
							}
						}
					}
				}

				if (Settings.IsTriggerHappy) {
					//Left trigger
					bool isLeftTriggerInEffect = m_previousGamepad.LeftTrigger > TriggerDeadzone;
					if (gamepad.LeftTrigger <= TriggerDeadzone) {   //No trigger
						if (isLeftTriggerInEffect) {
							if (m_isLeftTriggerDown) {
								foreach (var point in Settings.LeftTriggerPositions) {
									m_inputMapper.TouchUp(m_posMap[point]);
								}
							}
							m_isLeftTriggerDown = false;
						}
					}
					else {
						if (!isLeftTriggerInEffect) {
							foreach (var point in Settings.LeftTriggerPositions) {
								m_inputMapper.TouchDown(m_posMap[point], point + windowOffset);
							}
							m_isLeftTriggerDown = true;
						}
						else {
							if (m_isLeftTriggerDown) {
								foreach (var point in Settings.LeftTriggerPositions) {
									m_inputMapper.TouchUp(m_posMap[point]);
								}
							}
							else {
								foreach (var point in Settings.LeftTriggerPositions) {
									m_inputMapper.TouchDown(m_posMap[point], point + windowOffset);
								}
							}
							m_isLeftTriggerDown = !m_isLeftTriggerDown;
						}
					}

					//Right trigger
					bool isRightTriggerInEffect = m_previousGamepad.RightTrigger > TriggerDeadzone;
					if (gamepad.RightTrigger <= TriggerDeadzone) {   //No trigger
						if (isRightTriggerInEffect) {
							if (m_isRightTriggerDown) {
								foreach (var point in Settings.RightTriggerPositions) {
									m_inputMapper.TouchUp(m_posMap[point]);
								}
							}
							m_isRightTriggerDown = false;
						}
					}
					else {
						if (!isRightTriggerInEffect) {
							foreach (var point in Settings.RightTriggerPositions) {
								m_inputMapper.TouchDown(m_posMap[point], point + windowOffset);
							}
							m_isRightTriggerDown = true;
						}
						else {
							if (m_isRightTriggerDown) {
								foreach (var point in Settings.RightTriggerPositions) {
									m_inputMapper.TouchUp(m_posMap[point]);
								}
							}
							else {
								foreach (var point in Settings.RightTriggerPositions) {
									m_inputMapper.TouchDown(m_posMap[point], point + windowOffset);
								}
							}
							m_isRightTriggerDown = !m_isRightTriggerDown;
						}
					}
				}
				else {
/*
					//Left trigger
					bool isLeftTriggerInEffect = m_previousGamepad.LeftTrigger > TriggerDeadzone;
					if (gamepad.LeftTrigger <= TriggerDeadzone) {   //No trigger
						if (isLeftTriggerInEffect) {
							foreach (var point in Settings.LeftTriggerPositions) {
								m_inputMapper.TouchUp(m_posMap[point]);
							}
						}
					}
					else {
						if (!isLeftTriggerInEffect) {
							foreach (var point in Settings.LeftTriggerPositions) {
								m_inputMapper.TouchDown(m_posMap[point], point + windowOffset);
							}
						}
						else {
							foreach (var point in Settings.LeftTriggerPositions) {
								m_inputMapper.TouchUpdate(m_posMap[point], point + windowOffset);
							}
						}
					}

					//Right trigger
					bool isRightTriggerInEffect = m_previousGamepad.RightTrigger > TriggerDeadzone;
					if (gamepad.RightTrigger <= TriggerDeadzone) {   //No trigger
						if (isRightTriggerInEffect) {
							foreach (var point in Settings.RightTriggerPositions) {
								m_inputMapper.TouchUp(m_posMap[point]);
							}
						}
					}
					else {
						if (!isRightTriggerInEffect) {
							foreach (var point in Settings.RightTriggerPositions) {
								m_inputMapper.TouchDown(m_posMap[point], point + windowOffset);
							}
						}
						else {
							foreach (var point in Settings.RightTriggerPositions) {
								m_inputMapper.TouchUpdate(m_posMap[point], point + windowOffset);
							}
						}
					}
*/
					//Right trigger
					bool isRightTriggerInEffect = m_previousGamepad.RightTrigger > TriggerDeadzone;
					if (gamepad.RightTrigger <= TriggerDeadzone) {   //No trigger
						if (isRightTriggerInEffect && m_rightTriggerFrame > 0) {
							var actionType = TriggerAction.TriangleTripletFrenzy(m_rightTriggerFrame - 1);
							if (TriggerAction.IsButtonDown(actionType)) {
								int buttonId = TriggerAction.GetButtonIndex(actionType);
								foreach (var point in Settings.ButtonPositions[buttonId]) {
									m_inputMapper.TouchUp(m_posMap[point]);
								}
							}
							m_rightTriggerFrame = 0;
						}
					}
					else {
						var actionType = TriggerAction.TriangleTripletFrenzy(m_rightTriggerFrame);
						if (TriggerAction.IsButtonDown(actionType)) {
							int buttonId = TriggerAction.GetButtonIndex(actionType);
							foreach (var point in Settings.ButtonPositions[buttonId]) {
								m_inputMapper.TouchDown(m_posMap[point], point + windowOffset);
							}
						}
						else if (TriggerAction.IsButtonUpdate(actionType)) {
							int buttonId = TriggerAction.GetButtonIndex(actionType);
							foreach (var point in Settings.ButtonPositions[buttonId]) {
								m_inputMapper.TouchUpdate(m_posMap[point], point + windowOffset);
							}
						}
						else if (TriggerAction.IsButtonUp(actionType)){
							int buttonId = TriggerAction.GetButtonIndex(actionType);
							foreach (var point in Settings.ButtonPositions[buttonId]) {
								m_inputMapper.TouchUp(m_posMap[point]);
							}
						}
						++m_rightTriggerFrame;
					}
				}

				m_previousGamepad = gamepad;
			}
			else {
				m_previousGamepad = new XInput.Gamepad();
				m_isDirectionInEffect = false;
				m_isLeftTriggerDown = false;
				m_isRightTriggerDown = false;
			}

			m_inputMapper.SendTouchData();
		}
	}
}
