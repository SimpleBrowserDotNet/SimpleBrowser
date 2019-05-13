// -----------------------------------------------------------------------
// <copyright file="CommaSelector.cs" company="SimpleBrowser">
// Copyright © 2010 - 2019, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Query.Selectors
{
    using System.Text.RegularExpressions;

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