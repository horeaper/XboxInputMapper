#include "stdafx.h"
#include "InputMapperLibrary.h"

namespace XboxInputMapper
{
	InputMapperLibrary::InputMapperLibrary(bool isVisualizeTouch)
	{
		m_pimpl = new InputMapperLibraryPimpl();
		m_pimpl->DirectionTouchId = -1;

		SetTouchVisualize(isVisualizeTouch);
		ZeroMemory(m_pimpl->contact, sizeof(m_pimpl->contact));
		for (int cnt = 0; cnt < MaxTouchCount; ++cnt) {
			m_pimpl->contact[cnt].pointerInfo.pointerFlags = POINTER_FLAG_NONE;
			m_pimpl->contact[cnt].pointerInfo.pointerType = PT_TOUCH;
			m_pimpl->contact[cnt].pointerInfo.pointerId = cnt;
			m_pimpl->contact[cnt].touchMask = TOUCH_MASK_CONTACTAREA;
		}
	}

	InputMapperLibrary::~InputMapperLibrary()
	{
		this->!InputMapperLibrary();
	}

	InputMapperLibrary::!InputMapperLibrary()
	{
		delete m_pimpl;
	}

	void InputMapperLibrary::SetTouchVisualize(bool isVisualizeTouch)
	{
		InitializeTouchInjection(MaxTouchCount, isVisualizeTouch ? TOUCH_FEEDBACK_DEFAULT : TOUCH_FEEDBACK_NONE);
	}

	void InputMapperLibrary::DirectionDown(Point point)
	{
		m_pimpl->DirectionTouchId = FindAvailableIndex();
		PointerDown(m_pimpl->DirectionTouchId, (int)point.X, (int)point.Y);
	}

	void InputMapperLibrary::DirectionUpdate(Point point)
	{
		if (m_pimpl->DirectionTouchId != -1) {
			PointerUpdate(m_pimpl->DirectionTouchId, (int)point.X, (int)point.Y);
		}
	}

	void InputMapperLibrary::DirectionUp()
	{
		if (m_pimpl->DirectionTouchId != -1) {
			PointerUp(m_pimpl->DirectionTouchId);
			m_pimpl->DirectionTouchId = -1;
		}
	}

	void InputMapperLibrary::ButtonDown(int buttonId, List<Point> ^pointArray, Point pointOffset)
	{
		m_pimpl->ButtonTouchId[buttonId].resize(pointArray->Count);
		for (int cnt = 0; cnt < pointArray->Count; ++cnt) {
			m_pimpl->ButtonTouchId[buttonId][cnt] = FindAvailableIndex();
			PointerDown(m_pimpl->ButtonTouchId[buttonId][cnt], (int)(pointArray[cnt].X + pointOffset.X), (int)(pointArray[cnt].Y + pointOffset.Y));
		}
	}

	void InputMapperLibrary::ButtonUpdate(int buttonId, List<Point> ^pointArray, Point pointOffset)
	{
		for (int cnt = 0; cnt < pointArray->Count; ++cnt) {
			PointerUpdate(m_pimpl->ButtonTouchId[buttonId][cnt], (int)(pointArray[cnt].X + pointOffset.X), (int)(pointArray[cnt].Y + pointOffset.Y));
		}
	}

	void InputMapperLibrary::ButtonUp(int buttonId)
	{
		for (unsigned cnt = 0; cnt < m_pimpl->ButtonTouchId[buttonId].size(); ++cnt) {
			PointerUp(m_pimpl->ButtonTouchId[buttonId][cnt]);
		}
		m_pimpl->ButtonTouchId[buttonId].clear();
	}

	bool InputMapperLibrary::SendTouchData()
	{
		vector<POINTER_TOUCH_INFO> injectArray;
		for (int cnt = 0; cnt < MaxTouchCount; ++cnt) {
			if (m_pimpl->contact[cnt].pointerInfo.pointerFlags != POINTER_FLAG_NONE) {
				injectArray.push_back(m_pimpl->contact[cnt]);
			}
		}
		if (injectArray.size() > 0) {
			bool result = InjectTouchInput(injectArray.size(), &injectArray.front()) ? true : false;
			for (int cnt = 0; cnt < MaxTouchCount; ++cnt) {
				if (m_pimpl->contact[cnt].pointerInfo.pointerFlags == POINTER_FLAG_UP) {
					m_pimpl->contact[cnt].pointerInfo.pointerFlags = POINTER_FLAG_NONE;
				}
			}
			return result;
		}
		else {
			return true;
		}
	}
	
	//========================================================================
	// private:
	//========================================================================

	int InputMapperLibrary::FindAvailableIndex()
	{
		for (int cnt = 0; cnt < MaxTouchCount; ++cnt) {
			if (m_pimpl->contact[cnt].pointerInfo.pointerFlags == POINTER_FLAG_NONE) {
				return cnt;
			}
		}
		throw gcnew System::IndexOutOfRangeException(L"Cannot find available index");
	}

	void InputMapperLibrary::PointerDown(int index, int x, int y)
	{
		m_pimpl->contact[index].pointerInfo.pointerFlags = POINTER_FLAG_DOWN | POINTER_FLAG_INRANGE | POINTER_FLAG_INCONTACT;
		m_pimpl->contact[index].pointerInfo.ptPixelLocation.x = x;
		m_pimpl->contact[index].pointerInfo.ptPixelLocation.y = y;
		m_pimpl->contact[index].rcContact.left = x - ContactAreaRadius;
		m_pimpl->contact[index].rcContact.top = y - ContactAreaRadius;
		m_pimpl->contact[index].rcContact.right = x + ContactAreaRadius;
		m_pimpl->contact[index].rcContact.bottom = y + ContactAreaRadius;
	}

	void InputMapperLibrary::PointerUpdate(int index, int x, int y)
	{
		m_pimpl->contact[index].pointerInfo.pointerFlags = POINTER_FLAG_UPDATE | POINTER_FLAG_INRANGE | POINTER_FLAG_INCONTACT;
		m_pimpl->contact[index].pointerInfo.ptPixelLocation.x = x;
		m_pimpl->contact[index].pointerInfo.ptPixelLocation.y = y;
		m_pimpl->contact[index].rcContact.left = x - ContactAreaRadius;
		m_pimpl->contact[index].rcContact.top = y - ContactAreaRadius;
		m_pimpl->contact[index].rcContact.right = x + ContactAreaRadius;
		m_pimpl->contact[index].rcContact.bottom = y + ContactAreaRadius;
	}

	void InputMapperLibrary::PointerUp(int index)
	{
		m_pimpl->contact[index].pointerInfo.pointerFlags = POINTER_FLAG_UP;
	}
}
