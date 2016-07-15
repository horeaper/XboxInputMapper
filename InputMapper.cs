using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
		}

		public void TouchUpdate(int index, Point point)
		{
			m_contact[index].PointerInfo.PointerFlags = PointerFlags.InRange | PointerFlags.InContact | PointerFlags.Update;
			m_contact[index].PointerInfo.PixelLocation.X = (int)point.X;
			m_contact[index].PointerInfo.PixelLocation.Y = (int)point.Y;
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
				TouchInjection.InjectTouchInput(m_injectArray.Count, m_injectArray.ToArray());

				for (int cnt = 0; cnt < m_contact.Length; ++cnt) {
					if (m_contact[cnt].PointerInfo.PointerFlags == PointerFlags.Up) {
						m_contact[cnt].PointerInfo.PointerFlags = PointerFlags.None;
					}
				}
			}
		}
	}
}
