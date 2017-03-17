using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.IO;
using System;


/// <summary>
/// CSVの列とのマッピングのための属性クラス
/// </summary>
public class CsvColumnAttribute : Attribute {
	public CsvColumnAttribute (int columnIndex)
		: this (columnIndex, null) {
	}
	public CsvColumnAttribute (int columnIndex, object defaultValue) {
		this.ColumnIndex = columnIndex;
		this.DefaultValue = defaultValue;
	}

	public int ColumnIndex{ get; set; }
	public object DefaultValue{ get; set; }
		
}


public class CSVReader<T> : IEnumerable<T>, IDisposable where T : class, new() {

	public event EventHandler<ConvertFailedEventArgs> ConvertFailed;

	/// <summary>
	/// Type毎のデータコンバータ
	/// </summary>
	private Dictionary<Type, TypeConverter> converters = new Dictionary<Type, TypeConverter> ();

	/// <summary>
	/// 列番号をキーとしてフィールド or プロパティへのsetメソッドが格納される
	/// </summary>
	private Dictionary<int, Action<object, string>> setters = new Dictionary<int, Action<object, string>> ();

	/// <summary>
	/// Tの情報をLoad
	/// setterには列番号をキーとしたsetメソッドが格納される
	/// </summary>
	private void LoadType () {
		Type type = typeof (T);

		// Field, Propertyのみを対象とする
		var memberTypes = new MemberTypes[]{ MemberTypes.Field, MemberTypes.Property };

		// インスタンスメンバを対象
		BindingFlags flag = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

		foreach (MemberInfo member in type.GetMembers(flag).Where((member) => memberTypes.Contains(member.MemberType))) {
			CsvColumnAttribute csvColumn = GetCsvColumnAttribute (member);

			if (csvColumn == null) {
				continue;
			}

			int columnIndex = csvColumn.ColumnIndex;
			object defaultValue = csvColumn.DefaultValue;

			if (member.MemberType == MemberTypes.Field) {
				// field
				FieldInfo fieldInfo = type.GetField (member.Name, flag);
				setters [columnIndex] = (target, value) => 
					fieldInfo.SetValue (target, GetConvertedValue (fieldInfo, value, defaultValue));
			} else {
				// property
				PropertyInfo propertyInfo = type.GetProperty (member.Name, flag);
				setters [columnIndex] = (target, value) =>
					propertyInfo.SetValue (target, GetConvertedValue (propertyInfo, value, defaultValue), null);
			}
		}
	}

	/// <summary>
	/// 対象のMemberInfoからCsvColumnAttributeを取得する
	/// </summary>
	/// <returns>The csv column attribute.</returns>
	/// <param name="member">Member.</param>
	private CsvColumnAttribute GetCsvColumnAttribute (MemberInfo member) {
		return (CsvColumnAttribute)member.GetCustomAttributes (typeof (CsvColumnAttribute), true).FirstOrDefault ();
	}


	/// <summary>
	/// valueを対象のTypeへ変換する。出来ない場合はdefaultを返す
	/// </summary>
	/// <returns>The converted value.</returns>
	/// <param name="info">Info.</param>
	/// <param name="value">Value.</param>
	/// <param name="default">Default.</param>
	private object GetConvertedValue (MemberInfo info, object value, object @default) {
		Type type = null;
		if (info is FieldInfo) {
			type = (info as FieldInfo).FieldType;
		} else if (info is PropertyInfo) {
			type = (info as PropertyInfo).PropertyType;
		}

		// コンバータは同じTypeを使用することがあるためキャッシュしておく
		if (!converters.ContainsKey (type)) {
			converters [type] = TypeDescriptor.GetConverter (type);
		}

		TypeConverter converter = converters [type];

		// 変換出来ない場合に例外を受け取りたい場合
//		return converter.ConvertFrom(value);

		// 失敗した場合にCsvColumnAttributeの規定値プロパティを返す場合
		try {
			// 変換した値を返す
			return converter.ConvertFrom (value);
		} catch (Exception) {
			// 変換出来なかった場合は規定値を返す
			return @default;
		}
	}


	private StringReader reader;
	private string filePath;
	private bool skipFirstLine;
	private Encoding encoding;

	public CSVReader (string filePath)
		: this (filePath, true) {
		
	}

	public CSVReader (string filePath, bool skipFirstLine)
		: this (filePath, skipFirstLine, null) {
	}

	public CSVReader (string filePath, bool skipFirstLine, Encoding encoding) {
		// 拡張子確認
//		if (!filePath.EndsWith (".csv", StringComparison.CurrentCultureIgnoreCase)) {
//			throw new FormatException ("拡張子が.csvでないファイル名が指定されました");
//		}

		this.filePath = filePath;
		this.skipFirstLine = skipFirstLine;
		this.encoding = encoding;

		// 規定のエンコードの設定
		if (this.encoding == null) {
			this.encoding = Encoding.GetEncoding ("utf-8");
		}

		// Tを解析する
		LoadType ();
		TextAsset csv = Resources.Load (this.filePath) as TextAsset;


		this.reader = new StringReader (csv.text);
		// ヘッダを飛ばす場合は1行読む
		if (skipFirstLine) {
			this.reader.ReadLine ();
		}
	}


	public void Dispose () {
		using (reader) {
		}
		reader = null;
	}

	public IEnumerator<T> GetEnumerator () {
		string line;
		while ((line = reader.ReadLine ()) != null) {
			// Tのインスタンス作成
			var data = new T ();

			// 行をセパレータで分解
			string[] fields = line.Split (',');

			// セル数分ループを回す
			foreach (int columnIndex in Enumerable.Range(0, fields.Length)) {
				// 列番号に対応するsetメソッドがない場合は処理しない
				if (!setters.ContainsKey (columnIndex)) {
					continue;
				}

				// setメソッドでdataに値を入れる
				setters [columnIndex] (data, fields [columnIndex]);
			}

			yield return data;
		}
	}

	System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator () {
		return this.GetEnumerator ();
	}
}

/// <summary>
/// 変換失敗したときのイベント引数クラス
/// </summary>
public class ConvertFailedEventArgs : EventArgs {
	public ConvertFailedEventArgs (MemberInfo info, object value, object defaultValue, Exception ex) {
		this.MemberInfo = info;
		this.FailedValue = value;
		this.CorrectValue = defaultValue;
		this.Exception = ex;
	}

	// 変換に失敗したメンバの情報
	public MemberInfo MemberInfo{ get; private set; }

	// 失敗時の値
	public object FailedValue{ get; private set; }

	// 正しい値をイベントで受け取る側が設定する。規定値はCsvColumnAttribute.DefaultValue
	public object CorrectValue{ get; set; }

	// 発生した例外
	public Exception Exception{ get; private set; }
}
