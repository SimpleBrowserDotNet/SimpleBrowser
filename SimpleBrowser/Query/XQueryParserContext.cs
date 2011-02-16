using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SimpleBrowser.Query
{
	public class XQueryParserContext
	{
		private SelectorParserCatalog _catalog;
		private string _query;
		private Stack<char> _groupStack = new Stack<char>();

		public XQueryParserContext(SelectorParserCatalog catalog, string query)
		{
			if(query == null)
				throw new ArgumentNullException("query");
			if(catalog == null)
				throw new ArgumentNullException("catalog");
			_query = query.Trim();
			_catalog = catalog;
		}

		public IXQuerySelector MatchNextSelector()
		{
			return Catalog.GetNextSelector(this);
		}

		public SelectorParserCatalog Catalog
		{
			get { return _catalog; }
		}

		public string Query
		{
			get { return _query; }
		}

		public int Index { get; set; }

		public bool EndOfQuery { get { return Index >= Query.Length; } }
		public char CharAtIndex { get { return Query[Index]; } }

		public bool ReadWhiteSpace()
		{
			int n = Index;
			while(!EndOfQuery && char.IsWhiteSpace(CharAtIndex))
				Index++;
			return n != Index;
		}

		public char ReadChar()
		{
			return Query[Index++];
		}

		public bool ReadCharIf(char c)
		{
			if(CharAtIndex == c)
			{
				Index++;
				return true;
			}
			return false;
		}

		public string ReadLetters()
		{
			var sb = new StringBuilder();
			while(!EndOfQuery && char.IsLetter(_query[Index]))
				sb.Append(_query[Index++]);
			if(sb.Length == 0)
				throw new XQueryException("The query seems to be missing one or more letters at position " + Index, _query, Index, 1);
			return sb.ToString();
		}

		public string ReadDigits()
		{
			var sb = new StringBuilder();
			while(!EndOfQuery && char.IsDigit(_query[Index]))
				sb.Append(_query[Index++]);
			if(sb.Length == 0)
				throw new XQueryException("The query seems to be missing one or more digits at position " + Index, _query, Index, 1);
			return sb.ToString();
		}

		static readonly Regex RxReadWord = new Regex(@"^[A-Za-z_][A-Za-z0-9_]*");
		static readonly Regex RxReadWordWithHyphens = new Regex(@"^[A-Za-z_][A-Za-z0-9_\-]*");
		public string ReadWord(bool allowHyphens)
		{
			if(!EndOfQuery)
			{
				var match = (allowHyphens ? RxReadWordWithHyphens : RxReadWord).Match(_query.Substring(Index));
				if(match.Success)
				{
					Index += match.Length;
					return match.Value;
				}
			}
			throw new XQueryException("The query seems to be missing one or more letters at position " + Index, _query, Index, 1);
		}

		static readonly Regex RxReadTraversalOperator = new Regex(@"^(\>|\<\<?)", RegexOptions.ExplicitCapture);
		public string ReadTraversalOperator()
		{
			if(!EndOfQuery)
			{
				var match = RxReadTraversalOperator.Match(_query.Substring(Index));
				if(match.Success)
				{
					Index += match.Length;
					return match.Value;
				}
			}
			return null;
		}

		public void Read(char c)
		{
			if(Index + 1 <= _query.Length)
				if(_query[Index] == c)
				{
					Index++;
					return;
				}
			throw new XQueryException("Unexpected character found when trying to read '" + c + "' at position " + Index + " in query", _query, Index, 1);
		}

		public void Read(string s)
		{
			if(Index + s.Length <= _query.Length)
				if(_query.Substring(Index, s.Length) == s)
				{
					Index += s.Length;
					return;
				}
			throw new XQueryException("Unexpected characters found when trying to read '" + s + "' at position " + Index + " in query", _query, Index, s.Length);
		}

		public void AssertNotEndOfQuery()
		{
			if(EndOfQuery)
				throw new XQueryException("The query expression appears to have been cut off prematurely", _query, _query.Length, 0);
		}

		public void ReadGroupStart()
		{
			var c = ReadChar();
			switch(c)
			{
				case '[': c = ']'; break;
				case '(': c = ')'; break;
				case '{': c = '}'; break;
				default: throw new XQueryException("The group opening character '" + c + "' is not recognised", _query, Index, 1);
			}
			_groupStack.Push(c);
		}

		public void ReadGroupEnd()
		{
			if(CharAtIndex != _groupStack.LastOrDefault())
				throw new XQueryException("Expected a group closing character '" + _groupStack.Last() + "'", _query, Index, 1);
			ReadChar();
			_groupStack.Pop();
		}

		static readonly Regex RxReadString = new Regex(@"^[A-Za-z0-9_]+");
		public string ReadString()
		{
			var c = CharAtIndex;
			StringBuilder sb = new StringBuilder();
			if(c != '\'' && c != '"')
			{
				var match = RxReadString.Match(_query.Substring(Index));
				if(match.Success)
				{
					Index += match.Length;
					return match.Value;
				}
				throw new XQueryException("Unexpected character in query expression at position " + Index, _query, Index, 1);
			}
			ReadChar();
			bool escaped = false;
			while(true)
			{
				AssertNotEndOfQuery();
				var k = ReadChar();
				if(!escaped)
					if(k == c)
						return sb.ToString();
					else if(k == '\\')
						escaped = true;
					else
						sb.Append(k);
				else
				{
					sb.Append(k);
					escaped = false;
				}
			}
		}

		static readonly string[] ComparisonOperators = new[] { "=", "^=", "$=", "~=", "*=", ">", "<" };
		static readonly char[] ComparisonOperatorChars = "=^$~*><".ToArray();
		public string ReadComparisonOperator()
		{
			var start = Index;
			bool isNot = CharAtIndex == '!';
			if(isNot)
				ReadChar();
			StringBuilder sb = new StringBuilder();
			while(!EndOfQuery)
			{
				if(!ComparisonOperatorChars.Contains(CharAtIndex))
					break;
				sb.Append(ReadChar());
			}
			string op = sb.ToString();
			if(isNot) sb.Insert(0, '!');
			if(ComparisonOperators.Contains(op))
				return sb.ToString();

			throw new XQueryException("An unexpected comparison operator was found: " + sb, _query, start, sb.Length);
		}

		public delegate void AssertParamValidDelegate(string val, int charIndex, int length, XQueryParserContext context);
		public string[] ParseParameterGroup(string filterName, int? minPrms, int? maxPrms, params AssertParamValidDelegate[] assertParamValid)
		{
			AssertNotEndOfQuery();
			if(CharAtIndex != '(')
				throw new XQueryException("Expected an opening bracket at position " + Index, _query, Index, 1);
			
			int start = Index, length = 0;
			ReadGroupStart();

			var prms = new List<string>();
			while(true)
			{
				ReadWhiteSpace();
				AssertNotEndOfQuery();

				if(CharAtIndex == ')')
				{
					ReadGroupEnd();
					length = Index - start;
					break;
				}

				if(prms.Count > 0)
					Read(',');
				ReadWhiteSpace();

				int prmstart = Index;
				string prm = ReadString();
				int prmlen = Index - 1 - prmstart;
				if(assertParamValid.Length > prms.Count)
					assertParamValid[prms.Count](prm, prmstart, prmlen, this);
				prms.Add(prm);
			}
			if(prms.Count < (minPrms ?? 0) || prms.Count > (maxPrms ?? int.MaxValue))
			{
				string msg;
				if(minPrms.HasValue && maxPrms.HasValue && maxPrms.Value == minPrms.Value)
					msg = "The '" + filterName + "' filter requires " + minPrms + " parameters";
				else if(minPrms.HasValue && maxPrms.HasValue)
					msg = "The '" + filterName + "' filter requires between " + minPrms + " and " + maxPrms + " parameters";
				else if(minPrms.HasValue)
					msg = "The '" + filterName + "' filter requires at least " + minPrms + " parameters";
				else
					msg = "The '" + filterName + "' filter can have no more than " + maxPrms + " parameters";
				throw new XQueryException(msg, _query, start, length);
			}
			return prms.ToArray();
		}
	}
}