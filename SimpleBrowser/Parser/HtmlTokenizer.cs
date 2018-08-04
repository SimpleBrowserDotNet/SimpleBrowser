// -----------------------------------------------------------------------
// <copyright file="HtmlTokenizer.cs" company="SimpleBrowser">
// Copyright © 2010 - 2018, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class HtmlTokenizer
    {
        private class ParserContext
        {
            public ParserContext(string html)
            {
                this.Html = html;
                this.Tokens = new List<HtmlParserToken>();
            }

            public string Html { get; private set; }
            public int Index { get; set; }
            public List<HtmlParserToken> Tokens { get; set; }
            public bool RemoveExtraWhiteSpace { get; set; }

            public bool EndOfString { get { return this.Index >= this.Html.Length; } }
            public bool AtLastIndex { get { return this.Index == this.Html.Length - 1; } }
            public char CharAtIndex { get { return this.Html[this.Index]; } }

            public bool InScriptTag { get; set; }

            public string AdjustForWhitespace(string str)
            {
                if (this.RemoveExtraWhiteSpace)
                {
                    return Regex.Replace(str, @"\s+", " ");
                }

                return str;
            }
        }

        public static List<HtmlParserToken> Parse(string html, bool removeExtraWhiteSpace = false)
        {
            ParserContext context = new ParserContext(html) { RemoveExtraWhiteSpace = removeExtraWhiteSpace };
            ReadNext(context);
            return context.Tokens;
        }

        private static Regex RxNextToken = new Regex(@"\<((\!((?<doctype>DOCTYPE)|(?<cdata>\[CDATA\[)|(?<comment>(\s)?--)|(?<conditional>\[)))|(?<xmldecl>\?\s?xml)|(?<element>[a-z])|(?<close>/[a-z])|())", RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static Regex RxNextScriptToken = new Regex(@"\<((?<comment>!(\s)?--)|(?<close>/script(\s)?\>))", RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private static void ReadNext(ParserContext context)
        {
            while (!context.EndOfString)
            {
                Match match;
                if (context.InScriptTag)
                {
                    match = RxNextScriptToken.Match(context.Html, context.Index);

                    if (match.Groups["close"].Success)
                    {
                        context.InScriptTag = false;
                    }
                }
                else
                {
                    match = RxNextToken.Match(context.Html, context.Index);
                }

                if (match.Success)
                {
                    int len = match.Index - context.Index;
                    if (len > 0)
                    {
                        string str = context.Html.Substring(context.Index, len);
                        context.Tokens.Add(new HtmlParserToken { Type = TokenType.Text, A = context.AdjustForWhitespace(str), Raw = str });
                        context.Index += len;
                    }
                    if (match.Groups["xmldecl"].Success)
                    {
                        ReadXmlDeclaration(context);
                    }
                    else if (match.Groups["doctype"].Success)
                    {
                        ReadDocTypeDeclaration(context);
                    }
                    else if (match.Groups["comment"].Success)
                    {
                        ReadComment(context);
                    }
                    else if (match.Groups["close"].Success)
                    {
                        ReadCloseElement(context);
                    }
                    else if (match.Groups["cdata"].Success)
                    {
                        ReadCdata(context);
                    }
                    else if (match.Groups["conditional"].Success)
                    {
                        ReadConditional(context);
                    }
                    else
                    {
                        ReadElement(context);
                    }
                }
                else
                {
                    string str = context.Html.Substring(context.Index);
                    context.Tokens.Add(new HtmlParserToken { Type = TokenType.Text, A = context.AdjustForWhitespace(str), Raw = str });
                    return;
                }
            }
        }

        private static void ReadXmlDeclaration(ParserContext context)
        {
            // always closes at the next '>' character
            int start = context.Index;
            int n = context.Html.IndexOf('>', context.Index);
            context.Index = n == -1 ? context.Html.Length : n + 1;
            context.Tokens.Add(new HtmlParserToken { Type = TokenType.XmlDeclaration, Raw = context.Html.Substring(start, context.Index - start) });
        }

        private static void ReadDocTypeDeclaration(ParserContext context)
        {
            // always closes at the next '>'
            int start = context.Index;
            int n = context.Html.IndexOf('>', context.Index);
            context.Index = n == -1 ? context.Html.Length : n + 1;
            context.Tokens.Add(new HtmlParserToken { Type = TokenType.DocTypeDeclaration, Raw = context.Html.Substring(start, context.Index - start) });
        }

        private static Regex RxStartComment = new Regex(@"\<\!(\s)?--", RegexOptions.ExplicitCapture);

        private static void ReadComment(ParserContext context)
        {
            int len;
            int n = context.Html.IndexOf("-->", context.Index);
            Match match = RxStartComment.Match(context.Html, context.Index);
            int start = match.Index;
            int innerStart = match.Index + match.Length;
            if (n > -1)
            {
                len = Math.Max(0, n - innerStart);
                context.Index = n + match.Length - 1;
            }
            else
            {
                n = context.Html.IndexOf('>', context.Index);
                if (n == -1)
                {
                    len = Math.Max(0, context.Html.Length - innerStart);
                    context.Index = context.Html.Length;
                }
                else
                {
                    len = Math.Max(0, n - innerStart);
                    context.Index = n + 1;
                }
            }
            context.Tokens.Add(new HtmlParserToken { Type = TokenType.Comment, A = len == 0 ? string.Empty : context.Html.Substring(innerStart, len), Raw = context.Html.Substring(start, context.Index - start) });
        }

        private static Regex RxStartCdata = new Regex(@"\<\!\[CDATA\[", RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private static void ReadCdata(ParserContext context)
        {
            int len;
            int n = context.Html.IndexOf("]]>", context.Index);
            Match match = RxStartCdata.Match(context.Html, context.Index);
            int start = match.Index;
            int innerStart = match.Index + match.Length;
            if (n > -1)
            {
                len = Math.Max(0, n - innerStart);
                context.Index = n + 3;
            }
            else
            {
                n = context.Html.IndexOf('>', context.Index);
                if (n == -1)
                {
                    len = Math.Max(0, context.Html.Length - innerStart);
                    context.Index = context.Html.Length;
                }
                else
                {
                    len = Math.Max(0, n - innerStart);
                    context.Index = n + 1;
                }
            }
            context.Tokens.Add(new HtmlParserToken { Type = TokenType.Cdata, A = len == 0 ? null : context.Html.Substring(innerStart, len), Raw = context.Html.Substring(start, context.Index - start) });
        }

        private static Regex RxStartConditional = new Regex(@"\<\!\[", RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private static void ReadConditional(ParserContext context)
        {
            int len;
            int n = context.Html.IndexOf("]>", context.Index);
            Match match = RxStartConditional.Match(context.Html, context.Index);
            int start = match.Index;
            int innerStart = match.Index + match.Length - 1;
            if (n > -1)
            {
                len = Math.Max(0, n - innerStart) + 1;
                context.Index = n + 2;
            }
            else
            {
                n = context.Html.IndexOf('>', context.Index);
                if (n == -1)
                {
                    len = Math.Max(0, context.Html.Length - innerStart) + 1;
                    context.Index = context.Html.Length;
                }
                else
                {
                    len = Math.Max(0, n - innerStart) + 1;
                    context.Index = n + 1;
                }
            }
            context.Tokens.Add(new HtmlParserToken { Type = TokenType.Comment, A = len == 0 ? null : context.Html.Substring(innerStart, len), Raw = context.Html.Substring(start, context.Index - start) });
        }

        private static void SkipWhiteSpace(ParserContext context)
        {
            while (!context.EndOfString && char.IsWhiteSpace(context.CharAtIndex))
            {
                context.Index++;
            }
        }

        private static void SkipToNext(ParserContext context, params char[] chars)
        {
            while (!context.EndOfString && !chars.Contains(context.CharAtIndex))
            {
                context.Index++;
            }
        }

        private static void SkipToNextOrEndOfWhiteSpace(ParserContext context, params char[] chars)
        {
            while (!context.EndOfString && !chars.Contains(context.CharAtIndex) && !char.IsWhiteSpace(context.CharAtIndex))
            {
                context.Index++;
            }

            SkipWhiteSpace(context);
        }

        private static bool ReadChar(ParserContext context, char ch)
        {
            if (context.CharAtIndex == '>')
            {
                context.Index++;
                return true;
            }
            return false;
        }

        private static string ReadStringUntil(ParserContext context, params char[] chars)
        {
            int start = context.Index;
            while (!context.EndOfString && !chars.Contains(context.CharAtIndex))
            {
                context.Index++;
            }

            return context.Html.Substring(start, context.Index - start);
        }

        private static string ReadStringUntilWhiteSpaceOr(ParserContext context, params char[] chars)
        {
            int start = context.Index;
            while (!context.EndOfString && !chars.Contains(context.CharAtIndex) && !char.IsWhiteSpace(context.CharAtIndex))
            {
                context.Index++;
            }

            return context.Html.Substring(start, context.Index - start);
        }

        /* READING ATTRIBUTE VALUES
		 * name - any string of non-whitespace characters
		 * can't start with ="'<>
		 * can't contain whitespace or any of <>=
		 * following whitespace is ignored
		 * if an equal symbol is found, follow up with attribute value
		 * if value started with a quote, only that same quote terminates the value
		 * if value did NOT start with a quote, the value terminates with whitespace or any of <>
		 */

        private static Regex RxReadTagName = new Regex(@"[A-Za-z][A-Za-z0-9]*");
        private static Regex RxReadAttribute = new Regex(@"(?<name>([^\='""\<\>\s/]|/(?=\>))[^\=\<\>\s]*)(?<eq>\s*=\s*(?<quote>'|"")?(?(quote)(?<value>(?(\k<quote>)|.)*)|(?<value>([^\s\<\>]|/(?=\>))*))\k<quote>?)?", RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private static void ReadElement(ParserContext context)
        {
            int start = context.Index;
            context.Index++;
            Match match = RxReadTagName.Match(context.Html, context.Index);
            context.Index += match.Length;
            HtmlParserToken elementToken = new HtmlParserToken { Type = TokenType.Element, A = match.Value };
            if (match.Value.ToLowerInvariant() == "script")
            {
                context.InScriptTag = true;
            }

            context.Tokens.Add(elementToken);

            while (!context.EndOfString)
            {
                // read whitespace before an attribute name
                SkipWhiteSpace(context);

                if (!context.EndOfString && RxReadAttribute.IsMatch(context.CharAtIndex.ToString()))
                {
                    Match attrMatch = RxReadAttribute.Match(context.Html, context.Index);
                    HtmlParserToken token = new HtmlParserToken { Type = TokenType.Attribute, A = attrMatch.Groups["name"].Value, Raw = attrMatch.Value };
                    Group valgrp = attrMatch.Groups["value"];
                    if (valgrp.Success)
                    {
                        token.B = valgrp.Value;
                    }

                    context.Tokens.Add(token);
                    context.Index += attrMatch.Length;
                }
                else
                {
                    if (context.Index < context.Html.Length - 1 && context.Html.Substring(context.Index, 2) == "/>")
                    {
                        context.Index += 2;
                        break;
                    }
                    char ch = context.CharAtIndex;
                    if (ch != '<')
                    {
                        context.Index++;
                    }

                    if (ch == '>' || ch == '<')
                    {
                        break;
                    }
                }
            }

            elementToken.Raw = context.Html.Substring(start, context.Index - start);
        }

        private static Regex RxReadCloseAttribute = new Regex(@"\</(?<name>[^\s=\>/]+)[^\>]*\>");

        private static void ReadCloseElement(ParserContext context)
        {
            Match match = RxReadCloseAttribute.Match(context.Html, context.Index);
            if (!match.Success)
            {
                string str = context.Html.Substring(context.Index);
                context.Tokens.Add(new HtmlParserToken { Type = TokenType.Text, A = context.AdjustForWhitespace(str), Raw = str });
                context.Index = context.Html.Length;
            }
            else
            {
                HtmlParserToken newToken = new HtmlParserToken { Type = TokenType.CloseElement, Raw = match.Value, A = match.Groups["name"].Value };
                context.Tokens.Add(newToken);
                //HACK there might be a tag inside of a tag (Incorrectly closed tag) like </strong</td>
                //If we find this, we are going to adjust
                if (newToken.A.IndexOf("<") > -1)
                {
                    int index = match.Value.Substring(2).IndexOf("<");
                    newToken.A = newToken.A.Substring(0, index);
                    context.Index += index + 2;
                }
                else
                {
                    context.Index += match.Length;
                }
            }
        }
    }

    public enum TokenType
    {
        Comment,
        Text,
        XmlDeclaration,
        DocTypeDeclaration,
        Element,
        Attribute,
        CloseElement,
        Cdata
    }

    public class HtmlParserToken
    {
        public TokenType Type { get; set; }
        public string A { get; set; }
        public string B { get; set; }
        public string Raw { get; set; }

        public override string ToString()
        {
            if (this.Raw == null)
            {
                return base.ToString();
            }

            if (this.Raw.Length < 50)
            {
                return this.Raw;
            }

            return string.Concat(this.Raw.Substring(0, 47), "...");
        }
    }
}