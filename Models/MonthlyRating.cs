using System;
using System.Collections.Generic;
using System.Linq;
using DynamicData;

namespace TableTennis.Models
{
    public sealed class MonthlyRating
    {
        private readonly GamesDb _gamesDb;

        private readonly SourceCache<MonthlyRatingForContestant, Guid> _ratingsForContestants =
            new(x => x.ContestantGuid);

        public MonthlyRating(GamesDb gamesDb)
        {
            _gamesDb = gamesDb;
            CalculateScores();
            _gamesDb.ContestantsPreview()
                .WhereReasonsAre(ChangeReason.Add)
                .Subscribe(set =>
                {
                    foreach (var newContestant in set.Select(x => x.Current)) CalculateAddedContestant(newContestant);
                });
            _gamesDb.GamesResultsPreview()
                .WhereReasonsAre(ChangeReason.Add)
                .Subscribe(set =>
                {
                    foreach (var newGameResult in set.Select(x => x.Current)) CalculateAddedGameResult(newGameResult);
                });
        }

        public IEnumerable<MonthlyRatingForContestant> RatingsForContestants => _ratingsForContestants.Items;

        public IObservable<IChangeSet<MonthlyRatingForContestant, Guid>> RatingsForContestantsConnect() =>
            _ratingsForContestants.Connect();

        private readonly SourceCache<MonthlyRatingForGameResult, Guid> _ratingsForGameResults =
            new(x => x.GameResultGuid);

        public IEnumerable<MonthlyRatingForGameResult> RatingsForGameResults => _ratingsForGameResults.Items;


        public const int MaxRating = 100;
        public const double AlphaCoeff = 0.01;

        public void CalculateScores()
        {
            _ratingsForGameResults.Edit(scoreDeltas =>
                _ratingsForContestants.Edit(scores =>
                {
                    scores.Clear();
                    scoreDeltas.Clear();
                    scores.AddOrUpdate(_gamesDb.Contestants.Select(x => new MonthlyRatingForContestant(x.Guid)));
                    foreach (var gameResult in _gamesDb.GamesResults.OrderBy(x => x.DateTime))
                        CalculateAddedGameResult(gameResult, () => scores.Items, scoreDeltas.AddOrUpdate);
                }));
        }

        public void CalculateAddedContestant(Contestant contestant) =>
            _ratingsForContestants.AddOrUpdate(new MonthlyRatingForContestant(contestant.Guid));

        private static int GetBasePositiveDelta(int rating) =>
            rating switch
            {
                < 20 => 10,
                < 30 => 8,
                < 50 => 6,
                < 70 => 4,
                < 90 => 2,
                < 100 => 1,
                _ => 0
            };
        
        private static int GetBaseNegativeDelta(int rating) =>
            rating switch
            {
                < 1 => 0,
                < 20 => -6,
                < 30 => -6,
                < 50 => -6,
                < 70 => -6,
                < 90 => -4,
                < 100 => -2,
                _ => -2
            };

        private static int GetDelta(int firstContestantRating, int secondContestantRating, bool isFirstContestantWinner)
        {
            int delta;
            if (isFirstContestantWinner)
            {
                var basePositiveDelta = GetBasePositiveDelta(firstContestantRating);
                delta = (int) Math.Round(
                    basePositiveDelta * (1 + (secondContestantRating - firstContestantRating) * 1.0 / MaxRating),
                    0);
            }
            else
            {
                var baseNegativeDelta = GetBaseNegativeDelta(firstContestantRating);
                delta = (int) Math.Round(
                    baseNegativeDelta * (1 + (firstContestantRating - secondContestantRating) * 2.0 / MaxRating),
                    0);
            }

            return (firstContestantRating + delta) switch
            {
                < 0 => -firstContestantRating,
                > MaxRating => MaxRating - firstContestantRating,
                _ => delta
            };
        }

        public void CalculateAddedGameResult(GameResult gameResult,
            Func<IEnumerable<MonthlyRatingForContestant>>? scores = null,
            Action<MonthlyRatingForGameResult>? addScore = null)
        {
            scores ??= () => RatingsForContestants;
            addScore ??= score => _ratingsForGameResults.AddOrUpdate(score);

            var firstContestantGuid = gameResult.FirstContestantResult.ContestantGuid;
            var secondContestantGuid = gameResult.SecondContestantResult.ContestantGuid;
            var firstContestantRating = scores.Invoke().First(x => x.ContestantGuid == firstContestantGuid).Score;
            var secondContestantRating = scores.Invoke().First(x => x.ContestantGuid == secondContestantGuid).Score;
            var firstContestantDelta = GetDelta(firstContestantRating, secondContestantRating,
                gameResult.IsFirstContestantWinner());
            var secondContestantDelta = GetDelta(secondContestantRating, firstContestantRating,
                !gameResult.IsFirstContestantWinner());
            
            addScore.Invoke(new MonthlyRatingForGameResult(
                gameResult.Guid,
                firstContestantDelta,
                secondContestantDelta,
                firstContestantRating,
                secondContestantRating));
            scores.Invoke().First(x => x.ContestantGuid == firstContestantGuid).UpdateScore(firstContestantDelta);
            scores.Invoke().First(x => x.ContestantGuid == secondContestantGuid).UpdateScore(secondContestantDelta);
        }
    }
}