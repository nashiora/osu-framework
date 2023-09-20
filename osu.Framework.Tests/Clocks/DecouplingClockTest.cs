﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using NUnit.Framework;
using osu.Framework.Timing;

namespace osu.Framework.Tests.Clocks
{
    [TestFixture]
    public class DecouplingClockTest
    {
        private TestClockWithRange source = null!;
        private DecouplingFramedClock decoupleable = null!;

        [SetUp]
        public void SetUp()
        {
            source = new TestClockWithRange();

            decoupleable = new DecouplingFramedClock();
            decoupleable.ChangeSource(source);
        }

        #region Start/stop by decoupleable

        /// <summary>
        /// Tests that the source clock starts when the coupled clock starts.
        /// </summary>
        [Test]
        public void TestSourceStartedByCoupled()
        {
            decoupleable.Start();

            Assert.IsTrue(source.IsRunning, "Source should be running.");
        }

        /// <summary>
        /// Tests that the source clock stops when the coupled clock stops.
        /// </summary>
        [Test]
        public void TestSourceStoppedByCoupled()
        {
            decoupleable.Start();
            decoupleable.Stop();

            Assert.IsFalse(source.IsRunning, "Source should not be running.");
        }

        /// <summary>
        /// Tests that the source clock starts when the decoupled clock starts.
        /// </summary>
        [Test]
        public void TestSourceStartedByDecoupled()
        {
            decoupleable.AllowDecoupling = true;
            decoupleable.Start();

            Assert.IsTrue(source.IsRunning, "Source should be running.");
        }

        /// <summary>
        /// Tests that the source clock stops when the decoupled clock stops.
        /// </summary>
        [Test]
        public void TestSourceStoppedByDecoupled()
        {
            decoupleable.Start();

            decoupleable.AllowDecoupling = true;
            decoupleable.Stop();

            Assert.IsFalse(source.IsRunning, "Source should not be running.");
        }

        #endregion

        #region Start/stop by source

        /// <summary>
        /// Tests that the coupled clock starts when the source clock starts.
        /// </summary>
        [Test]
        public void TestCoupledStartedBySourceClock()
        {
            source.Start();
            decoupleable.ProcessFrame();

            Assert.IsTrue(decoupleable.IsRunning, "Coupled should be running.");
        }

        /// <summary>
        /// Tests that the coupled clock stops when the source clock stops.
        /// </summary>
        [Test]
        public void TestCoupledStoppedBySourceClock()
        {
            decoupleable.Start();

            source.Stop();
            decoupleable.ProcessFrame();

            Assert.IsFalse(decoupleable.IsRunning, "Coupled should not be running.");
            Assert.That(decoupleable.CurrentTime, Is.EqualTo(source.CurrentTime));
        }

        /// <summary>
        /// Tests that the decoupled clock doesn't start when the source clock starts.
        /// </summary>
        [Test]
        public void TestDecoupledNotStartedBySourceClock()
        {
            decoupleable.AllowDecoupling = true;

            source.Start();
            decoupleable.ProcessFrame();

            Assert.IsFalse(decoupleable.IsRunning, "Decoupled should not be running.");
        }

        /// <summary>
        /// Tests that the decoupled clock doesn't stop when the source clock stops.
        /// </summary>
        [Test]
        public void TestDecoupledNotStoppedBySourceClock()
        {
            decoupleable.Start();
            decoupleable.AllowDecoupling = true;

            source.Stop();
            decoupleable.ProcessFrame();

            Assert.IsTrue(decoupleable.IsRunning, "Decoupled should be running.");
        }

        #endregion

        #region Source changes

        [Test]
        public void SourceChangeTransfersValueAdjustable()
        {
            // For decoupled clocks, value transfer is preferred in the direction of the track if possible.
            // In other words, we want to keep our current time even if the source changes, as long as the source supports it.
            //
            // This tests the case where it is supported.

            const double first_source_time = 256000;
            const double second_source_time = 128000;

            source.Seek(first_source_time);
            source.Start();

            var secondSource = new TestClock
            {
                // importantly, test a value lower than the original source.
                // this is to both test value transfer *and* the case where time is going backwards, as
                // some clocks have special provisions for this.
                CurrentTime = second_source_time
            };

            decoupleable.ProcessFrame();
            Assert.That(decoupleable.CurrentTime, Is.EqualTo(first_source_time));

            decoupleable.ChangeSource(secondSource);
            decoupleable.ProcessFrame();

            Assert.That(secondSource.CurrentTime, Is.EqualTo(first_source_time));
            Assert.That(decoupleable.CurrentTime, Is.EqualTo(first_source_time));
        }

        [Test]
        public void SourceChangeTransfersValueNonAdjustable()
        {
            // For decoupled clocks, value transfer is preferred in the direction of the track if possible.
            // In other words, we want to keep our current time even if the source changes, as long as the source supports it.
            //
            // This tests the case where it is NOT supported.

            const double first_source_time = 256000;
            const double second_source_time = 128000;

            source.Seek(first_source_time);
            source.Start();

            var secondSource = new TestNonAdjustableClock
            {
                // importantly, test a value lower than the original source.
                // this is to both test value transfer *and* the case where time is going backwards, as
                // some clocks have special provisions for this.
                CurrentTime = second_source_time
            };

            decoupleable.ProcessFrame();
            Assert.That(decoupleable.CurrentTime, Is.EqualTo(first_source_time));

            decoupleable.ChangeSource(secondSource);
            decoupleable.ProcessFrame();

            Assert.That(secondSource.CurrentTime, Is.EqualTo(second_source_time));
            Assert.That(decoupleable.CurrentTime, Is.EqualTo(second_source_time));
        }

        #endregion

        #region Offset start

        /// <summary>
        /// Tests that the coupled clock seeks to the correct position when the source clock starts.
        /// </summary>
        [Test]
        public void TestCoupledStartBySourceWithSourceOffset()
        {
            source.Seek(1000);

            source.Start();
            decoupleable.ProcessFrame();

            Assert.AreEqual(source.CurrentTime, decoupleable.CurrentTime, "Coupled time should match source time.");
        }

        /// <summary>
        /// Tests that the coupled clock seeks the source clock to its time when it starts.
        /// </summary>
        [Test]
        public void TestCoupledStartWithSouceOffset()
        {
            source.Seek(1000);
            decoupleable.Start();

            Assert.AreEqual(0, source.CurrentTime);
            Assert.AreEqual(source.CurrentTime, decoupleable.CurrentTime, "Coupled time should match source time.");
        }

        [Test]
        public void TestFromNegativeCoupledMode()
        {
            decoupleable.AllowDecoupling = false;
            decoupleable.Seek(-1000);

            decoupleable.ProcessFrame();

            Assert.AreEqual(0, source.CurrentTime);
            Assert.AreEqual(0, decoupleable.CurrentTime);
        }

        /// <summary>
        /// Tests that the decoupled clocks starts the source as a result of being able to handle the current time.
        /// </summary>
        [Test]
        public void TestDecoupledStartsSourceIfAllowable()
        {
            decoupleable.AllowDecoupling = true;
            // decoupleable.CustomAllowableErrorMilliseconds = 1000;
            decoupleable.Seek(-50);
            decoupleable.ProcessFrame();
            decoupleable.Start();

            // Delay a bit to make sure the clock crosses the 0 boundary
            Thread.Sleep(100);
            decoupleable.ProcessFrame();

            Assert.That(source.IsRunning, Is.True);
        }

        /// <summary>
        /// Tests that during forward playback the decoupled clock always moves in the forwards direction after starting the source clock.
        /// For this test, the source clock is started when the decoupled time crosses the 0ms-boundary.
        /// </summary>
        [Test]
        public void TestForwardPlaybackDecoupledTimeDoesNotRewindAfterSourceStarts()
        {
            decoupleable.AllowDecoupling = true;
            // decoupleable.CustomAllowableErrorMilliseconds = 1000;
            decoupleable.Seek(-50);
            decoupleable.ProcessFrame();
            decoupleable.Start();

            // Delay a bit to make sure the clock crosses the 0ms boundary
            Thread.Sleep(100);
            decoupleable.ProcessFrame();

            // Make sure that time doesn't rewind. Note that the source clock does not move by itself,
            double last = decoupleable.CurrentTime;
            decoupleable.ProcessFrame();
            Assert.That(decoupleable.CurrentTime, Is.GreaterThanOrEqualTo(last));
        }

        /// <summary>
        /// Tests that during backwards playback the decoupled clock always moves in the backwards direction after starting the source clock.
        /// For this test, the source clock is started when the decoupled time crosses the 1000ms-boundary.
        /// </summary>
        [Test]
        public void TestBackwardPlaybackDecoupledTimeDoesNotRewindAfterSourceStarts()
        {
            source.MaxTime = 1000;
            source.Rate = -1;
            decoupleable.AllowDecoupling = true;
            // decoupleable.CustomAllowableErrorMilliseconds = 1000;

            // Bring the source clock into a good state by seeking to a valid time
            decoupleable.Seek(1000);
            decoupleable.Start();
            decoupleable.ProcessFrame();
            decoupleable.Stop();

            decoupleable.Seek(1050);
            decoupleable.ProcessFrame();
            decoupleable.Start();

            // Delay a bit to make sure the clock crosses the 1000ms boundary
            Thread.Sleep(100);
            decoupleable.ProcessFrame();

            // Make sure that time doesn't rewind
            double last = decoupleable.CurrentTime;
            decoupleable.ProcessFrame();
            Assert.That(decoupleable.CurrentTime, Is.LessThanOrEqualTo(last));
        }

        /// <summary>
        /// Tests that the decoupled clock seeks the source clock to its time when it starts.
        /// </summary>
        [Test]
        public void TestDecoupledStartWithSourceOffset()
        {
            decoupleable.AllowDecoupling = true;

            source.Seek(1000);
            decoupleable.Start();

            Assert.AreEqual(0, source.CurrentTime);
            Assert.AreEqual(source.CurrentTime, decoupleable.CurrentTime, "Deoupled time should match source time.");
        }

        #endregion

        #region Seeking

        /// <summary>
        /// Tests that the source clock is seeked when the coupled clock is seeked.
        /// </summary>
        [Test]
        public void TestSourceSeekedByCoupledSeek()
        {
            decoupleable.Seek(1000);

            Assert.AreEqual(source.CurrentTime, decoupleable.CurrentTime, "Source time should match coupled time.");
        }

        /// <summary>
        /// Tests that the coupled clock is seeked when the source clock is seeked.
        /// </summary>
        [Test]
        public void TestCoupledSeekedBySourceSeek()
        {
            decoupleable.Start();

            source.Seek(1000);
            decoupleable.ProcessFrame();

            Assert.AreEqual(source.CurrentTime, decoupleable.CurrentTime, "Coupled time should match source time.");
        }

        /// <summary>
        /// Tests that the source clock is seeked when the decoupled clock is seeked.
        /// </summary>
        [Test]
        public void TestSourceSeekedByDecoupledSeek()
        {
            decoupleable.AllowDecoupling = true;
            decoupleable.Seek(1000);

            Assert.AreEqual(decoupleable.CurrentTime, 1000, "Decoupled time should match seek target.");
            // Seek on the source is not performed as the clock is stopped.
            Assert.AreNotEqual(source.CurrentTime, decoupleable.CurrentTime, "Source time should not match coupled time.");
        }

        /// <summary>
        /// Tests that the coupled clock is not seeked while stopped and the source clock is seeked.
        /// </summary>
        [Test]
        public void TestDecoupledNotSeekedBySourceSeekWhenStopped()
        {
            decoupleable.AllowDecoupling = true;

            source.Seek(1000);
            decoupleable.ProcessFrame();

            Assert.AreEqual(0, decoupleable.CurrentTime);
            Assert.AreNotEqual(source.CurrentTime, decoupleable.CurrentTime, "Coupled time should not match source time.");
        }

        /// <summary>
        /// Tests that seeking a decoupled clock negatively does not cause it to seek to the positive source time.
        /// </summary>
        [Test]
        public void TestDecoupledNotSeekedPositivelyByFailedNegativeSeek()
        {
            decoupleable.AllowDecoupling = true;
            decoupleable.Start();

            decoupleable.Seek(-5000);

            Assert.That(source.IsRunning, Is.False);
            Assert.That(decoupleable.IsRunning, Is.True);
            Assert.That(decoupleable.CurrentTime, Is.LessThan(0));
        }

        #endregion

        /// <summary>
        /// Tests that the state of the decouplable clock is preserved when it is stopped after processing a frame.
        /// </summary>
        [Test]
        public void TestStoppingAfterProcessingFramePreservesState()
        {
            decoupleable.Start();
            source.CurrentTime = 1000;

            decoupleable.ProcessFrame();
            decoupleable.Stop();

            Assert.AreEqual(source.CurrentTime, decoupleable.CurrentTime, "Decoupled should match source time.");
        }

        /// <summary>
        /// Tests that the state of the decouplable clock is preserved when it is stopped after having being started by the source clock.
        /// </summary>
        [Test]
        public void TestStoppingAfterStartingBySourcePreservesState()
        {
            source.Start();
            source.CurrentTime = 1000;

            decoupleable.ProcessFrame();
            decoupleable.Stop();

            Assert.AreEqual(source.CurrentTime, decoupleable.CurrentTime, "Decoupled should match source time.");
        }

        private class TestClockWithRange : TestClock
        {
            public double MinTime => 0;
            public double MaxTime { get; set; } = double.PositiveInfinity;

            public override bool Seek(double position)
            {
                if (Math.Clamp(position, MinTime, MaxTime) != position)
                    return false;

                return base.Seek(position);
            }
        }
    }
}
