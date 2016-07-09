#include "stdafx.h"
#include "InputMapperLibrary.h"

namespace XboxInputMapper
{
	InputMapperLibrary::InputMapperLibrary(bool isVisualizeTouch)
	{
		m_contact = new POINTER_TOUCH_INFO[MaxTouchCount];
		m_injectArray = new vector<POINTER_TOUCH_INFO>();
		SetTouchVisualize(isVisualizeTouch);
	}

	void InputMapperLibrary::SetTouchVisualize(bool isVisualizeTouch)
	{
		InitializeTouchInjection(MaxTouchCount, isVisualizeTouch ? TOUCH_FEEDBACK_DEFAULT : TOUCH_FEEDBACK_NONE);

		ZeroMemory(m_contact, sizeof(POINTER_TOUCH_INFO) * MaxTouchCount);
		for (int cnt = 0; cnt < MaxTouchCount; ++cnt) {
			m_contact[cnt].pointerInfo.pointerFlags = POINTER_FLAG_NONE;
			m_contact[cnt].pointerInfo.pointerType = PT_TOUCH;
			m_contact[cnt].pointerInfo.pointerId = cnt;
		}
	}

	void InputMapperLibrary::TouchDown(int index, Point point)
	{
		m_contact[index].pointerInfo.pointerFlags = POINTER_FLAG_INRANGE | POINTER_FLAG_INCONTACT | POINTER_FLAG_DOWN;
		m_contact[index].pointerInfo.ptPixelLocation.x = (int)point.X;
		m_contact[index].pointerInfo.ptPixelLocation.y = (int)point.Y;
	}

	void InputMapperLibrary::TouchUpdate(int index, Point point)
	{
		m_contact[index].pointerInfo.pointerFlags = POINTER_FLAG_INRANGE | POINTER_FLAG_INCONTACT | POINTER_FLAG_UPDATE;
		m_contact[index].pointerInfo.ptPixelLocation.x = (int)point.X;
		m_contact[index].pointerInfo.ptPixelLocation.y = (int)point.Y;
	}

	void InputMapperLibrary::TouchUp(int index)
	{
		m_contact[index].pointerInfo.pointerFlags = POINTER_FLAG_INRANGE | POINTER_FLAG_UP;
	}

	void InputMapperLibrary::SendTouchData()
	{
		m_injectArray->clear();
		for (int cnt = 0; cnt < MaxTouchCount; ++cnt) {
			if (m_contact[cnt].pointerInfo.pointerFlags != POINTER_FLAG_NONE) {
				m_injectArray->push_back(m_contact[cnt]);
			}
		}
		if (m_injectArray->size() > 0) {
			InjectTouchInput(m_injectArray->size(), &m_injectArray->front());

			for (int cnt = 0; cnt < MaxTouchCount; ++cnt) {
				if (m_contact[cnt].pointerInfo.pointerFlags == (POINTER_FLAG_INRANGE | POINTER_FLAG_UP)) {
					m_contact[cnt].pointerInfo.pointerFlags = POINTER_FLAG_INRANGE | POINTER_FLAG_UPDATE;
				}
			}
		}
	}
}
