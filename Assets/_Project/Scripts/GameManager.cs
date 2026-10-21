using UnityEngine;
using StarterAssets;

public class GameManager : MonoBehaviour
{
    public int score;
    public bool HasEnded { get; private set; }
    private ThirdPersonController player;
    private AudioPlaybackHandle windHandle;

    private void OnEnable()
    {
        player = Object.FindAnyObjectByType<ThirdPersonController>();
        if (player == null)
        {
            Debug.LogWarning("GameManager cannot connect movement audio without a ThirdPersonController.", this);
            return;
        }

        player.Footstepped += PlayFootstep;
        player.Jumped += PlayJump;
        player.Landed += PlayLanding;
    }

    void Start()
    {
        if (!HasEnded)
        {
            windHandle = AudioEvents.PlayControlled(new AudioRequest(AudioID.AmbientWind));
        }
    }

    private void OnDisable()
    {
        if (player != null)
        {
            player.Footstepped -= PlayFootstep;
            player.Jumped -= PlayJump;
            player.Landed -= PlayLanding;
        }

        AudioEvents.Stop(windHandle, allowFadeout: false);
    }

    private void PlayFootstep()
    {
        PlayMovementSound(AudioID.PlayerFootsteps);
    }

    private void PlayJump()
    {
        PlayMovementSound(AudioID.PlayerJump);
    }

    private void PlayLanding()
    {
        PlayMovementSound(AudioID.PlayerLand);
    }

    private void PlayMovementSound(AudioID id)
    {
        if (!HasEnded && player != null)
        {
            AudioEvents.Play(new AudioRequest(id, player.transform.position));
        }
    }

    public void CollectCoin(Vector3 position, bool isFalseCoin = false)
    {
        if (HasEnded)
        {
            return;
        }

        AudioEvents.Play(new AudioRequest(
            isFalseCoin ? AudioID.FalseCoinPickup : AudioID.CoinPickup, position));
        if (isFalseCoin)
        {
            GameOver();
        }
        else
        {
            AddScore(1);
        }
    }

    public void AddScore(int amount)
    {
        if (HasEnded)
        {
            return;
        }

        score += amount;
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("Cannot update music parameter because there is no active AudioManager.", this);
        }
        else
        {
            AudioEvents.SetParameter(
                AudioManager.Instance.CurrentMusic,
                "Volume",
                Mathf.Clamp01(score / 4f)
            );
        }

        if (score >=4)
        {
            Win(); 
        }
    }
    public void GameOver()
    {
        if (HasEnded)
        {
            return;
        }

        HasEnded = true;
        Debug.Log("Game Over!");
        AudioEvents.Play(new AudioRequest(AudioID.GameLose));
        StopSceneMusic();
    }
    void Win()
    {
        if (HasEnded)
        {
            return;
        }

        HasEnded = true;
        Debug.Log("You win!");
        AudioEvents.Play(new AudioRequest(AudioID.GameWin));
        StopSceneMusic();
    }

    private void StopSceneMusic()
    {
        AudioEvents.Stop(windHandle, allowFadeout: false);
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("Cannot stop scene music because there is no active AudioManager.", this);
            return;
        }

        AudioManager.Instance.StopMusic();
    }
}
