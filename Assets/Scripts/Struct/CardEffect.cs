﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Struct
{
    public abstract class CardEffect:Effect
    {
        public string trigger;

        public enum FunctionTarget
        {
            none,
            self,
            target
        }

        public FunctionTarget functionTarget;

        public abstract string GetDescription();
        
    }
}
