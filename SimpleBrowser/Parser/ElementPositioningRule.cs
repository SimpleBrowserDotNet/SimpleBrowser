// -----------------------------------------------------------------------
// <copyright file="ElementPositioningRule.cs" company="SimpleBrowser">
// Copyright © 2010 - 2018, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;

    internal abstract class ElementPositioningRule
    {
        /// <summary>
        /// The tag name for the element
        /// </summary>
        public abstract string TagName { get; }

        /// <summary>
        /// If the element is present in the wrong area, it will be removed and appended to the correct area
        /// </summary>
        public abstract DocumentArea Area { get; }

        /// <summary>
        /// Check the element position in relation to its parent
        /// </summary>
        /// <param name="element">The XElement instance to validate and reposition as needed</param>
        public virtual void ValidateAndReposition(XElement element)
        {
        }

        /// <summary>
        /// If null, the tag can have both text and non-text children. If true, it can only have text children and if false, it cannot have text children.
        /// </summary>
        public virtual bool? TextChildren { get { return null; } }

        private static Dictionary<string, ElementPositioningRule> _rules;

        public static ElementPositioningRule Get(string tagName)
        {
            lock (typeof(ElementPositioningRule))
            {
                if (_rules == null)
                {
                    _rules = new Dictionary<string, ElementPositioningRule>();
                    foreach (Type type in typeof(ElementPositioningRule).Assembly.GetTypes().Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(ElementPositioningRule))))
                    {
                        ElementPositioningRule rule = (ElementPositioningRule)Activator.CreateInstance(type);
                        _rules.Add(rule.TagName, rule);
                    }
                }
                ElementPositioningRule r;
                return _rules.TryGetValue(tagName, out r) ? r : null;
            }
        }
    }

    internal abstract class BodyElementPositioningRule : ElementPositioningRule
    {
        /// <summary>
        /// If the element is present in the wrong area, it will be removed and appended to the correct area
        /// </summary>
        public override DocumentArea Area { get { return DocumentArea.Body; } }
    }

    internal abstract class HeadElementPositioningRule : ElementPositioningRule
    {
        /// <summary>
        /// If the element is present in the wrong area, it will be removed and appended to the correct area
        /// </summary>
        public override DocumentArea Area { get { return DocumentArea.Head; } }
    }

    internal enum DocumentArea
    {
        Body,
        Head,
        Any
    }
}