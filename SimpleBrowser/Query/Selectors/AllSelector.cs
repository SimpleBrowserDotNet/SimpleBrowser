// -----------------------------------------------------------------------
// <copyright file="AllSelector.cs" company="SimpleBrowser">
// Copyright © 2010 - 2019, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Query.Selectors
{
    using System.Text.RegularExpressions;

    public class AllSelector : IXQuerySelector
    {
        public AllSelector()
        {
        }

        public bool IsTransposeSelector { get { return false; } }

        public void Execute(XQueryResultsContext context)
        {
            context.ResultSetInternal = context.ResultSetInternal;
        }

        internal static readonly Regex RxSelector = new Regex(@"^\*");
    }

    public class AllSelectorCreator : XQuerySelectorCreator
    {
        public override Regex MatchNext { get { return AllSelector.RxSelector; } }

        public override IXQuerySelector Create(XQueryParserContext context, Match match)
        {
            return new AllSelector();
        }
    }
}