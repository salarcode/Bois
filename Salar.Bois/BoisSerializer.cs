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
		private int _serializeDepth;
		private readonly BoisTypeCache _typeCache = new BoisTypeCache();

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
			var writer = new BinaryWriter(output, Encoding);

			WriteValue(writer, obj, typeof(T));
		}

		/// <summary>
		/// Deserilizing binary data to a new instance.
		/// </summary>
		/// <param name="objectData">The binary data.</param>
		/// <typeparam name="T">The object type.</typeparam>
		/// <returns>New instance of the deserialized data.</returns>
		public T Deserialize<T>(Stream objectData)
		{
			var reader = new BinaryReader(objectData, Encoding);
			return (T)ReadMember(reader, typeof(T));
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
				var reader = new BinaryReader(mem, Encoding);
				return (T)ReadMember(reader, typeof(T));
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

		private void WriteObject(BinaryWriter writer, object obj)
		{
			if (obj == null)
			{
				// number of writable members count is null means the value is null too
				// Int32? nullable
				PrimitivesConvertion.WriteVarInt(writer, (int?)null);
				return;
			}

			_serializeDepth++;
			var type = obj.GetType();

			var boisType = _typeCache.GetTypeInfo(type, true) as BoisTypeInfo;

			// number of writable members count
			// Int32? nullable
			PrimitivesConvertion.WriteVarInt(writer, (int?)boisType.Members.Length);

			// writing the members
			for (int i = 0; i < boisType.Members.Length; i++)
			{
				var mem = boisType.Members[i];
				if (mem.MemberType == EnBoisMemberType.Property)
				{
					var value = mem.PropertyGetter(obj);
					WriteValue(writer, mem, value);
				}
				else if (mem.MemberType == EnBoisMemberType.Field)
				{
					var finfo = (FieldInfo)mem.Info;
					var value = finfo.GetValue(obj);
					WriteValue(writer, mem, value);
				}
			}
			_serializeDepth--;
		}

		private void WriteNullableType(BinaryWriter writer, bool isnull)
		{
			writer.Write(isnull ? (byte)1 : (byte)0);
		}

		/// <summary>
		/// Also called by root
		/// </summary>
		void WriteValue(BinaryWriter writer, object value, Type type)
		{
			var bionType = _typeCache.GetTypeInfo(type, true);
			if (!bionType.IsSupportedPrimitive)
			{
				if (value == null)
				{
					WriteNullableType(writer, true);
					return;
				}
			}
			WriteValue(writer, bionType, value);
		}

		void WriteValue(BinaryWriter writer, object value)
		{
			if (value == null)
			{
				WriteNullableType(writer, true);
				return;
			}

			var objType = value.GetType();
			var bionType = _typeCache.GetTypeInfo(objType, true);

			WriteValue(writer, bionType, value);
		}

		void WriteValue(BinaryWriter writer, BoisMemberInfo boisMemInfo, object value)
		{
			if (!boisMemInfo.IsSupportedPrimitive && !boisMemInfo.IsContainerObject)
			{
				if (value == null)
				{
					WriteNullableType(writer, true);
					return;
				}
				else if (boisMemInfo.IsNullable)
				{
					WriteNullableType(writer, false);
				}
			}

			switch (boisMemInfo.KnownType)
			{
				case EnBoisKnownType.Unknown:

					if (boisMemInfo.IsContainerObject)
					{
						WriteObject(writer, value);
					}
					else if (boisMemInfo.IsStringDictionary)
					{
						WriteStringDictionary(writer, value as IDictionary);
					}
					else if (boisMemInfo.IsDictionary)
					{
						WriteDictionary(writer, value as IDictionary);
					}
					else if (boisMemInfo.IsCollection || boisMemInfo.IsArray)
					{
						if (boisMemInfo.IsGeneric)
						{
							WriteGenericList(writer, value as IEnumerable);
						}
						else
						{
							WriteArray(writer, value as IEnumerable);
						}
					}
					break;

				case EnBoisKnownType.Int16:
					if (value == null || boisMemInfo.IsNullable)
					{
						PrimitivesConvertion.WriteVarInt(writer, (short?)value);
					}
					else
					{
						PrimitivesConvertion.WriteVarInt(writer, (short)value);
					}
					break;

				case EnBoisKnownType.Int32:
					if (value == null || boisMemInfo.IsNullable)
					{
						PrimitivesConvertion.WriteVarInt(writer, (int?)value);
					}
					else
					{
						PrimitivesConvertion.WriteVarInt(writer, (int)value);
					}

					break;

				case EnBoisKnownType.Int64:
					if (value == null || boisMemInfo.IsNullable)
					{
						PrimitivesConvertion.WriteVarInt(writer, (long?)value);
					}
					else
					{
						PrimitivesConvertion.WriteVarInt(writer, (long)value);
					}
					break;

				case EnBoisKnownType.UInt16:
					writer.Write((ushort)value);
					break;

				case EnBoisKnownType.UInt32:
					writer.Write((uint)value);
					break;

				case EnBoisKnownType.UInt64:
					writer.Write((ulong)value);
					break;

				case EnBoisKnownType.Double:
					writer.Write((double)value);
					break;

				case EnBoisKnownType.Decimal:
#if SILVERLIGHT
					WriteDecimal(writer, (decimal)value);
#else
					writer.Write((decimal)value);
#endif
					break;

				case EnBoisKnownType.Single:
					writer.Write((float)value);
					break;

				case EnBoisKnownType.Byte:
					writer.Write((byte)value);
					break;

				case EnBoisKnownType.SByte:
					writer.Write((sbyte)value);
					break;

				case EnBoisKnownType.ByteArray:
					WriteBytes(writer, (byte[])value);
					break;

				case EnBoisKnownType.String:
					WriteString(writer, value as string);
					break;

				case EnBoisKnownType.Char:
					writer.Write((char)value);
					break;

				case EnBoisKnownType.Guid:
					WriteGuid(writer, (Guid)value);
					break;

				case EnBoisKnownType.Bool:
					writer.Write((byte)(((bool)value) ? 1 : 0));
					break;

				case EnBoisKnownType.Enum:
					WriteEnum(writer, (Enum)value);
					break;

				case EnBoisKnownType.DateTime:
					WriteDateTime(writer, (DateTime)value);
					break;

				case EnBoisKnownType.TimeSpan:
					WriteTimeSpan(writer, (TimeSpan)value);
					break;

#if !SILVERLIGHT
				case EnBoisKnownType.DataSet:
					WriteDataset(writer, value as DataSet);
					break;

				case EnBoisKnownType.DataTable:
					WriteDataTable(writer, value as DataTable);
					break;
				case EnBoisKnownType.NameValueColl:
					WriteCollectionNameValue(writer, value as NameValueCollection);
					break;

				case EnBoisKnownType.Color:
					WriteColor(writer, (Color)value);
					break;
#endif

				case EnBoisKnownType.Version:
					WriteVersion(writer, value as Version);
					break;

				case EnBoisKnownType.DbNull:
					// Do not write anything, it is already written as Nullable object. //WriteNullableType(true);
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}


#if !SILVERLIGHT
		private void WriteCollectionNameValue(BinaryWriter writer, NameValueCollection nameValue)
		{
			// Int32
			PrimitivesConvertion.WriteVarInt(writer, nameValue.Count);

			foreach (string key in nameValue)
			{
				WriteValue(writer, key);
				WriteValue(writer, nameValue[key]);
			}
		}

		private void WriteColor(BinaryWriter writer, Color c)
		{
			int argb = c.ToArgb();
			// Int32
			PrimitivesConvertion.WriteVarInt(writer, argb);
		}

		private string GetXmlSchema(DataTable dt)
		{
			using (var writer = new StringWriter())
			{
				dt.WriteXmlSchema(writer);
				return writer.ToString();
			}
		}

		private void WriteDataset(BinaryWriter writer, DataSet ds)
		{
			// Int32
			PrimitivesConvertion.WriteVarInt(writer, ds.Tables.Count);

			foreach (DataTable table in ds.Tables)
			{
				WriteDataTable(writer, table);
			}
		}

		private void WriteDataTable(BinaryWriter writer, DataTable table)
		{
			if (string.IsNullOrEmpty(table.TableName))
				table.TableName = "tbl_" + DateTime.Now.Ticks.GetHashCode().ToString();
			WriteString(writer, GetXmlSchema(table));

			// Int32
			PrimitivesConvertion.WriteVarInt(writer, table.Rows.Count);

			var colsCount = table.Columns.Count;
			foreach (DataRow row in table.Rows)
			{
				WriteDataRow(writer, row, colsCount);
			}
		}

		private void WriteDataRow(BinaryWriter writer, DataRow row, int columnCount)
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
			PrimitivesConvertion.WriteVarInt(writer, values.Count);

			foreach (var value in values)
			{
				// Int32
				PrimitivesConvertion.WriteVarInt(writer, value.Key);

				WriteValue(writer, value.Value);
			}
		}
#endif

		private void WriteEnum(BinaryWriter writer, Enum e)
		{
			// Int32
			PrimitivesConvertion.WriteVarInt(writer, (int)((object)e));
		}

		private void WriteGenericList(BinaryWriter writer, IEnumerable array)
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
			PrimitivesConvertion.WriteVarInt(writer, count);
			foreach (object obj in array)
			{
				WriteValue(writer, obj, itemType);
			}
		}
		private void WriteArray(BinaryWriter writer, IEnumerable array)
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
			PrimitivesConvertion.WriteVarInt(writer, count);
			foreach (object obj in array)
			{
				WriteValue(writer, obj);
			}
		}

		private void WriteBytes(BinaryWriter writer, byte[] bytes)
		{
			// Int32
			PrimitivesConvertion.WriteVarInt(writer, bytes.Length);
			writer.Write(bytes);
		}

		private void WriteDictionary(BinaryWriter writer, IDictionary dic)
		{
			// Int32
			PrimitivesConvertion.WriteVarInt(writer, dic.Count);

			var genericType = dic.GetType().GetGenericArguments();
			var keyType = genericType[0];
			var valType = genericType[1];

			foreach (DictionaryEntry entry in dic)
			{
				WriteValue(writer, entry.Key, keyType);
				WriteValue(writer, entry.Value, valType);
			}
		}

		private void WriteStringDictionary(BinaryWriter writer, IDictionary dic)
		{
			// Int32
			PrimitivesConvertion.WriteVarInt(writer, dic.Count);

			var genericType = dic.GetType().GetGenericArguments();
			var keyType = typeof(string);
			var valType = genericType[1];

			foreach (DictionaryEntry entry in dic)
			{
				WriteValue(writer, entry.Key.ToString(), keyType);
				WriteValue(writer, entry.Value, valType);
			}
		}

		[Obsolete]
		private void WritePairString(BinaryWriter writer, string name, object value)
		{
			WriteString(writer, name);
			WriteValue(writer, value);
		}

		[Obsolete]
		private void WritePairObject(BinaryWriter writer, object key, object value)
		{
			WriteValue(writer, key);
			WriteValue(writer, value);
		}

		private void WriteDateTime(BinaryWriter writer, DateTime dateTime)
		{
			var dt = dateTime;
			if (dt == DateTime.MinValue || dt == DateTime.MaxValue)
			{
				PrimitivesConvertion.WriteVarInt(writer, dt.Ticks);
			}
			else
			{
				if (dt.Kind != DateTimeKind.Utc)
				{
					dt = dt.ToUniversalTime();
				}
				//Int64
				PrimitivesConvertion.WriteVarInt(writer, dt.Ticks);
			}
		}

		private void WriteTimeSpan(BinaryWriter writer, TimeSpan timeSpan)
		{
			// Int64
			PrimitivesConvertion.WriteVarInt(writer, timeSpan.Ticks);
		}
		private void WriteVersion(BinaryWriter writer, Version version)
		{
			WriteString(writer, version.ToString());
		}

		private void WriteString(BinaryWriter writer, string str)
		{
			if (str == null)
			{
				PrimitivesConvertion.WriteVarInt(writer, (int?)null);
			}
			else if (str.Length == 0)
			{
				PrimitivesConvertion.WriteVarInt(writer, (int?)0);
			}
			else
			{
				var strBytes = Encoding.GetBytes(str);
				// Int32
				PrimitivesConvertion.WriteVarInt(writer, (int?)strBytes.Length);
				writer.Write(strBytes);
			}
		}

		[Obsolete]
		private void WriteString_OLD(BinaryWriter writer, string str)
		{
			WriteBytes(writer, Encoding.GetBytes(str));
		}
		[Obsolete]
		private void WriteString_OLD(BinaryWriter writer, string str, bool checkNull)
		{
			if (checkNull)
			{
				if (str == null)
					WriteNullableType(writer, true);
				else
					WriteNullableType(writer, false);
			}
			if (str == null)
			{
				// length of the string array
				// Int32
				PrimitivesConvertion.WriteVarInt(writer, (int)0);
			}
			else
			{
				WriteBytes(writer, Encoding.GetBytes(str));
			}
		}

		private void WriteGuid(BinaryWriter writer, Guid g)
		{
			if (g == Guid.Empty)
			{
				// Int32
				PrimitivesConvertion.WriteVarInt(writer, 0);
				return;
			}

			var data = g.ToByteArray();
			// Int32
			PrimitivesConvertion.WriteVarInt(writer, data.Length);
			writer.Write(data);
		}
		#endregion

		#region Deserialization methods

		private object ReadObject(BinaryReader reader, Type type)
		{
			//Int32
			var binaryMemberCount = PrimitivesConvertion.ReadVarInt32Nullable(reader);
			if (binaryMemberCount == null)
			{
				return null;
			}

			var bionType = _typeCache.GetTypeInfo(type, true) as BoisTypeInfo;

			var members = bionType.Members;
			var resultObj = _typeCache.CreateInstance(type);
			ReadMembers(reader, resultObj, members, binaryMemberCount.Value);
			return resultObj;
		}

		private void ReadMembers(BinaryReader reader, object obj, BoisMemberInfo[] memberList, int binaryMemberCount)
		{
			var objectMemberCount = memberList.Length;
			var memberProcessed = 0;
			var dataLeng = reader.BaseStream.Length;
			var data = reader.BaseStream;

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
					var value = ReadMember(reader, memInfo, pinfo.PropertyType);

					// using the setter
					memInfo.PropertySetter(obj, value);
				}
				else
				{
					var finfo = memInfo.Info as FieldInfo;

					// read the value
					var value = ReadMember(reader, memInfo, finfo.FieldType);

					ReflectionHelper.SetValue(obj, value, finfo);
				}

			}
		}

		private object ReadMember(BinaryReader reader, Type memType)
		{
			var memInfo = _typeCache.GetTypeInfo(memType, true);
			return ReadMember(reader, memInfo, memType);
		}

		private object ReadMember(BinaryReader reader, BoisMemberInfo memInfo, Type memType)
		{
			if (memInfo.IsNullable &&
				!memInfo.IsSupportedPrimitive &&
				!memInfo.IsContainerObject)
			{
				bool isNull = reader.ReadByte() != 0;

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
						return ReadObject(reader, actualMemberType);
					}
					else if (memInfo.IsStringDictionary)
					{
						return ReadStringDictionary(reader, actualMemberType);
					}
					else if (memInfo.IsDictionary)
					{
						return ReadDictionary(reader, actualMemberType);
					}
					else if (memInfo.IsCollection)
					{
						if (memInfo.IsGeneric)
						{
							return ReadGenericList(reader, actualMemberType);
						}
						return ReadArray(reader, actualMemberType);
					}
					else if (memInfo.IsArray)
					{
						return ReadArray(reader, actualMemberType);
					}

					break;

				case EnBoisKnownType.Int16:
					if (memInfo.IsNullable)
					{
						return PrimitivesConvertion.ReadVarInt16Nullable(reader);
					}
					return PrimitivesConvertion.ReadVarInt16(reader);

				case EnBoisKnownType.Int32:
					if (memInfo.IsNullable)
					{
						return PrimitivesConvertion.ReadVarInt32Nullable(reader);
					}
					return PrimitivesConvertion.ReadVarInt32(reader);

				case EnBoisKnownType.Int64:
					if (memInfo.IsNullable)
					{
						return PrimitivesConvertion.ReadVarInt64Nullable(reader);
					}
					return PrimitivesConvertion.ReadVarInt64(reader);

				case EnBoisKnownType.UInt16:
					return reader.ReadUInt16();

				case EnBoisKnownType.UInt32:
					return reader.ReadUInt32();

				case EnBoisKnownType.UInt64:
					return reader.ReadUInt64();

				case EnBoisKnownType.Double:
					return reader.ReadDouble();

				case EnBoisKnownType.Decimal:
#if SILVERLIGHT
					return ReadDecimal(reader);
#else
					return reader.ReadDecimal();
#endif

				case EnBoisKnownType.Single:
					return reader.ReadSingle();

				case EnBoisKnownType.Byte:
					return reader.ReadByte();

				case EnBoisKnownType.SByte:
					return reader.ReadSByte();

				case EnBoisKnownType.ByteArray:
					return ReadBytes(reader);

				case EnBoisKnownType.String:
					return ReadString(reader);

				case EnBoisKnownType.Char:
					return reader.ReadChar();

				case EnBoisKnownType.Guid:
					return ReadGuid(reader);

				case EnBoisKnownType.Bool:
					return ReadBoolean(reader);

				case EnBoisKnownType.Enum:
					return ReadEnum(reader, actualMemberType);

				case EnBoisKnownType.DateTime:
					return ReadDateTime(reader);

				case EnBoisKnownType.TimeSpan:
					return ReadTimeSpan(reader);

#if !SILVERLIGHT
				case EnBoisKnownType.DataSet:
					return ReadDataset(reader, actualMemberType);

				case EnBoisKnownType.DataTable:
					return ReadDataTable(reader);

				case EnBoisKnownType.NameValueColl:
					return ReadCollectionNameValue(reader, actualMemberType);

				case EnBoisKnownType.Color:
					return ReadColor(reader);
#endif

				case EnBoisKnownType.Version:
					return ReadVersion(reader);

				case EnBoisKnownType.DbNull:
					return DBNull.Value;

				default:
					throw new ArgumentOutOfRangeException();
			}
			return null;
		}

		private object ReadEnum(BinaryReader reader, Type type)
		{
			var val = PrimitivesConvertion.ReadVarInt32(reader);
			return Enum.ToObject(type, val);
		}

		private Array ReadArray(BinaryReader reader, Type type)
		{
			var count = PrimitivesConvertion.ReadVarInt32(reader);

			var itemType = type.GetElementType();
			if (itemType == null)
				throw new ArgumentException("Unknown 'Object' array type is not supported.\n" + type);

			var arr = ReflectionHelper.CreateArray(itemType, count);
			var lst = arr as IList;
			for (int i = 0; i < count; i++)
			{
				var val = ReadMember(reader, itemType);
				lst[i] = val;
			}
			return arr;
		}

		private IList ReadGenericList(BinaryReader reader, Type type)
		{
			var count = PrimitivesConvertion.ReadVarInt32(reader);

			var listObj = (IList)_typeCache.CreateInstance(type);

			var itemType = type.GetGenericArguments()[0];
			for (int i = 0; i < count; i++)
			{
				var val = ReadMember(reader, itemType);
				listObj.Add(val);
			}

			return listObj;
		}

		private IList ReadIListImpl(BinaryReader reader, Type type)
		{
			var count = PrimitivesConvertion.ReadVarInt32(reader);

			var listObj = (IList)_typeCache.CreateInstance(type);

			var itemType = type.GetElementType();
			if (itemType == null)
				throw new ArgumentException("Unknown ICollection implementation is not supported.\n" + type.ToString());

			for (int i = 0; i < count; i++)
			{
				var val = ReadMember(reader, itemType);
				listObj.Add(val);
			}

			return listObj;
		}
		private byte[] ReadBytes(BinaryReader reader)
		{
			var length = PrimitivesConvertion.ReadVarInt32(reader);
			return reader.ReadBytes(length);
		}

#if SILVERLIGHT
		private decimal ReadDecimal(BinaryReader reader)
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
		private object ReadColor(BinaryReader reader)
		{
			return Color.FromArgb(PrimitivesConvertion.ReadVarInt32(reader));
		}
		private object ReadCollectionNameValue(BinaryReader reader, Type type)
		{
			var count = PrimitivesConvertion.ReadVarInt32(reader);
			var nameValue = (NameValueCollection)_typeCache.CreateInstance(type);
			var strType = typeof(string);
			for (int i = 0; i < count; i++)
			{
				var name = ReadMember(reader, strType) as string;
				var val = ReadMember(reader, strType) as string;
				nameValue.Add(name, val);
			}
			return nameValue;
		}
		private DataTable ReadDataTable(BinaryReader reader)
		{
			var dt = _typeCache.CreateInstance(typeof(DataTable)) as DataTable;

			var schema = ReadString(reader);
			//dt.TableName = name;
			SetXmlSchema(dt, schema);

			var cols = dt.Columns;

			var rowCount = PrimitivesConvertion.ReadVarInt32(reader);
			for (int i = 0; i < rowCount; i++)
			{
				var row = dt.Rows.Add();
				ReadDataRow(reader, row, cols);
			}
			return dt;
		}

		private void ReadDataRow(BinaryReader reader, DataRow row, DataColumnCollection cols)
		{
			var itemCount = PrimitivesConvertion.ReadVarInt32(reader);
			var colCount = cols.Count;
			var passedIndex = 0;
			while (passedIndex < colCount && passedIndex < itemCount)
			{
				passedIndex++;

				var colIndex = PrimitivesConvertion.ReadVarInt32(reader);
				var col = cols[colIndex];
				var val = ReadMember(reader, col.DataType);
				row[col] = val ?? DBNull.Value;
			}

		}

		private object ReadDataset(BinaryReader reader, Type memType)
		{
			var count = PrimitivesConvertion.ReadVarInt32(reader);
			var ds = _typeCache.CreateInstance(memType) as DataSet;
			for (int i = 0; i < count; i++)
			{
				var dt = ReadDataTable(reader);
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

		private object ReadDictionary(BinaryReader reader, Type memType)
		{
			var count = PrimitivesConvertion.ReadVarInt32(reader);
			var dic = _typeCache.CreateInstance(memType) as IDictionary;

			var genericType = memType.GetGenericArguments();
			var keyType = genericType[0];
			var valType = genericType[1];

			for (int i = 0; i < count; i++)
			{
				var key = ReadMember(reader, keyType);
				var val = ReadMember(reader, valType);
				dic.Add(key, val);
			}
			return dic;
		}

		private object ReadStringDictionary(BinaryReader reader, Type memType)
		{
			var count = PrimitivesConvertion.ReadVarInt32(reader);
			var dic = _typeCache.CreateInstance(memType) as IDictionary;

			var genericType = memType.GetGenericArguments();
			var keyType = genericType[0];
			var valType = genericType[1];

			for (int i = 0; i < count; i++)
			{
				var key = ReadMember(reader, keyType);
				var val = ReadMember(reader, valType);
				dic.Add(key, val);
			}
			return dic;
		}

		private TimeSpan ReadTimeSpan(BinaryReader reader)
		{
			var ticks = PrimitivesConvertion.ReadVarInt64(reader);
			return new TimeSpan(ticks);
		}
		private object ReadVersion(BinaryReader reader)
		{
			return new Version(ReadString(reader));
		}

		private object ReadDateTime(BinaryReader reader)
		{
			var ticks = PrimitivesConvertion.ReadVarInt64(reader);
			if (ticks == DateTime.MinValue.Ticks || ticks == DateTime.MaxValue.Ticks)
			{
				return new DateTime(ticks);
			}

			return new DateTime(ticks, DateTimeKind.Utc).ToLocalTime();
		}

		private object ReadBoolean(BinaryReader reader)
		{
			return reader.ReadByte() != 0;
		}

		private string ReadString(BinaryReader reader)
		{
			int? length = PrimitivesConvertion.ReadVarInt32Nullable(reader);
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
				var strBuff = reader.ReadBytes(length.Value);
				return Encoding.GetString(strBuff, 0, strBuff.Length);
			}
		}

		private object ReadGuid(BinaryReader reader)
		{
			var gbuff = ReadBytes(reader);
			if (gbuff.Length == 0)
				return Guid.Empty;
			return new Guid(gbuff);
		}

		private object ReadChar(BinaryReader reader)
		{
			return reader.ReadChar();
		}
		#endregion

	}
}
