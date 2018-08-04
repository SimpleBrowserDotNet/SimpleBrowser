// -----------------------------------------------------------------------
// <copyright file="ElementSelector.cs" company="SimpleBrowser">
// Copyright © 2010 - 2018, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Query.Selectors
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;

    public class ElementSelector : IXQuerySelector
    {
        private readonly string _name;

        public ElementSelector(string name)
        {
            this._name = name.ToLower();
        }

        public bool IsTransposeSelector { get { return false; } }

        public void Execute(XQueryResultsContext context)
        {
            IEnumerable<XElement> set = context.ResultSetInternal;
            Debug.WriteLine("selecting <" + this._name + "> from " + set.Count() + " nodes");
            context.ResultSetInternal = set
                .Where(x => string.Compare(x.Name.LocalName, this._name, true) == 0);
        }

        internal static readonly Regex RxSelector = new Regex(@"^[A-Za-z][A-Za-z0-9_\-]*");
    }

    public class ElementSelectorCreator : XQuerySelectorCreator
    {
        public override Regex MatchNext { get { return ElementSelector.RxSelector; } }

        public override IXQuerySelector Create(XQueryParserContext context, Match match)
        {
            return new ElementSelector(match.Value);
        }
    }
}