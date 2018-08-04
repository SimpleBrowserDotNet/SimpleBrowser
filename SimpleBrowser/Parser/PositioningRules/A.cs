// -----------------------------------------------------------------------
// <copyright file="A.cs" company="SimpleBrowser">
// Copyright © 2010 - 2018, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Parser.PositioningRules
{
    internal class A : BodyElementPositioningRule
    {
        // static readonly string[] _allowedParentTags = new [] { "" };
        public override string TagName { get { return "a"; } }
    }
}