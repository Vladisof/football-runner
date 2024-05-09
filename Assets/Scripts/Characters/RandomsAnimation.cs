using UnityEngine;
namespace Characters
{
	public class RandomsAnimation : StateMachineBehaviour
	{
		public string parameter;

		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			animator.SetInteger(parameter, 0);
		}
	}
}
