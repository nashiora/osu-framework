﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using NUnit.Framework;
using osu.Framework.Configuration;
using osu.Framework.Graphics;

namespace osu.Framework.Tests.Bindables
{
    [TestFixture]
    public class BindableBindingTest
    {
        [Test]
        public void TestPropagation()
        {
            Bindable<string> bindable1 = new Bindable<string>("default");
            Bindable<string> bindable2 = bindable1.GetBoundCopy();

            Assert.AreEqual("default", bindable1.Value);
            Assert.AreEqual(bindable2.Value, bindable1.Value);

            bindable1.Value = "new value";

            Assert.AreEqual("new value", bindable1.Value);
            Assert.AreEqual(bindable2.Value, bindable1.Value);
        }

        [Test]
        public void TestDisabled()
        {
            Bindable<string> bindable1 = new Bindable<string>("default");
            Bindable<string> bindable2 = bindable1.GetBoundCopy();

            bindable1.Disabled = true;

            Assert.Throws<InvalidOperationException>(() => bindable1.Value = "new value");
            Assert.Throws<InvalidOperationException>(() => bindable2.Value = "new value");

            bindable1.Disabled = false;

            bindable1.Value = "new value";

            Assert.AreEqual("new value", bindable1.Value);
            Assert.AreEqual("new value", bindable2.Value);

            bindable2.Value = "new value 2";

            Assert.AreEqual("new value 2", bindable1.Value);
            Assert.AreEqual("new value 2", bindable2.Value);
        }

        [Test]
        public void TestValueChanged()
        {
            Bindable<string> bindable1 = new Bindable<string>("default");
            Bindable<string> bindable2 = bindable1.GetBoundCopy();

            int changed1 = 0, changed2 = 0;

            bindable1.ValueChanged += _ => changed1++;
            bindable2.ValueChanged += _ => changed2++;

            bindable1.Value = "new value";

            Assert.AreEqual(1, changed1);
            Assert.AreEqual(1, changed2);

            bindable1.Value = "new value 2";

            Assert.AreEqual(2, changed1);
            Assert.AreEqual(2, changed2);

            // should not re-fire, as the value hasn't changed.
            bindable1.Value = "new value 2";

            Assert.AreEqual(2, changed1);
            Assert.AreEqual(2, changed2);
        }

        [Test]
        public void TestValueChangedWithUpstreamRejection()
        {
            Bindable<string> bindable1 = new Bindable<string>("won't change");
            Bindable<string> bindable2 = bindable1.GetBoundCopy();

            int changed1 = 0, changed2 = 0;

            bindable1.ValueChanged += v => changed1++;
            bindable2.ValueChanged += _ =>
            {
                bindable2.Value = "won't change";
                changed2++;
            };

            bindable1.Value = "new value";

            Assert.AreEqual("won't change", bindable1.Value);
            Assert.AreEqual(bindable1.Value, bindable2.Value);

            // bindable1 should only receive the final value changed, skipping the intermediary (overidden) one.
            Assert.AreEqual(1, changed1);
            Assert.AreEqual(2, changed2);
        }

        [Test]
        public void TestDisabledChanged()
        {
            Bindable<string> bindable1 = new Bindable<string>("default");
            Bindable<string> bindable2 = bindable1.GetBoundCopy();

            bool disabled1 = false, disabled2 = false;

            bindable1.DisabledChanged += v => disabled1 = v;
            bindable2.DisabledChanged += v => disabled2 = v;

            bindable1.Disabled = true;

            Assert.AreEqual(true, disabled1);
            Assert.AreEqual(true, disabled2);

            bindable1.Disabled = false;

            Assert.AreEqual(false, disabled1);
            Assert.AreEqual(false, disabled2);
        }

        [Test]
        public void TestDisabledChangedWithUpstreamRejection()
        {
            Bindable<string> bindable1 = new Bindable<string>("won't change");
            Bindable<string> bindable2 = bindable1.GetBoundCopy();

            int changed1 = 0, changed2 = 0;

            bindable1.DisabledChanged += v => changed1++;
            bindable2.DisabledChanged += _ =>
            {
                bindable2.Disabled = false;
                changed2++;
            };

            bindable1.Disabled = true;

            Assert.IsFalse(bindable1.Disabled);
            Assert.IsFalse(bindable2.Disabled);

            // bindable1 should only receive the final disabled changed, skipping the intermediary (overidden) one.
            Assert.AreEqual(1, changed1);
            Assert.AreEqual(2, changed2);
        }

        [Test]
        public void TestMinValueChanged()
        {
            BindableInt bindable1 = new BindableInt();
            BindableInt bindable2 = new BindableInt();
            bindable2.BindTo(bindable1);

            int minValue1 = 0, minValue2 = 0;

            bindable1.MinValueChanged += v => minValue1 = v;
            bindable2.MinValueChanged += v => minValue2 = v;

            bindable1.MinValue = 1;

            Assert.AreEqual(1, minValue1);
            Assert.AreEqual(1, minValue2);

            bindable1.MinValue = 2;

            Assert.AreEqual(2, minValue1);
            Assert.AreEqual(2, minValue2);
        }

        [Test]
        public void TestMinValueChangedWithUpstreamRejection()
        {
            BindableInt bindable1 = new BindableInt(1337); // Won't change
            BindableInt bindable2 = new BindableInt();
            bindable2.BindTo(bindable1);

            int changed1 = 0, changed2 = 0;

            bindable1.MinValueChanged += v => changed1++;
            bindable2.MinValueChanged += _ =>
            {
                bindable2.MinValue = 1337;
                changed2++;
            };

            bindable1.MinValue = 2;

            Assert.AreEqual(1337, bindable1.MinValue);
            Assert.AreEqual(bindable1.MinValue, bindable2.MinValue);

            // bindable1 should only receive the final value changed, skipping the intermediary (overidden) one.
            Assert.AreEqual(1, changed1);
            Assert.AreEqual(2, changed2);
        }

        [Test]
        public void TestMaxValueChanged()
        {
            BindableInt bindable1 = new BindableInt();
            BindableInt bindable2 = new BindableInt();
            bindable2.BindTo(bindable1);

            int minValue1 = 0, minValue2 = 0;

            bindable1.MaxValueChanged += v => minValue1 = v;
            bindable2.MaxValueChanged += v => minValue2 = v;

            bindable1.MaxValue = 1;

            Assert.AreEqual(1, minValue1);
            Assert.AreEqual(1, minValue2);

            bindable1.MaxValue = 2;

            Assert.AreEqual(2, minValue1);
            Assert.AreEqual(2, minValue2);
        }

        [Test]
        public void TestMaxValueChangedWithUpstreamRejection()
        {
            BindableInt bindable1 = new BindableInt(1337); // Won't change
            BindableInt bindable2 = new BindableInt();
            bindable2.BindTo(bindable1);

            int changed1 = 0, changed2 = 0;

            bindable1.MaxValueChanged += v => changed1++;
            bindable2.MaxValueChanged += _ =>
            {
                bindable2.MaxValue = 1337;
                changed2++;
            };

            bindable1.MaxValue = 2;

            Assert.AreEqual(1337, bindable1.MaxValue);
            Assert.AreEqual(bindable1.MaxValue, bindable2.MaxValue);

            // bindable1 should only receive the final value changed, skipping the intermediary (overidden) one.
            Assert.AreEqual(1, changed1);
            Assert.AreEqual(2, changed2);
        }

        [Test]
        public void TestUnbindOnDrawableDispose()
        {
            var drawable = new TestDrawable();

            drawable.SetValue(1);
            Assert.IsTrue(drawable.ValueChanged, "bound correctly");

            drawable.Dispose();
            drawable.ValueChanged = false;

            drawable.SetValue(2);
            Assert.IsFalse(drawable.ValueChanged, "unbound correctly");
        }

        [Test]
        public void TestUnbindOnDrawableDisposeSubClass()
        {
            var drawable = new TestSubDrawable();

            drawable.SetValue(1);
            Assert.IsTrue(drawable.ValueChanged, "bound correctly");
            Assert.IsTrue(drawable.ValueChanged2, "bound correctly");

            drawable.Dispose();
            drawable.ValueChanged = false;
            drawable.ValueChanged2 = false;

            drawable.SetValue(2);
            Assert.IsFalse(drawable.ValueChanged, "unbound correctly");
            Assert.IsFalse(drawable.ValueChanged2, "unbound correctly");
        }

        [Test]
        public void TestUnbindOnDrawableDisposeCached()
        {
            // Build cache
            var drawable = new TestDrawable();
            drawable.Dispose();

            TestUnbindOnDrawableDispose();
        }

        [Test]
        public void TestUnbindOnDrawableDisposeProperty()
        {
            var bindable = new Bindable<int>();

            bool valueChanged = false;
            bindable.ValueChanged += _ => valueChanged = true;

            var drawable = new TestDrawable2 { GetBindable = () => bindable };

            drawable.SetValue(1);
            Assert.IsTrue(valueChanged, "bound correctly");

            drawable.Dispose();
            valueChanged = false;

            drawable.SetValue(2);
            Assert.IsFalse(valueChanged, "unbound correctly");
        }

        [Test]
        public void TestUnbindOnDrawableDisposePropertyCached()
        {
            // Build cache
            var drawable = new TestDrawable2();
            drawable.Dispose();

            TestUnbindOnDrawableDispose();
        }

        [Test]
        public void TestUnbindFrom()
        {
            var bindable1 = new Bindable<int>(5);
            var bindable2 = new Bindable<int>();
            bindable2.BindTo(bindable1);

            Assert.AreEqual(bindable1.Value, bindable2.Value);

            bindable2.UnbindFrom(bindable1);
            bindable1.Value = 10;

            Assert.AreNotEqual(bindable1.Value, bindable2.Value);
        }

        private class TestDrawable : Drawable
        {
            public bool ValueChanged;

            private readonly Bindable<int> bindable = new Bindable<int>();

            public TestDrawable()
            {
                bindable.BindValueChanged(_ => ValueChanged = true);
            }

            public virtual void SetValue(int value) => bindable.Value = value;
        }

        private class TestSubDrawable : TestDrawable
        {
            public bool ValueChanged2;

            private readonly Bindable<int> bindable = new Bindable<int>();

            public TestSubDrawable()
            {
                bindable.BindValueChanged(_ => ValueChanged2 = true);
            }

            public override void SetValue(int value)
            {
                bindable.Value = value;
                base.SetValue(value);
            }
        }

        private class TestDrawable2 : Drawable
        {
            public Func<Bindable<int>> GetBindable;
            private Bindable<int> bindable => GetBindable();

            public void SetValue(int value) => bindable.Value = value;
        }
    }
}
