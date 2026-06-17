using Fusion;
using UnityEngine;

public class Singleton<T> : NetworkBehaviour where T : NetworkBehaviour
{
    private static T instance;
    public static T Instance
    {
        get
        {
            return instance;
        }
    }

    public override void Spawned()
    {
        base.Spawned();

        if (instance != null)
        {
            Runner.Despawn(Object);
            return;
        }
        else
        {
            instance = this as T;
        }
    }
}
