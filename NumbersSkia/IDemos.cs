using NumbersSkia.Agent;
using NumbersSkia.Mappers;

namespace NumbersSkia.Demo;

public delegate SKWorkspaceMapper PageCreator();
public interface IDemos
{
    List<PageCreator> Pages { get; }
    SKWorkspaceMapper NextTest(MouseAgent agent);
    SKWorkspaceMapper PreviousTest(MouseAgent agent);
    SKWorkspaceMapper Reload(MouseAgent agent);

    SKWorkspaceMapper LoadTest(int index, MouseAgent mouseAgent);
}
