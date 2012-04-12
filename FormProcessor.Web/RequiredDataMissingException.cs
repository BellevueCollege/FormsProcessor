using System;
using System.Collections.Generic;

namespace FormProcessor
{
	public class RequiredDataMissingException : Exception
	{
		public IList<string> MissingFields{get;private set;}

		public RequiredDataMissingException(IList<string> missingFields)
		{
			MissingFields = missingFields;
		}
	}
}