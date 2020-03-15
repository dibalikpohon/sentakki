﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using System.Diagnostics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Audio;
using osu.Game.Beatmaps.ControlPoints;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Framework.Input.Bindings;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Maimai.Objects.Drawables.Pieces;
using osu.Game.Rulesets.Maimai.Configuration;
using osu.Game.Rulesets.Maimai.UI;
using osu.Game.Rulesets.Scoring;
using System;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Input.Bindings;
using System.Linq;

namespace osu.Game.Rulesets.Maimai.Objects.Drawables
{
    public class DrawableMaimaiHitObject : DrawableHitObject<MaimaiHitObject>
    {
        public readonly HitReceptor HitArea;
        public readonly MainCirclePiece CirclePiece;
        public readonly CircularProgress hitObjectLine;

        public Func<DrawableMaimaiHitObject, bool> CheckValidation;

        private Bindable<Color4> accentColor;
        double fadeIn = 500, moveTo, idle;

        /// <summary>
        /// The action that caused this <see cref="DrawableHit"/> to be hit.
        /// </summary>
        public MaimaiAction? HitAction => HitArea.HitAction;

        private bool validActionPressed;

        /// <summary>
        /// A list of keys which can result in hits for this HitObject.
        /// </summary>
        public MaimaiAction[] HitActions { get; set; } = new[]
        {
            MaimaiAction.Button1,
            MaimaiAction.Button2,
        };
        protected override double InitialLifetimeOffset => 3500;

        public DrawableMaimaiHitObject(MaimaiHitObject hitObject)
            : base(hitObject)
        {
            accentColor = new Bindable<Color4>(hitObject.NoteColor);
            AccentColour.BindTo(accentColor);
            RelativeSizeAxes = Axes.Both;
            CornerRadius = 120;
            CornerExponent = 2;
            Size = Vector2.Zero;
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            AddRangeInternal(new Drawable[] {
                hitObjectLine = new CircularProgress
                {
                    Size = new Vector2(MaimaiPlayfield.noteStartDistance*2),
                    RelativePositionAxes = Axes.None,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = HitObject.NoteColor,
                    InnerRadius = .025f,
                    RelativeSizeAxes = Axes.None,
                    Rotation =  -45 +HitObject.Angle,
                    Current = new Bindable<double>(0.25),
                    Alpha = 0f,
                },
                CirclePiece = new MainCirclePiece()
                {

                    Scale = new Vector2(0f),
                    Rotation = hitObject.Angle,
                    Position = HitObject.Position
                },
                HitArea = new HitReceptor()
                {
                    Hit = () =>
                    {
                        if (AllJudged)
                            return false;

                        UpdateResult(true);
                        return true;
                    },
                    RelativeSizeAxes = Axes.None,
                    Position = hitObject.endPosition
                },

            });

        }
        [BackgroundDependencyLoader(true)]
        private void load(MaimaiRulesetConfigManager settings)
        {
            Bindable<double> AnimationDuration = new Bindable<double>(1000);
            settings?.BindWith(MaimaiRulesetSettings.AnimationDuration, AnimationDuration);
            AnimationDuration.TriggerChange();
            fadeIn = 500;
            moveTo = AnimationDuration.Value;
            idle = 3500 - fadeIn - moveTo;
        }

        protected override void UpdateInitialTransforms()
        {
            base.UpdateInitialTransforms();
            CirclePiece.ScaleTo(0f, idle).Then().FadeInFromZero(fadeIn).ScaleTo(1f, fadeIn).Then().MoveTo(HitObject.endPosition, moveTo);
            hitObjectLine.FadeTo(0, idle).Then(h => h.FadeTo(.75f, fadeIn).Then(h => h.ResizeTo(600, moveTo)));
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            Debug.Assert(HitObject.HitWindows != null);

            if (!userTriggered)
            {
                if (!HitObject.HitWindows.CanBeHit(timeOffset))
                    ApplyResult(r => r.Type = HitResult.Miss);

                return;
            }

            var result = HitObject.HitWindows.ResultFor(timeOffset);
            if (result == HitResult.None)
            {
                return;
            }

            ApplyResult(r => r.Type = result);
        }

        protected override void UpdateStateTransforms(ArmedState state)
        {
            base.UpdateStateTransforms(state);

            const double time_fade_hit = 400, time_fade_miss = 400;

            switch (state)
            {
                case ArmedState.Hit:
                    var b = HitObject.Angle + 90;
                    var a = b * (float)(Math.PI / 180);

                    //MaimaiNote.ScaleTo(2f, time_fade_hit, Easing.OutCubic)
                    //   .FadeColour(Color4.Yellow, time_fade_hit, Easing.OutCubic)
                    //   .MoveToOffset(new Vector2(-(500 * (float)Math.Cos(a)), -(500 * (float)Math.Sin(a))), time_fade_hit, Easing.OutCubic)
                    //   .FadeOut(time_fade_hit);

                    //MaimaiNote.FadeOut(time_fade_hit);
                    hitObjectLine.FadeOut();
                    this.ScaleTo(1f, time_fade_hit).Expire();

                    break;

                case ArmedState.Miss:
                    var c = HitObject.Angle + 90;
                    var d = c * (float)(Math.PI / 180);

                    CirclePiece.ScaleTo(0.5f, time_fade_miss, Easing.InCubic)
                       .FadeColour(Color4.Red, time_fade_miss, Easing.OutQuint)
                       .MoveToOffset(new Vector2(-(100 * (float)Math.Cos(d)), -(100 * (float)Math.Sin(d))), time_fade_hit, Easing.OutCubic)
                       .FadeOut(time_fade_miss);

                    CirclePiece.FadeOut(time_fade_miss);
                    hitObjectLine.FadeOut();
                    this.ScaleTo(1f, time_fade_miss).Expire();

                    break;
            }
        }
        public class HitReceptor : CircularContainer, IKeyBindingHandler<MaimaiAction>
        {
            // IsHovered is used
            public override bool HandlePositionalInput => true;

            public Func<bool> Hit;

            public MaimaiAction? HitAction;
            public HitReceptor()
            {
                RelativeSizeAxes = Axes.None;
                Size = new Vector2(350f);
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
            }

            public bool OnPressed(MaimaiAction action)
            {
                switch (action)
                {
                    case MaimaiAction.Button1:
                    case MaimaiAction.Button2:
                        if (IsHovered && (Hit?.Invoke() ?? false))
                        {
                            HitAction = action;
                            return true;
                        }

                        break;
                }

                return false;
            }
            public void OnReleased(MaimaiAction action)
            {
            }
        }

    }
}
