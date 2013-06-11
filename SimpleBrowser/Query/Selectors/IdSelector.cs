using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Xml.Linq;
using SimpleBrowser;

namespace SimpleBrowser.Query.Selectors
{
	public class IdSelector : IXQuerySelector
	{
		private readonly string _id;

		public IdSelector(string id)
		{
			_id = id.ToLower();
		}

		public bool IsTransposeSelector { get { return false; } }

		public void Execute(XQueryResultsContext context)
		{
			// var ids = context.ResultSetInternal.Where(x => x.HasAttributeCI("id")).Select(x => x.GetAttributeCI("id")).ToArray();
			var results = context.ResultSetInternal.Where(x => string.Compare(x.GetAttributeCI("id"), _id, true) == 0);
			context.ResultSetInternal = results;
		}

		internal static readonly Regex RxSelector = new Regex(@"^\#(?<id>[A-Za-z_][A-Za-z0-9_\-:\.]+)");
	}

	public class IdSelectorCreator : XQuerySelectorCreator
	{
		public override Regex MatchNext { get { return IdSelector.RxSelector; } }

		public override IXQuerySelector Create(XQueryParserContext context, Match match)
		{
			return new IdSelector(match.Groups["id"].Value);
		}
	}
}
