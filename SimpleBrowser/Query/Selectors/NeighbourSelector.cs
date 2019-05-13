// -----------------------------------------------------------------------
// <copyright file="NeighbourSelector.cs" company="SimpleBrowser">
// Copyright © 2010 - 2019, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Query.Selectors
{
    using System.Linq;
    using System.Text.RegularExpressions;

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