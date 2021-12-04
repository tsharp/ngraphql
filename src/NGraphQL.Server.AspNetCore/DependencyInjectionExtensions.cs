namespace Microsoft.AspNetCore.Builder;

using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using NGraphQL.Server.AspNetCore;
using static Microsoft.AspNetCore.Routing.Patterns.RoutePatternFactory;

public static class DependencyInjectionExtensions {
  internal static bool IsAdded<TService, TImplementation>(this IServiceCollection services) {
    return services
        .Where(s => s.ServiceType == typeof(TService) && s.ImplementationType == typeof(TImplementation))
        .Any();
  }

  internal static bool IsAdded<TService>(this IServiceCollection services) =>
    services.IsAdded<TService, TService>();

  public static IGraphQLServiceBuilder AddGraphQL(this IServiceCollection services) {
    return GraphQLServiceBuilder.Create(services);
  }

  public static GraphEndpointConventionBuilder MapGraphQL(
      this IEndpointRouteBuilder endpointRouteBuilder,
      string endpoint = "/graphql",
      string schemaName = default)
      => MapGraphQL(endpointRouteBuilder, new PathString(endpoint), schemaName);

  public static GraphEndpointConventionBuilder MapGraphQL(
      this IEndpointRouteBuilder endpointRouteBuilder,
      PathString path,
      string schemaName = default) {
    if (endpointRouteBuilder is null) {
      throw new ArgumentNullException(nameof(endpointRouteBuilder));
    }

    var pattern = Parse(path.ToString().TrimEnd('/') + "/{**slug}");
    IApplicationBuilder requestPipeline = endpointRouteBuilder.CreateApplicationBuilder();

    var options = requestPipeline
      .ApplicationServices
      .GetRequiredService<GraphQLServiceOptions>();

    requestPipeline
      .UseMiddleware<GraphQLMiddleware>()
      .UseMiddleware<ServiceUnavailableMiddleware>();

    return new GraphEndpointConventionBuilder(
        endpointRouteBuilder
            .Map(pattern, requestPipeline.Build())
            .WithDisplayName("GraphQL Server Endpoint"));
  }
}