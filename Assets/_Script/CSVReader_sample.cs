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
		

	public AnimationCurve CreateLinearAnimationCurve () {
		AnimationCurve curve = new AnimationCurve ();
		for (int i = 0; i < csvFile.Count; i++) {
			// 100で割って正規化
//			curve.AddKey (KeyframeUtil.GetNew (GetTotalSecond (csvFile [i].time), (float)csvFile [i].value_ / 100f, TangentMode.Linear));
			float f = GetTotalMilliSecond (csvFile [i].time);
			// 秒にする
			f = f / 1000f;
			curve.AddKey (KeyframeUtil.GetNew (f, (float)csvFile [i].value_ / 100f, TangentMode.Linear));
		}
		curve.UpdateAllLinearTangents ();

		return curve;
	}


}
