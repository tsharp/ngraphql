namespace NGraphQL.Server.AspNetCore {
  public interface IGraphQLServiceBuilder {
    public IGraphQLServiceBuilder AllowIntrospection(bool allowIntrospection);
    public IGraphQLServiceBuilder AllowGetRequests(bool allowGetRequests);
  }
}
