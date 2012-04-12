using System.Data.Entity;
using Common.Logging;

namespace FormProcessor
{
	public class FormProcessorDbContext : DbContext
	{
		readonly private static ILog _log = LogManager.GetCurrentClassLogger();

		public DbSet<FormSubmissionEntity> Forms{get;set;}
	}
}