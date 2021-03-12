using Microsoft.AspNetCore.Components;
using System.IO;
using System.Threading.Tasks;

namespace Transcom.Web.Services
{
	public class TeamPageLoaderService
	{
		public static FileInfo HtmlContentFile => new(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "team", "content.html"));

		public static MarkupString HtmlContent => new(File.ReadAllText(HtmlContentFile.FullName));
	}
}
