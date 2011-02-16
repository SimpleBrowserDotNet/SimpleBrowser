using System.Text.RegularExpressions;

namespace SimpleBrowser.Query
{
	public interface IXQuerySelector
	{
		void Execute(XQueryResultsContext context);
		bool IsTransposeSelector { get; }
	}
}