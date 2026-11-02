using System;

namespace Gameplay
{
    public class LevelTimer
    {
        public float RemainingSeconds { get; private set; }
        public bool IsExpired => RemainingSeconds <= 0f;
        public bool IsPaused { get; private set; }

        public LevelTimer(int durationSeconds)
        {
            RemainingSeconds = durationSeconds;
        }

        public void Tick(float deltaTime)
        {
            if (IsPaused)
            {
                return;
            }

            RemainingSeconds = Math.Max(0f, RemainingSeconds - deltaTime);
        }

        public void Pause()
        {
            IsPaused = true;
        }

        public void Resume()
        {
            IsPaused = false;
        }
    }
}
