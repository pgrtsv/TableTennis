// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Reactive.Linq;
// using DynamicData;
// using ReactiveUI;
//
// namespace TableTennis.Models
// {
//     public sealed class MonthlyRating : IRatingSystem
//     {
//         private readonly GamesDb _gamesDb;
//
//         private readonly SourceCache<RatingForContestant, Guid> _ratingsForContestants =
//             new(x => x.ContestantGuid);
//
//         private readonly SourceCache<GameResultStatistics, Guid> _gameResultsStatistics = new(x => x.GameResultGuid);
//
//         public MonthlyRating(GamesDb gamesDb)
//         {
//             _gamesDb = gamesDb;
//             _gamesDb.ContestantsDbConnect()
//                 .Where(db => db != null)
//                 .Subscribe(_ => CalculateScores());
//             _gamesDb.ContestantsDbConnect()
//                 .Where(db => db != null)
//                 .Select(db => db!.ContestantsPreview().WhereReasonsAre(ChangeReason.Add))
//                 .Switch()
//                 .Subscribe(set =>
//                 {
//                     foreach (var newContestant in set.Select(change => change.Current))
//                         CalculateAddedContestantScore(newContestant.Guid);
//                 });
//             _gamesDb.ContestantsDbConnect()
//                 .Where(db => db != null)
//                 .Select(db => db!.ContestantsPreview().WhereReasonsAre(ChangeReason.Remove))
//                 .Switch()
//                 .Subscribe(set =>
//                 {
//                     foreach (var oldContestant in set.Select(change => change.Current))
//                         _ratingsForContestants.Remove(oldContestant.Guid);
//                 });
//
//             _gamesDb.GamesResultsPreview()
//                 .WhereReasonsAre(ChangeReason.Add)
//                 .Subscribe(set =>
//                 {
//                     foreach (var newGameResult in set.Select(change => change.Current))
//                         CalculateAddedGameResult(newGameResult);
//                 });
//
//             _gamesDb.GamesResultsConnect()
//                 .Subscribe(set =>
//                 {
//                     foreach (var oldGameResult in set.Select(change => change.Previous)
//                         .Where(x => x.HasValue)
//                         .Select(x => x.Value))
//                         _gameResultsStatistics.Remove(oldGameResult.Guid);
//                     foreach (var newGameResult in set.Select(change => change.Current).OrderBy(x => x.DateTime))
//                     {
//                         var lastGameBefore = gamesDb.GamesResults.Where(x =>
//                                 x.DidContestantTakePart(newGameResult.FirstContestantScore.ContestantGuid) &&
//                                 x.DidContestantTakePart(newGameResult.SecondContestantScore.ContestantGuid) &&
//                                 x.DateTime < newGameResult.DateTime)
//                             .OrderByDescending(x => x.DateTime)
//                             .FirstOrDefault();
//                         if (lastGameBefore == null)
//                         {
//                             _gameResultsStatistics.AddOrUpdate(new GameResultStatistics(
//                                 newGameResult.Guid,
//                                 0,
//                                 0,
//                                 0,
//                                 0,
//                                 0));
//                             continue;
//                         }
//
//                         var lastGameStatisticsBefore = _gameResultsStatistics.Lookup(lastGameBefore.Guid)
//                             .Value;
//                         _gameResultsStatistics.AddOrUpdate(new GameResultStatistics(
//                             newGameResult.Guid,
//                             lastGameStatisticsBefore.ContestantsGamesCount + 1,
//                             newGameResult.IsFirstContestantWinner()
//                                 ? lastGameStatisticsBefore.FirstContestantWins + 1
//                                 : lastGameStatisticsBefore.FirstContestantWins,
//                             newGameResult.IsFirstContestantWinner()
//                                 ? lastGameStatisticsBefore.SecondContestantWins
//                                 : lastGameStatisticsBefore.SecondContestantWins + 1,
//                             lastGameStatisticsBefore.FirstContestantScore + newGameResult.FirstContestantScore.Score,
//                             lastGameStatisticsBefore.SecondContestantScore +
//                             newGameResult.SecondContestantScore.Score));
//                     }
//                 });
//         }
//
//         public IEnumerable<RatingForContestant> RatingsForContestants => _ratingsForContestants.Items;
//
//         public IEnumerable<GameResultStatistics> GameResultsStatistics => _gameResultsStatistics.Items;
//
//         public GameResultStatistics GetStatisticsForGameResult(Guid gameResultGuid) =>
//             _gameResultsStatistics.Lookup(gameResultGuid).Value;
//
//         public IObservable<IChangeSet<RatingForContestant, Guid>> RatingsForContestantsConnect() =>
//             _ratingsForContestants.Connect();
//
//         private readonly SourceCache<RatingDeltaForGameResult, Guid> _ratingsForGameResults =
//             new(x => x.GameResultGuid);
//
//         public IEnumerable<RatingDeltaForGameResult> RatingsForGameResults => _ratingsForGameResults.Items;
//
//
//         public const int MaxRating = 100;
//
//         public void CalculateScores()
//         {
//             _ratingsForGameResults.Edit(scoreDeltas =>
//                 _ratingsForContestants.Edit(scores =>
//                 {
//                     scores.Clear();
//                     scoreDeltas.Clear();
//                     scores.AddOrUpdate(
//                         _gamesDb.GetContestantsDb()!.Contestants.Select(x => new RatingForContestant(x.Guid, 0)));
//                     foreach (var gameResult in _gamesDb.GamesResults.OrderBy(x => x.DateTime))
//                         CalculateAddedGameResult(gameResult, () => scores.Items, scoreDeltas.AddOrUpdate);
//                 }));
//         }
//
//         public void CalculateAddedContestantScore(Guid contestantGuid) =>
//             _ratingsForContestants.AddOrUpdate(new RatingForContestant(contestantGuid, 0));
//
//         private static int GetBasePositiveDelta(int rating) =>
//             rating switch
//             {
//                 < 20 => 10,
//                 < 30 => 8,
//                 < 50 => 6,
//                 < 70 => 5,
//                 < 90 => 3,
//                 < 100 => 1,
//                 _ => 0
//             };
//
//         private static int GetBaseNegativeDelta(int rating) =>
//             rating switch
//             {
//                 < 1 => 0,
//                 < 20 => -5,
//                 < 30 => -5,
//                 < 50 => -5,
//                 < 70 => -5,
//                 < 90 => -3,
//                 < 100 => -2,
//                 _ => -2
//             };
//
//         private static int GetDelta(int firstContestantRating, int secondContestantRating, bool isFirstContestantWinner)
//         {
//             int delta;
//             if (isFirstContestantWinner)
//             {
//                 var basePositiveDelta = GetBasePositiveDelta(firstContestantRating);
//                 delta = (int) Math.Round(
//                     basePositiveDelta * (1 + (secondContestantRating - firstContestantRating) * 2.0 / MaxRating),
//                     0);
//             }
//             else
//             {
//                 var baseNegativeDelta = GetBaseNegativeDelta(firstContestantRating);
//                 delta = (int) Math.Round(
//                     baseNegativeDelta * (1 + (firstContestantRating - secondContestantRating) * 2.5 / MaxRating),
//                     0);
//             }
//
//             return (firstContestantRating + delta) switch
//             {
//                 < 0 => -firstContestantRating,
//                 > MaxRating => MaxRating - firstContestantRating,
//                 _ => delta
//             };
//         }
//
//         public void CalculateAddedGameResult(GameResult gameResult,
//             Func<IEnumerable<RatingForContestant>>? scores = null,
//             Action<RatingDeltaForGameResult>? addScore = null,
//             Action<RatingForContestant>? addOrUpdateRatingForContestant = null)
//         {
//             scores ??= () => RatingsForContestants;
//             addScore ??= score => _ratingsForGameResults.AddOrUpdate(score);
//             addOrUpdateRatingForContestant ??= rating => _ratingsForContestants.AddOrUpdate(rating);
//
//             var firstContestantGuid = gameResult.FirstContestantScore.ContestantGuid;
//             var secondContestantGuid = gameResult.SecondContestantScore.ContestantGuid;
//             var firstContestantRating = scores.Invoke().First(x => x.ContestantGuid == firstContestantGuid).Score;
//             var secondContestantRating = scores.Invoke().First(x => x.ContestantGuid == secondContestantGuid).Score;
//             var firstContestantDelta = GetDelta(firstContestantRating, secondContestantRating,
//                 gameResult.IsFirstContestantWinner());
//             var secondContestantDelta = GetDelta(secondContestantRating, firstContestantRating,
//                 !gameResult.IsFirstContestantWinner());
//
//             addScore.Invoke(new RatingDeltaForGameResult(
//                 gameResult.Guid,
//                 firstContestantDelta,
//                 secondContestantDelta,
//                 firstContestantRating,
//                 secondContestantRating));
//             addOrUpdateRatingForContestant.Invoke(new RatingForContestant(
//                 firstContestantGuid,
//                 firstContestantRating + firstContestantDelta));
//             addOrUpdateRatingForContestant.Invoke(new RatingForContestant(
//                 secondContestantGuid,
//                 secondContestantRating + secondContestantDelta));
//             // scores.Invoke().First(x => x.ContestantGuid == firstContestantGuid).UpdateScore(firstContestantDelta);
//             // scores.Invoke().First(x => x.ContestantGuid == secondContestantGuid).UpdateScore(secondContestantDelta);
//         }
//     }
// }