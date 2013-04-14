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
	/// Salar.Bois serializer.
	/// Which provides binary serialization and deserialzation for .NET objects.
	/// BOIS stands for 'Binary Object Indexed Serialization'.
	/// </summary>
	/// <Author>
	/// Salar Khalilzadeh
	/// </Author>
	public class BoisSerializer
	{
		private BinaryWriter _serializeOut;
		private int _serializeDepth;
		private readonly BoisTypeCache _typeCache = new BoisTypeCache();
		private BinaryReader _input;

		/// <summary>
		/// Character encoding for strings.
		/// </summary>
		public Encoding Encoding { get; set; }

		/// <summary>
		/// Initializing a new instance of Bois serializar.
		/// </summary>
		public BoisSerializer()
		{
			Encoding = Encoding.UTF8;
		}

		/// <summary>
		/// Serializing an object to binary bois format.
		/// </summary>
		/// <param name="obj">The object to be serialized.</param>
		/// <param name="output">The output of the serialization in binary.</param>
		/// <typeparam name="T">The object type.</typeparam>
		public void Serialize<T>(T obj, Stream output)
		{
			if (obj == null)
				throw new ArgumentNullException("obj", "Object cannot be null.");
			_serializeDepth = 0;
			_serializeOut = new BinaryWriter(output, Encoding);

			WriteValue(obj, typeof(T));
		}

		/// <summary>
		/// Deserilizing binary data to a new instance.
		/// </summary>
		/// <param name="objectData">The binary data.</param>
		/// <typeparam name="T">The object type.</typeparam>
		/// <returns>New instance of the deserialized data.</returns>
		public T Deserialize<T>(Stream objectData)
		{
			_input = new BinaryReader(objectData, Encoding);
			return (T)ReadMember(typeof(T));
		}

		/// <summary>
		/// Deserilizing binary data to a new instance.
		/// </summary>
		/// <param name="objectBuffer">The binary data.</param>
		/// <param name="index">The index in buffer at which the stream begins.</param>
		/// <param name="count">The length of the stream in bytes.</param>
		/// <typeparam name="T">The object type.</typeparam>
		/// <returns>New instance of the deserialized data.</returns>
		public T Deserialize<T>(byte[] objectBuffer, int index, int count)
		{
			using (var mem = new MemoryStream(objectBuffer, index, count, false))
			{
				_input = new BinaryReader(mem, Encoding);
				return (T)ReadMember(typeof(T));
			}
		}

		/// <summary>
		/// Removes all cached information about types.
		/// </summary>
		public void ClearCache()
		{
			_typeCache.ClearCache();
		}

		/// <summary>
		/// Reads type information and caches it.
		/// </summary>
		/// <typeparam name="T">The object type.</typeparam>
		public void Initialize<T>()
		{
			_typeCache.Initialize<T>();
		}

		/// <summary>
		/// Reads type information and caches it.
		/// </summary>
		/// <param name="types">The objects types.</param>
		public void Initialize(params Type[] types)
		{
			_typeCache.Initialize(types);
		}


		#region Serialization methods

		private void WriteObject(object obj)
		{
			if (obj == null)
			{
				// number of writable members count is null means the value is null too
				// Int32? nullable
				PrimitivesConvertion.WriteVarInt(_serializeOut, (int?)null);
				return;
			}

			_serializeDepth++;
			var type = obj.GetType();

			var boisType = _typeCache.GetTypeInfo(type, true) as BoisTypeInfo;

			// number of writable members count
			// Int32? nullable
			PrimitivesConvertion.WriteVarInt(_serializeOut, (int?)boisType.Members.Length);

			// writing the members
			for (int i = 0; i < boisType.Members.Length; i++)
			{
				var mem = boisType.Members[i];
				if (mem.MemberType == EnBoisMemberType.Property)
				{
					var value = mem.PropertyGetter(obj);
					WriteValue(mem, value);
				}
				else if (mem.MemberType == EnBoisMemberType.Field)
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
			var bionType = _typeCache.GetTypeInfo(type, true);
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
			var bionType = _typeCache.GetTypeInfo(objType, true);

			WriteValue(bionType, value);
		}

		void WriteValue(BoisMemberInfo boisMemInfo, object value)
		{
			if (!boisMemInfo.IsSupportedPrimitive && !boisMemInfo.IsContainerObject)
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
				case EnBoisKnownType.Unknown:

					if (boisMemInfo.IsContainerObject)
					{
						WriteObject(value);
					}
					else if (boisMemInfo.IsStringDictionary)
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
					break;

				case EnBoisKnownType.Int16:
					if (value == null || boisMemInfo.IsNullable)
					{
						PrimitivesConvertion.WriteVarInt(_serializeOut, (short?)value);
					}
					else
					{
						PrimitivesConvertion.WriteVarInt(_serializeOut, (short)value);
					}
					break;

				case EnBoisKnownType.Int32:
					if (value == null || boisMemInfo.IsNullable)
					{
						PrimitivesConvertion.WriteVarInt(_serializeOut, (int?)value);
					}
					else
					{
						PrimitivesConvertion.WriteVarInt(_serializeOut, (int)value);
					}

					break;

				case EnBoisKnownType.Int64:
					if (value == null || boisMemInfo.IsNullable)
					{
						PrimitivesConvertion.WriteVarInt(_serializeOut, (long?)value);
					}
					else
					{
						PrimitivesConvertion.WriteVarInt(_serializeOut, (long)value);
					}
					break;

				case EnBoisKnownType.UInt16:
					_serializeOut.Write((ushort)value);
					break;

				case EnBoisKnownType.UInt32:
					_serializeOut.Write((uint)value);
					break;

				case EnBoisKnownType.UInt64:
					_serializeOut.Write((ulong)value);
					break;

				case EnBoisKnownType.Double:
					_serializeOut.Write((double)value);
					break;

				case EnBoisKnownType.Decimal:
#if SILVERLIGHT
					WriteDecimal(_serializeOut, (decimal)value);
#else
					_serializeOut.Write((decimal)value);
#endif
					break;

				case EnBoisKnownType.Single:
					_serializeOut.Write((float)value);
					break;

				case EnBoisKnownType.Byte:
					_serializeOut.Write((byte)value);
					break;

				case EnBoisKnownType.SByte:
					_serializeOut.Write((sbyte)value);
					break;

				case EnBoisKnownType.ByteArray:
					WriteBytes((byte[])value);
					break;

				case EnBoisKnownType.String:
					WriteString(value as string);
					break;

				case EnBoisKnownType.Char:
					_serializeOut.Write((char)value);
					break;

				case EnBoisKnownType.Guid:
					WriteGuid((Guid)value);
					break;

				case EnBoisKnownType.Bool:
					_serializeOut.Write((byte)(((bool)value) ? 1 : 0));
					break;

				case EnBoisKnownType.Enum:
					WriteEnum((Enum)value);
					break;

				case EnBoisKnownType.DateTime:
					WriteDateTime((DateTime)value);
					break;

				case EnBoisKnownType.TimeSpan:
					WriteTimeSpan((TimeSpan)value);
					break;

#if !SILVERLIGHT
				case EnBoisKnownType.DataSet:
					WriteDataset(value as DataSet);
					break;

				case EnBoisKnownType.DataTable:
					WriteDataTable(value as DataTable);
					break;
				case EnBoisKnownType.NameValueColl:
					WriteCollectionNameValue(value as NameValueCollection);
					break;

				case EnBoisKnownType.Color:
					WriteColor((Color)value);
					break;
#endif

				case EnBoisKnownType.Version:
					WriteVersion(value as Version);
					break;

				case EnBoisKnownType.DbNull:
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
			//Int32
			var binaryMemberCount = PrimitivesConvertion.ReadVarInt32Nullable(_input);
			if (binaryMemberCount == null)
			{
				return null;
			}

			var bionType = _typeCache.GetTypeInfo(type, true) as BoisTypeInfo;

			var members = bionType.Members;
			var resultObj = _typeCache.CreateInstance(type);
			ReadMembers(resultObj, members, binaryMemberCount.Value);
			return resultObj;
		}

		private void ReadMembers(object obj, BoisMemberInfo[] memberList, int binaryMemberCount)
		{
			var objectMemberCount = memberList.Length;
			var memberProcessed = 0;
			var dataLeng = _input.BaseStream.Length;
			var data = _input.BaseStream;

			//var objType = obj.GetType();

			// while all members are processed
			while (memberProcessed < binaryMemberCount &&
				   memberProcessed < objectMemberCount &&
				   data.Position < dataLeng)
			{
				// the member from member list according to the index
				var memInfo = memberList[memberProcessed];
				memberProcessed++;

				// set the value
				if (memInfo.MemberType == EnBoisMemberType.Property)
				{
					var pinfo = memInfo.Info as PropertyInfo;

					// read the value
					var value = ReadMember(memInfo, pinfo.PropertyType);

					// using the setter
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
			var memInfo = _typeCache.GetTypeInfo(memType, true);
			return ReadMember(memInfo, memType);
		}

		private object ReadMember(BoisMemberInfo memInfo, Type memType)
		{
			if (memInfo.IsNullable &&
				!memInfo.IsSupportedPrimitive &&
				!memInfo.IsContainerObject)
			{
				bool isNull = _input.ReadByte() != 0;

				if (isNull)
				{
					return null;
				}
			}
			var actualMemberType = memType;
			if (memInfo.IsNullable && memInfo.NullableUnderlyingType != null)
			{
				actualMemberType = memInfo.NullableUnderlyingType;
			}

			switch (memInfo.KnownType)
			{
				case EnBoisKnownType.Unknown:

					if (memInfo.IsContainerObject)
					{
						return ReadObject(actualMemberType);
					}
					else if (memInfo.IsStringDictionary)
					{
						return ReadStringDictionary(actualMemberType);
					}
					else if (memInfo.IsDictionary)
					{
						return ReadDictionary(actualMemberType);
					}
					else if (memInfo.IsCollection)
					{
						if (memInfo.IsGeneric)
						{
							return ReadGenericList(actualMemberType);
						}
						return ReadArray(actualMemberType);
					}
					else if (memInfo.IsArray)
					{
						return ReadArray(actualMemberType);
					}

					break;

				case EnBoisKnownType.Int16:
					if (memInfo.IsNullable)
					{
						return PrimitivesConvertion.ReadVarInt16Nullable(_input);
					}
					return PrimitivesConvertion.ReadVarInt16(_input);

				case EnBoisKnownType.Int32:
					if (memInfo.IsNullable)
					{
						return PrimitivesConvertion.ReadVarInt32Nullable(_input);
					}
					return PrimitivesConvertion.ReadVarInt32(_input);

				case EnBoisKnownType.Int64:
					if (memInfo.IsNullable)
					{
						return PrimitivesConvertion.ReadVarInt64Nullable(_input);
					}
					return PrimitivesConvertion.ReadVarInt64(_input);

				case EnBoisKnownType.UInt16:
					return _input.ReadUInt16();

				case EnBoisKnownType.UInt32:
					return _input.ReadUInt32();

				case EnBoisKnownType.UInt64:
					return _input.ReadUInt64();

				case EnBoisKnownType.Double:
					return _input.ReadDouble();

				case EnBoisKnownType.Decimal:
#if SILVERLIGHT
					return ReadDecimal(_input);
#else
					return _input.ReadDecimal();
#endif

				case EnBoisKnownType.Single:
					return _input.ReadSingle();

				case EnBoisKnownType.Byte:
					return _input.ReadByte();

				case EnBoisKnownType.SByte:
					return _input.ReadSByte();

				case EnBoisKnownType.ByteArray:
					return ReadBytes();

				case EnBoisKnownType.String:
					return ReadString();

				case EnBoisKnownType.Char:
					return _input.ReadChar();

				case EnBoisKnownType.Guid:
					return ReadGuid();

				case EnBoisKnownType.Bool:
					return ReadBoolean();

				case EnBoisKnownType.Enum:
					return ReadEnum(actualMemberType);

				case EnBoisKnownType.DateTime:
					return ReadDateTime();

				case EnBoisKnownType.TimeSpan:
					return ReadTimeSpan();

#if !SILVERLIGHT
				case EnBoisKnownType.DataSet:
					return ReadDataset(actualMemberType);

				case EnBoisKnownType.DataTable:
					return ReadDataTable();

				case EnBoisKnownType.NameValueColl:
					return ReadCollectionNameValue(actualMemberType);

				case EnBoisKnownType.Color:
					return ReadColor();
#endif

				case EnBoisKnownType.Version:
					return ReadVersion();

				case EnBoisKnownType.DbNull:
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

			var listObj = (IList)_typeCache.CreateInstance(type);

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

			var listObj = (IList)_typeCache.CreateInstance(type);

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
			var nameValue = (NameValueCollection)_typeCache.CreateInstance(type);
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
			var dt = _typeCache.CreateInstance(typeof(DataTable)) as DataTable;

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
			var ds = _typeCache.CreateInstance(memType) as DataSet;
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
			var dic = _typeCache.CreateInstance(memType) as IDictionary;

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
			var dic = _typeCache.CreateInstance(memType) as IDictionary;

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
