using System.Collections.Generic;
using Assets.Scripts.Extensions;
using Assets.Scripts.Panels;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.Reusable
{
    public class TutorialPanelScript : MonoBehaviour, IClosablePanel
    {
        public TextMeshProUGUI TutorialText;
        public GameObject ButtonsPanel;

        private const string PrefabPath = "Prefabs/TutorialPanel";

        private readonly IDictionary<string, string> explanations = new Dictionary<string, string>
        {
            { string.Empty, string.Empty },

            {
                "ConnectButton", @"The game is played using your phone, so have it handy.
On the Main Menu, there are two ways to connect:
• Faster - Scan the QR Code
• Using your phone, go to the address on the screen. Enter the <b>Room Code</b> displayed on the Main screen and your player name as well. Press <b>“Join”</b> and you should see your avatar on the game screen.

Please note, you need at least <b>2 players</b> to start the game."
            },

            {
                "ChoosePlaylistButton", @"After everyone’s joined and ready, press <b>Start</b> from the  Main Menu. Then, you can choose any playlist that you would like to play.
Don't worry though, if you can't decide, just select <b>Random</b> and enjoy the surprise."
            },

            {
                "PlayButton", @"• Listen to the music playing and see if you can recognize the song.
• Look at the snippet presented on the game screen and enter a <b>fake</b> lyric on your phone. to fool the other players.
• Try to find the real lyric and vote it on your phone.
• All the votes are presented and points are awarded: <b>100 points</b> for every vote you receive & <b>300 points</b> for voting the correct lyric.
• The score is presented and this repeats for 5 rounds (you can change the number of rounds from the Options menu).
• The last round is a <b>speed round</b>! Enter the correct lyric fast and you receive 10 points for every second remaining on the clock.
• <b>See who wins and play again!</b>"
            },

            {
                "DoItAgainButton", @"You can play again, either with the same players or new ones. You can even select the same playlist and you might get some different songs.
You can also enable explicit lyrics and get access to even more playlists and increase or decrease the number of rounds, all from the <b>Options</b> menu.

<b>Have fun and post online your funniest moments!</b>"
            }
        };

        private string currentKey = string.Empty;

        private void Start()
        {
            ButtonsPanel.GetComponentInChildren<ButtonWithIcon>().Select();
            //this.ButtonSelected();
        }

        private void Update()
        {
            if (!isActiveAndEnabled) return;

            if (Input.GetKeyDown(KeyCode.Escape)) Destroy(gameObject);

            if (EventSystem.current.currentSelectedGameObject == null &&
                transform.IsLastChild())
                ButtonsPanel.GetComponentInChildren<ButtonWithIcon>().Select();
        }

        public void Close() => Destroy(gameObject);

        public static void Instantiate()
        {
            PanelManager.CloseAllOpenPanels();
            var parent = FindObjectOfType<Canvas>();
            var prefab = Resources.Load<GameObject>(PrefabPath);
            var instance = GameObject.Instantiate(prefab, parent.transform);
            instance.transform.SetAsLastSibling();
        }

        public void ButtonSelected()
        {
            var selectedButton = EventSystem.current.currentSelectedGameObject;
            var selectedKey = selectedButton?.name;
            if (selectedKey == null) return;

            if (currentKey == selectedKey) return;

            currentKey = selectedKey;
            if (explanations.ContainsKey(currentKey)
                && TutorialText.text != explanations[currentKey])
            {
                TutorialText.text = explanations[currentKey];
                selectedButton.GetComponent<ButtonWithIcon>().Select();
            }
        }

        public void ButtonPointerEnter(BaseEventData eventData)
        {
            if (eventData is PointerEventData pointerEventData)
            {
                var button = pointerEventData.pointerEnter.GetComponent<ButtonWithIcon>()
                             ?? pointerEventData.pointerEnter.GetComponentInParent<ButtonWithIcon>();

                button.Select();
            }
        }
    }
}