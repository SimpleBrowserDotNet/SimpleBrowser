// -----------------------------------------------------------------------
// <copyright file="DescendentSelector.cs" company="SimpleBrowser">
// Copyright © 2010 - 2019, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Query.Selectors
{
    using System.Text.RegularExpressions;
    using System.Xml.Linq;

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

        public override int Priority { get { return -1000; } }
    }
}