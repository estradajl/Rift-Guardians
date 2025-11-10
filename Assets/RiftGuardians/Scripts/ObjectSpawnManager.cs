using UnityEngine;
using DG.Tweening;

public class ObjectSpawnManager : MonoBehaviour
{
    public GameObject[] interactiveObjects;
    public Transform center;
    public float radius = 1.5f;
    public float delayBetweenSpawns = 0.25f;

    public void SpawnCenterObjects()
    {
        Sequence seq = DOTween.Sequence();

        foreach (var obj in interactiveObjects)
        {
            Vector3 spawnPos = center.position + Random.insideUnitSphere * radius;
            spawnPos.y = center.position.y;

            var inst = Instantiate(obj, spawnPos, Quaternion.identity);
            inst.transform.localScale = Vector3.zero;

            seq.Append(inst.transform.DOScale(1f, 0.6f)
                .SetEase(Ease.OutBack)
                .SetDelay(delayBetweenSpawns));
        }

        seq.Play();
    }
}
