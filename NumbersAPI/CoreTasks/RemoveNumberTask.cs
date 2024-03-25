namespace NumbersAPI.CoreTasks;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NumbersAPI.CommandEngine;
using NumbersCore.Primitives;
using NumbersCore.Utils;

public class RemoveNumberTask : TaskBase
{
    public Number Number { get; }
    public Domain Domain { get; }
    public PRange PRange { get; }

    public override bool IsValid => true;

    public RemoveNumberTask(Number number)
    {
        Number = number;
        Domain = number.Domain; // domain is removed when number is removed
        PRange = number.Value; // Its possible with a shared focal that the value has been changed and original 'forgotten'
    }
    public override void RunTask()
    {
        Number.Domain.RemoveNumber(Number);
    }

    public override void UnRunTask()
    {
        Domain.AddNumber(Number);
        Number.Value = PRange;
    }
}
