using UnityEngine;

public class RandomAnimation : StateMachineBehaviour
{
	public string parameter;

	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.SetInteger(parameter, 0);
	}
}
