using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace NGraphQL.Server.AspNetCore {
  public class GraphQLMiddleware : MiddlewareBase {
    public GraphQLMiddleware(RequestDelegate next) : base(next) {
    }

    protected override Task HandleRequest(HttpContext context) {

      var options = context.RequestServices.GetRequiredService<GraphQLServiceOptions>();

      // Handle Post
      if (HttpMethods.IsPost(context.Request.Method)) {
        return HandlePost(context);
      }

      var isGet = HttpMethods.IsGet(context.Request.Method);

      if(isGet && options.AllowGetRequests) {
        return HandleGet(context);
      }

      // The default is method not supported
      context.Response.StatusCode = 405;
      return Task.CompletedTask;
    }

    private async Task HandlePost(HttpContext context) {

    }

    private async Task HandleGet(HttpContext context) {
      context.Response.StatusCode = 501;
    }
  }
}
