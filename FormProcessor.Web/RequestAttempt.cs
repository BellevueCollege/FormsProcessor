using System;
using System.Collections.Generic;
using System.Linq;

namespace FormProcessor
{
	/// <summary>
	/// 
	/// </summary>
	public class RequestAttempt
	{
		public const int MaxAllowed = 5;
		public const int SecondInterval = 60;

		public DateTime Timestamp{get;set;}
		public string ClientIP{get;set;}

		public RequestAttempt(string clientIP, DateTime timestamp)
		{
			ClientIP = clientIP;
			Timestamp = timestamp;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public static class RequestAttemptExtensions
	{
		public static void Record(this IList<RequestAttempt> attempts, string clientIP, DateTime timestamp)
		{
			attempts.Add(new RequestAttempt(clientIP, timestamp));
		}

		public static void Record(this IList<RequestAttempt> attempts, string clientIP)
		{
			attempts.Add(new RequestAttempt(clientIP, DateTime.Now));
		}

		public static int Count(this IList<RequestAttempt> attempts, string ip)
		{
			return attempts.Where(i => i.ClientIP == ip).Count();
		}

		public static void Expire(this IList<RequestAttempt> attempts, DateTime timestamp)
		{
			// create a new collection, because we're going to modify the original
			IList<RequestAttempt> attemptsCopy = attempts.Where(a => a.Timestamp <= timestamp).ToList();

			foreach (RequestAttempt attempt in attemptsCopy)
			{
				attempts.Remove(attempt);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="attempts"></param>
		/// <remarks>
		/// Default timespan is 30 minutes
		/// </remarks>
		public static void Expire(this IList<RequestAttempt> attempts)
		{
			Expire(attempts, DateTime.Now.Subtract(new TimeSpan(0, 0, RequestAttempt.SecondInterval)));
		}
	}

}