// -----------------------------------------------------------------------
// <copyright file="Namespace.cs" company="SimpleBrowser">
// See https://github.com/axefrog/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.UnitTests.OfflineTests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using NUnit.Framework;

    /// <summary>
    /// A unit test for the html tags with attributes, and specifically namespace attributes.
    /// </summary>
    [TestFixture]
    public class Namespace
    {
        /// <summary>
        /// The number of valid (not ignored/dropped) namespaces in the html tag of $CommentElements.htm$.
        /// </summary>
        private static int namespaceCount = 9;

        /// <summary>
        /// Tests that the html element may have attributes and that namespace attributes are parsed correctly.
        /// </summary>
        [Test]
        public void HtmlElement_Attributes()
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.CommentElements.htm"));
            HtmlResult result = b.Find("html1");

            Assert.IsTrue(result.Exists);
            Assert.AreEqual(namespaceCount, result.XElement.Attributes().Count());

            List<XAttribute> attributes = new List<XAttribute>();
            attributes.AddRange(result.XElement.Attributes());

            for (int index = 0; index < namespaceCount; index++)
            {
                XAttribute attribute = attributes[index];
                switch (index)
                {
                    case 0:
                        {
                            Assert.AreEqual(attribute.Name.LocalName, "xmlns_");
                            Assert.AreEqual(attribute.Value, "http://www.w3.org/1999/xhtml");
                            break;
                        }

                    case 1:
                        {
                            Assert.IsFalse(attribute.IsNamespaceDeclaration);
                            Assert.AreEqual(attribute.Name.LocalName, "lang");
                            Assert.AreEqual(attribute.Value, "en");
                            break;
                        }

                    case 2:
                        {
                            Assert.IsFalse(attribute.IsNamespaceDeclaration);
                            Assert.AreEqual(attribute.Name.LocalName, "xml_lang");
                            Assert.AreEqual(attribute.Value, "en");
                            break;
                        }

                    case 3:
                        {
                            Assert.IsFalse(attribute.IsNamespaceDeclaration);
                            Assert.AreEqual(attribute.Name.LocalName, "id");
                            Assert.AreEqual(attribute.Value, "html1");
                            break;
                        }

                    case 4:
                        {
                            Assert.IsFalse(attribute.IsNamespaceDeclaration);
                            Assert.AreEqual(attribute.Name.LocalName, "class");
                            Assert.AreEqual(attribute.Value, "cookieBar");
                            break;
                        }

                    case 5:
                        {
                            Assert.AreEqual(attribute.Name.LocalName, "xmlns_fb");
                            Assert.AreEqual(attribute.Value, "http://www.facebook.com/2008/fbml");
                            break;
                        }

                    case 6:
                        {
                            Assert.AreEqual(attribute.Name.LocalName, "xmlns_xsi");
                            Assert.AreEqual(attribute.Value, "http://www.w3.org/2001/XMLSchema-instance");
                            break;
                        }

                    case 7:
                        {
                            Assert.AreEqual(attribute.Name.LocalName, "xsi_schemalocation");
                            Assert.AreEqual(attribute.Value, "http://namespaces.ordnancesurvey.co.uk/cmd/local/v1.1 http://www.ordnancesurvey.co.uk/oswebsite/xml/cmdschema/local/V1.1/CMDFeatures.xsd");

                            XAttribute parent_attribute = result.XElement.Attributes().FirstOrDefault(element => element.Name == "xmlns_xsi");
                            Assert.IsNotNull(parent_attribute);
                            Assert.AreEqual(parent_attribute.Value, "http://www.w3.org/2001/XMLSchema-instance");

                            break;
                        }
                    case 8:
                        {
                          Assert.IsFalse(attribute.IsNamespaceDeclaration);
                          Assert.AreEqual(attribute.Name.LocalName, "xsl_badattribute");
                          Assert.AreEqual(attribute.Value, "http://www.w3.org");
                          Assert.That(attribute.Name.Namespace == "");
                          break;
                        }
                }
            }
        }
    }
}
