﻿using System;
using System.Collections.Generic;
using System.Text;

namespace GameCreator.Interpreter
{
    class Access : Expr
    {
        public Expr Lefthand;
        public string Name;
        public Expr[] Indices;
        public Access(Expr left, string n, Expr[] ind, int l, int c) : base(l, c) { Lefthand = left; Name = n; Indices = ind; }
        private long info(out int i1, out int i2)
        {
            Value left = Value.Zero, v1 = Value.Zero, v2 = Value.Zero;
            if (Lefthand != null)
            {
                left = Lefthand.Eval();
                if (!left.IsReal) Error("Wrong type of variable index");
            }
            if (Indices.Length != 0)
            {
                v1 = Indices[0].Eval();
                if (!v1.IsReal) Error("Wrong type of array index");
                if (Indices.Length == 2)
                {
                    v2 = Indices[1].Eval();
                    if (!v2.IsReal) Error("Wrong type of array index");
                }
                i1 = Indices.Length == 1 ? 0 : (int)Math.Round(v1.Real);
                i2 = Indices.Length == 1 ? (int)Math.Round(v1.Real) : (int)Math.Round(v2.Real);
                if (i1 < 0 || i2 < 0) Error("Negative array index");
            }
            else { i1 = 0; i2 = 0; }
            if (i1 >= 32000 || i2 >= 32000) Error("Array index >= 32000");
            return (long)Math.Round(left.Real);
        }
        public void Set(Value v)
        {
            int i1, i2;
            long left = info(out i1, out i2);
            if (Lefthand == null)
                Env.SetVar(Name, i1, i2, v);
            else
                Env.SetVar(left, Name, i1, i2, v);
        }
        public override Value Eval()
        {
            int i1, i2;
            long left = info(out i1, out i2);
            if (Lefthand == null)
            {
                if (i1 != 0 || i2 != 0)
                {
                    if (!Env.VariableExists(Name) || !Env.Variable(Name).CheckIndex(i1, i2))
                        Error("Unknown variable " + Name + " or array index out of bounds");
                }
                else
                    if (!Env.VariableExists(Name)) Error("Unknown variable " + Name);
                return Env.GetVar(Name, i1, i2);
            }
            else
            {
                if (i1 != 0 || i2 != 0)
                {
                    if (!Env.VariableExists(left, Name) || !Env.Variable(left, Name).CheckIndex(i1, i2))
                        Error("Unknown variable " + Name + " or array index out of bounds");
                }
                else
                    if (!Env.VariableExists(left, Name)) Error("Unknown variable " + Name);
                return Env.GetVar(left, Name, i1, i2);
            }
        }
    }
}
