﻿using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Game.Audio;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.Configuration;
using osu.Game.Rulesets.Sentakki.Objects.Drawables.Pieces.TouchHolds;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki.Objects.Drawables
{
    public partial class DrawableTouchHold : DrawableSentakkiHitObject
    {
        public new TouchHold HitObject => (TouchHold)base.HitObject;

        public override bool HandlePositionalInput => true;

        private SentakkiInputManager sentakkiActionInputManager = null!;
        internal SentakkiInputManager SentakkiActionInputManager => sentakkiActionInputManager ??= (SentakkiInputManager)GetContainingInputManager();

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => TouchHoldBody.ReceivePositionalInputAt(screenSpacePos);

        public TouchHoldBody TouchHoldBody = null!;

        private PausableSkinnableSound holdSample = null!;

        public DrawableTouchHold()
            : this(null)
        {
        }

        public DrawableTouchHold(TouchHold? hitObject)
            : base(hitObject)
        {
        }

        [BackgroundDependencyLoader]
        private void load(SentakkiRulesetConfigManager? sentakkiConfigs)
        {
            sentakkiConfigs?.BindWith(SentakkiRulesetSettings.TouchAnimationDuration, AnimationDuration);
            Colour = Color4.SlateGray;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Scale = new Vector2(0f);
            Alpha = 0;
            AddRangeInternal(new Drawable[]
            {
                TouchHoldBody = new TouchHoldBody(),
                holdSample = new PausableSkinnableSound
                {
                    Volume = { Value = 1 },
                    Looping = true,
                    Frequency = { Value = 1 }
                }
            });

            isHitting.BindValueChanged(b =>
            {
                if (b.NewValue) beginHold();
                else endHold();
            });
        }

        protected override void LoadSamples()
        {
            base.LoadSamples();

            var firstSample = HitObject.Samples.FirstOrDefault();

            if (firstSample != null)
            {
                var clone = HitObject.SampleControlPoint.ApplyTo(firstSample).With("spinnerspin");

                holdSample.Samples = new ISampleInfo[] { clone };
                holdSample.Frequency.Value = 1;
            }
        }

        public override void StopAllSamples()
        {
            base.StopAllSamples();
            holdSample?.Stop();
        }

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        protected override void OnFree()
        {
            base.OnFree();

            holdSample.ClearSamples();
            isHitting.Value = false;
            totalHoldTime = 0;
        }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();
            double fadeIn = AdjustedAnimationDuration;
            this.FadeInFromZero(fadeIn).ScaleTo(1, fadeIn);

            using (BeginDelayedSequence(fadeIn))
            {
                TouchHoldBody.ProgressPiece.TransformBindableTo(TouchHoldBody.ProgressPiece.ProgressBindable, 1, ((IHasDuration)HitObject).Duration);
            }
        }

        private readonly Bindable<bool> isHitting = new Bindable<bool>();

        private double totalHoldTime;

        private void beginHold()
        {
            Colour = Color4.White;
            holdSample.Play();
        }

        private void endHold()
        {
            Colour = Color4.SlateGray;
            holdSample.Stop();
        }

        protected override void Update()
        {
            base.Update();

            if (isHitting.Value)
            {
                totalHoldTime = Math.Clamp(totalHoldTime + Time.Elapsed, 0, ((IHasDuration)HitObject).Duration);
                holdSample.Frequency.Value = 0.5 + (totalHoldTime / ((IHasDuration)HitObject).Duration);
            }

            isHitting.Value = Time.Current >= HitObject.StartTime
                              && Time.Current <= HitObject.GetEndTime()
                              && (Auto || checkForTouchInput() || ((SentakkiActionInputManager?.PressedActions.Any() ?? false) && IsHovered));
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (Time.Current < ((IHasDuration)HitObject).EndTime) return;

            double result = totalHoldTime / ((IHasDuration)HitObject).Duration;

            HitResult resultType;

            if (result >= .75)
                resultType = HitResult.Great;
            else if (result >= .5)
                resultType = HitResult.Good;
            else if (result >= .25)
                resultType = HitResult.Ok;
            else
                resultType = HitResult.Miss;

            AccentColour.Value = colours.ForHitResult(resultType);
            ApplyResult(resultType);
        }

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            base.UpdateHitStateTransforms(state);
            const double time_fade_hit = 100, time_fade_miss = 400;

            switch (state)
            {
                case ArmedState.Hit:
                    this.Delay(time_fade_hit).Expire();
                    break;

                case ArmedState.Miss:
                    this.ScaleTo(.0f, time_fade_miss).FadeOut(time_fade_miss).Expire();
                    break;
            }
        }

        private bool checkForTouchInput()
        {
            var touchInput = SentakkiActionInputManager.CurrentState.Touch;

            // Avoiding Linq to minimize allocations, since this would be called every update of this node
            for (TouchSource t = TouchSource.Touch1; t <= TouchSource.Touch10; ++t)
            {
                if (touchInput.GetTouchPosition(t) is Vector2 touchPosition && ReceivePositionalInputAt(touchPosition))
                    return true;
            }

            return false;
        }
    }
}
