using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop.Infrastructure;
using System.Collections.Concurrent;

namespace Microsoft.JSInterop
{
	public static class JSExtensions
	{
		private static readonly ConcurrentDictionary<string, IJSObjectReference> _references = new();
		private static async Task removeReference(KeyValuePair<string, IJSObjectReference> reference)
		{
			await reference.Value.DisposeAsync();
			_references.TryRemove(reference.Key, out _);
		}

		public static async Task DisposeAsync(this IJSRuntime jsRuntime)
		{
			await Task.WhenAll(_references.Select(removeReference));
		}
		public static async ValueTask InvokeVoidAsync(this IJSRuntime jsRuntime, Type component, string identifier, params object?[]? args)
		{
			ArgumentNullException.ThrowIfNull(jsRuntime);

			IJSObjectReference _jsRuntime = await jsRuntime.GetJsReference(component);
			await _jsRuntime.InvokeAsync<IJSVoidResult>(identifier, args);
		}
		public static async ValueTask InvokeVoidAsync(this IJSRuntime jsRuntime, ComponentBase component, string identifier, params object?[]? args)
		{
			ArgumentNullException.ThrowIfNull(jsRuntime);

			IJSObjectReference _jsRuntime = await jsRuntime.GetJsReference(component.GetType());
			await _jsRuntime.InvokeAsync<IJSVoidResult>(identifier, args);
		}
		public static async ValueTask<TValue> InvokeAsync<TValue>(this IJSRuntime jsRuntime, Type component, string identifier, params object?[]? args)
		{
			ArgumentNullException.ThrowIfNull(jsRuntime);

			IJSObjectReference _jsRuntime = await jsRuntime.GetJsReference(component);
			TValue result = await _jsRuntime.InvokeAsync<TValue>(identifier, args);
			return result;
		}
		public static async ValueTask<TValue> InvokeAsync<TValue>(this IJSRuntime jsRuntime, ComponentBase component, string identifier, params object?[]? args)
		{
			ArgumentNullException.ThrowIfNull(jsRuntime);

			IJSObjectReference _jsRuntime = await jsRuntime.GetJsReference(component.GetType());
			TValue result = await _jsRuntime.InvokeAsync<TValue>(identifier, args);
			return result;
		}
		private async static Task<IJSObjectReference> GetJsReference(this IJSRuntime jsRuntime, Type compType)
		{
			ArgumentNullException.ThrowIfNull(compType);
			IJSObjectReference result;

			string fullName = compType.FullName ?? "";

			if (_references.TryGetValue(fullName, out result))
				return result;

			var projectName = compType.Assembly.GetName().Name + ".";
			var fileName = fullName.TrimStart(projectName.ToCharArray());
			string jsFileName = $"./{fileName.Replace('.', '/')}.razor.js";

			if (!File.Exists(jsFileName))
				throw new Exception($"File {jsFileName} do not exists");

			result = await jsRuntime.InvokeAsync<IJSObjectReference>("import", jsFileName);

			_references.TryAdd(fullName, result);

			return result;
		}
	}
}
