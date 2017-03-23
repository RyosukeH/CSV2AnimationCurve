using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using CurveExtended;

public class CSVReader_sample : MonoBehaviour {

	/// <summary>
	/// AnimationCuve作成用csvファイル。keyとvalueのみのフォーマット
	/// </summary>
	private class Ww_csv_File {
		[CsvColumnAttribute (0, "0:00")]
		public string time{ get; set; }

		[CsvColumnAttribute (1, 0)]
		public int value_;

		public override string ToString () {
			return string.Format ("time={0}, value={1}", time, value_);
		}
	}


	List<Ww_csv_File> csvFile;

	[SerializeField]
	string csvFilePath = "CSV/";

	public void ReadCSVFile (bool isSkipFirstLine = true) {
		csvFile = new List<Ww_csv_File> ();

		string filePath = csvFilePath;

		using (var reader = new CSVReader<Ww_csv_File> (filePath, isSkipFirstLine)) {
			reader.ToList ().ForEach (load_ => {
				Debug.Log (load_.ToString ());
				csvFile.Add (load_);
			});

			Debug.Log ("csvFile count: " + csvFile.Count);
		}
	}


	float GetTotalSecond (string time_) {
		// csvでは00:00の記述なので時間の部分を追加している
		// 00:00:00の書き方ではこの部分不要
		string temp = "0:" + time_;		
		TimeSpan ts = TimeSpan.Parse (temp);
		//		Debug.Log ("ts total sec: " + ts.TotalSeconds);
		return (float)ts.TotalSeconds;

	}

	float GetTotalMilliSecond (string time_) {
		// csvでは00:00の記述なので時間の部分を追加している
		// 00:00:00の書き方ではこの部分不要
		//		string temp = "0:" + time_;		
		TimeSpan ts = TimeSpan.Parse (time_);
		//		TimeSpan ts = TimeSpan.Parse (temp);
		//		Debug.Log ("ts total sec: " + ts.TotalSeconds);
		//		return (float)ts.TotalSeconds;
		return (float)ts.TotalMilliseconds;

	}
		

	//	public AnimationCurve CreateLinearAnimationCurve () {
	//		AnimationCurve curve = new AnimationCurve ();
	//		for (int i = 0; i < csvFile.Count; i++) {
	//			// 100で割って正規化
	////			curve.AddKey (KeyframeUtil.GetNew (GetTotalSecond (csvFile [i].time), (float)csvFile [i].value_ / 100f, TangentMode.Linear));
	//			float f = GetTotalMilliSecond (csvFile [i].time);
	//			// 秒にする
	//			f = f / 1000f;
	//
	//			// Editor上でしか使用できない...
	////			curve.AddKey (KeyframeUtil.GetNew (f, (float)csvFile [i].value_ / 100f, TangentMode.Linear));
	//			curve.AddKey (new Keyframe (f, (float)csvFile [i].value_ / 100f));
	//		}
	////		curve.UpdateAllLinearTangents ();
	//
	//		return curve;
	//	}

	/// <summary>
	/// Creates the smooth tangent animation curve.
	/// </summary>
	/// <returns>The smooth animation curve.</returns>
	public AnimationCurve CreateSmoothAnimationCurve () {
		AnimationCurve curve = new AnimationCurve ();
		for (int i = 0; i < csvFile.Count; i++) {
			
			float f = GetTotalMilliSecond (csvFile [i].time);
			// 秒にする
			f = f / 1000f;
	
			// Editor上でしか使用できない...
			//			curve.AddKey (KeyframeUtil.GetNew (f, (float)csvFile [i].value_ / 100f, TangentMode.Linear));

			// 100で割って正規化
			curve.AddKey (new Keyframe (f, (float)csvFile [i].value_ / 100f));
		}

		return curve;
	}

	/// <summary>
	/// Creates the linear tangent animation curve.
	/// </summary>
	/// <returns>The linear animation curve.</returns>
	public AnimationCurve CreateLinearAnimationCurve () {
		AnimationCurve curve = new AnimationCurve ();
		List<Keyframe> ks = new List<Keyframe> ();
		for (int i = 0; i < csvFile.Count - 1; i++) {

			float f = GetTotalMilliSecond (csvFile [i].time);
			// 秒にする
			f = f / 1000f;

			float f1 = GetTotalMilliSecond (csvFile [i + 1].time);
			f1 = f1 / 1000f;


			// Editor上でしか使用できない...
			//			curve.AddKey (KeyframeUtil.GetNew (f, (float)csvFile [i].value_ / 100f, TangentMode.Linear));

			// 100で割って正規化
			AnimationCurve c = AnimationCurve.Linear (f, (float)csvFile [i].value_ / 100f, f1, (float)csvFile [i + 1].value_ / 100f);
			foreach (Keyframe _k in c.keys) {
				ks.Add (_k);
			}
		}

		curve = new AnimationCurve (ks.ToArray ());
		//		curve.UpdateAllLinearTangents ();

		return curve;
	}


}
