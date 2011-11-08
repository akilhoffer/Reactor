using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace Reactor.Specs
{
    [TestClass]
    public abstract class SpecificationFor<T>
    {
        [TestInitialize]
        public virtual void Setup()
        {
            Context();
            Subject = InitializeSubject();
            Because();
        }

        [TestCleanup]
        public virtual void TearDown()
        {
        }

        protected T Subject;

        [DebuggerNonUserCode]
        public virtual void Context() { }

        public abstract T InitializeSubject();

        public virtual void Because() { }
    }
}
