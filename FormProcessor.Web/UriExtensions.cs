using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ctc.Ods.Extensions
{
	public static class Uri
	{
		/// <summary>
		/// Returns the <see cref="System.Uri.AbsoluteUri"/> without the <see cref="System.Uri.Query"/> segment
		/// </summary>
		/// <param name="uri"></param>
		/// <returns></returns>
		public static string GetAbsoluteUriWithoutQuery(this System.Uri uri)
		{
			string absoluteUri = uri.AbsoluteUri;
			int queryPosition = absoluteUri.IndexOf('?');
			
			if (queryPosition > -1)
			{
				return absoluteUri.Substring(0, queryPosition);
			}
			return absoluteUri;
		}
	}
}