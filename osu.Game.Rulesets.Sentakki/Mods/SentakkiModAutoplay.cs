﻿using osu.Game.Beatmaps;
using osu.Game.Rulesets.Sentakki.Objects;
using osu.Game.Rulesets.Sentakki.Replays;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Game.Users;
using osu.Game.Rulesets.Sentakki.Objects.Drawables;
using osu.Game.Rulesets.Objects.Drawables;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Utils;

namespace osu.Game.Rulesets.Sentakki.Mods
{
    public class SentakkiModAutoplay : ModAutoplay<SentakkiHitObject>, IApplicableToDrawableHitObjects
    {

        private string getRandomCharacter()
        {
            string[] characters = {
                "Mai-chan",
                "Sen-kun"
            };

            return characters[RNG.Next(0, characters.Length)];
        }
        public override Score CreateReplayScore(IBeatmap beatmap)
        {

            return new Score
            {
                ScoreInfo = new ScoreInfo
                {
                    User = new User { Username = getRandomCharacter() },
                },
                Replay = new SentakkiAutoGenerator(beatmap).Generate(),
            };
        }

        public void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            foreach (var d in drawables.OfType<DrawableSentakkiHitObject>())
            {
                d.Auto = true;
                foreach (DrawableSentakkiHitObject nested in d.NestedHitObjects)
                    nested.Auto = true;
            }
        }
    }
}
