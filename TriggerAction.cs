using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XboxInputMapper.Native;

namespace XboxInputMapper
{
	static class TriggerAction
	{
		public enum ActionType
		{
			YDown,
			YUp,
			BDown,
			BUp,
			ADown,
			AUp,
		}

		public static ActionType TriangleTripletFrenzy(int frame)
		{
			return (ActionType)(frame % 6);
		}

		public static bool IsButtonDown(ActionType type)
		{
			return type == ActionType.YDown || type == ActionType.BDown || type == ActionType.ADown;
		}

		public static int GetButtonIndex(ActionType type)
		{
			XInput.GamePadButton? button = null;
			switch (type) {
				case ActionType.YDown:
				case ActionType.YUp:
					button = XInput.GamePadButton.Y;
					break;
				case ActionType.BDown:
				case ActionType.BUp:
					button = XInput.GamePadButton.B;
					break;
				case ActionType.ADown:
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
