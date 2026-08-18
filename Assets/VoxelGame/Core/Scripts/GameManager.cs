using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

using VoxelGame.Input;
using VoxelGame.Voxel;

namespace VoxelGame.Core
{
    public class GameManager : MonoBehaviour
    {
        private bool isGame;

        [Inject] private readonly ChunkSystem chunkSystem;
        [Inject] private readonly GameMenu gameMenu;
        [Inject] private readonly Inputs inputs;

        [SerializeField] private GameObject player;

        private void Start()
        {
            isGame = true;

            inputs.PlayerInputs.Escape.OnStarted.AddListener(() => SetGameState(!isGame));
            inputs.PlayerInputs.Inventory.OnStarted.AddListener(SwitchInventory);

            gameMenu.pause.btnContinue.OnClick.AddListener(() => SetGameState(true));
            gameMenu.pause.btnSettings.OnClick.AddListener(() =>
            {
                gameMenu.pause.Open(false);
                gameMenu.settings.Open(true);
            });
            gameMenu.pause.btnExit.OnClick.AddListener(() => LoadSceneMenu());

            gameMenu.settings.OnClosed.AddListener(() =>
            {
                if (!isGame)
                    gameMenu.pause.Open(true);
            });

            if (player)
                StartCoroutine(PlacePlayer());
        }

        IEnumerator PlacePlayer()
        {
            bool findPlace = false;

            yield return new WaitForSeconds(0.25f);

            float maxDelay = 5f;

            while (!findPlace && maxDelay > 0f)
            {

                var chunk = chunkSystem.GetChunk(Vector2Int.zero);

                if (chunk == null)
                {
                    yield return new WaitForSeconds(0.25f);
                }
                else
                {
                    for (int y = VoxelData.chunkHeight - 2; y > 0; y--)
                    {
                        Vector3Int pos = new Vector3Int(8, y, 8);
                        if (chunk.GetVoxel(pos) > 0)
                        {
                            Vector3Int prevPos = new Vector3Int(pos.x, pos.y + 1, pos.z);
                            Vector3 voxelCenterOffset = Vector3.one * 0.5f;
                            Vector3 playerOffset = new Vector3(0f, 1f, 0f);
                            player.transform.position = prevPos + voxelCenterOffset + playerOffset;
                            findPlace = true;
                            yield return new WaitForSeconds(1f);
                            break;
                        }
                    }
                }

                maxDelay -= Time.deltaTime;
            }

            player.SetActive(true);
        }

        private void LoadSceneMenu()
        {
            gameMenu.fader.Fade(true, () =>
            {
                inputs.PlayerInputs.inputLocker.IsEnabled = true;
                SceneManager.LoadSceneAsync("menu");
            });
        }

        public void SetGameState(bool isGame)
        {
            this.isGame = isGame;

            inputs.gameState = isGame ? GameState.Game : GameState.Menu;
            inputs.PlayerInputs.inputLocker.IsEnabled = !isGame;

            gameMenu.pause.Open(!isGame);

            if (isGame)
                gameMenu.settings.Open(false);

            gameMenu.inventory.Open(false);
        }

        public void SwitchInventory()
        {
            if (isGame)
            {
                inputs.PlayerInputs.inputLocker.IsEnabled = gameMenu.inventory.SwithOpen();
                inputs.gameState = inputs.PlayerInputs.inputLocker.IsEnabled ? GameState.Menu : GameState.Game;
            }
        }
    }
}