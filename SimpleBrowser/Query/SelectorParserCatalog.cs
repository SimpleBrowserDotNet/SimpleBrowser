using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SimpleBrowser.Query
{
	public class SelectorParserCatalog
	{
		private static XQuerySelectorCreator[] _selectors;

		static SelectorParserCatalog()
		{
			_selectors = typeof(SelectorParserCatalog)
				.Assembly
				.GetTypes()
				.Where(t => t.IsSubclassOf(typeof(XQuerySelectorCreator)))
				.Select(xqstype => (XQuerySelectorCreator)Activator.CreateInstance(xqstype))
				.OrderBy(xqsc => xqsc.Priority) // we want high priority at the end of the line
				.ToArray();
		}

		internal IXQuerySelector GetNextSelector(XQueryParserContext context)
		{
			Match match = null;
			XQuerySelectorCreator xqsc = null;
			int matchLength = 0;
			var str = context.Query.Substring(context.Index);
			XQuerySelectorCreator[] selectors;
			lock(_selectors)
				selectors = _selectors.ToArray();
			foreach(var qs in selectors)
			{
				var m = qs.MatchNext.Match(str);
				if(m.Success && m.Length > matchLength)
				{
					matchLength = m.Length;
					xqsc = qs;
					match = m;
				}
			}
			if(xqsc == null)
				return null;
			var sel = xqsc.Create(context, match);
			context.Index += match.Length;
			return sel;
		}
	}
}