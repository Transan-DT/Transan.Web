using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transcom.Web.Data;

namespace Transcom.Web.Services
{
	public class GlossaryService
	{
		private readonly IMongoCollection<GlossaryEntry> glossaryEntries;


		public GlossaryService(IConfiguration configuration)
		{
			IConfigurationSection mongoConfig = configuration.GetSection("MongoDatabase");

			MongoClient client = new(mongoConfig["ConnectionString"]);
			IMongoDatabase db = client.GetDatabase(mongoConfig["DatabaseName"]);

			glossaryEntries = db.GetCollection<GlossaryEntry>("GlossaryEntries");

			if (glossaryEntries.EstimatedDocumentCount() is 0)
			{
				glossaryEntries.InsertOne(new());
			}
		}


		public IQueryable<GlossaryEntry> GetAllEntries() => glossaryEntries.AsQueryable();

		public async Task<IEnumerable<GlossaryEntry>> SearchEntriesAsync(string search) => await (await glossaryEntries.FindAsync(GetSearchFilter(search))).ToListAsync();


		private static FilterDefinition<GlossaryEntry> GetSearchFilter(string search) => Builders<GlossaryEntry>.Filter.Regex(e => e.DisplayTitle, new(search, "gi"));
	}
}
