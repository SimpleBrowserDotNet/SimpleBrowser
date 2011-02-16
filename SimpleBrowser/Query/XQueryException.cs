using System;

namespace SimpleBrowser.Query
{
	public class XQueryException : Exception
	{
		public string Query { get; set; }
		public int Index { get; set; }
		public int Length { get; set; }

		public XQueryException(string message, string query, int index, int length)
			: base(message)
		{
			Query = query;
			if(index >= query.Length)
				index = query.Length - 1;
			Index = index;
			if(index + length >= query.Length)
				length = query.Length - index;
			Length = length;
		}
	}
}