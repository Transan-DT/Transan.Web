﻿using Microsoft.Extensions.Configuration;
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

		public IOrderedEnumerable<IGrouping<char, GlossaryEntry>> GetEntriesByStartLetter() =>
			from entry in GetAllEntries().AsEnumerable()
			group entry by entry.DisplayTitle[0] into letterGroup
			orderby letterGroup.Key
			select letterGroup;

		public async Task<IEnumerable<GlossaryEntry>> SearchEntriesAsync(string search) => await (await glossaryEntries.FindAsync(GetSearchFilter(search))).ToListAsync();

		public async Task<GlossaryEntry> FetchEntryAsync(string urlId) => await (await glossaryEntries.FindAsync(Builders<GlossaryEntry>.Filter.Eq(e => e.UrlTitle, urlId))).FirstOrDefaultAsync();


		private static FilterDefinition<GlossaryEntry> GetSearchFilter(string search) => Builders<GlossaryEntry>.Filter.Regex(e => e.DisplayTitle, new(search, "gi"));
	}
}
