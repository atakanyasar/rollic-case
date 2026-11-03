using Data.Levels;
using Data.Storage;
using Gameplay.Model;
using UnityEngine;

namespace Gameplay.Flow
{
    public class LevelSession
    {
        private const float ContinueTimeSeconds = 20f;
        private const int ContinueCost = 900;
        private const int WinReward = 40;

        public enum SessionState
        {
            Playing,
            AwaitingContinue,
            ConfirmingRetry,
            Paused,
            Finished,
            Completed
        }

        private readonly LevelProgress _progress;
        private readonly CoinInventory _coins;

        public Level Level { get; }
        public LevelTimer Timer { get; }
        public SessionState State { get; private set; }
        public int CoinBalance => _coins.Balance;
        public bool CanContinue => CoinBalance >= ContinueCost;
        public bool IsPaused => State == SessionState.AwaitingContinue || State == SessionState.ConfirmingRetry || State == SessionState.Paused;

        public LevelSession(LevelCatalog catalog)
        {
            _progress = new LevelProgress(catalog);
            _coins = new CoinInventory();
            Level = new Level(_progress.CurrentLevel);
            Timer = new LevelTimer(Level.TimeLimitSeconds);
            State = SessionState.Playing;
        }

        public bool Tick(float deltaTime)
        {
            if (State != SessionState.Playing)
            {
                return false;
            }
            Timer.Tick(deltaTime);
            if (!Timer.IsExpired)
            {
                return false;
            }
            State = SessionState.AwaitingContinue;
            Timer.Pause();
            return true;
        }

        public bool Continue()
        {
            if (State != SessionState.AwaitingContinue || !_coins.TrySpend(ContinueCost))
            {
                return false;
            }
            Timer.AddTime(ContinueTimeSeconds);
            State = SessionState.Playing;
            Timer.Resume();
            return true;
        }

        public bool DeclineContinue()
        {
            return ChangeState(SessionState.AwaitingContinue, SessionState.Finished);
        }

        public bool OpenRetry()
        {
            return ChangeState(SessionState.Playing, SessionState.ConfirmingRetry);
        }

        public bool CloseRetry()
        {
            return ChangeState(SessionState.ConfirmingRetry, SessionState.Playing);
        }

        public bool ConfirmRetry()
        {
            return ChangeState(SessionState.ConfirmingRetry, SessionState.Finished);
        }

        public bool Pause()
        {
            return ChangeState(SessionState.Playing, SessionState.Paused);
        }

        public bool Resume()
        {
            return ChangeState(SessionState.Paused, SessionState.Playing);
        }

        public bool Finish(bool awardReward)
        {
            if (!ChangeState(SessionState.Completed, SessionState.Finished))
            {
                return false;
            }
            if (awardReward)
            {
                _coins.Add(WinReward);
            }
            return true;
        }

        public bool TryBeginExit(Block block, Vector2Int direction, out Door door)
        {
            door = null;
            return State == SessionState.Playing && Level.TryBeginExit(block, direction, out door);
        }

        public bool CompleteExit(Block block)
        {
            if (State != SessionState.Playing)
            {
                return false;
            }
            Level.RemoveBlock(block);
            if (!Level.IsCompleted)
            {
                return false;
            }
            State = SessionState.Completed;
            Timer.Pause();
            _progress.Advance();
            return true;
        }

        private bool ChangeState(SessionState expected, SessionState next)
        {
            if (State != expected)
            {
                return false;
            }
            State = next;
            if (next == SessionState.Playing)
            {
                Timer.Resume();
            }
            else
            {
                Timer.Pause();
            }
            return true;
        }
    }
}
