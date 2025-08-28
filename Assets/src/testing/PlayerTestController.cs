using UnityEngine;

public class PlayerTestController : MonoBehaviour
{

    Rigidbody2D rb;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Move() {
        rb.MovePosition(rb.position + new Vector2(1, 0));
    }

}
