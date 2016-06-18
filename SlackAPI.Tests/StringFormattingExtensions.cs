using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace SlackAPI.Tests
{
	public static class StringFormattingExtensions
	{
		/// <summary>
		/// A String.Format that can use named parameters which refer to properties on an object
		/// </summary>
		/// <param name="format">A format string using "{name}" or "{#name}" to refer to named properties</param>
		/// <param name="source">An object whose properties are substituted into the format</param>
		/// <param name="defaults">Delegate to provide values for params not available from <paramref name="source"/></param>
		/// <returns></returns>
		public static string FormatWith(this string format, object source, Func<string, string> defaults = null)
		{
			if (format == null)
				throw new ArgumentNullException("format");

			var result = new StringBuilder(format.Length * 2);

			using (var reader = new StringReader(format))
			{
				var expression = new StringBuilder();

				var state = State.OutsideExpression;
				do
				{
					int c;
					switch (state)
					{
						case State.OutsideExpression:
							c = reader.Read();
							switch (c)
							{
								case -1:
									state = State.End;
									break;
								case '{':
									state = State.OnOpenBracket;
									break;
								case '}':
									state = State.OnCloseBracket;
									break;
								default:
									result.Append((char)c);
									break;
							}
							break;
						case State.OnOpenBracket:
							c = reader.Read();
							switch (c)
							{
								case -1:
									throw new FormatException();
								case '{':
									result.Append('{');
									state = State.OutsideExpression;
									break;
								case '#': break;
								default:
									expression.Append((char)c);
									state = State.InsideExpression;
									break;
							}
							break;
						case State.InsideExpression:
							c = reader.Read();
							switch (c)
							{
								case -1:
									throw new FormatException();
								case '}':
									result.Append(EvalExpression(source, expression.ToString(), defaults));
									expression.Length = 0;
									state = State.OutsideExpression;
									break;
								default:
									expression.Append((char)c);
									break;
							}
							break;
						case State.OnCloseBracket:
							c = reader.Read();
							switch (c)
							{
								case '}':
									result.Append('}');
									state = State.OutsideExpression;
									break;
								default:
									throw new FormatException();
							}
							break;
						default:
							throw new InvalidOperationException("Invalid state.");
					}
				} while (state != State.End);
			}

			return result.ToString();
		}

		private static string EvalExpression(object source, string expression, Func<string, string> defaults)
		{
			string format = string.Empty;

			int colonIndex = expression.IndexOf(':');
			if (colonIndex > 0)
			{
				format = expression.Substring(colonIndex + 1);
				expression = expression.Substring(0, colonIndex);
			}

			string result;
			if (string.IsNullOrEmpty(format))
				result = (DataBinder.Eval(source, expression) ?? string.Empty).ToString();
			else
				result = DataBinder.Eval(source, expression, "{0:" + format + "}");

			if (string.IsNullOrEmpty(result))
				result = defaults(expression);
			if (string.IsNullOrEmpty(result))
				return string.Empty;
			return result;
		}

		private enum State
		{
			OutsideExpression,
			OnOpenBracket,
			InsideExpression,
			OnCloseBracket,
			End
		}

		/// <summary>
		/// Essentially a copy of System.Web.UI.DataBinder
		/// <para>
		/// Provides support for rapid application development (RAD) designers to generate and parse data-binding expression syntax. This class cannot be inherited.
		/// </para>
		/// </summary>
		public static class DataBinder
		{
			private static readonly char[] expressionPartSeparator = new char[] { '.' };
			private static readonly char[] indexExprStartChars = new char[] { '[', '(' };
			private static readonly char[] indexExprEndChars = new char[] { ']', ')' };
			private static readonly ConcurrentDictionary<Type, PropertyDescriptorCollection> propertyCache = new ConcurrentDictionary<Type, PropertyDescriptorCollection>();
			private static bool enableCaching = true;

			/// <summary>
			/// Gets or sets a value that indicates whether data caching is enabled at run time.
			/// </summary>
			/// 
			/// <returns>
			/// true if caching is enabled for the <see cref="T:System.Web.UI.DataBinder"/> class; otherwise, false.
			/// </returns>
			public static bool EnableCaching
			{
				[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
				get
				{
					return enableCaching;
				}
				set
				{
					enableCaching = value;
					if (value)
						return;
					propertyCache.Clear();
				}
			}

			/// <summary>
			/// Evaluates data-binding expressions at run time.
			/// </summary>
			/// 
			/// <returns>
			/// An <see cref="T:System.Object"/> instance that results from the evaluation of the data-binding expression.
			/// </returns>
			/// <param name="container">The object reference against which the expression is evaluated. This must be a valid object identifier in the page's specified language. </param><param name="expression">The navigation path from the <paramref name="container"/> object to the public property value to be placed in the bound control property. This must be a string of property or field names separated by periods, such as Tables[0].DefaultView.[0].Price in C# or Tables(0).DefaultView.(0).Price in Visual Basic. </param><exception cref="T:System.ArgumentNullException"><paramref name="expression"/> is null or is an empty string after trimming.</exception>
			public static object Eval(object container, string expression)
			{
				if (expression == null)
					throw new ArgumentNullException("expression");
				expression = expression.Trim();
				if (expression.Length == 0)
					throw new ArgumentNullException("expression");
				if (container == null)
					return (object)null;
				string[] expressionParts = expression.Split(expressionPartSeparator);
				return Eval(container, expressionParts);
			}

			/// <summary>
			/// Evaluates data-binding expressions at run time and formats the result as a string.
			/// </summary>
			/// 
			/// <returns>
			/// A <see cref="T:System.String"/> object that results from evaluating the data-binding expression and converting it to a string type.
			/// </returns>
			/// <param name="container">The object reference against which the expression is evaluated. This must be a valid object identifier in the page's specified language. </param><param name="expression">The navigation path from the <paramref name="container"/> object to the public property value to be placed in the bound control property. This must be a string of property or field names separated by periods, such as Tables[0].DefaultView.[0].Price in C# or Tables(0).DefaultView.(0).Price in Visual Basic. </param><param name="format">A .NET Framework format string (like those used by <see cref="M:System.String.Format(System.String,System.Object)"/>) that converts the <see cref="T:System.Object"/> instance returned by the data-binding expression to a <see cref="T:System.String"/> object. </param>
			public static string Eval(object container, string expression, string format)
			{
				object obj = Eval(container, expression);
				if (obj == null || obj == DBNull.Value)
					return string.Empty;
				if (string.IsNullOrEmpty(format))
					return obj.ToString();
				else
					return string.Format(format, obj);
			}

			internal static PropertyDescriptorCollection GetPropertiesFromCache(object container)
			{
				if (!EnableCaching || container is ICustomTypeDescriptor)
					return TypeDescriptor.GetProperties(container);
				Type type = container.GetType();
				return propertyCache.GetOrAdd(type, t => TypeDescriptor.GetProperties(t));
			}

			/// <summary>
			/// Retrieves the value of the specified property of the specified object.
			/// </summary>
			/// 
			/// <returns>
			/// The value of the specified property.
			/// </returns>
			/// <param name="container">The object that contains the property. </param><param name="propName">The name of the property that contains the value to retrieve. </param><exception cref="T:System.ArgumentNullException"><paramref name="container"/> is null.-or- <paramref name="propName"/> is null or an empty string (""). </exception><exception cref="T:System.Web.HttpException">The object in <paramref name="container"/> does not have the property specified by <paramref name="propName"/>. </exception>
			public static object GetPropertyValue(object container, string propName)
			{
				if (container == null)
					throw new ArgumentNullException("container");
				if (string.IsNullOrEmpty(propName))
					throw new ArgumentNullException("propName");

				var propertyDescriptor = GetPropertiesFromCache(container).Find(propName, true);
				if (propertyDescriptor != null)
					return propertyDescriptor.GetValue(container);

				var d = container as IDictionary<string, string>;
				if (d != null)
				{
					string value;
					if (d.TryGetValue(propName, out value))
						return value;
					else
						return string.Empty;
				}

				var dmop = container as IDynamicMetaObjectProvider;
				if (dmop != null)
				{
					dynamic expando = container;
					return expando[propName];
				}

				return string.Empty;
			}

			/// <summary>
			/// Retrieves the value of the specified property of the specified object, and then formats the results.
			/// </summary>
			/// 
			/// <returns>
			/// The value of the specified property in the format specified by <paramref name="format"/>.
			/// </returns>
			/// <param name="container">The object that contains the property. </param><param name="propName">The name of the property that contains the value to retrieve. </param><param name="format">A string that specifies the format in which to display the results. </param><exception cref="T:System.ArgumentNullException"><paramref name="container"/> is null.- or - <paramref name="propName"/> is null or an empty string (""). </exception><exception cref="T:System.Web.HttpException">The object in <paramref name="container"/> does not have the property specified by <paramref name="propName"/>. </exception>
			public static string GetPropertyValue(object container, string propName, string format)
			{
				object propertyValue = GetPropertyValue(container, propName);
				if (propertyValue == null || propertyValue == DBNull.Value)
					return string.Empty;
				if (string.IsNullOrEmpty(format))
					return propertyValue.ToString();
				else
					return string.Format(format, propertyValue);
			}

			/// <summary>
			/// Retrieves the value of a property of the specified container and navigation path.
			/// </summary>
			/// 
			/// <returns>
			/// An object that results from the evaluation of the data-binding expression.
			/// </returns>
			/// <param name="container">The object reference against which <paramref name="expr"/> is evaluated. This must be a valid object identifier in the specified language for the page.</param><param name="expr">The navigation path from the <paramref name="container"/> object to the public property value to place in the bound control property. This must be a string of property or field names separated by periods, such as Tables[0].DefaultView.[0].Price in C# or Tables(0).DefaultView.(0).Price in Visual Basic.</param><exception cref="T:System.ArgumentNullException"><paramref name="container"/> is null.- or -<paramref name="expr"/> is null or an empty string ("").</exception><exception cref="T:System.ArgumentException"><paramref name="expr"/> is not a valid indexed expression.- or -<paramref name="expr"/> does not allow indexed access.</exception>
			public static object GetIndexedPropertyValue(object container, string expr)
			{
				if (container == null)
					throw new ArgumentNullException("container");
				if (string.IsNullOrEmpty(expr))
					throw new ArgumentNullException("expr");
				object obj1 = (object)null;
				bool flag = false;
				int length = expr.IndexOfAny(indexExprStartChars);
				int num = expr.IndexOfAny(indexExprEndChars, length + 1);
				if (length < 0 || num < 0 || num == length + 1)
					throw new ArgumentException(string.Format("DataBinder_Invalid_Indexed_Expr: '{0}'", expr));
				else
				{
					string propName = (string)null;
					object obj2 = (object)null;
					string s = expr.Substring(length + 1, num - length - 1).Trim();
					if (length != 0)
						propName = expr.Substring(0, length);
					if (s.Length != 0)
					{
						if ((int)s[0] == 34 && (int)s[s.Length - 1] == 34 || (int)s[0] == 39 && (int)s[s.Length - 1] == 39)
							obj2 = (object)s.Substring(1, s.Length - 2);
						else if (char.IsDigit(s[0]))
						{
							int result;
							flag = int.TryParse(s, NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowLeadingSign, (IFormatProvider)CultureInfo.InvariantCulture, out result);
							obj2 = !flag ? (object)s : (object)result;
						}
						else
							obj2 = (object)s;
					}
					if (obj2 == null)
						throw new ArgumentException(string.Format("DataBinder_Invalid_Indexed_Expr: '{0}'", expr));
					else
					{
						object obj3 = propName == null || propName.Length == 0 ? container : GetPropertyValue(container, propName);
						if (obj3 != null)
						{
							Array array = obj3 as Array;
							if (array != null && flag)
								obj1 = array.GetValue((int)obj2);
							else if (obj3 is IList && flag)
							{
								obj1 = ((IList)obj3)[(int)obj2];
							}
							else
							{
								PropertyInfo property = obj3.GetType().GetProperty(
									"Item",
									BindingFlags.Instance | BindingFlags.Public,
									(Binder)null,
									(Type)null,
									new Type[] { obj2.GetType() },
									(ParameterModifier[])null
								);
								if (property != (PropertyInfo)null)
									obj1 = property.GetValue(obj3, new object[] { obj2 });
								else
									throw new ArgumentException(string.Format("DataBinder_No_Indexed_Accessor: ", obj3.GetType().FullName));
							}
						}
						return obj1;
					}
				}
			}

			/// <summary>
			/// Retrieves the value of the specified property for the specified container, and then formats the results.
			/// </summary>
			/// <returns>
			/// The value of the specified property in the format specified by <paramref name="format"/>.
			/// </returns>
			/// <param name="container">The object reference against which the expression is evaluated. This must be a valid object identifier in the specified language for the page.</param><param name="propName">The name of the property that contains the value to retrieve.</param><param name="format">A string that specifies the format in which to display the results.</param>
			public static string GetIndexedPropertyValue(object container, string propName, string format)
			{
				object indexedPropertyValue = GetIndexedPropertyValue(container, propName);
				if (indexedPropertyValue == null || indexedPropertyValue == DBNull.Value)
					return string.Empty;
				if (string.IsNullOrEmpty(format))
					return indexedPropertyValue.ToString();
				else
					return string.Format(format, indexedPropertyValue);
			}

			internal static bool IsNull(object value)
			{
				return value == null || Convert.IsDBNull(value);
			}

			private static object Eval(object container, string[] expressionParts)
			{
				object container1 = container;
				for (int index = 0; index < expressionParts.Length && container1 != null; ++index)
				{
					string str = expressionParts[index];
					container1 = str.IndexOfAny(indexExprStartChars) >= 0 ? GetIndexedPropertyValue(container1, str) : GetPropertyValue(container1, str);
				}
				return container1;
			}
		}
	}
}