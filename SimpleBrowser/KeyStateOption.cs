// -----------------------------------------------------------------------
// <copyright file="KeyStateOption.cs" company="SimpleBrowser">
// Copyright © 2010 - 2018, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser
{
    using System;

    [Flags]
    public enum KeyStateOption
    {
        None = 0,
        Shift = 1,
        Ctrl = 2,
        Alt = 4
    }
}