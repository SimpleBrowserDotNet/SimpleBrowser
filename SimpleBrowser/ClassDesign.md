Class Design
============

The classes inside SimpleBrowser have a fairly simple design, This page tries to describe the responsibilities of the classes and how the (should) interact. 

Browser
-------
This is the central class that the clients interact with. It is responsible for navigation and parsing the responses. 

HtmlResult
----------
This is the class that is passed out to client code to interact with elements in a page. The object can represent one or more HtmlElement instances. HtmlResult is the result of a query on the Browser object (unsing any of the FindXxx methods or Select(). HtmlResult is also the strtingpoint for new queries using the Select or Refine methods).

HtmlElement
-----------
This is a family of classes that wrap the XElement objects that form the inner storage of the page. The interaction with the underlying XML is primarily done through these classes. To get the correct kind of instance wrapping a certain XElement, you can call HtmlElement.CreateFor(element). These classes contain the implementation of the actions that should folow after clicking an anchor, checkbox, etc. They also implement the logic around being selected/unselected for options and form elements.