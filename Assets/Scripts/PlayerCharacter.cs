using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    public BoxCollider2D boxCollider2D;
    public Rigidbody2D rb;
    [Space]
    public bool dead = false;
    [Space]
    public bool grounded = false;
    public float raytraceDist = 5f;
    public string onTopOf = "";
    [Space]
    public bool canWallJump = false;
    public float wallRayDist = 0.05f;
    public string nextTo = "";
    [Space]
    public float jumpForce;
    public float movementForce;

    Vector3 rayLeftOffset;
    Vector3 rayCenterOffset;
    Vector3 rayRightOffset;

    Transform normalParent;

    // Start is called before the first frame update
    void Start()
    {
        rayLeftOffset = new Vector3(-boxCollider2D.bounds.extents.x, -boxCollider2D.bounds.extents.y * 1.1f, 0f);
        rayCenterOffset = new Vector3(0f, -boxCollider2D.bounds.extents.y * 1.1f, 0f);
        rayRightOffset = new Vector3(boxCollider2D.bounds.extents.x, -boxCollider2D.bounds.extents.y * 1.1f, 0f);
        normalParent = this.transform.parent;
    }

    // Update is called once per frame
    void Update()
    {
        nextTo = "";
        onTopOf = "";
        grounded = CheckGrounded();
        canWallJump = CheckWallJump(Input.GetAxisRaw("Horizontal"));
        this.transform.SetParent(normalParent);

        if (Input.GetButtonDown("Jump"))
        {
            if (grounded)
            {
                rb.AddForce(Vector2.up * jumpForce);
            }
            else if (canWallJump)
            {
                rb.AddForce(Vector2.up * jumpForce);
            }
        }

        if (canWallJump && rb.velocity.y < 0f)
        {
            rb.gravityScale = 0f;
            rb.velocity = new Vector2();
        }
        else
        {
            rb.gravityScale = 1f;
        }

        rb.AddForce(Vector2.right * Input.GetAxisRaw("Horizontal") * movementForce);
    }

    bool CheckWallJump(float direction)
    {
        if (direction == 0f)
        {
            return false;
        }

        RaycastHit2D wallCheck = Physics2D.Raycast(new Vector2(this.transform.localPosition.x + (boxCollider2D.bounds.extents.x * direction) * 1.1f, this.transform.localPosition.y),
            Vector2.right * direction, wallRayDist);

        Debug.DrawLine(new Vector3(this.transform.localPosition.x + (boxCollider2D.bounds.extents.x * direction) * 1.1f, this.transform.localPosition.y, 0f),
            new Vector3(this.transform.localPosition.x + (boxCollider2D.bounds.extents.x * direction) + (wallRayDist * direction), this.transform.localPosition.y, 0f), Color.red);

        if (wallCheck.collider != null)
        {
            nextTo = wallCheck.transform.name;
            if (wallCheck.transform.tag == "Platform")
            {
                return true;
            }
        }
        return false;
    }

    bool CheckGrounded()
    {
        RaycastHit2D hitLeft = Physics2D.Raycast(new Vector2(rayLeftOffset.x + this.transform.localPosition.x, rayLeftOffset.y + this.transform.localPosition.y), -Vector2.up, raytraceDist);
        Debug.DrawLine(rayLeftOffset + this.transform.localPosition, rayLeftOffset + this.transform.localPosition + (-Vector3.up * raytraceDist), Color.green);
        if (hitLeft.collider != null)
        {
            onTopOf = hitLeft.transform.name + "| " + hitLeft.transform.tag;
            if (hitLeft.transform.tag == "Platform")
            {
                return true;
            }
        }

        RaycastHit2D hitCenter = Physics2D.Raycast(new Vector2(rayCenterOffset.x + this.transform.localPosition.x, rayCenterOffset.y + this.transform.localPosition.y), -Vector2.up, raytraceDist);
        Debug.DrawLine(rayCenterOffset + this.transform.localPosition, rayCenterOffset + this.transform.localPosition + (-Vector3.up * raytraceDist), Color.green);
        if (hitCenter.collider != null)
        {
            if (hitCenter.transform.tag == "Platform")
            {
                return true;
            }
        }

        RaycastHit2D hitRight = Physics2D.Raycast(new Vector2(rayRightOffset.x + this.transform.localPosition.x, rayRightOffset.y + this.transform.localPosition.y), -Vector2.up, raytraceDist);
        Debug.DrawLine(rayRightOffset + this.transform.localPosition, rayRightOffset+ this.transform.localPosition + (-Vector3.up * raytraceDist), Color.green);
        if (hitRight.collider != null)
        {
            if (hitRight.transform.tag == "Platform")
            {
                return true;
            }
        }

        return false;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.transform.tag == "Death")
        {
            dead = true;
        }
        else if (col.transform.tag == "Platform")
        {
            this.transform.SetParent(col.transform);
        }
    }
}
