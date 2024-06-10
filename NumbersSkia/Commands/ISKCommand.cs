using NumbersSkia.Agent;
using NumbersSkia.Mappers;
using NumbersAPI.Commands;

namespace NumbersSkia.Commands;

using NumbersSkia.Drawing;
using System.Collections.Generic;

public interface ISKCommand : ICommand
{
    List<int> PreviousSelection { get; }

    new MouseAgent Agent { get; }
    SKMapper Mapper { get; }
    SKSegment Guideline { get; }

    float T { get; } // this will be Number once default traits are in.
}
