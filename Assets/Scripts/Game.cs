using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum GameStatus
{
    Waiting_on_first_card,
    waiting_on_second_card,
    match_found,
    no_match_found
}

public class Game : MonoBehaviour
{
    [SerializeField] private int columns = 4;
    [SerializeField] private int rows = 3;

    [SerializeField] private float totalPairs;
    [SerializeField] private string frontsidesFolder = "Sprites/frontsides";
    [SerializeField] private string backsidesFolder = "Sprites/backsides";

    [SerializeField] private Sprite[] frontSprites;
    [SerializeField] private Sprite[] backSprites;
    [SerializeField] private Sprite selectedBackSprite;
    [SerializeField] private List<Sprite> selectedFrontSprites;

    [SerializeField] private GameObject cardPrefabs;
    private Stack<GameObject> stackOfCards;
    private GameObject[,] placedCards;

    [SerializeField] private Transform fieldAnchor;
    [SerializeField] private float offsetX;
    [SerializeField] private float offsetY;

    private GameStatus status;
    public GameObject[] selectedCards;
    [SerializeField] private float timeoutTimer;
    [SerializeField] private float timeoutTarget;

    private void Start()
    {
        MakeCards();
        DistibuteCards();

        selectedCards = new GameObject[2];
        status = GameStatus.Waiting_on_first_card;
    }

    private void Update()
    {
        if (status == GameStatus.match_found || status == GameStatus.no_match_found)
        {
            RotateBackOrRemovePair();
        }
    }

    private void MakeCards()
    {
        CalculateAmountOfPairs();
        LoadSprites();
        SelectFrontSprite();
        SelectBackSprite();
        ConstructCards();
    }

    private void DistibuteCards()
    {
        placedCards = new GameObject[columns, rows];
        ShuffleCards();
        PlaceCardsOnField();
    }

    private void CalculateAmountOfPairs()
    {
        if ((rows * columns) % 2 == 0)
        {
            totalPairs = (rows * columns) / 2;
        }
        else
        {
            Debug.LogError("The number of cards is odd; cannot form pairs.");
        }
    }

    private void LoadSprites()
    {
        frontSprites = Resources.LoadAll<Sprite>(frontsidesFolder);
        backSprites = Resources.LoadAll<Sprite>(backsidesFolder);

        if (frontSprites.Length == 0 || backSprites.Length == 0)
        {
            Debug.LogError("Sprites could not be loaded. Check folder paths.");
        }
    }

    private void SelectBackSprite()
    {
        if (backSprites.Length > 0)
        {
            int rnd = Random.Range(0, backSprites.Length);
            selectedBackSprite = backSprites[rnd];
        }
        else
        {
            Debug.LogError("No backside sprites available to select.");
        }
    }

    private void SelectFrontSprite()
    {
        selectedFrontSprites = new List<Sprite>();

        if (frontSprites.Length < totalPairs)
        {
            Debug.LogError("Not enough unique front sprites to create " + totalPairs + " pairs.");
            return;
        }

        while (selectedFrontSprites.Count < totalPairs)
        {
            int rnd = Random.Range(0, frontSprites.Length);

            if (!selectedFrontSprites.Contains(frontSprites[rnd]))
            {
                selectedFrontSprites.Add(frontSprites[rnd]);
            }
        }
    }

    private void ConstructCards()
    {
        stackOfCards = new Stack<GameObject>();

        foreach (Sprite selectedFrontSprite in selectedFrontSprites)
        {
            for (int i = 0; i < 2; i++)
            {
                GameObject go = Instantiate(cardPrefabs);
                Card cardScript = go.GetComponent<Card>();

                cardScript.SetBack(selectedBackSprite);
                cardScript.SetFront(selectedFrontSprite);
                go.name = selectedFrontSprite.name;

                stackOfCards.Push(go);
            }
        }
    }

    private void ShuffleCards()
    {
        while (stackOfCards.Count > 0)
        {
            int randX = Random.Range(0, columns);
            int randY = Random.Range(0, rows);

            if (placedCards[randX, randY] == null)
            {
                placedCards[randX, randY] = stackOfCards.Pop();
            }
        }
    }

    private void PlaceCardsOnField()
    {
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                GameObject card = placedCards[x, y];
                Card cardScript = card.GetComponent<Card>();

                Vector2 cardSize = cardScript.GetBackSize();
                float posX = fieldAnchor.position.x + (x * (cardSize.x + offsetX));
                float posY = fieldAnchor.position.y + (y * (cardSize.y + offsetY));
                placedCards[x, y].transform.position = new Vector3(posX, posY, 0f);
            }
        }
    }

    public void SelectCard(GameObject card)
    {
        if (status == GameStatus.Waiting_on_first_card)
        {
            selectedCards[0] = card;
            status = GameStatus.waiting_on_second_card;
        }
        else if (status == GameStatus.waiting_on_second_card)
        {
            selectedCards[1] = card;
            CheckForMatchingPair();
        }
    }

    private void CheckForMatchingPair()
    {
        timeoutTimer = 0f;
        if (selectedCards[0].name == selectedCards[1].name)
        {
            status = GameStatus.match_found;
        }
        else
        {
            status = GameStatus.no_match_found;
        }
    }

    private void RotateBackOrRemovePair()
    {
        timeoutTimer += Time.deltaTime;

        if (timeoutTimer > timeoutTarget)
        {
            if (status == GameStatus.match_found)
            {
                selectedCards[0].SetActive(false);
                selectedCards[1].SetActive(false);
            }
            else if (status == GameStatus.no_match_found)
            {
                selectedCards[0].GetComponent<Card>().TurnToBack();
                selectedCards[1].GetComponent<Card>().TurnToBack();
            }

            selectedCards[0] = null;
            selectedCards[1] = null;
            status = GameStatus.Waiting_on_first_card;
            timeoutTimer = 0f;
        }
    }

    public bool AllowedToSelectCard(Card card)
    {
        return selectedCards[0] == null || (selectedCards[1] == null && selectedCards[0] != card.gameObject);
    }
}
