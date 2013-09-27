using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SimpleBrowser.Query.Selectors
{
	public class NeighbourSelector : IXQuerySelector
	{
		public void Execute(XQueryResultsContext context)
		{
			context.PreTranslateResultSet = x => { return x.Select(e => e.ElementsAfterSelf().FirstOrDefault()); };
		}

		public bool IsTransposeSelector { get { return true; } }

		internal static readonly Regex RxSelector = new Regex(@"^\s*\+\s*");
	}

	public class NeighbourSelectorCreator : XQuerySelectorCreator
	{
		public override Regex MatchNext { get { return NeighbourSelector.RxSelector; } }

		public override IXQuerySelector Create(XQueryParserContext context, Match match)
		{
			return new NeighbourSelector();
		}
	}

}
