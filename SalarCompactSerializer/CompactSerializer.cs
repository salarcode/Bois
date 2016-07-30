using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace SalarCompactSerializer
{
	public class CompactSerializer
	{
		private bool _ordinalNotation = true;
		private bool _useUtcDateTime = true;
		private bool _serializeNullValues = false;
		private bool _useExtensions = false;
		private StringBuilder _serializeOut;
		private int _serializeDepth;
		//private bool UseOptimizedDatasetSchema = false;
		private bool _showReadOnlyProperties;

		public bool OrdinalNotation
		{
			get { return _ordinalNotation; }
			set { _ordinalNotation = value; }
		}

		public bool UseUtcDateTime
		{
			get { return _useUtcDateTime; }
			set { _useUtcDateTime = value; }
		}

		public bool SerializeNullValues
		{
			get { return _serializeNullValues; }
			set { _serializeNullValues = value; }
		}

		public bool UseExtensions
		{
			get { return _useExtensions; }
			set { _useExtensions = value; }
		}

		public bool ShowReadOnlyProperties
		{
			get { return _showReadOnlyProperties; }
			set { _showReadOnlyProperties = value; }
		}

		public string Serialize(object obj)
		{
			try
			{
				_serializeDepth = 0;
				_serializeOut = new StringBuilder();

				WriteObject(obj);

				return _serializeOut.ToString();
			}
			finally
			{
				_serializeOut.Length = 0;
				_serializeOut = null;
			}
		}

		private void WriteObject(object obj)
		{
			_serializeDepth++;
			_serializeOut.Append("{");
			var type = obj.GetType();
			var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
			var fields = type.GetFields(BindingFlags.GetField | BindingFlags.Public | BindingFlags.Instance);

			var memCount = props.Length + fields.Length;
			var memIndex = 0;
			bool writeDone;
			for (int i = 0; i < props.Length; i++)
			{
				var p = props[i];
				if (!_showReadOnlyProperties && !p.CanWrite)
					continue;

				writeDone = WriteMember(obj, memIndex, p);
				memIndex++;
				if (writeDone && memIndex < memCount)
					_serializeOut.Append(',');
			}
			for (int i = 0; i < fields.Length; i++)
			{
				writeDone = WriteMember(obj, memIndex, fields[i]);
				memIndex++;
				if (writeDone && memIndex < memCount)
					_serializeOut.Append(',');
			}
			_serializeOut.Append('}');
		}

		bool WriteMember(object obj, int index, FieldInfo memInfo)
		{
			var value = memInfo.GetValue(obj);
			if (value == null && !_serializeNullValues)
				return false;
			if (_ordinalNotation)
			{
				_serializeOut.Append(index);
				_serializeOut.Append(':');
			}
			else
			{
				WriteStringDirect(memInfo.Name);
				_serializeOut.Append(':');
			}
			WriteValue(value);
			return true;
		}
		bool WriteMember(object obj, int index, PropertyInfo memInfo)
		{
			var value = memInfo.GetValue(obj, null);
			if (value == null && !_serializeNullValues)
				return false;
			if (_ordinalNotation)
			{
				_serializeOut.Append(index);
				_serializeOut.Append(':');
			}
			else
			{
				WriteStringDirect(memInfo.Name);
				_serializeOut.Append(':');
			}
			WriteValue(value);
			return true;
		}

		void WriteValue(object obj)
		{
			if (obj == null || obj is DBNull)
			{
				_serializeOut.Append("null");
			}
			else if (obj is string || obj is char)
			{
				WriteString(obj.ToString());
			}
			else if (obj is Guid)
			{
				WriteGuid((Guid)obj);
			}
			else if (obj is bool)
			{
				if (_ordinalNotation)
				{
					_serializeOut.Append(((bool)obj) ? 1 : 0);
				}
				else
				{
					_serializeOut.Append(((bool)obj) ? "true" : "false");
				}
			}
			else if (
				obj is int || obj is long || obj is double ||
				obj is decimal || obj is float ||
				obj is byte || obj is short ||
				obj is sbyte || obj is ushort ||
				obj is uint || obj is ulong
				)
			{
				_serializeOut.Append(((IConvertible)obj).ToString(NumberFormatInfo.InvariantInfo));
			}
			else if (obj is DateTime)
			{
				WriteDateTime((DateTime)obj);
			}
			else if (obj is IDictionary && obj.GetType().IsGenericType &&
					 obj.GetType().GetGenericArguments()[0] == typeof(string))
			{
				WriteStringDictionary((IDictionary)obj);
			}
			else if (obj is IDictionary)
			{
				WriteDictionary((IDictionary)obj);
			}
#if !SILVERLIGHT
			else if (obj is DataSet)
			{
				WriteDataset((DataSet)obj);
			}
			else if (obj is DataTable)
			{
				this.WriteDataTable((DataTable)obj);
			}
#endif
			else if (obj is byte[])
			{
				WriteBytes((byte[])obj);
			}
			else if (obj is Array || obj is IList || obj is ICollection)
			{
				WriteArray((IEnumerable)obj);
			}
			else if (obj is Enum)
			{
				WriteEnum((Enum)obj);
			}
			else
			{
				WriteUnknownObject(obj);
			}
		}

		private void WriteUnknownObject(object obj)
		{
			WriteObject(obj);
		}


		private void WriteEnum(Enum e)
		{
			if (_ordinalNotation)
			{
				_serializeOut.Append(((int)((object)e)).ToString(NumberFormatInfo.InvariantInfo));
			}
			else
			{
				WriteStringDirect(e.ToString());
			}
		}

		private void WriteArray(IEnumerable array)
		{
			_serializeOut.Append('[');

			bool pendingSeperator = false;

			foreach (object obj in array)
			{
				if (pendingSeperator) _serializeOut.Append(',');

				WriteValue(obj);

				pendingSeperator = true;
			}
			_serializeOut.Append(']');
		}


		private void WriteBytes(byte[] bytes)
		{
#if !SILVERLIGHT
			WriteStringDirect(Convert.ToBase64String(bytes, 0, bytes.Length, Base64FormattingOptions.None));
#else
            WriteStringDirect(Convert.ToBase64String(bytes, 0, bytes.Length));
#endif
		}


#if !SILVERLIGHT
		void WriteDataTable(DataTable dt)
		{
			_serializeOut.Append('{');
			if (_useExtensions)
			{
				//this.WritePair("$schema", _params.UseOptimizedDatasetSchema ? (object)this.GetSchema(dt) : this.GetXmlSchema(dt));
				WritePair("$schema", GetXmlSchema(dt));
				_serializeOut.Append(',');
			}

			WriteDataTableData(dt);

			// end datatable
			_serializeOut.Append('}');
		}

		private string GetXmlSchema(DataTable dt)
		{
			using (var writer = new StringWriter())
			{
				dt.WriteXmlSchema(writer);
				return dt.ToString();
			}
		}

		private void WriteDataset(DataSet ds)
		{
			_serializeOut.Append('{');
			if (UseExtensions)
			{
				//WritePair("$schema", UseOptimizedDatasetSchema ? (object)GetSchema(ds) : ds.GetXmlSchema());
				WritePair("$schema", ds.GetXmlSchema());
				_serializeOut.Append(',');
			}
			bool tablesep = false;
			foreach (DataTable table in ds.Tables)
			{
				if (tablesep) _serializeOut.Append(",");
				tablesep = true;
				WriteDataTableData(table);
			}
			// end dataset
			_serializeOut.Append('}');
		}

		private void WriteDataTableData(DataTable table)
		{
			_serializeOut.Append('\"');
			_serializeOut.Append(table.TableName);
			_serializeOut.Append("\":[");
			DataColumnCollection cols = table.Columns;
			bool rowseparator = false;
			foreach (DataRow row in table.Rows)
			{
				if (rowseparator)
					_serializeOut.Append(",");
				rowseparator = true;
				_serializeOut.Append('[');

				bool pendingSeperator = false;
				foreach (DataColumn column in cols)
				{
					if (pendingSeperator) _serializeOut.Append(',');
					WriteValue(row[column]);
					pendingSeperator = true;
				}
				_serializeOut.Append(']');
			}

			_serializeOut.Append(']');
		}
#endif

		private void WriteDictionary(IDictionary dic)
		{
			_serializeOut.Append('[');

			bool pendingSeparator = false;

			foreach (DictionaryEntry entry in dic)
			{
				if (pendingSeparator)
					_serializeOut.Append(',');
				_serializeOut.Append('{');
				WritePair("k", entry.Key);
				_serializeOut.Append(",");
				WritePair("v", entry.Value);
				_serializeOut.Append('}');

				pendingSeparator = true;
			}
			_serializeOut.Append(']');
		}

		private void WriteStringDictionary(IDictionary dic)
		{
			_serializeOut.Append('{');

			bool pendingSeparator = false;

			foreach (DictionaryEntry entry in dic)
			{
				if (pendingSeparator) _serializeOut.Append(',');

				WritePair((string)entry.Key, entry.Value);

				pendingSeparator = true;
			}
			_serializeOut.Append('}');
		}

		private void WritePair(string name, object value)
		{
			if ((value == null || value is DBNull) && _serializeNullValues == false)
				return;
			WriteStringDirect(name);

			_serializeOut.Append(':');

			WriteValue(value);
		}

		private void WriteDateTime(DateTime dateTime)
		{
			// datetime format standard : yyyy-MM-dd HH:mm:ss
			DateTime dt = dateTime;
			if (UseUtcDateTime)
				dt = dateTime.ToUniversalTime();

			_serializeOut.Append("\"");
			_serializeOut.Append(dt.Year.ToString("0000", NumberFormatInfo.InvariantInfo));
			_serializeOut.Append("-");
			_serializeOut.Append(dt.Month.ToString("00", NumberFormatInfo.InvariantInfo));
			_serializeOut.Append("-");
			_serializeOut.Append(dt.Day.ToString("00", NumberFormatInfo.InvariantInfo));
			_serializeOut.Append(" ");
			_serializeOut.Append(dt.Hour.ToString("00", NumberFormatInfo.InvariantInfo));
			_serializeOut.Append(":");
			_serializeOut.Append(dt.Minute.ToString("00", NumberFormatInfo.InvariantInfo));
			_serializeOut.Append(":");
			_serializeOut.Append(dt.Second.ToString("00", NumberFormatInfo.InvariantInfo));

			if (UseUtcDateTime)
				_serializeOut.Append("Z");

			_serializeOut.Append("\"");
		}

		private void WriteGuid(Guid g)
		{
			//if (_params.UseFastGuid == false)
			WriteStringDirect(g.ToString());
			//else
			//	WriteBytes(g.ToByteArray());
		}
		private void WriteStringDirect(string s)
		{
			_serializeOut.Append('\"');
			_serializeOut.Append(s);
			_serializeOut.Append('\"');
		}
		private void WriteString(string s)
		{
			_serializeOut.Append('\"');

			int runIndex = -1;

			for (var index = 0; index < s.Length; ++index)
			{
				var c = s[index];

				if (c >= ' ' && c < 128 && c != '\"' && c != '\\')
				{
					if (runIndex == -1)
					{
						runIndex = index;
					}

					continue;
				}

				if (runIndex != -1)
				{
					_serializeOut.Append(s, runIndex, index - runIndex);
					runIndex = -1;
				}

				switch (c)
				{
					case '\t': _serializeOut.Append("\\t"); break;
					case '\r': _serializeOut.Append("\\r"); break;
					case '\n': _serializeOut.Append("\\n"); break;
					case '"':
					case '\\': _serializeOut.Append('\\'); _serializeOut.Append(c); break;
					default:
						_serializeOut.Append("\\u");
						_serializeOut.Append(((int)c).ToString("X4", NumberFormatInfo.InvariantInfo));
						break;
				}
			}

			if (runIndex != -1)
			{
				_serializeOut.Append(s, runIndex, s.Length - runIndex);
			}

			_serializeOut.Append('\"');
		}
	}
}
