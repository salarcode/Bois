using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
#if !SILVERLIGHT
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
#endif

/* 
 * Salar BOIS (Binary Object Indexed Serialization)
 * by Salar Khalilzadeh
 * 
 * https://bois.codeplex.com/
 * Mozilla Public License v2
 */
namespace Salar.Bois
{
	/// <summary>
	/// Salar.Bois Serializer. BOIS stands for 'Binary Object Indexed Serialization'.
	/// </summary>
	/// <Author>
	/// Salar Khalilzadeh
	/// </Author>
	public class BoisSerializer
	{
		private BinaryWriter _serializeOut;
		private int _serializeDepth;
		private readonly ReflectionCache _reflection = new ReflectionCache();
		private BinaryReader _input;

		public Encoding Encoding { get; set; }

		public BoisSerializer()
		{
			Encoding = Encoding.UTF8;
		}

		public void Serialize<T>(T obj, Stream output)
		{
			if (obj == null)
				throw new ArgumentNullException("obj", "Object cannot be null.");
			_serializeDepth = 0;
			_serializeOut = new BinaryWriter(output, Encoding);

			WriteValue(obj, typeof(T));
		}

		public T Deserialize<T>(Stream objectData)
		{
			_input = new BinaryReader(objectData, Encoding);
			return (T)ReadMember(typeof(T));
		}

		public T Deserialize<T>(byte[] objectBuffer, int index, int count)
		{
			using (var mem = new MemoryStream(objectBuffer, index, count, false))
			{
				_input = new BinaryReader(mem, Encoding);
				return (T)ReadMember(typeof(T));
			}
		}

		public void ClearCache()
		{
			BoisTypeCache.ClearCache();
		}

		public static void Initialize<T>()
		{
			BoisTypeCache.Initialize<T>();
		}

		public static void Initialize(params Type[] types)
		{
			BoisTypeCache.Initialize(types);
		}


		#region Serialization methods

		private void WriteObject(object obj)
		{
			_serializeDepth++;
			var type = obj.GetType();

			var boisType = BoisTypeCache.GetTypeInfo(type, true) as BoisTypeCache.BoisTypeInfo;

			// number of writable members count
			// Int32
			PrimitivesConvertion.WriteVarInt(_serializeOut, boisType.Members.Length);

			// writing the members
			for (int i = 0; i < boisType.Members.Length; i++)
			{
				var mem = boisType.Members[i];
				if (mem.MemberType == BoisTypeCache.EnBoisMemberType.Property)
				{
					var value = mem.PropertyGetter(obj);
					WriteValue(mem, value);
				}
				else if (mem.MemberType == BoisTypeCache.EnBoisMemberType.Field)
				{
					var finfo = (FieldInfo)mem.Info;
					var value = finfo.GetValue(obj);
					WriteValue(mem, value);
				}
			}
			_serializeDepth--;
		}

		private void WriteNullableType(bool isnull)
		{
			_serializeOut.Write(isnull ? (byte)1 : (byte)0);
		}

		/// <summary>
		/// Also called by root
		/// </summary>
		void WriteValue(object value, Type type)
		{
			var bionType = BoisTypeCache.GetTypeInfo(type, true);
			if (!bionType.IsSupportedPrimitive)
			{
				if (value == null)
				{
					WriteNullableType(true);
					return;
				}
			}
			WriteValue(bionType, value);
		}

		void WriteValue(object value)
		{
			if (value == null)
			{
				WriteNullableType(true);
				return;
			}

			var objType = value.GetType();
			var bionType = BoisTypeCache.GetTypeInfo(objType, true);

			WriteValue(bionType, value);
		}

		void WriteValue(BoisTypeCache.BoisMemberInfo boisMemInfo, object value)
		{
			if (!boisMemInfo.IsSupportedPrimitive)
			{
				if (value == null)
				{
					WriteNullableType(true);
					return;
				}
				else if (boisMemInfo.IsNullable)
				{
					WriteNullableType(false);
				}
			}

			switch (boisMemInfo.KnownType)
			{
				case BoisTypeCache.EnBoisKnownType.Unknown:

					if (boisMemInfo.IsStringDictionary)
					{
						WriteStringDictionary(value as IDictionary);
					}
					else if (boisMemInfo.IsDictionary)
					{
						WriteDictionary(value as IDictionary);
					}
					else if (boisMemInfo.IsCollection || boisMemInfo.IsArray)
					{
						if (boisMemInfo.IsGeneric)
						{
							WriteGenericList(value as IEnumerable);
						}
						else
						{
							WriteArray(value as IEnumerable);
						}
					}
					else
					{
						WriteObject(value);
					}
					break;

				case BoisTypeCache.EnBoisKnownType.Int16:
					if (value == null || boisMemInfo.IsNullable)
					{
						PrimitivesConvertion.WriteVarInt(_serializeOut, (short?)value);
					}
					else
					{
						PrimitivesConvertion.WriteVarInt(_serializeOut, (short)value);
					}
					break;

				case BoisTypeCache.EnBoisKnownType.Int32:
					if (value == null || boisMemInfo.IsNullable)
					{
						PrimitivesConvertion.WriteVarInt(_serializeOut, (int?)value);
					}
					else
					{
						PrimitivesConvertion.WriteVarInt(_serializeOut, (int)value);
					}

					break;

				case BoisTypeCache.EnBoisKnownType.Int64:
					if (value == null || boisMemInfo.IsNullable)
					{
						PrimitivesConvertion.WriteVarInt(_serializeOut, (long?)value);
					}
					else
					{
						PrimitivesConvertion.WriteVarInt(_serializeOut, (long)value);
					}
					break;

				case BoisTypeCache.EnBoisKnownType.UInt16:
					_serializeOut.Write((ushort)value);
					break;

				case BoisTypeCache.EnBoisKnownType.UInt32:
					_serializeOut.Write((uint)value);
					break;

				case BoisTypeCache.EnBoisKnownType.UInt64:
					_serializeOut.Write((ulong)value);
					break;

				case BoisTypeCache.EnBoisKnownType.Double:
					_serializeOut.Write((double)value);
					break;

				case BoisTypeCache.EnBoisKnownType.Decimal:
#if SILVERLIGHT
					WriteDecimal(_serializeOut, (decimal)value);
#else
					_serializeOut.Write((decimal)value);
#endif
					break;

				case BoisTypeCache.EnBoisKnownType.Single:
					_serializeOut.Write((float)value);
					break;

				case BoisTypeCache.EnBoisKnownType.Byte:
					_serializeOut.Write((byte)value);
					break;

				case BoisTypeCache.EnBoisKnownType.SByte:
					_serializeOut.Write((sbyte)value);
					break;

				case BoisTypeCache.EnBoisKnownType.ByteArray:
					WriteBytes((byte[])value);
					break;

				case BoisTypeCache.EnBoisKnownType.String:
					WriteString(value as string);
					break;

				case BoisTypeCache.EnBoisKnownType.Char:
					_serializeOut.Write((char)value);
					break;

				case BoisTypeCache.EnBoisKnownType.Guid:
					WriteGuid((Guid)value);
					break;

				case BoisTypeCache.EnBoisKnownType.Bool:
					_serializeOut.Write((byte)(((bool)value) ? 1 : 0));
					break;

				case BoisTypeCache.EnBoisKnownType.Enum:
					WriteEnum((Enum)value);
					break;

				case BoisTypeCache.EnBoisKnownType.DateTime:
					WriteDateTime((DateTime)value);
					break;

				case BoisTypeCache.EnBoisKnownType.TimeSpan:
					WriteTimeSpan((TimeSpan)value);
					break;

#if !SILVERLIGHT
				case BoisTypeCache.EnBoisKnownType.DataSet:
					WriteDataset(value as DataSet);
					break;

				case BoisTypeCache.EnBoisKnownType.DataTable:
					WriteDataTable(value as DataTable);
					break;
				case BoisTypeCache.EnBoisKnownType.NameValueColl:
					WriteCollectionNameValue(value as NameValueCollection);
					break;

				case BoisTypeCache.EnBoisKnownType.Color:
					WriteColor((Color)value);
					break;
#endif

				case BoisTypeCache.EnBoisKnownType.Version:
					WriteVersion(value as Version);
					break;

				case BoisTypeCache.EnBoisKnownType.DbNull:
					// Do not write anything, it is already written as Nullable object. //WriteNullableType(true);
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}


#if !SILVERLIGHT
		private void WriteCollectionNameValue(NameValueCollection nameValue)
		{
			// Int32
			PrimitivesConvertion.WriteVarInt(_serializeOut, nameValue.Count);

			foreach (string key in nameValue)
			{
				WriteValue(key);
				WriteValue(nameValue[key]);
			}
		}

		private void WriteColor(Color c)
		{
			int argb = c.ToArgb();
			// Int32
			PrimitivesConvertion.WriteVarInt(_serializeOut, argb);
		}

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
			// Int32
			PrimitivesConvertion.WriteVarInt(_serializeOut, ds.Tables.Count);

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

			// Int32
			PrimitivesConvertion.WriteVarInt(_serializeOut, table.Rows.Count);

			var colsCount = table.Columns.Count;
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
			// Int32
			PrimitivesConvertion.WriteVarInt(_serializeOut, values.Count);

			foreach (var value in values)
			{
				// Int32
				PrimitivesConvertion.WriteVarInt(_serializeOut, value.Key);

				WriteValue(value.Value);
			}
		}
#endif

		private void WriteEnum(Enum e)
		{
			// Int32
			PrimitivesConvertion.WriteVarInt(_serializeOut, (int)((object)e));
		}

		private void WriteGenericList(IEnumerable array)
		{
			int count = 0;
			var col = array as ICollection;
			if (col != null)
				count = (int)col.Count;
			else
			{
				foreach (object obj in array)
					count++;
			}
			var itemType = array.GetType().GetGenericArguments()[0];
			// Int32
			PrimitivesConvertion.WriteVarInt(_serializeOut, count);
			foreach (object obj in array)
			{
				WriteValue(obj, itemType);
			}
		}
		private void WriteArray(IEnumerable array)
		{
			int count = 0;
			var col = array as ICollection;
			if (col != null)
				count = (int)col.Count;
			else
			{
				foreach (object obj in array)
					count++;
			}

			// Int32
			PrimitivesConvertion.WriteVarInt(_serializeOut, count);
			foreach (object obj in array)
			{
				WriteValue(obj);
			}
		}

		private void WriteBytes(byte[] bytes)
		{
			// Int32
			PrimitivesConvertion.WriteVarInt(_serializeOut, bytes.Length);
			_serializeOut.Write(bytes);
		}

		private void WriteDictionary(IDictionary dic)
		{
			// Int32
			PrimitivesConvertion.WriteVarInt(_serializeOut, dic.Count);

			var genericType = dic.GetType().GetGenericArguments();
			var keyType = genericType[0];
			var valType = genericType[1];

			foreach (DictionaryEntry entry in dic)
			{
				WriteValue(entry.Key, keyType);
				WriteValue(entry.Value, valType);
			}
		}

		private void WriteStringDictionary(IDictionary dic)
		{
			// Int32
			PrimitivesConvertion.WriteVarInt(_serializeOut, dic.Count);

			var genericType = dic.GetType().GetGenericArguments();
			var keyType = typeof(string);
			var valType = genericType[1];

			foreach (DictionaryEntry entry in dic)
			{
				WriteValue(entry.Key.ToString(), keyType);
				WriteValue(entry.Value, valType);
			}
		}

		[Obsolete]
		private void WritePairString(string name, object value)
		{
			WriteString(name);
			WriteValue(value);
		}

		[Obsolete]
		private void WritePairObject(object key, object value)
		{
			WriteValue(key);
			WriteValue(value);
		}

		private void WriteDateTime(DateTime dateTime)
		{
			var dt = dateTime;
			if (dt == DateTime.MinValue || dt == DateTime.MaxValue)
			{
				PrimitivesConvertion.WriteVarInt(_serializeOut, dt.Ticks);
			}
			else
			{
				if (dt.Kind != DateTimeKind.Utc)
				{
					dt = dt.ToUniversalTime();
				}
				//Int64
				PrimitivesConvertion.WriteVarInt(_serializeOut, dt.Ticks);
			}
		}

		private void WriteTimeSpan(TimeSpan timeSpan)
		{
			// Int64
			PrimitivesConvertion.WriteVarInt(_serializeOut, timeSpan.Ticks);
		}
		private void WriteVersion(Version version)
		{
			WriteString(version.ToString());
		}

		private void WriteString(string str)
		{
			if (str == null)
			{
				PrimitivesConvertion.WriteVarInt(_serializeOut, (int?)null);
			}
			else if (str.Length == 0)
			{
				PrimitivesConvertion.WriteVarInt(_serializeOut, (int?)0);
			}
			else
			{
				var strBytes = Encoding.GetBytes(str);
				// Int32
				PrimitivesConvertion.WriteVarInt(_serializeOut, (int?)strBytes.Length);
				_serializeOut.Write(strBytes);
			}
		}

		[Obsolete]
		private void WriteString_OLD(string str)
		{
			WriteBytes(Encoding.GetBytes(str));
		}
		[Obsolete]
		private void WriteString_OLD(string str, bool checkNull)
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
				// Int32
				PrimitivesConvertion.WriteVarInt(_serializeOut, (int)0);
			}
			else
			{
				WriteBytes(Encoding.GetBytes(str));
			}
		}

		private void WriteGuid(Guid g)
		{
			if (g == Guid.Empty)
			{
				// Int32
				PrimitivesConvertion.WriteVarInt(_serializeOut, 0);
				return;
			}

			var data = g.ToByteArray();
			// Int32
			PrimitivesConvertion.WriteVarInt(_serializeOut, data.Length);
			_serializeOut.Write(data);
		}
		#endregion

		#region Deserialization methods

		private object ReadObject(Type type)
		{
			var bionType = BoisTypeCache.GetTypeInfo(type, true) as BoisTypeCache.BoisTypeInfo;

			var members = bionType.Members;
			var resultObj = _reflection.CreateInstance(type);
			ReadMembers(resultObj, members);
			return resultObj;
		}

		private void ReadMembers(object obj, BoisTypeCache.BoisMemberInfo[] memberList)
		{
			//Int32
			var binaryMemberCount = PrimitivesConvertion.ReadVarInt32(_input);
			var objectMemberCount = memberList.Length;
			var memberProcessed = 0;
			var dataLeng = _input.BaseStream.Length;
			var data = _input.BaseStream;

			var objType = obj.GetType();

			// while all members are processed
			while (memberProcessed < binaryMemberCount &&
				   memberProcessed < objectMemberCount &&
				   data.Position < dataLeng)
			{
				// the member from member list according to the index
				var memInfo = memberList[memberProcessed];
				memberProcessed++;

				// set the value
				if (memInfo.MemberType == BoisTypeCache.EnBoisMemberType.Property)
				{
					var pinfo = memInfo.Info as PropertyInfo;

					// read the value
					var value = ReadMember(memInfo, pinfo.PropertyType);

					////memInfo.PropertySetter(obj, value);
					//if (objType.IsValueType)
					//	pinfo.SetValue(obj, value, null);
					//else
					//{
					//	memInfo.PropertySetter(obj, value);
					//}
					memInfo.PropertySetter(obj, value);
				}
				else
				{
					var finfo = memInfo.Info as FieldInfo;

					// read the value
					var value = ReadMember(memInfo, finfo.FieldType);

					ReflectionHelper.SetValue(obj, value, finfo);
				}

			}
		}

		private object ReadMember(Type memType)
		{
			var memInfo = BoisTypeCache.GetTypeInfo(memType, true);
			return ReadMember(memInfo, memType);
		}

		private object ReadMember(BoisTypeCache.BoisMemberInfo memInfo, Type memType)
		{
			if (memInfo.IsNullable && !memInfo.IsSupportedPrimitive)
			{
				bool isNull = _input.ReadByte() != 0;

				if (isNull)
				{
					return null;
				}
			}
			switch (memInfo.KnownType)
			{
				case BoisTypeCache.EnBoisKnownType.Unknown:

					if (memInfo.IsStringDictionary)
					{
						return ReadStringDictionary(memType);
					}
					else if (memInfo.IsDictionary)
					{
						return ReadDictionary(memType);
					}
					else if (memInfo.IsCollection)
					{
						if (memInfo.IsGeneric)
						{
							return ReadGenericList(memType);
						}
						return ReadArray(memType);
					}
					else if (memInfo.IsArray)
					{
						return ReadArray(memType);
					}
					else
					{
						return ReadObject(memType);
					}
					break;

				case BoisTypeCache.EnBoisKnownType.Int16:
					if (memInfo.IsNullable)
					{
						return PrimitivesConvertion.ReadVarInt16Nullable(_input);
					}
					return PrimitivesConvertion.ReadVarInt16(_input);

				case BoisTypeCache.EnBoisKnownType.Int32:
					if (memInfo.IsNullable)
					{
						return PrimitivesConvertion.ReadVarInt32Nullable(_input);
					}
					return PrimitivesConvertion.ReadVarInt32(_input);

				case BoisTypeCache.EnBoisKnownType.Int64:
					if (memInfo.IsNullable)
					{
						return PrimitivesConvertion.ReadVarInt64Nullable(_input);
					}
					return PrimitivesConvertion.ReadVarInt64(_input);

				case BoisTypeCache.EnBoisKnownType.UInt16:
					return _input.ReadUInt16();

				case BoisTypeCache.EnBoisKnownType.UInt32:
					return _input.ReadUInt32();

				case BoisTypeCache.EnBoisKnownType.UInt64:
					return _input.ReadUInt64();

				case BoisTypeCache.EnBoisKnownType.Double:
					return _input.ReadDouble();

				case BoisTypeCache.EnBoisKnownType.Decimal:
#if SILVERLIGHT
					return ReadDecimal(_input);
#else
					return _input.ReadDecimal();
#endif

				case BoisTypeCache.EnBoisKnownType.Single:
					return _input.ReadSingle();

				case BoisTypeCache.EnBoisKnownType.Byte:
					return _input.ReadByte();

				case BoisTypeCache.EnBoisKnownType.SByte:
					return _input.ReadSByte();

				case BoisTypeCache.EnBoisKnownType.ByteArray:
					return ReadBytes();

				case BoisTypeCache.EnBoisKnownType.String:
					return ReadString();

				case BoisTypeCache.EnBoisKnownType.Char:
					return _input.ReadChar();

				case BoisTypeCache.EnBoisKnownType.Guid:
					return ReadGuid();

				case BoisTypeCache.EnBoisKnownType.Bool:
					return ReadBoolean();

				case BoisTypeCache.EnBoisKnownType.Enum:
					return ReadEnum(memType);

				case BoisTypeCache.EnBoisKnownType.DateTime:
					return ReadDateTime();

				case BoisTypeCache.EnBoisKnownType.TimeSpan:
					return ReadTimeSpan();

#if !SILVERLIGHT
				case BoisTypeCache.EnBoisKnownType.DataSet:
					return ReadDataset(memType);

				case BoisTypeCache.EnBoisKnownType.DataTable:
					return ReadDataTable();

				case BoisTypeCache.EnBoisKnownType.NameValueColl:
					return ReadCollectionNameValue(memType);

				case BoisTypeCache.EnBoisKnownType.Color:
					return ReadColor();
#endif

				case BoisTypeCache.EnBoisKnownType.Version:
					return ReadVersion();

				case BoisTypeCache.EnBoisKnownType.DbNull:
					return DBNull.Value;

				default:
					throw new ArgumentOutOfRangeException();
			}
			return null;
		}

		private object ReadEnum(Type type)
		{
			var val = PrimitivesConvertion.ReadVarInt32(_input);
			return Enum.ToObject(type, val);
		}

		private Array ReadArray(Type type)
		{
			var count = PrimitivesConvertion.ReadVarInt32(_input);

			var itemType = type.GetElementType();
			if (itemType == null)
				throw new ArgumentException("Unknown 'Object' array type is not supported.\n" + type);

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
			var count = PrimitivesConvertion.ReadVarInt32(_input);

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
			var count = PrimitivesConvertion.ReadVarInt32(_input);

			var listObj = (IList)_reflection.CreateInstance(type);

			var itemType = type.GetElementType();
			if (itemType == null)
				throw new ArgumentException("Unknown ICollection implementation is not supported.\n" + type.ToString());

			for (int i = 0; i < count; i++)
			{
				var val = ReadMember(itemType);
				listObj.Add(val);
			}

			return listObj;
		}
		private byte[] ReadBytes()
		{
			var length = PrimitivesConvertion.ReadVarInt32(_input);
			return _input.ReadBytes(length);
		}

#if SILVERLIGHT
		decimal ReadDecimal(BinaryReader reader)
		{
			var bits = new int[4];
			bits[0] = reader.ReadInt32();
			bits[1] = reader.ReadInt32();
			bits[2] = reader.ReadInt32();
			bits[3] = reader.ReadInt32();
			return new decimal(bits);
		}

		private void WriteDecimal(BinaryWriter writer, decimal val)
		{
			var bits = decimal.GetBits(val);
			writer.Write(bits[0]);
			writer.Write(bits[1]);
			writer.Write(bits[2]);
			writer.Write(bits[3]);
		}
#else
		private object ReadColor()
		{
			return Color.FromArgb(PrimitivesConvertion.ReadVarInt32(_input));
		}
		private object ReadCollectionNameValue(Type type)
		{
			var count = PrimitivesConvertion.ReadVarInt32(_input);
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
		private DataTable ReadDataTable()
		{
			var dt = _reflection.CreateInstance(typeof(DataTable)) as DataTable;

			var schema = ReadString();
			//dt.TableName = name;
			SetXmlSchema(dt, schema);

			var cols = dt.Columns;

			var rowCount = PrimitivesConvertion.ReadVarInt32(_input);
			for (int i = 0; i < rowCount; i++)
			{
				var row = dt.Rows.Add();
				ReadDataRow(row, cols);
			}
			return dt;
		}

		private void ReadDataRow(DataRow row, DataColumnCollection cols)
		{
			var itemCount = PrimitivesConvertion.ReadVarInt32(_input);
			var colCount = cols.Count;
			var passedIndex = 0;
			while (passedIndex < colCount && passedIndex < itemCount)
			{
				passedIndex++;

				var colIndex = PrimitivesConvertion.ReadVarInt32(_input);
				var col = cols[colIndex];
				var val = ReadMember(col.DataType);
				row[col] = val ?? DBNull.Value;
			}

		}

		private object ReadDataset(Type memType)
		{
			var count = PrimitivesConvertion.ReadVarInt32(_input);
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
#endif

		private object ReadDictionary(Type memType)
		{
			var count = PrimitivesConvertion.ReadVarInt32(_input);
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
			var count = PrimitivesConvertion.ReadVarInt32(_input);
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

		private TimeSpan ReadTimeSpan()
		{
			var ticks = PrimitivesConvertion.ReadVarInt64(_input);
			return new TimeSpan(ticks);
		}
		private object ReadVersion()
		{
			return new Version(ReadString());
		}

		private object ReadDateTime()
		{
			var ticks = PrimitivesConvertion.ReadVarInt64(_input);
			if (ticks == DateTime.MinValue.Ticks || ticks == DateTime.MaxValue.Ticks)
			{
				return new DateTime(ticks);
			}

			return new DateTime(ticks, DateTimeKind.Utc).ToLocalTime();
		}

		private object ReadBoolean()
		{
			return _input.ReadByte() != 0;
		}

		private string ReadString()
		{
			int? length = PrimitivesConvertion.ReadVarInt32Nullable(_input);
			if (length == null)
			{
				return null;
			}
			else if (length == 0)
			{
				return string.Empty;
			}
			else
			{
				var strBuff = _input.ReadBytes(length.Value);
				return Encoding.GetString(strBuff, 0, strBuff.Length);
			}
		}

		private object ReadGuid()
		{
			var gbuff = ReadBytes();
			if (gbuff.Length == 0)
				return Guid.Empty;
			return new Guid(gbuff);
		}

		private object ReadChar()
		{
			return _input.ReadChar();
		}
		#endregion

	}
}
