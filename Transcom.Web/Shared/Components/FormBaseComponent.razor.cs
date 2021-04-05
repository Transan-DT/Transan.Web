using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Transcom.Web.Data.Forms;
using Transcom.Web.Services;

namespace Transcom.Web.Shared.Components
{
	public abstract class FormBaseComponent<TForm> : ComponentBase where TForm : FormBase, new()
	{
		[Parameter] public TForm Form { get; set; }
		[Parameter] public EventCallback<TForm> OnValidSubmit { get; set; }

		[Inject] protected FormService<TForm> FormService { get; set; }
		protected EditContext FormEditContext { get; set; }

		private ClaimsPrincipal user;
		private ulong userSnowflake;


		[Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; init; }
		[Inject] private IHttpContextAccessor HttpContextAccessor { get; init; }
		


		protected override async Task OnParametersSetAsync()
		{
			user = (await AuthenticationStateProvider.GetAuthenticationStateAsync()).User;
			userSnowflake = Convert.ToUInt64(user.FindFirstValue(ClaimTypes.NameIdentifier));

			Form = new()
			{
				Id = ObjectId.GenerateNewId(),
				UserSnowflake = userSnowflake,
				IpAddress = HttpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString()
			};

			FormEditContext = new(Form);

			await base.OnParametersSetAsync();
		}

		protected async Task OnValidFormAsync()
		{
			await FormService.SubmitFormAsync(Form);
			await OnValidSubmit.InvokeAsync(Form);
		}
	}
}