using UnityEngine;

[ExecuteInEditMode]
public class WorldCurvedWorldCurve : MonoBehaviour
{
	[Range(-0.1f, 0.1f)]
	public float curveStrength = 0.01f;

	private int _mCurveStrengthID;

    private void OnEnable()
    {
        _mCurveStrengthID = Shader.PropertyToID("_CurveStrength");
    }

    private void Update()
	{
		Shader.SetGlobalFloat(_mCurveStrengthID, curveStrength);
	}
}
