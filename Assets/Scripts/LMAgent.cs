using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class LMAgent : Agent
{
    [SerializeField] private Transform goal;
    public Transform spawn;

    private Rigidbody rb;

    private float previousDistance;

    [Header("Movement")]
    public float moveSpeed = 4f;
    public float rotateSpeed = 120f;



    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();

        //Seteo maxStep para no volver a cambiarlo entero cada vez que quiera modificarlo desde el editor
        this.MaxStep = 1500;
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        // no necesario usando raycast
    
    }
    public override void OnEpisodeBegin()
    {
        Debug.Log("Nuevo episodio");
        // posición aleatoria
        transform.position =
           new Vector3( spawn.position.x,1,spawn.position.z) +
            new Vector3(
                Random.Range(-0.5f, 0.5f),
                0f,
                Random.Range(-0.5f, 0.5f)
            );

        // rotación aleatoria
        transform.rotation =
         Quaternion.identity;
         

        // reset físicas
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        previousDistance =
            Vector3.Distance(
                transform.position,
                goal.position
            );
    }



    public override void OnActionReceived(ActionBuffers actions)
    {
        int action = actions.DiscreteActions[0];

        switch (action)
        {
            // avanzar
            case 1:

                rb.MovePosition(
                    rb.position +
                    transform.forward *
                    moveSpeed *
                    Time.fixedDeltaTime
                );

                break;

            // izquierda
            case 2:

                transform.Rotate(
                    Vector3.up,
                    -rotateSpeed * Time.fixedDeltaTime
                );

                break;

            // derecha
            case 3:

                transform.Rotate(
                    Vector3.up,
                    rotateSpeed * Time.fixedDeltaTime
                );

                break;
        }
        // DEBUG MAXSTEP
        if (StepCount >= MaxStep - 1)
        {
            Debug.Log("Fin por MaxStep");
        }
        // distancia actual
        float currentDistance =
            Vector3.Distance(
                transform.position,
                goal.position
            );

        // recompensa acercarse
        float distanceDelta = previousDistance - currentDistance;

        // acercarse
        AddReward(distanceDelta * 0.02f);

        previousDistance = currentDistance;

        // penalización tiempo
        AddReward(-0.0001f);

        // detenerse
        if (action == 0)
        {
            rb.linearVelocity = Vector3.zero;
        }

        if (transform.position.y < -1f)
        {
            AddReward(-0.3f);
            EndEpisode();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Wall"))
        {
            AddReward(-0.02f);
            //  EndEpisode();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Objective"))
        {
            AddReward(50f);
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;

        if (Input.GetKey(KeyCode.W))
        {
            discreteActions[0] = 1;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            discreteActions[0] = 2;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            discreteActions[0] = 3;
        }
        else
        {
            discreteActions[0] = 0;
        }
    }
}