#pragma once

namespace XboxInputMapper
{
	using namespace System;
	using namespace System::Collections::Generic;
	using namespace System::Windows;

	public ref class InputMapperLibrary
	{
	public:
		InputMapperLibrary(bool isVisualizeTouch);

		literal int MaxTouchCount = 30;

		void SetTouchVisualize(bool isVisualizeTouch);

		void TouchDown(int index, Point point);
		void TouchUpdate(int index, Point point);
		void TouchUp(int index);

		void SendTouchData();

	private:
		POINTER_TOUCH_INFO *m_contact;
		vector<POINTER_TOUCH_INFO> *m_injectArray;

	};
}
