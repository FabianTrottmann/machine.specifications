﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Machine.Specifications.Model;

namespace Machine.Specifications.Factories
{
  public class DescriptionFactory
  {
    private SpecificationFactory _specificationFactory;

    public DescriptionFactory()
    {
      _specificationFactory = new SpecificationFactory();
    }

    public Description CreateSpecificationFrom(object instance)
    {
      var type = instance.GetType();
      var fieldInfos = type.GetPrivateFields();
      string whenClause = "";
      List<FieldInfo> itFieldInfos = new List<FieldInfo>();
      FieldInfo whenFieldInfo = null;
      When when = null;

      var beforeAlls = ExtractPrivateFieldValues<Context>(instance, "before_all");
      var beforeEachs = ExtractPrivateFieldValues<Context>(instance, "before_each");
      beforeAlls.Reverse();
      beforeEachs.Reverse();

      var afterAlls = ExtractPrivateFieldValues<Context>(instance, "after_all");
      var afterEachs = ExtractPrivateFieldValues<Context>(instance, "after_each");

      foreach (FieldInfo info in fieldInfos)
      {
        if (info.FieldType == typeof (When))
        {
          whenFieldInfo = info;
          whenClause = info.Name.ReplaceUnderscores();
          when = (When)whenFieldInfo.GetValue(instance);
        }
        else if (info.FieldType == typeof(It) ||
            info.FieldType == typeof(It_should_throw))
        {
          itFieldInfos.Add(info);
        }
      }

      var description = new Description(type, instance, beforeEachs, beforeAlls, afterEachs, afterAlls, when) { 
            WhenClause = whenClause
          };

      foreach (FieldInfo info in itFieldInfos)
      {
        Specification specification = _specificationFactory.CreateSpecification(instance, info);
        description.AddSpecification(specification);
      }

      return description;
    }

    private List<T> ExtractPrivateFieldValues<T>(object instance, string name)
    {
      var delegates = new List<T>();
      var type = instance.GetType();
      while (type != null)
      {
        FieldInfo field = type.GetPrivateFieldsWith(typeof(T)).Where(x => x.Name == name).FirstOrDefault();
        if (field != null)
        {
          T val = (T)field.GetValue(instance);
          delegates.Add(val);
        }

        type = type.BaseType;
      }

      return delegates;
    }
  }
}