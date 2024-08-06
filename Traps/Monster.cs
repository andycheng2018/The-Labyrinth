using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.AI;

namespace AC
{
    public class Monster : NetworkBehaviour
    {
        [Header("Monster Settings")]
        public GameObject attackBox;
        public float chaseDistance;
        public float attackRange;
        public float moveSpeed;
        public float lookSpeed;

        [Header("Flying Settings")]
        public bool isFlying;
        public float lowBaseOffset;
        public float highBaseOffset;
        public float offsetTransitionSpeed;
        private float currentBaseOffset;
        private float randomBaseOffset;

        public NetworkVariable<float> health = new NetworkVariable<float>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private NetworkAnimator anim;
        private NavMeshAgent agent;
        private AudioSync audioSync;

        private void Start()
        {
            anim = GetComponent<NetworkAnimator>();
            agent = GetComponent<NavMeshAgent>();
            audioSync = GetComponent<AudioSync>();
        }

        private void Update()
        {
            GameObject closestPlayer = FindClosestPlayer();

            if (closestPlayer != null)
            {
                float distance = Vector3.Distance(transform.position, closestPlayer.transform.position);
                

                if (distance <= chaseDistance)
                {
                    if (isFlying) {
                        currentBaseOffset = Mathf.Lerp(currentBaseOffset, lowBaseOffset, offsetTransitionSpeed * Time.fixedDeltaTime);
                        agent.baseOffset = currentBaseOffset;
                    }

                    if (distance <= attackRange)
                    {
                        anim.SetTrigger("Attack");
                        agent.isStopped = true;
                        Vector3 direction = (closestPlayer.transform.position - transform.position).normalized;
                        Quaternion lookRotation = Quaternion.LookRotation(direction);
                        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * lookSpeed);       
                    }
                    else
                    {
                        anim.SetTrigger("Run");
                        audioSync.ChangePitch(1.2f);
                        agent.isStopped = false;
                        agent.SetDestination(closestPlayer.transform.position);
                    }
                }
                else
                {
                    if (isFlying) {
                        currentBaseOffset = Mathf.Lerp(currentBaseOffset, randomBaseOffset, offsetTransitionSpeed * Time.fixedDeltaTime);
                        agent.baseOffset = currentBaseOffset;
                    }
                    anim.SetTrigger("Walk");
                    audioSync.ChangePitch(1);
                    agent.isStopped = false;

                    if (!agent.pathPending && agent.remainingDistance < 0.1f)
                    {
                        SetRandomDestination();
                    }
                }
            }

            if (agent.isOnOffMeshLink)
            {
                OffMeshLinkData data = agent.currentOffMeshLinkData;
                Vector3 endPos = data.endPos + Vector3.up * agent.baseOffset;

                agent.transform.position = Vector3.MoveTowards(agent.transform.position, endPos, agent.speed * Time.deltaTime);

                if (agent.transform.position == endPos)
                {
                    agent.CompleteOffMeshLink();
                }
            }
        }

        private GameObject FindClosestPlayer()
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            GameObject closestPlayer = null;
            float closestDistance = Mathf.Infinity;
            Vector3 currentPosition = transform.position;

            foreach (GameObject player in players)
            {
                if (player != gameObject)
                {
                    float distance = Vector3.Distance(player.transform.position, currentPosition);

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestPlayer = player;
                    }
                }
            }

            return closestPlayer;
        }

        private void SetRandomDestination()
        {
            Vector3 randomPoint = RandomPointOnNavMesh();
            agent.SetDestination(randomPoint);

            if (isFlying)
                randomBaseOffset = Random.Range(lowBaseOffset, highBaseOffset);
        }

        private Vector3 RandomPointOnNavMesh()
        {
            Vector3 randomPoint = Vector3.zero;
            bool found = false;
            int attempts = 0;
            while (!found && attempts < 30) // Limiting attempts to avoid infinite loop
            {
                Vector3 randomDirection = Random.insideUnitSphere * 10;
                randomDirection += transform.position; // Ensure the point is relative to the agent's position

                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomDirection, out hit, 10f, NavMesh.AllAreas))
                {
                    randomPoint = hit.position;
                    found = true;
                }

                attempts++;
            }

            if (!found)
            {
                Debug.LogWarning("Failed to find a valid random point on the NavMesh after 30 attempts.");
            }

            return randomPoint;
        }

        public void Slap()
        {
            StartCoroutine(SlapTime());
        }

        private IEnumerator SlapTime()
        {
            audioSync.PlaySound(0);
            attackBox.GetComponent<Collider>().enabled = true;
            yield return new WaitForSeconds(0.1f);
            attackBox.GetComponent<Collider>().enabled = false;
        }

        private void OnTriggerEnter(Collider other) {
            if (other.CompareTag("Door"))
            {
                if (!other.GetComponent<Door>().isOpen.Value)
                    other.GetComponent<Door>().ChangeStateServerRpc();
            }

            if (other.tag == "Weapon")
            {
                TakeDamage(other.GetComponent<TrapDamage>().damage);
            }

            if (other.tag == "Fire") {
                other.transform.GetComponent<NetworkObject>().Despawn(true);
                audioSync.PlaySound(0);
            }

            if (other.tag == "Player") {
                if (!other.GetComponent<Player>().isFlickering)
                    StartCoroutine(other.GetComponent<Player>().FlickerLight());
            }
        }

        public void TakeDamage(float damage)
        {
            audioSync.PlaySound(1);
            TakeDamageServerRpc(damage);
            
            if (health.Value <= 0)
            {
                StartCoroutine(Despawn());
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void TakeDamageServerRpc(float damage) {
            TakeDamageClientRpc(damage);
        }

        [ClientRpc]
        private void TakeDamageClientRpc(float damage) {
            health.Value -= damage;
        }

        private IEnumerator Despawn()
        {
            this.enabled = false;
            yield return new WaitForSeconds(2f);
            gameObject.GetComponent<NetworkObject>().Despawn(true);
        }
    }
}
