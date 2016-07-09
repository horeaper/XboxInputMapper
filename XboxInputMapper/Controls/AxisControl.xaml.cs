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

		public AxisControl(int radius)
		{
			InitializeComponent();
			shapeBackground.Fill = UnselectedBrush;
			textAxisRadius.Text = radius.ToString();
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

		public event EventHandler RemoveClicked;

		private void MenuRemove_Click(object sender, RoutedEventArgs e)
		{
			RemoveClicked?.Invoke(this, e);
		}

		private void TextAxisRadius_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			ushort value;
			if (!ushort.TryParse(e.Text, out value)) {
				e.Handled = true;
			}
		}

		void TextAxisRadius_TextChanged(object sender, TextChangedEventArgs e)
		{
			ushort value;
			if (ushort.TryParse(textAxisRadius.Text, out value) && value >= 10) {
				AxisRadius = value;
			}
			else {
				textAxisRadius.Text = "10";
			}
		}
	}
}
