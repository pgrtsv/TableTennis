using System;
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

        public Contestant(MilitaryRank rank, FullName name)
        {
            Guid = Guid.NewGuid();
            Rank = rank;
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <summary>
        /// Только для сериализаторов!
        /// </summary>
        [JsonConstructor, UsedImplicitly]
        public Contestant(Guid guid, MilitaryRank rank, FullName name)
        {
            Guid = guid;
            Rank = rank;
            Name = name;
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