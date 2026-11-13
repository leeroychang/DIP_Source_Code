using UnityEngine;

public class SaveChildScript : MonoBehaviour
{
    [SerializeField] GameObject Child;

    void OnDestroy()
    {
        Child.transform.SetParent(null);
        if (Child.TryGetComponent(out ParticleSystem particle))
        {
            particle.Stop(true,ParticleSystemStopBehavior.StopEmitting);
            var main = particle.main;
            main.stopAction = ParticleSystemStopAction.Destroy;
        }
    }
}
