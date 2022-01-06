using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NGraphQL.Data;

namespace NGraphQL.Server.AspNetCore {
  public class GraphQLMiddleware : MiddlewareBase {
    // FIXME: This should probably be a singleton value?
    private GraphQLServer server = new GraphQLServer();
    private JsonVariablesDeserializer variablesDeserializer = new JsonVariablesDeserializer();

    public GraphQLMiddleware(RequestDelegate next) : base(next) {
      server.Events.RequestPrepared += OnRequestPrepared;
    }

    private void OnRequestPrepared(object sender, GraphQLServerEventArgs e) {
      if (e.RequestContext.Operation.Variables.Count == 0) {
        return;
      }

      variablesDeserializer.PrepareRequestVariables(e.RequestContext);
    }

    protected override Task HandleRequest(HttpContext context) {

      var options = context.RequestServices.GetRequiredService<GraphQLServiceOptions>();

      // Handle Post
      if (HttpMethods.IsPost(context.Request.Method)) {
        return HandlePost(context);
      }

      var isGet = HttpMethods.IsGet(context.Request.Method);

      if (isGet && options.AllowGetRequests) {
        return HandleGet(context);
      }

      // The default is method not supported
      context.Response.StatusCode = 405;
      return Task.CompletedTask;
    }

    private async Task HandlePost(HttpContext context) {
      //StreamReader reader = new StreamReader(context.Request.Body);
      //string text = await reader.ReadToEndAsync();

      var request = await context.Request.Body.DeserializeFromJsonStreamAsync<GraphQLRequest>(context.RequestAborted);
      var requestContext = server.CreateRequestContext(request, context.RequestAborted, context.User, null, context);
      await server.ExecuteRequestAsync(requestContext);

      if (requestContext.Response.Errors != null && requestContext.Response.Errors.Any()) {
        context.Response.StatusCode = 400;
      }

      var raw = JsonSerializer.Serialize(requestContext.Response);

      await context.Response.Body.SerializeToJsonStreamAsync(requestContext.Response, context.RequestAborted);
    }

    private async Task HandleGet(HttpContext context) {
      context.Response.StatusCode = 501;
    }
  }
}
