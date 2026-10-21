public enum AudioID
{
    // These values are serialized by Unity. Keep existing values when adding or moving IDs.
    None = 0, // No sound assigned.

    // Player: short effects triggered by movement or animation events.
    PlayerFootsteps = 1, // Play on each grounded foot contact; stop triggering while airborne.
    PlayerJump = 2, // Play once when a jump actually begins.
    PlayerLand = 3, // Play once on returning to the ground after being airborne.

    // Optional environment sounds.
    AmbientWind = 4, // Loop during gameplay; stop when leaving gameplay.

    // Music: author as looping 2D events; switch through AudioManager.PlayMusic.
    MainMenuMusic = 5, // Main menu soundtrack; fade out when entering gameplay.
    GameplayMusic = 6, // Gameplay soundtrack; fade out when returning to the menu.

    // UI: short 2D effects, independent of the player's position.
    ButtonClick = 7, // Play when a button is activated by mouse, keyboard, or controller.
    ButtonHover = 8, // Play when the pointer enters a button.
    ButtonSelect = 9, // Play when keyboard/controller navigation focuses a button.

    // Coin platformer: these IDs describe sounds, not the collection or win/loss logic.
    CoinPickup = 10, // Play once per real coin collected, including the fourth coin.
    FalseCoinPickup = 11, // Play once on collecting the false coin, before or with the loss cue.
    GameWin = 12, // Play once when the fourth real coin is collected; not on earlier pickups.
    GameLose = 13 // Play once when the false coin causes a loss; ignore further pickups afterward.
}
