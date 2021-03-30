using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using TableTennis.Models.Json;

namespace TableTennis.Models
{
    [JsonConverter(typeof(MilitaryRankConverter))]
    public sealed class MilitaryRank: IComparable
    {
        public int Id { get; }
        public string Name { get; }

        private MilitaryRank(int id, string name)
        {
            Id = id;
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            Name = name;
        }
        public static MilitaryRank None { get; } = new MilitaryRank(1, "-");
        public static MilitaryRank Private { get; } = new MilitaryRank(2, "Рядовой");
        public static MilitaryRank LanceCorporal { get; } = new MilitaryRank(3, "Ефрейтор");
        public static MilitaryRank Sergeant { get; } = new MilitaryRank(4, "Сержант");
        public static MilitaryRank SrSergeant { get; } = new MilitaryRank(5, "Старший сержант");
        public static MilitaryRank Leutenant { get; } = new MilitaryRank(6, "Лейтенант");
        public static MilitaryRank SrLeutenant { get; } = new MilitaryRank(7, "Старший лейтенант");
        public static MilitaryRank Captain { get; } = new MilitaryRank(8, "Капитан");
        public static MilitaryRank Major { get; } = new MilitaryRank(9, "Майор");

        public static IEnumerable<MilitaryRank> GetAll() => new[] { None, Private, LanceCorporal, Sergeant, SrSergeant, Leutenant, SrLeutenant, Captain, Major };

        public static MilitaryRank GetById(int id) => GetAll().First(x => x.Id == id);
        
        public override string ToString() => Name;
        public int CompareTo(object? other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            if (!(other is MilitaryRank otherRank))
                return 1;
            if (Id == otherRank.Id) return 0;
            if (Id < otherRank.Id) return -1;
            return 1;
        }
    }
}