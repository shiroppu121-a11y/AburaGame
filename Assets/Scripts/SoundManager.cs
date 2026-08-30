using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField]
    private AudioSource bgmSource;

    [SerializeField]
    private AudioSource seSource;

    [Header("BGM")]
    [SerializeField]
    private AudioClip gameBGM;

    [Header("SE")]
    [SerializeField]
    private AudioClip gameStartSE;

    [SerializeField]
    private AudioClip pourSE;

    [SerializeField]
    private AudioClip stageClearSE;

    [SerializeField]
    private AudioClip drinkSE;

    [SerializeField]
    private AudioClip hintSE;

    [SerializeField]
    private AudioClip allClearSE;

    private void Awake()
    {
        if (bgmSource == null)
        {
            Debug.LogError("BGM—pAudioSource‚ª–¢“o˜^‚Å‚·");
        }

        if (seSource == null)
        {
            Debug.LogError("SE—pAudioSource‚ª–¢“o˜^‚Å‚·");
        }
    }

    public void PlayBGM()
    {
        if (bgmSource == null || gameBGM == null)
        {
            return;
        }

        bgmSource.clip = gameBGM;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource == null)
        {
            return;
        }

        bgmSource.Stop();
    }

    public void PlayGameStartSE()
    {
        PlaySE(gameStartSE);
    }

    public void PlayPourSE()
    {
        PlaySE(pourSE);
    }

    public void PlayStageClearSE()
    {
        //PlaySE(stageClearSE);
        PlaySE(drinkSE);
    }

    public void PlayHintSE()
    {
        PlaySE(hintSE);
    }

    public void PlayAllClearSE()
    {
        PlaySE(allClearSE);
    }

    private void PlaySE(AudioClip audioClip)
    {
        if (seSource == null || audioClip == null)
        {
            return;
        }

        seSource.PlayOneShot(audioClip);
    }

    public void StopSE()
    {
        if (seSource == null)
        {
            return;
        }

        seSource.Stop();
    }
}