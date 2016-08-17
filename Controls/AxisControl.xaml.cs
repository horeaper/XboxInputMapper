using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace XboxInputMapper.Controls
{
	public partial class AxisControl : UserControl, IDisplayControl
	{
		public static readonly SolidColorBrush UnselectedBrush = new SolidColorBrush(Color.FromArgb(128, 0, 255, 255));
		public static readonly SolidColorBrush SelectedBrush = new SolidColorBrush(Color.FromArgb(192, 0, 192, 192));

		public AxisControl()
		{
			InitializeComponent();
			shapeBackground.Fill = UnselectedBrush;
			textAxisRadius.Text = MainWindow.Settings.AxisRadius.ToString();
			textAxisReverseOffset.Text = MainWindow.Settings.AxisReverseOffset.ToString();
		}

		private void AxisControl_OnLoaded(object sender, RoutedEventArgs e)
		{
			if (MainWindow.Settings.AxisReverseOffset != 0) {
				menuShowReverseAxis.IsChecked = true;
				shapeReverseAxisIn.Visibility = Visibility.Visible;
				shapeReverseAxisOut.Visibility = Visibility.Visible;
			}
		}

		public bool IsSelected
		{
			get { return ReferenceEquals(shapeBackground.Fill, SelectedBrush); }
			set { shapeBackground.Fill = value ? SelectedBrush : UnselectedBrush; }
		}

		public Point Location
		{
			get { return new Point(Canvas.GetLeft(this), Canvas.GetTop(this)); }
			set { Canvas.SetLeft(this, value.X); Canvas.SetTop(this, value.Y); }
		}

		public int AxisRadius
		{
			get { return -(int)matrixTranslate.X; }
			set
			{
				Width = value * 2;
				Height = value * 2;
				matrixTranslate.X = -value;
				matrixTranslate.Y = -value;
			}
		}

		public int AxisReverseOffset
		{
			get { return (int)shapeReverseAxisIn.Margin.Left; }
			set
			{
				shapeReverseAxisIn.Margin = new Thickness(value, 0, -value, 0);
				shapeReverseAxisOut.Margin = new Thickness(value, 0, -value, 0);
			}
		}

		public event EventHandler RemoveClicked;

		private void MenuRemove_Click(object sender, RoutedEventArgs e)
		{
			RemoveClicked?.Invoke(this, e);
		}

		private void TextAxisRadius_TextChanged(object sender, TextChangedEventArgs e)
		{
			ushort value;
			if (ushort.TryParse(textAxisRadius.Text, out value) && value >= 10) {
				AxisRadius = value;
			}
		}

		private void TextAxisReverseOffset_TextChanged(object sender, TextChangedEventArgs e)
		{
			short value;
			if (short.TryParse(textAxisReverseOffset.Text, out value)) {
				AxisReverseOffset = value;
			}
		}

		private void MenuShowReverseAxis_Click(object sender, RoutedEventArgs e)
		{
			menuShowReverseAxis.IsChecked = !menuShowReverseAxis.IsChecked;
			shapeReverseAxisIn.Visibility = menuShowReverseAxis.IsChecked ? Visibility.Visible : Visibility.Collapsed;
			shapeReverseAxisOut.Visibility = menuShowReverseAxis.IsChecked ? Visibility.Visible : Visibility.Collapsed;
		}
	}
}
