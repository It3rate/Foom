using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NumbersAPI.CommandEngine;
using NumbersCore.Primitives;
using NumbersCore.Utils;

namespace NumbersAPI.CoreTasks
{
    public class SelectionSetTask : TaskBase, ICreateTask
    {
        public override bool IsValid => throw new NotImplementedException();
    }
}
