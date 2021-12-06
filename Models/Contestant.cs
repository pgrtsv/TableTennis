using System;
using System.Reactive;
using System.Reactive.Subjects;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace TableTennis.Models
{
    /// <summary>
    /// Спортсмен.
    /// </summary>
    public sealed class Contestant
    {
        /// <summary>
        /// Guid спортсмена.
        /// </summary>
        public Guid Guid { get; }
        
        /// <summary>
        /// Воинское звание спорстмена.
        /// </summary>
        public MilitaryRank Rank { get; }
        
        /// <summary>
        /// ФИО спортсмена.
        /// </summary>
        public FullName Name { get; }
        
        /// <summary>
        /// Дата получения спортсменом звания рядового. Если спортсмен не срочник или дата не была указана,
        /// <see cref="BecamePrivateDateTime"/> == <code>01.01.2020</code>.
        /// </summary>
        public DateTime BecamePrivateDateTime { get; private set; }

        private readonly Subject<DateTime> _becamePrivateDateTimeChanged = new();
        
        /// <summary>
        /// Когда изменяется <see cref="BecamePrivateDateTime"/>, генерирует новое значение <see cref="BecamePrivateDateTime"/>.
        /// </summary>
        public IObservable<DateTime> BecamePrivateDateTimeChanged() => _becamePrivateDateTimeChanged;

        /// <summary>
        /// Создаёт запись о новом спортсмене.
        /// </summary>
        /// <param name="rank">Воинское звание.</param>
        /// <param name="name">ФИО.</param>
        public Contestant(MilitaryRank rank, FullName name)
        {
            Guid = Guid.NewGuid();
            Rank = rank;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            BecamePrivateDateTime = new DateTime(2020, 1, 1);
        }

        /// <summary>
        /// Только для сериализаторов!
        /// </summary>
        [JsonConstructor, UsedImplicitly]
        public Contestant(Guid guid, MilitaryRank rank, FullName name, DateTime becamePrivateDateTime)
        {
            Guid = guid;
            Rank = rank;
            Name = name;
            BecamePrivateDateTime = becamePrivateDateTime;
        }

        /// <summary>
        /// Задаёт <see cref="BecamePrivateDateTime"/> спорстмена.
        /// </summary>
        /// <param name="dateTime">Дата получения звания рядового.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void SetBecamePrivateDateTime(DateTime dateTime)
        {
            if (dateTime == BecamePrivateDateTime) return;
            if (dateTime.Year != 2020) throw new ArgumentOutOfRangeException(nameof(dateTime));
            BecamePrivateDateTime = dateTime;
            _becamePrivateDateTimeChanged.OnNext(dateTime);
        }

        private bool Equals(Contestant other)
        {
            return Guid.Equals(other.Guid);
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || obj is Contestant other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Guid.GetHashCode();
        }
    }
}