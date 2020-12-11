﻿using NGraphQL.Introspection;
using NGraphQL.Model;
using NGraphQL.Server.Execution;
using NGraphQL.Server.RequestModel;

namespace NGraphQL.Core {

  [HandlesDirective("@include")]
  public class IncludeDirectiveHandler: DirectiveHandler, ISkipDirectiveAction {
    bool _if; 

    public IncludeDirectiveHandler(DirectiveContext context, object[] args) 
      : base(context, args, typeof(ISkipDirectiveAction)) {
      _if = (bool)args[0];
    }

    public bool ShouldSkip(RequestContext context, MappedField field) => !_if; 
  }

}
