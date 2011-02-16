using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace SimpleBrowser.Query.Selectors
{
	public class ChildSelector : IXQuerySelector
	{
		public void Execute(XQueryResultsContext context)
		{
			context.PreTranslateResultSet = x => { return x.Elements(); };
		}

		public bool IsTransposeSelector { get { return true; } }

		internal static readonly Regex RxSelector = new Regex(@"^\s*\>\s*");
	}

	public class ChildSelectorCreator : XQuerySelectorCreator
	{
		public override Regex MatchNext { get { return ChildSelector.RxSelector; } }

		public override IXQuerySelector Create(XQueryParserContext context, Match match)
		{
			return new ChildSelector();
		}
	}
}
