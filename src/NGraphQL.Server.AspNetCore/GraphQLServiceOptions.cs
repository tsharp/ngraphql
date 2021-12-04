namespace NGraphQL.Server.AspNetCore {
  internal class GraphQLServiceOptions {
    /// <summary>
    /// Enables or disables introspection on the server. Disabled by default.
    /// </summary>
    public bool AllowIntrospection { get; internal set; } = false;
    public bool AllowGetRequests { get; internal set; } = false;
  }
}
