namespace GameManager
{

	public class Modifier
	{
		public virtual void OnRunStart(GamesState state)
		{

		}

		public virtual void OnRunTick(GamesState state)
		{

		}

		public virtual bool OnRunEnd(GamesState state)
		{
			return true;
		}
	}

	public class LimitedLengthRun : Modifier
	{
		public float distance;

		public LimitedLengthRun(float dist)
		{
			distance = dist;
		}

		public override void OnRunTick(GamesState state)
		{
			if(state.trackManager.worldDistance >= distance)
			{
				state.trackManager.CharactersController.currentLife = 0;
			}
		}

		public override void OnRunStart(GamesState state)
		{

		}

		public override bool OnRunEnd(GamesState state)
		{
			state.QuitToLoadout();
			return false;
		}
	}

	public class SeededRun : Modifier
	{
		int m_Seed;

		protected const int k_DaysInAWeek = 7;

		public SeededRun()
		{
			m_Seed = System.DateTime.Now.DayOfYear / k_DaysInAWeek;
		}

		public override void OnRunStart(GamesState state)
		{
			state.trackManager.trackSeed = m_Seed;
		}

		public override bool OnRunEnd(GamesState state)
		{
			state.QuitToLoadout();
			return false;
		}
	}

	public class SingleLifeRun : Modifier
	{
		public override void OnRunTick(GamesState state)
		{
			if (state.trackManager.CharactersController.currentLife > 1)
				state.trackManager.CharactersController.currentLife = 1;
		}


		public override void OnRunStart(GamesState state)
		{

		}

		public override bool OnRunEnd(GamesState state)
		{
			state.QuitToLoadout();
			return false;
		}
	}
}