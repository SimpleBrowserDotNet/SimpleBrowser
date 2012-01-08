using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SimpleBrowser.Query
{
	public class XQueryResultsContext
	{
		public XQueryResultsContext(XDocument doc)
		{
			Document = doc;
		}

		private List<IEnumerable<XElement>> _resultSets = new List<IEnumerable<XElement>>();
		private IEnumerable<XElement> _currentResultSet;

		internal Func<IEnumerable<XElement>, IEnumerable<XElement>> PreTranslateResultSet { get; set; }
		internal XDocument Document { get; private set; }

		internal XElement[] ResultSet
		{
			get
			{
				return _resultSets
					.SelectMany(x => x)
					.Distinct() // new XElementEqualityComparer()
					.ToArray();
			}
		}

		internal IEnumerable<XElement> ResultSetInternal
		{
			get
			{
				if(_currentResultSet != null)
				{
					if(PreTranslateResultSet != null)
					{
						var results = PreTranslateResultSet(_currentResultSet);
						PreTranslateResultSet = null;
						_currentResultSet = results;
					}
					return _currentResultSet;
				}
				return Document.Descendants();
			}

			set
			{
				if(_currentResultSet == null)
					_resultSets.Add(value);
				else
					_resultSets[_resultSets.Count - 1] = value;
				_currentResultSet = value;
			}
		}

		internal void NewResultSet()
		{
			_currentResultSet = null;
		}

		class XElementEqualityComparer : IEqualityComparer<XElement>
		{
			private XNodeEqualityComparer _comparer = new XNodeEqualityComparer();
			public bool Equals(XElement x, XElement y)
			{
				return ReferenceEquals(x, y);
			}

			public int GetHashCode(XElement obj)
			{
				return _comparer.GetHashCode(obj);
			}
		}
	}
}