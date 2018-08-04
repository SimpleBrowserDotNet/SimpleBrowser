// -----------------------------------------------------------------------
// <copyright file="Selectors.cs" company="SimpleBrowser">
// Copyright © 2010 - 2018, Nathan Ridley and the SimpleBrowser contributors.
// See https://github.com/SimpleBrowserDotNet/SimpleBrowser/blob/master/readme.md
// </copyright>
// -----------------------------------------------------------------------

namespace SimpleBrowser.UnitTests.OfflineTests
{
    using System.Linq;
    using NUnit.Framework;

    [TestFixture]
    public class Selectors
    {
        [Test]
        public void SearchingAnInputElementBySeveralSelectingMethods()
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.SimpleForm.htm"));

            HtmlResult colorBox = b.Find("colorBox"); // find by id
            Assert.That(colorBox.Count() == 1, "There should be exactly 1 element with ID colorBox");

            colorBox = b.Find("input", new { name = "colorBox", type = "color" }); // find by attributes
            Assert.That(colorBox.Count() == 1, "There should be exactly 1 element with name colorBox and type color");

            colorBox = b.Find("input", new { name = "colorBox", type = "Color" }); // find by attributes
            Assert.That(colorBox.Count() == 1, "There should be exactly 1 element with name colorBox and type color");

            colorBox = b.Find("input", new { name = "colorBox", type = "Colors" }); // find by attributes
            Assert.That(colorBox.Exists == false, "There should be no element with name colorBox and type Colors");

            colorBox = b.Find(ElementType.Checkbox, new { name = "colorBox", type = "Color" }); // find by attributes
            Assert.That(colorBox.Count() == 0, "Input elements with types other than the specified type should not be found");

            colorBox = b.Find("input", FindBy.Name, "colorBox"); // find by FindBy
            Assert.That(colorBox.Count() == 1, "There should be exactly 1 element with name colorBox");

            colorBox = b.Select("input[name=colorBox]"); // find by Css selector
            Assert.That(colorBox.Count() == 1, "There should be exactly 1 element with name colorBox");

            colorBox = b.Select("input[type=color]"); // find by Css selector
            Assert.That(colorBox.Count() == 1, "There should be exactly 1 element with type color");

            colorBox = b.Select("input[type=Color]"); // find by Css selector
            Assert.That(colorBox.Count() == 0, "There should be no element for the expression input[type=Color] (CSS is case sensitive)");

            HtmlResult clickLink = b.Select(".clickLink"); // find by Css selector
            Assert.That(clickLink.Count() == 1, "There should be one element for the expression .clickLink");
        }

        [Test]
        public void SearchingAnElementBySeveralSelectingMethods()
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.SimpleForm.htm"));

            HtmlResult colorBox = b.Find("first-checkbox"); // find by id
            Assert.That(colorBox.Count() == 1, "There should be exactly 1 element with ID first-checkbox");

            colorBox = b.Select("*[type=checkbox][checked]");
            Assert.That(colorBox.Count() == 1, "There should be exactly 1 element with type checkbox and checked");
        }

        [Test]
        public void UsePlusSelector()
        {
            Browser b = new Browser();
            b.SetContent(Helper.GetFromResources("SimpleBrowser.UnitTests.SampleDocs.SimpleForm.htm"));
            HtmlResult inputDirectlyUnderForm = b.Select("div + input");
            Assert.That(inputDirectlyUnderForm.Count() == 1); // only one <input> comes directly after a div
        }
    }
}