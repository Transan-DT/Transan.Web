using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Transan.Web.Pages.Auth
{
	public class LogoutModel : PageModel
	{
		public async Task<IActionResult> OnGet()
		{
			await HttpContext.SignOutAsync();
			return Redirect("/");
		}
	}
}
