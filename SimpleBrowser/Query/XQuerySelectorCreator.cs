using System;
using System.Text.RegularExpressions;

namespace SimpleBrowser.Query
{
	public abstract class XQuerySelectorCreator
	{
		public abstract Regex MatchNext { get; }
		public abstract IXQuerySelector Create(XQueryParserContext context, Match match);
		public virtual int Priority { get { return 0; } }
	}
}