using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace SimpleBrowser.Parser
{
	public class DocumentBuilder
	{
		/// <summary>
		/// Defines all of the tags that are self-closing (i.e., "empty" in HTML 4 or "void" in HTML 5.)
		/// </summary>
		/// <remarks>
		/// All tags exist in both HTML 4 and 5, except the following:
		/// HTML 4 only: basefont, frame, isindex
		/// HTML 5 only: embed, keygen, source, track, wbr
		/// </remarks>
		static readonly string[] SelfClosing = new[] { "area", "base", "basefont", "br", "col", "command", "embed", "frame", "hr", "img", "input", "isindex", "keygen", "link", "meta", "param", "source", "track", "wbr" };

		private readonly List<HtmlParserToken> _tokens;
		private XDocument _doc;
		private DocumentBuilder(List<HtmlParserToken> tokens)
		{
			_tokens = tokens;
			string doctype = string.Empty;
			HtmlParserToken doctypeToken = tokens.Where(t => t.Type == TokenType.DocTypeDeclaration).FirstOrDefault();
			if (doctypeToken != null)
			{
				doctype = doctypeToken.Raw;
			}

			try
			{
				_doc = XDocument.Parse(string.Format("<?xml version=\"1.0\"?>{0}<html />", doctype));
			}
			catch (XmlException)
			{
				// System.Xml.Linq.XDocument throws an XmlException if it encounters a DOCTYPE it
				// can't parse. If this occurs, do not use the DOCTYPE from the page.
				_doc = XDocument.Parse("<?xml version=\"1.0\"?><html />");
			}
			if (_doc.DocumentType != null)
			{
#if !__MonoCS__
				_doc.DocumentType.InternalSubset = null;
#endif
			}
		}

		public static XDocument Parse(List<HtmlParserToken> tokens)
		{
			var hdb = new DocumentBuilder(tokens);
			hdb.Assemble();
			return hdb._doc;
		}

		private string SanitizeElementName(string name)
		{
			if(name.Contains(":"))
				name = name.Substring(name.LastIndexOf(":") + 1);
			return name.ToLowerInvariant();
		}

		int _index;
		private void Assemble()
		{
			var stack = new Stack<XElement>();
			Func<XElement> topOrRoot = () => stack.Count == 0 ? _doc.Root : stack.Peek();
			while (_index < _tokens.Count)
			{
				var token = _tokens[_index++];
				switch (token.Type)
				{
					case TokenType.Element:
						{
							var name = SanitizeElementName(token.A);
							XElement current = null;
							if (name == "html")
							{
								current = topOrRoot();
							}
							else
							{
								current = new XElement(name);
								topOrRoot().Add(current);
							}

							ReadAttributes(current);
							if (!SelfClosing.Contains(name))
							{
								stack.Push(current);
							}

							break;
						}

					case TokenType.CloseElement:
						{
							var name = SanitizeElementName(token.A);
							if (stack.Any(x => x.Name == name))
							{
								do
								{
									var x = stack.Pop();
									if (x.Name == name)
									{
										break;
									}
								} while (stack.Count > 0);
							}

							break;
						}

					case TokenType.Comment:
						{
							topOrRoot().Add(new XComment(token.A));
							break;
						}

					case TokenType.Cdata:
						{
							topOrRoot().Add(new XCData(token.A));
							break;
						}

					case TokenType.Text:
						{
							var parent = topOrRoot();
							if (parent.Name.LocalName.ToLower() == "textarea")
							{
								parent.Add(new XText(token.Raw));
							}
							else
							{
								parent.Add(new XText(token.A));
							}

							break;
						}
				}
			}
		}

		static readonly Regex RxValidAttrName = new Regex(@"^[A-Za-z_][A-Za-z0-9_\-\.]*$");
		private void ReadAttributes(XElement current)
		{
			while(_index < _tokens.Count && _tokens[_index].Type == TokenType.Attribute)
			{
				var token = _tokens[_index++];
				var name = token.A.ToLowerInvariant();
				name = name.Replace(':', '_');
				if (name == "xmlns")
				{
					name += "_";
				}

				if(RxValidAttrName.IsMatch(name))
				{
					current.SetAttributeValue(name, HttpUtility.HtmlDecode(token.B ?? token.A ?? string.Empty));
				}
			}
		}
	}
}