using System;
using System.Collections.Generic;
using System.Linq;

namespace TableTennis.Models
{
    public sealed class BasicRatingMethod : IRatingMethod
    {
        public RatingDeltaForGameResult CalculateRatingDelta(
            GameResult gameResult,
            int firstContestantRating,
            int secondContestantRating)
        {
            var firstContestantDelta = GetDelta(
                firstContestantRating, 
                secondContestantRating,
                gameResult.FirstContestantScore.Score,
                gameResult.SecondContestantScore.Score);
            var secondContestantDelta = GetDelta(
                secondContestantRating, 
                firstContestantRating,
                gameResult.SecondContestantScore.Score,
                gameResult.FirstContestantScore.Score);
            return new RatingDeltaForGameResult(
                gameResult.Guid,
                firstContestantDelta,
                secondContestantDelta,
                firstContestantRating,
                secondContestantRating);
        }

        public int InitialRating => 0;
        public int MaxRating => 100;
        public int MinRating => 0;

        public string Name => "Простой (0-100)";

        private int GetBasePositiveDelta(int rating) =>
            rating switch
            {
                < 20 => 10,
                < 30 => 8,
                < 50 => 6,
                < 70 => 5,
                < 90 => 3,
                < 100 => 1,
                _ => 0
            };

        private int GetBaseNegativeDelta(int rating) =>
            rating switch
            {
                < 1 => 0,
                < 20 => -5,
                < 30 => -5,
                < 50 => -5,
                < 70 => -5,
                < 90 => -3,
                < 100 => -2,
                _ => -2
            };

        public double MinPositiveModifierForScoreDifference => 1.0;
        public double MaxPositiveModifierForScoreDifference => 1.2;
        public double MinNegativeModifierForScoreDifference => 1.0;
        public double MaxNegativeModifierForScoreDifference => 1.4;

        /// <summary>
        /// Возвращает модификатор счёта от <see cref="MinPositiveModifierForScoreDifference"/> до
        /// <see cref="MaxPositiveModifierForScoreDifference"/> для победившего игрока.
        /// </summary>
        /// <param name="loserScore">Счёт проигравшего игрока.</param>
        private double GetPositiveScoreModifier(int loserScore) =>
            ((11 - loserScore) *
             (MaxPositiveModifierForScoreDifference - MinPositiveModifierForScoreDifference) +
             11 * MinPositiveModifierForScoreDifference -
             MaxPositiveModifierForScoreDifference)
            / 10;

        /// <summary>
        /// Возвращает модификатор счёта от <see cref="MinNegativeModifierForScoreDifference"/> до
        /// <see cref="MaxNegativeModifierForScoreDifference"/> для проигравшего игрока.
        /// </summary>
        /// <param name="loserScore">Счёт проигравшего игрока.</param>
        private double GetNegativeScoreModifier(int loserScore) =>
            ((11 - loserScore) *
             (MaxNegativeModifierForScoreDifference - MinNegativeModifierForScoreDifference) +
             11 * MinNegativeModifierForScoreDifference -
             MaxNegativeModifierForScoreDifference)
            / 10;

        /// <summary>
        /// Возвращает дельту - изменение рейтинга для спортсмена, участвовавшего в матче.
        /// </summary>
        /// <param name="targetContestantRating">Рейтинг спортсмена на момент участия в матче.</param>
        /// <param name="opponentRating">Рейтинг оппонента на момент участия в матче.</param>
        /// <param name="targetContestantScore">Счёт спортсмена в конце матча.</param>
        /// <param name="opponentScore">Счёт оппонента в конце матча.</param>
        private int GetDelta(
            int targetContestantRating,
            int opponentRating,
            int targetContestantScore,
            int opponentScore)
        {
            int delta;
            var isFirstContestantWinner = targetContestantScore == 11;
            if (isFirstContestantWinner)
            {
                var basePositiveDelta = GetBasePositiveDelta(targetContestantRating);
                var positiveScoreModifier = GetPositiveScoreModifier(opponentScore);
                var positiveRatingModifier = 1 + (opponentRating - targetContestantRating) * 2.0 / MaxRating;
                if (positiveRatingModifier < 0) positiveRatingModifier = 0;
                delta = (int) Math.Round(basePositiveDelta * positiveScoreModifier * positiveRatingModifier, 0);
            }
            else
            {
                var baseNegativeDelta = GetBaseNegativeDelta(targetContestantRating);
                var negativeScoreModifier = GetNegativeScoreModifier(targetContestantScore);
                var negativeRatingModifier = 1 + (targetContestantRating - opponentRating) * 2.5 / MaxRating;
                if (negativeRatingModifier < 0) negativeRatingModifier = 0;
                delta = (int) Math.Round(baseNegativeDelta * negativeScoreModifier * negativeRatingModifier, 0);
            }

            if (targetContestantRating + delta < MinRating)
                return -targetContestantRating;
            if (targetContestantRating + delta > MaxRating)
                return MaxRating - targetContestantRating;
            return delta;
        }
    }
}