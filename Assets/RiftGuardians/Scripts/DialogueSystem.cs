using UnityEngine;
using DG.Tweening;

public class DialogueSystem : MonoBehaviour
{
    public AudioSource voiceSource;
    public AudioClip[] clips;

    public void PlayIntroDialogue(System.Action onDialogueComplete)
    {
        Sequence dialogueSeq = DOTween.Sequence();

        foreach (var clip in clips)
        {
            dialogueSeq.AppendCallback(() =>
            {
                voiceSource.clip = clip;
                voiceSource.Play();
            });

            dialogueSeq.AppendInterval(clip.length + 0.3f);
        }

        dialogueSeq.OnComplete(() =>
        {
            Debug.Log("Dialogue finished.");
            onDialogueComplete?.Invoke();
        });

        dialogueSeq.Play();
    }
}
