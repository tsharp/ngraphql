﻿using NGraphQL.Model;
using NGraphQL.Server.Execution;
using NGraphQL.Model.Request;

namespace NGraphQL.Core {

  public class SkipDirectiveHandler: IDirectiveHandler, ISkipDirectiveAction {
    
    public bool ShouldSkip(RequestContext context, MappedSelectionItem item, object[] argValues) {
      var boolArg = (bool) argValues[0];
      return boolArg;
    } 
  
  }

}