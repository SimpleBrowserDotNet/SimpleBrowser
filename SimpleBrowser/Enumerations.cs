// -----------------------------------------------------------------------
// <copyright file="Enumerations.cs" company="SimpleBrowser">
// Copyright © 2010 - 2019, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser
{
    public enum FindBy
    {
        Name,
        Id,
        Class,
        Value,
        Text,
        PartialText,
        PartialName,
        PartialClass,
        PartialValue,
        PartialId
    }

    public enum ElementType
    {
        Anchor,
        TextField,
        Button,
        RadioButton,
        Checkbox,
        SelectBox,
        Script
    }

    public enum ClickResult
    {
        Failed,
        SucceededNoOp,
        SucceededNoNavigation,
        SucceededNavigationComplete,
        SucceededNavigationError
    }
}