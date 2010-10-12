﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Boogie.ModelViewer.Base
{
  public class Provider : ILanguageProvider
  {
    public static Provider Instance = new Provider();
    private Provider() { }

    public bool IsMyModel(Model m)
    {
      return true;
    }

    public IEnumerable<IDisplayNode> GetStates(Model m)
    {
      yield return GenericNodes.Functions(m);
      yield return GenericNodes.Constants(m);
      foreach (var s in m.States)
        yield return new StateNode(s);
    }
  }

  public class StateNode : IDisplayNode
  {
    protected Model.CapturedState state;

    public StateNode(Model.CapturedState s)
    {
      state = s;
    }

    public virtual string Name
    {
      get { return state.Name; }
    }

    public virtual IEnumerable<string> Values
    {
      get { foreach (var v in state.Variables) yield return v; }
    }

    public virtual bool Expandable { get { return state.VariableCount != 0; } }

    public virtual IEnumerable<IDisplayNode> Expand()
    {
      foreach (var v in state.Variables) {
        yield return new ElementNode(v, state.TryGet(v));
      }
    }

    public object ViewSync { get; set; }
  }

  public class ElementNode : IDisplayNode
  {
    protected Model.Element elt;
    protected string name;

    public ElementNode(string name, Model.Element elt)
    {
      this.name = name;
      this.elt = elt;
    }

    public virtual string Name
    {
      get { return name; }
    }

    public virtual IEnumerable<string> Values
    {
      get
      {
        if (!(elt is Model.Uninterpreted))
          yield return elt.ToString();
        foreach (var tupl in elt.Names) {
          if (tupl.Func.Arity == 0)
            yield return tupl.Func.Name;
        }
      }
    }

    public virtual bool Expandable { get { return false; } }
    public virtual IEnumerable<IDisplayNode> Expand() { yield break; }

    public object ViewSync { get; set; }
  }

  public static class GenericNodes
  {
    public static IDisplayNode Function(Model.Func f)
    {
      return new ContainerNode<Model.FuncTuple>(f.Name, a => new AppNode(a), f.Apps);
    }

    public static IDisplayNode Functions(Model m)
    {
      return new ContainerNode<Model.Func>("Functions", f => f.Arity == 0 ? null : Function(f), m.Functions);
    }

    public static IDisplayNode Constants(Model m)
    {
      return new ContainerNode<Model.Func>("Constants", f => f.Arity != 0 ? null : new AppNode(f.Apps.First()), m.Functions);
    }
  }

  public class AppNode : ElementNode
  {
    protected Model.FuncTuple tupl;

    public AppNode(Model.FuncTuple t)
      : base(t.Func.Name, t.Result)
    {
      tupl = t;
      var sb = new StringBuilder();
      sb.Append(t.Func.Name);
      if (t.Args.Length > 0) {
        sb.Append("(");
        foreach (var a in t.Args)
          sb.Append(a.ToString()).Append(", ");
        sb.Length -= 2;
        sb.Append(")");
      }
      name = sb.ToString();
    }
  }

}
