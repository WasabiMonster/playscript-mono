//
// XQueryFunctionCliImple.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

//
// Runtime-level (native) implementation of XQuery 1.0 and XPath 2.0 
// Functions implementation. XQueryCliFunction
// See XQuery 1.0 and XPath 2.0 Functions and Operators.
//
#if NET_2_0
using System;
using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Query;
using System.Xml.Schema;
using System.Xml.XPath;

namespace Mono.Xml.XPath2
{
	public class XQueryFunctionCliImpl
	{
		internal static XmlSchemaType XmlTypeFromCliType (Type cliType)
		{
			switch (Type.GetTypeCode (cliType)) {
			case TypeCode.Int32:
				return XmlSchemaSimpleType.XsInt;
			case TypeCode.Decimal:
				return XmlSchemaSimpleType.XsDecimal;
			case TypeCode.Double:
				return XmlSchemaSimpleType.XsDouble;
			case TypeCode.Single:
				return XmlSchemaSimpleType.XsFloat;
			case TypeCode.Int64:
				return XmlSchemaSimpleType.XsLong;
			case TypeCode.Int16:
				return XmlSchemaSimpleType.XsShort;
			case TypeCode.UInt16:
				return XmlSchemaSimpleType.XsUnsignedShort;
			case TypeCode.UInt32:
				return XmlSchemaSimpleType.XsUnsignedInt;
			case TypeCode.String:
				return XmlSchemaSimpleType.XsString;
			case TypeCode.DateTime:
				return XmlSchemaSimpleType.XsDateTime;
			case TypeCode.Boolean:
				return XmlSchemaSimpleType.XsBoolean;
			}
			if (cliType == typeof (XmlQualifiedName))
				return XmlSchemaSimpleType.XsQName;
			return null;
		}

		private static XPathItem ToItem (object arg)
		{
			if (arg == null)
				return null;
			XPathItem item = arg as XPathItem;
			if (item != null)
				return item;
			XPathSequence seq = arg as XPathSequence;
			if (seq != null)
				return seq.MoveNext () ? seq.Current : null;
			return new XPathAtomicValue (arg, XmlTypeFromCliType (arg.GetType ()));
		}

		// Accessors

		public static XmlQualifiedName FnNodeName (XPathNavigator arg)
		{
			if (arg == null)
				return null;

			return arg.LocalName == String.Empty ?
				XmlQualifiedName.Empty :
				new XmlQualifiedName (arg.LocalName, arg.NamespaceURI);
		}

		public static bool FnNilled (XPathNavigator arg)
		{
			if (arg == null)
				throw new XmlQueryException ("Function nilled() does not allow empty sequence parameter.");

			IXmlSchemaInfo info = arg.NodeType == XPathNodeType.Element ? arg.SchemaInfo : null;
			return info != null && info.IsNil;
		}

		public static string FnString (XQueryContext context)
		{
			XPathItem item = context.CurrentItem;
			if (item == null)
				throw new ArgumentException ("FONC0001: undefined context item");
			return FnString (item);
		}

		[MonoTODO]
		public static string FnString (object arg)
		{
			if (arg == null)
				return String.Empty;
			XPathNavigator nav = arg as XPathNavigator;
			if (nav != null)
				return nav.Value;
			// FIXME: it should be exactly the same as "arg cast as xs:string"
			XPathItem item = ToItem (arg);
			return item != null ? XQueryConvert.ItemToString (item) : null;
		}

		[MonoTODO]
		public static XPathAtomicValue FnData (object arg)
		{
			// FIXME: parameter should be object []
			XPathNavigator nav = arg as XPathNavigator;
			if (nav != null) {
				XmlSchemaType st = nav.SchemaInfo != null ? nav.SchemaInfo.SchemaType : null;
				return new XPathAtomicValue (nav.TypedValue, st != null ? st : XmlSchemaComplexType.AnyType);
			}
			else
				return (XPathAtomicValue) arg;
		}

		public static string FnBaseUri (XPathNavigator nav)
		{
			return nav != null ? nav.BaseURI : null;
		}

		public static string FnDocumentUri (XPathNavigator nav)
		{
			if (nav == null)
				return null;
			XPathNavigator root = nav.Clone ();
			root.MoveToRoot ();
			return root.BaseURI;
		}

		// Error

		[MonoTODO]
		public static void FnError (object arg)
		{
			throw new NotImplementedException ();
		}

		// Trace

		[MonoTODO]
		public static object FnTrace (object arg)
		{
			throw new NotImplementedException ();
		}

		// Numeric Operation

		[MonoTODO]
		public static object FnAbs (object arg)
		{
			if (arg is int)
				return System.Math.Abs ((int) arg);
			if (arg is long)
				return System.Math.Abs ((long) arg);
			else if (arg is decimal)
				return System.Math.Abs ((decimal) arg);
			else if (arg is double)
				return System.Math.Abs ((double) arg);
			else if (arg is float)
				return System.Math.Abs ((float) arg);
			else if (arg is short)
				return System.Math.Abs ((short) arg);
			else if (arg is uint || arg is ulong || arg is ushort)
				return arg;
			return null;
		}

		[MonoTODO]
		public static object FnCeiling (object arg)
		{
			if (arg is decimal) {
				decimal d = (decimal) arg;
				decimal d2 = Decimal.Floor (d);
				return d2 != d ? d2 + 1 : d2;
			}
			else if (arg is double || arg is float)
				return System.Math.Ceiling ((double) arg);
			else if (arg is int || arg is long || arg is short || arg is uint || arg is ulong || arg is ushort)
				return arg;
			return null;
		}

		[MonoTODO]
		public static object FnFloor (object arg)
		{
			if (arg is decimal)
				return Decimal.Floor ((decimal) arg);
			else if (arg is double || arg is float)
				return System.Math.Floor ((double) arg);
			else if (arg is int || arg is long || arg is short || arg is uint || arg is ulong || arg is ushort)
				return arg;
			return null;
		}

		[MonoTODO]
		public static object FnRound (object arg)
		{
			if (arg is decimal)
				return Decimal.Round ((decimal) arg, 0);
			else if (arg is double || arg is float)
				return System.Math.Round ((double) arg);
			else if (arg is int || arg is long || arg is short || arg is uint || arg is ulong || arg is ushort)
				return arg;
			return null;
		}

		[MonoTODO]
		public static object FnRoundHalfToEven (object arg)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static string FnCodepointsToString (int [] arg)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static int [] FnStringToCodepoints (string arg)
		{
			throw new NotImplementedException ();
		}

		public static int FnCompare (XQueryContext ctx, string s1, string s2)
		{
			return FnCompare (s1, s2, ctx.DefaultCollation);
		}

		public static int FnCompare (XQueryContext ctx, string s1, string s2, string collation)
		{
			return FnCompare (s1, s2, ctx.GetCulture (collation));
		}

		private static int FnCompare (string s1, string s2, CultureInfo ci)
		{
			return ci.CompareInfo.Compare (s1, s2);
		}

		public static string FnConcat (object o1, object o2)
		{
			return String.Concat (o1, o2);
		}

		public static string FnStringJoin (string [] strings, string separator)
		{
			return String.Join (separator, strings);
		}

		public static string FnSubstring (string src, double loc)
		{
			return src.Substring ((int) loc);
		}

		public static string FnSubstring (string src, double loc, double length)
		{
			return src.Substring ((int) loc, (int) length);
		}

		public static int FnStringLength (XQueryContext ctx)
		{
			return FnString (ctx).Length;
		}

		public static int FnStringLength (string s)
		{
			return s.Length;
		}

		public static string FnNormalizeSpace (XQueryContext ctx)
		{
			return FnNormalizeSpace (FnString (ctx));
		}

		[MonoTODO]
		public static string FnNormalizeSpace (string s)
		{
			throw new NotImplementedException ();
		}

		public static string FnNormalizeUnicode (string arg)
		{
			return FnNormalizeUnicode (arg, "NFC");
		}

		[MonoTODO]
		public static string FnNormalizeUnicode (string arg, string normalizationForm)
		{
			throw new NotImplementedException ();
		}

		public static string FnUpperCase (string arg)
		{
			// FIXME: supply culture
			return arg.ToUpper ();
		}

		public static string FnLowerCase (string arg)
		{
			// FIXME: supply culture
			return arg.ToLower ();
		}

		public static string FnTranslate (string arg, string mapString, string transString)
		{
			return arg == null ? null : arg.Replace (mapString, transString);
		}

		[MonoTODO]
		public static string FnEscapeUri (string uriPart, bool escapeReserved)
		{
			throw new NotImplementedException ();
		}

		public static bool FnContains (XQueryContext ctx, string arg1, string arg2)
		{
			return FnContains (arg1, arg2, ctx.DefaultCollation);
		}

		public static bool FnContains (XQueryContext ctx, string arg1, string arg2, string collation)
		{
			return FnContains (arg1, arg2, ctx.GetCulture (collation));
		}

		private static bool FnContains (string arg1, string arg2, CultureInfo ci)
		{
			if (arg1 == null)
				arg1 = String.Empty;
			if (arg2 == null)
				arg2 = String.Empty;
			if (arg2 == String.Empty)
				return true;
			return ci.CompareInfo.IndexOf (arg1, arg2) >= 0;
		}

		public static bool FnStartsWith (XQueryContext ctx, string arg1, string arg2)
		{
			return FnStartsWith (arg1, arg2, ctx.DefaultCollation);
		}

		public static bool FnStartsWith (XQueryContext ctx, string arg1, string arg2, string collation)
		{
			return FnStartsWith (arg1, arg2, ctx.GetCulture (collation));
		}

		private static bool FnStartsWith (string arg1, string arg2, CultureInfo ci)
		{
			return ci.CompareInfo.IsPrefix (arg1, arg2);
		}

		public static bool FnEndsWith (XQueryContext ctx, string arg1, string arg2)
		{
			return FnEndsWith (arg1, arg2, ctx.DefaultCollation);
		}

		public static bool FnEndsWith (XQueryContext ctx, string arg1, string arg2, string collation)
		{
			return FnEndsWith (arg1, arg2, ctx.GetCulture (collation));
		}

		private static bool FnEndsWith (string arg1, string arg2, CultureInfo ci)
		{
			return ci.CompareInfo.IsSuffix (arg1, arg2);
		}

		public static string FnSubstringBefore (XQueryContext ctx, string arg1, string arg2)
		{
			return FnSubstringBefore (arg1, arg2, ctx.DefaultCollation);
		}

		public static string FnSubstringBefore (XQueryContext ctx, string arg1, string arg2, string collation)
		{
			return FnSubstringBefore (arg1, arg2, ctx.GetCulture (collation));
		}

		private static string FnSubstringBefore (string arg1, string arg2, CultureInfo ci)
		{
			int index = ci.CompareInfo.IndexOf (arg1, arg2);
			return arg1.Substring (0, index);
		}

		public static string FnSubstringAfter (XQueryContext ctx, string arg1, string arg2)
		{
			return FnSubstringAfter (arg1, arg2, ctx.DefaultCollation);
		}

		public static string FnSubstringAfter (XQueryContext ctx, string arg1, string arg2, string collation)
		{
			return FnSubstringAfter (arg1, arg2, ctx.GetCulture (collation));
		}

		private static string FnSubstringAfter (string arg1, string arg2, CultureInfo ci)
		{
			int index = ci.CompareInfo.IndexOf (arg1, arg2);
			return arg1.Substring (index);
		}

		public static bool FnMatches (string input, string pattern)
		{
			return new Regex (pattern).IsMatch (input);
		}

		[MonoTODO]
		public static bool FnMatches (string input, string pattern, string flags)
		{
			throw new NotImplementedException ();
		}

		public static string FnReplace (string input, string pattern, string replace)
		{
			return new Regex (pattern).Replace (input, replace);
		}

		[MonoTODO]
		public static string FnReplace (string input, string pattern, string replace, string flags)
		{
			throw new NotImplementedException ();
		}

		public static string [] FnTokenize (string input, string pattern)
		{
			return new Regex (pattern).Split (input);
		}

		[MonoTODO]
		public static string [] FnTokenize (string input, string pattern, string flags)
		{
			throw new NotImplementedException ();
		}

		public static string FnResolveUri (XQueryContext ctx, string relUri)
		{
			return new Uri (new Uri (ctx.StaticContext.BaseUri), relUri).ToString ();
		}

		public static string FnResolveUri (string relUri, string baseUri)
		{
			return new Uri (new Uri (baseUri), relUri).ToString ();
		}

		public static object FnTrue ()
		{
			return true;
		}

		public static object FnFalse ()
		{
			return false;
		}

		public static object FnNot (bool value)
		{
			return !value;
		}

		// FIXME: add a bunch of annoying datetime functions

		[MonoTODO]
		public static object FnResolveQName ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static object FnExpandQName ()
		{
			throw new NotImplementedException ();
		}

		public static string FnLocalNameFromQName (XmlQualifiedName name)
		{
			return name != null ? name.Name : null;
		}

		public static object FnNamespaceUriFromQName (XmlQualifiedName name)
		{
			return name != null ? name.Namespace : null;
		}

		public static object FnNamespaceUriForPrefix (XQueryContext context, string prefix)
		{
			return prefix != null ? context.LookupNamespace (prefix) : null;
		}

		public static string [] FnInScopePrefixes (XQueryContext context)
		{
			IDictionary dict = context.GetNamespacesInScope (XmlNamespaceScope.ExcludeXml);
			ArrayList keys = new ArrayList (dict.Keys);
			return keys.ToArray (typeof (string)) as string [];
		}

		public static string FnName (XPathNavigator nav)
		{
			return nav != null ? nav.Name : null;
		}

		public static string FnLocalName (XPathNavigator nav)
		{
			return nav != null ? nav.LocalName : null;
		}

		public static string FnNamespaceUri (XPathNavigator nav)
		{
			return nav != null ? nav.NamespaceURI : null;
		}

		public static double FnNumber (XQueryContext ctx)
		{
			return FnNumber (ctx.CurrentItem);
		}

		public static double FnNumber (object arg)
		{
			if (arg == null)
				throw new XmlQueryException ("Context item could not be ndetermined during number() evaluation.");
			XPathItem item = ToItem (arg);
			return XQueryConvert.ItemToDouble (item);
		}

		public static bool FnLang (XQueryContext ctx, string testLang)
		{
			return FnLang (testLang, ctx.CurrentNode);
		}

		public static bool FnLang (string testLang, XPathNavigator node)
		{
			return testLang == node.XmlLang;
		}

		public static XPathNavigator FnRoot (XQueryContext ctx)
		{
			if (ctx.CurrentItem == null)
				throw new XmlQueryException ("FONC0001: Undefined context item.");
			if (ctx.CurrentNode == null)
				throw new XmlQueryException ("FOTY0011: Context item is not a node.");
			return FnRoot (ctx.CurrentNode);
		}

		public static XPathNavigator FnRoot (XPathNavigator node)
		{
			if (node == null)
				return null;
			XPathNavigator root = node.Clone ();
			root.MoveToRoot ();
			return root;
		}

		public static bool FnBoolean (IEnumerator e)
		{
			if (!e.MoveNext ())
				return false;
			XPathItem item = e.Current as XPathItem;
			if (e.MoveNext ())
				return true;
			return XQueryConvert.ItemToBoolean (item);
		}

		public static IEnumerable FnIndexOf (XQueryContext ctx, IEnumerable e, XPathItem item)
		{
			return FnIndexOf (e, item, ctx.DefaultCollation);
		}

		public static IEnumerable FnIndexOf (IEnumerable items, XPathItem item, CultureInfo ci)
		{
			ArrayList al = new ArrayList ();
			IEnumerator e = items.GetEnumerator ();
			for (int i = 0; e.MoveNext (); i++) {
				XPathItem iter = e.Current as XPathItem;
				if (iter.XmlType.TypeCode == XmlTypeCode.String) {
					if (ci.CompareInfo.Compare (iter.Value, item.Value) == 0)
						al.Add (i);
				}
				else {
					IComparable ic = (IComparable) iter.TypedValue;
					if (ic.CompareTo ((IComparable) item.TypedValue) == 0)
						al.Add (i);
				}
			}
			return al;
		}

		public static bool FnEmpty (IEnumerable e)
		{
			if (e is XPathEmptySequence)
				return true;
			return !e.GetEnumerator ().MoveNext ();
		}

		public static object FnExists (IEnumerable e)
		{
			if (e is XPathEmptySequence)
				return false;
			return e.GetEnumerator ().MoveNext ();
		}

		[MonoTODO]
		public static object FnDistinctValues (XQueryContext ctx, IEnumerable items)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static object FnDistinctValues (XQueryContext ctx, IEnumerable items, string collation)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static IEnumerable FnInsertBefore (IEnumerable target, int position, IEnumerable inserts)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static IEnumerable FnRemove (IEnumerable target, int position)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static IEnumerable FnReverse (IEnumerable arg)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static object FnSubsequence (IEnumerable sourceSeq, double startingLoc)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static object FnSubsequence (IEnumerable sourceSeq, double startingLoc, double length)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		// Basically it should be optimized by XQueryASTCompiler
		public static IEnumerable FnUnordered (IEnumerable e)
		{
			return e;
		}

		[MonoTODO]
		public static object FnZeroOrMore (IEnumerable e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static object FnOneOrMore (IEnumerable e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static object FnExactlyOne (IEnumerable e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static object FnDeepEqual (XQueryContext ctx, IEnumerable p1, IEnumerable p2)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static object FnDeepEqual (XQueryContext ctx, IEnumerable p1, IEnumerable p2, string collation)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static object FnCount (IEnumerable e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static object FnAvg (IEnumerable e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static object FnMax (XQueryContext ctx, IEnumerable e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static object FnMax (XQueryContext ctx, IEnumerable e, string collation)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static object FnMin (XQueryContext ctx, IEnumerable e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static object FnMin (XQueryContext ctx, IEnumerable e, string collation)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static object FnSum (IEnumerable e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static object FnSum (IEnumerable e, XPathItem zero)
		{
			throw new NotImplementedException ();
		}

		public static XPathNavigator FnId (XQueryContext ctx, string id)
		{
			return FnId (id, ctx.CurrentNode);
		}

		public static XPathNavigator FnId (string id, XPathNavigator nav)
		{
			XPathNavigator node = nav.Clone ();
			return node.MoveToId (id) ? node : null;
		}

		[MonoTODO]
		public static object FnIdRef (XQueryContext ctx, string arg)
		{
			return FnIdRef (arg, ctx.CurrentNode);
		}

		[MonoTODO]
		public static object FnIdRef (string arg, XPathNavigator node)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static object FnDoc (XQueryContext ctx, string uri)
		{
			throw new NotImplementedException ();
		}

		public static IEnumerable FnCollection (XQueryContext ctx, string name)
		{
			return ctx.ResolveCollection (name);
		}

		public static int FnPosition (XQueryContext ctx)
		{
			return ctx.CurrentSequence.Position;
		}

		public static int FnLast (XQueryContext ctx)
		{
			return ctx.CurrentSequence.Count;
		}

		public static DateTime FnCurrentDateTime ()
		{
			return DateTime.Now;
		}

		public static DateTime FnCurrentDate ()
		{
			return DateTime.Today;
		}

		public static DateTime FnCurrentTime ()
		{
			return new DateTime (DateTime.Now.TimeOfDay.Ticks);
		}

		[MonoTODO]
		public static object FnDefaultCollation ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static object FnImplicitTimeZone ()
		{
			throw new NotImplementedException ();
		}
	}
}
#endif
