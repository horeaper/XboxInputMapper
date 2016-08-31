using System;
using XboxInputMapper.Native;

namespace XboxInputMapper
{
	static class TriggerAction
	{
		public enum ActionType
		{
			YDown,
			YUpdate,
			YUp,
			YIdle,
			BDown,
			BUpdate,
			BUp,
			BIdle,
			ADown,
			AUpdate,
			AUp,
			AIdle,

			Count
		}

		public static ActionType TriangleTripletFrenzy(int frame)
		{
			return (ActionType)(frame % (int)ActionType.Count);
		}

		public static bool IsButtonDown(ActionType type)
		{
			return type == ActionType.YDown || type == ActionType.BDown || type == ActionType.ADown;
		}

		public static bool IsButtonUpdate(ActionType type)
		{
			return type == ActionType.YUpdate || type == ActionType.BUpdate || type == ActionType.AUpdate;
		}

		public static bool IsButtonUp(ActionType type)
		{
			return type == ActionType.YUp || type == ActionType.BUp || type == ActionType.AUp;
		}

		public static bool IsButtonIdle(ActionType type)
		{
			return type == ActionType.YIdle || type == ActionType.BIdle || type == ActionType.AIdle;
		}

		public static int GetButtonIndex(ActionType type)
		{
			XInput.GamePadButton? button = null;
			switch (type) {
				case ActionType.YDown:
				case ActionType.YUpdate:
				case ActionType.YUp:
					button = XInput.GamePadButton.Y;
					break;
				case ActionType.BDown:
				case ActionType.BUpdate:
				case ActionType.BUp:
					button = XInput.GamePadButton.B;
					break;
				case ActionType.ADown:
				case ActionType.AUpdate:
				case ActionType.AUp:
					button = XInput.GamePadButton.A;
					break;
			}
			if (button.HasValue) {
				return Array.IndexOf(Constants.ButtonValue, button.Value);
			}
			else {
				throw new ArgumentOutOfRangeException(nameof(type));
			}
		}
	}
}
