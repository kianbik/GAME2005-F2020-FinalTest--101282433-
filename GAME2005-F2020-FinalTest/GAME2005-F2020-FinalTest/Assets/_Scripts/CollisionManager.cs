using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CollisionManager : MonoBehaviour
{
    public CubeBehaviour[] cubes;
    public BulletBehaviour[] spheres;
    public PlayerBehaviour player;
    private static Vector3[] faces;

    // Start is called before the first frame update
    void Start()
    {
        cubes = FindObjectsOfType<CubeBehaviour>();

        faces = new Vector3[]
        {
            Vector3.left, Vector3.right,
            Vector3.down, Vector3.up,
            Vector3.back , Vector3.forward
        };
    }

    // Update is called once per frame
    void Update()
    {
        spheres = FindObjectsOfType<BulletBehaviour>();

        // check each AABB with every other AABB in the scene
        for (int i = 0; i < cubes.Length; i++)
        {
            for (int j = 0; j < cubes.Length; j++)
            {
                if (i != j)
                {
                    CheckAABBs(cubes[i], cubes[j]);
                }
            }
        }

        // Check each sphere against each AABB in the scene
        foreach (var sphere in spheres)
        {
            foreach (var cube in cubes)
            {
                if (cube.name != "Player")
                {
                    CheckSphereAABB(sphere, cube);
                }

            }
        }


    }

    public static void CheckSphereAABB(BulletBehaviour s, CubeBehaviour b)
    {
        // get box closest point to sphere center by clamping
        var x = Mathf.Max(b.min.x, Mathf.Min(s.transform.position.x, b.max.x));
        var y = Mathf.Max(b.min.y, Mathf.Min(s.transform.position.y, b.max.y));
        var z = Mathf.Max(b.min.z, Mathf.Min(s.transform.position.z, b.max.z));

        var distance = Math.Sqrt((x - s.transform.position.x) * (x - s.transform.position.x) +
                                 (y - s.transform.position.y) * (y - s.transform.position.y) +
                                 (z - s.transform.position.z) * (z - s.transform.position.z));

        if ((distance < s.radius) && (!s.isColliding))
        {
            // determine the distances between the contact extents
            float[] distances = {
                (b.max.x - s.transform.position.x),
                (s.transform.position.x - b.min.x),
                (b.max.y - s.transform.position.y),
                (s.transform.position.y - b.min.y),
                (b.max.z - s.transform.position.z),
                (s.transform.position.z - b.min.z)
            };

            float penetration = float.MaxValue;
            Vector3 face = Vector3.zero;

            // check each face to see if it is the one that connected
            for (int i = 0; i < 6; i++)
            {
                if (distances[i] < penetration)
                {
                    // determine the penetration distance
                    penetration = distances[i];
                    face = faces[i];
                }
            }

            s.penetration = penetration;
            s.collisionNormal = face;
            //s.isColliding = true;


            Reflect(s);
        }

    }

    // This helper function reflects the bullet when it hits an AABB face
    private static void Reflect(BulletBehaviour s)
    {
        if ((s.collisionNormal == Vector3.forward) || (s.collisionNormal == Vector3.back))
        {
            s.direction = new Vector3(s.direction.x, s.direction.y, -s.direction.z);
        }
        else if ((s.collisionNormal == Vector3.right) || (s.collisionNormal == Vector3.left))
        {
            s.direction = new Vector3(-s.direction.x, s.direction.y, s.direction.z);
        }
        else if ((s.collisionNormal == Vector3.up) || (s.collisionNormal == Vector3.down))
        {
            s.direction = new Vector3(s.direction.x, -s.direction.y, s.direction.z);
        }
    }


    public static void CheckAABBs(CubeBehaviour p, CubeBehaviour b)
    {
        Contact contactB = new Contact(b);

        if ((p.min.x <= b.max.x && p.max.x >= b.min.x) &&
            (p.min.y <= b.max.y && p.max.y >= b.min.y) &&
            (p.min.z <= b.max.z && p.max.z >= b.min.z))
        {
            // determine the distances between the contact extents
            float[] distances = {
                (b.max.x - p.min.x),
                (p.max.x - b.min.x),
                (b.max.y - p.min.y),
                (p.max.y - b.min.y),
                (b.max.z - p.min.z),
                (p.max.z - b.min.z)
            };

            float penetration = float.MaxValue;
            Vector3 face = Vector3.zero;

            // check each face to see if it is the one that connected
            for (int i = 0; i < 6; i++)
            {
                if (distances[i] < penetration)
                {
                    // determine the penetration distance
                    penetration = distances[i];
                    face = faces[i];
                }
            }

            // set the contact properties
            contactB.face = face;
            contactB.penetration = penetration;


            // check if contact does not exist
            if (!p.contacts.Contains(contactB))
            {
          // remove any contact that matches the name but not other parameters
          for (int i = p.contacts.Count - 1; i > -1; i--)
          {
              if (p.contacts[i].cube.name.Equals(contactB.cube.name))
              {
                  p.contacts.RemoveAt(i);
              }
          }

          if (contactB.face == Vector3.down)
          {
              p.gameObject.GetComponent<RigidBody3D>().Stop();
              p.isGrounded = true;
          }

          else
          {
              if (b.gameObject.GetComponent<RigidBody3D>().bodyType == BodyType.DYNAMIC)
              {

         if (contactB.face == Vector3.forward)
         {
             b.transform.position = new Vector3(b.transform.position.x, b.transform.position.y, b.transform.position.z + contactB.penetration);
             b.isGrounded = true;
             p.isGrounded = true;
             p.isColliding = true;

         }
         if (contactB.face == Vector3.back)
         {
             b.transform.position = new Vector3(b.transform.position.x, b.transform.position.y, b.transform.position.z - contactB.penetration);
             b.isGrounded = true;
             p.isGrounded = true;
             p.isColliding = true;
         }
         if (contactB.face == Vector3.right)
         {
             b.transform.position = new Vector3(b.transform.position.x + contactB.penetration, b.transform.position.y, b.transform.position.z);
             b.isGrounded = true;
             p.isGrounded = true;
             p.isColliding = true;
         }
         if (contactB.face == Vector3.left)
         {
             b.transform.position = new Vector3(b.transform.position.x - contactB.penetration, b.transform.position.y, b.transform.position.z);

             b.isGrounded = true;
             p.isGrounded = true;
             p.isColliding = true;
         }
                            }

                }
               
     
                p.isColliding = true;
               
            }
        }
        else
        {

            if (p.contacts.Exists(x => x.cube.gameObject.name == b.gameObject.name))
            {
                p.contacts.Remove(p.contacts.Find(x => x.cube.gameObject.name.Equals(b.gameObject.name)));
                p.isColliding = false;
               
                if (p.gameObject.GetComponent<RigidBody3D>().bodyType == BodyType.DYNAMIC)
                {
                    p.gameObject.GetComponent<RigidBody3D>().isFalling = true;
                    p.isGrounded = false;
                }
            }
        }
    }



}