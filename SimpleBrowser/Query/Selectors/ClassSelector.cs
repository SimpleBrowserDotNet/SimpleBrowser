// -----------------------------------------------------------------------
// <copyright file="ClassSelector.cs" company="SimpleBrowser">
// Copyright © 2010 - 2018, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Query.Selectors
{
    using System.Linq;
    using System.Text.RegularExpressions;
    using SimpleBrowser;

    public class ClassSelector : IXQuerySelector
    {
        private readonly string _class;

        public ClassSelector(string @class)
        {
            this._class = @class;
        }

        public bool IsTransposeSelector { get { return false; } }

        public void Execute(XQueryResultsContext context)
        {
            context.ResultSetInternal = context.ResultSetInternal
                .Where(x =>
                    {
                        string c = x.GetAttributeCI("class");
                        if (c == null)
                        {
                            return false;
                        }

                        return c.Split(' ').Contains(this._class);
                    });
        }

        internal static readonly Regex RxSelector = new Regex(@"^\.(?<class>[A-Za-z0-9_\-]+)");
    }

    public class ClassSelectorCreator : XQuerySelectorCreator
    {
        public override Regex MatchNext { get { return ClassSelector.RxSelector; } }

        public override IXQuerySelector Create(XQueryParserContext context, Match match)
        {
            return new ClassSelector(match.Groups["class"].Value);
        }
    }
}