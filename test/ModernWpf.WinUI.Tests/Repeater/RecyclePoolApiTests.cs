using System;
using System.Windows.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.Repeater;

[TestClass]
public class RecyclePoolApiTests
{
    [TestMethod]
    public void ValidateElementsHaveCorrectKeys()
    {
        WpfTestHost.Run(() =>
        {
            const string buttonKey = "ButtonKey";
            const string textBlockKey = "TextBlockKey";
            const string stackPanelKey = "StackPanelKey";

            var pool = new RecyclePool();
            pool.PutElement(new Button(), buttonKey);
            pool.PutElement(new TextBlock(), textBlockKey);
            pool.PutElement(new StackPanel(), stackPanelKey);
            pool.PutElement(new Button(), buttonKey);
            pool.PutElement(new TextBlock(), textBlockKey);
            pool.PutElement(new StackPanel(), stackPanelKey);
            pool.PutElement(new Button(), buttonKey);
            pool.PutElement(new TextBlock(), textBlockKey);
            pool.PutElement(new StackPanel(), stackPanelKey);

            Assert.IsNotNull((Button)pool.TryGetElement(buttonKey));
            Assert.IsNotNull((Button)pool.TryGetElement(buttonKey));
            Assert.IsNotNull((Button)pool.TryGetElement(buttonKey));
            Assert.IsNull(pool.TryGetElement(buttonKey));

            Assert.IsNotNull((TextBlock)pool.TryGetElement(textBlockKey));
            Assert.IsNotNull((TextBlock)pool.TryGetElement(textBlockKey));
            Assert.IsNotNull((TextBlock)pool.TryGetElement(textBlockKey));
            Assert.IsNull(pool.TryGetElement(textBlockKey));

            Assert.IsNotNull((StackPanel)pool.TryGetElement(stackPanelKey));
            Assert.IsNotNull((StackPanel)pool.TryGetElement(stackPanelKey));
            Assert.IsNotNull((StackPanel)pool.TryGetElement(stackPanelKey));
            Assert.IsNull(pool.TryGetElement(stackPanelKey));

            Assert.ThrowsExactly<ArgumentNullException>(() => pool.PutElement(new Button(), null!, null));
            Assert.ThrowsExactly<ArgumentException>(() => pool.PutElement(new Button(), buttonKey, new Button()));
            Assert.ThrowsExactly<ArgumentNullException>(() => pool.TryGetElement(null!, null));
        });
    }

    [TestMethod]
    public void ValidateOwnershipWithStackPanel()
    {
        WpfTestHost.Run(() =>
        {
            var pool = new RecyclePool();
            var owner = new StackPanel();
            var child = new Button();
            owner.Children.Add(child);

            pool.PutElement(child, "Key", owner);
            var recycled = pool.TryGetElement("Key", owner);

            Assert.AreSame(child, recycled);
            Assert.AreEqual(0, owner.Children.IndexOf(child));
        });
    }

    [TestMethod]
    public void ValidateChildRemovedFromParentWhenOwnerIsDifferent()
    {
        WpfTestHost.Run(() =>
        {
            const string key1 = "Key1";
            const string key2 = "Key2";

            var pool = new RecyclePool();
            var parent1 = new StackPanel();
            var child1 = new Button();
            var child2 = new Button();
            parent1.Children.Add(child1);
            parent1.Children.Add(child2);

            pool.PutElement(child1, key1);
            pool.PutElement(child2, key2);

            var parent2 = new StackPanel();
            var recycled1 = (Button)pool.TryGetElement(key2, parent2);
            var recycled2 = (Button)pool.TryGetElement(key1, parent2);

            Assert.IsNull(recycled1.Parent);
            Assert.IsNull(recycled2.Parent);
        });
    }
}
