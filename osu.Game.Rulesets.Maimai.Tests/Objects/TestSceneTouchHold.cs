﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Maimai.Objects;
using osu.Game.Rulesets.Maimai.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Graphics;
using System;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Maimai.Tests.Objects
{
    [TestFixture]
    public class TestSceneTouchHold : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(DrawableMaimaiTouchHold)
        };

        private readonly Container content;
        protected override Container<Drawable> Content => content;

        private int depthIndex;

        public TestSceneTouchHold()
        {
            base.Content.Add(content = new MaimaiInputManager(new RulesetInfo { ID = 0 }));

            AddStep("Miss Single", () => testSingle());
            AddStep("Hit Single", () => testSingle(true));
        }

        private void testSingle(bool auto = false)
        {
            var circle = new MaimaiTouchHold
            {
                StartTime = Time.Current + 1000,
                Duration = 5000,
                Position = new Vector2(0, 0)
            };

            circle.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty { });

            var drawable = CreateDrawableTouchHoldNote(circle, auto);

            Add(drawable);
        }

        protected virtual TestDrawableTouchHoldNote CreateDrawableTouchHoldNote(MaimaiTouchHold circle, bool auto) => new TestDrawableTouchHoldNote(circle, auto)
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Depth = depthIndex++,
        };

        protected class TestDrawableTouchHoldNote : DrawableMaimaiTouchHold
        {
            public TestDrawableTouchHoldNote(MaimaiTouchHold h, bool auto)
                : base(h)
            {
                this.Auto = auto;
            }

            public void TriggerJudgement() => UpdateResult(true);

            protected override void CheckForResult(bool userTriggered, double timeOffset)
            {
                if (this.Auto && !userTriggered && timeOffset > 0)
                {
                    // force success
                    ApplyResult(r => r.Type = HitResult.Perfect);
                }
                else
                    base.CheckForResult(userTriggered, timeOffset);
            }
        }
    }
}