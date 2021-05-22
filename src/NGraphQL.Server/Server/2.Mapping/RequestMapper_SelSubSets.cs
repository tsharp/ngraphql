﻿using System.Collections.Generic;
using System.Linq;
using NGraphQL.CodeFirst;
using NGraphQL.Core;
using NGraphQL.Model;
using NGraphQL.Server.Execution;
using NGraphQL.Model.Request;
using NGraphQL.Utilities;

namespace NGraphQL.Server.Parsing {

  /// <summary>RequestMapper takes request tree and maps its objects to API model; for ex: selection field is mapped to field definition</summary>
  public partial class RequestMapper {

    private void MapOperation(GraphQLOperation op) {
      _currentOp = op; 
      MapSelectionSubSet(op.SelectionSubset, op.OperationTypeDef, op.Directives);
    }

    private void MapSelectionSubSet(SelectionSubset selSubset, TypeDefBase typeDef, IList<RequestDirective> directives) {
      switch(typeDef) {
        case ScalarTypeDef _:
        case EnumTypeDef _:
          // that should never happen
          AddError($"Scalar or Enum may not have a selection subset", selSubset);
          break;

        case ObjectTypeDef objTypeDef:
          MapObjectSelectionSubset(selSubset, objTypeDef, directives);
          break;

        case InterfaceTypeDef intTypeDef:
          foreach(var objType in intTypeDef.PossibleTypes)
            MapObjectSelectionSubset(selSubset, objType, directives);
          break;

        case UnionTypeDef unionTypeDef:
          foreach(var objType in unionTypeDef.PossibleTypes)
            MapObjectSelectionSubset(selSubset, objType, directives, isForUnion: true);
          break;
      }
    }
    
    // Might be called for ObjectType or Interface (for intf - just to check fields exist)
    private void MapObjectSelectionSubset(SelectionSubset selSubset, ObjectTypeDef objectTypeDef, IList<RequestDirective> directives, bool isForUnion = false) {
      // Map arguments on fields, add directives, map fragments 
      foreach (var item in selSubset.Items) {
        AddRuntimeRequestDirectives(item);
        switch (item) {
          case SelectionField selFld:
            var fldDef = objectTypeDef.Fields.FirstOrDefault(f => f.Name == selFld.Name);
            if (fldDef == null) {
              // if field not found, the behavior depends if it is a union; it is error for a union
              if (!isForUnion)
                AddError($"Field '{selFld.Name}' not found on type '{objectTypeDef.Name}'.", selFld);
              continue;
            }
            selFld.MappedArgs = MapArguments(selFld.Args, fldDef.Args, selFld);
            AddRuntimeModelDirectives(fldDef);
            break;

          case FragmentSpread fspread:
            if (fspread.Fragment == null) { //for named fragments
              fspread.Fragment = GetFragmentDef(fspread.Name);
              if (fspread.Fragment == null)
                AddError($"Fragment {fspread.Name} not defined.", fspread);
            }
            break; 
        }//switch
      } //foreach item

      if (_requestContext.Failed)
        return; 

      foreach (var typeMapping in objectTypeDef.Mappings) {
        var mappedItems = new List<MappedSelectionItem>();
        foreach (var item in selSubset.Items) {

          switch (item) {
            case SelectionField selFld:
              var fldResolver = typeMapping.FieldResolvers.FirstOrDefault(fr => fr.Field.Name == selFld.Name);
              if (fldResolver == null) 
                // it is not error, it should have been caught earlier; it is unmatch for union
                continue;
              var mappedFld = new MappedSelectionField(selFld, fldResolver, mappedItems.Count);
              mappedItems.Add(mappedFld);
              ValidateMappedFieldAndProcessSubset(mappedFld);
              break;

            case FragmentSpread fs:
              var onType = fs.Fragment.OnTypeRef?.TypeDef;
              var skip = onType != null && onType != objectTypeDef;
              if (skip)
                continue;
              MapObjectSelectionSubset(fs.Fragment.SelectionSubset, objectTypeDef, fs.Directives, isForUnion);
              var mappedSpread = new MappedFragmentSpread(fs);
              mappedItems.Add(mappedSpread);
              break;
          }//switch

        } //foreach item

        selSubset.MappedSubSets.Add(new MappedSelectionSubSet() { Mapping = typeMapping, MappedItems = mappedItems });
      } //foreach typeMapping
    }

    private FragmentDef GetFragmentDef(string name) {
      var fragm = _requestContext.ParsedRequest.Fragments.FirstOrDefault(f => f.Name == name);
      return fragm; 
    } 

    private void ValidateMappedFieldAndProcessSubset(MappedSelectionField mappedField) {
      var typeDef = mappedField.Resolver.Field.TypeRef.TypeDef;
      var selField = mappedField.Field;
      var selSubset = selField.SelectionSubset;
      var typeName = typeDef.Name; 
      switch(typeDef) {
        case ScalarTypeDef _:
        case EnumTypeDef _:
          if (selSubset != null)
            AddError($"Field '{selField.Key}' of type '{typeName}' may not have a selection subset.", selSubset);
          break;
        
        default: // ObjectType, Union or Interface 
          if (selSubset == null) {
            AddError($"Field '{selField.Key}' of type '{typeName}' must have a selection subset.", selField);
            return; 
          }
          _pendingSelectionSets.Add(new PendingSelectionSet() {
            SubSet = selSubset, OverType = typeDef, Directives = selField.Directives
          });
          break;
      }
    }


  } // class
}