namespace TableTennis.Models
{
    /// <summary>
    /// Метод расчёта рейтинга спортсменов.
    /// </summary>
    public interface IRatingMethod
    {
        /// <summary>
        /// Возвращает изменение рейтинга спортсмена за матч.
        /// </summary>
        /// <param name="gameResult">Результаты матча.</param>
        /// <param name="firstContestantRating">Рейтинг первого спортсмена до матча.</param>
        /// <param name="secondContestantRating">Рейтинг второго спортсмена до матча.</param>
        RatingDeltaForGameResult CalculateRatingDelta(
            GameResult gameResult, 
            int firstContestantRating, 
            int secondContestantRating);
        
        /// <summary>
        /// Начальный рейтинг спорстменов.
        /// </summary>
        int InitialRating { get; }
        
        /// <summary>
        /// Наибольший рейтинг, который можно достичь.
        /// </summary>
        int MaxRating { get; }
        
        /// <summary>
        /// Наименьший рейтинг, до которого можно упасть.
        /// </summary>
        int MinRating { get; }
        
        string Name { get; }
    }
}