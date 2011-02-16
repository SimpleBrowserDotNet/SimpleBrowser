using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace SimpleBrowser.Query
{
	/* Some simple rules about jQuery:
	 * 
	 * parsing operates in a context which is a set of included elements
	 * no elements are selected in a blank query- a selector is required in order to obtain a subset of elements from the context
	 * a new (raw) context draws from any available node in the tree, unlike an empty context which is the result of selections with no matches
	 * a filtered context only operates on nodes that are selected within the context (which could be none if selectors have filtered out all nodes)
	 * a query should always start and end with a selector- a query starting or ending with a shift operator is invalid
	 * context is cloned for subqueries, as the context may alter during a subquery but need to be reset upon resolution of the subquery e.g. :not(p > b)
	
	 * there are five types of selectors:
		 * #id - select an element by its 'id' attribute - starts with a hash
		 * .class - select an element by its 'class' attribute - starts with a period
		 * tag - selects an element according to its tag name - starts with a letter
		 * [attribute filter] - selects an element by the existence of, or value of, an attribute with a given name - starts with a square bracket
		 * :named filter - selects elements that match a defined rule (note that this type of selector can select and include elements outside of the current selection) - starts with a colon
	
	 * shift operators traverse the context - they are:
		* > filter context to children of existing selection
		* [space] filter context to descendents of existing selection
		* + filter context to next adjacent siblings of existing selection
	 
	 * usage examples: http://ejohn.org/files/selectors.html
	 */

	public static class XQuery
	{
		internal static IXQuerySelector[] Parse(XQueryParserContext context)
		{
			var list = new List<IXQuerySelector>();
			while(!context.EndOfQuery)
			{
				var selector = context.MatchNextSelector();
				if(selector == null)
					throw new XQueryException("Unexpected character at position " + context.Index + " in query: " + context.Query.Substring(context.Index), context.Query, context.Index, 1);
				list.Add(selector);
			}
			return list.ToArray();
		}

		public static XElement[] Execute(string query, XDocument doc, params XElement[] baseElements)
		{
			var parserContext = new XQueryParserContext(new SelectorParserCatalog(), query);
			var selectors = Parse(parserContext);
			var resultsContext = new XQueryResultsContext(doc);
			if(baseElements.Length > 0)
				resultsContext.ResultSetInternal = baseElements;
			if(selectors.Length > 0)
				if(selectors.Last().IsTransposeSelector || selectors.First().IsTransposeSelector)
					throw new XQueryException("A query may not start or end with a transposal selector (e.g. >)", query, 0, query.Length);
			foreach(var selector in selectors)
				selector.Execute(resultsContext);
			return resultsContext.ResultSet;
		}
	}
}