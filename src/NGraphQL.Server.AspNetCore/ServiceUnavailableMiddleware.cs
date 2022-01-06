using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace NGraphQL.Server.AspNetCore;
public class ServiceUnavailableMiddleware : MiddlewareBase {
  public ServiceUnavailableMiddleware(RequestDelegate next) : base(next) {

  }

  protected override async Task HandleRequest(HttpContext context) {
    await Task.CompletedTask;
    context.Response.StatusCode = 503;
  }
}
