// -----------------------------------------------------------------------
// <copyright file="ChildSelector.cs" company="SimpleBrowser">
// Copyright © 2010 - 2019, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Query.Selectors
{
    using System.Text.RegularExpressions;
    using System.Xml.Linq;

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