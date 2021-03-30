using System;
using System.Text.RegularExpressions;

namespace TableTennis.Models
{
    /// <summary>
    /// ФИО.
    /// </summary>
    public sealed class FullName: IComparable
    {
        /// <summary>
        /// Инициал имени.
        /// </summary>
        public string FirstName { get; }
        
        /// <summary>
        /// Фамилия.
        /// </summary>
        public string LastName { get; }
        
        /// <summary>
        /// Инициал отчества.
        /// </summary>
        public string ParentName { get; }

        public FullName(string firstName, string lastName, string parentName)
        {
            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentNullException(nameof(lastName));
            FirstName = firstName;
            LastName = lastName ?? string.Empty;
            ParentName = parentName ?? string.Empty;
        }

        public static bool TryParse(out FullName? fullName, string text)
        {
            var match = Regex.Match(text, @"^[A-Я][а-я]+\s[A-Я]\.\s[А-Я].$");
            if (!match.Success)
            {
                fullName = default;
                return false;
            }

            fullName = Parse(text);
            return true;
        }

        public static FullName Parse(string text)
        {
            var matches = Regex.Matches(text, @"[А-Я][а-я]*\.?");
            return new FullName(matches[1].Value, matches[0].Value, matches[2].Value);
        }

        private bool Equals(FullName other)
        {
            return FirstName == other.FirstName && LastName == other.LastName && ParentName == other.ParentName;
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || obj is FullName other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(FirstName, LastName, ParentName);
        }

        public override string ToString() => $"{LastName} {FirstName} {ParentName}";
        
        public int CompareTo(object? obj)
        {
            if (!(obj is FullName otherName))
                throw new NotSupportedException();
            return CompareTo(otherName);
        }

        public int CompareTo(FullName? other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            var lastNameComparison = string.Compare(LastName, other.LastName, StringComparison.Ordinal);
            if (lastNameComparison != 0) return lastNameComparison;
            var firstNameComparison = string.Compare(FirstName, other.FirstName, StringComparison.Ordinal);
            if (firstNameComparison != 0) return firstNameComparison;
            return string.Compare(ParentName, other.ParentName, StringComparison.Ordinal);
        }
    }
}