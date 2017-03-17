using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;

namespace CurveExtended {

	public enum TangentMode {
		Editable = 0,
		Smooth = 1,
		Linear = 2,
		Stepped = Smooth | Linear
	}

	public enum TangentDirection {
		Left,
		Right
	}



	public class KeyframeUtil {

		public static Keyframe GetNew (float time, float value, TangentMode leftAndRight) {
			return GetNew (time, value, leftAndRight, leftAndRight);
		}

		public static Keyframe GetNew (float time, float value, TangentMode left, TangentMode right) {
			object boxed = new Keyframe (time, value);

			SetKeyBroken (boxed, true);
			SetKeyTangentMode (boxed, 0, left);
			SetKeyTangentMode (boxed, 1, right);

			Keyframe keyframe = (Keyframe)boxed;
			if (left == TangentMode.Stepped) {
				keyframe.inTangent = float.PositiveInfinity;
			}
			if (right == TangentMode.Stepped) {
				keyframe.outTangent = float.PositiveInfinity;
			}

			return keyframe;
		}


		public static void SetKeyTangentMode (object keyframe, int leftRight, TangentMode mode) {
			Type t = typeof (UnityEngine.Keyframe);
			FieldInfo field = t.GetField ("m_TangentMode", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			int tangentMode = (int)field.GetValue (keyframe);

			if (leftRight == 0) {
				tangentMode &= -7;
				tangentMode |= (int)mode << 1;
			
			} else {
				tangentMode &= -25;
				tangentMode |= (int)mode << 3;
			}

			field.SetValue (keyframe, tangentMode);
			if (GetKeyTangentMode (tangentMode, leftRight) == mode) {
				return;
			}

			Debug.Log ("BUG");
		}


		public static TangentMode GetKeyTangentMode (int tangentMode, int leftRight) {
			if (leftRight == 0) {
				return (TangentMode)((tangentMode & 6) >> 1);
			} else {
				return (TangentMode)((tangentMode & 24) >> 3);

			}
		}

		public static TangentMode GetKeyTangentMode (Keyframe keyframe, int leftRight) {
			Type t = typeof (UnityEngine.Keyframe);
			FieldInfo field = t.GetField ("m_TangentMode", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			int tangentMode = (int)field.GetValue (keyframe);

			if (leftRight == 0) {
				return (TangentMode)((tangentMode & 6) >> 1);
			} else {
				return (TangentMode)((tangentMode & 24) >> 3);

			}
		}


		public static void SetKeyBroken (object keyframe, bool broken) {
			Type t = typeof (UnityEngine.Keyframe);
			FieldInfo field = t.GetField ("m_TangentMode", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			int tangentMode = (int)field.GetValue (keyframe);

			if (broken) {
				tangentMode |= 1;
			} else {
				tangentMode &= -2;
			}

			field.SetValue (keyframe, tangentMode);
		}
	}
}
