using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace NGraphQL.Server.AspNetCore {
  public abstract class MiddlewareBase : IDisposable {

    private readonly RequestDelegate next;

    public MiddlewareBase(RequestDelegate next) {
      this.next = next;
    }

    protected virtual async Task HandleRequest(HttpContext context) => await Task.CompletedTask;

    protected virtual async Task Next(HttpContext context) => await next(context);

    public async Task InvokeAsync(HttpContext context) {
      await HandleRequest(context);
    }

    public virtual void Dispose() {
    }
  }
}
