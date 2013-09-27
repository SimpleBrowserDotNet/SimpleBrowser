using System;
using System.Linq;
using System.Xml.Linq;
using System.Text.RegularExpressions;

namespace SimpleBrowser.Query.Selectors
{
	public class DescendentSelector : IXQuerySelector
	{
		public void Execute(XQueryResultsContext context)
		{
			context.PreTranslateResultSet = x => x.Descendants();
		}

		public bool IsTransposeSelector { get { return true; } }

		internal static readonly Regex RxSelector = new Regex(@"^\s+");
	}

	public class DescendentSelectorCreator : XQuerySelectorCreator
	{
		public override Regex MatchNext { get { return DescendentSelector.RxSelector; } }

		public override IXQuerySelector Create(XQueryParserContext context, Match match)
		{
			return new DescendentSelector();
		}
		public override int Priority{get{return -1000;}}
	}
}
