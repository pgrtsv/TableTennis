using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text.Json.Serialization;
using DynamicData;
using JetBrains.Annotations;

namespace TableTennis.Models
{
    /// <summary>
    /// БД спортсменов.
    /// </summary>
    public sealed class ContestantsDb
    {
        private readonly SourceCache<Contestant, Guid> _contestants = new(contestant => contestant.Guid);

        public IEnumerable<Contestant> Contestants => _contestants.Items;

        public IObservable<IChangeSet<Contestant, Guid>> ContestantsConnect() => _contestants.Connect();

        public IObservable<IChangeSet<Contestant, Guid>> ContestantsPreview() => _contestants.Preview();

        /// <summary>
        /// Создаёт новую БД спортсменов.
        /// </summary>
        public ContestantsDb()
        {
        }

        /// <summary>
        /// Только для сериализаторов!
        /// </summary>
        [JsonConstructor, UsedImplicitly]
        public ContestantsDb(IEnumerable<Contestant> contestants)
        {
            _contestants.AddOrUpdate(contestants);
        }

        /// <summary>
        /// Добавляет в БД запись о спортсмене.
        /// </summary>
        /// <param name="contestant"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void AddContestant(Contestant contestant)
        {
            if (contestant == null)
                throw new ArgumentNullException(nameof(contestant));
            if (_contestants.Keys.Contains(contestant.Guid))
                throw new ArgumentException(string.Empty, nameof(contestant));
            if (_contestants.Items.Any(existingContestant => Equals(existingContestant.Name, contestant.Name)))
                throw new ArgumentException(string.Empty, nameof(contestant));
            _contestants.AddOrUpdate(contestant);
        }

        public void RemoveContestant(Contestant contestant, GamesDb gamesDb)
        {
            if (!CanRemoveContestant(contestant, gamesDb))
                throw new ArgumentException(string.Empty, nameof(contestant));

            _contestants.Remove(contestant);
        }

        public bool CanRemoveContestant(Contestant contestant, GamesDb gamesDb)
        {
            if (contestant == null)
                throw new ArgumentNullException(nameof(contestant));
            if (!_contestants.Lookup(contestant.Guid).HasValue)
                throw new ArgumentException(string.Empty, nameof(contestant));
            if (gamesDb == null)
                throw new ArgumentNullException(nameof(gamesDb));
            return !gamesDb.GamesResults.Any(gameResult => gameResult.DidContestantTakePart(contestant.Guid));
        }

        public IObservable<bool> CanRemoveContestantConnect(Contestant contestant, IObservable<GamesDb> gamesDb)
        {
            if (contestant == null)
                throw new ArgumentNullException(nameof(contestant));
            if (!_contestants.Lookup(contestant.Guid).HasValue)
                throw new ArgumentException(string.Empty, nameof(contestant));

            return gamesDb.Select(db =>
                    db.GamesResultsConnect().Select(_ => CanRemoveContestant(contestant, db)))
                .Switch();
        }
    }
}