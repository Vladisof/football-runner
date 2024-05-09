using UnityEngine;
using System.Collections.Generic;

public class PoolObj
{
	private readonly Stack<GameObject> _mFreeInstances;
	private readonly GameObject _mOriginal;

	public PoolObj(GameObject original, int initialSize)
	{
		_mOriginal = original;
		_mFreeInstances = new Stack<GameObject>(initialSize);

		for (int i = 0; i < initialSize; ++i)
		{
			GameObject obj = Object.Instantiate(original);
			obj.SetActive(false);
            _mFreeInstances.Push(obj);
		}
	}

	public GameObject Get(Vector3 pos, Quaternion eQuad)
	{
	    GameObject ret = _mFreeInstances.Count > 0 ? _mFreeInstances.Pop() : Object.Instantiate(_mOriginal);

		ret.SetActive(true);
		ret.transform.position = pos;
		ret.transform.rotation = eQuad;

		return ret;
	}

	public void Free(GameObject obj)
	{
		obj.transform.SetParent(null);
		obj.SetActive(false);
		_mFreeInstances.Push(obj);
	}
}
