using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Xml.Linq;
using System;

namespace SimpleBrowser.Query.Selectors
{
    /// <summary>
    ///  For spec on valid CSS attribute selectors, check http://www.w3.org/TR/selectors/#attribute-selectors
    /// </summary>
	public class AttributeSelector : IXQuerySelector
	{
		private readonly string _name;
        private readonly string _op;
        private readonly string _value;

		public AttributeSelector(string name, string op, string value)
		{
			_name = name.ToLower();
            _op = op;
            _value = RemoveQuotes(value.Trim());
		}

        private string RemoveQuotes(string quoted)
        {
			if (quoted == "") return "";
            if (quoted[0] == quoted[quoted.Length - 1])
            {
                return quoted.Trim('\'', '"');
            }
            return quoted;
        }

		public bool IsTransposeSelector { get { return false; } }

		public void Execute(XQueryResultsContext context)
		{
            Func<string, bool> complies;
            switch (_op)
            {
				case "":
				case null:
                    complies = (v) => (v != null);
                    break;
                case "=":
                    complies = (v) => (v == _value);
                    break;
                case "!=":
                    // for matching not-is
                    complies = (v) => (v == null || _value!=v);
                    break;
                case "~=":
                    // value is whitespace separated words and we need to match only one
                    complies = (v) =>
                        {
                            if (v == null) return false;
                            var values = v.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                            return values.Contains(_value);
                        };
                    break;
                case "|=":
                    // for matching lang values like en-US with |=en
                    complies = (v) =>
                    {
                        if (v == null) return false;
                        if(v == _value)return true;
                        if (v.StartsWith(_value + "-")) return true;
                        return false;
                    };
                    break;
                case "^=":
                    // for matching starts-with
                    complies = (v) =>
                    {
                        if (v == null) return false;
                        if (String.IsNullOrEmpty(_value)) return false;
                        return (v.StartsWith(_value));
                    };
                    break;
                case "$=":
                    // for matching ends-with
                    complies = (v) =>
                    {
                        if (v == null) return false;
                        if (String.IsNullOrEmpty(_value)) return false;
                        return (v.EndsWith(_value));
                    };
                    break;
                case "*=":
                    // for matching contains
                    complies = (v) =>
                    {
                        if (v == null) return false;
                        if (String.IsNullOrEmpty(_value)) return false;
                        return (v.Contains(_value));
                    };
                    break;
                default:
                    throw new ArgumentException("Not a valid operator:" + _op);
            }
			context.ResultSetInternal = context.ResultSetInternal
				.Where(x => 
                    {
                        string valToCompare = x.GetAttribute(_name);
                        return complies(valToCompare);
                    });
		}

        internal static readonly Regex RxSelector = new Regex(@"^\[(?<name>[\w]+)\s*((?<operator>[~|\^$*!]?=)\s*(?<value>[^\]]*))?\]");
	}

	public class AttributeSelectorCreator : XQuerySelectorCreator
	{
		public override Regex MatchNext { get { return AttributeSelector.RxSelector; } }

		public override IXQuerySelector Create(XQueryParserContext context, Match match)
		{
            if (match.Groups["operator"] == null)
            {
                return new AttributeSelector(match.Groups["name"].Value, null, null);
            }
            return new AttributeSelector(match.Groups["name"].Value, match.Groups["operator"].Value, match.Groups["value"].Value);
        }
	}
}
