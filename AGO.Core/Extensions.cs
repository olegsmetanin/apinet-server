using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using AGO.Core.Attributes.Constraints;
using AGO.Core.Json;
using AGO.Core.Model;

namespace AGO.Core
{
	public static class Extensions
	{
		public static bool IsNullOrWhiteSpace(this string str)
		{
			return String.IsNullOrWhiteSpace(str);
		}

		public static bool IsNullOrEmpty(this string str)
		{
			return String.IsNullOrEmpty(str);
		}

		public static string AddPrefix(this string str, string prefix)
		{
			if (str.IsNullOrEmpty() || prefix.IsNullOrEmpty())
				return str ?? string.Empty;
			return str.StartsWith(prefix)
				? str
				: prefix + str;
		}

		public static string TrimSafe(this string str)
		{
			return str.IsNullOrWhiteSpace() ? string.Empty : str.Trim();
		}

		public static string RemovePrefix(this string str, string prefix)
		{
			if (str.IsNullOrEmpty() || prefix.IsNullOrEmpty())
				return str ?? string.Empty;
			return str.StartsWith(prefix)
				? str.Substring(prefix.Length, str.Length - prefix.Length)
				: str;
		}

		public static string AddSuffix(this string str, string suffix)
		{
			if (str.IsNullOrEmpty() || suffix.IsNullOrEmpty())
				return str ?? string.Empty;
			return str.EndsWith(suffix)
				? str
				: str + suffix;
		}

		public static string RemoveSuffix(this string str, string suffix)
		{
			if (str.IsNullOrEmpty() || suffix.IsNullOrEmpty())
				return str ?? string.Empty;
			return str.EndsWith(suffix)
				? str.Substring(0, str.Length - suffix.Length)
				: str;
		}

		public static bool IsValue(this Type type)
		{
			return type != null && 
				(type.IsValueType || (type.IsNullable() && IsValue(type.GetGenericArguments()[0])));
		}

		public static bool IsNullable(this Type type)
		{
			return type != null && (type.IsGenericType &&
				type.GetGenericTypeDefinition() == typeof(Nullable<>));
		}

		public static string ToStringSafe(
			this DateTime? date,
			IFormatProvider formatProvider = null,
			string format = "")
		{
			var result = String.Empty;
			if (date != null)
			{
				result = format.IsNullOrEmpty()
					? date.Value.ToString(formatProvider)
					: date.Value.ToString(format, formatProvider);
			}
			return result;
		}

		public static int ParseSafe(this string str)
		{
			int value;
			int.TryParse(str, out value);
			return value;
		}

		public static object ParseEnumSafe(this string str, Type enumType, bool ignoreCase = true)
		{
			if (str.IsNullOrWhiteSpace())
				return Activator.CreateInstance(enumType);
			try
			{
				return Enum.Parse(enumType, str, ignoreCase);
			}
			catch (Exception)
			{
				return Activator.CreateInstance(enumType);
			}
		}

		public static TType ParseEnumSafe<TType>(
			this string value,
			TType defaultValue,
			bool ignoreCase = true)
			where TType : struct
		{
			if (value.IsNullOrEmpty())
				return defaultValue;
			TType result;
			try
			{
				result = (TType)Enum.Parse(typeof(TType), value, ignoreCase);
			}
			catch (Exception)
			{
				result = defaultValue;
			}
			return result;
		}

		public static TType? ParseEnumSafe<TType>(
			this string value,
			bool ignoreCase = true)
			where TType : struct
		{
			if (value.IsNullOrEmpty())
				return null;
			TType? result;
			try
			{
				result = (TType)Enum.Parse(typeof(TType), value, ignoreCase);
			}
			catch (Exception)
			{
				result = null;
			}
			return result;
		}

		public static DateTime? ToUniversalTime(this DateTime? dateTime)
		{
			if (dateTime == null)
				return null;
			return dateTime.Value.ToUniversalTime();
		}

		public static DateTime? ToLocalTime(this DateTime? dateTime)
		{
			if (dateTime == null)
				return null;
			return dateTime.Value.ToLocalTime();
		}

		public static bool IsNullOrEmpty(this Guid? value)
		{
			return value == null || default(Guid).Equals(value.Value);
		}

		public static string ToStringSafe(this object obj, IFormatProvider formatProvider = null)
		{
			var result = obj as string;
			if (result != null)
				return result;

			result = String.Empty;
			if (obj != null)
				result = obj is IConvertible ? ((IConvertible)obj).ToString(formatProvider) : obj.ToString();
			return result;
		}

		public static object ConvertSafe(
			this object obj,
			Type type,
			IFormatProvider formatProvider = null)
		{
			if (obj == null)
				return null;

			var resultType = type;
			var isNullable = false;
			if (type.IsNullable())
			{
				resultType = type.GetGenericArguments()[0];
				isNullable = true;
			}
			if (resultType.IsInstanceOfType(obj))
				return obj;

			var strObj = obj as string;
			strObj = strObj != null ? strObj.Trim() : null;

			if (typeof (string).IsAssignableFrom(resultType))
			{
				if (strObj != null)
					return strObj;

				var identifiedModel = obj as IIdentifiedModel;
				return identifiedModel != null ? identifiedModel.UniqueId : obj.ToStringSafe(formatProvider);
			}

			if (resultType == typeof(bool) && strObj != null)
				return strObj.Equals("1") || strObj.Equals("true", StringComparison.InvariantCultureIgnoreCase);

			if (resultType == typeof(Guid) && strObj != null)
			{
				Guid resultGuid;
				if (Guid.TryParse(strObj, out resultGuid))
					return resultGuid;
			}

			if (resultType == typeof(TimeSpan))
			{
				TimeSpan result;
				if (TimeSpan.TryParse(strObj, out result))
					return result;
			}

			if (resultType.IsEnum && strObj != null)
			{
				if (isNullable && strObj.IsNullOrEmpty())
					return null;
				return strObj.ParseEnumSafe(resultType);
			}

			if (typeof(Type).IsAssignableFrom(resultType) && strObj != null)
			{
				try
				{
					return Type.GetType(strObj);
				}
				catch
				{
					return null;
				}
			}

			if (typeof(Uri).IsAssignableFrom(resultType) && strObj != null)
			{
				try
				{
					return new Uri(strObj);
				}
				catch
				{
					return null;
				}
			}

			try
			{
				return Convert.ChangeType(obj, resultType, formatProvider);
			}
			catch (InvalidCastException)
			{
				return null;
			}
			catch (FormatException)
			{
				return null;
			}
			catch (OverflowException)
			{
				return null;
			}
		}

		public static TType ConvertSafe<TType>(
			this object obj,
			IFormatProvider formatProvider = null)
		{
			var result = obj.ConvertSafe(typeof(TType), formatProvider);
			return result != null ? (TType)result : default(TType);
		}

		public static TType FirstAttribute<TType>(
			this MemberInfo method,
			bool inherit,
			bool checkInterfaces = false)
			where TType : Attribute
		{
			return method.FirstAttribute(typeof(TType), inherit, checkInterfaces) as TType;
		}

		public static Attribute FirstPropertyAttribute(
			this MemberInfo method,
			Type type,
			bool inherit,
			bool checkInterfaces = false)
		{
			if (method == null)
				return null;
			if (!method.Name.StartsWith("get_") && !method.Name.StartsWith("set_"))
				return null;
			if (method.DeclaringType == null)
				return null;
			var property = method.DeclaringType.GetProperty(method.Name.Substring(4));
			if (property == null)
				return null;
			var attribute = property.FirstAttribute(type, inherit);
			if (attribute != null)
				return attribute;

			var methodBase = method as MethodBase;
			var declaringType = methodBase != null ? methodBase.DeclaringType : null;
			if (!checkInterfaces || methodBase == null || declaringType == null)
				return null;
			var parameterTypes = methodBase.GetParameters().Select(pi => pi.ParameterType).ToArray();
			foreach (var interfaceType in declaringType.GetInterfaces())
			{
				var methodName = methodBase.Name;
				if (methodName.IndexOf('.') != -1)
				{
					if (methodName.StartsWith(interfaceType.FullName.Replace('+', '.')))
						methodName = methodName.Remove(0, interfaceType.FullName.Length + 1);
				}

				var intfMethod = interfaceType.GetMethod(methodName, parameterTypes);
				if (intfMethod == null)
					continue;
				attribute = intfMethod.FirstPropertyAttribute(type, inherit);
				if (attribute != null)
					return attribute;
			}
			return null;
		}

		public static Attribute FirstAttribute(
			this MemberInfo method,
			Type type, bool inherit,
			bool checkInterfaces = false)
		{
			if (method == null)
				return null;

			var attributes = method.GetCustomAttributes(type, inherit);
			var attribute = attributes.Length > 0 ? attributes[0] as Attribute : null;
			if (attribute != null)
				return attribute;

			var methodBase = method as MethodBase;
			var declaringType = methodBase != null ? methodBase.DeclaringType : null;
			if (!checkInterfaces || methodBase == null || declaringType == null)
				return null;
			var parameterTypes = methodBase.GetParameters().Select(pi => pi.ParameterType).ToArray();
			foreach (var interfaceType in declaringType.GetInterfaces())
			{
				var methodName = methodBase.Name;
				if (methodName.IndexOf('.') != -1)
				{
					if (methodName.StartsWith(interfaceType.FullName.Replace('+', '.')))
						methodName = methodName.Remove(0, interfaceType.FullName.Length + 1);
				}

				var intfMethod = interfaceType.GetMethod(methodName, parameterTypes);
				if (intfMethod == null)
					continue;
				attribute = intfMethod.FirstAttribute(type, inherit);
				if (attribute != null)
					return attribute;
			}
			return null;
		}

		private static object ConvertValue(object value, Type toType, bool safe)
		{
			return value == null
			       	? null
			       	: safe
			       	  	? value.ConvertSafe(toType)
			       	  	: Convert.ChangeType(value, toType);
		}

		public static void SetMemberValue(
			this object obj,
			string memberName,
			object value,
			bool convertSafe = true)
		{
			if (obj == null || !obj.GetType().IsClass)
				return;

			var prop = obj.GetType().GetProperty(memberName);
			if (prop != null)
			{
				prop.SetValue(obj, ConvertValue(value, prop.PropertyType, convertSafe) , null);
				return;
			}
			var field = obj.GetType().GetField(memberName);
			if (field == null)
				return;
			field.SetValue(obj, ConvertValue(value, field.FieldType, convertSafe));
		}

		public static object GetMemberValue(
			this object obj,
			string memberName,
			object defaultValue = null)
		{
			if (obj == null || !obj.GetType().IsClass)
				return defaultValue;

			var prop = obj.GetType().GetProperty(memberName);
			if (prop != null)
				return prop.GetValue(obj, null);
			var field = obj.GetType().GetField(memberName);
			return field != null ? field.GetValue(obj) : defaultValue;
		}

		public static T GetMemberValue<T>(
			this object obj,
			string memberName,
			T defaultValue = default(T))
		{
			var value = obj.GetMemberValue(memberName);
			if (value == null)
				return defaultValue;

			if (value is T)
				return (T)value;

			var converted = value.ConvertSafe(typeof(T));
			if (converted != null)
				return (T)converted;

			return defaultValue;
		}

		public static bool IsObjectEmpty(
			object obj,
			Type paramType,
			bool ignoreWhitespace)
		{
			var nullable = paramType.IsNullable();
			var empty =
				((paramType.IsClass || paramType.IsInterface) && obj == null) ||
				(nullable && obj == null) ||
				(paramType.IsArray && (obj == null || ((Array)obj).Length == 0)) ||
				(!nullable && (paramType.IsValueType && !typeof(bool).IsAssignableFrom(paramType) && !paramType.IsEnum)
					&& Activator.CreateInstance(paramType).Equals(obj));
			if (!empty && obj is string)
				empty = ignoreWhitespace ? ((string)obj).IsNullOrWhiteSpace() : ((string)obj).IsNullOrEmpty();
			if (!empty && obj is ICollection)
				empty = ((ICollection)obj).Count == 0;

			return empty;
		}

		public static bool IsObjectInRange(
			object obj,
			Type paramType,
			object start,
			object end,
			bool inclusive)
		{
			if (obj == null)
				return true;
			var realType = paramType.IsNullable() ? paramType.GetGenericArguments()[0] : paramType;
			var isString = typeof(string).IsAssignableFrom(realType) || realType.IsEnum;
			if (!realType.IsValueType && !isString)
				return true;

			var realStart = (isString ? start.ConvertSafe<Int32>() : start.ConvertSafe(realType)) as IComparable;
			var realEnd = (isString ? end.ConvertSafe<Int32>() : end.ConvertSafe(realType)) as IComparable;
			if (realStart == null && realEnd == null)
				return true;
			var realParam = (isString ? obj.ToStringSafe().Length : obj.ConvertSafe(realType)) as IComparable;
			if (realParam == null)
				return false;

			var result = true;
			if (realStart != null)
			{
				var diff = realParam.CompareTo(realStart);
				result = inclusive ? diff >= 0 : diff > 0;
			}
			if (realEnd != null)
			{
				var diff = realParam.CompareTo(realEnd);
				result = result && (inclusive ? diff <= 0 : diff < 0);
			}
			return result;
		}

		public static void UnionWithReplace<TType>(
			this ISet<TType> collection,
			IEnumerable<TType> models)
		{
			if (collection == null || models == null)
				return;
			var list = models.ToList();
			collection.ExceptWith(list);
			collection.UnionWith(list);
		}

		private static int _bufferLength = 1024;
		public static int BufferLength
		{
			get { return _bufferLength; }
			set { if (value > 0) _bufferLength = value; }
		}

		public static void WriteTo(this Stream source, Stream target,
			Action<int> afterBlockWritten = null)
		{
			WriteTo(source, target, BufferLength, afterBlockWritten);
		}

		public static void WriteTo(
			this Stream source,
			Stream target,
			int bufferLength,
			Action<int> afterBlockWritten = null)
		{
			if (source == null)
				return;
			if (target == null)
				throw new ArgumentNullException("target");
			if (bufferLength <= 0)
				throw new ArgumentException("bufferLength<=0");
			var buffer = new byte[bufferLength];
			int bytesRead;

			do
			{
				bytesRead = source.Read(buffer, 0, buffer.Length);
				if (bytesRead <= 0)
					continue;
				target.Write(buffer, 0, bytesRead);
				if (afterBlockWritten != null)
					afterBlockWritten(bytesRead);
			}
			while (bytesRead > 0);
		}

		public static void WriteRange(
			this Stream source,
			Stream target,
			int start,
			int length,
			Action<int> afterBlockWritten = null)
		{
			source.WriteRange(target, start, length, BufferLength, afterBlockWritten);
		}

		public static void WriteRange(
			this Stream source,
			Stream target,
			long start,
			long length,
			int bufferLength,
			Action<int> afterBlockWritten = null)
		{
			if (source == null)
				return;
			if (!source.CanSeek)
				return;
			if (target == null)
				throw new ArgumentNullException("target");
			if (bufferLength <= 0)
				throw new ArgumentException("bufferLength<=0");

			start = start < 0 ? 0 : start;
			source.Seek(start, SeekOrigin.Current);

			var buffer = new byte[bufferLength];
			int bytesRead;
			int toRead;
			var bytesLeft = length;

			do
			{
				toRead = (int)(bytesLeft > bufferLength ? bufferLength : bytesLeft);
				bytesRead = source.Read(buffer, 0, toRead);
				if (bytesRead <= 0)
					continue;
				target.Write(buffer, 0, bytesRead);
				bytesLeft -= bytesRead;
				if (afterBlockWritten != null)
					afterBlockWritten(bytesRead);
			}
			while (bytesRead > 0 && bytesRead == toRead);
		}

		public static Attribute FindInvalidPropertyConstraintAttribute(this PropertyInfo propertyInfo, object propertyValue)
		{
			if (propertyInfo == null)
				throw new ArgumentNullException("propertyInfo");

			return FindInvalidItemConstraintAttribute(
				propertyInfo.PropertyType,
				propertyValue,
				propertyInfo.GetCustomAttributes(typeof(NotNullAttribute), true).FirstOrDefault() as NotNullAttribute,
				propertyInfo.GetCustomAttributes(typeof(NotEmptyAttribute), true).FirstOrDefault() as NotEmptyAttribute,
				propertyInfo.GetCustomAttributes(typeof(InRangeAttribute), true).OfType<InRangeAttribute>());
		}

		public static Attribute FindInvalidParameterConstraintAttribute(this ParameterInfo parameterInfo, object parameterValue)
		{
			if (parameterInfo == null)
				throw new ArgumentNullException("parameterInfo");

			return FindInvalidItemConstraintAttribute(
				parameterInfo.ParameterType,
				parameterValue,
				parameterInfo.GetCustomAttributes(typeof(NotNullAttribute), true).FirstOrDefault() as NotNullAttribute,
				parameterInfo.GetCustomAttributes(typeof(NotEmptyAttribute), true).FirstOrDefault() as NotEmptyAttribute,
				parameterInfo.GetCustomAttributes(typeof(InRangeAttribute), true).OfType<InRangeAttribute>());
		}

		public static Attribute FindInvalidItemConstraintAttribute(
			Type itemType,
			object itemValue,
			NotNullAttribute notNull,
			NotEmptyAttribute notEmpty,
			IEnumerable<InRangeAttribute> inRange)
		{
			if (itemType == null)
				throw new ArgumentNullException("itemType");

			if ((notNull != null || notEmpty != null) && itemValue == null)
				return ((Attribute) notNull) ?? notEmpty;

			if (notEmpty != null && IsEmptyParam(itemValue, itemType, notEmpty.IgnoreWhitespace))
				return notEmpty;

			return inRange.FirstOrDefault(ra => !IsParamInRange(itemValue, itemType, ra.Start, ra.End, ra.Inclusive));
		}

		public static bool IsEmptyParam(object param, Type paramType, bool ignoreWhitespace)
		{
			var nullable = paramType.IsNullable();
			var empty =
				((paramType.IsClass || paramType.IsInterface) && param == null) ||
				(nullable && param == null) ||
				(paramType.IsArray && (param == null || ((Array)param).Length == 0)) ||
				(!nullable && (paramType.IsValueType && !typeof(bool).IsAssignableFrom(paramType) && !paramType.IsEnum)
					&& Activator.CreateInstance(paramType).Equals(param));
			if (!empty && param is string)
				empty = ignoreWhitespace ? ((string)param).IsNullOrWhiteSpace() : ((string)param).IsNullOrEmpty();
			if (!empty && param is ICollection)
				empty = ((ICollection)param).Count == 0;
			if (!empty && param!=null && paramType.IsGenericType && typeof(ISet<>).IsAssignableFrom(paramType.GetGenericTypeDefinition()))
				empty = param.GetMemberValue<int>("Count") == 0;

			return empty;
		}

		public static bool IsParamInRange(object param, Type paramType, object start, object end, bool inclusive)
		{
			if (param == null)
				return true;
			var realType = paramType.IsNullable() ? paramType.GetGenericArguments()[0] : paramType;
			var isString = typeof(string).IsAssignableFrom(realType) || realType.IsEnum;
			if (!realType.IsValueType && !isString)
				return true;

			var realStart = (isString ? start.ConvertSafe<Int32>() : start.ConvertSafe(realType)) as IComparable;
			var realEnd = (isString ? end.ConvertSafe<Int32>() : end.ConvertSafe(realType)) as IComparable;
			if (realStart == null && realEnd == null)
				return true;
			var realParam = (isString ? param.ToStringSafe().Length : param.ConvertSafe(realType)) as IComparable;
			if (realParam == null)
				return false;

			var result = true;
			if (realStart != null)
			{
				var diff = realParam.CompareTo(realStart);
				result = inclusive ? diff >= 0 : diff > 0;
			}
			if (realEnd != null)
			{
				var diff = realParam.CompareTo(realEnd);
				result = result && (inclusive ? diff <= 0 : diff < 0);
			}
			return result;
		}

		public static string TokenValue(this JToken token, string propertyName = null)
		{
			if (token == null)
				throw new ArgumentNullException("token");

			var jValue = token as JValue;
			var jProperty = token as JProperty;
			var jObject = token as JObject;

			if (jValue != null)
			{
				var value = jValue.Value;
				var dateTime = value as DateTime?;
				return dateTime != null 
					? DateTime.SpecifyKind(dateTime.Value, DateTimeKind.Local).ToString("u") 
					: value.ConvertSafe<string>();
			}
			if (jProperty != null)
				return TokenValue(jProperty.Value);

			JToken subToken;
			return jObject != null && propertyName != null && jObject.TryGetValue(propertyName, out subToken)
				? TokenValue(subToken)
				: null;
		}

		public static Type ModelType(
			this JObject obj, 
			string typeProperty = IdentifiedModelConverter.ModelTypePropertyName, 
			string assemblyProperty = IdentifiedModelConverter.ModelAssemblyPropertyName)
		{
			if (obj == null)
				return null;

			var modelTypeProperty = obj.Property(typeProperty);
			var modelAssemblyProperty = obj.Property(assemblyProperty);
			if (modelTypeProperty == null || modelAssemblyProperty == null)
				return null;

			var modelType = (modelTypeProperty.Value.TokenValue() + ", " +
				modelAssemblyProperty.Value.TokenValue()).ConvertSafe<Type>();
			
			return modelType != null && typeof (IIdentifiedModel).IsAssignableFrom(modelType) &&
			       modelType.IsClass && !modelType.IsAbstract
				? modelType
				: null;
		}

		public static string FirstCharToUpper(this string str)
		{
			str = str.TrimSafe();
			if (str.IsNullOrEmpty())
				return str;

			var chars = str.ToCharArray();
			chars[0] = char.ToUpper(chars[0]);
			return new string(chars);
		}

		public static string FirstCharToLower(this string str)
		{
			str = str.TrimSafe();
			if (str.IsNullOrEmpty())
				return str;

			var chars = str.ToCharArray();
			chars[0] = char.ToLower(chars[0]);
			return new string(chars);
		}
	}
}
