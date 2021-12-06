using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using Newtonsoft.Json;
using Stateless;
using TableTennis.Models.Tournaments.Validators;

namespace TableTennis.Models.Tournaments
{
    public sealed class Tournament
    {
        private readonly StateMachine<TournamentState, Trigger> _machine;

        public Tournament()
        {
            _rules = new TournamentRules();
            _state = new BehaviorSubject<TournamentState>(TournamentState.Created());
            _contestantsGuids = new Guid[_rules.MaxContestantsCount()];
            _machine = new StateMachine<TournamentState, Trigger>(() => _state.Value,
                newState => _state.OnNext(newState));
            _machine.Configure(TournamentState.Created())
                .Permit(Trigger.StartTournament, TournamentState.TournamentInProgress.FirstWave());
            _machine.Configure(TournamentState.TournamentInProgress.FirstWave())
                .Permit(Trigger.Cancel, TournamentState.Cancelled())
                .Permit(Trigger.MoveToNextWave, TournamentState.TournamentInProgress.SecondWave());
            _machine.Configure(TournamentState.TournamentInProgress.SecondWave())
                .Permit(Trigger.Cancel, TournamentState.Cancelled())
                .Permit(Trigger.MoveToNextWave, TournamentState.TournamentInProgress.SemiFinal());
            _machine.Configure(TournamentState.TournamentInProgress.SemiFinal())
                .Permit(Trigger.Cancel, TournamentState.Cancelled())
                .Permit(Trigger.MoveToNextWave, TournamentState.TournamentInProgress.Final());
            _machine.Configure(TournamentState.TournamentInProgress.Final())
                .Permit(Trigger.Cancel, TournamentState.Cancelled())
                .Permit(Trigger.Finish, TournamentState.Finished());
        }

        [JsonConstructor]
        public Tournament(
            TournamentState currentState,
            TournamentRules rules,
            IEnumerable<Guid> contestants)
        {
            _state = new BehaviorSubject<TournamentState>(currentState);
            _rules = rules;
            _contestantsGuids = contestants.ToArray();
        }

        enum Trigger
        {
            StartTournament,
            MoveToNextWave,
            Finish,
            Cancel,
            SetGameResult,
            StartGame
        }

        private readonly TournamentRules _rules;
        public TournamentRules Rules => _rules;
        public bool AreRulesValid() => new TournamentRulesValidator().Validate(_rules).IsValid;

        private readonly BehaviorSubject<TournamentState> _state;
        public TournamentState CurrentState => _state.Value;
        public IObservable<TournamentState> StateConnect() => _state;

        private readonly Guid[] _contestantsGuids;
        public IEnumerable<Guid> ContestantsGuids => _contestantsGuids;
        public IEnumerable<Guid> GetActiveContestantsGuids() => _contestantsGuids.Where(x => x != default);

        public bool AreContestantsValid()
        {
            var contestants = _contestantsGuids.Where(x => x != default).ToArray();
            if (contestants.Length < _rules.MinContestantsCount()) return false;
            if (contestants.Distinct().Count() != contestants.Length) return false;
            return true;
        }

        public void SetContestant(Guid contestantGuid, int position)
        {
            if (_state.Value is not TournamentState.TournamentCreated) throw new InvalidOperationException();
            _contestantsGuids[position] = contestantGuid;
        }

        public bool CanTournamentStart() =>
            AreRulesValid() && AreContestantsValid() && _state.Value is TournamentState.TournamentCreated;

        /// <summary>
        /// Начинает турнир.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void StartTournament()
        {
            if (!CanTournamentStart()) throw new InvalidOperationException();
            _machine.Fire(Trigger.StartTournament);
        }

        public bool CanCancelTournament() => _state.Value is TournamentState.TournamentInProgress;

        /// <summary>
        /// Отменяет начатый турнир.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void CancelTournament()
        {
            if (!CanCancelTournament()) throw new InvalidOperationException();
            _machine.Fire(Trigger.Cancel);
        }

        /// <summary>
        /// Задаёт результат текущего матча.
        /// </summary>
        public void SetGameResult(int firstContestantScore, int secondContestantScore)
        {
        }

        /// <summary>
        /// Начинает матч досрочно.
        /// </summary>
        public void StartGameNow()
        {
        }

        private void OnTournamentStarted()
        {
        }

        // private void StartFirstWave()
        // {

        //
        //     var fullPairs = pairs.Where(pair => pair.IsFull()).ToArray();
        //     // foreach (var pair in fullPairs)
        // }

        // private void Play

        /// <summary>
        /// Состояние турнира.
        /// </summary>
        public abstract class TournamentState
        {
            public abstract int Id { get; }

            public sealed class TournamentCreated : TournamentState
            {
                internal TournamentCreated()
                {
                }

                public override int Id => 0;

                public override T Switch<T>(
                    Func<TournamentCreated, T> created,
                    Func<TournamentInProgress, T> started,
                    Func<TournamentFinished, T> finished,
                    Func<TournamentCancelled, T> cancelled) => created.Invoke(this);
            }

            public abstract class TournamentInProgress : TournamentState
            {
                public sealed class FirstWaveInProgress : TournamentInProgress
                {
                    internal FirstWaveInProgress(IEnumerable<Guid> contestantsGuids)
                    {
                        var contestants = contestantsGuids.ToArray();
                        if (contestants.Length != 16)
                            throw new ArgumentException(string.Empty, nameof(contestantsGuids));
                        _gamesStates = new GameState[8];
                        for (var i = 0; i < 8; i++)
                            _gamesStates[i] = GameState.Created(contestants[i * 2], contestants[i * 2 + 1]);
                        _currentGame = new BehaviorSubject<GameState>(_gamesStates[0]);
                        var machine = new StateMachine<GameState, Trigger>(
                            () => _currentGame.Value,
                            newState => _currentGame.OnNext(newState))
                            .Configure();
                        
                        for (int i = 0; i < 8; i++)
                        {
                            var gameState = _gamesStates[i];
                            if (!gameState.CanBePlayed())
                            {
                                _gamesStates[i] = GameState.Skipped(gameState);
                                _currentGame.OnNext(_gamesStates[i]);
                                _currentGame.OnNext(_gamesStates[i + 1]);
                                continue;
                            }
                            
                            _currentGame.OnNext(GameState.Preparing(gameState));
                        }
                    }

                    public override int Id => 1;

                    private readonly GameState[] _gamesStates;

                    public IEnumerable<GameState> GamesStates => _gamesStates;

                    private readonly BehaviorSubject<GameState> _currentGame;

                    public GameState CurrentGame => _currentGame.Value;

                    public IObservable<GameState> CurrentGameConnect() => _currentGame;

                    public override T Switch<T>(
                        Func<FirstWaveInProgress, T> firstWave,
                        Func<SecondWaveInProgress, T> secondWave,
                        Func<SemiFinalInProgress, T> semiFinal,
                        Func<FinalInProgress, T> final) =>
                        firstWave.Invoke(this);
                }

                public sealed class SecondWaveInProgress : TournamentInProgress
                {
                    internal SecondWaveInProgress()
                    {
                    }

                    public override int Id => 2;

                    public override T Switch<T>(
                        Func<FirstWaveInProgress, T> firstWave,
                        Func<SecondWaveInProgress, T> secondWave,
                        Func<SemiFinalInProgress, T> semiFinal,
                        Func<FinalInProgress, T> final) =>
                        secondWave.Invoke(this);
                }

                public sealed class SemiFinalInProgress : TournamentInProgress
                {
                    internal SemiFinalInProgress()
                    {
                    }


                    public override int Id => 3;

                    public override T Switch<T>(
                        Func<FirstWaveInProgress, T> firstWave,
                        Func<SecondWaveInProgress, T> secondWave,
                        Func<SemiFinalInProgress, T> semiFinal,
                        Func<FinalInProgress, T> final) =>
                        semiFinal.Invoke(this);
                }

                public sealed class FinalInProgress : TournamentInProgress
                {
                    internal FinalInProgress()
                    {
                    }

                    public override int Id => 4;

                    public override T Switch<T>(
                        Func<FirstWaveInProgress, T> firstWave,
                        Func<SecondWaveInProgress, T> secondWave,
                        Func<SemiFinalInProgress, T> semiFinal,
                        Func<FinalInProgress, T> final) =>
                        final.Invoke(this);
                }

                public static FirstWaveInProgress FirstWave() => new();

                public static SecondWaveInProgress SecondWave() => new();
                public static SemiFinalInProgress SemiFinal() => new();
                public static FinalInProgress Final() => new();

                public abstract T Switch<T>(
                    Func<FirstWaveInProgress, T> firstWave,
                    Func<SecondWaveInProgress, T> secondWave,
                    Func<SemiFinalInProgress, T> semiFinal,
                    Func<FinalInProgress, T> final);

                public override T Switch<T>(
                    Func<TournamentCreated, T> created,
                    Func<TournamentInProgress, T> started,
                    Func<TournamentFinished, T> finished,
                    Func<TournamentCancelled, T> cancelled) =>
                    started.Invoke(this);
            }

            public sealed class TournamentFinished : TournamentState
            {
                public override int Id => 5;

                public override T Switch<T>(
                    Func<TournamentCreated, T> created,
                    Func<TournamentInProgress, T> started,
                    Func<TournamentFinished, T> finished,
                    Func<TournamentCancelled, T> cancelled) =>
                    finished.Invoke(this);
            }

            public sealed class TournamentCancelled : TournamentState
            {
                public override int Id => 6;

                public override T Switch<T>(
                    Func<TournamentCreated, T> created,
                    Func<TournamentInProgress, T> started,
                    Func<TournamentFinished, T> finished,
                    Func<TournamentCancelled, T> cancelled) =>
                    cancelled.Invoke(this);
            }

            public static TournamentCreated Created() => new();

            public static TournamentFinished Finished() => new();

            public static TournamentCancelled Cancelled() => new();

            public abstract T Switch<T>(
                Func<TournamentCreated, T> created,
                Func<TournamentInProgress, T> started,
                Func<TournamentFinished, T> finished,
                Func<TournamentCancelled, T> cancelled);
        }

        /// <summary>
        /// Состояние матча турнира.
        /// </summary>
        public abstract class GameState
        {
            public abstract int Id { get; }
            
            public abstract Guid GameGuid { get; }

            public abstract Guid FirstContestantGuid { get; }

            public abstract Guid SecondContestantGuid { get; }
            
            public bool CanBePlayed() => FirstContestantGuid != default && SecondContestantGuid != default;

            /// <summary>
            /// Матч ещё не начат.
            /// </summary>
            public sealed class GameCreated : GameState
            {
                public GameCreated(Guid firstContestantGuid, Guid secondContestantGuid)
                {
                    if (firstContestantGuid == secondContestantGuid)
                        throw new ArgumentException(string.Empty, nameof(secondContestantGuid));
                    FirstContestantGuid = firstContestantGuid;
                    SecondContestantGuid = secondContestantGuid;
                    GameGuid = Guid.NewGuid();
                }

                public override int Id => 1;
                
                public override Guid GameGuid { get; }
                public override Guid FirstContestantGuid { get; }
                public override Guid SecondContestantGuid { get; }

                public override T Match<T>(Func<GameCreated, T> notStarted, Func<GamePreparing, T> preparing,
                    Func<GameInProgress, T> inProgress, Func<GameFinished, T> finished, Func<GamePaused, T> paused,
                    Func<GameCancelled, T> cancelled,
                    Func<GameSkipped, T> skipped) =>
                    notStarted.Invoke(this);

                public override void Match(Action<GameCreated> notStarted, Action<GamePreparing> preparing,
                    Action<GameInProgress> inProgress, Action<GameFinished> finished, Action<GamePaused> paused,
                    Action<GameCancelled> cancelled,
                    Action<GameSkipped> skipped) =>
                    notStarted.Invoke(this);
            }

            /// <summary>
            /// Подготовка к матчу.
            /// </summary>
            public sealed class GamePreparing : GameState
            {
                public GamePreparing(GameState previousState)
                {
                    GameGuid = previousState.GameGuid;
                    FirstContestantGuid = previousState.FirstContestantGuid;
                    SecondContestantGuid = previousState.SecondContestantGuid;
                }

                public override int Id => 2;
                public override Guid GameGuid { get; }
                public override Guid FirstContestantGuid { get; }
                public override Guid SecondContestantGuid { get; }

                public override T Match<T>(Func<GameCreated, T> notStarted, Func<GamePreparing, T> preparing,
                    Func<GameInProgress, T> inProgress, Func<GameFinished, T> finished, Func<GamePaused, T> paused,
                    Func<GameCancelled, T> cancelled,
                    Func<GameSkipped, T> skipped) =>
                    preparing.Invoke(this);

                public override void Match(Action<GameCreated> notStarted, Action<GamePreparing> preparing,
                    Action<GameInProgress> inProgress, Action<GameFinished> finished, Action<GamePaused> paused,
                    Action<GameCancelled> cancelled,
                    Action<GameSkipped> skipped) =>
                    preparing.Invoke(this);
            }

            /// <summary>
            /// Спортсмены играют матч.
            /// </summary>
            public sealed class GameInProgress : GameState
            {
                public GameInProgress(GameState previousState)
                {
                    GameGuid = previousState.GameGuid;
                    FirstContestantGuid = previousState.FirstContestantGuid;
                    SecondContestantGuid = previousState.SecondContestantGuid;
                }

                public override int Id => 3;
                public override Guid GameGuid { get; }
                public override Guid FirstContestantGuid { get; }
                public override Guid SecondContestantGuid { get; }

                public override T Match<T>(Func<GameCreated, T> notStarted, Func<GamePreparing, T> preparing,
                    Func<GameInProgress, T> inProgress, Func<GameFinished, T> finished, Func<GamePaused, T> paused,
                    Func<GameCancelled, T> cancelled,
                    Func<GameSkipped, T> skipped) =>
                    inProgress.Invoke(this);

                public override void Match(Action<GameCreated> notStarted, Action<GamePreparing> preparing,
                    Action<GameInProgress> inProgress, Action<GameFinished> finished, Action<GamePaused> paused,
                    Action<GameCancelled> cancelled,
                    Action<GameSkipped> skipped) =>
                    inProgress.Invoke(this);
            }

            /// <summary>
            /// Матч завершён.
            /// </summary>
            public sealed class GameFinished : GameState
            {
                /// <inheritdoc />
                public override int Id => 4;

                public override Guid GameGuid { get; }

                /// <inheritdoc />
                public override Guid FirstContestantGuid { get; }

                /// <inheritdoc />
                public override Guid SecondContestantGuid { get; }

                public GameFinished(
                    GameState previousState,
                    int firstContestantScore,
                    int secondContestantScore,
                    TimeSpan timeLeft,
                    TimeSpan timeSpent)
                {
                    FirstContestantScore = firstContestantScore;
                    SecondContestantScore = secondContestantScore;
                    TimeLeft = timeLeft;
                    TimeSpent = timeSpent;
                    GameGuid = previousState.GameGuid;
                    FirstContestantGuid = previousState.FirstContestantGuid;
                    SecondContestantGuid = previousState.SecondContestantGuid;
                    DateTime = DateTime.Now;
                }

                /// <summary>
                /// Счёт первого спортсмена.
                /// </summary>
                public int FirstContestantScore { get; }

                /// <summary>
                /// Счёт второго спортсмена.
                /// </summary>
                public int SecondContestantScore { get; }

                /// <summary>
                /// Дата и время, когда закончился матч.
                /// </summary>
                public DateTime DateTime { get; }

                /// <summary>
                /// Сколько времени оставалось играть на момент окончания матча.
                /// </summary>
                public TimeSpan TimeLeft { get; }

                /// <summary>
                /// Сколько времени длился матч.
                /// </summary>
                public TimeSpan TimeSpent { get; }


                public override T Match<T>(Func<GameCreated, T> notStarted, Func<GamePreparing, T> preparing,
                    Func<GameInProgress, T> inProgress, Func<GameFinished, T> finished, Func<GamePaused, T> paused,
                    Func<GameCancelled, T> cancelled,
                    Func<GameSkipped, T> skipped) =>
                    finished.Invoke(this);

                public override void Match(Action<GameCreated> notStarted, Action<GamePreparing> preparing,
                    Action<GameInProgress> inProgress, Action<GameFinished> finished, Action<GamePaused> paused,
                    Action<GameCancelled> cancelled,
                    Action<GameSkipped> skipped) =>
                    finished.Invoke(this);
            }

            /// <summary>
            /// Матч приостановлен.
            /// </summary>
            public sealed class GamePaused : GameState
            {
                public GamePaused(GameState previousState)
                {
                    GameGuid = previousState.GameGuid;
                    FirstContestantGuid = previousState.FirstContestantGuid;
                    SecondContestantGuid = previousState.SecondContestantGuid;
                }

                public override int Id => 5;
                public override Guid GameGuid { get; }

                /// <inheritdoc />
                public override Guid FirstContestantGuid { get; }

                /// <inheritdoc />
                public override Guid SecondContestantGuid { get; }

                public override T Match<T>(Func<GameCreated, T> notStarted, Func<GamePreparing, T> preparing,
                    Func<GameInProgress, T> inProgress, Func<GameFinished, T> finished, Func<GamePaused, T> paused,
                    Func<GameCancelled, T> cancelled,
                    Func<GameSkipped, T> skipped) =>
                    paused.Invoke(this);

                public override void Match(Action<GameCreated> notStarted, Action<GamePreparing> preparing,
                    Action<GameInProgress> inProgress, Action<GameFinished> finished, Action<GamePaused> paused,
                    Action<GameCancelled> cancelled,
                    Action<GameSkipped> skipped) =>
                    paused.Invoke(this);
            }

            /// <summary>
            /// Матч отменён из-за отмены турнира.
            /// </summary>
            public sealed class GameCancelled : GameState
            {
                public GameCancelled(GameState previousState)
                {
                    GameGuid = previousState.GameGuid;
                    FirstContestantGuid = previousState.FirstContestantGuid;
                    SecondContestantGuid = previousState.SecondContestantGuid;
                }

                public override int Id => 6;
                public override Guid GameGuid { get; }

                /// <inheritdoc />
                public override Guid FirstContestantGuid { get; }

                /// <inheritdoc />
                public override Guid SecondContestantGuid { get; }

                public override T Match<T>(Func<GameCreated, T> notStarted, Func<GamePreparing, T> preparing,
                    Func<GameInProgress, T> inProgress, Func<GameFinished, T> finished, Func<GamePaused, T> paused,
                    Func<GameCancelled, T> cancelled,
                    Func<GameSkipped, T> skipped) =>
                    cancelled.Invoke(this);

                public override void Match(Action<GameCreated> notStarted, Action<GamePreparing> preparing,
                    Action<GameInProgress> inProgress, Action<GameFinished> finished, Action<GamePaused> paused,
                    Action<GameCancelled> cancelled,
                    Action<GameSkipped> skipped) =>
                    cancelled.Invoke(this);
            }

            /// <summary>
            /// Матч автоматически пропущен.
            /// </summary>
            public sealed class GameSkipped : GameState
            {
                public GameSkipped(GameState previousState)
                {
                    GameGuid = previousState.GameGuid;
                    FirstContestantGuid = previousState.FirstContestantGuid;
                    SecondContestantGuid = previousState.SecondContestantGuid;
                }

                public override int Id => 6;
                public override Guid GameGuid { get; }

                /// <inheritdoc />
                public override Guid FirstContestantGuid { get; }

                /// <inheritdoc />
                public override Guid SecondContestantGuid { get; }

                public override T Match<T>(Func<GameCreated, T> notStarted, Func<GamePreparing, T> preparing,
                    Func<GameInProgress, T> inProgress, Func<GameFinished, T> finished, Func<GamePaused, T> paused,
                    Func<GameCancelled, T> cancelled,
                    Func<GameSkipped, T> skipped) =>
                    skipped.Invoke(this);

                public override void Match(Action<GameCreated> notStarted, Action<GamePreparing> preparing,
                    Action<GameInProgress> inProgress, Action<GameFinished> finished, Action<GamePaused> paused,
                    Action<GameCancelled> cancelled,
                    Action<GameSkipped> skipped) =>
                    skipped.Invoke(this);
            }

            public abstract T Match<T>(
                Func<GameCreated, T> notStarted,
                Func<GamePreparing, T> preparing,
                Func<GameInProgress, T> inProgress,
                Func<GameFinished, T> finished,
                Func<GamePaused, T> paused,
                Func<GameCancelled, T> cancelled,
                Func<GameSkipped, T> skipped);

            public abstract void Match(
                Action<GameCreated> notStarted,
                Action<GamePreparing> preparing,
                Action<GameInProgress> inProgress,
                Action<GameFinished> finished,
                Action<GamePaused> paused,
                Action<GameCancelled> cancelled,
                Action<GameSkipped> skipped);

            public static GameFinished Finished(
                GameState previousState,
                int firstContestantScore,
                int secondContestantScore,
                TimeSpan timeLeft,
                TimeSpan timeSpent) =>
                new(previousState, firstContestantScore, secondContestantScore, timeLeft,
                    timeSpent);

            public static GameCreated Created(Guid firstContestantGuid, Guid secondContestantGuid) =>
                new(firstContestantGuid, secondContestantGuid);

            public static GameSkipped Skipped(GameState previousState) =>
                new(previousState);

            public static GamePreparing Preparing(GameState previousState) =>
                new(previousState);

            public static GameInProgress InProgress(GameState previousState) =>
                new(previousState);

            public static GamePaused Paused(GameState previousState) =>
                new(previousState);

            public static GameCancelled Cancelled(GameState previousState) =>
                new(previousState);
        }
    }
}