using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Xml.Linq;

namespace SimpleBrowser.Query.Selectors
{
	public class AllSelector : IXQuerySelector
	{
		public AllSelector()
		{
		}

		public bool IsTransposeSelector { get { return false; } }

		public void Execute(XQueryResultsContext context)
		{
			context.ResultSetInternal = context.ResultSetInternal;
		}

		internal static readonly Regex RxSelector = new Regex(@"^\*");
	}

	public class AllSelectorCreator : XQuerySelectorCreator
	{
		public override Regex MatchNext { get { return AllSelector.RxSelector; } }

		public override IXQuerySelector Create(XQueryParserContext context, Match match)
		{
			return new AllSelector();
		}
	}
}
