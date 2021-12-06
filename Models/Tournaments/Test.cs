using System;
using Console = System.Console;

namespace TableTennis.Models.Tournaments
{
    public class Test
    {
        public Test()
        {
            var tournament = new Tournament();
            tournament.StateConnect().Subscribe(state => Console.WriteLine(state.Switch(
                created => "Турнир создан.",
                started => started.Switch(
                    firstWave => "Первая волна.",
                    secondWave => "Вторая волна.",
                    semiFinal => "Полуфинал.",
                    final => "Финал."),
                finished => "Турнир окончен.",
                cancelled => "Турнир отменён."
            )));
            tournament.StartTournament();
        }
    }
}