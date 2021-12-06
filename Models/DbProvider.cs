using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using DynamicData;

namespace TableTennis.Models
{
    public sealed class DbProvider
    {
        private readonly BehaviorSubject<ContestantsDb?> _contestantsDb;
        public IObservable<ContestantsDb?> ContestantsDb => _contestantsDb;
        private readonly BehaviorSubject<GamesDb?> _gamesDb;
        public IObservable<GamesDb?> GamesDb => _gamesDb;


        public DbProvider()
        {
            _contestantsDb = new BehaviorSubject<ContestantsDb?>(null);
            _gamesDb = new BehaviorSubject<GamesDb?>(null);
            _isSavingContestantsDb = new BehaviorSubject<bool>(false);
            _isSavingGamesDb = new BehaviorSubject<bool>(false);
        }

        private DateTime _lastSavedContestantsDbDateTime;
        private DateTime _lastSavedGamesDbDateTime;
        private BehaviorSubject<bool> _isSavingContestantsDb;
        public bool IsSavingContestantsDb => _isSavingContestantsDb.Value;
        public IObservable<bool> IsSavingContestantsDbConnect() => _isSavingContestantsDb;

        private BehaviorSubject<bool> _isSavingGamesDb;
        public bool IsSavingGamesDb => _isSavingGamesDb.Value;
        public IObservable<bool> IsSavingGamesDbConnect() => _isSavingGamesDb;

        public const string ContestantsDbFilePath = "contestants.json";
        public const string GamesDbFilePath = "games.json";

        private JsonSerializerOptions SerializationSettings { get; } = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true
        };

        public void SaveContestantsDb()
        {
            _isSavingContestantsDb.OnNext(true);
            File.WriteAllText(ContestantsDbFilePath, JsonSerializer.Serialize(
                _contestantsDb.Value,
                SerializationSettings));
            _lastSavedContestantsDbDateTime = File.GetLastWriteTime(ContestantsDbFilePath);
            _isSavingContestantsDb.OnNext(false);
        }

        public void SaveGamesDb()
        {
            _isSavingGamesDb.OnNext(true);
            File.WriteAllText(GamesDbFilePath, JsonSerializer.Serialize(
                _gamesDb.Value,
                SerializationSettings));
            _lastSavedGamesDbDateTime = File.GetLastWriteTime(GamesDbFilePath);
            _isSavingGamesDb.OnNext(false);
        }

        public void LoadContestantsDb()
        {
            _contestantsDb.OnNext(JsonSerializer.Deserialize<ContestantsDb>(
                File.ReadAllText(ContestantsDbFilePath),
                SerializationSettings));
        }

        public void LoadGamesDb()
        {
            var gamesDb = JsonSerializer.Deserialize<GamesDb>(
                File.ReadAllText(GamesDbFilePath),
                SerializationSettings);
            if (gamesDb == null)
                throw new InvalidOperationException();
            _gamesDb.OnNext(gamesDb);
        }

        public void LoadOrNew()
        {
            var doesContestantsDbExist = File.Exists(ContestantsDbFilePath);
            if (doesContestantsDbExist)
                LoadContestantsDb();
            else
                _contestantsDb.OnNext(new ContestantsDb());

            if (File.Exists(GamesDbFilePath))
            {
                if (!doesContestantsDbExist) throw new Exception();
                LoadGamesDb();
            }
            else
            {
                var gamesDb = new GamesDb();
                _gamesDb.OnNext(gamesDb);
            }
        }

        public void Bench()
        {
            var random = new Random();
            var contestants = Enumerable.Range(0, 40)
                .Select(_ => new Contestant(MilitaryRank.Private, new FullName("А.", "Спортсмен", "Б.")))
                .ToArray();
            _contestantsDb.OnNext(new ContestantsDb(contestants));
            var gameResults = Enumerable.Range(0, 10000)
                .Select(_ =>
                {
                    var firstContestantIndex = random.Next(0, contestants.Length);
                    var secondContestantIndex = random.Next(0, contestants.Length);
                    while (firstContestantIndex == secondContestantIndex)
                        secondContestantIndex = random.Next(0, contestants.Length);
                    return new GameResult(
                        new ContestantScore(contestants[firstContestantIndex], 11),
                        new ContestantScore(contestants[secondContestantIndex], random.Next(0, 10)));
                })
                .ToArray();
            var gamesDb = new GamesDb(gameResults);
            _gamesDb.OnNext(gamesDb);
        }

        public IDisposable EnableAutoSaving(TimeSpan throttle) => new CompositeDisposable(
            GamesDb.Select(db => db!.GamesResultsPreview().Select(_ => Unit.Default))
                .Switch()
                .Throttle(throttle)
                .Subscribe(_ => SaveGamesDb()),
            ContestantsDb.Select(db => db!.ContestantsPreview()
                    .AutoRefreshOnObservable(contestant => contestant.BecamePrivateDateTimeChanged())
                    .Select(_ => Unit.Default))
                .Switch()
                .Throttle(throttle)
                .Subscribe(_ => SaveContestantsDb())
        );

        public IDisposable EnableAutoLoading(TimeSpan throttle)
        {
            IObservable<EventPattern<FileSystemEventArgs>> WatchCreatedOrChanged(FileSystemWatcher fileSystemWatcher) =>
                Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                    handler =>
                    {
                        fileSystemWatcher.Created += handler;
                        fileSystemWatcher.Changed += handler;
                    },
                    handler =>
                    {
                        fileSystemWatcher.Created -= handler;
                        fileSystemWatcher.Changed -= handler;
                    });

            var contestantsDbWatcher = new FileSystemWatcher(".", ContestantsDbFilePath) {EnableRaisingEvents = true};
            var gamesDbWatcher = new FileSystemWatcher(".", GamesDbFilePath) {EnableRaisingEvents = true};
            return new CompositeDisposable(
                WatchCreatedOrChanged(contestantsDbWatcher)
                    .Where(_ => File.GetLastWriteTime(ContestantsDbFilePath) != _lastSavedContestantsDbDateTime && !IsSavingContestantsDb)
                    .Throttle(throttle)
                    .ObserveOn(SynchronizationContext.Current!)
                    .Subscribe(_ => LoadContestantsDb()),
                WatchCreatedOrChanged(gamesDbWatcher)
                    .Where(_ => File.GetLastWriteTime(GamesDbFilePath) != _lastSavedGamesDbDateTime && !IsSavingGamesDb)
                    .Throttle(throttle)
                    .ObserveOn(SynchronizationContext.Current!)
                    .Subscribe(_ => LoadGamesDb()),
                contestantsDbWatcher,
                gamesDbWatcher);
        }
    }
}