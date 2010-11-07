using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;

namespace SimpleBrowser.Parser
{
	public class DocumentBuilder
	{
		static readonly string[] SelfClosing = new[] { "area", "base", "basefont", "br", "hr", "iframe", "input", "img", "link", "meta", "param" };

		private readonly List<HtmlParserToken> _tokens;
		private XDocument _doc;
		private DocumentBuilder(List<HtmlParserToken> tokens)
		{
			_tokens = tokens;
			_doc = XDocument.Parse("<?xml version=\"1.0\"?><html />");
		}

		public static XDocument Parse(List<HtmlParserToken> tokens)
		{
			var hdb = new DocumentBuilder(tokens);
			hdb.Assemble();
			return hdb._doc;
		}

		int _index;
		private void Assemble()
		{
			var stack = new Stack<XElement>();
			Func<XElement> topOrRoot = () => stack.Count == 0 ? _doc.Root : stack.Peek();
			while(_index < _tokens.Count)
			{
				var token = _tokens[_index++];
				switch(token.Type)
				{
					case TokenType.Element:
					{
						var name = token.A.ToLowerInvariant();
						if(name == "html") break;
						var current = new XElement(name);
						topOrRoot().Add(current);
						ReadAttributes(current);
						if(!SelfClosing.Contains(name))
							stack.Push(current);
						break;
					}

					case TokenType.CloseElement:
					{
						var name = token.A.ToLowerInvariant();
						if(name == "html") break;
						if(stack.Any(x => x.Name == name))
							do
							{
								var x = stack.Pop();
								if(x.Name == name)
									break;
							} while(stack.Count > 0);
						break;
					}

					case TokenType.Comment:
						topOrRoot().Add(new XComment(token.A));
						break;

					case TokenType.Text:
						topOrRoot().Add(new XText(token.A));
						break;
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
				if(name != "xmlns" && RxValidAttrName.IsMatch(name))
					current.SetAttributeValue(name, HttpUtility.HtmlDecode(token.B ?? token.A ?? ""));
			}
		}
	}
}