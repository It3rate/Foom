using NumbersAPI.Commands;
using NumbersAPI.Motion;
using NumbersCore.Primitives;

namespace NumbersAPI.CommandEngine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public interface ITask
    {
	    int Id { get; }
	    ICommand Command { get; }
	    CommandAgent Agent { get; set; }
        TaskTimer Timer { get; }
        bool IsValid { get; }
	    void RunTask();
	    void UnRunTask();
        //void OnAddedToCommand(ICommand command);
    }

    public interface ICreateTask
    {
    }
}
