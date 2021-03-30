using System;
using System.IO;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;

namespace TableTennis.Models
{
    public sealed class GamesDbProvider
    {
        private readonly BehaviorSubject<GamesDb?> _gamesDb;

        public GamesDbProvider() => _gamesDb = new BehaviorSubject<GamesDb?>(null);

        private DateTime _lastSavedDateTime = default;

        public IObservable<GamesDb?> GamesDb => _gamesDb;
        
        public const string FilePath = "tennis_db.json";

        private JsonSerializerOptions SerializationSettings => new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        
        public void Save()
        {
            File.WriteAllText(FilePath, JsonSerializer.Serialize(
                _gamesDb.Value,
                SerializationSettings));
            _lastSavedDateTime = File.GetLastWriteTime(FilePath);
        }

        public void Load() => _gamesDb.OnNext(JsonSerializer.Deserialize<GamesDb>(
            File.ReadAllText(FilePath), 
            SerializationSettings));

        public void LoadOrNew()
        {
            if (File.Exists(FilePath))
                Load();
            else
                _gamesDb.OnNext(new GamesDb());
        }

        public IDisposable EnableAutoSaving(TimeSpan throttle) => GamesDb.Select(db => 
                Observable.Merge(
                        db!.GamesResultsPreview()
                            .Select(_ => Unit.Default),
                        db.ContestantsPreview()
                            .Select(_ => Unit.Default)
                        ))
            .Switch()
            .Throttle(throttle)
            .Subscribe(_ => Save());

        public IDisposable EnableAutoLoading(TimeSpan throttle)
        {
            var watcher = new FileSystemWatcher(".", FilePath);
            var dbFileCreated = Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                x => watcher.Created += x,
                x => watcher.Created -= x
            );
            var dbFileChanged = Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                x => watcher.Changed += x,
                x => watcher.Changed -= x
            );
            watcher.EnableRaisingEvents = true;
            return new CompositeDisposable(dbFileCreated.Merge(dbFileChanged)
                    .ObserveOn(new NewThreadScheduler())
                    .Where(_ => File.GetLastWriteTime(FilePath) != _lastSavedDateTime)
                    .Throttle(throttle)
                    .Subscribe(_ => Load()),
                watcher);
        }
    }
}