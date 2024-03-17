using Microsoft.VisualStudio.TestTools.UnitTesting;
using NumbersCore.Primitives;

namespace NumbersTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    [TestClass]
    public class WorkspaceTests
    {
	    private Brain _brain => Brain.ActiveBrain;
	    private Workspace _workspace;
	    private Trait _trait;
        private Focal _unitFocal;
	    private Focal _maxMin;
	    private Domain _domain;

	    [TestInitialize]
	    public void Init()
	    {
            _workspace = new Workspace(_brain);

		    _trait = Trait.CreateIn(_brain, "workspace tests");
            _unitFocal = new Focal(-4, 6);
		    _maxMin = new Focal(-54, 46);
		    _domain = new Domain(_trait, _unitFocal, _maxMin, "WorkspaceTests");
	    }

	    [TestMethod]
	    public void CoreWorkspaceTests()
	    {
            _workspace.AddDomains(true, _domain);
            Assert.AreEqual(_workspace.ActiveElementCount, 3); // domain, unit and range

            var f5 = new Focal(20, 90);
            var n5 = _domain.CreateNumber(f5);
            _workspace.AddElements(n5);
            Assert.AreEqual(4, _workspace.ActiveElementCount);
            _workspace.ClearAll();
            Assert.AreEqual( 0, _workspace.ActiveElementCount);
            _workspace.AddDomains(true, _domain);
            Assert.AreEqual(4, _workspace.ActiveElementCount);
            _workspace.RemoveDomains(true, _domain);
            Assert.AreEqual( 0, _workspace.ActiveElementCount);
            _workspace.AddDomains(false, _domain);
            Assert.AreEqual( 1, _workspace.ActiveElementCount);
            _workspace.AddDomains(true, _domain);
            Assert.AreEqual( 4, _workspace.ActiveElementCount);
            _workspace.RemoveTraits(true, _trait);
            Assert.AreEqual(0, _workspace.ActiveElementCount);
        }
    }
}
