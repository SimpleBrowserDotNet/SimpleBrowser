using System;
using NUnit.Framework;
using SimpleBrowser.Internal;

namespace SimpleBrowser.UnitTests.InternalTests
{
    [TestFixture]
    public class SetCookieHeaderParserTests
    {
        [Test]
        public void KeyValueCookie_ParsesNameValueCorrectly()
        {
            var actual = SetCookieHeaderParser.GetAllCookiesFromHeader("test.com", "theme=light");
            Assert.That(actual.Count, Is.EqualTo(1));
            Assert.That(actual[0].Name, Is.EqualTo("theme"));
            Assert.That(actual[0].Value, Is.EqualTo("light"));
        }

        [Test]
        public void KeyValueCookie_StripsCarriageReturns()
        {
            var actual = SetCookieHeaderParser.GetAllCookiesFromHeader("test.com", "theme=light\r\n");
            Assert.That(actual.Count, Is.EqualTo(1));
            Assert.That(actual[0].Name, Is.EqualTo("theme"));
            Assert.That(actual[0].Value, Is.EqualTo("light"));
        }

        [Test]
        public void KeyOnlyCookie_ParsesNameValueCorrectly()
        {
            var actual = SetCookieHeaderParser.GetAllCookiesFromHeader("test.com", "theme");
            Assert.That(actual.Count, Is.EqualTo(1));
            Assert.That(actual[0].Name, Is.EqualTo("theme"));
            Assert.That(actual[0].Value, Is.EqualTo(""));
        }

        [Test]
        public void CookieWithNoDomain_UsesDefaultDomain()
        {
            const string testDomain = "test.com";
            const string cookieHeader = "theme";
            var actual = SetCookieHeaderParser.GetAllCookiesFromHeader(testDomain, cookieHeader);
            Assert.That(actual[0].Domain, Is.EqualTo(testDomain));
        }

        [Test]
        public void CookieWithDomain_UsesAssignedDomain()
        {
            const string testDomain = "test.com";
            const string cookieHeader = "sessionToken=abc123; Domain=.cookie-specified.com";
            var actual = SetCookieHeaderParser.GetAllCookiesFromHeader(testDomain, cookieHeader);
            Assert.That(actual[0].Domain, Is.EqualTo(".cookie-specified.com"));
        }

        [Test]
        public void CookieWithNoPath_PathIsForwardSlash()
        {
            const string testDomain = "test.com";
            const string cookieHeader = "sessionToken=abc123";
            var actual = SetCookieHeaderParser.GetAllCookiesFromHeader(testDomain, cookieHeader);
            Assert.That(actual[0].Path, Is.EqualTo("/"));
        }

        [Test]
        public void CookieWithPath_UsesAssignedPath()
        {
            const string testDomain = "test.com";
            const string cookieHeader = "sessionToken=abc123; Path=/test/path";
            var actual = SetCookieHeaderParser.GetAllCookiesFromHeader(testDomain, cookieHeader);
            Assert.That(actual[0].Path, Is.EqualTo("/test/path"));
        }

        [Test]
        public void KeyValueCookieWithExpiresAttribute_DisregardsExpiresValue()
        {
            const string cookieHeader = "sessionToken=abc123; Expires=Wed, 09 Jun 2021 10:18:14 GMT";
            var actual = SetCookieHeaderParser.GetAllCookiesFromHeader("test.com", cookieHeader);
            Assert.That(actual.Count, Is.EqualTo(1));
            Assert.That(actual[0].Name, Is.EqualTo("sessionToken"));
            Assert.That(actual[0].Value, Is.EqualTo("abc123"));
            Assert.That(actual[0].Expires, Is.EqualTo(DateTime.MinValue));
        }

        [Test]
        public void KeyOnlyCookieWithExpiresAttribute_DisregardsExpiresValue()
        {
            const string cookieHeader = "sessionToken; Expires=Wed, 09 Jun 2021 10:18:14 GMT";
            var actual = SetCookieHeaderParser.GetAllCookiesFromHeader("test.com", cookieHeader);
            Assert.That(actual.Count, Is.EqualTo(1));
            Assert.That(actual[0].Name, Is.EqualTo("sessionToken"));
            Assert.That(actual[0].Value, Is.EqualTo(""));
            Assert.That(actual[0].Expires, Is.EqualTo(DateTime.MinValue));
        }

        [Test]
        public void CommaSeparatedCookies_AreReadAsMultipleCookies()
        {
            const string cookieHeader = "key1=value1; Expires=Test, TestDate, key2=value2";
            var actual = SetCookieHeaderParser.GetAllCookiesFromHeader("test.com", cookieHeader);
            Assert.That(actual.Count, Is.EqualTo(2));
            Assert.That(actual[0].Name, Is.EqualTo("key1"));
            Assert.That(actual[0].Value, Is.EqualTo("value1"));
            Assert.That(actual[1].Name, Is.EqualTo("key2"));
            Assert.That(actual[1].Value, Is.EqualTo("value2"));
        }

        [Test]
        public void CookieContainingCommaValue_IsReadAsASingleCookie()
        {
            const string cookieHeader = "key=\"value1, value2\"";
            var actual = SetCookieHeaderParser.GetAllCookiesFromHeader("test.com", cookieHeader);
            Assert.That(actual.Count, Is.EqualTo(1));
            Assert.That(actual[0].Name, Is.EqualTo("key"));
            Assert.That(actual[0].Value, Is.EqualTo("value1, value2"));
        }

        [Test]
        public void ComplexFacebookCookie_DoesNotThrowException()
        {
            const string cookieHeader =
                "datr=80gTVQMaYfPPoOakoxDXhrmQ; expires=Fri, 24-Mar-2017 23:46:59 GMT; Max-Age=63072000; path=/; domain=.facebook.com; httponly,reg_ext_ref=deleted; expires=Thu, 01-Jan-1970 00:00:01 GMT; Max-Age=0; path=/; domain=.facebook.com,reg_fb_ref=https%3A%2F%2Fwww.facebook.com%2F; path=/; domain=.facebook.com; httponly,reg_fb_gate=https%3A%2F%2Fwww.facebook.com%2F; path=/; domain=.facebook.com; httponly";
            var actual = SetCookieHeaderParser.GetAllCookiesFromHeader("test.com", cookieHeader);
        }
    }
}
