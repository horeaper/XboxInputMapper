using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using XboxInputMapper.Native;

namespace XboxInputMapper
{
	class InputMapper
	{
		public const int MaxTouchCount = 32;

		PointerTouchInfo[] m_contact = new PointerTouchInfo[MaxTouchCount];
		List<PointerTouchInfo> m_injectArray = new List<PointerTouchInfo>();
		bool m_isVisualizeTouch;

		public InputMapper()
		{
			for (int cnt = 0; cnt < m_contact.Length; ++cnt) {
				m_contact[cnt].PointerInfo.PointerType = PointerInputType.Touch;
				m_contact[cnt].PointerInfo.PointerId = (uint)cnt;
				m_contact[cnt].TouchFlags = TouchFlags.None;
				m_contact[cnt].TouchMasks = TouchMask.ContactArea | TouchMask.Orientation | TouchMask.Pressure;
				m_contact[cnt].Orientation = 90;
				m_contact[cnt].Pressure = 1024;
			}
		}

		public void SetTouchVisualize(bool isVisualizeTouch)
		{
			m_isVisualizeTouch = isVisualizeTouch;
			TouchInjection.InitializeTouchInjection(MaxTouchCount, isVisualizeTouch ? TouchFeedback.Default : TouchFeedback.None);
		}

		public void TouchDown(int index, Point point)
		{
			m_contact[index].PointerInfo.PointerFlags = PointerFlags.InRange | PointerFlags.InContact | PointerFlags.Down;
			m_contact[index].PointerInfo.PixelLocation.X = (int)point.X;
			m_contact[index].PointerInfo.PixelLocation.Y = (int)point.Y;
			m_contact[index].ContactArea.Left = (int)point.X - 2;
			m_contact[index].ContactArea.Right = (int)point.X + 2;
			m_contact[index].ContactArea.Top = (int)point.Y - 2;
			m_contact[index].ContactArea.Bottom = (int)point.Y + 2;
		}

		public void TouchUpdate(int index, Point point)
		{
			m_contact[index].PointerInfo.PointerFlags = PointerFlags.InRange | PointerFlags.InContact | PointerFlags.Update;
			m_contact[index].PointerInfo.PixelLocation.X = (int)point.X;
			m_contact[index].PointerInfo.PixelLocation.Y = (int)point.Y;
			m_contact[index].ContactArea.Left = (int)point.X - 2;
			m_contact[index].ContactArea.Right = (int)point.X + 2;
			m_contact[index].ContactArea.Top = (int)point.Y - 2;
			m_contact[index].ContactArea.Bottom = (int)point.Y + 2;
		}

		public void TouchUp(int index)
		{
			m_contact[index].PointerInfo.PointerFlags = PointerFlags.Up;
		}

		public void SendTouchData()
		{
			m_injectArray.Clear();
			for (int cnt = 0; cnt < m_contact.Length; ++cnt) {
				if (m_contact[cnt].PointerInfo.PointerFlags != PointerFlags.None) {
					m_injectArray.Add(m_contact[cnt]);
				}
			}

			if (m_injectArray.Count > 0) {
#if DEBUG
				bool result = TouchInjection.InjectTouchInput(m_injectArray.Count, m_injectArray.ToArray());
				if (!result) {
					int errorCode = Marshal.GetLastWin32Error();
				}
#else
				TouchInjection.InjectTouchInput(m_injectArray.Count, m_injectArray.ToArray());
#endif

				for (int cnt = 0; cnt < m_contact.Length; ++cnt) {
					if (m_contact[cnt].PointerInfo.PointerFlags == PointerFlags.Up) {
						m_contact[cnt].PointerInfo.PointerFlags = PointerFlags.None;
					}
				}
			}
		}
	}
}
