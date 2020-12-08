﻿using System; 
using NGraphQL.Introspection;
using NGraphQL.Runtime;

namespace NGraphQL.Model {

  public class DirectiveContext {
    public DirectiveDef Def;
    public DirectiveLocation Location;
    public object Owner;
    public Location SourceLocation;
  }
}
