using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace TableTennis
{
    public sealed class AppSettings
    {
        public AppSettings()
        {
            IsRubetsEnabled = true;
            SectionNumber = 1;
        }

        /// <summary>
        /// Только для сериализаторов!
        /// </summary>
        [JsonConstructor, UsedImplicitly]
        public AppSettings(bool isRubetsEnabled, int sectionNumber, bool areDembelFactsEnabled, TimeSpan dembelFactsDelay)
        {
            IsRubetsEnabled = isRubetsEnabled;
            SectionNumber = sectionNumber;
            AreDembelFactsEnabled = areDembelFactsEnabled;
            DembelFactsDelay = dembelFactsDelay;
        }

        /// <summary>
        /// true, если включена Рубец-интеграция.
        /// </summary>
        public bool IsRubetsEnabled { get; set; }

        /// <summary>
        /// Номер роты.
        /// </summary>
        public int SectionNumber { get; set; }
        
        /// <summary>
        /// Если true, дембель-факты показываются в приложении.
        /// </summary>
        public bool AreDembelFactsEnabled { get; set; }
        
        /// <summary>
        /// Время задержки между показами дембель-фактов.
        /// </summary>
        public TimeSpan DembelFactsDelay { get; set; }

        public const string FilePath = "settings.json";

        public void Save() => File.WriteAllText(FilePath, JsonSerializer.Serialize(this));

        public static AppSettings Load() => JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath)) ??
                                            throw new InvalidOperationException();

        public static AppSettings LoadOrNew() => File.Exists(FilePath) ? Load() : new AppSettings();
    }
}