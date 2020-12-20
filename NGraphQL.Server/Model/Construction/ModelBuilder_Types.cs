﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NGraphQL.CodeFirst;
using NGraphQL.Core.Scalars;
using NGraphQL.Introspection;
using NGraphQL.Utilities;

namespace NGraphQL.Model.Construction {

  public partial class ModelBuilder {

    private bool RegisterGraphQLTypesAndScalars() {
      foreach (var module in _server.Modules) {
        var mName = module.Name;
        // scalars
        foreach (var scalarType in module.ScalarTypes) {
          var scalar = (Scalar)Activator.CreateInstance(scalarType);
          var sTypeDef = new ScalarTypeDef(scalar);
          RegisterTypeDef(sTypeDef);
        }
        // other types
        foreach (var type in module.EnumTypes)
          CreateRegisterTypeDef(type, module, TypeKind.Enum);
        foreach (var type in module.ObjectTypes)
          CreateRegisterTypeDef(type, module, TypeKind.Object);
        foreach (var type in module.InputTypes)
          CreateRegisterTypeDef(type, module, TypeKind.InputObject);
        foreach (var type in module.InterfaceTypes)
          CreateRegisterTypeDef(type, module, TypeKind.Interface);
        foreach (var type in module.UnionTypes)
          CreateRegisterTypeDef(type, module, TypeKind.Union);
      } // foreach module

      return !_model.HasErrors;
    } //method


    private void CreateRegisterTypeDef(Type type, GraphQLModule module, TypeKind typeKind) {
      try {
        if (_model.TypesByClrType.ContainsKey(type)) {
          AddError($"Duplicate registration of type {type.Name}, module {module.Name}.");
          return;
        }
        var typeName = GetGraphQLName(type);
        if (_model.TypesByName.ContainsKey(typeName)) {
          AddError($"GraphQL type {typeName} already registered; CLR type: {type}, module: {module.Name}.");
          return;
        }
        var typeDef = CreateTypeDef(type, typeName, typeKind, module.Name);
        if (typeDef == null)
          return;
        typeDef.Module = module;
        typeDef.TypeRole = TypeRole.DataType;
        var hideAttr = type.GetAttribute<HiddenAttribute>();
        if (hideAttr != null)
          typeDef.Hidden = true;
        RegisterTypeDef(typeDef); 
      } catch (Exception ex) {
        AddError($"FATAL: Failed to register type {type}, error: {ex}. ");
      }
    }

    private void RegisterTypeDef(TypeDefBase typeDef) {
        _model.Types.Add(typeDef);
        _model.TypesByName.Add(typeDef.Name, typeDef);
        if (typeDef.ClrType != null) { //schema has no CLR type
          if (typeDef.Kind != TypeKind.Scalar)
            typeDef.Description = _docLoader.GetDocString(typeDef.ClrType, typeDef.ClrType);
          if (typeDef.IsDefaultForClrType)
            _model.TypesByClrType.Add(typeDef.ClrType, typeDef);
        }
    }

    private TypeDefBase CreateTypeDef (Type type, string typeName, TypeKind typeKind, string moduleName) {
      // Enum
      switch (typeKind) {
        case TypeKind.Enum:
          if (!type.IsEnum) {
            AddError($"Type {type} cannot be registered as Enum GraphQL type, must be enum; module: {moduleName}");
            return null; 
          }
          var flagsAttr = type.GetAttribute<FlagsAttribute>();
          return new EnumTypeDef(typeName, type, isFlagSet: flagsAttr != null);

        case TypeKind.Object:
          return new ObjectTypeDef(typeName, type);
        case TypeKind.Interface:
          return new InterfaceTypeDef(typeName, type);
        case TypeKind.InputObject:
          return new InputObjectTypeDef(typeName, type);
        case TypeKind.Union:
          return new UnionTypeDef(typeName, type);
      }
      // should never happen
      return null;
    }

    private TypeRef GetTypeRef(Type type, ICustomAttributeProvider attributeSource, string location) {
      var scalarAttr = attributeSource.GetAttribute<ScalarAttribute>();

      UnwrapClrType(type, attributeSource, out var baseType, out var kinds);

      TypeDefBase typeDef;
      if (scalarAttr != null) {
        typeDef = _model.GetScalarTypeDef(scalarAttr.ScalarName);
        if (type == null) {
          AddError($"{location}: scalar type {scalarAttr.ScalarName} is not defined. ");
          return null;
        }
      } else if (_model.TypesByEntityType.TryGetValue(baseType, out var mappedTypeDef))
        typeDef = mappedTypeDef;
      else if (!_model.TypesByClrType.TryGetValue(baseType, out typeDef)) {
        AddError($"{location}: type {baseType} is not registered. ");
        return null;
      }

      // add typeDef kind to kinds list and find/create type ref
      var allKinds = new List<TypeKind>();
      allKinds.Add(typeDef.Kind);

      // Flags enums are represented by enum arrays
      if (typeDef.IsEnumFlagArray()) {
        allKinds.Add(TypeKind.NotNull);
        allKinds.Add(TypeKind.List);
      }

      allKinds.AddRange(kinds);
      var typeRef = typeDef.GetCreateTypeRef(allKinds);
      return typeRef;
    }

    private void UnwrapClrType(Type type, ICustomAttributeProvider attributeSource, out Type baseType, out List<TypeKind> kinds) {
      kinds = new List<TypeKind>();
      bool notNull = attributeSource.GetAttribute<NullAttribute>() == null;
      Type valueTypeUnder;

      if (type.IsGenericListOrArray(out baseType, out var rank)) {
        valueTypeUnder = Nullable.GetUnderlyingType(baseType);
        baseType = valueTypeUnder ?? baseType;
        var withNulls = attributeSource.GetAttribute<WithNullsAttribute>() != null || valueTypeUnder != null;
        if (!withNulls)
          kinds.Add(TypeKind.NotNull);
        for (int i = 0; i < rank; i++)
          kinds.Add(TypeKind.List);
        if (notNull)
          kinds.Add(TypeKind.NotNull);
        return;
      }

      // not array      
      baseType = type;
      // check for nullable value type
      valueTypeUnder = Nullable.GetUnderlyingType(type);
      if (valueTypeUnder != null) {
        baseType = valueTypeUnder;
        notNull = false;
      }

      if (notNull)
        kinds.Add(TypeKind.NotNull);
    }

  }//class
}
