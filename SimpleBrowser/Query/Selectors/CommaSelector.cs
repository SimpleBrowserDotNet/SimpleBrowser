using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace SimpleBrowser.Query.Selectors
{
	public class CommaSelector : IXQuerySelector
	{
		public void Execute(XQueryResultsContext context)
		{
			context.NewResultSet();
		}

		public bool IsTransposeSelector { get { return true; } }

		internal static readonly Regex RxSelector = new Regex(@"^\s*,\s*");
	}

	public class CommaSelectorCreator : XQuerySelectorCreator
	{
		public override Regex MatchNext { get { return CommaSelector.RxSelector; } }

		public override IXQuerySelector Create(XQueryParserContext context, Match match)
		{
			return new CommaSelector();
		}
	}
}
