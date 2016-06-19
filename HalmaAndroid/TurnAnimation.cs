namespace HalmaAndroid
{
    class TurnAnimation
    {
        public Turn Turn { get; private set; } = new Turn();
        public uint Player { get; private set; }

        private const double timePerWaypoint = 0.25f;
        private int reachedWaypoint = -1;
        private float waypointPercentage = 0.0f;

        private System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

        public bool Active => reachedWaypoint >= 0;

        public event GameView.AnimationFinishedHandler AnimationFinished;

        public void Update()
        {
            double timeSinceLastStep = timer.Elapsed.TotalSeconds;
            timer.Restart();

            if (reachedWaypoint < 0 || Turn.TurnSequence == null)
                return;

            waypointPercentage += (float)(timeSinceLastStep / timePerWaypoint);
            if (waypointPercentage > 1.0f)
            {
                waypointPercentage -= 1.0f;
                ++reachedWaypoint;
                if (reachedWaypoint >= Turn.TurnSequence.Count - 1)
                {
                    reachedWaypoint = -1;
                    waypointPercentage = 0.0f;

                    AnimationFinished?.Invoke();
                }
            }
        }

        public void GetCurrentCartesian(out float x, out float y)
        {
            HexCoord a = Turn.TurnSequence[reachedWaypoint];
            float aX, aY;
            a.ToCartesian(out aX, out aY);

            HexCoord b = Turn.TurnSequence[reachedWaypoint + 1];
            float bX, bY;
            b.ToCartesian(out bX, out bY);

            float animationPercentage = MathUtils.Smootherstep(0.0f, 1.0f, waypointPercentage);
            x = MathUtils.Lerp(aX, bX, animationPercentage);
            y = MathUtils.Lerp(aY, bY, animationPercentage);
        }

        public void AnimateTurn(Turn turn, uint player)
        {
            System.Diagnostics.Debug.Assert(turn.TurnSequence != null);
            System.Diagnostics.Debug.Assert(turn.TurnSequence.Count > 1);

            this.Turn = turn;
            this.Player = player;
            this.reachedWaypoint = 0;
            this.timer.Reset();
        }
    }
}