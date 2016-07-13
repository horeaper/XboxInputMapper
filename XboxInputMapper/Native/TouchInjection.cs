using System;
using System.Runtime.InteropServices;

namespace XboxInputMapper.Native
{
	/// <summary>
	/// Use this Classes static methods to initialize and inject touch input.
	/// </summary>
	static class TouchInjection
	{
		/// <summary>
		/// Call this first to initialize the TouchInjection!
		/// </summary>
		/// <param name="maxCount">The maximum number of touch points to simulate. Must be less than 256!</param>
		/// <param name="feedbackMode">Specifies the visual feedback mode of the generated touch points</param>
		/// <returns>true if success</returns>
		[DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool InitializeTouchInjection(uint maxCount, TouchFeedback feedbackMode);

		/// <summary>
		/// Inject an array of PointerTouchInfo
		/// </summary>
		/// <param name="count">The exact number of entries in the array</param>
		/// <param name="contacts">The PointerTouchInfo to inject</param>
		/// <returns>true if success</returns>
		[DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool InjectTouchInput(int count, [MarshalAs(UnmanagedType.LPArray)] PointerTouchInfo[] contacts);
	}

	/// <summary>
	/// Enum of touch visualization options
	/// </summary>
	enum TouchFeedback
	{
		/// <summary>
		/// Specifies default touch visualizations.
		/// </summary>
		Default = 0x1,

		/// <summary>
		/// Specifies indirect touch visualizations.
		/// </summary>
		Indirect = 0x2,

		/// <summary>
		/// Specifies no touch visualizations.
		/// </summary>
		None = 0x3
	}

	/// <summary>
	/// The contact area.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	struct ContactArea
	{
		public int Left;
		public int Top;
		public int Right;
		public int Bottom;
	}

	/// <summary>
	/// Values that can appear in the TouchMask field of the PointerTouchInfo structure
	/// </summary>
	[Flags]
	enum TouchFlags
	{ 
		/// <summary>
		/// Indicates that no flags are set.
		/// </summary>
		None = 0x00000000
	}

	/// <summary>
	/// Values that can appear in the TouchMask field of the PointerTouchInfo structure.
	/// </summary>
	[Flags]
	enum TouchMask
	{
		/// <summary>
		/// Default. None of the optional fields are valid.
		/// </summary>
		None = 0x00000000,
		/// <summary>
		/// The ContactArea field is valid
		/// </summary>
		ContactArea = 0x00000001,
		/// <summary>
		/// The orientation field is valid
		/// </summary>
		Orientation = 0x00000002,
		/// <summary>
		/// The pressure field is valid
		/// </summary>
		Pressure = 0x00000004
	}

	/// <summary>
	/// Values that can appear in the PointerFlags field of the PointerInfo structure.
	/// </summary>
	[Flags]
	enum PointerFlags
	{
		/// <summary>
		/// Default
		/// </summary>
		None = 0x00000000,

		/// <summary>
		/// Indicates the arrival of a new pointer
		/// </summary>
		New = 0x00000001,

		/// <summary>
		/// Indicates that this pointer continues to exist. When this flag is not set, it indicates the pointer has left detection range. 
		/// This flag is typically not set only when a hovering pointer leaves detection range (PointerFlag.UPDATE is set) or when a pointer in contact with a window surface leaves detection range (PointerFlag.UP is set). 
		/// </summary>
		InRange = 0x00000002,

		/// <summary>
		/// Indicates that this pointer is in contact with the digitizer surface. When this flag is not set, it indicates a hovering pointer.
		/// </summary>
		InContact = 0x00000004,

		/// <summary>
		/// Indicates a primary action, analogous to a mouse left button down.
		///A touch pointer has this flag set when it is in contact with the digitizer surface.
		///A pen pointer has this flag set when it is in contact with the digitizer surface with no buttons pressed.
		///A mouse pointer has this flag set when the mouse left button is down.
		/// </summary>
		FirstButton = 0x00000010,

		/// <summary>
		/// Indicates a secondary action, analogous to a mouse right button down.
		/// A touch pointer does not use this flag.
		/// A pen pointer has this flag set when it is in contact with the digitizer surface with the pen barrel button pressed.
		/// A mouse pointer has this flag set when the mouse right button is down.
		/// </summary>
		SecondButton = 0x00000020,

		/// <summary>
		/// Analogous to a mouse wheel button down.
		/// A touch pointer does not use this flag. 
		/// A pen pointer does not use this flag. 
		/// A mouse pointer has this flag set when the mouse wheel button is down.
		/// </summary>
		ThirdButton = 0x00000040,

		/// <summary>
		/// Indicates that this pointer has been designated as primary. A primary pointer may perform actions beyond those available to non-primary pointers. For example, when a primary pointer makes contact with a window’s surface, it may provide the window an opportunity to activate by sending it a WM_POINTERACTIVATE message.
		/// </summary>
		Primary = 0x00002000,

		/// <summary>
		/// Confidence is a suggestion from the source device about whether the pointer represents an intended or accidental interaction, which is especially relevant for PT_TOUCH pointers where an accidental interaction (such as with the palm of the hand) can trigger input. The presence of this flag indicates that the source device has high confidence that this input is part of an intended interaction.
		/// </summary>
		Confidence = 0x00004000,

		/// <summary>
		/// Indicates that the pointer is departing in an abnormal manner, such as when the system receives invalid input for the pointer or when a device with active pointers departs abruptly. If the application receiving the input is in a position to do so, it should treat the interaction as not completed and reverse any effects of the concerned pointer.
		/// </summary>
		Canceled = 0x00008000,

		/// <summary>
		/// Indicates that this pointer just transitioned to a “down” state; that is, it made contact with the window surface.
		/// </summary>
		Down = 0x00010000,

		/// <summary>
		/// Indicates that this information provides a simple update that does not include pointer state changes.
		/// </summary>
		Update = 0x00020000,

		/// <summary>
		/// Indicates that this pointer just transitioned to an “up” state; that is, it broke contact with the window surface.
		/// </summary>
		Up = 0x00040000,

		/// <summary>
		/// Indicates input associated with a pointer wheel. For mouse pointers, this is equivalent to the action of the mouse scroll wheel (WM_MOUSEWHEEL).
		/// </summary>
		Wheel = 0x00080000,

		/// <summary>
		/// Indicates input associated with a pointer h-wheel. For mouse pointers, this is equivalent to the action of the mouse horizontal scroll wheel (WM_MOUSEHWHEEL).
		/// </summary>
		HWheel = 0x00100000
	}
	
	/// <summary>
	/// The TouchPoint structure defines the x- and y- coordinates of a point.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	struct TouchPoint
	{
		/// <summary>
		/// The x-coordinate of the point.
		/// </summary>
		public int X;

		/// <summary>
		/// The y-coordinate of the point.
		/// </summary>
		public int Y;
	}

	/// <summary>
	/// Identifies the pointer input types.
	/// </summary>
	enum PointerInputType
	{
		/// <summary>
		/// Generic pointer type. This type never appears in pointer messages or pointer data. Some data query functions allow the caller to restrict the query to specific pointer type. The PT_POINTER type can be used in these functions to specify that the query is to include pointers of all types
		/// </summary>
		Pointer = 0x00000001,

		/// <summary>
		/// Touch pointer type.
		/// </summary>
		Touch = 0x00000002,

		/// <summary>
		/// Pen pointer type.
		/// </summary>
		Pen = 0x00000003,

		/// <summary>
		/// Mouse pointer type
		/// </summary>
		Mouse = 0x00000004
	};
	
	/// <summary>
	/// Contains basic pointer information common to all pointer types. Applications can retrieve this information using the GetPointerInfo, GetPointerFrameInfo, GetPointerInfoHistory and GetPointerFrameInfoHistory functions. 
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	struct PointerInfo
	{
		/// <summary>
		/// A value from the PointerInputType enumeration that specifies the pointer type.
		/// </summary>
		public PointerInputType PointerType;

		/// <summary>
		/// An identifier that uniquely identifies a pointer during its lifetime. A pointer comes into existence when it is first detected and ends its existence when it goes out of detection range. Note that if a physical entity (finger or pen) goes out of detection range and then returns to be detected again, it is treated as a new pointer and may be assigned a new pointer identifier.
		/// </summary>
		public uint PointerId;

		/// <summary>
		/// An identifier common to multiple pointers for which the source device reported an update in a single input frame. For example, a parallel-mode multi-touch digitizer may report the positions of multiple touch contacts in a single update to the system.
		/// Note that frame identifier is assigned as input is reported to the system for all pointers across all devices. Therefore, this field may not contain strictly sequential values in a single series of messages that a window receives. However, this field will contain the same numerical value for all input updates that were reported in the same input frame by a single device.
		/// </summary>
		public uint FrameId;

		/// <summary>
		/// May be any reasonable combination of flags from the Pointer Flags constants.
		/// </summary>
		public PointerFlags PointerFlags;

		/// <summary>
		/// Handle to the source device that can be used in calls to the raw input device API and the digitizer device API.
		/// </summary>
		public IntPtr SourceDevice;

		/// <summary>
		/// Window to which this message was targeted. If the pointer is captured, either implicitly by virtue of having made contact over this window or explicitly using the pointer capture API, this is the capture window. If the pointer is uncaptured, this is the window over which the pointer was when this message was generated.
		/// </summary>
		public IntPtr WindowTarget;

		/// <summary>
		/// Location in screen coordinates.
		/// </summary>
		public TouchPoint PixelLocation;

		/// <summary>
		/// Location in device coordinates.
		/// </summary>
		public TouchPoint PixelLocationRaw;
	   
		/// <summary>
		/// Location in HIMETRIC units.
		/// </summary>
		public TouchPoint HimetricLocation;

		/// <summary>
		/// Location in device coordinates in HIMETRIC units.
		/// </summary>
		public TouchPoint HimetricLocationRaw;
		
		/// <summary>
		/// A message time stamp assigned by the system when this input was received.
		/// </summary>
		public uint Time;

		/// <summary>
		/// Count of inputs that were coalesced into this message. This count matches the total count of entries that can be returned by a call to GetPointerInfoHistory. If no coalescing occurred, this count is 1 for the single input represented by the message.
		/// </summary>
		public uint HistoryCount;

		/// <summary>
		/// A value whose meaning depends on the nature of input. 
		/// When flags indicate PointerFlag.WHEEL, this value indicates the distance the wheel is rotated, expressed in multiples or factors of WHEEL_DELTA. A positive value indicates that the wheel was rotated forward and a negative value indicates that the wheel was rotated backward. 
		/// When flags indicate PointerFlag.HWHEEL, this value indicates the distance the wheel is rotated, expressed in multiples or factors of WHEEL_DELTA. A positive value indicates that the wheel was rotated to the right and a negative value indicates that the wheel was rotated to the left. 
		/// </summary>
		public int InputData;

		/// <summary>
		/// Indicates which keyboard modifier keys were pressed at the time the input was generated. May be zero or a combination of the following values. 
		/// POINTER_MOD_SHIFT – A SHIFT key was pressed. 
		/// POINTER_MOD_CTRL – A CTRL key was pressed. 
		/// </summary>
		public uint KeyStates;

		/// <summary>
		/// The value of the high-resolution performance counter when the pointer message was received (high-precision, 64 bit alternative to dwTime). The value can be calibrated when the touch digitizer hardware supports the scan timestamp information in its input report.
		/// </summary>
		public ulong PerformanceCount;

		/// <summary>
		/// A value from the PointerButtonChangeType enumeration that specifies the change in button state between this input and the previous input.
		/// </summary>
		public PointerButtonChangeType ButtonChangeType;
	}

	/// <summary>
	/// Enumeration of PointerButtonChangeTypes
	/// </summary>
	enum PointerButtonChangeType
	{
		NONE,
		FIRSTBUTTON_DOWN,
		FIRSTBUTTON_UP,
		SECONDBUTTON_DOWN,
		SECONDBUTTON_UP,
		THIRDBUTTON_DOWN,
		THIRDBUTTON_UP,
		FOURTHBUTTON_DOWN,
		FOURTHBUTTON_UP,
		FIFTHBUTTON_DOWN,
		FIFTHBUTTON_UP
	}

	/// <summary>
	/// Contains information about a 'contact' (coordinates, size, pressure...)
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	struct PointerTouchInfo
	{
		///<summary>
		/// Contains basic pointer information common to all pointer types.
		///</summary>
		public PointerInfo PointerInfo;

		///<summary>
		/// Lists the touch flags.
		///</summary>
		public TouchFlags TouchFlags;

		/// <summary>
		/// Indicates which of the optional fields contain valid values. The member can be zero or any combination of the values from the Touch Mask constants.
		/// </summary>
		public TouchMask TouchMasks;

		///<summary>
		/// Pointer contact area in pixel screen coordinates. 
		/// By default, if the device does not report a contact area, 
		/// this field defaults to a 0-by-0 rectangle centered around the pointer location.
		///</summary>
		public ContactArea ContactArea;

		/// <summary>
		/// A raw pointer contact area.
		/// </summary>
		public ContactArea ContactAreaRaw;

		///<summary>
		/// A pointer orientation, with a value between 0 and 359, where 0 indicates a touch pointer 
		/// aligned with the x-axis and pointing from left to right; increasing values indicate degrees
		/// of rotation in the clockwise direction.
		/// This field defaults to 0 if the device does not report orientation.
		///</summary>
		public uint Orientation;

		///<summary>
		/// Pointer pressure normalized in a range of 0 to 256.
		///</summary>
		public uint Pressure;
	}
}
