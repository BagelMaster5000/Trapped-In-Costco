using UnityEngine;

public class AudioController : MonoBehaviour
{
    [Header("Game World")]
    [SerializeField] AudioClip[] itemGrab;
    AudioSource[] itemGrabSources;
    void PlayGrab(string dummy) => itemGrabSources[Random.Range(0, itemGrabSources.Length)].Play();
    [SerializeField] AudioClip[] itemPocket;
    AudioSource[] itemPocketSources;
    void PlayPocket() => itemPocketSources[Random.Range(0, itemPocketSources.Length)].Play();
    [SerializeField] AudioClip[] itemSmash;
    AudioSource[] itemSmashSources;
    void PlaySmash() => itemSmashSources[Random.Range(0, itemSmashSources.Length)].Play();
    [SerializeField] AudioClip[] clap;
    AudioSource[] clapSources;
    void PlayClap() => clapSources[Random.Range(0, clapSources.Length)].Play();
    [SerializeField] AudioClip[] angry;
    AudioSource[] angrySources;
    void PlayAngry() => angrySources[Random.Range(0, angrySources.Length)].Play();

    [SerializeField] AudioClip[] footsteps;
    AudioSource[] footstepsSources;
    void PlayFootsteps() => footstepsSources[Random.Range(0, footstepsSources.Length)].Play();
    [SerializeField] AudioClip[] quipGrunt;
    AudioSource[] quipGruntSources;
    void PlayQuipGrunt() => quipGruntSources[Random.Range(0, quipGruntSources.Length)].Play();
    [SerializeField] AudioClip[] sampleMunching;
    AudioSource[] sampleMunchingSources;
    void PlaySampleMunch() => sampleMunchingSources[Random.Range(0, sampleMunchingSources.Length)].Play();


    [Header("Out of Game World")]
    [SerializeField] AudioClip startGame;
    void PlayStartGame() { menuNoiseSource.clip = startGame; menuNoiseSource.Play(); }
    [SerializeField] AudioClip pauseGame;
    void PlayPauseGame() { menuNoiseSource.clip = pauseGame; menuNoiseSource.Play(); }
    [SerializeField] AudioClip unpauseGame;
    void PlayUnpauseGame() { menuNoiseSource.clip = unpauseGame; menuNoiseSource.Play(); }
    [SerializeField] AudioClip restartGame;
    void PlayRestartGame() { menuNoiseSource.clip = restartGame; menuNoiseSource.Play(); }
    AudioSource menuNoiseSource;

    [SerializeField] AudioClip gotCorrectItem;
    AudioSource gotCorrectItemSource;
    void PlayGotCorrectItem() => gotCorrectItemSource.Play();
    [SerializeField] AudioClip gotWrongItem;
    AudioSource gotWrongItemSource;
    void PlayGotWrongItem() => gotWrongItemSource.Play();

    [SerializeField] AudioClip shoppingListComplete;
    AudioSource shoppingListCompleteSource;
    void PlayShoppingListComplete() => shoppingListCompleteSource.Play();
    [SerializeField] AudioClip costcoEscaped;
    AudioSource costcoEscapedSource;
    void PlayCostcoEscaped() => costcoEscapedSource.Play();


    [Header("Music and Ambience")]
    [SerializeField] AudioClip music;
    AudioSource musicSource;
    [SerializeField] AudioClip ambience;
    AudioSource ambienceSource;

    private void Start()
    {
        CreateAudioSources();

        LinkSoundsToEvents();
    }

    private void CreateAudioSources()
    {
        itemGrabSources = new AudioSource[itemGrab.Length];
        for (int a = 0; a < itemGrabSources.Length; a++)
        {
            itemGrabSources[a] = gameObject.AddComponent<AudioSource>();
            itemGrabSources[a].clip = itemGrab[a];
        }

        itemPocketSources = new AudioSource[itemPocket.Length];
        for (int a = 0; a < itemPocketSources.Length; a++)
        {
            itemPocketSources[a] = gameObject.AddComponent<AudioSource>();
            itemPocketSources[a].clip = itemPocket[a];
        }

        itemSmashSources = new AudioSource[itemSmash.Length];
        for (int a = 0; a < itemSmashSources.Length; a++)
        {
            itemSmashSources[a] = gameObject.AddComponent<AudioSource>();
            itemSmashSources[a].clip = itemSmash[a];
        }

        clapSources = new AudioSource[clap.Length];
        for (int a = 0; a < clapSources.Length; a++)
        {
            clapSources[a] = gameObject.AddComponent<AudioSource>();
            clapSources[a].clip = clap[a];
        }

        angrySources = new AudioSource[angry.Length];
        for (int a = 0; a < angrySources.Length; a++)
        {
            angrySources[a] = gameObject.AddComponent<AudioSource>();
            angrySources[a].clip = angry[a];
        }


        footstepsSources = new AudioSource[footsteps.Length];
        for (int a = 0; a < footstepsSources.Length; a++)
        {
            footstepsSources[a] = gameObject.AddComponent<AudioSource>();
            footstepsSources[a].clip = footsteps[a];
        }

        quipGruntSources = new AudioSource[quipGrunt.Length];
        for (int a = 0; a < quipGruntSources.Length; a++)
        {
            quipGruntSources[a] = gameObject.AddComponent<AudioSource>();
            quipGruntSources[a].clip = quipGrunt[a];
        }

        sampleMunchingSources = new AudioSource[sampleMunching.Length];
        for (int a = 0; a < sampleMunchingSources.Length; a++)
        {
            sampleMunchingSources[a] = gameObject.AddComponent<AudioSource>();
            sampleMunchingSources[a].clip = sampleMunching[a];
        }


        menuNoiseSource = gameObject.AddComponent<AudioSource>();


        gotCorrectItemSource = gameObject.AddComponent<AudioSource>();
        gotCorrectItemSource.clip = gotCorrectItem;

        gotWrongItemSource = gameObject.AddComponent<AudioSource>();
        gotWrongItemSource.clip = gotWrongItem;


        shoppingListCompleteSource = gameObject.AddComponent<AudioSource>();
        shoppingListCompleteSource.clip = shoppingListComplete;

        costcoEscapedSource = gameObject.AddComponent<AudioSource>();
        costcoEscapedSource.clip = costcoEscaped;


        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.clip = music;

        ambienceSource = gameObject.AddComponent<AudioSource>();
        ambienceSource.clip = ambience;
    }

    private void LinkSoundsToEvents()
    {
        //GameController.staticReference.OnPickup += PlayGrab;
        GameController.staticReference.OnPocket += PlayPocket;
        GameController.staticReference.OnSmash += PlaySmash;
        GameController.staticReference.OnClap += PlayClap;
        GameController.staticReference.OnAngry += PlayAngry;

        GameController.staticReference.OnMove += (bool successfulMove) => { if (successfulMove) PlayFootsteps(); };
        // TODO nothing for quip
        GameController.staticReference.OnTryMoveWhileFreeSamples += PlaySampleMunch;

        GameController.staticReference.OnGameStart += PlayStartGame;
        GameController.staticReference.OnGamePause += PlayPauseGame;
        GameController.staticReference.OnGameUnpause += PlayUnpauseGame;
        GameController.staticReference.OnGameRestart += PlayRestartGame;

        GameController.staticReference.OnGotCorrectItem += PlayGotCorrectItem;
        GameController.staticReference.OnGotWrongItem += PlayGotWrongItem;

        GameController.staticReference.OnShoppingListComplete += PlayShoppingListComplete;
        GameController.staticReference.OnGameWin += PlayCostcoEscaped;

        // TODO Play music on game start
        // TODO Stop music on game win or restart
        // TODO Play ambience on start of scene
        // TODO Stop ambience restart
    }
}