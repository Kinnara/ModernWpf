using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.Repeater;

[TestClass]
public class IndexPathApiTests
{
    [TestMethod]
    public void ValidateIndexPath()
    {
        WpfTestHost.Run(() =>
        {
            var path = IndexPath.CreateFromIndices(null);
            Assert.AreEqual(0, path.GetSize());

            path = IndexPath.CreateFrom(5);
            Assert.AreEqual(1, path.GetSize());
            Assert.AreEqual(5, path.GetAt(0));

            path = IndexPath.CreateFrom(1, 2);
            Assert.AreEqual(2, path.GetSize());
            Assert.AreEqual(1, path.GetAt(0));
            Assert.AreEqual(2, path.GetAt(1));

            Assert.AreEqual(0, IndexPath.CreateFrom(0, 1).CompareTo(IndexPath.CreateFrom(0, 1)));
            Assert.AreEqual(-1, IndexPath.CreateFrom(0, 1).CompareTo(IndexPath.CreateFrom(1, 0)));
            Assert.AreEqual(1, IndexPath.CreateFrom(0, 1).CompareTo(IndexPath.CreateFrom(0, 0)));

            Assert.AreEqual(-1, IndexPath.CreateFrom(1, 0).CompareTo(IndexPath.CreateFrom(1, 1)));
            Assert.AreEqual(0, IndexPath.CreateFrom(1, 0).CompareTo(IndexPath.CreateFrom(1, 0)));
            Assert.AreEqual(1, IndexPath.CreateFrom(1, 1).CompareTo(IndexPath.CreateFrom(1, 0)));

            var emptyPath = IndexPath.CreateFromIndices(null);
            Assert.AreEqual(0, emptyPath.CompareTo(emptyPath));

            var path1 = IndexPath.CreateFrom(1);
            Assert.AreEqual(-1, emptyPath.CompareTo(path1));
            Assert.AreEqual(1, path1.CompareTo(emptyPath));

            var path12 = IndexPath.CreateFrom(1, 2);
            Assert.AreEqual(-1, path1.CompareTo(path12));
            Assert.AreEqual(1, path12.CompareTo(path1));
        });
    }
}
