#nullable disable

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Veldrid;
using osu.Framework.Testing;
using osuTK;
using osuTK.Graphics;

namespace osu.Framework.Tests.Visual.Containers
{
    public partial class TestSceneViewport3D : FrameworkTestScene
    {
        [Resolved]
        private FrameworkDebugConfigManager debugConfig { get; set; }

        private Drawable3D testCube;

        [SetUp]
        public void Setup() => Schedule(Clear);

        [TearDownSteps]
        public void TearDownSteps()
        {
        }

        [Test]
        public void TestOpaqueCubeIn3DSpace()
        {
            createCube();
        }

        private void createCube(Action<Drawable3D> setupAction = null) => AddStep("create cube", () =>
        {
            Child = new Container
            {
                Masking = true,
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(0.5f),
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Size = new Vector2(1),
                        Colour = new Colour4(0.1f, 0.1f, 0.1f, 1.0f),
                    },

                    new Viewport3D
                    {
                        RelativeSizeAxes = Axes.Both,
                        Size = new Vector2(1),
                        Children = new Drawable3D[]
                        {
                            testCube = new Drawable3D(),
                        }
                    },
                }
            };

            setupAction?.Invoke(testCube);
        });

        private partial class TestCube // : Cube
        {
        }
    }
}

