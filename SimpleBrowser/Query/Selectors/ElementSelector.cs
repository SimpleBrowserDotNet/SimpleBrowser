using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Linq;
using System.Xml.Linq;

namespace SimpleBrowser.Query.Selectors
{
	public class ElementSelector : IXQuerySelector
	{
		private readonly string _name;

		public ElementSelector(string name)
		{
			_name = name.ToLower();
		}

		public bool IsTransposeSelector { get { return false; } }

		public void Execute(XQueryResultsContext context)
		{
			var set = context.ResultSetInternal;
			Debug.WriteLine("selecting <" + _name + "> from " + set.Count() + " nodes");
			context.ResultSetInternal = set
				.Where(x => string.Compare(x.Name.LocalName, _name, true) == 0);
		}

		internal static readonly Regex RxSelector = new Regex(@"^[A-Za-z][A-Za-z0-9_\-]*");
	}

	public class ElementSelectorCreator : XQuerySelectorCreator
	{
		public override Regex MatchNext { get { return ElementSelector.RxSelector; } }

		public override IXQuerySelector Create(XQueryParserContext context, Match match)
		{
			return new ElementSelector(match.Value);
		}
	}
}
