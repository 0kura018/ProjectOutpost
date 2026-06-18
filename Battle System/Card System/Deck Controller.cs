using System.Collections.Generic;
using UnityEngine;

namespace BattleSystem.CardSystem
{

    public class DeckController : MonoBehaviour
    {
        public static DeckController Instance { get; private set; }

        [Header("Config")]
        [SerializeField] private DeckConfig deckConfig;

        [Header("Settings")]
        [SerializeField] private int maxHandSize = 5;

        [Header("Draw Settings")]
        [Tooltip("����� ����� ������������� �����, ����� ������� ���������� �����")]
        [SerializeField] private float drawDelayAfterUse = 2f;

        [Header("UI")]
        [SerializeField] private Transform handContainer;
        [SerializeField] private UICardPrefab cardPrefab;

        private List<CardInstance> drawPile = new();
        private List<CardInstance> hand = new();
        private List<CardInstance> discardPile = new();
        private List<UICardPrefab> handUI = new();

        private float _drawTimer;
        private bool _waitingToDraw;

        public IReadOnlyList<CardInstance> Hand => hand;
        public int DrawPileCount => drawPile.Count;
        public int DiscardPileCount => discardPile.Count;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            InitializeDeck();

        }

        private void Update()
        {
            if (_waitingToDraw)
            {

                _drawTimer -= Time.unscaledDeltaTime;

                if (_drawTimer <= 0f)
                {
                    _waitingToDraw = false;
                    DrawToFullHand();
                }
            }
        }

        public void InitializeDeck()
        {
            drawPile.Clear();
            hand.Clear();
            discardPile.Clear();

            foreach (var config in deckConfig.GetAllCards())
            {
                drawPile.Add(new CardInstance(config));
            }

            Shuffle();
        }

        public void Shuffle()
        {
            for (int i = drawPile.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (drawPile[i], drawPile[j]) = (drawPile[j], drawPile[i]);
            }
        }

        public CardInstance DrawCard()
        {
            if (hand.Count >= maxHandSize) return null;

            if (drawPile.Count == 0)
            {
                if (discardPile.Count == 0) return null;
                ReshuffleDiscardPile();
            }

            var card = drawPile[^1];
            drawPile.RemoveAt(drawPile.Count - 1);
            hand.Add(card);

            SpawnCardUI(card);
            return card;
        }

        public void DrawToFullHand()
        {
            while (hand.Count < maxHandSize && (drawPile.Count > 0 || discardPile.Count > 0))
            {
                DrawCard();
            }
        }

        public void DiscardCard(CardInstance card)
        {
            if (!hand.Remove(card)) return;

            card.ResetModifiers();
            discardPile.Add(card);

            RemoveCardUI(card);

            StartDrawTimer();
        }

        public void DiscardHand()
        {
            while (hand.Count > 0)
            {
                DiscardCard(hand[^1]);
            }
        }

        private void StartDrawTimer()
        {
            _drawTimer = drawDelayAfterUse;
            _waitingToDraw = true;
        }

        public void CancelDrawTimer()
        {
            _waitingToDraw = false;
            _drawTimer = 0f;
        }

        private void ReshuffleDiscardPile()
        {
            drawPile.AddRange(discardPile);
            discardPile.Clear();
            Shuffle();
        }

        private void SpawnCardUI(CardInstance card)
        {
            if (cardPrefab == null || handContainer == null) return;

            var ui = Instantiate(cardPrefab, handContainer);
            ui.Bind(card);
            handUI.Add(ui);
        }

        private void RemoveCardUI(CardInstance card)
        {
            var ui = handUI.Find(x => x.BoundCard == card);
            if (ui != null)
            {
                handUI.Remove(ui);
                Destroy(ui.gameObject);
            }
        }

        public void AddCardToDeck(CardConfig config, int copies = 1)
        {
            if (config == null) return;

            for (int i = 0; i < copies; i++)
            {
                drawPile.Add(new CardInstance(config));
            }

            Debug.Log($"[DeckController] Added {copies}x '{config.CardName}' to deck");
        }

        public void AddCardAndShuffle(CardConfig config, int copies = 1)
        {
            AddCardToDeck(config, copies);
            Shuffle();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}