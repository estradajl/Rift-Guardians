using UnityEngine;
using DG.Tweening;

public class CeilingEventController : MonoBehaviour
{
    public GameObject crackPrefab;
    public GameObject entityPrefab;
    public Transform ceilingPoint;
    public float dropDistance = 2f;

    public void SpawnCrackAndEntity()
    {
        // Grieta inicial
        var crack = Instantiate(crackPrefab, ceilingPoint.position, Quaternion.identity);
        var crackFX = crack.transform;
        crackFX.localScale = Vector3.zero;

        crackFX.DOScale(1f, 0.8f).SetEase(Ease.OutBack);

        // Entidad desciende desde el techo
        var entity = Instantiate(entityPrefab, ceilingPoint.position, Quaternion.identity);
        entity.transform.localScale = Vector3.zero;

        Sequence entitySeq = DOTween.Sequence();
        entitySeq.AppendInterval(0.8f);
        entitySeq.Append(entity.transform.DOScale(1f, 1f).SetEase(Ease.OutElastic));
        entitySeq.Join(entity.transform.DOMoveY(ceilingPoint.position.y - dropDistance, 1.5f)
            .SetEase(Ease.InOutQuad));

        entitySeq.Play();
    }
}
