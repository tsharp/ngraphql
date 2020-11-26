﻿using System;
using System.Collections.Generic;
using System.Text;
using NGraphQL.CodeFirst;

namespace NGraphQL.Core {

  public class TokenData {
    public string TermName;
    public string Text;
    public object ParsedValue;
  }

  public interface IScalarContext {
    void AddError(GraphQLError error, Exception sourceException = null);
  }

  public class Scalar {
    public readonly string Name;
    public readonly string Description;
    public Type DefaultClrType;
    public readonly bool IsCustom;

    public Scalar(string name, string description, Type defaultClrType, bool isCustom = false) {
      Name = name;
      Description = description;
      DefaultClrType = defaultClrType;
      IsCustom = isCustom;
    }

    public virtual object ParseToken(IFieldContext context, TokenData tokenData) {
      return tokenData.ParsedValue; // this is value converted by Irony parser
    }
    // from deserialized Json variable value
    public virtual object ConvertInputValue(object value) {
      return value;
    }

    // used in Schema doc output
    public virtual string ToSchemaDocString(object value) {
      if (value == null)
        return "null";
      return value.ToString();
    }

    public virtual object ToOutput(IFieldContext context, object value) {
      return value;
    }



  }
}