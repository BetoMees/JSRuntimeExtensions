using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop.Infrastructure;

namespace Microsoft.JSInterop
{
	public static class JSExtensions
	{
		public static async ValueTask InvokeVoidAsync(this IJSRuntime jsRuntime, Type component, string identifier, params object?[]? args)
		{
			ArgumentNullException.ThrowIfNull(jsRuntime);

			IJSObjectReference _jsRuntime = await jsRuntime.GetJsReference(component);
			await _jsRuntime.InvokeAsync<IJSVoidResult>(identifier, args);
			await _jsRuntime.DisposeAsync();
		}
		public static async ValueTask InvokeVoidAsync(this IJSRuntime jsRuntime, ComponentBase component, string identifier, params object?[]? args)
		{
			ArgumentNullException.ThrowIfNull(jsRuntime);

			IJSObjectReference _jsRuntime = await jsRuntime.GetJsReference(component.GetType());
			await _jsRuntime.InvokeAsync<IJSVoidResult>(identifier, args);
			await _jsRuntime.DisposeAsync();
		}
		public static async ValueTask<TValue> InvokeAsync<TValue>(this IJSRuntime jsRuntime, Type component, string identifier, params object?[]? args)
		{
			ArgumentNullException.ThrowIfNull(jsRuntime);

			IJSObjectReference _jsRuntime = await jsRuntime.GetJsReference(component);
			TValue result = await _jsRuntime.InvokeAsync<TValue>(identifier, args);
			await _jsRuntime.DisposeAsync();

			return result;
		}
		public static async ValueTask<TValue> InvokeAsync<TValue>(this IJSRuntime jsRuntime, ComponentBase component, string identifier, params object?[]? args)
		{
			ArgumentNullException.ThrowIfNull(jsRuntime);

			IJSObjectReference _jsRuntime = await jsRuntime.GetJsReference(component.GetType());
			TValue result = await _jsRuntime.InvokeAsync<TValue>(identifier, args);
			await _jsRuntime.DisposeAsync();

			return result;
		}
		private async static Task<IJSObjectReference> GetJsReference(this IJSRuntime jsRuntime, Type compType)
		{
			ArgumentNullException.ThrowIfNull(compType);

			string fullName = compType.FullName ?? "";
			var projectName = compType.Assembly.GetName().Name + ".";

			var fileName = fullName.TrimStart(projectName.ToCharArray());
			string jsFileName = $"./{fileName.Replace('.', '/')}.razor.js";

			if (!File.Exists(jsFileName))
				throw new Exception($"File {jsFileName} do not exists");

			return await jsRuntime.InvokeAsync<IJSObjectReference>("import", jsFileName);
		}
	}
}
