using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Sentakki.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Sentakki.Objects
{
    // Used to increase the weighting of an object
    public class ScorePaddingObject : HitObject
    {
        public override Judgement CreateJudgement() => new SentakkiJudgement();

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;
    }
}
