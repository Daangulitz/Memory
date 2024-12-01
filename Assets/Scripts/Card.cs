using System;
using UnityEngine;

public enum CardStatus
{
    show_back = 0,
    Show_Front,
    rotating_to_back,
    rotating_to_front
}

public class Card : MonoBehaviour
{
    [SerializeField] CardStatus status;

    [SerializeField] float TurnTargetTime;
    private float TurnTime;
    
    private Quaternion startRotation;
    private Quaternion targetRotation;

    private float percentage;
    
    [SerializeField] SpriteRenderer FrontRenderer;
    [SerializeField] SpriteRenderer BackRenderer;
    
    public Game game;

    private void Start()
    {
        // Initialize if necessary
    }


    private void Update()
    {
        if (status == CardStatus.rotating_to_front || status == CardStatus.rotating_to_back)
        {
          
            if (percentage < 1)
            {
                TurnTime += Time.deltaTime;
                percentage = TurnTime / TurnTargetTime;

               
                transform.rotation = Quaternion.Slerp(startRotation, targetRotation, percentage);
            }
            else
            {
                // set the rotation to targetRotation
                transform.rotation = targetRotation;
                
                // looks witch side to show
                if (status == CardStatus.rotating_to_front)
                {
                    status = CardStatus.Show_Front;

                    game.SelectCard( gameObject);
                }
                else if (status == CardStatus.rotating_to_back)
                {
                    status = CardStatus.show_back;
                }
            }
        }
    }

    private void GetFrontAndBackSpriteRenderers()
    {
        foreach (Transform t in transform)
        {
            if (t.name == "Front")
            {
                FrontRenderer = t.GetComponent<SpriteRenderer>();
            } else if (t.name == "Back")
            {
                BackRenderer = t.GetComponent<SpriteRenderer>();
            }
        }
    }

    public void SetFront(Sprite sprite)
    {
        if (FrontRenderer != null)
        {
            FrontRenderer.sprite = sprite;
        }           
    }

    public void SetBack(Sprite sprite)
    {
        if (BackRenderer != null)
        {
            BackRenderer.sprite = sprite;
        }
    }
    
    private void Awake()
    {
        status = CardStatus.show_back; // auto sets back to show at begin
        GetFrontAndBackSpriteRenderers();
        game = FindObjectOfType<Game>();
    }

    public void TurnToFront()
    {
        TurnTime = 0f;  // resets turntime
        percentage = 0f; // resets percentage
        status = CardStatus.rotating_to_front;
        startRotation = transform.rotation;
        targetRotation = Quaternion.Euler(0, 180, 0); // sets witch way to rotate
    }

    public void TurnToBack()
    {
        TurnTime = 0f; // resets turntime
        percentage = 0f; // resets percentage
        status = CardStatus.rotating_to_back;
        startRotation = transform.rotation;
        targetRotation = Quaternion.Euler(0, 0, 0); // sets witch way to rotate
    }
    
    private void OnMouseUp()
    {
        if (game.AllowedToSelectCard(this) == true)
        {
            // sets witch way need to flip
            if (status == CardStatus.show_back)
            {
                game.SelectCard(gameObject);
                TurnToFront();
            }
            else if (status == CardStatus.Show_Front)
            {
                TurnToBack();
            }
        }
    }

    public Vector2 GetFrontSize()
    {
        if (FrontRenderer == null)
        {
            Debug.LogError("There is no frontrenderer found");
        }
        return FrontRenderer.bounds.size;
    }

    public Vector2 GetBackSize()
    {
        if (BackRenderer == null)
        {
            Debug.LogError("There is no backrenderer found");
        }
        return BackRenderer.bounds.size;
    }
}
