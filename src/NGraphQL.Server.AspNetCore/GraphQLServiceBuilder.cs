using Microsoft.Extensions.DependencyInjection;

namespace NGraphQL.Server.AspNetCore {
  public class GraphQLServiceBuilder : IGraphQLServiceBuilder {
    private IServiceCollection services { get; }
    private GraphQLServiceOptions options = new GraphQLServiceOptions();

    public GraphQLServiceBuilder(IServiceCollection services) {
      this.services = services;
      this.RegisterServices();
    }

    public static IGraphQLServiceBuilder Create(IServiceCollection services) {
      return new GraphQLServiceBuilder(services);
    }

    private void RegisterServices() {
      services.AddSingleton(options);
    }

    public IGraphQLServiceBuilder AllowGetRequests(bool allowGetRequests = true) {
      options.AllowGetRequests = allowGetRequests;
      return this;
    }

    public IGraphQLServiceBuilder AllowIntrospection(bool allowIntrospection = true) {
      options.AllowIntrospection = allowIntrospection;
      return this;
    }
  }
}
