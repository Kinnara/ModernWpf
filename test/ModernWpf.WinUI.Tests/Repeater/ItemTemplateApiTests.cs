using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.Repeater;

[TestClass]
public class ItemTemplateApiTests
{
    [TestMethod]
    public void ValidateCustomRecyclingElementFactory()
    {
        WpfTestHost.Run(() =>
        {
            var owner = new StackPanel();
            var factory = new RecyclingElementFactoryDerived
            {
                RecyclePool = new RecyclePool(),
                Templates = { { "key", CreateTextBlockTemplate("uninitialized") } },
                SelectTemplateIdFunc = (data, elementOwner) =>
                {
                    Assert.AreSame(owner, elementOwner);
                    return "key";
                },
                GetElementFunc = (data, elementOwner, elementFromBase) =>
                {
                    Assert.AreSame(owner, elementOwner);
                    ((TextBlock)elementFromBase).Text = data!.ToString();
                    return elementFromBase;
                },
                ClearElementFunc = (element, elementOwner) =>
                {
                    Assert.AreSame(owner, elementOwner);
                    ((TextBlock)element).Text = "uninitialized";
                }
            };

            var element = (TextBlock)factory.GetElement(new ElementFactoryGetArgs { Parent = owner, Data = 3 });
            Assert.AreEqual("3", element.Text);

            factory.RecycleElement(new ElementFactoryRecycleArgs { Parent = owner, Element = element });
            Assert.AreEqual("uninitialized", element.Text);
        });
    }

    [TestMethod]
    public void ValidateRecyclingElementFactoryWithSingleTemplate()
    {
        WpfTestHost.Run(() =>
        {
            var owner = new StackPanel();
            var factory = new RecyclingElementFactory
            {
                RecyclePool = new RecyclePool(),
                Templates = { { "key", CreateTextBlockTemplate("single") } }
            };
            factory.SelectTemplateKey += (sender, args) =>
            {
                throw new InvalidOperationException("SelectTemplateKey should not be raised when using a single template.");
            };

            var element = (TextBlock)factory.GetElement(new ElementFactoryGetArgs { Parent = owner, Data = 0 });

            Assert.AreEqual("single", element.Text);
            Assert.AreEqual("key", RecyclePool.GetReuseKey(element));
        });
    }

    [TestMethod]
    public void ValidateRecyclingElementFactorySelectTemplateKey()
    {
        WpfTestHost.Run(() =>
        {
            var owner = new StackPanel();
            var factory = new RecyclingElementFactory
            {
                RecyclePool = new RecyclePool(),
                Templates =
                {
                    { "even", CreateTextBlockTemplate("even") },
                    { "odd", CreateTextBlockTemplate("odd") }
                }
            };
            factory.SelectTemplateKey += (sender, args) =>
            {
                Assert.AreSame(owner, args.Owner);
                args.TemplateKey = (int)args.DataContext % 2 == 0 ? "even" : "odd";
            };

            var even = (TextBlock)factory.GetElement(new ElementFactoryGetArgs { Parent = owner, Data = 2 });
            var odd = (TextBlock)factory.GetElement(new ElementFactoryGetArgs { Parent = owner, Data = 3 });

            Assert.AreEqual("even", even.Text);
            Assert.AreEqual("odd", odd.Text);
            Assert.AreEqual("even", RecyclePool.GetReuseKey(even));
            Assert.AreEqual("odd", RecyclePool.GetReuseKey(odd));
        });
    }

    [TestMethod]
    public void ValidateRecyclingElementFactoryReusesRecycledElement()
    {
        WpfTestHost.Run(() =>
        {
            var owner = new StackPanel();
            var factory = new RecyclingElementFactory
            {
                RecyclePool = new RecyclePool(),
                Templates = { { "key", CreateTextBlockTemplate("reused") } }
            };

            var element = factory.GetElement(new ElementFactoryGetArgs { Parent = owner, Data = 0 });
            factory.RecycleElement(new ElementFactoryRecycleArgs { Parent = owner, Element = element });
            var recycledElement = factory.GetElement(new ElementFactoryGetArgs { Parent = owner, Data = 1 });

            Assert.AreSame(element, recycledElement);
        });
    }

    [TestMethod]
    public void ValidateRecyclingElementFactoryWithNoTemplate()
    {
        WpfTestHost.Run(() =>
        {
            var factory = new RecyclingElementFactory { RecyclePool = new RecyclePool() };

            var exception = Assert.ThrowsException<InvalidOperationException>(() =>
                factory.GetElement(new ElementFactoryGetArgs { Parent = new StackPanel(), Data = 0 }));
            StringAssert.Contains(exception.Message, "Templates property cannot be null or empty.");
        });
    }

    [TestMethod]
    public void ValidateRecyclingElementFactoryWithMissingTemplateKey()
    {
        WpfTestHost.Run(() =>
        {
            var factory = new RecyclingElementFactory
            {
                RecyclePool = new RecyclePool(),
                Templates =
                {
                    { "known", CreateTextBlockTemplate("known") },
                    { "other", CreateTextBlockTemplate("other") }
                }
            };
            factory.SelectTemplateKey += (sender, args) => { args.TemplateKey = "missing"; };

            var exception = Assert.ThrowsException<InvalidOperationException>(() =>
                factory.GetElement(new ElementFactoryGetArgs { Parent = new StackPanel(), Data = 0 }));
            StringAssert.Contains(exception.Message, "No templates of key missing were found in the templates collection.");
        });
    }

    [TestMethod]
    public void ValidateRecyclingElementFactoryWithEmptyTemplateKey()
    {
        WpfTestHost.Run(() =>
        {
            var factory = new RecyclingElementFactory
            {
                RecyclePool = new RecyclePool(),
                Templates =
                {
                    { "known", CreateTextBlockTemplate("known") },
                    { "other", CreateTextBlockTemplate("other") }
                }
            };
            factory.SelectTemplateKey += (sender, args) => { args.TemplateKey = string.Empty; };

            var exception = Assert.ThrowsException<InvalidOperationException>(() =>
                factory.GetElement(new ElementFactoryGetArgs { Parent = new StackPanel(), Data = 0 }));
            StringAssert.Contains(exception.Message, "Please provide a valid template identifier");
        });
    }

    private static DataTemplate CreateTextBlockTemplate(string text)
    {
        return (DataTemplate)XamlReader.Parse(
            $@"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                  <TextBlock Text='{text}' />
               </DataTemplate>");
    }

    private sealed class RecyclingElementFactoryDerived : RecyclingElementFactory
    {
        public Func<object, UIElement, UIElement, UIElement>? GetElementFunc { get; set; }

        public Action<UIElement, UIElement>? ClearElementFunc { get; set; }

        public Func<object, UIElement, string>? SelectTemplateIdFunc { get; set; }

        protected override UIElement GetElementCore(ElementFactoryGetArgs args)
        {
            var element = base.GetElementCore(args);
            return GetElementFunc != null ? GetElementFunc(args.Data, args.Parent, element) : element;
        }

        protected override void RecycleElementCore(ElementFactoryRecycleArgs args)
        {
            base.RecycleElementCore(args);
            ClearElementFunc?.Invoke(args.Element, args.Parent);
        }

        protected override string OnSelectTemplateKeyCore(object dataContext, UIElement owner)
        {
            return SelectTemplateIdFunc != null ? SelectTemplateIdFunc(dataContext, owner) : base.OnSelectTemplateKeyCore(dataContext, owner);
        }
    }
}
