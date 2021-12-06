using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Aggregation;

namespace TableTennis.Models
{
    /// <summary>
    /// Система рейтинга спортсменов.
    /// </summary>
    public sealed class RatingSystem : IDisposable
    {
        private readonly IRatingMethod _ratingMethod;

        public GamesDb GamesDb { get; }
        
        public ContestantsDb ContestantsDb { get; }

        private readonly SourceCache<RatingForContestant, Guid> _ratingsForContestants =
            new(x => x.ContestantGuid);

        private readonly SourceCache<GameResultStatistics, Guid> _gameResultsStatistics = new(x => x.GameResultGuid);

        private readonly SourceCache<RatingDeltaForGameResult, Guid> _ratingDeltasForGameResults =
            new(x => x.GameResultGuid);

        private readonly CompositeDisposable _cleanUp;

        public RatingSystem(IRatingMethod ratingMethod, GamesDb gamesDb, ContestantsDb contestantsDb)
        {
            _ratingMethod = ratingMethod;
            GamesDb = gamesDb;
            ContestantsDb = contestantsDb;
            _cleanUp = new CompositeDisposable(
                _ratingsForContestants,
                _gameResultsStatistics,
                _ratingDeltasForGameResults);

            var orderedGameResults = gamesDb.GamesResults.OrderBy(x => x.DateTime).ToArray();

            for (var i = 0; i < orderedGameResults.Length; i++)
            {
                var gameResult = orderedGameResults[i];

                var contestantsGamesCount = 0;
                var firstContestantWins = 0;
                var secondContestantWins = 0;
                var firstContestantScore = 0;
                var secondContestantScore = 0;

                for (var j = 0; j < i; j++)
                {
                    var previousGameResult = orderedGameResults[j];
                    if (!previousGameResult.DidContestantTakePart(gameResult.FirstContestantScore.ContestantGuid) ||
                        !previousGameResult.DidContestantTakePart(gameResult.SecondContestantScore.ContestantGuid))
                        continue;
                    contestantsGamesCount += 1;
                    if (previousGameResult.IsFirstContestantWinner())
                        firstContestantWins += 1;
                    else
                        secondContestantWins += 1;
                    firstContestantScore += previousGameResult.FirstContestantScore.Score;
                    secondContestantScore += previousGameResult.SecondContestantScore.Score;
                }
                _gameResultsStatistics.AddOrUpdate(new GameResultStatistics(
                    gameResult.Guid,
                    contestantsGamesCount,
                    firstContestantWins,
                    secondContestantWins,
                    firstContestantScore,
                    secondContestantScore));

                var firstContestantFoundRating =
                    _ratingsForContestants.Lookup(gameResult.FirstContestantScore.ContestantGuid);
                var secondContestantFoundRating =
                    _ratingsForContestants.Lookup(gameResult.SecondContestantScore.ContestantGuid);
                var firstContestantRating = firstContestantFoundRating.HasValue
                    ? firstContestantFoundRating.Value.Score
                    : ratingMethod.InitialRating;
                var secondContestantRating = secondContestantFoundRating.HasValue
                    ? secondContestantFoundRating.Value.Score
                    : ratingMethod.InitialRating;
                
                var delta = ratingMethod.CalculateRatingDelta(gameResult, firstContestantRating, secondContestantRating);
                _ratingDeltasForGameResults.AddOrUpdate(delta);
                _ratingsForContestants.AddOrUpdate(new RatingForContestant(
                    gameResult.FirstContestantScore.ContestantGuid,
                    delta.FirstContestantInitialScore + delta.FirstContestantDelta));
                _ratingsForContestants.AddOrUpdate(new RatingForContestant(
                    gameResult.SecondContestantScore.ContestantGuid,
                    delta.SecondContestantInitialScore + delta.SecondContestantDelta));
            }

            foreach (var contestant in contestantsDb.Contestants)
                if (!_ratingsForContestants.Lookup(contestant.Guid).HasValue)
                    _ratingsForContestants.AddOrUpdate(new RatingForContestant(contestant.Guid,
                        ratingMethod.InitialRating));

            contestantsDb.ContestantsPreview()
                .WhereReasonsAre(ChangeReason.Add)
                .Subscribe(set =>
                {
                    foreach (var newContestant in set.Select(change => change.Current))
                        _ratingsForContestants.AddOrUpdate(new RatingForContestant(newContestant.Guid,
                            ratingMethod.InitialRating));
                });

            gamesDb.GamesResultsPreview()
                .WhereReasonsAre(ChangeReason.Add)
                .Subscribe(set =>
                {
                    foreach (var newGameResult in set.Select(change => change.Current))
                    {
                        var previousContestantsGameResults = GamesDb.GamesResults
                            .Where(gameResult =>
                                gameResult.DidContestantTakePart(newGameResult.FirstContestantScore.ContestantGuid) &&
                                gameResult.DidContestantTakePart(newGameResult.SecondContestantScore.ContestantGuid) &&
                                gameResult.DateTime < newGameResult.DateTime)
                            .ToArray();
                        var firstContestantWins = previousContestantsGameResults
                            .Count(previousContestantsGameResult =>
                                previousContestantsGameResult.GetWinnerGuid() ==
                                newGameResult.FirstContestantScore.ContestantGuid);
                        var secondContestantWins = previousContestantsGameResults
                            .Count(previousContestantsGameResult =>
                                previousContestantsGameResult.GetWinnerGuid() ==
                                newGameResult.SecondContestantScore.ContestantGuid);
                        var firstContestantScore = previousContestantsGameResults
                            .Sum(previousGameResult =>
                                previousGameResult.FirstContestantScore.ContestantGuid ==
                                newGameResult.FirstContestantScore.ContestantGuid
                                    ? previousGameResult.FirstContestantScore.Score
                                    : previousGameResult.SecondContestantScore.Score);
                        var secondContestantScore = previousContestantsGameResults
                            .Sum(previousGameResult =>
                                previousGameResult.FirstContestantScore.ContestantGuid ==
                                newGameResult.SecondContestantScore.ContestantGuid
                                    ? previousGameResult.FirstContestantScore.Score
                                    : previousGameResult.SecondContestantScore.Score);
                        _gameResultsStatistics.AddOrUpdate(new GameResultStatistics(
                            newGameResult.Guid,
                            previousContestantsGameResults.Length,
                            firstContestantWins,
                            secondContestantWins,
                            firstContestantScore,
                            secondContestantScore
                        ));
                        var firstContestantFoundRating =
                            _ratingsForContestants.Lookup(newGameResult.FirstContestantScore.ContestantGuid);
                        var secondContestantFoundRating =
                            _ratingsForContestants.Lookup(newGameResult.SecondContestantScore.ContestantGuid);
                        var firstContestantRating = firstContestantFoundRating.HasValue
                            ? firstContestantFoundRating.Value.Score
                            : ratingMethod.InitialRating;
                        var secondContestantRating = secondContestantFoundRating.HasValue
                            ? secondContestantFoundRating.Value.Score
                            : ratingMethod.InitialRating;
                        var delta = ratingMethod.CalculateRatingDelta(newGameResult, firstContestantRating, secondContestantRating);
                        _ratingDeltasForGameResults.AddOrUpdate(delta);
                        _ratingsForContestants.AddOrUpdate(new RatingForContestant(
                            newGameResult.FirstContestantScore.ContestantGuid,
                            delta.FirstContestantInitialScore + delta.FirstContestantDelta));
                        _ratingsForContestants.AddOrUpdate(new RatingForContestant(
                            newGameResult.SecondContestantScore.ContestantGuid,
                            delta.SecondContestantInitialScore + delta.SecondContestantDelta));
                    }
                })
                .DisposeWith(_cleanUp);
        }

        /// <summary>
        /// Возвращает перечисление - рейтинг каждого спортсмена.
        /// </summary>
        public IEnumerable<RatingForContestant> RatingsForContestants => _ratingsForContestants.Items;

        /// <summary>
        /// DynamicData-интерфейс для <see cref="RatingsForContestants"/>.
        /// </summary>
        public IObservable<IChangeSet<RatingForContestant, Guid>> RatingsForContestantsConnect() =>
            _ratingsForContestants.Connect();

        /// <summary>
        /// Возвращает перечисление - статистику по каждому сыгранному матчу.
        /// </summary>
        public IEnumerable<GameResultStatistics> GameResultsStatistics => _gameResultsStatistics.Items;

        public GameResultStatistics GetStatisticsForGameResult(Guid gameResultGuid)
        {
            var result = _gameResultsStatistics
                .Lookup(gameResultGuid);
            if (!result.HasValue) throw new KeyNotFoundException();
            return result.Value;
        }

        /// <summary>
        /// Возвращает перечисление - изменения в рейтинге спортсменов за каждый сыгранный матч.
        /// </summary>
        public IEnumerable<RatingDeltaForGameResult> RatingDeltasForGameResults => _ratingDeltasForGameResults.Items;

        public RatingDeltaForGameResult GetRatingDeltaForGameResult(Guid gameResultGuid)
        {
            var lookUp = _ratingDeltasForGameResults.Lookup(gameResultGuid);
            if (!lookUp.HasValue) throw new ArgumentException(string.Empty, nameof(gameResultGuid));
            return lookUp.Value;
        }

        public void Dispose() => _cleanUp.Dispose();
    }
}