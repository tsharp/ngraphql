﻿using System;
using System.Collections.Generic;
using System.Text;
using Irony.Parsing;
using NGraphQL.Model;
using NGraphQL.Server;
using System.Linq;
using NGraphQL.Server.Parsing;
using NGraphQL.Model.Request;

namespace NGraphQL.Server.Parsing {
  using Node = Irony.Parsing.ParseTreeNode;

  public partial class RequestBuilder {

    private List<RequestDirective> BuildDirectives(Node dirListNode, DirectiveLocation atLocation, RequestObjectBase parent) {
      var dirList = new List<RequestDirective>();
      if(dirListNode == null)
        return dirList;
      foreach(var dirNode in dirListNode.ChildNodes) {
        var dir = BuildDirective(dirNode, atLocation, parent);
        if(dir == null)
          continue;
        dirList.Add(dir);
      }
      return dirList;
    }

    private RequestDirective BuildDirective(Node dirNode, DirectiveLocation atLocation, RequestObjectBase parent) {
      var dirName = dirNode.ChildNodes[0].GetText();
      _path.Push(dirName);
      try {
        var dirDef = LookupDirective(dirNode);
        if(dirDef == null) 
          return null; // error is already logged
        if(!dirDef.Locations.IsSet(atLocation)) {
          AddError($"Directive {dirName} may not be placed at this location ({atLocation}). Valid locations: [{dirDef.Locations}].", dirNode);
          return null;
        }
        var dir = new RequestDirective() { Def = dirDef, Name = dirName, Location = dirNode.GetLocation(), Parent = parent };
        var argListNode = dirNode.FindChild(TermNames.ArgListOpt);
        dir.Args = BuildArguments(argListNode.ChildNodes, dir);
        return dir;
      } finally {
        _path.Pop();
      }
    }

  }
}