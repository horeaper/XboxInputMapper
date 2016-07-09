#pragma once

namespace XboxInputMapper
{
	using namespace System;
	using namespace System::Collections::Generic;
	using namespace System::Windows;
	struct InputMapperLibraryPimpl;

	public ref class InputMapperLibrary
	{
	public:
		InputMapperLibrary(bool isVisualizeTouch);
		~InputMapperLibrary();
		!InputMapperLibrary();

		static const int ButtonCount = 14;
		static const int LeftTriggerButtonId = 14;
		static const int RightTriggerButtonId = 15;
		static const int MaxTouchCount = 16;

		void SetTouchVisualize(bool isVisualizeTouch);

		void DirectionDown(Point point);
		void DirectionUpdate(Point point);
		void DirectionUp();

		void ButtonDown(int buttonId, List<Point> ^pointArray, Point pointOffset);
		void ButtonUpdate(int buttonId, List<Point> ^pointArray, Point pointOffset);
		void ButtonUp(int buttonId);

		void SendTouchData();

	private:
		int FindAvailableIndex();
		void PointerDown(int index, int x, int y);
		void PointerUpdate(int index, int x, int y);
		void PointerUp(int index);

		InputMapperLibraryPimpl *m_pimpl;
	};

	struct InputMapperLibraryPimpl
	{
		POINTER_TOUCH_INFO contact[InputMapperLibrary::MaxTouchCount];

		int DirectionTouchId;
		vector<int> ButtonTouchId[InputMapperLibrary::ButtonCount + 2];
	};
}
