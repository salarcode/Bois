using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;

namespace Salar.Bion
{
	/// <summary>
	/// Binary Indexed Object Notation Serializer
	/// </summary>
	/// <Author>
	/// Salar Khalilzadeh
	/// </Author>
	public class BionSerializer_NullCheck
	{
		private bool _useUtcDateTime = true;
		private BinaryWriter _serializeOut;
		private int _serializeDepth;
		private readonly ReflectionCache _reflection = new ReflectionCache();
		private BinaryReader _input;

		public Encoding Encoding { get; set; }

		public bool UseUtcDateTime
		{
			get { return _useUtcDateTime; }
			set { _useUtcDateTime = value; }
		}

		public BionSerializer_NullCheck()
		{
			Encoding = Encoding.UTF8;
		}

		public void Serialize<T>(T obj, Stream output)
		{
			if (obj == null)
				throw new ArgumentNullException("obj", "Object can not be null!");
			_serializeDepth = 0;

			_serializeOut = new BinaryWriter(output, Encoding);

			WriteValue(obj, typeof(T));
		}

		public T Deserialize<T>(Stream objectData)
		{
			_input = new BinaryReader(objectData, Encoding);
			return (T)ReadMember(typeof(T));
		}


		#region Serialization methods

		private void WriteObject(object obj)
		{
			_serializeDepth++;

			var type = obj.GetType();
			var info = _reflection.GetTypeInfo(type);

			var props = info.Properties;
			var fields = info.Fields;

			var values = new List<KeyValuePair<object, Type>>();
			//ushort memIndex = 0;

			// reading property values
			for (short i = 0; i < props.Length; i++)
			{
				var p = props[i];

				var value = _reflection.GetValue(obj, p);
				values.Add(new KeyValuePair<object, Type>(value, p.PropertyType));
				//memIndex++;
			}

			for (short i = 0; i < fields.Length; i++)
			{
				var f = fields[i];
				var value = f.GetValue(obj);

				values.Add(new KeyValuePair<object, Type>(value, f.FieldType));
				//memIndex++;
			}

			// number of non-null members count
			var valCount = (ushort)values.Count;
			_serializeOut.Write(valCount);

			// sort by index, neede feature in future
			//values.Sort();

			if (valCount > 0)
			{
				foreach (KeyValuePair<object, Type> val in values)
				{
					WriteValue(val.Key, val.Value);
				}
			}

			_serializeDepth--;
		}

		//void WriteMemberValue(ushort index, object value)
		//{
		//	WriteValue(value);
		//}

		private void WriteNullableType(bool isnull)
		{
			_serializeOut.Write(isnull ? (byte)1 : (byte)0);
		}


		void WriteValue(object obj, Type objType = null)
		{
			if (obj == null)
			{
				WriteNullableType(true);
				return;
			}
			else if (obj is DBNull)
			{
				WriteNullableType(true);
				return;
			}

			if (objType == null)
			{
				objType = obj.GetType();
			}
			if (ReflectionHelper.IsNullable(objType))
			{
				// this is an nullable type
				// but is not null anyway!
				WriteNullableType(false);
			}


			// starting the check of the shit!
			if (obj is string)
			{
				WriteString(obj as string); //.ToString());
			}
			else if (obj is char)
			{
				_serializeOut.Write((char)obj);
			}
			else if (obj is Guid)
			{
				WriteGuid((Guid)obj);
			}
			else if (obj is bool)
			{
				_serializeOut.Write((byte)(((bool)obj) ? 1 : 0));
			}
			else if (
				obj is int || obj is long || obj is double ||
				obj is decimal || obj is float ||
				obj is byte || obj is short ||
				obj is sbyte || obj is ushort ||
				obj is uint || obj is ulong
				)
			{
				WriteNumber(obj);
			}
			else if (obj is DateTime)
			{
				WriteDateTime((DateTime)obj);
			}
			else if (obj is IDictionary && objType.IsGenericType &&
					 objType.GetGenericArguments()[0] == typeof(string))
			{
				WriteStringDictionary(obj as IDictionary);
			}
			else if (obj is IDictionary)
			{
				WriteDictionary(obj as IDictionary);
			}
#if !SILVERLIGHT
			else if (obj is DataSet)
			{
				WriteDataset(obj as DataSet);
			}
			else if (obj is DataTable)
			{
				WriteDataTable(obj as DataTable);
			}
#endif
			else if (obj is byte[])
			{
				WriteBytes((byte[])obj);
			}
			else if (obj is NameValueCollection)
			{
				WriteCollectionNameValue(obj as NameValueCollection);
			}
			else if (obj is Array || obj is IList || obj is ICollection)
			{
				WriteArray(obj as IEnumerable);
			}
			else if (obj is Enum)
			{
				WriteEnum((Enum)obj);
			}
			else if (obj is Color)
			{
				WriteColor((Color)obj);
			}
			else if (obj is TimeSpan)
			{
				WriteTimeSpan((TimeSpan)obj);
			}
			else if (obj is Version)
			{
				WriteVersion(obj as Version);
			}
			else
			{
				WriteObject(obj);
			}
		}

#if !SILVERLIGHT
		private string GetXmlSchema(DataTable dt)
		{
			using (var writer = new StringWriter())
			{
				dt.WriteXmlSchema(writer);
				return writer.ToString();
			}
		}

		private void WriteDataset(DataSet ds)
		{
			_serializeOut.Write(ds.Tables.Count);
			foreach (DataTable table in ds.Tables)
			{
				WriteDataTable(table);
			}
		}

		private void WriteDataTable(DataTable table)
		{
			if (string.IsNullOrEmpty(table.TableName))
				table.TableName = "tbl_" + DateTime.Now.Ticks.GetHashCode().ToString();
			WriteString(GetXmlSchema(table));

			var colsCount = table.Columns.Count;
			_serializeOut.Write(table.Rows.Count);

			foreach (DataRow row in table.Rows)
			{
				WriteDataRow(row, colsCount);
			}
		}

		private void WriteDataRow(DataRow row, int columnCount)
		{
			var values = new Dictionary<int, object>();
			for (int i = 0; i < columnCount; i++)
			{
				var val = row[i];
				if (val != null && !Convert.IsDBNull(val))
					values.Add(i, val);
			}

			// count of non-null columns
			_serializeOut.Write(values.Count);
			foreach (var value in values)
			{
				_serializeOut.Write(value.Key);
				WriteValue(value.Value);
			}
		}

#endif

		private void WriteUnknownObject(object obj, Type objType)
		{
			//if (objType != null && ReflectionHelper.IsNullable(objType))
			//{
			//	// this is an nullable type
			//	// but is not null anyway!
			//	WriteNullableType(false);
			//}
			WriteObject(obj);
		}

		private void WriteColor(Color c)
		{
			int argb = c.ToArgb();
			_serializeOut.Write(argb);
		}

		private void WriteEnum(Enum e)
		{
			_serializeOut.Write((int)((object)e));
		}

		private void WriteCollectionNameValue(NameValueCollection nameValue)
		{
			var count = (ushort)nameValue.Count;
			_serializeOut.Write(count);

			foreach (string key in nameValue)
			{
				WriteValue(key);
				WriteValue(nameValue[key]);
			}
		}

		private void WriteArray(IEnumerable array)
		{
			ushort count = 0;
			var col = array as ICollection;
			if (col != null)
				count = (ushort)col.Count;
			else
			{
				foreach (object obj in array)
					count++;
			}

			_serializeOut.Write(count);
			foreach (object obj in array)
			{
				WriteValue(obj);
			}
		}

		private void WriteBytes(byte[] bytes)
		{
			_serializeOut.Write(bytes.Length);
			_serializeOut.Write(bytes);
		}

		private void WriteDictionary(IDictionary dic)
		{
			_serializeOut.Write(dic.Count);

			foreach (DictionaryEntry entry in dic)
			{
				WritePairObject(entry.Key, entry.Value);
			}
		}

		private void WriteStringDictionary(IDictionary dic)
		{
			// Count type is int
			_serializeOut.Write(dic.Count);
			foreach (DictionaryEntry entry in dic)
			{
				WritePairString(entry.Key.ToString(), entry.Value);
			}
		}

		private void WritePairString(string name, object value)
		{
			WriteString(name, true);
			WriteValue(value);
		}

		private void WritePairObject(object key, object value)
		{
			WriteValue(key);
			WriteValue(value);
		}

		private void WriteDateTime(DateTime dateTime)
		{
			var dt = dateTime;
			if (UseUtcDateTime)
				dt = dateTime.ToUniversalTime();
			_serializeOut.Write(dt.Ticks);
		}

		private void WriteTimeSpan(TimeSpan timeSpan)
		{
			_serializeOut.Write(timeSpan.Ticks);
		}
		private void WriteVersion(Version version)
		{
			WriteString(version.ToString());
		}

		private void WriteNumber(object obj)
		{
			if (obj is int)
			{
				_serializeOut.Write((int)obj);
			}
			else if (obj is long)
			{
				_serializeOut.Write((long)obj);
			}
			else if (obj is double)
			{
				_serializeOut.Write((double)obj);
			}
			else if (obj is decimal)
			{
				_serializeOut.Write((decimal)obj);
			}
			else if (obj is float)
			{
				_serializeOut.Write((float)obj);
			}
			else if (obj is byte)
			{
				_serializeOut.Write((byte)obj);
			}
			else if (obj is short)
			{
				_serializeOut.Write((short)obj);
			}
			else if (obj is sbyte)
			{
				_serializeOut.Write((sbyte)obj);
			}
			else if (obj is ushort)
			{
				_serializeOut.Write((ushort)obj);
			}
			else if (obj is uint)
			{
				_serializeOut.Write((uint)obj);
			}
			else if (obj is ulong)
			{
				_serializeOut.Write((ulong)obj);
			}
		}

		private void WriteString(string str)
		{
			WriteBytes(Encoding.GetBytes(str));
		}
		private void WriteString(string str, bool checkNull)
		{
			if (checkNull)
			{
				if (str == null)
					WriteNullableType(true);
				else
					WriteNullableType(false);
			}
			if (str == null)
			{
				// length of the string array
				_serializeOut.Write((int)0);
			}
			else
			{
				WriteBytes(Encoding.GetBytes(str));
			}
		}

		private void WriteGuid(Guid g)
		{
			var data = g.ToByteArray();
			_serializeOut.Write(data.Length);
			_serializeOut.Write(data);
		}
		#endregion

		#region Deserialization methods
		private object ReadObject(Type type)
		{
			var info = _reflection.GetTypeInfo(type);

			var memberArray = info.Members;
			var memIndex = memberArray.Length;

			var resultObj = _reflection.CreateInstance(type);
			ReadMembers(resultObj, memberArray, memIndex);

			return resultObj;
		}

		private void ReadMembers(object obj, MemberInfo[] memberList, int memberCount)
		//private void ReadMembers(object obj, IList<MemberInfo> memberList)
		{
			var binaryMemberCount = _input.ReadUInt16();

			var objectMemberCount = memberCount;
			var memberProcessed = 0;
			var dataLeng = _input.BaseStream.Length;
			var data = _input.BaseStream;

			// do while all members are processed
			while (memberProcessed < binaryMemberCount &&
				   memberProcessed < objectMemberCount &&
				   data.Position < dataLeng)
			{
				// the member from member list according to the index
				var memInfo = memberList[memberProcessed];
				memberProcessed++;

				object value;
				var propertyInfo = memInfo as PropertyInfo;
				if (propertyInfo != null)
				{
					var pinfo = propertyInfo;
					value = ReadMember(pinfo.PropertyType);

					_reflection.SetValue(obj, value, pinfo);
					//ReflectionHelper.SetValue(obj, value, pinfo);
				}
				else
				{
					var fieldInfo = memInfo as FieldInfo;
					if (fieldInfo != null)
					{
						var finfo = fieldInfo;
						value = ReadMember(finfo.FieldType);

						ReflectionHelper.SetValue(obj, value, finfo);
					}
				}

				//continue, not supported type
			}
		}

 		private object ReadMember(Type memType)
		{
			if (ReflectionHelper.IsNullable(memType))
			{
				bool isNull = _input.ReadByte() != 0;

				if (isNull)
				{
					return null;
				}
			}

			if (memType == typeof(string))
			{
				return ReadString();
			}
			if (memType == typeof(char))
			{
				return ReadChar();
			}
			if (memType == typeof(bool))
			{
				return ReadBoolean();
			}
			if (memType == typeof(DateTime))
			{
				return ReadDateTime();
			}
			if (memType == typeof(byte[]))
			{
				return ReadBytes();
			}
			if (ReflectionHelper.CompareSubType(memType, typeof(Enum)))
			{
				return ReadEnum();
			}
			if (ReflectionHelper.CompareSubType(memType, typeof(Array)))
			{
				return ReadArray(memType);
			}
			if (memType.IsGenericType &&
				ReflectionHelper.CompareInterface(memType, typeof(IDictionary)) &&
				memType.GetGenericArguments()[0] == typeof(string))
			{
				return ReadStringDictionary(memType);
			}
			if (ReflectionHelper.CompareInterface(memType, typeof(IDictionary)))
			{
				return ReadDictionary(memType);
			}
			if (memType == typeof(Color))
			{
				return ReadColor();
			}
			if (memType == typeof(TimeSpan))
			{
				return ReadTimeSpan();
			}
			if (memType == typeof(Version))
			{
				return ReadVersion();
			}
#if !SILVERLIGHT
			if (ReflectionHelper.CompareSubType(memType, typeof(DataSet)))
			{
				return ReadDataset(memType);
			}
			if (ReflectionHelper.CompareSubType(memType, typeof(DataTable)))
			{
				return ReadDataTable();
			}
#endif

			if (ReflectionHelper.CompareSubType(memType, typeof(NameValueCollection)))
			{
				return ReadCollectionNameValue(memType);
			}
			if (ReflectionHelper.CompareInterface(memType, typeof(IList)) ||
				ReflectionHelper.CompareInterface(memType, typeof(ICollection)))
			{
				if (memType.IsGenericType)
				{
					return ReadGenericList(memType);
				}
				return ReadArray(memType);
			}
			object output;
			if (TryReadNumber(memType, out output))
			{
				return output;
			}
			if (memType == typeof(Guid))
			{
				return ReadGuid();
			}
			if (memType == typeof(DBNull))
			{
				// ignore!
				return DBNull.Value;
			}
			return ReadUnknownObject(memType);
		}


		private object ReadUnknownObject(Type memType)
		{
			return ReadObject(memType);
		}

		private bool TryReadNumber(Type memType, out object output)
		{
			if (memType.IsClass)
			{
				output = null;
				return false;
			}
			if (memType == typeof(int))
			{
				output = _input.ReadInt32();
			}
			else if (memType == typeof(long))
			{
				output = _input.ReadInt64();
			}
			else if (memType == typeof(double))
			{
				output = _input.ReadDouble();
			}
			else if (memType == typeof(decimal))
			{
				output = _input.ReadDecimal();
			}
			else if (memType == typeof(float))
			{
				output = _input.ReadSingle();
			}
			else if (memType == typeof(byte))
			{
				output = _input.ReadByte();
			}
			else if (memType == typeof(short))
			{
				output = _input.ReadInt16();
			}
			else if (memType == typeof(sbyte))
			{
				output = _input.ReadSByte();
			}
			else if (memType == typeof(ushort))
			{
				output = _input.ReadUInt16();
			}
			else if (memType == typeof(uint))
			{
				output = _input.ReadUInt32();
			}
			else if (memType == typeof(ulong))
			{
				output = _input.ReadUInt64();
			}
			else
			{
				output = null;
				return false;
			}
			return true;
		}

		private object ReadColor()
		{
			return Color.FromArgb(_input.ReadInt32());
		}
		private object ReadEnum()
		{
			return _input.ReadInt32();
		}

		private object ReadCollectionNameValue(Type type)
		{
			var count = _input.ReadUInt16();
			var nameValue = (NameValueCollection)_reflection.CreateInstance(type);
			var strType = typeof(string);
			for (int i = 0; i < count; i++)
			{
				var name = ReadMember(strType) as string;
				var val = ReadMember(strType) as string;
				nameValue.Add(name, val);
			}
			return nameValue;
		}


		private Array ReadArray(Type type)
		{
			var count = _input.ReadUInt16();
			var itemType = type.GetElementType();
			if (itemType == null)
				throw new InvalidDataException("Unknown 'Object' array type is not supported.\n" + type);

			var arr = ReflectionHelper.CreateArray(itemType, count);
			var lst = arr as IList;
			for (int i = 0; i < count; i++)
			{
				var val = ReadMember(itemType);
				lst[i] = val;
			}
			return arr;
		}

		private IList ReadGenericList(Type type)
		{
			var count = _input.ReadUInt16();
			var listObj = (IList)_reflection.CreateInstance(type);

			var itemType = type.GetGenericArguments()[0];
			for (int i = 0; i < count; i++)
			{
				var val = ReadMember(itemType);
				listObj.Add(val);
			}

			return listObj;
		}

		private IList ReadIListImpl(Type type)
		{
			var count = _input.ReadUInt16();
			var listObj = (IList)_reflection.CreateInstance(type);

			var itemType = type.GetElementType();
			if (itemType == null)
				throw new InvalidDataException("Unknown ICollection implementation is not supported.\n" + type.ToString());

			for (int i = 0; i < count; i++)
			{
				var val = ReadMember(itemType);
				listObj.Add(val);
			}

			return listObj;
		}
		private byte[] ReadBytes()
		{
			var length = _input.ReadInt32();
			return _input.ReadBytes(length);
		}

		private DataTable ReadDataTable()
		{
			var dt = (DataTable)_reflection.CreateInstance(typeof(DataTable));
			//var name = ReadString();
			var schema = ReadString();
			//dt.TableName = name;
			SetXmlSchema(dt, schema);

			var cols = dt.Columns;
			var rowCount = _input.ReadInt32();
			for (int i = 0; i < rowCount; i++)
			{
				var row = dt.Rows.Add();
				ReadDataRow(row, cols);
			}
			return dt;
		}

		private void ReadDataRow(DataRow row, DataColumnCollection cols)
		{
			var itemCount = _input.ReadInt32();
			var colCount = cols.Count;
			var passedIndex = 0;
			while (passedIndex < colCount && passedIndex < itemCount)
			{
				passedIndex++;

				var colIndex = _input.ReadInt32();
				var col = cols[colIndex];
				var val = ReadMember(col.DataType);
				row[col] = val;
			}

		}

		private object ReadDataset(Type memType)
		{
			var count = _input.ReadInt32();
			var ds = _reflection.CreateInstance(memType) as DataSet;
			for (int i = 0; i < count; i++)
			{
				var dt = ReadDataTable();
				ds.Tables.Add(dt);
			}
			return ds;
		}
		private void SetXmlSchema(DataTable dt, string schema)
		{
			using (var reader = new StringReader(schema))
			{
				dt.ReadXmlSchema(reader);
			}
		}

		private object ReadDictionary(Type memType)
		{
			var count = _input.ReadInt32();
			var dic = _reflection.CreateInstance(memType) as IDictionary;

			var genericType = memType.GetGenericArguments();
			var keyType = genericType[0];
			var valType = genericType[1];

			for (int i = 0; i < count; i++)
			{
				var key = ReadMember(keyType);
				var val = ReadMember(valType);
				dic.Add(key, val);
			}
			return dic;
		}

		private object ReadStringDictionary(Type memType)
		{
			var count = _input.ReadInt32();
			var dic = _reflection.CreateInstance(memType) as IDictionary;

			var genericType = memType.GetGenericArguments();
			var keyType = genericType[0];
			var valType = genericType[1];

			for (int i = 0; i < count; i++)
			{
				var key = ReadMember(keyType);
				var val = ReadMember(valType);
				dic.Add(key, val);
			}
			return dic;
		}

		private object ReadTimeSpan()
		{
			var ticks = _input.ReadInt64();
			return new TimeSpan(ticks);
		}
		private object ReadVersion()
		{
			return new Version(ReadString());
		}

		private object ReadDateTime()
		{
			var ticks = _input.ReadInt64();
			if (_useUtcDateTime)
				return new DateTime(ticks, DateTimeKind.Utc);
			return new DateTime(ticks);
		}

		private object ReadBoolean()
		{
			return _input.ReadByte() != 0;
		}

		private string ReadString()
		{
			var strBuff = ReadBytes();
			return Encoding.GetString(strBuff, 0, strBuff.Length);
		}

		private object ReadGuid()
		{
			var gbuff = ReadBytes();
			return new Guid(gbuff);
		}

		private object ReadChar()
		{
			return _input.ReadChar();
		}
		#endregion

	}
}
