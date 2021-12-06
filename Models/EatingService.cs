using System;

namespace TableTennis.Models
{
    public static class EatingService
    {
        /// <summary>
        /// Количество рот.
        /// </summary>
        public const int CompaniesCount = 8;

        /// <summary>
        /// Первый день, когда рота ходила последней.
        /// </summary>
        public static readonly DateTime StartDay = new(2021, 3, 22);


        /// <summary>
        /// Возвращает позицию роты в графике на указанный день.
        /// </summary>
        public static int GetCompanyPosition(DateTime day)
        {
            var daysPassed = (day - StartDay).Days;
            var weeksPassed = daysPassed / 7;
            var fullCircles = weeksPassed / 8;
            weeksPassed -= fullCircles * 8;
            return CompaniesCount - weeksPassed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startTime">Начало приёма пищи для рот.</param>
        /// <param name="breakTime">Время перерыва между 5 и 6 ротами, принимающими пищу.</param>
        /// <param name="day">День, на который нужно вернуть время.</param>
        private static DateTime GetTime(TimeSpan startTime, TimeSpan breakTime, DateTime day)
        {
            var position = GetCompanyPosition(day);
            var time = startTime.Add((position - 1) * TimeSpan.FromMinutes(5));
            if (position > 5) time = time.Add(breakTime);
            return DateTime.Today.Add(time);
        }

        public static DateTime GetBreakfastTime(DateTime day) => GetTime(
            TimeSpan.FromHours(7), 
            TimeSpan.FromMinutes(30),
            day);

        public static DateTime GetDinnerTime(DateTime day) => GetTime(
            TimeSpan.FromHours(13), 
            TimeSpan.FromMinutes(35),
            day);

        public static DateTime GetSupperTime(DateTime day) => GetTime(
                TimeSpan.FromHours(18).Add(TimeSpan.FromMinutes(45)), 
                TimeSpan.FromMinutes(30), 
                day);
    }
}