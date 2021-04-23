using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transcom.Web.Data.Forms;

namespace Transcom.Web.Services
{
	public class FormService<TForm> where TForm : FormBase
	{
		private readonly IMongoCollection<TForm> signups;
		private readonly DiscordClient discordClient;
		private readonly IConfiguration config;

		public delegate Task FormSubmittedEventHandler(object sender, FormSubmittedEventArgs args);
		public event FormSubmittedEventHandler FormSubmitted;

		public FormService(IMongoClient mongoClient, IConfiguration config, DiscordClient discordClient)
		{
			this.config = config;
			this.discordClient = discordClient;

			IMongoDatabase db = mongoClient.GetDatabase(config["MongoDb:Databases:Web"]);
			signups = db.GetCollection<TForm>(typeof(TForm).Name);
		}

		public IQueryable<TForm> ListForms() => signups.AsQueryable();

		public async Task<TForm> GetFormAsync(ObjectId id) => (await signups.FindAsync(f => f.Id == id)).FirstOrDefault();
		public async Task<IEnumerable<TForm>> GetUserFormsAsync(ulong snowflake) => (await signups.FindAsync(f => f.UserSnowflake == snowflake)).ToEnumerable();

		public async Task SubmitFormAsync(TForm form)
		{
			await signups.InsertOneAsync(form with
			{
				SubmittedAt = DateTime.UtcNow
			});

			FormSubmitted?.Invoke(this, new() { Form = form });
		}


		public async Task<bool> UserIsOnServerAsync(ulong snowflake)
		{
			DiscordGuild guild = await discordClient.GetGuildAsync(config.GetValue<ulong>("DiscordIntegration:Server:Id"));
			return guild.Members.ContainsKey(snowflake);
		}
	}

	public class FormSubmittedEventArgs : EventArgs
	{
		public FormBase Form { get; init; }
	}
}
