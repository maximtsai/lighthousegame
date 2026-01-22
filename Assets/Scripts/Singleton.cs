using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
	private static T instance;
	public static T Instance => instance;

	protected virtual void Awake()
	{
		if (instance != null && instance != this)
		{
			Debug.Log("Destroyed existing singleton " + typeof(T).Name);
			Destroy(gameObject);
			return;
		}

		instance = (T)(object)this;
		DontDestroyOnLoad(gameObject);
	}
}